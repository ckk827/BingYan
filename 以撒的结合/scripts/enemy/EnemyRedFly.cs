using Godot;
using System;

public partial class EnemyRedFly : EnemyBase
{
    
    private Player player;
    [Export] public float HeartPercent = 0;
    [Export] public float BombPercent = 0;
    [Export] public float KeyPercent = 0;

    public override void _Ready()
    {
        base._Ready();
        DropTable = new DropItem[]
            {
                       new DropItem(PickupType.Heart, HeartPercent), // 掉落心
                       new DropItem(PickupType.Bomb, BombPercent),  // 掉落炸弹
                       new DropItem(PickupType.Key, KeyPercent),  // 掉落炸弹
            };
       
        CanMove = true;
        EnableContactDamage = true;

        // 找到场景中的玩家
        player = GetTree().Root.GetNode<Player>("root/player");
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
