using UnityEngine;
using CriWare;
using CriWare.Assets;

/// <summary>
/// スコアに応じてBGMのBass/Chordセレクタラベルを切り替えるコントローラー。
/// ADXのバーティカルリミックス（セレクタ切り替え）を制御する。
/// </summary>
public class MusicIntensityController : MonoBehaviour
{
    [System.Serializable]
    public class IntensityTrack
    {
        [Tooltip("このトラックに切り替わるスコアの閾値")]
        public int scoreThreshold;
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

    [Header("セレクタ名")]
    [SerializeField] private string bassSelectorName = "Bass_Selector";
    [SerializeField] private string chordSelectorName = "Chord_Selector";

    private int currentTrackIndex = -1;
    private CriAtomExPlayback bgmPlayback;

    private void Start()
    {
        if (bgmSource == null || tracks == null || tracks.Length == 0) return;

        // 再生前に初期ラベルを設定してからPlay
        currentTrackIndex = 0;
        bgmSource.player.SetSelectorLabel(bassSelectorName, tracks[0].bassLabel);
        bgmSource.player.SetSelectorLabel(chordSelectorName, tracks[0].chordLabel);
        bgmPlayback = bgmSource.Play();

        // Start後にイベント購読（GameManagerがStart時点で確実に存在するため）
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += OnScoreChanged;
            GameManager.Instance.OnItemCollected += PlayBell;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= OnScoreChanged;
            GameManager.Instance.OnItemCollected -= PlayBell;
        }
    }

    private void OnScoreChanged(int score)
    {
        int newTrackIndex = ResolveTrack(score);
        if (newTrackIndex == currentTrackIndex) return;

        currentTrackIndex = newTrackIndex;
        ApplyTrack(tracks[currentTrackIndex]);
    }

    /// <summary>
    /// スコアに対応するトラックインデックスを返す。
    /// 閾値を超えた最後のトラックを採用する。
    /// </summary>
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

        bgmSource.player.SetSelectorLabel(bassSelectorName, track.bassLabel);
        bgmSource.player.SetSelectorLabel(chordSelectorName, track.chordLabel);
        bgmSource.player.Update(bgmPlayback);
    }

    /// <summary>
    /// ゲームリセット時にIntensityを初期状態に戻す
    /// </summary>
    public void ResetIntensity()
    {
        currentTrackIndex = -1;
        if (tracks == null || tracks.Length == 0) return;
        ApplyTrack(tracks[0]);
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
