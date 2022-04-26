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
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Content
{
    public class ContentMediaItem : BaseContentStorage
    {
        protected List<string> _SourceContentKeys;              // The keys of the study lists containing the written content source.
        protected string _TranscriptContentKey;                 // The key of the transcript study list, generated or otherwise.
        protected string _DisplayWidth;                         // The display width in pixels, or "auto" or "native".
        protected string _DisplayHeight;                        // The display height in pixels, or "auto" or "native".
        protected List<LanguageMediaItem> _LanguageMediaItems;  // Describes different media files representing the same content in different languages.
        protected ContentMediaItem _MediaItemSource;            // If referencing another media item in full. Not stored.
        protected List<MultiLanguageString> _LocalSpeakerNames; // Local speaker names for generated items.
        protected List<MultiLanguageItem> _LocalStudyItems;     // Local study items for generated media items.
        protected string _MediaFileNamePattern;                 // Media file name pattern (i.e. FileName%t%h.mp3).
        protected string _PlayerSource;                         // Source of player (File, Cloud, YouTube).
        protected string _PlayerType;                           // Type of player.
        public static List<string> PlayerSources = new List<string>()
        {
            "File",
            "Cloud",
            "YouTube"
        };
        public static List<string> PlayerTypes = new List<string>()
        {
            "Full",
            "Small",
            "Tiny",
            "None"
        };

        public ContentMediaItem(object key, BaseObjectContent content, List<string> sourceContentKeys,
            string transcriptContentKey, List<LanguageMediaItem> languageMediaItems)
            : base(key, "MediaItems", content)
        {
            _SourceContentKeys = sourceContentKeys;
            _TranscriptContentKey = transcriptContentKey;
            _LanguageMediaItems = languageMediaItems;
            _MediaItemSource = null;
            _LocalSpeakerNames = null;
            _LocalStudyItems = null;
            _MediaFileNamePattern = null;
            _PlayerSource = "File";
            _PlayerType = "Full";
        }

        public ContentMediaItem(object key)
            : base(key, "MediaItems", null)
        {
            ClearContentMediaItem();
        }

        public ContentMediaItem(ContentMediaItem other)
            : base(other)
        {
            Copy(other);
            Modified = false;
        }

        public ContentMediaItem(XElement element)
        {
            OnElement(element);
        }

        public ContentMediaItem()
        {
            ClearContentMediaItem();
        }

        public void Copy(ContentMediaItem other)
        {
            base.Copy(other);

            if (other == null)
            {
                ClearContentMediaItem();
                return;
            }

            if (other.SourceContentKeys != null)
                _SourceContentKeys = new List<string>(other.SourceContentKeys);
            else
                _SourceContentKeys = null;

            _TranscriptContentKey = other.TranscriptContentKey;
            _DisplayWidth = other.DisplayWidth;
            _DisplayHeight = other.DisplayHeight;

            _LanguageMediaItems = other.CloneLanguageMediaItems();
            _LocalSpeakerNames = other.CloneLocalSpeakerNames();
            _LocalStudyItems = other.CloneLocalStudyItems();
            _MediaFileNamePattern = other.MediaFileNamePattern;
            _PlayerSource = other.PlayerSource;
            _PlayerType = other.PlayerType;

            ModifiedFlag = true;
        }

        public void CopyDeep(ContentMediaItem other)
        {
            Copy(other);
            base.CopyDeep(other);
        }

        public override void Clear()
        {
            base.Clear();
            ClearContentMediaItem();
        }

        public void ClearContentMediaItem()
        {
            _Source = "MediaItems";
            _SourceContentKeys = null;
            _TranscriptContentKey = null;
            _DisplayWidth = null;
            _DisplayHeight = null;
            _LanguageMediaItems = null;
            _LocalSpeakerNames = null;
            _LocalStudyItems = null;
            _MediaFileNamePattern = null;
            _PlayerSource = "File";
            _PlayerType = "Full";
        }

        public override IBaseObject Clone()
        {
            return new ContentMediaItem(this);
        }

        public override ContentClassType ContentClass
        {
            get
            {
                return ContentClassType.MediaItem;
            }
        }

        public string ContentType
        {
            get
            {
                BaseObjectContent content = Content;
                if (content != null)
                    return content.ContentType;
                return null;
            }
        }

        public string ContentSubType
        {
            get
            {
                BaseObjectContent content = Content;
                if (content != null)
                    return content.ContentSubType;
                return null;
            }
        }

        public List<string> SourceContentKeys
        {
            get
            {
                return _SourceContentKeys;
            }
            set
            {
                if (value != _SourceContentKeys)
                {
                    _SourceContentKeys = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string SourceContentKeysString
        {
            get
            {
                return ObjectUtilities.GetStringFromStringList(_SourceContentKeys);
            }
            set
            {
                if (value != SourceContentKeysString)
                {
                    _SourceContentKeys = ObjectUtilities.GetStringListFromString(value);
                    ModifiedFlag = true;
                }
            }
        }

        public List<BaseObjectContent> SourceContents
        {
            get
            {
                List<BaseObjectContent> sourceContents = new List<BaseObjectContent>();
                BaseObjectNode node = Node;

                if ((node != null) && (_SourceContentKeys != null))
                {
                    foreach (object sourceContentKey in _SourceContentKeys)
                    {
                        BaseObjectContent sourceContent;

                        sourceContent = node.GetContent((string)sourceContentKey);

                        if (sourceContent != null)
                            sourceContents.Add(sourceContent);
                    }
                }

                return sourceContents;
            }
            set
            {
                if ((value == null) || (value.Count() == 0))
                {
                    if ((_SourceContentKeys != null) && (_SourceContentKeys.Count() != 0))
                    {
                        _SourceContentKeys = new List<string>();
                        ModifiedFlag = true;
                    }
                }
                else
                {
                    _SourceContentKeys = new List<string>();

                    foreach (BaseObjectContent sourceContent in value)
                        _SourceContentKeys.Add(sourceContent.KeyString);

                    ModifiedFlag = true;
                }
            }
        }

        public List<ContentStudyList> SourceStudyLists
        {
            get
            {
                List<ContentStudyList> sourceStudyLists = new List<ContentStudyList>();
                BaseObjectNode node = Node;

                if ((node != null) && (_SourceContentKeys != null))
                {
                    foreach (object sourceContentKey in _SourceContentKeys)
                    {
                        BaseObjectContent sourceContent;

                        sourceContent = node.GetContent((string)sourceContentKey);

                        if (sourceContent != null)
                        {
                            ContentStudyList studyList = sourceContent.ContentStorageStudyList;
                            if (studyList != null)
                                sourceStudyLists.Add(studyList);
                        }
                    }
                }

                return sourceStudyLists;
            }
        }

        public string TranscriptContentKey
        {
            get
            {
                return _TranscriptContentKey;
            }
            set
            {
                if (value != _TranscriptContentKey)
                {
                    _TranscriptContentKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public BaseObjectContent TranscriptContent
        {
            get
            {
                BaseObjectContent transcriptContent = null;
                BaseObjectNode node = Node;

                if ((node != null) && (_TranscriptContentKey != null))
                    transcriptContent = node.GetContent(_TranscriptContentKey);

                return transcriptContent;
            }
            set
            {
                if (value == null)
                {
                    if (_TranscriptContentKey != null)
                    {
                        _TranscriptContentKey = null;
                        ModifiedFlag = true;
                    }
                }
                else if (value.KeyString != _TranscriptContentKey)
                {
                    _TranscriptContentKey = value.KeyString;
                    ModifiedFlag = true;
                }
            }
        }

        public string DisplayWidth
        {
            get
            {
                if (String.IsNullOrEmpty(_DisplayWidth))
                    return "auto";
                return _DisplayWidth;
            }
            set
            {
                if (value != _DisplayWidth)
                {
                    _DisplayWidth = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string DisplayHeight
        {
            get
            {
                if (String.IsNullOrEmpty(_DisplayHeight))
                    return "auto";
                return _DisplayHeight;
            }
            set
            {
                if (value != _DisplayHeight)
                {
                    _DisplayHeight = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<LanguageMediaItem> LanguageMediaItems
        {
            get
            {
                return _LanguageMediaItems;
            }
            set
            {
                if (value != _LanguageMediaItems)
                {
                    _LanguageMediaItems = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<MediaDescription> MediaDescriptions
        {
            get
            {
                List<MediaDescription> mediaDescriptions = new List<MediaDescription>();
                if (_LanguageMediaItems != null)
                {
                    foreach (LanguageMediaItem languageMediaItem in _LanguageMediaItems)
                    {
                        if (languageMediaItem.MediaDescriptions != null)
                            mediaDescriptions.AddRange(languageMediaItem.MediaDescriptions);
                    }
                }
                return mediaDescriptions;
            }
        }

        public List<MultiLanguageString> LocalSpeakerNames
        {
            get
            {
                return _LocalSpeakerNames;
            }
            set
            {
                if (_LocalSpeakerNames != value)
                    ModifiedFlag = true;

                _LocalSpeakerNames = value;
            }
        }

        public void CloneLocalSpeakerNameCheck(MultiLanguageItem studyItem)
        {
            string speakerNameKey = studyItem.SpeakerNameKey;

            if (String.IsNullOrEmpty(speakerNameKey))
                return;

            if ((_LocalSpeakerNames != null) && (_LocalSpeakerNames.FirstOrDefault(x => x.KeyString == speakerNameKey) != null))
                return;

            if (studyItem.StudyList != null)
            {
                MultiLanguageString speakerNameItem = studyItem.StudyList.GetSpeakerName(speakerNameKey);

                if (speakerNameItem == null)
                    return;

                speakerNameItem = new MultiLanguageString(speakerNameItem);

                if (_LocalSpeakerNames == null)
                    _LocalSpeakerNames = new List<MultiLanguageString>() { speakerNameItem };
                else
                    _LocalSpeakerNames.Add(speakerNameItem);
            }
        }

        public List<MultiLanguageItem> LocalStudyItems
        {
            get
            {
                return _LocalStudyItems;
            }
            set
            {
                if (_LocalStudyItems != value)
                {
                    _LocalStudyItems = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string MediaFileNamePattern
        {
            get
            {
                return _MediaFileNamePattern;
            }
            set
            {
                if (value != _MediaFileNamePattern)
                {
                    _MediaFileNamePattern = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string PlayerSource
        {
            get
            {
                return _PlayerSource;
            }
            set
            {
                if (value != _PlayerSource)
                {
                    _PlayerSource = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string PlayerType
        {
            get
            {
                return _PlayerType;
            }
            set
            {
                if (value != _PlayerType)
                {
                    _PlayerType = value;
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

                if (_LanguageMediaItems != null)
                {
                    foreach (LanguageMediaItem languageMediaItem in _LanguageMediaItems)
                    {
                        if (languageMediaItem.Modified)
                            return true;
                    }
                }

                if (_LocalSpeakerNames != null)
                {
                    foreach (MultiLanguageString speakerName in _LocalSpeakerNames)
                    {
                        if (speakerName.Modified)
                            return true;
                    }
                }

                if (_LocalStudyItems != null)
                {
                    foreach (MultiLanguageItem studyItem in _LocalStudyItems)
                    {
                        if (studyItem.Modified)
                            return true;
                    }
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_LanguageMediaItems != null)
                {
                    foreach (LanguageMediaItem languageMediaItem in _LanguageMediaItems)
                        languageMediaItem.Modified = false;
                }

                if (_LocalSpeakerNames != null)
                {
                    foreach (MultiLanguageString speakerName in _LocalSpeakerNames)
                        speakerName.Modified = false;
                }

                if (_LocalStudyItems != null)
                {
                    foreach (MultiLanguageItem studyItem in _LocalStudyItems)
                        studyItem.Modified = false;
                }
            }
        }

        public LanguageMediaItem GetLanguageMediaItem(object key)
        {
            if ((_LanguageMediaItems != null) && (key != null))
                return _LanguageMediaItems.FirstOrDefault(x => x.MatchKey(key));

            return null;
        }

        public LanguageMediaItem GetLanguageMediaItemIndexed(int index)
        {
            if ((_LanguageMediaItems != null) && (index >= 0) && (index < _LanguageMediaItems.Count()))
                return _LanguageMediaItems.ElementAt(index);

            return null;
        }

        public LanguageMediaItem GetLanguageMediaItemWithLanguages(
            LanguageID targetLanguageID, LanguageID hostLanguageID)
        {
            LanguageMediaItem languageMediaItem;
            int count;
            int index;

            if ((_LanguageMediaItems != null) && ((count = _LanguageMediaItems.Count()) != 0))
            {
                for (index = 0; index < count; index++)
                {
                    languageMediaItem = _LanguageMediaItems[index];

                    if (languageMediaItem.HasTargetLanguage(targetLanguageID)
                        && languageMediaItem.HasHostLanguage(hostLanguageID))
                    {
                        return languageMediaItem;
                    }
                }
                for (index = 0; index < count; index++)
                {
                    languageMediaItem = _LanguageMediaItems[index];

                    if (languageMediaItem.HasTargetLanguage(targetLanguageID)
                        && languageMediaItem.HasHostLanguage(null))
                    {
                        return languageMediaItem;
                    }
                }
            }

            return null;
        }

        public LanguageMediaItem GetLanguageMediaItemWithMediaLanguages(
            LanguageID targetMediaLanguageID, LanguageID hostMediaLanguageID)
        {
            LanguageMediaItem languageMediaItem;
            int count;
            int index;

            if ((_LanguageMediaItems != null) && ((count = _LanguageMediaItems.Count()) != 0))
            {
                for (index = 0; index < count; index++)
                {
                    languageMediaItem = _LanguageMediaItems[index];

                    LanguageID theTargetMediaLanguageID = languageMediaItem.TargetMediaLanguageID;
                    LanguageID theHostMediaLanguageID = languageMediaItem.HostMediaLanguageID;

                    if ((targetMediaLanguageID == theTargetMediaLanguageID)
                        && (hostMediaLanguageID == theHostMediaLanguageID))
                    {
                        return languageMediaItem;
                    }
                }
                for (index = 0; index < count; index++)
                {
                    languageMediaItem = _LanguageMediaItems[index];

                    LanguageID theTargetMediaLanguageID = languageMediaItem.TargetMediaLanguageID;
                    LanguageID theHostMediaLanguageID = languageMediaItem.HostMediaLanguageID;

                    if ((targetMediaLanguageID == theTargetMediaLanguageID)
                        && (theHostMediaLanguageID == null))
                    {
                        return languageMediaItem;
                    }
                }
            }

            return null;
        }

        public List<LanguageMediaItem> GetLanguageMediaItemsWithLanguages(
            List<LanguageID> targetLanguageIDs, List<LanguageID> hostLanguageIDs)
        {
            List<LanguageMediaItem> languageMediaItems = new List<LanguageMediaItem>();
            LanguageMediaItem languageMediaItem;
            int count;
            int index;

            if ((_LanguageMediaItems != null) && ((count = _LanguageMediaItems.Count()) != 0))
            {
                if ((targetLanguageIDs != null) && (targetLanguageIDs.Count() != 0) &&
                    (hostLanguageIDs != null) && (hostLanguageIDs.Count() != 0))
                {
                    for (index = 0; index < count; index++)
                    {
                        bool done = false;

                        languageMediaItem = _LanguageMediaItems[index];

                        foreach (LanguageID targetLanguageID in targetLanguageIDs)
                        {
                            if (done)
                                break;

                            foreach (LanguageID hostLanguageID in hostLanguageIDs)
                            {
                                if (languageMediaItem.HasTargetLanguage(targetLanguageID)
                                    && languageMediaItem.HasHostLanguage(hostLanguageID))
                                {
                                    languageMediaItems.Add(languageMediaItem);
                                    done = true;
                                    break;
                                }
                                else if (languageMediaItem.HasTargetLanguage(targetLanguageID)
                                    && languageMediaItem.HasHostLanguage(null))
                                {
                                    languageMediaItems.Add(languageMediaItem);
                                    done = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                else if ((targetLanguageIDs != null) && (targetLanguageIDs.Count() != 0) &&
                    ((hostLanguageIDs == null) || (hostLanguageIDs.Count() == 0)))
                {
                    for (index = 0; index < count; index++)
                    {
                        languageMediaItem = _LanguageMediaItems[index];

                        foreach (LanguageID targetLanguageID in targetLanguageIDs)
                        {
                            if (languageMediaItem.HasTargetLanguage(targetLanguageID))
                            {
                                languageMediaItems.Add(languageMediaItem);
                                break;
                            }
                        }
                    }
                }
                else if (((targetLanguageIDs == null) || (targetLanguageIDs.Count() == 0)) &&
                    (hostLanguageIDs != null) && (hostLanguageIDs.Count() != 0))
                {
                    for (index = 0; index < count; index++)
                    {
                        languageMediaItem = _LanguageMediaItems[index];

                        foreach (LanguageID hostLanguageID in hostLanguageIDs)
                        {
                            if (languageMediaItem.HasHostLanguage(hostLanguageID))
                            {
                                languageMediaItems.Add(languageMediaItem);
                                break;
                            }
                        }
                    }
                }
            }

            return languageMediaItems;
        }

        public MediaDescription GetMediaDescriptionIndexed(
            string languageMediaItemKey, int index)
        {
            LanguageMediaItem languageMediaItem = GetLanguageMediaItem(languageMediaItemKey);
            MediaDescription mediaDescription = null;

            if (languageMediaItem != null)
                mediaDescription = languageMediaItem.GetMediaDescriptionIndexed(index);

            return mediaDescription;
        }

        public MediaDescription GetMediaDescriptionWithLanguagesIndexed(
            LanguageID targetLanguageID, LanguageID hostLanguageID, int index)
        {
            LanguageMediaItem languageMediaItem = GetLanguageMediaItemWithLanguages(
                targetLanguageID, hostLanguageID);
            MediaDescription mediaDescription = null;

            if (languageMediaItem != null)
                mediaDescription = languageMediaItem.GetMediaDescriptionIndexed(index);

            return mediaDescription;
        }

        public MediaDescription GetMediaDescriptionWithMediaLanguagesIndexed(
            LanguageID targetMediaLanguageID, LanguageID hostMediaLanguageID, int index)
        {
            LanguageMediaItem languageMediaItem = GetLanguageMediaItemWithMediaLanguages(
                targetMediaLanguageID, hostMediaLanguageID);
            MediaDescription mediaDescription = null;

            if (languageMediaItem != null)
                mediaDescription = languageMediaItem.GetMediaDescriptionIndexed(index);

            return mediaDescription;
        }

        public MediaDescription GetMediaDescriptionWithLanguagesMimeType(
            LanguageID targetLanguageID, LanguageID hostLanguageID, string mimeType)
        {
            LanguageMediaItem languageMediaItem = GetLanguageMediaItemWithLanguages(
                targetLanguageID, hostLanguageID);
            MediaDescription mediaDescription = null;

            if (languageMediaItem != null)
                mediaDescription = languageMediaItem.GetMediaDescriptionWithMimeType(mimeType);

            return mediaDescription;
        }

        public bool AddLanguageMediaItem(LanguageMediaItem languageMediaItem)
        {
            if (languageMediaItem == null)
                return false;

            languageMediaItem.MediaItem = this;

            if (_LanguageMediaItems == null)
                _LanguageMediaItems = new List<LanguageMediaItem>(1) { languageMediaItem };
            else
                _LanguageMediaItems.Add(languageMediaItem);

            ModifiedFlag = true;

            return true;
        }

        public bool InsertLanguageMediaItemIndexed(int index, LanguageMediaItem languageMediaItem)
        {
            if (languageMediaItem == null)
                return false;

            languageMediaItem.MediaItem = this;

            if (_LanguageMediaItems == null)
                _LanguageMediaItems = new List<LanguageMediaItem>(1) { languageMediaItem };
            else if (index < _LanguageMediaItems.Count())
                _LanguageMediaItems.Insert(index, languageMediaItem);
            else
                _LanguageMediaItems.Add(languageMediaItem);

            ModifiedFlag = true;

            return true;
        }

        public bool DeleteLanguageMediaItem(LanguageMediaItem languageMediaItem)
        {
            if (_LanguageMediaItems != null)
            {
                if (_LanguageMediaItems.Remove(languageMediaItem))
                {
                    ModifiedFlag = true;
                    return true;
                }
            }

            return false;
        }

        public bool DeleteLanguageMediaItemKey(object key)
        {
            if ((_LanguageMediaItems != null) && (key != null))
            {
                LanguageMediaItem languageMediaItem = GetLanguageMediaItem(key);

                if (languageMediaItem != null)
                {
                    _LanguageMediaItems.Remove(languageMediaItem);
                    ModifiedFlag = true;
                    return true;
                }
            }

            return false;
        }

        public bool DeleteLanguageMediaItemWithLanguages(
            LanguageID targetMediaLanguageID, LanguageID hostMediaLanguageID)
        {
            LanguageMediaItem languageMediaItem;
            bool returnValue = true;

            if (_LanguageMediaItems != null)
            {
                while ((languageMediaItem = GetLanguageMediaItemWithLanguages(
                        targetMediaLanguageID, hostMediaLanguageID)) != null)
                {
                    returnValue = DeleteLanguageMediaItem(languageMediaItem);
                }
            }

            return returnValue;
        }

        public bool DeleteLanguageMediaItemIndexed(int index)
        {
            if ((_LanguageMediaItems != null) && (index >= 0) && (index < _LanguageMediaItems.Count()))
            {
                _LanguageMediaItems.RemoveAt(index);
                ModifiedFlag = true;
                return true;
            }
            return false;
        }

        public void DeleteAllLanguageMediaItems()
        {
            if (_LanguageMediaItems != null)
            {
                if (_LanguageMediaItems.Count() != 0)
                    ModifiedFlag = true;

                _LanguageMediaItems.Clear();
            }
        }

        public int LanguageMediaItemCount()
        {
            if (_LanguageMediaItems != null)
                return (_LanguageMediaItems.Count());

            return 0;
        }

        public List<LanguageMediaItem> CloneLanguageMediaItems()
        {
            if (_LanguageMediaItems == null)
                return null;

            List<LanguageMediaItem> returnValue = new List<LanguageMediaItem>(_LanguageMediaItems.Count());

            foreach (LanguageMediaItem languageMediaItem in _LanguageMediaItems)
                returnValue.Add(new LanguageMediaItem(languageMediaItem));

            return returnValue;
        }

        public List<LanguageID> MediaTargetLanguageIDs
        {
            get
            {
                List<LanguageID> languageIDs = new List<LanguageID>();
                if (_LanguageMediaItems != null)
                {
                    foreach (LanguageMediaItem languageMediaItem in _LanguageMediaItems)
                    {
                        List<LanguageID> mediaLanguageIDs = languageMediaItem.MediaTargetLanguageIDs;
                        foreach (LanguageID mediaLanguageID in mediaLanguageIDs)
                        {
                            if (!languageIDs.Contains(mediaLanguageID))
                                languageIDs.Add(mediaLanguageID);
                        }
                    }
                }
                return languageIDs;
            }
        }

        public List<LanguageID> MediaHostLanguageIDs
        {
            get
            {
                List<LanguageID> languageIDs = new List<LanguageID>();
                if (_LanguageMediaItems != null)
                {
                    foreach (LanguageMediaItem languageMediaItem in _LanguageMediaItems)
                    {
                        List<LanguageID> mediaLanguageIDs = languageMediaItem.MediaHostLanguageIDs;
                        foreach (LanguageID mediaLanguageID in mediaLanguageIDs)
                        {
                            if (!languageIDs.Contains(mediaLanguageID))
                                languageIDs.Add(mediaLanguageID);
                        }
                    }
                }
                return languageIDs;
            }
        }

        public void CollectMediaFiles(Dictionary<string, bool> languageSelectFlags,
            List<string> mediaFiles, VisitMedia visitFunction)
        {
            if (_LanguageMediaItems != null)
            {
                if ((_Content != null) && _Content.GetOptionFlag("DescendentMediaPlaceholder", false))
                    return;

                foreach (LanguageMediaItem languageMediaItem in _LanguageMediaItems)
                {
                    bool anyCollectIt = true;

                    if (languageSelectFlags != null)
                    {
                        bool targetCollectIt = true;
                        bool hostCollectIt = true;

                        if (languageMediaItem.TargetLanguageIDs != null)
                        {
                            int targetCollectItCount = 0;

                            foreach (LanguageID languageID in languageMediaItem.TargetLanguageIDs)
                            {
                                bool collectIt = true;

                                if (!languageSelectFlags.TryGetValue(
                                        languageID.LanguageCultureExtensionCode,
                                        out collectIt))
                                    collectIt = false;

                                if (collectIt)
                                    targetCollectItCount++;
                            }

                            if (targetCollectItCount == 0)
                                targetCollectIt = false;
                        }

                        if (languageMediaItem.HostLanguageIDs != null)
                        {
                            int hostCollectItCount = 0;

                            foreach (LanguageID languageID in languageMediaItem.HostLanguageIDs)
                            {
                                bool collectIt = true;

                                if (!languageSelectFlags.TryGetValue(
                                        languageID.LanguageCultureExtensionCode,
                                        out collectIt))
                                    collectIt = false;

                                if (collectIt)
                                    hostCollectItCount++;
                            }

                            if (hostCollectItCount == 0)
                                hostCollectIt = false;
                        }

                        if (!targetCollectIt || !hostCollectIt)
                            anyCollectIt = false;
                    }

                    if (anyCollectIt)
                        languageMediaItem.CollectMediaFiles(mediaFiles, visitFunction);
                }
            }
        }

        public override void CollectReferences(List<IBaseObjectKeyed> references, List<IBaseObjectKeyed> externalSavedChildren,
            List<IBaseObjectKeyed> externalNonSavedChildren, Dictionary<int, bool> intSelectFlags,
            Dictionary<string, bool> stringSelectFlags, Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles, VisitMedia visitFunction)
        {
            CollectMediaFiles(languageSelectFlags, mediaFiles, visitFunction);
        }

        public override bool CopyMedia(string targetDirectoryRoot, List<string> copiedFiles, ref string errorMessage)
        {
            bool returnValue = true;

            if (_LanguageMediaItems != null)
            {
                foreach (LanguageMediaItem languageMediaItem in _LanguageMediaItems)
                {
                    if (!languageMediaItem.CopyMedia(targetDirectoryRoot, copiedFiles, ref errorMessage))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public static string GetDefaultFileSuffix(string contentType)
        {
            string defaultSuffix = LanguageMediaItem.GetDefaultFileSuffix(contentType);
            return defaultSuffix;
        }

        public static List<OptionDescriptor> GetDefaultDescriptors(string contentType, string contentSubType,
            UserProfile userProfile)
        {
            List<OptionDescriptor> newOptionDescriptors = null;
            string defaultSourceComponentKeys;
            string otherTeachersCanEdit = "Inherit";

            switch (contentSubType)
            {
                case "Introduction":
                case "Summary":
                case "Grammar":
                case "Culture":
                case "Lesson":
                    defaultSourceComponentKeys = "Text, Words";
                    break;
                case "Dialog":
                    defaultSourceComponentKeys = "Text";
                    break;
                case "Review":
                    defaultSourceComponentKeys = "Text, Words";
                    break;
                case "Text":
                    defaultSourceComponentKeys = "Text";
                    break;
                case "Vocabulary":
                    defaultSourceComponentKeys = "Words";
                    break;
                case "Characters":
                    defaultSourceComponentKeys = "Characters";
                    break;
                case "Expansion":
                    defaultSourceComponentKeys = "Expansion";
                    break;
                case "Exercises":
                    defaultSourceComponentKeys = "Exercises";
                    break;
                case "Notes":
                    defaultSourceComponentKeys = "Notes";
                    break;
                case "NotesLite":
                    defaultSourceComponentKeys = "NotesLite";
                    break;
                case "List":
                case "Comments":
                default:
                    defaultSourceComponentKeys = "Text";
                    break;
            }

            switch (contentType)
            {
                case "Document":
                    break;
                case "Audio":
                    newOptionDescriptors = new List<OptionDescriptor>()
                    {
                        new OptionDescriptor("DescendentMediaPlaceholder", "flag", "Descendent media placeholder",
                            "This option indicates that this media item is a placeholder for media items of the same type in descendents of a group or course.", "false"),
                        new OptionDescriptor("SourceComponentKeys", "textcomponentset", "Source component keys",
                            "This option determines the default source component keys.", defaultSourceComponentKeys),
                        new OptionDescriptor("GenerateFileName", "flag", "Generate file name",
                            "This option determines whether a default file name will be generated.", "true"),
                        new OptionDescriptor("FilePrefix", "string", "FilePrefix",
                            "This option determines the default file prefix.", ""),
                        new OptionDescriptor("FileSuffix", "string", "FileSuffix",
                            "This option determines the default file suffix.", GetDefaultFileSuffix(contentType)),
                        new OptionDescriptor("LanguageSuffix", "flag", "LanguageSuffix",
                            "This option determines whether the generated file name includes a language suffix.", "true"),
                        new OptionDescriptor("TranscriptComponentKey", "string", "Transcript component key",
                            "This option determines the transcript component key for generated media.", ""),
                        new OptionDescriptor("WarnIfEmpty", "flag", "Warn if empty",
                            "This option indicates that when displaying the content in a content list, don't display a warning if the content is empty.", "true"),
                        new OptionDescriptor("DisableStatistics", "flag", "Disable statistics",
                            "This option indicates that this content item should not calculate statistics if true.", "false"),
                        new OptionDescriptor("HideStatisticsFromParent", "flag", "Hide statistics from parent",
                            "This option indicates that this content item should hide statistics from parents if true.", "false"),
                        CreateEditPermissionOptionDescriptor(otherTeachersCanEdit),
                        new OptionDescriptor("PlayerSource", "stringset", "Player source",
                            "This option determines where the media comes from.", "File",
                            PlayerSources),
                        new OptionDescriptor("PlayerType", "stringset", "Player type",
                            "This option determines the type of media player.", "Full",
                            PlayerTypes),
                        new OptionDescriptor("AutoPlay", "flag", "Autoplay",
                            "This option indicates that this media item should begin playing automatically when the content is displayed.", "true")
                    };
                    break;
                case "Video":
                    newOptionDescriptors = new List<OptionDescriptor>()
                    {
                        new OptionDescriptor("DescendentMediaPlaceholder", "flag", "Descendent media placeholder",
                            "This option indicates that this media item is a placeholder for media items of the same type in descendents of a group or course.", "false"),
                        new OptionDescriptor("SourceComponentKeys", "textcomponentset", "Source component keys",
                            "This option determines the default source component keys.", defaultSourceComponentKeys),
                        new OptionDescriptor("GenerateFileName", "flag", "Generate file name",
                            "This option determines whether a default file name will be generated.", "true"),
                        new OptionDescriptor("FilePrefix", "string", "FilePrefix",
                            "This option determines the default file prefix.", ""),
                        new OptionDescriptor("FileSuffix", "string", "FileSuffix",
                            "This option determines the default file suffix.", GetDefaultFileSuffix(contentType)),
                        new OptionDescriptor("LanguageSuffix", "flag", "LanguageSuffix",
                            "This option determines whether the generated file name includes a language suffix.", "true"),
                        new OptionDescriptor("TranscriptComponentKey", "string", "Transcript component key",
                            "This option determines the transcript component key for generated media.", ""),
                        new OptionDescriptor("WarnIfEmpty", "flag", "Warn if empty",
                            "This option indicates that when displaying the content in a content list, don't display a warning if the content is empty.", "true"),
                        CreateEditPermissionOptionDescriptor(otherTeachersCanEdit),
                        new OptionDescriptor("PlayerSource", "stringset", "Player source",
                            "This option determines where the media comes from.", "File",
                            PlayerSources),
                        new OptionDescriptor("PlayerType", "stringset", "Player type",
                            "This option determines the type of media player.", "Full",
                            PlayerTypes),
                        new OptionDescriptor("AutoPlay", "flag", "Autoplay",
                            "This option indicates that this media item should begin playing automatically when the content is displayed.", "true"),
                        new OptionDescriptor("DisplayWidth", "string", "Display width",
                            "This option is for specifying the width for displaying the content."
                                + " Specify \"auto\" to use a default width, or \"native\" to use the content's native width."
                                + " Otherwise specify a width number in pixels.", "auto"),
                        new OptionDescriptor("DisplayHeight", "string", "Display height",
                            "This option is for specifying the width for displaying the content."
                                + " Specify \"auto\" to use a default height, or \"native\" to use the content's native height."
                                + " Otherwise specify a height number in pixels.", "auto")
                    };
                    break;
                case "Automated":
                    newOptionDescriptors = new List<OptionDescriptor>()
                    {
                        new OptionDescriptor("DescendentMediaPlaceholder", "flag", "Descendent media placeholder",
                            "This option indicates that this media item is a placeholder for media items of the same type in descendents of a group or course.", "false"),
                        new OptionDescriptor("SourceComponentKeys", "textcomponentset", "Source component keys",
                            "This option determines the default source component keys.", defaultSourceComponentKeys),
                        CreateEditPermissionOptionDescriptor(otherTeachersCanEdit),
                        new OptionDescriptor("AutoPlay", "flag", "Autoplay",
                            "This option indicates that this media item should begin playing automatically when the content is displayed.", "false")
                    };
                    break;
                case "Image":
                    newOptionDescriptors = new List<OptionDescriptor>()
                    {
                        new OptionDescriptor("DescendentMediaPlaceholder", "flag", "Descendent media placeholder",
                            "This option indicates that this media item is a placeholder for media items of the same type in descendents of a group or course.", "false"),
                        new OptionDescriptor("GenerateFileName", "flag", "Generate file name",
                            "This option determines whether a default file name will be generated.", "true"),
                        new OptionDescriptor("FilePrefix", "string", "FilePrefix",
                            "This option determines the default file prefix.", ""),
                        new OptionDescriptor("FileSuffix", "string", "FileSuffix",
                            "This option determines the default file suffix.", GetDefaultFileSuffix(contentType)),
                        new OptionDescriptor("LanguageSuffix", "flag", "LanguageSuffix",
                            "This option determines whether the generated file name includes a language suffix.", "false"),
                        new OptionDescriptor("DisplayWidth", "string", "Display width",
                            "This option is for specifying the width for displaying the content."
                                + " Specify \"auto\" to use a default width, or \"native\" to use the content's native width."
                                + " Otherwise specify a width number in pixels.", "native"),
                        new OptionDescriptor("DisplayHeight", "string", "Display height",
                            "This option is for specifying the width for displaying the content."
                                + " Specify \"auto\" to use a default height, or \"native\" to use the content's native height."
                                + " Otherwise specify a height number in pixels.", "native"),
                        CreateEditPermissionOptionDescriptor(otherTeachersCanEdit),
                    };
                    break;
                case "TextFile":
                case "PDF":
                    newOptionDescriptors = new List<OptionDescriptor>()
                    {
                        new OptionDescriptor("DescendentMediaPlaceholder", "flag", "Descendent media placeholder",
                            "This option indicates that this media item is a placeholder for media items of the same type in descendents of a group or course.", "false"),
                        new OptionDescriptor("GenerateFileName", "flag", "Generate file name",
                            "This option determines whether a default file name will be generated.", "true"),
                        new OptionDescriptor("FilePrefix", "string", "FilePrefix",
                            "This option determines the default file prefix.", ""),
                        new OptionDescriptor("FileSuffix", "string", "FileSuffix",
                            "This option determines the default file suffix.", GetDefaultFileSuffix(contentType)),
                        new OptionDescriptor("LanguageSuffix", "flag", "LanguageSuffix",
                            "This option determines whether the generated file name includes a language suffix.", "true"),
                        new OptionDescriptor("DisplayWidth", "string", "Display width",
                            "This option is for specifying the width for displaying the content."
                                + " Specify \"auto\" to use a default width, or \"native\" to use the content's native width."
                                + " Otherwise specify a width number in pixels.", "auto"),
                        new OptionDescriptor("DisplayHeight", "string", "Display height",
                            "This option is for specifying the width for displaying the content."
                                + " Specify \"auto\" to use a default height, or \"native\" to use the content's native height."
                                + " Otherwise specify a height number in pixels.", "auto"),
                        CreateEditPermissionOptionDescriptor(otherTeachersCanEdit),
                    };
                    break;
                case "Embedded":
                    newOptionDescriptors = new List<OptionDescriptor>()
                    {
                        new OptionDescriptor("DescendentMediaPlaceholder", "flag", "Descendent media placeholder",
                            "This option indicates that this media item is a placeholder for media items of the same type in descendents of a group or course.", "false"),
                        new OptionDescriptor("DisplayWidth", "string", "Display width",
                            "This option is for specifying the width for displaying the content."
                                + " Specify \"auto\" to use a default width, or \"native\" to use the content's native width."
                                + " Otherwise specify a width number in pixels.", "auto"),
                        new OptionDescriptor("DisplayHeight", "string", "Display height",
                            "This option is for specifying the width for displaying the content."
                                + " Specify \"auto\" to use a default height, or \"native\" to use the content's native height."
                                + " Otherwise specify a height number in pixels.", "auto"),
                        CreateEditPermissionOptionDescriptor(otherTeachersCanEdit),
                    };
                    break;
                case "Media":
                case "Transcript":
                case "Text":
                case "Sentences":
                case "Words":
                case "Characters":
                case "Expansion":
                case "Exercises":
                case "Notes":
                case "Comments":
                default:
                    throw new Exception("ContentMediaItem.GetDefaultDescriptors: Unknown content type: " + contentType);
            }

            return newOptionDescriptors;
        }

        public override void SetupOptions(MasterContentItem contentItem)
        {
            base.SetupOptions(contentItem);

            if (Content != null)
            {
                SourceContentKeys = Content.GetOptionStringList("SourceComponentKeys");

                if (Content.GetOptionFlag("GenerateFileName", true) &&
                    String.IsNullOrEmpty(MediaFileNamePattern))
                {
                    string filePrefix = Content.GetOptionString("FilePrefix", "");
                    string fileSuffix = Content.GetOptionString("FileSuffix", GetDefaultFileExtension());
                    string contentSubTypeSuffix = "-" + ContentSubType.ToLower();
                    bool addLanguageSuffix = Content.GetOptionFlag("LanguageSuffix", false);
                    string languageSuffixPattern = "";
                    if (addLanguageSuffix)
                    {
                        if (contentItem.FirstTargetLanguageID != null)
                            languageSuffixPattern += "%t";
                        if (contentItem.FirstHostLanguageID != null)
                            languageSuffixPattern += "%h";
                    }
                    string mediaFileNamePattern = filePrefix +
                        MediaUtilities.FileFriendlyName(Node.GetTitleString()) +
                        contentSubTypeSuffix +
                        (addLanguageSuffix ? "-" + languageSuffixPattern : "") +
                        fileSuffix;
                    MediaFileNamePattern = mediaFileNamePattern;
                }

                PlayerSource = Content.GetOptionString("PlayerSource", PlayerSource);
                PlayerType = Content.GetOptionString("PlayerType", PlayerType);

                GenerateMediaDescriptions();

                DisplayWidth = contentItem.GetOptionValue("DisplayWidth");
                DisplayHeight = contentItem.GetOptionValue("DisplayHeight");
            }
        }

        public string GetMediaFileNamePattern()
        {
            string filePrefix = Content.GetOptionString("FilePrefix", "");
            string fileSuffix = Content.GetOptionString("FileSuffix", GetDefaultFileExtension());
            string contentSubTypeSuffix = "-" + ContentSubType.ToLower();
            bool addLanguageSuffix = Content.GetOptionFlag("LanguageSuffix", false);
            string languageSuffixPattern = "";
            if (addLanguageSuffix)
            {
                if (Content.FirstTargetLanguageID != null)
                    languageSuffixPattern += "%t";
                if (Content.FirstHostLanguageID != null)
                    languageSuffixPattern += "%h";
            }
            string mediaFileNamePattern = filePrefix +
                MediaUtilities.FileFriendlyName(Node.GetTitleString()) +
                contentSubTypeSuffix +
                (addLanguageSuffix ? "-" + languageSuffixPattern : "") +
                fileSuffix;
            return mediaFileNamePattern;
        }

        public string ComposeMediaFileName(LanguageID targetLanguageID, LanguageID hostLanguageID,
            LanguageID uiLanguageID, string alternateSuffix, string fileExt)
        {
            string filePrefix = Content.GetOptionString("FilePrefix", "");
            string contentSubTypeSuffix = "-" + ContentSubType.ToLower();
            bool addLanguageSuffix = Content.GetOptionFlag("LanguageSuffix", false);
            string languageSuffixPattern = "";
            if (addLanguageSuffix)
            {
                if (targetLanguageID != null)
                    languageSuffixPattern += targetLanguageID.LanguageCode;
                if (Content.FirstHostLanguageID != null)
                    languageSuffixPattern += hostLanguageID.LanguageCode;
            }
            string mediaFileNamePattern = filePrefix +
                MediaUtilities.FileFriendlyName(Node.GetTitleString()) +
                contentSubTypeSuffix +
                (addLanguageSuffix ? "-" + languageSuffixPattern : "") +
                alternateSuffix +
                fileExt;
            return mediaFileNamePattern;
        }

        public void GenerateMediaDescriptions()
        {
            BaseObjectContent content = Content;

            if (content == null)
                return;

            List<LanguageID> targetLanguageIDs = content.TargetLanguageIDs;
            List<LanguageID> hostLanguageIDs = content.HostLanguageIDs;
            Dictionary<LanguageID, List<LanguageID>> targetMediaLanguageIDDictionary =
                LanguageID.GetMediaLanguageIDDictionary(targetLanguageIDs);
            Dictionary<LanguageID, List<LanguageID>> hostMediaLanguageIDDictionary =
                LanguageID.GetMediaLanguageIDDictionary(hostLanguageIDs);
            LanguageID currentTargetMediaLanguageID;
            LanguageID currentHostMediaLanguageID;
            List<LanguageID> currentTargetLanguageIDs;
            List<LanguageID> currentHostLanguageIDs;

            if (targetMediaLanguageIDDictionary.Count() == 0)
            {
                if (hostMediaLanguageIDDictionary.Count != 0)
                {
                    foreach (KeyValuePair<LanguageID, List<LanguageID>> hostKVP in hostMediaLanguageIDDictionary)
                    {
                        currentHostMediaLanguageID = hostKVP.Key;
                        currentHostLanguageIDs = hostKVP.Value;

                        LanguageMediaItem languageMediaItem = GetLanguageMediaItemWithMediaLanguages(null, currentHostMediaLanguageID);

                        if (languageMediaItem == null)
                        {
                            string languageMediaItemKey = LanguageMediaItem.ComposeLanguageMediaItemKey(null, currentHostMediaLanguageID);
                            languageMediaItem = new LanguageMediaItem(
                                languageMediaItemKey,
                                null,
                                currentHostLanguageIDs,
                                Owner);
                            AddLanguageMediaItem(languageMediaItem);
                        }
                        else
                        {
                            if (LanguageID.CompareLanguageIDLists(languageMediaItem.TargetLanguageIDs, null) != 0)
                                languageMediaItem.TargetLanguageIDs = null;
                            if (LanguageID.CompareLanguageIDLists(languageMediaItem.HostLanguageIDs, currentHostLanguageIDs) != 0)
                                languageMediaItem.HostLanguageIDs = currentHostLanguageIDs;
                        }

                        languageMediaItem.GenerateMediaDescriptions(MediaFileNamePattern);
                    }
                }
                else
                {
                    LanguageMediaItem languageMediaItem = GetLanguageMediaItemWithMediaLanguages(null, null);

                    if (languageMediaItem == null)
                    {
                        string languageMediaItemKey = LanguageMediaItem.ComposeLanguageMediaItemKey((LanguageID)null, (LanguageID)null);
                        languageMediaItem = new LanguageMediaItem(
                            languageMediaItemKey,
                            null,
                            null,
                            Owner);
                        AddLanguageMediaItem(languageMediaItem);
                    }
                    else
                    {
                        if (LanguageID.CompareLanguageIDLists(languageMediaItem.TargetLanguageIDs, null) != 0)
                            languageMediaItem.TargetLanguageIDs = null;
                        if (LanguageID.CompareLanguageIDLists(languageMediaItem.HostLanguageIDs, null) != 0)
                            languageMediaItem.HostLanguageIDs = null;
                    }

                    languageMediaItem.GenerateMediaDescriptions(MediaFileNamePattern);
                }
            }
            else
            {
                foreach (KeyValuePair<LanguageID, List<LanguageID>> targetKVP in targetMediaLanguageIDDictionary)
                {
                    currentTargetMediaLanguageID = targetKVP.Key;
                    currentTargetLanguageIDs = targetKVP.Value;

                    if (hostMediaLanguageIDDictionary.Count != 0)
                    {
                        foreach (KeyValuePair<LanguageID, List<LanguageID>> hostKVP in hostMediaLanguageIDDictionary)
                        {
                            currentHostMediaLanguageID = hostKVP.Key;
                            currentHostLanguageIDs = hostKVP.Value;

                            LanguageMediaItem languageMediaItem = GetLanguageMediaItemWithMediaLanguages(currentTargetMediaLanguageID, currentHostMediaLanguageID);

                            if (languageMediaItem == null)
                            {
                                string languageMediaItemKey = LanguageMediaItem.ComposeLanguageMediaItemKey(currentTargetMediaLanguageID, currentHostMediaLanguageID);
                                languageMediaItem = new LanguageMediaItem(
                                    languageMediaItemKey,
                                    currentTargetLanguageIDs,
                                    currentHostLanguageIDs,
                                    Owner);
                                AddLanguageMediaItem(languageMediaItem);
                            }
                            else
                            {
                                if (LanguageID.CompareLanguageIDLists(languageMediaItem.TargetLanguageIDs, currentTargetLanguageIDs) != 0)
                                    languageMediaItem.TargetLanguageIDs = currentTargetLanguageIDs;
                                if (LanguageID.CompareLanguageIDLists(languageMediaItem.HostLanguageIDs, currentHostLanguageIDs) != 0)
                                    languageMediaItem.HostLanguageIDs = currentHostLanguageIDs;
                            }

                            languageMediaItem.GenerateMediaDescriptions(MediaFileNamePattern);
                        }
                    }
                    else
                    {
                        LanguageMediaItem languageMediaItem = GetLanguageMediaItemWithMediaLanguages(currentTargetMediaLanguageID, null);

                        if (languageMediaItem == null)
                        {
                            string languageMediaItemKey = LanguageMediaItem.ComposeLanguageMediaItemKey(currentTargetMediaLanguageID, null);
                            languageMediaItem = new LanguageMediaItem(
                                languageMediaItemKey,
                                currentTargetLanguageIDs,
                                null,
                                Owner);
                            AddLanguageMediaItem(languageMediaItem);
                        }
                        else
                        {
                            if (LanguageID.CompareLanguageIDLists(languageMediaItem.TargetLanguageIDs, currentTargetLanguageIDs) != 0)
                                languageMediaItem.TargetLanguageIDs = currentTargetLanguageIDs;
                            if (LanguageID.CompareLanguageIDLists(languageMediaItem.HostLanguageIDs, null) != 0)
                                languageMediaItem.HostLanguageIDs = null;
                        }

                        languageMediaItem.GenerateMediaDescriptions(MediaFileNamePattern);
                    }
                }
            }

            RemoveUnusedLanguageMediaItems();
        }

        public void SetupMediaDescriptions(string baseFileName, string fileExtension, bool addLanguageSuffix)
        {
            BaseObjectContent content = Content;

            if (content == null)
                return;

            List<LanguageID> targetLanguageIDs = content.TargetLanguageIDs;
            List<LanguageID> hostLanguageIDs = content.HostLanguageIDs;
            Dictionary<LanguageID, List<LanguageID>> targetMediaLanguageIDDictionary =
                LanguageID.GetMediaLanguageIDDictionary(targetLanguageIDs);
            Dictionary<LanguageID, List<LanguageID>> hostMediaLanguageIDDictionary =
                LanguageID.GetMediaLanguageIDDictionary(hostLanguageIDs);
            LanguageID currentTargetMediaLanguageID;
            LanguageID currentHostMediaLanguageID;
            List<LanguageID> currentTargetLanguageIDs;
            List<LanguageID> currentHostLanguageIDs;

            foreach (KeyValuePair<LanguageID, List<LanguageID>> targetKVP in targetMediaLanguageIDDictionary)
            {
                currentTargetMediaLanguageID = targetKVP.Key;
                currentTargetLanguageIDs = targetKVP.Value;

                if (hostMediaLanguageIDDictionary.Count != 0)
                {
                    foreach (KeyValuePair<LanguageID, List<LanguageID>> hostKVP in hostMediaLanguageIDDictionary)
                    {
                        currentHostMediaLanguageID = hostKVP.Key;
                        currentHostLanguageIDs = hostKVP.Value;

                        LanguageMediaItem languageMediaItem = GetLanguageMediaItemWithMediaLanguages(currentTargetMediaLanguageID, currentHostMediaLanguageID);

                        if (languageMediaItem == null)
                        {
                            string languageMediaItemKey = LanguageMediaItem.ComposeLanguageMediaItemKey(currentTargetMediaLanguageID, currentHostMediaLanguageID);
                            languageMediaItem = new LanguageMediaItem(
                                languageMediaItemKey,
                                currentTargetLanguageIDs,
                                currentHostLanguageIDs,
                                Owner);
                            AddLanguageMediaItem(languageMediaItem);
                        }
                        else
                        {
                            if (LanguageID.CompareLanguageIDLists(languageMediaItem.TargetLanguageIDs, currentTargetLanguageIDs) != 0)
                                languageMediaItem.TargetLanguageIDs = currentTargetLanguageIDs;
                            if (LanguageID.CompareLanguageIDLists(languageMediaItem.HostLanguageIDs, currentHostLanguageIDs) != 0)
                                languageMediaItem.HostLanguageIDs = currentHostLanguageIDs;
                        }

                        languageMediaItem.SetupMediaDescriptions(baseFileName, fileExtension, addLanguageSuffix);
                    }
                }
                else
                {
                    LanguageMediaItem languageMediaItem = GetLanguageMediaItemWithMediaLanguages(currentTargetMediaLanguageID, null);

                    if (languageMediaItem == null)
                    {
                        string languageMediaItemKey = LanguageMediaItem.ComposeLanguageMediaItemKey(currentTargetMediaLanguageID, null);
                        languageMediaItem = new LanguageMediaItem(
                            languageMediaItemKey,
                            currentTargetLanguageIDs,
                            null,
                            Owner);
                        AddLanguageMediaItem(languageMediaItem);
                    }
                    else
                    {
                        if (LanguageID.CompareLanguageIDLists(languageMediaItem.TargetLanguageIDs, currentTargetLanguageIDs) != 0)
                            languageMediaItem.TargetLanguageIDs = currentTargetLanguageIDs;
                        if (LanguageID.CompareLanguageIDLists(languageMediaItem.HostLanguageIDs, null) != 0)
                            languageMediaItem.HostLanguageIDs = null;
                    }

                    languageMediaItem.SetupMediaDescriptions(baseFileName, fileExtension, addLanguageSuffix);
                }
            }

            RemoveUnusedLanguageMediaItems();
        }

        public void SetupMediaDescriptionsCheck(string baseFileName, string fileExtension, bool addLanguageSuffix)
        {
            BaseObjectContent content = Content;

            if (content == null)
                return;

            List<LanguageID> targetLanguageIDs = content.TargetLanguageIDs;
            List<LanguageID> hostLanguageIDs = content.HostLanguageIDs;
            Dictionary<LanguageID, List<LanguageID>> targetMediaLanguageIDDictionary =
                LanguageID.GetMediaLanguageIDDictionary(targetLanguageIDs);
            Dictionary<LanguageID, List<LanguageID>> hostMediaLanguageIDDictionary =
                LanguageID.GetMediaLanguageIDDictionary(hostLanguageIDs);
            LanguageID currentTargetMediaLanguageID;
            LanguageID currentHostMediaLanguageID;
            List<LanguageID> currentTargetLanguageIDs;
            List<LanguageID> currentHostLanguageIDs;

            foreach (KeyValuePair<LanguageID, List<LanguageID>> targetKVP in targetMediaLanguageIDDictionary)
            {
                currentTargetMediaLanguageID = targetKVP.Key;
                currentTargetLanguageIDs = targetKVP.Value;

                if (hostMediaLanguageIDDictionary.Count != 0)
                {
                    foreach (KeyValuePair<LanguageID, List<LanguageID>> hostKVP in hostMediaLanguageIDDictionary)
                    {
                        currentHostMediaLanguageID = hostKVP.Key;
                        currentHostLanguageIDs = hostKVP.Value;

                        LanguageMediaItem languageMediaItem = GetLanguageMediaItemWithMediaLanguages(currentTargetMediaLanguageID, currentHostMediaLanguageID);

                        if (languageMediaItem == null)
                        {
                            string languageMediaItemKey = LanguageMediaItem.ComposeLanguageMediaItemKey(currentTargetMediaLanguageID, currentHostMediaLanguageID);
                            languageMediaItem = new LanguageMediaItem(
                                languageMediaItemKey,
                                currentTargetLanguageIDs,
                                currentHostLanguageIDs,
                                Owner);
                            AddLanguageMediaItem(languageMediaItem);
                            languageMediaItem.SetupMediaDescriptions(baseFileName, fileExtension, addLanguageSuffix);
                        }
                        else
                        {
                            if (LanguageID.CompareLanguageIDLists(languageMediaItem.TargetLanguageIDs, currentTargetLanguageIDs) != 0)
                                languageMediaItem.TargetLanguageIDs = currentTargetLanguageIDs;
                            if (LanguageID.CompareLanguageIDLists(languageMediaItem.HostLanguageIDs, currentHostLanguageIDs) != 0)
                                languageMediaItem.HostLanguageIDs = currentHostLanguageIDs;
                        }
                    }
                }
                else
                {
                    LanguageMediaItem languageMediaItem = GetLanguageMediaItemWithMediaLanguages(currentTargetMediaLanguageID, null);

                    if (languageMediaItem == null)
                    {
                        string languageMediaItemKey = LanguageMediaItem.ComposeLanguageMediaItemKey(currentTargetMediaLanguageID, null);
                        languageMediaItem = new LanguageMediaItem(
                            languageMediaItemKey,
                            currentTargetLanguageIDs,
                            null,
                            Owner);
                        AddLanguageMediaItem(languageMediaItem);
                        languageMediaItem.SetupMediaDescriptions(baseFileName, fileExtension, addLanguageSuffix);
                    }
                    else
                    {
                        if (LanguageID.CompareLanguageIDLists(languageMediaItem.TargetLanguageIDs, currentTargetLanguageIDs) != 0)
                            languageMediaItem.TargetLanguageIDs = currentTargetLanguageIDs;
                        if (LanguageID.CompareLanguageIDLists(languageMediaItem.HostLanguageIDs, null) != 0)
                            languageMediaItem.HostLanguageIDs = null;
                    }
                }
            }

            RemoveUnusedLanguageMediaItems();
        }

        public void RemoveUnusedLanguageMediaItems()
        {
            BaseObjectContent content = Content;

            if (content == null)
                return;

            if (_LanguageMediaItems == null)
                return;

            List<LanguageID> targetMediaLanguageIDs = content.MediaTargetLanguageIDs;
            List<LanguageID> hostMediaLanguageIDs = content.MediaHostLanguageIDs;

            if (targetMediaLanguageIDs == null)
                targetMediaLanguageIDs = new List<LanguageID>();

            if (hostMediaLanguageIDs == null)
                hostMediaLanguageIDs = new List<LanguageID>();

            int count = _LanguageMediaItems.Count;
            int index;

            for (index = count - 1; index >= 0; index--)
            {
                LanguageMediaItem languageMediaItem = _LanguageMediaItems[index];
                List<LanguageID> itemTargetMediaLanguages = languageMediaItem.MediaTargetLanguageIDs;
                List<LanguageID> itemHostMediaLanguages = languageMediaItem.MediaHostLanguageIDs;
                bool remove = false;

                if (targetMediaLanguageIDs.Count != 0)
                {
                    if ((itemTargetMediaLanguages == null) || (itemTargetMediaLanguages.Count == 0))
                        remove = true;
                    else
                    {
                        foreach (LanguageID languageID in itemTargetMediaLanguages)
                        {
                            if (!targetMediaLanguageIDs.Contains(languageID))
                            {
                                remove = true;
                                break;
                            }

                            if ((itemHostMediaLanguages != null) && (itemHostMediaLanguages.Count() != 0))
                            {
                                foreach (LanguageID hostLanguageID in itemHostMediaLanguages)
                                {
                                    if (!hostMediaLanguageIDs.Contains(hostLanguageID))
                                    {
                                        remove = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (hostMediaLanguageIDs.Count != 0)
                {
                    if ((itemHostMediaLanguages == null) || (itemHostMediaLanguages.Count == 0))
                        remove = true;
                    else
                    {
                        foreach (LanguageID languageID in itemHostMediaLanguages)
                        {
                            if (!hostMediaLanguageIDs.Contains(languageID))
                            {
                                remove = true;
                                break;
                            }

                            if ((itemTargetMediaLanguages != null) && (itemTargetMediaLanguages.Count() != 0))
                            {
                                foreach (LanguageID targetLanguageID in itemTargetMediaLanguages)
                                {
                                    if (!targetMediaLanguageIDs.Contains(targetLanguageID))
                                    {
                                        remove = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    remove = true;
                    break;
                }

                if (remove)
                    DeleteLanguageMediaItemIndexed(index);
            }
        }

        public string GetDefaultFileExtension()
        {
            string fileExtension;

            switch (ContentType)
            {
                case "Audio":
                    fileExtension = ".mp3";
                    break;
                case "Video":
                    fileExtension = ".mp4";
                    break;
                case "Image":
                    fileExtension = ".jpg";
                    break;
                case "TextFile":
                    fileExtension = ".txt";
                    break;
                case "PDF":
                    fileExtension = ".pdf";
                    break;
                default:
                    fileExtension = "";
                    break;
            }

            return fileExtension;
        }

        public string GetDefaultMimeType(string fileExtension)
        {
            string mimeType = MediaUtilities.GetMimeTypeFromMediaTypeAndFileExtension(
                ContentType, fileExtension);
            return mimeType;
        }

        public MediaTypeCode GetDefaultMediaType()
        {
            MediaTypeCode mediaType;

            switch (ContentType)
            {
                case "Audio":
                    mediaType = MediaTypeCode.Audio;
                    break;
                case "Video":
                    mediaType = MediaTypeCode.Video;
                    break;
                case "Image":
                    mediaType = MediaTypeCode.Image;
                    break;
                case "TextFile":
                    mediaType = MediaTypeCode.TextFile;
                    break;
                case "PDF":
                    mediaType = MediaTypeCode.PDF;
                    break;
                case "Embedded":
                    mediaType = MediaTypeCode.Embedded;
                    break;
                default:
                    mediaType = MediaTypeCode.Unknown;
                    break;
            }

            return mediaType;
        }

        public ContentMediaItem MediaItemSource
        {
            get
            {
                return _MediaItemSource;
            }
            set
            {
                if (value != _MediaItemSource)
                {
                    _MediaItemSource = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override string MediaTildeUrl
        {
            get
            {
                if (_MediaItemSource != null)
                    return _MediaItemSource.MediaTildeUrl;

                return base.MediaTildeUrl;
            }
        }

        public override BaseContentStorage ReferenceSource
        {
            get
            {
                return MediaItemSource;
            }
            set
            {
                MediaItemSource = value as ContentMediaItem;
            }
        }

        public MultiLanguageString GetLocalSpeakerName(string key)
        {
            if (key == null)
                key = "";

            if (_LocalSpeakerNames != null)
                return _LocalSpeakerNames.FirstOrDefault(x => x.KeyString == key);

            return null;
        }

        public MultiLanguageString GetLocalSpeakerNameIndexed(int index)
        {
            if ((_LocalSpeakerNames != null) && (index >= 0) && (index < _LocalSpeakerNames.Count()))
                return _LocalSpeakerNames[index];

            return null;
        }

        public string GetLocalSpeakerNameText(string key, LanguageID languageID)
        {
            MultiLanguageString speakerName = GetLocalSpeakerName(key);

            if (speakerName != null)
                return speakerName.Text(languageID);

            return "";
        }

        public int GetLocalSpeakerNameIndex(string key)
        {
            if ((_LocalSpeakerNames != null) && (key != null))
                return _LocalSpeakerNames.IndexOf(GetLocalSpeakerName(key));

            return -1;
        }

        public List<MultiLanguageString> CloneLocalSpeakerNames()
        {
            if (_LocalSpeakerNames == null)
                return null;

            List<MultiLanguageString> returnValue = new List<MultiLanguageString>(_LocalSpeakerNames.Count());

            foreach (MultiLanguageString multiLanguageItem in _LocalSpeakerNames)
                returnValue.Add(new MultiLanguageString(multiLanguageItem));

            return returnValue;
        }

        public bool InsertLocalSpeakerNameIndexed(int index, MultiLanguageString speakerName)
        {
            if (_LocalSpeakerNames == null)
                _LocalSpeakerNames = new List<MultiLanguageString>(1) { speakerName };
            else if ((index >= 0) && (index <= _LocalSpeakerNames.Count()))
                _LocalSpeakerNames.Insert(index, speakerName);
            else
                return false;

            ModifiedFlag = true;

            return true;
        }

        public bool InsertLocalSpeakerNamesIndexed(int index, List<MultiLanguageString> speakerNames)
        {
            if (speakerNames == null)
                return true;

            if (_LocalSpeakerNames == null)
                _LocalSpeakerNames = new List<MultiLanguageString>(speakerNames);
            else if ((index >= 0) && (index <= _LocalSpeakerNames.Count()))
                _LocalSpeakerNames.InsertRange(index, speakerNames);
            else
                return false;

            ModifiedFlag = true;

            return true;
        }

        public bool AddLocalSpeakerName(MultiLanguageString speakerName)
        {
            if (_LocalSpeakerNames == null)
                _LocalSpeakerNames = new List<MultiLanguageString>(1) { speakerName };
            else
                _LocalSpeakerNames.Add(speakerName);

            ModifiedFlag = true;

            return true;
        }

        public bool AddLocalSpeakerNames(List<MultiLanguageString> speakerNames)
        {
            if (_LocalSpeakerNames == null)
                _LocalSpeakerNames = new List<MultiLanguageString>(speakerNames);
            else
                _LocalSpeakerNames.AddRange(speakerNames);

            ModifiedFlag = true;

            return true;
        }

        public bool DeleteLocalSpeakerName(MultiLanguageString speakerName)
        {
            if (_LocalSpeakerNames != null)
            {
                if (_LocalSpeakerNames.Remove(speakerName))
                {
                    ModifiedFlag = true;
                    return true;
                }

                if (_LocalStudyItems != null)
                {
                    int count = _LocalStudyItems.Count();
                    for (int index = 0; index < count; index++)
                    {
                        MultiLanguageItem studyItem = _LocalStudyItems[index];
                        if (studyItem.SpeakerNameKey == speakerName.KeyString)
                            studyItem.SpeakerNameKey = null;
                    }
                }
            }
            return false;
        }

        public bool DeleteLocalSpeakerNameIndexed(int index)
        {
            MultiLanguageString speakerName = GetLocalSpeakerNameIndexed(index);

            if (speakerName != null)
                return DeleteLocalSpeakerName(speakerName);

            return false;
        }

        public void DeleteAllLocalSpeakerNames()
        {
            if (_LocalSpeakerNames != null)
                ModifiedFlag = true;
            _LocalSpeakerNames = null;
            if (_LocalStudyItems != null)
            {
                int count = _LocalStudyItems.Count();
                for (int index = 0; index < count; index++)
                {
                    MultiLanguageItem studyItem = _LocalStudyItems[index];
                    studyItem.SpeakerNameKey = null;
                }
            }
        }

        public int LocalSpeakerNameCount()
        {
            if (_LocalSpeakerNames != null)
                return (_LocalSpeakerNames.Count());
            return 0;
        }

        public List<MultiLanguageItem> CloneLocalStudyItems()
        {
            if (_LocalStudyItems == null)
                return null;

            List<MultiLanguageItem> newList = new List<MultiLanguageItem>();

            foreach (MultiLanguageItem studyItem in _LocalStudyItems)
                newList.Add(new MultiLanguageItem(studyItem));

            return newList;
        }

        public MultiLanguageItem GetLocalStudyItemIndexed(int index)
        {
            if ((_LocalStudyItems != null) && (index >= 0) && (index < _LocalStudyItems.Count()))
                return _LocalStudyItems[index];
            return null;
        }

        public int GetLocalStudyItemIndex(MultiLanguageItem studyItem)
        {
            if (_LocalStudyItems != null)
                return _LocalStudyItems.IndexOf(studyItem);
            return -1;
        }

        public bool AddLocalStudyItem(MultiLanguageItem studyItem)
        {
            if (_LocalStudyItems == null)
                _LocalStudyItems = new List<MultiLanguageItem>(1) { studyItem };
            else
                _LocalStudyItems.Add(studyItem);

            ModifiedFlag = true;

            return true;
        }

        public bool InsertLocalStudyItem(int index, MultiLanguageItem studyItem)
        {
            if (_LocalStudyItems == null)
                _LocalStudyItems = new List<MultiLanguageItem>(1) { studyItem };
            else
                _LocalStudyItems.Insert(index, studyItem);

            ModifiedFlag = true;

            return true;
        }

        public bool AddLocalStudyItems(List<MultiLanguageItem> studyItems)
        {
            if (_LocalStudyItems == null)
                _LocalStudyItems = new List<MultiLanguageItem>(studyItems);
            else
                _LocalStudyItems.AddRange(studyItems);

            ModifiedFlag = true;

            return true;
        }

        public bool DeleteLocalStudyItem(MultiLanguageItem studyItem)
        {
            if (_LocalStudyItems != null)
            {
                if (_LocalStudyItems.Remove(studyItem))
                {
                    ModifiedFlag = true;
                    return true;
                }
            }

            return false;
        }

        public bool DeleteLocalStudyItemIndexed(int index)
        {
            if ((_LocalStudyItems != null) && (index >= 0) && (index < _LocalStudyItems.Count()))
            {
                _LocalStudyItems.RemoveAt(index);
                ModifiedFlag = true;
                return true;
            }

            return false;
        }

        public void DeleteAllLocalStudyItems()
        {
            if ((_LocalStudyItems != null) && (_LocalStudyItems.Count != 0))
                ModifiedFlag = true;
            _LocalStudyItems = null;
        }

        public int LocalStudyItemCount()
        {
            if (_LocalStudyItems != null)
                return (_LocalStudyItems.Count());

            return 0;
        }

        public string AllocateLocalStudyItemKey()
        {
            int ordinal = LocalStudyItemCount();
            string value = "I" + ordinal.ToString();
            return value;
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItemKey, LanguageID mediaLanguageID,
            TimeSpan mediaStartTime, TimeSpan mediaStopTime)
        {
            if (_LocalStudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _LocalStudyItems)
                    studyItem.MapTextClear(mediaItemKey, languageMediaItemKey, mediaLanguageID,
                        mediaStartTime, mediaStopTime);
            }
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItemKey, List<LanguageID> mediaLanguageIDs,
            TimeSpan mediaStartTime, TimeSpan mediaStopTime)
        {
            if (_LocalStudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _LocalStudyItems)
                    studyItem.MapTextClear(mediaItemKey, languageMediaItemKey, mediaLanguageIDs,
                        mediaStartTime, mediaStopTime);
            }
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItemKey, LanguageID mediaLanguageID)
        {
            if (_LocalStudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _LocalStudyItems)
                    studyItem.MapTextClear(mediaItemKey, languageMediaItemKey, mediaLanguageID);
            }
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItemKey, List<LanguageID> mediaLanguageIDs)
        {
            if (_LocalStudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _LocalStudyItems)
                    studyItem.MapTextClear(mediaItemKey, languageMediaItemKey, mediaLanguageIDs);
            }
        }

        public override void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            if (_ReferenceSourceKey != null)
            {
                if (_MediaItemSource == null)
                    _MediaItemSource = mainRepository.MediaItems.Get(_ReferenceSourceKey);
            }
        }

        public override bool SaveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;
            return returnValue;
        }

        public override bool UpdateReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;
            return returnValue;
        }

        public override bool UpdateReferencesCheck(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;
            return returnValue;
        }

        public override void ClearReferences(bool recurseParents, bool recurseChildren)
        {
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if ((_SourceContentKeys != null) && (_SourceContentKeys.Count() != 0))
                element.Add(new XAttribute("SourceContentKeys", ObjectUtilities.GetStringFromStringList(_SourceContentKeys)));

            if (!String.IsNullOrEmpty(_TranscriptContentKey))
                element.Add(new XAttribute("TranscriptContentKey", _TranscriptContentKey));

            if (!String.IsNullOrEmpty(_DisplayWidth))
                element.Add(new XAttribute("Width", _DisplayWidth));

            if (!String.IsNullOrEmpty(_DisplayHeight))
                element.Add(new XAttribute("_DisplayHeight", _DisplayHeight));

            if (_LanguageMediaItems != null)
            {
                foreach (LanguageMediaItem languageMediaItem in _LanguageMediaItems)
                    element.Add(languageMediaItem.GetElement("LanguageMediaItem"));
            }

            if (_LocalSpeakerNames != null)
            {
                foreach (MultiLanguageString speakerName in _LocalSpeakerNames)
                    element.Add(speakerName.GetElement("LocalSpeakerName"));
            }

            if (_LocalStudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _LocalStudyItems)
                    element.Add(studyItem.GetElement("LocalStudyItem"));
            }

            if (!String.IsNullOrEmpty(_MediaFileNamePattern))
                element.Add(new XAttribute("MediaFileNamePattern", _MediaFileNamePattern));

            if (!String.IsNullOrEmpty(_PlayerSource) && (_PlayerSource != "File"))
                element.Add(new XElement("PlayerSource", _PlayerSource));

            if (!String.IsNullOrEmpty(_PlayerType) && (_PlayerType != "Full"))
                element.Add(new XElement("PlayerType", _PlayerType));

            return element;
        }

        public override XElement GetElementFiltered(string name, Dictionary<string, bool> itemKeyFlags)
        {
            XElement element = base.GetElementFiltered(name, itemKeyFlags);

            if ((_SourceContentKeys != null) && (_SourceContentKeys.Count() != 0))
                element.Add(new XAttribute("SourceContentKeys", ObjectUtilities.GetStringFromStringList(_SourceContentKeys)));

            if (!String.IsNullOrEmpty(_TranscriptContentKey))
                element.Add(new XAttribute("TranscriptContentKey", _TranscriptContentKey));

            if (!String.IsNullOrEmpty(_DisplayWidth))
                element.Add(new XAttribute("DisplayWidth", _DisplayWidth));

            if (!String.IsNullOrEmpty(_DisplayHeight))
                element.Add(new XAttribute("DisplayHeight", _DisplayHeight));

            if (_LanguageMediaItems != null)
            {
                foreach (LanguageMediaItem languageMediaItem in _LanguageMediaItems)
                    element.Add(languageMediaItem.GetElement("LanguageMediaItem"));
            }

            if (_LocalSpeakerNames != null)
            {
                foreach (MultiLanguageString speakerName in _LocalSpeakerNames)
                    element.Add(speakerName.GetElement("LocalSpeakerName"));
            }

            if (_LocalStudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in _LocalStudyItems)
                    element.Add(studyItem.GetElement("LocalStudyItem"));
            }

            if (!String.IsNullOrEmpty(_MediaFileNamePattern))
                element.Add(new XAttribute("MediaFileNamePattern", _MediaFileNamePattern));

            if (!String.IsNullOrEmpty(_PlayerSource) && (_PlayerSource != "File"))
                element.Add(new XElement("PlayerSource", _PlayerSource));

            if (!String.IsNullOrEmpty(_PlayerType) && (_PlayerType != "Full"))
                element.Add(new XElement("PlayerType", _PlayerType));

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "SourceContentKeys":
                    SourceContentKeys = ObjectUtilities.GetStringListFromString(attributeValue);
                    break;
                case "TranscriptContentKey":
                    TranscriptContentKey = attributeValue;
                    break;
                case "DisplayWidth":
                case "Width":   // Legacy
                    DisplayWidth = attributeValue;
                    break;
                case "DisplayHeight":
                case "_DisplayHeight":   // Legacy
                case "Height":   // Legacy
                    DisplayHeight = attributeValue;
                    break;
                case "MediaFileNamePattern":
                    MediaFileNamePattern = attributeValue;
                    break;
                case "PlayerSource": // Legacy
                    PlayerSource = attributeValue;
                    break;
                case "PlayerType": // Legacy
                    PlayerType = attributeValue;
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            MultiLanguageItem studyItem;
            MultiLanguageString speakerName;

            switch (childElement.Name.LocalName)
            {
                case "LanguageMediaItem":
                    {
                        LanguageMediaItem languageMediaItem = new LanguageMediaItem(childElement);
                        AddLanguageMediaItem(languageMediaItem);
                    }
                    break;
                case "LocalStudyItem":
                    studyItem = new MultiLanguageItem(childElement);
                    AddLocalStudyItem(studyItem);
                    break;
                case "LocalSpeakerName":
                    speakerName = new MultiLanguageString(childElement);
                    AddLocalSpeakerName(speakerName);
                    break;
                // Legacy.
                case "MediaDescription":
                    {
                        MediaDescription mediaDescription = new MediaDescription(childElement);
                        LanguageMediaItem languageMediaItem = GetLanguageMediaItemIndexed(0);
                        if (languageMediaItem == null)
                        {
                            LanguageID targetMediaLanguageID = BaseObjectContent.CurrentContent.FirstMediaTargetLanguageID;
                            LanguageID hostMediaLanguageID = BaseObjectContent.CurrentContent.FirstMediaHostLanguageID;
                            string languageMediaItemKey = LanguageMediaItem.ComposeLanguageMediaItemKey(
                                targetMediaLanguageID,
                                hostMediaLanguageID);
                            languageMediaItem = new LanguageMediaItem(
                                languageMediaItemKey,
                                BaseObjectContent.CurrentContent.CloneTargetLanguageIDs(),
                                BaseObjectContent.CurrentContent.CloneHostLanguageIDs(),
                                BaseObjectContent.CurrentContent.Owner,
                                new List<MediaDescription>() { mediaDescription });
                            AddLanguageMediaItem(languageMediaItem);
                        }
                        else
                            languageMediaItem.AddMediaDescription(mediaDescription);
                    }
                    break;
                case "PlayerSource":
                    PlayerSource = childElement.Value.Trim();
                    break;
                case "PlayerType":
                    PlayerType = childElement.Value.Trim();
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public static int Compare(ContentMediaItem item1, ContentMediaItem item2)
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
            ContentMediaItem otherContentMediaItem = other as ContentMediaItem;
            int diff;

            if (otherContentMediaItem != null)
            {
                diff = base.Compare(other);

                if (diff != 0)
                    return diff;

                diff = ObjectUtilities.CompareStringLists(_SourceContentKeys, otherContentMediaItem.SourceContentKeys);

                if (diff != 0)
                    return diff;

                diff = ObjectUtilities.CompareStrings(_TranscriptContentKey, otherContentMediaItem.TranscriptContentKey);

                if (diff != 0)
                    return diff;

                diff = LanguageMediaItem.CompareLanguageMediaItemLists(_LanguageMediaItems, otherContentMediaItem.LanguageMediaItems);

                return diff;
            }

            return base.Compare(other);
        }

        public static int CompareContentMediaItemLists(List<ContentMediaItem> list1, List<ContentMediaItem> list2)
        {
            return ObjectUtilities.CompareTypedObjectLists<ContentMediaItem>(list1, list2);
        }
    }
}
