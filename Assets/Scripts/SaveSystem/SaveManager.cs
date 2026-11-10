using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;
    public static SaveManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SaveManager>();
            }
            return _instance;
        }
        private set => _instance = value;
    }

    private FileDataHandler dataHandler;
    private GameData gameData;
    private List<ISaveable> allSaveables;

    [SerializeField] private string fileName = "unityalexdev.json";
    [SerializeField] private bool encryptData = true;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        StartCoroutine(LoadDataOnSceneLoadedCo());
    }

    private IEnumerator LoadDataOnSceneLoadedCo()
    {
        // Chờ 1 frame để tất cả GameObject trong scene mới chạy xong Awake() và Start()
        yield return null;

        allSaveables = FindISaveables();
        if (gameData != null)
        {
            foreach (var saveable in allSaveables)
                saveable?.LoadData(gameData);
        }
    }

    private IEnumerator Start()
    {
        Debug.Log(Application.persistentDataPath);
        if (dataHandler == null)
            dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, encryptData);
        allSaveables = FindISaveables();

        yield return null;
        LoadGame();
    }

    private void LoadGame()
    {
        if (dataHandler == null)
            dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, encryptData);

        gameData = dataHandler.LoadData();

        if (gameData == null)
        {
            Debug.Log("No save data found, creating new save!");
            gameData = new GameData();
        }

        allSaveables = FindISaveables();
        foreach (var saveable in allSaveables)
            saveable.LoadData(gameData);
    }

    public void SaveGame()
    {
        if (dataHandler == null)
            dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, encryptData);

        if (gameData == null)
            gameData = new GameData();

        allSaveables = FindISaveables();

        foreach (var saveable in allSaveables)
            saveable?.SaveData(ref gameData);

        dataHandler.SaveData(gameData);
    }

    public GameData GetGameData()
    {
        if (gameData == null)
        {
            if (dataHandler == null)
                dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, encryptData);
            gameData = dataHandler.LoadData() ?? new GameData();
        }
        return gameData;
    }

    [ContextMenu("*** Delete save data ***")]
    public void DeleteSaveData()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, encryptData);
        dataHandler.Delete();

        LoadGame();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && gameData != null)
            SaveGame();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && gameData != null)
            SaveGame();
    }

    private void OnApplicationQuit()
    {
        if (gameData != null)
            SaveGame();
    }

    private List<ISaveable> FindISaveables()
    {
        return 
            FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<ISaveable>()
            .ToList();
    }
}
