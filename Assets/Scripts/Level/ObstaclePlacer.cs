/*
 * ObstaclePlacer.cs
 * 障害物配置担当クラス
 */

using UnityEngine;

public class ObstaclePlacer : MonoBehaviour
{
    public GameObject obstaclePrefab;

    [Range(0f, 1f)]
    public float spawnChance = 0.5f;

    public void Place(Transform obstacleRoot)
    {
        var points = obstacleRoot.GetComponentsInChildren<ObstacleSpawnPoint>(includeInactive: false);

        foreach (var sp in points)
        {
            if (Random.value > spawnChance) continue;

            var o = Instantiate(obstaclePrefab, sp.transform.position, Quaternion.identity);
            o.transform.SetParent(obstacleRoot, worldPositionStays: true);
        }
    }
}
