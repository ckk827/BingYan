using Godot;
using System;
using System.Runtime.CompilerServices;
using static Godot.WebSocketPeer;

public partial class Player : CharacterBody2D
{
    
    [Export] public float moveSpeed = 5f;
    [Export] public int health = 6;
    [Export] public float tearCD = 0.3f;
    [Export] public float bombCD = 0.5f;
    [Export] public float invincibleDuration = 1f;
    [Export] private PackedScene tearScene;
    [Export] private PackedScene bombScene;
    [Export] public int bombCount = 1; // 玩家拥有的炸弹数
    [Export] public int keyCount = 0;
    [Export] public int energy = 0;
    [Export] public int ExtraDamage = 0;

    [Export] public ActiveItem CurrentActiveItem { get; private set; }
    [Export] public int MaxEnergy { get; private set; } = 0;

    //private string currentDir = "Down";
    private string currentState = "Stand";
    private string currentDirection = "Down";

    private AnimatedSprite2D feet; // 脚动画
    private AnimatedSprite2D head; // 头动画
    private GameOverScreen gameOverScreen; // 引用结束UI节点
    private Area2D pickupArea; //用于道具检测

    private Vector2 moveInput = Vector2.Zero;
    private Vector2 facingDirection = Vector2.Down;

    private float tearTimer = 0f;             // 射击计时器
    private float bombTimer = 0f;             // 炸弹计时器
    private float invincibleTimer = 0f;             // 无敌时间计时器

    private float activeItemTimer = 0f;
    private bool canUseActiveItem = true;

    //public event Action OnHealthChanged;
    //public event Action OnBombCountChanged;
    //public event Action OnKeyCountChanged;

    public override void _Ready()
    {
        feet = GetNode<AnimatedSprite2D>("feet"); 
        head = GetNode<AnimatedSprite2D>("head"); 
        gameOverScreen = GetParent().GetNode<GameOverScreen>("GameOverScreen");
        if (gameOverScreen == null) GD.Print("获取结束UI失败");

        //确保头显示在脚上
        feet.ZIndex = 0;
        head.ZIndex = 1;

        pickupArea = GetNode<Area2D>("PickupArea");
        pickupArea.AreaEntered += OnAreaEntered;
    }
    public override void _PhysicsProcess(double delta)
    {
        getSpeed();
        getDirection();
        feetAnimation();
        HeadAnimation();
        HandleShooting(delta); // 射击逻辑
        HandleBomb(delta); // 炸弹逻辑
        Invincibility( delta); // 无敌时间逻辑
        HandleActiveItem(delta);
        MoveAndSlide();
    }
    private void HandleActiveItem(double delta)
    {
        // 更新使用冷却
        if (!canUseActiveItem)
        {
            activeItemTimer -= (float)delta;
            if (activeItemTimer <= 0)
            {
                canUseActiveItem = true;
            }
        }
        
        // 检查使用主动道具输入
        if (Input.IsActionJustPressed("item") && 
            canUseActiveItem && 
            CurrentActiveItem != null && 
            energy >= CurrentActiveItem.EnergyCost)
        {
            UseActiveItem();
        }
    }
    
    // 使用主动道具
    private void UseActiveItem()
    {
        if (CurrentActiveItem != null && energy >= CurrentActiveItem.EnergyCost)
        {
            CurrentActiveItem.UseItem(this);
            energy -= CurrentActiveItem.EnergyCost;
            
            // 设置使用冷却
            canUseActiveItem = false;
            activeItemTimer = 0.5f;
            
            GD.Print($"使用了 {CurrentActiveItem.ItemName}. 剩余能量: {energy}");
        }
        else if (CurrentActiveItem != null && energy < CurrentActiveItem.EnergyCost)
        {
            GD.Print($"能量不足! 需要 {CurrentActiveItem.EnergyCost} 能量，当前只有 {energy}");
        }
    }
    
    // 拾取主动道具
    public void PickupActiveItem(ActiveItem item)
    {
        GD.Print($"尝试拾取: {item.ItemName}");
        
        // 如果已经有主动道具，先丢弃
        if (CurrentActiveItem != null)
        {
            GD.Print($"丢弃当前道具: {CurrentActiveItem.ItemName}");
            DropCurrentActiveItem();
        }
        
        // 拾取新道具
        CurrentActiveItem = item;
        CurrentActiveItem.PickupItem();
        
        // 设置能量上限
        MaxEnergy = CurrentActiveItem.MaxEnergy;
        energy = MaxEnergy; // 拾取时充满能量

        if (item.GetParent() != null)       //  先删除该节点
        {
            item.GetParent().RemoveChild(item);
        }
        if (CurrentActiveItem is Area2D area)
        {
            var collision = area.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
            if (collision != null) collision.Disabled = true;
        }
            // 重新设置父节点，使道具跟随玩家
            this.AddChild(CurrentActiveItem);
        CurrentActiveItem.GlobalPosition = GlobalPosition;
        
        GD.Print($"拾取了 {item.ItemName}. 能量上限: {MaxEnergy}, 当前能量: {energy}");
    }
    
    // 丢弃当前主动道具
    private void DropCurrentActiveItem()
    {
        if (CurrentActiveItem != null)
        {
            CurrentActiveItem.DropItem(GlobalPosition);
            CurrentActiveItem = null;
            MaxEnergy = 0;
            energy = 0;
            
            GD.Print("丢弃了主动道具");
        }
    }
    private void OnAreaEntered(Area2D area)
    {
        if (area is ActiveItem item)
        {
            if (!item.IsEquipped)
                CallDeferred(nameof(PickupActiveItem), item);
        }
    }
    private void getSpeed()
    {
        moveInput = Vector2.Zero;
        if (Input.IsActionPressed("right")) moveInput.X += 1;
        if (Input.IsActionPressed("left")) moveInput.X -= 1;
        if (Input.IsActionPressed("down")) moveInput.Y += 1;
        if (Input.IsActionPressed("up")) moveInput.Y -= 1;
        Velocity = moveInput.Normalized() * moveSpeed;
    }
    private void getDirection()
    {
        //
        if (Input.IsActionPressed("right")) facingDirection = Vector2.Right;
        if (Input.IsActionPressed("left")) facingDirection = Vector2.Left;
        if (Input.IsActionPressed("down")) facingDirection = Vector2.Down;
        if (Input.IsActionPressed("up")) facingDirection = Vector2.Up;
        //头部优先跟随射击方向
        if (Input.IsActionPressed("l_shoot")) facingDirection = Vector2.Left;
        if (Input.IsActionPressed("r_shoot")) facingDirection = Vector2.Right;
        if (Input.IsActionPressed("d_shoot")) facingDirection = Vector2.Down;
        if (Input.IsActionPressed("u_shoot")) facingDirection = Vector2.Up;
    }
    private void feetAnimation()
    {
        currentState = (Velocity == Vector2.Zero) ? "Stand" : "Walk";
        if (moveInput.X < 0) currentDirection = "Left";
        if (moveInput.X > 0) currentDirection = "Right";
        if (moveInput.Y < 0) currentDirection = "Up";
        if (moveInput.Y > 0) currentDirection = "Down";
        feet.Play("feet" + currentState + currentDirection);
    }
    private void HeadAnimation()
    {
        
        if (facingDirection == Vector2.Left) head.Play("headLeft");
        if (facingDirection == Vector2.Right) head.Play("headRight");
        if (facingDirection == Vector2.Up) head.Play("headUp");
        if (facingDirection == Vector2.Down) head.Play("headDown");
    }
    private void HandleShooting(double delta)
    {
        // 冷却时间递减
        if (tearTimer > 0)
            tearTimer -= (float)delta;

        // 检查射击输入
        Vector2 shootDir = Vector2.Zero;
        if (Input.IsActionPressed("l_shoot")) shootDir = Vector2.Left;
        if (Input.IsActionPressed("r_shoot")) shootDir = Vector2.Right;
        if (Input.IsActionPressed("d_shoot")) shootDir = Vector2.Down;
        if (Input.IsActionPressed("u_shoot")) shootDir = Vector2.Up;

        // 如果有射击输入并且冷却结束
        if (shootDir != Vector2.Zero && tearTimer <= 0)
        {
            Shoot(shootDir,ExtraDamage);
            tearTimer = tearCD; // 重置冷却
        }
    }

    private void Shoot(Vector2 shootDir,int extradamage)
    {
        var tear = (Tear)tearScene.Instantiate();
        tear.GlobalPosition = GlobalPosition;     // 从玩家位置发射
        tear.GetShootDirection(shootDir, Velocity , 0.5f);  //输入发射速度方向，玩家速度及其权重，计算出子弹初速度方向
        // 添加到场景
        tear.damage += extradamage;
        GetTree().CurrentScene.AddChild(tear);
    }
    private void HandleBomb(double delta)
    {
       if (bombTimer > 0)
        bombTimer -= (float)delta;
       else
    {
        // 判断是否还有可用炸弹
        if (Input.IsActionPressed("bomb") && bombCount > 0)
        {
            Bomb();
            bombTimer = bombCD;
        }
    }
    }
    private void Bomb()
{
    var bomb = (Bomb)bombScene.Instantiate();
    bomb.GlobalPosition = GlobalPosition;

    // 放置炸弹时减少数量
    bombCount--;

    GetTree().CurrentScene.AddChild(bomb);
}
    public void TakeDamage(int amount)
    {
        if (invincibleTimer < 0 || invincibleTimer == 0 )
        {
            health -= amount; // 扣血
            GD.Print($"Player took {amount} damage! Current health: {health}");

            if (health <= 0)
            {
                Die();
            }

            invincibleTimer = invincibleDuration;
        }
    }

    private void Die()
    {
        GD.Print("Player died!");
        if (gameOverScreen != null)
        {
            gameOverScreen.ShowResult(false); 
        }
        QueueFree(); 
        
    }

    private void Invincibility(double delta) // 无敌时间控制
    {
        if (invincibleTimer > 0)
        {
            invincibleTimer-=(float)delta;
            // 无敌时间人物闪烁
            if (Mathf.FloorToInt(invincibleTimer * 10) % 2 == 0)
                Visible = true;
            else
                Visible = false;
        }
        else
        {
            Visible = true; // 计时结束，恢复正常
        }
    }
}
