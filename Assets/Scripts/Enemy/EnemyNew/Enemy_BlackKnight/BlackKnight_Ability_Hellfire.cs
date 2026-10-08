using System.Collections;
using UnityEngine;

public class BlackKnight_Ability_Hellfire : MonoBehaviour
{
    [Header("Hellfire Leap Slam Settings")]
    [SerializeField] private GameObject hellfirePillarPrefab;
    [SerializeField] private DamageScaleData hellfireDamageScale;
    [SerializeField] private float leapHellfireStartOffset = 1.2f;
    public float cooldown = 8.5f;

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

    public bool CanUse() => hellfirePillarPrefab != null && Time.time > lastUsedTime + cooldown;
    public void SetCooldown() => lastUsedTime = Time.time;

    public void TriggerLeapHellfire(LayerMask whatIsTarget)
    {
        if (blackKnight == null)
            blackKnight = GetComponentInParent<Enemy_BlackKnight>();

        if (blackKnight == null)
            return;

        SetCooldown();
        blackKnight.PerformAttackWithScale(blackKnight.heavyAttackScale);
        blackKnight.sfx?.PlayAttackHit();

        if (hellfirePillarPrefab == null)
            return;

        int dir = blackKnight.facingDir;
        float groundY = blackKnight.GroundPos != null ? blackKnight.GroundPos.position.y : transform.position.y;
        Vector3 spawnPos = new Vector3(blackKnight.transform.position.x + dir * leapHellfireStartOffset, groundY, 0f);

        LayerMask groundMask = blackKnight.whatIsGround.value != 0 ? blackKnight.whatIsGround : LayerMask.GetMask("Ground");
        RaycastHit2D hit = Physics2D.Raycast(spawnPos + Vector3.up * 1.5f, Vector2.down, 4f, groundMask);
        if (hit.collider != null)
            spawnPos = hit.point;

        GameObject pillar = Instantiate(hellfirePillarPrefab, spawnPos, Quaternion.identity);
        var pillarScript = pillar.GetComponent<Enemy_BlackKnight_HellfirePillar>();
        if (pillarScript == null)
            pillarScript = pillar.AddComponent<Enemy_BlackKnight_HellfirePillar>();

        var fallbackScale = (blackKnight.combat != null) ? blackKnight.combat.basicAttackScale : null;
        var scale = hellfireDamageScale ?? fallbackScale;

        pillarScript.SetupPillar(blackKnight, dir, scale, whatIsTarget, groundMask);
    }
}
