using Godot;
using System;
using System.Collections.Generic;

public partial class Spike : Area2D
{
    [Export] public int damage = 1;
    [Export] public float attackCD = 1f; // 每隔 1 秒攻击一次

    private Dictionary<Node, float> lastHitTimes = new Dictionary<Node, float>();

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is Player || body.Name.ToString().StartsWith("enemy"))
        {
            if (!lastHitTimes.ContainsKey(body))
            {
                lastHitTimes[body] = -attackCD; // 保证一进入就能攻击
                ApplyDamage(body);
            }
        }
    }

    private void OnBodyExited(Node body)
    {
        if (lastHitTimes.ContainsKey(body))
            lastHitTimes.Remove(body);
    }

    public override void _Process(double delta)
    {
        var keys = new List<Node>(lastHitTimes.Keys);
        foreach (var body in keys)
        {
            if (!IsInstanceValid(body))
            {
                lastHitTimes.Remove(body);
                continue;
            }

            lastHitTimes[body] += (float)delta;
            if (lastHitTimes[body] >= attackCD)
            {
                ApplyDamage(body);
                lastHitTimes[body] = 0f;
            }
        }
    }

    private void ApplyDamage(Node body)
    {
        GD.Print($"{body.Name} hit by spike for {damage} damage");
        body.Call("TakeDamage", damage);
    }
}
