using System.Collections;
using UnityEngine;

public class BlackKnight_Ability_FireOrbs : MonoBehaviour
{
    [Header("Fire Orbs Ultimate Settings")]
    [SerializeField] private GameObject fireOrbPrefab;
    [SerializeField] private DamageScaleData fireOrbDamageScale;
    public float cooldown = 12f;

    private Enemy_BlackKnight blackKnight;
    private float lastUsedTime = float.NegativeInfinity;

    public void Initialize(Enemy_BlackKnight blackKnight)
    {
        this.blackKnight = blackKnight;
    }

    private void Awake()
    {
        if (blackKnight == null)
            blackKnight = GetComponentInParent<Enemy_BlackKnight>();
    }

    public bool CanUse() => fireOrbPrefab != null && Time.time > lastUsedTime + cooldown;
    public void SetCooldown() => lastUsedTime = Time.time;

    public void CastOrbsBarrage(LayerMask whatIsTarget)
    {
        if (blackKnight == null)
            blackKnight = GetComponentInParent<Enemy_BlackKnight>();

        if (fireOrbPrefab == null || blackKnight == null)
            return;

        SetCooldown();

        // 5 Crown Arc offsets above Boss
        Vector3[] offsets = new Vector3[]
        {
            new Vector3(-1.3f, 2.1f, 0f),
            new Vector3(-0.65f, 2.6f, 0f),
            new Vector3(0f, 2.85f, 0f),
            new Vector3(0.65f, 2.6f, 0f),
            new Vector3(1.3f, 2.1f, 0f)
        };

        Transform playerTarget = blackKnight.player != null ? blackKnight.player : blackKnight.GetPlayerReference();
        var fallbackScale = (blackKnight.combat != null) ? blackKnight.combat.basicAttackScale : null;
        var scale = fireOrbDamageScale ?? fallbackScale;

        for (int i = 0; i < offsets.Length; i++)
        {
            Vector3 spawnPos = blackKnight.transform.position + offsets[i];
            GameObject orbObj = Instantiate(fireOrbPrefab, spawnPos, Quaternion.identity);
            var orbScript = orbObj.GetComponent<Enemy_BlackKnight_FireOrb>();
            if (orbScript == null)
                orbScript = orbObj.AddComponent<Enemy_BlackKnight_FireOrb>();

            float delay = 0.7f + i * 0.18f;
            orbScript.SetupOrb(blackKnight, playerTarget, offsets[i], delay, scale, whatIsTarget, blackKnight.whatIsGround);
        }
    }
}
