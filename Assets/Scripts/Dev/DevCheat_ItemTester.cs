using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DevCheat_ItemTester : MonoBehaviour
{
    private static DevCheat_ItemTester instance;
    private bool showGui = true;
    private Rect windowRect = new Rect(10, 10, 260, 485);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        if (instance != null) return;
        GameObject go = new GameObject("[DEV_CHEAT_ITEM_TESTER]");
        instance = go.AddComponent<DevCheat_ItemTester>();
        DontDestroyOnLoad(go);
    }

    private void Update()
    {
        // Chỉ dùng F12 hoặc ~ để Ẩn/Hiện Menu Test, không dùng phím tắt cho các chức năng cheat
        if (Input.GetKeyDown(KeyCode.F12) || Input.GetKeyDown(KeyCode.BackQuote))
            showGui = !showGui;
    }

    private void OnGUI()
    {
        // Không hiện menu cheat lúc đang ở MainMenu
        if (SceneManager.GetActiveScene().name == "MainMenu")
            return;

        if (!showGui)
        {
            if (GUI.Button(new Rect(10, 10, 140, 28), "🛠️ Menu Test (~ / F12)"))
                showGui = true;
            return;
        }

        windowRect = GUI.Window(9999, windowRect, DrawWindow, "🛠️ MENU TEST TRANG BỊ & VFX");
    }

    private void DrawWindow(int windowID)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Mặc nhanh Full Bộ Set (4 món):", EditorStyles_BoldLabel());
        if (GUILayout.Button("✕ Đóng", GUILayout.Width(55), GUILayout.Height(20)))
        {
            showGui = false;
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Set Cuồng Nộ (Berserker)", GUILayout.Height(26)))
            EquipSet("Berserker");
        if (GUILayout.Button("Set Sát Thủ (Shadow Assassin)", GUILayout.Height(26)))
            EquipSet("Shadow");
        if (GUILayout.Button("Set Hoàng Gia (Stormguard)", GUILayout.Height(26)))
            EquipSet("Royal");
        if (GUILayout.Button("Set Pháp Sư (Astral Archmage)", GUILayout.Height(26)))
            EquipSet("Astral");
        if (GUILayout.Button("Set Viêm Đế (Inferno Overlord)", GUILayout.Height(26)))
            EquipSet("Inferno");

        GUILayout.Space(6);
        GUILayout.Label("Thao tác Máu / Sinh tồn:", EditorStyles_BoldLabel());
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Máu 20%", GUILayout.Height(26)))
            SetHealthPercent(0.2f);
        if (GUILayout.Button("Đầy Máu", GUILayout.Height(26)))
            RestoreHealthFull();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Tự Sát (Test Bất Tử Set)", GUILayout.Height(26)))
        {
            if (Player.instance != null && Player.instance.health != null)
                Player.instance.health.TakeDamage(999999, 0, ElementType.None, null);
        }

        GUILayout.Space(6);
        GUILayout.Label("Kỹ Năng & Điểm:", EditorStyles_BoldLabel());
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+100 SP", GUILayout.Height(26)))
            AddSkillPoints(100);

        string dragonBtnText = (GameManager.instance != null && GameManager.instance.IsDragonTransformed)
            ? "Hủy Hóa Rồng"
            : "Hóa Rồng";
        if (GUILayout.Button(dragonBtnText, GUILayout.Height(26)))
            ToggleDragonTransformation();
        GUILayout.EndHorizontal();

        GUILayout.Space(6);
        int currentGold = (Player.instance != null && Player.instance.inventory != null) ? Player.instance.inventory.gold : 0;
        GUILayout.Label($"Tiền Tệ / Vàng (Hiện có: {currentGold:N0}g):", EditorStyles_BoldLabel());
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+10,000 Vàng", GUILayout.Height(26)))
            AddGold(10000);
        if (GUILayout.Button("+100,000 Vàng", GUILayout.Height(26)))
            AddGold(100000);
        GUILayout.EndHorizontal();

        GUILayout.Space(6);
        GUILayout.Label("Nhiệm Vụ (Quest Tool):", EditorStyles_BoldLabel());
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Max Tiến Độ Quest", GUILayout.Height(26)))
            CompleteCurrentActiveQuestGoals();
        if (GUILayout.Button("Trả Nhanh Tất Cả", GUILayout.Height(26)))
            FastClaimAllCompletedQuests();
        GUILayout.EndHorizontal();

        GUILayout.Space(6);
        GUILayout.Label("💡 F12 hoặc phím [~]: Ẩn/Hiện Menu", EditorStyles_ItalicLabel());
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    private GUIStyle EditorStyles_BoldLabel()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontStyle = FontStyle.Bold;
        return style;
    }

    private GUIStyle EditorStyles_ItalicLabel()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontStyle = FontStyle.Italic;
        style.fontSize = 11;
        return style;
    }

    private List<ItemDataSO> GetAllItemDataSOs()
    {
        List<ItemDataSO> list = new List<ItemDataSO>();

        // 1. Lấy từ Database gốc của Player (hoạt động tốt trên WebGL & build thực tế)
        Player player = Player.instance;
        if (player != null && player.inventory != null && player.inventory.ItemDataBase != null)
        {
            if (player.inventory.ItemDataBase.itemList != null)
            {
                list.AddRange(player.inventory.ItemDataBase.itemList);
            }
        }

#if UNITY_EDITOR
        if (list.Count == 0)
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ItemDataSO");
            foreach (var guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                ItemDataSO item = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
                if (item != null)
                    list.Add(item);
            }
        }
#else
        if (list.Count == 0)
        {
            ItemDataSO[] loaded = Resources.FindObjectsOfTypeAll<ItemDataSO>();
            list.AddRange(loaded);
        }
#endif
        return list;
    }

    private void EquipSet(string setKeyword)
    {
        Player player = Player.instance;
        if (player == null || player.inventory == null)
        {
            Debug.LogWarning("[DevCheat] Chưa tìm thấy Player trong Scene!");
            return;
        }

        player.inventory.maxInventorySize = 50;

        // Tháo toàn bộ trang bị hiện có để làm sạch các slot (tránh xung đột slot trinket)
        List<Inventory_EquipmentSlot> currentSlots = new List<Inventory_EquipmentSlot>(player.inventory.equipList);
        foreach (var slot in currentSlots)
        {
            if (slot.HasItem())
            {
                player.inventory.UnequipItem(slot.equipedItem);
            }
        }

        List<ItemDataSO> allItems = GetAllItemDataSOs();
        List<EquipmentDataSO> setEquips = new List<EquipmentDataSO>();

        foreach (var item in allItems)
        {
            if (item is EquipmentDataSO equip && equip.itemSet != null)
            {
                if (equip.itemSet.name.ToLower().Contains(setKeyword.ToLower()) ||
                    equip.itemSet.setName.ToLower().Contains(setKeyword.ToLower()))
                {
                    if (!setEquips.Contains(equip))
                        setEquips.Add(equip);
                }
            }
        }

        foreach (var equip in setEquips)
        {
            Inventory_Item invItem = new Inventory_Item(equip);
            player.inventory.AddItem(invItem);
            player.inventory.TryEquipItem(invItem);
        }

        player.inventory.TriggerUpdateUI();

        int activeInSlots = 0;
        foreach (var slot in player.inventory.equipList)
        {
            if (slot.HasItem() && slot.equipedItem.itemData is EquipmentDataSO eq && eq.itemSet != null)
            {
                if (eq.itemSet.name.ToLower().Contains(setKeyword.ToLower()) ||
                    eq.itemSet.setName.ToLower().Contains(setKeyword.ToLower()))
                {
                    activeInSlots++;
                }
            }
        }

        Debug.Log($"<color=green>[DevCheat] Đã trang bị thành công {activeInSlots}/4 món thuộc Set {setKeyword}!</color>");
    }

    private void SetHealthPercent(float percent)
    {
        Player player = Player.instance;
        if (player != null && player.health != null)
        {
            player.health.SetHealthToPercent(percent);
            Debug.Log($"<color=yellow>[DevCheat] Đặt máu Player về {percent * 100}% để test Low HP VFX!</color>");
        }
    }

    private void RestoreHealthFull()
    {
        Player player = Player.instance;
        if (player != null && player.health != null && player.stats != null)
        {
            player.health.IncreaseHealth(player.stats.GetMaxHealth());
            Debug.Log("<color=green>[DevCheat] Hồi đầy máu cho Player!</color>");
        }
    }

    private void AddSkillPoints(int points)
    {
        UI_SkillTree skillTree = null;
        if (UI.instance != null && UI.instance.skillTreeUI != null)
        {
            skillTree = UI.instance.skillTreeUI;
        }
        else
        {
            skillTree = FindFirstObjectByType<UI_SkillTree>(FindObjectsInactive.Include);
        }

        if (skillTree != null)
        {
            skillTree.AddSkillPoints(points);
            Debug.Log($"<color=cyan>[DevCheat] Đã cộng thành công +{points} Điểm Kỹ Năng!</color>");
        }
        else
        {
            Debug.LogWarning("[DevCheat] Chưa tìm thấy UI_SkillTree trong Scene!");
        }
    }

    private void ToggleDragonTransformation()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ToggleDragonTransformation();
        }
        else
        {
            Debug.LogWarning("[DevCheat] Chưa tìm thấy GameManager trong Scene để Hóa Rồng!");
        }
    }

    private void AddGold(int amount)
    {
        Player player = Player.instance;
        Inventory_Player inv = (player != null && player.inventory != null) ? player.inventory : FindFirstObjectByType<Inventory_Player>();

        if (inv != null)
        {
            inv.gold += amount;
            inv.TriggerUpdateUI();
            Debug.Log($"<color=yellow>[DevCheat] Đã cộng +{amount:N0} Vàng! Tổng vàng hiện tại: {inv.gold:N0}g</color>");
        }
        else
        {
            Debug.LogWarning("[DevCheat] Chưa tìm thấy Inventory_Player trong Scene!");
        }
    }

    private void CompleteCurrentActiveQuestGoals()
    {
        Player player = Player.instance;
        if (player != null && player.questManager != null && player.questManager.activeQuests != null)
        {
            foreach (var q in player.questManager.activeQuests)
            {
                if (q != null && q.questDataSO != null && !q.CanGetReward())
                {
                    q.AddQuestProgress(q.questDataSO.requiredAmount);
                }
            }
            player.questManager.OnQuestListUpdated?.Invoke();
            SaveManager.instance?.SaveGame();
            Debug.Log("<color=green>[DevCheat] Đã hoàn thành toàn bộ mục tiêu của các nhiệm vụ đang làm!</color>");
        }
    }

    private void FastClaimAllCompletedQuests()
    {
        Player player = Player.instance;
        if (player != null && player.questManager != null && player.questManager.activeQuests != null)
        {
            var completedList = player.questManager.activeQuests.FindAll(q => q != null && q.CanGetReward());
            foreach (var q in completedList)
            {
                player.questManager.ClaimQuestRewardAndComplete(q);
            }
            Debug.Log($"<color=yellow>[DevCheat] Đã nhận thưởng và trả nhanh {completedList.Count} nhiệm vụ!</color>");
        }
    }
}

