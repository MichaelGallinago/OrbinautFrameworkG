using System;
using Godot;
using OrbinautFramework3.Framework.Animations;

namespace OrbinautFramework3.Framework;

public partial class SceneLateUpdate : Node
{
    public event Action Update;
     
    public override void _Process(double delta)
    {
        Animator.Update(FrameworkData.ProcessSpeed);
        Update?.Invoke();
    }
}