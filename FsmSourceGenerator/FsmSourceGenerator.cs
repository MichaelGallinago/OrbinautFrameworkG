using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace FsmSourceGenerator;

[Generator]
public class FsmGenerator : IIncrementalGenerator
{
    private const string GeneratorNamespace = nameof(FsmSourceGenerator);
    private const string AttributeName = nameof(FsmAttribute);
    private const string StateAttributeName = nameof(FsmStateAttribute);
    private const string FullStateAttributeName = $"{GeneratorNamespace}.{StateAttributeName}";
    private const string FullAttributeName = $"{GeneratorNamespace}.{AttributeName}";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<FsmData?>> provider = 
            context.SyntaxProvider.ForAttributeWithMetadataName(
                FullAttributeName,
                static (_, _) => true,
                static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Collect();

        IncrementalValueProvider<ImmutableArray<StateData?>> statesProvider =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                    FullStateAttributeName,
                    static (node, _) => node is StructDeclarationSyntax,
                    static (ctx, _) => GetStatesTargetForGeneration(ctx))
                .Where(result => result is { FsmName: not null })
                .Collect();

        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider).Combine(statesProvider), Build);
    }
    
    private static FsmData? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        AttributeData? attribute = context.Attributes
            .FirstOrDefault(a => a.AttributeClass?.Name == AttributeName);

        if (attribute == null) return null;
        
        var name = (string)attribute.ConstructorArguments[0].Value!;
        var namespaceName = (string)attribute.ConstructorArguments[1].Value!;

        return new FsmData(name, namespaceName);
    }
    
    private static StateData? GetStatesTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        AttributeData? attribute = context.Attributes
            .FirstOrDefault(a => a.AttributeClass?.Name == StateAttributeName);
        
        if (attribute == null) return null;
        var fsmNameArgument = attribute.ConstructorArguments.First().Value as string;

        var structDeclaration = (StructDeclarationSyntax)context.TargetNode;
        return new StateData(structDeclaration, fsmNameArgument!);
    }
    
    private readonly record struct FsmData(string Name, string Namespace)
    {
        public readonly string Name = Name;
        public readonly string Namespace = Namespace;
    }
    
    private readonly record struct StateData(StructDeclarationSyntax Struct, string FsmName)
    {
        public readonly StructDeclarationSyntax Struct = Struct;
        public readonly string? FsmName = FsmName;
    }
    
    private static void Build(SourceProductionContext context, (
        (Compilation Compilation, ImmutableArray<FsmData?> Fsms) Left, ImmutableArray<StateData?> States) data)
    {
        ImmutableArray<FsmData?> fsms = data.Left.Fsms;
        
        if (fsms.IsDefaultOrEmpty) return;
        var sourceBuilder = new StringBuilder();

        foreach (FsmData? fsm in fsms)
        {
            if (fsm == null) continue;
            string name = fsm.Value.Name;
            StructDeclarationSyntax[] states = data.States
                .Where(x => x!.Value.FsmName == name)
                .Select(x => x!.Value.Struct)
                .ToArray();
            CreateFsmFile(sourceBuilder, fsm.Value, context, states, data.Left.Compilation);
        }
    }

    private static void CreateFsmFile(
        StringBuilder sourceBuilder, FsmData fsmData, 
        SourceProductionContext context, StructDeclarationSyntax[] structs, Compilation compilation)
    {
        HashSet<string> namespaces = [];
        Dictionary<string, string> constructorTypes = [];
        Dictionary<string, HashSet<string>> stateTypes = [];
        
        foreach (StructDeclarationSyntax structDeclaration in structs)
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
            
            SemanticModel semanticModel = compilation.GetSemanticModel(structDeclaration.SyntaxTree);

            if (structDeclaration.ParameterList == null) continue;
            
            SeparatedSyntaxList<ParameterSyntax> parameters = structDeclaration.ParameterList.Parameters;

            string stateName = structDeclaration.Identifier.Text;
            stateTypes.Add(structDeclaration.Identifier.Text, []);
            foreach (ParameterSyntax parameter in parameters)
            {
                IParameterSymbol? parameterSymbol = semanticModel.GetDeclaredSymbol(parameter);
                if (parameterSymbol == null) continue;
                
                stateTypes[stateName].Add(parameterSymbol.Name);
                
                if (!constructorTypes.ContainsKey(parameterSymbol.Name))
                {
                    constructorTypes.Add(parameterSymbol.Name, parameterSymbol.Type.Name);
                }
                
                string typeNamespace = parameterSymbol.Type.ContainingNamespace.ToDisplayString();
                if (!string.IsNullOrEmpty(typeNamespace))
                {
                    namespaces.Add(typeNamespace);
                }
            }
        }
        
        List<string> entersMethods = [];
        Dictionary<string, bool> exitsMethods = [];
        Dictionary<string, Dictionary<string, bool>> otherMethods = [];
        foreach (StructDeclarationSyntax structDeclaration in structs)
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
                    case "Exit":
                        SeparatedSyntaxList<ParameterSyntax> parameters = method.ParameterList.Parameters;
                        if (parameters.Count == 0)
                        {
                            exitsMethods.Add(structName, false);
                        }
                        else if (parameters.Count == 1)
                        {
                            SemanticModel semanticModel = compilation.GetSemanticModel(structDeclaration.SyntaxTree);
                            ITypeSymbol? parameterSymbol = semanticModel.GetTypeInfo(parameters.First().Type!).Type;
                            if (parameterSymbol == null) continue;
                            string name = parameterSymbol.Name;
                            if (name == "States" || name == $"{fsmData.Name}.States")
                            {
                                exitsMethods.Add(structName, true);
                            }
                        }
                        break;
                    default:
                        var typeName = method.ReturnType.ToString();
                        bool isStateChanger = typeName == "States" || typeName == $"{fsmData.Name}.States";
                        
                        if (otherMethods.ContainsKey(methodName))
                        {
                            otherMethods[methodName].Add(structName, isStateChanger);
                            break;
                        }
                        
                        otherMethods.Add(methodName, new Dictionary<string, bool> {{structName, isStateChanger}});
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

"""
        ).Append("\nnamespace ").Append(fsmData.Namespace).Append('\n').Append(
"""
{
    [StructLayout(LayoutKind.Explicit)]
"""
        ).Append("\n\tpublic struct ").Append(fsmData.Name).Append("Fsm");
        
        if (constructorTypes.Count <= 0)
        {
            sourceBuilder.Append("\n");
        }
        else
        {
            sourceBuilder.Append('(');
            foreach (KeyValuePair<string, string> constructorType in constructorTypes)
            {
                sourceBuilder.Append(constructorType.Value).Append(' ').Append(constructorType.Key).Append(", ");
            }
            sourceBuilder.Remove(sourceBuilder.Length - 2, 2);
            sourceBuilder.Append(")\n");
        }
        
        sourceBuilder.Append(
"""
    {
        public enum States : 
"""
        ).Append("int").Append("\n\t\t{\n\t\t\t");

        foreach (StructDeclarationSyntax? structDeclaration in structs)
        {
            sourceBuilder.Append(structDeclaration.Identifier.Text).Append(", ");
        }

        if (structs.All(x => x.Identifier.Text != "None"))
        {
            sourceBuilder.Append("None");
        }

        sourceBuilder.Append('\n').Append(
"""
        }
        
        public States State
        {
            get => _state;
            set
            {
"""
        );
        
        if (exitsMethods.Count > 0)
        {
            sourceBuilder.Append(
"""

                if (value == _state) return;
                Exit(value);
"""
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
            sourceBuilder.Append($"\n\t\t\t\t\tcase States.{name}: {name} = new {name}(");

            if (stateTypes.TryGetValue(name, out HashSet<string>? parameters) && parameters is { Count: > 0 })
            {
                foreach (string parameter in parameters)
                {
                    sourceBuilder.Append('_').Append(parameter).Append(", ");
                }
                sourceBuilder.Remove(sourceBuilder.Length - 2, 2);
            }
            sourceBuilder.Append(");").Append(entersMethods.Contains(name) ? $" {name}.Enter(); break;" : " break;");
        }

        sourceBuilder.Append('\n').Append(
"""             
                    default: throw new ArgumentOutOfRangeException();
                }
                _state = value;
            }
        }
        
        [FieldOffset(0)] private States _state = States.None;
"""    
        );

        const int offsetStep = 8;
        int offset = offsetStep;
        
        foreach (KeyValuePair<string, string> type in constructorTypes)
        {
            sourceBuilder.Append("\n\t\t[FieldOffset(").Append(offset).Append(")] private ").Append(type.Value)
                .Append(" _").Append(type.Key).Append(" = ").Append(type.Key).Append(';');
            offset += offsetStep;
        }
        
        foreach (StructDeclarationSyntax? structDeclaration in structs)
        {
            string name = structDeclaration.Identifier.Text;
            sourceBuilder.Append($"\n\t\t[FieldOffset({offset})] private {name} {name};");
        }
        
        foreach (KeyValuePair<string, Dictionary<string, bool>> method in otherMethods)
        {
            AddMethod(sourceBuilder, method.Key, method.Value, true);
        }
        
        if (exitsMethods.Count > 0)
        {
            AddExit(sourceBuilder, exitsMethods);
        }

        sourceBuilder.Append('\n').Append(
"""
    }
}

"""
        );
        
        context.AddSource("ActionFsm.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        sourceBuilder.Clear();
    }

    private static void AddMethod(
        StringBuilder sourceBuilder, string methodName, Dictionary<string, bool> states, bool isPublic)
    {
        string access = isPublic ? "public" : "private";
        sourceBuilder.Append("\n\n\t\t").Append(access).Append(" void ").Append(methodName).Append("()\n").Append(
"""
        {
            switch (_state)
            {
"""
        );
        
        foreach (KeyValuePair<string, bool> state in states)
        {
            sourceBuilder.Append(state.Value
                ? $"\n\t\t\t\tcase States.{state.Key}: State = {state.Key}.{methodName}(); break;"
                : $"\n\t\t\t\tcase States.{state.Key}: {state.Key}.{methodName}(); break;");
        }

        sourceBuilder.Append('\n').Append(
"""
            }
        }
"""
        );
    }
    
    private static void AddExit(StringBuilder sourceBuilder, Dictionary<string, bool> actions)
    {
        sourceBuilder.Append("\n\n\t\tprivate void Exit(States nextState)\n").Append(
            """
                    {
                        switch (_state)
                        {
            """
        );
        
        foreach (KeyValuePair<string, bool> action in actions)
        {
            string parameter = action.Value ? "nextState" : "";
            sourceBuilder.Append($"\n\t\t\t\tcase States.{action.Key}: {action.Key}.Exit({parameter}); break;");
        }

        sourceBuilder.Append('\n').Append(
            """
                        }
                    }
            """
        );
    }
}
