using System;

namespace FsmSourceGenerator;

[System.Diagnostics.Conditional("Fsm_Attributes")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class FsmStateSwitcherAttribute(string fsmName) : Attribute
{
    public string FsmName { get; } = fsmName;
}
