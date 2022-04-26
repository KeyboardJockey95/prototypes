using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTLanguageModelsPortable.Helpers
{
    public class TimeTrace
    {
        public string Name;
        public Int64 Time;
        public Int64 Delta;      // From previous event.
        public Int64 Duration;   // From this event start, or 0 if not measured.

        public TimeTrace(string name, Int64 time, Int64 delta)
        {
            Name = name;
            Time = time;
            Delta = delta;
            Duration = 0;
        }

        public TimeTrace(string name)
        {
            Name = name;
            Time = 0;
            Delta = 0;
            Duration = 0;
        }

        public static int Compare(TimeTrace trace1, TimeTrace trace2)
        {
            return String.Compare(trace1.Name, trace2.Name);
        }
    }
}
