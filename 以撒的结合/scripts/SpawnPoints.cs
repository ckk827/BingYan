using Godot;
using System;
using System.Collections.Generic;

public partial class SpawnPoints : Node2D
{
    [Export] public PackedScene[] EnemyScenes;
    [Export] public PackedScene[] ObstacleScenes;
    [Export] public int EnemyCount = 3;
    [Export] public int ObstacleCount = 2;
    [Export] public Rect2 SpawnArea = new Rect2(-100, -100, 200, 200);
    [Export] public float minDistance = 50f; // 最小间距，防止重叠

    private Random random = new Random();
    private List<Vector2> occupiedPositions = new List<Vector2>();
    private Room parentRoom;


    public override void _Ready()
    {
        parentRoom = GetParent<Room>(); // ✅ 获取上级房间
        var detector = parentRoom.GetNode<Area2D>("PlayerDetector");
        detector.BodyEntered += OnPlayerEntered;
    }

    private void OnPlayerEntered(Node body)
    {
        if (body is Player)
        {
            //GD.Print("玩家进入房间，开始生成敌人和障碍物");
            //CallDeferred(nameof(SpawnObstacles), ObstacleCount);
            //CallDeferred(nameof(SpawnEnemy), EnemyCount);
        }
    }
    public void Spawn()
    {
        GD.Print("玩家进入房间，开始生成敌人和障碍物");
        CallDeferred(nameof(SpawnObstacles), ObstacleCount);
        CallDeferred(nameof(SpawnEnemy), EnemyCount);
    }

    private void SpawnEnemy(int MaxEnemyNum)
    {
        if (EnemyScenes.Length == 0) return;

        for (int i = 0; i < MaxEnemyNum; i++)
        {
            Vector2 pos = GetNonOverlappingPosition();
            var scene = EnemyScenes[random.Next(EnemyScenes.Length)];
            var enemy = scene.Instantiate<EnemyBase>(); // ✅ 这里用 EnemyBase
            enemy.Position = pos;
            enemy.Name = $"enemy_{i}";

            // ✅ 绑定事件
            enemy.OnEnemyDied += parentRoom.OnEnemyDied;

            parentRoom.AddChild(enemy);
            occupiedPositions.Add(pos);
        }
    }


    private void SpawnObstacles(int MaxObstacleNum)
    {
        if (ObstacleScenes.Length == 0) return;

        for (int i = 0; i < MaxObstacleNum; i++)
        {
            Vector2 pos = GetNonOverlappingPosition();
            var scene = ObstacleScenes[random.Next(ObstacleScenes.Length)];
            var obstacle = scene.Instantiate<Node2D>();
            obstacle.Position = pos;
            obstacle.Name = "stone";
            GetParent().AddChild(obstacle);
            occupiedPositions.Add(pos);
        }
    }

    private Vector2 GetNonOverlappingPosition()
    {
        Vector2 pos;
        int tries = 0;
        do
        {
            float x = (float)(random.NextDouble() * SpawnArea.Size.X + SpawnArea.Position.X);
            float y = (float)(random.NextDouble() * SpawnArea.Size.Y + SpawnArea.Position.Y);
            pos = Position + new Vector2(x, y);
            tries++;
            if (tries > 100) break; // 防卡死
        }
        while (IsOverlapping(pos));
        return pos;
    }

    private bool IsOverlapping(Vector2 pos)
    {
        foreach (var p in occupiedPositions)
        {
            if (pos.DistanceTo(p) < minDistance)
                return true;
        }
        return false;
    }

}
