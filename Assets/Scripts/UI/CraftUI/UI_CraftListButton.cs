using UnityEngine;

public class UI_CraftListButton : MonoBehaviour
{
    [SerializeField] private ItemListDataSO craftData;
    private UI_CraftSlot[] craftSlots;


    public void SetCraftSlots(UI_CraftSlot[] craftSlots) => this.craftSlots = craftSlots;

    public void UpdateCraftSlots()
    {
        if (craftData == null)
        {
            Debug.Log("You need to assign craft list data!");
            return;
        }

        if (craftSlots == null)
            return;

        foreach (var slot in craftSlots)
        {
            slot.gameObject.SetActive(false);
        }

        int count = Mathf.Min(craftData.itemList.Length, craftSlots.Length);
        for (int i = 0; i < count; i++)
        {
            ItemDataSO itemData = craftData.itemList[i];

            if (itemData != null)
            {
                craftSlots[i].gameObject.SetActive(true);
                craftSlots[i].SetupButton(itemData);
            }
        }
    }
}
