using System;
using System.Collections;
using UnityEngine;

public partial class GameManager
{
    [Header("Dragon Transformation Settings")]
    [SerializeField] private GameObject dragonPrefab;
    [SerializeField] private GameObject transformVfxPrefab;
    [SerializeField] private float defaultDragonDuration = 20f;

    private GameObject activeDragonInstance;
    private Player cachedPlayer;
    private Coroutine dragonDurationCoroutine;
    private Coroutine invulnerabilityCoroutine;

    public bool IsDragonTransformed => activeDragonInstance != null;
    public GameObject ActiveDragon => activeDragonInstance;
    public event Action<bool> OnDragonTransformationChanged;

    private void Update()
    {
        UpdateDragonTransformationInput();
    }

    private void UpdateDragonTransformationInput()
    {
#if UNITY_EDITOR
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (!IsDragonTransformed)
            {
                Debug.Log("[GameManager] Nhấn phím T: Bắt đầu Hóa Rồng.");
                StartDragonTransformation(defaultDragonDuration);
            }
            else
            {
                Debug.Log("[GameManager] Nhấn phím T: Hủy sớm, hoàn về Player.");
                EndDragonTransformation();
            }
        }
#endif
    }

    /// <summary>
    /// Kích hoạt chuỗi biến hình thành Rồng (Spawn Dragon_Form, ẩn Player, chuyển Camera theo dõi Rồng).
    /// </summary>
    public bool StartDragonTransformation(float duration = -1f)
    {
        if (IsDragonTransformed)
            return false;

        Player player = Player.instance != null ? Player.instance : FindFirstObjectByType<Player>(FindObjectsInactive.Include);
        if (player == null || player.health == null || player.health.isDead)
        {
            Debug.LogWarning("[GameManager] Không tìm thấy Player hoặc Player đã chết, không thể hóa rồng.");
            return false;
        }

        EnsureTransformationReferences();

        if (duration <= 0f)
            duration = defaultDragonDuration;

        cachedPlayer = player;
        Vector3 spawnPos = player.transform.position;
        int facingDir = player.facingDir;

        // 1. Sinh VFX nổ Hắc Hỏa tại chân Player
        SpawnTransformationVfx(spawnPos);

        // 2. Âm thanh biến hình uy lực
        if (AudioManager.instance != null)
            AudioManager.instance.PlayGlobalSFX("dragon_attack");

        // 3. Sinh thực thể Rồng (Dragon_Form.prefab)
        if (dragonPrefab == null)
        {
            Debug.LogError("[GameManager] dragonPrefab chưa được gán trong Inspector!");
            return false;
        }

        activeDragonInstance = Instantiate(dragonPrefab, spawnPos, Quaternion.identity);
        Dragon_Controller dragonCtrl = activeDragonInstance.GetComponent<Dragon_Controller>();
        if (dragonCtrl != null)
        {
            // Thiết lập hướng mặt khớp với Player lúc biến
            if (facingDir < 0 && dragonCtrl.isFacingRight)
            {
                dragonCtrl.Flip();
            }
        }

        // 4. Reset VFX thị giác và ẩn Player
        if (player.vfx != null)
            player.vfx.ResetVisuals();

        if (player.dragonForm != null)
            player.dragonForm.ForceResetVisuals();

        if (player.rb != null)
            player.rb.linearVelocity = Vector2.zero;
        player.gameObject.SetActive(false);

        // 5. Chuyển Camera bám theo Rồng
        if (CameraCtrl.instance != null)
        {
            CameraCtrl.instance.SetTarget(activeDragonInstance.transform);
        }

        // 6. Bắt đầu đếm ngược thời gian duy trì
        if (dragonDurationCoroutine != null)
            StopCoroutine(dragonDurationCoroutine);
        dragonDurationCoroutine = StartCoroutine(DragonDurationRoutine(duration));

        Debug.Log($"[GameManager] Hóa Rồng thành công! Thời gian duy trì: {duration}s.");
        OnDragonTransformationChanged?.Invoke(true);
        return true;
    }

    /// <summary>
    /// Hoàn về dạng Player (hủy Rồng, đưa Player về vị trí Rồng, chuyển Camera lại cho Player, cấp bất tử ngắn).
    /// Được gọi tự động khi hết giờ hoặc khi Rồng chết.
    /// </summary>
    public void EndDragonTransformation()
    {
        if (!IsDragonTransformed)
            return;

        if (dragonDurationCoroutine != null)
        {
            StopCoroutine(dragonDurationCoroutine);
            dragonDurationCoroutine = null;
        }

        Vector3 returnPos = activeDragonInstance != null ? activeDragonInstance.transform.position : Vector3.zero;

        // 1. Sinh VFX khói lửa tan biến tại vị trí Rồng
        if (activeDragonInstance != null)
        {
            SpawnTransformationVfx(returnPos);
            Destroy(activeDragonInstance);
            activeDragonInstance = null;
        }

        // 2. Kích hoạt lại Player tại vị trí Rồng
        Player player = cachedPlayer != null ? cachedPlayer : (Player.instance != null ? Player.instance : FindFirstObjectByType<Player>(FindObjectsInactive.Include));
        if (player != null)
        {
            player.transform.position = returnPos;
            player.gameObject.SetActive(true);

            // Khôi phục hoàn toàn visual nguyên bản, xóa bỏ flash trắng và tắt visual rồng phụ
            if (player.vfx != null)
                player.vfx.ResetVisuals();

            if (player.dragonForm != null)
                player.dragonForm.ForceResetVisuals();

            Transform dragonAvatar = player.transform.Find("Dragon_Avatar");
            if (dragonAvatar != null)
                dragonAvatar.gameObject.SetActive(false);

            Transform knightVisual = player.transform.Find("Animator");
            if (knightVisual != null)
                knightVisual.gameObject.SetActive(true);

            if (player.anim != null)
            {
                player.anim.enabled = true;
                player.anim.Play("idle", 0, 0f);
            }

            if (player.rb != null)
                player.rb.linearVelocity = Vector2.zero;

            if (player.stateMachine != null && player.idleState != null)
                player.stateMachine.ChangeState(player.idleState);

            // Cấp 1.5s miễn nhiễm sát thương (I-Frames) để an toàn khi vừa trở lại làm người
            if (invulnerabilityCoroutine != null)
                StopCoroutine(invulnerabilityCoroutine);
            invulnerabilityCoroutine = StartCoroutine(PlayerTemporaryInvulnerabilityRoutine(player, 1.5f));

            // 3. Chuyển Camera bám lại theo Player
            if (CameraCtrl.instance != null)
            {
                CameraCtrl.instance.SetTarget(player.transform);
            }

            Debug.Log($"[GameManager] Hoàn về dạng Player thành công tại vị trí: {returnPos}!");
        }
        else
        {
            Debug.LogError("[GameManager] Không tìm thấy Player để kích hoạt lại sau khi hóa rồng!");
        }

        OnDragonTransformationChanged?.Invoke(false);
    }

    private IEnumerator DragonDurationRoutine(float duration)
    {
        Debug.Log($"[GameManager] Đang đếm ngược hóa rồng: {duration}s");
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (activeDragonInstance == null)
            {
                Debug.Log("[GameManager] Rồng đã biến mất trước khi hết giờ (có thể đã chết), kết thúc đếm ngược.");
                yield break;
            }
            yield return null;
            elapsed += Time.deltaTime;
        }

        Debug.Log("[GameManager] Đã hết thời gian hóa rồng! Bắt đầu hoàn về dạng Player...");
        EndDragonTransformation();
    }

    private IEnumerator PlayerTemporaryInvulnerabilityRoutine(Player player, float duration)
    {
        if (player != null && player.health != null)
        {
            player.health.SetCanTakeDamage(false);
            yield return new WaitForSeconds(duration);
            if (player != null && player.health != null)
            {
                player.health.SetCanTakeDamage(true);
            }
        }
    }

    private void SpawnTransformationVfx(Vector3 position)
    {
        if (transformVfxPrefab != null)
        {
            GameObject vfx = Instantiate(transformVfxPrefab, position, Quaternion.identity);
            Renderer[] renderers = vfx.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.sortingLayerName = "Default";
                r.sortingOrder = Mathf.Clamp(r.sortingOrder, 1, 10);
            }
            Destroy(vfx, 1.5f);
        }
    }

    private void EnsureTransformationReferences()
    {
#if UNITY_EDITOR
        if (dragonPrefab == null)
        {
            dragonPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Dragon/Dragon_Form.prefab");
        }
        if (transformVfxPrefab == null)
        {
            transformVfxPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Vfx_UseItem/Vfx_Dragon_FireExplosion.prefab");
        }
#endif
    }
}
