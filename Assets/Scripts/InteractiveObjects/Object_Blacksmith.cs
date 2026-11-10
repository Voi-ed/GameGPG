using UnityEngine;

public class Object_Blacksmith : Object_NPC, IInteractable
{
    private Animator anim;
    private Inventory_Player inventory;
    private Inventory_Storage storage;

    protected override void Awake()
    {
        base.Awake();
        storage = GetComponent<Inventory_Storage>();
        anim = GetComponentInChildren<Animator>();
        if (anim != null)
            anim.SetBool("isBlacksmith", true);
    }

    public override void Interact()
    {
        base.Interact();
        if (ui != null)
        {
            if (ui.storageUI != null && storage != null)
                ui.storageUI.SetupStorageUI(storage);
            if (ui.craftUI != null && storage != null)
                ui.craftUI.SetupCraftUI(storage);

            ui.OpenStorageUI(true);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (player == null)
            return;

        inventory = player.GetComponent<Inventory_Player>();
        if (storage != null && inventory != null)
            storage.SetInventory(inventory);
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponentInParent<Player>() == null)
            return;

        base.OnTriggerExit2D(collision);
        if (ui != null)
        {
            ui.HideAllTooltips();
            ui.OpenStorageUI(false);
        }
    }

}
