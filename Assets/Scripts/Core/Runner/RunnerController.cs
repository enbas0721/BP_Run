using System;
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
    [SerializeField] private float riseGravityMultiplier = 1.4f;
    [SerializeField] private float fallGravityMultiplier = 2.6f;

    [Header("Debug")]
    [SerializeField] private bool freezeRotation = true;

    // Events
    public event Action<int, int> OnLaneChangeRequested;
    public event Action OnJumped;
    public event Action OnLanded;
    public event Action<bool> OnGroundedChanged;

    private Rigidbody rb;

    // Lane System
    private int currentLane = 0; // 0=center, -1=left, 1=right
    
    private float laneSpeedMultiplier = 1f;
    public void SetLaneSpeedMultiplier(float mul) => laneSpeedMultiplier = Mathf.Max(0.05f, mul);

    private float forwardSpeedMultiplier = 1f;
    public void SetForwardSpeedMultiplier(float mul) => forwardSpeedMultiplier = Mathf.Max(0f, mul);

    private float lastGroundedTime = -999f;
    private float lastJumpPressedTime = -999f;

    private bool jumpQueued;

    private bool wasGrounded;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (freezeRotation) rb.freezeRotation = true;
    }

    private void Update()
    {
        bool grounded = IsGrounded();

        if (!IsPlaying()) return;

        if (grounded != wasGrounded)
        {
            OnGroundedChanged?.Invoke(grounded);

            if (grounded)
                OnLanded?.Invoke();

            wasGrounded = grounded;
        }

        if (grounded)
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

        pos.z += (forwardSpeed * forwardSpeedMultiplier) * Time.fixedDeltaTime;

        float targetX = currentLane * laneWidth;
        pos.x = Mathf.Lerp(pos.x, targetX, (laneMoveSpeed * laneSpeedMultiplier )* Time.fixedDeltaTime);

        rb.MovePosition(pos);
        ApplyExtraGravity();

        if (jumpQueued)
        {
            jumpQueued = false;
            DoJump();
        }
    }
    public void MoveLane(int direction)
    {
        if (!IsPlaying()) return;

        int from = currentLane;

        currentLane += direction;
        currentLane = Mathf.Clamp(currentLane, -1, 1);

        int to = currentLane;

        if (to != from)
        {
            OnLaneChangeRequested(from, to);
        }
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

        OnJumped?.Invoke();
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

    private void ApplyExtraGravity()
    {
        var v = rb.linearVelocity;

        if (v.y > 0.01f)
        {
            // 上昇中の加速度追加
            rb.AddForce(Physics.gravity * (riseGravityMultiplier - 1f), ForceMode.Acceleration);
        }
        else if (v.y < -0.01f)
        {
            // 加工中の加速度追加
            rb.AddForce(Physics.gravity * (fallGravityMultiplier - 1f), ForceMode.Acceleration);
        }
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
