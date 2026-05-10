using UnityEngine;

public class BoneRotation : MonoBehaviour
{
    [Header("Rotation Speed (degrees/sec)")]
    public float speedX = 0f;
    public float speedY = 90f;
    public float speedZ = 0f;

    [Header("Options")]
    public Space rotationSpace = Space.Self;
    public bool randomizeOnStart = false;
    public float randomSpeedMin = -180f;
    public float randomSpeedMax = 180f;

    [Header("Pulse (optional)")]
    public bool usePulse = false;
    public float pulseFrequency = 1f;   // Hz
    public float pulseMinMultiplier = 0.5f;
    public float pulseMaxMultiplier = 1.5f;

    private Vector3 baseSpeed;

    void Start()
    {
        if (randomizeOnStart)
        {
            speedX = Random.Range(randomSpeedMin, randomSpeedMax);
            speedY = Random.Range(randomSpeedMin, randomSpeedMax);
            speedZ = Random.Range(randomSpeedMin, randomSpeedMax);
        }

        baseSpeed = new Vector3(speedX, speedY, speedZ);
    }

    void Update()
    {
        Vector3 currentSpeed = baseSpeed;

        if (usePulse)
        {
            float pulse = Mathf.Lerp(pulseMinMultiplier, pulseMaxMultiplier,
                          (Mathf.Sin(Time.time * pulseFrequency * Mathf.PI * 2f) + 1f) * 0.5f);
            currentSpeed *= pulse;
        }

        transform.Rotate(currentSpeed * Time.deltaTime, rotationSpace);
    }

    // 外部から速度を上書きするユーティリティ
    public void SetSpeed(float x, float y, float z)
    {
        speedX = x; speedY = y; speedZ = z;
        baseSpeed = new Vector3(x, y, z);
    }

    public void Stop() => baseSpeed = Vector3.zero;
    public void Resume() => baseSpeed = new Vector3(speedX, speedY, speedZ);
}