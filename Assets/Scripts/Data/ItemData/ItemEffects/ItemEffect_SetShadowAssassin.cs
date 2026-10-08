using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Set - Shadow Assassin", fileName = "Item effect data - Set Shadow Assassin")]
public class ItemEffect_SetShadowAssassin : ItemEffect_DataSO
{
    [Header("Execute Passive (< 35% HP)")]
    [SerializeField] private float executeThreshold = 0.35f;
    [SerializeField] private float bonusDamagePercent = 0.75f;
    [SerializeField] private GameObject executeHitVfxPrefab;

    [Header("Backstab (Chém Sau Lưng)")]
    [SerializeField] private float backstabBonusDamage = 0.5f;
    [SerializeField] private GameObject bloodVfxPrefab;

    [Header("Shadow Substitution (Thế Thân khi < 50% HP)")]
    [SerializeField] private float substitutionHpThreshold = 0.5f;
    [SerializeField] private float substitutionCooldown = 15.0f;
    [SerializeField] private float stealthDuration = 1.0f;
    [SerializeField] private float revengeCritMultiplier = 1.5f;
    [SerializeField] private float backstabOffset = 1.0f;
    [SerializeField] private LayerMask whatIsEnemy;

    [Header("Substitution VFX")]
    [SerializeField] private GameObject smokeVfxPrefab;
    [SerializeField] private GameObject slashVfxPrefab;

    private float lastSubstitutionTime = -999f;
    private bool isRevengeEmpowered = false;
    private Coroutine stealthRoutine;

    private void OnEnable()
    {
        lastSubstitutionTime = -999f;
        isRevengeEmpowered = false;
    }

    private GameObject GetExecuteHitPrefab() => executeHitVfxPrefab;
    private GameObject GetBloodVfxPrefab() => bloodVfxPrefab;
    private GameObject GetSmokeVfxPrefab() => smokeVfxPrefab;
    private GameObject GetSlashVfxPrefab() => slashVfxPrefab;

    private LayerMask GetEnemyLayerMask()
    {
        if (whatIsEnemy.value != 0) return whatIsEnemy;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) return 1 << enemyLayer;
        return 1 << 8;
    }

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        lastSubstitutionTime = -999f;
        isRevengeEmpowered = false;

        if (player == null) return;

        if (player.combat != null)
            player.combat.OnDoingDamage += HandleAttackHit;

        if (player.health != null)
            player.health.OnTakingDamage += HandlePlayerTakeDamage;

        Debug.Log("<color=cyan>[Set Sát Thủ] Kích hoạt thành công 4 món! (Chém Sau Lưng + Thuật Thế Thân < 50% HP + Cửa Sổ Phục Thù + Kết Liễu)</color>");
    }

    public override void Unsubscribe()
    {
        if (stealthRoutine != null && player != null)
        {
            try { player.StopCoroutine(stealthRoutine); } catch { }
            stealthRoutine = null;
        }

        if (player != null)
        {
            player.gameObject.layer = LayerMask.NameToLayer("Player");
            SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = Color.white;
        }

        isRevengeEmpowered = false;

        if (player != null && player.combat != null)
            player.combat.OnDoingDamage -= HandleAttackHit;

        if (player != null && player.health != null)
            player.health.OnTakingDamage -= HandlePlayerTakeDamage;

        base.Unsubscribe();
    }

    private void OnDisable()
    {
        lastSubstitutionTime = -999f;
        isRevengeEmpowered = false;
        stealthRoutine = null;
    }

    private void HandlePlayerTakeDamage()
    {
        if (player == null || player.health == null || player.health.isDead)
            return;

        if (Time.time < lastSubstitutionTime + substitutionCooldown)
            return;

        if (player.health.GetHealthPercent() > substitutionHpThreshold)
            return;

        TriggerSubstitution();
    }

    private void TriggerSubstitution()
    {
        lastSubstitutionTime = Time.time;

        // 1. Scan for nearest living enemy within 8m
        LayerMask mask = GetEnemyLayerMask();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.transform.position, 8.0f, mask);

        Collider2D targetEnemy = null;
        float minDist = float.MaxValue;

        foreach (var col in colliders)
        {
            if (col == null) continue;
            Entity_Health h = col.GetComponentInParent<Entity_Health>();
            if (h != null && h.isDead) continue;

            float dist = Vector2.Distance(player.transform.position, col.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                targetEnemy = col;
            }
        }

        Vector3 oldPos = player.transform.position;

        // 2. Spawn Smoke Cloud at old position
        GameObject smokePrefab = GetSmokeVfxPrefab();
        if (smokePrefab != null)
        {
            GameObject smoke = Instantiate(smokePrefab, oldPos, Quaternion.identity);
            ApplySortingLayer(smoke, "Player", 45);
            Destroy(smoke, 2f);
        }

        // 3. Spawn Decoy Silhouette at old position
        if (player.vfx != null)
        {
            player.vfx.CreateImageEcho(new Color(0.15f, 0.15f, 0.15f, 0.85f));
        }

        // 4. Teleport behind target enemy if found
        if (targetEnemy != null)
        {
            Entity enemyEntity = targetEnemy.GetComponentInParent<Entity>();
            int enemyFacing = enemyEntity != null ? enemyEntity.facingDir : player.facingDir;
            Vector3 enemyPos = targetEnemy.transform.position;

            Vector3 backstabPos = new Vector3(enemyPos.x - (enemyFacing * backstabOffset), enemyPos.y, 0f);

            // Wall check
            if (player.whatIsGround.value != 0)
            {
                RaycastHit2D wallCheck = Physics2D.Raycast(enemyPos, Vector2.right * -enemyFacing, backstabOffset, player.whatIsGround);
                if (wallCheck.collider != null)
                {
                    backstabPos.x = wallCheck.point.x + (enemyFacing * 0.25f);
                }
            }

            player.transform.position = backstabPos;
            player.SetVelocity(0, 0);
            FaceTarget(targetEnemy.transform);

            Debug.Log($"<color=#00FFCC>[Set Sát Thủ] THUẬT THẾ THÂN! Máu < 50% -> Thoát xác ra sau lưng {targetEnemy.name}!</color>");
        }
        else
        {
            Debug.Log("<color=#00FFCC>[Set Sát Thủ] THUẬT THẾ THÂN! Máu < 50% -> Nổ bom khói và tàng hình!</color>");
        }

        // 5. Activate Revenge Stealth Window (1.0s)
        if (stealthRoutine != null && player != null)
        {
            try { player.StopCoroutine(stealthRoutine); } catch { }
        }

        stealthRoutine = player.StartCoroutine(RevengeStealthRoutine());
    }

    private IEnumerator RevengeStealthRoutine()
    {
        isRevengeEmpowered = true;

        // Set Untargetable layer and semi-transparent
        int originalLayer = player.gameObject.layer;
        int untargetableLayer = LayerMask.NameToLayer("Untargetable");
        if (untargetableLayer != -1)
            player.gameObject.layer = untargetableLayer;

        SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
        Color originalColor = Color.white;
        if (sr != null)
        {
            originalColor = sr.color;
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.45f);
        }

        // 1.0s stealth duration
        float timer = 0f;
        while (timer < stealthDuration && player != null)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Restore layer and color
        if (player != null)
        {
            player.gameObject.layer = LayerMask.NameToLayer("Player");
            if (sr != null)
            {
                sr.color = originalColor;
            }
        }

        // Revenge empower stays ready for 2.0s additional or until an attack hits
        float remainingEmpowerTime = 2.0f;
        while (remainingEmpowerTime > 0f && isRevengeEmpowered && player != null)
        {
            remainingEmpowerTime -= Time.deltaTime;
            yield return null;
        }

        isRevengeEmpowered = false;
        stealthRoutine = null;
    }

    private void HandleAttackHit(AttackData attackData, Transform target)
    {
        if (target == null || player == null)
            return;

        Entity targetEntity = target.GetComponentInParent<Entity>();
        IDamageable damageable = target.GetComponentInParent<IDamageable>();
        Entity_Health targetHealth = target.GetComponentInParent<Entity_Health>();

        if (damageable == null)
            return;

        float baseDmg = attackData.phyiscalDamage;

        // 1. Check if attack is from behind (Backstab)
        bool isBehind = false;
        if (targetEntity != null)
        {
            int enemyFacing = targetEntity.facingDir;
            bool onBackSide = (enemyFacing > 0 && player.transform.position.x < target.position.x) ||
                              (enemyFacing < 0 && player.transform.position.x > target.position.x);

            if (onBackSide || player.facingDir == enemyFacing)
                isBehind = true;
        }

        // 2. BACKSTAB: Blood Slash + Bonus Damage
        if (isBehind && baseDmg > 0)
        {
            float backstabBonus = baseDmg * backstabBonusDamage;
            damageable.TakeDamage(backstabBonus, 0, ElementType.None, player.transform);

            // Spawn blood VFX
            GameObject bloodPrefab = GetBloodVfxPrefab();
            if (bloodPrefab != null)
            {
                GameObject blood = Instantiate(bloodPrefab, target.position, Quaternion.identity);
                ApplySortingLayer(blood, "Player", 65);
                Destroy(blood, 1.5f);
            }

            Debug.Log($"<color=#FF0044>[Set Sát Thủ] CHÉM SAU LƯNG TÓE MÁU! Gây thêm {backstabBonus:F1} Sát thương!</color>");
        }

        // 3. REVENGE STRIKE (+150% Crit Damage)
        if (isRevengeEmpowered && baseDmg > 0)
        {
            isRevengeEmpowered = false;
            float revengeBonus = baseDmg * revengeCritMultiplier;
            damageable.TakeDamage(revengeBonus, 0, ElementType.None, player.transform);

            // Native Dark Shadow Echo & Crit Hit feedback
            if (player.vfx != null)
            {
                player.vfx.CreateImageEcho(new Color(0.45f, 0.05f, 0.75f, 0.85f));
                player.vfx.CreateOnHitVFX(target, true, ElementType.None);
            }

            // Spawn optional slash VFX if assigned
            GameObject slashPrefab = GetSlashVfxPrefab();
            if (slashPrefab != null)
            {
                GameObject slash = Instantiate(slashPrefab, target.position, Quaternion.identity);
                ApplySortingLayer(slash, "Player", 70);
                Destroy(slash, 1.5f);
            }

            Debug.Log($"<color=#D154FF>[Set Sát Thủ] ĐÒN PHỤC THÙ! Phản kích từ bóng tối gây thêm {revengeBonus:F1} Sát thương!</color>");
        }

        // 4. EXECUTE PASSIVE (Target < 35% HP)
        if (targetHealth != null && targetHealth.GetHealthPercent() <= executeThreshold && !targetHealth.isDead && baseDmg > 0)
        {
            float executeBonus = baseDmg * bonusDamagePercent;
            damageable.TakeDamage(executeBonus, 0, ElementType.None, player.transform);

            // Native Crit Hit Impact
            if (player.vfx != null)
            {
                player.vfx.CreateOnHitVFX(target, true, ElementType.None);
            }

            // Spawn optional execute cross VFX if assigned
            GameObject hitCross = GetExecuteHitPrefab();
            if (hitCross != null)
            {
                GameObject cross = Instantiate(hitCross, target.position, Quaternion.identity);
                ApplySortingLayer(cross, "Player", 75);
                Destroy(cross, 2f);
            }

            Debug.Log($"<color=red>[Set Sát Thủ] KẾT LIỄU! Mục tiêu < 35% Máu -> Gây thêm {executeBonus:F1} Sát thương!</color>");
        }
    }

    private void FaceTarget(Transform target)
    {
        if (player == null || target == null) return;
        float dir = target.position.x - player.transform.position.x;
        if (dir > 0 && player.facingDir < 0)
            player.Flip();
        else if (dir < 0 && player.facingDir > 0)
            player.Flip();
    }

    private void ApplySortingLayer(GameObject obj, string layerName, int order)
    {
        if (obj == null) return;
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            r.sortingLayerName = layerName;
            r.sortingOrder = order;
        }

        ParticleSystem[] systems = obj.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in systems)
        {
            ps.Play();
        }
    }
}
