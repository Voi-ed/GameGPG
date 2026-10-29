using UnityEngine;

public class Enemy_MageSpellCastState : EnemyState
{
    private Enemy_Mage enemyMage;

    private float recoveryTimer = 0.4f;

    public Enemy_MageSpellCastState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemyMage = enemy as Enemy_Mage;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.MakeUntargetable(true);

        enemyMage.SetVelocity(0, 0);
        enemyMage.SetSpellCastPerformed(false);
        recoveryTimer = 0.4f;
        stateTimer = 4.0f; // Timeout an toàn tối đa phòng hờ

        Transform target = enemyMage.GetPlayerTarget();
        if (target != null)
        {
            int dir = target.position.x > enemyMage.transform.position.x ? 1 : -1;
            enemyMage.HandleFlip(dir);
        }
    }

    public override void Update()
    {
        base.Update();

        if (enemyMage.spellCastPerformed)
        {
            anim.SetBool("spellCast_performed", true);
            recoveryTimer -= Time.deltaTime;
        }

        // Khi bắn xong: Chuyển về battleState ngay khi nhận trigger animation hoặc sau 0.4s phục hồi
        if (triggerCalled || (enemyMage.spellCastPerformed && recoveryTimer <= 0) || stateTimer <= 0)
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        anim.SetBool("spellCast_performed", false);

        if (enemyMage != null)
        {
            enemyMage.lastTimeCastSpell = Time.time;
        }
    }
}
