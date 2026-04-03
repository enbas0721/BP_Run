using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 音楽制御の中枢。BPM管理・小節カウント・トラック同期を担当。
/// 空のGameObjectにアタッチして使用する。
/// </summary>
public class MusicManager : MonoBehaviour
{
    // ===== 設定 =====
    [Header("BPM設定")]
    [SerializeField] private float bpm = 120f;
    [SerializeField] private int beatsPerBar = 4;
    [SerializeField] private int barsPerClip = 4;

    [Header("トラック設定")]
    [SerializeField] private LoopTrack bassTrack;
    [SerializeField] private LoopTrack kickTrack;
    [SerializeField] private LoopTrack chordTrack;
    [SerializeField] private BellTrack bellTrack;

    [Header("再生制御")]
    [SerializeField] private bool playOnAwake = true;

    // ===== プロパティ =====
    /// <summary>現在のBPM</summary>
    public float BPM => bpm;

    /// <summary>1クリップ（4小節）の長さ（秒）</summary>
    public double ClipDuration => 60.0 / bpm * beatsPerBar * barsPerClip;

    /// <summary>1拍の長さ（秒）</summary>
    public double BeatDuration => 60.0 / bpm;

    /// <summary>1小節の長さ（秒）</summary>
    public double BarDuration => 60.0 / bpm * beatsPerBar;

    /// <summary>再生開始時刻（DSP時間）</summary>
    public double StartDspTime { get; private set; }

    /// <summary>現在の小節番号（0始まり）</summary>
    public int CurrentBar { get; private set; }

    /// <summary>現在のクリップサイクル番号（0始まり）</summary>
    public int CurrentCycle { get; private set; }

    /// <summary>再生中かどうか</summary>
    public bool IsPlaying { get; private set; }

    // ===== イベント =====
    /// <summary>新しいクリップサイクルが始まる直前に発火（次サイクルのクリップを予約するタイミング）</summary>
    public event Action<int> OnCycleScheduling;

    /// <summary>新しいクリップサイクルが開始した瞬間に発火</summary>
    public event Action<int> OnCycleStarted;

    /// <summary>小節が切り替わった瞬間に発火</summary>
    public event Action<int> OnBarChanged;

    // ===== 内部状態 =====
    private double nextCycleDspTime;
    private int lastNotifiedBar = -1;

    // スケジューリングの先読み時間（秒）
    // この時間だけ前もってクリップ切替をスケジュールする
    private const double SCHEDULE_AHEAD = 0.2;

    // ===== ループトラック一覧 =====
    private List<LoopTrack> loopTracks = new List<LoopTrack>();

    private void Awake()
    {
        // LoopTrackを収集
        if (bassTrack != null) loopTracks.Add(bassTrack);
        if (kickTrack != null) loopTracks.Add(kickTrack);
        if (chordTrack != null) loopTracks.Add(chordTrack);

        // 各トラックにManagerの参照を渡す
        foreach (var track in loopTracks)
        {
            track.Initialize(this);
        }

        if (bellTrack != null)
        {
            bellTrack.Initialize(this);
        }
    }

    private void Start()
    {
        if (playOnAwake)
        {
            Play();
        }
    }

    /// <summary>
    /// 全トラックの同期再生を開始する
    /// </summary>
    public void Play()
    {
        if (IsPlaying) return;

        IsPlaying = true;
        CurrentBar = 0;
        CurrentCycle = 0;
        lastNotifiedBar = -1;

        // 少し先の時刻から再生開始（バッファ確保）
        StartDspTime = AudioSettings.dspTime + 0.5;
        nextCycleDspTime = StartDspTime;

        // 最初のサイクルをスケジュール
        ScheduleNextCycle();
    }

    /// <summary>
    /// 全トラックの再生を停止する
    /// </summary>
    public void Stop()
    {
        if (!IsPlaying) return;

        IsPlaying = false;

        foreach (var track in loopTracks)
        {
            track.StopAll();
        }
    }

    private void Update()
    {
        if (!IsPlaying) return;

        double currentDsp = AudioSettings.dspTime;

        // --- 小節カウントの更新 ---
        double elapsed = currentDsp - StartDspTime;
        if (elapsed >= 0)
        {
            int bar = (int)(elapsed / BarDuration);
            if (bar != lastNotifiedBar)
            {
                lastNotifiedBar = bar;
                CurrentBar = bar;
                OnBarChanged?.Invoke(bar);
            }
        }

        // サイクル境界を超えたらサイクル番号を更新
        double cycleElapsed = currentDsp - StartDspTime;
        if (cycleElapsed >= 0)
        {
            int newCycle = (int)(cycleElapsed / ClipDuration);
            if (newCycle > CurrentCycle)
            {
                CurrentCycle = newCycle;
                OnCycleStarted?.Invoke(CurrentCycle);
            }
        }

        // 次サイクルの先読みスケジューリング
        // nextCycleDspTime は ScheduleNextCycle 内で即座に進めるため多重呼び出しは発生しない
        if (currentDsp >= nextCycleDspTime - SCHEDULE_AHEAD)
        {
            ScheduleNextCycle();
        }
    }

    private void ScheduleNextCycle()
    {
        // 先に時刻を進めて多重呼び出しを防ぐ
        double scheduleDspTime = nextCycleDspTime;
        nextCycleDspTime += ClipDuration;

        if (loopTracks.Count == 0)
        {
            Debug.LogWarning("[MusicManager] loopTracksが空です。InspectorでLoopTrackを割り当ててください。");
            return;
        }

        // イベントで外部にクリップ選択の機会を与える
        try
        {
            OnCycleScheduling?.Invoke(CurrentCycle + 1);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MusicManager] OnCycleScheduling で例外発生: {e.Message}");
        }

        // 各LoopTrackに次のクリップを予約再生させる
        foreach (var track in loopTracks)
        {
            track.ScheduleNext(scheduleDspTime);
        }
    }

    // ===== ユーティリティ =====

    /// <summary>
    /// 次の小節境界までの残り時間（秒）
    /// </summary>
    public double TimeToNextBar()
    {
        if (!IsPlaying) return 0;
        double elapsed = AudioSettings.dspTime - StartDspTime;
        double barPos = elapsed % BarDuration;
        return BarDuration - barPos;
    }

    /// <summary>
    /// 次のクリップ境界までの残り時間（秒）
    /// </summary>
    public double TimeToNextCycle()
    {
        if (!IsPlaying) return 0;
        double elapsed = AudioSettings.dspTime - StartDspTime;
        double cyclePos = elapsed % ClipDuration;
        return ClipDuration - cyclePos;
    }

    /// <summary>
    /// 現在のクリップ内での進行度（0.0〜1.0）
    /// </summary>
    public float CycleProgress()
    {
        if (!IsPlaying) return 0f;
        double elapsed = AudioSettings.dspTime - StartDspTime;
        double cyclePos = elapsed % ClipDuration;
        return (float)(cyclePos / ClipDuration);
    }
}
