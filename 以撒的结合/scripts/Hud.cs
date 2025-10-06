//using Godot;
//using System;

//public partial class Hud : CanvasLayer
//{
//    [Export] public HBoxContainer HeartsContainer;
//    [Export] public HBoxContainer KeysContainer;
//    [Export] public HBoxContainer BombsContainer;

//    [Export] public Texture2D FullHeart;
//    [Export] public Texture2D HalfHeart;
//    [Export] public Texture2D KeyIcon;
//    [Export] public Texture2D BombIcon;

//    private Player player;
//    private Label keyLabel;
//    private Label bombLabel;
//    private TextureRect keyIconRect;
//    private TextureRect bombIconRect;

//    public override void _Ready()
//    {
//        // 找到玩家节点
//        player = GetTree().Root.GetNode<Player>("root/player");
//        if (player == null)
//        {
//            GD.PrintErr("HUD: 找不到玩家节点！");
//            return;
//        }

//        // 获取图标节点和文字
//        keyLabel = KeysContainer.GetNode<Label>("KeyLabel");
//        bombLabel = BombsContainer.GetNode<Label>("BombLabel");
//        keyIconRect = KeysContainer.GetNode<TextureRect>("KeyIcon");
//        bombIconRect = BombsContainer.GetNode<TextureRect>("BombIcon");

//        // 把 Inspector 拖进来的贴图赋值
//        if (KeyIcon != null)
//            keyIconRect.Texture = KeyIcon;
//        if (BombIcon != null)
//            bombIconRect.Texture = BombIcon;
//    }

//    public override void _Process(double delta)
//    {
//        if (player == null) return;

//        UpdateHearts();
//        UpdateKeys();
//        UpdateBombs();
//    }

//    private void UpdateHearts()
//    {
//        foreach (Node child in HeartsContainer.GetChildren())
//        {
//            child.QueueFree();
//        }

//        int health = player.health; // 每 2 血算一颗心
//        int fullHearts = health / 2;
//        bool hasHalf = health % 2 == 1;

//        for (int i = 0; i < fullHearts; i++)
//        {
//            var heart = new TextureRect
//            {
//                Texture = FullHeart,
//                ExpandMode = TextureRect.ExpandModeEnum.KeepSize,
//                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
//                CustomMinimumSize = new Vector2(16, 16)
//            };
//            HeartsContainer.AddChild(heart);
//        }

//        if (hasHalf)
//        {
//            var half = new TextureRect
//            {
//                Texture = HalfHeart,
//                ExpandMode = TextureRect.ExpandModeEnum.KeepSize,
//                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
//                CustomMinimumSize = new Vector2(16, 16)
//            };
//            HeartsContainer.AddChild(half);
//        }
//    }

//    private void UpdateKeys()
//    {
//        keyLabel.Text = $"x {player.keyCount}";
//    }

//    private void UpdateBombs()
//    {
//        bombLabel.Text = $"x {player.bombCount}";
//    }
//}
using Godot;
using System;

public partial class Hud : CanvasLayer
{
    [Export] public HBoxContainer HeartsContainer;
    [Export] public HBoxContainer KeysContainer;
    [Export] public HBoxContainer BombsContainer;

    [Export] public Texture2D FullHeart;
    [Export] public Texture2D HalfHeart;
    [Export] public Texture2D EmptyHeart;
    [Export] public Texture2D KeyIcon;
    [Export] public Texture2D BombIcon;

    private Player player;
    private Label keyLabel;
    private Label bombLabel;
    private TextureRect keyIconRect;
    private TextureRect bombIconRect;

    // 存储心形节点的数组
    private TextureRect[] heartTextures;

    public override void _Ready()
    {
        GD.Print("HUD: _Ready() 开始");

        // 找到玩家节点
        player = GetTree().Root.GetNode<Player>("root/player");
        if (player == null)
        {
            GD.PrintErr("HUD: 找不到玩家节点！");
            // 尝试其他可能的路径
            player = GetNodeOrNull<Player>("/root/root/player");
            if (player == null)
            {
                GD.PrintErr("HUD: 使用备用路径也找不到玩家节点！");
                return;
            }
        }
        GD.Print($"HUD: 找到玩家节点，生命值: {player.health}");

        // 检查容器
        if (HeartsContainer == null)
        {
            GD.PrintErr("HUD: HeartsContainer 为 null!");
        }
        else
        {
            GD.Print($"HUD: HeartsContainer 找到，子节点数: {HeartsContainer.GetChildCount()}");
        }

        // 初始化心形节点数组
        InitializeHearts();

        // 检查其他UI元素
        if (KeysContainer != null)
        {
            keyLabel = KeysContainer.GetNode<Label>("KeyLabel");
            keyIconRect = KeysContainer.GetNode<TextureRect>("KeyIcon");
            GD.Print($"HUD: KeyLabel: {keyLabel != null}, KeyIcon: {keyIconRect != null}");
        }
        else
        {
            GD.PrintErr("HUD: KeysContainer 为 null!");
        }

        if (BombsContainer != null)
        {
            bombLabel = BombsContainer.GetNode<Label>("BombLabel");
            bombIconRect = BombsContainer.GetNode<TextureRect>("BombIcon");
            GD.Print($"HUD: BombLabel: {bombLabel != null}, BombIcon: {bombIconRect != null}");
        }
        else
        {
            GD.PrintErr("HUD: BombsContainer 为 null!");
        }

        // 检查纹理
        GD.Print($"HUD: 纹理 - FullHeart: {FullHeart != null}, HalfHeart: {HalfHeart != null}, EmptyHeart: {EmptyHeart != null}");
        GD.Print($"HUD: 纹理 - KeyIcon: {KeyIcon != null}, BombIcon: {BombIcon != null}");

        // 设置图标
        if (KeyIcon != null && keyIconRect != null)
            keyIconRect.Texture = KeyIcon;
        if (BombIcon != null && bombIconRect != null)
            bombIconRect.Texture = BombIcon;

        GD.Print("HUD: _Ready() 完成");
    }

    private void InitializeHearts()
    {
        if (HeartsContainer == null)
        {
            GD.PrintErr("HUD: HeartsContainer 未设置！");
            return;
        }

        // 获取所有心形子节点
        var heartNodes = HeartsContainer.GetChildren();
        heartTextures = new TextureRect[heartNodes.Count];

        for (int i = 0; i < heartNodes.Count; i++)
        {
            heartTextures[i] = heartNodes[i] as TextureRect;
            if (heartTextures[i] != null)
            {
                // 设置心形节点的基本属性
                heartTextures[i].ExpandMode = TextureRect.ExpandModeEnum.KeepSize;
                heartTextures[i].StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                heartTextures[i].CustomMinimumSize = new Vector2(16, 16);
            }
        }

        GD.Print($"HUD: 初始化了 {heartTextures.Length} 个心形节点");
    }

    public override void _Process(double delta)
    {
        if (player == null)
        {
            // 尝试重新获取玩家节点
            player = GetTree().Root.GetNodeOrNull<Player>("root/player");
            if (player == null) return;
        }

        // 直接更新所有UI元素，不检查变化
        UpdateHearts();
        UpdateKeys();
        UpdateBombs();
    }

    private void UpdateHearts()
    {
        if (heartTextures == null || heartTextures.Length == 0)
        {
            // 如果心形数组未初始化，尝试初始化
            InitializeHearts();
            return;
        }

        if (FullHeart == null || HalfHeart == null || EmptyHeart == null)
        {
            GD.PrintErr("HUD: 心形纹理未设置！");
            return;
        }

        if (player == null) return;

        int health = player.health;
        int maxHearts = heartTextures.Length;

        for (int i = 0; i < maxHearts; i++)
        {
            if (heartTextures[i] == null) continue;

            // 计算当前心形应该显示的状态
            int heartHealth = health - i * 2;

            if (heartHealth >= 2)
            {
                // 完整的心
                heartTextures[i].Texture = FullHeart;
                heartTextures[i].Visible = true;
            }
            else if (heartHealth == 1)
            {
                // 半心
                heartTextures[i].Texture = HalfHeart;
                heartTextures[i].Visible = true;
            }
            else //if (heartHealth <= 0 && i < (health + 1) / 2)
            {
                // 空心（已损失的心）
                heartTextures[i].Texture = EmptyHeart;
                heartTextures[i].Visible = true;
            }
            //else
            //{
            //    // 隐藏多余的心形
            //    heartTextures[i].Visible = false;
            //}
        }
    }

    private void UpdateKeys()
    {
        if (keyLabel != null && player != null)
            keyLabel.Text = $": {player.keyCount}";
    }

    private void UpdateBombs()
    {
        if (bombLabel != null && player != null)
            bombLabel.Text = $": {player.bombCount}";
    }
}