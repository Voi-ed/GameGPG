using UnityEngine;
using UnityEngine.SceneManagement;

public partial class GameManager : MonoBehaviour, ISaveable
{
    private static GameManager _instance;
    public static GameManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("___ GAME MANAGER (Auto) ___");
                    _instance = go.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    private VFX_PoolManager _vfxPool;
    public VFX_PoolManager vfxPool
    {
        get
        {
            if (_vfxPool == null)
            {
                _vfxPool = GetComponentInChildren<VFX_PoolManager>();
                if (_vfxPool == null)
                {
                    GameObject poolObj = new GameObject("VFX_PoolManager");
                    poolObj.transform.SetParent(transform);
                    _vfxPool = poolObj.AddComponent<VFX_PoolManager>();
                }
            }
            return _vfxPool;
        }
        private set => _vfxPool = value;
    }

    private Vector3 lastPlayerPosition;
    private string lastScenePlayed;
    private bool dataLoaded;
    public bool isTransitioningScene { get; set; } = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Đảm bảo vfxPool luôn được khởi tạo sẵn khi GameManager thức dậy
        if (_vfxPool == null)
        {
            _ = vfxPool;
        }
    }

    public void ContinuePlay()
    {
        ChangeScene(lastScenePlayed, RespawnType.NonSpecific);
    }

    public void RestartScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        ChangeScene(sceneName, RespawnType.NonSpecific);
    }

    public void LoadData(GameData data)
    {
        lastScenePlayed = data.lastScenePlayed;
        lastPlayerPosition = data.lastPlayerPosition;

        if (string.IsNullOrEmpty(lastScenePlayed) ||
            lastScenePlayed == "Level_0" ||
            lastScenePlayed == "LeveL_1" ||
            lastScenePlayed == "LeveL_2" ||
            lastScenePlayed == "SampleScene" ||
            lastScenePlayed == "BattleScene" ||
            lastScenePlayed.Contains("Arena"))
        {
            lastScenePlayed = "1_Village & Camp";
            lastPlayerPosition = Vector3.zero;
        }

        dataLoaded = true;
    }

    public void SaveData(ref GameData data)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "MainMenu")
            return;

        // Nếu thoát game hoặc lưu lúc đang ở Đấu Trường (BattleScene), luôn lưu map đích là Làng
        if (currentScene == "BattleScene" || currentScene.Contains("Arena"))
        {
            data.lastScenePlayed = "1_Village & Camp";
            data.lastPlayerPosition = Vector3.zero;
            dataLoaded = false;
            return;
        }

        if (Player.instance != null)
            data.lastPlayerPosition = Player.instance.transform.position;

        data.lastScenePlayed = currentScene;
        dataLoaded = false;
    }
}
