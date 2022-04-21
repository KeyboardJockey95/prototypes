using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace JTLanguageModelsPortable.Helpers
{
    public class SoftwareTimer
    {
        protected static bool isPerfCounterSupported = false;
        protected static Int64 frequency = 0;
        protected Int64 startTime;
        protected Int64 stopTime;
        public static SoftwareTimer GlobalTimer;

        public SoftwareTimer()
        {
        }

        public Int64 Frequency
        {
            get
            {
                return frequency;
            }
        }

        public virtual Int64 Value
        {
            get
            {
                if (GlobalTimer != null)
                    return GlobalTimer.Value;
                return 0;
            }
        }

        public static Int64 ValueStatic
        {
            get
            {
                if (GlobalTimer != null)
                    return GlobalTimer.Value;
                return 0;
            }
        }

        public void Start()
        {
            startTime = Value;
        }

        public void Stop()
        {
            stopTime = Value;
        }

        public Int64 GetTimeInTicks()
        {
            return stopTime - startTime;
        }

        public double GetTimeInSeconds()
        {
            Int64 elapsedTicks = stopTime - startTime;
            double elapsedSeconds = (double)elapsedTicks / frequency;
            return elapsedSeconds;
        }

        public double GetTimeInMilliSeconds()
        {
            Int64 elapsedTicks = stopTime - startTime;
            double elapsedSeconds = (double)elapsedTicks / frequency;
            return elapsedSeconds * 1000;
        }

        public double GetTimeInMicroSeconds()
        {
            Int64 elapsedTicks = stopTime - startTime;
            double elapsedSeconds = (double)elapsedTicks / frequency;
            return elapsedSeconds * 1000000;
        }

        public double GetElapsedTimeInSeconds()
        {
            Int64 elapsedTicks = Value - startTime;
            double elapsedSeconds = (double)elapsedTicks / frequency;
            return elapsedSeconds;
        }

        public static double GetTimeInSecondsStatic(Int64 startTime, Int64 stopTime)
        {
            Int64 elapsedTicks = stopTime - startTime;
            double elapsedSeconds = (double)elapsedTicks / frequency;
            return elapsedSeconds;
        }

        public static double GetTimeInMilliSecondsStatic(Int64 startTime, Int64 stopTime)
        {
            Int64 elapsedTicks = stopTime - startTime;
            double elapsedSeconds = (double)elapsedTicks / frequency;
            return elapsedSeconds * 1000;
        }

        public static double GetTimeInMicroSecondsStatic(Int64 startTime, Int64 stopTime)
        {
            Int64 elapsedTicks = stopTime - startTime;
            double elapsedSeconds = (double)elapsedTicks / frequency;
            return elapsedSeconds * 1000000;
        }

        public static double GetElapsedTimeInSecondsStatic(Int64 startTime)
        {
            long value;
            if (GlobalTimer != null)
                value = GlobalTimer.Value;
            else
                value = 0;
            Int64 elapsedTicks = value - startTime;
            double elapsedSeconds = (double)elapsedTicks / frequency;
            return elapsedSeconds;
        }
    }
}
