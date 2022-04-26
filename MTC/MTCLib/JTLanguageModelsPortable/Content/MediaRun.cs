using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Content
{
    // References a segment within an audio/visual media.
    public class MediaRun : BaseObjectKeyed
    {
        protected string _FileName;
        protected MediaStorageState _StorageState;
        protected string _MediaItemKey;
        protected string _LanguageMediaItemKey;
        protected string _Owner;
        protected TimeSpan _Start;
        protected TimeSpan _Length;
        // Normal key values when in a text run.
        public static List<string> MediaRunKeys = new List<string> { "Audio", "SlowAudio", "Video", "SlowVideo", "Picture", "BigPicture", "SmallPicture" };

        public MediaRun(object key, string fileName, string mediaItemKey,
            string languageMediaItemKey, string owner, TimeSpan start, TimeSpan length)
            : base(key)
        {
            _FileName = fileName;
            _StorageState = MediaStorageState.Unknown;
            _MediaItemKey = mediaItemKey;
            _LanguageMediaItemKey = languageMediaItemKey;
            _Owner = owner;
            _Start = start;
            _Length = length;
        }

        // For media reference.
        public MediaRun(object key, string mediaItemKey, string languageMediaItemKey,
                TimeSpan start, TimeSpan length)
            : base(key)
        {
            _FileName = null;
            _StorageState = MediaStorageState.Unknown;
            _MediaItemKey = mediaItemKey;
            _LanguageMediaItemKey = languageMediaItemKey;
            _Owner = null;
            _Start = start;
            _Length = length;
        }

        // For owned media.
        public MediaRun(object key, string fileName, string owner)
            : base(key)
        {
            _FileName = fileName;
            _StorageState = MediaStorageState.Unknown;
            _MediaItemKey = null;
            _LanguageMediaItemKey = null;
            _Owner = owner;
            _Start = TimeSpan.Zero;
            _Length = TimeSpan.Zero;
        }

        public MediaRun(MediaRun other)
            : base(other)
        {
            CopyMediaRun(other);
        }

        public MediaRun(XElement element)
        {
            OnElement(element);
        }

        public MediaRun()
        {
            ClearMediaRun();
        }

        public override void Clear()
        {
            base.Clear();
            ClearMediaRun();
        }

        public void ClearMediaRun()
        {
            _FileName = null;
            _StorageState = MediaStorageState.Unknown;
            _MediaItemKey = null;
            _LanguageMediaItemKey = null;
            _Owner = null;
            _Start = TimeSpan.Zero;
            _Length = TimeSpan.Zero;
        }

        public void CopyMediaRun(MediaRun other)
        {
            Key = other.Key;
            FileName = other.FileName;
            StorageState = other.StorageState;
            MediaItemKey = other.MediaItemKey;
            LanguageMediaItemKey = other.LanguageMediaItemKey;
            Owner = other.Owner;
            Start = other.Start;
            Length = other.Length;
        }

        public void JoinMediaRun(MediaRun other)
        {
            TimeSpan newStart;
            TimeSpan newLength;

            if (other.Start < Start)
                newStart = other.Start;
            else
                newStart = Start;

            if (other.Stop > Stop)
                newLength = other.Stop - newStart;
            else
                newLength = Stop - newStart;

            Start = newStart;
            Length = newLength;
        }

        public void Merge(MediaRun other)
        {
            JoinMediaRun(other);

            if (_Key == null)
                Key = other.Key;

            if (String.IsNullOrEmpty(_FileName))
                _FileName = other.FileName;
            else if (!String.IsNullOrEmpty(other.FileName))
                _FileName += "," + other.FileName;

            _StorageState = other.StorageState;
            _MediaItemKey = other.MediaItemKey;
            _LanguageMediaItemKey = other.LanguageMediaItemKey;
            _Owner = other.Owner;
        }

        public void ConvertToFile(string fileName)
        {
            MediaItemKey = null;
            LanguageMediaItemKey = null;
            Start = TimeSpan.Zero;
            Length = TimeSpan.Zero;
            FileName = fileName;
        }

        public override IBaseObject Clone()
        {
            return new MediaRun(this);
        }

        public string FileName
        {
            get
            {
                return _FileName;
            }
            set
            {
                if (value != _FileName)
                {
                    ModifiedFlag = true;
                    _FileName = value;
                }
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

        public bool IsHasFileName
        {
            get
            {
                return !string.IsNullOrEmpty(_FileName);
            }
        }

        public bool IsFullUrl
        {
            get
            {
                if (!String.IsNullOrEmpty(_FileName) && _FileName.Contains(":"))
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

        public bool IsTildeUrl
        {
            get
            {
                if (!String.IsNullOrEmpty(_FileName) && _FileName.StartsWith("~/"))
                    return true;

                return false;
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
            else if (url.StartsWith("~"))
                return url;

            if (String.IsNullOrEmpty(directoryUrl))
                directoryUrl = "~/";

            if (url.StartsWith("../"))
            {
                directoryUrl = MediaUtilities.RemoveLastNode(directoryUrl);
                url = MediaUtilities.RemoveFirstNode(url);
                string parent = MediaUtilities.GetFirstNode(url);
                directoryUrl = MediaUtilities.ConcatenateUrlPath(directoryUrl, parent);
                url = MediaUtilities.RemoveFirstNode(url);
            }

            if (!directoryUrl.EndsWith("/"))
                directoryUrl += "/";

            url = directoryUrl + url;

            return url;
        }

        public string GetDirectoryPath(string directoryUrl)
        {
            string url = GetUrl(directoryUrl);
            string path = ApplicationData.MapToFilePath(url);
            return path;
        }

        public bool IsReference
        {
            get
            {
                return !String.IsNullOrEmpty(_MediaItemKey);
            }
        }

        public string MediaItemKey
        {
            get
            {
                return _MediaItemKey;
            }
            set
            {
                if (value != _MediaItemKey)
                {
                    ModifiedFlag = true;
                    _MediaItemKey = value;
                }
            }
        }

        public string LanguageMediaItemKey
        {
            get
            {
                return _LanguageMediaItemKey;
            }
            set
            {
                if (value != _LanguageMediaItemKey)
                {
                    ModifiedFlag = true;
                    _LanguageMediaItemKey = value;
                }
            }
        }

        public string MimeType
        {
            get
            {
                if (String.IsNullOrEmpty(_FileName))
                    return String.Empty;
                switch (KeyString)
                {
                    case "Audio":
                    case "SlowAudio":
                        return MediaUtilities.GetMimeTypeFromMediaTypeAndFileName("audio", _FileName);
                    case "Video":
                    case "SlowVideo":
                        return MediaUtilities.GetMimeTypeFromMediaTypeAndFileName("video", _FileName);
                    case "Picture":
                    case "BigPicture":
                    case "SmallPicture":
                        return MediaUtilities.GetMimeTypeFromMediaTypeAndFileName("image", _FileName);
                    default:
                        return String.Empty;
                }
            }
        }

        public override string Owner
        {
            get
            {
                return _Owner;
            }
            set
            {
                if (value != _Owner)
                {
                    ModifiedFlag = true;
                    _Owner = value;
                }
            }
        }

        public TimeSpan Start
        {
            get
            {
                return _Start;
            }
            set
            {
                if (value != _Start)
                {
                    ModifiedFlag = true;
                    _Start = value;
                }
            }
        }

        public TimeSpan Stop
        {
            get
            {
                return _Start + Length;
            }

            set
            {
                if (value >= _Start)
                    Length = value - _Start;
                else
                    Length = TimeSpan.Zero;
            }
        }

        public TimeSpan Length
        {
            get
            {
                return _Length;
            }
            set
            {
                if (value != _Length)
                {
                    ModifiedFlag = true;
                    _Length = value;
                }
            }
        }

        public bool IsEmpty()
        {
            if (_Length == TimeSpan.Zero)
                return true;

            return false;
        }

        public bool IsSegment()
        {
            if (_Length == TimeSpan.Zero)
                return false;

            return true;
        }

        public bool IsAudio()
        {
            switch (KeyString)
            {
                case "Audio":
                case "SlowAudio":
                    return true;
                case "Video":
                case "SlowVideo":
                    return false;
                case "Picture":
                case "BigPicture":
                case "SmallPicture":
                    return false;
                default:
                    return false;
            }
        }

        public bool IsVideo()
        {
            switch (KeyString)
            {
                case "Audio":
                case "SlowAudio":
                    return true;
                case "Video":
                case "SlowVideo":
                    return false;
                case "Picture":
                case "BigPicture":
                case "SmallPicture":
                    return false;
                default:
                    return false;
            }
        }

        public bool IsAudioVideo()
        {
            switch (KeyString)
            {
                case "Audio":
                case "SlowAudio":
                    return true;
                case "Video":
                case "SlowVideo":
                    return true;
                case "Picture":
                case "BigPicture":
                case "SmallPicture":
                    return false;
                default:
                    return false;
            }
        }

        public bool IsPicture()
        {
            switch (KeyString)
            {
                case "Audio":
                case "SlowAudio":
                    return false;
                case "Video":
                case "SlowVideo":
                    return false;
                case "Picture":
                case "BigPicture":
                case "SmallPicture":
                    return true;
                default:
                    return false;
            }
        }

        public bool IsSlow()
        {
            switch (KeyString)
            {
                case "SlowAudio":
                case "SlowVideo":
                    return true;
                case "Audio":
                case "Video":
                case "Picture":
                case "BigPicture":
                case "SmallPicture":
                default:
                    return false;
            }
        }

        // Return true if any audio/video media.  
        // Note: Media references will be collapsed to a "(url)#t(start),(end)" url.
        public bool GetMediaInfo(string mediaRunKey, string mediaPathUrl, BaseObjectNode node,
            out bool hasAudio, out bool hasVideo, out bool hasSlow, out bool hasPicture,
            List<string> audioVideoUrls)
        {
            bool hasMedia = false;

            hasAudio = false;
            hasVideo = false;
            hasSlow = false;
            hasPicture = false;

            switch (KeyString)
            {
                case "SlowAudio":
                    hasAudio = true;
                    hasSlow = true;
                    break;
                case "SlowVideo":
                    hasVideo = true;
                    hasSlow = true;
                    break;
                case "Audio":
                    hasAudio = true;
                    break;
                case "Video":
                    hasVideo = true;
                    break;
                case "Picture":
                case "BigPicture":
                case "SmallPicture":
                    hasPicture = true;
                    break;
                default:
                    return false;
            }

            hasMedia = CollectMediaUrls(mediaRunKey, mediaPathUrl, node, null, audioVideoUrls, null);

            return hasMedia;
        }

        static char[] Pound = new char[] { '#' };
        static char[] TComma = new char[] { 't', 'T', ',' };

        // Note: Media references will be collapsed to a "(url)#t(start),(end)" url.
        public bool CollectMediaUrls(string mediaRunKey, string mediaPathUrl, BaseObjectNode node,
            object content, List<string> mediaUrls, VisitMedia visitFunction)
        {
            bool returnValue = true;

            if (String.IsNullOrEmpty(mediaRunKey))
            {
                if (!IsAudioVideo())
                    return false;
            }
            else if (KeyString != mediaRunKey)
                return false;

            string url = String.Empty;

            if (IsReference)
            {
                if (node == null)
                    returnValue = false;
                else
                {
                    ContentMediaItem mediaItem = node.GetMediaItem(_MediaItemKey);

                    if (mediaItem != null)
                    {
                        if ((mediaItem.PlayerSource == "File") || (mediaItem.PlayerSource == "Cloud"))
                        {
                            if (mediaItem.LanguageMediaItemCount() != 0)
                            {
                                LanguageMediaItem languageMediaItem = mediaItem.GetLanguageMediaItem(_LanguageMediaItemKey);
                                if (languageMediaItem != null)
                                {
                                    MediaDescription mediaDescription = languageMediaItem.GetMediaDescriptionIndexed(0);
                                    if (mediaDescription != null)
                                        url = mediaDescription.GetUrl(languageMediaItem.MediaTildeUrl);
                                }
                            }
                        }
                        else if (mediaItem.PlayerSource == "YouTube")
                        {
                            if (mediaItem.LanguageMediaItemCount() != 0)
                            {
                                LanguageMediaItem languageMediaItem = mediaItem.GetLanguageMediaItem(_LanguageMediaItemKey);
                                if (languageMediaItem != null)
                                {
                                    MediaDescription mediaDescription = languageMediaItem.GetAudioMediaDescription();
                                    if (mediaDescription != null)
                                        url = mediaDescription.GetUrl(languageMediaItem.MediaTildeUrl);
                                }
                            }
                        }
                        else
                            returnValue = false;
                    }
                    else
                        returnValue = false;
                }
            }
            else if (IsFullUrl || IsTildeUrl)
                url = _FileName;
            else if (!IsHasFileName)
                return false;
            else
            {
                url = mediaPathUrl;

                if (!url.EndsWith("/"))
                    url += "/";

                url = url + _FileName;
            }

            if (returnValue && (_Length != TimeSpan.Zero))
            {
                double start = Start.TotalSeconds;
                double end = Stop.TotalSeconds;

                // We basically have to merge in this run with a previous run from the same reference, if the reference
                // is last in the current list.
                if ((mediaUrls.Count() != 0) && mediaUrls.Last().StartsWith(url) && mediaUrls.Last().Contains("#"))
                {
                    string[] split1 = mediaUrls.Last().Split(Pound);
                    url = split1[0];
                    string[] split2 = split1[1].Split(TComma, StringSplitOptions.RemoveEmptyEntries);
                    double otherStart = Convert.ToDouble(split2[0]);
                    double otherEnd = Convert.ToDouble(split2[1]);

                    if (otherStart < start)
                        start = otherStart;

                    if (otherEnd > end)
                        end = otherEnd;

                    mediaUrls.RemoveAt(mediaUrls.Count() - 1);
                }

                url += "#t" + start.ToString() + "," + end.ToString();
            }

            if (returnValue)
            {
                if (visitFunction != null)
                    visitFunction(mediaUrls, content, this, url, null);
                else if (mediaUrls != null)
                {
                    if (!mediaUrls.Contains(url))
                        mediaUrls.Add(url);
                }
            }

            return returnValue;
        }

        public void CollectMediaFiles(string directoryUrl, List<string> mediaFiles, object content, VisitMedia visitFunction)
        {
            if (IsReference || IsFullUrl)
                return;

            string filePath = GetDirectoryPath(directoryUrl);

            if (String.IsNullOrEmpty(filePath))
                return;

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
            if (IsReference)
                return true;

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

            try
            {
                FileSingleton.DirectoryExistsCheck(targetFilePath);
                FileSingleton.Copy(sourceFilePath, targetFilePath, true);

                if (copiedFiles != null)
                    copiedFiles.Add(targetFilePath);
            }
            catch (Exception exception)
            {
                errorMessage = (!String.IsNullOrEmpty(errorMessage) ? errorMessage + "\n" : String.Empty) +
                    "Error copying " + sourceFilePath + " to " + targetFilePath + ": " +
                    exception.Message;
                return false;
            }

            return true;
        }

        public void ClearTimes()
        {
            _Start = TimeSpan.Zero;
            _Length = TimeSpan.Zero;
        }

        public bool Contains(TimeSpan time)
        {
            if ((time >= _Start) && (time < Stop))
                return true;

            return false;
        }

        public static TimeSpan FuzzyDelta = new TimeSpan(0, 0, 0, 0, 5);

        public bool FuzzyContains(TimeSpan time)
        {
            if (_Length.Ticks != 0)
            {
                if ((time >= _Start - FuzzyDelta) && (time < Stop + FuzzyDelta))
                    return true;
            }

            return false;
        }

        public bool OverlapsTime(TimeSpan mediaStartTime, TimeSpan mediaStopTime)
        {
            if (Stop < mediaStartTime)
                return false;
            if (Start > mediaStopTime)
                return false;
            return true;
        }

        public bool GetIntersection(
            TimeSpan mediaStartTime,
            TimeSpan mediaStopTime,
            out TimeSpan intersection)
        {
            intersection = TimeSpan.Zero;

            if (Stop < mediaStartTime)
                return false;

            if (Start > mediaStopTime)
                return false;

            TimeSpan commonStart;
            TimeSpan commonStop;

            if (mediaStartTime < Start)
                commonStart = Start;
            else
                commonStart = mediaStartTime;

            if (mediaStopTime < Stop)
                commonStop = mediaStopTime;
            else
                commonStop = Stop;

            intersection = commonStop - commonStart;

            return true;
        }

        public override string ToString()
        {
            return _Start.ToString() + " " + _Length.ToString();
        }

        public void GetDumpString(string prefix, StringBuilder sb)
        {
            sb.AppendLine(prefix + "Key: " + KeyString);
            sb.AppendLine(prefix + "FileName: " + FileName);
            sb.AppendLine(prefix + "Owner: " + Owner);
            sb.AppendLine(prefix + "Start: " + Start.ToString());
            sb.AppendLine(prefix + "Length: " + Length.ToString());
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_FileName != null)
                element.Add(new XAttribute("FileName", _FileName));
            if (_StorageState != MediaStorageState.Unknown)
                element.Add(new XAttribute("StorageState", _StorageState.ToString()));
            if (!String.IsNullOrEmpty(_MediaItemKey))
                element.Add(new XAttribute("MediaItemKey", _MediaItemKey));
            if (!String.IsNullOrEmpty(_LanguageMediaItemKey))
                element.Add(new XAttribute("LanguageMediaItemKey", _LanguageMediaItemKey));
            if (_Owner != null)
                element.Add(new XAttribute("Owner", _Owner));
            element.Add(new XAttribute("Start", _Start.ToString()));
            element.Add(new XAttribute("Length", _Length.ToString()));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "FileName":
                    _FileName = attributeValue;
                    break;
                case "StorageState":
                    _StorageState = ApplicationData.GetStorageStateFromString(attributeValue);
                    break;
                case "MediaItemKey":
                    _MediaItemKey = attributeValue;;
                    break;
                case "LanguageMediaItemKey":
                    _LanguageMediaItemKey = attributeValue;
                    break;
                case "Owner":
                    _Owner = attributeValue;
                    break;
                case "Start":
                    _Start = TimeSpan.Parse(attributeValue);
                    break;
                case "Length":
                    _Length = TimeSpan.Parse(attributeValue);
                    break;
                // Legacy:
                case "ReferenceKey":
                    _MediaItemKey = attributeValue;
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            MediaRun otherMediaRun = other as MediaRun;

            if (otherMediaRun == null)
                return base.Compare(other);

            int diff = base.Compare(other);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_MediaItemKey, otherMediaRun.MediaItemKey);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_LanguageMediaItemKey, otherMediaRun.LanguageMediaItemKey);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_FileName, otherMediaRun.FileName);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareStrings(_Owner, otherMediaRun.Owner);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareTimeSpans(_Start, otherMediaRun.Start);
            if (diff != 0)
                return diff;
            diff = ObjectUtilities.CompareTimeSpans(_Length, otherMediaRun.Length);
            if (diff != 0)
                return diff;
            return 0;
        }

        public static int Compare(MediaRun object1, MediaRun object2)
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
