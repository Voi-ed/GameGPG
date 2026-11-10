using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    public enum InitialFacingDirection
    {
        Right = 1,
        Left = -1,
        Random = 0
    }

    [Header("Base Spawn Settings")]
    [Tooltip("Tự động spawn quái khi bắt đầu Scene")]
    [SerializeField] private bool spawnOnStart = true;

    [Tooltip("Thời gian đếm ngược (giây) để hồi sinh quái sau khi bị tiêu diệt")]
    [SerializeField] private float respawnCooldown = 20f;

    [Tooltip("Khoảng cách tối thiểu từ Player tới điểm spawn để được phép hồi sinh (tránh đẻ quái đè lên người chơi)")]
    [SerializeField] private float minPlayerDistance = 8f;

    [Tooltip("Tùy chọn: Chỉ hồi sinh khi điểm spawn nằm ngoài khung hình Camera")]
    [SerializeField] private bool requireOffscreen = false;

    [Tooltip("Mỗi lần hồi sinh sau khi chết sẽ chọn ngẫu nhiên lại quái mới")]
    [SerializeField] private bool randomizeOnRespawn = true;

    [Tooltip("Hướng quay mặt ban đầu của quái khi xuất hiện (Right, Left hoặc Random ngẫu nhiên)")]
    [SerializeField] private InitialFacingDirection initialFacing = InitialFacingDirection.Random;

    [Header("Custom Override (Optional)")]
    [Tooltip("Gán quái tùy chọn tại đây nếu muốn ghi đè hoặc bổ sung quái riêng cho điểm này")]
    [SerializeField] private GameObject customEnemyPrefab;

    [Header("Visual & Effects (Optional)")]
    [Tooltip("Hiệu ứng VFX xuất hiện khi quái spawn")]
    [SerializeField] private GameObject spawnVfxPrefab;

    public GameObject CustomEnemyPrefab => customEnemyPrefab;
    public GameObject CurrentEnemyPrefab => currentEnemyPrefab;
    public float RespawnCooldown => respawnCooldown;
    public bool IsSpawned => currentEnemy != null && !isEnemyDead;
    public bool IsRespawning => isRespawning;
    public float RemainingRespawnTime => Mathf.Max(0, respawnTimer);

    protected Enemy currentEnemy;
    protected GameObject currentEnemyPrefab;
    protected Entity_Health registeredHealth;
    protected bool isEnemyDead;
    protected bool isRespawning;
    protected float respawnTimer;
    protected Coroutine respawnCoroutine;

    protected virtual void Start()
    {
        if (spawnOnStart)
        {
            StartCoroutine(SpawnWithQuestSyncCo());
        }
    }

    protected virtual void OnEnable()
    {
        StartCoroutine(RegisterQuestListenerCo());
    }

    private IEnumerator RegisterQuestListenerCo()
    {
        while (Player.instance == null || Player.instance.questManager == null)
            yield return null;

        Player.instance.questManager.OnQuestListUpdated -= OnQuestListUpdated;
        Player.instance.questManager.OnQuestListUpdated += OnQuestListUpdated;
    }

    private void OnQuestListUpdated()
    {
        if (gameObject.activeInHierarchy && enabled)
        {
            // Nếu quái hiện tại không khớp với nhiệm vụ mới, chuyển đổi ngay sang quái nhiệm vụ
            GameObject target = SelectEnemyPrefab();
            if (target != null && currentEnemyPrefab != target)
            {
                currentEnemyPrefab = target;
                SpawnEnemy();
            }
        }
    }

    private IEnumerator SpawnWithQuestSyncCo()
    {
        // Chờ 1 frame để SaveManager và Player_QuestManager nạp dữ liệu từ file lưu
        yield return null;

        float timeout = 0.35f;
        while ((Player.instance == null || Player.instance.questManager == null) && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        SpawnEnemy();
    }

    public virtual GameObject SpawnEnemy()
    {
        if (randomizeOnRespawn || currentEnemyPrefab == null)
        {
            currentEnemyPrefab = SelectEnemyPrefab();
        }

        if (currentEnemyPrefab == null)
        {
            Debug.LogWarning($"[EnemySpawnPoint] GameObject '{gameObject.name}' không có quái nào được gán để spawn!");
            return null;
        }

        // Dọn dẹp quái cũ nếu còn tồn tại
        if (currentEnemy != null)
        {
            if (registeredHealth != null)
            {
                registeredHealth.OnDeath -= OnCurrentEnemyDeath;
                registeredHealth = null;
            }

            Destroy(currentEnemy.gameObject);
            currentEnemy = null;
        }

        // Hiệu ứng VFX xuất hiện nếu có
        if (spawnVfxPrefab != null)
        {
            Instantiate(spawnVfxPrefab, transform.position, Quaternion.identity);
        }

        GameObject enemyObj = Instantiate(currentEnemyPrefab, transform.position, Quaternion.identity);
        currentEnemy = enemyObj.GetComponent<Enemy>();

        registeredHealth = currentEnemy != null ? currentEnemy.health : null;
        if (registeredHealth == null)
        {
            registeredHealth = enemyObj.GetComponent<Entity_Health>();
        }

        if (currentEnemy != null)
        {
            int targetDir = DetermineFacingDirection();
            if (currentEnemy.facingDir != targetDir)
            {
                currentEnemy.Flip();
            }

            isEnemyDead = false;
            isRespawning = false;
        }

        if (registeredHealth != null)
        {
            registeredHealth.OnDeath += OnCurrentEnemyDeath;
        }

        return enemyObj;
    }

    /// <summary>
    /// Phương thức ảo để các class con theo từng Map override và chọn quái ngẫu nhiên
    /// </summary>
    public virtual GameObject SelectEnemyPrefab()
    {
        return customEnemyPrefab;
    }

    /// <summary>
    /// Kiểm tra nhiệm vụ hiện tại hoặc tiến trình cốt truyện để ƯU TIÊN SINH ĐÚNG quái nhiệm vụ trong map.
    /// </summary>
    protected GameObject GetQuestTargetEnemyPrefab(params GameObject[] candidatePrefabs)
    {
        if (candidatePrefabs == null || candidatePrefabs.Length == 0)
            return null;

        // 1. Kiểm tra các quest đang làm từ Player_QuestManager
        if (Player.instance != null && Player.instance.questManager != null)
        {
            var activeQuests = Player.instance.questManager.activeQuests;
            if (activeQuests != null && activeQuests.Count > 0)
            {
                List<GameObject> matchingCandidates = new List<GameObject>();

                for (int c = 0; c < candidatePrefabs.Length; c++)
                {
                    GameObject prefab = candidatePrefabs[c];
                    if (prefab == null) continue;

                    Enemy enemyComp = prefab.GetComponent<Enemy>();
                    if (enemyComp == null) continue;

                    string enemyId = enemyComp.questTargetId;
                    if (string.IsNullOrEmpty(enemyId)) continue;

                    for (int i = 0; i < activeQuests.Count; i++)
                    {
                        var q = activeQuests[i];
                        if (q == null || q.questDataSO == null) continue;

                        // Nếu quest đã đủ số lượng (đã xong, chờ nhận thưởng) -> KHÔNG ưu tiên sinh quái này nữa!
                        if (q.CanGetReward()) continue;

                        string neededId = q.questDataSO.questTargetId;

                        // Khớp trực tiếp questTargetId (ví dụ: enemy_knight_cref, enemy_knight_footman,...)
                        if (enemyId == neededId)
                        {
                            matchingCandidates.Add(prefab);
                            break;
                        }

                        // Khớp quest thu thập vật phẩm (ví dụ: Slime Jelly -> quái enemy_slime)
                        if (neededId == "Material data - Slime jelly" && enemyId == "enemy_slime")
                        {
                            matchingCandidates.Add(prefab);
                            break;
                        }
                    }
                }

                if (matchingCandidates.Count > 0)
                {
                    // 100% sinh đúng con quái đang cần làm nhiệm vụ trong map này!
                    return matchingCandidates[Random.Range(0, matchingCandidates.Count)];
                }
            }
        }

        // 2. Dự phòng: Đọc từ SaveManager theo tiến trình cốt truyện nếu chưa nhận quest
        string storylineTargetId = null;
        if (SaveManager.instance != null && SaveManager.instance.GetGameData() != null)
        {
            var data = SaveManager.instance.GetGameData();
            int completedCount = data.completedQuests != null ? data.completedQuests.Count : 0;
            if (Player.instance != null && Player.instance.questManager != null && Player.instance.questManager.completedQuests != null)
                completedCount = Mathf.Max(completedCount, Player.instance.questManager.completedQuests.Count);

            // Từng chặng nhiệm vụ tương ứng với 1 loại quái duy nhất:
            if (completedCount == 0) storylineTargetId = "enemy_slime";
            else if (completedCount == 1) storylineTargetId = "enemy_archerElf";
            else if (completedCount == 2) storylineTargetId = "enemy_mage";
            else if (completedCount == 3) storylineTargetId = "enemy_skeleton";
            else if (completedCount == 4) storylineTargetId = "enemy_knight_cref";
            else if (completedCount == 5) storylineTargetId = "enemy_severed_fang";
            else if (completedCount == 6) storylineTargetId = "enemy_samuraizipp";
            else if (completedCount == 7) storylineTargetId = "enemy_arch_demon";
        }

        if (!string.IsNullOrEmpty(storylineTargetId))
        {
            for (int i = 0; i < candidatePrefabs.Length; i++)
            {
                GameObject prefab = candidatePrefabs[i];
                if (prefab == null) continue;

                Enemy enemyComponent = prefab.GetComponent<Enemy>();
                if (enemyComponent != null && enemyComponent.questTargetId == storylineTargetId)
                {
                    return prefab;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Helper hàm chọn quái theo trọng số từ danh sách các slot riêng
    /// </summary>
    protected GameObject PickWeightedRandom(params (GameObject prefab, int weight)[] candidates)
    {
        int totalWeight = 0;
        for (int i = 0; i < candidates.Length; i++)
        {
            if (candidates[i].prefab != null && candidates[i].weight > 0)
                totalWeight += candidates[i].weight;
        }

        if (totalWeight <= 0)
        {
            for (int i = 0; i < candidates.Length; i++)
            {
                if (candidates[i].prefab != null)
                    return candidates[i].prefab;
            }
            return null;
        }

        int roll = Random.Range(0, totalWeight);
        int currentCumulative = 0;

        for (int i = 0; i < candidates.Length; i++)
        {
            if (candidates[i].prefab == null || candidates[i].weight <= 0)
                continue;

            currentCumulative += candidates[i].weight;
            if (roll < currentCumulative)
            {
                return candidates[i].prefab;
            }
        }

        return null;
    }

    protected int DetermineFacingDirection()
    {
        if (initialFacing == InitialFacingDirection.Random)
        {
            return Random.value > 0.5f ? 1 : -1;
        }

        return (int)initialFacing;
    }

    protected virtual void OnCurrentEnemyDeath()
    {
        if (registeredHealth != null)
        {
            registeredHealth.OnDeath -= OnCurrentEnemyDeath;
            registeredHealth = null;
        }

        isEnemyDead = true;

        if (gameObject.activeInHierarchy && enabled)
        {
            if (respawnCoroutine != null)
                StopCoroutine(respawnCoroutine);

            respawnCoroutine = StartCoroutine(RespawnCo());
        }
    }

    protected virtual IEnumerator RespawnCo()
    {
        isRespawning = true;
        respawnTimer = respawnCooldown;

        while (respawnTimer > 0)
        {
            respawnTimer -= Time.deltaTime;
            yield return null;
        }

        while (IsPlayerTooClose() || (requireOffscreen && IsVisibleOnScreen()))
        {
            yield return new WaitForSeconds(0.6f);
        }

        isRespawning = false;
        respawnCoroutine = null;
        SpawnEnemy();
    }

    protected bool IsPlayerTooClose()
    {
        if (minPlayerDistance <= 0)
            return false;

        Player player = Player.instance != null ? Player.instance : FindFirstObjectByType<Player>();
        if (player == null)
            return false;

        float dist = Vector2.Distance(player.transform.position, transform.position);
        return dist < minPlayerDistance;
    }

    protected bool IsVisibleOnScreen()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return false;

        Vector3 viewportPoint = cam.WorldToViewportPoint(transform.position);
        return viewportPoint.z > 0 &&
               viewportPoint.x >= -0.1f && viewportPoint.x <= 1.1f &&
               viewportPoint.y >= -0.1f && viewportPoint.y <= 1.1f;
    }

    public virtual void ClearEnemy()
    {
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }

        if (registeredHealth != null)
        {
            registeredHealth.OnDeath -= OnCurrentEnemyDeath;
            registeredHealth = null;
        }

        if (currentEnemy != null)
        {
            Destroy(currentEnemy.gameObject);
            currentEnemy = null;
        }

        currentEnemyPrefab = null;
        isEnemyDead = false;
        isRespawning = false;
        respawnTimer = 0;
    }

    protected virtual void OnDisable()
    {
        if (Player.instance != null && Player.instance.questManager != null)
        {
            Player.instance.questManager.OnQuestListUpdated -= OnQuestListUpdated;
        }

        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }

        if (registeredHealth != null)
        {
            registeredHealth.OnDeath -= OnCurrentEnemyDeath;
            registeredHealth = null;
        }
    }

    protected virtual void OnDestroy()
    {
        if (Player.instance != null && Player.instance.questManager != null)
        {
            Player.instance.questManager.OnQuestListUpdated -= OnQuestListUpdated;
        }

        if (registeredHealth != null)
        {
            registeredHealth.OnDeath -= OnCurrentEnemyDeath;
            registeredHealth = null;
        }
    }

    protected virtual string GetSpawnPointLabel()
    {
        if (currentEnemy != null)
            return currentEnemy.name.Replace("(Clone)", "");

        if (customEnemyPrefab != null)
            return customEnemyPrefab.name;

        return "[Chưa Gán Quái]";
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = isRespawning 
            ? new Color(1f, 0.45f, 0.15f, 0.85f) 
            : (isEnemyDead ? Color.red : new Color(0.2f, 0.85f, 0.35f, 0.85f));

        Gizmos.DrawWireSphere(transform.position, 0.6f);

        if (initialFacing == InitialFacingDirection.Random)
        {
            Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.8f);
            Gizmos.DrawRay(transform.position, Vector3.right * 1f);
            Gizmos.DrawRay(transform.position, Vector3.left * 1f);
        }
        else
        {
            Vector3 dirVector = (initialFacing == InitialFacingDirection.Right ? Vector3.right : Vector3.left) * 1.2f;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, dirVector);
            Vector3 arrowHead1 = initialFacing == InitialFacingDirection.Right ? new Vector3(-0.3f, 0.25f, 0) : new Vector3(0.3f, 0.25f, 0);
            Vector3 arrowHead2 = initialFacing == InitialFacingDirection.Right ? new Vector3(-0.3f, -0.25f, 0) : new Vector3(0.3f, -0.25f, 0);
            Gizmos.DrawRay(transform.position + dirVector, arrowHead1);
            Gizmos.DrawRay(transform.position + dirVector, arrowHead2);
        }

#if UNITY_EDITOR
        string enemyInfo = GetSpawnPointLabel();
        string statusText = isRespawning ? $" [Hồi sinh: {Mathf.CeilToInt(respawnTimer)}s]" : "";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.9f, $"{enemyInfo}{statusText}");
#endif
    }
}
