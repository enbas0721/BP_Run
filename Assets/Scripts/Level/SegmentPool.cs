using UnityEngine;
using System.Collections.Generic;

public class SegmentPool : MonoBehaviour
{
    public SegmentBase segmentPrefab;
    public int poolSize = 5;

    private Queue<SegmentBase> pool = new Queue<SegmentBase>();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var seg = Instantiate(segmentPrefab, transform);
            seg.gameObject.SetActive(false);
            pool.Enqueue(seg);
        }
    }

    public SegmentBase Get()
    {
        var seg = pool.Dequeue();
        seg.gameObject.SetActive(true);
        pool.Enqueue(seg);
        return seg;
    }
}
