using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    // Represents a word example used to collect inflections.
    public class WordDescriptor : BaseObject
    {
        // Word class.
        public string ClassKey;
        // Word subclass.
        public string SubClassKey;
        // Word.
        public string Word;

        public WordDescriptor(
            string classKey,
            string subClassKey,
            string word)
        {
            ClassKey = classKey;
            SubClassKey = subClassKey;
            Word = word;
        }

        public WordDescriptor(XElement element)
        {
            OnElement(element);
        }

        public WordDescriptor(WordDescriptor other)
        {
            CopyWordDescriptor(other);
        }

        public WordDescriptor()
        {
            ClearWordDescriptor();
        }

        public void CopyWordDescriptor(WordDescriptor other)
        {
            ClassKey = other.ClassKey;
            SubClassKey = other.SubClassKey;
            Word = other.Word;
        }

        public void ClearWordDescriptor()
        {
            ClassKey = null;
            SubClassKey = null;
            Word = null;
        }

        public override void Clear()
        {
            ClearWordDescriptor();
        }

        public override string ToString()
        {
            return Xml.ToString();
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if (!String.IsNullOrEmpty(ClassKey))
                element.Add(new XAttribute("ClassKey", ClassKey));

            if (!String.IsNullOrEmpty(SubClassKey))
                element.Add(new XAttribute("SubClassKey", SubClassKey));

            if (!String.IsNullOrEmpty(Word))
                element.Add(new XAttribute("Word", Word));

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "ClassKey":
                    ClassKey = attributeValue;
                    break;
                case "SubClassKey":
                    SubClassKey = attributeValue;
                    break;
                case "Word":
                    Word = attributeValue;
                    break;
                default:
                    return OnUnknownAttribute(attribute);
            }

            return true;
        }
    }
}
