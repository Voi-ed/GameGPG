using System.Collections.Generic;
using UnityEngine;

public class Satyr_FireBreathEmitter : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private Vector2 hitboxSize = new Vector2(4.5f, 2.0f);
    [SerializeField] private Vector2 hitboxOffset = new Vector2(2.5f, 0f);
    [SerializeField] private float hitInterval = 0.25f;
    [SerializeField] private LayerMask whatIsTarget;

    private Enemy_Satyr satyr;
    private DamageScaleData damageScale;
    private GameObject activeVfxInstance;
    private ParticleSystem[] cachedParticles;
    private BoxCollider2D col;
    private bool isEmitting = false;
    private readonly Dictionary<IDamageable, float> lastHitTimes = new Dictionary<IDamageable, float>();

    public void Setup(Enemy_Satyr owner, DamageScaleData damageScale, LayerMask targetMask)
    {
        this.satyr = owner;
        this.damageScale = damageScale;
        this.whatIsTarget = targetMask.value != 0 ? targetMask : LayerMask.GetMask("Player");

        col = GetComponent<BoxCollider2D>();
        if (col == null)
            col = gameObject.AddComponent<BoxCollider2D>();

        col.isTrigger = true;
        col.size = hitboxSize;
        col.offset = hitboxOffset;
        col.enabled = false;
    }

    public void StartBreath(GameObject vfxPrefab)
    {
        isEmitting = true;
        lastHitTimes.Clear();

        if (col != null)
            col.enabled = true;

        // Tạo VFX một lần duy nhất, các lần sau chỉ Play lại (tránh Instantiate/Destroy sinh GC)
        if (activeVfxInstance == null && vfxPrefab != null)
        {
            activeVfxInstance = Instantiate(vfxPrefab, transform.position, transform.rotation, transform);
            activeVfxInstance.transform.localPosition = Vector3.zero;
            activeVfxInstance.transform.localRotation = Quaternion.identity;

            // Ensure particle renderers render properly
            Renderer[] renderers = activeVfxInstance.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.sortingLayerName = "Default";
                r.sortingOrder = 30;
            }

            cachedParticles = activeVfxInstance.GetComponentsInChildren<ParticleSystem>(true);
        }

        if (activeVfxInstance != null)
        {
            activeVfxInstance.SetActive(true);
            foreach (var ps in cachedParticles)
                ps.Play(false);
        }
    }

    public void StopBreath()
    {
        isEmitting = false;

        if (col != null)
            col.enabled = false;

        // Ngừng phát hạt mới, để hạt cũ tắt dần tự nhiên
        if (cachedParticles != null)
        {
            foreach (var ps in cachedParticles)
                ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    public void ForceKillBreath()
    {
        isEmitting = false;

        if (col != null)
            col.enabled = false;

        // Xóa sạch hạt ngay (vd: trước khi Dash) để lửa không bám theo boss
        if (cachedParticles != null)
        {
            foreach (var ps in cachedParticles)
                ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isEmitting || ((1 << other.gameObject.layer) & whatIsTarget.value) == 0)
            return;

        if (!other.TryGetComponent<IDamageable>(out var damageable))
            return;

        if (!lastHitTimes.TryGetValue(damageable, out float lastTime) || Time.time >= lastTime + hitInterval)
        {
            lastHitTimes[damageable] = Time.time;

            if (satyr != null && satyr.combat != null)
            {
                satyr.PerformAttackOnTarget(other.transform, damageScale);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.4f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(hitboxOffset, hitboxSize);
    }
}
