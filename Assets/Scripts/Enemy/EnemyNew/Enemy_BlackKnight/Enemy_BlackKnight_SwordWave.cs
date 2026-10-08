using UnityEngine;

public class Enemy_BlackKnight_SwordWave : MonoBehaviour, ICounterable
{
    [Header("Wave Motion & Detection")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 2.5f;
    [SerializeField] private Vector2 colliderSize = new Vector2(2f, 1.8f);
    [SerializeField] private Vector2 colliderOffset = new Vector2(0f, 0.9f);
    [SerializeField] private LayerMask whatIsObstacle;
    [SerializeField] private bool canBeCountered = true;

    public bool CanBeCountered => canBeCountered;

    private Enemy_BlackKnight blackKnight;
    private DamageScaleData damageScale;
    private LayerMask whatIsTarget;
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private int moveDir = 1;
    private bool hasHit;

    public void SetupWave(Enemy_BlackKnight blackKnight, int direction, DamageScaleData damageScale, LayerMask targetMask, LayerMask obstacleMask)
    {
        this.blackKnight = blackKnight;
        this.damageScale = damageScale;
        this.moveDir = direction >= 0 ? 1 : -1;
        this.whatIsTarget = targetMask.value != 0 ? targetMask : LayerMask.GetMask("Player");
        this.whatIsObstacle = obstacleMask.value != 0 ? obstacleMask : LayerMask.GetMask("Ground");

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = new Vector2(speed * moveDir, 0);

        col = GetComponent<BoxCollider2D>();
        if (col == null)
            col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = colliderSize;
        col.offset = colliderOffset;

        // Face movement direction
        if (moveDir < 0)
        {
            transform.Rotate(0, 180, 0);
        }

        // Ensure 2D sorting order
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            r.sortingLayerName = "Default";
            r.sortingOrder = 25;
        }

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit)
            return;

        // Check Target
        if (((1 << collision.gameObject.layer) & whatIsTarget) != 0)
        {
            hasHit = true;
            if (blackKnight != null)
            {
                blackKnight.PerformAttackOnTarget(collision.transform, damageScale);
            }
            Destroy(gameObject, 0.05f);
            return;
        }

        // Check Obstacles (Walls)
        if (((1 << collision.gameObject.layer) & whatIsObstacle) != 0)
        {
            hasHit = true;
            Destroy(gameObject, 0.05f);
        }
    }

    public void HandleCounter()
    {
        if (!canBeCountered)
            return;

        // Deflect back towards enemy
        moveDir *= -1;
        transform.Rotate(0, 180, 0);
        if (rb != null)
            rb.linearVelocity = new Vector2(speed * moveDir, 0);

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
            whatIsTarget = whatIsTarget | (1 << enemyLayer);
    }
}
