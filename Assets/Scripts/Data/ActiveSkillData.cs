using UnityEngine;

[CreateAssetMenu(fileName = "NewActiveSkillData", menuName = "Scriptable Objects/ActiveSkillData")]
public class ActiveSkillData : ScriptableObject
{
    [Header("Skill Info")]
    [SerializeField] private string skillName;
    [TextArea] 
    [SerializeField] private string description;

    [Header("Shop Settings")]
    [SerializeField] private int unlockCost = 100;
    [SerializeField] private int refillCost = 10;

    public string SkillName => skillName;
    public string Description => description;
    public int UnlockCost => unlockCost;
    public int RefillCost => refillCost;

    public int GetCurrentCost(bool isUnlocked)
    {
        return isUnlocked ? refillCost : unlockCost;
    }
}