using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[System.Serializable]
public class AntialiasingSettingsQuality
{
    public QualityManager.QualityTier qualityTier;
    public UnityEngine.Rendering.Universal.AntialiasingMode aaMode;
    public UnityEngine.Rendering.Universal.AntialiasingQuality aaQuality;
}

[CreateAssetMenu(menuName = "Configs/Quality Config", fileName = "QualityConfig")]
public class QualityConfig : ScriptableObject
{
    [Header("General")]
    public bool enableDynamicQualityManagement;
    public int maxFramerate;

    [Header("Tiers")]
    public QualityManager.QualityTier minQualityTier;
    public QualityManager.QualityTier defaultQualityTier;
    public QualityManager.QualityTier maxQualityTier;

    [Header("Render Scale Settings (0 = off)")]
    public int maxDpi;

    [Header("Render Scales")]
    public List<float> minRenderScales;

    [Header("Render Textures")]
    public bool useRenderScaleForRT;
    public GraphicsFormat renderTextureFormat = GraphicsFormat.R16G16B16A16_SFloat;
    public RenderTextureFormat renderTextureFormatFallback = RenderTextureFormat.ARGB32;

    [Header("Audio Mono/Stereo")]
    public List<bool> forceMonoAudioTiers;

    [Header("Beauty Settings")]
    public bool enabledAntialiasing = false;
    public bool enabledVSync = false;
    public AntialiasingSettingsQuality[] aaSettings;

    public float GetMinRenderScale(QualityManager.QualityTier tier)
    {
        return minRenderScales[(int)tier];
    }

    public AntialiasingSettingsQuality GetAntialiasingQualityForQualityTier(QualityManager.QualityTier tier)
    {
        AntialiasingSettingsQuality setting = null;
        for (int i = aaSettings.Length - 1; i >= 0; i--)
        {
            setting = aaSettings[i];
            if (setting.qualityTier == tier)
                break;
            else
                setting = null;
        }
        return setting;
    }

}
