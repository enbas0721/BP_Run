using UnityEngine;

public class RunnerGameOverTrigger : MonoBehaviour
{
    [Header("Obstacle")]
    [SerializeField] private string obstacleTag = "Obstacle";

    [Header("Fall Death")]
    [SerializeField] private float fallY = -3f;

    [SerializeField] private bool logOnBlocked = true;

    private bool dead = false;

    public void Reset()
    {
        dead = false;
    }

    private void Update()
    {
        if (dead) return;

        if (GameManager.Instance == null) return;
        if (GameManager.Instance.State != GameState.Playing) return;

        if (transform.position.y < fallY)
        {
            DoGameOver("Feel below fallY");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (dead) return;

        if (GameManager.Instance == null) return;
        if (GameManager.Instance.State != GameState.Playing) return;

        if (!collision.collider.CompareTag(obstacleTag)) return;

        if (GameManager.Instance.IsInvincible)
        {
            if (logOnBlocked)
            {
                Debug.Log("[Game Over BLOCKED] Collision with Obstacle", this);
            }
            return;
        }

        DoGameOver("Collided with Obstacle");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (dead) return;


        if (GameManager.Instance == null) return;
        if (GameManager.Instance.State != GameState.Playing) return;

        if (!other.CompareTag(obstacleTag)) return;

        if (GameManager.Instance.IsInvincible)
        {
            if (logOnBlocked)
            {
                Debug.Log("[GameOver BLOCKED] Triggered with Obstacle", this);
            }
            return;
        }

        DoGameOver("Triggered with Obstacle");
    }

    private void DoGameOver(string reason)
    {
        dead = true;

        Debug.Log($"[GameOver] {reason}", this);

        GameManager.Instance.GameOver();
    }
}
