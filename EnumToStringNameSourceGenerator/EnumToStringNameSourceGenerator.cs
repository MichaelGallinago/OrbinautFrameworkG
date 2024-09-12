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
public sealed class EnumToStringNameSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<EnumToProcess?> enums = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (syntax, cancellationToken) => syntax.IsKind(SyntaxKind.Attribute),
            transform: static (ctx, cancellationToken) => GetSemanticTargetForGeneration(ctx, cancellationToken))
        .Where(static m => m is not null);

        IncrementalValueProvider<ImmutableArray<EnumToProcess?>> enumToProcess = enums.Collect();
        
        context.RegisterSourceOutput(enumToProcess, 
            (spc, source) => GenerateCode(spc, source!));
    }
    
    private static EnumToProcess? GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
    {
        Compilation compilation = ctx.SemanticModel.Compilation;
        INamedTypeSymbol? fastEnumToStringAttributeSymbol = 
            compilation.GetTypeByMetadataName("EnumToStringNameSourceGenerator.EnumToStringNameAttribute");
        if (fastEnumToStringAttributeSymbol is null)
            return null;

        var attributeSyntax = (AttributeSyntax)ctx.Node;
        foreach (AttributeData? attribute in compilation.Assembly.GetAttributes())
        {
            if (attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken) != attributeSyntax) continue;

            if (!fastEnumToStringAttributeSymbol.Equals(attribute.AttributeClass, SymbolEqualityComparer.Default)) continue;

            if (attribute.ConstructorArguments.Length != 1) continue;

            TypedConstant argument = attribute.ConstructorArguments[0];
            if (argument.Value is null) continue;

            var enumType = (ITypeSymbol)argument.Value;
            if (enumType.TypeKind != TypeKind.Enum) continue;

            return new EnumToProcess(enumType, GetMembers(), IsPublic(enumType, attribute), GetNamespace(attribute));

            List<EnumMemberToProcess> GetMembers()
            {
                var result = new List<EnumMemberToProcess>();
                foreach (var member in enumType.GetMembers())
                {
                    if (member is not IFieldSymbol field)
                        continue;

                    if (field.ConstantValue is null)
                        continue;

                    result.Add(new(member.Name, field.ConstantValue));
                }

                return result;
            }
        }

        return null;
    }

    private static bool IsPublic(ISymbol enumType, AttributeData attribute)
    {
        bool result = IsVisibleOutsideOfAssembly(enumType);
        foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
        {
            if (argument.Key == "IsPublic")
            {
                return result && (bool)argument.Value.Value!;
            }
        }

        return result;
    }
    
    private static string? GetNamespace(AttributeData attribute)
    {
        foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
        {
            if (argument.Key == "ExtensionMethodNamespace") return (string?)argument.Value.Value;
        }

        return null;
    }

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<EnumToProcess> enumToProcess)
    {
        string code = GenerateCode(enumToProcess);
        context.AddSource("FastEnumToStringExtensions.g.cs", SourceText.From(code, Encoding.UTF8));
    }

    private static string GenerateCode(ImmutableArray<EnumToProcess> enums)
    {
        var sb = new StringBuilder();

        foreach (IGrouping<string?, EnumToProcess>? enumerationGroup in enums.GroupBy(en => en.FullNamespace, StringComparer.Ordinal).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            bool typeIsPublic = enumerationGroup.Any(enumeration => enumeration.IsPublic);
            string typeVisibility = typeIsPublic ? "public" : "internal";

            foreach (EnumToProcess? enumeration in enumerationGroup.OrderBy(e => e.FullCsharpName, StringComparer.Ordinal))
            {
                string methodVisibility = enumeration.IsPublic ? "public" : "internal";

                if (!string.IsNullOrEmpty(enumeration.FullNamespace))
                {
                    sb.Append("namespace ").Append(enumeration.FullNamespace).AppendLine();
                    sb.AppendLine("{");
                }

                sb.AppendLine($"/// <summary>A class with memory-optimized alternative to regular ToString() on enums.</summary>");
                sb.Append(typeVisibility).AppendLine(" static partial class FastEnumToStringExtensions");
                sb.AppendLine("{");

                sb.Append("    ")
                  .Append("/// <summary>A memory-optimized alternative to regular ToString() method on <see cref=\"")
                  .Append(enumeration.DocumentationId)
                  .Append("\">")
                  .Append(enumeration.FullCsharpName)
                  .Append(" enum</see>.</summary>\"")
                  .AppendLine();
                sb.Append("    ").Append(methodVisibility).Append(" static string ToStringFast(this global::").Append(enumeration.FullCsharpName).AppendLine(" value)");
                sb.AppendLine("    {");
                sb.AppendLine("        return value switch");
                sb.AppendLine("        {");
                foreach (EnumMemberToProcess? member in enumeration.Members)
                {
                    sb.Append("            ").Append(enumeration.FullCsharpName).Append('.').Append(member.Name).Append(" => nameof(").Append(enumeration.FullCsharpName).Append('.').Append(member.Name).AppendLine("),");
                }

                sb.AppendLine("        _ => value.ToString(),");
                sb.AppendLine("        };");
                sb.AppendLine("    }");
                sb.AppendLine("}");

                if (!string.IsNullOrEmpty(enumeration.FullNamespace))
                {
                    sb.AppendLine("}");
                }
            }
        }

        return sb.ToString();
    }

    private static bool IsVisibleOutsideOfAssembly([NotNullWhen(true)] ISymbol? symbol)
    {
        if (symbol is null) return false;

        if (symbol.DeclaredAccessibility != Accessibility.Public &&
            symbol.DeclaredAccessibility != Accessibility.Protected &&
            symbol.DeclaredAccessibility != Accessibility.ProtectedOrInternal)
        {
            return false;
        }

        return symbol.ContainingType is null || IsVisibleOutsideOfAssembly(symbol.ContainingType);
    }

    private sealed record EnumToProcess(ITypeSymbol EnumSymbol, List<EnumMemberToProcess> Members, bool IsPublic, string? Namespace)
    {
        public string FullCsharpName { get; } = EnumSymbol.ToString()!;
        public string? FullNamespace { get; } = Namespace ?? GetNamespace(EnumSymbol);
        public string DocumentationId { get; } = DocumentationCommentId.CreateDeclarationId(EnumSymbol);

        private static string? GetNamespace(ITypeSymbol symbol)
        {
            string? result = null;
            INamespaceSymbol? ns = symbol.ContainingNamespace;
            while (ns is not null && !ns.IsGlobalNamespace)
            {
                if (result is not null)
                {
                    result = ns.Name + "." + result;
                }
                else
                {
                    result = ns.Name;
                }

                ns = ns.ContainingNamespace;
            }

            return result;
        }
    }
    
    private sealed record EnumMemberToProcess(string Name, object Value);
}
