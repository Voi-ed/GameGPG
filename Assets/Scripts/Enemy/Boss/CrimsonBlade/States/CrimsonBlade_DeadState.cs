using UnityEngine;

public class CrimsonBlade_DeadState : Enemy_DeadState
{
    private Enemy_Boss_CrimsonBlade crimsonBlade;

    public CrimsonBlade_DeadState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        crimsonBlade = enemy as Enemy_Boss_CrimsonBlade;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.SetVelocity(0f, 0f);
        enemy.EnableCounterWindow(false);

        Collider2D col = enemy.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        Entity_DropManager dropManager = enemy.GetComponent<Entity_DropManager>();
        if (dropManager != null)
        {
            dropManager.DropItems();
        }
    }

    public override void Update()
    {
        base.Update();
        enemy.SetVelocity(0f, enemy.rb.linearVelocity.y);
    }
}
