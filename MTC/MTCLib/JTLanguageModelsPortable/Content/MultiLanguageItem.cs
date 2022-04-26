using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Content
{
    public class MultiLanguageItem : BaseObjectKeyed
    {
        protected List<LanguageItem> _LanguageItems;
        protected string _SpeakerNameKey;
        protected List<Annotation> _Annotations;
        protected List<MultiLanguageItemReference> _ExpansionReferences;
        protected MultiLanguageItemReference _ItemSource;
        protected string _MediaTildeUrl;        // Not saved.
        protected ContentStudyList _StudyList;  // Not saved.

        public MultiLanguageItem(object key, List<LanguageItem> languageItems, string speakerNameKey,
                List<Annotation> annotations, List<MultiLanguageItemReference> expansionReferences,
                ContentStudyList studyList)
            : base(key)
        {
            if (languageItems == null)
                _LanguageItems = new List<LanguageItem>();
            else
                _LanguageItems = languageItems;

            _SpeakerNameKey = speakerNameKey;
            _Annotations = annotations;
            _ExpansionReferences = expansionReferences;
            _ItemSource = null;
            _MediaTildeUrl = null;
            _StudyList = studyList;
        }

        public MultiLanguageItem(object key, List<LanguageItem> languageItems)
            : base(key)
        {
            if (languageItems == null)
                _LanguageItems = new List<LanguageItem>();
            else
                _LanguageItems = languageItems;

            _SpeakerNameKey = null;
            _Annotations = null;
            _ExpansionReferences = null;
            _ItemSource = null;
            _MediaTildeUrl = null;
            _StudyList = null;
        }

        public MultiLanguageItem(object key, LanguageItem languageItem)
            : base(key)
        {
            if (languageItem == null)
                _LanguageItems = new List<LanguageItem>();
            else
                _LanguageItems = new List<LanguageItem>() { languageItem };

            _SpeakerNameKey = null;
            _Annotations = null;
            _ExpansionReferences = null;
            _ItemSource = null;
            _MediaTildeUrl = null;
            _StudyList = null;
        }

        public MultiLanguageItem(object key, List<LanguageDescriptor> languageDescriptors)
            : base(key)
        {
            _LanguageItems = new List<LanguageItem>(languageDescriptors.Count());

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (languageDescriptor.Used && (languageDescriptor.LanguageID != null))
                    _LanguageItems.Add(new LanguageItem(key, languageDescriptor.LanguageID, ""));
            }

            _SpeakerNameKey = null;
            _Annotations = null;
            _ExpansionReferences = null;
            _ItemSource = null;
            _MediaTildeUrl = null;
            _StudyList = null;
        }

        public MultiLanguageItem(
                object key,
                List<LanguageDescriptor> targetLanguageDescriptors,
                List<LanguageDescriptor> hostLanguageDescriptors)
            : base(key)
        {
            _LanguageItems = new List<LanguageItem>(targetLanguageDescriptors.Count() + hostLanguageDescriptors.Count());

            foreach (LanguageDescriptor languageDescriptor in targetLanguageDescriptors)
            {
                if (languageDescriptor.Used && (languageDescriptor.LanguageID != null))
                    _LanguageItems.Add(new LanguageItem(key, languageDescriptor.LanguageID, ""));
            }

            foreach (LanguageDescriptor languageDescriptor in hostLanguageDescriptors)
            {
                if (languageDescriptor.Used && (languageDescriptor.LanguageID != null) && !HasLanguageID(languageDescriptor.LanguageID))
                    _LanguageItems.Add(new LanguageItem(key, languageDescriptor.LanguageID, ""));
            }

            _SpeakerNameKey = null;
            _Annotations = null;
            _ExpansionReferences = null;
            _ItemSource = null;
            _MediaTildeUrl = null;
            _StudyList = null;
        }

        public MultiLanguageItem(object key, List<LanguageID> languageIDs)
            : base(key)
        {
            _LanguageItems = new List<LanguageItem>(languageIDs.Count());

            foreach (LanguageID languageID in languageIDs)
                _LanguageItems.Add(new LanguageItem(key, languageID, ""));

            _SpeakerNameKey = null;
            _Annotations = null;
            _ExpansionReferences = null;
            _ItemSource = null;
            _MediaTildeUrl = null;
            _StudyList = null;
        }

        public MultiLanguageItem(object key, List<LanguageID> languageIDs, MultiLanguageString text)
            : base(key)
        {
            _LanguageItems = new List<LanguageItem>(languageIDs.Count());

            foreach (LanguageID languageID in languageIDs)
            {
                string str = text.Text(languageID);
                _LanguageItems.Add(new LanguageItem(key, languageID, str));
            }

            _SpeakerNameKey = null;
            _Annotations = null;
            _ExpansionReferences = null;
            _ItemSource = null;
            _MediaTildeUrl = null;
            _StudyList = null;
        }

        public MultiLanguageItem(
                object key,
                List<LanguageID> targetLanguageIDs,
                List<LanguageID> hostLanguageIDs)
            : base(key)
        {
            int count = (hostLanguageIDs != null ? hostLanguageIDs.Count() : 0) + (targetLanguageIDs != null ? targetLanguageIDs.Count() : 0);
            _LanguageItems = new List<LanguageItem>(count);
            foreach (LanguageID languageID in targetLanguageIDs)
                _LanguageItems.Add(new LanguageItem(key, languageID, ""));
            foreach (LanguageID languageID in hostLanguageIDs)
            {
                if (!HasLanguageID(languageID))
                    _LanguageItems.Add(new LanguageItem(key, languageID, ""));
            }
            _SpeakerNameKey = null;
            _Annotations = null;
            _ExpansionReferences = null;
            _ItemSource = null;
            _MediaTildeUrl = null;
            _StudyList = null;
        }

        public MultiLanguageItem(object key, List<LanguageDescriptor> languageDescriptors, string speakerNameKey, List<Annotation> annotations,
            List<MultiLanguageItemReference> expansionReferences, ContentStudyList studyList)
        {
            Key = key;
            SpeakerNameKey = speakerNameKey;
            _LanguageItems = new List<LanguageItem>(languageDescriptors.Count());
            List<LanguageID> languageIDs = new List<LanguageID>(languageDescriptors.Count());

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (languageDescriptor.Used && (languageDescriptor.LanguageID != null))
                {
                    LanguageID languageID = languageDescriptor.LanguageID;
                    _LanguageItems.Add(new LanguageItem(key, languageID, ""));
                    languageIDs.Add(languageID);
                }
            }

            _Annotations = Annotation.FilteredAnnotations(annotations, languageIDs);
            _ExpansionReferences = expansionReferences;
            _ItemSource = null;
            _MediaTildeUrl = null;
            _StudyList = studyList;
        }

        public MultiLanguageItem(object key, LanguageID languageID, string text)
            : base(key)
        {
            LanguageItem languageItem = new LanguageItem(key, languageID, text);
            _LanguageItems = new List<LanguageItem>() { languageItem };
            _Annotations = null;
            _ExpansionReferences = null;
            _ItemSource = null;
            _MediaTildeUrl = null;
            _StudyList = null;
        }

        public MultiLanguageItem(object key, DictionaryEntry dictionaryEntry, List<LanguageID> languageIDs,
                string speakerNameKey)
            : base(key)
        {
            string definition;
            LanguageItem languageItem;

            _LanguageItems = new List<LanguageItem>();
            _SpeakerNameKey = speakerNameKey;
            _Annotations = null;
            _ExpansionReferences = null;
            _ItemSource = null;
            _MediaTildeUrl = null;
            _StudyList = null;

            foreach (LanguageID languageID in languageIDs)
            {
                definition = dictionaryEntry.GetDefinition(languageID, false, false);
                languageItem = new LanguageItem(key, languageID, definition);
                Add(languageItem);
            }
        }

        public MultiLanguageItem(MultiLanguageString other)
            : base(other)
        {
            CopyText(other);
            ModifiedFlag = false;
        }

        public MultiLanguageItem(object key, MultiLanguageString other)
            : base(key)
        {
            CopyText(other);
            ModifiedFlag = false;
        }

        public MultiLanguageItem(MultiLanguageItem other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public MultiLanguageItem(object key, MultiLanguageItem other)
            : base(key)
        {
            CopyText(other);
            ModifiedFlag = false;
        }

        public MultiLanguageItem(object key, MultiLanguageItem other, List<LanguageID> languageIDs)
            : base(key)
        {
            CopyFiltered(other, languageIDs);
            ModifiedFlag = false;
        }

        public MultiLanguageItem(XElement element)
        {
            OnElement(element);
        }

        public MultiLanguageItem()
        {
            ClearMultiLanguageItem();
        }

        public override void Clear()
        {
            base.Clear();
            ClearMultiLanguageItem();
        }

        public void ClearMultiLanguageItem()
        {
            _LanguageItems = new List<LanguageItem>();
            _SpeakerNameKey = null;
            _Annotations = null;
            _ExpansionReferences = null;
            _ItemSource = null;
            _MediaTildeUrl = null;
            _StudyList = null;
        }

        public virtual void Copy(MultiLanguageItem other)
        {
            if (other == null)
            {
                Clear();
                return;
            }

            base.Copy(other);

            _LanguageItems = other.CloneLanguageItems();
            _SpeakerNameKey = other.SpeakerNameKey;
            _Annotations = other.CloneAnnotations();
            _ExpansionReferences = other.CloneExpansionReferences();
            _ItemSource = CloneItemSource();
            _MediaTildeUrl = other.MediaTildeUrl;
            _StudyList = other.StudyList;
            ModifiedFlag = true;
        }

        public virtual void CopyDeep(MultiLanguageItem other)
        {
            this.Copy(other);
        }

        public override IBaseObject Clone()
        {
            return new MultiLanguageItem(this);
        }

        public override string Owner
        {
            get
            {
                if (_StudyList != null)
                    return _StudyList.Owner;
                return base.Owner;
            }
        }

        public void PropagateLanguages(List<LanguageID> languageIDs)
        {
            List<LanguageItem> languageItems = new List<LanguageItem>();

            if (languageIDs != null)
            {
                foreach (LanguageID languageID in languageIDs)
                {
                    LanguageItem languageItem = null;

                    if (_LanguageItems != null)
                        languageItem = _LanguageItems.FirstOrDefault(x => x.LanguageID == languageID);

                    if (languageItem == null)
                    {
                        languageItem = new LanguageItem(Key, languageID, String.Empty);
                        ModifiedFlag = true;
                    }

                    languageItems.Add(languageItem);
                }

                if ((_LanguageItems == null) || (languageItems.Count != _LanguageItems.Count))
                    ModifiedFlag = true;

                _LanguageItems = languageItems;
            }
        }

        public MultiLanguageString GetMultiLanguageString(string key)
        {
            MultiLanguageString mls = new MultiLanguageString(key);

            if (_LanguageItems != null)
            {
                foreach (LanguageItem languageItem in _LanguageItems)
                {
                    LanguageString ls = new LanguageString(key, languageItem.LanguageID, languageItem.Text);
                    mls.Add(ls);
                }
            }

            return mls;
        }

        public MultiLanguageString GetHeading()
        {
            MultiLanguageString mls = new MultiLanguageString("Heading");

            if (_LanguageItems != null)
            {
                foreach (LanguageItem languageItem in _LanguageItems)
                {
                    string text = String.Empty;

                    foreach (LanguageItem li in _LanguageItems)
                    {
                        if (li == languageItem)
                            continue;

                        if (String.IsNullOrEmpty(text))
                            text = li.Text + " ";
                        else
                            text += "[" + li.Text + "] ";
                    }

                    text += "(" + languageItem.Text + ")";

                    LanguageString ls = new LanguageString("Heading", languageItem.LanguageID, text);
                    mls.Add(ls);
                }
            }

            return mls;
        }

        public Annotation GetHeadingAnnotation()
        {
            MultiLanguageString mls = GetHeading();
            Annotation annotation = new Annotation("Heading", null, mls);
            return annotation;
        }

        public MultiLanguageItem CreateSentenceStudyItem(int sentenceIndex)
        {
            MultiLanguageItem multiLanguageItem = new MultiLanguageItem(
                Key,
                CloneSentenceLanguageItems(sentenceIndex));
            return multiLanguageItem;
        }

        public List<LanguageItem> LanguageItems
        {
            get
            {
                return _LanguageItems;
            }
            set
            {
                if (_LanguageItems != value)
                    ModifiedFlag = true;

                _LanguageItems = value;
            }
        }

        public bool HasSpeakerNameKey
        {
            get
            {
                return !String.IsNullOrEmpty(_SpeakerNameKey);
            }
        }

        public string SpeakerNameKey
        {
            get
            {
                return _SpeakerNameKey;
            }
            set
            {
                if (_SpeakerNameKey != value)
                    ModifiedFlag = true;

                _SpeakerNameKey = value;
            }
        }

        public MultiLanguageString SpeakerName
        {
            get
            {
                if (!String.IsNullOrEmpty(_SpeakerNameKey))
                {
                    if (StudyList != null)
                        return StudyList.GetSpeakerName(_SpeakerNameKey);
                }
                return null;
            }
            set
            {
                if (value == null)
                    SpeakerNameKey = null;
                else
                    SpeakerNameKey = value.KeyString;
            }
        }

        public List<Annotation> Annotations
        {
            get
            {
                return _Annotations;
            }
            set
            {
                if (_Annotations != value)
                    ModifiedFlag = true;

                _Annotations = value;
            }
        }

        public List<MultiLanguageItemReference> ExpansionReferences
        {
            get
            {
                return _ExpansionReferences;
            }
            set
            {
                if (_ExpansionReferences != value)
                    ModifiedFlag = true;

                _ExpansionReferences = value;
            }
        }

        public MultiLanguageItemReference ItemSource
        {
            get
            {
                return _ItemSource;
            }
            set
            {
                if (_ItemSource != value)
                    ModifiedFlag = true;

                _ItemSource = value;
            }
        }

        public bool HasItemSource
        {
            get
            {
                return (_ItemSource != null);
            }
        }

        public MultiLanguageItem ItemSourceItem
        {
            get
            {
                if (_ItemSource != null)
                    return _ItemSource.Item;
                return null;
            }
        }

        public bool HasItemSourceItem
        {
            get
            {
                return (_ItemSource != null) && (_ItemSource.Item != null);
            }
        }

        public string MediaTildeUrl
        {
            get
            {
                if (!String.IsNullOrEmpty(_MediaTildeUrl))
                    return _MediaTildeUrl;

                if ((_ItemSource != null) && (_ItemSource.Item != null))
                {
                    string url = _ItemSource.Item.MediaTildeUrl;

                    if (!String.IsNullOrEmpty(url))
                        return url;
                }

                if (_StudyList != null)
                    return _StudyList.MediaTildeUrl;

                return null;
            }
            set
            {
                _MediaTildeUrl = value;
            }
        }

        public string MediaDirectoryPath
        {
            get
            {
                try
                {
                    return ApplicationData.MapToFilePath(MediaTildeUrl);
                }
                catch (Exception)
                {
                    return String.Empty;
                }
            }
        }

        public ContentStudyList StudyList
        {
            get
            {
                return _StudyList;
            }
            set
            {
                _StudyList = value;
            }
        }

        public object StudyListKey
        {
            get
            {
                if (_StudyList != null)
                    return _StudyList.Key;

                return null;
            }
        }

        public BaseObjectContent Content
        {
            get
            {
                if (_StudyList != null)
                    return _StudyList.Content;
                return null;
            }
            set
            {
                if (value != null)
                    _StudyList = value.ContentStorageStudyList;
                else
                    _StudyList = null;
            }
        }

        public string ContentKey
        {
            get
            {
                if (_StudyList != null)
                    return _StudyList.Content.KeyString;

                return null;
            }
        }

        public string ContentName(LanguageID uiLanguageID)
        {
            BaseObjectContent content = Content;

            if (content != null)
                return content.GetTitleString(uiLanguageID);

            return String.Empty;
        }

        public string StudyListNodeKey
        {
            get
            {
                if (_StudyList != null)
                    return _StudyList.Content.KeyString;

                return null;
            }
        }

        public object NodeKey
        {
            get
            {
                if (Node != null)
                    return Node.Key;

                return null;
            }
        }

        public BaseObjectNode Node
        {
            get
            {
                if (_StudyList != null)
                    return _StudyList.Node;

                return null;
            }
        }

        public string NodeName(LanguageID uiLanguageID)
        {
            BaseObjectNode node = Node;

            if (node != null)
                return node.GetTitleString(uiLanguageID);

            return String.Empty;
        }

        public BaseObjectNodeTree Tree
        {
            get
            {
                BaseObjectNode node = Node;
                if (node != null)
                {
                    if (node is BaseObjectNodeTree)
                        return node as BaseObjectNodeTree;
                    return node.Tree;
                }

                return null;
            }
        }

        public string TreeName(LanguageID uiLanguageID)
        {
            BaseObjectNodeTree tree = Tree;

            if (tree != null)
                return tree.GetTitleString(uiLanguageID);

            return String.Empty;
        }

        public string CompoundStudyItemKey
        {
            get
            {
                object nodeKeyObject = NodeKey;
                string nodeKey = (nodeKeyObject != null ? nodeKeyObject.ToString() : String.Empty);
                string contentKey = ContentKey;
                string studyItemKey = nodeKey + "_" + contentKey + "_" + KeyString;
                return studyItemKey;
            }
        }

        public List<int> GetIndexPath()
        {
            List<int> indexPath;

            BaseObjectContent content = Content;

            if (content != null)
            {
                indexPath = content.GetIndexPath();
                ContentStudyList studyList = content.ContentStorageStudyList;
                if (studyList != null)
                    indexPath.Add(studyList.GetStudyItemIndex(this));
            }
            else
                indexPath = new List<int>();

            return indexPath;
        }

        public List<string> GetNamePath(LanguageID uiLanguageID)
        {
            List<string> namePath;

            BaseObjectContent content = Content;

            if (content != null)
                namePath = content.GetNamePath(uiLanguageID);
            else
                namePath = new List<string>();

            return namePath;
        }

        public string GetNamePathString(LanguageID uiLanguageID, string separator)
        {
            string namePathString;
            BaseObjectContent content = Content;

            if (content != null)
                namePathString = content.GetNamePathString(uiLanguageID, separator);
            else
                namePathString = String.Empty;

            return namePathString;
        }

        public List<string> GetNamePathInclusive(LanguageID uiLanguageID)
        {
            List<string> namePath;

            BaseObjectContent content = Content;

            if (content != null)
            {
                namePath = content.GetNamePath(uiLanguageID);
                namePath.Add(KeyString);
            }
            else
                namePath = new List<string>() { KeyString };

            return namePath;
        }

        public string GetNamePathStringInclusive(LanguageID uiLanguageID, string separator = null)
        {
            List<string> namePath = GetNamePathInclusive(uiLanguageID);
            int i = 0;
            string namePathString = String.Empty;

            if (separator == null)
                separator = "/";

            foreach (string name in namePath)
            {
                if (i != 0)
                    namePathString += separator;

                namePathString += name;
                i++;
            }

            return namePathString;
        }

        public string GetNamePathStringWithOrdinalInclusive(LanguageID uiLanguageID, string separator = null)
        {
            List<string> namePath = GetNamePathInclusive(uiLanguageID);
            int i = 0;
            string namePathString = String.Empty;

            Annotation ordinalAnnotation = FindAnnotation("Ordinal");

            if (ordinalAnnotation != null)
                namePath[namePath.Count() - 1] = ordinalAnnotation.Value;

            if (separator == null)
                separator = "/";

            foreach (string name in namePath)
            {
                if (i != 0)
                    namePathString += separator;

                namePathString += name;
                i++;
            }

            return namePathString;
        }

        public string GetNameStringWithOrdinal()
        {
            string nameString = String.Empty;

            Annotation ordinalAnnotation = FindAnnotation("Ordinal");

            if (ordinalAnnotation != null)
                nameString = ordinalAnnotation.Value;
            else
                nameString = KeyString;

            return nameString;
        }

        public string GetFullTitleString(LanguageID uiLanguageID)
        {
            List<string> namePath = GetNamePath(uiLanguageID);
            int i = 0;
            string title = String.Empty;

            foreach (string name in namePath)
            {
                if (i != 0)
                    title += "/";

                title += name;
                i++;
            }

            return title;
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_LanguageItems != null)
                {
                    foreach (LanguageItem languageItem in _LanguageItems)
                    {
                        if (languageItem.Modified)
                            return true;
                    }
                }

                if (_Annotations != null)
                {
                    foreach (Annotation annotation in _Annotations)
                    {
                        if (annotation.Modified)
                            return true;
                    }
                }

                if (_ExpansionReferences != null)
                {
                    foreach (MultiLanguageItemReference expansionReference in _ExpansionReferences)
                    {
                        if (expansionReference.Modified)
                            return true;
                    }
                }

                if (_ItemSource != null)
                {
                    if (_ItemSource.Modified)
                        return true;
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_LanguageItems != null)
                {
                    foreach (LanguageItem languageItem in _LanguageItems)
                        languageItem.Modified = false;
                }

                if (_Annotations != null)
                {
                    foreach (Annotation annotation in _Annotations)
                        annotation.Modified = false;
                }

                if (_ExpansionReferences != null)
                {
                    foreach (MultiLanguageItemReference expansionReference in _ExpansionReferences)
                        expansionReference.Modified = false;
                }

                if (_ItemSource != null)
                    _ItemSource.Modified = false;
            }
        }

        public List<LanguageItem> Lookup(Matcher matcher)
        {
            IEnumerable<LanguageItem> lookupQuery =
                from languageItem in _LanguageItems
                where (matcher.Match(languageItem))
                select languageItem;

            if (lookupQuery != null)
                return lookupQuery.ToList();

            return null;
        }

        public LanguageItem LanguageItem(LanguageID languageID)
        {
            if (_LanguageItems != null)
                return _LanguageItems.FirstOrDefault(ls => ls.LanguageID == languageID);

            return (null);
        }

        public LanguageItem LanguageItem(int index)
        {
            if ((_LanguageItems != null) && (index >= 0) && (index < _LanguageItems.Count()))
                return _LanguageItems.ElementAt(index);

            return null;
        }

        public LanguageItem LanguageItemFuzzy(LanguageID languageID)
        {
            if (_LanguageItems != null)
            {
                LanguageItem languageItem = _LanguageItems.FirstOrDefault(ls => ls.LanguageID == languageID);

                if (languageItem == null)
                    languageItem = _LanguageItems.FirstOrDefault(
                        ls => (ls.LanguageID.LanguageCode == languageID.LanguageCode)
                            && (ls.LanguageID.CultureCode == null)
                            && (ls.LanguageID.ExtensionCode == languageID.ExtensionCode));

                if ((languageItem == null) && (languageID.ExtensionCode != null))
                    languageItem = _LanguageItems.FirstOrDefault(
                        ls => (ls.LanguageID.LanguageCode == languageID.LanguageCode)
                            && (ls.LanguageID.ExtensionCode == languageID.ExtensionCode));

                if ((languageItem == null) && (languageID.CultureCode == null) && (languageID.ExtensionCode == null))
                    languageItem = _LanguageItems.FirstOrDefault(ls => ls.LanguageID.LanguageCode == languageID.LanguageCode);

                if (languageItem == null)
                    languageItem = _LanguageItems.FirstOrDefault(ls => ls.LanguageID.LanguageCode == null);

                return languageItem;
            }

            return (null);
        }

        public LanguageItem RootLanguageItem(LanguageID rootLanguageID)
        {
            if (_LanguageItems == null)
                return null;

            LanguageItem rootLanguageItem = _LanguageItems.FirstOrDefault(ls => ls.LanguageID == rootLanguageID);

            if (rootLanguageItem != null)
                return rootLanguageItem;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (LanguageLookup.IsSameFamily(languageItem.LanguageID, rootLanguageID))
                    return languageItem;
            }

            return (null);
        }

        public LanguageItem LanguageItemMedia(LanguageID languageID)
        {
            if (languageID == null)
                return null;

            if (_LanguageItems != null)
            {
                LanguageItem languageItem = _LanguageItems.FirstOrDefault(ls => ls.LanguageID == languageID);

                if (languageItem == null)
                    languageItem = _LanguageItems.FirstOrDefault(
                        ls => (ls.LanguageID.LanguageCode == languageID.LanguageCode));

                return languageItem;
            }

            return (null);
        }

        public List<LanguageItem> CloneLanguageItems()
        {
            if (_LanguageItems == null)
                return null;

            List<LanguageItem> returnValue = new List<LanguageItem>(_LanguageItems.Count());

            foreach (LanguageItem languageItem in _LanguageItems)
                returnValue.Add(new LanguageItem(languageItem));

            return returnValue;
        }

        public List<LanguageItem> CloneSentenceLanguageItems(int sentenceIndex)
        {
            if (_LanguageItems == null)
                return null;

            List<LanguageItem> returnValue = new List<LanguageItem>(_LanguageItems.Count());

            foreach (LanguageItem languageItem in _LanguageItems)
                returnValue.Add(languageItem.CloneSentenceLanguageItem(sentenceIndex));

            return returnValue;
        }

        public bool HasText()
        {
            if (_LanguageItems == null)
                return false;
            foreach (LanguageItem languageItem in _LanguageItems)
                if (languageItem.HasText())
                    return true;
            return false;
        }

        public bool HasText(LanguageID languageID)
        {
            LanguageItem ls = LanguageItem(languageID);
            if (ls != null)
                return ls.HasText();
            return false;
        }

        public bool HasText(List<LanguageID> languageIDs)
        {
            if (_LanguageItems == null)
                return false;
            if (languageIDs == null)
                return false;
            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (!languageIDs.Contains(languageItem.LanguageID))
                    continue;
                if (languageItem.HasText())
                    return true;
            }
            return false;
        }

        public bool HasAllText(List<LanguageID> languageIDs)
        {
            if (_LanguageItems == null)
                return false;
            if (languageIDs == null)
                return false;
            int textCount = 0;
            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (!languageIDs.Contains(languageItem.LanguageID))
                    continue;
                if (languageItem.HasText())
                    textCount++;
            }
            if (textCount == languageIDs.Count())
                return true;
            return false;
        }

        public string Text(LanguageID languageID)
        {
            LanguageItem ls = LanguageItem(languageID);
            if (ls != null)
                return ls.Text;
            return String.Empty;
        }

        public void SetText(LanguageID languageID, string text)
        {
            LanguageItem ls = LanguageItem(languageID);
            if (ls != null)
                ls.Text = text;
            else
            {
                ls = new LanguageItem(Key, languageID, text);
                Add(ls);
            }
        }

        public string Text(int index)
        {
            LanguageItem ls = LanguageItem(index);
            if (ls != null)
                return ls.Text;
            return String.Empty;
        }

        public void SetText(int index, LanguageID languageID, string text)
        {
            LanguageItem ls = LanguageItem(index);
            if (ls != null)
                ls.Text = text;
            else
            {
                ls = new LanguageItem(Key, languageID, text);
                Insert(index, ls);
            }
        }

        public string RunText(LanguageID languageID, int sentenceIndex)
        {
            LanguageItem ls = LanguageItem(languageID);
            if (ls != null)
                return ls.GetRunText(sentenceIndex);
            return String.Empty;
        }

        public static string ConcatenatedText(
            List<MultiLanguageItem> studyItems,
            LanguageID languageID)
        {
            if (studyItems == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                string text = studyItem.Text(languageID);
                sb.Append(text);
            }

            return sb.ToString();
        }

        public static string ConcatenatedText(
            List<MultiLanguageItem> studyItems,
            List<LanguageID> languageIDs)
        {
            if (studyItems == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();

            foreach (LanguageID languageID in languageIDs)
            {
                foreach (MultiLanguageItem studyItem in studyItems)
                {
                    string text = studyItem.Text(languageID);
                    sb.Append(text);
                }
            }

            return sb.ToString();
        }

        public List<string> GetWords(LanguageID languageID)
        {
            LanguageItem languageItem = LanguageItem(languageID);

            if (languageItem == null)
                return new List<string>();

            return languageItem.GetWords();
        }

        public List<string> GetWords(List<LanguageID> languageIDs)
        {
            List<string> words = new List<string>();

            foreach (LanguageID languageID in languageIDs)
                words.AddRange(GetWords(languageID));

            return words;
        }

        public static List<string> GetWords(
            List<MultiLanguageItem> studyItems,
            LanguageID languageID)
        {
            List<string> words = new List<string>();

            foreach (MultiLanguageItem studyItem in studyItems)
                words.AddRange(studyItem.GetWords(languageID));

            return words;
        }

        public static List<string> GetWords(
            List<MultiLanguageItem> studyItems,
            List<LanguageID> languageIDs)
        {
            List<string> words = new List<string>();

            foreach (LanguageID languageID in languageIDs)
                words.AddRange(GetWords(studyItems, languageID));

            return words;
        }

        public List<string> GetUniqueWords(LanguageID languageID)
        {
            LanguageItem languageItem = LanguageItem(languageID);

            if (languageItem == null)
                return new List<string>();

            return languageItem.GetUniqueWords();
        }

        public List<string> GetUniqueWords(List<LanguageID> languageIDs)
        {
            List<string> words = new List<string>();
            HashSet<string> hashSet = new HashSet<string>();

            foreach (LanguageID languageID in languageIDs)
            {
                List<string> subWords = GetUniqueWords(languageID);

                foreach (string word in subWords)
                {
                    if (hashSet.Add(word))
                        words.Add(word);
                }
            }

            return words;
        }

        public static List<string> GetUniqueWords(
            List<MultiLanguageItem> studyItems,
            LanguageID languageID)
        {
            List<string> words = new List<string>();
            HashSet<string> hashSet = new HashSet<string>();

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                List<string> subWords = studyItem.GetUniqueWords(languageID);

                foreach (string word in subWords)
                {
                    if (hashSet.Add(word))
                        words.Add(word);
                }
            }

            return words;
        }

        public static List<string> GetUniqueWords(
            List<MultiLanguageItem> studyItems,
            List<LanguageID> languageIDs)
        {
            List<string> words = new List<string>();
            HashSet<string> hashSet = new HashSet<string>();

            foreach (LanguageID languageID in languageIDs)
            {
                foreach (MultiLanguageItem studyItem in studyItems)
                {
                    List<string> subWords = studyItem.GetUniqueWords(languageID);

                    foreach (string word in subWords)
                    {
                        if (hashSet.Add(word))
                            words.Add(word);
                    }
                }
            }

            return words;
        }

        public string TextMedia(LanguageID languageID)
        {
            LanguageItem ls = LanguageItemMedia(languageID);
            if (ls != null)
                return ls.Text;
            return String.Empty;
        }

        public string TextFuzzy(LanguageID languageID)
        {
            LanguageItem ls = LanguageItemFuzzy(languageID);
            if (ls != null)
                return ls.Text;
            return String.Empty;
        }

        public List<string> StringList()
        {
            List<string> stringList = new List<string>(Count());

            if (_LanguageItems != null)
            {
                foreach (LanguageItem ls in _LanguageItems)
                    stringList.Add(ls.Text == null ? String.Empty : ls.Text);
            }

            return stringList;
        }

        public List<string> StringList(List<LanguageDescriptor> languageDescriptors)
        {
            List<string> stringList = new List<string>(Count());

            if (_LanguageItems != null)
            {
                foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
                {
                    if (!languageDescriptor.Used || !languageDescriptor.Show)
                        continue;

                    LanguageItem languageItem = LanguageItem(languageDescriptor.LanguageID);

                    if (languageItem != null)
                        stringList.Add(languageItem.Text == null ? String.Empty : languageItem.Text);
                }
            }

            return stringList;
        }

        public List<string> StringList(List<LanguageID> languageIDs)
        {
            List<string> stringList = new List<string>(Count());

            if (_LanguageItems != null)
            {
                foreach (LanguageID languageID in languageIDs)
                {
                    LanguageItem languageItem = LanguageItem(languageID);

                    if (languageItem != null)
                        stringList.Add(languageItem.Text == null ? String.Empty : languageItem.Text);
                }
            }

            return stringList;
        }

        public string[] StringArray()
        {
            return StringList().ToArray();
        }

        public string[] StringArray(List<LanguageDescriptor> languageDescriptors)
        {
            return StringList(languageDescriptors).ToArray();
        }

        public string[] StringArray(List<LanguageID> languageIDs)
        {
            return StringList(languageIDs).ToArray();
        }

        public string GetStringListString(List<LanguageID> languageIDs)
        {
            string stringList = String.Empty;
            int count = 0;

            foreach (LanguageID languageID in languageIDs)
            {
                string text = Text(languageID);

                if (count != 0)
                    stringList += ",";

                stringList += text;
                count++;
            }

            return stringList;
        }

        public bool IsEmpty()
        {
            if (_LanguageItems != null)
            {
                foreach (LanguageItem languageItem in LanguageItems)
                {
                    if (!languageItem.IsEmpty())
                        return false;
                }
            }

            return true;
        }

        public bool IsTextMatch(
            MultiLanguageItem other,
            List<LanguageID> languageIDs)
        {
            if (other == null)
                return false;

            if (_LanguageItems == null)
                return false;

            bool returnValue = true;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (!languageItem.HasText())
                    continue;

                LanguageID languageID = languageItem.LanguageID;

                if ((languageIDs == null) || languageIDs.Contains(languageID))
                {
                    string otherText = other.Text(languageID);
                    string thisText = languageItem.Text;

                    if (otherText != thisText)
                    {
                        returnValue = false;
                        break;
                    }
                }
            }

            return returnValue;
        }

        public bool IsTextMatch(
            MultiLanguageString text,
            List<LanguageID> languageIDs)
        {
            if (text == null)
                return false;

            if (_LanguageItems == null)
                return false;

            bool returnValue = true;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (!languageItem.HasText())
                    continue;

                LanguageID languageID = languageItem.LanguageID;

                if ((languageIDs == null) || languageIDs.Contains(languageID))
                {
                    string otherText = text.Text(languageID);
                    string thisText = languageItem.Text;

                    if (otherText != thisText)
                    {
                        returnValue = false;
                        break;
                    }
                }
            }

            return returnValue;
        }

        public bool IsTextMatch(
            string text,
            LanguageID languageID)
        {
            if (_LanguageItems == null)
                return false;

            LanguageItem languageItem = LanguageItem(languageID);

            if (languageItem == null)
                return false;

            if (!languageItem.HasText())
                return false;

            string thisText = languageItem.Text;

            if (text == thisText)
                return true;

            return false;
        }

        public bool IsCaseInsensitiveTextMatch(
            MultiLanguageItem other,
            List<LanguageID> languageIDs)
        {
            if (other == null)
                return false;

            if (_LanguageItems == null)
                return false;

            bool returnValue = true;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (!languageItem.HasText())
                    continue;

                LanguageID languageID = languageItem.LanguageID;

                if ((languageIDs == null) || languageIDs.Contains(languageID))
                {
                    string otherText = other.Text(languageID).ToLower();
                    string thisText = languageItem.Text.ToLower();

                    if (otherText != thisText)
                    {
                        returnValue = false;
                        break;
                    }
                }
            }

            return returnValue;
        }

        public bool IsCaseInsensitiveTextMatch(
            MultiLanguageString text,
            List<LanguageID> languageIDs)
        {
            if (text == null)
                return false;

            if (_LanguageItems == null)
                return false;

            bool returnValue = true;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (!languageItem.HasText())
                    continue;

                LanguageID languageID = languageItem.LanguageID;

                if ((languageIDs == null) || languageIDs.Contains(languageID))
                {
                    string otherText = text.Text(languageID).ToLower();
                    string thisText = languageItem.Text.ToLower();

                    if (otherText != thisText)
                    {
                        returnValue = false;
                        break;
                    }
                }
            }

            return returnValue;
        }

        public bool IsCaseInsensitiveTextMatch(
            string text,
            LanguageID languageID)
        {
            if (_LanguageItems == null)
                return false;

            LanguageItem languageItem = LanguageItem(languageID);

            if (languageItem == null)
                return false;

            if (!languageItem.HasText())
                return false;

            string thisText = languageItem.Text.ToLower();

            if (text.ToLower() == thisText)
                return true;

            return false;
        }

        public LanguageID LanguageID(int index)
        {
            LanguageItem ls = LanguageItem(index);
            if (ls != null)
                return ls.LanguageID;
            return null;
        }

        public string Language(int index)
        {
            LanguageItem ls = LanguageItem(index);
            if (ls != null)
                return ls.LanguageID.Language;
            return String.Empty;
        }

        public bool HasLanguageID(LanguageID languageID)
        {
            LanguageItem ls = LanguageItem(languageID);
            if (ls != null)
                return true;
            return false;
        }

        public bool HasLanguageID(Matcher filter)
        {
            if (LanguageItems == null)
                return false;

            foreach (LanguageItem languageItem in LanguageItems)
            {
                if (filter.Match(languageItem.LanguageID))
                    return true;
            }

            return false;
        }

        public List<LanguageID> GetFamilyLanguageIDs(LanguageID languageID)
        {
            List<LanguageID> familyLanguageIDs = new List<LanguageID>();

            if (LanguageItems == null)
                return familyLanguageIDs;

            foreach (LanguageItem languageItem in LanguageItems)
            {
                LanguageID lid = languageItem.LanguageID;

                if ((lid == languageID) || LanguageLookup.IsSameFamily(lid, languageID))
                    familyLanguageIDs.Add(lid);
            }

            return familyLanguageIDs;
        }

        public List<LanguageID> GetSiblingLanguageIDs(LanguageID languageID)
        {
            List<LanguageID> siblingLanguageIDs = new List<LanguageID>();

            if (LanguageItems == null)
                return siblingLanguageIDs;

            foreach (LanguageItem languageItem in LanguageItems)
            {
                LanguageID lid = languageItem.LanguageID;

                if ((lid != languageID) && LanguageLookup.IsSameFamily(lid, languageID))
                    siblingLanguageIDs.Add(lid);
            }

            return siblingLanguageIDs;
        }

        public bool NeedsTranslation()
        {
            int hasTextCount = 0;
            int emptyCount = 0;

            if (LanguageItems == null)
                return false;

            foreach (LanguageItem languageItem in LanguageItems)
            {
                if (languageItem.HasText())
                    hasTextCount++;
                else
                    emptyCount++;
            }

            if ((hasTextCount != 0) && (emptyCount != 0))
                return true;

            return false;
        }

        public bool Add(LanguageItem languageItem)
        {
            if (LanguageItem(languageItem.LanguageID) == null)
            {
                if (_LanguageItems == null)
                    _LanguageItems = new List<LanguageItem>(1) { languageItem };
                else
                    _LanguageItems.Add(languageItem);
                ModifiedFlag = true;
                return true;
            }
            return false;
        }

        public bool Insert(int index, LanguageItem languageItem)
        {
            if (LanguageItem(index) != languageItem)
            {
                if (_LanguageItems == null)
                    _LanguageItems = new List<LanguageItem>(1) { languageItem };
                else
                    _LanguageItems.Insert(index, languageItem);
                ModifiedFlag = true;
                return true;
            }
            return false;
        }

        public bool Delete(LanguageItem languageItem)
        {
            if (_LanguageItems != null)
            {
                if (_LanguageItems.Remove(languageItem))
                {
                    ModifiedFlag = true;
                    return true;
                }
            }
            return false;
        }

        public bool Delete(int index)
        {
            if ((_LanguageItems != null) && (index >= 0) && (index < _LanguageItems.Count()))
            {
                _LanguageItems.RemoveAt(index);
                ModifiedFlag = true;
                return true;
            }
            return false;
        }

        public void DeleteAll()
        {
            if (_LanguageItems != null)
                ModifiedFlag = true;
            _LanguageItems = null;
        }

        public List<LanguageID> LanguageIDs
        {
            get
            {
                List<LanguageID> languageIDs = new List<LanguageID>();

                if (_LanguageItems != null)
                {
                    foreach (LanguageItem languageItem in _LanguageItems)
                    {
                        if (languageItem.LanguageID != null)
                            languageIDs.Add(languageItem.LanguageID);
                    }
                }

                return languageIDs;
            }
        }

        public bool Reorder(LanguageIDMatcher languageIDMatcher)
        {
            int index = 0;
            bool wasReordered = false;
            int count = languageIDMatcher.LanguageIDs.Count();
            LanguageID languageID;

            foreach (LanguageItem ls in _LanguageItems)
            {
                if (index == count)
                {
                    wasReordered = true;
                    break;
                }


                languageID = languageIDMatcher.LanguageIDs[index++];

                if (!LanguageIDMatcher.MatchLanguageIDs(MatchCode.Exact, languageID, ls.LanguageID))
                {
                    wasReordered = true;
                    break;
                }
            }

            if (wasReordered)
            {
                List<LanguageItem> list = new List<LanguageItem>(_LanguageItems.Count());

                foreach (LanguageID lid in languageIDMatcher.LanguageIDs)
                {
                    LanguageItem ls = _LanguageItems.FirstOrDefault(s => s.LanguageID == lid);

                    if (ls != null)
                        list.Add(ls);
                    else
                    {
                        ls = _LanguageItems.FirstOrDefault(s => LanguageIDMatcher.MatchLanguageIDs(MatchCode.Exact, lid, s.LanguageID));

                        if (ls != null)
                            list.Add(ls);
                    }
                }

                _LanguageItems = list;
                ModifiedFlag = true;
            }

            return wasReordered;
        }

        public bool IsOverlapping(MultiLanguageItem other)
        {
            if ((Count() == 0) || other.Count() == 0)
                return false;

            foreach (LanguageItem newLanguageItem in other.LanguageItems)
            {
                LanguageItem oldLanguageItem = LanguageItem(newLanguageItem.LanguageID);

                if (oldLanguageItem == null)
                    return false;

                if (!oldLanguageItem.IsOverlapping(newLanguageItem))
                    return false;
            }

            if (AnnotationCount() == other.AnnotationCount())
            {
                if (AnnotationCount() != 0)
                {
                    foreach (Annotation newAnnotation in other.Annotations)
                    {
                        Annotation oldAnnotation = FindAnnotation(newAnnotation.Type);

                        if (oldAnnotation == null)
                            return false;

                        if (!oldAnnotation.IsOverlapping(newAnnotation))
                            return false;
                    }
                }
            }
            else
                return false;

            return true;
        }

        public bool IsOverlappingAnchored(MultiLanguageItem other,
            Dictionary<string, bool> anchorLanguageFlags)
        {
            if ((Count() == 0) || other.Count() == 0)
                return false;

            foreach (LanguageItem newLanguageItem in other.LanguageItems)
            {
                LanguageID languageID = newLanguageItem.LanguageID;

                if (anchorLanguageFlags != null)
                {
                    bool useIt = false;

                    if (anchorLanguageFlags.TryGetValue(languageID.LanguageCultureExtensionCode, out useIt))
                    {
                        if (!useIt)
                            continue;
                    }
                }

                LanguageItem oldLanguageItem = LanguageItem(languageID);

                if (oldLanguageItem == null)
                    return false;

                if (!oldLanguageItem.IsOverlapping(newLanguageItem))
                    return false;
            }

            if (AnnotationCount() == other.AnnotationCount())
            {
                if (AnnotationCount() != 0)
                {
                    foreach (Annotation newAnnotation in other.Annotations)
                    {
                        Annotation oldAnnotation = FindAnnotation(newAnnotation.Type);

                        if (oldAnnotation == null)
                            return false;

                        if (!oldAnnotation.IsOverlappingAnchored(newAnnotation, anchorLanguageFlags))
                            return false;
                    }
                }
            }
            else
                return false;

            return true;
        }

        public void CopyText(MultiLanguageItem other)
        {
            if (_LanguageItems == null)
                _LanguageItems = new List<LanguageItem>(other._LanguageItems.Capacity);
            else
            {
                _LanguageItems.Clear();
                _LanguageItems.Capacity = other._LanguageItems.Capacity;
            }

            foreach (LanguageItem languageItem in other.LanguageItems)
                _LanguageItems.Add(new LanguageItem(languageItem));

            if (other.AnnotationCount() != 0)
            {
                foreach (Annotation annotation in other.Annotations)
                    AddAnnotation(new Annotation(annotation));
            }

            ModifiedFlag = true;
        }

        public void CopyText(MultiLanguageString other)
        {
            if (_LanguageItems == null)
                _LanguageItems = new List<LanguageItem>(other.LanguageStrings.Capacity);
            else
            {
                _LanguageItems.Clear();
                _LanguageItems.Capacity = other.LanguageStrings.Capacity;
            }

            foreach (LanguageString languageString in other.LanguageStrings)
                _LanguageItems.Add(new LanguageItem(Key, languageString.LanguageID, languageString.Text));

            ModifiedFlag = true;
        }

        public void CopyFiltered(MultiLanguageItem other, List<LanguageID> languageIDs)
        {
            _LanguageItems = other.FilteredLanguageItems(languageIDs);
            _Annotations = other.FilteredAnnotations(languageIDs);
            ModifiedFlag = true;
        }

        public void CopyFiltered(MultiLanguageItem other, List<LanguageDescriptor> languageDescriptors)
        {
            _LanguageItems = other.FilteredLanguageItems(languageDescriptors);
            _Annotations = other.FilteredAnnotations(languageDescriptors);
            ModifiedFlag = true;
        }

        public void CopyFiltered(MultiLanguageString other, List<LanguageDescriptor> languageDescriptors)
        {
            if (_LanguageItems == null)
                _LanguageItems = new List<LanguageItem>(languageDescriptors.Count());
            else
            {
                _LanguageItems.Clear();
                _LanguageItems.Capacity = languageDescriptors.Count();
            }

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (!languageDescriptor.Used)
                    continue;
                LanguageID languageID = languageDescriptor.LanguageID;
                LanguageString languageString = other.LanguageString(languageID);
                if (languageString != null)
                    _LanguageItems.Add(new LanguageItem(Key, languageID, languageString.Text));
                else
                    _LanguageItems.Add(new LanguageItem(Key, languageID, String.Empty));
            }

            ModifiedFlag = true;
        }

        public void MergeUnconditional(MultiLanguageItem other, bool isJoinSentences)
        {
            List<LanguageItem> list = new List<LanguageItem>(_LanguageItems.Count() + other.Count());
            foreach (LanguageItem ls1 in _LanguageItems)
                list.Add(ls1);
            foreach (LanguageItem ls2 in other.LanguageItems)
            {
                bool found = false;
                foreach (LanguageItem ls3 in list)
                {
                    if (ls3.LanguageID == ls2.LanguageID)
                    {
                        if (String.IsNullOrEmpty(ls3.Text) && String.IsNullOrEmpty(ls2.Text))
                            continue;
                        else if (String.IsNullOrEmpty(ls3.Text))
                        {
                            ls3.Text = ls2.Text;
                            ls3.SentenceRuns = ls2.CloneSentenceRuns();
                            ls3.WordRuns = ls2.CloneWordRuns();
                        }
                        else if (!String.IsNullOrEmpty(ls2.Text))
                        {
                            if (isJoinSentences)
                                ls3.MergeAndJoin(ls2);
                            else
                                ls3.Merge(ls2);
                        }
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    LanguageItem ls4 = new LanguageItem(ls2);
                    ls4.Key = Key;
                    if (isJoinSentences)
                        ls4.JoinSentenceRuns();
                    list.Add(ls4);
                }
            }
            _LanguageItems = list;
            MergeAnnotations(other);
            ModifiedFlag = true;
        }

        // Returns false if any items conflict.
        public bool Merge(MultiLanguageItem other)
        {
            List<LanguageItem> list = new List<LanguageItem>(_LanguageItems.Count() + other.Count());
            bool found = false;
            foreach (LanguageItem ls1 in _LanguageItems)
                list.Add(ls1);
            foreach (LanguageItem ls2 in other.LanguageItems)
            {
                foreach (LanguageItem ls3 in list)
                {
                    if (ls3.MatchKey(ls2.Key) && (ls3.LanguageID == ls2.LanguageID))
                    {
                        if (ls3.Text != ls2.Text)
                        {
                            if (!String.IsNullOrEmpty(ls3.Text) && !String.IsNullOrEmpty(ls2.Text))
                                return false;
                            else if (!String.IsNullOrEmpty(ls2.Text))
                            {
                                ls3.Text = ls2.Text;
                                ls3.SentenceRuns = ls2.CloneSentenceRuns();
                                ls3.WordRuns = ls2.CloneWordRuns();
                            }
                        }
                        if ((ls3.SentenceRunCount() != 0) && (ls2.SentenceRunCount() == 0))
                            ls2.LoadSentenceRunsFromText();
                        if (ls3.SentenceRunCount() != ls2.SentenceRunCount())
                            return false;
                        ls3.MergeMediaRuns(ls2);
                        found = true;
                        break;
                    }
                }
                if (!found)
                    list.Add(ls2);
            }
            _LanguageItems = list;
            MergeAnnotations(other);
            ModifiedFlag = true;
            return true;
        }

        // Merge items, contatenating differing language items with a "; " separator.
        public void MergeConcatSentences(MultiLanguageItem other)
        {
            List<LanguageItem> list = new List<LanguageItem>(_LanguageItems.Count() + other.Count());
            bool found = false;
            foreach (LanguageItem ls1 in _LanguageItems)
                list.Add(ls1);
            foreach (LanguageItem ls2 in other.LanguageItems)
            {
                foreach (LanguageItem ls3 in list)
                {
                    if (ls3.LanguageID == ls2.LanguageID)
                    {
                        ls3.MergeConcatSentences(ls2);
                        found = true;
                        break;
                    }
                }
                if (!found)
                    list.Add(ls2);
            }
            _LanguageItems = list;
            ModifiedFlag = true;
        }

        // Overwrite other onto this, but leave language items in this that are not in other alone.
        public bool MergeOverwrite(MultiLanguageItem other)
        {
            foreach (LanguageItem lsOther in other.LanguageItems)
            {
                LanguageItem lsThis = LanguageItem(lsOther.LanguageID);
                if (lsThis == null)
                {
                    lsThis = new LanguageItem(lsOther);
                    Add(lsThis);
                }
                else if (!String.IsNullOrEmpty(lsOther.Text))
                {
                    if ((lsThis.SentenceRunCount() != 0) && (lsOther.SentenceRunCount() == 0))
                        lsOther.LoadSentenceRunsFromText();
                    lsOther.MergeMediaRuns(lsThis);
                    lsThis.Text = lsOther.Text;
                    lsThis.SentenceRuns = lsOther.CloneSentenceRuns();
                    lsThis.WordRuns = lsOther.CloneWordRuns();
                }
            }
            MergeAnnotations(other);
            ModifiedFlag = true;
            return true;
        }

        public void UpdateWordRunsFromText(bool needsWordParsing, List<LanguageID> languageIDs, DictionaryRepository dictionary)
        {
            if (languageIDs == null)
                return;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                languageItem.UpdateWordRunsFromText(needsWordParsing, dictionary);
            }
        }

        public void PrimeSentenceRunsForWordItem(List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                languageItem.PrimeSentenceRunsForWordItem();
            }
        }

        public bool HasAlignment(LanguageID targetLanguageID, LanguageID hostLanguageID)
        {
            if ((targetLanguageID == null) || (hostLanguageID == null))
                return false;

            LanguageItem targetLanguageItem = LanguageItem(targetLanguageID);
            LanguageItem hostLanguageItem = LanguageItem(hostLanguageID);

            if ((targetLanguageItem == null) || (hostLanguageItem == null))
                return false;

            return targetLanguageItem.HasAlignment(hostLanguageID);
        }

        public MultiLanguageString GetAlignedText(
            LanguageID targetLanguageID,
            int targetWordIndex,
            List<LanguageID> hostLanguageIDs)
        {
            if (targetLanguageID == null)
                return null;

            LanguageItem targetLanguageItem = LanguageItem(targetLanguageID);

            if (targetLanguageItem == null)
                return null;

            List<TextRun> targetWordRuns = targetLanguageItem.WordRuns;

            if ((targetWordRuns == null) || (targetWordRuns.Count() == 0))
                return null;

            if ((targetWordIndex < 0) || (targetWordIndex >= targetWordRuns.Count()))
                return null;

            TextRun targetWordRun = targetWordRuns[targetWordIndex];

            if (targetWordRun == null)
                return null;

            string targetWord = targetLanguageItem.GetRunText(targetWordRun);

            if (hostLanguageIDs == null)
                return null;

            MultiLanguageString mls = new MultiLanguageString(targetWord);
            HashSet<string> hostWordsUsed = new HashSet<string>();

            foreach (LanguageID hostLanguageID in hostLanguageIDs)
            {
                LanguageItem hostLanguageItem = LanguageItem(hostLanguageID);

                if (hostLanguageItem == null)
                    continue;

                List<TextRun> hostWordRuns = hostLanguageItem.WordRuns;

                if ((hostWordRuns == null) || (hostWordRuns.Count() == 0))
                    continue;

                int hostWordCount = hostWordRuns.Count();
                WordMapping wordMapping = targetWordRun.GetWordMapping(hostLanguageID);

                if ((wordMapping == null) || !wordMapping.HasWordIndexes())
                    continue;

                string hostWords = String.Empty;

                hostWordsUsed.Clear();

                foreach (int hostWordIndex in wordMapping.WordIndexes)
                {
                    if ((hostWordIndex >= 0) && (hostWordIndex < hostWordCount))
                    {
                        TextRun hostWordRun = hostWordRuns[hostWordIndex];
                        string hostWord = hostLanguageItem.GetRunText(hostWordRun);

                        if (hostWordsUsed.Add(hostWord.ToLower()))
                            hostWords += (String.IsNullOrEmpty(hostWords) ? "" : " ") + hostWord;
                    }
                }

                if (String.IsNullOrEmpty(hostWords))
                    continue;

                mls.Add(new LanguageString(targetWord, hostLanguageID, hostWords));
            }

            if (mls.Count() == 0)
                return null;

            return mls;
        }

        public MultiLanguageString GetAlignedPhrase(
            LanguageID targetLanguageID,
            int targetPhraseIndex,
            List<LanguageID> hostLanguageIDs)
        {
            if (targetLanguageID == null)
                return null;

            LanguageItem targetLanguageItem = LanguageItem(targetLanguageID);

            if (targetLanguageItem == null)
                return null;

            List<TextRun> targetPhraseRuns = targetLanguageItem.PhraseRuns;

            if ((targetPhraseRuns == null) || (targetPhraseRuns.Count() == 0))
                return null;

            if ((targetPhraseIndex < 0) || (targetPhraseIndex >= targetPhraseRuns.Count()))
                return null;

            TextRun targetPhraseRun = targetPhraseRuns[targetPhraseIndex];

            if (targetPhraseRun == null)
                return null;

            string targetPhrase = targetLanguageItem.GetRunText(targetPhraseRun);

            if (hostLanguageIDs == null)
                return null;

            MultiLanguageString mls = new MultiLanguageString(targetPhrase);
            HashSet<string> hostWordsUsed = new HashSet<string>();

            foreach (LanguageID hostLanguageID in hostLanguageIDs)
            {
                LanguageItem hostLanguageItem = LanguageItem(hostLanguageID);

                if (hostLanguageItem == null)
                    continue;

                List<TextRun> hostWordRuns = hostLanguageItem.WordRuns;

                if ((hostWordRuns == null) || (hostWordRuns.Count() == 0))
                    continue;

                int hostWordCount = hostWordRuns.Count();
                WordMapping wordMapping = targetPhraseRun.GetWordMapping(hostLanguageID);

                if ((wordMapping == null) || !wordMapping.HasWordIndexes())
                    continue;

                string hostWords = String.Empty;

                hostWordsUsed.Clear();

                foreach (int hostWordIndex in wordMapping.WordIndexes)
                {
                    if ((hostWordIndex >= 0) && (hostWordIndex < hostWordCount))
                    {
                        TextRun hostWordRun = hostWordRuns[hostWordIndex];
                        string hostWord = hostLanguageItem.GetRunText(hostWordRun);

                        if (hostWordsUsed.Add(hostWord.ToLower()))
                            hostWords += (String.IsNullOrEmpty(hostWords) ? "" : " ") + hostWord;
                    }
                }

                if (String.IsNullOrEmpty(hostWords))
                    continue;

                mls.Add(new LanguageString(targetPhrase, hostLanguageID, hostWords));
            }

            if (mls.Count() == 0)
                return null;

            return mls;
        }

        public void ExtractSentenceWordAlignment(MultiLanguageItem sourceMLI, int sourceSentenceIndex, int targetSentenceIndex)
        {
            foreach (LanguageItem sourceLanguageItem in sourceMLI.LanguageItems)
            {
                LanguageID targetLanguageID = sourceLanguageItem.LanguageID;
                LanguageItem targetLanguageItem = LanguageItem(targetLanguageID);

                TextRun sourceSentenceRun = sourceLanguageItem.GetSentenceRun(sourceSentenceIndex);

                if (sourceSentenceRun == null)
                    continue;

                int sourceWordStartIndex = sourceLanguageItem.GetSentenceRunWordStartIndex(sourceSentenceIndex);
                int sourceWordStopIndex = sourceLanguageItem.GetSentenceRunWordStartIndex(sourceSentenceIndex + 1);

                if (targetLanguageItem == null)
                    continue;

                TextRun targetSentenceRun = targetLanguageItem.GetSentenceRun(targetSentenceIndex);

                if (targetSentenceRun == null)
                    continue;

                int targetWordStartIndex = targetLanguageItem.GetSentenceRunWordStartIndex(targetSentenceIndex);
                int targetWordStopIndex = targetLanguageItem.GetSentenceRunWordStartIndex(targetSentenceIndex + 1);
                int sourceWordIndex;
                int targetWordIndex;

                for (sourceWordIndex = sourceWordStartIndex, targetWordIndex = targetWordStartIndex;
                    (sourceWordIndex < sourceWordStopIndex) && (targetWordIndex < targetWordStopIndex);
                    sourceWordIndex++, targetWordIndex++)
                {
                    TextRun sourceWordRun = sourceLanguageItem.GetWordRun(sourceWordIndex);
                    TextRun targetWordRun = targetLanguageItem.GetWordRun(targetWordIndex);

                    foreach (LanguageItem targetHostLanguageItem in _LanguageItems)
                    {
                        LanguageID hostLanguageID = targetHostLanguageItem.LanguageID;

                        if (LanguageLookup.IsSameFamily(hostLanguageID, targetLanguageID))
                            continue;

                        LanguageItem sourceHostLanguageItem = sourceMLI.LanguageItem(hostLanguageID);

                        if (sourceHostLanguageItem == null)
                            continue;

                        int sourceHostWordStartIndex = sourceHostLanguageItem.GetSentenceRunWordStartIndex(sourceSentenceIndex);
                        int targetHostWordStartIndex = targetHostLanguageItem.GetSentenceRunWordStartIndex(targetSentenceIndex);
                        int deltaWordIndex = sourceHostWordStartIndex - targetHostWordStartIndex;

                        WordMapping sourceWordMapping = sourceWordRun.GetWordMapping(hostLanguageID);

                        if (sourceWordMapping == null)
                            continue;

                        int[] targetWordIndexes = sourceWordMapping.CloneWordIndexes();

                        for (int index = 0; index < targetWordIndexes.Length; index++)
                            targetWordIndexes[index] -= deltaWordIndex;

                        WordMapping targetWordMapping = targetWordRun.GetWordMapping(hostLanguageID);

                        if (targetWordMapping == null)
                            targetWordMapping = new WordMapping(hostLanguageID, targetWordIndexes);

                        targetWordRun.SetWordMapping(targetWordMapping);
                    }
                }
            }
        }

        public void ClearWordMappings(List<LanguageID> targetLanguageIDs, List<LanguageID> hostLanguageIDs)
        {
            foreach (LanguageID targetLanguageID in targetLanguageIDs)
            {
                LanguageItem targetLanguageITem = LanguageItem(targetLanguageID);

                if (targetLanguageITem == null)
                    continue;

                targetLanguageITem.ClearWordMappings(hostLanguageIDs);
            }
        }

        public override void Rekey(object newKey)
        {
            base.Rekey(newKey);

            if (_LanguageItems != null)
            {
                foreach (LanguageItem languageItem in _LanguageItems)
                    languageItem.Key = newKey;
            }
        }

        public int Count()
        {
            if (_LanguageItems != null)
                return (_LanguageItems.Count());
            return 0;
        }

        public int Count(List<LanguageID> languageIDs)
        {
            int count = 0;

            if (_LanguageItems != null)
            {
                foreach (LanguageID languageID in languageIDs)
                {
                    if (LanguageItem(languageID) != null)
                        count++;
                }
            }

            return count;
        }

        public List<LanguageItem> FilteredLanguageItems(List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return new List<LanguageItem>();
            List<LanguageItem> languageItems = new List<LanguageItem>(languageIDs.Count());
            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);
                if (languageItem != null)
                    languageItems.Add(new LanguageItem(languageItem));
            }
            return languageItems;
        }

        public List<LanguageItem> FilteredLanguageItems(List<LanguageDescriptor> languageDescriptors)
        {
            if (languageDescriptors == null)
                return new List<LanguageItem>();
            List<LanguageItem> languageItems = new List<LanguageItem>(languageDescriptors.Count());
            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (!languageDescriptor.Used)
                    continue;
                LanguageID languageID = languageDescriptor.LanguageID;
                LanguageItem languageItem = LanguageItem(languageID);
                if (languageItem != null)
                    languageItems.Add(new LanguageItem(languageItem));
            }
            return languageItems;
        }

        public List<Annotation> FilteredAnnotations(List<LanguageID> languageIDs)
        {
            return Annotation.FilteredAnnotations(_Annotations, languageIDs);
        }

        public List<Annotation> FilteredAnnotations(List<LanguageDescriptor> languageDescriptors)
        {
            return Annotation.FilteredAnnotations(_Annotations, languageDescriptors);
        }

        public MultiLanguageItem FilteredMultiLanguageItem(List<LanguageID> languageIDs)
        {
            return new MultiLanguageItem(Key, FilteredLanguageItems(languageIDs), _SpeakerNameKey, FilteredAnnotations(languageIDs),
                CloneExpansionReferences(), _StudyList);
        }

        public MultiLanguageItem FilteredMultiLanguageItem(List<LanguageDescriptor> languageDescriptors)
        {
            return new MultiLanguageItem(Key, FilteredLanguageItems(languageDescriptors), _SpeakerNameKey, FilteredAnnotations(languageDescriptors),
                CloneExpansionReferences(), _StudyList);
        }

        public MultiLanguageItem FilteredSentenceIndexed(int index, List<LanguageID> languageIDs)
        {
            if (_LanguageItems != null)
            {
                object key = Key;
                MultiLanguageItem multiLanguageItem = new MultiLanguageItem();

                multiLanguageItem.Key = key;
                multiLanguageItem.SpeakerNameKey = null;

                foreach (LanguageID languageID in languageIDs)
                {
                    LanguageItem languageItem = LanguageItem(languageID);
                    LanguageItem sentenceItem;

                    if (languageItem == null)
                        sentenceItem = new LanguageItem(key, languageID, String.Empty);
                    else if ((languageItem.SentenceRuns == null) || (index >= languageItem.SentenceRunCount()))
                    {
                        if (index == 0)
                            sentenceItem = new LanguageItem(key, languageID, languageItem.Text);
                        else
                            sentenceItem = new LanguageItem(key, languageID, String.Empty);
                    }
                    else
                    {
                        TextRun run = languageItem.SentenceRuns[index];
                        TextRun newRun = new TextRun(run);
                        newRun.Start = 0;
                        sentenceItem = new LanguageItem(
                            key,
                            languageID,
                            languageItem.Text.Substring(run.Start, run.Length),
                            new List<TextRun>(1) { newRun },
                            null);
                    }

                    multiLanguageItem.Add(sentenceItem);
                }

                multiLanguageItem.Modified = false;

                return multiLanguageItem;
            }
            else
                return FilteredMultiLanguageItem(languageIDs);
        }

        public int GetSentenceCount(List<LanguageID> languageIDs)
        {
            if (_LanguageItems == null)
                return 0;

            int sentenceCount = 0;

            if (languageIDs != null)
            {
                foreach (LanguageID languageID in languageIDs)
                {
                    LanguageItem languageItem = LanguageItem(languageID);

                    if (languageItem == null)
                        continue;

                    if (languageItem.SentenceRuns == null)
                    {
                        if (!String.IsNullOrEmpty(languageItem.Text) && (sentenceCount < 1))
                            sentenceCount = 1;
                    }
                    else if (languageItem.SentenceRunCount() > sentenceCount)
                        sentenceCount = languageItem.SentenceRunCount();
                }
            }
            else
            {
                foreach (LanguageItem languageItem in _LanguageItems)
                {
                    if (languageItem == null)
                        continue;

                    if (languageItem.SentenceRuns == null)
                    {
                        if (!String.IsNullOrEmpty(languageItem.Text) && (sentenceCount < 1))
                            sentenceCount = 1;
                    }
                    else if (languageItem.SentenceRunCount() > sentenceCount)
                        sentenceCount = languageItem.SentenceRunCount();
                }
            }

            return sentenceCount;
        }

        public int GetMaxSentenceCount(List<LanguageID> languageIDs)
        {
            if (_LanguageItems == null)
                return 0;

            int sentenceCount = 0;
            int maxSentenceCount = 0;

            if (languageIDs != null)
            {
                foreach (LanguageID languageID in languageIDs)
                {
                    LanguageItem languageItem = LanguageItem(languageID);

                    if (languageItem == null)
                        continue;

                    if (languageItem.SentenceRuns == null)
                    {
                        if (!String.IsNullOrEmpty(languageItem.Text) && (sentenceCount < 1))
                            sentenceCount = 1;
                    }
                    else if (languageItem.SentenceRunCount() > sentenceCount)
                        sentenceCount = languageItem.SentenceRunCount();

                    if (sentenceCount > maxSentenceCount)
                        maxSentenceCount = sentenceCount;
                }
            }
            else
            {
                foreach (LanguageItem languageItem in _LanguageItems)
                {
                    if (languageItem == null)
                        continue;

                    if (languageItem.SentenceRunCount() > sentenceCount)
                        sentenceCount = languageItem.SentenceRunCount();

                    if (sentenceCount > maxSentenceCount)
                        maxSentenceCount = sentenceCount;
                }
            }

            return maxSentenceCount;
        }

        public int GetSeparateSentenceCount(List<LanguageID> languageIDs)
        {
            if (_LanguageItems == null)
                return 0;

            int totalSentenceCount = 0;

            if (languageIDs != null)
            {
                foreach (LanguageID languageID in languageIDs)
                {
                    LanguageItem languageItem = LanguageItem(languageID);
                    int sentenceCount = 0;

                    if (languageItem == null)
                        continue;

                    if (languageItem.SentenceRuns == null)
                    {
                        if (!String.IsNullOrEmpty(languageItem.Text))
                            sentenceCount = 1;
                    }
                    else
                        sentenceCount = languageItem.SentenceRunCount();

                    totalSentenceCount += sentenceCount;
                }
            }
            else
            {
                foreach (LanguageItem languageItem in _LanguageItems)
                {
                    if (languageItem == null)
                        continue;

                    totalSentenceCount += languageItem.SentenceRunCount();
                }
            }

            return totalSentenceCount;
        }

        public MultiLanguageItem GetSentenceIndexed(int index, List<LanguageID> languageIDs)
        {
            if (_LanguageItems != null)
            {
                object key = Key;
                MultiLanguageItem multiLanguageItem = new MultiLanguageItem();

                multiLanguageItem.Key = key;

                if (languageIDs != null)
                {
                    foreach (LanguageID languageID in languageIDs)
                    {
                        LanguageItem languageItem = LanguageItem(languageID);
                        LanguageItem sentenceItem;

                        if ((languageItem == null) || String.IsNullOrEmpty(languageItem.Text))
                            sentenceItem = new LanguageItem(key, languageID, String.Empty);
                        else if ((languageItem.SentenceRuns == null) || (index >= languageItem.SentenceRunCount()))
                        {
                            if (index == 0)
                                sentenceItem = new LanguageItem(key, languageID, languageItem.Text);
                            else
                                sentenceItem = new LanguageItem(key, languageID, String.Empty);
                        }
                        else
                        {
                            TextRun run = languageItem.SentenceRuns[index];
                            if ((run.Start <= languageItem.Text.Length) && (run.Start + run.Length <= languageItem.Text.Length))
                            {
                                sentenceItem = new LanguageItem(
                                    key,
                                    languageID,
                                    languageItem.Text.Substring(run.Start, run.Length),
                                    new List<TextRun>(1) { run },
                                    null);
                            }
                            else if (run.Start < languageItem.Text.Length)
                            {
                                run.Length = languageItem.Text.Length - run.Start;
                                sentenceItem = new LanguageItem(
                                    key,
                                    languageID,
                                    languageItem.Text.Substring(run.Start, run.Length),
                                    new List<TextRun>(1) { run },
                                    null);
                            }
                            else
                            {
                                run.Start = languageItem.Text.Length;
                                run.Length = 0;
                                sentenceItem = new LanguageItem(key, languageID, String.Empty);
                            }
                        }

                        multiLanguageItem.Add(sentenceItem);
                    }
                }
                else
                {
                    foreach (LanguageItem languageItem in _LanguageItems)
                    {
                        if (languageItem == null)
                            continue;

                        LanguageItem sentenceItem;
                        LanguageID languageID = languageItem.LanguageID;

                        if ((languageItem.SentenceRuns == null) || (index >= languageItem.SentenceRunCount()))
                        {
                            if (index == 0)
                                sentenceItem = new LanguageItem(key, languageID, languageItem.Text);
                            else
                                sentenceItem = new LanguageItem(key, languageID, String.Empty);
                        }
                        else
                        {
                            TextRun run = languageItem.SentenceRuns[index];
                            if ((run.Start <= languageItem.Text.Length) && (run.Start + run.Length <= languageItem.Text.Length))
                            {
                                sentenceItem = new LanguageItem(
                                    key,
                                    languageID,
                                    languageItem.Text.Substring(run.Start, run.Length),
                                    new List<TextRun>(1) { run },
                                    null);
                            }
                            else if (run.Start < languageItem.Text.Length)
                            {
                                run.Length = languageItem.Text.Length - run.Start;
                                sentenceItem = new LanguageItem(
                                    key,
                                    languageID,
                                    languageItem.Text.Substring(run.Start, run.Length),
                                    new List<TextRun>(1) { run },
                                    null);
                            }
                            else
                            {
                                run.Start = languageItem.Text.Length;
                                run.Length = 0;
                                sentenceItem = new LanguageItem(key, languageID, String.Empty);
                            }
                        }

                        multiLanguageItem.Add(sentenceItem);
                    }
                }

                multiLanguageItem.Modified = false;

                return multiLanguageItem;
            }
            else
                return FilteredMultiLanguageItem(languageIDs);
        }

        public bool InsertEmptySentenceRuns(int sentenceIndex)
        {
            if (_LanguageItems == null)
                return false;

            bool returnValue = true;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (!languageItem.InsertEmptySentenceRun(sentenceIndex))
                    returnValue = false;
            }

            return returnValue;
        }

        public bool DeleteSentenceRunsIndexed(int index)
        {
            if (_LanguageItems == null)
                return false;

            bool returnValue = true;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (!languageItem.DeleteSentenceRunIndexed(index))
                    returnValue = false;
            }

            return returnValue;
        }

        public bool SentenceCheck(List<LanguageID> languageIDs)
        {
            if (_LanguageItems == null)
                return true;

            int sentenceCount = 0;
            int newCount;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                if (languageItem.SentenceRuns == null)
                {
                    if (!String.IsNullOrEmpty(languageItem.Text) && (sentenceCount < 1))
                        sentenceCount = 1;
                }
                else if ((newCount = languageItem.SentenceRunCount()) != sentenceCount)
                {
                    if (sentenceCount == 0)
                        sentenceCount = newCount;
                    else
                        return false;
                }
            }

            return true;
        }

        public bool HaveSentenceMismatch()
        {
            if (_LanguageItems == null)
                return false;

            int sentenceCount = 0;
            int newCount;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (languageItem == null)
                    continue;

                if (languageItem.SentenceRuns == null)
                {
                    if (!String.IsNullOrEmpty(languageItem.Text) && (sentenceCount < 1))
                        sentenceCount = 1;
                }
                else if ((newCount = languageItem.SentenceRunCount()) != sentenceCount)
                {
                    if (newCount != 0)
                    {
                        if (sentenceCount == 0)
                            sentenceCount = newCount;
                        else
                            return true;
                    }
                }
            }

            return false;
        }

        public bool HaveSentenceMismatch(List<LanguageID> languageIDs)
        {
            if (_LanguageItems == null)
                return false;

            int sentenceCount = 0;
            int newCount;

            if ((languageIDs == null) || (languageIDs.Count() == 0))
                return false;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                if (languageItem.SentenceRuns == null)
                {
                    if (!String.IsNullOrEmpty(languageItem.Text) && (sentenceCount < 1))
                        sentenceCount = 1;
                }
                else if ((newCount = languageItem.SentenceRunCount()) != sentenceCount)
                {
                    if (newCount != 0)
                    {
                        if (sentenceCount == 0)
                            sentenceCount = newCount;
                        else
                            return true;
                    }
                }
            }

            return false;
        }

        public bool MapText(BaseObjectContent contentMediaItem,
            LanguageMediaItem languageMediaItem, LanguageID mediaLanguageID,
            int sentenceIndex, TimeSpan mediaStartTime, TimeSpan mediaStopTime)
        {
            bool returnValue = true;

            if (_LanguageItems == null)
                return false;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (languageItem.LanguageID.MediaLanguageCompare(mediaLanguageID) != 0)
                    continue;

                if (!languageItem.MapText(contentMediaItem, languageMediaItem, sentenceIndex, mediaStartTime, mediaStopTime))
                    returnValue = false;
            }

            return returnValue;
        }

        public bool MapText(BaseObjectContent contentMediaItem,
            LanguageMediaItem languageMediaItem, List<LanguageID> mediaLanguageIDs,
            int sentenceIndex, TimeSpan mediaStartTime, TimeSpan mediaStopTime)
        {
            bool returnValue = true;

            if (mediaLanguageIDs != null)
            {
                foreach (LanguageID mediaLanguageID in mediaLanguageIDs)
                {
                    if (!MapText(contentMediaItem, languageMediaItem, mediaLanguageID,
                            sentenceIndex, mediaStartTime, mediaStopTime))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItem,
            LanguageID mediaLanguageID, int sentenceIndex)
        {
            if (_LanguageItems == null)
                return;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (languageItem.LanguageID.MediaLanguageCompare(mediaLanguageID) != 0)
                    continue;

                languageItem.MapTextClear(mediaItemKey, languageMediaItem, sentenceIndex);
            }
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItem,
            List<LanguageID> mediaLanguageIDs, int sentenceIndex)
        {
            if (mediaLanguageIDs != null)
            {
                foreach (LanguageID mediaLanguageID in mediaLanguageIDs)
                    MapTextClear(mediaItemKey, languageMediaItem, mediaLanguageID, sentenceIndex);
            }
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItem,
            LanguageID mediaLanguageID, TimeSpan mediaStartTime, TimeSpan mediaStopTime)
        {
            if (_LanguageItems == null)
                return;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (languageItem.LanguageID.MediaLanguageCompare(mediaLanguageID) != 0)
                    continue;

                languageItem.MapTextClear(mediaItemKey, languageMediaItem, mediaStartTime, mediaStopTime);
            }
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItem,
            List<LanguageID> mediaLanguageIDs, TimeSpan mediaStartTime, TimeSpan mediaStopTime)
        {
            if (mediaLanguageIDs != null)
            {
                foreach (LanguageID mediaLanguageID in mediaLanguageIDs)
                    MapTextClear(mediaItemKey, languageMediaItem, mediaLanguageID, mediaStartTime, mediaStopTime);
            }
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItem,
            LanguageID mediaLanguageID)
        {
            if (_LanguageItems == null)
                return;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (languageItem.LanguageID.MediaLanguageCompare(mediaLanguageID) != 0)
                    continue;

                languageItem.MapTextClear(mediaItemKey, languageMediaItem);
            }
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItem,
            List<LanguageID> mediaLanguageIDs)
        {
            if (mediaLanguageIDs != null)
            {
                foreach (LanguageID mediaLanguageID in mediaLanguageIDs)
                    MapTextClear(mediaItemKey, languageMediaItem, mediaLanguageID);
            }
        }

        public void CollapseSentenceRuns()
        {
            if (_LanguageItems == null)
                return;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (languageItem == null)
                    continue;

                languageItem.CollapseSentenceRuns();
            }
        }

        public void FixSentenceMismatch()
        {
            if (_LanguageItems == null)
                return;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (languageItem == null)
                    continue;

                languageItem.CollapseSentenceRuns();
            }
        }

        public void JoinPhrases(LanguageID languageID)
        {
        }

        public void JoinSentenceRuns(List<LanguageID> languageIDs)
        {
            if (_LanguageItems == null)
                return;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                languageItem.JoinSentenceRuns();
            }
        }

        public void JoinSentenceRuns(List<LanguageID> languageIDs, int sentenceIndex, int sentenceCount)
        {
            if (_LanguageItems == null)
                return;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                languageItem.JoinSentenceRuns(sentenceIndex, sentenceCount);
            }
        }

        public void ComputeRuns(DictionaryRepository dictionary)
        {
            ComputeSentenceRuns();
            ComputeWordRuns(dictionary);
        }

        public void SplitSentenceRuns(List<LanguageID> languageIDs)
        {
            if (_LanguageItems == null)
                return;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                languageItem.SplitSentenceRuns();
            }
        }

        public void SplitSentenceRuns(List<LanguageID> languageIDs, int sentenceIndex)
        {
            if (_LanguageItems == null)
                return;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                languageItem.SplitSentenceRun(sentenceIndex);
            }
        }

        public void ComputeSentenceRuns()
        {
            if (LanguageItems == null)
                return;

            foreach (LanguageItem languageItem in LanguageItems)
            {
                //languageItem.SentenceRuns = null;
                //languageItem.LoadSentenceRunsFromText();
                languageItem.SplitSentenceRuns();
            }
        }

        public void AutoResetSentenceRuns(List<LanguageID> languageIDs)
        {
            if (LanguageItems == null)
                return;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                languageItem.AutoResetSentenceRuns();
            }
        }

        public void JoinWordRuns(List<LanguageID> languageIDs)
        {
            if (_LanguageItems == null)
                return;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                languageItem.JoinWordRuns();
            }
        }

        public void JoinWordRuns(List<LanguageID> languageIDs, int wordIndex, int wordCount)
        {
            if (_LanguageItems == null)
                return;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                languageItem.JoinWordRuns(wordIndex, wordCount);
            }
        }

        public void ComputeWordRuns(DictionaryRepository dictionary)
        {
            if (LanguageItems == null)
                return;

            foreach (LanguageItem languageItem in LanguageItems)
            {
                languageItem.WordRuns = null;
                languageItem.LoadWordRunsFromText(dictionary);
            }
        }

        public void AutoResetWordRuns(List<LanguageID> languageIDs, DictionaryRepository dictionary)
        {
            if (LanguageItems == null)
                return;

            List<LanguageID> doneLanguageIDs = new List<LanguageID>();

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem;

                if (doneLanguageIDs.Contains(languageID))
                    continue;

                if (LanguageLookup.IsAlternatePhonetic(languageID))
                {
                    LanguageID baseLanguageID = LanguageLookup.GetRootLanguageID(languageID);

                    if (!HasLanguageID(baseLanguageID))
                    {
                        languageItem = LanguageItem(languageID);

                        if (languageItem == null)
                            continue;

                        languageItem.AutoResetWordRuns(dictionary);
                        doneLanguageIDs.Add(languageID);
                        continue;
                    }
                }

                if (LanguageLookup.HasAlternatePhonetic(languageID))
                {
                    LanguageID baseLanguageID = LanguageLookup.GetRootLanguageID(languageID);
                    LanguageTool tool = ApplicationData.LanguageTools.Create(baseLanguageID);

                    if (tool != null)
                    {
                        tool.ResetMultiLanguageItemWordRuns(this);
                        List<LanguageID> familyLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(baseLanguageID);
                        doneLanguageIDs.AddRange(familyLanguageIDs);
                        continue;
                    }
                }

                languageItem = LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                languageItem.AutoResetWordRuns(dictionary);

                doneLanguageIDs.Add(languageID);
            }
        }

        public void UpdateLanguageItemWordRuns(
            LanguageItem languageItem,
            LanguageTool tool)
        {
            LanguageID languageID = languageItem.LanguageID;
            LanguageID baseLanguageID = LanguageLookup.GetRootLanguageID(languageID);
            LanguageItem baseLanguageItem = LanguageItem(baseLanguageID);

            if (LanguageLookup.IsAlternatePhonetic(languageID))
            {
                if ((baseLanguageItem == null) || !baseLanguageItem.HasText())
                {
                    if (tool != null)
                        tool.ResetLanguageItemWordRuns(languageItem);
                    else
                        languageItem.AutoResetWordRuns(tool.DictionaryDatabase);
                    return;
                }

                if (tool != null)
                {
                    List<TextGraphNode> path = tool.GetPathFromWordRuns(baseLanguageItem.Text, baseLanguageItem.WordRuns);
                    tool.GetLanguageItemWordRunsSynchronizedFromPath(languageItem, baseLanguageID, path);
                    return;
                }
            }
            else if (tool != null)
                tool.ResetLanguageItemWordRuns(languageItem);
            else
                languageItem.AutoResetWordRuns(tool.DictionaryDatabase);
        }

        public bool IsWordMismatch(LanguageID languageID)
        {
            if (!LanguageLookup.HasAnyAlternates(languageID))
                return false;

            List<LanguageID> sameFamilyLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(languageID);
            int languageCount = sameFamilyLanguageIDs.Count();

            LanguageItem languageItem = LanguageItem(sameFamilyLanguageIDs[0]);

            if (languageItem == null)
                return true;

            int wordCount = languageItem.WordRunCount();

            for (int languageIndex = 1; languageIndex < languageCount; languageIndex++)
            {
                LanguageID lid = sameFamilyLanguageIDs[languageIndex];
                languageItem = LanguageItem(lid);

                if (languageItem == null)
                    continue;
                else if (!languageItem.HasText())
                    continue;
                else if (wordCount != languageItem.WordRunCount())
                    return true;
            }

            return false;
        }

        public bool IsSentenceMismatch(List<LanguageID> languageIDs)
        {
            int languageCount = (languageIDs != null ? languageIDs.Count() : 0);

            if (languageCount == 0)
                return false;

            LanguageItem languageItem;
            int sentenceCount = GetMaxSentenceCount(languageIDs);

            for (int languageIndex = 0; languageIndex < languageCount; languageIndex++)
            {
                LanguageID lid = languageIDs[languageIndex];
                languageItem = LanguageItem(lid);

                int languageSentenceCount = (languageItem != null ? languageItem.SentenceRunCount() : 0);

                if (languageSentenceCount != sentenceCount)
                    return true;
            }

            return false;
        }

        public bool IsSentenceMismatch(List<LanguageID> languageIDs, LanguageID languageID)
        {
            int languageCount = (languageIDs != null ? languageIDs.Count() : 0);

            if (languageCount == 0)
                return false;

            LanguageItem languageItem;
            int sentenceCount = GetMaxSentenceCount(languageIDs);

            for (int languageIndex = 0; languageIndex < languageCount; languageIndex++)
            {
                LanguageID lid = languageIDs[languageIndex];
                languageItem = LanguageItem(lid);

                int languageSentenceCount = (languageItem != null ? languageItem.SentenceRunCount() : 0);

                if (languageSentenceCount != sentenceCount)
                {
                    if (lid == languageID)
                        return true;
                }
            }

            return false;
        }

        public bool IsWordMismatch(int sentenceIndex, LanguageID languageID)
        {
            if (!LanguageLookup.HasAnyAlternates(languageID))
                return false;

            List<LanguageID> sameFamilyLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(languageID);
            int languageCount = sameFamilyLanguageIDs.Count();

            LanguageItem languageItem = LanguageItem(sameFamilyLanguageIDs[0]);

            if (languageItem == null)
                return true;

            int wordCount = languageItem.SentenceWordRunCount(sentenceIndex);

            for (int languageIndex = 1; languageIndex < languageCount; languageIndex++)
            {
                LanguageID lid = sameFamilyLanguageIDs[languageIndex];
                languageItem = LanguageItem(lid);

                if ((languageItem == null) || (wordCount != languageItem.SentenceWordRunCount(sentenceIndex)))
                    return true;
            }

            return false;
        }

        public void GetSentenceAndWordRuns(DictionaryRepository dictionary)
        {
            if (LanguageItems == null)
                return;

            foreach (LanguageItem languageItem in LanguageItems)
                languageItem.GetSentenceAndWordRuns(dictionary);
        }

        public void SentenceAndWordRunCheck(DictionaryRepository dictionary)
        {
            if (LanguageItems == null)
                return;

            SentenceRunCheck();
            WordRunCheck(dictionary);
        }

        public void LoadSentenceAndWordRunsFromText(DictionaryRepository dictionary)
        {
            if (LanguageItems == null)
                return;

            foreach (LanguageItem languageItem in LanguageItems)
                languageItem.LoadSentenceAndWordRunsFromText(dictionary);
        }

        public bool SentenceRunCheck()
        {
            bool returnValue = true;

            if (LanguageItems == null)
                return false;

            int runCount = -1;

            foreach (LanguageItem languageItem in LanguageItems)
            {
                if ((languageItem.SentenceRuns == null) ||
                        ((languageItem.SentenceRunCount() == 0) && languageItem.HasText()))
                    languageItem.LoadSentenceRunsFromText();

                int currentRunCount = languageItem.SentenceRunCount();

                if (runCount == -1)
                    runCount = currentRunCount;
                else if (currentRunCount != runCount)
                    returnValue = false;
            }

            return returnValue;
        }

        public MultiLanguageItem GetVocabularySentence(string vocabularyText, LanguageID languageID)
        {
            MultiLanguageItem returnValue = null;

            if (!SentenceRunCheck())
                return returnValue;

            LanguageItem languageItem = LanguageItem(languageID);

            if (languageItem == null)
                return returnValue;

            if (String.IsNullOrEmpty(languageItem.Text))
                return returnValue;

            if (!languageItem.Text.Contains(vocabularyText))
                return returnValue;

            TextRun bestRun = null;
            int bestIndex = -1;
            int runIndex = 0;

            foreach (TextRun sentenceRun in languageItem.SentenceRuns)
            {
                if (languageItem.GetRunText(sentenceRun).Contains(vocabularyText))
                {
                    if (bestRun == null)
                    {
                        bestRun = sentenceRun;
                        bestIndex = runIndex;
                    }
                    else if (sentenceRun.Length < bestRun.Length)
                    {
                        bestRun = sentenceRun;
                        bestIndex = runIndex;
                    }
                }

                runIndex++;
            }

            List<LanguageItem> languageItems = new List<LanguageItem>(LanguageItems.Count());

            foreach (LanguageItem aLanguageItem in LanguageItems)
            {
                TextRun run = aLanguageItem.GetSentenceRun(bestIndex);
                LanguageItem vocabLanguageItem = new LanguageItem(vocabularyText, aLanguageItem.LanguageID, aLanguageItem.GetRunText(run));
                languageItems.Add(vocabLanguageItem);
            }

            returnValue = new MultiLanguageItem(vocabularyText, languageItems);

            return returnValue;
        }

        public void WordRunCheck(DictionaryRepository dictionary)
        {
            if (LanguageItems == null)
                return;

            foreach (LanguageItem languageItem in LanguageItems)
            {
                if (languageItem.WordRuns == null)
                    languageItem.LoadWordRunsFromText(dictionary);
            }
        }

        public void WordRunCheckLanguages(List<LanguageID> languageIDs, DictionaryRepository dictionary)
        {
            if (LanguageItems == null)
                return;

            foreach (LanguageItem languageItem in LanguageItems)
            {
                if (languageIDs.Contains(languageItem.LanguageID))
                {
                    if (languageItem.WordRuns == null)
                        languageItem.LoadWordRunsFromText(dictionary);
                }
            }
        }

        public int GetWordCount(List<LanguageID> languageIDs)
        {
            if (_LanguageItems == null)
                return 0;

            int wordCount = 0;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                if (languageItem.WordRuns == null)
                {
                    if (!String.IsNullOrEmpty(languageItem.Text) && (wordCount < 1))
                        wordCount = 1;
                }
                else if (languageItem.WordRuns.Count() > wordCount)
                    wordCount = languageItem.WordRuns.Count();
            }

            return wordCount;
        }

        public int GetMaxWordCount(List<LanguageID> languageIDs)
        {
            if (_LanguageItems == null)
                return 0;

            int wordCount = 0;
            int maxWordCount = 0;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                if (languageItem.WordRuns == null)
                {
                    if (!String.IsNullOrEmpty(languageItem.Text) && (wordCount < 1))
                        wordCount = 1;
                }
                else if (languageItem.WordRuns.Count() > wordCount)
                    wordCount = languageItem.WordRuns.Count();

                if (wordCount > maxWordCount)
                    maxWordCount = wordCount;
            }

            return maxWordCount;
        }

        public MultiLanguageItem GetSentenceWord(int wordIndex, List<LanguageID> languageIDs)
        {
            List<LanguageItem> wordLanguageItems = new List<LanguageItem>();

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);
                TextRun wordRun = languageItem.GetWordRun(wordIndex);
                string wordText = languageItem.GetRunText(wordRun);
                LanguageItem wordLanguageItem = new LanguageItem(wordIndex.ToString(), languageID, wordText);
                wordLanguageItems.Add(wordLanguageItem);
            }

            MultiLanguageItem wordMultiLanguageItem = new MultiLanguageItem(wordIndex.ToString(), wordLanguageItems);

            return wordMultiLanguageItem;
        }

        public MultiLanguageItem GetPunctuation(int wordIndex, List<LanguageID> languageIDs)
        {
            List<LanguageItem> punctuationLanguageItems = null;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);
                TextRun punctuationRun = languageItem.GetWordRun(wordIndex);
                TextRun punctuationAfterRun = languageItem.GetWordRun(wordIndex + 1);
                int startIndex;
                int stopIndex;

                if (punctuationRun != null)
                    startIndex = punctuationRun.Stop;
                else
                    startIndex = languageItem.TextLength;

                if (punctuationAfterRun != null)
                    stopIndex = punctuationAfterRun.Start;
                else
                    stopIndex = languageItem.TextLength;

                int length = stopIndex - startIndex;

                if (length > 0)
                {
                    string punctuationText = languageItem.GetSubText(startIndex, length).Trim();

                    if (!String.IsNullOrEmpty(punctuationText))
                    {
                        LanguageItem punctuationLanguageItem = new LanguageItem(wordIndex.ToString() + "p", languageID, punctuationText);

                        if (punctuationLanguageItems == null)
                            punctuationLanguageItems = new List<LanguageItem>();

                        punctuationLanguageItems.Add(punctuationLanguageItem);
                    }
                }
            }

            MultiLanguageItem punctuationMultiLanguageItem = null;

            if (punctuationLanguageItems != null)
                punctuationMultiLanguageItem = new MultiLanguageItem(wordIndex.ToString() + "p", punctuationLanguageItems);

            return punctuationMultiLanguageItem;
        }

        public List<MultiLanguageItem> GetSentenceWords(List<LanguageID> languageIDs, bool includePunctuation)
        {
            List<MultiLanguageItem> wordItems = new List<MultiLanguageItem>();
            LanguageID primaryLanguageID = languageIDs.First();

            if (primaryLanguageID == null)
                return wordItems;

            LanguageItem primaryLanguageItem = LanguageItem(primaryLanguageID);

            if (primaryLanguageItem == null)
                return wordItems;

            int wordCount = primaryLanguageItem.WordRunCount();
            int wordIndex;

            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                MultiLanguageItem wordItem = GetSentenceWord(wordIndex, languageIDs);
                wordItems.Add(wordItem);

                if (includePunctuation)
                {
                    wordItem = GetPunctuation(wordIndex, languageIDs);

                    if (wordItem != null)
                        wordItems.Add(wordItem);
                }
            }

            return wordItems;
        }

        public void RunCheck(DictionaryRepository dictionary)
        {
            SentenceRunCheck();
            WordRunCheck(dictionary);
        }

        public bool GetMediaInfo(string mediaRunKey, LanguageID languageID, string mediaPathUrl, BaseObjectNode node, out bool hasAudio, out bool hasVideo, out bool hasSlow, out bool hasPicture,
            List<string> audioVideoUrls)
        {
            bool hasAudioTemp;
            bool hasVideoTemp;
            bool hasSlowTemp;
            bool hasPictureTemp;
            bool returnValue = false;

            hasAudio = false;
            hasVideo = false;
            hasSlow = false;
            hasPicture = false;

            if (_LanguageItems == null)
                return false;

            LanguageItem languageItem = LanguageItem(languageID);

            if (languageItem == null)
                languageItem = LanguageItemFuzzy(languageID);

            if (languageItem != null)
            {
                if (languageItem.HasSentenceRuns())
                {
                    foreach (TextRun sentenceRun in languageItem.SentenceRuns)
                    {
                        if (sentenceRun.MediaRuns != null)
                        {
                            foreach (MediaRun mediaRun in sentenceRun.MediaRuns)
                            {
                                bool returnTemp = mediaRun.GetMediaInfo(mediaRunKey, mediaPathUrl, node,
                                    out hasAudioTemp, out hasVideoTemp, out hasSlowTemp, out hasPictureTemp, audioVideoUrls);

                                if (hasAudioTemp)
                                    hasAudio = true;

                                if (hasVideoTemp)
                                    hasVideo = true;

                                if (hasSlowTemp)
                                    hasSlow = true;

                                if (hasPictureTemp)
                                    hasPicture = true;

                                if (!returnTemp)
                                    continue;

                                returnValue = true;
                            }
                        }
                    }
                }
            }

            return returnValue;
        }

        public bool HasMediaRun(LanguageID languageID, string fileName)
        {
            if (!String.IsNullOrEmpty(fileName))
            {
                LanguageItem languageItem = LanguageItem(languageID);

                if (languageItem == null)
                    languageItem = LanguageItemFuzzy(languageID);

                if (languageItem != null)
                {
                    if (languageItem.HasSentenceRuns())
                    {
                        foreach (TextRun sentenceRun in languageItem.SentenceRuns)
                        {
                            if (sentenceRun.MediaRuns != null)
                            {
                                foreach (MediaRun mediaRun in sentenceRun.MediaRuns)
                                {
                                    if (mediaRun.FileName == fileName)
                                        return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public bool HasMediaUrlRun(LanguageID languageID)
        {
            LanguageItem languageItem = LanguageItem(languageID);

            if (languageItem == null)
                languageItem = LanguageItemFuzzy(languageID);

            if (languageItem != null)
            {
                if (languageItem.HasSentenceRuns())
                {
                    foreach (TextRun sentenceRun in languageItem.SentenceRuns)
                    {
                        if (sentenceRun.MediaRuns != null)
                        {
                            foreach (MediaRun mediaRun in sentenceRun.MediaRuns)
                            {
                                if (!mediaRun.IsSegment())
                                    return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public bool HasMediaRunWithKey(object key, LanguageID languageID)
        {
            if ((languageID == null) || String.IsNullOrEmpty(languageID.LanguageCode) || languageID.LanguageCode.StartsWith("("))
            {
                if (LanguageItems != null)
                {
                    foreach (LanguageItem languageItem in LanguageItems)
                    {
                        if (languageItem.HasMediaRunWithKey(key))
                            return true;
                    }
                }
            }
            else
            {
                LanguageItem languageItem = LanguageItemMedia(languageID);

                if (languageItem != null)
                {
                    if (languageItem.HasMediaRunWithKey(key))
                        return true;
                }
            }

            return false;
        }

        public bool HasMediaRunWithUrl(string url, LanguageID languageID)
        {
            if ((languageID == null) || String.IsNullOrEmpty(languageID.LanguageCode) || languageID.LanguageCode.StartsWith("("))
            {
                if (LanguageItems != null)
                {
                    foreach (LanguageItem languageItem in LanguageItems)
                    {
                        if (languageItem.HasMediaRunWithUrl(url))
                            return true;
                    }
                }
            }
            else
            {
                LanguageItem languageItem = LanguageItemMedia(languageID);

                if (languageItem != null)
                {
                    if (languageItem.HasMediaRunWithUrl(url))
                        return true;
                }
            }

            return false;
        }

        public bool HasAnyMediaRun()
        {
            if (_LanguageItems == null)
                return false;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (languageItem.HasSentenceRuns())
                {
                    foreach (TextRun sentenceRun in languageItem.SentenceRuns)
                    {
                        if (sentenceRun.MediaRuns != null)
                        {
                            if (sentenceRun.MediaRunCount() != 0)
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        public MediaRun GetMediaRunWithKey(LanguageID languageID, int sentenceIndex, object key)
        {
            LanguageItem languageItem = LanguageItem(languageID);

            if (languageItem != null)
                return languageItem.GetMediaRunWithKey(sentenceIndex, key);

            return null;
        }

        public MediaRun GetMediaRun(LanguageID languageID, int sentenceIndex, object key)
        {
            LanguageItem languageItem = LanguageItem(languageID);

            if (languageItem != null)
                return languageItem.GetMediaRun(sentenceIndex, key);

            return null;
        }

        public MediaRun GetMediaRunWithUrl(LanguageID languageID, int sentenceIndex, string fileName)
        {
            LanguageItem languageItem = LanguageItem(languageID);

            if (languageItem != null)
                return languageItem.GetMediaRunWithUrl(sentenceIndex, fileName);

            return null;
        }

        public MediaRun GetMediaRunWithUrl(string fileName)
        {
            if (_LanguageItems == null)
                return null;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                MediaRun mediaRun = languageItem.GetMediaRunWithUrl(fileName);

                if (mediaRun != null)
                    return mediaRun;
            }

            return null;
        }

        public MediaRun GetMediaRunWithReferenceKeys(LanguageID languageID, int sentenceIndex, string mediaItemKey, string languageMediaItemKey)
        {
            LanguageItem languageItem = LanguageItem(languageID);

            if (languageItem != null)
                return languageItem.GetMediaRunWithReferenceKeys(sentenceIndex, mediaItemKey, languageMediaItemKey);

            return null;
        }

        public MediaRun GetMediaRunWithReferenceKeys(
            List<LanguageID> languageIDs,
            int sentenceIndex,
            string mediaItemKey,
            string languageMediaItemKey,
            out LanguageID foundLanguageID)
        {
            foundLanguageID = null;

            if ((languageIDs == null) || (_LanguageItems == null))
                return null;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                MediaRun mediaRun = languageItem.GetMediaRunWithReferenceKeys(sentenceIndex, mediaItemKey, languageMediaItemKey);

                if (mediaRun != null)
                {
                    foundLanguageID = languageID;
                    return mediaRun;
                }
            }

            return null;
        }

        public MediaRun GetMediaRunWithReferenceKeys(int sentenceIndex, string mediaItemKey, string languageMediaItemKey)
        {
            if (_LanguageItems == null)
                return null;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                MediaRun mediaRun = languageItem.GetMediaRunWithReferenceKeys(sentenceIndex, mediaItemKey, languageMediaItemKey);

                if (mediaRun != null)
                    return mediaRun;
            }

            return null;
        }

        public MediaRun GetMediaRunWithReferenceKeys(int sentenceIndex, string mediaItemKey, string languageMediaItemKey, out LanguageID languageID)
        {
            languageID = null;

            if (_LanguageItems == null)
                return null;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                MediaRun mediaRun = languageItem.GetMediaRunWithReferenceKeys(sentenceIndex, mediaItemKey, languageMediaItemKey);

                if (mediaRun != null)
                {
                    languageID = languageItem.LanguageID;
                    return mediaRun;
                }
            }

            return null;
        }

        public void GetMediaRunsWithReferenceKeys(List<MediaRun> mediaRuns,
            LanguageID mediaLanguageID, string mediaItemKey, string languageMediaItemKey)
        {
            if (_LanguageItems == null)
                return;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (!languageItem.IsMediaLanguageMatch(mediaLanguageID))
                    continue;

                languageItem.GetMediaRunsWithReferenceKeys(mediaRuns, mediaItemKey, languageMediaItemKey);
            }
        }

        public int GetMediaRunCount(LanguageID mediaLanguageID, int sentenceIndex, string mediaRunKey)
        {
            int count = 0;

            foreach (LanguageItem languageItem in _LanguageItems)
            {
                if (!languageItem.IsMediaLanguageMatch(mediaLanguageID))
                    continue;

                int newCount = languageItem.GetMediaRunCount(sentenceIndex, mediaRunKey);

                if (newCount > count)
                    count = newCount;

            }

            return count;
        }

        public MediaRun GetMergedReferenceMediaRun(LanguageID languageID, object key,
            string mediaItemKey, string languageMediaItemKey)
        {
            LanguageItem languageItem = LanguageItem(languageID);

            if (languageItem != null)
                return languageItem.GetMergedReferenceMediaRun(key, mediaItemKey, languageMediaItemKey);

            return null;
        }

        public void DeleteMediaRunsWithReferenceKey(string mediaItemKey, string languageMediaItemKey)
        {
            if (_LanguageItems == null)
                return;

            foreach (LanguageItem languageItem in _LanguageItems)
                languageItem.DeleteMediaRunsWithReferenceKeys(mediaItemKey, languageMediaItemKey);
        }

        // Re-map reference media run times to target time.
        // Target time advanced to end of mapped time period.
        public void RemapReferenceMediaRuns(
            LanguageID languageID,
            string mediaRunKey,
            string mediaItemKey,
            string languageMediaItemKey,
            TimeSpan timeToSubtract)
        {
            LanguageItem languageItem = LanguageItem(languageID);

            if (languageItem != null)
                languageItem.RemapReferenceMediaRuns(mediaRunKey, mediaItemKey, languageMediaItemKey, timeToSubtract);
        }

        // Note: Media references will be collapsed to a "(url)#t(start),(end)" url.
        // Returns true if any media found.
        public bool CollectMediaUrls(string mediaRunKey, string mediaPathUrl, BaseObjectNode node, object content,
            List<string> mediaUrls, VisitMedia visitFunction, LanguageID mediaLanguageID)
        {
            bool returnValue = false;

            if (_LanguageItems != null)
            {
                if ((mediaLanguageID == null) || String.IsNullOrEmpty(mediaLanguageID.LanguageCode) || mediaLanguageID.LanguageCode.StartsWith("("))
                {
                    foreach (LanguageItem languageItem in _LanguageItems)
                    {
                        if (languageItem.CollectMediaUrls(mediaRunKey, mediaPathUrl, node, content, mediaUrls, visitFunction))
                            returnValue = true;
                    }
                }
                else
                {
                    foreach (LanguageItem languageItem in _LanguageItems)
                    {
                        if (languageItem.LanguageID.MediaLanguageCompare(mediaLanguageID) == 0)
                        {
                            if (languageItem.CollectMediaUrls(mediaRunKey, mediaPathUrl, node, content, mediaUrls, visitFunction))
                                returnValue = true;
                        }
                    }
                }
            }

            return returnValue;
        }

        public void CollectMediaFiles(string directoryUrl, Dictionary<string, bool> languageSelectFlags,
            List<string> mediaFiles, object content, VisitMedia visitFunction)
        {
            if (_LanguageItems != null)
            {
                foreach (LanguageItem languageItem in _LanguageItems)
                {
                    bool collectIt = true;
                    if ((languageSelectFlags != null) &&
                            !languageSelectFlags.TryGetValue(
                                languageItem.LanguageID.LanguageCultureExtensionCode,
                                out collectIt))
                        collectIt = false;
                    if (collectIt)
                        languageItem.CollectMediaFiles(directoryUrl, mediaFiles, content, visitFunction);
                }
            }
        }

        public void CollectMediaReferences(Dictionary<string, bool> languageSelectFlags,
            List<MediaRun> mediaRuns)
        {
            if (_LanguageItems != null)
            {
                foreach (LanguageItem languageItem in _LanguageItems)
                {
                    bool collectIt = true;
                    if ((languageSelectFlags != null) &&
                            !languageSelectFlags.TryGetValue(
                                languageItem.LanguageID.LanguageCultureExtensionCode,
                                out collectIt))
                        collectIt = false;
                    if (collectIt)
                        languageItem.CollectMediaReferences(mediaRuns);
                }
            }
        }

        public bool CopyMedia(string sourceMediaDirectory, string targetDirectoryRoot, List<string> copiedFiles,
            ref string errorMessage)
        {
            bool returnValue = true;

            if (_LanguageItems != null)
            {
                foreach (LanguageItem languageItem in _LanguageItems)
                {
                    if (!languageItem.CopyMedia(sourceMediaDirectory, targetDirectoryRoot, copiedFiles, ref errorMessage))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public bool PropogateMediaRun(LanguageID mediaLanguageID, int sentenceIndex, MediaRun mediaRun)
        {
            bool returnValue = false;

            if (_LanguageItems != null)
            {
                foreach (LanguageItem languageItem in _LanguageItems)
                {
                    if (languageItem.LanguageID.MediaLanguageCompare(mediaLanguageID) == 0)
                    {
                        if (!languageItem.HasSentenceRuns())
                        {
                            languageItem.LoadSentenceRunsFromText();

                            if (!languageItem.HasSentenceRuns())
                                continue;
                        }

                        TextRun sentenceRun = languageItem.GetSentenceRun(sentenceIndex);

                        if (sentenceRun != null)
                        {
                            MediaRun oldMediaRun = sentenceRun.GetMediaRunCorresponding(mediaRun);

                            if (oldMediaRun != mediaRun)
                            {
                                if (oldMediaRun != null)
                                    oldMediaRun.CopyMediaRun(mediaRun);
                                else
                                    sentenceRun.AddMediaRun(new MediaRun(mediaRun));
                            }

                            returnValue = true;
                        }
                    }
                }
            }

            return returnValue;
        }

        public bool PropogateMediaRun(int sentenceIndex, MediaRun mediaRun)
        {
            bool returnValue = false;

            if (_LanguageItems != null)
            {
                foreach (LanguageItem languageItem in _LanguageItems)
                {
                    if (!languageItem.HasSentenceRuns())
                    {
                        languageItem.LoadSentenceRunsFromText();

                        if (!languageItem.HasSentenceRuns())
                            continue;
                    }

                    TextRun sentenceRun = languageItem.GetSentenceRun(sentenceIndex);

                    if (sentenceRun != null)
                    {
                        MediaRun oldMediaRun = sentenceRun.GetMediaRunCorresponding(mediaRun);

                        if (oldMediaRun != null)
                            oldMediaRun.CopyMediaRun(mediaRun);
                        else
                            sentenceRun.AddMediaRun(new MediaRun(mediaRun));

                        returnValue = true;
                    }
                }
            }

            return returnValue;
        }

        public bool PropogateMediaRun(List<LanguageID> languageIDs, int sentenceIndex, MediaRun mediaRun)
        {
            bool returnValue = false;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = LanguageItem(languageID);

                if (languageItem != null)
                {
                    if (!languageItem.HasSentenceRuns())
                    {
                        languageItem.LoadSentenceRunsFromText();

                        if (!languageItem.HasSentenceRuns())
                            continue;
                    }

                    TextRun sentenceRun = languageItem.GetSentenceRun(sentenceIndex);

                    if (sentenceRun != null)
                    {
                        MediaRun oldMediaRun = sentenceRun.GetMediaRun(mediaRun.Key);

                        if (oldMediaRun != null)
                            oldMediaRun.CopyMediaRun(mediaRun);
                        else
                            sentenceRun.AddMediaRun(new MediaRun(mediaRun));

                        returnValue = true;
                    }
                }
            }

            return returnValue;
        }

        public bool PropogateMediaRunUnconditional(LanguageID mediaLanguageID, int sentenceIndex, MediaRun mediaRun)
        {
            bool returnValue = false;

            if (_LanguageItems != null)
            {
                foreach (LanguageItem languageItem in _LanguageItems)
                {
                    if (!languageItem.HasSentenceRuns())
                    {
                        languageItem.LoadSentenceRunsFromText();

                        if (!languageItem.HasSentenceRuns())
                            continue;
                    }

                    if (languageItem.LanguageID.MediaLanguageCompare(mediaLanguageID) == 0)
                    {
                        TextRun sentenceRun = languageItem.GetSentenceRun(sentenceIndex);

                        if (sentenceRun != null)
                        {
                            if ((sentenceRun.MediaRuns == null) || !sentenceRun.MediaRuns.Contains(mediaRun))
                                sentenceRun.AddMediaRun(new MediaRun(mediaRun));

                            returnValue = true;
                        }
                    }
                }
            }

            return returnValue;
        }

        public int AnnotationCount()
        {
            if (_Annotations != null)
                return _Annotations.Count();

            return 0;
        }

        public bool HasAnnotations()
        {
            if (_Annotations != null)
                return (_Annotations.Count() != 0 ? true : false);

            return false;
        }

        public bool HasAnnotation(string type)
        {
            if ((_Annotations != null) && !String.IsNullOrEmpty(type))
                return _Annotations.FirstOrDefault(x => x.Type == type) != null;

            return false;
        }

        public Annotation AnnotationIndexed(int index)
        {
            if ((_Annotations != null) && (index >= 0) && (index < _Annotations.Count()))
                return _Annotations.ElementAt(index);
            return null;
        }

        public Annotation FindAnnotation(string type, string tag = null)
        {
            if ((_Annotations != null) && !String.IsNullOrEmpty(type))
                return _Annotations.FirstOrDefault(x => x.Type == type);
            return null;
        }

        public string AnnotationValue(string type, string tag = null)
        {
            Annotation annotation = FindAnnotation(type, tag);

            if (annotation != null)
                return annotation.Value;

            return String.Empty;
        }

        public MultiLanguageString AnnotationMultiLanguageString(string type, string tag = null)
        {
            Annotation annotation = FindAnnotation(type);

            if (annotation != null)
                return annotation.Text;

            return null;
        }

        public string AnnotationText(string type, LanguageID languageID, string tag = null)
        {
            MultiLanguageString mls = AnnotationMultiLanguageString(type, tag);

            if (mls != null)
                return mls.Text(languageID);

            return String.Empty;
        }

        public void AddAnnotation(Annotation annotation)
        {
            if (_Annotations == null)
                _Annotations = new List<Annotation>();

            _Annotations.Add(annotation);
            ModifiedFlag = true;
        }

        public void AddAnnotations(List<Annotation> annotations)
        {
            if ((annotations == null) || (annotations.Count() == 0))
                return;

            if (_Annotations == null)
                _Annotations = new List<Annotation>();

            _Annotations.AddRange(annotations);
            ModifiedFlag = true;
        }

        public void InsertAnnotation(int index, Annotation annotation)
        {
            if (_Annotations == null)
                _Annotations = new List<Annotation>();
            if ((index >= 0) && (index < _Annotations.Count()))
                _Annotations.Insert(index, annotation);
            else
                _Annotations.Add(annotation);
            ModifiedFlag = true;
        }

        public void DeleteAnnotation(Annotation annotation)
        {
            if (_Annotations != null)
            {
                if (_Annotations.Remove(annotation))
                    ModifiedFlag = true;
            }
        }

        public void DeleteAnnotationTyped(string type, string tag = null)
        {
            Annotation annotation = FindAnnotation(type, tag);

            if (annotation != null)
                DeleteAnnotation(annotation);
        }

        public void DeleteAnnotationIndexed(int index)
        {
            if (_Annotations != null)
            {
                if ((index >= 0) && (index < _Annotations.Count()))
                {
                    _Annotations.RemoveAt(index);
                    ModifiedFlag = true;
                }
            }
        }

        public void DeleteAnnotations()
        {
            if (_Annotations != null)
            {
                if (_Annotations.Count() != 0)
                    ModifiedFlag = true;

                _Annotations = null;
            }
        }

        public List<Annotation> CloneAnnotations()
        {
            if (_Annotations == null)
                return null;

            List<Annotation> returnValue = new List<Annotation>(_Annotations.Count());

            foreach (Annotation annotation in _Annotations)
                returnValue.Add(new Annotation(annotation));

            return returnValue;
        }

        public void MergeAnnotations(MultiLanguageItem other)
        {
            if (other.AnnotationCount() == 0)
                return;

            foreach (Annotation otherAnnotation in other.Annotations)
            {
                if (!HasAnnotation(otherAnnotation.Type))
                    AddAnnotation(new Annotation(otherAnnotation));
                else
                {
                    Annotation annotation = FindAnnotation(otherAnnotation.Type);
                    annotation.Merge(otherAnnotation);
                }
            }
        }

        public bool IsNotMapped()
        {
            Annotation notMappedAnnotation = FindAnnotation("NotMapped");

            if (notMappedAnnotation == null)
                return false;

            return notMappedAnnotation.ValueAsBool;
        }

        public int ExpansionReferenceCount()
        {
            if (_ExpansionReferences != null)
                return _ExpansionReferences.Count();

            return 0;
        }

        public bool HasExpansionReference()
        {
            if (_ExpansionReferences != null)
                return (_ExpansionReferences.Count() != 0 ? true : false);

            return false;
        }

        public bool HasExpansionReference(MultiLanguageItemReference expansionReference)
        {
            if (_ExpansionReferences != null)
                return (_ExpansionReferences.FirstOrDefault(x => x.MatchKey(expansionReference.Key)) != null ? true : false);

            return false;
        }

        public MultiLanguageItemReference ExpansionReferenceIndexed(int index)
        {
            if ((_ExpansionReferences != null) && (index >= 0) && (index < _ExpansionReferences.Count()))
                return _ExpansionReferences.ElementAt(index);
            return null;
        }

        public void AddExpansionReference(MultiLanguageItemReference expansionReference)
        {
            if (_ExpansionReferences == null)
                _ExpansionReferences = new List<MultiLanguageItemReference>();

            _ExpansionReferences.Add(expansionReference);
            ModifiedFlag = true;
        }

        public void InsertExpansionReference(int index, MultiLanguageItemReference expansionReference)
        {
            if (_ExpansionReferences == null)
                _ExpansionReferences = new List<MultiLanguageItemReference>();

            if (index < _ExpansionReferences.Count())
                _ExpansionReferences.Insert(index, expansionReference);
            else
                _ExpansionReferences.Add(expansionReference);

            ModifiedFlag = true;
        }

        public void DeleteExpansionReference(MultiLanguageItemReference expansionReference)
        {
            if (_ExpansionReferences != null)
            {
                if (_ExpansionReferences.Remove(expansionReference))
                    ModifiedFlag = true;
            }
        }

        public void DeleteExpansionReferenceIndexed(int index)
        {
            if (_ExpansionReferences == null)
                return;

            if ((index >= _ExpansionReferences.Count()) && (index < _ExpansionReferences.Count()))
            {
                _ExpansionReferences.RemoveAt(index);
                ModifiedFlag = true;
            }
        }

        public void DeleteExpansionReferences()
        {
            if (_ExpansionReferences != null)
            {
                if (_ExpansionReferences.Count() != 0)
                    ModifiedFlag = true;

                _ExpansionReferences = null;
            }
        }

        public List<MultiLanguageItemReference> CloneExpansionReferences()
        {
            if (_ExpansionReferences == null)
                return null;

            List<MultiLanguageItemReference> returnValue = new List<MultiLanguageItemReference>(_ExpansionReferences.Count());

            foreach (MultiLanguageItemReference expansionReference in _ExpansionReferences)
                returnValue.Add(new MultiLanguageItemReference(expansionReference));

            return returnValue;
        }

        public int ExpansionCount()
        {
            return ExpansionReferenceCount();
        }

        public bool HasExpansion()
        {
            return HasExpansion();
        }

        public MultiLanguageItem ExpansionIndexed(int index)
        {
            MultiLanguageItemReference reference = ExpansionReferenceIndexed(index);

            if (reference == null)
                return null;

            BaseObjectNode node = Node;

            if (node == null)
                return null;

            if ((node.Key != reference.NodeKey) && (node.Tree != null))
            {
                node = node.Tree.GetNode(reference.NodeKey);

                if (node == null)
                    return null;
            }

            BaseObjectContent content = node.GetContent(reference.NodeContentKey);
            ContentStudyList expansionStudyList;
            MultiLanguageItem expansion = null;

            if (content != null)
            {
                if (content.ContentStorage == null)
                    content.ResolveReferences(ApplicationData.Repositories, false, false);

                expansionStudyList = content.GetContentStorageTyped<ContentStudyList>();
            }
            else
                expansionStudyList = null;

            if (expansionStudyList != null)
                expansion = expansionStudyList.GetStudyItem(reference.Key);

            return expansion;
        }

        public void AddExpansion(MultiLanguageItem expansion)
        {
            MultiLanguageItemReference reference = new MultiLanguageItemReference(expansion.Key, expansion.StudyListKey, expansion.StudyListNodeKey, expansion.NodeKey, expansion);
            AddExpansionReference(reference);
        }

        public void InsertExpansion(int index, MultiLanguageItem expansion)
        {
            MultiLanguageItemReference reference = new MultiLanguageItemReference(expansion.Key, expansion.StudyListKey, expansion.StudyListNodeKey, expansion.NodeKey, expansion);
            InsertExpansionReference(index, reference);
        }

        public void DeleteExpansion(MultiLanguageItem expansion)
        {
            if (_ExpansionReferences == null)
                return;

            int index = 0;

            foreach (MultiLanguageItemReference reference in _ExpansionReferences)
            {
                if (expansion.MatchKey(reference.ContentKey) && expansion.MatchKey(reference.Key))
                    break;

                index++;
            }

            DeleteExpansionReferenceIndexed(index);
        }

        public void DeleteExpansionIndexed(int index)
        {
            DeleteExpansionReferenceIndexed(index);
        }

        public void DeleteExpansions()
        {
            DeleteExpansionReferences();
        }

        public MultiLanguageItemReference CloneItemSource()
        {
            if (_ItemSource == null)
                return null;

            MultiLanguageItemReference returnValue = new MultiLanguageItemReference(_ItemSource);

            return returnValue;
        }

        public void SaveToReference()
        {
            if (HasItemSourceItem)
            {
                ItemSourceItem.LanguageItems = LanguageItems;
                ItemSourceItem.Annotations = Annotations;
            }
        }

        public void NoMoreReference()
        {
            ItemSource = null;
        }

        public bool DontTrackSource
        {
            get
            {
                if (ItemSource != null)
                    return ItemSource.DontTrackSource;
                return false;
            }
            set
            {
                if (ItemSource != null)
                    ItemSource.DontTrackSource = value;
            }
        }

        public virtual void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            if ((_ItemSource != null) && !DontTrackSource)
            {
                if ((_ItemSource.Item == null) || (_ItemSource.Tree == null))
                {
                    _ItemSource.ResolveReference(mainRepository,
                        ref _StudyList.StudyListCache, ref Tree.TreeCache);

                    /*
                    MultiLanguageItem item = _ItemSource.Item;

                    if (item != null)
                    {
                        _LanguageItems = item.CloneLanguageItems();
                        _Annotations = item.CloneAnnotations();
                    }
                    */
                }
            }
        }

        public virtual bool SaveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            if (_ItemSource != null)
                return _ItemSource.SaveReference(mainRepository);

            return true;
        }

        public virtual bool UpdateReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            if (_ItemSource != null)
                return _ItemSource.UpdateReference(mainRepository);

            return true;
        }

        public virtual bool UpdateReferencesCheck(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            if (_ItemSource != null)
                return _ItemSource.UpdateReferenceCheck(mainRepository);

            return true;
        }

        public virtual void ClearReferences(bool recurseParents, bool recurseChildren)
        {
            if (_ItemSource != null)
                _ItemSource.ClearReference();
        }

        public override void Display(string label, DisplayDetail detail, int indent)
        {
            switch (detail)
            {
                case DisplayDetail.Lite:
                    {
                        string strs = String.Empty;
                        int c = Count();
                        int i;
                        for (i = 0; i < c; i++)
                        {
                            if (i != 0)
                                strs += "|";
                            strs += Text(i);
                        }
                        if (!String.IsNullOrEmpty(KeyString))
                            ObjectUtilities.DisplayLabelArgument(this, label, indent, KeyString + ": " + strs);
                        else
                            ObjectUtilities.DisplayLabelArgument(this, label, indent, strs);
                    }
                    break;
                case DisplayDetail.Full:
                    {
                        if (!String.IsNullOrEmpty(KeyString))
                            ObjectUtilities.DisplayLabelArgument(this, label, indent, KeyString);
                        else
                            ObjectUtilities.DisplayLabel(this, label, indent);
                        int c = Count();
                        int i;
                        for (i = 0; i < c; i++)
                        {
                            LanguageItem li = LanguageItem(i);
                            string fmt = li.LanguageAbbrev + ": " + Text(i);
                            ObjectUtilities.DisplayMessage(fmt, indent + 1);
                        }
                        if (!String.IsNullOrEmpty(_SpeakerNameKey))
                            DisplayField("SpeakerNameKey", _SpeakerNameKey, indent + 1);
                        if (AnnotationCount() != 0)
                        {
                            foreach (Annotation annotation in _Annotations)
                                annotation.Display(null, detail, indent + 1);
                        }
                        if (ExpansionReferenceCount() != 0)
                        {
                            foreach (MultiLanguageItemReference expansionReference in _ExpansionReferences)
                                expansionReference.Display(null, detail, indent + 1);
                        }
                        if (_ItemSource != null)
                            _ItemSource.Display(null, detail, indent);
                        if (!String.IsNullOrEmpty(_MediaTildeUrl))
                            DisplayField("MediaTildeUrl", _MediaTildeUrl, indent + 1);
                        if (_ItemSource != null)
                            DisplayField("StudyList", _StudyList.KeyString, indent);
                    }
                    break;
                case DisplayDetail.Diagnostic:
                case DisplayDetail.Xml:
                    base.Display(label, detail, indent);
                    break;
                default:
                    break;
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if ((_LanguageItems != null) && (_LanguageItems.Count() != 0))
            {
                element.Add(new XAttribute("Count", _LanguageItems.Count().ToString()));

                foreach (LanguageItem languageItem in _LanguageItems)
                    element.Add(languageItem.Xml);
            }

            if (!String.IsNullOrEmpty(_SpeakerNameKey))
                element.Add(new XAttribute("SpeakerNameKey", _SpeakerNameKey));

            if ((_Annotations != null) && (_Annotations.Count() != 0))
            {
                foreach (Annotation annotation in _Annotations)
                    element.Add(annotation.Xml);
            }

            if ((_ExpansionReferences != null) && (_ExpansionReferences.Count() != 0))
            {
                foreach (MultiLanguageItemReference expansionReference in _ExpansionReferences)
                    element.Add(expansionReference.GetElement("ExpansionReference"));
            }

            if (_ItemSource != null)
                element.Add(_ItemSource.GetElement("ItemSource"));

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Count":
                    _LanguageItems = new List<LanguageItem>(Convert.ToInt32(attributeValue));
                    break;
                case "SpeakerNameKey":
                    _SpeakerNameKey = attributeValue;
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
                case "LanguageItem":
                    {
                        LanguageItem languageItem = new LanguageItem(childElement);
                        Add(languageItem);
                    }
                    break;
                case "Annotation":
                    {
                        Annotation annotation = new Annotation(childElement);
                        AddAnnotation(annotation);
                    }
                    break;
                case "ExpansionReference":
                    {
                        MultiLanguageItemReference expansionReference = new MultiLanguageItemReference(childElement);
                        AddExpansionReference(expansionReference);
                    }
                    break;
                case "ItemSource":
                    ItemSource = new MultiLanguageItemReference(childElement);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            MultiLanguageItem otherMultiLanguageItem = other as MultiLanguageItem;

            if (otherMultiLanguageItem == null)
                return 1;

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = JTLanguageModelsPortable.Content.LanguageItem.CompareLanguageItemLists(_LanguageItems, otherMultiLanguageItem.LanguageItems);

            if (diff != 0)
                return diff;

            diff = Annotation.CompareAnnotationLists(_Annotations, otherMultiLanguageItem.Annotations);

            if (diff != 0)
                return diff;

            diff = MultiLanguageItemReference.CompareMultiLanguageItemReferenceLists(_ExpansionReferences, otherMultiLanguageItem.ExpansionReferences);

            if (diff != 0)
                return diff;

            diff = MultiLanguageItemReference.CompareMultiLanguageItemReference(_ItemSource, otherMultiLanguageItem.ItemSource);

            return diff;
        }

        public static int Compare(MultiLanguageItem item1, MultiLanguageItem item2)
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

        public static LanguageID LanguageToCompare;

        public static int CompareMultiLanguageItem(MultiLanguageItem item1, MultiLanguageItem item2)
        {
            if (((object)item1 == null) && ((object)item2 == null))
                return 0;
            if ((object)item1 == null)
                return -1;
            if ((object)item2 == null)
                return 1;
            if (LanguageToCompare == null)
                LanguageToCompare = LanguageLookup.English;
            LanguageItem ls1 = item1.LanguageItem(LanguageToCompare);
            LanguageItem ls2 = item2.LanguageItem(LanguageToCompare);
            return JTLanguageModelsPortable.Content.LanguageItem.Compare(ls1, ls2);
        }

        public static int CompareText(MultiLanguageItem item1, MultiLanguageItem item2)
        {
            if (((object)item1 == null) && ((object)item2 == null))
                return 0;
            if ((object)item1 == null)
                return -1;
            if ((object)item2 == null)
                return 1;
            foreach (LanguageItem languageItem1 in item1.LanguageItems)
            {
                LanguageItem languageItem2 = item2.LanguageItem(languageItem1.LanguageID);
                if (languageItem2 == null)
                    continue;
                int diff = LanguageString.CompareText(languageItem1, languageItem2);
                if (diff != 0)
                    return diff;
            }
            return 0;
        }

        public static int CompareMultiLanguageItemLists(List<MultiLanguageItem> list1, List<MultiLanguageItem> list2)
        {
            return ObjectUtilities.CompareTypedObjectLists<MultiLanguageItem>(list1, list2);
        }

        public static int GetStudyItemsSentenceCount(List<LanguageID> languageIDs, List<MultiLanguageItem> studyItems)
        {
            int count = 0;

            if (languageIDs == null)
                return 0;

            if (studyItems == null)
                return 0;

            foreach (MultiLanguageItem studyItem in studyItems)
                count += studyItem.GetSentenceCount(languageIDs);

            return count;
        }

        public static int GetStudyItemsMaxSentenceCount(List<LanguageID> languageIDs, List<MultiLanguageItem> studyItems)
        {
            int count = 0;

            if (languageIDs == null)
                return 0;

            if (studyItems == null)
                return 0;

            foreach (MultiLanguageItem studyItem in studyItems)
                count += studyItem.GetMaxSentenceCount(languageIDs);

            return count;
        }

        public static int GetStudyItemsSeparateSentenceCount(List<LanguageID> languageIDs, List<MultiLanguageItem> studyItems)
        {
            int count = 0;

            if (languageIDs == null)
                return 0;

            if (studyItems == null)
                return 0;

            foreach (MultiLanguageItem studyItem in studyItems)
                count += studyItem.GetSeparateSentenceCount(languageIDs);

            return count;
        }

        public static MultiLanguageItem GetStudyItemsSentenceIndexed(List<MultiLanguageItem> studyItems, int index, List<LanguageID> languageIDs)
        {
            int studyItemIndex = 0;
            int currentBaseIndex = 0;
            int count = 0;

            if (studyItems == null)
                return null;

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                currentBaseIndex = studyItemIndex;
                count = studyItem.GetMaxSentenceCount(languageIDs);

                if (index < currentBaseIndex + count)
                {
                    MultiLanguageItem multiLanguageItem = studyItem.GetSentenceIndexed(index - currentBaseIndex, languageIDs);
                    return multiLanguageItem;
                }

                studyItemIndex += count;
            }

            return null;
        }

        public static List<ContentStudyList> GetStudyItemsStudyLists(List<MultiLanguageItem> studyItems)
        {
            List<ContentStudyList> studyLists = new List<ContentStudyList>();

            if (studyItems == null)
                return studyLists;

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                ContentStudyList studyList = studyItem.StudyList;

                if (!studyLists.Contains(studyList))
                    studyLists.Add(studyList);
            }

            return studyLists;
        }

        public static void SetStudyItemsStudyLists(List<MultiLanguageItem> studyItems,
            ContentStudyList studyList)
        {
            if (studyItems == null)
                return;

            foreach (MultiLanguageItem studyItem in studyItems)
                studyItem.StudyList = studyList;
        }

        public static bool GetStudyItemAndSentenceIndex(List<MultiLanguageItem> studyItems,
            int index, List<LanguageID> languageIDs,
            out MultiLanguageItem studyItem, out int studyItemIndex, out int sentenceIndex)
        {
            studyItem = null;
            studyItemIndex = 0;
            sentenceIndex = 0;

            if (studyItems == null)
                return false;

            int itemIndex = 0;
            int paragraphCount = studyItems.Count();
            int sentenceCount;
            bool returnValue = true;

            for (studyItemIndex = 0; studyItemIndex < paragraphCount; studyItemIndex++)
            {
                studyItem = studyItems[studyItemIndex];
                sentenceCount = studyItem.GetMaxSentenceCount(languageIDs);

                if (sentenceCount == 0)
                    sentenceCount = 1;

                if (index < (itemIndex + sentenceCount))
                {
                    sentenceIndex = index - itemIndex;
                    break;
                }

                itemIndex += sentenceCount;
            }

            return returnValue;
        }

        public static bool GetLanguageItemAndSentenceIndex(List<MultiLanguageItem> studyItems,
            int index, LanguageID languageID,
            List<LanguageID> languageIDs,
            out MultiLanguageItem studyItem,
            out LanguageItem languageItem,
            out int sentenceIndex)
        {
            studyItem = null;
            languageItem = null;
            sentenceIndex = 0;

            if (studyItems == null)
                return false;

            MultiLanguageItem multiLanguageItem;
            int itemIndex = 0;
            int paragraphCount = studyItems.Count();
            int sentenceCount;
            bool returnValue = true;

            for (int paragraphIndex = 0; paragraphIndex < paragraphCount; paragraphIndex++)
            {
                multiLanguageItem = studyItems[paragraphIndex];

                if (multiLanguageItem == null)
                    return false;

                sentenceCount = multiLanguageItem.GetMaxSentenceCount(languageIDs);

                if (sentenceCount == 0)
                    sentenceCount = 1;

                if (index < (itemIndex + sentenceCount))
                {
                    sentenceIndex = index - itemIndex;
                    studyItem = multiLanguageItem;
                    languageItem = studyItem.LanguageItem(languageID);
                    break;
                }

                itemIndex += sentenceCount;
            }

            return returnValue;
        }

        public static bool GetStudyItemLanguageItemAndSentenceIndex(List<MultiLanguageItem> studyItems,
            int index, LanguageID languageID,
            List<LanguageID> languageIDs,
            out MultiLanguageItem studyItem, out LanguageItem languageItem, out int studyItemIndex, out int sentenceIndex)
        {
            studyItem = null;
            languageItem = null;
            studyItemIndex = 0;
            sentenceIndex = 0;

            if (studyItems == null)
                return false;

            int itemIndex = 0;
            int paragraphCount = studyItems.Count();
            int sentenceCount;
            bool returnValue = false;

            for (studyItemIndex = 0; studyItemIndex < paragraphCount; studyItemIndex++)
            {
                studyItem = studyItems[studyItemIndex];

                if (studyItem == null)
                    return false;

                sentenceCount = studyItem.GetMaxSentenceCount(languageIDs);

                if (sentenceCount == 0)
                    sentenceCount = 1;

                if (index < (itemIndex + sentenceCount))
                {
                    sentenceIndex = index - itemIndex;
                    languageItem = studyItem.LanguageItem(languageID);
                    returnValue = true;
                    break;
                }

                itemIndex += sentenceCount;
            }

            return returnValue;
        }

        public static bool GetSentenceItemIndex(List<MultiLanguageItem> studyItems,
            int studyItemIndex, int sentenceIndex,
            List<LanguageID> languageIDs, out int sentenceItemIndex)
        {
            sentenceItemIndex = 0;

            if (studyItems == null)
                return false;

            int paragraphCount = studyItems.Count();
            int sentenceCount;
            bool returnValue = true;

            for (int paragraphIndex = 0; paragraphIndex < studyItemIndex; paragraphIndex++)
            {
                MultiLanguageItem studyItem = studyItems[paragraphIndex];
                sentenceCount = studyItem.GetMaxSentenceCount(languageIDs);
                if (sentenceCount == 0)
                    sentenceCount = 1;
                sentenceItemIndex += sentenceCount;
            }

            if (sentenceIndex != -1)
                sentenceItemIndex += sentenceIndex;

            return returnValue;
        }

        public static bool GetItemIndex(List<MultiLanguageItem> studyItems,
            int studyItemIndex, int sentenceIndex,
            List<LanguageID> languageIDs, LanguageID itemLanguageID,
            string rowTextFormat, string languageTextFormat,
            out int itemIndex)
        {
            bool returnValue = false;

            itemIndex = 0;

            switch (languageTextFormat)
            {
                default:
                case "Mixed":
                    switch (rowTextFormat)
                    {
                        default:
                        case "Paragraphs":
                            itemIndex = studyItemIndex;
                            break;
                        case "Sentences":
                            returnValue = GetSentenceItemIndex(studyItems, studyItemIndex, sentenceIndex,
                                languageIDs, out itemIndex);
                            break;
                    }
                    break;
                case "Separate":
                    switch (rowTextFormat)
                    {
                        default:
                        case "Paragraphs":
                            itemIndex = studyItemIndex;
                            break;
                        case "Sentences":
                            List<LanguageID> lids;
                            if (itemLanguageID != null)
                                lids = new List<LanguageID>() { itemLanguageID };
                            else
                                lids = languageIDs;
                            returnValue = GetSentenceItemIndex(studyItems, studyItemIndex, sentenceIndex,
                                lids, out itemIndex);
                            break;
                    }
                    break;
            }

            return returnValue;
        }

        public static bool GetSentenceRunsIndexed(List<MultiLanguageItem> studyItems,
            int index, List<LanguageID> languageIDs,
            out MultiLanguageItem studyItem, out List<TextRun> sentenceRuns)
        {
            studyItem = null;
            sentenceRuns = new List<TextRun>();

            if (studyItems == null)
                return false;

            int itemIndex = 0;
            int paragraphCount = studyItems.Count();
            int paragraphLength;
            int sentenceCount;
            int sentenceIndex;
            LanguageItem languageItem;
            TextRun sentenceRun;
            bool returnValue = true;

            for (int paragraphIndex = 0; paragraphIndex < paragraphCount; paragraphIndex++)
            {
                studyItem = studyItems[paragraphIndex];
                sentenceCount = studyItem.GetMaxSentenceCount(languageIDs);

                if (index < (itemIndex + sentenceCount))
                {
                    sentenceIndex = index - itemIndex;

                    foreach (LanguageID languageID in languageIDs)
                    {
                        languageItem = studyItem.LanguageItem(languageID);

                        if (languageItem != null)
                        {
                            sentenceRun = languageItem.GetSentenceRun(sentenceIndex);
                            paragraphLength = languageItem.TextLength;
                        }
                        else
                        {
                            sentenceRun = null;
                            paragraphLength = 0;
                        }

                        if (sentenceRun == null)
                            sentenceRun = new TextRun(paragraphLength, 0, null);

                        sentenceRuns.Add(sentenceRun);
                    }

                    break;
                }

                itemIndex += sentenceCount;
            }

            return returnValue;
        }

        public static bool GetMediaInfo(List<MultiLanguageItem> studyItems, string mediaRunKey, LanguageID languageID,
            out bool hasAudio, out bool hasVideo, out bool hasSlow, out bool hasPicture,
            List<string> audioVideoUrls)
        {
            bool hasAudioTemp;
            bool hasVideoTemp;
            bool hasSlowTemp;
            bool hasPictureTemp;
            bool returnValue = false;

            hasAudio = false;
            hasVideo = false;
            hasSlow = false;
            hasPicture = false;

            if (studyItems != null)
            {
                foreach (MultiLanguageItem studyItem in studyItems)
                {
                    if (studyItem.GetMediaInfo(mediaRunKey, languageID, studyItem.MediaTildeUrl, studyItem.Node,
                            out hasAudioTemp, out hasVideoTemp, out hasSlowTemp, out hasPictureTemp, audioVideoUrls))
                        returnValue = true;

                    if (hasAudioTemp)
                        hasAudio = true;

                    if (hasVideoTemp)
                        hasVideo = true;

                    if (hasSlowTemp)
                        hasSlow = true;

                    if (hasPictureTemp)
                        hasPicture = true;
                }
            }

            return returnValue;
        }

        public static List<List<Annotation>> GetAnnotationLists(List<MultiLanguageItem> studyItems,
            bool addChildTitles, LanguageID uiLanguageID, ContentStudyList rootStudyList)
        {
            if (studyItems == null)
                return new List<List<Annotation>>(0);

            int studyItemCount = studyItems.Count;
            List<List<Annotation>> annotationLists = new List<List<Annotation>>(studyItemCount);
            List<Annotation> annotations;
            MultiLanguageItem studyItem;
            ContentStudyList currentStudyList = rootStudyList;
            BaseObjectContent currentContent = rootStudyList.Content;
            BaseObjectNode currentNode = currentContent.Node;
            List<BaseObjectNode> currentNodeList = currentNode.NodeHierarchyList;
            List<BaseObjectNode> nodeList = null;
            MultiLanguageString currentTitle;
            Annotation currentTitleAnnotation;
            string currentTitleString;
            int studyItemIndex;

            if (studyItems != null)
            {
                studyItemCount = studyItems.Count;
                annotationLists = new List<List<Annotation>>(studyItems.Count);

                for (studyItemIndex = 0; studyItemIndex < studyItemCount; studyItemIndex++)
                {
                    studyItem = studyItems[studyItemIndex];

                    if (studyItem == null) // should not happen
                        continue;

                    annotations = studyItem.Annotations;

                    if (studyItem.StudyList != currentStudyList)
                    {
                        currentStudyList = studyItem.StudyList;

                        if (annotations != null)
                            annotations = new List<Annotation>(annotations);
                        else
                            annotations = new List<Annotation>(1);

                        //if (!studyItem.HasAnnotation("Heading"))
                        {
                            currentContent = currentStudyList.Content;
                            currentNode = currentStudyList.Node;

                            if (currentContent.HasContentParent())
                            {
                                currentTitle = currentContent.Title;

                                if (currentTitle != null)
                                {
                                    currentTitleString = currentTitle.Text(uiLanguageID);

                                    if (!String.IsNullOrEmpty(currentTitleString))
                                    {
                                        currentTitleAnnotation = new Annotation(
                                            "Heading",
                                            null,
                                            new MultiLanguageString(
                                                "Heading", uiLanguageID, currentTitleString));
                                        annotations.Insert(0, currentTitleAnnotation);
                                    }
                                }
                            }

                            //if (!currentContent.HasContentParent())
                            {
                                nodeList = currentNode.NodeHierarchyList;

                                if (BaseObjectNode.CompareNodeLists(nodeList, currentNodeList) != 0)
                                {
                                    int count = nodeList.Count /*- 1*/;
                                    int index;

                                    for (index = count - 1; index >= 0; index--)
                                    {
                                        BaseObjectNode parentNode = nodeList[index];

                                        if ((index < currentNodeList.Count) && (parentNode == currentNodeList[index]))
                                            continue;

                                        currentTitle = parentNode.Title;

                                        if (currentTitle != null)
                                        {
                                            currentTitleString = currentTitle.Text(uiLanguageID);

                                            if (!String.IsNullOrEmpty(currentTitleString))
                                            {
                                                currentTitleAnnotation = new Annotation(
                                                    "Heading",
                                                    null,
                                                    new MultiLanguageString(
                                                        "Heading", uiLanguageID, currentTitleString));
                                                annotations.Insert(0, currentTitleAnnotation);
                                            }
                                        }
                                    }
                                }

                                currentNodeList = nodeList;
                            }
                        }
                    }

                    annotationLists.Add(annotations);
                }
            }
            else
                annotationLists = new List<List<Annotation>>(0);

            return annotationLists;
        }

        public static List<MultiLanguageItem> CloneStudyItems(
            List<MultiLanguageItem> sourceStudyItems)
        {
            if (sourceStudyItems == null)
                return null;

            int studyItemCount = sourceStudyItems.Count;
            List<MultiLanguageItem> targetStudyItems = new List<MultiLanguageItem>(studyItemCount);
            MultiLanguageItem studyItem;

            for (int studyItemIndex = 0; studyItemIndex < studyItemCount; studyItemIndex++)
            {
                studyItem = sourceStudyItems[studyItemIndex];

                if (studyItem == null)
                    continue;

                targetStudyItems.Add(new MultiLanguageItem(studyItem));
            }

            return targetStudyItems;
        }

        public static bool CopyStudyItemsSelected(
            List<MultiLanguageItem> sourceStudyItems,
            List<MultiLanguageItem> targetStudyItems,
            List<bool> itemSelectFlags)
        {
            int studyItemCount = sourceStudyItems.Count;
            MultiLanguageItem studyItem;
            bool returnValue = true;

            for (int studyItemIndex = 0; studyItemIndex < studyItemCount; studyItemIndex++)
            {
                if (!itemSelectFlags[studyItemIndex])
                    continue;

                studyItem = sourceStudyItems[studyItemIndex];

                if (studyItem == null)
                {
                    returnValue = false;
                    continue;
                }

                targetStudyItems.Add(new MultiLanguageItem(studyItem));
            }

            return returnValue;
        }

        public static bool CutStudyItemsSelected(
            List<MultiLanguageItem> sourceStudyItems,
            List<MultiLanguageItem> targetStudyItems,
            List<bool> itemSelectFlags)
        {
            int studyItemCount = sourceStudyItems.Count;
            MultiLanguageItem studyItem;
            bool returnValue = true;

            for (int studyItemIndex = studyItemCount - 1; studyItemIndex >= 0; studyItemIndex--)
            {
                if (!itemSelectFlags[studyItemIndex])
                    continue;

                studyItem = sourceStudyItems[studyItemIndex];

                if (studyItem == null)
                {
                    returnValue = false;
                    continue;
                }

                ContentStudyList targetStudyList = studyItem.StudyList;

                if (targetStudyList == null)
                {
                    returnValue = false;
                    continue;
                }

                targetStudyItems.Insert(0, studyItem);

                targetStudyList.DeleteStudyItem(studyItem);
                ContentUtilities.DeleteSelectFlags(itemSelectFlags, studyItemIndex, 1);
            }

            return returnValue;
        }

        public static void CopySpeakerNamesFromStudyItems(
            List<MultiLanguageItem> studyItems,
            ContentStudyList sourceStudyList)
        {
            foreach (MultiLanguageItem studyItem in studyItems)
            {
                string speakerNameKey = studyItem.SpeakerNameKey;
                ContentStudyList targetStudyList = studyItem.StudyList;

                if (targetStudyList == null)
                    continue;

                if (!targetStudyList.HasSpeakerName(speakerNameKey))
                {
                    MultiLanguageString speakerName = sourceStudyList.GetSpeakerName(speakerNameKey);

                    if (speakerName != null)
                        targetStudyList.AddSpeakerName(speakerName);
                }
            }
        }

        public void DumpWords(List<LanguageID> languageIDs)
        {
            List<MultiLanguageItem> sentenceWords = GetSentenceWords(languageIDs, true);

            foreach (MultiLanguageItem wordMLS in sentenceWords)
            {
                foreach (LanguageItem languageItem in wordMLS.LanguageItems)
                    ApplicationData.Global.PutConsoleMessage("|" + languageItem.Text + "|");

                ApplicationData.Global.PutConsoleMessage("");
            }

            ApplicationData.Global.PutConsoleMessage("------");
            ApplicationData.Global.PutConsoleMessage("");
        }
    }
}
