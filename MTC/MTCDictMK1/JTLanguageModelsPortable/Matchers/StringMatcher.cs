using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Matchers
{
    public partial class StringMatcher : Matcher
    {
        public List<string> Patterns { get; set; }
        public bool IgnoreCase { get; set; }
        public LanguageID LanguageID { get; set; }

        public StringMatcher(string pattern, string memberName, MatchCode matchType, int page, int pageSize)
            : base(memberName, matchType, page, pageSize)
        {
            Patterns = new List<string>(1) { TextUtilities.GetNormalizedString(pattern) };
            IgnoreCase = false;
            LanguageID = null;
        }

        public StringMatcher(string pattern, string memberName, MatchCode matchType, bool ignoreCase, LanguageID languageID, int page, int pageSize)
            : base(memberName, matchType, page, pageSize)
        {
            Patterns = new List<string>(1) { TextUtilities.GetNormalizedString(pattern) };
            IgnoreCase = ignoreCase;
            LanguageID = languageID;
        }

        public StringMatcher(List<string> patterns, string memberName, MatchCode matchType, int page, int pageSize)
            : base(memberName, matchType, page, pageSize)
        {
            Patterns = patterns;
            NormalizePatterns();
            IgnoreCase = false;
            LanguageID = null;
        }

        public StringMatcher(List<string> patterns, string memberName, MatchCode matchType, bool ignoreCase, LanguageID languageID, int page, int pageSize)
            : base(memberName, matchType, page, pageSize)
        {
            Patterns = patterns;
            NormalizePatterns();
            IgnoreCase = ignoreCase;
            LanguageID = languageID;
        }

        public StringMatcher(StringMatcher other)
            : base(other)
        {
            CopyStringMatcher(other);
        }

        public StringMatcher(XElement element)
        {
            OnElement(element);
        }

        public StringMatcher()
        {
            ClearStringMatcher();
        }

        public override void Clear()
        {
            base.Clear();
            ClearStringMatcher();
        }

        public void ClearStringMatcher()
        {
            Patterns = null;
            IgnoreCase = false;
            LanguageID = null;
        }

        public virtual void CopyStringMatcher(StringMatcher other)
        {
            if (other.Patterns != null)
                Patterns = new List<string>(other.Patterns);
            else
                Patterns = null;
            IgnoreCase = other.IgnoreCase;
            LanguageID = other.LanguageID;
        }

        public void NormalizePatterns()
        {
            if (Patterns == null)
                return;

            int count = Patterns.Count();
            int index;

            for (index = 0; index < count; index++)
                Patterns[index] = TextUtilities.GetNormalizedString(Patterns[index]);
        }

        public override bool Match(object obj)
        {
            if ((Patterns == null) || (Patterns.Count() == 0))
                return true;

            string stringObj = obj as string;

            if (stringObj == null)
            {
                IBaseObjectKeyed baseObj = obj as IBaseObjectKeyed;

                if (baseObj != null)
                {
                    object subObject = ObjectTypes.GetMemberValue(baseObj, MemberName);

                    if (subObject != null)
                    {
                        if ((stringObj = subObject as string) == null)
                            stringObj = subObject.ToString();
                    }
                }
            }

#if !PORTABLE
            CultureInfo cultureInfo = null;
#endif

            stringObj = TextUtilities.GetNormalizedString(stringObj);

            if (IgnoreCase)
            {
                if (stringObj == null)
                    stringObj = String.Empty;

#if PORTABLE
                stringObj = stringObj.ToLower();
#else
                cultureInfo = (LanguageID == null ? null : LanguageID.CultureInfo);

                if (cultureInfo == null)
                    stringObj = stringObj.ToLower();
                else
                    stringObj = ObjectUtilities.ToLower(stringObj, cultureInfo);
#endif
            }

            switch (MatchType)
            {
                case MatchCode.Between:
                    if (Patterns.Count() == 2)
                    {
                        if ((string.Compare(stringObj, Patterns[0]) >= 0) && (string.Compare(stringObj, Patterns[1]) <= 0))
                            return true;
                    }
                    else
                        throw new ObjectException("StringMatcher.Match:  Need 2 patterns.");
                    break;
                case MatchCode.Outside:
                    if (Patterns.Count() == 2)
                    {
                        if ((string.Compare(stringObj, Patterns[0]) < 0) || (string.Compare(stringObj, Patterns[1]) > 0))
                            return true;
                    }
                    else
                        throw new ObjectException("StringMatcher.Match:  Need 2 patterns.");
                    break;
                default:
                    foreach (string pattern in Patterns)
                    {
                        string thePattern = pattern;

                        if (thePattern == null)
                            thePattern = String.Empty;

                        if (IgnoreCase)
                        {
#if PORTABLE
                            thePattern = thePattern.ToLower();
#else
                            if (cultureInfo == null)
                                thePattern = thePattern.ToLower();
                            else
                                thePattern = ObjectUtilities.ToLower(thePattern, cultureInfo);
#endif

                            if (MatchStrings(MatchType, thePattern, stringObj))
                                return true;
                        }
                        else
                        {
                            if (MatchStrings(MatchType, thePattern, stringObj))
                                return true;
                        }
                    }
                    break;
            }

            return false;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (Patterns != null)
            {
                XElement subElement = new XElement("Patterns");
                foreach (string pattern in Patterns)
                    subElement.Add(new XElement("Pattern", pattern));
                element.Add(subElement);
            }
            element.Add(new XAttribute("IgnoreCase", IgnoreCase.ToString()));
            if (LanguageID != null)
                element.Add(new XAttribute("LanguageID", LanguageID.LanguageCultureExtensionCode));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "IgnoreCase":
                    IgnoreCase = (attributeValue == "True" ? true : false);
                    break;
                case "LanguageID":
                    LanguageID = LanguageLookup.GetLanguageID(attributeValue);
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
                case "Patterns":
                    {
                        IEnumerable<XElement> elements = childElement.Elements();
                        Patterns = new List<string>(elements.Count());
                        foreach (XElement subElement in elements)
                            Patterns.Add(subElement.Value);
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
