using UnityEngine;

/// <summary>
/// プレス機の上部パーツを上下に動かすコンポーネント。
/// 上部パーツ（upperPart）の localPosition を制御する。
/// </summary>
public class PressAnimator : MonoBehaviour
{
    [Header("対象")]
    [Tooltip("上下に動かす上部パーツのTransform")]
    [SerializeField] private Transform upperPart;

    [Header("上下運動")]
    [Tooltip("最上点のローカルY座標")]
    [SerializeField] private float topY = 2f;
    [Tooltip("最下点のローカルY座標")]
    [SerializeField] private float bottomY = 0f;
    [Tooltip("最上点での待機時間（秒）")]
    [SerializeField] private float topWaitDuration = 0.5f;
    [Tooltip("下降にかかる時間（秒）")]
    [SerializeField] private float descendDuration = 0.3f;
    [Tooltip("最下点での待機時間（秒）")]
    [SerializeField] private float bottomWaitDuration = 0.2f;
    [Tooltip("上昇にかかる時間（秒）")]
    [SerializeField] private float ascendDuration = 0.5f;

    [Header("振動")]
    [Tooltip("プレス到達時に本体全体が振動する強さ")]
    [SerializeField] private float shakeStrength = 0.1f;
    [Tooltip("振動の持続時間（秒）")]
    [SerializeField] private float shakeDuration = 0.15f;
    [Tooltip("振動の周波数")]
    [SerializeField] private float shakeFrequency = 40f;

    private Blastable blastable;
    private Vector3 baseLocalPosition;
    private float elapsed = 0f;
    private float shakeElapsed = -1f;

    // フェーズ: 0=待機(上), 1=下降, 2=待機(下), 3=上昇
    private int phase = 0;

    private void Awake()
    {
        baseLocalPosition = transform.localPosition;
        blastable = GetComponent<Blastable>();
    }

    private void OnEnable()
    {
        if (blastable) blastable.OnBlasted += OnBlasted;
    }

    private void OnDisable()
    {
        if (blastable) blastable.OnBlasted -= OnBlasted;
    }

    private void OnBlasted()
    {
        enabled = false;
        transform.localPosition = baseLocalPosition;
    }

    private void Start()
    {
        shakeElapsed = -1f;
        transform.localPosition = baseLocalPosition;

        // フェーズと経過時間をランダムに初期化して複数インスタンスの同期を防ぐ
        phase = Random.Range(0, 4);
        float phaseDuration = phase == 0 ? topWaitDuration
                            : phase == 1 ? descendDuration
                            : phase == 2 ? bottomWaitDuration
                            : ascendDuration;
        elapsed = Random.Range(0f, phaseDuration);

        // upperPartの初期Y位置をelapsedに合わせて設定
        if (upperPart)
        {
            float y = phase == 0 ? topY
                    : phase == 1 ? Mathf.Lerp(topY, bottomY, Mathf.Pow(elapsed / descendDuration, 3f))
                    : phase == 2 ? bottomY
                    : Mathf.Lerp(bottomY, topY, 1f - Mathf.Pow(1f - elapsed / ascendDuration, 3f));
            SetUpperY(y);
        }
    }

    private void Update()
    {
        if (!upperPart) return;

        elapsed += Time.deltaTime;

        switch (phase)
        {
            case 0: // 最上点で待機
                SetUpperY(topY);
                if (elapsed >= topWaitDuration)
                {
                    elapsed = 0f;
                    phase = 1;
                }
                break;

            case 1: // 下降（EaseIn: 急加速）
                float descT = Mathf.Clamp01(elapsed / descendDuration);
                float descEased = descT * descT * descT; // EaseInCubic
                SetUpperY(Mathf.Lerp(topY, bottomY, descEased));

                if (elapsed >= descendDuration)
                {
                    SetUpperY(bottomY);
                    elapsed = 0f;
                    shakeElapsed = 0f; // 振動開始
                    phase = 2;
                }
                break;

            case 2: // 最下点で待機
                SetUpperY(bottomY);
                if (elapsed >= bottomWaitDuration)
                {
                    elapsed = 0f;
                    phase = 3;
                }
                break;

            case 3: // 上昇（EaseOut: 急減速）
                float ascT = Mathf.Clamp01(elapsed / ascendDuration);
                float ascEased = 1f - Mathf.Pow(1f - ascT, 3f); // EaseOutCubic
                SetUpperY(Mathf.Lerp(bottomY, topY, ascEased));

                if (elapsed >= ascendDuration)
                {
                    SetUpperY(topY);
                    elapsed = 0f;
                    phase = 0;
                }
                break;
        }

        // 振動
        if (shakeElapsed >= 0f)
        {
            shakeElapsed += Time.deltaTime;
            if (shakeElapsed < shakeDuration)
            {
                float decay = 1f - (shakeElapsed / shakeDuration);
                float offsetX = Mathf.Sin(shakeElapsed * shakeFrequency) * shakeStrength * decay;
                float offsetZ = Mathf.Cos(shakeElapsed * shakeFrequency * 0.7f) * shakeStrength * decay;
                transform.localPosition = baseLocalPosition + new Vector3(offsetX, 0f, offsetZ);
            }
            else
            {
                transform.localPosition = baseLocalPosition;
                shakeElapsed = -1f;
            }
        }
    }

    private void SetUpperY(float y)
    {
        var p = upperPart.localPosition;
        p.y = y;
        upperPart.localPosition = p;
    }
}
