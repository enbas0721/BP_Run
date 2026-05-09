using System.Collections;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rollRotateSpeed = 360f;

    private bool isMoving = false;
    private bool isStopped = false;
    private Quaternion startLocalRot;

    private void OnEnable()
    {
        isStopped = false;
        isMoving = false;
        startLocalRot = transform.localRotation;
    }

    private void OnDisable()
    {
        isStopped = true;
        isMoving = false;
        StopAllCoroutines();
    }

    // Called from BarrelStartTrigger (child object) when player enters the trigger zone
    public void NotifyPlayerEntered()
    {
        if (isStopped || isMoving) return;
        isMoving = true;
        StartCoroutine(RollRoutine());
    }

    // Non-trigger collider: stops rolling on contact with Player or Obstacle
    private void OnCollisionEnter(Collision collision)
    {
        if (!isMoving) return;
        if (collision.transform.IsChildOf(transform)) return;
        if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("Obstacle"))
        {
            isStopped = true;
            StopAllCoroutines();
        }
    }

    private IEnumerator RollRoutine()
    {
        while (!isStopped)
        {
            float delta = moveSpeed * Time.deltaTime;
            transform.localPosition += Vector3.back * delta;
            transform.localRotation *= Quaternion.Euler(-rollRotateSpeed * Time.deltaTime, 0f, 0f);
            yield return null;
        }
    }
}
