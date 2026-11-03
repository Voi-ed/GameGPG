using System.Collections;
using UnityEngine;

public class SkillObject_FireExplosion : SkillObject_Base
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 2.4f;
    [SerializeField] private float damageMultiplier = 1.8f;
    [SerializeField] private Vector2 knockbackPower = new Vector2(8f, 7.5f);
    [SerializeField] private float knockbackDuration = 0.35f;
    [SerializeField] private float lifetime = 1.0f;

    [Header("Stun Settings")]
    [SerializeField] private bool canStun = true;
    [SerializeField] private float stunDuration = 1.5f;

    [Header("Game Feel")]
    [SerializeField] private bool useHitStop = true;
    [SerializeField] private float hitStopDuration = 0.05f;

    [Header("Visual Settings")]
    [SerializeField] private GameObject fireExplosionVfxPrefab;
    [SerializeField] private Vector3 vfxOffset = new Vector3(0, 0.2f, 0);
    [SerializeField] private Vector3 vfxScale = new Vector3(1.35f, 1.35f, 1.35f);

    private GameObject spawnedVfx;
    private bool isMain = true;
    private bool isHitStopping = false;

    public void SetupFireExplosion(
        Entity_Stats stats,
        DamageScaleData scaleData,
        GameObject vfxPrefab = null,
        bool isMainExplosion = true,
        float overrideRadius = -1f,
        float overrideDamageMult = -1f,
        Vector3? overrideVfxScale = null)
    {
        playerStats = stats;
        damageScaleData = scaleData;
        isMain = isMainExplosion;

        if (overrideRadius > 0)
            explosionRadius = overrideRadius;

        if (overrideDamageMult > 0)
            damageMultiplier = overrideDamageMult;

        if (overrideVfxScale.HasValue)
            vfxScale = overrideVfxScale.Value;

        if (!isMainExplosion)
        {
            canStun = false;
            useHitStop = false;
            knockbackPower = new Vector2(2f, 5.5f);
        }

        checkRadius = explosionRadius;

        if (vfxPrefab != null)
            fireExplosionVfxPrefab = vfxPrefab;

#if UNITY_EDITOR
        if (fireExplosionVfxPrefab == null)
        {
            fireExplosionVfxPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Vfx_UseItem/Vfx_Dragon_FireExplosion.prefab");
        }
#endif

        if (fireExplosionVfxPrefab != null && spawnedVfx == null)
        {
            Quaternion rot = fireExplosionVfxPrefab.transform.rotation;
            spawnedVfx = Instantiate(fireExplosionVfxPrefab, transform.position + vfxOffset, rot, transform);
            spawnedVfx.transform.localScale = vfxScale;

            foreach (var r in spawnedVfx.GetComponentsInChildren<Renderer>(true))
            {
                r.sortingLayerName = "Vfx_Front";
                if (r.sortingOrder < 15)
                    r.sortingOrder = 15;
            }
        }

        Explode();
        Destroy(gameObject, lifetime);
    }

    private void Explode()
    {
        if (playerStats == null)
            return;

        // Âm thanh bộc phá uy lực cho Hỏa Long Xung Kích
        if (AudioManager.instance != null)
        {
            if (isMain)
                AudioManager.instance.PlayGlobalSFX("dragon_attack");
            else
                AudioManager.instance.PlayGlobalSFX("player_attackHit");
        }

        AttackData attackData = playerStats.GetAttackData(damageScaleData);
        float baseDamage = playerStats.GetBaseDamage();
        float physDamage = baseDamage * 0.4f * (damageScaleData != null ? damageScaleData.phyiscal : 1f);
        float fireDamage = Mathf.Max(baseDamage * damageMultiplier * (damageScaleData != null ? damageScaleData.elemental : 1f), isMain ? 20f : 10f);

        Collider2D[] targets = GetEnemiesAround(transform, explosionRadius);
        bool hitAnyEnemy = false;

        foreach (var target in targets)
        {
            IDamageable damageable = target.GetComponentInParent<IDamageable>() ?? target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(physDamage, fireDamage, ElementType.Fire, transform);
                hitAnyEnemy = true;
            }

            Entity_StatusHandler statusHandler = target.GetComponentInParent<Entity_StatusHandler>() ?? target.GetComponent<Entity_StatusHandler>();
            if (statusHandler != null && attackData != null)
            {
                statusHandler.ApplyStatusEffect(ElementType.Fire, attackData.effectData);
            }

            // Stun enemy (only on main explosion)
            if (canStun)
            {
                Enemy enemy = target.GetComponentInParent<Enemy>() ?? target.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.ApplyStun(stunDuration);
                }
            }

            // Knockback
            Entity enemyEntity = target.GetComponentInParent<Entity>() ?? target.GetComponent<Entity>();
            if (enemyEntity != null)
            {
                float dirX = Mathf.Sign(enemyEntity.transform.position.x - transform.position.x);
                if (Mathf.Abs(enemyEntity.transform.position.x - transform.position.x) < 0.1f)
                    dirX = 1f;

                Vector2 knockForce = new Vector2(dirX * knockbackPower.x, knockbackPower.y);
                enemyEntity.ReciveKnockback(knockForce, knockbackDuration);
            }
        }

        // Micro hit-stop if hit any enemy on main explosion
        if (isMain && useHitStop && hitAnyEnemy)
        {
            StartCoroutine(HitStopCo(hitStopDuration));
        }
    }

    private IEnumerator HitStopCo(float duration)
    {
        if (Time.timeScale > 0.1f)
        {
            isHitStopping = true;
            float prevScale = Time.timeScale;
            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = prevScale;
            isHitStopping = false;
        }
    }

    private void OnDestroy()
    {
        if (isHitStopping)
        {
            Time.timeScale = 1f;
        }

        if (spawnedVfx != null)
            Destroy(spawnedVfx);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = isMain ? Color.red : new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
