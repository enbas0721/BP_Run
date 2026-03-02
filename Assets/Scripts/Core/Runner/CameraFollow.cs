using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

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


        transform.position = target.position + offset;
    }
}
