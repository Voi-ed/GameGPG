using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý hệ thống chỉ dẫn / mục tiêu nhiệm vụ (Objective Hint HUD).
/// Tự động cập nhật theo tiến trình Quest và hiển thị gọn gàng trên màn hình.
/// Được gắn trực tiếp vào Canvas.prefab để tự động hiện diện ở mọi màn chơi.
/// </summary>
public class HintManager : MonoBehaviour
{
    public static HintManager instance { get; private set; }

    [Header("UI References (Optional - Tự động tạo nếu để trống)")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Sprite handPointerSprite;
    [SerializeField] private Image handIcon;

    private Player_QuestManager questManager;
    private string lastDisplayedText = "";
    private Coroutine customHintCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        // 1. Đảm bảo UI khung Hint luôn có mặt trên màn hình
        EnsureUIExists();

        // 2. Kích hoạt con trỏ thế giới (World Pointer)
        if (GetComponent<QuestWorldIndicator>() == null)
        {
            gameObject.AddComponent<QuestWorldIndicator>();
        }

        // 3. Kết nối với Player_QuestManager
        StartCoroutine(SetupQuestManagerConnectionCo());
    }

    private IEnumerator SetupQuestManagerConnectionCo()
    {
        // Chờ Player và QuestManager sẵn sàng
        float timeout = 4.0f;
        while ((Player.instance == null || Player.instance.questManager == null) && timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (Player.instance != null && Player.instance.questManager != null)
        {
            questManager = Player.instance.questManager;
            questManager.OnQuestListUpdated -= UpdateHintDisplay;
            questManager.OnQuestListUpdated += UpdateHintDisplay;
        }

        if (Player.instance != null && Player.instance.inventory != null)
        {
            Player.instance.inventory.OnInventoryChange -= UpdateHintDisplay;
            Player.instance.inventory.OnInventoryChange += UpdateHintDisplay;
        }

        // Cập nhật dòng hint đầu tiên
        UpdateHintDisplay();
    }

    private void OnDestroy()
    {
        if (questManager != null)
        {
            questManager.OnQuestListUpdated -= UpdateHintDisplay;
        }

        if (Player.instance != null && Player.instance.inventory != null)
        {
            Player.instance.inventory.OnInventoryChange -= UpdateHintDisplay;
        }

        if (instance == this)
            instance = null;
    }

    /// <summary>
    /// Tự động cập nhật mục tiêu dựa trên trạng thái Quest hiện tại
    /// </summary>
    public void UpdateHintDisplay()
    {
        if (customHintCoroutine != null)
            return; // Đang hiển thị hint tùy biến tạm thời

        // 1. Nếu đang có Quest đang làm (Active Quest)
        if (questManager != null && questManager.activeQuests != null && questManager.activeQuests.Count > 0)
        {
            var validQuests = questManager.activeQuests.FindAll(q => q != null && q.questDataSO != null);
            if (validQuests.Count == 1)
            {
                AdjustPanelHeight(1);
                var currentQuest = validQuests[0];
                if (currentQuest.CanGetReward())
                {
                    string npcName = GetQuestGiverNpcName(currentQuest.questDataSO);
                    ShowHint(
                        "NHIỆM VỤ HOÀN THÀNH",
                        $"📍 Bạn đã hoàn thành <b>'{currentQuest.questDataSO.questName}'</b>!\n" +
                        $"Quay về gặp <b>{npcName}</b> tại Làng để nhận thưởng [F].",
                        new Color(0.2f, 1f, 0.4f)
                    );
                }
                else
                {
                    string progressText = $"({currentQuest.GetCurrentAmount()}/{currentQuest.questDataSO.requiredAmount})";
                    string targetId = currentQuest.questDataSO.questTargetId;
                    var (directionText, combatTip) = GetQuestGuideInfo(targetId, currentQuest.questDataSO);

                    ShowHint(
                        $"{currentQuest.questDataSO.questName.ToUpper()} <color=#FFD700>{progressText}</color>",
                        $"📍 <b>Hướng đi:</b> {directionText}\n⚔️ <b>Cách đánh/Mẹo:</b> <color=#CFD8DC>{combatTip}</color>",
                        new Color(1f, 0.85f, 0.2f)
                    );
                }
                return;
            }
            else if (validQuests.Count >= 2)
            {
                ShowMultiQuestHint(validQuests);
                return;
            }
        }

        // 2. Nếu chưa nhận Quest nào: Xem tiến trình cốt truyện
        if (SaveManager.instance != null && SaveManager.instance.GetGameData() != null)
        {
            var data = SaveManager.instance.GetGameData();
            bool isVillage = SceneManager.GetActiveScene().name.Contains("Village");

            if (data.hasWatchedIntroComic || isVillage)
            {
                AdjustPanelHeight(1);
                int completedCount = data.completedQuests != null ? data.completedQuests.Count : 0;
                if (questManager != null && questManager.completedQuests != null)
                    completedCount = Mathf.Max(completedCount, questManager.completedQuests.Count);

                if (completedCount == 0)
                {
                    ShowHint(
                        "MỤC TIÊU CỐT TRUYỆN",
                        "📍 <b>Hướng đi:</b> Gặp <b>Joanna</b> tại tiền đồn trung tâm Làng để nhận nhiệm vụ đầu tiên.\n" +
                        "💡 <b>Mẹo:</b> Nhấn [F] để nhận nhiệm vụ, hoặc [T] để mở bản đồ Dịch Chuyển.",
                        new Color(1f, 0.85f, 0.2f)
                    );
                }
                else if (completedCount >= 11)
                {
                    ShowHint(
                        "CHIẾN CÔNG HOÀN TẤT",
                        "🏆 Bạn đã hoàn thành xuất sắc tất cả nhiệm vụ bảo vệ Tiền đồn và Vương quốc!",
                        new Color(0.2f, 1f, 0.4f)
                    );
                }
                else
                {
                    ShowHint(
                        "MỤC TIÊU KẾ TIẾP",
                        "📍 <b>Hướng đi:</b> Quay về gặp <b>Joanna</b> (Tiền đồn Làng) để nhận nhiệm vụ kế tiếp.\n" +
                        "💡 <b>Mẹo:</b> Sử dụng phím tắt [T] Dịch Chuyển để di chuyển về Làng nhanh chóng.",
                        new Color(1f, 0.85f, 0.2f)
                    );
                }
                return;
            }
        }

        // Nếu mới vào chưa qua comic hoặc không có mục tiêu
        HideHint();
    }

    /// <summary>
    /// Lấy tên đầy đủ của NPC giao / nhận thưởng nhiệm vụ
    /// </summary>
    public static string GetQuestGiverNpcName(QuestDataSO questSO)
    {
        if (questSO == null) return "Joanna";

        string qName = questSO.questName ?? "";
        string qId = questSO.questTargetId ?? "";
        string assetName = questSO.name ?? "";

        // 1. Thương nhân Bob
        if (qId == "merchant_bob" ||
            assetName.Contains("Merchant Bob") ||
            qName.Contains("Thu Hồi Phôi Sắt") ||
            qName.Contains("Rắc Rối Thuốc Hồi Máu") ||
            assetName.Contains("Potion Commotion") ||
            assetName.Contains("Iron Recovery") ||
            questSO.rewardType == RewardType.Merchant)
        {
            return "Bob";
        }

        // 2. Nhà Giả Kim Eleonore
        if (qId == "eleonore_witch" ||
            assetName.Contains("Alchemist Eleonore") ||
            qName.Contains("Thạch Slime Giả Kim") ||
            qName.Contains("Lõi Độc Côn Trùng") ||
            assetName.Contains("Slime Jelly") ||
            assetName.Contains("Toxic Core"))
        {
            return "Eleonore";
        }

        // 3. Thợ rèn Joe
        if (qId == "blacksmith_joe" ||
            assetName.Contains("Blacksmith") ||
            qName.Contains("Rèn Đúc Vũ Khí") ||
            assetName.Contains("Dangerous to Go Alone") ||
            questSO.rewardType == RewardType.Blacksmith)
        {
            return "Joe";
        }

        // 4. Mặc định: Chỉ huy Joanna
        return "Joanna";
    }

    /// <summary>
    /// Tra cứu Hướng đi chi tiết và Mẹo chiến đấu cho từng loại quái nhiệm vụ
    /// </summary>
    private (string direction, string combatTip) GetQuestGuideInfo(string targetId, QuestDataSO questSO = null)
    {
        if (questSO != null && questSO.questType == QuestType.Delivery)
        {
            string itemName = questSO.itemToDeliver != null ? questSO.itemToDeliver.itemName : "Thuốc Hồi Máu";
            return (
                $"Thu thập đủ {questSO.requiredAmount} {itemName}. Có thể mua tại chòi Nhà Giả Kim Eleonore, hoặc tự chế tạo / đánh quái nhặt.",
                $"Khi gom đủ {questSO.requiredAmount} {itemName}, quay lại gặp Thương nhân Bob [F] để giao hàng và nhận thưởng!"
            );
        }

        switch (targetId)
        {
            case "merchant_bob":
                return (
                    "Đến góc chợ Làng (1_Village & Camp), tìm gặp Thương nhân Bob.",
                    "Nhấn phím [F] để trò chuyện, mua sắm trang bị và nhận vật phẩm phòng thân."
                );
            case "eleonore_witch":
                return (
                    "Đến chòi thảo mộc ở Làng (1_Village & Camp), tìm gặp Nhà Giả Kim Eleonore.",
                    "Nhấn phím [F] để trò chuyện, tìm hiểu về độc dược và nhận thuốc hồi phục."
                );
            case "blacksmith_joe":
                return (
                    "Đến lò rèn ở Làng (1_Village & Camp), tìm gặp Thợ rèn Joe.",
                    "Nhấn phím [F] để trò chuyện, mở hòm đồ cất trữ và nhận thanh kiếm khởi đầu."
                );
            case "Material data - Slime jelly":
                return (
                    "Dịch chuyển [T] sang Map 2 (Rừng Xanh), diệt Slime thu thập 4 Thạch Slime.",
                    "Đem Thạch Slime về nộp cho Nhà Giả Kim Eleonore để nhận bình thuốc hồi phục."
                );
            case "enemy_sicklezeep":
                return (
                    "Dịch chuyển [T] sang Map 3 (Đầm Lầy), tiêu diệt Bọ Sicklezeep.",
                    "Bọ lưỡi hái di chuyển lắt léo và phun axit. Giữ cự ly và lướt né axit để dồn sát thương!"
                );
            case "enemy_knight_footman":
                return (
                    "Dịch chuyển [T] sang Map 5 (Ngục Lâu Đài), tiêu diệt lính gác Knight Footman.",
                    "Lính cầm kiếm chém trực diện. Đợi hắn vung kiếm thì bước lùi hoặc phản đòn để trừng phạt!"
                );
            case "enemy_slime":
                return (
                    "Dịch chuyển [T] sang Map 2 (Rừng Xanh), Slime ở các bãi cỏ quanh bìa rừng.",
                    "Cận chiến, có chiêu nảy đè. Áp sát chém liên hoàn ngắt nhịp hoặc nhảy lên né đập đất!"
                );
            case "enemy_archerElf":
                return (
                    "Map 2 (Rừng Xanh), tiến sâu vào rừng rậm, chú ý các lùm cây trên cao.",
                    "Bắn tên tầm xa liên tục. Canh lúc hắn vừa giương cung để Lướt (Dash) áp sát dồn dame!"
                );
            case "enemy_mage":
                return (
                    "Dịch chuyển [T] sang Map 3 (Đầm Lầy) hoặc Map 4 (Hang Động).",
                    "Bắn cầu ma pháp và bẫy gai. Giữ cự ly né phép, khi hắn hồi chiêu thì lao vào chém phá thế!"
                );
            case "enemy_skeleton":
                return (
                    "Dịch chuyển [T] sang Map 4 (Hang Động) - hầm mỏ sâu dưới lòng đất.",
                    "Chém kiếm nhanh có khiên. Canh đòn vung tay của hắn để Phản Đòn (Parry/Counter) gây choáng!"
                );
            case "enemy_knight_cref":
                return (
                    "Dịch chuyển [T] sang Map 5 (Ngục Lâu Đài), đại sảnh tầng hầm.",
                    "Hiệp sĩ giáp sắt khiên dày. Vòng ra sau lưng hoặc dùng Kỹ Năng Đòn Nặng phá vỡ thế thủ!"
                );
            case "enemy_severed_fang":
                return (
                    "Dịch chuyển [T] sang Map 6 (Hỏa Ngục), quanh các hồ dung nham.",
                    "Quái thú cào cấu cực nhanh. Né chuỗi 3 đòn cào của nó rồi lập tức phản kích!"
                );
            case "enemy_samuraizipp":
                return (
                    "Dịch chuyển [T] sang Map 7 (Vùng Đất Chết), bãi hoang mạc cằn cỗi.",
                    "Lướt kiếm thần tốc. Tránh đánh lúc hắn vào thế thủ, hãy né chiêu lướt rồi trừng phạt!"
                );
            case "enemy_crimson_blade":
                return (
                    "Gặp Joanna tại Tiền đồn Làng [F] ➔ Chọn 'Khiêu chiến Boss' ➔ Chọn '1. Crimson Blade' để vào Đấu Trường (BattleScene).",
                    "Kiếm Sĩ Huyết Ảnh chém nhanh và có thế thủ phản đòn (Blood Guard). Tránh đánh khi hắn giơ kiếm thủ, canh lúc hắn vừa chém xong để phản kích!"
                );
            case "enemy_arch_demon":
                return (
                    "Gặp Joanna tại Tiền đồn Làng [F] ➔ Chọn 'Khiêu chiến Boss' ➔ Chọn '8. ARCH DEMON' để vào Đấu Trường (BattleScene).",
                    "Chúa quỷ quét lửa diện rộng. Tận dụng tối đa I-frame của đòn lướt để né và dồn skill kết liễu!"
                );
            case "enemy_reaper":
                return (
                    "Gặp Joanna tại Tiền đồn Làng [F] ➔ Chọn 'Khiêu chiến Boss' ➔ Chọn '7. Death Reaper' để vào Đấu Trường (BattleScene).",
                    "Lưỡi hái vung chém diện rộng và tàng hình áp sát. Lướt né đúng nhịp khi hắn hiện hình để phản kích!"
                );
            default:
                return (
                    "Mở bản đồ hoặc dịch chuyển [T] đến khu vực mục tiêu.",
                    "Quan sát hành vi và động tác của mục tiêu để né tránh và phản kích kịp thời."
                );
        }
    }

    private static readonly string[] questColorHexes = new string[]
    {
        "#FFD700", // Vàng Kim (Khớp màu con trỏ 1)
        "#40E0D0", // Xanh Lam Ngọc (Khớp màu con trỏ 2)
        "#7CFC00", // Xanh Lục (Khớp màu con trỏ 3)
        "#FFA500"  // Cam Hổ Phách (Khớp màu con trỏ 4)
    };

    private void ShowMultiQuestHint(List<QuestData> quests)
    {
        int total = quests.Count;
        int readyCount = quests.FindAll(q => q.CanGetReward()).Count;

        string title = readyCount > 0
            ? $"🎯 NHIỆM VỤ ({readyCount}/{total} HOÀN THÀNH)"
            : $"🎯 MỤC TIÊU NHIỆM VỤ ({total})";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < total; i++)
        {
            var q = quests[i];
            string colorHex = questColorHexes[i % questColorHexes.Length];
            string qName = q.questDataSO.questName;

            if (q.CanGetReward())
            {
                string npcName = GetQuestGiverNpcName(q.questDataSO);
                sb.AppendLine($"<color={colorHex}><b>▶ [{i + 1}] {qName}:</b></color> <color=#00FF7F>[Xong] Gặp {npcName} nhận thưởng!</color>");
            }
            else
            {
                int curAmount = q.GetCurrentAmount();
                string progress = $"({curAmount}/{q.questDataSO.requiredAmount})";
                var (dir, _) = GetQuestGuideInfo(q.questDataSO.questTargetId, q.questDataSO);
                sb.AppendLine($"<color={colorHex}><b>▶ [{i + 1}] {qName} <color=#FFD700>{progress}</color>:</b></color>");
                sb.AppendLine($"   📍 <color=#E0E0E0>{dir}</color>");
            }
        }

        ShowHint(title, sb.ToString().TrimEnd(), new Color(1f, 0.88f, 0.25f));
        AdjustPanelHeight(total);
    }

    private void AdjustPanelHeight(int questCount)
    {
        if (hintPanel == null) return;
        RectTransform pRect = hintPanel.GetComponent<RectTransform>();
        if (pRect != null)
        {
            // Panel rộng 440px và chiều cao co giãn thoải mái theo số lượng nhiệm vụ
            float targetHeight = questCount <= 1 ? 104f : (68f + questCount * 44f);
            pRect.sizeDelta = new Vector2(440f, targetHeight);
            if (bodyText != null)
            {
                RectTransform bRect = bodyText.rectTransform;
                bRect.sizeDelta = new Vector2(-24f, targetHeight - 34f);
            }
        }
    }

    /// <summary>
    /// Hiển thị Hint với hiệu ứng chuyển đổi mượt mà
    /// </summary>
    public void ShowHint(string title, string body, Color titleColor)
    {
        EnsureUIExists();
        if (hintPanel == null) return;

        string combinedKey = title + body;
        if (combinedKey == lastDisplayedText && hintPanel.activeSelf)
            return; // Tránh nhấp nháy lại nếu nội dung không đổi

        lastDisplayedText = combinedKey;
        hintPanel.SetActive(true);

        if (titleText != null)
        {
            titleText.text = title;
            titleText.color = titleColor;
        }

        if (bodyText != null)
        {
            bodyText.text = body;
        }

        // Hiệu ứng Pop nhẹ để người chơi chú ý
        if (hintPanel.transform != null)
        {
            hintPanel.transform.DOKill();
            hintPanel.transform.localScale = Vector3.one * 0.95f;
            hintPanel.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
        }

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.alpha = 0.5f;
            canvasGroup.DOFade(1f, 0.25f);
        }
    }

    /// <summary>
    /// Cho phép hiển thị một gợi ý tạm thời (ví dụ khi giải đố hoặc vào khu vực bí mật)
    /// </summary>
    public void SetCustomHint(string title, string body, float duration = 5f)
    {
        if (customHintCoroutine != null)
            StopCoroutine(customHintCoroutine);

        customHintCoroutine = StartCoroutine(CustomHintCo(title, body, duration));
    }

    private IEnumerator CustomHintCo(string title, string body, float duration)
    {
        ShowHint(title, body, new Color(0.3f, 0.85f, 1f));
        yield return new WaitForSeconds(duration);

        customHintCoroutine = null;
        UpdateHintDisplay();
    }

    public void HideHint()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);
        lastDisplayedText = "";
    }

    /// <summary>
    /// Tự động khởi tạo giao diện nếu chưa kéo thả trong Inspector (Plug & Play)
    /// </summary>
    private void EnsureUIExists()
    {
        if (hintPanel != null && titleText != null && bodyText != null)
            return;

        // Tìm Canvas In-Game
        Transform parentTransform = transform;
        if (GetComponentInParent<Canvas>() == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            Transform inGameUI = canvas.transform.Find("UI_InGame");
            parentTransform = inGameUI != null ? inGameUI : canvas.transform;
        }

        // Kiểm tra xem đã có object con tên HUD_ObjectiveHintPanel chưa
        Transform existing = parentTransform.Find("HUD_ObjectiveHintPanel");
        if (existing != null)
        {
            hintPanel = existing.gameObject;
            titleText = existing.Find("Title_Text")?.GetComponent<TextMeshProUGUI>();
            bodyText = existing.Find("Body_Text")?.GetComponent<TextMeshProUGUI>();
            canvasGroup = existing.GetComponent<CanvasGroup>();

            RectTransform pRect = hintPanel.GetComponent<RectTransform>();
            if (pRect != null && pRect.sizeDelta.x < 440f)
                pRect.sizeDelta = new Vector2(440f, pRect.sizeDelta.y);

            Transform handTransform = existing.Find("Hand_Icon");
            if (handTransform == null)
            {
                GameObject handObj = new GameObject("Hand_Icon");
                handObj.transform.SetParent(hintPanel.transform, false);
                RectTransform hRect = handObj.AddComponent<RectTransform>();
                hRect.anchorMin = new Vector2(0f, 1f);
                hRect.anchorMax = new Vector2(0f, 1f);
                hRect.pivot = new Vector2(0f, 1f);
                hRect.anchoredPosition = new Vector2(10f, -8f);
                hRect.sizeDelta = new Vector2(20f, 20f);

                handIcon = handObj.AddComponent<Image>();
                if (handPointerSprite == null)
                {
#if UNITY_EDITOR
                    handPointerSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Graphics/UI/HandPointer_Pixel.png");
#endif
                }
                if (handPointerSprite != null)
                {
                    handIcon.sprite = handPointerSprite;
                }
                hRect.DOKill();
                hRect.DOAnchorPosX(14f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }
            else
            {
                handIcon = handTransform.GetComponent<Image>();
                if (handPointerSprite == null)
                {
#if UNITY_EDITOR
                    handPointerSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Graphics/UI/HandPointer_Pixel.png");
#endif
                }
                if (handPointerSprite != null && handIcon.sprite == null)
                {
                    handIcon.sprite = handPointerSprite;
                }
                RectTransform hRect = handIcon.rectTransform;
                hRect.DOKill();
                hRect.anchoredPosition = new Vector2(10f, -8f);
                hRect.DOAnchorPosX(14f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }

            if (titleText != null)
            {
                RectTransform tRect = titleText.rectTransform;
                tRect.anchorMin = new Vector2(0f, 1f);
                tRect.anchorMax = new Vector2(1f, 1f);
                tRect.pivot = new Vector2(0f, 1f);
                tRect.anchoredPosition = new Vector2(34f, -8f);
                tRect.sizeDelta = new Vector2(-44f, 20f);
            }

            if (bodyText != null)
            {
                RectTransform bRect = bodyText.rectTransform;
                bRect.anchorMin = new Vector2(0f, 1f);
                bRect.anchorMax = new Vector2(1f, 1f);
                bRect.pivot = new Vector2(0f, 1f);
                bRect.anchoredPosition = new Vector2(12f, -30f);
                float curH = pRect != null ? pRect.sizeDelta.y : 102f;
                bRect.sizeDelta = new Vector2(-24f, curH - 34f);
                bodyText.fontSize = 11.5f;
                bodyText.enableWordWrapping = true;
                return;
            }
        }

        // Tạo khung Panel chứa Hint
        GameObject panelObj = new GameObject("HUD_ObjectiveHintPanel");
        panelObj.transform.SetParent(parentTransform, false);

        RectTransform rect = panelObj.AddComponent<RectTransform>();
        // Neo góc trên bên trái màn hình (dưới thanh máu)
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(30f, -85f);
        rect.sizeDelta = new Vector2(440f, 104f);

        // Nền tối bán trong suốt chuẩn Pixel RPG
        Image bgImage = panelObj.AddComponent<Image>();
        bgImage.color = new Color(0.06f, 0.08f, 0.12f, 0.88f);

        // Viền bóng
        Outline outline = panelObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.4f, 0.45f, 0.55f, 0.45f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        canvasGroup = panelObj.AddComponent<CanvasGroup>();

        // Icon Bàn tay chỉ ngón Pixel Art
        GameObject newHandObj = new GameObject("Hand_Icon");
        newHandObj.transform.SetParent(panelObj.transform, false);
        RectTransform newHandRect = newHandObj.AddComponent<RectTransform>();
        newHandRect.anchorMin = new Vector2(0f, 1f);
        newHandRect.anchorMax = new Vector2(0f, 1f);
        newHandRect.pivot = new Vector2(0f, 1f);
        newHandRect.anchoredPosition = new Vector2(10f, -8f);
        newHandRect.sizeDelta = new Vector2(20f, 20f);

        handIcon = newHandObj.AddComponent<Image>();
        if (handPointerSprite == null)
        {
#if UNITY_EDITOR
            handPointerSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Graphics/UI/HandPointer_Pixel.png");
#endif
        }
        if (handPointerSprite != null)
        {
            handIcon.sprite = handPointerSprite;
        }
        newHandRect.DOKill();
        newHandRect.DOAnchorPosX(14f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

        // Text Tiêu đề
        GameObject titleObj = new GameObject("Title_Text");
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.anchoredPosition = new Vector2(34f, -8f);
        titleRect.sizeDelta = new Vector2(-44f, 20f);

        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.fontSize = 13f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Left;
        titleText.color = new Color(1f, 0.85f, 0.2f);
        titleText.text = "MỤC TIÊU";

        // Text Nội dung
        GameObject bodyObj = new GameObject("Body_Text");
        bodyObj.transform.SetParent(panelObj.transform, false);
        RectTransform bodyRect = bodyObj.AddComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 1f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.pivot = new Vector2(0f, 1f);
        bodyRect.anchoredPosition = new Vector2(12f, -30f);
        bodyRect.sizeDelta = new Vector2(-24f, 70f);

        bodyText = bodyObj.AddComponent<TextMeshProUGUI>();
        bodyText.fontSize = 11.5f;
        bodyText.alignment = TextAlignmentOptions.TopLeft;
        bodyText.color = new Color(0.95f, 0.95f, 0.95f);
        bodyText.enableWordWrapping = true;
        bodyText.text = "Đang tải mục tiêu...";

        hintPanel = panelObj;
    }
}
