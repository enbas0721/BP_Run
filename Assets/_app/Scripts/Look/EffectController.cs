using System.Collections;
using UnityEngine;

public enum EffectCondition
{
    Always,     // 常時
    OnRush,     // Rush中
    OnNearMiss, // NearMiss中
}

[System.Serializable]
public class EffectEntry
{
    public ParticleSystem particles;
    public EffectCondition condition;
    [Tooltip("ONにすると条件が外れた後も既存パーティクルを最後まで再生する")]
    public bool playToCompletion = false;
}

/// <summary>
/// 複数のParticleSystemを条件ごとに一括管理する。
/// RunnerのGameObjectにアタッチし、子オブジェクトのParticleSystemを登録する。
/// </summary>
public class EffectController : MonoBehaviour
{
    [SerializeField] private EffectEntry[] entries;
    [SerializeField] private AdrenalineSystem adrenalineSystem;

    [Header("Rush Effect")]
    [SerializeField] private RushEffect rushEffect;

    [Header("NearMiss Effect")]
    [Tooltip("NearMiss時にスポーンするTorusTrailMoverのPrefab")]
    [SerializeField] private TorusTrailMover nearMissTorusPrefab;
    [Tooltip("ランナーに追従する時間（秒）")]
    [SerializeField] private float nearMissFollowDuration = 0.2f;
    [Tooltip("切り離し後にその場に残る時間（秒）")]
    [SerializeField] private float nearMissLingerDuration = 0.4f;

    private bool prevRushActive = false;

    private void Awake()
    {
        if (!adrenalineSystem)
            adrenalineSystem = FindFirstObjectByType<AdrenalineSystem>();

        foreach (var entry in entries)
        {
            if (entry.particles)
                entry.particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void OnEnable()
    {
        if (adrenalineSystem)
            adrenalineSystem.OnNearMissStarted += OnNearMissStarted;
    }

    private void OnDisable()
    {
        if (adrenalineSystem)
            adrenalineSystem.OnNearMissStarted -= OnNearMissStarted;
    }

    private void Update()
    {
        foreach (var entry in entries)
        {
            if (!entry.particles) continue;

            bool shouldPlay = EvaluateCondition(entry.condition);
            bool isPlaying  = entry.particles.isEmitting;

            if (shouldPlay && !isPlaying)
                entry.particles.Play();
            else if (!shouldPlay && isPlaying)
            {
                var stopBehavior = entry.playToCompletion
                    ? ParticleSystemStopBehavior.StopEmitting
                    : ParticleSystemStopBehavior.StopEmittingAndClear;
                entry.particles.Stop(true, stopBehavior);
            }
        }

        UpdateRushEffect();
    }

    private void OnNearMissStarted(int direction)
    {
        if (!nearMissTorusPrefab) return;
        StartCoroutine(SpawnNearMissTorus(direction));
    }

    private IEnumerator SpawnNearMissTorus(int direction)
    {
        // ランナーの子として生成 → 横移動に追従
        var instance = Instantiate(nearMissTorusPrefab, transform);
        instance.transform.localPosition = Vector3.zero;
        instance.StartRotation(direction);

        // 追従フェーズ
        yield return new WaitForSeconds(nearMissFollowDuration);

        // 切り離し → その場に残る
        instance.transform.SetParent(null, worldPositionStays: true);

        // 残留フェーズ後にDestroy
        yield return new WaitForSeconds(nearMissLingerDuration);

        Destroy(instance.gameObject);
    }

    private void UpdateRushEffect()
    {
        if (!rushEffect) return;

        bool rushActive = adrenalineSystem != null && adrenalineSystem.IsRushActive;
        if (rushActive == prevRushActive) return;

        prevRushActive = rushActive;
        if (rushActive) rushEffect.Activate();
        else            rushEffect.Deactivate();
    }

    private bool EvaluateCondition(EffectCondition condition)
    {
        switch (condition)
        {
            case EffectCondition.Always:
                return true;
            case EffectCondition.OnRush:
                return adrenalineSystem != null && adrenalineSystem.IsRushActive;
            case EffectCondition.OnNearMiss:
                return adrenalineSystem != null && adrenalineSystem.IsNearMissActive;
            default:
                return false;
        }
    }
}
