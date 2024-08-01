using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
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
        
        foreach (string? namespaceName in namespaces.Distinct())
        {
            sourceBuilder.Append($"using {namespaceName};\n");
        }
        
        sourceBuilder.Append(
"""
using System;
using System.Runtime.InteropServices;

namespace OrbinautFramework3.Objects.Player
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Actions(Player player)
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
                _type = value;
                switch (_type)
                {
"""
        );
        
        foreach (StructDeclarationSyntax? structDeclaration in structs)
        {
            string name = structDeclaration.Identifier.Text;
            sourceBuilder.Append($"\n\t\t\t\t\tcase Types.{name}: {name} = new {name} ");
            sourceBuilder.Append("{ Player = _player }; break;");
        }

        sourceBuilder.Append('\n').Append(
"""             
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        [FieldOffset(0)] private Types _type;
        [FieldOffset(8)] private Player _player = player;
"""    
        );
        
        foreach (StructDeclarationSyntax? structDeclaration in structs)
        {
            string name = structDeclaration.Identifier.Text;
            sourceBuilder.Append($"\n\t\t[FieldOffset(16)] public {name} {name};");
        }

        sourceBuilder.Append('\n').Append(
"""

        public void Perform()
        {
            switch (_type)
            {
"""
        );
        
        foreach (StructDeclarationSyntax? structDeclaration in structs)
        {
            string name = structDeclaration.Identifier.Text;
            sourceBuilder.Append($"\n\t\t\t\tcase Types.{name}: {name}.Perform(); break;");
        }

        sourceBuilder.Append('\n').Append(
"""
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}

"""
        );
        
        context.AddSource("Action.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
    }
}
