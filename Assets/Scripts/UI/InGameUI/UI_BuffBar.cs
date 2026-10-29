using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuffBar : MonoBehaviour
{
    [Header("Buff Slot Prefab / Template")]
    [SerializeField] private GameObject buffSlotPrefab;
    [SerializeField] private Transform slotsParent;

    [Header("Positioning Configuration")]
    [Tooltip("Khoảng cách phía trên Quả Cầu Máu (Default: 40, 185)")]
    [SerializeField] private Vector2 defaultAnchoredPosition = new Vector2(40f, 185f);

    private Player player;
    private Player_Stats playerStats;
    private List<UI_BuffSlot> activeSlots = new List<UI_BuffSlot>();

    private Sprite cachedSolidSprite;
    private Sprite cachedRingSprite;

    private void Awake()
    {
        if (slotsParent == null)
            slotsParent = transform;

        EnsureLayoutGroup();
    }

    private void Start()
    {
        InitializeWithPlayer();
    }

    private void EnsureLayoutGroup()
    {
        HorizontalLayoutGroup hlg = slotsParent.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null)
        {
            hlg = slotsParent.gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        hlg.spacing = 8f;
        hlg.childAlignment = TextAnchor.LowerLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        ContentSizeFitter csf = slotsParent.GetComponent<ContentSizeFitter>();
        if (csf == null)
        {
            csf = slotsParent.gameObject.AddComponent<ContentSizeFitter>();
        }
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public void Setup(Player targetPlayer)
    {
        UnsubscribeFromStats();

        player = targetPlayer;
        if (player != null && player.stats is Player_Stats ps)
        {
            playerStats = ps;
            SubscribeToStats();
            RefreshAllBuffs();
        }
    }

    private void InitializeWithPlayer()
    {
        if (player == null)
        {
            player = Player.instance != null ? Player.instance : FindFirstObjectByType<Player>();
        }

        if (player != null && playerStats == null && player.stats is Player_Stats ps)
        {
            playerStats = ps;
            SubscribeToStats();
            RefreshAllBuffs();
        }
    }

    private void SubscribeToStats()
    {
        if (playerStats == null)
            return;

        playerStats.OnBuffListChanged += RefreshAllBuffs;
        playerStats.OnBuffsTick += UpdateBuffTicks;
    }

    private void UnsubscribeFromStats()
    {
        if (playerStats == null)
            return;

        playerStats.OnBuffListChanged -= RefreshAllBuffs;
        playerStats.OnBuffsTick -= UpdateBuffTicks;
    }

    private void OnEnable()
    {
        SubscribeToStats();
        RefreshAllBuffs();
    }

    private void OnDisable()
    {
        UnsubscribeFromStats();
    }

    private void OnDestroy()
    {
        UnsubscribeFromStats();
    }

    public void RefreshAllBuffs()
    {
        if (playerStats == null)
            return;

        var buffs = playerStats.GetActiveBuffs();

        // 1. Dọn dẹp các slot thừa hoặc không còn trong danh sách buff
        for (int i = activeSlots.Count - 1; i >= 0; i--)
        {
            UI_BuffSlot slot = activeSlots[i];
            bool stillActive = false;
            foreach (var b in buffs)
            {
                if (b.sourceId == slot.CurrentSourceId)
                {
                    stillActive = true;
                    break;
                }
            }

            if (!stillActive)
            {
                activeSlots.RemoveAt(i);
                Destroy(slot.gameObject);
            }
        }

        // 2. Thêm mới hoặc cập nhật các buff hiện có
        foreach (var buff in buffs)
        {
            UI_BuffSlot existing = activeSlots.Find(s => s.CurrentSourceId == buff.sourceId);
            if (existing != null)
            {
                existing.UpdateSlot(buff);
            }
            else
            {
                UI_BuffSlot newSlot = CreateNewSlot();
                newSlot.Setup(buff);
                activeSlots.Add(newSlot);
            }
        }
    }

    private void UpdateBuffTicks()
    {
        if (playerStats == null)
            return;

        var buffs = playerStats.GetActiveBuffs();
        foreach (var slot in activeSlots)
        {
            foreach (var buff in buffs)
            {
                if (buff.sourceId == slot.CurrentSourceId)
                {
                    slot.UpdateSlot(buff);
                    break;
                }
            }
        }
    }

    private UI_BuffSlot CreateNewSlot()
    {
        if (buffSlotPrefab != null)
        {
            GameObject obj = Instantiate(buffSlotPrefab, slotsParent);
            return obj.GetComponent<UI_BuffSlot>();
        }

        // Procedural Factory Fallback: Tạo slot sắc nét với textures bo tròn chống răng cưa
        EnsureProceduralSprites();
        return UI_BuffSlot.CreateSlot(slotsParent, cachedSolidSprite, cachedRingSprite);
    }

    private void EnsureProceduralSprites()
    {
        if (cachedSolidSprite == null)
        {
            cachedSolidSprite = CreateRoundedBoxSprite(64, 12, Color.white);
        }

        if (cachedRingSprite == null)
        {
            cachedRingSprite = CreateRoundedRingSprite(64, 12, 4f, Color.white);
        }
    }

    // =========================================================================
    // FACTORY METHOD: Tự động khởi tạo Buff Bar tại góc dưới bên trái (trên Quả Cầu Máu)
    // =========================================================================
    public static UI_BuffBar CreateBuffBar(Transform parentCanvas)
    {
        GameObject root = new GameObject("UI_ActiveBuffBar_BottomLeft", typeof(RectTransform));
        root.transform.SetParent(parentCanvas, false);

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero; // Bottom-Left
        rootRect.anchorMax = Vector2.zero;
        rootRect.pivot = new Vector2(0f, 0f);
        rootRect.sizeDelta = new Vector2(300f, 44f);
        rootRect.anchoredPosition = new Vector2(40f, 185f); // Ngay phía trên Health Globe (y=40, size=132)

        UI_BuffBar buffBar = root.AddComponent<UI_BuffBar>();
        buffBar.slotsParent = rootRect;
        buffBar.EnsureLayoutGroup();

        Player p = Player.instance != null ? Player.instance : Object.FindFirstObjectByType<Player>();
        if (p != null)
        {
            buffBar.Setup(p);
        }

        return buffBar;
    }

    // Procedural Graphics
    private static Sprite CreateRoundedBoxSprite(int size, int cornerRadius, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];

        float half = size * 0.5f;
        float r = cornerRadius;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(0, Mathf.Abs(x - half + 0.5f) - (half - r));
                float dy = Mathf.Max(0, Mathf.Abs(y - half + 0.5f) - (half - r));
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01((r - dist) + 0.5f);
                pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private static Sprite CreateRoundedRingSprite(int size, int cornerRadius, float thickness, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];

        float half = size * 0.5f;
        float r = cornerRadius;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(0, Mathf.Abs(x - half + 0.5f) - (half - r));
                float dy = Mathf.Max(0, Mathf.Abs(y - half + 0.5f) - (half - r));
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float outerAlpha = Mathf.Clamp01((r - dist) + 0.5f);
                float innerAlpha = Mathf.Clamp01((r - thickness - dist) + 0.5f);
                float alpha = Mathf.Clamp01(outerAlpha - innerAlpha);
                pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
