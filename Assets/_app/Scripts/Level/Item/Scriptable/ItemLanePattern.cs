using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Runner/ItemLanePattern")]
public class ItemLanePattern : ScriptableObject
{
    [Min(0f)] public float weight = 1f;

    [System.Serializable]
    public class Line
    {
        public AnimationCurve lane = AnimationCurve.Linear(0, 0, 1, 0);
        public AnimationCurve y = AnimationCurve.Linear(0, 0, 1, 0);
    }

    public List<Line> lines = new();

}