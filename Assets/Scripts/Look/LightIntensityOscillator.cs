using UnityEngine;

public class LightIntensityOscillator : MonoBehaviour
{
    [Header("明るさの設定")]
    [Tooltip("最小の明るさ")]
    public float minIntensity = 0.5f;
    [Tooltip("最大の明るさ")]
    public float maxIntensity = 2.0f;

    [Header("スピード設定")]
    [Tooltip("往復にかかる速度")]
    public float speed = 2.0f;

    private Light targetLight;

    void Start()
    {
        // Lightコンポーネントを取得
        targetLight = GetComponent<Light>();

        if (targetLight == null)
        {
            Debug.LogError("Lightコンポーネントが見つかりません。");
        }
    }

    void Update()
    {
        if (targetLight != null)
        {
            // Mathf.PingPong(経過時間 * 速度, 変化の幅)
            // 0から(max - min)の間を往復する値を作る
            float lerp = Mathf.PingPong(Time.time * speed, maxIntensity - minIntensity);
            
            // 最小値に足し合わせることで、minIntensity 〜 maxIntensityの間を動かす
            targetLight.intensity = minIntensity + lerp;
        }
    }
}