using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu, Shop, Playing, MergeSettling, UpgradeSelection, Paused, GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private const string titleSceneName = "TitleScene";
    private const string gameSceneName = "GameScene";

    public GameState currentState;

    public event Action<GameState> OnStateChanged;
    public event Action OnNewGameStarted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private bool openShopOnTitleLoad = false;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameSceneName)
        {
            OnNewGameStarted?.Invoke();
            ChangeState(GameState.Playing);
        }
        else if (scene.name == titleSceneName)
        {
            GameState target = openShopOnTitleLoad ? GameState.Shop : GameState.MainMenu;
            openShopOnTitleLoad = false;
            ChangeState(target);
        }
    }

    private void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == gameSceneName)
            ChangeState(GameState.Playing);
        else
            ChangeState(GameState.MainMenu);
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;

        Time.timeScale = (newState == GameState.Paused || newState == GameState.UpgradeSelection || newState == GameState.GameOver) ? 0f : 1f;

        if (newState == GameState.GameOver)
            if (ScoreManager.Instance != null) ScoreManager.Instance.HandleGameOver();

        OnStateChanged?.Invoke(newState);
    }

    public void StartNewGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        currentState = GameState.MainMenu;
        SceneManager.LoadScene(titleSceneName);
        OnStateChanged?.Invoke(GameState.MainMenu);
    }

    public void ReturnToShop()
    {
        Time.timeScale = 1f;
        openShopOnTitleLoad = true;
        SceneManager.LoadScene(titleSceneName);
    }
}