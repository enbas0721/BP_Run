using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class ShareButton : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void JS_Share(byte[] data, int length);
#endif

    [SerializeField] private Button button;

    private void Awake()
    {
        if (button) button.onClick.AddListener(OnShareClicked);
    }

    private void OnShareClicked()
    {
        StartCoroutine(ShareCoroutine());
    }

    private IEnumerator ShareCoroutine()
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        byte[] png = screenshot.EncodeToPNG();
        Destroy(screenshot);

#if UNITY_WEBGL && !UNITY_EDITOR
        JS_Share(png, png.Length);
#else
        Debug.Log("[ShareButton] WebGL以外では動作しません");
#endif
    }
}