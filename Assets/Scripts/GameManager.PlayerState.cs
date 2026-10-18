using System.Collections.Generic;
using UnityEngine;

public partial class GameManager
{
    private float savedPlayerHealth = -1f;
    private Dictionary<SkillType, float> savedSkillCooldowns = new Dictionary<SkillType, float>();

    public void ResetTransitionState()
    {
        savedPlayerHealth = -1f;
        savedSkillCooldowns.Clear();
    }

    private void CapturePlayerState()
    {
        Player player = Player.instance != null ? Player.instance : FindFirstObjectByType<Player>();
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

        // 1. Áp dụng lại lượng máu đã lưu
        if (savedPlayerHealth > 0 && targetPlayer.health != null)
        {
            targetPlayer.health.SetCurrentHealth(savedPlayerHealth);
        }

        // 2. Áp dụng lại thời gian hồi chiêu của từng skill
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
