using UnityEngine;

public class UI_Quest : MonoBehaviour, ISaveable
{
    private GameData currentGameData;

    [SerializeField] private UI_ItemSlotParent inventorySlots;
    [SerializeField] private UI_QuestPreviw questPreviw;

    private UI_QuestSlot[] questSlots;
    public Player_QuestManager questManager {  get; private set; }

    private void Awake()
    {
        questSlots = GetComponentsInChildren<UI_QuestSlot>(true);
        questManager = Player.instance.questManager;
    }

    public void SetupQuestUI(QuestDataSO[] questsToSetup)
    {
        foreach(var slot in questSlots)
            slot.gameObject.SetActive(false);

        if (questsToSetup == null || questsToSetup.Length == 0)
            return;

        // Chế độ nhiệm vụ tuần tự: Chỉ hiển thị duy nhất nhiệm vụ kế tiếp chưa hoàn thành
        QuestDataSO nextAvailableQuest = null;
        for (int i = 0; i < questsToSetup.Length; i++)
        {
            var q = questsToSetup[i];
            if (q == null) continue;

            bool isCompleted = questManager != null && questManager.QuestIsCompleted(q);
            if (!isCompleted && currentGameData != null)
            {
                isCompleted = currentGameData.completedQuests.TryGetValue(q.questSaveId, out bool c) && c;
            }

            if (!isCompleted)
            {
                nextAvailableQuest = q;
                break; // Chỉ lấy nhiệm vụ đầu tiên chưa làm trong chuỗi
            }
        }

        if (nextAvailableQuest != null && questSlots.Length > 0)
        {
            questSlots[0].gameObject.SetActive(true);
            questSlots[0].SetupQuestSlot(nextAvailableQuest);
        }

        questPreviw.MakeQuestPreviwEmpty();
        if (Player.instance != null && Player.instance.inventory != null)
            inventorySlots.UpdateSlots(Player.instance.inventory.itemList);

        UpdateQuestList();
    }

    public void UpdateQuestList()
    {
        foreach (var slot in questSlots)
        {
            if (slot.questInSlot == null) continue;

            if (slot.gameObject.activeSelf && CanTakeQuest(slot.questInSlot) == false)
                slot.gameObject.SetActive(false);
        }
    }

    private bool CanTakeQuest(QuestDataSO questToCheck)
    {
        if (questToCheck == null) return false;
        bool questActive = questManager != null && questManager.QuestIsActive(questToCheck);
        bool questCompleted = questManager != null && questManager.QuestIsCompleted(questToCheck);

        if (!questCompleted && currentGameData != null)
        {
            questCompleted =
                currentGameData.completedQuests.TryGetValue(questToCheck.questSaveId, out bool isCompleted) && isCompleted;
        }

        return questActive == false && questCompleted == false;
    }

    public UI_QuestPreviw GetQuestPreviw() => questPreviw;

    public void LoadData(GameData data)
    {
        currentGameData = data;
    }

    public void SaveData(ref GameData data)
    {
        
    }
}
