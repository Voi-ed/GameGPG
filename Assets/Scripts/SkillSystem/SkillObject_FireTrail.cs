using UnityEngine;

public class SkillObject_FireTrail : SkillObject_Base
{
    [SerializeField] private float lifetime = 2.5f;
    [SerializeField] private float damageInterval = 0.35f;
    [SerializeField] private GameObject fireVfxPrefab;

    [Header("Visual Adjustments")]
    [Tooltip("Dịch chuyển vị trí ngọn lửa (giảm Y để hạ ngọn lửa sát đất)")]
    [SerializeField] private Vector3 vfxOffset = new Vector3(0, -0.75f, 0);
    [Tooltip("Kích thước ngọn lửa (giảm Y để ngọn lửa không bốc quá cao)")]
    [SerializeField] private Vector3 vfxScale = new Vector3(0.7f, 0.45f, 0.7f);

    private float expireTime;
    private float lastDamageTime;
    private GameObject spawnedVfx;

    public void SetupFireTrail(Entity_Stats stats, DamageScaleData scaleData, GameObject vfxPrefab = null, float duration = 2.5f, float radius = 1.0f)
    {
        playerStats = stats;
        damageScaleData = scaleData;
        lifetime = duration;
        checkRadius = radius;
        expireTime = Time.time + lifetime;
        lastDamageTime = -damageInterval; // Immediate first tick

        if (vfxPrefab != null)
            fireVfxPrefab = vfxPrefab;

        if (fireVfxPrefab != null && spawnedVfx == null)
        {
            spawnedVfx = Instantiate(fireVfxPrefab, transform.position + vfxOffset, Quaternion.identity, transform);
            spawnedVfx.transform.localScale = vfxScale;
        }
    }

    private void Update()
    {
        if (Time.time >= expireTime)
        {
            Destroy(gameObject);
            return;
        }

        if (Time.time >= lastDamageTime + damageInterval)
        {
            lastDamageTime = Time.time;
            DamageEnemiesInTrail();
        }
    }

    private void DamageEnemiesInTrail()
    {
        if (playerStats == null)
            return;

        foreach (var target in GetEnemiesAround(transform, checkRadius))
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable == null)
                continue;

            AttackData attackData = playerStats.GetAttackData(damageScaleData);
            Entity_StatusHandler statusHandler = target.GetComponent<Entity_StatusHandler>();

            float baseDamage = playerStats.GetBaseDamage();
            float physDamage = baseDamage * 0.2f * (damageScaleData != null ? damageScaleData.phyiscal : 1f);
            float fireDamage = Mathf.Max(baseDamage * 0.35f * (damageScaleData != null ? damageScaleData.elemental : 1f), 8f);

            damageable.TakeDamage(physDamage, fireDamage, ElementType.Fire, transform);

            if (statusHandler != null && attackData != null)
                statusHandler.ApplyStatusEffect(ElementType.Fire, attackData.effectData);
        }
    }

    public void Detonate(Entity_Stats stats, DamageScaleData scaleData, GameObject explosionPrefab)
    {
        if (explosionPrefab != null)
        {
            Vector3 pos = transform.position;
            GameObject expObj = Instantiate(explosionPrefab, pos, Quaternion.identity);
            SkillObject_FireExplosion explosion = expObj.GetComponent<SkillObject_FireExplosion>();
            if (explosion != null)
            {
                explosion.SetupFireExplosion(
                    stats: stats,
                    scaleData: scaleData,
                    isMainExplosion: false,
                    overrideRadius: 1.5f,
                    overrideDamageMult: 0.85f,
                    overrideVfxScale: new Vector3(0.75f, 0.75f, 0.75f)
                );
            }
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (spawnedVfx != null)
            Destroy(spawnedVfx);
    }
}
