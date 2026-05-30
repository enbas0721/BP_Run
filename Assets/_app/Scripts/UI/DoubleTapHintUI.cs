using UnityEngine;

public class DoubleTapHintUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup hintGroup;
    [SerializeField] private AdrenalineSystem adrenaline;
    [SerializeField] private float pulseSpeed = 1.5f;

    private bool isShowing = false;
    private float showStartTime = 0f;

    private void Awake()
    {
        if (!adrenaline) adrenaline = FindFirstObjectByType<AdrenalineSystem>();
        hintGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnFirstGaugeFull += ShowHint;
            GameManager.Instance.OnStateChanged += OnStateChanged;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnFirstGaugeFull -= ShowHint;
            GameManager.Instance.OnStateChanged -= OnStateChanged;
        }
    }

    private void ShowHint()
    {
        isShowing = true;
        showStartTime = Time.unscaledTime;
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.Playing || state == GameState.GameOver)
        {
            isShowing = false;
            hintGroup.alpha = 0f;
        }
    }

    private void Update()
    {
        if (!isShowing) return;

        if (adrenaline.IsRushActive)
        {
            isShowing = false;
            hintGroup.alpha = 0f;
            return;
        }

        float elapsed = (Time.unscaledTime - showStartTime) * pulseSpeed;
        hintGroup.alpha = (-Mathf.Cos(elapsed) + 1f) * 0.5f;
    }
}
