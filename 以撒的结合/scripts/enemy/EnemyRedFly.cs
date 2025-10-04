using Godot;
using System;

public partial class EnemyRedFly : EnemyBase
{
    
    private Player player;

    public override void _Ready()
    {
        base._Ready();

        CanMove = true;
        EnableContactDamage = true;

        // 找到场景中的玩家
        player = GetTree().Root.GetNode<Player>("Node2D/player");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (player == null || health <= 0) return;

        // 计算方向并移动
        Vector2 dir = (player.GlobalPosition - GlobalPosition).Normalized();
        Velocity = dir * moveSpeed;

        
    }
}
