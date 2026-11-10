using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Điều phối Cutscene nhân vật tỉnh dậy tại Làng (1_Village & Camp)
/// sau khi vừa kết thúc đoạn Comic Intro mở đầu.
/// Chỉ chạy đúng 1 lần duy nhất trong toàn bộ game (được kiểm soát bởi GameData.hasFinishedVillageWakeup).
/// </summary>
public class VillageWakeupCutscene : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UI_Dialogue dialogueUI;

    [Header("Cutscene Settings")]
    [SerializeField] private float initialDelay = 1.0f;
    [SerializeField] private string characterName = "Nhân vật chính";

    private bool dialogueFinished = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitAutoTrigger()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Chỉ kích hoạt tại scene Làng
        if (scene.name == "1_Village & Camp")
        {
            // Tránh tạo trùng lặp nếu đã có script trong Scene
            if (FindFirstObjectByType<VillageWakeupCutscene>() != null)
                return;

            // Điều kiện chặt chẽ:
            // 1. Phải ĐÃ XEM Comic mở đầu (hasWatchedIntroComic == true)
            // 2. Phải CHƯA TỪNG hoàn thành cutscene thức dậy tại Làng (hasFinishedVillageWakeup == false)
            bool watchedComic = false;
            bool finishedWakeup = false;

            if (GameManager.instance != null && GameManager.instance.hasWatchedIntroComic)
                watchedComic = true;

            if (SaveManager.instance != null && SaveManager.instance.GetGameData() != null)
            {
                var data = SaveManager.instance.GetGameData();
                if (data.hasWatchedIntroComic) watchedComic = true;
                if (data.hasFinishedVillageWakeup) finishedWakeup = true;
            }

            // Nếu chưa xem Comic hoặc đã hoàn thành thức dậy trước đó -> Thoát ngay lập tức
            if (!watchedComic || finishedWakeup)
                return;

            // Tự động khởi tạo bộ điều phối Cutscene
            GameObject cutsceneObj = new GameObject("___ Village Wakeup Cutscene (Auto) ___");
            cutsceneObj.AddComponent<VillageWakeupCutscene>();
        }
    }

    private void Start()
    {
        // Kiểm tra an toàn lần nữa trước khi chạy
        bool watchedComic = false;
        bool finishedWakeup = false;

        if (GameManager.instance != null && GameManager.instance.hasWatchedIntroComic)
            watchedComic = true;

        if (SaveManager.instance != null && SaveManager.instance.GetGameData() != null)
        {
            var data = SaveManager.instance.GetGameData();
            if (data.hasWatchedIntroComic) watchedComic = true;
            if (data.hasFinishedVillageWakeup) finishedWakeup = true;
        }

        if (!watchedComic || finishedWakeup)
        {
            Destroy(gameObject);
            return;
        }

        if (dialogueUI == null)
            dialogueUI = FindFirstObjectByType<UI_Dialogue>(FindObjectsInactive.Include);

        StartCoroutine(PlayWakeupSequenceCo());
    }

    private IEnumerator PlayWakeupSequenceCo()
    {
        // 1. Chờ Player và các hệ thống trong Scene ổn định
        float timeout = 3.0f;
        while (Player.instance == null && timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(initialDelay);

        // 2. Khóa tạm thời điều khiển của Player
        LockPlayer(true);

        if (dialogueUI == null)
            dialogueUI = FindFirstObjectByType<UI_Dialogue>(FindObjectsInactive.Include);

        // 3. Chuỗi thoại tỉnh dậy
        yield return ShowDialogueCo(characterName, "Ư... đầu mình nhức buốt quá... Rốt cuộc chuyện gì vừa xảy ra thế này?");

        yield return ShowDialogueCo(characterName, "Mọi người đâu hết rồi?! Cả ngôi làng... không còn một bóng người?!");

        yield return ShowDialogueCo(characterName, "Luồng hắc khí tà ác đó... Mình phải đi tìm hiểu xem lũ quái vật đã bắt mọi người đi đâu và giải cứu họ!");

        // 4. Đóng hộp thoại
        if (dialogueUI != null)
            dialogueUI.CloseDialogue();

        yield return new WaitForSeconds(0.3f);

        // 5. Đánh dấu đã hoàn thành phân cảnh tỉnh dậy và lưu game
        if (GameManager.instance != null && SaveManager.instance != null && SaveManager.instance.GetGameData() != null)
        {
            SaveManager.instance.GetGameData().hasFinishedVillageWakeup = true;
            SaveManager.instance.SaveGame();
        }

        // Cập nhật mục tiêu chỉ dẫn trên HUD ngay sau khi tỉnh dậy
        HintManager.instance?.UpdateHintDisplay();

        // 6. Trả lại quyền điều khiển cho Player
        LockPlayer(false);

        // Tự hủy GameObject helper này sau khi hoàn thành nhiệm vụ
        Destroy(gameObject);
    }

    private IEnumerator ShowDialogueCo(string speaker, string text, Sprite portrait = null)
    {
        if (dialogueUI == null)
            yield break;

        dialogueFinished = false;
        dialogueUI.PlayCustomDialogue(speaker, text, portrait, () => dialogueFinished = true);

        while (!dialogueFinished)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
    }

    private void LockPlayer(bool isLocked)
    {
        if (Player.instance != null)
        {
            if (isLocked)
            {
                Player.instance.input?.Disable();
                Player.instance.SetVelocity(0, 0);
            }
            else
            {
                Player.instance.input?.Enable();
            }
        }
    }
}
