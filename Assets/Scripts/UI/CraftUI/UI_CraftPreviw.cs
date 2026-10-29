using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CraftPreviw : MonoBehaviour
{
    private Inventory_Item itemToCraft;
    private Inventory_Storage storage;
    private List<UI_CraftPreviwSlot> craftPreviwSlots = new List<UI_CraftPreviwSlot>();
    private Transform slotsParent;
    private GameObject slotTemplate;

    [Header("Item Previw Setup")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemInfo;
    [SerializeField] private TextMeshProUGUI buttonText;

    private void Awake()
    {
        InitializeSlots();
    }

    public void SetupCraftPreviw(Inventory_Storage storage)
    {
        this.storage = storage;
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        if (craftPreviwSlots == null)
            craftPreviwSlots = new List<UI_CraftPreviwSlot>();

        craftPreviwSlots.Clear();

        // Lấy tất cả slot con (bao gồm cả các slot đang bị ẩn inactive)
        UI_CraftPreviwSlot[] foundSlots = GetComponentsInChildren<UI_CraftPreviwSlot>(true);
        if (foundSlots != null && foundSlots.Length > 0)
        {
            craftPreviwSlots.AddRange(foundSlots);
            slotsParent = foundSlots[0].transform.parent;
            slotTemplate = foundSlots[0].gameObject;
        }

        foreach (var slot in craftPreviwSlots)
        {
            if (slot != null)
                slot.gameObject.SetActive(false);
        }
    }

    public void ConfirmCraft()
    {
        if (itemToCraft == null)
        {
            if (buttonText != null)
                buttonText.text = "Chọn vật phẩm.";
            return;
        }

        if (storage != null && storage.CanCraftItem(itemToCraft))
            storage.CraftItem(itemToCraft);

        UpdateCraftPreviwSlots();
    }

    public void UpdateCraftPreviw(ItemDataSO itemData)
    {
        if (itemData == null)
            return;

        itemToCraft = new Inventory_Item(itemData);

        if (itemIcon != null)
            itemIcon.sprite = itemData.itemIcon;
        if (itemName != null)
            itemName.text = itemData.itemName;
        if (itemInfo != null)
            itemInfo.text = itemToCraft.GetItemInfo();

        UpdateCraftPreviwSlots();
    }

    private void UpdateCraftPreviwSlots()
    {
        if (itemToCraft == null || itemToCraft.itemData == null || itemToCraft.itemData.craftRecipe == null)
            return;

        if (craftPreviwSlots == null || craftPreviwSlots.Count == 0)
            InitializeSlots();

        foreach (var slot in craftPreviwSlots)
        {
            if (slot != null)
                slot.gameObject.SetActive(false);
        }

        int recipeCount = itemToCraft.itemData.craftRecipe.Length;

        // Nếu công thức cần nhiều nguyên liệu hơn số slot đang có trên UI: Tự động clone thêm slot con
        while (craftPreviwSlots.Count < recipeCount && slotTemplate != null && slotsParent != null)
        {
            GameObject newSlotObj = Instantiate(slotTemplate, slotsParent);
            newSlotObj.name = $"CraftPreviwSlot_{craftPreviwSlots.Count + 1}";
            UI_CraftPreviwSlot newSlot = newSlotObj.GetComponent<UI_CraftPreviwSlot>();
            if (newSlot != null)
            {
                craftPreviwSlots.Add(newSlot);
            }
            else
            {
                break;
            }
        }

        // Duyệt an toàn với độ dài nhỏ nhất, ngăn chặn hoàn toàn IndexOutOfRangeException
        int displayCount = Mathf.Min(recipeCount, craftPreviwSlots.Count);
        for (int i = 0; i < displayCount; i++)
        {
            Inventory_Item requiredItem = itemToCraft.itemData.craftRecipe[i];
            if (requiredItem == null || requiredItem.itemData == null)
                continue;

            int availableAmount = storage != null ? storage.GetAvailableAmountOf(requiredItem.itemData) : 0;
            int requiredAmount = requiredItem.stackSize;

            if (craftPreviwSlots[i] != null)
            {
                craftPreviwSlots[i].gameObject.SetActive(true);
                craftPreviwSlots[i].SetupPreviwSlot(requiredItem.itemData, availableAmount, requiredAmount);
            }
        }
    }
}
