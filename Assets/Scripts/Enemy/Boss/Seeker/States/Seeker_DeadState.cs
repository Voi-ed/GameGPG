using UnityEngine;

public class Seeker_DeadState : Enemy_DeadState
{
    private Enemy_Boss_Seeker seeker;

    public Seeker_DeadState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        seeker = enemy as Enemy_Boss_Seeker;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.SetVelocity(0f, 0f);
        enemy.EnableCounterWindow(false);

        // Vô hiệu hóa va chạm để không cản trở người chơi
        Collider2D col = enemy.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Rơi đồ boss
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
