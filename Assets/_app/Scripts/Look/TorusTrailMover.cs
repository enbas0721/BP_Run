using UnityEngine;

/// <summary>
/// 親オブジェクトに追従しながら、ドーナツ型（トーラス）軌道で移動するスクリプト
/// 親が移動すればトーラス軌道の中心も一緒に動く
/// </summary>
public class TorusTrailMover : MonoBehaviour
{
    [Header("トーラス設定")]
    [Tooltip("大円の半径（中心からドーナツの中心までの距離）")]
    public float majorRadius = 3f;

    [Tooltip("小円の半径（ドーナツの太さ）")]
    public float minorRadius = 1f;

    [Header("速度設定")]
    [Tooltip("大円を1周するのにかかる秒数")]
    public float majorDuration = 3f;

    [Tooltip("小円を1周するのにかかる秒数")]
    public float minorDuration = 0.5f;

    [Header("制御")]
    [Tooltip("trueで自動的に1回転を開始する")]
    public bool playOnStart = true;

    [Tooltip("デバッグ用：ループで回転し続ける（Inspectorでオン/オフ可能）")]
    public bool debugLoop = false;

    // 内部状態
    private bool _isPlaying = false;
    private float _elapsedTime = 0f;
    private float _direction = 1f; // 1f=左回り, -1f=右回り
    private Vector3 _localOffset; // 親からの初期ローカルオフセット
    private TrailRenderer[] _trails;

    private void Awake()
    {
        _trails = GetComponentsInChildren<TrailRenderer>(true);
        foreach (var t in _trails) t.enabled = false;
    }

    private void Start()
    {
        // 親からの初期オフセットを記録
        _localOffset = transform.localPosition;

        if (playOnStart || debugLoop)
        {
            StartRotation();
        }
    }

    private void Update()
    {
        if (!_isPlaying) return;

        _elapsedTime += Time.deltaTime;

        // 大円の進行度（0〜1）
        float majorT = _elapsedTime / majorDuration;

        // ループでない場合、1周で停止
        if (!debugLoop && majorT >= 1f)
        {
            majorT = 1f;
            _isPlaying = false;
        }

        // 大円の角度（Y軸周り）※_directionで回転方向を制御
        float majorAngle = majorT * Mathf.PI * 2f * _direction;

        // 小円の角度（XY平面に平行）
        float minorT = _elapsedTime / minorDuration;
        float minorAngle = minorT * Mathf.PI * 2f;

        // 大円上の位置（XZ平面）
        Vector3 majorCenter = new Vector3(
            Mathf.Cos(majorAngle) * majorRadius,
            0f,
            Mathf.Sin(majorAngle) * majorRadius
        );

        // 小円のオフセット（大円の半径方向 + Y軸方向）
        Vector3 radialDir = new Vector3(Mathf.Cos(majorAngle), 0f, Mathf.Sin(majorAngle)).normalized;
        Vector3 upDir = Vector3.up;

        Vector3 minorOffset = radialDir * (Mathf.Cos(minorAngle) * minorRadius)
                            + upDir * (Mathf.Sin(minorAngle) * minorRadius);

        // ローカル座標で設定 → 親が動けば自動的に追従する
        transform.localPosition = _localOffset + majorCenter + minorOffset;
    }

    /// <summary>
    /// 回転を開始する（外部から呼び出し可能）
    /// </summary>
    public void StartRotation(int direction = 1)
    {
        _direction = direction >= 0 ? 1f : -1f;
        _elapsedTime = 0f;
        _isPlaying = true;

        if (_trails != null)
            foreach (var t in _trails) { t.enabled = true; t.Clear(); }
    }

    /// <summary>
    /// 回転を停止する（外部から呼び出し可能）
    /// </summary>
    public void StopRotation()
    {
        _isPlaying = false;
        foreach (var t in _trails) { t.Clear(); t.enabled = false; }
    }

    /// <summary>
    /// デバッグループのオン/オフを切り替えて再スタート
    /// </summary>
    public void SetDebugLoop(bool enabled)
    {
        debugLoop = enabled;
        if (enabled && !_isPlaying)
        {
            StartRotation();
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            if (debugLoop && !_isPlaying)
            {
                StartRotation();
            }
        }
    }

    // Sceneビューでトーラス軌道をプレビュー表示（親の位置基準）
    private void OnDrawGizmosSelected()
    {
        Vector3 origin;
        if (transform.parent != null)
        {
            origin = Application.isPlaying
                ? transform.parent.TransformPoint(_localOffset)
                : transform.parent.TransformPoint(transform.localPosition);
        }
        else
        {
            origin = transform.position;
        }

        // 大円（黄色）
        Gizmos.color = Color.yellow;
        DrawCircleGizmo(origin, majorRadius, 64);

        // 小円（シアン）
        Gizmos.color = Color.cyan;
        int previewCount = 12;
        for (int i = 0; i < previewCount; i++)
        {
            float angle = (float)i / previewCount * Mathf.PI * 2f;
            Vector3 center = origin + new Vector3(Mathf.Cos(angle) * majorRadius, 0f, Mathf.Sin(angle) * majorRadius);
            Vector3 radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
            DrawMinorCircleGizmo(center, minorRadius, 32, radial);
        }
    }

    private void DrawCircleGizmo(Vector3 center, float radius, int segments)
    {
        Vector3 prev = center + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }

    private void DrawMinorCircleGizmo(Vector3 center, float radius, int segments, Vector3 radialDir)
    {
        Vector3 up = Vector3.up;
        Vector3 prev = center + radialDir * radius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Vector3 next = center + radialDir * (Mathf.Cos(angle) * radius) + up * (Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}