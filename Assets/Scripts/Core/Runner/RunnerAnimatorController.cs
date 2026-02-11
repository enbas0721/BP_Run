using UnityEngine;

public enum RunnerAnimMoveState
{ 
    Stand = 0,
    Run = 1,
    Death = 2,
}

[RequireComponent(typeof(Animator))]
public class RunnerAnimatorController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RunnerController runner;
    [SerializeField] private Animator animator;

    private int moveStateHash;
    private int jumpHash;
    private int groundedHash;
    private int deathHash;

    private void Awake()
    {
        if (!runner) runner = GetComponentInParent<RunnerController>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        moveStateHash = Animator.StringToHash("MoveState");
        jumpHash = Animator.StringToHash("Jump");
        groundedHash = Animator.StringToHash("IsGrounded");
        deathHash = Animator.StringToHash("Death");
    }

    private void OnEnable()
    {
        if (runner)
        {
            runner.OnJumped += HandleJumped;
            runner.OnGroundedChanged += HandleGroundedChanged;
        }

        if (GameManager.Instance)
            GameManager.Instance.OnStateChanged += HandleGameStateChanged;

        SyncInitialState();
    }

    private void OnDisable()
    {
        if (runner)
        {
            runner.OnJumped -= HandleJumped;
            runner.OnGroundedChanged -= HandleGroundedChanged;
        }

        if (GameManager.Instance)
            GameManager.Instance.OnStateChanged -= HandleGameStateChanged;
    }

    private void HandleJumped()
    {
        animator.SetTrigger(jumpHash);
    }

    private void HandleGroundedChanged(bool grounded)
    {
        animator.SetBool(groundedHash, grounded);
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Ready:
                animator.SetInteger(moveStateHash, (int)RunnerAnimMoveState.Stand);
                break;

            case GameState.Playing:
                animator.SetInteger(moveStateHash, (int)RunnerAnimMoveState.Run);
                break;

            case GameState.GameOver:
                animator.SetTrigger(deathHash);
                break;
        }
    }

    private void SyncInitialState()
    {
        if (!GameManager.Instance) return;
        HandleGameStateChanged(GameManager.Instance.State);
    }
}


