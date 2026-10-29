using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Điều khiển Quả Cầu Hắc Hỏa (FX_Fire_03_2) của Rồng khi thi triển kỹ năng Hắc Hỏa Cầu (Left Shift).
/// - Di chuyển liên tục bằng CircleCast để chống xuyên tường / xuyên quái (anti-tunneling).
/// - 3 lớp bảo hiểm tự tắt (Triple Fail-Safe): Hết thời gian bay, bay quá khoảng cách, hoặc coroutine bảo hiểm độc lập.
/// - Cơ chế tắt 2 giai đoạn (Graceful Despawn): Ngắt sinh hạt (StopEmitting), tạo vụ nổ, để khói/tàn lửa tan tự nhiên rồi mới Destroy.
/// </summary>
public class Dragon_FireballProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float maxLifetime = 2.0f;
    [SerializeField] private float maxDistance = 24f;
    [SerializeField] private float hitRadius = 0.75f;
    [SerializeField] private float explosionRadius = 2.4f;
    [SerializeField] private Vector2 knockbackForce = new Vector2(11f, 5.5f);

    [Header("VFX References (Direct Serialization)")]
    [SerializeField] private GameObject explosionPrefab;

    public GameObject ExplosionPrefab
    {
        get => explosionPrefab;
        set => explosionPrefab = value;
    }

    private float damage = 220f;
    private int facingDir = 1;
    private Transform damageDealer;
    private LayerMask whatIsEnemy;
    private LayerMask whatIsGround;

    private Vector3 startPosition;
    private Vector3 previousPosition;
    private float spawnTime;
    private float distanceTraveled = 0f;
    private bool hasExploded = false;
    private bool initialized = false;
    private ParticleSystem[] childParticleSystems;

    private const float SPAWN_GRACE_DISTANCE = 0.4f; // Không kích nổ sàn trong 0.4m đầu tiên khi vừa rời miệng

    public void Setup(float damage, float speed, int facingDir, Transform damageDealer, LayerMask enemyMask, LayerMask groundMask, GameObject customExplosionPrefab = null)
    {
        this.damage = damage;
        if (speed > 0f)
            this.speed = speed;
        this.facingDir = facingDir >= 0 ? 1 : -1;
        this.damageDealer = damageDealer;
        this.whatIsEnemy = enemyMask.value != 0 ? enemyMask : LayerMask.GetMask("Enemy", "InteractiveObjects");
        this.whatIsGround = groundMask.value != 0 ? groundMask : LayerMask.GetMask("Ground", "Default");

        if (customExplosionPrefab != null)
            this.explosionPrefab = customExplosionPrefab;

        spawnTime = Time.time;
        startPosition = transform.position;
        previousPosition = transform.position;
        distanceTraveled = 0f;

        // Lật hướng hạt theo hướng bay
        Vector3 localScale = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(localScale.x) * this.facingDir, localScale.y, localScale.z);

        // Đảm bảo sorting layer hiển thị nổi bật phía trước
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            r.sortingLayerName = "Default";
            r.sortingOrder = 25;
        }

        childParticleSystems = GetComponentsInChildren<ParticleSystem>(true);

        initialized = true;

        // Lớp bảo hiểm số 3: Coroutine độc lập cam kết tự dọn dẹp sau (maxLifetime + 1.2s)
        StartCoroutine(FailSafeDestroyRoutine(maxLifetime + 1.2f));
    }

    private void Start()
    {
        if (!initialized)
        {
            Setup(damage, speed, 1, transform, whatIsEnemy, whatIsGround);
        }
    }

    private void Update()
    {
        if (hasExploded)
            return;

        // Tính toán khoảng cách di chuyển frame này
        float moveDistance = speed * Time.deltaTime;
        Vector3 moveDelta = Vector3.right * (facingDir * moveDistance);
        Vector3 targetPosition = transform.position + moveDelta;

        distanceTraveled += moveDistance;

        // 1. Quét va chạm liên tục (Continuous Raycast/CircleCast Sweep) chống tunneling
        RaycastHit2D hitEnemy = Physics2D.CircleCast(previousPosition, hitRadius, moveDelta.normalized, moveDistance, whatIsEnemy);
        if (hitEnemy.collider != null)
        {
            if (damageDealer == null || hitEnemy.collider.transform.root != damageDealer.root)
            {
                transform.position = hitEnemy.point;
                Explode();
                return;
            }
        }

        // Kiểm tra va chạm với tường / mặt đất (chỉ sau khi đã bay khỏi vùng an toàn quanh miệng)
        if (distanceTraveled > SPAWN_GRACE_DISTANCE)
        {
            RaycastHit2D hitGround = Physics2D.CircleCast(previousPosition, hitRadius * 0.5f, moveDelta.normalized, moveDistance, whatIsGround);
            if (hitGround.collider != null && (damageDealer == null || hitGround.collider.transform.root != damageDealer.root))
            {
                transform.position = hitGround.point;
                Explode();
                return;
            }
        }

        // Cập nhật vị trí
        previousPosition = transform.position;
        transform.position = targetPosition;

        // 2. Lớp bảo hiểm số 1 & 2: Hết thời gian bay hoặc vượt quá khoảng cách tối đa
        if (Time.time - spawnTime >= maxLifetime || distanceTraveled >= maxDistance)
        {
            Explode();
        }
    }

    /// <summary>
    /// Kích nổ và thực hiện quy trình ngắt hạt mượt mà (Graceful Despawn).
    /// </summary>
    private void Explode()
    {
        if (hasExploded)
            return;
        hasExploded = true;

        // 1. Sinh vụ nổ Hắc Hỏa (FX_Fire_05_2) tại vị trí va chạm
        if (explosionPrefab != null)
        {
            GameObject expGO = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Renderer[] renderers = expGO.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.sortingLayerName = "Default";
                r.sortingOrder = 26;
            }
            Destroy(expGO, 1.6f);
        }

        // 2. Gây sát thương AoE diện rộng
        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, whatIsEnemy);

        bool hitAny = false;
        foreach (var col in hits)
        {
            if (damageDealer != null && col.transform.root == damageDealer.root)
                continue;

            IDamageable target = col.GetComponent<IDamageable>() ?? col.GetComponentInParent<IDamageable>();
            if (target != null && !damagedTargets.Contains(target))
            {
                damagedTargets.Add(target);
                hitAny = true;

                try
                {
                    target.TakeDamage(damage, damage * 0.5f, ElementType.Fire, damageDealer != null ? damageDealer : transform);
                }
                catch (System.Exception) { }

                Rigidbody2D targetRb = col.GetComponent<Rigidbody2D>() ?? col.GetComponentInParent<Rigidbody2D>();
                if (targetRb != null)
                {
                    targetRb.linearVelocity = new Vector2(facingDir * knockbackForce.x, knockbackForce.y);
                }
            }
        }

        if (hitAny && AudioManager.instance != null)
        {
            AudioManager.instance.PlayGlobalSFX("player_attackHit");
        }

        // 3. Cơ chế tắt hạt mượt mà: Dừng sinh hạt mới (StopEmitting), để khói và tàn lửa hiện có tan dần
        StartCoroutine(GracefulDespawnRoutine(0.45f));
    }

    private IEnumerator GracefulDespawnRoutine(float fadeDelay)
    {
        // Ngắt tất cả ParticleSystem để hạt hiện có bay nốt rồi tắt
        if (childParticleSystems != null)
        {
            foreach (var ps in childParticleSystems)
            {
                if (ps != null)
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        yield return new WaitForSeconds(fadeDelay);

        Destroy(gameObject);
    }

    private IEnumerator FailSafeDestroyRoutine(float timeout)
    {
        yield return new WaitForSeconds(timeout);
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, hitRadius);
        Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
