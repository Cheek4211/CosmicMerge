using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu, Shop, Playing, Paused, GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string titleSceneName = "Title";
    [SerializeField] private string gameSceneName = "Game";

    public GameState currentState;

    public event Action<GameState> OnStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ChangeState(GameState.MainMenu);
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;

        Time.timeScale = (newState == GameState.Paused) ? 0f : 1f;

        if (newState == GameState.GameOver)
        {
            if (ScoreManager.Instance != null) ScoreManager.Instance.HandleGameOver();
            SceneManager.LoadScene(titleSceneName);
        }

        OnStateChanged?.Invoke(newState);
    }

    public void StartNewGame()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
        OnStateChanged?.Invoke(GameState.Playing);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        currentState = GameState.MainMenu;
        SceneManager.LoadScene(titleSceneName);
        OnStateChanged?.Invoke(GameState.MainMenu);
    }
}