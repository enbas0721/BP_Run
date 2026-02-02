using System;
using UnityEngine;

public enum GameState
{ 
    Ready,
    Playing,
    Paused,
    GameOver,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score Settings")]
    [Tooltip("1秒あたりのスコア増加量")]
    [SerializeField] private float scorePerSecond = 10f;

    public GameState State { get; private set; } = GameState.Ready;

    public event Action<int> OnScoreChanged;
    public event Action<GameState> OnStateChanged;

    public ScoreSystem ScoreSystem { get; private set; }

    private void Awake()
    {
        if ( Instance != null && Instance != this )
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ScoreSystem = new ScoreSystem(scorePerSecond);
        ScoreSystem.OnScoreChanged += HandleScoreChanged;
    }

    private void Start()
    {
        /* [TODO] ゲーム管理画面ができたらそこからステート変更  */
        SetState(GameState.Playing);
        OnScoreChanged?.Invoke(ScoreSystem.Score);
    }
    private void OnDestroy()
    {
        if (ScoreSystem != null)
        {
            ScoreSystem.OnScoreChanged -= HandleScoreChanged;
        }
    }
    private void Update()
    {
        if (State != GameState.Playing) return;
        ScoreSystem.Tick(Time.deltaTime);
    }

    public void SetState(GameState next)
    {
        if (State == next) return;
        State = next;

        /* [MEMO] TimeScale変更で大丈夫？ */
        Time.timeScale = (State == GameState.Paused) ? 0f : 1f;

        OnStateChanged?.Invoke(State);
    }

    public void GameOver()
    {
        if (State == GameState.GameOver) return;
        SetState(GameState.GameOver);
    }

    public void ResetRun()
    {
        ScoreSystem.Reset();
        OnScoreChanged?.Invoke(ScoreSystem.Score);
        SetState(GameState.Playing);
    }

    public void AddScore(int amount)
    {
        if (State != GameState.Playing) return;
        ScoreSystem.Add(amount);
    }

    private void HandleScoreChanged(int newScore)
    {
        OnScoreChanged?.Invoke(newScore);
    }

}
