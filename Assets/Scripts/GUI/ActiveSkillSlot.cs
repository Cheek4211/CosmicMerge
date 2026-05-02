using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActiveSkillSlot : MonoBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] private ActiveSkillId skillId;
    [SerializeField] private ActiveSkillData skillData;
    [SerializeField] private SkillShopManager shopManager;

    private TextMeshProUGUI nameText;
    private TextMeshProUGUI explainText;
    private TextMeshProUGUI levelText;
    private Button levelUpBtn;

    private void Awake()
    {
        nameText    = transform.Find("SkillId").GetComponent<TextMeshProUGUI>();
        levelText   = transform.Find("SkillLevel").GetComponent<TextMeshProUGUI>();
        explainText = transform.Find("SkillExplain").GetComponent<TextMeshProUGUI>();
        levelUpBtn  = transform.Find("LevelUp").GetComponent<Button>();

        levelUpBtn.onClick.AddListener(OnClickBuy);
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
            shopManager?.UpdateShopUI();
        }
    }

    private bool GetIsUnlocked()
    {
        switch (skillId)
        {
            case ActiveSkillId.CosmicWar:  return UpgradeManager.Instance.IsCosmicWarUnlocked;
            case ActiveSkillId.Alien:      return UpgradeManager.Instance.IsAlienAbductionUnlocked;
            case ActiveSkillId.Evolution:  return UpgradeManager.Instance.IsEvolutionLightUnlocked;
            default:                       return false;
        }
    }

    private int GetAmmo()
    {
        switch (skillId)
        {
            case ActiveSkillId.CosmicWar:  return UpgradeManager.Instance.AmmoCosmicWar;
            case ActiveSkillId.Alien:      return UpgradeManager.Instance.AmmoAlienAbduction;
            case ActiveSkillId.Evolution:  return UpgradeManager.Instance.AmmoEvolutionLight;
            default:                       return 0;
        }
    }
}