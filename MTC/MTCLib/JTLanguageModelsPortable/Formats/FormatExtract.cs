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
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Matchers;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatExtract : FormatPatterned
    {
        public string TextInputFormatName { get; set; }
        public bool ExtractSentences { get; set; }
        public bool ExtractWords { get; set; }
        public bool ExtractCharacters { get; set; }
        public bool ExtractExpansion { get; set; }
        public string SentencesTargetKeyName { get; set; }
        public string WordsTargetKeyName { get; set; }
        public string CharactersTargetKeyName { get; set; }
        public string ExpansionTargetKeyName { get; set; }
        private static string FormatDescription = "Extract sentences, words, and/or characters from source content or text.";

        public FormatExtract()
            : base("Line", "Extract", "FormatExtract", FormatDescription, String.Empty, String.Empty,
                  "text/plain", ".txt", null, null, null, null, null)
        {
            ClearFormatExtract();
        }

        public FormatExtract(FormatExtract other)
            : base(other)
        {
            CopyFormatExtract(other);
        }

        // For derived classes.
        public FormatExtract(string name, string type, string description, string targetType, string importExportType,
                string mimeType, string defaultExtension,
                UserRecord userRecord, UserProfile userProfile, IMainRepository repositories, LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base("Line", name, type, description, targetType, importExportType, mimeType, defaultExtension,
                  userRecord, userProfile, repositories, languageUtilities, nodeUtilities)
        {
            ClearFormatExtract();
        }

        public override Format Clone()
        {
            return new FormatExtract(this);
        }

        public void ClearFormatExtract()
        {
            NoDataStreamExport = true;
            ExtractSentences = true;
            ExtractWords = false;
            ExtractCharacters = false;
            ExtractExpansion = false;
            SentencesTargetKeyName = "Sentences";
            WordsTargetKeyName = "Words";
            CharactersTargetKeyName = "Characters";
            ExpansionTargetKeyName = "Expansion";
            IsIncludeMedia = true;
            IsLookupDictionaryAudio = true;
            IsLookupDictionaryPictures = true;
            IsSynthesizeMissingAudio = true;
            IsForceAudio = true;
            IsTranslateMissingItems = true;

            // Override some Format items.
            DefaultContentType = "Text";
            DefaultContentSubType = "Text";
        }

        public void CopyFormatExtract(FormatExtract other)
        {
            NoDataStreamExport = other.NoDataStreamExport;
            ExtractSentences = other.ExtractSentences;
            ExtractWords = other.ExtractWords;
            ExtractCharacters = other.ExtractCharacters;
            ExtractExpansion = other.ExtractExpansion;
            SentencesTargetKeyName = other.SentencesTargetKeyName;
            WordsTargetKeyName = other.WordsTargetKeyName;
            CharactersTargetKeyName = other.CharactersTargetKeyName;
            ExpansionTargetKeyName = other.ExpansionTargetKeyName;
            IsIncludeMedia = other.IsIncludeMedia;
            IsLookupDictionaryAudio = other.IsLookupDictionaryAudio;
            IsLookupDictionaryPictures = other.IsLookupDictionaryPictures;
            IsSynthesizeMissingAudio = other.IsSynthesizeMissingAudio;
            IsTranslateMissingItems = other.IsTranslateMissingItems;
        }

        public override void Read(Stream stream)
        {
            //if (Timer != null)
            //    Timer.Start();

            ReadPatterned(stream);

            if (!String.IsNullOrEmpty(Error))
            {
                if (Timer != null)
                {
                    Timer.Stop();
                    OperationTime = Timer.GetTimeInSeconds();
                }

                throw new Exception(Error);
            }

            try
            {
                if ((Targets != null) && (Targets.Count() != 0))
                {
                    List<IBaseObject> targets = new List<IBaseObject>(Targets);

                    foreach (IBaseObject obj in targets)
                        ReadExtractObject(obj);
                }
                else if ((ReadObjects != null) && (ReadObjects.Count() != 0))
                {
                    List<IBaseObject> readObjects = new List<IBaseObject>(ReadObjects);

                    foreach (IBaseObject obj in readObjects)
                        ReadExtractObject(obj);
                }
            }
            catch (Exception exc)
            {
                PutExceptionError(exc);
            }
            finally
            {
                if (Timer != null)
                {
                    //Timer.Stop();
                    OperationTime = Timer.GetTimeInSeconds();
                }
            }

            if (!String.IsNullOrEmpty(Error))
                throw new Exception(Error);
        }

        protected void ReadExtractObject(IBaseObject obj)
        {
            if ((obj is BaseObjectContent) || (obj is BaseObjectNode) || (obj is ToolStudyList))
            {
                Component = obj;

                if (IsSupportedStatic("Import", TargetType, "Support"))
                    ReadExtractComponent();

                Component = null;
            }
            else
            {
                Error = "Read not supported for this object type: " + obj.GetType().Name;
                throw new Exception(Error);
            }
        }

        protected void ReadExtractComponent()
        {
            BaseObjectTitled titledObject = null;
            BaseObjectNodeTree tree = null;
            BaseObjectNode node = null;
            BaseObjectContent content = null;

            if (Component is BaseObjectTitled)
            {
                titledObject = GetComponent<BaseObjectTitled>();

                if (titledObject is BaseObjectContent)
                {
                    content = GetComponent<BaseObjectContent>();
                    node = content.Node;

                    if (node == null)
                    {
                        PutErrorArgument("Target content has no node: ", content.GetTitleString());
                        return;
                    }

                    if (node.IsTree())
                        tree = node as BaseObjectNodeTree;
                    else
                        tree = node.Tree;

                    ReadExtractContent(tree, node, content);
                }
                else if (titledObject is BaseObjectNode)
                {
                    node = titledObject as BaseObjectNode;

                    if (node.IsTree())
                        tree = node as BaseObjectNodeTree;
                    else
                        tree = node.Tree;

                    ReadExtractNode(tree, node);

                    string contentName = DefaultContentType;

                    if (String.IsNullOrEmpty(contentName))
                        contentName = "Text";

                    content = node.GetContent(contentName);

                    if (content == null)
                        content = node.GetFirstContentWithType(contentName);

                    if (content == null)
                    {
                        PutErrorArgument("No \"" + contentName + "\" content found in ", node.GetTitleString());
                        return;
                    }
                }
                else
                {
                    Error = "Unexpected source object.";
                    return;
                }
            }
            else
            {
                Error = "Unexpected source object.";
                return;
            }

            if (SubDivide && SubDivideToStudyListsOnly)
            {
                ContentStudyList studyList = content.ContentStorageStudyList;

                if (studyList == null)
                {
                    Error = "No study list for " + content.GetTitleString();
                    return;
                }

                List<MultiLanguageItem> studyItems = studyList.StudyItemsRecurse;

                if (studyItems == null)
                    return;

                if ((content.ContentType != "Sentences") &&
                    (content.ContentType != "Words") &&
                    (content.ContentType != "Characters"))
                {
                    if (ExtractSentences)
                        WriteSentencesStudyItems(node, content, studyItems, DeleteBeforeImport);
                }

                if ((content.ContentType != "Words") &&
                    (content.ContentType != "Characters"))
                {
                    if (ExtractWords)
                        WriteWordsStudyItems(node, content, studyItems, DeleteBeforeImport);
                }

                if ((content.ContentType != "Characters"))
                {
                    if (ExtractCharacters)
                        WriteCharactersStudyItems(node, content, studyItems, DeleteBeforeImport);
                }

                if (ExtractExpansion)
                    WriteExpansionStudyItems(node, content, studyItems, DeleteBeforeImport);
            }

            NodeUtilities.UpdateTreeCheck(tree, false, false);
        }

        protected void ReadExtractNode(BaseObjectNodeTree tree, BaseObjectNode node)
        {
            BaseObjectContent content;
            string contentName = DefaultContentType;

            if (String.IsNullOrEmpty(contentName))
                contentName = "Text";

            content = node.GetContent(contentName);

            if (content == null)
                content = node.GetFirstContentWithType(contentName);

            if (content == null)
            {
                PutErrorArgument("No \"" + contentName + "\" content found in ", node.GetTitleString());
                return;
            }

            ReadExtractContent(tree, node, content);

            if (SubDivide && !SubDivideToStudyListsOnly && node.HasChildren())
            {
                foreach (BaseObjectNode childNode in node.Children)
                    ReadExtractNode(tree, childNode);
            }
        }

        protected void ReadExtractContent(
            BaseObjectNodeTree tree,
            BaseObjectNode node,
            BaseObjectContent content)
        {
            ContentStudyList studyList = content.ContentStorageStudyList;

            if (studyList == null)
            {
                PutErrorArgument("No study list for content " + content.GetTitleString() + " in ", node.GetTitleString());
                return;
            }

            if (IsIncludeMedia && IsSynthesizeMissingAudio)
            {
                NodeUtilities.AddSynthesizedVoiceToStudyList(
                    tree,
                    node,
                    content,
                    studyList,
                    LanguageLookup.Target,
                    "(default)",
                    0,
                    false,
                    false,
                    null,
                    true);
            }

            if (!SubDivide || !SubDivideToStudyListsOnly)
            {
                List<MultiLanguageItem> studyItems = studyList.StudyItemsRecurse;

                if (studyItems == null)
                    return;

                if ((content.ContentType != "Sentences") &&
                    (content.ContentType != "Words") &&
                    (content.ContentType != "Characters"))
                {
                    if (ExtractSentences)
                        WriteSentencesStudyItems(node, content, studyItems, DeleteBeforeImport);
                }

                if ((content.ContentType != "Words") &&
                    (content.ContentType != "Characters"))
                {
                    if (ExtractWords)
                        WriteWordsStudyItems(node, content, studyItems, DeleteBeforeImport);
                }

                if ((content.ContentType != "Characters"))
                {
                    if (ExtractCharacters)
                        WriteCharactersStudyItems(node, content, studyItems, DeleteBeforeImport);
                }

                if (ExtractExpansion)
                    WriteExpansionStudyItems(node, content, studyItems, DeleteBeforeImport);
            }
        }

        public override void Write(Stream stream)
        {
            if (Targets != null)
            {
                foreach (IBaseObject target in Targets)
                {
                    BaseObjectTitled titled = target as BaseObjectTitled;

                    if (titled == null)
                        continue;

                    if ((titled.Owner != UserRecord.UserName) && (titled.Owner != UserRecord.Team))
                    {
                        if (!UserRecord.IsAdministrator())
                        {
                            Error = "Sorry, you need to be the owner of the target to use Extract.";
                            return;
                        }
                    }
                }
            }

            using (StreamWriter writer = new StreamWriter(stream, new System.Text.UTF8Encoding(true)))
            {
                TitleCount = 0;
                ComponentIndex = 0;

                if (Targets != null)
                {
                    foreach (IBaseObject target in Targets)
                        WriteExtractObject(writer, target);
                }

                writer.Flush();
            }
        }

        protected void WriteExtractObject(StreamWriter writer, IBaseObject obj)
        {
            if (obj is BaseObjectContent)
            {
                BaseObjectContent content = obj as BaseObjectContent;

                if (!IsSupportedVirtual("Export", "BaseObjectContent", "Support"))
                    return;

                Boolean exportIt = true;

                Tree = content.Tree;
                Component = obj;

                if (ContentKeyFlags != null)
                {
                    if (!ContentKeyFlags.TryGetValue(content.KeyString, out exportIt))
                        exportIt = true;
                }

                if (exportIt)
                    WriteExtractComponent(writer);

                if (content.ContentChildrenCount() != 0)
                {
                    foreach (BaseObjectContent childContent in content.ContentChildren)
                    {
                        if (childContent.KeyString == content.KeyString)
                            continue;

                        WriteExtractObject(writer, childContent);
                    }
                }

                if (exportIt)
                    ComponentIndex = ComponentIndex + 1;

                Component = null;
            }
            else if (obj is BaseObjectNode)
            {
                BaseObjectNode node = obj as BaseObjectNode;

                if (!IsSupportedVirtual("Export", "BaseObjectNode", "Support"))
                    return;

                bool exportIt = true;

                if (NodeKeyFlags != null)
                {
                    if (!NodeKeyFlags.TryGetValue(node.KeyInt, out exportIt))
                        exportIt = true;
                }

                if (exportIt)
                {
                    if (node.ContentChildrenCount() != 0)
                    {
                        foreach (BaseObjectContent content in node.ContentChildren)
                        {
                            ItemIndex = 0;
                            WriteExtractObject(writer, content);
                        }
                    }
                }

                if (node.ChildCount() != 0)
                {
                    foreach (BaseObjectNode childNode in node.Children)
                        WriteExtractObject(writer, childNode);
                }
            }

            if (Tree != null)
                NodeUtilities.UpdateTreeCheck(Tree, false, false);
        }

        protected void WriteExtractComponent(StreamWriter writer)
        {
            BaseObjectTitled titledObject = null;
            BaseObjectContent content = null;
            ContentStudyList studyList = null;
            BaseObjectNode node = null;
            BaseObjectNodeTree tree = null;
            List<MultiLanguageItem> studyItems = null;
            string relativePathToSource = String.Empty;

            if (Component is BaseObjectTitled)
            {
                titledObject = GetComponent<BaseObjectTitled>();

                if (titledObject is BaseObjectContent)
                {
                    content = GetComponent<BaseObjectContent>();
                    node = content.Node;
                }
                else if (titledObject is BaseObjectNode)
                {
                    node = titledObject as BaseObjectNode;
                    content = node.GetContent("Text");

                    if (content == null)
                        content = node.GetFirstContentWithType("Text");

                    if (content == null)
                    {
                        Error = "No \"Text\" content found.";
                        return;
                    }
                }
                else
                {
                    Error = "Unexpected source object.";
                    return;
                }
            }
            else
            {
                Error = "Unexpected source object.";
                return;
            }

            if (node.IsTree())
                tree = titledObject as BaseObjectNodeTree;
            else
                tree = node.Tree;

            if (content != null)
            {
                studyList = content.ContentStorageStudyList;

                if (studyList != null)
                {
                    studyItems = studyList.StudyItems;
                    relativePathToSource = "../" + content.Directory;

                    if ((content.ContentType != "Sentences") &&
                        (content.ContentType != "Words") &&
                        (content.ContentType != "Characters"))
                    {
                        if (ExtractSentences)
                            WriteSentencesStudyItems(node, content, studyItems, DeleteBeforeExport);
                    }

                    if ((content.ContentType != "Words") &&
                        (content.ContentType != "Characters"))
                    {
                        if (ExtractWords)
                            WriteWordsStudyItems(node, content, studyItems, DeleteBeforeExport);
                    }

                    if ((content.ContentType != "Characters"))
                    {
                        if (ExtractCharacters)
                            WriteCharactersStudyItems(node, content, studyItems, DeleteBeforeExport);
                    }

                    if (ExtractExpansion)
                        WriteExpansionStudyItems(node, content, studyItems, DeleteBeforeExport);

                    if (tree != null)
                        NodeUtilities.UpdateTreeCheck(tree, false, false);
                }
            }
        }

        protected void WriteSentencesStudyItems(BaseObjectNode node,
            BaseObjectContent sourceContent, List<MultiLanguageItem> studyItems,
            bool isDeleteBefore)
        {
            BaseObjectContent content = TargetVocabularyContentCreateCheck(
                node,
                sourceContent,
                SentencesTargetKeyName,
                isDeleteBefore);

            ContentStudyList studyList = content.ContentStorageStudyList;

            if (studyList == null)
                throw new Exception("WriteSentencesStudyItems: Missing study list.");

            Component = content;
            ItemIndex = studyList.StudyItemCount();

            List<LanguageID> languageIDs = content.LanguageIDs;

            if (DeleteBeforeExport)
            {
                studyList.DeleteAllStudyItems();
                studyList.StudyItemOrdinal = 0;
            }

            LanguageID targetLanguageID = UserProfile.TargetLanguageIDSafe;

            string destTildeUrl = content.MediaTildeUrl + "/";
            BaseObjectContent oldContent;
            MultiLanguageItem oldStudyItem;

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                studyItem.SentenceRunCheck();

                if (studyItem.IsSentenceMismatch(languageIDs))
                    studyItem.JoinSentenceRuns(languageIDs);

                int sentenceCount = studyItem.GetMaxSentenceCount(languageIDs);

                if (sentenceCount == 0)
                    continue;

                string sourceTildeUrl = studyItem.MediaTildeUrl + "/";
                string relativePathToSource = MediaUtilities.MakeRelativeUrl(destTildeUrl, sourceTildeUrl);

                if (relativePathToSource.EndsWith("/"))
                    relativePathToSource = relativePathToSource.Substring(0, relativePathToSource.Length - 1);

                for (int sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
                {
                    string sentenceText = studyItem.RunText(targetLanguageID, sentenceIndex);

                    if (studyList.FindStudyItem(sentenceText, targetLanguageID) != null)
                        continue;

                    if (NodeUtilities.StringExistsInPriorContentCheck(
                            content,
                            sentenceText,
                            targetLanguageID,
                            out oldContent,
                            out oldStudyItem))
                        continue;

                    if (IsExcludePrior)
                    {
                        if (NodeUtilities.StringExistsInPriorLessonsCheck(
                                content,
                                sentenceText,
                                targetLanguageID,
                                out oldContent,
                                out oldStudyItem))
                            continue;
                    }

                    string studyItemKey = studyList.AllocateStudyItemKey();
                    MultiLanguageItem targetStudyItem = new MultiLanguageItem(studyItemKey, new List<LanguageItem>());

                    foreach (LanguageItem sourceLanguageItem in studyItem.LanguageItems)
                    {
                        TextRun sourceSentenceRun = sourceLanguageItem.GetSentenceRun(sentenceIndex);
                        string text = sourceLanguageItem.GetRunText(sourceSentenceRun);
                        LanguageID languageID = sourceLanguageItem.LanguageID;
                        List<MediaRun> targetMediaRuns = null;
                        if (sourceSentenceRun != null)
                        {
                            text = sourceLanguageItem.GetRunText(sourceSentenceRun);
                            if (IsIncludeMedia)
                            {
                                if (sourceSentenceRun.MediaRunCount() != 0)
                                    targetMediaRuns = sourceSentenceRun.CloneAndRetargetMediaRuns(
                                        relativePathToSource);
                                if (IsSynthesizeMissingAudio)
                                {
                                    if ((targetMediaRuns == null) ||
                                        (targetMediaRuns.Count() == 0) ||
                                        (targetMediaRuns.FirstOrDefault(x => x.KeyString.StartsWith("Audio")) == null))
                                    {
                                        string fileName = MediaUtilities.ComposeStudyItemFileName(
                                            studyItemKey, 0, languageID, "Audio", ".mp3");
                                        string urlDirectory = destTildeUrl;
                                        string urlPath = MediaUtilities.ConcatenateUrlPath(urlDirectory, fileName);
                                        string filePath = ApplicationData.MapToFilePath(urlPath);

                                        bool returnValue = NodeUtilities.AddSynthesizedVoiceDefault(
                                            text,
                                            filePath,
                                            languageID);

                                        if (returnValue)
                                        {
                                            MediaRun mediaRun = new MediaRun("Audio", fileName, studyItem.Owner);
                                            targetMediaRuns = new List<MediaRun>() { mediaRun };
                                        }
                                    }
                                }
                            }
                        }
                        List<TextRun> wordRuns = null;
                        if (sourceLanguageItem.HasWordRuns())
                            wordRuns = sourceLanguageItem.GetSentenceWordRunsRetargeted(sourceSentenceRun);
                        TextRun targetSentenceRun = new TextRun(0, text.Length, targetMediaRuns);
                        LanguageItem targetLanguageItem = new LanguageItem(
                            studyItemKey,
                            sourceLanguageItem.LanguageID,
                            text,
                            new List<TextRun>(1) { targetSentenceRun },
                            wordRuns);
                        targetStudyItem.Add(targetLanguageItem);
                    }

                    targetStudyItem.ExtractSentenceWordAlignment(studyItem, sentenceIndex, 0);
                    studyList.AddStudyItem(targetStudyItem);
                }
            }

            if (SubDivide)
                NodeUtilities.SubDivideStudyList(studyList, TitlePrefix, Master,
                    true, StudyItemSubDivideCount, MinorSubDivideCount, MajorSubDivideCount);

            NodeUtilities.UpdateContentHelper(content);
        }

        protected void WriteWordsStudyItems(BaseObjectNode node,
            BaseObjectContent sourceContent, List<MultiLanguageItem> studyItems,
            bool isDeleteBefore)
        {
            BaseObjectContent content = TargetVocabularyContentCreateCheck(
                node,
                sourceContent,
                WordsTargetKeyName,
                isDeleteBefore);
            string errorMessage = null;

            ContentStudyList studyList = content.ContentStorageStudyList;

            if (studyList == null)
                throw new Exception("WriteWordsStudyItems: Missing study list.");

            Component = content;

            ItemIndex = studyList.StudyItemCount();

            if (!UserProfile.HasTargetLanguage)
                throw new Exception("WriteWordsStudyItems: No current target language.");

            LanguageID targetLanguageID = null;

            foreach (LanguageID languageID in UserProfile.TargetLanguageIDs)
            {
                if (content.HasTargetLanguage(languageID))
                {
                    if (targetLanguageID == null)
                        targetLanguageID = languageID;
                    else if (LanguageLookup.IsAlternatePhonetic(targetLanguageID) &&
                        !LanguageLookup.IsAlternatePhonetic(languageID))
                    {
                        targetLanguageID = languageID;
                        break;
                    }
                }
            }

            List<LanguageID> targetLanguageIDs = content.TargetLanguageIDs;
            List<LanguageID> hostLanguageIDs = content.HostLanguageIDs;
            List<LanguageID> languageIDs = content.LanguageIDs;

            foreach (MultiLanguageItem studyItem in studyItems)
                studyItem.WordRunCheckLanguages(targetLanguageIDs, Repositories.Dictionary);

            List<string> words = new List<string>();
            HashSet<string> wordsHash = new HashSet<string>();

            NodeUtilities.CollectWords(studyItems, targetLanguageID, words, wordsHash);

            int wordCount = words.Count();
            int wordIndex;
            string destTildeUrl = content.MediaTildeUrl + "/";
            BaseObjectContent oldContent;
            MultiLanguageItem oldStudyItem;

            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                string word = words[wordIndex];

                if (studyList.FindStudyItem(word, targetLanguageID) != null)
                    continue;

                if (NodeUtilities.StringExistsInPriorContentCheck(
                        content,
                        word,
                        targetLanguageID,
                        out oldContent,
                        out oldStudyItem))
                    continue;

                if (IsExcludePrior)
                {
                    if (NodeUtilities.StringExistsInPriorLessonsCheck(
                            content,
                            word,
                            targetLanguageID,
                            out oldContent,
                            out oldStudyItem))
                        continue;
                }

                string studyItemKey = studyList.AllocateStudyItemKey();
                MultiLanguageItem targetStudyItem = new MultiLanguageItem(
                    studyItemKey,
                    languageIDs);

                LanguageItem targetLanguageItem = targetStudyItem.LanguageItem(targetLanguageID);
                targetLanguageItem.Text = word;
                TextRun targetRun = new TextRun(0, word.Length, null);
                targetLanguageItem.SentenceRuns = new List<TextRun>() { targetRun };

                DictionaryEntry dictionaryEntry = null;
                List<DictionaryEntry> dictionaryEntries = null;

                dictionaryEntries = NodeUtilities.LookupLocalOrRemoteDictionaryEntries(
                    word,
                    targetLanguageIDs,
                    hostLanguageIDs);

                if ((dictionaryEntries != null) && (dictionaryEntries.Count() != 0))
                {
                    dictionaryEntry = dictionaryEntries.First();
                    int senseCount = dictionaryEntry.SenseCount;
                    int senseIndex;

                    if (dictionaryEntry.AlternateCount != 0)
                    {
                        foreach (LanguageString alternate in dictionaryEntry.Alternates)
                        {
                            LanguageID alternateLanguageID = alternate.LanguageID;

                            if (targetLanguageIDs.Contains(alternateLanguageID))
                            {
                                LanguageItem alternateLanguageItem = targetStudyItem.LanguageItem(alternateLanguageID);

                                if (!alternateLanguageItem.HasText())
                                {
                                    string alternateText = alternate.Text;

                                    if (LanguageLookup.IsAlternatePhonetic(alternateLanguageID))
                                        ConvertPinyinNumeric.ToToneMarks(out alternateText, alternateText);

                                    alternateLanguageItem.Text = alternateText;
                                }
                            }
                        }
                    }

                    for (senseIndex = 0; senseIndex < senseCount; senseIndex++)
                    {
                        Sense sense = dictionaryEntry.GetSenseIndexed(senseIndex);

                        foreach (LanguageID languageID in hostLanguageIDs)
                        {
                            LanguageItem hostLanguageItem = targetStudyItem.LanguageItem(languageID);

                            if (sense.HasLanguage(languageID))
                            {
                                string definitionString = sense.GetDefinition(languageID, false, false);

                                definitionString = definitionString.Replace(" / ", ", ");

                                if (!hostLanguageItem.HasText())
                                    hostLanguageItem.Text = definitionString;
                            }
                        }
                    }
                }

                if (IsTranslateMissingItems)
                    LanguageUtilities.Translator.TranslateMultiLanguageItem(
                        targetStudyItem,
                        languageIDs,
                        false,
                        false,
                        out errorMessage,
                        false);

                // Need content set before getting media.
                studyList.AddStudyItem(targetStudyItem);

                if (IsIncludeMedia)
                {
                    NodeUtilities.GetMediaForStudyItem(
                        targetStudyItem,
                        languageIDs,
                        true,
                        false,
                        ApplicationData.IsMobileVersion || ApplicationData.IsTestMobileVersion,
                        IsLookupDictionaryAudio,
                        IsSynthesizeMissingAudio,
                        IsForceAudio,
                        IsLookupDictionaryPictures);
                }

                /*
                NodeUtilities.AddStudyItemToDictionary(
                    node,
                    content,
                    targetStudyItem,
                    languageIDs,
                    languageIDs,
                    LexicalCategory.Unknown,
                    String.Empty,
                    false,
                    false,
                    ref errorMessage);
                */
            }

            if (SubDivide)
                NodeUtilities.SubDivideStudyList(studyList, TitlePrefix, Master,
                    true, StudyItemSubDivideCount, MinorSubDivideCount, MajorSubDivideCount);

            NodeUtilities.UpdateContentHelper(content);
        }

        protected void WriteCharactersStudyItems(BaseObjectNode node,
            BaseObjectContent sourceContent, List<MultiLanguageItem> studyItems,
            bool isDeleteBefore)
        {
            BaseObjectContent content = TargetVocabularyContentCreateCheck(
                node,
                sourceContent,
                CharactersTargetKeyName,
                isDeleteBefore);
            string errorMessage = null;

            ContentStudyList studyList = content.ContentStorageStudyList;

            if (studyList == null)
                throw new Exception("WriteCharactersStudyItems: Missing study list.");

            Component = content;

            ItemIndex = studyList.StudyItemCount();

            if (!UserProfile.HasTargetLanguage)
                throw new Exception("WriteCharactersStudyItems: No current target language.");

            LanguageID targetLanguageID = null;

            foreach (LanguageID languageID in UserProfile.TargetLanguageIDs)
            {
                if (content.HasTargetLanguage(languageID))
                {
                    if (targetLanguageID == null)
                        targetLanguageID = languageID;
                    else if (LanguageLookup.IsAlternatePhonetic(targetLanguageID) &&
                        !LanguageLookup.IsAlternatePhonetic(languageID))
                    {
                        targetLanguageID = languageID;
                        break;
                    }
                }
            }

            if (targetLanguageID == null)
                return;

            LanguageDescription languageDescription = LanguageLookup.GetLanguageDescription(targetLanguageID);

            if ((languageDescription == null) || !languageDescription.CharacterBased)
            {
                PutError("Skipping character extraction because the target language is not character-based.");
                return;
            }

            List<LanguageID> targetLanguageIDs = content.TargetLanguageIDs;
            List<LanguageID> hostLanguageIDs = content.HostLanguageIDs;
            List<LanguageID> languageIDs = content.LanguageIDs;

            List<string> characters = NodeUtilities.CollectCharacters(studyItems, targetLanguageID);
            int characterCount = characters.Count();
            int characterIndex;
            string destTildeUrl = content.MediaTildeUrl + "/";
            BaseObjectContent oldContent;
            MultiLanguageItem oldStudyItem;

            for (characterIndex = 0; characterIndex < characterCount; characterIndex++)
            {
                string character = characters[characterIndex];

                if (studyList.FindStudyItem(character, targetLanguageID) != null)
                    continue;

                if (NodeUtilities.StringExistsInPriorContentCheck(
                        content,
                        character,
                        targetLanguageID,
                        out oldContent,
                        out oldStudyItem))
                    continue;

                if (IsExcludePrior)
                {
                    if (NodeUtilities.StringExistsInPriorLessonsCheck(
                            content,
                            character,
                            targetLanguageID,
                            out oldContent,
                            out oldStudyItem))
                        continue;
                }

                string studyItemKey = studyList.AllocateStudyItemKey();
                MultiLanguageItem targetStudyItem = new MultiLanguageItem(
                    studyItemKey,
                    languageIDs);

                LanguageItem targetLanguageItem = targetStudyItem.LanguageItem(targetLanguageID);
                targetLanguageItem.Text = character;
                TextRun targetRun = new TextRun(0, character.Length, null);
                targetLanguageItem.SentenceRuns = new List<TextRun>() { targetRun };

                DictionaryEntry dictionaryEntry = null;
                List<DictionaryEntry> dictionaryEntries = NodeUtilities.LookupLocalOrRemoteDictionaryEntries(
                    character,
                    targetLanguageIDs,
                    hostLanguageIDs);

                if ((dictionaryEntries != null) && (dictionaryEntries.Count() != 0))
                {
                    dictionaryEntry = dictionaryEntries.First();
                    int senseCount = dictionaryEntry.SenseCount;
                    int senseIndex;

                    if (dictionaryEntry.AlternateCount != 0)
                    {
                        foreach (LanguageString alternate in dictionaryEntry.Alternates)
                        {
                            LanguageID alternateLanguageID = alternate.LanguageID;

                            if (targetLanguageIDs.Contains(alternateLanguageID))
                            {
                                LanguageItem alternateLanguageItem = targetStudyItem.LanguageItem(alternateLanguageID);

                                if (!alternateLanguageItem.HasText())
                                {
                                    string alternateText = alternate.Text;

                                    if (LanguageLookup.IsAlternatePhonetic(alternateLanguageID))
                                        ConvertPinyinNumeric.ToToneMarks(out alternateText, alternateText);

                                    alternateLanguageItem.Text = alternateText;
                                }
                            }
                        }
                    }

                    for (senseIndex = 0; senseIndex < senseCount; senseIndex++)
                    {
                        Sense sense = dictionaryEntry.GetSenseIndexed(senseIndex);

                        foreach (LanguageID languageID in hostLanguageIDs)
                        {
                            LanguageItem hostLanguageItem = targetStudyItem.LanguageItem(languageID);

                            if (sense.HasLanguage(languageID))
                            {
                                string definitionString = sense.GetDefinition(languageID, false, false);

                                definitionString = definitionString.Replace(" / ", ", ");

                                if (!hostLanguageItem.HasText())
                                    hostLanguageItem.Text = definitionString;
                            }
                        }
                    }
                }

                if (IsTranslateMissingItems)
                    LanguageUtilities.Translator.TranslateMultiLanguageItem(
                        targetStudyItem,
                        languageIDs,
                        false,
                        false,
                        out errorMessage,
                        false);

                // Need content set before getting media.
                studyList.AddStudyItem(targetStudyItem);

                if (IsIncludeMedia)
                {
                    NodeUtilities.GetMediaForStudyItem(
                        targetStudyItem,
                        languageIDs,
                        true,
                        false,
                        ApplicationData.IsMobileVersion || ApplicationData.IsTestMobileVersion,
                        IsLookupDictionaryAudio,
                        IsSynthesizeMissingAudio,
                        IsForceAudio,
                        IsLookupDictionaryPictures);
                }

                /*
                NodeUtilities.AddStudyItemToDictionary(
                    node,
                    content,
                    targetStudyItem,
                    languageIDs,
                    languageIDs,
                    LexicalCategory.Unknown,
                    String.Empty,
                    false,
                    false,
                    ref errorMessage);
                */
            }

            if (SubDivide)
                NodeUtilities.SubDivideStudyList(studyList, TitlePrefix, Master,
                    true, StudyItemSubDivideCount, MinorSubDivideCount, MajorSubDivideCount);

            NodeUtilities.UpdateContentHelper(content);
        }

        protected void WriteExpansionStudyItems(BaseObjectNode node,
            BaseObjectContent sourceContent, List<MultiLanguageItem> studyItems,
            bool isDeleteBefore)
        {
        }

        protected BaseObjectContent TargetVocabularyContentCreateCheck(
            BaseObjectNode node,
            BaseObjectContent sourceContent,
            string targetContentKeyBase,
            bool isDeleteBefore)
        {
            string sourceContentKey = sourceContent.KeyString;
            string sourceContentType = sourceContent.ContentType;
            string contentKeySuffix = sourceContentKey.Substring(sourceContentType.Length);
            string nodeContentKey = targetContentKeyBase + contentKeySuffix;
            BaseObjectContent contentParent = null;

            if (sourceContent.HasContentParent())
            {
                BaseObjectContent sourceContentParent = sourceContent.ContentParent;
                string sourceContentParentKey = sourceContentParent.KeyString;
                string sourceContentParentType = sourceContentParent.ContentType;
                string parentContentKeySuffix = sourceContentParentKey.Substring(sourceContentType.Length);
                string parentNodeContentKey = targetContentKeyBase + parentContentKeySuffix;
                contentParent = node.GetContent(parentNodeContentKey);

                if (contentParent == null)
                    throw new Exception("Content parent doesn't exist: " + parentNodeContentKey);
            }

            BaseObjectContent content = CreateVocabularyContentCheck(
                node, contentParent, targetContentKeyBase, nodeContentKey, isDeleteBefore);

            if (content == null)
                throw new Exception("TargetVocabularyContentCreateCheck: Missing content: " + nodeContentKey);

            return content;
        }

        public static string TextInputFormatHelp = "Select the text input format to use.";
        public static string ExtractSentencesHelp = "Check this to output sentences.";
        public static string ExtractWordsHelp = "Check this to output words.";
        public static string ExtractCharactersHelp = "Check this to output characters.";
        public static string ExtractExpansionHelp = "Check this to output expansion content.";
        public static string SentencesTargetKeyNameHelp = "Select sentences target content key.";
        public static string WordsTargetKeyNameHelp = "Select words target content key.";
        public static string CharactersTargetKeyNameHelp = "Select characters target content key.";
        public static string ExpansionTargetKeyNameHelp = "Select expansion target content key.";

        public override void LoadFromArguments()
        {
            Arrangement = GetStringListArgumentDefaulted("TextInputFormat", "stringlist", "r", "Line",
                GetTextInputFormatNames(), "Text input format", TextInputFormatHelp);

            if (String.IsNullOrEmpty(Arrangement))
                Arrangement = "Line";

            DeleteBeforeExport = GetFlagArgumentDefaulted("DeleteBeforeExport", "flag", "w", DeleteBeforeExport,
                "Delete before export", DeleteBeforeExportHelp, null, null);

            base.LoadFromArguments();

            ExtractSentences = GetFlagArgumentDefaulted("ExtractSentences", "flag", "rw", ExtractSentences, "Extract sentences",
                ExtractSentencesHelp, null, null);
            SentencesTargetKeyName = GetStringListArgumentDefaulted("SentencesTargetKeyName", "stringlist", "rw", SentencesTargetKeyName,
                GetTargetKeys("Sentences"), "Sentences target key name", ExtractSentencesHelp);
            ExtractWords = GetFlagArgumentDefaulted("ExtractWords", "flag", "rw", ExtractWords, "Extract words",
                ExtractWordsHelp, null, null);
            WordsTargetKeyName = GetStringListArgumentDefaulted("WordsTargetKeyName", "stringlist", "rw", WordsTargetKeyName,
                GetTargetKeys("Words"), "Words target key name", ExtractWordsHelp);
            ExtractCharacters = GetFlagArgumentDefaulted("ExtractCharacters", "flag", "rw", ExtractCharacters, "Extract characters",
                ExtractCharactersHelp, null, null);
            CharactersTargetKeyName = GetStringListArgumentDefaulted("CharactersTargetKeyName", "stringlist", "rw", CharactersTargetKeyName,
                GetTargetKeys("Characters"), "Characters target key name",
                ExtractCharactersHelp);
            /*
            ExtractExpansion = GetFlagArgumentDefaulted("ExtractExpansion", "flag", "rw", ExtractExpansion, "Extract expansion",
                ExtractExpansionHelp, null, null);
            ExpansionTargetKeyName = GetStringListArgumentDefaulted("ExpansionTargetKeyName", "stringlist", "rw", ExpansionTargetKeyName,
                GetTargetKeys("Expansion"), "Expansion target key name",
                ExtractExpansionHelp);
            */

            IsIncludeMedia = GetFlagArgumentDefaulted("IsIncludeMedia", "flag", "rw", IsIncludeMedia, "Include media",
                IsIncludeMediaHelp, null, null);

            IsLookupDictionaryAudio = GetFlagArgumentDefaulted("IsLookupDictionaryAudio", "flag", "rw", IsLookupDictionaryAudio, "Lookup dictionary audio",
                IsLookupDictionaryAudioHelp, null, null);
            IsLookupDictionaryPictures = GetFlagArgumentDefaulted("IsLookupDictionaryPictures", "flag", "rw", IsLookupDictionaryPictures, "Lookup dictionary pictures",
                IsLookupDictionaryPicturesHelp, null, null);

            IsSynthesizeMissingAudio = GetFlagArgumentDefaulted("IsSynthesizeMissingAudio", "flag", "rw", IsSynthesizeMissingAudio, "Synthesize missing audio",
                IsSynthesizeMissingAudioHelp, null, null);

            FixupArguments();

            if (NodeUtilities != null)
                Label = NodeUtilities.GetLabelFromContentType(DefaultContentType, DefaultContentSubType);
        }

        public override void SaveToArguments()
        {
            SetStringListArgument("TextInputFormat", "stringlist", "r", Arrangement,
                GetTextInputFormatNames(), "Text input format", TextInputFormatHelp);

            SetFlagArgument("DeleteBeforeExport", "flag", "w", DeleteBeforeExport, "Delete before export",
                DeleteBeforeExportHelp, null, null);

            base.SaveToArguments();

            SetFlagArgument("ExtractSentences", "flag", "rw", ExtractSentences, "Extract sentences",
                ExtractSentencesHelp, null, null);
            SetStringListArgument("SentencesTargetKeyName", "stringlist", "rw", SentencesTargetKeyName,
                GetTargetKeys("Sentences"), "Sentences target key name",
                ExtractSentencesHelp);

            SetFlagArgument("ExtractWords", "flag", "rw", ExtractWords, "Extract words",
                ExtractWordsHelp, null, null);
            SetStringListArgument("WordsTargetKeyName", "stringlist", "rw", WordsTargetKeyName,
                GetTargetKeys("Words"), "Words target key name",
                ExtractWordsHelp);
            SetFlagArgument("ExtractCharacters", "flag", "rw", ExtractCharacters, "Extract characters",
                ExtractCharactersHelp, null, null);
            SetStringListArgument("CharactersTargetKeyName", "stringlist", "rw", CharactersTargetKeyName,
                GetTargetKeys("Characters"), "Characters target key name",
                ExtractCharactersHelp);
            /*
            SetFlagArgument("ExtractExpansion", "flag", "rw", ExtractExpansion, "Extract expansion",
                ExtractExpansionHelp, null, null);
            SetStringListArgument("ExpansionTargetKeyName", "stringlist", "rw", ExpansionTargetKeyName,
                GetTargetKeys("Expansion"), "Expansion target key name",
                ExtractExpansionHelp);
            */

            SetFlagArgument("IsIncludeMedia", "flag", "rw", IsIncludeMedia, "Include media",
                IsIncludeMediaHelp, null, null);

            SetFlagArgument("IsLookupDictionaryAudio", "flag", "rw", IsLookupDictionaryAudio, "Lookup dictionary audio",
                IsLookupDictionaryAudioHelp, null, null);
            SetFlagArgument("IsLookupDictionaryPictures", "flag", "rw", IsLookupDictionaryPictures, "Lookup dictionary pictures",
                IsLookupDictionaryPicturesHelp, null, null);

            SetFlagArgument("IsSynthesizeMissingAudio", "flag", "rw", IsSynthesizeMissingAudio, "Synthesize missing audio",
                IsSynthesizeMissingAudioHelp, null, null);

            FixupArguments();
        }

        protected void FixupArguments()
        {
            FormatArgument argument = FindArgument("Pattern");

            if (argument != null)
                argument.Direction = "r";

            argument = FindArgument("UseComments");

            if (argument != null)
                argument.Direction = "r";

            argument = FindArgument("CommentPrefix");

            if (argument != null)
                argument.Direction = "r";

            argument = FindArgument("Ordinal");

            if (argument != null)
                argument.Direction = "";

            argument = FindArgument("IsTranslateMissingItems");

            if (argument != null)
                argument.Direction = "rw";

            argument = FindArgument("IsExcludePrior");

            if (argument != null)
                argument.Direction = "rw";
        }

        public List<string> GetTextInputFormatNames()
        {
            List<string> names = new List<string>() { "Line", "Block" };
            return names;
        }

        public List<string> GetTargetKeys(string label)
        {
            List<string> keys = new List<string>();

            if ((Targets != null) && (Targets.Count != 0))
            {
                foreach (IBaseObject target in Targets)
                {
                    if (target is BaseObjectNode)
                    {
                        BaseObjectNode node = target as BaseObjectNode;
                        GetNodeTargetKeys(node, label, keys);
                    }
                    else if (target is BaseObjectContent)
                    {
                        BaseObjectContent content = target as BaseObjectContent;
                        GetContentTargetKeys(content, label, keys);
                    }
                }
            }
            else if (NodeSource != null)
                GetNodeTargetKeys(NodeSource, label, keys);
            else if (TreeSource != null)
                GetNodeTargetKeys(TreeSource, label, keys);
            else if (!keys.Contains(label))
                keys.Add(label);

            if (keys.Count() == 0)
                keys.Add(label);

            return keys;
        }

        public void GetNodeTargetKeys(BaseObjectNode node, string label, List<string> keys)
        {
            if (node.ContentCount() != 0)
            {
                foreach (BaseObjectContent content in node.ContentList)
                    GetContentTargetKeys(content, label, keys);
            }
        }

        public void GetContentTargetKeys(BaseObjectContent content, string label, List<string> keys)
        {
            if ((content.ContentClass == ContentClassType.StudyList) &&
                (content.Label == label))
            {
                string key = content.KeyString;

                if (!keys.Contains(key))
                    keys.Add(key);
            }

            if (content.HasContentChildren())
            {
                foreach (BaseObjectContent contentChild in content.ContentChildren)
                {
                    if (contentChild.KeyString == content.KeyString)
                        continue;
                    GetContentTargetKeys(contentChild, label, keys);
                }
            }
        }

        public override void DumpArguments(string label)
        {
            base.DumpArguments(label);
        }

        public static new bool IsSupportedStatic(string importExport, string contentName, string capability)
        {
            switch (contentName)
            {
                case "BaseObjectNodeTree":
                case "BaseObjectNode":
                case "BaseObjectContent":
                case "ContentStudyList":
                case "ContentMediaList":
                case "ContentMediaItem":
                    if (capability == "Support")
                        return true;
                    else if ((importExport == "Export") && ((capability == "UseFlags") || (capability == "Content")))
                        return true;
                    else if ((importExport == "Import") && (capability == "Text"))
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

        public static new string TypeStringStatic { get { return "Extract"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
