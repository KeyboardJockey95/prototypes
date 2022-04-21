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
    public class AutomatedMarkupRenderer : MarkupRendererContent
    {
        public LanguageID CurrentVoiceLanguageID { get; set; }
        public string CurrentHostVoiceName { get; set; }
        public string CurrentTargetVoiceName { get; set; }
        public string CurrentVoiceName { get; set; }
        public Dictionary<string, string> SpeakerToVoiceNameMap { get; set; }
        public string CurrentSpeed { get; set; }
        public bool FilterAsides { get; set; }
        public XElement GenerateElement { get; set; }
        public BaseObjectContent MediaContent { get; set; }
        public ContentMediaItem MediaItem { get; set; }
        public LanguageMediaItem LanguageMediaItem { get; set; }
        public string GenerateMediaDirectoryUrl { get; set; }
        public string SharedMediaDirectoryUrl { get; set; }
        public string MarkupMediaDirectoryUrl { get; set; }
        public string MediaDirectoryUrl { get; set; }
        public int MaxFileBaseLength { get; set; }
        public int MaxFilePathLength { get; set; }
        public ITextToSpeech SpeechEngine { get; set; }
        protected int FadeInCount { get; set; }
        protected int FadeOutCount { get; set; }
        public AutomatedCompiledMarkup CompiledMarkup { get; set; }

        public AutomatedMarkupRenderer(
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
                string generateMediaDirectoryUrl,
                string sharedMediaDirectoryUrl)
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
                  true,
                  false)
        {
            DefaultConfigurationName = "Read0";
            CurrentVoiceLanguageID = null;
            CurrentHostVoiceName = hostVoiceName;
            CurrentTargetVoiceName = targetVoiceName;
            SpeakerToVoiceNameMap = speakerToVoiceNameMap;
            CurrentSpeed = "normal";
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
            FadeInCount = 1000;
            FadeOutCount = 1000;
            CompiledMarkup = new AutomatedCompiledMarkup(this);
            InChoice = false;
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
            bool nodeWasReplaced = HandleGenerateElement();

            if (HasError)
                return false;

            return true;
        }

        protected bool HandleGenerateElement()
        {
            bool returnValue = false;

            if (GenerateElement == null)
            {
                if (MarkupTemplate != null)
                {
                    Error = "Sorry, the automated template doesn't have a \"generate\" element, so there is nothing to do.";
                    return false;
                }

                returnValue = HandleNonTemplateGeneration();
            }
            else if (GenerateElement.Elements().Count() == 0)
            {
                Error = "Sorry, the \"generate\" element in the automated template is empty, so there is nothing to do.";
                return false;
            }
            else
                returnValue = HandleElement(GenerateElement);

            return returnValue;
        }

        protected override bool HandleNonTemplateGeneration()
        {
            bool returnValue = true;

            InGenerate = true;

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
            string message;

            if (Content == null)
                return false;

            if (multiLanguageItem == null)
                return false;

            string mediaDirectoryUrl = multiLanguageItem.MediaTildeUrl;
            LanguageItem languageItem = multiLanguageItem.LanguageItem(languageID);

            if (languageItem == null)
                return false;

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

                    if (mediaRun != null)
                    {
                        if (mediaRun.IsReference)
                        {
                            string mediaItemKey = mediaRun.MediaItemKey;
                            string languageMediaItemKey = mediaRun.LanguageMediaItemKey;
                            ContentMediaItem mediaItem = Node.GetMediaItem(mediaItemKey);

                            if (mediaItem != null)
                            {
                                // Don't render from target media item.
                                if (mediaItem != MediaContent.ContentStorageMediaItem)
                                {
                                    MediaDescription mediaDescription = mediaItem.GetMediaDescriptionIndexed(
                                        languageMediaItemKey, 0);

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
                                    sentenceRun.DeleteMediaRun(mediaRun);
                                    OutputSourceSentence(multiLanguageItem, languageItem, sentenceRun,
                                        text, languageID, speed);
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

                            if (!AppendSourceFileAudio(multiLanguageItem, languageItem, sentenceRun,
                                sentenceText, languageID, url, contentSpeed, renderSpeed, out message))
                            {
                                HandleError("Error appending item: " + message);
                                return false;
                            }
                        }
                    }
                    else
                        OutputSourceSentence(multiLanguageItem, languageItem, sentenceRun,
                            text, languageID, speed);

                    OutputPause("multiply", 1.3, 2.0);

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

                OutputSourceSentence(multiLanguageItem, languageItem, null,
                    text, languageID, speed);
            }

            return true;
        }

        protected override bool HandleGenerate(XElement element)
        {
            bool returnValue;

            InGenerate = true;

            StudyItem = null;
            CurrentTargetLanguageID = TargetLanguageID;
            CurrentHostLanguageID = HostLanguageID;

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
            double value = 0.0;
            double minimum = 0.0;
            string mode = "seconds";
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
                            value = Convert.ToDouble(attributeValue);
                        }
                        catch (Exception)
                        {
                            Error = Error + "\nError converting pause seconds attribute to a number: " + attributeValue;
                        }
                        mode = "seconds";
                        break;
                    case "multiply":
                        try
                        {
                            value = Convert.ToDouble(attributeValue);
                        }
                        catch (Exception)
                        {
                            Error = Error + "\nError converting multiply attribute to a number: " + attributeValue;
                        }
                        mode = "multiply";
                        break;
                    case "add":
                        try
                        {
                            value = Convert.ToDouble(attributeValue);
                        }
                        catch (Exception)
                        {
                            Error = Error + "\nError converting add attribute to a number: " + attributeValue;
                        }
                        mode = "add";
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

            OutputPause(mode, value, minimum);

            returnValue = HandleElementChildren(element) && returnValue;

            return returnValue;
        }

        protected override bool HandleItem(XElement element)
        {
            int index = -1;
            int startIndex = -1;
            int endIndex = 1;
            string label = String.Empty;
            string tag = String.Empty;
            bool all = false;
            LanguageID languageID = CurrentTargetLanguageID;
            string speed = CurrentSpeed;
            bool returnValue = true;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "index":
                        ParseInteger(attributeValue, out index);
                        break;
                    case "start":
                        ParseInteger(attributeValue, out startIndex);
                        break;
                    case "end":
                        ParseInteger(attributeValue, out endIndex);
                        break;
                    case "label":
                        label = attributeValue;
                        break;
                    case "tag":
                        tag = attributeValue;
                        break;
                    case "all":
                        all = true;
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

            OutputItem(index, startIndex, endIndex, label, tag, all, languageID, speed);

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
            bool isRandomUnique = false;
            bool isRandomNew = false;
            bool isAdaptiveMixNew = false;
            int chunkSize = ToolProfile.DefaultChunkSize;
            int level = ToolProfile.DefaultReviewLevel;
            string profileName = DefaultProfileName;
            string configurationKey = DefaultConfigurationName;
            int totalCount = -1;
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

            OutputPushWorkingSet(contentKey, tag, label, selector, mode, isRandomUnique, isRandomNew, isAdaptiveMixNew,
                chunkSize, level, profileName, configurationKey);

            int blockStartLabel = CurrentLabel;

            OutputGetAndTouchItem(-1);
            PushFixup();

            returnValue = HandleElementChildren(element) && returnValue;

            OutputWait();

            OutputNextItemAndBranch(-1);
            PushFixup();

            OutputUnconditionalBranch(blockStartLabel);

            PopFixup(CurrentLabel, "TargetLabel");
            PopFixup(CurrentLabel, "TargetLabel");

            OutputPopWorkingSet();

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

            OutputPushWorkingSet(contentKey, tag, label, selector, mode, isRandomUnique, isRandomNew, isAdaptiveMixNew,
                chunkSize, level, profileName, configurationKey);

            OutputPushDurationTimes(duration);

            int blockStartLabel = CurrentLabel;

            OutputDurationConditionalLoop(-1);
            PushFixup();

            OutputGetAndTouchItem(-1);
            PushFixup();

            returnValue = HandleElementChildren(element) && returnValue;

            OutputWait();

            OutputNextItemAndBranch(-1);
            PushFixup();

            OutputUnconditionalBranch(blockStartLabel);

            PopFixup(CurrentLabel, "TargetLabel");
            PopFixup(CurrentLabel, "TargetLabel");
            PopFixup(CurrentLabel, "TargetLabel");

            OutputPopVariable("duration");
            OutputPopVariable("startTime");

            OutputPopWorkingSet();

            return returnValue;
        }

        protected override bool HandleLoop(XElement element)
        {
            bool nodeWasReplaced = false;
            int blockStartLabel = CurrentLabel;
            HandleElementChildren(element);
            OutputUnconditionalBranch(blockStartLabel);
            return nodeWasReplaced;
        }

        public bool LastInstructionWasWait
        {
            get
            {
                return CompiledMarkup.LastInstructionWasWait;
            }
        }

        protected override bool HandleChoose(XElement element)
        {
            int nth = MarkupTemplate.GetElementNth(GenerateElement, element);
            BaseObjectNodeTree tree = SourceTree;
            BaseObjectNode node = SourceNode;
            BaseObjectContent content = DocumentContent;
            int treeKey = tree.KeyInt;
            int nodeKey;
            string contentKey = content.KeyString;
            LanguageID languageID = HostLanguageID;
            string prompt = null;
            bool nodeWasReplaced = false;

            if (node == tree)
            {
                node = null;
                nodeKey = -1;
            }
            else
                nodeKey = node.KeyInt;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "id":
                        break;
                    case "language":
                        ParseLanguageID(attributeValue, out languageID);
                        break;
                    case "prompt":
                        prompt = LanguageUtilities.TranslateString(attributeValue, languageID);
                        break;
                    case "orientation":
                    case "width":
                    case "choicewidth":
                    case "class":
                    case "style":
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            int choiceCount = 0;

            foreach (XElement descendent in element.Descendants())
            {
                if (descendent.Name.LocalName.ToLower() == "choice")
                    choiceCount++;
            }

            int choiceIndex = 0;

            foreach (XElement descendent in element.Descendants())
            {
                if (descendent.Name.LocalName.ToLower() == "choice")
                {
                    foreach (XAttribute attribute in descendent.Attributes())
                    {
                        string attributeValue = attribute.Value.Trim();

                        switch (attribute.Name.LocalName.ToLower())
                        {
                            case "prompt":
                                string choicePrompt = LanguageUtilities.TranslateString(attributeValue, languageID);
                                if (choiceIndex == 0)
                                {
                                    if (!String.IsNullOrEmpty(prompt))
                                    {
                                        if (!prompt.EndsWith(":"))
                                            prompt += ":";

                                        prompt += "  ";
                                    }
                                }
                                else
                                    prompt += ", ";
                                prompt += choicePrompt;
                                //prompt += "\n" + choicePrompt;
                                break;
                            default:
                                break;
                        }
                    }

                    choiceIndex++;
                }
            }

            if (!String.IsNullOrEmpty(prompt))
                HandleSentence(null, prompt, null, CurrentHostLanguageID, "Normal");

            //if (!LastInstructionWasWait)
            //    OutputWait();

            OutputPushVariable("ChoiceID", String.Empty);

            OutputChoose(
                treeKey,
                nodeKey,
                contentKey,
                nth);

            OutputWait();

            ChoiceIDOrdinal = 0;
            HandleElementChildren(element);

            OutputPopVariable("ChoiceID");

            return nodeWasReplaced;
        }

        protected override bool HandleChoice(XElement element)
        {
            string choiceID = "Choose1Choice" + (++ChoiceIDOrdinal).ToString();
            string mode = "Display";
            bool nodeWasReplaced = false;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "id":
                    case "prompt":
                    case "language":
                    case "width":
                    case "class":
                    case "style":
                    case "name":
                        break;
                    case "mode":
                        mode = attributeValue;
                        break;
                    case "command":
                    case "target":
                    case "goto":
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            OutputChoiceConditionalBranch(choiceID, -1);
            PushFixup();

            if (mode.ToLower() == "automated")
            {
                bool saveInChoice = InChoice;
                InChoice = true;
                HandleElementChildren(element);
                InChoice = saveInChoice;

                if (!LastInstructionWasWait)
                    OutputWait();
            }

            OutputWaitForDone();

            PopFixup(CurrentLabel, "TargetLabel");

            return nodeWasReplaced;
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
                string title = "Audio";
                if ((node == tree) || (node == null))
                {
                    node = null;
                    content = tree.GetContent(contentKey);
                }
                else
                    content = node.GetContent(contentKey);
                if (content != null)
                    title = content.GetTitleString();
                else
                {
                    HandleElementError(element, "Content " + contentKey + " not found.");
                    return nodeWasReplaced;
                }
                string label = S("(Playing media: \"" + title + "\")");
                if (isView)
                {
                    ContentRenderParameters contentRenderParameters = new ContentRenderParameters(
                        tree, node, contentKey, itemIndex, itemCount, sentenceIndex, tag,
                        overriddenTargetLanguageID, mediaLanguageID, subType, subSubType, optionsList,
                        UseAudio, UsePicture, (++PlayerIDOrdinal).ToString(), true, "Next", label);
                    if (!LastInstructionWasWait)
                        OutputWait();
                    OutputLabel(label);
                    OutputPlayView(
                        contentRenderParameters.TreeKey,
                        contentRenderParameters.NodeKey,
                        contentRenderParameters.ContentKey,
                        contentRenderParameters.LanguageMediaItemKey,
                        contentRenderParameters.OptionsList,
                        label,
                        AutomatedContinueMode.WaitForDone,
                        0);
                    OutputWait();
                }
                else
                {
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
                                OutputStudyItemTimeTextMap(treeKey, nodeKey, contentKey, overriddenTargetLanguageID);
                                OutputPlayMedia(mediaUrl, isAudio, speed);
                                OutputLabel(label);
                                OutputWait();
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
            }
            else
            {
                XText textNode = new XText("(invalid play element: \"" + element.ToString() + "\")");
                element.ReplaceWith(textNode);
            }

            return nodeWasReplaced;
        }

        protected override bool HandleStudy(XElement element)
        {
            LanguageID overriddenLanguageID = null;
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
            bool isView = !InChoice;
            List<BaseString> optionsList = null;
            bool nodeWasReplaced = false;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "content":
                        ParseContent(attributeValue, overriddenLanguageID, out contentKey,
                            out itemIndex, out itemCount, out sentenceIndex, out tag, out subType, out subSubType,
                            out overriddenLanguageID, out optionsList, out errorMessage);
                        break;
                    case "subtype":
                        subType = attributeValue;
                        break;
                    case "subsubtype":
                        subSubType = attributeValue;
                        break;
                    case "language":
                        ParseLanguageID(attributeValue, out overriddenLanguageID);
                        if (mediaLanguageID == null)
                            mediaLanguageID = overriddenLanguageID.MediaLanguageID();
                        break;
                    case "medialanguage":
                        ParseMediaLanguageID(attributeValue, out mediaLanguageID);
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
                    case "options":
                        optionsList = BaseString.GetBaseStringListFromString(attributeValue);
                        break;
                    case "useaudio":
                    case "usepicture":
                    case "usemedia":
                    case "endofstudy":
                    case "session":
                    case "sessionindex":
                    case "tool":
                    case "tooltype":
                    case "profilen":
                    case "configuration":
                    case "mode":
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
            else if (!isView)
            {
            }
            else if (variableValue != null)
            {
            }
            else if (contentKey != null)
            {
                BaseObjectNodeTree tree = SourceTree;
                BaseObjectNode node = SourceNode;
                BaseObjectContent content;
                string title = "Study";
                if ((node == tree) || (node == null))
                {
                    node = null;
                    content = tree.GetContent(contentKey);
                }
                else
                    content = node.GetContent(contentKey);
                if (content != null)
                    title = content.GetTitleString();
                else
                {
                    HandleElementError(element, "Content " + contentKey + " not found.");
                    return nodeWasReplaced;
                }
                string label = S("(Studying: \"" + title + "\")");
                ContentRenderParameters contentRenderParameters = new ContentRenderParameters(
                    tree, node, contentKey, itemIndex, itemCount, sentenceIndex, tag,
                    overriddenLanguageID, mediaLanguageID, subType, subSubType, optionsList,
                    UseAudio, UsePicture, (++PlayerIDOrdinal).ToString(), true, "Next", label);
                if (!LastInstructionWasWait)
                    OutputWait();
                OutputLabel(label);
                OutputStudyView(
                    contentRenderParameters.TreeKey,
                    contentRenderParameters.NodeKey,
                    contentRenderParameters.ContentKey,
                    contentRenderParameters.OptionsList,
                    label,
                    AutomatedContinueMode.WaitForDone,
                    0);
                OutputWait();
            }
            else
            {
                XText textNode = new XText("(invalid study element: \"" + element.ToString() + "\")");
                element.ReplaceWith(textNode);
            }

            return nodeWasReplaced;
        }

        protected override bool HandleStudyItem(XElement element)
        {
            int index = -1;
            bool nodeWasReplaced = false;

            foreach (XAttribute attribute in element.Attributes())
            {
                string attributeValue = attribute.Value.Trim();

                attributeValue = EvaluateAttributeExpression(attributeValue);

                switch (attribute.Name.LocalName.ToLower())
                {
                    case "index":
                        ParseInteger(attributeValue, out index);
                        break;
                    default:
                        HandleElementError(element, "(unknown attribute in element \"" + element.Name.LocalName + "\": \"" + attribute.Name.LocalName + "\")");
                        return false;
                }
            }

            string contentKey = "dummy";
            string label = S("(studying an item)");
            ContentRenderParameters contentRenderParameters = new ContentRenderParameters(
                SourceTree, SourceNode, contentKey, index, 1, 0, null,
                null, null, null, null, null,
                UseAudio, UsePicture, (++PlayerIDOrdinal).ToString(), false, null, String.Empty);
            if (!LastInstructionWasWait)
                OutputWait();
            //OutputLabel(label);
            OutputStudyItemView(
                contentRenderParameters.TreeKey,
                contentRenderParameters.NodeKey,
                contentKey,
                index,
                label,
                contentRenderParameters.OptionsList,
                AutomatedContinueMode.WaitForEndOfStudyItem,
                0);
            //OutputWait();

            return nodeWasReplaced;
        }

        protected bool HandleMultiLanguageItem(XElement element, MultiLanguageItem multiLanguageItem, LanguageID languageID, string speed)
        {
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
                return false;

            speed = GetSpeedKeyFromString(speed);

            int sentenceCount = languageItem.SentenceRuns.Count();
            string contentSpeed = "Normal";
            string renderSpeed = "Normal";

            if ((languageItem.SentenceRuns != null) && (sentenceCount != 0))
            {
                int sentenceIndex = 0;

                foreach (TextRun sentenceRun in languageItem.SentenceRuns)
                {
                    string sentenceText = languageItem.GetRunText(sentenceRun);
                    MediaRun mediaRun = null;
                    MultiLanguageItem transcriptSpeakerText = new MultiLanguageItem();

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

                                    LastItemTime = mediaRun.Length;

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

                            if (mediaRun.Length == TimeSpan.Zero)
                                mediaRun.Length = MediaUtilities.GetMediaUrlTimeSpan(url);

                            LastItemTime = mediaRun.Length;

                            if (!AppendFileAudio(transcriptSpeakerText, sentenceText, languageID, url,
                                contentSpeed, renderSpeed, out message))
                            {
                                HandleElementError(element, "Error appending item: " + message);
                                return false;
                            }
                        }
                    }
                    else
                        OutputSentence(text, multiLanguageItem.SpeakerNameKey, languageID, speed, transcriptSpeakerText);

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

                OutputSentence(text, multiLanguageItem.SpeakerNameKey, languageID, speed, transcriptSpeakerText);
            }

            return true;
        }

        protected override XNode RenderContentView(ContentRenderParameters parameters)
        {
            string controller = ((SourceTree != null) && SourceTree.IsPlan() ? "Plans" : "Lessons");
            object routeValues = new
            {
                treeKey = parameters.TreeKey,
                nodeKey = parameters.NodeKey,
                contentKey = parameters.ContentKey
            };
            string url = ApplicationData.Global.CreateHostUrl(controller, "RenderContent", routeValues);
            OutputView(url, AutomatedContinueMode.WaitForDone, 0);
            OutputWait();
            XText textNode = new XText("(Rendered content: \"" + parameters.ContentKey + "\")");
            return textNode;
        }

        protected override void HandleMarkerContent(string marker)
        {
            if (!LastInstructionWasWait)
                OutputWait();

            CompiledMarkup.AddMarker(marker, CurrentLabel);
        }

        protected override void HandleGoToContent(string marker)
        {
            CompiledMarkup.AddGoTo(marker);
        }

        // Returns true if node was replaced.
        protected override bool HandleTextNode(XText node)
        {
            string text = node.Value.Trim();
            bool nodeWasReplaced = false;

            if (!String.IsNullOrEmpty(text))
                HandleText(text, CurrentSpeed);

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
                                            UseAudio, UsePicture, "1", false, null, String.Empty);

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

        public MediaRun CreateItemMediaRun(string studyItemKey, int sentenceRunIndex,
            string sentenceText, LanguageID languageID, string speakerKey,
            string speed, string mediaDirectoryUrl)
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

            if (!String.IsNullOrEmpty(sentenceText))
            {
                if (!SpeechEngine.SpeakToFile(sentenceText, filePath, mimeType, out message))
                {
                    Error = message;
                    return mediaRun;
                }
            }

            return mediaRun;
        }

        public MediaRun CreateSentenceMediaRun(string sentence, string speakerKey, LanguageID languageID, string speed)
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
                int fileBaseLimit = MaxFileBaseLength/2;
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

            if (String.IsNullOrEmpty(sentenceText))
                return mediaRun;

            if (!SpeechEngine.SpeakToFile(sentenceText, filePath, mimeType, out message))
            {
                Error = message;
                return mediaRun;
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
            string mimeType = "audio/mpeg3";
            string message = String.Empty;

            if (!FileSingleton.Exists(filePath))
            {
                if (transcriptSpeakerText != null)
                    VoiceCheck(transcriptSpeakerText.SpeakerNameKey, languageID, speed);
                else
                    VoiceCheck(null, languageID, speed);

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

            OutputText(sentenceText, fileUrl);

            return true;
        }

        public void VoiceCheck(string speakerNameKey, LanguageID voiceLanguageID, string speed)
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
                    speedValue = -5;
                    break;
                case "normal":
                    speedValue = 0;
                    break;
                case "fast":
                    speedValue = 5;
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

            waveData.FadeInOutSamples(0, waveData.SampleCount, FadeInCount, FadeOutCount);

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

            return true;
        }

        protected virtual bool AppendFileAudio(MultiLanguageItem transcriptSpeakerText, string sentenceText,
            LanguageID languageID, string fileUrl, string contentSpeed, string renderSpeed, out string message)
        {
            string filePath = ApplicationData.MapToFilePath(fileUrl);
            string mimeType = "audio/mpeg3";

            message = String.Empty;

            if (!FileSingleton.Exists(filePath))
            {
                if (GetSpeedKeyFromString(contentSpeed) == "Slow")
                    MediaUtilities.ChangeFileExtension(filePath, "_slow" + MediaUtilities.GetFileExtension(filePath));

                if (transcriptSpeakerText != null)
                    VoiceCheck(transcriptSpeakerText.SpeakerNameKey, languageID, renderSpeed);
                else
                    VoiceCheck(null, languageID, renderSpeed);

                sentenceText = MediaUtilities.FilterTextBeforeSpeech(sentenceText, languageID, UserProfile, FilterAsides);

                if (!SpeechEngine.SpeakToFile(sentenceText, filePath, mimeType, out message))
                {
                    Error = message;
                    return false;
                }
            }

            OutputText(sentenceText, fileUrl);

            return true;
        }

        protected virtual bool AppendSourceFileAudio(MultiLanguageItem multiLanguageItem, LanguageItem languageItem,
            TextRun sentenceRun, string sentenceText,
            LanguageID languageID, string fileUrl, string contentSpeed, string renderSpeed, out string message)
        {
            string filePath = ApplicationData.MapToFilePath(fileUrl);
            string mimeType = "audio/mpeg3";

            message = String.Empty;

            if (!FileSingleton.Exists(filePath))
            {
                if (GetSpeedKeyFromString(contentSpeed) == "Slow")
                    MediaUtilities.ChangeFileExtension(filePath, "_slow" + MediaUtilities.GetFileExtension(filePath));

                VoiceCheck(multiLanguageItem.SpeakerNameKey, languageID, renderSpeed);

                sentenceText = MediaUtilities.FilterTextBeforeSpeech(sentenceText, languageID, UserProfile, FilterAsides);

                if (!SpeechEngine.SpeakToFile(sentenceText, filePath, mimeType, out message))
                {
                    Error = message;
                    return false;
                }
            }

            OutputText(sentenceText, fileUrl);

            return true;
        }

        protected virtual bool AppendFileAudioSegment(MultiLanguageItem transcriptSpeakerText, string sentenceText,
            LanguageID languageID, string fileUrl,
            TimeSpan startTime, TimeSpan lengthTime, string contentSpeed, string renderSpeed, out string message)
        {
            string filePath = ApplicationData.MapToFilePath(fileUrl);

            message = String.Empty;

            if (!FileSingleton.Exists(filePath))
            {
                message = S("Referenced audio file for segment doesn't exist: ") + fileUrl;
                Error = message;
                return false;
            }

            OutputTextSegment(sentenceText, fileUrl, startTime, lengthTime);

            return true;
        }

        protected virtual bool AppendSourceFileAudioSegment(MultiLanguageItem multiLanguageItem, LanguageItem languageItem,
            TextRun sentenceRun, string sentenceText,
            LanguageID languageID, string fileUrl,
            TimeSpan startTime, TimeSpan lengthTime, string contentSpeed, string renderSpeed, out string message)
        {
            string filePath = ApplicationData.MapToFilePath(fileUrl);

            message = String.Empty;

            if (!FileSingleton.Exists(filePath))
            {
                message = S("Referenced audio file for segment doesn't exist: ") + fileUrl;
                Error = message;
                return false;
            }

            OutputTextSegment(sentenceText, fileUrl, startTime, lengthTime);

            return true;
        }

        protected void OutputPlayView(
            int treeKey,
            int nodeKey,
            string contentKey,
            string languageMediaItemKey,
            List<BaseString> optionsList,
            string label,
            AutomatedContinueMode continueMode,
            int timeSeconds)
        {
            string controller = ((SourceTree != null) && SourceTree.IsPlan() ? "Plans" : "Lessons");
            object routeValues = new
            {
                treeKey,
                nodeKey,
                contentKey,
                languageMediaItemKey,
                speed = BaseString.GetOptionStringFromList(optionsList, "Speed", "Normal"),
                endOfMedia = BaseString.GetOptionStringFromList(optionsList, "EndOfMedia", "Stop"),
                endOfViewCallback = "AutomatedEndOfView",
                autoPlay = BaseString.GetOptionFlagFromList(optionsList, "AutoPlay", true),
                player = BaseString.GetOptionStringFromList(optionsList, "Player", "Full"),
                playerID = BaseString.GetOptionStringFromList(optionsList, "PlayerID", "1"),
                showVolume = BaseString.GetOptionFlagFromList(optionsList, "ShowVolume", true),
                showTimeSlider = BaseString.GetOptionFlagFromList(optionsList, "ShowTimeSlider", true),
                showTimeText = BaseString.GetOptionFlagFromList(optionsList, "ShowTimeText", true),
                label
            };
            string url = ApplicationData.Global.CreateHostUrl(controller, "AutomatedPlay", routeValues);
            OutputView(url, continueMode, timeSeconds);
        }

        protected void OutputStudyView(
            int treeKey,
            int nodeKey,
            string contentKey,
            List<BaseString> optionsList,
            string label,
            AutomatedContinueMode continueMode,
            int timeSeconds)
        {
            string controller = ((SourceTree != null) && SourceTree.IsPlan() ? "Plans" : "Lessons");
            int sessionIndex = ObjectUtilities.GetIntegerFromString(
                BaseString.GetOptionStringFromList(optionsList, "SessionIndex", "0"), 0);
            ToolTypeCode toolType = ToolUtilities.GetToolTypeCodeFromString(
                BaseString.GetOptionStringFromList(optionsList, "ToolType", "Unknown"));
            string profileName = BaseString.GetOptionStringFromList(optionsList, "ProfileName", String.Empty);
            string configurationKey = BaseString.GetOptionStringFromList(optionsList, "ConfigurationKey", String.Empty);
            ToolSelectorMode mode = ToolItemSelector.GetToolSelectorModeFromString(
                BaseString.GetOptionStringFromList(optionsList, "Mode", "Unknown"));
            object routeValues = new
            {
                treeKey,
                nodeKey,
                contentKey,
                automatedContentKey = DocumentContent.KeyString,
                itemIndex = -1,
                endOfViewCallback = "AutomatedEndOfView",
                label,
                sessionIndex,
                toolType,
                profileName,
                configurationKey,
                mode
            };
            string url = ApplicationData.Global.CreateHostUrl(controller, "AutomatedStudy", routeValues);
            OutputView(url, continueMode, timeSeconds);
        }

        protected void OutputStudyItemView(
            int treeKey,
            int nodeKey,
            string contentKey,
            int itemIndex,
            string label,
            List<BaseString> optionsList,
            AutomatedContinueMode continueMode,
            int timeSeconds)
        {
            string controller = ((SourceTree != null) && SourceTree.IsPlan() ? "Plans" : "Lessons");
            int sessionIndex = ObjectUtilities.GetIntegerFromString(
                BaseString.GetOptionStringFromList(optionsList, "SessionIndex", "0"), 0);
            ToolTypeCode toolType = ToolUtilities.GetToolTypeCodeFromString(
                BaseString.GetOptionStringFromList(optionsList, "ToolType", "Unknown"));
            string profileName = BaseString.GetOptionStringFromList(optionsList, "ProfileName", String.Empty);
            string configurationKey = BaseString.GetOptionStringFromList(optionsList, "ConfigurationKey", String.Empty);
            ToolSelectorMode mode = ToolItemSelector.GetToolSelectorModeFromString(
                BaseString.GetOptionStringFromList(optionsList, "Mode", "Unknown"));
            object routeValues = new
            {
                treeKey,
                nodeKey,
                automatedContentKey = DocumentContent.KeyString,
                contentKey,
                itemIndex,
                endOfViewCallback = "AutomatedEndOfView",
                label,
                sessionIndex,
                toolType,
                profileName,
                configurationKey,
                mode
            };
            string url = ApplicationData.Global.CreateHostUrl(controller, "AutomatedStudy", routeValues);
            CompiledMarkup.AddStudyItemInstruction(url, itemIndex, continueMode, timeSeconds);
        }

        protected void OutputMarkupElementView(
            int treeKey,
            int nodeKey,
            string contentKey,
            string elementName,
            string name,
            int nth,
            bool childrenOnly,
            AutomatedContinueMode continueMode,
            int timeSeconds)
        {
            string controller = ((SourceTree != null) && SourceTree.IsPlan() ? "Plans" : "Lessons");
            object routeValues = new
            {
                treeKey,
                nodeKey,
                contentKey,
                elementName,
                name,
                nth,
                childrenOnly,
                inGenerate = true
            };
            string url = ApplicationData.Global.CreateHostUrl(controller, "RenderMarkupElement", routeValues);
            OutputView(url, continueMode, timeSeconds);
        }

        protected void OutputView(string url, AutomatedContinueMode continueMode, int timeSeconds)
        {
            CompiledMarkup.AddViewInstruction(
                url,
                continueMode,
                timeSeconds);
        }

        protected void OutputChoose(
            int treeKey,
            int nodeKey,
            string contentKey,
            int nth)
        {
            string controller = ((SourceTree != null) && SourceTree.IsPlan() ? "Plans" : "Lessons");
            object routeValues = new
            {
                treeKey,
                nodeKey,
                contentKey,
                elementName = "choose",
                name = "",
                nth,
                childrenOnly = false,
                inGenerate = true
            };
            string url = ApplicationData.Global.CreateHostUrl(controller, "RenderMarkupElement", routeValues);
            CompiledMarkup.AddChooseInstruction(url);
        }

        protected void OutputPlayMedia(
            string mediaUrl,
            bool isAudio,
            string speed)
        {
            CompiledMarkup.AddPlayMediaInstruction(mediaUrl, isAudio, speed);
        }

        protected void OutputStudyItemTimeTextMap(
            int treeKey,
            int nodeKey,
            string contentKey,
            LanguageID languageID)
        {
            string controller = ((SourceTree != null) && SourceTree.IsPlan() ? "Plans" : "Lessons");
            object routeValues = new
            {
                treeKey,
                nodeKey,
                contentKey,
                languageCode = languageID.LanguageCultureExtensionCode,
                allMappings = true
            };
            string url = ApplicationData.Global.CreateHostUrl(controller, "StudyItemTimeMap", routeValues);
            CompiledMarkup.AddStudyItemTimeTextMapInstruction(url);
        }

        protected void OutputLabel(string label)
        {
            CompiledMarkup.AddLabelInstruction(label);
        }

        protected void OutputText(string text, string audioFileUrl)
        {
            audioFileUrl = MediaUtilities.GetContentUrl(audioFileUrl, String.Empty);
            CompiledMarkup.AddTextInstruction(
                text,
                audioFileUrl);
        }

        protected void OutputTextSegment(string text, string audioFileUrl,
            TimeSpan startTime, TimeSpan lengthTime)
        {
            audioFileUrl = MediaUtilities.GetContentUrl(audioFileUrl, String.Empty);
            CompiledMarkup.AddTextSegmentInstruction(
                text,
                audioFileUrl,
                startTime,
                lengthTime);
        }

        protected void OutputWait()
        {
            CompiledMarkup.AddWaitInstruction();
        }

        protected void OutputWaitForDone()
        {
            CompiledMarkup.AddWaitForDoneInstruction();
        }

        protected void OutputPause(string mode, double value, double minimum)
        {
            CompiledMarkup.AddPauseInstruction(mode, value, minimum);
        }

        protected void OutputItem(
            int index, int startIndex, int endIndex, string label, string tag,
            bool all, LanguageID languageID, string speed)
        {
            CompiledMarkup.AddItemInstruction(index, startIndex, endIndex, label, tag, all, languageID, speed);
        }

        protected void OutputPushWorkingSet(string contentKey, string tag, string label,
            SelectorAlgorithmCode selector, ToolSelectorMode mode,
            bool isRandomUnique, bool isRandomNew, bool isAdaptiveMixNew, int chunkSize, int level,
            string profileName, string configurationKey)
        {
            CompiledMarkup.AddPushWorkingSetInstruction(contentKey, tag, label, selector, mode, isRandomUnique, isRandomNew, isAdaptiveMixNew,
                chunkSize, level, profileName, configurationKey);
        }

        protected void OutputPopWorkingSet()
        {
            CompiledMarkup.AddPopWorkingSetInstruction();
        }

        protected void OutputPushWorkingSetCount(string name)
        {
            CompiledMarkup.AddPushWorkingSetCountInstruction(name);
        }

        protected void OutputPushDurationTimes(TimeSpan duration)
        {
            CompiledMarkup.AddPushDurationTimesInstruction(duration);
        }

        protected void OutputPushVariable(string name, object value)
        {
            CompiledMarkup.AddPushVariableInstruction(name, value);
        }

        protected void OutputPopVariable(string name)
        {
            CompiledMarkup.AddPopVariableInstruction(name);
        }

        protected void OutputIncrementVariable(string name)
        {
            CompiledMarkup.AddIncrementVariableInstruction(name);
        }

        protected void OutputIndexCountConditionalLoop(int label)
        {
            CompiledMarkup.AddIndexCountConditionalLoopInstruction(label);
        }

        protected void OutputDurationConditionalLoop(int label)
        {
            CompiledMarkup.AddDurationConditionalLoopInstruction(label);
        }

        protected void OutputUnconditionalBranch(int label)
        {
            CompiledMarkup.AddUnconditionalBranchInstruction(label);
        }

        protected void OutputChoiceConditionalBranch(string choiceID, int label)
        {
            CompiledMarkup.AddChoiceConditionalBranchInstruction(choiceID, label);
        }

        protected void OutputGetAndTouchItem(int label)
        {
            CompiledMarkup.AddGetAndTouchItemInstruction(label);
        }

        protected void OutputNextItemAndBranch(int label)
        {
            CompiledMarkup.AddNextItemAndBranchInstruction(label);
        }

        protected int CurrentLabel
        {
            get
            {
                return CompiledMarkup.LabelOrdinal;
            }
        }

        protected void PushFixup()
        {
            CompiledMarkup.PushFixup();
        }

        protected void PopFixup(int label, string name)
        {
            CompiledMarkup.PopFixup(label, name);
        }

        protected override bool HandleElementError(XElement element, string message)
        {
            if ((Error != null) && Error.Contains(message))
                return true;
            Error = Error + "\n" + message;
            return HandleText(message, "normal");
        }
    }
}
