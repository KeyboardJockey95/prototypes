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
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Language;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatSubs2Srs : Format
    {
        // Parameters.
        public string ShowName { get; set; }
        public List<LanguageID> SubtitleLanguageIDs { get; set; }
        public float SubtitleTimeOffset { get; set; }
        public string TargetOwner { get; set; }
        public string Package { get; set; }
        public bool DoSentenceFixes { get; set; }
        public string SentenceFixesKey { get; set; }
        public bool DoWordFixes { get; set; }
        public string WordFixesKey { get; set; }
        public bool ExtractSpeakerNames { get; set; }
        public string UserMediaFilePath;
        public bool IncludeMedia { get; set; }
        public bool LinkMediaItem { get; set; }
        public string MediaItemKey { get; set; }
        public string LanguageMediaItemKey { get; set; }
        public string MediaRunKey { get; set; }
        private List<string> MediaFiles;
        private string ContentFilePath;
        private string MediaFilePath;

        // Implementation.
        protected ContentStudyList StudyList { get; set; }
        protected SentenceFixes SentenceFixes { get; set; }
        protected List<MultiLanguageItem> StudyItems { get; set; }
        protected List<string> Names { get; set; }

        private static string FormatDescription = "Format for importing/exporting subtitles and media from a"
            + " .zip of the output of the program \"subs2srs\".";

        public FormatSubs2Srs()
            : base(
                  "Subs2Srs",
                  "FormatSubs2Srs",
                  FormatDescription,
                  String.Empty,
                  String.Empty,
                  "text/plain",
                  ".zip",
                  null,
                  null,
                  null,
                  null,
                  null)
        {
            ClearFormatSubs2Srs();
        }

        public FormatSubs2Srs(FormatSubs2Srs other)
            : base(other)
        {
            CopyFormatSubs2Srs(other);
        }

        public FormatSubs2Srs(
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
            ClearFormatSubs2Srs();
        }

        public void ClearFormatSubs2Srs()
        {
            // Local parameters.

            ShowName = String.Empty;
            SubtitleLanguageIDs = null;
            SubtitleTimeOffset = 0.0f;
            Package = String.Empty;
            DoSentenceFixes = false;
            SentenceFixesKey = null;
            DoWordFixes = false;
            WordFixesKey = null;
            ExtractSpeakerNames = false;
            LinkMediaItem = false;
            MediaItemKey = "VideoLesson";
            LanguageMediaItemKey = "";
            MediaRunKey = "Video";

            // Base parameters.

            IncludeMedia = true;
            DefaultContentType = "Text";
            DefaultContentSubType = "Text";
            Label = "Text";

            // Implementation.

            StudyList = null;
            SentenceFixes = null;
            StudyItems = null;
            Names = null;
        }

        public void CopyFormatSubs2Srs(FormatSubs2Srs other)
        {
            ShowName = other.ShowName;

            if (other.SubtitleLanguageIDs != null)
                SubtitleLanguageIDs = new List<LanguageID>(SubtitleLanguageIDs);
            else
                SubtitleLanguageIDs = null;

            LinkMediaItem = other.LinkMediaItem;
            MediaItemKey = other.MediaItemKey;
            LanguageMediaItemKey = other.LanguageMediaItemKey;
            MediaRunKey = other.MediaRunKey;
            SubtitleTimeOffset = other.SubtitleTimeOffset;
            ExtractSpeakerNames = other.ExtractSpeakerNames;
        }

        public override Format Clone()
        {
            return new FormatSubs2Srs(this);
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

        private string globalMediaDir;
        private string globalUserMediaDir;
        private string globalUserMediaDirLower;
        private string externalUserPathNode;
        private string externalUserPath;
        private string externalUserPathLower;
        private List<string> directoriesToDelete;

        public override void Read(Stream stream)
        {
            int b1 = stream.ReadByte();
            int b2 = stream.ReadByte();
            string userName = (UserRecord != null ? UserRecord.UserName : "Guest");
            string dir = ApplicationData.TempPath + ApplicationData.PlatformPathSeparator +
                userName + ApplicationData.PlatformPathSeparator;
            string filePath;
            string mediaDir;

            if ((SubtitleLanguageIDs == null) || (SubtitleLanguageIDs.Count() == 0))
            {
                PutError("You need to select the subtitle language(s).");
                return;
            }

            if (DeleteBeforeImport)
            {
                StudyList = GetTargetStudyList();

                if (StudyList != null)
                {
                    NodeUtilities.DeleteContentChildrenHelper(StudyList.Content);
                    NodeUtilities.DeleteStudyListContentsHelper(StudyList.Content, true);
                }
            }

            if (!IncludeMedia)
                ReadTSV(stream);
            else
            {
                switch (TargetType)
                {
                    case "DictionaryEntry":
                        globalMediaDir = ApplicationData.ContentPath +
                            ApplicationData.PlatformPathSeparator +
                            "Dictionary" +
                            ApplicationData.PlatformPathSeparator;
                        globalUserMediaDir = globalMediaDir;
                        break;
                    default:
                        globalMediaDir = ApplicationData.MediaPath + ApplicationData.PlatformPathSeparator;
                        if (String.IsNullOrEmpty(UserMediaFilePath))
                            globalUserMediaDir = globalMediaDir + userName + ApplicationData.PlatformPathSeparator;
                        else
                            globalUserMediaDir = UserMediaFilePath + ApplicationData.PlatformPathSeparator;
                        break;
                }

                globalUserMediaDirLower = globalUserMediaDir.ToLower();
                externalUserPathNode = "externalUser/";
                externalUserPath = externalUserPathNode;
                externalUserPathLower = externalUserPath.ToLower();
                directoriesToDelete = new List<string>();

                MediaFilePath = null;

                if ((Targets != null) && (Targets.Count() == 1) && (Targets[0] is BaseObjectTitled))
                {
                    BaseObjectTitled titledObject = Targets[0] as BaseObjectTitled;

                    if (titledObject is BaseObjectNode)
                    {
                        BaseObjectNode node = titledObject as BaseObjectNode;
                        mediaDir = node.MediaDirectoryPath;
                    }
                    else
                        mediaDir = titledObject.MediaDirectoryPath;

                    mediaDir += ApplicationData.PlatformPathSeparator;
                }
                else if (!String.IsNullOrEmpty(TargetMediaDirectory))
                    mediaDir = TargetMediaDirectory + ApplicationData.PlatformPathSeparator;
                else
                    mediaDir = globalUserMediaDir;

                MediaFilePath = mediaDir;
                ContentFilePath = mediaDir + ShowName + ".tsv";
                MediaFiles = new List<string>();

                stream.Seek(0, SeekOrigin.Begin);

                if ((b1 == 'P') && (b2 == 'K'))
                {
                    filePath = dir + "mediapackage" + DefaultFileExtension;

                    if (FileSingleton.Exists(filePath))
                        FileSingleton.Delete(filePath);

                    if (!WriteToTemporary(stream, filePath))
                        return;

                    IArchiveFile zipFile = null;

                    try
                    {
                        zipFile = FileSingleton.Archive();

                        if (zipFile.Create(filePath))
                        {
                            ContinueProgress(zipFile.Count() + 2);
                            zipFile.Extract(mediaDir, true, HandleReadFile);
                            zipFile.Close();
                            zipFile = null;
                        }

                        UpdateProgressElapsed("Converting media files...");
                        PostProcessMediaFiles(mediaDir, MediaFiles);
                    }
                    catch (Exception exc)
                    {
                        string msg = "Exception during zip: " + exc.Message;
                        if (exc.InnerException != null)
                            msg += exc.InnerException.Message;
                        throw new Exception(msg);
                    }
                    finally
                    {
                        if (zipFile != null)
                            zipFile.Close();

                        UpdateProgressElapsed("Cleaning up...");

                        if (FileSingleton.Exists(ContentFilePath))
                            FileSingleton.Delete(ContentFilePath);

                        if (FileSingleton.Exists(filePath))
                            FileSingleton.Delete(filePath);

                        if (directoriesToDelete.Count() != 0)
                        {
                            foreach (string deleteDir in directoriesToDelete)
                            {
                                try
                                {
                                    FileSingleton.DeleteDirectory(deleteDir);
                                }
                                catch (Exception exc)
                                {
                                    string msg = "Exception during deleting external directories: " + exc.Message;
                                    if (exc.InnerException != null)
                                        msg += exc.InnerException.Message;
                                }
                            }
                        }

                        string externalPath = mediaDir + externalUserPathNode;

                        if (FileSingleton.DirectoryExists(externalPath))
                        {
                            try
                            {
                                FileSingleton.DeleteDirectory(externalPath);
                            }
                            catch (Exception exc)
                            {
                                string msg = "Exception during deleting external directory: " + exc.Message;
                                if (exc.InnerException != null)
                                    msg += exc.InnerException.Message;
                            }
                        }

                        ContentFilePath = null;
                        MediaFiles = null;

                        EndContinuedProgress();
                    }
                }
                else
                {
                    Error = "Read not supported for this file type.";
                    throw new Exception(Error);
                }
            }

            if (SubDivide)
            {
                UpdateProgressElapsed("Subdividing study lists ...");
                SubDivideStudyList(StudyList);
            }
        }

        public bool HandleReadFile(string filePath, ref string baseDirectory)
        {
            string tsvFileName = ShowName + ".tsv";
            bool returnValue = true;

            ApplicationData.Global.PutConsoleMessage("HandleReadFile(\"" + filePath + "\", \"" + baseDirectory + "\")");

            if (filePath == tsvFileName)
            {
                Stream contentStream = null;

                try
                {
                    contentStream = FileSingleton.OpenRead(ContentFilePath);

                    if (contentStream != null)
                    {
                        // Get the .tsv file, pulling out the study lists.
                        ReadTSV(contentStream);
                    }
                }
                catch (Exception)
                {
                    returnValue = false;
                }
                finally
                {
                    if (contentStream != null)
                        FileSingleton.Close(contentStream);

                    UpdateProgressElapsed("Unpacking media files...");
                }
            }
            else if (filePath.EndsWith("/"))
            {
                string deletePath = MediaUtilities.ConcatenateFilePath(
                    MediaFilePath,
                    filePath.Replace("/", ApplicationData.PlatformPathSeparator));

                if (!directoriesToDelete.Contains(deletePath))
                    directoriesToDelete.Add(deletePath);
            }
            else
            {
                string fileName = MediaUtilities.GetFileName(filePath);
                string fullFilePath = MediaUtilities.ConcatenateFilePath(
                    MediaFilePath,
                    filePath.Replace("/", ApplicationData.PlatformPathSeparator));
                string fullNewFilePath = MediaUtilities.ConcatenateFilePath(MediaFilePath, fileName);

                try
                {
                    if (FileSingleton.Exists(fullNewFilePath))
                        FileSingleton.Delete(fullNewFilePath);

                    FileSingleton.Move(fullFilePath, fullNewFilePath);
                }
                catch (Exception)
                {
                    returnValue = false;
                }

                MediaFiles.Add(fileName);

                UpdateProgressElapsed("Unpacked " + fileName + "...");
            }

            return returnValue;
        }

        private int CountReadTargets()
        {
            int count = 0;

            foreach (IBaseObject readTarget in ReadObjects)
            {
                if (readTarget.GetType().Name == TargetType)
                    count++;
            }

            return count;
        }

        protected void ReadTSV(Stream stream)
        {
            List<LanguageID> languageIDs = UniqueLanguageIDs;
            bool isOK = true;

            UpdateProgressElapsed("Reading .tsv file ...");

            Component = StudyList = GetTargetStudyList();

            if (StudyList != null)
            {
                InitializeTools();

                try
                {
                    UpdateProgressElapsed("Reading .tsv file ...");

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string showName;
                        int sectionNumber;
                        string targetText;
                        string hostText;
                        string mp3FileName;
                        string imageFileName;
                        TimeSpan startTime;
                        TimeSpan endTime;
                        int lineNumber = 0;

                        // Load entries.
                        while (ReadEntry(
                            reader,
                            out showName,
                            out sectionNumber,
                            out targetText,
                            out hostText,
                            out mp3FileName,
                            out imageFileName,
                            out startTime,
                            out endTime,
                            ref lineNumber))
                        {
                            if (String.IsNullOrEmpty(targetText))
                                continue;

                            if (!ProcessEntry(
                                showName,
                                sectionNumber,
                                targetText,
                                hostText,
                                mp3FileName,
                                imageFileName,
                                startTime,
                                endTime,
                                lineNumber))
                            {
                                isOK = false;
                                break;
                            }
                        }
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

        protected bool ReadEntry(
            StreamReader reader,
            out string showName,
            out int sectionNumber,
            out string targetText,
            out string hostText,
            out string mp3FileName,
            out string imageFileName,
            out TimeSpan startTime,
            out TimeSpan endTime,
            ref int lineNumber)
        {
            string line;

            showName = String.Empty;
            sectionNumber = -1;
            targetText = String.Empty;
            hostText = String.Empty;
            mp3FileName = String.Empty;
            imageFileName = String.Empty;
            startTime = TimeSpan.Zero;
            endTime = TimeSpan.Zero;

            for (;;)
            {
                line = reader.ReadLine();

                if (line == null)
                    return false;

                line = line.Trim();

                lineNumber++;

                if (String.IsNullOrEmpty(line))
                    continue;

                string[] parts = line.Split(LanguageLookup.Tab);
                int partsCount = parts.Length;

                if (partsCount != 6)
                    throw new Exception("Error line " + lineNumber.ToString() + ": Expected 6 fields in subs2srs line: tags seq sound image target host");

                string nameLabel = parts[0];
                string soundLabel = parts[2];
                string imageElement = parts[3];

                targetText = parts[4].Trim();

                hostText = parts[5].Trim();

                string name;

                if (!ObjectUtilities.ParseStringUnderscoreInteger(nameLabel, out name, out sectionNumber))
                    throw new Exception("Error line " + lineNumber.ToString() + " parsing name label: " + nameLabel);

                if (!soundLabel.StartsWith("[sound:") || !soundLabel.EndsWith("]"))
                    throw new Exception("Error line " + lineNumber.ToString() + " parsing sound label: " + soundLabel);

                mp3FileName = soundLabel.Substring(7, soundLabel.Length - 8);

                int tagStartOffset = mp3FileName.LastIndexOf('_');
                int tagEndOffset = mp3FileName.LastIndexOf('.');
                int dashOffset = mp3FileName.LastIndexOf('-');

                if ((tagStartOffset == -1) || (tagEndOffset == -1) || (dashOffset == -1))
                    throw new Exception("Error line " + lineNumber.ToString() + " parsing sound label: " + soundLabel);

                tagStartOffset++;

                string startTimeString = mp3FileName.Substring(tagStartOffset, dashOffset - tagStartOffset);

                if (!ParseTimeField(startTimeString, out startTime))
                    throw new Exception("Error line " + lineNumber.ToString() + " parsing start time: " + startTimeString);

                dashOffset++;

                string endTimeString = mp3FileName.Substring(dashOffset, tagEndOffset - dashOffset);

                if (!ParseTimeField(endTimeString, out endTime))
                    throw new Exception("Error line " + lineNumber.ToString() + " parsing end time: " + endTimeString);

                string[] imageParts = imageElement.Split(LanguageLookup.DoubleQuote);

                if (imageParts.Length != 3)
                    throw new Exception("Error line " + lineNumber.ToString() + " parsing image: " + imageElement);

                imageFileName = imageParts[1];

                return true;
            }
        }

        protected bool ParseTimeField(
            string text,
            out TimeSpan time)
        {
            int hours;
            int minutes;
            int seconds;
            int milliseconds;
            string[] parts = text.Split(LanguageLookup.Dot);

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

        protected bool ProcessEntry(
            string showName,
            int sectionNumber,
            string targetText,
            string hostText,
            string mp3FileName,
            string imageFileName,
            TimeSpan startTime,
            TimeSpan endTime,
            int lineNumber)
        {
            BaseObjectContent content = StudyList.Content;
            MultiLanguageItem studyItem;

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

            string studyItemKey = StudyList.AllocateStudyItemKey();
            studyItem = new MultiLanguageItem(studyItemKey, StudyList.Content.LanguageIDs);
            OverwriteText(studyItem, targetText, hostText, mp3FileName, imageFileName, startTime, endTime);
            StudyList.AddStudyItem(studyItem);
            ItemIndex = ItemIndex + 1;

            if (StudyItems == null)
                StudyItems = new List<MultiLanguageItem>() { studyItem };
            else
                StudyItems.Add(studyItem);

            return true;
        }

        protected void OverwriteText(
            MultiLanguageItem studyItem,
            string targetText,
            string hostText,
            string mp3FileName,
            string imageFileName,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            TimeSpan lengthTime = endTime - startTime;

            if (String.IsNullOrEmpty(targetText) && String.IsNullOrEmpty(hostText))
                return;

            if (String.IsNullOrEmpty(LanguageMediaItemKey))
                LanguageMediaItemKey = TargetLanguageID.LanguageCode;

            int languageIndex;
            int languageCount = SubtitleLanguageIDs.Count();

            for (languageIndex = 0; languageIndex < languageCount; languageIndex++)
            {
                LanguageID languageID = SubtitleLanguageIDs[languageIndex];
                LanguageItem languageItem = studyItem.LanguageItem(languageID);
                string text = (languageIndex == 0 ? targetText : hostText);
                string sentenceText = ProcessSentenceText(
                    studyItem,
                    text,
                    languageID);

                if (languageItem != null)
                {
                    languageItem.Text = sentenceText;
                    languageItem.SentenceRuns = new List<TextRun>(1);
                    TextRun sentenceRun;
                    List<MediaRun> mediaRuns = null;

                    if (IncludeMedia && (languageIndex == 0))
                    {
                        MediaRun fileMediaRun = new MediaRun(MediaRunKey, mp3FileName, Owner);

                        if (mediaRuns == null)
                            mediaRuns = new List<MediaRun>();

                        mediaRuns.Add(fileMediaRun);

                        if (!String.IsNullOrEmpty(imageFileName))
                        {
                            MediaRun imageMediaRun = new MediaRun("Picture", imageFileName, Owner);

                            if (mediaRuns == null)
                                mediaRuns = new List<MediaRun>();

                            mediaRuns.Add(imageMediaRun);
                        }
                    }

                    if (LinkMediaItem)
                    {
                        MediaRun referenceMediaRun = new MediaRun(MediaRunKey, null, MediaItemKey, LanguageMediaItemKey, Owner, startTime, lengthTime);

                        if (mediaRuns == null)
                            mediaRuns = new List<MediaRun>();

                        mediaRuns.Add(referenceMediaRun);
                    }

                    sentenceRun = new TextRun(
                        0,
                        sentenceText.Length,
                        mediaRuns);
                    languageItem.AddSentenceRun(sentenceRun);
                    FixupLanguageItem(languageItem);
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

        public static string ShowNameHelp = "Enter the show name.";
        public static string SubtitleLanguageIDsHelp = "Subtitle languages (target or target and host).";
        public static string IncludeMediaHelp = "Check this to upload a zip with media, or uncheck it to upload a .tsv file only.";
        public static string LinkMediaItemHelp = "Check this to add media item reference.";
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

            DeleteBeforeImport = GetFlagArgumentDefaulted("DeleteBeforeImport", "flag", "r", DeleteBeforeImport,
                "Delete before import", DeleteBeforeImportHelp, null, null);

            ShowName = GetArgumentDefaulted("ShowName", "string", "r", ShowName, "Show name",
                ShowNameHelp);

            SubtitleLanguageIDs = GetLanguageIDListArgumentDefaulted(
                "SubtitleLanguageIDs",
                "languagelist",
                "r",
                SubtitleLanguageIDs,
                "Subtitle Languages",
                SubtitleLanguageIDsHelp);

            SubtitleTimeOffset = GetFloatArgumentDefaulted(
                "SubtitleTimeOffset",
                "float",
                "r",
                SubtitleTimeOffset,
                "Subtitle Time Offset",
                SubtitleTimeOffsetHelp);

            IncludeMedia = GetFlagArgumentDefaulted("IncludeMedia", "flag", "r", IncludeMedia,
                "Zip with individual media", IncludeMediaHelp, null, null);

            LinkMediaItem = GetFlagArgumentDefaulted("LinkMediaItem", "flag", "r", LinkMediaItem,
                "Link media item", LinkMediaItemHelp,
                new List<string>()
                {
                    "MediaItemKey",
                    "LanguageMediaItemKey",
                    "MediaRunKey"
                },
                null);

            MediaItemKey = GetArgumentDefaulted("MediaItemKey", "string", "r", MediaItemKey, "MediaItemKey",
                MediaItemKeyHelp);

            LanguageMediaItemKey = GetArgumentDefaulted("LanguageMediaItemKey", "string", "r",
                LanguageMediaItemKey, "LanguageMediaItemKey",
                LanguageMediaItemKeyHelp);

            MediaRunKey = GetStringListArgumentDefaulted("MediaRunKey", "string", "r",
                MediaRunKey, MediaRun.MediaRunKeys, "MediaRunKey",
                MediaRunKeyHelp);

            IsSynthesizeMissingAudio = GetFlagArgumentDefaulted("IsSynthesizeMissingAudio", "flag", "r", IsSynthesizeMissingAudio, "Synthesize missing audio",
                IsSynthesizeMissingAudioHelp, null, null);
            IsForceAudio = GetFlagArgumentDefaulted("IsForceAudio", "flag", "r", IsForceAudio, "Force synthesis of audio",
                IsForceAudioHelp, null, null);

            ExtractSpeakerNames = GetFlagArgumentDefaulted("ExtractSpeakerNames", "flag", "r", ExtractSpeakerNames, "Extract speaker names", ExtractSpeakerNamesHelp,
                null, null);

            IsTranslateMissingItems = GetFlagArgumentDefaulted("IsTranslateMissingItems", "flag", "r", IsTranslateMissingItems, "Translate missing items",
                IsTranslateMissingItemsHelp, null, null);

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
                            "MajorSubDivideCount"
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

            SetFlagArgument("DeleteBeforeImport", "flag", "r", DeleteBeforeImport, "Delete before import",
                DeleteBeforeImportHelp, null, null);

            SetArgument("ShowName", "string", "r", ShowName, "Show name",
                ShowNameHelp, null, null);

            SetLanguageIDListArgument(
                "SubtitleLanguageIDs",
                "languagelist",
                "r",
                SubtitleLanguageIDs,
                "Subtitle Languages",
                SubtitleLanguageIDsHelp);

            SetFloatArgument(
                "SubtitleTimeOffset",
                "float",
                "r",
                SubtitleTimeOffset,
                "Subtitle Time Offset",
                SubtitleTimeOffsetHelp);

            SetFlagArgument("IncludeMedia", "flag", "r", IncludeMedia,
                "Zip with individual media", IncludeMediaHelp, null, null);

            SetFlagArgument("LinkMediaItem", "flag", "r", LinkMediaItem,
                "Link media item", LinkMediaItemHelp,
                new List<string>()
                {
                    "MediaItemKey",
                    "LanguageMediaItemKey",
                    "MediaRunKey"
                },
                null);

            SetArgument("MediaItemKey", "string", "r", MediaItemKey, "MediaItemKey",
                MediaItemKeyHelp, null, null);

            SetArgument("LanguageMediaItemKey", "string", "r", LanguageMediaItemKey, "LanguageMediaItemKey",
                LanguageMediaItemKeyHelp, null, null);

            SetStringListArgument("MediaRunKey", "string", "r",
                MediaRunKey, MediaRun.MediaRunKeys, "MediaRunKey",
                MediaRunKeyHelp);

            SetFlagArgument("IsSynthesizeMissingAudio", "flag", "r", IsSynthesizeMissingAudio, "Synthesize missing audio",
                IsSynthesizeMissingAudioHelp, null, null);
            SetFlagArgument("IsForceAudio", "flag", "r", IsForceAudio, "Force synthesis of audio",
                IsForceAudioHelp, null, null);
            SetIntegerArgument("VoiceSpeed", "integer", "r", 0,
                "Voice speed", VoiceSpeedHelp);

            SetFlagArgument("ExtractSpeakerNames", "flag", "r", ExtractSpeakerNames, "Extract speaker names", ExtractSpeakerNamesHelp, null, null);

            SetFlagArgument("IsTranslateMissingItems", "flag", "r", IsTranslateMissingItems, "Translate missing items",
                IsTranslateMissingItemsHelp, null, null);

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

        public static new string TypeStringStatic { get { return "Subs2Srs"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
