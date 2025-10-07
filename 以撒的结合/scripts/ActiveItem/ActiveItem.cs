using Godot;

public partial class ActiveItem : Area2D
{
    [Export] public string ItemName { get; set; } = "未知道具";
    [Export] public string Description { get; set; } = "没有描述";
    [Export] public int EnergyCost { get; set; } = 1; // 使用所需的能量
    [Export] public int MaxEnergy { get; set; } = 6;  // 能量上限
    [Export] public Texture2D Icon { get; set; }

    // 道具状态
    public bool IsEquipped { get; set; } = false;

    // 信号：当道具被使用时
    [Signal]
    public delegate void ItemUsedEventHandler();

    // 信号：当道具被丢弃时
    [Signal]
    public delegate void ItemDroppedEventHandler();

    public override void _Ready()
    {
        // 设置碰撞层为道具层
      //  CollisionLayer = 5; // 第5层是道具层
       // CollisionMask = 1;  // 检测玩家层（第1层）

        // 添加碰撞形状（如果不存在）
        if (GetChildCount() == 0 || GetNodeOrNull<CollisionShape2D>("CollisionShape2D") == null)
        {
            var collision = new CollisionShape2D();
            collision.Name = "CollisionShape2D";
            var shape = new CircleShape2D();
            shape.Radius = 8;
            collision.Shape = shape;
            AddChild(collision);
        }
    }

    public virtual void UseItem(Player player)
    {
        GD.Print($"使用 {ItemName}");
        EmitSignal(SignalName.ItemUsed);
    }

    // 丢弃道具
    public virtual void DropItem(Vector2 position)
    {
        IsEquipped = false;
        GlobalPosition = position;

        CollisionMask = 1; // 检测玩家层

        Visible = true;

        EmitSignal(SignalName.ItemDropped);
    }

    public virtual void PickupItem()
    {
        IsEquipped = true;

        // 禁用碰撞检测
        CollisionMask = 0;

        // 隐藏道具
        Visible = false;

        GD.Print($"{ItemName} 被拾取");
    }
}