using UnityEngine;
using System;

public class ScoreManager : Singleton<ScoreManager>
{
    public event Action<int> OnScoreChanged;
    public int Score { get; private set; }
    
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
