using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
public class DevCheat_ItemTester : MonoBehaviour
{
    private static DevCheat_ItemTester instance;
    private bool showGui = true;
    private Rect windowRect = new Rect(10, 10, 260, 345);

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
        if (Input.GetKeyDown(KeyCode.F12))
            showGui = !showGui;

        if (Input.GetKeyDown(KeyCode.F2))
            EquipSet("Berserker");

        if (Input.GetKeyDown(KeyCode.F3))
            EquipSet("Shadow");

        if (Input.GetKeyDown(KeyCode.F4))
            EquipSet("Royal");

        if (Input.GetKeyDown(KeyCode.F5))
            EquipSet("Astral");

        if (Input.GetKeyDown(KeyCode.F6))
            EquipSet("Inferno");

        if (Input.GetKeyDown(KeyCode.H))
            SetHealthPercent(0.2f);

        if (Input.GetKeyDown(KeyCode.J))
            RestoreHealthFull();
    }

    private void OnGUI()
    {
        if (!showGui)
        {
            if (GUI.Button(new Rect(10, 10, 120, 28), "Mở Menu Test (F12)"))
                showGui = true;
            return;
        }

        windowRect = GUI.Window(9999, windowRect, DrawWindow, "🛠️ MENU TEST TRANG BỊ & VFX");
    }

    private void DrawWindow(int windowID)
    {
        GUILayout.Label("Mặc nhanh Full Bộ Set (4 món):", EditorStyles_BoldLabel());
        if (GUILayout.Button("[F2] Set Cuồng Nộ (Berserker)", GUILayout.Height(26)))
            EquipSet("Berserker");
        if (GUILayout.Button("[F3] Set Sát Thủ (Shadow Assassin)", GUILayout.Height(26)))
            EquipSet("Shadow");
        if (GUILayout.Button("[F4] Set Hoàng Gia (Stormguard)", GUILayout.Height(26)))
            EquipSet("Royal");
        if (GUILayout.Button("[F5] Set Pháp Sư (Astral Archmage)", GUILayout.Height(26)))
            EquipSet("Astral");
        if (GUILayout.Button("[F6] Set Viêm Đế (Inferno Overlord)", GUILayout.Height(26)))
            EquipSet("Inferno");

        GUILayout.Space(6);
        GUILayout.Label("Thao tác Máu / Sinh tồn:", EditorStyles_BoldLabel());
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("[H] Máu 20%", GUILayout.Height(26)))
            SetHealthPercent(0.2f);
        if (GUILayout.Button("[J] Đầy Máu", GUILayout.Height(26)))
            RestoreHealthFull();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("[N] Tự Sát (Test Bất Tử Set)", GUILayout.Height(26)))
        {
            if (Player.instance != null && Player.instance.health != null)
                Player.instance.health.TakeDamage(999999, 0, ElementType.None, null);
        }

        GUILayout.Space(6);
        GUILayout.Label("💡 F12: Ẩn/Hiện | 'C': Mở Túi đồ", EditorStyles_ItalicLabel());
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
#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ItemDataSO");
        foreach (var guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ItemDataSO item = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
            if (item != null)
                list.Add(item);
        }
#else
        ItemDataSO[] loaded = Resources.FindObjectsOfTypeAll<ItemDataSO>();
        list.AddRange(loaded);
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
}
#endif
