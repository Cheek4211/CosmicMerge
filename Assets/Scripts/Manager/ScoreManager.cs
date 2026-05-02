using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [SerializeField] public int currentScore = 0;
    [SerializeField] private int scoreToPointRatio = 200;

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
        GameManager.Instance.OnNewGameStarted += ResetScore;
        UpdateScoreUI();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnNewGameStarted -= ResetScore;
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"{currentScore}";
        }
    }
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
    }

    public void HandleGameOver()
    {
        int earnedPoints = currentScore / scoreToPointRatio;

        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.AddPoints(earnedPoints);
            Debug.Log($"GameOver! Score: {currentScore} / Earned: {earnedPoints}P / Total: {UpgradeManager.Instance.TotalPoints}P");
        }
    }
}