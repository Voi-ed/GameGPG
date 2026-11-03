using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Player : Inventory_Base
{
    public event Action<int> OnQuickSlotUsed;

    public Inventory_Storage storage { get ; private set; }
    public List<Inventory_EquipmentSlot> equipList;

    [Header("Quick Item Slots")]
    public Inventory_Item[] quickItems = new Inventory_Item[2];

    [Header("Gold Info")]
    public int gold = 10000;

    protected override void Awake()
    {
        base.Awake();
        storage = FindFirstObjectByType<Inventory_Storage>();
    }

    public void SetQuickItemInSlot(int slotNumber, Inventory_Item itemToSet)
    {
        quickItems[slotNumber - 1] = itemToSet;
        TriggerUpdateUI();
    }

    public void TryUseQuickItemInSlot(int passedSlotNumber)
    {
        int slotNumber = passedSlotNumber - 1;
        var itemToUse = quickItems[slotNumber];

        if (itemToUse == null)
            return;

        TryUseItem(itemToUse);

        if (FindItem(itemToUse) == null)
        {
            quickItems[slotNumber] = FindSameItem(itemToUse);
        }

        TriggerUpdateUI();
        OnQuickSlotUsed?.Invoke(slotNumber);
    }

    public void TryEquipItem(Inventory_Item item)
    {
        var inventoryItem = FindItem(item);
        var matchingSlots = equipList.FindAll(slot => slot.slotType == item.itemData.itemType);

        // STEP 1 : Try to find empty slot and equip item
        foreach (var slot in matchingSlots)
        {
            if (slot.HasItem() == false)
            {
                EquipItem(inventoryItem, slot);
                return;
            }
        }

        // STEP 2: No empty slots ? Replace first one
        var slotToReplace = matchingSlots[0];
        var itemToUneqip = slotToReplace.equipedItem;

        UnequipItem(itemToUneqip,slotToReplace != null);
        EquipItem(inventoryItem, slotToReplace);
    }

    private void EquipItem(Inventory_Item itemToEquip, Inventory_EquipmentSlot slot)
    {
        float savaedHealthPercent = player.health.GetHealthPercent();

        slot.equipedItem = itemToEquip;
        slot.equipedItem.AddModifiers(player.stats);
        slot.equipedItem.AddItemEffect(player);

        UpdateSetBonuses();

        player.health.SetHealthToPercent(savaedHealthPercent);
        RemoveOneItem(itemToEquip);
    }
    public void UnequipItem(Inventory_Item itemToUnequip,bool replacingItem = false)
    {
        if (CanAddItem(itemToUnequip) == false && replacingItem  == false)
        {
            Debug.Log("No space!");
            return;
        }

        float savedHealthPercent = player.health.GetHealthPercent();
        var slotToUnequip = equipList.Find(slot => slot.equipedItem == itemToUnequip);

        if(slotToUnequip != null)
            slotToUnequip.equipedItem = null;

        itemToUnequip.RemoveModifiers(player.stats);
        itemToUnequip.RemoveItemEffect();

        UpdateSetBonuses();

        player.health.SetHealthToPercent(savedHealthPercent);
        AddItem(itemToUnequip);
    }

    public void UnequipAndDiscardItem(Inventory_EquipmentSlot slot)
    {
        if (slot == null || slot.HasItem() == false)
            return;

        var itemToDiscard = slot.equipedItem;
        slot.equipedItem = null;

        itemToDiscard.RemoveModifiers(player.stats);
        itemToDiscard.RemoveItemEffect();

        UpdateSetBonuses();
        TriggerUpdateUI();
    }

    private Dictionary<ItemSetDataSO, int> activeSetCounts = new Dictionary<ItemSetDataSO, int>();
    private List<ItemEffect_DataSO> activeSetEffects = new List<ItemEffect_DataSO>();

    private void UpdateSetBonuses()
    {
        Dictionary<ItemSetDataSO, int> currentCounts = new Dictionary<ItemSetDataSO, int>();

        foreach (var slot in equipList)
        {
            if (slot.HasItem() && slot.equipedItem.itemData is EquipmentDataSO equipData && equipData.itemSet != null)
            {
                ItemSetDataSO set = equipData.itemSet;
                if (!currentCounts.ContainsKey(set))
                    currentCounts[set] = 0;
                currentCounts[set]++;
            }
        }

        foreach (var kvp in activeSetCounts)
        {
            ItemSetDataSO set = kvp.Key;
            int oldCount = kvp.Value;
            int newCount = currentCounts.ContainsKey(set) ? currentCounts[set] : 0;

            if (oldCount >= 2 && newCount < 2)
            {
                RemoveSetModifiers(set.twoPieceModifiers, $"SetBonus_{set.name}_2Piece");
            }

            if (oldCount >= 4 && newCount < 4)
            {
                RemoveSetModifiers(set.fourPieceModifiers, $"SetBonus_{set.name}_4Piece");
                if (set.fourPieceEffect != null && activeSetEffects.Contains(set.fourPieceEffect))
                {
                    set.fourPieceEffect.Unsubscribe();
                    activeSetEffects.Remove(set.fourPieceEffect);
                }
            }
        }

        foreach (var kvp in currentCounts)
        {
            ItemSetDataSO set = kvp.Key;
            int newCount = kvp.Value;
            int oldCount = activeSetCounts.ContainsKey(set) ? activeSetCounts[set] : 0;

            if (newCount >= 2 && oldCount < 2)
            {
                ApplySetModifiers(set.twoPieceModifiers, $"SetBonus_{set.name}_2Piece");
            }

            if (newCount >= 4 && oldCount < 4)
            {
                ApplySetModifiers(set.fourPieceModifiers, $"SetBonus_{set.name}_4Piece");
                if (set.fourPieceEffect != null && !activeSetEffects.Contains(set.fourPieceEffect))
                {
                    set.fourPieceEffect.BindSource(player, null, set);
                    activeSetEffects.Add(set.fourPieceEffect);
                }
            }
        }

        activeSetCounts = currentCounts;
    }

    private void ApplySetModifiers(ItemModifier[] modifiers, string source)
    {
        if (modifiers == null) return;
        foreach (var mod in modifiers)
        {
            Stat stat = player.stats.GetStatByType(mod.statType);
            stat?.AddModifier(mod.value, source);
        }
    }

    private void RemoveSetModifiers(ItemModifier[] modifiers, string source)
    {
        if (modifiers == null) return;
        foreach (var mod in modifiers)
        {
            Stat stat = player.stats.GetStatByType(mod.statType);
            stat?.RemoveModifier(source);
        }
    }

    public override void SaveData(ref GameData data)
    {
        data.gold = gold;
        data.inventory.Clear();
        data.equipedItems.Clear();

        foreach (var item in itemList)
        {
            if (item != null && item.itemData != null)
            {
                string saveId = item.itemData.saveId;
                

                if (data.inventory.ContainsKey(saveId) == false)
                    data.inventory[saveId] = 0;

                data.inventory[saveId] += item.stackSize;
            }
        }

        foreach (var slot in equipList)
        {
            if (slot.HasItem())
                data.equipedItems[slot.equipedItem.itemData.saveId] = slot.slotType;
        }

        if (data.quickItemSaveIds == null)
            data.quickItemSaveIds = new List<string>();
        data.quickItemSaveIds.Clear();

        if (quickItems != null)
        {
            for (int i = 0; i < quickItems.Length; i++)
            {
                if (quickItems[i] != null && quickItems[i].itemData != null)
                    data.quickItemSaveIds.Add(quickItems[i].itemData.saveId);
                else
                    data.quickItemSaveIds.Add(string.Empty);
            }
        }
    }

    public override void LoadData(GameData data)
    {
        gold = data.gold;   

        // 1. Dọn sạch túi đồ trước khi nạp lại từ save để tránh nhân đôi đồ khi đổi scene
        itemList.Clear();

        if (data.inventory != null)
        {
            foreach (var entry in data.inventory)
            {
                string saveId = entry.Key;
                int stackSize = entry.Value;

                ItemDataSO itemData = itemDataBase != null ? itemDataBase.GetItemData(saveId) : null;

                if (itemData == null)
                {
                    Debug.LogWarning("Item not found: " + saveId);
                    continue;
                }

                for (int i = 0; i < stackSize; i++)
                {
                    Inventory_Item itemToLoad = new Inventory_Item(itemData);
                    AddItem(itemToLoad);    
                }
            }
        }

        // 2. Dọn sạch và gỡ bỏ Modifiers của toàn bộ trang bị cũ đang mang trên người
        if (equipList != null)
        {
            foreach (var slot in equipList)
            {
                if (slot != null && slot.equipedItem != null)
                {
                    if (player != null && player.stats != null)
                        slot.equipedItem.RemoveModifiers(player.stats);
                    slot.equipedItem.RemoveItemEffect();
                    slot.equipedItem = null;
                }
            }
        }

        // 3. Nạp lại các trang bị đã lưu từ save data với đầy đủ kiểm tra null-safety
        if (data.equipedItems != null && equipList != null)
        {
            foreach (var entry in data.equipedItems)
            {
                string saveId = entry.Key;
                ItemType equipemntSlotType = entry.Value;

                if (string.IsNullOrEmpty(saveId))
                    continue;

                ItemDataSO itemData = itemDataBase != null ? itemDataBase.GetItemData(saveId) : null;
                if (itemData == null)
                {
                    Debug.LogWarning("[Inventory_Player] Không tìm thấy dữ liệu trang bị cho saveId: " + saveId);
                    continue;
                }

                Inventory_Item itemToLoad = new Inventory_Item(itemData);

                var slot = equipList.Find(s => s != null && s.slotType == equipemntSlotType && s.HasItem() == false);
                if (slot == null)
                {
                    Debug.LogWarning($"[Inventory_Player] Không tìm thấy slot trống phù hợp cho {itemData.itemName} ({equipemntSlotType})!");
                    continue;
                }

                slot.equipedItem = itemToLoad;
                if (player != null && player.stats != null)
                    slot.equipedItem.AddModifiers(player.stats);
                if (player != null)
                    slot.equipedItem.AddItemEffect(player);
            }
        }

        // Nạp và đồng bộ lại các ô Quick Item Slots
        if (data.quickItemSaveIds != null && quickItems != null)
        {
            for (int i = 0; i < data.quickItemSaveIds.Count && i < quickItems.Length; i++)
            {
                string saveId = data.quickItemSaveIds[i];
                if (!string.IsNullOrEmpty(saveId))
                {
                    quickItems[i] = itemList.Find(it => it != null && it.itemData != null && it.itemData.saveId == saveId);
                }
                else
                {
                    quickItems[i] = null;
                }
            }
        }

        UpdateSetBonuses();
        TriggerUpdateUI();
    }
}
