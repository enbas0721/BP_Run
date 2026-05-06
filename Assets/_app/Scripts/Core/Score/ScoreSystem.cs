using System;

public class ScoreSystem
{
    public event Action<int> OnScoreChanged;

    public int Score { get; private set; }
    private float scoreFloat;

    private float scorePerSecond;

    public ScoreSystem(float scorePerSecond)
    {
        this.scorePerSecond = scorePerSecond;
        Reset();
    }

    public void SetScorePerSecond(float value)
    {
        scorePerSecond = value;
    }

    public void Reset()
    {
        scoreFloat = 0f;
        SetScore(0);
    }

    public void Tick(float deltaTime)
    {
        if (deltaTime <= 0f) return;

        scoreFloat += scorePerSecond * deltaTime;
        int newScore = (int)Math.Floor(scoreFloat);

        if (newScore != Score)
        {
            SetScore(newScore);
        }
    }
    public void Add(int amount)
    {
        scoreFloat += amount;
        int newScore = (int)Math.Floor(scoreFloat);
        if (newScore != Score)
        {
            SetScore(newScore);
        }
    }

    private void SetScore(int value)
    {
        Score = value;
        OnScoreChanged?.Invoke(Score);
    }
}
