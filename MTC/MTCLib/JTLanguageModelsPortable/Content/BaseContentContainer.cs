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
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Content
{
    public class BaseContentContainer : BaseObjectTitled
    {
        protected string _ContentParentKey;
        protected List<string> _ContentChildrenKeys;
        protected bool _IsReference;
        protected object _ReferenceTreeKey;
        protected string _ReferenceMediaTildeUrl;
        // Owning tree.  Not stored in database, but cached here.
        protected BaseObjectNodeTree _ReferenceTree;

        public BaseContentContainer(object key, MultiLanguageString title, MultiLanguageString description,
                string source, string package, string label, string imageFileName, int index, bool isPublic,
                List<LanguageID> targetLanguageIDs, List<LanguageID> hostLanguageIDs, string owner,
                BaseObjectContent contentParent, List<BaseObjectContent> contentChildren)
            : base(key, title, description, source, package, label, imageFileName, index, isPublic,
                targetLanguageIDs, hostLanguageIDs, owner)
        {
            ContentParent = contentParent;
            if (contentChildren != null)
            {
                _ContentChildrenKeys = new List<string>();
                foreach (BaseObjectContent content in contentChildren)
                    _ContentChildrenKeys.Add(content.KeyString);
            }
            _IsReference = false;
            _ReferenceTreeKey = null;
            _ReferenceTree = null;
            _ReferenceMediaTildeUrl = null;
        }

        public BaseContentContainer(BaseContentContainer other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public BaseContentContainer(object key)
            : base(key)
        {
            ClearBaseContentContainer();
        }

        public BaseContentContainer(BaseContentContainer other, object key)
            : base(other, key)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public BaseContentContainer(XElement element)
        {
            OnElement(element);
        }

        public BaseContentContainer()
        {
            ClearBaseContentContainer();
        }

        public override void Clear()
        {
            base.Clear();
            ClearBaseContentContainer();
        }

        public void ClearBaseContentContainer()
        {
            _ContentParentKey = null;
            _ContentChildrenKeys = null;
            _IsReference = false;
            _ReferenceTreeKey = null;
            _ReferenceTree = null;
            _ReferenceMediaTildeUrl = null;
        }

        public void Copy(BaseContentContainer other)
        {
            base.Copy(other);

            if (other == null)
            {
                ClearBaseContentContainer();
                return;
            }

            _ContentParentKey = other.ContentParentKey;

            if (other.ContentChildrenKeys != null)
                _ContentChildrenKeys = new List<string>(other.ContentChildrenKeys);
            else
                _ContentChildrenKeys = null;

            _IsReference = other.IsReference;
            _ReferenceTreeKey = other.ReferenceTreeKey;
            _ReferenceTree = other.ReferenceTree;
            _ReferenceMediaTildeUrl = other.ReferenceMediaTildeUrl;
        }

        public void CopyDeep(BaseContentContainer other)
        {
            Copy(other);
            base.CopyDeep(other);
        }

        public override IBaseObject Clone()
        {
            return new BaseContentContainer(this);
        }

        public override string MediaTildeUrl
        {
            get
            {
                if (!String.IsNullOrEmpty(_ReferenceMediaTildeUrl))
                    return _ReferenceMediaTildeUrl;
                else
                {
                    string directory = Directory;

                    if (ContentParent != null)
                        directory = ContentParent.MediaTildeUrl + "/" + directory;

                    return directory;
                }
            }
        }

        public string ReferenceMediaTildeUrl
        {
            get
            {
                return _ReferenceMediaTildeUrl;
            }
            set
            {
                if (value != _ReferenceMediaTildeUrl)
                {
                    _ReferenceMediaTildeUrl = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasContentParent()
        {
            if (!String.IsNullOrEmpty(ContentParentKey))
                return true;
            return false;
        }

        public virtual string ContentParentKey
        {
            get
            {
                return _ContentParentKey;
            }
            set
            {
                if (value != _ContentParentKey)
                {
                    _ContentParentKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public virtual BaseObjectContent ContentParent
        {
            get
            {
                return null;
            }
            set
            {
                if (value != null)
                {
                    if (_ContentParentKey != value.KeyString)
                    {
                        _ContentParentKey = value.KeyString;
                        ModifiedFlag = true;
                    }
                }
                else if (_ContentParentKey != null)
                {
                    _ContentParentKey = null;
                    ModifiedFlag = true;
                }
            }
        }

        public virtual List<string> ContentChildrenKeys
        {
            get
            {
                return _ContentChildrenKeys;
            }
            set
            {
                if (value != _ContentChildrenKeys)
                {
                    _ContentChildrenKeys = value;
                    ModifiedFlag = true;
                }
            }
        }

        public virtual List<BaseObjectContent> ContentChildren
        {
            get
            {
                return null;
            }
            set
            {
                if (value != null)
                {
                    _ContentChildrenKeys = new List<string>();
                    foreach (BaseObjectContent content in value)
                        _ContentChildrenKeys.Add(content.KeyString);
                    ReindexContent();
                    ModifiedFlag = true;
                }
                else if (_ContentChildrenKeys != null)
                {
                    _ContentChildrenKeys = null;
                    ModifiedFlag = true;
                }
            }
        }

        public int ContentChildrenCount()
        {
            if (_ContentChildrenKeys != null)
                return _ContentChildrenKeys.Count();
            return 0;
        }

        public bool HasContentChildren()
        {
            if (_ContentChildrenKeys != null)
                return _ContentChildrenKeys.Count() != 0;

            return false;
        }

        public void SortContentChildrenByIndex(bool sortSubContent)
        {
            List<BaseObjectContent> contentChildren = ContentChildren;

            if (contentChildren == null)
                return;

            if (sortSubContent)
            {
                foreach (BaseObjectContent subContent in contentChildren)
                {
                    if (subContent.KeyString == KeyString)
                        continue;

                    subContent.SortContentChildrenByIndex(true);

                    if (subContent.Modified)
                        ModifiedFlag = true;
                }
            }

            contentChildren.Sort(BaseObjectContent.CompareIndices);

            List<string> newList = new List<string>();

            foreach (BaseObjectContent subContent in contentChildren)
                newList.Add(subContent.KeyString);

            if (ObjectUtilities.CompareStringLists(newList, _ContentChildrenKeys) != 0)
            {
                _ContentChildrenKeys = newList;
                ModifiedFlag = true;
            }
        }

        public virtual List<BaseObjectContent> ContentList
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public virtual List<string> ContentKeysList
        {
            get
            {
                return _ContentChildrenKeys;
            }
        }

        public bool HasContent()
        {
            List<BaseObjectContent> contentList = ContentList;
            return (((contentList != null) && (contentList.Count() != 0)) ? true : false);
        }

        public bool HasContentWithClass(ContentClassType classType)
        {
            List<BaseObjectContent> contentList = ContentList;
            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if (content.ContentClass == classType)
                        return true;
                }
            }

            return false;
        }

        public bool HasContentWithSource(string source)
        {
            List<BaseObjectContent> contentList = ContentList;
            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if (content.Source == source)
                        return true;
                }
            }

            return false;
        }

        public bool HasContentWithType(string contentType)
        {
            List<BaseObjectContent> contentList = ContentList;
            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if (content.ContentType == contentType)
                        return true;
                }
            }

            return false;
        }

        public bool HasContentWithSubType(string contentSubType)
        {
            List<BaseObjectContent> contentList = ContentList;
            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if (content.ContentSubType == contentSubType)
                        return true;
                }
            }

            return false;
        }

        public bool HasContentWithTypeAndSubType(string contentType, string contentSubType)
        {
            List<BaseObjectContent> contentList = ContentList;
            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if ((content.ContentType == contentType) && (content.ContentSubType == contentSubType))
                        return true;
                }
            }

            return false;
        }

        public bool HasContentWithLabelAndSource(string label, string source)
        {
            List<BaseObjectContent> contentList = ContentList;
            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if ((content.Source == source) && (content.Label == label))
                        return true;
                }
            }

            return false;
        }

        public List<BaseObjectContent> LookupContent(Matcher matcher)
        {
            List<BaseObjectContent> contentList = ContentList;

            if (contentList == null)
                return new List<BaseObjectContent>();

            IEnumerable<BaseObjectContent> lookupQuery =
                from content in contentList
                where (matcher.Match(content))
                select content;

            return lookupQuery.ToList();
        }

        public BaseObjectContent GetContent(string nodeContentKey)
        {
            List<BaseObjectContent> contentList = ContentList;
            if ((contentList != null) && !String.IsNullOrEmpty(nodeContentKey))
                return contentList.FirstOrDefault(x => x.MatchKey(nodeContentKey));
            return null;
        }

        public BaseObjectContent GetContentType(string contentType, string contentSubType)
        {
            List<BaseObjectContent> contentList = ContentList;
            if ((contentList != null) && !String.IsNullOrEmpty(contentType))
                return contentList.FirstOrDefault(x => (x.ContentType == contentType) && (x.ContentSubType == contentSubType));
            return null;
        }

        public BaseContentStorage GetContentStorageType(string contentType, string contentSubType)
        {
            BaseObjectContent content = GetContentType(contentType, contentSubType);
            if (content != null)
                return content.ContentStorage;
            return null;
        }

        public BaseObjectContent GetContentIndexed(int index)
        {
            List<BaseObjectContent> contentList = ContentList;
            if ((contentList != null) && (index >= 0) && (index < contentList.Count()))
                return contentList.ElementAt(index);
            return null;
        }

        public BaseContentStorage GetContentStorageIndexed(int index)
        {
            BaseObjectContent content = GetContentIndexed(index);
            if (content != null)
                return content.ContentStorage;
            return null;
        }

        public List<BaseObjectContent> GetContentWithStorageClass(ContentClassType contentClass)
        {
            List<BaseObjectContent> contentList = ContentList;
            List<BaseObjectContent> returnValue = null;

            if (contentList != null)
                returnValue = contentList.Where(x => x.ContentClass == contentClass).ToList();

            return returnValue;
        }

        public List<BaseContentStorage> GetContentStorageWithStorageClass(ContentClassType contentClass)
        {
            List<BaseObjectContent> contentList = GetContentWithStorageClass(contentClass);
            List<BaseContentStorage> returnValue = new List<BaseContentStorage>();

            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if ((content.ContentClass == contentClass) && (content.ContentStorage != null))
                        returnValue.Add(content.ContentStorage);
                }
            }

            return returnValue;
        }

        public BaseObjectContent GetContentWithSource(object contentKey, string source)
        {
            List<BaseObjectContent> contentList = ContentList;
            BaseObjectContent content = null;

            if (contentList != null)
                content = contentList.FirstOrDefault(x => ObjectUtilities.MatchKeys(x.Key, contentKey) && (x.Source == source));

            return content;
        }

        public BaseContentStorage GetContentStorageWithSource(object contentKey, string source)
        {
            BaseObjectContent content = GetContentWithSource(contentKey, source);
            if (content != null)
                return content.ContentStorage;
            return null;
        }

        public List<BaseObjectContent> GetContentWithSource(string source)
        {
            List<BaseObjectContent> contentList = ContentList;
            List<BaseObjectContent> children = new List<BaseObjectContent>();

            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if (content.Source == source)
                        children.Add(content);
                }
            }

            return children;
        }

        public List<BaseContentStorage> GetContentStorageWithSource(string source)
        {
            List<BaseObjectContent> contentList = GetContentWithSource(source);
            List<BaseContentStorage> returnValue = new List<BaseContentStorage>();

            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if (content.ContentStorage != null)
                        returnValue.Add(content.ContentStorage);
                }
            }

            return returnValue;
        }

        public List<BaseObjectContent> GetContentWithType(string contentType)
        {
            List<BaseObjectContent> contentList = ContentList;
            List<BaseObjectContent> children = new List<BaseObjectContent>();

            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if (content.ContentType == contentType)
                        children.Add(content);
                }
            }

            return children;
        }

        public List<BaseContentStorage> GetContentStorageWithType(string contentType)
        {
            List<BaseObjectContent> contentList = GetContentWithType(contentType);
            List<BaseContentStorage> returnValue = new List<BaseContentStorage>();

            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if (content.ContentStorage != null)
                        returnValue.Add(content.ContentStorage);
                }
            }

            return returnValue;
        }

        public List<BaseObjectContent> GetContentWithSubType(string contentSubType)
        {
            List<BaseObjectContent> contentList = ContentList;
            List<BaseObjectContent> children = new List<BaseObjectContent>();

            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if (content.ContentSubType == contentSubType)
                        children.Add(content);
                }
            }

            return children;
        }

        public List<BaseContentStorage> GetContentStorageWithSubType(string contentSubType)
        {
            List<BaseObjectContent> contentList = GetContentWithSubType(contentSubType);
            List<BaseContentStorage> returnValue = new List<BaseContentStorage>();

            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if (content.ContentStorage != null)
                        returnValue.Add(content.ContentStorage);
                }
            }

            return returnValue;
        }

        public BaseObjectContent GetContentWithTypeAndSubType(string contentType, string contentSubType)
        {
            List<BaseObjectContent> contentList = ContentList;

            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if ((content.ContentType == contentType) && (content.ContentSubType == contentSubType))
                        return content;
                }
            }

            return null;
        }

        public List<BaseObjectContent> GetContentListWithTypeAndSubType(string contentType, string contentSubType,
            List<BaseObjectContent> contentList = null)
        {
            List<BaseObjectContent> localContentList = ContentList;
            List<BaseObjectContent> children = (contentList != null ? contentList : new List<BaseObjectContent>());

            if (localContentList != null)
            {
                foreach (BaseObjectContent content in localContentList)
                {
                    if ((content.ContentType == contentType) && (content.ContentSubType == contentSubType))
                        children.Add(content);
                }
            }

            return children;
        }

        public BaseContentStorage GetContentStorageWithTypeAndSubType(string contentType, string contentSubType)
        {
            BaseObjectContent content = GetContentWithTypeAndSubType(contentType, contentSubType);

            if (content != null)
                return content.ContentStorage;

            return null;
        }

        public BaseObjectContent GetFirstContentWithType(string contentType)
        {
            List<BaseObjectContent> contentList = ContentList;
            List<BaseObjectContent> children = new List<BaseObjectContent>();

            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if (content.ContentType == contentType)
                        return content;
                }
            }

            return null;
        }

        public BaseContentStorage GetFirstContentStorageWithType(string contentType)
        {
            BaseObjectContent content = GetFirstContentWithType(contentType);

            if (content != null)
                return content.ContentStorage;

            return null;
        }

        public List<BaseObjectContent> GetContentFiltered(string source, string contentType, string contentSubType,
            string title, LanguageID languageID)
        {
            List<BaseObjectContent> contentList = ContentList;
            List<BaseObjectContent> references = new List<BaseObjectContent>();

            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if (!String.IsNullOrEmpty(source))
                    {
                        if (content.Source != source)
                            continue;
                    }

                    if (!String.IsNullOrEmpty(contentType))
                    {
                        if (content.ContentType != contentType)
                            continue;
                    }

                    if (!String.IsNullOrEmpty(contentSubType))
                    {
                        if (content.ContentSubType != contentSubType)
                            continue;
                    }

                    if (!String.IsNullOrEmpty(title) && (languageID != null))
                    {
                        if (content.GetTitleString(languageID) != title)
                            continue;
                    }

                    references.Add(content);
                }
            }

            return references;
        }

        public BaseObjectContent GetContentWithLabelAndSource(string label, string source)
        {
            List<BaseObjectContent> contentList = ContentList;
            BaseObjectContent content = null;

            if (contentList != null)
                content = contentList.FirstOrDefault(x => (x.Label == label) && (x.Source == source));

            return content;
        }

        public BaseContentStorage GetContentStorageWithLabelAndSource(string label, string source)
        {
            BaseObjectContent content = GetContentWithLabelAndSource(label, source);

            if (content != null)
                return content.ContentStorage;

            return null;
        }

        public BaseObjectContent GetContentWithSourceAndTitle(string source, string title, LanguageID languageID)
        {
            List<BaseObjectContent> contentList = ContentList;
            if ((contentList != null) && (source != null))
                return contentList.FirstOrDefault(x => (x.Source == source) && (x.Title != null) && (x.Title.Text(languageID) == title));
            return null;
        }

        public BaseContentStorage GetContentStorageWithSourceAndTitle(string source, string title, LanguageID languageID)
        {
            BaseObjectContent content = GetContentWithSourceAndTitle(source, title, languageID);

            if (content != null)
                return content.ContentStorage;

            return null;
        }

        public List<T> GetContentStorageWithStorageClassTyped<T>(ContentClassType contentClass)
            where T : BaseContentStorage
        {
            List<BaseObjectContent> contentList = GetContentWithStorageClass(contentClass);
            List<T> returnValue = new List<T>();

            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if ((content.ContentClass == contentClass) && (content.ContentStorage != null))
                        returnValue.Add(content.GetContentStorageTyped<T>());
                }
            }

            return returnValue;
        }

        public T GetContentStorageTyped<T>(string nodeContentKey)
            where T : BaseContentStorage
        {
            return GetContentStorage(nodeContentKey) as T;
        }

        public T GetContentIndexedTyped<T>(int index)
            where T : BaseContentStorage
        {
            return GetContentStorageIndexed(index) as T;
        }

        public T GetContentStorageWithSourceTyped<T>(object contentKey, string source)
            where T : BaseContentStorage
        {
            return GetContentStorageWithSource(contentKey, source) as T;
        }

        public List<T> GetContentStorageWithSourceTyped<T>(string source)
            where T : BaseContentStorage
        {
            List<BaseContentStorage> list = GetContentStorageWithSource(source);
            if (list != null)
                return list.Cast<T>().ToList();
            return null;
        }

        public List<T> GetContentStorageWithTypeTyped<T>(string contentType)
            where T : BaseContentStorage
        {
            List<BaseContentStorage> list = GetContentStorageWithType(contentType);
            if (list != null)
                return list.Cast<T>().ToList();
            return null;
        }

        public List<T> GetContentStorageWithSubTypeTyped<T>(string contentSubType)
            where T : BaseObjectContent
        {
            List<BaseContentStorage> list = GetContentStorageWithSubType(contentSubType);
            if (list != null)
                return list.Cast<T>().ToList();
            return null;
        }

        public T GetContentStorageWithTypeAndSubTypeTyped<T>(string contentType, string contentSubType)
            where T : BaseContentStorage
        {
            return GetContentStorageWithTypeAndSubType(contentType, contentSubType) as T;
        }

        public T GetContentStorageWithLabelAndSourceTyped<T>(string label, string source)
            where T : BaseContentStorage
        {
            return GetContentStorageWithLabelAndSource(label, source) as T;
        }

        public T GetContentStorageWithSourceAndTitleTyped<T>(string source, string title, LanguageID languageID)
            where T : BaseContentStorage
        {
            return GetContentStorageWithSourceAndTitle(source, title, languageID) as T;
        }

        public BaseObjectContent GetContentWithStorageKey(object contentStorageKey, ContentClassType classType)
        {
            List<BaseObjectContent> contentList = ContentList;
            BaseObjectContent content = null;

            if (contentList != null)
                content = contentList.FirstOrDefault(x => (x.ContentClass == classType) && ObjectUtilities.MatchKeys(x.ContentStorageKey, contentStorageKey));

            return content;
        }

        public int GetContentIndex(BaseObjectContent content)
        {
            if (content == null)
                return -1;
            if (_ContentChildrenKeys != null)
                return _ContentChildrenKeys.IndexOf(content.KeyString);
            return -1;
        }

        public bool GetMediaItemLanguageMediaItemKeysAndNames(
            List<string> languageMediaItemKeys, List<string> languageMediaItemNames,
            LanguageID uiLanguageID)
        {
            List<ContentMediaItem> mediaItems = GetContentStorageWithStorageClassTyped<ContentMediaItem>(ContentClassType.MediaItem);

            foreach (ContentMediaItem mediaItem in mediaItems)
            {
                if (mediaItem.LanguageMediaItemCount() != 0)
                {
                    foreach (LanguageMediaItem languageMediaItem in mediaItem.LanguageMediaItems)
                    {
                        string key = languageMediaItem.KeyString;

                        if (!languageMediaItemKeys.Contains(key))
                        {
                            string name = languageMediaItem.GetName(uiLanguageID);
                            languageMediaItemKeys.Add(key);
                            languageMediaItemNames.Add(name);
                        }
                    }
                }
            }

            return true;
        }

        public int GetContentAllIndex(BaseObjectContent content)
        {
            if (content == null)
                return -1;
            List<BaseObjectContent> contentList = ContentList;
            if (contentList != null)
                return contentList.IndexOf(content);
            return -1;
        }

        public virtual bool AddContentChild(BaseObjectContent content)
        {
            if (content == null)
                return false;
            if (_ContentChildrenKeys != null)
            {
                if (!_ContentChildrenKeys.Contains(content.KeyString))
                {
                    content.Index = _ContentChildrenKeys.Count();
                    _ContentChildrenKeys.Add(content.KeyString);
                }
            }
            else
            {
                content.Index = 0;
                _ContentChildrenKeys = new List<String>() { content.KeyString };
            }
            ReindexContent(content.Index + 1);
            ModifiedFlag = true;
            return true;
        }

        public virtual bool InsertContentChildIndexed(int index, BaseObjectContent content)
        {
            if (content == null)
                return false;
            if (_ContentChildrenKeys != null)
            {
                if ((index >= 0) && (index <= _ContentChildrenKeys.Count()))
                {
                    content.Index = index;
                    _ContentChildrenKeys.Insert(index, content.KeyString);
                    ReindexContent(index + 1);
                }
                else if (index >= _ContentChildrenKeys.Count())
                {
                    content.Index = _ContentChildrenKeys.Count();
                    _ContentChildrenKeys.Add(content.KeyString);
                }
                else
                    return false;
            }
            else
            {
                content.Index = 0;
                _ContentChildrenKeys = new List<String>() { content.KeyString };
            }
            ModifiedFlag = true;
            return true;
        }

        public BaseObjectContent GetFirstContent()
        {
            if ((_ContentChildrenKeys == null) || (_ContentChildrenKeys.Count() == 0))
                return null;

            return GetContent(_ContentChildrenKeys[0]);
        }

        public BaseObjectContent GetLastContent()
        {
            if ((_ContentChildrenKeys == null) || (_ContentChildrenKeys.Count() == 0))
                return null;

            return GetContent(_ContentChildrenKeys.Last());
        }

        public BaseObjectContent GetFirstSelectedContent(Dictionary<string, bool> targetContentSelectFlags)
        {
            if ((_ContentChildrenKeys == null) || (_ContentChildrenKeys.Count() == 0))
                return null;

            int index;
            int count = _ContentChildrenKeys.Count();

            for (index = 0; index < count; index++)
            {
                BaseObjectContent childContent = GetContent(_ContentChildrenKeys[index]);

                if (childContent == null)
                    continue;

                if (targetContentSelectFlags == null)
                    return childContent;

                bool useIt;

                if (targetContentSelectFlags.TryGetValue(childContent.KeyString, out useIt))
                {
                    if (useIt)
                        return childContent;
                }

                childContent = childContent.GetFirstSelectedContent(targetContentSelectFlags);

                if (childContent != null)
                    return childContent;
            }

            return null;
        }

        public BaseObjectContent GetLastSelectedContent(Dictionary<string, bool> targetContentSelectFlags)
        {
            if ((_ContentChildrenKeys == null) || (_ContentChildrenKeys.Count() == 0))
                return null;

            int index;
            int count = _ContentChildrenKeys.Count();

            for (index = count - 1; index >= 0; index--)
            {
                BaseObjectContent childContent = GetContent(_ContentChildrenKeys[index]);

                if (childContent == null)
                    continue;

                if (targetContentSelectFlags == null)
                    return childContent;

                bool useIt;

                if (targetContentSelectFlags.TryGetValue(childContent.KeyString, out useIt))
                {
                    if (useIt)
                        return childContent;
                }

                childContent = childContent.GetFirstSelectedContent(targetContentSelectFlags);

                if (childContent != null)
                    return childContent;
            }

            return null;
        }

        public BaseObjectContent GetChildContentIndexed(int index)
        {
            if ((_ContentChildrenKeys == null) || (_ContentChildrenKeys.Count() == 0))
                return null;

            string contentKey = _ContentChildrenKeys[index];

            BaseObjectContent childContent = GetContent(contentKey);

            return childContent;
        }

        public int GetChildContentIndex(BaseObjectContent content)
        {
            if ((_ContentChildrenKeys == null) || (_ContentChildrenKeys.Count() == 0))
                return -1;

            return _ContentChildrenKeys.IndexOf(content.KeyString);
        }

        public bool InsertSelectedContents(int targetContentIndex, BaseObjectNode targetNodeOrTree,
            BaseContentContainer sourceContentContainer, Dictionary<string, bool> sourceContentSelectFlags,
            NodeUtilities nodeUtilities, Dictionary<string, bool> targetContentSelectFlags)
        {
            bool returnValue = true;

            if (sourceContentContainer.ContentChildrenCount() == 0)
                return true;

            if (targetContentIndex == -1)
                targetContentIndex = ContentChildrenCount();

            foreach (string sourceChildKey in sourceContentContainer.ContentChildrenKeys)
            {
                bool useIt = false;

                if (sourceContentSelectFlags == null)
                    useIt = true;
                else
                    sourceContentSelectFlags.TryGetValue(sourceChildKey, out useIt);

                if (!useIt)
                    continue;

                BaseObjectContent sourceChildContent = sourceContentContainer.GetContent(sourceChildKey);

                if (sourceChildContent == null)
                    continue;

                BaseObjectContent targetChildContent = nodeUtilities.CopyContent(
                    targetNodeOrTree, sourceChildContent, false);

                if (!InsertContentChildIndexed(targetContentIndex, targetChildContent))
                    returnValue = false;

                if (targetContentSelectFlags != null)
                    targetContentSelectFlags[targetChildContent.KeyString] = true;

                if (!targetChildContent.InsertSelectedContents(0, targetNodeOrTree,
                        sourceChildContent, sourceContentSelectFlags, nodeUtilities, targetContentSelectFlags))
                    returnValue = false;

                targetContentIndex++;
            }

            return returnValue;
        }

        public bool DeleteContent(BaseObjectContent content)
        {
            if (content!= null)
                DeleteContentChildKey(content.KeyString);
            List<BaseObjectContent> contentList = ContentList;
            if (contentList != null)
            {
                if (contentList.Remove(content))
                {
                    ContentList = contentList;
                    ModifiedFlag = true;
                    return true;
                }
            }
            return false;
        }

        public bool DeleteContentKey(string nodeContentKey)
        {
            if (!String.IsNullOrEmpty(nodeContentKey))
                DeleteContentChildKey(nodeContentKey);
            List<BaseObjectContent> contentList = ContentList;
            if ((contentList != null) && !String.IsNullOrEmpty(nodeContentKey))
            {
                BaseObjectContent content = GetContent(nodeContentKey);

                if (content != null)
                {
                    contentList.Remove(content);
                    ContentList = contentList;
                    ModifiedFlag = true;
                    return true;
                }
            }
            return false;
        }

        public bool DeleteContentChildKey(string nodeContentKey)
        {
            if (_ContentChildrenKeys != null)
            {
                while (_ContentChildrenKeys.Contains(nodeContentKey))
                    _ContentChildrenKeys.Remove(nodeContentKey);
            }
            return false;
        }

        public bool DeleteContentWithSource(object contentKey, string source)
        {
            List<BaseObjectContent> contentList = ContentList;
            if ((contentList != null) && (contentKey != null))
            {
                BaseObjectContent content = GetContentWithSource(contentKey, source);
                if (content != null)
                {
                    contentList.Remove(content);
                    ContentList = contentList;
                    ModifiedFlag = true;
                    return true;
                }
            }
            return false;
        }

        public bool DeleteContentIndexed(int index)
        {
            List<BaseObjectContent> contentList = ContentList;
            if ((contentList != null) && (index >= 0) && (index < contentList.Count()))
            {
                contentList.RemoveAt(index);
                ContentList = contentList;
                ModifiedFlag = true;
                return true;
            }
            return false;
        }

        public void DeleteAllContent()
        {
            List<BaseObjectContent> contentList = ContentList;
            if (contentList != null)
            {
                if (contentList.Count() != 0)
                    ModifiedFlag = true;
                contentList.Clear();
                ContentList = contentList;
            }
            if ((_ContentChildrenKeys != null) && (_ContentChildrenKeys.Count() != 0))
            {
                _ContentChildrenKeys.Clear();
                ModifiedFlag = true;
            }
        }

        public void DeleteAllContentWithSource(string source)
        {
            List<BaseObjectContent> contentList = ContentList;
            if (contentList != null)
            {
                int count = contentList.Count();
                int index;
                for (index = count - 1; index >= 0; index--)
                {
                    BaseObjectContent content = contentList[index];
                    if (content.Source != source)
                        continue;
                    contentList.RemoveAt(index);
                    ModifiedFlag = true;
                }
                ContentList = contentList;
            }
        }

        public int ContentCount()
        {
            List<BaseObjectContent> contentList = ContentList;
            if (contentList != null)
                return (contentList.Count());
            return 0;
        }

        public int ContentCountWithSource(string source)
        {
            List<BaseObjectContent> contentList = ContentList;
            int count = 0;
            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if (content.Source == source)
                        count++;
                }
            }
            return count;
        }

        public int ContentCountWithType(string contentType)
        {
            List<BaseObjectContent> contentList = ContentList;
            int count = 0;
            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if (content.ContentType == contentType)
                        count++;
                }
            }
            return count;
        }

        public bool MoveContent(BaseObjectContent content, bool moveUp)
        {
            if ((_ContentChildrenKeys == null) || (content == null))
                return false;

            string key = content.KeyString;
            int currentIndex = _ContentChildrenKeys.IndexOf(key);
            int newIndex;

            if (currentIndex == -1)
                return false;

            if (moveUp)
            {
                if (currentIndex == 0)
                    return false;

                newIndex = currentIndex - 1;
            }
            else
            {
                if (currentIndex == _ContentChildrenKeys.Count() - 1)
                    return false;

                newIndex = currentIndex + 1;
            }

            _ContentChildrenKeys.RemoveAt(currentIndex);
            _ContentChildrenKeys.Insert(newIndex, key);
            ReindexContent();
            ModifiedFlag = true;

            return true;
        }

        public bool ReindexContent()
        {
            bool returnValue = false;

            if (_ContentChildrenKeys == null)
                return returnValue;

            int index = 0;

            foreach(string key in _ContentChildrenKeys)
            {
                BaseObjectContent content = GetContent(key);

                if (content == null)
                    continue;

                if (content.Index != index)
                {
                    content.Index = index;
                    ModifiedFlag = true;
                    returnValue = true;
                }

                index++;
            }

            return returnValue;
        }

        public bool ReindexContent(int startIndex)
        {
            bool returnValue = false;

            if (_ContentChildrenKeys == null)
                return returnValue;

            int count = _ContentChildrenKeys.Count();

            for (int index = startIndex; index < count; index++)
            {
                BaseObjectContent content = GetContent(_ContentChildrenKeys[index]);

                if (content == null)
                    continue;

                if (content.Index != index)
                {
                    content.Index = index;
                    ModifiedFlag = true;
                    returnValue = true;
                }

                index++;
            }

            return returnValue;
        }

        public BaseContentStorage GetContentStorage(string nodeContentKey)
        {
            if (!String.IsNullOrEmpty(nodeContentKey))
            {
                BaseObjectContent content = GetContent(nodeContentKey);
                if (content != null)
                    return content.ContentStorage;
            }
            return null;
        }

        public ContentMediaItem GetMediaItem(string nodeContentKey)
        {
            return GetContentStorageTyped<ContentMediaItem>(nodeContentKey);
        }

        public List<BaseObjectContent> ContentStudyLists()
        {
            return GetContentWithStorageClass(ContentClassType.StudyList);
        }

        public List<BaseObjectContent> ContentMediaItems()
        {
            return GetContentWithStorageClass(ContentClassType.MediaItem);
        }

        public List<string> GetContentKeyStrings(ContentClassType classType)
        {
            List<string> returnValue = new List<string>();

            if (classType != ContentClassType.None)
            {
                List<BaseObjectContent> contentList = GetContentWithStorageClass(classType);

                if (contentList != null)
                {
                    foreach (BaseObjectContent content in contentList)
                        returnValue.Add(content.KeyString);
                }
            }

            return returnValue;
        }

        public List<string> GetContentKeyStrings(string contentType)
        {
            List<string> returnValue = new List<string>();

            if (!String.IsNullOrEmpty(contentType))
            {
                List<BaseObjectContent> contentList = GetContentWithType(contentType);

                if (contentList != null)
                {
                    foreach (BaseObjectContent content in contentList)
                        returnValue.Add(content.KeyString);
                }
            }

            return returnValue;
        }

        public List<string> GetContentKeyStrings(List<string> contentTypes)
        {
            List<string> returnValue = new List<string>();

            if (contentTypes != null)
            {
                foreach (string contentType in contentTypes)
                {
                    List<BaseObjectContent> contentList = GetContentWithType(contentType);

                    if (contentList != null)
                    {
                        foreach (BaseObjectContent content in contentList)
                            returnValue.Add(content.KeyString);
                    }
                }
            }

            return returnValue;
        }

        public bool IsReference
        {
            get
            {
                return _IsReference;
            }
            set
            {
                if (value != _IsReference)
                {
                    _IsReference = value;
                    ModifiedFlag = true;
                }
            }
        }

        public object ReferenceTreeKey
        {
            get
            {
                return _ReferenceTreeKey;
            }
            set
            {
                if (value != _ReferenceTreeKey)
                {
                    _ReferenceTreeKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public BaseObjectNodeTree ReferenceTree
        {
            get
            {
                return _ReferenceTree;
            }
            set
            {
                _ReferenceTree = value;
            }
        }

        public virtual bool CopyMedia(string newDirectoryRoot, List<string> copiedFiles, ref string errorMessage)
        {
            return true;
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                return false;
            }
            set
            {
                base.Modified = value;
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (!String.IsNullOrEmpty(_ContentParentKey))
                element.Add(new XElement("ContentParentKey", _ContentParentKey));

            if ((_ContentChildrenKeys != null) && (_ContentChildrenKeys.Count() != 0))
                element.Add(ObjectUtilities.GetElementFromStringList("ContentChildrenKeys", _ContentChildrenKeys));

            if (_IsReference)
                element.Add(new XAttribute("IsReference", _IsReference.ToString()));

            if (_ReferenceTreeKey != null)
                element.Add(new XAttribute("ReferenceTreeKey", _ReferenceTreeKey.ToString()));

            if (!String.IsNullOrEmpty(_ReferenceMediaTildeUrl))
                element.Add(new XElement("ReferenceMediaTildeUrl", _ReferenceMediaTildeUrl));

            return element;
        }

        public virtual XElement GetElementFiltered(string name, Dictionary<int, bool> childNodeFlags,
            Dictionary<string, bool> childContentFlags)
        {
            XElement element = base.GetElement(name);

            if (!String.IsNullOrEmpty(_ContentParentKey))
            {
                BaseObjectContent parentContent = ContentParent;

                while (parentContent != null)
                {
                    bool useIt = true;

                    if (childContentFlags != null)
                    {
                        if (!childContentFlags.TryGetValue(parentContent.KeyString, out useIt))
                            useIt = true;
                    }

                    if (useIt)
                    {
                        element.Add(new XElement("ContentParentKey", parentContent.KeyString));
                        break;
                    }

                    parentContent = parentContent.ContentParent;
                }
            }

            if ((_ContentChildrenKeys != null) && (_ContentChildrenKeys.Count() != 0))
            {
                List<string> childrenKeys = new List<string>();
                LoadContentChildrenKeys(childrenKeys, childContentFlags);
                element.Add(ObjectUtilities.GetElementFromStringList("ContentChildrenKeys", childrenKeys));
            }

            if (_IsReference)
                element.Add(new XAttribute("IsReference", _IsReference.ToString()));

            if (_ReferenceTreeKey != null)
                element.Add(new XAttribute("ReferenceTreeKey", _ReferenceTreeKey.ToString()));

            if (!String.IsNullOrEmpty(_ReferenceMediaTildeUrl))
                element.Add(new XElement("ReferenceMediaTildeUrl", _ReferenceMediaTildeUrl));

            return element;
        }

        public void LoadContentChildrenKeys(List<string> childrenKeys, Dictionary<string, bool> childContentFlags)
        {
            List<BaseObjectContent> contentChildren = ContentChildren;

            if (contentChildren == null)
                return;

            foreach (BaseObjectContent contentChild in contentChildren)
            {
                bool useIt = true;
                string childKey = contentChild.KeyString;

                if (childContentFlags != null)
                {
                    if (!childContentFlags.TryGetValue(childKey, out useIt))
                        useIt = true;
                }

                if (useIt)
                    childrenKeys.Add(childKey);
                else
                    contentChild.LoadContentChildrenKeys(childrenKeys, childContentFlags);
            }
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "IsReference":
                    IsReference = (attributeValue == true.ToString());
                    break;
                case "ReferenceTreeKey":
                    ReferenceTreeKey = ObjectUtilities.ParseIntKeyString(attributeValue);
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "ContentParentKey":
                    ContentParentKey = childElement.Value.Trim();
                    break;
                case "ContentChildrenKeys":
                    ContentChildrenKeys = ObjectUtilities.GetStringListFromElement(childElement);
                    // Hack to remove infinite recursion.
                    for (int i = ContentChildrenKeys.Count() - 1; i >= 0; i--)
                    {
                        if (ContentChildrenKeys[i] == KeyString)
                            ContentChildrenKeys.RemoveAt(i);
                    }
                    break;
                case "ReferenceMediaTildeUrl":
                    ReferenceMediaTildeUrl = childElement.Value.Trim();
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            BaseContentContainer otherBaseContentContainer = other as BaseContentContainer;

            if (otherBaseContentContainer == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareStrings(_ContentParentKey, otherBaseContentContainer.ContentParentKey);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareStringLists(_ContentChildrenKeys, otherBaseContentContainer.ContentChildrenKeys);

            return diff;
        }

        public static int Compare(BaseContentContainer item1, BaseContentContainer item2)
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

        public static int CompareKeys(BaseContentContainer object1, BaseContentContainer object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return ObjectUtilities.CompareKeys(object1, object2);
        }

        public static int CompareContainerLists(List<BaseContentContainer> object1, List<BaseContentContainer> object2)
        {
            return ObjectUtilities.CompareTypedObjectLists<BaseContentContainer>(object1, object2);
        }

        public override void CollectReferences(List<IBaseObjectKeyed> references, List<IBaseObjectKeyed> externalSavedChildren,
            List<IBaseObjectKeyed> externalNonSavedChildren, Dictionary<int, bool> nodeSelectFlags,
            Dictionary<string, bool> contentSelectFlags, Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles, VisitMedia visitFunction)
        {
            List<BaseObjectContent> contentList = ContentChildren;

            base.CollectReferences(references, externalSavedChildren, externalNonSavedChildren,
                        nodeSelectFlags, contentSelectFlags, itemSelectFlags, languageSelectFlags, mediaFiles, visitFunction);

            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    content.CollectReferences(references, externalSavedChildren, externalNonSavedChildren,
                        nodeSelectFlags, contentSelectFlags, itemSelectFlags, languageSelectFlags, mediaFiles, visitFunction);
                }
            }
        }

        public override void OnFixup(FixupDictionary fixups)
        {
            List<BaseObjectContent> contentList = ContentList;
            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                    content.OnFixup(fixups);
            }
        }
    }
}
