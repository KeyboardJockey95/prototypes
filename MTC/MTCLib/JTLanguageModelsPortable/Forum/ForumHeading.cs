using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.Forum
{
    public class ForumHeading : BaseObjectTitled
    {
        protected object _CategoryKey;
        protected int _TopicCount;
        protected DateTime _Created;
        protected DateTime _Last;
        protected List<string> _Watchers;

        public ForumHeading(object key, MultiLanguageString title, MultiLanguageString description,
                string owner, int index, object categoryKey, int topicCount)
            : base(key)
        {
            Title = title;
            Description = description;
            Owner = owner;
            Index = index;
            _CategoryKey = categoryKey;
            _TopicCount = topicCount;
            _Created = DateTime.MinValue;
            _Last = DateTime.MinValue;
            _Watchers = null;
        }

        public ForumHeading(ForumHeading other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public ForumHeading(XElement element)
        {
            OnElement(element);
        }

        public ForumHeading()
        {
            ClearForumHeading();
        }

        public override void Clear()
        {
            base.Clear();
            ClearForumHeading();
        }

        public void ClearForumHeading()
        {
            _CategoryKey = null;
            _TopicCount = 0;
            _Created = DateTime.MinValue;
            _Last = DateTime.MinValue;
            _Watchers = null;
        }

        public virtual void Copy(ForumHeading other)
        {
            _CategoryKey = other.CategoryKey;
            _TopicCount = other.TopicCount;
            _Created = other.Created;
            _Last = other.Last;
            _Watchers = other.Watchers;
        }

        public override IBaseObject Clone()
        {
            return new ForumHeading(this);
        }

        public object CategoryKey
        {
            get
            {
                return _CategoryKey;
            }
            set
            {
                if (value != _CategoryKey)
                {
                    _CategoryKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int TopicCount
        {
            get
            {
                return _TopicCount;
            }
            set
            {
                if (value != _TopicCount)
                {
                    _TopicCount = value;
                    ModifiedFlag = true;
                }
            }
        }

        public DateTime Created
        {
            get
            {
                return _Created;
            }
            set
            {
                if (value != _Created)
                {
                    _Created = value;
                    ModifiedFlag = true;
                }
            }
        }

        public DateTime Last
        {
            get
            {
                return _Last;
            }
            set
            {
                if (value != _Last)
                {
                    _Last = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> Watchers
        {
            get
            {
                return _Watchers;
            }
            set
            {
                if (value != _Watchers)
                {
                    _Watchers = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_CategoryKey != null)
                element.Add(new XElement("CategoryKey", _CategoryKey.ToString()));
            element.Add(new XElement("TopicCount", _TopicCount.ToString()));
            element.Add(new XElement("Created", ObjectUtilities.GetStringFromDateTime(_Created)));
            element.Add(new XElement("Last", ObjectUtilities.GetStringFromDateTime(_Last)));
            if (_Watchers != null)
                element.Add(ObjectUtilities.GetElementFromStringList("Watchers", _Watchers));
            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "CategoryKey":
                    _CategoryKey = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "TopicCount":
                    _TopicCount = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "Created":
                    _Created = ObjectUtilities.GetDateTimeFromString(childElement.Value.Trim(), DateTime.MinValue);
                    break;
                case "Last":
                    _Last = ObjectUtilities.GetDateTimeFromString(childElement.Value.Trim(), DateTime.MinValue);
                    break;
                case "Watchers":
                    _Watchers = ObjectUtilities.GetStringListFromElement(childElement);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override void OnFixup(FixupDictionary fixups)
        {
            base.OnFixup(fixups);

            if (CategoryKey != null)
            {
                string xmlKey = CategoryKey.ToString();

                if (!String.IsNullOrEmpty(xmlKey))
                {
                    IBaseObjectKeyed target = fixups.Get(Source, xmlKey);

                    if (target != null)
                    {
                        CategoryKey = target.Key;
                    }
                }
            }
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ForumHeading otherObject = other as ForumHeading;

            if (otherObject == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareObjects(_CategoryKey, otherObject.CategoryKey);
            if (diff != 0)
                return diff;
            diff = _TopicCount - otherObject.TopicCount;
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareDateTimes(_Created, otherObject.Created);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareDateTimes(_Last, otherObject.Last);
            if (diff != 0)
                return diff;
            return ObjectUtilities.CompareStringLists(_Watchers, otherObject.Watchers);
        }

        public static int Compare(ForumHeading object1, ForumHeading object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }
    }
}
