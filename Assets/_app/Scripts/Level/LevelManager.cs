using UnityEngine;

public enum DifficultyLevel
{
    Easy     = 0,
    Normal   = 1,
    Hard     = 2,
    VeryHard = 3,
}

/// <summary>
/// スコアに応じてRoadSegmentPoolの重みをガウス分布で動的に更新する。
/// 分布の山がEasy→VeryHardへシフトしていく。
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoadSegmentPool roadSegmentPool;

    [Header("Difficulty Curve")]
    
    [Tooltip("難易度上昇を開始するスコア（チュートリアル終了後を想定）")]
    [SerializeField] private float scoreAtMinDifficulty = 5000f;
    
    [Tooltip("ピークがmaxDifficultyに到達するスコア")]
    [SerializeField] private float scoreAtMaxDifficulty = 30000f;

    [Tooltip("ピークが到達する最大難易度")]
    [SerializeField] private DifficultyLevel maxDifficulty = DifficultyLevel.VeryHard;
    [Tooltip("ガウス分布の広がり（大きいほど隣接難易度が多く混ざる）")]

    [SerializeField] private float sigma = 1.0f;
    
    [Tooltip("どの難易度レベルにも保証する最低重み(0にならない)")]
    [SerializeField] [Min(0.01f)] private float minWeight = 0.1f;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnScoreChanged += OnScoreChanged;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnScoreChanged -= OnScoreChanged;
    }

    private void Start()
    {
        // 初期スコア0で初回更新
        UpdateWeights(0);
    }

    private void OnScoreChanged(int score)
    {
        UpdateWeights(score);
    }

    private void UpdateWeights(int score)
    {
        if (roadSegmentPool == null) return;

        float maxIndex = (float)maxDifficulty;
        float range = scoreAtMaxDifficulty - scoreAtMinDifficulty;
        float t = range > 0f ? Mathf.Clamp01((score - scoreAtMinDifficulty) / range) : 1f;
        float peak = t * maxIndex;

        foreach (var entry in roadSegmentPool.entries)
        {
            float levelIndex = (float)entry.difficultyLevel;
            float diff = (levelIndex - peak) / sigma;
            float gaussian = Mathf.Exp(-0.5f * diff * diff);
            entry.weight = Mathf.Max(minWeight, gaussian);
        }
    }

#if UNITY_EDITOR
    // インスペクターでリアルタイム確認用
    [Header("Debug (Editor Only)")]
    [SerializeField] [Range(0f, 100000f)] private float debugScore = 0f;
    private float prevDebugScore = -1f;

    private void OnValidate()
    {
        if (!Application.isPlaying) return;
        if (!Mathf.Approximately(debugScore, prevDebugScore))
        {
            prevDebugScore = debugScore;
            UpdateWeights((int)debugScore);
        }
    }
#endif
}
