using System.Collections;
using UnityEngine;


public class Object_Buff : MonoBehaviour
{
    private Player_Stats statsToModify;

    [Header("Buff details")]
    [SerializeField] private BuffEffectData[] buffs;
    [SerializeField] private string buffName;
    [SerializeField] private float buffDuration = 4;
    [SerializeField] private GameObject buffVfxPrefab;


    [Header("Floaty movement")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatRange = .1f;
    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
        transform.position = startPosition + new Vector3(0, yOffset);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        statsToModify = collision.GetComponent<Player_Stats>();


        if (statsToModify != null && statsToModify.CanApplyBuffOf(buffName))
        {
            if (buffVfxPrefab != null)
            {
                Player player = collision.GetComponent<Player>();
                player?.vfx?.CreateEffectOf(buffVfxPrefab, player.transform, true);
            }

            statsToModify.ApplyBuff(buffs, buffDuration, buffName);
            Destroy(gameObject);
        }
    }
}
