using System;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item Set", fileName = "ItemSet - ")]
public class ItemSetDataSO : ScriptableObject
{
    public string setName;

    [Header("Set VFX Theme")]
    [Tooltip("If true, all items and bonus effects belonging to this set will inherit this theme color.")]
    public bool overrideVfxColor = false;
    [Tooltip("Theme color applied to VFX when this set is active.")]
    public Color setVfxColor = Color.white;

    [Header("2-Piece Set Bonus")]
    public ItemModifier[] twoPieceModifiers;

    [Header("4-Piece Full Set Bonus")]
    public ItemModifier[] fourPieceModifiers;
    public ItemEffect_DataSO fourPieceEffect;
}
