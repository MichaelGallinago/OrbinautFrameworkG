using System;

namespace EnumToStringNameSourceGenerator;

[System.Diagnostics.Conditional("EnumToStringName_Attributes")]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class EnumToStringNameAttribute(Type enumType) : Attribute
{
    public Type EnumType { get; } = enumType;
    public bool IsPublic { get; set; } = true;
    public string? ExtensionMethodNamespace { get; set; }
}