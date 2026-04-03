using UnityEngine;

/// <summary>
/// ループ再生トラック（Bass, Kick, Chord用）。
/// 2つのAudioSourceをダブルバッファリングし、シームレスにクリップを切り替える。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class LoopTrack : MonoBehaviour
{
    [Header("トラック設定")]
    [SerializeField] private string trackName = "Track";
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private int initialClipIndex = 0;

    [Header("音量")]
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private bool muted = false;

    // ===== プロパティ =====
    public string TrackName => trackName;
    public int CurrentClipIndex { get; private set; }
    public int NextClipIndex { get; private set; }
    public int ClipCount => clips != null ? clips.Length : 0;
    public bool IsMuted
    {
        get => muted;
        set
        {
            muted = value;
            ApplyVolume();
        }
    }
    public float Volume
    {
        get => volume;
        set
        {
            volume = Mathf.Clamp01(value);
            ApplyVolume();
        }
    }

    // ===== 内部状態 =====
    private AudioSource[] sources = new AudioSource[2];
    private int activeSourceIndex = 0;
    private MusicManager manager;
    private bool initialized = false;


    /// <summary>
    /// MusicManagerから呼ばれる初期化
    /// </summary>
    public void Initialize(MusicManager mgr)
    {
        manager = mgr;

        // AudioSourceを2つ用意（ダブルバッファ）
        sources[0] = GetComponent<AudioSource>();
        sources[0].Stop();
        sources[0].clip = null;
        sources[0].playOnAwake = false;
        sources[0].loop = false;

        // 2つ目のAudioSourceを追加
        sources[1] = gameObject.AddComponent<AudioSource>();
        sources[1].playOnAwake = false;
        sources[1].loop = false;

        // 出力設定をコピー
        sources[1].outputAudioMixerGroup = sources[0].outputAudioMixerGroup;
        sources[1].spatialBlend = sources[0].spatialBlend;

        CurrentClipIndex = initialClipIndex;
        NextClipIndex = initialClipIndex;

        // 全クリップを事前ロードしてスケジュール時の遅延を防ぐ
        if (clips != null)
        {
            foreach (var clip in clips)
            {
                if (clip != null && clip.loadState == AudioDataLoadState.Unloaded)
                    clip.LoadAudioData();
            }
        }

        ApplyVolume();
        initialized = true;
    }

    /// <summary>
    /// 次のサイクルで再生するクリップを指定する。
    /// ScheduleNext()が呼ばれる前にセットすること。
    /// </summary>
    public void SetNextClip(int clipIndex)
    {
        if (clips == null || clips.Length == 0) return;
        NextClipIndex = Mathf.Clamp(clipIndex, 0, clips.Length - 1);
    }

    /// <summary>
    /// 次のサイクルで再生するクリップを名前で指定する
    /// </summary>
    public void SetNextClipByName(string clipName)
    {
        if (clips == null) return;
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null && clips[i].name == clipName)
            {
                SetNextClip(i);
                return;
            }
        }
        Debug.LogWarning($"[{trackName}] Clip not found: {clipName}");
    }

    /// <summary>
    /// MusicManagerから呼ばれる。指定DSP時刻に次のクリップをスケジュール再生する。
    /// </summary>
    public void ScheduleNext(double dspTime)
    {
        if (!initialized || clips == null || clips.Length == 0) return;

        // バッファを切り替え
        int nextSourceIndex = 1 - activeSourceIndex;
        AudioSource nextSource = sources[nextSourceIndex];

        // クリップをセット
        AudioClip clip = clips[NextClipIndex];
        if (clip == null)
        {
            Debug.LogWarning($"[{trackName}] Clip at index {NextClipIndex} is null");
            return;
        }

        nextSource.clip = clip;
        nextSource.PlayScheduled(dspTime);

        // 現在のソースを予定時刻で停止（clipがない場合はスキップ）
        if (sources[activeSourceIndex].clip != null)
            sources[activeSourceIndex].SetScheduledEndTime(dspTime);

        // 状態を更新
        activeSourceIndex = nextSourceIndex;
        CurrentClipIndex = NextClipIndex;
    }

    /// <summary>
    /// 全AudioSourceを停止
    /// </summary>
    public void StopAll()
    {
        foreach (var src in sources)
        {
            if (src != null) src.Stop();
        }
    }

    private void ApplyVolume()
    {
        float effectiveVolume = muted ? 0f : volume;
        foreach (var src in sources)
        {
            if (src != null) src.volume = effectiveVolume;
        }
    }

    // ===== エディタ用 =====
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (initialized)
        {
            ApplyVolume();
        }
    }
#endif
}
