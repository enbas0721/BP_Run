using UnityEngine;

public class EnvSegmentBase : MonoBehaviour
{
    [SerializeField] private Transform endPoint;

    public float SegmentLength => endPoint.localPosition.z;

    public float GetEndZ()
    {
        return transform.position.z + SegmentLength;
    }
}