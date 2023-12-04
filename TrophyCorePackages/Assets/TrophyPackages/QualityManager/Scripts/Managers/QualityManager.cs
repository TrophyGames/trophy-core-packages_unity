using System;
using System.Collections;
using System.Collections.Generic;
using Trophy.Logging;
using UnityEngine;

public class QualityManager : MonoBehaviour
{
    public enum QualityTier
    {
        LOWEST = 0,
        LOW = 1,
        NORMAL = 2,
        HIGH = 3,
        ULTRA = 4,
        AUTOMATIC = 5,
    }

    public const string PP_QUALITY_TIER = "CurrentQualityTier";
    public const string PP_QUALITY_AUTOMATIC = "CurrentQualityChangeAutomatic";
    public const string PP_RENDER_SCALE_STEP = "CurrentRenderScaleStep";
    public const string PP_DEFAULT_SCREEN_MODE = "DefaultScreenMode";
    public const string PP_DEFAULT_SCREEN_RESOLUTION = "CurrentScreenResolution";
    public const string PP_SHAKE_AND_DISTORTIONS_ENABLE = "ScreenShakeEnabled";
    public const string PP_DEPTH_OF_FIELD = "ScreenDepthOfField";
    public const string PP_ANTI_ALIASING_ENABLED = "AAEnabled";
    public const string PP_VSYNC_STATE = "VSyncTurnedOn";

    private const int FPS_TARGET_TOLERANCE = 5;
    private const int FPS_MAX_OFFSET = 3;
    private const int FPS_SAMPLES_CAPACITY = 300;
    private const int CHECKS_AT_MAX_FPS_REQUIRED_FOR_QUALITY_INCREASE = 3;
    private const int DEFAULT_PREFS_VALUE = -1;
    private const float QUALITY_CHECK_COOLDOWN = 5f;
    public const QualityTier MIN_QUALITY_TIER = QualityTier.LOWEST;
    public const QualityTier MAX_QUALITY_TIER = QualityTier.ULTRA;

    public static QualityManager Instance { get; private set; }
    public static Action<QualityTier> OnQualityTierChanged;
    public bool IsInitialized { get; private set; }
    public RuntimePlatform CurrentPlatform { get; private set; }
    public QualityConfig CurrentConfig { get; private set; }

    public QualityTier CurrentQualityTier = QualityTier.NORMAL;

    // FPS checking.
    public int AverageFPS { get; private set; }
    public bool DynamicQualityManagementActive => dynamicQualityManagementActive;
    private int currentFrameTimeToFPS;
    private int indexSample;
    private int fpsSamplesCount;
    private int[] fpsSamples;
    private bool dynamicQualityManagementActive;
    private bool qualityCheckIsReady;
    private uint averageAddedFps;
    private int checksAtMaxFps;
    private float qualityCheckTimer;

    [HideInInspector] public bool antialiasingEnabled = false;

    [Header("Default Configs")]
    public QualityConfig defaultConfigPC;
    public QualityConfig defaultConfigAndroid;
    public QualityConfig defaultConfigIOS;

    [Header("Sub Quality Managers")]
    public TargetFramerateQualityManager targetFramerateQualityManager;
    public TierQualityManager tierQualityManager;
    public AudioQualityManager audioQualityManager;
    public RenderScaleQualityManager renderScaleQualityManager;
    public RenderTextureQualityManager renderTextureQualityManager;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            fpsSamples = new int[FPS_SAMPLES_CAPACITY];
        }
        else if (this != Instance)
        {
            Destroy(this);
        }
    }
    
    // -------------------------------------------
    // Initialize methods.
    // -------------------------------------------
    public IEnumerator Initialize(RuntimePlatform platform)
    {
        CurrentPlatform = platform;
        CurrentConfig = GetPlatformDefaultConfig(platform);
        CurrentQualityTier = GetQualityTier(CurrentConfig);

        InitializeSubManagers();
        InitializeScreenSettings();
        InitializeBeautySettings();

        ResetFrameSamples();
        IsInitialized = true;
        CameraManager.Instance.CheckScreenSize();

        if (CurrentPlatform != RuntimePlatform.WindowsPlayer)
            SetDynamicQualityManagementActiveLocal();

        yield return true;
    }

    private void InitializeSubManagers()
    {
        targetFramerateQualityManager.Initialize(CurrentConfig.maxFramerate);
        tierQualityManager.Initialize(CurrentQualityTier);
        audioQualityManager.Initialize(CurrentQualityTier);
        renderScaleQualityManager.Initialize(CurrentQualityTier);
        renderTextureQualityManager.Initialize();
    }

    private void InitializeScreenSettings()
    {
        int screenMode = PlayerPrefs.GetInt(PP_DEFAULT_SCREEN_MODE, 0);
        Screen.fullScreenMode = (FullScreenMode)screenMode;
        Screen.fullScreen = Screen.fullScreenMode != FullScreenMode.Windowed;

        // Try to read default resolution, if it exist append it if not use default.
        string resolutionStr = PlayerPrefs.GetString(PP_DEFAULT_SCREEN_RESOLUTION, null);
        Resolution currentRes = Screen.currentResolution;
        if (!string.IsNullOrEmpty(resolutionStr))
        {
            string[] resolutionParts = resolutionStr.Split(',');
            currentRes = new Resolution();
            currentRes.width = int.Parse(resolutionParts[0]);
            currentRes.height = int.Parse(resolutionParts[1]);
            currentRes.refreshRate = int.Parse(resolutionParts[2]);
        }

        Screen.SetResolution(currentRes.width, currentRes.height, Screen.fullScreenMode, currentRes.refreshRate);
    }

    private void InitializeBeautySettings()
    {
        // Apply Antialising.
        if (CurrentConfig.enabledAntialiasing)
        {
            if (PlayerPrefs.HasKey(PP_ANTI_ALIASING_ENABLED))
                antialiasingEnabled = PlayerPrefs.GetInt(PP_ANTI_ALIASING_ENABLED, DEFAULT_PREFS_VALUE) == 1 ? true : default;
            else
                antialiasingEnabled = CurrentConfig.enabledAntialiasing;
        }

        // Apply VSync.
        if (CurrentConfig.enabledVSync)
        {
            if (PlayerPrefs.HasKey(PP_VSYNC_STATE))
                QualitySettings.vSyncCount = PlayerPrefs.GetInt(PP_VSYNC_STATE, DEFAULT_PREFS_VALUE);
            else
                QualitySettings.vSyncCount = CurrentConfig.enabledVSync ? 1 : default;
        }
    }

    // -------------------------------------------
    // Update.
    // -------------------------------------------
    private void Update()
    {
        if (!IsInitialized || !dynamicQualityManagementActive || !AllowAutomaticQualityChange())
            return;

        UpdateAverageFPS();
        UpdateQualityTierOrRenderScale();
    }

    private void UpdateAverageFPS()
    {
        currentFrameTimeToFPS = (Mathf.RoundToInt(1f / Time.unscaledDeltaTime));
        indexSample++;

        if (indexSample >= FPS_SAMPLES_CAPACITY)
            indexSample = 0;

        fpsSamples[indexSample] = currentFrameTimeToFPS;

        if (fpsSamplesCount < FPS_SAMPLES_CAPACITY)
            fpsSamplesCount++;

        averageAddedFps = 0;
        for (int i = 0; i < fpsSamplesCount; i++)
        {
            if (fpsSamples[i] == 0)
                continue;
            averageAddedFps += (uint)fpsSamples[i];
        }

        AverageFPS = (short)((float)averageAddedFps / (float)fpsSamplesCount);
    }

    private void UpdateQualityTierOrRenderScale()
    {
        if (qualityCheckIsReady)
        {
            qualityCheckIsReady = default;
            ResetFrameSamples();

            // Check for quality decrease.
            if (AverageFPS < CurrentConfig.maxFramerate - FPS_TARGET_TOLERANCE)
            {
                if (!renderScaleQualityManager.IsAtMinimumScale)
                {
                    renderScaleQualityManager.ReduceRenderScale();
                }
                else
                {
                    if (!IsAtMinPlatformQualityTier(CurrentConfig))
                        DecreaseQualityTier();
                }
            }
            else if (AverageFPS >= CurrentConfig.maxFramerate - FPS_MAX_OFFSET)
            {
                checksAtMaxFps++;
            }

            // Check for quality increase.
            if (checksAtMaxFps >= CHECKS_AT_MAX_FPS_REQUIRED_FOR_QUALITY_INCREASE)
            {
                checksAtMaxFps = default;

                if (renderScaleQualityManager.IsAtMaximumScale)
                {
                    if (!IsAtMaxPlatformQualityTier(CurrentConfig))
                        IncreaseQualityTier();
                }
                else
                {
                    renderScaleQualityManager.IncreaseRenderScale();
                }
            }
        }
        else
        {
            qualityCheckTimer += Time.deltaTime;
            if (qualityCheckTimer >= QUALITY_CHECK_COOLDOWN)
            {
                qualityCheckTimer = default;
                qualityCheckIsReady = true;
            }
        }
    }

    // -------------------------------------------
    // Platform methods.
    // -------------------------------------------
    private QualityConfig GetPlatformDefaultConfig(RuntimePlatform platform)
    {
#if UNITY_EDITOR
        // On editor depending on which machine this is run it will always report.
        // WindowsEditor, LinuxEditor, OSXEditor.
        platform = RunPlatform();
#endif
        switch (platform)
        {
            case RuntimePlatform.IPhonePlayer:
                return defaultConfigIOS;

            case RuntimePlatform.Android:
                return defaultConfigAndroid;

            default:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.LinuxEditor:
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.LinuxPlayer:
                return defaultConfigPC;
        }
    }

    private RuntimePlatform RunPlatform()
    {
        RuntimePlatform current = Application.platform;
#if UNITY_EDITOR
        // On editor depending on which machine this is run it will always report.
        // WindowsEditor, LinuxEditor, OSXEditor.
        UnityEditor.BuildTarget currentTarget = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
        if (currentTarget == UnityEditor.BuildTarget.Android)
        {
            current = RuntimePlatform.Android;
        }
        else if (currentTarget == UnityEditor.BuildTarget.iOS)
        {
            current = RuntimePlatform.IPhonePlayer;
        }
#endif
        return current;
    }

    // -------------------------------------------
    // Quality tier methods.
    // -------------------------------------------
    public void DynamicallyChangeQualityTier(QualityTier tier)
    {
        LogUtility.Log("Dynamically changed quality to: " + tier, LogCondition.Full);

        SetSubManagersQualityTier();
        OnQualityTierChanged?.Invoke(tier);
        CameraManager.Instance.UpdateCameraAntialiasing();
    }

    public void ManuallyChangeQualityTier(QualityTier tier)
    {
        LogUtility.Log("Manually changed quality to: " + tier, LogCondition.Full);

        if (CurrentQualityTier != tier)
            CurrentQualityTier = tier;

        SetSubManagersQualityTier();
        OnQualityTierChanged?.Invoke(tier);
        CameraManager.Instance.UpdateCameraAntialiasing();

        PlayerPrefs.SetInt(PP_QUALITY_TIER, (int)tier);
    }

    private void DecreaseQualityTier()
    {
        if ((int)CurrentQualityTier <= (int)MIN_QUALITY_TIER)
            return;

        CurrentQualityTier--;
        DynamicallyChangeQualityTier(CurrentQualityTier);
    }

    private void IncreaseQualityTier()
    {
        if ((int)CurrentQualityTier >= (int)MAX_QUALITY_TIER)
            return;

        CurrentQualityTier++;
        DynamicallyChangeQualityTier(CurrentQualityTier);
    }

    private QualityTier GetQualityTier(QualityConfig config)
    {
        // Get default tier.
        QualityTier defaultTier = GetDefaultQualityTier(config);

        // Set tier.
        QualityTier tier = (QualityTier)PlayerPrefs.GetInt(PP_QUALITY_TIER, (int)defaultTier);
        if (tier < config.minQualityTier)
            tier = config.minQualityTier;
        else if (tier > config.maxQualityTier)
            tier = config.maxQualityTier;

        return tier;
    }

    private QualityTier GetDefaultQualityTier(QualityConfig config)
    {
        QualityTier defaultTier = config.defaultQualityTier;

        // Switch on platform.
        switch (RunPlatform())
        {
            // IOS.
            case RuntimePlatform.IPhonePlayer:
                // TODO change this to check on device number maybe (fx: IPhone 11).
                defaultTier = config.defaultQualityTier;
#if UNITY_IOS
                UnityEngine.iOS.Device.hideHomeButton = true;
#endif
                break;

            // Android.
            case RuntimePlatform.Android:
                // Get default quality from device free RAM ("free" being the amount of RAM apps can use).      
                long roundedPhysicalMemory = TrophyNative.Instance.RoundedPhysicalMemory(TrophyNative.MemoryFormat.MB);
                if (roundedPhysicalMemory <= 0)
                {
                    // If 0 or less, then device is not compatible with systemMemorySize, use default.
                    defaultTier = config.defaultQualityTier;
                }
                else if (roundedPhysicalMemory <= 2560)
                {
                    defaultTier = QualityTier.LOWEST;
                }
                else if (roundedPhysicalMemory <= 3072)
                {
                    defaultTier = QualityTier.LOW;
                }
                else if (roundedPhysicalMemory <= 6144)
                {
                    defaultTier = QualityTier.NORMAL;
                }
                else
                {
                    defaultTier = QualityTier.HIGH;
                }
                break;

            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.LinuxEditor:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.LinuxPlayer:
                defaultTier = QualityTier.AUTOMATIC;
                break;

            default:
                defaultTier = config.defaultQualityTier;
                break;
        }


        return defaultTier;
    }

    // -------------------------------------------
    // Set dynamic quality management.
    // -------------------------------------------
    public void SetDynamicQualityManagementActiveLocal()
    {
        SetDynamicQualityManagementActive();
    }
    
    public void SetDynamicQualityManagementActive()
    {
        if (!CurrentConfig.enableDynamicQualityManagement)
            return;

        dynamicQualityManagementActive = true;

        renderScaleQualityManager.OnQualityManagementBegin();
        renderTextureQualityManager.OnQualityManagementBegin();
        audioQualityManager.OnQualityManagementBegin();
        tierQualityManager.OnQualityManagementBegin();
    }
    public void SetDynamicQualityManagementInactive()
    {
        dynamicQualityManagementActive = false;

        renderScaleQualityManager.OnQualityManagementEnd();
        renderTextureQualityManager.OnQualityManagementEnd();
        audioQualityManager.OnQualityManagementEnd();
        tierQualityManager.OnQualityManagementEnd();
    }

    // -------------------------------------------
    // Utility.
    // -------------------------------------------
    private void ResetFrameSamples()
    {
        if (fpsSamples != null)
            fpsSamples = new int[FPS_SAMPLES_CAPACITY];

        indexSample = 0;
        fpsSamplesCount = 0;
    }

    public static RuntimePlatform BuildToRuntimePlatform(UnityEditor.BuildTarget buildTarget)
    {
#if UNITY_EDITOR
        switch (buildTarget)
        {
            case UnityEditor.BuildTarget.Android:
                return RuntimePlatform.Android;
            case UnityEditor.BuildTarget.iOS:
                return RuntimePlatform.IPhonePlayer;
            case UnityEditor.BuildTarget.StandaloneWindows64:
            case UnityEditor.BuildTarget.StandaloneWindows:
                return RuntimePlatform.WindowsPlayer;
            default:
                Debug.LogError("BuildTarget [" + buildTarget + "] is currently not mapped to a runtimePlatform.");
                return RuntimePlatform.WindowsPlayer;
        }
#else
        return Application.platform;
#endif

    }

    private bool AllowAutomaticQualityChange()
    {
        bool hasPermission = true;
#if UNITY_STANDALONE
        hasPermission = PlayerPrefs.GetInt(PP_QUALITY_AUTOMATIC, default) != default;
#endif
        return hasPermission;
    }

    private void SetSubManagersQualityTier()
    {
        tierQualityManager.OnQualityTierChanged(CurrentQualityTier);
        audioQualityManager.OnQualityTierChanged(CurrentQualityTier);
        renderScaleQualityManager.OnQualityTierChanged(CurrentQualityTier);
        renderTextureQualityManager.OnQualityTierChanged(CurrentQualityTier);
    }

    private bool IsAtMinPlatformQualityTier(QualityConfig config) => CurrentQualityTier == config.minQualityTier;
    private bool IsAtMaxPlatformQualityTier(QualityConfig config) => CurrentQualityTier == config.maxQualityTier;
}
