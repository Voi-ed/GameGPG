using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Codex_MonsterSlot : MonoBehaviour
{
    [SerializeField] private Image monsterIcon;
    [SerializeField] private Image rankBadge;
    [SerializeField] private TextMeshProUGUI monsterNameText;
    [SerializeField] private TextMeshProUGUI monsterRankText;
    [SerializeField] private Button button;

    private Codex_MonsterDataSO monsterData;
    private UI_Codex codex;

    public void Setup(Codex_MonsterDataSO monster, UI_Codex codexUI)
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
        }

        if (monsterRankText != null)
        {
            monsterRankText.text = monsterData.monsterRank.ToString();
            monsterRankText.color = GetRankColor(monsterData.monsterRank);
        }

        if (rankBadge != null)
        {
            rankBadge.color = GetRankColor(monsterData.monsterRank);
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (codex != null && monsterData != null)
        {
            codex.ShowMonsterDetails(monsterData);
        }
    }

    public static Color GetRankColor(MonsterRank rank)
    {
        switch (rank)
        {
            case MonsterRank.Normal: return new Color(0.8f, 0.8f, 0.8f);
            case MonsterRank.Elite: return new Color(1f, 0.4f, 0.2f);
            case MonsterRank.Boss: return new Color(0.9f, 0.15f, 0.2f);
            default: return Color.white;
        }
    }
}

