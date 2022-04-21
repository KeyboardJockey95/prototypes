using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Tables
{
    public class GeneralStatistics
    {
        public List<GeneralStatistic> Statistics { get; set; }
        public Dictionary<string, GeneralStatistic> StatisticsDictionary { get; set; }

        public GeneralStatistics(List<GeneralStatistic> statistics)
        {
            ClearGeneralStatistics();
            AddGeneralStatistics(statistics);
        }

        public GeneralStatistics(GeneralStatistics other)
        {
            CopyGeneralStatistics(other);
        }

        public GeneralStatistics()
        {
            ClearGeneralStatistics();
        }

        public void ClearGeneralStatistics()
        {
            Statistics = new List<GeneralStatistic>();
            StatisticsDictionary = new Dictionary<string, GeneralStatistic>();
        }

        public void CopyGeneralStatistics(GeneralStatistics other)
        {
            Statistics = new List<GeneralStatistic>();
            StatisticsDictionary = new Dictionary<string, GeneralStatistic>();

            foreach (GeneralStatistic otherStatistic in other.Statistics)
            {
                GeneralStatistic newStatistic = new GeneralStatistic(otherStatistic);
                Statistics.Add(newStatistic);
                StatisticsDictionary.Add(newStatistic.Name, newStatistic);
            }
        }

        public GeneralStatistic GetGeneralStatisticIndexed(int index)
        {
            if ((index < 0) || (index >= Statistics.Count()))
                return null;

            GeneralStatistic generalStatistic = Statistics[index];

            return generalStatistic;
        }

        public GeneralStatistic GetGeneralStatisticNamed(string name)
        {
            GeneralStatistic generalStatistic;

            if (StatisticsDictionary.TryGetValue(name, out generalStatistic))
                return generalStatistic;

            return generalStatistic;
        }

        public void AddGeneralStatistic(GeneralStatistic generalStatistic)
        {
            Statistics.Add(generalStatistic);
            StatisticsDictionary.Add(generalStatistic.Name, generalStatistic);
        }

        public void AddGeneralStatistics(List<GeneralStatistic> generalStatistics)
        {
            if (generalStatistics == null)
                return;

            foreach (GeneralStatistic generalStatistic in generalStatistics)
            {
                Statistics.Add(generalStatistic);
                StatisticsDictionary.Add(generalStatistic.Name, generalStatistic);
            }
        }

        public void InsertGeneralStatistic(int index, GeneralStatistic generalStatistic)
        {
            Statistics.Insert(index, generalStatistic);
            StatisticsDictionary.Add(generalStatistic.Name, generalStatistic);
        }

        public void DeleteGeneralStatisticIndexed( int index)
        {
            if ((index < 0) || (index >= Statistics.Count()))
                return;

            GeneralStatistic generalStatistic = Statistics[index];

            Statistics.RemoveAt(index);
            StatisticsDictionary.Remove(generalStatistic.Name);
        }

        public void DeleteGeneralStatisticNamed(string name)
        {
            GeneralStatistic generalStatistic;

            if (!StatisticsDictionary.TryGetValue(name, out generalStatistic))
                return;

            Statistics.Remove(generalStatistic);
            StatisticsDictionary.Remove(generalStatistic.Name);
        }

        public void DeleteAllGeneralStatistics()
        {
            Statistics.Clear();
            StatisticsDictionary.Clear();
        }

        public int GeneralStatisticCount()
        {
            return Statistics.Count();
        }

        public void AddCountStatistic(string name, string label)
        {
            AddGeneralStatistic(new GeneralStatistic(name, label, StatisticType.Count));
        }

        public void AddCountStatistic(string name)
        {
            AddGeneralStatistic(new GeneralStatistic(name, StatisticType.Count));
        }

        public void AddPercentageStatistic(string name)
        {
            AddGeneralStatistic(new GeneralStatistic(name, StatisticType.Percentage));
        }

        public void AddCountHistogramStatistic(string name, string label)
        {
            AddGeneralStatistic(new GeneralStatistic(name, label, StatisticType.CountHistogram));
        }

        public void AddCountHistogramStatistic(string name)
        {
            AddGeneralStatistic(new GeneralStatistic(name, StatisticType.CountHistogram));
        }

        public int GetCount(string name)
        {
            GeneralStatistic generalStatistic;

            if (!StatisticsDictionary.TryGetValue(name, out generalStatistic))
                return 0;

            return generalStatistic.Count;
        }

        public void SetCount(string name, int count)
        {
            GeneralStatistic generalStatistic;

            if (!StatisticsDictionary.TryGetValue(name, out generalStatistic))
                return;

            generalStatistic.Count = count;
        }

        public void IncrementCount(string name)
        {
            GeneralStatistic generalStatistic;

            if (!StatisticsDictionary.TryGetValue(name, out generalStatistic))
                return;

            generalStatistic.IncrementCount();
        }

        public void DecrementCount(string name)
        {
            GeneralStatistic generalStatistic;

            if (!StatisticsDictionary.TryGetValue(name, out generalStatistic))
                return;

            generalStatistic.DecrementCount();
        }

        public void SetPercentage(
            string name,
            double percentage)  // 1.0 == 100 %
        {
            GeneralStatistic generalStatistic;

            if (!StatisticsDictionary.TryGetValue(name, out generalStatistic))
                return;

            generalStatistic.Percentage = percentage;
        }

        public void SetPercentage(
            string name,
            string numeratorName,
            string denominatorName)
        {
            int numerator = GetCount(numeratorName);
            int denominator = GetCount(denominatorName);
            double percentage;

            if (denominator != 0)
                percentage = (double)numerator / denominator;
            else
                percentage = double.NaN;

            SetPercentage(name, percentage);
        }

        public void SetCountHistogram(string name, List<int> histogram)
        {
            GeneralStatistic generalStatistic;

            if (!StatisticsDictionary.TryGetValue(name, out generalStatistic))
                return;

            generalStatistic.CountHistogram = histogram;
        }

        public void IncrementCountHistogram(string name, int count)
        {
            GeneralStatistic generalStatistic;

            if (!StatisticsDictionary.TryGetValue(name, out generalStatistic))
                return;

            generalStatistic.IncrementCountHistogram(count);
        }

        public void DecrementCountHistogram(string name, int count)
        {
            GeneralStatistic generalStatistic;

            if (!StatisticsDictionary.TryGetValue(name, out generalStatistic))
                return;

            generalStatistic.DecrementCountHistogram(count);
        }

        public string FormatReport(string reportHeading)
        {
            StringBuilder sb = new StringBuilder();
            int maxLabelSize = GetMaxLabelSize();

            if (!String.IsNullOrEmpty(reportHeading))
            {
                sb.AppendLine(reportHeading);
                sb.AppendLine();
            }

            foreach (GeneralStatistic generalStatistic in Statistics)
            {
                string label = generalStatistic.Label;
                string spaces = TextUtilities.GetSpaces(maxLabelSize - label.Length);
                sb.AppendLine(label + ":" + spaces + generalStatistic.ValueString());
            }

            return sb.ToString();
        }

        public string FormatReportTSV(string reportHeading)
        {
            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrEmpty(reportHeading))
            {
                sb.AppendLine(reportHeading);
                sb.AppendLine();
            }

            foreach (GeneralStatistic generalStatistic in Statistics)
                sb.AppendLine(generalStatistic.Label + "\t" + generalStatistic.ValueString());

            return sb.ToString();
        }

        public static string FormatMultipleReportTSV(
            string reportHeading,
            List<GeneralStatistics> statisticsList)
        {
            if ((statisticsList == null) || (statisticsList.Count == 0))
                return "(no statistics)\n";

            GeneralStatistics firstStatistics = statisticsList[0];
            StringBuilder sb = new StringBuilder();
            int outsideCount = firstStatistics.GeneralStatisticCount();
            int outsideIndex;

            if (!String.IsNullOrEmpty(reportHeading))
            {
                sb.AppendLine(reportHeading);
                sb.AppendLine();
            }

            for (outsideIndex = 0; outsideIndex < outsideCount; outsideIndex++)
            {
                int insideCount = statisticsList.Count();
                int insideIndex;

                for (insideIndex = 0; insideIndex < insideCount; insideIndex++)
                {
                    GeneralStatistics statistics = statisticsList[insideIndex];
                    GeneralStatistic generalStatistic = statistics.GetGeneralStatisticIndexed(outsideIndex);

                    if (insideIndex == 0)
                    {
                        string label = generalStatistic.Label;
                        sb.Append(label);
                    }

                    sb.Append("\t");
                    sb.Append(generalStatistic.ValueString());
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return FormatReport(null);
        }

        public int GetMaxLabelSize()
        {
            int count = 0;

            foreach (GeneralStatistic generalStatistic in Statistics)
            {
                int temp = generalStatistic.Label.Length;

                if (temp > count)
                    count = temp;
            }

            return count;
        }

        public string FormatHistogram(List<int> histogram)
        {
            int count = 0;
            int index;
            StringBuilder sb = new StringBuilder();

            for (index = 0; index < count; index++)
            {
                if (index != 0)
                    sb.Append(", ");

                int value = histogram[index];
                sb.Append(value.ToString());
            }

            return sb.ToString();
        }
    }
}
