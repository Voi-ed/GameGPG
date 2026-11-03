using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Codex_EffectSlot : MonoBehaviour
{
    [SerializeField] private Image effectIcon;
    [SerializeField] private TextMeshProUGUI effectNameText;
    [SerializeField] private Button button;

    private ItemEffect_DataSO effectData;
    private UI_Codex codex;

    public void Setup(ItemEffect_DataSO effect, UI_Codex codexUI)
    {
        effectData = effect;
        codex = codexUI;

        if (effectData == null) return;

        if (effectNameText != null)
        {
            effectNameText.text = FormatEffectName(effectData.name);
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (codex != null && effectData != null)
        {
            codex.ShowEffectDetails(effectData);
        }
    }

    public static string FormatEffectName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName)) return "";

        string name = rawName;
        string[] prefixes = { "Item effect data - ", "Item Effect Data - ", "ItemEffectData - ", "ItemEffect_", "ItemEffect - ", "Effect - ", "Effect_" };
        foreach (var p in prefixes)
        {
            if (name.StartsWith(p, System.StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(p.Length);
                break;
            }
        }

        // Chèn khoảng trắng trước các chữ cái hoa nếu là camelCase/PascalCase
        if (!name.Contains(" "))
        {
            name = System.Text.RegularExpressions.Regex.Replace(name, "(\\B[A-Z])", " $1");
        }

        return name.Trim();
    }
}

