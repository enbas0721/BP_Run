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

        float target;

        if (adrenaline.IsRushActive)
        {
            target = adrenaline.RushRemaining01;
            fillImage.color = Color.yellow;
        }
        else
        {
            float max = Mathf.Max(1f, adrenaline.GaugeMax);
            target = Mathf.Clamp01(adrenaline.Gauge / max);

            fillImage.color = adrenaline.CanActivateRush ? Color.yellow : Color.blue;
        }

        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, target, 12f * Time.deltaTime);
    }
}
