using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Matchers
{
    public partial class GuidMatcher : Matcher
    {
        public List<Guid> Patterns { get; set; }

        public GuidMatcher(Guid pattern, string memberName, MatchCode matchType, int page, int pageSize)
            : base(memberName, matchType, page, pageSize)
        {
            Patterns = new List<Guid>(1) { pattern };
        }

        public GuidMatcher(List<Guid> patterns, string memberName, MatchCode matchType, int page, int pageSize)
            : base(memberName, matchType, page, pageSize)
        {
            Patterns = patterns;
        }

        public GuidMatcher(GuidMatcher other)
        {
            CopyGuidMatcher(other);
        }

        public GuidMatcher(XElement element)
        {
            OnElement(element);
        }

        public GuidMatcher()
        {
            ClearGuidMatcher();
        }

        public override void Clear()
        {
            base.Clear();
            ClearGuidMatcher();
        }

        public void ClearGuidMatcher()
        {
            Patterns = null;
        }

        public virtual void CopyGuidMatcher(GuidMatcher other)
        {
            if (other.Patterns != null)
                Patterns = new List<Guid>(other.Patterns);
            else
                Patterns = null;
        }

        public override bool Match(object obj)
        {
            if ((Patterns == null) || (Patterns.Count() == 0))
                return true;

            if (obj == null)
                return false;

            Guid numberObj = Guid.Empty;

            if (obj.GetType() == typeof(Guid))
                numberObj = (Guid)obj;
            else if (obj.GetType() == typeof(string))
                numberObj = Guid.Parse((string)obj);
            else
            {
                IBaseObjectKeyed baseObj = obj as IBaseObjectKeyed;

                if (baseObj != null)
                {
                    object subObject = ObjectTypes.GetMemberValue(baseObj, MemberName);

                    if (subObject != null)
                    {
                        if (subObject.GetType() == typeof(Guid))
                            numberObj = (Guid)subObject;
                        else if (subObject.GetType() == typeof(string))
                            numberObj = Guid.Parse((string)subObject);
                    }
                }
            }

            switch (MatchType)
            {
                case MatchCode.Between:
                    if (Patterns.Count() == 2)
                    {
                        if ((numberObj.CompareTo(Patterns[0]) >= 0) && (numberObj.CompareTo(Patterns[1]) <= 0))
                            return true;
                    }
                    else
                        throw new ObjectException("GuidMatcher.Match:  Need 2 patterns.");
                    break;
                case MatchCode.Outside:
                    if (Patterns.Count() == 2)
                    {
                        if ((numberObj.CompareTo(Patterns[0]) < 0) || (numberObj.CompareTo(Patterns[1]) > 0))
                            return true;
                    }
                    else
                        throw new ObjectException("GuidMatcher.Match:  Need 2 patterns.");
                    break;
                default:
                    foreach (Guid pattern in Patterns)
                    {
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
                                if (numberObj.CompareTo(pattern) > 0)
                                    return true;
                                break;
                            case MatchCode.GreaterOrEqual:
                                if (numberObj.CompareTo(pattern) >= 0)
                                    return true;
                                break;
                            case MatchCode.Less:
                                if (numberObj.CompareTo(pattern) < 0)
                                    return true;
                                break;
                            case MatchCode.LessOrEqual:
                                if (numberObj.CompareTo(pattern) <= 0)
                                    return true;
                                break;
                            default:
                                throw new ObjectException("GuidMatcher.Match:  Unexpected match type.");
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
                foreach (Guid pattern in Patterns)
                    subElement.Add(new XElement("Pattern", pattern.ToString()));
                element.Add(subElement);
            }
            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Patterns":
                    {
                        IEnumerable<XElement> elements = childElement.Elements();
                        Patterns = new List<Guid>(elements.Count());
                        foreach (XElement subElement in elements)
                            Patterns.Add(Guid.Parse(subElement.Value));
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
