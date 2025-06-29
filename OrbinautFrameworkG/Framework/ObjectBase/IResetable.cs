﻿using Godot;
using OrbinautFrameworkG.Framework.Culling;

namespace OrbinautFrameworkG.Framework.ObjectBase;

public interface IResetable : ICullable
{
    new Vector2 Position { get; set; }
    void Reset();
}
