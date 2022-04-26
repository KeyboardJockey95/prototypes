using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Matchers
{
    public partial class BaseKeyMatcher : Matcher
    {
        public List<object> Patterns { get; set; }
        public bool IgnoreCase { get; set; }
        public LanguageID LanguageID { get; set; }
        public static string BaseMemberName = "Key";

        public BaseKeyMatcher(object pattern, MatchCode matchType, int page, int pageSize)
            : base(BaseMemberName, matchType, page, pageSize)
        {
            Patterns = new List<object>(1) { pattern };
            NormalizePatterns();
            IgnoreCase = false;
            LanguageID = null;
        }

        public BaseKeyMatcher(object pattern, MatchCode matchType, bool ignoreCase, LanguageID languageID, int page, int pageSize)
            : base(BaseMemberName, matchType, page, pageSize)
        {
            Patterns = new List<object>(1) { pattern };
            NormalizePatterns();
            IgnoreCase = ignoreCase;
            LanguageID = languageID;
        }

        public BaseKeyMatcher(List<object> patterns, MatchCode matchType, int page, int pageSize)
            : base(BaseMemberName, matchType, page, pageSize)
        {
            Patterns = patterns;
            NormalizePatterns();
            IgnoreCase = false;
            LanguageID = null;
        }

        public BaseKeyMatcher(List<object> patterns, MatchCode matchType, bool ignoreCase, LanguageID languageID, int page, int pageSize)
            : base(BaseMemberName, matchType, page, pageSize)
        {
            Patterns = patterns;
            NormalizePatterns();
            IgnoreCase = ignoreCase;
            LanguageID = languageID;
        }

        public BaseKeyMatcher(IBaseObjectKeyed pattern, MatchCode matchType, int page, int pageSize)
            : base(BaseMemberName, matchType, page, pageSize)
        {
            Patterns = new List<object>(1) { pattern.Key };
            NormalizePatterns();
            IgnoreCase = false;
            LanguageID = null;
        }

        public BaseKeyMatcher(IBaseObjectKeyed pattern, MatchCode matchType, bool ignoreCase, LanguageID languageID, int page, int pageSize)
            : base(BaseMemberName, matchType, page, pageSize)
        {
            Patterns = new List<object>(1) { pattern.Key };
            NormalizePatterns();
            IgnoreCase = ignoreCase;
            LanguageID = languageID;
        }

        public BaseKeyMatcher(List<IBaseObjectKeyed> patterns, MatchCode matchType, int page, int pageSize)
            : base(BaseMemberName, matchType, page, pageSize)
        {
            Patterns = BasesToKeys(patterns);
            NormalizePatterns();
            IgnoreCase = false;
            LanguageID = null;
        }

        public BaseKeyMatcher(List<IBaseObjectKeyed> patterns, MatchCode matchType, bool ignoreCase, LanguageID languageID, int page, int pageSize)
            : base(BaseMemberName, matchType, page, pageSize)
        {
            Patterns = BasesToKeys(patterns);
            NormalizePatterns();
            IgnoreCase = ignoreCase;
            LanguageID = languageID;
        }

        public BaseKeyMatcher(BaseKeyMatcher other)
            : base(other)
        {
            CopyBaseKeyMatcher(other);
        }

        public BaseKeyMatcher(XElement element)
        {
            OnElement(element);
        }

        public BaseKeyMatcher()
        {
            ClearBaseKeyMatcher();
        }

        public override void Clear()
        {
            base.Clear();
            ClearBaseKeyMatcher();
        }

        public void ClearBaseKeyMatcher()
        {
            MemberName = BaseMemberName;
            Patterns = null;
            IgnoreCase = false;
            LanguageID = null;
        }

        public virtual void CopyBaseKeyMatcher(BaseKeyMatcher other)
        {
            if (other.Patterns != null)
                Patterns = new List<object>(other.Patterns);
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
            {
                object pattern = Patterns[index];

                if (pattern is string)
                    Patterns[index] = TextUtilities.GetNormalizedString(pattern as string);
            }
        }

        public override bool Match(object obj)
        {
            if ((MatchType == MatchCode.Any) || (Patterns == null) || (Patterns.Count() == 0) || (Patterns[0] == null))
                return true;

            object firstPattern = Patterns[0];

            if (firstPattern.GetType() == typeof(string))
            {
                if (String.IsNullOrEmpty(firstPattern as string))
                    return true;
            }

            if (obj == null)
                return false;

            Type patternType = Patterns[0].GetType();

            string stringObj = obj as string;

            if (stringObj == null)
            {
                IBaseObjectKeyed baseObj;

                if ((baseObj = obj as IBaseObjectKeyed) != null)
                {
                    obj = baseObj.Key;
                    stringObj = obj as string;
                }
            }

            Type objType = obj.GetType();

            if (objType != patternType)
                return false;

            if (stringObj != null)
            {
#if !PORTABLE
                CultureInfo cultureInfo = null;
#endif

                stringObj = TextUtilities.GetNormalizedString(stringObj);

                if (IgnoreCase)
                {
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
                            if ((string.Compare(stringObj, Patterns[0] as string) >= 0) && (string.Compare(stringObj, Patterns[1] as string) <= 0))
                                return true;
                        }
                        else
                            throw new ObjectException("BaseKeyMatcher.Match:  New 2 patterns.");
                        break;
                    case MatchCode.Outside:
                        if (Patterns.Count() == 2)
                        {
                            if ((string.Compare(stringObj, Patterns[0] as string) < 0) || (string.Compare(stringObj, Patterns[1] as string) > 0))
                                return true;
                        }
                        else
                            throw new ObjectException("BaseKeyMatcher.Match:  Need 2 patterns.");
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
            else if (objType == typeof(int))
            {
                int numberObj = (int)obj;

                switch (MatchType)
                {
                    case MatchCode.Between:
                        if (Patterns.Count() == 2)
                        {
                            if ((numberObj >= (int)Patterns[0]) && (numberObj <= (int)Patterns[1]))
                                return true;
                        }
                        else
                            throw new ObjectException("BaseKeyMatcher.Match:  Need 2 patterns.");
                        break;
                    case MatchCode.Outside:
                        if (Patterns.Count() == 2)
                        {
                            if ((numberObj < (int)Patterns[0]) || (numberObj > (int)Patterns[1]))
                                return true;
                        }
                        else
                            throw new ObjectException("BaseKeyMatcher.Match:  Need 2 patterns.");
                        break;
                    default:
                        foreach (object objPattern in Patterns)
                        {
                            int pattern = (int)objPattern;

                            switch (MatchType)
                            {
                                case MatchCode.Any:
                                    return true;
                                case MatchCode.Exact:
                                    if (numberObj == pattern)
                                        return true;
                                    break;
                                case MatchCode.StartsWith:
                                    if (numberObj.ToString().StartsWith(pattern.ToString()))
                                        return true;
                                    break;
                                case MatchCode.EndsWith:
                                    if (numberObj.ToString().EndsWith(pattern.ToString()))
                                        return true;
                                    break;
                                case MatchCode.Contains:
                                    if (numberObj.ToString().Contains(pattern.ToString()))
                                        return true;
                                    break;
                                case MatchCode.Fuzzy:
                                    // Punt for now.
                                    if (numberObj.ToString().Contains(pattern.ToString()))
                                        return true;
                                    break;
                                case MatchCode.RegEx:
                                    if (numberObj == pattern)
                                        return true;
                                    break;
                                case MatchCode.Not:
                                    if (numberObj != pattern)
                                        return true;
                                    break;
                                case MatchCode.Greater:
                                    if (numberObj > pattern)
                                        return true;
                                    break;
                                case MatchCode.GreaterOrEqual:
                                    if (numberObj >= pattern)
                                        return true;
                                    break;
                                case MatchCode.Less:
                                    if (numberObj < pattern)
                                        return true;
                                    break;
                                case MatchCode.LessOrEqual:
                                    if (numberObj <= pattern)
                                        return true;
                                    break;
                                default:
                                    throw new ObjectException("BaseKeyMatcher.Match:  Unexpected match type.");
                            }
                        }
                        break;
                }

                return false;
            }
            else if (objType == typeof(float))
            {
                float numberObj = 0;

                switch (MatchType)
                {
                    case MatchCode.Between:
                        if (Patterns.Count() == 2)
                        {
                            if ((numberObj >= (float)Patterns[0]) && (numberObj <= (float)Patterns[1]))
                                return true;
                        }
                        else
                            throw new ObjectException("BaseKeyMatcher.Match:  Need 2 patterns.");
                        break;
                    case MatchCode.Outside:
                        if (Patterns.Count() == 2)
                        {
                            if ((numberObj < (float)Patterns[0]) || (numberObj > (float)Patterns[1]))
                                return true;
                        }
                        else
                            throw new ObjectException("BaseKeyMatcher.Match:  Need 2 patterns.");
                        break;
                    default:
                        foreach (object objPattern in Patterns)
                        {
                            float pattern = (float)objPattern;

                            switch (MatchType)
                            {
                                case MatchCode.Any:
                                    return true;
                                case MatchCode.Exact:
                                    if (numberObj == pattern)
                                        return true;
                                    break;
                                case MatchCode.StartsWith:
                                    if (numberObj.ToString().StartsWith(pattern.ToString()))
                                        return true;
                                    break;
                                case MatchCode.EndsWith:
                                    if (numberObj.ToString().EndsWith(pattern.ToString()))
                                        return true;
                                    break;
                                case MatchCode.Contains:
                                    if (numberObj.ToString().Contains(pattern.ToString()))
                                        return true;
                                    break;
                                case MatchCode.Fuzzy:
                                    // Punt for now.
                                    if (numberObj.ToString().Contains(pattern.ToString()))
                                        return true;
                                    break;
                                case MatchCode.RegEx:
                                    if (numberObj == pattern)
                                        return true;
                                    break;
                                case MatchCode.Not:
                                    if (numberObj != pattern)
                                        return true;
                                    break;
                                case MatchCode.Greater:
                                    if (numberObj > pattern)
                                        return true;
                                    break;
                                case MatchCode.GreaterOrEqual:
                                    if (numberObj >= pattern)
                                        return true;
                                    break;
                                case MatchCode.Less:
                                    if (numberObj < pattern)
                                        return true;
                                    break;
                                case MatchCode.LessOrEqual:
                                    if (numberObj <= pattern)
                                        return true;
                                    break;
                                default:
                                    throw new ObjectException("BaseKeyMatcher.Match:  Unexpected match type.");
                            }
                        }
                        break;
                }
            }

            return false;
        }

        public static List<object> BasesToKeys(List<IBaseObjectKeyed> bases)
        {
            if (bases == null)
                return new List<object>();

            List<object> keys = new List<object>(bases.Count());

            foreach (IBaseObjectKeyed obj in bases)
            {
                if (obj == null)
                    keys.Add("");
                else
                    keys.Add(obj.Key);
            }

            return keys;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (Patterns != null)
            {
                XElement subElement = new XElement("Patterns");
                foreach (object pattern in Patterns)
                {
                    XElement subSubElement = new XElement("Pattern", pattern);
                    subSubElement.Add(new XAttribute("Type", pattern.GetType().Name));
                    subElement.Add(subSubElement);
                }
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
                        Patterns = new List<object>(elements.Count());
                        foreach (XElement subElement in elements)
                        {
                            XAttribute typeAttribute = subElement.FirstAttribute;
                            string typeString = typeAttribute.Value;
                            object pattern = ObjectUtilities.GetKeyFromString(subElement.Value, typeString);
                            Patterns.Add(pattern);
                        }
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
