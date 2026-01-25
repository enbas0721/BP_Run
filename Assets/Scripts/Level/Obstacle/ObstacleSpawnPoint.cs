using UnityEngine;

public enum LaneId { Left = -1, Center = 0, Right = 1 }

public class ObstacleSpawnPoint : MonoBehaviour
{
    public LaneId lane = LaneId.Center;
}
