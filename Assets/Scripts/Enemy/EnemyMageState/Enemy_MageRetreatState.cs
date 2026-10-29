using UnityEngine;

public class Enemy_MageRetreatState : EnemyState
{
    private Enemy_Mage enemyMage;
    private Vector3 startPosition;
    private Transform player;

    public Enemy_MageRetreatState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemyMage = enemy as Enemy_Mage;
    }

    public override void Enter()
    {
        base.Enter();

        if (player == null && enemyMage != null)
            player = enemyMage.GetPlayerTarget();

        startPosition = enemy.transform.position;

        rb.linearVelocity = new Vector2(enemyMage.retreatSpeed * -DirectionToPlayer(), 0);
        enemy.HandleFlip(DirectionToPlayer());
        enemy.MakeUntargetable(true);
        enemy.vfx.DoImageEchoEffect(1f);
    }

    public override void Update()
    {
        base.Update();

        bool reachedMaxDistance = Vector2.Distance(enemy.transform.position, startPosition) > enemyMage.retreatMaxDistance;

        if (reachedMaxDistance || enemyMage.CantMoveBackwards())
            stateMachine.ChangeState(enemyMage.mageSpellCastState);
    }

    public override void Exit()
    {
        base.Exit();
        enemy.vfx.StopImageEchoEffect();
        enemy.MakeUntargetable(true);
    }

    protected int DirectionToPlayer()
    {
        if (player == null && enemyMage != null)
            player = enemyMage.GetPlayerTarget();

        if (player == null)
            return enemy.facingDir;

        return player.position.x > enemy.transform.position.x ? 1 : -1;
    }
}
