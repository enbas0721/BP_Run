using System.Collections;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShareButton : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void JS_Share(byte[] data, int length);
#endif

    [SerializeField] private Button button;

    [Header("Share UI")]
    [SerializeField] private GameObject shareUI;         // ShareUI ルートオブジェクト
    [SerializeField] private Transform backgroundParent; // Background オブジェクト（6枚の Image の親）
    [SerializeField] private TMP_Text scoreNumText;      // ScoreNum
    [SerializeField] private TMP_Text bestScoreText;     // BestScore
    [SerializeField] private TMP_Text dateDayText;       // date
    [SerializeField] private TMP_Text dateMonthText;     // month
    [SerializeField] private TMP_Text dateYearText;      // year

    private static readonly string[] MonthNames =
        { "JAN", "FEB", "MAR", "APR", "MAY", "JUN",
          "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };

    private void Awake()
    {
        if (shareUI) shareUI.SetActive(false);
    }

    public void ExecuteShare()
    {
        StartCoroutine(ShareCoroutine());
    }

    private IEnumerator ShareCoroutine()
    {
        // スコアをセット
        int score = GameManager.Instance != null ? GameManager.Instance.ScoreSystem.Score : 0;
        int best  = GameManager.Instance != null ? GameManager.Instance.BestScore : 0;
        if (scoreNumText) scoreNumText.text = score.ToString();
        if (bestScoreText) bestScoreText.text = best.ToString();

        // 日付をセット
        var now = System.DateTime.Now;
        if (dateDayText)   dateDayText.text   = now.Day.ToString("D2");
        if (dateMonthText) dateMonthText.text  = MonthNames[now.Month - 1];
        if (dateYearText)  dateYearText.text   = now.Year.ToString();

        // ランダム背景を選択
        SetRandomBackground();

        // ShareUI を一時有効化してスクリーンショット
        shareUI.SetActive(true);

        yield return new WaitForEndOfFrame();

        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        byte[] png = screenshot.EncodeToPNG();
        Destroy(screenshot);

        // シェア後に ShareUI を非表示
        shareUI.SetActive(false);

#if UNITY_WEBGL && !UNITY_EDITOR
        JS_Share(png, png.Length);
#else
        Debug.Log($"[ShareButton] Score={score} Best={best} PNG={png.Length} bytes");
#endif
    }

    private void SetRandomBackground()
    {
        if (!backgroundParent) return;
        int count = backgroundParent.childCount;
        if (count == 0) return;

        int selected = Random.Range(0, count);
        for (int i = 0; i < count; i++)
            backgroundParent.GetChild(i).gameObject.SetActive(i == selected);
    }
}
