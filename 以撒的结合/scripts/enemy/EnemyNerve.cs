using Godot;
using System;

public partial class EnemyNerve : EnemyBase
{
    public override void _Ready()
    {
        base._Ready();
        CanMove = false;
        EnableContactDamage = true;
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }

}

