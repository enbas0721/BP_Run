using UnityEngine;

public class RunnerController : MonoBehaviour
{
    public float forwardSpeed = 8f;
    public float laneMoveSpeed = 10f;
    public float laneWidth = 3f;

    private int currentLane = 0; // 0=center, -1=left, 1=right
    private Vector3 targetPosition;

    private void Start()
    {
        targetPosition = transform.position;
    }

    private void Update()
    {
        ForwardMove();
        LaneMove();
    }

    private void ForwardMove()
    {
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
    }

    private void LaneMove()
    {
        targetPosition.x = currentLane * laneWidth;
        transform.position = Vector3.Lerp(
            transform.position,
            new Vector3(targetPosition.x, transform.position.y, transform.position.z),
            Time.deltaTime * laneMoveSpeed
        );
    }

    public void MoveLane(int direction)
    {
        currentLane += direction;
        currentLane = Mathf.Clamp(currentLane, -1, 1);
    }

    public void Jump()
    {
        // 後で実装（重力＋ジャンプ力）
    }

    public void Slide()
    {
        // 後で実装（コライダー縮める等）
    }
}
