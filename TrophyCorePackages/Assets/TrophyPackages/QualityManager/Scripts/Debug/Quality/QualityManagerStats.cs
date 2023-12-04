using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class QualityManagerStats : MonoBehaviour
{
    public TextMeshProUGUI qualityManagerStatsText;

    void Update()
    {
        if (QualityManager.Instance != null && QualityManager.Instance.IsInitialized)
        {
            qualityManagerStatsText.text = "Dynamic: " + QualityManager.Instance.DynamicQualityManagementActive + 
                " | Quality tier: " + QualityManager.Instance.CurrentQualityTier + 
                " | RenderScale: " + UniversalRenderPipeline.asset.renderScale + " Step: " + QualityManager.Instance.renderScaleQualityManager.CurrentRenderScaleStep + 
                " | FPS: " + QualityManager.Instance.AverageFPS + 
                " | MinMaxScale: " + QualityManager.Instance.renderScaleQualityManager.MinRenderScale + ", " + QualityManager.Instance.renderScaleQualityManager.MaxRenderScale;
        }
    }
}
