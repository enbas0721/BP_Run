/*
 * SegmentSpwner.cs
 * セグメントの配置先や配置セグメントを管理・決定
 */

using UnityEngine;
using System.Collections.Generic;

public class SegmentSpawner : MonoBehaviour
{
    [SerializeField] private Transform player;

    [Header("Game Segment (Road)")]
    [SerializeField] private SegmentPool pool;
    [SerializeField] private ItemLanePlacer itemPlacer;
    [Tooltip("開始時点の前方オフセット（スタートセグメントの長さ分）")]
    [SerializeField] private float initialAheadOffset = 20f;
    [Tooltip("プレイヤーの前方に確保したい床の距離")]
    [SerializeField] private float aheadDistance = 60f;
    [Tooltip("初期に敷く最低枚数（見た目のため）")]
    [SerializeField] private int initialSegments = 4;
    [Tooltip("プレイヤーの後方で回収する距離（セグメント終端がこの距離だけ後ろなら回収）")]
    [SerializeField] private float behindDistance = 30f;

    [Header("Env Segment (Walls/Ceiling)")]
    [SerializeField] private EnvSegmentPool envPool;
    [SerializeField] private float envInitialAheadOffset = 20f;
    [SerializeField] private float envAheadDistance = 80f;
    [SerializeField] private int envInitialSegment = 3;
    [SerializeField] private float envBehindDistance = 40f;

    /* 障害物の自動生成は無効化 */
    /* [Header("Obstacle")] */
    /* [SerializeField] private ObstaclePlacer obstaclePlacer; */

    private float roadSpawnZ = 0f;
    private float envSpawnZ = 0f;

    private readonly Queue<SegmentBase> activeSegments = new Queue<SegmentBase>();
    private readonly Queue<EnvSegmentBase> activeEnvSegments = new Queue<EnvSegmentBase>();

    void Start()
    {
        roadSpawnZ = initialAheadOffset;
        envSpawnZ = envInitialAheadOffset;

        for (int i = 0; i < initialSegments; i++)
        {
            SpawnRoadSegment();
        }

        for (int i = 0; i < envInitialSegment; i++)
        {
            SpawnEnvSegment();
        }
    }

    void Update()
    {
        // プレイヤー前方の確保距離を満たすまで、必要枚数をまとめて生成
        while (roadSpawnZ <= player.position.z + aheadDistance)
        {
            if (!SpawnRoadSegment())
            {
                /* Segmentが生成できなかったらbreak(Editorのフリーズ回避) */
                break;
            }
        }

        while (envSpawnZ <= player.position.z + envAheadDistance)
        {
            if (!SpawnEnvSegment())
            {
                break;
            }
        }

        ReleaseOldRoadSegments();
        ReleaseOldEnvSegments();
    }

    private bool SpawnRoadSegment()
    {
        var seg = pool.Get();
        if (seg == null)
        {
            Debug.LogError("SpawnSegment failed: pool.Get() returned null");
            return false;
        }

        seg.transform.position = new Vector3(0, 0, roadSpawnZ);

        /* 障害物はセグメントに手動配置しておくので自動生成は無効化 */
        /* seg.RebuildObstacles(obstaclePlacer); */

        seg.RebuildItems(itemPlacer);

        activeSegments.Enqueue(seg);
        roadSpawnZ += seg.SegmentLength;

        return true;
    }

    private bool SpawnEnvSegment()
    {
        var env = envPool.Get();
        if (env == null)
        {
            Debug.LogError("SpawnEnvSegmnet failed: envPool.Get() returned null.");
            return false;
        }

        env.transform.position = new Vector3(0, 0, envSpawnZ);

        activeEnvSegments.Enqueue(env);
        envSpawnZ += env.SegmentLength;
        
        return true;
    }

    private void ReleaseOldRoadSegments()
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
    private void ReleaseOldEnvSegments()
    {
        while (activeEnvSegments.Count > 0)
        {
            var head = activeEnvSegments.Peek();
            if (head.GetEndZ() < player.position.z - envBehindDistance)
            {
                activeEnvSegments.Dequeue();
                envPool.Release(head);
            }
            else break;
        }
    }
}
