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
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Markup
{
    public class AudioMarkupRenderer : MarkupRendererContent
    {
        public LanguageID CurrentVoiceLanguageID { get; set; }
        public string CurrentHostVoiceName { get; set; }
        public string CurrentTargetVoiceName { get; set; }
        public string CurrentVoiceName { get; set; }
        public Dictionary<string, string> SpeakerToVoiceNameMap { get; set; }
        public string CurrentSpeed { get; set; }
        public int DefaultSpeed { get; set; }
        public double DefaultPauseSeconds { get; set; }
        public bool FilterAsides { get; set; }
        public XElement GenerateElement { get; set; }
        public BaseObjectContent MediaContent { get; set; }
        public ContentMediaItem MediaItem { get; set; }
        public LanguageMediaItem LanguageMediaItem { get; set; }
        public BaseObjectContent TranscriptContent { get; set; }
        public ContentStudyList TranscriptStudyList { get; set; }
        public string GenerateMediaDirectoryUrl { get; set; }
        public string SharedMediaDirectoryUrl { get; set; }
        public string MarkupMediaDirectoryUrl { get; set; }
        public string MediaDirectoryUrl { get; set; }
        public int MaxFileBaseLength { get; set; }
        public int MaxFilePathLength { get; set; }
        public bool IsGenerateLocalTranscript { get; set; }
        public bool IsGenerateStudyItemsOnly { get; set; }
        protected ITextToSpeech SpeechEngine { get; set; }
        protected string AudioOutputPath { get; set; }
        protected string AlternateAudioOutputPath { get; set; }
        protected WaveAudioBuffer AudioBuffer { get; set; }
        protected string AudioBufferPath { get; set; }
        protected int FadeInCount { get; set; }
        protected int FadeOutCount { get; set; }
        protected string CachedFileUrl { get; set; }
        protected WaveAudioBuffer CachedWaveData { get; set; }
        protected MediaRun LastGeneratedMediaRun { get; set; }

        public AudioMarkupRenderer(
                BaseObjectContent documentContent,
                BaseObjectNodeTree sourceTree,
                BaseObjectNode sourceNode,
                List<BaseObjectContent> sourceContents,
                MarkupTemplate markupTemplate,
                UserRecord userRecord,
                UserProfile userProfile,
                LanguageID targetLanguageID,
                LanguageID hostLanguageID,
                LanguageID uiLanguageID,
                LanguageUtilities languageUtilities,
                IMainRepository repositories,
                string hostVoiceName,
                string targetVoiceName,
                Dictionary<string, string> speakerToVoiceNameMap,
                BaseObjectContent mediaContent,
                BaseObjectContent transcriptContent,
                string generateMediaDirectoryUrl,
                string sharedMediaDirectoryUrl,
                bool useAudio,
                bool usePicture,
                bool isGenerateLocalTranscript,
                bool isGenerateStudyItemsOnly)
            : base(
                  documentContent,
                  sourceTree,
                  sourceNode,
                  sourceContents,
                  markupTemplate,
                  userRecord,
                  userProfile,
                  targetLanguageID,
                  hostLanguageID,
                  languageUtilities,
                  repositories,
                  useAudio,
                  usePicture)
        {
            DefaultConfigurationName = "Read0";
            CurrentVoiceLanguageID = null;
            CurrentHostVoiceName = hostVoiceName;
            CurrentTargetVoiceName = targetVoiceName;
            SpeakerToVoiceNameMap = speakerToVoiceNameMap;
            CurrentSpeed = "normal";
            DefaultSpeed = 0;
            DefaultPauseSeconds = 1.0;
            FilterAsides = false;
            MarkupTemplate = markupTemplate;
            if (markupTemplate != null)
                GenerateElement = FindGenerateElement();
            else
                GenerateElement = null;
            if (GenerateElement != null)
                GenerateElement = new XElement(GenerateElement);
            MediaContent = mediaContent;
            MediaItem = MediaContent.ContentStorageMediaItem;
            LanguageMediaItem = MediaItem.GetLanguageMediaItemWithLanguages(
                targetLanguageID, hostLanguageID);
            TranscriptContent = transcriptContent;
            if (TranscriptContent != null)
                TranscriptStudyList = TranscriptContent.ContentStorageStudyList;
            else
                TranscriptStudyList = null;
            GenerateMediaDirectoryUrl = generateMediaDirectoryUrl;
            SharedMediaDirectoryUrl = sharedMediaDirectoryUrl;
            if (MediaItem != null)
                MediaDirectoryUrl = MediaItem.MediaTildeUrl;
            if (markupTemplate == null)
                MarkupMediaDirectoryUrl = String.Empty;
            else
                MarkupMediaDirectoryUrl = markupTemplate.MediaTildeUrl;
            SpeechEngine = TextToSpeechSingleton.GetTextToSpeech();
            MaxFileBaseLength = 80;
            MaxFilePathLength = 240;
            MediaDescription mediaDescription = MediaItem.GetMediaDescriptionWithLanguagesIndexed(
                targetLanguageID, hostLanguageID, 0);
            if (mediaDescription != null)
            {
                AudioOutputPath = ApplicationData.MapToFilePath(
                    mediaDescription.GetContentUrl(MediaContent.MediaTildeUrl));
                AlternateAudioOutputPath = MediaUtilities.GetAlternateFilePath(AudioOutputPath, "-aa.ogg");
            }
            AudioBufferPath = MediaUtilities.ChangeFileExtension(AudioOutputPath, ".wav");
            AudioBuffer = null;
            FadeInCount = 1000;
            FadeOutCount = 1000;
            IsGenerateLocalTranscript = isGenerateLocalTranscript;
            IsGenerateStudyItemsOnly = isGenerateStudyItemsOnly;
            LastGeneratedMediaRun = null;
        }

        public XElement FindGenerateElement()
        {
            if (MarkupTemplate.Markup.Name.LocalName.ToLower() == "generate")
                return MarkupTemplate.Markup;

            XElement generateElement = MarkupTemplate.Markup.Element("generate");

            if (generateElement == null)
                generateElement = MarkupTemplate.Markup.Element("Generate");

            if (generateElement == null)
                generateElement = MarkupTemplate.Markup.Element("GENERATE");

            return generateElement;
        }

        public virtual bool Generate()
        {
            FileSingleton.DirectoryExistsCheck(AudioBufferPath);
            bool nodeWasReplaced = HandleGenerateElement();
            bool returnValue = true;

            if (HasError)
                return false;

            if (AudioBuffer != null)
            {
                string message;

                returnValue = MediaConvertSingleton.ConvertFile(
                    AudioBufferPath, "audio/wav", AudioOutputPath, "audio/mpeg3", out message);

                //if (returnValue)
                //    returnValue = MediaConvertSingleton.ConvertFile(
                //        AudioBufferPath, "audio/wav", AlternateAudioOutputPath, "audio/ogg", out message);

                AudioBuffer.Delete();
            }

            return returnValue;
        }

        protected bool HandleGenerateElement()
        {
            bool returnValue = false;

            MediaItem.DeleteAllLocalStudyItems();

            DeletePriorReferenceMediaRuns();

            DeleteLanguageMediaItemMediaHelper(LanguageMediaItem);

            if (GenerateElement == null)
            {
                if (MarkupTemplate != null)
                {
                    Error = "Can't find \"generate\" element.";
                    return false;
                }

                returnValue = HandleNonTemplateGeneration();
            }
            else
            {
                if (TranscriptStudyList != null)
                    TranscriptStudyList.DeleteAllStudyItems();

                if (StartTime == DateTime.MinValue)
                    StartTime = DateTime.UtcNow;

                RenderedElement = new XElement(GenerateElement);
                returnValue = HandleElement(RenderedElement);
            }

            return returnValue;
        }

        public bool DeleteLanguageMediaItemMediaHelper(LanguageMediaItem languageMediaItem)
        {
            bool returnValue = true;
            BaseObjectContent content = languageMediaItem.Content;
            content.SetupDirectoryCheck();
            string mediaPath = content.MediaTildeUrl;
            foreach (MediaDescription mediaDescription in languageMediaItem.MediaDescriptions)
            {
                if (mediaDescription.IsFullUrl || mediaDescription.IsEmbedded)
                    continue;
                string filePath = mediaDescription.GetDirectoryPath(mediaPath);
                try
                {
                    bool fileUploaded = FileSingleton.Exists(filePath);
                    if (fileUploaded)
                        FileSingleton.Delete(filePath);
                    MediaConvertSingleton.DeleteAlternates(filePath, mediaDescription.MimeType);
                }
                catch (Exception)
                {
                    returnValue = false;
                }
            }
            return returnValue;
        }

        protected override bool HandleNonTemplateGeneration()
        {
            bool returnValue = true;

            InGenerate = true;
            MediaItem.DeleteAllLocalStudyItems();

            StartTime = DateTime.MinValue;
            CurrentTime = StartTime;
            LoadWorkingSet(
                null,
                null,
                null,
                SelectorAlgorithmCode.Forward,
                ToolSelectorMode.Normal,
                ToolProfile.DefaultNewLimit,
                ToolProfile.DefaultReviewLimit,
                false,
                false,
                false,
                ToolProfile.DefaultChunkSize,
                ToolProfile.DefaultReviewLevel,
                DefaultProfileName,
                DefaultConfigurationName);

            StudyItem = null;
            CurrentTargetLanguageID = TargetLanguageID;
            CurrentHostLanguageID = UILanguageID;

            int studyItemCount = WorkingSetStudyList.StudyItemCount();
            int studyItemIndex;

            AppendSilence(DefaultPauseSeconds);

            for (studyItemIndex = 0; studyItemIndex < studyItemCount; studyItemIndex++)
            {
                MultiLanguageItem studyItem = WorkingSetStudyList.GetStudyItemIndexed(studyItemIndex);

                if (!HandleNonTemplateStudyItem(studyItem, CurrentTargetLanguageID, CurrentSpeed))
                    returnValue = false;
            }

            InGenerate = false;

            return returnValue;
        }

        protected bool HandleNonTemplateStudyItem(MultiLanguageItem multiLanguageItem, LanguageID languageID, string speed)
        {
            LanguageID mediaLanguageID = languageID.MediaLanguageID();
            DateTime saveCurrentTime = CurrentTime;
            string message;

            if (Content == null)
                return false;

            if (multiLanguageItem == null)
                return false;

            string mediaDirectoryUrl;

            if (multiLanguageItem.Content != null)
                mediaDirectoryUrl = multiLanguageItem.Content.MediaTildeUrl;
            else
                mediaDirectoryUrl = MediaContent.MediaTildeUrl;

            LanguageItem languageItem = multiLanguageItem.LanguageItem(languageID);

            if (languageItem == null)
                return true;

            if (!languageItem.HasText())
                return true;

            //languageItem.DeleteMediaRunsWithReferenceKey(MediaContent.KeyString);

            speed = GetSpeedKeyFromString(speed);

            if (languageItem.SentenceRunCount() == 0)
                languageItem.LoadSentenceRunsFromText();

            int sentenceCount = languageItem.SentenceRunCount();
            string contentSpeed = "Normal";
            string renderSpeed = "Normal";

            if (sentenceCount != 0)
            {
                int sentenceIndex = 0;

                foreach (TextRun sentenceRun in languageItem.SentenceRuns)
                {
                    string sentenceText = languageItem.GetRunText(sentenceRun);
                    MediaRun mediaRun = null;

                    if (speed == "Normal")
                        mediaRun = sentenceRun.GetMediaRunNonReference("Audio");
                    else if (speed == "Slow")
                    {
                        contentSpeed = "Slow";
                        mediaRun = sentenceRun.GetMediaRunNonReference("SlowAudio");

                        if (mediaRun == null)
                        {
                            mediaRun = sentenceRun.GetMediaRunNonReference("Audio");
                            renderSpeed = "Slow";
                        }
                    }

                    string text = languageItem.GetRunText(sentenceRun);

                    if (mediaRun != null)
                    {
                        if (mediaRun.IsReference)
                        {
                            string mediaItemKey = mediaRun.MediaItemKey;
                            string languageMediaItemKey = mediaRun.LanguageMediaItemKey;
                            ContentMediaItem mediaItem = Node.GetMediaItem(mediaItemKey);

                            if (mediaItem != null)
                            {
                                MediaDescription mediaDescription = mediaItem.GetMediaDescriptionWithLanguagesIndexed(
                                    CurrentTargetLanguageID, CurrentHostLanguageID, 0);

                                if (mediaDescription != null)
                                {
                                    string url = mediaDescription.GetUrl(mediaItem.MediaTildeUrl);

                                    if (String.IsNullOrEmpty(url))
                                    {
                                        HandleError("Media reference media run has no media reference: " + sentenceText);
                                        return false;
                                    }

                                    if (!AppendSourceFileAudioSegment(multiLanguageItem, languageItem, sentenceRun, sentenceText,
                                        languageID, url, mediaRun.Start, mediaRun.Length, contentSpeed, renderSpeed, out message))
                                    {
                                        HandleError("Error appending item: " + message);
                                        return false;
                                    }
                                }
                                else
                                {
                                    HandleError("Media item has no file: " + mediaItemKey + " " + languageMediaItemKey);
                                    return false;
                                }
                            }
                            else
                            {
                                HandleError("Media reference media item not found: " + mediaItemKey);
                                return false;
                            }
                        }
                        else
                        {
                            string url = mediaRun.GetUrl(mediaDirectoryUrl);
                            DateTime saveStart = CurrentTime;
                            TimeSpan start = CurrentTime - StartTime;

                            if (AppendSourceFileAudio(multiLanguageItem, languageItem, sentenceRun,
                                sentenceText, languageID, url, contentSpeed, renderSpeed, out message))
                            {
                                multiLanguageItem.PropogateMediaRun(mediaLanguageID, sentenceIndex, mediaRun);
                                TimeSpan length = CurrentTime - saveStart;
                                MediaRun newMediaRun = CreateSentenceRunReferenceMediaRun(sentenceRun, mediaRun.KeyString, start, length);
                                if (newMediaRun != null)
                                    multiLanguageItem.PropogateMediaRun(mediaLanguageID, sentenceIndex, newMediaRun);
                            }
                            else
                            {
                                HandleError("Error appending item: " + message);
                                return false;
                            }
                        }
                    }
                    else
                        OutputNewSentenceMediaRun(multiLanguageItem, languageItem, sentenceRun, sentenceIndex,
                            text, multiLanguageItem.SpeakerNameKey, languageID,
                            contentSpeed, renderSpeed, null, out message);

                    AppendSilence(DefaultPauseSeconds);

                    sentenceIndex++;
                }
            }
            else
            {
                string text = languageItem.Text;
                OutputNewSentenceMediaRun(multiLanguageItem, languageItem, null, 0,
                    text, multiLanguageItem.SpeakerNameKey, languageID,
                    contentSpeed, speed, null, out message);
                AppendSilence(DefaultPauseSeconds);
            }

            LastItemTime = CurrentTime - saveCurrentTime;

            return true;
        }

        protected override bool HandleGenerate(XElement element)
        {
            bool returnValue;

            InGenerate = true;

            StartTime = DateTime.MinValue;
            CurrentTime = StartTime;
            LoadWorkingSet(
                null,
                null,
                null,
                SelectorAlgorithmCode.Forward,
                ToolSelectorMode.Normal,
                ToolProfile.DefaultNewLimit,
                ToolProfile.DefaultReviewLimit,
                false,
                false,
                false,
                ToolProfile.DefaultChunkSize,
                ToolProfile.DefaultReviewLevel,
                DefaultProfileName,
                DefaultConfigurationName);

            StudyItem = null;
            CurrentTargetLanguageID = TargetLanguageID;
            CurrentHostLanguageID = UILanguageID;

            returnValue = HandleElementChildren(element);

            InGenerate = false;

            return returnValue;
        }

        protected override bool HandleSay(XElement element)
        {
            string field = null;
            bool returnValue = true;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "field":
                        field = attributeValue;
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            if (String.IsNullOrEmpty(field))
            {
                Error = Error + "\nThe \"say\" element needs a \"field\" attribute.";
                returnValue = false;
            }
            else
            {
                string text = EvaluateField(field);
                returnValue = HandleText(text, CurrentSpeed);
            }

            returnValue = HandleElementChildren(element) && returnValue;

            return returnValue;
        }

        protected override bool HandlePause(XElement element)
        {
            double seconds = LastItemTime.TotalSeconds;
            double minimum = 0.0;
            bool returnValue = true;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "seconds":
                        try
                        {
                            seconds = Convert.ToDouble(attributeValue);
                        }
                        catch (Exception)
                        {
                            Error = Error + "\nError converting pause seconds attribute to a number: " + attributeValue;
                        }
                        break;
                    case "multiply":
                        try
                        {
                            double factor = Convert.ToDouble(attributeValue);
                            seconds *= factor;
                        }
                        catch (Exception)
                        {
                            Error = Error + "\nError converting multiply attribute to a number: " + attributeValue;
                        }
                        break;
                    case "add":
                        try
                        {
                            double add = Convert.ToDouble(attributeValue);
                            seconds += add;
                        }
                        catch (Exception)
                        {
                            Error = Error + "\nError converting add attribute to a number: " + attributeValue;
                        }
                        break;
                    case "minimum":
                        try
                        {
                            minimum = Convert.ToDouble(attributeValue);
                        }
                        catch (Exception)
                        {
                            HandleElementError(element, "\nError converting pause seconds attribute to a number: " + attributeValue);
                            return false;
                        }
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute: \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            if (seconds > 0.0)
                returnValue = AppendSilence(seconds);

            returnValue = HandleElementChildren(element) && returnValue;

            return returnValue;
        }

        protected override bool HandleItem(XElement element)
        {
            int startIndex = -1;
            int endIndex = 1;
            string speed = CurrentSpeed;
            LanguageID languageID = CurrentTargetLanguageID;
            bool returnValue = true;

            if (WorkingSet == null)
            {
                HandleElementError(element, "(no working set created--do you have an enclosing generate element?)");
                return false;
            }

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "index":
                        ParseInteger(attributeValue, out startIndex);
                        break;
                    case "start":
                        ParseInteger(attributeValue, out startIndex);
                        break;
                    case "end":
                        ParseInteger(attributeValue, out endIndex);
                        break;
                    case "label":
                        if (!WorkingSet.GetLabeledToolStudyItemRange(attributeValue, CurrentHostLanguageID, out startIndex, out endIndex))
                        {
                            HandleElementError(element, "(Item label not found in element \"" + element.Name.LocalName + "\": \"" + attributeValue + "\")");
                            return false;
                        }
                        break;
                    case "tag":
                        if (!WorkingSet.GetTaggedToolStudyItemRange(attributeValue, out startIndex, out endIndex))
                        {
                            HandleElementError(element, "(Item tag not found in element \"" + element.Name.LocalName + "\": \"" + attributeValue + "\")");
                            return false;
                        }
                        break;
                    case "all":
                        startIndex = 0;
                        endIndex = WorkingSet.ToolStudyItemCount();
                        break;
                    case "language":
                        ParseLanguageID(attributeValue, out languageID);
                        break;
                    case "speed":
                        speed = attributeValue.ToLower();
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            if (startIndex == -1)
            {
                if (StudyItem == null)
                    StudyItem = WorkingSet.GetToolStudyItemIndexed(0).StudyItem;

                if (StudyItem == null)
                    return true;

                return HandleMultiLanguageItem(element, StudyItem, languageID, speed);
            }
            else
            {
                if (endIndex < startIndex)
                    endIndex = startIndex + 1;

                for (int index = startIndex; index < endIndex; index++)
                {
                    MultiLanguageItem multiLanguageItem = WorkingSet.GetToolStudyItemIndexed(index).StudyItem;

                    if (multiLanguageItem == null)
                        continue;

                    if (!HandleMultiLanguageItem(element, multiLanguageItem, languageID, speed))
                        return false;
                }
            }

            returnValue = HandleElementChildren(element) && returnValue;

            return returnValue;
        }

        protected override bool HandleEach(XElement element)
        {
            string contentKey = null;
            string tag = String.Empty;
            string label = String.Empty;
            LanguageID languageID = TargetLanguageID;
            SelectorAlgorithmCode selector = SelectorAlgorithmCode.Forward;
            ToolSelectorMode mode = ToolSelectorMode.Normal;
            int newLimit = ToolProfile.DefaultNewLimit;
            int reviewLimit = ToolProfile.DefaultReviewLimit;
            bool isRandomUnique = false;
            bool isRandomNew = false;
            bool isAdaptiveMixNew = false;
            int chunkSize = ToolProfile.DefaultChunkSize;
            int level = ToolProfile.DefaultReviewLevel;
            string profileName = DefaultProfileName;
            string configurationKey = DefaultConfigurationName;
            int totalCount = -1;
            ContentStudyList savedWorkingSetStudyList;
            ToolStudyList savedWorkingSet;
            ToolStudyItem entry;
            ToolItemStatus status;
            bool returnValue = true;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "content":
                    case "componentkey":
                        contentKey = attributeValue;
                        break;
                    case "tag":
                        tag = attributeValue;
                        break;
                    case "label":
                        label = attributeValue;
                        break;
                    case "language":
                        ParseLanguageID(attributeValue, out languageID);
                        break;
                    case "selector":
                        try
                        {
                            selector = ToolProfile.GetSelectorAlgorithmCodeFromString(attributeValue);
                        }
                        catch (Exception exception)
                        {
                            HandleElementError(element, exception.Message);
                            return false;
                        }
                        break;
                    case "mode":
                        try
                        {
                            mode = ToolItemSelector.GetToolSelectorModeFromString(attributeValue);
                        }
                        catch (Exception exception)
                        {
                            HandleElementError(element, exception.Message);
                            return false;
                        }
                        break;
                    case "level":
                        ParseInteger(attributeValue, out level);
                        break;
                    case "profile":
                        profileName = attributeValue;
                        break;
                    case "configuration":
                        configurationKey = attributeValue;
                        break;
                    case "count":
                        ParseInteger(attributeValue, out totalCount);
                        break;
                    case "newLimit":
                        ParseInteger(attributeValue, out newLimit);
                        break;
                    case "reviewLimit":
                        ParseInteger(attributeValue, out reviewLimit);
                        break;
                    case "randomunique":
                        ParseBoolean(attributeValue, out isRandomUnique);
                        break;
                    case "randomnew":
                        ParseBoolean(attributeValue, out isRandomNew);
                        break;
                    case "adaptivemixnew":
                        ParseBoolean(attributeValue, out isAdaptiveMixNew);
                        break;
                    case "chunksize":
                        ParseInteger(attributeValue, out chunkSize);
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            PushWorkingSet(
                contentKey,
                tag,
                label,
                selector,
                mode,
                newLimit,
                reviewLimit,
                isRandomUnique,
                isRandomNew,
                isAdaptiveMixNew,
                chunkSize,
                level,
                profileName,
                ref configurationKey,
                out savedWorkingSetStudyList,
                out savedWorkingSet);

            int count = WorkingSet.ToolStudyItemCount();
            int index = 0;

            if (totalCount == -1)
                totalCount = count;

            for (; index < totalCount; index++)
            {
                entry = WorkingSet.GetToolStudyItemIndexed(Selector.CurrentIndex);

                if (entry == null)
                    break;

                StudyItem = entry.StudyItem;
                status = entry.GetStatus(Configuration.Key);
                ToolProfile toolProfile = Configuration.Profile;
                toolProfile.TouchApplyGrade(
                    status,
                    1.0f,
                    CurrentTime,
                    Configuration);

                UpdateCurrentTime();

                returnValue = HandleElementChildren(element) && returnValue;

                if (!Selector.SetNextIndex())
                    break;
            }

            PopWorkingSet(savedWorkingSetStudyList, savedWorkingSet);

            return returnValue;
        }

        protected override bool HandleDuration(XElement element)
        {
            string contentKey = null;
            string tag = String.Empty;
            string label = String.Empty;
            SelectorAlgorithmCode selector = SelectorAlgorithmCode.Forward;
            ToolSelectorMode mode = ToolSelectorMode.Normal;
            bool isRandomUnique = false;
            bool isRandomNew = false;
            bool isAdaptiveMixNew = false;
            int chunkSize = ToolProfile.DefaultChunkSize;
            int level = ToolProfile.DefaultReviewLevel;
            string profileName = DefaultProfileName;
            string configurationKey = DefaultConfigurationName;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            bool stopAtEnd = false;
            LanguageID languageID = TargetLanguageID;
            TimeSpan duration;
            DateTime endTime;
            ContentStudyList savedWorkingSetStudyList;
            ToolStudyList savedWorkingSet;
            ToolStudyItem entry;
            ToolItemStatus status;
            bool returnValue = true;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "content":
                    case "componentkey":
                        contentKey = attributeValue;
                        break;
                    case "tag":
                        tag = attributeValue;
                        break;
                    case "label":
                        label = attributeValue;
                        break;
                    case "language":
                        ParseLanguageID(attributeValue, out languageID);
                        break;
                    case "selector":
                        try
                        {
                            selector = ToolProfile.GetSelectorAlgorithmCodeFromString(attributeValue);
                        }
                        catch (Exception exception)
                        {
                            HandleElementError(element, exception.Message);
                            return false;
                        }
                        break;
                    case "mode":
                        try
                        {
                            mode = ToolItemSelector.GetToolSelectorModeFromString(attributeValue);
                        }
                        catch (Exception exception)
                        {
                            HandleElementError(element, exception.Message);
                            return false;
                        }
                        break;
                    case "randomunique":
                        ParseBoolean(attributeValue, out isRandomUnique);
                        break;
                    case "randomnew":
                        ParseBoolean(attributeValue, out isRandomNew);
                        break;
                    case "adaptivemixnew":
                        ParseBoolean(attributeValue, out isAdaptiveMixNew);
                        break;
                    case "chunksize":
                        ParseInteger(attributeValue, out chunkSize);
                        break;
                    case "level":
                        ParseInteger(attributeValue, out level);
                        break;
                    case "profile":
                        profileName = attributeValue;
                        break;
                    case "configuration":
                        configurationKey = attributeValue;
                        break;
                    case "hours":
                        ParseInteger(attributeValue, out hours);
                        break;
                    case "minutes":
                        ParseInteger(attributeValue, out minutes);
                        break;
                    case "seconds":
                        ParseInteger(attributeValue, out seconds);
                        break;
                    case "stopatend":
                        stopAtEnd = (attributeValue == "true" ? true : false);
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            duration = new TimeSpan(hours, minutes, seconds);
            endTime = CurrentTime + duration;

            PushWorkingSet(
                contentKey,
                tag,
                label,
                selector,
                mode,
                ToolProfile.DefaultNewLimit,
                ToolProfile.DefaultReviewLimit,
                isRandomUnique,
                isRandomNew,
                isAdaptiveMixNew,
                chunkSize,
                level,
                profileName,
                ref configurationKey,
                out savedWorkingSetStudyList,
                out savedWorkingSet);

            while (CurrentTime < endTime)
            {
                entry = WorkingSet.GetToolStudyItemIndexed(Selector.CurrentIndex);

                if (entry == null)
                    break;

                StudyItem = entry.StudyItem;

                if (Configuration == null)
                    SetUpDefaultConfiguration();

                status = entry.GetStatus(Configuration.Key);
                ToolProfile toolProfile = Configuration.Profile;
                toolProfile.TouchApplyGrade(
                    status,
                    1.0f,
                    CurrentTime,
                    Configuration);

                returnValue = HandleElementChildren(element) && returnValue;

                if (!Selector.SetNextIndex() && stopAtEnd)
                    break;
            }

            PopWorkingSet(savedWorkingSetStudyList, savedWorkingSet);

            return returnValue;
        }

        protected override bool HandlePlay(XElement element)
        {
            LanguageID overriddenTargetLanguageID = null;
            LanguageID overriddenHostLanguageID = null;
            LanguageID mediaLanguageID = null;
            MultiLanguageItem variableValue = null;
            string contentKey = null;
            string subType = null;
            string subSubType = null;
            string errorMessage = null;
            string tag = String.Empty;
            int itemIndex = -1;
            int itemCount = -1;
            int sentenceIndex = -1;
            bool isView = false;
            string speed = "Normal";
            List<BaseString> optionsList = null;
            bool nodeWasReplaced = false;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "content":
                        ParseContent(attributeValue, overriddenTargetLanguageID, out contentKey,
                            out itemIndex, out itemCount, out sentenceIndex, out tag, out subType, out subSubType,
                            out overriddenTargetLanguageID, out optionsList, out errorMessage);
                        break;
                    case "target":
                    case "targetlanguage":
                    case "language":
                        ParseLanguageID(attributeValue, out overriddenTargetLanguageID);
                        if (mediaLanguageID == null)
                            mediaLanguageID = overriddenTargetLanguageID.MediaLanguageID();
                        break;
                    case "host":
                    case "hostlanguage":
                        ParseLanguageID(attributeValue, out overriddenHostLanguageID);
                        break;
                    case "studyitemindex":
                    case "itemindex":
                    case "index":   // Legacy
                        ParseInteger(attributeValue, out itemIndex);
                        if ((itemIndex != -1) && (itemCount == -1))
                            itemCount = 1;
                        break;
                    case "itemcount":
                        ParseInteger(attributeValue, out itemCount);
                        break;
                    case "sentenceindex":
                        ParseInteger(attributeValue, out sentenceIndex);
                        break;
                    case "tag":
                        tag = attributeValue;
                        break;
                    case "name":
                        ParseVariableName(attributeValue, out variableValue);
                        break;
                    case "view":
                        ParseBoolean(attributeValue, out isView);
                        break;
                    case "speed":
                        speed = GetSpeedKeyFromString(attributeValue);
                        if (!ParseRenderOption(attribute.Name.LocalName, speed, ref optionsList))
                        {
                            HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                            return false;
                        }
                        break;
                    case "options":
                        optionsList = BaseString.GetBaseStringListFromString(attributeValue);
                        break;
                    case "elementtype":
                    case "class":
                    case "style":
                    case "languageformat":
                    case "rowformat":
                    case "displayformat":
                    case "showannotations":
                    case "useaudio":
                    case "usepicture":
                    case "usemedia":
                    case "autoplay":
                    case "player":
                    case "noplayer":
                    case "playerid":
                    case "showvolume":
                    case "showtimeslider":
                    case "showtimetext":
                    case "endofmedia":
                    default:
                        if (!ParseRenderOption(attribute.Name.LocalName, attributeValue, ref optionsList))
                        {
                            HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                            return false;
                        }
                        break;
                }
            }

            if (errorMessage != null)
                HandleElementError(element, errorMessage);
            else if (variableValue != null)
            {
            }
            else if (contentKey != null)
            {
                BaseObjectNodeTree tree = SourceTree;
                BaseObjectNode node = SourceNode;
                BaseObjectContent content;
                if ((node == tree) || (node == null))
                {
                    node = null;
                    content = tree.GetContent(contentKey);
                }
                else
                    content = node.GetContent(contentKey);
                if (content == null)
                {
                    HandleElementError(element, "Content " + contentKey + " not found.");
                    return nodeWasReplaced;
                }
                int treeKey = tree.KeyInt;
                int nodeKey = (node != null ? node.KeyInt : -1);
                ContentMediaItem mediaItem = content.ContentStorageMediaItem;
                if (mediaItem != null)
                {
                    LanguageMediaItem languageMediaItem = null;
                    if (overriddenTargetLanguageID == null)
                        overriddenTargetLanguageID = TargetLanguageID;
                    if (overriddenHostLanguageID == null)
                        overriddenHostLanguageID = HostLanguageID;
                    languageMediaItem = mediaItem.GetLanguageMediaItemWithLanguages(
                        overriddenTargetLanguageID, overriddenHostLanguageID);
                    if (languageMediaItem == null)
                        languageMediaItem = mediaItem.GetLanguageMediaItemWithLanguages(
                            overriddenTargetLanguageID, null);
                    if (languageMediaItem != null)
                    {
                        MediaDescription mediaDescription = languageMediaItem.GetMediaDescriptionIndexed(0);
                        if (mediaDescription != null)
                        {
                            string mediaUrl = mediaDescription.GetContentUrl(content.MediaTildeUrl);
                            bool isAudio = (content.ContentType == "Audio" ? true : false);
                            string message = null;

                            if (!AppendMediaItem(content, mediaItem,
                                    languageMediaItem, mediaDescription, mediaUrl,
                                    "Normal", speed, out message))
                                HandleElementError(message);
                        }
                        else
                            HandleElementError(element, "Media description for " + contentKey + "not found.");
                    }
                    else
                        HandleElementError(element, "Media item " + contentKey + " does not support the target language.");
                }
                else
                    HandleElementError(element, "Media item " + contentKey + "not found.");
            }
            else
            {
                XText textNode = new XText("(invalid play element: \"" + element.ToString() + "\")");
                element.ReplaceWith(textNode);
            }

            return nodeWasReplaced;
        }

        protected override XNode RenderContentView(ContentRenderParameters parameters)
        {
            XNode returnValue = null;

            switch (parameters.Content.ContentClass)
            {
                default:
                case ContentClassType.None:
                    break;
                case ContentClassType.DocumentItem:
                    break;
                case ContentClassType.MediaList:
                    break;
                case ContentClassType.MediaItem:
                    returnValue = HandleAppendMediaItem(
                        parameters, parameters.Content.ContentStorageMediaItem);
                    break;
                case ContentClassType.StudyList:
                    break;
            }

            if (returnValue != null)
                return returnValue;

            return HandleElementError("Not implemented: RenderContentView content class "
                + parameters.Content.ContentClass.ToString());
        }

        protected XNode HandleAppendMediaItem(ContentRenderParameters parameters, ContentMediaItem mediaItem)
        {
            BaseObjectContent mediaItemContent = mediaItem.Content;
            LanguageMediaItem languageMediaItem = mediaItem.GetLanguageMediaItemWithLanguages(
                TargetLanguageID, HostLanguageID);

            if (languageMediaItem == null)
                languageMediaItem = mediaItem.GetLanguageMediaItemWithLanguages(
                    TargetLanguageID, UILanguageID);

            if (languageMediaItem == null)
                languageMediaItem = mediaItem.GetLanguageMediaItemWithLanguages(
                    TargetLanguageID, null);

            string label = mediaItem.Content.GetContentMessageLabel(UILanguageID);

            if (languageMediaItem == null)
                return HandleElementError("Media item doesn't have the given language: " + label);

            MediaDescription mediaDescription = languageMediaItem.GetMediaDescriptionIndexed(0);

            if (mediaDescription == null)
                return HandleElementError("Language media item doesn't have any media descriptions: " + label);

            string fileUrl = mediaDescription.GetUrl(mediaItem.MediaTildeUrl);
            string filePath = ApplicationData.MapToFilePath(fileUrl);

            if (!FileSingleton.Exists(filePath))
                return HandleElementError("Media file not uploaded yet: " + label);

            string message;

            DateTime startTime = CurrentTime;
            WaveAudioBuffer waveData = MediaConvertSingleton.Mp3Decoding(filePath, out message);

            //if (GetSpeedKeyFromString(renderSpeed) == "Slow")
            //    waveData.SpeedChange(GetSpeedMultiplier(renderSpeed));

            if (waveData != null)
            {
                if (AppendAudio(null, "", UILanguageID, waveData, out message))
                {
                    if (mediaItem.SourceContentKeys != null)
                    {
                        if (MediaItem.SourceContentKeys == null)
                            MediaItem.SourceContentKeys = new List<string>(mediaItem.SourceContentKeys);
                        else
                        {
                            foreach (string sourceContentKey in mediaItem.SourceContentKeys)
                            {
                                if (!MediaItem.SourceContentKeys.Contains(sourceContentKey))
                                    MediaItem.SourceContentKeys.Add(sourceContentKey);
                            }
                        }
                    }
                    CreateMediaItemMediaRuns(mediaItem, mediaItemContent.KeyString,
                        languageMediaItem.KeyString, startTime);
                    return HandleElementMessage("(" + label + " inserted.)");
                }
                else
                    return HandleElementError(message);
            }
            else
                return HandleElementError(message);
        }

        protected bool HandleMultiLanguageItem(XElement element,
            MultiLanguageItem multiLanguageItem, LanguageID languageID, string speed)
        {
            LanguageID mediaLanguageID = languageID.MediaLanguageID();
            DateTime saveCurrentTime = CurrentTime;
            string message;

            if (Content == null)
                return false;

            if (multiLanguageItem == null)
                return false;

            string mediaDirectoryUrl;

            if (multiLanguageItem.Content != null)
                mediaDirectoryUrl = multiLanguageItem.Content.MediaTildeUrl;
            else
                mediaDirectoryUrl = Content.MediaTildeUrl;

            LanguageItem languageItem = multiLanguageItem.LanguageItem(languageID);

            if (languageItem == null)
                return true;

            if (!languageItem.HasText())
                return true;

            //languageItem.DeleteMediaRunsWithReferenceKey(MediaContent.KeyString);

            speed = GetSpeedKeyFromString(speed);

            int sentenceCount = languageItem.SentenceRuns.Count();
            string contentSpeed = "Normal";
            string renderSpeed = "Normal";

            LastGeneratedMediaRun = null;

            if ((languageItem.SentenceRuns != null) && (sentenceCount != 0))
            {
                int sentenceIndex = 0;

                foreach (TextRun sentenceRun in languageItem.SentenceRuns)
                {
                    string sentenceText = languageItem.GetRunText(sentenceRun);
                    MediaRun mediaRun = null;
                    MultiLanguageItem transcriptSpeakerText = new MultiLanguageItem();

                    if (IsGenerateLocalTranscript)
                        MediaItem.CloneLocalSpeakerNameCheck(multiLanguageItem);

                    transcriptSpeakerText.SpeakerNameKey = multiLanguageItem.SpeakerNameKey;

                    if (speed == "Normal")
                        mediaRun = sentenceRun.GetMediaRun("Audio");
                    else if (speed == "Slow")
                    {
                        contentSpeed = "Slow";
                        mediaRun = sentenceRun.GetMediaRun("SlowAudio");

                        if (mediaRun == null)
                        {
                            mediaRun = sentenceRun.GetMediaRun("Audio");
                            renderSpeed = "Slow";
                        }
                    }

                    string text = languageItem.GetRunText(sentenceRun);
                    TextRun transcriptTextRun = new TextRun(0, sentenceRun.Length, null);
                    LanguageItem transcriptLanguageItem = new LanguageItem(null, CurrentHostLanguageID, text,
                        new List<TextRun>(1) { transcriptTextRun }, null);
                    transcriptSpeakerText.Add(transcriptLanguageItem);

                    if (mediaRun != null)
                    {
                        if (mediaRun.IsReference)
                        {
                            string mediaItemKey = mediaRun.MediaItemKey;
                            string languageMediaItemKey = mediaRun.LanguageMediaItemKey;
                            ContentMediaItem mediaItem = Node.GetMediaItem(mediaItemKey);

                            if (mediaItem != null)
                            {
                                MediaDescription mediaDescription = mediaItem.GetMediaDescriptionIndexed(
                                    languageMediaItemKey, 0);

                                if (mediaDescription != null)
                                {
                                    string url = mediaDescription.GetUrl(mediaItem.MediaTildeUrl);

                                    if (String.IsNullOrEmpty(url))
                                    {
                                        HandleElementError(element, "Media reference media run has no media reference: " + sentenceText);
                                        return false;
                                    }

                                    if (!AppendFileAudioSegment(transcriptSpeakerText, sentenceText,
                                        languageID, url, mediaRun.Start, mediaRun.Length, contentSpeed, renderSpeed, out message))
                                    {
                                        HandleElementError(element, "Error appending item: " + message);
                                        return false;
                                    }
                                }
                                else
                                {
                                    HandleElementError(element, "Media item has no file: " + mediaItemKey + " " + languageMediaItemKey);
                                    return false;
                                }
                            }
                            else
                            {
                                HandleElementError(element, "Media reference media item not found: " + mediaItemKey);
                                return false;
                            }
                        }
                        else
                        {
                            string url = mediaRun.GetUrl(mediaDirectoryUrl);
                            DateTime saveStart = CurrentTime;
                            TimeSpan start = CurrentTime - StartTime;

                            if (AppendFileAudio(transcriptSpeakerText, sentenceText, languageID, url,
                                contentSpeed, renderSpeed, out message))
                            {
                                multiLanguageItem.PropogateMediaRun(mediaLanguageID, sentenceIndex, mediaRun);
                                TimeSpan length = CurrentTime - saveStart;
                                MediaRun newMediaRun = CreateSentenceRunReferenceMediaRun(
                                    sentenceRun, mediaRun.KeyString, start, length);
                                if (newMediaRun != null)
                                    multiLanguageItem.PropogateMediaRunUnconditional(mediaLanguageID, sentenceIndex, newMediaRun);
                            }
                            else
                            {
                                HandleElementError(element, "Error appending item: " + message);
                                return false;
                            }
                        }
                    }
                    else
                        OutputNewSentenceMediaRun(multiLanguageItem, languageItem, sentenceRun, sentenceIndex,
                            text, multiLanguageItem.SpeakerNameKey, languageID,
                            contentSpeed, renderSpeed, transcriptSpeakerText, out message);

                    sentenceIndex++;
                }
            }
            else
            {
                string text = languageItem.Text;
                MultiLanguageItem transcriptSpeakerText = new MultiLanguageItem();
                TextRun transcriptTextRun = new TextRun(0, languageItem.Text.Length, null);
                LanguageItem transcriptLanguageItem = new LanguageItem(null, languageItem.LanguageID, text,
                    new List<TextRun>(1) { transcriptTextRun }, null);

                transcriptSpeakerText.Add(transcriptLanguageItem);

                OutputNewSentenceMediaRun(multiLanguageItem, languageItem, null, 0,
                    text, multiLanguageItem.SpeakerNameKey, languageID,
                    contentSpeed, speed, transcriptSpeakerText, out message);
            }

            LastItemTime = CurrentTime - saveCurrentTime;

            return true;
        }

        // Returns true if node was replaced.
        protected override bool HandleTextNode(XText node)
        {
            string text = node.Value.Trim();
            bool nodeWasReplaced = false;

            if (!String.IsNullOrEmpty(text))
            {
                if (HandleText(text, CurrentSpeed))
                    nodeWasReplaced = true;
            }

            return nodeWasReplaced;
        }

        protected bool HandleText(string text, string speed)
        {
            List<string> sentences = TextUtilities.ParseSentencesFromRawText(text);
            bool returnValue = true;

            foreach (string sentence in sentences)
            {
                if (!HandleSentence(null, sentence, null, CurrentHostLanguageID, speed))
                {
                    returnValue = false;
                    break;
                }
            }

            return returnValue;
        }

        protected bool HandleSentence(MultiLanguageItem transcriptSpeakerText, string sentence, string speakerKey, LanguageID languageID, string speed)
        {
            bool returnValue = true;
            MultiLanguageItem multiLanguageItem = null;
            LanguageItem languageItem = null;
            TextRun sentenceRun = null;
            MediaRun sentenceMediaRun = null;
            string sentenceText = sentence;
            string message;

            if (sentence.Contains("$(") || sentence.Contains("{"))
            {
                int count = sentence.Length;
                int index;
                int eindex;
                StringBuilder sb = new StringBuilder();
                string fullReferenceString;
                string referenceString;
                string variableName = null;
                string contentKey = null;
                int itemIndex = -1;
                int itemCount = -1;
                int sentenceIndex = -1;
                string tag = String.Empty;
                string subType = null;
                string subSubType = null;
                string errorMessage = null;
                LanguageID hostLanguageID = UILanguageID;
                LanguageID mediaLanguageID = null;
                List<BaseString> optionsList = null;
                string text;
                XNode subNode;

                if ((languageID == null) || String.IsNullOrEmpty(languageID.LanguageCultureExtensionCode))
                    languageID = hostLanguageID;

                LanguageID tmpLanguageID;
                LanguageID currentLanguageID = languageID;

                for (index = 0; index < count; index++)
                {
                    if ((sentence[index] == '\\') && ((sentence[index + 1] == '{') || ((sentence[index + 1] == '$') && (sentence[index + 2] == '('))))
                    {
                        sb.Append(sentence[index + 1]);

                        if (sentence[index + 2] == '(')
                            sb.Append('(');
                    }
                    else if (sentence[index] == '{')
                    {
                        int i = index + 1;
                        int e = i;

                        if (sb.Length != 0)
                        {
                            sentenceText = sb.ToString();
                            returnValue = OutputSentence(sentenceText, speakerKey, languageID, speed, transcriptSpeakerText) && returnValue;
                            sentenceText = String.Empty;
                            sb.Clear();
                        }

                        while (e < count)
                        {
                            if (sentence[e] == '}')
                            {
                                text = variableName = sentence.Substring(i, e - i);
                                variableName = MarkupTemplate.FilterVariableName(variableName);
                                multiLanguageItem = MarkupTemplate.MultiLanguageItem(variableName);

                                if (multiLanguageItem != null)
                                {
                                    languageItem = multiLanguageItem.LanguageItem(UILanguageID);

                                    if ((languageItem == null) && (multiLanguageItem.LanguageItems != null))
                                        languageItem = multiLanguageItem.LanguageItem(0);

                                    if (languageItem != null)
                                    {
                                        if (languageItem.HasSentenceRuns())
                                        {
                                            sentenceRun = languageItem.SentenceRuns[0];

                                            if (sentenceRun != null)
                                            {
                                                sentenceMediaRun = sentenceRun.GetMediaRun("Audio");

                                                if (sentenceMediaRun != null)
                                                {
                                                    string fileUrl = sentenceMediaRun.GetUrl(MarkupMediaDirectoryUrl);

                                                    if (!AppendFileAudio(transcriptSpeakerText, languageItem.Text, languageID,
                                                        fileUrl, speed, speed, out message))
                                                    {
                                                        sb.Append("Error appending item: " + message);
                                                        returnValue = false;
                                                    }

                                                    break;
                                                }
                                            }
                                        }

                                        text = languageItem.Text;
                                    }
                                    else
                                        text = String.Empty;
                                }
                                else
                                {
                                    BaseString variable = GetVariable(variableName);

                                    if (variable != null)
                                        text = variable.Text;
                                }

                                if (text != null)
                                    sb.Append(text);

                                break;
                            }

                            e++;
                        }

                        if (variableName == null)
                            sb.Append("(unmatched '}')");

                        index = e;
                    }
                    else if ((sentence[index] == '$') && (index < (count - 1)) && (sentence[index + 1] == '('))
                    {
                        for (eindex = index + 2; eindex < count; eindex++)
                        {
                            if (sentence[eindex] == '(')
                            {
                                for (eindex = eindex + 1; eindex < count; eindex++)
                                {
                                    if (sentence[eindex] == ')')
                                        break;
                                }
                            }
                            else if (sentence[eindex] == ')')
                            {
                                fullReferenceString = sentence.Substring(index, eindex - index + 1);
                                referenceString = sentence.Substring(index + 2, eindex - (index + 2));

                                ParseReference(referenceString, null,
                                    out variableName, out contentKey, out itemIndex, out itemCount, out sentenceIndex, out tag,
                                    out subType, out subSubType, out tmpLanguageID, out optionsList, out errorMessage);

                                if (tmpLanguageID == null)
                                    tmpLanguageID = languageID;
                                else if (tmpLanguageID != languageID)
                                {
                                    if (sb.Length != 0)
                                    {
                                        sentenceText = sb.ToString();
                                        returnValue = OutputSentence(sentenceText, speakerKey, currentLanguageID, speed, transcriptSpeakerText) && returnValue;
                                        sentenceText = String.Empty;
                                        sb.Clear();
                                    }
                                }

                                currentLanguageID = tmpLanguageID;

                                if (errorMessage != null)
                                    sb.Append("(Error parsing reference: \"" + errorMessage + "\")");
                                else if (contentKey != null)
                                {
                                    if (!SubstituteKeyWord(contentKey, itemIndex, itemCount, sentenceIndex, subType, languageID, hostLanguageID, optionsList, sb))
                                    {
                                        ContentRenderParameters contentRenderParameters = new ContentRenderParameters(
                                            SourceTree, SourceNode, contentKey, itemIndex, itemCount, sentenceIndex, tag,
                                            languageID, mediaLanguageID, subType, subSubType, optionsList,
                                            UseAudio, UsePicture, "1", true, "Stop", String.Empty);

                                        subNode = HandleContent(fullReferenceString, contentRenderParameters);

                                        if (subNode != null)
                                            ExtractTextFromNode(sb, subNode);
                                        else
                                            sb.Append("(error rendering content \"" + contentKey + "\" from: " + referenceString + "\")");
                                    }
                                }
                                else if (variableName != null)
                                {
                                    multiLanguageItem = MarkupTemplate.MultiLanguageItem(variableName);

                                    if (multiLanguageItem != null)
                                    {
                                        languageItem = multiLanguageItem.LanguageItem(currentLanguageID);

                                        if (languageItem != null)
                                        {
                                            if (languageItem.HasSentenceRuns())
                                            {
                                                sentenceRun = languageItem.SentenceRuns[0];

                                                if (sentenceRun != null)
                                                {
                                                    sentenceMediaRun = sentenceRun.GetMediaRun("Audio");

                                                    if (sentenceMediaRun != null)
                                                    {
                                                        string fileUrl = sentenceMediaRun.GetUrl(MarkupMediaDirectoryUrl);

                                                        if (sb.Length != 0)
                                                        {
                                                            sentenceText = sb.ToString();
                                                            returnValue = OutputSentence(sentenceText, speakerKey, currentLanguageID, speed, transcriptSpeakerText) && returnValue;
                                                            sentenceText = String.Empty;
                                                            sb.Clear();
                                                        }

                                                        if (AppendFileAudio(transcriptSpeakerText, languageItem.Text, currentLanguageID,
                                                                fileUrl, speed, speed, out message))
                                                            text = null;
                                                        else
                                                        {
                                                            text = "Error appending item: " + message;
                                                            returnValue = false;
                                                        }
                                                    }
                                                    else
                                                        text = languageItem.Text;
                                                }
                                                else
                                                    text = languageItem.Text;
                                            }
                                            else
                                                text = languageItem.Text;
                                        }
                                        else if (multiLanguageItem.LanguageItems != null)
                                            text = "(no translation: \"" + multiLanguageItem.LanguageItem(0).Text + "\")";
                                        else
                                            text = "(no translation)";
                                    }
                                    else
                                    {
                                        BaseString variable = GetVariable(variableName);

                                        if (variable != null)
                                            text = variable.Text;
                                        else
                                            text = variableName;
                                    }

                                    if (!String.IsNullOrEmpty(text))
                                        sb.Append(text);
                                }

                                index = eindex;
                                break;
                            }
                        }
                    }
                    else
                        sb.Append(sentence[index]);
                }

                sentenceText = sb.ToString();
            }

            if (!String.IsNullOrEmpty(sentenceText))
                returnValue = OutputSentence(sentenceText, speakerKey, languageID, speed, transcriptSpeakerText) && returnValue;

            return returnValue;
        }

        protected bool OutputSentence(string sentenceText, string speakerKey, LanguageID languageID, string speed,
            MultiLanguageItem transcriptSpeakerText)
        {
            MultiLanguageItem multiLanguageItem = null;
            LanguageItem languageItem = null;
            TextRun sentenceRun = null;
            MediaRun sentenceMediaRun = null;
            List<string> sentences;
            bool returnValue = true;

            if (sentenceText.Contains("\n"))
                sentences = sentenceText.Split(newlines, StringSplitOptions.RemoveEmptyEntries).ToList();
            else
                sentences = new List<string>(1) { sentenceText };

            foreach (string str in sentences)
            {
                sentenceMediaRun = null;
                languageItem = null;
                sentenceRun = null;

                if (MarkupTemplate != null)
                {
                    string variableName = MarkupTemplate.FilterVariableName(str);
                    multiLanguageItem = MarkupTemplate.MultiLanguageItem(variableName);
                }

                if (multiLanguageItem != null)
                {
                    languageItem = multiLanguageItem.LanguageItem(languageID);

                    if (languageItem != null)
                    {
                        if (languageItem.HasSentenceRuns())
                        {
                            sentenceRun = languageItem.SentenceRuns.First();
                            MediaRun mediaRun = sentenceRun.GetMediaRun("Audio");

                            if (mediaRun != null)
                            {
                                if (!mediaRun.IsReference)
                                    sentenceMediaRun = new MediaRun(mediaRun);
                            }
                        }
                    }
                }

                if (sentenceMediaRun == null)
                {
                    sentenceMediaRun = CreateSentenceMediaRun(str, speakerKey, languageID, speed);

                    if (sentenceMediaRun == null)
                        return false;

                    if (languageItem != null)
                    {
                        if (sentenceRun == null)
                        {
                            sentenceRun = new TextRun(languageItem, sentenceMediaRun);
                            languageItem.SentenceRuns = new List<TextRun>(1) { sentenceRun };
                        }
                        else
                            sentenceRun.MediaRuns = new List<MediaRun>(1) { sentenceMediaRun };
                    }
                }

                if (!CreateTranscriptMediaRun(transcriptSpeakerText, sentenceText, sentenceMediaRun, languageID, speed))
                    return false;
            }

            return returnValue;
        }

        protected bool OutputNewSentenceMediaRun(
            MultiLanguageItem multiLanguageItem, LanguageItem languageItem, TextRun sentenceRun,
            int sentenceRunIndex, string sentenceText, string speakerKey, LanguageID languageID,
            string contentSpeed, string renderSpeed,
            MultiLanguageItem transcriptSpeakerText, out string message)
        {
            MediaRun sentenceMediaRun = null;
            string mediaDirectoryUrl = multiLanguageItem.MediaTildeUrl;
            DateTime saveStart = CurrentTime;
            TimeSpan start = CurrentTime - StartTime;
            bool returnValue = true;

            if (languageItem == null)
            {
                languageItem = new LanguageItem(multiLanguageItem.Key, languageID, sentenceText);
                multiLanguageItem.Add(languageItem);
            }

            VoiceCheck(multiLanguageItem.SpeakerNameKey, languageID, contentSpeed);

            sentenceMediaRun = CreateNewSentenceMediaRun(multiLanguageItem.KeyString,
                sentenceRunIndex, sentenceText, languageID, speakerKey, contentSpeed, mediaDirectoryUrl);

            if (sentenceRun == null)
            {
                sentenceRun = new Content.TextRun(languageItem, sentenceMediaRun);
                languageItem.AddSentenceRun(sentenceRun);
            }
            else
                sentenceRun.AddMediaRun(sentenceMediaRun);

            LanguageID mediaLanguageID = languageID.MediaLanguageID();
            multiLanguageItem.PropogateMediaRun(mediaLanguageID, sentenceRunIndex, sentenceMediaRun);

            string url = sentenceMediaRun.GetUrl(mediaDirectoryUrl);

            if (AppendFileAudio(transcriptSpeakerText, sentenceText, languageID, url,
                    contentSpeed, renderSpeed, out message))
            {
                TimeSpan fudge = new TimeSpan((long)(CurrentTime.Ticks * 0.0015));
                TimeSpan length = CurrentTime - saveStart;
                TimeSpan fudgedStart = start - fudge;
                MediaRun newMediaRun = CreateSentenceRunReferenceMediaRun(sentenceRun, sentenceMediaRun.KeyString, fudgedStart, length);
                if (newMediaRun != null)
                    multiLanguageItem.PropogateMediaRun(mediaLanguageID, sentenceRunIndex, newMediaRun);
            }
            else
                returnValue = false;

            return returnValue;
        }

        protected MediaRun CreateNewSentenceMediaRun(string studyItemKey, int sentenceRunIndex,
            string sentenceText, LanguageID languageID, string speakerKey, string speed, string mediaDirectoryUrl)
        {
            string fileName = MediaUtilities.ComposeStudyItemFileName(
                studyItemKey, sentenceRunIndex, languageID, "Audio", ".mp3");
            string urlPath = MediaUtilities.ConcatenateUrlPath(mediaDirectoryUrl, fileName);
            string filePath = ApplicationData.MapToFilePath(urlPath);
            TimeSpan length = new TimeSpan(0);
            string mimeType = "audio/mpeg3";
            string message = String.Empty;
            string mediaRunKey = (GetSpeedKeyFromString(speed) == "Slow" ? "SlowAudio" : "Audio");
            MediaRun mediaRun = new MediaRun(
                mediaRunKey,
                fileName,
                null,
                null,
                MediaItem.Owner,
                TimeSpan.Zero,
                TimeSpan.Zero);

            if (FileSingleton.Exists(filePath))
                FileSingleton.Delete(filePath);

            FileSingleton.DirectoryExistsCheck(filePath);

            sentenceText = MediaUtilities.FilterTextBeforeSpeech(sentenceText, languageID, UserProfile, FilterAsides);

            VoiceCheck(speakerKey, languageID, speed);

            if (String.IsNullOrEmpty(sentenceText))
                return mediaRun;

            if (!SpeechEngine.SpeakToFile(sentenceText, filePath, mimeType, out message))
            {
                Error = message;
                return mediaRun;
            }

            return mediaRun;
        }

        protected MediaRun CreateSentenceRunReferenceMediaRun(TextRun sentenceRun, string mediaRunKey,
            TimeSpan start, TimeSpan length)
        {
            MediaRun mediaRun = null;

            if (TranscriptStudyList == null)
            {
                mediaRun = new MediaRun(
                    mediaRunKey,
                    null,
                    MediaContent.KeyString,
                    LanguageMediaItem.KeyString,
                    MediaItem.Owner,
                    start,
                    length);

                sentenceRun.AddMediaRun(mediaRun);
            }

            return mediaRun;
        }

        protected void CreateMediaItemMediaRuns(ContentMediaItem mediaItem,
            string mediaItemKey, string languageMediaItemKey, DateTime startTime)
        {
            BaseObjectContent mediaContent = mediaItem.Content;
            BaseObjectContent transcriptContent = mediaItem.TranscriptContent;

            if (mediaItem.LocalStudyItemCount() != 0)
            {
                if (mediaItem.LocalSpeakerNameCount() != 0)
                    MediaItem.LocalSpeakerNames = mediaItem.CloneLocalSpeakerNames();
                CreateLocalStudyItems(mediaItem.LocalStudyItems, mediaItemKey, languageMediaItemKey, startTime);
                return;
            }

            if (transcriptContent != null)
            {
                transcriptContent.ResolveReferences(Repositories, false, false);
                ContentStudyList transcriptStudyList = transcriptContent.ContentStorageStudyList;

                if (transcriptStudyList != null)
                {
                    CreateStudyListMediaItemMediaRuns(transcriptStudyList.StudyItems, mediaItemKey, languageMediaItemKey, startTime);
                    return;
                }
            }

            List<ContentStudyList> sourceStudyLists = mediaItem.SourceStudyLists;

            if (sourceStudyLists != null)
            {
                foreach (ContentStudyList studyList in sourceStudyLists)
                    CreateStudyListMediaItemMediaRuns(studyList.StudyItems, mediaItemKey, languageMediaItemKey, startTime);
            }
        }

        protected void CreateLocalStudyItems(List<MultiLanguageItem> studyItems,
            string mediaItemKey, string languageMediaItemKey, DateTime startTime)
        {
            if (studyItems == null)
                return;

            MediaItem.LocalStudyItems = new List<MultiLanguageItem>();

            foreach (MultiLanguageItem oldStudyItem in studyItems)
            {
                MultiLanguageItem studyItem = oldStudyItem.Clone() as MultiLanguageItem;

                if (studyItem.LanguageItems == null)
                    continue;

                MediaItem.LocalStudyItems.Add(studyItem);

                foreach (LanguageItem languageItem in studyItem.LanguageItems)
                {
                    int sentenceRunCount = languageItem.SentenceRunCount();
                    int sentenceRunIndex;

                    for (sentenceRunIndex = 0; sentenceRunIndex < sentenceRunCount; sentenceRunIndex++)
                    {
                        TextRun sentenceRun = languageItem.GetSentenceRun(sentenceRunIndex);

                        if (sentenceRun.MediaRuns == null)
                            continue;

                        foreach (MediaRun mediaRun in sentenceRun.MediaRuns)
                        {
                            TimeSpan start = (startTime - StartTime) + mediaRun.Start;
                            mediaRun.MediaItemKey = MediaContent.KeyString;
                            mediaRun.LanguageMediaItemKey = LanguageMediaItem.KeyString;
                            mediaRun.Start = start;
                        }
                    }
                }
            }
        }

        protected void CreateStudyListMediaItemMediaRuns(List<MultiLanguageItem> studyItems,
            string mediaItemKey, string languageMediaItemKey, DateTime startTime)
        {
            if (studyItems == null)
                return;

            int studyItemCount = studyItems.Count;
            int studyItemIndex;
            List<MediaRun> mediaItemMediaRuns = new List<MediaRun>();

            for (studyItemIndex = 0; studyItemIndex < studyItemCount; studyItemIndex++)
            {
                MultiLanguageItem studyItem = studyItems[studyItemIndex];

                if (studyItem.LanguageItems == null)
                    continue;

                foreach (LanguageItem languageItem in studyItem.LanguageItems)
                {
                    int sentenceRunCount = languageItem.SentenceRunCount();
                    int sentenceRunIndex;

                    for (sentenceRunIndex = 0; sentenceRunIndex < sentenceRunCount; sentenceRunIndex++)
                    {
                        TextRun sentenceRun = languageItem.GetSentenceRun(sentenceRunIndex);

                        if (sentenceRun.MediaRuns == null)
                            continue;

                        mediaItemMediaRuns.Clear();
                        sentenceRun.GetMediaRunsWithReferenceKeys(mediaItemMediaRuns, mediaItemKey, languageMediaItemKey);

                        foreach (MediaRun mediaItemMediaRun in mediaItemMediaRuns)
                        {
                            TimeSpan start = (startTime - StartTime) + mediaItemMediaRun.Start;
                            TimeSpan length = mediaItemMediaRun.Length;
                            MediaRun mediaRun = new MediaRun(
                                mediaItemMediaRun.KeyString,
                                null,
                                MediaContent.KeyString,
                                LanguageMediaItem.KeyString,
                                MediaItem.Owner,
                                start,
                                length);
                            sentenceRun.AddMediaRun(mediaRun);

                        }
                    }
                }
            }
        }

        protected bool OutputSourceSentence(MultiLanguageItem multiLanguageItem, LanguageItem languageItem,
            TextRun sentenceRun, string sentenceText, LanguageID languageID, string speed)
        {
            MediaRun sentenceMediaRun = null;
            bool returnValue = true;

            if (languageItem == null)
            {
                languageItem = new LanguageItem(multiLanguageItem.Key, languageID, sentenceText);
                multiLanguageItem.Add(languageItem);
            }

            VoiceCheck(multiLanguageItem.SpeakerNameKey, languageID, speed);

            sentenceMediaRun = CreateSourceMediaRun(sentenceText, languageID, speed);

            if (sentenceRun == null)
            {
                sentenceRun = new Content.TextRun(languageItem, sentenceMediaRun);
                languageItem.AddSentenceRun(sentenceRun);
            }
            else
                sentenceRun.AddMediaRun(sentenceMediaRun);

            return returnValue;
        }

        protected void ExtractTextFromNode(StringBuilder sb, XNode node)
        {
            switch (node.GetType().Name)
            {
                case "XText":
                    XText textNode = node as XText;
                    sb.Append(textNode.Value);
                    break;
                case "XElement":
                    XElement subElement = node as XElement;
                    sb.Append(subElement.Value);
                    break;
                default:
                    break;
            }
        }

        protected void DeletePriorReferenceMediaRuns()
        {
            List<ContentStudyList> sourceStudyLists = MediaItem.SourceStudyLists;

            if (sourceStudyLists == null)
                return;

            string mediaItemKey = MediaContent.KeyString;
            string languageMediaItemKey = LanguageMediaItem.KeyString;

            foreach (ContentStudyList studyList in sourceStudyLists)
                studyList.DeleteMediaRunsWithReferenceKeys(mediaItemKey, languageMediaItemKey);
        }

        protected MediaRun CreateSentenceMediaRun(string sentence, string speakerKey, LanguageID languageID, string speed)
        {
            string fileBaseName = TextUtilities.MakeValidFileBase(sentence, MaxFileBaseLength) + (GetSpeedKeyFromString(speed) == "Slow" ? "_slow" : "");
            string fileName = fileBaseName + ".mp3";
            string sharedFileUrl = SharedMediaDirectoryUrl + "/" + fileName;
            string generateFileUrl = GenerateMediaDirectoryUrl + "/" + fileName;
            string sharedFilePath = ApplicationData.MapToFilePath(sharedFileUrl);
            string generateFilePath = ApplicationData.MapToFilePath(generateFileUrl);
            string fileUrl = null;
            string filePath = null;
            string lessonKey = MediaItem.Node.KeyString;
            string mediaRunKey;
            string mimeType = "audio/mpeg3";
            string message = String.Empty;
            MediaRun mediaRun = null;

            if (String.IsNullOrEmpty(generateFilePath) && !String.IsNullOrEmpty(generateFileUrl))
            {
                int fileBaseLimit = MaxFileBaseLength / 2;
                if (fileBaseLimit >= 20)
                {
                    fileBaseName = TextUtilities.MakeValidFileBase(sentence, fileBaseLimit) + (GetSpeedKeyFromString(speed) == "Slow" ? "_slow" : "");
                    fileName = fileBaseName + ".mp3";
                    sharedFileUrl = SharedMediaDirectoryUrl + "/" + fileName;
                    generateFileUrl = GenerateMediaDirectoryUrl + "/" + fileName;
                    sharedFilePath = ApplicationData.MapToFilePath(sharedFileUrl);
                    generateFilePath = ApplicationData.MapToFilePath(generateFileUrl);
                    if (String.IsNullOrEmpty(generateFilePath) && !String.IsNullOrEmpty(generateFileUrl))
                    {
                        Error = S("Url to file path mapping for generated audio failed after shortening attempt: ") + generateFileUrl;
                        return null;
                    }
                }
                else
                {
                    Error = S("Url to file path mapping for generated audio failed: ") + generateFileUrl;
                    return null;
                }
            }

            if (generateFilePath.Length > MaxFilePathLength)
            {
                int overage = generateFilePath.Length - MaxFilePathLength;
                int fileBaseLimit = MaxFileBaseLength - overage;
                if (fileBaseLimit <= 20)
                {
                    Error = S("File path for this sentence is too long: ") + sentence;
                    return null;
                }
                fileBaseName = TextUtilities.MakeValidFileBase(sentence, fileBaseLimit) + (GetSpeedKeyFromString(speed) == "Slow" ? "_slow" : "");
                fileName = fileBaseName + ".mp3";
                sharedFileUrl = SharedMediaDirectoryUrl + "/" + fileName;
                generateFileUrl = GenerateMediaDirectoryUrl + "/" + fileName;
                sharedFilePath = ApplicationData.MapToFilePath(sharedFileUrl);
                generateFilePath = ApplicationData.MapToFilePath(generateFileUrl);
            }

            if (FileSingleton.Exists(sharedFilePath))
            {
                filePath = sharedFilePath;
                fileUrl = sharedFileUrl;
                mediaRunKey = "Shared";
            }
            else if (FileSingleton.Exists(generateFilePath))
            {
                filePath = generateFilePath;
                fileUrl = generateFileUrl;
                mediaRunKey = "NotShared";
            }
            else
            {
                sentence = MediaUtilities.FilterTextBeforeSpeech(sentence, languageID, UserProfile, FilterAsides);

                VoiceCheck(speakerKey, languageID, speed);

                if (!String.IsNullOrEmpty(sentence))
                {
                    if (!SpeechEngine.SpeakToFile(sentence, generateFilePath, mimeType, out message))
                    {
                        Error = message;
                        return null;
                    }
                }

                filePath = generateFilePath;
                fileUrl = generateFileUrl;
                mediaRunKey = "NotShared";
            }

            mediaRun = new MediaRun(mediaRunKey, fileName, MediaItem.Owner);

            return mediaRun;
        }

        protected MediaRun CreateSourceMediaRun(string sentenceText, LanguageID languageID, string speed)
        {
            string fileUrl = GenerateMediaDirectoryUrl + "/" + "CurrentSentence.wav";
            string filePath = ApplicationData.MapToFilePath(fileUrl);
            TimeSpan length = new TimeSpan(0);
            string mimeType = "audio/wav";
            string message = String.Empty;
            IDataBuffer storage;
            WaveAudioBuffer waveData;
            string mediaRunKey = (GetSpeedKeyFromString(speed) == "Slow" ? "SlowAudio" : "Audio");
            string mediaItemKey = MediaContent.KeyString;
            string languageMediaItemKey = LanguageMediaItem.KeyString;
            TimeSpan start = CurrentTime - StartTime;
            MediaRun mediaRun = new MediaRun(
                mediaRunKey,
                null,
                mediaItemKey,
                languageMediaItemKey,
                MediaItem.Owner,
                start,
                TimeSpan.Zero);

            if (FileSingleton.Exists(filePath))
                FileSingleton.Delete(filePath);

            FileSingleton.DirectoryExistsCheck(filePath);

            sentenceText = MediaUtilities.FilterTextBeforeSpeech(sentenceText, languageID, UserProfile, FilterAsides);

            if (!String.IsNullOrEmpty(sentenceText))
            {
                if (!SpeechEngine.SpeakToFile(sentenceText, filePath, mimeType, out message))
                {
                    Error = message;
                    return mediaRun;
                }
            }

            if (FileSingleton.Exists(filePath))
            {
                storage = new FileBuffer(filePath);
                waveData = new WaveAudioBuffer(storage);

                if (AppendSourceAudio(sentenceText, languageID, waveData, out message))
                {
                    mediaRun.Length = waveData.Duration;
                    CurrentTime = CurrentTime + mediaRun.Length;
                    // Adjust media run times so that they are not back-to-back.
                    AdjustMediaRunSpan(mediaRun, waveData.SampleRate);
                }

                if (FileSingleton.Exists(filePath))
                    FileSingleton.Delete(filePath);
            }

            return mediaRun;
        }

        protected void AdjustMediaRunSpan(MediaRun mediaRun, int sampleRate)
        {
            long ticks = (FadeInCount * 10000000L) / sampleRate;
            TimeSpan timeAdust = TimeSpan.FromTicks(ticks * 4);
            mediaRun.Length = mediaRun.Length - timeAdust;
        }

        protected bool CreateTranscriptMediaRun(MultiLanguageItem transcriptSpeakerText,
            string sentenceText, MediaRun sentenceMediaRun, LanguageID languageID, string speed)
        {
            string fileUrl = sentenceMediaRun.GetUrl(GenerateMediaDirectoryUrl);
            string filePath = ApplicationData.MapToFilePath(fileUrl);
            DateTime start = CurrentTime;
            TimeSpan length = new TimeSpan(0);
            string mimeType = "audio/mpeg3";
            string message = String.Empty;
            WaveAudioBuffer waveData;
            string speakerNameKey = null;

            if (transcriptSpeakerText != null)
                speakerNameKey = transcriptSpeakerText.SpeakerNameKey;

            if (!FileSingleton.Exists(filePath))
            {
                VoiceCheck(speakerNameKey, languageID, speed);

                sentenceText = MediaUtilities.FilterTextBeforeSpeech(sentenceText, languageID, UserProfile, FilterAsides);

                if (String.IsNullOrEmpty(sentenceText))
                    return true;

                if (!SpeechEngine.SpeakToFile(sentenceText, filePath, mimeType, out message))
                {
                    Error = message;
                    return false;
                }

                if (!FileSingleton.Exists(filePath))
                    return true;
            }

            waveData = MediaConvertSingleton.Mp3Decoding(filePath, out message);

            if (waveData != null)
            {
                length = waveData.Duration;

                if (AppendAudio(transcriptSpeakerText, sentenceText, languageID, waveData, out message))
                    return true;
            }

            return false;
        }

        protected void AddTranscriptItem(MultiLanguageItem transcriptSpeakerText, string sentenceText,
            LanguageID languageID, MediaRun mediaRun)
        {
            int sentenceIndex;
            string sentenceKey;

            if (TranscriptStudyList != null)
            {
                sentenceIndex = TranscriptStudyList.StudyItemCount();
                sentenceKey = TranscriptStudyList.AllocateStudyItemKey();
            }
            else
            {
                if (!IsGenerateLocalTranscript)
                    return;

                sentenceIndex = MediaItem.LocalStudyItemCount();
                sentenceKey = MediaItem.AllocateLocalStudyItemKey();
            }

            List<MediaRun> mediaRuns = new List<MediaRun>(1) { mediaRun };
            TextRun newSentenceRun = new TextRun(0, sentenceText.Length, mediaRuns);
            List<TextRun> sentenceRuns = new List<TextRun>(1) { newSentenceRun };
            List<TextRun> wordRuns = null;

            if (transcriptSpeakerText != null)
            {
                LanguageItem languageItem = transcriptSpeakerText.LanguageItem(CurrentHostLanguageID);

                if (languageItem == null)
                {
                    languageItem = new LanguageItem(sentenceKey, CurrentHostLanguageID, sentenceText, sentenceRuns, wordRuns);
                    transcriptSpeakerText.Add(languageItem);
                }
                else
                    languageItem.SentenceRuns = sentenceRuns;

                transcriptSpeakerText.Key = sentenceKey;

                foreach (LanguageItem aLanguageItem in transcriptSpeakerText.LanguageItems)
                {
                    aLanguageItem.Key = sentenceKey;

                    if (aLanguageItem.LanguageID != languageID)
                    {
                        if (aLanguageItem.SentenceRunCount() != 0)
                            aLanguageItem.SentenceRuns[0].MediaRuns = mediaRuns;
                    }
                }
            }
            else
            {
                LanguageItem newLanguageItem = new LanguageItem(sentenceKey, CurrentHostLanguageID, sentenceText, sentenceRuns, wordRuns);
                transcriptSpeakerText = new MultiLanguageItem(sentenceKey, newLanguageItem);
            }

            if (TranscriptStudyList != null)
                TranscriptStudyList.AddStudyItem(transcriptSpeakerText);
            else
                MediaItem.AddLocalStudyItem(transcriptSpeakerText);
        }

        protected void VoiceCheck(string speakerNameKey, LanguageID voiceLanguageID, string speed)
        {
            string voiceName = null;

            if (voiceLanguageID == null)
                voiceLanguageID = TargetLanguageID;

            if (voiceLanguageID == null)
                voiceLanguageID = HostLanguageID;

            if (voiceLanguageID == null)
                voiceLanguageID = UILanguageID;

            if (voiceLanguageID == null)
                voiceLanguageID = LanguageLookup.English;

            if (SpeakerToVoiceNameMap != null)
            {
                if (String.IsNullOrEmpty(speakerNameKey))
                    speakerNameKey = "default";

                string mediaLanguageCode = voiceLanguageID.MediaLanguageCode();
                string speakerLanguageKey = speakerNameKey + "_" + mediaLanguageCode;

                if (!SpeakerToVoiceNameMap.TryGetValue(speakerLanguageKey, out voiceName))
                    voiceName = "(default)";

                if (String.IsNullOrEmpty(voiceName) || (voiceName == "(default)"))
                    voiceName = GetDefaultVoiceName(voiceLanguageID);

                if (CurrentVoiceName != voiceName)
                {
                    CurrentVoiceName = voiceName;
                    SpeechEngine.SetVoice(CurrentVoiceName, voiceLanguageID);
                }
            }

            if (CurrentVoiceLanguageID != voiceLanguageID)
            {
                CurrentVoiceLanguageID = voiceLanguageID;

                if (String.IsNullOrEmpty(voiceName))
                {
                    if (CurrentVoiceLanguageID == CurrentHostLanguageID)
                        CurrentVoiceName = CurrentHostVoiceName;
                    else if (CurrentVoiceLanguageID == CurrentTargetLanguageID)
                        CurrentVoiceName = CurrentTargetVoiceName;
                    else
                        return;

                    SpeechEngine.SetVoice(CurrentVoiceName, voiceLanguageID);
                }
            }

            int speedValue = GetSpeedFromString(speed);

            SpeechEngine.SetSpeed(speedValue);
        }

        protected string GetDefaultVoiceName(LanguageID voiceLanguageID)
        {
            string voiceName = "(default)";

            LanguageDescription languageDescription = LanguageLookup.GetLanguageDescription(voiceLanguageID);

            if (languageDescription != null)
            {
                voiceName = languageDescription.PreferedVoiceNameOrDefault;

                if (String.IsNullOrEmpty(voiceName))
                    voiceName = "(default)";
            }

            return voiceName;
        }

        protected int GetSpeedFromString(string speed)
        {
            int speedValue = 0;

            switch (speed.ToLower())
            {
                case "slow":
                    speedValue = -5 + DefaultSpeed;
                    break;
                case "normal":
                    speedValue = DefaultSpeed;
                    break;
                case "fast":
                    speedValue = 5 + DefaultSpeed;
                    break;
                default:
                    ParseInteger(speed, out speedValue);
                    break;
            }

            return speedValue;
        }

        protected virtual bool AppendSilence(double seconds)
        {
            bool returnValue = true;

            if (AudioBuffer == null)
            {
                IDataBuffer storage = new FileBuffer(AudioBufferPath);
                AudioBuffer = new WaveAudioBuffer(storage);
                AudioBuffer.Initialize(2, 44100, 16, null, false);
            }

            AudioBuffer.AppendSilence(seconds);

            TimeSpan length = TimeSpan.FromSeconds(seconds);
            CurrentTime += length;

            return returnValue;
        }

        protected virtual bool AppendAudio(MultiLanguageItem transcriptSpeakerText, string sentenceText,
            LanguageID languageID, WaveAudioBuffer waveData, out string message)
        {
            message = null;

            if (waveData.NumberOfChannels != 2)
            {
                WaveAudioBuffer oldWaveData = waveData;
                IDataBuffer storage = new MemoryBuffer();
                waveData = new WaveAudioBuffer(storage);
                waveData.Initialize(1, oldWaveData.SampleRate, oldWaveData.BitsPerSample, null, false);

                if (!MediaConvertSingleton.NumberOfChannelsChange(oldWaveData, waveData, 2, false))
                {
                    message = "Error converting audio to stereo.";
                    return false;
                }
            }

            int sampleRate = 44100;

            if (AudioBuffer != null)
                sampleRate = AudioBuffer.SampleRate;

            if (waveData.SampleRate != sampleRate)
            {
                WaveAudioBuffer oldWaveData = waveData;
                IDataBuffer storage = new MemoryBuffer();
                waveData = new WaveAudioBuffer(storage);
                waveData.Initialize(1, sampleRate, oldWaveData.BitsPerSample, null, false);

                if (!MediaConvertSingleton.RateChange(oldWaveData, waveData, sampleRate, false))
                {
                    message = "Error changing audio rate.";
                    return false;
                }
            }

            waveData.FadeInOutSamples(0, waveData.SampleCount, FadeInCount, FadeOutCount);

            if (AudioBuffer == null)
            {
                IDataBuffer storage = new FileBuffer(AudioBufferPath);
                AudioBuffer = new WaveAudioBuffer(storage, waveData);
            }
            else
                AudioBuffer.Append(waveData);

            TimeSpan start = CurrentTime - StartTime;

            CurrentTime += waveData.Duration;

            if (transcriptSpeakerText != null)
            {
                MediaRun mediaRun = new MediaRun("Audio", null,
                    MediaContent.KeyString, LanguageMediaItem.KeyString,
                    UserRecord.Owner, start, waveData.Duration);
                LastGeneratedMediaRun = mediaRun;
                AddTranscriptItem(transcriptSpeakerText, sentenceText, languageID, mediaRun);
            }
            else if (IsGenerateLocalTranscript && !IsGenerateStudyItemsOnly
                && (languageID != null) && !String.IsNullOrEmpty(sentenceText))
            {
                MediaRun mediaRun = new MediaRun("Audio", null, MediaContent.KeyString,
                    LanguageMediaItem.KeyString, UserRecord.Owner,
                    start, waveData.Duration);
                LastGeneratedMediaRun = mediaRun;
                transcriptSpeakerText = new MultiLanguageItem();
                TextRun transcriptTextRun = new TextRun(0, sentenceText.Length, null);
                LanguageItem transcriptLanguageItem = new LanguageItem(null, languageID, sentenceText,
                    new List<TextRun>(1) { transcriptTextRun }, null);
                transcriptSpeakerText.Add(transcriptLanguageItem);
                AddTranscriptItem(transcriptSpeakerText, sentenceText, languageID, mediaRun);
            }

            return true;
        }

        protected virtual bool AppendSourceAudio(string sentenceText,
            LanguageID languageID, WaveAudioBuffer waveData, out string message)
        {
            message = null;

            if (waveData.NumberOfChannels != 2)
            {
                WaveAudioBuffer oldWaveData = waveData;
                IDataBuffer storage = new MemoryBuffer();
                waveData = new WaveAudioBuffer(storage);
                waveData.Initialize(1, oldWaveData.SampleRate, oldWaveData.BitsPerSample, null, false);

                if (!MediaConvertSingleton.NumberOfChannelsChange(oldWaveData, waveData, 2, false))
                {
                    message = "Error converting audio to stereo.";
                    return false;
                }
            }

            waveData.FadeInOutSamples(0, waveData.SampleCount, FadeInCount, FadeOutCount);

            if (AudioBuffer == null)
            {
                IDataBuffer storage = new FileBuffer(AudioBufferPath);
                AudioBuffer = new WaveAudioBuffer(storage, waveData);
            }
            else
                AudioBuffer.Append(waveData);

            return true;
        }

        protected virtual bool AppendFileAudio(MultiLanguageItem transcriptSpeakerText, string sentenceText,
            LanguageID languageID, string fileUrl, string contentSpeed, string renderSpeed, out string message)
        {
            string filePath = ApplicationData.MapToFilePath(fileUrl);
            TimeSpan start = CurrentTime - StartTime;
            TimeSpan length = new TimeSpan(0);
            string mimeType = "audio/mpeg3";
            WaveAudioBuffer waveData;
            string speakerNameKey = null;

            if (transcriptSpeakerText != null)
                speakerNameKey = transcriptSpeakerText.SpeakerNameKey;

            message = String.Empty;

            if (!FileSingleton.Exists(filePath))
            {
                if (GetSpeedKeyFromString(contentSpeed) == "Slow")
                    MediaUtilities.ChangeFileExtension(filePath, "_slow" + MediaUtilities.GetFileExtension(filePath));

                VoiceCheck(speakerNameKey, languageID, renderSpeed);

                sentenceText = MediaUtilities.FilterTextBeforeSpeech(sentenceText, languageID, UserProfile, FilterAsides);

                if (String.IsNullOrEmpty(sentenceText))
                    return true;

                if (!SpeechEngine.SpeakToFile(sentenceText, filePath, mimeType, out message))
                {
                    Error = message;
                    return false;
                }

                if (!FileSingleton.Exists(filePath))
                    return true;

                waveData = MediaConvertSingleton.Mp3Decoding(filePath, out message);
            }
            else
            {
                waveData = MediaConvertSingleton.Mp3Decoding(filePath, out message);

                if (GetSpeedKeyFromString(renderSpeed) == "Slow")
                    waveData.SpeedChange(GetSpeedMultiplier(renderSpeed));
            }

            if (waveData != null)
            {
                length = waveData.Duration;

                if (AppendAudio(transcriptSpeakerText, sentenceText, languageID, waveData, out message))
                    return true;
            }

            return false;
        }

        protected virtual bool AppendSourceFileAudio(MultiLanguageItem multiLanguageItem, LanguageItem languageItem,
            TextRun sentenceRun, string sentenceText,
            LanguageID languageID, string fileUrl, string contentSpeed, string renderSpeed, out string message)
        {
            string filePath = ApplicationData.MapToFilePath(fileUrl);
            TimeSpan start = CurrentTime - StartTime;
            TimeSpan length = new TimeSpan(0);
            string mimeType = "audio/mpeg3";
            WaveAudioBuffer waveData;

            message = String.Empty;

            if (!FileSingleton.Exists(filePath))
            {
                if (GetSpeedKeyFromString(contentSpeed) == "Slow")
                    MediaUtilities.ChangeFileExtension(filePath, "_slow" + MediaUtilities.GetFileExtension(filePath));

                VoiceCheck(multiLanguageItem.SpeakerNameKey, languageID, renderSpeed);

                sentenceText = MediaUtilities.FilterTextBeforeSpeech(sentenceText, languageID, UserProfile, FilterAsides);

                if (String.IsNullOrEmpty(sentenceText))
                    return true;

                if (!SpeechEngine.SpeakToFile(sentenceText, filePath, mimeType, out message))
                {
                    Error = message;
                    return false;
                }

                if (!FileSingleton.Exists(filePath))
                    return true;

                waveData = MediaConvertSingleton.Mp3Decoding(filePath, out message);
            }
            else
            {
                waveData = MediaConvertSingleton.Mp3Decoding(filePath, out message);

                if (GetSpeedKeyFromString(renderSpeed) == "Slow")
                    waveData.SpeedChange(GetSpeedMultiplier(renderSpeed));
            }

            if (waveData != null)
            {
                length = waveData.Duration;

                if (AppendSourceAudio(sentenceText, languageID, waveData, out message))
                {
                    CurrentTime = CurrentTime + length;
                    return true;
                }
            }

            return false;
        }

        protected virtual bool AppendFileAudioSegment(MultiLanguageItem transcriptSpeakerText, string sentenceText,
            LanguageID languageID, string fileUrl,
            TimeSpan startTime, TimeSpan lengthTime, string contentSpeed, string renderSpeed, out string message)
        {
            string filePath = ApplicationData.MapToFilePath(fileUrl);
            TimeSpan start = CurrentTime - StartTime;

            message = String.Empty;

            if (fileUrl != CachedFileUrl)
            {
                if (FileSingleton.Exists(filePath))
                {
                    CachedWaveData = MediaConvertSingleton.Mp3Decoding(filePath, out message);

                    if (CachedWaveData == null)
                    {
                        message = "Error reading decoding media reference file: " + fileUrl;
                        CachedFileUrl = null;
                        return false;
                    }

                    if (CachedWaveData.NumberOfChannels != 2)
                    {
                        WaveAudioBuffer oldWaveData = CachedWaveData;
                        IDataBuffer storage = new MemoryBuffer();
                        CachedWaveData = new WaveAudioBuffer(storage);
                        CachedWaveData.Initialize(1, oldWaveData.SampleRate, oldWaveData.BitsPerSample, null, false);

                        if (!MediaConvertSingleton.NumberOfChannelsChange(oldWaveData, CachedWaveData, 2, false))
                        {
                            message = "Error converting audio to stereo.";
                            return false;
                        }
                    }

                    CachedFileUrl = fileUrl;
                }
                else
                {
                    message = "Media reference file doesn't exist: " + fileUrl;
                    return false;
                }
            }

            if (AudioBuffer == null)
            {
                IDataBuffer storage = new FileBuffer(AudioBufferPath);
                AudioBuffer = new WaveAudioBuffer(storage);
                AudioBuffer.Initialize(CachedWaveData.NumberOfChannels, CachedWaveData.SampleRate, CachedWaveData.BitsPerSample, null, false);
            }

            int mediaRate = CachedWaveData.SampleRate;
            int sourceSampleIndex = (int)(startTime.TotalSeconds * mediaRate);
            int destinationSampleIndex = AudioBuffer.SampleCount;
            int sampleCount = (int)(lengthTime.TotalSeconds * mediaRate);

            if (GetSpeedKeyFromString(renderSpeed) != "Normal")
            {
                float speedMultiplier = GetSpeedMultiplier(renderSpeed);

                if (!AudioBuffer.SetFromSpeedChange(CachedWaveData, sourceSampleIndex, destinationSampleIndex, sampleCount,
                    speedMultiplier))
                {
                    message = "Error appending media reference segment: " + fileUrl;
                    return false;
                }

                lengthTime = TimeSpan.FromSeconds(lengthTime.TotalSeconds / speedMultiplier);
            }
            else
            {
                if (!AudioBuffer.SetFrom(CachedWaveData, sourceSampleIndex, destinationSampleIndex, sampleCount))
                {
                    message = "Error appending media reference segment: " + fileUrl;
                    return false;
                }
            }

            CurrentTime += lengthTime;

            if (transcriptSpeakerText != null)
            {
                MediaRun mediaRun = new MediaRun("Audio", null, MediaContent.KeyString,
                    LanguageMediaItem.KeyString, UserRecord.Owner,
                    start, lengthTime);

                AddTranscriptItem(transcriptSpeakerText, sentenceText, languageID, mediaRun);
            }
            else if (!IsGenerateStudyItemsOnly && (languageID != null) && !String.IsNullOrEmpty(sentenceText))
            {
                MediaRun mediaRun = new MediaRun("Audio", null, MediaContent.KeyString,
                    LanguageMediaItem.KeyString, UserRecord.Owner,
                    start, lengthTime);
                LastGeneratedMediaRun = mediaRun;
                transcriptSpeakerText = new MultiLanguageItem();
                TextRun transcriptTextRun = new TextRun(0, sentenceText.Length, null);
                LanguageItem transcriptLanguageItem = new LanguageItem(null, languageID, sentenceText,
                    new List<TextRun>(1) { transcriptTextRun }, null);
                transcriptSpeakerText.Add(transcriptLanguageItem);
                AddTranscriptItem(transcriptSpeakerText, sentenceText, languageID, mediaRun);
            }

            return true;
        }

        protected virtual bool AppendSourceFileAudioSegment(MultiLanguageItem multiLanguageItem, LanguageItem languageItem,
            TextRun sentenceRun, string sentenceText,
            LanguageID languageID, string fileUrl,
            TimeSpan startTime, TimeSpan lengthTime, string contentSpeed, string renderSpeed, out string message)
        {
            string filePath = ApplicationData.MapToFilePath(fileUrl);
            TimeSpan start = CurrentTime - StartTime;

            message = String.Empty;

            if (fileUrl != CachedFileUrl)
            {
                if (FileSingleton.Exists(filePath))
                {
                    CachedWaveData = MediaConvertSingleton.Mp3Decoding(filePath, out message);

                    if (CachedWaveData == null)
                    {
                        message = "Error reading decoding media reference file: " + fileUrl;
                        CachedFileUrl = null;
                        return false;
                    }

                    if (CachedWaveData.NumberOfChannels != 2)
                    {
                        WaveAudioBuffer oldWaveData = CachedWaveData;
                        IDataBuffer storage = new MemoryBuffer();
                        CachedWaveData = new WaveAudioBuffer(storage);
                        CachedWaveData.Initialize(1, oldWaveData.SampleRate, oldWaveData.BitsPerSample, null, false);

                        if (!MediaConvertSingleton.NumberOfChannelsChange(oldWaveData, CachedWaveData, 2, false))
                        {
                            message = "Error converting audio to stereo.";
                            return false;
                        }
                    }

                    CachedFileUrl = fileUrl;
                }
                else
                {
                    message = "Media reference file doesn't exist: " + fileUrl;
                    return false;
                }
            }

            if (AudioBuffer == null)
            {
                IDataBuffer storage = new FileBuffer(AudioBufferPath);
                AudioBuffer = new WaveAudioBuffer(storage);
                AudioBuffer.Initialize(CachedWaveData.NumberOfChannels, CachedWaveData.SampleRate, CachedWaveData.BitsPerSample, null, false);
            }

            int mediaRate = CachedWaveData.SampleRate;
            int sourceSampleIndex = (int)(startTime.TotalSeconds * mediaRate);
            int destinationSampleIndex = AudioBuffer.SampleCount;
            int sampleCount = (int)(lengthTime.TotalSeconds * mediaRate);

            if (GetSpeedKeyFromString(renderSpeed) != "Normal")
            {
                float speedMultiplier = GetSpeedMultiplier(renderSpeed);

                if (!AudioBuffer.SetFromSpeedChange(CachedWaveData, sourceSampleIndex, destinationSampleIndex, sampleCount,
                    speedMultiplier))
                {
                    message = "Error appending media reference segment: " + fileUrl;
                    return false;
                }

                lengthTime = TimeSpan.FromSeconds(lengthTime.TotalSeconds / speedMultiplier);
            }
            else
            {
                if (!AudioBuffer.SetFrom(CachedWaveData, sourceSampleIndex, destinationSampleIndex, sampleCount))
                {
                    message = "Error appending media reference segment: " + fileUrl;
                    return false;
                }
            }

            CurrentTime += lengthTime;

            if (languageItem == null)
            {
                languageItem = new LanguageItem(multiLanguageItem.Key, languageID, sentenceText);
                multiLanguageItem.Add(languageItem);
            }

            MediaRun sentenceMediaRun = new MediaRun("Audio", null, MediaContent.KeyString,
                LanguageMediaItem.KeyString, UserRecord.Owner,
                start, lengthTime);

            if (sentenceRun == null)
            {
                sentenceRun = new Content.TextRun(languageItem, sentenceMediaRun);
                languageItem.AddSentenceRun(sentenceRun);
            }
            else
                sentenceRun.AddMediaRun(sentenceMediaRun);

            return true;
        }

        protected virtual bool AppendMediaItem(BaseObjectContent content, ContentMediaItem mediaItem,
            LanguageMediaItem languageMediaItem, MediaDescription mediaDescription, string fileUrl,
            string contentSpeed, string renderSpeed, out string message)
        {
            string filePath = ApplicationData.MapToFilePath(fileUrl);
            TimeSpan start = CurrentTime - StartTime;
            TimeSpan length = new TimeSpan(0);
            WaveAudioBuffer waveData;

            message = String.Empty;

            if (!FileSingleton.Exists(filePath))
                return true;

            if (mediaDescription.MimeType != "audio/mpeg3")
            {
                message = "Appended media item must be .mp3.";
                return false;
            }

            waveData = MediaConvertSingleton.Mp3Decoding(filePath, out message);

            if (waveData != null)
            {
                if (GetSpeedKeyFromString(renderSpeed) == "Slow")
                    waveData.SpeedChange(GetSpeedMultiplier(renderSpeed));

                length = waveData.Duration;

                if (AppendSourceAudio(null, languageMediaItem.TargetMediaLanguageID, waveData, out message))
                {
                    if (mediaItem.SourceContentKeys != null)
                    {
                        if (MediaItem.SourceContentKeys == null)
                            MediaItem.SourceContentKeys = new List<string>(mediaItem.SourceContentKeys);
                        else
                        {
                            foreach (string sourceContentKey in mediaItem.SourceContentKeys)
                            {
                                if (!MediaItem.SourceContentKeys.Contains(sourceContentKey))
                                    MediaItem.SourceContentKeys.Add(sourceContentKey);
                            }
                        }
                    }

                    CreateMediaItemMediaRuns(mediaItem, content.KeyString,
                        languageMediaItem.KeyString, CurrentTime);

                    CurrentTime = CurrentTime + length;

                    return true;
                }
            }

            return false;
        }

        protected override bool HandleElementError(XElement element, string message)
        {
            Error = Error + "\n" + message;
            return HandleText(message, "normal");
        }
    }
}
