using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EnumToStringNameSourceGenerator;

[Generator]
public sealed class EnumToStringNameGenerator : IIncrementalGenerator
{
    private const string AttributeName = nameof(EnumToStringNameAttribute);
    private const string FullAttributeName = $"{nameof(EnumToStringNameSourceGenerator)}.{AttributeName}";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<EnumToProcess?>> provider = 
            context.SyntaxProvider.ForAttributeWithMetadataName(
                FullAttributeName,
                predicate: static (node, _) => node is EnumDeclarationSyntax,
                transform: static (context, _) => GetSemanticTargetForGeneration(context))
            .Collect();

        context.RegisterSourceOutput(provider, 
            (spc, source) => GenerateCode(spc, source!));
    }
    
    private static EnumToProcess? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext ctx)
    {
        var enumSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        
        Compilation compilation = ctx.SemanticModel.Compilation;
        
        AttributeData? attribute = enumSymbol.GetAttributes()
            .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == FullAttributeName);

        if (attribute == null) return null;
        
        INamedTypeSymbol? attributeSymbol = compilation.GetTypeByMetadataName(FullAttributeName);
        if (attributeSymbol != null && 
            !attributeSymbol.Equals(attribute.AttributeClass, SymbolEqualityComparer.Default)) return null;
        
        if (attribute.ConstructorArguments.Length != 0) return null;
        
        (bool isPublic, string className, string? @namespace) = GetArguments(enumSymbol, attribute); 
        return new EnumToProcess(enumSymbol, GetMembers(enumSymbol), isPublic, @namespace, className);
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

    private static (bool, string, string?) GetArguments(ISymbol enumType, AttributeData attribute)
    {
        (bool isPublic, string className, string? @namespace) result = 
            (IsVisibleOutsideOfAssembly(enumType), $"{enumType.Name}StringNames", null);
        
        foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
        {
            switch (argument.Key)
            {
                case "IsPublic": 
                    result.isPublic = result.isPublic && (bool)argument.Value.Value!; 
                    break;
                case "ClassName":
                    if (argument.Value.Value == null) break;
                    result.className = (string)argument.Value.Value;
                    break;
                case "ExtensionMethodNamespace": 
                    result.@namespace = (string?)argument.Value.Value; 
                    break;
            }
        }

        return result;
    }
    
    private static void GenerateCode(SourceProductionContext context, ImmutableArray<EnumToProcess> enums)
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
        /// <summary>
        /// Returns the StringName corresponding to the enum.
        /// </summary>
        /// <param name="value"><see cref="{{enumeration.DocumentationId}}">{{enumeration.FullCsharpName}}</see>.</param>
        /// <returns><see cref="T:Godot.StringName">Godot.StringName</see>.</returns>
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
