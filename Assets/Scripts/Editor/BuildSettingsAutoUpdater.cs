#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class BuildSettingsAutoUpdater
{
    static BuildSettingsAutoUpdater()
    {
        EditorApplication.delayCall += RefreshBuildSettings;
    }

    [MenuItem("Tools/GameGPG/Refresh Build Settings")]
    public static void RefreshBuildSettings()
    {
        string[] targetScenes = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Map/1_Village & Camp.unity",
            "Assets/Scenes/Map/2_Forest Green.unity",
            "Assets/Scenes/Map/3_Swamp.unity",
            "Assets/Scenes/Map/4_Caves.unity",
            "Assets/Scenes/Map/5_Castle Dungeon.unity",
            "Assets/Scenes/Map/6_Inferno Underworld.unity",
            "Assets/Scenes/Map/7_Wasteland.unity",
            "Assets/Scenes/Map/8_Metroidvania Ruins.unity",
            "Assets/Scenes/Level_0.unity",
            "Assets/Scenes/LeveL_1.unity",
            "Assets/Scenes/LeveL_2.unity",
            "Assets/Scenes/SampleScene.unity"
        };

        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        foreach (string scenePath in targetScenes)
        {
            if (File.Exists(scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"[BuildSettingsAutoUpdater] Successfully registered {scenes.Count} scenes into Unity Build Settings!");
    }
}
#endif
