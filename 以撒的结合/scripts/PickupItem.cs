using Godot;
using System;

public enum PickupType
{
    Heart,
    Bomb,
    Key
}

public partial class PickupItem : Area2D
{
    [Export] public PickupType Type = PickupType.Heart;

    private AnimatedSprite2D sprite;

    public override void _Ready()
    {
        sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        UpdateAppearance();

        BodyEntered += OnBodyEntered;
    }

    /// <summary>
    /// 根据掉落物类型设置材质/动画
    /// </summary>
    private void UpdateAppearance()
    {
        if (sprite == null) return;

        switch (Type)
        {
            case PickupType.Heart:
                sprite.Animation = "Heart"; // Heart 动画或帧
                break;
            case PickupType.Bomb:
                sprite.Animation = "Bomb";  // Bomb 动画或帧
                break;
            case PickupType.Key:
                sprite.Animation = "Key";   // Key 动画或帧
                break;
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (body is Player player)
        {
            switch (Type)
            {
                case PickupType.Heart:
                    player.health = Math.Min(player.health + 1, 999); // 假设最大血量999
                    GD.Print($"玩家捡到红心，当前血量 {player.health}");
                    break;
                case PickupType.Bomb:
                    player.bombCount++;  // 玩家炸弹数量+1
                    GD.Print($"玩家捡到炸弹，当前炸弹数 {player.bombCount}");
                    break;
                case PickupType.Key:
                    player.keyCount++;   // 玩家钥匙数量+1
                    GD.Print($"玩家捡到钥匙，当前钥匙数 {player.keyCount}");
                    break;
            }

            QueueFree(); // 玩家捡起后移除掉落物
        }
    }
}
