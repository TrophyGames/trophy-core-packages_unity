using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;
using Trophy.Logging;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    private const int UPDATE_CHECK_FRAME_INTERVAL = 50;

    public Camera CurrentBaseCamera { get; private set; }
    public float CurrentAspectRatio { get; private set; }
    public float CurrentPixelCount { get; private set; }

    public Action<float> OnAspectRatioChanged;
    public Action OnScreenResolutionChanged;
    public Action<bool> OnOpaqueTexturesChanged;

    private List<Camera> allCameras = new List<Camera>();
    private List<Camera> allOverlayCameras = new List<Camera>();
    private Stack<Camera> baseCameraHistory = new Stack<Camera>();

    private float newAspectRatio;
    private int newPixelCount;
    private int checkIntervalCounter = 1;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (this != Instance)
            Destroy(this);
    }

    public IEnumerator Initialize()
    {
        CheckScreenSize();
        yield return true;
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    private void Update()
    {
        if (checkIntervalCounter % UPDATE_CHECK_FRAME_INTERVAL == 0)
        {
            CheckScreenSize();
        }
        else
        {
            checkIntervalCounter++;
        }
    }
#endif

    public void CheckScreenSize()
    {
        checkIntervalCounter = 1;

#if UNITY_EDITOR
        newAspectRatio = (float)Screen.width / (float)Screen.height;
        newPixelCount = Screen.width * Screen.height;
#else
        newAspectRatio = (float)Screen.currentResolution.width / Screen.currentResolution.height;
        newPixelCount = Screen.currentResolution.width * Screen.currentResolution.height;
#endif

        if (newAspectRatio != CurrentAspectRatio)
        {
            CurrentAspectRatio = newAspectRatio;
            OnAspectRatioChanged?.Invoke(newAspectRatio);
        }

        if (newPixelCount != CurrentPixelCount)
        {
            CurrentPixelCount = newPixelCount;
            OnScreenResolutionChanged?.Invoke();

            if (QualityManager.Instance != null && QualityManager.Instance.IsInitialized)
                QualityManager.Instance.renderTextureQualityManager.UpdateCurrentRenderTextures();
        }
    }

    public void AddCamera(Camera cam, bool isBase)
    {
        allCameras.Add(cam);

        if (isBase)
        {
            baseCameraHistory.Push(CurrentBaseCamera);
            CurrentBaseCamera = cam;
        }

        UpdateCameras();
        UpdateCameraAntialiasing();
    }

    public void RemoveCamera(Camera cam)
    {
        allCameras.Remove(cam);

        if (cam == CurrentBaseCamera)
        {
            UniversalAdditionalCameraData camData = CurrentBaseCamera.GetUniversalAdditionalCameraData();
            if (camData != null)
                camData.cameraStack.Clear();

            CurrentBaseCamera = null;

            while (CurrentBaseCamera == null && baseCameraHistory.Count > 0)
            {
                Camera historyCam = baseCameraHistory.Pop();
                if (historyCam != null)
                {
                    CurrentBaseCamera = historyCam;
                    break;
                }
            }
        }

        UpdateCameras();
        UpdateCameraAntialiasing();
    }

    private void UpdateCameras()
    {
        allOverlayCameras.Clear();

        UniversalAdditionalCameraData baseCameraData = null;
        foreach (Camera camera in allCameras)
        {
            UniversalAdditionalCameraData camData = camera.GetUniversalAdditionalCameraData();
            if (camera == CurrentBaseCamera)
            {
                baseCameraData = camData;
                if (baseCameraData != null)
                    baseCameraData.renderType = CameraRenderType.Base;
            }
            else
            {
                camData.renderType = CameraRenderType.Overlay;
                allOverlayCameras.Add(camera);
            }
        }

        if (allOverlayCameras.Count > 0)
        {
            if (baseCameraData != null)
                baseCameraData.cameraStack.AddRange(allOverlayCameras);
        }
    }

    public void UpdateCameraAntialiasing()
    {
        QualityConfig config = QualityManager.Instance.CurrentConfig;

        if (!config.enabledAntialiasing)
        {
            LogUtility.Log("Config has not enabled AA", LogCondition.Full);
            return;
        }

        if (!QualityManager.Instance.antialiasingEnabled)
            return;

        AntialiasingSettingsQuality aaSetting = config.GetAntialiasingQualityForQualityTier(QualityManager.Instance.CurrentQualityTier);
        if (aaSetting == null)
            return;

        // Update cameras.
        for (int i = 0; i < allCameras.Count; i++)
        {
            Camera cam = allCameras[i];
            if (cam == null)
                continue;

            UniversalAdditionalCameraData universalcamData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (universalcamData == null)
            {
                LogUtility.Log("Unable to find universal camera data: " + cam.name);
                continue;
            }
            universalcamData.antialiasing = aaSetting.aaMode;
            universalcamData.antialiasingQuality = aaSetting.aaQuality;
        }
    }
}
