using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Matchers;

namespace JTLanguageModelsPortable.Language
{
    public partial class JapaneseTool : LanguageTool
    {
        public override List<string> AutomaticRowKeys(string type)
        {
            InflectorTable inflectorTable = InflectorTable(type);
            List<string> rowKeys = null;

            if (inflectorTable != null)
                rowKeys = inflectorTable.AutomaticRowKeys;

            if ((rowKeys == null) || (rowKeys.Count() == 0))
            {
                rowKeys = new List<string>();

                switch (type)
                {
                    case "Verb":
                        rowKeys.Add("Polarity");
                        break;
                    case "Noun":
                        rowKeys.Add("Polarity");
                        break;
                    case "Adjective":
                        rowKeys.Add("Polarity");
                        break;
                    default:
                        break;
                }
            }

            return rowKeys;
        }

        public override List<string> AutomaticColumnKeys(string type)
        {
            InflectorTable inflectorTable = InflectorTable(type);
            List<string> columnKeys = null;

            if (inflectorTable != null)
                columnKeys = inflectorTable.AutomaticColumnKeys;

            if ((columnKeys == null) || (columnKeys.Count() == 0))
            {
                columnKeys = new List<string>();

                switch (type)
                {
                    case "Verb":
                        columnKeys.Add("Politeness");
                        break;
                    case "Noun":
                        columnKeys.Add("Politeness");
                        break;
                    case "Adjective":
                        columnKeys.Add("Politeness");
                        break;
                    default:
                        break;
                }
            }

            return columnKeys;
        }

        public override bool AutomaticUsePronouns()
        {
            return false;
        }
    }
}
