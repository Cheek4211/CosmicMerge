using UnityEngine;

public class TitleUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject shopPanel;


    private void Start()
    {
        GameManager.Instance.OnStateChanged += UpdateUI;
        UpdateUI(GameManager.Instance.currentState);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= UpdateUI;
    }

    private void UpdateUI(GameState state)
    {
        mainMenuPanel.SetActive(state == GameState.MainMenu);
        shopPanel.SetActive(state == GameState.Shop);
    }

    public void OnClickStartGame() => GameManager.Instance.StartNewGame();
    public void OnClickOpenShop() => GameManager.Instance.ChangeState(GameState.Shop);
    public void OnClickCloseShop() => GameManager.Instance.ChangeState(GameState.MainMenu);

}
