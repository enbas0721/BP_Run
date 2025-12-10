using UnityEngine;

public class SegmentSpawner : MonoBehaviour
{
    public Transform player;
    public SegmentPool pool;
    public int initialSegments = 4;

    private float spawnZ = 0f;

    void Start()
    {
        // 最初に数枚敷く
        for (int i = 0; i < initialSegments; i++)
            SpawnSegment();
    }

    void Update()
    {
        // プレイヤーが前進したら次の Segment を追加
        if (player.position.z > spawnZ - (initialSegments * 10f))
        {
            SpawnSegment();
        }
    }

    void SpawnSegment()
    {
        var seg = pool.Get();

        seg.transform.position = new Vector3(0, 0, spawnZ);
        spawnZ += seg.SegmentLength;
    }
}
