using Godot;
using System;

public partial class EnemyBase : CharacterBody2D
{
    [Export] public int health = 3;
    [Export] public float moveSpeed = 100f;
    [Export] public bool EnableContactDamage = true; // 碰撞伤害开关
    [Export] public int ContactDamage = 1;           // 碰撞伤害数值
    [Export] public bool CanMove = true; //控制是否移动


    private AnimatedSprite2D EnemyAnim; // 敌人动画

    private Area2D hitbox;


    public override void _Ready()
    {
        EnemyAnim = GetNode<AnimatedSprite2D>("EnemyAnim");
        EnemyAnim.Play("live");
        // 如果子类场景有 Hitbox，就取出来
        if (HasNode("HitBox"))
        {
            hitbox = GetNode<Area2D>("HitBox");
            hitbox.BodyEntered += OnBodyEntered;
        }

        EnemyAnim.AnimationFinished += EnemyClear; //订阅动画结束事件，结束后再清除
    }
    public override void _PhysicsProcess(double delta)
    {
        //if (health > 0&&EnemyAnim.Animation!="live")
        //{
        //    EnemyAnim.Play("live");
        //}
        if (CanMove && Velocity.Length() > 0)
            MoveAndSlide();
    }
    public virtual void TakeDamage(int amount)
    {
        health -= amount;
        GD.Print($"{Name} took {amount} damage, health = {health}");
        if (health <= 0) 
            Die();
    }

    protected virtual void Die()
    {
        GD.Print($"{Name} died!");
        EnemyAnim.Play("die");
        EnableContactDamage = false;
    }
    private void OnBodyEntered(Node2D body)
    {
        if (!EnableContactDamage) return; 

        if (body is Player player)
        {
            GD.Print($"{Name}  {ContactDamage}");
            player.TakeDamage(ContactDamage);
        }
    }
    private void EnemyClear()
    {
        if (EnemyAnim.Animation == "die")
            QueueFree();
    }
    public virtual void GetVelocityDirection()
    {
        return;
    }
}