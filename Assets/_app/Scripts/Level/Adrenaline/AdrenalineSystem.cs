using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RunnerController))]
public class AdrenalineSystem : MonoBehaviour
{
    [Header("Gauge")]
    [SerializeField] private float gaugeMax = 100f;
    [SerializeField] private float gauge = 0f;

    [Header("NearMiss (Lane Change Slow)")]
    [SerializeField] private float nearMissDuration = 0.20f;
    [SerializeField] private float nearMissCooldown = 0.1f;
    [SerializeField] private float nearMissTimeScale = 0.3f;
    [SerializeField] private float nearMissAdrenalinePerSecond = 40f;
    [Tooltip("初回ゲージ満タンまでのチュートリアル中に使用するゲージ上昇量")]
    [SerializeField] private float tutorialNearMissAdrenalinePerSecond = 80f;

    private float originalFixedDeltaTime;

    [Header("Rush (Invincible + Fast)")]
    [SerializeField] private float rushDuration = 5.0f;
    [SerializeField] private float rushForwardSpeedMultiplier = 2.0f;
    [SerializeField] private float rushWindDownDuration = 1.5f;
    [SerializeField] private AnimationCurve rushWindDownCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public event System.Action<int> OnNearMissStarted;
    public event System.Action OnNearMissEnded;

    public bool CanActivateRush => gauge >= gaugeMax && !rushActive;

    public float Gauge => gauge;
    public float GaugeMax => gaugeMax;

    private RunnerController runner;

    private readonly HashSet<NearMissZone> overlappedZones = new();

    private bool nearMissActive = false;
    private float nearMissEndTime = -999f;
    private float cooldownUntil = -999f;
    private float gaugePerSec = 0f;

    private bool rushActive = false;
    private float rushEndTime = -999f;
    private float rushPausedRemaining = 0f;

    private bool gaugeFillNotified = false;

    public bool IsRushActive => rushActive;
    public bool IsNearMissActive => nearMissActive;

    private void Awake()
    {
        runner = GetComponent<RunnerController>();

        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void OnEnable()
    {
        runner.OnLaneChangeRequested += HandleLaneChangeRequested;
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        runner.OnLaneChangeRequested -= HandleLaneChangeRequested;
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Paused && rushActive)
        {
            rushPausedRemaining = rushEndTime - Time.unscaledTime;
        }
        else if (state == GameState.Playing && rushPausedRemaining > 0f)
        {
            rushEndTime = Time.unscaledTime + rushPausedRemaining;
            rushPausedRemaining = 0f;
        }
    }

    public void ResetSystem()
    {
        // 1) NearMiss 強制終了（TimeScaleを必ず戻す）
        if (nearMissActive)
        {
            nearMissActive = false;
            gaugePerSec = 0f;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        // 2) Rush 強制終了（速度倍率を戻す）
        rushActive = false;
        rushEndTime = -999f;
        if (runner) runner.SetRushForwardMultiplier(1f);

        // 3) ゲージとクールダウン
        gauge = 0f;
        nearMissEndTime = -999f;
        cooldownUntil = -999f;

        // 4) 接触ゾーン情報をクリア（次のランに持ち越さない）
        overlappedZones.Clear();

        gaugeFillNotified = false;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        float now = Time.unscaledTime;

        if (nearMissActive)
        {
            gauge = Mathf.Clamp(gauge + gaugePerSec * Time.deltaTime, 0f, gaugeMax);

            if (now >= nearMissEndTime)
                EndNearMiss();
        }

        if (!gaugeFillNotified && CanActivateRush)
        {
            gaugeFillNotified = true;
            GameManager.Instance?.NotifyFirstGaugeFull();
        }

        if (rushActive)
        {
            float windDownStart = rushEndTime - rushWindDownDuration;
            if (now >= windDownStart)
            {
                float t = Mathf.Clamp01((now - windDownStart) / rushWindDownDuration);
                runner.SetRushForwardMultiplier(Mathf.Lerp(rushForwardSpeedMultiplier, 1f, rushWindDownCurve.Evaluate(t)));
            }
            if (now >= rushEndTime)
            {
                rushActive = false;
                runner.SetRushForwardMultiplier(1f);
            }
        }
    }

    private void HandleLaneChangeRequested(int fromLane, int toLane)
    {
        if (rushActive) return;
        if (Time.unscaledTime < cooldownUntil) return;
        if (nearMissActive) return;

        NearMissZone zone = GetBestZone();
        if (zone == null) return;

        int dir = Mathf.Clamp(toLane - fromLane, -1, 1);
        StartNearMiss(zone, dir);
    }

    private NearMissZone GetBestZone()
    {
        foreach (var z in overlappedZones)
        {
            if (z != null) return z;
        }
        return null;
    }

    private void StartNearMiss(NearMissZone zone, int direction)
    {
        nearMissActive = true;
        nearMissEndTime = Time.unscaledTime + nearMissDuration;
        cooldownUntil = Time.unscaledTime + nearMissCooldown;

        gaugePerSec = gaugeFillNotified ? nearMissAdrenalinePerSecond : tutorialNearMissAdrenalinePerSecond;

        Time.timeScale = nearMissTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * nearMissTimeScale;

        OnNearMissStarted?.Invoke(direction);
        GameManager.Instance?.NotifyNearMissStarted();
    }

    private void EndNearMiss()
    {
        nearMissActive = false;
        gaugePerSec = 0f;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        OnNearMissEnded?.Invoke();
    }

    public void ActivateRush()
    {
        if (!CanActivateRush) return;

        gauge = 0f;

        rushActive = true;
        rushEndTime = Time.unscaledTime + rushDuration;

        runner.SetRushForwardMultiplier(rushForwardSpeedMultiplier);

        if (GameManager.Instance != null)
            GameManager.Instance.SetInvincibleFor(rushDuration);
    }

    public float RushRemaining01
    {
        get
        {
            if (!rushActive) return 0f;
            float remain = rushEndTime - Time.unscaledTime;
            return Mathf.Clamp01(remain / Mathf.Max(0.001f, rushDuration));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (rushActive) return;

        var zone = other.GetComponent<NearMissZone>();
        if (zone != null)
        {
            overlappedZones.Add(zone);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var zone = other.GetComponent<NearMissZone>();
        if (zone != null) overlappedZones.Remove(zone);
    }
}
