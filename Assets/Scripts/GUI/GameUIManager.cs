using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;

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
        hudPanel.SetActive(state == GameState.Playing || state == GameState.Paused);
        pausePanel.SetActive(state == GameState.Paused);
    }

    public void OnClickResume() => GameManager.Instance.ChangeState(GameState.Playing);
    public void OnClickPause() => GameManager.Instance.ChangeState(GameState.Paused);
    public void OnClickReStart() => GameManager.Instance.StartNewGame();
    public void OnClickReturnToMain() => GameManager.Instance.ReturnToMainMenu();
}
