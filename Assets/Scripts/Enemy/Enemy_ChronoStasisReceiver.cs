using UnityEngine;

public class Enemy_ChronoStasisReceiver : MonoBehaviour
{
    private Enemy enemy;
    private Enemy_Health enemyHealth;

    [Header("Accumulated Data")]
    public float accumulatedPhysicalDamage { get; private set; }
    public float accumulatedElementalDamage { get; private set; }
    public int accumulatedHits { get; private set; }
    private ElementType lastElement = ElementType.None;
    private Transform lastAttacker;

    [Header("VFX Prefabs")]
    [SerializeField] private GameObject hitSlashPrefab;
    [SerializeField] private GameObject energyGlowPrefab;
    [SerializeField] private GameObject burstExplosionPrefab;

    private GameObject activeGlowInstance;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        enemyHealth = GetComponent<Enemy_Health>();
    }

    public void SetupPrefabs(GameObject slash, GameObject glow, GameObject explosion)
    {
        if (slash != null) hitSlashPrefab = slash;
        if (glow != null) energyGlowPrefab = glow;
        if (explosion != null) burstExplosionPrefab = explosion;
    }

    public void EnterStasis()
    {
        if (enemy != null)
            enemy.FreezeInTime(true);

        if (energyGlowPrefab != null && activeGlowInstance == null)
        {
            activeGlowInstance = Instantiate(energyGlowPrefab, transform.position, Quaternion.identity, transform);
            activeGlowInstance.transform.localScale = Vector3.one * 0.4f;
        }
    }

    public void RecordDamage(float physDamage, float elemDamage, ElementType element, Transform attacker)
    {
        accumulatedPhysicalDamage += physDamage;
        accumulatedElementalDamage += elemDamage;
        accumulatedHits++;
        lastElement = element;
        lastAttacker = attacker;

        // Visual 1: Spawn slash effect on enemy
        if (hitSlashPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.2f, 0.4f), 0);
            Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            Instantiate(hitSlashPrefab, spawnPos, rotation);
        }

        // Visual 2: Scale up energy glow size based on hits
        if (activeGlowInstance != null)
        {
            float targetScale = Mathf.Clamp(0.4f + (accumulatedHits * 0.15f), 0.4f, 2.0f);
            activeGlowInstance.transform.localScale = Vector3.one * targetScale;
        }
    }

    public void DetonateAndRelease(float burstMultiplier = 1.25f)
    {
        if (activeGlowInstance != null)
        {
            Destroy(activeGlowInstance);
            activeGlowInstance = null;
        }

        if (enemy != null)
            enemy.FreezeInTime(false);

        if (accumulatedHits > 0 && enemyHealth != null && !enemyHealth.isDead)
        {
            // Spawn Finisher Explosion VFX
            if (burstExplosionPrefab != null)
            {
                Instantiate(burstExplosionPrefab, transform.position, Quaternion.identity);
            }

            // Spawn an enlarged final cross slash
            if (hitSlashPrefab != null)
            {
                GameObject finalSlash = Instantiate(hitSlashPrefab, transform.position, Quaternion.identity);
                finalSlash.transform.localScale = Vector3.one * 2f;
            }

            float totalDamage = (accumulatedPhysicalDamage + accumulatedElementalDamage) * burstMultiplier;

            // Apply direct damage to health
            enemyHealth.ReduceHealth(totalDamage);

            if (!enemyHealth.isDead && lastAttacker != null)
            {
                // Apply knockback if survived
                Vector2 knockbackDir = (transform.position.x > lastAttacker.position.x) ? Vector2.right : Vector2.left;
                enemy.ReciveKnockback(new Vector2(knockbackDir.x * 6f, 5f), 0.4f);
            }
        }

        Destroy(this);
    }
}
