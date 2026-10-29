using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Điều khiển Cột Lửa Ma Quái (Hellfire Pillar) của Rồng khi kích hoạt kỹ năng Long Nộ Địa Ngục (Right Click).
/// - Tự động bắt điểm tiếp đất (snapToGround) ngay dưới chân mục tiêu để vòng ma trận lửa bám sàn chuẩn xác.
/// - Đảm bảo sorting layer/order hiển thị phía trước sàn đấu và quái vật.
/// - Gây sát thương Hỏa diện rộng (AoE), hất tung quái vật lên trời (Knock-up) và tắt hạt mượt mà (Graceful Despawn).
/// </summary>
public class Dragon_HellfirePillar : MonoBehaviour
{
    [Header("Detection & Damage")]
    [SerializeField] private float damageRadius = 2.4f;
    [SerializeField] private Vector2 damageOffset = new Vector2(0f, 1.2f);
    [SerializeField] private Vector2 knockbackForce = new Vector2(3.5f, 10.5f);
    [SerializeField] private float lifetime = 1.8f;
    [SerializeField] private float activeDamageDuration = 0.4f;
    [SerializeField] private LayerMask whatIsEnemy;

    [Header("Ground Snapping")]
    [SerializeField] private bool snapToGround = true;
    [SerializeField] private float groundCheckDistance = 30.0f;
    [SerializeField] private LayerMask whatIsGround;

    private float damage;
    private int facingDir = 1;
    private Transform damageDealer;
    private float spawnTime;
    private bool isDespawning = false;
    private bool initialized = false;
    private ParticleSystem[] childParticleSystems;
    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    public void Setup(float damage, int facingDir, Transform damageDealer, LayerMask enemyMask, LayerMask groundMask)
    {
        this.damage = damage;
        this.facingDir = facingDir >= 0 ? 1 : -1;
        this.damageDealer = damageDealer;
        this.spawnTime = Time.time;

        if (enemyMask.value != 0)
            this.whatIsEnemy = enemyMask;
        else
            this.whatIsEnemy = LayerMask.GetMask("Enemy", "InteractiveObjects");

        if (groundMask.value != 0)
            this.whatIsGround = groundMask;
        else
            this.whatIsGround = LayerMask.GetMask("Ground", "Default");

        // Bắt điểm tiếp đất ngay dưới chân mục tiêu (bảo đảm luôn bám sàn tới 30m)
        if (snapToGround)
        {
            float checkDist = Mathf.Max(groundCheckDistance, 30f);
            RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.up * 1.5f, Vector2.down, checkDist, whatIsGround);
            if (hit.collider != null)
            {
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            }
        }

        // Đảm bảo sorting layer 2D để Particle System không bị che khuất và tắt loop
        childParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in childParticleSystems)
        {
            var main = ps.main;
            main.loop = false;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            r.sortingLayerName = "Default";
            r.sortingOrder = 25;
        }

        initialized = true;

        // Quét đợt nổ đầu tiên ngay khi cột lửa bùng lên
        ApplyAoEDamage();

        // Hẹn giờ tắt hạt mượt mà (Graceful Despawn)
        StartCoroutine(GracefulLifecycleRoutine());
    }

    private void Start()
    {
        if (!initialized)
        {
            Setup(150f, 1, transform, whatIsEnemy, whatIsGround);
        }
    }

    private void Update()
    {
        if (!initialized || isDespawning)
            return;

        // Tiếp tục quét sát thương trong 0.4s đầu tiên khi cột lửa đang bùng mạnh nhất
        if (Time.time - spawnTime <= activeDamageDuration)
        {
            ApplyAoEDamage();
        }
    }

    private IEnumerator GracefulLifecycleRoutine()
    {
        float emitTime = Mathf.Max(0.4f, lifetime - 0.5f);
        yield return new WaitForSeconds(emitTime);

        isDespawning = true;
        if (childParticleSystems != null)
        {
            foreach (var ps in childParticleSystems)
            {
                if (ps != null)
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        yield return new WaitForSeconds(0.55f);

        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyAoEDamage()
    {
        Vector2 center = (Vector2)transform.position + damageOffset;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, damageRadius, whatIsEnemy);

        foreach (var col in colliders)
        {
            if (damageDealer != null && col.transform.root == damageDealer.root)
                continue;

            IDamageable target = col.GetComponent<IDamageable>() ?? col.GetComponentInParent<IDamageable>();
            if (target != null && !hitTargets.Contains(target))
            {
                hitTargets.Add(target);

                try
                {
                    target.TakeDamage(damage, damage * 0.5f, ElementType.Fire, damageDealer != null ? damageDealer : transform);
                }
                catch (System.Exception) { }

                if (AudioManager.instance != null)
                    AudioManager.instance.PlayGlobalSFX("player_attackHit");

                Rigidbody2D rb = col.GetComponent<Rigidbody2D>() ?? col.GetComponentInParent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(facingDir * knockbackForce.x, knockbackForce.y);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + damageOffset, damageRadius);
    }
}
