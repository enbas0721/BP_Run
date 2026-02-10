using UnityEngine;

public class RunnerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float forwardSpeed = 8f;
    [SerializeField] private float laneMoveSpeed = 10f;
    [SerializeField] private float laneWidth = 3f;

    [Header("Jump")]
    [SerializeField] private float jumpVelocity = 7f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float coyoteTime = 0.08f;
    [SerializeField] private float jumpBufferTime = 0.10f;

    private int currentLane = 0; // 0=center, -1=left, 1=right
    private Vector3 targetPosition;

    private Rigidbody rb;

    private float lastGroundedTime = -999f;
    private float lastJumpPressedTime = -999f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        targetPosition = transform.position;
    }

    private void Update()
    {
        ForwardMove();
        LaneMove();

        if (IsGrounded())
        {
            lastGroundedTime = Time.time;
        }

        /* ジャンプまでのバッファ時間（ジャンプ開始が早すぎるのを制御） */
        if (Time.time - lastJumpPressedTime <= jumpBufferTime &&
            Time.time - lastGroundedTime <= coyoteTime) 
        {
            DoJump();
            lastJumpPressedTime = -999f;
            lastGroundedTime = -999f;
        }
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
        lastJumpPressedTime = Time.time;
    }

    public void Slide()
    {
        // 後で実装（コライダー縮める等）
    }

    private void DoJump()
    {
        if (!rb) return;

        var v = rb.linearVelocity;
        if (v.y < 0f) v.y = 0f;
        rb.linearVelocity = v;

        rb.AddForce(Vector3.up * jumpVelocity, ForceMode.VelocityChange);
    }

    private bool IsGrounded()
    {
        if (!groundCheck) return false;
        return Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
#endif
}
