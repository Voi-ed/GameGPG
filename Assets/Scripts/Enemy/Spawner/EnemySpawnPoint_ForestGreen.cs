using UnityEngine;

[AddComponentMenu("Gameplay/Spawner/Enemy Spawn Point - Forest Green")]
public class EnemySpawnPoint_ForestGreen : EnemySpawnPoint
{
    [Header("Forest Green Enemies (Ô Gán Riêng Từng Quái)")]
    [Tooltip("Prefab Slime Lớn (Quái biểu tượng duy nhất tại Rừng)")]
    [SerializeField] private GameObject largeSlimePrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int largeSlimeWeight = 60;

    [Tooltip("Prefab Cung Thủ Elf")]
    [SerializeField] private GameObject archerElfPrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int archerElfWeight = 40;

    public GameObject LargeSlimePrefab => largeSlimePrefab;
    public GameObject ArcherElfPrefab => archerElfPrefab;

#if UNITY_EDITOR
    private void Reset()
    {
        largeSlimePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_Slime_Large.prefab");
        archerElfPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_ArcherElf.prefab");
    }
#endif

    public override GameObject SelectEnemyPrefab()
    {
        if (CustomEnemyPrefab != null)
            return CustomEnemyPrefab;

        return PickWeightedRandom(
            (largeSlimePrefab, largeSlimeWeight),
            (archerElfPrefab, archerElfWeight)
        );
    }

    protected override string GetSpawnPointLabel()
    {
        if (currentEnemy != null)
            return currentEnemy.name.Replace("(Clone)", "");

        if (CustomEnemyPrefab != null)
            return CustomEnemyPrefab.name;

        int activeTypes = 0;
        if (largeSlimePrefab != null && largeSlimeWeight > 0) activeTypes++;
        if (archerElfPrefab != null && archerElfWeight > 0) activeTypes++;

        return activeTypes > 0 ? $"🌲 Forest ({activeTypes} quái)" : "[Chưa Gán Quái]";
    }
}
