using UnityEngine;

public class SkillObject_Sword : SkillObject_Base
{
    protected Skill_SwordThrow swordManager;

    protected Transform playerTransform;
    protected bool shouldComeback;
    protected float comebackSpeed = 20;
    protected float maxAllowedDistance = 25;
    protected float comebackTimer;
    protected const float maxComebackDuration = 3f;

    protected virtual void Update()
    {
        if (shouldComeback == false && rb != null && rb.simulated)
            transform.right = rb.linearVelocity;

        HandleComeback();
    }

    public virtual void SetupSword(Skill_SwordThrow swordManager, Vector2 direction)
    {
        rb.linearVelocity = direction;

        this.swordManager = swordManager;

        playerTransform = swordManager.transform.root;
        playerStats = swordManager.player.stats;
        damageScaleData = swordManager.damageScaleData;
        comebackTimer = 0f;
    }

    public virtual void GetSwordBackToPlayer()
    {
        shouldComeback = true;

        if (transform.parent != null)
            transform.parent = null;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }
    }

    protected void HandleComeback()
    {
        if (playerTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance > maxAllowedDistance && shouldComeback == false)
            GetSwordBackToPlayer();

        if (shouldComeback == false)
            return;

        comebackTimer += Time.deltaTime;

        Vector2 directionToPlayer = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        if (directionToPlayer.sqrMagnitude > 0.001f)
            transform.right = directionToPlayer;

        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, comebackSpeed * Time.deltaTime);

        if (distance < .5f || comebackTimer >= maxComebackDuration)
            Destroy(gameObject);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        StopSword(collision);
        DamageEnemiesInRadius(transform, 1);
    }

    protected void StopSword(Collider2D collision)
    {
        rb.simulated = false;
        transform.parent = collision.transform;
    }
}
