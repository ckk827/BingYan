using Godot;
using System;

public partial class Tear : Area2D
{
    [Export] public float speed = 1000f;
    [Export] public float maxDistance = 400f;
    private Vector2 direction = Vector2.Zero;
    private Vector2 startPosition;
    private AnimatedSprite2D tear; // 眼泪
    public override void _Ready()
    {
        base._Ready();
        tear = GetNode<AnimatedSprite2D>("tear");
        startPosition = GlobalPosition;
        this.BodyEntered += Hit;
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        GlobalPosition += direction * speed * (float)delta;
        tear.Play("tearFly");
        if ((GlobalPosition-startPosition).Length()>maxDistance)
        {
            tear.Play("tearBoom");
            QueueFree();
        }
    }
    public void GetShootDirection(Vector2 shootSpeedDirection,Vector2 moveSpeed, float moveInfluence = 0.3f)
    {
        direction = (shootSpeedDirection * speed + moveSpeed*moveInfluence).Normalized();
    }
    private void Hit(Node2D body)
    {
        if (body.Name!="Player")
        {
            tear.Play("tearBoom");
            QueueFree();
        }
    }
}
