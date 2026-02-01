using UnityEngine;

public class PooledSegment : MonoBehaviour
{
    public int PoolIndex { get; private set; } = -1;

    public void BindToPool(int poolIndex)
    {
        PoolIndex = poolIndex;
    }
}
