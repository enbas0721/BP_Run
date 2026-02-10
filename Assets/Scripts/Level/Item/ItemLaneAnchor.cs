using UnityEngine;

public class ItemLaneAnchor : MonoBehaviour
{
    [Tooltip("Anchor抽選の相対重み(大きいほど選ばれやすい)")]
    [Min(0f)] public float spawnWeight = 1f;
    [Tooltip("Anchorのアイテム配置パターン")]
    public ItemLanePattern pattern;

    [Header("Local-Z Range")]
    public Transform startMarker;
    public Transform endMarker;

    [Header("Gizmo Preview")]
    public bool drawPreviewGizmo = true;
    [Tooltip("表示する点の大きさ")]
    public float gizmoPointRadius = 0.4f;
    [Tooltip("点を線で結ぶ")]
    public bool drawPolyline = true;

    [Header("共有設定")]
    public ItemLaneSettings settings;

    private static readonly System.Collections.Generic.List<ItemLaneSampling.Sample> _samples
        = new System.Collections.Generic.List<ItemLaneSampling.Sample>(256);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1.0f);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawPreviewGizmo) return;
        if (settings == null) return;
        if (pattern == null) return;

        ItemLaneSampling.SamplePositionsLocalZ(
            transform, startMarker, endMarker, pattern, settings, _samples);

        if (_samples.Count == 0) return;

        int currentLine = -1;
        Vector3? prev = null;

        for (int i = 0; i < _samples.Count; i++)
        {
            var s = _samples[i];

            if (s.lineIndex != currentLine)
            {
                currentLine = s.lineIndex;
                Gizmos.color = (currentLine % 2 == 0) ? Color.magenta : Color.cyan;
                prev = null;
            }

            Gizmos.DrawWireSphere(s.worldPos, gizmoPointRadius);

            if (drawPolyline && prev.HasValue)
                Gizmos.DrawLine(prev.Value, s.worldPos);

            prev = s.worldPos;
        }
    }
}
