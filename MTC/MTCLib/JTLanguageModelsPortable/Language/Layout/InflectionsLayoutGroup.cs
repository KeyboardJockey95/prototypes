using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;

namespace JTLanguageModelsPortable.Language
{
    public class InflectionsLayoutGroup : BaseObjectKeyed
    {
        protected Designator _Designation;
        protected List<InflectionsLayoutHeading> _Headings;
        protected List<InflectionsLayoutGroup> _SubGroups;
        protected List<InflectionsLayoutColumn> _Columns;
        protected List<InflectionsLayoutRow> _Rows;

        public InflectionsLayoutGroup(string key, Designator designation) :
            base(key)
        {
            ClearInflectionsLayoutGroup();
            _Designation = designation;
        }

        public InflectionsLayoutGroup(XElement element)
        {
            ClearInflectionsLayoutGroup();
            OnElement(element);
        }

        public InflectionsLayoutGroup(InflectionsLayoutGroup other) : base(other)
        {
            CopyInflectionsLayoutGroup(other);
        }

        public InflectionsLayoutGroup()
        {
            ClearInflectionsLayoutGroup();
        }

        public void ClearInflectionsLayoutGroup()
        {
            _Designation = new Designator(String.Empty, new List<Classifier>());
            _Headings = null;
            _SubGroups = null;
            _Rows = null;
        }

        public void CopyInflectionsLayoutGroup(InflectionsLayoutGroup other)
        {
            _Designation = new Designator(other.Designation);
            _Headings = other.CloneHeadings();
            _SubGroups = other.CloneSubGroups();
            _Rows = other.CloneRows();
        }

        public Designator Designation
        {
            get
            {
                return _Designation;
            }
            set
            {
                _Designation = value;
            }
        }

        public string Label
        {
            get
            {
                return _Designation.Label;
            }
            set
            {
                _Designation.Label = value;
            }
        }

        public List<Classifier> Classifications
        {
            get
            {
                return _Designation.Classifications;
            }
            set
            {
                _Designation.Classifications = value;
            }
        }

        public int ClassificationCount()
        {
            if (Classifications == null)
                return 0;
            return Classifications.Count();
        }

        public Classifier GetClassificationIndexed(int index)
        {
            if (Classifications == null)
                return null;

            if ((index >= 0) && (index < Classifications.Count()))
                return Classifications[index];

            return null;
        }

        public void AppendClassification(Classifier classifier)
        {
            if (Classifications != null)
                Classifications.Add(classifier);
            else
                Classifications = new List<Classifier>() { classifier };
        }

        public List<Classifier> CloneClassifications()
        {
            if (Classifications == null)
                return null;

            List<Classifier> classifications = new List<Classifier>();

            foreach (Classifier classifier in Classifications)
                classifications.Add(new Classifier(classifier));

            return classifications;
        }

        public List<InflectionsLayoutHeading> Headings
        {
            get
            {
                return _Headings;
            }
            set
            {
                if (value != _Headings)
                {
                    _Headings = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int HeadingsCount()
        {
            if (_Headings == null)
                return 0;

            int count = _Headings.Count();

            return count;
        }

        public InflectionsLayoutHeading GetHeadingIndexed(int index)
        {
            if (_Headings == null)
                return null;

            if ((index < 0) || (index >= _Headings.Count()))
                return null;

            InflectionsLayoutHeading heading = _Headings[index];

            return heading;
        }

        public void AppendHeading(InflectionsLayoutHeading heading)
        {
            if (_Headings == null)
                _Headings = new List<InflectionsLayoutHeading>() { heading };
            else
                _Headings.Add(heading);
        }

        public List<InflectionsLayoutHeading> CloneHeadings()
        {
            if (_Headings == null)
                return null;

            List<InflectionsLayoutHeading> headings = new List<InflectionsLayoutHeading>(_Headings.Count());

            foreach (InflectionsLayoutHeading heading in _Headings)
                headings.Add(new InflectionsLayoutHeading(heading));

            return headings;
        }

        public string GroupKey(InflectionsLayoutGroup parentGroup)
        {
            int count = ClassificationCount();
            int index;
            string parentKey = (parentGroup != null ? parentGroup.GroupKey(null) : String.Empty);
            string groupKey = parentKey;

            if (count == 0)
                return Name;

            for (index = 0; index < count; index++)
            {
                Classifier classifier = GetClassificationIndexed(index);

                if (groupKey == classifier.Text)
                    continue;

                if (groupKey != String.Empty)
                    groupKey += " ";

                groupKey += classifier.Text;
            }

            return groupKey;
        }

        public List<InflectionsLayoutGroup> SubGroups
        {
            get
            {
                return _SubGroups;
            }
            set
            {
                if (value != _SubGroups)
                {
                    _SubGroups = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int SubGroupCount()
        {
            if (_SubGroups != null)
                return _SubGroups.Count();

            return 0;
        }

        public InflectionsLayoutGroup GetSubGroup(string name)
        {
            if (_SubGroups == null)
                return null;

            InflectionsLayoutGroup subGroup = _SubGroups.FirstOrDefault(
                x => (x.Name == name) || (x.Label == name));

            return subGroup;
        }

        public InflectionsLayoutGroup GetSubGroupIndexed(int index)
        {
            if (_SubGroups == null)
                return null;

            if ((index >= 0) && (index < _SubGroups.Count()))
                return _SubGroups[index];

            return null;
        }

        public void AppendSubGroup(InflectionsLayoutGroup subGroup)
        {
            if (_SubGroups == null)
                _SubGroups = new List<InflectionsLayoutGroup>() { subGroup };
            else
                _SubGroups.Add(subGroup);
        }

        public void DeleteSubGroup(string name)
        {
            if (_SubGroups == null)
                return;

            InflectionsLayoutGroup subGroup = _SubGroups.FirstOrDefault(
                x => (x.Name == name) || (x.Label == name));

            if (subGroup != null)
                DeleteSubGroup(subGroup);
        }

        public void DeleteSubGroup(InflectionsLayoutGroup subGroup)
        {
            if (_SubGroups == null)
                return;

            if (subGroup != null)
                _SubGroups.Remove(subGroup);
        }

        public void DeleteSubGroupIndexed(int index)
        {
            if (_SubGroups == null)
                return;

            if ((index >= 0) && (index < _SubGroups.Count()))
                return;

            _SubGroups.RemoveAt(index);
        }

        public List<InflectionsLayoutGroup> CloneSubGroups()
        {
            if (_SubGroups == null)
                return null;

            List<InflectionsLayoutGroup> subGroups = new List<InflectionsLayoutGroup>(_SubGroups.Count());

            foreach (InflectionsLayoutGroup subGroup in _SubGroups)
                subGroups.Add(new InflectionsLayoutGroup(subGroup));

            return subGroups;
        }

        public List<InflectionsLayoutColumn> Columns
        {
            get
            {
                return _Columns;
            }
            set
            {
                if (value != _Columns)
                {
                    _Columns = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int ColumnCount()
        {
            if (_Columns == null)
                return 0;

            return _Columns.Count();
        }

        public InflectionsLayoutColumn GetColumnIndexed(int index)
        {
            if (_Columns == null)
                return null;

            if ((index >= 0) && (index < _Columns.Count()))
                return _Columns[index];

            return null;
        }

        public void AddColumn(InflectionsLayoutColumn column)
        {
            if (_Columns == null)
                _Columns = new List<InflectionsLayoutColumn>() { column };
            else
                _Columns.Add(column);
        }

        public void InsertColumn(int index, InflectionsLayoutColumn column)
        {
            if (_Columns == null)
                _Columns = new List<InflectionsLayoutColumn>() { column };
            else
                _Columns.Insert(index, column);
        }

        public List<InflectionsLayoutColumn> CloneColumns()
        {
            if (_Columns == null)
                return null;

            List<InflectionsLayoutColumn> columns = new List<InflectionsLayoutColumn>(_Columns.Count());

            foreach (InflectionsLayoutColumn column in _Columns)
                columns.Add(new InflectionsLayoutColumn(column));

            return columns;
        }

        public List<InflectionsLayoutRow> Rows
        {
            get
            {
                return _Rows;
            }
            set
            {
                if (value != _Rows)
                {
                    _Rows = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int RowCount()
        {
            if (_Rows == null)
                return 0;

            int count = _Rows.Count();

            return count;
        }

        public InflectionsLayoutRow GetRow(string name)
        {
            if (_Rows == null)
                return null;

            InflectionsLayoutRow row = _Rows.FirstOrDefault(x => x.Name == name);

            return row;
        }

        public InflectionsLayoutRow GetRowIndexed(int index)
        {
            if (_Rows == null)
                return null;

            if ((index < 0) || (index >= _Rows.Count()))
                return null;

            InflectionsLayoutRow row = _Rows[index];

            return row;
        }

        public void AddRow(InflectionsLayoutRow row)
        {
            if (_Rows == null)
                _Rows = new List<InflectionsLayoutRow>() { row };
            else
                _Rows.Add(row);
        }

        public void InsertRow(int index, InflectionsLayoutRow row)
        {
            if (_Rows == null)
                _Rows = new List<InflectionsLayoutRow>() { row };
            else
                _Rows.Insert(index, row);
        }

        public List<InflectionsLayoutRow> CloneRows()
        {
            if (_Rows == null)
                return null;

            List<InflectionsLayoutRow> rows = new List<InflectionsLayoutRow>(_Rows.Count());

            foreach (InflectionsLayoutRow row in _Rows)
                rows.Add(new InflectionsLayoutRow(row));

            return rows;
        }

        public void GetDataDimensions(out int dataRowCount, out int dataColumnCount)
        {
            int rowCount = RowCount();
            int rowIndex;

            dataRowCount = 0;
            dataColumnCount = 0;

            if (ColumnCount() != 0)
            {
                dataRowCount = RowCount();
                dataColumnCount = ColumnCount();
            }
            else
            {
                for (rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    InflectionsLayoutRow row = GetRowIndexed(rowIndex);

                    if (row.Name == "ColumnHeadings")
                        continue;

                    dataRowCount++;

                    int columnCount = row.DataCellCount();

                    if (columnCount > dataColumnCount)
                        dataColumnCount = columnCount;
                }
            }
        }

        public bool IsInflectionDataEmpty(
            List<Inflection>[,] inflectionData,
            InflectionsLayoutGroup parentGroup)
        {
            int maxRows, maxColumns;

            if ((RowCount() == 0) && (parentGroup != null))
                parentGroup.GetDataDimensions(out maxRows, out maxColumns);
            else
                GetDataDimensions(out maxRows, out maxColumns);

            for (int rowIndex = 0; rowIndex < maxRows; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < maxColumns; columnIndex++)
                {
                    if (inflectionData[rowIndex, columnIndex] != null)
                        return false;
                }
            }

            return true;
        }

        public override void Display(string label, DisplayDetail detail, int indent)
        {
            switch (detail)
            {
                case DisplayDetail.Lite:
                case DisplayDetail.Full:
                    if (Designation != null)
                        DisplayLabelArgument(label, Designation.Label, indent);
                    else
                        DisplayLabel(label, indent);
                    if (_Headings != null)
                    {
                        foreach (InflectionsLayoutHeading heading in _Headings)
                            DisplayFieldObject("Heading", heading, indent + 1);
                    }
                    else
                        DisplayMessage("(no headings)", indent + 1);
                    if (_SubGroups != null)
                    {
                        foreach (InflectionsLayoutGroup group in _SubGroups)
                            DisplayFieldObject("SubGroup", group, indent + 1);
                    }
                    else
                        DisplayMessage("(no subgroups)", indent + 1);
                    if (_Columns != null)
                    {
                        foreach (InflectionsLayoutColumn column in _Columns)
                            DisplayFieldObject("Column", column, indent + 1);
                    }
                    else
                        DisplayMessage("(no columns)", indent + 1);
                    if (_Rows != null)
                    {
                        foreach (InflectionsLayoutRow row in _Rows)
                            DisplayFieldObject("Row", row, indent + 1);
                    }
                    else
                        DisplayMessage("(no subgroups)", indent + 1);
                    break;
                case DisplayDetail.Diagnostic:
                case DisplayDetail.Xml:
                    base.Display(label, detail, indent);
                    break;
                default:
                    break;
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if (Key != null)
                element.Add(new XAttribute("Name", KeyString));

            if (!String.IsNullOrEmpty(Label))
                element.Add(new XAttribute("Label", Label));

            if ((_Headings != null) && (_Headings.Count() != 0))
            {
                foreach (InflectionsLayoutHeading heading in _Headings)
                    element.Add(heading.GetElement("Heading"));
            }

            if (Classifications != null)
            {
                foreach (Classifier classification in Classifications)
                {
                    XElement classifierElement = classification.GetElement("Classifier");
                    element.Add(classifierElement);
                }
            }

            if (_SubGroups != null)
            {
                string label = (name == "Major" ? "Minor" : "SubGroup");

                foreach (InflectionsLayoutGroup subGroup in _SubGroups)
                {
                    XElement subGroupElement = subGroup.GetElement(label);
                    element.Add(subGroupElement);
                }
            }

            if ((_Columns != null) && (_Columns.Count() != 0))
            {
                foreach (InflectionsLayoutColumn column in _Columns)
                    element.Add(column.GetElement("Column"));
            }

            if ((_Rows != null) && (_Rows.Count() != 0))
            {
                foreach (InflectionsLayoutRow row in _Rows)
                    element.Add(row.GetElement("Row"));
            }

            return element;
        }

        public override void OnElement(XElement element)
        {
            base.OnElement(element);

            if (Key == null)
            {
                if (_Designation != null)
                    Key = _Designation.ComposeLabel();
            }
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Name":
                    Key = attributeValue;
                    break;
                case "Label":
                    _Designation.Label = attributeValue;
                    break;
                default:
                    return OnUnknownAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Heading":
                    {
                        InflectionsLayoutHeading heading = new InflectionsLayoutHeading(childElement);
                        AppendHeading(heading);
                    }
                    break;
                case "Classifier":
                    {
                        Classifier classification = new Classifier(childElement);
                        AppendClassification(classification);
                    }
                    break;
                case "SubGroup":
                case "Minor":
                    {
                        InflectionsLayoutGroup subGroup = new InflectionsLayoutGroup(childElement);
                        AppendSubGroup(subGroup);
                    }
                    break;
                case "Column":
                    {
                        InflectionsLayoutColumn column = new InflectionsLayoutColumn(childElement);
                        AddColumn(column);
                    }
                    break;
                case "Row":
                    {
                        InflectionsLayoutRow row = new InflectionsLayoutRow(childElement);
                        AddRow(row);
                    }
                    break;
                default:
                    throw new Exception("Unexpected child element in InflectionLayoutGroup: " + childElement.Name.LocalName);
            }

            return true;
        }
    }
}
