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
    [SerializeField] private RoadSegmentPool roadPool;
    [SerializeField] private ItemLanePlacer itemPlacer;
    [Tooltip("開始時点の前方オフセット（スタートセグメントの長さ分）")]
    [SerializeField] private float roadInitialAheadOffset = 20f;
    [Tooltip("プレイヤーの前方に確保したい床の距離")]
    [SerializeField] private float aheadDistance = 60f;
    [Tooltip("初期に敷く最低枚数（見た目のため）")]
    [SerializeField] private int roadInitialSegments = 4;
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

    private readonly Queue<RoadSegmentBase> activeRoadSegments = new Queue<RoadSegmentBase>();
    private readonly Queue<EnvSegmentBase> activeEnvSegments = new Queue<EnvSegmentBase>();

    void Start()
    {
        ResetSegments();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            return;

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

    public void ResetSegments()
    {
        // 既存セグメントの回収
        while (activeRoadSegments.Count > 0)
        {
            var seg = activeRoadSegments.Dequeue();
            roadPool.Release(seg);
        }

        while (activeEnvSegments.Count > 0)
        {
            var env = activeEnvSegments.Dequeue();
            envPool.Release(env);
        }

        // スポーン位置の初期化
        roadSpawnZ = roadInitialAheadOffset;
        envSpawnZ = envInitialAheadOffset;

        // 初期セグメントの生成
        for (int i = 0; i < roadInitialSegments; i++ )
        {
            if (!SpawnRoadSegment()) break;
        }
        for (int i = 0; i < envInitialSegment; i++)
        {
            if (!SpawnEnvSegment()) break;
        }
    }

    private bool SpawnRoadSegment()
    {
        var seg = roadPool.Get();
        if (seg == null)
        {
            Debug.LogError("SpawnSegment failed: roadPool.Get() returned null");
            return false;
        }

        seg.transform.position = new Vector3(0, 0, roadSpawnZ);

        /* 障害物はセグメントに手動配置しておくので自動生成は無効化 */
        /* seg.RebuildObstacles(obstaclePlacer); */

        seg.RebuildItems(itemPlacer);

        activeRoadSegments.Enqueue(seg);
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
        while (activeRoadSegments.Count > 0)
        {
            var head = activeRoadSegments.Peek();
            float headEndZ = head.GetEndZ();

            if (headEndZ < player.position.z - behindDistance)
            {
                activeRoadSegments.Dequeue();
                roadPool.Release(head);
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
