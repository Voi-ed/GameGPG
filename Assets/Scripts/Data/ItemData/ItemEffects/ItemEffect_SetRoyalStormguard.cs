using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Set - Royal Stormguard", fileName = "Item effect data - Set Royal Stormguard")]
public class ItemEffect_SetRoyalStormguard : ItemEffect_DataSO
{
    [SerializeField] private float strikeInterval = 2.5f;
    [SerializeField] private float strikeDamage = 50f;
    [SerializeField] private float detectionRadius = 7f;
    [SerializeField] private float splashRadius = 2.5f;
    [SerializeField] private LayerMask whatIsEnemy;
    private Coroutine stormRoutine;
    [Header("VFX")]
    [SerializeField] private GameObject lightningVfxPrefab;

    private GameObject GetVfxPrefab() => lightningVfxPrefab;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        if (stormRoutine != null)
            player.StopCoroutine(stormRoutine);
        stormRoutine = player.StartCoroutine(StormguardLoop());
    }

    public override void Unsubscribe()
    {
        if (stormRoutine != null && player != null)
            player.StopCoroutine(stormRoutine);
        stormRoutine = null;
        base.Unsubscribe();
    }

    private IEnumerator StormguardLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(strikeInterval);

            if (player == null)
                yield break;

            Collider2D[] enemies = Physics2D.OverlapCircleAll(player.transform.position, detectionRadius, whatIsEnemy);
            if (enemies.Length > 0)
            {
                Collider2D target = enemies[Random.Range(0, enemies.Length)];
                if (target != null)
                {
                    Vector3 strikePos = target.transform.position;

                    GameObject prefab = GetVfxPrefab();
                    if (prefab != null)
                    {
                        GameObject vfx = InstantiateVfx(prefab, strikePos, Quaternion.identity);
                        Destroy(vfx, 2f);
                    }

                    Collider2D[] splashed = Physics2D.OverlapCircleAll(strikePos, splashRadius, whatIsEnemy);
                    foreach (var col in splashed)
                    {
                        IDamageable damageable = col.GetComponent<IDamageable>();
                        if (damageable != null)
                        {
                            damageable.TakeDamage(0, strikeDamage, ElementType.Lightning, player.transform);
                        }
                    }
                }
            }
        }
    }
}
