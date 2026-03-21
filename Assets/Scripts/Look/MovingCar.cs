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
    [SerializeField] private float centerX = 0f;           // 中央のX座標
    [SerializeField] private float rightX = 3f;            // 右のX座標
    [SerializeField] private float leftX = -3f;            // 左のX座標

    private bool isStopped = false;
    private float[] xPositions;

    void Start()
    {
        xPositions = new float[] { centerX, rightX, leftX };
        StartCoroutine(MovementRoutine());
    }

    private IEnumerator MovementRoutine()
    {
        while (!isStopped)
        {
            // 前方にforwardDistance分移動
            Vector3 forwardTarget = transform.position + Vector3.forward * forwardDistance;
            yield return StartCoroutine(MoveToPosition(forwardTarget));

            if (isStopped) yield break;

            yield return new WaitForSeconds(waitInterval);

            if (isStopped) yield break;

            // ランダムにX位置（中央・右・左）を選択して移動
            float targetX = xPositions[Random.Range(0, xPositions.Length)];
            Vector3 lateralTarget = new Vector3(targetX, transform.position.y, transform.position.z);

            // 横方向に回転してから移動
            yield return StartCoroutine(RotateAndMove(lateralTarget));

            if (isStopped) yield break;

            // 前方（Z+方向）に回転を戻す
            yield return StartCoroutine(RotateTo(Quaternion.identity));

            yield return new WaitForSeconds(waitInterval);
        }
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        while (!isStopped && Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator RotateAndMove(Vector3 target)
    {
        if (Vector3.Distance(transform.position, target) < 0.05f) yield break;

        // ターゲット方向を向くように回転
        Vector3 direction = (target - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        yield return StartCoroutine(RotateTo(targetRotation));

        if (isStopped) yield break;

        // 目標位置まで移動
        yield return StartCoroutine(MoveToPosition(target));
    }

    private IEnumerator RotateTo(Quaternion targetRotation)
    {
        while (!isStopped && Quaternion.Angle(transform.rotation, targetRotation) > 0.5f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        isStopped = true;
        StopAllCoroutines();
    }
}
