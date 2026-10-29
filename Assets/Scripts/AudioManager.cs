using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioDatabaseSO audioDB;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [Space]
    private Transform player;

    private AudioClip lastMusicPlayed;
    private string currentBgmGroupName;
    private Coroutine currentBgmCo;
    [SerializeField] private bool bgmShouldPlay;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }


        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (bgmSource.isPlaying == false && bgmShouldPlay)
        {
            if (string.IsNullOrEmpty(currentBgmGroupName) == false)
                NextBGM(currentBgmGroupName);
        }

        if (bgmSource.isPlaying && bgmShouldPlay == false)
            StopBGM();
    }

    public void StartBGM(string musicGroup)
    {
        bgmShouldPlay = true;

        if (musicGroup == currentBgmGroupName)
            return;

        NextBGM(musicGroup);
    }

    public void NextBGM(string musicGroup)
    {
        bgmShouldPlay = true;
        currentBgmGroupName = musicGroup;

        if (currentBgmCo != null)
            StopCoroutine(currentBgmCo);

        currentBgmCo = StartCoroutine(SwitchMusicCo(musicGroup));
    }

    public void StopBGM()
    {
        bgmShouldPlay = false;

        StartCoroutine(FadeVolumeCo(bgmSource, 0, 1));

        if (currentBgmCo != null)
            StopCoroutine(currentBgmCo);
    }

    private IEnumerator SwitchMusicCo(string musicGroup)
    {
        if (audioDB == null)
            yield break;

        AudioClipData data = audioDB.Get(musicGroup);

        if (data == null || data.clips == null || data.clips.Count == 0)
        {
            Debug.Log("No audio found for group " + musicGroup);
            yield break;
        }

        AudioClip nextMusic = data.GetRandomClip();

        if (data.clips.Count > 1)
        {
            while (nextMusic == lastMusicPlayed)
                nextMusic = data.GetRandomClip();
        }

        if (bgmSource.isPlaying)
            yield return FadeVolumeCo(bgmSource, 0, 1f);


        lastMusicPlayed = nextMusic;
        bgmSource.clip = nextMusic;
        bgmSource.volume = 0;
        bgmSource.Play();

        StartCoroutine(FadeVolumeCo(bgmSource, data.maxVolume, 1f));
    }

    private IEnumerator FadeVolumeCo(AudioSource source, float targetVolume, float duration)
    {
        float time = 0;
        float startVolume = source.volume;

        while (time < duration)
        {
            time += Time.deltaTime;

            source.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    public void PlaySFX(string soundName, AudioSource source, float minDistanceToHearSound = 5)
    {
        if (audioDB == null || source == null)
            return;

        var data = audioDB.Get(soundName);
        if (data == null)
            return;

        var clip = data.GetRandomClip();
        if (clip == null) return;

        if (player == null && Player.instance != null)
            player = Player.instance.transform;

        float maxVolume = data.maxVolume;
        float volume = maxVolume;

        if (player != null)
        {
            float distance = Vector2.Distance(source.transform.position, player.position);
            float t = Mathf.Clamp01(1 - (distance / minDistanceToHearSound));
            volume = Mathf.Lerp(0, maxVolume, t * t); // exponential falloff
        }

        source.pitch = Random.Range(.98f, 1.02f);
        source.volume = volume;
        source.PlayOneShot(clip);
    }

    public void PlayGlobalSFX(string soundName)
    {
        if (audioDB == null || sfxSource == null)
            return;

        var data = audioDB.Get(soundName);
        if (data == null) return;

        var clip = data.GetRandomClip();
        if (clip == null) return;

        sfxSource.pitch = Random.Range(.98f, 1.02f);
        sfxSource.volume = data.maxVolume;
        sfxSource.PlayOneShot(clip);
    }
}
