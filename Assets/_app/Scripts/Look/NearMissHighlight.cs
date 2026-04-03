using UnityEngine;

/// <summary>
/// アドレナリンゲージが初めて満タンになるまでNearMissZoneをパーティクルで強調表示する。
/// NearMissZoneと同じGameObject、もしくはその親にアタッチする。
/// </summary>
public class NearMissHighlight : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnFirstGaugeFull += Deactivate;

        if (particles)
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        bool alreadyFilled = GameManager.Instance != null && GameManager.Instance.IsFirstGaugeFilled;
        if (!alreadyFilled && particles)
            particles.Play();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnFirstGaugeFull -= Deactivate;

        if (particles)
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Deactivate()
    {
        if (particles)
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
