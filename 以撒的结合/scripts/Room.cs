using Godot;
using System;

public partial class Room : Node2D
{
    [Export] public Vector2 RoomSize = new Vector2(1280, 640); // 房间大小
    [Export] public PackedScene[] EnemyScenes;
    [Export] public PackedScene[] ObstacleScenes;

    [Export] public int EnemyCount = 3;
    [Export] public int ObstacleCount = 2;
    [Export] public float SpawnMinDistance = 50f;

    [Export] public bool TopDoorEnabled = true;
    [Export] public bool TopDoorOpen = true;
    [Export] public bool BottomDoorEnabled = true;
    [Export] public bool BottomDoorOpen = true;
    [Export] public bool LeftDoorEnabled = true;
    [Export] public bool LeftDoorOpen = true;
    [Export] public bool RightDoorEnabled = true;
    [Export] public bool RightDoorOpen = true;
    [Export] public NodePath LeftDoorTargetRoomPath;
    [Export] public NodePath RightDoorTargetRoomPath;
    [Export] public NodePath TopDoorTargetRoomPath;
    [Export] public NodePath BottomDoorTargetRoomPath;
    [Export] public NodePath CameraPath;




    private Camera2D camera;
    private bool cleared = false;
    private int enemyDead = 0;
    private int enemyTotal = 0;
    private SpawnPoints spawnPoints;


    public override void _Ready()
    {
        camera = GetTree().Root.GetNode<Camera2D>("root/Camera2D");
        spawnPoints = GetNode<SpawnPoints>("SpawnPoints");

        CallDeferred(nameof(SyncChildNodes));

        // 玩家进入房间检测器
        var detector = GetNode<Area2D>("PlayerDetector");
        var enemyCount = GetNode<SpawnPoints>("SpawnPoints");  // 取出敌人生成节点，得到敌人总数

        detector.BodyEntered += OnPlayerEntered;
        EnemyBase.OnEnemyDied += OnEnemyDied;
        enemyTotal = enemyCount.EnemyCount;

        if (enemyTotal == 0)
            ClearRoom();
    }
    private void SyncChildNodes()
    {
        // 同步 SpawnPoints
        var sp = GetNodeOrNull<SpawnPoints>("SpawnPoints");
        if (sp != null)
        {
            sp.EnemyCount = EnemyCount;
            sp.ObstacleCount = ObstacleCount;
            sp.minDistance = SpawnMinDistance;
        }

        // 同步 Doors
        var doorRoot = GetNodeOrNull<Node2D>("Doors");
        if (doorRoot != null)
        {
            SyncDoor(doorRoot.GetNodeOrNull<Door>("DoorTop"), TopDoorEnabled, TopDoorOpen, TopDoorTargetRoomPath);
            SyncDoor(doorRoot.GetNodeOrNull<Door>("DoorBottom"), BottomDoorEnabled, BottomDoorOpen, BottomDoorTargetRoomPath);
            SyncDoor(doorRoot.GetNodeOrNull<Door>("DoorLeft"), LeftDoorEnabled, LeftDoorOpen, LeftDoorTargetRoomPath);
            SyncDoor(doorRoot.GetNodeOrNull<Door>("DoorRight"), RightDoorEnabled, RightDoorOpen, RightDoorTargetRoomPath);
        }
    }
    private void SyncDoor(Door door, bool enabled, bool open, NodePath nodePath)
    {
        if (door == null) return;
        door.IsEnabled = enabled;
        door.IsOpen = open;
        door.CallDeferred("UpdateDoorAppearance");
        nodePath = door.TargetRoomPath;
    }
   
    private void OnEnemyDied(EnemyBase enemy)
    {
        // 检查敌人是否属于这个房间
        if (enemy.GetParent().GetParent() != this) return;

        enemyDead++;
        GD.Print($"[{Name}] 敌人死亡 {enemyDead}/{enemyTotal}");

        if (enemyDead >= enemyTotal)
            ClearRoom();
    }
    private void OnPlayerEntered(Node body)
    {
        if (body is Player)
        {
            GD.Print($"玩家进入房间 {Name}");
            LockDoors();        // 关门
            AdjustCamera();     // 限制摄像机
        }
    }

    public void ClearRoom()
    {
        if (cleared) return;
        cleared = true;
        UnlockDoors();
        GD.Print($"房间 {Name} 已清空，门已打开");
    }

    private void LockDoors()
    {
        foreach (Area2D door in GetTree().GetNodesInGroup("Doors"))
            door.Monitoring = false;
    }

    private void UnlockDoors()
    {
        foreach (Area2D door in GetTree().GetNodesInGroup("Doors"))
            door.Monitoring = true;
    }

    private void AdjustCamera()
    {
        if (camera == null) return;

        camera.LimitLeft = (int)(GlobalPosition.X - RoomSize.X / 2);
        camera.LimitTop = (int)(GlobalPosition.Y - RoomSize.Y / 2);
        camera.LimitRight = (int)(GlobalPosition.X + RoomSize.X/2);
        camera.LimitBottom = (int)(GlobalPosition.Y + RoomSize.Y/2);
    }
    public Vector2 GetEntrancePosition(string direction)
    {
        // Room 下有若干 Door 节点：DoorTop, DoorBottom, DoorLeft, DoorRight
        var doorNode = GetNodeOrNull<Door>($"Door{direction}");
        if (doorNode != null)
        {
            // 玩家应该出现在门的稍内侧一点点
            Vector2 offset = direction switch
            {
                "Top" => new Vector2(0, 32),
                "Bottom" => new Vector2(0, -32),
                "Left" => new Vector2(32, 0),
                "Right" => new Vector2(-32, 0),
                _ => Vector2.Zero
            };
            return doorNode.GlobalPosition + offset;
        }
        return GlobalPosition;
    }
   
}