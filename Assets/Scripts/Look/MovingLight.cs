using UnityEngine;

public class MovingLight : MonoBehaviour
{
    public Transform yAxisPivot; // 左右回転用
    public Transform xAxisPivot; // 上下回転用

    public float panSpeed = 1.0f;
    public float tiltSpeed = 1.2f;
    public float panRange = 45f;
    public float tiltRange = 30f;

    private float panOffset;
    private float tiltOffset;

    void Awake()
    {
        panOffset  = Random.Range(0f, Mathf.PI * 2f);
        tiltOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // 左右の首振り（Sin波で往復させる）
        float pan = Mathf.Sin(Time.time * panSpeed + panOffset) * panRange;
        yAxisPivot.localRotation = Quaternion.Euler(0, pan, 0);

        // 上下の首振り
        float tilt = Mathf.Sin(Time.time * tiltSpeed + tiltOffset) * tiltRange;
        xAxisPivot.localRotation = Quaternion.Euler(tilt, 0, 0);
    }
}