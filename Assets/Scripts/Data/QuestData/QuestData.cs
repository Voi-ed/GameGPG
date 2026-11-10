using System;


[Serializable]
public class QuestData 
{
    public QuestDataSO questDataSO;
    public int currentAmount;
    public bool canGetReward;


    public void AddQuestProgress(int amount = 1)
    {
        currentAmount = currentAmount + amount;
        canGetReward = CanGetReward();
    }

    public bool CanGetReward()
    {
        if (questDataSO == null) return false;

        // Nếu là nhiệm vụ giao nộp vật phẩm (Delivery), kiểm tra số lượng vật phẩm trong Inventory của Player
        if (questDataSO.questType == QuestType.Delivery)
        {
            if (Player.instance != null && Player.instance.inventory != null && questDataSO.itemToDeliver != null)
            {
                return Player.instance.inventory.HasItemAmount(questDataSO.itemToDeliver, questDataSO.requiredAmount);
            }
        }

        return currentAmount >= questDataSO.requiredAmount;
    }

    public int GetCurrentAmount()
    {
        if (questDataSO != null && questDataSO.questType == QuestType.Delivery)
        {
            if (Player.instance != null && Player.instance.inventory != null && questDataSO.itemToDeliver != null)
            {
                int count = 0;
                foreach (var item in Player.instance.inventory.itemList)
                {
                    if (item != null && item.itemData == questDataSO.itemToDeliver)
                        count += item.stackSize;
                }
                return count;
            }
        }
        return currentAmount;
    }

    public QuestData(QuestDataSO questSO)
    {
        this.questDataSO = questSO;
    }
}
