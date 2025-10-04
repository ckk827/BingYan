using Godot;
using System;

public partial class Door : StaticBody2D
{
    [Export] public string DoorName = "Top"; // 标记门的方向: Top / Bottom / Left / Right

    private AnimatedSprite2D DoorAnim;
    private CollisionShape2D DoorCollider;
    
    public override void _Ready()
    {
        DoorAnim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        DoorCollider = GetNode<CollisionShape2D>("CollisionShape2D");
    }


    public void Open()
    {
        string animName = $"Door{DoorName}Open";
        DoorAnim.Play(animName);

        DoorCollider.Disabled = true;
    }


    public void Close()
    {
        string animName = $"Door{DoorName}Close";
        DoorAnim.Play(animName);

        DoorCollider.Disabled = false;
    }
}
