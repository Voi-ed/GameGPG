using UnityEngine;

/// <summary>
/// NPC di chuyển xuyên suốt map/màn hình (ví dụ: thuyền buồm qua lại giữa 2 bờ sông, xe chở khách đi lại 2 đầu bến).
/// Khi đi hết một đầu bản đồ (ra ngoài màn hình) -> dừng nghỉ -> quay đầu -> đi ngược lại sang đầu bên kia!
/// </summary>
public class NpcMovement_FullMap : NpcMovement
{
    public enum TraverseDirection
    {
        AutoDetectByPosition, // Tự động: đặt bên trái camera -> đi sang phải; đặt bên phải camera -> đi sang trái
        LeftToRight,          // Bắt đầu đi từ trái sang phải
        RightToLeft           // Bắt đầu đi từ phải sang trái
    }

    protected TraverseDirection traverseDirection = TraverseDirection.AutoDetectByPosition;
    protected float screenPadding = 2f;

    private float boundaryCheckTimer = 0f;
    private const float BoundaryCheckInterval = 0.1f;

    public virtual void SetFullMapSettings(float padding, TraverseDirection direction = TraverseDirection.AutoDetectByPosition)
    {
        screenPadding = padding;
        traverseDirection = direction;
        InitTraverseDirection();
    }

    protected override void Start()
    {
        base.Start();

        // Khởi tạo hướng xuất phát chuẩn xác theo vị trí đặt NPC
        InitTraverseDirection();

        isWalking = true;
        if (anim != null)
            anim.SetBool(MoveParam, true);
    }

    /// <summary>
    /// Tự động xác định hướng đi ban đầu: nếu đặt NPC ở bên trái camera thì xuất phát đi sang phải
    /// </summary>
    public void InitTraverseDirection()
    {
        if (traverseDirection == TraverseDirection.LeftToRight)
        {
            SetDirection(true);
            return;
        }

        if (traverseDirection == TraverseDirection.RightToLeft)
        {
            SetDirection(false);
            return;
        }

        // AutoDetectByPosition:
        float camCenterX = 0f;
        NpcManager mgr = NpcManager.instance;
        if (mgr != null && (mgr.CameraMaxX - mgr.CameraMinX) > 0.001f)
        {
            camCenterX = (mgr.CameraMinX + mgr.CameraMaxX) * 0.5f;
        }
        else
        {
            Camera cam = Camera.main;
            if (cam != null)
                camCenterX = cam.transform.position.x;
        }

        // Đặt ở bên trái tâm camera -> xuất phát đi sang phải
        bool goRight = transform.position.x <= camCenterX;
        SetDirection(goRight);
    }

    protected override void UpdateMovementLogic()
    {
        if (isWalking)
        {
            boundaryCheckTimer -= Time.deltaTime;
            if (boundaryCheckTimer <= 0f)
            {
                boundaryCheckTimer = BoundaryCheckInterval;

                NpcManager mgr = NpcManager.instance;
                if (mgr != null)
                {
                    float minX = mgr.CameraMinX;
                    float maxX = mgr.CameraMaxX;

                    // Đi sang phải: khi vượt qua mép phải màn hình
                    if (moveRight && transform.position.x > maxX + screenPadding)
                    {
                        OnReachBoundary();
                    }
                    // Đi sang trái: khi vượt qua mép trái màn hình
                    else if (moveLeft && transform.position.x < minX - screenPadding)
                    {
                        OnReachBoundary();
                    }
                }
            }
        }
        else
        {
            waitCounter -= Time.deltaTime;

            if (waitCounter <= 0f)
            {
                isWalking = true;
                moveSpeed = GetRandomSpeed();
                if (anim != null)
                    anim.SetBool(MoveParam, true);
            }
        }
    }

    protected override float GetRandomSpeed()
    {
        if (moveSpeed > 0f)
            return Random.Range(moveSpeed * 0.85f, moveSpeed * 1.15f);

        // Con full map chỉ tầm 0.2 đến 0.4
        return Random.Range(0.2f, 0.4f);
    }

    /// <summary>
    /// Khi ra khỏi một đầu màn hình: Dừng lại ngoài màn hình -> Quay đầu -> Chờ hết giờ -> Đi ngược lại sang đầu bên kia!
    /// </summary>
    private void OnReachBoundary()
    {
        StopWalking();
        moveSpeed = GetRandomSpeed();

        // Quay đầu ngược lại chuẩn bị cho chuyến đi tiếp theo
        bool goOpposite = !moveRight;
        SetDirection(goOpposite);
    }

    protected override void ApplyPhysicsMovement()
    {
        if (rb == null) return;

        float vx = moveRight ? moveSpeed : -moveSpeed;
        rb.linearVelocity = new Vector2(vx, 0);
    }
}
