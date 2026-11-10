using UnityEngine;

public class Object_ItemPickup : MonoBehaviour
{
    [SerializeField] private Vector2 dropForce = new Vector2(2f, 4.5f);
    [SerializeField] private ItemDataSO itemData;

    [Space]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D col;

    [Header("Magnet / Auto Pickup Settings")]
    [SerializeField] private float magnetRadius = 3.5f;
    [SerializeField] private float pickupDelay = 0.35f;
    [SerializeField] private float initialFlySpeed = 6f;
    [SerializeField] private float maxFlySpeed = 24f;
    [SerializeField] private float flyAcceleration = 35f;
    [SerializeField] private float collectDistance = 0.4f;

    private bool hasLanded = false;
    private float timeSinceSpawn = 0f;
    private float timeSinceLanded = 0f;
    private bool isBeingAttracted = false;
    private float currentFlySpeed = 0f;
    private Vector3 initialScale = Vector3.one;
    private float fullCooldownTimer = 0f;

    private void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (col == null) col = GetComponent<Collider2D>();

        initialScale = transform.localScale;

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

        // 1. Bỏ qua va chạm vật lý với Player để không bao giờ xô đẩy hay cản chân nhân vật
        IgnorePlayerCollision();

        // 2. Bỏ qua va chạm giữa các item drop xung quanh để không bị lực nén ép văng xuyên sàn
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
        isBeingAttracted = false;
        currentFlySpeed = 0f;
        timeSinceSpawn = 0f;
        timeSinceLanded = 0f;
        transform.localScale = initialScale;

        if (col != null)
            col.isTrigger = false;

        if (rb != null)
        {
            rb.gravityScale = 1f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Tinh chỉnh lực đẩy: Nếu Prefab/Scene cũ đang lưu y = 10 thì tự động clamp về ~4.5f để nảy nhẹ vừa mắt
            float yForce = (dropForce.y >= 7f) ? 4.5f : dropForce.y;
            float xRange = Mathf.Min(dropForce.x, 2.2f);
            float xDropForce = Random.Range(-xRange, xRange);

            rb.linearVelocity = new Vector2(xDropForce, yForce);
        }
    }

    private void IgnorePlayerCollision()
    {
        if (col == null) return;

        Player player = Player.instance != null ? Player.instance : FindFirstObjectByType<Player>();
        if (player != null)
        {
            Collider2D[] playerColliders = player.GetComponentsInChildren<Collider2D>(true);
            foreach (var pCol in playerColliders)
            {
                if (pCol != null && pCol != col)
                {
                    Physics2D.IgnoreCollision(col, pCol, true);
                }
            }
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
        timeSinceSpawn += Time.deltaTime;

        if (fullCooldownTimer > 0f)
            fullCooldownTimer -= Time.deltaTime;

        if (hasLanded)
        {
            timeSinceLanded += Time.deltaTime;
        }

        // Cơ chế tiếp đất tự nhiên khi chưa tiếp đất và chưa bị hút
        if (!hasLanded && !isBeingAttracted)
        {
            // Raycast kiểm tra mặt sàn ngay dưới chân khi item đang rơi xuống
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

        // Xử lý cơ chế nam châm hút về phía Player (chỉ khi đã rơi xuống đất xong)
        HandleMagnet();
    }

    private void HandleMagnet()
    {
        // ❗ BẮT BUỘC: Item PHẢI rơi chạm đất xong mới bắt đầu hút vào người
        if (!hasLanded)
            return;

        // Chờ thêm một nhịp rất nhỏ sau khi chạm đất (0.15s) để người chơi thấy item nằm yên trên sàn
        if (timeSinceLanded < 0.15f)
            return;

        if (fullCooldownTimer > 0f)
            return;

        Player player = Player.instance != null ? Player.instance : FindFirstObjectByType<Player>();
        if (player == null || player.health == null || player.health.isDead)
            return;

        // ❗ KIỂM TRA SỨC CHỨA TÚI ĐỒ: Nếu túi đồ đang đầy (không thể chứa thêm item này), TUYỆT ĐỐI KHÔNG HÚT!
        if (!CanPlayerPickUp(player))
        {
            if (isBeingAttracted)
            {
                OnInventoryFull(player);
            }
            return;
        }

        // Tâm điểm hút: Hướng về trọng tâm Player hoặc Rồng nếu đang hóa rồng
        Vector3 targetPos;
        if (GameManager.instance != null && GameManager.instance.IsDragonTransformed && GameManager.instance.ActiveDragon != null)
        {
            targetPos = GameManager.instance.ActiveDragon.transform.position;
        }
        else
        {
            targetPos = player.transform.position + Vector3.up * 0.6f;
        }

        float distance = Vector2.Distance(transform.position, targetPos);

        // Phát hiện Player trong bán kính hút
        if (!isBeingAttracted)
        {
            if (distance <= magnetRadius)
            {
                isBeingAttracted = true;
                currentFlySpeed = initialFlySpeed;

                // Tắt vật lý rơi để bắt đầu quỹ đạo bay mượt
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.gravityScale = 0f;
                    rb.constraints = RigidbodyConstraints2D.FreezeAll;
                }
                if (col != null)
                {
                    col.isTrigger = true;
                }
            }
        }

        // Đang trong trạng thái hút bay về Player
        if (isBeingAttracted)
        {
            // Gia tốc bay tăng dần theo thời gian (Ease-in)
            currentFlySpeed = Mathf.MoveTowards(currentFlySpeed, maxFlySpeed, flyAcceleration * Time.deltaTime);

            // Bay mượt theo Player
            transform.position = Vector3.MoveTowards(transform.position, targetPos, currentFlySpeed * Time.deltaTime);

            // Thu nhỏ kích thước nhẹ nhàng khi lại gần ngực Player (< 0.8m)
            if (distance < 0.8f)
            {
                float t = Mathf.Clamp01(distance / 0.8f);
                transform.localScale = Vector3.Lerp(initialScale * 0.25f, initialScale, t);
            }
            else
            {
                transform.localScale = initialScale;
            }

            // Khi đã tiếp cận sát Player -> Gom vào túi đồ
            if (distance <= collectDistance)
            {
                Inventory_Player inventory = player.inventory != null ? player.inventory : player.GetComponent<Inventory_Player>();
                if (inventory != null)
                {
                    TryCollectItem(inventory);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Luôn bỏ qua va chạm với Player nếu phát hiện
        if (collision.gameObject.GetComponentInParent<Player>() != null)
        {
            Physics2D.IgnoreCollision(col, collision.collider, true);
            return;
        }

        // Bỏ qua va chạm giữa các item pickup
        if (collision.gameObject.GetComponent<Object_ItemPickup>() != null)
        {
            Physics2D.IgnoreCollision(col, collision.collider, true);
            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && !hasLanded)
        {
            // Chỉ tiếp đất và freeze khi tiếp xúc với mặt trên sàn (normal.y > 0.3f)
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
        timeSinceLanded = 0f;

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
        // ❗ BẮT BUỘC: Item PHẢI rơi chạm đất xong mới cho nhặt
        if (!hasLanded)
            return;

        Inventory_Player inventory = collision.GetComponent<Inventory_Player>();
        if (inventory == null)
            inventory = collision.GetComponentInParent<Inventory_Player>();

        if (inventory != null)
        {
            TryCollectItem(inventory);
        }
    }

    public bool TryCollectItem(Inventory_Player inventory)
    {
        if (inventory == null || itemData == null)
            return false;

        Inventory_Item itemToAdd = new Inventory_Item(itemData);
        Inventory_Storage storage = inventory.storage;

        if (storage == null)
            storage = FindFirstObjectByType<Inventory_Storage>();

        // Vật phẩm nguyên liệu (Material): Ưu tiên cất vào Storage / SaveManager
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
                else
                {
                    OnInventoryFull(inventory.GetComponent<Player>());
                    return false;
                }
            }

            if (AudioManager.instance != null)
                AudioManager.instance.PlayGlobalSFX("item_pickup");

            Destroy(gameObject);
            return true;
        }

        // Trang bị & Đồ dùng khác: Kiểm tra sức chứa túi đồ
        if (inventory.CanAddItem(itemToAdd))
        {
            inventory.AddItem(itemToAdd);

            if (AudioManager.instance != null)
                AudioManager.instance.PlayGlobalSFX("item_pickup");

            Destroy(gameObject);
            return true;
        }
        else
        {
            OnInventoryFull(inventory.GetComponent<Player>());
            return false;
        }
    }

    /// <summary>
    /// Kiểm tra người chơi có thể nhặt vật phẩm này vào hành trang hay không
    /// </summary>
    private bool CanPlayerPickUp(Player player)
    {
        if (player == null || itemData == null)
            return false;

        // Vật phẩm nguyên liệu (Material): Tự động gom vào hòm Storage / SaveManager nên luôn nhặt được
        if (itemData.itemType == ItemType.Material)
            return true;

        Inventory_Player inventory = player.inventory != null ? player.inventory : player.GetComponent<Inventory_Player>();
        if (inventory == null)
            return false;

        Inventory_Item itemToAdd = new Inventory_Item(itemData);
        return inventory.CanAddItem(itemToAdd);
    }

    private void OnInventoryFull(Player player = null)
    {
        // Khi túi đầy, khôi phục lại scale ban đầu, dừng hút và cho rơi lại sàn đất
        transform.localScale = initialScale;
        isBeingAttracted = false;
        fullCooldownTimer = 1.5f; // Chờ 1.5s trước khi kiểm tra hút lại

        if (col != null)
        {
            col.isTrigger = false;
        }

        if (rb != null)
        {
            rb.gravityScale = 1f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Đẩy nhẹ vật phẩm ra xa khỏi người chơi một chút và rơi xuống sàn
            float pushDir = (player != null && transform.position.x < player.transform.position.x) ? -1.2f : 1.2f;
            rb.linearVelocity = new Vector2(pushDir, 2f);
        }

        hasLanded = false;
        timeSinceLanded = 0f;
    }
}
