using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Runner/ItemLanePattern")]
public class ItemLanePattern : ScriptableObject
{
    public LineTag tag;

    [Min(0f)] public float weight = 1f;

    [System.Serializable]
    public class Line
    {
        public AnimationCurve lane = AnimationCurve.Linear(0, 0, 1, 0);
        public AnimationCurve y = AnimationCurve.Linear(0, 0, 1, 0);
    }

    public List<Line> lines = new();

}

[System.Serializable]
public struct StepPoint
{
    [Tooltip("レーン：-1=左, 0=中, 1=右（3レーン固定）")]
    public int lane;
    
    [Tooltip("前方向ステップ（itemSpacingを掛ける）")]
    public int zStep;

    [Tooltip("上下ステップ（verticalStepを掛ける）。弧用。普段0でOK")]
    public int yStep;
}
