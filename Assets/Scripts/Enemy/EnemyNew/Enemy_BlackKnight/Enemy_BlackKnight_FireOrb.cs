using System.Collections;
using UnityEngine;

public class Enemy_BlackKnight_FireOrb : MonoBehaviour, ICounterable
{
    [Header("Flight Settings")]
    [SerializeField] private float flySpeed = 13f;
    [SerializeField] private float homingStrength = 3.5f;
    [SerializeField] private float homingDuration = 0.5f;
    [SerializeField] private float maxLifetime = 3.5f;
    [SerializeField] private bool canBeCountered = true;

    [Header("Targeting Settings")]
    [Tooltip("Fallback Y offset applied to target position if no Collider2D is found")]
    [SerializeField] private float bodyAimOffsetY = -0.3f;

    [Header("Impact Settings")]
    [SerializeField] private Vector2 colliderSize = new Vector2(0.9f, 0.9f);
    [SerializeField] private float burnDuration = 3f;
    [SerializeField] private float burnDamage = 10f;

    public bool CanBeCountered => canBeCountered;

    private Enemy_BlackKnight owner;
    private Transform target;
    private Collider2D targetCollider;
    private Vector3 localOffset;
    private float launchTime;
    private DamageScaleData damageScale;
    private LayerMask whatIsTarget;
    private LayerMask whatIsObstacle;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isLaunched = false;
    private bool hasHit = false;
    private Vector2 currentVelocity;
    private float launchTimestamp;
    private float floatPhase;

    public void SetupOrb(
        Enemy_BlackKnight owner,
        Transform target,
        Vector3 localOffset,
        float launchDelay,
        DamageScaleData damageScale,
        LayerMask targetMask,
        LayerMask obstacleMask)
    {
        this.owner = owner;
        this.target = target;
        this.targetCollider = target != null ? target.GetComponent<Collider2D>() : null;
        this.localOffset = localOffset;
        this.launchTime = Time.time + launchDelay;
        this.damageScale = damageScale;
        this.whatIsTarget = targetMask.value != 0 ? targetMask : LayerMask.GetMask("Player");
        this.whatIsObstacle = obstacleMask.value != 0 ? obstacleMask : LayerMask.GetMask("Ground");

        this.floatPhase = Random.Range(0f, Mathf.PI * 2f);

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        col = GetComponent<Collider2D>();
        if (col == null)
        {
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.size = colliderSize;
            col = box;
        }
        col.isTrigger = true;
        col.enabled = false; // Disabled while hovering above Boss

        // Ensure proper 2D sorting order so particles render above entities
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            r.sortingLayerName = "Default";
            r.sortingOrder = 24;
        }

        // Initial pop-in scale
        transform.localScale = Vector3.zero;
        StartCoroutine(PopInRoutine());
    }

    private IEnumerator PopInRoutine()
    {
        Vector3 targetScale = Vector3.one * 0.5f;
        float elapsed = 0f;
        float duration = 0.22f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, elapsed / duration);
            yield return null;
        }
        transform.localScale = targetScale;
    }

    private void Update()
    {
        if (hasHit)
            return;

        if (!isLaunched)
        {
            // Hover relative to Boss with subtle bobbing
            if (owner != null)
            {
                float bob = Mathf.Sin(Time.time * 5f + floatPhase) * 0.1f;
                transform.position = owner.transform.position + localOffset + new Vector3(0, bob, 0);
            }

            if (Time.time >= launchTime)
            {
                Launch();
            }
        }
        else
        {
            FlyStep();
        }
    }

    private Vector3 GetTargetCenterPosition()
    {
        if (target == null)
            return transform.position + Vector3.right * (owner != null ? owner.facingDir : 1);

        // Prioritize actual geometric center of target's collider (dead center of torso)
        if (targetCollider != null)
            return targetCollider.bounds.center;

        // Fallback using body aim offset
        return target.position + new Vector3(0f, bodyAimOffsetY, 0f);
    }

    private void Launch()
    {
        isLaunched = true;
        launchTimestamp = Time.time;

        if (col != null)
            col.enabled = true;

        // Aim directly at the center of the player's torso
        Vector3 targetPos = GetTargetCenterPosition();
        Vector2 dir = ((Vector2)targetPos - (Vector2)transform.position).normalized;
        if (dir.sqrMagnitude < 0.001f)
            dir = owner != null && owner.facingDir < 0 ? Vector2.left : Vector2.right;

        currentVelocity = dir * flySpeed;
        rb.linearVelocity = currentVelocity;

        RotateToVelocity();
        owner?.sfx?.PlayAttackHit();

        Destroy(gameObject, maxLifetime);
    }

    private void FlyStep()
    {
        // Mild homing tracking for the first homingDuration seconds towards torso center
        if (target != null && Time.time < launchTimestamp + homingDuration)
        {
            Vector3 targetPos = GetTargetCenterPosition();
            Vector2 desiredDir = ((Vector2)targetPos - (Vector2)transform.position).normalized;
            Vector2 newDir = Vector3.RotateTowards(currentVelocity.normalized, desiredDir, homingStrength * Time.deltaTime, 0f);
            currentVelocity = newDir * flySpeed;
            rb.linearVelocity = currentVelocity;
            RotateToVelocity();
        }
    }

    private void RotateToVelocity()
    {
        if (currentVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit || !isLaunched)
            return;

        // Hit Target (Player, or Enemy if deflected)
        if (((1 << collision.gameObject.layer) & whatIsTarget) != 0)
        {
            hasHit = true;
            if (owner != null)
            {
                owner.PerformAttackOnTarget(collision.transform, damageScale);
            }

            // Apply Burn status effect
            Entity_StatusHandler status = collision.GetComponent<Entity_StatusHandler>();
            if (status != null)
            {
                status.ApplyStatusEffect(ElementType.Fire, new ElementalEffectData
                {
                    burnDuratoin = burnDuration,
                    totalBurnDamage = burnDamage
                });
            }

            DestroyOrb();
            return;
        }

        // Hit Obstacle (Ground / Walls)
        if (((1 << collision.gameObject.layer) & whatIsObstacle) != 0)
        {
            hasHit = true;
            DestroyOrb();
        }
    }

    private void DestroyOrb()
    {
        if (col != null)
            col.enabled = false;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // Fade/stop particle emission
        ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>();
        foreach (var sys in systems)
        {
            var emission = sys.emission;
            emission.enabled = false;
        }

        Destroy(gameObject, 0.4f);
    }

    public void HandleCounter()
    {
        if (!canBeCountered || !isLaunched)
            return;

        target = owner != null ? owner.transform : null;
        targetCollider = owner != null ? owner.GetComponent<Collider2D>() : null;

        // Deflect back towards center of Boss body!
        Vector3 bossCenter = GetTargetCenterPosition();
        Vector2 dirToOwner = ((Vector2)bossCenter - (Vector2)transform.position).normalized;
        currentVelocity = dirToOwner * (flySpeed * 1.25f);

        rb.linearVelocity = currentVelocity;
        RotateToVelocity();

        // Switch target mask to hit Enemy layer
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
            whatIsTarget = (1 << enemyLayer);
    }
}
