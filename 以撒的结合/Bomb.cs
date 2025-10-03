using Godot;
using System;

public partial class Bomb : Area2D
{
    [Export] public float BombDelay=2f;
    [Export] public float explodeR = 50f;
    [Export] public int damage = 1;

    private AnimatedSprite2D bomb;

    public override void _Ready()
    {
        base._Ready();
        bomb = GetNode<AnimatedSprite2D>("bomb");
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        BombDelay-=(float)delta;
        if (BombDelay<0)
        {

            BombDelay = 2f;
        }
    }
    private void Explode()
    {

    }
}
