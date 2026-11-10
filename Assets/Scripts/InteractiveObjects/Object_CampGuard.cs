using UnityEngine;

public class Object_CampGuard : Object_NPC, IInteractable
{
    [Header("Quest & Dialogue")]
    [SerializeField] private DialogueLineSO firstDialogueLine;
    [SerializeField] private QuestDataSO[] quests;

    [Header("Boss Challenge")]
    [Tooltip("Nếu bật, khi ấn F sẽ mở trực tiếp bảng Khiêu chiến Boss thay vì mở hội thoại")]
    [SerializeField] private bool directOpenBossChallenge = false;

    public override void Interact()
    {
        base.Interact();

        if (directOpenBossChallenge && ui != null)
        {
            ui.OpenBossChallengeUI(true);
            return;
        }

        if (firstDialogueLine != null && ui != null)
            ui.OpenDialogueUI(firstDialogueLine, new DialogueNpcData(rewardNpc, quests));
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponentInParent<Player>() == null)
            return;

        base.OnTriggerExit2D(collision);
        if (ui != null)
        {
            ui.HideAllTooltips();
        }
    }
}
