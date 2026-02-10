using UnityEngine;

public class AdrenalineButtonUI : MonoBehaviour
{
    [SerializeField] private AdrenalineSystem adrenaline;

    public void OnClick()
    {
        if (!adrenaline) adrenaline = FindFirstObjectByType<AdrenalineSystem>();
        if (!adrenaline) return;
        adrenaline.ActivateRush();
    }
}
