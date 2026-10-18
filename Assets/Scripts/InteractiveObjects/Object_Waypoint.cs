using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum WaypointDirection
{
    AutoByPosition, // Tự động: X >= 0 là Cổng Tiến (Next Map), X < 0 là Cổng Lùi (Previous Map)
    NextMap,        // Cổng Tiến (Next Map)
    PreviousMap     // Cổng Lùi (Previous Map)
}

public class Object_Waypoint : MonoBehaviour
{
    public static readonly string[] MapList = new string[]
    {
        "1_Village & Camp",
        "2_Forest Green",
        "3_Swamp",
        "4_Caves",
        "5_Castle Dungeon",
        "6_Inferno Underworld",
        "7_Wasteland",
        "8_Metroidvania Ruins"
    };

    [Header("Cấu hình Cổng (Tự động 100%)")]
    [Tooltip("AutoByPosition: Đặt bên phải sẽ tự sang map sau, đặt bên trái sẽ tự lùi về map trước")]
    [SerializeField] private WaypointDirection direction = WaypointDirection.AutoByPosition;

    [Tooltip("Lặp lại vòng: Map 8 đi tiếp sẽ quay về Map 1, Map 1 lùi lại sẽ sang Map 8")]
    [SerializeField] private bool loopPlaylist = true;

    [Tooltip("Điểm xuất hiện của Player (để trống sẽ tự lấy vị trí an toàn cạnh Waypoint)")]
    [SerializeField] private Transform respawnPoint;

    [Header("Trigger Cooldown")]
    [Tooltip("Thời gian chờ tối thiểu (giây) sau khi vừa chuyển map mới cho phép cổng kích hoạt lại")]
    [SerializeField] private float triggerCooldown = 1.0f;

    [Header("Thông tin Tự động Nhận diện (Chỉ xem)")]
    [SerializeField] private string targetSceneDebug;
    [SerializeField] private WaypointDirection effectiveDirectionDebug;
    [SerializeField] private bool canBeTriggered = true;

    private float cooldownEndTime = 0f;
    private WaypointDirection effectiveDirection;

    private void Awake()
    {
        canBeTriggered = true;
        effectiveDirection = CalculateEffectiveDirection();
    }

    /// <summary>
    /// Tính toán hướng hiệu dụng dựa trên cấu hình hoặc vị trí tương đối giữa các cổng trong Scene
    /// </summary>
    private WaypointDirection CalculateEffectiveDirection()
    {
        if (direction != WaypointDirection.AutoByPosition)
            return direction;

        if (Application.isPlaying)
        {
            var waypoints = FindObjectsByType<Object_Waypoint>(FindObjectsSortMode.None);
            if (waypoints.Length > 1)
            {
                float avgX = 0f;
                for (int i = 0; i < waypoints.Length; i++)
                {
                    avgX += waypoints[i].transform.position.x;
                }
                avgX /= waypoints.Length;
                return transform.position.x >= avgX ? WaypointDirection.NextMap : WaypointDirection.PreviousMap;
            }
        }

        return transform.position.x >= 0 ? WaypointDirection.NextMap : WaypointDirection.PreviousMap;
    }

    /// <summary>
    /// Xác định hướng hiệu dụng (NextMap hay PreviousMap)
    /// </summary>
    public WaypointDirection GetEffectiveDirection()
    {
        if (direction != WaypointDirection.AutoByPosition)
            return direction;

        if (Application.isPlaying && effectiveDirection != WaypointDirection.AutoByPosition)
            return effectiveDirection;

        return CalculateEffectiveDirection();
    }

    /// <summary>
    /// Tự động tính toán Scene đích dựa vào tên scene hiện tại và hướng của cổng
    /// </summary>
    public string GetTargetScene()
    {
        string currentScene = gameObject.scene.IsValid() ? gameObject.scene.name : SceneManager.GetActiveScene().name;
        WaypointDirection dir = GetEffectiveDirection();

        int currentIndex = FindSceneIndex(currentScene);

        if (currentIndex != -1)
        {
            int targetIndex;
            if (dir == WaypointDirection.NextMap)
            {
                targetIndex = currentIndex + 1;
                if (targetIndex >= MapList.Length)
                    targetIndex = loopPlaylist ? 0 : MapList.Length - 1;
            }
            else
            {
                targetIndex = currentIndex - 1;
                if (targetIndex < 0)
                    targetIndex = loopPlaylist ? MapList.Length - 1 : 0;
            }
            return MapList[targetIndex];
        }

        // Fallback: nếu scene hiện tại không nằm trong 8 map (ví dụ Level_0)
        return dir == WaypointDirection.NextMap ? MapList[0] : MapList[MapList.Length - 1];
    }

    private int FindSceneIndex(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return -1;

        // 1. So sánh tên chính xác
        for (int i = 0; i < MapList.Length; i++)
        {
            if (string.Equals(MapList[i], sceneName, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        // 2. Tìm theo số thứ tự ở đầu tên scene (ví dụ: "1_", "2_", ...)
        Match match = Regex.Match(sceneName, @"^(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int sceneNum))
        {
            int index = sceneNum - 1;
            if (index >= 0 && index < MapList.Length)
                return index;
        }

        return -1;
    }

    /// <summary>
    /// Vị trí xuất hiện của Player: ưu tiên respawnPoint; nếu không có, tự động tính vị trí an toàn ở ngoài Collider của cổng
    /// </summary>
    public Vector3 GetSpawnPosition()
    {
        if (respawnPoint != null)
            return respawnPoint.position;

        Collider2D col = GetComponent<Collider2D>();
        float sign = (GetEffectiveDirection() == WaypointDirection.PreviousMap) ? 1f : -1f;

        if (col != null)
        {
            float safeDistance = col.bounds.extents.x + 0.8f;
            Vector3 center = col.bounds.center;
            return new Vector3(center.x + sign * safeDistance, transform.position.y, transform.position.z);
        }

        return transform.position + new Vector3(sign * 1.5f, 0, 0);
    }

    /// <summary>
    /// Khóa tạm thời cổng trong một khoảng thời gian (tránh trigger lặp khi vừa load map)
    /// </summary>
    public void DisableTriggerTemporarily(float duration)
    {
        canBeTriggered = false;
        cooldownEndTime = Time.time + duration;
    }

    public void SetCanTrigger(bool state)
    {
        canBeTriggered = state;
        if (!state)
        {
            cooldownEndTime = Mathf.Max(cooldownEndTime, Time.time + triggerCooldown);
        }
        else
        {
            cooldownEndTime = 0f;
        }
    }

    #region Tương thích ngược với hệ thống cũ
    public RespawnType GetWaypointType()
    {
        return GetEffectiveDirection() == WaypointDirection.NextMap ? RespawnType.Exit : RespawnType.Enter;
    }

    public Vector3 GetPositionAndSetTriggerFalse()
    {
        DisableTriggerTemporarily(triggerCooldown);
        return GetSpawnPosition();
    }
    #endregion

    private void OnValidate()
    {
        effectiveDirectionDebug = GetEffectiveDirection();
        targetSceneDebug = GetTargetScene();

        string actionText = effectiveDirectionDebug == WaypointDirection.NextMap ? "Tiến tới" : "Lùi về";
        gameObject.name = $"Object_Waypoint - [{actionText}: {targetSceneDebug}]";
    }

    private bool IsPlayer(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            return true;
        if (collision.GetComponentInParent<Player>() != null)
            return true;
        return false;
    }

    private void TryTrigger(Collider2D collision)
    {
        if (!IsPlayer(collision))
            return;

        // Nếu đang trong thời gian cooldown (vừa load map mới sang) thì không kích hoạt
        if (Time.time < cooldownEndTime)
            return;

        if (GameManager.instance == null)
            return;

        // Nếu GameManager đang trong quá trình chuyển scene thì bỏ qua
        if (GameManager.instance.isTransitioningScene)
            return;

        // Khóa trigger để không gọi liên tiếp nhiều lần
        cooldownEndTime = Time.time + 5f;
        canBeTriggered = false;

        string targetScene = GetTargetScene();
        WaypointDirection dir = GetEffectiveDirection();

        GameManager.instance.ChangeSceneByWaypoint(targetScene, dir);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryTrigger(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryTrigger(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (IsPlayer(collision))
        {
            if (Time.time >= cooldownEndTime)
            {
                canBeTriggered = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        WaypointDirection dir = GetEffectiveDirection();
        Gizmos.color = dir == WaypointDirection.NextMap ? new Color(0.2f, 1f, 0.2f, 0.7f) : new Color(1f, 0.6f, 0.1f, 0.7f);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(1.5f, 3f, 0));
        }

        // Vẽ mũi tên hướng đi
        Vector3 pos = transform.position;
        Vector3 arrowDir = dir == WaypointDirection.NextMap ? Vector3.right : Vector3.left;
        Gizmos.DrawRay(pos, arrowDir * 2f);
        Gizmos.DrawRay(pos + arrowDir * 2f, (arrowDir * -0.5f + Vector3.up * 0.3f));
        Gizmos.DrawRay(pos + arrowDir * 2f, (arrowDir * -0.5f + Vector3.down * 0.3f));

        // Vẽ điểm xuất hiện trực quan (Cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(GetSpawnPosition(), 0.3f);
    }
}
