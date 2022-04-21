using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.Content
{
    public class ContentItemReference<T> : BaseObjectKeyed
        where T : BaseObjectKeyed, new()
    {
        protected object _TreeKey;
        protected object _NodeKey;
        protected string _NodeContentKey;
        protected object _ContentKey;
        protected object _ItemKey;
        protected bool _DontTrackSource;
        protected T _Item;
        protected BaseObjectNodeTree _Tree; // Not saved.
        protected BaseObjectNode _Node;     // Not saved.

        public ContentItemReference(object key, object itemKey, object contentKey, string nodeContentKey,
            object nodeKey, object treeKey, BaseObjectNode node, BaseObjectNodeTree tree, T item) : base(key)
        {
            _TreeKey = treeKey;
            _NodeKey = nodeKey;
            _NodeContentKey = nodeContentKey;
            _ContentKey = contentKey;
            _ItemKey = itemKey;
            _DontTrackSource = false;
            _Item = item;
            _Tree = tree;
            _Node = node;
        }

        public ContentItemReference(object key, object itemKey, object contentKey, string nodeContentKey,
            object nodeKey, object treeKey, T item) : base(key)
        {
            _TreeKey = treeKey;
            _NodeKey = nodeKey;
            _NodeContentKey = nodeContentKey;
            _ContentKey = contentKey;
            _ItemKey = itemKey;
            _DontTrackSource = false;
            _Item = item;
            _Tree = null;
            _Node = null;
        }

        public ContentItemReference(ContentItemReference<T> other)
            : base(other)
        {
            _TreeKey = other.TreeKey;
            _NodeKey = other.NodeKey;
            _NodeContentKey = other.NodeContentKey;
            _ContentKey = other.ContentKey;
            _ItemKey = other.ItemKey;
            _DontTrackSource = false;
            _Item = other.Item;
            _Tree = other.Tree;
            _Node = other.Node;
        }

        public ContentItemReference(XElement element)
        {
            OnElement(element);
        }

        public ContentItemReference()
        {
            ClearContentItemReference();
        }

        public override void Clear()
        {
            base.Clear();
            ClearContentItemReference();
        }

        public void ClearContentItemReference()
        {
            _TreeKey = null;
            _NodeKey = null;
            _NodeContentKey = null;
            _ContentKey = null;
            _ItemKey = null;
            _DontTrackSource = false;
            _Item = null;
            _Tree = null;
            _Node = null;
        }

        public void CopyContentItemReference(ContentItemReference<T> other)
        {
            base.Copy(other);
            _TreeKey = other.TreeKey;
            _NodeKey = other.NodeKey;
            _NodeContentKey = other.NodeContentKey;
            _ContentKey = other.ContentKey;
            _ItemKey = other.ItemKey;
            _DontTrackSource = other.DontTrackSource;
            _Item = other.Item;
            _Tree = other.Tree;
            _Node = other.Node;
        }

        public void CopyReference(ContentItemReference<T> other)
        {
            _TreeKey = other.TreeKey;
            _NodeKey = other.NodeKey;
            _NodeContentKey = other.NodeContentKey;
            _ContentKey = other.ContentKey;
            _ItemKey = other.ItemKey;
            _Item = other.Item;
            _Tree = other.Tree;
            _Node = other.Node;
        }

        public void Copy(ContentItemReference<T> other)
        {
            if (other == null)
            {
                ClearBaseObjectKeyed();
                ClearContentItemReference();
                return;
            }

            CopyContentItemReference(other);
            base.Copy(other);
        }

        public void CopyDeep(ContentItemReference<T> other)
        {
            _TreeKey = other.TreeKey;
            _NodeKey = other.NodeKey;
            _NodeContentKey = other.NodeContentKey;
            _ContentKey = other.ContentKey;
            _ItemKey = other.ItemKey;
            _Item = other.Item;
            _Tree = other.Tree;
            _Node = other.Node;
            base.CopyDeep(other);
        }

        public override IBaseObject Clone()
        {
            return new ContentItemReference<T>(this);
        }

        public object TreeKey
        {
            get
            {
                return _TreeKey;
            }
            set
            {
                _TreeKey = value;
            }
        }

        public object NodeKey
        {
            get
            {
                return _NodeKey;
            }
            set
            {
                _NodeKey = value;
            }
        }

        public string NodeContentKey
        {
            get
            {
                return _NodeContentKey;
            }
            set
            {
                _NodeContentKey = value;
            }
        }

        public object ContentKey
        {
            get
            {
                return _ContentKey;
            }
            set
            {
                _ContentKey = value;
            }
        }

        public object ItemKey
        {
            get
            {
                return _ItemKey;
            }
            set
            {
                _ItemKey = value;
            }
        }

        public bool DontTrackSource
        {
            get
            {
                return _DontTrackSource;
            }
            set
            {
                if (_DontTrackSource != value)
                {
                    _DontTrackSource = value;
                    ModifiedFlag = true;
                }
            }
        }

        public T Item
        {
            get
            {
                return _Item;
            }
            set
            {
                _Item = value;

                if (value != null)
                    Key = value.Key;
            }
        }

        public BaseObjectNodeTree Tree
        {
            get
            {
                return _Tree;
            }
            set
            {
                _Tree = value;
            }
        }

        public BaseObjectNode Node
        {
            get
            {
                return _Node;
            }
            set
            {
                _Node = value;
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_TreeKey != null)
                element.Add(new XAttribute("TreeKey", _TreeKey.ToString()));
            if (_NodeKey != null)
                element.Add(new XAttribute("NodeKey", _NodeKey.ToString()));
            if (_NodeContentKey != null)
                element.Add(new XAttribute("NodeContentKey", _NodeContentKey.ToString()));
            if (_ContentKey != null)
                element.Add(new XAttribute("ContentKey", _ContentKey.ToString()));
            if (_ItemKey != null)
                element.Add(new XAttribute("ItemKey", _ItemKey.ToString()));
            if (_DontTrackSource)
                element.Add(new XAttribute("DontTrackSource", _DontTrackSource.ToString()));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "TreeKey":
                    _TreeKey = ObjectUtilities.ParseIntKeyString(attributeValue);
                    break;
                case "NodeKey":
                    _NodeKey = ObjectUtilities.ParseIntKeyString(attributeValue);
                    break;
                case "NodeContentKey":
                    _NodeContentKey = attributeValue;
                    break;
                case "ContentKey":
                    _ContentKey = ObjectUtilities.ParseIntKeyString(attributeValue);
                    break;
                case "ItemKey":
                    _ItemKey = attributeValue;
                    break;
                case "DontTrackSource":
                    _DontTrackSource = (attributeValue == true.ToString() ? true : false);
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override void OnFixup(FixupDictionary fixups)
        {
            base.OnFixup(fixups);

            string xmlKey = TreeKey.ToString();
            string source = Source;

            if (!String.IsNullOrEmpty(xmlKey))
            {
                IBaseObjectKeyed target = fixups.Get(source, xmlKey);

                if (target != null)
                {
                    if (_TreeKey != target.Key)
                    {
                        _TreeKey = target.Key;
                        ModifiedFlag = true;
                    }
                }
                else if (_TreeKey != null)
                {
                    _TreeKey = null;
                    ModifiedFlag = true;
                }
            }

            xmlKey = ContentKey.ToString();
            source = "StudyLists";

            if (!String.IsNullOrEmpty(xmlKey))
            {
                IBaseObjectKeyed target = fixups.Get(source, xmlKey);

                if (target != null)
                {
                    if (_ContentKey != target.Key)
                    {
                        _ContentKey = target.Key;
                        ModifiedFlag = true;
                    }
                }
                else if (_ContentKey != null)
                {
                    _ContentKey = null;
                    ModifiedFlag = true;
                }
            }
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ContentItemReference<T> otherContentItemReference = other as ContentItemReference<T>;

            if (otherContentItemReference == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareObjects(_ItemKey, otherContentItemReference.ItemKey);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareObjects(_ContentKey, otherContentItemReference.ContentKey);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareObjects(_NodeContentKey, otherContentItemReference.NodeContentKey);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareObjects(_NodeKey, otherContentItemReference.NodeKey);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareObjects(_TreeKey, otherContentItemReference.TreeKey);

            return diff;
        }

        public static int Compare(ObjectReference<T> object1, ObjectReference<T> object2)
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
