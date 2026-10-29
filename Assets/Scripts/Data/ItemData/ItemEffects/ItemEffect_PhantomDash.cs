using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Phantom Dash", fileName = "Item effect data - Phantom Dash")]
public class ItemEffect_PhantomDash : ItemEffect_DataSO
{
    [Header("Vanish / Stealth Settings")]
    [SerializeField] private float stealthDuration = 1.5f;
    [SerializeField] private float cooldown = 4f;
    [SerializeField] private float ambushBonusPercent = 0.35f;

    [Header("VFX")]
    [SerializeField] private GameObject phantomTrailVfxPrefab;

    private float lastTimeTriggered = -999f;
    private bool isInStealth = false;
    private Coroutine stealthRoutine;

    private GameObject GetVfxPrefab() => phantomTrailVfxPrefab;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        lastTimeTriggered = -999f;
        isInStealth = false;

        if (player == null) return;

        if (player.skillManager != null && player.skillManager.dash != null)
            player.skillManager.dash.OnDashEnd += HandleDashEnd;

        if (player.combat != null)
            player.combat.OnDoingDamage += HandleAttackHit;
    }

    public override void Unsubscribe()
    {
        EndStealth();

        if (player != null && player.skillManager != null && player.skillManager.dash != null)
            player.skillManager.dash.OnDashEnd -= HandleDashEnd;

        if (player != null && player.combat != null)
            player.combat.OnDoingDamage -= HandleAttackHit;

        base.Unsubscribe();
    }

    private void OnDisable()
    {
        EndStealth();
        lastTimeTriggered = -999f;
        isInStealth = false;
    }

    private void HandleDashEnd()
    {
        if (player == null || player.health == null || player.health.isDead)
            return;

        if (Time.time < lastTimeTriggered + cooldown)
            return;

        lastTimeTriggered = Time.time;

        // Hiệu ứng khói / bóng ma khi bắt đầu tàng hình
        GameObject prefab = GetVfxPrefab();
        if (prefab != null)
        {
            GameObject vfx = InstantiateVfx(prefab, player.transform.position, Quaternion.identity);
            DestroyVfx(vfx, 1.5f);
        }

        if (player.vfx != null)
        {
            player.vfx.CreateImageEcho(new Color(0.18f, 0.08f, 0.3f, 0.85f));
        }

        if (stealthRoutine != null && player != null)
        {
            try { player.StopCoroutine(stealthRoutine); } catch { }
        }

        stealthRoutine = player.StartCoroutine(StealthRoutine());
    }

    private IEnumerator StealthRoutine()
    {
        isInStealth = true;

        // Chuyển sang layer Untargetable và làm mờ hình ảnh nhân vật
        int untargetableLayer = LayerMask.NameToLayer("Untargetable");
        if (untargetableLayer != -1)
            player.gameObject.layer = untargetableLayer;

        SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
        Color originalColor = Color.white;
        if (sr != null)
        {
            originalColor = sr.color;
            sr.color = new Color(originalColor.r * 0.7f, originalColor.g * 0.7f, originalColor.b, 0.45f);
        }

        float timer = 0f;
        while (timer < stealthDuration && isInStealth && player != null)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        EndStealthInternal(sr, originalColor);
    }

    private void HandleAttackHit(AttackData attackData, Transform target)
    {
        if (isInStealth && target != null)
        {
            // Đòn đánh kích hoạt từ bóng tối (Ambush) gây thêm sát thương
            if (ambushBonusPercent > 0 && attackData != null)
            {
                IDamageable damageable = target.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    float bonus = attackData.phyiscalDamage * ambushBonusPercent;
                    if (bonus > 0)
                    {
                        damageable.TakeDamage(bonus, 0, ElementType.None, player.transform);
                        Debug.Log($"<color=#7B68EE>[Nhẫn Ảo Ảnh] ĐÒN ĐÁNH TỪ BÓNG TỐI! Gây thêm {bonus:F1} Sát thương!</color>");
                    }
                }
            }

            EndStealth();
        }
    }

    private void EndStealth()
    {
        if (stealthRoutine != null && player != null)
        {
            try { player.StopCoroutine(stealthRoutine); } catch { }
            stealthRoutine = null;
        }

        if (player != null)
        {
            SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
            EndStealthInternal(sr, Color.white);
        }
        else
        {
            isInStealth = false;
        }
    }

    private void EndStealthInternal(SpriteRenderer sr, Color targetColor)
    {
        isInStealth = false;
        stealthRoutine = null;

        if (player != null)
        {
            player.gameObject.layer = LayerMask.NameToLayer("Player");
            if (sr != null)
            {
                sr.color = targetColor;
            }
        }
    }
}
