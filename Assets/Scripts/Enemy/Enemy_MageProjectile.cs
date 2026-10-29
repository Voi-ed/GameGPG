using UnityEngine;

public class Enemy_MageProjectile : MonoBehaviour
{
    private Entity_Combat combat;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator anim;

    [Header("Flight & Arc Settings")]
    [SerializeField] private float arcHeight = 2.5f;
    [SerializeField] private LayerMask whatCanCollideWith;

    [Header("Explosion Settings")]
    [Tooltip("Bán kính nổ sát thương khi quả cầu chạm đất hoặc trúng mục tiêu")]
    [SerializeField] private float explosionRadius = 1.3f;

    private bool hasHit;

    public void SetupProjectile(Transform target, Entity_Combat combat)
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponentInChildren<Animator>();

        if (anim != null)
            anim.enabled = false;

        this.combat = combat;
        hasHit = false;

        Vector2 targetPosition;
        if (target != null)
        {
            // Nhắm vào ngực/thân người chơi (thay vì chân sát mặt đất)
            targetPosition = (Vector2)target.position + Vector2.up * 0.8f;
        }
        else if (Player.instance != null)
        {
            targetPosition = (Vector2)Player.instance.transform.position + Vector2.up * 0.8f;
        }
        else
        {
            targetPosition = (Vector2)transform.position + Vector2.right * 4f;
        }

        Vector2 velocity = CalculateBallisticVelocity(transform.position, targetPosition);
        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }

        Destroy(gameObject, 6f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit)
            return;

        if (((1 << collision.gameObject.layer) & whatCanCollideWith) != 0)
        {
            hasHit = true;
            ApplyImpactDamage(collision);

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0;
            }

            if (anim != null)
                anim.enabled = true;

            if (col != null)
                col.enabled = false;

            Destroy(gameObject, 1.5f);
        }
    }

    private void ApplyImpactDamage(Collider2D directHit)
    {
        if (combat == null)
            return;

        bool damagedDirect = false;

        // 1. Gây sát thương trực tiếp lên đối tượng va chạm nếu có IDamageable
        if (directHit != null)
        {
            IDamageable directDamageable = directHit.GetComponentInParent<IDamageable>() ?? directHit.GetComponent<IDamageable>();
            if (directDamageable != null)
            {
                combat.PerformAttackOnTarget(directHit.transform);
                damagedDirect = true;
            }
        }

        // 2. Sát thương nổ lan (AOE) xung quanh điểm va chạm (đảm bảo kể cả khi quả cầu chạm sàn đất cạnh người chơi vẫn nổ trúng Player)
        LayerMask playerMask = LayerMask.GetMask("Player");
        Collider2D[] splashHits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, playerMask);
        foreach (var hit in splashHits)
        {
            if (hit == null) continue;

            // Nếu chưa gây sát thương lên mục tiêu này ở bước 1
            if (!damagedDirect || (hit.transform != directHit.transform && hit.transform.root != directHit.transform.root))
            {
                combat.PerformAttackOnTarget(hit.transform);
            }
        }
    }

    /// <summary>
    /// Tính toán vận tốc đường cong cầu vồng (ballistic arc) an toàn không bị lỗi chia cho 0 hay NaN
    /// </summary>
    private Vector2 CalculateBallisticVelocity(Vector2 start, Vector2 end)
    {
        float gravity = rb != null ? Mathf.Abs(Physics2D.gravity.y * rb.gravityScale) : 9.8f;
        if (gravity <= 0.01f) gravity = 9.8f;

        float displacementY = end.y - start.y;
        float displacementX = end.x - start.x;

        // Đỉnh vòm parabol luôn cao hơn cả điểm xuất phát lẫn điểm đích
        float peakHeight = Mathf.Max(arcHeight, displacementY + 0.5f);

        float timeToApex = Mathf.Sqrt(2f * peakHeight / gravity);
        float fallHeight = Mathf.Max(0.01f, peakHeight - displacementY);
        float timeFromApex = Mathf.Sqrt(2f * fallHeight / gravity);

        float totalTime = Mathf.Max(0.15f, timeToApex + timeFromApex);

        float velocityY = Mathf.Sqrt(2f * gravity * peakHeight);
        float velocityX = displacementX / totalTime;

        return new Vector2(velocityX, velocityY);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
