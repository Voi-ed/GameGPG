using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private string musicGroupName;

    private void Start()
    {
        if (AudioManager.instance == null)
            return;

        string targetBgm = musicGroupName;

        if (string.IsNullOrEmpty(targetBgm) || targetBgm == "playlist_levels")
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName.Contains("1_Village") || sceneName.Contains("Camp"))
                targetBgm = "bgm_village";
            else if (sceneName.Contains("2_Forest"))
                targetBgm = "bgm_forest";
            else if (sceneName.Contains("3_Swamp"))
                targetBgm = "bgm_swamp";
            else if (sceneName.Contains("4_Caves"))
                targetBgm = "bgm_caves";
            else if (sceneName.Contains("5_Castle"))
                targetBgm = "bgm_castle";
            else if (sceneName.Contains("6_Inferno"))
                targetBgm = "bgm_inferno";
            else if (sceneName.Contains("7_Wasteland"))
                targetBgm = "bgm_wasteland";
            else if (sceneName.Contains("8_Metroidvania") || sceneName.Contains("Ruins"))
                targetBgm = "bgm_ruins";
            else if (sceneName.Contains("BattleScene") || sceneName.Contains("Arena"))
                targetBgm = "bgm_boss";
            else if (string.IsNullOrEmpty(targetBgm))
                targetBgm = "playlist_levels";
        }

        AudioManager.instance.StartBGM(targetBgm);
    }
}
