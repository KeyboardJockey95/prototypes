using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Media
{
    public enum MediaTypeCode
    {
        Unknown,
        Audio,
        Video,
        Image,
        TextFile,
        PDF,
        Embedded,
        MultiPart
    }

    // Describes a media file.
    // If used in a ContentMediaItem, the key is the mime type string.
    public class MediaDescription : BaseObjectKeyed
    {
        protected MediaTypeCode _MediaType;
        protected string _MimeType;
        protected string _FileName;
        protected MediaStorageState _StorageState;
        public static MediaTypeCode DefaultMediaTypeCode = MediaTypeCode.Audio;
        public static string DefaultMediaMimeType = "audio/mpeg3";

        public MediaDescription(object key, MediaTypeCode mediaType, string mimeType, string fileName)
            : base(key)
        {
            _MediaType = mediaType;
            _MimeType = mimeType;
            _FileName = fileName;
            _StorageState = MediaStorageState.Unknown;
        }

        public MediaDescription(MediaDescription other)
            : base(other)
        {
            CopyMediaDescription(other);
            ModifiedFlag = false;
        }

        public MediaDescription(object key)
            : base(key)
        {
            ClearMediaDescription();
        }

        public MediaDescription(XElement element)
        {
            OnElement(element);
        }

        public MediaDescription()
        {
            ClearMediaDescription();
        }

        public override void Clear()
        {
            base.Clear();
            ClearMediaDescription();
        }

        public void ClearMediaDescription()
        {
            _MediaType = MediaTypeCode.Unknown;
            _MimeType = String.Empty;
            _FileName = String.Empty;
            _StorageState = MediaStorageState.Unknown;
        }

        public void CopyMediaDescription(MediaDescription other)
        {
            FileType = other.FileType;
            MimeType = other.MimeType;
            FileName = other.FileName;
            _StorageState = other.StorageState;
        }

        public override IBaseObject Clone()
        {
            return new MediaDescription(this);
        }

        public MediaTypeCode MediaType
        {
            get
            {
                return _MediaType;
            }
            set
            {
                if (_MediaType != value)
                {
                    _MediaType = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string FileType
        {
            get
            {
                return MediaUtilities.GetMediaTypeStringFromCode(_MediaType);
            }
            set
            {
                MediaTypeCode code = MediaUtilities.GetMediaTypeFromString(value);

                if (_MediaType != code)
                {
                    _MediaType = code;
                    ModifiedFlag = true;
                }
            }
        }

        public string MimeType
        {
            get
            {
                return _MimeType;
            }
            set
            {
                if (_MimeType != value)
                {
                    _MimeType = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasFileName()
        {
            return !String.IsNullOrEmpty(_FileName);
        }

        public string FileName
        {
            get
            {
                return _FileName;
            }
            set
            {
                if (_FileName != value)
                {
                    _FileName = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string BaseFileName
        {
            get
            {
                return MediaUtilities.GetBaseFileName(_FileName);
            }
            set
            {
                if (BaseFileName != value)
                {
                    string directory = MediaUtilities.GetFilePath(_FileName);
                    string fileExtension = MediaUtilities.GetFileExtension(_FileName);
                    string separator = MediaUtilities.GetPathSeparator(_FileName);
                    _FileName = MediaUtilities.ComposeFilePath(directory, value, fileExtension, separator);
                    ModifiedFlag = true;
                }
            }
        }

        public bool IsFullUrl
        {
            get
            {
                if (!String.IsNullOrEmpty(_FileName) && _FileName.Contains(":") && !_FileName.StartsWith("<"))
                    return true;

                return false;
            }
        }

        public bool IsRelativeUrl
        {
            get
            {
                if (!String.IsNullOrEmpty(_FileName) && _FileName.Contains(".."))
                    return true;

                return false;
            }
        }

        public bool IsEmbedded
        {
            get
            {
                if (!String.IsNullOrEmpty(_FileName) && _FileName.StartsWith("<"))
                    return true;

                return false;
            }
        }

        public MediaStorageState StorageState
        {
            get
            {
                return _StorageState;
            }
            set
            {
                if (value != _StorageState)
                {
                    _StorageState = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string GetUrlWithMediaCheck(string directoryUrl)
        {
            string url = GetUrl(directoryUrl);

            if (ApplicationData.IsMobileVersion)
            {
                bool changed;
                ApplicationData.Global.HandleMediaAccess(ref url, ref _StorageState, out changed);
                if (changed)
                    Modified = true;
            }

            return url;
        }

        public string GetUrl(string directoryUrl)
        {
            string url = FileName;

            if (String.IsNullOrEmpty(url))
                return String.Empty;

            if (IsFullUrl)
                return url;

            if (String.IsNullOrEmpty(directoryUrl))
                directoryUrl = "~/";

            if (!directoryUrl.EndsWith("/"))
                directoryUrl += "/";

            url = directoryUrl + url;

            return url;
        }

        public string GetContentUrlWithMediaCheck(string directoryUrl)
        {
            string url = GetUrlWithMediaCheck(directoryUrl);

            if (url.StartsWith("~"))
                url = url.Substring(1);

            return url;
        }

        public string GetContentUrl(string directoryUrl)
        {
            string url = GetUrl(directoryUrl);

            if (url.StartsWith("~"))
                url = url.Substring(1);

            return url;
        }

        public string GetDirectoryPath(string directoryUrl)
        {
            string url = GetUrl(directoryUrl);
            try
            {
                string path = ApplicationData.MapToFilePath(url);
                return path;
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        public string GetDirectoryPathWithMediaCheck(string directoryUrl)
        {
            string url = GetUrlWithMediaCheck(directoryUrl);
            try
            {
                string path = ApplicationData.MapToFilePath(url);
                return path;
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        public void CollectMediaFiles(string directoryUrl, List<string> mediaFiles, object content, VisitMedia visitFunction)
        {
            if (IsFullUrl)
                return;

            string filePath = GetDirectoryPath(directoryUrl);

            if (visitFunction != null)
                visitFunction(mediaFiles, content, this, filePath, MimeType);
            else if (mediaFiles != null)
            {
                if (!mediaFiles.Contains(filePath))
                    mediaFiles.Add(filePath);
            }
        }

        public bool CopyMedia(string sourceMediaDirectory, string targetDirectoryRoot, List<string> copiedFiles,
            ref string errorMessage)
        {
            if (IsFullUrl)
                return true;

            if (String.IsNullOrEmpty(FileName))
                return true;

            string sourceFilePath = sourceMediaDirectory;

            if (!sourceFilePath.EndsWith(ApplicationData.PlatformPathSeparator))
                sourceFilePath += ApplicationData.PlatformPathSeparator;

            sourceFilePath += FileName;

            if (!FileSingleton.Exists(sourceFilePath))
                return true;

            string targetFilePath = sourceFilePath.Replace(sourceMediaDirectory, targetDirectoryRoot);

            if (targetFilePath == sourceFilePath)
                return true;
            else if ((copiedFiles != null) && copiedFiles.Contains(targetFilePath))
                return true;

            try
            {
                FileSingleton.DirectoryExistsCheck(targetFilePath);
                FileSingleton.Copy(sourceFilePath, targetFilePath, true);

                if (copiedFiles != null)
                    copiedFiles.Add(targetFilePath);
            }
            catch (Exception)
            {
                errorMessage = (!String.IsNullOrEmpty(errorMessage) ? errorMessage + "\n" : String.Empty) +
                    "Error copying " + sourceFilePath + " to " + targetFilePath + ".";
                return false;
            }

            return true;
        }

        public void SetFileAndMimeTypeFromFileName()
        {
            if (FileName == null)
                return;

            string fileName = FileName.ToLower();
            string fileExtension = MediaUtilities.GetFileExtension(fileName);
            MimeType = MediaUtilities.GetMimeTypeFromFileName(fileName);

            switch (fileExtension)
            {
                case ".wmv":
                case ".mp4":
                case ".webm":
                    MediaType = MediaTypeCode.Video;
                    FileType = "Video";
                    break;
                case ".mp3":
                case ".ogg":
                case ".spx":
                case ".wav":
                    MediaType = MediaTypeCode.Audio;
                    FileType = "Audio";
                    break;
                case ".jpg":
                case ".png":
                case ".gif":
                    MediaType = MediaTypeCode.Image;
                    FileType = "Image";
                    break;
                case ".txt":
                    MediaType = MediaTypeCode.TextFile;
                    FileType = "TextFile";
                    break;
                case ".pdf":
                    MediaType = MediaTypeCode.PDF;
                    FileType = "PDF";
                    break;
                case "":
                    MediaType = MediaTypeCode.MultiPart;
                    FileType = "MultiPart";
                    break;
                default:
                    MediaType = MediaTypeCode.Unknown;
                    FileType = "Unknown";
                    break;
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_MediaType != MediaTypeCode.Unknown)
                element.Add(new XAttribute("MediaType", FileType));
            if (!String.IsNullOrEmpty(_MimeType))
                element.Add(new XAttribute("MimeType", _MimeType));
            if (_FileName != null)
                element.Add(new XAttribute("FileName", _FileName));
            if (_StorageState != MediaStorageState.Unknown)
                element.Add(new XAttribute("StorageState", _StorageState.ToString()));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "MediaType":
                    _MediaType = MediaUtilities.GetMediaTypeFromString(attributeValue);
                    break;
                case "MimeType":
                    _MimeType = attributeValue;
                    break;
                case "FileName":
                    _FileName = attributeValue;
                    break;
                case "StorageState":
                    _StorageState = ApplicationData.GetStorageStateFromString(attributeValue);
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            MediaDescription otherMediaDescription = other as MediaDescription;

            if (otherMediaDescription == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(FileType, otherMediaDescription.FileType);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_MimeType, otherMediaDescription.MimeType);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_FileName, otherMediaDescription.FileName);
            return diff;
        }

        public static int Compare(MediaDescription object1, MediaDescription object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareKeys(MediaDescription object1, MediaDescription object2)
        {
            return ObjectUtilities.CompareKeys(object1, object2);
        }

        public static int CompareMediaDescriptionLists(List<MediaDescription> list1, List<MediaDescription> list2)
        {
            return ObjectUtilities.CompareTypedObjectLists<MediaDescription>(list1, list2);
        }
    }
}
