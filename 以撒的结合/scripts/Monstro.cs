using Godot;
using System;

public partial class Monstro : EnemyBase
{
    [Export] public float jumpCooldown = 3.0f; // 跳跃冷却时间
    [Export] public float jumpWindup = 1.0f;   // 跳跃前摇时间
    [Export] public float jumpSpeed = 400f;    // 跳跃速度
    [Export] public int jumpDamage = 2;        // 跳跃伤害
    [Export] public int tearDamage = 1;        // 眼泪伤害
    [Export] public PackedScene tearScene;     // 眼泪场景

    private const float ROOM_WIDTH = 1280f;
    private const float ROOM_HEIGHT = 640f;

    // BOSS状态
    public enum MonstroState
    {
        Idle,       // 空闲
        Moving,     // 移动
        JumpWindup, // 跳跃前摇
        Jumping,    // 跳跃中
        Landing     // 落地
    }

    [Export] public MonstroState CurrentState = MonstroState.Idle;

    private Player player;
    private Timer jumpTimer;
    private Timer jumpWindupTimer;
    private Timer jumpDurationTimer;
    private Timer landTimer;
    private Timer hurtTimer;
    private Vector2 jumpTarget;

    [Export] public int MaxHealth = 50;


    private GameOverScreen gameOverScreen; // 引用结束UI节点


    private Rect2 roomBounds;

    private AnimatedSprite2D monstroAnim;

    public override void _Ready()
    {
        // 获取玩家引用
        player = GetTree().Root.GetNode<Player>("root/player");
        if (player == null)
        {
            GD.PrintErr("Monstro: 找不到玩家节点！");
        }
        gameOverScreen = GetParent().GetParent().GetParent().GetNode<GameOverScreen>("GameOverScreen");


        SetupRoomBounds();

        // 获取动画精灵 - 直接获取MonstroAnim节点
        monstroAnim = GetNode<AnimatedSprite2D>("MonstroAnim");
        if (monstroAnim == null)
        {
            GD.PrintErr("Monstro: 找不到MonstroAnim节点！");
        }

        // 创建专用计时器
        jumpTimer = new Timer();
        jumpTimer.WaitTime = jumpCooldown;
        jumpTimer.Timeout += StartJumpWindup;
        jumpTimer.OneShot = false; // 设置为循环计时器
        AddChild(jumpTimer);

        jumpWindupTimer = new Timer();
        jumpWindupTimer.OneShot = true;
        jumpWindupTimer.Timeout += StartJump;
        AddChild(jumpWindupTimer);

        jumpDurationTimer = new Timer();
        jumpDurationTimer.OneShot = true;
        jumpDurationTimer.Timeout += Land;
        AddChild(jumpDurationTimer);

        landTimer = new Timer();
        landTimer.OneShot = true;
        landTimer.Timeout += ReturnToMoving;
        AddChild(landTimer);

        hurtTimer = new Timer();
        hurtTimer.OneShot = true;
        hurtTimer.Timeout += OnHurtAnimationFinished;
        AddChild(hurtTimer);

        MaxHealth = health;       // 获取最大血量用于血条显示
        var hud = GetTree().Root.GetNodeOrNull<Hud>("root/HUD");
        if (hud != null)
            hud.ShowBossBar("Monstro", MaxHealth);
        else GD.Print("Monstro: HUD节点获取失败");

        // 初始状态
        ChangeState(MonstroState.Moving); // 直接进入移动状态
        jumpTimer.Start(); // 开始跳跃计时
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!CanMove || player == null) return;

        switch (CurrentState)
        {
            case MonstroState.Idle:
                // 空闲状态，可以转向玩家
                LookAtPlayer();
                break;

            case MonstroState.Moving:
                // 缓慢接近玩家
                MoveTowardsPlayer();
                MoveAndSlide();
                break;

            case MonstroState.JumpWindup:
                // 跳跃前摇，不移动但转向玩家
                LookAtPlayer();
                Velocity = Vector2.Zero;
                break;

            case MonstroState.Jumping:
                // 跳跃移动
                JumpMovement();
                MoveAndSlide();
                break;

            case MonstroState.Landing:
                // 落地状态，不移动
                Velocity = Vector2.Zero;
                break;
        }
    }

    private void SetupRoomBounds()
    {
        // 获取Room节点 
        Node roomNode = GetParent()?.GetParent();

        if (roomNode != null && roomNode is Node2D room2D)
        {
    
            Vector2 roomPosition = room2D.GlobalPosition;
            Vector2 offset = new Vector2(ROOM_WIDTH, ROOM_HEIGHT);
            roomBounds = new Rect2(roomPosition- offset, offset);
            GD.Print($"Monstro: 使用Room边界 - 位置: {roomPosition}, 尺寸: {ROOM_WIDTH}x{ROOM_HEIGHT}");
        }
        else
        {
            // 备用方案：使用Monstro的当前位置和默认边界
            roomBounds = new Rect2(GlobalPosition - new Vector2(ROOM_WIDTH / 2, ROOM_HEIGHT / 2),
                                 new Vector2(ROOM_WIDTH, ROOM_HEIGHT));
            GD.PrintErr("Monstro: 无法找到Room节点，使用备用边界");
        }
    }

    private void ChangeState(MonstroState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        // 根据状态播放对应动画
        switch (newState)
        {
            case MonstroState.Idle:
                monstroAnim.Play("idle");
                break;

            case MonstroState.Moving:
                monstroAnim.Play("move");
                break;

            case MonstroState.JumpWindup:
                monstroAnim.Play("jump_windup");
                // 设置跳跃目标 - 玩家当前位置
                if (player != null)
                {
                    jumpTarget = player.GlobalPosition;
                    // 确保跳跃目标在房间边界内
                 //   jumpTarget = jumpTarget.Clamp(roomBounds.Position, roomBounds.End);
                }
                break;

            case MonstroState.Jumping:
                monstroAnim.Play("jump");
                // 跳跃期间增加伤害
                ContactDamage = jumpDamage;
                break;

            case MonstroState.Landing:
                monstroAnim.Play("land");
                // 恢复普通碰撞伤害
                ContactDamage = 1;
                break;
        }

        GD.Print($"Monstro状态改变: {newState}");
    }

    private void LookAtPlayer()
    {
        if (player == null) return;

        // 根据玩家位置翻转动画
        if (player.GlobalPosition.X < GlobalPosition.X)
        {
            monstroAnim.FlipH = true;
        }
        else
        {
            monstroAnim.FlipH = false;
        }
    }

    private void MoveTowardsPlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
        Velocity = direction * moveSpeed;

        LookAtPlayer();
    }

    private void JumpMovement()
    {
        Vector2 direction = (jumpTarget - GlobalPosition).Normalized();
        Velocity = direction * jumpSpeed;

        // 检查是否到达跳跃目标
        if (GlobalPosition.DistanceTo(jumpTarget) < 10f)
        {
            Land();
        }
    }

    private void StartJumpWindup()
    {
        if (CurrentState != MonstroState.Idle && CurrentState != MonstroState.Moving) return;

        ChangeState(MonstroState.JumpWindup);

        // 使用专用计时器，避免信号连接问题
        jumpWindupTimer.WaitTime = jumpWindup;
        jumpWindupTimer.Start();
    }

    private void StartJump()
    {
        ChangeState(MonstroState.Jumping);

        // 跳跃结束后落地
        float jumpDuration = GlobalPosition.DistanceTo(jumpTarget) / jumpSpeed;
        jumpDurationTimer.WaitTime = jumpDuration;
        jumpDurationTimer.Start();
    }

    private void Land()
    {
        ChangeState(MonstroState.Landing);

        // 落地后发射眼泪
        ShootTears();

        landTimer.WaitTime = 0.5f; // 落地动画持续时间
        landTimer.Start();
    }

    private void ReturnToMoving()
    {
        ChangeState(MonstroState.Moving);

    }

    private void ShootTears()
    {
        if (tearScene == null || player == null) return;

        // 发射8个方向的眼泪
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.Pi / 4; // 45度间隔
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            var tear = tearScene.Instantiate() as TearEnemy;
            if (tear != null)
            {
                GetParent().AddChild(tear);
                tear.GlobalPosition = GlobalPosition;
                tear.SetDirection(direction);
                tear.Damage = tearDamage;
            }
        }
    }

    public override void TakeDamage(int amount)
    {
        health -= amount;
        GD.Print($"{Name} took {amount} damage, health = {health}");

        var hud = GetTree().Root.GetNodeOrNull<Hud>("root/HUD");
        if (hud != null)
            hud.UpdateBossBar(health);
        // 血量低时特殊行为
        if (health > 0)
        {
  
            if (CurrentState == MonstroState.JumpWindup)
            {
                jumpWindupTimer.Stop();
                ChangeState(MonstroState.Moving);
        
            }

            // 低血量时增加攻击频率
            if (health <= 3)
            {
                jumpTimer.WaitTime = jumpCooldown * 0.7f; // 减少30%冷却
            }

       
            if (monstroAnim.Animation != "hurt")
            {
               // monstroAnim.Play("hurt");
                // 受伤动画后回到当前状态
                hurtTimer.WaitTime = 0.3f;
                hurtTimer.Start();
            }
        }
        else
        {
            Die();
            gameOverScreen.ShowResult(true);
        }
    }

    private void OnHurtAnimationFinished()
    {
        if (CurrentState == MonstroState.Moving)
            monstroAnim.Play("move");
        else if (CurrentState == MonstroState.Idle)
            monstroAnim.Play("idle");
    }

    protected override void Die()
    {
        // 停止所有计时器
        jumpTimer.Stop();
        jumpWindupTimer.Stop();
        jumpDurationTimer.Stop();
        landTimer.Stop();
        hurtTimer.Stop();

        // 触发死亡事件
       InvokeOnEnemyDied();

        monstroAnim.Play("die");

        // 禁用移动和碰撞伤害
        EnableContactDamage = false;
        CanMove = false;
        Velocity = Vector2.Zero;
    }

    public void SetRoomBounds(Rect2 bounds)
    {
        roomBounds = bounds;
    }
}