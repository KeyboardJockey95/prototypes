using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Language
{
    public partial class LanguageTool : BaseObjectLanguage
    {
        public bool MatchAudioToStudyItems(
            string mediaPath,
            List<MultiLanguageItem> studyItems,
            string mediaItemKey,
            string languageMediaItemKey,
            UserRecord userRecord,
            AudioMapperStatistics statistics,
            out string errorMessage)
        {
            AudioMapper audioMapper = new AudioMapper(this, userRecord, statistics);

            bool returnValue = audioMapper.MatchAudioToStudyItems(
                mediaPath,
                studyItems,
                mediaItemKey,
                languageMediaItemKey);

            errorMessage = audioMapper.Error;

            return returnValue;
        }

        public bool MatchSpeechToTextToStudyItems(
            WaveAudioBuffer audioWavData,
            List<MultiLanguageItem> studyItems,
            int studyItemStartIndex,
            int sentenceStartIndex,
            LanguageID languageID,
            string matchedText,
            List<TextRun> matchedWordRuns,
            string mediaItemKey,
            string languageMediaItemKey,
            UserRecord userRecord,
            AudioMapperStatistics statistics,
            out string errorMessage)
        {
            AudioMapper audioMapper = new AudioMapper(this, userRecord, statistics);

            bool returnValue = audioMapper.MatchSpeechToTextToStudyItems(
                audioWavData,
                studyItems,
                mediaItemKey,
                languageMediaItemKey,
                matchedText,
                matchedWordRuns);

            errorMessage = audioMapper.Error;

            return returnValue;
        }

        public bool MatchAudioToStudyItemsOld(
            string mediaPath,
            List<MultiLanguageItem> studyItems,
            LanguageID languageID,
            string mediaItemKey,
            string languageMediaItemKey,
            string userName,
            out string errorMessage)
        {
            errorMessage = null;

            if (String.IsNullOrEmpty(mediaPath))
            {
                errorMessage = "No media file path.";
                return false;
            }

            if (!FileSingleton.Exists(mediaPath))
            {
                errorMessage = "Media file doesn't exist: " + mediaPath;
                return false;
            }

            WaveAudioBuffer audioWavData = MediaConvertSingleton.Mp3Decoding(mediaPath, out errorMessage);

            if (audioWavData == null)
                return false;

            List<LanguageID> languageIDs = new List<LanguageID>() { languageID };
            int startWaveIndex = 0;
            int endWaveIndex = audioWavData.SampleCount;
            int totalSampleCount = endWaveIndex - startWaveIndex;
            int sampleCount = endWaveIndex - startWaveIndex;
            TimeSpan mediaStart;
            TimeSpan mediaStop;
            string matchedText;
            List<TextRun> matchedWordRuns;
            int studyItemIndex = 0;
            int sentenceIndex = 0;
            int parsedWordIndex = 0;
            List<string> hints = MultiLanguageItem.GetUniqueWords(studyItems, languageIDs);
            string nodePathString = (studyItems.Count() != 0 ? studyItems[0].Node.GetNamePathString(null, "_") : String.Empty);
            string cacheKey = cacheKey = nodePathString + "_" + MediaUtilities.GetBaseFileName(mediaPath)
                + "-"
                + startWaveIndex.ToString()
                + "-"
                + endWaveIndex.ToString();

#if true
            string waveFilePath = MediaUtilities.ConcatenateFilePath(
                ApplicationData.TempPath,
                cacheKey + "_wave.wav");
            FileBuffer waveFileBuffer = new FileBuffer(waveFilePath, audioWavData.Storage);
#endif

            mediaStart = audioWavData.GetSampleTime(startWaveIndex);
            mediaStop = audioWavData.GetSampleTime(endWaveIndex);

            if (SpeechToTextSingleton.MapWaveText(
                audioWavData,
                mediaStart,
                mediaStop,
                languageIDs,
                hints,
                userName,
                cacheKey,
                out matchedText,
                out matchedWordRuns,
                out errorMessage))
            {
                if (!MatchSpeechToTextToStudyItemsOld(
                        audioWavData,
                        studyItems,
                        studyItemIndex,
                        sentenceIndex,
                        languageID,
                        matchedText,
                        matchedWordRuns,
                        mediaItemKey,
                        languageMediaItemKey,
                        out studyItemIndex,
                        out sentenceIndex,
                        out parsedWordIndex,
                        out errorMessage))
                {
                    return false;
                }
            }
            else
                return false;

            return true;
        }

        public static int DebugRowBreak = 25;

        public int MappedSentenceCount;
        public int CombinedSentenceScorePoint75;
        public int CombinedSentenceScorePoint25;
        public int SentenceMatchLastWordCount;
        public int SentenceNotMatchLastWordCount;
        public int RequiredResetCount;
        public int IgnoredSentenceCount;
        public int SaveMappedSentenceCount;
        private int SaveCombinedSentenceScorePoint75;
        private int SaveCombinedSentenceScorePoint25;
        public int SaveSentenceMatchLastWordCount;
        public int SaveSentenceNotMatchLastWordCount;
        private int SaveRequiredResetCount;
        private int SaveIgnoredSentenceCount;

        private static int ExtraSentenceLength = 3;

        public bool MatchSpeechToTextToStudyItemsOld(
            WaveAudioBuffer audioWavData,
            List<MultiLanguageItem> studyItems,
            int studyItemStartIndex,
            int sentenceStartIndex,
            LanguageID languageID,
            string parsedText,
            List<TextRun> parsedTextRuns,
            string mediaItemKey,
            string languageMediaItemKey,
            out int studyItemNextIndex,
            out int sentenceNextIndex,
            out int parsedNextIndex,
            out string errorMessage)
        {
            List<SentenceRun> studySentenceRuns;
            List<WordRun> studyWordRuns;
            SoftwareTimer timer = new Helpers.SoftwareTimer();

            timer.Start();

            studyItemNextIndex = studyItemStartIndex;
            sentenceNextIndex = sentenceStartIndex;
            parsedNextIndex = 0;
            errorMessage = null;

            SaveMappedSentenceCount = MappedSentenceCount;
            SaveCombinedSentenceScorePoint75 = CombinedSentenceScorePoint75;
            SaveCombinedSentenceScorePoint25 = CombinedSentenceScorePoint25;
            SaveSentenceMatchLastWordCount = SentenceMatchLastWordCount;
            SaveSentenceNotMatchLastWordCount = SentenceNotMatchLastWordCount;
            SaveRequiredResetCount = RequiredResetCount;
            SaveIgnoredSentenceCount = IgnoredSentenceCount;

            if (!SentenceRun.GetStudySentencesAndWordRuns(
                    studyItems,
                    languageID,
                    mediaItemKey,
                    languageMediaItemKey,
                    studyItemStartIndex,
                    sentenceStartIndex,
                    out studySentenceRuns,
                    out studyWordRuns))
                return false;

            int studySentenceRunIndex;
            int studySentenceRunCount = studySentenceRuns.Count();
            List<WordRun> parsedWordRuns = WordRun.GetParsedRuns(parsedText, parsedTextRuns);

            if (studySentenceRunCount == 0)
                return false;

            string studyText = SentenceRun.GetCombinedText(studySentenceRuns);

            // Standardize word run text.
            FixupContent(
                studyText,
                ref parsedText,
                studySentenceRuns,
                studyWordRuns,
                parsedWordRuns);

            DumpDebugWordMappingResultsBegin();

            List<SentenceRun> parsedSentenceRuns = new List<SentenceRun>();
            int parsedSentenceRunIndex = 0;
            int parsedWordRunStartIndex = 0;
            int studyWordRunCount = studyWordRuns.Count();
            int parsedWordRunCount = parsedWordRuns.Count();
            int studyWordIndex = 0;
            double roundingOffset = 0.5;

            for (studySentenceRunIndex = 0; studySentenceRunIndex < studySentenceRunCount; studySentenceRunIndex++, parsedSentenceRunIndex++)
            {
                SentenceRun studySentenceRun = studySentenceRuns[studySentenceRunIndex];
                SentenceRun studySentenceRunNext = (studySentenceRunIndex + 1 < studySentenceRunCount ?
                    studySentenceRuns[studySentenceRunIndex + 1] : null);

                if ((DebugRowBreak != -1) && (DebugRowBreak == studySentenceRunIndex + 2))
                    ApplicationData.Global.PutConsoleMessage("Row " + DebugRowBreak);

                // Do any abbreviation and number substitutions in parsed word runs segment.
                FixupSentence(
                    studySentenceRun,
                    studySentenceRunNext,
                    studyWordRuns,
                    parsedWordRuns,
                    parsedWordRunStartIndex);

                // Number of parsed word runs may have changed after substitutions.
                parsedWordRunCount = parsedWordRuns.Count();

                int studyWordRunRemainderCount = studyWordRunCount - studyWordIndex;
                int parsedWordRunRemainderCount = parsedWordRunCount - parsedWordRunStartIndex;
                double parsedWordCountRatio = (double)parsedWordRunRemainderCount / studyWordRunRemainderCount;
                //double parsedWordCountRatio = (double)parsedWordRunCount / studyWordRunCount;

                if (parsedWordCountRatio > 1.25)
                    parsedWordCountRatio = 1.25;
                else if (parsedWordCountRatio < .80)
                    parsedWordCountRatio = .80;

                //ApplicationData.Global.PutConsoleMessage("parsedWordCountRatio = " + parsedWordCountRatio.ToString() +
                //    "  " + studySentenceRun.Label);

                SentenceRun extraStudySentenceRun = null;
                int extraStudySentenceRunIndex = studySentenceRunIndex + 1;

                if (extraStudySentenceRunIndex < studySentenceRunCount)
                {
                    extraStudySentenceRun = new SentenceRun(studySentenceRuns[extraStudySentenceRunIndex]);

                    int nextWordCount =
                        (extraStudySentenceRun.SentenceWordCount > ExtraSentenceLength ? ExtraSentenceLength : extraStudySentenceRun.SentenceWordCount);
                    int remainderWordCount = studyWordRuns.Count() - studySentenceRun.StopWordIndexInParent;
                    nextWordCount = (remainderWordCount >= nextWordCount ? nextWordCount : remainderWordCount);

                    extraStudySentenceRun.UpdateStopIndex(extraStudySentenceRun.StartWordIndexInParent + nextWordCount);
                }

                bool isLastSentence = (studySentenceRunIndex == studySentenceRunCount - 1);
                int parsedWordRunStopIndex;
                int originalWordCount = studySentenceRun.SentenceWordCount;

                // If last sentence...
                if (isLastSentence)
                    parsedWordRunStopIndex = parsedWordRunCount;
                else
                {
                    parsedWordRunStopIndex = (int)(parsedWordRunStartIndex +
                        ((originalWordCount * parsedWordCountRatio) + roundingOffset));

                    if (parsedWordRunStopIndex > parsedWordRunCount)
                        parsedWordRunStopIndex = parsedWordRunCount;
                }

                // Get initial candidate parsed sentence run.
                SentenceRun parsedSentenceRun = new SentenceRun(
                    parsedText,
                    parsedWordRuns,
                    parsedWordRunStartIndex,
                    parsedWordRunStopIndex,
                    parsedSentenceRunIndex);
                SentenceRun nextParsedSentenceRun;

                GetCandidateScore(
                    studySentenceRun,
                    extraStudySentenceRun,
                    parsedSentenceRun,
                    out nextParsedSentenceRun);

                if (parsedSentenceRun.CombinedScore < 1.0)
                    parsedSentenceRun = GetBestParsedSentenceRun(
                        studySentenceRun,
                        extraStudySentenceRun,
                        parsedSentenceRun,
                        nextParsedSentenceRun,
                        parsedWordRunCount);
                else
                {
                    DumpDebugWordMappingCandidateState(
                        "############### Not doing best candidate search. " + (isLastSentence ? "Last sentence." : "Combined score was 1."),
                        "",
                        0,
                        studySentenceRun,
                        parsedSentenceRun,
                        extraStudySentenceRun,
                        nextParsedSentenceRun);
                }

                double combinedScore = parsedSentenceRun.CombinedScore;
                double mainScore = parsedSentenceRun.Score;

                studySentenceRun.MainScorePriorToResync = mainScore;
                studySentenceRun.BestParsedSentencePriorToResync = parsedSentenceRun.SentenceText;

                bool needResync = false;

                if (mainScore < 0.4)
                    needResync = true;

                //if ((mainScore < 0.338) && (originalWordCount <= 5))
                //    needResync = true;
                //else if ((mainScore < 0.338) && (originalWordCount > 5))
                //    needResync = true;

                // If the score is bad, we may be lost, so we try to get back on track.
                if (needResync)
                {
                    int parsedWordRunCandidateIndex = 0;
                    double bestMainScore = 0.0;
                    double bestMainFactoredScore = 0.0;
                    int bestParsedStartIndex = -1;
                    int bestParsedStopIndex = -1;
                    WordRun firstStudyWordRun = studySentenceRun.GetWordRun(0);

                    DumpDebugWordMappingResultsLabel("############### Entering resync. mainScore was " + mainScore.ToString());

                    for (; parsedWordRunCandidateIndex < parsedWordRunCount; parsedWordRunCandidateIndex++)
                    {
                        // Do any abbreviation and number substitutions in parsed word runs segment.
                        FixupSentence(
                            studySentenceRun,
                            studySentenceRunNext,
                            studyWordRuns,
                            parsedWordRuns,
                            parsedWordRunStartIndex);

                        // Number of parsed word runs may have changed after substitutions.
                        parsedWordRunCount = parsedWordRuns.Count();
                        parsedWordCountRatio = 1.0;

                        parsedWordRunStopIndex = (int)(parsedWordRunCandidateIndex +
                            ((originalWordCount * parsedWordCountRatio) + roundingOffset));

                        if (parsedWordRunStopIndex > parsedWordRunCount)
                            parsedWordRunStopIndex = parsedWordRunCount;

                        if (parsedWordRunStopIndex == parsedWordRunCandidateIndex)
                            break;

                        SentenceRun parsedSentenceRunCandidate = new SentenceRun(
                            parsedText,
                            parsedWordRuns,
                            parsedWordRunCandidateIndex,
                            parsedWordRunStopIndex,
                            parsedSentenceRunIndex);

                        WordRun firstParsedWordRun = parsedSentenceRunCandidate.GetWordRun(0);
                        double firstWordScore = GetWordMatchScore(
                            firstStudyWordRun.Word,
                            firstParsedWordRun.Word,
                            0,
                            0,
                            originalWordCount,
                            parsedSentenceRunCandidate.SentenceWordCount,
                            1,
                            1,
                            0,
                            0,
                            studySentenceRun.SentenceTextLengthNoSeparators,
                            parsedSentenceRunCandidate.SentenceTextLengthNoSeparators);

                        double sentenceScore = GetMatchScore(studySentenceRun, parsedSentenceRunCandidate);
                        double firstWordAdjustedScore = ((sentenceScore * originalWordCount) + (firstWordScore * 3)) / (originalWordCount + 3);

                        double wordPositionFactor = 1 - Math.Abs(
                            ((double)studyWordIndex / studyWordRunCount) -
                                ((double)parsedSentenceRunCandidate.StartWordIndexInParent / parsedWordRunCount));

                        /*
                        string extraMessage = "firstWordScore = " + firstWordScore.ToString() +
                            ", firstWordAdjustedScore = " + firstWordAdjustedScore.ToString() +
                            ", wordPositionFactor = " + wordPositionFactor.ToString();
                        */

                        wordPositionFactor = wordPositionFactor * wordPositionFactor;

                        double mainFactoredScore = firstWordAdjustedScore * wordPositionFactor;

                        if (mainFactoredScore > bestMainScore)
                        {
                            bestMainScore = sentenceScore;
                            bestMainFactoredScore = mainFactoredScore;
                            bestParsedStartIndex = parsedWordRunCandidateIndex;
                            bestParsedStopIndex = parsedWordRunStopIndex;
                        }

                        /*
                        DumpDebugWordMappingCandidateState(
                            "Resync candidate - " + ((mainFactoredScore < bestMainScore) ? "not" : "") + " updating best",
                            extraMessage,
                            parsedWordRunCandidateIndex,
                            studySentenceRun,
                            parsedSentenceRunCandidate,
                            null,
                            null);
                        */
                    }

                    RequiredResetCount++;

                    // If we found a good match.
                    if (bestMainScore >= (2 * mainScore))
                    {
                        DumpDebugWordMappingResultsLabel("############### Found good match for resync. Redoing best candiate search.");

                        parsedWordRunStartIndex = bestParsedStartIndex;
                        parsedWordRunStopIndex = bestParsedStopIndex;

                        parsedSentenceRun = new SentenceRun(
                            parsedText,
                            parsedWordRuns,
                            parsedWordRunStartIndex,
                            parsedWordRunStopIndex,
                            parsedSentenceRunIndex);

                        GetCandidateScore(
                            studySentenceRun,
                            extraStudySentenceRun,
                            parsedSentenceRun,
                            out nextParsedSentenceRun);

                        if (!isLastSentence && parsedSentenceRun.CombinedScore < 1.0)
                            parsedSentenceRun = GetBestParsedSentenceRun(
                                studySentenceRun,
                                extraStudySentenceRun,
                                parsedSentenceRun,
                                nextParsedSentenceRun,
                                parsedWordRunCount);

                        combinedScore = parsedSentenceRun.CombinedScore;
                        studySentenceRun.RequiredReset = true;
                        parsedSentenceRun.RequiredReset = true;
                    }
                    else if (mainScore < 0.4)
                    //else if (mainScore < 0.25)
                    {
                        // Else ignore sentence.
                        parsedSentenceRun = new SentenceRun(
                            parsedText,
                            parsedWordRuns,
                            parsedWordRunStartIndex,
                            parsedWordRunStartIndex,
                            parsedSentenceRunIndex);
                        studySentenceRun.SetUnmatched();
                        combinedScore = 0.0;
                        IgnoredSentenceCount++;
                        parsedSentenceRuns.Add(parsedSentenceRun);
                        studyWordIndex += originalWordCount;

                        DumpDebugWordMappingCandidateState(
                            "Best score less than 0.25. Ignoring sentence.",
                            "",
                            bestParsedStartIndex,
                            studySentenceRun,
                            parsedSentenceRun,
                            extraStudySentenceRun,
                            nextParsedSentenceRun);

                        MappedSentenceCount++;
                        continue;
                    }
                    else
                    {
                        DumpDebugWordMappingCandidateState(
                            "Didn't find good resync match. using previous best match.",
                            "",
                            bestParsedStartIndex,
                            studySentenceRun,
                            parsedSentenceRun,
                            extraStudySentenceRun,
                            nextParsedSentenceRun);
                    }
                }

                if (combinedScore < 0.75)
                    CombinedSentenceScorePoint75++;

                if (combinedScore < 0.25)
                    CombinedSentenceScorePoint25++;

                parsedSentenceRuns.Add(parsedSentenceRun);
                parsedWordRunStartIndex = parsedSentenceRun.StopWordIndexInParent;
                studyWordIndex += originalWordCount;

                // Count sentences mapped, whether matched or ignored.
                MappedSentenceCount++;

                int parsedSentenceWordCount;

                // Count number of sentences with matching last word.
                if ((parsedSentenceRun != null) && ((parsedSentenceWordCount = parsedSentenceRun.SentenceWordCount) != 0))
                {
                    WordRun studyLastWordRun = studySentenceRun.LastWordRunPossiblyCombined;
                    WordRun parsedLastWordRun = parsedSentenceRun.LastWordRunPossiblyCombined;

                    if ((studyLastWordRun != null) && (parsedLastWordRun != null))
                    {
                        if (studyLastWordRun.Matched &&
                                parsedLastWordRun.Matched &&
                                (studyLastWordRun.WordIndexOther == parsedLastWordRun.WordIndex))
                            SentenceMatchLastWordCount++;
                        else
                            SentenceNotMatchLastWordCount++;
                    }
                    else
                        SentenceNotMatchLastWordCount++;
                }
                else
                    SentenceNotMatchLastWordCount++;
            }

            // Take the timing info from the study and parsed sentence runs and set up
            // media runs in the study items.
            SetUpMediaRuns(
                studySentenceRuns,
                parsedSentenceRuns,
                studyWordRuns,
                parsedWordRuns,
                mediaItemKey,
                languageMediaItemKey,
                audioWavData,
                ref errorMessage);

            if (studySentenceRuns.Count() > parsedSentenceRuns.Count())
            {
                SentenceRun studySentenceRun = studySentenceRuns[parsedSentenceRuns.Count()];
                studyItemNextIndex = studySentenceRun.StudyItemIndex;
                sentenceNextIndex = studySentenceRun.SentenceIndex;
            }
            else
            {
                studyItemNextIndex = parsedWordRuns.Count();
                sentenceNextIndex = 0;
            }

            parsedNextIndex = parsedSentenceRuns.Count();

            MultiLanguageItem studyItem = (studyItems.Count() != 0 ? studyItems[0] : null);
            BaseObjectNode node = (studyItem != null ? studyItem.Node : null);
            string key = (node != null ? node.GetNamePathString(LanguageLookup.English, "_") : mediaItemKey + "_" + languageMediaItemKey).Replace(' ', '_');

            timer.Stop();
            double operationTime = timer.GetTimeInSeconds();

            DumpMappingResults(
                studySentenceRuns,
                parsedSentenceRuns,
                languageID,
                mediaItemKey,
                languageMediaItemKey,
                key,
                operationTime);

            DumpWordMappingResults(
                studySentenceRuns,
                parsedSentenceRuns,
                key);

            DumpDebugWordMappingResultsEnd(key);

            return true;
        }

        public SentenceRun GetBestParsedSentenceRun(
                SentenceRun studySentenceRun,
                SentenceRun nextStudySentenceRun,
                SentenceRun parsedSentenceRun,
                SentenceRun nextParsedSentenceRun,
                int parsedWordCount)
        {
            int parsedCandidateStartIndex = parsedSentenceRun.StartWordIndexInParent;
            int parsedCandidateEndIndex;
            int highIndex = parsedWordCount;
            int lowIndex = parsedCandidateStartIndex;
            int bestParsedCandidateEndIndex = parsedSentenceRun.StopWordIndexInParent;
            double bestMainScore = parsedSentenceRun.Score;
            double bestCombinedScore = parsedSentenceRun.CombinedScore;
            SentenceRun parsedSentenceRunCandidate = new SentenceRun(parsedSentenceRun);
            SentenceRun nextParsedSentenceRunCandidate = null;

            if (highIndex > parsedSentenceRun.ParentWordCount)
                highIndex = parsedSentenceRun.ParentWordCount;

            DumpDebugWordMappingResultsLabel("############### Finding best sentence candidate:");

            DumpDebugWordMappingCandidateState(
                "Initial candidate",
                "",
                0,
                studySentenceRun,
                parsedSentenceRunCandidate,
                nextStudySentenceRun,
                nextParsedSentenceRun);

            if (parsedSentenceRun.Score == 1.0)
            {
                DumpDebugWordMappingResultsLabel("Exiting early because main score is 1.");
                return parsedSentenceRun;
            }

            for (int loop = 0; loop < 2; loop++)
            {
                int delta = (loop == 0 ? 1 : -1);
                int offset = delta;
                double lastMainScore = parsedSentenceRun.Score;
                double lastCombinedScore = parsedSentenceRun.CombinedScore;
                int worseCount = 0;

                for (parsedCandidateEndIndex = parsedSentenceRun.StopWordIndexInParent + delta;
                    (parsedCandidateEndIndex > lowIndex) && (parsedCandidateEndIndex < highIndex);
                    parsedCandidateEndIndex += delta, offset += delta)
                {
                    parsedSentenceRunCandidate.UpdateStopIndex(parsedCandidateEndIndex);

                    double combinedScore = GetCandidateScore(
                        studySentenceRun,
                        nextStudySentenceRun,
                        parsedSentenceRunCandidate,
                        out nextParsedSentenceRunCandidate);
                    double mainScore = studySentenceRun.Score;

                    if (mainScore == 1.0)
                    {
                        bestMainScore = mainScore;
                        bestCombinedScore = combinedScore;
                        bestParsedCandidateEndIndex = parsedCandidateEndIndex;

                        DumpDebugWordMappingCandidateState(
                            "Main score was 1.0",
                            "",
                            offset,
                            studySentenceRun,
                            parsedSentenceRunCandidate,
                            nextStudySentenceRun,
                            nextParsedSentenceRun);

                        DumpDebugWordMappingResultsLabel("Exiting inner loop, MainScore was 1.");

                        break;
                    }

                    if (combinedScore > bestCombinedScore)
                    {
                        bestMainScore = mainScore;
                        bestCombinedScore = combinedScore;
                        bestParsedCandidateEndIndex = parsedCandidateEndIndex;

                        DumpDebugWordMappingCandidateState(
                            "New best candidate",
                            "",
                            offset,
                            studySentenceRun,
                            parsedSentenceRunCandidate,
                            nextStudySentenceRun,
                            nextParsedSentenceRun);

                        worseCount = 0;
                    }
                    else if (combinedScore == bestCombinedScore)
                    {
                        DumpDebugWordMappingCandidateState(
                            "Equal to best candidate",
                            "",
                            offset,
                            studySentenceRun,
                            parsedSentenceRunCandidate,
                            nextStudySentenceRun,
                            nextParsedSentenceRun);

                        worseCount = 0;
                    }
                    else
                    {
                        DumpDebugWordMappingCandidateState(
                            "Not a better candidate",
                            "",
                            offset,
                            studySentenceRun,
                            parsedSentenceRunCandidate,
                            nextStudySentenceRun,
                            nextParsedSentenceRun);
                    }

                    if (combinedScore == 1.0)
                    {
                        DumpDebugWordMappingResultsLabel("Exiting inner loop, CombinedScore=" + combinedScore.ToString() + ", MainScore=" + studySentenceRun.Score.ToString());
                        break;
                    }

                    if ((mainScore < lastMainScore) && (combinedScore < lastCombinedScore))
                    {
                        worseCount++;

                        if (worseCount == 3)
                        {
                            DumpDebugWordMappingResultsLabel("Exiting inner loop, both scores less than last. WorseCount=" + worseCount.ToString() +
                                " CombinedScore=" + combinedScore.ToString() + ", MainScore=" + mainScore.ToString() +
                                " LastCombinedScore=" + lastCombinedScore.ToString() + ", LastMainScore=" + lastMainScore.ToString());
                            break;
                        }
                        else
                        {
                            DumpDebugWordMappingResultsLabel("Both scores worse. WorseCount=" + worseCount.ToString() +
                                " CombinedScore=" + combinedScore.ToString() + ", MainScore=" + mainScore.ToString() +
                                " LastCombinedScore=" + lastCombinedScore.ToString() + ", LastMainScore=" + lastMainScore.ToString());
                        }
                    }

                    lastMainScore = mainScore;
                    lastCombinedScore = combinedScore;
                }

                if ((lastMainScore == 1.0) || (lastCombinedScore == 1.0))
                    break;
            }

            parsedSentenceRun.UpdateStopIndex(bestParsedCandidateEndIndex);

            bestCombinedScore = GetCandidateScore(
                studySentenceRun,
                nextStudySentenceRun,
                parsedSentenceRun,
                out nextParsedSentenceRunCandidate);

            DumpDebugWordMappingCandidateState(
                "CHOSEN CANDIDATE",
                "",
                bestParsedCandidateEndIndex - parsedSentenceRun.StopWordIndexInParent,
                studySentenceRun,
                parsedSentenceRun,
                nextStudySentenceRun,
                nextParsedSentenceRunCandidate);

            return parsedSentenceRun;
        }

        protected double GetCandidateScore(
            SentenceRun studySentenceRun,
            SentenceRun extendedStudySentenceRun,
            SentenceRun parsedSentenceRun,
            out SentenceRun extendedParsedSentenceRun)
        {
            extendedParsedSentenceRun = null;

            if ((studySentenceRun.SentenceWordCount == 0) && (extendedStudySentenceRun.SentenceWordCount == 0))
            {
                studySentenceRun.Score = 1.0;
                parsedSentenceRun.Score = 1.0;
                studySentenceRun.CombinedScore = 1.0;
                parsedSentenceRun.CombinedScore = 1.0;
                return 1.0;
            }

            double sentenceMatchScore = GetMatchScore(
                studySentenceRun,
                parsedSentenceRun);

            studySentenceRun.Score = sentenceMatchScore;
            parsedSentenceRun.Score = sentenceMatchScore;
            studySentenceRun.CombinedScore = sentenceMatchScore;
            parsedSentenceRun.CombinedScore = sentenceMatchScore;

            studySentenceRun.ExtraSentenceRun = extendedStudySentenceRun;
            parsedSentenceRun.ExtraSentenceRun = null;

            if (extendedStudySentenceRun != null)
            {
                int originalExtraCharCount = extendedStudySentenceRun.SentenceTextLengthNoSeparators;
                int parsedExtraCharCount = 0;
                int extraParsedWordRunStartIndex = parsedSentenceRun.StopWordIndexInParent;
                int extraWordIndex = 0;
                int extraWordIndexEnd = parsedSentenceRun.ParentWordCount - extraParsedWordRunStartIndex;
                List<WordRun> parsedRuns = parsedSentenceRun.ParentWordRuns;
                WordRun extraParsedWordRun = null;

                for (extraWordIndex = 0; extraWordIndex < extraWordIndexEnd; extraWordIndex++)
                {
                    extraParsedWordRun = parsedRuns[extraParsedWordRunStartIndex + extraWordIndex];

                    parsedExtraCharCount += extraParsedWordRun.Length;

                    if (parsedExtraCharCount >= originalExtraCharCount)
                        break;
                }

                int parsedExtraEndIndex = extraParsedWordRunStartIndex + extraWordIndex + 1;

                if (parsedExtraEndIndex > parsedSentenceRun.ParentWordCount)
                    parsedExtraEndIndex = parsedSentenceRun.ParentWordCount;

                extendedParsedSentenceRun = new SentenceRun(
                    parsedSentenceRun.ParentText,
                    parsedSentenceRun.ParentWordRuns,
                    parsedSentenceRun.StopWordIndexInParent,
                    parsedExtraEndIndex,
                    parsedSentenceRun.ParentSentenceIndex + 1);

                if (parsedExtraCharCount > originalExtraCharCount)
                {
                    int wordLength = extraParsedWordRun.Length - (parsedExtraCharCount - originalExtraCharCount);

                    if (wordLength > extraParsedWordRun.Word.Length)
                        wordLength = extraParsedWordRun.Word.Length;

                    int wordStartIndex = extraParsedWordRun.StartIndex;
                    int wordStopIndex = wordStartIndex + wordLength;
                    string word = extraParsedWordRun.Word.Substring(0, wordLength);
                    int textStartIndexNoSeparators = extraParsedWordRun.StartIndexNoSeparators;
                    int textStopIndexNoSeparators = textStartIndexNoSeparators + wordLength;
                    extraParsedWordRun = new WordRun(
                        word,
                        wordStartIndex,
                        wordStopIndex,
                        textStartIndexNoSeparators,
                        textStopIndexNoSeparators,
                        extraParsedWordRun.StartTime,
                        extraParsedWordRun.StopTime,
                        extraParsedWordRun.StudyItemIndex,
                        extraParsedWordRun.SentenceIndex,
                        extraParsedWordRun.WordIndex,
                        -1,
                        false,
                        false);
                    extendedParsedSentenceRun.SetLocalWordRun(extraWordIndex, extraParsedWordRun);
                    extendedParsedSentenceRun.SentenceTextStopIndex = extraParsedWordRun.StopIndex;
                    extendedParsedSentenceRun.SentenceText = extendedParsedSentenceRun.ParentText.Substring(
                        extendedParsedSentenceRun.SentenceTextStartIndex,
                        extendedParsedSentenceRun.SentenceTextLength);
                    extendedParsedSentenceRun.UpdateSentenceLengthNoSeparators();
                }

                int wordsInOriginalSentence = studySentenceRun.SentenceWordCount;
                double nextSentenceStartMatchScore = GetMatchScore(
                    extendedStudySentenceRun,
                    extendedParsedSentenceRun);
                int wordsInNextSentenceStart = extendedStudySentenceRun.SentenceWordCount;

                var combinedMatchScore =
                    ((sentenceMatchScore * wordsInOriginalSentence)
                        + (nextSentenceStartMatchScore * wordsInNextSentenceStart * 1.25)) /
                    ((double)wordsInOriginalSentence + (wordsInNextSentenceStart * 1.25));

                extendedStudySentenceRun.Score = nextSentenceStartMatchScore;
                extendedParsedSentenceRun.Score = nextSentenceStartMatchScore;
                studySentenceRun.CombinedScore = combinedMatchScore;
                parsedSentenceRun.CombinedScore = combinedMatchScore;
                studySentenceRun.ExtraSentenceRun = extendedStudySentenceRun;
                parsedSentenceRun.ExtraSentenceRun = extendedParsedSentenceRun;
                return combinedMatchScore;
            }

            return sentenceMatchScore;
        }

        protected double GetMatchScore(
            SentenceRun originalSentenceRun,
            SentenceRun parsedSentenceRun)
        {
            int matchedCount = 0;
            int originalUnmatchedCount = 0;
            int parsedUnmatchedCount = 0;
            double sumOfOrignalWordMatchScores;
            double sumOfParsedWordMatchScores;
            GetWordMatchScores(
                originalSentenceRun,
                parsedSentenceRun,
                out matchedCount,
                out originalUnmatchedCount,
                out parsedUnmatchedCount,
                out sumOfOrignalWordMatchScores,
                out sumOfParsedWordMatchScores);
            int originalWordCount = originalSentenceRun.FinalSentenceWordCount;
            int parsedWordCount = parsedSentenceRun.FinalSentenceWordCount;
            double sentenceMatchScore =
                (sumOfOrignalWordMatchScores + 1) /
                    (originalWordCount + 1 + sumOfParsedWordMatchScores/4);
            return sentenceMatchScore;
        }

        protected static int MaxWordGroupSize = 3;

        protected double GetWordMatchScores(
            SentenceRun originalSentenceRun,
            SentenceRun parsedSentenceRun,
            out int matchedCount,
            out int originalUnmatchedCount,
            out int parsedUnmatchedCount,
            out double sumOfBestOriginalWordMatchScores,
            out double sumOfBestParsedWordMatchScores)
        {
            int originalWordCount = originalSentenceRun.SentenceWordCount;
            int originalWordIndex;
            WordRun originalWordRun = null;
            int parsedWordCount = parsedSentenceRun.SentenceWordCount;
            int parsedWordIndex;
            WordRun parsedWordRun = null;

            matchedCount = 0;
            originalUnmatchedCount = 0;
            parsedUnmatchedCount = 0;
            sumOfBestOriginalWordMatchScores = 0.0;
            sumOfBestParsedWordMatchScores = 0.0;

            originalSentenceRun.ClearWordFlags();
            parsedSentenceRun.ClearWordFlags();

            // Get single match scores.
            for (originalWordIndex = 0;
                originalWordIndex < originalWordCount;
                originalWordIndex++)
            {
                originalWordRun = originalSentenceRun.GetWordRun(originalWordIndex);
                string originalWord = originalSentenceRun.GetWordSpanNoSpace(originalWordIndex, 1);
                string parsedWord;
                double bestEditDistanceScore = 0.0;
                int bestWordIndexParsed = -1;
                bool wasMatched = false;

                for (parsedWordIndex = 0;
                    parsedWordIndex < parsedWordCount;
                    parsedWordIndex++)
                {
                    parsedWordRun = parsedSentenceRun.GetWordRun(parsedWordIndex);
                    parsedWord = parsedSentenceRun.GetWordSpanNoSpace(parsedWordIndex, 1);
                    double editDistanceScore = GetEditDistanceScore(
                        originalWord,
                        parsedWord);

                    if (editDistanceScore > bestEditDistanceScore)
                    {
                        bestEditDistanceScore = editDistanceScore;
                        bestWordIndexParsed = parsedWordIndex;
                        wasMatched = true;
                    }
                }

                if (wasMatched)
                {
                    parsedWordRun = parsedSentenceRun.GetWordRun(bestWordIndexParsed);
                    parsedWord = parsedWordRun.Word;

                    GetAndRecordSingleWordEditDistanceScore(
                        originalWord,
                        parsedWord,
                        originalWordIndex,
                        bestWordIndexParsed,
                        originalWordRun,
                        parsedWordRun);
                }
            }

            // Get possibly combined word match scores and find best.
            for (originalWordIndex = 0;
                originalWordIndex < originalWordCount;
                originalWordIndex += (originalWordRun.WordSpanCount != 0 ? originalWordRun.WordSpanCount : 1))
            {
                originalWordRun = originalSentenceRun.GetWordRun(originalWordIndex);
                string originalWord;
                string parsedWord;
                double bestWordMatchScore = 0.0;
                int bestWordLengthOriginal = 0;
                int bestWordLengthParsed = 0;
                int bestWordIndexParsed = -1;
                int wordIndexLimitOriginal = MaxWordGroupSize;
                bool wasMatched = false;

                if (originalWordIndex + wordIndexLimitOriginal > originalWordCount)
                    wordIndexLimitOriginal = originalWordCount - originalWordIndex;

                for (parsedWordIndex = 0;
                    parsedWordIndex < parsedWordCount;
                    parsedWordIndex++)
                {
                    parsedWordRun = parsedSentenceRun.GetWordRun(parsedWordIndex);
                    parsedWord = parsedWordRun.Word;
                    originalWord = originalWordRun.Word;

                    int originalWordLength;
                    int parsedWordLength;
                    double editDistanceScore;
                    double bestEditDistanceScore = GetEditDistanceScore(
                            originalWord,
                            parsedWord);
                    int bestLocalWordLengthOriginal = 1;
                    int bestLocalWordLengthParsed = 1;

                    if (bestEditDistanceScore != 1.0)
                    {
                        int wordIndexLimitParsed = MaxWordGroupSize;

                        if (parsedWordIndex + wordIndexLimitParsed > parsedWordCount)
                            wordIndexLimitParsed = parsedWordCount - parsedWordIndex;

                        //if ((originalWord == "traduccion") && (parsedWord == "traduccion"))
                        //    ApplicationData.Global.PutConsoleMessage("traduccion");

                        for (originalWordLength = 2, parsedWordLength = 1; originalWordLength <= wordIndexLimitOriginal; originalWordLength++)
                        {
                            string combinedOriginalWordCandidate = originalWordRun.GetCombinedWord(originalWordLength, originalSentenceRun);
                            editDistanceScore = GetEditDistanceScore(
                                combinedOriginalWordCandidate,
                                parsedWord);

                            if (editDistanceScore == 0.0)
                                continue;

                            if (editDistanceScore >= bestEditDistanceScore)
                            {
                                bool betterThanSingle = true;

                                for (int i = 0; i < originalWordLength; i++)
                                {
                                    WordRun individualWordRun = originalSentenceRun.GetWordRun(originalWordIndex + i);

                                    if (individualWordRun.SingleWordEditDistanceScore > editDistanceScore)
                                        betterThanSingle = false;
                                }

                                if (betterThanSingle)
                                {
                                    bestEditDistanceScore = editDistanceScore;
                                    bestLocalWordLengthOriginal = originalWordLength;
                                    bestLocalWordLengthParsed = parsedWordLength;
                                }
                            }
                        }

                        originalWord = originalWordRun.Word;

                        // We already did originalWordLength = 1 and parsedWordLength = 1.
                        for (originalWordLength = 1, parsedWordLength = 2; parsedWordLength <= wordIndexLimitParsed; parsedWordLength++)
                        {
                            parsedWord = parsedWordRun.GetCombinedWord(parsedWordLength, parsedSentenceRun);

                            editDistanceScore = GetEditDistanceScore(
                                originalWord,
                                parsedWord);

                            if (editDistanceScore == 0.0)
                                continue;

                            if (editDistanceScore >= bestEditDistanceScore)
                            {
                                bestEditDistanceScore = editDistanceScore;
                                bestLocalWordLengthOriginal = originalWordLength;
                                bestLocalWordLengthParsed = parsedWordLength;
                            }
                        }
                    }

                    originalWord = originalWordRun.GetCombinedWord(bestLocalWordLengthOriginal, originalSentenceRun);
                    parsedWord = parsedWordRun.GetCombinedWord(bestLocalWordLengthParsed, parsedSentenceRun);

                    double wordMatchScore = GetWordMatchScore(
                        originalWord,
                        parsedWord,
                        originalWordIndex,
                        parsedWordIndex,
                        originalWordCount,
                        parsedWordCount,
                        bestLocalWordLengthOriginal,
                        bestLocalWordLengthParsed,
                        originalWordRun.StartIndexNoSeparators,
                        parsedWordRun.StartIndexNoSeparators,
                        originalSentenceRun.SentenceTextLengthNoSeparators,
                        parsedSentenceRun.SentenceTextLengthNoSeparators);

                    if (wordMatchScore > bestWordMatchScore)
                    {
                        bestWordMatchScore = wordMatchScore;
                        bestWordLengthOriginal = bestLocalWordLengthOriginal;
                        bestWordLengthParsed = bestLocalWordLengthParsed;
                        bestWordIndexParsed = parsedWordIndex;
                        wasMatched = true;
                    }
                }

                if (wasMatched)
                {
                    parsedWordRun = parsedSentenceRun.GetWordRun(bestWordIndexParsed);

                    for (int i = 1; i < bestWordLengthOriginal; i++)
                    {
                        WordRun wordRun = originalSentenceRun.GetWordRun(originalWordIndex + i);
                        wordRun.Used = true;
                        wordRun.Matched = true;
                        wordRun.WordSpanCount = 0;
                        wordRun.WordMatchScore = 0.0;
                        wordRun.WordIndexOther = -1;
                        wordRun.OtherWordRun = null;
                    }

                    for (int i = 1; i < bestWordLengthParsed; i++)
                    {
                        WordRun wordRun = parsedSentenceRun.GetWordRun(bestWordIndexParsed + i);
                        wordRun.Used = true;
                        wordRun.Matched = true;
                        wordRun.WordSpanCount = 0;
                        wordRun.WordMatchScore = 0.0;
                        wordRun.WordIndexOther = -1;
                        wordRun.OtherWordRun = null;
                    }

                    parsedWordRun = parsedSentenceRun.GetWordRun(bestWordIndexParsed);

                    originalWord = originalWordRun.GetCombinedWord(bestWordLengthOriginal, originalSentenceRun);
                    parsedWord = parsedWordRun.GetCombinedWord(bestWordLengthParsed, parsedSentenceRun);

                    GetAndRecordMatchedWordScore(
                        originalWord,
                        parsedWord,
                        originalWordIndex,
                        bestWordIndexParsed,
                        originalWordCount,
                        parsedWordCount,
                        bestWordLengthOriginal,
                        bestWordLengthParsed,
                        originalWordRun.StartIndexNoSeparators,
                        parsedWordRun.StartIndexNoSeparators,
                        originalSentenceRun.SentenceTextLengthNoSeparators,
                        parsedSentenceRun.SentenceTextLengthNoSeparators,
                        originalWordRun,
                        parsedWordRun);
                }
            }

            int finalWordCountOriginal = 0;
            int finalWordCountParsed = 0;
            List<WordRun> wordRunsOriginal = new List<WordRun>();
            List<WordRun> wordRunsParsed = new List<WordRun>();

            // First clear the parsed word runs.
            // We are later going to replace the parsed runs with those saved in the original word runs.
            for (parsedWordIndex = 0; parsedWordIndex < parsedWordCount; parsedWordIndex++)
            {
                parsedWordRun = parsedSentenceRun.GetWordRun(parsedWordIndex);
                parsedWordRun.ClearDataFields();
            }

            // Copy parsed saved word runs from original.
            // For parsed word runs that have been combined, clear the span count and other stuff.
            for (originalWordIndex = 0; originalWordIndex < originalWordCount; originalWordIndex++)
            {
                originalWordRun = originalSentenceRun.GetWordRun(originalWordIndex);
                parsedWordRun = originalWordRun.OtherWordRun;

                if (parsedWordRun != null)
                {
                    parsedWordIndex = originalWordRun.WordIndexOther;
                    parsedSentenceRun.SetLocalWordRun(parsedWordIndex, parsedWordRun);

                    for (int i = 1; i < parsedWordRun.WordSpanCount; i++)
                    {
                        WordRun wordRun = parsedSentenceRun.GetWordRun(parsedWordIndex + i);
                        wordRun.Used = true;
                        wordRun.Matched = true;
                        wordRun.WordSpanCount = 0;
                        wordRun.WordMatchScore = 0.0;
                    }
                }
            }

            // Get parsed sentence word count and set new indexes. Populate new list.
            for (parsedWordIndex = 0; parsedWordIndex < parsedWordCount; parsedWordIndex++)
            {
                parsedWordRun = parsedSentenceRun.GetWordRun(parsedWordIndex);
                parsedWordRun.WordIndex = finalWordCountParsed;

                if (parsedWordRun.WordSpanCount != 0)
                {
                    wordRunsParsed.Add(parsedWordRun);
                    finalWordCountParsed++;
                }
            }

            // Get ofiginal sentence word count and set new indexes. Populate new list.
            for (originalWordIndex = 0; originalWordIndex < originalWordCount; originalWordIndex++)
            {
                originalWordRun = originalSentenceRun.GetWordRun(originalWordIndex);
                originalWordRun.WordIndex = finalWordCountOriginal;

                if (originalWordRun.WordSpanCount != 0)
                {
                    wordRunsOriginal.Add(originalWordRun);
                    finalWordCountOriginal++;

                    for (int i = 1; i < originalWordRun.WordSpanCount; i++)
                    {
                        WordRun wordRun = originalSentenceRun.GetWordRun(originalWordIndex + i);
                        wordRun.Used = true;
                        wordRun.Matched = true;
                        wordRun.WordSpanCount = 0;
                        wordRun.WordMatchScore = 0.0;
                    }
                }
            }

            // Save final sentence word counts.
            originalSentenceRun.FinalSentenceWordCount = finalWordCountOriginal;
            parsedSentenceRun.FinalSentenceWordCount = finalWordCountParsed;

            // Collect sum, matched and unmatched counts, and unmatched words for original sentence.
            for (originalWordIndex = 0; originalWordIndex < finalWordCountOriginal; originalWordIndex++)
            {
                originalWordRun = wordRunsOriginal[originalWordIndex];

                if (originalWordRun.Matched)
                {
                    parsedWordRun = originalWordRun.OtherWordRun;
                    parsedWordIndex = parsedWordRun.WordIndex;

                    GetWordMatchScoreFinal(
                        originalWordRun,
                        parsedWordRun,
                        originalWordIndex,
                        parsedWordIndex,
                        finalWordCountOriginal,
                        finalWordCountParsed,
                        originalSentenceRun.SentenceTextLengthNoSeparators,
                        parsedSentenceRun.SentenceTextLengthNoSeparators);

                    matchedCount++;

                    if (originalWordRun.WordSpanCount != 0)
                        sumOfBestOriginalWordMatchScores += originalWordRun.WordMatchScore;

                    if (originalWordRun.WordMatchScore < 0.49)
                    {
                        if (originalSentenceRun.UnmatchedWords == null)
                            originalSentenceRun.UnmatchedWords = new List<string>();

                        originalSentenceRun.UnmatchedWords.Add(originalWordRun.Word);
                    }
                }
                else
                {
                    originalUnmatchedCount++;

                    if (originalSentenceRun.UnmatchedWords == null)
                        originalSentenceRun.UnmatchedWords = new List<string>();

                    originalSentenceRun.UnmatchedWords.Add(originalWordRun.Word);
                }

                originalWordRun.SentenceWordCount = originalSentenceRun.SentenceWordCount;
                originalWordRun.FinalSentenceWordCount = originalSentenceRun.FinalSentenceWordCount;
            }

            // Collect sum, matched and unmatched counts, and unmatched words for parsed sentence.
            for (parsedWordIndex = 0; parsedWordIndex < finalWordCountParsed; parsedWordIndex++)
            {
                parsedWordRun = wordRunsParsed[parsedWordIndex]; ;

                if (!parsedWordRun.Matched)
                {
                    parsedUnmatchedCount++;

                    if (parsedSentenceRun.UnmatchedWords == null)
                        parsedSentenceRun.UnmatchedWords = new List<string>();

                    parsedSentenceRun.UnmatchedWords.Add(parsedWordRun.Word);
                }

                parsedWordRun.SentenceWordCount = parsedSentenceRun.SentenceWordCount;
                parsedWordRun.FinalSentenceWordCount = parsedSentenceRun.FinalSentenceWordCount;

                if (parsedWordRun.WordSpanCount != 0)
                    sumOfBestParsedWordMatchScores += (1.0 - parsedWordRun.WordMatchScore);
            }

            originalSentenceRun.MatchedWordCount = matchedCount;
            originalSentenceRun.UnmatchedWordCount = originalUnmatchedCount;
            parsedSentenceRun.MatchedWordCount = matchedCount;
            parsedSentenceRun.UnmatchedWordCount = parsedUnmatchedCount;

            return sumOfBestParsedWordMatchScores;
        }

        // Word inputs already standardized.
        public double GetEditDistanceScore(
            string originalWord,
            string parsedWord)
        {
            if (originalWord == null)
                throw new Exception("GetEditDistanceScore: originalWord is null.");

            if (parsedWord == null)
                throw new Exception("GetEditDistanceScore: parsedWord is null.");

            int originalWordLength = originalWord.Length;
            double editDistance = TextUtilities.ComputeDistance(originalWord, parsedWord);

            double editDistanceFactor = ((double)originalWordLength + 1.0 - editDistance) / (originalWordLength + 1);

            if (editDistanceFactor < 0.0)
                editDistanceFactor = 0.0;

            return editDistanceFactor;
        }

        protected void GetAndRecordSingleWordEditDistanceScore(
            string originalWord,
            string parsedWord,
            int wordPositionInOriginal,
            int wordPositionInParsed,
            WordRun originalWordRun,
            WordRun parsedWordRun)
        {
            double editDistanceScore = GetEditDistanceScore(originalWord, parsedWord);

            originalWordRun.EditDistanceScore = editDistanceScore;
            originalWordRun.SingleWordEditDistanceScore = editDistanceScore;
            originalWordRun.WordSpanCount = 1;
            originalWordRun.MatchedWord = originalWord;
            originalWordRun.WordIndexOther = wordPositionInParsed;
            //originalWordRun.Used = true;
            //originalWordRun.Matched = true;

            parsedWordRun.EditDistanceScore = editDistanceScore;
            parsedWordRun.SingleWordEditDistanceScore = editDistanceScore;
            parsedWordRun.WordSpanCount = 1;
            parsedWordRun.MatchedWord = parsedWord;
            parsedWordRun.WordIndexOther = wordPositionInOriginal;
            parsedWordRun.OtherWordRun = new WordRun(originalWordRun);
            //parsedWordRun.Used = true;
            //parsedWordRun.Matched = true;

            originalWordRun.OtherWordRun = new WordRun(parsedWordRun);
        }

        protected void GetWordMatchScoreFinal(
            WordRun originalWordRun,
            WordRun parsedWordRun,
            int wordPositionInOriginal,
            int wordPositionInParsed,
            int sentenceWordCountInOriginal,
            int sentenceWordCountInParsed,
            int sentenceCharacterCountInOriginal,
            int sentenceCharacterCountInParsed)
        {
            int editDistance;
            double editDistanceFactor;
            double wordPositionFactor;
            double wordMatchScore;

            GetWordMatchScore(
                originalWordRun.MatchedWord,
                parsedWordRun.MatchedWord,
                wordPositionInOriginal,
                wordPositionInParsed,
                sentenceWordCountInOriginal,
                sentenceWordCountInParsed,
                1,      // Combined word is now one word.
                1,      // "
                originalWordRun.StartIndexNoSeparators,
                parsedWordRun.StartIndexNoSeparators,
                sentenceCharacterCountInOriginal,
                sentenceCharacterCountInParsed,
                out editDistance,
                out editDistanceFactor,
                out wordPositionFactor,
                out wordMatchScore);


            originalWordRun.EditDistance = editDistance;
            originalWordRun.EditDistanceFactor = editDistanceFactor;
            originalWordRun.WordPositionFactor = wordPositionFactor;
            originalWordRun.WordMatchScore = wordMatchScore;
            originalWordRun.WordIndexOther = wordPositionInParsed;
            originalWordRun.OtherWordRun = parsedWordRun;

            parsedWordRun.EditDistance = editDistance;
            parsedWordRun.EditDistanceFactor = editDistanceFactor;
            parsedWordRun.WordPositionFactor = wordPositionFactor;
            parsedWordRun.WordMatchScore = wordMatchScore;
            parsedWordRun.WordIndexOther = wordPositionInOriginal;
            parsedWordRun.OtherWordRun = originalWordRun;
        }

        protected double GetWordMatchScore(
            string originalWord,
            string parsedWord,
            int wordPositionInOriginal,
            int wordPositionInParsed,
            int wordCountInOriginal,
            int wordCountInParsed,
            int originalWordLength,
            int parsedWordLength,
            int characterPositionInOriginal,
            int characterPositionInParsed,
            int characterCountInOriginal,
            int characterCountInParsed)
        {
            int editDistance;
            double editDistanceFactor;
            double wordPositionFactor;
            double wordMatchScore;

            GetWordMatchScore(
                originalWord,
                parsedWord,
                wordPositionInOriginal,
                wordPositionInParsed,
                wordCountInOriginal,
                wordCountInParsed,
                originalWordLength,
                parsedWordLength,
                characterPositionInOriginal,
                characterPositionInParsed,
                characterCountInOriginal,
                characterCountInParsed,
                out editDistance,
                out editDistanceFactor,
                out wordPositionFactor,
                out wordMatchScore);

            return wordMatchScore;
        }

        protected void GetWordMatchScore(
            string originalWord,
            string parsedWord,
            int wordPositionInOriginal,
            int wordPositionInParsed,
            int wordCountInOriginal,
            int wordCountInParsed,
            int combinedOriginalWordLength,
            int combinedParsedWordLength,
            int characterPositionInOriginal,
            int characterPositionInParsed,
            int characterCountInOriginal,
            int characterCountInParsed,
            out int editDistance,
            out double editDistanceFactor,
            out double wordPositionFactor,
            out double wordMatchScore)
        {
            if (originalWord == null)
                throw new Exception("GetEditDistanceScore: originalWord is null.");

            if (parsedWord == null)
                throw new Exception("GetEditDistanceScore: parsedWord is null.");

            int originalWordLength = originalWord.Length;
            editDistance = TextUtilities.ComputeDistance(originalWord, parsedWord);

            //editDistanceFactor = (double)(originalWordLength - editDistance) / originalWordLength;
            editDistanceFactor = ((double)originalWordLength + 1.0 - editDistance) / (originalWordLength + 1);

            if (editDistanceFactor < 0.0)
                editDistanceFactor = 0.0;

            wordPositionFactor = 1 - Math.Abs(((double)characterPositionInOriginal / characterCountInOriginal) - ((double)characterPositionInParsed / characterCountInParsed));

            wordPositionFactor = wordPositionFactor * wordPositionFactor;

            editDistanceFactor = editDistanceFactor * editDistanceFactor;

            wordMatchScore = editDistanceFactor * wordPositionFactor;

            if (wordMatchScore < 0.0)
                ApplicationData.Global.PutConsoleMessage("Word score lower than 0: " + originalWord);
        }

        protected void GetAndRecordMatchedWordScore(
            string originalWord,
            string parsedWord,
            int wordPositionInOriginal,
            int wordPositionInParsed,
            int wordCountInOriginal,
            int wordCountInParsed,
            int originalWordLength,
            int parsedWordLength,
            int characterPositionInOriginal,
            int characterPositionInParsed,
            int characterCountInOriginal,
            int characterCountInParsed,
            WordRun originalWordRun,
            WordRun parsedWordRun)
        {
            int editDistance;
            double editDistanceFactor;
            double wordPositionFactor;
            double wordMatchScore;

            GetWordMatchScore(
                originalWord,
                parsedWord,
                wordPositionInOriginal,
                wordPositionInParsed,
                wordCountInOriginal,
                wordCountInParsed,
                originalWordLength,
                parsedWordLength,
                characterPositionInOriginal,
                characterPositionInParsed,
                characterCountInOriginal,
                characterCountInParsed,
                out editDistance,
                out editDistanceFactor,
                out wordPositionFactor,
                out wordMatchScore);

            originalWordRun.WordMatchScore = wordMatchScore;
            originalWordRun.WordSpanCount = originalWordLength;
            originalWordRun.WordIndexOther = wordPositionInParsed;
            originalWordRun.Used = true;
            originalWordRun.Matched = true;
            originalWordRun.MatchedWord = originalWord;
            originalWordRun.EditDistance = editDistance;
            originalWordRun.EditDistanceFactor = editDistanceFactor;
            originalWordRun.WordPositionFactor = wordPositionFactor;
            originalWordRun.WordMatchScore = wordMatchScore;
            originalWordRun.WordIndexOther = wordPositionInParsed;
            originalWordRun.StartTime = parsedWordRun.StartTime;
            originalWordRun.StopTime = parsedWordRun.StopTime;

            parsedWordRun.WordMatchScore = wordMatchScore;
            parsedWordRun.WordSpanCount = parsedWordLength;
            parsedWordRun.WordIndexOther = wordPositionInOriginal;
            parsedWordRun.Used = true;
            parsedWordRun.Matched = true;
            parsedWordRun.MatchedWord = parsedWord;
            parsedWordRun.EditDistance = editDistance;
            parsedWordRun.EditDistanceFactor = editDistanceFactor;
            parsedWordRun.WordPositionFactor = wordPositionFactor;
            parsedWordRun.WordMatchScore = wordMatchScore;
            parsedWordRun.WordIndexOther = wordPositionInOriginal;
            parsedWordRun.OtherWordRun = new WordRun(originalWordRun);

            originalWordRun.OtherWordRun = new WordRun(parsedWordRun);
        }

        public virtual string StandardizeWord(string text)
        {
            var normalizedString = text.ToLower();
            normalizedString = ApplicationData.Global.GetUnaccentedWord(normalizedString);
            normalizedString = TextUtilities.RemoveSpacesAndPunctuation(normalizedString);
            return normalizedString;
        }

        public string[] GetStandardizedWordArray(string text)
        {
            string[] parts = text.Split(LanguageLookup.Space);
            int c = parts.Length;
            int i;

            for (i = 0; i < c; i++)
                parts[i] = StandardizeWord(parts[i].ToLower());

            return parts;
        }

        private Dictionary<string, string[]> _AbbreviationWordRuns;

        public Dictionary<string, string[]> AbbreviationWordRuns
        {
            get
            {
                if (_AbbreviationWordRuns == null)
                {
                    _AbbreviationWordRuns = new Dictionary<string, string[]>();

                    Dictionary<string, string> abbrevsDict = AbbreviationDictionary;

                    foreach (KeyValuePair<string, string> kvp in abbrevsDict)
                        _AbbreviationWordRuns.Add(StandardizeWord(kvp.Key).ToLower(), GetStandardizedWordArray(kvp.Value));
                }

                return _AbbreviationWordRuns;
            }
        }

        // Returns true if substitution took place.
        protected bool WordSubstitutionCheck(
            SentenceRun originalSentenceRun,
            SentenceRun parsedSentenceRun,
            int originalWordIndex,
            int parsedWordIndex,
            ref string originalWord,
            ref string parsedWord,
            out int originalWordCount,
            out int parsedWordCount)
        {
            int numberWordCount;

            originalWordCount = 1;
            parsedWordCount = 1;

            if (IsNumberString(originalWord))
            {
                if (IsNumberString(parsedWord))
                    return false;

                if (MatchNumberString(originalWord, parsedSentenceRun, parsedWordIndex, out numberWordCount))
                {
                    parsedWordCount = numberWordCount;
                    parsedWord = originalWord;
                    return true;
                }
            }
            else if (IsNumberString(parsedWord))
            {
                if (MatchNumberString(parsedWord, originalSentenceRun, originalWordIndex, out numberWordCount))
                {
                    parsedWordCount = numberWordCount;
                    parsedWord = originalSentenceRun.GetWordSpan(originalWordIndex, numberWordCount);
                    return true;
                }
            }

#if true
            string[] expansion;

            if (AbbreviationWordRuns.TryGetValue(originalWord, out expansion))
            {
                int pe = parsedSentenceRun.GetMatchingWordSpanCount(parsedWordIndex, expansion);

                if ((pe != 0) && (pe == expansion.Length))
                {
                    originalWordCount = 1;
                    parsedWordCount = expansion.Length;
                    parsedWord = originalWord;
                    return true;
                }
            }
#endif

            return false;
        }

        public bool MatchNumberString(
            string numberWord,
            SentenceRun sentenceRun,
            int wordIndex,
            out int wordCount)
        {
            wordCount = 1;

            int patternNumberValue = ObjectUtilities.GetIntegerFromString(numberWord.Replace(",", ""), -1);

            if (patternNumberValue == -1)
                return false;

            string accumulatedNumberWord = String.Empty;
            int accumulatedNumber = 0;
            int sentenceLength = sentenceRun.SentenceWordCount;

            for (int i = 0; i < sentenceLength; i++)
            {
                string word = sentenceRun.GetWord(wordIndex + i);
                string normalizedWord = NormalizeNumberWord(word);

                if ((normalizedWord == word) && !IsNumberString(word))
                    return false;

                int numberValue = ObjectUtilities.GetIntegerFromString(normalizedWord, -1);

                if (numberValue == -1)
                    return false;

                if (numberValue <= 9)
                    accumulatedNumber += numberValue;
                else if (accumulatedNumber != 0)
                    accumulatedNumber *= numberValue;
                else
                    accumulatedNumber += numberValue;

                if (accumulatedNumber == patternNumberValue)
                    return true;
                else if (accumulatedNumber > patternNumberValue)
                    return false;
            }

            return false;
        }

        public static int AmplitudeThreshold = 1;
        public static TimeSpan SilenceWidthThreshold = new TimeSpan(0, 0, 0, 0, 200);
        public static TimeSpan ExtensionTime = new TimeSpan(0, 0, 0, 1, 0);

        protected bool GetBestSilence(
            WaveAudioBuffer audioWavData,
            TimeSpan lowTimeLimit,
            TimeSpan highTimeLimit,
            out TimeSpan silenceStart,
            out TimeSpan silenceStop)
        {
            bool returnValue = FindBreak(
                audioWavData,
                lowTimeLimit,
                highTimeLimit,
                out silenceStart,
                out silenceStop);

            if (!returnValue)
            {
                TimeSpan newLowTimeLimit = lowTimeLimit;
                TimeSpan newHighTimeLimit = highTimeLimit + ExtensionTime;

                if (lowTimeLimit > ExtensionTime)
                    newLowTimeLimit = lowTimeLimit - ExtensionTime;
                else
                    newLowTimeLimit = TimeSpan.Zero;

                TimeSpan duration = audioWavData.Duration;

                if (newHighTimeLimit > duration)
                    newHighTimeLimit = duration;

                returnValue = FindBreak(
                    audioWavData,
                    newLowTimeLimit,
                    newHighTimeLimit,
                    out silenceStart,
                    out silenceStop);

#if true
                if (!returnValue)
                {
                    newHighTimeLimit = newHighTimeLimit + ExtensionTime;

                    if (lowTimeLimit > ExtensionTime)
                        newLowTimeLimit = newLowTimeLimit - ExtensionTime;
                    else
                        newLowTimeLimit = TimeSpan.Zero;

                    if (newHighTimeLimit > duration)
                        newHighTimeLimit = duration;

                    returnValue = FindBreak(
                        audioWavData,
                        newLowTimeLimit,
                        newHighTimeLimit,
                        out silenceStart,
                        out silenceStop);
                }
#endif
            }

            return returnValue;
        }

        protected bool FindBreak(
            WaveAudioBuffer audioWavData,
            TimeSpan startTime,
            TimeSpan endTime,
            out TimeSpan silenceStart,
            out TimeSpan silenceStop)
        {
            int sampleIndexA;
            int sampleIndexB;
            int silenceStartIndex;
            int silenceStopIndex;
            int sentenceSampleIndex;
            int maxSampleIndex = audioWavData.SampleCount;
            TimeSpan startTimeA;
            TimeSpan startTimeB;
            int silenceWidth = audioWavData.GetSampleIndexFromTime(SilenceWidthThreshold);

            startTimeA = startTime;
            startTimeB = endTime;

            sampleIndexA = audioWavData.GetSampleIndexFromTime(startTimeA);
            sampleIndexB = audioWavData.GetSampleIndexFromTime(startTimeB);

            if (sampleIndexB > maxSampleIndex)
                sampleIndexB = maxSampleIndex;

            if (audioWavData.FindLongestSilence(
                sampleIndexA,
                sampleIndexB,
                AmplitudeThreshold,
                silenceWidth,
                out silenceStartIndex,
                out silenceStopIndex))
            {
                sentenceSampleIndex = (silenceStartIndex + silenceStopIndex) / 2;
                silenceStart = audioWavData.GetSampleTime(sentenceSampleIndex);
                silenceStop = audioWavData.GetSampleTime(silenceStopIndex);
                return true;
            }
            else
            {
                silenceStart = TimeSpan.Zero;
                silenceStop = TimeSpan.Zero;
                return false;
            }
        }

        // Default search length.
        protected static TimeSpan DefaultSearchLengthTime = new TimeSpan(0, 0, 0, 1, 0);
        protected static TimeSpan DefaultSearchLengthTimeTripled = new TimeSpan(0, 0, 0, 3, 0);
        protected static TimeSpan DefaultSearchLengthTimeStrict = new TimeSpan(0, 0, 0, 0, 100);

        // 50 msec;
        protected static TimeSpan DefaultLeadTime = new TimeSpan(0, 0, 0, 0, 50);

        // 50 msec;
        protected static TimeSpan MinimumLeadTime = new TimeSpan(0, 0, 0, 0, 50);

        protected TimeSpan GetLeadTime(TimeSpan silenceWidth)
        {
            if (silenceWidth < MinimumLeadTime)
                return new TimeSpan(silenceWidth.Ticks / 3);

            return DefaultLeadTime;
        }

        protected bool FixupContent(
            string studyText,
            ref string parsedText,
            List<SentenceRun> studySentenceRuns,
            List<WordRun> studyWordRuns,
            List<WordRun> parsedWordRuns)
        {
            foreach (WordRun wordRun in studyWordRuns)
                wordRun.Word = StandardizeWord(wordRun.Word);

            foreach (WordRun wordRun in parsedWordRuns)
                wordRun.Word = StandardizeWord(wordRun.Word);

            return true;
        }

        protected bool FixupSentence(
            SentenceRun studySentenceRun,
            SentenceRun studySentenceRunNext,
            List<WordRun> studyWordRuns,
            List<WordRun> parsedWordRuns,
            int parsedWordIndexStart)
        {
            Dictionary<string, string[]> abbrevsDict = AbbreviationWordRuns;

            if (abbrevsDict == null)
                return false;

            bool returnValue = true;
            int studyWordCount = studySentenceRun.SentenceWordCount;
            int studyWordIndex;
            int parsedWordIndex = parsedWordIndexStart;
            int parsedWordCount = parsedWordRuns.Count();

            if (studySentenceRunNext != null)
                studyWordCount += studySentenceRunNext.SentenceWordCount;

            List<WordRun> studySentenceWordRuns = studyWordRuns.GetRange(studySentenceRun.StartWordIndexInParent, studyWordCount);

            int parsedWordLimit = parsedWordIndexStart + (studyWordCount * 2);

            for (studyWordIndex = 0; studyWordIndex < studyWordCount; studyWordIndex++)
            {
                WordRun studyWordRun = studySentenceWordRuns[studyWordIndex];
                string studyWord = studyWordRun.Word;
                string[] expansion;
                int matchedParsedIndex;
                int matchedParsedLength;

                if (abbrevsDict.TryGetValue(studyWord, out expansion))
                {
                    string[] abbrevArray = new string[1] { studyWord };

                    if (FindExpansionMatch(
                        parsedWordRuns,
                        parsedWordIndexStart,
                        parsedWordLimit,
                        abbrevArray,
                        expansion,
                        out matchedParsedIndex))
                    {
                        WordRun.ReplaceWordRuns(
                            parsedWordRuns,
                            matchedParsedIndex,
                            expansion.Length,
                            studyWord);
                        parsedWordIndexStart = matchedParsedIndex + 1;
                    }
                }
                else if (IsNumberString(studyWord))
                {
                    if (FindNumberMatch(
                        parsedWordRuns,
                        parsedWordIndexStart,
                        parsedWordLimit,
                        studyWord,
                        out matchedParsedIndex,
                        out matchedParsedLength))
                    {
                        WordRun.ReplaceWordRuns(
                            parsedWordRuns,
                            matchedParsedIndex,
                            matchedParsedLength,
                            studyWord);
                        parsedWordIndexStart = matchedParsedIndex + 1;
                    }
                }
                else if (IsExpandedNumberString(studyWord))
                {
                    int tempStudyCount;
                    string[] digitsArray;
                    string[] expandedArray;

                    if (CollectAndConvertExpandedNumberWords(
                        studySentenceWordRuns,
                        studyWordIndex,
                        studyWordCount,
                        out tempStudyCount,
                        out expandedArray,
                        out digitsArray))
                    {
                        if (FindExpansionMatch(
                            parsedWordRuns,
                            parsedWordIndexStart,
                            parsedWordLimit,
                            expandedArray,
                            digitsArray,
                            out matchedParsedIndex))
                        {
                            WordRun.ReplaceWordRunSpan(
                                parsedWordRuns,
                                matchedParsedIndex,
                                1,
                                studySentenceWordRuns,
                                studyWordIndex,
                                tempStudyCount);
                            parsedWordIndexStart = matchedParsedIndex + 1;
                            studyWordIndex = studyWordIndex + tempStudyCount - 1;
                        }
                    }
                }
            }

            return returnValue;
        }

        protected bool FindExpansionMatch(
            List<WordRun> wordRuns,
            int wordIndexStart,
            int wordLimit,
            string[] original,
            string[] expansion,
            out int matchedWordIndex)
        {
            int wordIndex;

            matchedWordIndex = -1;

            for (wordIndex = wordIndexStart; wordIndex < wordLimit; wordIndex++)
            {
                int index = wordIndex;
                int endIndex = wordIndex + expansion.Length;

                if (index >= wordRuns.Count())
                    break;

                if (endIndex > wordRuns.Count())
                    break;

                WordRun wordRun = wordRuns[index];
                int matchCount;

                if (wordRun.Word == StandardizeWord(original[0]))
                {
                    matchCount = 1;

                    for (int i = 1; i < original.Length; i++)
                    {
                        wordRun = wordRuns[index + i];

                        if (wordRun.Word == StandardizeWord(original[i]))
                            matchCount++;
                        else
                            break;
                    }

                    if (matchCount == original.Length)
                    {
                        matchedWordIndex = index;
                        return false;
                    }
                }
                else if (wordRun.Word == StandardizeWord(expansion[0]))
                {
                    matchCount = 1;

                    for (int i = 1; i < expansion.Length; i++)
                    {
                        wordRun = wordRuns[index + i];

                        if (wordRun.Word == StandardizeWord(expansion[i]))
                            matchCount++;
                        else
                            break;
                    }

                    if (matchCount == expansion.Length)
                    {
                        matchedWordIndex = index;
                        return true;
                    }
                }
            }

            return false;
        }

        protected bool FindNumberMatch(
            List<WordRun> wordRuns,
            int wordIndexStart,
            int wordLimit,
            string numberString,
            out int matchedWordIndex,
            out int matchedWordLength)
        {
            int wordIndex;

            matchedWordIndex = -1;
            matchedWordLength = 0;

            List<string[]> expansions = GetNumberExpansions(numberString);
            int bestIndex = -1;
            int bestLength = 0;

            for (wordIndex = wordIndexStart; wordIndex < wordLimit; wordIndex++)
            {
                foreach (string[] expansion in expansions)
                {
                    int index = wordIndex;
                    int endIndex = wordIndexStart + wordIndex + expansion.Length;

                    if (index >= wordRuns.Count())
                        break;

                    if (endIndex > wordRuns.Count())
                        break;

                    WordRun wordRun = wordRuns[index];
                    int matchCount = 0;

                    if (StandardizeWord(wordRun.Word) == StandardizeWord(expansion[0]))
                    {
                        matchCount++;

                        for (int i = 1; i < expansion.Length; i++)
                        {
                            wordRun = wordRuns[index + i];

                            if (StandardizeWord(wordRun.Word) == StandardizeWord(expansion[i]))
                                matchCount++;
                            else
                                break;
                        }

                        if (matchCount == expansion.Length)
                        {
                            if (expansion.Length > bestLength)
                            {
                                bestIndex = index;
                                bestLength = expansion.Length;
                            }

                            break;
                        }
                    }
                }
            }

            if (bestLength > 0)
            {
                matchedWordIndex = bestIndex;
                matchedWordLength = bestLength;
                return true;
            }

            return false;
        }

        protected bool CollectAndConvertExpandedNumberWords(
            List<WordRun> wordRuns,
            int wordIndexStart,
            int wordIndexLimit,
            out int matchedCount,
            out string[] expansionArray,
            out string[] matchedDigits)
        {
            int wordIndex;
            List<string> expansionList = new List<string>();
            List<string> expandedDigits = null;
            string lastWord = null;

            matchedCount = 0;
            expansionArray = null;
            matchedDigits = null;

            for (wordIndex = wordIndexStart; wordIndex < wordIndexLimit; wordIndex++)
            {
                WordRun wordRun = wordRuns[wordIndex];
                string word = wordRun.Word;

                if (IsExpandedNumberString(word))
                {
                    if (expandedDigits == null)
                        expandedDigits = new List<string>();

                    expandedDigits.Add(word);
                }
                else if ((wordIndex != 0) && IsExpandedNumberStringConnector(lastWord, word))
                {
                    if (expandedDigits == null)
                        expandedDigits = new List<string>();

                    expandedDigits.Add(word);
                }
                else
                    break;

                lastWord = word;
            }

            if ((expandedDigits != null) && (expandedDigits.Count() > 1)
                    && IsExpandedNumberStringConnector(expandedDigits.Last()))
                expandedDigits.RemoveAt(expandedDigits.Count() - 1);

            if (expandedDigits != null)
            {
                string matchedNumberString = GetDigitsStringFromNumberExpansionList(expandedDigits);

                if (!String.IsNullOrEmpty(matchedNumberString))
                {
                    matchedCount = expandedDigits.Count();
                    expansionArray = expandedDigits.ToArray();
                    matchedDigits = new string[1] { matchedNumberString };
                    return true;
                }
            }

            return false;
        }

        // Take the timing info from the study and parsed sentence runs and set up
        // media runs in the study items.
        protected void SetUpMediaRuns(
            List<SentenceRun> studySentenceRuns,
            List<SentenceRun> parsedSentenceRuns,
            List<WordRun> studyWordRuns,
            List<WordRun> parsedWordRuns,
            string mediaItemKey,
            string languageMediaItemKey,
            WaveAudioBuffer audioWavData,
            ref string errorMessage)
        {
            SetupKnownSentenceRunTimes(
                studySentenceRuns,
                parsedSentenceRuns,
                studyWordRuns,
                parsedWordRuns,
                audioWavData,
                ref errorMessage);

            SetupMissingSentenceRunTimes(
                studySentenceRuns,
                studyWordRuns,
                audioWavData,
                ref errorMessage);

            AddMediaRuns(
                studySentenceRuns,
                mediaItemKey,
                languageMediaItemKey,
                ref errorMessage);
        }

        // Take the timing info from the study and parsed sentence runs and set up
        // media runs in the study items.
        protected void SetupKnownSentenceRunTimes(
            List<SentenceRun> studySentenceRuns,
            List<SentenceRun> parsedSentenceRuns,
            List<WordRun> studyWordRuns,
            List<WordRun> parsedWordRuns,
            WaveAudioBuffer audioWavData,
            ref string errorMessage)
        {
            int studySentenceRunCount = studySentenceRuns.Count();
            int studySentenceRunIndex;
            int parsedSentenceRunCount = parsedSentenceRuns.Count();
            TimeSpan mediaLength = audioWavData.TimeLength;

            // Now set up media runs. Study and parsed sentence runs should be in sync, though the counts may differ.
            for (studySentenceRunIndex = 0; studySentenceRunIndex < studySentenceRunCount; studySentenceRunIndex++)
            {
                SentenceRun studySentenceRun = studySentenceRuns[studySentenceRunIndex];

                // If no sentence run on the parsed side, delete media run and continue.
                if (studySentenceRunIndex >= parsedSentenceRunCount)
                    continue;

                SentenceRun parsedSentenceRun = parsedSentenceRuns[studySentenceRunIndex];

                if (parsedSentenceRun.SentenceWordCount == 0)
                    continue;

                int parsedWordRunStopIndex = parsedSentenceRun.StopWordIndexInParent;

                int nextStudySentenceRunIndex = studySentenceRunIndex + 1;
                SentenceRun nextStudySentenceRun = null;
                SentenceRun nextParsedSentenceRun = null;
                WordRun firstParsedWordRun = parsedSentenceRun.GetWordRun(0);
                WordRun lastParsedWordRun = parsedWordRuns[parsedWordRunStopIndex - 1];
                WordRun nextParsedWordRun = null;

                if (nextStudySentenceRunIndex < studySentenceRunCount)
                    nextStudySentenceRun = studySentenceRuns[nextStudySentenceRunIndex];

                if (nextStudySentenceRunIndex < parsedSentenceRunCount)
                {
                    nextParsedSentenceRun = parsedSentenceRuns[nextStudySentenceRunIndex];

                    if (nextParsedSentenceRun.StartWordIndexInParent < parsedWordRuns.Count())
                        nextParsedWordRun = parsedWordRuns[nextParsedSentenceRun.StartWordIndexInParent];
                }

                TimeSpan lowTimeLimit;
                TimeSpan highTimeLimit;
                TimeSpan silenceStart;
                TimeSpan silenceStop;

                // Do sentence start.

                highTimeLimit = firstParsedWordRun.MiddleTime;

                if (firstParsedWordRun.WordMatchScore > 0.9)
                    lowTimeLimit = firstParsedWordRun.StartTime - DefaultSearchLengthTimeStrict;
                else
                    lowTimeLimit = firstParsedWordRun.StartTime - DefaultSearchLengthTime;

                if (lowTimeLimit < TimeSpan.Zero)
                    lowTimeLimit = TimeSpan.Zero;

                if (GetBestSilence(
                        audioWavData,
                        lowTimeLimit,
                        highTimeLimit,
                        out silenceStart,
                        out silenceStop))
                {
                    studySentenceRun.SentenceStartTime = silenceStop - GetLeadTime(silenceStop - silenceStart);
                    studySentenceRun.SilenceStartFront = silenceStart;
                    studySentenceRun.SilenceStopFront = silenceStop;
                }
                else
                    studySentenceRun.SentenceStartTime = firstParsedWordRun.StartTime - DefaultLeadTime;

                // Do sentence stop.

                lowTimeLimit = lastParsedWordRun.MiddleTime;

                if (lastParsedWordRun.WordMatchScore > 0.9)
                    highTimeLimit = lastParsedWordRun.StopTime + DefaultSearchLengthTimeStrict;
                else
                    highTimeLimit = lastParsedWordRun.StopTime + DefaultSearchLengthTime;

                if (highTimeLimit > mediaLength)
                    highTimeLimit = mediaLength;

                if (GetBestSilence(
                    audioWavData,
                    lowTimeLimit,
                    highTimeLimit,
                    out silenceStart,
                    out silenceStop))
                {
                    studySentenceRun.SentenceStopTime = silenceStart + GetLeadTime(silenceStop - silenceStart);
                    studySentenceRun.SilenceStartBack = silenceStart;
                    studySentenceRun.SilenceStopBack = silenceStop;
                }
                else
                    studySentenceRun.SentenceStopTime = lastParsedWordRun.StopTime + DefaultLeadTime;

                // Validate times.
                if (studySentenceRun.SentenceStopTime < studySentenceRun.SentenceStartTime)
                {
                    // Redo using start and stop word times.

                    // Do sentence start.

                    highTimeLimit = firstParsedWordRun.StartTime;
                    lowTimeLimit = firstParsedWordRun.StartTime - DefaultSearchLengthTimeStrict;

                    if (lowTimeLimit < TimeSpan.Zero)
                        lowTimeLimit = TimeSpan.Zero;

                    if (GetBestSilence(
                            audioWavData,
                            lowTimeLimit,
                            highTimeLimit,
                            out silenceStart,
                            out silenceStop))
                    {
                        studySentenceRun.SentenceStartTime = silenceStop - GetLeadTime(silenceStop - silenceStart);
                        studySentenceRun.SilenceStartFront = silenceStart;
                        studySentenceRun.SilenceStopFront = silenceStop;
                    }
                    else
                        studySentenceRun.SentenceStartTime = firstParsedWordRun.StartTime - DefaultLeadTime;

                    // Do sentence stop.

                    lowTimeLimit = lastParsedWordRun.StopTime;
                    highTimeLimit = lastParsedWordRun.StopTime + DefaultSearchLengthTime;

                    if (highTimeLimit > mediaLength)
                        highTimeLimit = mediaLength;

                    if (GetBestSilence(
                        audioWavData,
                        lowTimeLimit,
                        highTimeLimit,
                        out silenceStart,
                        out silenceStop))
                    {
                        studySentenceRun.SentenceStopTime = silenceStart + GetLeadTime(silenceStop - silenceStart);
                        studySentenceRun.SilenceStartBack = silenceStart;
                        studySentenceRun.SilenceStopBack = silenceStop;
                    }
                    else
                        studySentenceRun.SentenceStopTime = lastParsedWordRun.StopTime + DefaultLeadTime;

                    if (studySentenceRun.SentenceStopTime < studySentenceRun.SentenceStartTime)
                    {
                        studySentenceRun.SentenceStartTime = firstParsedWordRun.StartTime;
                        studySentenceRun.SentenceStopTime = lastParsedWordRun.StopTime;
                        studySentenceRun.SilenceStartFront = TimeSpan.Zero;
                        studySentenceRun.SilenceStopFront = TimeSpan.Zero;
                        studySentenceRun.SilenceStartBack = TimeSpan.Zero;
                        studySentenceRun.SilenceStopBack = TimeSpan.Zero;

                        if (!String.IsNullOrEmpty(studySentenceRun.Error))
                            studySentenceRun.Error += "|";

                        studySentenceRun.Error += "Reset times to parsed word run times";
                    }
                }
            }
        }

        // Take the timing info from the study and parsed sentence runs and set up
        // media runs in the study items.
        protected void SetupMissingSentenceRunTimes(
            List<SentenceRun> studySentenceRuns,
            List<WordRun> studyWordRuns,
            WaveAudioBuffer audioWavData,
            ref string errorMessage)
        {
            int studySentenceRunCount = studySentenceRuns.Count();
            int studySentenceRunIndex;

            // Now set up media runs. Study and parsed sentence runs should be in sync, though the counts may differ.
            for (studySentenceRunIndex = 0; studySentenceRunIndex < studySentenceRunCount; studySentenceRunIndex++)
            {
                SentenceRun studySentenceRun = studySentenceRuns[studySentenceRunIndex];

                if (studySentenceRun.HasValidSentenceTimes())
                    continue;

                int previousStudySentenceRunIndex = studySentenceRunIndex - 1;
                int nextStudySentenceRunIndex = studySentenceRunIndex + 1;
                SentenceRun previousStudySentenceRun = null;
                SentenceRun nextStudySentenceRun = null;
                TimeSpan previousTime = TimeSpan.Zero;
                TimeSpan nextTime = TimeSpan.Zero;
                TimeSpan midTime = TimeSpan.Zero;
                TimeSpan lowTimeLimit;
                TimeSpan highTimeLimit;
                TimeSpan silenceStart;
                TimeSpan silenceStop;

                if ((previousStudySentenceRunIndex >= 0) && (previousStudySentenceRunIndex < studySentenceRunCount))
                {
                    previousStudySentenceRun = studySentenceRuns[previousStudySentenceRunIndex];
                    previousTime = previousStudySentenceRun.SentenceStopTime;
                }

                if (nextStudySentenceRunIndex < studySentenceRunCount)
                {
                    nextStudySentenceRun = studySentenceRuns[nextStudySentenceRunIndex];
                    nextTime = nextStudySentenceRun.SentenceStartTime;
                }

                if ((previousTime != TimeSpan.Zero) && (nextTime != TimeSpan.Zero))
                    midTime = new TimeSpan((previousTime.Ticks + nextTime.Ticks) / 2);

                if ((previousTime != TimeSpan.Zero) &&
                    (studySentenceRun.SentenceStartTime == TimeSpan.Zero))
                {
                    highTimeLimit = previousTime + DefaultSearchLengthTime;

                    if (GetBestSilence(
                            audioWavData,
                            lowTimeLimit,
                            highTimeLimit,
                            out silenceStart,
                            out silenceStop))
                    {
                        studySentenceRun.SentenceStartTime = silenceStop - GetLeadTime(silenceStop - silenceStart);
                        studySentenceRun.SilenceStartFront = silenceStart;
                        studySentenceRun.SilenceStopFront = silenceStop;
                    }
                    else
                    {
                        if (midTime != TimeSpan.Zero)
                            highTimeLimit = midTime;
                        else
                        {
                            highTimeLimit = previousTime + DefaultSearchLengthTimeTripled;

                            if ((nextTime != TimeSpan.Zero) && (highTimeLimit > nextTime))
                                highTimeLimit = TimeSpan.Zero;
                        }

                        lowTimeLimit = previousTime;

                        if (highTimeLimit != TimeSpan.Zero)
                        {
                            if (GetBestSilence(
                                    audioWavData,
                                    lowTimeLimit,
                                    highTimeLimit,
                                    out silenceStart,
                                    out silenceStop))
                            {
                                studySentenceRun.SentenceStartTime = silenceStop - GetLeadTime(silenceStop - silenceStart);
                                studySentenceRun.SilenceStartFront = silenceStart;
                                studySentenceRun.SilenceStopFront = silenceStop;
                            }
                        }
                    }
                }

                if ((nextTime != TimeSpan.Zero) &&
                    (studySentenceRun.SentenceStopTime == TimeSpan.Zero))
                {
                    lowTimeLimit = nextTime - DefaultSearchLengthTime;

                    if (GetBestSilence(
                            audioWavData,
                            lowTimeLimit,
                            highTimeLimit,
                            out silenceStart,
                            out silenceStop))
                    {
                        studySentenceRun.SentenceStartTime = silenceStop - GetLeadTime(silenceStop - silenceStart);
                        studySentenceRun.SilenceStartBack = silenceStart;
                        studySentenceRun.SilenceStopBack = silenceStop;
                    }
                    else
                    {
                        if (midTime != TimeSpan.Zero)
                            lowTimeLimit = midTime;
                        else
                        {
                            lowTimeLimit = nextTime - DefaultSearchLengthTime;

                            if ((previousTime != TimeSpan.Zero) && (lowTimeLimit < previousTime))
                                lowTimeLimit = TimeSpan.Zero;
                        }

                        highTimeLimit = nextTime;

                        if (lowTimeLimit != TimeSpan.Zero)
                        {
                            if (GetBestSilence(
                                    audioWavData,
                                    lowTimeLimit,
                                    highTimeLimit,
                                    out silenceStart,
                                    out silenceStop))
                            {
                                studySentenceRun.SentenceStartTime = silenceStop - GetLeadTime(silenceStop - silenceStart);
                                studySentenceRun.SilenceStartBack = silenceStart;
                                studySentenceRun.SilenceStopBack = silenceStop;
                            }
                        }
                    }
                }

                if (studySentenceRun.SentenceStopTime < studySentenceRun.SentenceStartTime)
                {
                    /*
                    studySentenceRun.SentenceStartTime = TimeSpan.Zero;
                    studySentenceRun.SentenceStopTime = TimeSpan.Zero;
                    studySentenceRun.SilenceStartFront = TimeSpan.Zero;
                    studySentenceRun.SilenceStopFront = TimeSpan.Zero;
                    studySentenceRun.SilenceStartBack = TimeSpan.Zero;
                    studySentenceRun.SilenceStopBack = TimeSpan.Zero;
                    */

                    if (!String.IsNullOrEmpty(studySentenceRun.Error))
                        studySentenceRun.Error += "|";

                    studySentenceRun.Error += "Times were bad (2)";
                }
                else
                {
                    for (int i = 0; i < studySentenceRunIndex; i++)
                    {
                        SentenceRun sentenceRun = studySentenceRuns[i];

                        if (studySentenceRun.IsOverlapsTime(sentenceRun))
                        {
                            /*
                            studySentenceRun.SentenceStartTime = TimeSpan.Zero;
                            studySentenceRun.SentenceStopTime = TimeSpan.Zero;
                            studySentenceRun.SilenceStartFront = TimeSpan.Zero;
                            studySentenceRun.SilenceStopFront = TimeSpan.Zero;
                            studySentenceRun.SilenceStartBack = TimeSpan.Zero;
                            studySentenceRun.SilenceStopBack = TimeSpan.Zero;
                            */

                            if (!String.IsNullOrEmpty(studySentenceRun.Error))
                                studySentenceRun.Error += "|";

                            studySentenceRun.Error += "Time overlapped (2)";
                            break;
                        }
                    }
                }
            }
        }

        // Take the timing info from the study and parsed sentence runs and set up
        // media runs in the study items.
        protected void AddMediaRuns(
            List<SentenceRun> studySentenceRuns,
            string mediaItemKey,
            string languageMediaItemKey,
            ref string errorMessage)
        {
            int studySentenceRunCount = studySentenceRuns.Count();
            int studySentenceRunIndex;

            // Now set up media runs. Study and parsed sentence runs should be in sync, though the counts may differ.
            for (studySentenceRunIndex = 0; studySentenceRunIndex < studySentenceRunCount; studySentenceRunIndex++)
            {
                SentenceRun studySentenceRun = studySentenceRuns[studySentenceRunIndex];
                TextRun sentenceRun = studySentenceRun.StudySentence;
                MediaRun mediaRun = sentenceRun.GetMediaRunWithReferenceKeys(mediaItemKey, languageMediaItemKey);

                if (!studySentenceRun.HasValidSentenceTimes())
                {
                    MultiLanguageItem studyItem = studySentenceRun.StudyItem;
                    string namePathString = studyItem.GetNameStringWithOrdinal() + "(" + studySentenceRun.SentenceIndex.ToString() + ")";

                    if (!String.IsNullOrEmpty(errorMessage))
                        errorMessage += "\n";

                    errorMessage += "Sentence with no media time for " + LanguageName + ": " + namePathString;

                    if (mediaRun != null)
                        sentenceRun.DeleteMediaRun(mediaRun);

                    continue;
                }

                if (mediaRun == null)
                {
                    mediaRun = new MediaRun(
                        "Audio",
                        mediaItemKey,
                        languageMediaItemKey,
                        studySentenceRun.SentenceStartTime,
                        studySentenceRun.SentenceLengthTime);
                    sentenceRun.AddMediaRun(mediaRun);
                }
                else
                {
                    mediaRun.Start = studySentenceRun.SentenceStartTime;
                    mediaRun.Length = studySentenceRun.SentenceLengthTime;
                }
            }
        }

        public static bool DumpAudioTextMappingResults = true;
        public static bool DumpWordAudioTextMappingResults = true;
        public static bool DumpDebugWordAudioTextMappingResults = true;
        public static string DumpAudioTextMappingDirectoryPath;
        public static StringBuilder DumpAudioTextMappingSB = null;
        public static StringBuilder DumpWordAudioTextMappingSB = null;
        public static StringBuilder DumpDebugWordAudioTextMappingSB = null;

        // Dump mapping results.
        protected void DumpMappingResults(
            List<SentenceRun> studySentenceRuns,
            List<SentenceRun> parsedSentenceRuns,
            LanguageID languageID,
            string mediaItemKey,
            string languageMediaItemKey,
            string key,
            double durationSeconds)
        {
            if (!DumpAudioTextMappingResults)
                return;

            StringBuilder sb = new StringBuilder();

            DumpMappingResultsHeading(sb);

            if ((DumpAudioTextMappingSB != null) && (DumpAudioTextMappingSB.Length == 0))
                DumpMappingResultsHeading(DumpAudioTextMappingSB);

            int sentenceIndex;
            int studySentenceCount = studySentenceRuns.Count();
            int parsedSentenceCount = parsedSentenceRuns.Count();

            if (parsedSentenceCount != studySentenceCount)
            {
                string errorMessage = "studySentenceRuns and parsedSentenceRuns have different counts: "
                    + studySentenceCount.ToString() + ", " + parsedSentenceCount.ToString();
                sb.AppendLine(errorMessage);
            }

            for (sentenceIndex = 0; sentenceIndex < studySentenceCount; sentenceIndex++)
            {
                SentenceRun studySentenceRun = studySentenceRuns[sentenceIndex];
                SentenceRun parsedSentenceRun = (sentenceIndex < parsedSentenceCount ? parsedSentenceRuns[sentenceIndex] : null);
                DumpMappingResultsSentence(studySentenceRun, parsedSentenceRun, languageID, mediaItemKey, languageMediaItemKey, sb);

                if (DumpAudioTextMappingSB != null)
                    DumpMappingResultsSentence(studySentenceRun, parsedSentenceRun, languageID, mediaItemKey, languageMediaItemKey, DumpAudioTextMappingSB);
            }

            if (DumpAudioTextMappingSB != null)
            {
                DumpAudioTextMappingSB.AppendLine("MappedSentenceCount = " + (MappedSentenceCount - SaveMappedSentenceCount).ToString());
                DumpAudioTextMappingSB.AppendLine("CombinedSentenceScorePoint25 = " + (CombinedSentenceScorePoint25 - SaveCombinedSentenceScorePoint25).ToString());
                DumpAudioTextMappingSB.AppendLine("CombinedSentenceScorePoint75 = " + (CombinedSentenceScorePoint75 - SaveCombinedSentenceScorePoint75).ToString());
                DumpAudioTextMappingSB.AppendLine("SentenceMatchLastWordCount = " + (SentenceMatchLastWordCount - SaveSentenceMatchLastWordCount).ToString());
                DumpAudioTextMappingSB.AppendLine("SentenceNotMatchLastWordCount = " + (SentenceNotMatchLastWordCount - SaveSentenceNotMatchLastWordCount).ToString());
                DumpAudioTextMappingSB.AppendLine("RequiredResetCount = " + (RequiredResetCount - SaveRequiredResetCount).ToString());
                DumpAudioTextMappingSB.AppendLine("IgnoredSentenceCount = " + (IgnoredSentenceCount - SaveIgnoredSentenceCount).ToString());
                DumpAudioTextMappingSB.AppendLine("Timing = " + TimeSpan.FromSeconds(durationSeconds).ToString());
                DumpAudioTextMappingSB.AppendLine("When = " + DateTime.Now.ToString());
            }

            sb.AppendLine("MappedSentenceCount = " + (MappedSentenceCount - SaveMappedSentenceCount).ToString());
            sb.AppendLine("CombinedSentenceScorePoint25 = " + (CombinedSentenceScorePoint25 - SaveCombinedSentenceScorePoint25).ToString());
            sb.AppendLine("CombinedSentenceScorePoint75 = " + (CombinedSentenceScorePoint75 - SaveCombinedSentenceScorePoint75).ToString());
            sb.AppendLine("SentenceMatchLastWordCount = " + (SentenceMatchLastWordCount - SaveSentenceMatchLastWordCount).ToString());
            sb.AppendLine("SentenceNotMatchLastWordCount = " + (SentenceNotMatchLastWordCount - SaveSentenceNotMatchLastWordCount).ToString());
            sb.AppendLine("RequiredResetCount = " + (RequiredResetCount - SaveRequiredResetCount).ToString());
            sb.AppendLine("IgnoredSentenceCount = " + (IgnoredSentenceCount - SaveIgnoredSentenceCount).ToString());
            sb.AppendLine("Timing = " + TimeSpan.FromSeconds(durationSeconds).ToString());
            sb.AppendLine("When = " + DateTime.Now.ToString());

            string filePath = MediaUtilities.ConcatenateFilePath(
                (!String.IsNullOrEmpty(DumpAudioTextMappingDirectoryPath) ?
                    DumpAudioTextMappingDirectoryPath :
                    ApplicationData.TempPath),
                key + "_MappingResults_" + LanguageID.LanguageCultureExtensionCode + ".txt");

            FileSingleton.DirectoryExistsCheck(filePath);

            if (FileSingleton.Exists(filePath))
                FileSingleton.Delete(filePath);

            FileSingleton.WriteAllText(filePath, sb.ToString(), ApplicationData.Encoding);
        }

        protected void DumpMappingResultsHeading(StringBuilder sb)
        {
            sb.AppendLine(
                "Path" +
                "\t" +
                "Verse" +
                "\t" +
                "Sentence #" +
                "\t" +
                "Original sentence text" +
                "\t" +
                "Parsed sentence text" +
                "\t" +
                "Original start word index" +
                "\t" +
                "Parsed start word index" +
                "\t" +
                "Original word count" +
                "\t" +
                "Parsed word count" +
                "\t" +
                "Original final word count" +
                "\t" +
                "Parsed final word count" +
                "\t" +
                "Original sentence length no separators" +
                "\t" +
                "Parsed sentence length no separators" +
                "\t" +
                "Matched word count" +
                "\t" +
                "Original unmatched count" +
                "\t" +
                "Parsed unmatched count" +
                "\t" +
                "Original unmatched words" +
                "\t" +
                "Parsed unmatched words" +
                "\t" +
                "Required reset" +
                "\t" +
                "Ignored" +
                "\t" +
                "Original extra text" +
                "\t" +
                "Parsed extra text" +
                "\t" +
                "Main score" +
                "\t" +
                "Extra score" +
                "\t" +
                "Combined score" +
                "\t" +
                "Main score prior to resync" +
                "\t" +
                "Parsed sentence prior to resync" +
                "\t" +
                "Matched/total percentage" +
                "\t" +
                "Sentence start time" +
                "\t" +
                "Sentence stop time" +
                "\t" +
                "Sentence time length" +
                "\t" +
                "Silence start time front" +
                "\t" +
                "Silence stop time front" +
                "\t" +
                "Silence time length front" +
                "\t" +
                "Silence start time back" +
                "\t" +
                "Silence stop time back" +
                "\t" +
                "Silence time length back" +
                "\t" +
                "Last word start time" +
                "\t" +
                "Last word stop time" +
                "\t" +
                "Next word start time" +
                "\t" +
                "Next word stop time" +
                "\t" +
                "Parsed last word not matched" +
                "\t" +
                "Error message");
        }

        // Dump mapping results.
        protected void DumpMappingResultsSentence(
            SentenceRun studySentenceRun,
            SentenceRun parsedSentenceRun,
            LanguageID languageID,
            string mediaItemKey,
            string languageMediaItemKey,
            StringBuilder sb)
        {
            MultiLanguageItem studyItem = studySentenceRun.StudyItem;
            LanguageItem languageItem = studyItem.LanguageItem(languageID);
            int sentenceIndex = studySentenceRun.SentenceIndex;
            TextRun sentenceRun = languageItem.GetSentenceRun(sentenceIndex);
            MediaRun mediaRun = sentenceRun.GetMediaRunWithKeyAndReferenceKeys("Audio", mediaItemKey, languageMediaItemKey);
            int sentenceWordRunStartIndex = studySentenceRun.StartWordIndexInParent;
            int sentenceWordRunCount = studySentenceRun.SentenceWordCount;
            int matchedRunCount = studySentenceRun.MatchedWordCount;
            int unmatchedRunCount = studySentenceRun.UnmatchedWordCount;
            SentenceRun studyExtraSentenceRun = studySentenceRun.ExtraSentenceRun;
            SentenceRun parsedExtraSentenceRun = parsedSentenceRun.ExtraSentenceRun;
            string errorMessage = String.Empty;

            if (!String.IsNullOrEmpty(studySentenceRun.Error))
            {
                if (!String.IsNullOrEmpty(errorMessage))
                    errorMessage += "|";

                errorMessage += "(original) " + studySentenceRun.Error;
            }

            if (!String.IsNullOrEmpty(parsedSentenceRun.Error))
            {
                if (!String.IsNullOrEmpty(errorMessage))
                    errorMessage += "|";

                errorMessage += "(parsed) " + parsedSentenceRun.Error;
            }

            if (mediaRun == null)
                mediaRun = sentenceRun.GetMediaRunWithKeyAndReferenceKeys("Video", mediaItemKey, languageMediaItemKey);

            string Path = studyItem.Content.GetNamePathString(LanguageLookup.English, "/");
            string Verse = studyItem.GetNameStringWithOrdinal();
            int SentenceNumber = sentenceIndex + 1;
            string OriginalSentenceText = studySentenceRun.SentenceText;
            string ParsedSentenceText = (parsedSentenceRun != null ? parsedSentenceRun.SentenceText : "");
            int OriginalStartWordIndex = studySentenceRun.StartWordIndexInParent;
            int ParsedStartWordIndex = parsedSentenceRun.StartWordIndexInParent;
            int OriginalWordCount = sentenceWordRunCount;
            int ParsedWordCount = parsedSentenceRun.SentenceWordCount;
            int OriginalFinalWordCount = studySentenceRun.FinalSentenceWordCount;
            int ParsedFinalWordCount = parsedSentenceRun.FinalSentenceWordCount;
            int OriginalSentenceLengthNoSeparators = studySentenceRun.SentenceTextLengthNoSeparators;
            int ParsedSentenceLengthNoSeparators = parsedSentenceRun.SentenceTextLengthNoSeparators;
            int MatchedWordCount = matchedRunCount;
            int OriginalUnmatchedCount = unmatchedRunCount;
            int ParsedUnmatchedCount = (parsedSentenceRun != null ? parsedSentenceRun.UnmatchedWordCount : 0);
            string OriginalUnmatchedWords =
                (studySentenceRun.UnmatchedWords != null ?
                    TextUtilities.GetStringFromStringListDelimited(studySentenceRun.UnmatchedWords, ", ") :
                    String.Empty);
            string ParsedUnmatchedWords =
                ((parsedSentenceRun != null) && (parsedSentenceRun.UnmatchedWords != null) ?
                    TextUtilities.GetStringFromStringListDelimited(parsedSentenceRun.UnmatchedWords, ", ") :
                    String.Empty);
            string RequiredReset = (studySentenceRun.RequiredReset ? "1" : "0");
            string Ignored = (studySentenceRun.Ignored ? "1" : "0");
            string OriginalExtraText = (studyExtraSentenceRun != null ? studyExtraSentenceRun.SentenceText : "");
            string ParsedExtraText = (parsedExtraSentenceRun != null ? parsedExtraSentenceRun.SentenceText : "");
            double MainScore = studySentenceRun.Score;
            double ExtraScore = (studyExtraSentenceRun != null ? studyExtraSentenceRun.Score : 0.0);
            double CombinedScore = studySentenceRun.CombinedScore;
            double SentenceStart = studySentenceRun.SentenceStartTime.TotalSeconds;
            double SentenceStop = studySentenceRun.SentenceStopTime.TotalSeconds;
            double SentenceLength = studySentenceRun.SentenceLengthTime.TotalSeconds;
            double SilenceStartFront = studySentenceRun.SilenceStartFront.TotalSeconds;
            double SilenceStopFront = studySentenceRun.SilenceStopFront.TotalSeconds;
            double SilenceLengthFront = studySentenceRun.SilenceLengthFront.TotalSeconds;
            double SilenceStartBack = studySentenceRun.SilenceStartBack.TotalSeconds;
            double SilenceStopBack = studySentenceRun.SilenceStopBack.TotalSeconds;
            double SilenceLengthBack = studySentenceRun.SilenceLengthBack.TotalSeconds;
            double LastWordStartTime = parsedSentenceRun.GetLastWordStartTime().TotalSeconds;
            double LastWordStopTime = parsedSentenceRun.GetLastWordStopTime().TotalSeconds;
            double NextWordStartTime = parsedSentenceRun.GetNextWordStartTime().TotalSeconds;
            double NextWordStopTime = parsedSentenceRun.GetNextWordStopTime().TotalSeconds;
            double MatchedTotalPercentage = 0;

            if (OriginalWordCount != 0)
                MatchedTotalPercentage = (double)MatchedWordCount / OriginalWordCount;

            int parsedSentenceWordCount;
            int ParsedLastWordNotMatched = 1;

            // Count number of sentences with matching last word.
            if ((parsedSentenceRun != null) && ((parsedSentenceWordCount = parsedSentenceRun.SentenceWordCount) != 0))
            {
                WordRun studyLastWordRun = studySentenceRun.LastWordRunPossiblyCombined;
                WordRun parsedLastWordRun = parsedSentenceRun.LastWordRunPossiblyCombined;

                if ((studyLastWordRun != null) && (parsedLastWordRun != null))
                {
                    if (studyLastWordRun.Matched &&
                            parsedLastWordRun.Matched &&
                            (studyLastWordRun.WordIndexOther == parsedLastWordRun.WordIndex))
                        ParsedLastWordNotMatched = 0;
                }
            }

            sb.AppendLine(
                Path +
                "\t" +
                Verse +
                "\t" +
                SentenceNumber.ToString() +
                "\t" +
                OriginalSentenceText +
                "\t" +
                ParsedSentenceText +
                "\t" +
                OriginalStartWordIndex +
                "\t" +
                ParsedStartWordIndex +
                "\t" +
                OriginalWordCount.ToString() +
                "\t" +
                ParsedWordCount.ToString() +
                "\t" +
                OriginalFinalWordCount.ToString() +
                "\t" +
                ParsedFinalWordCount.ToString() +
                "\t" +
                OriginalSentenceLengthNoSeparators.ToString() +
                "\t" +
                ParsedSentenceLengthNoSeparators.ToString() +
                "\t" +
                MatchedWordCount.ToString() +
                "\t" +
                OriginalUnmatchedCount.ToString() +
                "\t" +
                ParsedUnmatchedCount.ToString() +
                "\t" +
                OriginalUnmatchedWords +
                "\t" +
                ParsedUnmatchedWords +
                "\t" +
                RequiredReset +
                "\t" +
                Ignored +
                "\t" +
                OriginalExtraText +
                "\t" +
                ParsedExtraText +
                "\t" +
                MainScore.ToString() +
                "\t" +
                ExtraScore.ToString() +
                "\t" +
                CombinedScore.ToString() +
                "\t" +
                studySentenceRun.MainScorePriorToResync.ToString() +
                "\t" +
                studySentenceRun.BestParsedSentencePriorToResync +
                "\t" +
                MatchedTotalPercentage.ToString() +
                "\t" +
                SentenceStart.ToString() +
                "\t" +
                SentenceStop.ToString() +
                "\t" +
                SentenceLength.ToString() +
                "\t" +
                SilenceStartFront.ToString() +
                "\t" +
                SilenceStopFront.ToString() +
                "\t" +
                SilenceLengthFront.ToString() +
                "\t" +
                SilenceStartBack.ToString() +
                "\t" +
                SilenceStopBack.ToString() +
                "\t" +
                SilenceLengthBack.ToString() +
                "\t" +
                LastWordStartTime.ToString() +
                "\t" +
                LastWordStopTime.ToString() +
                "\t" +
                NextWordStartTime.ToString() +
                "\t" +
                NextWordStopTime.ToString() +
                "\t" +
                ParsedLastWordNotMatched.ToString() +
                "\t" +
                errorMessage);
        }

        // Dump mapping results.
        protected void DumpWordMappingResults(
            List<SentenceRun> studySentenceRuns,
            List<SentenceRun> parsedSentenceRuns,
            string key)
        {
            if (!DumpWordAudioTextMappingResults)
                return;

            StringBuilder sb = new StringBuilder();

            DumpWordMappingResultsHeading(sb);

            if ((DumpWordAudioTextMappingSB != null) && (DumpWordAudioTextMappingSB.Length == 0))
                DumpWordMappingResultsHeading(DumpWordAudioTextMappingSB);

            int sentenceIndex;
            int studySentenceCount = studySentenceRuns.Count();
            int parsedSentenceCount = parsedSentenceRuns.Count();

            if (parsedSentenceCount != studySentenceCount)
            {
                string errorMessage = "studySentenceRuns and parsedSentenceRuns have different counts: "
                    + studySentenceCount.ToString() + ", " + parsedSentenceCount.ToString();
                sb.AppendLine(errorMessage);
            }

            for (sentenceIndex = 0; sentenceIndex < studySentenceCount; sentenceIndex++)
            {
                SentenceRun studySentenceRun = studySentenceRuns[sentenceIndex];
                SentenceRun parsedSentenceRun = (sentenceIndex < parsedSentenceCount ? parsedSentenceRuns[sentenceIndex] : null);
                DumpWordMappingResultsSentence(studySentenceRun, parsedSentenceRun, sb);

                if (DumpWordAudioTextMappingSB != null)
                    DumpWordMappingResultsSentence(studySentenceRun, parsedSentenceRun, DumpWordAudioTextMappingSB);
            }

            string filePath = MediaUtilities.ConcatenateFilePath(
                (!String.IsNullOrEmpty(DumpAudioTextMappingDirectoryPath) ?
                    DumpAudioTextMappingDirectoryPath :
                    ApplicationData.TempPath),
                key + "_WordMappingResults_" + LanguageID.LanguageCultureExtensionCode + ".txt");

            FileSingleton.DirectoryExistsCheck(filePath);

            if (FileSingleton.Exists(filePath))
                FileSingleton.Delete(filePath);

            FileSingleton.WriteAllText(filePath, sb.ToString(), ApplicationData.Encoding);
        }

        // Start dump of continuous word mapping results.
        protected void DumpDebugWordMappingResultsBegin()
        {
            if (!DumpWordAudioTextMappingResults)
                return;

            DumpDebugWordAudioTextMappingSB = new StringBuilder();
            DumpWordMappingResultsHeading(DumpDebugWordAudioTextMappingSB);
        }

        // Dump candidate state.
        protected void DumpDebugWordMappingCandidateState(
            string mainLabel,
            string extraMessage,
            int offset,
            SentenceRun studySentenceRun,
            SentenceRun parsedSentenceRunCandidate,
            SentenceRun nextStudySentenceRun,
            SentenceRun nextParsedSentenceRunCandidate)
        {
            DumpDebugWordMappingResultsLabel(mainLabel);
            DumpDebugWordMappingResultsLabel("OrigRaw(" +
                studySentenceRun.SentenceWordCount.ToString() +
                "," +
                studySentenceRun.FinalSentenceWordCount.ToString() +
                "," +
                studySentenceRun.SentenceTextLengthNoSeparators.ToString() + 
                ")=" +
                studySentenceRun.SentenceText);
            DumpDebugWordMappingResultsLabel("CandRaw(" +
                parsedSentenceRunCandidate.SentenceWordCount.ToString() +
                "," +
                parsedSentenceRunCandidate.FinalSentenceWordCount.ToString() +
                "," +
                parsedSentenceRunCandidate.SentenceTextLengthNoSeparators.ToString() +
                ")=" +
                parsedSentenceRunCandidate.SentenceText);
            DumpDebugWordMappingResultsLabel("OrigWords=" + studySentenceRun.SentenceFromWords);
            DumpDebugWordMappingResultsLabel("CandWords=" + parsedSentenceRunCandidate.SentenceFromWords);

            DumpDebugWordMappingResultsSentence(
                studySentenceRun,
                parsedSentenceRunCandidate,
                "Offset = " + offset.ToString() +
                    ", CombinedScore = " + parsedSentenceRunCandidate.CombinedScore +
                    ", MainScore = " + parsedSentenceRunCandidate.Score +
                    (nextParsedSentenceRunCandidate != null ? ", ExtraScore = " + nextParsedSentenceRunCandidate.Score : ""),
                extraMessage);

            if ((nextStudySentenceRun != null) && (nextParsedSentenceRunCandidate != null))
            {
                DumpDebugWordMappingResultsLabel("Next OrigRaw(" +
                    nextStudySentenceRun.SentenceWordCount.ToString() +
                    "," +
                    nextStudySentenceRun.FinalSentenceWordCount.ToString() +
                    "," +
                    nextStudySentenceRun.SentenceTextLengthNoSeparators.ToString() +
                    ")=" +
                    nextStudySentenceRun.SentenceText);
                DumpDebugWordMappingResultsLabel("Next CandRaw(" +
                    nextParsedSentenceRunCandidate.SentenceWordCount.ToString() +
                    "," +
                    nextParsedSentenceRunCandidate.FinalSentenceWordCount.ToString() +
                    "," +
                    nextParsedSentenceRunCandidate.SentenceTextLengthNoSeparators.ToString() +
                    ")=" +
                    nextParsedSentenceRunCandidate.SentenceText);
                DumpDebugWordMappingResultsLabel("Next OrigWord=" + nextStudySentenceRun.SentenceFromWords);
                DumpDebugWordMappingResultsLabel("Next CandWord=" + nextParsedSentenceRunCandidate.SentenceFromWords);

                DumpDebugWordMappingResultsSentence(
                    nextStudySentenceRun,
                    nextParsedSentenceRunCandidate,
                    "ExtraScore = " + nextParsedSentenceRunCandidate.Score,
                    "");
            }
            else
                DumpDebugWordMappingResultsLabel("(No next sentence candidate.)");
        }

        // Dump debug word mapping results for the given sentence.
        protected void DumpDebugWordMappingResultsSentence(
            SentenceRun studySentenceRun,
            SentenceRun parsedSentenceRun,
            string label,
            string extraMessage)
        {
            if (!DumpWordAudioTextMappingResults || (DumpDebugWordAudioTextMappingSB == null))
                return;

            if (!String.IsNullOrEmpty(label))
                DumpDebugWordAudioTextMappingSB.AppendLine(label);

            if (!String.IsNullOrEmpty(extraMessage))
                DumpDebugWordAudioTextMappingSB.AppendLine(extraMessage);

            DumpWordMappingResultsSentence(studySentenceRun, parsedSentenceRun, DumpDebugWordAudioTextMappingSB);
        }

        // Dump mapping results.
        protected void DumpDebugWordMappingResultsLabel(string label)
        {
            if (!DumpWordAudioTextMappingResults || (DumpDebugWordAudioTextMappingSB == null))
                return;

            if (!String.IsNullOrEmpty(label))
                DumpDebugWordAudioTextMappingSB.AppendLine(label);
        }

        // Stop dump of continuous word mapping results.
        protected void DumpDebugWordMappingResultsEnd(string key)
        {
            if (!DumpWordAudioTextMappingResults || (DumpDebugWordAudioTextMappingSB == null))
                return;

            string filePath = MediaUtilities.ConcatenateFilePath(
                (!String.IsNullOrEmpty(DumpAudioTextMappingDirectoryPath) ?
                    DumpAudioTextMappingDirectoryPath :
                    ApplicationData.TempPath),
                key + "_DebugWordMappingResults_" + LanguageID.LanguageCultureExtensionCode + ".txt");

            FileSingleton.DirectoryExistsCheck(filePath);

            if (FileSingleton.Exists(filePath))
                FileSingleton.Delete(filePath);

            FileSingleton.WriteAllText(filePath, DumpDebugWordAudioTextMappingSB.ToString(), ApplicationData.Encoding);
        }

        protected void DumpWordMappingResultsHeading(StringBuilder sb)
        {
            DumpWordMappingResultsWord(
                null,
                null,
                null,
                0,
                false,
                true,
                sb);
        }

        // Dump mapping results.
        protected void DumpWordMappingResultsSentence(
            SentenceRun studySentenceRun,
            SentenceRun parsedSentenceRun,
            StringBuilder sb)
        {
            int OriginalWordCount = studySentenceRun.SentenceWordCount;

            List<WordRun> wordRuns = studySentenceRun.GetWordRuns();
            int wordIndex;

            for (wordIndex = 0; wordIndex < OriginalWordCount; wordIndex++)
            {
                WordRun studyWordRun = wordRuns[wordIndex];
                int WordNumber = wordIndex + 1;

                DumpWordMappingResultsWord(
                    studySentenceRun,
                    parsedSentenceRun,
                    studyWordRun,
                    WordNumber,
                    false,
                    false,
                    sb);
            }
        }

        // Dump mapping results.
        protected void DumpWordMappingResultsWord(
            SentenceRun studySentenceRun,
            SentenceRun parsedSentenceRun,
            WordRun wordRun,
            int wordNumber,
            bool isParsed,
            bool isHeading,
            StringBuilder sb)
        {
            MultiLanguageItem studyItem = null;
            int sentenceIndex = 0;
            int parsedWordIndex = 0;
            WordRun parsedWordRun = null;

            string Path = null;
            string Verse = null;
            int SentenceNumber = 0;
            string OriginalSentenceText = null;
            string ParsedSentenceText = null;
            int OriginalWordCount = 0;
            int ParsedWordCount = 0;
            int FinalOriginalWordCount = 0;
            int FinalParsedWordCount = 0;
            int OriginalSentenceLengthNoSeparators = 0;
            int ParsedSentenceLengthNoSeparators = 0;
            int MatchedWordCount = 0;
            int OriginalUnmatchedCount = 0;
            int ParsedUnmatchedCount = 0;

            if (!isHeading)
            {
                studyItem = studySentenceRun.StudyItem;
                sentenceIndex = studySentenceRun.SentenceIndex;
                parsedWordIndex = wordRun.WordIndexOther;
                parsedWordRun = wordRun.OtherWordRun;

                Path = studyItem.Content.GetNamePathString(LanguageLookup.English, "/");
                Verse = studyItem.GetNameStringWithOrdinal();
                SentenceNumber = sentenceIndex + 1;
                OriginalSentenceText = studySentenceRun.SentenceText;
                ParsedSentenceText = (parsedSentenceRun != null ? parsedSentenceRun.SentenceText : "");
                OriginalWordCount = studySentenceRun.SentenceWordCount;
                ParsedWordCount = parsedSentenceRun.SentenceWordCount;
                FinalOriginalWordCount = studySentenceRun.FinalSentenceWordCount;
                FinalParsedWordCount = parsedSentenceRun.FinalSentenceWordCount;
                OriginalSentenceLengthNoSeparators = studySentenceRun.SentenceTextLengthNoSeparators;
                ParsedSentenceLengthNoSeparators = parsedSentenceRun.SentenceTextLengthNoSeparators;
                MatchedWordCount = studySentenceRun.MatchedWordCount;
                OriginalUnmatchedCount = studySentenceRun.UnmatchedWordCount;
                ParsedUnmatchedCount = (parsedSentenceRun != null ? parsedSentenceRun.UnmatchedWordCount : 0);
            }

            sb.Append(isHeading ? "Path" : Path);
            sb.Append("\t");
            sb.Append(isHeading ? "Verse" : Verse);
            sb.Append("\t");
            sb.Append(isHeading ? "Sentence #" : SentenceNumber.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Word #" : wordNumber.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original Word" : wordRun.Word);
            sb.Append("\t");
            sb.Append(isHeading ? "Parsed Word" : (parsedWordRun != null ? parsedWordRun.Word : String.Empty));
            sb.Append("\t");
            sb.Append(isHeading ? "Matched Original Word" : wordRun.MatchedWord);
            sb.Append("\t");
            sb.Append(isHeading ? "Matched Parsed Word" : (parsedWordRun != null ? parsedWordRun.MatchedWord : String.Empty));
            sb.Append("\t");
            sb.Append(isHeading ? "Word Index" : wordRun.WordIndex.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Word Index Other" : wordRun.WordIndexOther.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Matched" : (wordRun.Matched ? "1" : "0"));
            sb.Append("\t");
            sb.Append(isHeading ? "Word Match Score" : wordRun.WordMatchScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Character Position Factor" : wordRun.WordPositionFactor.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Edit Distance" : wordRun.EditDistance.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Edit Distance Factor squared" : wordRun.EditDistanceFactor.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Single Word Edit Distance Score" : wordRun.SingleWordEditDistanceScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Word Span Count" : wordRun.WordSpanCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original Start Index No Separators" : wordRun.StartIndexNoSeparators.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original Stop Index No Separators" : wordRun.StopIndexNoSeparators.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Parsed Start Index No Separators" : (parsedWordRun != null ? parsedWordRun.StartIndexNoSeparators.ToString() : ""));
            sb.Append("\t");
            sb.Append(isHeading ? "Parsed Stop Index No Separators" : (parsedWordRun != null ? parsedWordRun.StopIndexNoSeparators.ToString() : ""));
            sb.Append("\t");
            sb.Append(isHeading ? "Start Index" : wordRun.StartIndex.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Stop Index" : wordRun.StopIndex.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Sentence Index" : wordRun.SentenceIndex.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Study Item Index" : wordRun.StudyItemIndex.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Start Time" : wordRun.StartTime.TotalSeconds.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Stop Time" : wordRun.StopTime.TotalSeconds.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original word count" : OriginalWordCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Parsed word count" : ParsedWordCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original final word count" : FinalOriginalWordCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Parsed final word count" : FinalParsedWordCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original sentence length no separators" : OriginalSentenceLengthNoSeparators.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Parsed sentence length no separators" : ParsedSentenceLengthNoSeparators.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Matched word count" : MatchedWordCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original unmatched count" : OriginalUnmatchedCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Parsed unmatched count" : ParsedUnmatchedCount.ToString());
            sb.AppendLine("");
        }
    }
}
