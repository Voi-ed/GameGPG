using System;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Equipment item", fileName = "Equipment data - ")]

public class EquipmentDataSO : ItemDataSO
{
    [Header("Item modifiers")]
    public ItemModifier[] modifiers;

    [Header("Item Set")]
    public ItemSetDataSO itemSet;
}

[Serializable]
public class ItemModifier
{
    public StatType statType;
    public float value;
}