using UnityEngine;

[CreateAssetMenu(fileName = "PassiveSkillData", menuName = "Scriptable Objects/PassiveSkillData")]
public class PassiveSkillData : ScriptableObject
{
    [Header("Skill Info")]
    [SerializeField] private string skillName;
    [TextArea] 
    [SerializeField] private string description;

    [Header("Upgrade Settings")]
    [SerializeField] private int maxLevel = 5;
    [SerializeField] private int baseCost = 1;
    [SerializeField] private int costMultiplier = 2;

    public string SkillName => skillName;
    public string Description => description;
    public int MaxLevel => maxLevel;

    public int GetCostForNextLevel(int currentLevel)
    {
        if (currentLevel >= maxLevel) return 9999;
        return baseCost + (currentLevel - 1) * costMultiplier; 
    }
}