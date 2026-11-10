using UnityEngine;

/// <summary>
/// NPC chỉ di chuyển quanh vị trí ban đầu (ví dụ: người đứng ban công, quét sân, đứng gác cổng).
/// Tối ưu hiệu năng: Sleep Rigidbody khi đứng yên, Gizmos trực quan.
/// </summary>
public class NpcMovement_Static : NpcMovement
{
    [Header("Static Patrol Settings")]
    [Tooltip("Bán kính tối đa NPC được phép đi quanh điểm gốc (mỗi bên). Ví dụ 0.8m = chỉ đi quanh quẩn 1 khoảng nhỏ")]
    [SerializeField] protected float maxPatrolDistance = 0.8f;

    protected Vector2 startPosition;

    public virtual void SetPatrolDistance(float distance)
    {
        maxPatrolDistance = distance;
    }

    protected override void Awake()
    {
        base.Awake();
        // Lưu chính xác tọa độ gốc ban đầu
        startPosition = transform.position;
    }

    protected override float GetRandomSpeed()
    {
        if (moveSpeed > 0f)
            return Random.Range(moveSpeed * 0.85f, moveSpeed * 1.15f);

        // Static đi bước chậm, nhỏ: 0.25 - 0.45
        return Random.Range(0.25f, 0.45f);
    }

    /// <summary>
    /// Bước đi ngắn (1s - 2.2s) vì cự ly ban công nhỏ
    /// </summary>
    protected override float GetRandomWalkTime()
    {
        return Random.Range(1.0f, 2.2f);
    }

    /// <summary>
    /// Đứng quan sát/ngắm cảnh lâu hơn một chút (2s - 4s)
    /// </summary>
    protected override float GetRandomWaitTime()
    {
        return Random.Range(2.0f, 4.0f);
    }

    protected override void UpdateMovementLogic()
    {
        if (isWalking)
        {
            walkCounter -= Time.deltaTime;

            float posX = transform.position.x;
            bool reachedRightLimit = moveRight && (posX >= startPosition.x + maxPatrolDistance);
            bool reachedLeftLimit = moveLeft && (posX <= startPosition.x - maxPatrolDistance);

            if (walkCounter <= 0f || reachedRightLimit || reachedLeftLimit)
            {
                // Kẹp chặt vị trí không cho văng ra ngoài mép ban công
                if (reachedRightLimit)
                {
                    Vector3 pos = transform.position;
                    pos.x = startPosition.x + maxPatrolDistance;
                    transform.position = pos;
                }
                else if (reachedLeftLimit)
                {
                    Vector3 pos = transform.position;
                    pos.x = startPosition.x - maxPatrolDistance;
                    transform.position = pos;
                }

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
    /// Chọn hướng thông minh: luôn có xu hướng đi ngược trở về điểm gốc startPosition
    /// </summary>
    public override void ChooseDirection()
    {
        float offsetFromCenter = transform.position.x - startPosition.x;

        // Nếu đang ở sát mép phải -> 100% bắt buộc đi trái
        if (offsetFromCenter >= maxPatrolDistance * 0.7f)
        {
            SetDirection(false);
        }
        // Nếu đang ở sát mép trái -> 100% bắt buộc đi phải
        else if (offsetFromCenter <= -maxPatrolDistance * 0.7f)
        {
            SetDirection(true);
        }
        // Nếu đang lệch nhẹ sang phải -> ưu tiên 75% quay đầu về trái
        else if (offsetFromCenter > 0.1f)
        {
            bool goLeft = Random.value < 0.75f;
            SetDirection(!goLeft);
        }
        // Nếu đang lệch nhẹ sang trái -> ưu tiên 75% quay đầu về phải
        else if (offsetFromCenter < -0.1f)
        {
            bool goRight = Random.value < 0.75f;
            SetDirection(goRight);
        }
        // Đang ở ngay tâm gốc -> 50/50 đi trái hoặc phải
        else
        {
            SetDirection(Random.value > 0.5f);
        }

        StartWalking();
    }

    protected override void ApplyPhysicsMovement()
    {
        if (rb == null) return;

        float vx = moveRight ? moveSpeed : -moveSpeed;
        rb.linearVelocity = new Vector2(vx, 0);
    }

    /// <summary>
    /// Hiển thị vạch tuần tra màu xanh lá trên Scene View để sếp căn chỉnh cự ly ban công
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying ? (Vector3)startPosition : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin + Vector3.left * maxPatrolDistance, origin + Vector3.right * maxPatrolDistance);
        Gizmos.DrawWireSphere(origin + Vector3.left * maxPatrolDistance, 0.08f);
        Gizmos.DrawWireSphere(origin + Vector3.right * maxPatrolDistance, 0.08f);
    }
}
