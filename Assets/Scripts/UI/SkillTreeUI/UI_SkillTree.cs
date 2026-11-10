using System.Linq;
using TMPro;
using UnityEngine;

public class UI_SkillTree : MonoBehaviour, ISaveable
{
    [SerializeField] private int skillPoints;
    [SerializeField] private TextMeshProUGUI skillPointsText;
    [SerializeField] private UI_TreeConnectHandler[] parentNodes;
    private UI_TreeNode[] allTreeNodes;
    public Player_SkillManager skillManager { get ; private set; }


    private void Start()
    {
        UpdateAllConnections();
        UpdateSkillPointsUI();
    }

    private void UpdateSkillPointsUI()
    {
        skillPointsText.text = skillPoints.ToString();
    }

    public void UnlockDefaultSkills()
    {
        allTreeNodes = GetComponentsInChildren<UI_TreeNode>(true);
        skillManager = FindAnyObjectByType<Player_SkillManager>();


        foreach (var node in allTreeNodes)
            node.UnlockDefaultSkill();
    }

    [ContextMenu("Reset Skill Tree")]
    public void RefundAllSkills()
    {
        UI_TreeNode[] skillNodes = GetComponentsInChildren<UI_TreeNode>();

        foreach (var node in skillNodes)
            node.Refund();
    }

    public bool EnoughSkillPoints(int cost) => skillPoints >= cost;
    public void RemoveSkillPoints(int cost)
    {
        skillPoints = skillPoints - cost;
        UpdateSkillPointsUI();
    }
    public void AddSkillPoints(int points)
    {
        skillPoints = skillPoints + points;
        UpdateSkillPointsUI();
    }



    [ContextMenu("Update All Connections")]
    public void UpdateAllConnections()
    {
        foreach (var node in parentNodes)
        {
            node.UpdateAllConnections();
        }
    }

    public void LoadData(GameData data)
    {
        skillPoints = data.skillPoints;

        if (allTreeNodes == null)
            allTreeNodes = GetComponentsInChildren<UI_TreeNode>(true);

        if (skillManager == null)
            skillManager = FindAnyObjectByType<Player_SkillManager>();

        if (allTreeNodes != null)
        {
            foreach (var node in allTreeNodes)
            {
                if (node == null || node.skillData == null) continue;
                string skillName = node.skillData.displayName;

                if (data.skillTreeUI != null && data.skillTreeUI.TryGetValue(skillName, out bool unlocked) && unlocked)
                    node.UnlockWithSaveData();
            }
        }

        if (skillManager != null && skillManager.allSkills != null && allTreeNodes != null)
        {
            foreach (var skill in skillManager.allSkills)
            {
                if (skill == null) continue;
                if (data.skillUpgrades != null && data.skillUpgrades.TryGetValue(skill.GetSkillType(), out SkillUpgradeType upgradeType))
                {
                    var upgradeNode = allTreeNodes.FirstOrDefault(node => node != null && node.skillData != null && node.skillData.upgradeData != null && node.skillData.upgradeData.upgradeType == upgradeType);

                    if (upgradeNode != null)
                        skill.SetSkillUpgrade(upgradeNode.skillData);
                }
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        data.skillPoints = skillPoints;
        data.skillTreeUI.Clear();
        data.skillUpgrades.Clear();

        foreach (var node in allTreeNodes)
        {
            string skillName = node.skillData.displayName;
            data.skillTreeUI[skillName] = node.isUnlocked;
        }


        foreach (var skill in skillManager.allSkills)
        {
            data.skillUpgrades[skill.GetSkillType()] = skill.GetUpgrade();
        }
    }
}
