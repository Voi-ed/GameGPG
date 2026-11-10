using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Video;


public class GameTimeline : MonoBehaviour
{
    public string sceneName;
    public VideoPlayer videoPlayer = null;
    //public TextMeshProUGUI skipText = null;
    private float gameTimer = 0;

    IEnumerator PlayVideo()
    {
        //load and prepare the video for playing
        bool videoLoadedFailed = false;
        videoPlayer.Prepare();

        //keep looping until the video is prepared and ready to play
        while (!videoPlayer.isPrepared)
        {
            //if more than 11 second passed then video loading failed and skip it to scene loading
            if (Time.time > gameTimer + 11.0f)
            {
                //video couldnt be prepared
                videoLoadedFailed = true;
                break;
            }

            //to avoid get crashed
            yield return null;
        }

        //if video prepared lets play it
        if (!videoLoadedFailed)
        {
            //start the video
            videoPlayer.Play();
            Time.timeScale = 0;
        }

        //while its playing test for key presses to skip the video
        gameTimer = Time.time;
        while (videoPlayer.isPlaying)
        {
            if (Time.time > gameTimer + 0.6f)
            {
                gameTimer = Time.time;
            }

            yield return null;
        }

        //Redisable vSync
        QualitySettings.vSyncCount = 0;
        //load the level name
        AsyncOperation sceneLoader = SceneManager.LoadSceneAsync(sceneName);
        Time.timeScale = 1;
        //dont automatic activate it when it loaded ,we want to do it manually
        sceneLoader.allowSceneActivation = false;
        //activate the loaded scene
        sceneLoader.allowSceneActivation = true;
        gameObject.SetActive(false);

    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //turn on the Vcencount just for video player
            QualitySettings.vSyncCount = 1;

            //begin time
            gameTimer = Time.time;

            //play the video
            StartCoroutine(PlayVideo());
        }
    }
}
