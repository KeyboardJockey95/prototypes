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
    public enum MatchCode
    {
        Any,
        Exact,
        StartsWith,
        EndsWith,
        Contains,
        ContainsWord,
        Fuzzy,
        RegEx,
        Or,
        And,
        Xor,
        Not,
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual,
        Between,
        Outside,
        ParseBest,
        ParseAll,
        CustomBase
    }

    public partial class Matcher : BaseObject
    {
        protected string _MemberName;
        protected MatchCode _MatchType;
        protected int _Page;
        protected int _PageSize;
        protected List<Matcher> _Matchers;

        public Matcher(string memberName, MatchCode matchType, int page, int pageSize)
        {
            _MemberName = memberName;
            _MatchType = matchType;
            _Page = page;
            _PageSize = pageSize;
            _Matchers = null;
        }

        public Matcher(Matcher matcher, string memberName, MatchCode matchType, int page, int pageSize)
        {
            _MemberName = memberName;
            _MatchType = matchType;
            _Page = page;
            _PageSize = pageSize;

            if (matcher != null)
                _Matchers = new List<Matcher>(1) { matcher };
            else
                _Matchers = null;
        }

        public Matcher(List<Matcher> matchers, string memberName, MatchCode matchType, int page, int pageSize)
        {
            _MemberName = memberName;
            _MatchType = matchType;
            _Page = page;
            _PageSize = pageSize;
            _Matchers = matchers;
        }

        public Matcher(Matcher other)
        {
            CopyMatcher(other);
        }

        public Matcher(XElement element)
        {
            OnElement(element);
        }

        public Matcher()
        {
            ClearMatcher();
        }

        public override void Clear()
        {
            base.Clear();
            ClearMatcher();
        }

        public void ClearMatcher()
        {
            _MemberName = null;
            _MatchType = MatchCode.Any;
            _Page = 0;
            _PageSize = 0;
            _Matchers = null;
        }

        public virtual void CopyMatcher(Matcher other)
        {
            _MemberName = other.MemberName;
            _MatchType = other.MatchType;
            _Page = other.Page;
            _PageSize = other.PageSize;
            if (other.Matchers != null)
                _Matchers = new List<Matcher>(other.Matchers);
            else
                _Matchers = null;
        }

        public string MemberName
        {
            get
            {
                return _MemberName;
            }
            set
            {
                _MemberName = value;
            }
        }

        public MatchCode MatchType
        {
            get
            {
                return _MatchType;
            }
            set
            {
                _MatchType = value;
            }
        }

        public int Page
        {
            get
            {
                return _Page;
            }
            set
            {
                _Page = value;
            }
        }

        public int PageSize
        {
            get
            {
                return _PageSize;
            }
            set
            {
                _PageSize = value;
            }
        }

        public List<Matcher> Matchers
        {
            get
            {
                return _Matchers;
            }
            set
            {
                _Matchers = value;
            }
        }

        public virtual bool Match(object obj)
        {
            throw new ObjectException("Matcher.Match should not be reached.");
        }

        public virtual bool MatchWithPaging(object obj, int indexIn, out int indexOut)
        {
            indexOut = indexIn;

            if (Match(obj))
            {
                if (PageSize == 0)
                    return true;

                if (((indexIn / PageSize) + 1) == Page)
                {
                    indexOut = indexIn + 1;
                    return true;
                }
            }

            return false;
        }

        public static bool MatchStrings(MatchCode matchType, string pattern, string stringObj)
        {
            if (pattern == null)
                pattern = String.Empty;

            if (stringObj == null)
                stringObj = String.Empty;

            switch (matchType)
            {
                case MatchCode.Any:
                    return true;
                case MatchCode.Exact:
                    if (stringObj == pattern)
                        return true;
                    break;
                case MatchCode.StartsWith:
                    if (stringObj.StartsWith(pattern))
                        return true;
                    break;
                case MatchCode.EndsWith:
                    if (stringObj.EndsWith(pattern))
                        return true;
                    break;
                case MatchCode.Contains:
                    if (stringObj.Contains(pattern))
                        return true;
                    break;
                case MatchCode.ContainsWord:
                    if (TextUtilities.ContainsWholeWordCaseInsensitive(stringObj, pattern))
                        return true;
                    break;
                case MatchCode.Fuzzy:
                    // Punt for now.
                    if (stringObj.Contains(pattern))
                        return true;
                    break;
                case MatchCode.RegEx:
                    try
                    {
                        return Regex.IsMatch(stringObj, pattern);
                    }
                    catch
                    {
                    }
                    break;
                case MatchCode.Not:
                    if (stringObj != pattern)
                        return true;
                    break;
                case MatchCode.Greater:
                    if (string.Compare(stringObj, pattern) > 0)
                        return true;
                    break;
                case MatchCode.GreaterOrEqual:
                    if (string.Compare(stringObj, pattern) >= 0)
                        return true;
                    break;
                case MatchCode.Less:
                    if (string.Compare(stringObj, pattern) < 0)
                        return true;
                    break;
                case MatchCode.LessOrEqual:
                    if (string.Compare(stringObj, pattern) <= 0)
                        return true;
                    break;
                default:
                    throw new ObjectException("Matcher.MatchStrings:  Unexpected match type.");
            }

            return false;
        }

        public static bool MatchDelimitedStrings(MatchCode matchType, string pattern, string stringObj, string delimiter)
        {
            if (pattern == null)
                pattern = String.Empty;

            if (stringObj == null)
                stringObj = String.Empty;

            switch (matchType)
            {
                case MatchCode.Any:
                    return true;
                case MatchCode.Exact:
                    if (pattern == stringObj)
                        return true;
                    if (stringObj == delimiter + pattern + delimiter)
                        return true;
                    break;
                case MatchCode.StartsWith:
                    if (pattern == stringObj)
                        return true;
                    if (stringObj.StartsWith(delimiter + pattern))
                        return true;
                    break;
                case MatchCode.EndsWith:
                    if (pattern == stringObj)
                        return true;
                    if (stringObj.EndsWith(pattern + delimiter))
                        return true;
                    break;
                case MatchCode.Contains:
                    if (String.IsNullOrEmpty(pattern))
                        return true;
                    if (stringObj.Contains(delimiter + pattern + delimiter))
                        return true;
                    break;
                case MatchCode.ContainsWord:
                    if (TextUtilities.ContainsWholeWordCaseInsensitive(stringObj, delimiter + pattern + delimiter))
                        return true;
                    break;
                case MatchCode.Fuzzy:
                    // Punt for now.
                    if (String.IsNullOrEmpty(pattern))
                        return true;
                    if (stringObj.Contains(delimiter + pattern + delimiter))
                        return true;
                    break;
                case MatchCode.RegEx:
                    if (String.IsNullOrEmpty(pattern))
                        return true;
                    if (String.IsNullOrEmpty(stringObj))
                        return false;
                    if (delimiter == "|")
                        delimiter = "\\|";
                    try
                    {
                        return Regex.IsMatch(stringObj, "^.*" + delimiter + pattern + delimiter + ".*$");
                    }
                    catch
                    {
                    }
                    break;
                case MatchCode.Not:
                    if (pattern == stringObj)
                        return false;
                    if (stringObj != delimiter + pattern + delimiter)
                        return true;
                    break;
                case MatchCode.Greater:
                    if (pattern == stringObj)
                        return false;
                    if (string.Compare(stringObj, delimiter + pattern + delimiter) > 0)
                        return true;
                    break;
                case MatchCode.GreaterOrEqual:
                    if (pattern == stringObj)
                        return true;
                    if (string.Compare(stringObj, delimiter + pattern + delimiter) >= 0)
                        return true;
                    break;
                case MatchCode.Less:
                    if (string.Compare(stringObj, delimiter + pattern + delimiter) < 0)
                        return false;
                    break;
                case MatchCode.LessOrEqual:
                    if (pattern == stringObj)
                        return true;
                    if (string.Compare(stringObj, delimiter + pattern + delimiter) <= 0)
                        return true;
                    break;
                default:
                    throw new ObjectException("Matcher.MatchDelimitedStrings:  Unexpected match type.");
            }

            return false;
        }

        public virtual MatchCode GetMatchCodeFromString(string str)
        {
            return GetMatchCodeFromStringStatic(str);
        }

        public static MatchCode GetMatchCodeFromStringStatic(string str)
        {
            switch (str)
            {
                case "":
                case null:
                    return MatchCode.Any;
                case "Any":
                    return MatchCode.Any;
                case "Exact":
                    return MatchCode.Exact;
                case "StartsWith":
                    return MatchCode.StartsWith;
                case "EndsWith":
                    return MatchCode.EndsWith;
                case "Contains":
                    return MatchCode.Contains;
                case "ContainsWord":
                    return MatchCode.ContainsWord;
                case "Fuzzy":
                    return MatchCode.Fuzzy;
                case "RegEx":
                    return MatchCode.RegEx;
                case "Or":
                    return MatchCode.Or;
                case "And":
                    return MatchCode.And;
                case "Xor":
                    return MatchCode.Xor;
                case "Not":
                    return MatchCode.Not;
                case "Greater":
                    return MatchCode.Greater;
                case "GreaterOrEqual":
                    return MatchCode.GreaterOrEqual;
                case "Less":
                    return MatchCode.Less;
                case "LessOrEqual":
                    return MatchCode.LessOrEqual;
                case "Between":
                    return MatchCode.Between;
                case "Outside":
                    return MatchCode.Outside;
                case "ParseBest":
                    return MatchCode.ParseBest;
                case "ParseAll":
                    return MatchCode.ParseAll;
                default:
                    throw new ObjectException("Matcher.GetMatchCodeFromString: Unknown match code string \"" + str + "\".");
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if (MemberName != null)
                element.Add(new XAttribute("MemberName", MemberName));

            element.Add(new XAttribute("MatchType", MatchType));

            if (Page != 0)
                element.Add(new XAttribute("Page", Page));

            if (PageSize != 0)
                element.Add(new XAttribute("PageSize", PageSize));

            if ((Matchers != null) && (Matchers.Count() != 0))
            {
                XElement matchersElement = new XElement("Matchers");

                foreach (Matcher matcher in Matchers)
                    matchersElement.Add(matcher.Xml);

                element.Add(matchersElement);
            }

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "MemberName":
                    MemberName = attributeValue;
                    break;
                case "MatchType":
                    MatchType = GetMatchCodeFromString(attributeValue);
                    break;
                case "Page":
                    if (!String.IsNullOrEmpty(attributeValue))
                        Page = Convert.ToInt32(attributeValue);
                    else
                        Page = 0;
                    break;
                case "PageSize":
                    if (!String.IsNullOrEmpty(attributeValue))
                        PageSize = Convert.ToInt32(attributeValue);
                    else
                        PageSize = 0;
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
                case "Matchers":
                    {
                        IEnumerable<XElement> elements = childElement.Elements();
                        _Matchers = new List<Matcher>(elements.Count());
                        foreach (XElement subElement in elements)
                        {
                            Matcher matcher = (Matcher)ObjectUtilities.ResurrectBaseObject(subElement);
                            if (matcher != null)
                                _Matchers.Add(matcher);
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
