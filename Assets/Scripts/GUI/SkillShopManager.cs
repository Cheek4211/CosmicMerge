using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillShopManager : MonoBehaviour
{
    [Header("Top UI")]
    [SerializeField] private TextMeshProUGUI totalPointsText;

    [Header("Tab Panels")]
    [SerializeField] private GameObject passivePanel;
    [SerializeField] private GameObject activePanel;

    [Header("Tab Buttons")]
    [SerializeField] private Button passiveTabBtn;
    [SerializeField] private Button activeTabBtn;

    [Header("Skill Slots")]
    [SerializeField] private PassiveSkillSlot[] passiveSlots;
    [SerializeField] private ActiveSkillSlot[] activeSlots;

    private void Awake()
    {
        passiveTabBtn.onClick.AddListener(ShowPassiveTab);
        activeTabBtn.onClick.AddListener(ShowActiveTab);
    }

    private void OnEnable()
    {
        ShowPassiveTab();
    }

    public void ShowPassiveTab()
    {
        passivePanel.SetActive(true);
        activePanel.SetActive(false);
        UpdateAllShopUI();
    }

    public void ShowActiveTab()
    {
        passivePanel.SetActive(false);
        activePanel.SetActive(true);
        UpdateAllShopUI();
    }

    public void UpdateAllShopUI()
    {
        if (UpgradeManager.Instance == null) return;

        totalPointsText.text = $"{UpgradeManager.Instance.TotalPoints}";

        if (passivePanel.activeSelf)
        {
            foreach (var slot in passiveSlots)
            {
                if (slot == null) continue;
                slot.UpdateSlotUI();
            }
        }
        else
        {
            foreach (var slot in activeSlots)
            {
                if (slot == null) continue;
                slot.UpdateSlotUI();
            }
        }
    }
}