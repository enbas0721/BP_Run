using UnityEngine;

public class MovingLight : MonoBehaviour
{
    public Transform yAxisPivot; // ¶‰E‰ñ“]—p
    public Transform xAxisPivot; // ã‰º‰ñ“]—p

    public float panSpeed = 1.0f;
    public float tiltSpeed = 1.2f;
    public float panRange = 45f;
    public float tiltRange = 30f;

    void Update()
    {
        // ¶‰E‚ÌñU‚èiSin”g‚Å‰•œ‚³‚¹‚éj
        float pan = Mathf.Sin(Time.time * panSpeed) * panRange;
        yAxisPivot.localRotation = Quaternion.Euler(0, pan, 0);

        // ã‰º‚ÌñU‚è
        float tilt = Mathf.Sin(Time.time * tiltSpeed) * tiltRange;
        xAxisPivot.localRotation = Quaternion.Euler(tilt, 0, 0);
    }
}