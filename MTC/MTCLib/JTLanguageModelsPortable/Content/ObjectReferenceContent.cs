using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.Content
{
    public class ObjectReferenceContent : BaseContentContainer
    {
        // Content key in repository identified by Source.
        // The inherited Key value is only for identification in the node,
        // and only has to be unique for the node.
        protected object _ContentKey;
        protected string _ContentType;
        protected string _ContentSubType;
        protected BaseObjectContent _Item;

        public ObjectReferenceContent(
                object key,     // Content key unique to node.
                MultiLanguageString title, MultiLanguageString description,
                string source, string package, string label, string imageFileName, int index, bool isPublic,
                List<LanguageID> targetLanguageIDs, List<LanguageID> hostLanguageIDs, string owner,
                string contentKey,  // Repository key, in repository identified by source.
                string contentType, string contentSubType, BaseObjectContent item)
            : base(key, title, description, source, package, label, imageFileName, index, isPublic,
                targetLanguageIDs, hostLanguageIDs, owner, null, null)
        {
            _ContentKey = contentKey;
            _ContentType = contentType;
            _ContentSubType = contentSubType;
            _Item = item;
            UpdateSubContentReferences();
        }

        public ObjectReferenceContent(object key, BaseObjectContent item)
            : base(key)
        {
            UpdateReference(item);
            _Modified = false;
        }

        public ObjectReferenceContent(ObjectReferenceContent other, object key, string source)
            : base(other, key)
        {
            CopyReference(other);
            _Modified = false;
        }

        public ObjectReferenceContent(ObjectReferenceContent other)
            : base(other, other.Key)
        {
            CopyReference(other);
            _Modified = false;
        }

        public ObjectReferenceContent(XElement element)
        {
            OnElement(element);
        }

        public ObjectReferenceContent()
        {
            ClearContentReference();
        }

        public override void Clear()
        {
            base.Clear();
            ClearContentReference();
        }

        public void ClearContentReference()
        {
            _Source = null;
            _ContentKey = null;
            _ContentType = String.Empty;
            _ContentSubType = String.Empty;
            _Item = null;
        }

        public void CopyContentReference(ObjectReferenceContent other)
        {
            base.Copy(other);
            _Source = other.Source;
            _ContentKey = other.ContentKey;
            _ContentType = other.ContentType;
            _ContentSubType = other.ContentSubType;
            _Item = other.Item;
            UpdateSubContentReferences();
        }

        public void CopyReference(ObjectReferenceContent other)
        {
            _Source = other.Source;
            _ContentKey = other.ContentKey;
            _ContentType = other.ContentType;
            _ContentSubType = other.ContentSubType;
            _Item = other.Item;
            UpdateSubContentReferences();
        }

        public void UpdateReference(BaseObjectContent other)
        {
            if (other != null)
            {
                Copy(other);
                Key = other.NodeContentKey;
                _ContentKey = other.Key;
                _ContentType = other.ContentType;
                _ContentSubType = other.ContentSubType;
                _Item = other;
                UpdateSubContentReferences();
            }
            else
            {
                _Item = null;
                _ContentReferenceList = null;
            }
        }

        public void Copy(ObjectReferenceContent other)
        {
            if (other == null)
            {
                ClearBaseObjectTitled();
                ClearContentReference();
                return;
            }

            CopyContentReference(other);
            base.Copy(other);
        }

        public void CopyDeep(ObjectReferenceContent other)
        {
            _Source = other.Source;
            _ContentKey = other.ContentKey;
            _ContentType = other.ContentType;
            _ContentSubType = other.ContentSubType;
            _Item = other.Item;
            base.CopyDeep(other);
        }

        public override IBaseObject Clone()
        {
            return new ObjectReferenceContent(this);
        }

        public void UpdateSubContentReferences()
        {
            _ContentReferenceList = null;

            if (_Item != null)
            {
                if (_Item is ContentMediaList)
                {
                    ContentMediaList mediaList = _Item as ContentMediaList;

                    if (mediaList.MediaItemCount() != 0)
                    {
                        foreach (ContentMediaItem mediaItem in mediaList.MediaItems)
                        {
                            ObjectReferenceContent contentReference = new ObjectReferenceContent(mediaItem.NodeContentKey, mediaItem);
                            AddContentReference(contentReference);
                        }
                    }
                }
                else if (_Item.ContentCount() != 0)
                {
                    foreach (ObjectReferenceContent contentReference in _Item.ContentReferenceList)
                    {
                        ObjectReferenceContent newContentReference = new ObjectReferenceContent(contentReference);
                        AddContentReference(contentReference);
                    }
                }
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
                if (_ContentKey != value)
                {
                    _ContentKey = value;
                    _Modified = true;
                }
            }
        }

        public string ContentType
        {
            get
            {
                return _ContentType;
            }
            set
            {
                if (_ContentType != value)
                {
                    _ContentType = value;
                    _Modified = true;
                }
            }
        }

        public string ContentSubType
        {
            get
            {
                return _ContentSubType;
            }
            set
            {
                if (_ContentSubType != value)
                {
                    _ContentSubType = value;
                    _Modified = true;
                }
            }
        }

        public BaseObjectContent Item
        {
            get
            {
                return _Item;
            }
            set
            {
                _Item = value;
            }
        }

        public T TypedItem<T>() where T : BaseObjectContent
        {
            return (T)_Item;
        }

        public T TypedItemAs<T>() where T : BaseObjectContent
        {
            return _Item as T;
        }

        public bool MatchContent(BaseObjectContent content)
        {
            if (content == null)
                return false;

            if (!MatchKey(content.NodeContentKey))
                return false;

            if (!content.MatchKey(ContentKey))
                return false;

            if (Source != content.Source)
                return false;

            return true;
        }

        public override string GetTitleString(LanguageID uiLanguageID)
        {
            string str = base.GetTitleString(uiLanguageID);

            if (String.IsNullOrEmpty(str))
                str = ContentType + (!String.IsNullOrEmpty(ContentSubType) ? " " + ContentSubType : "");

            return str;
        }

        public void ResolveReference(IMainRepository mainRepository)
        {
            if (_Item == null)
                _Item = (BaseObjectContent)mainRepository.ResolveReference(_Source, null, ContentKey);
        }

        public bool SaveReference(IMainRepository mainRepository)
        {
            if (_Item != null)
                return mainRepository.SaveReference(_Source, null, _Item);
            return true;
        }

        public override void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            if (_Item == null)
                _Item = (BaseObjectContent)mainRepository.ResolveReference(_Source, null, ContentKey);
        }

        public override bool SaveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;
            if (_Item != null)
            {
                returnValue = mainRepository.SaveReference(_Source, null, _Item);
                if (returnValue)
                    ContentKey = _Item.Key;
            }
            return returnValue;
        }

        public override void ClearReferences(bool recurseParents, bool recurseChildren)
        {
            _Item = null;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (_ContentKey != null)
                element.Add(new XAttribute("ContentKey", _ContentKey.ToString()));

            if (!String.IsNullOrEmpty(_ContentType))
                element.Add(new XAttribute("ContentType", _ContentType));

            if (!String.IsNullOrEmpty(_ContentSubType))
                element.Add(new XAttribute("ContentSubType", _ContentSubType));

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "ContentKey":
                    _ContentKey = ObjectUtilities.GetKeyFromString(attributeValue, null);
                    break;
                case "ContentType":
                    _ContentType = attributeValue;
                    break;
                case "ContentSubType":
                    _ContentSubType = attributeValue;
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ObjectReferenceContent otherContentReference = other as ObjectReferenceContent;

            if (otherContentReference == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareObjects(_ContentKey, otherContentReference.ContentKey);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareStrings(_ContentType, otherContentReference.ContentType);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareStrings(_ContentSubType, otherContentReference.ContentSubType);

            return diff;
        }

        public static int Compare(ObjectReferenceContent object1, ObjectReferenceContent object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareLists(List<ObjectReferenceContent> object1, List<ObjectReferenceContent> object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            int count1 = object1.Count();
            int count2 = object2.Count();
            if (count1 != count2)
                return count1 - count2;
            int index;
            for (index = 0; index < count1; index++)
            {
                int diff = Compare(object1[index], object2[index]);
                if (diff != 0)
                    return diff;
            }
            return 0;
        }

        public static int CompareKeys(ObjectReferenceContent object1, ObjectReferenceContent object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareObjectReferenceContentLists(List<ObjectReferenceContent> object1, List<ObjectReferenceContent> object2)
        {
            return ObjectUtilities.CompareTypedObjectLists<ObjectReferenceContent>(object1, object2);
        }

        public override void OnFixup(FixupDictionary fixups)
        {
            string xmlKey = KeyString;

            if (!String.IsNullOrEmpty(xmlKey) && !String.IsNullOrEmpty(_Source))
            {
                IBaseObjectKeyed target = fixups.Get(_Source, xmlKey);

                if (target != null)
                {
                    Key = target.Key;
                    _Item = (BaseObjectContent)target;
                }
            }
        }
    }
}
