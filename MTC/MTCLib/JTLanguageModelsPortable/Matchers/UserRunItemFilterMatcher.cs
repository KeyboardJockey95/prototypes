using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;

namespace JTLanguageModelsPortable.Matchers
{
    public class UserRunItemFilterMatcher : StringMatcher
    {
        public bool IsNew { get; set; }
        public bool IsActive { get; set; }
        public bool IsLearned { get; set; }

        public UserRunItemFilterMatcher(
                string pattern,
                bool isNew,
                bool isActive,
                bool isLearned,
                string memberName,
                MatchCode matchType,
                int page,
                int pageSize)
            : base(pattern, memberName, matchType, page, pageSize)
        {
            IsNew = isNew;
            IsActive = isActive;
            IsLearned = isLearned;
        }

        public UserRunItemFilterMatcher(
                string pattern,
                bool isNew,
                bool isActive,
                bool isLearned,
                string memberName,
                MatchCode matchType,
                bool ignoreCase,
                LanguageID languageID,
                int page,
                int pageSize)
            : base(pattern, memberName, matchType, page, pageSize)
        {
            IsNew = isNew;
            IsActive = isActive;
            IsLearned = isLearned;
        }

        public UserRunItemFilterMatcher(
                List<string> patterns,
                bool isNew,
                bool isActive,
                bool isLearned,
                string memberName,
                MatchCode matchType,
                int page,
                int pageSize)
            : base(patterns, memberName, matchType, page, pageSize)
        {
            IsNew = isNew;
            IsActive = isActive;
            IsLearned = isLearned;
        }

        public UserRunItemFilterMatcher(
                List<string> patterns,
                bool isNew,
                bool isActive,
                bool isLearned,
                string memberName,
                MatchCode matchType,
                bool ignoreCase,
                LanguageID languageID,
                int page,
                int pageSize)
            : base(patterns, memberName, matchType, page, pageSize)
        {
            IsNew = isNew;
            IsActive = isActive;
            IsLearned = isLearned;
        }

        public UserRunItemFilterMatcher(UserRunItemFilterMatcher other)
            : base(other)
        {
            CopyUserRunItemFilterMatcher(other);
        }

        public UserRunItemFilterMatcher(XElement element)
        {
            OnElement(element);
        }

        public UserRunItemFilterMatcher()
        {
            ClearUserRunItemFilterMatcher();
        }

        public override void Clear()
        {
            base.Clear();
            ClearUserRunItemFilterMatcher();
        }

        public void ClearUserRunItemFilterMatcher()
        {
            IsNew = false;
            IsActive = false;
            IsLearned = false;
        }

        public virtual void CopyUserRunItemFilterMatcher(UserRunItemFilterMatcher other)
        {
            IsNew = other.IsNew;
            IsActive = other.IsActive;
            IsLearned = other.IsLearned;
        }

        public override bool Match(object obj)
        {
            bool matched;

            if ((Patterns != null) && (Patterns.Count() != 0))
                matched = base.Match(obj);
            else
                matched = true;

            if (!matched)
                return false;

            UserRunItem userRunItem = obj as UserRunItem;

            if (userRunItem != null)
            {
                matched = false;

                switch (MatchType)
                {
                    case MatchCode.Between:
                        break;
                    case MatchCode.Outside:
                        break;
                    default:
                        switch (userRunItem.UserRunState)
                        {
                            case UserRunStateCode.Future:
                                if (IsNew)
                                    matched = true;
                                break;
                            case UserRunStateCode.Active:
                                if (IsActive)
                                    matched = true;
                                break;
                            case UserRunStateCode.Learned:
                                if (IsLearned)
                                    matched = true;
                                break;
                        }
                        break;
                }
            }

            return matched;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            element.Add(new XAttribute("IsNew", IsNew.ToString()));
            element.Add(new XAttribute("IsActive", IsActive.ToString()));
            element.Add(new XAttribute("IsLearned", IsLearned.ToString()));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "IsNew":
                    IsNew = (attributeValue == "True" ? true : false);
                    break;
                case "IsActive":
                    IsActive = (attributeValue == "True" ? true : false);
                    break;
                case "IsLearned":
                    IsLearned = (attributeValue == "True" ? true : false);
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }
    }
}
