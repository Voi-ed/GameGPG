using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class MonsterDropSource
{
    public Codex_MonsterDataSO monster;
    public float dropChance;
}

[Serializable]
public class ItemDropDetail
{
    public ItemDataSO item;
    public float dropChance;
}

[CreateAssetMenu(fileName = "Codex Database", menuName = "RPG Setup/Codex/Codex Database")]
public class Codex_DatabaseSO : ScriptableObject
{
    [Header("All Registered Entries")]
    public ItemDataSO[] allItems;
    public Codex_MonsterDataSO[] allMonsters;
    public ItemEffect_DataSO[] allEffects;

    // Runtime Cached Lookup: Item -> Monsters that drop it
    private Dictionary<ItemDataSO, List<MonsterDropSource>> itemDropLookup;
    // Runtime Cached Lookup: Monster -> Drop entries
    private Dictionary<Codex_MonsterDataSO, List<ItemDropDetail>> monsterDropLookup;

    public void Initialize()
    {
        BuildCrossReferences();
    }

    public void BuildCrossReferences()
    {
        itemDropLookup = new Dictionary<ItemDataSO, List<MonsterDropSource>>();
        monsterDropLookup = new Dictionary<Codex_MonsterDataSO, List<ItemDropDetail>>();

        if (allMonsters == null) return;

        foreach (var monster in allMonsters)
        {
            if (monster == null) continue;

            List<ItemDropDetail> dropsForThisMonster = new List<ItemDropDetail>();

            if (monster.dropData != null && monster.dropData.itemList != null)
            {
                foreach (var item in monster.dropData.itemList)
                {
                    if (item == null) continue;

                    float chance = item.GetDropChance();

                    dropsForThisMonster.Add(new ItemDropDetail
                    {
                        item = item,
                        dropChance = chance
                    });

                    if (!itemDropLookup.TryGetValue(item, out var sources))
                    {
                        sources = new List<MonsterDropSource>();
                        itemDropLookup[item] = sources;
                    }

                    sources.Add(new MonsterDropSource
                    {
                        monster = monster,
                        dropChance = chance
                    });
                }
            }

            monsterDropLookup[monster] = dropsForThisMonster;
        }
    }

    public List<MonsterDropSource> GetSourcesForItem(ItemDataSO item)
    {
        if (itemDropLookup == null) Initialize();
        if (item != null && itemDropLookup.TryGetValue(item, out var sources))
            return sources;
        return new List<MonsterDropSource>();
    }

    public List<ItemDropDetail> GetDropsForMonster(Codex_MonsterDataSO monster)
    {
        if (monsterDropLookup == null) Initialize();
        if (monster != null && monsterDropLookup.TryGetValue(monster, out var drops))
            return drops;
        return new List<ItemDropDetail>();
    }

#if UNITY_EDITOR
    [ContextMenu("Auto Collect All Items, Monsters and Effects")]
    public void CollectDatabase()
    {
        // 1. Items
        string[] itemGuids = AssetDatabase.FindAssets("t:ItemDataSO");
        allItems = itemGuids
            .Select(g => AssetDatabase.LoadAssetAtPath<ItemDataSO>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(i => i != null)
            .OrderBy(i => i.itemType)
            .ThenByDescending(i => (int)i.itemRarity)
            .ToArray();

        // 2. Monsters
        string[] monsterGuids = AssetDatabase.FindAssets("t:Codex_MonsterDataSO");
        allMonsters = monsterGuids
            .Select(g => AssetDatabase.LoadAssetAtPath<Codex_MonsterDataSO>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(m => m != null)
            .OrderByDescending(m => (int)m.monsterRank)
            .ThenBy(m => m.monsterName)
            .ToArray();

        // 3. Effects
        string[] effectGuids = AssetDatabase.FindAssets("t:ItemEffect_DataSO");
        allEffects = effectGuids
            .Select(g => AssetDatabase.LoadAssetAtPath<ItemEffect_DataSO>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(e => e != null)
            .OrderBy(e => e.name)
            .ToArray();

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        Debug.Log($"<color=cyan>[Codex_DatabaseSO]</color> Đã nạp thành công: {allItems.Length} Items, {allMonsters.Length} Monsters, {allEffects.Length} Effects!");
    }
#endif
}

