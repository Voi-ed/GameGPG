using UnityEngine;

/// <summary>
/// Lớp cơ sở (Base State) cho hệ thống State Machine của Quái Rồng (Dragon_Controller).
/// Kế thừa trực tiếp từ EntityState để đồng bộ 100% kiến trúc chuẩn của dự án.
/// </summary>
public abstract class DragonState : EntityState
{
    protected Dragon_Controller dragon;
    protected PlayerInputSet input => dragon != null ? dragon.input : null;

    public DragonState(Dragon_Controller dragon, StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
    {
        this.dragon = dragon;
        this.anim = dragon.anim;
        this.rb = dragon.rb;
    }

    public override void Enter()
    {
        if (anim == null && dragon != null)
            anim = dragon.anim;
        if (rb == null && dragon != null)
            rb = dragon.rb;

        base.Enter();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void UpdateAnimationParameters()
    {
        base.UpdateAnimationParameters();
        if (anim != null && rb != null)
        {
            anim.SetFloat("yVelocity", rb.linearVelocity.y);
            anim.SetBool("isGrounded", dragon.isGrounded);
        }
    }
}
