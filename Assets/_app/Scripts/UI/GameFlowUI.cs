using System;
using System.Collections;
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
    [SerializeField] private TMP_Text resultBestScoreText;

    [Header("Game UI")]
    [SerializeField] private TMP_Text gameBestScoreText;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button shareButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;

    [Header("Confirm Button Settings")]
    [Tooltip("確認状態に切り替わるときのスプライト（retry）")]
    [SerializeField] private Sprite retryConfirmSprite;
    [Tooltip("確認状態に切り替わるときのスプライト（share）")]
    [SerializeField] private Sprite shareConfirmSprite;
    [Tooltip("確認待ちがリセットされるまでの秒数")]
    [SerializeField] private float confirmTimeout = 2f;

    private Sprite retryNormalSprite;
    private Sprite shareNormalSprite;
    private Button pendingButton;
    private Coroutine confirmCoroutine;

    private void Awake()
    {
        if (startButton)  startButton.onClick.AddListener(OnStartClicked);
        if (pauseButton)  pauseButton.onClick.AddListener(OnPauseClicked);
        if (resumeButton) resumeButton.onClick.AddListener(OnResumeClicked);

        if (retryButton)
        {
            retryNormalSprite = retryButton.GetComponent<Image>()?.sprite;
            retryButton.onClick.AddListener(() => OnConfirmClicked(retryButton, retryConfirmSprite, () => GameManager.Instance.ResetRun()));
        }
        if (shareButton)
        {
            shareNormalSprite = shareButton.GetComponent<Image>()?.sprite;
            shareButton.onClick.AddListener(() => OnConfirmClicked(shareButton, shareConfirmSprite, OnShareConfirmed));
        }
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            GameManager.Instance.OnReadyToShowResult += ShowResult;
            GameManager.Instance.OnBestScoreChanged += UpdateBestScoreTexts;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance  != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            GameManager.Instance.OnReadyToShowResult -= ShowResult;
            GameManager.Instance.OnBestScoreChanged -= UpdateBestScoreTexts;
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

    private void OnConfirmClicked(Button btn, Sprite confirmSprite, Action onConfirmed)
    {
        if (pendingButton == btn)
        {
            // 2度目：確定
            ResetConfirmState();
            GameManager.Instance.NotifyResultUIDecided();
            onConfirmed?.Invoke();
            return;
        }

        // 別ボタンが確認中なら先にリセット
        if (pendingButton != null)
            ResetConfirmState();

        // 1度目：確認状態へ
        pendingButton = btn;
        var img = btn.GetComponent<Image>();
        if (img && confirmSprite) img.sprite = confirmSprite;
        GameManager.Instance.NotifyResultUISelected();

        if (confirmCoroutine != null) StopCoroutine(confirmCoroutine);
        confirmCoroutine = StartCoroutine(ConfirmTimeoutRoutine());
    }

    private IEnumerator ConfirmTimeoutRoutine()
    {
        yield return new WaitForSecondsRealtime(confirmTimeout);
        ResetConfirmState();
    }

    private void ResetConfirmState()
    {
        if (pendingButton == retryButton)
        {
            var img = retryButton.GetComponent<Image>();
            if (img && retryNormalSprite) img.sprite = retryNormalSprite;
        }
        else if (pendingButton == shareButton)
        {
            var img = shareButton.GetComponent<Image>();
            if (img && shareNormalSprite) img.sprite = shareNormalSprite;
        }
        pendingButton = null;
        if (confirmCoroutine != null) { StopCoroutine(confirmCoroutine); confirmCoroutine = null; }
    }

    private void OnShareConfirmed()
    {
        shareButton?.GetComponent<ShareButton>()?.ExecuteShare();
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
        UpdateBestScoreTexts(GameManager.Instance != null ? GameManager.Instance.BestScore : 0);
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

        if (GameManager.Instance != null)
        {
            if (resultScoreText)
                resultScoreText.SetText("{0}", GameManager.Instance.ScoreSystem.Score);
            UpdateBestScoreTexts(GameManager.Instance.BestScore);
        }
    }

    private void UpdateBestScoreTexts(int bestScore)
    {
        if (gameBestScoreText) gameBestScoreText.SetText("{0}", bestScore);
        if (resultBestScoreText) resultBestScoreText.SetText("{0}", bestScore);
    }
    private void HideAll()
    {
        if (titlePanel) titlePanel.SetActive(false);
        if (gamePanel) gamePanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);

    }
}
