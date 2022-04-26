using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public class InflectionsLayoutRow : BaseObjectKeyed
    {
        protected List<InflectionsLayoutHeading> _Headings;
        protected Designator _Designation;
        public List<InflectionsLayoutColumn> _Columns;
        protected List<InflectionsLayoutCell> _Cells;

        public InflectionsLayoutRow(string key) : base(key)
        {
            ClearInflectionsLayoutRow();
        }

        public InflectionsLayoutRow(
            List<InflectionsLayoutHeading> headings,
            Designator designation) : base(designation.Label)
        {
            _Headings = headings;
            _Designation = designation;
            _Cells = null;
        }

        public InflectionsLayoutRow(Designator designation) : base(designation.Label)
        {
            _Headings = null;
            _Designation = designation;
            _Columns = new List<InflectionsLayoutColumn>();
            _Cells = null;
        }

        public InflectionsLayoutRow(XElement element)
        {
            ClearInflectionsLayoutRow();
            OnElement(element);
        }

        public InflectionsLayoutRow(InflectionsLayoutRow other)
        {
            CopyInflectionsLayoutRow(other);
        }

        public InflectionsLayoutRow()
        {
            ClearInflectionsLayoutRow();
        }

        public void ClearInflectionsLayoutRow()
        {
            _Headings = null;
            _Designation = new Designator(null, new List<Classifier>());
            _Columns = new List<InflectionsLayoutColumn>();
            _Cells = null;
        }

        public void CopyInflectionsLayoutRow(InflectionsLayoutRow other)
        {
            _Headings = other.CloneHeadings();
            _Designation = other.CloneDesignation();
            _Columns = other.CloneColumns();
            _Cells = other.CloneCells();
        }

        public List<InflectionsLayoutHeading> Headings
        {
            get
            {
                return _Headings;
            }
            set
            {
                _Headings = value;
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

        public void DeleteHeadingIndexed(int index)
        {
            if (_Headings == null)
                return;

            if ((index < 0) || (index >= _Headings.Count()))
                return;

            _Headings.RemoveAt(index);
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

        public Designator CloneDesignation()
        {
            return new Designator(_Designation);
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
            return _Designation.ClassificationCount();
        }

        public Classifier GetClassification(string name)
        {
            return _Designation.GetClassification(name);
        }

        public Classifier GetClassificationIndexed(int index)
        {
            return _Designation.GetClassificationIndexed(index);
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
            if ((_Designation == null) || (_Designation.ClassificationCount() == 0))
                return null;

            List<Classifier> classifications = new List<Classifier>();

            foreach (Classifier classifier in _Designation.Classifications)
                classifications.Add(new Classifier(classifier));

            return classifications;
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

        public List<InflectionsLayoutCell> Cells
        {
            get
            {
                return _Cells;
            }
            set
            {
                if (value != _Cells)
                {
                    _Cells = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int CellCount()
        {
            if (_Cells == null)
                return 0;
            return _Cells.Count();
        }

        public int DataCellCount()
        {
            if (_Cells == null)
                return 0;

            int cellCount = _Cells.Count();
            int cellIndex;
            int dataCellCount = 0;

            for (cellIndex = 0; cellIndex < cellCount; cellIndex++)
            {
                InflectionsLayoutCell cell = _Cells[cellIndex];

                if ((cell == null) || (cell.HeadingsCount() == 0))
                    dataCellCount++;
            }

            return dataCellCount;
        }

        public InflectionsLayoutCell GetCellIndexed(int index)
        {
            if (_Cells == null)
                return null;

            if ((index >= 0) && (index < _Cells.Count()))
                return _Cells[index];

            return null;
        }

        public void InsertCell(int index, InflectionsLayoutCell cell)
        {
            if (_Cells == null)
                _Cells = new List<InflectionsLayoutCell>() { cell };
            else
                _Cells.Insert(index, cell);
        }

        public void AppendCell(InflectionsLayoutCell cell)
        {
            if (_Cells == null)
                _Cells = new List<InflectionsLayoutCell>() { cell };
            else
                _Cells.Add(cell);
        }

        public List<InflectionsLayoutCell> CloneCells()
        {
            if (_Cells == null)
                return null;

            List<InflectionsLayoutCell> Cells = new List<InflectionsLayoutCell>(_Cells.Count());

            foreach (InflectionsLayoutCell cell in _Cells)
                Cells.Add(new InflectionsLayoutCell(cell));

            return Cells;
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
                    if (Columns != null)
                    {
                        foreach (InflectionsLayoutColumn column in Columns)
                            DisplayFieldObject("Column", column, indent + 1);
                    }
                    else
                        DisplayMessage("(no columns)", indent + 1);
                    if (Cells != null)
                    {
                        foreach (InflectionsLayoutCell cell in Cells)
                            DisplayFieldObject("Cell", cell, indent + 1);
                    }
                    else
                        DisplayMessage("(no cells)", indent + 1);
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

            if (_Cells != null)
            {
                foreach (InflectionsLayoutCell cell in _Cells)
                {
                    XElement cellElement = cell.GetElement("Cell");
                    element.Add(cellElement);
                }
            }

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Name":
                    Key = attributeValue;
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
                case "CellFormat":
                    break;
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
                case "Cell":
                    {
                        InflectionsLayoutCell cell = new InflectionsLayoutCell(childElement);
                        AppendCell(cell);
                    }
                    break;
                default:
                    throw new Exception("Unexpected child element in InflectionLayoutCell: " + childElement.Name.LocalName);
            }

            return true;
        }
    }
}
