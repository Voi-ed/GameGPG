using UnityEngine;

public class Dragon_DeadState : DragonState
{
    public Dragon_DeadState(Dragon_Controller dragon, StateMachine stateMachine, string animBoolName) 
        : base(dragon, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        anim.SetBool("isDead", true);

        if (AudioManager.instance != null)
            AudioManager.instance.PlayGlobalSFX("dragon_death");
    }

    public override void Update()
    {
        base.Update();
    }
}
