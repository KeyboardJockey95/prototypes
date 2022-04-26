using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Tables
{
    public class DataRow
    {
        public DataFormat ColumnFormat { get; set; }
        public List<object> Cells { get; set; }

        public DataRow(DataFormat columnFormat)
        {
            ColumnFormat = columnFormat;
            Initialize();
        }

        public DataRow(DataRow other, DataFormat columnFormat, bool isRawCopy)
        {
            ColumnFormat = columnFormat;
            InitializeFromOther(other, isRawCopy);
        }

        public DataRow(DataRow other, DataFormat fromFormat, DataFormat toFormat)
        {
            ColumnFormat = toFormat;

            Initialize();

            int count = ColumnFormat.ColumnCount();
            int index;

            for (index = 0; index < count; index++)
            {
                DataColumn column = toFormat.GetColumnIndexed(index);
                DataColumn otherColumn = fromFormat.GetColumn(column.Name);
                object cell;

                if (otherColumn != null)
                {
                    int otherIndex = otherColumn.Index;
                    cell = other.Cells[otherIndex];
                }
                else
                    cell = null;

                SetCellIndexed(column.Index, cell);
            }
        }

        public DataRow(DataRow other)
        {
            ColumnFormat = other.ColumnFormat;
            InitializeFromOther(other, true);
        }

        public DataRow(DataRow other, bool isRawCopy)
        {
            ColumnFormat = other.ColumnFormat;
            InitializeFromOther(other, isRawCopy);
        }

        public DataRow()
        {
            Cells = new List<object>();
        }

        public void Initialize()
        {
            if (ColumnFormat != null)
            {
                Cells = new List<object>(ColumnFormat.ColumnCount());

                foreach (DataColumn column in ColumnFormat.Columns)
                    Cells.Add(null);
            }
            else
                Cells = new List<object>();
        }

        public void InitializeFromOther(DataRow other, bool isRawCopy)
        {
            if (other.Cells == null)
                Initialize();
            else if ((other.ColumnFormat == null) && ((ColumnFormat == null) || (ColumnFormat.ColumnCount() == 0)))
                Cells = new List<object>(other.Cells);
            else if (isRawCopy)
                Cells = new List<object>(other.Cells);
            else
            {
                Initialize();

                int count = ColumnFormat.ColumnCount();
                int index;

                for (index = 0; index < count; index++)
                {
                    DataColumn column = ColumnFormat.GetColumnIndexed(index);
                    DataColumn otherColumn = other.ColumnFormat.GetColumn(column.Name);
                    object cell;

                    if (otherColumn != null)
                    {
                        int otherIndex = otherColumn.Index;
                        cell = other.Cells[otherIndex];
                    }
                    else
                        cell = null;

                    SetCellIndexed(column.Index, cell);
                }
            }
        }

        public void CopyFromOther(DataRow other)
        {
            DataFormat otherColumnFormat = other.ColumnFormat;
            int count = otherColumnFormat.ColumnCount();
            int index;

            for (index = 0; index < count; index++)
            {
                DataColumn otherColumn = otherColumnFormat.GetColumnIndexed(index);
                DataColumn column = ColumnFormat.GetColumn(otherColumn.Name);
                object cell;

                if (column != null)
                {
                    int otherIndex = otherColumn.Index;
                    cell = other.Cells[otherIndex];
                    SetCellIndexed(column.Index, cell);
                }
            }
        }

        public int ColumnCount()
        {
            return ColumnFormat.ColumnCount();
        }

        public void Clear(bool isUseDefaults)
        {
            if (Cells == null)
                return;

            int count = ColumnCount();

            if (Cells.Count() != count)
                throw new Exception("DataRow.Clear: Cells count mismatch.");

            if (isUseDefaults)
            {
                for (int index = 0; index < count; index++)
                    Cells[index] = ColumnFormat.GetColumnIndexed(index).DefaultValue;
            }
            else
            {
                for (int index = 0; index < count; index++)
                    Cells[index] = null;
            }
        }

        public void ClearRange(
            int startIndexInclusive,
            int stopIndexInclusive,
            bool isUseDefaults)
        {
            if (Cells == null)
                return;

            int count = ColumnCount();

            if (Cells.Count() != count)
                throw new Exception("DataRow.ClearRange: Cells count mismatch.");

            if ((startIndexInclusive <= 0) || (stopIndexInclusive >= count))
                throw new Exception("DataRow.ClearRange: Index out of range.");

            if (isUseDefaults)
            {
                for (int index = startIndexInclusive; index <= stopIndexInclusive; index++)
                    Cells[index] = ColumnFormat.GetColumnIndexed(index).DefaultValue;
            }
            else
            {
                for (int index = startIndexInclusive; index <= stopIndexInclusive; index++)
                    Cells[index] = null;
            }
        }

        public DataColumn GetColumn(string name)
        {
            DataColumn column = ColumnFormat.GetColumn(name);
            return column;
        }

        public DataColumn GetColumnCheck(string name)
        {
            DataColumn column = ColumnFormat.GetColumn(name);
            return column;
        }

        public int GetColumnIndex(string name)
        {
            DataColumn column = GetColumn(name);

            if (column == null)
                return -1;

            return column.Index;
        }

        public int GetColumnIndexCheck(string name)
        {
            DataColumn column = GetColumnCheck(name);

            if (column != null)
                return column.Index;

            return -1;
        }

        public object GetCellValue(string columnName)
        {
            int index = GetColumnIndexCheck(columnName);

            if (index == -1)
                return null;

            object cell = Cells[index];
            return cell;
        }

        public string GetCellValueString(string columnName)
        {
            object cell = GetCellValue(columnName);
            return GetCellValueStringFromObject(cell);
        }

        public string GetCellValueStringFromObject(object cell)
        {
            if (cell == null)
                return String.Empty;
            else if (cell is bool)
                return ((bool)cell ? "1" : "0");
            else if (cell is float)
            {
                float floatValue = (float)cell;
                if (float.IsNaN(floatValue))
                    return String.Empty;
                return floatValue.ToString();
            }
            else if (cell is double)
            {
                double doubleValue = (double)cell;
                if (double.IsNaN(doubleValue))
                    return String.Empty;
                return doubleValue.ToString();
            }
            else if (cell is List<bool>)
                return ObjectUtilities.GetStringFromObjectList<bool>(cell as List<bool>);
            else if (cell is List<int>)
                return ObjectUtilities.GetStringFromObjectList<int>(cell as List<int>);
            else if (cell is List<float>)
                return ObjectUtilities.GetStringFromObjectList<float>(cell as List<float>);
            else if (cell is List<double>)
                return ObjectUtilities.GetStringFromObjectList<double>(cell as List<double>);
            else if (cell is List<string>)
                return ObjectUtilities.GetStringFromObjectList<string>(cell as List<string>);
            else
                return cell.ToString();
        }

        public bool GetCellValueBool(string columnName)
        {
            DataColumn column = GetColumnCheck(columnName);
            object cell = Cells[column.Index];

            if (cell == null)
                return false;
            else if (!(cell is bool))
            {
                if (cell is string)
                {
                    bool value = ObjectUtilities.GetBoolFromString((string)cell, column.DefaultValueBool);
                    return value;
                }
                else if (cell is int)
                {
                    bool value = ((int)cell != 0);
                    return value;
                }
                return false;
            }

            return (bool)cell;
        }

        public int GetCellValueInteger(string columnName)
        {
            DataColumn column = GetColumnCheck(columnName);
            object cell = Cells[column.Index];

            if (cell == null)
                return -1;
            else if (!(cell is int))
            {
                if (cell is string)
                {
                    int value = ObjectUtilities.GetIntegerFromString((string)cell, column.DefaultValueInteger);
                    return value;
                }
                return -1;
            }

            return (int)cell;
        }

        public float GetCellValueFloat(string columnName)
        {
            DataColumn column = GetColumnCheck(columnName);
            object cell = Cells[column.Index];

            if (cell == null)
                return 0.0f;
            else if (!(cell is float))
            {
                if (cell is string)
                {
                    float value = ObjectUtilities.GetFloatFromString((string)cell, column.DefaultValueFloat);
                    return value;
                }
                return 0.0f;
            }

            return (float)cell;
        }

        public double GetCellValueDouble(string columnName)
        {
            DataColumn column = GetColumnCheck(columnName);
            object cell = Cells[column.Index];

            if (cell == null)
                return 0.0f;
            else if (!(cell is double))
            {
                if (cell is string)
                {
                    double value = ObjectUtilities.GetDoubleFromString((string)cell, column.DefaultValueDouble);
                    return value;
                }
                return 0.0;
            }

            return (double)cell;
        }

        public List<T> GetCellValueList<T>(string columnName, DataType dataType)
        {
            DataColumn column = GetColumnCheck(columnName);
            object cell = Cells[column.Index];

            if (cell == null)
                return null;
            else if (!(cell is List<T>))
            {
                List<object> list = new List<object>();

                if (cell is string)
                {
                    string[] parts = ((string)cell).Split();

                    if (parts.Length != 0)
                    {
                        foreach (string part in parts)
                        {
                            switch (dataType)
                            {
                                case DataType.None:
                                case DataType.String:
                                    list.Add(part);
                                    break;
                                case DataType.Bool:
                                    list.Add(ObjectUtilities.GetBoolFromString(part, false));
                                    break;
                                case DataType.Integer:
                                    list.Add(ObjectUtilities.GetIntegerFromString(part, 0));
                                    break;
                                case DataType.Float:
                                    list.Add(ObjectUtilities.GetFloatFromString(part, 0.0f));
                                    break;
                                case DataType.Double:
                                    list.Add(ObjectUtilities.GetDoubleFromString(part, 0.0));
                                    break;
                                default:
                                    throw new Exception("GetCellValueList: Unexpected data type.");
                            }
                        }
                    }
                }
                else
                    throw new Exception("GetCellValueList: Unexpected value type.");

                return list.Cast<T>().ToList();
            }

            return (List<T>)cell;
        }

        public object GetCellValueIndexed(int index)
        {
            if ((index < 0) || (index >= Cells.Count()))
                return null;

            object cell = Cells[index];
            return cell;
        }

        public string GetCellValueStringIndexed(int index)
        {
            if ((index < 0) || (index >= Cells.Count()))
                return null;

            object cell = Cells[index];

            if (cell == null)
                return String.Empty;
            else if (cell is string)
                return (string)cell;

            return cell.ToString();
        }

        public bool SetCell(string columnName, object value)
        {
            DataColumn column = GetColumn(columnName);

            if (column == null)
                return false;

            Cells[column.Index] = value;

            return true;
        }

        public bool SetCellIndexed(int index, object value)
        {
            if ((index < 0) || (index >= Cells.Count()))
                return false;

            Cells[index] = value;

            return true;
        }

        public bool SetCellValueFromString(string columnName, string value)
        {
            DataColumn column = GetColumn(columnName);

            if (column == null)
                return false;

            object newValue = null;

            switch (column.MajorType)
            {
                case DataType.None:
                case DataType.String:
                    newValue = value;
                    break;
                case DataType.Bool:
                    newValue = ObjectUtilities.GetBoolFromString(value, false);
                    break;
                case DataType.Integer:
                    newValue = ObjectUtilities.GetIntegerFromString(value, 0);
                    break;
                case DataType.Float:
                    newValue = ObjectUtilities.GetFloatFromString(value, 0.0f);
                    break;
                case DataType.Double:
                    newValue = ObjectUtilities.GetDoubleFromString(value, 0.0);
                    break;
                default:
                    throw new Exception("SetCellValueFromString: Unexpected data type.");
            }

            Cells[column.Index] = newValue;

            return true;
        }

        public int CellCount()
        {
            return Cells.Count();
        }

        public bool IsMatch(List<MatchItem> matchItems)
        {
            if ((matchItems == null) || (matchItems.Count() == 0))
                return true;

            foreach (MatchItem matchItem in matchItems)
            {
                string columnName = matchItem.Key;
                object value = GetCellValue(columnName);

                if (!matchItem.IsMatch(value))
                    return false;
            }

            return true;
        }

        public bool IsMatch(CompoundMatchItem compoundMatchItem)
        {
            bool returnValue = true;

            if ((compoundMatchItem != null) &&
                (compoundMatchItem.MatchItemLists != null) &&
                (compoundMatchItem.MatchItemLists.Count() != 0))
            {
                switch (compoundMatchItem.CompareType)
                {
                    case CompoundMatchItemCompareType.And:
                        returnValue = true;
                        break;
                    case CompoundMatchItemCompareType.Or:
                        returnValue = false;
                        break;
                }

                foreach (List<MatchItem> matchItems in compoundMatchItem.MatchItemLists)
                {
                    bool isMatch = IsMatch(matchItems);

                    switch (compoundMatchItem.CompareType)
                    {
                        case CompoundMatchItemCompareType.And:
                            if (!isMatch)
                                return false;
                            break;
                        case CompoundMatchItemCompareType.Or:
                            if (isMatch)
                                return true;
                            break;
                    }
                }
            }

            return returnValue;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (CellCount() != 0)
            {
                foreach (object cell in Cells)
                {
                    if (sb.Length != 0)
                        sb.Append("|");

                    if (cell == null)
                        sb.Append("(null)");
                    else if (cell is string)
                        sb.Append((string)cell);
                    else
                        sb.Append(GetCellValueStringFromObject(cell));
                }
            }

            return sb.ToString();
        }

        public void CopyColumns(DataRow dataRow, string[] columnNames)
        {
            foreach (string columnName in columnNames)
                SetCell(columnName, dataRow.GetCellValue(columnName));
        }
    }
}
