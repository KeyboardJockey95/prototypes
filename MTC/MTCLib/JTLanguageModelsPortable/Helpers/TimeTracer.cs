using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTLanguageModelsPortable.Helpers
{
    public class TimeTracer
    {
        public string Name { get; set; }
        public Int64 BaseTime { get; set; }
        public Int64 LastEventTime { get; set; }
        public List<TimeTrace> TimeTraces { get; set; }
        public bool Enable { get; set; }
        public int EventCount { get; set; }
        public Dictionary<string, int> EventCounts { get; set; }
        public static bool EnableInitialValue = false;
        private static Dictionary<string, TimeTracer> _Tracers;


        public TimeTracer(string name)
        {
            Name = name;
            BaseTime = SoftwareTimer.ValueStatic;
            LastEventTime = BaseTime;
            TimeTraces = new List<TimeTrace>();
            EventCounts = new Dictionary<string, int>();
            Enable = EnableInitialValue;
        }

        public void RecordEvent(string name)
        {
            if (Enable)
            {
                Int64 eventTime = SoftwareTimer.ValueStatic;
                Int64 time = eventTime - BaseTime;
                Int64 delta = eventTime - LastEventTime;
                LastEventTime = eventTime;
                try
                {
                    TimeTraces.Add(new TimeTrace(name, time, delta));
                    EventCount = EventCount + 1;
                    int count;
                    if (!EventCounts.TryGetValue(name, out count))
                        EventCounts.Add(name, 1);
                    else
                    {
                        count++;
                        EventCounts[name] = count;
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public void RecordEventEnd(string name)
        {
            if (Enable)
            {
                Int64 time = SoftwareTimer.ValueStatic - BaseTime;

                for (int index = Count() - 1; index >= 0; index--)
                {
                    TimeTrace trace = GetIndexed(index);

                    if (trace.Name == name)
                    {
                        trace.Duration = time - trace.Time;
                        break;
                    }
                }
            }
        }

        public int Count()
        {
            return TimeTraces.Count();
        }

        public TimeTrace GetIndexed(int index)
        {
            if ((index >= 0) && (index < Count()))
                return TimeTraces[index];
            return new TimeTrace("(Bad index)");
        }

        public Int64 TotalDuration(string name)
        {
            Int64 total = 0;

            foreach (TimeTrace trace in TimeTraces)
            {
                if ((name == "All") || trace.Name == name)
                    total += trace.Duration;
            }

            return total;
        }

        public void Clear()
        {
            BaseTime = SoftwareTimer.ValueStatic;
            LastEventTime = BaseTime;
            TimeTraces.Clear();
        }

        public static int Compare(TimeTracer tracer1, TimeTracer tracer2)
        {
            return String.Compare(tracer1.Name, tracer2.Name);
        }

        public static TimeTracer MainTracer
        {
            get
            {
                return GetTracer("Main");
            }
        }

        public static TimeTracer GetTracer(string name)
        {
            if (_Tracers == null)
                _Tracers = new Dictionary<string, TimeTracer>();

            TimeTracer tracer = null;

            lock (_Tracers)
            {
                if (!_Tracers.TryGetValue(name, out tracer))
                {
                    tracer = new TimeTracer(name);

                    try
                    {
                        _Tracers.Add(name, tracer);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return tracer;
        }

        public static List<TimeTracer> GetTracers(bool doSort)
        {
            List<TimeTracer> tracers = new List<TimeTracer>();

            if (_Tracers != null)
            {
                lock (_Tracers)
                {
                    foreach (KeyValuePair<string, TimeTracer> kvp in _Tracers)
                    {
                        if (kvp.Value != null)
                            tracers.Add(kvp.Value);
                    }
                }
            }

            if (doSort)
                tracers.Sort(TimeTracer.Compare);

            return tracers;
        }

        public static void RecordEventStatic(string tracerName, string eventName)
        {
            GetTracer(tracerName).RecordEvent(eventName);
            GetTracer("Combined").RecordEvent(tracerName + " " + eventName);
        }

        public static void RecordEventEndStatic(string tracerName, string eventName)
        {
            GetTracer(tracerName).RecordEventEnd(eventName);
            GetTracer("Combined").RecordEventEnd(tracerName + " " + eventName);
        }

        public static void SetEnableStatic(string tracerName, bool state)
        {
            GetTracer(tracerName).Enable = state;
            GetTracer("Combined").Enable = state;
        }

        public static int GetEventCount(string tracerName)
        {
            return GetTracer(tracerName).EventCount;
        }

        public static void ClearStatic(string tracerName)
        {
            GetTracer(tracerName).Clear();
            GetTracer("Combined").Clear();
        }
    }
}
