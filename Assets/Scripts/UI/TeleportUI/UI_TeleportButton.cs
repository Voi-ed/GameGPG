using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_TeleportButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI mapNameText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Image buttonImage;

    private string targetSceneName;
    private Action<string> onTeleportClicked;

    private void Awake()
    {
        EnsureComponents();
    }

    private void EnsureComponents()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (mapNameText == null)
            mapNameText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
    }

    public void Setup(string sceneName, string displayName, bool isCurrentMap, Action<string> onClickCallback, TextMeshProUGUI explicitText = null)
    {
        targetSceneName = sceneName;
        onTeleportClicked = onClickCallback;

        EnsureComponents();

        if (explicitText != null)
            mapNameText = explicitText;

        if (mapNameText != null)
        {
            mapNameText.text = isCurrentMap ? $"{displayName}  <color=#00e5ff>[Hiện tại]</color>" : displayName;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
            button.interactable = !isCurrentMap;
        }

        if (buttonImage != null)
        {
            buttonImage.color = isCurrentMap ? new Color(0.2f, 0.3f, 0.4f, 0.6f) : new Color(0.18f, 0.22f, 0.28f, 0.9f);
        }
    }

    private void HandleClick()
    {
        onTeleportClicked?.Invoke(targetSceneName);
    }
}
