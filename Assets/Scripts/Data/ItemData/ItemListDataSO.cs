using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


public enum ItemListFilterType
{
    All,
    Weapon,
    Armor,
    Trinket,
    Consumable,
    Material
}

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item list", fileName = "List of items - ")]
public class ItemListDataSO : ScriptableObject
{
    [Header("Filter Settings for Auto-Fill")]
    public ItemListFilterType filterType = ItemListFilterType.All;
    public bool craftableOnly = false;

    [Header("Item List")]
    public ItemDataSO[] itemList;

    public ItemDataSO GetItemData(string saveId)
    {
        return itemList.FirstOrDefault(item => item != null && item.saveId == saveId);
    }

#if UNITY_EDITOR
    [ContextMenu("Auto-fill with filtered ItemDataSO")]
    public void CollectItemsData()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemDataSO");

        var query = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<ItemDataSO>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(item => item != null);

        // Lọc theo loại vật phẩm
        if (filterType != ItemListFilterType.All)
        {
            ItemType targetType = (ItemType)System.Enum.Parse(typeof(ItemType), filterType.ToString());
            query = query.Where(item => item.itemType == targetType);
        }

        // Lọc theo món có công thức chế tạo
        if (craftableOnly)
        {
            query = query.Where(item => item.craftRecipe != null && item.craftRecipe.Length > 0);
        }

        itemList = query.ToArray();

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        Debug.Log($"[ItemListDataSO] Đã nạp {itemList.Length} items vào '{name}' (Loại: {filterType}, Chỉ đồ chế được: {craftableOnly})");
    }
#endif
}
