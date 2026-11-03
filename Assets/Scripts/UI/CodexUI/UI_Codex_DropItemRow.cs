using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Codex_DropItemRow : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image rarityBorder;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI dropChanceText;
    [SerializeField] private Button jumpButton;

    private ItemDataSO itemData;
    private UI_Codex codex;

    public void Setup(ItemDataSO item, float chance, UI_Codex codexUI)
    {
        itemData = item;
        codex = codexUI;

        if (itemData == null) return;

        if (itemIcon != null)
        {
            itemIcon.sprite = itemData.itemIcon;
            itemIcon.enabled = (itemData.itemIcon != null);
        }

        Color rarityColor = UI_Codex_ItemSlot.GetRarityColor(itemData.itemRarity);

        if (itemNameText != null)
        {
            itemNameText.text = itemData.itemName;
            itemNameText.color = rarityColor;
        }

        if (rarityBorder != null)
        {
            rarityBorder.color = rarityColor;
        }

        if (itemTypeText != null)
        {
            itemTypeText.text = itemData.itemType.ToString();
        }

        if (dropChanceText != null)
        {
            dropChanceText.text = $"{chance:F1}%";
        }

        if (jumpButton != null)
        {
            jumpButton.onClick.RemoveAllListeners();
            jumpButton.onClick.AddListener(OnJumpClick);
        }
    }

    private void OnJumpClick()
    {
        if (codex != null && itemData != null)
        {
            codex.JumpToItem(itemData);
        }
    }
}
