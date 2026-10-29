#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class DamageTextPrefabCreator
{
    private const string PREFAB_PATH = "Assets/Prefab/VFX/DamageText.prefab";

    [MenuItem("Tools/GameGPG/Generate DamageText Prefab")]
    public static void GeneratePrefab()
    {
        string dir = Path.GetDirectoryName(PREFAB_PATH);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();
        }

        GameObject go = new GameObject("DamageText");

        TextMeshPro tmpro = go.AddComponent<TextMeshPro>();
        tmpro.alignment = TextAlignmentOptions.Center;
        tmpro.fontSize = 2f;
        tmpro.text = "100";
        tmpro.color = Color.white;

        // Tìm Font Asset mặc định của dự án
        TMP_FontAsset font = FindDefaultFontAsset();
        if (font != null)
        {
            tmpro.font = font;
        }

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingLayerName = "Vfx_Front";
            mr.sortingOrder = 100;
        }

        DamageTextController controller = go.AddComponent<DamageTextController>();

        // Lưu thành Prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PREFAB_PATH);
        Object.DestroyImmediate(go);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Gán tham chiếu prefab vào DamageTextManager nếu có trong scene
        DamageTextManager manager = Object.FindFirstObjectByType<DamageTextManager>();
        if (manager != null)
        {
            SerializedObject so = new SerializedObject(manager);
            SerializedProperty prop = so.FindProperty("damageTextPrefab");
            if (prop != null)
            {
                prop.objectReferenceValue = prefab;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(manager);
            }
        }

        Selection.activeObject = prefab;
        Debug.Log($"[DamageTextPrefabCreator] Created DamageText prefab successfully at: {PREFAB_PATH}");
    }

    private static TMP_FontAsset FindDefaultFontAsset()
    {
        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font != null)
                return font;
        }
        return TMP_Settings.defaultFontAsset;
    }
}
#endif
