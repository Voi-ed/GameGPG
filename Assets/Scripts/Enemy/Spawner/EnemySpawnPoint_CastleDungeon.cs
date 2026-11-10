using UnityEngine;

[AddComponentMenu("Gameplay/Spawner/Enemy Spawn Point - Castle Dungeon")]
public class EnemySpawnPoint_CastleDungeon : EnemySpawnPoint
{
    [Header("Castle Dungeon Enemies (Ô Gán Riêng Từng Quái)")]
    [Tooltip("Prefab Hiệp Sĩ Bộ Binh")]
    [SerializeField] private GameObject knightFootmanPrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int knightFootmanWeight = 50;

    [Tooltip("Prefab Hiệp Sĩ Trưởng Cref")]
    [SerializeField] private GameObject knightCrefPrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int knightCrefWeight = 25;

    [Tooltip("Prefab Pháp Sư Mage")]
    [SerializeField] private GameObject magePrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int mageWeight = 25;

    public GameObject KnightFootmanPrefab => knightFootmanPrefab;
    public GameObject KnightCrefPrefab => knightCrefPrefab;
    public GameObject MagePrefab => magePrefab;

#if UNITY_EDITOR
    private void Reset()
    {
        knightFootmanPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_Knight/Enemy_KnightFootman.prefab");
        knightCrefPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_Knight/Enemy_KnightCref.prefab");
        magePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_Mage.prefab");
    }
#endif

    public override GameObject SelectEnemyPrefab()
    {
        if (CustomEnemyPrefab != null)
            return CustomEnemyPrefab;

        // Ưu tiên quái theo nhiệm vụ đang làm (Knight Cref, Footman hoặc Mage)
        GameObject questTarget = GetQuestTargetEnemyPrefab(knightCrefPrefab, knightFootmanPrefab, magePrefab);
        if (questTarget != null)
            return questTarget;

        return PickWeightedRandom(
            (knightFootmanPrefab, knightFootmanWeight),
            (knightCrefPrefab, knightCrefWeight),
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
        if (knightFootmanPrefab != null && knightFootmanWeight > 0) activeTypes++;
        if (knightCrefPrefab != null && knightCrefWeight > 0) activeTypes++;
        if (magePrefab != null && mageWeight > 0) activeTypes++;

        return activeTypes > 0 ? $"🏰 Dungeon ({activeTypes} quái)" : "[Chưa Gán Quái]";
    }
}
