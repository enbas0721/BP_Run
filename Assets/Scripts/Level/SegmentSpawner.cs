/*
 * SegmentSpwner.cs
 * セグメントの配置先や配置セグメントを管理・決定
 */

using UnityEngine;

public class SegmentSpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private SegmentPool pool;

    [Header("Obstacle")]
    [SerializeField] private ObstaclePlacer obstaclePlacer;

    [Header("Item")]
    [SerializeField] private ItemPlacer itemPlacer;

    [Header("Spawn Control")]
    [Tooltip("プレイヤーの前方に確保したい床の距離")]
    [SerializeField] private float aheadDistance = 60f;

    [Tooltip("初期に敷く最低枚数（見た目のため）")]
    [SerializeField] private int initialSegments = 4;

    private float spawnZ = 0f;

    void Start()
    {
        for (int i = 0; i < initialSegments; i++)
            SpawnSegment();
    }

    void Update()
    {
        // プレイヤー前方の確保距離を満たすまで、必要枚数をまとめて生成
        while (spawnZ < player.position.z + aheadDistance)
        {
            SpawnSegment();
        }
    }

    void SpawnSegment()
    {
        var seg = pool.Get();
        seg.transform.position = new Vector3(0, 0, spawnZ);

        /* セグメント内への障害物の破壊と生成はセグメントに任せる。 */
        seg.RebuildObstacles(obstaclePlacer);
        seg.RebuildItems(itemPlacer);

        spawnZ += seg.SegmentLength;
    }
}
