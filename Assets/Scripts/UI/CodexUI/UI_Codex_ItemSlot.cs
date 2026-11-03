using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Codex_ItemSlot : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image rarityBorder;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Button button;

    private ItemDataSO itemData;
    private UI_Codex codex;

    public void Setup(ItemDataSO item, UI_Codex codexUI)
    {
        itemData = item;
        codex = codexUI;

        if (itemData == null) return;

        if (itemIcon != null)
        {
            itemIcon.sprite = itemData.itemIcon;
            itemIcon.enabled = (itemData.itemIcon != null);
        }

        if (itemNameText != null)
        {
            itemNameText.text = itemData.itemName;
            itemNameText.color = GetRarityColor(itemData.itemRarity);
        }

        if (rarityBorder != null)
        {
            rarityBorder.color = GetRarityColor(itemData.itemRarity);
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (codex != null && itemData != null)
        {
            codex.ShowItemDetails(itemData);
        }
    }

    public static Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return new Color(0.85f, 0.85f, 0.85f);
            case ItemRarity.Uncommon: return new Color(0.2f, 0.85f, 0.3f);
            case ItemRarity.Rare: return new Color(0.2f, 0.6f, 1f);
            case ItemRarity.Epic: return new Color(0.75f, 0.3f, 1f);
            case ItemRarity.Legendary: return new Color(1f, 0.65f, 0.1f);
            default: return Color.white;
        }
    }
}

