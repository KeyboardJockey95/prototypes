using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    // Used in describing sentence patterns.
    // Key is the type.
    // Base is the designator which describes inflected form, if any.
    public class WordToken : Designator
    {
        // Word type ("Main", "Helper", "Pronoun", "ImplicitPronoun", "Polarizer", "Modal").
        public string Type;
        // The word text.
        public LiteralString Word;
        // Operations.
        public List<Operation> Operations;

        public static string[] Types =
        {
            "Main",             // The main inflectable.
            "Helper",           // A helper inflectable.
            "Pronoun",          // A pronoun.
            "ImplicitPronoun",  // An implicit pronoun.
            "Polarizer",        // A polarizer word (i.e. "not").
            "Modal",            // A modal word (i.e. "will").
            "Iterator"          // Placeholder for an iterator.
        };

        public WordToken(
            string type,
            LiteralString word,
            List<Classifier> classifications) : base(type, classifications)
        {
            ClearWordToken();
            Type = type;
            Word = word;
        }

        public WordToken(XElement element)
        {
            OnElement(element);
            DefaultLabelCheck();
        }

        public WordToken(WordToken other) : base(other)
        {
            CopyWordToken(other);
        }

        public WordToken()
        {
            ClearWordToken();
        }

        public void ClearWordToken()
        {
            Type = null;
            Word = null;
            Operations = null;
        }

        public void CopyWordToken(WordToken other)
        {
            Type = other.Type;
            Word = other.CloneWord();
            Operations = other.CloneOperations();
        }

        public override void Clear()
        {
            ClearWordToken();
        }

        public override string ToString()
        {
            return Xml.ToString();
        }

        public LiteralString CloneWord()
        {
            if (Word == null)
                return null;

            return new LiteralString(Word);
        }

        public List<Operation> CloneOperations()
        {
            if (Operations == null)
                return null;

            List<Operation> operations = new List<Operation>();

            foreach (Operation operation in Operations)
                operations.Add(new Operation(operation));

            return operations;
        }

        public Operation GetOperationIndex(int index)
        {
            if ((Operations == null) || (index < 0) || (index >= Operations.Count()))
                return null;

            return Operations[index];
        }

        public void AppendOperation(Operation operation)
        {
            if (Operations == null)
                Operations = new List<Operation>() { operation };
            else
                Operations.Add(operation);
        }

        public int OperationCount()
        {
            if (Operations == null)
                return 0;

            return Operations.Count();
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (!String.IsNullOrEmpty(Type))
                element.Add(new XAttribute("Type", Type));

            if (Word != null)
                element.Add(new XAttribute("Word", Word.StringListString));

            if (Operations != null)
            {
                foreach (Operation operation in Operations)
                    element.Add(operation.GetElement("Operation"));
            }

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Type":
                    Type = attributeValue;
                    break;
                case "Word":
                    Word = new LiteralString(attributeValue);
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
                case "Type":
                    Type = childElement.Value.Trim();
                    break;
                case "Word":
                    Word = new LiteralString(childElement);
                    break;
                case "Operation":
                    AppendOperation(new Operation(childElement));
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
