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

        string fullStackPrice = ($"Price:{price}x{itemToShow.stackSize} - {totalPrice}g.");
        string singleStackPrice = ($"Price:{price}g.");

        
        itemPrice.text = itemToShow.stackSize > 1 ? fullStackPrice : singleStackPrice;
        itemType.text = itemToShow.itemData.itemType.ToString();
        itemInfo.text = itemToShow.GetItemInfo();

        string color = GetColorByRarity(itemToShow.itemData.itemRarity);
        itemName.text = GetColoredText(color,itemToShow.itemData.itemName);
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
