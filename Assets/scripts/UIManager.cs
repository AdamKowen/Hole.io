using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public GameObject gameOverPanel;
    public TMP_Text[] highScoreTexts;

    [Header("Timer Settings")]
    public float startTime = 90f;  // you can change in Inspector
    private float remainingTime;
    private bool gameRunning = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        ResetUI();
        
        // NEW: bind to ScoreManager every load, and sync current score
        if (ScoreManager.Instance)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScore;
            UpdateScore(ScoreManager.Instance.Score);
        }
    }

    void Update()
    {
        if (!gameRunning) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            EndGame();
        }
        UpdateTimerText();
    }

    public void StartGame()
    {
        if (ScoreManager.Instance) ScoreManager.Instance.ResetScore(); // ⬅️ חשוב

        // turn labels on when game actually starts
        if (scoreText) scoreText.gameObject.SetActive(true);
        if (timerText) timerText.gameObject.SetActive(true);
        
        remainingTime = startTime;
        gameRunning = true;
        gameOverPanel.SetActive(false);
        UpdateScore(ScoreManager.Instance ? ScoreManager.Instance.Score : 0);
        UpdateTimerText();
    }


    public void EndGame()
    {
        gameRunning = false;

        // NEW: hard-freeze the hole so player can't move post-GameOver
        var gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        if (gm && gm.hole)
        {
            gm.hole.Freeze(true);
            gm.hole.enabled = false;
        }

        gameOverPanel.SetActive(true);
        SaveHighScore(ScoreManager.Instance ? ScoreManager.Instance.Score : 0);
        ShowHighScores();
    }

    public void UpdateScore(int newScore)
    {
        if (scoreText)
            scoreText.text = "Score: " + newScore;
    }

    void UpdateTimerText()
    {
        if (timerText)
            timerText.text = "Time: " + Mathf.CeilToInt(remainingTime);
    }

    void ResetUI()
    {
        UpdateScore(0);
        UpdateTimerText();
        gameOverPanel.SetActive(false);
    }

    // High Scores (saved locally)
    void SaveHighScore(int newScore)
    {
        int[] scores = new int[5];
        for (int i = 0; i < 5; i++)
            scores[i] = PlayerPrefs.GetInt("HighScore" + i, 0);

        for (int i = 0; i < 5; i++)
        {
            if (newScore > scores[i])
            {
                for (int j = 4; j > i; j--)
                    scores[j] = scores[j - 1];
                scores[i] = newScore;
                break;
            }
        }

        for (int i = 0; i < 5; i++)
            PlayerPrefs.SetInt("HighScore" + i, scores[i]);

        PlayerPrefs.Save();
    }

    void ShowHighScores()
    {
        for (int i = 0; i < highScoreTexts.Length; i++)
        {
            int score = PlayerPrefs.GetInt("HighScore" + i, 0);
            highScoreTexts[i].text = $"{i + 1}. {score}";
        }
    }

    // Hook for Play Again button
    public void OnPlayAgain()
    {
        // Tell next scene load to auto-start (skip the start menu once)
        PlayerPrefs.SetInt("SkipMenuOnce", 1);
        PlayerPrefs.Save();

        var s = SceneManager.GetActiveScene();
        SceneManager.LoadScene(s.buildIndex);
    }
}
