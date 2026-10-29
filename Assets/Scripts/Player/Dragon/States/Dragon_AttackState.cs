using UnityEngine;

/// <summary>
/// State tấn công cho Dragon_Controller:
/// - Thực hiện đòn cào xé móng vuốt (Dragon_Attack).
/// - Tự động lướt nhẹ về phía trước khi vung đòn.
/// - Thoát về Idle khi kết thúc hoạt ảnh (CurrentStateTrigger).
/// </summary>
public class Dragon_AttackState : DragonState
{
    public int comboIndex => 1;

    private float attackStepTimer;
    private const float attackStepDuration = 0.18f;
    private const float forwardVelocity = 3.5f;

    public Dragon_AttackState(Dragon_Controller dragon, StateMachine stateMachine, string animBoolName) 
        : base(dragon, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        anim.SetBool("isAttacking", true);
        anim.SetInteger("comboIndex", 1);

        ApplyAttackStep();
    }

    public override void Update()
    {
        base.Update();

        HandleAttackStep();

        // Khi Animation Event 'CurrentStateTrigger' được gọi ở cuối frame thu đòn -> quay về Idle
        if (triggerCalled)
        {
            stateMachine.ChangeState(dragon.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        anim.SetBool("isAttacking", false);
    }

    private void ApplyAttackStep()
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(dragon.facingDir * forwardVelocity, rb.linearVelocity.y);

        attackStepTimer = attackStepDuration;
    }

    private void HandleAttackStep()
    {
        attackStepTimer -= Time.deltaTime;
        if (attackStepTimer <= 0 && rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }
}
