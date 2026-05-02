using UnityEngine;
using TMPro;

public class SkillShopManager : MonoBehaviour
{
    [Header("Top UI")]
    [SerializeField] private TextMeshProUGUI totalPointsText;

    [Header("Skill Slots")]
    [SerializeField] private ActiveSkillSlot[] activeSlots;

    private void Start()
    {
        UpdateShopUI();
    }

    public void UpdateShopUI()
    {
        if (UpgradeManager.Instance == null) return;

        totalPointsText.text = $"{UpgradeManager.Instance.TotalPoints}";

        foreach (var slot in activeSlots)
        {
            if (slot == null) continue;
            slot.UpdateSlotUI();
        }
    }
}
