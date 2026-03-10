using System.Collections;
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
    [SerializeField] private AdrenalineSystem adrenaline;
    [SerializeField] private Animator animator;

    private int moveStateHash;
    private int jumpHash;
    private int groundedHash;
    private int deathHash;
    private int spinLeftHash;
    private int spinRightHash;

    private void Awake()
    {
        if (!runner) runner = GetComponentInParent<RunnerController>();
        if (!adrenaline) adrenaline = GetComponent<AdrenalineSystem>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        moveStateHash = Animator.StringToHash("MoveState");
        jumpHash = Animator.StringToHash("Jump");
        groundedHash = Animator.StringToHash("IsGrounded");
        deathHash = Animator.StringToHash("Death");
        spinLeftHash = Animator.StringToHash("SpinLeft");
        spinRightHash = Animator.StringToHash("SpinRight");
        
    }

    private void OnEnable()
    {
        if (runner)
        {
            runner.OnJumped += HandleJumped;
            runner.OnGroundedChanged += HandleGroundedChanged;
        }

        if (adrenaline)
        {
            adrenaline.OnNearMissStarted += HandleNearMissStarted;
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

        if (adrenaline)
        {
            adrenaline.OnNearMissStarted -= HandleNearMissStarted;
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
                animator.SetInteger(moveStateHash, (int)RunnerAnimMoveState.Death);
                StartCoroutine(WaitForDeathAnim());
                break;
        }
    }

    private void HandleNearMissStarted(int direction)
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        if (direction < 0) animator.SetTrigger(spinLeftHash);
        else if (direction > 0) animator.SetTrigger(spinRightHash);
    }

    private IEnumerator WaitForDeathAnim()
    {
        yield return null;
        while (animator.IsInTransition(0))
            yield return null;
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;
        animator.SetInteger(moveStateHash, (int)RunnerAnimMoveState.Stand);
        GameManager.Instance?.NotifyReadyToShowResult();
    }

    private void SyncInitialState()
    {
        if (!GameManager.Instance) return;
        HandleGameStateChanged(GameManager.Instance.State);
    }
}


