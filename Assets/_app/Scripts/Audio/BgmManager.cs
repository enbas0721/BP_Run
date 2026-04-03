using CriWare;
using UnityEngine;

public class BgmManager : MonoBehaviour
{
    [SerializeField] private CriAtomSource bgmSource;

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        if (bgmSource == null) return;

        switch (state)
        {
            case GameState.Playing:
                if (bgmSource.IsPaused())
                    bgmSource.Pause(false);
                else
                    bgmSource.Play();
                break;

            case GameState.Paused:
                bgmSource.Pause(true);
                break;

            case GameState.GameOver:
            case GameState.Ready:
                bgmSource.Stop();
                break;
        }
    }
}
