using System.Collections;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("Blast Settings")]
    [SerializeField] private int blastScore = 100;
    [SerializeField] private Vector3 blastForce = new Vector3(0f, 4f, -8f);
    [SerializeField] private float destroyDelay = 3f;

    private Rigidbody rb;
    private bool blasted = false;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        ResetState();
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
        if (blasted) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.State != GameState.Playing) return;
        if (!GameManager.Instance.IsInvincible) return;
        if (!other.CompareTag("Player")) return;

        Blast();
    }

    private void Blast()
    {
        blasted = true;

        // RunnerGameOverTrigger に再反応させないためタグを外す
        gameObject.tag = "Untagged";

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(blastForce, ForceMode.Impulse);
        }

        GameManager.Instance.AddScore(blastScore);

        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = false;
    }

    private void ResetState()
    {
        blasted = false;
        gameObject.tag = "Obstacle";
        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;
        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.isKinematic = true;
        }
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;
        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = true;
    }
}