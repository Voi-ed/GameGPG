using System.Collections;
using TMPro;
using UnityEngine;

public class DamageTextController : MonoBehaviour
{
    private TextMeshPro textMesh;
    private MeshRenderer meshRenderer;
    private Coroutine motionCoroutine;

    [Header("Sorting")]
    [SerializeField] private string sortingLayerName = "Vfx_Front";
    [SerializeField] private int sortingOrder = 100;

    [Header("Colors")]
    [SerializeField] private Color normalDamageColor = Color.white;
    [SerializeField] private Color critDamageColor = new Color(1f, 0.28f, 0.1f);      // Cam đỏ rực rỡ
    [SerializeField] private Color fireColor = new Color(1f, 0.42f, 0.15f);            // Lửa
    [SerializeField] private Color iceColor = new Color(0.1f, 0.9f, 1f);               // Băng cyan
    [SerializeField] private Color lightningColor = new Color(1f, 0.92f, 0.25f);       // Sét vàng
    [SerializeField] private Color missColor = new Color(0.75f, 0.82f, 0.88f);         // Miss xám bạc

    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.75f;
    [SerializeField] private float floatDistance = 0.85f;
    [SerializeField] private float horizontalSpread = 0.25f;
    [Range(0.2f, 6f)] [SerializeField] private float baseFontSize = 0.8f;
    [Range(0.2f, 8f)] [SerializeField] private float critFontSize = 1.2f;

    [Header("Optional Font Asset Override")]
    [SerializeField] private TMP_FontAsset customFont;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();

        if (textMesh == null)
            textMesh = gameObject.AddComponent<TextMeshPro>();

        if (customFont != null)
            textMesh.font = customFont;

        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            meshRenderer.sortingLayerName = sortingLayerName;
            meshRenderer.sortingOrder = sortingOrder;
        }

        textMesh.isOrthographic = true;
        textMesh.alignment = TextAlignmentOptions.Center;
    }

    public void Setup(float damage, bool isCrit, ElementType element, int hitIndex = 0)
    {
        InitializeComponents();

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (motionCoroutine != null)
            StopCoroutine(motionCoroutine);

        int roundedDmg = Mathf.RoundToInt(damage);
        Color targetColor = normalDamageColor;

        if (isCrit)
        {
            textMesh.text = $"CRIT! {roundedDmg}";
            textMesh.fontSize = critFontSize;
            targetColor = critDamageColor;
        }
        else
        {
            textMesh.text = roundedDmg.ToString();
            textMesh.fontSize = baseFontSize;

            switch (element)
            {
                case ElementType.Fire:
                    targetColor = fireColor;
                    break;
                case ElementType.Ice:
                    targetColor = iceColor;
                    break;
                case ElementType.Lightning:
                    targetColor = lightningColor;
                    break;
                default:
                    targetColor = normalDamageColor;
                    break;
            }
        }

        motionCoroutine = StartCoroutine(AnimateMotion(targetColor, isCrit, element, hitIndex));
    }

    public void SetupMiss()
    {
        InitializeComponents();

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (motionCoroutine != null)
            StopCoroutine(motionCoroutine);

        textMesh.text = "MISS";
        textMesh.fontSize = baseFontSize * 0.9f;
        motionCoroutine = StartCoroutine(AnimateMotion(missColor, false, ElementType.None, 0));
    }

    private IEnumerator AnimateMotion(Color textColor, bool isCrit, ElementType element = ElementType.None, int hitIndex = 0)
    {
        // Phân tách hướng văng theo loại sát thương và thứ tự đòn bồi trong cùng khoảnh khắc:
        // - hitIndex 0: Đòn đánh ban đầu (Nguyên tố -> Phải, Crit -> Trái, Thường -> Giữa)
        // - hitIndex 1 (vd: Sát thủ Backstab / Sét giật): Lệch Phải + nâng cao hơn
        // - hitIndex 2 (vd: Sát thủ Execute / Phục thù): Lệch Trái + nâng cao hơn nữa
        // - hitIndex 3+: Bay vọt thẳng lên đỉnh cao
        float xOffset = 0f;
        float yExtra = 0f;

        if (hitIndex == 0)
        {
            if (element != ElementType.None)
            {
                xOffset = 0.5f + Random.Range(-0.05f, 0.08f);
            }
            else if (isCrit)
            {
                xOffset = -0.35f + Random.Range(-0.06f, 0.05f);
            }
            else
            {
                xOffset = Random.Range(-horizontalSpread * 0.5f, horizontalSpread * 0.5f);
            }
        }
        else if (hitIndex == 1)
        {
            // Đòn bồi thứ 2 (Backstab hoặc proc phụ)
            xOffset = (element != ElementType.None) ? 0.62f : 0.48f;
            xOffset += Random.Range(-0.04f, 0.05f);
            yExtra = 0.22f; // Nhấc cao hơn 0.22m ngay từ đầu để không che số trước
        }
        else if (hitIndex == 2)
        {
            // Đòn bồi thứ 3 (Execute kết liễu hoặc đòn tiếp)
            xOffset = -0.48f + Random.Range(-0.05f, 0.04f);
            yExtra = 0.38f;
        }
        else
        {
            // Đòn bồi thứ 4 trở đi
            xOffset = Random.Range(-0.1f, 0.1f);
            yExtra = 0.5f;
        }

        Vector3 startPos = transform.position + new Vector3(xOffset, yExtra, 0);
        // Bay lên cao và chếch nhẹ ra ngoài để các số tách nhau rõ ràng
        Vector3 endPos = startPos + new Vector3(xOffset * 0.25f, floatDistance, 0);
        transform.position = startPos;

        float elapsed = 0f;
        Vector3 baseScale = Vector3.one;
        float popMultiplier = isCrit ? 1.18f : 1.08f;

        textMesh.color = textColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Chuyển động bay lên mượt mà theo hàm Ease Out Cubic
            float easeOut = 1f - Mathf.Pow(1f - t, 3);
            transform.position = Vector3.Lerp(startPos, endPos, easeOut);

            // Hiệu ứng Pop Scale: 0 -> to vọt lên -> về scale chuẩn
            float scaleFactor = 1f;
            if (t < 0.15f)
            {
                scaleFactor = Mathf.Lerp(0.5f, popMultiplier, t / 0.15f);
            }
            else if (t < 0.3f)
            {
                scaleFactor = Mathf.Lerp(popMultiplier, 1f, (t - 0.15f) / 0.15f);
            }
            transform.localScale = baseScale * scaleFactor;

            // Mờ dần (Fade out) trong 30% thời lượng cuối
            if (t > 0.7f)
            {
                float fadeT = (t - 0.7f) / 0.3f;
                Color c = textColor;
                c.a = Mathf.Lerp(1f, 0f, fadeT);
                textMesh.color = c;
            }

            yield return null;
        }

        // Hoàn trả về VFX Pool để tái sử dụng, tránh GC Allocation
        if (GameManager.instance != null && GameManager.instance.vfxPool != null)
        {
            GameManager.instance.vfxPool.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        if (motionCoroutine != null)
            StopCoroutine(motionCoroutine);

        transform.localScale = Vector3.one;

        if (textMesh != null)
        {
            Color c = textMesh.color;
            c.a = 1f;
            textMesh.color = c;
        }
    }
}
