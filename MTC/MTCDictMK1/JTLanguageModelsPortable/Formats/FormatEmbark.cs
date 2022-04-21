using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Language;

namespace JTLanguageModelsPortable.Formats
{
    public partial class FormatEmbark : Format
    {
        // Arguments.

        public List<string> NodePathPatterns { get; set; }
        public static string NodePathPatternsHelp = "Enter the lesson path patterns for filtering specific lessons or groups."
            + " A lesson path pattern is a three part path in the form \"level/season/lesson\", which translates to \"course/group/lesson\" in JTLanguage."
            + " Use '*' for a wild card, i.e. \"*/*/*\" for all lessons.  Separate multiple paths with a comma or newline."
            + " Append a verse selection like: \":3-5,8\"";

        public bool OutputHierarchy { get; set; }
        public static string OutputHierarchyHelp = "Set this to true to output a node hierarchy."
            + " Otherwise a flat list of nodes is output.";

        public string OutputMediaDirectory { get; set; }
        public static string OutputMediaDirectoryHelp = "Set this to the directory path where the media should be output.";

        //public bool OutputSentenceMedia { get; set; }
        //public static string OutputSentenceMediaHelp = "Set this to true to output stand alone sentence or paragraph audio files.";

        public bool OutputIndented { get; set; }
        public static string OutputIndentedHelp = "Set this to true to tabify the JSON output for human readability.";

        public bool IncludeInsets { get; set; }
        public static string IncludeInsetsHelp = "Set this to true to output insets also.";

        protected bool IsNoAudio { get; set; }
        protected static string IsNoAudioPrompt = "No audio";
        protected static string IsNoAudioHelp = "If checked, won't use audio.";

        // Internal.
        protected string Section;
        protected static char[] NodePathSeparators = { '/' };

        protected List<PathDesignator> PathPatterns;
        protected List<object> WalkerResults;
        protected List<string> UniqueWords = new List<string>();
        protected HashSet<string> UniqueWordsSet = new HashSet<string>();

        public class GlossaryData
        {
            public LanguageID LanguageID;
            public Dictionary<string, DictionaryEntry> EntryDictionary;
            public List<DictionaryEntry> EntryList;
        }

        protected Dictionary<LanguageID, GlossaryData> GlossaryDataCache;
        //protected Dictionary<string, UserRunItem> UserRunItemDictionary;
        //protected List<UserRunItem> UserRunItemList;
        protected int RunOrdinal;       // Used to identify run spans.
        private static string FormatDescription = "Import/Export content in MTC Embark format.";

        protected List<LanguageID> TargetRootLanguageIDs;
        protected List<LanguageID> HostRootLanguageIDs;
        protected List<LanguageID> TargetAllLanguageIDs;
        protected List<LanguageID> HostAllLanguageIDs;

        public FormatEmbark(
                string targetType,
                UserRecord userRecord,
                UserProfile userProfile,
                IMainRepository repositories,
                LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base("Embark", "FormatEmbark", FormatDescription, targetType, "File", "application/json", ".json",
                userRecord, userProfile, repositories, languageUtilities, nodeUtilities)
        {
            ClearFormatEmbark();
        }

        public FormatEmbark(FormatEmbark other)
            : base(other)
        {
            ClearFormatEmbark();
        }

        public FormatEmbark()
            : base("Embark", "FormatEmbark", FormatDescription, String.Empty, "File", "application/json", ".json",
                null, null, null, null, null)
        {
            ClearFormatEmbark();
        }

        // For derived classes.
        public FormatEmbark(string arrangement, string name, string type, string description, string targetType, string importExportType,
                string mimeType, string defaultExtension,
                UserRecord userRecord, UserProfile userProfile, IMainRepository repositories, LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base(name, type, description, targetType, importExportType, mimeType, defaultExtension,
                  userRecord, userProfile, repositories, languageUtilities, nodeUtilities)
        {
            ClearFormatEmbark();
        }

        public void ClearFormatEmbark()
        {
            Ordinal = 0;
            PreserveTargetNames = false;
            Stack = null;
            AllowStudent = false;
            AllowTeacher = false;
            AllowAdministrator = true;

            NodePathPatterns = null;
            OutputHierarchy = false;
            OutputMediaDirectory = null;
            OutputIndented = false;
            IncludeInsets = false;
            IsNoAudio = false;

            Section = null;
            NodePathSeparators = new char[] { '/' };
            PathPatterns = null;
            WalkerResults = null;
            UniqueWords = new List<string>();
            UniqueWordsSet = new HashSet<string>();
        }

        public void CopyFormatEmbark(FormatEmbark other)
        {
            ClearFormatEmbark();
        }

        public override Format Clone()
        {
            return new FormatEmbark(this);
        }

        public override void Read(Stream stream)
        {
            throw new ObjectException("FormatEmbark: Read not implemented.");
        }

        public override void Write(Stream stream)
        {
            if ((Targets == null) || (Targets.Count() == 0))
                throw new Exception("No targets set.");

            bool saveProgressInitialized = IsProgressInitialized;

            if (!IsProgressInitialized)
                InitializeProgress("Write", true, ProgressCountBase);

            StringBuilder sb = new StringBuilder();

            foreach (IBaseObject target in Targets)
            {
                object targetObjectCSharp = CreateTargetCSharp(target);

                if (targetObjectCSharp == null)
                    continue;

                string targetObjectJSON = JsonConvert.SerializeObject(
                    targetObjectCSharp,
                    (OutputIndented ? Formatting.Indented : Formatting.None));
                sb.AppendLine(targetObjectJSON);
            }

            byte[] data = ApplicationData.Global.GetBytesFromStringUTF8(sb.ToString());

            stream.Write(data, 0, data.Length);

            if (!saveProgressInitialized)
                FinishProgress("Write", true);
        }

        public override void InitializeProgress(
            string operation,
            bool doContinue,
            int progressCountBase)
        {
            if (!IsProgressInitialized)
            {
                IsProgressInitialized = true;

                if ((Targets == null) || (Targets.Count() == 0))
                    throw new Exception("No targets set.");

                TargetRootLanguageIDs = LanguageLookup.GetFamilyRootLanguageIDs(TargetLanguageIDs);
                HostRootLanguageIDs = LanguageLookup.GetFamilyRootLanguageIDs(HostLanguageIDs);

                TargetAllLanguageIDs = LanguageLookup.GetLanguagesPlusAlternateLanguageIDs(TargetRootLanguageIDs);
                HostAllLanguageIDs = LanguageLookup.GetLanguagesPlusAlternateLanguageIDs(HostRootLanguageIDs);

                PathPatterns = GetPathDesignators(NodePathPatterns);

                GlossaryDataCache = new Dictionary<LanguageID, GlossaryData>();
                RunOrdinal = 0;

                foreach (LanguageID languageID in TargetRootLanguageIDs)
                {
                    GlossaryData glossaryData = new GlossaryData();

                    glossaryData.LanguageID = languageID;
                    glossaryData.EntryDictionary = new Dictionary<string, DictionaryEntry>(StringComparer.OrdinalIgnoreCase);
                    glossaryData.EntryList = new List<DictionaryEntry>();
                    GlossaryDataCache.Add(languageID, glossaryData);
                }

                foreach (LanguageID languageID in HostRootLanguageIDs)
                {
                    if (GetGlossaryData(languageID) != null)
                        continue;

                    GlossaryData glossaryData = new GlossaryData();

                    glossaryData.LanguageID = languageID;
                    glossaryData.EntryDictionary = new Dictionary<string, DictionaryEntry>(StringComparer.OrdinalIgnoreCase);
                    glossaryData.EntryList = new List<DictionaryEntry>();
                    GlossaryDataCache.Add(languageID, glossaryData);
                }

                int targetLanguageCount = TargetRootLanguageIDs.Count();
                int wordListCount = targetLanguageCount;
                int senseListCount = 0;

                foreach (LanguageID targetLanguageID in TargetRootLanguageIDs)
                {
                    foreach (LanguageID hostLanguageID in HostRootLanguageIDs)
                    {
                        if (hostLanguageID == targetLanguageID)
                            continue;

                        senseListCount++;
                    }
                }

                int nodeCount = 0;

                foreach (IBaseObject target in Targets)
                    nodeCount += GetNodeCounts(target);

                int targetCount = Targets.Count();
                int progressCount = (targetCount * nodeCount * targetLanguageCount) + wordListCount + senseListCount;

                if (doContinue)
                    ContinueProgress(progressCountBase + progressCount);
                else
                {
                    CreateTimerCheckAndStart();
                    ResetProgress();
                    StartProgress(progressCountBase + progressCount);
                }
            }
        }

        public override void FinishProgress(
            string operation,
            bool doContinue)
        {
            if (doContinue)
                EndContinuedProgress();
            else
                EndProgress();

            if (Timer != null)
            {
                Timer.Stop();
                OperationTime = Timer.GetTimeInSeconds();
            }
        }

        public List<PathDesignator> GetPathDesignators(List<string> pathPatterns)
        {
            List<PathDesignator> pathDesignators = PathDesignator.GetPathDesignators(pathPatterns);
            return pathDesignators;
        }

        public int GetNodeCounts(IBaseObject target)
        {
            List<BaseObjectNode> lessons = new List<BaseObjectNode>();
            GetLessons(lessons, target, 0);
            return lessons.Count();
        }

        public object CreateTargetCSharp(IBaseObject target)
        {
            List<Node> nodes = new List<Node>();
            Node targetObject = null;
            List<string> namePath = null;
            string packageKey = String.Empty;
            List<string> targetLanguageCodes = new List<string>();
            List<string> hostLanguageCodes = new List<string>();

            if (OutputHierarchy)
            {
                foreach (LanguageID targetLanguageID in TargetRootLanguageIDs)
                {
                    string targetLanguageCode = LanguageCodes.GetCode_ll_CC_FromCode2(targetLanguageID.LanguageCultureExtensionCode);

                    if (target is BaseObjectNodeTree)
                    {
                        BaseObjectNodeTree tree = target as BaseObjectNodeTree;
                        namePath = tree.GetNamePath(UILanguageID);
                        targetObject = CreateTargetTree(tree, targetLanguageID, HostLanguageID, 0);
                    }
                    else if (target is BaseObjectNode)
                    {
                        BaseObjectNode node = target as BaseObjectNode;

                        namePath = node.GetNamePath(UILanguageID);

                        if (node.IsGroup())
                            targetObject = CreateTargetGroup(node, targetLanguageID, HostLanguageID, 0);
                        else
                            targetObject = CreateTargetLesson(node, targetLanguageID, HostLanguageID, 0);
                    }
                    else if (target is BaseObjectContent)
                    {
                        BaseObjectContent content = target as BaseObjectContent;

                        namePath = content.Node.GetNamePath(UILanguageID);

                        switch (content.ContentClass)
                        {
                            case ContentClassType.StudyList:
                                targetObject = CreateTargetStudyList(content, targetLanguageID, HostLanguageID, 0);
                                break;
                            case ContentClassType.MediaItem:
                                targetObject = CreateTargetMediaItem(content, targetLanguageID, HostLanguageID, 0);
                                break;
                            case ContentClassType.DocumentItem:
                                throw new Exception("Can't export document item contect: " + content.KeyString);
                        }
                    }
                    else
                        throw new Exception("Unexpected target type: " + target.GetType().Name);

                    if (targetObject != null)
                        nodes.Add(targetObject);

                    if (!targetLanguageCodes.Contains(targetLanguageCode))
                        targetLanguageCodes.Add(targetLanguageCode);
                }
            }
            else
            {
                List<BaseObjectNode> lessons = new List<BaseObjectNode>();

                GetLessons(lessons, target, 0);

                BaseObjectNode parent = target as BaseObjectNode;

                if (lessons.Count() > 1)
                    namePath = parent.GetNamePath(UILanguageID);
                else if (lessons.Count() == 1)
                {
                    BaseObjectNode lesson = lessons.First();
                    PathDesignator pathDesignator;

                    namePath = lesson.GetNamePath(UILanguageID);

                    if (MatchNodePath(lesson, -1, out pathDesignator))
                    {
                        if ((pathDesignator != null) && !String.IsNullOrEmpty(pathDesignator.ParagraphSelectionTitle))
                            namePath.Add(pathDesignator.ParagraphSelectionTitle.Replace(":", ""));
                    }
                }

                foreach (BaseObjectNode lesson in lessons)
                {
                    foreach (LanguageID targetLanguageID in TargetRootLanguageIDs)
                    {
                        string targetLanguageCode = LanguageCodes.GetCode_ll_CC_FromCode2(targetLanguageID.LanguageCultureExtensionCode);
                        targetObject = CreateTargetLesson(lesson, targetLanguageID, HostLanguageID, -1);

                        if (targetObject != null)
                            nodes.Add(targetObject);
                    }
                }
            }

            List<WordList> wordLists = new List<WordList>();

            CreateGlossaries(wordLists);

            if (namePath != null)
            {
                int count = namePath.Count();
                int index;

                for (index = 0; index < count; index++)
                {
                    string nodeName = namePath[index];

                    if (index != 0)
                        packageKey += "_";

                    packageKey += MediaUtilities.FileFriendlyName(nodeName);
                }
            }

            foreach (LanguageID targetLanguageID in TargetRootLanguageIDs)
            {
                string targetLanguageCode = LanguageCodes.GetCode_ll_CC_FromCode2(targetLanguageID.LanguageCultureExtensionCode);
                if (!targetLanguageCodes.Contains(targetLanguageCode))
                    targetLanguageCodes.Add(targetLanguageCode);
            }

            Dictionary<string, LanguageCodeToNameTable> languageCodeToNameTables = new Dictionary<string, LanguageCodeToNameTable>();

            foreach (LanguageID hostLanguageID in HostRootLanguageIDs)
            {
                string hostLanguageCode = LanguageCodes.GetCode_ll_CC_FromCode2(hostLanguageID.LanguageCultureExtensionCode);

                if (!hostLanguageCodes.Contains(hostLanguageCode))
                    hostLanguageCodes.Add(hostLanguageCode);

                Dictionary<string, string> mapTable = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (LanguageID targetLanguageID in TargetAllLanguageIDs)
                {
                    string languageCode = LanguageCodes.GetCode_ll_CC_FromCode2(targetLanguageID.LanguageCode) +
                        GetLanguageInstanceSuffix(targetLanguageID);
                    string testName;

                    if (!mapTable.TryGetValue(languageCode, out testName))
                    {
                        string languageName = targetLanguageID.LanguageName(hostLanguageID).Replace("-", " ");
                        mapTable.Add(languageCode, languageName);
                    }
                }

                foreach (LanguageID languageID in HostAllLanguageIDs)
                {
                    string languageCode = LanguageCodes.GetCode_ll_CC_FromCode2(languageID.LanguageCode) +
                        GetLanguageInstanceSuffix(languageID);
                    string testName;

                    if (!mapTable.TryGetValue(languageCode, out testName))
                    {
                        string languageName = languageID.LanguageName(hostLanguageID).Replace("-", " ");
                        mapTable.Add(languageCode, languageName);
                    }
                }

                LanguageCodeToNameTable languageCodeToNameTable = new LanguageCodeToNameTable();
                languageCodeToNameTable.LanguageCode = hostLanguageCode;
                languageCodeToNameTable.Table = mapTable;
                languageCodeToNameTables.Add(hostLanguageCode, languageCodeToNameTable);
            }
            EmbarkPackage package = new EmbarkPackage();
            package.Key = packageKey;
            package.FormatVersion = CurrentFormatVersion;
            package.TargetLanguageCodes = targetLanguageCodes.ToArray();
            package.HostLanguageCodes = hostLanguageCodes.ToArray();
            package.Nodes = nodes.ToArray();
            package.WordLists = wordLists.ToArray();
            package.LanguageCodeToNameTables = languageCodeToNameTables;

            return package;
        }

        protected Node CreateTargetTree(
            BaseObjectNodeTree tree,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            int level)
        {
            Node node = CreateTargetGroup(tree, targetLanguageID, hostLanguageID, level);
            return node;
        }

        protected Node CreateTargetGroup(
            BaseObjectNode group,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            int level)
        {
            PathDesignator pathDesignator = null;

            if ((level != -1) && !MatchNodePath(group, level, out pathDesignator))
                return null;

            if (group.HasChildren())
            {
                Node node = new Node();
                List<Node> children = new List<Node>();
                Node child;
                string nodeKey = MediaUtilities.FileFriendlyName(group.GetTitleString(UILanguageID));

                foreach (BaseObjectNode childNode in group.Children)
                {
                    if (childNode.IsGroup())
                        child = CreateTargetGroup(childNode, targetLanguageID, hostLanguageID, level + 1);
                    else
                        child = CreateTargetLesson(childNode, targetLanguageID, hostLanguageID, level + 1);

                    if (child != null)
                        children.Add(child);
                }

                node.Key = nodeKey;
                node.TargetLanguageCodes = GetFamilyLanguageCodes(targetLanguageID);
                node.HostLanguageCode = GetLanguageCode(hostLanguageID);
                node.Title = GetMultiString(group.Title, targetLanguageID, hostLanguageID, String.Empty);
                node.Description = GetMultiString(group.Description, targetLanguageID, hostLanguageID, String.Empty);
                node.TextUnits = null;
                node.MediaPath = null;
                node.MediaFileName = null;
                node.Children = children.ToArray();

                return node;
            }

            return null;
        }

        protected Node CreateTargetLesson(
            BaseObjectNode lesson,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            int level)
        {
            PathDesignator pathDesignator = null;

            if ((level != -1) && !MatchNodePath(lesson, level, out pathDesignator))
                return null;

            BaseObjectContent textContent = lesson.GetContentWithStorageClass(ContentClassType.StudyList).FirstOrDefault();
            Node node = CreateTargetStudyList(textContent, targetLanguageID, hostLanguageID, level);
            return node;
        }

        protected Node CreateTargetStudyList(
            BaseObjectContent content,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            int level)
        {
            BaseObjectNode lesson = content.Node;
            BaseObjectContent audioContent = null;
            PathDesignator pathDesignator = null;

            if ((level != -1) && !MatchNodePath(lesson, level, out pathDesignator))
                return null;

            if (!IsNoAudio)
            {
                foreach (BaseObjectContent nodeContent in lesson.ContentChildren)
                {
                    if (nodeContent.ContentClass != ContentClassType.MediaItem)
                        continue;

                    ContentMediaItem mediaItem = nodeContent.ContentStorageMediaItem;

                    if (mediaItem == null)
                        continue;

                    if (mediaItem.SourceContentKeys != null)
                    {
                        if (mediaItem.SourceContentKeys.Contains(content.KeyString))
                        {
                            audioContent = mediaItem.Content;
                            break;
                        }
                    }
                }
            }

            Node node = CreateTargetContentLesson(audioContent, content, targetLanguageID, hostLanguageID, level);

            return node;
        }

        protected Node CreateTargetMediaItem(
            BaseObjectContent content,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            int level)
        {
            BaseObjectNode lesson = content.Node;
            BaseObjectContent studyListContent = null;
            ContentMediaItem mediaItem = content.ContentStorageMediaItem;
            PathDesignator pathDesignator = null;

            if (!MatchNodePath(lesson, level, out pathDesignator))
                return null;

            if ((mediaItem.SourceContentKeys != null) &&(mediaItem.SourceContentKeys.Count() != 0))
            {
                string studyListContentKey = mediaItem.SourceContentKeys[0];
                studyListContent = lesson.GetContent(studyListContentKey);
            }

            Node node = CreateTargetContentLesson(content, studyListContent, targetLanguageID, hostLanguageID, level);

            return node;
        }

        public class AnnotationItem
        {
            public string InputTag;             // Input style to match.
            public TextType OutputType;         // Output TextType enum.

            public AnnotationItem(string inputTag, TextType outputType)
            {
                InputTag = inputTag;
                OutputType = outputType;
            }
        }

        // Map annotation tag to text type.
        public static AnnotationItem[] AnnotationTagMap = {
            new AnnotationItem("SimpleText", TextType.SimpleText),                  // 0 = Simple text.
            new AnnotationItem("Verse", TextType.Verse),                            // 1 = Verse.
            new AnnotationItem("Heading", TextType.Heading),                        // 2 = Chapter heading.
            new AnnotationItem("SubHeading", TextType.SubHeading),                  // 3 = Chapter subheading.
            new AnnotationItem("ByLine", TextType.ByLine),                          // 4 = Byline.
            new AnnotationItem("Kicker", TextType.Kicker),                          // 5 = Kicker.
            new AnnotationItem("Intro", TextType.Intro),                            // 6 = Introduction.
            new AnnotationItem("StudyIntro", TextType.StudyIntro),                  // 7 = Study introduction.
            new AnnotationItem("Title", TextType.Title),                            // 8 = General title.
            new AnnotationItem("TitleNumber", TextType.TitleNumber),                // 9 = Title with a number.
            new AnnotationItem("SubTitle", TextType.SubTitle),                      // 10 = General subtitle.
            new AnnotationItem("Summary", TextType.Summary),                        // 11 = Summary.
            new AnnotationItem("SuperScript", TextType.SuperScript),                // 12 = Superscript to footnote.
            new AnnotationItem("Footnote", TextType.Footnote),                      // 13 = Footnote.
            new AnnotationItem("Suffix", TextType.Suffix),                          // 14 = Suffix.
            new AnnotationItem("InsetHeadingRed", TextType.InsetHeadingRed),        // 15 = Red inset heading.
            new AnnotationItem("InsetHeadingGreen", TextType.InsetHeadingGreen),    // 16 = Green inset heading.
            new AnnotationItem("InsetHeadingOrange", TextType.InsetHeadingOrange),  // 17 = Orange inset heading.
            new AnnotationItem("InsetHeadingTeal", TextType.InsetHeadingTeal),      // 18 = Teal inset heading.
            new AnnotationItem("InsetText", TextType.InsetText),                    // 19 = Inset text.
            new AnnotationItem("AsideHeading", TextType.AsideHeading),              // 20 = Aside heading.
            new AnnotationItem("AsideText", TextType.AsideText)                     // 21 = Aside text.
            };

        protected Node CreateTargetContentLesson(
            BaseObjectContent audioMediaItemContent,
            BaseObjectContent studyListContent,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            int level)
        {
            BaseObjectNode lesson = null;
            ContentStudyList studyList = null;
            ContentMediaItem mediaItem = null;
            List<MultiLanguageItem> studyItems;
            PathDesignator pathDesignator = null;
            string languageCode = GetLanguageCode(targetLanguageID);
            string languageName = targetLanguageID.LanguageName(UILanguageID);
            List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(targetLanguageID);
            GlossaryData glossaryData = GetGlossaryData(targetLanguageID);
            List<LanguageID> hostAlternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(hostLanguageID);
            GlossaryData hostGlossaryData = GetGlossaryData(hostLanguageID);
            string nodeKey = String.Empty;
            string parentPathName = String.Empty;
            string title = String.Empty;
            string description = String.Empty;
            string paragraphSelection = String.Empty;
            string paragraphSelectionTitle = String.Empty;

            if (studyListContent == null)
                throw new Exception("No study list content in node.");

            lesson = studyListContent.Node;

            if (!MatchNodePath(lesson, level, out pathDesignator))
                return null;

            if ((pathDesignator != null) && (pathDesignator.ParagraphSelection != null) && (pathDesignator.ParagraphSelection.Count() != 0))
            {
                paragraphSelectionTitle = pathDesignator.ParagraphSelectionTitle;
                paragraphSelection = paragraphSelectionTitle.Replace(":", "_").Replace(",", "_").Replace(";", "_");
            }

            List<string> namePath = lesson.GetNamePath(UILanguageID);
            string pathTitle = GetPathTitle(namePath) + paragraphSelectionTitle;

            UpdateProgressElapsed("Creating node for " + languageName + " " + pathTitle + "...");

            if (namePath != null)
            {
                int count = namePath.Count();
                int index;

                for (index = 0; index < count; index++)
                {
                    string nodeName = namePath[index];

                    if (index != 0)
                        nodeKey += "_";

                    nodeKey += MediaUtilities.FileFriendlyName(nodeName);
                }
            }

            nodeKey += paragraphSelection;

            title = lesson.GetTitleString(targetLanguageID) + paragraphSelectionTitle;
            description = lesson.GetDescriptionString(targetLanguageID);

            studyList = studyListContent.ContentStorageStudyList;
            studyItems = studyList.StudyItemsRecurse;

            if (audioMediaItemContent != null)
                mediaItem = audioMediaItemContent.ContentStorageMediaItem;

            List<TextUnit> textUnits = new List<TextUnit>();

            studyItems = FilterStudyItems(pathDesignator, studyItems);

            NodeUtilities.CollectDictionaryWords(
                studyItems,
                targetLanguageID,
                alternateLanguageIDs,
                HostAllLanguageIDs,
                true,
                IsTranslateMissingItems,
                IsAddNewItemsToDictionary,
                glossaryData.EntryList,
                glossaryData.EntryDictionary);

            if (NodeUtilities.HasMessageOrError)
            {
                if (NodeUtilities.HasError)
                {
                    PutError(NodeUtilities.Error);
                    UpdateProgressMessageElapsed(NodeUtilities.Error);
                    NodeUtilities.Error = String.Empty;
                }

                if (!String.IsNullOrEmpty(NodeUtilities.Message))
                {
                    UpdateProgressMessageElapsed(NodeUtilities.Message);
                    NodeUtilities.Message = String.Empty;
                }
            }

            NodeUtilities.CollectDictionaryWords(
                studyItems,
                hostLanguageID,
                hostAlternateLanguageIDs,
                TargetAllLanguageIDs,
                true,
                IsTranslateMissingItems,
                IsAddNewItemsToDictionary,
                hostGlossaryData.EntryList,
                hostGlossaryData.EntryDictionary);

            if (NodeUtilities.HasMessageOrError)
            {
                if (NodeUtilities.HasError)
                {
                    PutError(NodeUtilities.Error);
                    UpdateProgressMessageElapsed(NodeUtilities.Error);
                    NodeUtilities.Error = String.Empty;
                }

                if (!String.IsNullOrEmpty(NodeUtilities.Message))
                {
                    UpdateProgressMessageElapsed(NodeUtilities.Message);
                    NodeUtilities.Message = String.Empty;
                }
            }

            string mediaFileName = null;
            string mediaURLPath = null;
            string destFilePath = null;
            string destDirectoryPath = null;

            if (!IsNoAudio)
            {
                LanguageMediaItem languageMediaItem =
                    (mediaItem != null ?
                        mediaItem.GetLanguageMediaItemWithLanguages(
                            targetLanguageID,
                            null) :
                        null);

                // Media URL path composition.
                string mediaDirName = MediaUtilities.GetFileName(OutputMediaDirectory);

                mediaFileName = nodeKey + "_" + languageCode + ".mp3";
                mediaURLPath = mediaDirName + "/Lesson/" + languageCode;

                // Media file path composition.
                destFilePath = MediaUtilities.ConcatenateFilePath(OutputMediaDirectory, "Lesson");
                destDirectoryPath = destFilePath = MediaUtilities.ConcatenateFilePath(destFilePath, languageCode);
                destFilePath = MediaUtilities.ConcatenateFilePath(destFilePath, mediaFileName);

                if ((studyItems.Count() != 0) && (languageMediaItem != null))
                {
                    MediaDescription mediaDescription = languageMediaItem.GetAudioMediaDescription();

                    if (mediaDescription != null)
                    {
                        string sourceTildeUrl = mediaDescription.GetContentUrlWithMediaCheck(audioMediaItemContent.MediaTildeUrl);
                        string sourceFilePath = ApplicationData.MapToFilePath(sourceTildeUrl);

                        if (((pathDesignator.ParagraphSelection != null)
                                && (pathDesignator.ParagraphSelection.Count() != 0))
                            || HasInsetFilter(pathDesignator))
                        {
                            try
                            {
                                if (FileSingleton.Exists(destFilePath))
                                    FileSingleton.Delete(destFilePath);
                                else
                                    FileSingleton.DirectoryExistsCheck(destFilePath);

                                GenerateEditedAudio(
                                    studyList,
                                    studyItems,
                                    pathDesignator,
                                    targetLanguageID,
                                    audioMediaItemContent.KeyString,
                                    languageMediaItem.KeyString,
                                    sourceFilePath,
                                    destFilePath);
                            }
                            catch (Exception exc)
                            {
                                PutExceptionError("Exception during text unit media editing", exc);
                            }
                        }
                        else
                        {
                            if (!FileSingleton.Exists(destFilePath))
                            {
                                if (FileSingleton.Exists(sourceFilePath))
                                {
                                    try
                                    {
                                        FileSingleton.DirectoryExistsCheck(destFilePath);
                                        FileSingleton.Copy(sourceFilePath, destFilePath);
                                    }
                                    catch (Exception exc)
                                    {
                                        PutExceptionError("Exception during text unit media copy", exc);
                                    }
                                }
                                else
                                    PutMessage("Warning: Source media file does not exist: " + sourceFilePath);
                            }
                        }
                    }
                    else
                    {
                        // No media present.
                        mediaFileName = String.Empty;
                    }
                }
                else
                {
                    // No media present.
                    mediaFileName = String.Empty;
                }
            }

            if (studyItems.Count() != 0)
            {
                bool inInset = false;
                bool inAside = false;

                foreach (MultiLanguageItem studyItem in studyItems)
                {
                    LanguageItem targetLanguageItem = studyItem.LanguageItem(targetLanguageID);
                    LanguageItem hostLanguageItem = studyItem.LanguageItem(hostLanguageID);

                    if (targetLanguageItem == null)
                        continue;

                    List<RunGroup> runGroups = new List<RunGroup>();
                    List<TextRun> sentenceRuns = targetLanguageItem.SentenceRuns;
                    TextType textType = TextType.SimpleText;
                    string paragraphPrefix = String.Empty;

                    if (studyItem.HasAnnotations())
                    {
                        foreach (Annotation annotation in studyItem.Annotations)
                        {
                            AnnotationItem annotationItem = AnnotationTagMap.FirstOrDefault(x => x.InputTag == annotation.Tag);

                            if (annotationItem != null)
                                textType = annotationItem.OutputType;

                            if (textType == TextType.Verse)
                                paragraphPrefix = annotation.Value + " ";
                        }
                    }

                    List<string> texts = new List<string>() { targetLanguageItem.Text };
                    string hostTranslation = hostLanguageItem.Text;
                    List<string> prefices = null;

                    if (!String.IsNullOrEmpty(paragraphPrefix))
                        prefices = new List<string>() { paragraphPrefix };

                    if (alternateLanguageIDs != null)
                    {
                        foreach (LanguageID alternateLanguageID in alternateLanguageIDs)
                        {
                            texts.Add(studyItem.Text(alternateLanguageID));

                            if (!String.IsNullOrEmpty(paragraphPrefix))
                                prefices.Add(paragraphPrefix);
                        }
                    }

                    Annotation startAnnotation = studyItem.FindAnnotation("Start");
                    Annotation styleAnnotation = studyItem.FindAnnotation("Style");

                    if (startAnnotation != null)
                    {
                        if (startAnnotation.Value.StartsWith("Inset"))
                        {
                            AnnotationItem annotationItem = AnnotationTagMap.FirstOrDefault(x => x.InputTag == startAnnotation.Tag);

                            if (annotationItem != null)
                                textType = annotationItem.OutputType;

                            inInset = true;
                        }
                        else if (startAnnotation.Value.StartsWith("Aside"))
                        {
                            AnnotationItem annotationItem = AnnotationTagMap.FirstOrDefault(x => x.InputTag == startAnnotation.Tag);

                            if (annotationItem != null)
                                textType = annotationItem.OutputType;

                            inAside = true;
                        }
                    }
                    else if (inInset)
                    {
                        if (styleAnnotation != null)
                        {
                            AnnotationItem annotationItem = AnnotationTagMap.FirstOrDefault(x => x.InputTag == styleAnnotation.Tag);

                            if (annotationItem != null)
                                textType = annotationItem.OutputType;
                            else
                                textType = TextType.InsetText;
                        }
                        else
                            textType = TextType.InsetText;
                    }
                    else if (inAside)
                    {
                        if (styleAnnotation != null)
                        {
                            AnnotationItem annotationItem = AnnotationTagMap.FirstOrDefault(x => x.InputTag == styleAnnotation.Tag);

                            if (annotationItem != null)
                                textType = annotationItem.OutputType;
                            else
                                textType = TextType.AsideText;
                        }
                        else
                            textType = TextType.AsideText;
                    }

                    Annotation stopAnnotation = studyItem.FindAnnotation("Stop");

                    if (stopAnnotation != null)
                    {
                        if (stopAnnotation.Value.StartsWith("Inset"))
                            inInset = false;
                        else if (stopAnnotation.Value.StartsWith("Aside"))
                            inAside = false;
                    }

                    if (sentenceRuns != null)
                    {
                        float startTime = -1;
                        float stopTime = -1;
                        //string unitMediaUrl = String.Empty;

                        if (IsParagraphsOnly && (sentenceRuns.Count() > 1))
                        {
                            TextRun mergedSentenceRun = targetLanguageItem.GetMergedSentenceRun();
                            sentenceRuns = new List<TextRun>() { mergedSentenceRun };
                        }

                        int sentenceCount = sentenceRuns.Count();
                        int sentenceIndex;

                        for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
                        {
                            TextRun sentenceRun = sentenceRuns[sentenceIndex];
                            LanguageRuns mainLanguageRun = GetMainLanguageRuns(
                                studyItem,
                                targetLanguageItem,
                                sentenceRun,
                                sentenceIndex,
                                targetLanguageID,
                                alternateLanguageIDs,
                                hostLanguageID,
                                paragraphPrefix,
                                glossaryData);
                            List<LanguageRuns> languageRuns = new List<LanguageRuns>() { mainLanguageRun };
                            float sentenceStartTime = -1;
                            float sentenceStopTime = -1;

                            if ((alternateLanguageIDs != null) && (alternateLanguageIDs.Count() != 0))
                            {
                                foreach (LanguageID alternateLanguageID in alternateLanguageIDs)
                                {
                                    LanguageItem alternateLanguageItem = studyItem.LanguageItem(alternateLanguageID);
                                    List<TextRun> alternateSentenceRuns = alternateLanguageItem.SentenceRuns;

                                    if (IsParagraphsOnly && (alternateSentenceRuns.Count() > 1))
                                    {
                                        TextRun mergedSentenceRun = targetLanguageItem.GetMergedSentenceRun();
                                        alternateSentenceRuns = new List<TextRun>() { mergedSentenceRun };
                                    }

                                    if (sentenceIndex >= alternateSentenceRuns.Count())
                                    {
                                        PutMessage("Sentence run mismatch. nodeKey = " + nodeKey + ", language = " +
                                            alternateLanguageID.LanguageName(UILanguageID) +
                                            ", paragraph = " + alternateLanguageItem.Text);
                                        continue;
                                    }

                                    TextRun alternateSentenceRun = alternateSentenceRuns[sentenceIndex];

                                    LanguageRuns alternateLanguageRun = GetAlternateLanguageRuns(
                                        alternateLanguageItem,
                                        alternateSentenceRun,
                                        alternateLanguageID,
                                        paragraphPrefix,
                                        mainLanguageRun);

                                    languageRuns.Add(alternateLanguageRun);
                                }
                            }

                            if (sentenceRun.MediaRunCount() != 0)
                            {
                                foreach (MediaRun mediaRun in sentenceRun.MediaRuns)
                                {
                                    if (mediaRun.KeyString == "Audio")
                                    {
                                        if (mediaRun.IsReference)
                                        {
                                            sentenceStartTime = (float)mediaRun.Start.TotalSeconds;
                                            sentenceStopTime = (float)mediaRun.Stop.TotalSeconds;

                                            if (startTime == -1)
                                                startTime = sentenceStartTime;

                                            stopTime = sentenceStopTime;
                                        }
#if false
                                        // If we are using sentence media.
                                        else if (OutputSentenceMedia)
                                        {
                                            string sourceTildeUrl = mediaRun.GetUrlWithMediaCheck(studyListContent.MediaTildeUrl);
                                            string sourceFilePath = ApplicationData.MapToFilePath(sourceTildeUrl);
                                            string fileName = MediaUtilities.GetFileName(sourceTildeUrl);
                                            string destFilePath = MediaUtilities.ConcatenateFilePath(
                                                OutputMediaDirectory, "lesson");
                                            destFilePath = MediaUtilities.ConcatenateFilePath(
                                                destFilePath, languageCode);
                                            destFilePath = MediaUtilities.ConcatenateFilePath(
                                                destFilePath, fileName);
                                            string mediaDirName = MediaUtilities.GetFileName(OutputMediaDirectory);
                                            string destRelativeUrl = mediaDirName + "/Lesson/" + languageCode + "/" + fileName;
                                            unitMediaUrl += destRelativeUrl;

                                            if (!FileSingleton.Exists(destFilePath))
                                            {
                                                try
                                                {
                                                    FileSingleton.DirectoryExistsCheck(destFilePath);
                                                    FileSingleton.Copy(sourceFilePath, destFilePath);
                                                }
                                                catch (Exception exc)
                                                {
                                                    PutExceptionError("Exception during text unit media copy", exc);
                                                }
                                            }
                                        }
#endif
                                    }
                                }
                            }

                            RunGroup runGroup = new RunGroup();
                            runGroup.LanguageRuns = languageRuns.ToArray();
                            runGroup.StartTime = sentenceStartTime;
                            runGroup.StopTime = sentenceStopTime;
                            //runGroup.MediaUrl = unitMediaUrl;

                            runGroups.Add(runGroup);
                        }

                        TextUnit textUnit = new TextUnit();

                        if (prefices != null)
                            textUnit.Prefix = prefices.ToArray();
                        else
                            textUnit.Prefix = null;

                        textUnit.Text = texts.ToArray();
                        textUnit.Translation = hostTranslation;
                        textUnit.RunGroups = runGroups.ToArray();
                        textUnit.Type = (int)textType;
                        textUnit.StartTime = startTime;
                        textUnit.StopTime = stopTime;

                        textUnits.Add(textUnit);
                    }
                }

                string iconFileName = String.Empty;
                string iconDirectoryPath = MediaUtilities.ConcatenateFilePath(OutputMediaDirectory, "Lesson");
                iconDirectoryPath = MediaUtilities.ConcatenateFilePath(iconDirectoryPath, languageCode);
                string iconMediaURLPath = MediaUtilities.GetFileName(OutputMediaDirectory) + "/Lesson/" + languageCode;

                if (lesson.HasImageFile)
                {
                    string iconSourceFilePath = lesson.ImageFilePath;

                    if (FileSingleton.Exists(iconSourceFilePath))
                    {
                        iconFileName = nodeKey + ".jpg";
                        string iconDestFilePath = MediaUtilities.ConcatenateFilePath(iconDirectoryPath, iconFileName);

                        try
                        {
                            if (FileSingleton.Exists(iconDestFilePath))
                                FileSingleton.Delete(iconDestFilePath);

                            FileSingleton.DirectoryExistsCheck(iconDestFilePath);
                            FileSingleton.Copy(iconSourceFilePath, iconDestFilePath);
                        }
                        catch (Exception exc)
                        {
                            PutExceptionError("Exception during text unit icon copy", exc);
                        }
                    }
                }

                Node node = new Node();
                node.Key = nodeKey;
                node.TargetLanguageCodes = GetFamilyLanguageCodes(targetLanguageID);
                node.HostLanguageCode = GetLanguageCode(hostLanguageID);
                node.Title = GetMultiString(lesson.Title, targetLanguageID, hostLanguageID, paragraphSelectionTitle);
                node.TitlePath = GetTitlePath(lesson, targetLanguageID, hostLanguageID, paragraphSelectionTitle);
                node.Description = GetMultiString(lesson.Description, targetLanguageID, hostLanguageID, String.Empty);
                node.TextUnits = textUnits.ToArray();
                node.MediaPath = iconMediaURLPath;
                node.MediaFileName = mediaFileName;
                node.IconFileName = iconFileName;
                node.Children = null;

                return node;
            }

            return null;
        }

        protected string GetPathTitle(List<string> namePath)
        {
            string pathTitle = String.Empty;
            
            foreach (string name in namePath)
            {
                if (!String.IsNullOrEmpty(pathTitle))
                    pathTitle += "/";

                pathTitle += name;
            }

            return pathTitle;
        }

        protected List<MultiLanguageItem> FilterStudyItems(
            PathDesignator pathDesignator,
            List<MultiLanguageItem> studyItems)
        {
            if (pathDesignator == null)
                return studyItems;

            List<MultiLanguageItem> filteredStudyItems = new List<MultiLanguageItem>();

            // If we have an inset node in the node path
            if (HasInsetFilter(pathDesignator))
            {
                bool inInset = false;
                bool isMatch = false;

                foreach (MultiLanguageItem studyItem in studyItems)
                {
                    if (studyItem.HasAnnotation("Start"))
                    {
                        inInset = true;

                        // Get the heading.
                        string text = studyItem.Text(LanguageLookup.English);

                        isMatch = (text == pathDesignator.Nodes.Last());
                    }

                    if (inInset && isMatch)
                        filteredStudyItems.Add(studyItem);

                    if (studyItem.HasAnnotation("Stop"))
                    {
                        inInset = false;
                        isMatch = false;
                    }
                }

                studyItems = filteredStudyItems;
            }
            // If not including insets, do a first pass to take them out.
            else if (!IncludeInsets)
            {
                bool inInset = false;

                foreach (MultiLanguageItem studyItem in studyItems)
                {
                    if (studyItem.HasAnnotation("Start"))
                        inInset = true;

                    if (!inInset)
                        filteredStudyItems.Add(studyItem);

                    if (studyItem.HasAnnotation("Stop"))
                        inInset = false;
                }

                studyItems = new List<MultiLanguageItem>(filteredStudyItems);
            }

            // Filter paragraph selections.
            if ((pathDesignator.ParagraphSelection != null) && (pathDesignator.ParagraphSelection.Count() != 0))
            {
                List<int> selectors = pathDesignator.ParagraphSelection;
                int count = selectors.Count();
                int index;
                bool hasOrdinals = false;

                filteredStudyItems.Clear();

                foreach (MultiLanguageItem studyItem in studyItems)
                {
                    if (studyItem.HasAnnotation("Ordinal"))
                    {
                        hasOrdinals = true;
                        break;
                    }
                }

                int studyItemIndex = 0;

                foreach (MultiLanguageItem studyItem in studyItems)
                {
                    int ordinal;

                    if (hasOrdinals)
                    {
                        Annotation annotation = studyItem.FindAnnotation("Ordinal");

                        if (annotation == null)
                            continue;

                        ordinal = ObjectUtilities.GetIntegerFromString(annotation.Value, 0);
                    }
                    else
                        ordinal = studyItemIndex;

                    for (index = 0; index < count; index += 2)
                    {
                        if ((ordinal >= selectors[index]) && (ordinal <= selectors[index + 1]))
                            filteredStudyItems.Add(studyItem);
                    }

                    studyItemIndex++;
                }
            }
            else
                filteredStudyItems = studyItems;

            return filteredStudyItems;
        }

        // For now, if we have 4 nodes, the last is an inset.
        protected bool MatchInsetName(PathDesignator pathDesignator, string insetName)
        {
            if (pathDesignator.Nodes.Count() < 4)
                throw new Exception("No inset filter node.");

            if (pathDesignator.Nodes[3] == insetName)
                return true;

            return false;
        }

        // For now, if we have 4 nodes, the last is an inset.
        protected bool HasInsetFilter(PathDesignator pathDesignator)
        {
            if (pathDesignator.Nodes.Count() == 4)
                return true;

            return false;
        }

        protected LanguageRuns GetMainLanguageRuns(
            MultiLanguageItem studyItem,
            LanguageItem languageItem,
            TextRun sentenceRun,
            int sentenceIndex,
            LanguageID targetLanguageID,
            List<LanguageID> alternateLanguageIDs,
            LanguageID hostLanguageID,
            string paragraphPrefix,
            GlossaryData glossaryData)
        {
            string languageCode = GetLanguageCode(targetLanguageID) +
                GetLanguageInstanceSuffix(targetLanguageID);
            string languageText = String.Empty;
            bool haveAlternates = ((alternateLanguageIDs != null) && (alternateLanguageIDs.Count() != 0));
            List<TextRun> wordRuns = null;
            TextRun wordRun;
            List<Run> runs = new List<Run>();
            Run run;
            LanguageItem hostLanguageItem = studyItem.LanguageItem(hostLanguageID);
            TextRun hostSentenceRun = (hostLanguageItem != null ? hostLanguageItem.GetSentenceRun(sentenceIndex) : null);
            List<DictionaryEntry> hostDictionaryEntries = GetSentenceRunWordDictionaryEntries(
                hostLanguageItem, hostSentenceRun);

            if (languageItem != null)
            {
                languageText = languageItem.Text;
                wordRuns = languageItem.PhrasedWordRuns;
            }

            int groupStart = sentenceRun.Start;
            int groupStop = sentenceRun.Stop;
            int textIndex = groupStart;

            if (wordRuns != null)
            {
                int runCount = wordRuns.Count();
                int runIndex;
                //bool first = true;

                for (runIndex = 0; runIndex < runCount; runIndex++)
                {
                    wordRun = wordRuns[runIndex];

                    if ((wordRun.Stop <= sentenceRun.Start) || (wordRun.Start >= sentenceRun.Stop))
                        continue;

#if true            // Text in TextUnits
                    int runStart = wordRun.Start;
                    int runStop = wordRun.Stop;
                    string text = languageText.Substring(wordRun.Start, wordRun.Length);
                    textIndex += text.Length;
#else               // Text in Runs.
                    string prefix;
                    string text;
                    string suffix;

                    if (textIndex < wordRun.Start)
                    {
                        prefix = languageText.Substring(textIndex, wordRun.Start - textIndex);

                        // If not first run of the sentence, get the latter part of the prefix we
                        // want to use, assuming the last run got the first part.
                        if (!first)
                            FilterPrefix(ref prefix, ref textIndex);
                        else
                            textIndex += prefix.Length;
                    }
                    else
                        prefix = String.Empty;

                    first = false;

                    if (!String.IsNullOrEmpty(paragraphPrefix))
                    {
                        prefix = paragraphPrefix + " " + prefix;
                        paragraphPrefix = "";
                    }

                    text = languageText.Substring(wordRun.Start, wordRun.Length);

                    textIndex += text.Length;
                    TextRun nextRun;

                    if ((runIndex + 1 < runCount) && ((nextRun = wordRuns[runIndex + 1]).Start <= sentenceRun.Stop))
                    {
                        if (wordRun.Stop < nextRun.Start)
                        {
                            suffix = languageText.Substring(wordRun.Stop, nextRun.Start - wordRun.Stop);

                            // If the run is not the last run of the sentence, we get the part of the suffix
                            // we want to use and leave the rest for the next run.
                            FilterSuffix(ref suffix, ref textIndex);
                        }
                        else
                            suffix = String.Empty;
                    }
                    else if (wordRun.Stop < sentenceRun.Stop)
                    {
                        suffix = languageText.Substring(wordRun.Stop, sentenceRun.Stop - wordRun.Stop);
                        textIndex += suffix.Length;
                    }
                    else
                        suffix = String.Empty;
#endif

                    int wordIndex = -1;
                    List<int> senseIndices = null;
                    int reading = -1;
                    List<Dictionary.Sense> senses = null;

                    if (!String.IsNullOrEmpty(text))
                    {
                        DictionaryEntry dictionaryEntry;

                        if (glossaryData.EntryDictionary.TryGetValue(text.ToLower(), out dictionaryEntry))
                        {
                            bool isPhoneticTarget = LanguageLookup.IsAlternatePhonetic(dictionaryEntry.LanguageID);

                            wordIndex = glossaryData.EntryList.IndexOf(dictionaryEntry);

                            if (haveAlternates && dictionaryEntry.HasAlternates())
                            {
                                foreach (LanguageID alternateLanguageID in alternateLanguageIDs)
                                {
                                    LanguageItem alternateLanguageItem = studyItem.LanguageItem(alternateLanguageID);

                                    if (alternateLanguageItem == null)
                                        continue;

                                    TextRun alternateWordRun = alternateLanguageItem.GetWordRun(runIndex);

                                    if (alternateWordRun == null)
                                        continue;

                                    string alternateWord = alternateLanguageItem.GetRunText(alternateWordRun).ToLower();
                                    string alternateWordNoSpace = TextUtilities.StripWhiteSpace(alternateWord);

                                    foreach (LanguageString alternate in dictionaryEntry.Alternates)
                                    {
                                        if (alternate.LanguageID != alternateLanguageID)
                                            continue;

                                        string alternateLower = alternate.TextLower;

                                        if ((alternateLower == alternateWord) || (alternateLower == alternateWordNoSpace))
                                        {
                                            int newReading = alternate.KeyInt;

                                            // We cheat a little here and use the host language to
                                            // pick a sense with a reading.
                                            senses = dictionaryEntry.GetSensesWithReading(
                                                newReading,
                                                HostLanguageIDs);

                                            if (senses.Count() == 0)
                                                continue;

                                            // If no other reading yet, this becomes the reading.
                                            if (reading == -1)
                                            {
                                                reading = newReading;

                                                if (senseIndices == null)
                                                    senseIndices = new List<int>();
                                                else
                                                    senseIndices.Clear();

                                                foreach (Dictionary.Sense aSense in senses)
                                                    senseIndices.Add(dictionaryEntry.Senses.IndexOf(aSense));
                                            }
                                            // Else we give priority to a reading that is opposite in terms of phoneticness.
                                            else if (isPhoneticTarget != LanguageLookup.IsAlternatePhonetic(alternateLanguageID))
                                            {
                                                reading = newReading;

                                                if (senseIndices == null)
                                                    senseIndices = new List<int>();
                                                else
                                                    senseIndices.Clear();

                                                foreach (Dictionary.Sense aSense in senses)
                                                    senseIndices.Add(dictionaryEntry.Senses.IndexOf(aSense));
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                senses = dictionaryEntry.GetSensesWithLanguageID(HostLanguageIDs.First());

                                if (senses.Count() == 1)
                                    senseIndices = new List<int>() { 0 };
                            }

                            if ((senses != null) && (senses.Count() > 1))
                                FindBestTranslationIndexes(
                                    senses,
                                    hostDictionaryEntries,
                                    targetLanguageID,
                                    hostLanguageID,
                                    ref senseIndices);
                        }
                    }

                    run = new Run();

#if true            // Text in TextUnits
                    run.RunStart = runStart;
                    run.RunStop = runStop;
#else               // Text in Runs
                    run.Prefix = prefix;
                    run.Text = text;
                    run.Suffix = suffix;
#endif
                    run.GlossaryKey = wordIndex;

                    if (senseIndices != null)
                        run.TranslationIndexes = senseIndices.ToArray();
                    else
                        run.TranslationIndexes = null;

                    runs.Add(run);
                }
            }

            LanguageRuns languageRuns = new LanguageRuns();
            languageRuns.LanguageCode = languageCode;
#if true                    // Text in TextUnits.
            languageRuns.GroupStart = groupStart;
            languageRuns.GroupStop = groupStop;
#endif
            languageRuns.Runs = runs.ToArray();

            return languageRuns;
        }

        protected LanguageRuns GetAlternateLanguageRuns(
            LanguageItem languageItem,
            TextRun sentenceRun,
            LanguageID languageID,
            string paragraphPrefix,
            LanguageRuns mainLanguageRuns)
        {
            string languageCode = LanguageCodes.GetCode_ll_CC_FromCode2(languageID.LanguageCode) +
                GetLanguageInstanceSuffix(languageID);
            string languageText = String.Empty;
            List<TextRun> wordRuns = null;
            TextRun wordRun;
            List<Run> runs = new List<Run>();
            Run mainRun;
            Run run;
            int groupStart = sentenceRun.Start;
            int groupStop = sentenceRun.Stop;

            if (languageItem != null)
            {
                languageText = languageItem.Text;
                wordRuns = languageItem.PhrasedWordRuns;
            }

            int textIndex = 0;

            if (wordRuns != null)
            {
                int runCount = wordRuns.Count();
                int runIndex;
                //bool first = true;

                if (wordRuns.Count() != mainLanguageRuns.Runs.Length)
                {
                    PutError("Word runs don't match for language " + languageCode
                        + " text: " + languageItem.Text);
                    return null;
                }

                for (runIndex = 0; runIndex < runCount; runIndex++)
                {
                    wordRun = wordRuns[runIndex];

                    if ((wordRun.Stop <= sentenceRun.Start) || (wordRun.Start >= sentenceRun.Stop))
                        continue;

                    mainRun = mainLanguageRuns.Runs[runIndex];

#if true            // Text in TextUnits
                    int runStart = wordRun.Start;
                    int runStop = wordRun.Stop;
                    string text = languageText.Substring(wordRun.Start, wordRun.Length);
                    textIndex += text.Length;
#else               // Text in Runs
                    string prefix;
                    string text;
                    string suffix;

                    if (textIndex < wordRun.Start)
                    {
                        prefix = languageText.Substring(textIndex, wordRun.Start - textIndex);

                        // If not first run of the sentence, get the latter part of the prefix we
                        // want to use, assuming the last run got the first part.
                        if (!first)
                            FilterPrefix(ref prefix, ref textIndex);
                        else
                            textIndex += prefix.Length;
                    }
                    else
                        prefix = String.Empty;

                    first = false;

                    if (!String.IsNullOrEmpty(paragraphPrefix))
                    {
                        prefix = paragraphPrefix + " " + prefix;
                        paragraphPrefix = "";
                    }

                    text = languageText.Substring(wordRun.Start, wordRun.Length);

                    textIndex += text.Length;
                    TextRun nextRun;

                    if ((runIndex + 1 < runCount) && ((nextRun = wordRuns[runIndex + 1]).Start <= sentenceRun.Stop))
                    {
                        if (wordRun.Stop < nextRun.Start)
                        {
                            suffix = languageText.Substring(wordRun.Stop, nextRun.Start - wordRun.Stop);

                            // If the run is not the last run of the sentence, we get the part of the suffix
                            // we want to use and leave the rest for the next run.
                            FilterSuffix(ref suffix, ref textIndex);
                        }
                        else
                            suffix = String.Empty;
                    }
                    else if (wordRun.Stop < sentenceRun.Stop)
                    {
                        suffix = languageText.Substring(wordRun.Stop, sentenceRun.Stop - wordRun.Stop);
                        textIndex += suffix.Length;
                    }
                    else
                        suffix = String.Empty;
#endif

                    run = new Run();

#if true            // Text in TextUnits
                    run.RunStart = runStart;
                    run.RunStop = runStop;
#else               // Text in Runs
                    run.Prefix = prefix;
                    run.Text = text;
                    run.Suffix = suffix;
#endif
                    run.GlossaryKey = mainRun.GlossaryKey;
                    run.TranslationIndexes = mainRun.TranslationIndexes;

                    runs.Add(run);

                    textIndex = wordRun.Stop;
                }
            }

            LanguageRuns languageRuns = new LanguageRuns();
            languageRuns.LanguageCode = languageCode;
#if true                    // Text in TextUnits.
            languageRuns.GroupStart = groupStart;
            languageRuns.GroupStop = groupStop;
#endif
            languageRuns.Runs = runs.ToArray();

            return languageRuns;
        }

#if false   // Text in Runs.
        protected void FilterPrefix(ref string prefix, ref int textIndex)
        {
            int index;
            int count = prefix.Length;

            for (index = count - 1; index >= 0; index--)
            {
                char chr = prefix[index];

                if (LanguageLookup.MatchedStartCharacters.Contains(chr))
                    continue;

                if (LanguageLookup.PunctuationCharacters.Contains(chr))
                    break;
            }

            index++;

            if (index > 0)
            {
                textIndex += index;
                prefix = prefix.Substring(index, count - index);
            }
            else
                textIndex += count;
        }

        protected void FilterSuffix(ref string suffix, ref int textIndex)
        {
            int index;
            int count = suffix.Length;

            for (index = 0; index < count; index++)
            {
                char chr = suffix[index];

                if (!LanguageLookup.PunctuationCharacters.Contains(chr))
                    break;
            }

            if (index < count)
            {
                textIndex += index;
                suffix = suffix.Substring(0, index);
            }
            else
                textIndex += count;
        }
#endif

        protected bool FindBestTranslationIndexes(
            List<Dictionary.Sense> senses,
            List<DictionaryEntry> hostDictionaryEntries,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            ref List<int> senseIndices)
        {
            bool returnValue = false;
            int senseCount = senses.Count();
            int indexSense;
            List<DictionaryEntry> matchedEntries = new List<DictionaryEntry>();
            List<int> matchedIndices = new List<int>();

            foreach (DictionaryEntry hostDictionaryEntry in hostDictionaryEntries)
            {
                string hostWord = hostDictionaryEntry.KeyString;

                for (indexSense = 0; indexSense < senseCount; indexSense++)
                {
                    Dictionary.Sense sense = senses[indexSense];
                    LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(hostLanguageID);

                    if (languageSynonyms == null)
                        continue;

                    if (languageSynonyms.HasProbableSynonyms())
                    {
                        foreach (ProbableMeaning probableSynonym in languageSynonyms.ProbableSynonyms)
                        {
                            if (probableSynonym.MatchMeaningIgnoreCase(hostWord))
                            {
                                matchedEntries.Add(hostDictionaryEntry);
                                matchedIndices.Add(indexSense);
                            }
                        }
                    }
                }
            }

            if (matchedIndices.Count() != 0)
            {
                senseIndices = matchedIndices;
                returnValue = true;
            }
            else
                senseIndices = null;

            return returnValue;
        }

        protected bool GenerateEditedAudio(
            ContentStudyList studyList,
            List<MultiLanguageItem> targetStudyItems,
            PathDesignator pathDesignator,
            LanguageID languageID,
            string mediaItemKey,
            string languageMediaItemKey,
            string sourceFilePath,
            string destFilePath)
        {
            WaveAudioBuffer audioBuffer;
            List<MultiLanguageItem> studyItems = studyList.StudyItemsRecurse;
            int studyItemIndex = 0;
            List<int> selectors = pathDesignator.ParagraphSelection;
            int count = (selectors != null ? selectors.Count() : 0);
            List<TimeSpan> segmentSpans = new List<TimeSpan>();
            List<TimeSpan> deleteSpans = new List<TimeSpan>();
            TimeSpan startTime;
            TimeSpan stopTime;
            TimeSpan remappedTime = TimeSpan.Zero;
            int index;
            bool inSegment = false;
            string errorMessage = null;
            List<LanguageID> familyLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(languageID);
            bool isHaveInsetFilter = HasInsetFilter(pathDesignator);
            string currentInsetName = null;

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                bool found = true;

                if (selectors != null)
                {
                    int ordinal = studyItemIndex;
                    Annotation annotation = studyItem.FindAnnotation("Ordinal");

                    found = false;

                    if (annotation != null)
                        ordinal = ObjectUtilities.GetIntegerFromString(annotation.Value, 0);

                    for (index = 0; index < count; index += 2)
                    {
                        if ((ordinal >= selectors[index]) && (ordinal <= selectors[index + 1]))
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (isHaveInsetFilter)
                {
                    found = false;

                    if (studyItem.HasAnnotation("Start"))
                        currentInsetName = studyItem.Text(LanguageLookup.English);

                    if (!String.IsNullOrEmpty(currentInsetName))
                    {
                        if (MatchInsetName(pathDesignator, currentInsetName))
                            found = true;
                    }

                    if (studyItem.HasAnnotation("Stop"))
                        currentInsetName = null;
                }

                if (found)
                {
                    MediaRun mediaRun = studyItem.GetMergedReferenceMediaRun(
                        languageID, "Audio", mediaItemKey, languageMediaItemKey);

                    if (mediaRun != null)
                    {
                        if (inSegment)
                            stopTime = mediaRun.Stop;
                        else
                        {
                            startTime = mediaRun.Start;
                            stopTime = mediaRun.Stop;
                            inSegment = true;
                        }

                        MultiLanguageItem targetStudyItem = targetStudyItems.FirstOrDefault(x => x.KeyString == studyItem.KeyString);

                        if (targetStudyItem == null)
                            throw new Exception("Study item mismatch while extracting main audio.");

                        TimeSpan timeToSubtract = mediaRun.Start - remappedTime;

                        targetStudyItem.RemapReferenceMediaRuns(
                            languageID,
                            mediaRun.KeyString,
                            mediaRun.MediaItemKey,
                            mediaRun.LanguageMediaItemKey,
                            timeToSubtract);

                        for (int li = 1; li < familyLanguageIDs.Count(); li++)
                        {
                            LanguageID alternateLanguageID = familyLanguageIDs[li];
                            targetStudyItem.RemapReferenceMediaRuns(
                                alternateLanguageID,
                                mediaRun.KeyString,
                                mediaRun.MediaItemKey,
                                mediaRun.LanguageMediaItemKey,
                                timeToSubtract);
                        }

                        remappedTime += mediaRun.Length;
                    }
                }
                else
                {
                    if (inSegment)
                    {
                        segmentSpans.Add(startTime);
                        segmentSpans.Add(stopTime);
                        inSegment = false;
                    }
                }

                studyItemIndex++;
            }

            if (inSegment)
            {
                segmentSpans.Add(startTime);
                segmentSpans.Add(stopTime);
            }

            if (segmentSpans.Count() == 0)
                return false;

            try
            {
                audioBuffer = MediaConvertSingleton.Mp3Decoding(sourceFilePath, out errorMessage);

                if ((audioBuffer == null) || !String.IsNullOrEmpty(errorMessage))
                {
                    PutError(errorMessage);
                    return false;
                }
            }
            catch (Exception exc)
            {
                PutExceptionError("Exception while decoding audio file " + sourceFilePath, exc);
                return false;
            }

            TimeSpan timeLength = audioBuffer.TimeLength;

            if (segmentSpans[0] != TimeSpan.Zero)
            {
                deleteSpans.Add(TimeSpan.Zero);
                deleteSpans.AddRange(segmentSpans);
            }

            count = segmentSpans.Count();

            if (segmentSpans[count - 1] < timeLength)
                deleteSpans.Add(timeLength);
            else
                deleteSpans.RemoveAt(deleteSpans.Count() - 1);

            count = deleteSpans.Count();

            for (index = count - 2; index >= 0; index -= 2)
            {
                startTime = deleteSpans[index];
                stopTime = deleteSpans[index + 1];

                int startWaveIndex = audioBuffer.GetSampleIndexFromTime(startTime);
                int endWaveIndex = audioBuffer.GetSampleIndexFromTime(stopTime);
                int sampleCount = endWaveIndex - startWaveIndex;

                audioBuffer.DeleteSamples(startWaveIndex, sampleCount);
            }

            if (!MediaConvertSingleton.Mp3Encoding(destFilePath, audioBuffer, out errorMessage))
            {
                PutError(errorMessage);
                return false;
            }

            return true;
        }

        protected GlossaryData GetGlossaryData(LanguageID languageID)
        {
            GlossaryData glossaryData = null;
            GlossaryDataCache.TryGetValue(languageID, out glossaryData);
            return glossaryData;
        }

        protected DictionaryEntry GetDictionaryEntry(
            string key,
            LanguageID languageID,
            out int glossaryIndex)
        {
            GlossaryData glossaryData = GetGlossaryData(languageID);
            DictionaryEntry dictionaryEntry = null;

            glossaryIndex = -1;

            if (!glossaryData.EntryDictionary.TryGetValue(key, out dictionaryEntry))
            {
                string normalizedWord = TextUtilities.GetGenericNormalizedPunctuationString(key);

                dictionaryEntry = Repositories.Dictionary.Get(normalizedWord, languageID);

                if (dictionaryEntry != null)
                {
                    glossaryIndex = glossaryData.EntryList.Count();
                    glossaryData.EntryList.Add(dictionaryEntry);
                    glossaryData.EntryDictionary.Add(key, dictionaryEntry);
                }
            }
            else
                glossaryIndex = glossaryData.EntryList.IndexOf(dictionaryEntry);

            return dictionaryEntry;
        }

        protected List<DictionaryEntry> GetSentenceRunWordDictionaryEntries(
            LanguageItem languageItem,
            TextRun sentenceRun)
        {
            List<DictionaryEntry> entries = new List<DictionaryEntry>();

            if ((languageItem == null) || (languageItem.WordRunCount() == 0) || (sentenceRun == null))
                return entries;

            LanguageID languageID = languageItem.LanguageID;
            GlossaryData glossaryData = GetGlossaryData(languageID);

            foreach (TextRun wordRun in languageItem.PhrasedWordRuns)
            {
                if ((wordRun.Stop <= sentenceRun.Start) || (wordRun.Start >= sentenceRun.Stop))
                    continue;

                string word = languageItem.GetRunText(wordRun);

                if (String.IsNullOrEmpty(word))
                    continue;

                DictionaryEntry dictionaryEntry;

                if (!glossaryData.EntryDictionary.TryGetValue(word.ToLower(), out dictionaryEntry))
                    continue;

                if (entries.FirstOrDefault(x => x.KeyString == word) == null)
                    entries.Add(dictionaryEntry);
            }

            return entries;
        }

        protected bool MatchNodePath(BaseObjectNode node, int level, out PathDesignator path)
        {
            bool returnValue = false;

            path = null;

            if ((level == -1) || (PathPatterns == null))
            {
                level = 0;

                BaseObjectNode aNode = node;

                while (aNode.Parent != null)
                {
                    aNode = aNode.Parent;
                    level++;
                }

                if (!node.IsTree())
                    level++;
            }

            foreach (PathDesignator pathDesignator in PathPatterns)
            {
                if (level >= pathDesignator.Nodes.Count())
                    continue;

                string topPattern = GetMatchableString(pathDesignator.Nodes[level]);
                string topTitle = GetMatchableString(node.GetTitleString(UILanguageID));

                if (MatchPattern(topPattern, topTitle))
                {
                    BaseObjectNode aNode = node.Parent;
                    bool match = true;

                    if (aNode == null)
                        aNode = node.Tree;

                    for (int i = level - 1; (aNode != null) && (i >= 0); i--)
                    {
                        string pattern = GetMatchableString(pathDesignator.Nodes[i]);
                        string title = GetMatchableString(aNode.GetTitleString(UILanguageID));

                        if (!MatchPattern(pattern, title))
                        {
                            match = false;
                            break;
                        }

                        if (aNode.Parent == null)
                            aNode = aNode.Tree;
                        else
                            aNode = aNode.Parent;
                    }

                    if (match)
                    {
                        path = pathDesignator;
                        returnValue = true;
                        break;
                    }
                }
            }

            return returnValue;
        }

        protected string GetMatchableString(string str)
        {
            string matchableString = TextUtilities.GetCanonicalText(str, UILanguageID);

            matchableString = matchableString.Replace("—", "-");

            return matchableString;
        }

        protected bool MatchPattern(string pattern, string text)
        {
            if (pattern == "*")
                return true;

            if (pattern == text)
                return true;

            if (Regex.IsMatch(text, "^" + pattern + "$"))
                return true;

            return false;
        }

        protected bool CreateGlossaries(List<WordList> wordLists)
        {
            bool returnValue = true;

            foreach (LanguageID targetLanguageID in TargetRootLanguageIDs)
            {
                foreach (LanguageID hostLanguageID in HostRootLanguageIDs)
                {
                    if (hostLanguageID == targetLanguageID)
                        continue;

                    UpdateProgressElapsed("Creating glossary for "
                        + targetLanguageID.LanguageName(UILanguageID)
                        + " and "
                        + hostLanguageID.LanguageName(UILanguageID) + "...");

                    GlossaryData glossaryData = GetGlossaryData(targetLanguageID);

                    if ((glossaryData.EntryList == null) || (glossaryData.EntryList.Count() == 0))
                    {
                        PutMessage("No glossary data for target language " + targetLanguageID.LanguageName(UILanguageID));
                        //returnValue = false;
                        continue;
                    }

                    WordList wordList = CreateWordList(targetLanguageID, hostLanguageID, glossaryData);

                    if (wordList == null)
                    {
                        returnValue = false;
                        continue;
                    }
                    else if (wordList.Words.Length == 0)
                    {
                        PutError("No words in word list for languages "
                            + targetLanguageID.LanguageName(UILanguageID)
                            + " and "
                            + hostLanguageID.LanguageName(UILanguageID)
                            + ".");
                        returnValue = false;
                        continue;
                    }

                    wordLists.Add(wordList);
                }
            }

            return returnValue;
        }

        protected WordList CreateWordList(
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            GlossaryData glossaryData)
        {
            string targetLanguageCode = LanguageCodes.GetCode_ll_CC_FromCode2(targetLanguageID.LanguageCultureExtensionCode);
            WordList wordList = new WordList();
            List<Word> targetWords = new List<Word>();
            List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(targetLanguageID);
            List<Inflection> inflections = new List<Inflection>();
            Dictionary<string, int> inflectionsIndexDictionary = new Dictionary<string, int>();
#if FUTURE
            List<PartOfSpeech> partsOfSpeech = new List<PartOfSpeech>();
            Dictionary<string, int> partsOfSpeechIndexDictionary = new Dictionary<string, int>();
#endif
            string mediaDirName = MediaUtilities.GetFileName(OutputMediaDirectory);
            string mediaURLPath = mediaDirName + "/Dictionary/" + targetLanguageCode;
            LanguageID preferedMediaLanguageID = LanguageLookup.GetPreferedMediaLanguageID(targetLanguageID);
            string dictionaryLanguageAudioPath =
                MediaUtilities.ConcatenateFilePath(
                    ApplicationData.ContentPath,
                    MediaUtilities.GetDictionaryAudioDirectoryPath(preferedMediaLanguageID));
            string destDirectoryPath = MediaUtilities.ConcatenateFilePath(OutputMediaDirectory, "Dictionary");
            destDirectoryPath = MediaUtilities.ConcatenateFilePath(
                destDirectoryPath, 
                targetLanguageCode);

            FileSingleton.DirectoryExistsCheck(dictionaryLanguageAudioPath);
            FileSingleton.DirectoryExistsCheck(destDirectoryPath);

            int wordIndex;

            for (wordIndex = 0; wordIndex < glossaryData.EntryList.Count(); wordIndex++)
            {
                DictionaryEntry dictionaryEntry = glossaryData.EntryList[wordIndex];

                Word word = CreateWord(
                    dictionaryEntry,
                    targetLanguageID,
                    hostLanguageID,
                    alternateLanguageIDs,
                    preferedMediaLanguageID,
                    destDirectoryPath,
#if FUTURE
                    partsOfSpeechIndexDictionary,
                    partsOfSpeech,
#endif
                    inflectionsIndexDictionary,
                    inflections);

                targetWords.Add(word);
            }

            wordList.TargetLanguageCodes = GetFamilyLanguageCodes(targetLanguageID);
            wordList.HostLanguageCodes = GetFamilyLanguageCodes(hostLanguageID);
            wordList.Words = targetWords.ToArray();
            wordList.MediaPath = mediaURLPath;
            wordList.Inflections = inflections.ToArray();
#if FUTURE
            wordList.PartsOfSpeech = partsOfSpeech.ToArray();
#endif

            return wordList;
        }

        protected Word CreateWord(
            DictionaryEntry dictionaryEntry,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            List<LanguageID> alternateLanguageIDs,
            LanguageID preferedMediaLanguageID,
            string destDirectoryPath,
#if FUTURE
            Dictionary<string, int> partsOfSpeechIndexDictionary,
            List<PartOfSpeech> partsOfSpeech,
#endif
            Dictionary<string, int> inflectionsIndexDictionary,
            List<Inflection> inflections)
        {
            string targetWord = dictionaryEntry.KeyString;
            List<string> alternateWords = null;
            List<string> audioKeys = (IsNoAudio ? null : new List<string>());
            List<string> mediaFileNames = null;
            List<Translation> translations;

            if (dictionaryEntry.HasAlternates())
            {
                alternateWords = new List<string>();

                foreach (LanguageString alternate in dictionaryEntry.Alternates)
                {
                    alternateWords.Add(alternate.Text);

                    if (!IsNoAudio)
                    {
                        if (alternate.LanguageID == preferedMediaLanguageID)
                            audioKeys.Add(MediaUtilities.FileFriendlyName(alternate.Text));
                    }
                }

                if (!IsNoAudio)
                {
                    if (preferedMediaLanguageID == targetLanguageID)
                        audioKeys.Add(MediaUtilities.FileFriendlyName(targetWord));
                }
            }
            else if (!IsNoAudio)
                audioKeys.Add(MediaUtilities.FileFriendlyName(targetWord));

            if (!IsNoAudio)
                mediaFileNames = GetAudioFileNames(
                    targetWord,
                    targetLanguageID,
                    audioKeys,
                    preferedMediaLanguageID,
                    destDirectoryPath);

            translations = GetTranslations(
                dictionaryEntry,
                hostLanguageID,
                alternateLanguageIDs,
#if FUTURE
                partsOfSpeechIndexDictionary,
                partsOfSpeech,
#endif
                inflectionsIndexDictionary,
                inflections);

            Word word = new Word();
            word.Text = targetWord;

            if (alternateWords != null)
                word.Alternates = alternateWords.ToArray();
            else
                word.Alternates = null;

            if (mediaFileNames != null)
                word.MediaFileNames = mediaFileNames.ToArray();
            else
                word.MediaFileNames = null;

            word.Translations = translations.ToArray();

            return word;
        }

        protected List<Translation> GetTranslations(
            DictionaryEntry dictionaryEntry,
            LanguageID hostLanguageID,
            List<LanguageID> alternateLanguageIDs,
#if FUTURE
            Dictionary<string, int> partsOfSpeechIndexDictionary,
            List<PartOfSpeech> partsOfSpeech,
#endif
            Dictionary<string, int> inflectionsIndexDictionary,
            List<Inflection> inflections)
        {
            List<Translation> translations = new List<Translation>();
            int senseIndex = 0;

            foreach (Dictionary.Sense sense in dictionaryEntry.Senses)
            {
                LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(hostLanguageID);

                if (languageSynonyms == null)
                    continue;

                List<LanguageSynonyms> alternateLanguageSynonymsList = null;

                if (alternateLanguageIDs != null)
                {
                    alternateLanguageSynonymsList = new List<LanguageSynonyms>();

                    foreach (LanguageID alternateLanguageID in alternateLanguageIDs)
                    {
                        LanguageSynonyms alternateLanguageSynonyms = sense.GetLanguageSynonyms(alternateLanguageID);

                        if (alternateLanguageSynonyms == null)
                            continue;

                        alternateLanguageSynonymsList.Add(alternateLanguageSynonyms);
                    }
                }

                int synonymCount = languageSynonyms.SynonymCount;
                int synonymIndex;
                string category = sense.CategoryString;
                int reading = sense.Reading;
#if FUTURE
                int partOfSpeechIndex = GetPartOfSpeechIndex(
                    sense,
                    partsOfSpeechIndexDictionary,
                    partsOfSpeech);
#endif
                int lemmaIndex = -1;
                int inflectionIndex = -1;

                GetInflectionAndLemmaIndex(
                    sense,
                    TargetLanguageID,
                    hostLanguageID,
                    inflectionsIndexDictionary,
                    inflections,
                    out inflectionIndex,
                    out lemmaIndex);

                for (synonymIndex = 0; synonymIndex < synonymCount; synonymIndex++)
                {
                    ProbableMeaning probableSynonym = languageSynonyms.GetProbableSynonymIndexed(synonymIndex);
                    string synonymText = probableSynonym.Meaning;
                    List<string> alternateTexts = null;

                    if (alternateLanguageSynonymsList != null)
                    {
                        alternateTexts = new List<string>();

                        foreach (LanguageSynonyms alternateLanguageSynonyms in alternateLanguageSynonymsList)
                        {
                            string alternateText = alternateLanguageSynonyms.GetSynonymIndexed(synonymIndex);
                            alternateTexts.Add(alternateText);
                        }
                    }

                    Translation translation = new Translation();
                    translation.Text = synonymText;
                    translation.Reading = reading;
                    translation.LemmaIndex = lemmaIndex;
                    translation.InflectionIndex = inflectionIndex;

#if FUTURE
                    if (alternateTexts != null)
                        translation.Alternates = alternateTexts.ToArray();
                    else
                        translation.Alternates = null;

                    translation.SenseID = senseIndex;
                    translation.PartOfSpeechIndex = partOfSpeechIndex;
#endif

                    translations.Add(translation);
                    senseIndex++;
                }
            }

            return translations;
        }

        protected List<string> GetAudioFileNames(
            string targetWord,
            LanguageID targetLanguageID,
            List<string> audioKeys,
            LanguageID preferedMediaLanguageID,
            string destDirectoryPath)
        {
            List<string> mediaFileNames = null;

            foreach (string audioKey in audioKeys)
            {
                string mediaFileName = audioKey + ".mp3";
                string dictionaryAudioFilePath =
                    MediaUtilities.ConcatenateFilePath(
                        ApplicationData.ContentPath,
                        MediaUtilities.GetDictionaryAudioFilePath(preferedMediaLanguageID, audioKey + ".mp3"));
                bool dictionaryAudioExists = FileSingleton.Exists(dictionaryAudioFilePath);

                string destFilePath = MediaUtilities.ConcatenateFilePath(destDirectoryPath, mediaFileName);

                if (!IsForceAudio && FileSingleton.Exists(dictionaryAudioFilePath))
                {
                    try
                    {
                        if (FileSingleton.Exists(destFilePath))
                            FileSingleton.Delete(destFilePath);

                        FileSingleton.Copy(dictionaryAudioFilePath, destFilePath);

                        if (mediaFileNames == null)
                            mediaFileNames = new List<string>();

                        mediaFileNames.Add(mediaFileName);
                    }
                    catch (Exception exc)
                    {
                        PutExceptionError("Exception during dictionary media copy", exc);
                    }
                }
                else if (IsSynthesizeMissingAudio)
                {
                    try
                    {
                        if (IsForceAudio || !FileSingleton.Exists(destFilePath))
                        {
                            UpdateProgressMessageElapsed("Synthesizing speech for word: " + targetWord);

                            bool entryHasAudio = NodeUtilities.AddSynthesizedVoiceDefault(
                                targetWord,
                                destFilePath,
                                targetLanguageID);

                            if (entryHasAudio)
                            {
                                if (FileSingleton.Exists(destFilePath))
                                {
                                    if (mediaFileNames == null)
                                        mediaFileNames = new List<string>();

                                    mediaFileNames.Add(mediaFileName);

                                    if (!dictionaryAudioExists)
                                        FileSingleton.Copy(destFilePath, dictionaryAudioFilePath);
                                }
                                else
                                    ApplicationData.Global.PutConsoleMessage("Audio missing after synthesis for: " + targetWord);
                            }
                            else
                                ApplicationData.Global.PutConsoleMessage("Audio synthesis failed for: " + targetWord);
                        }
                        else
                            mediaFileNames.Add(mediaFileName);
                    }
                    catch (Exception exc)
                    {
                        PutExceptionError("Exception during glossary audio synthesis", exc);
                    }
                }
            }

            return mediaFileNames;
        }

#if FUTURE
        protected int GetPartOfSpeechIndex(
            Sense sense,
            Dictionary<string, int> partsOfSpeechIndexDictionary,
            List<PartOfSpeech> partsOfSpeech)
        {
            LexicalCategory category = sense.Category;
            string categoryString = sense.CategoryString;
            string abbreviatedPartOfSpeechName = String.Empty;
            int partOfSpeechIndex = -1;

            if ((category != LexicalCategory.Unknown) && !String.IsNullOrEmpty(categoryString))
            {
                categoryString = category.ToString() + "," + categoryString;

                if (!partsOfSpeechIndexDictionary.TryGetValue(categoryString, out partOfSpeechIndex))
                {
                    PartOfSpeech partOfSpeech = new PartOfSpeech();
                    partOfSpeech.FullName = categoryString;
                    abbreviatedPartOfSpeechName = sense.GetLexicalCategoryAbbreviation(LanguageLookup.English);

                    if (categoryString == "(auto)")
                        abbreviatedPartOfSpeechName += " Au";

                    partOfSpeech.Abbreviation = abbreviatedPartOfSpeechName;

                    partOfSpeechIndex = partsOfSpeech.Count();
                    partsOfSpeech.Add(partOfSpeech);
                    partsOfSpeechIndexDictionary.Add(categoryString, partOfSpeechIndex);
                }
            }
            else if (!String.IsNullOrEmpty(categoryString))
            {
                if (!partsOfSpeechIndexDictionary.TryGetValue(categoryString, out partOfSpeechIndex))
                {
                    PartOfSpeech partOfSpeech = new PartOfSpeech();
                    partOfSpeech.FullName = "(unknown) " + categoryString;

                    abbreviatedPartOfSpeechName = sense.GetLexicalCategoryAbbreviation(LanguageLookup.English);

                    if (categoryString == "(auto)")
                        abbreviatedPartOfSpeechName += " Au";

                    partOfSpeech.Abbreviation = abbreviatedPartOfSpeechName;

                    partOfSpeechIndex = partsOfSpeech.Count();
                    partsOfSpeech.Add(partOfSpeech);
                    partsOfSpeechIndexDictionary.Add(categoryString, partOfSpeechIndex);
                }
            }
            else if (sense.Category != LexicalCategory.Unknown)
            {
                categoryString = category.ToString();

                if (!partsOfSpeechIndexDictionary.TryGetValue(categoryString, out partOfSpeechIndex))
                {
                    abbreviatedPartOfSpeechName = sense.GetLexicalCategoryAbbreviation(LanguageLookup.English);
                    PartOfSpeech partOfSpeech = new PartOfSpeech();
                    partOfSpeech.FullName = categoryString;
                    partOfSpeech.Abbreviation = abbreviatedPartOfSpeechName;
                    partOfSpeechIndex = partsOfSpeech.Count();
                    partsOfSpeech.Add(partOfSpeech);
                    partsOfSpeechIndexDictionary.Add(categoryString, partOfSpeechIndex);
                }
            }

            return partOfSpeechIndex;
        }
#endif

        protected bool GetInflectionAndLemmaIndex(
            Sense sense,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            Dictionary<string, int> inflectionsIndexDictionary,
            List<Inflection> inflections,
            out int inflectionIndex,
            out int lemmaIndex)
        {
            bool returnValue = false;

            inflectionIndex = -1;
            lemmaIndex = -1;

            if (sense.HasInflections())
            {
                foreach (Language.Inflection anInflection in sense.Inflections)
                {
                    Designator designation = anInflection.Designation;
                    string fullName = designation.Label;
                    string abbreviation = "";
                    string targetLemma = anInflection.GetDictionaryForm(targetLanguageID);

                    if (!inflectionsIndexDictionary.TryGetValue(fullName, out inflectionIndex))
                    {
                        Inflection inflection = new Inflection();
                        inflection.FullName = fullName;
                        List<NameValuePair> designators = new List<NameValuePair>();

                        if (designation.Classifications != null)
                        {
                            foreach (Classifier classifier in designation.Classifications)
                            {
                                NameValuePair nvp = new NameValuePair();
                                nvp.Name = classifier.Name;
                                nvp.Value = classifier.Text;
                                designators.Add(nvp);
                                abbreviation += GetInflectionCategoryAbbreviation(classifier.Name, classifier.Text);
                            }
                        }

                        inflection.Abbreviation = abbreviation;
                        inflection.Designators = designators.ToArray();
                        inflectionIndex = inflections.Count();
                        inflections.Add(inflection);
                        inflectionsIndexDictionary.Add(fullName, inflectionIndex);
                    }

                    GetDictionaryEntry(targetLemma, targetLanguageID, out lemmaIndex);

                    returnValue = true;
                }
            }

            return returnValue;
        }

        protected static string[] InflectionCategoryAbbreviationInitializers =
        {
            /* Aspect */    "Imperfect", "Imp",  "Imperfect1", "Imp1",  "Imperfect2", "Imp2",  "Progressive", "Prog",  "Perfect", "Pfct",
            /* Gender */    "Masculine", "Masc",  "Feminine", "Fem",
            /* Mood */      "Indicative", "Ind",  "Conditional", "Cond",  "Subjunctive", "Subj",  "Imperative", "Imp",
            /* Number */    "Singular", "Sngl",  "Plural", "Plur",
            /* Person */    "First", "Frst",  "Second", "Scnd",  "Third", "Thrd",
            /* Polarity */  "Positive", "Pos",  "Negative", "Neg",
            /* Politeness */"Informal", "Infm",  "Informal1", "Infm1",  "Formal", "Form",  "Informal2", "Infm2",
            /* Special */   "Dictionary", "Dic",  "Stem", "Stm",  "Infinitive", "Infn",  "Gerund", "Ger",  "Participle", "Part",
            /* Tense */     "Present", "Pres",  "Past", "Pst",  "Future", "Fut",
            /* Contraction */ "Contraction", "Cont"
        };

        protected static Dictionary<string, string> InflectionCategoryAbbreviationMap = null;

        protected string GetInflectionCategoryAbbreviation(string classifierName, string classifierValue)
        {
            string abbrev;

            if (InflectionCategoryAbbreviationMap == null)
            {
                InflectionCategoryAbbreviationMap = new Dictionary<string, string>();

                int count = InflectionCategoryAbbreviationInitializers.Length;
                int index;

                for (index = 0; index < count; index += 2)
                    InflectionCategoryAbbreviationMap.Add(InflectionCategoryAbbreviationInitializers[index], InflectionCategoryAbbreviationInitializers[index + 1]);
            }

            if (!InflectionCategoryAbbreviationMap.TryGetValue(classifierValue, out abbrev))
            {
                int count = InflectionCategoryAbbreviationInitializers.Length;
                int index;

                for (index = count - 2; index >= 0; index -= 2)
                {
                    if (classifierValue.StartsWith(InflectionCategoryAbbreviationInitializers[index]))
                    {
                        abbrev = InflectionCategoryAbbreviationInitializers[index + 1];
                        break;
                    }
                }
            }

            if (String.IsNullOrEmpty(abbrev))
                abbrev = classifierValue.Substring(0, 3);

            return abbrev;
        }

        protected void GetLessons(
            List<BaseObjectNode> lessons,
            object target,
            int level)
        {
            PathDesignator pathDesignator = null;

            if (target is BaseObjectNodeTree)
            {
                BaseObjectNodeTree tree = target as BaseObjectNodeTree;

                if ((level != -1) && !MatchNodePath(tree, level, out pathDesignator))
                    return;

                if (tree.HasChildren())
                {
                    foreach (BaseObjectNode childNode in tree.Children)
                        GetLessons(lessons, childNode, level + 1);
                }
            }
            else if (target is BaseObjectNode)
            {
                BaseObjectNode node = target as BaseObjectNode;

                if ((level != -1) && !MatchNodePath(node, level, out pathDesignator))
                    return;

                if (node.IsGroup())
                {
                    if (node.HasChildren())
                    {
                        foreach (BaseObjectNode childNode in node.Children)
                            GetLessons(lessons, childNode, level + 1);
                    }
                }
                else
                    lessons.Add(node);
            }
            else if (target is BaseObjectContent)
            {
                BaseObjectContent content = target as BaseObjectContent;
                BaseObjectNode node = content.Node;

                if ((level != -1) && !MatchNodePath(node, level, out pathDesignator))
                    return;

                lessons.Add(node);
            }
            else
                throw new Exception("Unexpected target type: " + target.GetType().Name);
        }

        // Get an array of strings for the given root and its alternate languages.
        protected string[] GetMultiString(
            MultiLanguageString mls,
            LanguageID targetRootLanguageID,
            LanguageID hostLanguageID,
            string suffix)
        {
            List<LanguageID> familyLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(targetRootLanguageID);
            List<string> multiString = new List<string>();
            string languageString;

            familyLanguageIDs.Add(hostLanguageID);

            foreach (LanguageID lid in familyLanguageIDs)
            {
                languageString = mls.Text(lid) + suffix;
                multiString.Add(languageString);
            }

            return multiString.ToArray();
        }

        // Get an array of strings for the given root and its alternate languages.
        protected string[] GetTitlePath(
            BaseObjectNode node,
            LanguageID targetRootLanguageID,
            LanguageID hostLanguageID,
            string suffix)
        {
            BaseObjectNode aNode;
            List<LanguageID> familyLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(targetRootLanguageID);
            List<string> multiString = new List<string>();
            familyLanguageIDs.Add(hostLanguageID);
            int languageCount = familyLanguageIDs.Count();
            int languageIndex;
            string languageString;

            aNode = node;

            while (aNode != null)
            {
                for (languageIndex = languageCount - 1; languageIndex >= 0; languageIndex--)
                {
                    LanguageID lid = familyLanguageIDs[languageIndex];
                    languageString = aNode.Title.Text(lid) + (aNode == node ? suffix : "");
                    multiString.Insert(0, languageString);
                }

                aNode = aNode.Parent;
            }

            if (!node.IsTree())
            {
                aNode = node.Tree;

                for (languageIndex = languageCount - 1; languageIndex >= 0; languageIndex--)
                {
                    LanguageID lid = familyLanguageIDs[languageIndex];
                    languageString = aNode.Title.Text(lid);
                    multiString.Insert(0, languageString);
                }
            }

            return multiString.ToArray();
        }

        // Get an array of language codes for the root and alternate languages.
        protected string[] GetFamilyLanguageCodes(LanguageID rootLanguageID)
        {
            string languageCode = LanguageCodes.GetCode_ll_CC_FromCode2(rootLanguageID.LanguageCultureExtensionCode);
            List<LanguageID> familyLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(rootLanguageID);
            List<string> languageCodes = new List<string>();

            foreach (LanguageID lid in familyLanguageIDs)
            {
                string suffix = GetLanguageInstanceSuffix(lid);
                languageCodes.Add(languageCode + suffix);
            }

            return languageCodes.ToArray();
        }

        // Get an array of language codes for the root and alternate languages.
        protected string GetLanguageCode(LanguageID languageID)
        {
            string languageCode = LanguageCodes.GetCode_ll_CC_FromCode2(languageID.LanguageCultureExtensionCode);
            return languageCode;
        }

        // Get the language code suffix extension for a root or alternate language ID.
        protected string GetLanguageInstanceSuffix(LanguageID languageID)
        {
            string instanceSuffix = String.Empty;

            switch (languageID.ExtensionCode)
            {
                case "rm":
                case "rj":
                    instanceSuffix = "-r";
                    break;
                case "kn":
                    instanceSuffix = "-k";
                    break;
                case "vw":
                    instanceSuffix = "-v";
                    break;
                default:
                    switch (languageID.CultureCode)
                    {
                        case "CHS":
                            instanceSuffix = "-s";
                            break;
                        case "CHT":
                            instanceSuffix = "-t";
                            break;
                        default:
                            instanceSuffix = "";
                            break;
                    }
                    break;
            }

            return instanceSuffix;
        }

        public override void LoadFromArguments()
        {
            base.LoadFromArguments();

            TargetLanguageIDs = GetLanguageIDListArgumentDefaulted("TargetLanguageIDs", "languagelist", "r", TargetLanguageIDs,
                "Target languages", Format.TargetLanguageIDsHelp);

            HostLanguageIDs = GetLanguageIDListArgumentDefaulted("HostLanguageIDs", "languagelist", "r", HostLanguageIDs,
                "Host languages", Format.HostLanguageIDsHelp);

            NodePathPatterns = GetArgumentStringListDefaulted("NodePathPatterns", "bigstring", "w",
                NodePathPatterns, "Node path patterns", NodePathPatternsHelp);

            OutputHierarchy = GetFlagArgumentDefaulted("OutputHierarchy", "flag", "w", OutputHierarchy, "Output hierarchy",
                OutputHierarchyHelp, null, null);

            OutputMediaDirectory = GetArgumentDefaulted("OutputMediaDirectory", "string", "w", OutputMediaDirectory,
                "Output media directory", OutputMediaDirectoryHelp);

            OutputIndented = GetFlagArgumentDefaulted("OutputIndented", "flag", "w", OutputIndented, "Output indented",
                OutputIndentedHelp, null, null);

            IncludeInsets = GetFlagArgumentDefaulted("IncludeInsets", "flag", "r", IncludeInsets, "Include insets",
                IncludeInsetsHelp, null, null);

            IsParagraphsOnly = GetFlagArgumentDefaulted("IsParagraphsOnly", "flag", "r", IsParagraphsOnly, "Paragraphs only",
                IsParagraphsOnlyHelp, null, null);

            IsTranslateMissingItems = GetFlagArgumentDefaulted("IsTranslateMissingItems", "flag", "r", IsTranslateMissingItems, "Translate missing items",
                IsTranslateMissingItemsHelp, null, null);

            IsAddNewItemsToDictionary = GetFlagArgumentDefaulted("IsAddNewItemsToDictionary", "flag", "r", IsAddNewItemsToDictionary, "Add new items to dictionary",
                IsAddNewItemsToDictionaryHelp, null, null);

            IsSynthesizeMissingAudio = GetFlagArgumentDefaulted("IsSynthesizeMissingAudio", "flag", "w", IsSynthesizeMissingAudio, "Synthesize missing audio",
                IsSynthesizeMissingAudioHelp, null, null);

            IsForceAudio = GetFlagArgumentDefaulted("IsForceAudio", "flag", "r", IsForceAudio, "Force synthesis of audio",
                IsForceAudioHelp, null, null);

            IsNoAudio = GetFlagArgumentDefaulted("IsNoAudio", "flag", "r", IsNoAudio, IsNoAudioPrompt,
                IsNoAudioHelp, null, null);

            //OutputSentenceMedia = GetFlagArgumentDefaulted("OutputSentenceMedia", "flag", "w", OutputSentenceMedia,
            //    "Output sentence media", OutputSentenceMediaHelp, null, null);
        }

        public override void SaveToArguments()
        {
            base.SaveToArguments();

            SetArgumentStringList("NodePathPatterns", "string", "w", NodePathPatterns, "Node path patterns",
                NodePathPatternsHelp, null, null);

            SetFlagArgument("OutputHierarchy", "flag", "w", OutputHierarchy, "Output hierarchy",
                OutputHierarchyHelp, null, null);

            SetArgument("OutputMediaDirectory", "string", "w", OutputMediaDirectory,
                "Output media directory", OutputMediaDirectoryHelp, null, null);

            SetFlagArgument("OutputIndented", "flag", "w", OutputIndented, "Output indented",
                OutputIndentedHelp, null, null);

            SetFlagArgument("IncludeInsets", "flag", "r", IncludeInsets, "Include insets",
                IncludeInsetsHelp, null, null);

            SetFlagArgument("IsParagraphsOnly", "flag", "r", IsParagraphsOnly, "Paragraphs only",
                IsParagraphsOnlyHelp, null, null);

            SetFlagArgument("IsTranslateMissingItems", "flag", "r", IsTranslateMissingItems, "Translate missing items",
                IsTranslateMissingItemsHelp, null, null);

            SetFlagArgument("IsAddNewItemsToDictionary", "flag", "r", IsAddNewItemsToDictionary, "Add new items to dictionary",
                IsAddNewItemsToDictionaryHelp, null, null);

            SetFlagArgument("IsSynthesizeMissingAudio", "flag", "w", IsSynthesizeMissingAudio, "Synthesize missing audio",
                IsSynthesizeMissingAudioHelp, null, null);

            SetFlagArgument("IsForceAudio", "flag", "r", IsForceAudio, "Force synthesis of audio",
                IsForceAudioHelp, null, null);

            SetFlagArgument("IsNoAudio", "flag", "r", IsNoAudio, IsNoAudioPrompt,
                IsNoAudioHelp, null, null);

            //SetFlagArgument("OutputSentenceMedia", "flag", "w", OutputSentenceMedia,
            //    "Output sentence media", OutputSentenceMediaHelp, null, null);
        }

        public override void DumpArguments(string label)
        {
            base.DumpArguments(label);

            DumpArgumentList<String>("Node path patterns", NodePathPatterns);
        }

        public static string EmbarkDictionarySourceName = "Embark";

        protected static int _EmbarkDictionarySourceID = 0;
        public static int EmbarkDictionarySourceID
        {
            get
            {
                if (_EmbarkDictionarySourceID == 0)
                    _EmbarkDictionarySourceID = ApplicationData.DictionarySourcesLazy.Add(EmbarkDictionarySourceName);

                return _EmbarkDictionarySourceID;
            }
        }

        protected static List<int> _EmbarkDictionarySourceIDList = null;
        public static List<int> EmbarkDictionarySourceIDList
        {
            get
            {
                if (_EmbarkDictionarySourceIDList == null)
                    _EmbarkDictionarySourceIDList = new List<int>(1) { EmbarkDictionarySourceID };

                return _EmbarkDictionarySourceIDList;
            }
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
                    else if ((importExport == "Export") && (capability == "UseFlags"))
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

        public static new string TypeStringStatic { get { return "Embark"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
