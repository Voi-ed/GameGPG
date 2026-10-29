using System;
using UnityEngine;

[Serializable]
public class ActiveBuffInfo
{
    public string sourceId;
    public string buffName;
    public Sprite icon;
    public float duration;
    public float remainingTime;
    public BuffEffectData[] statsModified;
    public string description;

    public float Progress => duration > 0 ? Mathf.Clamp01(remainingTime / duration) : 0f;
    public bool IsExpiringSoon => remainingTime <= 3f;

    public ActiveBuffInfo(string sourceId, string buffName, Sprite icon, float duration, BuffEffectData[] statsModified, string description = "")
    {
        this.sourceId = sourceId;
        this.buffName = buffName;
        this.icon = icon;
        this.duration = duration;
        this.remainingTime = duration;
        this.statsModified = statsModified;
        this.description = description;
    }
}
