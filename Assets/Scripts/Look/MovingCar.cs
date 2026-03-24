using System.Collections;
using UnityEngine;

public class MovingCar : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float forwardDistance = 5f;   // 前方移動距離（メートル）
    [SerializeField] private float moveSpeed = 3f;         // 移動速度
    [SerializeField] private float rotateSpeed = 180f;     // 回転速度（度/秒）
    [SerializeField] private float waitInterval = 2f;      // 移動間の待機時間（秒）

    [Header("X Position Settings")]
    [SerializeField] private float centerX = 0f;           // 中央のX座標（ローカル）
    [SerializeField] private float rightX = 3f;            // 右のX座標（ローカル）
    [SerializeField] private float leftX = -3f;            // 左のX座標（ローカル）

    private bool isStopped = false;
    private float[] xPositions;
    private Vector3 startLocalPos;
    private Quaternion startLocalRot;

    private void Awake()
    {
        xPositions = new float[] { centerX, rightX, leftX };
    }

    private void OnEnable()
    {
        isStopped = false;
        startLocalPos = transform.localPosition;
        startLocalRot = transform.localRotation;
        StartCoroutine(MovementRoutine());
    }

    private void OnDisable()
    {
        isStopped = true;
        StopAllCoroutines();
    }

    private IEnumerator MovementRoutine()
    {
        while (!isStopped)
        {
            // Phase 1: -Z方向へ移動
            Vector3 forwardLocalTarget = new Vector3(transform.localPosition.x, startLocalPos.y, startLocalPos.z - forwardDistance);
            yield return StartCoroutine(RotateAndMoveLocal(forwardLocalTarget));
            if (isStopped) yield break;

            yield return new WaitForSeconds(waitInterval);
            if (isStopped) yield break;

            // Phase 2: ランダムX位置へ横移動
            float targetX = xPositions[Random.Range(0, xPositions.Length)];
            Vector3 lateralLocalTarget = new Vector3(targetX, transform.localPosition.y, transform.localPosition.z);
            yield return StartCoroutine(RotateAndMoveLocal(lateralLocalTarget));
            if (isStopped) yield break;

            yield return new WaitForSeconds(waitInterval);
            if (isStopped) yield break;

            // Phase 3: +Z方向へ開始位置まで戻る
            Vector3 returnLocalTarget = new Vector3(transform.localPosition.x, startLocalPos.y, startLocalPos.z);
            yield return StartCoroutine(RotateAndMoveLocal(returnLocalTarget));
            if (isStopped) yield break;

            yield return new WaitForSeconds(waitInterval);
        }
    }

    private IEnumerator MoveToLocalPosition(Vector3 localTarget)
    {
        while (!isStopped)
        {
            Vector3 worldTarget = LocalToWorld(localTarget);
            if (Vector3.Distance(transform.position, worldTarget) <= 0.05f) break;
            transform.position = Vector3.MoveTowards(transform.position, worldTarget, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator RotateAndMoveLocal(Vector3 localTarget)
    {
        Vector3 localDir = localTarget - transform.localPosition;
        if (localDir.sqrMagnitude < 0.05f * 0.05f) yield break;
        localDir.Normalize();

        // XZ平面の移動方向からY角度を直接計算
        float angle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
        Quaternion targetLocalRot = Quaternion.Euler(0, angle, 0);
        yield return StartCoroutine(RotateToLocal(targetLocalRot));

        if (isStopped) yield break;

        yield return StartCoroutine(MoveToLocalPosition(localTarget));
    }

    private IEnumerator RotateToLocal(Quaternion targetLocalRot)
    {
        while (!isStopped && Quaternion.Angle(transform.localRotation, targetLocalRot) > 0.5f)
        {
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetLocalRot, rotateSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private Vector3 LocalToWorld(Vector3 localPos)
    {
        return transform.parent != null ? transform.parent.TransformPoint(localPos) : localPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Obstacle")) return;
        if (other.transform.IsChildOf(transform)) return;
        isStopped = true;
        StopAllCoroutines();
    }
}
