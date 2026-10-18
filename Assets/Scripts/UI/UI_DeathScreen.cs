using UnityEngine;

public class UI_DeathScreen : MonoBehaviour
{
    public void GoToCampBTN()
    {
        GameManager.instance.ChangeScene("1_Village & Camp", RespawnType.NonSpecific);
    }

    public void GoToCheckpointBTN()
    {
        GameManager.instance.RestartScene();
    }

    public void GoToMainMenuBTN()
    {
        GameManager.instance.ChangeScene("MainMenu", RespawnType.NonSpecific);
    }
}
