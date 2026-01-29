using UnityEngine;

public class ItemLaneAnchor : MonoBehaviour
{
    [Range(0f, 1f)] public float spawnChance = 1f;
    public LineTag[] allowedTags;

    [Header("Local-Z Range")]
    public Transform startMarker;
    public Transform endMarker;

    [Header("Gizmo Preview (Editor)")]
    public bool drawPreviewGizmo = true;

    [Tooltip("プレビューしたいパターン（ScriptableObject）")]
    public ItemLanePattern previewPattern;

    [Tooltip("表示する点の大きさ")]
    public float gizmoPointRadius = 0.12f;

    [Tooltip("点を線で結ぶ")]
    public bool drawPolyline = true;

    [Header("Preview Tuning (match ItemLanePlacer)")]
    [Tooltip("隣レーン中心間距離")]
    public float laneWidth = 1.5f;

    [Tooltip("アイテム間隔（Z方向）")]
    public float itemSpacing = 1.0f;

    [Tooltip("上下方向の1ステップ距離")]
    public float verticalStep = 0.5f;

    [Tooltip("endMarker未指定時の既定Z長")]
    public float defaultRangeLengthZ = 10f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1.0f);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawPreviewGizmo) return;
        if (previewPattern == null) return;

        Transform basis = transform.parent != null ? transform.parent : transform;

        Transform sTr = startMarker ? startMarker : transform;
        Transform eTr = endMarker ? endMarker : null;

        Vector3 sLocal = basis.InverseTransformPoint(sTr.position);
        Vector3 eLocal = eTr ? basis.InverseTransformPoint(eTr.position)
                             : (sLocal + Vector3.forward * defaultRangeLengthZ);

        float z0 = sLocal.z;
        float z1 = eLocal.z;
        if (Mathf.Approximately(z0, z1)) return;

        float zMin = Mathf.Min(z0, z1);
        float zMax = Mathf.Max(z0, z1);
        float lengthZ = zMax - zMin;
        if (lengthZ <= 0.001f) return;

        int count = Mathf.Max(1, Mathf.FloorToInt(lengthZ / Mathf.Max(0.001f, itemSpacing)) + 1);

        bool forward = (z1 >= z0);
        float zStart = forward ? zMin : zMax;
        float zEnd = forward ? zMax : zMin;

        Gizmos.color = new Color(1f, 0.7f, 0.2f, 1f);
        Vector3 wStart = basis.TransformPoint(new Vector3(sLocal.x, sLocal.y, zStart));
        Vector3 wEnd = basis.TransformPoint(new Vector3(sLocal.x, sLocal.y, zEnd));
        Gizmos.DrawLine(wStart, wEnd);

        DrawLaneGuides(basis, sLocal, zStart, zEnd);

        for (int li = 0; li < previewPattern.lines.Count; li++)
        {
            var line = previewPattern.lines[li];
            if (line == null) continue;

            Gizmos.color = (li % 2 == 0) ? Color.magenta : Color.cyan;

            Vector3? prev = null;

            for (int i = 0; i < count; i++)
            {
                float t = (count == 1) ? 0f : (float)i / (count - 1);

                float laneF = line.lane.Evaluate(t);
                int lane = Mathf.Clamp(Mathf.RoundToInt(laneF), -1, 1);

                float y = line.y.Evaluate(t) * verticalStep;
                float z = Mathf.Lerp(zStart, zEnd, t);

                Vector3 localPos = new Vector3(
                    sLocal.x + lane * laneWidth,
                    sLocal.y + y,
                    z
                );

                Vector3 worldPos = basis.TransformPoint(localPos);

                Gizmos.DrawWireSphere(worldPos, gizmoPointRadius);

                if (drawPolyline && prev.HasValue)
                    Gizmos.DrawLine(prev.Value, worldPos);

                prev = worldPos;
            }
        }
    }

    private void DrawLaneGuides(Transform basis, Vector3 sLocal, float zStart, float zEnd)
    {
        Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.6f);

        float[] lanes = { -1f, 0f, 1f };
        for (int i = 0; i < lanes.Length; i++)
        {
            float x = sLocal.x + lanes[i] * laneWidth;

            Vector3 a = basis.TransformPoint(new Vector3(x, sLocal.y, zStart));
            Vector3 b = basis.TransformPoint(new Vector3(x, sLocal.y, zEnd));
            Gizmos.DrawLine(a, b);
        }
    }
}
