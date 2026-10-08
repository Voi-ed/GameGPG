using UnityEngine;

public class BlackKnight_ComboAttackState : EnemyState
{
    private Enemy_BlackKnight blackKnight;
    public int currentComboIndex { get; private set; } = 1;
    private int targetComboHits;
    private bool hitConnectedInCombo;

    public BlackKnight_ComboAttackState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        blackKnight = enemy as Enemy_BlackKnight;
    }

    public override void Enter()
    {
        base.Enter();
        SyncAttackSpeed();

        currentComboIndex = 1;
        anim.SetInteger("comboIndex", currentComboIndex);
        hitConnectedInCombo = false;

        if (blackKnight.combat != null)
            blackKnight.combat.OnDoingDamage += HandleDamageDealt;

        // Dynamic combo depth: Enraged always aims for full combo, otherwise randomize
        if (blackKnight.isEnraged)
        {
            targetComboHits = blackKnight.maxComboHits;
        }
        else
        {
            targetComboHits = Random.Range(blackKnight.minComboHits, blackKnight.maxComboHits + 1);
        }

        // Distance-aware initial forward momentum
        float dist = DistanceToPlayer();
        float stepSpeed = dist < 1.2f ? blackKnight.comboStepSpeed * 0.4f : blackKnight.comboStepSpeed;
        enemy.SetVelocity(stepSpeed * enemy.facingDir, rb.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        // Slow down smoothly after step
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

        // 1. Check if player fled out of melee reach
        if (dist > blackKnight.comboMaxFollowDistance)
        {
            // If Heavy Attack is ready, punish the fleeing player with a gap-closer (Thrust or Leap Slam)
            if (blackKnight.CanHeavyAttack() && Random.value < blackKnight.branchToHeavyOnFleeChance)
            {
                int heavyChoice = dist > 4.5f ? 3 : 1;
                blackKnight.SetHeavyAttackIndex(heavyChoice);
                stateMachine.ChangeState(blackKnight.bkHeavyAttackState);
                return;
            }

            // Otherwise, abort combo cleanly to pursue in BattleState
            stateMachine.ChangeState(blackKnight.bkBattleState);
            return;
        }

        // 2. Check if player flanked behind
        if (playerBehind)
        {
            // Close flank punish: Branch into 360 Spin Cleave
            if (dist < 3f && blackKnight.CanHeavyAttack() && Random.value < blackKnight.branchToSpinOnFlankChance)
            {
                blackKnight.SetHeavyAttackIndex(2);
                stateMachine.ChangeState(blackKnight.bkHeavyAttackState);
                return;
            }

            // Otherwise re-orient (Flip) to face the player
            enemy.Flip();
        }

        // 3. Check combo limit
        if (currentComboIndex >= targetComboHits || currentComboIndex >= blackKnight.maxComboHits)
        {
            stateMachine.ChangeState(blackKnight.bkBattleState);
            return;
        }

        // 4. Hit 4 (Counterable finisher) commitment check
        if (currentComboIndex == 3)
        {
            bool shouldDoFinisher = hitConnectedInCombo || blackKnight.isEnraged || Random.value < 0.4f;
            if (!shouldDoFinisher)
            {
                stateMachine.ChangeState(blackKnight.bkBattleState);
                return;
            }
        }

        // 5. Advance to next combo slash
        currentComboIndex++;
        triggerCalled = false;
        anim.SetInteger("comboIndex", currentComboIndex);

        // Forward lunge on each combo hit (scaled if already very close)
        float stepSpeed = dist < 1.2f ? blackKnight.comboStepSpeed * 0.4f : blackKnight.comboStepSpeed;
        enemy.SetVelocity(stepSpeed * enemy.facingDir, rb.linearVelocity.y);

        // Combo 4 is heavy finisher with counter window alert
        if (currentComboIndex == 4)
        {
            enemy.EnableCounterWindow(true);
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (blackKnight != null && blackKnight.combat != null)
            blackKnight.combat.OnDoingDamage -= HandleDamageDealt;

        currentComboIndex = 0;
        anim.SetInteger("comboIndex", 0);
        enemy.EnableCounterWindow(false);
        enemy.SetVelocity(0, rb.linearVelocity.y);

        blackKnight.bkBattleState?.ResetAttackCooldown();
    }

    public void OnComboHit()
    {
        blackKnight.PerformAttackWithScale(blackKnight.comboAttackScale);

        if (currentComboIndex == 4)
        {
            enemy.EnableCounterWindow(false);
        }
    }

    private void HandleDamageDealt(AttackData attackData, Transform target)
    {
        hitConnectedInCombo = true;
    }

    private Transform GetPlayer()
    {
        if (enemy.player != null)
            return enemy.player;

        return enemy.GetPlayerReference();
    }

    private float DistanceToPlayer()
    {
        Transform target = GetPlayer();
        if (target == null)
            return float.MaxValue;

        return Mathf.Abs(target.position.x - enemy.transform.position.x);
    }

    private int DirectionToPlayer()
    {
        Transform target = GetPlayer();
        if (target == null)
            return 0;

        return target.position.x > enemy.transform.position.x ? 1 : -1;
    }
}