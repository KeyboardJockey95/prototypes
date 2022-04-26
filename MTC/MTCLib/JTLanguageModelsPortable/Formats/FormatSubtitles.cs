using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Language;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatSubtitles : Format
    {
        // Parameters.
        public string SubFormat { get; set; }
        public static List<string> SubFormats = new List<string>() { "SRT", "VTT" };
        public List<LanguageID> SubtitleLanguageIDs { get; set; }
        public string MediaItemKey { get; set; }
        public string LanguageMediaItemKey { get; set; }
        public string MediaRunKey { get; set; }
        public float SubtitleTimeOffset { get; set; }
        public string TargetOwner { get; set; }
        public string Package { get; set; }
        public bool DoSentenceFixes { get; set; }
        public string SentenceFixesKey { get; set; }
        public bool DoWordFixes { get; set; }
        public string WordFixesKey { get; set; }
        public int VoiceSpeed { get; set; }
        public bool ExtractSpeakerNames { get; set; }
        public bool AddTranslationSuffix { get; set; }
        public bool FilterBracketed { get; set; }

        // Implementation.
        protected ContentStudyList StudyList { get; set; }
        protected SentenceFixes SentenceFixes { get; set; }
        protected List<MultiLanguageItem> StudyItems { get; set; }
        protected List<string> Names { get; set; }
        protected int EntryCounter;
        protected MediaRun LastMediaRun;

        private static string FormatDescription = "Format for importing/exporting .srt subtitle files as study items.";

        public FormatSubtitles()
            : base(
                  "Subtitles",
                  "FormatSubtitles",
                  FormatDescription,
                  String.Empty,
                  String.Empty,
                  "text/plain",
                  ".srt",
                  null,
                  null,
                  null,
                  null,
                  null)
        {
            ClearFormatSubtitles();
        }

        public FormatSubtitles(FormatSubtitles other)
            : base(other)
        {
            CopyFormatSubtitles(other);
        }

        public FormatSubtitles(
            string name,
            string type,
            string description,
            string targetType,
            string importExportType,
            string mimeType,
            string defaultExtension,
            UserRecord userRecord,
            UserProfile userProfile,
            IMainRepository repositories,
            LanguageUtilities languageUtilities,
            NodeUtilities nodeUtilities)
            : base(name, type, description, targetType, importExportType, mimeType, defaultExtension,
                  userRecord, userProfile, repositories, languageUtilities, nodeUtilities)
        {
            ClearFormatSubtitles();
        }

        public void ClearFormatSubtitles()
        {
            // Local parameters.

            SubFormat = "SRT";
            SubtitleLanguageIDs = null;
            MediaItemKey = "VideoLesson";
            LanguageMediaItemKey = "";
            MediaRunKey = "Video";
            SubtitleTimeOffset = 0.0f;
            Package = String.Empty;
            DoSentenceFixes = false;
            SentenceFixesKey = null;
            DoWordFixes = false;
            WordFixesKey = null;
            VoiceSpeed = 0;
            ExtractSpeakerNames = false;
            AddTranslationSuffix = false;
            FilterBracketed = true;

            // Base parameters.

            DefaultContentType = "Text";
            DefaultContentSubType = "Text";
            Label = "Text";

            // Implementation.

            StudyList = null;
            SentenceFixes = null;
            StudyItems = null;
            Names = null;
            EntryCounter = 0;
            LastMediaRun = null;
        }

        public void CopyFormatSubtitles(FormatSubtitles other)
        {
            ClearFormatSubtitles();

            SubFormat = other.SubFormat;

            if (other.SubtitleLanguageIDs != null)
                SubtitleLanguageIDs = new List<LanguageID>(SubtitleLanguageIDs);
            else
                SubtitleLanguageIDs = null;

            MediaItemKey = other.MediaItemKey;
            LanguageMediaItemKey = other.LanguageMediaItemKey;
            MediaRunKey = other.MediaRunKey;
            SubtitleTimeOffset = other.SubtitleTimeOffset;
            ExtractSpeakerNames = other.ExtractSpeakerNames;
            AddTranslationSuffix = other.AddTranslationSuffix;
            FilterBracketed = other.FilterBracketed;
        }

        public override Format Clone()
        {
            return new FormatSubtitles(this);
        }

        protected void InitializeTools()
        {
            foreach (LanguageID languageID in SubtitleLanguageIDs)
                InitializeTool(languageID);
        }

        protected void InitializeTool(LanguageID languageID)
        {
            BaseObjectContent content = StudyList.Content;
            BaseObjectNode node = content.NodeOrTree;
            BaseObjectNodeTree tree = content.Tree;
            string treeName = tree.GetTitleString(UILanguageID);
            treeName = MediaUtilities.FileFriendlyName(treeName);

            SentenceFixesKey = treeName;
            WordFixesKey = treeName;

            // Prime tool cache.
            LanguageTool tool = NodeUtilities.GetLanguageTool(languageID);

            if (DoSentenceFixes)
            {
                if (SentenceFixes == null)
                {
                    SentenceFixes sentenceFixes;
                    string filePath = SentenceFixes.GetFilePath(treeName, null);

                    if (SentenceFixes.CreateAndLoad(filePath, out sentenceFixes))
                        SentenceFixes = sentenceFixes;

                    if (!tree.HasOption(SentenceFixesKey))
                        tree.AddOptionString("SentenceFixesKey", SentenceFixesKey);
                }
            }
            else
                SentenceFixes = null;

            if (tool != null)
            {
                if (DoWordFixes)
                {
                    tool.InitializeWordFixes(WordFixesKey);

                    if (!tree.HasOption(WordFixesKey))
                        tree.AddOptionString("WordFixesKey", WordFixesKey);
                }

                tool.SentenceFixes = SentenceFixes;
            }
        }

        public override void Read(Stream stream)
        {
            List<LanguageID> languageIDs = UniqueLanguageIDs;
            bool isOK = true;

            if ((SubtitleLanguageIDs == null) || (SubtitleLanguageIDs.Count() == 0))
            {
                PutError("You need to select the subtitle language(s).");
                return;
            }

            if (DeleteBeforeImport)
                DeleteFirst();

            Component = StudyList = GetTargetStudyList();

            if (StudyList != null)
            {
                InitializeTools();

                try
                {
                    UpdateProgressElapsed("Reading stream ...");

                    EntryCounter = 0;
                    LastMediaRun = null;

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        isOK = ParseSubtitles(reader);
                    }

                    if (ExtractSpeakerNames && isOK && (StudyItems != null) && (Names != null))
                    {
                        LanguageID languageID = SubtitleLanguageIDs.First();
                        List<MultiLanguageString> speakerNames = StudyList.SpeakerNames;
                        int speakerIndex = 0;

                        foreach (string name in Names)
                        {
                            MultiLanguageString speakerName = null;

                            if (speakerNames != null)
                            {
                                if (speakerIndex < speakerNames.Count())
                                    speakerName = speakerNames[speakerIndex];
                            }

                            if (speakerName == null)
                            {
                                speakerName = new MultiLanguageString(name, languageIDs);
                                speakerName.SetText(languageID, name);

                                if (speakerNames == null)
                                {
                                    speakerNames = new List<MultiLanguageString>() { speakerName };
                                    StudyList.SpeakerNames = speakerNames;
                                }
                                else
                                    speakerNames.Add(speakerName);
                            }
                            else if (speakerNames == null)
                            {
                                speakerName.SetText(languageID, name);
                                speakerNames = new List<MultiLanguageString>() { speakerName };
                                StudyList.SpeakerNames = speakerNames;
                            }

                            if (IsTranslateMissingItems)
                            {
                                string errorMessage;

                                LanguageUtilities.Translator.TranslateMultiLanguageString(
                                    speakerName,
                                    languageIDs,
                                    out errorMessage,
                                    false);
                            }

                            speakerIndex++;
                        }
                    }

                    if (IsTranslateMissingItems && isOK && (StudyItems != null))
                    {
                        UpdateProgressElapsed("Translating ...");

                        foreach (MultiLanguageItem studyItem in StudyItems)
                        {
                            string errorMessage;

                            if (!LanguageUtilities.Translator.TranslateMultiLanguageItem(
                                    studyItem,
                                    languageIDs,
                                    false,
                                    false,
                                    out errorMessage,
                                    false))
                            {
                                PutError(errorMessage);
                                isOK = false;
                            }
                        }
                    }

                    if (AddTranslationSuffix && isOK && (StudyItems != null))
                    {
                        LanguageID targetLanguageID = UserProfile.TargetLanguageID;
                        LanguageID hostLanguageID = UserProfile.HostLanguageID;

                        UpdateProgressElapsed("Adding translation suffices ...");

                        foreach (MultiLanguageItem studyItem in StudyItems)
                        {
                            LanguageItem targetLanguageItem = studyItem.LanguageItem(targetLanguageID);
                            LanguageItem hostLanguageItem = studyItem.LanguageItem(hostLanguageID);
                            string hostText = hostLanguageItem.Text;

                            if ((targetLanguageItem == null) || (hostLanguageItem == null))
                                continue;

                            int sentenceCount = targetLanguageItem.SentenceRunCount();
                            int sentenceIndex;

                            while (hostLanguageItem.SentenceRunCount() < sentenceCount)
                            {
                                TextRun sentenceRun = new TextRun(
                                    hostLanguageItem.TextLength,
                                    0,
                                    null);
                                hostLanguageItem.AddSentenceRun(sentenceRun);
                            }

                            int hostSentenceCount = hostLanguageItem.SentenceRunCount();

                            for (sentenceIndex = sentenceCount - 1; sentenceIndex >= 0; sentenceIndex--)
                            {
                                TextRun targetSentenceRun = targetLanguageItem.GetSentenceRun(sentenceIndex);
                                TextRun hostSentenceRun = hostLanguageItem.GetSentenceRun(sentenceIndex);

                                string targetSentence = targetLanguageItem.GetRunText(targetSentenceRun);
                                string previousHostSentence = hostLanguageItem.GetRunText(hostSentenceRun);
                                string hostSentence;
                                LanguageTranslatorSource translatorSource;
                                string errorMessage;

                                if (String.IsNullOrEmpty(targetSentence))
                                    continue;

                                if (LanguageUtilities.Translator.TranslateString(
                                    "ContentTranslate",
                                    "",
                                    "",
                                    targetSentence,
                                    targetLanguageID,
                                    hostLanguageID,
                                    out hostSentence,
                                    out translatorSource,
                                    out errorMessage))
                                {
                                    if (hostSentence != previousHostSentence)
                                    {
                                        string insertText = " (" + hostSentence + ")";
                                        int insertIndex = hostSentenceRun.Stop;
                                        int insertLength = insertText.Length;

                                        hostText = hostText.Insert(insertIndex, insertText);
                                        hostLanguageItem.Text = hostText;
                                        hostSentenceRun.Length = hostSentenceRun.Length + insertLength;

                                        for (int index = sentenceIndex + 1; index < hostSentenceCount; index++)
                                        {
                                            TextRun afterRun = hostLanguageItem.GetSentenceRun(index);
                                            afterRun.Start = afterRun.Start + insertLength;
                                        }
                                    }
                                }
                            }

                            FixupLanguageItem(hostLanguageItem);
                        }
                    }

                    if (IsSynthesizeMissingAudio && isOK && (StudyItems != null))
                    {
                        BaseObjectContent content = StudyList.Content;
                        BaseObjectNode node = content.NodeOrTree;
                        BaseObjectNodeTree tree = content.Tree;

                        UpdateProgressElapsed("Generating speech ...");

                        foreach (LanguageID languageID in SubtitleLanguageIDs)
                        {
                            List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(
                                languageID);

                            if (!NodeUtilities.AddSynthesizedVoiceToStudyItemsListLanguage(
                                    tree,
                                    node,
                                    content,
                                    StudyItems,
                                    languageID,
                                    alternateLanguageIDs,
                                    "(default)",
                                    VoiceSpeed,
                                    IsForceAudio,
                                    FilterBracketed))
                                isOK = false;
                        }
                    }

                    if (SubDivide)
                    {
                        UpdateProgressElapsed("Subdividing study lists ...");
                        SubDivideStudyList(StudyList);
                    }
                }
                catch (Exception exc)
                {
                    string msg = exc.Message;

                    if (exc.InnerException != null)
                        msg += ": " + exc.InnerException.Message;

                    PutError(msg);
                }
                finally
                {
                    NodeUtilities.UpdateTreeCheck(StudyList.Tree, false, false);
                }
            }
        }

        protected bool ParseSubtitles(StreamReader reader)
        {
            bool returnValue;

            switch (SubFormat)
            {
                case "SRT":
                    returnValue = ParseSRT(reader);
                    break;
                case "VTT":
                    returnValue = ParseVTT(reader);
                    break;
                default:
                    PutError("Subtitle subformat not handled: " + SubFormat);
                    returnValue = false;
                    break;
            }

            return returnValue;
        }

        protected bool ParseSRT(StreamReader reader)
        {
            int entryNumber;
            TimeSpan startTime;
            TimeSpan endTime;
            List<string> text;
            bool returnValue = false;

            // Load entries.
            while (ReadEntrySRT(
                reader,
                out entryNumber,
                out startTime,
                out endTime,
                out text))
            {
                if (FilterBracketed)
                {
                    int c = text.Count();

                    for (int i = c - 1; i >= 0; i--)
                    {
                        string t = text[i];

                        if (t.StartsWith("[") || t.StartsWith("]"))
                            text.RemoveAt(i);
                    }
                }

                if (text.Count() == 0)
                    continue;

                if (!ProcessEntry(entryNumber, startTime, endTime, text))
                {
                    returnValue = false;
                    break;
                }
            }

            return returnValue;
        }

        private enum SRTState { EntryNumber, TimeTag, Text }

        protected bool ReadEntrySRT(
            StreamReader reader,
            out int entryNumber,
            out TimeSpan startTime,
            out TimeSpan endTime,
            out List<string> text)
        {
            SRTState state = SRTState.EntryNumber;
            string line;
            bool returnValue = false;

            entryNumber = 0;
            startTime = TimeSpan.Zero;
            endTime = TimeSpan.Zero;
            text = null;

            for (;;)
            {
                line = reader.ReadLine();

                if (line == null)
                    return false;

                line = line.Trim();

                switch (state)
                {
                    case SRTState.EntryNumber:
                        entryNumber = ObjectUtilities.GetIntegerFromString(line, 0);
                        if (entryNumber > 0)
                            state = SRTState.TimeTag;
                        break;
                    case SRTState.TimeTag:
                        if (ParseTimeTag(line, out startTime, out endTime))
                            state = SRTState.Text;
                        else
                            state = SRTState.EntryNumber;
                        text = new List<string>();
                        break;
                    case SRTState.Text:
                        if (String.IsNullOrEmpty(line))
                        {
                            returnValue = true;
                            return returnValue;
                        }
                        else
                            text.Add(line);
                        break;
                }
            }
        }

        protected bool ParseTimeTag(
            string line,
            out TimeSpan startTime,
            out TimeSpan endTime)
        {
            bool returnValue = false;

            int ofs = line.IndexOf(" --> ");

            if (ofs == -1)
                return false;

            string startField = line.Substring(0, ofs);
            string endField = line.Substring(ofs + 5);

            ofs = endField.IndexOf(' ');

            if (ofs != -1)
                endField = endField.Substring(0, ofs);

            if (ParseTimeField(startField, out startTime))
            {
                if (ParseTimeField(endField, out endTime))
                    returnValue = true;
            }
            
            return returnValue;
        }

        private static char[] TimeFieldSeps = { ':', ',', '.' };

        protected bool ParseTimeField(
            string text,
            out TimeSpan time)
        {
            int hours;
            int minutes;
            int seconds;
            int milliseconds;
            string[] parts = text.Split(TimeFieldSeps);

            if (parts.Length == 4)
            {
                hours = ObjectUtilities.GetIntegerFromString(parts[0], 0);
                minutes = ObjectUtilities.GetIntegerFromString(parts[1], 0);
                seconds = ObjectUtilities.GetIntegerFromString(parts[2], 0);
                milliseconds = ObjectUtilities.GetIntegerFromString(parts[3], 0);
                time = new TimeSpan(0, hours, minutes, seconds, milliseconds);
                return true;
            }

            return false;
        }

        protected bool ParseVTT(StreamReader reader)
        {
            string line;
            int lineIndex;
            int lineCount;
            List<string> lines = new List<string>();
            FormatSubtitleItem item = null;
            List<FormatSubtitleItem> items = new List<FormatSubtitleItem>();
            int entryNumber = 0;
            TimeSpan startTime;
            TimeSpan endTime;
            bool returnValue = true;

            // Load entries.
            while ((line = reader.ReadLine()) != null)
                lines.Add(line.Trim());

            lineCount = lines.Count();

            for (lineIndex = 0; lineIndex < lineCount; lineIndex++)
            {
                line = lines[lineIndex];

                if (ParseTimeTag(line, out startTime, out endTime))
                {
                    if (item != null)
                        item.DeleteOverlappingLines();

                    item = new FormatSubtitleItem();
                    item.EntryNumber = ++entryNumber;
                    item.StartTime = startTime;
                    item.EndTime = endTime;
                    items.Add(item);
                }
                else if (item != null)
                {
                    if (!String.IsNullOrEmpty(line))
                    {
                        string text = TextUtilities.StripHtml(line);

                        if (FilterBracketed)
                            text = TextUtilities.StripMatchedChars(text, '[', ']');

                        if (!String.IsNullOrEmpty(text))
                            item.AddLine(text);
                    }
                }
            }

            if (item != null)
                item.DeleteOverlappingLines();

            for (int itemIndex = items.Count() - 1; itemIndex > 0; itemIndex--)
            {
                FormatSubtitleItem item1 = items[itemIndex - 1];
                FormatSubtitleItem item2 = items[itemIndex];

                if (item1.LastLine == item2.FirstLine)
                {
                    if (item2.LineCount > 1)
                        item2.DeleteFirstLine();
                    else
                    {
                        item1.EndTime = item2.EndTime;
                        items.RemoveAt(itemIndex);
                    }
                }
            }

            foreach (FormatSubtitleItem vttItem in items)
            {
                if (!ProcessEntry(
                    vttItem.EntryNumber,
                    vttItem.StartTime,
                    vttItem.EndTime,
                    vttItem.Text))
                {
                    returnValue = false;
                    break;
                }
            }

            return returnValue;
        }

        protected bool IsTimeTagLineVTT(string line)
        {
            if (line.Contains(" --> "))
                return true;

            return false;
        }

        protected bool ReadEntryVTT(
            StreamReader reader,
            out int entryNumber,
            out TimeSpan startTime,
            out TimeSpan endTime,
            out List<string> text)
        {
            string timeLine;
            string textLine;

            entryNumber = ++EntryCounter;
            startTime = TimeSpan.Zero;
            endTime = TimeSpan.Zero;
            text = null;

            timeLine = reader.ReadLine();

            if (timeLine == null)
                return false;

            timeLine = timeLine.Trim();

            if (timeLine.Length == 5)
                timeLine = "00:" + timeLine;

            if (!TimeSpan.TryParse(timeLine, out startTime))
            {
                PutErrorArgument("Error parsing time", timeLine);
                return false;
            }

            endTime = startTime;

            textLine = reader.ReadLine();

            if (textLine == null)
                return false;

            textLine = textLine.Trim();

            text = new List<string>(1) { textLine };

            return true;
        }

        protected string JoinLines(string line1, string line2)
        {
            if (String.IsNullOrEmpty(line1))
                return line2;

            string text = line1;

            if (LanguageLookup.IsUseSpacesToSeparateWords(TargetLanguageID))
                text += " ";

            text += line2;

            return text;
        }

        protected bool ProcessEntry(
            int entryNumber,
            TimeSpan startTime,
            TimeSpan endTime,
            List<string> text)
        {
            BaseObjectContent content = StudyList.Content;
            MultiLanguageItem studyItem;
            int insertionIndex = -1;
            bool returnValue = true;

            if (SubtitleTimeOffset != 0.0f)
            {
                long ticks = (long)(SubtitleTimeOffset * TimeSpan.TicksPerSecond);

                if (ticks >= 0L)
                {
                    TimeSpan offset = new TimeSpan(ticks);
                    startTime = startTime.Add(offset);
                    endTime = endTime.Add(offset);
                }
                else
                {
                    TimeSpan offset = new TimeSpan(-ticks);
                    startTime = startTime.Subtract(offset);
                    endTime = endTime.Subtract(offset);
                }
            }

            if (IsDoMerge)
            {
                studyItem = FindOverlappingStudyItem(startTime, endTime, out insertionIndex);

                if (studyItem != null)
                    OverwriteText(studyItem, text, startTime, endTime);
                else
                {
                    string studyItemKey = StudyList.AllocateStudyItemKey();
                    studyItem = new MultiLanguageItem(studyItemKey, StudyList.Content.LanguageIDs);
                    OverwriteText(studyItem, text, startTime, endTime);

                    if (insertionIndex != -1)
                        StudyList.InsertStudyItemIndexed(insertionIndex, studyItem);
                    else
                        StudyList.AddStudyItem(studyItem);

                    ItemIndex = ItemIndex + 1;
                }
            }
            else
            {
                string studyItemKey = StudyList.AllocateStudyItemKey();
                studyItem = new MultiLanguageItem(studyItemKey, StudyList.Content.LanguageIDs);
                OverwriteText(studyItem, text, startTime, endTime);
                StudyList.AddStudyItem(studyItem);
                ItemIndex = ItemIndex + 1;
            }

            if (StudyItems == null)
                StudyItems = new List<MultiLanguageItem>() { studyItem };
            else
                StudyItems.Add(studyItem);

            return returnValue;
        }

        protected MultiLanguageItem FindOverlappingStudyItem(
            TimeSpan startTime,
            TimeSpan endTime,
            out int insertionIndex)
        {
            MultiLanguageItem bestStudyItem = null;
            TimeSpan bestIntersection = TimeSpan.Zero;
            int studyItemIndex = 0;

            insertionIndex = -1;

            if (StudyList.StudyItemCount() == 0)
                return null;

            foreach (MultiLanguageItem studyItem in StudyList.StudyItems)
            {
                foreach (LanguageItem languageItem in studyItem.LanguageItems)
                {
                    if (languageItem.HasText() &&
                        languageItem.HasSentenceRuns() &&
                        !SubtitleLanguageIDs.Contains(languageItem.LanguageID))
                    {
                        TimeSpan mergedStart;
                        TimeSpan mergedEnd;

                        if (languageItem.GetMediaRunTimeUnion(MediaRunKey, out mergedStart, out mergedEnd))
                        {
                            TimeSpan intersection;

                            if (GetIntersection(
                                startTime,
                                endTime,
                                mergedStart,
                                mergedEnd,
                                out intersection))
                            {
                                if (bestStudyItem == null)
                                {
                                    bool isOK = true;

                                    foreach (LanguageID languageID in SubtitleLanguageIDs)
                                    {
                                        if (studyItem.HasText(languageID))
                                        {
                                            isOK = false;
                                            break;
                                        }
                                    }

                                    if (isOK)
                                    {
                                        bestIntersection = intersection;
                                        bestStudyItem = studyItem;
                                    }
                                }
                            }

                            if (insertionIndex == -1)
                            {
                                if (mergedStart >= endTime)
                                    insertionIndex = studyItemIndex;
                            }
                        }
                    }
                }

                if (insertionIndex != -1)
                    break;

                studyItemIndex++;
            }

            return bestStudyItem;
        }

        protected bool GetIntersection(
            TimeSpan start1,
            TimeSpan end1,
            TimeSpan start2,
            TimeSpan end2,
            out TimeSpan intersection)
        {
            intersection = TimeSpan.Zero;

            if (end2 < start1)
                return false;

            if (start1 > end2)
                return false;

            TimeSpan commonStart;
            TimeSpan commonStop;

            if (start2 < start1)
                commonStart = start1;
            else
                commonStart = start2;

            if (end2 < end1)
                commonStop = end2;
            else
                commonStop = end1;

            intersection = commonStop - commonStart;

            return true;
        }

        protected static TimeSpan Gap = new TimeSpan(0, 0, 0, 0, 10);

        protected void OverwriteText(
            MultiLanguageItem studyItem,
            List<string> text,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            TimeSpan lengthTime = endTime - startTime;

            if (text.Count() == 0)
                return;

            if (String.IsNullOrEmpty(LanguageMediaItemKey))
                LanguageMediaItemKey = TargetLanguageID.LanguageCode;

            if (SubtitleLanguageIDs.Count() == 1)
            {
                LanguageID languageID = SubtitleLanguageIDs.First();
                LanguageItem languageItem = studyItem.LanguageItem(languageID);

                if (languageItem != null)
                {
                    if (languageItem.SentenceRunCount() != 0)
                        languageItem.DeleteSentenceRuns();

                    string paragraph = String.Empty;
                    int sentenceCount = text.Count();
                    int sentenceIndex = 0;
                    TextRun sentenceRun;
                    List<MediaRun> mediaRuns;
                    MediaRun mediaRun;
                    long startingTicks = startTime.Ticks;
                    long endingTicks = endTime.Ticks;
                    long totalTicks = endingTicks - startingTicks;
                    long sentenceTicks = totalTicks / sentenceCount;
                    TimeSpan lengthSpan = new TimeSpan(sentenceTicks);

                    foreach (string str in text)
                    {
                        if (sentenceIndex != 0)
                            paragraph += " ";

                        string sentenceText = ProcessSentenceText(
                            studyItem,
                            str,
                            languageID);
                        long startTicks = startingTicks + ((totalTicks * sentenceIndex) / sentenceCount);
                        TimeSpan localStart = new TimeSpan(startTicks);
                        mediaRun = new MediaRun(MediaRunKey, null, MediaItemKey, LanguageMediaItemKey, Owner, localStart, lengthSpan);
                        mediaRuns = new List<MediaRun>(1) { mediaRun };
                        sentenceRun = new TextRun(
                            paragraph.Length,
                            sentenceText.Length,
                            mediaRuns);
                        languageItem.AddSentenceRun(sentenceRun);

                        if ((LastMediaRun != null) && (LastMediaRun.Length == TimeSpan.Zero))
                        {
                            LastMediaRun.Length = localStart - LastMediaRun.Start;

                            if (LastMediaRun.Length > Gap)
                                LastMediaRun.Length = LastMediaRun.Length - Gap;
                        }

                        LastMediaRun = mediaRun;

                        paragraph += sentenceText;
                        sentenceIndex++;
                    }

                    languageItem.Text = paragraph;
                    FixupLanguageItem(languageItem);
                }
            }
            else
            {
                int index;
                int count = text.Count;
                int languageCount = SubtitleLanguageIDs.Count();

                foreach (LanguageID languageID in SubtitleLanguageIDs)
                {
                    LanguageItem languageItem = studyItem.LanguageItem(languageID);
                    languageItem.SentenceRuns = null;
                    languageItem.Text = String.Empty;
                }

                for (index = 0; index < count; index++)
                {
                    int languageIndex = (index + 1) / languageCount;
                    LanguageItem languageItem = studyItem.LanguageItem(SubtitleLanguageIDs[languageIndex]);
                    LanguageID languageID = languageItem.LanguageID;
                    string str = text[index];
                    string sentenceText = ProcessSentenceText(
                        studyItem,
                        str,
                        languageID);

                    if (languageItem != null)
                    {
                        string paragraph = languageItem.Text;
                        int sentenceIndex = languageItem.SentenceRunCount();
                        TextRun sentenceRun;
                        List<MediaRun> mediaRuns;
                        MediaRun mediaRun;
                        mediaRun = new MediaRun(MediaRunKey, null, MediaItemKey, LanguageMediaItemKey, Owner, startTime, lengthTime);
                        mediaRuns = new List<MediaRun>(1) { mediaRun };
                        sentenceRun = new TextRun(
                            paragraph.Length,
                            sentenceText.Length,
                            mediaRuns);
                        languageItem.AddSentenceRun(sentenceRun);
                        languageItem.Text = sentenceText;
                        FixupLanguageItem(languageItem);
                    }
                }

                foreach (LanguageID languageID in SubtitleLanguageIDs)
                {
                    LanguageItem languageItem = studyItem.LanguageItem(languageID);

                    if (languageItem.HasSentenceRuns())
                    {
                        int sentenceCount = languageItem.SentenceRunCount();
                        int sentenceIndex = 0;
                        MediaRun mediaRun;

                        foreach (TextRun sentenceRun in languageItem.SentenceRuns)
                        {
                            mediaRun = sentenceRun.GetMediaRun(MediaRunKey);

                            if (mediaRun != null)
                            {
                                long startingTicks = mediaRun.Start.Ticks;
                                long endingTicks = mediaRun.Stop.Ticks;
                                long totalTicks = endingTicks - startingTicks;
                                long sentenceTicks = totalTicks / sentenceCount;
                                TimeSpan lengthSpan = new TimeSpan(sentenceTicks);
                                long startTicks = startingTicks + ((totalTicks * sentenceIndex) / sentenceCount);
                                TimeSpan localStart = new TimeSpan(startTicks);

                                mediaRun.Start = localStart;
                                mediaRun.Length = lengthSpan;
                            }
                        }
                    }
                }
            }
        }

        protected string ProcessSentenceText(
            MultiLanguageItem studyItem,
            string text,
            LanguageID languageID)
        {
            string output = TextUtilities.StripHtml(text);

            output = output.Replace(LanguageLookup.UTF8SignatureString, "");

            if (ExtractSpeakerNames)
            {
                string input = output;
                List<string> nameStrings;

                if (ExtractParenthesizedText(input, out output, out nameStrings))
                {
                    int speakerIndex = 0;

                    input = output;

                    foreach (string name in nameStrings)
                    {
                        if (Names == null)
                        {
                            Names = new List<string>() { name };
                            speakerIndex = 0;
                        }
                        else if (!Names.Contains(name))
                        {
                            speakerIndex = Names.Count();
                            Names.Add(name);
                        }
                        else
                            speakerIndex = Names.IndexOf(name);
                    }

                    studyItem.SpeakerNameKey = Names[speakerIndex];
                }
            }

            return output;
        }

        protected bool ExtractParenthesizedText(
            string input,
            out string output,
            out List<string> names)
        {
            int length = input.Length;
            int index = 0;
            StringBuilder sb = new StringBuilder();
            int level = 0;
            int start = 0;
            StringBuilder nb = new StringBuilder();
            bool returnValue = false;

            names = null;
            output = String.Empty;

            for (index = 0; index < length; index++)
            {
                char chr = input[index];

                if (LanguageLookup.ParenthesisOpenCharacters.Contains(chr))
                {
                    level++;

                    if (level == 1)
                        start = index + 1;
                }
                else if (LanguageLookup.ParenthesisCloseCharacters.Contains(chr))
                {
                    level--;

                    if (level == 0)
                    {
                        string name = nb.ToString();
                        nb.Clear();

                        if (names == null)
                            names = new List<string>() { name };
                        else
                            names.Add(name);

                        returnValue = true;
                    }
                }
                else
                {
                    if (level == 0)
                        sb.Append(chr);
                    else
                        nb.Append(chr);
                }
            }

            output = sb.ToString();

            return returnValue;
        }

        protected void FixupLanguageItem(LanguageItem languageItem)
        {
            LanguageID languageID = languageItem.LanguageID;
            LanguageTool tool = GetLanguageTool(languageID);

            if (tool != null)
            {
                if (tool.SentenceFixes != null)
                    tool.SentenceFixes.FixLanguageItemCheck(languageItem);

                tool.ResetLanguageItemWordRuns(languageItem);
            }
            else
            {
                if (SentenceFixes != null)
                    SentenceFixes.FixLanguageItemCheck(languageItem);

                languageItem.AutoResetWordRuns(Repositories.Dictionary);
            }
        }

        public static string SubFormatHelp = "Format of subtitles.";
        public static string SubtitleLanguageIDsHelp = "Subtitle languages.";
        public static string MediaItemKeyHelp = "Enter the media item key the text associates with.";
        public static string LanguageMediaItemKeyHelp = "Enter the language media item key the text associates with (i.e. \"ja\" for Japanese).";
        public static string MediaRunKeyHelp = "Select the media run key.";
        public static string SubtitleTimeOffsetHelp = "Offset the subtitles by this floating-point number of seconds.";
        public static string OwnerHelp = "Enter owner user ID.";
        public static string PackageHelp = "Enter package name. This will restrict access to users with the package name set in their account.";
        public static string DoSentenceFixesHelp = "Check this to do sentence fixups.";
        public static string SentenceFixesKeyHelp = "Enter sentence fixups key.";
        public static string DoWordFixesHelp = "Check this to do word fixups.";
        public static string WordFixesKeyHelp = "Enter word fixups key.";
        public static string VoiceSpeedHelp = "Specify synthesised voice speed. Interger value from -10 to 10.";
        public static string ExtractSpeakerNamesHelp = "Use parenthesized terms as speaker names, removing them from the text.";
        public static string AddTranslationSuffixHelp = "Add automatic host translation suffix.";
        public static string FilterBracketedHelp = "Filter out bracketed lines.";

        public override void LoadFromArguments()
        {
            base.LoadFromArguments();

            SubFormat = GetStringListArgumentDefaulted("SubFormat", "stringlist", "r", SubFormat, SubFormats, "SubFormat",
                SubFormatHelp);

            DeleteBeforeImport = GetFlagArgumentDefaulted("DeleteBeforeImport", "flag", "r", DeleteBeforeImport,
                "Delete before import", DeleteBeforeImportHelp, null, null);

            IsDoMerge = GetFlagArgumentDefaulted("IsDoMerge", "flag", "r", IsDoMerge, "Merge", IsDoMergeHelp,
                null, null);

            SubtitleLanguageIDs = GetLanguageIDListArgumentDefaulted(
                "SubtitleLanguageIDs",
                "languagelist",
                "r",
                SubtitleLanguageIDs,
                "Subtitle Languages",
                SubtitleLanguageIDsHelp);

            MediaItemKey = GetArgumentDefaulted("MediaItemKey", "string", "r", MediaItemKey, "MediaItemKey",
                MediaItemKeyHelp);

            LanguageMediaItemKey = GetArgumentDefaulted("LanguageMediaItemKey", "string", "r",
                LanguageMediaItemKey, "LanguageMediaItemKey",
                LanguageMediaItemKeyHelp);

            MediaRunKey = GetStringListArgumentDefaulted("MediaRunKey", "string", "r",
                MediaRunKey, MediaRun.MediaRunKeys, "MediaRunKey",
                MediaRunKeyHelp);

            SubtitleTimeOffset = GetFloatArgumentDefaulted(
                "SubtitleTimeOffset",
                "float",
                "r",
                SubtitleTimeOffset,
                "Subtitle Time Offset",
                SubtitleTimeOffsetHelp);

            ExtractSpeakerNames = GetFlagArgumentDefaulted("ExtractSpeakerNames", "flag", "r", ExtractSpeakerNames, "Extract speaker names", ExtractSpeakerNamesHelp,
                null, null);
            AddTranslationSuffix = GetFlagArgumentDefaulted("AddTranslationSuffix", "flag", "r", AddTranslationSuffix, "Add translation suffix", AddTranslationSuffixHelp,
                null, null);
            FilterBracketed = GetFlagArgumentDefaulted("FilterBracketed", "flag", "r", FilterBracketed, "Filter bracketed", FilterBracketedHelp,
                null, null);

            IsTranslateMissingItems = GetFlagArgumentDefaulted("IsTranslateMissingItems", "flag", "r", IsTranslateMissingItems, "Translate missing items",
                IsTranslateMissingItemsHelp, null, null);
            IsSynthesizeMissingAudio = GetFlagArgumentDefaulted("IsSynthesizeMissingAudio", "flag", "r", IsSynthesizeMissingAudio, "Synthesize missing audio",
                IsSynthesizeMissingAudioHelp, null, null);
            IsForceAudio = GetFlagArgumentDefaulted("IsForceAudio", "flag", "r", IsForceAudio, "Force synthesis of audio",
                IsForceAudioHelp, null, null);
            VoiceSpeed = GetIntegerArgumentDefaulted("VoiceSpeed", "integer", "r", 0,
                "Voice speed", VoiceSpeedHelp);

            switch (TargetType)
            {
                case "BaseObjectNode":
                case "BaseObjectNodeTree":
                    SubDivide = GetFlagArgumentDefaulted("SubDivide", "flag", "r", SubDivide,
                        "Subdivide items into smaller groups", SubDivideHelp,
                        new List<string>()
                        {
                            "SubDivideToStudyListsOnly",
                            "MasterName",
                            "StudyItemSubDivideCount",
                            "MinorSubDivideCount",
                            "MajorSubDivideCount",
                        },
                        null);
                    SubDivideToStudyListsOnly = GetFlagArgumentDefaulted("SubDivideToStudyListsOnly", "flag", "r",
                        SubDivideToStudyListsOnly, "Subdivide to study lists only", SubDivideToStudyListsOnlyHelp,
                        null,
                        null);
                    TitlePrefix = GetArgumentDefaulted("TitlePrefix", "string", "r", "Default", "Title prefix", TitlePrefixHelp);
                    DefaultContentType = GetArgumentDefaulted("DefaultContentType", "string", "r", DefaultContentType, "Default content type", DefaultContentTypeHelp);
                    DefaultContentSubType = GetArgumentDefaulted("DefaultContentSubType", "string", "r", DefaultContentSubType, "Default content sub-type", DefaultContentSubTypeHelp);
                    MasterName = GetMasterListArgumentDefaulted("MasterName", "stringlist", "r", MasterName, "Master name", MasterNameHelp);
                    StudyItemSubDivideCount = GetIntegerArgumentDefaulted("StudyItemSubDivideCount", "integer", "r", StudyItemSubDivideCount, "Study items per leaf", StudyItemSubDivideCountHelp);
                    MinorSubDivideCount = GetIntegerArgumentDefaulted("MinorSubDivideCount", "integer", "r", MinorSubDivideCount, "Minor subdivide count", MinorSubDivideCountHelp);
                    MajorSubDivideCount = GetIntegerArgumentDefaulted("MajorSubDivideCount", "integer", "r", MajorSubDivideCount, "Major subdivide count", MinorSubDivideCountHelp);
                    break;
                case "BaseObjectContent":
                    SubDivide = GetFlagArgumentDefaulted("SubDivide", "flag", "r", SubDivide,
                        "Subdivide items into smaller groups", SubDivideHelp,
                        new List<string>()
                        {
                            "SubDivideToStudyListsOnly",
                            "MasterName",
                            "StudyItemSubDivideCount",
                            "MinorSubDivideCount",
                            "MajorSubDivideCount",
                        },
                        null);
                    SubDivideToStudyListsOnly = GetFlagArgumentDefaulted("SubDivideToStudyListsOnly", "flag", "r",
                        SubDivideToStudyListsOnly, "Subdivide to study lists only", SubDivideToStudyListsOnlyHelp,
                        null,
                        new List<string>()
                        {
                            "TitlePrefix",
                            "DefaultContentType",
                            "DefaultContentSubType",
                            "Label"
                        });
                    TitlePrefix = GetArgumentDefaulted("TitlePrefix", "string", "r", "Default", "Title prefix", TitlePrefixHelp);
                    DefaultContentType = GetArgumentDefaulted("DefaultContentType", "string", "r", DefaultContentType, "Default content type", DefaultContentTypeHelp);
                    DefaultContentSubType = GetArgumentDefaulted("DefaultContentSubType", "string", "r", DefaultContentSubType, "Default content sub-type", DefaultContentSubTypeHelp);
                    MasterName = GetMasterListArgumentDefaulted("MasterName", "stringlist", "r", MasterName, "Master name", MasterNameHelp);
                    StudyItemSubDivideCount = GetIntegerArgumentDefaulted("StudyItemSubDivideCount", "integer", "r", StudyItemSubDivideCount, "Study items per leaf", StudyItemSubDivideCountHelp);
                    MinorSubDivideCount = GetIntegerArgumentDefaulted("MinorSubDivideCount", "integer", "r", MinorSubDivideCount, "Minor subdivide count", MinorSubDivideCountHelp);
                    MajorSubDivideCount = GetIntegerArgumentDefaulted("MajorSubDivideCount", "integer", "r", MajorSubDivideCount, "Major subdivide count", MinorSubDivideCountHelp);
                    break;
                default:
                    break;
            }

            TargetOwner = GetArgumentDefaulted("Owner", "string", "r", Owner, "Owner",
                OwnerHelp);
            MakeTargetPublic = GetFlagArgumentDefaulted("MakeTargetPublic", "flag", "r",
                MakeTargetPublic, "Make public", MakeTargetPublicHelp, null, null);
            Package = GetArgumentDefaulted("Package", "string", "r", Package, "Package",
                PackageHelp);

            DoSentenceFixes = GetFlagArgumentDefaulted("DoSentenceFixes", "flag", "r", DoSentenceFixes, "Do sentence fixes",
                DoSentenceFixesHelp, null, null);
            DoWordFixes = GetFlagArgumentDefaulted("DoWordFixes", "flag", "r", DoWordFixes, "Do word fixes",
                DoWordFixesHelp, null, null);

            if (NodeUtilities != null)
                Label = NodeUtilities.GetLabelFromContentType(DefaultContentType, DefaultContentSubType);
        }

        public override void SaveToArguments()
        {
            base.SaveToArguments();

            SetStringListArgument("SubFormat", "stringlist", "r", SubFormat, SubFormats, "SubFormat",
                SubFormatHelp);

            SetFlagArgument("DeleteBeforeImport", "flag", "r", DeleteBeforeImport, "Delete before import",
                DeleteBeforeImportHelp, null, null);

            SetFlagArgument("IsDoMerge", "flag", "r", IsDoMerge, "Merge", IsDoMergeHelp, null, null);

            SetLanguageIDListArgument(
                "SubtitleLanguageIDs",
                "languagelist",
                "r",
                SubtitleLanguageIDs,
                "Subtitle Languages",
                SubtitleLanguageIDsHelp);

            SetArgument("MediaItemKey", "string", "r", MediaItemKey, "MediaItemKey",
                MediaItemKeyHelp, null, null);

            SetArgument("LanguageMediaItemKey", "string", "r", LanguageMediaItemKey, "LanguageMediaItemKey",
                LanguageMediaItemKeyHelp, null, null);

            SetStringListArgument("MediaRunKey", "string", "r",
                MediaRunKey, MediaRun.MediaRunKeys, "MediaRunKey",
                MediaRunKeyHelp);

            SetFloatArgument(
                "SubtitleTimeOffset",
                "float",
                "r",
                SubtitleTimeOffset,
                "Subtitle Time Offset",
                SubtitleTimeOffsetHelp);

            SetFlagArgument("ExtractSpeakerNames", "flag", "r", ExtractSpeakerNames, "Extract speaker names", ExtractSpeakerNamesHelp, null, null);
            SetFlagArgument("AddTranslationSuffix", "flag", "r", AddTranslationSuffix, "Add translation suffix", AddTranslationSuffixHelp, null, null);
            SetFlagArgument("FilterBracketed", "flag", "r", FilterBracketed, "Filter bracketed", FilterBracketedHelp,
                null, null);

            SetFlagArgument("IsTranslateMissingItems", "flag", "r", IsTranslateMissingItems, "Translate missing items",
                IsTranslateMissingItemsHelp, null, null);
            SetFlagArgument("IsSynthesizeMissingAudio", "flag", "r", IsSynthesizeMissingAudio, "Synthesize missing audio",
                IsSynthesizeMissingAudioHelp, null, null);
            SetFlagArgument("IsForceAudio", "flag", "r", IsForceAudio, "Force synthesis of audio",
                IsForceAudioHelp, null, null);
            SetIntegerArgument("VoiceSpeed", "integer", "r", 0,
                "Voice speed", VoiceSpeedHelp);

            switch (TargetType)
            {
                case "BaseObjectNode":
                case "BaseObjectNodeTree":
                    SetFlagArgument("SubDivide", "flag", "r", SubDivide,
                        "Subdivide items into smaller groups", SubDivideHelp,
                        new List<string>()
                        {
                            "SubDivideToStudyListsOnly",
                            "MasterName",
                            "StudyItemSubDivideCount",
                            "MinorSubDivideCount",
                            "MajorSubDivideCount",
                        },
                        null);
                    SetFlagArgument("SubDivideToStudyListsOnly", "flag", "r", SubDivideToStudyListsOnly,
                        "Subdivide to study lists only", SubDivideToStudyListsOnlyHelp,
                        null,
                        null);
                    SetArgument("TitlePrefix", "string", "r", TitlePrefix, "Title prefix", TitlePrefixHelp, null, null);
                    SetArgument("DefaultContentType", "string", "r", DefaultContentType, "Default content type", DefaultContentTypeHelp, null, null);
                    SetArgument("DefaultContentSubType", "string", "r", DefaultContentSubType, "Default content sub-type", DefaultContentSubTypeHelp, null, null);
                    SetMasterListArgument("MasterName", "stringlist", "r", MasterName, "Master name", MasterNameHelp);
                    SetIntegerArgument("StudyItemSubDivideCount", "integer", "r", StudyItemSubDivideCount, "Items per leaf", StudyItemSubDivideCountHelp);
                    SetIntegerArgument("MinorSubDivideCount", "integer", "r", MinorSubDivideCount, "Minor subdivide count", MinorSubDivideCountHelp);
                    SetIntegerArgument("MajorSubDivideCount", "integer", "r", MajorSubDivideCount, "Major subdivide count", MinorSubDivideCountHelp);
                    break;
                case "BaseObjectContent":
                    SetFlagArgument("SubDivide", "flag", "r", SubDivide,
                        "Subdivide items into smaller groups", SubDivideHelp,
                        new List<string>()
                        {
                            "SubDivideToStudyListsOnly",
                            "MasterName",
                            "StudyItemSubDivideCount",
                            "MinorSubDivideCount",
                            "MajorSubDivideCount"
                        },
                        null);
                    SetFlagArgument("SubDivideToStudyListsOnly", "flag", "r", SubDivideToStudyListsOnly,
                        "Subdivide to study lists only", SubDivideToStudyListsOnlyHelp,
                        null,
                        new List<string>()
                        {
                            "TitlePrefix",
                            "DefaultContentType",
                            "DefaultContentSubType"
                        });
                    SetArgument("TitlePrefix", "string", "r", TitlePrefix, "Title prefix", TitlePrefixHelp, null, null);
                    SetArgument("DefaultContentType", "string", "r", DefaultContentType, "Default content type", DefaultContentTypeHelp, null, null);
                    SetArgument("DefaultContentSubType", "string", "r", DefaultContentSubType, "Default content sub-type", DefaultContentSubTypeHelp, null, null);
                    SetMasterListArgument("MasterName", "stringlist", "r", MasterName, "Master name", MasterNameHelp);
                    SetIntegerArgument("StudyItemSubDivideCount", "integer", "r", StudyItemSubDivideCount, "Items per leaf", StudyItemSubDivideCountHelp);
                    SetIntegerArgument("MinorSubDivideCount", "integer", "r", MinorSubDivideCount, "Minor subdivide count", MinorSubDivideCountHelp);
                    SetIntegerArgument("MajorSubDivideCount", "integer", "r", MajorSubDivideCount, "Major subdivide count", MinorSubDivideCountHelp);
                    break;
                default:
                    break;
            }

            SetArgument("Owner", "string", "r", Owner, "Owner",
                OwnerHelp, null, null);
            SetFlagArgument("MakeTargetPublic", "flag", "r", MakeTargetPublic, "Make targets public",
                MakeTargetPublicHelp, null, null);
            SetArgument("Package", "string", "r", Package, "Package",
                PackageHelp, null, null);

            SetFlagArgument("DoSentenceFixes", "flag", "r", DoSentenceFixes, "Do sentence fixes",
                DoSentenceFixesHelp, null, null);
            SetFlagArgument("DoWordFixes", "flag", "r", DoWordFixes, "Do word fixes",
                DoWordFixesHelp, null, null);
        }

        public override void DumpArguments(string label)
        {
            base.DumpArguments(label);
        }

        public static new bool IsSupportedStatic(string importExport, string contentName, string capability)
        {
            if (importExport == "Export")
                return false;

            switch (contentName)
            {
                case "BaseObjectNodeTree":
                case "BaseObjectNode":
                case "BaseObjectContent":
                case "ContentStudyList":
                case "MultiLanguageItem":
                    if (capability == "Support")
                        return true;
                    else if (capability == "Text")
                        return true;
                    else if (capability == "RecursedStudyItems")
                        return true;
                    return false;
                default:
                    return false;
            }
        }

        public override bool IsSupportedVirtual(string importExport, string contentName, string capability)
        {
            return IsSupportedStatic(importExport, contentName, capability);
        }

        public static new string TypeStringStatic { get { return "Subtitles"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
