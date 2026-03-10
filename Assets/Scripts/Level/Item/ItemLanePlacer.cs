using System.Collections.Generic;
using UnityEngine;

public class ItemLanePlacer : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject itemPrefab;

    [Header("共有設定")]
    [SerializeField] private ItemLaneSettings settings;

    // 内部キャッシュ
    private readonly List<ItemLaneAnchor> _candidates = new List<ItemLaneAnchor>(32);
    private readonly List<ItemLaneSampling.Sample> _samples = new List<ItemLaneSampling.Sample>(256);

    public void Place(Transform anchorsRoot, Transform instancesRoot)
    {
        if (!itemPrefab || !anchorsRoot || !instancesRoot) return;
        if (settings == null) return;

        var anchors = anchorsRoot.GetComponentsInChildren<ItemLaneAnchor>(includeInactive: false);

        _candidates.Clear();
        float totalW = 0f;

        for (int i = 0; i < anchors.Length; i++)
        {
            var a = anchors[i];
            if (a == null) continue;
            if (a.pattern == null) continue;

            float w = Mathf.Max(0f, a.spawnWeight);
            if (w <= 0f) continue;

            _candidates.Add(a);
            totalW += w;
        }

        if (_candidates.Count == 0 || totalW <= 0f) return;

        float r = Random.value * totalW;
        float acc = 0f;
        ItemLaneAnchor picked = _candidates[_candidates.Count - 1];

        for (int i = 0; i < _candidates.Count; i++)
        {
            acc += Mathf.Max(0f, _candidates[i].spawnWeight);
            if (r <= acc)
            {
                picked = _candidates[i];
                break;
            }
        }

        ItemLaneSampling.SamplePositionsLocalZ(
            picked.transform, picked.startMarker, picked.endMarker, picked.pattern, settings, _samples);

        Transform parentForInstances = instancesRoot;

        Quaternion rot = (picked.transform.parent != null ? picked.transform.parent.rotation : picked.transform.rotation);

        for (int i = 0; i < _samples.Count; i++)
        {
            Instantiate(itemPrefab, _samples[i].worldPos, rot, parentForInstances);
        }
    }
}