using UnityEngine;

public class SegmentBase : MonoBehaviour
{
    public Transform endPoint;
    public Transform obstaclePoints;

    public float SegmentLength => endPoint.localPosition.z;

    public void RebuildObstacles(ObstaclePlacer placer)
    {
        if (!obstaclePoints || !placer) return;

        for (int i = obstaclePoints.childCount - 1; i >= 0; i--)
        {
            var child = obstaclePoints.GetChild(i);

            if (child.GetComponent<ObstacleSpawnPoint>() != null) continue;

            Destroy(child.gameObject);
        }

        /* 確率によって、セグメント内に障害物を配置するかどうかはPlacerが決める */
        placer.Place(obstaclePoints);
    }

    public float GetEndZ()
    {
        return transform.position.z + SegmentLength;
    }
}