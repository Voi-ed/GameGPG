using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Set - Berserker's Wrath", fileName = "Item effect data - Set Berserkers Wrath")]
public class ItemEffect_SetBerserkersWrath : ItemEffect_DataSO
{
    [SerializeField] private float healPercent = 0.35f;
    [SerializeField] private float invulnerabilityDuration = 3.5f;
    [SerializeField] private float damageBonus = 50f;
    [SerializeField] private float buffDuration = 6f;
    [SerializeField] private float cooldown = 90f;

    private float lastTriggerTime = -999f;
    private bool isRageActive = false;
    private Player_Health playerHealth;

    [Header("VFX")]
    [SerializeField] private GameObject wrathExplosionVfxPrefab;
    [SerializeField] private GameObject wrathAuraPrefab;
    private GameObject activeWrathAura;

    [Header("Crimson Berserk Visuals (Huyết Ma Tro Than)")]
    [SerializeField] private bool useDarkVoidVisuals = true;
    [SerializeField] private Color voidColor = new Color(0.65f, 0.02f, 0.05f, 1f);
    [SerializeField] private Color echoColor = new Color(0.12f, 0.02f, 0.02f, 0.8f);
    [SerializeField] private float echoInterval = 0.07f;
    private Coroutine voidVisualCo;
    private SpriteRenderer playerSr;

    private GameObject GetVfxPrefab() => wrathExplosionVfxPrefab;
    private GameObject GetAuraPrefab() => wrathAuraPrefab;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        playerHealth = player.GetComponent<Player_Health>();
        if (playerHealth != null)
        {
            playerHealth.canPreventDeath = CheckPreventDeath;
        }
    }

    public override void Unsubscribe()
    {
        if (playerHealth != null && playerHealth.canPreventDeath == CheckPreventDeath)
        {
            playerHealth.canPreventDeath = null;
        }
        if (activeWrathAura != null)
        {
            Destroy(activeWrathAura);
            activeWrathAura = null;
        }
        RemoveRageBuff();
        base.Unsubscribe();
    }

    private bool CheckPreventDeath()
    {
        if (Time.time < lastTriggerTime + cooldown)
            return false;

        lastTriggerTime = Time.time;
        if (player != null)
        {
            player.StartCoroutine(UndyingRageRoutine());
        }
        return true;
    }

    private IEnumerator UndyingRageRoutine()
    {
        if (player == null || player.health == null)
            yield break;

        GameObject prefab = GetVfxPrefab();
        if (prefab != null)
        {
            GameObject vfx = InstantiateVfx(prefab, player.transform.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        if (useDarkVoidVisuals)
        {
            if (activeWrathAura != null)
            {
                Destroy(activeWrathAura);
                activeWrathAura = null;
            }

            if (voidVisualCo == null && player != null)
            {
                voidVisualCo = player.StartCoroutine(VoidVisualRoutine());
            }
        }
        else
        {
            if (activeWrathAura == null)
            {
                GameObject auraPrefab = GetAuraPrefab();
                if (auraPrefab != null)
                {
                    Transform attachPoint = player.GroundPos != null ? player.GroundPos : player.transform;
                    activeWrathAura = Instantiate(auraPrefab, attachPoint);
                    ApplyVfxTheme(activeWrathAura);
                    if (attachPoint == player.GroundPos)
                        activeWrathAura.transform.localPosition = Vector3.zero;
                    else
                        activeWrathAura.transform.localPosition = auraPrefab.transform.localPosition;

                    activeWrathAura.transform.localRotation = auraPrefab.transform.localRotation;
                }
            }
        }

        // Restore health to healPercent
        player.health.SetHealthToPercent(healPercent);
        player.health.SetCanTakeDamage(false);

        // Add damage bonus
        if (!isRageActive && player.stats != null)
        {
            player.stats.offense.damage.AddModifier(damageBonus, "BerserkersWrath");
            isRageActive = true;
        }

        yield return new WaitForSeconds(invulnerabilityDuration);

        if (player != null && player.health != null)
        {
            player.health.SetCanTakeDamage(true);
        }

        float remainingBuffTime = Mathf.Max(0, buffDuration - invulnerabilityDuration);
        if (remainingBuffTime > 0)
            yield return new WaitForSeconds(remainingBuffTime);

        RemoveRageBuff();
    }

    private IEnumerator VoidVisualRoutine()
    {
        if (playerSr == null && player != null)
            playerSr = player.GetComponentInChildren<SpriteRenderer>();

        float echoTimer = 0f;

        while (isRageActive && player != null && player.health != null && !player.health.isDead)
        {
            float pulse = Mathf.PingPong(Time.time * 3f, 0.3f);
            if (playerSr != null)
            {
                playerSr.color = Color.Lerp(voidColor, voidColor * 1.35f, pulse);
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

    private void RemoveRageBuff()
    {
        StopVoidVisuals();

        if (activeWrathAura != null)
        {
            Destroy(activeWrathAura);
            activeWrathAura = null;
        }

        if (isRageActive && player != null && player.stats != null)
        {
            player.stats.offense.damage.RemoveModifier("BerserkersWrath");
            isRageActive = false;
        }
    }
}

