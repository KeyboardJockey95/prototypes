using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Tables
{
    public class DataRowComparer : IComparer<DataRow>
    {
        public List<int> ColumnNumbers;     // Base 1.  Negatives means sort descending.

        public DataRowComparer(List<int> columnNumbers)
        {
            ColumnNumbers = columnNumbers;
        }

        // Compares length of nodes.  If x length is greater than y length returns 1.
        public int Compare(DataRow x, DataRow y)
        {
            if (x == y)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            int columnCount = ColumnNumbers.Count();
            int columnIndex;

            /*
            if ((x.GetCellValueString("Word") == "cumplan") && (y.GetCellValueString("Word") == "cumplan"))
            {
                if ((x.GetCellValueString("Translation") == "fulfilled") && (y.GetCellValueString("Translation") == "may"))
                    ApplicationData.Global.PutConsoleMessage("cumplan/fulfulled+may");
                else if ((x.GetCellValueString("Translation") == "may") && (y.GetCellValueString("Translation") == "fulfilled"))
                    ApplicationData.Global.PutConsoleMessage("cumplan/may+fulfulled");
            }
            */

            for (columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                int columnNumber = ColumnNumbers[columnIndex];
                int index;
                object ox;
                object oy;
                int diff;

                if (columnNumber < 0)
                {
                    index = -columnNumber - 1;
                    ox = x.GetCellValueIndexed(index);
                    oy = y.GetCellValueIndexed(index);
                    diff = -ObjectUtilities.CompareObjects(ox, oy);
                }
                else
                {
                    index = columnNumber - 1;
                    ox = x.GetCellValueIndexed(index);
                    oy = y.GetCellValueIndexed(index);
                    diff = ObjectUtilities.CompareObjects(ox, oy);
                }

                if ((ox != null) && (oy != null))
                {
                    if (ox.GetType() != oy.GetType())
                        throw new Exception("In DataRowComparer, column number " + columnNumber.ToString() + " had different object types.");
                }

                if (diff != 0)
                    return diff;
            }

            return 0;
        }
    }
}
