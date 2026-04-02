using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeInput : MonoBehaviour
{
    [SerializeField] private RunnerController runner;
    [SerializeField] private AdrenalineSystem adrenaline;

    [Header("Swipe")]
    [SerializeField] private float minSwipePixels = 60f;

    [Header("Double Tap")]
    [SerializeField] private float doubleTapInterval = 0.3f;

    private Vector2 startPos;
    private bool tracking;

    private float lastTapTime = -999f;

    private void Awake()
    {
        if (!runner) runner = GetComponent<RunnerController>();
        if (!runner) runner = FindFirstObjectByType<RunnerController>();
        if (!adrenaline) adrenaline = FindFirstObjectByType<AdrenalineSystem>();
    }

    public void ResetInputState()
    {
        tracking = false;
        startPos = Vector2.zero;
        lastTapTime = -999f;
    }

    private void Update()
    {
        var mouse = Mouse.current;
        var touch = Touchscreen.current;

        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
        {
            tracking = false;
            return;
        }

            // Touch優先
        if (touch != null && touch.primaryTouch.press.isPressed)
        {
            var pos = touch.primaryTouch.position.ReadValue();

            if (!tracking)
            {
                tracking = true;
                startPos = pos;
            }
        }

        // Touch修了時
        if (tracking && touch != null && !touch.primaryTouch.press.isPressed)
        {
            tracking = false;
            var endPos = touch.primaryTouch.position.ReadValue();
            HandleSwipe(endPos - startPos);
            return;
        }

        // Editor/PC用(マウス)
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            tracking = true;
            startPos = mouse.position.ReadValue();
        }

        if (tracking && mouse.leftButton.wasReleasedThisFrame)
        {
            tracking = false;
            var endPos = mouse.position.ReadValue();
            HandleSwipe(endPos - startPos);
        }
    }

    private void HandleSwipe(Vector2 delta)
    {
        if (delta.magnitude < minSwipePixels)
        {
            // Tap (not a swipe) — check for double tap
            float now = Time.unscaledTime;
            if (now - lastTapTime <= doubleTapInterval)
            {
                adrenaline?.ActivateRush();
                lastTapTime = -999f;
            }
            else
            {
                lastTapTime = now;
            }
            return;
        }

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            /* 横方向スワイプ判定 */
            if (delta.x > 0) runner.MoveLane(+1);
            else runner.MoveLane(-1);
        }
        else
        {
            /* 縦方向スワイプ判定 */
            if (delta.y > 0) runner.Jump();
            else runner.Slide();
        }
    }
}
