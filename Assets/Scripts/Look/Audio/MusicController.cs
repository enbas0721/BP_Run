using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// MusicSystemの使用例。
/// ゲームの状態に応じてクリップを切り替えたり、Bellを鳴らしたりするデモ。
/// </summary>
public class MusicController : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private MusicManager musicManager;
    [SerializeField] private LoopTrack bassTrack;
    [SerializeField] private LoopTrack kickTrack;
    [SerializeField] private LoopTrack chordTrack;
    [SerializeField] private BellTrack bellTrack;

    private void OnEnable()
    {
        if (musicManager != null)
        {
            // サイクル（4小節）のスケジューリング直前にクリップを選択する
            musicManager.OnCycleScheduling += OnCycleScheduling;

            // サイクル開始時に何か処理したい場合
            musicManager.OnCycleStarted += OnCycleStarted;

            // 小節ごとの処理
            musicManager.OnBarChanged += OnBarChanged;
        }
    }

    private void OnDisable()
    {
        if (musicManager != null)
        {
            musicManager.OnCycleScheduling -= OnCycleScheduling;
            musicManager.OnCycleStarted -= OnCycleStarted;
            musicManager.OnBarChanged -= OnBarChanged;
        }
    }

    /// <summary>
    /// 次のサイクルで再生するクリップを決める。
    /// ここにゲームロジックに基づくクリップ選択を書く。
    /// </summary>
    private void OnCycleScheduling(int nextCycle)
    {
        // 例: ゲームの状態に応じてクリップを切り替える
        // --------------------------------------------------
        // GameState state = GameStateManager.CurrentState;
        //
        // switch (state)
        // {
        //     case GameState.Calm:
        //         bassTrack.SetNextClip(0);
        //         kickTrack.SetNextClip(0);
        //         chordTrack.SetNextClip(0);
        //         break;
        //     case GameState.Tension:
        //         bassTrack.SetNextClip(1);
        //         kickTrack.SetNextClip(1);
        //         chordTrack.SetNextClip(1);
        //         break;
        //     case GameState.Battle:
        //         bassTrack.SetNextClip(2);
        //         kickTrack.SetNextClip(2);
        //         chordTrack.SetNextClip(2);
        //         break;
        // }

        // デモ: サイクル番号に応じてクリップを切り替え
        int clipIndex = nextCycle % bassTrack.ClipCount;
        bassTrack.SetNextClip(clipIndex);

        clipIndex = nextCycle % kickTrack.ClipCount;
        kickTrack.SetNextClip(clipIndex);

        clipIndex = nextCycle % chordTrack.ClipCount;
        chordTrack.SetNextClip(clipIndex);

        Debug.Log($"[Music] Cycle {nextCycle} scheduled");
    }

    private void OnCycleStarted(int cycle)
    {
        Debug.Log($"[Music] Cycle {cycle} started");
    }

    private void OnBarChanged(int bar)
    {
        // 4小節中の何小節目か
        int barInCycle = bar % 4;
        Debug.Log($"[Music] Bar {bar} (cycle bar: {barInCycle})");
    }

    private void Update()
    {
        // ===== Bell のトリガー例 =====

        // キー入力で即座にBellを鳴らす
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame)
        {
            bellTrack.Play(0);
        }
        if (kb.digit2Key.wasPressedThisFrame)
        {
            bellTrack.Play(1);
        }
        if (kb.digit3Key.wasPressedThisFrame)
        {
            bellTrack.Play(2);
        }

        // Spaceキーで次の拍頭に合わせてBellを鳴らす（クオンタイズ）
        if (kb.spaceKey.wasPressedThisFrame)
        {
            bellTrack.PlayOnNextBeat(0);
        }

        // ===== ミュート制御の例 =====
        if (kb.bKey.wasPressedThisFrame)
        {
            bassTrack.IsMuted = !bassTrack.IsMuted;
            Debug.Log($"Bass muted: {bassTrack.IsMuted}");
        }
        if (kb.kKey.wasPressedThisFrame)
        {
            kickTrack.IsMuted = !kickTrack.IsMuted;
            Debug.Log($"Kick muted: {kickTrack.IsMuted}");
        }
        if (kb.cKey.wasPressedThisFrame)
        {
            chordTrack.IsMuted = !chordTrack.IsMuted;
            Debug.Log($"Chord muted: {chordTrack.IsMuted}");
        }

        // ===== 再生/停止制御 =====
        if (kb.pKey.wasPressedThisFrame)
        {
            if (musicManager.IsPlaying)
                musicManager.Stop();
            else
                musicManager.Play();
        }
    }

    // ===== 外部から呼べるAPI =====

    /// <summary>
    /// シーン切り替え等で全トラックのクリップを一括変更する。
    /// 次のサイクル境界で反映される。
    /// </summary>
    public void ChangeScene(int bassClip, int kickClip, int chordClip)
    {
        bassTrack.SetNextClip(bassClip);
        kickTrack.SetNextClip(kickClip);
        chordTrack.SetNextClip(chordClip);
    }

    /// <summary>
    /// Bellを任意タイミングで鳴らす（外部呼び出し用）
    /// </summary>
    public void TriggerBell(int clipIndex)
    {
        bellTrack.Play(clipIndex);
    }

    /// <summary>
    /// Bellを次の拍頭で鳴らす（外部呼び出し用）
    /// </summary>
    public void TriggerBellQuantized(int clipIndex)
    {
        bellTrack.PlayOnNextBeat(clipIndex);
    }
}
