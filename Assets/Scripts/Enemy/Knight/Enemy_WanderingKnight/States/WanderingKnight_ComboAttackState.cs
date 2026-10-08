using UnityEngine;

public class WanderingKnight_ComboAttackState : EnemyState
{
    private Enemy_WanderingKnight wanderingKnight;
    public int currentComboIndex { get; private set; } = 1;

    public WanderingKnight_ComboAttackState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        wanderingKnight = enemy as Enemy_WanderingKnight;
    }

    public override void Enter()
    {
        base.Enter();
        SyncAttackSpeed();

        currentComboIndex = 1;
        anim.SetInteger("comboIndex", currentComboIndex);

        // Initial forward step
        float dist = DistanceToPlayer();
        float stepSpeed = dist < 1.2f ? wanderingKnight.comboStepSpeed * 0.4f : wanderingKnight.comboStepSpeed;
        enemy.SetVelocity(stepSpeed * enemy.facingDir, rb.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        // Smoothly slow down forward momentum
        enemy.SetVelocity(Mathf.MoveTowards(rb.linearVelocity.x, 0, 8f * Time.deltaTime), rb.linearVelocity.y);

        if (triggerCalled)
        {
            EvaluateNextComboAction();
        }
    }

    private void EvaluateNextComboAction()
    {
        float dist = DistanceToPlayer();
        int dirToPlayer = DirectionToPlayer();
        bool playerBehind = (dirToPlayer != 0 && dirToPlayer != enemy.facingDir);

        // Player fled out of melee reach
        if (dist > 3.5f)
        {
            if (wanderingKnight.CanThrust())
            {
                stateMachine.ChangeState(wanderingKnight.wkThrustState);
                return;
            }

            stateMachine.ChangeState(wanderingKnight.wkBattleState);
            return;
        }

        // Player flanked behind
        if (playerBehind)
        {
            enemy.Flip();
        }

        // Combo finished
        if (currentComboIndex >= wanderingKnight.maxComboHits)
        {
            stateMachine.ChangeState(wanderingKnight.wkBattleState);
            return;
        }

        // Advance to next combo hit
        currentComboIndex++;
        triggerCalled = false;
        anim.SetInteger("comboIndex", currentComboIndex);

        // Forward step for next slash
        float stepSpeed = dist < 1.2f ? wanderingKnight.comboStepSpeed * 0.4f : wanderingKnight.comboStepSpeed;
        enemy.SetVelocity(stepSpeed * enemy.facingDir, rb.linearVelocity.y);

        // Hit 3 is the heavy finisher: open counter window for player!
        if (currentComboIndex == 3)
        {
            enemy.EnableCounterWindow(true);
        }
    }

    public override void Exit()
    {
        base.Exit();

        currentComboIndex = 0;
        anim.SetInteger("comboIndex", 0);
        enemy.EnableCounterWindow(false);
        enemy.SetVelocity(0, rb.linearVelocity.y);

        wanderingKnight.wkBattleState?.ResetAttackCooldown();
    }

    public void OnComboHit()
    {
        wanderingKnight.PerformAttackWithScale(wanderingKnight.comboAttackScale);

        if (currentComboIndex == 3)
        {
            enemy.EnableCounterWindow(false);
        }
    }

    private float DistanceToPlayer()
    {
        Transform target = enemy.player != null ? enemy.player : enemy.GetPlayerReference();
        if (target == null) return 99f;
        return Vector2.Distance(enemy.transform.position, target.position);
    }

    private int DirectionToPlayer()
    {
        Transform target = enemy.player != null ? enemy.player : enemy.GetPlayerReference();
        if (target == null) return enemy.facingDir;
        return target.position.x > enemy.transform.position.x ? 1 : -1;
    }
}
