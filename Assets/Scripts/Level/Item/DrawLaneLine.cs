using UnityEngine;

public class DrawLaneLine : MonoBehaviour
{
    [SerializeField] private ItemLaneSettings settings;
    [SerializeField] private Transform endPos;

    private void OnDrawGizmosSelected()
    {
        DrawLaneGuides();
    }

    private void DrawLaneGuides()
    {
        if (settings == null) return;

        Transform basis = transform;
        Vector3 basePos = transform.position;

        Vector3 sLocal = basis.InverseTransformPoint(basePos);

        Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.6f);

        float[] lanes = { -1f, 0f, 1f };
        float z0 = sLocal.z;
        float z1 = endPos.position.z;

        for (int i = 0; i < lanes.Length; i++)
        {
            float x = sLocal.x + lanes[i] * settings.laneWidth;
            Vector3 a = basis.TransformPoint(new Vector3(x, sLocal.y, z0));
            Vector3 b = basis.TransformPoint(new Vector3(x, sLocal.y, z1));
            Gizmos.DrawLine(a, b);
        }
    }
}
