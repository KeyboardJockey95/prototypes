using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.Dictionary
{
    public class ProbableMeaningComparer : IComparer<ProbableMeaning>
    {
        public string[] SortOrder;

        public static string[] DefaultSortOrder = new string[] { "-Frequency", "-Probability", "Meaning" };
        public static string[] DescendingProbabilitySortOrder = new string[] { "-Probability" };

        public ProbableMeaningComparer()
        {
            SortOrder = DefaultSortOrder;
        }

        // Sort order is an array of strings from: "Frequency", "Probability", "Meaning"
        // If a name is preceeded by "-", it means descending order for that item.
        public ProbableMeaningComparer(string[] sortOrder)
        {
            SortOrder = sortOrder;
        }

        public int Compare(ProbableMeaning x, ProbableMeaning y)
        {
            if (x == y)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            foreach (string sortItem in SortOrder)
            {
                switch (sortItem)
                {
                    case "Frequency":
                        if (x.Frequency < y.Frequency)
                            return -1;
                        else if (x.Frequency > y.Frequency)
                            return 1;
                        break;
                    case "-Frequency":
                        if (x.Frequency < y.Frequency)
                            return 1;
                        else if (x.Frequency > y.Frequency)
                            return -1;
                        break;
                    case "Probability":
                        if (x.Probability < y.Probability)
                            return -1;
                        else if (x.Probability > y.Probability)
                            return 1;
                        break;
                    case "-Probability":
                        if (x.Probability < y.Probability)
                            return 1;
                        else if (x.Probability > y.Probability)
                            return -1;
                        break;
                    case "Meaning":
                        {
                            int diff = String.Compare(x.Meaning, y.Meaning);
                            if (diff != 0)
                                return diff;
                        }
                        break;
                    case "-Meaning":
                        {
                            int diff = String.Compare(x.Meaning, y.Meaning);
                            if (diff != 0)
                                return -diff;
                        }
                        break;
                    default:
                        break;
                }
            }

            return 0;
        }

        // Old version, fixed sorting order.
#if false
        public int Compare(ProbableMeaning x, ProbableMeaning y)
        {
            if (x == y)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            if (x.Frequency == y.Frequency)
            {
                if (x.Probability == y.Probability)
                {
                    int diff = String.Compare(x.Meaning, y.Meaning);
                    return diff;
                }
                else if (float.IsNaN(x.Probability))
                    return 1;
                else if (float.IsNaN(y.Probability))
                    return -1;
                else if (x.Probability < y.Probability)
                    return 1;
                else
                    return -1;
            }
            else if (x.Frequency < y.Frequency)
                return 1;
            else
                return -1;
        }
#endif
    }
}
