using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class GameManager
{

    /// <summary>
    /// Chuyển scene tự động thông qua Waypoint (100% tự động điều phối vị trí xuất hiện của Player)
    /// </summary>
    public void ChangeSceneByWaypoint(string sceneName, WaypointDirection exitDirection)
    {
        if (isTransitioningScene)
            return;

        isTransitioningScene = true;

        if (SaveManager.instance != null)
            SaveManager.instance.SaveGame();

        Time.timeScale = 1;
        StartCoroutine(ChangeSceneByWaypointCo(sceneName, exitDirection));
    }

    private IEnumerator ChangeSceneByWaypointCo(string sceneName, WaypointDirection exitDirection)
    {
        // Ghi nhớ Máu và Cooldown trước khi đổi map
        CapturePlayerState();

        UI_FadeScreen fadeScreen = FindFadeScreenUI();

        if (fadeScreen != null)
        {
            fadeScreen.DoFadeOut();
            yield return fadeScreen.fadeEffectCo;
        }

        SceneManager.LoadScene(sceneName);

        dataLoaded = false;
        yield return null;

        if (SaveManager.instance != null)
        {
            while (!dataLoaded)
            {
                yield return null;
            }
        }

        fadeScreen = FindFadeScreenUI();
        if (fadeScreen != null)
            fadeScreen.DoFadeIn();

        Player player = Player.instance != null ? Player.instance : FindFirstObjectByType<Player>();
        if (player == null)
        {
            isTransitioningScene = false;
            yield break;
        }

        // Bàn giao lại Máu & Cooldown cho Player ở scene mới
        ApplyPlayerState(player);

        // Đi qua cổng NextMap -> Xuất hiện ở cổng PreviousMap của map mới
        // Đi qua cổng PreviousMap -> Xuất hiện ở cổng NextMap của map mới
        WaypointDirection arrivalDirection = (exitDirection == WaypointDirection.NextMap)
            ? WaypointDirection.PreviousMap
            : WaypointDirection.NextMap;

        var waypoints = FindObjectsByType<Object_Waypoint>(FindObjectsSortMode.None);
        Object_Waypoint targetWaypoint = waypoints.FirstOrDefault(wp => wp.GetEffectiveDirection() == arrivalDirection);

        // Fallback: nếu scene mới chỉ có 1 cổng
        if (targetWaypoint == null && waypoints.Length > 0)
            targetWaypoint = waypoints[0];

        if (targetWaypoint != null)
        {
            Vector3 position = targetWaypoint.GetSpawnPosition();

            targetWaypoint.DisableTriggerTemporarily(1.0f);
            player.TeleportPlayer(position);

            if (CameraCtrl.instance != null)
            {
                CameraCtrl.instance.SnapToTarget(player.transform);
            }
        }

        isTransitioningScene = false;
    }

    public void ChangeScene(string sceneName, RespawnType respwanType)
    {
        if (isTransitioningScene)
            return;

        isTransitioningScene = true;

        if (SaveManager.instance != null)
            SaveManager.instance.SaveGame();

        Time.timeScale = 1;
        StartCoroutine(ChangeSceneCo(sceneName, respwanType));
    }

    private IEnumerator ChangeSceneCo(string sceneName, RespawnType respawnType)
    {
        // Ghi nhớ Máu và Cooldown trước khi đổi scene
        CapturePlayerState();

        UI_FadeScreen fadeScreen = FindFadeScreenUI();

        if (fadeScreen != null)
        {
            fadeScreen.DoFadeOut();
            yield return fadeScreen.fadeEffectCo;
        }

        SceneManager.LoadScene(sceneName);

        dataLoaded = false;
        yield return null;

        if (SaveManager.instance != null)
        {
            while (!dataLoaded)
            {
                yield return null;
            }
        }

        fadeScreen = FindFadeScreenUI();
        if (fadeScreen != null)
            fadeScreen.DoFadeIn();

        Player player = Player.instance != null ? Player.instance : FindFirstObjectByType<Player>();

        if (player == null)
        {
            isTransitioningScene = false;
            yield break;
        }

        // Bàn giao lại Máu & Cooldown cho Player ở scene mới
        ApplyPlayerState(player);

        Vector3 position = GetNewPlayerPosition(respawnType);

        if (position != Vector3.zero)
            player.TeleportPlayer(position);

        if (CameraCtrl.instance != null)
        {
            CameraCtrl.instance.SnapToTarget(player.transform);
        }

        isTransitioningScene = false;
    }

    private UI_FadeScreen FindFadeScreenUI()
    {
        if (UI.instance != null && UI.instance.fadeScreenUI != null)
            return UI.instance.fadeScreenUI;
        else
            return FindFirstObjectByType<UI_FadeScreen>();
    }

    private Vector3 GetNewPlayerPosition(RespawnType type)
    {
        if (type == RespawnType.Portal)
        {
            Object_Portal portal = Object_Portal.instnace;
            if (portal == null)
                return Vector3.zero;

            Vector3 position = portal.GetPosition();

            portal.SetTrigger(false);
            portal.DisableIfNeeded();

            return position;
        }

        if (type == RespawnType.NonSpecific)
        {
            if (SaveManager.instance == null)
                return Vector3.zero;

            var data = SaveManager.instance.GetGameData();
            var checkpoints = FindObjectsByType<Object_Checkpoint>(FindObjectsSortMode.None);
            var unlockedCheckpoints = checkpoints
                .Where(cp => data.unlockedCheckpoints.TryGetValue(cp.GetCheckpointId(), out bool unlocked) && unlocked)
                .Select(cp => cp.GetPosition())
                .ToList();

            var enterWaypoints = FindObjectsByType<Object_Waypoint>(FindObjectsSortMode.None)
                .Where(wp => wp.GetWaypointType() == RespawnType.Enter)
                .Select(wp => wp.GetPositionAndSetTriggerFalse())
                .ToList();

            var selectedPositions = unlockedCheckpoints.Concat(enterWaypoints).ToList();

            if (selectedPositions.Count == 0)
                return Vector3.zero;

            return selectedPositions
                .OrderBy(position => Vector3.Distance(position, lastPlayerPosition))
                .First();
        }

        return GetWaypointPosition(type);
    }

    private Vector3 GetWaypointPosition(RespawnType type)
    {
        var waypoints = FindObjectsByType<Object_Waypoint>(FindObjectsSortMode.None);

        foreach (var point in waypoints)
        {
            if (point.GetWaypointType() == type)
                return point.GetPositionAndSetTriggerFalse();
        }

        return Vector3.zero;
    }
}
