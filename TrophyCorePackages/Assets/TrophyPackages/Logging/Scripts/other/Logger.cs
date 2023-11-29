using System;
using System.Collections;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace Trophy.LogManagement
{
    public static class Logger
    {
        /// <summary>
        /// Logs a message with the LogType.Log type.
        /// </summary>
        /// <param name="objArray">The objects to be included in the log message.</param>
        public static void Log(params object[] objArray)
        {
            LogCustom(LogType.Log, true, null, objArray);
        }

        /// <summary>
        /// Logs a message with the LogType.Log type without including a stack trace.
        /// </summary>
        /// <param name="objArray">The objects to be included in the log message.</param>
        public static void LogNoTrace(params object[] objArray)
        {
            LogCustom(LogType.Log, false, null, objArray);
        }

        /// <summary>
        /// Logs an error message with the LogType.Error type.
        /// </summary>
        /// <param name="objArray">The objects to be included in the error message.</param>
        public static void LogError(params object[] objArray)
        {
            LogCustom(LogType.Error, true, null, objArray);
        }

        /// <summary>
        /// Logs an error message with the LogType.Error type without including a stack trace.
        /// </summary>
        /// <param name="objArray">The objects to be included in the error message.</param>
        public static void LogErrorNoTrace(params object[] objArray)
        {
            LogCustom(LogType.Error, false, null, objArray);
        }

        // TODO: Usually call sites ignore objArray in favor of their own logic, tweak call sites or this
        /// <summary>
        /// Logs an exception along with additional objects.
        /// </summary>
        /// <param name="ex">The exception to be logged.</param>
        /// <param name="objArray">The additional objects to be included in the log message.</param>
        public static void LogException(Exception ex, params object[] objArray)
        {
            if (objArray.Length > 0)
                LogNoTrace(objArray);

            Debug.LogException(ex);
        }

        /// <summary>
        /// Logs a warning message with the LogType.Warning type.
        /// </summary>
        /// <param name="objArray">The objects to be included in the warning message.</param>
        public static void LogWarning(params object[] objArray)
        {
            LogCustom(LogType.Warning, true, null, objArray);
        }

        /// <summary>
        /// Logs a warning message with the LogType.Warning type without including a stack trace.
        /// </summary>
        /// <param name="objArray">The objects to be included in the warning message.</param>
        public static void LogWarningNoTrace(params object[] objArray)
        {
            LogCustom(LogType.Warning, false, null, objArray);
        }

        /// <summary>
        /// Logs a custom message with the specified log type, stack trace inclusion, context, and objects.
        /// </summary>
        /// <param name="type">The log type.</param>
        /// <param name="allowTrace">Determines whether to include a stack trace in the log output.</param>
        /// <param name="context">The context object to which the message applies.</param>
        /// <param name="objArray">The objects to be included in the log message.</param>
        public static void LogCustom(LogType type, bool allowTrace, Object context, params object[] objArray)
        {
            Profiler.BeginSample("Logger.LogCustom");
            string message = ToMessage(type, objArray);
            var storedTraceType = StackTraceLogType.Full;

            if (!allowTrace)
            {
                storedTraceType = Application.GetStackTraceLogType(type);
                Application.SetStackTraceLogType(type, StackTraceLogType.None);
            }

            switch (type)
            {
                case LogType.Assert:
                    DebugExtension.LogAssertion(message, context);
                    break;
                case LogType.Error:
                    Debug.LogError(message, context);
                    break;
                case LogType.Log:
                    Debug.Log(message, context);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message, context);
                    break;
                default:
                    LogError("Logger LogType", type, "not expected here");
                    break;
            }

            if (!allowTrace)
                Application.SetStackTraceLogType(type, storedTraceType);

            Profiler.EndSample();
        }

        /// <summary>
        /// Formats a log message based on the log type and the provided objects.
        /// </summary>
        /// <param name="type">The log type.</param>
        /// <param name="objArray">The objects to be included in the log message.</param>
        /// <returns>The formatted log message.</returns>
        public static string ToMessage(LogType type, IEnumerable objArray)
        {
            Profiler.BeginSample("Logger.ToMessage");
            var sb = new StringBuilder();

            sb.Append(
                type switch
                {
                    LogType.Assert => "ASSERT",
                    LogType.Error => "ERROR",
                    LogType.Warning => "WARNING",
                    _ => DateTime.UtcNow.ToString("HH:mm:ss", CultureInfo.InvariantCulture)
                }
            );

            NestedToMessage(sb, objArray);
            string result = sb.ToString();
            Profiler.EndSample();
            return result;
        }

        /// <summary>
        /// Helper method to recursively handle nested objects and append them to a StringBuilder.
        /// </summary>
        /// <param name="sb">The StringBuilder to append the nested objects.</param>
        /// <param name="objArray">The objects to be appended.</param>
        static void NestedToMessage(StringBuilder sb, IEnumerable objArray)
        {
            Profiler.BeginSample("Logger.NestedToMessage");

            foreach (object obj in objArray)
            {
                switch (obj)
                {
                    case null:
                        sb.Append(" null");
                        break;
                    case string s:
                        sb.Append(" ");
                        sb.Append(s);
                        break;
                    case IEnumerable e:
                        NestedToMessage(sb, e);
                        break;
                    case IFormattable f:
                        sb.Append(" ");
                        sb.Append(f.ToString(null, CultureInfo.InvariantCulture));
                        break;
                    default:
                        sb.Append(" ");
                        sb.Append(obj);
                        break;
                }
            }

            Profiler.EndSample();
        }
    }
}
