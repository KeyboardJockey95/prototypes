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
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.Content
{
    public class ContentMediaList : BaseObjectContent
    {
        protected List<ContentMediaItem> _MediaItems;

        public ContentMediaList(object key, MultiLanguageString title, MultiLanguageString description,
                string source, string package, string label, string imageFileName, int index, bool isPublic,
                List<LanguageID> targetLanguageIDs, List<LanguageID> hostLanguageIDs, string owner,
                BaseObjectNode node, string nodeContentKey, string contentType, string contentSubType, List<IBaseObjectKeyed> options,
                MarkupTemplate markupTemplate, MarkupTemplateReference markupReference,
                List<ContentMediaItem> mediaItems)
            : base(key, title, description, source, package, label, imageFileName, index, isPublic, targetLanguageIDs, hostLanguageIDs, owner,
                node, nodeContentKey, contentType, contentSubType, options, markupTemplate, markupReference, null, null)
        {
            _MediaItems = mediaItems;
        }

        public ContentMediaList(object key)
            : base(key)
        {
            ClearContentMediaList();
        }

        public ContentMediaList(ContentMediaList other, object key)
            : base(other, key)
        {
            Copy(other);
            Modified = false;
        }

        public ContentMediaList(ContentMediaList other)
            : base(other)
        {
            Copy(other);
            Modified = false;
        }

        public ContentMediaList(XElement element)
        {
            OnElement(element);
        }

        public ContentMediaList()
        {
            ClearContentMediaList();
        }

        public void Copy(ContentMediaList other)
        {
            base.Copy(other);

            if (other == null)
            {
                ClearContentMediaList();
                return;
            }

            _MediaItems = CloneMediaItems();

            _Modified = true;
        }

        public void CopyDeep(ContentMediaList other)
        {
            Copy(other);
            base.CopyDeep(other);
        }

        public override void Clear()
        {
            base.Clear();
            ClearContentMediaList();
        }

        public void ClearContentMediaList()
        {
            _MediaItems = null;
        }

        public override IBaseObject Clone()
        {
            return new ContentMediaList(this);
        }

        public override ContentClassType ContentClass
        {
            get
            {
                return ContentClassType.MediaList;
            }
        }

        public override string Directory
        {
            get
            {
                return "Media";
            }
            set
            {
            }
        }

        public List<ContentMediaItem> MediaItems
        {
            get
            {
                return _MediaItems;
            }
            set
            {
                if (value != _MediaItems)
                {
                    _MediaItems = value;
                    _Modified = true;
                }
            }
        }

        public override BaseObjectNode Node
        {
            get
            {
                return base.Node;
            }
            set
            {
                base.Node = value;

                if (_MediaItems != null)
                {
                    foreach (ContentMediaItem contentMediaItem in _MediaItems)
                        contentMediaItem.Node = value;
                }
            }
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_MediaItems != null)
                {
                    foreach (ContentMediaItem mediaItem in _MediaItems)
                    {
                        if (mediaItem.Modified)
                            return true;
                    }
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_MediaItems != null)
                {
                    foreach (ContentMediaItem mediaItem in _MediaItems)
                        mediaItem.Modified = false;
                }
            }
        }

        public ContentMediaItem GetMediaItem(string nodeContentKey)
        {
            if ((_MediaItems != null) && !String.IsNullOrEmpty(nodeContentKey))
                return _MediaItems.FirstOrDefault(x => x.NodeContentKey == nodeContentKey);

            return null;
        }

        public ContentMediaItem GetMediaItemIndexed(int index)
        {
            if ((_MediaItems != null) && (index >= 0) && (index < _MediaItems.Count()))
                return _MediaItems.ElementAt(index);

            return null;
        }

        public ContentMediaItem GetMediaItemWithLabel(string label)
        {
            ContentMediaItem mediaItem = null;

            if (_MediaItems != null)
                mediaItem = _MediaItems.FirstOrDefault(x => (x.Label == label));

            return mediaItem;
        }

        public bool AddMediaItem(ContentMediaItem mediaItem)
        {
            if (_MediaItems == null)
                _MediaItems = new List<ContentMediaItem>(1) { mediaItem };
            else
                _MediaItems.Add(mediaItem);

            _Modified = true;
            
            return true;
        }

        public bool InsertMediaItemIndexed(int index, ContentMediaItem mediaItem)
        {
            if (_MediaItems == null)
                _MediaItems = new List<ContentMediaItem>(1) { mediaItem };
            else if (index < _MediaItems.Count())
                _MediaItems.Insert(index, mediaItem);
            else
                _MediaItems.Add(mediaItem);

            _Modified = true;

            return true;
        }

        public bool DeleteMediaItem(ContentMediaItem mediaItem)
        {
            if (_MediaItems != null)
            {
                if (_MediaItems.Remove(mediaItem))
                {
                    _Modified = true;
                    return true;
                }
            }

            return false;
        }

        public bool DeleteMediaItemKey(string nodeContentKey)
        {
            ContentMediaItem mediaItem = GetMediaItem(nodeContentKey);

            if (mediaItem != null)
            {
                _MediaItems.Remove(mediaItem);
                _Modified = true;
                return true;
            }

            return false;
        }

        public bool DeleteMediaItemWithLabel(string label)
        {
            ContentMediaItem mediaItem;
            int count;
            int index;
            bool returnValue = false;

            if ((_MediaItems != null) && ((count = _MediaItems.Count()) != 0))
            {
                for (index = count - 1; index >= 0; index--)
                {
                    mediaItem = _MediaItems[index];

                    if (mediaItem.Label == label)
                    {
                        _MediaItems.RemoveAt(index);
                        returnValue = true;
                    }
                }
            }

            return returnValue;
        }

        public bool DeleteMediaItemIndexed(int index)
        {
            if ((_MediaItems != null) && (index >= 0) && (index < _MediaItems.Count()))
            {
                _MediaItems.RemoveAt(index);
                _Modified = true;
                return true;
            }
            return false;
        }

        public void DeleteAllMediaItems()
        {
            if (_MediaItems != null)
            {
                if (_MediaItems.Count() != 0)
                    _Modified = true;

                _MediaItems.Clear();
            }
        }

        public int MediaItemCount()
        {
            if (_MediaItems != null)
                return (_MediaItems.Count());

            return 0;
        }

        public List<ContentMediaItem> CloneMediaItems()
        {
            if (_MediaItems == null)
                return null;

            List<ContentMediaItem> returnValue = new List<ContentMediaItem>(_MediaItems.Count());

            foreach (ContentMediaItem mediaItem in _MediaItems)
                returnValue.Add(new ContentMediaItem(mediaItem));

            return returnValue;
        }

        public override void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            base.ResolveReferences(mainRepository, recurseParents, recurseChildren);

            if (_MediaItems != null)
            {
                foreach (ContentMediaItem mediaItem in _MediaItems)
                    mediaItem.ResolveReferences(mainRepository, false, recurseChildren);
            }
        }

        public override void ClearReferences(bool recurseParents, bool recurseChildren)
        {
            base.ClearReferences(recurseChildren, recurseChildren);

            if (_MediaItems != null)
            {
                foreach (ContentMediaItem mediaItem in _MediaItems)
                    mediaItem.ClearReferences(false, recurseChildren);
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (_MediaItems != null)
            {
                foreach (ContentMediaItem mediaItem in _MediaItems)
                    element.Add(mediaItem.GetElement("MediaItem"));
            }

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "MediaItem":
                    {
                        ContentMediaItem mediaItem = new ContentMediaItem(childElement);
                        AddMediaItem(mediaItem);
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public static int Compare(ContentMediaList item1, ContentMediaList item2)
        {
            if (((object)item1 == null) && ((object)item2 == null))
                return 0;
            if ((object)item1 == null)
                return -1;
            if ((object)item2 == null)
                return 1;
            int diff = item1.Compare(item2);
            return diff;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            ContentMediaList otherContentMediaList = other as ContentMediaList;
            int diff;

            if (otherContentMediaList != null)
            {
                diff = base.Compare(other);

                if (diff != 0)
                    return diff;

                diff = ContentMediaItem.CompareContentMediaItemLists(_MediaItems, otherContentMediaList.MediaItems);

                return diff;
            }

            return base.Compare(other);
        }
    }
}
