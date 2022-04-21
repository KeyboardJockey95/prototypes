using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Tables
{
    public enum StatisticType
    {
        None,
        Count,
        Percentage,
        CountHistogram
    }

    public class GeneralStatistic
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public StatisticType Type { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }          // 1.0 = 100 %
        public List<int> CountHistogram { get; set; }

        public GeneralStatistic(
            string name,
            string label,
            StatisticType type)
        {
            Name = name;
            Label = label;
            Type = type;
            Count = 0;
            Percentage = 0.0;

            switch (Type)
            {
                case StatisticType.None:
                    break;
                case StatisticType.Count:
                    break;
                case StatisticType.Percentage:
                    break;
                case StatisticType.CountHistogram:
                    CountHistogram = new List<int>();
                    break;
            }
        }

        public GeneralStatistic(
            string name,
            StatisticType type)
        {
            Name = name;
            Label = name;
            Type = type;
            Count = 0;
            Percentage = 0.0;

            switch (Type)
            {
                case StatisticType.None:
                    break;
                case StatisticType.Count:
                    break;
                case StatisticType.Percentage:
                    break;
                case StatisticType.CountHistogram:
                    CountHistogram = new List<int>();
                    break;
            }
        }

        public GeneralStatistic(GeneralStatistic other)
        {
            Name = other.Name;
            Label = other.Label;
            Type = other.Type;
            Count = other.Count;
            Percentage = other.Percentage;

            if (other.CountHistogram != null)
                CountHistogram = new List<int>(other.CountHistogram);
            else
                CountHistogram = null;
        }

        public GeneralStatistic()
        {
            Name = String.Empty;
            Label = String.Empty;
            Type = StatisticType.None;
            Count = 0;
            Percentage = 0.0;
            CountHistogram = null;
        }

        public override string ToString()
        {
            string returnValue = Label + ": " + ValueString();
            return returnValue;
        }

        public string ValueString()
        {
            string returnValue;

            switch (Type)
            {
                case StatisticType.None:
                    returnValue = "(None)";
                    break;
                case StatisticType.Count:
                    returnValue = Count.ToString();
                    break;
                case StatisticType.Percentage:
                    returnValue = Percentage.ToString("P");
                    break;
                case StatisticType.CountHistogram:
                    returnValue = ObjectUtilities.GetStringFromIntList(CountHistogram);
                    break;
                default:
                    returnValue = String.Empty;
                    break;
            }

            return returnValue;
        }

        public void IncrementCount()
        {
            Count += 1;
        }

        public void DecrementCount()
        {
            Count -= 1;
        }

        public void IncrementCountHistogram(int count)
        {
            if (CountHistogram == null)
                CountHistogram = new List<int>(count);

            while (count >= CountHistogram.Count())
                CountHistogram.Add(0);

            CountHistogram[count] += 1;
        }

        public void DecrementCountHistogram(int count)
        {
            if (CountHistogram == null)
                CountHistogram = new List<int>(count);

            while (count >= CountHistogram.Count())
                CountHistogram.Add(0);

            CountHistogram[count] -= 1;
        }
    }
}
