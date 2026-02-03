using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
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

    [Header("Debug")]
    [SerializeField] private bool freezeRotation = true;

    private Rigidbody rb;

    private int currentLane = 0; // 0=center, -1=left, 1=right

    private float lastGroundedTime = -999f;
    private float lastJumpPressedTime = -999f;

    private bool jumpQueued;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (freezeRotation) rb.freezeRotation = true;
    }

    private void Update()
    {
        if (!IsPlaying()) return;

        if (IsGrounded())
        {
            lastGroundedTime = Time.time;
        }

        /* ジャンプまでのバッファ時間（ジャンプ開始が早すぎるのを制御） */
        if (Time.time - lastJumpPressedTime <= jumpBufferTime &&
            Time.time - lastGroundedTime <= coyoteTime) 
        {
            jumpQueued = true;
            lastJumpPressedTime = -999f;
            lastGroundedTime = -999f;
        }
    }

    private void FixedUpdate()
    {
        if (!IsPlaying()) return;

        Vector3 pos = rb.position;

        pos.z += forwardSpeed * Time.fixedDeltaTime;

        float targetX = currentLane * laneWidth;
        pos.x = Mathf.Lerp(pos.x, targetX, laneMoveSpeed * Time.fixedDeltaTime);

        rb.MovePosition(pos);

        if (jumpQueued)
        {
            jumpQueued = false;
            DoJump();
        }
    }
    public void MoveLane(int direction)
    {
        if (!IsPlaying()) return;
        
        currentLane += direction;
        currentLane = Mathf.Clamp(currentLane, -1, 1);
    }

    public void Jump()
    {
        if (!IsPlaying()) return;
        lastJumpPressedTime = Time.time;
    }

    public void Slide()
    {
        if (!IsPlaying()) return;
        // [MEMO] 追加予定なし
    }

    private void DoJump()
    {
        Vector3 v = rb.linearVelocity;
        if (v.y < 0f) v.y = 0f;
        rb.linearVelocity = v;

        rb.AddForce(Vector3.up * jumpVelocity, ForceMode.VelocityChange);
    }

    private bool IsGrounded()
    {
        if (!groundCheck) return false;

        return Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundMask,
            QueryTriggerInteraction.Ignore
            );
    }
    private bool IsPlaying()
    {
        if (GameManager.Instance == null) return true;
        return GameManager.Instance.State == GameState.Playing;
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
