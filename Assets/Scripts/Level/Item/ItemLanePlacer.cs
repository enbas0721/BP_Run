using System.Collections.Generic;
using UnityEngine;

public class ItemLanePlacer : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject itemPrefab;

    [Header("Lane / Spacing (global)")]
    [Tooltip("隣のレーン中心までの距離")]
    [SerializeField] private float laneWidth = 1.5f;
    [Tooltip("アイテム間隔(倍率)")]
    [SerializeField] private float itemSpacing = 1.0f;
    [Tooltip("上下方向の距離（ジャンプ孤に利用）")]
    [SerializeField] private float verticalStep = 0.5f;

    [Header("If endMarker is null")]
    [SerializeField] private float defaultRangeLengthZ = 10f;

    [Header("Patternes")]
    [SerializeField] private List<ItemLanePattern> patterns = new();

    public void Place(Transform anchorsRoot, Transform instancesRoot)
    {
        if (!itemPrefab || !anchorsRoot || !instancesRoot) return;

        var anchors = anchorsRoot.GetComponentsInChildren<ItemLaneAnchor>(includeInactive: false);

        foreach (var anchor in anchors)
        {
            if (Random.value > anchor.spawnChance) continue;

            var pattern = PickPattern(anchor);
            if (pattern == null) continue;

            SpawnPattern(anchor, pattern, instancesRoot);
        }
    }

    private ItemLanePattern PickPattern(ItemLaneAnchor anchor)
    {
        bool useFilter = anchor.allowedTags != null && anchor.allowedTags.Length > 0;

        float total = 0f;
        for (int i = 0; i < patterns.Count; i++)
        {
            var p = patterns[i];
            if (!p || p.weight <= 0) continue;
            if (useFilter && !Contains(anchor.allowedTags, p.tag)) continue;
            total += p.weight;
        }
        if (total <= 0f) return null;

        float r = Random.value * total;
        float acc = 0f;
        for (int i = 0; i < patterns.Count; i++)
        {
            var p = patterns[i];
            if (!p || p.weight <= 0f) continue;
            if (useFilter && !Contains(anchor.allowedTags, p.tag)) continue;

            acc += p.weight;
            if (r <= acc) return p; 
        }

        return patterns[patterns.Count - 1];
    }

    private static bool Contains(LineTag[] tags, LineTag t)
    {
        for (int i = 0; i < tags.Length; i++)
        {
            if (tags[i] == t) return true;
        }
        return false;
    }

    private void SpawnPattern(ItemLaneAnchor anchor, ItemLanePattern pattern, Transform instancesRoot)
    {
        // 基準ローカル空間：anchorの親（通常 itemPoints 配下なので安定）
        Transform basis = anchor.transform.parent != null ? anchor.transform.parent : anchor.transform;

        Transform sTr = anchor.startMarker ? anchor.startMarker : anchor.transform;
        Transform eTr = anchor.endMarker ? anchor.endMarker : null;

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

        int count = Mathf.Max(1, Mathf.FloorToInt(lengthZ / itemSpacing) + 1);

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

                float y = line.y.Evaluate(t) * verticalStep;

                float z = Mathf.Lerp(zStart, zEnd, t);

                Vector3 localPos = new Vector3(
                    baseX + lane * laneWidth,
                    baseY + y,
                    z
                );

                Vector3 worldPos = basis.TransformPoint(localPos);

                Instantiate(itemPrefab, worldPos, basis.rotation, instancesRoot);
            }
        }
    }
}
