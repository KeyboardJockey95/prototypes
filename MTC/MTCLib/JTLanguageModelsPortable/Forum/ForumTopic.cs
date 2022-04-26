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
    public class ForumTopic : BaseObjectTitled
    {
        protected object _HeadingKey;
        protected DateTime _Created;
        protected DateTime _Last;
        protected int _PostingCount;
        protected string _LastPoster;
        protected List<string> _Watchers;
        protected int _ViewCount;
        protected string _LastViewer;

        public ForumTopic(object key, object headingKey, MultiLanguageString title, MultiLanguageString description,
                string owner, int index)
            : base(key)
        {
            Title = title;
            Description = description;
            Owner = owner;
            Index = index;
            _HeadingKey = headingKey;
            _Created = DateTime.MinValue;
            _Last = DateTime.MinValue;
            _PostingCount = 0;
            _LastPoster = null;
            _Watchers = null;
            _ViewCount = 0;
            _LastViewer = null;
        }

        public ForumTopic(ForumTopic other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public ForumTopic(XElement element)
        {
            OnElement(element);
        }

        public ForumTopic()
        {
            ClearForumTopic();
        }

        public override void Clear()
        {
            base.Clear();
            ClearForumTopic();
        }

        public void ClearForumTopic()
        {
            _HeadingKey = null;
            _Created = DateTime.MinValue;
            _Last = DateTime.MinValue;
            _PostingCount = 0;
            _LastPoster = null;
            _Watchers = null;
            _ViewCount = 0;
            _LastViewer = null;
        }

        public virtual void Copy(ForumTopic other)
        {
            _HeadingKey = other.HeadingKey;
            _Created = other.Created;
            _Last = other.Last;
            _PostingCount = other.PostingCount;
            _LastPoster = other.LastPoster;
            _Watchers = other.Watchers;
            _ViewCount = 0;
            _LastViewer = null;
        }

        public override IBaseObject Clone()
        {
            return new ForumTopic(this);
        }

        public object HeadingKey
        {
            get
            {
                return _HeadingKey;
            }
            set
            {
                if (value != _HeadingKey)
                {
                    _HeadingKey = value;
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

        public int PostingCount
        {
            get
            {
                return _PostingCount;
            }
            set
            {
                if (value != _PostingCount)
                {
                    _PostingCount = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string LastPoster
        {
            get
            {
                return _LastPoster;
            }
            set
            {
                if (value != _LastPoster)
                {
                    _LastPoster = value;
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

        public int ViewCount
        {
            get
            {
                return _ViewCount;
            }
            set
            {
                if (value != _ViewCount)
                {
                    _ViewCount = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string LastViewer
        {
            get
            {
                return _LastViewer;
            }
            set
            {
                if (value != _LastViewer)
                {
                    _LastViewer = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_HeadingKey != null)
                element.Add(new XElement("HeadingKey", _HeadingKey.ToString()));
            element.Add(new XElement("Created", ObjectUtilities.GetStringFromDateTime(_Created)));
            element.Add(new XElement("Last", ObjectUtilities.GetStringFromDateTime(_Last)));
            element.Add(new XElement("PostingCount", _PostingCount.ToString()));
            if (_LastPoster != null)
                element.Add(new XElement("LastPoster", _LastPoster));
            if (_Watchers != null)
                element.Add(ObjectUtilities.GetElementFromStringList("Watchers", _Watchers));
            element.Add(new XElement("ViewCount", _ViewCount.ToString()));
            if (_LastViewer != null)
                element.Add(new XElement("LastViewer", _LastViewer));
            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "HeadingKey":
                    _HeadingKey = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "Created":
                    _Created = ObjectUtilities.GetDateTimeFromString(childElement.Value.Trim(), DateTime.MinValue);
                    break;
                case "Last":
                    _Last = ObjectUtilities.GetDateTimeFromString(childElement.Value.Trim(), DateTime.MinValue);
                    break;
                case "PostingCount":
                    _PostingCount = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "LastPoster":
                    _LastPoster = childElement.Value.Trim();
                    break;
                case "Watchers":
                    _Watchers = ObjectUtilities.GetStringListFromElement(childElement);
                    break;
                case "ViewCount":
                    _ViewCount = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "LastViewer":
                    _LastViewer = childElement.Value.Trim();
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override void OnFixup(FixupDictionary fixups)
        {
            base.OnFixup(fixups);

            if (HeadingKey != null)
            {
                string xmlKey = HeadingKey.ToString();

                if (!String.IsNullOrEmpty(xmlKey))
                {
                    IBaseObjectKeyed target = fixups.Get(Source, xmlKey);

                    if (target != null)
                    {
                        HeadingKey = target.Key;
                    }
                }
            }
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ForumTopic otherObject = other as ForumTopic;

            if (otherObject == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareObjects(_HeadingKey, otherObject.HeadingKey);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareDateTimes(_Created, otherObject.Created);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareDateTimes(_Last, otherObject.Last);
            if (diff != 0)
                return diff;
            diff = _PostingCount - otherObject.PostingCount;
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_LastPoster, otherObject.LastPoster);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStringLists(_Watchers, otherObject.Watchers);
            if (diff != 0)
                return diff;
            diff = _ViewCount - otherObject.ViewCount;
            if (diff != 0)
                return diff;
            return ObjectUtilities.CompareStrings(_LastViewer, otherObject.LastViewer);
        }

        public static int Compare(ForumTopic object1, ForumTopic object2)
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
