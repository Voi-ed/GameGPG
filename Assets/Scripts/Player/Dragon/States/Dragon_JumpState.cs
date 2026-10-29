using UnityEngine;

public class Dragon_JumpState : DragonState
{
    public Dragon_JumpState(Dragon_Controller dragon, StateMachine stateMachine, string animBoolName) 
        : base(dragon, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        if (dragon.isGrounded)
        {
            if (rb != null)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, dragon.JumpForce);

            if (AudioManager.instance != null)
                AudioManager.instance.PlayGlobalSFX("dragon_attack");
        }
    }

    public override void Update()
    {
        base.Update();

        // Di chuyển trên không
        float targetX = dragon.moveInput.x * dragon.MoveSpeed;
        if (rb != null)
            rb.linearVelocity = new Vector2(targetX, rb.linearVelocity.y);

        // Đánh trên không
        if (input != null && input.Player.Attack.WasPressedThisFrame())
        {
            stateMachine.ChangeState(dragon.attackState);
            return;
        }

        // Chạm đất
        if (dragon.isGrounded && rb.linearVelocity.y <= 0.1f)
        {
            stateMachine.ChangeState(dragon.idleState);
            return;
        }
    }
}
