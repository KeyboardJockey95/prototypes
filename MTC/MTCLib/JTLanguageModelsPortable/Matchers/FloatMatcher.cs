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
    public partial class FloatMatcher : Matcher
    {
        public List<float> Patterns { get; set; }

        public FloatMatcher(float pattern, string memberName, MatchCode matchType, int page, int pageSize)
            : base(memberName, matchType, page, pageSize)
        {
            Patterns = new List<float>(1) { pattern };
        }

        public FloatMatcher(List<float> patterns, string memberName, MatchCode matchType, int page, int pageSize)
            : base(memberName, matchType, page, pageSize)
        {
            Patterns = patterns;
        }

        public FloatMatcher(FloatMatcher other)
        {
            CopyFloatMatcher(other);
        }

        public FloatMatcher(XElement element)
        {
            OnElement(element);
        }

        public FloatMatcher()
        {
            ClearFloatMatcher();
        }

        public override void Clear()
        {
            base.Clear();
            ClearFloatMatcher();
        }

        public void ClearFloatMatcher()
        {
            Patterns = null;
        }

        public virtual void CopyFloatMatcher(FloatMatcher other)
        {
            if (other.Patterns != null)
                Patterns = new List<float>(other.Patterns);
            else
                Patterns = null;
        }

        public override bool Match(object obj)
        {
            if ((Patterns == null) || (Patterns.Count() == 0))
                return true;

            if (obj == null)
                return false;

            float numberObj = 0;

            if (obj.GetType() == typeof(float))
                numberObj = (float)obj;
            else if (obj.GetType() == typeof(int))
                numberObj = (float)(int)obj;
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
                        if (subObject.GetType() == typeof(float))
                            numberObj = (float)subObject;
                        else if (subObject.GetType() == typeof(int))
                            numberObj = (float)(int)subObject;
                        else if (subObject.GetType() == typeof(string))
                            numberObj = Convert.ToInt32((string)subObject);
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
                        throw new ObjectException("FloatMatcher.Match:  Need 2 patterns.");
                    break;
                case MatchCode.Outside:
                    if (Patterns.Count() == 2)
                    {
                        if ((numberObj < Patterns[0]) || (numberObj > Patterns[1]))
                            return true;
                    }
                    else
                        throw new ObjectException("FloatMatcher.Match:  Need 2 patterns.");
                    break;
                default:
                    foreach (float pattern in Patterns)
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
                                throw new ObjectException("FloatMatcher.Match:  Unexpected match type.");
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
                foreach (float pattern in Patterns)
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
                        Patterns = new List<float>(elements.Count());
                        foreach (XElement subElement in elements)
                            Patterns.Add(Convert.ToSingle(subElement.Value));
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
