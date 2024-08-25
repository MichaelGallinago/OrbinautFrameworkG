using System;

namespace FsmSourceGenerator;

[System.Diagnostics.Conditional("Fsm_Attributes")]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class FsmAttribute(string name, string @namespace) : Attribute
{
    public string Name { get; } = name;
    public string Namespace { get; } = @namespace;
}