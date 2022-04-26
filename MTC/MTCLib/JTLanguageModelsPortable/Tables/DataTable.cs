using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Tables
{
    public class DataTable
    {
        public string Name { get; set; }
        public DataFormat ColumnFormat { get; set; }
        public List<DataRow> Rows { get; set; }

        public DataTable(
            string name,
            DataFormat columnFormat,
            List<DataRow> rows)
        {
            Name = name;
            ColumnFormat = columnFormat;
            Rows = rows;
        }

        public DataTable(string name, DataFormat columnFormat)
        {
            Name = name;
            ColumnFormat = columnFormat;
            Rows = new List<DataRow>();
        }

        public DataTable(DataTable other)
        {
            Name = other.Name;
            ColumnFormat = other.ColumnFormat;
            Rows = new List<DataRow>();

            foreach (DataRow row in other.Rows)
                Rows.Add(new DataRow(row, ColumnFormat, true));
        }

        public DataTable()
        {
            Name = String.Empty;
            ColumnFormat = new DataFormat();
            Rows = new List<DataRow>();
        }

        public bool Parse(
            string input,
            CompoundMatchItem includeFilter,
            CompoundMatchItem excludeFilter,
            bool isHasHeader,
            bool isAppend)
        {
            if (!isAppend || (Rows == null))
                Rows = new List<DataRow>();

            string dequotedInput = TextUtilities.Dequote(input);
            string[] lines = dequotedInput.Split(LanguageLookup.NewLine);
            int lineCount = lines.Length;
            int lineIndex = 0;
            char[] delimiter = new char[] { ColumnFormat.Delimiter };

            if (isHasHeader)
            {
                string headerRow = lines[lineIndex++];

                if (headerRow == null)
                    return true;

                string[] headerParts = headerRow.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                ColumnFormat.SetOrder(headerParts, false);
            }

            for (; lineIndex < lineCount; lineIndex++)
            {
                string[] parts;
                string line = lines[lineIndex];

                if (String.IsNullOrEmpty(line))
                    continue;

                DataRow row = new DataRow(ColumnFormat);

                parts = line.Split(delimiter);

                for (int i = 0; i < parts.Length; i++)
                {
                    DataColumn column = ColumnFormat.GetColumnIndexed(i);

                    if (column == null)
                        return false;

                    object value = column.ParseValue(parts[i]);

                    if (!row.SetCellIndexed(i, value))
                        return false;
                }

                if (includeFilter != null)
                {
                    if (!row.IsMatch(includeFilter))
                        continue;
                }

                if (excludeFilter != null)
                {
                    if (row.IsMatch(excludeFilter))
                        continue;
                }

                AddRow(row);
            }

            return true;
        }

        public bool Read(
            Stream stream,
            bool isHasHeader,
            bool isAppend)
        {
            return ReadFiltered(stream, null, null, isHasHeader, isAppend);
        }

        public bool ReadFiltered(
            Stream stream,
            CompoundMatchItem includeFilter,
            CompoundMatchItem excludeFilter,
            bool isHasHeader,
            bool isAppend)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string text = reader.ReadToEnd();
                return Parse(text, includeFilter, excludeFilter, isHasHeader, isAppend);
            }
        }

        public bool Write(Stream stream, bool isUTF8)
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                char delimiter = ColumnFormat.Delimiter;

                try
                {
                    int cellCount = ColumnFormat.ColumnCount();
                    string headerRow = ColumnFormat.GetHeadings();

                    if (isUTF8)
                        writer.Write(LanguageLookup.UTF8SignatureString);

                    writer.WriteLine(headerRow);

                    int count = Rows.Count();
                    int index;
                    StringBuilder sb = new StringBuilder();

                    for (index = 0; index < count; index++)
                    {
                        DataRow row = Rows[index];

                        int cellIndex;

                        for (cellIndex = 0; cellIndex < cellCount; cellIndex++)
                        {
                            DataColumn column = ColumnFormat.GetColumnIndexed(cellIndex);
                            string valueString = row.GetCellValueString(column.Name);

                            if (cellIndex != 0)
                                sb.Append(delimiter);

                            sb.Append(valueString);
                        }

                        writer.WriteLine(sb.ToString());
                        sb.Clear();
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        public bool Display(ApplicationData.DumpString displayFunction)
        {
            char delimiter = ColumnFormat.Delimiter;
            int cellCount = ColumnFormat.ColumnCount();
            string headerRow = ColumnFormat.GetHeadings();

            displayFunction(headerRow);

            int count = Rows.Count();
            int index;
            StringBuilder sb = new StringBuilder();

            for (index = 0; index < count; index++)
            {
                DataRow row = Rows[index];

                int cellIndex;

                for (cellIndex = 0; cellIndex < cellCount; cellIndex++)
                {
                    DataColumn column = ColumnFormat.GetColumnIndexed(cellIndex);
                    string valueString = row.GetCellValueString(column.Name);

                    if (cellIndex != 0)
                        sb.Append(delimiter);

                    sb.Append(valueString);
                }

                displayFunction(sb.ToString());
                sb.Clear();
            }

            return true;
        }

        public DataRow FindRowCompound(CompoundMatchItem compoundMatchItem)
        {
            foreach (DataRow dataRow in Rows)
            {
                if (dataRow.IsMatch(compoundMatchItem))
                    return dataRow;
            }

            return null;
        }

        public DataRow FindRowMatch(List<MatchItem> matchItems)
        {
            foreach (DataRow dataRow in Rows)
            {
                if (dataRow.IsMatch(matchItems))
                    return dataRow;
            }

            return null;
        }

        public DataRow FindRowWithColumnValue(string columnName, object value)
        {
            foreach (DataRow dataRow in Rows)
            {
                if (ObjectUtilities.MatchKeys(dataRow.GetCellValue(columnName), value))
                    return dataRow;
            }

            return null;
        }

        public DataRow FindRowWithColumnValueCaseInsensitive(string columnName, object value)
        {
            foreach (DataRow dataRow in Rows)
            {
                if (ObjectUtilities.MatchKeysCaseInsensitive(dataRow.GetCellValue(columnName), value))
                    return dataRow;
            }

            return null;
        }

        public List<DataRow> FindRowsCompound(CompoundMatchItem compoundMatchItem)
        {
            List<DataRow> dataRows = null;

            foreach (DataRow dataRow in Rows)
            {
                if (dataRow.IsMatch(compoundMatchItem))
                {
                    if (dataRows == null)
                        dataRows = new List<DataRow>() { dataRow };
                    else
                        dataRows.Add(dataRow);
                }
            }

            return dataRows;
        }

        public List<DataRow> FindRowsMatch(List<MatchItem> matchItems)
        {
            List<DataRow> dataRows = null;

            foreach (DataRow dataRow in Rows)
            {
                if (dataRow.IsMatch(matchItems))
                {
                    if (dataRows == null)
                        dataRows = new List<DataRow>() { dataRow };
                    else
                        dataRows.Add(dataRow);
                }
            }

            return dataRows;
        }

        public List<DataRow> FindRowsWithColumnValue(string columnName, object value)
        {
            List<DataRow> dataRows = null;

            foreach (DataRow dataRow in Rows)
            {
                if (ObjectUtilities.MatchKeys(dataRow.GetCellValue(columnName), value))
                {
                    if (dataRows == null)
                        dataRows = new List<DataRow>() { dataRow };
                    else
                        dataRows.Add(dataRow);
                }
            }

            return dataRows;
        }

        public List<DataRow> FindRowsWithColumnValueCaseInsensitive(string columnName, object value)
        {
            List<DataRow> dataRows = null;

            foreach (DataRow dataRow in Rows)
            {
                if (ObjectUtilities.MatchKeysCaseInsensitive(dataRow.GetCellValue(columnName), value))
                {
                    if (dataRows == null)
                        dataRows = new List<DataRow>() { dataRow };
                    else
                        dataRows.Add(dataRow);
                }
            }

            return dataRows;
        }

        public DataColumn GetColumnFormatColumn(string columnName)
        {
            return ColumnFormat.GetColumn(columnName);
        }

        public int GetRowIndex(DataRow row)
        {
            return Rows.IndexOf(row);
        }

        public DataRow GetRowIndexed(int index)
        {
            if ((index < 0) || (index >= Rows.Count()))
                return null;

            DataRow row = Rows[index];
            return row;
        }

        public void AddRow(DataRow row)
        {
            Rows.Add(row);
        }

        public void AddRows(List<DataRow> rows)
        {
            Rows.AddRange(rows);
        }

        public void InsertRow(int index, DataRow row)
        {
            Rows.Insert(index, row);
        }

        public void ReplaceRow(int index, DataRow row)
        {
            if ((index >= 0) && (index < RowCount()))
                Rows[index] = row;
            else
                throw new Exception("ReplaceRow.DeleteRows: Invalid index.");
        }

        public void DeleteRow(DataRow row)
        {
            if (Rows == null)
                return;

            Rows.Remove(row);
        }

        public bool DeleteRows(int index, int count)
        {
            if ((index >= 0) && (index < RowCount()) && (count >= 0) && (index + count <= RowCount()))
                Rows.RemoveRange(index, count);
            else
                throw new Exception("DataTable.DeleteRows: Invalid arguments.");

            return true;
        }

        public bool DeleteMatchedRows(List<MatchItem> matchItems)
        {
            for (int index = RowCount() - 1; index >= 0; index--)
            {
                DataRow dataRow = Rows[index];

                if (dataRow.IsMatch(matchItems))
                    Rows.RemoveAt(index);
            }

            return true;
        }

        public bool DeleteMatchedRows(CompoundMatchItem compoundMatchItem)
        {
            for (int index = RowCount() - 1; index >= 0; index--)
            {
                DataRow dataRow = Rows[index];

                if (dataRow.IsMatch(compoundMatchItem))
                    Rows.RemoveAt(index);
            }

            return true;
        }

        public bool DeleteUnmatchedRows(List<MatchItem> matchItems)
        {
            for (int index = RowCount() - 1; index >= 0; index--)
            {
                DataRow dataRow = Rows[index];

                if (!dataRow.IsMatch(matchItems))
                    Rows.RemoveAt(index);
            }

            return true;
        }

        public bool DeleteUnmatchedRows(CompoundMatchItem compoundMatchItem)
        {
            for (int index = RowCount() - 1; index >= 0; index--)
            {
                DataRow dataRow = Rows[index];

                if (!dataRow.IsMatch(compoundMatchItem))
                    Rows.RemoveAt(index);
            }

            return true;
        }

        public int RowCount()
        {
            return Rows.Count();
        }

        public void SortRows(List<string> sortColumns)
        {
            if ((sortColumns == null) || (sortColumns.Count() == 0))
                return;

            List<int> sortIndexes = new List<int>();

            foreach (string columnKey in sortColumns)
            {
                int index;

                if (columnKey.StartsWith("-"))
                {
                    string columnName = columnKey.Substring(1);
                    DataColumn column = ColumnFormat.GetColumnCheck(columnName);

                    index = column.Index;

                    if (index == -1)
                        throw new Exception("SortRows: Invalid index for: " + columnName);

                    sortIndexes.Add(-(index + 1));
                }
                else
                {
                    DataColumn column = ColumnFormat.GetColumnCheck(columnKey);

                    index = column.Index;

                    if (index == -1)
                        throw new Exception("SortRows: Invalid index for: " + columnKey);

                    sortIndexes.Add(index + 1);
                }
            }

            DataRowComparer comparer = new DataRowComparer(sortIndexes);

            Rows.Sort(comparer);
        }

        public string GetIndexedRowDisplayString(int rowIndex)
        {
            DataRow dataRow = GetRowIndexed(rowIndex);
            return GetRowDisplayString(dataRow);
        }

        public string GetRowDisplayString(DataRow dataRow)
        {
            StringBuilder sb = new StringBuilder();

            if (dataRow.CellCount() != 0)
            {
                int columnIndex = 0;

                foreach (DataColumn column in ColumnFormat.Columns)
                {
                    if (sb.Length != 0)
                        sb.Append("|");

                    sb.Append(column.Name);
                    sb.Append("=");
                    sb.Append(dataRow.GetCellValueStringIndexed(column.Index));
                    columnIndex++;
                }
            }

            return sb.ToString();
        }
    }
}
