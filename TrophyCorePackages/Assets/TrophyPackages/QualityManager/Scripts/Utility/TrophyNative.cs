using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrophyNative
{
    private const string PLUGIN_NAME = "com.trophygames.nativeplugin_systeminfo.SystemInfoInstance";

    public enum MemoryFormat
    {
        B,
        KB,
        MB,
        GB,
        TB
    }

    private static TrophyNative instance = null;
    public static TrophyNative Instance
    {
        get
        {

            if (instance == null)
            {
                instance = new TrophyNative();
                instance.Init(PLUGIN_NAME);
            }
            return instance;
        }

        private set
        {
            instance = value;
        }
    }

    AndroidJavaClass unityClass;
    AndroidJavaObject unityActivity;
    AndroidJavaObject _systemInfoInstance;

    void Init(string pluginName)
    {
#if !UNITY_EDITOR
        unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
        _systemInfoInstance = new AndroidJavaObject(pluginName);
        if (_systemInfoInstance == null)
        {
            Util.Log("Native System Info Plugin Error: Failed to Initialize");
        }
        _systemInfoInstance.CallStatic("receiveUnityActivity", unityActivity);
#endif
    }

    public long PhysicalMemory()
    {
#if !UNITY_EDITOR
        if (_systemInfoInstance != null)
        {
            var result = _systemInfoInstance.Call<long>("PhysicalMemory");
            return result;
        }
        else
        {
            Util.Log("Native System Info Plugin Error: Not Initialized");
            return 0;
        }
#else
        return 7516192768;
#endif
    }

    public long PhysicalMemory(MemoryFormat format)
    {
        var raw = PhysicalMemory();
        switch (format)
        {
            case MemoryFormat.KB:
                return (long)(raw / 1024f);
            case MemoryFormat.MB:
                return (long)(raw / 1048576f);
            case MemoryFormat.GB:
                return (long)(raw / 1073741824f);
            case MemoryFormat.TB:
                return (long)(raw / 1099511627776f);
            case MemoryFormat.B:
            default:
                return raw;
        }
    }

    public long RoundedPhysicalMemory(MemoryFormat format)
    {
        var formated = PhysicalMemory(format);
        return (long)Mathf.Round(formated);
    }

}
