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

    private float originalFixedDeltaTime;

    [Header("Rush (Invincible + Fast)")]
    [SerializeField] private float rushDuration = 5.0f;
    [SerializeField] private float rushForwardSpeedMultiplier = 2.0f;

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

    public bool IsRushActive => rushActive;

    private void Awake()
    {
        runner = GetComponent<RunnerController>();

        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void OnEnable()
    {
        runner.OnLaneChangeRequested += HandleLaneChangeRequested;
    }

    private void OnDisable()
    {
        runner.OnLaneChangeRequested -= HandleLaneChangeRequested;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        if (nearMissActive)
        {
            gauge = Mathf.Clamp(gauge + gaugePerSec * Time.deltaTime, 0f, gaugeMax);

            if (Time.time >= nearMissEndTime)
                EndNearMiss();
        }

        if (rushActive && Time.time >= rushEndTime)
        {
            rushActive = false;
            runner.SetRushForwardMultiplier(1f);
        }
    }

    private void HandleLaneChangeRequested(int fromLane, int toLane)
    {
        if (rushActive) return;
        if (Time.time < cooldownUntil) return;
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
        nearMissEndTime = Time.time + nearMissDuration;
        cooldownUntil = Time.time + nearMissCooldown;

        gaugePerSec = zone.AdrenalinePerSecond;

        Time.timeScale = nearMissTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * nearMissTimeScale;

        OnNearMissStarted?.Invoke(direction);
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
        rushEndTime = Time.time + rushDuration;

        runner.SetRushForwardMultiplier(rushForwardSpeedMultiplier);

        if (GameManager.Instance != null)
            GameManager.Instance.SetInvincibleFor(rushDuration);
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
