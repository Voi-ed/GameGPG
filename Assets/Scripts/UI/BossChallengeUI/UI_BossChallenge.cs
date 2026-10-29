using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BossChallenge : MonoBehaviour
{
    [System.Serializable]
    public class BossChallengeInfo
    {
        public string id;
        public string displayName;
        public string subtitle;
        public string difficulty;
        public bool isFinalBoss;

        public BossChallengeInfo(string id, string displayName, string subtitle, string difficulty, bool isFinalBoss)
        {
            this.id = id;
            this.displayName = displayName;
            this.subtitle = subtitle;
            this.difficulty = difficulty;
            this.isFinalBoss = isFinalBoss;
        }
    }

    [Header("UI Configuration")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform buttonsParent;
    [SerializeField] private UI_BossChallengeButton buttonPrefab;

    [Header("Boss List Customization (Optional)")]
    [SerializeField] private List<BossChallengeInfo> customBossList;

    public readonly List<BossChallengeInfo> defaultBossList = new List<BossChallengeInfo>()
    {
        new BossChallengeInfo("CrimsonBlade", "1. Crimson Blade", "Kiếm Sĩ Huyết Ảnh", "Độ khó: Dễ", false),
        new BossChallengeInfo("Seeker", "2. Void Seeker", "Kẻ Săn Lùng Hư Không", "Độ khó: Dễ - Vừa", false),
        new BossChallengeInfo("Satyr", "3. Forest Satyr", "Chiến Binh Quỷ Rừng", "Độ khó: Trung Bình", false),
        new BossChallengeInfo("MartialHero", "4. Martial Hero", "Võ Sĩ Kiếm Thuật Samurai", "Độ khó: Khá", false),
        new BossChallengeInfo("KnightMage", "5. Royal Knight Mage", "Ma Kiếm Sĩ Hoàng Gia", "Độ khó: Khó", false),
        new BossChallengeInfo("BlackKnight", "6. Dread Black Knight", "Hắc Hiệp Sĩ Thiết Giáp", "Độ khó: Rất Khó", false),
        new BossChallengeInfo("Reaper", "7. Death Reaper", "Tử Thần Hái Hồn", "Độ khó: Cực Khó", false),
        new BossChallengeInfo("ArchDemon", "8. ARCH DEMON", "ĐẠI MA VƯƠNG (FINAL BOSS)", "★ ÁC MỘNG - CHIẾN THẮNG GAME ★", true)
    };

    private void Awake()
    {
        if (closeButton == null)
        {
            Transform closeTransform = transform.Find("CloseButton") ?? transform.Find("Panel/CloseButton");
            if (closeTransform != null)
                closeButton = closeTransform.GetComponent<Button>();
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseUI);
        }
    }

    private void OnEnable()
    {
        SetupButtons();
    }

    public void SetupButtons()
    {
        if (buttonsParent == null)
            return;

        List<BossChallengeInfo> bossList = (customBossList != null && customBossList.Count > 0)
            ? customBossList
            : defaultBossList;

        if (buttonPrefab != null)
        {
            // Xoá các nút cũ trước khi sinh
            foreach (Transform child in buttonsParent)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < bossList.Count; i++)
            {
                var boss = bossList[i];
                bool isDefeated = BossArenaManager.IsBossDefeated(boss.id);

                // Boss 1 luôn mở, các boss sau chỉ mở khi boss trước đã bị tiêu diệt
                bool isUnlocked = (i == 0) || BossArenaManager.IsBossDefeated(bossList[i - 1].id);

                UI_BossChallengeButton btnInstance = Instantiate(buttonPrefab, buttonsParent);
                btnInstance.Setup(
                    boss.id,
                    boss.displayName,
                    boss.subtitle,
                    boss.difficulty,
                    boss.isFinalBoss,
                    isUnlocked,
                    isDefeated,
                    OnBossSelected
                );
            }
        }
        else
        {
            // Cập nhật lại các item runtime nếu đã được tạo từ trước
            UI_BossChallengeButton[] existingButtons = buttonsParent.GetComponentsInChildren<UI_BossChallengeButton>(true);
            for (int i = 0; i < existingButtons.Length && i < bossList.Count; i++)
            {
                var boss = bossList[i];
                bool isDefeated = BossArenaManager.IsBossDefeated(boss.id);
                bool isUnlocked = (i == 0) || BossArenaManager.IsBossDefeated(bossList[i - 1].id);

                existingButtons[i].Setup(
                    boss.id,
                    boss.displayName,
                    boss.subtitle,
                    boss.difficulty,
                    boss.isFinalBoss,
                    isUnlocked,
                    isDefeated,
                    OnBossSelected
                );
            }
        }
    }

    public void CloseUI()
    {
        if (UI.instance != null)
            UI.instance.OpenBossChallengeUI(false);
        else
            gameObject.SetActive(false);
    }

    private void OnBossSelected(string bossId)
    {
        // Kiểm tra an toàn: Nếu boss đã tiêu diệt thì không cho đánh lại
        if (BossArenaManager.IsBossDefeated(bossId))
        {
            Debug.Log($"[BossChallengeUI] Boss {bossId} đã bị tiêu diệt, không cần đánh lại!");
            return;
        }

        Debug.Log($"[BossChallengeUI] Player chose to eliminate boss: {bossId}");
        CloseUI();
        BossArenaManager.StartChallenge(bossId);
    }

    /// <summary>
    /// Tạo UI Runtime mở rộng, thoáng đãng, không dùng ScrollView cho 8 Boss
    /// </summary>
    public static UI_BossChallenge CreateDefaultUI(Transform canvasParent)
    {
        GameObject root = new GameObject("UI_BossChallenge", typeof(RectTransform), typeof(CanvasRenderer));
        root.transform.SetParent(canvasParent, false);

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;
        rootRect.anchoredPosition = Vector2.zero;

        // Dark background overlay
        Image overlayImage = root.AddComponent<Image>();
        overlayImage.color = new Color(0.04f, 0.04f, 0.08f, 0.88f);

        // Main Panel (To ra và thoáng: 980 x 680)
        GameObject panelGo = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelGo.transform.SetParent(root.transform, false);
        RectTransform panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(980, 680);
        panelRect.anchoredPosition = Vector2.zero;
        Image panelImage = panelGo.GetComponent<Image>();
        panelImage.color = new Color(0.11f, 0.11f, 0.15f, 0.98f);

        // Header Title
        GameObject titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(panelGo.transform, false);
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(-60, 50);
        titleRect.anchoredPosition = new Vector2(0, -18);

        TextMeshProUGUI titleText = titleGo.GetComponent<TextMeshProUGUI>();
        titleText.text = "⚔ ĐẤU TRƯỜNG TIÊU DIỆT BOSS ⚔";
        titleText.fontSize = 26;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 0.84f, 0.35f, 1f);

        // Close Button
        GameObject closeBtnGo = new GameObject("CloseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        closeBtnGo.transform.SetParent(panelGo.transform, false);
        RectTransform closeRect = closeBtnGo.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.sizeDelta = new Vector2(38, 38);
        closeRect.anchoredPosition = new Vector2(-20, -20);
        closeBtnGo.GetComponent<Image>().color = new Color(0.85f, 0.22f, 0.22f, 1f);

        GameObject closeTxtGo = new GameObject("X", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeTxtGo.transform.SetParent(closeBtnGo.transform, false);
        RectTransform closeTxtRect = closeTxtGo.GetComponent<RectTransform>();
        closeTxtRect.sizeDelta = new Vector2(38, 38);
        closeTxtRect.anchoredPosition = Vector2.zero;
        TextMeshProUGUI closeTxt = closeTxtGo.GetComponent<TextMeshProUGUI>();
        closeTxt.text = "✕";
        closeTxt.fontSize = 22;
        closeTxt.fontStyle = FontStyles.Bold;
        closeTxt.alignment = TextAlignmentOptions.Center;
        closeTxt.color = Color.white;

        // Container trực tiếp trên Panel (KHÔNG CẦN SCROLL VIEW)
        GameObject contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
        contentGo.transform.SetParent(panelGo.transform, false);
        RectTransform contentRect = contentGo.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.offsetMin = new Vector2(30, 20);
        contentRect.offsetMax = new Vector2(-30, -75);

        VerticalLayoutGroup layout = contentGo.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(0, 0, 5, 5);
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        UI_BossChallenge uiComponent = root.AddComponent<UI_BossChallenge>();
        uiComponent.closeButton = closeBtnGo.GetComponent<Button>();
        uiComponent.buttonsParent = contentRect;

        // Build 8 entries với kích thước đẹp mắt và phân cấp mở khóa tuần tự
        for (int i = 0; i < uiComponent.defaultBossList.Count; i++)
        {
            var boss = uiComponent.defaultBossList[i];
            bool isDefeated = BossArenaManager.IsBossDefeated(boss.id);
            bool isUnlocked = (i == 0) || BossArenaManager.IsBossDefeated(uiComponent.defaultBossList[i - 1].id);

            GameObject itemRow = new GameObject(boss.id, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            itemRow.transform.SetParent(contentRect, false);
            RectTransform itemRect = itemRow.GetComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(0, 56);

            Image rowBg = itemRow.GetComponent<Image>();
            if (isDefeated)
                rowBg.color = new Color(0.14f, 0.22f, 0.16f, 0.95f);
            else if (!isUnlocked)
                rowBg.color = new Color(0.12f, 0.12f, 0.15f, 0.65f);
            else if (boss.isFinalBoss)
                rowBg.color = new Color(0.38f, 0.08f, 0.08f, 0.98f);
            else
                rowBg.color = new Color(0.18f, 0.19f, 0.26f, 0.95f);

            // Left text (Name + Subtitle)
            GameObject infoTextGo = new GameObject("InfoText", typeof(RectTransform), typeof(TextMeshProUGUI));
            infoTextGo.transform.SetParent(itemRow.transform, false);
            RectTransform infoRect = infoTextGo.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0, 0);
            infoRect.anchorMax = new Vector2(0.64f, 1);
            infoRect.offsetMin = new Vector2(20, 0);
            infoRect.offsetMax = new Vector2(0, 0);

            TextMeshProUGUI infoText = infoTextGo.GetComponent<TextMeshProUGUI>();
            infoText.text = $"<b>{boss.displayName}</b> <size=14><color=#999999>— {boss.subtitle}</color></size>";
            infoText.fontSize = 17;
            infoText.alignment = TextAlignmentOptions.MidlineLeft;

            // Difficulty tag
            GameObject diffTextGo = new GameObject("DiffText", typeof(RectTransform), typeof(TextMeshProUGUI));
            diffTextGo.transform.SetParent(itemRow.transform, false);
            RectTransform diffRect = diffTextGo.GetComponent<RectTransform>();
            diffRect.anchorMin = new Vector2(0.64f, 0);
            diffRect.anchorMax = new Vector2(0.81f, 1);
            diffRect.offsetMin = Vector2.zero;
            diffRect.offsetMax = Vector2.zero;

            TextMeshProUGUI diffText = diffTextGo.GetComponent<TextMeshProUGUI>();
            diffText.text = boss.difficulty;
            diffText.fontSize = 14;
            diffText.alignment = TextAlignmentOptions.Center;

            // Action Button ("TIÊU DIỆT" / "✔ ĐÃ TIÊU DIỆT" / "🔒 KHOÁ")
            GameObject btnGo = new GameObject("ActionBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(itemRow.transform, false);
            RectTransform btnRect = btnGo.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.82f, 0.12f);
            btnRect.anchorMax = new Vector2(0.98f, 0.88f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            Button btn = btnGo.GetComponent<Button>();

            GameObject btnTxtGo = new GameObject("BtnText", typeof(RectTransform), typeof(TextMeshProUGUI));
            btnTxtGo.transform.SetParent(btnGo.transform, false);
            RectTransform btnTxtRect = btnTxtGo.GetComponent<RectTransform>();
            btnTxtRect.anchorMin = Vector2.zero;
            btnTxtRect.anchorMax = Vector2.one;
            btnTxtRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnTxt = btnTxtGo.GetComponent<TextMeshProUGUI>();
            btnTxt.fontSize = 14;
            btnTxt.fontStyle = FontStyles.Bold;
            btnTxt.alignment = TextAlignmentOptions.Center;

            UI_BossChallengeButton itemComp = itemRow.AddComponent<UI_BossChallengeButton>();
            itemComp.Setup(
                boss.id,
                boss.displayName,
                boss.subtitle,
                boss.difficulty,
                boss.isFinalBoss,
                isUnlocked,
                isDefeated,
                uiComponent.OnBossSelected,
                infoText,
                null,
                diffText,
                btn,
                btnTxt
            );
        }

        root.SetActive(false);
        return uiComponent;
    }
}
