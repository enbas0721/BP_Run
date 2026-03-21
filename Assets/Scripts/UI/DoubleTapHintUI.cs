using UnityEngine;

public class DoubleTapHintUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup hintGroup;
    [SerializeField] private AdrenalineSystem adrenaline;
    [SerializeField] private float pulseSpeed = 1.5f;

    private bool hasEverRushed = false;
    private bool isShowing = false;
    private float showStartTime = 0f;

    private void Awake()
    {
        if (!adrenaline) adrenaline = FindFirstObjectByType<AdrenalineSystem>();
        hintGroup.alpha = 0f;
    }

    private void Update()
    {
        if (hasEverRushed) return;

        if (isShowing && adrenaline.IsRushActive)
        {
            hasEverRushed = true;
            hintGroup.alpha = 0f;
            return;
        }

        if (!isShowing && adrenaline.CanActivateRush)
        {
            isShowing = true;
            showStartTime = Time.unscaledTime;
        }

        if (isShowing && !adrenaline.CanActivateRush && !adrenaline.IsRushActive)
        {
            isShowing = false;
            hintGroup.alpha = 0f;
        }

        if (isShowing)
        {
            float elapsed = (Time.unscaledTime - showStartTime) * pulseSpeed;
            hintGroup.alpha = (-Mathf.Cos(elapsed) + 1f) * 0.5f;
        }
    }
}
