using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// ラッシュ中にPlayerと接触したときにBlastする汎用コンポーネント。
/// </summary>
public class Blastable : MonoBehaviour
{
    public event Action OnBlasted;
    [Header("Blast Settings")]
    [SerializeField] private int blastScore = 100;
    [SerializeField] private float blastForceMagnitude = 60f;
    [SerializeField] private float blastUpAngle = 30f;
    [SerializeField] private float blastSpreadAngle = 25f;
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

    public void TryBlast(GameObject other)
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
        OnBlasted?.Invoke();
        GameManager.Instance.NotifyBlasted();

        if (rb != null)
        {
            rb.isKinematic = false;
            float yaw = UnityEngine.Random.Range(-blastSpreadAngle, blastSpreadAngle);
            Vector3 dir = Quaternion.Euler(-blastUpAngle, yaw, 0) * Vector3.forward;
            rb.AddForce(dir * blastForceMagnitude, ForceMode.Impulse);
        }

        if (blastScore > 0)
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
