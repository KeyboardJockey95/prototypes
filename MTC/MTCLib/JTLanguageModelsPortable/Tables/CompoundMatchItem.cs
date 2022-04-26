using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTLanguageModelsPortable.Tables
{
    public enum CompoundMatchItemCompareType
    {
        And,
        Or
    }

    public class CompoundMatchItem
    {
        public List<List<MatchItem>> MatchItemLists { get; set; }
        public CompoundMatchItemCompareType CompareType { get; set; }

        public CompoundMatchItem(
            List<List<MatchItem>> matchItemLists,
            CompoundMatchItemCompareType compareType)
        {
            MatchItemLists = matchItemLists;
            CompareType = compareType;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (MatchItemLists != null)
            {
                foreach (List<MatchItem> matchItems in MatchItemLists)
                {
                    if (sb.Length != 0)
                        sb.Append("},");
                    else
                        sb.Append("{");

                    bool first = true;

                    foreach (MatchItem matchItem in matchItems)
                    {
                        if (first)
                            first = false;
                        else
                            sb.Append(",");

                        sb.Append(matchItem.ToString());
                    }
                }
            }

            return "MatchItemLists = " + sb.ToString() + ", CompareType = " + CompareType.ToString();
        }
    }
}
