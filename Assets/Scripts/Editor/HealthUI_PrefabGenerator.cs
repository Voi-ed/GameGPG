#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class HealthUI_PrefabGenerator
{
    private const string TEXTURE_DIR = "Assets/Graphics/UI/HealthGlobe";
    private const string PREFAB_DIR = "Assets/Prefab/UI";
    private const string CANVAS_PREFAB_PATH = "Assets/Prefab/Game-Setup_Scene/Canvas.prefab";

    private const string PLAYER_GLOBE_PREFAB_PATH = "Assets/Prefab/UI/UI_PlayerHealthGlobe.prefab";
    private const string BOSS_BAR_PREFAB_PATH = "Assets/Prefab/UI/UI_BossHealthBar.prefab";
    private const string VICTORY_BANNER_PREFAB_PATH = "Assets/Prefab/UI/UI_BossVictoryBanner.prefab";

    [MenuItem("Tools/GameGPG/Generate Health UI Prefabs")]
    public static void GenerateAll()
    {
        EnsureDirectories();

        // Bước 1: Tạo các file Sprite PNG cho Quả cầu máu
        GenerateGlobeTextures();

        // Bước 2: Tạo Prefab UI_PlayerHealthGlobe
        GameObject playerGlobePrefab = GeneratePlayerHealthGlobePrefab();

        // Bước 3: Tạo Prefab UI_BossHealthBar
        GameObject bossBarPrefab = GenerateBossHealthBarPrefab();

        // Bước 4: Tạo Prefab UI_BossVictoryBanner
        GameObject victoryBannerPrefab = GenerateBossVictoryBannerPrefab();

        // Bước 5: Tích hợp trực tiếp vào Canvas.prefab
        AttachToCanvasPrefab(playerGlobePrefab, bossBarPrefab, victoryBannerPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=#4CAF50><b>[HealthUI_PrefabGenerator]</b> Hoàn tất tạo và cấu hình thành công các Prefab UI máu & Banner chiến thắng!</color>");
        EditorUtility.DisplayDialog("GameGPG UI Generator", 
            "Đã tạo thành công:\n1. Assets/Prefab/UI/UI_PlayerHealthGlobe.prefab\n2. Assets/Prefab/UI/UI_BossHealthBar.prefab\n3. Assets/Prefab/UI/UI_BossVictoryBanner.prefab\n\nVà đã tích hợp sẵn vào Assets/Prefab/Game-Setup_Scene/Canvas.prefab!", 
            "Tuyệt vời");
    }

    private static void EnsureDirectories()
    {
        if (!Directory.Exists(TEXTURE_DIR))
            Directory.CreateDirectory(TEXTURE_DIR);

        if (!Directory.Exists(PREFAB_DIR))
            Directory.CreateDirectory(PREFAB_DIR);

        AssetDatabase.Refresh();
    }

    private static void GenerateGlobeTextures()
    {
        string circlePath = $"{TEXTURE_DIR}/Globe_Circle.png";
        string ringPath = $"{TEXTURE_DIR}/Globe_Ring.png";
        string glassPath = $"{TEXTURE_DIR}/Globe_Glass.png";

        if (!File.Exists(circlePath))
        {
            byte[] circleBytes = CreateCirclePng(256, Color.white, 2.5f);
            File.WriteAllBytes(circlePath, circleBytes);
        }

        if (!File.Exists(ringPath))
        {
            byte[] ringBytes = CreateRingPng(256, 14f, new Color(0.25f, 0.22f, 0.2f, 1f), 2.5f);
            File.WriteAllBytes(ringPath, ringBytes);
        }

        if (!File.Exists(glassPath))
        {
            byte[] glassBytes = CreateGlassPng(256);
            File.WriteAllBytes(glassPath, glassBytes);
        }

        AssetDatabase.Refresh();

        ConfigureAsSprite(circlePath);
        ConfigureAsSprite(ringPath);
        ConfigureAsSprite(glassPath);

        AssetDatabase.SaveAssets();
    }

    private static void ConfigureAsSprite(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            bool modified = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                modified = true;
            }
            if (importer.alphaIsTransparency != true)
            {
                importer.alphaIsTransparency = true;
                modified = true;
            }
            if (importer.filterMode != FilterMode.Bilinear)
            {
                importer.filterMode = FilterMode.Bilinear;
                modified = true;
            }
            if (modified)
            {
                importer.SaveAndReimport();
            }
        }
    }

    private static GameObject GeneratePlayerHealthGlobePrefab()
    {
        Sprite circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{TEXTURE_DIR}/Globe_Circle.png");
        Sprite ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{TEXTURE_DIR}/Globe_Ring.png");
        Sprite glassSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{TEXTURE_DIR}/Globe_Glass.png");
        TMP_FontAsset font = FindProjectFont();

        GameObject root = new GameObject("UI_PlayerHealthGlobe_BottomLeft", typeof(RectTransform));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.zero;
        rootRect.pivot = Vector2.zero;
        rootRect.sizeDelta = new Vector2(132f, 132f);
        rootRect.anchoredPosition = new Vector2(40f, 40f);

        // 1. Dark Interior
        GameObject darkBgObj = new GameObject("Globe_DarkInterior", typeof(RectTransform), typeof(Image));
        darkBgObj.transform.SetParent(root.transform, false);
        StretchRect(darkBgObj.GetComponent<RectTransform>());
        Image darkBgImg = darkBgObj.GetComponent<Image>();
        darkBgImg.sprite = circleSprite;
        darkBgImg.color = new Color(0.06f, 0.06f, 0.08f, 0.95f);

        // 2. Ghost Liquid
        GameObject ghostObj = new GameObject("Globe_GhostLiquid", typeof(RectTransform), typeof(Image));
        ghostObj.transform.SetParent(root.transform, false);
        StretchRect(ghostObj.GetComponent<RectTransform>());
        Image ghostImg = ghostObj.GetComponent<Image>();
        ghostImg.sprite = circleSprite;
        ghostImg.color = new Color(1f, 0.65f, 0.15f, 0.92f);
        ghostImg.type = Image.Type.Filled;
        ghostImg.fillMethod = Image.FillMethod.Vertical;
        ghostImg.fillOrigin = (int)Image.OriginVertical.Bottom;
        ghostImg.fillAmount = 1f;

        // 3. Main Blood Liquid
        GameObject bloodObj = new GameObject("Globe_MainBloodLiquid", typeof(RectTransform), typeof(Image));
        bloodObj.transform.SetParent(root.transform, false);
        StretchRect(bloodObj.GetComponent<RectTransform>());
        Image bloodImg = bloodObj.GetComponent<Image>();
        bloodImg.sprite = circleSprite;
        bloodImg.color = new Color(0.9f, 0.12f, 0.12f, 1f);
        bloodImg.type = Image.Type.Filled;
        bloodImg.fillMethod = Image.FillMethod.Vertical;
        bloodImg.fillOrigin = (int)Image.OriginVertical.Bottom;
        bloodImg.fillAmount = 1f;

        // 4. Glass Highlight
        GameObject glassObj = new GameObject("Globe_GlassHighlight", typeof(RectTransform), typeof(Image));
        glassObj.transform.SetParent(root.transform, false);
        StretchRect(glassObj.GetComponent<RectTransform>());
        Image glassImg = glassObj.GetComponent<Image>();
        glassImg.sprite = glassSprite;
        glassImg.raycastTarget = false;

        // 5. Outer Frame
        GameObject frameObj = new GameObject("Globe_OuterFrame", typeof(RectTransform), typeof(Image));
        frameObj.transform.SetParent(root.transform, false);
        RectTransform frameRect = frameObj.GetComponent<RectTransform>();
        frameRect.anchorMin = Vector2.zero;
        frameRect.anchorMax = Vector2.one;
        frameRect.sizeDelta = new Vector2(4f, 4f);
        frameRect.anchoredPosition = Vector2.zero;
        Image frameImg = frameObj.GetComponent<Image>();
        frameImg.sprite = ringSprite;
        frameImg.raycastTarget = false;

        // 6. Health Text
        GameObject textObj = new GameObject("Globe_HealthText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(root.transform, false);
        StretchRect(textObj.GetComponent<RectTransform>());
        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = "<size=120%><b>100</b></size>\n<size=65%><color=#CFD8DC>/ 100 HP</color></size>";
        tmp.fontSize = 18f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        // Attach & Setup Component
        UI_PlayerHealthGlobe globeComponent = root.AddComponent<UI_PlayerHealthGlobe>();
        SerializedObject so = new SerializedObject(globeComponent);
        so.FindProperty("globeTransform").objectReferenceValue = rootRect;
        so.FindProperty("mainLiquidImage").objectReferenceValue = bloodImg;
        so.FindProperty("ghostLiquidImage").objectReferenceValue = ghostImg;
        so.FindProperty("glassHighlightImage").objectReferenceValue = glassImg;
        so.FindProperty("frameImage").objectReferenceValue = frameImg;
        so.FindProperty("healthText").objectReferenceValue = tmp;
        so.ApplyModifiedProperties();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PLAYER_GLOBE_PREFAB_PATH);
        Object.DestroyImmediate(root);
        Debug.Log($"[HealthUI_PrefabGenerator] Đã tạo Prefab UI_PlayerHealthGlobe tại: {PLAYER_GLOBE_PREFAB_PATH}");
        return prefab;
    }

    private static GameObject GenerateBossHealthBarPrefab()
    {
        TMP_FontAsset font = FindProjectFont();

        GameObject root = new GameObject("UI_BossHealthBar_TopCenter", typeof(RectTransform), typeof(CanvasGroup));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.sizeDelta = new Vector2(780f, 64f);
        rootRect.anchoredPosition = new Vector2(0f, -32f);

        CanvasGroup cg = root.GetComponent<CanvasGroup>();

        // 1. Backplate
        GameObject bgPanel = new GameObject("BackplatePanel", typeof(RectTransform), typeof(Image));
        bgPanel.transform.SetParent(root.transform, false);
        RectTransform bgRect = bgPanel.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = new Vector2(16f, 10f);
        bgRect.anchoredPosition = Vector2.zero;
        Image bgImg = bgPanel.GetComponent<Image>();
        bgImg.color = new Color(0.05f, 0.05f, 0.07f, 0.85f);

        // 2. Boss Name Text
        GameObject nameObj = new GameObject("BossNameText", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameObj.transform.SetParent(root.transform, false);
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 1f);
        nameRect.anchorMax = new Vector2(1f, 1f);
        nameRect.pivot = new Vector2(0.5f, 1f);
        nameRect.sizeDelta = new Vector2(760f, 26f);
        nameRect.anchoredPosition = new Vector2(0f, -5f);
        TextMeshProUGUI nameTMP = nameObj.GetComponent<TextMeshProUGUI>();
        if (font != null) nameTMP.font = font;
        nameTMP.text = "BOSS NAME";
        nameTMP.fontSize = 18f;
        nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.alignment = TextAlignmentOptions.Center;
        nameTMP.color = new Color(1f, 0.84f, 0.25f);

        // 3. Frame
        GameObject barFrame = new GameObject("HealthBarFrame", typeof(RectTransform), typeof(Image));
        barFrame.transform.SetParent(root.transform, false);
        RectTransform frameRect = barFrame.GetComponent<RectTransform>();
        frameRect.anchorMin = new Vector2(0f, 0f);
        frameRect.anchorMax = new Vector2(1f, 0f);
        frameRect.pivot = new Vector2(0.5f, 0f);
        frameRect.sizeDelta = new Vector2(750f, 22f);
        frameRect.anchoredPosition = new Vector2(0f, 8f);
        Image frameImg = barFrame.GetComponent<Image>();
        frameImg.color = new Color(0.12f, 0.12f, 0.16f, 1f);

        // 4. Ghost Slider
        GameObject ghostSliderObj = new GameObject("GhostSlider", typeof(RectTransform), typeof(Slider));
        ghostSliderObj.transform.SetParent(barFrame.transform, false);
        RectTransform ghostSliderRect = ghostSliderObj.GetComponent<RectTransform>();
        ghostSliderRect.anchorMin = Vector2.zero;
        ghostSliderRect.anchorMax = Vector2.one;
        ghostSliderRect.sizeDelta = new Vector2(-4f, -4f);
        ghostSliderRect.anchoredPosition = Vector2.zero;
        Slider ghostSlider = ghostSliderObj.GetComponent<Slider>();
        ghostSlider.interactable = false;
        ghostSlider.transition = Selectable.Transition.None;
        ghostSlider.minValue = 0f;
        ghostSlider.maxValue = 1f;

        GameObject ghostFill = new GameObject("GhostFill", typeof(RectTransform), typeof(Image));
        ghostFill.transform.SetParent(ghostSliderObj.transform, false);
        StretchRect(ghostFill.GetComponent<RectTransform>());
        Image ghostImg = ghostFill.GetComponent<Image>();
        ghostImg.color = new Color(1f, 0.65f, 0.15f, 0.95f);
        ghostSlider.fillRect = ghostFill.GetComponent<RectTransform>();

        // 5. Main Slider
        GameObject mainSliderObj = new GameObject("MainSlider", typeof(RectTransform), typeof(Slider));
        mainSliderObj.transform.SetParent(barFrame.transform, false);
        RectTransform mainSliderRect = mainSliderObj.GetComponent<RectTransform>();
        mainSliderRect.anchorMin = Vector2.zero;
        mainSliderRect.anchorMax = Vector2.one;
        mainSliderRect.sizeDelta = new Vector2(-4f, -4f);
        mainSliderRect.anchoredPosition = Vector2.zero;
        Slider mainSlider = mainSliderObj.GetComponent<Slider>();
        mainSlider.interactable = false;
        mainSlider.transition = Selectable.Transition.None;
        mainSlider.minValue = 0f;
        mainSlider.maxValue = 1f;

        GameObject mainFill = new GameObject("MainFill", typeof(RectTransform), typeof(Image));
        mainFill.transform.SetParent(mainSliderObj.transform, false);
        StretchRect(mainFill.GetComponent<RectTransform>());
        Image mainImg = mainFill.GetComponent<Image>();
        mainImg.color = new Color(0.85f, 0.16f, 0.16f, 1f);
        mainSlider.fillRect = mainFill.GetComponent<RectTransform>();

        // 6. HP Numbers Text
        GameObject numbersObj = new GameObject("HPNumbersText", typeof(RectTransform), typeof(TextMeshProUGUI));
        numbersObj.transform.SetParent(barFrame.transform, false);
        StretchRect(numbersObj.GetComponent<RectTransform>());
        TextMeshProUGUI numTMP = numbersObj.GetComponent<TextMeshProUGUI>();
        if (font != null) numTMP.font = font;
        numTMP.text = "100 / 100 (100%)";
        numTMP.fontSize = 12.5f;
        numTMP.fontStyle = FontStyles.Bold;
        numTMP.alignment = TextAlignmentOptions.Center;
        numTMP.color = Color.white;

        // Attach & Setup Component
        UI_BossHealthBar controller = root.AddComponent<UI_BossHealthBar>();
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("mainHealthSlider").objectReferenceValue = mainSlider;
        so.FindProperty("ghostHealthSlider").objectReferenceValue = ghostSlider;
        so.FindProperty("bossNameText").objectReferenceValue = nameTMP;
        so.FindProperty("healthNumbersText").objectReferenceValue = numTMP;
        so.FindProperty("canvasGroup").objectReferenceValue = cg;
        so.ApplyModifiedProperties();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, BOSS_BAR_PREFAB_PATH);
        Object.DestroyImmediate(root);
        Debug.Log($"[HealthUI_PrefabGenerator] Đã tạo Prefab UI_BossHealthBar tại: {BOSS_BAR_PREFAB_PATH}");
        return prefab;
    }

    private static GameObject GenerateBossVictoryBannerPrefab()
    {
        TMP_FontAsset font = FindProjectFont();

        // 1. Root: Fullscreen Overlay
        GameObject root = new GameObject("UI_BossVictoryBanner", typeof(RectTransform), typeof(CanvasGroup));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        StretchRect(rootRect);

        CanvasGroup cg = root.GetComponent<CanvasGroup>();

        // 2. Dimmed Backdrop
        GameObject backdrop = new GameObject("DimmedBackdrop", typeof(RectTransform), typeof(Image));
        backdrop.transform.SetParent(root.transform, false);
        StretchRect(backdrop.GetComponent<RectTransform>());
        Image backdropImg = backdrop.GetComponent<Image>();
        backdropImg.color = new Color(0.04f, 0.06f, 0.1f, 0.78f);
        backdropImg.raycastTarget = true;

        // 3. Central Banner Panel
        GameObject panel = new GameObject("BannerPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(root.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(640f, 280f);
        panelRect.anchoredPosition = new Vector2(0f, 15f);

        Image panelBg = panel.GetComponent<Image>();
        panelBg.color = new Color(0.08f, 0.09f, 0.13f, 0.96f);

        // 3.1. Outer Golden Border
        GameObject border = new GameObject("OuterBorder", typeof(RectTransform), typeof(Image));
        border.transform.SetParent(panel.transform, false);
        RectTransform borderRect = border.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = new Vector2(4f, 4f);
        borderRect.anchoredPosition = Vector2.zero;
        Image borderImg = border.GetComponent<Image>();
        borderImg.color = new Color(1f, 0.84f, 0.31f, 0.35f);
        borderImg.raycastTarget = false;

        // 3.2. Header Divider
        GameObject headerDiv = new GameObject("HeaderDivider", typeof(RectTransform), typeof(Image));
        headerDiv.transform.SetParent(panel.transform, false);
        RectTransform headerDivRect = headerDiv.GetComponent<RectTransform>();
        headerDivRect.anchorMin = new Vector2(0.5f, 1f);
        headerDivRect.anchorMax = new Vector2(0.5f, 1f);
        headerDivRect.pivot = new Vector2(0.5f, 1f);
        headerDivRect.sizeDelta = new Vector2(560f, 2f);
        headerDivRect.anchoredPosition = new Vector2(0f, -65f);
        Image headerDivImg = headerDiv.GetComponent<Image>();
        headerDivImg.color = new Color(1f, 0.84f, 0.31f, 0.45f);
        headerDivImg.raycastTarget = false;

        // 3.3. Footer Divider
        GameObject footerDiv = new GameObject("FooterDivider", typeof(RectTransform), typeof(Image));
        footerDiv.transform.SetParent(panel.transform, false);
        RectTransform footerDivRect = footerDiv.GetComponent<RectTransform>();
        footerDivRect.anchorMin = new Vector2(0.5f, 0f);
        footerDivRect.anchorMax = new Vector2(0.5f, 0f);
        footerDivRect.pivot = new Vector2(0.5f, 0f);
        footerDivRect.sizeDelta = new Vector2(560f, 2f);
        footerDivRect.anchoredPosition = new Vector2(0f, 85f);
        Image footerDivImg = footerDiv.GetComponent<Image>();
        footerDivImg.color = new Color(1f, 0.84f, 0.31f, 0.45f);
        footerDivImg.raycastTarget = false;

        // 3.4. Title Text
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(panel.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(600f, 50f);
        titleRect.anchoredPosition = new Vector2(0f, -14f);
        TextMeshProUGUI titleTMP = titleObj.GetComponent<TextMeshProUGUI>();
        if (font != null) titleTMP.font = font;
        titleTMP.text = "⚔ CHIẾN THẮNG BOSS! ⚔";
        titleTMP.fontSize = 28f;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(1f, 0.85f, 0.25f, 1f);
        titleTMP.raycastTarget = false;

        // 3.5. Subtitle Text
        GameObject subObj = new GameObject("SubtitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        subObj.transform.SetParent(panel.transform, false);
        RectTransform subRect = subObj.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0f, 0.5f);
        subRect.anchorMax = new Vector2(1f, 0.5f);
        subRect.pivot = new Vector2(0.5f, 0.5f);
        subRect.sizeDelta = new Vector2(580f, 44f);
        subRect.anchoredPosition = new Vector2(0f, 12f);
        TextMeshProUGUI subTMP = subObj.GetComponent<TextMeshProUGUI>();
        if (font != null) subTMP.font = font;
        subTMP.text = "Cổng dịch chuyển trở về đã được kích hoạt.";
        subTMP.fontSize = 16f;
        subTMP.alignment = TextAlignmentOptions.Center;
        subTMP.color = new Color(0.85f, 0.88f, 0.92f, 1f);
        subTMP.raycastTarget = false;

        // 3.6. Return Button
        GameObject btnObj = new GameObject("ReturnButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(panel.transform, false);
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0f);
        btnRect.anchorMax = new Vector2(0.5f, 0f);
        btnRect.pivot = new Vector2(0.5f, 0f);
        btnRect.sizeDelta = new Vector2(230f, 50f);
        btnRect.anchoredPosition = new Vector2(0f, 22f);
        Image btnImg = btnObj.GetComponent<Image>();
        btnImg.color = new Color(0.18f, 0.65f, 0.35f, 1f);

        Button btn = btnObj.GetComponent<Button>();
        btn.targetGraphic = btnImg;

        // Button Text
        GameObject btnTxtObj = new GameObject("BtnText", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        StretchRect(btnTxtObj.GetComponent<RectTransform>());
        TextMeshProUGUI btnTMP = btnTxtObj.GetComponent<TextMeshProUGUI>();
        if (font != null) btnTMP.font = font;
        btnTMP.text = "QUAY VỀ LÀNG";
        btnTMP.fontSize = 16f;
        btnTMP.fontStyle = FontStyles.Bold;
        btnTMP.alignment = TextAlignmentOptions.Center;
        btnTMP.color = Color.white;
        btnTMP.raycastTarget = false;

        // 4. Attach UI_BossVictoryBanner Component
        UI_BossVictoryBanner bannerComponent = root.AddComponent<UI_BossVictoryBanner>();
        SerializedObject so = new SerializedObject(bannerComponent);
        so.FindProperty("canvasGroup").objectReferenceValue = cg;
        so.FindProperty("panelRect").objectReferenceValue = panelRect;
        so.FindProperty("panelBackground").objectReferenceValue = panelBg;
        so.FindProperty("outerBorder").objectReferenceValue = borderImg;
        so.FindProperty("headerDivider").objectReferenceValue = headerDivImg;
        so.FindProperty("footerDivider").objectReferenceValue = footerDivImg;
        so.FindProperty("titleText").objectReferenceValue = titleTMP;
        so.FindProperty("subtitleText").objectReferenceValue = subTMP;
        so.FindProperty("returnButton").objectReferenceValue = btn;
        so.FindProperty("returnButtonBg").objectReferenceValue = btnImg;
        so.FindProperty("returnButtonText").objectReferenceValue = btnTMP;
        so.ApplyModifiedProperties();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, VICTORY_BANNER_PREFAB_PATH);
        Object.DestroyImmediate(root);
        Debug.Log($"[HealthUI_PrefabGenerator] Đã tạo Prefab UI_BossVictoryBanner tại: {VICTORY_BANNER_PREFAB_PATH}");
        return prefab;
    }

    private static void AttachToCanvasPrefab(GameObject playerGlobePrefab, GameObject bossBarPrefab, GameObject victoryBannerPrefab)
    {
        if (File.Exists(CANVAS_PREFAB_PATH))
        {
            GameObject canvasRoot = PrefabUtility.LoadPrefabContents(CANVAS_PREFAB_PATH);
            if (canvasRoot != null)
            {
                bool isDirty = false;

                // 1. Gắn Player Health Globe vào UI_InGame
                UI_InGame inGame = canvasRoot.GetComponentInChildren<UI_InGame>(true);
                if (inGame != null)
                {
                    UI_PlayerHealthGlobe existingGlobe = inGame.GetComponentInChildren<UI_PlayerHealthGlobe>(true);
                    if (existingGlobe == null && playerGlobePrefab != null)
                    {
                        GameObject spawnedGlobe = (GameObject)PrefabUtility.InstantiatePrefab(playerGlobePrefab, inGame.transform);
                        spawnedGlobe.name = "UI_PlayerHealthGlobe_BottomLeft";
                        existingGlobe = spawnedGlobe.GetComponent<UI_PlayerHealthGlobe>();
                        isDirty = true;
                    }

                    SerializedObject soInGame = new SerializedObject(inGame);
                    SerializedProperty useGlobeProp = soInGame.FindProperty("useHealthGlobe");
                    if (useGlobeProp != null) useGlobeProp.boolValue = true;

                    SerializedProperty globeProp = soInGame.FindProperty("healthGlobe");
                    if (globeProp != null && existingGlobe != null) globeProp.objectReferenceValue = existingGlobe;

                    soInGame.ApplyModifiedProperties();
                    isDirty = true;
                }

                // 2. Gắn Boss Health Bar vào Canvas Root
                UI_BossHealthBar existingBossBar = canvasRoot.GetComponentInChildren<UI_BossHealthBar>(true);
                if (existingBossBar == null && bossBarPrefab != null)
                {
                    GameObject spawnedBossBar = (GameObject)PrefabUtility.InstantiatePrefab(bossBarPrefab, canvasRoot.transform);
                    spawnedBossBar.name = "UI_BossHealthBar_TopCenter";
                    spawnedBossBar.SetActive(false); // Mặc định ẩn
                    isDirty = true;
                }

                // 3. Gắn Boss Victory Banner vào Canvas Root
                UI_BossVictoryBanner existingVictoryBanner = canvasRoot.GetComponentInChildren<UI_BossVictoryBanner>(true);
                if (existingVictoryBanner == null && victoryBannerPrefab != null)
                {
                    GameObject spawnedBanner = (GameObject)PrefabUtility.InstantiatePrefab(victoryBannerPrefab, canvasRoot.transform);
                    spawnedBanner.name = "UI_BossVictoryBanner";
                    spawnedBanner.SetActive(false); // Mặc định ẩn
                    isDirty = true;
                }

                if (isDirty)
                {
                    PrefabUtility.SaveAsPrefabAsset(canvasRoot, CANVAS_PREFAB_PATH);
                    Debug.Log($"[HealthUI_PrefabGenerator] Đã tích hợp thành công vào {CANVAS_PREFAB_PATH}");
                }

                PrefabUtility.UnloadPrefabContents(canvasRoot);
            }
        }
    }

    private static void StretchRect(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    private static TMP_FontAsset FindProjectFont()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/SyneMono-Regular SDF.asset");
        if (font != null) return font;

        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/NerkoOne-Regular SDF.asset");
        if (font != null) return font;

        return TMP_Settings.defaultFontAsset;
    }

    private static byte[] CreateCirclePng(int diameter, Color color, float borderSmoothness)
    {
        Texture2D tex = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
        float radius = diameter * 0.5f;
        Vector2 center = new Vector2(radius, radius);
        Color[] pixels = new Color[diameter * diameter];
        int idx = 0;

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float alpha = Mathf.Clamp01((radius - dist) / borderSmoothness);
                pixels[idx++] = new Color(color.r, color.g, color.b, color.a * alpha);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex.EncodeToPNG();
    }

    private static byte[] CreateRingPng(int diameter, float ringThickness, Color ringColor, float borderSmoothness)
    {
        Texture2D tex = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
        float outerRadius = diameter * 0.5f;
        float innerRadius = outerRadius - ringThickness;
        Vector2 center = new Vector2(outerRadius, outerRadius);
        Color[] pixels = new Color[diameter * diameter];
        int idx = 0;

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float outerAlpha = Mathf.Clamp01((outerRadius - dist) / borderSmoothness);
                float innerAlpha = Mathf.Clamp01((dist - innerRadius) / borderSmoothness);
                float finalAlpha = Mathf.Min(outerAlpha, innerAlpha);
                pixels[idx++] = new Color(ringColor.r, ringColor.g, ringColor.b, ringColor.a * finalAlpha);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex.EncodeToPNG();
    }

    private static byte[] CreateGlassPng(int diameter)
    {
        Texture2D tex = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
        float radius = diameter * 0.5f;
        Vector2 center = new Vector2(radius, radius);
        Vector2 highlightCenter = new Vector2(radius * 0.65f, radius * 1.35f);
        Color[] pixels = new Color[diameter * diameter];
        int idx = 0;

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);
                float distCenter = Vector2.Distance(pos, center);
                float outerAlpha = Mathf.Clamp01((radius - 4f - distCenter) / 2.5f);

                float distHighlight = Vector2.Distance(pos, highlightCenter);
                float highlight = Mathf.Clamp01(1f - (distHighlight / (radius * 0.75f)));
                highlight = Mathf.Pow(highlight, 2.2f);

                pixels[idx++] = new Color(1f, 1f, 1f, outerAlpha * highlight * 0.45f);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex.EncodeToPNG();
    }
}
#endif
