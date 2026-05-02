using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

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
        hudPanel.SetActive(state == GameState.Playing
            || state == GameState.MergeSettling
            || state == GameState.Paused
            || state == GameState.UpgradeSelection);
        pausePanel.SetActive(state == GameState.Paused);
        gameOverPanel.SetActive(state == GameState.GameOver);
    }
    public void OnClickPause() => GameManager.Instance.ChangeState(GameState.Paused);
    public void OnClickResume() => GameManager.Instance.ChangeState(GameState.Playing);
    public void OnClickRestart() => GameManager.Instance.StartNewGame();
    public void OnClickReturnToMain() => GameManager.Instance.ReturnToMainMenu();
    public void OnClickOpenShop() => GameManager.Instance.ReturnToShop();
    public void OnClickSettings() { }
    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
