using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ActionSourceGenerator;

[Generator]
public class ActionGenerator : IIncrementalGenerator
{
    private const string InterfaceName = "IAction";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<StructDeclarationSyntax>> structsWithInterface = 
            context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, _) => node is StructDeclarationSyntax,
                transform: (ctx, _) => (StructDeclarationSyntax)ctx.Node)
            .Where(structDecl => structDecl.BaseList != null && structDecl.BaseList.Types
                .Any(baseType => baseType.Type is IdentifierNameSyntax { Identifier.Text: InterfaceName }))
            .Collect();

        context.RegisterSourceOutput(structsWithInterface, Build);
    }
    
    private static void Build(SourceProductionContext context, ImmutableArray<StructDeclarationSyntax> structs)
    {
        if (structs.IsDefaultOrEmpty) return;
        
        var sourceBuilder = new StringBuilder();

        List<string> namespaces = [];
        foreach (StructDeclarationSyntax? structDeclaration in structs)
        {
            switch (structDeclaration.Parent)
            {
                case NamespaceDeclarationSyntax namespaceDecl:
                    namespaces.Add(namespaceDecl.Name.ToString());
                    break;
                case FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDecl:
                    namespaces.Add(fileScopedNamespaceDecl.Name.ToString());
                    break;
            }
        }
        
        List<string> entersMethods = [];
        List<string> exitsMethods = [];
        Dictionary<string, List<string>> otherMethods = [];
        foreach (StructDeclarationSyntax? structDeclaration in structs)
        {
            string structName = structDeclaration.Identifier.Text;
            
            IEnumerable<MethodDeclarationSyntax> methods = structDeclaration.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(method => method.Modifiers.Any(SyntaxKind.PublicKeyword));
            
            
            foreach (MethodDeclarationSyntax? method in methods)
            {
                string methodName = method.Identifier.Text;
                switch (methodName)
                {
                    case "Enter": entersMethods.Add(structName); break;
                    case "Exit": exitsMethods.Add(structName); break;
                    default:
                        if (otherMethods.ContainsKey(methodName))
                        {
                            otherMethods[methodName].Add(structName);
                            break;
                        }
                        
                        otherMethods.Add(methodName, [structName]);
                        break;
                }
            }
        }
        
        foreach (string? namespaceName in namespaces.Distinct())
        {
            sourceBuilder.Append($"using {namespaceName};\n");
        }
        
        sourceBuilder.Append(
"""
using System;
using System.Runtime.InteropServices;

namespace OrbinautFramework3.Objects.Player.Data
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Actions(PlayerData data)
    {
        public enum Types : 
"""
        );
        
        sourceBuilder.Append("int").Append("\n\t\t{\n\t\t\t");

        foreach (StructDeclarationSyntax? structDeclaration in structs)
        {
            sourceBuilder.Append(structDeclaration.Identifier.Text).Append(", ");
        }

        sourceBuilder.Append('\n').Append(
"""
        }
        
        public static implicit operator Types(Actions action) => action.Type;
        
        public Types Type
        {
            get => _type;
            set
            {
"""
        );
        
        if (exitsMethods.Count > 0)
        {
            sourceBuilder.Append(
"\n                Exit();"
            );
        }
        
        sourceBuilder.Append('\n').Append(
"""
                switch (value)
                {
"""
        );
        
        foreach (StructDeclarationSyntax? structDeclaration in structs)
        {
            string name = structDeclaration.Identifier.Text;
            sourceBuilder.Append($"\n\t\t\t\t\tcase Types.{name}: {name} = new {name} ");
            sourceBuilder.Append("{ Data = _data }; break;");
        }

        sourceBuilder.Append('\n').Append(
"""             
                    default: throw new ArgumentOutOfRangeException();
                }
                _type = value;
            }
        }
        
        [FieldOffset(0)] private Types _type = Types.None;
        [FieldOffset(8)] private PlayerData _data = data;
"""    
        );
        
        foreach (StructDeclarationSyntax? structDeclaration in structs)
        {
            string name = structDeclaration.Identifier.Text;
            sourceBuilder.Append($"\n\t\t[FieldOffset(16)] public {name} {name};");
        }
        
        foreach (KeyValuePair<string, List<string>> method in otherMethods)
        {
            AddMethod(sourceBuilder, method.Key, method.Value);
        }
        
        if (exitsMethods.Count > 0)
        {
            AddMethod(sourceBuilder, "Exit", exitsMethods);
        }

        sourceBuilder.Append('\n').Append(
"""
    }
}

"""
        );
        
        context.AddSource("Action.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
    }

    private static void AddMethod(
        StringBuilder sourceBuilder, string methodName, List<string> actions, string access = "public")
    {
        sourceBuilder.Append("\n\n\t\t").Append(access).Append(" void ").Append(methodName).Append("()\n").Append(
"""
        {
            switch (_type)
            {
"""
        );
        
        foreach (string action in actions)
        {
            sourceBuilder.Append($"\n\t\t\t\tcase Types.{action}: {action}.{methodName}(); break;");
        }

        sourceBuilder.Append('\n').Append(
"""
            }
        }
"""
        );
    }
}
