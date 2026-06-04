using System.Collections;
using UnityEngine;

public class DoubleTapHintUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup hintGroup;
    [SerializeField] private AdrenalineSystem adrenaline;
    [SerializeField] private float pulseSpeed = 1.5f;

    [Header("Gate Scale")]
    [SerializeField] private float gateScale = 1.8f;
    [SerializeField] private float gateScaleDuration = 0.4f;
    [SerializeField] private AnimationCurve gateScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isShowing = false;
    private float showStartTime = 0f;
    private Coroutine scaleCoroutine;

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
            GameManager.Instance.OnForceStopChanged += OnForceStopChanged;
            GameManager.Instance.OnGateEntered += OnGateEntered;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnFirstGaugeFull -= ShowHint;
            GameManager.Instance.OnStateChanged -= OnStateChanged;
            GameManager.Instance.OnForceStopChanged -= OnForceStopChanged;
            GameManager.Instance.OnGateEntered -= OnGateEntered;
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

    private void OnGateEntered()
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(gateScale));
    }

    private void OnForceStopChanged(bool stopped)
    {
        if (stopped) return;
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(1f));
    }

    private IEnumerator ScaleTo(float targetScale)
    {
        Vector3 fromScale = hintGroup.transform.localScale;
        Vector3 toScale = Vector3.one * targetScale;
        float elapsed = 0f;
        while (elapsed < gateScaleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = gateScaleCurve.Evaluate(Mathf.Clamp01(elapsed / gateScaleDuration));
            hintGroup.transform.localScale = Vector3.Lerp(fromScale, toScale, t);
            yield return null;
        }
        hintGroup.transform.localScale = toScale;
        scaleCoroutine = null;
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
