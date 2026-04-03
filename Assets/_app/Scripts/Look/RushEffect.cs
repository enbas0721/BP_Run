using UnityEngine;

/// <summary>
/// Rush中のLight ON・マテリアル差し替えを担当する。
/// EffectControllerから Activate / Deactivate が呼ばれる。
/// </summary>
public class RushEffect : MonoBehaviour
{
    [Header("Light")]
    [SerializeField] private Light pointLight;

    [Header("Material Swap")]
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;

    private Material[] originalMaterials;

    private void Awake()
    {
        // 元のマテリアルをキャッシュ
        if (renderers != null && inactiveMaterial == null)
        {
            originalMaterials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i]) originalMaterials[i] = renderers[i].material;
        }

        Deactivate();
    }

    public void Activate()
    {
        if (pointLight) pointLight.enabled = true;
        ApplyMaterial(activeMaterial);
    }

    public void Deactivate()
    {
        if (pointLight) pointLight.enabled = false;
        ApplyMaterial(inactiveMaterial);
    }

    private void ApplyMaterial(Material mat)
    {
        if (!mat || renderers == null) return;
        foreach (var r in renderers)
            if (r) r.material = mat;
    }
}
