using Godot;
using System;

public partial class Bomb : Area2D
{
    [Export] public float BombDelay = 2f;
    [Export] public float explodeR = 50f;
    [Export] public int damage = 1;

    private bool exploded = false;
    private AnimatedSprite2D bomb;
    private bool animPlayed = false;

    public override void _Ready()
    {
        base._Ready();
        BombDelay = 2f;
        bomb = GetNode<AnimatedSprite2D>("bomb");
        bomb.AnimationFinished += BombClear;
    }
    public override void _Process(double delta)
    {
        if (exploded) return;

        BombDelay -= (float)delta;
        if (BombDelay > 1 && !animPlayed)
            bomb.Play("bomb");
        if (BombDelay < 1 && !animPlayed)
        {
            bomb.Play("ExplodingBomb");
            animPlayed = true;
        }
        if (BombDelay < 0)
        {
            Explode();
        }
    }
    private void Explode()
    {
        if (exploded) return;
        exploded = true;

        CircleShape2D circle = new CircleShape2D();
        circle.Radius = explodeR;  // 半径设置为炸弹爆炸范围
        PhysicsShapeQueryParameters2D query = new PhysicsShapeQueryParameters2D();
        query.Shape = circle;
        query.Transform = new Transform2D(0, GlobalPosition); // 圆心位置
        query.CollideWithBodies = true;
        query.CollideWithAreas = true;
        query.Exclude = new Godot.Collections.Array<Rid>() { this.GetRid() }; // 排除自己

        // 执行查询
        var space = GetWorld2D().DirectSpaceState;
        var results = space.IntersectShape(query);
        GD.Print($"Explosion hit {results.Count} objects");

        foreach (var result in results)
        {
            Node objNode = result["collider"].As<Node>();
            Node2D obj = objNode as Node2D;
            if (obj == null) continue;                  // 如果为空，跳过
            GD.Print($"Explosion collided with: {obj.Name}");
            // 5️⃣ 判断对象是否可以被伤害
            string nodeName = obj.Name.ToString();      // 将Godot的StringName转换成C#的string
            if (nodeName == "player" || nodeName.StartsWith("enemy") || obj is Stone)
            {
                GD.Print($"{obj.Name} taking {damage} damage");
                obj.Call("TakeDamage", damage);
            }
        }

        bomb.Play("boom");
    }
    private void BombClear()
    {
        if (bomb.Animation == "boom") QueueFree();
    }

}