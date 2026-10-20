using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entity_DropManager : MonoBehaviour
{
    [SerializeField] private GameObject itemDropPrefab;
    [SerializeField] private ItemListDataSO dropData;

    [Header("Drop restrctions")]
    [SerializeField] private int maxRarityAmount = 1200;
    [SerializeField] private int maxItemsToDrop = 3;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
            DropItems();
    }

    public virtual void DropItems()
    {
        if (dropData == null)
        {
            Debug.Log("You need to assign drop data on entity" + gameObject.name);
            return;
        }

        List<ItemDataSO> itemsToDrop = RollDrops();
        int amountToDrop = Mathf.Min(itemsToDrop.Count, maxItemsToDrop);

        List<Collider2D> spawnedColliders = new List<Collider2D>();
        for (int i = 0; i < amountToDrop; i++)
        {
            GameObject newItem = CreateItemDrop(itemsToDrop[i]);
            if (newItem != null)
            {
                Collider2D newCol = newItem.GetComponent<Collider2D>();
                if (newCol != null)
                {
                    foreach (var oldCol in spawnedColliders)
                    {
                        if (oldCol != null)
                            Physics2D.IgnoreCollision(newCol, oldCol, true);
                    }
                    spawnedColliders.Add(newCol);
                }
            }
        }

    }

    public GameObject CreateItemDrop(ItemDataSO itemToDrop)
    {
        Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.25f, 0.25f), 0.5f, 0);
        GameObject newItem = Instantiate(itemDropPrefab, spawnPos, Quaternion.identity);
        newItem.GetComponent<Object_ItemPickup>().SetupItem(itemToDrop);
        return newItem;
    }

    public List<ItemDataSO> RollDrops()
    {
       

        List<ItemDataSO> possibleDrops = new List<ItemDataSO>();
        List<ItemDataSO> finalDrops = new List<ItemDataSO>();
        float maxRarityAmount = this.maxRarityAmount;

        // Step 1: Roll each item based on rarity and max drop chance
        foreach (var item in dropData.itemList)
        {
            float dropChance = item.GetDropChance();

            if(Random.Range(0,100) <= dropChance)
                possibleDrops.Add(item);
        }

        // Step 2: Sort by rarity (highest to lowest)
        possibleDrops = possibleDrops.OrderByDescending(item => item.itemRarity).ToList();

        // Step 3: Add items to final drop list until rarity limit on entity is reached

        foreach (var item in possibleDrops) 
        {                                   
            if (maxRarityAmount > (int)item.itemRarity)
            {
                finalDrops.Add(item);
                maxRarityAmount = maxRarityAmount - (int)item.itemRarity;
            }
        }

        return finalDrops;
    }
}
