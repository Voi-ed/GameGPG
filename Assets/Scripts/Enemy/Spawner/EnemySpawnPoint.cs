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
            SpawnEnemy();
        }
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
