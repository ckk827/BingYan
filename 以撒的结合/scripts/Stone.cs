using Godot;
using System;

public partial class Stone : StaticBody2D
{
    [Export] public int Health = 1; // 石块血量，可被炸弹或子弹摧毁
    private AnimatedSprite2D anim;
    private CollisionShape2D collider;
    private bool isBreaking = false;

    public override void _Ready()
    {
        anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        collider = GetNode<CollisionShape2D>("CollisionShape2D");

        if (AnimationExists("stone"))
            anim.Play("stone");

        anim.AnimationFinished += OnAnimationFinished;
    }

    public void TakeDamage(int damage)
    {
        if (isBreaking) return;
        Health -= damage;

        if (Health <= 0)
            Break();
    }

    private void Break()
    {
        isBreaking = true;
        collider.Disabled = true;

        if (AnimationExists("stoneBreak"))
            anim.Play("stoneBreak");
        else
            QueueFree(); 
    }

    private bool AnimationExists(string name)
    {
        return anim.SpriteFrames != null && anim.SpriteFrames.HasAnimation(name);
    }

    private void OnAnimationFinished()
    {
        if (isBreaking)
            QueueFree();
    }
}
