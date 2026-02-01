using UnityEngine;

public class SegmentBase : MonoBehaviour
{
    [SerializeField] private Transform endPoint;

    /*[Header("Obstacle")]*/
    /*[SerializeField] private Transform obstaclePoints; */
    /*[SerializeField] private Transform obstacleInstancesRoot; */

    [Header("Item")]
    [SerializeField] private Transform itemPoints;
    [SerializeField] private Transform itemInstancesRoot;

    public float SegmentLength => endPoint.localPosition.z;

    /* 廃止 */
    /* セグメントにランダムでオブジェクトを配置する必要が出た時に再利用 */
    /*public void RebuildObstacles(ObstaclePlacer placer)
    {
        if (!obstaclePoints || !placer  || !obstacleInstancesRoot) return;

        for (int i = obstacleInstancesRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(obstacleInstancesRoot.GetChild(i).gameObject);
        }

        placer.Place(obstaclePoints, obstacleInstancesRoot);
    }*/

    public void RebuildItems(ItemLanePlacer placer)
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