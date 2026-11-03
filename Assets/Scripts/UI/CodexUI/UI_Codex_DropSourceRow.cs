using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Codex_DropSourceRow : MonoBehaviour
{
    [SerializeField] private Image monsterIcon;
    [SerializeField] private TextMeshProUGUI monsterNameText;
    [SerializeField] private TextMeshProUGUI mapLocationText;
    [SerializeField] private TextMeshProUGUI dropChanceText;
    [SerializeField] private Button jumpButton;

    private Codex_MonsterDataSO monsterData;
    private UI_Codex codex;

    public void Setup(Codex_MonsterDataSO monster, float chance, UI_Codex codexUI)
    {
        monsterData = monster;
        codex = codexUI;

        if (monsterData == null) return;

        if (monsterIcon != null)
        {
            monsterIcon.sprite = monsterData.monsterIcon;
            monsterIcon.enabled = (monsterData.monsterIcon != null);
        }

        if (monsterNameText != null)
        {
            monsterNameText.text = monsterData.monsterName;
            monsterNameText.color = UI_Codex_MonsterSlot.GetRankColor(monsterData.monsterRank);
        }

        if (mapLocationText != null)
        {
            mapLocationText.text = monsterData.mapLocation;
        }

        if (dropChanceText != null)
        {
            dropChanceText.text = $"{chance:F1}%";
        }

        if (jumpButton != null)
        {
            jumpButton.onClick.RemoveAllListeners();
            jumpButton.onClick.AddListener(OnJumpClick);
        }
    }

    private void OnJumpClick()
    {
        if (codex != null && monsterData != null)
        {
            codex.JumpToMonster(monsterData);
        }
    }
}
