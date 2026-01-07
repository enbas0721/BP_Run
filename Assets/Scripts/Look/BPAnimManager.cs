using UnityEngine;

public class BPAnimManager : MonoBehaviour
{
    private Animator _animator;

    private static readonly int StateRun = Animator.StringToHash("BP_Run");
    private static readonly int StateDeath = Animator.StringToHash("BP_Death");
    private static readonly int StateJump = Animator.StringToHash("BP_Jump");
    private static readonly int StateStand = Animator.StringToHash("BP_Stand");
    private static readonly int StateWalk = Animator.StringToHash("BP_Walk");

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    [ContextMenu("Play Run")]
    public void PlayRun() => Play(StateRun);

    [ContextMenu("Play Stand")]
    public void PlayStand() => Play(StateStand);

    [ContextMenu("Play Walk")]
    public void PlayWalk() => Play(StateWalk);

    [ContextMenu("Play Jump")]
    public void PlayJump() => Play(StateJump);

    [ContextMenu("Play Death")]
    public void PlayDeath() => Play(StateDeath);

    private void Play(int stateHash)
    {
        if (_animator == null) _animator = GetComponent<Animator>();
        _animator.Play(stateHash);
    }
}