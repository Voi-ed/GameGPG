using System.Collections.Generic;
using UnityEngine;

public class Skill_DomainExpansion : Skill_Base
{
    [SerializeField] private GameObject domainPrefab;

    [Header("Slowing Down Upgrade")]
    [SerializeField] private float slowDownPercent = .8f;
    [SerializeField] private float slowDownDomainDuration = 5;

    [Header("Shard Cast Upgarade")]
    [SerializeField] private int shardsToCast = 10;
    [SerializeField] private float shardCastDomainSlow = 1;
    [SerializeField] private float shardCastDomainDuration = 8;
    private float spellCastTimer;
    private float spellsPerSecond;

    [Header("Time echo cast Upgrade")]
    [SerializeField] private int echoToCast = 8;
    [SerializeField] private float echoCastDomainSlow = 1;
    [SerializeField] private float echoCastDomainDuration = 6;
    [SerializeField] private float healthToRestoreWithEcho = .05f;

    [Header("Chrono Stasis Upgrade")]
    [SerializeField] private GameObject chronoStasisDomainPrefab;
    [SerializeField] private float stasisDuration = 5f;
    [SerializeField] private float stasisBurstMultiplier = 1.25f;

    [Header("Wan Jian Gui Zong Upgrade")]
    [SerializeField] private GameObject flyingSwordPrefab;
    [SerializeField] private int swordsToCast = 45;
    [SerializeField] private float swordBarrageDomainSlow = 0.85f;
    [SerializeField] private float swordBarrageDomainDuration = 5f;
    private const int TOTAL_SKY_SLOTS = 20;
    private SkillObject_FlyingSword[] skySwords = new SkillObject_FlyingSword[TOTAL_SKY_SLOTS];
    private float domainElapsedTime = 0f;
    private float bombardTimer = 0f;
    private bool grandFinaleTriggered = false;
    private int nextBombardSlot = 0;

    [Header("Domain details")]
    public float maxDomainSize = 10;
    public float expandSpeed = 3;

    private List<Enemy> trappedTargets = new List<Enemy>();
    private Transform currentTarget;

    public void CreateDomain()
    {
        spellsPerSecond = GetSpellsToCast() / GetDomainDuration();
        domainElapsedTime = 0f;
        grandFinaleTriggered = false;
        bombardTimer = 0.55f;
        nextBombardSlot = 0;

        GameObject prefabToUse = GetDomainPrefab();
        GameObject domain = Instantiate(prefabToUse, transform.position, Quaternion.identity);
        domain.GetComponent<SkillObject_DomainExpansion>().SetupDomain(this);

        if (upgradeType == SkillUpgradeType.Domain_SwordBarrage)
        {
            InitialSummonSkySwords();
        }
    }

    private GameObject GetDomainPrefab()
    {
        if (Unlocked(SkillUpgradeType.Domain_ChronoStasis))
        {
            if (chronoStasisDomainPrefab != null)
                return chronoStasisDomainPrefab;
        }

        return domainPrefab;
    }

    private Vector3[] skySlotOffsets = new Vector3[TOTAL_SKY_SLOTS];

    private void InitialSummonSkySwords()
    {
        GenerateSkySlotOffsets();
        skySwords = new SkillObject_FlyingSword[TOTAL_SKY_SLOTS];
        for (int i = 0; i < TOTAL_SKY_SLOTS; i++)
        {
            float delay = i * 0.018f; // Bắn xòe rợp trời trong ~0.36s
            skySwords[i] = SpawnSwordForSlot(i, delay);
        }
    }

    private void GenerateSkySlotOffsets()
    {
        for (int i = 0; i < TOTAL_SKY_SLOTS; i++)
        {
            // Phân bổ 20 slots dạng đám mây bồng bềnh hữu cơ, phá vỡ 2 hàng song song
            if (i < 11) // Tầng ngoài cao
            {
                float t = (float)i / 10f;
                float angleDeg = Mathf.Lerp(-75f, 75f, t) + Random.Range(-4f, 4f);
                float rad = angleDeg * Mathf.Deg2Rad;

                float spreadX = Mathf.Sin(rad) * 5.3f + Random.Range(-0.35f, 0.35f);
                float heightY = 4.6f + Mathf.Cos(rad) * 1.1f + Random.Range(-0.45f, 0.45f);
                skySlotOffsets[i] = new Vector3(spreadX, heightY, 0f);
            }
            else // Tầng trong thấp hơn
            {
                int j = i - 11;
                float t = (float)j / 8f;
                float angleDeg = Mathf.Lerp(-60f, 60f, t) + Random.Range(-5f, 5f);
                float rad = angleDeg * Mathf.Deg2Rad;

                float spreadX = Mathf.Sin(rad) * 3.8f + Random.Range(-0.38f, 0.38f);
                float heightY = 3.6f + Mathf.Cos(rad) * 0.9f + Random.Range(-0.45f, 0.45f);
                skySlotOffsets[i] = new Vector3(spreadX, heightY, 0f);
            }
        }
    }

    private SkillObject_FlyingSword SpawnSwordForSlot(int slotIndex, float delay)
    {
        if (flyingSwordPrefab == null)
            return null;

        Vector3 slotOffset = skySlotOffsets[slotIndex];
        GameObject swordObj = Instantiate(flyingSwordPrefab, player.transform.position, Quaternion.identity);
        SkillObject_FlyingSword flyingSword = swordObj.GetComponent<SkillObject_FlyingSword>();
        if (flyingSword != null)
        {
            flyingSword.SetupSkySlotSword(this, player.transform, slotIndex, slotOffset, delay);
        }
        return flyingSword;
    }

    public void DoSpellCasting()
    {
        if (upgradeType == SkillUpgradeType.Domain_SwordBarrage)
        {
            HandleSwordBarrageFlow();
            return;
        }

        spellCastTimer -= Time.deltaTime;

        if (currentTarget == null)
            currentTarget = FindTargetInDomain();

        if (currentTarget != null && spellCastTimer < 0)
        {
            CastSpell(currentTarget);
            spellCastTimer = 1 / spellsPerSecond;
            currentTarget = null;
        }
    }

    private void HandleSwordBarrageFlow()
    {
        domainElapsedTime += Time.deltaTime;

        // Pha 2: Oanh tạc & Bù mới liên tục (từ giây 0.55s đến 3.8s)
        if (domainElapsedTime >= 0.55f && domainElapsedTime < 3.8f)
        {
            bombardTimer -= Time.deltaTime;
            if (bombardTimer <= 0f)
            {
                bombardTimer = 0.12f; // Cứ mỗi 0.12s bắn 1 kiếm và bù 1 kiếm

                GetOptimalTarget(out Transform target, out Vector3 targetPos);

                int attempts = 0;
                while (attempts < TOTAL_SKY_SLOTS)
                {
                    int slot = nextBombardSlot;
                    nextBombardSlot = (nextBombardSlot + 3) % TOTAL_SKY_SLOTS;
                    attempts++;

                    if (skySwords[slot] != null && skySwords[slot].CurrentPhase == SkillObject_FlyingSword.SwordPhase.HoveringInSky)
                    {
                        // 1. Bắn thanh kiếm ở slot đó xuống đất
                        skySwords[slot].LaunchSingleSword(target, targetPos);

                        // 2. NGAY LẬP TỨC spawn một thanh kiếm mới tại ngực Player bay lên bù vào đúng slot đó!
                        skySwords[slot] = SpawnSwordForSlot(slot, 0f);
                        break;
                    }
                }
            }
        }
        // Pha 3: ĐẠI KẾT THÚC QUY TÔNG (từ giây 3.8s trở đi)
        else if (domainElapsedTime >= 3.8f && !grandFinaleTriggered)
        {
            grandFinaleTriggered = true;

            GetOptimalTarget(out Transform target, out Vector3 targetPos);

            // TẤT CẢ các kiếm còn trên trời ĐỒNG LOẠT LAO XUỐNG DỘI BOM!
            for (int i = 0; i < TOTAL_SKY_SLOTS; i++)
            {
                if (skySwords[i] != null)
                {
                    skySwords[i].TriggerGrandFinale(target, targetPos);
                }
            }
        }
    }

    /// <summary>
    /// Tìm mục tiêu tối ưu: Nếu có thanh kiếm ném chính đang cắm trên bãi chiến trường,
    /// nó sẽ đóng vai trò CỘT THU LÔI KIẾM KHÍ hút toàn bộ Vạn Kiếm dội bom vào đúng vị trí đó!
    /// </summary>
    private void GetOptimalTarget(out Transform targetTransform, out Vector3 targetPos)
    {
        Skill_SwordThrow swordThrow = skillManager != null ? skillManager.swordThrow : null;
        if (swordThrow != null && swordThrow.currentSword != null && swordThrow.currentSword.gameObject.activeInHierarchy)
        {
            targetTransform = swordThrow.currentSword.transform;
            targetPos = targetTransform.position;
            return;
        }

        targetTransform = FindTargetInDomain();
        if (targetTransform != null)
        {
            targetPos = targetTransform.position;
            return;
        }

        targetTransform = null;
        targetPos = player.transform.position + new Vector3(player.facingDir * 6f, -1.5f, 0);
    }

    private void CastSpell(Transform target)
    {
        if (upgradeType == SkillUpgradeType.Domain_EchoSpam)
        { 
            Vector3 offset = Random.value < .5f ? new Vector2(1,0) : new Vector2(-1,0);
            skillManager.timeEcho.CreateTimeEcho(target.position + offset);
        }

        if (upgradeType == SkillUpgradeType.Domain_ShardSpam)
        {
            skillManager.shard.CreateRawShard(target, true);
        }
    }

    private Transform FindTargetInDomain()
    {
        trappedTargets.RemoveAll(target => target == null || target.health.isDead);

        if (trappedTargets.Count == 0)
            return null;

        int randomIndex = Random.Range(0, trappedTargets.Count);
        return trappedTargets[randomIndex].transform;
    }

    public float GetDomainDuration()
    {
        if(upgradeType == SkillUpgradeType.Domain_SlowingDown)
            return slowDownDomainDuration;
        else if(upgradeType == SkillUpgradeType.Domain_ShardSpam)
            return shardCastDomainDuration;
        else if(upgradeType == SkillUpgradeType.Domain_EchoSpam)
            return echoCastDomainDuration;
        else if(upgradeType == SkillUpgradeType.Domain_ChronoStasis)
            return stasisDuration;
        else if(upgradeType == SkillUpgradeType.Domain_SwordBarrage)
            return swordBarrageDomainDuration;

        return 0;
    }

    public float GetStasisBurstMultiplier() => stasisBurstMultiplier;


    public float GetSlowPercentage()
    {
        if (upgradeType == SkillUpgradeType.Domain_SlowingDown)
            return slowDownPercent;
        else if (upgradeType == SkillUpgradeType.Domain_ShardSpam)
            return shardCastDomainSlow;
        else if (upgradeType == SkillUpgradeType.Domain_EchoSpam)
            return echoCastDomainSlow;
        else if (upgradeType == SkillUpgradeType.Domain_SwordBarrage)
            return swordBarrageDomainSlow;

        return 0;
    }

    private int GetSpellsToCast()
    {
        if (upgradeType == SkillUpgradeType.Domain_ShardSpam)
            return shardsToCast;
        else if (upgradeType == SkillUpgradeType.Domain_EchoSpam)
            return echoToCast;
        else if (upgradeType == SkillUpgradeType.Domain_SwordBarrage)
            return swordsToCast;

        return 0;
    }


    public bool InstantDomain()
    {
        return upgradeType != SkillUpgradeType.Domain_EchoSpam
            && upgradeType != SkillUpgradeType.Domain_ShardSpam
            && upgradeType != SkillUpgradeType.Domain_SwordBarrage;
    }

   
    public void AddTarget(Enemy targetToAdd)
    {
        trappedTargets.Add(targetToAdd);
    }

    public void ClearTargets()
    {
        foreach (var enemy in trappedTargets)
            enemy.StopSlowDown();

        trappedTargets = new List<Enemy>();
    }
}
