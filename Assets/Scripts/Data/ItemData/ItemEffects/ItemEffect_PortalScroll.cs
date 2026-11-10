using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Portal Scroll", fileName = "Item effect data - PortalScroll")]
public class ItemEffect_PortalScroll : ItemEffect_DataSO
{
    public override void ExecuteEffect()
    {
        Player player = Player.instance;
        if (player == null)
            return;

        Vector3 portalPosition = player.transform.position + new Vector3(player.facingDir * 1.5f, 0);

        // 1. Kích hoạt hiệu ứng cổng trong scene nếu có Object_Portal
        if (Object_Portal.instnace != null)
        {
            Object_Portal.instnace.ActivatePortal(portalPosition, player.facingDir);
        }

        // 2. Mở bảng chọn 8 Map Teleport UI (hoặc trực tiếp dịch chuyển về Làng)
        if (UI.instance != null)
        {
            UI.instance.OpenTeleportUI(true);
        }
        else if (GameManager.instance != null)
        {
            GameManager.instance.ChangeScene("1_Village & Camp", RespawnType.Portal);
        }

        if (AudioManager.instance != null)
            AudioManager.instance.PlayGlobalSFX("portal_teleport");
    }
}
