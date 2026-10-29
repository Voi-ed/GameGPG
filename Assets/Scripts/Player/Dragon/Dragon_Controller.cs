using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bộ điều khiển độc lập cho Quái Thú Rồng Bóng Tối (Shadow Demon Dragon).
/// Tích hợp State Machine đầy đủ (Dragon_IdleState, Dragon_MoveState, Dragon_AttackState, Dragon_JumpState, Dragon_DeadState)
/// Thiết kế chuẩn theo kiến trúc Player_BasicAttackState: Input Buffering, nối combo 3 đòn trực tiếp không qua Idle.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Dragon_Controller : MonoBehaviour, IDamageable
{
    [Header("Dragon - Movement")]
    [SerializeField] public float moveSpeed = 8f;
    [SerializeField] public float jumpForce = 13.5f;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.45f;

    [Header("Dragon - Combat (Left Click Attack)")]
    [SerializeField] private float attackDamage = 110f;
    [SerializeField] private float attackCooldown = 0.45f;
    [SerializeField] private float attackRange = 2.4f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private Vector2 knockbackForce = new Vector2(9f, 4.5f);
    [SerializeField] private GameObject hitVfxPrefab;
    [SerializeField] private GameObject clawSlashVfxPrefab;

    [Header("Dragon - Skill (Right Click Hellfire Roar)")]
    [SerializeField] private GameObject hellfirePillarPrefab;
    [SerializeField] private float skillDamage = 250f;
    [SerializeField] private float skillCooldown = 5.0f;
    [SerializeField] private float skillTargetRange = 11f;
    [SerializeField] private int maxPillarTargets = 3;
    [SerializeField] private float pillarChainDelay = 0.12f;
    private float lastSkillTime = float.NegativeInfinity;

    [Header("Dragon - Fireball Skill (Left Shift)")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private float fireballDamage = 220f;
    [SerializeField] private float fireballSpeed = 14f;
    [SerializeField] private float fireballCooldown = 3.0f;
    private float lastFireballTime = float.NegativeInfinity;

    [Header("Dragon - Stats & Defense")]
    [SerializeField] private float maxHealth = 1500f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool hasSuperArmor = true;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float footstepInterval = 0.42f;

    // Components
    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public Collider2D col { get; private set; }

    // State Machine
    public StateMachine stateMachine { get; private set; }
    public Dragon_IdleState idleState { get; private set; }
    public Dragon_MoveState moveState { get; private set; }
    public Dragon_AttackState attackState { get; private set; }
    public Dragon_SkillState skillState { get; private set; }
    public Dragon_FireballState fireballState { get; private set; }
    public Dragon_JumpState jumpState { get; private set; }
    public Dragon_DeadState deadState { get; private set; }

    // Input & State Properties
    public PlayerInputSet input { get; private set; }
    public Vector2 moveInput { get; private set; }
    public bool isGrounded { get; private set; }
    public bool isFacingRight { get; private set; } = true;
    public int facingDir { get; private set; } = 1;
    public bool isDead { get; private set; } = false;

    // Expose movement configs for states
    public float MoveSpeed => moveSpeed;
    public float JumpForce => jumpForce;

    private float lastFootstepTime = float.NegativeInfinity;

    // Actions
    public event Action OnTakingDamage;
    public event Action OnDeath;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();

        currentHealth = maxHealth;

        // Auto find AttackPoint / GroundCheck if missing
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0, -0.1f, 0);
            groundCheck = gc.transform;
        }

        if (attackPoint == null)
        {
            GameObject ap = new GameObject("AttackPoint");
            ap.transform.SetParent(transform);
            ap.transform.localPosition = new Vector3(1.6f, 1.2f, 0);
            attackPoint = ap.transform;
        }

        if (whatIsEnemy == 0)
            whatIsEnemy = LayerMask.GetMask("Enemy", "InteractiveObjects");
        if (whatIsGround == 0)
            whatIsGround = LayerMask.GetMask("Ground", "Default");

        if (input == null)
            input = new PlayerInputSet();

        // Khởi tạo State Machine chuẩn
        stateMachine = new StateMachine();
        idleState = new Dragon_IdleState(this, stateMachine, "idle");
        moveState = new Dragon_MoveState(this, stateMachine, "move");
        attackState = new Dragon_AttackState(this, stateMachine, "attack");
        skillState = new Dragon_SkillState(this, stateMachine, "skill");
        fireballState = new Dragon_FireballState(this, stateMachine, "fireball");
        jumpState = new Dragon_JumpState(this, stateMachine, "jump");
        deadState = new Dragon_DeadState(this, stateMachine, "dead");
    }

    private void Start()
    {
        stateMachine.Initialize(idleState);
    }

    private void OnEnable()
    {
        if (input == null)
            input = new PlayerInputSet();

        input.Enable();
    }

    private void OnDisable()
    {
        if (input != null)
            input.Disable();
    }

    private void Update()
    {
        if (isDead)
            return;

        if (input != null)
        {
            moveInput = input.Player.Movement.ReadValue<Vector2>();

            // Bấm Chuột Phải để xả kỹ năng Long Nộ Địa Ngục (Hellfire Roar)
            if (input.Player.RangeAttack.WasPressedThisFrame())
            {
                TryCastSkill();
            }

            // Bấm Left Shift (Dash) để hạ thân tụ khí bắn Hắc Hỏa Cầu (FX_Fire_03_2)
            if (input.Player.Dash.WasPressedThisFrame())
            {
                TryCastFireball();
            }
        }

        CheckGround();
        HandleFlip();
        HandleFootsteps();

        stateMachine.UpdateActiveState();
    }

    public void CheckGround()
    {
        if (groundCheck == null) return;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
    }

    private void HandleFlip()
    {
        if (moveInput.x > 0.05f && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput.x < -0.05f && isFacingRight)
        {
            Flip();
        }
    }

    public void Flip()
    {
        transform.Rotate(0, 180, 0);
        isFacingRight = !isFacingRight;
        facingDir *= -1;
    }

    private void HandleFootsteps()
    {
        if (isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.5f && Time.time - lastFootstepTime > footstepInterval)
        {
            lastFootstepTime = Time.time;
            if (AudioManager.instance != null && audioSource != null)
            {
                AudioManager.instance.PlaySFX("dragon_footstep", audioSource, 15f);
            }
        }
    }

    #region Dragon Attack & Animation Triggers

    /// <summary>
    /// Kích hoạt kỹ năng Long Nộ Địa Ngục (Hellfire Roar).
    /// Bắt buộc Rồng phải đứng trên mặt đất mới được gồng nộ triệu hồi Cột Lửa.
    /// </summary>
    public bool TryCastSkill()
    {
        if (!isGrounded)
            return false;

        if (Time.time < lastSkillTime + skillCooldown)
            return false;

        if (stateMachine.currentState == deadState || stateMachine.currentState == skillState || stateMachine.currentState == fireballState)
            return false;

        lastSkillTime = Time.time;
        stateMachine.ChangeState(skillState);
        return true;
    }

    /// <summary>
    /// Kích hoạt kỹ năng Hắc Hỏa Cầu (Left Shift): Rồng nằm xuống tụ khí rồi bắn cầu lửa FX_Fire_03_2.
    /// </summary>
    public bool TryCastFireball()
    {
        if (Time.time < lastFireballTime + fireballCooldown)
            return false;

        if (stateMachine.currentState == deadState || stateMachine.currentState == skillState || stateMachine.currentState == fireballState)
            return false;

        lastFireballTime = Time.time;
        stateMachine.ChangeState(fireballState);
        return true;
    }

    /// <summary>
    /// Được gọi bởi Dragon_AnimationTriggers khi frame vuốt cào vung trúng địch, frame rồng gầm nộ hoặc frame bắn cầu lửa (AttackTrigger Event).
    /// </summary>
    public void AttackTrigger()
    {
        if (stateMachine.currentState == skillState)
        {
            SpawnHellfirePillar();
        }
        else if (stateMachine.currentState == fireballState)
        {
            SpawnFireballProjectile();
            fireballState?.ApplyRecoil();
        }
        else
        {
            if (AudioManager.instance != null)
            {
                if (audioSource != null)
                    AudioManager.instance.PlaySFX("dragon_attack", audioSource, 20f);
                else
                    AudioManager.instance.PlayGlobalSFX("dragon_attack");
            }

            ExecuteClawDamage();
        }
    }

    public void SpawnFireballProjectile()
    {
        if (AudioManager.instance != null)
        {
            if (audioSource != null)
                AudioManager.instance.PlaySFX("dragon_attack", audioSource, 25f);
            else
                AudioManager.instance.PlayGlobalSFX("dragon_attack");
        }

        if (fireballPrefab == null)
            return;

        Vector3 spawnPos = attackPoint != null
            ? attackPoint.position + new Vector3(facingDir * 0.2f, -0.25f, 0f)
            : transform.position + new Vector3(facingDir * 2.8f, -0.2f, 0f);

        GameObject fbGO = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
        Dragon_FireballProjectile proj = fbGO.GetComponent<Dragon_FireballProjectile>();
        if (proj == null)
            proj = fbGO.AddComponent<Dragon_FireballProjectile>();

        proj.Setup(fireballDamage, fireballSpeed, facingDir, transform, whatIsEnemy, whatIsGround);
    }

    public void SpawnHellfirePillar()
    {
        if (AudioManager.instance != null)
        {
            if (audioSource != null)
                AudioManager.instance.PlaySFX("dragon_attack", audioSource, 25f);
            else
                AudioManager.instance.PlayGlobalSFX("dragon_attack");
        }

        if (hellfirePillarPrefab == null)
            return;

        // Quét tìm các Enemy trong tầm gầm (ưu tiên hướng Rồng đang nhìn, sắp xếp từ gần ra xa)
        LayerMask enemyMask = whatIsEnemy.value != 0 ? whatIsEnemy : LayerMask.GetMask("Enemy", "InteractiveObjects");
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, skillTargetRange, enemyMask);

        List<Collider2D> validTargets = new List<Collider2D>();
        HashSet<Transform> seenTargets = new HashSet<Transform>();

        foreach (var hit in hits)
        {
            if (hit == null || hit.transform.root == transform.root)
                continue;

            IDamageable damageable = hit.GetComponent<IDamageable>() ?? hit.GetComponentInParent<IDamageable>();
            if (damageable == null)
                continue;

            Entity_Health health = hit.GetComponent<Entity_Health>() ?? hit.GetComponentInParent<Entity_Health>();
            if (health != null && health.isDead)
                continue;

            Transform targetTransform = (damageable is MonoBehaviour mb) ? mb.transform : hit.transform.root;
            if (!seenTargets.Contains(targetTransform))
            {
                seenTargets.Add(targetTransform);
                validTargets.Add(hit);
            }
        }

        if (validTargets.Count > 0)
        {
            // Ưu tiên mục tiêu trước mặt, sau đó sắp xếp theo khoảng cách từ gần tới xa để nổ dây chuyền
            validTargets.Sort((a, b) =>
            {
                bool aInFront = (a.transform.position.x - transform.position.x) * facingDir >= -0.8f;
                bool bInFront = (b.transform.position.x - transform.position.x) * facingDir >= -0.8f;
                if (aInFront != bInFront)
                    return aInFront ? -1 : 1;

                float distA = Vector2.SqrMagnitude(a.transform.position - transform.position);
                float distB = Vector2.SqrMagnitude(b.transform.position - transform.position);
                return distA.CompareTo(distB);
            });

            int count = Mathf.Min(maxPillarTargets, validTargets.Count);
            StartCoroutine(SpawnPillarChainRoutine(validTargets.GetRange(0, count)));
        }
        else
        {
            // Fallback khi xung quanh không có quái: Triệu hồi cột lửa ở cự ly chuẩn 3.5m trước mặt
            Vector3 fallbackPos = transform.position + new Vector3(facingDir * 3.5f, 0f, 0f);
            InstantiateHellfirePillarAt(fallbackPos);
        }
    }

    private IEnumerator SpawnPillarChainRoutine(List<Collider2D> targets)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            Collider2D targetCol = targets[i];
            if (targetCol != null)
            {
                Vector3 footPos = new Vector3(targetCol.bounds.center.x, targetCol.bounds.min.y, transform.position.z);
                InstantiateHellfirePillarAt(footPos);
            }

            if (i < targets.Count - 1 && pillarChainDelay > 0f)
            {
                yield return new WaitForSeconds(pillarChainDelay);
            }
        }
    }

    private void InstantiateHellfirePillarAt(Vector3 spawnPos)
    {
        if (hellfirePillarPrefab == null)
            return;

        Vector3 groundPos = GetGroundedSpawnPosition(spawnPos);
        GameObject pillarGO = Instantiate(hellfirePillarPrefab, groundPos, Quaternion.identity);
        Dragon_HellfirePillar pillar = pillarGO.GetComponent<Dragon_HellfirePillar>();
        if (pillar == null)
            pillar = pillarGO.AddComponent<Dragon_HellfirePillar>();

        pillar.Setup(skillDamage, facingDir, transform, whatIsEnemy, whatIsGround);
    }

    /// <summary>
    /// Tìm vị trí tiếp đất chuẩn xác bên dưới mục tiêu (hoặc vị trí fallback) để cột lửa luôn bùng lên từ mặt đất.
    /// </summary>
    private Vector3 GetGroundedSpawnPosition(Vector3 targetPos)
    {
        LayerMask groundMask = whatIsGround.value != 0 ? whatIsGround : LayerMask.GetMask("Ground", "Default");

        // 1. Quét từ trên đỉnh mục tiêu xuống sâu 35m tìm collider mặt sàn
        float startY = targetPos.y + 2.0f;
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(targetPos.x, startY), Vector2.down, 35f, groundMask);
        if (hit.collider != null)
        {
            return new Vector3(targetPos.x, hit.point.y, transform.position.z);
        }

        // 2. Nếu quét từ targetPos không trúng (ví dụ mục tiêu chìm dưới địa hình dốc cao), quét từ độ cao của Rồng
        hit = Physics2D.Raycast(new Vector2(targetPos.x, transform.position.y + 2.0f), Vector2.down, 35f, groundMask);
        if (hit.collider != null)
        {
            return new Vector3(targetPos.x, hit.point.y, transform.position.z);
        }

        // 3. Fallback an toàn: nếu bên dưới không có collider sàn (hố sâu), ghim theo cao độ mặt đất của Rồng
        if (isGrounded)
        {
            return new Vector3(targetPos.x, transform.position.y, transform.position.z);
        }

        return targetPos;
    }

    /// <summary>
    /// Được gọi bởi Dragon_AnimationTriggers khi kết thúc đòn đánh tại frame thu đòn (CurrentStateTrigger Event).
    /// Chuyển tiếp tới State Machine để xử lý nối combo hoặc thoát về Idle.
    /// </summary>
    public void AnimationTrigger()
    {
        stateMachine.currentState.AnimationTrigger();
    }

    private void ExecuteClawDamage()
    {
        EnsureHitVfxReferences();

        Vector2 checkPos = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position + new Vector2(facingDir * 1.5f, 1.2f);
        float currentRange = attackRange;

        Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, currentRange);

        foreach (var colTarget in hits)
        {
            if (colTarget.transform.root == transform.root)
                continue;

            IDamageable damageable = colTarget.GetComponent<IDamageable>() ?? colTarget.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                try
                {
                    damageable.TakeDamage(attackDamage, attackDamage * 0.4f, ElementType.Fire, transform);
                }
                catch (Exception) { }

                // Sinh hiệu ứng chém trúng / tóe lửa va chạm tại điểm đánh (giữ nguyên scale gốc của Prefab)
                SpawnHitVfx(colTarget.bounds.center);

                if (AudioManager.instance != null && audioSource != null)
                    AudioManager.instance.PlaySFX("player_attackHit", audioSource, 15f);

                Rigidbody2D targetRb = colTarget.GetComponent<Rigidbody2D>() ?? colTarget.GetComponentInParent<Rigidbody2D>();
                if (targetRb != null)
                {
                    targetRb.linearVelocity = new Vector2(facingDir * knockbackForce.x, knockbackForce.y);
                }
            }
        }
    }

    private void SpawnHitVfx(Vector3 hitPos)
    {
        GameObject primaryPrefab = hitVfxPrefab != null ? hitVfxPrefab : clawSlashVfxPrefab;
        if (primaryPrefab != null)
        {
            GameObject vfx = null;
            if (GameManager.instance != null && GameManager.instance.vfxPool != null)
            {
                vfx = GameManager.instance.vfxPool.Get(primaryPrefab, hitPos, Quaternion.identity);
            }
            else
            {
                vfx = Instantiate(primaryPrefab, hitPos, Quaternion.identity);
            }

            if (vfx != null)
            {
                // Giữ nguyên đúng scale gốc từ Prefab (không phóng to)
                vfx.transform.localScale = primaryPrefab.transform.localScale;

                if (facingDir == -1)
                    vfx.transform.Rotate(0, 180, 0);

                Renderer[] renderers = vfx.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers)
                {
                    r.sortingLayerName = "Default";
                    r.sortingOrder = 28;
                }
            }
        }

        if (clawSlashVfxPrefab != null && clawSlashVfxPrefab != hitVfxPrefab)
        {
            GameObject slashVfx = Instantiate(clawSlashVfxPrefab, hitPos, Quaternion.identity);
            slashVfx.transform.localScale = clawSlashVfxPrefab.transform.localScale;

            if (facingDir == -1)
                slashVfx.transform.Rotate(0, 180, 0);

            Renderer[] renderers = slashVfx.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.sortingLayerName = "Default";
                r.sortingOrder = 27;
            }
            Destroy(slashVfx, 1.0f);
        }
    }

    private void EnsureHitVfxReferences()
    {
#if UNITY_EDITOR
        if (hitVfxPrefab == null)
        {
            hitVfxPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/VFX/VFX_OnHit.prefab");
        }
        if (clawSlashVfxPrefab == null)
        {
            clawSlashVfxPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Vfx_UseItem/Vfx_Shared_HitRed.prefab");
        }
#endif
    }

    #endregion

    #region IDamageable Implementation

    public bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        if (isDead)
            return false;

        float totalDamage = damage + elementalDamage;
        currentHealth -= totalDamage;

        Vector3 textPos = transform.position + Vector3.up * 2f;
        DamageTextManager.instance?.ShowDamageText(textPos, totalDamage, false, element);

        OnTakingDamage?.Invoke();

        if (AudioManager.instance != null && audioSource != null)
            AudioManager.instance.PlaySFX("dragon_hit", audioSource, 15f);

        if (!hasSuperArmor && anim != null)
            anim.SetTrigger("hit");

        if (currentHealth <= 0)
        {
            Die();
        }

        return true;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        stateMachine.ChangeState(deadState);

        if (col != null)
            col.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;

        OnDeath?.Invoke();

        if (GameManager.instance != null && GameManager.instance.IsDragonTransformed)
        {
            StartCoroutine(NotifyGameManagerAfterDeath(0.85f));
        }
        else
        {
            StartCoroutine(DestroyAfterDeath(3f));
        }
    }

    private IEnumerator NotifyGameManagerAfterDeath(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (GameManager.instance != null)
        {
            GameManager.instance.EndDragonTransformation();
        }
    }

    private IEnumerator DestroyAfterDeath(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
