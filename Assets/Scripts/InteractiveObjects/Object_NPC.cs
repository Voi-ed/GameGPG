using UnityEngine;

public class Object_NPC : MonoBehaviour, IInteractable
{
    protected Transform player;
    protected UI ui;
    protected Player_QuestManager questManager;

    [Header("Quest Info")]
    [SerializeField] private string npcTargetQuestId;
    [SerializeField] protected RewardType rewardNpc;
    [Space]
    [SerializeField] private Transform npc;
    [SerializeField] private GameObject interactToolTip;
    private bool facingRight = true;

    [Header("Floaty Tooltip")]
    [SerializeField] private float floatSpeed = 8f;
    [SerializeField] private float floatRange = .1f;
    private Vector3 startPosition;

    protected virtual void Awake()
    {
        ui = FindFirstObjectByType<UI>();
        if (interactToolTip != null)
        {
            startPosition = interactToolTip.transform.position;
            interactToolTip.SetActive(false);
        }
    }

    protected virtual void Start()
    {
        if (Player.instance != null)
            questManager = Player.instance.questManager;
    }

    protected virtual void Update()
    {
        HandleNpcFlip();
        HandleToolTipFloat();
    }

    private void HandleToolTipFloat()
    {
        if (interactToolTip != null && interactToolTip.activeSelf)
        {
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
            interactToolTip.transform.position = startPosition + new Vector3(0, yOffset);
        }
    }

    private void HandleNpcFlip()
    {
        if (player == null || npc == null)
            return;

        if (npc.position.x > player.position.x && facingRight)
        {
            npc.transform.Rotate(0, 180,0);
            facingRight = false;
        }
        else if (npc.position.x < player.position.x && facingRight == false)
        {
            npc.transform.Rotate(0, 180, 0);
            facingRight = true;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() == null)
            return;

        player = collision.transform;
        if (interactToolTip != null)
        {
            startPosition = interactToolTip.transform.position;
            interactToolTip.SetActive(true);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() == null)
            return;

        if (interactToolTip != null)
            interactToolTip.SetActive(false);
    }

    public virtual void Interact()
    {
        if (questManager == null && Player.instance != null)
            questManager = Player.instance.questManager;

        if (questManager != null && !string.IsNullOrEmpty(npcTargetQuestId))
            questManager.AddProgress(npcTargetQuestId);
    }
}
