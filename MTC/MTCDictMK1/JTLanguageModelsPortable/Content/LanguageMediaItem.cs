using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Content
{
    public class LanguageMediaItem : BaseObjectLanguages
    {
        protected List<MediaDescription> _MediaDescriptions;    // Describes different media files representing the same content in different formats.
        protected ContentMediaItem _MediaItem;                  // Not stored.

        public LanguageMediaItem(object key, List<LanguageID> targetLanguageIDs,
                List<LanguageID> hostLanguageIDs, string owner, List<MediaDescription> mediaDescriptions)
            : base(key, targetLanguageIDs, hostLanguageIDs, owner)
        {
            _MediaDescriptions = mediaDescriptions;
            _MediaItem = null;
        }

        public LanguageMediaItem(object key, List<LanguageID> targetLanguageIDs,
                List<LanguageID> hostLanguageIDs, string owner)
            : base(key, targetLanguageIDs, hostLanguageIDs, owner)
        {
            ClearLanguageMediaItem();
        }

        public LanguageMediaItem(object key)
            : base(key)
        {
            ClearLanguageMediaItem();
        }

        public LanguageMediaItem(LanguageMediaItem other)
            : base(other)
        {
            Copy(other);
            Modified = false;
        }

        public LanguageMediaItem(XElement element)
        {
            OnElement(element);
        }

        public LanguageMediaItem()
        {
            ClearLanguageMediaItem();
        }

        public void Copy(LanguageMediaItem other)
        {
            base.Copy(other);

            if (other == null)
            {
                ClearLanguageMediaItem();
                return;
            }

            _MediaDescriptions = other.CloneMediaDescriptions();
            _MediaItem = other.MediaItem;

            ModifiedFlag = true;
        }

        public void CopyDeep(LanguageMediaItem other)
        {
            Copy(other);
            base.CopyDeep(other);
        }

        public override void Clear()
        {
            base.Clear();
            ClearLanguageMediaItem();
        }

        public void ClearLanguageMediaItem()
        {
            _MediaDescriptions = null;
            _MediaItem = null;
        }

        public override IBaseObject Clone()
        {
            return new LanguageMediaItem(this);
        }

        public LanguageID TargetMediaLanguageID
        {
            get
            {
                return FirstMediaTargetLanguageID;
            }
        }

        public LanguageID HostMediaLanguageID
        {
            get
            {
                return FirstMediaHostLanguageID;
            }
        }

        public string GetName(LanguageID displayLanguageID)
        {
            LanguageID targetMediaLanguageID = TargetMediaLanguageID;
            LanguageID hostMediaLanguageID = HostMediaLanguageID;
            string name = String.Empty;

            if (targetMediaLanguageID != null)
                name += targetMediaLanguageID.LanguageName(displayLanguageID);

            if (hostMediaLanguageID != null)
            {
                if (!String.IsNullOrEmpty(name))
                    name += "-";

                name += hostMediaLanguageID.LanguageName(displayLanguageID);
            }

            return name;
        }

        public List<MediaDescription> MediaDescriptions
        {
            get
            {
                return _MediaDescriptions;
            }
            set
            {
                if (value != _MediaDescriptions)
                {
                    _MediaDescriptions = value;
                    ModifiedFlag = true;
                }
            }
        }

        public BaseObjectNode Node
        {
            get
            {
                BaseObjectContent content = Content;
                if (content != null)
                    return content.Node;
                return null;
            }
        }

        public BaseObjectContent Content
        {
            get
            {
                if (_MediaItem != null)
                    return _MediaItem.Content;
                return null;
            }
            set
            {
                if (value != null)
                    _MediaItem = value.ContentStorageMediaItem;
                else
                    _MediaItem = null;
            }
        }

        public ContentMediaItem MediaItem
        {
            get
            {
                return _MediaItem;
            }
            set
            {
                _MediaItem = value;
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

        public string MediaTildeUrl
        {
            get
            {
                if (_MediaItem != null)
                    return _MediaItem.MediaTildeUrl;
                return String.Empty;
            }
        }

        public MediaDescription GetMediaDescription(object key)
        {
            if ((_MediaDescriptions != null) && (key != null))
                return _MediaDescriptions.FirstOrDefault(x => x.MatchKey(key));

            return null;
        }

        public MediaDescription GetMediaDescriptionIndexed(int index)
        {
            if ((_MediaDescriptions != null) && (index >= 0) && (index < _MediaDescriptions.Count()))
                return _MediaDescriptions.ElementAt(index);

            return null;
        }

        public MediaDescription GetMediaDescriptionWithMimeType(string mimeType)
        {
            MediaDescription mediaDescription = null;

            if (_MediaDescriptions != null)
            {
                if (!String.IsNullOrEmpty(mimeType))
                    mediaDescription = _MediaDescriptions.FirstOrDefault(x => (x.MimeType == mimeType));
            }

            return mediaDescription;
        }

        public MediaDescription GetAudioMediaDescription()
        {
            if (_MediaDescriptions != null)
            {
                foreach (MediaDescription mediaDescription in _MediaDescriptions)
                {
                    switch (mediaDescription.MimeType)
                    {
                        case "audio/mp3":
                        case "audio/mpeg3":
                        case "audio/mpeg":
                            return mediaDescription;
                        default:
                            break;
                    }
                }
                foreach (MediaDescription mediaDescription in _MediaDescriptions)
                {
                    switch (mediaDescription.MimeType)
                    {
                        case "audio/ogg":
                            return mediaDescription;
                        default:
                            break;
                    }
                }
            }

            return null;
        }

        public bool AddMediaDescription(MediaDescription mediaDescription)
        {
            if (_MediaDescriptions == null)
                _MediaDescriptions = new List<MediaDescription>(1) { mediaDescription };
            else
                _MediaDescriptions.Add(mediaDescription);

            ModifiedFlag = true;

            return true;
        }

        public bool InsertMediaDescriptionIndexed(int index, MediaDescription mediaDescription)
        {
            if (_MediaDescriptions == null)
                _MediaDescriptions = new List<MediaDescription>(1) { mediaDescription };
            else if (index < _MediaDescriptions.Count())
                _MediaDescriptions.Insert(index, mediaDescription);
            else
                _MediaDescriptions.Add(mediaDescription);

            ModifiedFlag = true;

            return true;
        }

        public bool DeleteMediaDescription(MediaDescription mediaDescription)
        {
            if (_MediaDescriptions != null)
            {
                if (_MediaDescriptions.Remove(mediaDescription))
                {
                    ModifiedFlag = true;
                    return true;
                }
            }

            return false;
        }

        public bool DeleteMediaDescriptionKey(object key)
        {
            if ((_MediaDescriptions != null) && (key != null))
            {
                MediaDescription mediaDescription = GetMediaDescription(key);

                if (mediaDescription != null)
                {
                    _MediaDescriptions.Remove(mediaDescription);
                    ModifiedFlag = true;
                    return true;
                }
            }

            return false;
        }

        public bool DeleteMediaDescriptionWithMimeTypeAndLanguages(string mimeType)
        {
            MediaDescription mediaDescription;
            int count;
            int index;
            bool returnValue = false;

            if ((_MediaDescriptions != null) && ((count = _MediaDescriptions.Count()) != 0))
            {
                if (!String.IsNullOrEmpty(mimeType))
                {
                    for (index = count - 1; index >= 0; index--)
                    {
                        mediaDescription = _MediaDescriptions[index];

                        if (mediaDescription.MimeType == mimeType)
                        {
                            _MediaDescriptions.RemoveAt(index);
                            returnValue = true;
                        }
                    }
                }
            }

            return returnValue;
        }

        public bool DeleteMediaDescriptionIndexed(int index)
        {
            if ((_MediaDescriptions != null) && (index >= 0) && (index < _MediaDescriptions.Count()))
            {
                _MediaDescriptions.RemoveAt(index);
                ModifiedFlag = true;
                return true;
            }
            return false;
        }

        public void DeleteAllMediaDescriptions()
        {
            if (_MediaDescriptions != null)
            {
                if (_MediaDescriptions.Count() != 0)
                    ModifiedFlag = true;

                _MediaDescriptions.Clear();
            }
        }

        public int MediaDescriptionCount()
        {
            if (_MediaDescriptions != null)
                return (_MediaDescriptions.Count());

            return 0;
        }

        public List<MediaDescription> CloneMediaDescriptions()
        {
            if (_MediaDescriptions == null)
                return null;

            List<MediaDescription> returnValue = new List<MediaDescription>(_MediaDescriptions.Count());

            foreach (MediaDescription mediaDescription in _MediaDescriptions)
                returnValue.Add(new MediaDescription(mediaDescription));

            return returnValue;
        }

        public void CollectMediaFiles(List<string> mediaFiles, VisitMedia visitFunction)
        {
            if (_MediaDescriptions != null)
            {
                string mediaTildeUrl = MediaTildeUrl;

                foreach (MediaDescription mediaDescription in _MediaDescriptions)
                    mediaDescription.CollectMediaFiles(mediaTildeUrl, mediaFiles, _MediaItem, visitFunction);
            }
        }

        public override void CollectReferences(List<IBaseObjectKeyed> references, List<IBaseObjectKeyed> externalSavedChildren,
            List<IBaseObjectKeyed> externalNonSavedChildren, Dictionary<int, bool> intSelectFlags,
            Dictionary<string, bool> stringSelectFlags, Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles,
            VisitMedia visitFunction)
        {
            CollectMediaFiles(mediaFiles, visitFunction);
        }

        public bool CopyMedia(string targetDirectoryRoot, List<string> copiedFiles, ref string errorMessage)
        {
            bool returnValue = true;

            if (_MediaDescriptions != null)
            {
                string mediaTildeUrl = MediaTildeUrl;
                string sourceMediaDirectory = ApplicationData.MapToFilePath(mediaTildeUrl);

                foreach (MediaDescription mediaDescription in _MediaDescriptions)
                {
                    if (!mediaDescription.CopyMedia(sourceMediaDirectory, targetDirectoryRoot, copiedFiles, ref errorMessage))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public void GenerateMediaDescriptions(string mediaFileNamePattern)
        {
            string fileSuffix = MediaUtilities.GetFileExtension(mediaFileNamePattern);
            string fileName = mediaFileNamePattern;
            string mimeType = GetDefaultMimeType(fileSuffix);
            MediaTypeCode mediaType = GetDefaultMediaType();
            LanguageID targetMediaLanguageID = TargetMediaLanguageID;
            LanguageID hostMediaLanguageID = HostMediaLanguageID;

            if (fileName == null)
                fileName = String.Empty;

            if (String.IsNullOrEmpty(mimeType))
            {
                if (MediaItem.PlayerSource == "YouTube")
                {
                    if (mediaType == MediaTypeCode.Audio)
                        mimeType = "audio/youtubeid";
                    else
                        mimeType = "video/youtubeid";
                }
                else
                    mimeType = "application/url";
            }

            if (targetMediaLanguageID != null)
                fileName = fileName.Replace("%t", targetMediaLanguageID.LanguageCultureExtensionCode);

            if (hostMediaLanguageID != null)
                fileName = fileName.Replace("%h", hostMediaLanguageID.LanguageCultureExtensionCode);

            string baseFileName = MediaUtilities.GetBaseFileName(fileName);

            DeleteAllMediaDescriptions();

            if (!String.IsNullOrEmpty(fileName))
            {
                AddMediaDescription(new MediaDescription(mimeType, mediaType, mimeType, fileName));
                AddAlternateMediaDescriptions(mimeType, baseFileName);
            }
        }

        public void SetupMediaDescriptions(string baseFileName, string fileExtension, bool addLanguageSuffix)
        {
            string languageSuffix = String.Empty;
            string contentSubTypeSuffix = String.Empty;
            string fileName = null;
            string mimeType = GetDefaultMimeType(fileExtension);
            string mediaKey = mimeType;
            MediaTypeCode mediaType = GetDefaultMediaType();

            switch (ContentType)
            {
                case "Document":
                    break;
                case "Audio":
                case "Video":
                case "Image":
                case "TextFile":
                case "PDF":
                    baseFileName = GetSubstitutedBaseFileName(baseFileName, addLanguageSuffix);
                    fileName = baseFileName + fileExtension;
                    break;
                case "Embedded":
                case "Automated":
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
                    throw new Exception("ContentMediaItem.GenerateMediaDescriptions: Unexpected content type: " + ContentType);
            }

            DeleteAllMediaDescriptions();

            if (!String.IsNullOrEmpty(fileName))
            {
                AddMediaDescription(new MediaDescription(mimeType, mediaType, mimeType, fileName));
                AddAlternateMediaDescriptions(mimeType, baseFileName);
            }
        }

        public string GetSubstitutedBaseFileName(string baseFileName, bool addLanguageSuffix)
        {
            if (baseFileName.Contains("%"))
            {
                LanguageID targetMediaLanguageID = TargetMediaLanguageID;
                LanguageID hostMediaLanguageID = HostMediaLanguageID;
                if (targetMediaLanguageID != null)
                    baseFileName = baseFileName.Replace("%t", targetMediaLanguageID.LanguageCultureExtensionCode);
                if (hostMediaLanguageID != null)
                    baseFileName = baseFileName.Replace("%h", hostMediaLanguageID.LanguageCultureExtensionCode);
            }
            else if (addLanguageSuffix)
                baseFileName += GetMediaLanguageSuffix();
            return baseFileName;
        }

        public void AddAlternateMediaDescriptions(string mimeType, string baseFileName)
        {
            switch (mimeType)
            {
                case "audio/ogg":
                    //AddMediaDescription(new MediaDescription("audio/mpeg3", MediaTypeCode.Audio, "audio/mpeg3",
                    //    baseFileName + "-aa.mp3"));
                    break;
                case "audio/mpeg3":
                case "audio/mpeg":
                case "audio/mp3":
                    //AddMediaDescription(new MediaDescription("audio/ogg", MediaTypeCode.Audio, "audio/ogg",
                    //    baseFileName + "-aa.ogg"));
                    break;
                case "audio/mp4a-latm":
                    //AddMediaDescription(new MediaDescription("audio/mpeg3", MediaTypeCode.Audio, "audio/mpeg3",
                    //    baseFileName + "-aa.mp3"));
                    //AddMediaDescription(new MediaDescription("audio/ogg", MediaTypeCode.Audio, "audio/ogg",
                    //    baseFileName + "-aa.ogg"));
                    break;
                case "video/ogg":
                    //AddMediaDescription(new MediaDescription("video/x-ms-wmv", MediaTypeCode.Video, "video/x-ms-wmv",
                    //    baseFileName + "-av.wmv"));
                    //AddMediaDescription(new MediaDescription("video/mp4", MediaTypeCode.Video, "video/mp4",
                    //    baseFileName + "-av.mp4"));
                    //AddMediaDescription(new MediaDescription("audio/mpeg3", MediaTypeCode.Audio, "audio/mpeg3",
                    //    baseFileName + "-aa.mp3"));
                    //AddMediaDescription(new MediaDescription("audio/ogg", MediaTypeCode.Audio, "audio/ogg",
                    //    baseFileName + "-aa.ogg"));
                    break;
                case "video/mp4":
                    //AddMediaDescription(new MediaDescription("video/x-ms-wmv", MediaTypeCode.Video, "video/x-ms-wmv",
                    //    baseFileName + "-av.wmv"));
                    //AddMediaDescription(new MediaDescription("video/ogg", MediaTypeCode.Video, "video/ogg",
                    //    baseFileName + "-av.ogg"));
                    AddMediaDescription(new MediaDescription("audio/mpeg3", MediaTypeCode.Audio, "audio/mpeg3",
                        baseFileName + "-aa.mp3"));
                    //AddMediaDescription(new MediaDescription("audio/ogg", MediaTypeCode.Audio, "audio/ogg",
                    //    baseFileName + "-aa.ogg"));
                    break;
                case "video/x-ms-wmv":
                    //AddMediaDescription(new MediaDescription("video/mp4", MediaTypeCode.Video, "video/mp4",
                    //    baseFileName + "-av.mp4"));
                    //AddMediaDescription(new MediaDescription("video/ogg", MediaTypeCode.Video, "video/ogg",
                    //    baseFileName + "-av.ogg"));
                    //AddMediaDescription(new MediaDescription("audio/mpeg3", MediaTypeCode.Audio, "audio/mpeg3",
                    //    baseFileName + "-aa.mp3"));
                    //AddMediaDescription(new MediaDescription("audio/ogg", MediaTypeCode.Audio, "audio/ogg",
                    //    baseFileName + "-aa.ogg"));
                    break;
                default:
                    break;
            }
        }

        public static string GetDefaultFileSuffix(string contentType)
        {
            string defaultSuffix = null;

            switch (contentType)
            {
                case "Document":
                    break;
                case "Audio":
                    defaultSuffix = ".mp3";
                    break;
                case "Video":
                    defaultSuffix = ".mp4";
                    break;
                case "Image":
                    defaultSuffix = ".jpg";
                    break;
                case "TextFile":
                    defaultSuffix = ".txt";
                    break;
                case "PDF":
                    defaultSuffix = ".pdf";
                    break;
                case "Embedded":
                case "Automated":
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
                    break;
                default:
                    throw new Exception("GetDefaultFileSuffix: Unknown content type: " + contentType);
            }

            return defaultSuffix;
        }

        public string GetMediaLanguageSuffix()
        {
            string suffix = String.Empty;
            string languageCode;
            List<string> languageCodes = new List<string>();

            if (TargetLanguageIDs != null)
            {
                foreach (LanguageID languageID in TargetLanguageIDs)
                {
                    languageCode = languageID.LanguageCode;

                    if (!languageCodes.Contains(languageCode))
                    {
                        languageCodes.Add(languageCode);
                        suffix += "-" + languageCode;
                    }
                }
            }

            if (HostLanguageIDs != null)
            {
                foreach (LanguageID languageID in HostLanguageIDs)
                {
                    languageCode = languageID.LanguageCode;

                    if (!languageCodes.Contains(languageCode))
                    {
                        languageCodes.Add(languageCode);
                        suffix += "-" + languageCode;
                    }
                }
            }

            return suffix;
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
                case "Embedded":
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

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_MediaDescriptions != null)
                {
                    foreach (MediaDescription mediaDescription in _MediaDescriptions)
                    {
                        if (mediaDescription.Modified)
                            return true;
                    }
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_MediaDescriptions != null)
                {
                    foreach (MediaDescription mediaDescription in _MediaDescriptions)
                        mediaDescription.Modified = false;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (_MediaDescriptions != null)
            {
                foreach (MediaDescription mediaDescription in _MediaDescriptions)
                    element.Add(mediaDescription.GetElement("MediaDescription"));
            }

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "MediaDescription":
                    {
                        MediaDescription mediaDescription = new MediaDescription(childElement);
                        AddMediaDescription(mediaDescription);
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public static int Compare(LanguageMediaItem item1, LanguageMediaItem item2)
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
            LanguageMediaItem otherLanguageMediaItem = other as LanguageMediaItem;
            int diff;

            if (otherLanguageMediaItem != null)
            {
                diff = base.Compare(other);

                if (diff != 0)
                    return diff;

                diff = MediaDescription.CompareMediaDescriptionLists(_MediaDescriptions, otherLanguageMediaItem.MediaDescriptions);

                return diff;
            }

            return base.Compare(other);
        }

        public static int CompareLanguageMediaItemLists(List<LanguageMediaItem> list1, List<LanguageMediaItem> list2)
        {
            return ObjectUtilities.CompareTypedObjectLists<LanguageMediaItem>(list1, list2);
        }

        public static string ComposeLanguageMediaItemKeyNotMediaLanguages(
            LanguageID targetLanguageID, LanguageID hostLanguageID)
        {
            string key = String.Empty;

            if (targetLanguageID != null)
                key += targetLanguageID.MediaLanguageCode();

            if (hostLanguageID != null)
                key += hostLanguageID.MediaLanguageCode();

            return key;
        }

        public static string ComposeLanguageMediaItemKey(
            LanguageID targetMediaLanguageID, LanguageID hostMediaLanguageID)
        {
            string key = String.Empty;

            if (targetMediaLanguageID != null)
                key += targetMediaLanguageID.LanguageCultureExtensionCode;

            if (hostMediaLanguageID != null)
                key += hostMediaLanguageID.LanguageCultureExtensionCode;

            return key;
        }

        public static string ComposeLanguageMediaItemKey(
            string targetMediaLanguageCode, string hostMediaLanguageCode)
        {
            string key = String.Empty;

            if (!String.IsNullOrEmpty(targetMediaLanguageCode))
                key += targetMediaLanguageCode;

            if (!String.IsNullOrEmpty(hostMediaLanguageCode))
                key += hostMediaLanguageCode;

            return key;
        }
    }
}
