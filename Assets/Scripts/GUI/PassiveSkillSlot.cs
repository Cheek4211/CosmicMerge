using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PassiveSkillSlot : MonoBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] private PassiveSkillId skillId;
    [SerializeField] private PassiveSkillData skillData;

    private TextMeshProUGUI nameText;
    private TextMeshProUGUI explainText;
    private TextMeshProUGUI levelText;
    private Button levelUpBtn;

    public event System.Action OnUpgraded;

    private void Awake()
    {
        nameText   = transform.Find("SkillId").GetComponent<TextMeshProUGUI>();
        levelText  = transform.Find("SkillLevel").GetComponent<TextMeshProUGUI>();
        explainText = transform.Find("SkillExplain").GetComponent<TextMeshProUGUI>();
        levelUpBtn = transform.Find("LevelUp").GetComponent<Button>();

        levelUpBtn.onClick.AddListener(OnClickBuy);
    }

    public void UpdateSlotUI()
    {
        if (skillData == null) return;

        nameText.text = skillData.SkillName;
        explainText.text = skillData.Description;

        int currentLevel = GetCurrentLevel();

        if (currentLevel >= skillData.MaxLevel)
        {
            levelText.text = "MAX";
            levelUpBtn.interactable = false;
        }
        else
        {
            levelText.text = $"Lv.{currentLevel} → {currentLevel + 1}";
            levelUpBtn.interactable = true;
        }
    }

    private void OnClickBuy()
    {
        if (GetCurrentLevel() >= skillData.MaxLevel) return;

        UpgradeManager.Instance.UpgradePassive(skillId);
        OnUpgraded?.Invoke();
    }

    private int GetCurrentLevel()
    {
        switch (skillId)
        {
            case PassiveSkillId.Engine:   return UpgradeManager.Instance.EnginePowerLevel;
            case PassiveSkillId.Universe: return UpgradeManager.Instance.UniverseSizeLevel;
            case PassiveSkillId.Tech:     return UpgradeManager.Instance.TechLevel;
            default:                      return 1;
        }
    }
}