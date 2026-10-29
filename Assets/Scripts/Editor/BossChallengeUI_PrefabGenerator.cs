#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class BossChallengeUI_PrefabGenerator
{
    private const string PREFAB_DIR = "Assets/Prefab/UI";
    private const string CANVAS_PREFAB_PATH = "Assets/Prefab/Game-Setup_Scene/Canvas.prefab";
    private const string BOSS_CHALLENGE_PREFAB_PATH = "Assets/Prefab/UI/UI_BossChallenge.prefab";
    private const string BOSS_BUTTON_PREFAB_PATH = "Assets/Prefab/UI/UI_BossChallengeButton.prefab";

    [MenuItem("Tools/GameGPG/Generate Boss Challenge UI Prefab")]
    public static void GenerateAll()
    {
        EnsureDirectories();

        TMP_FontAsset font = FindProjectFont();

        // 1. Tạo Prefab Button đơn lẻ UI_BossChallengeButton
        GameObject buttonPrefab = GenerateBossChallengeButtonPrefab(font);

        // 2. Tạo Prefab tổng thể UI_BossChallenge
        GameObject bossChallengePrefab = GenerateBossChallengePrefab(font);

        // 3. Tích hợp trực tiếp vào Canvas.prefab
        AttachToCanvasPrefab(bossChallengePrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=#4CAF50><b>[BossChallengeUI_PrefabGenerator]</b> Đã tạo thành công Prefab UI_BossChallenge và gắn vào Canvas!</color>");
        
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog(
                "GameGPG UI Generator",
                "Đã tạo thành công:\n" +
                "1. Assets/Prefab/UI/UI_BossChallenge.prefab\n" +
                "2. Assets/Prefab/UI/UI_BossChallengeButton.prefab\n\n" +
                "Và đã tích hợp sẵn vào Assets/Prefab/Game-Setup_Scene/Canvas.prefab!",
                "Tuyệt vời"
            );
        }
    }

    private static void EnsureDirectories()
    {
        if (!Directory.Exists(PREFAB_DIR))
            Directory.CreateDirectory(PREFAB_DIR);

        AssetDatabase.Refresh();
    }

    private static TMP_FontAsset FindProjectFont()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/SyneMono-Regular SDF.asset");
        if (font == null)
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/NerkoOne-Regular SDF.asset");
        if (font == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            if (guids.Length > 0)
                font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        return font;
    }

    /// <summary>
    /// Sinh ra Prefab mẫu UI_BossChallengeButton đơn lẻ để tái sử dụng nếu cần
    /// </summary>
    private static GameObject GenerateBossChallengeButtonPrefab(TMP_FontAsset font)
    {
        GameObject itemRow = new GameObject("UI_BossChallengeButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform itemRect = itemRow.GetComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(0, 56);

        Image rowBg = itemRow.GetComponent<Image>();
        rowBg.color = new Color(0.18f, 0.19f, 0.26f, 0.95f);

        // Info Text (Tên boss + Phụ đề)
        GameObject infoTextGo = new GameObject("InfoText", typeof(RectTransform), typeof(TextMeshProUGUI));
        infoTextGo.transform.SetParent(itemRow.transform, false);
        RectTransform infoRect = infoTextGo.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0, 0);
        infoRect.anchorMax = new Vector2(0.64f, 1);
        infoRect.offsetMin = new Vector2(20, 0);
        infoRect.offsetMax = new Vector2(0, 0);

        TextMeshProUGUI infoText = infoTextGo.GetComponent<TextMeshProUGUI>();
        if (font != null) infoText.font = font;
        infoText.text = "<b>1. Crimson Blade</b> <size=14><color=#999999>— Kiếm Sĩ Huyết Ảnh</color></size>";
        infoText.fontSize = 17;
        infoText.alignment = TextAlignmentOptions.MidlineLeft;

        // Difficulty Text
        GameObject diffTextGo = new GameObject("DiffText", typeof(RectTransform), typeof(TextMeshProUGUI));
        diffTextGo.transform.SetParent(itemRow.transform, false);
        RectTransform diffRect = diffTextGo.GetComponent<RectTransform>();
        diffRect.anchorMin = new Vector2(0.64f, 0);
        diffRect.anchorMax = new Vector2(0.81f, 1);
        diffRect.offsetMin = Vector2.zero;
        diffRect.offsetMax = Vector2.zero;

        TextMeshProUGUI diffText = diffTextGo.GetComponent<TextMeshProUGUI>();
        if (font != null) diffText.font = font;
        diffText.text = "Độ khó: Dễ";
        diffText.fontSize = 14;
        diffText.alignment = TextAlignmentOptions.Center;

        // Action Button
        GameObject btnGo = new GameObject("ActionBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(itemRow.transform, false);
        RectTransform btnRect = btnGo.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.82f, 0.12f);
        btnRect.anchorMax = new Vector2(0.98f, 0.88f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;

        Image btnImg = btnGo.GetComponent<Image>();
        btnImg.color = new Color(0.85f, 0.28f, 0.15f, 1f);
        Button btn = btnGo.GetComponent<Button>();
        btn.targetGraphic = btnImg;

        GameObject btnTxtGo = new GameObject("BtnText", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnTxtGo.transform.SetParent(btnGo.transform, false);
        RectTransform btnTxtRect = btnTxtGo.GetComponent<RectTransform>();
        btnTxtRect.anchorMin = Vector2.zero;
        btnTxtRect.anchorMax = Vector2.one;
        btnTxtRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI btnTxt = btnTxtGo.GetComponent<TextMeshProUGUI>();
        if (font != null) btnTxt.font = font;
        btnTxt.text = "TIÊU DIỆT";
        btnTxt.fontSize = 14;
        btnTxt.fontStyle = FontStyles.Bold;
        btnTxt.alignment = TextAlignmentOptions.Center;
        btnTxt.color = Color.white;

        // Gắn script UI_BossChallengeButton và thiết lập serialize properties
        UI_BossChallengeButton itemComp = itemRow.AddComponent<UI_BossChallengeButton>();
        SerializedObject so = new SerializedObject(itemComp);
        so.FindProperty("bossNameText").objectReferenceValue = infoText;
        so.FindProperty("difficultyText").objectReferenceValue = diffText;
        so.FindProperty("challengeButton").objectReferenceValue = btn;
        so.FindProperty("buttonLabelText").objectReferenceValue = btnTxt;
        so.ApplyModifiedProperties();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(itemRow, BOSS_BUTTON_PREFAB_PATH);
        Object.DestroyImmediate(itemRow);
        Debug.Log($"[BossChallengeUI_PrefabGenerator] Đã tạo Button Prefab tại: {BOSS_BUTTON_PREFAB_PATH}");
        return prefab;
    }

    /// <summary>
    /// Sinh ra Prefab hoàn chỉnh UI_BossChallenge bao gồm Panel, Header, CloseBtn và 8 Boss Rows
    /// </summary>
    private static GameObject GenerateBossChallengePrefab(TMP_FontAsset font)
    {
        // Root Canvas Group / Overlay
        GameObject root = new GameObject("UI_BossChallenge", typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;
        rootRect.anchoredPosition = Vector2.zero;

        // Dark background overlay
        Image overlayImage = root.AddComponent<Image>();
        overlayImage.color = new Color(0.04f, 0.04f, 0.08f, 0.88f);

        // Main Panel (980 x 680)
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
        if (font != null) titleText.font = font;
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
        
        Image closeImg = closeBtnGo.GetComponent<Image>();
        closeImg.color = new Color(0.85f, 0.22f, 0.22f, 1f);
        Button closeBtn = closeBtnGo.GetComponent<Button>();
        closeBtn.targetGraphic = closeImg;

        GameObject closeTxtGo = new GameObject("X", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeTxtGo.transform.SetParent(closeBtnGo.transform, false);
        RectTransform closeTxtRect = closeTxtGo.GetComponent<RectTransform>();
        closeTxtRect.sizeDelta = new Vector2(38, 38);
        closeTxtRect.anchoredPosition = Vector2.zero;
        TextMeshProUGUI closeTxt = closeTxtGo.GetComponent<TextMeshProUGUI>();
        if (font != null) closeTxt.font = font;
        closeTxt.text = "✕";
        closeTxt.fontSize = 22;
        closeTxt.fontStyle = FontStyles.Bold;
        closeTxt.alignment = TextAlignmentOptions.Center;
        closeTxt.color = Color.white;

        // Content Container
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

        // UI_BossChallenge Component
        UI_BossChallenge uiComponent = root.AddComponent<UI_BossChallenge>();

        // Tạo sẵn 8 hàng mục Boss trong Content
        for (int i = 0; i < uiComponent.defaultBossList.Count; i++)
        {
            var boss = uiComponent.defaultBossList[i];

            GameObject itemRow = new GameObject(boss.id, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            itemRow.transform.SetParent(contentRect, false);
            RectTransform itemRect = itemRow.GetComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(0, 56);

            Image rowBg = itemRow.GetComponent<Image>();
            rowBg.color = boss.isFinalBoss 
                ? new Color(0.38f, 0.08f, 0.08f, 0.98f) 
                : new Color(0.18f, 0.19f, 0.26f, 0.95f);

            // InfoText
            GameObject infoTextGo = new GameObject("InfoText", typeof(RectTransform), typeof(TextMeshProUGUI));
            infoTextGo.transform.SetParent(itemRow.transform, false);
            RectTransform infoRect = infoTextGo.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0, 0);
            infoRect.anchorMax = new Vector2(0.64f, 1);
            infoRect.offsetMin = new Vector2(20, 0);
            infoRect.offsetMax = new Vector2(0, 0);

            TextMeshProUGUI infoText = infoTextGo.GetComponent<TextMeshProUGUI>();
            if (font != null) infoText.font = font;
            infoText.text = $"<b>{boss.displayName}</b> <size=14><color=#999999>— {boss.subtitle}</color></size>";
            infoText.fontSize = 17;
            infoText.alignment = TextAlignmentOptions.MidlineLeft;

            // DiffText
            GameObject diffTextGo = new GameObject("DiffText", typeof(RectTransform), typeof(TextMeshProUGUI));
            diffTextGo.transform.SetParent(itemRow.transform, false);
            RectTransform diffRect = diffTextGo.GetComponent<RectTransform>();
            diffRect.anchorMin = new Vector2(0.64f, 0);
            diffRect.anchorMax = new Vector2(0.81f, 1);
            diffRect.offsetMin = Vector2.zero;
            diffRect.offsetMax = Vector2.zero;

            TextMeshProUGUI diffText = diffTextGo.GetComponent<TextMeshProUGUI>();
            if (font != null) diffText.font = font;
            diffText.text = boss.difficulty;
            diffText.fontSize = 14;
            diffText.alignment = TextAlignmentOptions.Center;

            // ActionBtn
            GameObject btnGo = new GameObject("ActionBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(itemRow.transform, false);
            RectTransform btnRect = btnGo.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.82f, 0.12f);
            btnRect.anchorMax = new Vector2(0.98f, 0.88f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            Image btnImg = btnGo.GetComponent<Image>();
            btnImg.color = new Color(0.85f, 0.28f, 0.15f, 1f);
            Button btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnImg;

            GameObject btnTxtGo = new GameObject("BtnText", typeof(RectTransform), typeof(TextMeshProUGUI));
            btnTxtGo.transform.SetParent(btnGo.transform, false);
            RectTransform btnTxtRect = btnTxtGo.GetComponent<RectTransform>();
            btnTxtRect.anchorMin = Vector2.zero;
            btnTxtRect.anchorMax = Vector2.one;
            btnTxtRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnTxt = btnTxtGo.GetComponent<TextMeshProUGUI>();
            if (font != null) btnTxt.font = font;
            btnTxt.text = "TIÊU DIỆT";
            btnTxt.fontSize = 14;
            btnTxt.fontStyle = FontStyles.Bold;
            btnTxt.alignment = TextAlignmentOptions.Center;
            btnTxt.color = Color.white;

            // Setup UI_BossChallengeButton component
            UI_BossChallengeButton itemComp = itemRow.AddComponent<UI_BossChallengeButton>();
            SerializedObject soItem = new SerializedObject(itemComp);
            soItem.FindProperty("bossNameText").objectReferenceValue = infoText;
            soItem.FindProperty("difficultyText").objectReferenceValue = diffText;
            soItem.FindProperty("challengeButton").objectReferenceValue = btn;
            soItem.FindProperty("buttonLabelText").objectReferenceValue = btnTxt;
            soItem.ApplyModifiedProperties();
        }

        // Serialize references on UI_BossChallenge
        SerializedObject soUI = new SerializedObject(uiComponent);
        soUI.FindProperty("closeButton").objectReferenceValue = closeBtn;
        soUI.FindProperty("buttonsParent").objectReferenceValue = contentRect;
        soUI.ApplyModifiedProperties();

        // Mặc định ẩn để không che màn hình khi mới bật game
        root.SetActive(false);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, BOSS_CHALLENGE_PREFAB_PATH);
        Object.DestroyImmediate(root);
        Debug.Log($"[BossChallengeUI_PrefabGenerator] Đã tạo Prefab UI_BossChallenge tại: {BOSS_CHALLENGE_PREFAB_PATH}");
        return prefab;
    }

    /// <summary>
    /// Gắn Prefab UI_BossChallenge vào Canvas.prefab
    /// </summary>
    private static void AttachToCanvasPrefab(GameObject bossChallengePrefab)
    {
        if (File.Exists(CANVAS_PREFAB_PATH))
        {
            GameObject canvasRoot = PrefabUtility.LoadPrefabContents(CANVAS_PREFAB_PATH);
            if (canvasRoot != null)
            {
                UI_BossChallenge existingUI = canvasRoot.GetComponentInChildren<UI_BossChallenge>(true);
                if (existingUI == null && bossChallengePrefab != null)
                {
                    GameObject spawned = (GameObject)PrefabUtility.InstantiatePrefab(bossChallengePrefab, canvasRoot.transform);
                    spawned.name = "UI_BossChallenge";
                    spawned.SetActive(false); // Đảm bảo luôn ẩn mặc định

                    PrefabUtility.SaveAsPrefabAsset(canvasRoot, CANVAS_PREFAB_PATH);
                    Debug.Log($"[BossChallengeUI_PrefabGenerator] Đã gắn thành công UI_BossChallenge vào Canvas.prefab!");
                }
                else
                {
                    Debug.Log($"[BossChallengeUI_PrefabGenerator] Canvas.prefab đã có UI_BossChallenge, không cần gắn lại.");
                }

                PrefabUtility.UnloadPrefabContents(canvasRoot);
            }
        }
        else
        {
            Debug.LogWarning($"[BossChallengeUI_PrefabGenerator] Không tìm thấy file Canvas.prefab tại {CANVAS_PREFAB_PATH}");
        }
    }
}
#endif
