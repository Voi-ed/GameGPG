using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    private static DamageTextManager _instance;
    public static DamageTextManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DamageTextManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("___ DAMAGE TEXT MANAGER (Auto) ___");
                    _instance = go.AddComponent<DamageTextManager>();
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Prefab & Pool Configuration")]
    [Tooltip("Prefab DamageText chứa component DamageTextController và TextMeshPro. Nếu để trống, hệ thống sẽ tự sinh template dự phòng.")]
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private int prewarmCount = 25;

    [Header("Spawn Offset")]
    [SerializeField] private float yOffset = 0.2f;

    private GameObject runtimeTemplatePrefab;
    private float lastSpawnTime = -1f;
    private Vector3 lastSpawnPos = Vector3.zero;
    private int consecutiveHitCount = 0;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        PrewarmPool();
    }

    private void PrewarmPool()
    {
        GameObject prefabToUse = GetOrCreatePrefabTemplate();
        if (prefabToUse != null && GameManager.instance != null && GameManager.instance.vfxPool != null)
        {
            GameManager.instance.vfxPool.Prewarm(prefabToUse, prewarmCount);
        }
    }

    /// <summary>
    /// Hiển thị số sát thương bay lên tại vị trí world space
    /// </summary>
    public void ShowDamageText(Vector3 position, float damage, bool isCrit, ElementType element)
    {
        if (damage <= 0)
            return;

        GameObject prefab = GetOrCreatePrefabTemplate();
        if (prefab == null)
            return;

        // Tính toán đòn bồi / đa sát thương cùng 1 lúc trên 1 mục tiêu (vd: Đòn đánh + Backstab sát thủ / Giật sét)
        float timeSinceLast = Time.time - lastSpawnTime;
        float dist = Vector3.Distance(position, lastSpawnPos);

        if (timeSinceLast < 0.25f && dist < 1.5f)
        {
            consecutiveHitCount++;
        }
        else
        {
            consecutiveHitCount = 0;
        }

        lastSpawnTime = Time.time;
        lastSpawnPos = position;

        Vector3 spawnPos = position + Vector3.up * yOffset;
        GameObject textObj = SpawnFromPool(prefab, spawnPos);

        if (textObj != null && textObj.TryGetComponent<DamageTextController>(out var controller))
        {
            controller.Setup(damage, isCrit, element, consecutiveHitCount);
        }
    }

    /// <summary>
    /// Hiển thị chữ MISS/DODGE khi né đòn thành công
    /// </summary>
    public void ShowMissText(Vector3 position)
    {
        GameObject prefab = GetOrCreatePrefabTemplate();
        if (prefab == null)
            return;

        Vector3 spawnPos = position + Vector3.up * yOffset;
        GameObject textObj = SpawnFromPool(prefab, spawnPos);

        if (textObj != null && textObj.TryGetComponent<DamageTextController>(out var controller))
        {
            controller.SetupMiss();
        }
    }

    private GameObject SpawnFromPool(GameObject prefab, Vector3 spawnPos)
    {
        GameObject textObj = null;
        if (GameManager.instance != null && GameManager.instance.vfxPool != null)
        {
            textObj = GameManager.instance.vfxPool.Get(prefab, spawnPos, Quaternion.identity);
        }
        else
        {
            textObj = Instantiate(prefab, spawnPos, Quaternion.identity);
        }

        if (textObj != null && !textObj.activeSelf)
        {
            textObj.SetActive(true);
        }

        return textObj;
    }

    private GameObject GetOrCreatePrefabTemplate()
    {
        if (damageTextPrefab != null)
            return damageTextPrefab;

#if UNITY_EDITOR
        damageTextPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/VFX/DamageText.prefab");
        if (damageTextPrefab != null)
            return damageTextPrefab;
#endif

        if (runtimeTemplatePrefab == null)
        {
            GameObject template = new GameObject("[Template] DamageText_Runtime");
            template.transform.SetParent(transform);
            template.AddComponent<DamageTextController>();
            template.SetActive(false);
            runtimeTemplatePrefab = template;
        }

        return runtimeTemplatePrefab;
    }
}
