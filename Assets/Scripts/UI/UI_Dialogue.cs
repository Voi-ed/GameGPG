using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Dialogue : MonoBehaviour
{
    private UI ui;
    private DialogueNpcData npcData;
    private Player_QuestManager questManager;

    [SerializeField] private Image speakerPortrait;
    [SerializeField] private TextMeshProUGUI speakerName;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI[] dialogueChoicesText;

    [Space]
    [SerializeField] private float textSpeed = .03f;
    private string fullTextToShow;
    private Coroutine typeTextCo;

    private DialogueLineSO currentLine;
    private DialogueLineSO[] currentChoices;
    private DialogueLineSO selectedChoice;
    private int selectedChoiceIndex;

    private bool waitingToConfirm;
    private bool canInteract;

    // Hỗ trợ Comic / Cutscene không phụ thuộc Quest & Player
    private bool isCustomDialogue;
    private System.Action onCustomDialogueFinished;

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        if (Player.instance != null)
            questManager = Player.instance.questManager;
    }

    private void Update()
    {
        // Cho phép click chuột hoặc phím Space/E/Enter để tiếp tục thoại
        if (gameObject.activeInHierarchy && canInteract)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
            {
                DialogueInteraction();
            }
        }
    }

    public void SetupNpcData(DialogueNpcData npcData) => this.npcData = npcData;

    /// <summary>
    /// Hiển thị lời thoại tùy biến (dành riêng cho Comic, Cutscene mà không cần ScriptableObject)
    /// </summary>
    public void PlayCustomDialogue(string speaker, string text, Sprite portrait = null, System.Action onFinished = null)
    {
        gameObject.SetActive(true);
        currentLine = null;
        currentChoices = null;
        selectedChoice = null;
        canInteract = false;
        waitingToConfirm = false;
        isCustomDialogue = true;
        onCustomDialogueFinished = onFinished;

        HideAllChoices();

        if (speakerPortrait != null)
        {
            if (portrait != null)
            {
                speakerPortrait.gameObject.SetActive(true);
                speakerPortrait.sprite = portrait;
            }
            else
            {
                speakerPortrait.gameObject.SetActive(false);
            }
        }

        if (speakerName != null)
        {
            speakerName.text = speaker;
            speakerName.color = Color.white;
        }

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);
            dialogueText.color = Color.white;
        }

        fullTextToShow = text;
        typeTextCo = StartCoroutine(TypeTextCo(fullTextToShow));
        StartCoroutine(EnableInteractionCo());
    }

    public void PlayDialogueLine(DialogueLineSO line)
    {
        isCustomDialogue = false;
        onCustomDialogueFinished = null;
        currentLine = line;
        currentChoices = line.choiceLines;
        canInteract = false;
        selectedChoice = null;
        selectedChoiceIndex = 0;

        HideAllChoices();

        if (dialogueText != null)
            dialogueText.gameObject.SetActive(true);

        if (speakerPortrait != null)
        {
            speakerPortrait.gameObject.SetActive(line.speaker != null && line.speaker.speakerPortrait != null);
            if (line.speaker != null)
                speakerPortrait.sprite = line.speaker.speakerPortrait;
        }

        if (speakerName != null && line.speaker != null)
            speakerName.text = line.speaker.speakerName;

        fullTextToShow = line.actionType == DialogueActionType.None || line.actionType == DialogueActionType.PlayerMakeChoice ?
            line.GetRandomLine() : line.actionLine;

        typeTextCo = StartCoroutine(TypeTextCo(fullTextToShow));
        StartCoroutine(EnableInteractionCo());
    }

    private void HandleNextAction()
    {
        if (currentLine == null)
        {
            CloseDialogue();
            return;
        }

        switch (currentLine.actionType)
        {
            case DialogueActionType.OpenShop:
                if (ui != null)
                {
                    ui.SwitchToInGameUI();
                    ui.OpenMerchantUI(true);
                }
                break;
            case DialogueActionType.PlayerMakeChoice:
                if (selectedChoice == null)
                {
                    ShowChoices();
                }
                else
                {
                    DialogueLineSO selectedChoice = currentChoices[selectedChoiceIndex];
                    PlayDialogueLine(selectedChoice);
                }
                break;
            case DialogueActionType.OpenQuest:
                if (ui != null)
                {
                    QuestData completedQuest = null;
                    QuestData unfinishedQuestOfThisNpc = null;
                    QuestData anyUnfinishedQuest = null;

                    if (questManager != null && questManager.activeQuests != null)
                    {
                        // 1. Ưu tiên tìm quest đã hoàn thành của NPC này
                        completedQuest = questManager.activeQuests.Find(q =>
                            q != null && q.CanGetReward() &&
                            (npcData == null || npcData.quests == null || npcData.quests.Length == 0 ||
                             System.Array.Exists(npcData.quests, nq => nq == q.questDataSO)));

                        // 2. Nếu NPC này không có quest riêng đã xong, nhưng người chơi có bất kỳ quest nào đã xong -> CHO PHÉP TRẢ LUÔN!
                        if (completedQuest == null)
                        {
                            completedQuest = questManager.activeQuests.Find(q => q != null && q.CanGetReward());
                        }

                        // 3. Tìm quest đang làm dở của NPC này
                        unfinishedQuestOfThisNpc = questManager.activeQuests.Find(q =>
                            q != null && !q.CanGetReward() &&
                            (npcData == null || npcData.quests == null || npcData.quests.Length == 0 ||
                             System.Array.Exists(npcData.quests, nq => nq == q.questDataSO)));

                        // 4. Tìm quest đang làm dở của NPC khác
                        anyUnfinishedQuest = questManager.activeQuests.Find(q => q != null && !q.CanGetReward());
                    }

                    // ƯU TIÊN SỐ 1: BÁO CÁO & TRẢ THƯỞNG CHO NHIỆM VỤ ĐÃ HOÀN THÀNH
                    if (completedQuest != null)
                    {
                        string questName = completedQuest.questDataSO.questName;
                        questManager.ClaimQuestRewardAndComplete(completedQuest);

                        string speaker = npcData != null && currentLine != null && currentLine.speaker != null ? currentLine.speaker.speakerName : "Người Giao Việc";
                        Sprite portrait = currentLine != null && currentLine.speaker != null ? currentLine.speaker.speakerPortrait : null;

                        PlayCustomDialogue(
                            speaker,
                            $"Xuất sắc lắm! Cậu đã hoàn thành nhiệm vụ <b>'{questName}'</b>.\nĐây là chiến lợi phẩm và phần thưởng của cậu!",
                            portrait,
                            () =>
                            {
                                if (npcData != null && npcData.quests != null)
                                {
                                    ui.SwitchToInGameUI();
                                    ui.OpenQuestUI(npcData.quests);
                                }
                                else
                                {
                                    CloseDialogue();
                                }
                            }
                        );
                        return;
                    }

                    // ƯU TIÊN SỐ 2: NẾU ĐANG CÓ NHIỆM VỤ DỞ DANG CỦA CHÍNH NPC NÀY
                    if (unfinishedQuestOfThisNpc != null)
                    {
                        string speaker = npcData != null && currentLine != null && currentLine.speaker != null ? currentLine.speaker.speakerName : "Người Giao Việc";
                        Sprite portrait = currentLine != null && currentLine.speaker != null ? currentLine.speaker.speakerPortrait : null;

                        PlayCustomDialogue(
                            speaker,
                            $"Cậu đang thực hiện nhiệm vụ <b>'{unfinishedQuestOfThisNpc.questDataSO.questName}'</b> ({unfinishedQuestOfThisNpc.currentAmount}/{unfinishedQuestOfThisNpc.questDataSO.requiredAmount}).\nHãy hoàn thành mục tiêu trước đã!",
                            portrait,
                            () => CloseDialogue()
                        );
                        return;
                    }

                    // ƯU TIÊN SỐ 3: NẾU ĐANG CÓ NHIỆM VỤ DỞ DANG CỦA NPC KHÁC (Áp dụng luật 1 nhiệm vụ tại một thời điểm)
                    if (anyUnfinishedQuest != null)
                    {
                        string speaker = npcData != null && currentLine != null && currentLine.speaker != null ? currentLine.speaker.speakerName : "Người Giao Việc";
                        Sprite portrait = currentLine != null && currentLine.speaker != null ? currentLine.speaker.speakerPortrait : null;

                        PlayCustomDialogue(
                            speaker,
                            $"Cậu đang bận làm nhiệm vụ <b>'{anyUnfinishedQuest.questDataSO.questName}'</b>.\nHãy hoàn thành xong xuôi nhiệm vụ đó trước khi nhận thêm việc mới nhé!",
                            portrait,
                            () => CloseDialogue()
                        );
                        return;
                    }

                    bool hasAnyUnfinished = false;
                    if (npcData != null && npcData.quests != null)
                    {
                        foreach (var q in npcData.quests)
                        {
                            if (q == null) continue;
                            if (questManager != null && !questManager.QuestIsCompleted(q))
                            {
                                hasAnyUnfinished = true;
                                break;
                            }
                        }
                    }

                    if (!hasAnyUnfinished && npcData != null && npcData.quests != null && npcData.quests.Length > 0)
                    {
                        string speaker = npcData != null && currentLine != null && currentLine.speaker != null ? currentLine.speaker.speakerName : "Người Giao Việc";
                        Sprite portrait = currentLine != null && currentLine.speaker != null ? currentLine.speaker.speakerPortrait : null;

                        PlayCustomDialogue(
                            speaker,
                            "Cậu đã hoàn thành xuất sắc tất cả nhiệm vụ bảo vệ tiền đồn! Làng chúng ta ghi nhận đại công của cậu!\nĐấu Trường Boss đã mở ra, hãy tiếp tục tự do khiêu chiến các tầng Boss tiếp theo cho đến khi tiêu diệt Chúa Quỷ ArchDemon nhé!",
                            portrait,
                            () => CloseDialogue()
                        );
                    }
                    else
                    {
                        ui.SwitchToInGameUI();
                        if (npcData != null)
                            ui.OpenQuestUI(npcData.quests);
                    }
                }
                break;
            case DialogueActionType.GetQuestReward:
                if (ui != null)
                {
                    ui.SwitchToInGameUI();
                    if (questManager != null)
                    {
                        if (npcData != null)
                            questManager.TryGetRewardFrom(npcData.npcRewardType);

                        if (questManager.activeQuests != null)
                        {
                            var completedList = questManager.activeQuests.FindAll(q => q != null && q.CanGetReward());
                            foreach (var cq in completedList)
                            {
                                questManager.ClaimQuestRewardAndComplete(cq);
                            }
                        }
                    }
                }
                break;
            case DialogueActionType.OpenCraft:
                if (ui != null)
                {
                    ui.SwitchToInGameUI();
                    ui.OpenCraftUI(true);
                }
                break;
            case DialogueActionType.OpenBossChallenge:
                if (ui != null)
                {
                    ui.SwitchToInGameUI();
                    ui.OpenBossChallengeUI(true);
                }
                break;
            case DialogueActionType.CloseDialogue:
                CloseDialogue();
                break;
        }
    }

    public void CloseDialogue()
    {
        CompleteTyping();
        gameObject.SetActive(false);
        if (ui != null)
            ui.SwitchToInGameUI();
    }

    public void DialogueInteraction()
    {
        if (canInteract == false)
            return;

        if (typeTextCo != null)
        {
            CompleteTyping();
            waitingToConfirm = true;
            return;
        }

        if (isCustomDialogue)
        {
            if (waitingToConfirm)
            {
                waitingToConfirm = false;
                isCustomDialogue = false;
                var callback = onCustomDialogueFinished;
                onCustomDialogueFinished = null;
                callback?.Invoke();
            }
            return;
        }

        if (waitingToConfirm || selectedChoice != null)
        {
            waitingToConfirm = false;
            HandleNextAction();
        }
    }

    private void CompleteTyping()
    {
        if (typeTextCo != null)
        {
            StopCoroutine(typeTextCo);
            dialogueText.text = fullTextToShow;
            typeTextCo = null;
        }
    }

    private void ShowChoices()
    {
        for (int i = 0; i < dialogueChoicesText.Length; i++)
        {
            if (i < currentChoices.Length)
            {
                DialogueLineSO choice = currentChoices[i];
                string choiceText = choice.playerChoiceAnswer;

                if (choice.actionType == DialogueActionType.OpenQuest)
                {
                    QuestData completedQ = null;
                    if (questManager != null && questManager.activeQuests != null)
                    {
                        completedQ = questManager.activeQuests.Find(q =>
                            q != null && q.CanGetReward() &&
                            (npcData == null || npcData.quests == null || npcData.quests.Length == 0 ||
                             System.Array.Exists(npcData.quests, nq => nq == q.questDataSO)));

                        if (completedQ == null)
                        {
                            completedQ = questManager.activeQuests.Find(q => q != null && q.CanGetReward());
                        }
                    }

                    if (completedQ != null)
                    {
                        choiceText = $"<color=#00FF7F>[Hoàn Thành] Báo cáo: {completedQ.questDataSO.questName} (Nhận Thưởng)</color>";
                    }
                }

                dialogueChoicesText[i].gameObject.SetActive(true);
                dialogueChoicesText[i].text = selectedChoiceIndex == i ?
                    $"<color=yellow>> {i + 1}) {choiceText}</color>" :
                    $"  {i + 1}) {choiceText}";

                if (choice.actionType == DialogueActionType.GetQuestReward && (questManager == null || questManager.HasCompletedQuest() == false))
                    dialogueChoicesText[i].gameObject.SetActive(false);
            }
            else
            {
                dialogueChoicesText[i].gameObject.SetActive(false);
            }
        }

        selectedChoice = currentChoices[selectedChoiceIndex];
    }

    private void HideAllChoices()
    {
        if (dialogueChoicesText == null) return;
        foreach (var obj in dialogueChoicesText)
        {
            if (obj != null)
                obj.gameObject.SetActive(false);
        }
    }

    public void NavigateChoice(int direction)
    {
        if (currentChoices == null || currentChoices.Length <= 1)
            return;

        selectedChoiceIndex = selectedChoiceIndex + direction;
        selectedChoiceIndex = Mathf.Clamp(selectedChoiceIndex, 0, currentChoices.Length - 1);
        ShowChoices();
    }

    private IEnumerator TypeTextCo(string text)
    {
        dialogueText.text = "";

        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        if (isCustomDialogue || (currentLine != null && currentLine.actionType != DialogueActionType.PlayerMakeChoice))
        {
            waitingToConfirm = true;
        }
        else if (currentLine != null)
        {
            yield return new WaitForSeconds(.2f);
            selectedChoice = null;
            HandleNextAction();
        }

        typeTextCo = null;
    }

    private IEnumerator EnableInteractionCo()
    {
        yield return null;
        canInteract = true;
    }
}
