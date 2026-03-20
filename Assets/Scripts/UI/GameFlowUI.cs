using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameFlowUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject resultPanel;

    [Header("Result UI")]
    [SerializeField] private TMP_Text resultScoreText;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;

    private void Awake()
    {
        if (startButton) startButton.onClick.AddListener(OnStartClicked);
        if (retryButton) retryButton.onClick.AddListener(OnRetryClicked);
        if (pauseButton) pauseButton.onClick.AddListener(OnPauseClicked);
        if (resumeButton) resumeButton.onClick.AddListener(OnResumeClicked);
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            GameManager.Instance.OnReadyToShowResult += ShowResult;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance  != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            GameManager.Instance.OnReadyToShowResult -= ShowResult;
        }
    }

    private void Start()
    {
        ShowTitle();
    }

    private void OnStartClicked()
    {
        GameManager.Instance.StartRun();
    }

    private void OnRetryClicked()
    {
        GameManager.Instance.ResetRun();
    }

    private void OnPauseClicked()
    {
        GameManager.Instance.Pause();
    }

    private void OnResumeClicked()
    {
        GameManager.Instance.Resume();
    }

    private void HandleStateChanged(GameState state)
    {
        switch(state)
        {
            case GameState.Ready:
                ShowTitle();
                break;

            case GameState.Playing:
                ShowGame();
                SetPauseButtonVisible(true);
                break;

            case GameState.Paused:
                SetPauseButtonVisible(false);
                break;

            case GameState.GameOver:
                break;
        }
    }

    private void ShowTitle()
    {
        if (titlePanel) titlePanel.SetActive(true);
        if (gamePanel) gamePanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
    }

    private void ShowGame()
    {
        if (titlePanel) titlePanel.SetActive(false);
        if (gamePanel) gamePanel.SetActive(true);
        if (resultPanel) resultPanel.SetActive(false);
        SetPauseButtonVisible(true);
    }

    private void SetPauseButtonVisible(bool pausing)
    {
        if (pauseButton) pauseButton.gameObject.SetActive(pausing);
        if (resumeButton) resumeButton.gameObject.SetActive(!pausing);
    }

    private void ShowResult()
    {
        if (titlePanel) titlePanel.SetActive(false);
        if (gamePanel) gamePanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(true);

        if (resultScoreText && GameManager.Instance != null)
        {
            int score = GameManager.Instance.ScoreSystem.Score;
            resultScoreText.text = score.ToString();
        }
    }
    private void HideAll()
    {
        if (titlePanel) titlePanel.SetActive(false);
        if (gamePanel) gamePanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);

    }
}
