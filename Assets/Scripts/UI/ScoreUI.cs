using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;

    private void OnEnable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScoreChanged += RefreshUI;

        RefreshUI(GameManager.Instance.ScoreSystem.Score);
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScoreChanged -= RefreshUI;
    }

    private void RefreshUI(int score)
    {
        if (scoreText) scoreText.text = score.ToString();
    }

}
