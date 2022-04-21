using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    public class SpecialAction : BaseObject
    {
        public string Type;
        public LiteralString Input;
        public LiteralString Output;
        public LiteralString Prefix;
        public LiteralString Stem;
        public LiteralString Suffix;
        public LiteralString Qualifiers;
        public string From;
        public string To;
        public List<Condition> Conditions;
        public bool Done;
        public bool Regular;

        public SpecialAction(
            string type,
            LiteralString input,
            LiteralString output,
            LiteralString prefix,
            LiteralString stem,
            LiteralString suffix,
            LiteralString qualifiers,
            string from,
            string to,
            List<Condition> conditions,
            bool done,
            bool regular)
        {
            Type = type;
            Input = input;
            Output = output;
            Prefix = prefix;
            Stem = stem;
            Suffix = suffix;
            Qualifiers = qualifiers;
            From = from;
            To = to;
            Conditions = conditions;
            Done = done;
            Regular = regular;
        }

        public SpecialAction(
            string type,
            LiteralString input,
            LiteralString output)
        {
            Type = type;
            Input = input;
            Output = output;
            Prefix = null;
            Stem = null;
            Suffix = null;
            Qualifiers = null;
            From = null;
            To = null;
            Qualifiers = null;
            Conditions = null;
            Done = false;
            Regular = true;
        }

        public SpecialAction(
            string type,
            LiteralString input)
        {
            Type = type;
            Input = input;
            Output = null;
            Prefix = null;
            Stem = null;
            Suffix = null;
            Qualifiers = null;
            From = null;
            To = null;
            Conditions = null;
            Done = false;
            Regular = true;
        }

        public SpecialAction(
            string type)
        {
            Type = type;
            Input = null;
            Output = null;
            Prefix = null;
            Stem = null;
            Suffix = null;
            Qualifiers = null;
            From = null;
            To = null;
            Conditions = null;
            Done = false;
            Regular = true;
        }

        public SpecialAction(SpecialAction other)
        {
            CopySpecialAction(other);
        }

        public SpecialAction(XElement element)
        {
            ClearSpecialAction();
            OnElement(element);
        }

        public SpecialAction()
        {
            ClearSpecialAction();
        }

        public void ClearSpecialAction()
        {
            Type = null;
            Input = null;
            Output = null;
            Prefix = null;
            Stem = null;
            Suffix = null;
            Qualifiers = null;
            From = null;
            To = null;
            Conditions = null;
            Done = false;
            Regular = true;
        }

        public void CopySpecialAction(SpecialAction other)
        {
            Type = other.Type;
            Input = other.Input;
            Output = other.Output;
            Prefix = other.Prefix;
            Stem = other.Stem;
            Suffix = other.Suffix;
            From = other.From;
            To = other.To;
            Qualifiers = other.Qualifiers;
            Conditions = other.Conditions;
            Done = other.Done;
            Regular = other.Regular;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrEmpty(Type))
                sb.Append("Type=" + Type);

            if (Input != null)
                sb.Append("Input=" + Input.ToString());

            if (Output != null)
                sb.Append("Output=" + Output.ToString());

            if (Prefix != null)
                sb.Append("Prefix=" + Prefix.ToString());

            if (Stem != null)
                sb.Append("Stem=" + Stem.ToString());

            if (Suffix != null)
                sb.Append("Suffix=" + Suffix.ToString());

            if (From != null)
                sb.Append("From=" + From.ToString());

            if (To != null)
                sb.Append("To=" + To.ToString());

            if ((Conditions != null) && (Conditions.Count() != 0))
            {
                if (sb.Length != 0)
                    sb.Append(" ");

                sb.Append("Conditions=");
                int count = 0;

                foreach (Condition condition in Conditions)
                {
                    if (count != 0)
                        sb.Append(",");

                    sb.Append(condition.ToString());
                    count++;
                }
            }

            sb.Append("Done=" + Done.ToString());
            sb.Append("Regular=" + Regular.ToString());

            return sb.ToString();
        }

        public bool MatchCases(string caseLabel)
        {
            if ((Conditions == null) || (Conditions.Count() == 0))
                return true;

            foreach (Condition condition in Conditions)
            {
                if (condition.MatchCases(caseLabel))
                    return true;
            }

            return false;
        }

        public bool ChangesSuffix()
        {
            if (Type.Contains("Suffix"))
                return true;

            return false;
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if (!String.IsNullOrEmpty(Type))
                element.Add(new XAttribute("Type", Type));

            if (Input != null)
            {
                string literal = Input.StringListString;
                element.Add(new XElement("Input", literal));
            }

            if (Output != null)
            {
                string literal = Output.StringListString;
                element.Add(new XElement("Output", literal));
            }

            if (Prefix != null)
            {
                string literal = Prefix.StringListString;
                element.Add(new XElement("Prefix", literal));
            }

            if (Stem != null)
            {
                string literal = Stem.StringListString;
                element.Add(new XElement("Stem", literal));
            }

            if (Suffix != null)
            {
                string literal = Suffix.StringListString;
                element.Add(new XElement("Suffix", literal));
            }

            if (Qualifiers != null)
            {
                string literal = Qualifiers.StringListString;
                element.Add(new XElement("Qualifiers", literal));
            }

            if (!String.IsNullOrEmpty(From))
                element.Add(new XAttribute("From", From));

            if (!String.IsNullOrEmpty(To))
                element.Add(new XAttribute("To", To));

            if (Conditions != null)
            {
                if (Conditions.Count() == 1)
                {
                    Condition condition = Conditions[0];

                    if (!String.IsNullOrEmpty(condition.AndCase))
                        element.Add(new XAttribute("AndCase", condition.AndCase));

                    if (!String.IsNullOrEmpty(condition.AndNotCase))
                        element.Add(new XAttribute("AndNotCase", condition.AndNotCase));

                    if (!String.IsNullOrEmpty(condition.OrCase))
                        element.Add(new XAttribute("OrCase", condition.OrCase));

                    if (!String.IsNullOrEmpty(condition.OrNotCase))
                        element.Add(new XAttribute("OrNotCase", condition.OrNotCase));
                }
                else
                {
                    foreach (Condition condition in Conditions)
                    {
                        XElement childElement = condition.Xml;
                        element.Add(childElement);
                    }
                }
            }

            if (Done)
                element.Add(new XAttribute("Done", "true"));

            if (Regular)
                element.Add(new XAttribute("Regular", "true"));

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value;

            switch (attribute.Name.LocalName)
            {
                case "Type":
                    Type = attributeValue;
                    break;
                case "Input":
                    Input = new LiteralString(attributeValue);
                    break;
                case "Output":
                    Output = new LiteralString(attributeValue);
                    break;
                case "Prefix":
                    Prefix = new LiteralString(attributeValue);
                    break;
                case "Stem":
                    Stem = new LiteralString(attributeValue);
                    break;
                case "Suffix":
                    Suffix = new LiteralString(attributeValue);
                    break;
                case "Qualifiers":
                    Qualifiers = new LiteralString(attributeValue);
                    break;
                case "From":
                    From = attributeValue;
                    break;
                case "To":
                    To = attributeValue;
                    break;
                case "AndCase":
                    {
                        Condition condition = null;
                        if (Conditions == null)
                            Conditions = new List<Condition>();
                        if (Conditions.Count() == 0)
                            Conditions.Add(condition = new Condition());
                        else
                            condition = Conditions[0];
                        condition.AndCase = attributeValue;
                    }
                    break;
                case "AndNotCase":
                    {
                        Condition condition = null;
                        if (Conditions == null)
                            Conditions = new List<Condition>();
                        if (Conditions.Count() == 0)
                            Conditions.Add(condition = new Condition());
                        else
                            condition = Conditions[0];
                        condition.AndNotCase = attributeValue;
                    }
                    break;
                case "OrCase":
                    {
                        Condition condition = null;
                        if (Conditions == null)
                            Conditions = new List<Condition>();
                        if (Conditions.Count() == 0)
                            Conditions.Add(condition = new Condition());
                        else
                            condition = Conditions[0];
                        condition.OrCase = attributeValue;
                    }
                    break;
                case "OrNotCase":
                    {
                        Condition condition = null;
                        if (Conditions == null)
                            Conditions = new List<Condition>();
                        if (Conditions.Count() == 0)
                            Conditions.Add(condition = new Condition());
                        else
                            condition = Conditions[0];
                        condition.OrNotCase = attributeValue;
                    }
                    break;
                case "Done":
                    Done = ObjectUtilities.GetBoolFromString(attributeValue, false);
                    break;
                case "Regular":
                    Regular = ObjectUtilities.GetBoolFromString(attributeValue, false);
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
                case "Input":
                    if (childElement.HasElements)
                    {
                        List<string> stringList = new List<string>();
                        foreach (XElement subElement in childElement.Elements())
                            stringList.Add(subElement.Value);
                        Input = new LiteralString(stringList);
                    }
                    else
                        Input = new LiteralString(childElement.Value.Trim());
                    break;
                case "Output":
                    if (childElement.HasElements)
                    {
                        List<string> stringList = new List<string>();
                        foreach (XElement subElement in childElement.Elements())
                            stringList.Add(subElement.Value);
                        Output = new LiteralString(stringList);
                    }
                    else
                        Output = new LiteralString(childElement.Value.Trim());
                    break;
                case "Prefix":
                    if (childElement.HasElements)
                    {
                        List<string> stringList = new List<string>();
                        foreach (XElement subElement in childElement.Elements())
                            stringList.Add(subElement.Value);
                        Prefix = new LiteralString(stringList);
                    }
                    else
                        Prefix = new LiteralString(childElement.Value.Trim());
                    break;
                case "Stem":
                    if (childElement.HasElements)
                    {
                        List<string> stringList = new List<string>();
                        foreach (XElement subElement in childElement.Elements())
                            stringList.Add(subElement.Value);
                        Stem = new LiteralString(stringList);
                    }
                    else
                        Stem = new LiteralString(childElement.Value.Trim());
                    break;
                case "Suffix":
                    if (childElement.HasElements)
                    {
                        List<string> stringList = new List<string>();
                        foreach (XElement subElement in childElement.Elements())
                            stringList.Add(subElement.Value);
                        Suffix = new LiteralString(stringList);
                    }
                    else
                        Suffix = new LiteralString(childElement.Value.Trim());
                    break;
                case "Qualifiers":
                    if (childElement.HasElements)
                    {
                        List<string> stringList = new List<string>();
                        foreach (XElement subElement in childElement.Elements())
                            stringList.Add(subElement.Value);
                        Qualifiers = new LiteralString(stringList);
                    }
                    else
                        Qualifiers = new LiteralString(childElement.Value.Trim());
                    break;
                case "From":
                    From = childElement.Value.Trim();
                    break;
                case "To":
                    To = childElement.Value.Trim();
                    break;
                case "Condition":
                    {
                        Condition condition = new Condition(childElement);
                        if (Conditions == null)
                            Conditions = new List<Condition>() { condition };
                        else
                            Conditions.Add(condition);
                    }
                    break;
                case "AndCase":
                    {
                        Condition condition = null;
                        if (Conditions == null)
                            Conditions = new List<Condition>();
                        if (Conditions.Count() == 0)
                            Conditions.Add(condition = new Condition());
                        else
                            condition = Conditions[0];
                        condition.AndCase = childElement.Value.Trim();
                    }
                    break;
                case "AndNotCase":
                    {
                        Condition condition = null;
                        if (Conditions == null)
                            Conditions = new List<Condition>();
                        if (Conditions.Count() == 0)
                            Conditions.Add(condition = new Condition());
                        else
                            condition = Conditions[0];
                        condition.AndNotCase = childElement.Value.Trim();
                    }
                    break;
                case "OrCase":
                    {
                        Condition condition = null;
                        if (Conditions == null)
                            Conditions = new List<Condition>();
                        if (Conditions.Count() == 0)
                            Conditions.Add(condition = new Condition());
                        else
                            condition = Conditions[0];
                        condition.OrCase = childElement.Value.Trim();
                    }
                    break;
                case "OrNotCase":
                    {
                        Condition condition = null;
                        if (Conditions == null)
                            Conditions = new List<Condition>();
                        if (Conditions.Count() == 0)
                            Conditions.Add(condition = new Condition());
                        else
                            condition = Conditions[0];
                        condition.OrNotCase = childElement.Value.Trim();
                    }
                    break;
                case "Done":
                    Done = ObjectUtilities.GetBoolFromString(childElement.Value.Trim(), false);
                    break;
                case "Regular":
                    Regular = ObjectUtilities.GetBoolFromString(childElement.Value.Trim(), false);
                    break;
                default:
                    return false;
            }

            return true;
        }

        public int Compare(SpecialAction other)
        {
            int diff = 0;

            if ((diff = String.Compare(Type, other.Type)) != 0)
                return diff;

            if ((diff = LiteralString.Compare(Input, other.Input)) != 0)
                return diff;

            if ((diff = LiteralString.Compare(Output, other.Output)) != 0)
                return diff;

            if ((diff = LiteralString.Compare(Prefix, other.Prefix)) != 0)
                return diff;

            if ((diff = LiteralString.Compare(Stem, other.Stem)) != 0)
                return diff;

            if ((diff = LiteralString.Compare(Suffix, other.Suffix)) != 0)
                return diff;

            if ((diff = LiteralString.Compare(Qualifiers, other.Qualifiers)) != 0)
                return diff;

            if ((diff = String.Compare(From, other.From)) != 0)
                return diff;

            if ((diff = String.Compare(To, other.To)) != 0)
                return diff;

            if ((diff = Condition.CompareConditionLists(Conditions, other.Conditions)) != 0)
                return diff;

            if ((diff = Done.CompareTo(other.Done)) != 0)
                return diff;

            if ((diff = Regular.CompareTo(other.Regular)) != 0)
                return diff;

            return 0;
        }
    }
}
