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
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Master
{
    public class MasterContentItem : BaseObjectTitled
    {
        protected string _ContentType;
        protected string _ContentSubType;
        protected List<MasterContentItem> _ContentItems;
        protected MarkupTemplateReference _MarkupReference;
        protected MarkupTemplateReference _CopyMarkupReference;
        protected List<OptionDescriptor> _OptionDescriptors;

        public MasterContentItem(string nodeContentKey, string contentType, string contentSubType, MultiLanguageString title, MultiLanguageString description,
                string source, string package, string label, string imageFileName, int index, List<LanguageID> targetLanguageIDs,
                List<LanguageID> hostLanguageIDs, string owner, List<MasterContentItem> contentItems, MarkupTemplateReference markupReference,
                MarkupTemplateReference copyMarkupReference, List<OptionDescriptor> optionDescriptors) :
            base(nodeContentKey, title, description, source, package, label, imageFileName, index, true,
                targetLanguageIDs, hostLanguageIDs, owner)
        {
            _ContentType = contentType;
            _ContentSubType = contentSubType;
            _ContentItems = contentItems;
            _MarkupReference = markupReference;
            _CopyMarkupReference = copyMarkupReference;
            _OptionDescriptors = optionDescriptors;
            ModifiedFlag = false;
        }

        public MasterContentItem(XElement element)
        {
            OnElement(element);
        }

        public MasterContentItem(MasterContentItem other) : base(other)
        {
            ClearMasterContentItem();
            Copy(other);
        }

        public MasterContentItem()
        {
            ClearMasterContentItem();
        }

        public override void Clear()
        {
            base.Clear();
            ClearMasterContentItem();
        }

        public void ClearMasterContentItem()
        {
            _ContentType = String.Empty;
            _ContentSubType = String.Empty;
            _ContentItems = null;
            _MarkupReference = null;
            _CopyMarkupReference = null;
            ModifiedFlag = false;
        }

        public virtual void Copy(MasterContentItem other)
        {
            Key = other.Key;
            CopyTitledObject(other);
            _ContentType = other.ContentType;
            _ContentSubType = other.ContentSubType;
            _ContentItems = MasterContentItem.CopyMasterContentItemList(other.ContentItems);
            if (other.MarkupReference != null)
                _MarkupReference = new MarkupTemplateReference(other.MarkupReference);
            else
                _MarkupReference = null;
            if (other.CopyMarkupReference != null)
                _CopyMarkupReference = new MarkupTemplateReference(other.CopyMarkupReference);
            else
                _CopyMarkupReference = null;
            _OptionDescriptors = null;
            CopyOptionDescriptors(other.OptionDescriptors);
            PropogateLanguages();
            SetOwner(Owner);
            ModifiedFlag = false;
        }

        public void CopyMasterContentItemShallow(MasterContentItem other)
        {
            Key = other.Key;
            CopyLanguages(other);
            CopyTitledObject(other);
            _ContentType = other.ContentType;
            _ContentSubType = other.ContentSubType;
            if (other.MarkupReference != null)
                _MarkupReference = new MarkupTemplateReference(other.MarkupReference);
            else
                _MarkupReference = null;
            if (other.CopyMarkupReference != null)
                _CopyMarkupReference = new MarkupTemplateReference(other.CopyMarkupReference);
            else
                _CopyMarkupReference = null;
            _OptionDescriptors = null;
            CopyOptionDescriptors(other.OptionDescriptors);
            PropogateLanguages();
            SetOwner(Owner);
            ModifiedFlag = false;
        }

        public override IBaseObject Clone()
        {
            return new MasterContentItem(this);
        }

        public void PropogateLanguages()
        {
            if (_ContentItems != null)
            {
                foreach (MasterContentItem item in _ContentItems)
                    item.CopyLanguages(this);
            }
        }

        public void InitializeLanguages(NodeMaster master, MasterContentItem parentContentItem, UserProfile userProfile)
        {
            BaseObjectLanguages parent;

            if (parentContentItem != null)
                parent = parentContentItem;
            else
                parent = master;

            CopyLanguages(parent);

            /* Before LanguageMediaItem
            switch (ContentType)
            {
                case "Audio":
                case "Video":
                case "Automated":
                case "Image":
                case "TextFile":
                case "PDF":
                case "Embedded":
                case "Media":
                    break;
                default:
                    return;
            }

            LanguageID languageID = null;
            List<LanguageID> languageIDs = null;

            if (master != null)
            {
                List<LanguageID> targetLanguageIDs = parent.ExpandTargetLanguageIDs(userProfile);
                LanguageID hostLanguageID = userProfile.HostLanguageID;
                if (hostLanguageID == null)
                    hostLanguageID = userProfile.UILanguageID;
                string hostAbbrev = hostLanguageID.MediaLanguageAbbreviation(userProfile.UILanguageID);
                string key = ContentType + ContentSubType;
                List<string> testKeys = new List<string>(4);
                int lastIndex = 2;
                switch (ContentSubType)
                {
                    case "Introduction":
                    case "Summary":
                    case "Grammar":
                    case "Culture":
                    case "Lesson":
                        lastIndex = 3;
                        break;
                    default:
                        break;
                }
                if ((targetLanguageIDs != null) && (targetLanguageIDs.Count != 0))
                {
                    foreach (LanguageID targetLanguageID in targetLanguageIDs)
                    {
                        string abbrev = targetLanguageID.MediaLanguageAbbreviation(userProfile.UILanguageID);
                        testKeys.Clear();
                        testKeys.Add(ContentType + abbrev);
                        testKeys.Add(ContentType + abbrev + hostAbbrev);
                        testKeys.Add(ContentType + ContentSubType + abbrev);
                        testKeys.Add(ContentType + ContentSubType + abbrev + hostAbbrev);
                        bool found = false;
                        for (int index = 0; index < 4; index++)
                        {
                            string testKey = testKeys[index];

                            if (master.GetContentItem(testKey) != null)
                            {
                                lastIndex = index;
                                found = true;
                                break;
                            }
                        }
                        if (found)
                            continue;
                        key = testKeys[lastIndex];
                        languageID = targetLanguageID;
                        break;
                    }
                    if (languageID == null)
                        languageID = targetLanguageIDs.First();
                    languageIDs = new List<LanguageID>() { languageID };
                    foreach (LanguageID targetLanguageID in targetLanguageIDs)
                    {
                        if ((targetLanguageID != languageID) && (targetLanguageID.LanguageCode == languageID.LanguageCode))
                            languageIDs.Add(targetLanguageID);
                    }
                }
                if ((lastIndex == 0) || (lastIndex == 2))
                    HostLanguageIDs = new List<LanguageID>();
            }

            if (languageIDs != null)
                TargetLanguageIDs = languageIDs;
            */
        }

        public void SetOwner(string owner)
        {
            if (_ContentItems != null)
            {
                foreach (MasterContentItem item in _ContentItems)
                    item.Owner = owner;
            }

            Owner = owner;
        }

        public ContentClassType ContentClass
        {
            get
            {
                ContentClassType type;

                switch (_ContentType)
                {
                    case "Document":
                        type = ContentClassType.DocumentItem;
                        break;
                    case "Audio":
                    case "Video":
                    case "Automated":
                    case "Image":
                    case "TextFile":
                    case "PDF":
                    case "Embedded":
                        type = ContentClassType.MediaItem;
                        break;
                    case "Media":
                        type = ContentClassType.MediaList;
                        break;
                    case "Transcript":
                    case "Text":
                    case "Sentences":
                    case "Words":
                    case "Characters":
                    case "Expansion":
                    case "Exercises":
                    case "Notes":
                    case "Comments":
                        type = ContentClassType.StudyList;
                        break;
                    default:
                        type = ContentClassType.DocumentItem;
                        break;
                }

                return type;
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
                    ModifiedFlag = true;
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
                    ModifiedFlag = true;
                }
            }
        }

        public List<MasterContentItem> ContentItems
        {
            get
            {
                return _ContentItems;
            }
            set
            {
                if (_ContentItems == value)
                    return;

                if (MasterContentItem.CompareMasterContentItemLists(_ContentItems, value) != 0)
                {
                    _ContentItems = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasContentKey(string contentKey)
        {
            if (_ContentItems != null)
            {
                MasterContentItem item = _ContentItems.FirstOrDefault(x => x.KeyString == contentKey);
                if (item != null)
                    return true;
            }

            return false;
        }

        public bool HasContentType(string contentType)
        {
            if (_ContentItems != null)
            {
                MasterContentItem item = _ContentItems.FirstOrDefault(x => x.ContentType == contentType);
                if (item != null)
                    return true;
            }

            return false;
        }

        public bool HasContentSubType(string contentSubType)
        {
            if (_ContentItems != null)
            {
                MasterContentItem item = _ContentItems.FirstOrDefault(x => x.ContentSubType == contentSubType);
                if (item != null)
                    return true;
            }

            return false;
        }

        public bool HasContentTypeAndSubType(string contentType, string contentSubType)
        {
            if (_ContentItems != null)
            {
                MasterContentItem item = _ContentItems.FirstOrDefault(x => (x.ContentType == contentType) && (x.ContentSubType == contentSubType));
                if (item != null)
                    return true;
            }

            return false;
        }

        public MasterContentItem GetContentItem(string nodeContentKey)
        {
            if (_ContentItems != null)
            {
                MasterContentItem item = _ContentItems.FirstOrDefault(x => x.KeyString == nodeContentKey);
                return item;
            }

            return null;
        }

        public MasterContentItem GetContentItemType(string contentType)
        {
            if (_ContentItems != null)
            {
                MasterContentItem item = _ContentItems.FirstOrDefault(x => x.ContentType == contentType);
                return item;
            }

            return null;
        }

        public MasterContentItem GetContentItemSubType(string contentSubType)
        {
            if (_ContentItems != null)
            {
                MasterContentItem item = _ContentItems.FirstOrDefault(x => x.ContentSubType == contentSubType);
                return item;
            }

            return null;
        }

        public MasterContentItem GetContentItemTypeAndSubType(string contentType, string contentSubType)
        {
            if (_ContentItems != null)
            {
                MasterContentItem item = _ContentItems.FirstOrDefault(x => (x.ContentType == contentType) && (x.ContentSubType == contentSubType));
                return item;
            }

            return null;
        }

        public void AddContentItem(MasterContentItem item)
        {
            if (_ContentItems != null)
            {
                item.Index = _ContentItems.Count();
                _ContentItems.Add(item);
            }
            else
            {
                item.Index = 0;
                _ContentItems = new List<MasterContentItem>() { item };
            }

            ModifiedFlag = true;
        }

        public void AddContentItemGrouped(MasterContentItem contentItem)
        {
            if (_ContentItems == null)
            {
                _ContentItems = new List<MasterContentItem>() { contentItem };
                contentItem.Index = _ContentItems.Count() - 1;
                ModifiedFlag = true;
            }
            else
            {
                int bestIndex = -1;
                int index = 0;

                foreach (MasterContentItem mci in _ContentItems)
                {
                    if (mci.ContentType == contentItem.ContentType)
                        bestIndex = Index;

                    index++;
                }

                if (bestIndex != -1)
                    InsertContentItem(bestIndex + 1, contentItem);
                else
                {
                    _ContentItems.Add(contentItem);
                    contentItem.Index = _ContentItems.Count() - 1;
                    ModifiedFlag = true;
                }
            }
        }

        public bool InsertContentItem(int index, MasterContentItem item)
        {
            bool returnValue = false;

            if (_ContentItems != null)
            {
                if ((index >= 0) && (index < ContentItemCount()))
                {
                    item.Index = index;
                    _ContentItems.Insert(index, item);
                    returnValue = true;
                }
            }
            else
            {
                item.Index = index = 0;
                _ContentItems = new List<MasterContentItem>() { item };
                returnValue = true;
            }

            while (index < _ContentItems.Count())
                _ContentItems[index].Index = index++;

            ModifiedFlag = true;

            return returnValue;
        }

        public bool MoveContentItemUp(int index)
        {
            if ((index > 0) && (index < ContentItemCount()))
            {
                MasterContentItem menuItem = _ContentItems[index];
                _ContentItems.RemoveAt(index);
                index--;
                _ContentItems.Insert(index, menuItem);

                while (index < _ContentItems.Count())
                    _ContentItems[index].Index = index++;

                ModifiedFlag = true;
                return true;
            }

            return false;
        }

        public bool MoveContentItemDown(int index)
        {
            if ((index >= 0) && (index < ContentItemCount() - 1))
            {
                MasterContentItem menuItem = _ContentItems[index];
                _ContentItems.RemoveAt(index);
                _ContentItems.Insert(index + 1, menuItem);

                while (index < _ContentItems.Count())
                    _ContentItems[index].Index = index++;

                ModifiedFlag = true;
                return true;
            }

            return false;
        }

        public bool DeleteContentItemIndexed(int index)
        {
            if ((index >= 0) && (index < ContentItemCount()))
            {
                _ContentItems.RemoveAt(index);

                while (index < _ContentItems.Count())
                    _ContentItems[index].Index = index++;

                ModifiedFlag = true;
                return true;
            }

            return false;
        }

        public void DeleteAllContentItems()
        {
            _ContentItems = null;
            ModifiedFlag = true;
        }

        public int ContentItemCount()
        {
            if (_ContentItems != null)
                return _ContentItems.Count();

            return 0;
        }

        public bool ReindexContentItems()
        {
            bool returnValue = false;

            if (_ContentItems == null)
                return returnValue;

            int index = 0;

            foreach (MasterContentItem contentItem in _ContentItems)
            {
                if (contentItem.Index != index)
                {
                    contentItem.Index = index;
                    ModifiedFlag = true;
                    returnValue = true;
                }

                index++;
            }

            return returnValue;
        }

        public void ContentOptionsCheck(NodeMaster master, UserProfile userProfile)
        {
            List<OptionDescriptor> newOptionDescriptors = GetDefaultDescriptors(ContentType, ContentSubType, userProfile);

            foreach (OptionDescriptor newOptionDescriptor in newOptionDescriptors)
            {
                OptionDescriptor oldOptionDescriptor = GetOptionDescriptor(newOptionDescriptor.KeyString);

                if (oldOptionDescriptor == null)
                {
                    int index = newOptionDescriptors.IndexOf(newOptionDescriptor);

                    if (_OptionDescriptors == null)
                        OptionDescriptors = new List<OptionDescriptor>() { newOptionDescriptor };
                    else
                        InsertOptionDescriptor(index, newOptionDescriptor);
                }
                else if (oldOptionDescriptor.Type != newOptionDescriptor.Type)
                    oldOptionDescriptor.Copy(newOptionDescriptor);
            }
        }

        public void ContentTypeChanged(NodeMaster master, UserProfile userProfile)
        {
            List<OptionDescriptor> newOptionDescriptors = GetDefaultDescriptors(ContentType, ContentSubType, userProfile);

            foreach (OptionDescriptor newOptionDescriptor in newOptionDescriptors)
            {
                OptionDescriptor oldOptionDescriptor = GetOptionDescriptor(newOptionDescriptor.KeyString);

                if (oldOptionDescriptor != null)
                {
                    switch (oldOptionDescriptor.KeyString)
                    {
                        case "SourceComponentKeys":
                            break;
                        default:
                            newOptionDescriptor.Value = oldOptionDescriptor.Value;
                            break;
                    }
                }
            }

            OptionDescriptors = newOptionDescriptors;

            Key = ComposeKey(master, ContentType, ContentSubType, userProfile, false);

            string titleString = MasterContentItem.ComposeTitleString(master, KeyString, ContentType, ContentSubType, userProfile, false);
            Title = ObjectUtilities.CreateMultiLanguageString("Title", titleString,
                userProfile.HostLanguageDescriptors);

            string descriptionString = MasterContentItem.ComposeDescriptionString(master, KeyString, ContentType, ContentSubType, userProfile, false);
            Description = ObjectUtilities.CreateMultiLanguageString("Description", descriptionString,
                userProfile.HostLanguageDescriptors);
        }

        public static string ComposeKey(BaseObjectTitled owningObject, string contentType, string contentSubType,
            UserProfile userProfile, bool addLanguagePostfix)
        {
            string key = String.Empty;
            BaseObjectNode node = owningObject as BaseObjectNode;
            NodeMaster master = owningObject as NodeMaster;
            MasterContentItem parentContentItem = owningObject as MasterContentItem;

            switch (contentType)
            {
                case "Document":
                    key = contentType;
                    break;
                case "Audio":
                case "Video":
                case "Embedded":
                case "Automated":
                case "Media":
                    key = contentType + contentSubType;
                    if (addLanguagePostfix && (userProfile != null))
                    {
                        List<BaseObjectContent> contentList = null;
                        List<MasterContentItem> itemList = null;
                        if (node != null)
                        {
                            contentList = node.ContentList;
                            if (contentList == null)
                                contentList = new List<BaseObjectContent>();
                        }
                        else if (master != null)
                        {
                            itemList = master.ContentItems;
                            if (itemList == null)
                                itemList = new List<MasterContentItem>();
                        }
                        List<LanguageID> targetLanguageIDs = owningObject.ExpandTargetLanguageIDs(userProfile);
                        LanguageID hostLanguageID = userProfile.HostLanguageID;
                        if (hostLanguageID == null)
                            hostLanguageID = userProfile.UILanguageID;
                        string hostAbbrev = hostLanguageID.MediaLanguageAbbreviation(userProfile.UILanguageID);
                        List<string> testKeys = new List<string>(4);
                        int lastIndex = 2;
                        switch (contentSubType)
                        {
                            case "Introduction":
                            case "Summary":
                            case "Grammar":
                            case "Culture":
                            case "Lesson":
                                lastIndex = 3;
                                break;
                            default:
                                break;
                        }
                        if ((targetLanguageIDs != null) && (targetLanguageIDs.Count != 0))
                        {
                            foreach (LanguageID targetLanguageID in targetLanguageIDs)
                            {
                                string abbrev = targetLanguageID.MediaLanguageAbbreviation(userProfile.UILanguageID);
                                testKeys.Clear();
                                testKeys.Add(contentType + abbrev);
                                testKeys.Add(contentType + abbrev + hostAbbrev);
                                testKeys.Add(contentType + contentSubType + abbrev);
                                testKeys.Add(contentType + contentSubType + abbrev + hostAbbrev);
                                bool keyFound = false;
                                for (int index = 0; index < 4; index++)
                                {
                                    string testKey = testKeys[index];
                                    if (contentList != null)
                                    {
                                        if (contentList.FirstOrDefault(x => x.KeyString == testKey) != null)
                                        {
                                            lastIndex = index;
                                            keyFound = true;
                                            break;
                                        }
                                    }
                                    else if (itemList != null)
                                    {
                                        if (itemList.FirstOrDefault(x => x.KeyString == testKey) != null)
                                        {
                                            lastIndex = index;
                                            keyFound = true;
                                            break;
                                        }
                                    }
                                }
                                if (keyFound)
                                    continue;
                                key = testKeys[lastIndex];
                                break;
                            }
                        }
                    }
                    break;
                case "Image":
                case "TextFile":
                case "PDF":
                    key = contentType + (contentSubType != "Lesson" ? contentSubType : "");
                    break;
                case "Transcript":
                case "Text":
                case "Sentences":
                case "Words":
                case "Characters":
                case "Expansion":
                case "Exercises":
                case "Notes":
                case "Comments":
                    key = contentType;
                    break;
                default:
                    key = contentType;
                    break;
            }

            if ((node != null) && (node.GetContent(key) != null))
            {
                string suffix = String.Empty;
                int index = 1;
                string baseKey = key;

                do
                {
                    key = baseKey + index.ToString();
                    index++;
                }
                while (node.GetContent(key) != null);
            }
            else if ((master != null) && (master.GetContentItem(key) != null))
            {
                string suffix = String.Empty;
                int index = 1;
                string baseKey = key;

                do
                {
                    key = baseKey + index.ToString();
                    index++;
                }
                while (master.GetContentItem(key) != null);
            }

            return key;
        }

        public static string ComposeTitleString(
            BaseObjectTitled owningObject,
            string contentKey,
            string contentType,
            string contentSubType,
            UserProfile userProfile,
            bool addLanguagePostfix)
        {
            BaseObjectNode node = owningObject as BaseObjectNode;
            NodeMaster master = owningObject as NodeMaster;
            string key = contentKey;
            string title = String.Empty;
            bool useContentSubType = (contentSubType != "Lesson") && !String.IsNullOrEmpty(contentSubType);
            string referenceKey = ComposeKey(null, contentType, contentSubType,
                userProfile, addLanguagePostfix);
            string keySuffix = contentKey.Substring(referenceKey.Length);
            string titleSuffix = " " + (!String.IsNullOrEmpty(keySuffix) ? keySuffix.Replace("_", "-") : "");

            switch (contentType)
            {
                case "Document":
                    title = contentType + titleSuffix;
                    break;
                case "Audio":
                case "Video":
                case "Embedded":
                case "Automated":
                case "Media":
                    title = contentType + " " + contentSubType + titleSuffix;
                    if (addLanguagePostfix)
                    {
                        if (userProfile != null)
                        {
                            List<BaseObjectContent> contentList = null;
                            List<MasterContentItem> itemList = null;
                            if (node != null)
                            {
                                contentList = node.ContentList;
                                if (contentList == null)
                                    contentList = new List<BaseObjectContent>();
                            }
                            else if (master != null)
                            {
                                itemList = master.ContentItems;
                                if (itemList == null)
                                    itemList = new List<MasterContentItem>();
                            }
                            List<LanguageID> targetLanguageIDs = owningObject.ExpandTargetLanguageIDs(userProfile);
                            LanguageID hostLanguageID = userProfile.HostLanguageID;
                            if (hostLanguageID == null)
                                hostLanguageID = userProfile.UILanguageID;
                            string hostAbbrev = hostLanguageID.MediaLanguageAbbreviation(userProfile.UILanguageID);
                            List<string> testKeys = new List<string>(4);
                            int lastIndex = 2;
                            switch (contentSubType)
                            {
                                case "Introduction":
                                case "Summary":
                                case "Grammar":
                                case "Culture":
                                case "Lesson":
                                    lastIndex = 3;
                                    break;
                                default:
                                    break;
                            }
                            if ((targetLanguageIDs != null) && (targetLanguageIDs.Count != 0))
                            {
                                foreach (LanguageID targetLanguageID in targetLanguageIDs)
                                {
                                    string abbrev = targetLanguageID.MediaLanguageAbbreviation(userProfile.UILanguageID);
                                    testKeys.Clear();
                                    testKeys.Add(contentType + abbrev);
                                    testKeys.Add(contentType + abbrev + hostAbbrev);
                                    testKeys.Add(contentType + contentSubType + abbrev);
                                    testKeys.Add(contentType + contentSubType + abbrev + hostAbbrev);
                                    bool keyFound = false;
                                    for (int index = 0; index < 4; index++)
                                    {
                                        string testKey = testKeys[index];
                                        if (contentList != null)
                                        {
                                            if (contentList.FirstOrDefault(x => x.KeyString == testKey) != null)
                                            {
                                                lastIndex = index;
                                                keyFound = true;
                                                break;
                                            }
                                        }
                                        else if (itemList != null)
                                        {
                                            if (itemList.FirstOrDefault(x => x.KeyString == testKey) != null)
                                            {
                                                lastIndex = index;
                                                keyFound = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (keyFound)
                                        continue;
                                    key = testKeys[lastIndex];
                                    switch (lastIndex)
                                    {
                                        case 0:
                                            title = contentType + " " + abbrev + titleSuffix;
                                            break;
                                        case 1:
                                            title = contentType + " " + abbrev + hostAbbrev + titleSuffix;
                                            break;
                                        case 2:
                                            title = contentType + " " + contentSubType + " " + abbrev + titleSuffix;
                                            break;
                                        case 3:
                                            title = contentType + " " + contentSubType + " " + abbrev + hostAbbrev + titleSuffix;
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case "Image":
                case "TextFile":
                case "PDF":
                    title = contentType + (useContentSubType ? " " + contentSubType : "") + titleSuffix;
                    break;
                case "Transcript":
                case "Text":
                    title = contentType + titleSuffix;
                    break;
                case "Sentences":
                case "Words":
                case "Characters":
                case "Expansion":
                    title = contentType + (useContentSubType ? " " + contentSubType : "") + titleSuffix;
                    break;
                case "Exercises":
                case "Notes":
                case "Comments":
                    title = contentType + titleSuffix;
                    break;
                default:
                    title = contentType + titleSuffix;
                    break;
            }

            if ((node != null) && (node.GetContent(key) != null))
            {
                string suffix = String.Empty;
                int index = 1;
                string baseKey = key;
                string baseTitle = title;

                do
                {
                    key = baseKey + index.ToString();
                    title = baseTitle + " " + index.ToString();
                    index++;
                }
                while (node.GetContent(key) != null);
            }
            else if ((master != null) && (master.GetContentItem(key) != null))
            {
                string suffix = String.Empty;
                int index = 1;
                string baseKey = key;
                string baseTitle = title;

                do
                {
                    key = baseKey + index.ToString();
                    title = baseTitle + " " + index.ToString();
                    index++;
                }
                while (master.GetContentItem(key) != null);
            }

            return title;
        }

        public static string ComposeDescriptionString(
            BaseObjectTitled owningObject,
            string contentKey,
            string contentType,
            string contentSubType,
            UserProfile userProfile,
            bool addLanguagePostfix)
        {
            BaseObjectNode node = owningObject as BaseObjectNode;
            NodeMaster master = owningObject as NodeMaster;
            string key = contentKey;
            string description = String.Empty;
            bool useContentSubType = (contentSubType != "Lesson") && !String.IsNullOrEmpty(contentSubType);
            string referenceKey = ComposeKey(null, contentType, contentSubType,
                userProfile, addLanguagePostfix);
            string keySuffix = contentKey.Substring(referenceKey.Length);
            string descriptionSuffix = " " + (!String.IsNullOrEmpty(keySuffix) ? keySuffix.Replace("_", "-") : "");

            switch (contentType)
            {
                case "Document":
                    description = contentType + descriptionSuffix;
                    break;
                case "Audio":
                case "Video":
                case "Embedded":
                case "Automated":
                case "Media":
                    description = contentType + " " + contentSubType.ToLower();
                    if (addLanguagePostfix)
                    {
                        if (userProfile != null)
                        {
                            List<BaseObjectContent> contentList = null;
                            List<MasterContentItem> itemList = null;
                            if (node != null)
                            {
                                contentList = node.ContentList;
                                if (contentList == null)
                                    contentList = new List<BaseObjectContent>();
                            }
                            else if (master != null)
                            {
                                itemList = master.ContentItems;
                                if (itemList == null)
                                    itemList = new List<MasterContentItem>();
                            }
                            List<LanguageID> targetLanguageIDs = owningObject.ExpandTargetLanguageIDs(userProfile);
                            LanguageID hostLanguageID = userProfile.HostLanguageID;
                            if (hostLanguageID == null)
                                hostLanguageID = userProfile.UILanguageID;
                            string hostAbbrev = hostLanguageID.MediaLanguageAbbreviation(userProfile.UILanguageID);
                            string hostLanguageName = hostLanguageID.LanguageName(userProfile.UILanguageID);
                            List<string> testKeys = new List<string>(4);
                            int lastIndex = 2;
                            switch (contentSubType)
                            {
                                case "Introduction":
                                case "Summary":
                                case "Grammar":
                                case "Culture":
                                case "Lesson":
                                    lastIndex = 3;
                                    break;
                                default:
                                    break;
                            }
                            if ((targetLanguageIDs != null) && (targetLanguageIDs.Count != 0))
                            {
                                foreach (LanguageID targetLanguageID in targetLanguageIDs)
                                {
                                    string abbrev = targetLanguageID.MediaLanguageAbbreviation(userProfile.UILanguageID);
                                    string targetLanguageName = targetLanguageID.LanguageName(userProfile.UILanguageID);
                                    testKeys.Clear();
                                    testKeys.Add(contentType + abbrev);
                                    testKeys.Add(contentType + abbrev + hostAbbrev);
                                    testKeys.Add(contentType + contentSubType + abbrev);
                                    testKeys.Add(contentType + contentSubType + abbrev + hostAbbrev);
                                    bool keyFound = false;
                                    for (int index = 0; index < 4; index++)
                                    {
                                        string testKey = testKeys[index];
                                        if (contentList != null)
                                        {
                                            if (contentList.FirstOrDefault(x => x.KeyString == testKey) != null)
                                            {
                                                lastIndex = index;
                                                keyFound = true;
                                                break;
                                            }
                                        }
                                        else if (itemList != null)
                                        {
                                            if (itemList.FirstOrDefault(x => x.KeyString == testKey) != null)
                                            {
                                                lastIndex = index;
                                                keyFound = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (keyFound)
                                        continue;
                                    key = testKeys[lastIndex];
                                    switch (lastIndex)
                                    {
                                        case 0:
                                        case 2:
                                            description = contentType + " " + contentSubType.ToLower() + " (in " + targetLanguageName + ")";
                                            break;
                                        case 1:
                                        case 3:
                                            description = contentType + " " + contentSubType.ToLower() + " (in " + targetLanguageName + " and " + hostLanguageName + ")";
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case "Image":
                case "TextFile":
                case "PDF":
                    description = contentType + (useContentSubType ? " " + contentSubType.ToLower() : "") + descriptionSuffix + " file";
                    break;
                case "Transcript":
                case "Text":
                    description = contentType + descriptionSuffix;
                    break;
                case "Sentences":
                case "Words":
                case "Characters":
                case "Expansion":
                    description = contentType + (useContentSubType ? " " + contentSubType.ToLower() : "") + descriptionSuffix;
                    break;
                case "Exercises":
                case "Notes":
                case "Comments":
                    description = contentType + descriptionSuffix;
                    break;
                default:
                    description = contentType + descriptionSuffix;
                    break;
            }

            if ((node != null) && (node.GetContent(key) != null))
            {
                string suffix = String.Empty;
                int index = 1;
                string baseKey = key;
                string baseTitle = description;

                do
                {
                    key = baseKey + index.ToString();
                    description = baseTitle + " " + index.ToString();
                    index++;
                }
                while (node.GetContent(key) != null);
            }
            else if ((master != null) && (master.GetContentItem(key) != null))
            {
                string suffix = String.Empty;
                int index = 1;
                string baseKey = key;
                string baseTitle = description;

                do
                {
                    key = baseKey + index.ToString();
                    description = baseTitle + " " + index.ToString();
                    index++;
                }
                while (master.GetContentItem(key) != null);
            }

            description += ".";

            return description;
        }

        public MarkupTemplateReference MarkupReference
        {
            get
            {
                return _MarkupReference;
            }
            set
            {
                if (_MarkupReference != value)
                {
                    _MarkupReference = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string MarkupTemplateKey
        {
            get
            {
                if (_MarkupReference != null)
                    return _MarkupReference.KeyString;
                return null;
            }
            set
            {
                if (_MarkupReference != null)
                {
                    if (_MarkupReference.KeyString != value)
                    {
                        _MarkupReference.Key = value;
                        ModifiedFlag = true;
                    }
                }
                else if (value != null)
                {
                    _MarkupReference = new MarkupTemplateReference(value, null, null);
                }
            }
        }

        public bool IsLocalMarkupTemplate
        {
            get
            {
                return MarkupTemplateKey == "(local)";
            }
        }

        public MarkupTemplate MarkupTemplate
        {
            get
            {
                if (_MarkupReference != null)
                    return _MarkupReference.Item;

                return null;
            }
        }

        public MarkupTemplateReference CopyMarkupReference
        {
            get
            {
                return _CopyMarkupReference;
            }
            set
            {
                if (_CopyMarkupReference != value)
                {
                    _CopyMarkupReference = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string CopyMarkupTemplateKey
        {
            get
            {
                if (_CopyMarkupReference != null)
                    return _CopyMarkupReference.KeyString;
                return null;
            }
            set
            {
                if (_CopyMarkupReference != null)
                {
                    if (_CopyMarkupReference.KeyString != value)
                    {
                        _CopyMarkupReference.Key = value;
                        ModifiedFlag = true;
                    }
                }
                else if (value != null)
                {
                    _CopyMarkupReference = new MarkupTemplateReference(value, null, null);
                }
            }
        }

        public MarkupTemplate CopyMarkupTemplate
        {
            get
            {
                if (_CopyMarkupReference != null)
                    return _CopyMarkupReference.Item;

                return null;
            }
        }

        public static List<OptionDescriptor> GetDefaultDescriptors(string contentType, string contentSubType,
            UserProfile userProfile)
        {
            List<OptionDescriptor> newOptionDescriptors = null;

            switch (contentType)
            {
                case "Document":
                    newOptionDescriptors = ContentDocumentItem.GetDefaultDescriptors(contentType, contentSubType, userProfile);
                    break;
                case "Audio":
                case "Video":
                case "Automated":
                case "Image":
                case "TextFile":
                case "PDF":
                case "Embedded":
                    newOptionDescriptors = ContentMediaItem.GetDefaultDescriptors(contentType, contentSubType, userProfile);
                    break;
                case "Media":
                    newOptionDescriptors = new List<OptionDescriptor>()
                    {
                        new OptionDescriptor("DescendentMediaPlaceholder", "flag", "Descendent media placeholder",
                            "This option indicates that this media list is just a placeholder for all media items in descendents of a group or course.", "true"),
                    };
                    break;
                case "Transcript":
                case "Text":
                case "Sentences":
                case "Words":
                case "Characters":
                case "Expansion":
                case "Exercises":
                case "Notes":
                case "Comments":
                    newOptionDescriptors = ContentStudyList.GetDefaultDescriptors(contentType, contentSubType, userProfile);
                    break;
                default:
                    throw new Exception("GetDefaultDescriptors: Unknown content type: " + contentType);
            }

            return newOptionDescriptors;
        }

        public void SetupDefaultOptionDescriptors(UserProfile userProfile)
        {
            List<OptionDescriptor> newOptionDescriptors = GetDefaultDescriptors(ContentType, ContentSubType, userProfile);
            OptionDescriptors = newOptionDescriptors;
        }

        public void SetupDefaultOptionDescriptor(OptionDescriptor optionDescriptor, UserProfile userProfile)
        {
            string optionKey = optionDescriptor.KeyString;
            List<OptionDescriptor> componentOptionDescriptors = GetDefaultDescriptors(ContentType, ContentSubType, userProfile);
            if (componentOptionDescriptors != null)
            {
                OptionDescriptor componentOptionDescriptor = componentOptionDescriptors.FirstOrDefault(x => x.KeyString == optionKey);
                if ((componentOptionDescriptor != null) && (_OptionDescriptors != null))
                    optionDescriptor.Copy(componentOptionDescriptor);
            }
        }

        public List<OptionDescriptor> OptionDescriptors
        {
            get
            {
                return _OptionDescriptors;
            }
            set
            {
                if (value != _OptionDescriptors)
                {
                    _OptionDescriptors = value;
                    ModifiedFlag = true;
                }
            }
        }

        public OptionDescriptor GetOptionDescriptor(string key)
        {
            if (OptionDescriptors != null)
                return OptionDescriptors.FirstOrDefault(x => x.KeyString == key);

            return null;
        }

        public OptionDescriptor GetOptionDescriptorIndexed(int index)
        {
            if ((OptionDescriptors != null) && (index >= 0) && (index < OptionDescriptors.Count()))
                return OptionDescriptors[index];

            return null;
        }

        public void AddOptionDescriptor(OptionDescriptor optionDescriptor)
        {
            if (OptionDescriptors == null)
                OptionDescriptors = new List<OptionDescriptor>(1) { optionDescriptor };
            else
                OptionDescriptors.Add(optionDescriptor);
            ModifiedFlag = true;
        }

        public void AddStringOptionDescriptor(string name, string label, string help, string value)
        {
            OptionDescriptor optionDescriptor = new OptionDescriptor(name, "String", label, help, value);
            AddOptionDescriptor(optionDescriptor);
        }

        public void AddIntegerOptionDescriptor(string name, string label, string help, int value)
        {
            OptionDescriptor optionDescriptor = new OptionDescriptor(name, "Int32", label, help, value.ToString());
            AddOptionDescriptor(optionDescriptor);
        }

        public void AddFlagOptionDescriptor(string name, string label, string help, bool value)
        {
            OptionDescriptor optionDescriptor = new OptionDescriptor(name, "Boolean", label, help, value.ToString());
            AddOptionDescriptor(optionDescriptor);
        }

        public bool InsertOptionDescriptor(int index, OptionDescriptor optionDescriptor)
        {
            if (OptionDescriptors == null)
                OptionDescriptors = new List<OptionDescriptor>(1) { optionDescriptor };
            else if ((index >= 0) && (index <= OptionDescriptors.Count()))
                OptionDescriptors.Insert(index, optionDescriptor);
            else
                return false;
            ModifiedFlag = true;
            return true;
        }

        public bool DeleteOptionDescriptorIndexed(int index)
        {
            if ((OptionDescriptors != null) && (index >= 0) && (index < OptionDescriptors.Count()))
                OptionDescriptors.RemoveAt(index);
            else
                return false;
            ModifiedFlag = true;
            return true;
        }

        public int OptionDescriptorCount()
        {
            if (OptionDescriptors != null)
                return OptionDescriptors.Count();

            return 0;
        }

        public void CopyOptionDescriptors(List<OptionDescriptor> others)
        {
            if (others == null)
            {
                OptionDescriptors = null;
                return;
            }

            OptionDescriptors = new List<OptionDescriptor>(others.Count());

            foreach (OptionDescriptor other in others)
                OptionDescriptors.Add(new OptionDescriptor(other));
        }

        public virtual string GetOptionLabel(string optionKey)
        {
            OptionDescriptor optionDescriptor = GetOptionDescriptor(optionKey);
            if (optionDescriptor != null)
                return optionDescriptor.Label;
            return null;
        }

        public virtual string GetOptionType(string optionKey)
        {
            OptionDescriptor optionDescriptor = GetOptionDescriptor(optionKey);
            if (optionDescriptor != null)
                return optionDescriptor.Type;
            return null;
        }

        public virtual string GetOptionHelp(string optionKey)
        {
            OptionDescriptor optionDescriptor = GetOptionDescriptor(optionKey);
            if (optionDescriptor != null)
                return optionDescriptor.Help;
            return null;
        }

        public virtual string GetOptionValue(string optionKey)
        {
            OptionDescriptor optionDescriptor = GetOptionDescriptor(optionKey);
            if (optionDescriptor != null)
                return optionDescriptor.Value;
            return null;
        }

        public virtual List<string> GetOptionValues(string optionKey)
        {
            OptionDescriptor optionDescriptor = GetOptionDescriptor(optionKey);
            if (optionDescriptor != null)
                return optionDescriptor.Values;
            return null;
        }

        public virtual List<IBaseObjectKeyed> GetOptionsFromDescriptors()
        {
            List<OptionDescriptor> optionDescriptors = OptionDescriptors;

            if (optionDescriptors == null)
                return null;

            List<IBaseObjectKeyed> options = new List<IBaseObjectKeyed>(optionDescriptors.Count());

            foreach (OptionDescriptor optionDescriptor in optionDescriptors)
            {
                IBaseObjectKeyed option = null;

                if (optionDescriptor.Value != null)
                    option = new BaseString(optionDescriptor.KeyString, optionDescriptor.Value);

                if (option != null)
                    options.Add(option);
            }

            return options;
        }

        public virtual string GetOptionsStringFromDescriptors()
        {
            List<OptionDescriptor> optionDescriptors = OptionDescriptors;
            StringBuilder sb = new StringBuilder();
            int index = 0;

            if (optionDescriptors == null)
                return "";

            foreach (OptionDescriptor optionDescriptor in optionDescriptors)
            {
                if (optionDescriptor.Value != null)
                {
                    sb.Append((index != 0 ? "," : "") + optionDescriptor.KeyString + "=" + optionDescriptor.Value);
                    index++;
                }
            }

            return sb.ToString();
        }

        public MasterMenuItem CreateMenuItem()
        {
            string text = ContentType + (!String.IsNullOrEmpty(ContentSubType) && (ContentType != ContentSubType) ? " " + ContentSubType : "");
            //string text = TextUtilities.SeparateUpperLowerCase(KeyString);
            MasterMenuItem menuItem = new MasterMenuItem(
                text, String.Empty, String.Empty, KeyString, ContentType, ContentSubType);
            return menuItem;
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_ContentItems != null)
                {
                    foreach (MasterContentItem item in _ContentItems)
                    {
                        if (item.Modified)
                            return true;
                    }
                }

                if (_OptionDescriptors != null)
                {
                    foreach (OptionDescriptor item in _OptionDescriptors)
                    {
                        if (item.Modified)
                            return true;
                    }
                }

                if (_MarkupReference != null)
                {
                    if (_MarkupReference.Modified)
                        return true;
                }

                if (_CopyMarkupReference != null)
                {
                    if (_CopyMarkupReference.Modified)
                        return true;
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_ContentItems != null)
                {
                    foreach (MasterContentItem item in _ContentItems)
                        item.Modified = false;
                }

                if (_OptionDescriptors != null)
                {
                    foreach (OptionDescriptor item in _OptionDescriptors)
                        item.Modified = false;
                }

                if (_MarkupReference != null)
                    _MarkupReference.Modified = false;

                if (_CopyMarkupReference != null)
                    _CopyMarkupReference.Modified = false;
            }
        }

        public override void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            base.ResolveReferences(mainRepository, recurseParents, recurseChildren);

            if (_ContentItems != null)
            {
                foreach (MasterContentItem item in _ContentItems)
                    item.ResolveReferences(mainRepository, false, recurseChildren);
            }

            if ((_MarkupReference != null) && (_MarkupReference.Key != null)
                    && !_MarkupReference.KeyString.StartsWith("(") && (_MarkupReference.Item == null))
                _MarkupReference.ResolveReference(mainRepository);

            if (_CopyMarkupReference != null)
                _CopyMarkupReference.ResolveReference(mainRepository);
        }

        public override void ClearReferences(bool recurseParents, bool recurseChildren)
        {
            base.ClearReferences(recurseParents, recurseChildren);

            if (_ContentItems != null)
            {
                foreach (MasterContentItem item in _ContentItems)
                    item.ClearReferences(false, recurseChildren);
            }

            if (_MarkupReference != null)
                _MarkupReference.ClearReference();

            if (_CopyMarkupReference != null)
                _CopyMarkupReference.ClearReference();
        }

        public override void OnFixup(FixupDictionary fixups)
        {
            base.OnFixup(fixups);

            if (_ContentItems != null)
            {
                foreach (MasterContentItem item in _ContentItems)
                    item.OnFixup(fixups);
            }

            if (_MarkupReference != null)
                _MarkupReference.OnFixup(fixups);

            if (_CopyMarkupReference != null)
                _CopyMarkupReference.OnFixup(fixups);
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (!String.IsNullOrEmpty(_ContentType))
                element.Add(new XAttribute("ContentType", _ContentType));
            if (!String.IsNullOrEmpty(_ContentSubType))
                element.Add(new XAttribute("ContentSubType", _ContentSubType));
            if (_ContentItems != null)
            {
                XElement contentItemsElement = new XElement("ContentItems");
                foreach (MasterContentItem item in _ContentItems)
                    contentItemsElement.Add(item.Xml);
                element.Add(contentItemsElement);
            }
            if ((_MarkupReference != null) &&
                    (!String.IsNullOrEmpty(_MarkupReference.KeyString) || !String.IsNullOrEmpty(_MarkupReference.Name)))
                element.Add(_MarkupReference.GetElement("MarkupReference"));
            if ((_CopyMarkupReference != null) &&
                    (!String.IsNullOrEmpty(_CopyMarkupReference.KeyString) || !String.IsNullOrEmpty(_CopyMarkupReference.Name)))
                element.Add(_CopyMarkupReference.GetElement("CopyMarkupReference"));
            if ((_OptionDescriptors != null) && (_OptionDescriptors.Count() != 0))
            {
                XElement optionDescriptorsElement = new XElement("OptionDescriptors");
                foreach (OptionDescriptor optionDescriptor in _OptionDescriptors)
                    optionDescriptorsElement.Add(optionDescriptor.Xml);
                element.Add(optionDescriptorsElement);
            }

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "ContentType":
                    ContentType = attributeValue;
                    break;
                case "ContentSubType":
                    ContentSubType = attributeValue;
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
                case "ContentItems":
                    {
                        List<MasterContentItem> contentItems = new List<MasterContentItem>();
                        foreach (XElement grandChildElement in childElement.Elements())
                            contentItems.Add(new MasterContentItem(grandChildElement));
                        ContentItems = contentItems;
                    }
                    break;
                case "MarkupReference":
                    _MarkupReference = new MarkupTemplateReference(childElement);
                    break;
                case "CopyMarkupReference":
                    _CopyMarkupReference = new MarkupTemplateReference(childElement);
                    break;
                case "OptionDescriptors":
                    foreach (XElement optionElement in childElement.Elements())
                    {
                        OptionDescriptor optionDescriptor = new OptionDescriptor(optionElement);
                        AddOptionDescriptor(optionDescriptor);
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public static List<IBaseObjectKeyed> ParseOptions(string value)
        {
            if (String.IsNullOrEmpty(value))
                return null;

            List<IBaseObjectKeyed> options = new List<IBaseObjectKeyed>();
            string[] optionStrings = value.Split(LanguageLookup.Commas, StringSplitOptions.RemoveEmptyEntries);

            if (optionStrings != null)
            {
                foreach (string optionString in optionStrings)
                {
                    string[] tokens = optionString.Trim().Split(LanguageLookup.Assigns, StringSplitOptions.RemoveEmptyEntries);
                    string optionName = "";
                    string optionValue = "";

                    if (tokens != null)
                    {
                        if (tokens.Count() >= 1)
                            optionName = tokens[0].Trim();

                        if (tokens.Count() >= 2)
                            optionValue = tokens[1].Trim();
                    }

                    if (!String.IsNullOrEmpty(optionName))
                        options.Add(new BaseString(optionName, optionValue));
                }
            }

            return options;
        }

        public static string FormatOptions(List<IBaseObjectKeyed> options)
        {
            if (options == null)
                return "";

            StringBuilder sb = new StringBuilder();
            int index = 0;

            foreach (IBaseObjectKeyed option in options)
            {
                if (option is BaseString)
                {
                    BaseString optionString = option as BaseString;

                    if (index == 0)
                        sb.Append(option.KeyString + "=" + optionString.Text);
                    else
                        sb.Append(", " + option.KeyString + "=" + optionString.Text);
                }

                index++;
            }

            string optionsString = sb.ToString();
            return optionsString;
        }

        /*
        public string GetOptionsString()
        {
            return FormatOptions(_Options);
        }
        */

        public override int Compare(IBaseObjectKeyed other)
        {
            if (other == this)
                return 0;

            if (other == null)
                return 1;

            MasterContentItem otherMasterContentItem = other as MasterContentItem;

            if (otherMasterContentItem == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareStrings(_ContentType, otherMasterContentItem.ContentType);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareStrings(_ContentSubType, otherMasterContentItem.ContentSubType);

            if (diff != 0)
                return diff;

            diff = MasterContentItem.CompareMasterContentItemLists(_ContentItems, otherMasterContentItem.ContentItems);

            if (diff != 0)
                return diff;

            diff = MarkupTemplateReference.Compare(_MarkupReference, otherMasterContentItem.MarkupReference);

            if (diff != 0)
                return diff;

            diff = MarkupTemplateReference.Compare(_CopyMarkupReference, otherMasterContentItem.CopyMarkupReference);

            if (diff != 0)
                return diff;

            diff = OptionDescriptor.CompareOptionDescriptors(_OptionDescriptors, otherMasterContentItem.OptionDescriptors);

            if (diff != 0)
                return diff;

            return diff;
        }

        public static int CompareMasterContentItems(MasterContentItem item1, MasterContentItem item2)
        {
            if (item1 == item2)
                return 0;

            if (item2 == null)
                return 1;

            if (item1 == null)
                return -1;

            int diff = item1.Compare(item2);

            return diff;
        }

        public static int CompareMasterContentItemLists(List<MasterContentItem> list1, List<MasterContentItem> list2)
        {
            if (list1 == list2)
                return 0;

            if (list2 == null)
                return 1;

            if (list1 == null)
                return -1;

            int count1 = list1.Count();
            int count2 = list2.Count();
            int count = (count1 > count2 ? count2 : count1);
            int index;
            int diff;

            for (index = 0; index > count; index++)
            {
                diff = CompareMasterContentItems(list1[index], list2[index]);

                if (diff != 0)
                    return diff;
            }

            diff = count1 - count2;

            return diff;
        }

        public static List<MasterContentItem> CopyMasterContentItemList(List<MasterContentItem> source)
        {
            if (source == null)
                return null;

            int count = source.Count();
            int index;
            List<MasterContentItem> list = new List<MasterContentItem>(count);

            for (index = 0; index < count; index++)
            {
                MasterContentItem newItem = new MasterContentItem(source[index]);
                list.Add(newItem);
            }

            return list;
        }
    }
}
