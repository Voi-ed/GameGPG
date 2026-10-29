using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Audio Database")]
public class AudioDatabaseSO : ScriptableObject
{
    public List<AudioClipData> player;
    public List<AudioClipData> uiAudio;

    [Header("Music Lists")]
    public List<AudioClipData> mainMenuMusic;
    public List<AudioClipData> levelMusic;


    [Header("SFX & Jingles")]
    public List<AudioClipData> sfx;
    public List<AudioClipData> jingles;

    private Dictionary<string, AudioClipData> clipCollection;

    private void OnEnable()
    {
        clipCollection = new Dictionary<string, AudioClipData>();

        AddToCollection(player);
        AddToCollection(uiAudio);
        AddToCollection(mainMenuMusic);
        AddToCollection(levelMusic);
        AddToCollection(sfx);
        AddToCollection(jingles);
    }

    public AudioClipData Get(string groupName)
    {
        if (clipCollection == null)
            OnEnable();

        return clipCollection != null && clipCollection.TryGetValue(groupName, out var data) ? data : null;
    }

    private void AddToCollection(List<AudioClipData> listToAdd)
    {
        if (listToAdd == null)
            return;

        foreach (var data in listToAdd)
        {
            if (data != null && string.IsNullOrEmpty(data.audioName) == false && clipCollection.ContainsKey(data.audioName) == false)
            {
                clipCollection.Add(data.audioName, data);
            }
        }
    }
}

[System.Serializable]
public class AudioClipData
{
    public string audioName;
    public List<AudioClip> clips = new List<AudioClip>();
    [Range(0f, 1f)] public float maxVolume = 1f;

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Count == 0)
            return null;
        

        return clips[Random.Range(0,clips.Count)];
    }
}