using UnityEngine;

[AddComponentMenu("Gameplay/Spawner/Enemy Spawn Point - Swamp")]
public class EnemySpawnPoint_Swamp : EnemySpawnPoint
{
    [Header("Swamp Enemies (Ô Gán Riêng Từng Quái)")]
    [Tooltip("Prefab Tu Sĩ Rừng Druid")]
    [SerializeField] private GameObject druidPrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int druidWeight = 60;

    [Tooltip("Prefab Pháp Sư Đầm Lầy Mage")]
    [SerializeField] private GameObject magePrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int mageWeight = 40;

    public GameObject DruidPrefab => druidPrefab;
    public GameObject MagePrefab => magePrefab;

#if UNITY_EDITOR
    private void Reset()
    {
        druidPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_DuskBorne/Druid/Druid.prefab");
        magePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_Mage.prefab");
    }
#endif

    public override GameObject SelectEnemyPrefab()
    {
        if (CustomEnemyPrefab != null)
            return CustomEnemyPrefab;

        return PickWeightedRandom(
            (druidPrefab, druidWeight),
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
        if (druidPrefab != null && druidWeight > 0) activeTypes++;
        if (magePrefab != null && mageWeight > 0) activeTypes++;

        return activeTypes > 0 ? $"🌿 Swamp ({activeTypes} quái)" : "[Chưa Gán Quái]";
    }
}
