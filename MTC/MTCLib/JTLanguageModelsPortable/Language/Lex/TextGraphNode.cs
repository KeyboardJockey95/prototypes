using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Language
{
    public class TextGraphNode : TextRun
    {
        protected string _Text;
        protected DictionaryEntry _Entry;
        protected double _Weight;

        public TextGraphNode(
                int start,
                int length,
                string text,
                DictionaryEntry entry,
                double weight)
            : base(start, length, null)
        {
            _Text = text;
            _Entry = entry;
            _Weight = weight;
        }

        public TextGraphNode(TextGraphNode other) : base(other)
        {
            _Text = other.Text;
            _Entry = other.Entry;
            _Weight = other.Weight;
        }

        public TextGraphNode(XElement element)
        {
            _Text = null;
            _Entry = null;
            _Weight = 0.0;
            OnElement(element);
            _Modified = false;
        }

        public TextGraphNode()
        {
            ClearTextGraphNode();
        }

        public override void Clear()
        {
            base.Clear();
            ClearTextGraphNode();
        }

        public void ClearTextGraphNode()
        {
            _Text = null;
            _Entry = null;
            _Weight = 0.0;
        }

        public override IBaseObject Clone()
        {
            return new TextGraphNode(this);
        }

        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                if (value == null)
                    value = String.Empty;

                if (value != _Text)
                    _Modified = true;

                _Text = value;
            }
        }

        public DictionaryEntry Entry
        {
            get
            {
                return _Entry;
            }
            set
            {
                if (value != Entry)
                {
                    _Entry = value;
                    _Modified = true;
                }
            }
        }

        public double Weight
        {
            get
            {
                return _Weight;
            }
            set
            {
                if (value != _Weight)
                {
                    _Weight = value;
                    _Modified = true;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_Entry != null)
                element.Add(_Entry.GetElement("Entry"));
            if (!String.IsNullOrEmpty(_Text))
                element.Add(new XAttribute("Text", _Text));
            element.Add(new XAttribute("Weight", _Weight));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Text":
                    _Text = attributeValue;
                    break;
                case "Weight":
                    _Weight = Convert.ToDouble(attributeValue);
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Entry":
                    DictionaryEntry entry = new DictionaryEntry(childElement);
                    _Entry = entry;
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public static int Compare(TextGraphNode object1, TextGraphNode object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            int returnValue = TextRun.Compare(object1, object2);
            if (returnValue != 0)
                return returnValue;
            returnValue = object1.Text.CompareTo(object2.Text);
            if (returnValue != 0)
                return returnValue;
            if (object1.Weight > object2.Weight)
                return 1;
            else if (object1.Weight < object2.Weight)
                return -1;
            return 0;
        }
    }
}
