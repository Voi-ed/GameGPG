using UnityEngine;

[AddComponentMenu("Gameplay/Spawner/Enemy Spawn Point - Metroidvania Ruins")]
public class EnemySpawnPoint_MetroidvaniaRuins : EnemySpawnPoint
{
    [Header("Metroidvania Ruins Elite Enemies (Ô Gán Riêng Từng Quái)")]
    [Tooltip("Prefab Hiệp Sĩ Trưởng Cref")]
    [SerializeField] private GameObject knightCrefPrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int knightCrefWeight = 30;

    [Tooltip("Prefab Quỷ Nanh Vuốt SeveredFang")]
    [SerializeField] private GameObject severedFangPrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int severedFangWeight = 30;

    [Tooltip("Prefab Ninja Samurai Zipp")]
    [SerializeField] private GameObject samuraiZippPrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int samuraiZippWeight = 25;

    [Tooltip("Prefab Tiểu Quỷ DemonKin")]
    [SerializeField] private GameObject demonKinPrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int demonKinWeight = 15;

    public GameObject KnightCrefPrefab => knightCrefPrefab;
    public GameObject SeveredFangPrefab => severedFangPrefab;
    public GameObject SamuraiZippPrefab => samuraiZippPrefab;
    public GameObject DemonKinPrefab => demonKinPrefab;

#if UNITY_EDITOR
    private void Reset()
    {
        knightCrefPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_Knight/Enemy_KnightCref.prefab");
        severedFangPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_DuskBorne/SeveredFang/SeveredFang.prefab");
        samuraiZippPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_Samurai/Enemy_SamuraiZipp.prefab");
        demonKinPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_DuskBorne/DemonKin/DemonKin.prefab");
    }
#endif

    public override GameObject SelectEnemyPrefab()
    {
        if (CustomEnemyPrefab != null)
            return CustomEnemyPrefab;

        return PickWeightedRandom(
            (knightCrefPrefab, knightCrefWeight),
            (severedFangPrefab, severedFangWeight),
            (samuraiZippPrefab, samuraiZippWeight),
            (demonKinPrefab, demonKinWeight)
        );
    }

    protected override string GetSpawnPointLabel()
    {
        if (currentEnemy != null)
            return currentEnemy.name.Replace("(Clone)", "");

        if (CustomEnemyPrefab != null)
            return CustomEnemyPrefab.name;

        int activeTypes = 0;
        if (knightCrefPrefab != null && knightCrefWeight > 0) activeTypes++;
        if (severedFangPrefab != null && severedFangWeight > 0) activeTypes++;
        if (samuraiZippPrefab != null && samuraiZippWeight > 0) activeTypes++;
        if (demonKinPrefab != null && demonKinWeight > 0) activeTypes++;

        return activeTypes > 0 ? $"🏛️ Ruins ({activeTypes} quái)" : "[Chưa Gán Quái]";
    }
}
