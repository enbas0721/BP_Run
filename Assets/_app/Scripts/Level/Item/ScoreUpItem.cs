using UnityEngine;

public class ScoreUpItem : MonoBehaviour
{
    [SerializeField] private int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<RunnerController>() == null) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(amount);
        }

        Destroy(gameObject);
    }
}
