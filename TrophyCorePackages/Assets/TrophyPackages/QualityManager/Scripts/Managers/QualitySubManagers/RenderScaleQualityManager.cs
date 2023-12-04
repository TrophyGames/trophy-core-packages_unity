using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;


public class RenderScaleQualityManager : QualitySubManager
{
    private const int SCALE_STEPS = 3;
    public int CurrentRenderScaleStep { get; private set; }
    public bool IsAtMinimumScale { get { return CurrentRenderScaleStep == SCALE_STEPS; } }
    public bool IsAtMaximumScale { get { return CurrentRenderScaleStep == default; } }

    public float MinRenderScale { get; private set; }
    public float MaxRenderScale { get; private set; }

    public override void Initialize(QualityManager.QualityTier tier)
    {
        MaxRenderScale = GetMaxRenderScale();
        MinRenderScale = GetMinRenderScale(tier, MaxRenderScale);

        if (PlayerPrefs.HasKey(QualityManager.PP_RENDER_SCALE_STEP))
        {
            CurrentRenderScaleStep = PlayerPrefs.GetInt(QualityManager.PP_RENDER_SCALE_STEP, 0);
        }
        else
        {
            if (tier == QualityManager.QualityTier.LOWEST)
                CurrentRenderScaleStep = SCALE_STEPS;
        }

        SetRenderScaleLevel();
    }

    public override void OnQualityTierChanged(QualityManager.QualityTier newQualityTier)
    {
        MinRenderScale = GetMinRenderScale(newQualityTier, MaxRenderScale);
        SetRenderScaleLevel();
    }

    public override void OnQualityManagementEnd()
    {
        PlayerPrefs.SetInt(QualityManager.PP_RENDER_SCALE_STEP, CurrentRenderScaleStep);
    }

    private void SetRenderScaleLevel()
    {
        float stepSize = (MaxRenderScale - MinRenderScale) / (float)SCALE_STEPS;
        float currentScale = MaxRenderScale - (stepSize * CurrentRenderScaleStep);
        UniversalRenderPipeline.asset.renderScale = currentScale;
    }

    public void IncreaseRenderScale()
    {
        CurrentRenderScaleStep--;

        if (CurrentRenderScaleStep < (int)default)
            CurrentRenderScaleStep = default;

        SetRenderScaleLevel();
    }

    public void ReduceRenderScale()
    {
        CurrentRenderScaleStep++;

        if (CurrentRenderScaleStep > SCALE_STEPS)
            CurrentRenderScaleStep = SCALE_STEPS;

        SetRenderScaleLevel();
    }

    // -------------------------------------
    // Utility.
    // -------------------------------------
    public float GetCurrentRenderScale() => UniversalRenderPipeline.asset.renderScale;

    private float GetMinRenderScale(QualityManager.QualityTier tier, float maxRenderScale)
    {
        return maxRenderScale * QualityManager.Instance.CurrentConfig.GetMinRenderScale(tier);
    }

    private float GetMaxRenderScale()
    {
        int dpi = GetDPI();

        if (dpi == default)
            return 1;
        else
        {
            if (dpi > QualityManager.Instance.CurrentConfig.maxDpi)
                return (float)QualityManager.Instance.CurrentConfig.maxDpi / (float)dpi;
            else
                return 1;
        }
    }

    private int GetDPI()
    {
#if UNITY_STANDALONE && !UNITY_EDITOR
        return default;
#elif UNITY_ANDROID
        return DisplayMetricsAndroid.DensityDPI;
#elif UNITY_IOS
        return (int)Screen.dpi;
#else
        return default;
#endif
    }
}
