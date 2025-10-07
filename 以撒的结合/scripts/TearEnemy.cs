using Godot;
using System;

public partial class TearEnemy : Area2D
{
    [Export] public float speed = 600f; 
    [Export] public float maxDistance = 300f; 
    [Export] public float Damage = 1f;

    private Vector2 direction = Vector2.Zero;
    private Vector2 startPosition;
    private AnimatedSprite2D tear; 
    private bool isExploding = false;  // 爆炸标志，为true不造成伤害

    public override void _Ready()
    {
        base._Ready();
        tear = GetNode<AnimatedSprite2D>("tear");
        startPosition = GlobalPosition;

        tear.AnimationFinished += TearClear; // 订阅动画结束事件，结束后再清除

        this.BodyEntered += Hit;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        GlobalPosition += direction * speed * (float)delta;

        if (!isExploding && tear.Animation != "tear")
            tear.Play("tear");

        if (!isExploding && (GlobalPosition - startPosition).Length() > maxDistance)  // 超出射程，销毁
        {
            isExploding = true;
            speed = 0;
            GD.Print("敌人泪弹因超出射程爆炸");
            tear.Play("tearBoom");
        }
    }

    // 设置发射方向（简化版，不需要移动影响）
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.Normalized();
    }

    // 设置发射方向和速度
    public void SetDirectionAndSpeed(Vector2 newDirection, float newSpeed)
    {
        direction = newDirection.Normalized();
        speed = newSpeed;
    }

    private void Hit(Node2D body)
    {
        if (body is Player player && !isExploding)
        {
            isExploding = true;
            speed = 0;

            // 对玩家造成伤害
            player.TakeDamage((int)Damage);

            tear.Play("tearBoom");  // 击中，播放爆炸动画
            GD.Print("敌人泪弹因击中玩家爆炸");
        }
        if (body.Name == "wall")
        {
            isExploding = true;
            speed = 0;
            tear.Play("tearBoom");  // 击中，播放爆炸动画
            GD.Print("敌人泪弹因击中墙爆炸");
        }
    }

    private void TearClear()
    {
        if (tear.Animation == "tearBoom")
            QueueFree();
    }
}