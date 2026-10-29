using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Thunder Strike on Attack", fileName = "Item effect data - Thunder Strike")]
public class ItemEffect_ThunderStrikeOnAttack : ItemEffect_DataSO
{
    [SerializeField] private float procChance = 0.3f;
    [SerializeField] private float lightningDamage = 35f;
    [SerializeField] private float splashRadius = 2.5f;
    [SerializeField] private float cooldown = 1.2f;
    [SerializeField] private LayerMask whatIsEnemy;
    private float lastTimeTriggered = -999f;
    [Header("VFX")]
    [SerializeField] private GameObject lightningVfxPrefab;

    private GameObject GetVfxPrefab() => lightningVfxPrefab;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        if (player.combat != null)
            player.combat.OnDoingDamage += HandleAttackHit;
    }

    public override void Unsubscribe()
    {
        if (player != null && player.combat != null)
            player.combat.OnDoingDamage -= HandleAttackHit;
        base.Unsubscribe();
    }

    private void HandleAttackHit(AttackData attackData, Transform target)
    {
        if (Time.time < lastTimeTriggered + cooldown)
            return;

        bool shouldProc = attackData.isCrit || Random.value <= procChance;
        if (!shouldProc)
            return;

        lastTimeTriggered = Time.time;
        Player effectivePlayer = (player != null) ? player : (Player.instance != null ? Player.instance : FindFirstObjectByType<Player>());
        Vector3 targetPos = target != null ? target.position : (effectivePlayer != null ? effectivePlayer.transform.position : Vector3.zero);

        GameObject prefab = GetVfxPrefab();
        if (prefab != null)
        {
            GameObject vfx = InstantiateVfx(prefab, targetPos, Quaternion.identity);
            DestroyVfx(vfx, 2f);
        }

        Transform dealerTransform = effectivePlayer != null ? effectivePlayer.transform : target;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(targetPos, splashRadius, whatIsEnemy);
        foreach (var col in hitEnemies)
        {
            if (col == null)
                continue;

            // Bỏ qua nếu là chính người chơi
            if (effectivePlayer != null && col.gameObject == effectivePlayer.gameObject)
                continue;

            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(0, lightningDamage, ElementType.Lightning, dealerTransform);
            }
        }
    }
}
