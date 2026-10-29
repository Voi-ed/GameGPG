using UnityEngine;

public class Dragon_IdleState : DragonState
{
    public Dragon_IdleState(Dragon_Controller dragon, StateMachine stateMachine, string animBoolName) 
        : base(dragon, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        if (rb != null)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        if (input == null) return;

        // Dùng Kỹ Năng Hắc Hỏa Cầu khi nhấn Left Shift (Dash)
        if (input.Player.Dash.WasPressedThisFrame())
        {
            if (dragon.TryCastFireball())
                return;
        }

        // Dùng Kỹ Năng (Long Nộ Địa Ngục) khi nhấn Chuột Phải
        if (input.Player.RangeAttack.WasPressedThisFrame())
        {
            if (dragon.TryCastSkill())
                return;
        }

        // Rơi khỏi bệ khi không đứng trên mặt đất
        if (!dragon.isGrounded && rb != null && rb.linearVelocity.y < -0.1f)
        {
            stateMachine.ChangeState(dragon.jumpState);
            return;
        }

        // Nhảy khi nhấn Space
        if (input.Player.Jump.WasPressedThisFrame() && dragon.isGrounded)
        {
            stateMachine.ChangeState(dragon.jumpState);
            return;
        }

        // Tấn công khi nhấn Chuột Trái
        if (input.Player.Attack.WasPressedThisFrame())
        {
            stateMachine.ChangeState(dragon.attackState);
            return;
        }

        // Di chuyển khi có input A/D
        if (Mathf.Abs(dragon.moveInput.x) > 0.05f)
        {
            stateMachine.ChangeState(dragon.moveState);
            return;
        }
    }
}
