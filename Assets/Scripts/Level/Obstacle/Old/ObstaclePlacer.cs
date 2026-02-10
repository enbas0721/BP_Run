/*
 * ObstaclePlacer.cs
 * 障害物配置担当クラス
 */

using UnityEngine;

public class ObstaclePlacer : MonoBehaviour
{
    [SerializeField] private GameObject obstaclePrefab;

    [Range(0f, 1f)]
    [SerializeField] private float spawnChance = 0.5f;

    public void Place(Transform spawnPointsRoot, Transform instancesRoot)
    {
        if (!obstaclePrefab || !spawnPointsRoot || !instancesRoot) return;

        var points = spawnPointsRoot.GetComponentsInChildren<ObstacleSpawnPoint>(includeInactive: false);

        foreach (var sp in points)
        {
            if (Random.value > spawnChance) continue;

            var o = Instantiate(obstaclePrefab, sp.transform.position, Quaternion.identity);
            o.transform.SetParent(instancesRoot, worldPositionStays: true);
        }
    }
}
