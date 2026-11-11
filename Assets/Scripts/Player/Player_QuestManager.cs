using System.Collections.Generic;
using UnityEngine;

public class Player_QuestManager : MonoBehaviour, ISaveable
{
    public List<QuestData> activeQuests;
    public List<QuestData> completedQuests;

    public System.Action OnQuestListUpdated;

    private Entity_DropManager dropManager;
    private Inventory_Player inventory;

    [Header("QUEST DATABASE")]
    [SerializeField] private QuestDatabaseSO questDatabase;
    public QuestDatabaseSO QuestDatabase => questDatabase;

    private void Awake()
    {
        dropManager = GetComponent<Entity_DropManager>();
        inventory = GetComponent<Inventory_Player>();
    }



    public void TryGetRewardFrom(RewardType npcType)
    {
        List<QuestData> getRewardQuests = new List<QuestData>();

        foreach (var quest in activeQuests)
        {
            if (quest == null || quest.questDataSO == null) continue;

            if (quest.CanGetReward() && quest.questDataSO.rewardType == npcType)
                getRewardQuests.Add(quest);
        }

        foreach (var quest in getRewardQuests)
        {
            ClaimQuestRewardAndComplete(quest);
        }
    }

    private void GiveQuestReward(QuestDataSO questDataSO)
    {
        if (questDataSO == null || questDataSO.rewardItems == null) return;

        foreach (var item in questDataSO.rewardItems)
        {
            if (item == null || item.itemData == null) continue;

            for (int i = 0; i < item.stackSize; i++)
            {
                Inventory_Item itemToAdd = new Inventory_Item(item.itemData);
                bool added = false;

                if (inventory != null && inventory.CanAddItem(itemToAdd))
                {
                    inventory.AddItem(itemToAdd);
                    added = true;
                }

                if (!added && dropManager != null)
                {
                    dropManager.CreateItemDrop(item.itemData);
                }
            }
        }
    }

    public void ClaimQuestRewardAndComplete(QuestData questData)
    {
        if (questData == null || questData.questDataSO == null) return;

        if (questData.questDataSO.questType == QuestType.Delivery)
        {
            var requiredItem = questData.questDataSO.itemToDeliver;
            var requiredAmount = questData.questDataSO.requiredAmount;

            if (inventory != null && inventory.HasItemAmount(requiredItem, requiredAmount))
            {
                inventory.RemoveItemAmount(requiredItem, requiredAmount);
            }
        }

        GiveQuestReward(questData.questDataSO);
        CompleteQuest(questData);
    }

    public bool HasCompletedQuest()
    {
        for (int i = 0; i < activeQuests.Count; i++)
        {
            QuestData quest = activeQuests[i];

            if (quest.questDataSO.questType == QuestType.Delivery)
            {
                var requiredItem = quest.questDataSO.itemToDeliver;
                var requiredAmount = quest.questDataSO.requiredAmount;

                if (inventory.HasItemAmount(requiredItem, requiredAmount))
                    return true;
            }

            if (quest.CanGetReward())
                return true;
        }

        return false;
    }

    public void AddProgress(string questTargetId, int amount = 1)
    {
        bool updated = false;

        foreach (var quest in activeQuests)
        {
            if (quest.questDataSO.questTargetId != questTargetId)
                continue;

            if (quest.CanGetReward() == false)
            {
                quest.AddQuestProgress(amount);
                updated = true;
            }
        }

        if (updated)
        {
            SaveManager.instance?.SaveGame();
            OnQuestListUpdated?.Invoke();
        }
    }

    public int GetQuestProgress(QuestData questToCheck)
    {
        QuestData quest = activeQuests.Find(q => q == questToCheck);
        return quest != null ? quest.GetCurrentAmount() : 0;
    }

    /// <summary>
    /// Kiểm tra người chơi có được nhận thêm nhiệm vụ mới hay không (Giới hạn tối đa 1 nhiệm vụ đang làm).
    /// </summary>
    public bool CanAcceptNewQuest()
    {
        return activeQuests == null || activeQuests.Count == 0;
    }

    public void AcceptQuest(QuestDataSO questDataSO)
    {
        if (questDataSO == null) return;

        if (!CanAcceptNewQuest())
        {
            Debug.LogWarning($"[QuestManager] Không thể nhận '{questDataSO.questName}' vì đang có nhiệm vụ chưa hoàn tất!");
            return;
        }

        activeQuests.Add(new QuestData(questDataSO));
        SaveManager.instance?.SaveGame();
        OnQuestListUpdated?.Invoke();
    }

    public void CompleteQuest(QuestData questData)
    {
        completedQuests.Add(questData);
        activeQuests.Remove(questData);
        SaveManager.instance?.SaveGame();
        OnQuestListUpdated?.Invoke();
    }

    public bool QuestIsActive(QuestDataSO questToCheck)
    {
        if(questToCheck == null)
            return false;

        return activeQuests.Find(q => q.questDataSO == questToCheck) != null;
    }

    public bool QuestIsCompleted(QuestDataSO questToCheck)
    {
        if (questToCheck == null)
            return false;

        if (completedQuests.Find(q => q != null && q.questDataSO == questToCheck) != null)
            return true;

        if (SaveManager.instance != null && SaveManager.instance.GetGameData() != null)
        {
            var data = SaveManager.instance.GetGameData();
            if (data.completedQuests.TryGetValue(questToCheck.questSaveId, out bool isComp) && isComp)
                return true;
        }

        return false;
    }

    public void LoadData(GameData data)
    {
        activeQuests.Clear();
        completedQuests.Clear();

        if (questDatabase == null)
        {
            Debug.LogWarning("Quest database is not assigned in Player_QuestManager!");
            return;
        }

        foreach (var entry in data.activeQuests)
        {
            string questSaveId = entry.Key;
            int progress = entry.Value;

            QuestDataSO questDataSO = questDatabase.GetQuestById(questSaveId);

            if (questDataSO == null)
            {
                Debug.Log(questSaveId + " was not found in the database!");
                continue;
            }

            QuestData questToLoad = new QuestData(questDataSO);
            questToLoad.currentAmount = progress;

            activeQuests.Add(questToLoad);
        }

        foreach (var entry in data.completedQuests)
        {
            string questSaveId = entry.Key;

            QuestDataSO questDataSO = questDatabase.GetQuestById(questSaveId);

            if (questDataSO == null)
            {
                Debug.Log(questSaveId + " was not found in the database!");
                continue;
            }

            QuestData completedQuest = new QuestData(questDataSO);
            completedQuest.currentAmount = questDataSO.requiredAmount;
            completedQuests.Add(completedQuest);
        }

        OnQuestListUpdated?.Invoke();
    }

    public void SaveData(ref GameData data)
    {
        data.activeQuests.Clear();
        data.completedQuests.Clear();

        foreach (var quest in activeQuests)
        {
            if (quest != null && quest.questDataSO != null)
                data.activeQuests[quest.questDataSO.questSaveId] = quest.currentAmount;
        }

        foreach (var quest in completedQuests)
        {
            if (quest != null && quest.questDataSO != null)
                data.completedQuests[quest.questDataSO.questSaveId] = true;
        }
    }
}

