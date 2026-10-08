using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Dash : Skill_Base
{
    [Header("Fire Trail Upgrade")]
    [SerializeField] private GameObject fireTrailPrefab;
    [SerializeField] private float fireTrailInterval = 0.06f;
    private Coroutine fireTrailCoroutine;
    private readonly List<SkillObject_FireTrail> currentDashTrails = new List<SkillObject_FireTrail>();

    [Header("Infernal Crash Upgrade")]
    [SerializeField] private GameObject fireExplosionPrefab;
    private Coroutine chainDetonationCoroutine;

    public event System.Action OnDashStart;
    public event System.Action OnDashEnd;

    public void OnStartEffect()
    {
        OnDashStart?.Invoke();
        if (Unlocked(SkillUpgradeType.Dash_CloneOnStart) || Unlocked(SkillUpgradeType.Dash_CloneOnStartAndArrival))
            CreateClone();

        if (Unlocked(SkillUpgradeType.Dash_ShardOnShart) || Unlocked(SkillUpgradeType.Dash_ShardOnStartAndArrival))
            CreateShard();

        if (Unlocked(SkillUpgradeType.Dash_FireTrail) || Unlocked(SkillUpgradeType.Dash_InfernalCrash))
            StartFireTrail();
    }

    public void OnEndEffect()
    {
        OnDashEnd?.Invoke();
        if (Unlocked(SkillUpgradeType.Dash_CloneOnStartAndArrival))
            CreateClone();

        if (Unlocked(SkillUpgradeType.Dash_ShardOnStartAndArrival))
            CreateShard();

        if (Unlocked(SkillUpgradeType.Dash_InfernalCrash))
        {
            CreateFireExplosion();
            TriggerChainDetonation();
        }

        StopFireTrail();
    }

    private void StartFireTrail()
    {
        StopFireTrail();
        currentDashTrails.Clear();
        fireTrailCoroutine = StartCoroutine(FireTrailRoutine(player.dashDuration));
    }

    private void StopFireTrail()
    {
        if (fireTrailCoroutine != null)
        {
            StopCoroutine(fireTrailCoroutine);
            fireTrailCoroutine = null;
        }
    }

    private IEnumerator FireTrailRoutine(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            CreateFireTrailPatch();
            yield return new WaitForSeconds(fireTrailInterval);
            timer += fireTrailInterval;
        }
    }

    private void CreateFireTrailPatch()
    {
        if (fireTrailPrefab == null)
            return;

        Vector3 pos = player.transform.position;
        if (player.col != null)
            pos.y = player.col.bounds.min.y;
        else
            pos.y -= 0.95f;

        GameObject trail = Instantiate(fireTrailPrefab, pos, Quaternion.identity);
        SkillObject_FireTrail trailScript = trail.GetComponent<SkillObject_FireTrail>();
        if (trailScript != null)
        {
            trailScript.SetupFireTrail(player.stats, damageScaleData);
            currentDashTrails.Add(trailScript);
        }
    }

    private void CreateFireExplosion()
    {
        if (fireExplosionPrefab == null)
            return;

        Vector3 pos = player.transform.position;
        if (player.col != null)
            pos.y = player.col.bounds.min.y;
        else
            pos.y -= 0.95f;

        GameObject explosion = Instantiate(fireExplosionPrefab, pos, Quaternion.identity);
        SkillObject_FireExplosion explosionScript = explosion.GetComponent<SkillObject_FireExplosion>();
        if (explosionScript != null)
            explosionScript.SetupFireExplosion(player.stats, damageScaleData, isMainExplosion: true);
    }

    private void TriggerChainDetonation()
    {
        if (currentDashTrails == null || currentDashTrails.Count == 0)
            return;

        List<SkillObject_FireTrail> trailsToDetonate = new List<SkillObject_FireTrail>(currentDashTrails);
        currentDashTrails.Clear();

        if (chainDetonationCoroutine != null)
            StopCoroutine(chainDetonationCoroutine);

        chainDetonationCoroutine = StartCoroutine(ChainDetonationRoutine(trailsToDetonate));
    }

    private IEnumerator ChainDetonationRoutine(List<SkillObject_FireTrail> trails)
    {
        // Delay slightly after the main slam to let the impact register before ripple kicks off
        yield return new WaitForSeconds(0.06f);

        // Ripple backwards: from the most recent trail patch back to the origin
        for (int i = trails.Count - 1; i >= 0; i--)
        {
            SkillObject_FireTrail trail = trails[i];
            if (trail != null && trail.gameObject != null)
            {
                trail.Detonate(player.stats, damageScaleData, fireExplosionPrefab);
            }
            yield return new WaitForSeconds(0.04f);
        }
    }

    private void CreateShard()
    {
        skillManager.shard.CreateRawShard();
    }

    private void CreateClone()
    {
        skillManager.timeEcho.CreateTimeEcho();
    }
}
