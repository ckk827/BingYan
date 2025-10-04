using Godot;
using System;

public partial class EnemyNerve : EnemyBase
{
    public override void _Ready()
    {
        base._Ready();
        health = 20;
        CanMove = false;
        EnableContactDamage = true;
        ContactDamage = 1;
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }

}

