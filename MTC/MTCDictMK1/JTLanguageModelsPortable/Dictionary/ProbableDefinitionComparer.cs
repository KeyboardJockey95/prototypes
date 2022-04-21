using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.Dictionary
{
    public class ProbableDefinitionComparer : IComparer<ProbableDefinition>
    {
        public string[] SortOrder;

        public static string[] DefaultSortOrder = new string[] { "-Frequency", "TargetMeaning" };

        public ProbableDefinitionComparer()
        {
            SortOrder = DefaultSortOrder;
        }

        // Sort order is an array of strings from: "Frequency", "Probability", "Meaning"
        // If a name is preceeded by "-", it means descending order for that item.
        public ProbableDefinitionComparer(string[] sortOrder)
        {
            SortOrder = sortOrder;
        }

        public int Compare(ProbableDefinition x, ProbableDefinition y)
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
                    case "TargetMeaning":
                        {
                            int diff = String.Compare(x.TargetMeaning, y.TargetMeaning);
                            if (diff != 0)
                                return diff;
                        }
                        break;
                    case "-TargetMeaning":
                        {
                            int diff = String.Compare(x.TargetMeaning, y.TargetMeaning);
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
    }
}
