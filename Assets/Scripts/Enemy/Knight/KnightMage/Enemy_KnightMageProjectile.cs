using UnityEngine;

public class Enemy_KnightMageProjectile : MonoBehaviour, ICounterable
{
    private Entity_Combat combat;
    private DamageScaleData damageScale;
    private Rigidbody2D rb;
    private Collider2D col;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;

    [Header("Visual & Impact Settings")]
    [SerializeField] private GameObject visualPrefab;
    [SerializeField] private float visualScale = 0.45f;
    [SerializeField] private GameObject impactVfxPrefab;
    [SerializeField] private LayerMask whatCanCollideWith;
    [SerializeField] private bool canBeCountered = true;

    public bool CanBeCountered => canBeCountered && !hasHit;

    private ParticleSystem[] particleSystems;
    private GameObject visualInstance;
    private bool hasHit;

    public void SetupProjectile(Transform target, Entity_Combat combat, DamageScaleData damageScale)
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        this.combat = combat;
        this.damageScale = damageScale;

        Vector2 direction = Vector2.right;
        if (target != null)
        {
            Vector2 targetPos = (Vector2)target.position + Vector2.up * 0.6f;
            direction = (targetPos - (Vector2)transform.position).normalized;
        }

        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        SetupVisual();

        Destroy(gameObject, lifetime);
    }

    private void SetupVisual()
    {
        if (visualPrefab != null && visualInstance == null)
        {
            visualInstance = Instantiate(visualPrefab, transform);
            visualInstance.transform.localPosition = Vector3.zero;
            visualInstance.transform.localRotation = Quaternion.identity;
            visualInstance.transform.localScale = Vector3.one * visualScale;
        }

        particleSystems = GetComponentsInChildren<ParticleSystem>();

        // Thiết lập sorting layer đảm bảo particle luôn nổi trên 2D sprite
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            r.sortingLayerName = "Default";
            r.sortingOrder = 25;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit)
            return;

        if (((1 << collision.gameObject.layer) & whatCanCollideWith) != 0)
        {
            hasHit = true;

            combat?.PerformAttackOnTarget(collision.transform, damageScale);

            if (col != null)
                col.enabled = false;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            // Dừng phát hạt mới từ quả cầu
            if (particleSystems != null)
            {
                foreach (var ps in particleSystems)
                {
                    if (ps != null)
                    {
                        var emission = ps.emission;
                        emission.enabled = false;
                    }
                }
            }

            // Kích hoạt vụ nổ lửa va chạm
            if (impactVfxPrefab != null)
            {
                Instantiate(impactVfxPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject, 0.4f);
        }
    }

    public void HandleCounter()
    {
        if (!CanBeCountered)
            return;

        // Khi người chơi Parry/Counter: phản xạ đạn bay thẳng ngược về phía Knight Mage
        if (combat != null && rb != null)
        {
            Vector2 dirToCaster = ((Vector2)combat.transform.position + Vector2.up * 0.8f - (Vector2)transform.position).normalized;
            rb.linearVelocity = dirToCaster * speed * 1.25f;

            float angle = Mathf.Atan2(dirToCaster.y, dirToCaster.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Đổi layer có thể va chạm sang Enemy để đạn đánh trúng Boss
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer != -1)
            {
                whatCanCollideWith = (1 << enemyLayer) | LayerMask.GetMask("Ground");
            }
        }
    }
}
