
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor.PackageManager.UI;
using UnityEngine.Profiling;

namespace Trophy.LogManagement
{
    public static class SampleTimeTracker
    {
        static readonly Stack<SampleStack> Samples = new Stack<SampleStack>();
        
        /// <summary>
        /// Begins a new sample with the specified name.
        /// </summary>
        /// <param name="name">The name of the sample.</param>
        public static void BeginSample(string name)
        {
            Samples.Push(NewSample(name));
            Profiler.BeginSample(name);
        }

        private static SampleStack NewSample(string name)
        {
            return new SampleStack
            {
                Name = name,
                Timer = Stopwatch.StartNew(),
            };
        }
        
        /// <summary>
        /// Ends the current sample and returns the elapsed time in milliseconds.
        /// </summary>
        /// <returns>The elapsed time in milliseconds.</returns>
        public static long EndSample()
        {
            Profiler.EndSample();

            if (Samples.Count == 0)
            {
                // EndSample without a matching BeginSample
                Logger.LogError("RunTimeTracker sample underflow");
                return 0L;
            }

            SampleStack trackingPoint = Samples.Pop();
            trackingPoint.Timer.Stop();
            long ms = trackingPoint.Timer.ElapsedMilliseconds;

            if (ms >= 1000)
                Logger.LogNoTrace("SampleTimeTracker", trackingPoint.Name, ms, "ms");

            return ms;
        }

    }
}