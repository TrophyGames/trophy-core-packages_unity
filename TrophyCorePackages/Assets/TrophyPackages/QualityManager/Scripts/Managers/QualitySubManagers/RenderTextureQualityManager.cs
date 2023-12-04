using System.Collections;
using System.Collections.Generic;
using Trophy.Logging;
using UnityEngine;
using UnityEngine.Rendering.Universal;


public class RenderTextureQualityManager : QualitySubManager
{
    public QualityConfig CurrentConfig { get; private set; }
    public class RenderTextureRequestInfo
    {
        public Camera Camera { get; private set; }
        public UnityEngine.UI.RawImage RawImageTarget { get; private set; }

        public RenderTexture AssignedRenderTexture { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public enum RenderScalingMode
        {
            USE_DEFAULT,
            ON,
            OFF,
        }
        public RenderScalingMode RenderScaling { get; set; } = RenderScalingMode.USE_DEFAULT;

        public RenderTextureRequestInfo(Camera camera, UnityEngine.UI.RawImage rawImageTarget)
        {
            Camera = camera;
            RawImageTarget = rawImageTarget;
        }
    }
    public List<RenderTextureRequestInfo> CurrentRenderTextureRequestInfos { get; private set; } = new List<RenderTextureRequestInfo>();


    public void Initialize()
    {
        CurrentConfig = QualityManager.Instance.CurrentConfig;
    }

    public override void OnQualityTierChanged(QualityManager.QualityTier newQualityTier)
    {
        if (CurrentConfig.useRenderScaleForRT)
            UpdateCurrentRenderTextures();
    }


    public void Assign(RenderTextureRequestInfo info)
    {
        if (info == null)
        {
            LogUtility.Log("QualityManager ASSIGN: Info is null.", LogCondition.Full);
            return;
        }
        else if (info.RawImageTarget == null)
        {
            LogUtility.Log("QualityManager ASSIGN: Target is null. Releasing...", LogCondition.Full);
            info.AssignedRenderTexture.Release();
            return;
        }

        RectTransform rect = info.RawImageTarget.rectTransform;

        if (AspectIsPortrait)
        {
            float aspect = rect.sizeDelta.x / rect.sizeDelta.y;
            info.Width = Mathf.FloorToInt(Screen.currentResolution.width * (rect.sizeDelta.x / ReferenceResolution.x));
            info.Height = Mathf.FloorToInt(info.Width / aspect);
        }
        else
        {
            float aspect = rect.sizeDelta.y / rect.sizeDelta.x;
            info.Height = Mathf.FloorToInt(Screen.currentResolution.height * (rect.sizeDelta.y / ReferenceResolution.y));
            info.Width = Mathf.FloorToInt(info.Height / aspect);
        }

        // Apply renderScaling if relevant.
        if (CurrentConfig != null && CurrentConfig.useRenderScaleForRT && info.RenderScaling != RenderTextureRequestInfo.RenderScalingMode.OFF)
        {
            info.Width = Mathf.FloorToInt(info.Width * UniversalRenderPipeline.asset.renderScale);
            info.Height = Mathf.FloorToInt(info.Height * UniversalRenderPipeline.asset.renderScale);
        }

        if (info.AssignedRenderTexture != null)
        {
            if (info.AssignedRenderTexture.width != info.Width || info.AssignedRenderTexture.height != info.Height)
            {
                info.AssignedRenderTexture.Release();
                Create(info);
            }
        }
        else
        {
            if (CurrentRenderTextureRequestInfos != null)
            {
                Create(info);
                CurrentRenderTextureRequestInfos.Add(info);
            }
        }
    }

    private void Create(RenderTextureRequestInfo info)
    {
        RenderTexture rt;

        if (SystemInfo.IsFormatSupported(CurrentConfig.renderTextureFormat, UnityEngine.Experimental.Rendering.FormatUsage.Render))
            rt = new RenderTexture(info.Width, info.Height, 16, CurrentConfig.renderTextureFormat);
        else
            rt = new RenderTexture(info.Width, info.Height, 16, CurrentConfig.renderTextureFormatFallback);

        if (QualityManager.Instance.CurrentConfig.enabledAntialiasing)
        {
            rt.antiAliasing = 2;
            info.Camera.allowMSAA = true;
            UniversalAdditionalCameraData camData = info.Camera.GetUniversalAdditionalCameraData();
            if (camData != null)
            {
                camData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                camData.antialiasingQuality = AntialiasingQuality.High;
            }
        }
        info.AssignedRenderTexture = rt;
        info.Camera.targetTexture = rt;
        info.RawImageTarget.texture = rt;
    }

    public void UpdateCurrentRenderTextures()
    {
        foreach (RenderTextureRequestInfo info in CurrentRenderTextureRequestInfos)
        {
            Assign(info);
        }
    }

    public void Release(RenderTextureRequestInfo info)
    {
        if (CurrentRenderTextureRequestInfos.Contains(info))
        {
            CurrentRenderTextureRequestInfos.Remove(info);
            info.AssignedRenderTexture.Release();
        }
    }

    public bool AspectIsPortrait => (float)Screen.width / Screen.height < 1.0f;

    // TODO: Set this to the main canvas scaler referenceResolution.
    public Vector2 ReferenceResolution => new Vector2(1000f, 1000f);
}
