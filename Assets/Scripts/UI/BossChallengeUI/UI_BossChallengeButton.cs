using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BossChallengeButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private TextMeshProUGUI bossSubtitleText;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private Button challengeButton;
    [SerializeField] private TextMeshProUGUI buttonLabelText;
    [SerializeField] private GameObject finalBossBadge;

    private string bossId;
    private Action<string> onChallengeAction;

    private void Awake()
    {
        if (challengeButton == null)
            challengeButton = GetComponent<Button>() ?? GetComponentInChildren<Button>(true);

        if (buttonLabelText == null && challengeButton != null)
            buttonLabelText = challengeButton.GetComponentInChildren<TextMeshProUGUI>(true);

        if (challengeButton != null)
        {
            challengeButton.onClick.RemoveAllListeners();
            challengeButton.onClick.AddListener(OnButtonClicked);
        }
    }

    public void Setup(
        string id,
        string displayName,
        string subtitle,
        string difficulty,
        bool isFinalBoss,
        bool isUnlocked,
        bool isDefeated,
        Action<string> onChallengeClick,
        TextMeshProUGUI nameExplicit = null,
        TextMeshProUGUI subtitleExplicit = null,
        TextMeshProUGUI diffExplicit = null,
        Button btnExplicit = null,
        TextMeshProUGUI btnLabelExplicit = null)
    {
        bossId = id;
        onChallengeAction = onChallengeClick;

        if (nameExplicit != null) bossNameText = nameExplicit;
        if (subtitleExplicit != null) bossSubtitleText = subtitleExplicit;
        if (diffExplicit != null) difficultyText = diffExplicit;
        if (btnExplicit != null) challengeButton = btnExplicit;
        if (btnLabelExplicit != null) buttonLabelText = btnLabelExplicit;

        if (bossNameText == null)
            bossNameText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (buttonLabelText == null && challengeButton != null)
            buttonLabelText = challengeButton.GetComponentInChildren<TextMeshProUGUI>(true);

        // Name & Subtitle
        if (bossNameText != null)
        {
            if (isDefeated)
            {
                bossNameText.text = $"<s>{displayName}</s>";
                bossNameText.color = new Color(0.45f, 0.85f, 0.45f, 0.8f); // Xanh lá đã hạ gục
            }
            else if (!isUnlocked)
            {
                bossNameText.text = displayName;
                bossNameText.color = new Color(0.45f, 0.45f, 0.5f, 0.5f); // Mờ khóa
            }
            else
            {
                bossNameText.text = displayName;
                bossNameText.color = isFinalBoss ? new Color(1f, 0.35f, 0.35f, 1f) : Color.white;
            }
        }

        if (bossSubtitleText != null)
        {
            bossSubtitleText.text = subtitle;
            bossSubtitleText.color = isUnlocked ? new Color(0.7f, 0.7f, 0.75f, 1f) : new Color(0.4f, 0.4f, 0.45f, 0.5f);
        }

        // Difficulty tag
        if (difficultyText != null)
        {
            if (isDefeated)
            {
                difficultyText.text = "HOÀN THÀNH";
                difficultyText.color = new Color(0.4f, 0.85f, 0.4f, 0.9f);
            }
            else if (!isUnlocked)
            {
                difficultyText.text = "CHƯA MỞ";
                difficultyText.color = new Color(0.45f, 0.45f, 0.5f, 0.5f);
            }
            else
            {
                difficultyText.text = difficulty;
                difficultyText.color = isFinalBoss ? new Color(1f, 0.25f, 0.25f, 1f) : new Color(0.7f, 0.85f, 1f, 1f);
            }
        }

        if (finalBossBadge != null)
            finalBossBadge.SetActive(isFinalBoss);

        // Button state
        if (challengeButton != null)
        {
            Image btnImage = challengeButton.GetComponent<Image>();

            if (isDefeated)
            {
                // Đã tiêu diệt: Không cho đánh nữa
                challengeButton.interactable = false;
                if (buttonLabelText != null)
                {
                    buttonLabelText.text = "✔ ĐÃ TIÊU DIỆT";
                    buttonLabelText.color = new Color(0.75f, 1f, 0.75f, 0.9f);
                }
                if (btnImage != null)
                    btnImage.color = new Color(0.18f, 0.32f, 0.2f, 0.85f);
            }
            else if (!isUnlocked)
            {
                // Chưa mở khóa: Chờ đánh xong con trước
                challengeButton.interactable = false;
                if (buttonLabelText != null)
                {
                    buttonLabelText.text = "🔒 KHOÁ";
                    buttonLabelText.color = new Color(0.6f, 0.6f, 0.65f, 0.6f);
                }
                if (btnImage != null)
                    btnImage.color = new Color(0.18f, 0.18f, 0.22f, 0.6f);
            }
            else
            {
                // Con hiện tại cần tiêu diệt: Sáng rực và sẵn sàng
                challengeButton.interactable = true;
                challengeButton.onClick.RemoveAllListeners();
                challengeButton.onClick.AddListener(OnButtonClicked);

                if (buttonLabelText != null)
                {
                    buttonLabelText.text = "TIÊU DIỆT";
                    buttonLabelText.color = Color.white;
                }

                if (btnImage != null)
                {
                    btnImage.color = isFinalBoss
                        ? new Color(0.95f, 0.15f, 0.15f, 1f)
                        : new Color(0.85f, 0.28f, 0.15f, 1f); // Màu đỏ cam chiến đấu
                }
            }
        }
    }

    private void OnButtonClicked()
    {
        onChallengeAction?.Invoke(bossId);
    }
}
