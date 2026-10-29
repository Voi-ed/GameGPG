using UnityEngine;

public class Dragon_MoveState : DragonState
{
    public Dragon_MoveState(Dragon_Controller dragon, StateMachine stateMachine, string animBoolName) 
        : base(dragon, stateMachine, animBoolName)
    {
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

        // Tấn công khi nhấn Chuột Trái (hủy di chuyển sang tấn công ngay lập tức)
        if (input.Player.Attack.WasPressedThisFrame())
        {
            stateMachine.ChangeState(dragon.attackState);
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

        // Di chuyển
        float targetX = dragon.moveInput.x * dragon.MoveSpeed;
        if (rb != null)
            rb.linearVelocity = new Vector2(targetX, rb.linearVelocity.y);

        // Về Idle khi thả phím di chuyển
        if (Mathf.Abs(dragon.moveInput.x) < 0.05f)
        {
            stateMachine.ChangeState(dragon.idleState);
            return;
        }
    }
}
