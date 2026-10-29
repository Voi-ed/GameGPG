using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
[DisallowMultipleComponent]
public class EnemySpawnManager : MonoBehaviour
{
    [Header("Spawn Points Management")]
    [Tooltip("Danh sách các điểm spawn quái con (tự động quét từ các GameObject con)")]
    [SerializeField] private List<EnemySpawnPoint> spawnPoints = new List<EnemySpawnPoint>();

    [Header("Gizmos & Debug")]
    [Tooltip("Hiển thị đường nối từ Manager tới các điểm spawn trong Scene View")]
    [SerializeField] private bool showGizmoLinks = true;

    [Tooltip("Màu của đường nối Gizmo")]
    [SerializeField] private Color gizmoLinkColor = new Color(0.2f, 0.8f, 1f, 0.4f);

    public IReadOnlyList<EnemySpawnPoint> SpawnPoints => spawnPoints;
    public int TotalPointsCount => spawnPoints != null ? spawnPoints.Count : 0;

    private void Awake()
    {
        RefreshSpawnPoints();
    }

    private void OnValidate()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            RefreshSpawnPoints();
        }
    }

    /// <summary>
    /// Quét lại toàn bộ các EnemySpawnPoint nằm trong các GameObject con
    /// </summary>
    [ContextMenu("Refresh Spawn Points")]
    public void RefreshSpawnPoints()
    {
        spawnPoints.Clear();
        EnemySpawnPoint[] points = GetComponentsInChildren<EnemySpawnPoint>(true);
        if (points != null)
        {
            spawnPoints.AddRange(points);
        }
    }

    /// <summary>
    /// Kích hoạt sinh quái tại tất cả các điểm spawn
    /// </summary>
    [ContextMenu("Spawn All Enemies")]
    public void SpawnAllEnemies()
    {
        RefreshSpawnPoints();

        foreach (var point in spawnPoints)
        {
            if (point != null)
            {
                point.SpawnEnemy();
            }
        }
    }

    /// <summary>
    /// Xoá quái hiện tại và huỷ tiến trình hồi sinh ở tất cả các điểm spawn
    /// </summary>
    [ContextMenu("Clear All Enemies")]
    public void ClearAllEnemies()
    {
        foreach (var point in spawnPoints)
        {
            if (point != null)
            {
                point.ClearEnemy();
            }
        }
    }

    /// <summary>
    /// Đếm số quái hiện đang còn sống trên toàn map
    /// </summary>
    public int GetActiveEnemyCount()
    {
        int count = 0;
        foreach (var point in spawnPoints)
        {
            if (point != null && point.IsSpawned)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Đếm số điểm spawn đang trong thời gian đếm ngược chờ hồi sinh
    /// </summary>
    public int GetRespawningCount()
    {
        int count = 0;
        foreach (var point in spawnPoints)
        {
            if (point != null && point.IsRespawning)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Tạo một điểm spawn con mới kiểu T
    /// </summary>
    public T CreateChildSpawnPoint<T>(Vector3? position = null, string pointName = null) where T : EnemySpawnPoint
    {
        Vector3 spawnPos = position ?? transform.position;
        string finalName = string.IsNullOrEmpty(pointName) 
            ? $"SpawnPoint_{spawnPoints.Count + 1}" 
            : pointName;

        GameObject newPointObj = new GameObject(finalName);
        newPointObj.transform.SetParent(transform);
        newPointObj.transform.position = spawnPos;

        T newPoint = newPointObj.AddComponent<T>();
        spawnPoints.Add(newPoint);

        return newPoint;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmoLinks || spawnPoints == null)
            return;

        Gizmos.color = gizmoLinkColor;
        foreach (var point in spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawLine(transform.position, point.transform.position);
            }
        }

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.5f, 
            $"[EnemySpawnManager] Điểm spawn: {spawnPoints.Count} | Quái sống: {GetActiveEnemyCount()}"
        );
#endif
    }
}
