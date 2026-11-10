using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý tập trung các NPC trong scene và cache sẵn tọa độ Camera/Confiner để tối ưu hiệu năng tối đa.
/// </summary>
public class NpcManager : MonoBehaviour
{
    public static NpcManager instance { get; private set; }

    [Header("Managed NPCs")]
    [SerializeField] private List<NpcMovement> npcList = new List<NpcMovement>();

    // Fast Cache Tọa độ (truy xuất O(1), không tốn CPU tính toán lặp lại)
    public float CameraMinX { get; private set; }
    public float CameraMaxX { get; private set; }
    public float ConfinerMinX { get; private set; }
    public float ConfinerMaxX { get; private set; }
    public bool HasConfiner { get; private set; }

    private Camera cachedMainCam;
    private Collider2D cachedConfinerCollider;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        CacheCameraAndConfiner();
    }

    private void Start()
    {
        // Cache lại một lần nữa ở Start để đảm bảo các object khác đã khởi tạo xong
        CacheCameraAndConfiner();
    }

    /// <summary>
    /// Tìm và lưu sẵn tham chiếu Camera và Confiner 1 lần duy nhất (Zero GameObject.Find lúc runtime)
    /// </summary>
    private void CacheCameraAndConfiner()
    {
        if (cachedMainCam == null)
            cachedMainCam = Camera.main;

        if (cachedConfinerCollider == null)
        {
            GameObject confinerObj = GameObject.Find("CinemachineConfiner");
            if (confinerObj != null)
                cachedConfinerCollider = confinerObj.GetComponent<Collider2D>();
        }

        if (cachedConfinerCollider != null)
        {
            Bounds b = cachedConfinerCollider.bounds;
            ConfinerMinX = b.min.x;
            ConfinerMaxX = b.max.x;
            HasConfiner = true;
        }
    }

    private void LateUpdate()
    {
        // Chỉ tính toán biên camera ĐÚNG 1 LẦN DUY NHẤT mỗi frame cho toàn bộ game
        if (cachedMainCam == null)
        {
            cachedMainCam = Camera.main;
            if (cachedMainCam == null) return;
        }

        float halfWidth = cachedMainCam.orthographicSize * cachedMainCam.aspect;
        float camX = cachedMainCam.transform.position.x;
        CameraMinX = camX - halfWidth;
        CameraMaxX = camX + halfWidth;
    }

    private void Reset()
    {
        LoadAllNpcInScene();
    }

    [ContextMenu("Load All NPCs In Scene")]
    public void LoadAllNpcInScene()
    {
        npcList.Clear();
        NpcMovement[] foundNpcs = FindObjectsByType<NpcMovement>(FindObjectsSortMode.None);
        npcList.AddRange(foundNpcs);
    }

    public void RegisterNpc(NpcMovement npc)
    {
        if (npc != null && !npcList.Contains(npc))
        {
            npcList.Add(npc);
        }
    }

    public void UnregisterNpc(NpcMovement npc)
    {
        if (npc != null && npcList.Contains(npc))
        {
            npcList.Remove(npc);
        }
    }

    public IReadOnlyList<NpcMovement> GetAllNpcs()
    {
        return npcList;
    }

    public NpcMovement GetNearestNpc(Vector2 position)
    {
        NpcMovement nearest = null;
        float minDistanceSqr = float.MaxValue;

        for (int i = 0; i < npcList.Count; i++)
        {
            if (npcList[i] == null) continue;

            float distSqr = (position - (Vector2)npcList[i].transform.position).sqrMagnitude;
            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                nearest = npcList[i];
            }
        }

        return nearest;
    }

    public void SetAllNpcsActive(bool isActive)
    {
        for (int i = 0; i < npcList.Count; i++)
        {
            if (npcList[i] != null)
            {
                npcList[i].enabled = isActive;
            }
        }
    }

    /// <summary>
    /// Kích hoạt biến mất hàng loạt của toàn bộ NPC và nhân vật (cho Cutscene Comic mở đầu)
    /// </summary>
    public void TriggerMassDisappearance(float staggerDelay = 0.04f)
    {
        StartCoroutine(MassDisappearanceCo(staggerDelay));
    }

    private System.Collections.IEnumerator MassDisappearanceCo(float staggerDelay)
    {
        // Thu thập toàn bộ NPC và nhân vật cần làm biến mất vào một danh sách độc lập
        // (Khắc phục triệt để lỗi duyệt List gốc bị nhảy cóc do OnDisable gọi UnregisterNpc làm sót 50% NPC)
        HashSet<GameObject> targets = new HashSet<GameObject>();

        // 1. Toàn bộ NPC trong danh sách quản lý
        for (int i = 0; i < npcList.Count; i++)
        {
            if (npcList[i] != null && npcList[i].gameObject != null)
                targets.Add(npcList[i].gameObject);
        }

        // 2. Quét thêm toàn bộ component NpcMovement trong Scene (đảm bảo không sót bất kỳ NPC nào)
        NpcMovement[] foundNpcs = FindObjectsByType<NpcMovement>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var npc in foundNpcs)
        {
            if (npc != null && npc.gameObject != null)
                targets.Add(npc.gameObject);
        }

        // 3. Toàn bộ các object con của cụm NPC này
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child != null && child.gameObject != null)
                targets.Add(child.gameObject);
        }

        // 4. Nếu có cụm sinh vật/động vật khác trong scene (ví dụ Fish, Ballon...)
        GameObject fishGroup = GameObject.Find("Fish");
        if (fishGroup != null)
        {
            for (int i = 0; i < fishGroup.transform.childCount; i++)
            {
                Transform child = fishGroup.transform.GetChild(i);
                if (child != null && child.gameObject != null)
                    targets.Add(child.gameObject);
            }
        }

        List<GameObject> targetList = new List<GameObject>(targets);

        // Làm biến mất tuần tự tạo cảm giác luồng hắc ám cuốn từng người đi
        for (int i = 0; i < targetList.Count; i++)
        {
            if (targetList[i] != null && targetList[i].activeSelf)
            {
                targetList[i].SetActive(false);
                if (staggerDelay > 0f)
                    yield return new WaitForSeconds(staggerDelay);
            }
        }

        // Quét vét lần cuối để đảm bảo 100% không còn sót bất kỳ nhân vật nào
        for (int i = 0; i < targetList.Count; i++)
        {
            if (targetList[i] != null && targetList[i].activeSelf)
            {
                targetList[i].SetActive(false);
            }
        }

        npcList.Clear();
    }

    /// <summary>
    /// Đọc nhanh biên Camera từ biến cache
    /// </summary>
    public bool GetCameraHorizontalBounds(out float minX, out float maxX)
    {
        minX = CameraMinX;
        maxX = CameraMaxX;
        return (CameraMaxX - CameraMinX) > 0.001f;
    }

    /// <summary>
    /// Đọc nhanh biên Confiner từ biến cache
    /// </summary>
    public bool GetConfinerHorizontalBounds(out float minX, out float maxX)
    {
        minX = ConfinerMinX;
        maxX = ConfinerMaxX;
        return HasConfiner;
    }
}
