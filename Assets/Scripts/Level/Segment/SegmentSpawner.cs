/*
 * SegmentSpwner.cs
 * セグメントの配置先や配置セグメントを管理・決定
 */

using UnityEngine;
using System.Collections.Generic;

public class SegmentSpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private SegmentPool pool;

    /* 障害物の自動生成は無効化 */
    /* [Header("Obstacle")] */
    /* [SerializeField] private ObstaclePlacer obstaclePlacer; */

    [Header("Item")]
    [SerializeField] private ItemLanePlacer itemPlacer;

    [Header("Spawn Control")]
    [Tooltip("プレイヤーの前方に確保したい床の距離")]
    [SerializeField] private float aheadDistance = 60f;

    [Tooltip("初期に敷く最低枚数（見た目のため）")]
    [SerializeField] private int initialSegments = 4;

    [Tooltip("プレイヤーの後方で回収する距離（セグメント終端がこの距離だけ後ろなら回収）")]
    [SerializeField] private float behindDistance = 30f;

    private float spawnZ = 0f;
    private readonly Queue<SegmentBase> activeSegments = new Queue<SegmentBase>();

    void Start()
    {
        for (int i = 0; i < initialSegments; i++)
        {
            SpawnSegment();
        }
    }

    void Update()
    {
        // プレイヤー前方の確保距離を満たすまで、必要枚数をまとめて生成
        while (spawnZ <= player.position.z + aheadDistance)
        {
            if (!SpawnSegment())
            {
                /* Segmentが生成できなかったらbreak(Editorのフリーズ回避) */
                break;
            }
        }

        ReleaseOldSegments();
    }

    private bool SpawnSegment()
    {
        var seg = pool.Get();
        if (seg == null)
        {
            Debug.LogError("SpawnSegment failed: pool.Get() returned null");
            return false;
        }

        seg.transform.position = new Vector3(0, 0, spawnZ);

        /* 障害物はセグメントに手動追加するので自動生成は無効化 */
        /* seg.RebuildObstacles(obstaclePlacer); */
        
        /* [TODO] アイテム列の破壊と生成をセグメントに任せる。 */
        seg.RebuildItems(itemPlacer);

        activeSegments.Enqueue(seg);
        spawnZ += seg.SegmentLength;

        return true;
    }
    private void ReleaseOldSegments()
    {
        while (activeSegments.Count > 0)
        {
            var head = activeSegments.Peek();
            float headEndZ = head.GetEndZ();

            if (headEndZ < player.position.z - behindDistance)
            {
                activeSegments.Dequeue();
                pool.Release(head);
            }
            else
            {
                break;
            }
        }
    }
}
