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
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Content
{
    public class LanguageItem : LanguageString
    {
        protected List<TextRun> _SentenceRuns;  // Should not overlap.
        protected List<TextRun> _WordRuns;      // Should not overlap.
        protected List<TextRun> _PhraseRuns;    // Can overlap.

        public LanguageItem(object key, LanguageID languageID, string text, List<TextRun> sentenceRuns, List<TextRun> wordRuns)
            : base(key, languageID, text)
        {
            _SentenceRuns = sentenceRuns;
            _WordRuns = wordRuns;
            _PhraseRuns = null;
        }

        public LanguageItem(object key, LanguageID languageID, string text)
            : base(key, languageID, text)
        {
            _SentenceRuns = null;
            _WordRuns = null;
            _PhraseRuns = null;
        }

        public LanguageItem(LanguageItem other)
            : base(other)
        {
            if (other.SentenceRuns != null)
            {
                int count = other.SentenceRuns.Count();

                _SentenceRuns = new List<TextRun>(count);

                foreach (TextRun sentenceRun in other.SentenceRuns)
                    _SentenceRuns.Add(new TextRun(sentenceRun));
            }
            else
                _SentenceRuns = null;

            if (other.WordRuns != null)
            {
                int count = other.WordRuns.Count();

                _WordRuns = new List<TextRun>(count);

                foreach (TextRun wordRun in other.WordRuns)
                    _WordRuns.Add(new TextRun(wordRun));
            }
            else
                _WordRuns = null;
        }

        public LanguageItem(object key, LanguageItem other)
            : base(key, other)
        {
            Copy(other);
        }

        public LanguageItem(XElement element)
        {
            OnElement(element);
        }

        public LanguageItem()
        {
            ClearLanguageItem();
        }

        public override void Clear()
        {
            base.Clear();
            ClearLanguageItem();
        }

        public void ClearLanguageItem()
        {
            _SentenceRuns = null;
            _WordRuns = null;
        }

        public virtual void Copy(LanguageItem other)
        {
            base.Copy(other);

            if (other.SentenceRuns != null)
            {
                int count = other.SentenceRuns.Count();

                _SentenceRuns = new List<TextRun>(count);

                foreach (TextRun sentenceRun in other.SentenceRuns)
                    _SentenceRuns.Add(new TextRun(sentenceRun));
            }
            else
                _SentenceRuns = null;

            if (other.WordRuns != null)
            {
                int count = other.WordRuns.Count();

                _WordRuns = new List<TextRun>(count);

                foreach (TextRun wordRun in other.WordRuns)
                    _WordRuns.Add(new TextRun(wordRun));
            }
            else
                _WordRuns = null;
        }

        public virtual void CopyShallow(LanguageItem other)
        {
            if (other.SentenceRuns != null)
            {
                int count = other.SentenceRuns.Count();

                _SentenceRuns = new List<TextRun>(count);

                foreach (TextRun sentenceRun in other.SentenceRuns)
                    _SentenceRuns.Add(new TextRun(sentenceRun));
            }
            else
                _SentenceRuns = null;

            if (other.WordRuns != null)
            {
                int count = other.WordRuns.Count();

                _WordRuns = new List<TextRun>(count);

                foreach (TextRun wordRun in other.WordRuns)
                    _WordRuns.Add(new TextRun(wordRun));
            }
            else
                _WordRuns = null;
        }

        public virtual void CopyDeep(LanguageItem other)
        {
            Copy(other);
        }

        public override IBaseObject Clone()
        {
            return new LanguageItem(this);
        }

        public List<TextRun> CloneSentenceRuns()
        {
            if (_SentenceRuns == null)
                return null;

            List<TextRun> textRuns = new List<TextRun>();

            foreach (TextRun textRun in _SentenceRuns)
                textRuns.Add(new TextRun(textRun));

            return textRuns;
        }

        public List<TextRun> CloneWordRuns()
        {
            if (_WordRuns == null)
                return null;

            List<TextRun> textRuns = new List<TextRun>();

            foreach (TextRun textRun in _WordRuns)
                textRuns.Add(new TextRun(textRun));

            return textRuns;
        }

        public LanguageItem CloneSentenceLanguageItem(int sentenceIndex)
        {
            TextRun sentenceRun = GetSentenceRun(sentenceIndex);
            List<TextRun> sentenceRuns = new List<TextRun>();
            TextRun newSentenceRun;
            string text = String.Empty;

            if (sentenceRun != null)
            {
                text = GetRunText(sentenceRun);
                List<MediaRun> mediaRuns = sentenceRun.CloneMediaRuns();
                newSentenceRun = new TextRun(0, text.Length, mediaRuns);
            }
            else
                newSentenceRun = new TextRun();

            sentenceRuns.Add(newSentenceRun);
            LanguageItem languageItem = new LanguageItem(Key, LanguageID, text, sentenceRuns, null);
            return languageItem;
        }

        public string TextWithZeroWidthSpaces
        {
            get
            {
                string text = Text;

                if (HasWordRuns())
                {
                    StringBuilder sb = new StringBuilder();
                    int lastIndex = 0;

                    foreach (TextRun wordRun in WordRuns)
                    {
                        int si = wordRun.Start;
                        int len = wordRun.Length;
                        int ei = wordRun.Stop;

                        if (si > lastIndex)
                            sb.Append(text.Substring(lastIndex, si - lastIndex));

                        sb.Append(text.Substring(si, len));
                        sb.Append(LanguageLookup.ZeroWidthSpace);

                        lastIndex = ei;
                    }

                    if (text.Length > lastIndex)
                        sb.Append(text.Substring(lastIndex, text.Length - lastIndex));

                    text = sb.ToString();
                }

                return text;
            }
        }

        public List<TextRun> SentenceRuns
        {
            get
            {
                return _SentenceRuns;
            }
            set
            {
                if (value != _SentenceRuns)
                {
                    _Modified = true;
                    _SentenceRuns = value;
                }
            }
        }

        public bool HasSentenceRuns()
        {
            if ((_SentenceRuns != null) && (_SentenceRuns.Count() != 0))
                return true;
            return false;
        }

        public int SentenceRunCount()
        {
            if (_SentenceRuns != null)
                return _SentenceRuns.Count();
            return 0;
        }

        public TextRun GetSentenceRun(int index)
        {
            if ((_SentenceRuns != null) && (index >= 0) && (index < _SentenceRuns.Count()))
                return _SentenceRuns[index];
            return null;
        }

        public bool GetWordSentenceRun(
            int wordStartIndex,
            int wordStopIndex,
            out TextRun sentenceRun,
            out int sentenceIndex)
        {
            sentenceRun = null;
            sentenceIndex = 0;

            if (_SentenceRuns != null)
            {
                foreach (TextRun aRun in _SentenceRuns)
                {
                    if ((wordStartIndex >= aRun.Start) && (wordStopIndex <= aRun.Stop))
                    {
                        sentenceRun = aRun;
                        return true; 
                    }

                    sentenceIndex++;
                }
            }

            return false;
        }

        public TextRun GetMergedSentenceRun()
        {
            if (_SentenceRuns != null)
            {
                TextRun mergedSentenceRun = new Content.TextRun(0, TextLength, null);

                foreach (TextRun sentenceRun in _SentenceRuns)
                {
                    if (sentenceRun.MediaRunCount() != 0)
                    {
                        foreach (MediaRun mediaRun in sentenceRun.MediaRuns)
                        {
                            MediaRun mergedMediaRun = mergedSentenceRun.GetMediaRunCorresponding(mediaRun);

                            if (mergedMediaRun == null)
                            {
                                mergedMediaRun = new MediaRun(mediaRun);
                                mergedSentenceRun.AddMediaRun(mediaRun);
                            }
                            else
                                mergedMediaRun.Merge(mediaRun);
                        }
                    }
                }

                return mergedSentenceRun;
            }

            return null;
        }

        public TextRun GetSentenceRunContaining(int characterIndex)
        {
            TextRun sentenceRun = null;

            if (_SentenceRuns != null)
                sentenceRun = _SentenceRuns.FirstOrDefault(x => x.Contains(characterIndex));

            return sentenceRun;
        }

        public int GetSentenceIndexContaining(int characterIndex)
        {
            int sentenceIndex = -1;

            if (_SentenceRuns != null)
            {
                int sentenceCount = _SentenceRuns.Count();

                for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
                {
                    TextRun sentenceRun = _SentenceRuns[sentenceIndex];

                    if ((sentenceRun != null) && sentenceRun.Contains(characterIndex))
                        return sentenceIndex;
                }
            }

            return -1;
        }

        public bool HasSentenceRun(TextRun sentenceRun)
        {
            if (_SentenceRuns != null)
            {
                foreach (TextRun textRun in _SentenceRuns)
                {
                    if (TextRun.Compare(sentenceRun, textRun) == 0)
                        return true;
                }
            }

            return false;
        }

        public string GetRunText(TextRun textRun)
        {
            if ((textRun == null) || String.IsNullOrEmpty(_Text))
                return String.Empty;

            int start = textRun.Start;
            int length = textRun.Length;

            if (start < 0)
            {
                length += start;
                start = 0;
            }

            if (start >= _Text.Length)
                return String.Empty;

            if (start + length > _Text.Length)
                length = _Text.Length - start;

            string text = _Text.Substring(start, length);

            return text;
        }

        public string GetRunTextPreview(TextRun textRun, int length, string ellipsis)
        {
            string text = GetRunText(textRun);

            if (text.Length > length)
            {
                text = text.Substring(0, length);

                if (!String.IsNullOrEmpty(ellipsis))
                    text += ellipsis;
            }

            return text;
        }

        public string GetRunText(int sentenceIndex)
        {
            TextRun textRun = GetSentenceRun(sentenceIndex);
            return GetRunText(textRun);
        }

        public bool SetRunText(int index, string text)
        {
            if ((_SentenceRuns != null) && (index >= 0) && (index < _SentenceRuns.Count()))
            {
                TextRun sentenceRun = _SentenceRuns[index];
                int sentenceStart = sentenceRun.Start;
                int oldLength = sentenceRun.Length;
                int newLength = text.Length;
                int delta = newLength - oldLength;

                _Text = _Text.Remove(sentenceStart, oldLength);
                _Text = _Text.Insert(sentenceStart, text);

                sentenceRun.Length = newLength;
                _Modified = true;

                int sentenceCount = _SentenceRuns.Count();

                index++;

                if (oldLength != 0)
                {
                    while (index < sentenceCount)
                    {
                        sentenceRun = _SentenceRuns[index];
                        sentenceRun.Start = sentenceRun.Start + delta;
                        index++;
                    }
                }

                return true;
            }

            return false;
        }

        public bool MapText(BaseObjectContent contentMediaItem, LanguageMediaItem languageMediaItem,
            int sentenceIndex, TimeSpan mediaStartTime, TimeSpan mediaStopTime)
        {
            TextRun sentenceRun;
            string mediaItemKey = (contentMediaItem != null ? contentMediaItem.KeyString : String.Empty);
            string languageMediaItemKey = (languageMediaItem != null ? languageMediaItem.KeyString : String.Empty);
            TimeSpan mediaLengthTime = mediaStopTime - mediaStartTime;

            if (sentenceIndex > SentenceRunCount())
                return false;
            else if (sentenceIndex == SentenceRunCount())
            {
                if (sentenceIndex == 0)
                {
                    sentenceRun = new TextRun(this, null);
                    AddSentenceRun(sentenceRun);
                }
                else
                    return false;
            }
            else
                sentenceRun = GetSentenceRun(sentenceIndex);
            if (sentenceRun == null)
                return false;
            MediaRun mediaRun = null;
            if (contentMediaItem == null)
                mediaRun = sentenceRun.GetMediaRun("Audio");
            else
                mediaRun = sentenceRun.GetMediaRunWithReferenceKeys(mediaItemKey, languageMediaItemKey);
            if (mediaRun == null)
            {
                if (contentMediaItem != null)
                {
                    ContentMediaItem mediaItem = contentMediaItem.ContentStorageMediaItem;
                    string mediaRunKey = (mediaItem.GetDefaultMediaType() == Media.MediaTypeCode.Video ? "Video" : "Audio");
                    mediaRun = new MediaRun(mediaRunKey, mediaItemKey, languageMediaItemKey, mediaStartTime, mediaLengthTime);
                    sentenceRun.AddMediaRun(mediaRun);
                    return true;
                }
                else
                {
                    mediaRun = new MediaRun("Audio", mediaItemKey, languageMediaItemKey, mediaStartTime, mediaLengthTime);
                    sentenceRun.AddMediaRun(mediaRun);
                    return true;
                }
            }
            mediaRun.Start = mediaStartTime;
            mediaRun.Length = mediaLengthTime;
            return true;
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItemKey, int sentenceIndex)
        {
            TextRun sentenceRun = GetSentenceRun(sentenceIndex);

            if (sentenceRun != null)
                sentenceRun.MapTextClear(mediaItemKey, languageMediaItemKey);
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItemKey,
            TimeSpan mediaStartTime, TimeSpan mediaStopTime)
        {
            if (_SentenceRuns == null)
                return;

            foreach (TextRun sentenceRun in _SentenceRuns)
                sentenceRun.MapTextClear(mediaItemKey, languageMediaItemKey, mediaStartTime, mediaStopTime);
        }

        public void MapTextClear(string mediaItemKey, string languageMediaItemKey)
        {
            if (_SentenceRuns == null)
                return;

            foreach (TextRun sentenceRun in _SentenceRuns)
                sentenceRun.MapTextClear(mediaItemKey, languageMediaItemKey);
        }

        public bool InsertSentenceRun(int index, TextRun sentenceRun)
        {
            if (_SentenceRuns == null)
                _SentenceRuns = new List<TextRun>();

            if (index > _SentenceRuns.Count())
                index = _SentenceRuns.Count();

            if (index < _SentenceRuns.Count())
                _SentenceRuns.Insert(index, sentenceRun);
            else
                _SentenceRuns.Add(sentenceRun);

            _Modified = true;

            return true;
        }

        public bool InsertEmptySentenceRun(int index)
        {
            if (_SentenceRuns == null)
                _SentenceRuns = new List<TextRun>();

            if (index > _SentenceRuns.Count())
                index = _SentenceRuns.Count();

            if (index < _SentenceRuns.Count())
            {
                TextRun anchorRun = _SentenceRuns[index];

                if (anchorRun == null)
                    return false;

                TextRun sentenceRun = new TextRun(anchorRun.Start, 0, null);
                _SentenceRuns.Insert(index, sentenceRun);
            }
            else
            {
                TextRun sentenceRun = new TextRun(TextLength, 0, null);
                _SentenceRuns.Add(sentenceRun);
            }

            _Modified = true;

            return true;
        }

        public void AddSentenceRun(TextRun sentenceRun)
        {
            if (_SentenceRuns == null)
                _SentenceRuns = new List<TextRun>() { sentenceRun };
            else
                _SentenceRuns.Add(sentenceRun);

            _Modified = true;
        }

        public bool ClearSentenceRunIndexed(int index)
        {
            if ((_SentenceRuns != null) && (index >= 0) && (index < _SentenceRuns.Count()))
            {
                TextRun sentenceRun = _SentenceRuns[index];
                int sentenceStart = sentenceRun.Start;
                int sentenceLength = sentenceRun.Length;

                _Text = _Text.Remove(sentenceStart, sentenceLength);
                sentenceRun.Length = 0;
                _Modified = true;

                int sentenceCount = _SentenceRuns.Count();

                index++;

                if (sentenceLength != 0)
                {
                    while (index < sentenceCount)
                    {
                        sentenceRun = _SentenceRuns[index];
                        sentenceRun.Start = sentenceRun.Start - sentenceLength;
                        index++;
                    }
                }

                return true;
            }

            return false;
        }

        public bool DeleteSentenceRunIndexed(int index)
        {
            if ((_SentenceRuns != null) && (index >= 0) && (index < _SentenceRuns.Count()))
            {
                TextRun sentenceRun = _SentenceRuns[index];

                if (_SentenceRuns.Count() == 1)
                {
                    sentenceRun.Start = 0;
                    sentenceRun.Length = 0;
                    Text = String.Empty;
                    return true;
                }

                int sentenceStart = sentenceRun.Start;
                int sentenceLength = sentenceRun.Length;

                _SentenceRuns.RemoveAt(index);
                _Modified = true;

                int sentenceCount = _SentenceRuns.Count();

                if (index < sentenceCount)
                    sentenceLength = _SentenceRuns[index].Start - sentenceStart;
                else if ((_Text != null) && (sentenceStart + sentenceLength < _Text.Length))
                    sentenceLength = _Text.Length - sentenceStart;

                if (sentenceLength != 0)
                {
                    _Text = _Text.Remove(sentenceStart, sentenceLength);

                    while (index < sentenceCount)
                    {
                        sentenceRun = _SentenceRuns[index];
                        sentenceRun.Start = sentenceRun.Start - sentenceLength;
                        index++;
                    }
                }

                return true;
            }

            return false;
        }

        public void DeleteSentenceAndWordRuns()
        {
            DeleteSentenceRuns();
            DeleteWordRuns();
        }

        public void DeleteSentenceRuns()
        {
            if ((_SentenceRuns != null) && (_SentenceRuns.Count() != 0))
            {
                _SentenceRuns.Clear();
                _Modified = true;
            }
        }

        public void DeleteWordRuns()
        {
            if ((_WordRuns != null) && (_WordRuns.Count() != 0))
            {
                _WordRuns.Clear();
                _Modified = true;
            }
        }

        public void DeleteText()
        {
            DeleteSentenceRuns();
            _Text = null;
        }

        public bool IsMediaLanguageMatch(LanguageID mediaLanguageID)
        {
            if (mediaLanguageID == null)
                return false;

            if (LanguageID == null)
                return false;

            if (mediaLanguageID.LanguageCode == LanguageID.LanguageCode)
                return true;

            return false;
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

            if (_SentenceRuns != null)
            {
                foreach (TextRun sentenceRun in _SentenceRuns)
                {
                    bool returnTemp = sentenceRun.GetMediaInfo(mediaRunKey, mediaPathUrl, node,
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
            if (_SentenceRuns != null)
            {
                foreach (TextRun sentenceRun in _SentenceRuns)
                {
                    if (sentenceRun.HasMediaRunWithKey(key))
                        return true;
                }
            }

            return false;
        }

        public bool HasMediaRunWithUrl(string url)
        {
            if (!String.IsNullOrEmpty(url))
            {
                if (_SentenceRuns != null)
                {
                    foreach (TextRun sentenceRun in _SentenceRuns)
                    {
                        if (sentenceRun.HasMediaRunWithUrl(url))
                            return true;
                    }
                }
            }

            return false;
        }

        public MediaRun GetMediaRunWithKey(int sentenceIndex, object key)
        {
            TextRun sentenceRun = GetSentenceRun(sentenceIndex);

            if (sentenceRun != null)
                return sentenceRun.GetMediaRunWithKey(key);

            return null;
        }

        public MediaRun GetMediaRun(int sentenceIndex, object key)
        {
            TextRun sentenceRun = GetSentenceRun(sentenceIndex);

            if (sentenceRun != null)
                return sentenceRun.GetMediaRun(key);

            return null;
        }

        public MediaRun GetMediaRunWithUrl(int sentenceIndex, string fileName)
        {
            TextRun sentenceRun = GetSentenceRun(sentenceIndex);

            if (sentenceRun != null)
                return sentenceRun.GetMediaRunWithUrl(fileName);

            return null;
        }

        public MediaRun GetMediaRunWithUrl(string fileName)
        {
            if (_SentenceRuns == null)
                return null;

            foreach (TextRun sentenceRun in _SentenceRuns)
            {
                MediaRun mediaRun = sentenceRun.GetMediaRunWithUrl(fileName);

                if (mediaRun != null)
                    return mediaRun;
            }

            return null;
        }

        public MediaRun GetMediaRunWithReferenceKeys(
            int sentenceIndex,
            string mediaItemKey,
            string languageMediaItemKey)
        {
            TextRun sentenceRun = GetSentenceRun(sentenceIndex);

            if (sentenceRun != null)
                return sentenceRun.GetMediaRunWithReferenceKeys(mediaItemKey, languageMediaItemKey);

            return null;
        }

        public void GetMediaRunsWithReferenceKeys(
            List<MediaRun> mediaRuns,
            string mediaItemKey,
            string languageMediaItemKey)
        {
            if (_SentenceRuns == null)
                return;

            foreach (TextRun sentenceRun in _SentenceRuns)
                sentenceRun.GetMediaRunsWithReferenceKeys(mediaRuns, mediaItemKey, languageMediaItemKey);
        }

        public int GetMediaRunCount(int sentenceIndex, string mediaRunKey)
        {
            int count = 0;

            if (sentenceIndex == -1)
                sentenceIndex = 0;

            TextRun sentenceRun = GetSentenceRun(sentenceIndex);

            if (sentenceRun != null)
                return sentenceRun.GetMediaRunCount(mediaRunKey);

            return count;
        }

        public MediaRun GetMergedReferenceMediaRun(
            object key,
            string mediaItemKey,
            string languageMediaItemKey)
        {
            if ((_SentenceRuns == null) || (_SentenceRuns.Count() == 0))
                return null;

            MediaRun mergedMediaRun = null;

            foreach (TextRun sentenceRun in _SentenceRuns)
            {
                MediaRun mediaRun = sentenceRun.GetMediaRunWithKeyAndReferenceKeys(key, mediaItemKey, languageMediaItemKey);

                if (mediaRun == null)
                    continue;

                if (mergedMediaRun == null)
                    mergedMediaRun = new Content.MediaRun(mediaRun);
                else
                    mergedMediaRun.JoinMediaRun(mediaRun);
            }

            return mergedMediaRun;
        }

        // Returns true if any media runs removed.
        public bool DeleteMediaRunsWithReferenceKeys(string mediaItemKey, string languageMediaItemKey)
        {
            bool returnValue = false;

            if (_SentenceRuns == null)
                return false;

            foreach (TextRun sentenceRun in _SentenceRuns)
            {
                if (sentenceRun.DeleteMediaRunsWithReferenceKeys(mediaItemKey, languageMediaItemKey))
                    returnValue = true;
            }

            return returnValue;
        }

        // Re-map reference media run times to target time.
        // Target time advanced to end of mapped time period.
        public void RemapReferenceMediaRuns(
            string mediaRunKey,
            string mediaItemKey,
            string languageMediaItemKey,
            TimeSpan timeToSubtract)
        {
            if (_SentenceRuns == null)
                return;

            foreach (TextRun sentenceRun in _SentenceRuns)
                sentenceRun.RemapReferenceMediaRuns(mediaRunKey, mediaItemKey, languageMediaItemKey, timeToSubtract);
        }

        // Note: Media references will be collapsed to a "(url)#t(start),(end)" url.
        // Returns true if any media found.
        public bool CollectMediaUrls(string mediaRunKey, string mediaPathUrl, BaseObjectNode node,
            object content, List<string> audioUrls, VisitMedia visitFunction)
        {
            bool returnValue = false;

            if (_SentenceRuns != null)
            {
                foreach (TextRun sentenceRun in _SentenceRuns)
                {
                    if (sentenceRun.CollectMediaUrls(mediaRunKey, mediaPathUrl, node, content, audioUrls, visitFunction))
                        returnValue = true;
                }
            }

            return returnValue;
        }

        public bool GetMediaRunTimeUnion(string mediaRunKey, out TimeSpan mergedStart, out TimeSpan mergedStop)
        {
            if (_SentenceRuns != null)
            {
                TimeSpan bestStart = TimeSpan.MaxValue;
                TimeSpan bestStop = TimeSpan.Zero;

                foreach (TextRun sentenceRun in _SentenceRuns)
                {
                    MediaRun mediaRun = sentenceRun.GetMediaRunAny(mediaRunKey);

                    if (mediaRun != null)
                    {
                        if (mediaRun.Start < bestStart)
                            bestStart = mediaRun.Start;

                        if (mediaRun.Stop > bestStop)
                            bestStop = mediaRun.Stop;
                    }
                }

                if (bestStart < bestStop)
                {
                    mergedStart = bestStart;
                    mergedStop = bestStop;
                    return true;
                }
            }

            mergedStart = TimeSpan.Zero;
            mergedStop = TimeSpan.Zero;

            return false;
        }

        public void CollectMediaFiles(string directoryUrl, List<string> mediaFiles, object content, VisitMedia visitFunction)
        {
            if (_SentenceRuns != null)
            {
                foreach (TextRun sentenceRun in _SentenceRuns)
                    sentenceRun.CollectMediaFiles(directoryUrl, mediaFiles, content, visitFunction);
            }
        }

        public void CollectMediaReferences(List<MediaRun> mediaRuns)
        {
            if (_SentenceRuns != null)
            {
                foreach (TextRun sentenceRun in _SentenceRuns)
                    sentenceRun.CollectMediaReferences(mediaRuns);
            }
        }

        public bool CopyMedia(string sourceMediaDirectory, string targetDirectoryRoot, List<string> copiedFiles,
            ref string errorMessage)
        {
            bool returnValue = true;

            if (_SentenceRuns != null)
            {
                foreach (TextRun sentenceRun in _SentenceRuns)
                {
                    if (!sentenceRun.CopyMedia(sourceMediaDirectory, targetDirectoryRoot, copiedFiles, ref errorMessage))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public List<TextRun> WordRuns
        {
            get
            {
                return _WordRuns;
            }
            set
            {
                if (value != _WordRuns)
                {
                    _Modified = true;
                    _WordRuns = value;
                }
            }
        }

        public bool HasWordRuns()
        {
            if ((_WordRuns != null) && (_WordRuns.Count() != 0))
                return true;
            return false;
        }

        public int WordRunCount()
        {
            if (_WordRuns != null)
                return _WordRuns.Count();
            return 0;
        }

        public bool HasWordRun(TextRun wordRun)
        {
            if (_WordRuns != null)
            {
                foreach (TextRun textRun in _WordRuns)
                {
                    if (TextRun.Compare(wordRun, textRun) == 0)
                        return true;
                }
            }

            return false;
        }

        public int GetSentenceStartWordRunIndex(int sentenceIndex)
        {
            if (_WordRuns == null)
                return -1;

            TextRun sentenceRun = GetSentenceRun(sentenceIndex);

            if (sentenceRun == null)
                return -1;

            int wordRunIndex = 0;

            foreach (TextRun wordRun in _WordRuns)
            {
                if (sentenceRun.Contains(wordRun.Start))
                    return wordRunIndex;

                wordRunIndex++;
            }

            if (wordRunIndex >= _WordRuns.Count())
                wordRunIndex = -1;

            return wordRunIndex;
        }

        public int GetSentenceWordRunIndex(int sentenceIndex, int sentenceWordIndex)
        {
            if (_WordRuns == null)
                return -1;

            TextRun sentenceRun = GetSentenceRun(sentenceIndex);

            if (sentenceRun == null)
                return -1;

            int wordRunIndex = 0;

            foreach (TextRun wordRun in _WordRuns)
            {
                if (sentenceRun.Contains(wordRun.Start))
                    return wordRunIndex;

                wordRunIndex++;
            }

            if (wordRunIndex >= _WordRuns.Count())
                wordRunIndex = -1;


            if (wordRunIndex != -1)
            {
                wordRunIndex += sentenceWordIndex;

                if (wordRunIndex >= _WordRuns.Count())
                    wordRunIndex = -1;
            }

            return wordRunIndex;
        }

        public bool GetSentenceWordRunStartIndexAndCount(
            int sentenceIndex,
            out int sentenceWordRunStartIndex,
            out int sentenceWordRunCount)
        {
            TextRun sentenceRun = GetSentenceRun(sentenceIndex);
            return GetSentenceWordRunStartIndexAndCountStatic(sentenceRun, _WordRuns, out sentenceWordRunStartIndex, out sentenceWordRunCount);
        }

        public int GetSentenceWordRunCount(int sentenceIndex)
        {
            TextRun sentenceRun = GetSentenceRun(sentenceIndex);
            int sentenceWordRunStartIndex;
            int sentenceWordRunCount;

            if (GetSentenceWordRunStartIndexAndCountStatic(sentenceRun, _WordRuns, out sentenceWordRunStartIndex, out sentenceWordRunCount))
                return sentenceWordRunCount;

            return 0;
        }

        public static bool GetSentenceWordRunStartIndexAndCountStatic(
            TextRun sentenceRun,
            List<TextRun> wordRuns,
            out int sentenceWordRunStartIndex,
            out int sentenceWordRunCount)
        {
            sentenceWordRunStartIndex = -1;
            sentenceWordRunCount = -1;

            if (wordRuns == null)
                return false;

            if (sentenceRun == null)
                return false;

            int wordRunIndex = 0;
            int wordRunCount = 0;

            foreach (TextRun wordRun in wordRuns)
            {
                if (sentenceWordRunStartIndex == -1)
                {
                    if (sentenceRun.Contains(wordRun.Start))
                    {
                        sentenceWordRunStartIndex = wordRunIndex;
                        wordRunCount = 1;
                    }
                }
                else
                {
                    if (sentenceRun.Contains(wordRun.Start))
                        wordRunCount++;
                    else
                        break;
                }

                wordRunIndex++;
            }

            if (sentenceWordRunStartIndex == -1)
                return false;

            sentenceWordRunCount = wordRunCount;

            return true;
        }

        public List<TextRun> GetSentenceWordRuns(TextRun sentenceRun)
        {
            int start = sentenceRun.Start;
            int stop = sentenceRun.Stop;
            List<TextRun> wordRuns = new List<TextRun>();

            if (HasWordRuns())
            {
                List<TextRun> sourceWordRuns = WordRuns;

                foreach (TextRun sourceWordRun in sourceWordRuns)
                {
                    int wordStart = sourceWordRun.Start;
                    int wordStop = sourceWordRun.Stop;

                    if (wordStop <= start)
                        continue;
                    else if (wordStart >= stop)
                        continue;

                    wordRuns.Add(sourceWordRun);
                }
            }

            return wordRuns;
        }

        public List<TextRun> GetSentenceWordRunsRetargeted(TextRun sentenceRun)
        {
            int start = sentenceRun.Start;
            int stop = sentenceRun.Stop;
            List<TextRun> wordRuns = new List<TextRun>();

            if (HasWordRuns())
            {
                List<TextRun> sourceWordRuns = WordRuns;

                foreach (TextRun sourceWordRun in sourceWordRuns)
                {
                    int wordStart = sourceWordRun.Start;
                    int wordStop = sourceWordRun.Stop;

                    if (wordStop <= start)
                        continue;
                    else if (wordStart >= stop)
                        continue;

                    TextRun newWordRun = new TextRun(
                        wordStart - start,
                        sourceWordRun.Length,
                        null);

                    wordRuns.Add(newWordRun);
                }
            }

            return wordRuns;
        }

        public int GetSentenceRunWordStartIndex(int sentenceIndex)
        {
            int index = 0;
            int count = SentenceRunCount();
            int wordIndexStart = 0;

            for (index = 0; index < count; index++)
            {
                if (index == sentenceIndex)
                    break;

                wordIndexStart += SentenceWordRunCount(index);
            }

            return wordIndexStart;
        }

        public TextRun GetWordRun(int index)
        {
            if ((_WordRuns != null) && (index >= 0) && (index < _WordRuns.Count()))
                return _WordRuns[index];
            return null;
        }

        public string GetWordRunText(int wordIndex)
        {
            TextRun textRun = GetWordRun(wordIndex);
            return GetRunText(textRun);
        }

        public bool SetWordRunText(int index, string text)
        {
            if ((_WordRuns != null) && (index >= 0) && (index < _WordRuns.Count()))
            {
                TextRun wordRun = _WordRuns[index];
                int wordStart = wordRun.Start;
                int oldLength = wordRun.Length;
                int newLength = text.Length;
                int delta = newLength - oldLength;

                _Text = _Text.Remove(wordStart, oldLength);
                _Text = _Text.Insert(wordStart, text);

                wordRun.Length = newLength;
                _Modified = true;

                int wordCount = _WordRuns.Count();

                index++;

                if (oldLength != 0)
                {
                    while (index < wordCount)
                    {
                        wordRun = _WordRuns[index];
                        wordRun.Start = wordRun.Start + delta;
                        index++;
                    }
                }

                return true;
            }

            return false;
        }

        public string GetSubText(int startIndex, int length)
        {
            return Text.Substring(startIndex, length);
        }

        public List<string> GetWords()
        {
            List<string> words = new List<string>();

            if ((_WordRuns == null) || (_WordRuns.Count() == 0))
                words.Add(Text.Trim());
            else
            {
                foreach (TextRun textRun in _WordRuns)
                {
                    string word = GetRunText(textRun);
                    words.Add(word);
                }
            }

            return words;
        }

        public List<string> GetUniqueWords()
        {
            List<string> words = new List<string>();

            if ((_WordRuns == null) || (_WordRuns.Count() == 0))
                words.Add(Text.Trim());
            else
            {
                HashSet<string> hashSet = new HashSet<string>();

                foreach (TextRun textRun in _WordRuns)
                {
                    string word = GetRunText(textRun);

                    if (hashSet.Add(word))
                        words.Add(word);
                }
            }

            return words;
        }

        public List<TextRun> ExtractWordRuns(int start, int stop, int baseIndex)
        {
            List<TextRun> wordRuns = new List<TextRun>();

            if (HasWordRuns())
            {
                List<TextRun> sourceWordRuns = WordRuns;

                foreach (TextRun sourceWordRun in sourceWordRuns)
                {
                    int wordStart = sourceWordRun.Start;
                    int wordStop = sourceWordRun.Stop;

                    if (wordStop <= start)
                        continue;
                    else if (wordStart >= stop)
                        continue;

                    List<MediaRun> sourceMediaRuns = sourceWordRun.MediaRuns;

                    if (wordStart < start)
                    {
                        wordStart = start;
                        sourceMediaRuns = null;
                    }

                    if (wordStop > stop)
                    {
                        wordStop = stop;
                        sourceMediaRuns = null;
                    }

                    List<MediaRun> wordMediaRuns = null;

                    if (sourceMediaRuns != null)
                    {
                        wordMediaRuns = new List<MediaRun>(sourceMediaRuns.Count());

                        foreach (MediaRun sourceMediaRun in sourceMediaRuns)
                        {
                            MediaRun wordMediaRun = new MediaRun(sourceMediaRun);
                            wordMediaRuns.Add(wordMediaRun);
                        }
                    }

                    wordStart = (wordStart - start) + baseIndex;
                    wordStop = (wordStop - start) + baseIndex;

                    TextRun wordRun = new TextRun(wordStart, wordStop - wordStart, wordMediaRuns);
                    wordRuns.Add(wordRun);
                }
            }

            return wordRuns;
        }

        public bool ValidateWordRuns(out string errorMessage)
        {
            bool returnValue = true;

            errorMessage = null;

            if ((_WordRuns == null) || (_WordRuns.Count() == 0))
            {
                if (!String.IsNullOrEmpty(_Text))
                {
                    errorMessage = "ValidateWordRuns: Word runs empty but text not empty:\n    " +
                            Text;
                    return false;
                }

                return true;
            }

            int wordCount = _WordRuns.Count();
            int wordIndex;
            int lastStart = -1;

            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                TextRun wordRun = _WordRuns[wordIndex];
                TextRun wordRunStart = _WordRuns.FirstOrDefault(x => x.Start == wordRun.Start);
                TextRun wordRunStop = _WordRuns.FirstOrDefault(x => x.Stop == wordRun.Stop);

                if (wordRun.Length == 0)
                {
                    errorMessage = TextUtilities.AppendErrorMessage(
                        errorMessage,
                        "ValidateWordRuns: Word run length is 0: "
                            + " word index "
                            + wordIndex.ToString()
                            + " word run: "
                            + wordRun.ToString()
                            + " word: "
                            + GetRunText(wordRun),
                        true);
                    returnValue = false;
                }

                if (wordRun.Start < 0)
                {
                    errorMessage = TextUtilities.AppendErrorMessage(
                        errorMessage,
                        "ValidateWordRuns: Word run start is less than 0: "
                            + " word index "
                            + wordIndex.ToString()
                            + " word run: "
                            + wordRun.ToString()
                            + " word: "
                            + GetRunText(wordRun),
                        true);
                    returnValue = false;
                }

                if (wordRun.Stop > TextLength)
                {
                    errorMessage = TextUtilities.AppendErrorMessage(
                        errorMessage,
                        "ValidateWordRuns: Word run stop is greater than text length: "
                            + " word index "
                            + wordIndex.ToString()
                            + " word run: "
                            + wordRun.ToString()
                            + " word: "
                            + GetRunText(wordRun),
                        true);
                    returnValue = false;
                }

                if (wordRun.Start < lastStart)
                {
                    errorMessage = TextUtilities.AppendErrorMessage(
                        errorMessage,
                        "ValidateWordRuns: Word run out of order: word index "
                            + wordIndex.ToString()
                            + " word start Index: "
                            + wordRun.Start.ToString()
                            + " last word index: "
                            + lastStart
                            + " word: "
                            + GetRunText(wordRun),
                        true);
                    returnValue = false;
                }

                lastStart = wordRun.Start;
            }

            return returnValue;
        }

        public void GetSentenceAndWordRuns(DictionaryRepository dictionary)
        {
            LoadSentenceRunsFromText();
            LoadWordRunsFromText(dictionary);
        }

        public void SentenceAndWordRunCheck(DictionaryRepository dictionary)
        {
            SentenceRunCheck();
            WordRunCheck(dictionary);
        }

        public bool SentenceRunCheck()
        {
            bool returnValue = true;

            if ((SentenceRuns == null) ||
                    ((SentenceRunCount() == 0) && HasText()))
                LoadSentenceRunsFromText();

            return returnValue;
        }

        public bool ReDoSentenceRuns()
        {
            bool returnValue = true;

            if (_SentenceRuns != null)
                _SentenceRuns.Clear();

            LoadSentenceRunsFromText();

            return returnValue;
        }

        public bool ReDoSentenceRuns(LanguageItem baseLanguageItem)
        {
            bool returnValue = true;

            if (_SentenceRuns != null)
                _SentenceRuns.Clear();

            if ((WordRunCount() == 0) || (baseLanguageItem.WordRunCount() == 0))
                LoadSentenceRunsFromText();
            else if (WordRunCount() != baseLanguageItem.WordRunCount())
                LoadSentenceRunsFromText();
            else
            {
                int wordCount = baseLanguageItem.WordRunCount();
                int lastSentenceEndIndex = 0;

                baseLanguageItem.SentenceRunCheck();

                foreach (TextRun baseSentenceRun in baseLanguageItem.SentenceRuns)
                {
                    int baseSentenceStartIndex = baseSentenceRun.Start;
                    int wordIndex;
                    TextRun baseWordRun = null;
                    TextRun wordRunStart = null;

                    for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
                    {
                        baseWordRun = baseLanguageItem.GetWordRun(wordIndex);

                        if (baseSentenceRun.Contains(baseWordRun.Start))
                        {
                            wordRunStart = GetWordRun(wordIndex);
                            break;
                        }
                    }

                    if (wordRunStart != null)
                    {
                        int baseSentenceEndIndex = baseSentenceRun.Stop;
                        int sentenceStartIndex = wordRunStart.Start;
                        int sentenceStopIndex;
                        TextRun wordRunEnd = null;

                        // Skip leading non-sentence-break punctuation.
                        for (int i = sentenceStartIndex; i > lastSentenceEndIndex; i--)
                        {
                            char c = _Text[i];

                            if (char.IsWhiteSpace(c) || LanguageLookup.SentenceTerminatorCharacters.Contains(c))
                                break;

                            sentenceStartIndex--;
                        }

                        for (wordIndex++; wordIndex < wordCount; wordIndex++)
                        {
                            baseWordRun = baseLanguageItem.GetWordRun(wordIndex);

                            if (baseWordRun.Start >= baseSentenceEndIndex)
                            {
                                wordRunEnd = GetWordRun(wordIndex);
                                break;
                            }
                        }

                        if (wordRunEnd == null)
                            sentenceStopIndex = TextLength;
                        else
                        {
                            sentenceStopIndex = wordRunEnd.Start - 1;

                            // Skip next sentence leading non-sentence-break punctuation.
                            for (int i = sentenceStopIndex; i >= sentenceStartIndex; i--)
                            {
                                char c = _Text[i];

                                if (char.IsWhiteSpace(c) || LanguageLookup.SentenceTerminatorCharacters.Contains(c))
                                    break;

                                sentenceStopIndex--;
                            }

                            if (((sentenceStopIndex != 0) && char.IsWhiteSpace(_Text[sentenceStopIndex])))
                            {
                                while ((sentenceStopIndex != 0) && char.IsWhiteSpace(_Text[sentenceStopIndex]))
                                    sentenceStopIndex--;
                            }

                            if (!LanguageLookup.SentenceStartMarkers.Contains(_Text[sentenceStopIndex]))
                                sentenceStopIndex++;
                        }

                        TextRun sentenceRun = new TextRun(
                            sentenceStartIndex,
                            sentenceStopIndex - sentenceStartIndex,
                            null);

                        if (_SentenceRuns == null)
                            _SentenceRuns = new List<TextRun>() { sentenceRun };
                        else
                            _SentenceRuns.Add(sentenceRun);

                        lastSentenceEndIndex = sentenceRun.Stop;
                    }
                }
            }

            return returnValue;
        }

        public void AutoResetSentenceRuns()
        {
            List<TextRun> saveSentenceRuns = _SentenceRuns;
            _SentenceRuns = null;
            LoadSentenceRunsFromText();

            if ((saveSentenceRuns != null) && (_SentenceRuns != null))
            {
                int sentenceCount = saveSentenceRuns.Count();

                if (sentenceCount > _SentenceRuns.Count())
                    sentenceCount = _SentenceRuns.Count();

                for (int sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
                {
                    TextRun oldRun = saveSentenceRuns[sentenceIndex];
                    TextRun newRun = _SentenceRuns[sentenceIndex];
                    newRun.MediaRuns = oldRun.CloneMediaRuns();
                }
            }

            _Modified = true;
        }

        public static SentenceParsingAlgorithm SentenceParsingType = SentenceParsingAlgorithm.RawPunctuation;

        public void LoadSentenceRunsFromText()
        {
            List<TextRun> sentenceRuns = _SentenceRuns;
            ContentUtilities.ParseTextSentenceRuns(_Text, LanguageID, SentenceParsingType, ref sentenceRuns);
            SentenceRuns = sentenceRuns;
        }

        public void ResetSentenceRuns()
        {
            if ((_SentenceRuns == null) || (_SentenceRuns.Count == 0))
                return;

            TextRun sentenceRun;

            if (_SentenceRuns.Count() == 1)
            {
                sentenceRun = _SentenceRuns.First();

                if ((sentenceRun.Start == 0) && (sentenceRun.Length == TextLength))
                    return;
            }

            sentenceRun = new TextRun(0, TextLength, _SentenceRuns[0].MediaRuns);

            _SentenceRuns.Clear();
            _SentenceRuns.Add(sentenceRun);
            _Modified = true;
        }

        public void PrimeSentenceRunsForWordItem()
        {
            SentenceRuns = new List<TextRun>(1) { new TextRun(0, TextLength, null) };
        }

        public void WordRunCheck(DictionaryRepository dictionary)
        {
            if (WordRuns == null)
                LoadWordRunsFromText(dictionary);
        }

        public void ResetWordRuns()
        {
            if ((_WordRuns == null) || (_WordRuns.Count == 0))
                return;

            _WordRuns = null;
            _Modified = true;
        }

        public void AutoResetWordRuns(DictionaryRepository dictionary)
        {
            List<TextRun> saveWordRuns = _WordRuns;
            _WordRuns = null;
            LoadWordRunsFromText(dictionary);

            if ((saveWordRuns != null) && (_WordRuns != null))
            {
                int wordCount = saveWordRuns.Count();

                if (wordCount > _WordRuns.Count())
                    wordCount = _WordRuns.Count();

                for (int wordIndex = 0; wordIndex < wordCount; wordIndex++)
                {
                    TextRun oldRun = saveWordRuns[wordIndex];
                    TextRun newRun = _WordRuns[wordIndex];
                    newRun.MediaRuns = oldRun.CloneMediaRuns();
                }
            }

            _Modified = true;
        }

        public int SentenceWordRunCount(int sentenceIndex)
        {
            TextRun sentenceRun = GetSentenceRun(sentenceIndex);

            if ((sentenceRun == null) || !HasWordRuns())
                return 0;

            List<TextRun> sourceWordRuns = WordRuns;
            int wordRunCount = 0;
            int start = sentenceRun.Start;
            int stop = sentenceRun.Stop;

            foreach (TextRun sourceWordRun in sourceWordRuns)
            {
                int wordStart = sourceWordRun.Start;
                int wordStop = sourceWordRun.Stop;

                if ((wordStop >= start) && (wordStop <= stop))
                    wordRunCount++;
            }

            return wordRunCount;
        }

        public void ResetSentenceAndWordRuns()
        {
            ResetSentenceRuns();
            ResetWordRuns();
        }

        // Returns true if single quote character is an apostrophe.
        public bool CheckForApostrophe(string text, int apostropheIndex, char apostrophe, out int skipOffset)
        {
            return LanguageTool.CheckForApostropheCommon(text, LanguageID, apostropheIndex, apostrophe, out skipOffset);
        }

        // Returns true if bracketed correction.
        public bool CheckForBracketedCorrection(string text, int bracketIndex, char bracket, out int skipOffset)
        {
            return LanguageTool.CheckForBracketedCorrectionCommon(text, LanguageID, bracketIndex, bracket, out skipOffset);
        }

        // Returns true if period is at an abbreviation.
        public bool CheckForAbbreviation(string text, int periodIndex, char period, out int skipOffset)
        {
            return LanguageTool.CheckForAbbreviationLanguage(text, LanguageID, periodIndex, period, out skipOffset);
        }

        public void LoadWordRunsFromText(DictionaryRepository dictionary)
        {
            string text = (_Text == null ? "" : _Text);
            string runText;
            TextRun textRun;
            int runIndex = 0;
            int charIndex;
            int startIndex = 0;
            int runLength;
            int textLength = text.Length;
            char chr;
            bool isCharacterBased = LanguageLookup.IsCharacterBased(LanguageID);
            bool hasZeroWidthSpaces = false;
            DictionaryEntry dictionaryEntry;
            LanguageTool tool = ApplicationData.LanguageTools.Create(LanguageID);

            if (isCharacterBased)
            {
                if (text.IndexOf(LanguageLookup.ZeroWidthSpace) != -1)
                    hasZeroWidthSpaces = true;
                else if (text.IndexOf(LanguageLookup.ZeroWidthNoBreakSpace) != -1)
                    hasZeroWidthSpaces = true;
            }

            if (_WordRuns == null)
                _WordRuns = new List<TextRun>();

            int runCount = _WordRuns.Count();

            if (!hasZeroWidthSpaces && isCharacterBased && (dictionary != null))
            {
                if (tool != null)
                {
                    tool.GetWordRuns(text, LanguageID, _WordRuns);
                    runCount = runIndex = _WordRuns.Count();
                }
                else
                {
                    for (charIndex = 0; charIndex <= textLength;)
                    {
                        if (charIndex < textLength)
                            chr = text[charIndex];
                        else
                            chr = '\n';

                        if ((charIndex == textLength) ||
                            ((chr >= '\0') && (chr <= '~')) ||
                                LanguageLookup.NonAlphanumericAndSpaceAndPunctuationCharacters.Contains(chr) ||
                                char.IsPunctuation(chr))
                        {
                            if (charIndex > startIndex)
                            {
                                while (startIndex < charIndex)
                                {
                                    runLength = charIndex - startIndex;
                                    runText = text.Substring(startIndex, runLength);

                                    while (runLength > 0)
                                    {
                                        dictionaryEntry = dictionary.Get(runText, LanguageID);

                                        if ((dictionaryEntry != null) || (runLength == 1))
                                        {
                                            if (runIndex == _WordRuns.Count())
                                            {
                                                textRun = new TextRun(startIndex, runLength, null);
                                                _WordRuns.Add(textRun);
                                                runCount++;
                                            }
                                            else
                                            {
                                                textRun = _WordRuns[runIndex];
                                                textRun.Start = startIndex;
                                                textRun.Length = runLength;
                                            }

                                            runIndex++;
                                            break;
                                        }
                                        else
                                        {
                                            runLength--;

                                            if (runLength > 0)
                                                runText = text.Substring(startIndex, runLength);
                                        }
                                    }

                                    if (runLength < 1)
                                        startIndex++;
                                    else
                                        startIndex += runLength;
                                }

                                if (charIndex < textLength)
                                    chr = text[startIndex];
                                else
                                    chr = '\n';
                            }

                            if ((charIndex == textLength) ||
                                ((chr > '\0') && (chr <= ' ')) ||
                                LanguageLookup.NonAlphanumericAndSpaceAndPunctuationCharacters.Contains(chr) ||
                                char.IsPunctuation(chr))
                            {
                                while ((startIndex < textLength)
                                    && (((text[startIndex] > '\0') &&
                                        (text[startIndex] <= ' ')) ||
                                        LanguageLookup.NonAlphanumericAndSpaceAndPunctuationCharacters.Contains(text[charIndex]) ||
                                        char.IsPunctuation(text[charIndex])))
                                {
                                    startIndex++;
                                    charIndex++;
                                }
                            }
                            else if ((chr > ' ') && (chr <= '~'))
                            {
                                runLength = 0;

                                while ((charIndex < textLength) &&
                                    ((text[charIndex] > ' ') && (text[charIndex] <= '~')) &&
                                        !LanguageLookup.NonAlphanumericAndSpaceAndPunctuationCharacters.Contains(text[charIndex]) &&
                                        !char.IsPunctuation(text[charIndex]))
                                {
                                    charIndex++;
                                    runLength++;
                                }

                                if (runIndex == _WordRuns.Count())
                                {
                                    textRun = new TextRun(startIndex, runLength, null);
                                    _WordRuns.Add(textRun);
                                    runCount++;
                                }
                                else
                                {
                                    textRun = _WordRuns[runIndex];
                                    textRun.Start = startIndex;
                                    textRun.Length = runLength;
                                }

                                runIndex++;
                                startIndex = charIndex;
                            }

                            if (charIndex == textLength)
                                break;
                        }
                        else
                            charIndex++;
                    }
                }
            }
            else
            {
                for (charIndex = 0; charIndex <= textLength; )
                {
                    if (charIndex < textLength)
                        chr = text[charIndex];
                    else
                        chr = '\n';

                    if ((charIndex == textLength) ||
                        LanguageLookup.NonAlphanumericAndSpaceAndPunctuationCharacters.Contains(chr) ||
                        char.IsPunctuation(chr))
                    {
                        int skipOffset;

                        if (hasZeroWidthSpaces && (chr == ' '))
                        {
                            charIndex++;
                            continue;
                        }

                        if (chr == '.')
                        {
                            if (CheckForAbbreviation(text, charIndex, chr, out skipOffset))
                                charIndex += skipOffset;
                        }
                        else if ((chr == '\'') || (chr == '‘') || (chr == '’'))
                        {
                            if (CheckForApostrophe(text, charIndex, chr, out skipOffset))
                            {
                                charIndex += skipOffset;
                                continue;
                            }
                        }
                        else if ((chr == '[') || (chr == ']'))
                        {
                            if (CheckForBracketedCorrection(text, charIndex, chr, out skipOffset))
                            {
                                charIndex += skipOffset;
                                continue;
                            }
                        }

                        runLength = charIndex - startIndex;

                        if (runLength > 0)
                        {
                            runText = text.Substring(startIndex, runLength);

                            if (!char.IsDigit(runText[0]))
                            {
                                for (int i = runLength - 1; i > 0; i--)
                                {
                                    if (!char.IsDigit(runText[i]))
                                        break;

                                    runLength--;
                                }
                            }

                            if (runIndex == _WordRuns.Count())
                            {
                                textRun = new TextRun(startIndex, runLength, null);
                                _WordRuns.Add(textRun);
                                runCount++;
                            }
                            else
                            {
                                textRun = _WordRuns[runIndex];
                                textRun.Start = startIndex;
                                textRun.Length = runLength;
                            }

                            runIndex++;
                        }

                        // Skip to next word.
                        while ((charIndex < textLength) &&
                                (LanguageLookup.NonAlphanumericAndSpaceAndPunctuationCharacters.Contains(text[charIndex]) ||
                                    char.IsPunctuation(text[charIndex])))
                            charIndex++;

                        startIndex = charIndex;

                        if (charIndex == textLength)
                            break;
                    }
                    else
                        charIndex++;
                }
            }

            if (runIndex < runCount)
            {
                while (runCount > runIndex)
                {
                    runCount--;
                    _WordRuns.RemoveAt(runCount);
                }
            }

            Modified = true;
        }

        public void LoadSentenceAndWordRunsFromText(DictionaryRepository dictionary)
        {
            LoadSentenceRunsFromText();
            LoadWordRunsFromText(dictionary);
        }

        public void UpdateSentenceAndWordRunsFromText(
            bool needsSentenceParsing,
            bool needsWordParsing,
            DictionaryRepository dictionary)
        {
            if (needsSentenceParsing)
                LoadSentenceRunsFromText();
            else
                ResetSentenceRuns();

            if (needsWordParsing)
                LoadWordRunsFromText(dictionary);
            else
                ResetWordRuns();
        }

        public void UpdateWordRunsFromText(bool needsWordParsing, DictionaryRepository dictionary)
        {
            if (needsWordParsing)
                LoadWordRunsFromText(dictionary);
            else
                ResetWordRuns();
        }

        public void GetWordFragmentsAndRuns(ref List<string> wordFragments, ref List<TextRun> wordRuns)
        {
            if (wordFragments == null)
                wordFragments = new List<string>();
            else
                wordFragments.Clear();

            if (wordRuns == null)
                wordRuns = new List<TextRun>();
            else
                wordRuns.Clear();

            if ((_WordRuns == null) || (_WordRuns.Count == 0))
            {
                if (HasText())
                {
                    wordFragments.Add(Text);
                    wordRuns.Add(null);
                }

                return;
            }

            string text = _Text;
            int textIndex = 0;
            int runIndex;
            int runCount = WordRunCount();
            string wordFragment;
            int wordLength;
            TextRun wordRun = null;

            for (runIndex = 0; runIndex < runCount; runIndex++)
            {
                wordRun = GetWordRun(runIndex);

                if (textIndex < wordRun.Start)
                {
                    wordLength = wordRun.Start - textIndex;
                    wordFragment = text.Substring(textIndex, wordLength);
                    wordFragments.Add(wordFragment);
                    wordRuns.Add(null);
                    textIndex = wordRun.Start;
                }
                else
                    textIndex = wordRun.Start;

                wordLength = wordRun.Length;
                wordFragment = text.Substring(textIndex, wordLength);
                wordFragments.Add(wordFragment);
                wordRuns.Add(wordRun);
                textIndex += wordLength;
            }

            if (textIndex < TextLength)
            {
                wordLength = TextLength - textIndex;
                wordFragment = text.Substring(textIndex, wordLength);
                wordFragments.Add(wordFragment);
                wordRuns.Add(null);
            }
        }

        public void ClearWordMappings(List<LanguageID> languageIDs)
        {
            if (_WordRuns != null)
            {
                foreach (TextRun wordRun in _WordRuns)
                    wordRun.DeleteWordMapping(languageIDs);
            }

            if (_PhraseRuns != null)
            {
                foreach (TextRun phraseRun in _PhraseRuns)
                    phraseRun.DeleteWordMapping(languageIDs);
            }
        }

        public bool IsOverlapping(LanguageItem other)
        {
            if (other == null)
                return false;

            if (!String.IsNullOrEmpty(Text) && !String.IsNullOrEmpty(other.Text))
            {
                if (Text == other.Text)
                    return true;
            }
            else
                return true;

            return false;
        }

        // Merge media runs.
        public bool MergeMediaRuns(LanguageItem other)
        {
            bool returnValue = true;

            if (!MergeMediaRuns(_SentenceRuns, other.SentenceRuns))
                returnValue = false;

            if (!MergeMediaRuns(_WordRuns, other.WordRuns))
                returnValue = false;

            return returnValue;
        }

        // Merge media runs.
        public static bool MergeMediaRuns(List<TextRun> textRuns1, List<TextRun> textRuns2)
        {
            if ((textRuns1 == null) && (textRuns2 == null))
                return true;

            if ((textRuns1 == null) || (textRuns2 == null))
                return false;

            if (textRuns1.Count() != textRuns2.Count())
                return false;

            int count = textRuns1.Count();
            int index;
            TextRun textRun1;
            TextRun textRun2;
            bool returnValue = true;

            for (index = 0; index < count; index++)
            {
                textRun1 = textRuns1[index];
                textRun2 = textRuns2[index];

                if (!textRun1.MergeMediaRuns(textRun2))
                    returnValue = false;
            }

            return returnValue;
        }

        public void MergeAndJoin(LanguageItem other)
        {
            Merge(other);
            JoinSentenceRuns();
        }

        public void Merge(LanguageItem other)
        {
            if (other.SentenceRuns != null)
            {
                int oldLength = (_Text != null ? _Text.Length : 0);
                int start = oldLength;

                if (_SentenceRuns == null)
                    _SentenceRuns = new List<TextRun>(other.SentenceRuns.Count());

                foreach (TextRun sentenceRun in other.SentenceRuns)
                {
                    if (LanguageLookup.IsUseSpacesToSeparateWords(LanguageID))
                    {
                        _Text += " ";
                        start++;
                    }
                    _Text += other.GetRunText(sentenceRun);
                    TextRun newRun = new TextRun(sentenceRun);
                    newRun.Start = start;
                    _SentenceRuns.Add(newRun);
                    start += newRun.Length;

                    if ((_WordRuns != null) && (other.WordRuns != null))
                    {
                        int deltaOffset = newRun.Start - sentenceRun.Start;

                        foreach (TextRun wordRun in other.WordRuns)
                        {
                            if ((wordRun.Start >= sentenceRun.Start) && (wordRun.Stop <= sentenceRun.Stop))
                            {
                                TextRun newWordRun = new TextRun(wordRun);
                                newWordRun.Start += deltaOffset;
                                _WordRuns.Add(newWordRun);
                            }
                        }
                    }
                }
            }
        }

        // Merge items, contatenating differing language items with a "; " separator.
        public void MergeConcatSentences(LanguageItem other)
        {
            if (other.SentenceRuns != null)
            {
                int oldLength = (_Text != null ? _Text.Length : 0);
                int start = oldLength;

                if (_SentenceRuns == null)
                    _SentenceRuns = new List<TextRun>(other.SentenceRuns.Count());

                foreach (TextRun sentenceRun in other.SentenceRuns)
                {
                    if (HasSentenceRun(sentenceRun))
                        continue;
                    _Text += "; " + other.GetRunText(sentenceRun);
                    start += 2;
                    TextRun newRun = new TextRun(sentenceRun);
                    newRun.Start = start;
                    _SentenceRuns.Add(newRun);
                    start += newRun.Length;

                    if ((_WordRuns != null) && (other.WordRuns != null))
                    {
                        int deltaOffset = newRun.Start - sentenceRun.Start;

                        foreach (TextRun wordRun in other.WordRuns)
                        {
                            if ((wordRun.Start >= sentenceRun.Start) && (wordRun.Stop <= sentenceRun.Stop))
                            {
                                TextRun newWordRun = new TextRun(wordRun);
                                newWordRun.Start += deltaOffset;
                                _WordRuns.Add(newWordRun);
                            }
                        }
                    }
                }
            }
        }

        public void CollapseSentenceRuns()
        {
            JoinSentenceRuns();
        }

        public void JoinSentenceRuns()
        {
            List<TextRun> oldSentenceRuns = _SentenceRuns;
            TextRun newSentenceRun = new TextRun(0, _Text.Length, null);
            _SentenceRuns = new List<TextRun>(1) { newSentenceRun };
            _Modified = true;

            if (oldSentenceRuns != null)
            {
                List<MediaRun> newMediaRuns = new List<MediaRun>();

                foreach (TextRun sentenceRun in oldSentenceRuns)
                {
                    if (sentenceRun.MediaRuns == null)
                        continue;

                    foreach (MediaRun mediaRun in sentenceRun.MediaRuns)
                    {
                        if (!mediaRun.IsReference)
                        {
                            if (newMediaRuns.FirstOrDefault(x => !x.IsReference && (x.FileName == mediaRun.FileName)) == null)
                                newMediaRuns.Add(new MediaRun(mediaRun));
                        }
                        else
                        {
                            MediaRun mediaReferenceRun = newMediaRuns.FirstOrDefault(x => x.IsReference && (x.MediaItemKey == mediaRun.MediaItemKey) && (x.LanguageMediaItemKey == mediaRun.LanguageMediaItemKey));

                            if (mediaReferenceRun == null)
                                newMediaRuns.Add(new MediaRun(mediaRun));
                            else
                                mediaReferenceRun.JoinMediaRun(mediaRun);
                        }
                    }
                }

                if (newMediaRuns.Count() != 0)
                    newSentenceRun.MediaRuns = newMediaRuns;
            }
        }

        public void JoinSentenceRuns(int sentenceIndex, int sentenceCount)
        {
            List<TextRun> oldSentenceRuns = _SentenceRuns;
            List<MediaRun> newMediaRuns = new List<MediaRun>();
            int totalSentenceCount = SentenceRunCount();
            TextRun newSentenceRun = new TextRun(0, 0, null);
            TextRun sentenceRun;
            int index;

            if ((sentenceIndex >= totalSentenceCount) || (sentenceCount <= 0) || (sentenceCount > totalSentenceCount))
                return;

            _SentenceRuns = new List<TextRun>();

            for (index = 0; index < sentenceIndex; index++)
                _SentenceRuns.Add(oldSentenceRuns[index]);

            sentenceRun = oldSentenceRuns[index];
            newSentenceRun.Start = sentenceRun.Start;

            if (index + sentenceCount > totalSentenceCount)
                newSentenceRun.Length = TextLength - newSentenceRun.Start;
            else
            {
                sentenceRun = oldSentenceRuns[index + sentenceCount - 1];
                newSentenceRun.Length = sentenceRun.Stop - newSentenceRun.Start;
            }

            for (int i = 0; i < sentenceCount; i++, index++)
            {
                if (i >= oldSentenceRuns.Count())
                    break;

                sentenceRun = oldSentenceRuns[index];

                if (sentenceRun.MediaRuns == null)
                    continue;

                foreach (MediaRun mediaRun in sentenceRun.MediaRuns)
                {
                    if (!mediaRun.IsReference)
                    {
                        if (newMediaRuns.FirstOrDefault(x => !x.IsReference && (x.FileName == mediaRun.FileName)) == null)
                            newMediaRuns.Add(new MediaRun(mediaRun));
                    }
                    else
                    {
                        MediaRun mediaReferenceRun = newMediaRuns.FirstOrDefault(x => x.IsReference && (x.MediaItemKey == mediaRun.MediaItemKey) && (x.LanguageMediaItemKey == mediaRun.LanguageMediaItemKey));

                        if (mediaReferenceRun == null)
                            newMediaRuns.Add(new MediaRun(mediaRun));
                        else
                            mediaReferenceRun.JoinMediaRun(mediaRun);
                    }
                }
            }

            if (newMediaRuns.Count() != 0)
                newSentenceRun.MediaRuns = newMediaRuns;

            _SentenceRuns.Add(newSentenceRun);

            for (; index < totalSentenceCount; index++)
                _SentenceRuns.Add(oldSentenceRuns[index]);

            _Modified = true;
        }

        public void JoinSentenceRuns(List<TextRun> sentenceRuns)
        {
            if ((_SentenceRuns == null) || (sentenceRuns.Count() == 0))
                return;

            int startIndex = _SentenceRuns.IndexOf(sentenceRuns.First());
            int stopIndex = _SentenceRuns.IndexOf(sentenceRuns.Last());

            if (stopIndex > startIndex)
                JoinSentenceRuns(startIndex, (stopIndex - startIndex) + 1);
        }

        public void JoinSentenceRunsAt(int charIndex)
        {
            if (TextLength == 0)
                return;

            if ((SentenceRunCount() == 0) && (TextLength != 0))
                LoadSentenceRunsFromText();

            int sentenceIndex = 0;

            foreach (TextRun sentenceRun in SentenceRuns)
            {
                if (sentenceRun.Contains(charIndex))
                {
                    JoinSentenceRuns(sentenceIndex, 2);
                    break;
                }
                sentenceIndex++;
            }
        }

        public void SplitSentenceRuns()
        {
            int sentenceCount = SentenceRunCount();
            int sentenceIndex;

            if (sentenceCount == 0)
                LoadSentenceRunsFromText();
            else
            {
                for (sentenceIndex = sentenceCount - 1; sentenceIndex >= 0; sentenceIndex--)
                    SplitSentenceRun(sentenceIndex);
            }
        }

        public void SplitSentenceRun(int sentenceIndex)
        {
            if ((sentenceIndex == 0) && (SentenceRunCount() == 0) && (TextLength != 0))
            {
                LoadSentenceRunsFromText();
                return;
            }

            TextRun sentenceRun = GetSentenceRun(sentenceIndex);

            if (sentenceRun == null)
                return;

            string text = GetRunText(sentenceRun);

            LanguageItem languageItem = new LanguageItem(Key, LanguageID, text);

            languageItem.SplitSentenceRuns();

            if (languageItem.SentenceRunCount() <= 1)
                return;

            _SentenceRuns.RemoveAt(sentenceIndex);

            int runIndex = 0;

            foreach (TextRun splitRun in languageItem.SentenceRuns)
            {
                TextRun newRun = new TextRun(sentenceRun);
                newRun.Start = splitRun.Start;
                newRun.Length = splitRun.Length;
                _SentenceRuns.Insert(sentenceIndex + runIndex, newRun);
                runIndex++;
            }

            _Modified = true;
        }

        public int SplitSentenceRunAt(int charIndex)
        {
            if (TextLength == 0)
                return -1;

            if ((SentenceRunCount() == 0) && (TextLength != 0))
                LoadSentenceRunsFromText();

            int sentenceIndex = 0;

            foreach (TextRun sentenceRun in SentenceRuns)
            {
                if (sentenceRun.Contains(charIndex) && (charIndex != sentenceRun.Start) && (charIndex !=  sentenceRun.Stop))
                {
                    TextRun newRun = new TextRun(sentenceRun);
                    newRun.Start = charIndex;
                    newRun.Stop = sentenceRun.Stop;
                    sentenceRun.Stop = charIndex;
                    _SentenceRuns.Insert(sentenceIndex + 1, newRun);
                    NormalizeSentenceRunBoundaries(sentenceRun);
                    NormalizeSentenceRunBoundaries(newRun);
                    _Modified = true;
                    return sentenceIndex + 1;
                }
                sentenceIndex++;
            }

            return -1;
        }

        public bool SplitOrJoinSentenceRunsContaining(int start, int stop)
        {
            List<TextRun> sentenceRuns = FindSentenceRunsContaining(start, stop);

            if (sentenceRuns.Count() == 0)
                return false;
            else if (sentenceRuns.Count() == 1)
            {
                TextRun sentenceRun = sentenceRuns[0];

                if (sentenceRun.Start < start)
                {
                    int nextIndex = SplitSentenceRunAt(start);
                    if (nextIndex != -1)
                        sentenceRun = GetSentenceRun(nextIndex);
                }
                else if (sentenceRun.Start > start)
                {
                    sentenceRun.Start = start;
                    NormalizeSentenceRunBoundaries(sentenceRun);
                }

                if (sentenceRun.Stop < stop)
                {
                    sentenceRun.Stop = stop;
                    NormalizeSentenceRunBoundaries(sentenceRun);
                }
                else if (sentenceRun.Stop > stop)
                    SplitSentenceRunAt(stop);
            }
            else
                JoinSentenceRuns(sentenceRuns);

            return true;
        }

        public List<TextRun> FindSentenceRunsContaining(int start, int stop)
        {
            List<TextRun> sentenceRuns = new List<TextRun>();

            if (TextLength == 0)
                return sentenceRuns;

            if ((SentenceRunCount() == 0) && (TextLength != 0))
                LoadSentenceRunsFromText();

            foreach (TextRun sentenceRun in SentenceRuns)
            {
                if (sentenceRun.Contains(start))
                    sentenceRuns.Add(sentenceRun);
                else if (sentenceRun.Contains(stop - 1))
                    sentenceRuns.Add(sentenceRun);
                else if ((sentenceRun.Start >= start) && (sentenceRun.Stop < stop))
                    sentenceRuns.Add(sentenceRun);
            }

            return sentenceRuns;
        }

        public List<TextRun> FindSentenceRunsContainingAndClone(int start, int stop)
        {
            List<TextRun> sentenceRuns = FindSentenceRunsContaining(start, stop);
            List<TextRun> newRuns = null;

            if (sentenceRuns == null)
                return null;

            newRuns = new List<TextRun>();

            foreach (TextRun sentenceRun in sentenceRuns)
                newRuns.Add(new Content.TextRun(sentenceRun));

            return newRuns;
        }

        public bool FindSentenceRunContaining(int offset, out TextRun sentenceRun, out int index)
        {
            sentenceRun = null;
            index = -1;

            if (TextLength == 0)
                return false;

            if ((SentenceRunCount() == 0) && (TextLength != 0))
                LoadSentenceRunsFromText();

            index = 0;

            foreach (TextRun textRun in SentenceRuns)
            {
                if (textRun.Contains(offset))
                {
                    sentenceRun = textRun;
                    return true;
                }

                index++;
            }

            return false;
        }

        public void JoinWordRuns()
        {
            List<TextRun> oldWordRuns = _WordRuns;
            TextRun newWordRun = new TextRun(0, _Text.Length, null);
            _WordRuns = new List<TextRun>(1) { newWordRun };
            _Modified = true;

            if (oldWordRuns != null)
            {
                List<MediaRun> newMediaRuns = new List<MediaRun>();

                foreach (TextRun wordRun in oldWordRuns)
                {
                    if (wordRun.MediaRuns == null)
                        continue;

                    foreach (MediaRun mediaRun in wordRun.MediaRuns)
                    {
                        if (!mediaRun.IsReference)
                        {
                            if (newMediaRuns.FirstOrDefault(x => !x.IsReference && (x.FileName == mediaRun.FileName)) == null)
                                newMediaRuns.Add(new MediaRun(mediaRun));
                        }
                        else
                        {
                            MediaRun mediaReferenceRun = newMediaRuns.FirstOrDefault(x => x.IsReference && (x.MediaItemKey == mediaRun.MediaItemKey) && (x.LanguageMediaItemKey == mediaRun.LanguageMediaItemKey));

                            if (mediaReferenceRun == null)
                                newMediaRuns.Add(new MediaRun(mediaRun));
                            else
                                mediaReferenceRun.JoinMediaRun(mediaRun);
                        }
                    }
                }

                if (newMediaRuns.Count() != 0)
                    newWordRun.MediaRuns = newMediaRuns;
            }
        }

        public void JoinWordRuns(int wordIndex, int wordCount)
        {
            List<TextRun> oldWordRuns = _WordRuns;
            int oldWordCount = (oldWordRuns != null ? oldWordRuns.Count() : 0);
            List<MediaRun> newMediaRuns = new List<MediaRun>();
            int totalWordCount = WordRunCount();
            TextRun newWordRun = new TextRun(0, 0, null);
            TextRun wordRun;
            int index;

            if ((wordIndex >= totalWordCount) || (wordCount <= 0) || (wordCount > totalWordCount))
                return;

            _WordRuns = new List<TextRun>();

            for (index = 0; index < wordIndex; index++)
                _WordRuns.Add(oldWordRuns[index]);

            wordRun = oldWordRuns[index];
            newWordRun.Start = wordRun.Start;

            if (index + wordCount > totalWordCount)
                newWordRun.Length = TextLength - newWordRun.Start;
            else
            {
                wordRun = oldWordRuns[index + wordCount - 1];
                newWordRun.Length = wordRun.Stop - newWordRun.Start;
            }

            for (int i = 0; i < wordCount; i++, index++)
            {
                if (index >= oldWordRuns.Count())
                    break;

                wordRun = oldWordRuns[index];

                if (wordRun.MediaRuns == null)
                    continue;

                foreach (MediaRun mediaRun in wordRun.MediaRuns)
                {
                    if (!mediaRun.IsReference)
                    {
                        if (newMediaRuns.FirstOrDefault(x => !x.IsReference && (x.FileName == mediaRun.FileName)) == null)
                            newMediaRuns.Add(new MediaRun(mediaRun));
                    }
                    else
                    {
                        MediaRun mediaReferenceRun = newMediaRuns.FirstOrDefault(x => x.IsReference && (x.MediaItemKey == mediaRun.MediaItemKey) && (x.LanguageMediaItemKey == mediaRun.LanguageMediaItemKey));

                        if (mediaReferenceRun == null)
                            newMediaRuns.Add(new MediaRun(mediaRun));
                        else
                            mediaReferenceRun.JoinMediaRun(mediaRun);
                    }
                }
            }

            if (newMediaRuns.Count() != 0)
                newWordRun.MediaRuns = newMediaRuns;

            _WordRuns.Add(newWordRun);

            for (; index < totalWordCount; index++)
                _WordRuns.Add(oldWordRuns[index]);

            _Modified = true;
        }

        public void JoinWordRuns(List<TextRun> wordRuns)
        {
            if ((_WordRuns == null) || (wordRuns.Count() == 0))
                return;

            int startIndex = _WordRuns.IndexOf(wordRuns.First());
            int stopIndex = _WordRuns.IndexOf(wordRuns.Last());

            if (stopIndex > startIndex)
                JoinWordRuns(startIndex, (stopIndex - startIndex) + 1);
        }

        public void JoinWordRunsAt(int charIndex)
        {
            if (TextLength == 0)
                return;

            if ((WordRunCount() == 0) && (TextLength != 0))
                LoadWordRunsFromText(ApplicationData.Repositories.Dictionary);

            int wordIndex = 0;

            foreach (TextRun wordRun in WordRuns)
            {
                if (wordRun.Contains(charIndex))
                {
                    JoinWordRuns(wordIndex, 2);
                    break;
                }
                wordIndex++;
            }
        }

        public int SplitWordRunAt(int charIndex)
        {
            if (TextLength == 0)
                return -1;

            if ((WordRunCount() == 0) && (TextLength != 0))
                LoadWordRunsFromText(ApplicationData.Repositories.Dictionary);

            int wordIndex = 0;

            foreach (TextRun wordRun in WordRuns)
            {
                if (wordRun.Contains(charIndex) && (charIndex != wordRun.Start) && (charIndex != wordRun.Stop))
                {
                    TextRun newRun = new TextRun(wordRun);
                    newRun.Start = charIndex;
                    newRun.Stop = wordRun.Stop;
                    wordRun.Stop = charIndex;
                    _WordRuns.Insert(wordIndex + 1, newRun);
                    NormalizeWordRunBoundaries(wordRun);
                    NormalizeWordRunBoundaries(newRun);
                    FixupRomanizedSpacesUsingWordRuns();
                    _Modified = true;
                    return wordIndex + 1;
                }
                wordIndex++;
            }

            return -1;
        }

        public bool SplitOrJoinWordRunsContaining(int start, int stop)
        {
            List<TextRun> wordRuns = FindWordRunsContaining(start, stop);

            if (wordRuns.Count() == 0)
            {
                int lastCharIndex = 0;
                if ((_WordRuns != null) && (_WordRuns.Count() != 0))
                    lastCharIndex = _WordRuns.Last().Stop;
                if ((start >= lastCharIndex) && (start < TextLength) && (stop > start) && (stop <= TextLength))
                {
                    TextRun wordRun = new TextRun(start, stop - start, null);
                    NormalizeWordRunBoundaries(wordRun);
                    if (_WordRuns == null)
                        _WordRuns = new List<TextRun>() { wordRun };
                    else
                        _WordRuns.Add(wordRun);
                    _Modified = true;
                    return true;
                }
                else if ((_WordRuns != null) && (_WordRuns.Count() != 0))
                {
                    int wordCount = _WordRuns.Count() - 1;
                    int wordIndex;
                    for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
                    {
                        TextRun beforeRun = _WordRuns[wordIndex];
                        TextRun afterRun = _WordRuns[wordIndex + 1];
                        if ((start >= beforeRun.Stop) && (stop <= afterRun.Start))
                        {
                            TextRun wordRun = new TextRun(start, stop - start, null);
                            NormalizeWordRunBoundaries(wordRun);
                            _WordRuns.Insert(wordIndex + 1, wordRun);
                            _Modified = true;
                            return true;
                        }
                    }
                }
                return false;
            }
            else if (wordRuns.Count() == 1)
            {
                TextRun wordRun = wordRuns[0];

                if (wordRun.Start < start)
                {
                    int nextIndex = SplitWordRunAt(start);
                    if (nextIndex != -1)
                        wordRun = GetWordRun(nextIndex);
                }
                else if (wordRun.Start > start)
                {
                    wordRun.Start = start;
                    NormalizeWordRunBoundaries(wordRun);
                }

                if (wordRun.Stop < stop)
                {
                    wordRun.Stop = stop;
                    NormalizeWordRunBoundaries(wordRun);
                }
                else if (wordRun.Stop > stop)
                    SplitWordRunAt(stop);
                else
                    NormalizeWordRunBoundaries(wordRun);
            }
            else
            {
                int saveLength = TextLength;
                JoinWordRuns(wordRuns);
                int newLength = TextLength;
                if (newLength != saveLength)
                    stop -= saveLength - newLength;
                wordRuns = FindWordRunsContaining(start, stop);

                if (wordRuns.Count() == 0)
                    return false;
                else if (wordRuns.Count() == 1)
                {
                    TextRun wordRun = wordRuns[0];

                    if (wordRun.Start < start)
                    {
                        int nextIndex = SplitWordRunAt(start);
                        if (nextIndex != -1)
                            wordRun = GetWordRun(nextIndex);
                    }
                    else if (wordRun.Start > start)
                    {
                        wordRun.Start = start;
                        NormalizeWordRunBoundaries(wordRun);
                    }

                    if (wordRun.Stop < stop)
                    {
                        wordRun.Stop = stop;
                        NormalizeWordRunBoundaries(wordRun);
                    }
                    else if (wordRun.Stop > stop)
                        SplitWordRunAt(stop);
                    else
                        NormalizeWordRunBoundaries(wordRun);
                }
                else
                    return false;
            }

            return true;
        }

        public int FindWordRunIndexContaining(int textIndex)
        {
            List<TextRun> wordRuns = new List<TextRun>();

            if (TextLength == 0)
                return -1;

            if ((WordRunCount() == 0) && (TextLength != 0))
                LoadWordRunsFromText(ApplicationData.Repositories.Dictionary);

            int index = 0;

            foreach (TextRun wordRun in WordRuns)
            {
                if (wordRun.Contains(textIndex))
                    return index;

                index++;
            }

            return -1;
        }

        public List<TextRun> FindWordRunsContaining(int start, int stop)
        {
            List<TextRun> wordRuns = new List<TextRun>();

            if (TextLength == 0)
                return wordRuns;

            if ((WordRunCount() == 0) && (TextLength != 0))
                LoadWordRunsFromText(ApplicationData.Repositories.Dictionary);

            foreach (TextRun wordRun in WordRuns)
            {
                if (wordRun.Contains(start))
                    wordRuns.Add(wordRun);
                else if (wordRun.Contains(stop - 1))
                    wordRuns.Add(wordRun);
                else if ((wordRun.Start >= start) && (wordRun.Stop < stop))
                    wordRuns.Add(wordRun);
            }

            return wordRuns;
        }

        public List<TextRun> FindWordRunsContainingAndClone(int start, int stop)
        {
            List<TextRun> wordRuns = FindWordRunsContaining(start, stop);
            List<TextRun> newRuns = null;

            if (wordRuns == null)
                return null;

            newRuns = new List<TextRun>();

            foreach (TextRun wordRun in wordRuns)
                newRuns.Add(new Content.TextRun(wordRun));

            return newRuns;
        }

        public void NormalizeSentenceRunBoundaries(TextRun run)
        {
            int start = run.Start;
            int stop = run.Stop;

            for (; start < stop; start++)
            {
                char c = Text[start];
                if (!LanguageLookup.SpaceCharacters.Contains(c))
                    break;
            }

            for (; stop > start; stop--)
            {
                char c = Text[stop - 1];
                if (!LanguageLookup.SpaceCharacters.Contains(c))
                    break;
            }

            run.Start = start;
            run.Stop = stop;
        }

        public void NormalizeWordRunBoundaries(TextRun run)
        {
            int start = run.Start;
            int stop = run.Stop;

            for (; start < stop; start++)
            {
                char c = Text[start];
                if (!LanguageLookup.SpaceAndPunctuationCharacters.Contains(c))
                    break;
            }

            for (; stop > start; stop--)
            {
                char c = Text[stop - 1];
                if (!LanguageLookup.SpaceAndPunctuationCharacters.Contains(c))
                    break;
            }

            run.Start = start;
            run.Stop = stop;
        }

        public bool HasAlignment(LanguageID languageID)
        {
            if (_WordRuns == null)
                return false;

            foreach (TextRun wordRun in _WordRuns)
            {
                if (wordRun.HasAlignment(languageID))
                    return true;
            }

            return false;
        }

        public void FixupRomanizedSpacesUsingWordRuns()
        {
            if (!LanguageLookup.IsRomanized(LanguageID))
                return;

            List<TextRun> wordRuns = WordRuns;

            if ((wordRuns == null) || (wordRuns.Count() == 0))
                return;

            int runCount = wordRuns.Count();
            int runIndex;
            int textOffset = 0;
            string text = Text;
            int textLength = text.Length;

            for (runIndex = 0; runIndex < runCount; runIndex++)
            {
                TextRun wordRun = wordRuns[runIndex];

                if (textOffset != 0)
                    wordRun.Start = wordRun.Start + textOffset;

                int nextCharIndex = wordRun.Stop;

                if (nextCharIndex < textLength)
                {
                    char nextChar = text[nextCharIndex];

                    if (!char.IsWhiteSpace(nextChar) && !char.IsPunctuation(nextChar))
                    {
                        text = text.Insert(nextCharIndex, " ");
                        textOffset++;
                        textLength++;
                    }
                }
            }

            Text = text;
        }

        public List<TextRun> PhrasedWordRuns
        {
            get
            {
                if ((_PhraseRuns == null) || (_PhraseRuns.Count() == 0))
                    return _WordRuns;

                if ((_WordRuns == null) || (_WordRuns.Count() == 0))
                    return _WordRuns;

                int phraseCount = _PhraseRuns.Count();
                int phraseIndex;
                TextRun phraseRun;
                List<TextRun> runs = new List<TextRun>(_WordRuns);

                for (phraseIndex = 0; phraseIndex < phraseCount; phraseIndex++)
                {
                    int wordCount = runs.Count();
                    int wordIndex;

                    phraseRun = _PhraseRuns[phraseIndex];

                    for (wordIndex = 0; (wordIndex < wordCount) && (runs[wordIndex].Start != phraseRun.Start); wordIndex++)
                        ;

                    if (wordIndex == wordCount)
                    {
                        PutPhraseAndWordRunStartErrorMessage(
                            "PhrasedWordRuns",
                            phraseIndex,
                            phraseRun,
                            false);
                        continue;
                    }

                    int wordRunIndexPhraseStart = wordIndex;

                    for (; (wordIndex < wordCount) && (runs[wordIndex].Stop != phraseRun.Stop); wordIndex++)
                        ;

                    if (wordIndex == wordCount)
                    {
                        PutPhraseAndWordRunStartErrorMessage(
                            "PhrasedWordRuns",
                            phraseIndex,
                            phraseRun,
                            true);
                        continue;
                    }

                    int removeCount = (wordIndex - wordRunIndexPhraseStart) + 1;

                    runs.RemoveRange(wordRunIndexPhraseStart, removeCount);

                    runs.Insert(wordRunIndexPhraseStart, phraseRun);
                }

                return runs;
            }
        }

        public List<TextRun> GetSentencePhrasedWordRuns(TextRun sentenceRun)
        {
            if ((_PhraseRuns == null) || (_PhraseRuns.Count() == 0))
                return GetSentenceWordRuns(sentenceRun);

            if ((_WordRuns == null) || (_WordRuns.Count() == 0))
                return GetSentenceWordRuns(sentenceRun);

            int phraseCount = _PhraseRuns.Count();
            int phraseIndex;
            TextRun phraseRun;
            List<TextRun> runs = new List<TextRun>(GetSentenceWordRuns(sentenceRun));

            for (phraseIndex = 0; phraseIndex < phraseCount; phraseIndex++)
            {
                int wordCount = runs.Count();
                int wordIndex;

                phraseRun = _PhraseRuns[phraseIndex];

                if (!sentenceRun.Contains(phraseRun.Start))
                    continue;

                for (wordIndex = 0; (wordIndex < wordCount) && (runs[wordIndex].Start != phraseRun.Start); wordIndex++)
                    ;

                if (wordIndex == wordCount)
                    PutPhraseAndWordRunStartErrorMessage(
                        "GetSentencePhrasedWordRuns",
                        phraseIndex,
                        phraseRun,
                        false);

                int wordRunIndexPhraseStart = wordIndex;

                for (; (wordIndex < wordCount) && (runs[wordIndex].Stop != phraseRun.Stop); wordIndex++)
                    ;

                if (wordIndex == wordCount)
                    PutPhraseAndWordRunStartErrorMessage(
                        "GetSentencePhrasedWordRuns",
                        phraseIndex,
                        phraseRun,
                        true);

                int removeCount = (wordIndex - wordRunIndexPhraseStart) + 1;

                runs.RemoveRange(wordRunIndexPhraseStart, removeCount);

                runs.Insert(wordRunIndexPhraseStart, phraseRun);
            }

            return runs;
        }

        public List<TextRun> GetSentencePhrasedWordRuns(int sentenceIndex)
        {
            TextRun sentenceRun = GetSentenceRun(sentenceIndex);

            if (sentenceRun != null)
                return GetSentencePhrasedWordRuns(sentenceRun);

            return null;
        }

        public List<TextRun> GetSentencePhraseRunsRetargeted(TextRun sentenceRun)
        {
            int start = sentenceRun.Start;
            int stop = sentenceRun.Stop;
            List<TextRun> phraseRuns = new List<TextRun>();

            if (HasPhraseRuns())
            {
                List<TextRun> sourcePhraseRuns = PhraseRuns;

                foreach (TextRun sourcePhraseRun in sourcePhraseRuns)
                {
                    int phraseStart = sourcePhraseRun.Start;
                    int phraseStop = sourcePhraseRun.Stop;

                    if (phraseStop <= start)
                        continue;
                    else if (phraseStart >= stop)
                        continue;

                    TextRun newWordRun = new TextRun(
                        phraseStart - start,
                        sourcePhraseRun.Length,
                        null);

                    phraseRuns.Add(newWordRun);
                }
            }

            return phraseRuns;
        }

        public List<TextRun> GetPhrasedOrUnphrasedWordRuns(bool usePhrases)
        {
            if (usePhrases)
                return PhrasedWordRuns;
            else
                return _WordRuns;
        }

        public List<TextRun> GetFilteredPhrasedWordRuns(LanguageID hostLanguageID)
        {
            if ((_PhraseRuns == null) || (_PhraseRuns.Count() == 0))
                return _WordRuns;

            if ((_WordRuns == null) || (_WordRuns.Count() == 0))
                return _WordRuns;

            int phraseCount = _PhraseRuns.Count();
            int phraseIndex;
            TextRun phraseRun;
            List<TextRun> runs = new List<TextRun>(_WordRuns);

            for (phraseIndex = 0; phraseIndex < phraseCount; phraseIndex++)
            {
                phraseRun = _PhraseRuns[phraseIndex];

                if ((phraseRun.PhraseHostLanguageIDsCount() != 0) && !phraseRun.HasPhraseHostLanguageID(hostLanguageID))
                {
                    int wordCount = runs.Count();
                    int wordIndex;

                    for (wordIndex = 0; (wordIndex < wordCount) && (runs[wordIndex].Start != phraseRun.Start); wordIndex++)
                        ;

                    if (wordIndex == wordCount)
                        PutPhraseAndWordRunStartErrorMessage(
                            "GetFilteredPhrasedWordRuns",
                            phraseIndex,
                            phraseRun,
                            false);

                    int wordRunIndexPhraseStart = wordIndex;

                    for (; (wordIndex < wordCount) && (runs[wordIndex].Stop != phraseRun.Stop); wordIndex++)
                        ;

                    if (wordIndex == wordCount)
                        PutPhraseAndWordRunStartErrorMessage(
                            "GetFilteredPhrasedWordRuns",
                            phraseIndex,
                            phraseRun,
                            true);

                    int removeCount = (wordIndex - wordRunIndexPhraseStart) + 1;

                    runs.RemoveRange(wordRunIndexPhraseStart, removeCount);

                    runs.Insert(wordRunIndexPhraseStart, phraseRun);
                }
            }

            return runs;
        }

        public List<TextRun> PhraseRuns
        {
            get
            {
                return _PhraseRuns;
            }
            set
            {
                if (value != _PhraseRuns)
                {
                    _Modified = true;
                    _PhraseRuns = value;
                }
            }
        }


        public bool HasPhraseRuns()
        {
            if ((_PhraseRuns != null) && (_PhraseRuns.Count() != 0))
                return true;
            return false;
        }

        public int PhraseRunCount()
        {
            if (_PhraseRuns != null)
                return _PhraseRuns.Count();
            return 0;
        }

        public bool HasPhraseRun(TextRun phraseRun)
        {
            if (_PhraseRuns != null)
            {
                foreach (TextRun textRun in _PhraseRuns)
                {
                    if (TextRun.Compare(phraseRun, textRun) == 0)
                        return true;
                }
            }

            return false;
        }

        public List<TextRun> GetSentencePhraseRuns(TextRun sentenceRun)
        {
            int start = sentenceRun.Start;
            int stop = sentenceRun.Stop;
            List<TextRun> phraseRuns = new List<TextRun>();

            if (HasPhraseRuns())
            {
                List<TextRun> sourcePhraseRuns = PhraseRuns;

                foreach (TextRun sourcePhraseRun in sourcePhraseRuns)
                {
                    int phraseStart = sourcePhraseRun.Start;
                    int phraseStop = sourcePhraseRun.Stop;

                    if (phraseStop <= start)
                        continue;
                    else if (phraseStart >= stop)
                        continue;

                    phraseRuns.Add(sourcePhraseRun);
                }
            }

            return phraseRuns;
        }

        public TextRun FindPhraseRun(int textStartIndex, int textStopIndex)
        {
            if (_PhraseRuns == null)
                return null;

            foreach (TextRun phraseRun in _PhraseRuns)
            {
                if ((phraseRun.Start == textStartIndex) && (phraseRun.Stop == textStopIndex))
                    return phraseRun;
            }

            return null;
        }

        public bool HasOverlappingPhraseRun(int textStartIndex, int textStopIndex)
        {
            if ((_PhraseRuns == null) || (_PhraseRuns.Count() == 0))
                return false;

            foreach (TextRun phraseRun in _PhraseRuns)
            {
                if (phraseRun.Contains(textStartIndex))
                    return true;
                else if (phraseRun.Contains(textStopIndex - 1))
                    return true;
                else if ((phraseRun.Start >= textStartIndex) && (phraseRun.Stop < textStopIndex))
                    return true;
            }

            return false;
        }

        public List<TextRun> FindPhraseRunsContaining(int start, int stop)
        {
            if ((_PhraseRuns == null) || (_PhraseRuns.Count() == 0))
                return null;

            List<TextRun> phraseRuns = new List<TextRun>();

            foreach (TextRun phraseRun in _PhraseRuns)
            {
                if (phraseRun.Contains(start))
                    phraseRuns.Add(phraseRun);
                else if (phraseRun.Contains(stop - 1))
                    phraseRuns.Add(phraseRun);
                else if ((phraseRun.Start >= start) && (phraseRun.Stop < stop))
                    phraseRuns.Add(phraseRun);
            }

            return phraseRuns;
        }

        public TextRun FindLongestPhraseRunStarting(int textStartIndex)
        {
            if (_PhraseRuns == null)
                return null;

            int bestLength = -1;
            TextRun bestPhraseRun = null;

            foreach (TextRun phraseRun in _PhraseRuns)
            {
                if (phraseRun.Start == textStartIndex)
                {
                    if (phraseRun.Length > bestLength)
                    {
                        bestLength = phraseRun.Length;
                        bestPhraseRun = phraseRun;
                    }
                }
            }

            return bestPhraseRun;
        }

        public List<TextRun> FindPhraseRunsContaining(int textIndex)
        {
            List<TextRun> phraseRuns = null;

            if (_PhraseRuns == null)
                return null;

            foreach (TextRun phraseRun in _PhraseRuns)
            {
                if (phraseRun.Contains(textIndex))
                {
                    if (phraseRuns == null)
                        phraseRuns = new List<TextRun>() { phraseRun };
                    else
                        phraseRuns.Add(phraseRun);
                }
            }

            return phraseRuns;
        }

        public List<TextRun> FindPhraseRunsStarting(int textIndex)
        {
            List<TextRun> phraseRuns = null;

            if (_PhraseRuns == null)
                return null;

            foreach (TextRun phraseRun in _PhraseRuns)
            {
                if (phraseRun.Start == textIndex)
                {
                    if (phraseRuns == null)
                        phraseRuns = new List<TextRun>() { phraseRun };
                    else
                        phraseRuns.Add(phraseRun);
                }
            }

            return phraseRuns;
        }

        public TextRun GetPhraseRun(int index)
        {
            if ((_PhraseRuns != null) && (index >= 0) && (index < _PhraseRuns.Count()))
                return _PhraseRuns[index];
            return null;
        }

        public string GetPhraseRunText(int phraseIndex)
        {
            TextRun textRun = GetPhraseRun(phraseIndex);
            return GetRunText(textRun);
        }

        public bool SetPhraseRunText(int index, string text)
        {
            if ((_PhraseRuns != null) && (index >= 0) && (index < _PhraseRuns.Count()))
            {
                TextRun phraseRun = _PhraseRuns[index];
                int phraseStart = phraseRun.Start;
                int oldLength = phraseRun.Length;
                int newLength = text.Length;
                int delta = newLength - oldLength;

                _Text = _Text.Remove(phraseStart, oldLength);
                _Text = _Text.Insert(phraseStart, text);

                phraseRun.Length = newLength;
                _Modified = true;

                int phraseCount = _PhraseRuns.Count();

                index++;

                if (oldLength != 0)
                {
                    while (index < phraseCount)
                    {
                        phraseRun = _PhraseRuns[index];
                        phraseRun.Start = phraseRun.Start + delta;
                        index++;
                    }
                }

                return true;
            }

            return false;
        }

        public TextRun GetMatchedPhraseRun(int start, int stop)
        {
            if (_PhraseRuns != null)
            {
                foreach (TextRun phraseRun in _PhraseRuns)
                {
                    if ((phraseRun.Start == start) && (phraseRun.Stop == stop))
                        return phraseRun;
                }
            }

            return null;
        }

        public List<string> GetPhrases()
        {
            List<string> phrases = new List<string>();

            if ((_PhraseRuns == null) || (_PhraseRuns.Count() == 0))
                phrases.Add(Text.Trim());
            else
            {
                foreach (TextRun textRun in _PhraseRuns)
                {
                    string phrase = GetRunText(textRun);
                    phrases.Add(phrase);
                }
            }

            return phrases;
        }

        public List<string> GetUniquePhrases()
        {
            List<string> phrases = new List<string>();

            if ((_PhraseRuns == null) || (_PhraseRuns.Count() == 0))
                phrases.Add(Text.Trim());
            else
            {
                HashSet<string> hashSet = new HashSet<string>();

                foreach (TextRun textRun in _PhraseRuns)
                {
                    string phrase = GetRunText(textRun);

                    if (hashSet.Add(phrase))
                        phrases.Add(phrase);
                }
            }

            return phrases;
        }

        public List<TextRun> ExtractPhraseRuns(int start, int stop, int baseIndex)
        {
            List<TextRun> phraseRuns = new List<TextRun>();

            if (HasPhraseRuns())
            {
                List<TextRun> sourcePhraseRuns = PhraseRuns;

                foreach (TextRun sourcePhraseRun in sourcePhraseRuns)
                {
                    int phraseStart = sourcePhraseRun.Start;
                    int phraseStop = sourcePhraseRun.Stop;

                    if (phraseStop <= start)
                        continue;
                    else if (phraseStart >= stop)
                        continue;

                    List<MediaRun> sourceMediaRuns = sourcePhraseRun.MediaRuns;

                    if (phraseStart < start)
                    {
                        phraseStart = start;
                        sourceMediaRuns = null;
                    }

                    if (phraseStop > stop)
                    {
                        phraseStop = stop;
                        sourceMediaRuns = null;
                    }

                    List<MediaRun> phraseMediaRuns = null;

                    if (sourceMediaRuns != null)
                    {
                        phraseMediaRuns = new List<MediaRun>(sourceMediaRuns.Count());

                        foreach (MediaRun sourceMediaRun in sourceMediaRuns)
                        {
                            MediaRun phraseMediaRun = new MediaRun(sourceMediaRun);
                            phraseMediaRuns.Add(phraseMediaRun);
                        }
                    }

                    phraseStart = (phraseStart - start) + baseIndex;
                    phraseStop = (phraseStop - start) + baseIndex;

                    TextRun phraseRun = new TextRun(phraseStart, phraseStop - phraseStart, phraseMediaRuns);
                    phraseRuns.Add(phraseRun);
                }
            }

            return phraseRuns;
        }

        public void AddPhraseRun(TextRun phraseRun)
        {
            if (_PhraseRuns == null)
                _PhraseRuns = new List<TextRun>() { phraseRun };
            else if (_PhraseRuns.Count() == 0)
                _PhraseRuns.Add(phraseRun);
            else
            {
                int c = _PhraseRuns.Count();
                bool added = false;
                for (int i = 0; i > c; i++)
                {
                    TextRun oldPhraseRun = _PhraseRuns[i];
                    if (phraseRun.Start < oldPhraseRun.Start)
                    {
                        _PhraseRuns.Insert(i, phraseRun);
                        added = true;
                        break;
                    }
                    else if (phraseRun.Start == oldPhraseRun.Start)
                    {
                        if (phraseRun.Stop < oldPhraseRun.Stop)
                        {
                            _PhraseRuns.Insert(i, phraseRun);
                            added = true;
                            break;
                        }
                        else if (phraseRun.Stop == oldPhraseRun.Stop)
                            return;
                    }
                }
                if (!added)
                    _PhraseRuns.Add(phraseRun);
            }

            _Modified = true;
        }

        public void DeletePhraseRun(TextRun phraseRun)
        {
            if (_PhraseRuns != null)
            {
                _PhraseRuns.Remove(phraseRun);
                _Modified = true;
            }
        }

        public bool ValidatePhraseRuns(out string errorMessage)
        {
            bool returnValue = true;

            errorMessage = null;

            if ((_PhraseRuns == null) || (_PhraseRuns.Count() == 0))
                return true;

            if ((_WordRuns == null) || (_WordRuns.Count() == 0))
            {
                errorMessage = "ValidatePhraseRuns: Phrase runs not empty but word runs are empty.";
                return false;
            }

            int phraseCount = _PhraseRuns.Count();
            int phraseIndex;
            int lastStart = -1;

            for (phraseIndex = 0; phraseIndex < phraseCount; phraseIndex++)
            {
                TextRun phraseRun = _PhraseRuns[phraseIndex];
                TextRun wordRunStart = _WordRuns.FirstOrDefault(x => x.Start == phraseRun.Start);
                TextRun wordRunStop = _WordRuns.FirstOrDefault(x => x.Stop == phraseRun.Stop);

                if (phraseRun.Start < 0)
                {
                    errorMessage = TextUtilities.AppendErrorMessage(
                        errorMessage,
                        "ValidatePhraseRuns: Phrase run start is less than 0: phrase index "
                            + phraseIndex.ToString()
                            + " phrase start Index: "
                            + phraseRun.Start.ToString()
                            + " last phrase index: "
                            + lastStart
                            + " phrase: "
                            + GetRunText(phraseRun),
                        true);
                    returnValue = false;
                }

                if (phraseRun.Stop > TextLength)
                {
                    errorMessage = TextUtilities.AppendErrorMessage(
                        errorMessage,
                        "ValidatePhraseRuns: Phrase run stop is greater than text length: phrase index "
                            + phraseIndex.ToString()
                            + " phrase start Index: "
                            + phraseRun.Start.ToString()
                            + " last phrase index: "
                            + lastStart
                            + " phrase: "
                            + GetRunText(phraseRun),
                        true);
                    returnValue = false;
                }

                if (phraseRun.Length == 0)
                {
                    errorMessage = TextUtilities.AppendErrorMessage(
                        errorMessage,
                        "ValidatePhraseRuns: Phrase run length is 0: phrase index "
                            + phraseIndex.ToString()
                            + " phrase start Index: "
                            + phraseRun.Start.ToString()
                            + " last phrase index: "
                            + lastStart
                            + " phrase: "
                            + GetRunText(phraseRun),
                        true);
                    returnValue = false;
                }

                if (phraseRun.Stop < phraseRun.Start)
                {
                    errorMessage = TextUtilities.AppendErrorMessage(
                        errorMessage,
                        "ValidatePhraseRuns: Phrase run stop is less that its stop: phrase index "
                            + phraseIndex.ToString()
                            + " phrase start Index: "
                            + phraseRun.Start.ToString()
                            + " last phrase index: "
                            + lastStart
                            + " phrase: "
                            + GetRunText(phraseRun),
                        true);
                    returnValue = false;
                }

                if (phraseRun.Start < lastStart)
                {
                    errorMessage = TextUtilities.AppendErrorMessage(
                        errorMessage,
                        "ValidatePhraseRuns: Phrase run out of order: phrase index "
                            + phraseIndex.ToString()
                            + " phrase start Index: "
                            + phraseRun.Start.ToString()
                            + " last phrase index: "
                            + lastStart
                            + " phrase: "
                            + GetRunText(phraseRun),
                        true);
                    returnValue = false;
                }

                if (wordRunStart == null)
                {
                    errorMessage = TextUtilities.AppendErrorMessage(
                        errorMessage,
                        "ValidatePhraseRuns: Word run with phrase start not found: "
                            + " phrase index "
                            + phraseIndex.ToString()
                            + " phrase run: "
                            + phraseRun.ToString()
                            + " phrase: "
                            + GetRunText(phraseRun),
                        true);
                    returnValue = false;
                }

                if (wordRunStop == null)
                {
                    errorMessage = TextUtilities.AppendErrorMessage(
                        errorMessage,
                        "ValidatePhraseRuns: Word run with phrase stop not found: phrase index "
                            + " phrase index "
                            + phraseIndex.ToString()
                            + " phrase run: "
                            + phraseRun.ToString()
                            + " phrase: "
                            + GetRunText(phraseRun),
                        true);
                    returnValue = false;
                }

                lastStart = phraseRun.Start;
            }

            return returnValue;
        }

        public string GetPhrasesAndwordRunsDumpString()
        {
            int count = TextLength;
            int index;
            string text = Text;
            StringBuilder sb = new StringBuilder();

            for (index = 0; index < count; index++)
            {
                char chr = text[index];
                string phraseStarts = String.Empty;
                string phraseStops = String.Empty;
                string wordStarts = String.Empty;
                string wordStops = String.Empty;

                if (_PhraseRuns != null)
                {
                    for (int phraseIndex = 0; phraseIndex < _PhraseRuns.Count(); phraseIndex++)
                    {
                        TextRun phraseRun = _PhraseRuns[phraseIndex];

                        if (index == phraseRun.Start)
                            phraseStarts += "{";

                        if (index == phraseRun.Stop)
                            phraseStops += "}";
                    }
                }

                if (_WordRuns != null)
                {
                    for (int wordIndex = 0; wordIndex < _WordRuns.Count(); wordIndex++)
                    {
                        TextRun wordRun = _WordRuns[wordIndex];

                        if (index == wordRun.Start)
                            wordStarts += "[";

                        if (index == wordRun.Stop)
                            wordStops += "]";
                    }
                }

                sb.Append(phraseStarts);
                sb.Append(wordStarts);
                sb.Append(wordStops);
                sb.Append(phraseStops);
                sb.Append(chr);
            }

            return sb.ToString();
        }

        public string GetPhrasesAndwordRunsWithIndexesDumpString()
        {
            int count = TextLength;
            int index;
            string text = Text;
            StringBuilder sb = new StringBuilder();

            for (index = 0; index < count; index++)
            {
                char chr = text[index];
                string phraseStarts = String.Empty;
                string phraseStops = String.Empty;
                string wordStarts = String.Empty;
                string wordStops = String.Empty;

                if (_PhraseRuns != null)
                {
                    for (int phraseIndex = 0; phraseIndex < _PhraseRuns.Count(); phraseIndex++)
                    {
                        TextRun phraseRun = _PhraseRuns[phraseIndex];

                        if (index == phraseRun.Start)
                            phraseStarts += "(" + phraseIndex.ToString() + "," + index.ToString() + "){";

                        if (index == phraseRun.Stop)
                            phraseStops += "}" + "(" + phraseIndex.ToString() + "," + index.ToString() + ")";
                    }
                }

                if (_WordRuns != null)
                {
                    for (int wordIndex = 0; wordIndex < _WordRuns.Count(); wordIndex++)
                    {
                        TextRun wordRun = _WordRuns[wordIndex];

                        if (index == wordRun.Start)
                            wordStarts += "(" + wordIndex.ToString() + "," + index.ToString() + ")[";

                        if (index == wordRun.Stop)
                            wordStops += "]" + "(" + wordIndex.ToString() + "," + index.ToString() + ")";
                    }
                }

                sb.Append(phraseStarts);
                sb.Append(wordStarts);
                sb.Append(wordStops);
                sb.Append(phraseStops);
                sb.Append(chr);
            }

            return sb.ToString();
        }

        public void PutPhraseAndWordRunStartErrorMessage(
            string label,
            int phraseRunIndex,
            TextRun phraseRun,
            bool isStop)
        {
            string message =
                label
                + ": Phrase run index "
                + phraseRunIndex.ToString()
                + " (" + phraseRun.Start.ToString() + "," + phraseRun.Stop.ToString() + ")"
                + " has a "
                + (isStop ? "stop" : "start")
                + " that doesn't align with a word run\n"
                + "Dump:\n"
                + GetPhrasesAndwordRunsWithIndexesDumpString();
            ApplicationData.Global.PutConsoleErrorMessage(message);
            //throw new Exception(message);
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_SentenceRuns != null)
                {
                    foreach (TextRun textRun in _SentenceRuns)
                    {
                        if (textRun.Modified)
                            return true;
                    }
                }

                if (_WordRuns != null)
                {
                    foreach (TextRun textRun in _WordRuns)
                    {
                        if (textRun.Modified)
                            return true;
                    }
                }

                if (_PhraseRuns != null)
                {
                    foreach (TextRun textRun in _PhraseRuns)
                    {
                        if (textRun.Modified)
                            return true;
                    }
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_SentenceRuns != null)
                {
                    foreach (TextRun textRun in _SentenceRuns)
                        textRun.Modified = false;
                }

                if (_WordRuns != null)
                {
                    foreach (TextRun textRun in _WordRuns)
                        textRun.Modified = false;
                }

                if (_PhraseRuns != null)
                {
                    foreach (TextRun textRun in _PhraseRuns)
                        textRun.Modified = false;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElementTextElement(name);

            if (_SentenceRuns != null)
            {
                foreach (TextRun sentenceRun in _SentenceRuns)
                    element.Add(sentenceRun.GetElement("SentenceRun"));
            }

            if (_WordRuns != null)
            {
                foreach (TextRun wordRun in _WordRuns)
                    element.Add(wordRun.GetElement("WordRun"));
            }

            if (_PhraseRuns != null)
            {
                foreach (TextRun phraseRun in _PhraseRuns)
                    element.Add(phraseRun.GetElement("PhraseRun"));
            }

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "SentenceRun":
                    if (_SentenceRuns == null)
                        _SentenceRuns = new List<TextRun>();
                    _SentenceRuns.Add(new TextRun(childElement));
                    break;
                case "WordRun":
                    if (_WordRuns == null)
                        _WordRuns = new List<TextRun>();
                    _WordRuns.Add(new TextRun(childElement));
                    break;
                case "PhraseRun":
                    if (_PhraseRuns == null)
                        _PhraseRuns = new List<TextRun>();
                    _PhraseRuns.Add(new TextRun(childElement));
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int GetHashCode()
        {
            return _Key.GetHashCode();
        }

        /*
        public override bool Equals(object obj)
        {
            return this.Equals(obj as LanguageItem);
        }

        public virtual bool Equals(IBaseObjectKeyed obj)
        {
            return this.Equals(obj as LanguageItem);
        }

        public override bool Equals(BaseString obj)
        {
            return this.Equals(obj as LanguageItem);
        }

        public override bool Equals(LanguageString obj)
        {
            return this.Equals(obj as LanguageItem);
        }

        public virtual bool Equals(LanguageItem obj)
        {
            return Compare(obj) == 0 ? true : false;
        }

        public static bool operator ==(LanguageItem other1, LanguageItem other2)
        {
            if (((object)other1 == null) && ((object)other2 == null))
                return true;
            if (((object)other1 == null) || ((object)other2 == null))
                return false;
            return (other1.Compare(other2) == 0 ? true : false);
        }

        public static bool operator !=(LanguageItem other1, LanguageItem other2)
        {
            if (((object)other1 == null) && ((object)other2 == null))
                return false;
            if (((object)other1 == null) || ((object)other2 == null))
                return true;
            return (other1.Compare(other2) == 0 ? false : true);
        }
        */

        public override int Compare(IBaseObjectKeyed other)
        {
            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            LanguageItem otherLanguageItem = other as LanguageItem;

            if ((otherLanguageItem == null) || (otherLanguageItem.SentenceRuns == null))
            {
                if (_SentenceRuns != null)
                    return 1;
            }
            else if (_SentenceRuns == null)
                return -1;
            else
            {
                int count1 = _SentenceRuns.Count();
                int count2 = otherLanguageItem.SentenceRuns.Count();
                int count = (count1 < count2 ? count1 : count2);

                for (int index = 0; index < count; index++)
                {
                    diff = TextRun.Compare(_SentenceRuns[index], otherLanguageItem.SentenceRuns[index]);

                    if (diff != 0)
                        return diff;
                }

                if (count1 != count2)
                    return count1 - count2;
            }

            if ((otherLanguageItem == null) || (otherLanguageItem.WordRuns == null))
            {
                if (_WordRuns != null)
                    return 1;
            }
            else if (_WordRuns == null)
                return -1;
            else
            {
                int count1 = _WordRuns.Count();
                int count2 = otherLanguageItem.WordRuns.Count();
                int count = (count1 < count2 ? count1 : count2);

                for (int index = 0; index < count; index++)
                {
                    diff = TextRun.Compare(_WordRuns[index], otherLanguageItem.WordRuns[index]);

                    if (diff != 0)
                        return diff;
                }

                if (count1 != count2)
                    return count1 - count2;
            }

            if ((otherLanguageItem == null) || (otherLanguageItem.PhraseRuns == null))
            {
                if (_PhraseRuns != null)
                    return 1;
            }
            else if (_PhraseRuns == null)
                return -1;
            else
            {
                int count1 = _PhraseRuns.Count();
                int count2 = otherLanguageItem.PhraseRuns.Count();
                int count = (count1 < count2 ? count1 : count2);

                for (int index = 0; index < count; index++)
                {
                    diff = TextRun.Compare(_PhraseRuns[index], otherLanguageItem.PhraseRuns[index]);

                    if (diff != 0)
                        return diff;
                }

                if (count1 != count2)
                    return count1 - count2;
            }

            return 0;
        }

        public static int Compare(LanguageItem item1, LanguageItem item2)
        {
            if (((object)item1 == null) && ((object)item2 == null))
                return 0;
            if ((object)item1 == null)
                return -1;
            if ((object)item2 == null)
                return 1;
            return item1.Compare(item2);
        }

        public static int CompareLanguageItemLists(List<LanguageItem> list1, List<LanguageItem> list2)
        {
            return ObjectUtilities.CompareTypedObjectLists<LanguageItem>(list1, list2);
        }
    }
}
