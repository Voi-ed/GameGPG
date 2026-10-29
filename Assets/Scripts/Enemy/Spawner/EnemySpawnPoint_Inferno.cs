using UnityEngine;

[AddComponentMenu("Gameplay/Spawner/Enemy Spawn Point - Inferno")]
public class EnemySpawnPoint_Inferno : EnemySpawnPoint
{
    [Header("Inferno Enemies (Ô Gán Riêng Từng Quái)")]
    [Tooltip("Prefab Tiểu Quỷ DemonKin")]
    [SerializeField] private GameObject demonKinPrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int demonKinWeight = 50;

    [Tooltip("Prefab Quỷ Nanh Vuốt SeveredFang")]
    [SerializeField] private GameObject severedFangPrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int severedFangWeight = 30;

    [Tooltip("Prefab Pháp Sư Lửa Mage")]
    [SerializeField] private GameObject fireMagePrefab;
    [Range(0, 100)]
    [Tooltip("Tỷ lệ xuất hiện (0 = không ra, 100 = tối đa)")]
    [SerializeField] private int fireMageWeight = 20;

    public GameObject DemonKinPrefab => demonKinPrefab;
    public GameObject SeveredFangPrefab => severedFangPrefab;
    public GameObject FireMagePrefab => fireMagePrefab;

#if UNITY_EDITOR
    private void Reset()
    {
        demonKinPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_DuskBorne/DemonKin/DemonKin.prefab");
        severedFangPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_DuskBorne/SeveredFang/SeveredFang.prefab");
        fireMagePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Enemy_Mage.prefab");
    }
#endif

    public override GameObject SelectEnemyPrefab()
    {
        if (CustomEnemyPrefab != null)
            return CustomEnemyPrefab;

        return PickWeightedRandom(
            (demonKinPrefab, demonKinWeight),
            (severedFangPrefab, severedFangWeight),
            (fireMagePrefab, fireMageWeight)
        );
    }

    protected override string GetSpawnPointLabel()
    {
        if (currentEnemy != null)
            return currentEnemy.name.Replace("(Clone)", "");

        if (CustomEnemyPrefab != null)
            return CustomEnemyPrefab.name;

        int activeTypes = 0;
        if (demonKinPrefab != null && demonKinWeight > 0) activeTypes++;
        if (severedFangPrefab != null && severedFangWeight > 0) activeTypes++;
        if (fireMagePrefab != null && fireMageWeight > 0) activeTypes++;

        return activeTypes > 0 ? $"🔥 Inferno ({activeTypes} quái)" : "[Chưa Gán Quái]";
    }
}
