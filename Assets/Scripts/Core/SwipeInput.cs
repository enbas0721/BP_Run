using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeInput : MonoBehaviour
{
    [SerializeField] private RunnerController runner;

    [Header("Swipe")]
    [SerializeField] private float minSwipePixels = 60f;

    private Vector2 startPos;
    private bool tracking;

    private void Awake()
    {
        if (!runner) runner = GetComponent<RunnerController>();
        if (!runner) runner = FindFirstObjectByType<RunnerController>();
    }

    private void Update()
    {
        var mouse = Mouse.current;
        var touch = Touchscreen.current;

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
        if (delta.magnitude < minSwipePixels) return;

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
