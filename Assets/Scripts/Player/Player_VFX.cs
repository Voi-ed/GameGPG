using System.Collections;
using UnityEngine;

public class Player_VFX : Entity_VFX
{
    public GameObject CreateEffectOf(GameObject effect, Transform target, bool followTarget = false)
    {
        if (effect == null || target == null)
            return null;

        Transform parent = followTarget ? target : null;
        if (GameManager.instance != null && GameManager.instance.vfxPool != null)
        {
            return GameManager.instance.vfxPool.Get(effect, target.position, Quaternion.identity, parent);
        }

        return Instantiate(effect, target.position, Quaternion.identity, parent);
    }
}
