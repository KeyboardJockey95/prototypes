using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatGizaAlignment : FormatPatterned
    {
        // Argument data.
        public override LanguageID TargetLanguageID { get; set; }
        public static string TargetLanguageIDPrompt = "Target language code";
        public static string TargetLanguageIDHelp = "Enter the target language code.";

        public override LanguageID HostLanguageID { get; set; }
        public static string HostLanguageIDPrompt = "Host language code";
        public static string HostLanguageIDHelp = "Enter the host language code.";

        public string ContentKey { get; set; }
        protected static string ContentKeyPrompt = "Content key";
        protected static string ContentKeyHelp = "Enter the content key of the content item to add the alignment to.";

        protected string ParagraphOverrideFilePath { get; set; }
        protected static string ParagraphOverrideFilePathPrompt = "Paragraph override file path";
        protected string ParagraphOverrideFilePathHelp = "Enter the file path for the paragraph override input. Lines with: Book of Mormon/Alma/Chapter 32/Text/23";

        public int StartLineNumber { get; set; }
        protected static string StartLineNumberPrompt = "Start line number";
        protected static string StartLineNumberHelp = "Enter the alignment file sentence number to start taking, or -1 for all.";

        public int EndLineNumber { get; set; }
        protected static string EndLineNumberPrompt = "End line number";
        protected static string EndLineNumberHelp = "Enter the alignment file sentence number after which to stop taking, or -1 for all.";

        //public bool IsParagraphsOnly = true;
        protected static string IsParagraphsOnlyPrompt = "Paragraphs only";
        //protected static string IsParagraphsOnlyHelp = "Set this to true to parse paragraphs only, not sentences";

        public bool IsMergedAlignment { get; set; }
        protected static string IsMergedAlignmentPrompt = "Merged alignment";
        protected static string IsMergedAlignmentHelp = "Set this to true to import from an merged alignment file.";

        public bool IsFixupBlanks { get; set; }
        protected static string IsFixupBlanksPrompt = "Fixup blanks";
        protected static string IsFixupBlanksHelp = "Set this to true to try to fix up missing alignments.";

        public string DumpLineNumbers { get; set; }
        protected static string DumpLineNumbersPrompt = "Dump line numbesr";
        protected static string DumpLineNumbersHelp = "Set this to the line numbers of the alignment to dump (i.e. \"1,3-7\"), or -1 to dump nothing (default).";

        protected string DisplayFilePath { get; set; }
        protected static string DisplayFilePathPrompt = "Display file path";
        protected string DisplayFilePathHelp = "Enter the file path for the display format alignment file output.";

        public bool IsDisplayDictionary { get; set; }
        protected static string IsDisplayDictionaryPrompt = "Display dictionary";
        protected static string IsDisplayDictionaryHelp = "Set this to true to display dictonary definitions with the alignments.";

        // Implementation data.
        protected List<string> ParagraphOverridePaths;
        public StudySentenceList StudySentenceItems;
        protected StreamWriter DisplayWriter;
        protected List<int> DumpLineNumberStartStopPairs;
        protected int GizaLineCount;
        protected int ExtraLineCount;

        private static string FormatDescription = "GIZA++ word alignment import format.";

        public FormatGizaAlignment()
            : base("Line", "GizaAlignment", "FormatGizaAlignment", FormatDescription, String.Empty, String.Empty,
                  "text/plain", ".txt", null, null, null, null, null)
        {
            ClearFormatGizaAlignment();
        }

        public FormatGizaAlignment(FormatPatterned other)
            : base(other)
        {
            if (other is FormatGizaAlignment)
                CopyFormatGizaAlignment(other as FormatGizaAlignment);
        }

        // For derived classes.
        public FormatGizaAlignment(string name, string type, string description, string targetType, string importExportType,
                string mimeType, string defaultExtension,
                UserRecord userRecord, UserProfile userProfile, IMainRepository repositories, LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base("Line", name, type, description, targetType, importExportType, mimeType, defaultExtension,
                  userRecord, userProfile, repositories, languageUtilities, nodeUtilities)
        {
            ClearFormatGizaAlignment();
        }

        public void ClearFormatGizaAlignment()
        {
            UseComments = false;
            TargetLanguageID = TargetLanguageID;
            HostLanguageID = HostLanguageID;
            ContentKey = "Text";
            ParagraphOverrideFilePath = String.Empty;
            StartLineNumber = -1;
            EndLineNumber = -1;
            IsParagraphsOnly = false;
            IsMergedAlignment = false;
            IsFixupBlanks = false;
            DumpLineNumbers = "";
            DisplayFilePath = String.Empty;
            IsDisplayDictionary = false;
            ParagraphOverridePaths = new List<string>();
            StudySentenceItems = null;
            DisplayWriter = null;
            DumpLineNumberStartStopPairs = null;
            GizaLineCount = 0;
            ExtraLineCount = 0;
        }

        public void CopyFormatGizaAlignment(FormatGizaAlignment other)
        {
            UseComments = false;
            TargetLanguageID = other.TargetLanguageID;
            HostLanguageID = other.HostLanguageID;
            ContentKey = other.ContentKey;
            ParagraphOverrideFilePath = other.ParagraphOverrideFilePath;
            StartLineNumber = other.StartLineNumber;
            EndLineNumber = other.EndLineNumber;
            IsParagraphsOnly = other.IsParagraphsOnly;
            IsMergedAlignment = other.IsMergedAlignment;
            IsFixupBlanks = other.IsFixupBlanks;
            DumpLineNumbers = other.DumpLineNumbers;
            DisplayFilePath = other.DisplayFilePath;
            IsDisplayDictionary = other.IsDisplayDictionary;
            StudySentenceItems = null;
            DisplayWriter = null;
            DumpLineNumberStartStopPairs = null;
            GizaLineCount = 0;
            ExtraLineCount = 0;
        }

        public override Format Clone()
        {
            return new FormatGizaAlignment(this);
        }

        public override void LoadFromArguments()
        {
            TargetLanguageID = GetLanguageIDArgumentDefaulted("TargetLanguageID", "languageID", "rw",
                TargetLanguageID, TargetLanguageIDPrompt, TargetLanguageIDHelp);

            HostLanguageID = GetLanguageIDArgumentDefaulted("HostLanguageID", "languageID", "rw",
                HostLanguageID, HostLanguageIDPrompt, HostLanguageIDHelp);

            ContentKey = GetArgumentDefaulted("ContentKey", "string", "rw", ContentKey,
                ContentKeyPrompt, ContentKeyHelp);

            ParagraphOverrideFilePath = GetArgumentDefaulted("ParagraphOverrideFilePath", "string", "rw",
                ParagraphOverrideFilePath, ParagraphOverrideFilePathPrompt, ParagraphOverrideFilePathHelp);

            StartLineNumber = GetIntegerArgumentDefaulted("StartLineNumber", "integer", "rw", StartLineNumber,
                StartLineNumberPrompt, StartLineNumberHelp);

            EndLineNumber = GetIntegerArgumentDefaulted("EndLineNumber", "integer", "rw", EndLineNumber,
                EndLineNumberPrompt, EndLineNumberHelp);

            IsParagraphsOnly = GetFlagArgumentDefaulted("IsParagraphsOnly", "flag", "rw", IsParagraphsOnly,
                IsParagraphsOnlyPrompt, IsParagraphsOnlyHelp, null, null);

            IsMergedAlignment = GetFlagArgumentDefaulted("IsMergedAlignment", "flag", "rw", IsMergedAlignment,
                IsMergedAlignmentPrompt, IsMergedAlignmentHelp, null, null);

            IsFixupBlanks = GetFlagArgumentDefaulted("IsFixupBlanks", "flag", "rw", IsFixupBlanks,
                IsFixupBlanksPrompt, IsFixupBlanksHelp, null, null);

            DumpLineNumbers = GetArgumentDefaulted("DumpLineNumbers", "string", "rw", DumpLineNumbers, DumpLineNumbersPrompt,
                DumpLineNumbersHelp);

            DisplayFilePath = GetArgumentDefaulted("DisplayFilePath", "string", "rw", DisplayFilePath, DisplayFilePathPrompt,
                DisplayFilePathHelp);

            IsDisplayDictionary = GetFlagArgumentDefaulted("IsDisplayDictionary", "flag", "rw", IsDisplayDictionary,
                IsDisplayDictionaryPrompt, IsDisplayDictionaryHelp, null, null);

            FixupGizaArguments();
        }

        public override void SaveToArguments()
        {
            DeleteBeforeImport = false;
            UseComments = false;
            CommentPrefix = null;
            Pattern = "%{s}\\n%{s}\\n%{s}";
            RowCount = 3;

            SetLanguageIDArgument("TargetLanguageID", "languageID", "rw", TargetLanguageID,
                TargetLanguageIDPrompt, TargetLanguageIDHelp);

            SetLanguageIDArgument("HostLanguageID", "languageID", "rw", HostLanguageID,
                HostLanguageIDPrompt, HostLanguageIDHelp);

            SetArgument("ContentKey", "string", "rw", ContentKey,
                ContentKeyPrompt, ContentKeyHelp, null, null);

            SetArgument("ParagraphOverrideFilePath", "string", "rw",
                ParagraphOverrideFilePath, ParagraphOverrideFilePathPrompt, ParagraphOverrideFilePathHelp, null, null);

            SetIntegerArgument("StartLineNumber", "integer", "rw", StartLineNumber,
                StartLineNumberPrompt, StartLineNumberHelp);

            SetIntegerArgument("EndLineNumber", "integer", "rw", EndLineNumber,
                EndLineNumberPrompt, EndLineNumberHelp);

            SetFlagArgument("IsParagraphsOnly", "flag", "rw", IsParagraphsOnly,
                IsParagraphsOnlyPrompt, IsParagraphsOnlyHelp, null, null);

            SetFlagArgument("IsMergedAlignment", "flag", "rw", IsMergedAlignment, IsMergedAlignmentPrompt,
                IsMergedAlignmentHelp, null, null);

            SetFlagArgument("IsFixupBlanks", "flag", "rw", IsFixupBlanks,
                IsFixupBlanksPrompt, IsFixupBlanksHelp, null, null);

            SetArgument("DumpLineNumbers", "string", "rw", DumpLineNumbers, DumpLineNumbersPrompt,
                DumpLineNumbersHelp, null, null);

            SetArgument("DisplayFilePath", "string", "rw", DisplayFilePath, DisplayFilePathPrompt,
                DisplayFilePathHelp, null, null);

            SetFlagArgument("IsDisplayDictionary", "flag", "rw", IsDisplayDictionary,
                IsDisplayDictionaryPrompt, IsDisplayDictionaryHelp, null, null);
        }

        protected void FixupGizaArguments()
        {
            DeleteBeforeImport = false;
            UseComments = false;
            CommentPrefix = null;

            if (IsMergedAlignment)
            {
                Pattern = "%{s}\\n%{s}\\n%{s}\\n%{s}\\n%{s}\\n%{s}\\n%{s}\\n%{s}\\n";
                RowCount = 8;
                ExtraLineCount = 1;
            }
            else
            {
                Pattern = "%{s}\\n%{s}\\n%{s}";
                RowCount = 3;
                ExtraLineCount = 0;
            }

            TargetLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(TargetLanguageID);
            HostLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(HostLanguageID);
            UniqueLanguageIDs = LanguageID.ConcatenateUnqueList(HostLanguageIDs, TargetLanguageIDs);
            LanguageDescriptors = UserProfile.LanguageDescriptors;

            if (UserProfile != null)
                UILanguageID = UserProfile.UILanguageID;
            else
                UILanguageID = HostLanguageID;
        }

        protected virtual void InitializeRepositories()
        {
            if (Repositories == null)
                throw new Exception("Repositories not set in format.");
        }

        protected override void PreRead(int progressCount)
        {
            if (Timer != null)
                Timer.Start();

            ContinueProgress(ProgressCountBase + progressCount);

            InitializeRepositories();

            LineNumber = 0;

            // Get paragraph override paths.
            if (!InputTextLines(ParagraphOverrideFilePath, ParagraphOverridePaths, "paragraph override paths"))
                return;

            LoadStudyItems();

            if (!String.IsNullOrEmpty(DumpLineNumbers))
            {
                string verseKey;
                PathDesignator.GetStartStopPairs(DumpLineNumbers, out DumpLineNumberStartStopPairs, out verseKey);
            }
            else
                DumpLineNumberStartStopPairs = null;

            if (!String.IsNullOrEmpty(DisplayFilePath))
            {
                if (FileSingleton.Exists(DisplayFilePath))
                    FileSingleton.Delete(DisplayFilePath);

                Stream stream = FileSingleton.OpenWrite(DisplayFilePath);

                if (stream != null)
                    DisplayWriter = new StreamWriter(stream);
            }
        }

        protected override void PostRead()
        {
            //SaveStudyItems();

            //StudyItems = null;

            if (DisplayWriter != null)
            {
                DisplayWriter.Flush();
                FileSingleton.Close(DisplayWriter.BaseStream);
                DisplayWriter = null;
            }

            // We did multiple target, we might not have save any but the first, so we save it all again.
            UpdateTargets();

            EndContinuedProgress();
        }

        protected void LoadStudyItems()
        {
            StudySentenceItems = new StudySentenceList(UniqueLanguageIDs);

            if ((Targets != null) && (Targets.Count() != 0))
            {
                ItemWalker<FormatGizaAlignment> walker = new ItemWalker<FormatGizaAlignment>();

                walker.VisitStudyItemFunction = LoadStudyItemStatic;

                foreach (IBaseObject target in Targets)
                {
                    if (target is BaseObjectNodeTree)
                        walker.WalkTree(target as BaseObjectNodeTree, this);
                    else if (target is BaseObjectNode)
                        walker.WalkNode(target as BaseObjectNode, this);
                    else if (target is BaseObjectContent)
                        walker.WalkContent(target as BaseObjectContent, this);
                    else if (target is ContentStudyList)
                        walker.WalkStudyList(target as ContentStudyList, this);
                    else
                        throw new Exception("FormatGiza doesn't support " + target.GetType().Name);
                }
            }
            else
            {
                throw new Exception("Can only read alignment into existing content.");
            }
        }

        public static bool LoadStudyItemStatic(MultiLanguageItem studyItem,
            ItemWalker<FormatGizaAlignment> walker, FormatGizaAlignment context)
        {
            BaseObjectContent content = walker.Content;

            if (content == null)
            {
                if (walker.StudyList != null)
                    content = walker.StudyList.Content;
            }

            if (content == null)
                throw new Exception("LoadStudyItemStatic: null content");

            if (content.KeyString == context.ContentKey)
                context.LoadStudyItem(studyItem);

            return true;
        }

        public bool LoadStudyItem(MultiLanguageItem studyItem)
        {
            bool hasTarget = studyItem.HasText(TargetLanguageID);
            bool hasHost = studyItem.HasText(HostLanguageID);

            if (hasTarget && hasHost)
            {
                string studyItemPath = studyItem.GetNamePathStringWithOrdinalInclusive(UILanguageID);

                //if (StudySentenceItems.StudySentenceCount() == 85)
                //    DumpString("StudySentenceItems.StudySentenceCount() == " + StudySentenceItems.StudySentenceCount().ToString());

                if ((ParagraphOverridePaths != null) && ParagraphOverridePaths.Contains(studyItemPath))
                    studyItem.JoinSentenceRuns(UniqueLanguageIDs);
                else if (!ContentUtilities.ParseSentenceRuns(
                        studyItem,
                        TargetLanguageIDs,
                        HostLanguageIDs,
                        UniqueLanguageIDs,
                        SentenceParsingAlgorithm.RatioWalker,
                        SentenceParsingFallback.DoNothing))
                    throw new Exception("LoadStudyItem: Sentence parsing failed to produce aligned sentences: " +
                        studyItem.GetNamePathStringWithOrdinalInclusive(LanguageLookup.English));

                if (!StudySentenceItems.Add(studyItem))
                    throw new Exception("LoadStudyItem: Sentence count mismatch: " +
                        studyItem.GetNamePathStringWithOrdinalInclusive(LanguageLookup.English));

                if (StartLineNumber != -1)
                {
                    studyItem.ClearWordMappings(TargetLanguageIDs, HostLanguageIDs);

                    if (IsMergedAlignment)
                        studyItem.ClearWordMappings(HostLanguageIDs, TargetLanguageIDs);
                }
            }
            else if (hasTarget != hasHost)
            {
                string msg = "LoadStudyItem: Missing " + (!hasTarget ? "target" : "host")
                    + " text for: " + studyItem.GetNamePathStringWithOrdinalInclusive(LanguageLookup.English);
                //throw new Exception(msg);
                PutMessage(msg);
                ApplicationData.Global.PutConsoleMessage(msg);
            }

            return true;
        }

        protected override void ScanLine(string pattern, string line, ref List<Annotation> annotations)
        {
            ScanLineAlignment(line);
        }

        protected void ScanLineAlignment(string line)
        {
            line = line.Trim();

            if (String.IsNullOrEmpty(line))
                return;

            GizaLineCount++;

            if (RowCount > 1)
                line = line.Replace("\r", "");

            string[] lines = line.Split(LanguageLookup.NewLine);

            if (lines.Count() != RowCount - ExtraLineCount)
                throw new Exception("Didn't get " + RowCount.ToString() + " lines on line " + GizaLineCount.ToString() + ": " + line);

            string targetInfoLine;
            string targetTextLine;
            string targetAlignmentLine;
            string hostInfoLine;
            string hostTextLine;
            string hostAlignmentLine;

            if (IsMergedAlignment)
            {
                targetInfoLine = lines[0];
                targetTextLine = lines[2].Trim();
                targetAlignmentLine = lines[4];
                hostInfoLine = lines[1];
                hostTextLine = lines[3].Trim();
                hostAlignmentLine = lines[6];
            }
            else
            {
                targetInfoLine = lines[0];
                targetTextLine = lines[1].Trim();
                targetAlignmentLine = lines[2];
                hostInfoLine = null;
                hostTextLine = null;
                hostAlignmentLine = null;
            }

            int studySentenceItemIndex = ScanStudySentenceItemIndex(targetInfoLine);

            if ((StartLineNumber > 0) && (studySentenceItemIndex < (StartLineNumber - 1)))
                return;

            if ((EndLineNumber > 0) && (studySentenceItemIndex > (EndLineNumber - 1)))
                return;

            if (StartLineNumber > 0)
                studySentenceItemIndex -= StartLineNumber;
            else
                studySentenceItemIndex -= 1;

            if (studySentenceItemIndex >= StudySentenceItems.StudySentenceCount())
                throw new Exception("Item numbers don't match on line " + GizaLineCount.ToString() + ".");

            //if (studySentenceItemIndex == 287)
            //    DumpString("studySentenceItemIndex: " + studySentenceItemIndex.ToString());

            MultiLanguageItem studyItem = null;
            LanguageItem hostLanguageItem = null;
            LanguageItem targetLanguageItem = null;
            TextRun targetSentenceRun = null;
            TextRun hostSentenceRun = null;
            int sentenceIndex = -1;

            string targetKey = null;

            for (;;)
            {
                bool suceeded = StudySentenceItems.GetTargetHostStudyItemLanguageItemSentenceRun(
                    studySentenceItemIndex,
                    TargetLanguageID,
                    HostLanguageID,
                    out studyItem,
                    out targetLanguageItem,
                    out hostLanguageItem,
                    out targetSentenceRun,
                    out hostSentenceRun,
                    out sentenceIndex);

                if (!suceeded)
                {
                    DumpString("StudySentenceItems.GetTargetHostStudyItemLanguageItemSentenceRun failed:"
                        + " studySentenceItemIndex=" + studySentenceItemIndex.ToString()
                        + " line=" + line);
                    StudySentenceItems.Delete(studySentenceItemIndex);
                    continue;
                }

#if false
                DumpSentenceLine(
                    studySentenceItemIndex,
                    TargetLanguageID,
                    HostLanguageID,
                    studyItem,
                    targetLanguageItem,
                    hostLanguageItem,
                    targetSentenceRun,
                    hostSentenceRun,
                    sentenceIndex,
                    line);
#endif

                targetKey = ScanPreprocessLine(studyItem.Text(TargetLanguageID));

                if (!String.IsNullOrEmpty(targetKey))
                    break;

                StudySentenceItems.Delete(studySentenceItemIndex);

                if (StudySentenceItems.StudySentenceCount() == 0)
                    return;
            }

            hostLanguageItem.WordRunCheck(Repositories.Dictionary);
            targetLanguageItem.WordRunCheck(Repositories.Dictionary);

            foreach (LanguageID hostLanguageID in HostLanguageIDs)
            {
                ScanOneAlignment(
                    studyItem,
                    sentenceIndex,
                    TargetLanguageIDs,
                    hostLanguageID,
                    targetTextLine,
                    targetAlignmentLine);
            }

            bool isDumpLine = (DumpLineNumberStartStopPairs == null ? true : PathDesignator.MatchStartStopPair(studySentenceItemIndex + 1, DumpLineNumberStartStopPairs));

            if (IsMergedAlignment)
            {
                foreach (LanguageID targetLanguageID in TargetLanguageIDs)
                    ScanOneAlignment(
                        studyItem,
                        sentenceIndex,
                        HostLanguageIDs,
                        targetLanguageID,
                        hostTextLine,
                        hostAlignmentLine);

                if (IsFixupBlanks)
                {
                    if (isDumpLine)
                        DumpString("At entry: " + (studySentenceItemIndex + 1).ToString());

                    FixupBlanks(studyItem);
                }
            }

            if ((DisplayWriter == null) && isDumpLine)
            {
                string namePath = studyItem.GetNamePathStringInclusive(UILanguageID);
                DumpAlignment(studyItem, sentenceIndex, "Item " + (studySentenceItemIndex + 1).ToString() + " - " + namePath);
                DumpString("\n");
            }

            if ((DisplayWriter != null) && isDumpLine)
                OutputDisplayAlignment(
                    DisplayWriter,
                    studyItem,
                    sentenceIndex,
                    studyItem.GetNamePathStringInclusive(UILanguageID));

#if false
            string namePath = studyItem.GetNamePathStringInclusive(UILanguageID);

            if (namePath == "Spanish Book of Mormon/1 Nephi/Chapter 3/Text/p7")
            {
                DumpAlignment(studyItem, namePath);
                //DumpString("\n");
                //XElement element = studyItem.Xml;
                //string elementString = element.ToString();
                //DumpString(elementString);
            }
#endif
        }

        protected void DumpSentenceLine(
            int globalSentenceIndex,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            MultiLanguageItem studyItem,
            LanguageItem targetLanguageItem,
            LanguageItem hostLanguageItem,
            TextRun targetSentenceRun,
            TextRun hostSentenceRun,
            int localSentenceIndex,
            string line)
        {
            string path = studyItem.GetNamePathStringInclusive(LanguageLookup.English, "/");
            string tli = (targetLanguageItem != null ? targetLanguageItem.TextPreview(40, "...") : "(li null)");
            string hli = (hostLanguageItem != null ? hostLanguageItem.TextPreview(40, "...") : "(li null)");
            string tsr = (targetLanguageItem != null ? targetLanguageItem.GetRunTextPreview(targetSentenceRun, 40, "...") : "(li null)");
            string hsr = (hostLanguageItem != null ? hostLanguageItem.GetRunTextPreview(hostSentenceRun, 40, "...") : "(li null)");

            DumpString("path=" + path
                + "\ngi=" + globalSentenceIndex.ToString()
                + "\nli=" + localSentenceIndex
                + "\nsik=" + studyItem.KeyString
                + "\ntli=" + tli
                + "\nhli=" + hli
                + "\ntsr=" + tsr
                + "\nhsr=" + hsr);
            DumpString(line);
        }

        private BaseObjectNode LastNode;

#if true
        // Phrase runs already marked.
        protected void ScanOneAlignment(
            MultiLanguageItem studyItem,
            int sentenceIndex,
            List<LanguageID> languageIDs,
            LanguageID otherLanguageID,
            string textLine,
            string alignmentLine)
        {
            LanguageItem currentLanguageItem = studyItem.LanguageItem(languageIDs[0]);
            int currentStartWordRunIndex = currentLanguageItem.GetSentenceStartWordRunIndex(sentenceIndex);
            LanguageItem otherLanguageItem = studyItem.LanguageItem(otherLanguageID);
            int otherStartWordRunIndex = otherLanguageItem.GetSentenceStartWordRunIndex(sentenceIndex);

            BaseObjectNode node = studyItem.Node;
            if (node != LastNode)
            {
                string path = node.GetNamePathString(UILanguageID, "/");
                PutStatusMessage("Processing node: " + path);
                LastNode = node;
            }

#if false
            string namePath = studyItem.GetNamePathStringInclusive(UILanguageID);

            if (namePath == "Book of Mormon/Introduction and Witnesses/The Testimony of the Prophet Joseph Smith/Text/p3")
                DumpString(namePath);
#endif

            string[] textParts = textLine.Split(LanguageLookup.Space);
            List<int> wordNumberToRunIndexMap = ScanWordNumberToRunIndexMap(textParts);
            List<WordMapping> wordMappings = ScanWordMappings(
                textLine,
                textParts,
                alignmentLine,
                wordNumberToRunIndexMap,
                languageIDs.First(),
                otherLanguageID,
                studyItem,
                sentenceIndex,
                currentStartWordRunIndex,
                otherStartWordRunIndex);

            List<LanguageID> allLanguageIDs = new List<LanguageID>(languageIDs);
            allLanguageIDs.Add(otherLanguageID);

            CheckForPossiblyMismatchedSentences(
                studyItem,
                allLanguageIDs,
                wordMappings,
                sentenceIndex);

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = studyItem.LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                int count = languageItem.WordRunCount();
                int index;
                TextRun wordRun;
                WordMapping wordMapping;
                int sentenceStartWordRunIndex;
                int sentenceWordRunCount;

                if (!languageItem.GetSentenceWordRunStartIndexAndCount(sentenceIndex, out sentenceStartWordRunIndex, out sentenceWordRunCount))
                    return;

                for (index = 0; index < sentenceWordRunCount; index++)
                {
                    wordRun = languageItem.GetWordRun(index + sentenceStartWordRunIndex);

                    if (index < wordMappings.Count())
                    {
                        wordMapping = wordMappings[index];

                        if ((wordMapping.WordIndexes != null) && (wordMapping.WordIndexes.Length != 0))
                            wordRun.SetWordMapping(wordMapping);
                        else
                            wordRun.DeleteWordMapping(otherLanguageID);

                        TextRun phraseRun = languageItem.FindLongestPhraseRunStarting(wordRun.Start);

                        if (phraseRun != null)
                        {
                            if ((wordMapping.WordIndexes != null) && (wordMapping.WordIndexes.Length != 0))
                                phraseRun.SetWordMapping(wordMapping);
                            else
                                phraseRun.DeleteWordMapping(otherLanguageID);
                        }
                    }
                }
            }

#if false
            if (namePath == "Book of Mormon/Introduction and Witnesses/The Testimony of the Prophet Joseph Smith/Text/p3")
            {
                DumpAlignment(studyItem, sentenceIndex, namePath);
                DumpString("\n");
                XElement element = studyItem.Xml;
                string elementString = element.ToString();
                DumpString(elementString);
            }
#endif
        }
#else
        // Phrase runs made here from phrase files.
        protected void ScanOneAlignment(
            MultiLanguageItem studyItem,
            int sentenceIndex,
            List<LanguageID> languageIDs,
            LanguageID otherLanguageID,
            string textLine,
            string alignmentLine)
        {
            LanguageItem currentLanguageItem = studyItem.LanguageItem(languageIDs[0]);
            int currentStartWordRunIndex = currentLanguageItem.GetSentenceStartWordRunIndex(sentenceIndex);
            LanguageItem otherLanguageItem = studyItem.LanguageItem(otherLanguageID);
            int otherStartWordRunIndex = otherLanguageItem.GetSentenceStartWordRunIndex(sentenceIndex);

            BaseObjectNode node = studyItem.Node;
            if (node != LastNode)
            {
                string path = node.GetNamePathString(UILanguageID, "/");
                PutStatusMessage("Processing node: " + path);
                LastNode = node;
            }

            string[] textParts = textLine.Split(LanguageLookup.Space);
            List<int> wordNumberToRunIndexMap = ScanWordNumberToRunIndexMap(textParts);
            List<WordMapping> wordMappings = ScanWordMappings(
                textLine,
                textParts,
                alignmentLine,
                wordNumberToRunIndexMap,
                languageIDs.First(),
                otherLanguageID,
                studyItem,
                sentenceIndex,
                currentStartWordRunIndex,
                otherStartWordRunIndex);

            List<LanguageID> allLanguageIDs = new List<LanguageID>(languageIDs);
            allLanguageIDs.Add(otherLanguageID);

            CheckForPossiblyMismatchedSentences(
                studyItem,
                allLanguageIDs,
                wordMappings,
                sentenceIndex);

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = studyItem.LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                int count = languageItem.WordRunCount();
                int index;
                TextRun wordRun;
                WordMapping wordMapping;
                int sentenceStartWordRunIndex;
                int sentenceWordRunCount;

                if (!languageItem.GetSentenceWordRunStartIndexAndCount(sentenceIndex, out sentenceStartWordRunIndex, out sentenceWordRunCount))
                    return;

                for (index = 0; index < sentenceWordRunCount; index++)
                {
                    wordRun = languageItem.GetWordRun(index + sentenceStartWordRunIndex);

                    if (index < wordMappings.Count())
                    {
                        wordMapping = wordMappings[index];

                        if ((wordMapping.WordIndexes != null) && (wordMapping.WordIndexes.Length != 0))
                            wordRun.SetWordMapping(wordMapping);
                        else
                            wordRun.DeleteWordMapping(otherLanguageID);

                        TextRun phraseRun = languageItem.FindLongestPhraseRunStarting(wordRun.Start);

                        if (phraseRun != null)
                        {
                            if ((wordMapping.WordIndexes != null) && (wordMapping.WordIndexes.Length != 0))
                                phraseRun.SetWordMapping(wordMapping);
                            else
                                phraseRun.DeleteWordMapping(otherLanguageID);
                        }
                    }
                }
            }
        }
#endif

        protected int ScanStudySentenceItemIndex(string infoLine)
        {
            string[] parts = infoLine.Split(LanguageLookup.ParenthesisCharacters);

            if (parts.Length != 3)
                throw new Exception("Info line format error: " + infoLine);

            int studySentenceItemIndex = ObjectUtilities.GetIntegerFromString(parts[1], -1);

            return studySentenceItemIndex;
        }

        protected bool IsPunctuation(char c)
        {
            return char.IsPunctuation(c) || LanguageLookup.PunctuationCharacters.Contains(c);
        }

        protected bool IsPunctuation(string s)
        {
            foreach (char c in s)
            {
                if (!char.IsPunctuation(c) && !LanguageLookup.PunctuationCharacters.Contains(c))
                    return false;
            }

            return true;
        }

        protected bool IsWhiteSpace(char c)
        {
            return char.IsWhiteSpace(c) || LanguageLookup.SpaceCharacters.Contains(c);
        }

        protected string ScanPreprocessLine(string line)
        {
            line = line.ToLower();

            int index;
            int count = line.Length;
            int wordStart = 0;
            char c;
            StringBuilder sb = new StringBuilder();

            for (index = 0; index < count; index++)
            {
                c = line[index];

                if (IsWhiteSpace(c))
                {
                    if (index > wordStart)
                    {
                        if (sb.Length != 0)
                            sb.Append(LanguageLookup.Space);

                        sb.Append(line.Substring(wordStart, index - wordStart));
                    }

                    wordStart = index + 1;
                }
                else if (IsPunctuation(c))
                {
                    if (index > wordStart)
                    {
                        if (sb.Length != 0)
                            sb.Append(LanguageLookup.Space);

                        sb.Append(line.Substring(wordStart, index - wordStart));
                    }

                    if (sb.Length != 0)
                        sb.Append(LanguageLookup.Space);

                    sb.Append(c);

                    wordStart = index + 1;
                }
            }

            if (index > wordStart)
            {
                if (sb.Length != 0)
                    sb.Append(LanguageLookup.Space);

                sb.Append(line.Substring(wordStart, index - wordStart));
            }

            return sb.ToString();
        }

        protected List<int> ScanWordNumberToRunIndexMap(string[] hostParts)
        {
            int runIndex = 0;
            List<int> wordNumberToRunIndexMap = new List<int>();

            foreach (string part in hostParts)
            {
                if (!String.IsNullOrEmpty(part) && !IsPunctuation(part))
                {
                    if (part.Contains(LanguageLookup.NonBreakSpaceString))
                    {
                        int runIndexStart = runIndex;

#if true
                        int count = 0;

                        foreach (char c in part)
                        {
                            switch (c)
                            {
                                case '\x00A0':
                                case '-':
                                    count++;
                                    break;
                                default:
                                    break;
                            }
                        }

                        int runIndexStop = runIndexStart + count;
#else
                        int runIndexStop = runIndexStart + TextUtilities.CountChars(part, LanguageLookup.NonBreakSpace);
#endif
                        wordNumberToRunIndexMap.Add(runIndexStart);

                        runIndex = runIndexStop + 1;
                    }
                    else
                        wordNumberToRunIndexMap.Add(runIndex++);
                }
                else
                    wordNumberToRunIndexMap.Add(-1);
            }

            return wordNumberToRunIndexMap;
        }

        protected List<WordMapping> ScanWordMappings(
            string hostLine,
            string[] hostParts,
            string alignLine,
            List<int> wordNumberToRunIndexMap,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            MultiLanguageItem studyItem,
            int sentenceIndex,
            int targetWordRunStartIndex,
            int hostWordRunStartIndex)
        {
            //int targetCount = targetParts.Length;
            //int targetIndex = 0;
            string[] alignParts = alignLine.Split(LanguageLookup.Space);
            int alignCount = alignParts.Length;
            int alignIndex;
            List<WordMapping> wordMappings = new List<WordMapping>();
            LanguageItem targetLanguageItem = studyItem.LanguageItem(targetLanguageID);
            LanguageItem hostLanguageItem = studyItem.LanguageItem(hostLanguageID);

#if false
            string path = studyItem.GetNamePathStringInclusive(LanguageLookup.English);

            if (path == "Book of Mormon/Introduction and Witnesses/Introduction/Text/p9")
                ApplicationData.Global.PutConsoleMessage(path);
#endif

            for (alignIndex = 0; alignIndex < alignCount;)
            {
                string targetWord = alignParts[alignIndex++].Trim();

                if (alignIndex >= alignCount)
                    break;

                /*
                if ((alignIndex != 0) && (targetWord != "NULL"))
                {
                    if (targetWord != targetParts[targetIndex])
                        throw new Exception("target words don't match."
                            + " target word: " + targetParts[targetIndex]
                            + " align word: " + targetWord
                            + " host line: " + hostLine);

                    targetIndex++;
                }
                */

                bool isWord = (!String.IsNullOrEmpty(targetWord) && !IsPunctuation(targetWord) && (targetWord != "NULL"));

                if (alignParts[alignIndex++] != "({")
                    throw new Exception("Expected \"({\" token.");

                List<int> runIndexes = (isWord ? new List<int>() : null);

                for (; alignIndex < alignCount;)
                {
                    if (alignParts[alignIndex] == "})")
                    {
                        alignIndex++;
                        break;
                    }

                    string hostWordNumberString = alignParts[alignIndex++];
                    int hostWordNumber = int.Parse(hostWordNumberString);

                    if (isWord)
                    {
                        if (hostWordNumber > wordNumberToRunIndexMap.Count())
                            throw new Exception("Host word number bigger than map count.");

                        int hostWordRunIndex = hostWordNumber - 1;
                        string hostWord = hostParts[hostWordRunIndex].Trim();

                        if (!String.IsNullOrEmpty(hostWord) && !IsPunctuation(hostWord))
                        {
                            if (hostWordRunIndex >= hostLanguageItem.WordRuns.Count())
                                continue;

                            int hostRunIndex = wordNumberToRunIndexMap[hostWordRunIndex] + hostWordRunStartIndex;

                            if (hostRunIndex >= hostLanguageItem.WordRuns.Count())
                                continue;

#if true
                            int hostTextIndexStart = hostLanguageItem.WordRuns[hostRunIndex].Start;
                            int hostTextIndexStop = hostTextIndexStart + hostWord.Length;
                            TextRun hostWordRunStop = hostLanguageItem.WordRuns.FirstOrDefault(x => x.Stop == hostTextIndexStop);

                            if (hostWordRunStop == null)
                            {
                                string message =
                                    "Couldn't find word run stop for host word \""
                                    + hostWord
                                    + "\""
                                    + "\nrun index "
                                    + hostWordRunIndex.ToString()
                                    + " text start index "
                                    + hostTextIndexStart.ToString()
                                    + " text stop index "
                                    + hostTextIndexStop.ToString()
                                    + "\nin: "
                                    + studyItem.GetNamePathStringInclusive(LanguageLookup.English)
                                    + "\nHost runs:   "
                                    + hostLanguageItem.GetPhrasesAndwordRunsWithIndexesDumpString()
                                    + "\nTarget runs: "
                                    + targetLanguageItem.GetPhrasesAndwordRunsWithIndexesDumpString();
                                ApplicationData.Global.PutConsoleMessage(message);
                                //throw new Exception(message);
                            }

                            int hostWordRunIndexEnd = hostLanguageItem.WordRuns.IndexOf(hostWordRunStop);

                            if (hostWordRunIndexEnd > hostRunIndex)
                            {
                                CreateAndAddPhraseRun(
                                    studyItem,
                                    hostLanguageID,
                                    hostRunIndex,
                                    hostWordRunIndexEnd);

                                for (int i = hostRunIndex; i <= hostWordRunIndexEnd; i++)
                                    runIndexes.Add(i);
                            }
                            else
                                runIndexes.Add(hostRunIndex);

#else
                            int nbsCount = TextUtilities.CountChars(hostWord, LanguageLookup.NonBreakSpace);

                            if (nbsCount != 0)
                            {
                                int runIndexStart = hostRunIndex;
                                int runIndexStop = runIndexStart + nbsCount;

                                CreateAndAddPhraseRun(
                                    studyItem,
                                    hostLanguageID,
                                    runIndexStart,
                                    runIndexStop);

                                for (int i = 0; i < nbsCount; i++)
                                    runIndexes.Add(++hostRunIndex);
                            }
                            else
                                runIndexes.Add(hostRunIndex);
#endif
                        }
                    }
                }

                if (isWord)
                {
                    if (targetWordRunStartIndex >= targetLanguageItem.WordRuns.Count())
                        continue;

                    WordMapping wordMapping = new WordMapping(hostLanguageID, runIndexes.ToArray());
                    wordMappings.Add(wordMapping);
#if true
                    int targetRunIndex = targetWordRunStartIndex;
                    int targetTextIndexStart = targetLanguageItem.WordRuns[targetRunIndex].Start;
                    int targetTextIndexStop = targetTextIndexStart + targetWord.Length;
                    TextRun targetWordRunStop = targetLanguageItem.WordRuns.FirstOrDefault(x => x.Stop == targetTextIndexStop);

                    if (targetWordRunStop == null)
                    {
                        string message =
                            "Couldn't find word run stop for target word \""
                            + targetWord
                            + "\""
                            + "\nrun index "
                            + targetRunIndex.ToString()
                            + " text start index "
                            + targetTextIndexStart.ToString()
                            + " text stop index "
                            + targetTextIndexStop.ToString()
                            + "\nin: "
                            + studyItem.GetNamePathStringInclusive(LanguageLookup.English)
                            + "\nTarget runs: "
                            + targetLanguageItem.GetPhrasesAndwordRunsWithIndexesDumpString()
                            + "\nHost runs:   "
                            + hostLanguageItem.GetPhrasesAndwordRunsWithIndexesDumpString();
                        ApplicationData.Global.PutConsoleMessage(message);
                        ValidateWordNumberToRuns(
                            hostParts,
                            wordNumberToRunIndexMap,
                            hostLanguageItem,
                            hostWordRunStartIndex);
                        //throw new Exception(message);
                    }

                    int targetWordRunIndexEnd = targetLanguageItem.WordRuns.IndexOf(targetWordRunStop);

                    if (targetWordRunIndexEnd > targetRunIndex)
                    {
                        CreateAndAddPhraseRun(
                            studyItem,
                            targetLanguageID,
                            targetRunIndex,
                            targetWordRunIndexEnd);

                        // One was already put in above, so we just put in n - 1.
                        for (int i = targetRunIndex; i < targetWordRunIndexEnd; i++)
                            wordMappings.Add(wordMapping);

                        targetWordRunStartIndex += targetWordRunIndexEnd - targetRunIndex;
                    }
#else
                    int nbsCount = TextUtilities.CountChars(targetWord, LanguageLookup.NonBreakSpace);

                    if (nbsCount != 0)
                    {
                        int runIndexStart = targetWordRunStartIndex;
                        int runIndexStop = runIndexStart + nbsCount;

                        CreateAndAddPhraseRun(
                            studyItem,
                            targetLanguageID,
                            runIndexStart,
                            runIndexStop);

                        for (int i = 0; i < nbsCount; i++)
                            wordMappings.Add(wordMapping);

                        targetWordRunStartIndex += nbsCount;
                    }
#endif

                    targetWordRunStartIndex++;
                }
            }

            return wordMappings;
        }

        protected bool ValidateWordNumberToRuns(
            string[] hostParts,
            List<int> wordNumberToRunIndexMap,
            LanguageItem hostLanguageItem,
            int hostWordRunStartIndex)
        {
            int hpCount = hostParts.Length;
            int hpIndex;
            bool returnValue = true;

            if (hostParts.Count() != wordNumberToRunIndexMap.Count())
            {
                ApplicationData.Global.PutConsoleMessage("hostParts and wordNumberToRunIndexMap counts don't match.");
                return false;
            }

            for (hpIndex = 0; hpIndex < hpCount; hpIndex++)
            {
                string hp = hostParts[hpIndex];
                int hpl = hp.Length;
                int ri = wordNumberToRunIndexMap[hpIndex];
                if (ri != -1)
                {
                    int sri = ri + hostWordRunStartIndex;
                    TextRun wr = hostLanguageItem.GetWordRun(sri);
                    string wrs = hostLanguageItem.Text.Substring(wr.Start, hpl);
                    string wrsl = wrs.ToLower();
                    string hps = hp.Replace(LanguageLookup.NonBreakSpace, ' ');
                    if (hps != wrsl)
                    {
                        string message =
                            "hostParts and wordNumberToRunIndexMap mismatch at index "
                            + hpIndex.ToString()
                            + " wrsl = "
                            + wrsl
                            + " hps = "
                            + hps;
                        ApplicationData.Global.PutConsoleMessage(message);
                        returnValue = false;
                    }
                }
            }

            return returnValue;
        }

        protected void CreateAndAddPhraseRun(
            MultiLanguageItem studyItem,
            LanguageID rootLanguageID,
            int runIndexStart,
            int runIndexStop)
        {
            List<LanguageID> languageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(rootLanguageID);
            LanguageItem rootLanguageItem = studyItem.LanguageItem(rootLanguageID);
            int wordRunCount = -1;

            if (rootLanguageItem != null)
                wordRunCount = rootLanguageItem.WordRunCount();

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = studyItem.LanguageItem(languageID);

                if (languageItem != null)
                {
                    if (runIndexStop > languageItem.WordRunCount())
                        throw new Exception("Stop word run index (" + runIndexStop.ToString() + ") is greater than the number of word runs in language item: " + languageItem.Text);

                    TextRun startWordRun = languageItem.GetWordRun(runIndexStart);
                    TextRun stopWordRun = (runIndexStop < languageItem.WordRunCount() ?
                        languageItem.GetWordRun(runIndexStop) : null);

                    if (languageItem.WordRunCount() != wordRunCount)
                        continue;

                    if (startWordRun == null)
                        throw new Exception("Missing start word run for index " + runIndexStart.ToString() + " in language item: " + languageItem.Text);

                    int textStartIndex = startWordRun.Start;
                    int textStopIndex;

                    if (stopWordRun == null)
                        textStopIndex = languageItem.TextLength;
                    else
                        textStopIndex = stopWordRun.Stop;

                    if (!languageItem.HasOverlappingPhraseRun(textStartIndex, textStopIndex))
                    {
                        int textLength = textStopIndex - textStartIndex;

                        TextRun phraseRun = languageItem.FindPhraseRun(
                            textStartIndex,
                            textStopIndex);

                        if (phraseRun == null)
                        {
                            ApplicationData.Global.PutConsoleMessage(
                                "CreateAndAddPhraseRun: Phrase run mismatch.");
#if false
                            // Done in content now.
                            phraseRun = new TextRun(
                                textStartIndex,
                                textLength,
                                null);
                            languageItem.AddPhraseRun(phraseRun);
#endif
                        }
                    }
                }
            }
        }

        public void FixupBlanks(MultiLanguageItem studyItem)
        {
            foreach (LanguageID targetLanguageID in TargetLanguageIDs)
            {
                foreach (LanguageID hostLanguageID in HostLanguageIDs)
                {
                    FixupSingleBlanks(
                        studyItem,
                        targetLanguageID,
                        hostLanguageID);
                    FixupSingleBlanks(
                        studyItem,
                        hostLanguageID,
                        targetLanguageID);
                }
            }
        }

        // This function will look for a target and a host to target mapping is empty,
        // but the target to host mapping is not.  If found, if an adjacent mapping contains
        // the host to target mapping index, it will move it to the target to host mapping.
        public void FixupSingleBlanks(
            MultiLanguageItem studyItem,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID)
        {
#if false
            string namePath = studyItem.GetNamePathStringInclusive(UILanguageID);

            if (namePath == "Spanish Book of Mormon/1 Nephi/Chapter 3/Text/p7")
            {
                DumpString(namePath);
            }
#endif

            LanguageItem targetLanguageItem = studyItem.LanguageItem(targetLanguageID);
            LanguageItem hostLanguageItem = studyItem.LanguageItem(hostLanguageID);

            if ((targetLanguageItem == null) || (hostLanguageItem == null))
                return;

            int targetWordCount = targetLanguageItem.WordRunCount();
            int targetWordIndex;
            int hostWordCount = hostLanguageItem.WordRunCount();

            for (targetWordIndex = 0; targetWordIndex < targetWordCount; targetWordIndex++)
            {
                TextRun targetWordRun = targetLanguageItem.GetWordRun(targetWordIndex);

                if (targetWordRun == null)
                    continue;

                WordMapping targetWordMapping = targetWordRun.GetWordMapping(hostLanguageID);

                if ((targetWordMapping != null) && targetWordMapping.HasWordIndexes())
                {
                    int targetWordMappingCount = targetWordMapping.WordIndexCount();
                    int targetWordMappingIndex;

                    for (targetWordMappingIndex = targetWordMappingCount - 1; targetWordMappingIndex >= 0; targetWordMappingIndex--)
                    {
                        int hostWordIndex = targetWordMapping.GetWordIndex(targetWordMappingIndex);
                        TextRun hostWordRun = hostLanguageItem.GetWordRun(hostWordIndex);

                        if (hostWordRun == null)
                            continue;

                        WordMapping hostWordMapping = hostWordRun.GetWordMapping(hostLanguageID);

                        if ((hostWordMapping == null) || !hostWordMapping.HasWordIndexes())
                        {
                            int hostSentenceIndex = hostLanguageItem.GetSentenceIndexContaining(hostWordRun.Start);

                            if (hostSentenceIndex == -1)
                                continue;

                            TextRun hostSentenceRun = hostLanguageItem.GetSentenceRun(hostSentenceIndex);
                            int hostWordIndexBefore = hostWordIndex - 1;
                            bool wordDone = false;

                            if (hostWordIndexBefore < 0)
                                continue;

                            TextRun hostWordRunBefore = hostLanguageItem.GetWordRun(hostWordIndexBefore);

                            if ((hostWordRunBefore != null) && hostSentenceRun.Contains(hostWordRunBefore.Start))
                            {
                                WordMapping hostWordMappingBefore = hostWordRunBefore.GetWordMapping(targetLanguageID);

                                if ((hostWordMappingBefore != null) && (hostWordMappingBefore.WordIndexCount() > 1))
                                {
                                    int hostWordMappingBeforeIndex = hostWordMappingBefore.GetWordIndexIndex(targetWordIndex);

                                    if (hostWordMappingBeforeIndex != -1)
                                    {
                                        hostWordMappingBefore.DeleteWordIndexIndexed(hostWordMappingBeforeIndex);

                                        if (hostWordMapping == null)
                                        {
                                            hostWordMapping = new WordMapping(
                                                targetLanguageID,
                                                new int[1] { targetWordIndex });
                                            hostWordRun.SetWordMapping(hostWordMapping);
                                        }
                                        else
                                            hostWordMapping.AddWordIndex(targetWordIndex);

                                        wordDone = true;
                                    }
                                }
                            }

                            if (!wordDone)
                            {
                                int hostWordIndexAfter = hostWordIndex + 1;

                                if (hostWordIndexAfter < hostWordCount)
                                {
                                    TextRun hostWordRunAfter = hostLanguageItem.GetWordRun(hostWordIndexAfter);

                                    if ((hostWordRunAfter != null) && hostSentenceRun.Contains(hostWordRunAfter.Start))
                                    {
                                        WordMapping hostWordMappingAfter = hostWordRunAfter.GetWordMapping(targetLanguageID);

                                        if ((hostWordMappingAfter != null) && (hostWordMappingAfter.WordIndexCount() > 1))
                                        {
                                            int hostWordMappingAfterIndex = hostWordMappingAfter.GetWordIndexIndex(targetWordIndex);

                                            if (hostWordMappingAfterIndex != -1)
                                            {
                                                hostWordMappingAfter.DeleteWordIndexIndexed(hostWordMappingAfterIndex);

                                                if (hostWordMapping == null)
                                                {
                                                    hostWordMapping = new WordMapping(
                                                        targetLanguageID,
                                                        new int[1] { targetWordIndex });
                                                    hostWordRun.SetWordMapping(hostWordMapping);
                                                }
                                                else
                                                    hostWordMapping.AddWordIndex(targetWordIndex);

                                                wordDone = true;
                                            }
                                        }
                                    }
                                }
                            }
                            if (wordDone)
                                studyItem.Modified = true;
                        }
                    }
                }
            }
        }

        protected void CheckForPossiblyMismatchedSentences(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs,
            List<WordMapping> wordMappings,
            int sentenceIndex)
        {
            int alignCount = wordMappings.Count();
            int alignIndex;

            int bestMultipleCount = 0;
            int bestEmptyCount = 0;
            int emptyCount = 0;

            for (alignIndex = 0; alignIndex < alignCount; alignIndex++)
            {
                WordMapping slot = wordMappings[alignIndex];

                int count = slot.WordIndexCount();

                if (count > bestMultipleCount)
                    bestMultipleCount = count;

                if (count == 0)
                {
                    emptyCount++;

                    if (emptyCount > bestEmptyCount)
                        bestEmptyCount = emptyCount;
                }
                else
                    emptyCount = 0;
            }

            string message = String.Empty;

            //if (bestMultipleCount > 8)
            //    message += "Possible sentence mismatch: multiple count = " + bestMultipleCount.ToString();

            if (bestEmptyCount >= 8)
                message += "Possible sentence mismatch: empty count = " + bestEmptyCount.ToString();

            if (!String.IsNullOrEmpty(message))
            {
                PutMessageAlways(message);
                PutMessageAlways("Paragraph path: " + studyItem.GetNamePathStringWithOrdinalInclusive(UILanguageID));
                PutMessageAlways("Sentences:");
                PutMessageAlways("");

                foreach (LanguageID languageID in languageIDs)
                {
                    LanguageItem languageItem = studyItem.LanguageItem(languageID);
                    TextRun sentenceRun = languageItem.GetSentenceRun(sentenceIndex);
                    string sentence = languageItem.GetRunText(sentenceRun);
                    PutMessageAlways(sentence);
                }

                PutMessageAlways("");
            }
        }

        protected void OutputDisplayAlignment(
            StreamWriter writer,
            MultiLanguageItem studyItem,
            int sentenceIndex,
            string namePath)
        {
            NodeUtilities.OutputDisplayAlignment(
                writer,
                studyItem,
                sentenceIndex,
                namePath,
                TargetLanguageIDs,
                HostLanguageIDs,
                IsDisplayDictionary,
                false,                          // isUseDictionaryNoDeinflection
                true,                           // isUseDictionaryWithDeinflection
                false,                          // isUseAlignmentDictionary
                false,                          // isUseWordDictionary
                null,                           // targetAlignmentDictionary
                null,                           // targetWordDictionary
                null,                           // hostAlignmentDictionary
                null);                          // hostWordDictionary
        }

        protected void DumpAlignment(
            MultiLanguageItem studyItem,
            int sentenceIndex,
            string namePath)
        {
            NodeUtilities.DumpAlignment(
                studyItem,
                sentenceIndex,
                namePath,
                TargetLanguageIDs,
                HostLanguageIDs,
                IsDisplayDictionary,
                false,                          // isUseDictionaryNoDeinflection
                true,                           // isUseDictionaryWithDeinflection
                false,                          // isUseAlignmentDictionary
                false,                          // isUseWordDictionary
                null,                           // targetAlignmentDictionary
                null,                           // targetWordDictionary
                null,                           // hostAlignmentDictionary
                null);                          // hostWordDictionary
        }

        protected virtual bool InputTextLines(string inputFilePath, List<string> list, string label)
        {
            if (String.IsNullOrEmpty(inputFilePath))
                return true;

            //if (Verbose)
            //    PutConsoleMessage("Reading " + label + " file from " + inputFilePath + "...");

            Stream stream = OpenReadStream(inputFilePath);

            if (stream == null)
                return false;

            try
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                        list.Add(line);
                }
            }
            catch (Exception exc)
            {
                string msg = "Error during read of " + label + " file: " + exc.Message;

                if (exc.InnerException != null)
                    msg += ": " + exc.InnerException.Message;

                //PutConsoleErrorMessage(msg);
                Error = msg;
            }
            finally
            {
                CloseStream(stream);
            }

            return true;
        }

        public static new bool IsSupportedStatic(string importExport, string contentName, string capability)
        {
            switch (contentName)
            {
                case "BaseObjectNodeTree":
                case "BaseObjectNode":
                case "BaseObjectContent":
                case "ContentStudyList":
                    if (capability == "Support")
                        return true;
                    else if (importExport == "Import")
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

        public static new string TypeStringStatic { get { return "GizaAlignment"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
