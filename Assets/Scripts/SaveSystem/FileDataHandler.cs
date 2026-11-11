using System;
using System.ComponentModel;
using System.IO;
using UnityEngine;

public class FileDataHandler
{
    private string fullPath;
    private string dataFileName;
    private bool encrpyData;
    private string codeWord = "unitydev.com";

    public FileDataHandler(string dataDirPath, string dataFileName, bool encryptData)
    {
        this.dataFileName = dataFileName;
        fullPath = Path.Combine(dataDirPath, dataFileName);
        this.encrpyData = encryptData;
    }

    public void SaveData(GameData gameData)
    {
        try
        {
            // 1. Convert GameData to JSON string
            string dataToSave = JsonUtility.ToJson(gameData, true);

            if (encrpyData)
                dataToSave = EncryptDecrypt(dataToSave);

#if UNITY_WEBGL && !UNITY_EDITOR
            // Trên WebGL: Lưu qua PlayerPrefs (tự động đồng bộ IndexedDB an toàn tuyệt đối)
            try
            {
                PlayerPrefs.SetString("SaveData_" + dataFileName, dataToSave);
                PlayerPrefs.Save();
            }
            catch (Exception webEx)
            {
                Debug.LogWarning("[FileDataHandler] PlayerPrefs WebGL save warning: " + webEx);
            }
#endif

            // 2. Ghi file vào persistentDataPath (PC/Mac hoặc file system ảo IDBFS của WebGL)
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter write = new StreamWriter(stream))
                {
                    write.Write(dataToSave);
                }
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            // Đồng bộ virtual filesystem của WebGL xuống IndexedDB
            WebGLFileSaver.SyncFilesystem();
#endif
        }
        catch (Exception e)
        {
            Debug.LogError("Error on trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    public GameData LoadData()
    {
        GameData loadData = null;

#if UNITY_WEBGL && !UNITY_EDITOR
        // Trên WebGL: Ưu tiên tải từ PlayerPrefs nếu có
        if (PlayerPrefs.HasKey("SaveData_" + dataFileName))
        {
            try
            {
                string webData = PlayerPrefs.GetString("SaveData_" + dataFileName);
                if (encrpyData)
                    webData = EncryptDecrypt(webData);

                loadData = JsonUtility.FromJson<GameData>(webData);
                if (loadData != null)
                {
                    Debug.Log("[FileDataHandler] Loaded successfully from PlayerPrefs (WebGL)");
                    return loadData;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[FileDataHandler] Error loading from PlayerPrefs, fallback to file: " + ex);
            }
        }
#endif

        // Kiểm tra file lưu truyền thống
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";

                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                if (encrpyData)
                    dataToLoad = EncryptDecrypt(dataToLoad);

                loadData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error on trying to load data from file: " + fullPath + "\n" + e);
            }
        }

        return loadData;
    }

    public void Delete()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (PlayerPrefs.HasKey("SaveData_" + dataFileName))
        {
            PlayerPrefs.DeleteKey("SaveData_" + dataFileName);
            PlayerPrefs.Save();
        }
#endif
        if (File.Exists(fullPath))
            File.Delete(fullPath);

#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLFileSaver.SyncFilesystem();
#endif
    }

    private string EncryptDecrypt(string data)
    {
        string modifedData = "";

        for (int i = 0; i < data.Length; i++)
        {
            modifedData += (char)(data[i] ^ codeWord[i % codeWord.Length]);
        }

        return modifedData;
    }
}
