#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(EnemySpawnManager))]
public class EnemySpawnManagerEditor : Editor
{
    [MenuItem("GameObject/Gameplay/Enemy Spawn Manager", false, 10)]
    public static void CreateEnemySpawnManager(MenuCommand menuCommand)
    {
        GameObject go = new GameObject("EnemySpawnManager");
        go.AddComponent<EnemySpawnManager>();
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, "Create EnemySpawnManager");
        Selection.activeObject = go;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnemySpawnManager manager = (EnemySpawnManager)target;

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Công Cụ Quản Lý & Thiết Kế Map", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        System.Type currentMapSpawnType = GetSpawnPointTypeForScene();
        string mapTypeName = GetMapFriendlyName(currentMapSpawnType);

        EditorGUILayout.LabelField($"🗺️ Map hiện tại: {mapTypeName}", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"📊 Tổng số điểm spawn: {manager.TotalPointsCount}");

        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField($"🟢 Quái đang sống: {manager.GetActiveEnemyCount()}");
            EditorGUILayout.LabelField($"⏳ Đang chờ hồi sinh: {manager.GetRespawningCount()}");
        }

        EditorGUILayout.Space(6);

        // Nút thêm điểm spawn con tương ứng với map hiện tại
        GUI.backgroundColor = new Color(0.3f, 0.85f, 0.4f);
        string buttonLabel = $"➕ Thêm Điểm Spawn ({mapTypeName})";
        if (GUILayout.Button(buttonLabel, GUILayout.Height(32)))
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName($"Add Spawn Point for {mapTypeName}");

            Vector3 spawnPos = manager.transform.position;
            if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null)
            {
                Vector3 camPos = SceneView.lastActiveSceneView.camera.transform.position;
                spawnPos = new Vector3(camPos.x, camPos.y, 0f);
            }

            GameObject newObj = new GameObject($"SpawnPoint_{manager.TotalPointsCount + 1}");
            Undo.RegisterCreatedObjectUndo(newObj, "Create Spawn Point Object");
            Undo.SetTransformParent(newObj.transform, manager.transform, "Parent to Manager");
            newObj.transform.position = spawnPos;

            // Thêm đúng component SpawnPoint của map đó (tự động load sẵn các prefab quái tương ứng)
            Undo.AddComponent(newObj, currentMapSpawnType);
            manager.RefreshSpawnPoints();

            Selection.activeGameObject = newObj;
            EditorUtility.SetDirty(manager);
        }

        // Nút Quét Lại
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("🔄 Quét Lại Tất Cả Điểm Spawn Con", GUILayout.Height(24)))
        {
            Undo.RecordObject(manager, "Refresh Spawn Points");
            manager.RefreshSpawnPoints();
            EditorUtility.SetDirty(manager);
        }

        // Các nút trong Play Mode
        if (Application.isPlaying)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Điều Khiển Khi Đang Chạy Game (Play Mode):", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
            if (GUILayout.Button("⚔️ Sinh Tất Cả Quái"))
            {
                manager.SpawnAllEnemies();
            }

            GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f);
            if (GUILayout.Button("🧹 Xoá Tất Cả Quái"))
            {
                manager.ClearAllEnemies();
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
        GUI.backgroundColor = Color.white;
    }

    private static System.Type GetSpawnPointTypeForScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.Contains("Forest")) return typeof(EnemySpawnPoint_ForestGreen);
        if (sceneName.Contains("Swamp")) return typeof(EnemySpawnPoint_Swamp);
        if (sceneName.Contains("Caves")) return typeof(EnemySpawnPoint_Caves);
        if (sceneName.Contains("Castle") || sceneName.Contains("Dungeon")) return typeof(EnemySpawnPoint_CastleDungeon);
        if (sceneName.Contains("Inferno")) return typeof(EnemySpawnPoint_Inferno);
        if (sceneName.Contains("Wasteland")) return typeof(EnemySpawnPoint_Wasteland);
        if (sceneName.Contains("Ruins") || sceneName.Contains("Metroidvania")) return typeof(EnemySpawnPoint_MetroidvaniaRuins);

        return typeof(EnemySpawnPoint);
    }

    private static string GetMapFriendlyName(System.Type type)
    {
        if (type == typeof(EnemySpawnPoint_ForestGreen)) return "2_Forest Green";
        if (type == typeof(EnemySpawnPoint_Swamp)) return "3_Swamp";
        if (type == typeof(EnemySpawnPoint_Caves)) return "4_Caves";
        if (type == typeof(EnemySpawnPoint_CastleDungeon)) return "5_Castle Dungeon";
        if (type == typeof(EnemySpawnPoint_Inferno)) return "6_Inferno Underworld";
        if (type == typeof(EnemySpawnPoint_Wasteland)) return "7_Wasteland";
        if (type == typeof(EnemySpawnPoint_MetroidvaniaRuins)) return "8_Metroidvania Ruins";

        return "Generic (Cơ bản)";
    }
}
#endif
