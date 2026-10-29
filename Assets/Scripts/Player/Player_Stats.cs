using System;
using System.Collections.Generic;
using UnityEngine;

public class Player_Stats : Entity_Stats
{
    private List<string> activeBuff = new List<string>();
    private List<ActiveBuffInfo> activeBuffList = new List<ActiveBuffInfo>();
    private Inventory_Player inventory;

    public event Action<ActiveBuffInfo> OnBuffAdded;
    public event Action<string> OnBuffRemoved;
    public event Action OnBuffListChanged;
    public event Action OnBuffsTick;

    protected override void Awake()
    {
        base.Awake();
        inventory = GetComponent<Inventory_Player>();
    }

    protected virtual void Update()
    {
        if (activeBuffList.Count == 0)
            return;

        bool listChanged = false;
        for (int i = activeBuffList.Count - 1; i >= 0; i--)
        {
            var buff = activeBuffList[i];
            buff.remainingTime -= Time.deltaTime;

            if (buff.remainingTime <= 0)
            {
                RemoveBuffInternal(buff);
                listChanged = true;
            }
        }

        if (listChanged)
        {
            inventory?.TriggerUpdateUI();
            OnBuffListChanged?.Invoke();
        }
        else
        {
            OnBuffsTick?.Invoke();
        }
    }

    public bool CanApplyBuffOf(string source)
    {
        return activeBuff.Contains(source) == false;
    }

    public void ApplyBuff(BuffEffectData[] buffsToApply, float duration, string source)
    {
        ApplyBuff(buffsToApply, duration, source, null, source, "");
    }

    public void ApplyBuff(BuffEffectData[] buffsToApply, float duration, string source, Sprite icon, string buffName = "", string description = "")
    {
        if (CanApplyBuffOf(source) == false)
            return;

        if (buffsToApply == null || buffsToApply.Length == 0)
            return;

        activeBuff.Add(source);

        foreach (var buff in buffsToApply)
        {
            GetStatByType(buff.type).AddModifier(buff.value, source);
        }

        string displayName = string.IsNullOrEmpty(buffName) ? source : buffName;
        ActiveBuffInfo buffInfo = new ActiveBuffInfo(source, displayName, icon, duration, buffsToApply, description);
        activeBuffList.Add(buffInfo);

        OnBuffAdded?.Invoke(buffInfo);
        OnBuffListChanged?.Invoke();
        inventory?.TriggerUpdateUI();
    }

    private void RemoveBuffInternal(ActiveBuffInfo buff)
    {
        if (buff == null)
            return;

        if (buff.statsModified != null)
        {
            foreach (var modifier in buff.statsModified)
            {
                GetStatByType(modifier.type).RemoveModifier(buff.sourceId);
            }
        }

        activeBuff.Remove(buff.sourceId);
        activeBuffList.Remove(buff);
        OnBuffRemoved?.Invoke(buff.sourceId);
    }

    public void RemoveBuff(string source)
    {
        ActiveBuffInfo target = activeBuffList.Find(b => b.sourceId == source);
        if (target != null)
        {
            RemoveBuffInternal(target);
            inventory?.TriggerUpdateUI();
            OnBuffListChanged?.Invoke();
        }
    }

    public IReadOnlyList<ActiveBuffInfo> GetActiveBuffs() => activeBuffList;
}
