using Godot;
using System;

public partial class Room : Node2D
{
    [Export] public Vector2 RoomSize = new Vector2(800, 600); // 房间大小
    private Camera2D camera;
    private bool cleared = false;

    public override void _Ready()
    {
        camera = GetTree().GetRoot().GetNode<Camera2D>("GameRoot/Player/Camera2D");

        // 玩家进入房间检测器
        var detector = GetNode<Area2D>("PlayerDetector");
        detector.BodyEntered += OnPlayerEntered;
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

        camera.LimitLeft = (int)GlobalPosition.X;
        camera.LimitTop = (int)GlobalPosition.Y;
        camera.LimitRight = (int)(GlobalPosition.X + RoomSize.X);
        camera.LimitBottom = (int)(GlobalPosition.Y + RoomSize.Y);
    }
}