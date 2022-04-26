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
    public class ForumPosting : BaseObjectKeyed
    {
        protected object _HeadingKey;
        protected object _TopicKey;
        protected MultiLanguageString _Text;
        protected DateTime _Created;
        protected DateTime _Last;
        protected string _Owner;
        protected int _Index;
        protected int _Level;
        protected object _PostingParentKey;

        public ForumPosting(
                object key,
                object headingKey,
                object topicKey,
                MultiLanguageString text,
                string owner,
                int index,
                int level,
                object postingParentKey)
            : base(key)
        {
            _HeadingKey = headingKey;
            _TopicKey = topicKey;
            _Text = text;
            _Created = DateTime.MinValue;
            _Last = DateTime.MinValue;
            _Owner = owner;
            _Index = index;
            _Level = level;
            _PostingParentKey = postingParentKey;
        }

        public ForumPosting(ForumPosting other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public ForumPosting(XElement element)
        {
            OnElement(element);
        }

        public ForumPosting()
        {
            ClearForumPosting();
        }

        public override void Clear()
        {
            base.Clear();
            ClearForumPosting();
        }

        public void ClearForumPosting()
        {
            _HeadingKey = null;
            _TopicKey = null;
            _Text = null;
            _Created = DateTime.MinValue;
            _Last = DateTime.MinValue;
            _Owner = null;
            _Index = -1;
            _Level = 0;
            _PostingParentKey = null;
        }

        public virtual void Copy(ForumPosting other)
        {
            _HeadingKey = other.HeadingKey;
            _TopicKey = other.TopicKey;
            _Text = other.Text;
            _Created = other.Created;
            _Last = other.Last;
            _Owner = other.Owner;
            _Index = other.Index;
            _Level = other.Level;
            _PostingParentKey = other.PostingParentKey;
        }

        public override IBaseObject Clone()
        {
            return new ForumPosting(this);
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

        public object TopicKey
        {
            get
            {
                return _TopicKey;
            }
            set
            {
                if (value != _TopicKey)
                {
                    _TopicKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public MultiLanguageString Text
        {
            get
            {
                return _Text;
            }
            set
            {
                if (value != _Text)
                {
                    _Text = value;
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

        public override string Owner
        {
            get
            {
                return _Owner;
            }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int Index
        {
            get
            {
                return _Index;
            }
            set
            {
                if (_Index != value)
                {
                    _Index = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int Level
        {
            get
            {
                return _Level;
            }
            set
            {
                if (_Level != value)
                {
                    _Level = value;
                    ModifiedFlag = true;
                }
            }
        }

        public object PostingParentKey
        {
            get
            {
                return _PostingParentKey;
            }
            set
            {
                if (value != _PostingParentKey)
                {
                    _PostingParentKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int PostingParentIntKey
        {
            get
            {
                if (_PostingParentKey != null)
                    return (int)_PostingParentKey;
                return -1;
            }
            set
            {
                if (value != PostingParentIntKey)
                {
                    _PostingParentKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string PostingParentStringKey
        {
            get
            {
                if (_PostingParentKey != null)
                    return PostingParentIntKey.ToString();
                return "";
            }
            set
            {
                if (value != PostingParentStringKey)
                {
                    _PostingParentKey = ObjectUtilities.GetIntegerFromString(value, 0);
                    ModifiedFlag = true;
                }
            }
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if ((_Text != null) && _Text.Modified)
                    return true;

                return false;
            }
            set
            {
                base.Modified = value;

                if (_Text != null)
                    _Text.Modified = false;
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_HeadingKey != null)
                element.Add(new XElement("HeadingKey", _HeadingKey.ToString()));
            if (_TopicKey != null)
                element.Add(new XElement("TopicKey", _TopicKey.ToString()));
            if (_Text != null)
                element.Add(_Text.GetElement("Text"));
            element.Add(new XElement("Created", ObjectUtilities.GetStringFromDateTime(_Created)));
            element.Add(new XElement("Last", ObjectUtilities.GetStringFromDateTime(_Last)));
            if (_Owner != null)
                element.Add(new XElement("Owner", _Owner));
            element.Add(new XElement("Index", _Index.ToString()));
            if (_Level != 0)
                element.Add(new XElement("Level", _Level.ToString()));
            if (_PostingParentKey != null)
                element.Add(new XElement("PostingParentKey", _PostingParentKey.ToString()));
            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "HeadingKey":
                    _HeadingKey = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "TopicKey":
                    _TopicKey = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "Text":
                    _Text = new MultiLanguageString(childElement);
                    break;
                case "Created":
                    _Created = ObjectUtilities.GetDateTimeFromString(childElement.Value.Trim(), DateTime.MinValue);
                    break;
                case "Last":
                    _Last = ObjectUtilities.GetDateTimeFromString(childElement.Value.Trim(), DateTime.MinValue);
                    break;
                case "Owner":
                    _Owner = childElement.Value.Trim();
                    break;
                case "Index":
                    _Index = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "Level":
                    _Level = Convert.ToInt32(childElement.Value.Trim());
                    break;
                case "PostingParentKey":
                    _PostingParentKey = Convert.ToInt32(childElement.Value.Trim());
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override void OnFixup(FixupDictionary fixups)
        {
            base.OnFixup(fixups);

            if (TopicKey != null)
            {
                string xmlKey = TopicKey.ToString();

                if (!String.IsNullOrEmpty(xmlKey))
                {
                    IBaseObjectKeyed target = fixups.Get(Source, xmlKey);

                    if (target != null)
                    {
                        TopicKey = target.Key;
                    }
                }
            }

            if (PostingParentKey != null)
            {
                string xmlKey = PostingParentKey.ToString();

                if (!String.IsNullOrEmpty(xmlKey))
                {
                    IBaseObjectKeyed target = fixups.Get(Source, xmlKey);

                    if (target != null)
                    {
                        PostingParentKey = target.Key;
                    }
                }
            }
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ForumPosting otherObject = other as ForumPosting;

            if (otherObject == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareObjects(_HeadingKey, otherObject.HeadingKey);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareObjects(_TopicKey, otherObject.TopicKey);
            if (diff != 0)
                return diff;
            diff = MultiLanguageString.Compare(_Text, otherObject.Text);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareDateTimes(_Created, otherObject.Created);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareDateTimes(_Last, otherObject.Last);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Owner, otherObject.Owner);
            if (diff != 0)
                return diff;
            diff = _Index - otherObject.Index;
            if (diff != 0)
                return diff;
            diff = _Level - otherObject.Level;
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareObjects(_PostingParentKey, otherObject.PostingParentKey);
            return diff;
        }

        public static int Compare(ForumPosting object1, ForumPosting object2)
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
