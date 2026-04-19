using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    // 외부에서는 읽을 수만 있고, 덮어쓸 수는 없는 안전한 싱글톤 프로퍼티
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [SerializeField] public int currentScore = 0;
    [SerializeField] private int scoreToPointRatio = 100;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GameManager.Instance.OnStateChanged += OnStateChanged;
        UpdateScoreUI();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.Playing)
            ResetScore();
    }

    // 우주선이 융합할 때 외부(ShipManager)에서 호출할 함수
    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"점수: {currentScore}";
        }
    }
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
    }

    // 우주선이 경계선을 벗어나 타이머가 0이 되었을 때 호출할 게임 오버 함수
    public void HandleGameOver()
    {
        // int earnedPoints = currentScore / scoreToPointRatio;

        // if (UpgradeManager.Instance != null)
        // {
        //     UpgradeManager.Instance.totalPoints += earnedPoints;
        //     UpgradeManager.Instance.SaveData();
            
        //     Debug.Log($"gameOver! Score: {currentScore} / Point: {earnedPoints} / all Point: {UpgradeManager.Instance.totalPoints}");
        // }

    }
}