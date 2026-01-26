/*
 * SegmentPool.cs
 * セグメントオブジェクトをキューで保持。
 */ 

using UnityEngine;
using System.Collections.Generic;

public class SegmentPool : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    { 
        public SegmentBase segmentPrefab;
        [Tooltip("最初にプールしておくセグメントの数。<br>発生確率が低いものは小さくしておくとリソース削減できる。かも。")]
        public int warmCount = 3;
        [Tooltip("生成確率 [0f~1f]")]
        [Min(0f)] public float weight = 1f;
    }

    [Header("Segment Variants")]
    public List<Entry> entries = new List<Entry>();

    [Header("Options")]
    [SerializeField] private bool avoidSameAsLast = false;

    private readonly List<Queue<SegmentBase>> pools = new List<Queue<SegmentBase>>();
    private int lastIndex = -1;

    private void Awake()
    {
        pools.Clear();

        for (int i = 0; i < entries.Count; i++)
        {
            pools.Add(new Queue<SegmentBase>());

            var e = entries[i];
            if (e.segmentPrefab == null || e.warmCount <= 0) continue;

            for (int n = 0; n < e.warmCount; n++)
            {
                var seg = CreateInstance(i);
                Release(seg);
            }
        }
    }

    public SegmentBase Get()
    {
        int idx = PickIndexWeighted(avoidSameAsLast);
        if (idx < 0) return null;

        var q = pools[idx];
        SegmentBase seg;
        
        if (q.Count > 0)
        {
            seg = q.Dequeue();
        }
        else
        {
            seg = CreateInstance(idx);
        }

        seg.gameObject.SetActive(true);
        lastIndex = idx;
        return seg;
    }

    public void Release(SegmentBase seg)
    {
        if (seg == null) return;

        if (!seg.TryGetComponent<PooledSegment>(out var ps) || ps.PoolIndex < 0 || ps.PoolIndex >= pools.Count)
        {
            Destroy(seg.gameObject);
            return;
        }

        seg.gameObject.SetActive(false);
        seg.transform.SetParent(transform, false);
        pools[ps.PoolIndex].Enqueue(seg);
    }

    private SegmentBase CreateInstance(int poolIndex)
    {
        var prefab = entries[poolIndex].segmentPrefab;
        var seg = Instantiate(prefab, transform);

        var ps = seg.GetComponent<PooledSegment>();
        if (ps == null) ps = seg.gameObject.AddComponent<PooledSegment>();

        ps.BindToPool(poolIndex);

        seg.gameObject.SetActive(false);
        return seg;
    }

    /// <Summary>
    /// ret < 0 の場合エラー
    /// </Summary>
    private int PickIndexWeighted(bool avoidSameAsLast)
    {
        var cand = new List<int>();
        float total = 0f;

        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            if (e.segmentPrefab == null) continue;
            if (e.weight <= 0f) continue;
            if (avoidSameAsLast && i == lastIndex && entries.Count > 1) continue;

            cand.Add(i);
            total += e.weight;
        }

        if (cand.Count == 0)
        {
            if (avoidSameAsLast && lastIndex >= 0)
            {
                /* avoidSameAsLast有効時候補がなかったならlastIndexが唯一の候補 */
                return lastIndex;
            }
            else
            {
                return -1;
            }
        }

        float r = Random.value * total;
        float acc = 0f;
        foreach (var i in cand)
        {
            acc += entries[i].weight;
            if (r <= acc) return i;
        }

        return cand[cand.Count - 1];
    }
}
