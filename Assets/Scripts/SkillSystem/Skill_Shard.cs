using System;
using System.Collections;
using UnityEngine;

public class Skill_Shard : Skill_Base
{
    private SkillObject_Shard currentShard;
    private Entity_Health playerHealth;

    [SerializeField] private GameObject shardPrefab;
    [SerializeField] private float detonateTime = 2;

    [Header("Moving Shard Upgrade")]
    [SerializeField] private float shardSpeed = 7;

    [Header("Multicast Shard Upgrade")]
    [SerializeField] private int maxCharges = 3;
    [SerializeField] private int currentCharges;
    [SerializeField] private bool isRecharging;
    private float rechargeTimer;

    [Header("Teleport Shard Upgrade")]
    [SerializeField] private float shardExistDuration = 10;

    [Header("Health Rewind Shard Upgrade")]
    [SerializeField] private float savedHealthPercent;

    protected override void Awake()
    {
        base.Awake();
        currentCharges = maxCharges;
        playerHealth = GetComponentInParent<Entity_Health>();
    }

    private void Start()
    {
        if (Unlocked(SkillUpgradeType.Shard_Multicast))
        {
            currentCharges = maxCharges;
            UpdateChargesUI();
        }
    }

    private UI_SkillSlot GetSkillSlot() => player?.ui?.inGameUI?.GetSkillSlot(skillType);

    public void UpdateChargesUI()
    {
        var slot = GetSkillSlot();
        if (slot != null && Unlocked(SkillUpgradeType.Shard_Multicast))
        {
            slot.UpdateCharges(currentCharges, maxCharges);
        }
    }

    public override void SetSkillUpgrade(SkillDataSO skillData)
    {
        base.SetSkillUpgrade(skillData);

        if (Unlocked(SkillUpgradeType.Shard_Multicast))
        {
            currentCharges = maxCharges;
            if (isRecharging)
            {
                StopAllCoroutines();
                isRecharging = false;
            }
            UpdateChargesUI();
        }
        else
        {
            var slot = GetSkillSlot();
            if (slot != null)
                slot.ClearCharges();
        }
    }

    public override bool CanUseSkill()
    {
        if (upgradeType == SkillUpgradeType.None)
            return false;

        if (Unlocked(SkillUpgradeType.Shard_Multicast))
            return currentCharges > 0;

        return base.CanUseSkill();
    }

    public override void ReduceCooldownBy(float cooldownReduction)
    {
        base.ReduceCooldownBy(cooldownReduction);

        if (Unlocked(SkillUpgradeType.Shard_Multicast) && isRecharging)
        {
            rechargeTimer += cooldownReduction;
        }
    }

    public void CreateShard()
    {
        float detonateTime = GetDetonateTime();

        GameObject shard = Instantiate(shardPrefab, transform.position, Quaternion.identity);
        currentShard = shard.GetComponent<SkillObject_Shard>();
        currentShard.SetupShard(this);

        if (AudioManager.instance != null)
            AudioManager.instance.PlayGlobalSFX("skill_shard_throw");

        if (Unlocked(SkillUpgradeType.Shard_Teleport) || Unlocked(SkillUpgradeType.Shard_TeleportHpRewind))
            currentShard.OnExplode += ForceCooldown;
    }

    public void CreateRawShard(Transform target = null,bool shardsCanMove = false)
    {
        bool canMove = shardsCanMove != false ? shardsCanMove :
            Unlocked(SkillUpgradeType.Shard_MoveToEnemy) || Unlocked(SkillUpgradeType.Shard_Multicast);


        GameObject shard = Instantiate(shardPrefab, transform.position, Quaternion.identity);
        shard.GetComponent<SkillObject_Shard>().SetupShard(this, detonateTime, canMove, shardSpeed, target);

        if (AudioManager.instance != null)
            AudioManager.instance.PlayGlobalSFX("skill_shard_throw");
    }

    public void CreateDomainShard(Transform target)
    {

    }

    public override void TryUseSkill()
    {
        if (CanUseSkill() == false)
            return;

        if (Unlocked(SkillUpgradeType.Shard))
            HandleShardRegular();

        if (Unlocked(SkillUpgradeType.Shard_MoveToEnemy))
            HandleShardMoving();

        if (Unlocked(SkillUpgradeType.Shard_Multicast))
            HandleShardMulticast();

        if (Unlocked(SkillUpgradeType.Shard_Teleport))
            HandleShardTeleport();

        if(Unlocked(SkillUpgradeType.Shard_TeleportHpRewind))
            HandleShardHealthRewind();
    }

    private void HandleShardHealthRewind()
    {
        if (currentShard == null)
        {
            CreateShard();
            savedHealthPercent = playerHealth.GetHealthPercent();
        }
        else
        {
            SwapPlayerAndShard();
            playerHealth.SetHealthToPercent(savedHealthPercent);
            SetSkillOnCooldown();
        }
    }

    private void HandleShardTeleport()
    {
        if (currentShard == null)
        {
            CreateShard();
        }
        else
        {
            SwapPlayerAndShard();
            SetSkillOnCooldown();
        }
    }

    private void SwapPlayerAndShard()
    {
        Vector3 shardPosition = currentShard.transform.position;
        Vector3 playerPosition = player.transform.position;

        currentShard.transform.position = playerPosition;
        currentShard.Explode();

        player.TeleportPlayer(shardPosition);
    }

    private void HandleShardMulticast()
    {
        if (currentCharges <= 0)
            return;

        CreateShard();
        currentShard.MoveTowardsClosestTarget(shardSpeed);
        currentCharges--;

        UpdateChargesUI();

        if (isRecharging == false)
            StartCoroutine(ShardRechargeCo());
    }

    private IEnumerator ShardRechargeCo()
    {
        isRecharging = true;
        float cd = Mathf.Max(0.1f, cooldown);

        while (currentCharges < maxCharges)
        {
            rechargeTimer = 0f;
            while (rechargeTimer < cd)
            {
                rechargeTimer += Time.deltaTime;
                var slot = GetSkillSlot();
                if (slot != null)
                    slot.SetChargeCooldownFill(1f - (rechargeTimer / cd));

                yield return null;
            }

            currentCharges++;
            UpdateChargesUI();
        }

        var s = GetSkillSlot();
        if (s != null)
            s.SetChargeCooldownFill(0f);

        isRecharging = false;
    }

    private void HandleShardMoving()
    {
        CreateShard();
        currentShard.MoveTowardsClosestTarget(shardSpeed);

        SetSkillOnCooldown();
    }

    private void HandleShardRegular()
    {
        CreateShard();
        SetSkillOnCooldown();
    }

   

    public float GetDetonateTime()
    {
        if (Unlocked(SkillUpgradeType.Shard_Teleport) || Unlocked(SkillUpgradeType.Shard_TeleportHpRewind))
            return shardExistDuration;

        return detonateTime;
    }

    private void ForceCooldown()
    {
        if (OnCooldown() == false)
        {
            SetSkillOnCooldown();
            currentShard.OnExplode -= ForceCooldown;
        }
    }

}
