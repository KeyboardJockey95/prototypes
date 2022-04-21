using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    // Describes a word from a word family.
    // Key is the text in the main language.
    public class TokenDescriptor : BaseObjectKeyed
    {
        // The token text.
        public LiteralString Text;
        // The token designator (uses "Person", "Number", and maybe "Gender" and "Politeness").
        public List<Designator> Designators;
        // Token type ("Personal", "Object", "Suffix").
        public string Type;
        // Token position ("Subject", "Reflexive, "Object").
        public string Position;

        public TokenDescriptor(
            string key,
            LiteralString text,
            List<Designator> designations,
            string type,
            string position) : base(key)
        {
            Text = text;
            Designators = designations;
            Type = type;
            Position = position;
        }

        public TokenDescriptor(XElement element)
        {
            OnElement(element);
        }

        public TokenDescriptor(TokenDescriptor other) : base(other)
        {
            CopyTokenDescriptor(other);
        }

        public TokenDescriptor()
        {
            ClearTokenDescriptor();
        }

        public void CopyTokenDescriptor(TokenDescriptor other)
        {
            Text = other.Text;
            Designators = other.Designators;
            Type = other.Type;
            Position = other.Position;
        }

        public void ClearTokenDescriptor()
        {
            Text = null;
            Designators = null;
            Type = null;
            Position = null;
        }

        public override void Clear()
        {
            ClearTokenDescriptor();
        }

        public override string ToString()
        {
            return Xml.ToString();
        }

        public string TextFirst
        {
            get
            {
                return Text.GetIndexedString(0);
            }
        }

        public LiteralString CloneText()
        {
            if (Text == null)
                return null;

            return new LiteralString(Text);
        }

        public int GetMatchWeight(Designator designator)
        {
            int bestWeight = 0;

            foreach (Designator testDesignator in Designators)
            {
                int weight = testDesignator.GetMatchWeight(designator);

                if (weight > bestWeight)
                    bestWeight = weight;
            }

            return bestWeight;
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            element.Add(new XAttribute("Key", KeyString));

            if (Text != null)
                element.Add(Text.GetElement("Text"));

            if ((Designators != null) && (Designators.Count() != 0))
            {
                foreach (Designator designator in Designators)
                    element.Add(designator.GetElement("Designator"));
            }

            if (!String.IsNullOrEmpty(Type))
                element.Add(new XElement("Type", Type));

            if (!String.IsNullOrEmpty(Position))
                element.Add(new XElement("Position", Position));

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Key":
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
                case "Text":
                    Text = new LiteralString(childElement);
                    break;
                case "Designator":
                    if (Designators == null)
                        Designators = new List<Designator>();
                    Designators.Add(new Designator(childElement));
                    break;
                case "Type":
                    Type = childElement.Value.Trim();
                    break;
                case "Position":
                    Position = childElement.Value.Trim();
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
