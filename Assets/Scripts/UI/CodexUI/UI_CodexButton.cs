using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UI_CodexButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (UI.instance != null)
        {
            UI.instance.ToggleCodexUI();
        }
    }
}

