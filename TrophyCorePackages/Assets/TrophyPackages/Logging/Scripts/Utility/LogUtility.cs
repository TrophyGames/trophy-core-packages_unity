using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Trophy.Logging
{
    public enum LogCondition
    {
        None,
        Full,
    }

    public static class LogUtility
    {
        public static void Log(string message, LogCondition condition = LogCondition.None)
        {
            switch (condition)
            {
                case LogCondition.None:
                    Debug.Log(message);
                    break;

                case LogCondition.Full:
                default:
#if FULL_LOG
                    Debug.Log(message);
#endif
                    break;
            }
        }
    }
}

