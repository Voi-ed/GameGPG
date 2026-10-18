using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private string musicGroupName;

    private void Start()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.StartBGM(musicGroupName);
    }
}
