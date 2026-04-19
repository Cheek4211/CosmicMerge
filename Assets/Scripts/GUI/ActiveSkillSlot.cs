using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActiveSkillSlot : MonoBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] private string skillId;
    [SerializeField] private ActiveSkillData skillData;
    [SerializeField] private SkillShopManager shopManager;

    private TextMeshProUGUI nameText;
    private TextMeshProUGUI explainText;
    private TextMeshProUGUI levelText;
    private Image skillImage;
    private Button activeBtn;
    private Button levelUpBtn;

    private void Awake()
    {
        nameText = transform.Find("SkillId").GetComponent<TextMeshProUGUI>();
        //
        levelText = transform.Find("SkillLevel").GetComponent<TextMeshProUGUI>();
        skillImage = transform.Find("SkillImage").GetComponent<Image>();
        explainText = transform.Find("SkillExplain").GetComponent<TextMeshProUGUI>();
        //activeBtn = transform.Find("Skillactive").GetComponent<Button>();
        levelUpBtn = transform.Find("LevelUp").GetComponent<Button>();

        levelUpBtn.onClick.AddListener(OnClickBuy);
        
        // activeBtn.onClick.AddListener(OnUseSkill);
    }

    public void UpdateSlotUI()
    {
        if (skillData == null) return;

        nameText.text = skillData.SkillName;
        explainText.text = skillData.Description;
        
        // skillImage.sprite = skillData.skillIcon;

        bool isUnlocked = GetIsUnlocked();
        int currentAmmo = GetAmmo();
        
        if (!isUnlocked)
        {
            levelText.text = $"미해금\n{skillData.UnlockCost} P";
            levelUpBtn.interactable = UpgradeManager.Instance.TotalPoints >= skillData.UnlockCost;
        }
        else if (currentAmmo > 0)
        {
            levelText.text = "장전 완료";
            levelUpBtn.interactable = false;
        }
        else
        {
            levelText.text = $"재장전 필요\n{skillData.RefillCost} P";
            levelUpBtn.interactable = UpgradeManager.Instance.TotalPoints >= skillData.RefillCost;
        }
    }

    private void OnClickBuy()
    {
        bool isUnlocked = GetIsUnlocked();
        int ammo = GetAmmo();

        if (isUnlocked && ammo > 0) return; 

        int cost = skillData.GetCurrentCost(isUnlocked);
        
        if (UpgradeManager.Instance.TrySpendPoints(cost))
        {
            UpgradeManager.Instance.PurchaseActive(skillId, !isUnlocked);
            shopManager.UpdateAllShopUI();
        }
    }

    private bool GetIsUnlocked()
    {
        switch(skillId)
        {
            case "CosmicWar": return UpgradeManager.Instance.IsCosmicWarUnlocked;
            case "Alien": return UpgradeManager.Instance.IsAlienAbductionUnlocked;
            case "Evolution": return UpgradeManager.Instance.IsEvolutionLightUnlocked;
            default: return false;
        }
    }

    private int GetAmmo()
    {
        switch(skillId)
        {
            case "CosmicWar": return UpgradeManager.Instance.AmmoCosmicWar;
            case "Alien": return UpgradeManager.Instance.AmmoAlienAbduction;
            case "Evolution": return UpgradeManager.Instance.AmmoEvolutionLight;
            default: return 0;
        }
    }
}