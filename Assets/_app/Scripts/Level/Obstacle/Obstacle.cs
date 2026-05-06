using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private Blastable blastable;

    private void Awake()
    {
        blastable = GetComponent<Blastable>();
    }

    private void OnDisable()
    {
        gameObject.tag = "Obstacle";
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryBlast(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryBlast(other.gameObject);
    }

    private void TryBlast(GameObject other)
    {
        if (blastable == null) return;

        // Blast前にタグを外してGameOverTriggerに再反応させない
        if (GameManager.Instance != null
            && GameManager.Instance.IsInvincible
            && other.CompareTag("Player"))
        {
            gameObject.tag = "Untagged";
        }

        blastable.TryBlast(other);
    }
}
