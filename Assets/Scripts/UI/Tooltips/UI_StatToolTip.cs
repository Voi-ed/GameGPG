using TMPro;
using UnityEngine;

public class UI_StatToolTip : UI_ToolTip
{
    private Player_Stats playerStats;
    private TextMeshProUGUI statToolTipText;

    protected override void Awake()
    {
        base.Awake();
        playerStats = FindFirstObjectByType<Player_Stats>();
        statToolTipText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void ShowToolTip(bool show, RectTransform targetRect,StatType statType)
    {
        base.ShowToolTip(show, targetRect);
        statToolTipText.text = GetStatTextByType(statType);
    }

    public string GetStatTextByType(StatType type)
    {
        switch (type)
        {
            // Major Attributes
            case StatType.Strength:
                return "Tăng 1 sát thương vật lý mỗi điểm." +
                       "\nTăng 0.5% sát thương chí mạng mỗi điểm.";
            case StatType.Agility:
                return "Tăng 0.3% tỷ lệ chí mạng mỗi điểm." +
                       "\nTăng 0.5% né tránh mỗi điểm.";
            case StatType.Intelligence:
                return "Tăng 0.5% kháng nguyên tố mỗi điểm." +
                       "\nThêm 1 sát thương nguyên tố mỗi điểm thưởng." +
                       "\nNếu tất cả nguyên tố đều bằng 0, điểm thưởng sẽ không được áp dụng.";
            case StatType.Vitality:
                return "Tăng 5 máu tối đa mỗi điểm." +
                       "\nTăng 1 giáp mỗi điểm.";

            // Physical Damage
            case StatType.Damage:
                return "Quyết định lượng sát thương vật lý trong các đòn đánh của bạn.";
            case StatType.CritChance:
                return "Tỷ lệ đòn đánh gây sát thương chí mạng.";
            case StatType.CritPower:
                return "Tăng lượng sát thương gây ra từ các đòn đánh chí mạng.";
            case StatType.ArmorReduction:
                return "Phần trăm giáp của mục tiêu sẽ bị bỏ qua khi bạn tấn công.";
            case StatType.AttackSpeed:
                return "Quyết định tốc độ vung đòn và tấn công của bạn.";

            // Defense
            case StatType.MaxHealth:
                return "Tổng lượng sinh lực tối đa của nhân vật.";
            case StatType.HealthRegen:
                return "Lượng máu được hồi phục mỗi giây.";
            case StatType.Armor:
                return "Giảm sát thương vật lý nhận vào."
                    + "\nGiảm thiểu tối đa 85%."
                    + "\nGiảm thiểu hiện tại: " + playerStats.GetArmorMitigation(0) * 100 + "%.";
            case StatType.Evasion:
                return "Tỷ lệ né tránh hoàn toàn đòn tấn công." + "\nTối đa 85%.";

            // Elemental Damage
            case StatType.IceDamage:
                return "Quyết định lượng sát thương Băng trong đòn đánh.";
            case StatType.FireDamage:
                return "Quyết định lượng sát thương Lửa trong đòn đánh.";
            case StatType.LightningDamage:
                return "Quyết định lượng sát thương Sét trong đòn đánh.";
            case StatType.ElementalDamage:
                return
                    "Sát thương nguyên tố kết hợp cả ba hệ Lửa, Băng và Sét." +
                    "\nNguyên tố có chỉ số cao nhất sẽ áp dụng hiệu ứng và 100% sát thương." +
                    "\nHai nguyên tố còn lại đóng góp 50% sát thương như sát thương cộng thêm.";

            // Elemental Resistances
            case StatType.IceResistance:
                return "Giảm sát thương Băng phải gánh chịu.";
            case StatType.FireResistance:
                return "Giảm sát thương Lửa phải gánh chịu.";
            case StatType.LightningResistance:
                return "Giảm sát thương Sét phải gánh chịu.";

            default:
                return "Chưa có thông tin mô tả cho chỉ số này.";
        }
    }
}
