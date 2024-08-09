using System;

namespace FsmSourceGenerator;

[System.Diagnostics.Conditional("Fsm_Attributes")]
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
public sealed class FsmStateAttribute(string fsmName) : Attribute
{
    public string FsmName { get; } = fsmName;
}
