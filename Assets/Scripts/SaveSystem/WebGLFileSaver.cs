using System.Runtime.InteropServices;
using UnityEngine;

public static class WebGLFileSaver
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SyncFilesystemToIndexedDB();
#endif

    public static void SyncFilesystem()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            SyncFilesystemToIndexedDB();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[WebGLFileSaver] FS.syncfs exception: " + ex.Message);
        }
#endif
    }
}

