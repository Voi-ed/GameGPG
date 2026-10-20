using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Phantom Dash", fileName = "Item effect data - Phantom Dash")]
public class ItemEffect_PhantomDash : ItemEffect_DataSO
{
    [SerializeField] private float cooldown = 3f;
    private float lastTimeTriggered = -999f;

    [Header("VFX")]
    [SerializeField] private GameObject phantomTrailVfxPrefab;

    private GameObject GetVfxPrefab() => phantomTrailVfxPrefab;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        if (player.skillManager != null && player.skillManager.dash != null)
            player.skillManager.dash.OnDashEnd += HandleDashEnd;
    }

    public override void Unsubscribe()
    {
        if (player != null && player.skillManager != null && player.skillManager.dash != null)
            player.skillManager.dash.OnDashEnd -= HandleDashEnd;
        base.Unsubscribe();
    }

    private void HandleDashEnd()
    {
        if (player == null || player.skillManager == null || player.skillManager.timeEcho == null)
            return;

        if (Time.time < lastTimeTriggered + cooldown)
            return;

        lastTimeTriggered = Time.time;
        player.skillManager.timeEcho.CreateTimeEcho(player.transform.position);

        GameObject prefab = GetVfxPrefab();
        if (prefab != null)
        {
            GameObject vfx = InstantiateVfx(prefab, player.transform.position, Quaternion.identity);
            DestroyVfx(vfx, 2f);
        }
    }
}
