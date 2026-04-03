using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bell用トラック。任意タイミングでワンショット再生する。
/// PlayOneShotによるポリフォニー（同時発音）対応。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BellTrack : MonoBehaviour
{
    [Header("Bell設定")]
    [SerializeField] private AudioClip[] clips;

    [Header("音量")]
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private bool muted = false;

    [Header("ポリフォニー設定")]
    [Tooltip("同時発音数の上限。0=無制限")]
    [SerializeField] private int maxPolyphony = 8;

    // ===== プロパティ =====
    public int ClipCount => clips != null ? clips.Length : 0;
    public bool IsMuted
    {
        get => muted;
        set
        {
            muted = value;
            mainSource.volume = muted ? 0f : volume;
        }
    }
    public float Volume
    {
        get => volume;
        set
        {
            volume = Mathf.Clamp01(value);
            if (!muted) mainSource.volume = volume;
        }
    }

    // ===== 内部状態 =====
    private AudioSource mainSource;
    private MusicManager manager;

    // ポリフォニー管理用の追加AudioSource
    private List<AudioSource> polySources = new List<AudioSource>();
    private int polyRoundRobin = 0;

    public void Initialize(MusicManager mgr)
    {
        manager = mgr;

        mainSource = GetComponent<AudioSource>();
        mainSource.playOnAwake = false;
        mainSource.loop = false;
        mainSource.volume = muted ? 0f : volume;

        // 全クリップを事前ロードしてスケジュール時の遅延を防ぐ
        if (clips != null)
        {
            foreach (var clip in clips)
            {
                if (clip != null && clip.loadState == AudioDataLoadState.Unloaded)
                    clip.LoadAudioData();
            }
        }

        // ポリフォニー用のAudioSourceをプールとして作成
        if (maxPolyphony > 1)
        {
            // mainSourceも1つとしてカウント
            for (int i = 1; i < maxPolyphony; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = false;
                src.volume = mainSource.volume;
                src.outputAudioMixerGroup = mainSource.outputAudioMixerGroup;
                src.spatialBlend = mainSource.spatialBlend;
                polySources.Add(src);
            }
        }
    }

    /// <summary>
    /// インデックスを指定して即座に再生する
    /// </summary>
    public void Play(int clipIndex)
    {
        if (clips == null || clips.Length == 0) return;
        clipIndex = Mathf.Clamp(clipIndex, 0, clips.Length - 1);

        AudioClip clip = clips[clipIndex];
        if (clip == null) return;

        GetAvailableSource().PlayOneShot(clip, muted ? 0f : volume);
    }

    /// <summary>
    /// クリップ名を指定して即座に再生する
    /// </summary>
    public void Play(string clipName)
    {
        if (clips == null) return;
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null && clips[i].name == clipName)
            {
                Play(i);
                return;
            }
        }
        Debug.LogWarning($"[Bell] Clip not found: {clipName}");
    }

    /// <summary>
    /// 指定DSP時刻にスケジュール再生する（拍に合わせたい場合用）
    /// </summary>
    public void PlayScheduled(int clipIndex, double dspTime)
    {
        if (clips == null || clips.Length == 0) return;
        clipIndex = Mathf.Clamp(clipIndex, 0, clips.Length - 1);

        AudioClip clip = clips[clipIndex];
        if (clip == null) return;

        AudioSource src = GetAvailableSource();
        src.clip = clip;
        src.volume = muted ? 0f : volume;
        src.PlayScheduled(dspTime);
    }

    /// <summary>
    /// 次の拍頭に合わせて再生する
    /// </summary>
    public void PlayOnNextBeat(int clipIndex)
    {
        if (manager == null || !manager.IsPlaying) 
        {
            Play(clipIndex);
            return;
        }

        double elapsed = AudioSettings.dspTime - manager.StartDspTime;
        double beatPos = elapsed % manager.BeatDuration;
        double timeToNextBeat = manager.BeatDuration - beatPos;
        double scheduledTime = AudioSettings.dspTime + timeToNextBeat;

        PlayScheduled(clipIndex, scheduledTime);
    }

    /// <summary>
    /// すべてのBell音を停止する
    /// </summary>
    public void StopAll()
    {
        mainSource.Stop();
        foreach (var src in polySources)
        {
            src.Stop();
        }
    }

    private AudioSource GetAvailableSource()
    {
        if (maxPolyphony <= 1 || polySources.Count == 0)
            return mainSource;

        // ラウンドロビンで分配
        // mainSource + polySources の中から順番に使う
        int totalSources = 1 + polySources.Count;
        int index = polyRoundRobin % totalSources;
        polyRoundRobin = (polyRoundRobin + 1) % totalSources;

        if (index == 0) return mainSource;
        return polySources[index - 1];
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (mainSource != null)
        {
            mainSource.volume = muted ? 0f : volume;
        }
    }
#endif
}
