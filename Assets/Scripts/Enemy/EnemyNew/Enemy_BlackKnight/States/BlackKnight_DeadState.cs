using UnityEngine;

public class BlackKnight_DeadState : Enemy_DeadState
{
    private Collider2D col;

    public BlackKnight_DeadState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        col = enemy.GetComponent<Collider2D>();
    }

    public override void Enter()
    {
        if (col != null)
            col.enabled = false;

        anim.SetBool(animBoolName, true);
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;

        stateMachine.SwitchOffStateMachine();
        enemy.DestroyGameObjectWithDelay(5f);
    }
}
