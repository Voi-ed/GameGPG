using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SkillSlot : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler
{
    private UI ui;
    private Image skillIcon;
    private RectTransform rect;
    private Button button;

    private SkillDataSO skillData;

    public SkillType skillType;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private string inputKeyName;
    [SerializeField] private TextMeshProUGUI inputKeyText;
    [SerializeField] private TextMeshProUGUI chargesText;
    [SerializeField] private GameObject conflictSlot;

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        button = GetComponent<Button>();
        skillIcon = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
    }

    private void OnValidate()
    {
        gameObject.name = "UI_SkillSlot - " + skillType.ToString();
    }

    public void SetupSkillSlot(SkillDataSO selectedSkill)
    {
        this.skillData = selectedSkill;

        Color color = Color.black; color.a = .6f;
        cooldownImage.color = color;

        inputKeyText.text = inputKeyName;
        skillIcon.sprite = selectedSkill.icon;

        if(conflictSlot != null)
            conflictSlot.SetActive(false);

        if (selectedSkill != null && selectedSkill.upgradeData != null && selectedSkill.upgradeData.upgradeType == SkillUpgradeType.Shard_Multicast)
        {
            EnsureChargesText();
        }
        else
        {
            ClearCharges();
        }
    }

    public void StartCooldown(float cooldown)
    {
        cooldownImage.fillAmount = 1;
        StartCoroutine(CooldownCo(cooldown));
    }

    public void ResetCooldown()
    {
        StopAllCoroutines();
        if (cooldownImage != null)
            cooldownImage.fillAmount = 0;
    }

    private void EnsureChargesText()
    {
        if (chargesText != null)
            return;

        Transform existing = transform.Find("ChargesText");
        if (existing != null)
        {
            chargesText = existing.GetComponent<TextMeshProUGUI>();
            if (chargesText != null)
                return;
        }

        GameObject chargesObj = new GameObject("ChargesText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        chargesObj.transform.SetParent(transform, false);
        chargesObj.transform.SetAsLastSibling();

        RectTransform rt = chargesObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-6, -6);
        rt.sizeDelta = new Vector2(36, 36);

        chargesText = chargesObj.GetComponent<TextMeshProUGUI>();
        if (inputKeyText != null)
        {
            chargesText.font = inputKeyText.font;
            chargesText.fontSharedMaterial = inputKeyText.fontSharedMaterial;
        }

        chargesText.fontSize = 22;
        chargesText.fontStyle = FontStyles.Bold;
        chargesText.alignment = TextAlignmentOptions.TopRight;
        chargesText.color = Color.white;
        chargesText.enableWordWrapping = false;
        chargesText.raycastTarget = false;
        chargesText.outlineColor = Color.black;
        chargesText.outlineWidth = 0.25f;
    }

    public void UpdateCharges(int current, int max)
    {
        EnsureChargesText();
        if (chargesText == null)
            return;

        chargesText.gameObject.SetActive(true);
        chargesText.text = current.ToString();

        if (current <= 0)
        {
            chargesText.color = new Color(1f, 0.4f, 0.4f, 1f);
        }
        else
        {
            chargesText.color = Color.white;
        }
    }

    public void SetChargeCooldownFill(float fillAmount)
    {
        if (cooldownImage != null)
            cooldownImage.fillAmount = Mathf.Clamp01(fillAmount);
    }

    public void ClearCharges()
    {
        if (chargesText != null)
            chargesText.gameObject.SetActive(false);

        if (cooldownImage != null)
            cooldownImage.fillAmount = 0;
    }

    public void ResumeCooldown(float remainingDuration, float totalDuration)
    {
        if (remainingDuration <= 0 || totalDuration <= 0)
        {
            ResetCooldown();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ResumeCooldownCo(remainingDuration, totalDuration));
    }

    private IEnumerator ResumeCooldownCo(float remainingDuration, float totalDuration)
    {
        float timePassed = totalDuration - remainingDuration;
        if (cooldownImage != null)
            cooldownImage.fillAmount = remainingDuration / totalDuration;

        while (timePassed < totalDuration)
        {
            timePassed += Time.deltaTime;
            if (cooldownImage != null)
                cooldownImage.fillAmount = 1f - (timePassed / totalDuration);
            yield return null;
        }

        if (cooldownImage != null)
            cooldownImage.fillAmount = 0;
    }

    private IEnumerator CooldownCo(float duration)
    {
        float timePassed = 0;

        while (timePassed < duration)
        {
            timePassed = timePassed + Time.deltaTime;
            if (cooldownImage != null)
                cooldownImage.fillAmount = 1f - (timePassed / duration);
            yield return null;
        }

        if (cooldownImage != null)
            cooldownImage.fillAmount = 0;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.skillToolTip.ShowToolTip(false, null);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skillData == null)
            return;

        ui.skillToolTip.ShowToolTip(true, rect, skillData, null);
    }
}
