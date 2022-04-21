using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Content
{
    public enum ContentEditCommand
    {
        None,
        Display,
        SelectAll,
        SelectNone,
        SelectBetween,
        SelectPrevious,
        Save,
        Update,
        Append,
        Insert,
        Cancel,
        Delete,
        DeleteAll,
        EditAnnotations,
        DeleteAnnotations,
        EditAnnotation,
        AddAnnotation,
        InsertAnnotation,
        DeleteAnnotation,
        SaveAnnotation,
        TranslateAnnotation,
        CancelAnnotation,
        GenerateMedia,
        DeleteMedia,
        DeleteMediaRun,
        DeleteLanguageMedia,
        ExtractLanguageMedia,
        PropagateMap,
        EditMediaRun,
        AddMediaRun,
        InsertMediaRun,
        UploadMediaRun,
        SaveMediaRun,
        CancelMediaRun,
        MoveUpMediaRun,
        MoveDownMediaRun,
        SaveAddMedia,
        Join,
        JoinPhrases,
        Split,
        Fix,
        EditSentenceRuns,
        SplitOrJoinSentenceRuns,
        AutoResetSentenceRuns,
        JoinAllSentenceRuns,
        EditWordRuns,
        SplitOrJoinWordRuns,
        AutoResetWordRuns,
        JoinAllWordRuns,
        EditAlignment,
        SaveAlignment,
        MakePhrase,
        ClearLanguage,
        AddTranslations,
        Copy,
        Cut,
        Paste,
        ClearSandbox,
        ShowSandbox,
        HideSandbox,
        SetVoiceLanguage,
        AddSynthesizedVoice,
        SaveMarkupText,
        CopyMarkup,
        SetMediaLanguage,
        CollectStrings,
        SetSubtitleLanguage,
        Map,
        AutoMap,
        ClearMap,
        DeleteLocalStudyItems,
        Score,
        DefinitionAction,
        SetStudyItems,
        SetStage,
        SetNextReviewTime,
        Recreate,
        Reset,
        ResetAll
    }

    public enum ContentEditType
    {
        Display,
        Append,
        Edit,
        EditLanguage,
        InsertBefore,
        InsertAfter,
        RecordAudio,
        MapAudio,
        AddMedia,
        EditMediaRuns,
        EditAnnotations,
        EditSentenceRuns,
        EditWordRuns,
        EditAlignment
    }

    public enum CopyPasteType
    {
        // Operate on selection.
        Before,
        Replace,
        After,
        Under,
        // Operate on all.
        Prepend,
        All,
        Append
    }

    public enum EditMediaOperation
    {
        None,
        Cut,
        Copy,
        Paste,
        Clear,
        Delete,
        Crop,
        Undo,
        Redo,
        Deselect,
        Save,
        Cancel,
        Import,
        Export,
        UpdateAudio,
        DeleteMedia,
        Record,
        PostRecorded,
        RecordShutDown
    }

    // Select which sentence-parsing algorithm to use.
    public enum SentenceParsingAlgorithm
    {
        // Try to handle parseing, including start/stop quoting/delimiting, in a language-independent way. Individual language items.
        Context,
        // Just use sentence terminator characters as delimiters. Ignores start/stop quoting/delimiting. Individual language items.
        RawPunctuation,
        // Try to match punctuation. Ignores start/stop quoting/delimiting. Must be full study item.
        MatchPunctuation,
        // Splits sentences based on best ratios. Ignores start/stop quoting/delimiting. Must be full study item.
        RatioWalker
    }

    // Select what to do when sentences don't match across languages.
    public enum SentenceParsingFallback
    {
        // Leave sentences as they were parsed.
        DoNothing,
        // Revert to making the whole study item a full paragraph, with one sentence run covering it.
        FullParagraph,
        // If sentence counts match, but some ratios failed, collapse sentences with failed ratios.
        // Otherwise if mismatched, collapse to paragraph.
        CollapseFailedRatios
    }

    public static class ContentUtilities
    {
        // Returns false if sentences mismatched, even if the fallback was activated.
        public static bool ParseSentenceRuns(
            MultiLanguageItem studyItem,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            List<LanguageID> uniqueLanguageIDs,
            SentenceParsingAlgorithm parsingAlgorithm,
            SentenceParsingFallback fallback)
        {
            bool returnValue = true;

            if ((uniqueLanguageIDs == null) || (uniqueLanguageIDs.Count() == 0))
                uniqueLanguageIDs = studyItem.LanguageIDs;

            switch (parsingAlgorithm)
            {
                case SentenceParsingAlgorithm.Context:
                    ParseSentenceRunsContextPunctuation(studyItem, uniqueLanguageIDs);
                    break;
                case SentenceParsingAlgorithm.RawPunctuation:
                    ParseSentenceRunsRawPunctuation(studyItem, uniqueLanguageIDs);
                    break;
                case SentenceParsingAlgorithm.MatchPunctuation:
                    ParseSentenceRunsMatchPunctuation(studyItem, uniqueLanguageIDs);  // Never mismatched.
                    break;
                case SentenceParsingAlgorithm.RatioWalker:
                    ParseSentenceRunsRatioWalker(studyItem, targetLanguageIDs, hostLanguageIDs, uniqueLanguageIDs);
                    break;
                default:
                    throw new Exception("ContentUtilities.ParseSentenceRuns: Unexpected parsing algorithm: " + parsingAlgorithm);
            }

            returnValue = CheckSentenceRunsAndFallback(studyItem, uniqueLanguageIDs, fallback);

            return returnValue;
        }

        public static void ParseSentenceRunsContextPunctuation(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs)
        {
            if ((languageIDs == null) || (languageIDs.Count() == 0))
                languageIDs = studyItem.LanguageIDs;

            if (languageIDs.Count() == 0)
                return;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = studyItem.LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                List<TextRun> sentenceRuns = languageItem.SentenceRuns;

                ParseTextSentenceRunsContext(languageItem.Text, languageID, ref sentenceRuns);

                languageItem.SentenceRuns = sentenceRuns;
            }
        }

        public static void ParseSentenceRunsRawPunctuation(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs)
        {
            if ((languageIDs == null) || (languageIDs.Count() == 0))
                languageIDs = studyItem.LanguageIDs;

            if (languageIDs.Count() == 0)
                return;

            ParseSentenceRunsGivenPunctuation(studyItem, languageIDs, PunctuationCounter.PunctuationAll);
        }

        public static void ParseSentenceRunsMatchPunctuation(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs)
        {
            if ((languageIDs == null) || (languageIDs.Count() == 0))
                languageIDs = studyItem.LanguageIDs;

            if (languageIDs.Count() == 0)
                return;

            string[] sentenceBreakDelimiters = GetPunctuationDelimiters(studyItem, languageIDs, true);

            ParseSentenceRunsGivenPunctuation(studyItem, languageIDs, sentenceBreakDelimiters);
        }

        public static void ParseSentenceRunsGivenPunctuation(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs,
            string[] sentenceBreakDelimiters)
        {
            if ((languageIDs == null) || (languageIDs.Count() == 0))
                languageIDs = studyItem.LanguageIDs;

            if (languageIDs.Count() == 0)
                return;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = studyItem.LanguageItem(languageID);

                if (languageItem == null)
                    continue;

                List<TextRun> sentenceRuns = languageItem.SentenceRuns;

                if (sentenceRuns == null)
                    languageItem.SentenceRuns = sentenceRuns = new List<TextRun>();

                ParseTextSentenceRunsGivenPunctuation(languageItem.Text, languageID, sentenceBreakDelimiters, ref sentenceRuns);

                languageItem.SentenceRuns = sentenceRuns;
            }
        }

        // Returns false if sentences mismatched, even if the fallback was activated.
        public static bool CheckSentenceRunsAndFallback(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs,
            SentenceParsingFallback fallback)
        {
            if ((languageIDs == null) || (languageIDs.Count() == 0))
                languageIDs = studyItem.LanguageIDs;

            bool returnValue;

            switch (fallback)
            {
                case SentenceParsingFallback.DoNothing:
                    returnValue = VerifySentenceParsing(studyItem, languageIDs);
                    break;
                case SentenceParsingFallback.FullParagraph:
                    returnValue = VerifySentenceParsing(studyItem, languageIDs);
                    if (!returnValue)
                        studyItem.JoinSentenceRuns(languageIDs);
                    break;
                case SentenceParsingFallback.CollapseFailedRatios:
                    returnValue = VerifySentenceParsingAndCollapse(studyItem, languageIDs);
                    break;
                default:
                    throw new Exception("Unexpected fallback: " + fallback.ToString());
            }

            return returnValue;
        }

        public class SentenceParsingRatioLimits
        {
            public int LengthLow;
            public int LengthHigh;
            public double RatioLow;
            public double RatioHigh;

            public SentenceParsingRatioLimits(
                int lengthLow,
                int lengthHigh,
                double limitLow,
                double limitHigh)
            {
                LengthLow = lengthLow;
                LengthHigh = lengthHigh;
                RatioLow = limitLow;
                RatioHigh = limitHigh;
            }

            public bool IsInLengthLimits(int length)
            {
                return ((length >= LengthLow) && (length <= LengthHigh));
            }

            public bool IsInRatioLimits(double ratio)
            {
                return ((ratio >= RatioLow) && (ratio <= RatioHigh));
            }

            public override string ToString()
            {
                return  "L: " + LengthLow.ToString() + " - " + LengthHigh.ToString() + "  R: " + RatioLow.ToString() + " - " + RatioHigh.ToString();
            }
        }

        public class SentenceParsingRatioLimitChecker
        {
            public List<SentenceParsingRatioLimits> Limits;

            public SentenceParsingRatioLimitChecker(List<SentenceParsingRatioLimits> limits)
            {
                Limits = limits;
            }

            public SentenceParsingRatioLimits FindLimitsFromLength(int length)
            {
                foreach (SentenceParsingRatioLimits limits in Limits)
                {
                    if (limits.IsInLengthLimits(length))
                        return limits;
                }

                return null;
            }

            public static bool IsUseHigh = false;

            public SentenceParsingRatioLimits FindLimitsFromLengths(int targetLength, int hostLength)
            {
                int length = ((IsUseHigh ? targetLength > hostLength : targetLength < hostLength) ? targetLength : hostLength);
                return FindLimitsFromLength(length);
            }

            public bool IsInLimits(int length, double ratio)
            {
                SentenceParsingRatioLimits limits = FindLimitsFromLength(length);

                if (limits == null)
                    return false;

                return limits.IsInRatioLimits(ratio);
            }

            public bool AreInLimits(
                int targetLength,
                int hostLength,
                double ratio)
            {
                int length = ((IsUseHigh ? targetLength > hostLength : targetLength < hostLength) ? targetLength : hostLength);
                return IsInLimits(length, ratio);
            }
        }

        public static SentenceParsingRatioLimitChecker SentenceParsingLimitsForRatioWalker =
            new SentenceParsingRatioLimitChecker(
                new List<SentenceParsingRatioLimits>()
                {
                    new SentenceParsingRatioLimits(0,   40,             0.60,   1.0/0.60),  // 1.6666666
                    new SentenceParsingRatioLimits(41,  80,             0.72,   1.0/0.72),  // 1.3888888
                    new SentenceParsingRatioLimits(81,  int.MaxValue,   0.78,   1.0/0.78)   // 1.2820512
                }
            );

        // Returns false if failed verification.
        public static bool VerifySentenceParsing(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                languageIDs = studyItem.LanguageIDs;

            if (languageIDs == null)
                return true;

            if (studyItem.IsSentenceMismatch(languageIDs))
                return false;

            return VerifySentenceParsingRatios(studyItem, languageIDs);
        }

        // Returns false if failed verification.
        public static bool VerifySentenceParsingRatios(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                languageIDs = studyItem.LanguageIDs;

            if (languageIDs == null)
                return true;

            LanguageID targetLanguageID = languageIDs[0];
            LanguageItem targetLanguageItem = studyItem.LanguageItem(targetLanguageID);

            int sentenceCount = studyItem.GetSentenceCount(languageIDs);
            int sentenceIndex;

            for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
            {
                TextRun targetSentenceRun = (targetLanguageItem != null ? targetLanguageItem.GetSentenceRun(sentenceIndex) : null);

                if (targetSentenceRun == null)
                    continue;

                SentenceParsingInfo info = targetSentenceRun.ParsingInfo;

                if (info == null)
                    continue;

                if (info.SentenceFailed)
                    return false;
            }

            return true;
        }

        // Returns false if failed verification.
        public static bool VerifySentenceParsingAndCollapse(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs)
        {
            if ((languageIDs == null) || (languageIDs.Count() != 2))
                throw new Exception("VerifySentenceParsingAndCollapse: Need two languages.");

            if (studyItem.IsSentenceMismatch(languageIDs))
            {
                studyItem.JoinSentenceRuns(languageIDs);
                return false;
            }

            throw new Exception("VerifySentenceParsingAndCollapse: Not fully implemented.");

#if false
            LanguageID targetLanguageID = languageIDs[0];
            LanguageID hostLanguageID = languageIDs[1];
            LanguageItem targetLanguageItem = studyItem.LanguageItem(targetLanguageID);
            LanguageItem hostLanguageItem = studyItem.LanguageItem(hostLanguageID);

            int sentenceCount = studyItem.GetSentenceCount(languageIDs);
            int sentenceIndex;

            for (sentenceIndex = sentenceCount - 1; sentenceIndex >= 0; sentenceIndex--)
            {
                TextRun targetSentenceRun = (targetLanguageItem != null ? targetLanguageItem.GetSentenceRun(sentenceIndex) : null);

                if (targetSentenceRun == null)
                    continue;

                SentenceParsingInfo info = targetSentenceRun.ParsingInfo;

                if (info.SentenceFailed)
                {
                    doCollapse = true;
                    break;
                }
            }

            bool returnValue = true;

            return returnValue;
#endif
        }

#if false
        // Returns true if any sentences collapsed.
        public static bool CollapseFailedSentence(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs,
            List<double> targetHostRatioRatios,  // Array of the target/host ratios.
            List<bool> collapsedFlags)          // Array of flags showing collapsed sentences.
        {
            // Sentence counts across languages should be the same by now.
            int sentenceCount = studyItem.GetSentenceCount(languageIDs);
            int sentenceIndex;
            bool returnValue = false;

            if (sentenceCount < 2)
                return false;

            LanguageID targetLanguageID = languageIDs[0];
            LanguageID hostLanguageID = languageIDs[1];
            LanguageItem targetLanguageItem = studyItem.LanguageItem(targetLanguageID);
            LanguageItem hostLanguageItem = studyItem.LanguageItem(hostLanguageID);

            for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
            {
                TextRun targetSentenceRun = (targetLanguageItem != null ? targetLanguageItem.GetSentenceRun(sentenceIndex) : null);
                TextRun hostSentenceRun = (hostLanguageItem != null ? hostLanguageItem.GetSentenceRun(sentenceIndex) : null);
                int targetSentenceLength = (targetSentenceRun != null ? targetSentenceRun.Length : 0);
                int hostSentenceLength = (hostSentenceRun != null ? hostSentenceRun.Length : 0);
                double targetHostRatio = targetHostRatioRatios[sentenceIndex];
                SentenceParsingRatioLimits limits =
                    SentenceParsingLimitsForRatioWalker.FindLimitsFromLengths(targetSentenceLength, hostSentenceLength);

                // If the ratio is out of range.
                if (!limits.IsInRatioLimits(targetHostRatio))
                {
                    int joinIndex = sentenceIndex;

                    // If it's the first sentence, we can only join with the next.
                    if (sentenceIndex == 0)
                        joinIndex++;
                    // If it's the last sentence, we can only join with the previous.
                    else if (sentenceIndex == sentenceCount - 1)
                        joinIndex--;
                    // Otherwise we pick which one to join with.
                    else
                    {
                        // Get the ratios for the previous and next sentences.
                        double previousRatio = targetHostRatioRatios[sentenceIndex - 1];
                        double nextRatio = targetHostRatioRatios[sentenceIndex + 1];

                        // Pick one to join with.
                        if (targetHostRatio < limits.RatioLow)
                        {
                            if (previousRatio >= nextRatio)
                                joinIndex--;
                            else
                                joinIndex++;
                        }
                        else
                        {
                            if (previousRatio <= nextRatio)
                                joinIndex--;
                            else
                                joinIndex++;
                        }
                    }

                    if (joinIndex > sentenceIndex)
                    {
                        studyItem.JoinSentenceRuns(languageIDs, sentenceIndex, 2);
                        collapsedFlags[sentenceIndex] = true;
                        collapsedFlags.RemoveAt(joinIndex);
                    }
                    else
                    {
                        studyItem.JoinSentenceRuns(languageIDs, joinIndex, 2);
                        collapsedFlags[joinIndex] = true;
                        collapsedFlags.RemoveAt(sentenceIndex);
                    }

                    returnValue = true;
                    break;  // Stop once we've collapsed one sentence pair.
                }
            }

            return returnValue;
        }

        // Returns false if failed verification.
        public static bool GetSentenceParsingRatioRatios(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs,
            out List<double> targetRatios,
            out List<double> hostRatios,
            out List<double> targetHostRatioRatios,
            out List<bool> failedFlags)
        {
            bool returnValue = false;

            targetRatios = new List<double>();
            hostRatios = new List<double>();
            targetHostRatioRatios = new List<double>();
            failedFlags = new List<bool>();

            if ((languageIDs == null) || (languageIDs.Count() != 2))
                throw new Exception("GetSentenceParsingRatioRatios: Need two languages.");


            LanguageID targetLanguageID = languageIDs[0];
            LanguageID hostLanguageID = languageIDs[1];
            LanguageItem targetLanguageItem = studyItem.LanguageItem(targetLanguageID);
            LanguageItem hostLanguageItem = studyItem.LanguageItem(hostLanguageID);

            int sentenceCount = studyItem.GetSentenceCount(languageIDs);
            int sentenceIndex;
            List<double> ratios;

#if false
            for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
            {
                TextRun targetSentenceRun = (targetLanguageItem != null ? targetLanguageItem.GetSentenceRun(sentenceIndex) : null);
                TextRun hostSentenceRun = (hostLanguageItem != null ? hostLanguageItem.GetSentenceRun(sentenceIndex) : null);
                int targetSentenceLength = (targetSentenceRun != null ? targetSentenceRun.Length : 0);
                int hostSentenceLength = (hostSentenceRun != null ? hostSentenceRun.Length : 0);
                double ratio;

                if (hostSentenceLength != 0)
                {
                    ratio = (double)targetSentenceLength / (double)hostSentenceLength;
                    ratios.Add(ratio);

                    if (!SentenceParsingLimitsForRatioWalker.AreInLimits(targetSentenceLength, hostSentenceLength, ratio))
                        return false;
                }
                else
                    return false;
            }

            for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
            {
                for (languageIndex = 0; languageIndex < languageCount; languageIndex++)
                {
                    LanguageID languageID = languageIDs[languageIndex];
                    LanguageItem languageItem = studyItem.LanguageItem(languageID);

                    ratios = (languageIndex == 0 ? targetRatios : hostRatios);

                    if (languageItem == null)
                    {
                        ratios.Add(0.0);
                        continue;
                    }

                    TextRun sentenceRun = languageItem.GetSentenceRun(sentenceIndex);

                    if (sentenceRun != null)
                    {
                        double ratio = (double)sentenceRun.Length / (double)languageItem.TextLength;
                        ratios.Add(ratio);
                    }
                    else
                        ratios.Add(0.0);
                }

                double targetRatio = targetRatios[sentenceIndex];
                double hostRatio = hostRatios[sentenceIndex];

                if (hostRatio == 0.0)
                    return false;

                double targetHostRatio = targetRatio / hostRatio;

                targetHostRatioRatios.Add(targetHostRatio);

                if ((targetHostRatio < sentenceParsingRatioLowThreshold) || (targetHostRatio > sentenceParsingRatioHighThreshold))
                {
                    failedFlags.Add(true);
                    returnValue = false;
                }
                else
                    failedFlags.Add(false);
            }
#endif

            return returnValue;
        }

#if false
        public static int CounterMismatches;
#endif
#endif

        public static string[] GetPunctuationDelimiters(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs,
            bool isIgnoreLastPunctuation)
        {
            if ((studyItem == null) || (languageIDs == null) || (languageIDs.Count() == 0))
                return new string[0];

            List<PunctuationCounter> counters = new List<PunctuationCounter>(languageIDs.Count());

#if false
            bool mismatched = false;
#endif

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageItem languageItem = studyItem.LanguageItem(languageID);

                if ((languageItem == null) || !languageItem.HasText())
                {
                    counters.Add(new PunctuationCounter());
                    continue;
                }

                string text = languageItem.Text;

                if (isIgnoreLastPunctuation)
                    text = TextUtilities.RemovePunctuationFromStartAndEnd(text);

                PunctuationCounter counter = new PunctuationCounter(languageID.LanguageCultureExtensionCode, text, languageID);
                counters.Add(counter);
            }

            string[] delimiters = PunctuationCounter.GetUsedOrCommonDelimiters(counters);

            return delimiters;
        }

        public static bool DumpDebugging = false;
        public static ApplicationData.DumpString DumpStringDelegate = null;

        public static void ParseSentenceRunsRatioWalker(
            MultiLanguageItem studyItem,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            List<LanguageID> uniqueLanguageIDs)
        {

            if ((uniqueLanguageIDs == null) || (uniqueLanguageIDs.Count() == 0))
                throw new Exception("ParseSentenceRunsRatioWalker: null uniqueLanguageIDs");

            ParseSentenceRunsRawPunctuation(studyItem, uniqueLanguageIDs);

            foreach (LanguageID hostLanguageID in hostLanguageIDs)
            {
                int targetLanguageIndex;
                int targetLanguageCount = targetLanguageIDs.Count();

                for (targetLanguageIndex = 0; targetLanguageIndex < targetLanguageCount; targetLanguageIndex++)
                {
                    LanguageID targetLanguageID = targetLanguageIDs[targetLanguageIndex];

                    List<int> targetIndexesRaw = GetIndexes(studyItem, targetLanguageID);
                    List<int> hostIndexesRaw = GetIndexes(studyItem, hostLanguageID);

                    LanguageItem targetLanguageItem = studyItem.LanguageItem(targetLanguageID);
                    LanguageItem hostLanguageItem = studyItem.LanguageItem(hostLanguageID);

                    int targetLength = (targetLanguageItem != null ? targetLanguageItem.TextLength : 0);
                    int hostLength = (hostLanguageItem != null ? hostLanguageItem.TextLength : 0);

                    List<int> targetIndexesChosen = new List<int>();
                    List<int> hostIndexesChosen = new List<int>();
                    List<SentenceParsingInfo> parsingInfo = new List<SentenceParsingInfo>();

                    ChooseIndexes(
                        studyItem,
                        targetLanguageID,
                        hostLanguageID,
                        targetIndexesRaw,
                        hostIndexesRaw,
                        targetLength,
                        hostLength,
                        targetIndexesChosen,
                        hostIndexesChosen,
                        parsingInfo);

                    ApplyIndexes(studyItem, targetLanguageID, targetIndexesChosen);
                    ApplyIndexes(studyItem, hostLanguageID, hostIndexesChosen);
                    ApplyParsingInfo(studyItem, targetLanguageID, parsingInfo);
                }
            }
        }

        public static List<int> GetIndexes(
            MultiLanguageItem studyItem,
            LanguageID languageID)
        {
            LanguageItem languageItem = studyItem.LanguageItem(languageID);
            List<int> runLengths = new List<int>();

            if (languageItem != null)
            {
                List<TextRun> sentenceRuns = languageItem.SentenceRuns;
                int sentenceCount = sentenceRuns.Count();
                int sentenceIndex;

                for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
                {
                    TextRun sentenceRun = sentenceRuns[sentenceIndex];
                    runLengths.Add(sentenceRun.Start);
                }

                int textLength = languageItem.TextLength;

                if (!runLengths.Contains(textLength))
                    runLengths.Add(textLength);
            }
            else
                runLengths.Add(0);

            return runLengths;
        }

        public static void ChooseIndexes(
            MultiLanguageItem studyItem,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            List<int> targetIndexesRaw,
            List<int> hostIndexesRaw,
            int targetLength,
            int hostLength,
            List<int> targetLengthsChosen,
            List<int> hostLengthsChosen,
            List<SentenceParsingInfo> parsingInfo)
        {
            int targetCount = targetIndexesRaw.Count();
            int hostCount = hostIndexesRaw.Count();
            int targetIndex = 0;
            int hostIndex = 0;

            while (ChooseIndexesStep(
                studyItem,
                targetLanguageID,
                hostLanguageID,
                targetIndexesRaw,
                hostIndexesRaw,
                targetLength,
                hostLength,
                targetLengthsChosen,
                hostLengthsChosen,
                parsingInfo,
                ref targetIndex,
                ref hostIndex))
            {
            }
        }

        public static bool ChooseIndexesStep(
            MultiLanguageItem studyItem,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            List<int> targetIndexesRaw,
            List<int> hostIndexesRaw,
            int targetLength,
            int hostLength,
            List<int> targetIndexesChosen,
            List<int> hostIndexesChosen,
            List<SentenceParsingInfo> parsingInfo,
            ref int targetIndex,
            ref int hostIndex)
        {
            int targetNextIndex = targetIndex + 1;
            int hostNextIndex = hostIndex + 1;
            int targetHostNextIndex;
            int hostTargetNextIndex;
            double targetRatio;
            double targetRemainderRatio;
            double hostRatio;
            double hostRemainderRatio;
            double targetDistance;
            double hostDistance;
            double targetRemainderDistance;
            double hostRemainderDistance;
            LanguageItem targetLanguageItem = studyItem.LanguageItem(targetLanguageID);
            LanguageItem hostLanguageItem = studyItem.LanguageItem(hostLanguageID);
            string targetText = (targetLanguageItem != null ? targetLanguageItem.Text : String.Empty);
            string hostText = (hostLanguageItem != null ? hostLanguageItem.Text : String.Empty);
            bool wasAdded = false;

            if (DumpDebugging)
                DumpStringDelegate("Verse: " + studyItem.GetNamePathStringWithOrdinalInclusive(LanguageLookup.English));

            if (((targetNextIndex >= targetIndexesRaw.Count()) || (hostNextIndex >= hostIndexesRaw.Count())))
            {
                // We need to merge in the orphan sentence.
                if (targetNextIndex < targetIndexesRaw.Count())
                {
                    if (targetIndexesChosen.Count() != 0)
                    {
                        targetIndexesChosen[targetIndexesChosen.Count() - 1] = targetLength;

                        if (DumpDebugging)
                            DumpStringDelegate("Done. Merged target orphan.");
                    }
                }
                else if (hostNextIndex < hostIndexesRaw.Count())
                {
                    if (hostIndexesChosen.Count() != 0)
                    {
                        hostIndexesChosen[hostIndexesChosen.Count() - 1] = hostLength;

                        if (DumpDebugging)
                            DumpStringDelegate("Done. Merged host orphan.");
                    }
                }
                else if (DumpDebugging)
                    DumpStringDelegate("Done.");

                return false;
            }

            for (int passNumber = 1; ((targetNextIndex < targetIndexesRaw.Count()) && (hostNextIndex < hostIndexesRaw.Count())); passNumber++)
            {
                int targetSentenceLengthA;
                int hostSentenceLengthA;
                int targetRemainderLengthA;
                int hostRemainderLengthA;
                int targetSentenceLengthB;
                int hostSentenceLengthB;
                int targetRemainderLengthB;
                int hostRemainderLengthB;

                if (DumpDebugging)
                    DumpStringDelegate(
                        "Sentence " + (targetIndex + 1).ToString() + " (target-non-anchor) " + (hostIndex + 1).ToString() + " (host-anchor) pass " + passNumber.ToString());

                // Do target/host side,
                ChooseIndexesSideRatio(
                    targetText,
                    hostText,
                    targetLanguageID,
                    hostLanguageID,
                    targetIndexesRaw,
                    hostIndexesRaw,
                    targetIndex,
                    hostIndex,      // Host is the anchor.
                    targetNextIndex,
                    hostNextIndex,
                    targetLength,
                    hostLength,
                    false,
                    out targetSentenceLengthA,
                    out hostSentenceLengthA,
                    out targetRemainderLengthA,
                    out hostRemainderLengthA,
                    out targetHostNextIndex,
                    out targetRatio,
                    out targetRemainderRatio,
                    out targetDistance,
                    out targetRemainderDistance);

                if (DumpDebugging)
                    DumpStringDelegate(
                        "Sentence " + (targetIndex + 1).ToString() + " (target-anchor) " + (hostIndex + 1).ToString() + " (host-non-anchor) pass " + passNumber.ToString());

                // Do host/target side,
                ChooseIndexesSideRatio(
                    hostText,
                    targetText,
                    hostLanguageID,
                    targetLanguageID,
                    hostIndexesRaw,
                    targetIndexesRaw,
                    hostIndex,
                    targetIndex,    // Target is the anchor.
                    hostNextIndex,
                    targetNextIndex,
                    hostLength,
                    targetLength,
                    false,
                    out hostSentenceLengthB,
                    out targetSentenceLengthB,
                    out hostRemainderLengthB,
                    out targetRemainderLengthB,
                    out hostTargetNextIndex,
                    out hostRatio,
                    out hostRemainderRatio,
                    out hostDistance,
                    out hostRemainderDistance);

                int winningTargetSentenceLength = targetSentenceLengthB;
                int winningHostSentenceLength = hostSentenceLengthB;
                int winningTargetRemainderLength = targetRemainderLengthB;
                int winningHostRemainderLength = hostRemainderLengthB;
                double winningRatio = hostRatio;
                double winningRemainderRatio = hostRemainderRatio;
                double winningDistance = hostDistance;
                double winningRemainderDistance = hostRemainderDistance;
                int winningNextIndex = hostTargetNextIndex;
                bool targetWinner = false;

                double targetAverageDistance = (targetDistance + targetRemainderDistance) / 2;
                double hostAverageDistance = (hostDistance + hostRemainderDistance) / 2;

                if (targetAverageDistance <= hostAverageDistance)
                {
                    targetWinner = true;
                    winningTargetSentenceLength = targetSentenceLengthA;
                    winningHostSentenceLength = hostSentenceLengthA;
                    winningTargetRemainderLength = targetRemainderLengthA;
                    winningHostRemainderLength = hostRemainderLengthA;
                    winningRatio = targetRatio;
                    winningRemainderRatio = targetRemainderRatio;
                    winningDistance = targetDistance;
                    winningRemainderDistance = targetRemainderDistance;
                    winningNextIndex = targetHostNextIndex;
                }

                if (DumpDebugging)
                {
                    if (targetWinner)
                        DumpStringDelegate("Picked target, with host as anchor:");
                    else
                        DumpStringDelegate("Picked host, with target as anchor:");
                }

                int extraLoopCount = 0;

                while ((targetNextIndex + 1 < targetIndexesRaw.Count() - 1) && (hostNextIndex + 1 < hostIndexesRaw.Count() - 1))
                {
                    int extraWinningNextIndex;
                    double extraWinningRatio;
                    double extraWinningRemainderRatio;
                    double extraWinningDistance;
                    double extraWinningRemainderDistance;
                    int targetSentenceLengthExtra;
                    int hostSentenceLengthExtra;
                    int targetRemainderLengthExtra;
                    int hostRemainderLengthExtra;

                    if (DumpDebugging)
                        DumpStringDelegate("Extra check:");

                    if (targetWinner)
                    {
                        // Do target/host side,
                        ChooseIndexesSideRatio(
                            targetText,
                            hostText,
                            targetLanguageID,
                            hostLanguageID,
                            targetIndexesRaw,
                            hostIndexesRaw,
                            targetIndex,      // Host is the anchor.
                            hostIndex,
                            targetIndex + 1,
                            hostNextIndex + 1,
                            targetLength,
                            hostLength,
                            true,
                            out targetSentenceLengthExtra,
                            out hostSentenceLengthExtra,
                            out targetRemainderLengthExtra,
                            out hostRemainderLengthExtra,
                            out extraWinningNextIndex,
                            out extraWinningRatio,
                            out extraWinningRemainderRatio,
                            out extraWinningDistance,
                            out extraWinningRemainderDistance);
                    }
                    else
                    {
                        // Do host/target side,
                        ChooseIndexesSideRatio(
                            hostText,
                            targetText,
                            hostLanguageID,
                            targetLanguageID,
                            hostIndexesRaw,
                            targetIndexesRaw,
                            hostIndex,
                            targetIndex,    // Target is the anchor.
                            hostIndex + 1,
                            targetNextIndex + 1,
                            hostLength,
                            targetLength,
                            true,
                            out hostSentenceLengthExtra,
                            out targetSentenceLengthExtra,
                            out hostRemainderLengthExtra,
                            out targetRemainderLengthExtra,
                            out extraWinningNextIndex,
                            out extraWinningRatio,
                            out extraWinningRemainderRatio,
                            out extraWinningDistance,
                            out extraWinningRemainderDistance);
                    }

                    if ((extraWinningDistance < winningDistance) && (extraWinningRemainderDistance < winningRemainderDistance))
                    {
                        winningTargetSentenceLength = targetSentenceLengthExtra;
                        winningHostSentenceLength = hostSentenceLengthExtra;
                        winningTargetRemainderLength = targetRemainderLengthExtra;
                        winningHostRemainderLength = hostRemainderLengthExtra;
                        winningNextIndex = extraWinningNextIndex;
                        winningRatio = extraWinningRatio;
                        winningRemainderRatio = extraWinningRemainderRatio;
                        winningDistance = extraWinningDistance;
                        winningRemainderDistance = extraWinningRemainderDistance;

                        hostNextIndex++;
                        targetNextIndex++;

                        extraLoopCount++;

                        if (DumpDebugging)
                        {
                            DumpStringDelegate("    Extra chosen. extraLoopCount = " + extraLoopCount.ToString());
                            if (extraLoopCount > 1)
                                DumpStringDelegate("    extraLoopCount greater than 1");
                            DumpStringDelegate(
                                "    " + extraWinningDistance.ToString() + "(extraWinningDistance) < "
                                + winningDistance.ToString() + "(winningDistance) and");
                            DumpStringDelegate(
                                "    " + extraWinningRemainderDistance.ToString() + "(extraWinningRemainderDistance) < "
                                + winningRemainderDistance.ToString() + "(winningRemainderDistance)");
                        }
                    }
                    else
                    {
                        if (DumpDebugging)
                        {
                            DumpStringDelegate("    Extra rejected.");

                            if (extraWinningDistance >= winningDistance)
                                DumpStringDelegate(
                                    "    " + extraWinningDistance.ToString() + "(extraWinningDistance) >= "
                                    + winningDistance.ToString() + "(winningDistance)");
                            if (extraWinningRemainderDistance >= winningRemainderDistance)
                                DumpStringDelegate(
                                    "    " + extraWinningRemainderDistance.ToString() + "(extraWinningRemainderDistance) >= "
                                    + winningRemainderDistance.ToString() + "(winningRemainderDistance)");
                        }

                        break;
                    }
                }

                bool isHasWinningRemainderRatio = ((winningTargetRemainderLength != 0) && (winningHostRemainderLength != 0));

                if (SentenceParsingLimitsForRatioWalker.AreInLimits(winningTargetSentenceLength, winningHostSentenceLength, winningRatio) &&
                    (!isHasWinningRemainderRatio ||
                        SentenceParsingLimitsForRatioWalker.AreInLimits(winningTargetRemainderLength, winningHostRemainderLength, winningRemainderRatio)))
                {
                    SentenceParsingInfo info = new SentenceParsingInfo();
                    int winningTargetOffset;
                    int winningHostOffset;
                    int winningTargetNextIndex;
                    int winningHostNextIndex;

                    if (targetWinner)
                    {
                        winningTargetOffset = targetIndexesRaw[winningNextIndex];
                        winningHostOffset = hostIndexesRaw[hostNextIndex];
                        winningTargetNextIndex = winningNextIndex;
                        winningHostNextIndex = hostNextIndex;
                    }
                    else
                    {
                        winningTargetOffset = targetIndexesRaw[targetNextIndex];
                        winningHostOffset = hostIndexesRaw[winningNextIndex];
                        winningTargetNextIndex = targetNextIndex;
                        winningHostNextIndex = winningNextIndex;
                    }

                    int targetSentenceCount = winningTargetNextIndex - targetIndex;
                    int hostSentenceCount = winningHostNextIndex - hostIndex;
                    bool targetSentenceCollapsed = (targetSentenceCount > 1);
                    bool hostSentenceCollapsed = (hostSentenceCount > 1);

                    info.TargetSentenceStart = targetIndex;
                    info.HostSentenceStart = hostIndex;
                    info.TargetSentenceLength = winningTargetSentenceLength;
                    info.HostSentenceLength = winningHostSentenceLength;
                    //info.TargetSentenceStop = 0;
                    //info.HostSentenceStop = 0;
                    info.TargetParagraphLength = targetLength;
                    info.HostParagraphLength = hostLength;
                    //info.ParagraphDelimiters = String.Empty;
                    //info.TargetParagraphDelimiterCount = 0;
                    //info.HostParagraphDelimiterCount = 0;
                    //info.TargetUnusedSentenceDelimiters = String.Empty;
                    //info.HostUnusedSentenceDelimiters = String.Empty;
                    //info.TargetUnusedSentenceDelimiterCount = 0;
                    //info.HostUnusedSentenceDelimiterCount = 0;
                    //info.DelimiterDifference = 0;
                    //info.TargetParagraphWordCount = 0;
                    //info.HostParagraphWordCount = 0;
                    //info.TargetSentenceWordCount = 0;
                    //info.HostSentenceWordCount = 0;
                    info.TargetHostParagraphRatio = (double)targetLength / (double)hostLength;
                    info.TargetHostSentenceRatio = (double)winningTargetSentenceLength / (double)winningHostSentenceLength;
                    info.TargetSentenceRatio = (double)winningTargetSentenceLength / (double)(winningTargetRemainderLength + winningTargetSentenceLength);
                    info.HostSentenceRatio = (double)winningHostSentenceLength / (double)(winningHostRemainderLength + winningHostSentenceLength);
                    info.TargetHostSentenceRatioRatio = info.TargetSentenceRatio / info.HostSentenceRatio;
                    info.HostTargetSentenceRatioRatio = info.HostSentenceRatio / info.TargetSentenceRatio;
                    info.TargetSentenceCollapsed = targetSentenceCollapsed;
                    info.HostSentenceCollapsed = hostSentenceCollapsed;
                    info.SentenceFailed = false;

                    parsingInfo.Add(info);
                    wasAdded = true;

                    targetIndexesChosen.Add(winningTargetOffset);
                    hostIndexesChosen.Add(winningHostOffset);
                    targetIndex = winningTargetNextIndex;
                    hostIndex = winningHostNextIndex;

                    if (DumpDebugging)
                        DumpStringDelegate("    Range test succeeded.");


                    break;  // The winning ratio was within bounds, so we are done.
                }
                else
                {
                    if (DumpDebugging)
                    {
                        SentenceParsingRatioLimits limits = SentenceParsingLimitsForRatioWalker.FindLimitsFromLengths(
                            winningTargetSentenceLength, winningHostSentenceLength);
                        SentenceParsingRatioLimits remainderLimits = SentenceParsingLimitsForRatioWalker.FindLimitsFromLengths(
                            winningTargetSentenceLength, winningHostSentenceLength);

                        DumpStringDelegate("    Range test failed.");
                        if (!SentenceParsingLimitsForRatioWalker.AreInLimits(winningTargetSentenceLength, winningHostSentenceLength, winningRatio))
                            DumpStringDelegate("    winningRatio out of bounds (" + limits.ToString() + "): "
                                + winningRatio.ToString());

                        if (isHasWinningRemainderRatio &&
                                !SentenceParsingLimitsForRatioWalker.AreInLimits(winningTargetRemainderLength, winningHostRemainderLength, winningRemainderRatio))
                            DumpStringDelegate("    winningRemainderRatio out of bounds (" + remainderLimits.ToString() + "): " 
                                + winningRemainderRatio.ToString());
                    }

                    // It failed the range test, so we increase the anchor interval and try again.
                    targetNextIndex++;
                    hostNextIndex++;
                }
            }

            if (!wasAdded)
            {
                SentenceParsingInfo info = new SentenceParsingInfo();

                int targetSentenceCount = targetNextIndex - targetIndex;
                int hostSentenceCount = hostNextIndex - hostIndex;
                bool targetSentenceCollapsed = (targetSentenceCount > 1);
                bool hostSentenceCollapsed = (hostSentenceCount > 1);
                int targetSentenceLength = targetIndexesRaw[targetNextIndex] - targetIndexesRaw[targetIndex];
                int hostSentenceLength = hostIndexesRaw[hostNextIndex] - hostIndexesRaw[hostIndex];

                // We fill in the fields we can.
                info.TargetSentenceStart = targetIndex;
                info.HostSentenceStart = hostIndex;
                info.TargetSentenceLength = targetSentenceLength;
                info.HostSentenceLength = hostSentenceLength;
                //info.TargetSentenceStop = 0;
                //info.HostSentenceStop = 0;
                info.TargetParagraphLength = targetLength;
                info.HostParagraphLength = hostLength;
                //info.ParagraphDelimiters = String.Empty;
                //info.TargetParagraphDelimiterCount = 0;
                //info.HostParagraphDelimiterCount = 0;
                //info.TargetUnusedSentenceDelimiters = String.Empty;
                //info.HostUnusedSentenceDelimiters = String.Empty;
                //info.TargetUnusedSentenceDelimiterCount = 0;
                //info.HostUnusedSentenceDelimiterCount = 0;
                //info.DelimiterDifference = 0;
                //info.TargetParagraphWordCount = 0;
                //info.HostParagraphWordCount = 0;
                //info.TargetSentenceWordCount = 0;
                //info.HostSentenceWordCount = 0;
                info.TargetHostParagraphRatio = (double)targetLength / (double)hostLength;
                info.TargetHostSentenceRatio = (double)targetSentenceLength / (double)hostSentenceLength;
                info.TargetSentenceRatio = 0.0;
                info.HostSentenceRatio = 0.0;
                info.TargetHostSentenceRatioRatio = 0.0;
                info.HostTargetSentenceRatioRatio = 0.0;
                info.TargetSentenceCollapsed = targetSentenceCollapsed;
                info.HostSentenceCollapsed = hostSentenceCollapsed;
                info.SentenceFailed = true;

                parsingInfo.Add(info);
                wasAdded = true;

                targetIndexesChosen.Add(targetIndexesRaw[targetNextIndex]);
                hostIndexesChosen.Add(hostIndexesRaw[hostNextIndex]);
                targetIndex = targetNextIndex;
                hostIndex = hostNextIndex;

                ApplicationData.Global.PutConsoleErrorMessage("!!!! No sentence calculated for "
                    + studyItem.GetNamePathStringWithOrdinalInclusive(LanguageLookup.English)
                    + " sentence " + (targetIndex + 1).ToString() + "/" + (hostIndex + 1).ToString());
            }

            if (DumpDebugging)
                DumpStringDelegate("---\n");

            return ((targetNextIndex < targetIndexesRaw.Count()) && (hostNextIndex < hostIndexesRaw.Count()));
        }

        public static string FormatDouble(double value)
        {
            return String.Format("{0:F3}", value);
        }

        public static double DistanceFromOne(double value)
        {
            double diff = 1.0 - value;

            if (diff < 0)
                return -diff;

            return diff;
        }

        public static double UnbalancedDiffFactor = 1.7;

        public static void ChooseIndexesSideRatio(
            string majorText,
            string minorText,
            LanguageID majorLanguageID,
            LanguageID minorLanguageID,
            List<int> majorIndexesRaw,
            List<int> minorIndexesRaw,
            int majorIndex,
            int minorIndex,         // Minor is the anchor.
            int majorNextIndex,
            int minorNextIndex,
            int majorLength,
            int minorLength,
            bool dontAddMajor,
            out int chosenMajorSentenceLength,
            out int chosenMinorSentenceLength,
            out int chosenMajorRemainderLength,
            out int chosenMinorRemainderLength,
            out int chosenIndex,
            out double chosenRatio,
            out double chosenRemainderRatio,
            out double chosenDistance,
            out double chosenRemainderDistance)
        {
            int stopIndex = majorIndexesRaw.Count();
            int minorStartIndex = minorIndexesRaw[minorIndex];
            int minorStopIndex = (minorNextIndex < minorIndexesRaw.Count() ? minorIndexesRaw[minorNextIndex] : minorStartIndex);
            int minorSegmentLength = minorStopIndex - minorStartIndex;
            int minorRemainderLength = minorLength - minorStopIndex;
            int minorCombinedLength = minorLength - minorStartIndex;
            bool minorHaveRemainder = (minorRemainderLength != 0);
            double minorRatio = (double)minorSegmentLength / (double)minorCombinedLength;
            double minorRemainderRatio = 0.0;

            if (minorHaveRemainder)
                minorRemainderRatio = (double)minorRemainderLength / (double)minorCombinedLength;

            int majorStartIndex = majorIndexesRaw[majorIndex];
            int majorStopIndex1 = (majorNextIndex < majorIndexesRaw.Count() ? majorIndexesRaw[majorNextIndex] : majorStartIndex);
            int majorSegmentLength1 = majorStopIndex1 - majorStartIndex;
            int majorRemainderLength1 = majorLength - majorStopIndex1;
            int majorCombinedLength1 = majorLength - majorStartIndex;
            double majorRatio1 = (double)majorSegmentLength1 / (double)majorCombinedLength1;
            double majorMinorRatio1 = (minorRatio > 0.0 ? (majorRatio1 / minorRatio) : double.MaxValue);
            double majorMinorDiff1 = DistanceFromOne(majorMinorRatio1);
            double majorRemainderRatio1 = 0.0;
            double majorMinorRemainderRatio1 = 0.0;
            double majorMinorRemainderDiff1 = 0.0;
            double majorMinorAverageDiff1 = majorMinorDiff1;
            bool majorHaveRemainder1 = (majorRemainderLength1 != 0);
            string minorSegment = minorText.Substring(minorStartIndex, minorSegmentLength);
            string minorRemainder = minorText.Substring(minorStopIndex, minorRemainderLength);
            string majorSegment1 = majorText.Substring(majorStartIndex, majorSegmentLength1);
            string majorRemainder1 = majorText.Substring(majorStopIndex1, majorRemainderLength1);

            if (DumpDebugging)
            {
                DumpStringDelegate("minorSegment = " + minorSegment);
                DumpStringDelegate("minorRemainder = " + minorRemainder);
                DumpStringDelegate("minorSegmentLength = " + minorSegmentLength.ToString()
                    + ", minorRemainderLength = " + minorRemainderLength.ToString()
                    + ", minorCombinedLength = " + minorCombinedLength.ToString());
            }

            if (minorHaveRemainder && majorHaveRemainder1)
            {
                majorRemainderRatio1 = (double)majorRemainderLength1 / (double)majorCombinedLength1;
                majorMinorRemainderRatio1 = (minorRemainderRatio > 0.0 ? (majorRemainderRatio1 / minorRemainderRatio) : double.MaxValue);
                majorMinorRemainderDiff1 = DistanceFromOne(majorMinorRemainderRatio1);
                majorMinorAverageDiff1 = (majorMinorDiff1 + majorMinorRemainderDiff1) / 2;
            }

            chosenMajorSentenceLength = majorSegmentLength1;
            chosenMinorSentenceLength = minorSegmentLength;
            chosenMajorRemainderLength = majorRemainderLength1;
            chosenMinorRemainderLength = minorRemainderLength;
            chosenIndex = majorNextIndex;
            chosenRatio = majorMinorRatio1;
            chosenRemainderRatio = majorMinorRemainderRatio1;
            chosenDistance = majorMinorDiff1;
            chosenRemainderDistance = majorMinorRemainderDiff1;

            if (dontAddMajor || (majorNextIndex >= stopIndex - 1))
            {
                if (DumpDebugging)
                {
                    DumpStringDelegate("Choose segment 1:  Exited lower function early as we are at the end of the verse.");
                    DumpStringDelegate("majorSegment1=" + majorSegment1);
                    DumpStringDelegate("majorRemainder1=" + majorRemainder1);
                    DumpStringDelegate("majorSegmentLength1 = " + majorSegmentLength1.ToString()
                        + ", majorRemainderLength1 = " + majorRemainderLength1.ToString()
                        + ", majorCombinedLength1 = " + majorCombinedLength1.ToString());

                    if (dontAddMajor)
                    {
                        DumpStringDelegate("chosenRatio=" + chosenRatio.ToString());
                        DumpStringDelegate("chosenRemainderRatio=" + chosenRemainderRatio.ToString());
                    }
                }

                return;
            }

            for (majorNextIndex++; majorNextIndex < stopIndex; majorNextIndex++)
            {
                int majorStopIndex2 = majorIndexesRaw[majorNextIndex];
                int majorSegmentLength2 = majorStopIndex2 - majorStartIndex;
                int majorRemainderLength2 = majorLength - majorStopIndex2;
                int majorCombinedLength2 = majorLength - majorStartIndex;
                double majorRatio2 = (double)majorSegmentLength2 / (double)majorCombinedLength2;
                double majorMinorRatio2 = majorRatio2 / minorRatio;
                double majorMinorDiff2 = DistanceFromOne(majorMinorRatio2);
                double majorRemainderRatio2 = 0.0;
                double majorMinorRemainderRatio2 = 0.0f;
                double majorMinorRemainderDiff2 = 0.0;
                double majorMinorAverageDiff2 = majorMinorDiff2;
                bool majorHaveRemainder2 = (majorRemainderLength2 != 0);
                bool majorMinorHaveRemainders = (minorHaveRemainder && majorHaveRemainder1 && majorHaveRemainder2);
                string majorSegment2 = majorText.Substring(majorStartIndex, majorSegmentLength2);
                string majorRemainder2 = majorText.Substring(majorStopIndex2, majorRemainderLength2);

                if (majorMinorHaveRemainders)
                {
                    majorRemainderRatio2 = (double)majorRemainderLength2 / (double)majorCombinedLength2;
                    majorMinorRemainderRatio2 = (minorRemainderRatio > 0.0 ? (majorRemainderRatio2 / minorRemainderRatio) : double.MaxValue);
                    majorMinorRemainderDiff2 = DistanceFromOne(majorMinorRemainderRatio2);
                    majorMinorAverageDiff2 = (majorMinorDiff2 + majorMinorRemainderDiff2) / 2;

                    if (DumpDebugging)
                    {
                        DumpStringDelegate("majorSegment1 = " + majorSegment1);
                        DumpStringDelegate("majorSegment2 = " + majorSegment2);
                        DumpStringDelegate("majorRemainder1 = " + majorRemainder1);
                        DumpStringDelegate("majorRemainder2 = " + majorRemainder2);
                        DumpStringDelegate("majorSegmentLength1 = " + majorSegmentLength1.ToString()
                            + ", majorRemainderLength1 = " + majorRemainderLength1.ToString()
                            + ", majorCombinedLength1 = " + majorCombinedLength1.ToString());
                        DumpStringDelegate("majorSegmentLength2 = " + majorSegmentLength2.ToString()
                            + ", majorRemainderLength2 = " + majorRemainderLength2.ToString()
                            + ", majorCombinedLength2 = " + majorCombinedLength2.ToString());
                        DumpStringDelegate(
                            "majorMinorRatio1=" + majorMinorRatio1.ToString() + ", "
                            + "majorMinorRatio2=" + majorMinorRatio2.ToString() + ", "
                            + "majorMinorDiff1=" + majorMinorDiff1.ToString() + ", "
                            + "majorMinorDiff2=" + majorMinorDiff2.ToString());
                        DumpStringDelegate(
                            "majorMinorRemainderRatio1=" + majorMinorRemainderRatio1.ToString() + ", "
                            + "majorMinorRemainderRatio2=" + majorMinorRemainderRatio2.ToString() + ", "
                            + "majorMinorRemainderDiff1=" + majorMinorRemainderDiff1.ToString() + ", "
                            + "majorMinorRemainderDiff2=" + majorMinorRemainderDiff2.ToString());
                    }

                    if ((majorMinorDiff2 < majorMinorDiff1) && (majorMinorRemainderDiff2 < majorMinorRemainderDiff1))
                    {
                        string delimiterDump = String.Empty;
                        bool areRemaindersBalanced = (DoBalanceCheck && IsBalancedDelimiters(
                            majorRemainder2,
                            minorRemainder,
                            majorLanguageID,
                            minorLanguageID,
                            out delimiterDump));

                        if (DoBalanceCheck && !areRemaindersBalanced)
                        {
                            double majorMinorUnbalancedDiff2 = majorMinorDiff2 * UnbalancedDiffFactor;
                            double majorMinorRemainderUnbalancedDiff2 = majorMinorRemainderDiff2 * UnbalancedDiffFactor;

                            if (DumpDebugging)
                            {
                                DumpStringDelegate(
                                    "Remainders were unbalanced: " + delimiterDump
                                    + "\n    majorMinorUnbalancedDiff2=" + majorMinorUnbalancedDiff2.ToString()
                                    + ", majorMinorRemainderUnbalancedDiff2=" + majorMinorRemainderUnbalancedDiff2.ToString());
                            }

                            if ((majorMinorUnbalancedDiff2 < majorMinorDiff1) && (!majorMinorHaveRemainders || (majorMinorRemainderUnbalancedDiff2 < majorMinorRemainderDiff1)))
                            {
                                chosenMajorSentenceLength = majorSegmentLength2;
                                chosenMinorSentenceLength = minorSegmentLength;
                                chosenMajorRemainderLength = majorRemainderLength2;
                                chosenMinorRemainderLength = minorRemainderLength;
                                chosenIndex = majorNextIndex;
                                chosenRatio = majorMinorRatio2;
                                chosenRemainderRatio = majorMinorRemainderRatio2;
                                chosenDistance = majorMinorUnbalancedDiff2;
                                chosenRemainderDistance = majorMinorRemainderUnbalancedDiff2;

                                if (DumpDebugging)
                                    DumpStringDelegate("Chose segment 2 even though unbalanced: " + delimiterDump);
                            }
                            else
                            {
                                if (DumpDebugging)
                                    DumpStringDelegate("Chose segment 1. Remainders were unbalanced: " + delimiterDump);
                            }
                        }
                        else
                        {
                            chosenMajorSentenceLength = majorSegmentLength2;
                            chosenMinorSentenceLength = minorSegmentLength;
                            chosenMajorRemainderLength = majorRemainderLength2;
                            chosenMinorRemainderLength = minorRemainderLength;
                            chosenIndex = majorNextIndex;
                            chosenRatio = majorMinorRatio2;
                            chosenRemainderRatio = majorMinorRemainderRatio2;
                            chosenDistance = majorMinorDiff2;
                            chosenRemainderDistance = majorMinorRemainderDiff2;

                            if (DumpDebugging)
                            {
                                if (DoBalanceCheck)
                                    DumpStringDelegate("Chose segment 2. Remainders were balanced: " + delimiterDump);
                                else
                                    DumpStringDelegate("Chose segment 2.");
                            }
                        }
                    }
                    else if (DumpDebugging)
                        DumpStringDelegate("Chose segment 1.");
                }
                else
                {
                    double finalMinorRatio = minorRatio;
                    double finalMajorRatio1 = majorRatio1;
                    double finalMajorRatio2 = majorRatio2;
                    double finalMajorMinorRatio1 = finalMajorRatio1 / finalMinorRatio;
                    double finalMajorMinorRatio2 = finalMajorRatio2 / finalMinorRatio;
                    double finalMajorMinorDiff1 = DistanceFromOne(finalMajorMinorRatio1);
                    double finalMajorMinorDiff2 = DistanceFromOne(finalMajorMinorRatio2);

                    if (DumpDebugging)
                    {
                        DumpStringDelegate("majorSegment1 = " + majorSegment1);
                        DumpStringDelegate("majorSegment2 = " + majorSegment2);
                        DumpStringDelegate(
                            "finalMajorMinorRatio1=" + finalMajorMinorRatio1.ToString() + ", "
                            + "finalMajorMinorRatio2=" + finalMajorMinorRatio2.ToString() + ", "
                            + "finalMajorMinorDiff1=" + finalMajorMinorDiff1.ToString() + ", "
                            + "finalMajorMinorDiff2=" + finalMajorMinorDiff2.ToString());
                    }

                    if (finalMajorMinorDiff2 < finalMajorMinorDiff1)
                    {
                        string delimiterDump = String.Empty;
                        bool areRemaindersBalanced = (DoBalanceCheck && IsBalancedDelimiters(
                            majorRemainder2,
                            minorRemainder,
                            majorLanguageID,
                            minorLanguageID,
                            out delimiterDump));

                        if (DoBalanceCheck && !areRemaindersBalanced)
                        {
                            double finalMajorMinorUnbalancedDiff2 = finalMajorMinorDiff2 * UnbalancedDiffFactor;

                            if (DumpDebugging)
                            {
                                DumpStringDelegate(
                                    "Remainders were unbalanced: " + delimiterDump
                                    + "\n    finalMajorMinorUnbalancedDiff2=" + finalMajorMinorUnbalancedDiff2.ToString());
                            }

                            if (finalMajorMinorUnbalancedDiff2 < finalMajorMinorDiff1)
                            {
                                chosenMajorSentenceLength = majorSegmentLength2;
                                chosenMinorSentenceLength = minorSegmentLength;
                                chosenMajorRemainderLength = majorRemainderLength2;
                                chosenMinorRemainderLength = minorRemainderLength;
                                chosenIndex = majorNextIndex;
                                chosenRatio = majorMinorRatio2;
                                chosenRemainderRatio = 1.0;
                                chosenDistance = finalMajorMinorUnbalancedDiff2;
                                chosenRemainderDistance = 0.0;

                                if (DumpDebugging)
                                    DumpStringDelegate("Chose final segment 2 even though unbalanced: " + delimiterDump);
                            }
                            else
                            {
                                if (DumpDebugging)
                                    DumpStringDelegate("Chose final segment 1. Remainders were unbalanced: " + delimiterDump);
                            }
                        }
                        else
                        {
                            chosenMajorSentenceLength = majorSegmentLength2;
                            chosenMinorSentenceLength = minorSegmentLength;
                            chosenMajorRemainderLength = majorRemainderLength2;
                            chosenMinorRemainderLength = minorRemainderLength;
                            chosenIndex = majorNextIndex;
                            chosenRatio = majorMinorRatio2;
                            chosenRemainderRatio = 1.0;
                            chosenDistance = majorMinorDiff2;
                            chosenRemainderDistance = 0.0;

                            if (DumpDebugging)
                            {
                                if (DoBalanceCheck)
                                    DumpStringDelegate("Chose final segment 2. Remainders were balanced: " + delimiterDump);
                                else
                                    DumpStringDelegate("Chose final segment 2.");
                            }
                        }
                    }
                    else if (DumpDebugging)
                        DumpStringDelegate("Chose final segment 1.");
                }

                if ((majorMinorDiff2 > majorMinorDiff1) || (majorMinorHaveRemainders && (majorMinorRemainderDiff2 > majorMinorRemainderDiff1)))
                    break;

                majorMinorRatio1 = majorMinorRatio2;
                majorMinorDiff1 = majorMinorDiff2;
                majorMinorAverageDiff1 = majorMinorAverageDiff2;
                majorRemainderRatio1 = majorRemainderRatio2;
                majorMinorRemainderRatio1 = majorMinorRemainderRatio2;
                majorMinorRemainderDiff1 = majorMinorRemainderDiff2;
                majorSegment1 = majorSegment2;
                majorRemainder1 = majorRemainder2;
                majorHaveRemainder1 = majorHaveRemainder2;
            }
        }

        public enum CheckType
        {
            None,
            Counts,
            Order,
            Compatibility
        }

        public static bool DoBalanceCheck = true;

        public static CheckType[] CheckTypes =
            {
                //CheckType.None,
                CheckType.Counts,
                //CheckType.Order,
                //CheckType.Compatibility,
                //CheckType.Counts, CheckType.Compatibility,
                //CheckType.Order, CheckType.Compatibility,
        };

        public static bool IsBalancedDelimiters(
            string majorText,
            string minorText,
            LanguageID majorLanguageID,
            LanguageID minorLanguageID,
            out string delimiterDump)
        {
            bool returnValue = IsBalancedDelimitersWithTypes(
                CheckTypes,
                majorText,
                minorText,
                majorLanguageID,
                minorLanguageID,
                out delimiterDump);
            return returnValue;
        }

        public static bool IsBalancedDelimitersWithTypes(
            CheckType[] checkTypes,
            string majorText,
            string minorText,
            LanguageID majorLanguageID,
            LanguageID minorLanguageID,
            out string delimiterDump)
        {
            bool returnValue = true;

            delimiterDump = String.Empty;

            foreach (CheckType type in checkTypes)
            {
                switch (type)
                {
                    case CheckType.None:
                        break;
                    case CheckType.Counts:
                        returnValue = IsBalancedDelimitersCounts(
                            majorText,
                            minorText,
                            majorLanguageID,
                            minorLanguageID,
                            ref delimiterDump) && returnValue;
                        break;
                    case CheckType.Order:
                        returnValue = IsBalancedDelimitersOrder(
                            majorText,
                            minorText,
                            majorLanguageID,
                            minorLanguageID,
                            ref delimiterDump) && returnValue;
                        break;
                    case CheckType.Compatibility:
                        returnValue = IsBalancedDelimitersCompatibility(
                            majorText,
                            minorText,
                            majorLanguageID,
                            minorLanguageID,
                            ref delimiterDump) && returnValue;
                        break;
                    default:
                        throw new Exception("IsBalancedDelimitersWithTypes: Unsupported check type: " + type.ToString());
                }
            }

            return returnValue;
        }

        public static bool IsBalancedDelimitersCounts(
            string majorText,
            string minorText,
            LanguageID majorLanguageID,
            LanguageID minorLanguageID,
            ref string delimiterDump)
        {
            bool returnValue = true;

            delimiterDump = null;

            string majorTextTrimmed = TextUtilities.RemovePunctuationFromStartAndEnd(majorText);
            string minorTextTrimmed = TextUtilities.RemovePunctuationFromStartAndEnd(minorText);

            PunctuationCounter majorCounter = new PunctuationCounter(null, majorTextTrimmed, majorLanguageID);
            PunctuationCounter minorCounter = new PunctuationCounter(null, minorTextTrimmed, minorLanguageID);

            int majorDelimiterCount = majorCounter.GetSumOfCurrentCounts();
            int minorDelimiterCount = minorCounter.GetSumOfCurrentCounts();

            returnValue = (majorDelimiterCount == minorDelimiterCount);

            if (DumpDebugging)
            {
                if (!String.IsNullOrEmpty(delimiterDump))
                    delimiterDump += "; ";

                if (!returnValue)
                    delimiterDump += "Counts failed: ";

                delimiterDump += "majorDelimiterCount = " + majorDelimiterCount.ToString()
                    + ", minorDelimiterCount = " + minorDelimiterCount.ToString();

                delimiterDump += "    Major " + majorCounter.OrderString();
                delimiterDump += "\n";
                delimiterDump += "    Minor " + minorCounter.OrderString();
            }

            return returnValue;
        }

        public static bool IsBalancedDelimitersOrder(
            string majorText,
            string minorText,
            LanguageID majorLanguageID,
            LanguageID minorLanguageID,
            ref string delimiterDump)
        {
            bool returnValue = true;

            delimiterDump = null;

            string majorTextTrimmed = TextUtilities.RemovePunctuationFromStartAndEnd(majorText);
            string minorTextTrimmed = TextUtilities.RemovePunctuationFromStartAndEnd(minorText);

            PunctuationCounter majorCounter = new PunctuationCounter(null, majorTextTrimmed, majorLanguageID);
            PunctuationCounter minorCounter = new PunctuationCounter(null, minorTextTrimmed, minorLanguageID);

            returnValue = PunctuationCounter.CompareDelimiterOrder(majorCounter, minorCounter);

            if (DumpDebugging)
            {
                if (!String.IsNullOrEmpty(delimiterDump))
                    delimiterDump += "; ";

                if (!returnValue)
                    delimiterDump += "Order failed:\n";

                delimiterDump += "    Major " + majorCounter.OrderString();
                delimiterDump += "\n";
                delimiterDump += "    Minor " + minorCounter.OrderString();
            }

            return returnValue;
        }

        public static bool IsBalancedDelimitersCompatibility(
            string majorText,
            string minorText,
            LanguageID majorLanguageID,
            LanguageID minorLanguageID,
            ref string delimiterDump)
        {
            bool returnValue = true;

            delimiterDump = null;

            string majorTextTrimmed = TextUtilities.RemovePunctuationFromStartAndEnd(majorText);
            string minorTextTrimmed = TextUtilities.RemovePunctuationFromStartAndEnd(minorText);

            PunctuationCounter majorCounter = new PunctuationCounter(null, majorTextTrimmed, majorLanguageID);
            PunctuationCounter minorCounter = new PunctuationCounter(null, minorTextTrimmed, minorLanguageID);

            returnValue = PunctuationCounter.CompareCompatibleDelimiterOrder(majorCounter, minorCounter);

            if (DumpDebugging)
            {
                if (!String.IsNullOrEmpty(delimiterDump))
                    delimiterDump += "; ";

                if (!returnValue)
                    delimiterDump += "Compatibility order failed:\n";

                delimiterDump += "    Major " + majorCounter.OrderString();
                delimiterDump += "\n";
                delimiterDump += "    Minor " + minorCounter.OrderString();
            }

            return returnValue;
        }

        public static void ApplyIndexes(
            MultiLanguageItem studyItem,
            LanguageID languageID,
            List<int> indexes)
        {
            LanguageItem languageItem = studyItem.LanguageItem(languageID);
            int sentenceCount = (languageItem != null ? languageItem.SentenceRunCount() : 0);
            int sentenceIndex;
            int indexIndex = 0;
            int joinCount = 0;

            for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
            {
                int index;
                if (indexIndex >= indexes.Count())
                    index = languageItem.TextLength;
                else
                    index = indexes[indexIndex];
                int nextStart = ((sentenceIndex == sentenceCount - 1) ? languageItem.TextLength : languageItem.GetSentenceRun(sentenceIndex + 1).Start);
                if (nextStart == index)
                {
                    if (joinCount != 0)
                    {
                        languageItem.JoinSentenceRuns(sentenceIndex - joinCount, joinCount + 1);
                        sentenceCount -= joinCount;
                        sentenceIndex -= joinCount;
                    }

                    indexIndex++;
                    joinCount = 0;
                }
                else
                    joinCount++;
            }
        }

        public static void ApplyParsingInfo(
            MultiLanguageItem studyItem,
            LanguageID languageID,
            List<SentenceParsingInfo> parsingInfo)
        {
            LanguageItem languageItem = studyItem.LanguageItem(languageID);
            int sentenceCount = (languageItem != null ? languageItem.SentenceRunCount() : 0);
            int sentenceIndex;

            for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
            {
                TextRun sentenceRun = languageItem.GetSentenceRun(sentenceIndex);

                if (sentenceRun == null)
                {
                    if (DumpDebugging)
                        DumpStringDelegate("ApplyParsingInfo: Null sentence run: " + studyItem.GetNamePathStringWithOrdinalInclusive(LanguageLookup.English));
                    continue;
                }

                if (sentenceIndex >= parsingInfo.Count())
                {
                    if (DumpDebugging)
                        DumpStringDelegate("ApplyParsingInfo: parsingInfo count mismatch: Sentence count = " +
                            sentenceCount.ToString() + ", Info count = " + parsingInfo.Count().ToString() +
                            ": " + studyItem.GetNamePathStringWithOrdinalInclusive(LanguageLookup.English));
                    continue;
                }

                SentenceParsingInfo info = parsingInfo[sentenceIndex];

                sentenceRun.ParsingInfo = info;
            }
        }

        public static void GetParagraphInfo(
            MultiLanguageItem studyItem,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            out string paragraphDelimiters,
            out int targetParagraphDelimiterCount,
            out int hostParagraphDelimiterCount,
            out int targetParagraphLength,
            out int hostParagraphLength,
            out double targetHostParagraphLengthRatio,
            out int targetParagraphWordCount,
            out int hostParagraphWordCount)
        {
            string targetLanguageCode = targetLanguageID.LanguageCultureExtensionCode;
            string hostLanguageCode = hostLanguageID.LanguageCultureExtensionCode;
            LanguageItem targetLanguageItem = studyItem.LanguageItem(targetLanguageID);

            if (targetLanguageItem == null)
                throw new Exception("Target language item null: " + studyItem.GetNamePathStringWithOrdinalInclusive(LanguageLookup.English));

            LanguageItem hostLanguageItem = studyItem.LanguageItem(hostLanguageID);

            if (hostLanguageItem == null)
                throw new Exception("Host language item null: " + studyItem.GetNamePathStringWithOrdinalInclusive(LanguageLookup.English));

            string targetParagraph = targetLanguageItem.Text;
            string hostParagraph = hostLanguageItem.Text;

            string targetParagraphTrimmed = TextUtilities.RemovePunctuationFromStartAndEnd(targetLanguageItem.Text);
            string hostParagraphTrimmed = TextUtilities.RemovePunctuationFromStartAndEnd(hostLanguageItem.Text);

            targetParagraphDelimiterCount = PunctuationCounter.CountTotalPunctuation(targetParagraphTrimmed, targetLanguageID);
            hostParagraphDelimiterCount = PunctuationCounter.CountTotalPunctuation(hostParagraphTrimmed, hostLanguageID);

            targetParagraphLength = targetLanguageItem.TextLength;
            hostParagraphLength = hostLanguageItem.TextLength;
            targetHostParagraphLengthRatio = (hostParagraphLength != 0 ? (double)targetParagraphLength / hostParagraphLength : 0.0);

            targetParagraphWordCount = targetLanguageItem.WordRunCount();
            hostParagraphWordCount = hostLanguageItem.WordRunCount();

            PunctuationCounter targetPunctuationCounter = new PunctuationCounter(
                targetLanguageCode,
                targetParagraph,
                targetLanguageID);
            PunctuationCounter hostPunctuationCounter = new PunctuationCounter(
                hostLanguageCode,
                hostParagraph,
                hostLanguageID);

            string[] delimitersInParagraph = PunctuationCounter.GetUnionOfDelimiters(targetPunctuationCounter, hostPunctuationCounter);
            paragraphDelimiters = TextUtilities.GetConcatenatedStringFromStringArray(delimitersInParagraph);

            List<TextRun> targetSentenceRuns = targetLanguageItem.SentenceRuns;
            List<TextRun> hostSentenceRuns = hostLanguageItem.SentenceRuns;

            int targetSentenceCount = targetSentenceRuns.Count();
            int hostSentenceCount = hostSentenceRuns.Count();

            int sentenceCount = (targetSentenceCount > hostSentenceCount ? targetSentenceCount : hostSentenceCount);
            int sentenceIndex;

            for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
            {
                TextRun targetSentenceRun = (sentenceIndex < targetSentenceRuns.Count() ? targetSentenceRuns[sentenceIndex] : null);
                TextRun hostSentenceRun = (sentenceIndex < hostSentenceRuns.Count() ? hostSentenceRuns[sentenceIndex] : null);
                SentenceParsingInfo info = (targetSentenceRun != null ? targetSentenceRun.ParsingInfo : new SentenceParsingInfo());
                int sentenceNumber = sentenceIndex + 1;
                string targetSentence = targetLanguageItem.GetRunText(targetSentenceRun);
                string hostSentence = hostLanguageItem.GetRunText(hostSentenceRun);
                string targetUnusedSentenceDelimiters = String.Empty;
                string hostUnusedSentenceDelimiters = String.Empty;
                int targetUnusedSentenceDelimiterCount = 0;
                int hostUnusedSentenceDelimiterCount = 0;
                int usedTargetDelimiters = 0;
                int usedHostDelimiters = 0;
                int delimiterDifference = 0;
                int targetSentenceWordCount = targetLanguageItem.SentenceWordRunCount(sentenceIndex);
                int hostSentenceWordCount = hostLanguageItem.SentenceWordRunCount(sentenceIndex);
                double targetHostSentenceLengthRatio = 0.0;
                int targetSentenceLength = 0;
                int hostSentenceLength = 0;

                PunctuationCounter.GetTargetHostStatistics(
                    targetSentence,
                    hostSentence,
                    delimitersInParagraph,
                    targetLanguageID,
                    hostLanguageID,
                    true,
                    ref targetUnusedSentenceDelimiters,
                    ref hostUnusedSentenceDelimiters,
                    ref targetUnusedSentenceDelimiterCount,
                    ref hostUnusedSentenceDelimiterCount,
                    ref usedTargetDelimiters,
                    ref usedHostDelimiters,
                    ref delimiterDifference,
                    ref targetSentenceLength,
                    ref hostSentenceLength,
                    ref targetHostSentenceLengthRatio);

                if (info == null)
                    info = new SentenceParsingInfo();

                info.ParagraphDelimiters = paragraphDelimiters;
                info.TargetParagraphDelimiterCount = targetParagraphDelimiterCount;
                info.HostParagraphDelimiterCount = hostParagraphDelimiterCount;
                info.TargetUnusedSentenceDelimiters = targetUnusedSentenceDelimiters;
                info.HostUnusedSentenceDelimiters = hostUnusedSentenceDelimiters;
                info.TargetUnusedSentenceDelimiterCount = targetUnusedSentenceDelimiterCount;
                info.HostUnusedSentenceDelimiterCount = hostUnusedSentenceDelimiterCount;
                info.DelimiterDifference = delimiterDifference;
                info.TargetParagraphWordCount = targetParagraphWordCount;
                info.HostParagraphWordCount = hostParagraphWordCount;
                info.TargetSentenceWordCount = targetSentenceWordCount;
                info.HostSentenceWordCount = hostSentenceWordCount;
            }
        }

        public static void ParseTextSentenceRuns(
            string text,
            LanguageID languageID,
            SentenceParsingAlgorithm parsingAlgorithm,
            ref List<TextRun> sentenceRuns)
        {
            switch (parsingAlgorithm)
            {
                case SentenceParsingAlgorithm.Context:
                    ParseTextSentenceRunsContext(text, languageID, ref sentenceRuns);
                    break;
                case SentenceParsingAlgorithm.RawPunctuation:
                    ParseTextSentenceRunsRawPunctuation(text, languageID, ref sentenceRuns);
                    break;
                case SentenceParsingAlgorithm.MatchPunctuation:
                    throw new Exception("ParseTextSentenceRuns: Can't use MatchPunctuation with single sentence.");
                case SentenceParsingAlgorithm.RatioWalker:
                    throw new Exception("ParseTextSentenceRuns: Can't use WalkRatios with single sentence.");
                default:
                    throw new Exception("ContentUtilities.ParseSentenceRuns: Unexpected parsing algorithm: " + parsingAlgorithm);
            }
        }

        public static void ParseTextSentenceRunsContext(
            string text,
            LanguageID languageID,
            ref List<TextRun> sentenceRuns)
        {
            TextRun textRun;
            int runIndex = 0;
            int charIndex;
            int startIndex = 0;
            int runLength;
            int textLength = text.Length;
            char[] sentenceTerminators = LanguageLookup.SentenceTerminatorCharacters;
            char[] spaces = LanguageLookup.SpaceCharacters;
            char[] punctuation = LanguageLookup.PunctuationCharacters;
            char[] nonMatchedPunctuationCharacters = LanguageLookup.NonMatchedPunctuationCharacters;
            List<char> matchedStack = new List<char>();
            bool quotedParagraph = false;

            if (text == null)
                text = String.Empty;

            if (sentenceRuns == null)
                sentenceRuns = new List<TextRun>();

            if (text.StartsWith("\"") && text.EndsWith("\"") && (TextUtilities.CountChars(text, '"') == 2))
                quotedParagraph = true;
            else if (text.StartsWith("“") && text.EndsWith("”") && (TextUtilities.CountChars(text, '“') == 1)
                    && (TextUtilities.CountChars(text, '”') == 1))
                quotedParagraph = true;
            else if (text.StartsWith("「") && text.EndsWith("」") && (TextUtilities.CountChars(text, '「') == 2))
                quotedParagraph = true;

            int runCount = sentenceRuns.Count();
            char chrBeforeMatched = '\0';
            char lastChar = '\0';

            for (charIndex = 0; charIndex < textLength;)
            {
                char chr = text[charIndex];
                char nextChr = (charIndex < textLength - 1 ? text[charIndex + 1] : '\0');
                char topOfStack = (matchedStack.Count() != 0 ? matchedStack.Last() : '\0');

                switch (chr)
                {
                    case '(':
                        matchedStack.Add(')');
                        chrBeforeMatched = '\0';
                        break;
                    case '（':
                        matchedStack.Add('）');
                        chrBeforeMatched = '\0';
                        break;
                    case '[':
                        matchedStack.Add(']');
                        chrBeforeMatched = '\0';
                        break;
                    case '【':
                        matchedStack.Add('】');
                        chrBeforeMatched = '\0';
                        break;
                    case '"':
                        if (!quotedParagraph)
                        {
                            if ((topOfStack == '"') || (topOfStack == '“') || (topOfStack == '”'))
                                matchedStack.RemoveAt(matchedStack.Count() - 1);
                            else
                            {
                                matchedStack.Add('"');
                                chrBeforeMatched = '\0';
                            }
                        }
                        break;
                    case '“':
                        if (!quotedParagraph)
                        {
                            matchedStack.Add('”');
                            chrBeforeMatched = '\0';
                        }
                        break;
                    case '\'':
                        if ((topOfStack == '\'') || (topOfStack == '‘') || (topOfStack == '’'))
                            matchedStack.RemoveAt(matchedStack.Count() - 1);
                        else if ((text.Length > charIndex + 2) && ((text[charIndex + 1] == '\'') || (text[charIndex + 2] == '\'')))
                        {
                            matchedStack.Add('\'');
                            chrBeforeMatched = '\0';
                        }
                        else
                            chrBeforeMatched = chr;
                        break;
                    case '‘':
                        matchedStack.Add('’');
                        chrBeforeMatched = '\0';
                        break;
                    case '「':
                        matchedStack.Add('」');
                        chrBeforeMatched = '\0';
                        break;
                    case ')':
                        if ((chr == topOfStack) || (topOfStack == '）'))
                            matchedStack.RemoveAt(matchedStack.Count() - 1);
                        break;
                    case '）':
                        if ((chr == topOfStack) || (topOfStack == ')'))
                            matchedStack.RemoveAt(matchedStack.Count() - 1);
                        break;
                    case ']':
                        if ((chr == topOfStack) || (topOfStack == '】'))
                            matchedStack.RemoveAt(matchedStack.Count() - 1);
                        break;
                    case '】':
                        if ((chr == topOfStack) || (topOfStack == ']'))
                            matchedStack.RemoveAt(matchedStack.Count() - 1);
                        break;
                    case '”':
                        if (!quotedParagraph && ((chr == topOfStack) || (topOfStack == '"') || (topOfStack == '“')))
                            matchedStack.RemoveAt(matchedStack.Count() - 1);
                        break;
                    case '’':
                        if ((chr == topOfStack) || (topOfStack == '\'') || (topOfStack == '‘'))
                            matchedStack.RemoveAt(matchedStack.Count() - 1);
                        break;
                    case '」':
                        if (!quotedParagraph && ((chr == topOfStack) || (topOfStack == '「')))
                            matchedStack.RemoveAt(matchedStack.Count() - 1);
                        break;
                    case ' ':
                    case '\r':
                    case '\n':
                    case '\t':
                        break;
                    default:
                        chrBeforeMatched = chr;
                        break;
                }

                if ((matchedStack.Count() == 0) && (sentenceTerminators.Contains(chr) || ((chr == '-') && (nextChr == '-'))))
                {
                    if ((lastChar != '\0') && Char.IsDigit(lastChar))
                    {
                        int li = charIndex + 1;
                        while ((li < textLength) && Char.IsWhiteSpace(text[li]))
                            li++;
                        if ((li < textLength) && Char.IsDigit(text[li]))
                        {
                            charIndex++;
                            lastChar = chr;
                            chrBeforeMatched = '\0';
                            continue;
                        }
                        for (; (li < textLength) && Char.IsWhiteSpace(text[li]); li++)
                            ;
                        if ((li < textLength) && Char.IsDigit(text[li]))
                        {
                            for (li = charIndex - 1; (li >= 0) && Char.IsDigit(text[li]); li--)
                                ;
                            if (li < 0)
                            {
                                charIndex++;
                                lastChar = chr;
                                chrBeforeMatched = '\0';
                                continue;
                            }
                            for (; (li >= 0) && Char.IsWhiteSpace(text[li]); li--)
                                ;
                            if ((li >= 0) && sentenceTerminators.Contains(text[li]))
                            {
                                charIndex++;
                                lastChar = chr;
                                chrBeforeMatched = '\0';
                                continue;
                            }
                        }
                    }

                    int skipOffset;

                    if ((chr == '.') && LanguageTool.CheckForAbbreviationLanguage(text, languageID, charIndex, chr, out skipOffset))
                    {
                        charIndex += skipOffset;
                        lastChar = chr;
                        chrBeforeMatched = '\0';
                        int testIndex;
                        for (testIndex = charIndex;
                                (testIndex < textLength) && char.IsWhiteSpace(text[testIndex]);
                                testIndex++)
                            ;
                        if ((testIndex < textLength) /*&& !char.IsUpper(text[testIndex])*/)
                            continue;
                    }

                    chrBeforeMatched = '\0';
                    charIndex++;

                    // Skip ellipses or other punctuation.
                    while ((charIndex < textLength)
                            && (nonMatchedPunctuationCharacters.Contains(text[charIndex])
                                || (quotedParagraph && LanguageLookup.DoubleQuoteCharacters.Contains(text[charIndex]))))
                        charIndex++;

                    runLength = charIndex - startIndex;

                    if (runIndex == sentenceRuns.Count())
                    {
                        textRun = new TextRun(startIndex, runLength, null);
                        sentenceRuns.Add(textRun);
                        runCount++;
                    }
                    else
                    {
                        textRun = sentenceRuns[runIndex];
                        textRun.Start = startIndex;
                        textRun.Length = runLength;
                    }

                    runIndex++;

                    // Skip white space.
                    while ((charIndex < textLength) && spaces.Contains(text[charIndex]))
                        charIndex++;

                    startIndex = charIndex;
                }
                else
                    charIndex++;

                lastChar = chr;
            }

            if (charIndex > startIndex)
            {
                runLength = charIndex - startIndex;

                if (runIndex == sentenceRuns.Count())
                {
                    textRun = new TextRun(startIndex, runLength, null);
                    sentenceRuns.Add(textRun);
                    runCount++;
                }
                else
                {
                    textRun = sentenceRuns[runIndex];
                    textRun.Start = startIndex;
                    textRun.Length = runLength;
                }

                runIndex++;
            }

            if (runIndex < runCount)
            {
                while (runCount > runIndex)
                {
                    runCount--;
                    sentenceRuns.RemoveAt(runCount);
                }
            }
        }

        public static void ParseTextSentenceRunsRawPunctuation(
            string text,
            LanguageID languageID,
            ref List<TextRun> sentenceRuns)
        {
            ParseTextSentenceRunsGivenPunctuation(text, languageID, LanguageLookup.RawSentenceParsingPunctuationStrings, ref sentenceRuns);
        }

        public static string[] NonSentenceBreakersDashedNames =
            {
                "Smith"
            };

        // Ignorse any punctuation at end of text.
        public static void ParseTextSentenceRunsGivenPunctuation(
            string text,
            LanguageID languageID,
            string[] punctuationStrings,
            ref List<TextRun> sentenceRuns)
        {
            TextRun textRun;
            int runIndex = 0;
            int charIndex;
            int startIndex = 0;
            int runLength;
            int textLength = text.Length;

            if (text == null)
                text = String.Empty;

            if (sentenceRuns == null)
                sentenceRuns = new List<TextRun>();

            int runCount = sentenceRuns.Count();

            for (charIndex = 0; charIndex < textLength;)
            {
                char chr = text[charIndex];
                string punct = null;
                bool isStartMarker = LanguageLookup.SentenceStartMarkers.Contains(chr);

                if (isStartMarker)
                {
                    if (charIndex == 0)
                    {
                        charIndex++;
                        continue;
                    }
                }
                else
                    punct = punctuationStrings.FirstOrDefault(x => x[0] == chr);

                if ((punct != null) || isStartMarker)
                {
                    if (charIndex == startIndex)
                    {
                        charIndex++;

                        // Skip any other punctuation.
                        while ((charIndex < textLength) && LanguageLookup.PunctuationCharacters.Contains(text[charIndex]))
                            charIndex++;
                    }
                    else
                    {
                        int skipOffset;

                        if ((chr == '.') && LanguageTool.CheckForAbbreviationLanguage(text, languageID, charIndex, chr, out skipOffset))
                        {
                            charIndex += skipOffset;
                            continue;
                        }

                        if (chr == '—')
                        {
                            bool wasFound = false;

                            foreach (string name in NonSentenceBreakersDashedNames)
                            {
                                if (charIndex >= name.Length)
                                {
                                    if (String.Compare(text, charIndex - name.Length, name, 0, name.Length) == 0)
                                    {
                                        wasFound = true;
                                        break;
                                    }
                                }
                            }

                            if (wasFound)
                            {
                                charIndex++;
                                continue;
                            }
                        }

                        if (!isStartMarker)
                        {
                            charIndex++;

                            if (punct.Length > 1)
                            {
                                int tIndex = charIndex;
                                int subIndex = 1;

                                for (; (subIndex < punct.Length) && (tIndex < textLength); subIndex++, tIndex++)
                                {
                                    if (text[tIndex] != punct[subIndex])
                                        break;
                                }

                                if (subIndex < punct.Length)
                                    continue;
                            }

                            // Skip ellipses or other punctuation.
                            while ((charIndex < textLength) && LanguageLookup.PunctuationCharacters.Contains(text[charIndex]))
                                charIndex++;

                            int tmpIndex = charIndex;

                            // Skip any whitespace and extra punctuation strings.
                            while (tmpIndex < textLength)
                            {
                                chr = text[tmpIndex];

                                if (char.IsWhiteSpace(chr))
                                    tmpIndex++;
                                else
                                {
                                    punct = punctuationStrings.FirstOrDefault(x => x[0] == chr);

                                    if (punct != null)
                                    {
                                        if (punct.Length == 1)
                                        {
                                            tmpIndex++;
                                            charIndex = tmpIndex;
                                        }
                                        else if ((punct.Length <= textLength - tmpIndex) && (String.Compare(text, tmpIndex, punct, 0, punct.Length) == 0))
                                        {
                                            tmpIndex += punct.Length;
                                            charIndex = tmpIndex;
                                        }
                                        else
                                            break;
                                    }
                                    else
                                        break;
                                }
                            }
                        }

                        runLength = charIndex - startIndex;

                        if (runIndex == sentenceRuns.Count())
                        {
                            textRun = new TextRun(startIndex, runLength, null);
                            sentenceRuns.Add(textRun);
                            runCount++;
                        }
                        else
                        {
                            textRun = sentenceRuns[runIndex];
                            textRun.Start = startIndex;
                            textRun.Length = runLength;
                        }

                        runIndex++;

                        // Skip white space.
                        while ((charIndex < textLength) && LanguageLookup.SpaceCharacters.Contains(text[charIndex]))
                            charIndex++;

                        startIndex = charIndex;
                    }
                }
                else
                    charIndex++;
            }

            if (charIndex > startIndex)
            {
                runLength = charIndex - startIndex;

                if (runIndex == sentenceRuns.Count())
                {
                    textRun = new TextRun(startIndex, runLength, null);
                    sentenceRuns.Add(textRun);
                    runCount++;
                }
                else
                {
                    textRun = sentenceRuns[runIndex];
                    textRun.Start = startIndex;
                    textRun.Length = runLength;
                }

                runIndex++;
            }

            if (runIndex < runCount)
            {
                while (runCount > runIndex)
                {
                    runCount--;
                    sentenceRuns.RemoveAt(runCount);
                }
            }
        }

        // Collapse some sentences ending with any of the given characters, if some don't end with them.
        public static void CollapseSentencesCheck(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs,
            char[] punctuationChars,
            out bool isMatch,
            out bool isCollapsedAny)
        {
            int sentenceCount = studyItem.GetMaxSentenceCount(languageIDs);
            int sentenceIndex;
            LanguageItem languageItem;

            isMatch = false;
            isCollapsedAny = false;

            for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
            {
                isCollapsedAny = false;

                int semiCount = 0;
                int nonSemiCount = 0;

                foreach (LanguageID languageID in languageIDs)
                {
                    languageItem = studyItem.LanguageItem(languageID);

                    if (languageItem == null)
                        continue;

                    TextRun sentenceRun = languageItem.GetSentenceRun(sentenceIndex);

                    if (sentenceRun == null)
                        continue;

                    string text = languageItem.GetRunText(sentenceRun);
                    char lastChr = (!String.IsNullOrEmpty(text) ? text[text.Length - 1] : '\0');

                    if (punctuationChars.Contains(lastChr))
                        semiCount++;
                    else
                        nonSemiCount++;
                }

                if ((semiCount != 0) && (nonSemiCount != 0))
                {
                    foreach (LanguageID languageID in languageIDs)
                    {
                        languageItem = studyItem.LanguageItem(languageID);

                        if (languageItem == null)
                            continue;

                        TextRun sentenceRun = languageItem.GetSentenceRun(sentenceIndex);

                        if (sentenceRun == null)
                            continue;

                        string text = languageItem.GetRunText(sentenceRun);
                        char lastChr = (!String.IsNullOrEmpty(text) ? text[text.Length - 1] : '\0');

                        if (punctuationChars.Contains(lastChr))
                        {
                            languageItem.JoinSentenceRuns(sentenceIndex, 2);
                            isCollapsedAny = true;
                        }
                    }
                }

                if (isCollapsedAny)
                    break;
            }

            isMatch = !studyItem.HaveSentenceMismatch(languageIDs);
        }

        public static void PrepareMultiLanguageItem(MultiLanguageItem multiLanguageItem, string defaultValue,
            List<LanguageDescriptor> languageDescriptors)
        {
            string stringKey = multiLanguageItem.KeyString;
            int index = 0;

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (!languageDescriptor.Used || (languageDescriptor.LanguageID == null))
                    continue;

                LanguageID languageID = languageDescriptor.LanguageID;
                LanguageItem languageItem = multiLanguageItem.LanguageItem(languageID);

                if (languageItem == null)
                    multiLanguageItem.Insert(index,
                        new LanguageItem(stringKey, languageID, defaultValue));
                else if (multiLanguageItem.LanguageItems.IndexOf(languageItem) != index)
                {
                    multiLanguageItem.LanguageItems.Remove(languageItem);
                    multiLanguageItem.Insert(index, languageItem);
                }

                index++;
            }
        }

        public static void PrepareMultiLanguageItem(MultiLanguageItem multiLanguageItem, string defaultValue,
            List<LanguageID> languageIDs)
        {
            string stringKey = multiLanguageItem.KeyString;
            int index = 0;

            foreach (LanguageID languageID in languageIDs)
            {
                if (String.IsNullOrEmpty(languageID.LanguageCode) || languageID.LanguageCode.StartsWith("("))
                    continue;

                LanguageItem languageItem = multiLanguageItem.LanguageItem(languageID);

                if (languageItem == null)
                    multiLanguageItem.Insert(index,
                        new LanguageItem(stringKey, languageID, defaultValue));
                else if (multiLanguageItem.LanguageItems.IndexOf(languageItem) != index)
                {
                    multiLanguageItem.LanguageItems.Remove(languageItem);
                    multiLanguageItem.Insert(index, languageItem);
                }

                index++;
            }
        }

        public static void PrepareMultiLanguageItem(MultiLanguageItem multiLanguageItem, string defaultValue,
            MultiLanguageItem other)
        {
            string stringKey = multiLanguageItem.KeyString;

            foreach (LanguageItem languageItem in other.LanguageItems)
            {
                LanguageID languageID = languageItem.LanguageID;

                if (multiLanguageItem.LanguageItem(languageID) == null)
                    multiLanguageItem.Add(
                        new LanguageItem(stringKey, languageID, defaultValue));
            }
        }

        public static void PrepareMultiLanguageItem(MultiLanguageItem multiLanguageItem, string speakerNameKey, string defaultValue,
            MultiLanguageItem other)
        {
            multiLanguageItem.SpeakerNameKey = speakerNameKey;
            PrepareMultiLanguageItem(multiLanguageItem, defaultValue, other);
        }

        public static void SynchronizeMultiLanguageItemLanguages(MultiLanguageItem multiLanguageItem, string defaultValue,
            List<LanguageID> languageIDs)
        {
            /*
            if (multiLanguageItem == null)
                return;

            if ((languageIDs == null) || (languageIDs.Count() == 0))
            {
                multiLanguageItem.LanguageItems = new List<LanguageItem>();
                return;
            }

            List<LanguageItem> languageItems = new List<LanguageItem>(languageIDs.Count());
            string stringKey = multiLanguageItem.KeyString;

            foreach (LanguageID languageID in languageIDs)
            {
                if (String.IsNullOrEmpty(languageID.LanguageCode) || languageID.LanguageCode.StartsWith("("))
                    continue;

                LanguageItem languageItem = multiLanguageItem.LanguageItem(languageID);

                if (languageItem == null)
                    languageItem = new LanguageItem(stringKey, languageID, defaultValue);

                languageItems.Add(languageItem);
            }

            multiLanguageItem.LanguageItems = languageItems;
            */
            // Do it non-destructively.
            PrepareMultiLanguageItem(multiLanguageItem, defaultValue, languageIDs);
        }

        public static void RekeyMultiLanguageItems(List<MultiLanguageItem> multiLanguageItems, int startIndex = 0)
        {
            int count = multiLanguageItems.Count();
            int index = 0;
            int keyIndex = startIndex;

            while (index < count)
            {
                MultiLanguageItem multiLanguageItem = multiLanguageItems[index];
                string prefix = multiLanguageItem.KeyString.Substring(0, 1);
                string key = prefix + keyIndex.ToString();
                multiLanguageItem.Rekey(key);
                index++;
                keyIndex++;
            }
        }

        public static List<MultiLanguageItem> CopyAndRekeyMultiLanguageItemsWithUseFlags(List<MultiLanguageItem> multiLanguageItems, List<bool> useFlags,
            int startIndex = 0, string keyPrefix = "I", bool recomputeRuns = false)
        {
            if ((useFlags != null) && (multiLanguageItems != null))
            {
                int count = useFlags.Count();
                List<MultiLanguageItem> newInputMultiLanguageItems = new List<MultiLanguageItem>(count);
                int index;

                for (index = 0; index < count; index++)
                {
                    if (useFlags[index])
                        newInputMultiLanguageItems.Add(multiLanguageItems[index]);
                }

                multiLanguageItems = newInputMultiLanguageItems;
            }

            return CopyAndRekeyMultiLanguageItems(multiLanguageItems, startIndex, keyPrefix, recomputeRuns);
        }

        public static List<MultiLanguageItem> CopyAndRekeyMultiLanguageItems(List<MultiLanguageItem> multiLanguageItems, int startIndex = 0, string keyPrefix = "I",
            bool recomputeRuns = false)
        {
            if (multiLanguageItems == null)
                return null;

            List<MultiLanguageItem> outputMultiLanguageItems = new List<MultiLanguageItem>(multiLanguageItems.Count());
            int count = multiLanguageItems.Count();
            int index = 0;
            int keyIndex = startIndex;

            while (index < count)
            {
                if (recomputeRuns)
                    multiLanguageItems[index].ComputeSentenceRuns();

                MultiLanguageItem multiLanguageItem = new MultiLanguageItem(multiLanguageItems[index]);
                string key = keyPrefix + keyIndex.ToString();
                multiLanguageItem.Rekey(key);
                outputMultiLanguageItems.Add(multiLanguageItem);
                index++;
                keyIndex++;
            }

            return outputMultiLanguageItems;
        }

        // Format: "DoubleNewLines", "SingleNewLine"
        public static List<MultiLanguageItem> ParseMultiLanguageItemsFromRawText(MultiLanguageItem input, string keyPrefix, int startIndex, string format)
        {
            List<List<string>> paragraphs;
            List<string> list;
            LanguageItem languageItem;
            LanguageID languageID;
            int languageCount = input.Count();
            int languageIndex;
            string text;

            if (languageCount == 0)
                return null;

            paragraphs = new List<List<string>>(languageCount);

            for (languageIndex = 0; languageIndex < languageCount; languageIndex++)
            {
                languageItem = input.LanguageItem(languageIndex);

                languageID = languageItem.LanguageID;
                text = languageItem.Text;

                if (text != null)
                    text = text.Trim();
                else
                    text = "";

                list = ParseParagraphsFromText(text, languageID, format);
                paragraphs.Add(list);
            }

            // Validate sentence counts here.

            List<MultiLanguageItem> multiLanguageItems = GetMultiLanguageItemsFromStringLists(input, paragraphs, keyPrefix, startIndex);

            return multiLanguageItems;
        }

        // Get array of paragraphs, where a paragraph is delimited by a newline (optionally preceeded by a carriage return.
        // The newlines and carriage returns will be stripped.
        public static List<string> ParseParagraphsFromText(string input, LanguageID languageID, string format)
        {
            string text = input.Replace("\r", "");
            if (format == "DoubleNewLines")
            {
                text = text.Replace("\n\n", "\r");
                text = text.Replace("\n", LanguageLookup.GetLanguageSpace(languageID));
                text = text.Replace("\r", "\n");
            }
            List<string> paragraphs;
            string[] rawParagraphs = text.Split(LanguageLookup.NewLine);
            int paragraphCount = rawParagraphs.Count();
            int paragraphIndex;

            paragraphs = new List<string>(paragraphCount);

            for (paragraphIndex = 0; paragraphIndex < paragraphCount; paragraphIndex++)
                paragraphs.Add(rawParagraphs[paragraphIndex]);

            return paragraphs;
        }

        public static List<MultiLanguageItem> GetMultiLanguageItemsFromStringLists(
            MultiLanguageItem input, List<List<string>> strings, string keyPrefix, int startIndex)
        {
            List<MultiLanguageItem> multiLanguageItems;
            MultiLanguageItem multiLanguageItem;
            List<LanguageItem> languageItems;
            LanguageItem languageItem;
            LanguageID languageID;
            string paragraph;
            int languageCount = input.Count();
            int paragraphCount = 0;
            int paragraphIndex = 0;
            int languageIndex;

            for (languageIndex = 0; languageIndex < languageCount; languageIndex++)
            {
                int count = strings[languageIndex].Count();
                if (count > paragraphCount)
                    paragraphCount = count;
            }

            multiLanguageItems = new List<MultiLanguageItem>(paragraphCount);

            for (paragraphIndex = 0; paragraphIndex < paragraphCount; paragraphIndex++)
            {
                languageItems = new List<LanguageItem>(languageCount);
                string key = keyPrefix + (paragraphIndex + startIndex).ToString();

                for (languageIndex = 0; languageIndex < languageCount; languageIndex++)
                {
                    if (paragraphIndex >= strings[languageIndex].Count())
                        paragraph = String.Empty;
                    else
                        paragraph = strings[languageIndex][paragraphIndex];

                    languageID = input.LanguageID(languageIndex);
                    languageItem = new LanguageItem(key, languageID, paragraph);
                    languageItems.Add(languageItem);
                }

                multiLanguageItem = new MultiLanguageItem(key, languageItems);
                multiLanguageItems.Add(multiLanguageItem);
            }

            return multiLanguageItems;
        }

        public static List<MultiLanguageItem> ParseVocabularyFromMultiLanguageItemsWithUseFlags(List<MultiLanguageItem> inputMultiLanguageItems, List<bool> useFlags,
            LanguageID targetLanguageID, List<LanguageID> languageIDs, string label, string keyPrefix, bool recomputeRuns, bool useTranslationService,
            DictionaryRepository dictionary, ILanguageTranslator translator, UserRecord userRecord)
        {
            if ((useFlags != null) && (inputMultiLanguageItems != null))
            {
                int count = useFlags.Count();
                List<MultiLanguageItem> newInputMultiLanguageItems = new List<MultiLanguageItem>(count);
                int index;

                for (index = 0; index < count; index++)
                {
                    if (useFlags[index])
                        newInputMultiLanguageItems.Add(inputMultiLanguageItems[index]);
                }

                inputMultiLanguageItems = newInputMultiLanguageItems;
            }

            return ParseVocabularyFromMultiLanguageItems(inputMultiLanguageItems, targetLanguageID, languageIDs, label, keyPrefix, recomputeRuns,
                useTranslationService, dictionary, translator, userRecord);
        }

        public static List<MultiLanguageItem> ParseVocabularyFromMultiLanguageItems(List<MultiLanguageItem> inputMultiLanguageItems,
            LanguageID targetLanguageID, List<LanguageID> languageIDs, string label, string keyPrefix, bool recomputeRuns,
            bool useTranslationService, DictionaryRepository dictionary, ILanguageTranslator translator, UserRecord userRecord)
        {
            List<MultiLanguageItem> outputMultiLanguageItems = new List<MultiLanguageItem>();
            int outputMultiLanguageItemIndex = 0;

            foreach (MultiLanguageItem inputMultiLanguageItem in inputMultiLanguageItems)
            {
                switch (label)
                {
                    case "Sentences":
                        ParseSentenceMultiLanguageItemsFromMultiLanguageItem(inputMultiLanguageItem, targetLanguageID, languageIDs, keyPrefix, outputMultiLanguageItemIndex,
                            recomputeRuns, outputMultiLanguageItems, out outputMultiLanguageItemIndex);
                        break;
                    case "Words":
                        ParseWordMultiLanguageItemsFromMultiLanguageItem(inputMultiLanguageItem, targetLanguageID, languageIDs, keyPrefix, outputMultiLanguageItemIndex,
                            recomputeRuns, useTranslationService, outputMultiLanguageItems, dictionary, translator, userRecord,
                            out outputMultiLanguageItemIndex);
                        break;
                    case "Characters":
                        ParseCharacterMultiLanguageItemsFromMultiLanguageItem(inputMultiLanguageItem, targetLanguageID, languageIDs, keyPrefix, outputMultiLanguageItemIndex,
                            outputMultiLanguageItems, dictionary, out outputMultiLanguageItemIndex);
                        break;
                    default:
                        break;
                }
            }

            return outputMultiLanguageItems;
        }

        public static void ParseSentenceMultiLanguageItemsFromMultiLanguageItem(MultiLanguageItem inputMultiLanguageItem, LanguageID targetLanguageID, List<LanguageID> languageIDs,
            string keyPrefix, int startIndex, bool recomputeRuns, List<MultiLanguageItem> outputMultiLanguageItems, out int nextIndex)
        {
            MultiLanguageItem outputMultiLanguageItem;

            if (recomputeRuns)
                inputMultiLanguageItem.ComputeSentenceRuns();
            else
                inputMultiLanguageItem.SentenceRunCheck();

            int sentenceCount = inputMultiLanguageItem.GetSentenceCount(languageIDs);
            int sentenceIndex;

            for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
            {
                LanguageItem languageItem = inputMultiLanguageItem.LanguageItem(targetLanguageID);

                if (languageItem != null)
                {
                    string sentence;
                    TextRun sentenceRun = languageItem.GetSentenceRun(sentenceIndex);
                    if (sentenceRun != null)
                        sentence = languageItem.GetRunText(sentenceRun);
                    else
                        sentence = languageItem.Text;
                    if (outputMultiLanguageItems.FirstOrDefault(x => x.Text(targetLanguageID) == sentence) != null)
                        continue;
                }

                outputMultiLanguageItem = inputMultiLanguageItem.GetSentenceIndexed(sentenceIndex, languageIDs);
                outputMultiLanguageItem.Rekey(keyPrefix + startIndex.ToString());
                outputMultiLanguageItems.Add(outputMultiLanguageItem);
                startIndex++;
            }

            nextIndex = startIndex;
        }

        public static void ParseWordMultiLanguageItemsFromMultiLanguageItem(MultiLanguageItem inputMultiLanguageItem, LanguageID targetLanguageID, List<LanguageID> languageIDs,
            string keyPrefix, int startIndex, bool recomputeRuns, bool useTranslationService, List<MultiLanguageItem> outputMultiLanguageItems,
            DictionaryRepository dictionary, ILanguageTranslator translator, UserRecord userRecord, out int nextIndex)
        {
            MultiLanguageItem outputMultiLanguageItem;
            LanguageItem inputLanguageItem = inputMultiLanguageItem.LanguageItem(targetLanguageID);
            TextRun wordRun;
            string word;
            DictionaryEntry dictionaryEntry;
            string key;
            string speakerKey = inputMultiLanguageItem.SpeakerNameKey;
            string errorMessage;

            nextIndex = startIndex;

            if (inputLanguageItem == null)
            {
                throw new Exception("No text in target language.");
            }

            if (recomputeRuns || (inputLanguageItem.WordRuns == null))
                inputLanguageItem.LoadWordRunsFromText(dictionary);

            int wordCount = inputLanguageItem.WordRuns.Count();
            int wordIndex;

            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                wordRun = inputLanguageItem.WordRuns[wordIndex];
                word = inputLanguageItem.Text.Substring(wordRun.Start, wordRun.Length);

                if (outputMultiLanguageItems.FirstOrDefault(x => x.Text(targetLanguageID) == word) != null)
                    continue;

                key = keyPrefix + startIndex.ToString();

                dictionaryEntry = dictionary.Get(word, targetLanguageID);

                if (dictionaryEntry != null)
                    outputMultiLanguageItem = new MultiLanguageItem(key, dictionaryEntry, languageIDs, speakerKey);
                else
                    outputMultiLanguageItem = new MultiLanguageItem(
                        key,
                        new List<LanguageItem>() { new LanguageItem(key, targetLanguageID, word) },
                        speakerKey,
                        null, null, null);

                PrepareMultiLanguageItem(outputMultiLanguageItem, speakerKey, "", inputMultiLanguageItem);

                if ((translator != null) && outputMultiLanguageItem.NeedsTranslation())
                    translator.TranslateMultiLanguageItem(
                        outputMultiLanguageItem, languageIDs, false, false, out errorMessage, false);

                outputMultiLanguageItems.Add(outputMultiLanguageItem);
                startIndex++;
            }

            nextIndex = startIndex;
        }

        public static void ParseCharacterMultiLanguageItemsFromMultiLanguageItem(MultiLanguageItem inputMultiLanguageItem, LanguageID targetLanguageID, List<LanguageID> languageIDs,
            string keyPrefix, int startIndex, List<MultiLanguageItem> outputMultiLanguageItems, DictionaryRepository dictionary, out int nextIndex)
        {
            MultiLanguageItem outputMultiLanguageItem;
            LanguageItem inputLanguageItem = inputMultiLanguageItem.LanguageItem(targetLanguageID);
            string characterString;
            DictionaryEntry dictionaryEntry;
            string key;
            string speakerKey = inputMultiLanguageItem.SpeakerNameKey;
            char[] spaceAndPunctuations = LanguageLookup.SpaceAndPunctuationCharacters;

            nextIndex = startIndex;

            if (inputLanguageItem == null)
            {
                throw new Exception("No text in target language.");
            }

            foreach (char character in inputLanguageItem.Text)
            {
                if (((character >= '\0') && (character <= '~')) || spaceAndPunctuations.Contains(character))
                    continue;

                characterString = character.ToString();

                if (outputMultiLanguageItems.FirstOrDefault(x => x.Text(targetLanguageID) == characterString) != null)
                    continue;

                key = keyPrefix + startIndex.ToString();

                dictionaryEntry = dictionary.Get(characterString, targetLanguageID);

                if (dictionaryEntry != null)
                    outputMultiLanguageItem = new MultiLanguageItem(key, dictionaryEntry, languageIDs, speakerKey);
                else
                    outputMultiLanguageItem = new MultiLanguageItem(
                        key,
                        new List<LanguageItem>() { new LanguageItem(key, targetLanguageID, characterString) },
                        speakerKey, null, null, null);

                outputMultiLanguageItems.Add(outputMultiLanguageItem);
                startIndex++;
            }

            nextIndex = startIndex;
        }

        public static void InjectMultiLanguageItems(List<MultiLanguageItem> target, List<MultiLanguageItem> source,
            string editType, int editIndex)
        {
            MultiLanguageItem multiLanguageItem;
            MultiLanguageItem testMultiLanguageItem;
            int multiLanguageItemCount = target.Count();
            int newCount = source.Count();

            if (editType == null)
                return;

            if (editType.StartsWith("insert"))
                target.InsertRange(editIndex, source);
            else if (editType.StartsWith("edit"))
            {
                multiLanguageItem = target[editIndex];
                testMultiLanguageItem = source[0];
                int anIndex;
                int aCount = testMultiLanguageItem.LanguageItems.Count();
                for (anIndex = 0; anIndex < aCount; anIndex++)
                {
                    LanguageItem testlanguageItem = testMultiLanguageItem.LanguageItem(anIndex);
                    if (testlanguageItem == null)
                        continue;
                    LanguageItem languageItem = multiLanguageItem.LanguageItem(testlanguageItem.LanguageID);
                    if (languageItem != null)
                        languageItem.Text = testlanguageItem.Text;
                    else
                        multiLanguageItem.Add(testlanguageItem);
                }
                if (source.Count() > 1)
                {
                    target.RemoveAt(editIndex);
                    source.RemoveAt(0);
                    target.InsertRange(editIndex + 1, source);
                }
            }
            else if (editType.StartsWith("append"))
                target.AddRange(source);
        }

        public static void InsertSelectFlags(List<bool> itemSelectFlags, int index, int count, bool value)
        {
            if (itemSelectFlags == null)
                return;

            int itemCount = itemSelectFlags.Count();

            for (int i = 0; i < count; i++)
            {
                if (index <= itemCount)
                    itemSelectFlags.Insert(index, value);

                itemCount++;
                index++;
            }
        }

        public static void InsertSelectFlagsSeparateSentences(List<bool> itemSelectFlags, int index, int count,
            int targetLanguageIndex, int sentenceCount, int languageCount, bool value)
        {
            if (itemSelectFlags == null)
                return;

            int itemCount = itemSelectFlags.Count();

            for (int languageIndex = languageCount - 1; languageIndex >= 0; languageIndex--)
            {
                for (int i = 0; i < count; i++)
                {
                    int effectiveIndex = index + i + (languageIndex * sentenceCount);

                    if (effectiveIndex <= itemCount)
                    {
                        if (languageIndex == targetLanguageIndex)
                            itemSelectFlags.Insert(effectiveIndex, value);
                        else
                            itemSelectFlags.Insert(effectiveIndex, !value);
                    }

                    itemCount++;
                }
            }
        }

        public static void DeleteSelectFlags(List<bool> itemSelectFlags, int index, int count)
        {
            if (itemSelectFlags == null)
                return;

            int itemCount = itemSelectFlags.Count();

            index += count - 1;

            for (int i = count - 1; i >= 0; i--)
            {
                if (index < itemCount)
                    itemSelectFlags.RemoveAt(index);

                itemCount--;
                index--;
            }
        }

        public static void DeleteSelectFlagsSeparateSentences(List<bool> itemSelectFlags, int index, int count,
            int baseIndex, int sentenceCount, int languageCount)
        {
            if (itemSelectFlags == null)
                return;

            int itemCount = itemSelectFlags.Count();

            for (int languageIndex = languageCount - 1; languageIndex >= 0; languageIndex--)
            {
                for (int i = count - 1; i >= 0; i--)
                {
                    int effectiveIndex = baseIndex + i + (languageIndex * sentenceCount);

                    if (effectiveIndex <= itemCount)
                        itemSelectFlags.RemoveAt(effectiveIndex);

                    itemCount++;
                }
            }
        }

        public static MediaRun FindStringMediaRun(LanguageItem languageItem)
        {
            if (languageItem.HasSentenceRuns())
            {
                TextRun sentenceRun = languageItem.SentenceRuns.First();

                if ((sentenceRun.MediaRuns != null) && (sentenceRun.MediaRuns.Count() != 0))
                {
                    foreach (MediaRun mediaRun in sentenceRun.MediaRuns)
                    {
                        switch (mediaRun.KeyString)
                        {
                            case "Audio":
                                return mediaRun;
                            default:
                                break;
                        }
                    }
                }
            }

            return null;
        }

        public static string AddStringMediaRun(LanguageItem languageItem, string contentType, string mediaDirectory, string mediaOwner,
            bool isShared, out MediaRun theMediaRun, out string errorMessage)
        {
            errorMessage = String.Empty;

            string sentenceString = languageItem.Text;
            string fileBaseName = TextUtilities.MakeValidFileBase(sentenceString, 160);
            string fileName = fileBaseName + ".mp3";
            string dirUrl = mediaDirectory + (mediaDirectory.EndsWith("/") ? "" : "/") + contentType;
            string sharedMediaDirectoryUrl = "~/Content/Media/" + mediaOwner + "/Shared";
            string fileUrl;

            if (isShared)
                dirUrl = "~/Content/Media/" + mediaOwner + "/Shared";
            else
                dirUrl = mediaDirectory + (mediaDirectory.EndsWith("/") ? "" : "/") + contentType;

            fileUrl = dirUrl + "/" + fileName;

            TextRun sentenceRun;
            theMediaRun = new MediaRun("Audio", fileUrl, mediaOwner);

            if (languageItem.HasSentenceRuns())
            {
                sentenceRun = languageItem.SentenceRuns.First();

                if ((sentenceRun.MediaRuns == null) || (sentenceRun.MediaRuns.Count() == 0))
                    sentenceRun.MediaRuns = new List<MediaRun>(1) { theMediaRun };
                else
                    sentenceRun.MediaRuns.Add(theMediaRun);
            }
            else
            {
                sentenceRun = new TextRun(languageItem, theMediaRun);

                if ((languageItem.SentenceRuns == null) || (languageItem.SentenceRuns.Count() == 0))
                    languageItem.SentenceRuns = new List<TextRun>(1) { sentenceRun };
            }

            return fileUrl;
        }

        public static bool HasMedia(BaseObjectContent content, int itemIndex, LanguageID languageID, string mediaType,
            UserProfile userProfile, string keyPrefix)
        {
            ContentStudyList studyList = content.GetContentStorageTyped<ContentStudyList>();
            MultiLanguageItem multiLanguageItem = null;

            if (studyList == null)
                return false;

            if (userProfile != null)
            {
                string useAudioKey = keyPrefix + "UseAudio";
                string usePictureKey = keyPrefix + "UsePicture";
                bool useAudio = userProfile.GetUserOptionFlag(useAudioKey, true);
                bool usePicture = userProfile.GetUserOptionFlag(usePictureKey, true);
                bool useMedia = useAudio || usePicture;
                switch (mediaType)
                {
                    case "Media":
                        if (!useMedia)
                            return false;
                        break;
                    case "Audio":
                        if (!useAudio)
                            return false;
                        break;
                    case "Video":
                        if (!useAudio)
                            return false;
                        break;
                    case "Automated":
                        if (!useAudio)
                            return false;
                        break;
                    case "Picture":
                        if (!usePicture)
                            return false;
                        break;
                    default:
                        break;
                }
            }

            if (itemIndex == -1)
            {
                if (!studyList.HasMediaRunWithKey(mediaType, languageID))
                    return false;
            }
            else
            {
                multiLanguageItem = studyList.GetStudyItemIndexed(itemIndex);

                if (multiLanguageItem == null)
                    return false;

                if (!multiLanguageItem.HasMediaRunWithKey(mediaType, languageID))
                    return false;
            }

            return true;
        }

        public static bool DeleteItemListMediaRunsAndMedia(List<MultiLanguageItem> multiLanguageItems, string mediaTildeUrl, bool deleteMedia)
        {
            bool returnValue = true;

            foreach (MultiLanguageItem multiLanguageItem in multiLanguageItems)
            {
                if (!DeleteStudyItemMediaRunsAndMedia(multiLanguageItem, mediaTildeUrl, deleteMedia))
                    returnValue = false;
            }

            return returnValue;
        }

        public static bool DeleteStudyItemMediaRunsAndMedia(MultiLanguageItem multiLanguageItem, string mediaTildeUrl,
            bool deleteMedia)
        {
            bool returnValue = true;

            if (multiLanguageItem.Count() == 0)
                return returnValue;

            foreach (LanguageItem languageItem in multiLanguageItem.LanguageItems)
            {
                if (!DeleteLanguageItemMediaRunsAndMedia(languageItem, mediaTildeUrl, deleteMedia))
                    returnValue = false;
            }

            return returnValue;
        }

        public static bool DeleteStudyItemLanguageMediaRunsAndMedia(MultiLanguageItem multiLanguageItem,
            LanguageID languageID, string mediaTildeUrl, bool deleteMedia)
        {
            bool returnValue = true;

            if (multiLanguageItem.Count() == 0)
                return returnValue;

            LanguageItem languageItem = multiLanguageItem.LanguageItem(languageID);

            if (languageItem != null)
            {
                if (!DeleteLanguageItemMediaRunsAndMedia(languageItem, mediaTildeUrl, deleteMedia))
                    returnValue = false;
            }

            List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(languageID);

            if (alternateLanguageIDs != null)
            {
                foreach (LanguageID alternateLanguageID in alternateLanguageIDs)
                {
                    languageItem = multiLanguageItem.LanguageItem(alternateLanguageID);

                    if (languageItem != null)
                    {
                        if (!DeleteLanguageItemMediaRunsAndMedia(languageItem, mediaTildeUrl, deleteMedia))
                            returnValue = false;
                    }
                }
            }

            return returnValue;
        }

        public static bool DeleteStudyItemSentenceRunMediaRunsAndMedia(MultiLanguageItem multiLanguageItem, int sentenceIndex,
            string mediaTildeUrl, bool deleteMedia)
        {
            bool returnValue = true;

            if (multiLanguageItem.Count() == 0)
                return returnValue;

            foreach (LanguageItem languageItem in multiLanguageItem.LanguageItems)
            {
                TextRun sentenceRun = languageItem.GetSentenceRun(sentenceIndex);

                if (sentenceRun == null)
                    continue;

                if (!DeleteSentenceRunMediaRunsAndMedia(sentenceRun, mediaTildeUrl, deleteMedia))
                    returnValue = false;
            }

            return returnValue;
        }

        public static bool DeleteLanguageItemMediaRunsAndMedia(LanguageItem languageItem, string mediaTildeUrl,
            bool deleteMedia)
        {
            bool returnValue = true;

            if (!languageItem.HasSentenceRuns())
                return true;

            foreach (TextRun sentenceRun in languageItem.SentenceRuns)
            {
                if (!DeleteSentenceRunMediaRunsAndMedia(sentenceRun, mediaTildeUrl, deleteMedia))
                    returnValue = false;
            }

            return returnValue;
        }

        public static bool DeleteSentenceRunMediaRunsAndMedia(TextRun sentenceRun, string mediaTildeUrl,
            bool deleteMedia)
        {
            bool returnValue = true;

            if (sentenceRun.MediaRuns == null)
                return true;

            if (deleteMedia)
            {
                foreach (MediaRun mediaRun in sentenceRun.MediaRuns)
                {
                    if (!mediaRun.IsReference && !mediaRun.IsFullUrl && !mediaRun.IsTildeUrl && !mediaRun.IsRelativeUrl)
                    {
                        string filePath = mediaRun.GetDirectoryPath(mediaTildeUrl);
                        try
                        {
                            MediaConvertSingleton.DeleteAlternates(filePath, mediaRun.MimeType);
                            if (FileSingleton.Exists(filePath))
                                FileSingleton.Delete(filePath);
                        }
                        catch (Exception)
                        {
                            returnValue = false;
                        }
                    }
                }
            }

            sentenceRun.MediaRuns = null;

            return returnValue;
        }

        public static bool DeleteSentenceRunLagnuageMediaRunsAndMedia(MultiLanguageItem multiLanguageItem,
            LanguageID languageID, int sentenceIndex, string mediaTildeUrl, bool deleteMedia)
        {
            bool returnValue = true;

            if (multiLanguageItem.Count() == 0)
                return returnValue;

            LanguageItem languageItem = multiLanguageItem.LanguageItem(languageID);

            if (languageItem != null)
            {
                TextRun sentenceRun = languageItem.GetSentenceRun(sentenceIndex);

                if (sentenceRun != null)
                {
                    if (!DeleteSentenceRunMediaRunsAndMedia(sentenceRun, mediaTildeUrl, deleteMedia))
                        returnValue = false;
                }
            }

            List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(languageID);

            if (alternateLanguageIDs != null)
            {
                foreach (LanguageID alternateLanguageID in alternateLanguageIDs)
                {
                    languageItem = multiLanguageItem.LanguageItem(alternateLanguageID);

                    if (languageItem != null)
                    {
                        TextRun sentenceRun = languageItem.GetSentenceRun(sentenceIndex);

                        if (sentenceRun != null)
                        {
                            if (!DeleteSentenceRunMediaRunsAndMedia(sentenceRun, mediaTildeUrl, deleteMedia))
                                returnValue = false;
                        }
                    }
                }
            }

            return returnValue;
        }

        public static bool DeleteMediaRunMedia(MediaRun mediaRun, string mediaTildeUrl)
        {
            bool returnValue = true;

            if (!mediaRun.IsReference && !mediaRun.IsFullUrl && !mediaRun.IsTildeUrl && !mediaRun.IsRelativeUrl)
            {
                string filePath = mediaRun.GetDirectoryPath(mediaTildeUrl);
                try
                {
                    MediaConvertSingleton.DeleteAlternates(filePath, mediaRun.MimeType);
                    if (FileSingleton.Exists(filePath))
                        FileSingleton.Delete(filePath);
                }
                catch (Exception)
                {
                    returnValue = false;
                }
            }

            return returnValue;
        }

        public static string GetSourceFromContentType(string contentType)
        {
            string source;

            switch (contentType)
            {
                case "Document":
                    source = "DocumentItems";
                    break;
                case "Audio":
                case "Video":
                case "Automated":
                case "Image":
                case "TextFile":
                case "PDF":
                case "Embedded":
                case "Media":
                    source = "MediaItems";
                    break;
                case "Transcript":
                case "Text":
                case "Sentences":
                case "Words":
                case "Characters":
                case "Expansion":
                case "Exercises":
                case "Notes":
                case "Comments":
                    source = "StudyLists";
                    break;
                default:
                    source = null;
                    break;
            }

            return source;
        }

        public static ContentClassType GetContentClassFromContentType(string contentType)
        {
            ContentClassType contentClass;

            switch (contentType)
            {
                case "Document":
                    contentClass = ContentClassType.DocumentItem;
                    break;
                case "Audio":
                case "Video":
                case "Automated":
                case "Image":
                case "TextFile":
                case "PDF":
                case "Embedded":
                    contentClass = ContentClassType.MediaItem;
                    break;
                case "Media":
                    contentClass = ContentClassType.MediaList;
                    break;
                case "Transcript":
                case "Text":
                case "Sentences":
                case "Words":
                case "Characters":
                case "Expansion":
                case "Exercises":
                case "Notes":
                case "Comments":
                default:
                    contentClass = ContentClassType.StudyList;
                    break;
            }

            return contentClass;
        }

        public static string GetLabelFromContentType(string contentType)
        {
            string label = contentType;

            switch (contentType)
            {
                case "Document":
                    break;
                case "Audio":
                case "Video":
                case "Automated":
                case "Image":
                case "TextFile":
                case "PDF":
                case "Embedded":
                case "Media":
                    label = "Media";
                    break;
                case "Transcript":
                case "Text":
                    label = "Text";
                    break;
                case "Sentences":
                case "Words":
                case "Characters":
                case "Expansion":
                case "Exercises":
                    break;
                case "Notes":
                case "Comments":
                    label = "Sentences";
                    break;
                default:
                    label = null;
                    break;
            }

            return label;
        }

        public static string GetContentSubTypeFromType(string contentType)
        {
            string contentSubType = string.Empty;

            switch (contentType)
            {
                case "Document":
                    contentSubType = "Summary";
                    break;
                case "Audio":
                case "Video":
                case "Automated":
                case "Image":
                case "TextFile":
                case "PDF":
                case "Embedded":
                case "Media":
                    contentSubType = "Lesson";
                    break;
                case "Transcript":
                case "Text":
                    contentSubType = "Text";
                    break;
                case "Sentences":
                case "Words":
                case "Characters":
                    contentSubType = "Vocabulary";
                    break;
                case "Expansion":
                    contentSubType = "Expansion";
                    break;
                case "Exercises":
                    contentSubType = "Exercises";
                    break;
                case "Notes":
                    contentSubType = "Notes";
                    break;
                case "Comments":
                    contentSubType = "Comments";
                    break;
                default:
                    contentSubType = null;
                    break;
            }

            return contentSubType;
        }

        public static ContentEditCommand GetContentEditCommand(string command)
        {
            ContentEditCommand returnValue;

            switch (command)
            {
                case null:
                case "":
                case "None":
                    returnValue = ContentEditCommand.None;
                    break;
                case "Display":
                    returnValue = ContentEditCommand.Display;
                    break;
                case "SelectAll":
                    returnValue = ContentEditCommand.SelectAll;
                    break;
                case "SelectNone":
                    returnValue = ContentEditCommand.SelectNone;
                    break;
                case "SelectBetween":
                    returnValue = ContentEditCommand.SelectBetween;
                    break;
                case "SelectPrevious":
                    returnValue = ContentEditCommand.SelectPrevious;
                    break;
                case "Save":
                    returnValue = ContentEditCommand.Save;
                    break;
                case "Update":
                    returnValue = ContentEditCommand.Update;
                    break;
                case "Append":
                    returnValue = ContentEditCommand.Append;
                    break;
                case "Insert":
                    returnValue = ContentEditCommand.Insert;
                    break;
                case "Cancel":
                    returnValue = ContentEditCommand.Cancel;
                    break;
                case "Delete":
                    returnValue = ContentEditCommand.Delete;
                    break;
                case "DeleteAll":
                    returnValue = ContentEditCommand.DeleteAll;
                    break;
                case "EditAnnotations":
                    returnValue = ContentEditCommand.EditAnnotations;
                    break;
                case "DeleteAnnotations":
                    returnValue = ContentEditCommand.DeleteAnnotations;
                    break;
                case "EditAnnotation":
                    returnValue = ContentEditCommand.EditAnnotation;
                    break;
                case "AddAnnotation":
                    returnValue = ContentEditCommand.AddAnnotation;
                    break;
                case "InsertAnnotation":
                    returnValue = ContentEditCommand.InsertAnnotation;
                    break;
                case "DeleteAnnotation":
                    returnValue = ContentEditCommand.DeleteAnnotation;
                    break;
                case "SaveAnnotation":
                    returnValue = ContentEditCommand.SaveAnnotation;
                    break;
                case "TranslateAnnotation":
                    returnValue = ContentEditCommand.TranslateAnnotation;
                    break;
                case "CancelAnnotation":
                    returnValue = ContentEditCommand.CancelAnnotation;
                    break;
                case "GenerateMedia":
                    returnValue = ContentEditCommand.GenerateMedia;
                    break;
                case "DeleteMedia":
                    returnValue = ContentEditCommand.DeleteMedia;
                    break;
                case "DeleteMediaRun":
                    returnValue = ContentEditCommand.DeleteMediaRun;
                    break;
                case "DeleteLanguageMedia":
                    returnValue = ContentEditCommand.DeleteLanguageMedia;
                    break;
                case "ExtractLanguageMedia":
                    returnValue = ContentEditCommand.ExtractLanguageMedia;
                    break;
                case "PropagateMap":
                    returnValue = ContentEditCommand.PropagateMap;
                    break;
                case "EditMediaRun":
                    returnValue = ContentEditCommand.EditMediaRun;
                    break;
                case "AddMediaRun":
                    returnValue = ContentEditCommand.AddMediaRun;
                    break;
                case "InsertMediaRun":
                    returnValue = ContentEditCommand.InsertMediaRun;
                    break;
                case "UploadMediaRun":
                    returnValue = ContentEditCommand.UploadMediaRun;
                    break;
                case "SaveMediaRun":
                    returnValue = ContentEditCommand.SaveMediaRun;
                    break;
                case "CancelMediaRun":
                    returnValue = ContentEditCommand.CancelMediaRun;
                    break;
                case "MoveUpMediaRun":
                    returnValue = ContentEditCommand.MoveUpMediaRun;
                    break;
                case "MoveDownMediaRun":
                    returnValue = ContentEditCommand.MoveDownMediaRun;
                    break;
                case "SaveAddMedia":
                    returnValue = ContentEditCommand.SaveAddMedia;
                    break;
                case "Join":
                    returnValue = ContentEditCommand.Join;
                    break;
                case "JoinPhrases":
                    returnValue = ContentEditCommand.JoinPhrases;
                    break;
                case "Split":
                    returnValue = ContentEditCommand.Split;
                    break;
                case "Fix":
                    returnValue = ContentEditCommand.Fix;
                    break;
                case "EditSentenceRuns":
                    returnValue = ContentEditCommand.EditSentenceRuns;
                    break;
                case "SplitOrJoinSentenceRuns":
                    returnValue = ContentEditCommand.SplitOrJoinSentenceRuns;
                    break;
                case "AutoResetSentenceRuns":
                    returnValue = ContentEditCommand.AutoResetSentenceRuns;
                    break;
                case "JoinAllSentenceRuns":
                    returnValue = ContentEditCommand.JoinAllSentenceRuns;
                    break;
                case "EditWordRuns":
                    returnValue = ContentEditCommand.EditWordRuns;
                    break;
                case "AutoResetWordRuns":
                    returnValue = ContentEditCommand.AutoResetWordRuns;
                    break;
                case "SplitOrJoinWordRuns":
                    returnValue = ContentEditCommand.SplitOrJoinWordRuns;
                    break;
                case "JoinAllWordRuns":
                    returnValue = ContentEditCommand.JoinAllWordRuns;
                    break;
                case "EditAlignment":
                    returnValue = ContentEditCommand.EditAlignment;
                    break;
                case "SaveAlignment":
                    returnValue = ContentEditCommand.SaveAlignment;
                    break;
                case "MakePhrase":
                    returnValue = ContentEditCommand.MakePhrase;
                    break;
                case "ClearLanguage":
                    returnValue = ContentEditCommand.ClearLanguage;
                    break;
                case "AddTranslations":
                    returnValue = ContentEditCommand.AddTranslations;
                    break;
                case "Copy":
                    returnValue = ContentEditCommand.Copy;
                    break;
                case "Cut":
                    returnValue = ContentEditCommand.Cut;
                    break;
                case "Paste":
                    returnValue = ContentEditCommand.Paste;
                    break;
                case "ClearSandbox":
                    returnValue = ContentEditCommand.ClearSandbox;
                    break;
                case "ShowSandbox":
                    returnValue = ContentEditCommand.ShowSandbox;
                    break;
                case "HideSandbox":
                    returnValue = ContentEditCommand.HideSandbox;
                    break;
                case "SetVoiceLanguage":
                    returnValue = ContentEditCommand.SetVoiceLanguage;
                    break;
                case "AddSynthesizedVoice":
                    returnValue = ContentEditCommand.AddSynthesizedVoice;
                    break;
                case "SaveMarkupText":
                    returnValue = ContentEditCommand.SaveMarkupText;
                    break;
                case "CopyMarkup":
                    returnValue = ContentEditCommand.CopyMarkup;
                    break;
                case "SetMediaLanguage":
                    returnValue = ContentEditCommand.SetMediaLanguage;
                    break;
                case "CollectStrings":
                    returnValue = ContentEditCommand.CollectStrings;
                    break;
                case "SetSubtitleLanguage":
                    returnValue = ContentEditCommand.SetSubtitleLanguage;
                    break;
                case "Map":
                    returnValue = ContentEditCommand.Map;
                    break;
                case "AutoMap":
                    returnValue = ContentEditCommand.AutoMap;
                    break;
                case "ClearMap":
                    returnValue = ContentEditCommand.ClearMap;
                    break;
                case "DeleteLocalStudyItems":
                    returnValue = ContentEditCommand.DeleteLocalStudyItems;
                    break;
                case "Score":
                    returnValue = ContentEditCommand.Score;
                    break;
                case "DefinitionAction":
                    returnValue = ContentEditCommand.DefinitionAction;
                    break;
                case "SetStudyItems":
                    returnValue = ContentEditCommand.SetStudyItems;
                    break;
                case "SetStage":
                    returnValue = ContentEditCommand.SetStage;
                    break;
                case "SetNextReviewTime":
                    returnValue = ContentEditCommand.SetNextReviewTime;
                    break;
                case "Recreate":
                    returnValue = ContentEditCommand.Recreate;
                    break;
                case "Reset":
                    returnValue = ContentEditCommand.Reset;
                    break;
                case "ResetAll":
                    returnValue = ContentEditCommand.ResetAll;
                    break;
                default:
                    returnValue = ContentEditCommand.None;
                    throw new Exception("GetContentEditCommand - Not implemented: " + command);
            }

            return returnValue;
        }

        public static ContentEditType GetContentEditType(string editType)
        {
            ContentEditType returnValue;

            switch (editType)
            {
                case null:
                case "":
                case "Display":
                    returnValue = ContentEditType.Display;
                    break;
                case "Append":
                    returnValue = ContentEditType.Append;
                    break;
                case "Edit":
                    returnValue = ContentEditType.Edit;
                    break;
                case "EditLanguage":
                    returnValue = ContentEditType.EditLanguage;
                    break;
                case "InsertBefore":
                    returnValue = ContentEditType.InsertBefore;
                    break;
                case "InsertAfter":
                    returnValue = ContentEditType.InsertAfter;
                    break;
                case "RecordAudio":
                    returnValue = ContentEditType.RecordAudio;
                    break;
                case "MapAudio":
                    returnValue = ContentEditType.MapAudio;
                    break;
                case "AddMedia":
                    returnValue = ContentEditType.AddMedia;
                    break;
                case "EditMediaRuns":
                    returnValue = ContentEditType.EditMediaRuns;
                    break;
                case "EditAnnotations":
                    returnValue = ContentEditType.EditAnnotations;
                    break;
                case "EditSentenceRuns":
                    returnValue = ContentEditType.EditSentenceRuns;
                    break;
                case "EditWordRuns":
                    returnValue = ContentEditType.EditWordRuns;
                    break;
                case "EditAlignment":
                    returnValue = ContentEditType.EditAlignment;
                    break;
                default:
                    returnValue = ContentEditType.Display;
                    break;
            }

            return returnValue;
        }

        public static string[] CopyPasteStrings =
        {
            "Before",
            "Replace",
            "After",
            "Under",
            "Prepend",
            "All",
            "Append"
        };

        public static CopyPasteType GetCopyPasteType(string copyPasteType)
        {
            CopyPasteType returnValue;

            switch (copyPasteType)
            {
                case "Before":
                    returnValue = CopyPasteType.Before;
                    break;
                case "Replace":
                    returnValue = CopyPasteType.Replace;
                    break;
                case "After":
                    returnValue = CopyPasteType.After;
                    break;
                case "Under":
                    returnValue = CopyPasteType.Under;
                    break;
                case "Prepend":
                    returnValue = CopyPasteType.Prepend;
                    break;
                case "All":
                    returnValue = CopyPasteType.All;
                    break;
                case "Append":
                case null:
                case "":
                default:
                    returnValue = CopyPasteType.Append;
                    break;
            }

            return returnValue;
        }

        public static EditMediaOperation GetEditMediaOperation(string editOperation)
        {
            EditMediaOperation returnValue;

            switch (editOperation)
            {
                case null:
                case "":
                case "None":
                    returnValue = EditMediaOperation.None;
                    break;
                case "Cut":
                    returnValue = EditMediaOperation.Cut;
                    break;
                case "Copy":
                    returnValue = EditMediaOperation.Copy;
                    break;
                case "Paste":
                    returnValue = EditMediaOperation.Paste;
                    break;
                case "Clear":
                    returnValue = EditMediaOperation.Clear;
                    break;
                case "Delete":
                    returnValue = EditMediaOperation.Delete;
                    break;
                case "Crop":
                    returnValue = EditMediaOperation.Crop;
                    break;
                case "Undo":
                    returnValue = EditMediaOperation.Undo;
                    break;
                case "Redo":
                    returnValue = EditMediaOperation.Redo;
                    break;
                case "Deselect":
                    returnValue = EditMediaOperation.Deselect;
                    break;
                case "Save":
                    returnValue = EditMediaOperation.Save;
                    break;
                case "Cancel":
                    returnValue = EditMediaOperation.Cancel;
                    break;
                case "Import":
                    returnValue = EditMediaOperation.Import;
                    break;
                case "Export":
                    returnValue = EditMediaOperation.Export;
                    break;
                case "UpdateAudio":
                    returnValue = EditMediaOperation.UpdateAudio;
                    break;
                case "DeleteMedia":
                    returnValue = EditMediaOperation.DeleteMedia;
                    break;
                case "Record":
                    returnValue = EditMediaOperation.Record;
                    break;
                case "PostRecorded":
                    returnValue = EditMediaOperation.PostRecorded;
                    break;
                case "RecordShutDown":
                    returnValue = EditMediaOperation.RecordShutDown;
                    break;
                default:
                    returnValue = EditMediaOperation.None;
                    break;
            }

            return returnValue;
        }

        public static bool ShowStudyButton(
            ContentClassType classType,
            string contentType,
            string contentSubType)
        {
            if (classType != ContentClassType.StudyList)
                return false;

            switch (contentSubType)
            {
                case "Vocabulary":
                case "Text":
                case "Transcript":
                    return true;
                case "Dialog":
                    switch (contentType)
                    {
                        case "Vocabulary":
                        case "Text":
                        case "Transcript":
                            return true;
                        default:
                            return false;
                    }
                default:
                    return false;
            }
        }
    }
}
