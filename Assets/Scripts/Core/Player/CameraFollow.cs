using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    private void LateUpdate()
    {
        if (!target) return;

        if (GameManager.Instance != null &&
            GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        transform.position = target.position + offset;
    }
}
