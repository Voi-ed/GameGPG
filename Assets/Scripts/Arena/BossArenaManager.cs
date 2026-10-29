using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossArenaManager : MonoBehaviour
{
    [System.Serializable]
    public class BossArenaEntry
    {
        public string bossId;
        public string displayName;
        public GameObject bossPrefab;
        public bool isFinalBoss;
    }

    public static BossArenaManager instance { get; private set; }

    /// <summary>
    /// ID của Boss đã được Player chọn từ UI Khiêu Chiến
    /// </summary>
    public static string SelectedBossId { get; set; } = "CrimsonBlade";

    [Header("Boss Entries & Spawning")]
    [Tooltip("Danh sách cấu hình các Boss có thể khiêu chiến trong Đấu Trường")]
    [SerializeField] private BossArenaEntry[] availableBosses;
    [Tooltip("Vị trí xuất hiện của Boss")]
    [SerializeField] private Transform bossSpawnPoint;
    [Tooltip("Vị trí xuất hiện của Player khi vào Đấu Trường (tuỳ chọn)")]
    [SerializeField] private Transform playerSpawnPoint;

    [Header("End-Fight Banners & Portals")]
    [Tooltip("Cổng dịch chuyển trở về Camp sau khi hạ Boss")]
    [SerializeField] private GameObject returnPortal;
    [Tooltip("Banner UI hiển thị khi hạ Boss thông thường")]
    [SerializeField] private GameObject victoryBanner;
    [Tooltip("Banner UI hiển thị khi đánh bại Final Boss (ArchDemon) - PHÁ ĐẢO GAME")]
    [SerializeField] private GameObject winGameBanner;

    [Header("Boss Health Bar (Bottom Center)")]
    [Tooltip("Thanh máu Boss khổng lồ ở cạnh dưới màn hình. Nếu để trống, hệ thống sẽ tự động tạo giao diện chuẩn Souls-like.")]
    [SerializeField] private UI_BossHealthBar bossHealthBar;

    [Header("Scene Navigation")]
    [SerializeField] private string campSceneName = "CampScene";

    private GameObject currentSpawnedBoss;
    private Entity_Health currentBossHealth;
    private bool isFinalBossFight;
    private bool fightCompleted;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InitializeArenaElements();
        StartCoroutine(SetupArenaCo());
    }

    private void InitializeArenaElements()
    {
        if (returnPortal != null)
            returnPortal.SetActive(false);

        if (victoryBanner != null)
            victoryBanner.SetActive(false);

        if (winGameBanner != null)
            winGameBanner.SetActive(false);

        if (bossHealthBar != null)
            bossHealthBar.gameObject.SetActive(false);
    }

    private IEnumerator SetupArenaCo()
    {
        // Chờ 1 frame để scene và các manager (Player, GameManager) load đầy đủ
        yield return null;

        Player player = Player.instance != null ? Player.instance : FindFirstObjectByType<Player>();

        // Định vị Player nếu có chỉ định playerSpawnPoint
        if (player != null && playerSpawnPoint != null)
        {
            player.TeleportPlayer(playerSpawnPoint.position);
            if (CameraCtrl.instance != null)
                CameraCtrl.instance.SnapToTarget(player.transform);
        }

        SpawnSelectedBoss();

        // Đếm ngược 3... 2... 1... rồi mới cho đánh
        yield return StartCoroutine(CountdownCo(player));
    }

    private IEnumerator CountdownCo(Player player)
    {
        // Tạm khóa điều khiển của Player
        if (player != null && player.input != null)
            player.input.Player.Disable();

        // Tạm dừng Boss để đứng yên quan sát
        Enemy bossEnemy = currentSpawnedBoss != null ? currentSpawnedBoss.GetComponent<Enemy>() : null;
        if (bossEnemy != null)
        {
            bossEnemy.canChasePlayer = false;
            bossEnemy.enabled = false;
        }

        // Chờ thêm 0.3s cho hiệu ứng fade màn hình chuyển cảnh ổn định
        yield return new WaitForSeconds(0.4f);

        Canvas canvas = GetMainScreenCanvas();
        GameObject countdownGo = null;
        TextMeshProUGUI countdownText = null;

        if (canvas != null)
        {
            countdownGo = new GameObject("CountdownUI", typeof(RectTransform));
            countdownGo.transform.SetParent(canvas.transform, false);

            RectTransform rect = countdownGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(600, 200);
            rect.anchoredPosition = new Vector2(0, 40);

            countdownText = countdownGo.AddComponent<TextMeshProUGUI>();
            countdownText.alignment = TextAlignmentOptions.Center;
            countdownText.fontStyle = FontStyles.Bold;
        }

        string[] countdownSteps = { "3", "2", "1", "CHIẾN ĐẤU!" };
        Color[] stepColors = {
            new Color(1f, 0.35f, 0.25f, 1f), // 3: Đỏ cam
            new Color(1f, 0.75f, 0.2f, 1f),  // 2: Vàng cam
            new Color(1f, 0.95f, 0.3f, 1f),  // 1: Vàng sáng
            new Color(1f, 0.2f, 0.2f, 1f)     // CHIẾN ĐẤU!: Đỏ rực
        };

        for (int i = 0; i < countdownSteps.Length; i++)
        {
            if (countdownText != null)
            {
                countdownText.text = countdownSteps[i];
                countdownText.color = stepColors[i];
                countdownText.fontSize = (i == countdownSteps.Length - 1) ? 72 : 90;
            }

            float duration = (i == countdownSteps.Length - 1) ? 0.8f : 1.0f;
            yield return new WaitForSeconds(duration);
        }

        if (countdownGo != null)
            Destroy(countdownGo);

        // Mở khoá điều khiển Player và kích hoạt Boss tấn công
        if (player != null && player.input != null)
            player.input.Player.Enable();

        if (bossEnemy != null)
        {
            bossEnemy.enabled = true;
            bossEnemy.canChasePlayer = true;
        }

        // Kích hoạt hiệu ứng Intro thanh máu Boss chạy đầy từ 0 -> 100%
        if (bossHealthBar != null)
            bossHealthBar.PlayIntroAnimation();

        Debug.Log("[BossArenaManager] TRẬN CHIẾN CHÍNH THỨC BẮT ĐẦU!");
    }

    private void SpawnSelectedBoss()
    {
        if (string.IsNullOrEmpty(SelectedBossId))
            SelectedBossId = "CrimsonBlade";

        BossArenaEntry selectedEntry = null;

        if (availableBosses != null && availableBosses.Length > 0)
        {
            foreach (var entry in availableBosses)
            {
                if (string.Equals(entry.bossId, SelectedBossId, System.StringComparison.OrdinalIgnoreCase))
                {
                    selectedEntry = entry;
                    break;
                }
            }

            // Fallback: nếu không khớp ID thì lấy con đầu tiên
            if (selectedEntry == null)
                selectedEntry = availableBosses[0];
        }

        if (selectedEntry == null || selectedEntry.bossPrefab == null)
        {
            Debug.LogWarning($"[BossArenaManager] Không tìm thấy Prefab cho Boss '{SelectedBossId}'. Hãy gán bossEntries trong Inspector của BossArenaManager!");
            return;
        }

        isFinalBossFight = selectedEntry.isFinalBoss;

        Vector3 spawnPos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
        currentSpawnedBoss = Instantiate(selectedEntry.bossPrefab, spawnPos, Quaternion.identity);

        currentBossHealth = currentSpawnedBoss.GetComponent<Entity_Health>();
        if (currentBossHealth != null)
        {
            currentBossHealth.OnDeath += OnBossDefeated;

            // Khởi tạo thanh máu Boss to ở cạnh dưới màn hình (Bottom Center)
            SetupBossHealthBar(currentBossHealth, selectedEntry.displayName);
        }

        // Bổ sung Coroutine theo dõi fallback phòng khi Boss chết bằng cách Destroy
        StartCoroutine(WatchBossLifecycleCo(currentSpawnedBoss));

        Debug.Log($"[BossArenaManager] Đã triệu hồi Boss '{selectedEntry.displayName}' (ID: {selectedEntry.bossId}) tại vị trí {spawnPos}!");
    }

    private IEnumerator WatchBossLifecycleCo(GameObject bossObj)
    {
        while (bossObj != null && !fightCompleted)
        {
            if (currentBossHealth != null && currentBossHealth.isDead)
            {
                OnBossDefeated();
                yield break;
            }
            yield return new WaitForSeconds(0.25f);
        }

        // Nếu boss object đã bị destroy nhưng chưa kích hoạt OnBossDefeated
        if (!fightCompleted)
        {
            OnBossDefeated();
        }
    }

    private void SetupBossHealthBar(Entity_Health health, string displayName)
    {
        if (bossHealthBar == null)
        {
            bossHealthBar = FindFirstObjectByType<UI_BossHealthBar>();

            // Dọn dẹp thanh máu BottomCenter cũ nếu còn sót lại trong Scene
            if (bossHealthBar != null && bossHealthBar.gameObject.name.Contains("BottomCenter"))
            {
                Destroy(bossHealthBar.gameObject);
                bossHealthBar = null;
            }

            if (bossHealthBar == null)
            {
                Canvas mainCanvas = GetMainScreenCanvas();
                if (mainCanvas != null)
                {
                    bossHealthBar = UI_BossHealthBar.CreateTopCenterBar(mainCanvas.transform);
                }
            }
        }

        if (bossHealthBar != null)
        {
            bossHealthBar.Setup(health, displayName);
        }
    }

    /// <summary>
    /// Kích hoạt khi Boss bị tiêu diệt
    /// </summary>
    public void OnBossDefeated()
    {
        if (fightCompleted)
            return;

        fightCompleted = true;
        Debug.Log($"[BossArenaManager] BOSS '{SelectedBossId}' ĐÃ BỊ TIÊU DIỆT! Mở cổng dịch chuyển và ghi nhận tiến trình.");

        // Ẩn thanh máu Boss khi trận đấu kết thúc
        if (bossHealthBar != null)
            bossHealthBar.HideHealthBar();

        // Đánh dấu boss đã bị tiêu diệt vào dữ liệu game
        MarkBossDefeated(SelectedBossId);

        // Kích hoạt Cổng Dịch Chuyển để người chơi quay về
        if (returnPortal != null)
            returnPortal.SetActive(true);

        if (isFinalBossFight)
        {
            HandleGameWin();
        }
        else
        {
            HandleStandardVictory();
        }
    }

    private void HandleStandardVictory()
    {
        if (victoryBanner == null)
        {
            var bannerComp = FindFirstObjectByType<UI_BossVictoryBanner>(FindObjectsInactive.Include);
            if (bannerComp != null)
                victoryBanner = bannerComp.gameObject;
        }

        if (victoryBanner != null)
        {
            if (victoryBanner.TryGetComponent<UI_BossVictoryBanner>(out var bannerComp))
            {
                bannerComp.ShowVictory("⚔ CHIẾN THẮNG BOSS! ⚔", "Cổng dịch chuyển trở về đã được kích hoạt.", false, ReturnToCamp);
            }
            else
            {
                victoryBanner.SetActive(true);
            }
        }
        else
        {
            // Tự tạo Victory Banner trên màn hình nếu Inspector chưa kéo thả
            CreateFallbackBanner("⚔ CHIẾN THẮNG BOSS! ⚔", "Cổng dịch chuyển trở về đã được kích hoạt.", false);
        }
    }

    private void HandleGameWin()
    {
        Debug.Log("★ [BossArenaManager] CHÚC MỪNG BẠN ĐÃ PHÁ ĐẢO GAME! ĐẠI MA VƯƠNG ARCHDEMON ĐÃ BỊ TIÊU DIỆT! ★");

        if (winGameBanner == null)
        {
            var bannerComp = FindFirstObjectByType<UI_BossVictoryBanner>(FindObjectsInactive.Include);
            if (bannerComp != null)
                winGameBanner = bannerComp.gameObject;
        }

        if (winGameBanner != null)
        {
            if (winGameBanner.TryGetComponent<UI_BossVictoryBanner>(out var bannerComp))
            {
                bannerComp.ShowVictory("★ CHIẾN THẮNG HOÀN TOÀN - PHÁ ĐẢO GAME ★", "Bạn đã đánh bại Đại Ma Vương ArchDemon và bảo vệ thế giới!", true, ReturnToCamp);
            }
            else
            {
                winGameBanner.SetActive(true);
            }
        }
        else
        {
            // Tự tạo Win Game Banner tuyệt đẹp trên màn hình nếu Inspector chưa kéo thả
            CreateFallbackBanner("★ CHIẾN THẮNG HOÀN TOÀN - PHÁ ĐẢO GAME ★", "Bạn đã đánh bại Đại Ma Vương ArchDemon và bảo vệ thế giới!", true);
        }
    }

    private void CreateFallbackBanner(string title, string description, bool isWinGame)
    {
        Canvas canvas = GetMainScreenCanvas();
        if (canvas == null)
            return;

        GameObject bannerRoot = new GameObject("BossVictoryBannerFallback", typeof(RectTransform));
        bannerRoot.transform.SetParent(canvas.transform, false);

        RectTransform rect = bannerRoot.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.7f);
        rect.anchorMax = new Vector2(0.5f, 0.7f);
        rect.sizeDelta = new Vector2(600, 160);
        rect.anchoredPosition = Vector2.zero;

        Image bg = bannerRoot.AddComponent<Image>();
        bg.color = isWinGame ? new Color(0.1f, 0.05f, 0.15f, 0.95f) : new Color(0.1f, 0.12f, 0.18f, 0.95f);

        // Title
        GameObject titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(bannerRoot.transform, false);
        RectTransform tRect = titleGo.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0, 0.5f);
        tRect.anchorMax = new Vector2(1, 1);
        tRect.offsetMin = new Vector2(10, 0);
        tRect.offsetMax = new Vector2(-10, -10);

        TextMeshProUGUI tText = titleGo.GetComponent<TextMeshProUGUI>();
        tText.text = title;
        tText.fontSize = isWinGame ? 26 : 22;
        tText.fontStyle = FontStyles.Bold;
        tText.alignment = TextAlignmentOptions.Center;
        tText.color = isWinGame ? new Color(1f, 0.85f, 0.2f) : new Color(0.4f, 1f, 0.4f);

        // Subtitle / Description
        GameObject subGo = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        subGo.transform.SetParent(bannerRoot.transform, false);
        RectTransform sRect = subGo.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0, 0);
        sRect.anchorMax = new Vector2(1, 0.5f);
        sRect.offsetMin = new Vector2(10, 10);
        sRect.offsetMax = new Vector2(-10, 0);

        TextMeshProUGUI sText = subGo.GetComponent<TextMeshProUGUI>();
        sText.text = description;
        sText.fontSize = 15;
        sText.alignment = TextAlignmentOptions.Center;
        sText.color = Color.white;

        // Nút bấm Quay về Làng trên Banner
        GameObject btnGo = new GameObject("ReturnBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(bannerRoot.transform, false);
        RectTransform bRect = btnGo.GetComponent<RectTransform>();
        bRect.anchorMin = new Vector2(0.35f, -0.4f);
        bRect.anchorMax = new Vector2(0.65f, -0.05f);
        bRect.offsetMin = Vector2.zero;
        bRect.offsetMax = Vector2.zero;

        Image bImg = btnGo.GetComponent<Image>();
        bImg.color = isWinGame ? new Color(0.85f, 0.65f, 0.15f, 1f) : new Color(0.2f, 0.6f, 0.3f, 1f);

        GameObject bTxtGo = new GameObject("BtnTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        bTxtGo.transform.SetParent(btnGo.transform, false);
        RectTransform btRect = bTxtGo.GetComponent<RectTransform>();
        btRect.anchorMin = Vector2.zero;
        btRect.anchorMax = Vector2.one;
        btRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI btText = bTxtGo.GetComponent<TextMeshProUGUI>();
        btText.text = isWinGame ? "★ HOÀN THÀNH GAME ★" : "QUAY VỀ LÀNG";
        btText.fontSize = 15;
        btText.fontStyle = FontStyles.Bold;
        btText.alignment = TextAlignmentOptions.Center;
        btText.color = Color.white;

        Button returnBtn = btnGo.GetComponent<Button>();
        returnBtn.onClick.AddListener(ReturnToCamp);
    }

    /// <summary>
    /// Lấy Screen-Space Canvas chính của game, đảm bảo không bao giờ gắn nhầm vào World-Space Canvas của quái
    /// </summary>
    private Canvas GetMainScreenCanvas()
    {
        if (UI.instance != null)
        {
            Canvas uiCanvas = UI.instance.GetComponent<Canvas>() ?? UI.instance.GetComponentInParent<Canvas>();
            if (uiCanvas != null && uiCanvas.renderMode != RenderMode.WorldSpace)
                return uiCanvas;
        }

        // Lọc tất cả các Canvas trong scene, loại bỏ triệt để Canvas WorldSpace trên đầu quái
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            if (c.renderMode != RenderMode.WorldSpace)
                return c;
        }

        // Tạo ScreenSpaceOverlay Canvas riêng biệt cố định hoàn toàn trên màn hình
        GameObject overlayCanvasGo = new GameObject("ScreenOverlayCanvas_BossArena");
        Canvas newCanvas = overlayCanvasGo.AddComponent<Canvas>();
        newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        newCanvas.sortingOrder = 999;
        overlayCanvasGo.AddComponent<CanvasScaler>();
        overlayCanvasGo.AddComponent<GraphicRaycaster>();
        return newCanvas;
    }

    /// <summary>
    /// Rời Đấu Trường quay về Làng/Camp
    /// </summary>
    public void ReturnToCamp()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ChangeScene(campSceneName, RespawnType.NonSpecific);
        }
    }

    /// <summary>
    /// Bắt đầu vào đấu trường tiêu diệt Boss
    /// </summary>
    public static void StartChallenge(string bossId)
    {
        SelectedBossId = bossId;
        if (GameManager.instance != null)
        {
            GameManager.instance.ChangeScene("BattleScene", RespawnType.NonSpecific);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
        }
    }

    /// <summary>
    /// Kiểm tra xem Boss đã bị tiêu diệt chưa
    /// </summary>
    public static bool IsBossDefeated(string bossId)
    {
        if (string.IsNullOrEmpty(bossId))
            return false;

        if (SaveManager.instance != null && SaveManager.instance.GetGameData() != null)
        {
            var dict = SaveManager.instance.GetGameData().defeatedBosses;
            if (dict != null && dict.TryGetValue(bossId, out bool val))
                return val;
        }

        return PlayerPrefs.GetInt("BossDefeated_" + bossId, 0) == 1;
    }

    /// <summary>
    /// Đánh dấu Boss đã bị tiêu diệt (lưu vào SaveManager và PlayerPrefs)
    /// </summary>
    public static void MarkBossDefeated(string bossId)
    {
        if (string.IsNullOrEmpty(bossId))
            return;

        if (SaveManager.instance != null && SaveManager.instance.GetGameData() != null)
        {
            var dict = SaveManager.instance.GetGameData().defeatedBosses;
            if (dict != null)
            {
                dict[bossId] = true;
                SaveManager.instance.SaveGame();
            }
        }

        PlayerPrefs.SetInt("BossDefeated_" + bossId, 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Đặt lại tiến trình tiêu diệt Boss (dùng cho test/debug)
    /// </summary>
    public static void ResetBossProgress()
    {
        if (SaveManager.instance != null && SaveManager.instance.GetGameData() != null)
        {
            var dict = SaveManager.instance.GetGameData().defeatedBosses;
            if (dict != null)
            {
                dict.Clear();
                SaveManager.instance.SaveGame();
            }
        }

        string[] allBossIds = { "CrimsonBlade", "Seeker", "Satyr", "MartialHero", "KnightMage", "BlackKnight", "Reaper", "ArchDemon" };
        foreach (var id in allBossIds)
        {
            PlayerPrefs.DeleteKey("BossDefeated_" + id);
        }
        PlayerPrefs.Save();
        Debug.Log("[BossArenaManager] Đã đặt lại toàn bộ tiến trình Boss!");
    }
}
