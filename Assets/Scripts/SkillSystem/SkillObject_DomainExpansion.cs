using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillObject_DomainExpansion : SkillObject_Base 
{
    private Skill_DomainExpansion domainManager;

    private float expandSpeed = 2;
    private float duration;

    private float slowDownPercent = .9f;

    private Vector3 targetScale;
    private bool isShrinking;

    private bool isChronoStasis;
    private bool isSlowingDownDomain;
    private readonly List<Enemy_ChronoStasisReceiver> stasisReceivers = new List<Enemy_ChronoStasisReceiver>();
    private readonly List<Enemy_MageProjectile> frozenProjectiles = new List<Enemy_MageProjectile>();
    private readonly Dictionary<Enemy_MageProjectile, (Vector2 vel, float grav, bool col)> projectileStates = new Dictionary<Enemy_MageProjectile, (Vector2, float, bool)>();

    [Header("Domain Acceleration Buffs")]
    [SerializeField] private float stasisMoveSpeedMultiplier = 1.6f;
    [SerializeField] private float stasisAttackSpeedBonus = 0.85f;
    [SerializeField] private float slowDomainMoveSpeedMultiplier = 1.25f;
    [SerializeField] private float slowDomainAttackSpeedBonus = 0.35f;
    [SerializeField] private float ghostTrailInterval = 0.065f;
    [SerializeField] private Color chronoGhostColor = new Color(0.6f, 0.35f, 1f, 0.75f);
    [SerializeField] private Color slowGhostColor = new Color(0.35f, 0.85f, 1f, 0.6f);

    [Header("Chrono Stasis VFX Prefabs")]
    [SerializeField] private GameObject chronoSlashVfxPrefab;
    [SerializeField] private GameObject chronoGlowVfxPrefab;
    [SerializeField] private GameObject chronoExplosionVfxPrefab;

    private bool isPlayerBuffed;
    private float originalPlayerMoveSpeed;
    private Coroutine ghostTrailCo;

    public void SetupDomain(Skill_DomainExpansion domainManager)
    {
        this.domainManager = domainManager;

        duration = domainManager.GetDomainDuration();
        slowDownPercent = domainManager.GetSlowPercentage();
        expandSpeed = domainManager.expandSpeed;
        float maxSize = domainManager.maxDomainSize;

        SkillUpgradeType currentUpgrade = domainManager.GetUpgrade();
        isChronoStasis = currentUpgrade == SkillUpgradeType.Domain_ChronoStasis;
        isSlowingDownDomain = currentUpgrade == SkillUpgradeType.Domain_SlowingDown;

        if (isChronoStasis)
        {
            SetupChronoStasisVisuals();
        }

        if (CanBuffPlayer())
        {
            ApplyPlayerBuff();
        }

        targetScale = Vector3.one * maxSize;
        Invoke(nameof(ShrinkDomain), duration);
    }

    private void SetupChronoStasisVisuals()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(0.12f, 0.05f, 0.25f, 0.85f);
        }
    }

    private void Update()
    {
        HandleScaling();
        HandlePlayerBuffZone();
    }

    private void HandleScaling()
    {
        float sizeDiffrence = Mathf.Abs(transform.localScale.x - targetScale.x);
        bool shouldChangeScale = sizeDiffrence > .1f;

        if (shouldChangeScale)
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, expandSpeed * Time.deltaTime);

        if (isShrinking && sizeDiffrence < .1f)
            TerminateDomain();
    }

    private bool CanBuffPlayer() => isChronoStasis || isSlowingDownDomain;

    private void HandlePlayerBuffZone()
    {
        if (!CanBuffPlayer() || domainManager == null || domainManager.player == null)
            return;

        if (isShrinking)
        {
            RemovePlayerBuff();
            return;
        }

        float radius = transform.localScale.x * 0.5f;
        float distToPlayer = Vector2.Distance(transform.position, domainManager.player.transform.position);
        bool isInside = distToPlayer <= (radius + 0.5f);

        if (isInside && !isPlayerBuffed)
        {
            ApplyPlayerBuff();
        }
        else if (!isInside && isPlayerBuffed)
        {
            RemovePlayerBuff();
        }
    }

    private void ApplyPlayerBuff()
    {
        if (isPlayerBuffed || domainManager == null || domainManager.player == null)
            return;

        Player player = domainManager.player;
        originalPlayerMoveSpeed = player.moveSpeed;

        float moveMult = isChronoStasis ? stasisMoveSpeedMultiplier : slowDomainMoveSpeedMultiplier;
        float atkBonus = isChronoStasis ? stasisAttackSpeedBonus : slowDomainAttackSpeedBonus;

        player.moveSpeed = originalPlayerMoveSpeed * moveMult;
        player.stats.offense.attackSpeed.AddModifier(atkBonus, "DomainStasisBuff");

        if (player.anim != null)
            player.anim.SetFloat("attackSpeedMultiplier", player.stats.offense.attackSpeed.GetValue());

        isPlayerBuffed = true;

        if (ghostTrailCo != null)
            StopCoroutine(ghostTrailCo);
        ghostTrailCo = StartCoroutine(GhostTrailRoutine());
    }

    private void RemovePlayerBuff()
    {
        if (!isPlayerBuffed)
            return;

        isPlayerBuffed = false;

        if (ghostTrailCo != null)
        {
            StopCoroutine(ghostTrailCo);
            ghostTrailCo = null;
        }

        if (domainManager != null && domainManager.player != null)
        {
            Player player = domainManager.player;
            player.moveSpeed = originalPlayerMoveSpeed;
            player.stats.offense.attackSpeed.RemoveModifier("DomainStasisBuff");

            if (player.anim != null)
                player.anim.SetFloat("attackSpeedMultiplier", player.stats.offense.attackSpeed.GetValue());
        }
    }

    private IEnumerator GhostTrailRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(ghostTrailInterval);
        Color trailColor = isChronoStasis ? chronoGhostColor : slowGhostColor;

        while (isPlayerBuffed && domainManager != null && domainManager.player != null)
        {
            Player player = domainManager.player;
            bool isMoving = player.rb != null && (player.rb.linearVelocity.sqrMagnitude > 0.08f || player.moveInput.sqrMagnitude > 0.01f);
            bool isAttacking = player.stateMachine != null && (player.stateMachine.currentState == player.basicAttackState || player.stateMachine.currentState == player.jumpAttackState);

            if (isMoving || isAttacking)
            {
                if (player.vfx != null)
                {
                    player.vfx.CreateImageEcho(trailColor);
                }
            }

            yield return wait;
        }
    }

    private void TerminateDomain()
    {
        RemovePlayerBuff();
        domainManager.ClearTargets();
        Destroy(gameObject);
    }

    private void ShrinkDomain()
    {
        RemovePlayerBuff();

        if (isChronoStasis)
        {
            // Detonate and release all trapped enemies simultaneously
            float burstMult = domainManager.GetStasisBurstMultiplier();
            foreach (var receiver in stasisReceivers)
            {
                if (receiver != null)
                    receiver.DetonateAndRelease(burstMult);
            }
            stasisReceivers.Clear();

            ReleaseProjectiles();
        }

        targetScale = Vector3.zero;
        isShrinking = true;
    }

    private void OnDisable()
    {
        RemovePlayerBuff();
    }

    private void OnDestroy()
    {
        RemovePlayerBuff();
    }

    private void FreezeProjectile(Enemy_MageProjectile projectile)
    {
        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        Collider2D projCol = projectile.GetComponent<Collider2D>();

        if (projRb != null)
        {
            projectileStates[projectile] = (projRb.linearVelocity, projRb.gravityScale, projCol != null && projCol.enabled);
            projRb.linearVelocity = Vector2.zero;
            projRb.gravityScale = 0;
            if (projCol != null)
                projCol.enabled = false;

            frozenProjectiles.Add(projectile);
        }
    }

    private void ReleaseProjectiles()
    {
        foreach (var proj in frozenProjectiles)
        {
            if (proj != null && projectileStates.TryGetValue(proj, out var state))
            {
                Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
                Collider2D projCol = proj.GetComponent<Collider2D>();

                if (projRb != null)
                {
                    projRb.linearVelocity = state.vel;
                    projRb.gravityScale = state.grav;
                }
                if (projCol != null)
                    projCol.enabled = state.col;
            }
        }
        frozenProjectiles.Clear();
        projectileStates.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();

        if (enemy != null)
        {
            domainManager.AddTarget(enemy);

            if (isChronoStasis)
            {
                Enemy_ChronoStasisReceiver receiver = enemy.GetComponent<Enemy_ChronoStasisReceiver>();
                if (receiver == null)
                    receiver = enemy.gameObject.AddComponent<Enemy_ChronoStasisReceiver>();

                receiver.SetupPrefabs(chronoSlashVfxPrefab, chronoGlowVfxPrefab, chronoExplosionVfxPrefab);
                receiver.EnterStasis();
                if (!stasisReceivers.Contains(receiver))
                    stasisReceivers.Add(receiver);
            }
            else
            {
                enemy.SlowDownEntity(duration, slowDownPercent, true);
            }
            return;
        }

        if (isChronoStasis)
        {
            Enemy_MageProjectile projectile = collision.GetComponent<Enemy_MageProjectile>();
            if (projectile != null && !projectileStates.ContainsKey(projectile))
            {
                FreezeProjectile(projectile);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();

        if (enemy == null)
            return;

        if (isChronoStasis)
        {
            Enemy_ChronoStasisReceiver receiver = enemy.GetComponent<Enemy_ChronoStasisReceiver>();
            if (receiver != null)
            {
                receiver.DetonateAndRelease(domainManager.GetStasisBurstMultiplier());
                stasisReceivers.Remove(receiver);
            }
        }
        else
        {
            enemy.StopSlowDown();
        }
    }
}
