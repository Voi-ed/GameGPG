using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerHealthGlobe : MonoBehaviour
{
    [Header("Globe UI References")]
    [SerializeField] private Image mainLiquidImage;
    [SerializeField] private Image ghostLiquidImage;
    [SerializeField] private Image glassHighlightImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private RectTransform globeTransform;

    [Header("Liquid Colors")]
    [SerializeField] private Color fullHealthColor = new Color(0.9f, 0.12f, 0.12f, 1f);   // Đỏ ruby rực rỡ
    [SerializeField] private Color lowHealthColor = new Color(0.7f, 0.05f, 0.05f, 1f);    // Đỏ thẫm nguy cấp
    [SerializeField] private Color ghostLiquidColor = new Color(1f, 0.65f, 0.15f, 0.9f); // Vàng hổ phách
    [SerializeField] private Color healColor = new Color(0.2f, 0.95f, 0.45f, 1f);        // Xanh ngọc hồi máu

    [Header("Tween Animation Timings")]
    [SerializeField] private float mainFillDuration = 0.25f;
    [SerializeField] private float ghostDelay = 0.35f;
    [SerializeField] private float ghostDuration = 0.45f;
    [SerializeField] private float hitShakeStrength = 7f;
    [SerializeField] private float lowHpThreshold = 0.3f;

    private Player player;
    private Tween fillTween;
    private Tween ghostTween;
    private Tween numberTween;
    private Tween pulseTween;
    private Tween healFlashTween;

    private float previousHealth = -1f;
    private float displayedHealth = 0f;
    private float currentMaxHealth = 100f;
    private bool isPulsing = false;
    private Vector2 originalAnchoredPos = new Vector2(40f, 40f);

    private void Awake()
    {
        if (globeTransform == null)
            globeTransform = GetComponent<RectTransform>();

        if (globeTransform != null && globeTransform.anchoredPosition != Vector2.zero)
            originalAnchoredPos = globeTransform.anchoredPosition;
    }

    public void Setup(Player targetPlayer)
    {
        player = targetPlayer;
        if (player == null || player.health == null || player.stats == null)
            return;

        currentMaxHealth = player.stats.GetMaxHealth();
        float currentHp = player.health.GetCurrentHealth();
        displayedHealth = currentHp;
        previousHealth = currentHp;

        float targetPercent = currentMaxHealth > 0 ? Mathf.Clamp01(currentHp / currentMaxHealth) : 0f;

        if (mainLiquidImage != null) mainLiquidImage.fillAmount = targetPercent;
        if (ghostLiquidImage != null) ghostLiquidImage.fillAmount = targetPercent;

        UpdateHealthTextImmediate(currentHp, currentMaxHealth);
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        currentMaxHealth = maxHealth;
        float targetPercent = maxHealth > 0 ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

        bool isDamage = previousHealth >= 0 && currentHealth < previousHealth;
        bool isHealing = previousHealth >= 0 && currentHealth > previousHealth;
        previousHealth = currentHealth;

        // 1. Nhảy số máu mượt mà bằng Tween
        AnimateHealthNumber(currentHealth, maxHealth);

        // 2. Xử lý mực nước Vertical Fill
        if (isDamage)
        {
            // Nước đỏ tụt thẳng đứng nhanh
            fillTween?.Kill();
            if (mainLiquidImage != null)
            {
                fillTween = mainLiquidImage.DOFillAmount(targetPercent, mainFillDuration)
                    .SetEase(Ease.OutQuad);
            }

            // Nước vàng ghost tụt chậm thẳng đứng theo sau
            if (ghostLiquidImage != null)
            {
                ghostTween?.Kill();
                ghostTween = DOTween.Sequence()
                    .AppendInterval(ghostDelay)
                    .Append(ghostLiquidImage.DOFillAmount(targetPercent, ghostDuration).SetEase(Ease.OutCubic));
            }

            // Rung chấn quả cầu khi trúng đòn
            TriggerHitShake();
        }
        else if (isHealing)
        {
            // Khi hồi máu: Nước ghost bằng target ngay, nước đỏ dâng cuộn lên
            if (ghostLiquidImage != null)
            {
                ghostTween?.Kill();
                ghostLiquidImage.fillAmount = targetPercent;
            }

            fillTween?.Kill();
            if (mainLiquidImage != null)
            {
                fillTween = mainLiquidImage.DOFillAmount(targetPercent, mainFillDuration * 1.5f)
                    .SetEase(Ease.OutQuad);
            }

            TriggerHealFlash();
        }
        else
        {
            // Khởi tạo hoặc không đổi
            if (mainLiquidImage != null) mainLiquidImage.fillAmount = targetPercent;
            if (ghostLiquidImage != null) ghostLiquidImage.fillAmount = targetPercent;
        }

        // 3. Đổi màu máu khi thấp & Kiểm tra nhịp tim đập thình thịch
        UpdateHeartbeatPulse(targetPercent);
    }

    private void AnimateHealthNumber(float targetHealth, float maxHealth)
    {
        if (healthText == null)
            return;

        numberTween?.Kill();
        float startVal = displayedHealth;

        numberTween = DOVirtual.Float(startVal, targetHealth, 0.25f, val =>
        {
            displayedHealth = val;
            UpdateHealthTextImmediate(val, maxHealth);
        }).SetEase(Ease.OutQuad);
    }

    private void UpdateHealthTextImmediate(float current, float max)
    {
        if (healthText == null)
            return;

        int cur = Mathf.Max(0, Mathf.RoundToInt(current));
        int mx = Mathf.Max(1, Mathf.RoundToInt(max));

        healthText.text = $"<size=120%><b>{cur}</b></size>\n<size=65%><color=#CFD8DC>/ {mx} HP</color></size>";
    }

    private void TriggerHitShake()
    {
        if (globeTransform == null)
            return;

        globeTransform.DOKill(true);
        globeTransform.anchoredPosition = originalAnchoredPos;
        globeTransform.DOShakeAnchorPos(0.2f, hitShakeStrength, 25, 90f, false, true)
            .OnComplete(() =>
            {
                if (globeTransform != null)
                    globeTransform.anchoredPosition = originalAnchoredPos;
            });
    }

    private void TriggerHealFlash()
    {
        if (mainLiquidImage == null)
            return;

        healFlashTween?.Kill();
        mainLiquidImage.color = healColor;
        healFlashTween = mainLiquidImage.DOColor(fullHealthColor, 0.45f).SetEase(Ease.OutQuad);
    }

    private void UpdateHeartbeatPulse(float healthPercent)
    {
        if (healthPercent <= lowHpThreshold && healthPercent > 0f)
        {
            if (!isPulsing && globeTransform != null)
            {
                isPulsing = true;
                pulseTween?.Kill();
                pulseTween = globeTransform.DOScale(1.09f, 0.38f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);

                if (mainLiquidImage != null)
                    mainLiquidImage.color = lowHealthColor;
            }
        }
        else
        {
            if (isPulsing && globeTransform != null)
            {
                isPulsing = false;
                pulseTween?.Kill();
                globeTransform.DOScale(1f, 0.2f);

                if (mainLiquidImage != null)
                    mainLiquidImage.color = fullHealthColor;
            }
        }
    }

    private void OnDisable()
    {
        KillAllTweens();
    }

    private void OnDestroy()
    {
        KillAllTweens();
    }

    private void KillAllTweens()
    {
        fillTween?.Kill();
        ghostTween?.Kill();
        numberTween?.Kill();
        pulseTween?.Kill();
        healFlashTween?.Kill();

        if (globeTransform != null)
            globeTransform.DOKill();
    }

    // =========================================================================
    // FACTORY METHOD: Tạo tự động Quả Cầu Máu Diablo Vertical Fill ở Bottom-Left
    // =========================================================================
    public static UI_PlayerHealthGlobe CreateHealthGlobe(Transform parentCanvas)
    {
        // 1. Sinh các Sprite tròn chống răng cưa (Procedural Anti-aliased Sprites)
        Sprite solidCircle = CreateCircleSprite(256, Color.white, 2.5f);
        Sprite outerRing = CreateRingSprite(256, 14f, new Color(0.25f, 0.22f, 0.2f, 1f), 2.5f);
        Sprite glassSpecular = CreateGlassHighlightSprite(256);

        // 2. Root Container (Nằm ở Bottom-Left: x=40, y=40, size=132x132)
        GameObject root = new GameObject("UI_PlayerHealthGlobe_BottomLeft", typeof(RectTransform));
        root.transform.SetParent(parentCanvas, false);

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero; // Bottom-Left
        rootRect.anchorMax = Vector2.zero;
        rootRect.pivot = Vector2.zero;
        rootRect.sizeDelta = new Vector2(132f, 132f);
        rootRect.anchoredPosition = new Vector2(40f, 40f);

        // 3. Dark Globe Interior (Lòng cầu tối khi cạn máu)
        GameObject darkBgObj = new GameObject("Globe_DarkInterior", typeof(RectTransform), typeof(Image));
        darkBgObj.transform.SetParent(root.transform, false);
        RectTransform darkBgRect = darkBgObj.GetComponent<RectTransform>();
        darkBgRect.anchorMin = Vector2.zero;
        darkBgRect.anchorMax = Vector2.one;
        darkBgRect.sizeDelta = Vector2.zero;

        Image darkBgImg = darkBgObj.GetComponent<Image>();
        darkBgImg.sprite = solidCircle;
        darkBgImg.color = new Color(0.06f, 0.06f, 0.08f, 0.95f);

        // 4. Ghost Liquid (Mực nước vàng hổ phách tụt chậm theo phương thẳng đứng)
        GameObject ghostObj = new GameObject("Globe_GhostLiquid", typeof(RectTransform), typeof(Image));
        ghostObj.transform.SetParent(root.transform, false);
        RectTransform ghostRect = ghostObj.GetComponent<RectTransform>();
        ghostRect.anchorMin = Vector2.zero;
        ghostRect.anchorMax = Vector2.one;
        ghostRect.sizeDelta = Vector2.zero;

        Image ghostImg = ghostObj.GetComponent<Image>();
        ghostImg.sprite = solidCircle;
        ghostImg.color = new Color(1f, 0.65f, 0.15f, 0.92f); // Amber Gold
        ghostImg.type = Image.Type.Filled;
        ghostImg.fillMethod = Image.FillMethod.Vertical; // THEO CHIỀU DỌC!
        ghostImg.fillOrigin = (int)Image.OriginVertical.Bottom;
        ghostImg.fillAmount = 1f;

        // 5. Main Blood Liquid (Mực nước máu đỏ tươi dâng/hạ theo phương thẳng đứng)
        GameObject bloodObj = new GameObject("Globe_MainBloodLiquid", typeof(RectTransform), typeof(Image));
        bloodObj.transform.SetParent(root.transform, false);
        RectTransform bloodRect = bloodObj.GetComponent<RectTransform>();
        bloodRect.anchorMin = Vector2.zero;
        bloodRect.anchorMax = Vector2.one;
        bloodRect.sizeDelta = Vector2.zero;

        Image bloodImg = bloodObj.GetComponent<Image>();
        bloodImg.sprite = solidCircle;
        bloodImg.color = new Color(0.9f, 0.12f, 0.12f, 1f); // Crimson Red
        bloodImg.type = Image.Type.Filled;
        bloodImg.fillMethod = Image.FillMethod.Vertical; // THEO CHIỀU DỌC!
        bloodImg.fillOrigin = (int)Image.OriginVertical.Bottom;
        bloodImg.fillAmount = 1f;

        // 6. Glass Specular Highlight (Bóng kính phản quang 3D)
        GameObject glassObj = new GameObject("Globe_GlassHighlight", typeof(RectTransform), typeof(Image));
        glassObj.transform.SetParent(root.transform, false);
        RectTransform glassRect = glassObj.GetComponent<RectTransform>();
        glassRect.anchorMin = Vector2.zero;
        glassRect.anchorMax = Vector2.one;
        glassRect.sizeDelta = Vector2.zero;

        Image glassImg = glassObj.GetComponent<Image>();
        glassImg.sprite = glassSpecular;
        glassImg.raycastTarget = false;

        // 7. Outer Ornate Ring Frame (Viền kim loại bao quanh)
        GameObject frameObj = new GameObject("Globe_OuterFrame", typeof(RectTransform), typeof(Image));
        frameObj.transform.SetParent(root.transform, false);
        RectTransform frameRect = frameObj.GetComponent<RectTransform>();
        frameRect.anchorMin = Vector2.zero;
        frameRect.anchorMax = Vector2.one;
        frameRect.sizeDelta = new Vector2(4f, 4f);
        frameRect.anchoredPosition = Vector2.zero;

        Image frameImg = frameObj.GetComponent<Image>();
        frameImg.sprite = outerRing;
        frameImg.raycastTarget = false;

        // 8. Health Numbers Text (Tâm cầu)
        GameObject textObj = new GameObject("Globe_HealthText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(root.transform, false);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = "<size=120%><b>100</b></size>\n<size=65%><color=#CFD8DC>/ 100 HP</color></size>";
        tmp.fontSize = 18f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        // 9. Attach Component & Connect References
        UI_PlayerHealthGlobe globe = root.AddComponent<UI_PlayerHealthGlobe>();
        globe.globeTransform = rootRect;
        globe.originalAnchoredPos = new Vector2(40f, 40f);
        globe.mainLiquidImage = bloodImg;
        globe.ghostLiquidImage = ghostImg;
        globe.glassHighlightImage = glassImg;
        globe.frameImage = frameImg;
        globe.healthText = tmp;

        return globe;
    }

    // =========================================================================
    // PROCEDURAL SPRITE GENERATION (Khởi tạo hình ảnh vòng tròn mượt mà chống răng cưa)
    // =========================================================================
    private static Sprite CreateCircleSprite(int diameter, Color color, float borderSmoothness = 2.5f)
    {
        Texture2D texture = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        float radius = diameter * 0.5f;
        Vector2 center = new Vector2(radius, radius);
        Color[] pixels = new Color[diameter * diameter];
        int index = 0;

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float alpha = Mathf.Clamp01((radius - dist) / borderSmoothness);
                pixels[index++] = new Color(color.r, color.g, color.b, color.a * alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, diameter, diameter), new Vector2(0.5f, 0.5f), 100f);
    }

    private static Sprite CreateRingSprite(int diameter, float ringThickness, Color ringColor, float borderSmoothness = 2.5f)
    {
        Texture2D texture = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        float outerRadius = diameter * 0.5f;
        float innerRadius = outerRadius - ringThickness;
        Vector2 center = new Vector2(outerRadius, outerRadius);
        Color[] pixels = new Color[diameter * diameter];
        int index = 0;

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float outerAlpha = Mathf.Clamp01((outerRadius - dist) / borderSmoothness);
                float innerAlpha = Mathf.Clamp01((dist - innerRadius) / borderSmoothness);
                float finalAlpha = Mathf.Min(outerAlpha, innerAlpha);
                pixels[index++] = new Color(ringColor.r, ringColor.g, ringColor.b, ringColor.a * finalAlpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, diameter, diameter), new Vector2(0.5f, 0.5f), 100f);
    }

    private static Sprite CreateGlassHighlightSprite(int diameter)
    {
        Texture2D texture = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        float radius = diameter * 0.5f;
        Vector2 center = new Vector2(radius, radius);
        Vector2 highlightCenter = new Vector2(radius * 0.65f, radius * 1.35f);
        Color[] pixels = new Color[diameter * diameter];
        int index = 0;

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

                pixels[index++] = new Color(1f, 1f, 1f, outerAlpha * highlight * 0.45f);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, diameter, diameter), new Vector2(0.5f, 0.5f), 100f);
    }
}
