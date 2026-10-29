using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BossHealthBar : MonoBehaviour
{
    [Header("UI Component References")]
    [SerializeField] private Slider mainHealthSlider;
    [SerializeField] private Slider ghostHealthSlider;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private TextMeshProUGUI healthNumbersText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Timings")]
    [SerializeField] private float ghostDelay = 0.3f;
    [SerializeField] private float ghostDuration = 0.45f;
    [SerializeField] private float numberDuration = 0.35f;

    private Entity_Health currentBossHealth;
    private Entity_Stats currentBossStats;
    private Coroutine ghostCoroutine;
    private Coroutine numberCoroutine;
    private Coroutine introCoroutine;
    private Coroutine fadeCoroutine;

    private float displayedHealthValue = 0f;
    private float targetMaxHealth = 1f;

    public void Setup(Entity_Health health, string bossName)
    {
        if (health == null)
            return;

        // Huỷ đăng ký cũ nếu có
        CleanupSubscriptions();

        currentBossHealth = health;
        currentBossStats = health.GetComponent<Entity_Stats>();

        if (bossNameText != null)
        {
            bossNameText.text = string.IsNullOrEmpty(bossName) ? "BOSS" : bossName.ToUpper();
        }

        // Đăng ký event từ Entity_Health
        currentBossHealth.OnHealthUpdate += HandleHealthUpdate;
        currentBossHealth.OnDeath += HandleDeath;

        targetMaxHealth = currentBossStats != null ? currentBossStats.GetMaxHealth() : currentBossHealth.GetCurrentHealth();
        if (targetMaxHealth <= 0) targetMaxHealth = 100f;

        displayedHealthValue = currentBossHealth.GetCurrentHealth();

        // Đặt giá trị ban đầu cho các slider
        float healthPercent = Mathf.Clamp01(displayedHealthValue / targetMaxHealth);
        if (mainHealthSlider != null) mainHealthSlider.value = healthPercent;
        if (ghostHealthSlider != null) ghostHealthSlider.value = healthPercent;

        UpdateHealthNumberText(displayedHealthValue, targetMaxHealth);

        gameObject.SetActive(true);
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Hiệu ứng Intro hào hùng khi trận chiến bắt đầu: chạy từ 0 -> 100% đầy cây máu
    /// </summary>
    public void PlayIntroAnimation()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (introCoroutine != null)
            StopCoroutine(introCoroutine);

        introCoroutine = StartCoroutine(IntroFillCo());
    }

    private IEnumerator IntroFillCo()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (mainHealthSlider != null) mainHealthSlider.value = 0f;
        if (ghostHealthSlider != null) ghostHealthSlider.value = 0f;

        float targetHealth = currentBossHealth != null ? currentBossHealth.GetCurrentHealth() : targetMaxHealth;
        float targetPercent = Mathf.Clamp01(targetHealth / targetMaxHealth);

        float elapsed = 0f;
        float duration = 1.0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease out quad
            float smoothT = t * (2f - t);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t * 1.5f);

            float currentFill = Mathf.Lerp(0f, targetPercent, smoothT);
            if (mainHealthSlider != null) mainHealthSlider.value = currentFill;
            if (ghostHealthSlider != null) ghostHealthSlider.value = currentFill;

            float currentNum = Mathf.Lerp(0f, targetHealth, smoothT);
            UpdateHealthNumberText(currentNum, targetMaxHealth);

            yield return null;
        }

        displayedHealthValue = targetHealth;
        if (mainHealthSlider != null) mainHealthSlider.value = targetPercent;
        if (ghostHealthSlider != null) ghostHealthSlider.value = targetPercent;
        UpdateHealthNumberText(targetHealth, targetMaxHealth);
    }

    private void HandleHealthUpdate()
    {
        if (currentBossHealth == null)
            return;

        float newHp = currentBossHealth.GetCurrentHealth();
        float targetPercent = Mathf.Clamp01(newHp / targetMaxHealth);

        // 1. Cập nhật Main Slider ngay tức thì
        if (mainHealthSlider != null)
        {
            mainHealthSlider.value = targetPercent;
        }

        // 2. Chạy Coroutine Ghost Slider trễ 0.3s rồi lướt theo sau
        if (ghostCoroutine != null)
            StopCoroutine(ghostCoroutine);
        ghostCoroutine = StartCoroutine(GhostBarRoutine(targetPercent));

        // 3. Nhảy số máu mượt mà
        if (numberCoroutine != null)
            StopCoroutine(numberCoroutine);
        numberCoroutine = StartCoroutine(NumberLerpRoutine(newHp));
    }

    private IEnumerator GhostBarRoutine(float targetFill)
    {
        yield return new WaitForSeconds(ghostDelay);

        if (ghostHealthSlider == null)
            yield break;

        float startFill = ghostHealthSlider.value;
        float elapsed = 0f;

        while (elapsed < ghostDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / ghostDuration);
            // Ease out cubic
            float ease = 1f - Mathf.Pow(1f - t, 3);
            ghostHealthSlider.value = Mathf.Lerp(startFill, targetFill, ease);
            yield return null;
        }

        ghostHealthSlider.value = targetFill;
    }

    private IEnumerator NumberLerpRoutine(float targetHp)
    {
        float startHp = displayedHealthValue;
        float elapsed = 0f;

        while (elapsed < numberDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / numberDuration);
            displayedHealthValue = Mathf.Lerp(startHp, targetHp, t);
            UpdateHealthNumberText(displayedHealthValue, targetMaxHealth);
            yield return null;
        }

        displayedHealthValue = targetHp;
        UpdateHealthNumberText(displayedHealthValue, targetMaxHealth);
    }

    private void UpdateHealthNumberText(float current, float max)
    {
        if (healthNumbersText == null)
            return;

        int cur = Mathf.Max(0, Mathf.RoundToInt(current));
        int mx = Mathf.Max(1, Mathf.RoundToInt(max));
        float pct = Mathf.Clamp01((float)cur / mx) * 100f;

        healthNumbersText.text = $"{cur:N0} / {mx:N0}  <size=85%><color=#FFD54F>({pct:F0}%)</color></size>";
    }

    private void HandleDeath()
    {
        HideHealthBar();
    }

    /// <summary>
    /// Ẩn thanh máu khi Boss bị tiêu diệt
    /// </summary>
    public void HideHealthBar()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutAndDisableCo());
    }

    private IEnumerator FadeOutAndDisableCo()
    {
        if (mainHealthSlider != null) mainHealthSlider.value = 0f;
        if (ghostHealthSlider != null) ghostHealthSlider.value = 0f;
        UpdateHealthNumberText(0, targetMaxHealth);

        float elapsed = 0f;
        float duration = 0.8f;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

            yield return null;
        }

        gameObject.SetActive(false);
    }

    private void CleanupSubscriptions()
    {
        if (currentBossHealth != null)
        {
            currentBossHealth.OnHealthUpdate -= HandleHealthUpdate;
            currentBossHealth.OnDeath -= HandleDeath;
        }
    }

    private void OnDestroy()
    {
        CleanupSubscriptions();
    }

    // =========================================================================
    // FACTORY METHOD: Tạo tự động UI Top-Center Boss Bar (Chuẩn Souls-like / Metroidvania)
    // =========================================================================
    public static UI_BossHealthBar CreateTopCenterBar(Transform parentCanvas)
    {
        // 1. Root Container (Nằm ở Top Center, cách mép trên 32px)
        GameObject root = new GameObject("UI_BossHealthBar_TopCenter", typeof(RectTransform), typeof(CanvasGroup));
        root.transform.SetParent(parentCanvas, false);

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.sizeDelta = new Vector2(780f, 64f);
        rootRect.anchoredPosition = new Vector2(0f, -32f); // Cách đỉnh màn hình 32px

        CanvasGroup cg = root.GetComponent<CanvasGroup>();

        // 2. Background Panel Mờ (Dark Fantasy Vignette Panel)
        GameObject bgPanel = new GameObject("BackplatePanel", typeof(RectTransform), typeof(Image));
        bgPanel.transform.SetParent(root.transform, false);
        RectTransform bgRect = bgPanel.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = new Vector2(16f, 10f); // Rộng hơn một chút để ôm trọn
        bgRect.anchoredPosition = Vector2.zero;

        Image bgImg = bgPanel.GetComponent<Image>();
        bgImg.color = new Color(0.05f, 0.05f, 0.07f, 0.85f); // Đen mờ sang trọng

        // 3. Boss Name Text
        GameObject nameObj = new GameObject("BossNameText", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameObj.transform.SetParent(root.transform, false);

        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 1f);
        nameRect.anchorMax = new Vector2(1f, 1f);
        nameRect.pivot = new Vector2(0.5f, 1f);
        nameRect.sizeDelta = new Vector2(760f, 26f);
        nameRect.anchoredPosition = new Vector2(0f, -5f);

        TextMeshProUGUI nameTMP = nameObj.GetComponent<TextMeshProUGUI>();
        nameTMP.text = "BOSS NAME";
        nameTMP.fontSize = 18f;
        nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.alignment = TextAlignmentOptions.Center;
        nameTMP.color = new Color(1f, 0.84f, 0.25f); // Vàng kim #FFD54F

        // 4. Health Bar Outer Frame (Khung viền kim loại tối)
        GameObject barFrame = new GameObject("HealthBarFrame", typeof(RectTransform), typeof(Image));
        barFrame.transform.SetParent(root.transform, false);

        RectTransform frameRect = barFrame.GetComponent<RectTransform>();
        frameRect.anchorMin = new Vector2(0f, 0f);
        frameRect.anchorMax = new Vector2(1f, 0f);
        frameRect.pivot = new Vector2(0.5f, 0f);
        frameRect.sizeDelta = new Vector2(750f, 22f);
        frameRect.anchoredPosition = new Vector2(0f, 8f);

        Image frameImg = barFrame.GetComponent<Image>();
        frameImg.color = new Color(0.12f, 0.12f, 0.16f, 1f); // Khung viền xám đen

        // 5. Ghost Slider (Vàng hổ phách tụt trễ)
        GameObject ghostSliderObj = new GameObject("GhostSlider", typeof(RectTransform), typeof(Slider));
        ghostSliderObj.transform.SetParent(barFrame.transform, false);
        RectTransform ghostSliderRect = ghostSliderObj.GetComponent<RectTransform>();
        ghostSliderRect.anchorMin = Vector2.zero;
        ghostSliderRect.anchorMax = Vector2.one;
        ghostSliderRect.sizeDelta = new Vector2(-4f, -4f); // Viền 2px
        ghostSliderRect.anchoredPosition = Vector2.zero;

        Slider ghostSlider = ghostSliderObj.GetComponent<Slider>();
        ghostSlider.interactable = false;
        ghostSlider.transition = Selectable.Transition.None;
        ghostSlider.minValue = 0f;
        ghostSlider.maxValue = 1f;

        GameObject ghostFill = new GameObject("GhostFill", typeof(RectTransform), typeof(Image));
        ghostFill.transform.SetParent(ghostSliderObj.transform, false);
        RectTransform ghostFillRect = ghostFill.GetComponent<RectTransform>();
        ghostFillRect.anchorMin = Vector2.zero;
        ghostFillRect.anchorMax = Vector2.one;
        ghostFillRect.sizeDelta = Vector2.zero;

        Image ghostImg = ghostFill.GetComponent<Image>();
        ghostImg.color = new Color(1f, 0.65f, 0.15f, 0.95f); // Amber Gold
        ghostSlider.fillRect = ghostFillRect;

        // 6. Main Slider (Đỏ thẫm rực rỡ)
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
        RectTransform mainFillRect = mainFill.GetComponent<RectTransform>();
        mainFillRect.anchorMin = Vector2.zero;
        mainFillRect.anchorMax = Vector2.one;
        mainFillRect.sizeDelta = Vector2.zero;

        Image mainImg = mainFill.GetComponent<Image>();
        mainImg.color = new Color(0.85f, 0.16f, 0.16f, 1f); // Crimson Red
        mainSlider.fillRect = mainFillRect;

        // 7. Numbers Text (Nằm giữa thanh máu)
        GameObject numbersObj = new GameObject("HPNumbersText", typeof(RectTransform), typeof(TextMeshProUGUI));
        numbersObj.transform.SetParent(barFrame.transform, false);

        RectTransform numRect = numbersObj.GetComponent<RectTransform>();
        numRect.anchorMin = Vector2.zero;
        numRect.anchorMax = Vector2.one;
        numRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI numTMP = numbersObj.GetComponent<TextMeshProUGUI>();
        numTMP.text = "100 / 100 (100%)";
        numTMP.fontSize = 12.5f;
        numTMP.fontStyle = FontStyles.Bold;
        numTMP.alignment = TextAlignmentOptions.Center;
        numTMP.color = Color.white;

        // 8. Gán Controller Component
        UI_BossHealthBar controller = root.AddComponent<UI_BossHealthBar>();
        controller.mainHealthSlider = mainSlider;
        controller.ghostHealthSlider = ghostSlider;
        controller.bossNameText = nameTMP;
        controller.healthNumbersText = numTMP;
        controller.canvasGroup = cg;

        return controller;
    }

    public static UI_BossHealthBar CreateBottomCenterBar(Transform parentCanvas) => CreateTopCenterBar(parentCanvas);
}
