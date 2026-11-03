using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý trạng thái Biến Hình Thành Rồng (Dragon Transformation) của Player khi uống thuốc.
/// - Hoán đổi visual: Tắt hiệp sĩ, bật rồng.
/// - Khóa toàn bộ skill hệ thống (chỉ cho phép Click chuột trái đánh thường cào xé).
/// - Tự động đếm ngược thời gian và hiển thị trên UI_BuffBar.
/// </summary>
public class Player_DragonForm : MonoBehaviour
{
    public static Player_DragonForm instance { get; private set; }

    [Header("Visual Avatar References")]
    [SerializeField] private GameObject knightVisual;
    [SerializeField] private GameObject dragonVisual;
    [SerializeField] private Animator dragonAnim;
    [SerializeField] private RuntimeAnimatorController dragonAnimatorController;
    [SerializeField] private Sprite dragonBuffIcon;

    [Header("Dragon Combat")]
    [SerializeField] private float clawDamage = 150f;
    [SerializeField] private float clawRange = 2.8f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private Vector2 knockbackForce = new Vector2(10f, 5f);
    [SerializeField] private LayerMask whatIsEnemy;

    [Header("Dragon Collider Tuning")]
    [SerializeField] private Vector2 dragonColliderSize = new Vector2(1.6f, 3.0f);
    [SerializeField] private Vector2 dragonColliderOffset = new Vector2(0f, 1.5f);

    private Player player;
    private CapsuleCollider2D playerCol;
    private Vector2 defaultColliderSize;
    private Vector2 defaultColliderOffset;

    public bool IsDragonForm { get; private set; } = false;
    private float lastAttackTime = float.NegativeInfinity;
    private float footstepTimer = 0f;
    private int comboIndex = 0;
    private int currentAttackType = 1;
    private bool isAttacking = false;
    [SerializeField] private float comboResetTime = 0.85f;

    public const string BUFF_SOURCE_ID = "DragonTransformation";

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (knightVisual == null)
        {
            Transform animT = transform.Find("Animator");
            if (animT != null) knightVisual = animT.gameObject;
        }

        if (dragonVisual == null)
        {
            Transform dragT = transform.Find("Dragon_Avatar");
            if (dragT != null)
            {
                dragonVisual = dragT.gameObject;
                dragonAnim = dragonVisual.GetComponentInChildren<Animator>();
            }
        }

        if (dragonAnim == null && dragonVisual != null)
        {
            dragonAnim = dragonVisual.GetComponentInChildren<Animator>();
        }

        if (whatIsEnemy.value == 0)
        {
            whatIsEnemy = LayerMask.GetMask("Enemy", "InteractiveObjects");
        }

        if (dragonAnimatorController == null)
        {
            dragonAnimatorController = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/Animations/AnimatorControllers/Characters/Dragon_Controller.controller");
        }

        if (hellfirePillarPrefab == null)
        {
            hellfirePillarPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefab/Vfx_UseItem/Vfx_Dragon_HellfirePillar.prefab");
        }

        if (fireballPrefab == null)
        {
            fireballPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefab/Vfx_UseItem/Vfx_Dragon_Fireball.prefab");
        }

        if (dragonBuffIcon == null)
        {
            UnityEngine.Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(
                "Assets/Audio/VS2/Shadow_Demon_Dragon_Asset_Pack/Shadow_Demon_Dragon_Asset_Pack/Animations/Idle_Right/Idle_Right.png");
            if (assets != null)
            {
                foreach (var asset in assets)
                {
                    if (asset is Sprite sp)
                    {
                        dragonBuffIcon = sp;
                        break;
                    }
                }
            }
        }
    }

    private void Reset()
    {
        OnValidate();
    }
#endif

    private void Awake()
    {
        instance = this;
        player = GetComponent<Player>();
        playerCol = GetComponent<CapsuleCollider2D>();

        if (playerCol != null)
        {
            defaultColliderSize = playerCol.size;
            defaultColliderOffset = playerCol.offset;
        }

        if (whatIsEnemy == 0)
        {
            whatIsEnemy = LayerMask.GetMask("Enemy", "InteractiveObjects");
        }

        if (knightVisual == null)
        {
            Transform animT = transform.Find("Animator");
            if (animT != null) knightVisual = animT.gameObject;
        }

        if (dragonVisual == null)
        {
            Transform dragT = transform.Find("Dragon_Avatar");
            if (dragT != null)
            {
                dragonVisual = dragT.gameObject;
                dragonAnim = dragonVisual.GetComponentInChildren<Animator>();
            }
        }

        if (dragonAnim == null && dragonVisual != null)
        {
            dragonAnim = dragonVisual.GetComponentInChildren<Animator>();
        }

#if UNITY_EDITOR
        if (dragonAnimatorController == null)
            dragonAnimatorController = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/AnimatorControllers/Characters/Dragon_Controller.controller");
        if (hellfirePillarPrefab == null)
            hellfirePillarPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Vfx_UseItem/Vfx_Dragon_HellfirePillar.prefab");
        if (fireballPrefab == null)
            fireballPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Vfx_UseItem/Vfx_Dragon_Fireball.prefab");
#endif
    }

    private void Start()
    {
        EnsureVisualReferences();

        // Đảm bảo ban đầu Avatar Rồng tắt, Hiệp Sĩ bật
        if (dragonVisual != null) dragonVisual.SetActive(false);
        if (knightVisual != null) knightVisual.SetActive(true);
    }

    private void OnEnable()
    {
        EnsureVisualReferences();
        if (dragonVisual != null) dragonVisual.SetActive(false);
        if (knightVisual != null) knightVisual.SetActive(true);
    }

    public void ForceResetVisuals()
    {
        EnsureVisualReferences();
        if (dragonVisual != null) dragonVisual.SetActive(false);
        if (knightVisual != null) knightVisual.SetActive(true);
        IsDragonForm = false;
    }

    private void EnsureVisualReferences()
    {
        if (knightVisual == null)
        {
            Transform animT = transform.Find("Animator");
            if (animT != null) knightVisual = animT.gameObject;
        }

        if (dragonVisual == null)
        {
            Transform dragT = transform.Find("Dragon_Avatar");
            if (dragT != null)
            {
                dragonVisual = dragT.gameObject;
                dragonAnim = dragonVisual.GetComponentInChildren<Animator>();
                dragonVisual.SetActive(false);
            }
            else
            {
                // Tự động tạo child Dragon_Avatar nếu chưa có sẵn trong Prefab
                GameObject dragonGO = new GameObject("Dragon_Avatar");
                dragonGO.transform.SetParent(transform, false);
                dragonGO.transform.localPosition = Vector3.zero;
                dragonGO.transform.localRotation = Quaternion.identity;
                dragonGO.transform.localScale = Vector3.one;
                dragonGO.SetActive(false);

                // Thêm SpriteRenderer đồng bộ với Knight
                SpriteRenderer knightSr = knightVisual != null ? knightVisual.GetComponent<SpriteRenderer>() : null;
                SpriteRenderer dragonSr = dragonGO.AddComponent<SpriteRenderer>();
                if (knightSr != null)
                {
                    dragonSr.sortingLayerID = knightSr.sortingLayerID;
                    dragonSr.sortingOrder = knightSr.sortingOrder;
                    dragonSr.material = knightSr.material;
                }

                // Thêm Animator
                dragonAnim = dragonGO.AddComponent<Animator>();
                if (dragonAnimatorController != null)
                {
                    dragonAnim.runtimeAnimatorController = dragonAnimatorController;
                }

                // Thêm AnimationTriggers
                dragonGO.AddComponent<Dragon_AnimationTriggers>();

                dragonVisual = dragonGO;
            }
        }

        if (dragonAnim == null && dragonVisual != null)
        {
            dragonAnim = dragonVisual.GetComponentInChildren<Animator>();
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    private void Update()
    {
        if (!IsDragonForm)
            return;

        UpdateDragonMovementAnim();
        HandleFootsteps();
        HandleDragonAttackInput();
    }

    private void UpdateDragonMovementAnim()
    {
        if (dragonAnim == null || player == null || player.rb == null) return;

        bool isMoving = Mathf.Abs(player.rb.linearVelocity.x) > 0.15f;
        bool isGrounded = player.groundDetected;
        float yVelocity = player.rb.linearVelocity.y;
        bool inCombatAction = isAttacking || IsCastingSkill || IsCastingFireball;

        dragonAnim.SetBool("move", !inCombatAction && isMoving && isGrounded);
        dragonAnim.SetBool("idle", !inCombatAction && !isMoving && isGrounded);
        dragonAnim.SetBool("jump", !inCombatAction && !isGrounded);
        dragonAnim.SetBool("isMoving", isMoving);
        dragonAnim.SetBool("isGrounded", isGrounded);
        dragonAnim.SetFloat("yVelocity", yVelocity);
    }

    private void HandleFootsteps()
    {
        if (player == null || !player.groundDetected) return;

        if (Mathf.Abs(player.rb.linearVelocity.x) > 0.5f)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                footstepTimer = 0.42f;
                if (AudioManager.instance != null)
                    AudioManager.instance.PlayGlobalSFX("dragon_footstep");
            }
        }
    }

    [Header("Dragon - Skill (Hellfire Roar)")]
    [SerializeField] private GameObject hellfirePillarPrefab;
    [SerializeField] private float skillDamage = 250f;
    [SerializeField] private float skillCooldown = 5.0f;
    [SerializeField] private float skillTargetRange = 11f;
    [SerializeField] private int maxPillarTargets = 3;
    [SerializeField] private float pillarChainDelay = 0.12f;
    private float lastSkillTime = float.NegativeInfinity;
    public bool IsCastingSkill { get; private set; } = false;

    [Header("Dragon - Fireball Skill (Left Shift)")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private float fireballDamage = 220f;
    [SerializeField] private float fireballSpeed = 15f;
    [SerializeField] private float fireballCooldown = 3.0f;
    private float lastFireballTime = float.NegativeInfinity;
    public bool IsCastingFireball { get; private set; } = false;

    private void HandleDragonAttackInput()
    {
        if (player == null || player.input == null) return;

        // Bấm Click Chuột Trái (Attack) để cào xé
        if (player.input.Player.Attack.WasPressedThisFrame())
        {
            TryDragonAttack();
        }

        // Bấm Chuột Phải (RangeAttack) để xả kỹ năng Long Nộ Địa Ngục
        if (player.input.Player.RangeAttack.WasPressedThisFrame())
        {
            TryDragonSkill();
        }

        // Bấm Left Shift (Dash) để hạ thân tụ khí bắn Hắc Hỏa Cầu
        if (player.input.Player.Dash.WasPressedThisFrame())
        {
            TryDragonFireball();
        }
    }

    /// <summary>
    /// Kích hoạt hoạt ảnh cào xé của Rồng khi người chơi bấm Chuột Trái.
    /// Hoạt ảnh cào bắt đầu vung tay.
    /// Âm thanh đánh và sát thương thực tế sẽ được kích hoạt bởi Animation Event AttackTrigger!
    /// </summary>
    public void TryDragonAttack()
    {
        if (IsCastingSkill || isAttacking || Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;
        currentAttackType = 1;
        comboIndex = 0;
        isAttacking = true;

        if (dragonAnim != null)
        {
            dragonAnim.SetBool("idle", false);
            dragonAnim.SetBool("move", false);
            dragonAnim.SetBool("jump", false);
            dragonAnim.SetBool("attack", true);
            dragonAnim.SetBool("isAttacking", true);
        }
    }

    /// <summary>
    /// Kích hoạt kỹ năng Long Nộ Địa Ngục (Hellfire Roar) khi người chơi bấm Chuột Phải.
    /// Rồng gồng mình ngửa đầu gầm thét, triệu hồi cột lửa ma quỷ bùng nổ từ mặt đất.
    /// Bắt buộc phải đứng trên mặt đất mới được kích hoạt.
    /// </summary>
    public bool TryDragonSkill()
    {
        if (player != null && !player.groundDetected)
            return false;

        if (Time.time - lastSkillTime < skillCooldown || isAttacking || IsCastingFireball)
            return false;

        lastSkillTime = Time.time;
        IsCastingSkill = true;
        isAttacking = false;
        IsCastingFireball = false;

        if (dragonAnim != null)
        {
            dragonAnim.SetBool("idle", false);
            dragonAnim.SetBool("move", false);
            dragonAnim.SetBool("jump", false);
            dragonAnim.SetBool("attack", false);
            dragonAnim.SetBool("fireball", false);
            dragonAnim.SetBool("isAttacking", false);
            dragonAnim.SetBool("skill", true);
            dragonAnim.SetBool("isCastingSkill", true);
        }

        return true;
    }

    /// <summary>
    /// Kích hoạt kỹ năng Hắc Hỏa Cầu khi người chơi bấm Left Shift (Dash).
    /// Rồng hạ rạp thân tụ hỏa khí ở miệng rồi bắn cầu lửa FX_Fire_03_2.
    /// </summary>
    public bool TryDragonFireball()
    {
        if (Time.time - lastFireballTime < fireballCooldown || isAttacking || IsCastingSkill || IsCastingFireball)
            return false;

        lastFireballTime = Time.time;
        IsCastingFireball = true;
        isAttacking = false;
        IsCastingSkill = false;

        if (dragonAnim != null)
        {
            dragonAnim.SetBool("idle", false);
            dragonAnim.SetBool("move", false);
            dragonAnim.SetBool("jump", false);
            dragonAnim.SetBool("attack", false);
            dragonAnim.SetBool("skill", false);
            dragonAnim.SetBool("isAttacking", false);
            dragonAnim.SetBool("fireball", true);
            dragonAnim.SetBool("isCastingSkill", true);
        }

        return true;
    }

    /// <summary>
    /// Được gọi bởi Dragon_AnimationTriggers tại đúng frame vuốt rồng cào trúng địch (AttackTrigger).
    /// </summary>
    public void ExecuteClawAttack()
    {
        // Âm thanh vung vuốt gầm thét đúng lúc AttackTrigger
        if (AudioManager.instance != null)
            AudioManager.instance.PlayGlobalSFX("dragon_attack");

        ExecuteClawDamage(1.0f, 1.0f);
    }

    /// <summary>
    /// Được gọi bởi Dragon_AnimationTriggers tại đúng frame rồng há to miệng gầm thét (AttackTrigger ở frame 17).
    /// </summary>
    public void ExecuteDragonSkill()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlayGlobalSFX("dragon_attack");

        SpawnHellfirePillar();
    }

    /// <summary>
    /// Được gọi bởi Dragon_AnimationTriggers tại đúng frame rồng khạc bắn Hắc Hỏa Cầu (AttackTrigger).
    /// </summary>
    public void ExecuteDragonFireball()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlayGlobalSFX("dragon_attack");

        SpawnFireballProjectile();

        // Lực giật lùi (Recoil)
        if (player != null && player.rb != null)
        {
            player.rb.linearVelocity = new Vector2(-player.facingDir * 3.8f, player.rb.linearVelocity.y);
        }
    }

    private void SpawnFireballProjectile()
    {
        if (fireballPrefab == null)
            return;

        Vector3 spawnPos = transform.position + new Vector3(player.facingDir * 2.8f, -0.2f, 0f);
        GameObject fbGO = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
        Dragon_FireballProjectile proj = fbGO.GetComponent<Dragon_FireballProjectile>();
        if (proj == null)
            proj = fbGO.AddComponent<Dragon_FireballProjectile>();

        LayerMask groundMask = LayerMask.GetMask("Ground", "Default");
        proj.Setup(fireballDamage, fireballSpeed, player.facingDir, transform, whatIsEnemy, groundMask);
    }

    private void SpawnHellfirePillar()
    {
        if (hellfirePillarPrefab == null)
            return;

        int dir = player != null ? player.facingDir : 1;
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
            validTargets.Sort((a, b) =>
            {
                bool aInFront = (a.transform.position.x - transform.position.x) * dir >= -0.8f;
                bool bInFront = (b.transform.position.x - transform.position.x) * dir >= -0.8f;
                if (aInFront != bInFront)
                    return aInFront ? -1 : 1;

                float distA = Vector2.SqrMagnitude(a.transform.position - transform.position);
                float distB = Vector2.SqrMagnitude(b.transform.position - transform.position);
                return distA.CompareTo(distB);
            });

            int count = Mathf.Min(maxPillarTargets, validTargets.Count);
            StartCoroutine(SpawnPillarChainRoutine(validTargets.GetRange(0, count), dir));
        }
        else
        {
            Vector3 fallbackPos = transform.position + new Vector3(dir * 3.5f, 0f, 0f);
            InstantiateHellfirePillarAt(fallbackPos, dir);
        }
    }

    private IEnumerator SpawnPillarChainRoutine(List<Collider2D> targets, int dir)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            Collider2D targetCol = targets[i];
            if (targetCol != null)
            {
                Vector3 footPos = new Vector3(targetCol.bounds.center.x, targetCol.bounds.min.y, transform.position.z);
                InstantiateHellfirePillarAt(footPos, dir);
            }

            if (i < targets.Count - 1 && pillarChainDelay > 0f)
            {
                yield return new WaitForSeconds(pillarChainDelay);
            }
        }
    }

    private void InstantiateHellfirePillarAt(Vector3 spawnPos, int dir)
    {
        if (hellfirePillarPrefab == null)
            return;

        Vector3 groundPos = GetGroundedSpawnPosition(spawnPos);
        GameObject pillarGO = Instantiate(hellfirePillarPrefab, groundPos, Quaternion.identity);
        Dragon_HellfirePillar pillar = pillarGO.GetComponent<Dragon_HellfirePillar>();
        if (pillar == null)
            pillar = pillarGO.AddComponent<Dragon_HellfirePillar>();

        LayerMask groundMask = (player != null && player.whatIsGround.value != 0) ? player.whatIsGround : LayerMask.GetMask("Ground", "Default");
        pillar.Setup(skillDamage, dir, transform, whatIsEnemy, groundMask);
    }

    /// <summary>
    /// Tìm vị trí tiếp đất chuẩn xác bên dưới mục tiêu (hoặc vị trí fallback) để cột lửa luôn bùng lên từ mặt đất.
    /// </summary>
    private Vector3 GetGroundedSpawnPosition(Vector3 targetPos)
    {
        LayerMask groundMask = (player != null && player.whatIsGround.value != 0) ? player.whatIsGround : LayerMask.GetMask("Ground", "Default");

        // 1. Quét từ trên đỉnh mục tiêu xuống sâu 35m tìm collider mặt sàn
        float startY = targetPos.y + 2.0f;
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(targetPos.x, startY), Vector2.down, 35f, groundMask);
        if (hit.collider != null)
        {
            return new Vector3(targetPos.x, hit.point.y, transform.position.z);
        }

        // 2. Nếu quét từ targetPos không trúng (ví dụ mục tiêu chìm dưới địa hình dốc cao), quét từ độ cao của Player
        hit = Physics2D.Raycast(new Vector2(targetPos.x, transform.position.y + 2.0f), Vector2.down, 35f, groundMask);
        if (hit.collider != null)
        {
            return new Vector3(targetPos.x, hit.point.y, transform.position.z);
        }

        // 3. Fallback an toàn: nếu bên dưới không có collider sàn (hố sâu), ghim theo cao độ mặt đất của Player
        if (player != null && player.groundDetected)
        {
            return new Vector3(targetPos.x, transform.position.y, transform.position.z);
        }

        return targetPos;
    }

    /// <summary>
    /// Được gọi bởi Dragon_AnimationTriggers khi kết thúc hoạt ảnh cào hoặc gầm nộ (CurrentStateTrigger).
    /// </summary>
    public void OnAttackAnimationCompleted()
    {
        isAttacking = false;
        IsCastingSkill = false;
        IsCastingFireball = false;
        if (dragonAnim != null)
        {
            dragonAnim.SetBool("attack", false);
            dragonAnim.SetBool("isAttacking", false);
            dragonAnim.SetBool("skill", false);
            dragonAnim.SetBool("fireball", false);
            dragonAnim.SetBool("isCastingSkill", false);
        }
    }

    private void ExecuteClawDamage(float damageMult = 1f, float knockbackMult = 1f)
    {
        Vector2 checkPos = (Vector2)transform.position + new Vector2(player.facingDir * 1.6f, 1.4f);
        float effectiveRange = clawRange;
        ElementType element = ElementType.None;

        Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, effectiveRange);

        foreach (var hitCol in hits)
        {
            if (hitCol.transform.root == transform.root)
                continue;

            IDamageable target = hitCol.GetComponent<IDamageable>() ?? hitCol.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                // Sát thương cào xé / phun lửa / đập đất
                float actualDamage = clawDamage * damageMult;
                target.TakeDamage(actualDamage, actualDamage * 0.4f, element, transform);

                // Âm thanh chém trúng đanh thép
                if (AudioManager.instance != null)
                    AudioManager.instance.PlayGlobalSFX("player_attackHit");

                // Hất văng quái
                Rigidbody2D targetRb = hitCol.GetComponent<Rigidbody2D>() ?? hitCol.GetComponentInParent<Rigidbody2D>();
                if (targetRb != null)
                {
                    targetRb.linearVelocity = new Vector2(player.facingDir * knockbackForce.x * knockbackMult, knockbackForce.y * knockbackMult);
                }
            }
        }
    }

    #region Transformation Lifecycle

    /// <summary>
    /// Chuyển tiếp tới GameManager để điều phối thực thể Rồng độc lập
    /// </summary>
    public void TransformToDragon(float duration, Sprite customBuffIcon = null)
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.StartDragonTransformation(duration);
        }
    }

    /// <summary>
    /// Trở lại hình dạng Hiệp Sĩ thông qua GameManager
    /// </summary>
    public void RevertToKnight()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.EndDragonTransformation();
        }
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Vector2 checkPos = (Vector2)transform.position + new Vector2((player != null ? player.facingDir : 1) * 1.6f, 1.4f);
        Gizmos.DrawWireSphere(checkPos, clawRange);
    }
}
