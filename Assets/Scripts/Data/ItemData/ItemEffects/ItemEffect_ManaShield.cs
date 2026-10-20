using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Mana Shield", fileName = "Item effect data - Mana Shield")]
public class ItemEffect_ManaShield : ItemEffect_DataSO
{
    [SerializeField] private float maxShieldAmount = 60f;
    [SerializeField] private float rechargeDelay = 8f;
    private float currentShield = 0f;
    private float lastRechargeTime = -999f;
    [Header("VFX")]
    [SerializeField] private GameObject shieldVfxPrefab;

    private GameObject GetShieldPrefab()
    {
        return shieldVfxPrefab;
    }

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        currentShield = maxShieldAmount;
        if (player.health != null)
            player.health.OnTakingDamage += HandleDamageTaken;
    }

    public override void Unsubscribe()
    {
        if (player != null && player.health != null)
            player.health.OnTakingDamage -= HandleDamageTaken;
        base.Unsubscribe();
    }

    private void HandleDamageTaken()
    {
        if (player == null || player.health == null)
            return;

        if (currentShield <= 0)
        {
            if (Time.time >= lastRechargeTime + rechargeDelay)
            {
                currentShield = maxShieldAmount;
                lastRechargeTime = Time.time;
            }
            return;
        }

        float damageTaken = player.health.lastDamageTaken;
        if (damageTaken <= 0)
            return;

        float absorbed = Mathf.Min(currentShield, damageTaken);
        currentShield -= absorbed;
        player.health.IncreaseHealth(absorbed);

        // Arcane Blue Shield burst feedback (Tàn ảnh khiên ma thuật xanh lam)
        if (player.vfx != null)
        {
            player.vfx.CreateImageEcho(new Color(0.2f, 0.65f, 1f, 0.85f));
        }

        GameObject prefab = GetShieldPrefab();
        if (prefab != null)
        {
            GameObject vfx = InstantiateVfx(prefab, player.transform.position, Quaternion.identity);
            DestroyVfx(vfx, 2f);
        }

        lastRechargeTime = Time.time;
    }
}
