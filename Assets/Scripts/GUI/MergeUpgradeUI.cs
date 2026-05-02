using UnityEngine;

public class MergeUpgradeUI : MonoBehaviour
{
    private PassiveSkillSlot[] slots = new PassiveSkillSlot[3];

    private void Awake()
    {
        string[] childNames = { "First", "Second", "Third" };
        for (int i = 0; i < 3; i++)
        {
            slots[i] = transform.Find($"PassivePanel/{childNames[i]}")
                                .GetComponent<PassiveSkillSlot>();
            slots[i].OnUpgraded += Close;
        }

        gameObject.SetActive(false);
    }

    public void Show()
    {
        foreach (var slot in slots) slot.UpdateSlotUI();
        gameObject.SetActive(true);
        GameManager.Instance.ChangeState(GameState.UpgradeSelection);
    }

    private void Close()
    {
        gameObject.SetActive(false);
        GameManager.Instance.ChangeState(GameState.Playing);
    }
}
