using Godot;
using System;
using System.Runtime.CompilerServices;
using static Godot.WebSocketPeer;

public partial class Player : CharacterBody2D
{
    
    [Export] public float moveSpeed = 5f;
    [Export] public int health = 6;
    [Export] public float tearCD = 0.3f;
    [Export] public float bombCD = 0.5f;
    [Export] public float invincibleDuration = 1f;
    [Export] private PackedScene tearScene;
    [Export] private PackedScene bombScene;
    

    //private string currentDir = "Down";
    private string currentState = "Stand";
    private string currentDirection = "Down";

    private AnimatedSprite2D feet; // 脚动画
    private AnimatedSprite2D head; // 头动画

    private Vector2 moveInput = Vector2.Zero;
    private Vector2 facingDirection = Vector2.Down;

    private float tearTimer = 0f;             // 射击计时器
    private float bombTimer = 0f;             // 炸弹计时器
    private float invincibleTimer = 0f;             // 无敌时间计时器


    public override void _Ready()
    {
        feet = GetNode<AnimatedSprite2D>("feet"); 
        head = GetNode<AnimatedSprite2D>("head");

        //确保头显示在脚上
        feet.ZIndex = 0;
        head.ZIndex = 1;
    }
    public override void _PhysicsProcess(double delta)
    {
        getSpeed();
        getDirection();
        feetAnimation();
        HeadAnimation();
        HandleShooting(delta); // 射击逻辑
        HandleBomb(delta); // 炸弹逻辑
        Invincibility( delta); // 无敌时间逻辑
        MoveAndSlide();
    }
    private void getSpeed()
    {
        moveInput = Vector2.Zero;
        if (Input.IsActionPressed("right")) moveInput.X += 1;
        if (Input.IsActionPressed("left")) moveInput.X -= 1;
        if (Input.IsActionPressed("down")) moveInput.Y += 1;
        if (Input.IsActionPressed("up")) moveInput.Y -= 1;
        Velocity = moveInput.Normalized() * moveSpeed;
    }
    private void getDirection()
    {
        //
        if (Input.IsActionPressed("right")) facingDirection = Vector2.Right;
        if (Input.IsActionPressed("left")) facingDirection = Vector2.Left;
        if (Input.IsActionPressed("down")) facingDirection = Vector2.Down;
        if (Input.IsActionPressed("up")) facingDirection = Vector2.Up;
        //头部优先跟随射击方向
        if (Input.IsActionPressed("l_shoot")) facingDirection = Vector2.Left;
        if (Input.IsActionPressed("r_shoot")) facingDirection = Vector2.Right;
        if (Input.IsActionPressed("d_shoot")) facingDirection = Vector2.Down;
        if (Input.IsActionPressed("u_shoot")) facingDirection = Vector2.Up;
    }
    private void feetAnimation()
    {
        currentState = (Velocity == Vector2.Zero) ? "Stand" : "Walk";
        if (moveInput.X < 0) currentDirection = "Left";
        if (moveInput.X > 0) currentDirection = "Right";
        if (moveInput.Y < 0) currentDirection = "Up";
        if (moveInput.Y > 0) currentDirection = "Down";
        feet.Play("feet" + currentState + currentDirection);
    }
    private void HeadAnimation()
    {
        
        if (facingDirection == Vector2.Left) head.Play("headLeft");
        if (facingDirection == Vector2.Right) head.Play("headRight");
        if (facingDirection == Vector2.Up) head.Play("headUp");
        if (facingDirection == Vector2.Down) head.Play("headDown");
    }
    private void HandleShooting(double delta)
    {
        // 冷却时间递减
        if (tearTimer > 0)
            tearTimer -= (float)delta;

        // 检查射击输入
        Vector2 shootDir = Vector2.Zero;
        if (Input.IsActionPressed("l_shoot")) shootDir = Vector2.Left;
        if (Input.IsActionPressed("r_shoot")) shootDir = Vector2.Right;
        if (Input.IsActionPressed("d_shoot")) shootDir = Vector2.Down;
        if (Input.IsActionPressed("u_shoot")) shootDir = Vector2.Up;

        // 如果有射击输入并且冷却结束
        if (shootDir != Vector2.Zero && tearTimer <= 0)
        {
            Shoot(shootDir);
            tearTimer = tearCD; // 重置冷却
        }
    }

    private void Shoot(Vector2 shootDir)
    {
        var tear = (Tear)tearScene.Instantiate();
        tear.GlobalPosition = GlobalPosition;     // 从玩家位置发射
        tear.GetShootDirection(shootDir, Velocity , 0.3f);  //输入发射速度方向，玩家速度及其权重，计算出子弹初速度方向
        // 添加到场景
        GetTree().CurrentScene.AddChild(tear);
    }
    private void HandleBomb(double delta)
    {
        if (bombTimer > 0)
            bombTimer -= (float)delta;
        else
        {
            if (Input.IsActionPressed("bomb"))
            {
                Bomb();
                bombTimer = bombCD;
            }
        }
    }
    private void Bomb()
    {
        var bomb = (Bomb)bombScene.Instantiate();
        bomb.GlobalPosition = GlobalPosition; ;
        GetTree().CurrentScene.AddChild(bomb);
    }
    public void TakeDamage(int amount)
    {
        if (invincibleTimer < 0 || invincibleTimer == 0 )
        {
            health -= amount; // 扣血
            GD.Print($"Player took {amount} damage! Current health: {health}");

            if (health <= 0)
            {
                Die();
            }

            invincibleTimer = invincibleDuration;
        }
    }

    private void Die()
    {
        GD.Print("Player died!");
        QueueFree(); // 删除玩家节点
    }

    private void Invincibility(double delta) // 无敌时间控制
    {
        if (invincibleTimer > 0)
        {
            invincibleTimer-=(float)delta;
            // 无敌时间人物闪烁
            if (Mathf.FloorToInt(invincibleTimer * 10) % 2 == 0)
                Visible = true;
            else
                Visible = false;
        }
        else
        {
            Visible = true; // 计时结束，恢复正常
        }
    }
}
