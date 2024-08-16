using System;

namespace FsmSourceGenerator;

[System.Diagnostics.Conditional("Fsm_Attributes")]
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
public class FsmStateSwitcherAttribute(string fsmName) : Attribute
{
    public string FsmName { get; } = fsmName;
}
