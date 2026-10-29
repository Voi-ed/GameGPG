using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public struct TeleportDestinationData
{
    public string sceneName;
    public string displayName;

    public TeleportDestinationData(string sceneName, string displayName)
    {
        this.sceneName = sceneName;
        this.displayName = displayName;
    }
}

public class UI_Teleport : MonoBehaviour
{
    [Header("Buttons & Components")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform buttonsParent;
    [SerializeField] private UI_TeleportButton buttonPrefab;

    [Header("Pre-assigned Buttons (Tuỳ chọn)")]
    [Tooltip("Nếu sếp tự kéo các button có sẵn trong Hierarchy thì gán vào đây")]
    [SerializeField] private UI_TeleportButton[] preAssignedButtons;

    [Header("Danh Sách 8 Map")]
    [SerializeField] private TeleportDestinationData[] destinations = new TeleportDestinationData[]
    {
        new TeleportDestinationData("1_Village & Camp", "1. Làng & Doanh Trại (Village)"),
        new TeleportDestinationData("2_Forest Green", "2. Rừng Xanh (Forest Green)"),
        new TeleportDestinationData("3_Swamp", "3. Đầm Lầy (Swamp)"),
        new TeleportDestinationData("4_Caves", "4. Hang Động (Caves)"),
        new TeleportDestinationData("5_Castle Dungeon", "5. Ngục Lâu Đài (Castle)"),
        new TeleportDestinationData("6_Inferno Underworld", "6. Hỏa Ngục (Inferno)"),
        new TeleportDestinationData("7_Wasteland", "7. Vùng Đất Chết (Wasteland)"),
        new TeleportDestinationData("8_Metroidvania Ruins", "8. Tàn Tích Cổ (Ruins)")
    };

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseUI);

        if (preAssignedButtons == null || preAssignedButtons.Length == 0)
        {
            preAssignedButtons = GetComponentsInChildren<UI_TeleportButton>(true);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseUI();
        }
    }

    private void OnEnable()
    {
        SetupButtons();
    }

    public void SetupButtons()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Ưu tiên 1: Các nút dựng sẵn trong Hierarchy của Canvas
        if (preAssignedButtons != null && preAssignedButtons.Length > 0)
        {
            for (int i = 0; i < preAssignedButtons.Length; i++)
            {
                if (i < destinations.Length)
                {
                    preAssignedButtons[i].gameObject.SetActive(true);
                    bool isCurrent = string.Equals(currentScene, destinations[i].sceneName, StringComparison.OrdinalIgnoreCase);
                    preAssignedButtons[i].Setup(destinations[i].sceneName, destinations[i].displayName, isCurrent, OnDestinationSelected);
                }
                else
                {
                    preAssignedButtons[i].gameObject.SetActive(false);
                }
            }
        }
        // Ưu tiên 2: Tự động Instantiate prefab nếu có gán buttonPrefab
        else if (buttonPrefab != null && buttonsParent != null)
        {
            foreach (Transform child in buttonsParent)
                Destroy(child.gameObject);

            for (int i = 0; i < destinations.Length; i++)
            {
                UI_TeleportButton btn = Instantiate(buttonPrefab, buttonsParent);
                bool isCurrent = string.Equals(currentScene, destinations[i].sceneName, StringComparison.OrdinalIgnoreCase);
                btn.Setup(destinations[i].sceneName, destinations[i].displayName, isCurrent, OnDestinationSelected);
            }
        }
    }

    public void OnDestinationSelected(string targetScene)
    {
        if (string.IsNullOrEmpty(targetScene))
            return;

        if (AudioManager.instance != null)
            AudioManager.instance.PlayGlobalSFX("button_click");

        CloseUI();

        if (GameManager.instance != null)
        {
            GameManager.instance.ChangeScene(targetScene, RespawnType.Portal);
        }
    }

    public void CloseUI()
    {
        if (UI.instance != null)
            UI.instance.OpenTeleportUI(false);
        else
            gameObject.SetActive(false);
    }

    /// <summary>
    /// Tự động khởi tạo giao diện Teleport chuẩn đẹp mắt nếu trong Canvas chưa có sẵn
    /// </summary>
    public static UI_Teleport CreateDefaultUI(Transform canvasParent)
    {
        TMP_FontAsset font = null;
        var allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        if (allFonts != null && allFonts.Length > 0)
        {
            foreach (var f in allFonts)
            {
                if (f.name.Contains("NerkoOne") || f.name.Contains("alagard"))
                {
                    font = f;
                    break;
                }
            }
            if (font == null) font = allFonts[0];
        }

        // 1. Root UI GameObject
        GameObject rootGo = new GameObject("UI_Teleport", typeof(RectTransform));
        rootGo.transform.SetParent(canvasParent, false);
        RectTransform rootRect = rootGo.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;
        rootRect.anchoredPosition = Vector2.zero;

        // Dimmed background blocker
        Image bgBlocker = rootGo.AddComponent<Image>();
        bgBlocker.color = new Color(0f, 0f, 0f, 0.65f);

        // 2. Main Window Panel
        GameObject panelGo = new GameObject("WindowPanel", typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(rootGo.transform, false);
        RectTransform panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(620f, 480f);
        panelRect.anchoredPosition = Vector2.zero;
        Image panelImage = panelGo.GetComponent<Image>();
        panelImage.color = new Color(0.11f, 0.13f, 0.17f, 0.98f);

        // Border outline
        Outline outline = panelGo.AddComponent<Outline>();
        outline.effectColor = new Color(0.35f, 0.45f, 0.6f, 0.8f);
        outline.effectDistance = new Vector2(2, -2);

        // 3. Header Title
        GameObject titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(panelGo.transform, false);
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(0f, 50f);
        titleRect.anchoredPosition = new Vector2(0f, -15f);

        TextMeshProUGUI titleText = titleGo.GetComponent<TextMeshProUGUI>();
        if (font != null) titleText.font = font;
        titleText.text = "CỔNG DỊCH CHUYỂN (TELEPORT GATE)";
        titleText.fontSize = 28;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.95f, 0.85f, 0.5f, 1f);

        // 4. Close Button (X)
        GameObject closeGo = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeGo.transform.SetParent(panelGo.transform, false);
        RectTransform closeRect = closeGo.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(36f, 36f);
        closeRect.anchoredPosition = new Vector2(-15f, -15f);

        Image closeImg = closeGo.GetComponent<Image>();
        closeImg.color = new Color(0.7f, 0.2f, 0.2f, 0.9f);
        Button closeBtn = closeGo.GetComponent<Button>();

        GameObject closeTxtGo = new GameObject("X", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeTxtGo.transform.SetParent(closeGo.transform, false);
        RectTransform closeTxtRect = closeTxtGo.GetComponent<RectTransform>();
        closeTxtRect.sizeDelta = new Vector2(36f, 36f);
        closeTxtRect.anchoredPosition = Vector2.zero;
        TextMeshProUGUI closeTxt = closeTxtGo.GetComponent<TextMeshProUGUI>();
        if (font != null) closeTxt.font = font;
        closeTxt.text = "X";
        closeTxt.fontSize = 22;
        closeTxt.alignment = TextAlignmentOptions.Center;
        closeTxt.color = Color.white;
        closeTxt.raycastTarget = false;

        // 5. Grid for 8 Map Buttons (2 columns x 4 rows)
        GameObject gridGo = new GameObject("MapGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGo.transform.SetParent(panelGo.transform, false);
        RectTransform gridRect = gridGo.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0.5f, 0.5f);
        gridRect.anchorMax = new Vector2(0.5f, 0.5f);
        gridRect.sizeDelta = new Vector2(560f, 360f);
        gridRect.anchoredPosition = new Vector2(0f, -25f);

        GridLayoutGroup grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(270f, 75f);
        grid.spacing = new Vector2(16f, 12f);
        grid.childAlignment = TextAnchor.MiddleCenter;

        UI_Teleport teleportComponent = rootGo.AddComponent<UI_Teleport>();
        teleportComponent.closeButton = closeBtn;

        string currentScene = SceneManager.GetActiveScene().name;
        UI_TeleportButton[] generatedButtons = new UI_TeleportButton[teleportComponent.destinations.Length];

        for (int i = 0; i < teleportComponent.destinations.Length; i++)
        {
            GameObject btnGo = new GameObject($"Btn_{teleportComponent.destinations[i].sceneName}", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(gridGo.transform, false);

            Image btnImg = btnGo.GetComponent<Image>();
            btnImg.color = new Color(0.18f, 0.22f, 0.28f, 0.9f);
            btnImg.raycastTarget = true;

            Outline btnOutline = btnGo.AddComponent<Outline>();
            btnOutline.effectColor = new Color(0.4f, 0.55f, 0.7f, 0.5f);
            btnOutline.effectDistance = new Vector2(1, -1);

            GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(btnGo.transform, false);
            RectTransform txtRect = txtGo.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = new Vector2(-16f, -10f);
            txtRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI btnTxt = txtGo.GetComponent<TextMeshProUGUI>();
            if (font != null) btnTxt.font = font;
            btnTxt.fontSize = 20;
            btnTxt.alignment = TextAlignmentOptions.Center;
            btnTxt.enableWordWrapping = true;
            btnTxt.color = Color.white;
            btnTxt.raycastTarget = false;

            UI_TeleportButton teleBtn = btnGo.AddComponent<UI_TeleportButton>();
            bool isCurrent = string.Equals(currentScene, teleportComponent.destinations[i].sceneName, StringComparison.OrdinalIgnoreCase);
            teleBtn.Setup(teleportComponent.destinations[i].sceneName, teleportComponent.destinations[i].displayName, isCurrent, teleportComponent.OnDestinationSelected, btnTxt);
            generatedButtons[i] = teleBtn;
        }

        teleportComponent.preAssignedButtons = generatedButtons;
        closeBtn.onClick.AddListener(teleportComponent.CloseUI);

        teleportComponent.SetupButtons();

        return teleportComponent;
    }
}
