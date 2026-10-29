using UnityEngine;
using UnityEngine.SceneManagement;

public class Object_Portal : MonoBehaviour, ISaveable, IInteractable
{
    public static Object_Portal instnace;

    public bool isActive { get; private set; }

    [Header("Teleport Gate Setup")]
    [Tooltip("Nếu bật: Cổng là Teleport Gate cố định ở map, không tự ẩn và ấn F để mở Menu chọn 8 Map")]
    [SerializeField] private bool isFixedGate = true;
    [SerializeField] private GameObject interactToolTip;

    [Header("Floaty Tooltip")]
    [SerializeField] private float floatSpeed = 8f;
    [SerializeField] private float floatRange = 0.1f;
    private Vector3 tooltipLocalStartPos;

    [Header("Town Portal Scroll Setup (Legacy)")]
    [SerializeField] private Vector2 defaultPosition; // where portal apperas in town
    [SerializeField] private string townSceneName = "1_Village & Camp";

    [SerializeField] private Transform respawnPoint;
    [SerializeField] private bool canBeTriggered = true;

    private string currentSceneName;
    private string returnSceneName;
    private bool returningFromTown;

    private void Awake()
    {
        instnace = this;
        currentSceneName = SceneManager.GetActiveScene().name;

        if (interactToolTip != null)
        {
            tooltipLocalStartPos = interactToolTip.transform.localPosition;
            interactToolTip.SetActive(false);
        }

        if (isFixedGate)
        {
            isActive = true;
            canBeTriggered = true;
        }
        else
        {
            transform.position = new Vector3(9999, 9999); // Hide by default
        }
    }

    private void Update()
    {
        HandleToolTipFloat();
    }

    private void HandleToolTipFloat()
    {
        if (interactToolTip != null && interactToolTip.activeSelf)
        {
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
            interactToolTip.transform.localPosition = tooltipLocalStartPos + new Vector3(0, yOffset, 0);
            interactToolTip.transform.rotation = Quaternion.identity;
        }
    }

    public void ActivatePortal(Vector3 position, int facingDir = 1)
    {
        isActive = true;
        transform.position = position;
        SaveManager.instance?.GetGameData()?.inScenePortals?.Clear();

        if (facingDir == -1)
            transform.Rotate(0, 180, 0);
    }

    public void DisableIfNeeded()
    {
        if (isFixedGate)
            return;

        if (returningFromTown == false)
            return;

        SaveManager.instance?.GetGameData()?.inScenePortals?.Remove(currentSceneName);
        isActive = false;
        transform.position = new Vector3(9999, 9999);
    }

    private void UseTeleport()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlayGlobalSFX("portal_teleport");

        string destinationScene = InTown() ? returnSceneName : townSceneName;
        GameManager.instance?.ChangeScene(destinationScene, RespawnType.Portal);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() == null && collision.GetComponentInParent<Player>() == null)
            return;

        if (canBeTriggered == false)
            return;

        if (isFixedGate)
        {
            if (interactToolTip != null)
                interactToolTip.SetActive(true);
        }
        else
        {
            UseTeleport();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() == null && collision.GetComponentInParent<Player>() == null)
            return;

        canBeTriggered = true;

        if (interactToolTip != null)
            interactToolTip.SetActive(false);
    }

    public void Interact()
    {
        if (canBeTriggered == false)
            return;

        if (isFixedGate)
        {
            if (UI.instance != null)
            {
                UI.instance.OpenTeleportUI(true);
            }
        }
        else
        {
            UseTeleport();
        }
    }

    public void SetTrigger(bool trigger)
    {
        canBeTriggered = trigger;
        if (!trigger && interactToolTip != null)
            interactToolTip.SetActive(false);
    }

    public Vector3 GetPosition() => respawnPoint != null ? respawnPoint.position : transform.position;

    private bool InTown() => currentSceneName == townSceneName;

    public void LoadData(GameData data)
    {
        if (isFixedGate)
            return;

        if (InTown() && data.inScenePortals.Count > 0)
        {
            transform.position = defaultPosition;
            isActive = true;
        }
        else if (data.inScenePortals.TryGetValue(currentSceneName, out Vector3 portalPosition))
        {
            transform.position = portalPosition;
            isActive = true;
        }

        returningFromTown = data.returningFromTown;
        returnSceneName = data.portalDestinationSceneName;
    }

    public void SaveData(ref GameData data)
    {
        if (isFixedGate)
            return;

        data.returningFromTown = InTown();

        if (isActive && InTown() == false)
        {
            data.inScenePortals[currentSceneName] = transform.position;
            data.portalDestinationSceneName = currentSceneName;
        }
        else
        {
            data.inScenePortals.Remove(currentSceneName);
        }
    }
}
