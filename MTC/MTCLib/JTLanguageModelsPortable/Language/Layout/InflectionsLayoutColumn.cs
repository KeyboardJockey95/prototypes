using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public class InflectionsLayoutColumn : BaseObjectKeyed
    {
        protected List<InflectionsLayoutHeading> _Headings;
        protected Designator _Designation;
        protected InflectionsLayoutCell _Cell;

        public InflectionsLayoutColumn(
            List<InflectionsLayoutHeading> headings,
            Designator designation) : base(designation.Label)
        {
            _Headings = headings;
            _Designation = designation;
            _Cell = null;
        }

        public InflectionsLayoutColumn(Designator designation) : base(designation.Label)
        {
            _Headings = null;
            _Designation = designation;
            _Cell = null;
        }

        public InflectionsLayoutColumn(InflectionsLayoutColumn other) : base(other)
        {
            _Headings = other.CloneHeadings();
            _Designation = other.CloneDesignation();
            _Cell = other.CloneCell();
        }

        public InflectionsLayoutColumn(XElement element)
        {
            ClearInflectionsLayoutColumn();
            OnElement(element);
        }

        public InflectionsLayoutColumn()
        {
            ClearInflectionsLayoutColumn();
        }

        public void ClearInflectionsLayoutColumn()
        {
            _Headings = null;
            _Designation = new Designator(null, new List<Classifier>());
            _Cell = null;
        }

        public void CopyInflectionsLayoutColumn(InflectionsLayoutCell other)
        {
            _Headings = other.CloneHeadings();
            _Designation = CloneDesignation();
            _Cell = CloneCell();
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
            if (Classifications == null)
                return 0;
            return Classifications.Count();
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
            if (Classifications == null)
                return null;

            List<Classifier> classifications = new List<Classifier>();

            foreach (Classifier classifier in Classifications)
                classifications.Add(new Classifier(classifier));

            return classifications;
        }

        public InflectionsLayoutCell Cell
        {
            get
            {
                return _Cell;
            }
            set
            {
                _Cell = value;
            }
        }

        public InflectionsLayoutCell CloneCell()
        {
            if (_Cell == null)
                return null;

            return new InflectionsLayoutCell(_Cell);
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
                            heading.Display(null, detail, indent + 1);
                    }
                    DisplayFieldObject("Cell", Cell, indent + 1);
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

            if (!String.IsNullOrEmpty(Name))
                element.Add(new XAttribute("Name", Name));

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

            if (_Cell != null)
                element.Add(_Cell.GetElement("Cell"));

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
                    _Cell = new Language.InflectionsLayoutCell(childElement);
                    break;
                default:
                    throw new Exception("Unexpected child element in InflectionLayoutGroup: " + childElement.Name.LocalName);
            }

            return true;
        }
    }
}
