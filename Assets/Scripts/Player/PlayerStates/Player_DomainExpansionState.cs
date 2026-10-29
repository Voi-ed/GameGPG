using UnityEngine;

public class Player_DomainExpansionState : PlayerState
{
    private Vector2 originalPosition;
    private float originalGravity;
    private float maxDistanceToGoUp;

    private bool isLevitating;
    private bool createdDomain;
    private float riseTimeoutTimer;
    private const float MAX_RISE_TIME = 0.75f; // Thời gian tối đa để bay lên trước khi bắt buộc Levitate

    public Player_DomainExpansionState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 1. Triệt tiêu ngay lập tức mọi hiệu ứng Knockback từ đòn đánh trước
        player.StopKnockback();

        originalPosition = player.transform.position;
        originalGravity = rb.gravityScale;
        maxDistanceToGoUp = GetAvalibaleRiseDistance();

        // 2. Miễn nhiễm sát thương tuyệt đối khi chuẩn bị mở Lãnh Địa
        player.health.SetCanTakeDamage(false);

        // 3. Tắt trọng lực ngay để bay thẳng đứng mượt mà, không bị gravity kéo trễ
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(0, player.riseSpeed);

        isLevitating = false;
        createdDomain = false;
        riseTimeoutTimer = MAX_RISE_TIME;
    }

    public override void Update()
    {
        base.Update();

        if (!isLevitating)
        {
            // Duy trì vận tốc bay lên đều đặn
            rb.linearVelocity = new Vector2(0, player.riseSpeed);
            riseTimeoutTimer -= Time.deltaTime;

            float currentDistance = Vector2.Distance(originalPosition, player.transform.position);

            // Đạt độ cao tối đa HOẶC hết thời gian bay lên (tránh kẹt trần nhà / vật cản / dính đòn trước đó)
            if (currentDistance >= maxDistanceToGoUp || riseTimeoutTimer <= 0f)
            {
                Levitate();
            }
        }
        else
        {
            skillManager.domainExpansion.DoSpellCasting();

            if (stateTimer < 0)
            {
                isLevitating = false;
                if (player.groundDetected)
                    stateMachine.ChangeState(player.idleState);
                else
                    stateMachine.ChangeState(player.fallState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        createdDomain = false;
        isLevitating = false;
        rb.gravityScale = originalGravity;
        player.health.SetCanTakeDamage(true);
    }

    private void Levitate()
    {
        isLevitating = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;

        stateTimer = skillManager.domainExpansion.GetDomainDuration();

        if (createdDomain == false)
        {
            createdDomain = true;
            if (AudioManager.instance != null)
                AudioManager.instance.PlayGlobalSFX("domain_expansion");

            skillManager.domainExpansion.CreateDomain();
        }
    }

    private float GetAvalibaleRiseDistance()
    {
        RaycastHit2D hit =
            Physics2D.Raycast(player.transform.position, Vector2.up, player.riseMaxDistance, player.whatIsGround);

        return hit.collider != null ? Mathf.Max(0, hit.distance - 1) : player.riseMaxDistance;
    }
}
