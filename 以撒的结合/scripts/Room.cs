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

 //   [Signal] public delegate void RoomClearedEventHandler(); //房间已经清理


    public override void _Ready()
    {

        camera = GetTree().Root.GetNode<Camera2D>("root/Camera2D");
        spawnPoints = GetNode<SpawnPoints>("SpawnPoints");

        CallDeferred(nameof(SyncChildNodes));
       // enemyTotal = GetTree().GetNodesInGroup("Enemy").Count;

        // 玩家进入房间检测器
        var detector = GetNode<Area2D>("PlayerDetector");
        var enemyCount = GetNode<SpawnPoints>("SpawnPoints");  // 取出敌人生成节点，得到敌人总数
      
        detector.BodyEntered += OnPlayerEntered;

        enemyTotal = EnemyCount;

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
            sp.EnemyScenes = EnemyScenes;
            sp.ObstacleScenes = ObstacleScenes;
        }

        // 同步 Doors
        var doorRoot = GetNodeOrNull<Node2D>("doors");
        if (doorRoot != null)
        {
            GD.Print("正在覆盖");
            SyncDoor(doorRoot.GetNodeOrNull<Door>("DoorTop"), TopDoorEnabled, TopDoorOpen, TopDoorTargetRoomPath);
            SyncDoor(doorRoot.GetNodeOrNull<Door>("DoorBottom"), BottomDoorEnabled, BottomDoorOpen, BottomDoorTargetRoomPath);
            SyncDoor(doorRoot.GetNodeOrNull<Door>("DoorLeft"), LeftDoorEnabled, LeftDoorOpen, LeftDoorTargetRoomPath);
            SyncDoor(doorRoot.GetNodeOrNull<Door>("DoorRight"), RightDoorEnabled, RightDoorOpen, RightDoorTargetRoomPath);
        }
    }
    private void SyncDoor(Door door, bool enabled, bool open, NodePath nodePath)
    {
        if (door == null) 
            return;
        door.IsEnabled = enabled;
        door.IsOpen = open;
        door.CallDeferred("UpdateDoorAppearance");
        // 自动修正 nodePath
        
        if (nodePath != null)
        {
            var target = GetNodeOrNull(nodePath);
            if (target != null)
            {
                door.TargetRoomPath = target.GetPath();
                GD.Print($"{door.Name}: NodePath 已由 Room 自动修正  {door.TargetRoomPath}");
            }
            else
            {
                GD.Print($"{door.Name}: Room 无法解析该路径 {nodePath}");
            }
        }
 
        if (door.TargetRoomPath == null)
            GD.Print("目标路径为空");
    }
   
    public void OnEnemyDied(EnemyBase enemy)
    {
        //// 检查敌人是否属于这个房间
        //if (enemy.GetParent().GetParent() != this)
        //{
        //    GD.Print("不属于这个房间");
        //    return;
        //}

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
            if (enemyTotal!=0 && cleared == false)
                LockDoors();        // 关门
            AdjustCamera();     // 限制摄像机
            
        }
    }

    public void ClearRoom()
    {
        if (cleared) return;

        cleared = true;

    //    EmitSignal(SignalName.RoomCleared);
    
        UnlockDoors();
        var playernode = GetParent().GetParent().GetNode<Player>("player"); ;
        playernode.energy += 1;
        if (playernode.ExtraDamage != 0) playernode.ExtraDamage = 0;

        GameState.ClearedRooms.Add(Name); 
        GD.Print($"房间 {Name} 已清空，门已打开，状态已保存。");
    }

    private void LockDoors()
    {
        foreach (var node in GetTree().GetNodesInGroup("Doors"))
        {
            if (node is Door door)
            {
                door.IsOpen = false;
                door.CallDeferred("UpdateDoorAppearance");
            }
        }
        var spawnPoints = GetNode<SpawnPoints>("SpawnPoints");
        spawnPoints.Spawn();
    }

    private void UnlockDoors()
    {
        GD.Print("调用 UnlockDoors()");

        var doorNodes = GetTree().GetNodesInGroup("Doors");
        GD.Print($"找到 {doorNodes.Count} 个 Doors 组节点");

        foreach (var node in doorNodes)
        {
            GD.Print($"节点: {node.Name}, 类型: {node.GetType()}");

            if (node is Door door)
            {
                GD.Print($"解锁门: {door.DoorName}，设置 IsOpen = true");
                door.IsOpen = true;
                GD.Print("调用 UpdateDoorAppearance...");
                door.CallDeferred("UpdateDoorAppearance");
            }
            else
            {
                GD.Print($" 节点 {node.Name} 不是 Door 类型");
            }
        }
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
        var doorNode = GetNodeOrNull<Door>($"doors/Door{direction}");
        if (doorNode != null)
        {
            // 玩家应该出现在门的稍内侧一点点
            Vector2 offset = direction switch
            {
                "Top" => new Vector2(0, 200),
                "Bottom" => new Vector2(0, -200),
                "Left" => new Vector2(200, 0),
                "Right" => new Vector2(-200, 0),
                _ => Vector2.Zero
            };
            return doorNode.GlobalPosition + offset;
        }
        return GlobalPosition;
    }
   
}