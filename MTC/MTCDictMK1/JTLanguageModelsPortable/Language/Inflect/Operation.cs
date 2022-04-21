using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    public class Operation
    {
        // Operator. (See Operators.)
        public string Operator;
        // Operand.
        public string Operand;

        // Operator strings.
        public static string[] Operators =
        {
            "Inherit",      // Use inherited inflection. the operand is the source token type.
            "TruncateEnd",  // Truncate (Operand) number of characters from end.
            "Append"        // Append suffix, where the suffix is references by the operand.
                            // If the operand is "Iterator" use the pronoun iterator text.
        };

        public Operation(string operatorValue, string operand)
        {
            Operator = operatorValue;
            Operand = operand;
        }

        public Operation(Operation other)
        {
            Operator = other.Operator;
            Operand = other.Operand;
        }

        public Operation(XElement element)
        {
            Operator = element.Attribute("Operator").Value;
            Operand = element.Attribute("Operand").Value;
        }

        public Operation()
        {
            Operator = null;
            Operand = null;
        }

        public int OperandInteger(int defaultValue)
        {
            int returnValue = ObjectUtilities.GetIntegerFromString(Operand, defaultValue);
            return returnValue;
        }

        public override string ToString()
        {
            string returnValue = (Operator != null ? Operator : "") + " " + (Operand != null ? Operand : "");
            return returnValue;
        }

        public XElement GetElement(string name)
        {
            XElement element = new XElement(name);
            element.Add(new XAttribute("Operator", Operator));
            element.Add(new XAttribute("Operand", Operand));
            return element;
        }

        public int Compare(Operation other)
        {
            if (other == null)
                return 1;

            int diff = String.Compare(Operator, other.Operator);

            if (diff != 0)
                return diff;

            diff = String.Compare(Operand, other.Operand);

            if (diff != 0)
                return diff;

            return diff;
        }
    }
}
