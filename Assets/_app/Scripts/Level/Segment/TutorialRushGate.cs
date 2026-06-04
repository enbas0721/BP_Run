using System.Collections;
using UnityEngine;

/// <summary>
/// Rush チュートリアルゲート。
/// 未習得プレイヤーがトリガーに入ると前進を停止し、ゲージを満タンにしてダブルタップを促す。
/// Rush 発動を検知したら前進を再開し、習得済みフラグを保存する。
/// Rush 中に通過した場合はそのまま何もしない。
/// </summary>
public class TutorialRushGate : MonoBehaviour
{
    [Tooltip("前進停止までの減速時間（秒）")]
    [SerializeField] private float decelerationTime = 0.4f;

    private AdrenalineSystem adrenaline;
    private RunnerController runner;
    private bool triggered = false;

    private void Awake()
    {
        adrenaline = FindFirstObjectByType<AdrenalineSystem>();
        runner     = FindFirstObjectByType<RunnerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        if (adrenaline.IsRushActive) return;
        if (GameManager.Instance != null && GameManager.Instance.RushTutorialDone) return;

        triggered = true;
        StartCoroutine(GateSequence());
    }

    private IEnumerator GateSequence()
    {
        GameManager.Instance?.NotifyGateEntered();

        // 1. decelerationTime かけて前進を 0 に減速
        float elapsed = 0f;
        while (elapsed < decelerationTime)
        {
            elapsed += Time.deltaTime;
            runner.SetForceStopMultiplier(Mathf.Lerp(1f, 0f, elapsed / decelerationTime));
            yield return null;
        }
        runner.SetForceStopMultiplier(0f);
        GameManager.Instance?.SetForceStopped(true);

        // 2. ゲージが未満タンなら強制チャージ（DoubleTapHintUI が自動表示される）
        if (adrenaline.Gauge < adrenaline.GaugeMax)
            adrenaline.ForceFullGauge();

        // 3. Rush 発動を待つ
        while (!adrenaline.IsRushActive)
            yield return null;

        // 4. 前進再開・フラグ保存
        GameManager.Instance?.SetForceStopped(false);
        runner.SetForceStopMultiplier(1f);
        GameManager.Instance?.SetRushTutorialDone();
        enabled = false;
    }
}
