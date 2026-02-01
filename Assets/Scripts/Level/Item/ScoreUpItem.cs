using UnityEngine;

public class ScoreUpItem : MonoBehaviour
{
    [SerializeField] private int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<RunnerController>() == null) return;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(amount);
        }

        Destroy(gameObject);
    }
}
