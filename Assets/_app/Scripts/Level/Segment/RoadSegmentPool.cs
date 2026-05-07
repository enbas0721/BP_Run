/*
 * SegmentPool.cs
 * セグメントオブジェクトをキューで保持。
 */ 

using UnityEngine;
using System.Collections.Generic;

public class RoadSegmentPool : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public RoadSegmentBase segmentPrefab;
        [Tooltip("最初にプールしておくセグメントの数。<br>発生確率が低いものは小さくしておくとリソース削減できる。かも。")]
        public int warmCount = 3;
        [HideInInspector] public float weight = 1f;
        [Tooltip("LevelManagerによる難易度重み付けの基準レベル")]
        public DifficultyLevel difficultyLevel = DifficultyLevel.Easy;
        [Tooltip("ONにするとデバッグモード中はこのEntryのみが出現する")]
        public bool debugOnly = false;
    }

    [Header("Segment Variants")]
    public List<Entry> entries = new List<Entry>();

    [Header("Options")]
    [SerializeField] private bool avoidSameAsLast = false;
#if UNITY_EDITOR
    [Tooltip("ONにするとdebugOnlyフラグが立ったEntryのみ出現する（Editor専用）")]
    [SerializeField] private bool debugMode = false;
#endif

    private readonly List<Queue<RoadSegmentBase>> pools = new List<Queue<RoadSegmentBase>>();
    private readonly List<int> cand = new List<int>(32);
    private int lastIndex = -1;

#if UNITY_EDITOR
    [ContextMenu("Sort Entries by Difficulty")]
    private void SortEntriesByDifficulty()
    {
        entries.Sort((a, b) => a.difficultyLevel.CompareTo(b.difficultyLevel));
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    private void Awake()
    {
        pools.Clear();

        for (int i = 0; i < entries.Count; i++)
        {
            pools.Add(new Queue<RoadSegmentBase>());

            var e = entries[i];
            if (e.segmentPrefab == null || e.warmCount <= 0) continue;

            for (int n = 0; n < e.warmCount; n++)
            {
                var seg = CreateInstance(i);
                Release(seg);
            }
        }
    }

    public RoadSegmentBase Get()
    {
        int idx = PickIndexWeighted(avoidSameAsLast);
        if (idx < 0) return null;

        var q = pools[idx];
        RoadSegmentBase seg;
        
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

    public void Release(RoadSegmentBase seg)
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

    private RoadSegmentBase CreateInstance(int poolIndex)
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
#if UNITY_EDITOR
        bool isDebug = debugMode;
#else
        bool isDebug = false;
#endif
        float total = 0f;
        cand.Clear();

        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            if (e.segmentPrefab == null) continue;
            if (isDebug && !e.debugOnly) continue;
            if (!isDebug && e.weight <= 0f) continue;
            if (avoidSameAsLast && i == lastIndex && entries.Count > 1) continue;

            cand.Add(i);
            total += isDebug ? 1f : e.weight;
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
