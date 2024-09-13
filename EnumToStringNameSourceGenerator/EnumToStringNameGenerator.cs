using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EnumToStringNameSourceGenerator;

[Generator]
public sealed class EnumToStringNameGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<EnumToProcess?> enums = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (syntax, _) => syntax.IsKind(SyntaxKind.Attribute),
            transform: static (ctx, cancellationToken) => 
                GetSemanticTargetForGeneration(ctx, cancellationToken))
        .Where(static m => m is not null);

        IncrementalValueProvider<ImmutableArray<EnumToProcess?>> enumToProcess = enums.Collect();
        
        context.RegisterSourceOutput(enumToProcess, 
            (spc, source) => GenerateCode(source!, spc));
    }
    
    private static EnumToProcess? GetSemanticTargetForGeneration(
        GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
    {
        Compilation compilation = ctx.SemanticModel.Compilation;
        INamedTypeSymbol? attributeSymbol = 
            compilation.GetTypeByMetadataName("EnumToStringNameSourceGenerator.EnumToStringNameAttribute");
        
        if (attributeSymbol is null) return null;

        var attributeSyntax = (AttributeSyntax)ctx.Node;
        foreach (AttributeData? attribute in compilation.Assembly.GetAttributes())
        {
            if (attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken) != attributeSyntax) continue;
            if (!attributeSymbol.Equals(attribute.AttributeClass, SymbolEqualityComparer.Default)) continue;
            if (attribute.ConstructorArguments.Length != 1) continue;
            
            TypedConstant enumTypeArgument = attribute.ConstructorArguments[0];
            if (enumTypeArgument.Value == null) continue;
            
            var enumType = (ITypeSymbol)enumTypeArgument.Value;
            if (enumType.TypeKind != TypeKind.Enum) continue;
            
            (bool isPublic, string? @namespace, string className) = GetArguments(enumType, attribute); 
            return new EnumToProcess(enumType, GetMembers(enumType), isPublic, @namespace, className);
        }
        
        return null;
    }
    
    private static List<string> GetMembers(INamespaceOrTypeSymbol enumType)
    {
        var result = new List<string>();
        foreach (ISymbol? member in enumType.GetMembers())
        {
            if (member is not IFieldSymbol field) continue;
            if (field.ConstantValue is null) continue;

            result.Add(member.Name);
        }

        return result;
    }

    private static (bool isPublic, string? @namespace, string className) GetArguments(
        ISymbol enumType, AttributeData attribute)
    {
        (bool isPublic, string? @namespace, string className) result = 
            (IsVisibleOutsideOfAssembly(enumType), null, string.Empty);
        
        foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
        {
            switch (argument.Key)
            {
                case "IsPublic": 
                    result.isPublic = result.isPublic && (bool)argument.Value.Value!; 
                    break;
                case "ExtensionMethodNamespace": 
                    result.@namespace = (string?)argument.Value.Value; 
                    break;
                case "ClassName": 
                    result.className = argument.Value.Value == null ? string.Empty : (string)argument.Value.Value;
                    break;
            }
        }

        return result;
    }
    
    private static void GenerateCode(ImmutableArray<EnumToProcess> enums, SourceProductionContext context)
    {
        var sb = new StringBuilder();
        var tempSb = new StringBuilder();
        
        IOrderedEnumerable<IGrouping<string?, EnumToProcess>> groups = 
            enums.GroupBy(en => en.FullNamespace, StringComparer.Ordinal)
                .OrderBy(g => g.Key, StringComparer.Ordinal);
        
        foreach (IGrouping<string?, EnumToProcess>? enumerationGroup in groups)
        {
            bool typeIsPublic = enumerationGroup.Any(enumeration => enumeration.IsPublicEnum);
            string typeVisibility = typeIsPublic ? "public" : "internal";
            
            IOrderedEnumerable<EnumToProcess> enumerations = 
                enumerationGroup.OrderBy(e => e.FullCsharpName, StringComparer.Ordinal);
            
            foreach (EnumToProcess? enumeration in enumerations)
            {
                AddClass(sb, tempSb, enumeration, typeVisibility);
                var hintName = $"{enumeration.ClassName}.g.cs";
                context.AddSource(hintName, SourceText.From(sb.ToString(), Encoding.UTF8));
            }
        }
    }
    
    private static void AddClass(
        StringBuilder sb, StringBuilder tempSb, EnumToProcess enumeration, string typeVisibility)
    {
        sb.Clear();
        
        string visibility = enumeration.IsPublicEnum ? "public" : "internal";
        sb.AppendLine("using Godot;\n");
        
        bool addNamespace = !string.IsNullOrEmpty(enumeration.FullNamespace);
        if (addNamespace)
        {
            sb.Append(
$$"""
namespace {{enumeration.FullNamespace}}
{

"""
            );
        }
//{{enumeration.ClassName}}
        sb.Append(
$$"""
    /// <summary>A class with generated StringNames from enum.</summary>
    {{typeVisibility}} static partial class {{enumeration.ClassName}}
    {
{{GenerateStringNames(tempSb, enumeration, visibility)}}
        /// <summary>Returns the StringName corresponding to the enum <see cref="{{enumeration.DocumentationId}}">{{enumeration.FullCsharpName}} enum</see>.</summary>"
        {{visibility}} static StringName ToStringName(this global::{{enumeration.FullCsharpName}} value)
        {
            return value switch
            {
{{GenerateSwitchCases(tempSb, enumeration)}}                _ => value.ToString()
            };
        }
    }
"""
        );

        if (addNamespace)
        {
            sb.AppendLine("}");
        }
    }

    private static string GenerateStringNames(StringBuilder tempSb, EnumToProcess enumeration, string visibility)
    {
        tempSb.Clear();
        foreach (string? member in enumeration.Members)
        {
            tempSb.AppendLine(
$"        {visibility} static readonly StringName {member} = nameof({enumeration.FullCsharpName}.{member});");
        }
        
        return tempSb.ToString();
    }
    
    private static string GenerateSwitchCases(StringBuilder tempSb, EnumToProcess enumeration)
    {
        tempSb.Clear();
        foreach (string? member in enumeration.Members)
        {
            tempSb.AppendLine($"                {enumeration.FullCsharpName}.{member} => {member},");
        }

        return tempSb.ToString();
    }

    private static bool IsVisibleOutsideOfAssembly(ISymbol? symbol)
    {
        if (symbol?.DeclaredAccessibility 
            is not (Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal))
        {
            return false;
        }

        return symbol.ContainingType is null || IsVisibleOutsideOfAssembly(symbol.ContainingType);
    }

    private sealed class EnumToProcess(
        ISymbol enumSymbol, List<string> members, bool isPublicEnum, string? @namespace, string className)
    {
        public string ClassName { get; } = className;
        public bool IsPublicEnum { get; } = isPublicEnum;
        public string FullCsharpName { get; } = enumSymbol.ToString()!;
        public string? FullNamespace { get; } = @namespace ?? GetNamespace(enumSymbol);
        public string DocumentationId { get; } = DocumentationCommentId.CreateDeclarationId(enumSymbol);
        public List<string> Members { get; } = members;

        private static string? GetNamespace(ISymbol symbol)
        {
            string? result = null;
            INamespaceSymbol? ns = symbol.ContainingNamespace;
            while (ns is not null && !ns.IsGlobalNamespace)
            {
                result = result is not null ? ns.Name + "." + result : ns.Name;
                ns = ns.ContainingNamespace;
            }
            
            return result;
        }
    }
}
