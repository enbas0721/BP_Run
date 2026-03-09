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

    [Header("Difficulty / Speed Scaling")]
    [SerializeField] private RunnerController runner;
    [SerializeField] private float timeToMaxSpeed = 120f;
    [SerializeField] private float maxBaseSpeedMultiplier = 2.0f;
    [SerializeField] private AnimationCurve speedCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Game System")]
    [SerializeField] private SegmentSpawner segmentSpawner;
    [SerializeField] private AdrenalineSystem adrenalineSystem;
    [SerializeField] private SwipeInput swipeInput;

    private float playTime = 0f;

    [Header("Debug / Cheat")]
    [SerializeField] private bool debugInvincible = false;
    public bool DebugInvincible => debugInvincible;

    private float invincibleUntil = -1f;

    public bool IsInvincible
    {
        get
        {
            if (debugInvincible) return true;
            if (invincibleUntil < 0f) return false;
            return Time.time < invincibleUntil;
        }
    }

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
        SetState(GameState.Ready);
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

        playTime += Time.deltaTime;

        float t = Mathf.Clamp01(playTime / Mathf.Max(0.01f, timeToMaxSpeed));
        float curve = speedCurve.Evaluate(t);
        float baseMul = Mathf.Lerp(1f, maxBaseSpeedMultiplier, curve);

        if (runner) runner.SetBaseForwardMultiplier(baseMul);
    }

    public void SetState(GameState next)
    {
        if (State == next) return;
        State = next;

        /* [MEMO] Playing以外は止める */
        /* Time.timeScale = (State == GameState.Playing) ? 1f : 0f; */

        OnStateChanged?.Invoke(State);
    }

    public void GameOver()
    {
        if (State == GameState.GameOver) return;
        SetState(GameState.GameOver);
    }

    public void StartRun()
    {
        playTime = 0f;
        if (runner) runner.SetBaseForwardMultiplier(1f);

        ClearInvincible();

        ScoreSystem.Reset();
        OnScoreChanged?.Invoke(ScoreSystem.Score);


        SetState(GameState.Playing);
    }

    public void ResetRun()
    {
        playTime = 0f;
        // runnerのリセット
        if (runner)
        {
            runner.ResetRunner();
        }

        // segmentSpawnerのリセット
        if (segmentSpawner)
        {
            segmentSpawner.ResetSegments();
        }

        // adrenalineSystemのリセット
        if (adrenalineSystem)
        {
            adrenalineSystem.ResetSystem();
        }

        // SwipeInputのリセット
        if (swipeInput)
        {
            swipeInput.ResetInputState();
        }

        // 無敵状態をクリア
        ClearInvincible();

        // スコアリセット
        ScoreSystem.Reset();
        OnScoreChanged?.Invoke(ScoreSystem.Score);

        SetState(GameState.Ready);
    }

    public void SetInvincibleFor(float seconds)
    {
        if (seconds <= 0f) return;

        float until = Time.time + seconds;
        invincibleUntil = Mathf.Max(invincibleUntil, until);
    }

    public void ClearInvincible()
    {
        invincibleUntil = -1f;
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
