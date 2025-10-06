using Godot;
using System.Collections.Generic;

public partial class GameState : Node
{
    public static GameState Instance;

    // 保存已清空的房间名
    public static HashSet<string> ClearedRooms = new HashSet<string>();

    // 保存每个房间的敌人是否已生成
    public static HashSet<string> SpawnedRooms = new HashSet<string>();

    public override void _Ready()
    {
        Instance = this;
    }
}
