using UnityEngine;

public class Enemy_VFX : Entity_VFX
{
    [Header("Counter Attack Window")]
    [SerializeField] private GameObject attackAlert;

    [Header("Stun VFX")]
    [SerializeField] private GameObject stunVfxPrefab;
    private GameObject currentStunVfx;

    public void EnableAttackAlert(bool enable)
    {
        if (attackAlert == null)
            return;

        attackAlert.SetActive(enable);
    }

    public void PlayStunVfx(bool play)
    {
        if (play)
        {
            if (stunVfxPrefab != null && currentStunVfx == null)
            {
                currentStunVfx = Instantiate(stunVfxPrefab, transform.position, Quaternion.identity, transform);
            }
        }
        else
        {
            if (currentStunVfx != null)
            {
                Destroy(currentStunVfx);
                currentStunVfx = null;
            }
        }
    }

    private void OnDisable()
    {
        if (currentStunVfx != null)
        {
            Destroy(currentStunVfx);
            currentStunVfx = null;
        }
    }
}
