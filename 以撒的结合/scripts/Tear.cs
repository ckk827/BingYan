using Godot;
using System;

public partial class Tear : Area2D
{
    [Export] public float speed = 1000f;
    [Export] public float maxDistance = 400f;
    [Export] public float damage = 1f;

    private Vector2 direction = Vector2.Zero;
    private Vector2 startPosition;
    private AnimatedSprite2D tear; // 眼泪动画
    private bool isExploding = false;
    
    public override void _Ready()
    {
        base._Ready();
        tear = GetNode<AnimatedSprite2D>("tear");
        startPosition = GlobalPosition;

        tear.AnimationFinished += TearClear; //订阅动画结束事件，结束后再清除

        this.BodyEntered += Hit;
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        GlobalPosition += direction * speed * (float)delta;
        if (!isExploding && tear.Animation != "tearFly") 
            tear.Play("tearFly");
        if ((GlobalPosition-startPosition).Length()>maxDistance)  //超出射程，销毁
        {
            
            isExploding = true;
            speed = 0;
            tear.Play("tearBoom");
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
            isExploding = true;
            speed = 0;
            tear.Play("tearBoom");  //击中，销毁
        }
    }
    private void TearClear()
    {

        if (tear.Animation=="tearBoom") QueueFree(); 

    }
}
