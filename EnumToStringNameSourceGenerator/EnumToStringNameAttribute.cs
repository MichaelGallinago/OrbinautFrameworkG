using System;

namespace EnumToStringNameSourceGenerator;

[System.Diagnostics.Conditional("EnumToStringName_Attributes")]
[AttributeUsage(AttributeTargets.Enum, AllowMultiple = true)]
public sealed class EnumToStringNameAttribute : Attribute
{
    public bool IsPublic { get; set; } = true;
    public string? ClassName { get; set; }
    public string? ExtensionMethodNamespace { get; set; }
}