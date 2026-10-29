using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Effect/Dragon Transformation", fileName = "Item effect data - Dragon Transformation")]
public class ItemEffect_DragonTransformation : ItemEffect_DataSO
{
    [Header("Transformation Details")]
    [Tooltip("Thời gian duy trì trạng thái hóa rồng (giây)")]
    [SerializeField] private float duration = 20f;

    public override void ExecuteEffect()
    {
        base.ExecuteEffect();

        if (GameManager.instance != null)
        {
            GameManager.instance.StartDragonTransformation(duration);
        }
        else
        {
            Player targetPlayer = player != null ? player : Player.instance;
            Player_DragonForm dragonForm = targetPlayer != null ? targetPlayer.dragonForm : Player_DragonForm.instance;
            if (dragonForm != null)
            {
                dragonForm.TransformToDragon(duration);
            }
        }

        if (AudioManager.instance != null)
            AudioManager.instance.PlayGlobalSFX("potion_drink");
    }
}
