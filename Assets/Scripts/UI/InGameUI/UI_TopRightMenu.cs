using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_TopRightMenu : MonoBehaviour
{
    [Header("Menu Container")]
    [SerializeField] private RectTransform menuContainer;

    [Header("Menu Buttons (Text Based)")]
    [SerializeField] private Button btnCharacter;
    [SerializeField] private Button btnSkillTree;
    [SerializeField] private Button btnQuest;
    [SerializeField] private Button btnCodex;
    [SerializeField] private Button btnFastTravel;

    [Header("Button Dimensions")]
    [SerializeField] private Vector2 buttonSize = new Vector2(118f, 32f);

    [Header("Skill Points Badge")]
    [SerializeField] private GameObject skillBadgeObj;
    [SerializeField] private TextMeshProUGUI skillBadgeText;

    private int lastSkillPoints = -1;
    private TMP_FontAsset cachedFont;

    public void Initialize(UI_InGame inGame)
    {
        SetupMenuContainer();
        CleanupLegacyElements();
        CacheGameFont();
        SetupCodexButton();
        SetupCharacterButton();
        SetupSkillTreeButton();
        SetupQuestButton();
        SetupFastTravelButton();
        OrderButtons();
    }

    private void CacheGameFont()
    {
        if (cachedFont != null) return;

        // Ưu tiên lấy font từ TMP text có sẵn trong UI để đồng bộ giao diện
        TextMeshProUGUI existingText = GetComponentInChildren<TextMeshProUGUI>(true);
        if (existingText != null)
        {
            cachedFont = existingText.font;
        }
    }

    private void SetupMenuContainer()
    {
        if (menuContainer == null)
        {
            Transform existing = transform.Find("TopRight_MenuBar");
            if (existing != null)
            {
                menuContainer = existing.GetComponent<RectTransform>();
            }
        }

        if (menuContainer == null)
        {
            GameObject containerObj = new GameObject("TopRight_MenuBar");
            containerObj.transform.SetParent(transform, false);

            menuContainer = containerObj.AddComponent<RectTransform>();
            menuContainer.anchorMin = new Vector2(1f, 1f);
            menuContainer.anchorMax = new Vector2(1f, 1f);
            menuContainer.pivot = new Vector2(1f, 1f);
            menuContainer.anchoredPosition = new Vector2(-20f, -20f);

            HorizontalLayoutGroup hlg = containerObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            ContentSizeFitter csf = containerObj.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    private void CleanupLegacyElements()
    {
        // Xóa nút Dịch Chuyển cũ dạng thanh chữ dài nếu còn nằm ngoài container
        Transform oldBtn = transform.Find("Btn_FastTravel");
        if (oldBtn != null && oldBtn.parent == transform)
        {
            Destroy(oldBtn.gameObject);
        }
    }

    private void SetupCodexButton()
    {
        if (btnCodex == null)
        {
            Transform codexTr = transform.Find("Btn_Codex");
            if (codexTr == null && menuContainer != null)
                codexTr = menuContainer.Find("Btn_Codex");

            if (codexTr != null)
                btnCodex = codexTr.GetComponent<Button>();
        }

        if (btnCodex != null && menuContainer != null)
        {
            btnCodex.transform.SetParent(menuContainer, false);
            btnCodex.transform.localScale = Vector3.one;

            FormatTextButton(btnCodex.gameObject, "BÁCH KHOA [B]");

            btnCodex.onClick.RemoveAllListeners();
            btnCodex.onClick.AddListener(OnClickCodex);
        }
    }

    private void SetupCharacterButton()
    {
        if (btnCharacter == null && menuContainer != null)
        {
            Transform tr = menuContainer.Find("Btn_Character");
            if (tr != null)
                btnCharacter = tr.GetComponent<Button>();
        }

        if (btnCharacter == null && menuContainer != null)
        {
            btnCharacter = CreateTextButton("Btn_Character", "NHÂN VẬT [C]");
        }
        else if (btnCharacter != null)
        {
            FormatTextButton(btnCharacter.gameObject, "NHÂN VẬT [C]");
        }

        if (btnCharacter != null)
        {
            btnCharacter.onClick.RemoveAllListeners();
            btnCharacter.onClick.AddListener(OnClickCharacter);
        }
    }

    private void SetupSkillTreeButton()
    {
        if (btnSkillTree == null && menuContainer != null)
        {
            Transform tr = menuContainer.Find("Btn_SkillTree");
            if (tr != null)
                btnSkillTree = tr.GetComponent<Button>();
        }

        if (btnSkillTree == null && menuContainer != null)
        {
            btnSkillTree = CreateTextButton("Btn_SkillTree", "KỸ NĂNG [K]");
        }
        else if (btnSkillTree != null)
        {
            FormatTextButton(btnSkillTree.gameObject, "KỸ NĂNG [K]");
        }

        if (btnSkillTree != null)
        {
            btnSkillTree.onClick.RemoveAllListeners();
            btnSkillTree.onClick.AddListener(OnClickSkillTree);
            CreateSkillPointBadge(btnSkillTree.gameObject);
        }
    }

    private void SetupQuestButton()
    {
        if (btnQuest == null && menuContainer != null)
        {
            Transform tr = menuContainer.Find("Btn_Quest");
            if (tr != null)
                btnQuest = tr.GetComponent<Button>();
        }

        if (btnQuest == null && menuContainer != null)
        {
            btnQuest = CreateTextButton("Btn_Quest", "NHIỆM VỤ [J]");
        }
        else if (btnQuest != null)
        {
            FormatTextButton(btnQuest.gameObject, "NHIỆM VỤ [J]");
        }

        if (btnQuest != null)
        {
            btnQuest.onClick.RemoveAllListeners();
            btnQuest.onClick.AddListener(OnClickQuest);
        }
    }

    private void SetupFastTravelButton()
    {
        if (btnFastTravel == null && menuContainer != null)
        {
            Transform tr = menuContainer.Find("Btn_FastTravel");
            if (tr != null)
                btnFastTravel = tr.GetComponent<Button>();
        }

        if (btnFastTravel == null && menuContainer != null)
        {
            btnFastTravel = CreateTextButton("Btn_FastTravel", "DỊCH CHUYỂN [T]");
        }
        else if (btnFastTravel != null)
        {
            FormatTextButton(btnFastTravel.gameObject, "DỊCH CHUYỂN [T]");
        }

        if (btnFastTravel != null)
        {
            btnFastTravel.onClick.RemoveAllListeners();
            btnFastTravel.onClick.AddListener(OnClickFastTravel);
        }
    }

    private Button CreateTextButton(string buttonName, string labelText)
    {
        GameObject newBtnObj;

        if (btnCodex != null)
        {
            newBtnObj = Instantiate(btnCodex.gameObject, menuContainer);
            newBtnObj.name = buttonName;

            UI_CodexButton codexComp = newBtnObj.GetComponent<UI_CodexButton>();
            if (codexComp != null)
                Destroy(codexComp);
        }
        else
        {
            newBtnObj = new GameObject(buttonName);
            newBtnObj.transform.SetParent(menuContainer, false);

            newBtnObj.AddComponent<RectTransform>();
            newBtnObj.AddComponent<Image>();
            newBtnObj.AddComponent<Outline>();
            newBtnObj.AddComponent<Button>();
        }

        FormatTextButton(newBtnObj, labelText);
        return newBtnObj.GetComponent<Button>();
    }

    private void FormatTextButton(GameObject btnObj, string labelText)
    {
        // Ẩn/xóa mọi Icon ảnh cũ để giao diện thuần Text Button cực kỳ sạch sẽ
        Transform iconTr = btnObj.transform.Find("Icon");
        if (iconTr != null)
        {
            iconTr.gameObject.SetActive(false);
        }

        // Định hình kích thước nút
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        if (btnRect != null)
        {
            btnRect.sizeDelta = buttonSize;
            btnRect.localScale = Vector3.one;
            btnRect.pivot = new Vector2(0.5f, 0.5f);
        }

        // Nền tối bán trong suốt chuẩn RPG
        Image bg = btnObj.GetComponent<Image>();
        if (bg != null)
        {
            bg.color = new Color(0.08f, 0.10f, 0.16f, 0.92f);
            bg.sprite = null; // Bỏ sprite khung cũ nếu có
        }

        // Viền vàng kim Pixel Art
        Outline outline = btnObj.GetComponent<Outline>();
        if (outline == null)
            outline = btnObj.AddComponent<Outline>();

        outline.effectColor = new Color(0.95f, 0.75f, 0.2f, 0.85f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        // Hiệu ứng đổi màu Button
        Button btn = btnObj.GetComponent<Button>();
        if (btn != null)
        {
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.20f, 0.25f, 0.38f, 1f);
            colors.pressedColor = new Color(0.12f, 0.15f, 0.22f, 1f);
            colors.fadeDuration = 0.1f;
            btn.colors = colors;
        }

        // Cập nhật nhãn chữ Text
        Transform textTr = btnObj.transform.Find("Text");
        if (textTr == null)
            textTr = btnObj.transform.Find("KeyHint");

        TextMeshProUGUI tmpText = null;
        if (textTr == null)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            textTr = textObj.transform;
            tmpText = textObj.AddComponent<TextMeshProUGUI>();
        }
        else
        {
            tmpText = textTr.GetComponent<TextMeshProUGUI>();
        }

        if (tmpText != null)
        {
            RectTransform textRect = textTr.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;

            if (cachedFont != null)
                tmpText.font = cachedFont;

            tmpText.text = labelText;
            tmpText.fontSize = 10.5f;
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = new Color(1f, 0.88f, 0.35f, 1f);
        }
    }

    private void CreateSkillPointBadge(GameObject skillBtnObj)
    {
        if (skillBadgeObj != null && skillBadgeText != null)
            return;

        Transform existing = skillBtnObj.transform.Find("Badge");
        if (existing != null)
        {
            skillBadgeObj = existing.gameObject;
            skillBadgeText = skillBadgeObj.GetComponentInChildren<TextMeshProUGUI>();
            return;
        }

        GameObject badgeObj = new GameObject("Badge");
        badgeObj.transform.SetParent(skillBtnObj.transform, false);

        RectTransform rt = badgeObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(-4f, -4f);
        rt.sizeDelta = new Vector2(18f, 18f);

        Image bg = badgeObj.AddComponent<Image>();
        bg.color = new Color(0.9f, 0.15f, 0.15f, 1f);

        Outline outline = badgeObj.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.3f, 0.9f);
        outline.effectDistance = new Vector2(1f, -1f);

        GameObject textObj = new GameObject("BadgeText");
        textObj.transform.SetParent(badgeObj.transform, false);
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        skillBadgeText = textObj.AddComponent<TextMeshProUGUI>();
        if (cachedFont != null)
            skillBadgeText.font = cachedFont;

        skillBadgeText.fontSize = 9.5f;
        skillBadgeText.fontStyle = FontStyles.Bold;
        skillBadgeText.alignment = TextAlignmentOptions.Center;
        skillBadgeText.color = Color.white;
        skillBadgeText.text = "+";

        skillBadgeObj = badgeObj;
        skillBadgeObj.SetActive(false);
    }

    private void OrderButtons()
    {
        // Thứ tự hàng ngang từ trái sang phải: [Nhân vật] [Kỹ năng] [Nhiệm vụ] [Bách khoa] [Dịch chuyển]
        if (btnCharacter != null) btnCharacter.transform.SetSiblingIndex(0);
        if (btnSkillTree != null) btnSkillTree.transform.SetSiblingIndex(1);
        if (btnQuest != null) btnQuest.transform.SetSiblingIndex(2);
        if (btnCodex != null) btnCodex.transform.SetSiblingIndex(3);
        if (btnFastTravel != null) btnFastTravel.transform.SetSiblingIndex(4);
    }

    #region Click Actions With Mutual Exclusion

    public void OnClickCharacter()
    {
        if (btnCharacter != null)
            btnCharacter.transform.DOPunchScale(new Vector3(-0.06f, -0.06f, 0f), 0.12f, 8, 1).SetUpdate(true);

        if (UI.instance == null) return;

        // Đóng các bảng khác nếu đang mở
        if (UI.instance.activeQuestUI != null && UI.instance.activeQuestUI.gameObject.activeSelf)
            UI.instance.ToggleActiveQuestUI();
        if (UI.instance.skillTreeUI != null && UI.instance.skillTreeUI.gameObject.activeSelf)
            UI.instance.ToggleSkillTreeUI();
        if (UI.instance.codexUI != null && UI.instance.codexUI.gameObject.activeSelf)
            UI.instance.ToggleCodexUI();
        if (UI.instance.teleportUI != null && UI.instance.teleportUI.gameObject.activeSelf)
            UI.instance.OpenTeleportUI(false);

        UI.instance.ToggleInventoryUI();
    }

    public void OnClickSkillTree()
    {
        if (btnSkillTree != null)
            btnSkillTree.transform.DOPunchScale(new Vector3(-0.06f, -0.06f, 0f), 0.12f, 8, 1).SetUpdate(true);

        if (UI.instance == null) return;

        if (UI.instance.inventoryUI != null && UI.instance.inventoryUI.gameObject.activeSelf)
            UI.instance.ToggleInventoryUI();
        if (UI.instance.activeQuestUI != null && UI.instance.activeQuestUI.gameObject.activeSelf)
            UI.instance.ToggleActiveQuestUI();
        if (UI.instance.codexUI != null && UI.instance.codexUI.gameObject.activeSelf)
            UI.instance.ToggleCodexUI();
        if (UI.instance.teleportUI != null && UI.instance.teleportUI.gameObject.activeSelf)
            UI.instance.OpenTeleportUI(false);

        UI.instance.ToggleSkillTreeUI();
    }

    public void OnClickQuest()
    {
        if (btnQuest != null)
            btnQuest.transform.DOPunchScale(new Vector3(-0.06f, -0.06f, 0f), 0.12f, 8, 1).SetUpdate(true);

        if (UI.instance == null) return;

        if (UI.instance.inventoryUI != null && UI.instance.inventoryUI.gameObject.activeSelf)
            UI.instance.ToggleInventoryUI();
        if (UI.instance.skillTreeUI != null && UI.instance.skillTreeUI.gameObject.activeSelf)
            UI.instance.ToggleSkillTreeUI();
        if (UI.instance.codexUI != null && UI.instance.codexUI.gameObject.activeSelf)
            UI.instance.ToggleCodexUI();
        if (UI.instance.teleportUI != null && UI.instance.teleportUI.gameObject.activeSelf)
            UI.instance.OpenTeleportUI(false);

        UI.instance.ToggleActiveQuestUI();
    }

    public void OnClickCodex()
    {
        if (btnCodex != null)
            btnCodex.transform.DOPunchScale(new Vector3(-0.06f, -0.06f, 0f), 0.12f, 8, 1).SetUpdate(true);

        if (UI.instance == null) return;

        if (UI.instance.inventoryUI != null && UI.instance.inventoryUI.gameObject.activeSelf)
            UI.instance.ToggleInventoryUI();
        if (UI.instance.activeQuestUI != null && UI.instance.activeQuestUI.gameObject.activeSelf)
            UI.instance.ToggleActiveQuestUI();
        if (UI.instance.skillTreeUI != null && UI.instance.skillTreeUI.gameObject.activeSelf)
            UI.instance.ToggleSkillTreeUI();
        if (UI.instance.teleportUI != null && UI.instance.teleportUI.gameObject.activeSelf)
            UI.instance.OpenTeleportUI(false);

        UI.instance.ToggleCodexUI();
    }

    public void OnClickFastTravel()
    {
        if (btnFastTravel != null)
            btnFastTravel.transform.DOPunchScale(new Vector3(-0.06f, -0.06f, 0f), 0.12f, 8, 1).SetUpdate(true);

        if (UI.instance == null) return;

        if (UI.instance.inventoryUI != null && UI.instance.inventoryUI.gameObject.activeSelf)
            UI.instance.ToggleInventoryUI();
        if (UI.instance.activeQuestUI != null && UI.instance.activeQuestUI.gameObject.activeSelf)
            UI.instance.ToggleActiveQuestUI();
        if (UI.instance.skillTreeUI != null && UI.instance.skillTreeUI.gameObject.activeSelf)
            UI.instance.ToggleSkillTreeUI();
        if (UI.instance.codexUI != null && UI.instance.codexUI.gameObject.activeSelf)
            UI.instance.ToggleCodexUI();

        bool isOpen = UI.instance.teleportUI != null && UI.instance.teleportUI.gameObject.activeSelf;
        UI.instance.OpenTeleportUI(!isOpen);
    }

    #endregion

    private void Update()
    {
        HandleKeyboardShortcuts();
        UpdateSkillPointsBadge();
    }

    private void HandleKeyboardShortcuts()
    {
        if (Keyboard.current == null) return;

        if (UI.instance != null && UI.instance.dialogueUI != null && UI.instance.dialogueUI.gameObject.activeInHierarchy)
            return;

        // Phím tắt K mở Cây Kỹ Năng
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            OnClickSkillTree();
        }

        // Phím tắt J mở Bảng Nhiệm Vụ
        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            OnClickQuest();
        }

        // Phím tắt T mở Dịch Chuyển
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            OnClickFastTravel();
        }
    }

    private void UpdateSkillPointsBadge()
    {
        if (skillBadgeObj == null || UI.instance == null || UI.instance.skillTreeUI == null)
            return;

        int points = UI.instance.skillTreeUI.currentSkillPoints;
        if (points != lastSkillPoints)
        {
            lastSkillPoints = points;
            if (points > 0)
            {
                skillBadgeObj.SetActive(true);
                if (skillBadgeText != null)
                    skillBadgeText.text = points > 9 ? "9+" : points.ToString();
            }
            else
            {
                skillBadgeObj.SetActive(false);
            }
        }
    }
}
