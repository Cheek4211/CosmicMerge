using System;
using System.Collections;
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

    [Header("GameOver Settings")]
    [SerializeField] private float checkInterval = 0.5f; // 체크 주기
    [SerializeField] private float outOfBoundsLimitTime = 3.0f; // 밖에서 버틸 수 있는 시간

    private float outOfBoundsTimer = 0f;

    public GameState currentState;

    public event Action<GameState> OnStateChanged;

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


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameSceneName)
            ChangeState(GameState.Playing);
        else if (scene.name == titleSceneName)
            ChangeState(GameState.MainMenu);
    }

    private void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == gameSceneName)
        {
            ChangeState(GameState.Playing);
        }
        else
        {
            ChangeState(GameState.MainMenu);

        }

    }

    private IEnumerator GameStateCheckRoutine()
    {
        while (true)
        {
            if (currentState == GameState.Playing)
            {
                CheckGameOverCondition();
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    private void CheckGameOverCondition()
    {
        if (ShipManager.Instance == null || UniverseManager.Instance == null) return;

        // 1. 현재 필드에 존재하는 모든 행성들 (데이터)
        var allActiveShips = ShipManager.Instance.ActiveShips;
        // 2. 경계 영역 안에 있는 행성들 (물리)
        var insideShips = UniverseManager.Instance.InsideShips;

        bool isAnyShipMoving = false;
        bool isAnyShipOutside = false;

        foreach (var ship in allActiveShips)
        {
            // 1. 합쳐지는 중이거나, 아직 발사되지 않은(대기 중인) 행성은 검사 제외
            if (ship.IsMerged || !ship.IsLaunched) continue;

            // [조건 1] 움직이고 있는 행성이 있는지 체크
            if (!ship.IsStoped)
            {
                isAnyShipMoving = true;
            }

            // [조건 2] 영역 안에 포함되어 있지 않은 행성이 있는지 체크
            if (!insideShips.Contains(ship))
            {
                isAnyShipOutside = true;
            }
        }

        // [핵심 로직] 
        // 1. 움직이는 행성이 하나라도 있다면 판단을 다음 타임으로 넘김 (판정 보류)
        if (isAnyShipMoving)
        {
            outOfBoundsTimer = 0f;
            return;
        }

        // 2. 모든 행성이 멈춘 상태에서, 밖으로 나간 행성이 있다면 타이머 시작
        if (isAnyShipOutside)
        {
            outOfBoundsTimer += checkInterval;
            if (outOfBoundsTimer >= outOfBoundsLimitTime)
            {
                ChangeState(GameState.GameOver);
            }
        }
        else
        {
            outOfBoundsTimer = 0f;
        }
    }
    public void ChangeState(GameState newState)
    {
        currentState = newState;

        Time.timeScale = (newState == GameState.Paused) ? 0f : 1f;

        if (newState == GameState.GameOver)
        {
            if (ScoreManager.Instance != null) ScoreManager.Instance.HandleGameOver();
            Debug.Log($"[GameOver] Loading scene: '{titleSceneName}'");
            SceneManager.LoadScene(titleSceneName);
        }

        if(newState == GameState.Playing)
        {
            StartCoroutine(GameStateCheckRoutine());
        }

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
}