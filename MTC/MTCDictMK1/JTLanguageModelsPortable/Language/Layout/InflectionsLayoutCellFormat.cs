using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    public class InflectionsLayoutCellFormat : BaseObject
    {
        protected List<InflectionsLayoutHeading> _Headings;
        protected List<Classifier> _Classifications;
        protected int _Span;

        public InflectionsLayoutCellFormat(XElement element)
        {
            ClearInflectionsLayoutCellFormat();
            OnElement(element);
        }

        public InflectionsLayoutCellFormat(InflectionsLayoutCellFormat other)
        {
            CopyInflectionsLayoutCellFormat(other);
        }

        public InflectionsLayoutCellFormat()
        {
            ClearInflectionsLayoutCellFormat();
        }

        public void ClearInflectionsLayoutCellFormat()
        {
            _Headings = null;
            _Classifications = null;
            _Span = 0;
        }

        public void CopyInflectionsLayoutCellFormat(InflectionsLayoutCellFormat other)
        {
            _Headings = other.CloneHeadings();
            _Classifications = other.CloneClassifications();
            _Span = other.Span;
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

        public List<InflectionsLayoutHeading> CloneHeadings()
        {
            if (_Headings == null)
                return null;

            List<InflectionsLayoutHeading> headings = new List<InflectionsLayoutHeading>(_Headings.Count());

            foreach (InflectionsLayoutHeading heading in _Headings)
                headings.Add(new InflectionsLayoutHeading(heading));

            return headings;
        }

        public List<Classifier> Classifications
        {
            get
            {
                return _Classifications;
            }
            set
            {
                _Classifications = value;
            }
        }

        public int ClassificationCount()
        {
            if (_Classifications == null)
                return 0;
            return _Classifications.Count();
        }

        public Classifier GetClassificationIndexed(int index)
        {
            if (_Classifications == null)
                return null;

            if ((index >= 0) && (index < _Classifications.Count()))
                return _Classifications[index];

            return null;
        }

        public void AppendClassification(Classifier classifier)
        {
            if (_Classifications != null)
                _Classifications.Add(classifier);
            else
                _Classifications = new List<Classifier>() { classifier };
        }

        public List<Classifier> CloneClassifications()
        {
            if (_Classifications == null)
                return null;

            List<Classifier> classifications = new List<Classifier>();

            foreach (Classifier classifier in _Classifications)
                classifications.Add(new Classifier(classifier));

            return classifications;
        }

        public int Span
        {
            get
            {
                return _Span;
            }
            set
            {
                _Span = value;
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if (_Span > 1)
                element.Add(new XAttribute("Span", _Span));

            if ((_Headings != null) && (_Headings.Count() != 0))
            {
                foreach (InflectionsLayoutHeading heading in _Headings)
                    element.Add(heading.GetElement("Heading"));
            }

            if (_Classifications != null)
            {
                foreach (Classifier classification in _Classifications)
                {
                    XElement classifierElement = classification.GetElement("Classifier");
                    element.Add(classifierElement);
                }
            }

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Span":
                    _Span = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
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
                case "Span":
                    _Span = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), 0);
                    break;
                default:
                    throw new Exception("Unexpected child element in InflectionLayoutGroup: " + childElement.Name.LocalName);
            }

            return true;
        }
    }
}
