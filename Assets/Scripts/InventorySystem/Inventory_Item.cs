using System;
using System.Text;
using UnityEngine;

[Serializable]
public class Inventory_Item 
{
    private string itemId;

    public ItemDataSO itemData;
    public int stackSize = 1;

    public ItemModifier[] modifiers { get; private set; }
    public ItemEffect_DataSO itemEffect;

    public int buyPrice { get; private set; }
    public float sellPrice { get; private set; }


    public Inventory_Item(ItemDataSO itemData)
    {
        this.itemData = itemData;
        itemEffect = itemData.itemEffect;
        buyPrice = itemData.itemPrice;
        sellPrice = itemData.itemPrice * .35f;

        modifiers = EquipmentData()?.modifiers;
        itemId = itemData.itemName + " - " + Guid.NewGuid();
    }

    public void AddModifiers(Entity_Stats playerStats)
    {
        if (modifiers == null || playerStats == null) return;
        foreach (var mod in modifiers)
        {
            Stat statToModify = playerStats.GetStatByType(mod.statType);
            statToModify?.AddModifier(mod.value, itemId);
        }
    }

    public void RemoveModifiers(Entity_Stats playerStats)
    {
        if (modifiers == null || playerStats == null) return;
        foreach (var mod in modifiers)
        {
            Stat statToModify = playerStats.GetStatByType(mod.statType);
            statToModify?.RemoveModifier(itemId);
        }
    }

    public void AddItemEffect(Player player)
    {
        if (itemEffect != null)
        {
            EquipmentDataSO equip = itemData as EquipmentDataSO;
            itemEffect.BindSource(player, itemData, equip != null ? equip.itemSet : null);
        }
    }
    public void RemoveItemEffect() => itemEffect?.Unsubscribe();

    private EquipmentDataSO EquipmentData()
    {
        if(itemData is EquipmentDataSO equipment)
            return equipment;

        return null;
    }

    public bool CanAddStack() => stackSize < itemData.maxStackSize;
    public void AddStack() => stackSize++;
    public void RemoveStack() => stackSize--;

    public string GetItemInfo()
    {
        StringBuilder sb = new StringBuilder();
        
        if (itemData.itemType == ItemType.Material)
        {
            sb.AppendLine("");
            sb.AppendLine("Dùng để chế tạo trang bị");
            sb.AppendLine("");
            sb.AppendLine("");
            return sb.ToString();
        }

        if (itemData.itemType == ItemType.Consumable)
        {
            sb.AppendLine("");
            sb.AppendLine(itemEffect != null ? itemEffect.effectDescription : "");
            sb.AppendLine("");
            sb.AppendLine("");
            return sb.ToString();
        }

        sb.AppendLine("");

        EquipmentDataSO equip = EquipmentData();
        if (equip != null && equip.itemSet != null)
        {
            sb.AppendLine($"<color=#FFD700>Bộ trang bị: {equip.itemSet.setName}</color>");
            if (equip.itemSet.twoPieceModifiers != null && equip.itemSet.twoPieceModifiers.Length > 0)
            {
                sb.AppendLine("<color=#A0A0A0>(2 món):</color>");
                foreach (var mod in equip.itemSet.twoPieceModifiers)
                {
                    string mType = GetStatNameByType(mod.statType);
                    string mVal = IsPercentageStat(mod.statType) ? (mod.value <= 1f ? (mod.value * 100f).ToString("0.#") : mod.value.ToString()) + "%" : mod.value.ToString();
                    sb.AppendLine($"  <color=#55FF55>• +{mVal} {mType}</color>");
                }
            }
            bool hasFourPieceMods = equip.itemSet.fourPieceModifiers != null && equip.itemSet.fourPieceModifiers.Length > 0;
            bool hasFourPieceEffect = equip.itemSet.fourPieceEffect != null;

            if (hasFourPieceMods || hasFourPieceEffect)
            {
                sb.AppendLine("<color=#A0A0A0>(4 món):</color>");
                if (hasFourPieceMods)
                {
                    foreach (var mod in equip.itemSet.fourPieceModifiers)
                    {
                        string mType = GetStatNameByType(mod.statType);
                        string mVal = IsPercentageStat(mod.statType) ? (mod.value <= 1f ? (mod.value * 100f).ToString("0.#") : mod.value.ToString()) + "%" : mod.value.ToString();
                        sb.AppendLine($"  <color=#55FF55>• +{mVal} {mType}</color>");
                    }
                }

                if (hasFourPieceEffect && !string.IsNullOrEmpty(equip.itemSet.fourPieceEffect.effectDescription))
                {
                    sb.AppendLine($"  <color=#FFAA00>• {equip.itemSet.fourPieceEffect.effectDescription}</color>");
                }
            }
            sb.AppendLine("");
        }

        foreach (var mod in modifiers)
        {
            string modType = GetStatNameByType(mod.statType);
            string modValue = IsPercentageStat(mod.statType) ? (mod.value <= 1f ? (mod.value * 100f).ToString("0.#") : mod.value.ToString()) + "%" : mod.value.ToString();
            sb.AppendLine("+ " + modValue + " " + modType);
        }

        if (itemEffect != null)
        {
            sb.AppendLine("");
            sb.AppendLine("Hiệu ứng đặc biệt:");
            sb.AppendLine(itemEffect.effectDescription);
        }

        sb.AppendLine("");
        sb.AppendLine("");

        return sb.ToString();
    }

    private string GetStatNameByType(StatType type)
    {
        switch (type)
        {
            case StatType.MaxHealth: return "Máu tối đa";
            case StatType.HealthRegen: return "Hồi phục máu";
            case StatType.Armor: return "Giáp";
            case StatType.Evasion: return "Né tránh";

            case StatType.Strength: return "Sức mạnh";
            case StatType.Agility: return "Nhanh nhẹn";
            case StatType.Intelligence: return "Trí tuệ";
            case StatType.Vitality: return "Thể lực";

            case StatType.AttackSpeed: return "Tốc độ đánh";
            case StatType.Damage: return "Sát thương";
            case StatType.CritChance: return "Tỷ lệ chí mạng";
            case StatType.CritPower: return "Sát thương chí mạng";
            case StatType.ArmorReduction: return "Xuyên giáp";

            case StatType.FireDamage: return "Sát thương Lửa";
            case StatType.IceDamage: return "Sát thương Băng";
            case StatType.LightningDamage: return "Sát thương Sét";

            case StatType.IceResistance: return "Kháng Băng";
            case StatType.FireResistance: return "Kháng Lửa";
            case StatType.LightningResistance: return "Kháng Sét";
            default: return "Chỉ số không xác định";
        }
    }

    private bool IsPercentageStat(StatType type)
    {
        switch (type)
        {
            case StatType.CritChance:
            case StatType.CritPower:
            case StatType.ArmorReduction:
            case StatType.IceResistance:
            case StatType.FireResistance:
            case StatType.LightningResistance:
            case StatType.AttackSpeed:
            case StatType.Evasion:
                return true;
            default:
                return false;
        }
    }
}
