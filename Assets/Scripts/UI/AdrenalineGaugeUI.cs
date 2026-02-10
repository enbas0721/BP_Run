using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdrenalineGaugeUI : MonoBehaviour
{
    [SerializeField] private AdrenalineSystem adrenaline;
    [SerializeField] private Image fillImage;

    private void Awake()
    {
        if (!adrenaline) adrenaline = FindFirstObjectByType<AdrenalineSystem>();
    }

    private void Update()
    {
        if (!adrenaline || !fillImage) return;

        float max = Mathf.Max(1f, adrenaline.GaugeMax);
        float t = Mathf.Clamp01(adrenaline.Gauge / max);

        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, t, 12f * Time.deltaTime);


        if (adrenaline.CanActivateRush)
        {
            fillImage.color = Color.yellow;
        }
        else
        {
            fillImage.color = Color.blue;
        }
    }
}
