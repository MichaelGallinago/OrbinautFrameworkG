using System;

namespace FastEnumToStringSourceGenerator;

[System.Diagnostics.Conditional("EnumToStringName_Attributes")]
[AttributeUsage(AttributeTargets.Enum, AllowMultiple = true)]
public sealed class FastEnumToStringAttribute : Attribute
{
    public bool IsPublic { get; set; } = true;
    public string? ClassName { get; set; }
    public string? ExtensionMethodNamespace { get; set; }
}