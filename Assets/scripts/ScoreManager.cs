using UnityEngine;
using System;


//we may want to display the score, Adam what do you think? 
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public event Action<int> OnScoreChanged;
    public int Score { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        if (UIManager.Instance)
            OnScoreChanged += UIManager.Instance.UpdateScore;
    }

    public void AddPoints(int amount)
    {
        if (amount <= 0) return;
        Score += amount;
        OnScoreChanged?.Invoke(Score);
        Debug.Log("total score: " + Score);
    }

    public void ResetScore()
    {
        Score = 0;
        OnScoreChanged?.Invoke(Score);
    }
}
