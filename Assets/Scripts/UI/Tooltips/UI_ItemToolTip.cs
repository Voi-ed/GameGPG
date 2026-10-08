using System.Text;
using TMPro;
using UnityEngine;

public class UI_ItemToolTip : UI_ToolTip
{
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemType;
    [SerializeField] private TextMeshProUGUI itemInfo;

    [SerializeField] private TextMeshProUGUI itemPrice;
    [SerializeField] private Transform merchnatInfo;
    [SerializeField] private Transform inventoryInfo;

    public void ShowToolTip(bool show, RectTransform targetRect,Inventory_Item itemToShow,bool buyPrice = false,bool showMerchantInfo = false,bool showControls = true)
    {
        base.ShowToolTip(show, targetRect);

        if (showControls)
        {
            merchnatInfo.gameObject.SetActive(showMerchantInfo);
            inventoryInfo.gameObject.SetActive(!showMerchantInfo);
        }
        else
        {
            merchnatInfo.gameObject.SetActive(false);
            inventoryInfo.gameObject.SetActive(false);
        }

        int price = buyPrice ? itemToShow.buyPrice : Mathf.FloorToInt(itemToShow.sellPrice);
        int totalPrice = price * itemToShow.stackSize;

        string fullStackPrice = ($"Giá: {price} x {itemToShow.stackSize} - {totalPrice} vàng");
        string singleStackPrice = ($"Giá: {price} vàng");

        itemPrice.text = itemToShow.stackSize > 1 ? fullStackPrice : singleStackPrice;
        itemType.text = GetItemTypeName(itemToShow.itemData.itemType);
        itemInfo.text = itemToShow.GetItemInfo();

        string color = GetColorByRarity(itemToShow.itemData.itemRarity);
        itemName.text = GetColoredText(color, itemToShow.itemData.itemName);
    }

    private string GetItemTypeName(ItemType type)
    {
        switch (type)
        {
            case ItemType.Material: return "Nguyên liệu";
            case ItemType.Weapon: return "Vũ khí";
            case ItemType.Armor: return "Áo giáp";
            case ItemType.Trinket: return "Phụ kiện";
            case ItemType.Consumable: return "Tiêu hao";
            default: return type.ToString();
        }
    }

    private string GetColorByRarity(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "white";
            case ItemRarity.Uncommon: return "green";
            case ItemRarity.Rare: return "blue";
            case ItemRarity.Epic: return "purple";
            case ItemRarity.Legendary: return "orange";
            default: return "white";
        }
    }
}
