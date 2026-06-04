using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;

    [Header("Y Limit")]
    [SerializeField] private float minY = 0f;
    [SerializeField] private float maxY = 5f;

    [Header("Intro")]
    [SerializeField] private float introMoveDuration = 1.0f;
    [SerializeField] private AnimationCurve introMoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Transform introControlPoint;
    [SerializeField] private Vector3 introEndEulerAngles;

    [Header("Force Stop Zoom")]
    [Tooltip("停止時のカメラオフセット（通常offsetより近い値に）")]
    [SerializeField] private Vector3 zoomOffset;
    [SerializeField] private float zoomDuration = 0.6f;
    [SerializeField] private AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 currentOffset;
    private Coroutine zoomCoroutine;

    private Vector3 startPos;
    private Quaternion startRot;
    private bool introActive = false;
    private float introElapsed = 0f;
    private Vector3 introFromPos;
    private GameState previousState = GameState.Ready;
    private float savedShadowDistance;

    private void Awake()
    {
        startPos = this.transform.position;
        startRot = this.transform.rotation;
        currentOffset = offset;
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            GameManager.Instance.OnForceStopChanged += HandleForceStopChanged;
            GameManager.Instance.OnGateEntered += HandleGateEntered;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            GameManager.Instance.OnForceStopChanged -= HandleForceStopChanged;
            GameManager.Instance.OnGateEntered -= HandleGateEntered;
        }
    }

    private void HandleGateEntered()
    {
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(ZoomTo(zoomOffset));
    }

    private void HandleForceStopChanged(bool stopped)
    {
        if (stopped) return;
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(ZoomTo(offset));
    }

    private IEnumerator ZoomTo(Vector3 targetOffset)
    {
        Vector3 fromOffset = currentOffset;
        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = zoomCurve.Evaluate(Mathf.Clamp01(elapsed / zoomDuration));
            currentOffset = Vector3.Lerp(fromOffset, targetOffset, t);
            yield return null;
        }
        currentOffset = targetOffset;
        zoomCoroutine = null;
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Playing && previousState == GameState.Ready)
        {
            introFromPos = transform.position;
            introElapsed = 0f;
            introActive = true;
            savedShadowDistance = QualitySettings.shadowDistance;
            QualitySettings.shadowDistance = 0f;
        }
        else if (state == GameState.Ready)
        {
            introActive = false;
        }
        previousState = state;
    }

    private void LateUpdate()
    {
        if (!target) return;

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.State == GameState.Ready)
            {
                transform.SetPositionAndRotation(startPos, startRot);
                return;
            }
            else if (GameManager.Instance.State != GameState.Playing)
            {
                return;
            }
        }

        Vector3 targetPos = target.position + currentOffset;
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        if (introActive)
        {
            introElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(introElapsed / introMoveDuration);
            float ct = introMoveCurve.Evaluate(t);

            // 2次ベジェ曲線
            targetPos = target.position + currentOffset;
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
            Vector3 p1 = introControlPoint ? introControlPoint.position : Vector3.Lerp(introFromPos, targetPos, 0.5f);
            Vector3 bezierPos = (1 - ct) * (1 - ct) * introFromPos
                              + 2 * (1 - ct) * ct * p1
                              + ct * ct * targetPos;

            transform.position = bezierPos;
            Quaternion lookAtRot = Quaternion.LookRotation(target.position - bezierPos);
            Quaternion endRot = Quaternion.Euler(introEndEulerAngles);
            transform.rotation = Quaternion.Slerp(lookAtRot, endRot, ct);

            if (t >= 1f)
            {
                introActive = false;
            }
            return;
        }

        transform.position = targetPos;
    }
}
