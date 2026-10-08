using System.Collections.Generic;
using UnityEngine;

public class SkillObject_FlyingSword : SkillObject_Base
{
    public enum SwordPhase
    {
        DelayWait,
        AscendingToSky,
        HoveringInSky,
        SingleLockOn,
        GrandFinaleLockOn,
        Diving,
        SpinBlade,
        ImpaledOnGround,
        Impacted
    }

    private SwordPhase currentPhase = SwordPhase.DelayWait;
    public SwordPhase CurrentPhase => currentPhase;

    [Header("Slot & Delay")]
    private Transform playerTransform;
    private int slotIndex;
    private Vector3 slotOffset;
    private float spawnDelay = 0f;
    private float delayTimer = 0f;
    private Vector3 originalScale;

    [Header("Ascent to Sky")]
    private Vector3 startPoint;
    private Vector3 controlPoint;
    private Vector3 apexSlotPoint;
    [SerializeField] private float ascentDuration = 0.32f;
    private float ascentTimer = 0f;

    [Header("Hover in Sky")]
    private float hoverTimer = 0f;
    private Quaternion baseHoverRotation;
    private float floatSpeed;
    private float floatPhase;
    private float floatAmpX;
    private float floatAmpY;

    [Header("Lock-On & Recoil")]
    [SerializeField] private float singleLockOnDuration = 0.08f;
    [SerializeField] private float grandFinaleLockOnDuration = 0.25f;
    private float lockOnDuration;
    private float lockOnTimer = 0f;
    private Vector3 lockOnStartPos;
    private Transform targetEnemy;
    private Vector3 targetPosition;

    [Header("Dive Bomb")]
    [SerializeField] private float initialDiveSpeed = 38f;
    [SerializeField] private float maxDiveSpeed = 62f;
    [SerializeField] private float acceleration = 70f;
    private float currentDiveSpeed;
    private Vector3 diveDirection;
    private float maxLifetimeAfterDive = 2.0f;
    private bool isGrandFinale = false;

    [Header("Ground Impale")]
    [SerializeField] private float impaleDuration = 0.85f;
    private float impaleTimer = 0f;
    private Quaternion impaleRotation;

    [Header("Sword Throw Inherited Upgrades")]
    private SkillUpgradeType swordElement = SkillUpgradeType.None;
    private int pierceRemaining = 0;
    private int bounceRemaining = 0;
    private float spinAttackTimer = 0f;
    private static GameObject lightningVfxPrefab;
    private static GameObject iceBlastVfxPrefab;
    private readonly HashSet<Collider2D> enemiesHitAlready = new HashSet<Collider2D>();

    // References
    private Skill_DomainExpansion domainManager;
    private TrailRenderer trail;
    private Collider2D swordCollider;
    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();
        trail = GetComponentInChildren<TrailRenderer>();
        swordCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (swordCollider != null)
            swordCollider.enabled = false;

        originalScale = transform.localScale;
    }

    /// <summary>
    /// Khởi tạo phi kiếm gắn với slot trên bầu trời, kế thừa nguyên tố từ kỹ năng Ném Kiếm (Skill_SwordThrow).
    /// </summary>
    public void SetupSkySlotSword(
        Skill_DomainExpansion domainManager,
        Transform playerCenter,
        int slotIndex,
        Vector3 slotOffset,
        float spawnDelay)
    {
        this.domainManager = domainManager;
        this.playerStats = domainManager.player.stats;
        this.damageScaleData = domainManager.damageScaleData;
        this.playerTransform = playerCenter;
        this.slotIndex = slotIndex;
        this.slotOffset = slotOffset;
        this.spawnDelay = spawnDelay;

        startPoint = playerCenter != null ? playerCenter.position : transform.position;
        transform.position = startPoint;

        if (originalScale == Vector3.zero)
            originalScale = Vector3.one;

        // Chiều sâu xa - gần bằng biến thiên kích thước nhẹ
        float depthScale = Random.Range(0.88f, 1.15f);
        originalScale = originalScale * depthScale;

        apexSlotPoint = startPoint + slotOffset;
        controlPoint = startPoint + new Vector3(slotOffset.x * 0.3f, slotOffset.y * 0.95f, 0f);

        // Góc cắm thẳng đứng dọc xuống đất (-90 độ)
        baseHoverRotation = Quaternion.Euler(0, 0, -90f);

        floatSpeed = Random.Range(5.5f, 8.5f);
        floatPhase = Random.Range(0f, Mathf.PI * 2f);
        floatAmpX = Random.Range(0.02f, 0.04f);
        floatAmpY = Random.Range(0.035f, 0.065f);

        currentDiveSpeed = initialDiveSpeed;
        delayTimer = 0f;
        ascentTimer = 0f;
        hoverTimer = 0f;
        lockOnTimer = 0f;
        isGrandFinale = false;
        enemiesHitAlready.Clear();

        // ĐỒNG HÓA NGUYÊN TỐ & NÂNG CẤP TỪ KỸ NĂNG NÉM KIẾM
        Skill_SwordThrow swordThrow = domainManager != null && domainManager.skillManager != null ? domainManager.skillManager.swordThrow : null;
        swordElement = swordThrow != null ? swordThrow.GetUpgrade() : SkillUpgradeType.None;
        ApplyElementVisuals();

        if (spawnDelay > 0f)
        {
            currentPhase = SwordPhase.DelayWait;
            transform.localScale = Vector3.zero;
            if (trail != null)
            {
                trail.Clear();
                trail.emitting = false;
            }
        }
        else
        {
            StartAscent();
        }
    }

    private void ApplyElementVisuals()
    {
        if (spriteRenderer == null || trail == null)
            return;

        switch (swordElement)
        {
            case SkillUpgradeType.SwordThrow_Thunder:
                // Lôi Kiếm: Tím vàng sấm sét
                spriteRenderer.color = new Color(1f, 0.92f, 0.45f, 1f);
                trail.startColor = new Color(1f, 0.9f, 0.4f, 0.9f);
                trail.endColor = new Color(0.85f, 0.5f, 1f, 0f);
                break;

            case SkillUpgradeType.SwordThrow_Frost:
                // Băng Kiếm: Xanh tuyết băng giá
                spriteRenderer.color = new Color(0.75f, 0.95f, 1f, 1f);
                trail.startColor = new Color(0.65f, 0.95f, 1f, 0.9f);
                trail.endColor = new Color(0.35f, 0.65f, 1f, 0f);
                break;

            case SkillUpgradeType.SwordThrow_Pierce:
                // Kiếm Xuyên: Đỏ cam sắc nhọn
                spriteRenderer.color = new Color(1f, 0.6f, 0.4f, 1f);
                trail.startColor = new Color(1f, 0.55f, 0.3f, 0.9f);
                trail.endColor = new Color(1f, 0.2f, 0.1f, 0f);
                pierceRemaining = 2;
                break;

            case SkillUpgradeType.SwordThrow_Spin:
                // Kiếm Xoay: Xanh lục bảo hoàng kim
                spriteRenderer.color = new Color(0.55f, 1f, 0.75f, 1f);
                trail.startColor = new Color(0.5f, 1f, 0.7f, 0.9f);
                trail.endColor = new Color(0.1f, 0.8f, 0.4f, 0f);
                break;

            case SkillUpgradeType.SwordThrow_Bounce:
                // Kiếm Nảy: Hoàng kim lấp lánh
                spriteRenderer.color = new Color(1f, 0.85f, 0.35f, 1f);
                trail.startColor = new Color(1f, 0.85f, 0.3f, 0.9f);
                trail.endColor = new Color(1f, 0.5f, 0.1f, 0f);
                bounceRemaining = 1;
                break;

            default:
                // Kiếm thuần khiết: Cyan lam ngọc
                spriteRenderer.color = new Color(0.75f, 0.92f, 1f, 1f);
                trail.startColor = new Color(0.3f, 0.9f, 1f, 0.85f);
                trail.endColor = new Color(0f, 0.5f, 1f, 0f);
                break;
        }
    }

    private void StartAscent()
    {
        currentPhase = SwordPhase.AscendingToSky;
        ascentTimer = 0f;
        transform.localScale = originalScale;

        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
        }

        Vector3 initialTangent = (controlPoint - startPoint).normalized;
        if (initialTangent != Vector3.zero)
            transform.right = initialTangent;
    }

    private void Update()
    {
        switch (currentPhase)
        {
            case SwordPhase.DelayWait:
                HandleDelayWait();
                break;
            case SwordPhase.AscendingToSky:
                HandleAscendingToSky();
                break;
            case SwordPhase.HoveringInSky:
                HandleHoveringInSky();
                break;
            case SwordPhase.SingleLockOn:
            case SwordPhase.GrandFinaleLockOn:
                HandleLockOn();
                break;
            case SwordPhase.Diving:
                HandleDiving();
                break;
            case SwordPhase.SpinBlade:
                HandleSpinBlade();
                break;
            case SwordPhase.ImpaledOnGround:
                HandleImpaledOnGround();
                break;
            case SwordPhase.Impacted:
                break;
        }
    }

    private void HandleDelayWait()
    {
        delayTimer += Time.deltaTime;
        if (delayTimer >= spawnDelay)
        {
            StartAscent();
        }
    }

    private void HandleAscendingToSky()
    {
        ascentTimer += Time.deltaTime;
        float t = Mathf.Clamp01(ascentTimer / ascentDuration);

        Vector3 center = playerTransform != null ? playerTransform.position : startPoint;
        Vector3 curApex = center + slotOffset;
        Vector3 curControl = center + new Vector3(slotOffset.x * 0.3f, slotOffset.y * 0.95f, 0f);

        // Quadratic Bezier
        float u = 1f - t;
        Vector3 newPos = (u * u * center) + (2f * u * t * curControl) + (t * t * curApex);
        transform.position = newPos;

        Vector3 tangent = 2f * u * (curControl - center) + 2f * t * (curApex - curControl);
        if (tangent.sqrMagnitude > 0.001f)
            transform.right = tangent.normalized;

        if (t >= 1f)
        {
            currentPhase = SwordPhase.HoveringInSky;
            hoverTimer = 0f;
            transform.position = curApex;
            transform.rotation = baseHoverRotation;
        }
    }

    private void HandleHoveringInSky()
    {
        hoverTimer += Time.deltaTime;

        Vector3 center = playerTransform != null ? playerTransform.position : startPoint;
        Vector3 curSlot = center + slotOffset;

        float curX = Mathf.Cos(Time.time * floatSpeed + floatPhase) * floatAmpX;
        float curY = Mathf.Sin(Time.time * floatSpeed + floatPhase) * floatAmpY;
        transform.position = curSlot + new Vector3(curX, curY, 0);

        transform.rotation = baseHoverRotation;
    }

    /// <summary>
    /// Bắn đơn lẻ trong giai đoạn oanh tạc & bù mới liên tục (sát thương cấu rỉa).
    /// </summary>
    public void LaunchSingleSword(Transform target, Vector3 fallbackPos)
    {
        if (currentPhase != SwordPhase.HoveringInSky)
            return;

        this.targetEnemy = target;
        this.targetPosition = target != null ? target.position : fallbackPos;
        this.isGrandFinale = false;

        currentPhase = SwordPhase.SingleLockOn;
        lockOnDuration = singleLockOnDuration;
        lockOnTimer = 0f;
        lockOnStartPos = transform.position;
    }

    /// <summary>
    /// Đại kết thúc: Toàn bộ kiếm trên trời cùng dội bom với sát thương tối đa.
    /// </summary>
    public void TriggerGrandFinale(Transform target, Vector3 fallbackPos)
    {
        if (currentPhase != SwordPhase.HoveringInSky && currentPhase != SwordPhase.AscendingToSky)
            return;

        this.targetEnemy = target;
        this.targetPosition = target != null ? target.position : fallbackPos;
        this.isGrandFinale = true;

        currentPhase = SwordPhase.GrandFinaleLockOn;
        lockOnDuration = grandFinaleLockOnDuration;
        lockOnTimer = 0f;
        lockOnStartPos = transform.position;
    }

    private void HandleLockOn()
    {
        lockOnTimer += Time.deltaTime;

        if (targetEnemy != null && targetEnemy.gameObject.activeInHierarchy)
            targetPosition = targetEnemy.position;

        Vector3 jitter = (Vector3)(Random.insideUnitCircle * 0.04f);
        transform.position = lockOnStartPos + jitter;

        Vector3 dirToTarget = (targetPosition - transform.position).normalized;
        if (dirToTarget != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 35f);
        }

        if (lockOnTimer >= lockOnDuration)
        {
            currentPhase = SwordPhase.Diving;
            diveDirection = (targetPosition - transform.position).normalized;
            if (diveDirection == Vector3.zero)
                diveDirection = Vector3.down;

            transform.right = diveDirection;

            if (swordCollider != null)
                swordCollider.enabled = true;

            Destroy(gameObject, maxLifetimeAfterDive);
        }
    }

    private void HandleDiving()
    {
        currentDiveSpeed = Mathf.MoveTowards(currentDiveSpeed, maxDiveSpeed, acceleration * Time.deltaTime);

        // Homing bám theo mục tiêu
        if (targetEnemy != null && targetEnemy.gameObject.activeInHierarchy)
        {
            Vector3 desiredDir = (targetEnemy.position - transform.position).normalized;
            diveDirection = Vector3.RotateTowards(diveDirection, desiredDir, 4.0f * Time.deltaTime, 0f);
        }

        transform.position += diveDirection * currentDiveSpeed * Time.deltaTime;
        transform.right = diveDirection;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentPhase != SwordPhase.Diving)
            return;

        // Va chạm kẻ địch
        if ((whatIsEnemy.value & (1 << collision.gameObject.layer)) > 0)
        {
            if (enemiesHitAlready.Contains(collision))
                return;

            enemiesHitAlready.Add(collision);
            OnSwordHitEnemy(collision.transform);
        }
        // Va chạm mặt đất
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.CompareTag("Ground"))
        {
            OnSwordHitGround();
        }
    }

    private void OnSwordHitEnemy(Transform enemyTransform)
    {
        TriggerElementHitEffects();

        // Cân bằng sát thương: Bắn tỉa lẻ = 0.35x, Đại kết thúc Grand Finale = 1.0x
        float damageMultiplier = isGrandFinale ? 1.0f : 0.35f;
        DamageEnemiesInRadius(transform, checkRadius);

        // NÂNG CẤP KIẾM XUYÊN (Pierce): Xuyên qua quái, không dừng lại!
        if (swordElement == SkillUpgradeType.SwordThrow_Pierce && pierceRemaining > 0)
        {
            pierceRemaining--;
            return; // Tiếp tục bay xuyên qua!
        }

        // NÂNG CẤP KIẾM NẢY (Bounce): Nảy sang con quái thứ 2
        if (swordElement == SkillUpgradeType.SwordThrow_Bounce && bounceRemaining > 0)
        {
            bounceRemaining--;
            Transform nextTarget = FindNextBounceTarget(enemyTransform);
            if (nextTarget != null)
            {
                targetEnemy = nextTarget;
                targetPosition = nextTarget.position;
                diveDirection = (targetPosition - transform.position).normalized;
                transform.right = diveDirection;
                currentDiveSpeed = initialDiveSpeed * 1.2f;
                return; // Nảy sang quái tiếp theo!
            }
        }

        currentPhase = SwordPhase.Impacted;
        if (swordCollider != null)
            swordCollider.enabled = false;

        if (trail != null)
            trail.autodestruct = true;

        Destroy(gameObject, 0.05f);
    }

    private void OnSwordHitGround()
    {
        TriggerElementHitEffects();

        // NÂNG CẤP KIẾM XOAY (Spin): Cắm đất xoay tít chém liên hoàn!
        if (swordElement == SkillUpgradeType.SwordThrow_Spin)
        {
            currentPhase = SwordPhase.SpinBlade;
            impaleTimer = 0f;
            spinAttackTimer = 0f;
            if (swordCollider != null)
                swordCollider.enabled = false;
            return;
        }

        currentPhase = SwordPhase.ImpaledOnGround;
        impaleTimer = 0f;
        impaleRotation = transform.rotation;

        if (swordCollider != null)
            swordCollider.enabled = false;

        if (trail != null)
            trail.emitting = false;
    }

    private void TriggerElementHitEffects()
    {
        // Hiệu ứng Lôi Kiếm: Giáng sấm sét
        if (swordElement == SkillUpgradeType.SwordThrow_Thunder && lightningVfxPrefab != null)
        {
            Instantiate(lightningVfxPrefab, transform.position, Quaternion.identity);
        }
        // Hiệu ứng Băng Kiếm: Nổ hàn băng
        else if (swordElement == SkillUpgradeType.SwordThrow_Frost && iceBlastVfxPrefab != null)
        {
            Instantiate(iceBlastVfxPrefab, transform.position, Quaternion.identity);
        }
    }

    private void HandleSpinBlade()
    {
        impaleTimer += Time.deltaTime;
        spinAttackTimer -= Time.deltaTime;

        // Xoay tít lưỡi kiếm
        transform.Rotate(0, 0, 950f * Time.deltaTime);

        // Chém quét liên hồi mỗi 0.25s
        if (spinAttackTimer <= 0f)
        {
            spinAttackTimer = 0.25f;
            DamageEnemiesInRadius(transform, checkRadius * 1.3f);
        }

        // Mờ dần và biến mất sau 1.0s
        if (impaleTimer >= 0.7f && spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = Mathf.MoveTowards(c.a, 0f, Time.deltaTime * 3.5f);
            spriteRenderer.color = c;
        }

        if (impaleTimer >= 1.0f)
        {
            Destroy(gameObject);
        }
    }

    private void HandleImpaledOnGround()
    {
        impaleTimer += Time.deltaTime;

        if (impaleTimer < 0.45f)
        {
            float damp = 1f - (impaleTimer / 0.45f);
            float shakeAngle = Mathf.Sin(impaleTimer * 55f) * 4.5f * damp;
            transform.rotation = impaleRotation * Quaternion.Euler(0, 0, shakeAngle);
        }

        if (impaleTimer >= 0.5f && spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = Mathf.MoveTowards(c.a, 0f, Time.deltaTime * 4f);
            spriteRenderer.color = c;
        }

        if (impaleTimer >= impaleDuration)
        {
            Destroy(gameObject);
        }
    }

    private Transform FindNextBounceTarget(Transform currentTarget)
    {
        Collider2D[] candidates = Physics2D.OverlapCircleAll(transform.position, 4.5f, whatIsEnemy);
        Transform closest = null;
        float closestDist = float.MaxValue;

        foreach (var c in candidates)
        {
            if (c.transform == currentTarget || enemiesHitAlready.Contains(c))
                continue;

            float dist = Vector2.Distance(transform.position, c.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = c.transform;
            }
        }
        return closest;
    }
}
