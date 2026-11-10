using TMPro;
using UnityEngine;

public class UI_QuestPreviw : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questName;
    [SerializeField] private TextMeshProUGUI questDescription;
    [SerializeField] private TextMeshProUGUI questGoal;
    [SerializeField] private UI_QuestRewardSlot[] questReward;

    [SerializeField] private GameObject[] additionalObjects;
    private UI_Quest questUI;
    private QuestDataSO previwQuest;

    public void SetupQuestPreviw(QuestDataSO questDataSO)
    {
        questUI = transform.root.GetComponentInChildren<UI_Quest>();
        previwQuest = questDataSO;

        EnableAdditonalObjects(true);
        EnableQuestRewardObjects(false);

        questName.text = questDataSO.questName;
        questDescription.text = questDataSO.description;
        questGoal.text = questDataSO.questGoal + " " + questDataSO.requiredAmount;

        for (int i = 0; i < questDataSO.rewardItems.Length; i++)
        {
            Inventory_Item rewardItem = new Inventory_Item(questDataSO.rewardItems[i].itemData);
            rewardItem.stackSize = questDataSO.rewardItems[i].stackSize;

            questReward[i].gameObject.SetActive(true);
            questReward[i].UpdateSlot(rewardItem);
        }
    }

    public void AcceptQuestBTN()
    {
        if (questUI != null && questUI.questManager != null)
        {
            if (!questUI.questManager.CanAcceptNewQuest())
            {
                if (questDescription != null)
                {
                    questDescription.text = "<color=#FF4444>⚠️ Cậu đang có nhiệm vụ chưa hoàn tất! Hãy hoàn thành nhiệm vụ hiện tại trước khi nhận việc mới.</color>";
                }
                return;
            }
        }

        MakeQuestPreviwEmpty();

        if (questUI != null && questUI.questManager != null)
        {
            questUI.questManager.AcceptQuest(previwQuest);
            questUI.UpdateQuestList();
        }
    }

    public void MakeQuestPreviwEmpty()
    {
        questName.text = "";
        questDescription.text = "";

        EnableAdditonalObjects(false);
        EnableQuestRewardObjects(false);
    }

    private void EnableAdditonalObjects(bool enable)
    {
        foreach (var obj in additionalObjects)
            obj.SetActive(enable);
    }

    private void EnableQuestRewardObjects(bool enable)
    {
        foreach (var obj in questReward)
            obj.gameObject.SetActive(enable);
    }
}
