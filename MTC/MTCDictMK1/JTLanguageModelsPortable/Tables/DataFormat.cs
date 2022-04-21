using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Tables
{
    public class DataFormat
    {
        public string Name { get; set; }
        public char Delimiter { get; set; }
        public List<DataColumn> Columns { get; set; }
        public Dictionary<string, DataColumn> ColumnDictionary { get; set; }

        public DataFormat(string name, List<DataColumn> columns, char delimiter)
        {
            Name = name;
            Delimiter = delimiter;
            if (columns != null)
                Columns = new List<DataColumn>(columns);
            else
                Columns = new List<DataColumn>();
            Initialize();
        }

        public DataFormat(DataFormat other)
        {
            Name = other.Name;
            Delimiter = other.Delimiter;
            if (other.Columns != null)
                Columns = new List<DataColumn>(other.Columns);
            else
                Columns = new List<DataColumn>();
            Initialize();
        }

        public DataFormat()
        {
            Name = String.Empty;
            Delimiter = '\t';
            Columns = new List<DataColumn>();
            Initialize();
        }

        private void Initialize()
        {
            int count = Columns.Count();
            int index;

            for (index = 0; index < count; index++)
            {
                DataColumn column = Columns[index];
                column.Index = index;
            }

            ColumnDictionary = new Dictionary<string, DataColumn>();

            foreach (DataColumn column in Columns)
                ColumnDictionary.Add(column.Name, column);
        }

        public int ColumnCount()
        {
            return Columns.Count();
        }

        public DataColumn GetColumn(string name)
        {
            DataColumn column;

            if (ColumnDictionary.TryGetValue(name, out column))
                return column;

            return null;
        }

        public DataColumn GetColumnCheck(string name)
        {
            DataColumn column;

            if (ColumnDictionary.TryGetValue(name, out column))
                return column;

            throw new Exception("Unknown column name: " + name);
        }

        public DataColumn GetColumnIndexed(int index)
        {
            if ((index >= 0) && (index < Columns.Count()))
                return Columns[index];

            return null;
        }

        public DataColumn GetColumnIndexedCheck(int index)
        {
            if ((index >= 0) && (index < Columns.Count()))
                return Columns[index];

            throw new Exception("Column index out of range: " + index.ToString());
        }

        public int GetColumnIndex(string name)
        {
            DataColumn column = GetColumn(name);

            if (column == null)
                return -1;

            return column.Index;
        }

        public string GetHeadings()
        {
            StringBuilder sb = new StringBuilder();

            foreach (DataColumn column in Columns)
            {
                if (sb.Length != 0)
                    sb.Append(Delimiter);

                sb.Append(column.Name);
            }

            return sb.ToString();
        }

        public void SetOrder(
            string[] order,
            bool trim)
        {
            List<DataColumn> oldList = new List<DataColumn>(Columns);
            Columns.Clear();

            foreach (string name in order)
            {
                DataColumn column = oldList.FirstOrDefault(x => x.Name == name);

                if (column != null)
                {
                    Columns.Add(column);
                    oldList.Remove(column);
                }
                else
                    Columns.Add(new DataColumn(name));
            }

            if (!trim)
                Columns.AddRange(oldList);

            Initialize();
        }
    }
}
