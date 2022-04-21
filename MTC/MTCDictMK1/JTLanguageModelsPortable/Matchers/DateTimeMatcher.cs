using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Matchers
{
    public partial class DateTimeMatcher : Matcher
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public DateTimeMatcher(DateTime start, DateTime end,
            string memberName, MatchCode matchType, int page, int pageSize)
            : base(memberName, matchType, page, pageSize)
        {
            Start = start;
            End = end;
        }

        public DateTimeMatcher(DateTimeMatcher other)
        {
            CopyDateTimeMatcher(other);
        }

        public DateTimeMatcher(XElement element)
        {
            OnElement(element);
        }

        public DateTimeMatcher()
        {
            ClearDateTimeMatcher();
        }

        public override void Clear()
        {
            base.Clear();
            ClearDateTimeMatcher();
        }

        public void ClearDateTimeMatcher()
        {
            Start = DateTime.MinValue;
            End = DateTime.MaxValue;
        }

        public virtual void CopyDateTimeMatcher(DateTimeMatcher other)
        {
            Start = other.Start;
            End = other.End;
        }

        public override bool Match(object obj)
        {
            if ((Start == DateTime.MinValue) && (End == DateTime.MaxValue))
                return true;

            if (obj == null)
                return false;

            DateTime dateTimeObj = DateTime.MinValue;

            if (obj.GetType() == typeof(DateTime))
                dateTimeObj = (DateTime)obj;
            else if (obj.GetType() == typeof(string))
                dateTimeObj = Convert.ToDateTime((string)obj);
            else
            {
                IBaseObjectKeyed baseObj = obj as IBaseObjectKeyed;

                if (baseObj != null)
                {
                    object subObject = ObjectTypes.GetMemberValue(baseObj, MemberName);

                    if (subObject != null)
                    {
                        if (subObject.GetType() == typeof(DateTime))
                            dateTimeObj = (DateTime)subObject;
                    }
                }
            }

            switch (MatchType)
            {
                case MatchCode.Between:
                    if ((dateTimeObj >= Start) && (dateTimeObj <= End))
                        return true;
                    break;
                case MatchCode.Outside:
                    if ((dateTimeObj < Start) || (dateTimeObj > End))
                        return true;
                    break;
                default:
                    DateTime pattern = Start;
                    switch (MatchType)
                    {
                        case MatchCode.Any:
                            return true;
                        case MatchCode.Exact:
                            if (dateTimeObj == pattern)
                                return true;
                            break;
                        case MatchCode.StartsWith:
                            if (dateTimeObj.ToString().StartsWith(pattern.ToString()))
                                return true;
                            break;
                        case MatchCode.EndsWith:
                            if (dateTimeObj.ToString().EndsWith(pattern.ToString()))
                                return true;
                            break;
                        case MatchCode.Contains:
                            if (dateTimeObj.ToString().Contains(pattern.ToString()))
                                return true;
                            break;
                        case MatchCode.Fuzzy:
                            // Punt for now.
                            if (dateTimeObj.ToString().Contains(pattern.ToString()))
                                return true;
                            break;
                        case MatchCode.RegEx:
                            if (dateTimeObj == pattern)
                                return true;
                            break;
                        case MatchCode.Not:
                            if (dateTimeObj != pattern)
                                return true;
                            break;
                        case MatchCode.Greater:
                            if (dateTimeObj > pattern)
                                return true;
                            break;
                        case MatchCode.GreaterOrEqual:
                            if (dateTimeObj >= pattern)
                                return true;
                            break;
                        case MatchCode.Less:
                            if (dateTimeObj < pattern)
                                return true;
                            break;
                        case MatchCode.LessOrEqual:
                            if (dateTimeObj <= pattern)
                                return true;
                            break;
                        default:
                            throw new ObjectException("DateTimeMatcher.Match:  Unexpected match type.");
                    }
                    break;
            }

            return false;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            element.Add(new XElement("Start", ObjectUtilities.GetStringFromDateTime(Start)));
            element.Add(new XElement("End", ObjectUtilities.GetStringFromDateTime(End)));
            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Start":
                    Start = ObjectUtilities.GetDateTimeFromString(childElement.Value.Trim(), DateTime.MinValue);
                    break;
                case "End":
                    Start = ObjectUtilities.GetDateTimeFromString(childElement.Value.Trim(), DateTime.MinValue);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
