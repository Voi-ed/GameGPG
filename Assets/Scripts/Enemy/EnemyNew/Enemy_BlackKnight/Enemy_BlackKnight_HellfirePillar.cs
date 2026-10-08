using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_BlackKnight_HellfirePillar : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.5f;
    [SerializeField] private float moveDelay = 0.2f;
    [SerializeField] private bool snapToGround = true;
    [SerializeField] private float groundCheckDistance = 3.5f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Detection & Damage Settings")]
    [SerializeField] private LayerMask whatIsTarget;
    [SerializeField] private Vector2 colliderSize = new Vector2(1.8f, 4.5f);
    [SerializeField] private Vector2 colliderOffset = new Vector2(0f, 2.2f);
    [SerializeField] private float damageDelay = 0.15f;
    [SerializeField] private float hitInterval = 0.5f;
    [SerializeField] private float lifetime = 2.5f;
    [SerializeField] private float burnDamage = 10f;
    [SerializeField] private float burnDuration = 3f;

    private Enemy_BlackKnight blackKnight;
    private DamageScaleData damageScale;
    private BoxCollider2D col;
    private int moveDir = 1;
    private bool isMoving = false;
    private readonly Dictionary<IDamageable, float> lastHitTimes = new Dictionary<IDamageable, float>();

    public void SetupPillar(Enemy_BlackKnight blackKnight, int direction, DamageScaleData damageScale, LayerMask targetMask, LayerMask groundMask)
    {
        this.blackKnight = blackKnight;
        this.moveDir = direction >= 0 ? 1 : -1;
        this.damageScale = damageScale;
        this.whatIsTarget = targetMask.value != 0 ? targetMask : LayerMask.GetMask("Player");
        this.whatIsGround = groundMask.value != 0 ? groundMask : LayerMask.GetMask("Ground");

        // Orient direction if needed
        if (moveDir < 0)
        {
            Vector3 scale = transform.localScale;
            transform.localScale = new Vector3(-Mathf.Abs(scale.x), scale.y, scale.z);
        }

        col = GetComponent<BoxCollider2D>();
        if (col == null)
            col = gameObject.AddComponent<BoxCollider2D>();

        col.isTrigger = true;
        col.size = colliderSize;
        col.offset = colliderOffset;
        col.enabled = false;

        // Ensure 2D sorting order so particles render properly in front of ground/entities
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            r.sortingLayerName = "Default";
            r.sortingOrder = 25;
        }

        StartCoroutine(ActivationRoutine());
        Destroy(gameObject, lifetime);
    }

    private IEnumerator ActivationRoutine()
    {
        // 1. Initial eruption at point of impact
        yield return new WaitForSeconds(damageDelay);

        if (col != null)
            col.enabled = true;

        // 2. Delay before surging forward across the ground
        if (moveDelay > damageDelay)
            yield return new WaitForSeconds(moveDelay - damageDelay);

        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving)
            return;

        float step = moveSpeed * moveDir * Time.deltaTime;
        Vector3 newPos = transform.position + new Vector3(step, 0f, 0f);

        // Continuously snap to terrain/floor to travel smoothly over slopes and steps
        if (snapToGround && whatIsGround.value != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(newPos + Vector3.up * 1.5f, Vector2.down, groundCheckDistance + 1.5f, whatIsGround);
            if (hit.collider != null)
            {
                newPos.y = hit.point.y;
            }
        }

        transform.position = newPos;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryDamageTarget(collision);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryDamageTarget(collision);
    }

    private void TryDamageTarget(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & whatIsTarget) == 0)
            return;

        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable == null)
            return;

        if (!lastHitTimes.TryGetValue(damageable, out float lastTime) || Time.time >= lastTime + hitInterval)
        {
            lastHitTimes[damageable] = Time.time;

            if (blackKnight != null)
            {
                blackKnight.PerformAttackOnTarget(collision.transform, damageScale);
            }

            // Apply Burn elemental status effect
            Entity_StatusHandler statusHandler = collision.GetComponent<Entity_StatusHandler>();
            if (statusHandler != null)
            {
                statusHandler.ApplyStatusEffect(ElementType.Fire, new ElementalEffectData
                {
                    burnDuratoin = burnDuration,
                    totalBurnDamage = burnDamage
                });
            }
        }
    }
}
