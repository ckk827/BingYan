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
    [Export] public PackedScene PickupScene;

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
            var enemy = scene.Instantiate<EnemyBase>();
            enemy.Position = pos;
            enemy.Name = $"enemy_{i}";

            enemy.OnEnemyDied += (EnemyBase e) =>
            {
                parentRoom.OnEnemyDied(e);     // 房间计数
                HandleEnemyDrop(e);             // 掉落物生成
            };

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
    private void HandleEnemyDrop(EnemyBase enemy)
    {
        if (PickupScene == null || enemy.DropTable == null) return;

        foreach (var drop in enemy.DropTable)
        {
            if (random.NextDouble() < drop.Chance)
            {
                var pickup = PickupScene.Instantiate<PickupItem>();
                pickup.Type = drop.Type;

               //  保证掉落物不重叠
                Vector2 pos = GetNonOverlappingPosition();
                //Vector2 pos = enemy.GlobalPosition;
                //Vector2 pos = GetParent().ToLocal(enemy.GlobalPosition);
                //pickup.Position = pos;
                //if (pos == null) GD.Print("掉落物：敌人坐标获取失败");
                //else GD.Print("掉落物：敌人坐标获取成功");
                 pickup.Position = pos;

               // GetParent().AddChild(pickup);
                GetParent().CallDeferred("add_child", pickup);
                occupiedPositions.Add(pos);

                GD.Print($"生成掉落物 {drop.Type} 在 {pos}");
            }
        }
    }
}
