using UnityEngine;

public class Object_CampGuard : Object_NPC, IInteractable
{
    [Header("Quest & Dialogue")]
    [SerializeField] private DialogueLineSO firstDialogueLine;
    [SerializeField] private QuestDataSO[] quests;

    public override void Interact()
    {
        base.Interact();

        if (firstDialogueLine != null && ui != null)
            ui.OpenDialogueUI(firstDialogueLine, new DialogueNpcData(rewardNpc, quests));
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() == null)
            return;

        base.OnTriggerExit2D(collision);
        if (ui != null)
        {
            ui.HideAllTooltips();
        }
    }
}
