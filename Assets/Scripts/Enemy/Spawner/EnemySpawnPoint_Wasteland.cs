using UnityEngine;

[AddComponentMenu("Gameplay/Spawner/Enemy Spawn Point - Wasteland")]
public class EnemySpawnPoint_Wasteland : EnemySpawnPoint
{
    [Header("Wasteland Enemies (Ô Gán Riêng Từng Quái)")]
    [Tooltip("Prefab Ninja Samurai Zipp")]
    [SerializeField] private GameObject samuraiZippPrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int samuraiZippWeight = 45;

    [Tooltip("Prefab Ninja Lưỡi Hái Sicklezeep")]
    [SerializeField] private GameObject sicklezeepPrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int sicklezeepWeight = 35;

    [Tooltip("Prefab Bộ Xương Skeleton")]
    [SerializeField] private GameObject skeletonPrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int skeletonWeight = 20;

    public GameObject SamuraiZippPrefab => samuraiZippPrefab;
    public GameObject SicklezeepPrefab => sicklezeepPrefab;
    public GameObject SkeletonPrefab => skeletonPrefab;

#if UNITY_EDITOR
    private void Reset()
    {
        samuraiZippPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_Samurai/Enemy_SamuraiZipp.prefab");
        sicklezeepPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_Samurai/Enemy_Sicklezeep.prefab");
        skeletonPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_Skeleton.prefab");
    }
#endif

    public override GameObject SelectEnemyPrefab()
    {
        if (CustomEnemyPrefab != null)
            return CustomEnemyPrefab;

        return PickWeightedRandom(
            (samuraiZippPrefab, samuraiZippWeight),
            (sicklezeepPrefab, sicklezeepWeight),
            (skeletonPrefab, skeletonWeight)
        );
    }

    protected override string GetSpawnPointLabel()
    {
        if (currentEnemy != null)
            return currentEnemy.name.Replace("(Clone)", "");

        if (CustomEnemyPrefab != null)
            return CustomEnemyPrefab.name;

        int activeTypes = 0;
        if (samuraiZippPrefab != null && samuraiZippWeight > 0) activeTypes++;
        if (sicklezeepPrefab != null && sicklezeepWeight > 0) activeTypes++;
        if (skeletonPrefab != null && skeletonWeight > 0) activeTypes++;

        return activeTypes > 0 ? $"🌪️ Wasteland ({activeTypes} quái)" : "[Chưa Gán Quái]";
    }
}
