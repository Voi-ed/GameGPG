using UnityEngine;

public class BlackKnight_HeavyAttackState : EnemyState
{
    private Enemy_BlackKnight blackKnight;
    public int currentHeavyIndex { get; private set; } = 1;

    public BlackKnight_HeavyAttackState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        blackKnight = enemy as Enemy_BlackKnight;
    }

    public override void Enter()
    {
        base.Enter();

        blackKnight.hasHyperArmor = true;
        currentHeavyIndex = blackKnight.selectedHeavyAttackIndex;
        anim.SetInteger("heavyAttackIndex", currentHeavyIndex);

        // Apply motion depending on attack type
        if (currentHeavyIndex == 1) // Thrust
        {
            enemy.SetVelocity(blackKnight.thrustSpeed * enemy.facingDir, rb.linearVelocity.y);
        }
        else if (currentHeavyIndex == 2) // Spin
        {
            enemy.SetVelocity(0, rb.linearVelocity.y);
        }
        else if (currentHeavyIndex == 3) // Leap Slam
        {
            Transform target = enemy.player != null ? enemy.player : enemy.GetPlayerReference();
            float distToTarget = target != null ? Mathf.Abs(target.position.x - enemy.transform.position.x) : blackKnight.slamHorizontalSpeed;
            float dynamicLeapSpeed = Mathf.Clamp(distToTarget / 0.65f, blackKnight.slamHorizontalSpeed, 12f);
            enemy.SetVelocity(dynamicLeapSpeed * enemy.facingDir, blackKnight.slamLeapForce);
        }
    }

    public override void Update()
    {
        base.Update();

        if (currentHeavyIndex == 1)
        {
            // Decelerate thrust
            enemy.SetVelocity(Mathf.MoveTowards(rb.linearVelocity.x, 0, 15f * Time.deltaTime), rb.linearVelocity.y);
        }

        if (triggerCalled)
        {
            stateMachine.ChangeState(blackKnight.bkBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        blackKnight.hasHyperArmor = false;
        anim.SetInteger("heavyAttackIndex", 0);
        blackKnight.SetHeavyAttackOnCooldown();
        enemy.SetVelocity(0, rb.linearVelocity.y);
    }

    public void OnHeavyHit()
    {
        if (currentHeavyIndex == 2)
        {
            // 360 Spin Cleave hits all around
            blackKnight.PerformSurroundingAttack(blackKnight.heavyAttackScale, 2.5f);
        }
        else if (currentHeavyIndex == 3)
        {
            // Leap Slam creates fiery ground eruption with Hun0FX
            blackKnight.TriggerLeapSlamExplosion();
        }
        else
        {
            // Thrust or Slam with extra damage
            blackKnight.PerformAttackWithScale(blackKnight.heavyAttackScale);
        }
    }
}
