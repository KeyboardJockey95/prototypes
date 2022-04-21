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
    public class InflectionsLayoutHeading : BaseObject
    {
        protected string _Text;
        protected int _RowSpan;
        protected int _ColumnSpan;

        public InflectionsLayoutHeading(string text, int rowSpan, int columnSpan)
        {
            _Text = text;
            _RowSpan = rowSpan;
            _ColumnSpan = columnSpan;
        }

        public InflectionsLayoutHeading(XElement element)
        {
            ClearInflectionsLayoutHeading();
            OnElement(element);
        }

        public InflectionsLayoutHeading(InflectionsLayoutHeading other)
        {
            CopyInflectionsLayoutHeading(other);
        }

        public InflectionsLayoutHeading()
        {
            ClearInflectionsLayoutHeading();
        }

        public void ClearInflectionsLayoutHeading()
        {
            _Text = null;
            _RowSpan = 0;
            _ColumnSpan = 0;
        }

        public void CopyInflectionsLayoutHeading(InflectionsLayoutHeading other)
        {
            _Text = other.Text;
            _RowSpan = other.RowSpan;
            _ColumnSpan = other.ColumnSpan;
        }

        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                _Text = value;
            }
        }

        public int RowSpan
        {
            get
            {
                return _RowSpan;
            }
            set
            {
                _RowSpan = value;
            }
        }

        public int ColumnSpan
        {
            get
            {
                return _ColumnSpan;
            }
            set
            {
                _ColumnSpan = value;
            }
        }

        public override void Display(string label, DisplayDetail detail, int indent)
        {
            switch (detail)
            {
                case DisplayDetail.Lite:
                case DisplayDetail.Full:
                    DisplayLabelArgument(label, _Text, indent);
                    DisplayField("RowSpan", _RowSpan.ToString(), indent + 1);
                    DisplayField("ColumnSpan", _ColumnSpan.ToString(), indent + 1);
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
            XElement element = new XElement(name, (!String.IsNullOrEmpty(_Text) ? _Text : String.Empty));

            if (_RowSpan > 1)
                element.Add(new XAttribute("RowSpan", _RowSpan));

            if (_ColumnSpan > 1)
                element.Add(new XAttribute("ColumnSpan", _ColumnSpan));

            return element;
        }

        public override void OnElement(XElement element)
        {
            base.OnElement(element);
            _Text = element.Value;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "RowSpan":
                    _RowSpan = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "ColumnSpan":
                    _ColumnSpan = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                default:
                    return OnUnknownAttribute(attribute);
            }

            return true;
        }
    }
}
