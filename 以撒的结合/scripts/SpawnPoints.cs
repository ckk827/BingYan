using Godot;
using System;

public partial class SpawnPoints : Node2D
{
    [Export] public PackedScene[] EnemyScenes;   // 敌人预制体数组
    [Export] public PackedScene[] ObstacleScenes; // 障碍物预制体数组
    [Export] public int EnemyCount = 3;          // 生成的敌人数
    [Export] public int ObstacleCount = 2;       // 生成的障碍数量
    [Export] public Rect2 SpawnArea = new Rect2(-100, -100, 200, 200); // 随机范围（相对位置）

    private Random random = new Random();

    public override void _Ready()
    {
        var detector = GetParent().GetNode<Area2D>("PlayerDetector");
        detector.BodyEntered += OnPlayerEntered;
    }

    private void OnPlayerEntered(Node body)
    {
        if (body is Player)
        {
            GD.Print("玩家进入房间，开始生成敌人和障碍物");
            CallDeferred(nameof(SpawnEnemy), EnemyCount);
            CallDeferred(nameof(SpawnObstacle), EnemyCount);
            
        }
    }

    private void SpawnEnemy(int MaxEnemyNum)
    {
        if (EnemyScenes.Length == 0) return;

        for (int i = 0; i < MaxEnemyNum; i++)
        {
            Vector2 pos = new Vector2(random.Next(-600, 600), random.Next(-300, 300));

            var scene = EnemyScenes[random.Next(EnemyScenes.Length)];
            var enemy = scene.Instantiate<Node2D>();
            enemy.Position = pos;

            GetParent().AddChild(enemy);
        }
    }

    private void SpawnObstacle()
    {
        for (int i = 0; i < ObstacleCount; i++)
        {
            if (ObstacleScenes.Length == 0) return;
            var scene = ObstacleScenes[random.Next(ObstacleScenes.Length)];
            var obstacle = scene.Instantiate<Node2D>();
            obstacle.Position = GetRandomPosition();
            GetParent().AddChild(obstacle);
        }
    }

    private Vector2 GetRandomPosition()
    {
        float x = (float)(random.NextDouble() * SpawnArea.Size.X + SpawnArea.Position.X);
        float y = (float)(random.NextDouble() * SpawnArea.Size.Y + SpawnArea.Position.Y);
        return Position + new Vector2(x, y); // 相对于 SpawnPoint 的位置
    }
}