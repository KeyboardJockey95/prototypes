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
    public partial class IntMatcher : Matcher
    {
        public List<int> Patterns { get; set; }

        public IntMatcher(int pattern, string memberName, MatchCode matchType, int page, int pageSize)
            : base(memberName, matchType, page, pageSize)
        {
            Patterns = new List<int>(1) { pattern };
        }

        public IntMatcher(List<int> patterns, string memberName, MatchCode matchType, int page, int pageSize)
            : base(memberName, matchType, page, pageSize)
        {
            Patterns = patterns;
        }

        public IntMatcher(IntMatcher other)
        {
            CopyIntMatcher(other);
        }

        public IntMatcher(XElement element)
        {
            OnElement(element);
        }

        public IntMatcher()
        {
            ClearIntMatcher();
        }

        public override void Clear()
        {
            base.Clear();
            ClearIntMatcher();
        }

        public void ClearIntMatcher()
        {
            Patterns = null;
        }

        public virtual void CopyIntMatcher(IntMatcher other)
        {
            if (other.Patterns != null)
                Patterns = new List<int>(other.Patterns);
            else
                Patterns = null;
        }

        public override bool Match(object obj)
        {
            if ((Patterns == null) || (Patterns.Count() == 0))
                return true;

            if (obj == null)
                return false;

            int numberObj = 0;

            if (obj.GetType() == typeof(int))
                numberObj = (int)obj;
            else if (obj.GetType() == typeof(string))
                numberObj = Convert.ToInt32((string)obj);
            else
            {
                IBaseObjectKeyed baseObj = obj as IBaseObjectKeyed;

                if (baseObj != null)
                {
                    object subObject = ObjectTypes.GetMemberValue(baseObj, MemberName);

                    if (subObject != null)
                    {
                        if (subObject.GetType() == typeof(int))
                            numberObj = (int)subObject;
                    }
                }
            }

            switch (MatchType)
            {
                case MatchCode.Between:
                    if (Patterns.Count() == 2)
                    {
                        if ((numberObj >= Patterns[0]) && (numberObj <= Patterns[1]))
                            return true;
                    }
                    else
                        throw new ObjectException("IntMatcher.Match:  Need 2 patterns.");
                    break;
                case MatchCode.Outside:
                    if (Patterns.Count() == 2)
                    {
                        if ((numberObj < Patterns[0]) || (numberObj > Patterns[1]))
                            return true;
                    }
                    else
                        throw new ObjectException("IntMatcher.Match:  Need 2 patterns.");
                    break;
                default:
                    foreach (int pattern in Patterns)
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
                                throw new ObjectException("IntMatcher.Match:  Unexpected match type.");
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
                foreach (int pattern in Patterns)
                    subElement.Add(new XElement("Pattern", pattern));
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
                        Patterns = new List<int>(elements.Count());
                        foreach (XElement subElement in elements)
                            Patterns.Add(Convert.ToInt32(subElement.Value));
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
