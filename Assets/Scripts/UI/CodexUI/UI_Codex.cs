using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CodexTab
{
    Items,
    Monsters,
    Effects
}

public class UI_Codex : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private Codex_DatabaseSO database;

    [Header("Navigation Tabs")]
    [SerializeField] private Button tabItemButton;
    [SerializeField] private Button tabMonsterButton;
    [SerializeField] private Button tabEffectButton;
    [SerializeField] private Button closeButton;

    [Header("Left Page - List & Filters")]
    [SerializeField] private Transform listContentParent;
    [SerializeField] private GameObject itemFilterBar;
    [SerializeField] private RectTransform leftScrollViewRect;
    [SerializeField] private UI_Codex_ItemSlot itemSlotPrefab;
    [SerializeField] private UI_Codex_MonsterSlot monsterSlotPrefab;
    [SerializeField] private UI_Codex_EffectSlot effectSlotPrefab;
    [SerializeField] private TextMeshProUGUI pageTitleText;

    [Header("Right Page - Item Detail Panel")]
    [SerializeField] private GameObject itemDetailPanel;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image itemRarityBorder;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemRarityText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI itemStatsText;
    [SerializeField] private TextMeshProUGUI itemEffectText;
    [SerializeField] private TextMeshProUGUI itemSetText;
    [SerializeField] private Transform dropSourcesParent;
    [SerializeField] private UI_Codex_DropSourceRow dropSourceRowPrefab;
    [SerializeField] private GameObject noDropSourceNotice;

    [Header("Right Page - Monster Detail Panel")]
    [SerializeField] private GameObject monsterDetailPanel;
    [SerializeField] private Image monsterIcon;
    [SerializeField] private TextMeshProUGUI monsterNameText;
    [SerializeField] private TextMeshProUGUI monsterRankText;
    [SerializeField] private TextMeshProUGUI monsterHpText;
    [SerializeField] private TextMeshProUGUI monsterLocationText;
    [SerializeField] private TextMeshProUGUI monsterDescText;
    [SerializeField] private Transform dropItemsParent;
    [SerializeField] private UI_Codex_DropItemRow dropItemRowPrefab;
    [SerializeField] private GameObject noDropItemNotice;

    [Header("Right Page - Effect Detail Panel")]
    [SerializeField] private GameObject effectDetailPanel;
    [SerializeField] private TextMeshProUGUI effectNameText;
    [SerializeField] private TextMeshProUGUI effectDescText;
    [SerializeField] private TextMeshProUGUI effectItemsCountText;

    private CodexTab currentTab = CodexTab.Items;
    private ItemListFilterType itemFilter = ItemListFilterType.All;

    private void Awake()
    {
        if (database != null)
            database.Initialize();

        if (tabItemButton != null) tabItemButton.onClick.AddListener(() => SwitchTab(CodexTab.Items));
        if (tabMonsterButton != null) tabMonsterButton.onClick.AddListener(() => SwitchTab(CodexTab.Monsters));
        if (tabEffectButton != null) tabEffectButton.onClick.AddListener(() => SwitchTab(CodexTab.Effects));
        if (closeButton != null) closeButton.onClick.AddListener(CloseCodex);
    }

    private void OnEnable()
    {
        if (database == null) return;
        database.Initialize();
        SwitchTab(currentTab);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseCodex();
        }
    }

    public void CloseCodex()
    {
        if (UI.instance != null)
        {
            UI.instance.SwitchToInGameUI();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void SwitchTab(CodexTab tab)
    {
        currentTab = tab;

        ClearListContent();
        HideAllDetailPanels();

        if (itemFilterBar != null)
        {
            itemFilterBar.SetActive(tab == CodexTab.Items);
        }

        if (leftScrollViewRect != null)
        {
            leftScrollViewRect.anchoredPosition = new Vector2(leftScrollViewRect.anchoredPosition.x, tab == CodexTab.Items ? -72 : -38);
            leftScrollViewRect.sizeDelta = new Vector2(leftScrollViewRect.sizeDelta.x, tab == CodexTab.Items ? 442 : 476);
        }

        UpdateTabVisuals();

        switch (tab)
        {
            case CodexTab.Items:
                if (pageTitleText != null) pageTitleText.text = "BÁCH KHOA TRANG BỊ & VẬT PHẨM";
                PopulateItems();
                break;
            case CodexTab.Monsters:
                if (pageTitleText != null) pageTitleText.text = "SỔ TAY QUÁI VẬT & CHIẾN TRƯỜNG";
                PopulateMonsters();
                break;
            case CodexTab.Effects:
                if (pageTitleText != null) pageTitleText.text = "TỪ ĐIỂN HIỆU ỨNG CHIẾN ĐẤU";
                PopulateEffects();
                break;
        }
    }

    private void UpdateTabVisuals()
    {
        SetTabButtonState(tabItemButton, currentTab == CodexTab.Items);
        SetTabButtonState(tabMonsterButton, currentTab == CodexTab.Monsters);
        SetTabButtonState(tabEffectButton, currentTab == CodexTab.Effects);
    }

    private void SetTabButtonState(Button btn, bool isActive)
    {
        if (btn == null) return;
        Image img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.color = isActive ? new Color(0.30f, 0.26f, 0.40f) : new Color(0.16f, 0.14f, 0.20f);
        }
        TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            txt.color = isActive ? new Color(1f, 0.88f, 0.45f) : new Color(0.70f, 0.70f, 0.75f);
        }
    }

    public void JumpToMonster(Codex_MonsterDataSO monster)
    {
        if (monster == null) return;
        currentTab = CodexTab.Monsters;
        ClearListContent();
        if (pageTitleText != null) pageTitleText.text = "SỔ TAY QUÁI VẬT & CHIẾN TRƯỜNG";
        PopulateMonsters();
        ShowMonsterDetails(monster);
    }

    public void JumpToItem(ItemDataSO item)
    {
        if (item == null) return;
        currentTab = CodexTab.Items;
        itemFilter = ItemListFilterType.All;
        ClearListContent();
        if (pageTitleText != null) pageTitleText.text = "BÁCH KHOA TRANG BỊ & VẬT PHẨM";
        PopulateItems();
        ShowItemDetails(item);
    }

    private void ClearListContent()
    {
        if (listContentParent == null) return;
        foreach (Transform child in listContentParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void HideAllDetailPanels()
    {
        if (itemDetailPanel != null) itemDetailPanel.SetActive(false);
        if (monsterDetailPanel != null) monsterDetailPanel.SetActive(false);
        if (effectDetailPanel != null) effectDetailPanel.SetActive(false);
    }

    #region Items Tab
    public void SetItemFilter(int filterIndex)
    {
        itemFilter = (ItemListFilterType)filterIndex;
        if (currentTab == CodexTab.Items)
        {
            ClearListContent();
            PopulateItems();
        }
    }

    private void PopulateItems()
    {
        if (database == null || database.allItems == null || itemSlotPrefab == null) return;

        var items = database.allItems.AsEnumerable();
        if (itemFilter != ItemListFilterType.All)
        {
            ItemType targetType = (ItemType)System.Enum.Parse(typeof(ItemType), itemFilter.ToString());
            items = items.Where(i => i.itemType == targetType);
        }

        ItemDataSO firstItem = null;
        foreach (var item in items)
        {
            if (item == null) continue;
            if (firstItem == null) firstItem = item;

            var slot = Instantiate(itemSlotPrefab, listContentParent);
            slot.Setup(item, this);
        }

        if (firstItem != null)
        {
            ShowItemDetails(firstItem);
        }
    }

    public void ShowItemDetails(ItemDataSO item)
    {
        if (item == null) return;

        HideAllDetailPanels();
        if (itemDetailPanel != null) itemDetailPanel.SetActive(true);

        Color rarityCol = UI_Codex_ItemSlot.GetRarityColor(item.itemRarity);

        if (itemIcon != null)
        {
            itemIcon.sprite = item.itemIcon;
            itemIcon.enabled = (item.itemIcon != null);
        }

        if (itemRarityBorder != null) itemRarityBorder.color = rarityCol;
        if (itemNameText != null)
        {
            itemNameText.text = item.itemName;
            itemNameText.color = rarityCol;
        }
        if (itemRarityText != null)
        {
            itemRarityText.text = item.itemRarity.ToString().ToUpper();
            itemRarityText.color = rarityCol;
        }
        if (itemTypeText != null) itemTypeText.text = $"Loại: {item.itemType}";

        // Modifiers
        if (itemStatsText != null)
        {
            if (item is EquipmentDataSO eq && eq.modifiers != null && eq.modifiers.Length > 0)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (var mod in eq.modifiers)
                {
                    string sign = mod.value >= 0 ? "+" : "";
                    sb.AppendLine($"{mod.statType}: {sign}{mod.value}");
                }
                itemStatsText.text = sb.ToString();
            }
            else
            {
                itemStatsText.text = "Không có chỉ số cộng thêm.";
            }
        }

        // Effect
        if (itemEffectText != null)
        {
            if (item.itemEffect != null && !string.IsNullOrEmpty(item.itemEffect.effectDescription))
                itemEffectText.text = $"<b>Hiệu Ứng:</b> {item.itemEffect.effectDescription}";
            else
                itemEffectText.text = "Không có hiệu ứng đặc biệt.";
        }

        // Set
        if (itemSetText != null)
        {
            if (item is EquipmentDataSO eqSet && eqSet.itemSet != null)
                itemSetText.text = $"<b>Set:</b> {eqSet.itemSet.name}";
            else
                itemSetText.text = "";
        }

        // Drop Sources
        if (dropSourcesParent != null && dropSourceRowPrefab != null)
        {
            foreach (Transform c in dropSourcesParent) Destroy(c.gameObject);

            var sources = database.GetSourcesForItem(item);
            if (noDropSourceNotice != null) noDropSourceNotice.SetActive(sources.Count == 0);

            foreach (var src in sources)
            {
                var row = Instantiate(dropSourceRowPrefab, dropSourcesParent);
                row.Setup(src.monster, src.dropChance, this);
            }
        }
    }
    #endregion

    #region Monsters Tab
    private void PopulateMonsters()
    {
        if (database == null || database.allMonsters == null || monsterSlotPrefab == null) return;

        Codex_MonsterDataSO firstMonster = null;
        foreach (var monster in database.allMonsters)
        {
            if (monster == null) continue;
            if (firstMonster == null) firstMonster = monster;

            var slot = Instantiate(monsterSlotPrefab, listContentParent);
            slot.Setup(monster, this);
        }

        if (firstMonster != null)
        {
            ShowMonsterDetails(firstMonster);
        }
    }

    public void ShowMonsterDetails(Codex_MonsterDataSO monster)
    {
        if (monster == null) return;

        HideAllDetailPanels();
        if (monsterDetailPanel != null) monsterDetailPanel.SetActive(true);

        if (monsterIcon != null)
        {
            monsterIcon.sprite = monster.monsterIcon;
            monsterIcon.enabled = (monster.monsterIcon != null);
        }

        if (monsterNameText != null)
        {
            monsterNameText.text = monster.monsterName;
            monsterNameText.color = UI_Codex_MonsterSlot.GetRankColor(monster.monsterRank);
        }

        if (monsterRankText != null)
        {
            monsterRankText.text = $"Cấp Bậc: {monster.monsterRank}";
            monsterRankText.color = UI_Codex_MonsterSlot.GetRankColor(monster.monsterRank);
        }

        if (monsterHpText != null) monsterHpText.text = $"Máu Cơ Bản: {monster.baseHealth:N0}";
        if (monsterLocationText != null) monsterLocationText.text = $"Khu Vực: {monster.mapLocation}";
        if (monsterDescText != null) monsterDescText.text = monster.description;

        // Drop Items
        if (dropItemsParent != null && dropItemRowPrefab != null)
        {
            foreach (Transform c in dropItemsParent) Destroy(c.gameObject);

            var drops = database.GetDropsForMonster(monster);
            if (noDropItemNotice != null) noDropItemNotice.SetActive(drops.Count == 0);

            foreach (var drop in drops)
            {
                var row = Instantiate(dropItemRowPrefab, dropItemsParent);
                row.Setup(drop.item, drop.dropChance, this);
            }
        }
    }
    #endregion

    #region Effects Tab
    private void PopulateEffects()
    {
        if (database == null || database.allEffects == null || effectSlotPrefab == null) return;

        ItemEffect_DataSO firstEffect = null;
        foreach (var effect in database.allEffects)
        {
            if (effect == null) continue;
            if (firstEffect == null) firstEffect = effect;

            var slot = Instantiate(effectSlotPrefab, listContentParent);
            slot.Setup(effect, this);
        }

        if (firstEffect != null)
        {
            ShowEffectDetails(firstEffect);
        }
    }

    public void ShowEffectDetails(ItemEffect_DataSO effect)
    {
        if (effect == null) return;

        HideAllDetailPanels();
        if (effectDetailPanel != null) effectDetailPanel.SetActive(true);

        if (effectNameText != null)
            effectNameText.text = UI_Codex_EffectSlot.FormatEffectName(effect.name);

        if (effectDescText != null)
        {
            if (!string.IsNullOrEmpty(effect.effectDescription))
                effectDescText.text = effect.effectDescription;
            else
                effectDescText.text = "Hiệu ứng nội tại đặc biệt khi trang bị hoặc kích hoạt vật phẩm.";
        }

        if (effectItemsCountText != null)
        {
            int count = database.allItems.Count(i => i.itemEffect == effect);
            effectItemsCountText.text = $"Số trang bị sở hữu hiệu ứng này: {count}";
        }
    }
    #endregion
}
