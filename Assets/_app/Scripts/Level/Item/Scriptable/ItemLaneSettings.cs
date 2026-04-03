using UnityEngine;

[CreateAssetMenu(fileName = "ItemLaneSettings", menuName = "Runner/Item Lane Settings")]
public class ItemLaneSettings : ScriptableObject
{
    [Header("Lane / Spacing")]
    public float laneWidth = 3.0f;
    public float itemSpacing = 1.0f;
    public float verticalStep = 0.5f;

    [Header("If endMarker is null")]
    public float defaultRangeLengthZ = 10f;
}
