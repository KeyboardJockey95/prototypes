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
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatGizaInput : FormatPatterned
    {
        // Argument data.
        public string ContentKey { get; set; }
        public LanguageID LanguageID { get; set; }

        public override LanguageID TargetLanguageID { get; set; }
        protected static string TargetLanguageIDPrompt = "Target language code";
        protected static string TargetLanguageIDHelp = "Enter the target language code.";

        public override LanguageID HostLanguageID { get; set; }
        public static string HostLanguageIDPrompt = "Host language code";
        public static string HostLanguageIDHelp = "Enter the host language code.";

        //public bool IsParagraphsOnly = true;
        protected static string IsParagraphsOnlyPrompt = "Paragraphs only";
        //protected static string IsParagraphsOnlyHelp = "Set this to true to parse paragraphs only, not sentences";

        public bool IsOmitPunctuation { get; set; }
        protected static string IsOmitPunctuationPrompt = "Omit puntuation";
        protected static string IsOmitPunctuationHelp = "Set this to omit punctuation.";

        public bool IsDumpStatistics { get; set; }
        protected static string IsDumpStatisticsPrompt = "Dump statistics";
        protected static string IsDumpStatisticsHelp = "Set this to true to display statistics.";

        // Statistics.
        protected string LanguageName;
        protected int ParagraphCount;
        protected int SentenceCount;
        protected int SentenceMismatchCount;
        protected int TokenCount;
        protected int WordCount;
        protected int UniqueWordCount;
        protected int AverageSentenceWordCount;
        protected List<int> SentenceWordCountHistogram;
        protected int PunctuationCount;
        protected Dictionary<string, int> UniqueWords;

        private static string FormatDescription = "GIZA++ input export format.";

        public FormatGizaInput()
            : base("Line", "GizaInput", "FormatGizaInput", FormatDescription, String.Empty, String.Empty,
                  "text/plain", ".txt", null, null, null, null, null)
        {
            ClearFormatGizaInput();
        }

        public FormatGizaInput(FormatPatterned other)
            : base(other)
        {
            if (other is FormatGizaInput)
                CopyFormatGizaInput(other as FormatGizaInput);
        }

        // For derived classes.
        public FormatGizaInput(string name, string type, string description, string targetType, string importExportType,
                string mimeType, string defaultExtension,
                UserRecord userRecord, UserProfile userProfile, IMainRepository repositories, LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base("Line", name, type, description, targetType, importExportType, mimeType, defaultExtension,
                  userRecord, userProfile, repositories, languageUtilities, nodeUtilities)
        {
            ClearFormatGizaInput();
        }

        public void ClearFormatGizaInput()
        {
            UseComments = false;
            ContentKey = "Text";
            IsParagraphsOnly = true;
            IsOmitPunctuation = false;
            IsDumpStatistics = false;
            LanguageName = String.Empty;
            ParagraphCount = 0;
            SentenceCount = 0;
            SentenceMismatchCount = 0;
            TokenCount = 0;
            WordCount = 0;
            UniqueWordCount = 0;
            AverageSentenceWordCount = 0;
            SentenceWordCountHistogram = null;
            PunctuationCount = 0;
            UniqueWords = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        public void CopyFormatGizaInput(FormatGizaInput other)
        {
            UseComments = false;
            ContentKey = other.ContentKey;
            IsParagraphsOnly = other.IsParagraphsOnly;
            IsOmitPunctuation = other.IsOmitPunctuation;
            IsDumpStatistics = other.IsDumpStatistics;
            LanguageName = String.Empty;
            ParagraphCount = 0;
            SentenceCount = 0;
            SentenceMismatchCount = 0;
            TokenCount = 0;
            WordCount = 0;
            UniqueWordCount = 0;
            AverageSentenceWordCount = 0;
            SentenceWordCountHistogram = null;
            PunctuationCount = 0;
            UniqueWords = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        public override Format Clone()
        {
            return new FormatGizaInput(this);
        }

        public static string ContentKeyHelp = "Enter the content key.";
        public static string LanguageIDHelp = "Select the language.";

        public override void LoadFromArguments()
        {
            DeleteBeforeImport = false;
            UseComments = false;
            CommentPrefix = null;
            Pattern = "%{t}";
            RowCount = 1;

            ContentKey = GetArgumentDefaulted("ContentKey", "string", "r", ContentKey,
                "Content key", ContentKeyHelp);

            TargetLanguageID = GetLanguageIDArgumentDefaulted("TargetLanguageID", "languageID", "r", TargetLanguageID,
                TargetLanguageIDPrompt, TargetLanguageIDHelp);

            HostLanguageID = GetLanguageIDArgumentDefaulted("HostLanguageID", "languageID", "r", HostLanguageID,
                HostLanguageIDPrompt, HostLanguageIDHelp);

            IsParagraphsOnly = GetFlagArgumentDefaulted("IsParagraphsOnly", "flag", "w", IsParagraphsOnly,
                IsParagraphsOnlyPrompt, IsParagraphsOnlyHelp, null, null);

            IsOmitPunctuation = GetFlagArgumentDefaulted("IsOmitPunctuation", "flag", "w", IsOmitPunctuation,
                IsOmitPunctuationPrompt, IsOmitPunctuationHelp, null, null);

            IsDumpStatistics = GetFlagArgumentDefaulted("IsDumpStatistics", "flag", "w", IsDumpStatistics,
                IsDumpStatisticsPrompt, IsDumpStatisticsHelp, null, null);

            SetupGizaLanguages();
        }

        public override void SaveToArguments()
        {
            DeleteBeforeImport = false;
            UseComments = false;
            CommentPrefix = null;
            Pattern = "%{t}";
            RowCount = 1;

            SetArgument("ContentKey", "string", "r", ContentKey,
                "Content key", ContentKeyHelp, null, null);

            SetLanguageIDArgument("TargetLanguageID", "languageID", "r", TargetLanguageID, TargetLanguageIDPrompt,
                TargetLanguageIDHelp);

            SetLanguageIDArgument("HostLanguageID", "languageID", "r", HostLanguageID, HostLanguageIDPrompt,
                HostLanguageIDHelp);

            SetLanguageIDArgument("TargetLanguageID", "languageID", "r", TargetLanguageID,
                TargetLanguageIDPrompt, TargetLanguageIDHelp);

            SetLanguageIDArgument("HostLanguageID", "languageID", "r", HostLanguageID,
                HostLanguageIDPrompt, HostLanguageIDHelp);

            SetFlagArgument("IsParagraphsOnly", "flag", "w", IsParagraphsOnly,
                IsParagraphsOnlyPrompt, IsParagraphsOnlyHelp, null, null);

            SetFlagArgument("IsOmitPunctuation", "flag", "w", IsOmitPunctuation,
                IsOmitPunctuationPrompt, IsOmitPunctuationHelp, null, null);

            SetFlagArgument("IsDumpStatistics", "flag", "w", IsDumpStatistics,
                IsDumpStatisticsPrompt, IsDumpStatisticsHelp, null, null);
        }

        public void SetupGizaLanguages()
        {
            TargetLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(TargetLanguageID);
            HostLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(HostLanguageID);
            UniqueLanguageIDs = LanguageID.ConcatenateUnqueList(HostLanguageIDs, TargetLanguageIDs);
            LanguageDescriptors = LanguageDescriptor.GetTargetAndHostLanguageDescriptorsFromLanguageIDs(
                TargetLanguageIDs,
                HostLanguageIDs);

            if (UserProfile != null)
                UILanguageID = UserProfile.UILanguageID;
            else
                UILanguageID = LanguageLookup.English;

            LanguageName = TargetLanguageID.LanguageName(UILanguageID);
        }

        public override void Write(Stream stream)
        {
            ParagraphCount = 0;
            SentenceCount = 0;
            SentenceMismatchCount = 0;
            TokenCount = 0;
            WordCount = 0;
            AverageSentenceWordCount = 0;
            SentenceWordCountHistogram = new List<int>();
            PunctuationCount = 0;

            base.Write(stream);

            AverageSentenceWordCount = WordCount / SentenceCount;

            if (IsDumpStatistics)
                DumpStatistics();
        }

        protected override string FormatString(
            string pattern,
            MultiLanguageItem multiLanguageItem,
            List<object> arguments = null)
        {
            string returnValue;

            if (multiLanguageItem == null)
                throw new Exception("Null study item.");

            //string path = multiLanguageItem.GetNamePathStringInclusive(LanguageLookup.English, "/");

            //if (path == "Book of Mormon/Introduction and Witnesses/The Testimony of the Prophet Joseph Smith/Text/p2")
            //    DumpString("path = " + path);

            multiLanguageItem.SentenceAndWordRunCheck(Repositories.Dictionary);

            if (TargetLanguageID == null)
                throw new Exception("No language set for Giza input format.");

            LanguageItem targetLanguageItem = multiLanguageItem.LanguageItem(TargetLanguageID);
            LanguageItem hostLanguageItem = multiLanguageItem.LanguageItem(HostLanguageID);

            bool doParagraphs = IsParagraphsOnly;
            bool mismatchedSentences = true;
            int targetSentenceCount = 0;
            int hostSentenceCount = 0;

            // If the sentence counts are not the same, do paragraphs.
            if ((hostLanguageItem != null) && (targetLanguageItem != null))
            {
                targetSentenceCount = targetLanguageItem.SentenceRunCount();
                hostSentenceCount = hostLanguageItem.SentenceRunCount();
                mismatchedSentences = targetSentenceCount != hostSentenceCount;

                if (!hostLanguageItem.HasText() || mismatchedSentences)
                    doParagraphs = true;
            }
            else
                doParagraphs = true;

            if (targetLanguageItem != null)
            {
                if (doParagraphs)
                    returnValue = FormatStringParagraphs(targetLanguageItem);
                else
                    returnValue = FormatStringSentences(targetLanguageItem);
            }
            else
                returnValue = String.Empty;

            string[] sentences = returnValue.Split(LanguageLookup.NewLine);
            int lineCount = sentences.Length;
            int tokenCount = TextUtilities.CountChars(returnValue, ' ');

            if (!returnValue.EndsWith(" "))
                tokenCount++;

            int wordCount = (targetLanguageItem != null ? targetLanguageItem.WordRunCount() : 0);
            int punctuationCount = TextUtilities.CountPunctuation(
                targetLanguageItem != null ? targetLanguageItem.Text : String.Empty);

            ParagraphCount++;
            SentenceCount += targetSentenceCount;

            if (mismatchedSentences)
                SentenceMismatchCount++;

            TokenCount += tokenCount;
            WordCount += wordCount;
            PunctuationCount += punctuationCount;

            foreach (string sentence in sentences)
            {
                int sentenceWordCount = TextUtilities.CountChars(returnValue, ' ');
                int sentencePunctuationCount = TextUtilities.CountPunctuation(sentence);

                if (!sentence.EndsWith(" "))
                    sentenceWordCount++;

                sentenceWordCount -= sentencePunctuationCount;

                if (SentenceWordCountHistogram.Count() <= sentenceWordCount)
                {
                    int i = SentenceWordCountHistogram.Count();

                    for (; i <= sentenceWordCount; i++)
                        SentenceWordCountHistogram.Add(0);
                }

                SentenceWordCountHistogram[sentenceWordCount]++;
            }

            return returnValue;
        }

        protected string FormatStringSentences(LanguageItem languageItem)
        {
            string text = languageItem.TextLower;
            List<TextRun> sentenceRuns = languageItem.SentenceRuns;
            List<TextRun> wordRuns;
            int lastIndex = 0;
            int startIndex;
            int stopIndex;
            int length;
            StringBuilder sb = new StringBuilder();
            bool first = true;

            if ((sentenceRuns == null) || (sentenceRuns.Count() == 0))
                return String.Empty;

            foreach (TextRun sentenceRun in sentenceRuns)
            {
                if (first)
                    first = false;
                else
                    sb.AppendLine(String.Empty);

                wordRuns = languageItem.GetSentenceWordRuns(sentenceRun);

                if ((wordRuns == null) || (wordRuns.Count() == 0))
                    continue;

                foreach (TextRun wordRun in wordRuns)
                {
                    startIndex = wordRun.Start;
                    stopIndex = wordRun.Stop;
                    length = wordRun.Length;

                    if (lastIndex < startIndex)
                    {
                        string between = text.Substring(lastIndex, startIndex - lastIndex).Trim();

                        if (!String.IsNullOrEmpty(between))
                        {
                            if (IsOmitPunctuation)
                                sb.Append(" ");
                            else
                            {
                                foreach (char chr in between)
                                {
                                    if (!Char.IsWhiteSpace(chr))
                                    {
                                        if (sb.Length != 0)
                                            sb.Append(" ");

                                        sb.Append(chr);
                                    }
                                }
                            }
                        }
                    }

                    string word = text.Substring(startIndex, length);

                    if (sb.Length != 0)
                        sb.Append(" ");

                    sb.Append(word);

                    int wordCount = 0;

                    if (UniqueWords.TryGetValue(word, out wordCount))
                        UniqueWords[word] = wordCount + 1;
                    else
                    {
                        UniqueWords.Add(word, 1);
                        UniqueWordCount++;
                    }

                    lastIndex = stopIndex;
                }

                if ((lastIndex < text.Length) && !IsOmitPunctuation)
                {
                    string between = text.Substring(lastIndex, text.Length - lastIndex).Trim();

                    if (!String.IsNullOrEmpty(between))
                    {
                        foreach (char chr in between)
                        {
                            if (!Char.IsWhiteSpace(chr))
                            {
                                if (sb.Length != 0)
                                    sb.Append(" ");

                                sb.Append(chr);
                            }
                        }
                    }
                }
            }

            return sb.ToString();
        }

        protected string FormatStringParagraphs(LanguageItem languageItem)
        {
            string text = languageItem.TextLower;
            List<TextRun> wordRuns = languageItem.WordRuns;
            int lastIndex = 0;
            int startIndex;
            int stopIndex;
            int length;
            StringBuilder sb = new StringBuilder();

            if ((wordRuns == null) || (wordRuns.Count() == 0))
                return String.Empty;

            foreach (TextRun wordRun in wordRuns)
            {
                startIndex = wordRun.Start;
                stopIndex = wordRun.Stop;
                length = wordRun.Length;

                if (lastIndex < startIndex)
                {
                    string between = text.Substring(lastIndex, startIndex - lastIndex).Trim();

                    if (!String.IsNullOrEmpty(between))
                    {
                        foreach (char chr in between)
                        {
                            if (!Char.IsWhiteSpace(chr))
                            {
                                if (sb.Length != 0)
                                    sb.Append(" ");

                                sb.Append(chr);
                            }
                        }
                    }
                }

                string word = text.Substring(startIndex, length);

                if (sb.Length != 0)
                    sb.Append(" ");

                sb.Append(word);

                int wordCount = 0;

                if (UniqueWords.TryGetValue(word, out wordCount))
                    UniqueWords[word] = wordCount + 1;
                else
                {
                    UniqueWords.Add(word, 1);
                    UniqueWordCount++;
                }

                lastIndex = stopIndex;
            }

            if (lastIndex < text.Length)
            {
                string between = text.Substring(lastIndex, text.Length - lastIndex).Trim();

                if (!String.IsNullOrEmpty(between))
                {
                    foreach (char chr in between)
                    {
                        if (!Char.IsWhiteSpace(chr))
                        {
                            if (sb.Length != 0)
                                sb.Append(" ");

                            sb.Append(chr);
                        }
                    }
                }
            }

            return sb.ToString();
        }

        public override void DumpStatistics()
        {
            DumpString("Stats for " + Name + " for " + LanguageName + ":");
            DumpString("    ParagraphCount = " + ParagraphCount.ToString());
            DumpString("    SentenceCount = " + SentenceCount.ToString());
            DumpString("    SentenceMismatchCount = " + SentenceMismatchCount.ToString());
            DumpString("    WordCount = " + WordCount.ToString());
            DumpString("    UniqueWordCount = " + UniqueWordCount.ToString());
            DumpString("    PunctuationCount = " + PunctuationCount.ToString());
            DumpString("    AverageSentenceWordCount = " + AverageSentenceWordCount.ToString());
            DumpString("    SentenceWordCountHistogram:");

            DumpHistogram();
        }

        public void DumpHistogram()
        {
            int length = SentenceWordCountHistogram.Count();
            int index;
            int maxCount = 0;
            int height;
            string heightString;
            StringBuilder sb = new StringBuilder();

            for (index = 0; index < length; index++)
            {
                if (SentenceWordCountHistogram[index] > maxCount)
                    maxCount = SentenceWordCountHistogram[index];
            }

            string lengthString = length.ToString();
            int lengthStringCount = lengthString.Length;

            for (height = maxCount; height != 0; height--)
            {
                sb.Append(String.Format("        {0:d4} ", height));

                for (index = 0; index < length; index++)
                {
                    if (SentenceWordCountHistogram[index] >= height)
                        sb.Append("*");
                    else
                        sb.Append(" ");
                }

                DumpString(sb.ToString());
                sb.Clear();
            }

            sb.Clear();

            for (int digit = 0; digit < lengthStringCount; digit++)
            {
                sb.Append("             ");

                for (index = 0; index < length; index++)
                {
                    height = SentenceWordCountHistogram[index];
                    heightString = height.ToString();

                    if (digit < heightString.Length)
                        sb.Append(heightString[digit]);
                    else
                        sb.Append(" ");
                }

                DumpString(sb.ToString());
                sb.Clear();
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
                    if (capability == "Support")
                        return true;
                    else if ((importExport == "Export") && (capability == "UseFlags"))
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

        public static new string TypeStringStatic { get { return "GizaInput"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
