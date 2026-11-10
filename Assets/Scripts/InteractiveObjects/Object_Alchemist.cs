using UnityEngine;

public class Object_Alchemist : Object_NPC, IInteractable
{
    [Header("Quest & Dialogue")]
    [SerializeField] private DialogueLineSO firstDialogueLine;
    [SerializeField] private QuestDataSO[] quests;

    private Inventory_Player inventory;
    private Inventory_Merchant merchant;

    protected override void Awake()
    {
        base.Awake();
        merchant = GetComponent<Inventory_Merchant>();
    }

    public override void Interact()
    {
        base.Interact();

        if (merchant != null && ui != null && ui.merchantUI != null)
            ui.merchantUI.SetupMerchantUI(merchant, inventory);

        if (firstDialogueLine != null && ui != null)
            ui.OpenDialogueUI(firstDialogueLine, new DialogueNpcData(rewardNpc, quests));
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponentInParent<Player>() == null)
            return;

        base.OnTriggerEnter2D(collision);
        if (player != null)
            inventory = player.GetComponent<Inventory_Player>();

        if (merchant != null && inventory != null)
            merchant.SetInventory(inventory);
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponentInParent<Player>() == null)
            return;

        base.OnTriggerExit2D(collision);
        if (ui != null)
        {
            ui.HideAllTooltips();
            ui.OpenMerchantUI(false);
        }
    }
}
