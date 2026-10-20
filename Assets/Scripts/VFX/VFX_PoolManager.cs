using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFX_PoolManager : MonoBehaviour
{
    // Lưu các instance đang chờ tái sử dụng theo Prefab gốc
    private readonly Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

    // Lưu mapping giữa InstanceID của object đã sinh ra với Prefab gốc của nó để O(1) tra cứu khi Return
    private readonly Dictionary<int, GameObject> instanceToPrefabMap = new Dictionary<int, GameObject>();

    // Sub-containers trong Hierarchy cho từng loại Prefab để scene luôn ngăn nắp
    private readonly Dictionary<GameObject, Transform> poolParents = new Dictionary<GameObject, Transform>();

    /// <summary>
    /// Lấy một VFX instance từ Pool hoặc sinh mới nếu chưa có sẵn.
    /// </summary>
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null)
            return null;

        Queue<GameObject> poolQueue = GetOrCreateQueue(prefab);

        GameObject instance = null;

        // Lấy từ pool nếu có sẵn object còn sống
        while (poolQueue.Count > 0)
        {
            GameObject candidate = poolQueue.Dequeue();
            if (candidate != null)
            {
                instance = candidate;
                break;
            }
        }

        // Nếu pool đang rỗng thì tạo mới
        if (instance == null)
        {
            instance = Instantiate(prefab, position, rotation, parent);
            instanceToPrefabMap[instance.GetInstanceID()] = prefab;
        }
        else
        {
            // Cập nhật lại transform trước khi active
            Transform t = instance.transform;
            t.position = position;
            t.rotation = rotation;

            if (parent != null)
            {
                t.SetParent(parent);
            }
            else if (t.parent != null)
            {
                t.SetParent(null);
            }

            instance.SetActive(true);
        }

        return instance;
    }

    /// <summary>
    /// Hoàn trả ngay lập tức một VFX instance về Pool.
    /// </summary>
    public void ReturnToPool(GameObject instance)
    {
        if (instance == null)
            return;

        int id = instance.GetInstanceID();
        if (instanceToPrefabMap.TryGetValue(id, out GameObject prefab))
        {
            // Tránh enqueue nhiều lần nếu đối tượng đã inactive
            if (!instance.activeSelf)
                return;

            instance.SetActive(false);

            // Đưa về sub-container của PoolManager trong Hierarchy để scene gọn gàng
            Transform poolParent = GetOrCreateSubContainer(prefab);
            instance.transform.SetParent(poolParent);

            Queue<GameObject> queue = GetOrCreateQueue(prefab);
            queue.Enqueue(instance);
        }
        else
        {
            // Nếu không thuộc pool (ví dụ sinh bằng Instantiate thủ công ở nơi khác), fallback về Destroy
            Destroy(instance);
        }
    }

    /// <summary>
    /// Hẹn giờ hoàn trả VFX instance về Pool sau một khoảng delay (seconds).
    /// </summary>
    public void ReturnToPool(GameObject instance, float delay)
    {
        if (instance == null)
            return;

        if (delay <= 0f)
        {
            ReturnToPool(instance);
        }
        else
        {
            StartCoroutine(ReturnToPoolCo(instance, delay));
        }
    }

    private IEnumerator ReturnToPoolCo(GameObject instance, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (instance != null)
        {
            ReturnToPool(instance);
        }
    }

    /// <summary>
    /// Khởi tạo sẵn một số lượng VFX nhất định trong Pool để tránh giật khung hình ở lần xuất hiện đầu tiên.
    /// </summary>
    public void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0)
            return;

        Queue<GameObject> queue = GetOrCreateQueue(prefab);
        Transform parent = GetOrCreateSubContainer(prefab);

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, parent);
            obj.SetActive(false);
            instanceToPrefabMap[obj.GetInstanceID()] = prefab;
            queue.Enqueue(obj);
        }
    }

    private Queue<GameObject> GetOrCreateQueue(GameObject prefab)
    {
        if (!poolDictionary.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            poolDictionary[prefab] = queue;
        }
        return queue;
    }

    private Transform GetOrCreateSubContainer(GameObject prefab)
    {
        if (!poolParents.TryGetValue(prefab, out Transform container) || container == null)
        {
            GameObject sub = new GameObject($"[Pool] {prefab.name}");
            sub.transform.SetParent(transform);
            container = sub.transform;
            poolParents[prefab] = container;
        }
        return container;
    }
}
