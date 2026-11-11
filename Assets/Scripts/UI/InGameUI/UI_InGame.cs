using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour
{
    private Player player;
    private Inventory_Player inventory;
    private UI_SkillSlot[] skillSlots;

    [Header("Circular Health Globe (Diablo Style - Bottom Left)")]
    [Tooltip("Sử dụng Quả Cầu Máu Diablo ở góc dưới bên trái thay cho thanh máu ngang cũ")]
    [SerializeField] private bool useHealthGlobe = true;
    [SerializeField] private UI_PlayerHealthGlobe healthGlobe;

    [Header("Active Potion Buff Tray (Bottom Left)")]
    [SerializeField] private UI_BuffBar buffBar;

    [Header("Health Bar References (Legacy)")]
    [SerializeField] private RectTransform healthRect;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider ghostHealthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Health Bar Visual & Animation")]
    [SerializeField] private float baseBarWidth = 220f;
    [SerializeField] private float minBarWidth = 180f;
    [SerializeField] private float maxBarWidth = 360f;
    [SerializeField] private float widthPerHealthPoint = 0.5f;
    [SerializeField] private float mainFillDuration = 0.2f;
    [SerializeField] private float ghostDelay = 0.35f;
    [SerializeField] private float ghostDuration = 0.45f;
    [SerializeField] private bool enableHitShake = true;
    [SerializeField] private bool enableLowHpPulse = true;
    [SerializeField] private float lowHpThreshold = 0.3f;

    private float displayedHealth;
    private float previousHealth = -1f;
    private Tween healthFillTween;
    private Tween ghostFillTween;
    private Tween healthNumberTween;
    private Tween pulseTween;
    private Color originalFillColor = Color.red;

    [Header("Quick Item Slots")]
    [SerializeField] private float yOffsetQuickItemParent = 150;
    [SerializeField] private Transform quickItemOptionsParent;
    private UI_QuickItemSlotOption[] quickItemOptions;
    private UI_QuickItemSlot[] quickItemSlots;

    [Header("Top Right Menu Bar")]
    [SerializeField] private UI_TopRightMenu topRightMenu;

    private void Start()
    {
        quickItemSlots = GetComponentsInChildren<UI_QuickItemSlot>();

        player = FindFirstObjectByType<Player>();

        if (useHealthGlobe)
        {
            SetupHealthGlobe();
        }
        else
        {
            SetupHealthBarVisuals();
        }

        SetupBuffBar();

        if (player != null)
        {
            if (player.health != null)
            {
                player.health.OnHealthUpdate += UpdateHealthBar;
                UpdateHealthBar();
            }

            inventory = player.inventory;
            if (inventory != null)
            {
                inventory.OnInventoryChange += UpdateQuickSlotsUI;
                inventory.OnQuickSlotUsed += PlayQuickSlotFeedback;
                UpdateQuickSlotsUI();
            }
        }

        SetupTopRightMenu();
    }

    private void SetupTopRightMenu()
    {
        if (topRightMenu == null)
            topRightMenu = GetComponentInChildren<UI_TopRightMenu>(true);

        if (topRightMenu == null)
            topRightMenu = gameObject.AddComponent<UI_TopRightMenu>();

        topRightMenu.Initialize(this);
    }

    public void OnFastTravelClicked()
    {
        if (UI.instance != null)
        {
            bool isOpen = UI.instance.teleportUI != null && UI.instance.teleportUI.gameObject.activeSelf;
            UI.instance.OpenTeleportUI(!isOpen);
        }
    }

    private void OnDisable()
    {
        KillAllTweens();
    }

    private void OnDestroy()
    {
        if (player != null && player.health != null)
        {
            player.health.OnHealthUpdate -= UpdateHealthBar;
        }

        if (inventory != null)
        {
            inventory.OnInventoryChange -= UpdateQuickSlotsUI;
            inventory.OnQuickSlotUsed -= PlayQuickSlotFeedback;
        }

        KillAllTweens();
    }

    private void SetupHealthGlobe()
    {
        // Ẩn thanh máu ngang cũ ở Top-Left
        if (healthRect != null)
            healthRect.gameObject.SetActive(false);
        else if (healthSlider != null)
            healthSlider.gameObject.SetActive(false);

        if (healthGlobe == null)
        {
            healthGlobe = FindFirstObjectByType<UI_PlayerHealthGlobe>();
            if (healthGlobe == null)
            {
                Transform parent = transform.parent != null ? transform.parent : transform;
                healthGlobe = UI_PlayerHealthGlobe.CreateHealthGlobe(parent);
            }
        }

        if (healthGlobe != null && player != null)
        {
            healthGlobe.Setup(player);
        }
    }

    private void SetupBuffBar()
    {
        if (buffBar == null)
        {
            buffBar = FindFirstObjectByType<UI_BuffBar>();
            if (buffBar == null)
            {
                Transform parent = transform.parent != null ? transform.parent : transform;
                buffBar = UI_BuffBar.CreateBuffBar(parent);
            }
        }

        if (buffBar != null && player != null)
        {
            buffBar.Setup(player);
        }
    }

    private void SetupHealthBarVisuals()
    {
        if (healthSlider != null && healthSlider.fillRect != null)
        {
            if (fillImage == null)
                fillImage = healthSlider.fillRect.GetComponent<Image>();

            if (fillImage != null)
                originalFillColor = fillImage.color;

            SetupGhostBar();
        }
    }

    private void SetupGhostBar()
    {
        if (ghostHealthSlider != null)
            return;

        if (healthSlider == null || healthSlider.fillRect == null)
            return;

        Transform fillArea = healthSlider.fillRect.parent;
        if (fillArea == null)
            return;

        // Auto-create a Ghost Bar right behind the main Fill Area
        GameObject ghostAreaObj = Instantiate(fillArea.gameObject, fillArea.parent);
        ghostAreaObj.name = "Ghost Fill Area";
        int fillIndex = fillArea.GetSiblingIndex();
        ghostAreaObj.transform.SetSiblingIndex(fillIndex);

        ghostHealthSlider = ghostAreaObj.AddComponent<Slider>();
        ghostHealthSlider.interactable = false;
        ghostHealthSlider.transition = Selectable.Transition.None;
        ghostHealthSlider.direction = healthSlider.direction;
        ghostHealthSlider.minValue = 0f;
        ghostHealthSlider.maxValue = 1f;

        Image clonedFillImg = ghostAreaObj.GetComponentInChildren<Image>();
        if (clonedFillImg != null)
        {
            clonedFillImg.color = new Color(1f, 0.76f, 0.25f, 0.85f); // Amber / Gold Ghost Trail
            ghostHealthSlider.fillRect = clonedFillImg.rectTransform;
        }

        ghostHealthSlider.value = healthSlider.value;
    }

    private void KillAllTweens()
    {
        healthFillTween?.Kill();
        ghostFillTween?.Kill();
        healthNumberTween?.Kill();
        pulseTween?.Kill();

        if (healthRect != null)
            healthRect.DOKill();

        if (fillImage != null)
            fillImage.DOKill();
    }

    public void PlayQuickSlotFeedback(int slotNumber) => quickItemSlots[slotNumber].SimulateButtonFeedback();

    public void UpdateQuickSlotsUI()
    {
        if (inventory == null || inventory.quickItems == null)
            return;

        if (quickItemSlots == null || quickItemSlots.Length == 0)
            quickItemSlots = GetComponentsInChildren<UI_QuickItemSlot>();

        if (quickItemSlots == null)
            return;

        Inventory_Item[] quickItems = inventory.quickItems;

        for (int i = 0; i < quickItems.Length; i++)
        {
            if (i < quickItemSlots.Length && quickItemSlots[i] != null)
                quickItemSlots[i].UpdateQuickSlotUI(quickItems[i]);
        }
    }

    public void OpenQuickItemOptions(UI_QuickItemSlot quickItemSlot, RectTransform targetRect)
    {
        if(quickItemOptions == null)
            quickItemOptions = quickItemOptionsParent.GetComponentsInChildren<UI_QuickItemSlotOption>(true);

        List<Inventory_Item> consumables = inventory.itemList.FindAll(item => item.itemData.itemType == ItemType.Consumable);

        for (int i = 0; i < quickItemOptions.Length; i++)
        {
            if (i < consumables.Count)
            {
                quickItemOptions[i].gameObject.SetActive(true);
                quickItemOptions[i].SetupOption(quickItemSlot, consumables[i]);
            }
            else
                quickItemOptions[i].gameObject.SetActive(false);
        }

        quickItemOptionsParent.position = targetRect.position + Vector3.up * yOffsetQuickItemParent;
    }

    public void HideQuickItemOptions() => quickItemOptionsParent.position = new Vector3(0, 9999);

    public UI_SkillSlot GetSkillSlot(SkillType skillType)
    {
        if(skillSlots == null)
            skillSlots = GetComponentsInChildren<UI_SkillSlot>(true);

        foreach (var slot in skillSlots)
        {
            if (slot.skillType == skillType)
            {
                slot.gameObject.SetActive(true);
                return slot;
            }
        }

        return null;
    }

    private void UpdateHealthBar()
    {
        if (player == null || player.health == null || player.stats == null)
            return;

        float currentHealth = Mathf.Max(0, Mathf.RoundToInt(player.health.GetCurrentHealth()));
        float maxHealth = player.stats.GetMaxHealth();
        float targetPercent = maxHealth > 0 ? currentHealth / maxHealth : 0f;

        // Nếu bật chế độ Bình Cầu Máu Diablo ở Bottom-Left
        if (useHealthGlobe)
        {
            if (healthGlobe == null)
                SetupHealthGlobe();

            if (healthGlobe != null)
            {
                healthGlobe.UpdateHealth(currentHealth, maxHealth);
                return;
            }
        }

        // 1. Dynamic Bar Width (Clamped & Proportionate)
        UpdateBarWidth(maxHealth);

        // 2. Animate Health Value Text
        AnimateHealthText(currentHealth, maxHealth);

        // 3. Check damage vs healing
        bool isDamage = previousHealth >= 0 && currentHealth < previousHealth;
        bool isHealing = previousHealth >= 0 && currentHealth > previousHealth;
        previousHealth = currentHealth;

        // 4. Slider animation & Ghost bar logic
        if (isDamage)
        {
            // Main bar drops quickly
            healthFillTween?.Kill();
            healthFillTween = healthSlider.DOValue(targetPercent, mainFillDuration)
                .SetEase(Ease.OutQuad);

            // Ghost bar waits briefly, then smoothly eases down
            if (ghostHealthSlider != null)
            {
                ghostFillTween?.Kill();
                ghostFillTween = DOTween.Sequence()
                    .AppendInterval(ghostDelay)
                    .Append(ghostHealthSlider.DOValue(targetPercent, ghostDuration).SetEase(Ease.OutCubic));
            }

            // Hit Feedback: Shake & Flash
            TriggerHitFeedback();
        }
        else if (isHealing)
        {
            // Ghost bar catches up immediately to target, main bar rises smoothly
            if (ghostHealthSlider != null)
            {
                ghostFillTween?.Kill();
                ghostHealthSlider.value = targetPercent;
            }

            healthFillTween?.Kill();
            healthFillTween = healthSlider.DOValue(targetPercent, mainFillDuration * 1.5f)
                .SetEase(Ease.OutQuad);
        }
        else
        {
            // Initial state / unchanged
            healthSlider.value = targetPercent;
            if (ghostHealthSlider != null)
                ghostHealthSlider.value = targetPercent;
        }

        // 5. Low Health Pulse check
        UpdateLowHealthState(targetPercent);
    }

    private void UpdateBarWidth(float maxHealth)
    {
        if (healthRect == null)
            return;

        float targetWidth = Mathf.Clamp(baseBarWidth + (maxHealth - 100f) * widthPerHealthPoint, minBarWidth, maxBarWidth);

        if (Mathf.Abs(targetWidth - healthRect.sizeDelta.x) > 0.5f)
        {
            healthRect.DOKill();
            healthRect.DOSizeDelta(new Vector2(targetWidth, healthRect.sizeDelta.y), 0.3f)
                .SetEase(Ease.OutQuad);
        }
    }

    private void AnimateHealthText(float targetHealth, float maxHealth)
    {
        if (healthText == null)
            return;

        healthNumberTween?.Kill();

        float startHealth = displayedHealth;
        healthNumberTween = DOVirtual.Float(startHealth, targetHealth, 0.25f, val =>
        {
            displayedHealth = val;
            healthText.text = $"{Mathf.RoundToInt(val)} / {Mathf.RoundToInt(maxHealth)}";
        }).SetEase(Ease.OutQuad);
    }

    private void TriggerHitFeedback()
    {
        if (enableHitShake && healthRect != null)
        {
            healthRect.DOKill(complete: true);
            healthRect.DOPunchScale(new Vector3(0.06f, 0.06f, 0f), 0.18f, 10, 1)
                .OnComplete(() => healthRect.localScale = Vector3.one);
        }

        if (fillImage != null)
        {
            fillImage.DOKill();
            fillImage.color = Color.white;
            fillImage.DOColor(originalFillColor, 0.15f);
        }
    }

    private void UpdateLowHealthState(float percent)
    {
        if (!enableLowHpPulse || fillImage == null)
            return;

        bool isLowHp = percent > 0 && percent <= lowHpThreshold;

        if (isLowHp)
        {
            if (pulseTween == null || !pulseTween.IsActive())
            {
                Color darkRed = new Color(0.55f, 0.08f, 0.08f, 1f);
                pulseTween = fillImage.DOColor(darkRed, 0.45f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }

            if (healthText != null)
                healthText.color = new Color(1f, 0.35f, 0.35f, 1f);
        }
        else
        {
            if (pulseTween != null && pulseTween.IsActive())
            {
                pulseTween.Kill();
                pulseTween = null;
                fillImage.DOColor(originalFillColor, 0.2f);
            }

            if (healthText != null)
                healthText.color = Color.white;
        }
    }
}
