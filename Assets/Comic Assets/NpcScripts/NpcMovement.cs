using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public abstract class NpcMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] protected float moveSpeed = 0.4f;
    [SerializeField] protected float walkTime = 3f;
    [SerializeField] protected float waitTime = 2f;
    protected bool hasCustomStats = false;

    protected bool isWalking;
    protected bool facingLeft;
    protected float walkCounter;
    protected float waitCounter;

    protected bool moveRight = false;
    protected bool moveLeft = true;

    protected Rigidbody2D rb;
    protected Animator anim;

    protected static readonly int MoveParam = Animator.StringToHash("Move");

    protected virtual void Reset()
    {
        LoadComponents();
    }

    protected virtual void Awake()
    {
        LoadComponents();
    }

    protected virtual void LoadComponents()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.sleepMode = RigidbodySleepMode2D.StartAwake;
        }

        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }

        if (anim != null)
        {
            // Tối ưu CPU: Tạm dừng cập nhật Animator khi NPC nằm ngoài màn hình
            anim.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        }
    }

    protected virtual void OnEnable()
    {
        if (NpcManager.instance != null)
        {
            NpcManager.instance.RegisterNpc(this);
        }
    }

    protected virtual void OnDisable()
    {
        if (NpcManager.instance != null)
        {
            NpcManager.instance.UnregisterNpc(this);
        }
    }

    protected virtual void Start()
    {
        moveSpeed = GetRandomSpeed();

        // Khử hoàn toàn đồng bộ lúc bắt đầu
        isWalking = Random.value > 0.5f;
        if (isWalking)
        {
            walkCounter = Random.Range(0.5f, GetRandomWalkTime());
            waitCounter = 0f;
            if (anim != null) anim.SetBool(MoveParam, true);
        }
        else
        {
            waitCounter = Random.Range(0.5f, GetRandomWaitTime());
            walkCounter = 0f;
            if (anim != null) anim.SetBool(MoveParam, false);
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        // Random hướng xuất phát ban đầu
        bool startRight = Random.value > 0.5f;
        SetDirection(startRight);
    }

    /// <summary>
    /// Chuyển sang trạng thái dừng đợi một cách tối ưu (chỉ set vận tốc 0 đúng 1 lần duy nhất để Rigidbody đi ngủ)
    /// </summary>
    public virtual void StopWalking()
    {
        isWalking = false;
        waitCounter = GetRandomWaitTime();
        if (anim != null) anim.SetBool(MoveParam, false);
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Bắt đầu chu kỳ đi bộ mới
    /// </summary>
    public virtual void StartWalking()
    {
        isWalking = true;
        walkCounter = GetRandomWalkTime();
        moveSpeed = GetRandomSpeed();
        if (anim != null) anim.SetBool(MoveParam, true);
    }

    /// <summary>
    /// Hàm dùng để set các giá trị di chuyển qua code
    /// </summary>
    public virtual void SetMovementStats(float moveSpeed, float walkTime, float waitTime)
    {
        this.moveSpeed = moveSpeed;
        this.walkTime = walkTime;
        this.waitTime = waitTime;
        this.hasCustomStats = true;

        if (walkCounter <= 0f) walkCounter = GetRandomWalkTime();
        if (waitCounter <= 0f) waitCounter = GetRandomWaitTime();
    }

    /// <summary>
    /// Random tốc độ: dao động nhẹ quanh giá trị Inspector hoặc code gán
    /// </summary>
    protected virtual float GetRandomSpeed()
    {
        if (moveSpeed > 0f)
            return Random.Range(moveSpeed * 0.85f, moveSpeed * 1.15f);

        return Random.Range(0.3f, 0.6f);
    }

    /// <summary>
    /// Random thời gian đi bộ: dao động quanh giá trị walkTime
    /// </summary>
    protected virtual float GetRandomWalkTime()
    {
        if (walkTime > 0f)
            return Random.Range(walkTime * 0.7f, walkTime * 1.3f);

        return Random.Range(2f, 4f);
    }

    /// <summary>
    /// Random thời gian dừng chờ: dao động quanh giá trị waitTime
    /// </summary>
    protected virtual float GetRandomWaitTime()
    {
        if (waitTime > 0f)
            return Random.Range(waitTime * 0.7f, waitTime * 1.3f);

        return Random.Range(1f, 3f);
    }

    protected virtual void Update()
    {
        UpdateMovementLogic();
    }

    protected virtual void FixedUpdate()
    {
        // Tối ưu vật lý: CHỈ chạy tính toán khi NPC đang đi!
        // Khi đứng yên (isWalking == false), hoàn toàn KHÔNG can thiệp để Rigidbody2D ở chế độ Sleep
        if (isWalking)
        {
            ApplyPhysicsMovement();
        }
    }

    protected abstract void UpdateMovementLogic();
    protected abstract void ApplyPhysicsMovement();

    /// <summary>
    /// Chọn hướng ngẫu nhiên (trái hoặc phải), random lại tốc độ và thời gian
    /// </summary>
    public virtual void ChooseDirection()
    {
        bool goRight = Random.value > 0.5f;
        SetDirection(goRight);
        StartWalking();
    }

    public virtual void SetDirection(bool toRight)
    {
        moveRight = toRight;
        moveLeft = !toRight;

        // Đồng bộ hướng mặt (Flip) chính xác theo trục X tuyệt đối
        Vector3 scale = transform.localScale;
        scale.x = toRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
        facingLeft = !toRight;
    }

    public virtual void MoveRight()
    {
        SetDirection(true);
        if (anim != null)
            anim.SetBool(MoveParam, true);
    }

    public virtual void MoveLeft()
    {
        SetDirection(false);
        if (anim != null)
            anim.SetBool(MoveParam, true);
    }

    public virtual void Flip()
    {
        SetDirection(!moveRight);
    }
}
