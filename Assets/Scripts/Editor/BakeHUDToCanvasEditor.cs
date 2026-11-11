#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class BakeHUDToCanvasEditor
{
    private const string PREFAB_PATH = "Assets/Prefab/Game-Setup_Scene/Canvas.prefab";
    private const string FONT_PATH = "Assets/TextMesh Pro/Fonts/SyneMono-Regular SDF.asset";
    private const string HAND_POINTER_PATH = "Assets/Graphics/UI/HandPointer_Pixel.png";

    [MenuItem("GameGPG/Bake HUD to Canvas Prefab", false, 100)]
    public static void BakeHUD()
    {
        if (!File.Exists(PREFAB_PATH))
        {
            EditorUtility.DisplayDialog("Lỗi", $"Không tìm thấy Canvas prefab tại: {PREFAB_PATH}", "OK");
            return;
        }

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);
        if (font == null)
        {
            // Fallback font
            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            if (guids.Length > 0)
                font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        Sprite handSprite = AssetDatabase.LoadAssetAtPath<Sprite>(HAND_POINTER_PATH);

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PREFAB_PATH);
        try
        {
            Transform inGameTr = prefabRoot.transform.Find("UI_InGame");
            if (inGameTr == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy object UI_InGame trong Canvas.prefab!", "OK");
                return;
            }

            // 1. Đảm bảo UI_TopRightMenu component có mặt trên UI_InGame
            UI_TopRightMenu topMenu = inGameTr.GetComponent<UI_TopRightMenu>();
            if (topMenu == null)
            {
                topMenu = inGameTr.gameObject.AddComponent<UI_TopRightMenu>();
            }

            // 2. Đảm bảo HintManager component có mặt trên UI_InGame
            HintManager hintMgr = inGameTr.GetComponent<HintManager>();
            if (hintMgr == null)
            {
                hintMgr = inGameTr.gameObject.AddComponent<HintManager>();
            }

            // 3. Gán topRightMenu vào UI_InGame
            UI_InGame inGame = inGameTr.GetComponent<UI_InGame>();
            if (inGame != null)
            {
                SerializedObject soInGame = new SerializedObject(inGame);
                SerializedProperty propTopMenu = soInGame.FindProperty("topRightMenu");
                if (propTopMenu != null)
                {
                    propTopMenu.objectReferenceValue = topMenu;
                    soInGame.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            // 4. Xóa Btn_FastTravel cũ ngoài root UI_InGame nếu có
            Transform legacyFT = inGameTr.Find("Btn_FastTravel");
            if (legacyFT != null && legacyFT.parent == inGameTr)
            {
                Object.DestroyImmediate(legacyFT.gameObject);
            }

            // 5. Tạo / Cấu hình TopRight_MenuBar
            RectTransform rtMenuBar = SetupTopRightMenuBar(inGameTr);

            // 6. Tạo 5 Buttons chuẩn bên trong TopRight_MenuBar
            Button btnCharacter = SetupTextButton(rtMenuBar, "Btn_Character", "NHÂN VẬT [C]", 0, font);
            Button btnSkillTree = SetupTextButton(rtMenuBar, "Btn_SkillTree", "KỸ NĂNG [K]", 1, font);
            (GameObject badgeObj, TextMeshProUGUI badgeText) = SetupSkillBadge(btnSkillTree.gameObject, font);
            Button btnQuest = SetupTextButton(rtMenuBar, "Btn_Quest", "NHIỆM VỤ [J]", 2, font);
            Button btnCodex = SetupTextButton(rtMenuBar, "Btn_Codex", "BÁCH KHOA [B]", 3, font);
            Button btnFastTravel = SetupTextButton(rtMenuBar, "Btn_FastTravel", "DỊCH CHUYỂN [T]", 4, font);

            // 7. Gán SerializedProperties cho UI_TopRightMenu
            SerializedObject soTop = new SerializedObject(topMenu);
            SetObjectProp(soTop, "menuContainer", rtMenuBar);
            SetObjectProp(soTop, "btnCharacter", btnCharacter);
            SetObjectProp(soTop, "btnSkillTree", btnSkillTree);
            SetObjectProp(soTop, "btnQuest", btnQuest);
            SetObjectProp(soTop, "btnCodex", btnCodex);
            SetObjectProp(soTop, "btnFastTravel", btnFastTravel);
            SetObjectProp(soTop, "skillBadgeObj", badgeObj);
            SetObjectProp(soTop, "skillBadgeText", badgeText);
            SerializedProperty propSize = soTop.FindProperty("buttonSize");
            if (propSize != null) propSize.vector2Value = new Vector2(118f, 32f);
            soTop.ApplyModifiedPropertiesWithoutUndo();

            // 8. Tạo / Cấu hình HUD_ObjectiveHintPanel
            (GameObject hintPanel, TextMeshProUGUI titleText, TextMeshProUGUI bodyText, CanvasGroup cg, Image handIcon) =
                SetupHintPanel(inGameTr, font, handSprite);

            // 9. Gán SerializedProperties cho HintManager
            SerializedObject soHint = new SerializedObject(hintMgr);
            SetObjectProp(soHint, "hintPanel", hintPanel);
            SetObjectProp(soHint, "titleText", titleText);
            SetObjectProp(soHint, "bodyText", bodyText);
            SetObjectProp(soHint, "canvasGroup", cg);
            SetObjectProp(soHint, "handIcon", handIcon);
            SetObjectProp(soHint, "handPointerSprite", handSprite);
            soHint.ApplyModifiedPropertiesWithoutUndo();

            // 10. Lưu lại Prefab
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PREFAB_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=#55ff55>[GameGPG]</color> Bake thành công TopRight_MenuBar và HUD_ObjectiveHintPanel vào Canvas.prefab!");
            EditorUtility.DisplayDialog(
                "Thành Công!",
                "Đã bake thành công cả 2 cụm UI vào Canvas.prefab:\n\n" +
                "1. TopRight_MenuBar (5 nút text tiếng Việt chuẩn RPG)\n" +
                "2. HUD_ObjectiveHintPanel (Khung hiển thị mục tiêu & mẹo nhiệm vụ)\n\n" +
                "Tất cả GameObjects và SerializedFields đã được gán hoàn chỉnh!",
                "Tuyệt vời"
            );
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    private static void SetObjectProp(SerializedObject so, string propName, Object value)
    {
        SerializedProperty prop = so.FindProperty(propName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
        }
    }

    private static RectTransform SetupTopRightMenuBar(Transform parent)
    {
        Transform existing = parent.Find("TopRight_MenuBar");
        GameObject barObj;
        if (existing == null)
        {
            barObj = new GameObject("TopRight_MenuBar", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            barObj.transform.SetParent(parent, false);
        }
        else
        {
            barObj = existing.gameObject;
        }

        RectTransform rt = barObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-20f, -20f);
        rt.localScale = Vector3.one;

        HorizontalLayoutGroup hlg = barObj.GetComponent<HorizontalLayoutGroup>() ?? barObj.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8f;
        hlg.childAlignment = TextAnchor.MiddleRight;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        ContentSizeFitter csf = barObj.GetComponent<ContentSizeFitter>() ?? barObj.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return rt;
    }

    private static Button SetupTextButton(Transform parent, string buttonName, string label, int siblingIndex, TMP_FontAsset font)
    {
        Transform existing = parent.Find(buttonName);
        GameObject btnObj;
        if (existing == null)
        {
            // Kiểm tra xem có Btn_Codex cũ ngoài parent không
            Transform oldCodex = parent.parent.Find(buttonName);
            if (oldCodex != null)
            {
                oldCodex.SetParent(parent, false);
                btnObj = oldCodex.gameObject;
            }
            else
            {
                btnObj = new GameObject(buttonName, typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Button));
                btnObj.transform.SetParent(parent, false);
            }
        }
        else
        {
            btnObj = existing.gameObject;
        }

        btnObj.name = buttonName;
        btnObj.transform.SetSiblingIndex(siblingIndex);
        btnObj.transform.localScale = Vector3.one;

        // Xóa icon cũ nếu có
        Transform oldIcon = btnObj.transform.Find("Icon");
        if (oldIcon != null) Object.DestroyImmediate(oldIcon.gameObject);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(118f, 32f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Image img = btnObj.GetComponent<Image>() ?? btnObj.AddComponent<Image>();
        img.color = new Color(0.08f, 0.10f, 0.16f, 0.92f);
        img.sprite = null;

        Outline outline = btnObj.GetComponent<Outline>() ?? btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.95f, 0.75f, 0.2f, 0.85f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        Button btn = btnObj.GetComponent<Button>() ?? btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.20f, 0.25f, 0.38f, 1f);
        colors.pressedColor = new Color(0.12f, 0.15f, 0.22f, 1f);
        colors.fadeDuration = 0.1f;
        btn.colors = colors;

        // Child Text
        Transform textTr = btnObj.transform.Find("Text") ?? btnObj.transform.Find("KeyHint");
        GameObject textObj;
        if (textTr == null)
        {
            textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(btnObj.transform, false);
        }
        else
        {
            textObj = textTr.gameObject;
            textObj.name = "Text";
        }

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        RectTransform textRt = tmp.rectTransform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        textRt.pivot = new Vector2(0.5f, 0.5f);

        if (font != null) tmp.font = font;
        tmp.text = label;
        tmp.fontSize = 10.5f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.88f, 0.35f, 1f);

        return btn;
    }

    private static (GameObject badgeObj, TextMeshProUGUI badgeText) SetupSkillBadge(GameObject skillBtnObj, TMP_FontAsset font)
    {
        Transform existing = skillBtnObj.transform.Find("Badge");
        GameObject badgeObj;
        if (existing == null)
        {
            badgeObj = new GameObject("Badge", typeof(RectTransform), typeof(Image), typeof(Outline));
            badgeObj.transform.SetParent(skillBtnObj.transform, false);
        }
        else
        {
            badgeObj = existing.gameObject;
        }

        RectTransform rt = badgeObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(-4f, -4f);
        rt.sizeDelta = new Vector2(18f, 18f);

        Image img = badgeObj.GetComponent<Image>() ?? badgeObj.AddComponent<Image>();
        img.color = new Color(0.9f, 0.15f, 0.15f, 1f);

        Outline outline = badgeObj.GetComponent<Outline>() ?? badgeObj.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.3f, 0.9f);
        outline.effectDistance = new Vector2(1f, -1f);

        Transform textTr = badgeObj.transform.Find("BadgeText");
        GameObject textObj;
        if (textTr == null)
        {
            textObj = new GameObject("BadgeText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(badgeObj.transform, false);
        }
        else
        {
            textObj = textTr.gameObject;
        }

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        RectTransform textRt = tmp.rectTransform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        if (font != null) tmp.font = font;
        tmp.fontSize = 9.5f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.text = "+";

        badgeObj.SetActive(false);
        return (badgeObj, tmp);
    }

    private static (GameObject hintPanel, TextMeshProUGUI titleText, TextMeshProUGUI bodyText, CanvasGroup cg, Image handIcon)
        SetupHintPanel(Transform parent, TMP_FontAsset font, Sprite handSprite)
    {
        Transform existing = parent.Find("HUD_ObjectiveHintPanel");
        GameObject panelObj;
        if (existing == null)
        {
            panelObj = new GameObject("HUD_ObjectiveHintPanel", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(CanvasGroup));
            panelObj.transform.SetParent(parent, false);
        }
        else
        {
            panelObj = existing.gameObject;
        }

        RectTransform rt = panelObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(20f, -20f);
        rt.sizeDelta = new Vector2(440f, 102f);
        rt.localScale = Vector3.one;

        Image img = panelObj.GetComponent<Image>() ?? panelObj.AddComponent<Image>();
        img.color = new Color(0.08f, 0.10f, 0.16f, 0.90f);
        img.sprite = null;

        Outline outline = panelObj.GetComponent<Outline>() ?? panelObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.25f, 0.35f, 0.5f, 0.65f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        CanvasGroup cg = panelObj.GetComponent<CanvasGroup>() ?? panelObj.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        // Hand Icon
        Transform handTr = panelObj.transform.Find("Hand_Icon");
        GameObject handObj = handTr == null
            ? new GameObject("Hand_Icon", typeof(RectTransform), typeof(Image))
            : handTr.gameObject;
        handObj.transform.SetParent(panelObj.transform, false);

        RectTransform handRt = handObj.GetComponent<RectTransform>();
        handRt.anchorMin = new Vector2(0f, 1f);
        handRt.anchorMax = new Vector2(0f, 1f);
        handRt.pivot = new Vector2(0f, 1f);
        handRt.anchoredPosition = new Vector2(10f, -8f);
        handRt.sizeDelta = new Vector2(24f, 18f);

        Image handImg = handObj.GetComponent<Image>() ?? handObj.AddComponent<Image>();
        handImg.preserveAspect = true;
        if (handSprite != null) handImg.sprite = handSprite;

        // Title Text
        Transform titleTr = panelObj.transform.Find("Title_Text");
        GameObject titleObj = titleTr == null
            ? new GameObject("Title_Text", typeof(RectTransform), typeof(TextMeshProUGUI))
            : titleTr.gameObject;
        titleObj.transform.SetParent(panelObj.transform, false);

        TextMeshProUGUI tmpTitle = titleObj.GetComponent<TextMeshProUGUI>() ?? titleObj.AddComponent<TextMeshProUGUI>();
        RectTransform titleRt = tmpTitle.rectTransform;
        titleRt.anchorMin = new Vector2(0f, 1f);
        titleRt.anchorMax = new Vector2(1f, 1f);
        titleRt.pivot = new Vector2(0f, 1f);
        titleRt.anchoredPosition = new Vector2(34f, -8f);
        titleRt.sizeDelta = new Vector2(-44f, 20f);

        if (font != null) tmpTitle.font = font;
        tmpTitle.fontSize = 11.5f;
        tmpTitle.fontStyle = FontStyles.Bold;
        tmpTitle.color = new Color(1f, 0.85f, 0.2f);
        tmpTitle.text = "KHIÊU CHIẾN ĐẤU TRƯỜNG";

        // Body Text
        Transform bodyTr = panelObj.transform.Find("Body_Text");
        GameObject bodyObj = bodyTr == null
            ? new GameObject("Body_Text", typeof(RectTransform), typeof(TextMeshProUGUI))
            : bodyTr.gameObject;
        bodyObj.transform.SetParent(panelObj.transform, false);

        TextMeshProUGUI tmpBody = bodyObj.GetComponent<TextMeshProUGUI>() ?? bodyObj.AddComponent<TextMeshProUGUI>();
        RectTransform bodyRt = tmpBody.rectTransform;
        bodyRt.anchorMin = new Vector2(0f, 1f);
        bodyRt.anchorMax = new Vector2(1f, 1f);
        bodyRt.pivot = new Vector2(0f, 1f);
        bodyRt.anchoredPosition = new Vector2(12f, -30f);
        bodyRt.sizeDelta = new Vector2(-24f, 68f);

        if (font != null) tmpBody.font = font;
        tmpBody.fontSize = 11.5f;
        tmpBody.enableWordWrapping = true;
        tmpBody.color = Color.white;
        tmpBody.text = "Mục tiêu và chỉ dẫn sẽ tự động cập nhật khi làm nhiệm vụ.";

        tmpBody.ForceMeshUpdate();
        float bodyH = tmpBody.preferredHeight;
        float totalH = Mathf.Max(92f, 34f + bodyH + 14f);
        rt.sizeDelta = new Vector2(440f, totalH);
        bodyRt.sizeDelta = new Vector2(-24f, bodyH + 2f);

        panelObj.SetActive(false); // Ẩn mặc định cho tới khi QuestManager gọi ShowHint
        return (panelObj, tmpTitle, tmpBody, cg, handImg);
    }
}
#endif

