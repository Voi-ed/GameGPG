using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_BossVictoryBanner : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private Image panelBackground;
    [SerializeField] private Image outerBorder;
    [SerializeField] private Image headerDivider;
    [SerializeField] private Image footerDivider;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private Button returnButton;
    [SerializeField] private Image returnButtonBg;
    [SerializeField] private TextMeshProUGUI returnButtonText;

    [Header("Theme Colors")]
    [SerializeField] private Color standardTitleColor = new Color(1f, 0.85f, 0.25f, 1f);      // Vàng kim hoàng gia #FFD54F
    [SerializeField] private Color standardButtonColor = new Color(0.18f, 0.65f, 0.35f, 1f);   // Xanh ngọc lục bảo #2E7D32
    [SerializeField] private Color winGameTitleColor = new Color(1f, 0.45f, 0.15f, 1f);       // Cam lửa tối thượng
    [SerializeField] private Color winGameButtonColor = new Color(0.85f, 0.65f, 0.15f, 1f);    // Vàng kim rực rỡ

    private UnityAction currentReturnAction;
    private Tween punchTween;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (returnButton != null)
        {
            returnButton.onClick.AddListener(OnButtonClicked);
        }
    }

    /// <summary>
    /// Hiển thị bảng chiến thắng Boss với hoạt ảnh hoành tráng
    /// </summary>
    public void ShowVictory(string title, string subtitle, bool isWinGame, UnityAction onReturn)
    {
        currentReturnAction = onReturn;

        if (titleText != null)
        {
            titleText.text = string.IsNullOrEmpty(title) ? "⚔ CHIẾN THẮNG BOSS! ⚔" : title;
            titleText.color = isWinGame ? winGameTitleColor : standardTitleColor;
        }

        if (subtitleText != null)
        {
            subtitleText.text = string.IsNullOrEmpty(subtitle) ? "Cổng dịch chuyển trở về đã được kích hoạt." : subtitle;
        }

        if (returnButtonText != null)
        {
            returnButtonText.text = isWinGame ? "★ HOÀN THÀNH GAME ★" : "QUAY VỀ LÀNG";
        }

        if (returnButtonBg != null)
        {
            returnButtonBg.color = isWinGame ? winGameButtonColor : standardButtonColor;
        }

        gameObject.SetActive(true);

        // Khởi động hoạt ảnh xuất hiện mượt mà
        PlayEntranceAnimation();
    }

    private void PlayEntranceAnimation()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.35f).SetUpdate(true);
        }

        if (panelRect != null)
        {
            panelRect.localScale = new Vector3(0.75f, 0.75f, 1f);
            panelRect.DOScale(1f, 0.45f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        // Nhịp thở nhẹ cho nút quay về
        if (returnButton != null)
        {
            punchTween?.Kill();
            returnButton.transform.localScale = Vector3.one;
            punchTween = returnButton.transform.DOScale(1.04f, 0.8f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }
    }

    private void OnButtonClicked()
    {
        punchTween?.Kill();

        if (returnButton != null)
        {
            returnButton.transform.DOPunchScale(new Vector3(-0.1f, -0.1f, 0), 0.15f)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    currentReturnAction?.Invoke();
                });
        }
        else
        {
            currentReturnAction?.Invoke();
        }
    }

    private void OnDestroy()
    {
        punchTween?.Kill();
        if (panelRect != null)
            panelRect.DOKill();
        if (canvasGroup != null)
            canvasGroup.DOKill();
    }
}
