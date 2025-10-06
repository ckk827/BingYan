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
    private Timer attackTimer;
    private Vector2 jumpTarget;

    // BOSS房间边界
    private Rect2 roomBounds;

    // 动画精灵引用 - 使用EnemyBase中的EnemyAnim
    private AnimatedSprite2D monstroAnim;

    public override void _Ready()
    {
        // 先调用基类的_Ready
        base._Ready();

        // 获取玩家引用
        player = GetTree().Root.GetNode<Player>("root/player");

        // 设置BOSS房间边界（根据你的房间大小调整）
        roomBounds = new Rect2(GlobalPosition - new Vector2(200, 150), new Vector2(400, 300));

        // 获取动画精灵 - 使用基类中的EnemyAnim
        monstroAnim = EnemyAnim;

        // 创建计时器
        jumpTimer = new Timer();
        jumpTimer.WaitTime = jumpCooldown;
        jumpTimer.Timeout += StartJumpWindup;
        AddChild(jumpTimer);
        jumpTimer.Start();

        attackTimer = new Timer();
        AddChild(attackTimer);

        // 初始状态
        ChangeState(MonstroState.Idle);
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
                jumpTarget = player.GlobalPosition;
                // 确保跳跃目标在房间边界内
                jumpTarget = jumpTarget.Clamp(roomBounds.Position, roomBounds.End);
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

        // 根据玩家位置翻转精灵
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

        // 跳跃前摇结束后开始跳跃
        attackTimer.WaitTime = jumpWindup;
        attackTimer.Timeout += StartJump;
        attackTimer.Start();
    }

    private void StartJump()
    {
        ChangeState(MonstroState.Jumping);

        // 跳跃结束后落地
        float jumpDuration = GlobalPosition.DistanceTo(jumpTarget) / jumpSpeed;
        attackTimer.WaitTime = jumpDuration;
        attackTimer.Timeout += Land;
        attackTimer.Start();
    }

    private void Land()
    {
        ChangeState(MonstroState.Landing);

        // 落地后发射眼泪
        ShootTears();

        // 落地动画结束后回到移动状态
        attackTimer.WaitTime = 0.5f; // 落地动画持续时间
        attackTimer.Timeout += ReturnToMoving;
        attackTimer.Start();
    }

    private void ReturnToMoving()
    {
        ChangeState(MonstroState.Moving);

        // 重新开始跳跃计时
        jumpTimer.Start();
    }

    private void ShootTears()
    {
        if (tearScene == null || player == null) return;

        // 发射8个方向的眼泪
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.Pi / 4; // 45度间隔
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            var tear = tearScene.Instantiate() as Tear;
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
        base.TakeDamage(amount);

        // BOSS受伤时可能有特殊行为
        if (health > 0)
        {
            // 受伤时可能中断当前行动
            if (CurrentState == MonstroState.JumpWindup)
            {
                attackTimer.Stop();
                ChangeState(MonstroState.Moving);
                jumpTimer.Start(); // 重新开始跳跃计时
            }

            // 低血量时增加攻击频率
            if (health <= 3)
            {
                jumpTimer.WaitTime = jumpCooldown * 0.7f; // 减少30%冷却
            }

            // 播放受伤动画（如果有）
            if (monstroAnim.Animation != "hurt" && monstroAnim.HasAnimation("hurt"))
            {
                monstroAnim.Play("hurt");
                // 受伤动画后回到当前状态
                attackTimer.WaitTime = 0.3f;
                attackTimer.Timeout += () => {
                    if (CurrentState == MonstroState.Moving)
                        monstroAnim.Play("move");
                    else if (CurrentState == MonstroState.Idle)
                        monstroAnim.Play("idle");
                };
                attackTimer.Start();
            }
        }
    }

    protected override void Die()
    {
        // 停止所有计时器
        jumpTimer.Stop();
        attackTimer.Stop();

        // 触发死亡事件
        OnEnemyDied?.Invoke(this);

        // 播放死亡动画
        monstroAnim.Play("die");

        // 禁用移动和碰撞伤害
        EnableContactDamage = false;
        CanMove = false;
        Velocity = Vector2.Zero;
    }

    // 添加一个方法来处理房间边界设置
    public void SetRoomBounds(Rect2 bounds)
    {
        roomBounds = bounds;
    }
}