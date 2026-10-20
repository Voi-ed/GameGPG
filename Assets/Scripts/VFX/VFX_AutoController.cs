using System.Collections;
using UnityEngine;

public class VFX_AutoController : MonoBehaviour
{
    private SpriteRenderer sr;

    [SerializeField] private bool autoDestroy = true;
    [SerializeField] private float destroyDelay = 1;
    [Space]
    [SerializeField] private bool randomOffset = true;
    [SerializeField] private bool randomRotation = true;
    [Header("Fade effect")]
    [SerializeField] private bool canFade;
    [SerializeField] private float fadeSpeed = 1;


    [Header("Random rotation")]
    [SerializeField] private float minRotation = 0;
    [SerializeField] private float maxRotation = 360;

    [Header("Random position")]
    [SerializeField] private float xMinOffset = -.3f;
    [SerializeField] private float xMaxOffset = .3f;
    [Space]
    [SerializeField] private float yMinOffset = -.3f;
    [SerializeField] private float yMaxOffset = .3f;

    private Color originalColor = Color.white;
    private Animator[] animators;
    private ParticleSystem[] particleSystems;
    private TrailRenderer[] trailRenderers;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;

        animators = GetComponentsInChildren<Animator>(true);
        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        trailRenderers = GetComponentsInChildren<TrailRenderer>(true);
    }

    private void OnEnable()
    {
        ResetVisuals();

        ApplyRandomOffset();
        ApplyRandomRotation();

        if (canFade)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeCo());
        }

        if (autoDestroy)
        {
            if (GameManager.instance != null && GameManager.instance.vfxPool != null)
            {
                GameManager.instance.vfxPool.ReturnToPool(gameObject, destroyDelay);
            }
            else
            {
                Destroy(gameObject, destroyDelay);
            }
        }
    }

    private void OnDisable()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }

    private void ResetVisuals()
    {
        // 1. Phục hồi alpha & màu sắc SpriteRenderer
        if (sr != null)
        {
            sr.color = originalColor;
        }

        // 2. Tua lại Animation về thời điểm bắt đầu
        if (animators != null)
        {
            for (int i = 0; i < animators.Length; i++)
            {
                if (animators[i] != null && animators[i].runtimeAnimatorController != null)
                {
                    animators[i].Play(0, -1, 0f);
                }
            }
        }

        // 3. Xóa các hạt cũ và phát lại ParticleSystem
        if (particleSystems != null)
        {
            for (int i = 0; i < particleSystems.Length; i++)
            {
                if (particleSystems[i] != null)
                {
                    particleSystems[i].Clear();
                    particleSystems[i].Play();
                }
            }
        }

        // 4. Xóa vệt kéo TrailRenderer
        if (trailRenderers != null)
        {
            for (int i = 0; i < trailRenderers.Length; i++)
            {
                if (trailRenderers[i] != null)
                {
                    trailRenderers[i].Clear();
                }
            }
        }
    }

    private IEnumerator FadeCo()
    {
        Color targetColor = sr != null ? sr.color : originalColor;

        while (targetColor.a > 0)
        {
            targetColor.a = targetColor.a - (fadeSpeed * Time.deltaTime);
            if (sr != null)
                sr.color = targetColor;
            yield return null;
        }

        if (sr != null)
            sr.color = targetColor;
    }

    private void ApplyRandomOffset()
    {
        if (randomOffset == false)
            return;

        float xOffset = Random.Range(xMinOffset, xMaxOffset);
        float yOffset = Random.Range(yMinOffset, yMaxOffset);

        transform.position = transform.position + new Vector3(xOffset, yOffset);
    }

    private void ApplyRandomRotation()
    {
        if(randomRotation == false)
            return;

        float zRotation = Random.Range(minRotation, maxRotation);
        transform.Rotate(0, 0, zRotation);
    }
}
