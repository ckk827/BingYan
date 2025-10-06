using Godot;
using System;

public partial class EnemyNerve : EnemyBase
{
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
        CanMove = false;
        EnableContactDamage = true;
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }

}

