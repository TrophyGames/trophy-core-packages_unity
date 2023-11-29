using UnityEngine;

namespace Trophy.LogManagement
{
    /// <summary>
    /// Abstract base class for objects that can log messages.
    /// </summary>
    public abstract class LogableSingleton<T> : SingletonMonoBehaviour<T> where T : MonoBehaviour
    {
        protected static string _logName = "LogableObject";

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="objArray">The objects to log.</param>
        public static void Log(params object[] objArray)
        {
            Logger.Log(_logName, objArray);
        }


        /// <summary>
        /// Logs a message without a stack trace.
        /// </summary>
        /// <param name="objArray">The objects to log.</param>
        public static void LogNoTrace(params object[] objArray)
        {
            Logger.LogNoTrace(_logName, objArray);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="objArray">The objects to log.</param>
        public static void LogError(params object[] objArray)
        {
            Logger.LogError(_logName, objArray);
        }

        /// <summary>
        /// Logs an error message without a stack trace.
        /// </summary>
        /// <param name="objArray">The objects to log.</param>
        public static void LogErrorNoTrace(params object[] objArray)
        {
            Logger.LogErrorNoTrace(_logName, objArray);
        }

        /// <summary>
        /// Logs a warning message without a stack trace.
        /// </summary>
        /// <param name="objArray">The objects to log.</param>
        public static void LogWarningNoTrace(params object[] objArray)
        {
            Logger.LogWarningNoTrace(_logName, objArray);
        }
    }

}
