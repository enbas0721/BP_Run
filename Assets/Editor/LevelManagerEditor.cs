using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
    private static readonly string[] LevelNames = { "Easy", "Normal", "Hard", "VeryHard" };
    private static readonly Color[] LevelColors =
    {
        new Color(0.3f, 0.8f, 0.3f),   // Easy  : 緑
        new Color(0.3f, 0.6f, 1.0f),   // Normal: 青
        new Color(1.0f, 0.7f, 0.2f),   // Hard  : 橙
        new Color(1.0f, 0.3f, 0.3f),   // VHard : 赤
    };

    private SerializedProperty roadSegmentPool;
    private SerializedProperty scoreAtMaxDifficulty;
    private SerializedProperty maxDifficulty;
    private SerializedProperty sigma;
    private SerializedProperty minWeight;
    private SerializedProperty debugScore;

    private void OnEnable()
    {
        roadSegmentPool       = serializedObject.FindProperty("roadSegmentPool");
        scoreAtMaxDifficulty  = serializedObject.FindProperty("scoreAtMaxDifficulty");
        maxDifficulty         = serializedObject.FindProperty("maxDifficulty");
        sigma                 = serializedObject.FindProperty("sigma");
        minWeight             = serializedObject.FindProperty("minWeight");
        debugScore            = serializedObject.FindProperty("debugScore");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(roadSegmentPool);
        EditorGUILayout.Space(4);

        EditorGUILayout.LabelField("Difficulty Curve", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(scoreAtMaxDifficulty);
        EditorGUILayout.PropertyField(maxDifficulty);
        EditorGUILayout.PropertyField(sigma);
        EditorGUILayout.PropertyField(minWeight);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Debug (Editor Only)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(debugScore);

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(8);
        DrawDistributionGraph();
    }

    private void DrawDistributionGraph()
    {
        float scoreAtMax = scoreAtMaxDifficulty.floatValue;
        int maxIdx       = maxDifficulty.enumValueIndex;
        float sig        = Mathf.Max(0.01f, sigma.floatValue);
        float minW       = minWeight.floatValue;
        float score      = debugScore.floatValue;

        float t    = Mathf.Clamp01(score / Mathf.Max(1f, scoreAtMax));
        float peak = t * maxIdx;

        // 各レベルの重みを計算
        float[] weights = new float[4];
        for (int i = 0; i < 4; i++)
        {
            float diff = (i - peak) / sig;
            weights[i] = Mathf.Max(minW, Mathf.Exp(-0.5f * diff * diff));
        }

        // グラフ描画エリア
        EditorGUILayout.LabelField("Weight Distribution", EditorStyles.boldLabel);
        Rect graphRect = GUILayoutUtility.GetRect(0, 120, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(graphRect, new Color(0.15f, 0.15f, 0.15f));

        float padding   = 8f;
        float barAreaW  = graphRect.width  - padding * 2f;
        float barAreaH  = graphRect.height - padding * 2f;
        float barW      = barAreaW / 4f;
        float maxBarH   = barAreaH * 0.75f;

        for (int i = 0; i < 4; i++)
        {
            float normalized = weights[i]; // gaussian max = 1.0
            float barH = normalized * maxBarH;

            float x = graphRect.x + padding + i * barW + barW * 0.1f;
            float y = graphRect.y + padding + (barAreaH - barH);
            float w = barW * 0.8f;

            // バー本体
            EditorGUI.DrawRect(new Rect(x, y, w, barH), LevelColors[i]);

            // ピーク三角マーカー
            if (Mathf.Abs(i - peak) < 0.5f)
            {
                float markerY = graphRect.y + padding - 4f;
                EditorGUI.DrawRect(new Rect(x + w * 0.5f - 3f, markerY, 6f, 6f), Color.white);
            }

            // ラベル（レベル名）
            var labelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = LevelColors[i] }
            };
            EditorGUI.LabelField(
                new Rect(x, graphRect.y + graphRect.height - padding - 14f, w, 14f),
                LevelNames[i], labelStyle);

            // 数値
            var valStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = Color.white }
            };
            EditorGUI.LabelField(
                new Rect(x, Mathf.Max(y - 14f, graphRect.y), w, 14f),
                weights[i].ToString("F2"), valStyle);
        }

        // ピーク位置ライン
        float peakX = graphRect.x + padding + peak * barW + barW * 0.5f;
        EditorGUI.DrawRect(new Rect(peakX - 0.5f, graphRect.y + padding, 1f, maxBarH), new Color(1f, 1f, 1f, 0.3f));

        // スコア表示
        var scoreStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.gray } };
        EditorGUI.LabelField(
            new Rect(graphRect.x + padding, graphRect.y + 2f, graphRect.width, 14f),
            $"Score: {(int)score}  Peak: {peak:F2}", scoreStyle);

        // 変化をリアルタイム反映
        if (GUI.changed) Repaint();
    }
}