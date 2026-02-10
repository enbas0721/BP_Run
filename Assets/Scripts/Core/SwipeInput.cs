using UnityEngine;

public class SwipeInput : MonoBehaviour
{
    public RunnerController runner;

    private Vector2 startPos;
    private float minSwipe = 50f;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            startPos = Input.mousePosition;

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 endPos = Input.mousePosition;
            Vector2 delta = endPos - startPos;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                if (Mathf.Abs(delta.x) > minSwipe)
                {
                    if (delta.x > 0) runner.MoveLane(+1);
                    else runner.MoveLane(-1);
                }
            }
            else
            {
                if (Mathf.Abs(delta.y) > minSwipe)
                {
                    if (delta.y > 0) runner.Jump();
                    else runner.Slide();
                }
            }
        }
    }
}
