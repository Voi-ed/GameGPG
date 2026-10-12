using UnityEngine;

public class Satyr_DeadState : Enemy_DeadState
{
    private Enemy_Satyr satyr;

    public Satyr_DeadState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        satyr = enemy as Enemy_Satyr;
    }

    public override void Enter()
    {
        // Do not call base.Enter() because Enemy_DeadState disables anim and launches creep into the air

        Collider2D col = enemy.GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        anim.SetBool(animBoolName, true);
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        // Clean up active skill effects
        satyr?.StopFireBreathEmitter();
        if (satyr?.currentSpiritBomb != null)
        {
            satyr.currentSpiritBomb.InterruptAndExplode();
        }

        stateMachine.SwitchOffStateMachine();
        enemy.DestroyGameObjectWithDelay(7f);
    }

    public override void Update()
    {
        rb.linearVelocity = Vector2.zero;
    }
}
