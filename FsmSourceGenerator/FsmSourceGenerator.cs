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
    private const string StateSwitcherAttributeName = nameof(FsmStateSwitcherAttribute);
    private const string FullStateAttributeName = $"{GeneratorNamespace}.{StateAttributeName}";
    private const string FullStateSwitcherAttributeName = $"{GeneratorNamespace}.{StateSwitcherAttributeName}";
    private const string FullAttributeName = $"{GeneratorNamespace}.{AttributeName}";
    
    private const int FieldOffsetStep = 8;
    
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
        
        IncrementalValueProvider<ImmutableArray<StateSwitcherData?>> stateSwitchersProvider =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                    FullStateSwitcherAttributeName,
                    static (node, _) => node is MethodDeclarationSyntax,
                    static (ctx, _) => GetStateSwitchersTargetForGeneration(ctx))
                .Where(result => result is { FsmName: not null })
                .Collect();

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(provider).Combine(statesProvider).Combine(stateSwitchersProvider), 
            Build);
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
    
    private static StateSwitcherData? GetStateSwitchersTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        AttributeData? attribute = context.Attributes
            .FirstOrDefault(a => a.AttributeClass?.Name == StateSwitcherAttributeName);
        
        if (attribute == null) return null;
        var fsmNameArgument = attribute.ConstructorArguments.First().Value as string;

        var methodDeclaration = (MethodDeclarationSyntax)context.TargetNode;
        return new StateSwitcherData(methodDeclaration, fsmNameArgument!);
    }

    private record struct FsmData(string Name, string Namespace);
    private record struct StateData(StructDeclarationSyntax Struct, string FsmName);
    private record struct StateSwitcherData(MethodDeclarationSyntax Method, string FsmName);
    
    private static void Build(
        SourceProductionContext context, 
        (((Compilation Compilation, 
            ImmutableArray<FsmData?> Fsms) Left, 
            ImmutableArray<StateData?> States) Left, 
            ImmutableArray<StateSwitcherData?> StateSwitchers) data)
    {
        ImmutableArray<FsmData?> fsms = data.Left.Left.Fsms;

        if (fsms.IsDefaultOrEmpty) return;
        var sourceBuilder = new StringBuilder();

        foreach (FsmData? fsm in fsms)
        {
            if (fsm == null) continue;
            string name = fsm.Value.Name;
            
            StructDeclarationSyntax[] states = data.Left.States
                .Where(x => x!.Value.FsmName == name)
                .Select(x => x!.Value.Struct)
                .ToArray();
            
            MethodDeclarationSyntax[] stateSwitchers = data.StateSwitchers
                .Where(x => x!.Value.FsmName == name)
                .Select(x => x!.Value.Method)
                .ToArray();
            
            CreateFsmFile(sourceBuilder, fsm.Value, context, states, stateSwitchers, data.Left.Left.Compilation);
        }
    }

    private static void CreateFsmFile(
        StringBuilder sourceBuilder, FsmData fsmData, 
        SourceProductionContext context, StructDeclarationSyntax[] states, 
        MethodDeclarationSyntax[] stateSwitchers, Compilation compilation)
    {
        string fsmName = fsmData.Name + "Fsm";
        
        stateSwitchers = FilterStateSwitchers(stateSwitchers, states);
        
        GetDependencyData(states, compilation, 
            out HashSet<string> namespaces,
            out Dictionary<string, string> constructorTypes, 
            out Dictionary<string, HashSet<string>> stateTypes);

        GetMethods(states, compilation, fsmName, 
            out HashSet<string> entersMethods, 
            out Dictionary<string, bool> exitsMethods, 
            out Dictionary<string, Dictionary<string, bool>> otherMethods);

        AddUsings(sourceBuilder, namespaces);
        
        sourceBuilder.Append("\nnamespace ").AppendLine(fsmData.Namespace).Append(
"""
{
    [StructLayout(LayoutKind.Explicit)]
    public struct 
""").Append(fsmName);
        
        AddFsmConstructorParameters(sourceBuilder, constructorTypes);
        
        sourceBuilder.Append(
"""
    {
        public enum States
        {
            
""");
        var index = 0;
        foreach (StructDeclarationSyntax state in states)
        {
            sourceBuilder.Append(state.Identifier.Text).Append(", ");
            if (++index != 6) continue;
            sourceBuilder.Append("\n\t\t\t");
            index = 0;
        }
        
        foreach (MethodDeclarationSyntax stateSwitcher in stateSwitchers)
        {
            sourceBuilder.Append(stateSwitcher.Identifier.Text).Append(", ");
            if (++index != 6) continue;
            sourceBuilder.Append("\n\t\t\t");
            index = 0;
        }

        if (states.All(x => x.Identifier.Text != "None") &&
            stateSwitchers.All(x => x.Identifier.Text != "None"))
        {
            sourceBuilder.Append("None");
        }

        sourceBuilder.AppendLine().Append(
"""
        }
        
        public States State
        {
            get => _state;
            set
            {
""");
        
        if (exitsMethods.Count > 0)
        {
            sourceBuilder.Append(
"""

                if (value == _state) return;
                Exit(value);
""");
        }
        
        sourceBuilder.AppendLine().Append(
"""
                switch (value)
                {
""");
        AddStateSwitchers(sourceBuilder, stateSwitchers);
        AddInitializationCases(sourceBuilder, states, stateTypes, entersMethods);

        sourceBuilder.AppendLine().Append(
"""             
                    default: throw new ArgumentOutOfRangeException();
                }
                _state = value;
            }
        }
        
        [FieldOffset(0)] private States _state = States.None;
""");
        
        int offset = FieldOffsetStep;
        AddDependencyFields(sourceBuilder, constructorTypes, ref offset);
        AddStatesFields(sourceBuilder, states, offset);

        foreach (KeyValuePair<string, Dictionary<string, bool>> method in otherMethods)
        {
            AddMethod(sourceBuilder, method.Key, method.Value, true);
        }
        
        if (exitsMethods.Count > 0)
        {
            AddExit(sourceBuilder, exitsMethods);
        }

        sourceBuilder.AppendLine().Append(
"""
    }
}

""");
        
        context.AddSource("ActionFsm.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        sourceBuilder.Clear();
    }

    private static MethodDeclarationSyntax[] FilterStateSwitchers(
        MethodDeclarationSyntax[] stateSwitchers, StructDeclarationSyntax[] states)
    {
        IEnumerable<string> structNames = states.Select(s => s.Identifier.Text);
        return stateSwitchers.Where(m => !structNames.Contains(m.Identifier.Text)).ToArray();
    }

    private static void AddUsings(StringBuilder sourceBuilder, HashSet<string> namespaces)
    {
        foreach (string? namespaceName in namespaces.Distinct())
        {
            sourceBuilder.Append("using ").Append(namespaceName).AppendLine(";");
        }

        sourceBuilder.Append(
"""
using System;
using System.Runtime.InteropServices;

""");
    }

    private static void GetDependencyData(StructDeclarationSyntax[] structs, Compilation compilation,
        out HashSet<string> namespaces, 
        out Dictionary<string, string> constructorTypes,
        out Dictionary<string, HashSet<string>> stateTypes)
    {
        namespaces = [];
        constructorTypes = [];
        stateTypes = [];
        
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
                FillDependencyData(parameter, semanticModel, stateName, stateTypes, constructorTypes, namespaces);
            }
        }
    }

    private static void FillDependencyData(
        ParameterSyntax parameter, SemanticModel semanticModel, 
        string stateName, Dictionary<string, HashSet<string>> stateTypes,
        Dictionary<string, string> constructorTypes, HashSet<string> namespaces)
    {
        IParameterSymbol? parameterSymbol = semanticModel.GetDeclaredSymbol(parameter);
        if (parameterSymbol == null) return;
                
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

    private static void GetMethods(StructDeclarationSyntax[] structs, Compilation compilation, string fsmName,
        out HashSet<string> entersMethods, 
        out Dictionary<string, bool> exitsMethods,
        out Dictionary<string, Dictionary<string, bool>> otherMethods)
    {
        entersMethods = [];
        exitsMethods = [];
        otherMethods = [];
        
        foreach (StructDeclarationSyntax structDeclaration in structs)
        {
            string structName = structDeclaration.Identifier.Text;
            
            IEnumerable<MethodDeclarationSyntax> methods = structDeclaration.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(method => method.Modifiers.Any(SyntaxKind.PublicKeyword));
            
            foreach (MethodDeclarationSyntax? method in methods)
            {
                SelectMethod(method, structName, structDeclaration, entersMethods, 
                    exitsMethods, otherMethods, compilation, fsmName);
            }
        }
    }

    private static void SelectMethod(
        MethodDeclarationSyntax method, string structName, StructDeclarationSyntax structDeclaration, 
        HashSet<string> entersMethods, Dictionary<string, bool> exitsMethods, 
        Dictionary<string, Dictionary<string, bool>> otherMethods,
        Compilation compilation, string fsmName)
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
                    if (parameterSymbol == null) break;
                    string name = parameterSymbol.Name;
                    if (name == "States" || name == $"{fsmName}.States")
                    {
                        exitsMethods.Add(structName, true);
                    }
                }
                break;
            default:
                var typeName = method.ReturnType.ToString();
                bool isStateChanger = typeName == "States" || typeName == $"{fsmName}.States";
                        
                if (otherMethods.ContainsKey(methodName))
                {
                    otherMethods[methodName].Add(structName, isStateChanger);
                    break;
                }
                        
                otherMethods.Add(methodName, new Dictionary<string, bool> {{structName, isStateChanger}});
                break;
        }
    }

    private static void AddFsmConstructorParameters(
        StringBuilder sourceBuilder, Dictionary<string, string> constructorTypes)
    {
        if (constructorTypes.Count <= 0)
        {
            sourceBuilder.Append("\n");
            return;
        }
        
        sourceBuilder.Append('(');
        foreach (KeyValuePair<string, string> constructorType in constructorTypes)
        {
            sourceBuilder.Append(constructorType.Value).Append(' ').Append(constructorType.Key).Append(", ");
        }
        sourceBuilder.Remove(sourceBuilder.Length - 2, 2);
        sourceBuilder.Append(")\n");
    }

    private static void AddStateSwitchers(StringBuilder sourceBuilder, MethodDeclarationSyntax[] stateSwitchers)
    {
        foreach (MethodDeclarationSyntax stateSwitcher in stateSwitchers)
        {
            if (!IsMethodStatic(stateSwitcher)) continue;
            sourceBuilder.Append("\n\t\t\t\t\tcase States.").Append(stateSwitcher.Identifier.Text)
                .Append(": State = ").Append(GetFullMethodPath(stateSwitcher)).Append("(); return;");
        }
    }
    
    private static bool IsMethodStatic(MethodDeclarationSyntax methodDeclaration)
    {
        return methodDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword);
    }

    private static string GetFullMethodPath(MethodDeclarationSyntax methodDeclaration)
    {
        TypeDeclarationSyntax? typeDeclaration = methodDeclaration.Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault();
        
        FileScopedNamespaceDeclarationSyntax? fileScopedNamespace = methodDeclaration.Ancestors()
            .OfType<FileScopedNamespaceDeclarationSyntax>()
            .FirstOrDefault();
        
        NamespaceDeclarationSyntax? namespaceDeclaration = methodDeclaration.Ancestors()
            .OfType<NamespaceDeclarationSyntax>()
            .FirstOrDefault();
        
        string namespaceName = fileScopedNamespace != null 
            ? fileScopedNamespace.Name.ToString() 
            : namespaceDeclaration?.Name.ToString() ?? string.Empty;
        
        string typeName = typeDeclaration != null ? typeDeclaration.Identifier.Text : string.Empty;
        string methodName = methodDeclaration.Identifier.Text;

        return namespaceName == string.Empty ? $"{typeName}.{methodName}" : $"{namespaceName}.{typeName}.{methodName}";
    }

    private static void AddInitializationCases(
        StringBuilder sourceBuilder, StructDeclarationSyntax[] structs,  
        Dictionary<string, HashSet<string>> stateTypes, HashSet<string> entersMethods)
    {
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
    }

    private static void AddDependencyFields(
        StringBuilder sourceBuilder, Dictionary<string, string> constructorTypes, ref int offset)
    {
        foreach (KeyValuePair<string, string> type in constructorTypes)
        {
            sourceBuilder.Append("\n\t\t[FieldOffset(").Append(offset).Append(")] private ").Append(type.Value)
                .Append(" _").Append(type.Key).Append(" = ").Append(type.Key).Append(';');
            offset += FieldOffsetStep;
        }
    }

    private static void AddStatesFields(StringBuilder sourceBuilder, StructDeclarationSyntax[] structs, int offset)
    {
        var line = $"\n\t\t[FieldOffset({offset.ToString()})] private ";
        foreach (StructDeclarationSyntax? structDeclaration in structs)
        {
            string name = structDeclaration.Identifier.Text;
            sourceBuilder.Append(line).Append(name).Append(' ').Append(name).Append(";");
        }
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
