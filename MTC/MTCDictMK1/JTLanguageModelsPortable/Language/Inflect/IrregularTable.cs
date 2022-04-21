using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    public class IrregularTable : BaseObjectKeyed
    {
        protected List<SpecialAction> _IrregularColumns;
        protected List<List<string>> _TermTable;

        public IrregularTable(
            List<SpecialAction> irregularColumns,
            List<List<string>> termTable)
        {
            _IrregularColumns = irregularColumns;
            _TermTable = termTable;
        }

        public IrregularTable(XElement element)
        {
            ClearIrregularTable();
            OnElement(element);
        }

        public IrregularTable(IrregularTable other) : base(other)
        {
            CopyIrregularTable(other);
        }

        public IrregularTable()
        {
            ClearIrregularTable();
        }

        public void ClearIrregularTable()
        {
            _IrregularColumns = null;
            _TermTable = null;
        }

        public void CopyIrregularTable(IrregularTable other)
        {
            _IrregularColumns = other.IrregularColumns;
            _TermTable = other.TermTable;
        }

        public string Label
        {
            get
            {
                return KeyString;
            }
            set
            {
                Key = value;
            }
        }

        public List<SpecialAction> IrregularColumns
        {
            get
            {
                return _IrregularColumns;
            }
            set
            {
                if (value != _IrregularColumns)
                {
                    _IrregularColumns = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int IrregularColumnsCount()
        {
            if (_IrregularColumns != null)
                return _IrregularColumns.Count();
            return 0;
        }

        public List<List<string>> TermTable
        {
            get
            {
                return _TermTable;
            }
            set
            {
                if (value != _TermTable)
                {
                    _TermTable = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<SemiRegular> CreateIrregulars()
        {
            if (_TermTable == null)
                return null;

            int columnCount = IrregularColumnsCount();
            int columnIndex;
            int rowIndex = 0;
            string stem = null;
            List<SemiRegular> irregulars = new List<SemiRegular>();

            foreach (List<string> columns in _TermTable)
            {
                if (columns.Count() != columnCount)
                    throw new Exception("TermTable column count should have " + columnCount.ToString() + " terms in row " + rowIndex.ToString() + ".");

                List<SpecialAction> actions = new List<SpecialAction>();

                for (columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    string term = columns[columnIndex];
                    SpecialAction irregularColumn = _IrregularColumns[columnIndex];

                    if (irregularColumn.Type == "None")
                        stem = term;
                    else
                    {
                        SpecialAction action = new SpecialAction(irregularColumn);
                        action.Stem = new LiteralString(term);
                        actions.Add(action);
                    }
                }

                SemiRegular irregular = new SemiRegular(
                    stem,
                    new List<LiteralString> { new LiteralString(stem) },
                    actions,
                    false,
                    false,
                    null);

                irregulars.Add(irregular);
                rowIndex++;
            }

            return irregulars;
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if (!String.IsNullOrEmpty(Label))
                element.Add(new XAttribute("Label", Label));

            if (_IrregularColumns != null)
            {
                foreach (SpecialAction irregularColumn in _IrregularColumns)
                {
                    XElement irregularColumnElement = irregularColumn.GetElement("IrregularColumn");
                    element.Add(irregularColumnElement);
                }
            }

            if (_TermTable != null)
            {
                StringBuilder sb = new StringBuilder();
                bool first = true;

                foreach (List<string> row in _TermTable)
                {
                    if (!first)
                        sb.Append(",");

                    sb.Append("      " + ObjectUtilities.GetStringFromStringList(row));
                }
            }

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Label":
                    Label = attributeValue;
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
                case "Label":
                    Label = childElement.Value.Trim();
                    break;
                case "IrregularColumn":
                    if (_IrregularColumns == null)
                        _IrregularColumns = new List<SpecialAction> { new SpecialAction(childElement) };
                    else
                        _IrregularColumns.Add(new SpecialAction(childElement));
                    break;
                case "TermTable":
                    {
                        _TermTable = new List<List<string>>();
                        string value = childElement.Value.Trim();
                        string[] rows = value.Split(LanguageLookup.NewLine, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string row in rows)
                        {
                            List<string> columns = ObjectUtilities.GetStringListFromString(row.Trim());
                            _TermTable.Add(columns);
                        }
                    }
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
