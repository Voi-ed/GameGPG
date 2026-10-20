using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Berserk on Low Health", fileName = "Item effect data - Berserk on Low Health")]
public class ItemEffect_BerserkOnLowHealth : ItemEffect_DataSO
{
    [SerializeField] private float tier1Threshold = 0.5f; // < 50%
    [SerializeField] private float tier1DamageBonus = 25f;
    [SerializeField] private float tier2Threshold = 0.25f; // < 25%
    [SerializeField] private float tier2DamageBonus = 50f;
    [SerializeField] private float tier2AttackSpeedBonus = 0.3f;

    private const string BERSERK_SOURCE = "ItemEffect_Berserk";
    private int currentTier = 0;

    [Header("VFX")]
    [SerializeField] private GameObject berserkAuraPrefab;
    [SerializeField] private GameObject fireAuraPrefab;
    [SerializeField] private GameObject fireHitPrefab;
    private GameObject activeFireAuraInstance;

    [Header("Crimson Berserk Visuals (Huyết Ma Tro Than)")]
    [SerializeField] private bool useDarkVoidVisuals = true;
    [SerializeField] private Color voidColorTier1 = new Color(0.9f, 0.2f, 0.12f, 1f);
    [SerializeField] private Color voidColorTier2 = new Color(0.65f, 0.02f, 0.05f, 1f);
    [SerializeField] private Color echoColor = new Color(0.12f, 0.02f, 0.02f, 0.8f);
    [SerializeField] private float echoInterval = 0.07f;
    private Coroutine voidVisualCo;
    private SpriteRenderer playerSr;

    private GameObject GetAuraPrefab() => berserkAuraPrefab != null ? berserkAuraPrefab : fireAuraPrefab;

    private GameObject GetHitPrefab() => fireHitPrefab;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        if (player.health != null)
            player.health.OnHealthUpdate += EvaluateBerserk;
        if (player.combat != null)
            player.combat.OnDoingDamage += HandleAttackHit;
        Player.OnPlayerDeath += HandlePlayerDeath;
        EvaluateBerserk();
    }

    public override void Unsubscribe()
    {
        ClearBerserkBuffs();
        Player.OnPlayerDeath -= HandlePlayerDeath;
        if (player != null)
        {
            if (player.health != null)
                player.health.OnHealthUpdate -= EvaluateBerserk;
            if (player.combat != null)
                player.combat.OnDoingDamage -= HandleAttackHit;
        }
        base.Unsubscribe();
    }

    private void HandlePlayerDeath()
    {
        ClearBerserkBuffs();
    }

    private void HandleAttackHit(AttackData attackData, Transform target)
    {
        if (currentTier <= 0 || target == null || player == null)
            return;

        GameObject hitPrefab = GetHitPrefab();
        if (hitPrefab != null)
        {
            GameObject vfx = InstantiateVfx(hitPrefab, target.position, Quaternion.identity);
            DestroyVfx(vfx, 2f);
        }
    }

    private void EvaluateBerserk()
    {
        if (player == null || player.health == null || player.stats == null)
            return;

        if (player.health.isDead)
        {
            ClearBerserkBuffs();
            return;
        }

        float hpPercent = player.health.GetHealthPercent();
        int newTier = 0;

        if (hpPercent <= tier2Threshold)
            newTier = 2;
        else if (hpPercent <= tier1Threshold)
            newTier = 1;

        if (newTier == currentTier)
            return;

        currentTier = newTier;

        // Refresh stat modifiers cleanly
        player.stats.offense.damage.RemoveModifier(BERSERK_SOURCE);
        player.stats.offense.attackSpeed.RemoveModifier(BERSERK_SOURCE);

        if (currentTier == 0)
        {
            StopVoidVisuals();
            if (activeFireAuraInstance != null)
            {
                Destroy(activeFireAuraInstance);
                activeFireAuraInstance = null;
            }
            if (player.anim != null)
                player.anim.SetFloat("attackSpeedMultiplier", player.stats.offense.attackSpeed.GetValue());
            return;
        }

        if (currentTier == 1)
        {
            player.stats.offense.damage.AddModifier(tier1DamageBonus, BERSERK_SOURCE);
        }
        else if (currentTier == 2)
        {
            player.stats.offense.damage.AddModifier(tier2DamageBonus, BERSERK_SOURCE);
            player.stats.offense.attackSpeed.AddModifier(tier2AttackSpeedBonus, BERSERK_SOURCE);
            if (player.anim != null)
                player.anim.SetFloat("attackSpeedMultiplier", player.stats.offense.attackSpeed.GetValue());
        }

        if (useDarkVoidVisuals)
        {
            if (activeFireAuraInstance != null)
            {
                Destroy(activeFireAuraInstance);
                activeFireAuraInstance = null;
            }

            if (voidVisualCo == null && player != null)
            {
                voidVisualCo = player.StartCoroutine(VoidVisualRoutine());
            }
        }
        else
        {
            if (activeFireAuraInstance == null)
            {
                GameObject auraPrefab = GetAuraPrefab();
                if (auraPrefab != null)
                {
                    Transform attachPoint = player.GroundPos != null ? player.GroundPos : player.transform;
                    activeFireAuraInstance = Instantiate(auraPrefab, attachPoint);
                    if (attachPoint == player.GroundPos)
                        activeFireAuraInstance.transform.localPosition = Vector3.zero;
                    else
                        activeFireAuraInstance.transform.localPosition = auraPrefab.transform.localPosition;

                    activeFireAuraInstance.transform.localRotation = auraPrefab.transform.localRotation;
                }
            }
        }
    }

    private IEnumerator VoidVisualRoutine()
    {
        if (playerSr == null && player != null)
            playerSr = player.GetComponentInChildren<SpriteRenderer>();

        float echoTimer = 0f;

        while (currentTier > 0 && player != null && player.health != null && !player.health.isDead)
        {
            Color baseColor = currentTier == 2 ? voidColorTier2 : voidColorTier1;
            float pulse = Mathf.PingPong(Time.time * 2.5f, 0.25f);
            if (playerSr != null)
            {
                playerSr.color = Color.Lerp(baseColor, baseColor * 1.25f, pulse);
            }

            echoTimer += Time.deltaTime;
            if (echoTimer >= echoInterval)
            {
                echoTimer = 0f;
                bool isMoving = player.rb != null && player.rb.linearVelocity.sqrMagnitude > 0.05f;
                bool isAttacking = player.stateMachine != null &&
                    (player.stateMachine.currentState == player.basicAttackState ||
                     player.stateMachine.currentState == player.jumpAttackState);

                if ((isMoving || isAttacking) && player.vfx != null)
                {
                    player.vfx.CreateImageEcho(echoColor);
                }
            }

            yield return null;
        }

        if (playerSr != null)
            playerSr.color = Color.white;
        voidVisualCo = null;
    }

    private void StopVoidVisuals()
    {
        if (voidVisualCo != null && player != null)
        {
            player.StopCoroutine(voidVisualCo);
            voidVisualCo = null;
        }
        if (playerSr != null)
        {
            playerSr.color = Color.white;
        }
    }

    private void ClearBerserkBuffs()
    {
        StopVoidVisuals();

        if (activeFireAuraInstance != null)
        {
            Destroy(activeFireAuraInstance);
            activeFireAuraInstance = null;
        }

        if (player != null && player.stats != null)
        {
            player.stats.offense.damage.RemoveModifier(BERSERK_SOURCE);
            player.stats.offense.attackSpeed.RemoveModifier(BERSERK_SOURCE);
            if (player.anim != null)
                player.anim.SetFloat("attackSpeedMultiplier", player.stats.offense.attackSpeed.GetValue());
        }
        currentTier = 0;
    }
}

