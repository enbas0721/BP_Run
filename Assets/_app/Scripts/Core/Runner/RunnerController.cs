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
    [SerializeField] private float cancelFallMultiplier = 1.5f; // キャンセル時にfallGravityMultiplierへ重ねて掛ける倍率

    [Header("Jump Speed Scaling")]
    [Tooltip("速度倍率に応じてジャンプ重力・初速をスケールする強さ（0=無効, 1=完全追従）")]
    [SerializeField] [Range(0f, 1f)] private float jumpSpeedScaleStrength = 1f;

    [Header("Stairs / Slope")]
    [SerializeField] private float stepDownDistance = 0.4f; // 下り時の地面スナップ最大距離

    // Events
    public event Action<int, int> OnLaneChangeRequested;
    public event Action OnJumped;
    public event Action OnLanded;
    public event Action<bool> OnGroundedChanged;

    private Rigidbody rb;

    // For Reset
    private Vector3 startPos;
    private Quaternion startRot;

    // Lane System
    private int currentLane = 0; // 0=center, -1=left, 1=right
    
    private float laneSpeedMultiplier = 1f;
    public void SetLaneSpeedMultiplier(float mul) => laneSpeedMultiplier = Mathf.Max(0.05f, mul);

    private float baseForwardMultiplier = 1f;
    private float rushForwardMultiplier = 1f;

    private float lastGroundedTime = -999f;
    private float lastJumpPressedTime = -999f;

    private bool jumpQueued;
    private bool jumpCancelled;

    private bool wasGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        startPos = this.transform.position;
        startRot = this.transform.rotation;
    }
    public void ResetRunner()
    {
        currentLane = 0;

        laneSpeedMultiplier = 1f;
        baseForwardMultiplier = 1f;
        rushForwardMultiplier = 1f;

        lastGroundedTime = -999f;
        lastJumpPressedTime = -999f;
        jumpQueued = false;
        jumpCancelled = false;

        // 次のUpdateでGroundedChangedが飛ぶためfalseにしておいて切り替えさせる。
        wasGrounded = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 p = startPos;

        float x = 0f;
        float y = p.y;
        float z = p.z;

        rb.position = new Vector3(x, y, z);
        rb.rotation = startRot;
        transform.SetPositionAndRotation(rb.position, rb.rotation);

        /*rb.Sleep();
        rb.WakeUp();*/
    }

    private void Update()
    {
        bool grounded = IsGrounded();

        if (!IsPlaying()) return;

        if (grounded != wasGrounded)
        {
            OnGroundedChanged?.Invoke(grounded);

            if (grounded)
            {
                OnLanded?.Invoke();
                jumpCancelled = false;
            }

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

        float finalMul = baseForwardMultiplier * rushForwardMultiplier;
        pos.z += (forwardSpeed * finalMul) * Time.fixedDeltaTime;

        float targetX = currentLane * laneWidth;
        pos.x = Mathf.Lerp(pos.x, targetX, (laneMoveSpeed * laneSpeedMultiplier )* Time.fixedDeltaTime);

        // 下り階段・スロープ対応：落下中でなければ地面にスナップ
        if (rb.linearVelocity.y <= 0.01f && groundCheck != null)
        {
            float feetOffset = rb.position.y - groundCheck.position.y;
            if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, feetOffset + stepDownDistance, groundMask, QueryTriggerInteraction.Ignore))
            {
                float targetY = hit.point.y + feetOffset;
                if (targetY < pos.y)
                    pos.y = targetY;
            }
        }

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
            OnLaneChangeRequested?.Invoke(from, to);
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
        if (!IsGrounded())
            CancelJump();
    }

    private void CancelJump()
    {
        Vector3 v = rb.linearVelocity;
        if (v.y > 0f) v.y = 0f;
        rb.linearVelocity = v;
        jumpCancelled = true;
    }

    private void DoJump()
    {
        Vector3 v = rb.linearVelocity;
        if (v.y < 0f) v.y = 0f;
        rb.linearVelocity = v;

        // 速度スケール：jumpSpeedScaleStrength=1で完全追従、0で固定
        float k = Mathf.Lerp(1f, baseForwardMultiplier, jumpSpeedScaleStrength);
        rb.AddForce(Vector3.up * jumpVelocity * Mathf.Sqrt(k), ForceMode.VelocityChange);

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

        // 速度スケール：同じ比率で重力も強くすることで高さを維持しつつ滞空時間を短縮
        float k = Mathf.Lerp(1f, baseForwardMultiplier, jumpSpeedScaleStrength);

        if (v.y > 0.01f)
        {
            // 上昇中の加速度追加
            rb.AddForce(Physics.gravity * (riseGravityMultiplier - 1f) * k, ForceMode.Acceleration);
        }
        else if (v.y < -0.01f)
        {
            // 下降中の加速度追加（キャンセル時はcancelFallMultiplierを重ねて掛ける）
            float mul = jumpCancelled ? (fallGravityMultiplier - 1f) * cancelFallMultiplier : (fallGravityMultiplier - 1f);
            rb.AddForce(Physics.gravity * mul * k, ForceMode.Acceleration);
        }
    }

    public void SetBaseForwardMultiplier(float mul)
    {
        baseForwardMultiplier = Mathf.Max(0f, mul);
    }

    public void SetRushForwardMultiplier(float mul)
    {
        rushForwardMultiplier = Mathf.Max(0f, mul);
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
