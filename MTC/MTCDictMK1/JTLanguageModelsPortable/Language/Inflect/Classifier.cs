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
    public class Classifier : BaseString
    {
        public Classifier(object key, string value) : base(key, value)
        {
        }

        public Classifier(string text) : base(text)
        {
        }

        public Classifier(Classifier other) : base(other)
        {
        }

        public Classifier(object key, Classifier other) : base(key, other)
        {
        }

        public Classifier(XElement element)
        {
            OnElement(element);
        }

        public Classifier()
        {
            ClearClassifier();
        }

        public override void Clear()
        {
            base.Clear();
            ClearClassifier();
        }

        public void ClearClassifier()
        {
        }

        public void CopyClassifier(Classifier other)
        {
        }

        public override IBaseObject Clone()
        {
            return new Classifier(this);
        }

        public override string ToString()
        {
            return KeyString + "=" + Text;
        }

        public bool MatchValue(Classifier other)
        {
            if (other == null)
                return false;
            
            if (Text == other.Text)
                return true;

            if ((Text == "Any") || (other.Text == "Any"))
                return true;

            return false;
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if (!String.IsNullOrEmpty(KeyString))
                element.Add(new XAttribute("Key", KeyString));

            if (!String.IsNullOrEmpty(Text))
                element.Add(new XAttribute("Value", Text));

            return element;
        }

        public override void OnElement(XElement element)
        {
            Clear();

            foreach (XAttribute attribute in element.Attributes())
            {
                if (!OnAttribute(attribute))
                    throw new ObjectException("Unknown attribute " + attribute.Name + " in " + element.Name.ToString() + ".");
            }

            foreach (var childNode in element.Elements())
            {
                XElement childElement = childNode as XElement;

                if (childElement == null)
                    continue;

                if (!OnChildElement(childElement))
                    throw new ObjectException("Unknown child element " + childElement.Name + " in " + element.Name.ToString() + ".");
            }
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Key":
                    Key = attributeValue;
                    break;
                case "Value":
                    Text = attributeValue;
                    break;
                default:
                    return true;
            }

            return true;
        }

        public static List<Classifier> CopyClassifiers(List<Classifier> classifiers)
        {
            if (classifiers == null)
                return null;

            List<Classifier> list = new List<Classifier>(classifiers.Count());

            foreach (Classifier classifier in classifiers)
                list.Add(new Classifier(classifier));

            return list;
        }
    }
}
