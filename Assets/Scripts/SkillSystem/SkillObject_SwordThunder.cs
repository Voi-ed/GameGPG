using System.Collections.Generic;
using UnityEngine;

public class SkillObject_SwordThunder : SkillObject_Sword
{
    [Header("Lightning Strike Settings (On Impact)")]
    [SerializeField] private GameObject lightningStrikeVfxPrefab;
    [SerializeField] private float impactRadius = 2.2f;
    [SerializeField] private float impactDamageMultiplier = 1.3f;

    [Header("Lightning Rod Settings (While Stuck)")]
    [SerializeField] private GameObject zapVfxPrefab;
    [SerializeField] private float maxStuckDuration = 4.5f;
    [SerializeField] private float zapRadius = 3.0f;
    [SerializeField] private float zapInterval = 0.85f;
    [SerializeField] private float zapDamageMultiplier = 0.35f;
    [SerializeField] private int maxZapTargets = 3;

    [Header("Recall Thunder Settings")]
    [SerializeField] private float recallDamageMultiplier = 0.75f;
    [SerializeField] private float recallCheckRadius = 1.2f;

    private bool isStuck;
    private float zapTimer;
    private float stuckTimer;
    private readonly HashSet<IDamageable> enemiesHitOnReturn = new HashSet<IDamageable>();

    public override void SetupSword(Skill_SwordThrow swordManager, Vector2 direction)
    {
        base.SetupSword(swordManager, direction);
        isStuck = false;
        stuckTimer = 0f;
        zapTimer = zapInterval;
        enemiesHitOnReturn.Clear();
    }

    protected override void Update()
    {
        if (shouldComeback)
        {
            HandleRecallMovementAndDamage();
            return;
        }

        if (!isStuck)
        {
            transform.right = rb.linearVelocity;
            HandleDistanceCheck();
        }
        else
        {
            HandleLightningRod();
        }
    }

    private void HandleDistanceCheck()
    {
        if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) > maxAllowedDistance)
            GetSwordBackToPlayer();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (isStuck || shouldComeback)
            return;

        StopSword(collision);
        isStuck = true;
        stuckTimer = 0f;

        TriggerImpactLightningStrike();
    }

    private void TriggerImpactLightningStrike()
    {
        if (lightningStrikeVfxPrefab != null)
        {
            Instantiate(lightningStrikeVfxPrefab, transform.position, Quaternion.identity);
        }

        if (playerStats == null)
            return;

        AttackData attackData = playerStats.GetAttackData(damageScaleData);
        float baseDamage = playerStats.GetBaseDamage();
        float physDmg = baseDamage * 0.25f * (damageScaleData != null ? damageScaleData.phyiscal : 1f);
        float lightningDmg = Mathf.Max(baseDamage * impactDamageMultiplier * (damageScaleData != null ? damageScaleData.elemental : 1f), 10f);

        foreach (var target in GetEnemiesAround(transform, impactRadius))
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(physDmg, lightningDmg, ElementType.Lightning, transform);
            }

            Entity_StatusHandler statusHandler = target.GetComponent<Entity_StatusHandler>();
            if (statusHandler != null && attackData != null)
            {
                statusHandler.ApplyStatusEffect(ElementType.Lightning, attackData.effectData);
            }

            Entity enemyEntity = target.GetComponent<Entity>();
            if (enemyEntity != null)
            {
                Vector2 blowDir = (target.transform.position - transform.position).normalized;
                enemyEntity.ReciveKnockback(blowDir * 3f, 0.2f);
            }
        }
    }

    private void HandleLightningRod()
    {
        stuckTimer += Time.deltaTime;
        if (stuckTimer >= maxStuckDuration)
        {
            GetSwordBackToPlayer();
            return;
        }

        zapTimer -= Time.deltaTime;
        if (zapTimer <= 0f)
        {
            zapTimer = zapInterval;
            ZapNearbyEnemies();
        }
    }

    private void ZapNearbyEnemies()
    {
        if (playerStats == null)
            return;

        Collider2D[] potentialTargets = GetEnemiesAround(transform, zapRadius);
        if (potentialTargets == null || potentialTargets.Length == 0)
            return;

        AttackData attackData = playerStats.GetAttackData(damageScaleData);
        float baseDamage = playerStats.GetBaseDamage();
        float zapDmg = Mathf.Max(baseDamage * zapDamageMultiplier * (damageScaleData != null ? damageScaleData.elemental : 1f), 3f);

        int count = 0;
        foreach (var col in potentialTargets)
        {
            if (count >= maxZapTargets)
                break;

            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(0, zapDmg, ElementType.Lightning, transform);

                Entity_StatusHandler statusHandler = col.GetComponent<Entity_StatusHandler>();
                if (statusHandler != null && attackData != null)
                {
                    statusHandler.ApplyStatusEffect(ElementType.Lightning, attackData.effectData);
                }

                if (zapVfxPrefab != null)
                {
                    Instantiate(zapVfxPrefab, col.transform.position, Quaternion.identity);
                }

                // Không knockback/flinch liên tục để tránh stunlock quái vô hạn
                count++;
            }
        }
    }

    private void HandleRecallMovementAndDamage()
    {
        if (transform.parent != null)
            transform.parent = null;

        if (playerTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, comebackSpeed * Time.deltaTime);

        Vector2 toPlayer = (playerTransform.position - transform.position).normalized;
        if (toPlayer.sqrMagnitude > 0.001f)
            transform.right = toPlayer;

        DamageEnemiesOnReturnPath();

        if (distance < 0.6f)
            Destroy(gameObject);
    }

    private void DamageEnemiesOnReturnPath()
    {
        if (playerStats == null)
            return;

        Collider2D[] inPath = GetEnemiesAround(transform, recallCheckRadius);
        if (inPath == null)
            return;

        AttackData attackData = playerStats.GetAttackData(damageScaleData);
        float baseDamage = playerStats.GetBaseDamage();
        float returnDmg = Mathf.Max(baseDamage * recallDamageMultiplier * (damageScaleData != null ? damageScaleData.elemental : 1f), 6f);

        foreach (var col in inPath)
        {
            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable != null && !enemiesHitOnReturn.Contains(damageable))
            {
                enemiesHitOnReturn.Add(damageable);

                damageable.TakeDamage(baseDamage * 0.15f, returnDmg, ElementType.Lightning, transform);

                Entity_StatusHandler statusHandler = col.GetComponent<Entity_StatusHandler>();
                if (statusHandler != null && attackData != null)
                    statusHandler.ApplyStatusEffect(ElementType.Lightning, attackData.effectData);

                if (zapVfxPrefab != null)
                    Instantiate(zapVfxPrefab, col.transform.position, Quaternion.identity);

                Entity enemyEntity = col.GetComponent<Entity>();
                if (enemyEntity != null)
                {
                    enemyEntity.ReciveKnockback(new Vector2(Mathf.Sign(enemyEntity.transform.position.x - transform.position.x) * 4f, 3f), 0.2f);
                }
            }
        }
    }

    public override void GetSwordBackToPlayer()
    {
        base.GetSwordBackToPlayer();
        isStuck = false;
        if (rb != null)
            rb.simulated = false;
        if (transform.parent != null)
            transform.parent = null;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, isStuck ? zapRadius : impactRadius);
    }
}
