using UnityEngine;

/// <summary>
/// NPC di chuyển môi trường nước (ví dụ: cá bơi lội, sinh vật biển, lá cây hoặc rong rêu trôi dạt),
/// kết hợp di chuyển ngang và dao động trục Y dạng sóng Sin tạo cảm giác bồng bềnh dập dềnh.
/// </summary>
public class NpcMovement_Aquatic : NpcMovement
{
    protected float waveFrequency = 1.3f;
    protected float waveAmplitude = 0.12f;
    protected float wavePhaseOffset;

    public virtual void SetAquaticSettings(float frequency, float amplitude)
    {
        waveFrequency = frequency;
        waveAmplitude = amplitude;
    }

    protected override void Start()
    {
        base.Start();
        // Random pha lệch sóng để các NPC dưới nước không bị dập dềnh đồng bộ như nhau
        wavePhaseOffset = Random.Range(0f, Mathf.PI * 2f);
        waveFrequency = Random.Range(1.0f, 1.6f);
        waveAmplitude = Random.Range(0.08f, 0.18f);
    }

    protected override float GetRandomSpeed()
    {
        if (moveSpeed > 0f)
            return Random.Range(moveSpeed * 0.85f, moveSpeed * 1.15f);

        // Nước tầm 0.3 đến 0.6
        return Random.Range(0.3f, 0.6f);
    }

    protected override void UpdateMovementLogic()
    {
        if (isWalking)
        {
            walkCounter -= Time.deltaTime;

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

    protected override void ApplyPhysicsMovement()
    {
        if (rb == null) return;

        float vx = moveRight ? moveSpeed : -moveSpeed;

        // Sóng nước dập dềnh trục Y có pha lệch ngẫu nhiên
        float vy = Mathf.Sin((Time.time + wavePhaseOffset) * waveFrequency) * waveAmplitude;

        rb.linearVelocity = new Vector2(vx, vy);
    }
}
