using System;

namespace EnumToStringNameSourceGenerator;

[System.Diagnostics.Conditional("EnumToStringName_Attributes")]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class EnumToStringNameAttribute(Type enumType, string className) : Attribute
{
    public Type EnumType { get; } = enumType;
    public string ClassName { get; } = className;
    
    public bool IsPublic { get; set; } = true;
    public string? ExtensionMethodNamespace { get; set; }
}