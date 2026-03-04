using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdrenalineGaugeUI : MonoBehaviour
{
    [SerializeField] private AdrenalineSystem adrenaline;
    [SerializeField] private Image fillImage;

    [Header("Head Marker")]
    [SerializeField] private RectTransform barRect;
    [SerializeField] private RectTransform headMarker;
    [SerializeField] private float headPadding = 0f;
    [SerializeField] private float headSmooth = 20f;

    private void Awake()
    {
        if (!adrenaline) adrenaline = FindFirstObjectByType<AdrenalineSystem>();

        if (!barRect && fillImage) barRect = fillImage.transform.parent as RectTransform;
    }

    private void Update()
    {
        if (!adrenaline || !fillImage) return;

        float target;

        if (adrenaline.IsRushActive)
        {
            target = adrenaline.RushRemaining01;
            /* 満タン時色を帰る場合は有効に */
            /* fillImage.color = Color.yellow; */
        }
        else
        {
            float max = Mathf.Max(1f, adrenaline.GaugeMax);
            target = Mathf.Clamp01(adrenaline.Gauge / max);

            /* fillImage.color = adrenaline.CanActivateRush ? Color.yellow : Color.blue; */
        }

        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, target, 12f * Time.deltaTime);

        UpdateHeadMarker(fillImage.fillAmount);
    }

    private void UpdateHeadMarker(float fill01)
    {
        if (!headMarker || !barRect) return;

        float height = barRect.rect.height;

        // 下端 = -pivot.y * height
        float bottomY = -barRect.pivot.y * height;
        float topY = bottomY + height;

        // fill01(0..1) -> Y(下→上)
        float y = Mathf.Lerp(bottomY + headPadding, topY - headPadding, fill01);

        Vector2 a = headMarker.anchoredPosition;
        float newY = Mathf.Lerp(a.y, y, headSmooth * Time.deltaTime);
        headMarker.anchoredPosition = new Vector2(a.x, newY);
        headMarker.anchoredPosition = new Vector2(a.x, newY);
    }
}
