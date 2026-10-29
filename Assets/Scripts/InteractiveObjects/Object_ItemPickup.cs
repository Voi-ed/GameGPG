using UnityEngine;

public class Object_ItemPickup : MonoBehaviour
{

    [SerializeField] private Vector2 dropForce = new Vector2(3, 10);
    [SerializeField] private ItemDataSO itemData;

    [Space]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D col;

    private bool hasLanded = false;
    private float timeSinceSpawn = 0f;

    private void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (col == null) col = GetComponent<Collider2D>();

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            if (rb.mass < 1f) rb.mass = 1f;
        }
    }

    private void OnValidate()
    {
        if (itemData == null)
            return;

        sr = GetComponent<SpriteRenderer>();
        SetupVisuals();
    }

    public void SetupItem(ItemDataSO itemData)
    {
        this.itemData = itemData;
        SetupVisuals();

        // Bỏ qua va chạm giữa các item drop xung quanh để không bị lực nén ép văng xuyên sàn
        if (col != null)
        {
            Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 2f);
            foreach (var other in nearby)
            {
                if (other != null && other != col && other.GetComponent<Object_ItemPickup>() != null)
                {
                    Physics2D.IgnoreCollision(col, other, true);
                }
            }
        }

        hasLanded = false;
        timeSinceSpawn = 0f;
        col.isTrigger = false;
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            float xDropForce = Random.Range(-dropForce.x, dropForce.x);
            rb.linearVelocity = new Vector2(xDropForce, dropForce.y);
        }
    }

    private void SetupVisuals()
    {
        if (sr != null && itemData != null)
            sr.sprite = itemData.itemIcon;
        if (itemData != null)
            gameObject.name = "Object_ItemPickup - " + itemData.itemName;
    }

    private void Update()
    {
        if (hasLanded) return;

        timeSinceSpawn += Time.deltaTime;

        // Cơ chế bảo vệ: Raycast kiểm tra mặt sàn ngay dưới chân khi item đang rơi xuống
        if (timeSinceSpawn > 0.2f && rb != null && rb.linearVelocity.y <= 0.1f)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.45f, LayerMask.GetMask("Ground"));
            if (hit.collider != null)
            {
                LandOnGround(hit.point.y + (col != null ? col.bounds.extents.y : 0.35f));
            }
        }

        // Phòng trường hợp kẹt hoặc rơi quá lâu
        if (timeSinceSpawn > 4f)
        {
            LandOnGround(transform.position.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Bỏ qua va chạm giữa các item pickup
        if (collision.gameObject.GetComponent<Object_ItemPickup>() != null)
        {
            Physics2D.IgnoreCollision(col, collision.collider, true);
            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && !hasLanded)
        {
            // Chỉ tiếp đất và freeze khi tiếp xúc với mặt trên sàn (normal.y > 0.3f)
            // Tránh freeze khi đụng mép dưới hoặc trần sàn đá
            bool landedFromAbove = false;
            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y > 0.3f)
                {
                    landedFromAbove = true;
                    break;
                }
            }

            if (landedFromAbove || (rb != null && rb.linearVelocity.y <= 0.2f))
            {
                LandOnGround(transform.position.y);
            }
        }
    }

    private void LandOnGround(float targetY)
    {
        if (hasLanded) return;
        hasLanded = true;

        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
        if (col != null)
            col.isTrigger = true;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Inventory_Player inventory = collision.GetComponent<Inventory_Player>();

        if (inventory == null)
            return;

        Inventory_Item itemToAdd = new Inventory_Item(itemData);
        Inventory_Storage storage = inventory.storage;

        if (storage == null)
            storage = FindFirstObjectByType<Inventory_Storage>();

        if (itemData.itemType == ItemType.Material)
        {
            if (storage != null)
            {
                storage.AddMaterialToStash(itemToAdd);
            }
            else
            {
                // Khi ở BattleScene hoặc map quái không có rương Storage, tự động cất thẳng vào SaveManager
                if (SaveManager.instance != null && SaveManager.instance.GetGameData() != null)
                {
                    var materialsDict = SaveManager.instance.GetGameData().storageMaterials;
                    if (materialsDict != null)
                    {
                        if (materialsDict.TryGetValue(itemData.saveId, out int currentCount))
                            materialsDict[itemData.saveId] = currentCount + 1;
                        else
                            materialsDict[itemData.saveId] = 1;
                    }
                }
                else if (inventory.CanAddItem(itemToAdd))
                {
                    inventory.AddItem(itemToAdd);
                }
            }

            if (AudioManager.instance != null)
                AudioManager.instance.PlayGlobalSFX("item_pickup");

            Destroy(gameObject);
            return;
        }

        if (inventory.CanAddItem(itemToAdd))
        {
            inventory.AddItem(itemToAdd);

            if (AudioManager.instance != null)
                AudioManager.instance.PlayGlobalSFX("item_pickup");

            Destroy(gameObject);
        }
    }
}

