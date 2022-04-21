using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    public class Condition : BaseObject
    {
        public string AndCase;
        public string AndNotCase;
        public string OrCase;
        public string OrNotCase;

        public Condition(
            string andCaseLabel,
            string andNotCase,
            string orCaseLabel,
            string orNotCase)
        {
            AndCase = andCaseLabel;
            AndNotCase = andNotCase;
            OrCase = orCaseLabel;
            OrNotCase = orNotCase;
        }

        public Condition(Condition other)
        {
            CopyCondition(other);
        }

        public Condition(XElement element)
        {
            ClearCondition();
            OnElement(element);
        }

        public Condition()
        {
            ClearCondition();
        }

        public void ClearCondition()
        {
            AndCase = null;
            AndNotCase = null;
            OrCase = null;
            OrNotCase = null;
        }

        public void CopyCondition(Condition other)
        {
            AndCase = other.AndCase;
            AndNotCase = other.AndNotCase;
            OrCase = other.OrCase;
            OrNotCase = other.OrNotCase;
        }

        public bool MatchCases(string caseLabel)
        {
            if (!MatchAndCase(caseLabel))
                return false;

            if (MatchAndNotCase(caseLabel))
                return false;

            if (!MatchOrCase(caseLabel))
                return false;

            if (MatchOrNotCase(caseLabel))
                return false;

            return true;
        }

        public bool MatchAndCase(string caseLabel)
        {
            if (String.IsNullOrEmpty(AndCase))
                return true;

            if (AndCase.Contains(","))
            {
                string[] parts = AndCase.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    if (!caseLabel.Contains(part.Trim()))
                        return false;
                }
                return true;
            }
            else if (caseLabel == AndCase)
                return true;
            else if (!AndCase.Contains(" "))
            {
                if (TextUtilities.ContainsWholeWord(caseLabel, AndCase))
                    return true;
            }

            return false;
        }

        public bool MatchAndNotCase(string caseLabel)
        {
            if (String.IsNullOrEmpty(AndNotCase))
                return false;

            if (AndNotCase.Contains(","))
            {
                string[] parts = AndNotCase.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    if (!caseLabel.Contains(part.Trim()))
                        return false;
                }
                return true;
            }
            else if (caseLabel == AndNotCase)
                return true;
            else if (!AndNotCase.Contains(" "))
            {
                if (TextUtilities.ContainsWholeWord(caseLabel, AndNotCase))
                    return true;
            }

            return false;
        }

        public bool MatchOrCase(string caseLabel)
        {
            if (String.IsNullOrEmpty(OrCase))
                return true;

            if (OrCase.Contains(","))
            {
                string[] parts = OrCase.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    if (caseLabel.Contains(part.Trim()))
                        return true;
                }
            }
            else if (caseLabel.Contains(OrCase))
                return true;
            else if (!OrCase.Contains(" "))
            {
                if (TextUtilities.ContainsWholeWord(caseLabel, OrCase))
                    return true;
            }

            return false;
        }

        public bool MatchOrNotCase(string caseLabel)
        {
            if (String.IsNullOrEmpty(OrNotCase))
                return false;

            if (OrNotCase.Contains(","))
            {
                string[] parts = OrNotCase.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    if (caseLabel.Contains(part.Trim()))
                        return true;
                }

                return false;
            }
            else if (caseLabel.Contains(OrNotCase))
                return true;
            else if (!OrNotCase.Contains(" "))
            {
                if (TextUtilities.ContainsWholeWord(caseLabel, OrNotCase))
                    return true;
            }

            return false;
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if (!String.IsNullOrEmpty(AndCase))
                element.Add(new XAttribute("AndCase", AndCase));

            if (!String.IsNullOrEmpty(AndNotCase))
                element.Add(new XAttribute("AndNotCase", AndNotCase));

            if (!String.IsNullOrEmpty(OrCase))
                element.Add(new XAttribute("OrCase", OrCase));

            if (!String.IsNullOrEmpty(OrNotCase))
                element.Add(new XAttribute("OrNotCase", OrNotCase));

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value;

            switch (attribute.Name.LocalName)
            {
                case "AndCase":
                    AndCase = attributeValue;
                    break;
                case "AndNotCase":
                    AndNotCase = attributeValue;
                    break;
                case "OrCase":
                    OrCase = attributeValue;
                    break;
                case "OrNotCase":
                    OrNotCase = attributeValue;
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
                case "AndCase":
                    AndCase = childElement.Value.Trim();
                    break;
                case "AndNotCase":
                    AndNotCase = childElement.Value.Trim();
                    break;
                case "OrCase":
                    OrCase = childElement.Value.Trim();
                    break;
                case "OrNotCase":
                    OrNotCase = childElement.Value.Trim();
                    break;
                default:
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return (AndCase + AndNotCase + OrCase + OrNotCase).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == (object)this)
                return true;
            if (obj == null)
                return false;
            if (obj as Condition == null)
                return false;
            return Compare(obj as Condition) == 0 ? true : false;
        }

        public virtual bool Equals(Condition obj)
        {
            return Compare(obj) == 0 ? true : false;
        }

        public static bool operator ==(Condition other1, Condition other2)
        {
            if (((object)other1 == null) && ((object)other2 == null))
                return true;
            if (((object)other1 == null) || ((object)other2 == null))
                return false;
            return (other1.Compare(other2) == 0 ? true : false);
        }

        public static bool operator !=(Condition other1, Condition other2)
        {
            if (((object)other1 == null) && ((object)other2 == null))
                return false;
            if (((object)other1 == null) || ((object)other2 == null))
                return true;
            return (other1.Compare(other2) == 0 ? false : true);
        }

        public int Compare(Condition other)
        {
            int diff = 0;

            if ((diff = String.Compare(AndCase, other.AndCase)) != 0)
                return diff;

            if ((diff = String.Compare(AndNotCase, other.AndNotCase)) != 0)
                return diff;

            if ((diff = String.Compare(OrCase, other.OrCase)) != 0)
                return diff;

            if ((diff = String.Compare(OrNotCase, other.OrNotCase)) != 0)
                return diff;

            return 0;
        }

        public static int CompareConditionLists(List<Condition> list1, List<Condition> list2)
        {
            if (list1 == list2)
                return 0;

            if ((list1 == null) || (list2 == null))
            {
                if (list1 == null)
                    return -1;
                else
                    return 1;
            }


            int count1 = list1.Count();
            int count2 = list2.Count();
            int count = (count1 > count2 ? count2 : count1);

            for (int index = 0; index < count; index++)
            {
                if (list1[index] != list2[index])
                    return list1[index].Compare(list2[index]);
            }

            if (count1 > count2)
                return 1;
            else if (count1 == count2)
                return 0;
            else
                return -1;
        }
    }
}
