using UnityEngine;
using CriWare;
using CriWare.Assets;

/// <summary>
/// スコアに応じてBGMのセレクタラベルを切り替えるコントローラー。
/// ADXのバーティカルリミックス（セレクタ切り替え）を制御する。
/// usePhaseMode=trueの場合はPhaseセレクタ1つで切り替え、falseの場合はBass/Chord2つで切り替える。
/// </summary>
public class MusicIntensityController : MonoBehaviour
{
    [System.Serializable]
    public class IntensityTrack
    {
        [Tooltip("このトラックに切り替わるスコアの閾値")]
        public int scoreThreshold;

        [Header("Phase Mode")]
        [Tooltip("Phase_Selectorに設定するラベル（例: phase_0〜phase_4）")]
        public string phaseLabel = "phase_0";

        [Header("Bass/Chord/Kick Mode")]
        [Tooltip("Kick_Selectorに設定するラベル（例: kick_1〜kick_5）")]
        public string kickLabel = "kick_1";
        [Tooltip("Bass_Selectorに設定するラベル（例: bass_silent, bass_1〜bass_4）")]
        public string bassLabel = "bass_silent";
        [Tooltip("Chord_Selectorに設定するラベル（例: chord_silent, chord_1〜chord_4）")]
        public string chordLabel = "chord_silent";
    }

    [Header("BGMプレーヤー")]
    [Tooltip("BGM再生用のCriAtomSourceコンポーネント")]
    [SerializeField] private CriAtomSourceForAsset bgmSource;
    [Tooltip("Bell再生用のCriAtomSourceコンポーネント（BGMと別インスタンス）")]
    [SerializeField] private CriAtomSourceForAsset bellSource;

    [Header("Intensity Tracks")]
    [Tooltip("スコア閾値とセレクタラベルのマッピング（スコア昇順で設定する）")]
    [SerializeField] private IntensityTrack[] tracks;

    [Header("切り替えモード")]
    [Tooltip("trueの場合はPhaseセレクタ1つで切り替え、falseの場合はKick/Bass/Chord3つで切り替える")]
    [SerializeField] private bool usePhaseMode = false;

    [Header("セレクタ名")]
    [SerializeField] private string phaseSelectorName = "Phase_Selector";
    [SerializeField] private string kickSelectorName = "Kick_Selector";
    [SerializeField] private string bassSelectorName = "Bass_Selector";
    [SerializeField] private string chordSelectorName = "Chord_Selector";

    private int currentTrackIndex = -1;
    private CriAtomExPlayback bgmPlayback;
    private bool tutorialActive = true;

    private void Start()
    {
        StartBgm();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += OnScoreChanged;
            GameManager.Instance.OnItemCollected += PlayBell;
            GameManager.Instance.OnTutorialEnded += OnTutorialEnded;
            GameManager.Instance.OnStateChanged += OnStateChanged;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= OnScoreChanged;
            GameManager.Instance.OnItemCollected -= PlayBell;
            GameManager.Instance.OnTutorialEnded -= OnTutorialEnded;
            GameManager.Instance.OnStateChanged -= OnStateChanged;
        }
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.GameOver)
            bgmSource?.Stop();
    }

    private void OnTutorialEnded()
    {
        tutorialActive = false;
        int newTrackIndex = ResolveTrack(GameManager.Instance.ScoreSystem.Score);
        if (newTrackIndex == currentTrackIndex) return;

        currentTrackIndex = newTrackIndex;
        ApplyTrack(tracks[currentTrackIndex]);
    }

    private void OnScoreChanged(int score)
    {
        if (tutorialActive) return;

        int newTrackIndex = ResolveTrack(score);
        if (newTrackIndex == currentTrackIndex) return;

        currentTrackIndex = newTrackIndex;
        ApplyTrack(tracks[currentTrackIndex]);
    }

    private int ResolveTrack(int score)
    {
        int result = 0;
        for (int i = 0; i < tracks.Length; i++)
        {
            if (score >= tracks[i].scoreThreshold)
                result = i;
        }
        return result;
    }

    private void ApplyTrack(IntensityTrack track)
    {
        if (bgmSource == null) return;

        if (usePhaseMode)
        {
            bgmSource.player.SetSelectorLabel(phaseSelectorName, track.phaseLabel);
        }
        else
        {
            bgmSource.player.SetSelectorLabel(kickSelectorName, track.kickLabel);
            bgmSource.player.SetSelectorLabel(bassSelectorName, track.bassLabel);
            bgmSource.player.SetSelectorLabel(chordSelectorName, track.chordLabel);
        }

        bgmSource.player.Update(bgmPlayback);
    }

    /// <summary>
    /// BGMを初期状態（tracks[0]）から再生する。Start時およびリセット時に使用。
    /// </summary>
    public void StartBgm()
    {
        if (bgmSource == null || tracks == null || tracks.Length == 0) return;

        bgmSource.Stop();
        currentTrackIndex = 0;

        if (usePhaseMode)
        {
            bgmSource.player.SetSelectorLabel(phaseSelectorName, tracks[0].phaseLabel);
        }
        else
        {
            bgmSource.player.SetSelectorLabel(kickSelectorName, tracks[0].kickLabel);
            bgmSource.player.SetSelectorLabel(bassSelectorName, tracks[0].bassLabel);
            bgmSource.player.SetSelectorLabel(chordSelectorName, tracks[0].chordLabel);
        }

        bgmPlayback = bgmSource.Play();
    }

    /// <summary>
    /// ゲームリセット時にIntensityを初期状態に戻して再生し直す
    /// </summary>
    public void ResetIntensity()
    {
        tutorialActive = true;
        StartBgm();
    }

    /// <summary>
    /// Bellをワンショット再生する（ポイント取得時などに呼ぶ）
    /// </summary>
    public void PlayBell()
    {
        if (bellSource == null) return;
        bellSource.Play();
    }
}
