using UnityEngine;

public class SegmentBase : MonoBehaviour
{
    public Transform endPoint;

    // EndPoint の Z 位置が Segment の長さ
    public float SegmentLength => endPoint.localPosition.z;

    // この Segment の世界座標での終端 Z 座標を返す（拡張用）
    public float GetEndZ()
    {
        return transform.position.z + SegmentLength;
    }
}