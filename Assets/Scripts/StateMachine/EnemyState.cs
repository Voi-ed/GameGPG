using UnityEngine;

public class EnemyState : EntityState
{
    protected Enemy enemy;

    private static readonly int BattleSpeedHash = Animator.StringToHash("battleAnimSpeedMultiplier");
    private static readonly int MoveSpeedHash = Animator.StringToHash("moveAnimSpeedMultiplier");
    private static readonly int XVelocityHash = Animator.StringToHash("xVelocity");

    private bool hasBattleSpeedParam;
    private bool hasMoveSpeedParam;
    private bool hasXVelocityParam;

    public EnemyState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
    {
        this.enemy = enemy;

        rb = enemy.rb;
        anim = enemy.anim;
        stats = enemy.stats;

        CacheAnimatorParameters();
    }

    private void CacheAnimatorParameters()
    {
        if (anim == null) return;

        foreach (var param in anim.parameters)
        {
            if (param.nameHash == BattleSpeedHash) hasBattleSpeedParam = true;
            else if (param.nameHash == MoveSpeedHash) hasMoveSpeedParam = true;
            else if (param.nameHash == XVelocityHash) hasXVelocityParam = true;
        }
    }

    public override void UpdateAnimationParameters()
    {
        base.UpdateAnimationParameters();

        if (anim == null) return;

        if (hasBattleSpeedParam && enemy != null && enemy.moveSpeed > 0)
        {
            float battleAnimSpeedMultiplier = enemy.battleMoveSpeed / enemy.moveSpeed;
            anim.SetFloat(BattleSpeedHash, battleAnimSpeedMultiplier);
        }

        if (hasMoveSpeedParam && enemy != null)
        {
            anim.SetFloat(MoveSpeedHash, enemy.moveAnimSpeedMultiplier);
        }

        if (hasXVelocityParam && rb != null)
        {
            anim.SetFloat(XVelocityHash, rb.linearVelocity.x);
        }
    }
}
