using Godot;
using System;

public partial class PlayerDetector : Area2D
{
    // 定义信号
    [Signal] public delegate void player_entered_EventHandler();

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            // 发射自定义信号
            EmitSignal("player_entered_EventHandler");
        }
    }
}
