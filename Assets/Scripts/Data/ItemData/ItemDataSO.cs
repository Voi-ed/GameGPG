using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Material item", fileName = "Material data - ")]
public class ItemDataSO : ScriptableObject
{
    public string saveId; 


    [Header("Merchant details")]
    [Range(0,10000)]
    public int itemPrice = 100;
    public int minStackSizeAtShop = 1;
    public int maxStackSizeAtShop = 1;

    [Header("Drop Details")]
    public ItemRarity itemRarity = ItemRarity.Common;
    [Range(0, 100)]
    public float dropChance;
    [Range(0, 100)]
    public float maxDropChance = 65f;


    [Header("Craft details")]
    public Inventory_Item[] craftRecipe;


    [Header("Item details")]
    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType;
    public int maxStackSize = 1;

    [Header("Item effect")]
    public ItemEffect_DataSO itemEffect;

    private void OnValidate()
    {
        if (!System.Enum.IsDefined(typeof(ItemRarity), itemRarity))
        {
            int val = (int)itemRarity;
            if (val <= 100) itemRarity = ItemRarity.Common;
            else if (val <= 300) itemRarity = ItemRarity.Uncommon;
            else if (val <= 600) itemRarity = ItemRarity.Rare;
            else if (val <= 850) itemRarity = ItemRarity.Epic;
            else itemRarity = ItemRarity.Legendary;
        }

        dropChance = GetDropChance();

#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(this);
        saveId = AssetDatabase.AssetPathToGUID(path);
#endif
    }

    public float GetDropChance()
    {
        float maxRarity = 1000;
        float chance = (maxRarity - (int)itemRarity + 1) / maxRarity * 100;

        return Mathf.Min(chance, maxDropChance);
    }


}