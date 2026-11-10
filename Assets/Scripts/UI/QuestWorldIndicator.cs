using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Con trỏ Bàn Tay chỉ dẫn đa mục tiêu trong thế giới game (Multi-Target World Quest Pointer).
/// Hỗ trợ tự động định vị và hiển thị đồng thời nhiều con trỏ bay lơ lửng trên đầu các mục tiêu:
/// - Khi nhận nhiều nhiệm vụ: Mỗi nhiệm vụ có 1 con trỏ màu sắc riêng chỉ dẫn tới mục tiêu tương ứng.
/// - NPC (khi cần gặp/trả quest), Quái vật mục tiêu (khi đã ở trong map), hoặc Cổng Portal (khi cần đổi map).
/// </summary>
public class QuestWorldIndicator : MonoBehaviour
{
    public static QuestWorldIndicator instance { get; private set; }

    [Header("Indicator Settings")]
    [SerializeField] private Sprite handSprite;
    [SerializeField] private float floatSpeed = 5f;
    [SerializeField] private float floatRange = 0.2f;
    [SerializeField] private float yOffset = 1.6f;

    // Bảng màu phân biệt các nhiệm vụ khác nhau
    private readonly Color[] pointerColors = new Color[]
    {
        new Color(1f, 0.88f, 0.25f, 1f), // [1] Vàng Kim (Nhiệm vụ 1)
        new Color(0.25f, 0.9f, 1f, 1f),  // [2] Xanh Lam Ngọc (Nhiệm vụ 2)
        new Color(0.4f, 1f, 0.45f, 1f),  // [3] Xanh Lục Bảo (Nhiệm vụ 3)
        new Color(1f, 0.65f, 0.25f, 1f)  // [4] Cam Hổ Phách (Nhiệm vụ 4)
    };

    private class PointerInstance
    {
        public GameObject obj;
        public SpriteRenderer sr;
        public Transform target;
    }

    private readonly List<PointerInstance> pointerPool = new List<PointerInstance>();
    private float searchTimer = 0f;
    private const float SEARCH_INTERVAL = 0.35f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Khi đổi scene: Clear các target cũ và ẩn các con trỏ
        foreach (var p in pointerPool)
        {
            if (p != null)
            {
                p.target = null;
                if (p.obj != null)
                    p.obj.SetActive(false);
            }
        }
        searchTimer = 0f;
    }

    private void Update()
    {
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f)
        {
            searchTimer = SEARCH_INTERVAL;
            UpdateTargets();
        }

        // Cập nhật vị trí lơ lửng của tất cả con trỏ đang hoạt động
        float floatOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
        for (int i = 0; i < pointerPool.Count; i++)
        {
            var p = pointerPool[i];
            if (p != null && p.target != null && p.obj != null)
            {
                if (!p.obj.activeSelf)
                    p.obj.SetActive(true);

                p.obj.transform.position = p.target.position + new Vector3(0f, yOffset + floatOffset, 0f);
            }
            else if (p != null && p.obj != null && p.obj.activeSelf)
            {
                p.obj.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Tìm kiếm và gán mục tiêu cho từng nhiệm vụ đang hoạt động
    /// </summary>
    private void UpdateTargets()
    {
        if (Player.instance == null || Player.instance.questManager == null)
        {
            DeactivateAllPointers();
            return;
        }

        var questManager = Player.instance.questManager;
        var activeQuests = questManager.activeQuests;
        string activeScene = SceneManager.GetActiveScene().name;
        bool isVillage = activeScene.Contains("Village");

        // 1. Nếu KHÔNG có nhiệm vụ nào -> Chỉ về Joanna ở Làng để nhận nhiệm vụ
        if (activeQuests == null || activeQuests.Count == 0)
        {
            Transform defaultTarget = null;
            if (isVillage)
            {
                var joanna = FindFirstObjectByType<Object_CampGuard>();
                defaultTarget = joanna != null ? joanna.transform : null;
            }
            else
            {
                var portal = FindFirstObjectByType<Object_Portal>();
                defaultTarget = portal != null ? portal.transform : null;
            }

            if (defaultTarget != null)
            {
                AssignTargets(new List<Transform> { defaultTarget });
            }
            else
            {
                DeactivateAllPointers();
            }
            return;
        }

        // 2. Nếu có 1 hoặc nhiều nhiệm vụ -> Tìm mục tiêu tương ứng cho TỪNG nhiệm vụ
        List<Transform> questTargets = new List<Transform>();
        for (int i = 0; i < activeQuests.Count; i++)
        {
            var quest = activeQuests[i];
            if (quest == null || quest.questDataSO == null)
                continue;

            Transform target = GetTargetForQuest(quest, isVillage);
            if (target != null && !questTargets.Contains(target))
            {
                // Tránh gán trùng lặp (ví dụ 2 nhiệm vụ cùng chỉ vào Cổng Portal hoặc cùng về 1 NPC)
                questTargets.Add(target);
            }
        }

        AssignTargets(questTargets);
    }

    private void AssignTargets(List<Transform> targets)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            PointerInstance p = GetOrCreatePointer(i);
            p.target = targets[i];
            p.sr.color = pointerColors[i % pointerColors.Length];
            if (p.obj != null)
                p.obj.SetActive(true);
        }

        // Vô hiệu hóa các con trỏ dư thừa trong pool
        for (int i = targets.Count; i < pointerPool.Count; i++)
        {
            if (pointerPool[i] != null)
            {
                pointerPool[i].target = null;
                if (pointerPool[i].obj != null)
                    pointerPool[i].obj.SetActive(false);
            }
        }
    }

    private void DeactivateAllPointers()
    {
        foreach (var p in pointerPool)
        {
            if (p != null)
            {
                p.target = null;
                if (p.obj != null)
                    p.obj.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Xác định Transform mục tiêu chuẩn xác cho 1 nhiệm vụ cụ thể
    /// </summary>
    private Transform GetTargetForQuest(QuestData quest, bool isVillage)
    {
        if (quest == null || quest.questDataSO == null)
            return null;

        // A. Nhiệm vụ ĐÃ HOÀN THÀNH -> Chỉ về NPC trả quest (ở Làng) hoặc Cổng về Làng
        if (quest.CanGetReward())
        {
            if (isVillage)
            {
                Transform turnInNpc = FindNpcForQuestTurnIn(quest.questDataSO);
                return turnInNpc != null ? turnInNpc : FindFirstObjectByType<Object_CampGuard>()?.transform;
            }
            else
            {
                var portal = FindFirstObjectByType<Object_Portal>();
                return portal != null ? portal.transform : null;
            }
        }

        // B. Nhiệm vụ Nói chuyện / Tương tác NPC hoặc Khiêu chiến Boss Đấu Trường
        string targetId = quest.questDataSO.questTargetId;
        if (targetId == "enemy_crimson_blade" || targetId == "enemy_arch_demon" || targetId == "enemy_reaper")
        {
            if (isVillage)
            {
                var guard = FindFirstObjectByType<Object_CampGuard>();
                if (guard != null) return guard.transform;
            }
            else
            {
                Transform boss = FindNearestTargetEnemy(targetId);
                if (boss != null) return boss;
            }
        }

        if (quest.questDataSO.questType == QuestType.Talk ||
            targetId == "merchant_bob" || targetId == "eleonore_witch" || targetId == "blacksmith_joe" || targetId == "joanna_guard")
        {
            if (isVillage)
            {
                Transform targetNpc = FindTargetNpc(targetId);
                return targetNpc;
            }
            else
            {
                var portal = FindFirstObjectByType<Object_Portal>();
                return portal != null ? portal.transform : null;
            }
        }

        // C. Nhiệm vụ Săn quái / Thu thập nguyên liệu
        if (!string.IsNullOrEmpty(targetId))
        {
            Transform nearestEnemy = FindNearestTargetEnemy(targetId);
            if (nearestEnemy != null)
            {
                return nearestEnemy;
            }
        }

        // D. Nếu không có mục tiêu trong map hiện tại -> Chỉ về Cổng Portal để chuyển map
        var defaultPortal = FindFirstObjectByType<Object_Portal>();
        return defaultPortal != null ? defaultPortal.transform : null;
    }

    private Transform FindNpcForQuestTurnIn(QuestDataSO questSO)
    {
        if (questSO == null) return null;

        string npcName = HintManager.GetQuestGiverNpcName(questSO);

        if (npcName == "Bob")
        {
            var m = FindFirstObjectByType<Object_Merchant>();
            if (m != null) return m.transform;
        }
        else if (npcName == "Eleonore")
        {
            var a = FindFirstObjectByType<Object_Alchemist>();
            if (a != null) return a.transform;
        }
        else if (npcName == "Joe")
        {
            var b = FindFirstObjectByType<Object_Blacksmith>();
            if (b != null) return b.transform;
        }

        // Mặc định hoặc của Joanna
        var j = FindFirstObjectByType<Object_CampGuard>();
        if (j != null) return j.transform;

        return null;
    }

    private Transform FindTargetNpc(string targetId)
    {
        var npcs = FindObjectsByType<Object_NPC>(FindObjectsSortMode.None);
        foreach (var npc in npcs)
        {
            if (npc != null && npc.npcTargetQuestIdProperty == targetId)
                return npc.transform;
        }

        if (targetId == "merchant_bob")
        {
            var m = FindFirstObjectByType<Object_Merchant>();
            if (m != null) return m.transform;
        }
        else if (targetId == "eleonore_witch")
        {
            var a = FindFirstObjectByType<Object_Alchemist>();
            if (a != null) return a.transform;
        }
        else if (targetId == "blacksmith_joe")
        {
            var b = FindFirstObjectByType<Object_Blacksmith>();
            if (b != null) return b.transform;
        }
        else if (targetId == "joanna_guard")
        {
            var j = FindFirstObjectByType<Object_CampGuard>();
            if (j != null) return j.transform;
        }

        return null;
    }

    private Transform FindNearestTargetEnemy(string targetId)
    {
        if (Player.instance == null) return null;

        Vector3 playerPos = Player.instance.transform.position;
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        Transform bestTarget = null;
        float minDistanceSqr = float.MaxValue;

        for (int i = 0; i < allEnemies.Length; i++)
        {
            var enemy = allEnemies[i];
            if (enemy == null || enemy.gameObject == null || !enemy.gameObject.activeInHierarchy)
                continue;

            // Bỏ qua nếu quái đã chết
            if (enemy.health != null && enemy.health.isDead)
                continue;

            // Kiểm tra khớp targetId trực tiếp hoặc quái rơi nguyên liệu tương ứng
            bool isMatch = (enemy.questTargetId == targetId) ||
                           (targetId == "Material data - Slime jelly" && enemy.questTargetId == "enemy_slime");

            if (isMatch)
            {
                float distSqr = (enemy.transform.position - playerPos).sqrMagnitude;
                if (distSqr < minDistanceSqr)
                {
                    minDistanceSqr = distSqr;
                    bestTarget = enemy.transform;
                }
            }
        }

        return bestTarget;
    }

    private PointerInstance GetOrCreatePointer(int index)
    {
        while (pointerPool.Count <= index)
        {
            int newIdx = pointerPool.Count;
            GameObject pObj = new GameObject($"Quest_World_Pointer_{newIdx}");
            pObj.transform.SetParent(null);
            DontDestroyOnLoad(pObj);

            SpriteRenderer sr = pObj.AddComponent<SpriteRenderer>();
            if (handSprite == null)
            {
#if UNITY_EDITOR
                handSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Graphics/UI/HandPointer_Pixel.png");
#endif
            }
            if (handSprite != null)
            {
                sr.sprite = handSprite;
            }

            pObj.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
            pObj.transform.localScale = Vector3.one * 1.35f;
            sr.color = pointerColors[newIdx % pointerColors.Length];
            sr.sortingOrder = 300;
            pObj.SetActive(false);

            pointerPool.Add(new PointerInstance { obj = pObj, sr = sr, target = null });
        }

        return pointerPool[index];
    }

    private void OnDestroy()
    {
        foreach (var p in pointerPool)
        {
            if (p != null && p.obj != null)
                Destroy(p.obj);
        }
        pointerPool.Clear();

        if (instance == this)
            instance = null;
    }
}
