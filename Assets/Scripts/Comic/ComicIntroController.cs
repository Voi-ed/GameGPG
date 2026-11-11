using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

/// <summary>
/// Điều phối Cutscene mở đầu game (Comic Intro):
/// Thị trấn thanh bình -> Điềm báo -> Boss tấn công -> Mọi người biến mất -> Tỉnh dậy & Chuyển sang Level 1.
/// </summary>
public class ComicIntroController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UI_Dialogue dialogueUI;
    [SerializeField] private UI_FadeScreen fadeScreen;

    [Header("Atmosphere")]
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Color ominousSkyColor = new Color(0.35f, 0.12f, 0.45f, 1f); // Tím tối tà ác

    [Header("Boss & VFX (Optional)")]
    [SerializeField] private GameObject bossVisual;

    [Header("Next Level")]
    [SerializeField] private string nextSceneName = "1_Village & Camp";

    private bool dialogueFinished;
    private bool isSkipping = false;

    private void Awake()
    {
        if (dialogueUI == null)
            dialogueUI = FindFirstObjectByType<UI_Dialogue>(FindObjectsInactive.Include);

        if (fadeScreen == null)
            fadeScreen = FindFirstObjectByType<UI_FadeScreen>(FindObjectsInactive.Include);

        if (globalLight == null)
        {
            GameObject lightObj = GameObject.Find("Global Light 2D");
            if (lightObj != null)
                globalLight = lightObj.GetComponent<Light2D>();
        }
    }

    private void Start()
    {
        StartCoroutine(PlayComicStoryCo());
    }

    private void Update()
    {
        // Cho phép nhấn Escape hoặc Tab để bỏ qua Cutscene nếu muốn
        if (!isSkipping && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab)))
        {
            SkipCutscene();
        }
    }

    private IEnumerator PlayComicStoryCo()
    {
        // 1. Thị trấn thanh bình đầu game
        yield return new WaitForSeconds(1.0f);

        yield return ShowDialogueCo("Dẫn chuyện", "Một buổi chiều tối yên ả tại thị trấn... Cuộc sống nơi đây vẫn luôn thanh bình như bao ngày.");

        yield return new WaitForSeconds(0.4f);

        // 2. Điềm báo - Bầu trời chuyển sắc tím u ám
        if (globalLight != null)
        {
            DOTween.To(() => globalLight.color, c => globalLight.color = c, ominousSkyColor, 2.5f);
        }

        yield return ShowDialogueCo("Người dân", "Ủa? Sao trời bỗng nhiên tối sầm lại thế này? Có chuyện gì đang xảy ra vậy?!");

        // 3. Boss xuất hiện tung đại chiêu
        if (bossVisual != null)
            bossVisual.SetActive(true);

        yield return ShowDialogueCo("Kẻ Thống Trị Hắc Ám", "Tất cả các ngươi... linh hồn này thuộc về bóng tối! HÃY BIẾN MẤT!");

        // 4. Toàn bộ người dân và NPC bị biến mất hàng loạt
        if (NpcManager.instance != null)
        {
            NpcManager.instance.TriggerMassDisappearance(0.06f);
        }

        yield return new WaitForSeconds(1.5f);

        yield return ShowDialogueCo("Dẫn chuyện", "Chỉ trong một cái chớp mắt... luồng năng lượng tà ác đã cuốn đi tất cả mọi người.");

        yield return ShowDialogueCo("Dẫn chuyện", "Thị trấn bỗng chốc trở nên hoang tàn, lạnh lẽo không một bóng người...");

        // Đóng khung thoại
        if (dialogueUI != null)
            dialogueUI.CloseDialogue();

        yield return new WaitForSeconds(1.0f);

        // 5. Màn hình tối dần và bước vào cuộc hành trình chính
        FinishCutsceneAndLoadScene();
    }

    private IEnumerator ShowDialogueCo(string speaker, string text, Sprite portrait = null)
    {
        if (dialogueUI == null) yield break;

        dialogueFinished = false;
        dialogueUI.PlayCustomDialogue(speaker, text, portrait, () => dialogueFinished = true);

        while (!dialogueFinished)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
    }

    private void ShakeCamera(float duration, float strength)
    {
        // Đã tắt rung camera để trải nghiệm đọc thoại êm ái và không làm giật mặt nước
    }

    public void SkipCutscene()
    {
        if (isSkipping) return;
        isSkipping = true;

        StopAllCoroutines();
        if (dialogueUI != null)
            dialogueUI.CloseDialogue();

        FinishCutsceneAndLoadScene();
    }

    private void FinishCutsceneAndLoadScene()
    {
        // 1. Đánh dấu đã xem xong Comic mở đầu
        if (GameManager.instance != null)
            GameManager.instance.SetIntroComicWatched(true);

        if (SaveManager.instance != null && SaveManager.instance.GetGameData() != null)
        {
            SaveManager.instance.GetGameData().hasWatchedIntroComic = true;
            SaveManager.instance.SaveGame();
        }

        // 2. Chuyển scene sang Làng
        if (GameManager.instance != null)
        {
            GameManager.instance.ChangeScene(nextSceneName, RespawnType.NonSpecific);
        }
        else
        {
            StartCoroutine(TransitionToNextSceneCo());
        }
    }

    private IEnumerator TransitionToNextSceneCo()
    {
        if (fadeScreen != null)
        {
            fadeScreen.DoFadeOut();
            yield return new WaitForSeconds(1.2f);
        }

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
