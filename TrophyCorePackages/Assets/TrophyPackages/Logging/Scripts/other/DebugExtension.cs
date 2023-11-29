using UnityEngine;
using Object = UnityEngine.Object;

namespace Trophy.LogManagement
{
    public static class DebugExtension
    {
        /// <summary>
        /// Logs an assertion message.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="context">The object to which the message applies (optional).</param>
        public static void LogAssertion(string message, Object context = null)
        {
            if (context == null)
                Debug.LogAssertion(message);
            else
                Debug.LogAssertion(message, context);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The error message to be logged.</param>
        /// <param name="context">The object to which the error message applies (optional).</param>
        public static void LogError(string message, Object context = null)
        {
            if (context == null)
                Debug.LogError(message);
            else
                Debug.LogError(message, context);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The warning message to be logged.</param>
        /// <param name="context">The object to which the warning message applies (optional).</param>
        public static void LogWarning(string message, Object context = null)
        {
            if (context == null)
                Debug.LogWarning(message);
            else
                Debug.LogWarning(message, context);
        }

        /// <summary>
        /// Logs a general message.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="context">The object to which the message applies (optional).</param>
        public static void Log(string message, Object context = null)
        {
            if (context == null)
                Debug.Log(message);
            else
                Debug.Log(message, context);
        }

    }
}