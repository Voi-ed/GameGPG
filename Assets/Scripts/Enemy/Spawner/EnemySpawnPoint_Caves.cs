using UnityEngine;

[AddComponentMenu("Gameplay/Spawner/Enemy Spawn Point - Caves")]
public class EnemySpawnPoint_Caves : EnemySpawnPoint
{
    [Header("Caves Enemies (Ô Gán Riêng Từng Quái)")]
    [Tooltip("Prefab Bộ Xương Skeleton")]
    [SerializeField] private GameObject skeletonPrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int skeletonWeight = 60;

    [Tooltip("Prefab Pháp Sư Hắc Ám Mage")]
    [SerializeField] private GameObject magePrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int mageWeight = 40;

    public GameObject SkeletonPrefab => skeletonPrefab;
    public GameObject MagePrefab => magePrefab;

#if UNITY_EDITOR
    private void Reset()
    {
        skeletonPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_Skeleton.prefab");
        magePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_Mage.prefab");
    }
#endif

    public override GameObject SelectEnemyPrefab()
    {
        if (CustomEnemyPrefab != null)
            return CustomEnemyPrefab;

        // Ưu tiên quái theo nhiệm vụ đang làm (Skeleton hoặc Dark Mage)
        GameObject questTarget = GetQuestTargetEnemyPrefab(skeletonPrefab, magePrefab);
        if (questTarget != null)
            return questTarget;

        return PickWeightedRandom(
            (skeletonPrefab, skeletonWeight),
            (magePrefab, mageWeight)
        );
    }

    protected override string GetSpawnPointLabel()
    {
        if (currentEnemy != null)
            return currentEnemy.name.Replace("(Clone)", "");

        if (CustomEnemyPrefab != null)
            return CustomEnemyPrefab.name;

        int activeTypes = 0;
        if (skeletonPrefab != null && skeletonWeight > 0) activeTypes++;
        if (magePrefab != null && mageWeight > 0) activeTypes++;

        return activeTypes > 0 ? $"🪨 Caves ({activeTypes} quái)" : "[Chưa Gán Quái]";
    }
}
