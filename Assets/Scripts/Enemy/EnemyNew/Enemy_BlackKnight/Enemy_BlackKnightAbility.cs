using UnityEngine;

public class Enemy_BlackKnightAbility : MonoBehaviour
{
    [Header("Combat Detection")]
    [SerializeField] private LayerMask whatIsTarget;
    public LayerMask WhatIsTarget => whatIsTarget;

    public BlackKnight_Ability_Hellfire hellfire { get; private set; }
    public BlackKnight_Ability_FireOrbs fireOrbs { get; private set; }
    public Enemy_BlackKnight blackKnight { get; private set; }

    private void Awake()
    {
        blackKnight = GetComponentInParent<Enemy_BlackKnight>();
        hellfire = GetComponent<BlackKnight_Ability_Hellfire>();
        fireOrbs = GetComponent<BlackKnight_Ability_FireOrbs>();

        if (hellfire != null)
            hellfire.Initialize(blackKnight);
        if (fireOrbs != null)
            fireOrbs.Initialize(blackKnight);
    }

    // Hellfire methods
    public bool CanHellfire() => hellfire != null && hellfire.CanUse();
    public void SetHellfireOnCooldown() => hellfire?.SetCooldown();
    public void TriggerLeapSlamExplosion() => hellfire?.TriggerLeapHellfire(whatIsTarget);
    public bool isCastingHellfire => false;
    public float hellfireChannelDuration => 1.8f;
    public void StartHellfireSpell() => TriggerLeapSlamExplosion();

    // Ultimate Fire Orbs methods
    public bool CanCastUltimate() => fireOrbs != null && fireOrbs.CanUse();
    public void SetUltimateOnCooldown() => fireOrbs?.SetCooldown();
    public void CastInfernalOrbsUltimate() => fireOrbs?.CastOrbsBarrage(whatIsTarget);

    // Compatibility aliases
    public bool CanSwordWave() => CanCastUltimate();
    public void SetSwordWaveOnCooldown() => SetUltimateOnCooldown();
    public void CastSwordWave() => CastInfernalOrbsUltimate();

    public void TriggerEnrageNova()
    {
        if (blackKnight == null)
            blackKnight = GetComponentInParent<Enemy_BlackKnight>();

        if (blackKnight == null)
            return;

        blackKnight.ActivatePhase2();
        blackKnight.PerformSurroundingAttack(blackKnight.weaponArtDamageScale, 4.5f);
        blackKnight.sfx?.PlayAttackHit();
    }
}
