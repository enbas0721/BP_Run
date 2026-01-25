using UnityEngine;

public class SegmentBase : MonoBehaviour
{
    [SerializeField] private Transform endPoint;

    [Header("Obstacle")]
    [SerializeField] private Transform obstaclePoints;
    [SerializeField] private Transform obstacleInstancesRoot;

    [Header("Item")]
    [SerializeField] private Transform itemPoints;
    [SerializeField] private Transform itemInstancesRoot;

    public float SegmentLength => endPoint.localPosition.z;

    public void RebuildObstacles(ObstaclePlacer placer)
    {
        if (!obstaclePoints || !placer  || !obstacleInstancesRoot) return;

        for (int i = obstacleInstancesRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(obstacleInstancesRoot.GetChild(i).gameObject);
        }

        /* [TODO] 他のSpawnPointでの配置状況を考慮していないため全レーンに配置されうる*/
        /* 確率によって、セグメント内に障害物を配置するかどうかはPlacerが決める */
        placer.Place(obstaclePoints, obstacleInstancesRoot);
    }

    public void RebuildItems(ItemPlacer placer)
    {
        if (!placer || !itemPoints || !itemInstancesRoot) return;

        for (int i = itemInstancesRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(itemInstancesRoot.GetChild(i).gameObject);
        }

        placer.Place(itemPoints, itemInstancesRoot);
    }

    public float GetEndZ()
    {
        return transform.position.z + SegmentLength;
    }
}