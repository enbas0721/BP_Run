using System.Collections.Generic;
using UnityEngine;

public static class ItemLaneSampling
{
    public struct Sample
    {
        public int lineIndex;
        public Vector3 worldPos;
    }

    /// <summary>
    /// anchor/basis/start/end/pattern/settings から、生成すべきワールド座標列を outSamplesに詰める
    /// outSamples は Clearされる。
    /// </summary>
    public static void SamplePositionsLocalZ (
        Transform anchorTransform, 
        Transform startMarker,
        Transform endMarker,
        ItemLanePattern pattern,
        ItemLaneSettings settings,
        List<Sample> outSamples
        )
    {
        outSamples.Clear();
        if (pattern == null || settings == null) return;

        Transform basis = anchorTransform.parent != null ? anchorTransform.parent : anchorTransform;

        // 開始/終了位置のマーカーを取得。（nullならデフォルト値を設定）
        Transform sTr = startMarker ? startMarker : anchorTransform;
        Transform eTr = endMarker ? endMarker : null;

        Vector3 sLocal = basis.InverseTransformPoint(sTr.position);
        Vector3 eLocal = eTr
            ? basis.InverseTransformPoint(eTr.position)
            : (sLocal + Vector3.forward * settings.defaultRangeLengthZ);

        float z0 = sLocal.z;
        float z1 = eLocal.z;
        if (Mathf.Approximately(z0, z1)) return;

        float zMin = Mathf.Min(z0, z1);
        float zMax = Mathf.Max(z0, z1);
        float lengthZ = zMax - zMin;
        if (lengthZ <= 0.001f) return;

        float spacing = Mathf.Max(0.001f, settings.itemSpacing);
        int count = Mathf.Max(1, Mathf.FloorToInt(lengthZ / spacing) + 1);

        float baseX = sLocal.x;
        float baseY = sLocal.y;

        bool forward = (z1 >= z0);
        float zStart = forward ? zMin : zMax;
        float zEnd = forward ? zMax : zMin;

        for (int li = 0; li < pattern.lines.Count; li++)
        {
            var line = pattern.lines[li];
            if (line == null) continue;

            for (int i = 0; i < count; i++)
            {
                float t = (count == 1) ? 0f : (float)i / (count - 1);

                float laneF = line.lane.Evaluate(t);
                int lane = Mathf.Clamp(Mathf.RoundToInt(laneF), -1, 1);

                float y = line.y.Evaluate(t) * settings.verticalStep;
                float z = Mathf.Lerp(zStart, zEnd, t);

                Vector3 localPos = new Vector3(
                    baseX + lane * settings.laneWidth,
                    baseY + y,
                    z
                );

                Vector3 worldPos = basis.TransformPoint(localPos);

                outSamples.Add(new Sample { lineIndex = li, worldPos = worldPos });
            }
        }
    }
}
