using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Distance Score")]
    [Tooltip("1秒あたりのスコア増加量")]
    [SerializeField] private float scorePerSecond = 10f;

    public int Score { get; private set; }

    private float scoreFloat;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        RefreshUI();
    }

    private void Update()
    {
        /* 
         * [TODO] timeScaleではなくゲーム状態を
         * 管理するスクリプト（でき次第）からとってくる
         */
        if (Time.timeScale <= 0f) return;

        scoreFloat += scorePerSecond * Time.deltaTime;

        int newScore = Mathf.FloorToInt(scoreFloat);
        if (newScore != Score)
        {
            Score = newScore;
            RefreshUI();
        }
    }

    /* アイテム取得など外部からスコアアップするAPI */
    public void AddScore(int amount)
    {
        scoreFloat += amount;
        Score = Mathf.FloorToInt(scoreFloat);
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (scoreText) scoreText.text = Score.ToString();
    }
}
