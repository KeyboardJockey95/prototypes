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
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Content
{
    public enum ContentClassType
    {
        None,
        DocumentItem,
        MediaList,
        MediaItem,
        StudyList
    };

    public class BaseObjectContent : BaseMarkupContainer
    {
        // Owning node.  Not stored in database, but set dynamically.
        protected BaseObjectNode _Node;
        // See the ContentTypes string list below.
        protected string _ContentType;
        // See the ContentSubTypes string list below.
        protected string _ContentSubType;
        // Content storage key.
        protected int _ContentStorageKey;
        // Content storage object.  Not saved, but set dynamically.
        BaseContentStorage _ContentStorage;
        public static List<string> ContentTypes = new List<string>()
        {
            "Document",
            "Audio",
            "Video",
            "Automated",
            "Image",
            "TextFile",
            "PDF",
            "Embedded",
            "Media",
            "Transcript",
            "Text",
            "Sentences",
            "Words",
            "Characters",
            "Expansion",
            "Exercises",
            "Notes",
            "Comments"
        };
        public static List<string> ContentSubTypes = new List<string>()
        {
            "Introduction",
            "Summary",
            "Grammar",
            "Culture",
            "Lesson",
            "Dialog",
            "Review",
            "Transcript",
            "Text",
            "Vocabulary",
            "Characters",
            "Expansion",
            "Exercises",
            "Notes",
            "NotesLite",
            "List",
            "Comments"
        };
        public static List<string> CommonKeys = new List<string>()
        {
            "Document",
            "Summary",
            "Grammar",
            "Culture",
            "MediaList",
            "AudioLesson",
            "AudioDialog",
            "AudioReview",
            "AudioVocabulary",
            "VideoLesson",
            "VideoDialog",
            "VideoReview",
            "VideoVocabulary",
            "PDFLesson",
            "TextFileLesson",
            "Transcript",
            "Text",
            "Dialog",
            "Sentences",
            "Words",
            "Characters",
            "Expansion",
            "Exercises",
            "Notes",
            "Comments"
        };
        public static List<string> CommonTextKeys = new List<string>()
        {
            "Transcript",
            "Text",
            "Dialog",
            "Sentences",
            "Words",
            "Characters",
            "Expansion",
            "Exercises",
            "Notes",
            "Comments"
        };
        public static List<string> NonStudyListTextContentTypes = new List<string>()
        {
            "Comments"
        };
        public static List<string> MediaItemTypes = new List<string>()
        {
            "Audio",
            "Video",
            "Automated",
            "Image",
            "TextFile",
            "PDF",
            "Embedded"
        };
        public static List<string> MediaItemSubTypes = new List<string>()
        {
            "Lesson",
            "Dialog",
            "Review",
            "Text",
            "Vocabulary",
            "Expansion",
            "Exercises",
            "Notes",
            "Grammar",
            "Culture"
        };
        public static List<string> TypicalToolStudyListContentTypes = new List<string>()
        {
            "Sentences",
            "Words",
            "Characters",
            "Expansion",
            "Exercises"
        };

        public BaseObjectContent(string key, MultiLanguageString title, MultiLanguageString description,
                string source, string package, string label, string imageFileName, int index, bool isPublic,
                List<LanguageID> targetLanguageIDs, List<LanguageID> hostLanguageIDs, string owner,
                BaseObjectNode node, string contentType, string contentSubType,
                BaseContentStorage contentStorage, List<IBaseObjectKeyed> options, MarkupTemplate markupTemplate,
                MarkupTemplateReference markupReference,
                BaseObjectContent contentParent, List<BaseObjectContent> contentChildren)
            : base(key, title, description, source, package, label, imageFileName, index,
                isPublic, targetLanguageIDs, hostLanguageIDs, owner,
                options, markupTemplate, markupReference, contentParent, contentChildren)
        {
            _Node = node;
            _ContentType = contentType;
            _ContentSubType = contentSubType;
            if (contentStorage != null)
                _ContentStorageKey = contentStorage.KeyInt;
            _ContentStorage = contentStorage;
        }

        public BaseObjectContent(BaseObjectNode node, BaseContentStorage contentStorage)
        {
            ClearBaseObjectContent();
            _Node = node;
            if (contentStorage != null)
                _ContentStorageKey = contentStorage.KeyInt;
            _ContentStorage = contentStorage;
        }

        public BaseObjectContent(object key)
            : base(key)
        {
            ClearBaseObjectContent();
        }

        public BaseObjectContent(BaseObjectContent other, object key)
            : base(other, key)
        {
            Copy(other);
            Modified = false;
        }

        public BaseObjectContent(BaseObjectContent other)
            : base(other)
        {
            Copy(other);
            Modified = false;
        }

        public BaseObjectContent(XElement element)
        {
            OnElement(element);
        }

        public BaseObjectContent()
        {
            ClearBaseObjectContent();
        }

        public void Copy(BaseObjectContent other)
        {
            base.Copy(other);

            if (other == null)
            {
                ClearBaseObjectContent();
                return;
            }

            _Node = other.Node;
            _ContentType = other.ContentType;
            _ContentSubType = other.ContentSubType;
            _ContentStorageKey = other.ContentStorageKey;

            ModifiedFlag = true;
        }

        public void CopyDeep(BaseObjectContent other)
        {
            Copy(other);
            base.CopyDeep(other);
        }

        public override void CopyLanguages(BaseObjectLanguages other)
        {
            base.CopyLanguages(other);

            if (_ContentStorage != null)
                _ContentStorage.PropagateLanguages(this);
        }

        public override void Clear()
        {
            base.Clear();
            ClearBaseObjectContent();
        }

        public void ClearBaseObjectContent()
        {
            _Node = null;
            _ContentType = null;
            _ContentSubType = null;
            _ContentStorageKey = 0;
        }

        public override IBaseObject Clone()
        {
            return new BaseObjectContent(this);
        }

        public void UpdateContentProfile(BaseObjectContent other)
        {
            Key = other.Key;
            CopyTitledObjectAndLanguages(other);
            ContentType = other.ContentType;
            ContentSubType = other.ContentSubType;
        }

        public override string ComposeDirectory()
        {
            string name;
            switch (ContentClass)
            {
                case ContentClassType.MediaItem:
                    name = "Media";
                    break;
                case ContentClassType.MediaList:
                    name = String.Empty;
                    break;
                case ContentClassType.StudyList:
                    name = MediaUtilities.FileFriendlyName(Name);
                    break;
                case ContentClassType.DocumentItem:
                    name = MediaUtilities.FileFriendlyName(Name);
                    break;
                default:
                    name = String.Empty;
                    break;
            }
            return name;
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
                        return MediaUtilities.ConcatenateUrlPath(ContentParent.MediaTildeUrl, directory);
                    else if (_Node != null)
                        return MediaUtilities.ConcatenateUrlPath(_Node.MediaTildeUrl, directory);

                    return String.Empty;
                }
            }
        }

        public virtual BaseObjectNode Node
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

        public virtual BaseObjectNodeTree Tree
        {
            get
            {
                if (_Node != null)
                {
                    if (_Node.IsTree())
                        return _Node as BaseObjectNodeTree;

                    return _Node.Tree;
                }
                return null;
            }
        }

        public BaseObjectNode NodeOrTree
        {
            get
            {
                BaseObjectNode node = Node;
                if (node == null)
                    node = Tree;
                return node;
            }
        }

        public string ContentRootKey
        {
            get
            {
                BaseObjectContent contentParent = ContentParent;
                string key;

                if (contentParent != null)
                    key = contentParent.ContentRootKey;
                else
                    key = KeyString;

                return key;
            }
        }

        public override BaseObjectContent ContentParent
        {
            get
            {
                if (!String.IsNullOrEmpty(_ContentParentKey) && (_Node != null))
                    return _Node.GetContent(_ContentParentKey);
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

        public BaseContentContainer ParentContainer
        {
            get
            {
                if (!String.IsNullOrEmpty(_ContentParentKey) && (_Node != null))
                    return _Node.GetContent(_ContentParentKey);
                return _Node;
            }
        }

        public override List<int> GetIndexPath()
        {
            List<int> indexPath;

            BaseObjectContent parentContent = ContentParent;
            BaseObjectNode node = Node;
            BaseObjectNodeTree tree = Tree;

            if (parentContent != null)
            {
                indexPath = parentContent.GetIndexPath();
                indexPath.Add(parentContent.GetChildContentIndex(this));
            }
            else if (node != null)
            {
                indexPath = node.GetIndexPath();
                indexPath.Add(node.GetContentIndex(this));
            }
            else if (tree != null)
            {
                indexPath = tree.GetIndexPath();
                indexPath.Add(tree.GetContentIndex(this));
            }
            else
                return new List<int>();

            return indexPath;
        }

        public override List<string> GetNamePath(LanguageID uiLanguageID)
        {
            List<string> namePath;

            BaseObjectContent parentContent = ContentParent;
            BaseObjectNode node = Node;
            BaseObjectNodeTree tree = Tree;

            if (parentContent != null)
                namePath = parentContent.GetNamePath(uiLanguageID);
            else if (node != null)
                namePath = node.GetNamePath(uiLanguageID);
            else if (tree != null)
                namePath = tree.GetNamePath(uiLanguageID);
            else
                return new List<string>() { GetTitleString(uiLanguageID) };

            namePath.Add(GetTitleString(uiLanguageID));

            return namePath;
        }

        public override string GetNamePathString(LanguageID uiLanguageID, string separator)
        {
            List<string> namePath = GetNamePath(uiLanguageID);
            string namePathString = TextUtilities.GetStringFromStringListDelimited(namePath, separator);
            return namePathString;
        }

        // This is used to determine the content class type.
        public ContentClassType ContentClass
        {
            get
            {
                if (_ContentStorage != null)
                    return _ContentStorage.ContentClass;

                ContentClassType classType;

                switch (_ContentType)
                {
                    case "Document":
                        classType = ContentClassType.DocumentItem;
                        break;
                    case "Audio":
                    case "Video":
                    case "Automated":
                    case "Image":
                    case "TextFile":
                    case "PDF":
                    case "Embedded":
                        classType = ContentClassType.MediaItem;
                        break;
                    case "Media":
                        classType = ContentClassType.MediaList;
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
                        classType = ContentClassType.StudyList;
                        break;
                    default:
                        classType = ContentClassType.DocumentItem;
                        break;
                }

                return classType;
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

        public bool WarnIfEmpty
        {
            get
            {
                bool warn = false;

                switch (ContentType)
                {
                    case "Document":
                        break;
                    case "Audio":
                    case "Video":
                    case "Image":
                    case "TextFile":
                    case "PDF":
                    case "Embedded":
                        warn = true;
                        break;
                    case "Automated":
                    case "Media":
                        break;
                    case "Transcript":
                    case "Text":
                    case "Sentences":
                    case "Words":
                    case "Characters":
                    case "Expansion":
                    case "Exercises":
                        warn = true;
                        break;
                    case "Notes":
                    case "Comments":
                        break;
                    default:
                        break;
                }

                return warn;
            }
        }

        public bool NeedsSentenceParsing
        {
            get
            {
                if (ContentClass == ContentClassType.StudyList)
                {
                    switch (ContentType)
                    {
                        case "Transcript":
                        case "Text":
                            return true;
                        default:
                            break;
                    }
                }
                return false;
            }
        }

        public bool NeedsWordParsing
        {
            get
            {
                if (ContentClass == ContentClassType.StudyList)
                {
                    switch (ContentType)
                    {
                        case "Transcript":
                        case "Text":
                        case "Sentences":
                        case "Expansion":
                            return true;
                        default:
                            break;
                    }
                }
                return false;
            }
        }

        public override string GetInheritedOptionValue(string optionKey)
        {
            string optionValue = GetOptionString(optionKey);

            if (!String.IsNullOrEmpty(optionValue))
            {
                if (optionValue != "Inherited")
                    return optionValue;
            }

            if (ContentParent != null)
                return ContentParent.GetInheritedOptionValue(optionKey);
            else if (Node != null)
                return Node.GetInheritedOptionValue(optionKey);
            else if (Tree != null)
                return Tree.GetInheritedOptionValue(optionKey);

            return null;
        }

        public override bool FindContainerAndOptionFlag(string optionKey, out BaseMarkupContainer container, out bool flag)
        {
            bool returnValue = base.FindContainerAndOptionFlag(optionKey, out container, out flag);

            if (returnValue)
                return returnValue;

            if (ContentParent != null)
                returnValue = ContentParent.FindContainerAndOptionFlag(optionKey, out container, out flag);
            else if (Node != null)
                returnValue = Node.FindContainerAndOptionFlag(optionKey, out container, out flag);
            else if (Tree != null)
                returnValue = Tree.FindContainerAndOptionFlag(optionKey, out container, out flag);

            return returnValue;
        }

        public int ContentStorageKey
        {
            get
            {
                return _ContentStorageKey;
            }
            set
            {
                if (_ContentStorageKey != value)
                {
                    _ContentStorageKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasContentStorageKey
        {
            get
            {
                return _ContentStorageKey > 0;
            }
        }

        public BaseContentStorage ContentStorage
        {
            get
            {
                if ((_ContentStorage == null) && (_ContentStorageKey > 0) && !String.IsNullOrEmpty(Source))
                {
                    // This is temporary, until the old media item media description is gone.
                    CurrentContent = this;
                    _ContentStorage = ApplicationData.Repositories.ResolveReference(Source, null, _ContentStorageKey) as BaseContentStorage;
                    if ((_ContentStorage != null) && _ContentStorage.Modified)
                    {
                        _ContentStorage.TouchAndClearModified();
                        ApplicationData.Repositories.UpdateReference(Source, null, _ContentStorage);
                    }
                    CurrentContent = null;

                    if (_ContentStorage != null)
                        _ContentStorage.Content = this;
                }

                return _ContentStorage;
            }
            set
            {
                _ContentStorage = value;
                if (_ContentStorage != null)
                {
                    _ContentStorage.Content = this;
                    int key = _ContentStorage.KeyInt;

                    if (key != _ContentStorageKey)
                    {
                        _ContentStorageKey = key;
                        ModifiedFlag = true;
                    }
                }
            }
        }

        public int ContentStorageReferenceCount
        {
            get
            {
                if (ContentStorage != null)
                    return ContentStorage.ReferenceCount;
                return 1;
            }
        }

        public T GetContentStorageTyped<T>() where T : BaseContentStorage
        {
            return ContentStorage as T;
        }

        public ContentDocumentItem ContentStorageDocumentItem
        {
            get
            {
                return ContentStorage as ContentDocumentItem;
            }
            set
            {
                ContentStorage = value;
            }
        }

        public ContentMediaItem ContentStorageMediaItem
        {
            get
            {
                return ContentStorage as ContentMediaItem;
            }
            set
            {
                ContentStorage = value;
            }
        }

        public ContentStudyList ContentStorageStudyList
        {
            get
            {
                return ContentStorage as ContentStudyList;
            }
            set
            {
                ContentStorage = value;
            }
        }

        public override List<BaseObjectContent> ContentChildren
        {
            get
            {
                return ContentList;
            }
            set
            {
                ContentList = value;
            }
        }

        public override bool AddContentChild(BaseObjectContent content)
        {
            if (base.AddContentChild(content))
            {
                content.ContentParent = this;
                return _Node.AddContent(content);
            }
            return false;
        }

        public override bool InsertContentChildIndexed(int index, BaseObjectContent content)
        {
            if (base.InsertContentChildIndexed(index, content))
            {
                content.ContentParent = this;
                return _Node.AddContent(content);
            }
            return false;
        }

        public override List<BaseObjectContent> ContentList
        {
            get
            {
                List<BaseObjectContent> contentList = null;

                if ((_ContentChildrenKeys != null) && (_ContentChildrenKeys != null))
                {
                    contentList = new List<BaseObjectContent>();

                    foreach (string key in _ContentChildrenKeys)
                    {
                        BaseObjectContent content = _Node.GetContent(key);

                        if (content != null)
                            contentList.Add(content);
                    }
                }

                return contentList;
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

        public List<BaseObjectContent> GetContentDescendents(List<BaseObjectContent> contentList = null)
        {
            List<BaseObjectContent> children = (contentList != null ? contentList : new List<BaseObjectContent>());

            if ((_Node != null) && (_Node.ChildCount() != 0))
            {
                foreach (BaseObjectNode childNode in _Node.Children)
                    childNode.CollectContent(_ContentType, _ContentSubType, children);
            }

            return children;
        }

        public List<BaseObjectContent> GetMediaContentDescendents(List<BaseObjectContent> contentList = null)
        {
            List<BaseObjectContent> children = (contentList != null ? contentList : new List<BaseObjectContent>());

            if ((_Node != null) && (_Node.ChildCount() != 0))
            {
                foreach (BaseObjectNode childNode in _Node.Children)
                    childNode.CollectContent(ContentClassType.MediaItem, children);
            }

            return children;
        }

        public override string TypeLabel
        {
            get
            {
                if ((ContentType != ContentSubType) && !String.IsNullOrEmpty(ContentSubType))
                    return ContentType + " " + ContentSubType;
                else
                    return ContentType;
            }
        }

        public override string GetTitleString(LanguageID uiLanguageID)
        {
            string str = base.GetTitleString(uiLanguageID);

            if (String.IsNullOrEmpty(str))
                str = TypeLabel;

            return str;
        }

        public string GetContentMessageLabel(LanguageID uiLanguageID)
        {
            string str = base.GetTitleString(uiLanguageID);

            if (String.IsNullOrEmpty(str))
                str = TypeLabel;

            if (ContentParent != null)
                str = ContentParent.GetTitleString(uiLanguageID) + " " + str;

            if (_Node != null)
                str = _Node.GetTitleString(uiLanguageID) + " " + str;

            return str;
        }
        
        public override List<OptionDescriptor> OptionDescriptors
        {
            get
            {
                return MasterContentItem.GetDefaultDescriptors(ContentType, ContentSubType, null);
            }
            set
            {
            }
        }

        public virtual void SetupContent(MasterContentItem contentItem, UserProfile userProfile)
        {
            ContentClassType contentClass = contentItem.ContentClass;

            Title = contentItem.CloneTitle();
            Description = contentItem.CloneDescription();
            Source = contentItem.Source;
            if (Node != null)
                Package = Node.Package;
            Label = contentItem.Label;
            ImageFileName = contentItem.ImageFileName;
            Index = contentItem.Index;
            IsPublic = contentItem.IsPublic;
            if ((contentItem.TargetLanguageIDs != null) && (contentItem.TargetLanguageIDs.Count() == 1) &&
                    ((contentItem.TargetLanguageIDs.First() == LanguageLookup.Target) ||
                        (contentItem.TargetLanguageIDs.First() == LanguageLookup.Host) ||
                        (contentItem.TargetLanguageIDs.First() == LanguageLookup.My)))
                TargetLanguageIDs = contentItem.ExpandTargetLanguageIDs(userProfile);
            else
                TargetLanguageIDs = contentItem.CloneTargetLanguageIDs();
            if ((contentItem.HostLanguageIDs != null) && (contentItem.HostLanguageIDs.Count() == 1) &&
                    ((contentItem.HostLanguageIDs.First() == LanguageLookup.Host) ||
                        (contentItem.HostLanguageIDs.First() == LanguageLookup.Target) ||
                        (contentItem.HostLanguageIDs.First() == LanguageLookup.My)))
                HostLanguageIDs = contentItem.ExpandHostLanguageIDs(userProfile);
            else
                HostLanguageIDs = contentItem.CloneHostLanguageIDs();
            Owner = contentItem.Owner;
            Key = contentItem.KeyString;
            ContentType = contentItem.ContentType;
            ContentSubType = contentItem.ContentSubType;

            EnsureGuid();

            SetupDirectory();
            SetupContentStorage();
            SetupOptions(contentItem);
            SetupMarkupTemplate(contentItem);
        }

        public virtual void CheckContent(MasterContentItem contentItem, UserProfile userProfile)
        {
            ContentClassType contentClass = contentItem.ContentClass;

            Title.Merge(contentItem.CloneTitle());
            Description.Merge(contentItem.CloneDescription());
            Source = contentItem.Source;
            if (Node != null)
                Package = Node.Package;
            Label = contentItem.Label;
            ImageFileName = contentItem.ImageFileName;
            Index = contentItem.Index;
            IsPublic = contentItem.IsPublic;
            List<LanguageID> languageIDs;
            if ((contentItem.TargetLanguageIDs != null) && (contentItem.TargetLanguageIDs.Count() == 1) &&
                    ((contentItem.TargetLanguageIDs.First() == LanguageLookup.Target) ||
                        (contentItem.TargetLanguageIDs.First() == LanguageLookup.Host) ||
                        (contentItem.TargetLanguageIDs.First() == LanguageLookup.My)))
                languageIDs = contentItem.ExpandTargetLanguageIDs(userProfile);
            else
                languageIDs = contentItem.CloneTargetLanguageIDs();
            LanguageID.MergeLanguageIDLists(TargetLanguageIDs, languageIDs);
            if ((contentItem.HostLanguageIDs != null) && (contentItem.HostLanguageIDs.Count() == 1) &&
                    ((contentItem.HostLanguageIDs.First() == LanguageLookup.Host) ||
                        (contentItem.HostLanguageIDs.First() == LanguageLookup.Target) ||
                        (contentItem.HostLanguageIDs.First() == LanguageLookup.My)))
                languageIDs = contentItem.ExpandHostLanguageIDs(userProfile);
            else
                languageIDs = contentItem.CloneHostLanguageIDs();
            if (HostLanguageIDs != null)
                LanguageID.MergeLanguageIDLists(HostLanguageIDs, languageIDs);
            Owner = contentItem.Owner;
            Key = contentItem.KeyString;
            EnsureGuid();
            ContentType = contentItem.ContentType;
            ContentSubType = contentItem.ContentSubType;
            SetupOptions(contentItem);
        }

        public virtual void SetupContentStorage()
        {
            BaseContentStorage contentStorage = null;
            ContentClassType contentClass = ContentClass;

            if (_ContentStorageKey > 0)
                return;

            ContentStorageKey = 0;

            switch (contentClass)
            {
                case ContentClassType.DocumentItem:
                    contentStorage = new ContentDocumentItem();
                    break;
                case ContentClassType.MediaList:
                    break;
                case ContentClassType.MediaItem:
                    contentStorage = new ContentMediaItem();
                    break;
                case ContentClassType.StudyList:
                    contentStorage = new ContentStudyList();
                    break;
                default:
                    break;
            }

            if (contentStorage != null)
                contentStorage.Content = this;

            ContentStorage = contentStorage;
        }

        public virtual void SetupOptions(MasterContentItem contentItem)
        {
            Options = GetOptionsFromDescriptors(contentItem.OptionDescriptors);

            if (ContentStorage != null)
                ContentStorage.SetupOptions(contentItem);
        }

        public virtual void SetupMarkupTemplate(MasterContentItem contentItem)
        {
            if (ContentStorage != null)
                ContentStorage.SetupMarkupTemplate(this, contentItem);
            else if (contentItem.IsLocalMarkupTemplate)
            {
                if (_LocalMarkupTemplate == null)
                {
                    if (contentItem.CopyMarkupTemplate != null)
                        LocalMarkupTemplate = new MarkupTemplate(contentItem.CopyMarkupTemplate);
                    else
                        LocalMarkupTemplate = new MarkupTemplate("(local)");

                    _LocalMarkupTemplate.LocalOwningObject = this;
                }
            }
            else
            {
                if (contentItem.MarkupReference != null)
                    MarkupReference = new MarkupTemplateReference(contentItem.MarkupReference);
                else
                    MarkupReference = null;
            }
        }

        public bool ExpectSentenceRuns()
        {
            switch (Label)
            {
                case "Sentences":
                case "Words":
                case "Characters":
                case "Expansion":
                case "Notes":
                case "Document":
                    return false;
                default:
                    return true;
            }
        }

        public bool ExpectWordRuns()
        {
            switch (Label)
            {
                case "Words":
                case "Characters":
                case "Expansion":
                case "Notes":
                case "Document":
                    return false;
                default:
                    return true;
            }
        }

        public bool CopyContent(BaseObjectNode sourceNodeOrTree, Dictionary<string, bool> sourceContentSelectFlags,
            Dictionary<string, bool> targetContentSelectFlags, CopyPasteType copyMode, NodeUtilities nodeUtilities)
        {
            bool returnValue = true;

            if (sourceNodeOrTree == null)
                return false;

            BaseObjectContent selectedTargetContent = null;
            BaseContentContainer parentTargetContent = null;
            int selectedTargetContentIndex = -1;
            BaseObjectNode node = Node;
            BaseObjectNodeTree localTree = node.Tree;

            if (localTree == null)
                localTree = node as BaseObjectNodeTree;

            switch (copyMode)
            {
                case CopyPasteType.Before:
                    selectedTargetContent = GetFirstSelectedContent(targetContentSelectFlags);
                    if (selectedTargetContent != null)
                        parentTargetContent = selectedTargetContent.ParentContainer;
                    if (parentTargetContent == null)
                        parentTargetContent = this;
                    if (selectedTargetContent != null)
                        selectedTargetContentIndex = parentTargetContent.GetChildContentIndex(selectedTargetContent);
                    if (targetContentSelectFlags != null)
                        nodeUtilities.InitializeSubContentSelectFlags(this, targetContentSelectFlags, false);
                    break;
                case CopyPasteType.Replace:
                    selectedTargetContent = GetFirstSelectedContent(targetContentSelectFlags);
                    if (selectedTargetContent != null)
                        parentTargetContent = selectedTargetContent.ParentContainer;
                    if (parentTargetContent == null)
                        parentTargetContent = this;
                    if (selectedTargetContent != null)
                        selectedTargetContentIndex = parentTargetContent.GetChildContentIndex(selectedTargetContent);
                    if (!nodeUtilities.DeleteNodeSelectedContentHelperNoUpdate(localTree, node, targetContentSelectFlags))
                        return false;
                    break;
                case CopyPasteType.After:
                    selectedTargetContent = GetLastSelectedContent(targetContentSelectFlags);
                    if (selectedTargetContent != null)
                        parentTargetContent = selectedTargetContent.ParentContainer;
                    if (parentTargetContent == null)
                        parentTargetContent = this;
                    if (selectedTargetContent != null)
                        selectedTargetContentIndex = parentTargetContent.GetChildContentIndex(selectedTargetContent);
                    if (selectedTargetContentIndex != -1)
                        selectedTargetContentIndex++;
                    if (targetContentSelectFlags != null)
                        nodeUtilities.InitializeSubContentSelectFlags(this, targetContentSelectFlags, false);
                    break;
                case CopyPasteType.Under:
                    selectedTargetContent = GetFirstSelectedContent(targetContentSelectFlags);
                    if (selectedTargetContent != null)
                        parentTargetContent = selectedTargetContent;
                    if (parentTargetContent == null)
                        parentTargetContent = this;
                    selectedTargetContentIndex = parentTargetContent.ContentChildrenCount();
                    if (targetContentSelectFlags != null)
                        nodeUtilities.InitializeSubContentSelectFlags(this, targetContentSelectFlags, false);
                    break;
                case CopyPasteType.Prepend:
                    parentTargetContent = this;
                    selectedTargetContentIndex = 0;
                    if (targetContentSelectFlags != null)
                        nodeUtilities.InitializeSubContentSelectFlags(this, targetContentSelectFlags, false);
                    break;
                case CopyPasteType.All:
                    parentTargetContent = this;
                    selectedTargetContentIndex = 0;
                    if (!nodeUtilities.DeleteContentAllSubContentHelperNoUpdate(localTree, node, this))
                        return false;
                    if (targetContentSelectFlags != null)
                        targetContentSelectFlags.Clear();
                    break;
                case CopyPasteType.Append:
                default:
                    parentTargetContent = this;
                    selectedTargetContentIndex = ContentChildrenCount();
                    if (targetContentSelectFlags != null)
                        nodeUtilities.InitializeSubContentSelectFlags(this, targetContentSelectFlags, false);
                    break;
            }

            if (!parentTargetContent.InsertSelectedContents(selectedTargetContentIndex, node,
                    sourceNodeOrTree, sourceContentSelectFlags, nodeUtilities, targetContentSelectFlags))
                returnValue = false;

            return returnValue;
        }

        public override bool CopyMedia(string newDirectoryRoot, List<string> copiedFiles, ref string errorMessage)
        {
            if (ContentStorage != null)
                return ContentStorage.CopyMedia(newDirectoryRoot, copiedFiles, ref errorMessage);

            return true;
        }

        public static BaseObjectContent CurrentContent;

        public override void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            if ((_ContentStorageKey != 0) && !String.IsNullOrEmpty(Source))
            {
                if (_ContentStorage == null)
                {
                    // This is temporary, until the old media item media description is gone.
                    CurrentContent = this;
                    _ContentStorage = mainRepository.ResolveReference(Source, null, _ContentStorageKey) as BaseContentStorage;

                    if ((_ContentStorage != null) && _ContentStorage.Modified)
                    {
                        _ContentStorage.TouchAndClearModified();
                        mainRepository.UpdateReference(Source, null, _ContentStorage);
                    }

                    CurrentContent = null;
                }
                else
                    _ContentStorage = mainRepository.CacheCheckReference(Source, null, _ContentStorage) as BaseContentStorage;

                if (_ContentStorage != null)
                    _ContentStorage.Content = this;
            }

            if (recurseChildren && HasContent())
            {
                foreach (BaseObjectContent content in ContentChildren)
                    content.ResolveReferences(mainRepository, recurseParents, recurseChildren);
            }

            base.ResolveReferences(mainRepository, recurseParents, recurseChildren);
        }

        public override bool SaveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if ((_ContentStorage != null) && !String.IsNullOrEmpty(Source))
            {
                returnValue = _ContentStorage.SaveReferences(mainRepository, recurseParents, recurseChildren);
                returnValue = mainRepository.SaveReference(Source, null, _ContentStorage) && returnValue;
            }

            if (!base.SaveReferences(mainRepository, recurseParents, recurseChildren))
                returnValue = false;

            return returnValue;
        }

        public override bool UpdateReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if ((_ContentStorage != null) && !String.IsNullOrEmpty(Source))
            {
                returnValue = _ContentStorage.UpdateReferences(mainRepository, recurseParents, recurseChildren);
                returnValue = mainRepository.UpdateReference(Source, null, _ContentStorage) && returnValue;
            }

            if (!base.UpdateReferences(mainRepository, recurseParents, recurseChildren))
                returnValue = false;

            return returnValue;
        }

        public override bool UpdateReferencesCheck(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if ((_ContentStorage != null) && !String.IsNullOrEmpty(Source))
            {
                returnValue = _ContentStorage.UpdateReferences(mainRepository, recurseParents, recurseChildren);

                if (_ContentStorage.Modified)
                    returnValue = mainRepository.UpdateReference(Source, null, _ContentStorage) && returnValue;
            }

            if (!base.UpdateReferencesCheck(mainRepository, recurseParents, recurseChildren))
                returnValue = false;

            return returnValue;
        }

        public override void ClearReferences(bool recurseParents, bool recurseChildren)
        {
            base.ClearReferences(recurseParents, recurseChildren);
            _ContentStorage = null;
        }

        public override void CollectReferences(List<IBaseObjectKeyed> references, List<IBaseObjectKeyed> externalSavedChildren,
            List<IBaseObjectKeyed> externalNonSavedChildren, Dictionary<int, bool> nodeSelectFlags,
            Dictionary<string, bool> contentSelectFlags, Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles, VisitMedia visitFunction)
        {
            base.CollectReferences(references, externalSavedChildren, externalNonSavedChildren, nodeSelectFlags,
                contentSelectFlags, itemSelectFlags, languageSelectFlags, mediaFiles, visitFunction);

            if (ContentStorage != null)
            {
                bool useThis = true;

                if (contentSelectFlags != null)
                {
                    if (!contentSelectFlags.TryGetValue(KeyString, out useThis))
                        useThis = false;
                }

                if (useThis)
                {
                    AddUniqueReference(externalSavedChildren, _ContentStorage);
                    _ContentStorage.CollectReferences(references, externalSavedChildren, externalNonSavedChildren,
                        nodeSelectFlags, contentSelectFlags, itemSelectFlags, languageSelectFlags, mediaFiles, visitFunction);
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (!String.IsNullOrEmpty(_ContentType))
                element.Add(new XAttribute("ContentType", _ContentType));

            if (!String.IsNullOrEmpty(_ContentSubType))
                element.Add(new XAttribute("ContentSubType", _ContentSubType));

            if (_ContentStorageKey > 0)
                element.Add(new XAttribute("ContentStorageKey", _ContentStorageKey.ToString()));

            return element;
        }

        public override XElement GetElementFiltered(string name, Dictionary<int, bool> childNodeFlags,
            Dictionary<string, bool> childContentFlags)
        {
            XElement element = base.GetElementFiltered(name, childNodeFlags, childContentFlags);

            if (!String.IsNullOrEmpty(_ContentType))
                element.Add(new XAttribute("ContentType", _ContentType));

            if (!String.IsNullOrEmpty(_ContentSubType))
                element.Add(new XAttribute("ContentSubType", _ContentSubType));

            if (_ContentStorageKey > 0)
                element.Add(new XAttribute("ContentStorageKey", _ContentStorageKey.ToString()));

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "ContentType":
                    _ContentType = attributeValue;
                    break;
                case "ContentSubType":
                    _ContentSubType = attributeValue;
                    break;
                case "ContentStorageKey":
                    _ContentStorageKey = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override void OnFixup(FixupDictionary fixups)
        {
            base.OnFixup(fixups);

            if ((_ContentStorage == null) && (_ContentStorageKey != 0))
            {
                string xmlKey = _ContentStorageKey.ToString();
                BaseContentStorage contentStorage = fixups.Get(Source, xmlKey) as BaseContentStorage;

                if (contentStorage != null)
                {
                    _ContentStorageKey = contentStorage.KeyInt;
                    _ContentStorage = contentStorage;
                    _ContentStorage.Content = this;
                    ModifiedFlag = true;
                }
                else
                    ResolveReferences(fixups.Repositories, false, false);
            }
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            BaseObjectContent otherBaseObjectContent = other as BaseObjectContent;

            if (otherBaseObjectContent == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareStrings(_ContentType, otherBaseObjectContent.ContentType);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareStrings(_ContentSubType, otherBaseObjectContent.ContentSubType);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareInts(_ContentStorageKey, otherBaseObjectContent.ContentStorageKey);

            return diff;
        }

        public static int Compare(BaseObjectContent object1, BaseObjectContent object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareKeys(BaseObjectContent object1, BaseObjectContent object2)
        {
            return ObjectUtilities.CompareKeys(object1, object2);
        }

        public static List<string> GetContentSubTypes(string contentType)
        {
            List<string> contentSubTypes;
            switch (contentType)
            {
                case "Document":
                    contentSubTypes = new List<string>() { "Summary", "Grammar", "Culture", "Introduction" };
                    break;
                case "Audio":
                case "Video":
                case "Automated":
                    contentSubTypes = new List<string>()
                    {
                        "Lesson",
                        "Dialog",
                        "Review",
                        "Vocabulary"
                    };
                    break;
                case "Image":
                    contentSubTypes = new List<string>()
                    {
                        "Lesson",
                        "Dialog",
                        "Review",
                        "Vocabulary"
                    };
                    break;
                case "TextFile":
                case "PDF":
                case "Embedded":
                    contentSubTypes = new List<string>()
                    {
                        "Notes",
                        "Introduction",
                        "Summary",
                        "Grammar",
                        "Culture",
                        "Lesson",
                        "Dialog",
                        "Review",
                        "Text",
                        "Vocabulary",
                        "Expansion",
                        "Exercises"
                    };
                    break;
                case "Media":
                    contentSubTypes = new List<string>()
                    {
                        "List"
                    };
                    break;
                case "Transcript":
                    contentSubTypes = new List<string>()
                    {
                        "Text"
                    };
                    break;
                case "Text":
                    contentSubTypes = new List<string>()
                    {
                        "Text"
                    };
                    break;
                case "Sentences":
                case "Words":
                case "Characters":
                    contentSubTypes = new List<string>()
                    {
                        "Vocabulary"
                    };
                    break;
                case "Expansion":
                    contentSubTypes = new List<string>()
                    {
                        "Vocabulary"
                    };
                    break;
                case "Exercises":
                    contentSubTypes = new List<string>()
                    {
                        "Exercises",
                        "Vocabulary"
                    };
                    break;
                case "Notes":
                    contentSubTypes = new List<string>()
                    {
                        "Notes"
                    };
                    break;
                case "Comments":
                    contentSubTypes = new List<string>()
                    {
                        "Comments"
                    };
                    break;
                default:
                    contentSubTypes = new List<string>(ContentSubTypes);
                    break;
            }
            return contentSubTypes;
        }
    }
}
