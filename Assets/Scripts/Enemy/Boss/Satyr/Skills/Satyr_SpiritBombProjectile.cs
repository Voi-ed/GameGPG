using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Satyr_SpiritBombProjectile : MonoBehaviour
{
    [Header("Visual & Scaling")]
    [SerializeField] private GameObject visualPrefab;
    [SerializeField] private float flySpeed = 16f;
    [SerializeField] private float targetScale = 1.0f;

    [Header("Impact & Damage Settings")]
    [SerializeField] private float damageRadius = 2.0f;
    [SerializeField] private GameObject explosionVfxPrefab;
    [SerializeField] private LayerMask whatIsTarget;
    [SerializeField] private LayerMask whatIsGround;

    private Enemy_Satyr satyr;
    private DamageScaleData damageScale;
    private GameObject visualInstance;
    private Transform anchor;
    private Vector3 targetPos;

    private bool isCharging = false;
    private bool isLaunched = false;
    private float chargeTimer = 0f;
    private float chargeDuration = 1.0f;
    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    public void Charge(Enemy_Satyr owner, Transform chargeAnchor, float duration, DamageScaleData damageScaleData)
    {
        this.satyr = owner;
        this.anchor = chargeAnchor;
        this.chargeDuration = duration;
        this.damageScale = damageScaleData;

        if (whatIsTarget.value == 0 && owner != null && owner.WhatIsPlayer.value != 0)
            whatIsTarget = owner.WhatIsPlayer;

        if (whatIsGround.value == 0 && owner != null)
            whatIsGround = owner.whatIsGround;

        transform.position = anchor != null ? anchor.position : (owner != null ? owner.transform.position + Vector3.up * 2f : transform.position);
        transform.localScale = Vector3.zero;

        isCharging = true;
        isLaunched = false;
        chargeTimer = 0f;

        if (col != null)
            col.enabled = false;

        if (visualPrefab != null && visualInstance == null)
        {
            visualInstance = Instantiate(visualPrefab, transform);
            visualInstance.transform.localPosition = Vector3.zero;
            visualInstance.transform.localRotation = Quaternion.identity;
        }

        // Ensure particles render clearly on top of sprites
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            r.sortingLayerName = "Default";
            r.sortingOrder = 35;
        }
    }

    public void Setup(Enemy_Satyr owner, Transform chargeAnchor, float duration, float scale, LayerMask targetMask, LayerMask groundMask)
    {
        this.targetScale = scale;
        if (targetMask.value != 0) whatIsTarget = targetMask;
        if (groundMask.value != 0) whatIsGround = groundMask;
        Charge(owner, chargeAnchor, duration, null);
    }

    private void Update()
    {
        if (isCharging)
        {
            if (anchor != null)
                transform.position = anchor.position;

            chargeTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(chargeTimer / chargeDuration);
            transform.localScale = Vector3.one * (progress * targetScale);
        }
        else if (isLaunched)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, flySpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.2f)
            {
                ExplodeAtGround();
            }
        }
    }

    public void Launch(Vector3 targetFloorPosition)
    {
        isCharging = false;
        isLaunched = true;
        this.targetPos = targetFloorPosition;
        transform.parent = null;

        if (col != null)
            col.enabled = true;
    }

    public void Interrupt()
    {
        isCharging = false;
        isLaunched = false;

        if (col != null)
            col.enabled = false;

        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (visualInstance != null)
        {
            Destroy(visualInstance);
            visualInstance = null;
        }

        Destroy(gameObject, 0.1f);
    }

    public void InterruptAndExplode()
    {
        Interrupt();
    }

    private void ExplodeAtGround()
    {
        isLaunched = false;

        if (col != null)
            col.enabled = false;

        if (visualInstance != null)
        {
            Destroy(visualInstance);
            visualInstance = null;
        }

        if (explosionVfxPrefab != null)
        {
            GameObject vfx = Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);
            Renderer[] rList = vfx.GetComponentsInChildren<Renderer>(true);
            foreach (var r in rList)
            {
                r.sortingLayerName = "Default";
                r.sortingOrder = 35;
            }
            Destroy(vfx, 2.5f);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, damageRadius, whatIsTarget);
        if (satyr != null && satyr.combat != null)
        {
            foreach (var hit in hits)
            {
                satyr.PerformAttackOnTarget(hit.transform, damageScale);
            }
        }

        Destroy(gameObject, 0.05f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ nổ sớm khi chạm đất. Chạm player không nổ ngay, để quả cầu bay tới giữa thân rồi mới nổ (Update xử lý)
        if (isLaunched && ((1 << other.gameObject.layer) & whatIsGround.value) != 0)
        {
            ExplodeAtGround();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
