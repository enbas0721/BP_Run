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

    private Vector3 startPos;
    private Quaternion startRot;
    private bool introActive = false;
    private float introElapsed = 0f;
    private Vector3 introFromPos;

    private void Awake()
    {
        startPos = this.transform.position;
        startRot = this.transform.rotation;
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Playing)
        {
            introFromPos = transform.position;
            introElapsed = 0f;
            introActive = true;
        }
        else if (state == GameState.Ready)
        {
            introActive = false;
        }
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

        Vector3 targetPos = target.position + offset;
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        if (introActive)
        {
            introElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(introElapsed / introMoveDuration);
            float ct = introMoveCurve.Evaluate(t);

            // 2次ベジェ曲線
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
