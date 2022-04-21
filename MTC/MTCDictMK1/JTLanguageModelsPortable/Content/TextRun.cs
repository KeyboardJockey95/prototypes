using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Content
{
    public class TextRun : BaseObject
    {
        protected int _Start;
        protected int _Length;
        protected List<MediaRun> _MediaRuns;
        protected List<WordMapping> _WordMappings;
        protected List<LanguageID> _PhraseHostLanguageIDs;
        protected DictionaryEntry _CachedDictionaryEntry;   // Not saved.
        protected SentenceParsingInfo _ParsingInfo;         // Not saved.
        protected bool _Modified;

        public TextRun(int start, int length, List<MediaRun> mediaRuns)
        {
            _Start = start;
            _Length = length;
            _MediaRuns = mediaRuns;
            _WordMappings = null;
            _PhraseHostLanguageIDs = null;
            _CachedDictionaryEntry = null;
            _ParsingInfo = null;
            _Modified = false;
        }

        public TextRun(TextRun other)
        {
            _Start = other.Start;
            _Length = other.Length;
            _MediaRuns = other.CloneMediaRuns();
            _WordMappings = other.CloneWordMappings();
            _PhraseHostLanguageIDs = other.ClonePhraseHostLanguageIDs();
            _CachedDictionaryEntry = other.CachedDictionaryEntry;
            _ParsingInfo = other.ParsingInfo;
            _Modified = false;
        }

        public TextRun(LanguageItem languageItem, MediaRun mediaRun)
        {
            _Start = 0;
            _Length = languageItem.Text.Length;

            if (mediaRun != null)
                _MediaRuns = new List<MediaRun>(1) { mediaRun };
            else
                _MediaRuns = null;

            _WordMappings = null;
            _PhraseHostLanguageIDs = null;
            _CachedDictionaryEntry = null;
            _ParsingInfo = null;
            _Modified = false;
        }

        public TextRun(XElement element)
        {
            _WordMappings = null;
            _PhraseHostLanguageIDs = null;
            _CachedDictionaryEntry = null;
            _ParsingInfo = null;
            OnElement(element);
            _Modified = false;
        }

        public TextRun()
        {
            _Start = 0;
            _Length = 0;
            _MediaRuns = null;
            _WordMappings = null;
            _PhraseHostLanguageIDs = null;
            _CachedDictionaryEntry = null;
            _ParsingInfo = null;
            _Modified = false;
        }

        public override void Clear()
        {
            base.Clear();
            ClearTextRun();
        }

        public void ClearTextRun()
        {
            _Start = 0;
            _Length = 0;
            _MediaRuns = null;
            _WordMappings = null;
            _PhraseHostLanguageIDs = null;
            _CachedDictionaryEntry = null;
            _ParsingInfo = null;
            _Modified = false;
        }

        public override IBaseObject Clone()
        {
            return new TextRun(this);
        }

        public void Merge(TextRun other)
        {
            if (Stop != other.Start)
            {
                int diff = other.Start - Stop;

                if (diff > 0)
                    _Length += diff;
            }

            _Length += other.Length;

            if ((other.MediaRuns != null) && (other.MediaRuns.Count() != 0))
            {
                foreach (MediaRun otherMediaRun in other.MediaRuns)
                {
                    if (_MediaRuns == null)
                        _MediaRuns = new List<MediaRun>();

                    _MediaRuns.Add(new MediaRun(otherMediaRun));
                }
            }

            _WordMappings = null;
            _PhraseHostLanguageIDs = null;
            _CachedDictionaryEntry = null;
        }

        public bool MergeMediaRuns(TextRun other)
        {
            bool returnValue = true;

            if ((other.MediaRuns != null) && (other.MediaRuns.Count() != 0))
            {
                foreach (MediaRun otherMediaRun in other.MediaRuns)
                {
                    if (_MediaRuns == null)
                        _MediaRuns = new List<MediaRun>();

                    if (_MediaRuns.FirstOrDefault(x => MediaRun.Compare(x, otherMediaRun) == 0) == null)
                        _MediaRuns.Add(new MediaRun(otherMediaRun));
                }
            }

            return returnValue;
        }

        public bool IntegrateMediaRuns(List<MediaRun> mediaRuns, bool replaceSameKey)
        {
            bool returnValue = true;

            if ((mediaRuns != null) && (mediaRuns.Count() != 0))
            {
                foreach (MediaRun otherMediaRun in mediaRuns)
                {
                    if (_MediaRuns == null)
                        _MediaRuns = new List<MediaRun>();

                    bool done = false;

                    foreach (MediaRun thisMediaRun in _MediaRuns)
                    {
                        if (replaceSameKey)
                        {
                            if (thisMediaRun.KeyString == otherMediaRun.KeyString)
                            {
                                thisMediaRun.CopyDeep(otherMediaRun);
                                done = true;
                            }
                        }
                        else
                        {
                            if (thisMediaRun.Compare(otherMediaRun) == 0)
                                done = true;
                        }
                    }

                    if (!done)
                        _MediaRuns.Add(new MediaRun(otherMediaRun));
                }
            }

            return returnValue;
        }

        public int Start
        {
            get
            {
                return _Start;
            }
            set
            {
                if (_Start != value)
                {
                    _Start = value;
                    _Modified = true;
                }
            }
        }

        public int Stop
        {
            get
            {
                return _Start + Length;
            }

            set
            {
                int newLength = _Length;
                if (value >= _Start)
                    newLength = value - _Start;
                else
                    newLength = 0;
                if (newLength != _Length)
                {
                    _Length = newLength;
                    _Modified = true;
                }
            }
        }

        public int Length
        {
            get
            {
                return _Length;
            }
            set
            {
                if (_Length != value)
                {
                    _Length = value;
                    _Modified = true;
                }
            }
        }

        public List<MediaRun> MediaRuns
        {
            get
            {
                return _MediaRuns;
            }
            set
            {
                if (value != _MediaRuns)
                {
                    _Modified = true;
                    _MediaRuns = value;
                }
            }
        }

        public int MediaRunCount()
        {
            if (_MediaRuns == null)
                return 0;

            return _MediaRuns.Count();
        }

        public List<MediaRun> CloneMediaRuns()
        {
            List<MediaRun> mediaRuns = new List<MediaRun>();

            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                    mediaRuns.Add(new MediaRun(mediaRun));
            }

            return mediaRuns;
        }

        public List<MediaRun> CloneAndRetargetMediaRuns(string relativePath)
        {
            List<MediaRun> mediaRuns = new List<MediaRun>();

            if (_MediaRuns != null)
            {
                if (!String.IsNullOrEmpty(relativePath) && !relativePath.EndsWith("/"))
                    relativePath += "/";

                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    MediaRun newMediaRun = new MediaRun(mediaRun);

                    if (!newMediaRun.IsReference)
                        newMediaRun.FileName = relativePath + newMediaRun.FileName;

                    mediaRuns.Add(newMediaRun);
                }
            }

            return mediaRuns;
        }

        // Return true if any audio/video media.
        public bool HasAudioVideo()
        {
            bool hasAudioVideo = false;

            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if (mediaRun.IsAudioVideo())
                    {
                        hasAudioVideo = true;
                        break;
                    }
                }
            }

            return hasAudioVideo;
        }

        // Return true if any audio/video media.  
        // Note: Media references will be collapsed to a "(url)#t(start),(end)" url.
        public bool GetMediaInfo(string mediaRunKey, string mediaPathUrl, BaseObjectNode node,
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

            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
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

            return returnValue;
        }

        public bool HasMediaRunWithKey(object key)
        {
            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if (mediaRun.CompareKey(key) == 0)
                        return true;
                }
            }

            return false;
        }

        public bool HasMediaRunWithUrl(string fileName)
        {
            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if (mediaRun.FileName == fileName)
                        return true;
                }
            }

            return false;
        }

        public bool HasMediaRunWithReferenceKeys(string mediaItemKey, string languageMediaItemKey)
        {
            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if ((mediaRun.MediaItemKey == mediaItemKey) &&
                            (mediaRun.LanguageMediaItemKey == languageMediaItemKey))
                        return true;
                }
            }

            return false;
        }

        public MediaRun GetMediaRunWithKey(object key)
        {
            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if (mediaRun.CompareKey(key) == 0)
                        return mediaRun;
                }
            }

            return null;
        }

        public MediaRun GetMediaRun(object key)
        {
            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if (mediaRun.CompareKey(key) == 0)
                    {
                        if (mediaRun.IsReference)
                            return mediaRun;
                        else if (mediaRun.IsHasFileName)
                            return mediaRun;
                    }
                }
            }

            return null;
        }

        public MediaRun GetMediaRunNonReference(object key)
        {
            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if (mediaRun.CompareKey(key) == 0)
                    {
                        if (!mediaRun.IsReference && mediaRun.IsHasFileName)
                            return mediaRun;
                    }
                }
            }

            return null;
        }

        public MediaRun GetMediaRun(object key, int mediaRunIndex)
        {
            if (_MediaRuns != null)
            {
                int index = 0;

                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if (mediaRun.CompareKey(key) == 0)
                    {
                        if (mediaRun.IsReference || mediaRun.IsHasFileName)
                        {
                            if (index == mediaRunIndex)
                                return mediaRun;

                            index++;
                        }
                    }
                }
            }

            return null;
        }

        public MediaRun GetMediaRunWithUrl(string fileName)
        {
            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if (mediaRun.FileName == fileName)
                        return mediaRun;
                }
            }

            return null;
        }

        public MediaRun GetMediaRunWithReferenceKeys(string mediaItemKey, string languageMediaItemKey)
        {
            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if ((mediaRun.MediaItemKey == mediaItemKey) &&
                            (mediaRun.LanguageMediaItemKey == languageMediaItemKey))
                        return mediaRun;
                }
            }

            return null;
        }

        public MediaRun GetMediaRunWithKeyAndReferenceKeys(object key, string mediaItemKey, string languageMediaItemKey)
        {
            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if (mediaRun.CompareKey(key) == 0)
                    {
                        if ((mediaRun.MediaItemKey == mediaItemKey) &&
                                (mediaRun.LanguageMediaItemKey == languageMediaItemKey))
                            return mediaRun;
                    }
                }
            }

            return null;
        }

        public void GetMediaRunsWithReferenceKeys(List<MediaRun> mediaRuns,
            string mediaItemKey, string languageMediaItemKey)
        {
            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if ((mediaRun.MediaItemKey == mediaItemKey) &&
                            (mediaRun.LanguageMediaItemKey == languageMediaItemKey))
                        ObjectUtilities.ListAddUnique(mediaRuns, mediaRun);
                }
            }
        }

        public MediaRun GetMediaRunCorresponding(MediaRun mediaRun)
        {
            if (_MediaRuns != null)
            {
                foreach (MediaRun aMediaRun in _MediaRuns)
                {
                    if (aMediaRun.CompareKey(mediaRun.Key) == 0)
                    {
                        if (aMediaRun.IsReference && mediaRun.IsReference)
                        {
                            if ((aMediaRun.MediaItemKey == aMediaRun.MediaItemKey) &&
                                    (aMediaRun.LanguageMediaItemKey == mediaRun.LanguageMediaItemKey))
                                return aMediaRun;
                        }
                        else if (!aMediaRun.IsReference && !mediaRun.IsReference)
                            return aMediaRun;
                    }
                }
            }

            return null;
        }

        public MediaRun GetMediaRunAny(object key)
        {
            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if (mediaRun.CompareKey(key) == 0)
                        return mediaRun;
                }
            }

            return null;
        }

        public int GetMediaRunCount(string mediaRunKey)
        {
            int count = 0;

            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if (mediaRun.KeyString == mediaRunKey)
                        count++;
                }
            }

            return count;
        }

        // Re-map reference media run times to target time.
        // Target time advanced to end of mapped time period.
        public void RemapReferenceMediaRuns(
            string mediaRunKey,
            string mediaItemKey,
            string languageMediaItemKey,
            TimeSpan timeToSubtract)
        {
            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if (mediaRun.KeyString == mediaRunKey)
                    {
                        if (mediaRun.MediaItemKey == mediaItemKey)
                        {
                            if (mediaRun.LanguageMediaItemKey == languageMediaItemKey)
                                mediaRun.Start = mediaRun.Start - timeToSubtract;
                        }
                    }
                }
            }
        }

        // Note: Media references will be collapsed to a "(url)#t(start),(end)" url.
        // Returns true if any media found.
        public bool CollectMediaUrls(string mediaRunKey, string mediaPathUrl, BaseObjectNode node,
            object content, List<string> mediaUrls, VisitMedia visitFunction)
        {
            bool returnValue = false;

            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if (mediaRun.CollectMediaUrls(mediaRunKey, mediaPathUrl, node, content, mediaUrls, visitFunction))
                        returnValue = true;
                }
            }

            return returnValue;
        }

        public void CollectMediaFiles(string directoryUrl, List<string> mediaFiles, object content, VisitMedia visitFunction)
        {
            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                    mediaRun.CollectMediaFiles(directoryUrl, mediaFiles, content, visitFunction);
            }
        }

        public void CollectMediaReferences(List<MediaRun> mediaRuns)
        {
            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if (mediaRun.IsReference)
                        mediaRuns.Add(mediaRun);
                }
            }
        }

        public bool CopyMedia(string sourceMediaDirectory, string targetDirectoryRoot, List<string> copiedFiles,
            ref string errorMessage)
        {
            bool returnValue = true;

            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                {
                    if (!mediaRun.CopyMedia(sourceMediaDirectory, targetDirectoryRoot, copiedFiles, ref errorMessage))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public MediaRun GetMediaRunIndexed(int index)
        {
            if (_MediaRuns != null)
            {
                if ((index >= 0) && (index < _MediaRuns.Count()))
                    return _MediaRuns[index];
            }

            return null;
        }

        public void AddMediaRun(MediaRun mediaRun)
        {
            if (_MediaRuns == null)
                _MediaRuns = new List<MediaRun>() { mediaRun };
            else
                _MediaRuns.Add(mediaRun);

            _Modified = true;
        }

        public void InsertMediaRun(int index, MediaRun mediaRun)
        {
            if (_MediaRuns == null)
                _MediaRuns = new List<MediaRun>() { mediaRun };
            else if ((index >= 0) && (index <= _MediaRuns.Count()))
                _MediaRuns.Insert(index, mediaRun);

            _Modified = true;
        }

        public bool MoveUpMediaRun(MediaRun mediaRun)
        {
            if (_MediaRuns == null)
                return false;
            else
            {
                int index = _MediaRuns.IndexOf(mediaRun);
                if (index > 0)
                {
                    _MediaRuns.RemoveAt(index);
                    index--;
                    _MediaRuns.Insert(index, mediaRun);
                }
                else
                    return false;
            }
            _Modified = true;
            return true;
        }

        public bool MoveDownMediaRun(MediaRun mediaRun)
        {
            if (_MediaRuns == null)
                return false;
            else
            {
                int index = _MediaRuns.IndexOf(mediaRun);
                if ((index >= 0) && (index < _MediaRuns.Count() - 1))
                {
                    _MediaRuns.RemoveAt(index);
                    index++;
                    _MediaRuns.Insert(index, mediaRun);
                }
                else
                    return false;
            }
            _Modified = true;
            return true;
        }

        public bool DeleteMediaRun(MediaRun mediaRun)
        {
            if (_MediaRuns == null)
                return false;

            if (_MediaRuns.Remove(mediaRun))
            {
                _Modified = true;
                return true;
            }

            return false;
        }

        // Returns true if any media runs removed. Use languageMediaItem = null to remove
        // media runs referencing the media item.
        public bool DeleteMediaRunsWithReferenceKeys(string mediaItemKey, string languageMediaItemKey)
        {
            bool returnValue = false;

            if (_MediaRuns != null)
            {
                int index = _MediaRuns.Count() - 1;

                while (index >= 0)
                {
                    MediaRun mediaRun = _MediaRuns[index];

                    if ((mediaRun.MediaItemKey == mediaItemKey) &&
                        ((languageMediaItemKey == null) || (mediaRun.LanguageMediaItemKey == languageMediaItemKey)))
                    {
                        _MediaRuns.RemoveAt(index);
                        _Modified = true;
                        returnValue = true;
                    }

                    index--;
                }
            }

            return returnValue;
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItemKey)
        {
            DeleteMediaRunsWithReferenceKeys(mediaItemKey, languageMediaItemKey);
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItemKey,
            TimeSpan mediaStartTime, TimeSpan mediaStopTime)
        {
            if (_MediaRuns != null)
            {
                int index = _MediaRuns.Count() - 1;

                while (index >= 0)
                {
                    MediaRun mediaRun = _MediaRuns[index];

                    if ((mediaRun.MediaItemKey == mediaItemKey) &&
                        ((languageMediaItemKey == null) || (mediaRun.LanguageMediaItemKey == languageMediaItemKey)))
                    {
                        if (mediaRun.OverlapsTime(mediaStartTime, mediaStopTime))
                        {
                            _MediaRuns.RemoveAt(index);
                            _Modified = true;
                        }
                    }

                    index--;
                }
            }
        }

        public List<WordMapping> WordMappings
        {
            get
            {
                return _WordMappings;
            }
            set
            {
                if (value != _WordMappings)
                {
                    _Modified = true;
                    _WordMappings = value;
                }
            }
        }

        public int WordMappingCount()
        {
            if (_WordMappings == null)
                return 0;

            return _WordMappings.Count();
        }

        public List<WordMapping> CloneWordMappings()
        {
            List<WordMapping> wordMappings = new List<WordMapping>();

            if (_WordMappings != null)
            {
                foreach (WordMapping wordMapping in _WordMappings)
                    wordMappings.Add(new WordMapping(wordMapping));
            }

            return wordMappings;
        }

        public bool HasAlignment(LanguageID languageID)
        {
            if (_WordMappings == null)
                return false;

            if (languageID == null)
                return false;

            string languageCode = languageID.LanguageCode;
            WordMapping wordMapping = _WordMappings.FirstOrDefault(x => x.LanguageCode == languageCode);

            if (wordMapping == null)
                return false;

            if (!wordMapping.HasWordIndexes())
                return false;

            return true;
        }

        public WordMapping GetWordMapping(LanguageID languageID)
        {
            return GetWordMapping(languageID.LanguageCultureExtensionCode);
        }

        public WordMapping GetWordMapping(string languageCode)
        {
            if (_WordMappings == null)
                return null;

            WordMapping wordMapping = _WordMappings.FirstOrDefault(x => x.LanguageCode == languageCode);

            return wordMapping;
        }

        public void SetWordMapping(WordMapping wordMapping)
        {
            if (_WordMappings == null)
                _WordMappings = new List<WordMapping>() { wordMapping };
            else
            {
                WordMapping oldWordMapping = _WordMappings.FirstOrDefault(x => x.LanguageCode == wordMapping.LanguageCode);

                if (oldWordMapping != null)
                {
                    int index = _WordMappings.IndexOf(oldWordMapping);
                    _WordMappings[index] = wordMapping;
                }
                else
                    _WordMappings.Add(wordMapping);
            }

            _Modified = true;
        }

        public void DeleteWordMapping(List<LanguageID> languageIDs)
        {
            foreach (LanguageID languageID in languageIDs)
                DeleteWordMapping(languageID.LanguageCultureExtensionCode);
        }

        public void DeleteWordMapping(LanguageID languageID)
        {
            DeleteWordMapping(languageID.LanguageCultureExtensionCode);
        }

        public void DeleteWordMapping(string languageCode)
        {
            if (_WordMappings == null)
                return;

            for (int index = 0; index < _WordMappings.Count(); index++)
            {
                if (_WordMappings[index].LanguageCode == languageCode)
                {
                    _WordMappings.RemoveAt(index);
                    _Modified = true;
                }
            }
        }

        public string GetWordMappingsString()
        {
            if (_WordMappings == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();
            bool firstWordMapping = true;

            foreach (WordMapping wordMapping in _WordMappings)
            {
                if (firstWordMapping)
                    firstWordMapping = false;
                else
                    sb.Append(';');

                sb.Append(wordMapping.LanguageCode);
                sb.Append(',');

                bool firstIndex = true;

                foreach (int index in wordMapping.WordIndexes)
                {
                    if (!firstIndex)
                        sb.Append(',');

                    sb.Append(index.ToString());

                    firstIndex = false;
                }
            }

            return sb.ToString();
        }

        public void SetWordMappingsString(string str)
        {
            _Modified = true;

            if (String.IsNullOrEmpty(str))
            {
                _WordMappings = null;
                return;
            }

            if (_WordMappings == null)
                _WordMappings = new List<WordMapping>();
            else
                _WordMappings.Clear();

            string[] majorParts = str.Split(LanguageLookup.Semicolon);

            foreach (string majorPart in majorParts)
            {
                if (String.IsNullOrEmpty(majorPart))
                    continue;

                string[] minorParts = majorPart.Split(LanguageLookup.Comma);
                int minorCount = minorParts.Length;
                string languageCode = minorParts[0];
                List<int> indexes = new List<int>();

                for (int i = 1; i < minorCount; i++)
                {
                    int index = ObjectUtilities.GetIntegerFromString(minorParts[i], 0);
                    indexes.Add(index);
                }

                WordMapping wordMapping = new WordMapping(languageCode, indexes.ToArray());
                _WordMappings.Add(wordMapping);
            }
        }

        public List<LanguageID> PhraseHostLanguageIDs
        {
            get
            {
                return _PhraseHostLanguageIDs;
            }
            set
            {
                if (value != _PhraseHostLanguageIDs)
                {
                    _Modified = true;
                    _PhraseHostLanguageIDs = value;
                }
            }
        }

        public int PhraseHostLanguageIDsCount()
        {
            if (_PhraseHostLanguageIDs == null)
                return 0;

            return _PhraseHostLanguageIDs.Count();
        }

        public List<LanguageID> ClonePhraseHostLanguageIDs()
        {
            List<LanguageID> wordMappings = new List<LanguageID>();

            if (_PhraseHostLanguageIDs != null)
            {
                foreach (LanguageID wordMapping in _PhraseHostLanguageIDs)
                    wordMappings.Add(new LanguageID(wordMapping));
            }

            return wordMappings;
        }

        public bool HasPhraseHostLanguageID(LanguageID languageID)
        {
            if (_PhraseHostLanguageIDs == null)
                return false;

            if (languageID == null)
                return false;

            if (_PhraseHostLanguageIDs.Contains(languageID))
                return true;

            return false;
        }

        public LanguageID GetPhraseHostLanguageIDIndexed(int index)
        {
            if (_PhraseHostLanguageIDs == null)
                return null;

            if ((index >= 0) && (index < _PhraseHostLanguageIDs.Count()))
                return _PhraseHostLanguageIDs[index];

            return null;
        }

        public void AddPhraseHostLanguageID(LanguageID languageID)
        {
            if (_PhraseHostLanguageIDs == null)
                _PhraseHostLanguageIDs = new List<LanguageID>() { languageID };
            else
            {
                LanguageID oldLanguageID = _PhraseHostLanguageIDs.FirstOrDefault(x => x.LanguageCode == languageID.LanguageCode);

                if (oldLanguageID != null)
                {
                    int index = _PhraseHostLanguageIDs.IndexOf(oldLanguageID);
                    _PhraseHostLanguageIDs[index] = languageID;
                }
                else
                    _PhraseHostLanguageIDs.Add(languageID);
            }

            _Modified = true;
        }

        public void DeletePhraseHostLanguageIDs(List<LanguageID> languageIDs)
        {
            foreach (LanguageID languageID in languageIDs)
                DeletePhraseHostLanguageID(languageID);
        }

        public void DeletePhraseHostLanguageID(LanguageID languageID)
        {
            _PhraseHostLanguageIDs.Remove(languageID);
        }

        public void DeletePhraseHostLanguageIDIndexed(int index)
        {
            if (_PhraseHostLanguageIDs == null)
                return;

            if ((index >= 0) && (index < _PhraseHostLanguageIDs.Count()))
                _PhraseHostLanguageIDs.RemoveAt(index);
        }

        public string GetPhraseHostLanguageIDsString()
        {
            if (_PhraseHostLanguageIDs == null)
                return String.Empty;

            return LanguageID.ConvertLanguageIDListToDelimitedString(_PhraseHostLanguageIDs, "|", "|", "|");
        }

        public void SetPhraseHostLanguageIDsString(string str)
        {
            _Modified = true;
            _PhraseHostLanguageIDs = LanguageID.ParseLanguageIDDelimitedList(str, LanguageLookup.Bar);
        }

        // Not saved. Doesn't touch Modified.
        public DictionaryEntry CachedDictionaryEntry
        {
            get
            {
                return _CachedDictionaryEntry;
            }
            set
            {
                _CachedDictionaryEntry = value;
            }
        }

        public SentenceParsingInfo ParsingInfo
        {
            get
            {
                return _ParsingInfo;
            }
            set
            {
                _ParsingInfo = value;
            }
        }

        public virtual bool Modified
        {
            get
            {
                if (_Modified)
                    return true;

                if (_MediaRuns != null)
                {
                    foreach (MediaRun mediaRun in _MediaRuns)
                    {
                        if (mediaRun.Modified)
                            return true;
                    }
                }

                return false;
            }
            set
            {
                _Modified = value;

                if (_MediaRuns != null)
                {
                    foreach (MediaRun mediaRun in _MediaRuns)
                        mediaRun.Modified = false;
                }
            }
        }

        public bool Contains(int index)
        {
            if ((index >= _Start) && (index < Stop))
                return true;

            return false;
        }

        public override string ToString()
        {
            return _Start.ToString() + " " + _Length.ToString();
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            element.Add(new XAttribute("Start", _Start.ToString()));
            element.Add(new XAttribute("Length", _Length.ToString()));

            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                    element.Add(mediaRun.GetElement("MediaRun"));
            }

            if (_WordMappings != null)
            {
                string wordMappingsString = GetWordMappingsString();
                element.Add(new XElement("WordMappings", wordMappingsString));
            }

            if ((_PhraseHostLanguageIDs != null) && (_PhraseHostLanguageIDs.Count() != 0))
            {
                string phraseHostLanguagesString = GetPhraseHostLanguageIDsString();
                element.Add(new XElement("PhraseHostLanguageIDs", phraseHostLanguagesString));
            }

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Start":
                    _Start = Convert.ToInt32(attributeValue);
                    break;
                case "Length":
                    _Length = Convert.ToInt32(attributeValue);
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
                case "MediaRun":
                    if (_MediaRuns == null)
                        _MediaRuns = new List<MediaRun>();
                    _MediaRuns.Add(new MediaRun(childElement));
                    break;
                case "WordMappings":
                    SetWordMappingsString(childElement.Value.Trim());
                    _Modified = false;
                    break;
                case "PhraseHostLanguageIDs":
                    SetPhraseHostLanguageIDsString(childElement.Value.Trim());
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public static int Compare(TextRun object1, TextRun object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            if (object1.Start != object2.Start)
                return object2.Start - object1.Start;
            if (object1.Length != object2.Length)
                return object2.Length - object1.Length;
            return 0;
        }
    }
}
