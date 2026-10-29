using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuffSlot : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image radialCooldownOverlay;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image warningBorder;
    [SerializeField] private TextMeshProUGUI timerText;

    private ActiveBuffInfo currentBuff;
    private Tween pulseTween;
    private bool isExpiringPulseActive = false;

    public string CurrentSourceId => currentBuff?.sourceId;

    public void Setup(ActiveBuffInfo buffInfo)
    {
        currentBuff = buffInfo;

        if (iconImage != null)
        {
            if (buffInfo.icon != null)
            {
                iconImage.sprite = buffInfo.icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        isExpiringPulseActive = false;
        if (warningBorder != null)
        {
            warningBorder.gameObject.SetActive(false);
        }

        UpdateVisuals();
    }

    public void UpdateSlot(ActiveBuffInfo buffInfo)
    {
        currentBuff = buffInfo;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (currentBuff == null)
            return;

        // 1. Radial Dark Cooldown Overlay (Quét kim đồng hồ ngược: 1 -> 0 khi sắp hết)
        if (radialCooldownOverlay != null)
        {
            // Progress = remainingTime / duration. 
            // Khi vừa uống: progress = 1 -> overlay = 0 (sáng tỏ).
            // Càng gần hết giờ -> overlay phủ lên dần, hoặc ngược lại overlay rút đi.
            // Kiểu chuẩn RPG: Overlay màu xám phủ tỉ lệ (1 - progress) biểu thị phần thời gian đã trôi qua.
            radialCooldownOverlay.fillAmount = 1f - currentBuff.Progress;
        }

        // 2. Text hiển thị thời gian còn lại
        if (timerText != null)
        {
            float rem = currentBuff.remainingTime;
            if (rem >= 60f)
            {
                int mins = Mathf.CeilToInt(rem / 60f);
                timerText.text = $"{mins}m";
                timerText.color = Color.white;
            }
            else if (rem >= 10f)
            {
                int secs = Mathf.CeilToInt(rem);
                timerText.text = $"{secs}s";
                timerText.color = Color.white;
            }
            else if (rem > 0f)
            {
                timerText.text = $"{rem:F1}s";
                timerText.color = currentBuff.IsExpiringSoon ? new Color(1f, 0.35f, 0.35f, 1f) : Color.white;
            }
            else
            {
                timerText.text = "0s";
            }
        }

        // 3. Hiệu ứng nhấp nháy cảnh báo khi còn dưới 3 giây
        if (currentBuff.IsExpiringSoon && !isExpiringPulseActive)
        {
            StartExpiringPulse();
        }
    }

    private void StartExpiringPulse()
    {
        isExpiringPulseActive = true;

        if (warningBorder != null)
        {
            warningBorder.gameObject.SetActive(true);
            warningBorder.color = new Color(1f, 0.25f, 0.25f, 0.85f);
        }

        pulseTween?.Kill();
        pulseTween = transform.DOScale(1.15f, 0.25f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnDisable()
    {
        KillTweens();
    }

    private void OnDestroy()
    {
        KillTweens();
    }

    private void KillTweens()
    {
        if (pulseTween != null && pulseTween.IsActive())
        {
            pulseTween.Kill();
            pulseTween = null;
        }
        transform.localScale = Vector3.one;
    }

    // =========================================================================
    // FACTORY METHOD: Tự động khởi tạo cấu trúc UI sắc nét chống răng cưa
    // =========================================================================
    public static UI_BuffSlot CreateSlot(Transform parent, Sprite defaultSquare, Sprite defaultRing)
    {
        GameObject slotObj = new GameObject("UI_BuffSlot", typeof(RectTransform));
        slotObj.transform.SetParent(parent, false);

        RectTransform slotRect = slotObj.GetComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(40f, 40f);

        // 1. Dark Background
        GameObject bgObj = new GameObject("Slot_BG", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(slotObj.transform, false);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.sprite = defaultSquare;
        bgImg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        bgImg.raycastTarget = false;

        // 2. Buff Icon
        GameObject iconObj = new GameObject("Slot_Icon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(slotObj.transform, false);
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = new Vector2(-6f, -6f); // Padding 3px
        Image iconImg = iconObj.GetComponent<Image>();
        iconImg.preserveAspect = true;
        iconImg.raycastTarget = false;

        // 3. Radial Dark Cooldown Overlay
        GameObject overlayObj = new GameObject("Slot_RadialOverlay", typeof(RectTransform), typeof(Image));
        overlayObj.transform.SetParent(slotObj.transform, false);
        RectTransform overlayRect = overlayObj.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        Image overlayImg = overlayObj.GetComponent<Image>();
        overlayImg.sprite = defaultSquare;
        overlayImg.color = new Color(0f, 0f, 0f, 0.65f); // Đen mờ 65%
        overlayImg.type = Image.Type.Filled;
        overlayImg.fillMethod = Image.FillMethod.Radial360;
        overlayImg.fillOrigin = (int)Image.Origin360.Top;
        overlayImg.fillClockwise = false;
        overlayImg.fillAmount = 0f;
        overlayImg.raycastTarget = false;

        // 4. Outer Metallic Frame
        GameObject frameObj = new GameObject("Slot_Frame", typeof(RectTransform), typeof(Image));
        frameObj.transform.SetParent(slotObj.transform, false);
        RectTransform frameRect = frameObj.GetComponent<RectTransform>();
        frameRect.anchorMin = Vector2.zero;
        frameRect.anchorMax = Vector2.one;
        frameRect.sizeDelta = new Vector2(4f, 4f);
        Image frameImg = frameObj.GetComponent<Image>();
        frameImg.sprite = defaultRing;
        frameImg.color = new Color(0.7f, 0.65f, 0.55f, 0.9f); // Gold/Bronze
        frameImg.raycastTarget = false;

        // 5. Warning Glow Border (Khi sắp hết)
        GameObject warningObj = new GameObject("Slot_WarningBorder", typeof(RectTransform), typeof(Image));
        warningObj.transform.SetParent(slotObj.transform, false);
        RectTransform warnRect = warningObj.GetComponent<RectTransform>();
        warnRect.anchorMin = Vector2.zero;
        warnRect.anchorMax = Vector2.one;
        warnRect.sizeDelta = new Vector2(6f, 6f);
        Image warnImg = warningObj.GetComponent<Image>();
        warnImg.sprite = defaultRing;
        warnImg.color = new Color(1f, 0.2f, 0.2f, 0.85f);
        warnImg.raycastTarget = false;
        warningObj.SetActive(false);

        // 6. Timer Countdown Text
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

        // 7. Component attachment
        UI_BuffSlot slot = slotObj.AddComponent<UI_BuffSlot>();
        slot.iconImage = iconImg;
        slot.radialCooldownOverlay = overlayImg;
        slot.frameImage = frameImg;
        slot.warningBorder = warnImg;
        slot.timerText = tmp;

        return slot;
    }
}
