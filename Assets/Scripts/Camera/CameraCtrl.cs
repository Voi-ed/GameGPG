using UnityEngine;
using Unity.Cinemachine;

public class CameraCtrl : MonoBehaviour
{
    public static CameraCtrl instance { get; private set; }

    [SerializeField] private CinemachineCamera cinemachineCam;

    private void Awake()
    {
        instance = this;

        if (cinemachineCam == null)
            cinemachineCam = GetComponentInChildren<CinemachineCamera>(true);
    }

    private void Start()
    {
        FindAndFollowPlayer(true);
    }

    private void OnEnable()
    {
        FindAndFollowPlayer(true);
    }

    /// <summary>
    /// Tự động tìm Player trong scene và gắn làm mục tiêu theo dõi
    /// </summary>
    public void FindAndFollowPlayer(bool snapImmediately = false)
    {
        Player player = Player.instance != null ? Player.instance : FindFirstObjectByType<Player>();
        if (player != null)
        {
            SetTarget(player.transform, snapImmediately);
        }
    }

    /// <summary>
    /// Đổi mục tiêu theo dõi của Camera (hỗ trợ cutscene, boss, sự kiện)
    /// </summary>
    public void SetTarget(Transform target, bool snapImmediately = false)
    {
        if (cinemachineCam == null)
            cinemachineCam = GetComponentInChildren<CinemachineCamera>(true);

        if (cinemachineCam != null && target != null)
        {
            cinemachineCam.Target.TrackingTarget = target;

            if (snapImmediately)
            {
                SnapToTarget(target);
            }
        }
    }

    /// <summary>
    /// Đưa Camera lập tức đến tọa độ mục tiêu mà không bị trôi (lerp) màn hình khi chuyển cảnh
    /// </summary>
    public void SnapToTarget(Transform target = null)
    {
        if (cinemachineCam == null)
            cinemachineCam = GetComponentInChildren<CinemachineCamera>(true);

        if (cinemachineCam == null)
            return;

        Transform currentTarget = target != null ? target : cinemachineCam.Target.TrackingTarget;
        if (currentTarget == null)
            return;

        // Reset tracking state trong Cinemachine để không interpolate từ vị trí cũ
        cinemachineCam.PreviousStateIsValid = false;

        Vector3 targetPos = currentTarget.position;
        targetPos.z = cinemachineCam.transform.position.z;
        cinemachineCam.ForceCameraPosition(targetPos, cinemachineCam.transform.rotation);
    }

    /// <summary>
    /// Lấy biên ngang thế giới của Camera (minX: mép trái, maxX: mép phải)
    /// </summary>
    public bool GetCameraHorizontalBounds(out float minX, out float maxX)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            minX = 0f;
            maxX = 0f;
            return false;
        }

        float halfWidth = cam.orthographicSize * cam.aspect;
        float camX = cam.transform.position.x;
        minX = camX - halfWidth;
        maxX = camX + halfWidth;
        return true;
    }

    private Collider2D cachedConfinerCollider;

    /// <summary>
    /// Lấy biên ngang thế giới của CinemachineConfiner (minX: mép trái map, maxX: mép phải map)
    /// </summary>
    public bool GetConfinerHorizontalBounds(out float minX, out float maxX)
    {
        if (cachedConfinerCollider == null)
        {
            GameObject confinerObj = GameObject.Find("CinemachineConfiner");
            if (confinerObj != null)
                cachedConfinerCollider = confinerObj.GetComponent<Collider2D>();
        }

        if (cachedConfinerCollider != null)
        {
            Bounds b = cachedConfinerCollider.bounds;
            minX = b.min.x;
            maxX = b.max.x;
            return true;
        }

        minX = 0f;
        maxX = 0f;
        return false;
    }
}
