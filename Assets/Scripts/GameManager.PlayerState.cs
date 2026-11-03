using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class GameManager
{
    private float savedPlayerHealth = -1f;
    private Dictionary<SkillType, float> savedSkillCooldowns = new Dictionary<SkillType, float>();
    private List<string> savedQuickItemSaveIds = new List<string>();

    public void ResetTransitionState()
    {
        savedPlayerHealth = -1f;
        savedSkillCooldowns.Clear();
    }

    private void CapturePlayerState()
    {
        Player player = Player.instance != null ? Player.instance : FindFirstObjectByType<Player>();
        if (player != null && player.inventory != null && player.inventory.quickItems != null)
        {
            savedQuickItemSaveIds.Clear();
            for (int i = 0; i < player.inventory.quickItems.Length; i++)
            {
                var qItem = player.inventory.quickItems[i];
                savedQuickItemSaveIds.Add(qItem?.itemData != null ? qItem.itemData.saveId : string.Empty);
            }
        }

        string currentScene = SceneManager.GetActiveScene().name;

        // Nếu rời khỏi Đấu Trường Boss (BattleScene / Arena) để về làng hoặc nơi khác,
        // xóa dữ liệu máu bị trừ để khi đến scene mới người chơi được hồi full máu.
        if (currentScene == "BattleScene" || currentScene.Contains("Arena"))
        {
            ResetTransitionState();
            return;
        }

        if (player == null)
            return;

        if (player.health != null)
            savedPlayerHealth = player.health.GetCurrentHealth();

        savedSkillCooldowns.Clear();
        if (player.skillManager != null && player.skillManager.allSkills != null)
        {
            foreach (var skill in player.skillManager.allSkills)
            {
                if (skill == null)
                    continue;

                float remaining = skill.GetRemainingCooldown();
                if (remaining > 0)
                {
                    savedSkillCooldowns[skill.GetSkillType()] = remaining;
                }
            }
        }
    }

    private void ApplyPlayerState(Player targetPlayer)
    {
        if (targetPlayer == null)
            return;

        // Bàn giao Quick Items sang Player scene mới
        if (targetPlayer.inventory != null && savedQuickItemSaveIds.Count > 0 && targetPlayer.inventory.quickItems != null)
        {
            for (int i = 0; i < savedQuickItemSaveIds.Count && i < targetPlayer.inventory.quickItems.Length; i++)
            {
                string saveId = savedQuickItemSaveIds[i];
                if (!string.IsNullOrEmpty(saveId))
                {
                    targetPlayer.inventory.quickItems[i] = targetPlayer.inventory.itemList.Find(
                        it => it != null && it.itemData != null && it.itemData.saveId == saveId);
                }
            }
            targetPlayer.inventory.TriggerUpdateUI();
        }

        string currentScene = SceneManager.GetActiveScene().name;

        // 1. Nếu scene đích vừa nạp vào là Đấu Trường Boss: hồi 100% full máu và xoá hồi chiêu skill
        if (currentScene == "BattleScene" || currentScene.Contains("Arena"))
        {
            targetPlayer.health?.SetHealthToPercent(1f);
            savedPlayerHealth = -1f;
            savedSkillCooldowns.Clear();
            return;
        }

        // 2. Áp dụng lại lượng máu đã lưu
        if (savedPlayerHealth > 0 && targetPlayer.health != null)
        {
            targetPlayer.health.SetCurrentHealth(savedPlayerHealth);
        }
        else if (targetPlayer.health != null)
        {
            // Nếu không có máu đã lưu (ví dụ vừa đánh Boss xong về Làng, hoặc mới bắt đầu game), hồi full máu
            targetPlayer.health.SetHealthToPercent(1f);
        }

        // 3. Áp dụng lại thời gian hồi chiêu của từng skill
        if (savedSkillCooldowns.Count > 0 && targetPlayer.skillManager != null)
        {
            foreach (var kvp in savedSkillCooldowns)
            {
                var skill = targetPlayer.skillManager.GetSkillByType(kvp.Key);
                if (skill != null)
                {
                    skill.RestoreCooldown(kvp.Value);
                }
            }
        }
    }
}
