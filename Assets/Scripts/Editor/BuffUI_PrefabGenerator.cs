#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class BuffUI_PrefabGenerator
{
    private const string PREFAB_DIR = "Assets/Prefab/UI";
    private const string BUFF_SLOT_PREFAB_PATH = "Assets/Prefab/UI/UI_BuffSlot.prefab";
    private const string BUFF_BAR_PREFAB_PATH = "Assets/Prefab/UI/UI_BuffBar.prefab";

    [MenuItem("Tools/GameGPG/Generate Buff UI Prefabs")]
    public static void GeneratePrefabs()
    {
        if (!Directory.Exists(PREFAB_DIR))
            Directory.CreateDirectory(PREFAB_DIR);

        GameObject slotPrefab = GenerateBuffSlotPrefab();
        GameObject barPrefab = GenerateBuffBarPrefab(slotPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=#4CAF50><b>[BuffUI_PrefabGenerator]</b> Đã tạo thành công Prefab UI Buff Tray tại Assets/Prefab/UI/!</color>");
        EditorUtility.DisplayDialog("Buff UI Generator",
            "Đã tạo thành công:\n1. Assets/Prefab/UI/UI_BuffSlot.prefab\n2. Assets/Prefab/UI/UI_BuffBar.prefab\n\nHệ thống sẽ tự động hiển thị trên Quả Cầu Máu Diablo khi người chơi uống thuốc!",
            "OK");
    }

    private static GameObject GenerateBuffSlotPrefab()
    {
        Sprite solidSquare = CreateRoundedBoxSprite(64, 12, Color.white);
        Sprite ringFrame = CreateRoundedRingSprite(64, 12, 4f, Color.white);

        GameObject slotObj = new GameObject("UI_BuffSlot", typeof(RectTransform));
        RectTransform slotRect = slotObj.GetComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(40f, 40f);

        // 1. BG
        GameObject bgObj = new GameObject("Slot_BG", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(slotObj.transform, false);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.sprite = solidSquare;
        bgImg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        bgImg.raycastTarget = false;

        // 2. Icon
        GameObject iconObj = new GameObject("Slot_Icon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(slotObj.transform, false);
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = new Vector2(-6f, -6f);
        Image iconImg = iconObj.GetComponent<Image>();
        iconImg.preserveAspect = true;
        iconImg.raycastTarget = false;

        // 3. Radial Overlay
        GameObject overlayObj = new GameObject("Slot_RadialOverlay", typeof(RectTransform), typeof(Image));
        overlayObj.transform.SetParent(slotObj.transform, false);
        RectTransform overlayRect = overlayObj.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        Image overlayImg = overlayObj.GetComponent<Image>();
        overlayImg.sprite = solidSquare;
        overlayImg.color = new Color(0f, 0f, 0f, 0.65f);
        overlayImg.type = Image.Type.Filled;
        overlayImg.fillMethod = Image.FillMethod.Radial360;
        overlayImg.fillOrigin = (int)Image.Origin360.Top;
        overlayImg.fillClockwise = false;
        overlayImg.fillAmount = 0f;
        overlayImg.raycastTarget = false;

        // 4. Frame
        GameObject frameObj = new GameObject("Slot_Frame", typeof(RectTransform), typeof(Image));
        frameObj.transform.SetParent(slotObj.transform, false);
        RectTransform frameRect = frameObj.GetComponent<RectTransform>();
        frameRect.anchorMin = Vector2.zero;
        frameRect.anchorMax = Vector2.one;
        frameRect.sizeDelta = new Vector2(4f, 4f);
        Image frameImg = frameObj.GetComponent<Image>();
        frameImg.sprite = ringFrame;
        frameImg.color = new Color(0.7f, 0.65f, 0.55f, 0.9f);
        frameImg.raycastTarget = false;

        // 5. Warning
        GameObject warningObj = new GameObject("Slot_WarningBorder", typeof(RectTransform), typeof(Image));
        warningObj.transform.SetParent(slotObj.transform, false);
        RectTransform warnRect = warningObj.GetComponent<RectTransform>();
        warnRect.anchorMin = Vector2.zero;
        warnRect.anchorMax = Vector2.one;
        warnRect.sizeDelta = new Vector2(6f, 6f);
        Image warnImg = warningObj.GetComponent<Image>();
        warnImg.sprite = ringFrame;
        warnImg.color = new Color(1f, 0.2f, 0.2f, 0.85f);
        warnImg.raycastTarget = false;
        warningObj.SetActive(false);

        // 6. Text
        GameObject textObj = new GameObject("Slot_TimerText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(slotObj.transform, false);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 0.45f);
        textRect.anchoredPosition = new Vector2(0f, -2f);
        textRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = "30s";
        tmp.fontSize = 11f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = Color.black;
        tmp.raycastTarget = false;

        UI_BuffSlot slot = slotObj.AddComponent<UI_BuffSlot>();
        SerializedObject so = new SerializedObject(slot);
        so.FindProperty("iconImage").objectReferenceValue = iconImg;
        so.FindProperty("radialCooldownOverlay").objectReferenceValue = overlayImg;
        so.FindProperty("frameImage").objectReferenceValue = frameImg;
        so.FindProperty("warningBorder").objectReferenceValue = warnImg;
        so.FindProperty("timerText").objectReferenceValue = tmp;
        so.ApplyModifiedProperties();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(slotObj, BUFF_SLOT_PREFAB_PATH);
        Object.DestroyImmediate(slotObj);
        return prefab;
    }

    private static GameObject GenerateBuffBarPrefab(GameObject slotPrefab)
    {
        GameObject root = new GameObject("UI_BuffBar", typeof(RectTransform));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.zero;
        rootRect.pivot = new Vector2(0f, 0f);
        rootRect.sizeDelta = new Vector2(300f, 44f);
        rootRect.anchoredPosition = new Vector2(40f, 185f);

        HorizontalLayoutGroup hlg = root.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8f;
        hlg.childAlignment = TextAnchor.LowerLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        ContentSizeFitter csf = root.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        UI_BuffBar buffBar = root.AddComponent<UI_BuffBar>();
        SerializedObject so = new SerializedObject(buffBar);
        so.FindProperty("buffSlotPrefab").objectReferenceValue = slotPrefab;
        so.FindProperty("slotsParent").objectReferenceValue = rootRect;
        so.ApplyModifiedProperties();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, BUFF_BAR_PREFAB_PATH);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static Sprite CreateRoundedBoxSprite(int size, int cornerRadius, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];

        float half = size * 0.5f;
        float r = cornerRadius;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(0, Mathf.Abs(x - half + 0.5f) - (half - r));
                float dy = Mathf.Max(0, Mathf.Abs(y - half + 0.5f) - (half - r));
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01((r - dist) + 0.5f);
                pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private static Sprite CreateRoundedRingSprite(int size, int cornerRadius, float thickness, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];

        float half = size * 0.5f;
        float r = cornerRadius;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(0, Mathf.Abs(x - half + 0.5f) - (half - r));
                float dy = Mathf.Max(0, Mathf.Abs(y - half + 0.5f) - (half - r));
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float outerAlpha = Mathf.Clamp01((r - dist) + 0.5f);
                float innerAlpha = Mathf.Clamp01((r - thickness - dist) + 0.5f);
                float alpha = Mathf.Clamp01(outerAlpha - innerAlpha);
                pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
#endif
