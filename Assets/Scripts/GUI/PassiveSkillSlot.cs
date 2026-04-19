using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PassiveSkillSlot : MonoBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] private string skillId; // "Engine", "Universe", "Tech" 중 하나 입력
    [SerializeField] private PassiveSkillData skillData;
    [SerializeField] private SkillShopManager shopManager;

    // 인스펙터 노출 없이 내부에서 자동으로 찾아서 쓸 변수들
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI explainText;
    private TextMeshProUGUI levelText;
    private Image skillImage;
    private Button activeBtn;
    private Button levelUpBtn;

    private void Awake()
    {
        nameText = transform.Find("SkillId").GetComponent<TextMeshProUGUI>();
        
        levelText = transform.Find("SkillLevel").GetComponent<TextMeshProUGUI>();
        skillImage = transform.Find("SkillImage").GetComponent<Image>();
        //activeBtn = transform.Find("Skillactive").GetComponent<Button>();
        explainText = transform.Find("SkillExplain").GetComponent<TextMeshProUGUI>();
        levelUpBtn = transform.Find("LevelUp").GetComponent<Button>();

        levelUpBtn.onClick.AddListener(OnClickBuy);

        if (activeBtn != null) 
        {
            activeBtn.gameObject.SetActive(false); 
        }
    }

    public void UpdateSlotUI()
    {
        if (skillData == null) return;

        nameText.text = skillData.SkillName;
        explainText.text = skillData.Description;

        int currentLevel = GetCurrentLevel();

        if (currentLevel >= skillData.MaxLevel)
        {
            levelText.text = "MAX\n-";
            levelUpBtn.interactable = false; 
        }
        else
        {
            int cost = skillData.GetCostForNextLevel(currentLevel);
            levelText.text = $"Lv.{currentLevel}\n{cost} P";
            
            levelUpBtn.interactable = UpgradeManager.Instance.TotalPoints >= cost;
        }
    }

    private void OnClickBuy()
    {
        int currentLevel = GetCurrentLevel();
        if (currentLevel >= skillData.MaxLevel) return;

        int cost = skillData.GetCostForNextLevel(currentLevel);
        
        if (UpgradeManager.Instance.TrySpendPoints(cost))
        {
            UpgradeManager.Instance.UpgradePassive(skillId);
            shopManager.UpdateAllShopUI();
        }
    }

    private int GetCurrentLevel()
    {
        switch(skillId)
        {
            case "Engine": return UpgradeManager.Instance.EnginePowerLevel;
            case "Universe": return UpgradeManager.Instance.UniverseSizeLevel;
            case "Tech": return UpgradeManager.Instance.TechLevel;
            default: return 1;
        }
    }
}