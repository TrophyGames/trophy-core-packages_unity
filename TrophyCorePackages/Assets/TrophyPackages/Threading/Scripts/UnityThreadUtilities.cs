using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using Trophy.LogManagement;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
using UnityEditor;
#endif

using Logger = Trophy.LogManagement.Logger;

namespace TrophyGames.Threading
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class UnityThreadUtilities : LogableSingleton<UnityThreadUtilities>
    {
        /// <summary>
        /// For any non-MonoBehaviour wanting to hook up to Unity's OnApplicationFocus events
        /// </summary>
        public static readonly List<Action<bool>> UnityFocusHook = new List<Action<bool>>();

        /// <summary>
        /// For any non-MonoBehaviour wanting to hook up to Unity's OnApplicationPause events
        /// </summary>
        public static readonly List<Action<bool>> UnityPauseHook = new List<Action<bool>>();

        /// <summary>
        /// For any non-MonoBehaviour wanting to hook up to Unity's Update events
        /// </summary>
        public static readonly List<Action> UnityUpdateHook = new List<Action>();

        static readonly ConcurrentQueue<SourceAction> UpdateRunQueue = new ConcurrentQueue<SourceAction>();

        static UnityThreadUtilities()
        {
            _logName = "UnityThreadUtils";
#if UNITY_EDITOR
            // Should check here if we are in play mode
            EditorApplication.update += UpdateStatic;
#endif
        }

        public static void RunInUpdate(string source, Action action)
        {
            UpdateRunQueue.Enqueue(new SourceAction
            {
                Source = source,
                Action = action,
            });
        }

        public static void RunInUpdateWithDelay(string source, float delay, Action action)
        {
            RunInUpdate("RunInUpdateWithDelay", () => Instance.StartCoroutine(DelayCoroutine(source, delay, action)));
        }

        public static void RunCoroutineOutsidePlayMode(IEnumerator coroutine)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(coroutine);
                return;
            }
#endif
            RunInUpdate("RunCoroutine", () => Instance.StartCoroutine(coroutine));
        }

        public static void StopOrQuit()
        {
#if UNITY_EDITOR
            Log("stopping...");
            EditorApplication.isPlaying = false;
#else
            Log("quitting...");
            Application.Quit();
#endif
        }

        static IEnumerator DelayCoroutine(string source, float delay, Action action)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                yield return new EditorWaitForSeconds(delay);
            else
#endif
                yield return new WaitForSecondsRealtime(delay);

            try
            {
                SampleTimeTracker.BeginSample(_logName + " " + source);
                action.Invoke();
                SampleTimeTracker.EndSample();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, _logName, source);
            }
        }

        struct SourceAction
        {
            public string Source;
            public Action Action;
        }

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                return;
            }
            // This is where other global initializers should be awaked
        }

        void Start()
        {
            // This is where other global initializers should be started
        }

        public void Update()
        {
            UpdateStatic();
        }

        static void UpdateStatic()
        {
            for (int i = 0; i < UnityUpdateHook.Count; i++)
            {
                UnityUpdateHook[i].Invoke();
            }
            if (!UpdateRunQueue.IsEmpty)
            {
                int max = UpdateRunQueue.Count;
                for (int i = 0; i < max && UpdateRunQueue.TryDequeue(out SourceAction action); i++)
                {
                    SampleTimeTracker.BeginSample(action.Source);
                    try
                    {
                        action.Action();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex, _logName, action.Source);
                    }
                    SampleTimeTracker.EndSample();
                }
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            var actions = new List<Action<bool>>(UnityFocusHook);
            actions.ForEach(action => action.Invoke(hasFocus));
        }

        void OnApplicationPause(bool isPausing)
        {
            var actions = new List<Action<bool>>(UnityPauseHook);

            actions.ForEach(action => action.Invoke(isPausing));
        }

    }
}

