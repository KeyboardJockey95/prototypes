using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Matchers
{
    public partial class LanguageIDMatcher : Matcher
    {
        public List<LanguageID> LanguageIDs { get; set; }
        public string Delimiter { get; set; }

        public LanguageIDMatcher(LanguageID languageID, string memberName, MatchCode matchType, int page, int pageSize)
            : base(memberName, matchType, page, pageSize)
        {
            LanguageIDs = new List<LanguageID>(1) { languageID };
            Delimiter = null;
        }

        public LanguageIDMatcher(List<LanguageID> languageIDs, string memberName, MatchCode matchType, int page, int pageSize)
            : base(memberName, matchType, page, pageSize)
        {
            LanguageIDs = languageIDs;
            Delimiter = null;
        }

        public LanguageIDMatcher(List<LanguageDescriptor> languageDescriptors, string memberName, MatchCode matchType, int page, int pageSize)
            : base(memberName, matchType, page, pageSize)
        {
            if (languageDescriptors != null)
            {
                LanguageIDs = new List<LanguageID>(languageDescriptors.Count());
                foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
                {
                    if (languageDescriptor.Used && (languageDescriptor.LanguageID != null))
                        LanguageIDs.Add(languageDescriptor.LanguageID);
                }
            }
            else
                LanguageIDs = null;
            Delimiter = null;
        }

        public LanguageIDMatcher(LanguageID languageID, string delimiter, string memberName, MatchCode matchType, int page, int pageSize)
            : base(memberName, matchType, page, pageSize)
        {
            LanguageIDs = new List<LanguageID>(1) { languageID };
            Delimiter = delimiter;
        }

        public LanguageIDMatcher(List<LanguageID> languageIDs, string delimiter, string memberName, MatchCode matchType, int page, int pageSize)
            : base(memberName, matchType, page, pageSize)
        {
            LanguageIDs = languageIDs;
            Delimiter = delimiter;
        }

        public LanguageIDMatcher(List<LanguageDescriptor> languageDescriptors, string delimiter, string memberName, MatchCode matchType, int page, int pageSize)
            : base(memberName, matchType, page, pageSize)
        {
            if (languageDescriptors != null)
            {
                LanguageIDs = new List<LanguageID>(languageDescriptors.Count());
                foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
                {
                    if (languageDescriptor.Used && (languageDescriptor.LanguageID != null))
                        LanguageIDs.Add(languageDescriptor.LanguageID);
                }
            }
            else
                LanguageIDs = null;
            Delimiter = delimiter;
        }

        public LanguageIDMatcher(LanguageIDMatcher other)
            : base(other)
        {
            CopyLanguageIDMatcher(other);
        }

        public LanguageIDMatcher(XElement element)
        {
            OnElement(element);
        }

        public LanguageIDMatcher()
        {
            ClearLanguageIDMatcher();
        }

        public override void Clear()
        {
            base.Clear();
            ClearLanguageIDMatcher();
        }

        public void ClearLanguageIDMatcher()
        {
            LanguageIDs = null;
            Delimiter = null;
        }

        public virtual void CopyLanguageIDMatcher(LanguageIDMatcher other)
        {
            if (other.LanguageIDs != null)
                LanguageIDs = new List<LanguageID>(other.LanguageIDs);
            else
                LanguageIDs = null;
            Delimiter = other.Delimiter;
        }

        public override bool Match(object obj)
        {
            if ((LanguageIDs == null) || (LanguageIDs.Count() == 0))
                return true;

            string stringObj = obj as string;
            LanguageID languageIDObj;
            IBaseObjectKeyed baseObj;

            if (stringObj == null)
            {
                if ((languageIDObj = obj as LanguageID) != null)
                {
                    if ((stringObj = languageIDObj.LanguageCultureExtensionCode) == null)
                        stringObj = String.Empty;
                }
                else if ((baseObj = obj as IBaseObjectKeyed) != null)
                {
                    object languageID = ObjectTypes.GetMemberValue(baseObj, MemberName);

                    if (languageID != null)
                    {
                        if ((languageIDObj = languageID as LanguageID) != null)
                        {
                            if ((stringObj = languageIDObj.LanguageCultureExtensionCode) == null)
                                stringObj = String.Empty;
                        }
                        else
                            stringObj = languageID as string;
                    }
                    else
                        stringObj = String.Empty;
                }
                else
                    stringObj = String.Empty;
            }

            if ((LanguageIDs == null) || (LanguageIDs.Count() == 0))
                return true;

            if (stringObj.Contains("(all languages)"))
                return true;
            else if (String.IsNullOrEmpty(Delimiter))
            {
                foreach (LanguageID languageID in LanguageIDs)
                {
                    string pattern;

                    if (languageID == null)
                        pattern = String.Empty;
                    else
                        pattern = languageID.LanguageCultureExtensionCode;

                    if (MatchStrings(MatchType, pattern, stringObj))
                        return true;
                }
            }
            else
            {
                foreach (LanguageID languageID in LanguageIDs)
                {
                    string pattern;

                    if (languageID == null)
                        pattern = String.Empty;
                    else
                        pattern = languageID.LanguageCultureExtensionCode;

                    if (MatchDelimitedStrings(MatchType, pattern, stringObj, Delimiter))
                        return true;
                }
            }

            return false;
        }

        public static bool MatchLanguageIDs(MatchCode matchType, LanguageID pattern, LanguageID obj)
        {
            string patternString;
            string objString;

            if (pattern == null)
                patternString = String.Empty;
            else
                patternString = pattern.LanguageCultureExtensionCode;

            if (obj == null)
                objString = String.Empty;
            else
                objString = obj.LanguageCultureExtensionCode;

            return MatchStrings(matchType, patternString, objString);
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (LanguageIDs != null)
            {
                XElement subElement = new XElement("LanguageIDs");
                foreach (LanguageID pattern in LanguageIDs)
                    subElement.Add(new XElement("LanguageID", pattern.LanguageCultureExtensionCode));
                element.Add(subElement);
            }
            if (Delimiter != null)
                element.Add(new XAttribute("Delimiter", Delimiter));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Delimiter":
                    Delimiter = attributeValue;
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
                case "LanguageIDs":
                    {
                        IEnumerable<XElement> elements = childElement.Elements();
                        LanguageIDs = new List<LanguageID>(elements.Count());
                        foreach (XElement subElement in elements)
                            LanguageIDs.Add(new LanguageID(subElement.Value));
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
