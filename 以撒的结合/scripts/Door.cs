using Godot;
using System;

public partial class Door : Area2D
{
    [Export] public string DoorName = "Top"; // Top / Bottom / Left / Right
    [Export] public NodePath TargetRoomPath;  // 目标房间路径
    [Export] public Vector2 EntranceOffset = Vector2.Zero; // 玩家出现位置偏移
    [Export] public bool IsEnabled = true; // 是否启用（房间设计时可关闭）
    [Export] public bool IsOpen = true;    // 是否打开（战斗时控制）

    private AnimatedSprite2D doorAnim;
    private CollisionShape2D doorCollider;

    public override void _Ready()
    {
        doorAnim = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        doorCollider = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        GD.Print($"[{Name}] Door Ready: anim={doorAnim != null}, collider={doorCollider != null}");
        BodyEntered += OnPlayerEntered;


            // UpdateDoorAppearance();
        
    }
    public void UpdateDoorAppearance()
    {
        GD.Print($"[{Name}] UpdateDoorAppearance: IsEnabled={IsEnabled}, IsOpen={IsOpen}");

        if (!IsEnabled)
        {
            GD.Print($"[{Name}] 门被禁用，不显示。");
            Visible = false;
            if (doorCollider != null)
                doorCollider.Disabled = true;
            return;
        }

        Visible = true;

        if (doorAnim != null)
        {
            string anim = IsOpen ? $"Door{DoorName}Open" : $"Door{DoorName}Close";
            GD.Print($"[{Name}] 播放动画: {anim}");
            doorAnim.Play(anim);
        }
        else
        {
            GD.Print($"[{Name}] doorAnim 为空！");
        }

        if (doorCollider != null)
        {
            doorCollider.Disabled = !IsEnabled || !IsOpen;
            GD.Print($"[{Name}] 碰撞体状态: Disabled={doorCollider.Disabled}");
        }
        else
        {
            GD.Print($"[{Name}] doorCollider 为空！");
        }
    }

    public void Open()
    {
        IsOpen = true;
       // UpdateDoorAppearance();
    }
    public void Close()
    {
        IsOpen = false;
       // UpdateDoorAppearance();
    }
    private void OnPlayerEntered(Node body)
    {
        if (!IsEnabled || !IsOpen)
        {
            GD.Print($"{Name}: 门未启用或未开启，无法传送");
            return;
        }
        if (body is not Player player)
        {
            GD.Print($"{Name}: 进入的不是玩家");
            return;
        }
        if (TargetRoomPath == null || TargetRoomPath.IsEmpty)
        {
            GD.Print($"{Name}: 没有设置目标房间路径");
            return;
        }

        GD.Print($"{Name}: 当前节点路径 = {GetPath()}");
        GD.Print($"{Name}: 目标房间路径 = {TargetRoomPath}");
        var targetRoom = GetNodeOrNull<Room>(TargetRoomPath);

        if (targetRoom == null)
        {
            GD.Print($"{Name}: 找不到目标房间");
            return;
        }
        // 找目标房间的相反门
        Vector2 targetPos = targetRoom.GetEntrancePosition(OppositeDirection(DoorName)) + EntranceOffset;
        player.GlobalPosition = targetPos;

        GD.Print($"{Name}: 玩家传送到房间 {targetRoom.Name} 的门 {OppositeDirection(DoorName)}");
    }
    private string OppositeDirection(string Doorname)
    {
        if (Doorname == "Top") return "Bottom";
        if (Doorname == "Bottom") return "Top"; 
        if (Doorname == "Right") return "Left"; 
        if (Doorname == "Left") return "Right"; 
        GD.Print("OppositeDirection: 未找到房间方向名"); return Doorname;

    }
}
