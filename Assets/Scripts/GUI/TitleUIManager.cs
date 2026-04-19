using UnityEngine;

public class TitleUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject gameOverPanel;

    private void Start()
    {
        GameManager.Instance.OnStateChanged += UpdateUI;
        UpdateUI(GameManager.Instance.currentState);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= UpdateUI;
        }
    }

    private void UpdateUI(GameState state)
    {
        mainMenuPanel.SetActive(state == GameState.MainMenu);
        shopPanel.SetActive(state == GameState.Shop);
        gameOverPanel.SetActive(state == GameState.GameOver);
    }

    public void OnClickStartGame() => GameManager.Instance.StartNewGame();
    public void OnClickOpenShop() => GameManager.Instance.ChangeState(GameState.Shop);
    public void OnClickCloseShop() => GameManager.Instance.ChangeState(GameState.MainMenu);
    public void OnClickReturnToMain() => GameManager.Instance.ReturnToMainMenu();
}
