using UnityEngine;

public enum MonsterRank
{
    Normal,
    Elite,
    Boss
}

[CreateAssetMenu(fileName = "MonsterData - ", menuName = "RPG Setup/Codex/Monster Data")]
public class Codex_MonsterDataSO : ScriptableObject
{
    [Header("Basic Information")]
    public string monsterId;
    public string monsterName;
    public Sprite monsterIcon;
    public GameObject monsterPrefab;
    public MonsterRank monsterRank = MonsterRank.Normal;

    [Header("Stats & Region")]
    public int baseHealth = 100;
    public string mapLocation = "Level 0";
    [TextArea(2, 4)]
    public string description = "Quái vật nguy hiểm.";

    [Header("Drop Table")]
    public ItemListDataSO dropData;
}

