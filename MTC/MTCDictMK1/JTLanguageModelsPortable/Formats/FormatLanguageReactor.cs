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
using JTLanguageModelsPortable.Tables;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatLanguageReactor : Format
    {
        // Parameters.
        public static bool DoText { get; set; }
        public static bool DoSentences { get; set; }
        public static bool DoWords { get; set; }
        public static bool AddAsChildContent { get; set; }
        public string Package { get; set; }
        public bool IncludeAudio { get; set; }
        public bool IncludePicture { get; set; }
        public bool IncludeMedia { get { return IncludeAudio || IncludePicture; } }
        public string Tag { get; set; }
        public static List<string> Tags = new List<string>() { "(any)", "green", "yellow", "red", "blue", "phrases" };
        public bool HighlightWord { get; set; }
        public LanguageID TransliterationLanguageID { get; set; }

        // Implementation.
        protected DataTable Table;
        protected ContentStudyList StudyList;
        protected List<MultiLanguageItem> StudyItems;
        private List<string> MediaFiles;
        private string ContentFilePath;
        private string MediaFilePath;
        public string ContentType { get; set; }

        // Table format.

        // Column field names.
        const string KeyItemKey = "Item key";
        const string KeyItemType = "Item type";
        const string KeySubtitle = "Subtitle";
        const string KeyTranslation = "Translation";
        const string KeyWord = "Word";
        const string KeyLemma = "Lemma";
        const string KeyPartOfSpeech = "Part of speech";
        const string KeyTags = "Tags";
        const string KeyWordDefinition = "Word definition";
        const string KeySource = "Source";
        const string KeyLanguage = "Language";
        const string KeyTranslationLanguage = "Translation language";
        const string KeyWordTransliteration = "Word transliteration";
        const string KeyPhraseTransliteration = "Phrase transliteration";
        const string KeySubtitleIndex = "Subtitle index";
        const string KeyVideoID = "Video ID";
        const string KeyVideoTitle = "Video title";
        const string KeyDateCreated = "Date created";
        const string KeyContext = "Context";
        const string KeyContextMachineTranslation = "Context machine translation";
        const string KeyContextHumanTranslation = "Context human translation";
        const string KeyPreviousImage = "Previous image";
        const string KeyNextImage = "Next image";
        const string KeySubtitleAudio = "Subtitle audio";

        // This array determines the columns for a Language reactor CSV file embedded in the.zip.
        public static List<DataColumn> LanguageReactorColumns = new List<DataColumn>()
        {
            new DataColumn(KeyItemKey, "", DataType.String),
            new DataColumn(KeyItemType, "", DataType.String),
            new DataColumn(KeySubtitle, "", DataType.String),
            new DataColumn(KeyTranslation, "", DataType.String),
            new DataColumn(KeyWord, "", DataType.String),
            new DataColumn(KeyLemma, "", DataType.String),
            new DataColumn(KeyPartOfSpeech, "", DataType.String),
            new DataColumn(KeyTags, "", DataType.String),
            new DataColumn(KeyWordDefinition, "", DataType.String),
            new DataColumn(KeySource, "", DataType.String),
            new DataColumn(KeyLanguage, "", DataType.String),
            new DataColumn(KeyTranslationLanguage, "", DataType.String),
            new DataColumn(KeyWordTransliteration, "", DataType.String),
            new DataColumn(KeyPhraseTransliteration, "", DataType.String),
            new DataColumn(KeySubtitleIndex, 0, DataType.Integer),
            new DataColumn(KeyVideoID, "", DataType.String),
            new DataColumn(KeyVideoTitle, "", DataType.String),
            new DataColumn(KeyDateCreated, "", DataType.String),
            new DataColumn(KeyContext, "", DataType.String),
            new DataColumn(KeyContextMachineTranslation, "", DataType.String),
            new DataColumn(KeyContextHumanTranslation, "", DataType.String),
            new DataColumn(KeyPreviousImage, "", DataType.String),
            new DataColumn(KeyNextImage, "", DataType.String),
            new DataColumn(KeySubtitleAudio, "", DataType.String)
        };

        // This object determines the column format of the CSV table.
        public DataFormat LanguageReactorTableFormat = new DataFormat("Language Reactor", LanguageReactorColumns, '\t');

        // Format description.
        private static string FormatDescription = "Format for importing words, sentences, and media from"
            + " output from the Chrome plug-in \"Language Reactor\".";

        public FormatLanguageReactor()
            : base(
                  "LanguageReactor",
                  "FormatLanguageReactor",
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
            ClearFormatLanguageReactor();
        }

        public FormatLanguageReactor(FormatLanguageReactor other)
            : base(other)
        {
            CopyFormatLanguageReactor(other);
        }

        public FormatLanguageReactor(
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
            ClearFormatLanguageReactor();
        }

        public void ClearFormatLanguageReactor()
        {
            // Local parameters.

            DoText = true;
            DoSentences = true;
            DoWords = true;
            AddAsChildContent = false;
            Package = String.Empty;
            Tag = String.Empty;
            HighlightWord = true;

            // Base parameters.

            IncludeAudio = true;
            IncludePicture = false;
            DefaultContentType = "Words";
            DefaultContentSubType = "Vocabulary";
            Label = "Words";

            // Implementation.

            StudyList = null;
            StudyItems = null;
        }

        public void CopyFormatLanguageReactor(FormatLanguageReactor other)
        {
            ClearFormatLanguageReactor();
        }

        public override Format Clone()
        {
            return new FormatLanguageReactor(this);
        }

        private string globalMediaDir;
        private string globalUserMediaDir;
        private string globalUserMediaDirLower;
        private string externalUserPathNode;
        private string externalUserPath;
        private string externalUserPathLower;
        private List<string> directoriesToDelete;
        private List<string> DefaultContentSelectors = new List<string>() { "Text", "Sentences", "Words" };
        private List<string> ContentSelectors = new List<string>();

        public override void Read(Stream stream)
        {
            int b1 = stream.ReadByte();
            int b2 = stream.ReadByte();
            string userName = (UserRecord != null ? UserRecord.UserName : "Guest");
            string dir = ApplicationData.TempPath + ApplicationData.PlatformPathSeparator +
                userName + ApplicationData.PlatformPathSeparator;
            string filePath;
            string mediaDir = String.Empty;
            bool isZip = false;

            if ((b1 == 'P') && (b2 == 'K'))
                isZip = true;

            if (DeleteBeforeImport)
            {
                StudyList = GetTargetStudyList();

                if (StudyList != null)
                {
                    NodeUtilities.DeleteContentChildrenHelper(StudyList.Content);
                    NodeUtilities.DeleteStudyListContentsHelper(StudyList.Content, true);
                }
            }

            if (TargetLanguageIDs.Count() > 1)
                TransliterationLanguageID = TargetLanguageIDs.Last();
            else
                TransliterationLanguageID = null;

            if ((Targets != null) && (Targets.Count() == 1) && (Targets[0] is BaseObjectTitled))
            {
                BaseObjectTitled titledObject = Targets[0] as BaseObjectTitled;

                if (titledObject is BaseObjectNode)
                {
                    BaseObjectNode node = titledObject as BaseObjectNode;

                    foreach (string selector in DefaultContentSelectors)
                    {
                        BaseObjectContent content = node.GetContent(selector);

                        if (content != null)
                        {
                            if (AddAsChildContent)
                                content = AddStudyContentChildReplaceTarget(content);

                            if (content.ContentType == "Sentences")
                                mediaDir = content.MediaDirectoryPath;

                            ContentSelectors.Add(content.KeyString);
                        }
                    }
                }
                else if (titledObject is BaseObjectContent)
                {
                    if (AddAsChildContent)
                    {
                        BaseObjectContent contentParent = titledObject as BaseObjectContent;
                        titledObject = AddStudyContentChildReplaceTarget(contentParent);
                    }

                    ContentSelectors.Add(titledObject.KeyString);
                    mediaDir = titledObject.MediaDirectoryPath;
                }
                else
                {
                    ContentSelectors.Add(titledObject.KeyString);
                    mediaDir = titledObject.MediaDirectoryPath;
                }

                mediaDir += ApplicationData.PlatformPathSeparator;
            }
            else if (!String.IsNullOrEmpty(TargetMediaDirectory))
                mediaDir = TargetMediaDirectory + ApplicationData.PlatformPathSeparator;
            else
                mediaDir = globalUserMediaDir;

            if (!isZip)
                ReadTSV(stream);
            else
            {
                globalMediaDir = ApplicationData.MediaPath + ApplicationData.PlatformPathSeparator;
                globalUserMediaDir = globalMediaDir + userName + ApplicationData.PlatformPathSeparator;
                globalUserMediaDirLower = globalUserMediaDir.ToLower();
                externalUserPathNode = "externalUser/";
                externalUserPath = externalUserPathNode;
                externalUserPathLower = externalUserPath.ToLower();
                directoriesToDelete = new List<string>();

                MediaFilePath = mediaDir;
                ContentFilePath = mediaDir + "items.csv";
                MediaFiles = new List<string>();

                stream.Seek(0, SeekOrigin.Begin);

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

            PostProcess();
        }

        public bool HandleReadFile(string filePath, ref string baseDirectory)
        {
            string tsvFileName = "items.csv";
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
            else if (IncludeMedia)
            {
                if (filePath.EndsWith("/"))
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
                    string fileExt = MediaUtilities.GetFileExtension(fileName);
                    string mimeType = MediaUtilities.GetMimeTypeFromFileName(fileName);

                    if (MediaUtilities.IsSupportedPictureMimeType(mimeType))
                    {
                        if (!IncludePicture)
                            return returnValue;
                    }
                    else if (MediaUtilities.IsSupportedAudioMimeType(mimeType))
                    {
                        if (!IncludeAudio)
                            return returnValue;
                    }
                    else
                    {
                        PutMessage("Ignoring unexpected media type: " + mimeType + " (" + fileExt + ")");
                        return returnValue;
                    }

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
            BaseObjectNode node = null;
            BaseObjectContent content = null;
            bool isNodeTarget = false;

            UpdateProgressElapsed("Reading .csv file ...");

            Table = new DataTable("CSV", LanguageReactorTableFormat);

            if (!Table.Read(stream, false, false))
            {
                PutError("Error reading CSV table.");
                return;
            }

            int rowCount = Table.RowCount();
            int rowIndex;

            if (rowCount == 0)
            {
                PutMessage("Empty CSV table");
                return;
            }

            foreach (string selector in ContentSelectors)
            {
                if ((Targets != null) && (Targets.Count() != 0))
                {
                    IBaseObject target = Targets[0];

                    if ((target is BaseObjectContent) || (target is ContentStudyList))
                    {
                        content = target as BaseObjectContent;
                        node = content.Node;
                    }
                    else if (target is BaseObjectNode)
                    {
                        isNodeTarget = true;
                        node = target as BaseObjectNode;
                        content = node.GetContent(selector);
                    }
                    else
                        return;

                    Component = target;
                }
                else
                    return;

                Component = StudyList = content.ContentStorageStudyList;
                ContentType = content.ContentType;

                if (isNodeTarget)
                {
                    switch (ContentType)
                    {
                        case "Text":
                            if (!DoText)
                                continue;
                            break;
                        case "Sentences":
                            if (!DoSentences)
                                continue;
                            break;
                        case "Words":
                            if (!DoWords)
                                continue;
                            break;
                        default:
                            continue;
                    }
                }

                if (StudyList != null)
                {
                    for (rowIndex = rowCount - 1; rowIndex >= 0; rowIndex--)
                    {
                        DataRow dataRow = Table.GetRowIndexed(rowIndex);

                        if (!ProcessRow(dataRow))
                            break;
                    }
                }

                //if (HighlightWord && ((ContentType == "Sentences") || (ContentType == "Text")))
                //    DoWordHighlighting(Table, StudyList);
            }
        }

        protected bool ProcessRow(DataRow dataRow)
        {
            string targetText = String.Empty;
            string transliterationText = String.Empty;
            string hostText = String.Empty;
            string mp3FileName = String.Empty;
            string prevImageFileName = String.Empty;
            string nextImageFileName = String.Empty;

            string itemType = dataRow.GetCellValueString(KeyItemType);
            string subtitle = dataRow.GetCellValueString(KeySubtitle);
            string translation = dataRow.GetCellValueString(KeyTranslation);
            string word = dataRow.GetCellValueString(KeyWord);
            string lemma = dataRow.GetCellValueString(KeyLemma);
            string partOfSpeech = dataRow.GetCellValueString(KeyPartOfSpeech);
            string tags = dataRow.GetCellValueString(KeyTags);
            string wordDefinition = dataRow.GetCellValueString(KeyWordDefinition);
            string wordTransliteration = dataRow.GetCellValueString(KeyWordTransliteration);
            string phraseTransliteration = dataRow.GetCellValueString(KeyPhraseTransliteration);
            string context = dataRow.GetCellValueString(KeyContext);
            string contextMachineTranslation = dataRow.GetCellValueString(KeyContextMachineTranslation);
            string contextHumanTranslation = dataRow.GetCellValueString(KeyContextHumanTranslation);
            string previousImage = dataRow.GetCellValueString(KeyPreviousImage);
            string nextImage = dataRow.GetCellValueString(KeyNextImage);
            string subtitleAudio = dataRow.GetCellValueString(KeySubtitleAudio);

            if (Tag != "(any)")
            {
                if (Tag == "phrases")
                {
                    if (itemType != "Phrase")
                        return true;
                }
                else
                {
                    if (itemType != "Word")
                        return true;

                    if (tags != Tag)
                        return true;
                }
            }

            if (ContentType == "Words")
            {
                if (String.IsNullOrEmpty(word))
                    return true;

                targetText = lemma;
                transliterationText = wordTransliteration;
                hostText = wordDefinition;
                mp3FileName = String.Empty;
                prevImageFileName = String.Empty;
                nextImageFileName = String.Empty;
            }
            else if (ContentType == "Sentences")
            {
                if (String.IsNullOrEmpty(subtitle))
                    return true;

                targetText = subtitle;
                transliterationText = phraseTransliteration;
                hostText = translation;
                mp3FileName = subtitleAudio;
                prevImageFileName = previousImage;
                nextImageFileName = nextImage;
            }
            else if (ContentType == "Text")
            {
                if (String.IsNullOrEmpty(context))
                    return true;

                targetText = context;
                transliterationText = String.Empty;

                if (!String.IsNullOrEmpty(contextHumanTranslation))
                    hostText = contextHumanTranslation;
                else
                    hostText = contextMachineTranslation;

                mp3FileName = String.Empty;
                prevImageFileName = String.Empty;
                nextImageFileName = String.Empty;
            }

            if (HasDuplicate(targetText))
                return true;

            if (HighlightWord && ((ContentType == "Sentences") || (ContentType == "Text")))
            {
                if (!String.IsNullOrEmpty(targetText) && !String.IsNullOrEmpty(word))
                    targetText = targetText.Replace(word, " *" + word + "* ");

                if (!String.IsNullOrEmpty(transliterationText) && !String.IsNullOrEmpty(wordTransliteration))
                    transliterationText = transliterationText.Replace(wordTransliteration, " *" + wordTransliteration + "* ");

                if (!String.IsNullOrEmpty(hostText) && !String.IsNullOrEmpty(wordDefinition))
                    hostText = hostText.Replace(wordDefinition, " *" + wordDefinition + "* ");
            }

            bool returnValue = ProcessEntry(
                targetText,
                transliterationText,
                hostText,
                mp3FileName,
                prevImageFileName,
                nextImageFileName);

            return returnValue;
        }

        protected bool HasDuplicate(string targetText)
        {
            if (StudyItems == null)
                return false;

            LanguageID targetLanguageID = TargetLanguageID;

            foreach (MultiLanguageItem studyItem in StudyItems)
            {
                string text = studyItem.Text(targetLanguageID).Replace(" *", "").Replace("* ", "");

                if (text == targetText)
                    return true;
            }

            return false;
        }

        protected bool ProcessEntry(
            string targetText,
            string transliterationText,
            string hostText,
            string mp3FileName,
            string prevImageFileName,
            string nextImageFileName)
        {
            BaseObjectContent content = StudyList.Content;
            MultiLanguageItem studyItem;
            string studyItemKey = StudyList.AllocateStudyItemKey();

            studyItem = new MultiLanguageItem(studyItemKey, StudyList.Content.LanguageIDs);

            OverwriteText(
                studyItem,
                targetText,
                transliterationText,
                hostText,
                mp3FileName,
                prevImageFileName,
                nextImageFileName);

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
            string transliterationText,
            string hostText,
            string mp3FileName,
            string prevImageFileName,
            string nextImageFileName)
        {
            if (String.IsNullOrEmpty(targetText) && String.IsNullOrEmpty(hostText))
                return;

            int languageIndex;
            int languageCount = TargetRomanizationHostLanguageIDs.Count();

            for (languageIndex = 0; languageIndex < languageCount; languageIndex++)
            {
                LanguageID languageID = TargetRomanizationHostLanguageIDs[languageIndex];
                LanguageItem languageItem = studyItem.LanguageItem(languageID);
                string text;

                if (languageIndex == 0)
                    text = targetText;
                else if (languageIndex == languageCount - 1)
                    text = hostText;
                else if (languageID == TransliterationLanguageID)
                    text = transliterationText;
                else
                    continue;

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

                    if (languageIndex == 0)
                    {
                        if (IncludeAudio && !String.IsNullOrEmpty(mp3FileName))
                        {
                            MediaRun fileMediaRun = new MediaRun("Audio", mp3FileName, Owner);

                            if (mediaRuns == null)
                                mediaRuns = new List<MediaRun>();

                            mediaRuns.Add(fileMediaRun);
                        }

                        if (IncludePicture)
                        {
                            if (!String.IsNullOrEmpty(prevImageFileName))
                            {
                                MediaRun imageMediaRun = new MediaRun("Picture", prevImageFileName, Owner);

                                if (mediaRuns == null)
                                    mediaRuns = new List<MediaRun>();

                                mediaRuns.Add(imageMediaRun);
                            }

                            if (!String.IsNullOrEmpty(nextImageFileName))
                            {
                                MediaRun imageMediaRun = new MediaRun("Picture", nextImageFileName, Owner);

                                if (mediaRuns == null)
                                    mediaRuns = new List<MediaRun>();

                                mediaRuns.Add(imageMediaRun);
                            }
                        }
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
            string output = text;
            return output;
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
                languageItem.AutoResetWordRuns(Repositories.Dictionary);
        }

        protected void DoWordHighlighting(DataTable table, ContentStudyList studyList)
        {
            int rowCount = Table.RowCount();
            int rowIndex;

            for (rowIndex = rowCount - 1; rowIndex >= 0; rowIndex--)
            {
                DataRow dataRow = Table.GetRowIndexed(rowIndex);
                string targetText = dataRow.GetCellValueString(KeyWord);
                string transliterationText = dataRow.GetCellValueString(KeyWordTransliteration);
                string hostText = dataRow.GetCellValueString(KeyWordDefinition);

                if (String.IsNullOrEmpty(targetText))
                    continue;

                foreach (MultiLanguageItem studyItem in StudyList.StudyItems)
                {
                    LanguageItem targetLanguageItem = studyItem.LanguageItem(TargetLanguageID);
                    LanguageItem transliterationLanguageItem = studyItem.LanguageItem(TransliterationLanguageID);
                    LanguageItem hostLanguageItem = studyItem.LanguageItem(HostLanguageID);
                    string targetNew = " *" + targetText + "* ";
                    string transliterationNew = " *" + transliterationText + "* ";
                    string hostNew = " *" + hostText + "* ";

                    if ((targetLanguageItem != null) && !targetLanguageItem.Text.Contains(targetNew))
                        targetLanguageItem.Text = targetLanguageItem.Text.Replace(targetText, targetNew);

                    if ((transliterationLanguageItem != null) && !transliterationLanguageItem.Text.Contains(targetNew))
                        transliterationLanguageItem.Text = transliterationLanguageItem.Text.Replace(transliterationText, transliterationNew);

                    if ((hostLanguageItem != null) && !hostLanguageItem.Text.Contains(hostNew))
                        hostLanguageItem.Text = hostLanguageItem.Text.Replace(hostText, hostNew);
                }
            }
        }

        public void PostProcess()
        {
            List<LanguageID> languageIDs = UniqueLanguageIDs;
            bool isOK = true;

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

            if (IsSynthesizeMissingAudio && isOK && (StudyItems != null))
            {
                BaseObjectContent content = StudyList.Content;
                BaseObjectNode node = content.NodeOrTree;
                BaseObjectNodeTree tree = content.Tree;

                UpdateProgressElapsed("Generating speech ...");

                List<LanguageID> rootLanguageIDs = LanguageLookup.GetFamilyRootLanguageIDs(TargetLanguageIDs);

                foreach (LanguageID languageID in rootLanguageIDs)
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
                            -2,
                            IsForceAudio,
                            true))
                        isOK = false;
                }
            }

            if (SubDivide)
                DoSubDivide();

            NodeUtilities.UpdateTreeCheck(StudyList.Tree, false, false);
        }

        protected void DoSubDivide()
        {
            BaseObjectNode node = null;
            BaseObjectContent content = null;

            UpdateProgressElapsed("Subdividing study lists ...");

            foreach (string selector in ContentSelectors)
            {
                if ((Targets != null) && (Targets.Count() != 0))
                {
                    IBaseObject target = Targets[0];

                    if ((target is BaseObjectContent) || (target is ContentStudyList))
                        content = target as BaseObjectContent;
                    else if (target is BaseObjectNode)
                    {
                        node = target as BaseObjectNode;
                        content = node.GetContent(selector);
                    }
                    else
                        return;
                }
                else
                    return;

                Component = StudyList = content.ContentStorageStudyList;

                SubDivideStudyList(StudyList);
            }
        }

        public static string TransliterationLanguageIDHelp = "Select the transliteration language.";
        public static string DoTextHelp = "Check this to load Text content.";
        public static string DoSentencesHelp = "Check this to load Sentences content.";
        public static string DoWordsHelp = "Check this to load Words content.";
        public static string AddAsChildContentHelp = "Check this to add the content as a new content child.";
        public static string IncludeAudioHelp = "Check this to include audio media (ZIP format only).";
        public static string IncludePictureHelp = "Check this to include picture media (ZIP format only).";
        public static string OwnerHelp = "Enter owner user ID.";
        public static string PackageHelp = "Enter package name. This will restrict access to users with the package name set in their account.";
        public static string TagHelp = "Select the tag to filter.";
        public static string HighlightWordHelp = "Check this to highlight the vocabulary word in the phrase.";

        public override void LoadFromArguments()
        {
            base.LoadFromArguments();

            if (TargetLanguageIDs.Count() > 1)
                TransliterationLanguageID = TargetLanguageIDs.Last();

            DeleteBeforeImport = GetFlagArgumentDefaulted("DeleteBeforeImport", "flag", "r", DeleteBeforeImport,
                "Delete before import", DeleteBeforeImportHelp, null, null);

            IncludeAudio = GetFlagArgumentDefaulted("IncludeAudio", "flag", "r", IncludeAudio,
                "Include audio", IncludeAudioHelp, null, null);

            IncludePicture = GetFlagArgumentDefaulted("IncludePicture", "flag", "r", IncludePicture,
                "Include picture", IncludePictureHelp, null, null);

            IsSynthesizeMissingAudio = GetFlagArgumentDefaulted("IsSynthesizeMissingAudio", "flag", "r", IsSynthesizeMissingAudio, "Synthesize missing audio",
                IsSynthesizeMissingAudioHelp, null, null);
            IsForceAudio = GetFlagArgumentDefaulted("IsForceAudio", "flag", "r", IsForceAudio, "Force synthesis of audio",
                IsForceAudioHelp, null, null);

            TransliterationLanguageID = GetLanguageIDArgumentDefaulted("TransliterationLanguageID", "languageID", "r", TransliterationLanguageID,
                "Transliteration language", TransliterationLanguageIDHelp);

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
                    MasterName = GetMasterListArgumentDefaulted("MasterName", "stringlist", "r", MasterName, "Master name", MasterNameHelp);
                    StudyItemSubDivideCount = GetIntegerArgumentDefaulted("StudyItemSubDivideCount", "integer", "r", StudyItemSubDivideCount, "Study items per leaf", StudyItemSubDivideCountHelp);
                    MinorSubDivideCount = GetIntegerArgumentDefaulted("MinorSubDivideCount", "integer", "r", MinorSubDivideCount, "Minor subdivide count", MinorSubDivideCountHelp);
                    MajorSubDivideCount = GetIntegerArgumentDefaulted("MajorSubDivideCount", "integer", "r", MajorSubDivideCount, "Major subdivide count", MinorSubDivideCountHelp);
                    DoText = GetFlagArgumentDefaulted("DoText", "flag", "r", DoText, "Do text", DoTextHelp, null, null);
                    DoWords = GetFlagArgumentDefaulted("DoWords", "flag", "r", DoWords, "Do words", DoWordsHelp, null, null);
                    DoSentences = GetFlagArgumentDefaulted("DoSentences", "flag", "r", DoSentences, "Do sentences", DoSentencesHelp, null, null);
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
                    StudyItemSubDivideCount = GetIntegerArgumentDefaulted("StudyItemSubDivideCount", "integer", "r", StudyItemSubDivideCount, "Study items per leaf", StudyItemSubDivideCountHelp);
                    MinorSubDivideCount = GetIntegerArgumentDefaulted("MinorSubDivideCount", "integer", "r", MinorSubDivideCount, "Minor subdivide count", MinorSubDivideCountHelp);
                    MajorSubDivideCount = GetIntegerArgumentDefaulted("MajorSubDivideCount", "integer", "r", MajorSubDivideCount, "Major subdivide count", MinorSubDivideCountHelp);
                    break;
                default:
                    break;
            }

            AddAsChildContent = GetFlagArgumentDefaulted("AddAsChildContent", "flag", "r", AddAsChildContent, "Add as child content",
                AddAsChildContentHelp, null, null);

            Package = GetArgumentDefaulted("Package", "string", "r", Package, "Package",
                PackageHelp);

            Tag = GetStringListArgumentDefaulted("Tag", "stringlist", "r", Tag, Tags, "Tag",
                TagHelp);

            HighlightWord = GetFlagArgumentDefaulted("HighlightWord", "flag", "r", HighlightWord, "Highlight word",
                HighlightWordHelp, null, null);

            if (NodeUtilities != null)
                Label = NodeUtilities.GetLabelFromContentType(DefaultContentType, DefaultContentSubType);
        }

        public override void SaveToArguments()
        {
            base.SaveToArguments();

            SetFlagArgument("DeleteBeforeImport", "flag", "r", DeleteBeforeImport, "Delete before import",
                DeleteBeforeImportHelp, null, null);

            SetFlagArgument("IncludeAudio", "flag", "r", IncludeAudio,
                "Include audio", IncludeAudioHelp, null, null);

            SetFlagArgument("IncludePicture", "flag", "r", IncludePicture,
                "Include picture", IncludePictureHelp, null, null);

            SetFlagArgument("IsSynthesizeMissingAudio", "flag", "r", IsSynthesizeMissingAudio, "Synthesize missing audio",
                IsSynthesizeMissingAudioHelp, null, null);
            SetFlagArgument("IsForceAudio", "flag", "r", IsForceAudio, "Force synthesis of audio",
                IsForceAudioHelp, null, null);

            SetLanguageIDArgument("TransliterationLanguageID", "languageID", "r", TransliterationLanguageID,
                "Transliteration language", TransliterationLanguageIDHelp);

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
                    SetMasterListArgument("MasterName", "stringlist", "r", MasterName, "Master name", MasterNameHelp);
                    SetIntegerArgument("StudyItemSubDivideCount", "integer", "r", StudyItemSubDivideCount, "Items per leaf", StudyItemSubDivideCountHelp);
                    SetIntegerArgument("MinorSubDivideCount", "integer", "r", MinorSubDivideCount, "Minor subdivide count", MinorSubDivideCountHelp);
                    SetIntegerArgument("MajorSubDivideCount", "integer", "r", MajorSubDivideCount, "Major subdivide count", MinorSubDivideCountHelp);
                    SetFlagArgument("DoText", "flag", "r", DoText, "Do text", DoTextHelp, null, null);
                    SetFlagArgument("DoWords", "flag", "r", DoWords, "Do words", DoWordsHelp, null, null);
                    SetFlagArgument("DoSentences", "flag", "r", DoSentences, "Do sentences", DoSentencesHelp, null, null);
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
                    SetMasterListArgument("MasterName", "stringlist", "r", MasterName, "Master name", MasterNameHelp);
                    SetIntegerArgument("StudyItemSubDivideCount", "integer", "r", StudyItemSubDivideCount, "Items per leaf", StudyItemSubDivideCountHelp);
                    SetIntegerArgument("MinorSubDivideCount", "integer", "r", MinorSubDivideCount, "Minor subdivide count", MinorSubDivideCountHelp);
                    SetIntegerArgument("MajorSubDivideCount", "integer", "r", MajorSubDivideCount, "Major subdivide count", MinorSubDivideCountHelp);
                    break;
                default:
                    break;
            }

            SetFlagArgument("AddAsChildContent", "flag", "r", AddAsChildContent, "Add as child content",
                AddAsChildContentHelp, null, null);

            SetArgument("Owner", "string", "r", Owner, "Owner",
                OwnerHelp, null, null);
            SetArgument("Package", "string", "r", Package, "Package",
                PackageHelp, null, null);
            SetStringListArgument("Tag", "stringlist", "r", Tag, Tags, "Tag",
                TagHelp);
            SetFlagArgument("HighlightWord", "flag", "r", HighlightWord, "Highlight word",
                HighlightWordHelp, null, null);
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

        public static new string TypeStringStatic { get { return "LanguageReactor"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
