using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC di chuyển tuần tra trong map (tốc độ 0.8 - 1.5).
/// Tối ưu hóa hiệu năng cao: Zero GC Allocation, Cache O(1) biên Camera & Confiner.
/// </summary>
public class NpcMovement_Roam : NpcMovement
{
    [Header("Skin Swap (Animator List)")]
    [Tooltip("Danh sách các Animator Controller để đổi sang con khác khi đi ra ngoài màn hình")]
    [SerializeField] protected List<RuntimeAnimatorController> npcAnimators = new List<RuntimeAnimatorController>();

    protected float outOfCameraPadding = 1.5f; // Khoảng cách ngoài rìa camera để kích hoạt đổi skin
    protected float confinerPadding = 0.5f;     // Không cho đi vượt quá Confiner quá khoảng này

    // Tối ưu CPU: Chỉ kiểm tra biên mỗi 0.1s thay vì 60 lần/giây
    private float boundaryCheckTimer = 0f;
    private const float BoundaryCheckInterval = 0.1f;

    public virtual void SetNpcAnimators(List<RuntimeAnimatorController> animators)
    {
        npcAnimators = animators;
    }

    protected override void Start()
    {
        base.Start();

        // Ngay khi bắt đầu game: random ngẫu nhiên 1 Animator từ danh sách
        RandomizeInitialAnimator();
    }

    private void RandomizeInitialAnimator()
    {
        if (npcAnimators != null && npcAnimators.Count > 0 && anim != null)
        {
            int randomIndex = Random.Range(0, npcAnimators.Count);
            if (npcAnimators[randomIndex] != null)
            {
                anim.runtimeAnimatorController = npcAnimators[randomIndex];
                anim.SetBool(MoveParam, isWalking);
            }
        }
    }

    protected override float GetRandomSpeed()
    {
        if (moveSpeed > 0f)
            return Random.Range(moveSpeed * 0.85f, moveSpeed * 1.15f);

        // Roam từ 0.8 đến 1.5
        return Random.Range(0.8f, 1.5f);
    }

    protected override void UpdateMovementLogic()
    {
        if (isWalking)
        {
            walkCounter -= Time.deltaTime;

            // Tối ưu CPU: Giảm tần suất kiểm tra biên (10 lần/s là mắt thường không phân biệt được)
            boundaryCheckTimer -= Time.deltaTime;
            if (boundaryCheckTimer <= 0f)
            {
                boundaryCheckTimer = BoundaryCheckInterval;
                if (CheckAndHandleBoundaryFast())
                {
                    return;
                }
            }

            if (walkCounter <= 0f)
            {
                StopWalking();
            }
        }
        else
        {
            waitCounter -= Time.deltaTime;

            if (waitCounter <= 0f)
            {
                ChooseDirection();
            }
        }
    }

    /// <summary>
    /// Kiểm tra biên cực nhanh qua biến cache O(1) của NpcManager (Zero GameObject.Find, Zero Camera.main)
    /// </summary>
    private bool CheckAndHandleBoundaryFast()
    {
        NpcManager mgr = NpcManager.instance;
        if (mgr == null) return false;

        float camMinX = mgr.CameraMinX;
        float camMaxX = mgr.CameraMaxX;
        float confMinX = mgr.ConfinerMinX;
        float confMaxX = mgr.ConfinerMaxX;
        bool hasConfiner = mgr.HasConfiner;

        float posX = transform.position.x;

        // Đi sang trái: ra ngoài camera bên trái
        if (moveLeft)
        {
            bool isOutsideCam = posX < (camMinX - outOfCameraPadding);
            bool isNearConfinerEdge = hasConfiner && (posX <= confMinX - confinerPadding);

            if (isOutsideCam || isNearConfinerEdge)
            {
                OnReachOutside(isLeftSide: true);
                return true;
            }
        }
        // Đi sang phải: ra khỏi camera bên phải
        else if (moveRight)
        {
            bool isOutsideCam = posX > (camMaxX + outOfCameraPadding);
            bool isNearConfinerEdge = hasConfiner && (posX >= confMaxX + confinerPadding);

            if (isOutsideCam || isNearConfinerEdge)
            {
                OnReachOutside(isLeftSide: false);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Xử lý khi NPC đã ra khỏi màn hình: Đổi Animator sang con khác, quay đầu hướng vào trong rồi đi vào
    /// </summary>
    private void OnReachOutside(bool isLeftSide)
    {
        StopWalking();

        // 1. Đổi sang Animator khác trong List (Zero GC Alloc)
        SwapRandomAnimatorFast();

        // 2. Quay đầu hướng vào lại trong màn hình:
        // Đang ở bên trái -> quay mặt sang phải để đi vào
        // Đang ở bên phải -> quay mặt sang trái để đi vào
        SetDirection(isLeftSide);
    }

    /// <summary>
    /// Đổi sang Animator khác với Zero GC Allocation (không new List, không tạo rác Heap)
    /// </summary>
    private void SwapRandomAnimatorFast()
    {
        if (npcAnimators == null || npcAnimators.Count == 0 || anim == null) return;

        int count = npcAnimators.Count;
        if (count == 1)
        {
            anim.runtimeAnimatorController = npcAnimators[0];
            return;
        }

        // Toán học chọn chỉ mục khác controller hiện tại mà KHÔNG tốn 1 byte cấp phát bộ nhớ
        RuntimeAnimatorController current = anim.runtimeAnimatorController;
        int currentIndex = npcAnimators.IndexOf(current);

        int offset = Random.Range(1, count);
        int nextIndex = (currentIndex >= 0) ? (currentIndex + offset) % count : Random.Range(0, count);

        if (npcAnimators[nextIndex] != null)
        {
            anim.runtimeAnimatorController = npcAnimators[nextIndex];
            anim.SetBool(MoveParam, isWalking);
        }
    }

    protected override void ApplyPhysicsMovement()
    {
        if (rb == null) return;

        float vx = moveRight ? moveSpeed : -moveSpeed;
        rb.linearVelocity = new Vector2(vx, 0);
    }
}
