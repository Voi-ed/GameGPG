using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ArchDemon_DarkOrb : MonoBehaviour
{
    [Header("Orb Movement")]
    [SerializeField] private float speed = 11.0f;
    [SerializeField] private float lifetime = 3.5f;

    [Header("Targeting & Damage")]
    [SerializeField] private LayerMask whatIsTarget;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float damageRadius = 1.0f;

    private Enemy owner;
    private DamageScaleData damageScale;
    private Vector2 direction = Vector2.right;
    private bool hasExploded = false;

    public void Launch(Enemy caster, Vector2 shootDirection, DamageScaleData damageData, LayerMask targetMask, LayerMask groundMask)
    {
        this.owner = caster;
        this.direction = shootDirection.normalized;
        this.damageScale = damageData;
        if (targetMask.value != 0) whatIsTarget = targetMask;
        if (groundMask.value != 0) whatIsGround = groundMask;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (hasExploded) return;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasExploded) return;

        // Bỏ qua va chạm với chủ nhân
        if (owner != null && collision.gameObject == owner.gameObject) return;

        int layerMask = 1 << collision.gameObject.layer;
        if ((whatIsTarget.value & layerMask) != 0 || (whatIsGround.value & layerMask) != 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, damageRadius, whatIsTarget);
        var damagedTargets = new System.Collections.Generic.HashSet<IDamageable>();

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                if (damagedTargets.Add(damageable))
                {
                    if (owner != null && owner.combat != null)
                    {
                        owner.combat.PerformAttackOnTarget(hit.transform, damageScale);
                    }
                }
            }
        }

        Destroy(gameObject, 0.05f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
