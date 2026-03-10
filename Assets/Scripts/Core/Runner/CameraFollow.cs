using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;

    [Header("Y Limit")]
    [SerializeField] private float minY = 0f;
    [SerializeField] private float maxY = 5f;

    private Vector3 startPos;
    private Quaternion startRot;

    private void Awake()
    {
        startPos = this.transform.position;
        startRot = this.transform.rotation;
    }
    
    private void LateUpdate()
    {
        if (!target) return;

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.State == GameState.Ready)
            {
                transform.SetPositionAndRotation(startPos, startRot);
                return;

            }
            else if (GameManager.Instance.State != GameState.Playing)
            {
                return;
            }
        }

        Vector3 pos = target.position + offset;

        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }
}
