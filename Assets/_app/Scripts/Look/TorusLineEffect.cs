using UnityEngine;

/// <summary>
/// LineRendererを使用してトーラス（ドーナツ）の軌跡面を静止描画するスクリプト。
/// StartRotation()呼び出し時に全頂点を一括計算して表示する。
/// useWorldSpace=falseのためローカル空間で描画され、親移動による形状の歪みがない。
/// </summary>
public class TorusLineEffect : MonoBehaviour
{
    [Header("トーラス設定")]
    [Tooltip("大円の半径（中心からドーナツの中心までの距離）")]
    public float majorRadius = 3f;

    [Tooltip("小円の半径（ドーナツの太さ）")]
    public float minorRadius = 1f;

    [Tooltip("大円1周あたりの小円の巻き数（整数にすると軌跡が閉じてきれいになる）")]
    public int windingCount = 6;

    [Header("LineRenderer設定")]
    [Tooltip("全体の頂点数（多いほど滑らか）")]
    public int segments = 200;

    [Header("制御")]
    [Tooltip("trueで自動的に表示を開始する")]
    public bool playOnStart = false;

    private LineRenderer[] _lineRenderers;

    private void Awake()
    {
        _lineRenderers = GetComponentsInChildren<LineRenderer>(true);
        foreach (var lr in _lineRenderers)
        {
            lr.useWorldSpace = false;
            lr.loop = false;
            lr.positionCount = segments;
            lr.enabled = false;
        }
    }

    private void Start()
    {
        if (playOnStart)
            StartRotation();
    }

    /// <summary>
    /// トーラス軌跡を描画して表示する（外部から呼び出し可能）
    /// </summary>
    public void StartRotation(int direction = 1)
    {
        float dir = direction >= 0 ? 1f : -1f;

        Vector3[] positions = new Vector3[segments];
        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / segments;
            float majorAngle = t * Mathf.PI * 2f * dir;
            float minorAngle = t * windingCount * Mathf.PI * 2f;

            Vector3 majorCenter = new Vector3(
                Mathf.Cos(majorAngle) * majorRadius,
                0f,
                Mathf.Sin(majorAngle) * majorRadius
            );
            Vector3 radialDir = new Vector3(Mathf.Cos(majorAngle), 0f, Mathf.Sin(majorAngle)).normalized;

            positions[i] = majorCenter
                + radialDir  * Mathf.Cos(minorAngle) * minorRadius
                + Vector3.up * Mathf.Sin(minorAngle) * minorRadius;
        }

        if (_lineRenderers == null) return;
        foreach (var lr in _lineRenderers)
        {
            lr.SetPositions(positions);
            lr.enabled = true;
        }
    }

    /// <summary>
    /// 表示を停止する（外部から呼び出し可能）
    /// </summary>
    public void StopRotation()
    {
        if (_lineRenderers == null) return;
        foreach (var lr in _lineRenderers)
            lr.enabled = false;
    }

    // Sceneビューでトーラス軌道をプレビュー表示
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;

        Gizmos.color = Color.yellow;
        int previewSegs = 64;
        Vector3 prev = origin + new Vector3(majorRadius, 0f, 0f);
        for (int i = 1; i <= previewSegs; i++)
        {
            float a = (float)i / previewSegs * Mathf.PI * 2f;
            Vector3 next = origin + new Vector3(Mathf.Cos(a) * majorRadius, 0f, Mathf.Sin(a) * majorRadius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }

        Gizmos.color = Color.cyan;
        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / segments;
            float majorAngle = t * Mathf.PI * 2f;
            float minorAngle = t * windingCount * Mathf.PI * 2f;

            Vector3 majorCenter = origin + new Vector3(Mathf.Cos(majorAngle) * majorRadius, 0f, Mathf.Sin(majorAngle) * majorRadius);
            Vector3 radial = new Vector3(Mathf.Cos(majorAngle), 0f, Mathf.Sin(majorAngle)).normalized;

            Vector3 p = majorCenter
                + radial     * Mathf.Cos(minorAngle) * minorRadius
                + Vector3.up * Mathf.Sin(minorAngle) * minorRadius;

            Gizmos.DrawSphere(p, 0.02f);
        }
    }
}
