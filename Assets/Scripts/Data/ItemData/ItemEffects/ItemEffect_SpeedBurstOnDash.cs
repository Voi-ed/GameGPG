using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Speed Burst on Dash", fileName = "Item effect data - Speed Burst")]
public class ItemEffect_SpeedBurstOnDash : ItemEffect_DataSO
{
    [SerializeField] private float speedBonusPercent = 0.35f;
    [SerializeField] private float duration = 2.5f;
    private Coroutine buffRoutine;
    private bool isBuffActive = false;
    private float appliedBonus = 0f;

    [Header("VFX")]
    [SerializeField] private GameObject windTrailsPrefab;
    private GameObject activeWindVfx;

    private GameObject GetVfxPrefab() => windTrailsPrefab;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        if (player.skillManager != null && player.skillManager.dash != null)
            player.skillManager.dash.OnDashEnd += HandleDashEnd;
    }

    public override void Unsubscribe()
    {
        RemoveBuff();
        if (player != null && player.skillManager != null && player.skillManager.dash != null)
            player.skillManager.dash.OnDashEnd -= HandleDashEnd;
        base.Unsubscribe();
    }

    private void HandleDashEnd()
    {
        if (player == null || player.stats == null)
            return;

        if (buffRoutine != null)
            player.StopCoroutine(buffRoutine);

        buffRoutine = player.StartCoroutine(SpeedBuffRoutine());
    }

    private IEnumerator SpeedBuffRoutine()
    {
        if (!isBuffActive && player != null)
        {
            appliedBonus = player.moveSpeed * speedBonusPercent;
            player.moveSpeed += appliedBonus;
            isBuffActive = true;

            if (activeWindVfx == null)
            {
                GameObject prefab = GetVfxPrefab();
                if (prefab != null)
                {
                    activeWindVfx = Instantiate(prefab, player.transform);
                    activeWindVfx.transform.localPosition = Vector3.zero;
                }
            }
        }

        yield return new WaitForSeconds(duration);

        RemoveBuff();
    }

    private void RemoveBuff()
    {
        if (activeWindVfx != null)
        {
            Destroy(activeWindVfx);
            activeWindVfx = null;
        }

        if (isBuffActive && player != null)
        {
            player.moveSpeed -= appliedBonus;
            isBuffActive = false;
        }
        buffRoutine = null;
    }
}
