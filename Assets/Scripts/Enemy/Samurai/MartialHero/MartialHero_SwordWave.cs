using UnityEngine;

public class MartialHero_SwordWave : MonoBehaviour, ICounterable
{
    [Header("Movement & Lifetime")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 2.5f;
    [SerializeField] private Vector2 colliderSize = new Vector2(1.2f, 0.4f);
    [SerializeField] private Vector2 colliderOffset = new Vector2(0.2f, 0f);
    [SerializeField] private LayerMask whatIsObstacle;
    [SerializeField] private bool canBeCountered = true;

    [Header("Hit Effects")]
    [SerializeField] private GameObject hitVfxPrefab;

    public bool CanBeCountered => canBeCountered;

    private Enemy_MartialHero owner;
    private DamageScaleData damageScale;
    private LayerMask whatIsTarget;
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private int moveDir = 1;
    private bool hasHit;
    private bool isReflected;

    private void Update()
    {
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            transform.right = rb.linearVelocity;
        }
    }

    public void Setup(Enemy_MartialHero martialHero, int direction, DamageScaleData damageScale, LayerMask targetMask, LayerMask obstacleMask)
    {
        this.owner = martialHero;
        this.damageScale = damageScale;
        this.moveDir = direction >= 0 ? 1 : -1;
        this.whatIsTarget = targetMask.value != 0 ? targetMask : LayerMask.GetMask("Player");
        this.whatIsObstacle = obstacleMask.value != 0 ? obstacleMask : LayerMask.GetMask("Ground");

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = new Vector2(speed * moveDir, 0);

        col = GetComponent<BoxCollider2D>();
        if (col == null)
            col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = colliderSize;
        col.offset = colliderOffset;

        UpdateFacing();

        // Ensure 2D sorting
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            if (string.IsNullOrEmpty(r.sortingLayerName) || r.sortingLayerName == "Default")
            {
                r.sortingLayerName = "Default";
                r.sortingOrder = 30;
            }
        }

        Destroy(gameObject, lifetime);
    }

    private void UpdateFacing()
    {
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
            transform.right = rb.linearVelocity;
        else
            transform.right = new Vector2(moveDir, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit)
            return;

        // Check Target
        if (((1 << collision.gameObject.layer) & whatIsTarget) != 0)
        {
            hasHit = true;
            SpawnHitVfx(collision.transform.position);

            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                if (isReflected)
                {
                    // Player parried it back: deal damage to Boss
                    if (Player.instance != null && Player.instance.stats != null)
                    {
                        AttackData attackData = Player.instance.stats.GetAttackData(new DamageScaleData { phyiscal = 1.5f });
                        damageable.TakeDamage(attackData.phyiscalDamage, attackData.elementalDamage, attackData.element, Player.instance.transform);
                    }
                    else
                    {
                        damageable.TakeDamage(35f, 0, ElementType.None, transform);
                    }

                    if (owner != null && collision.transform == owner.transform)
                    {
                        owner.HandleCounter();
                    }
                }
                else
                {
                    // Boss hit Player
                    if (owner != null && owner.stats != null)
                    {
                        DamageScaleData scale = damageScale != null ? damageScale : new DamageScaleData { phyiscal = 1.2f };
                        AttackData attackData = owner.stats.GetAttackData(scale);
                        damageable.TakeDamage(attackData.phyiscalDamage, attackData.elementalDamage, attackData.element, owner.transform);
                    }
                }
            }

            Destroy(gameObject, 0.05f);
            return;
        }

        // Check Obstacle (Wall/Ground)
        if (((1 << collision.gameObject.layer) & whatIsObstacle) != 0)
        {
            hasHit = true;
            SpawnHitVfx(transform.position);
            Destroy(gameObject, 0.05f);
        }
    }

    private void SpawnHitVfx(Vector3 pos)
    {
        if (hitVfxPrefab != null)
        {
            GameObject vfx = Instantiate(hitVfxPrefab, pos, Quaternion.identity);
            Renderer[] renderers = vfx.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.sortingLayerName = "Default";
                r.sortingOrder = 35;
            }
            Destroy(vfx, 1.2f);
        }
    }

    public void HandleCounter()
    {
        if (!canBeCountered || isReflected)
            return;

        isReflected = true;
        moveDir *= -1;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(speed * moveDir * 1.35f, 0);
            transform.right = rb.linearVelocity;
        }
        else
        {
            UpdateFacing();
        }

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
            whatIsTarget = 1 << enemyLayer;
    }
}
