using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenderTextureQualityTest : MonoBehaviour
{
    [Header("UI")]
    public RawImage rawImageRenderTexture;

    [Header("3D Setup")]
    public Camera captureModelCamera;
    private RenderTextureQualityManager.RenderTextureRequestInfo rtRequestInfo;

    private void Start()
    {
        StartCoroutine(Setup());
    }

    public IEnumerator Setup()
    {
        while (!QualityManager.Instance.IsInitialized)
        {
            yield return new WaitForEndOfFrame();
        }

        rtRequestInfo = new RenderTextureQualityManager.RenderTextureRequestInfo(captureModelCamera, rawImageRenderTexture);
        QualityManager.Instance.renderTextureQualityManager.Assign(rtRequestInfo);
        captureModelCamera.enabled = true;

        yield return new WaitForEndOfFrame();
        rawImageRenderTexture.enabled = true;
    }

    public void OnDestroy()
    {
        QualityManager.Instance.renderTextureQualityManager?.Release(rtRequestInfo);
    }
}
