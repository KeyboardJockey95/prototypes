using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public class AudioMapper : TaskUtilities
    {
        // Main data.

        // Language tool for the target language.
        public LanguageTool TargetLanguageTool { get; set; }

        // Sentence mappings.
        public List<SentenceMapping> SentenceMappings { get; set; }

        // Original sentence runs.
        public List<MappingSentenceRun> OriginalSentenceRuns { get; set; }

        // Original word runs.
        public List<MappingWordRun> OriginalWordRuns { get; set; }

        // List of original text blocks. Corresponds to study item index.
        public List<TextBlock> OriginalTextBlocks { get; set; }

        // List of original text blocks before substitutions. Corresponds to study item index.
        public List<TextBlock> RawOriginalTextBlocks { get; set; }

        // Transcribed word runs.
        public List<MappingWordRun> TranscribedWordRuns { get; set; }

        // Transcribed text block.
        public TextBlock TranscribedTextBlock { get; set; }

        // Transcribed text block before substitutions.
        public TextBlock RawTranscribedTextBlock { get; set; }

        // Statistics for the mapping.
        public AudioMapperStatistics Statistics { get; set; }

        // Accumulated error messages.
        public string Error { get; set; }

        // Implementation data.

        // Dictionary of abbreviations that have been standardized.
        private Dictionary<string, string[]> _AbbreviationStandardizedDictionary;

        // Dictionary of abbreviation definitions, keyed on first word of expansion.
        private Dictionary<string, List<KeyValuePair<string, string[]>>> _AbbreviationFirstWordDictionary;

        // Implementation constants.

        // Match score mode code enum.
        public enum MatchScoreModeCode
        {
            UseCharacterIndexOnly,
            UseWordIndexOnly,
            UseHighestMatchScore,
            UseWordAndCharacterIndexProduct
        };

        // Extra sentence candidate word count mode code enum.
        public enum ExtraSentenceWordCountModeCode
        {
            ConstantWordCount,
            OriginalMainCountHalf,
            OriginalMainCountFull,
            OriginalMainCountDivisor,
            OriginalNextCountFull
        };

        // Extra sentence candidate mode code enum.
        public enum ExtraSentenceCharacterCountModeCode
        {
            MatchOriginalCharacterCount,
            MatchOriginalCharacterCountNoSplit,
            MatchOriginalWordCount
        };

        // Sentence match mode.
        public enum SentenceMatchModeCode
        {
            UseCombinedSentenceScore,
            UseMainScore,
            UseSentenceDistance
        };

        // Weighting mode code enum.
        public enum WeightingModeCode
        {
            WordCountWeighting,
            ConstantWeighting
        };

        // Match score mode.
        public static MatchScoreModeCode MatchScoreMode = MatchScoreModeCode.UseCharacterIndexOnly;

        // Extra sentence candidate word count mode code enum.
        public static ExtraSentenceWordCountModeCode ExtraSentenceWordCountMode = ExtraSentenceWordCountModeCode.ConstantWordCount;

        // Extra sentence candidate mode.
        public static ExtraSentenceCharacterCountModeCode ExtraSentenceCharacterCountMode = ExtraSentenceCharacterCountModeCode.MatchOriginalCharacterCount;

        // Sentence match mode.
        public static SentenceMatchModeCode SentenceMatchMode = SentenceMatchModeCode.UseCombinedSentenceScore;

        // Whether to walk sentence start or not.
        public static bool WalkSentenceStart = false;

        // Weighting mode.
        public static WeightingModeCode WeightingMode = WeightingModeCode.ConstantWeighting;

        // Main sentence weighting factor.
        public static double MainSentenceWeightingFactor = 1.0;

        // Extra sentence weighting factor.
        // For WordCountWeighting, use 1.125
        // For ConstantWeighting, try 0.1, 0.25, 0.5, 0.75
        public static double ExtraSentenceWeightingFactor = 0.25;

        // Number of words for extra sentence.
        public static int ExtraSentenceLength = 5;

        // Minimum words for extra sentence.
        public static int ExtraSentenceLengthMinimum = 0;

        // Maximum words for extra sentence.
        public static int ExtraSentenceLengthMaximum = 0;

        // Divisor for next sentence length. 0 for no divisor and use ExtraSentenceLength;
        public static int ExtraSentenceDivisor = 0;

        // Divisor for unmatched transcribed word sum.
        public static double TranscribedUnusedSumPenaltyDivisor = 3;

        // Resync main score threshold.
        public static double ResyncMainScoreThreshold = 0.32;

        // Ignore main score threshold.
        public static double IgnoreMainScoreThreshold = 0.32;

        // Max number of words that can be combined.
        public static int MaxWordGroupSize = 3;

        // Edit distance factor exponent.
        public static double EditDistanceFactorExponent = 2.0;

        // Position factor exponent.
        public static double PositionFactorExponent = 1.9;

        // Initial amplitude threshold for looking for silence.
        public static int AmplitudeThreshold = 128;

        // Amplitude increment step.
        public static int AmplitudeIncrement = 64;

        // Amplitude max silence threshold.
        public static int AmplitudeSilenceMax = 384;

        // Initial silence break threshold.
        public static TimeSpan SilenceWidthThreshold = new TimeSpan(0, 0, 0, 0, 350);

        // Minimal silence break threshold.
        public static TimeSpan SilenceWidthThresholdMinimal = new TimeSpan(0, 0, 0, 0, 50);

        // Search time initial extra.
        public static TimeSpan SearchLimitDefault = new TimeSpan(0, 0, 0, 1, 0);

        // Search time increment.
        public static TimeSpan SearchTimeIncrement = new TimeSpan(0, 0, 0, 0, 500);

        // Default sentence break lead time - 50 msec.
        public static TimeSpan DefaultLeadTime = new TimeSpan(0, 0, 0, 0, 50);

        // Minimum sentence break lead time - 100 msec.
        public static TimeSpan MinimumLeadTime = new TimeSpan(0, 0, 0, 0, 100);

        // Debugging data.

        // Report row breakpoint. Set to -1 to disable breakpoint. Value is row number in report table.
        public static int DebugRowBreak = -1;

        // Main constructor.
        public AudioMapper(
            LanguageTool targetLanguageTool,
            UserRecord userRecord,
            AudioMapperStatistics statistics) : base(userRecord)
        {
            ClearAudioMapper();
            TargetLanguageTool = targetLanguageTool;
            Statistics = statistics;
        }

        // Copy constructor.
        public AudioMapper(AudioMapper other) : base(other)
        {
            ClearAudioMapper();
            CopyAudioMapper(other);
        }

        // Default constructor.
        public AudioMapper()
        {
            ClearAudioMapper();
        }

        // Clear audio mapper data.
        protected void ClearAudioMapper()
        {
            TargetLanguageTool = null;
            SentenceMappings = null;
            OriginalSentenceRuns = null;
            OriginalWordRuns = null;
            OriginalTextBlocks = null;
            RawOriginalTextBlocks = null;
            TranscribedWordRuns = null;
            TranscribedTextBlock = null;
            RawTranscribedTextBlock = null;
            Statistics = null;
            Error = String.Empty;
            _AbbreviationStandardizedDictionary = null;
            _AbbreviationFirstWordDictionary = null;
        }

        // Target language ID.
        public LanguageID TargetLanguageID
        {
            get
            {
                if (TargetLanguageTool != null)
                    return TargetLanguageTool.LanguageID;

                return null;
            }
        }

        // Do audio mapping given the audio file path and study items.
        public bool MatchAudioToStudyItems(
            string mediaPath,
            List<MultiLanguageItem> studyItems,
            string mediaItemKey,
            string languageMediaItemKey)
        {
            if (String.IsNullOrEmpty(mediaPath))
            {
                PutError("No media file path.");
                return false;
            }

            if (!FileSingleton.Exists(mediaPath))
            {
                PutError("Media file doesn't exist", mediaPath);
                return false;
            }

            string errorMessage;
            WaveAudioBuffer audioWavData = MediaConvertSingleton.Mp3Decoding(mediaPath, out errorMessage);

            if (audioWavData == null)
            {
                PutError(errorMessage);
                return false;
            }

            List<LanguageID> languageIDs = new List<LanguageID>() { TargetLanguageID };
            int startWaveIndex = 0;
            int endWaveIndex = audioWavData.SampleCount;
            int totalSampleCount = endWaveIndex - startWaveIndex;
            int sampleCount = endWaveIndex - startWaveIndex;
            TimeSpan mediaStart;
            TimeSpan mediaStop;
            string matchedText;
            List<TextRun> matchedWordRuns;
            List<string> hints = MultiLanguageItem.GetUniqueWords(studyItems, languageIDs);
            string nodePathString = (studyItems.Count() != 0 ? studyItems[0].Node.GetNamePathString(null, "_") : String.Empty);
            string cacheKey = cacheKey = nodePathString + "_" + MediaUtilities.GetBaseFileName(mediaPath)
                + "-"
                + startWaveIndex.ToString()
                + "-"
                + endWaveIndex.ToString();
            string audioCacheFilePath = ApplicationData.Global.GetUserDataDoubleNestedFilePath(
                UserName,
                "AudioMapping",
                "AudioCache",
                cacheKey + "_wave.wav");
            FileBuffer waveFileBuffer = new FileBuffer(audioCacheFilePath, audioWavData.Storage);

            mediaStart = audioWavData.GetSampleTime(startWaveIndex);
            mediaStop = audioWavData.GetSampleTime(endWaveIndex);

            if (Statistics != null)
                Statistics.StartSpeechToTextTimer();

            if (SpeechToTextSingleton.MapWaveText(
                audioWavData,
                mediaStart,
                mediaStop,
                languageIDs,
                hints,
                UserName,
                cacheKey,
                out matchedText,
                out matchedWordRuns,
                out errorMessage))
            {
                if (Statistics != null)
                    Statistics.StopSpeechToTextTimer();

                if (!MatchSpeechToTextToStudyItems(
                        audioWavData,
                        studyItems,
                        mediaItemKey,
                        languageMediaItemKey,
                        matchedText,
                        matchedWordRuns))
                {
                    return false;
                }
            }
            else
            {
                if (Statistics != null)
                    Statistics.StopSpeechToTextTimer();

                return false;
            }

            return true;
        }

        public bool MatchSpeechToTextToStudyItems(
            WaveAudioBuffer audioWavData,
            List<MultiLanguageItem> studyItems,
            string mediaItemKey,
            string languageMediaItemKey,
            string transcribedText,
            List<TextRun> transcribedWordRuns)
        {
            bool returnValue = true;

            // Start timer.
            if (Statistics != null)
                Statistics.StartMappingTimer();

            try
            {
                // Initialize sentence mapping.
                if (!InitializeSentenceMapping(studyItems))
                    return false;

                // Initialize transcribed word runs.
                if (!InitializeTranscribedTextAndWordRuns(
                        transcribedText,
                        transcribedWordRuns))
                    return false;

                int sentenceCount = SentenceMappings.Count();
                int transcribedWordIndex = 0;

                for (int sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
                {
                    if ((DebugRowBreak != -1) && (sentenceIndex + 3 == DebugRowBreak))
                        PutConsoleMessage("Debug row breakpoint", DebugRowBreak.ToString());

                    // Do the mapping for the indexed sentence.
                    if (!MapSentence(
                        SentenceMappings,
                        OriginalWordRuns,
                        OriginalTextBlocks,
                        RawOriginalTextBlocks,
                        TranscribedWordRuns,
                        TranscribedTextBlock,
                        sentenceIndex,
                        ref transcribedWordIndex))
                    {
                        returnValue = false;
                        break;
                    }
                }

                // Set up the sentence times.
                ConfigureSentenceTimes(
                    audioWavData,
                    SentenceMappings);

                // Set up the media runs.
                ConfigureMediaRuns(
                    studyItems,
                    mediaItemKey,
                    languageMediaItemKey,
                    SentenceMappings);
            }
            catch (Exception exception)
            {
                PutExceptionError("Exception during mapping", exception);
            }
            finally
            {
                // Stop timer.
                if (Statistics != null)
                    Statistics.StopMappingTimer();

                // Do reporting.
                HandleReporting(SentenceMappings);
            }

            return returnValue;
        }

        // Initialize sentence mapping.
        // This function populates the SentenceMappings and OriginalWordRuns lists.
        protected bool InitializeSentenceMapping(List<MultiLanguageItem> studyItems)
        {
            SentenceMappings = new List<SentenceMapping>();
            OriginalSentenceRuns = new List<Language.MappingSentenceRun>();
            OriginalWordRuns = new List<MappingWordRun>();
            OriginalTextBlocks = new List<TextBlock>();
            RawOriginalTextBlocks = new List<TextBlock>();

            if (studyItems == null)
                return false;

            int paragraphCount = studyItems.Count();
            int parentStartWordIndex = 0;
            int wordCount;
            MultiLanguageItem studyItem;
            LanguageItem languageItem;
            int originalSentenceIndex = 0;

            if (Statistics != null)
                Statistics.SetupCodeLabel("Original");

            for (int studyItemIndex = 0; studyItemIndex < paragraphCount; studyItemIndex++)
            {
                studyItem = studyItems[studyItemIndex];

                if (studyItem == null)
                    continue;

                languageItem = studyItem.LanguageItem(TargetLanguageID);

                int studyItemSentenceCount = languageItem.SentenceRunCount();
                int studyItemSentenceIndex;

                if ((languageItem == null) || (studyItemSentenceCount == 0))
                    continue;

                for (studyItemSentenceIndex = 0; studyItemSentenceIndex < studyItemSentenceCount; studyItemSentenceIndex++)
                {
                    TextRun studyItemSentenceRun = languageItem.GetSentenceRun(studyItemSentenceIndex);

                    if (Statistics != null)
                        Statistics.SetupSentencePath(studyItem, studyItemSentenceIndex);

                    if (studyItemSentenceRun == null)
                        continue;

                    int studyItemSentenceWordRunStartIndex;
                    int studyItemSentenceWordRunCount;

                    if (!languageItem.GetSentenceWordRunStartIndexAndCount(
                            studyItemSentenceIndex,
                            out studyItemSentenceWordRunStartIndex,
                            out studyItemSentenceWordRunCount))
                        return false;

                    List<TextRun> studyWordRuns = languageItem.WordRuns.GetRange(
                        studyItemSentenceWordRunStartIndex,
                        studyItemSentenceWordRunCount);
                    List<MappingWordRun> mappingWordRuns = new List<MappingWordRun>();
                    int wordIndex;
                    int wordStartIndexNoSeparators = 0;
                    int rawSentenceTextStartIndex = studyItemSentenceRun.Start;
                    int rawSentenceTextLength = studyItemSentenceRun.Length;
                    string rawSentenceText = languageItem.GetRunText(studyItemSentenceRun);
                    int textIndex = 0;
                    int rawTextIndex = studyItemSentenceRun.Start;
                    StringBuilder sb = new StringBuilder();

                    TextBlock textBlock = new TextBlock(OriginalTextBlocks.Count().ToString(), rawSentenceText);
                    OriginalTextBlocks.Add(textBlock);

                    TextBlock rawTextBlock = new TextBlock(RawOriginalTextBlocks.Count().ToString(), rawSentenceText);
                    RawOriginalTextBlocks.Add(rawTextBlock);

                    for (wordIndex = 0; wordIndex < studyItemSentenceWordRunCount; wordIndex++)
                    {
                        TextRun studyWordRun = studyWordRuns[wordIndex];
                        string rawWordText = languageItem.GetRunText(studyWordRun);
                        string wordText = TargetLanguageTool.StandardizeWord(rawWordText);
                        int wordLength = wordText.Length;
                        int textStartIndex = studyWordRun.Start - studyItemSentenceRun.Start;
                        int rawWordLength = studyWordRun.Length;
                        int parentWordIndex = parentStartWordIndex + wordIndex;

                        if (studyWordRun.Start > rawTextIndex)
                        {
                            int separatorLength = studyWordRun.Start - rawTextIndex;
                            sb.Append(languageItem.Text.Substring(rawTextIndex, separatorLength));
                            rawTextIndex += separatorLength;
                            textIndex += separatorLength;
                        }

                        MappingWordRun mappingWordRun = new MappingWordRun(
                            wordText,
                            rawWordText,
                            textIndex,
                            rawTextIndex,
                            textIndex,
                            wordStartIndexNoSeparators,
                            parentWordIndex,
                            wordIndex);

                        mappingWordRuns.Add(mappingWordRun);
                        wordStartIndexNoSeparators += wordLength;

                        sb.Append(wordText);
                        rawTextIndex += rawWordText.Length;
                        textIndex += wordText.Length;
                    }

                    if (rawTextIndex < studyItemSentenceRun.Stop)
                        sb.Append(languageItem.Text.Substring(rawTextIndex, studyItemSentenceRun.Stop - rawTextIndex));

                    // Replace text block text with standardized text.
                    textBlock.Text = sb.ToString();

                    // Do abbreviation and number normalizations.
                    HandleWordSubstitutions(mappingWordRuns, textBlock);

                    int sentenceTextStartIndex = 0;
                    int sentenceTextLength = 0;
                    string sentenceText = String.Empty;

                    if (mappingWordRuns.Count() != 0)
                    {
                        sentenceTextStartIndex = mappingWordRuns.First().TextStartIndex;
                        int sentenceTextStopIndex = mappingWordRuns.Last().TextStopIndex;
                        sentenceTextLength = sentenceTextStopIndex - sentenceTextStartIndex;
                        sentenceText = textBlock.Substring(sentenceTextStartIndex, sentenceTextLength);
                    }

                    MappingSentenceRun originalSentence = new MappingSentenceRun(
                        sentenceTextStartIndex,
                        sentenceTextLength,
                        sentenceText,
                        rawSentenceText,
                        mappingWordRuns,
                        parentStartWordIndex);

                    OriginalSentenceRuns.Add(new Language.MappingSentenceRun(originalSentence));

                    SentenceMapping sentenceMapping = new SentenceMapping(
                        originalSentenceIndex,
                        originalSentence,
                        studyItem,
                        studyItemIndex,
                        studyItemSentenceRun,
                        studyItemSentenceIndex,
                        parentStartWordIndex);

                    SentenceMappings.Add(sentenceMapping);

                    OriginalWordRuns.AddRange(sentenceMapping.OriginalSentence.WordRuns);
                    wordCount = sentenceMapping.OriginalSentence.WordCount;
                    parentStartWordIndex += wordCount;
                    originalSentenceIndex++;
                }
            }

            return true;
        }

        // Initialize transcribed word runs.
        // This function populated the TranscribedWordRuns list.
        protected bool InitializeTranscribedTextAndWordRuns(
            string transcribedText,
            List<TextRun> transcribedTextRuns)
        {
            int wordIndex = 0;
            int wordStartIndexNoSeparators = 0;

            TranscribedTextBlock = new TextBlock(transcribedText);
            RawTranscribedTextBlock = new TextBlock(transcribedText);
            TranscribedWordRuns = new List<MappingWordRun>();
            int textIndex = 0;
            int rawTextIndex = 0;
            StringBuilder sb = new StringBuilder();

            if (Statistics != null)
            {
                Statistics.SetupCodeLabel("Transcribed");
                Statistics.SetupSentencePath(null, -1);
            }

            foreach (TextRun wordRun in transcribedTextRuns)
            {
                int wordStartIndex = wordRun.Start;
                int rawWordLength = wordRun.Length;
                TimeSpan wordStartTime;
                TimeSpan wordStopTime;

                string rawWordText = transcribedText.Substring(wordStartIndex, rawWordLength);
                string wordText = StandardizeWord(rawWordText);
                int wordLength = wordText.Length;

                // Probably was a '-'.
                if (wordLength == 0)
                    continue;

                MediaRun mediaRun = wordRun.MediaRuns.FirstOrDefault();

                if (mediaRun != null)
                {
                    wordStartTime = mediaRun.Start;
                    wordStopTime = mediaRun.Stop;
                }
                else
                {
                    wordStartTime = TimeSpan.Zero;
                    wordStopTime = TimeSpan.Zero;
                }

                if (wordRun.Start > rawTextIndex)
                {
                    int separatorLength = wordRun.Start - rawTextIndex;
                    sb.Append(transcribedText.Substring(rawTextIndex, separatorLength));
                    rawTextIndex += separatorLength;
                    textIndex += separatorLength;
                }

                MappingWordRun transcribedWordRun = new MappingWordRun(
                    wordText,
                    rawWordText,
                    textIndex,
                    rawTextIndex,
                    textIndex,
                    wordStartIndexNoSeparators,
                    wordIndex,
                    wordIndex,
                    wordStartTime,
                    wordStopTime);

                TranscribedWordRuns.Add(transcribedWordRun);

                wordStartIndexNoSeparators += wordLength;
                wordIndex++;

                sb.Append(wordText);
                rawTextIndex += rawWordText.Length;
                textIndex += wordText.Length;
            }

            if (rawTextIndex < transcribedText.Length)
                sb.Append(transcribedText.Substring(rawTextIndex, transcribedText.Length - rawTextIndex));

            // Replace text block text with standardized text.
            TranscribedTextBlock.Text = sb.ToString();

            // Do abbreviation and number normalizations.
            HandleWordSubstitutions(TranscribedWordRuns, TranscribedTextBlock);

            return true;
        }

        // Process any needed substitutions for a list of word runs.
        // Returns true if any substitutions were made.
        public bool HandleWordSubstitutions(
            List<MappingWordRun> wordRuns,
            TextBlock textBlock)                // For transcribed sentence, of null if original
        {
            bool returnValue = false;

            // Handle abbreviation substitutions.
            if (HandleAbbreviationWordSubstitutions(wordRuns, textBlock))
                returnValue = true;

            // Handle number substitutions.
            if (HandleNumberWordSubstitutions(wordRuns, textBlock))
                returnValue = true;

            return returnValue;
        }

        // Process any needed abbreviation substitutions for a list of word runs.
        // Abbreviation expansions are replaced with standard abbreviations.
        // Returns true if any substitutions were made.
        public bool HandleAbbreviationWordSubstitutions(
            List<MappingWordRun> wordRuns,
            TextBlock textBlock)                // For transcribed sentence, of null if original
        {
            int wordCount = wordRuns.Count();
            int wordIndex;
            int substitutedWordCount;
            bool returnValue = false;

            for (wordIndex = 0; wordIndex < wordCount; )
            {
                // Check for any abbreviation substitutions at the current index.
                if (HandleAbbreviationWordSubstitutionToExpansion(
                    wordRuns,
                    wordIndex,
                    ref wordCount,
                    out substitutedWordCount,
                    textBlock))
                {
                    wordIndex += substitutedWordCount;
                    returnValue = true;
                }
                else
                    wordIndex++;
            }

            return returnValue;
        }

        // Process any needed abbreviation substitutions for the given index in a list of word runs.
        // Abbreviations are replaced with the abbreviation expansions.
        // Returns true if any substitutions were made.
        public bool HandleAbbreviationWordSubstitutionToExpansion(
            List<MappingWordRun> wordRuns,
            int wordIndex,
            ref int wordCount,
            out int substitutedWordCount,
            TextBlock textBlock)
        {
            bool returnValue = false;

            substitutedWordCount = 0;

            MappingWordRun abbreviatedWordRun = wordRuns[wordIndex];
            string abbreviation = abbreviatedWordRun.WordText;
            string[] expansion;

            if (AbbreviationStandardizedDictionary.TryGetValue(abbreviation, out expansion))
            {
                MappingWordRun oldWordRun = wordRuns[wordIndex];
                List<MappingWordRun> newWordRuns = new List<Language.MappingWordRun>();
                int expansionCharacterIndex = 0;
                int expansionCharacterPosition = 0;
                int expansionCharacterCount = TextUtilities.SumStringArrayLengths(expansion);
                int expansionLength = expansion.Length;
                int i;

                for (i = 0; i < expansionLength; i++)
                {
                    string expansionWord = expansion[i];
                    MappingWordRun wordRun = new MappingWordRun(
                        oldWordRun,
                        expansionWord,
                        expansionCharacterIndex,
                        expansionCharacterPosition,
                        expansionCharacterCount,
                        i);
                    newWordRuns.Add(wordRun);
                    expansionCharacterIndex += expansionWord.Length + 1;
                    expansionCharacterPosition += expansionWord.Length;
                }

                ReplaceWordRun(
                    wordRuns,
                    wordIndex,
                    newWordRuns);

                string abbreviationExpansion = TextUtilities.GetConcatenatedStringFromStringArray(expansion, " ");

                textBlock.Text = textBlock.Text.Remove(
                    oldWordRun.TextStartIndex,
                    oldWordRun.TextLength);
                textBlock.Text = textBlock.Text.Insert(
                    oldWordRun.TextStartIndex,
                    abbreviationExpansion);

                wordCount += expansionLength - 1;
                substitutedWordCount = expansionLength;
                returnValue = true;

                if (Statistics != null)
                    Statistics.DumpAbbreviationNormalization(abbreviation, abbreviationExpansion, wordIndex);
            }

            return returnValue;
        }

#if false   // We normalize to expanded abbreviations.
        // Process any needed abbreviation substitutions for the given index in a list of word runs.
        // Abbreviation expansions are replaced with standard abbreviations.
        // Returns true if any substitutions were made.
        public bool HandleAbbreviationWordSubstitutionToAbbreviation(
            List<MappingWordRun> wordRuns,
            int wordIndex,
            ref int wordCount,
            out int substitutedWordCount,
            TextBlock textBlock)
        {
            bool returnValue = false;

            substitutedWordCount = 0;

            MappingWordRun firstWordRun = wordRuns[wordIndex];
            string firstWord = firstWordRun.WordText;
            List<KeyValuePair<string, string[]>> abbreviationDefinitions;

            if (AbbreviationFirstWordDictionary.TryGetValue(firstWord, out abbreviationDefinitions))
            {
                foreach (KeyValuePair<string, string[]> abbreviationDefinition in abbreviationDefinitions)
                {
                    string abbrev = abbreviationDefinition.Key;
                    string[] expansion = abbreviationDefinition.Value;
                    int expansionLength = expansion.Length;
                    int i;
                    bool matched = true;

                    for (i = 1; i < expansionLength; i++)
                    {
                        if (i >= wordCount)
                        {
                            matched = false;
                            break;
                        }

                        MappingWordRun wordRun = wordRuns[wordIndex + i];
                        string runWord = wordRun.WordText;
                        string expansionWord = expansion[i];

                        if (runWord != expansionWord)
                        {
                            matched = false;
                            break;
                        }
                    }

                    if (matched)
                    {
                        ReplaceWordRunRange(wordRuns, wordIndex, expansionLength, abbrev);

                        MappingWordRun lastWordRun = wordRuns[wordIndex + expansionLength - 1];
                        int oldPhraseLength = lastWordRun.SentenceTextStopIndex - firstWordRun.SentenceTextStartIndex;

                        textBlock.Text = textBlock.Text.Remove(
                            firstWordRun.TextStartIndex,
                            oldPhraseLength);
                        textBlock.Text = textBlock.Text.Insert(
                            firstWordRun.TextStartIndex,
                            abbrev);

                        wordCount -= expansionLength - 1;
                        substitutedWordCount = expansionLength;
                        returnValue = true;
                        break;
                    }
                }
            }

            return returnValue;
        }
#endif

        // Process any needed number substitutions for a list of word runs.
        // Spelled-out numbers are replaced with digits.
        // Returns true if any substitutions were made.
        public bool HandleNumberWordSubstitutions(
            List<MappingWordRun> wordRuns,
            TextBlock textBlock)                // For transcribed sentence, of null if original)
        {
            int wordCount = wordRuns.Count();
            int wordIndex;
            int substitutedWordCount;
            bool returnValue = false;

            for (wordIndex = 0; wordIndex < wordCount;)
            {
                // Check for any number substitutions at the current index.
                if (HandleDigitSubstitutionToExpansion(
                    wordRuns,
                    wordIndex,
                    ref wordCount,
                    out substitutedWordCount,
                    textBlock))
                {
                    wordIndex += substitutedWordCount;
                    returnValue = true;
                }
                else
                    wordIndex++;
            }

            return returnValue;
        }

        // Process any needed number substitutions for the given index in a list of word runs.
        // Expaneded (non-digit) numbers are replaced with the spelled-out.
        // Returns true if any substitutions were made.
        public bool HandleDigitSubstitutionToExpansion(
            List<MappingWordRun> wordRuns,
            int wordIndex,
            ref int wordCount,
            out int substitutedWordCount,
            TextBlock textBlock)
        {
            bool returnValue = false;

            substitutedWordCount = 0;

            int wordRunCount = wordRuns.Count();
            MappingWordRun wordRun = wordRuns[wordIndex];
            string word = wordRun.WordText;
            bool isExpandedNumberString = TargetLanguageTool.IsExpandedNumberString(word);
            bool isDigitNumberString = TargetLanguageTool.IsNumberString(word);

            if (isExpandedNumberString || isDigitNumberString)
            {
                int numberRunLength;
                string matchedNumberString = String.Empty;
                string rawNumberString = word;
                MappingWordRun prevWordRun = wordRun;

                if (isExpandedNumberString)
                {
                    List<string> numberStrings = new List<string>() { word };

                    for (numberRunLength = 1; wordIndex + numberRunLength < wordRunCount; numberRunLength++)
                    {
                        wordRun = wordRuns[wordIndex + numberRunLength];

                        string separator = textBlock.Substring(
                            prevWordRun.TextStopIndex,
                            wordRun.TextStartIndex - prevWordRun.TextStopIndex);

                        // If not whitespace between numbers, treat it as a separate number.
                        if (!String.IsNullOrEmpty(separator.Trim()))
                            break;

                        word = wordRun.WordText;

                        if (!TargetLanguageTool.IsExpandedNumberString(word))
                            break;

                        numberStrings.Add(word);

                        rawNumberString += separator + word;
                        prevWordRun = wordRun;
                    }

                    matchedNumberString = TargetLanguageTool.GetDigitsStringFromNumberExpansionList(numberStrings);
                }
                else
                {
                    matchedNumberString = word;

                    for (numberRunLength = 1; wordIndex + numberRunLength < wordRunCount; numberRunLength++)
                    {
                        wordRun = wordRuns[wordIndex + numberRunLength];
                        word = wordRun.WordText;

                        if (!TargetLanguageTool.IsNumberString(word))
                            break;

                        string separator = textBlock.Substring(
                            prevWordRun.TextStopIndex,
                            wordRun.TextStartIndex - prevWordRun.TextStopIndex);

                        if (separator != ",")
                            break;

                        matchedNumberString += word;

                        rawNumberString += separator + word;
                        prevWordRun = wordRun;
                    }
                }

                MappingWordRun firstWordRun = wordRuns[wordIndex];
                MappingWordRun lastWordRun = wordRuns[wordIndex + numberRunLength - 1];
                int expansionCharacterIndex = 0;
                int expansionCharacterPosition = 0;
                int runLength = matchedNumberString.Length;
                string oldText = textBlock.Substring(
                    firstWordRun.TextStartIndex,
                    lastWordRun.TextStopIndex - firstWordRun.TextStartIndex);
                List<MappingWordRun> newWordRuns = new List<MappingWordRun>();
                string newNumberString = String.Empty;

#if true
                int expansionCharacterCount = 0;
                string[] standardNumberExpansion = TargetLanguageTool.GetNumberExpansionStandard(matchedNumberString);

                if (standardNumberExpansion != null)
                {
                    runLength = standardNumberExpansion.Length;

                    // Standardize expansion and get total character count.
                    for (int i = 0; i < runLength; i++)
                    {
                        string numberPart = StandardizeWord(standardNumberExpansion[i]);
                        standardNumberExpansion[i] = numberPart;
                        expansionCharacterCount += numberPart.Length;
                    }

                    // Create word runs for digit names.
                    for (int i = 0; i < runLength; i++)
                    {
                        string numberPart = standardNumberExpansion[i];
                        int numberPartLength = numberPart.Length;
                        MappingWordRun newWordRun = new MappingWordRun(
                            firstWordRun,
                            numberPart,
                            expansionCharacterIndex,
                            expansionCharacterPosition,
                            expansionCharacterCount,
                            i);

                        newWordRuns.Add(newWordRun);
                        expansionCharacterIndex += numberPartLength + 1;
                        expansionCharacterPosition += numberPartLength;

                        if (!String.IsNullOrEmpty(newNumberString))
                            newNumberString += " ";

                        newNumberString += numberPart;
                    }
                }
                else
                {
                    PutError("Number error", matchedNumberString);
                    newNumberString = matchedNumberString;
                }
#elif true
                int expansionCharacterCount = 1;
                // Create word runs for digit names.
                for (int i = 0; i < runLength; i++)
                {
                    string digitName = StandardizeWord(TargetLanguageTool.GetDigitName(matchedNumberString.Substring(i, 1)));
                    MappingWordRun newWordRun = new MappingWordRun(
                        firstWordRun,
                        digitName,
                        expansionCharacterIndex,
                        expansionCharacterPosition,
                        expansionCharacterCount,
                        i);

                    newWordRuns.Add(newWordRun);
                    expansionCharacterCount = digitName.Length;
                    expansionCharacterIndex += expansionCharacterCount + 1;
                    expansionCharacterPosition += expansionCharacterCount;

                    if (!String.IsNullOrEmpty(newNumberString))
                        newNumberString += " ";

                    newNumberString += digitName;
                }
#else
                int expansionCharacterCount = 1;
                // Create word runs for digits.
                for (int i = 0; i < runLength; i++)
                {
                    string digit = matchedNumberString.Substring(i, 1);
                    MappingWordRun newWordRun = new MappingWordRun(
                        firstWordRun,
                        digit,
                        expansionCharacterIndex,
                        expansionCharacterPosition,
                        expansionCharacterCount,
                        i);

                    newWordRuns.Add(newWordRun);
                    expansionCharacterIndex += expansionCharacterCount + 1;
                    expansionCharacterPosition += expansionCharacterCount;

                    if (!String.IsNullOrEmpty(newNumberString))
                        newNumberString += " ";

                    newNumberString += digit;
                }
#endif

                ReplaceWordRunRange(
                    wordRuns,
                    wordIndex,
                    numberRunLength,
                    newWordRuns);

                textBlock.Text = textBlock.Text.Remove(
                    firstWordRun.TextStartIndex,
                    lastWordRun.TextStopIndex - firstWordRun.TextStartIndex);
                textBlock.Text = textBlock.Text.Insert(
                    firstWordRun.TextStartIndex,
                    newNumberString);

                wordCount += newWordRuns.Count() - numberRunLength;
                substitutedWordCount = newWordRuns.Count();
                returnValue = true;

                if (Statistics != null)
                    Statistics.DumpNumberNormalization(rawNumberString, newNumberString, wordIndex);
            }

            return returnValue;
        }

        // Dictionary of abbreviations that have been standardized.
        private Dictionary<string, string[]> AbbreviationStandardizedDictionary
        {
            get
            {
                if (_AbbreviationStandardizedDictionary == null)
                    InitializeAbbreviationStandardized();

                return _AbbreviationStandardizedDictionary;
            }
        }

        // Initialize dictionary of abbreviations that have been standardized.
        public void InitializeAbbreviationStandardized()
        {
            Dictionary<string, string> abbreviationDictionary = TargetLanguageTool.AbbreviationDictionary;

            _AbbreviationStandardizedDictionary = new Dictionary<string, string[]>();

            if (abbreviationDictionary == null)
                return;

            foreach (KeyValuePair<string, string> kvp in abbreviationDictionary)
            {
                string abbrev = StandardizeWord(kvp.Key);
                string[] expansion = kvp.Value.Split(LanguageLookup.SpaceCharacters, StringSplitOptions.RemoveEmptyEntries);
                int c = expansion.Length;

                for (int i = 0; i < c; i++)
                    expansion[i] = StandardizeWord(expansion[i]);

                _AbbreviationStandardizedDictionary.Add(abbrev, expansion);
            }
        }

        // Dictionary of abbreviation definitions, keyed on first word of expansion.
        private Dictionary<string, List<KeyValuePair<string, string[]>>> AbbreviationFirstWordDictionary
        {
            get
            {
                if (_AbbreviationFirstWordDictionary == null)
                    InitializeAbbreviationFirstWord();

                return _AbbreviationFirstWordDictionary;
            }
        }

        // Initialize dictionary of abbreviation definitions, keyed on first word of expansion.
        public void InitializeAbbreviationFirstWord()
        {
            Dictionary<string, string[]> abbreviationDictionary = AbbreviationStandardizedDictionary;

            _AbbreviationFirstWordDictionary = new Dictionary<string, List<KeyValuePair<string, string[]>>>();

            foreach (KeyValuePair<string, string[]> abbreviationDefinition in abbreviationDictionary)
            {
                string[] expansion = abbreviationDefinition.Value;
                string key = expansion[0];
                List<KeyValuePair<string, string[]>> abbreviationDefinitions;

                if (_AbbreviationFirstWordDictionary.TryGetValue(key, out abbreviationDefinitions))
                    abbreviationDefinitions.Add(abbreviationDefinition);
                else
                {
                    abbreviationDefinitions = new List<KeyValuePair<string, string[]>>() { abbreviationDefinition };
                    _AbbreviationFirstWordDictionary.Add(key, abbreviationDefinitions);
                }
            }
        }

        // Standardize word.
        // Lowercase, no accents, no spaces, no punctuation.
        // Relay to language tool.
        public string StandardizeWord(string word)
        {
            return TargetLanguageTool.StandardizeWord(word);
        }

        // Map one sentence.
        public bool MapSentence(
            List<SentenceMapping> sentenceMappings,
            List<MappingWordRun> originalWordRuns,
            List<TextBlock> originalTextBlocks,
            List<TextBlock> rawOriginalTextBlocks,
            List<MappingWordRun> transcribedWordRuns,
            TextBlock transcribedTextBlock,
            int sentenceIndex,
            ref int transcribedParentStartWordIndex)
        {
            SentenceMapping sentenceMapping = sentenceMappings[sentenceIndex];
            MappingSentenceRun originalSentence = sentenceMapping.OriginalSentence;
            int originalParentStartWordIndex = originalSentence.ParentStartWordIndex;
            int originalSentenceWordRunCount = originalSentence.WordCount;
            int sentenceMappingCount = sentenceMappings.Count();
            int originalWordRunCount = originalWordRuns.Count();
            int transcribedWordRunCount = transcribedWordRuns.Count();
            MappingSentenceRun originalExtraSentenceRun = null;
            int transcribedEndWordIndex;

            if (Statistics != null)
                Statistics.SetupSentencePath(sentenceMapping.StudyItem, sentenceMapping.SentenceIndex);

            if (sentenceIndex + 1 < sentenceMappingCount)
                originalExtraSentenceRun = CreateOriginalExtraSentence(
                    sentenceMappings,
                    originalWordRuns,
                    originalTextBlocks,
                    rawOriginalTextBlocks,
                    sentenceIndex + 1);

            sentenceMapping.OriginalExtraSentence = originalExtraSentenceRun;

            transcribedEndWordIndex = CalculateTranscribedCandidateEndWordIndex(
                originalParentStartWordIndex,
                originalSentenceWordRunCount,
                originalWordRunCount,
                transcribedParentStartWordIndex,
                transcribedWordRunCount);

            int transcribedSentenceWordCount = transcribedEndWordIndex - transcribedParentStartWordIndex;
            int transcribedSentenceWordStartIndex = transcribedParentStartWordIndex;

            FindBestSentenceCandidate(
                sentenceMapping,        // Will have transcribed sentences set to best candidate afterwards.
                SentenceMappings,
                OriginalWordRuns,
                TranscribedWordRuns,
                TranscribedTextBlock,
                sentenceIndex,
                ref transcribedParentStartWordIndex,
                ref transcribedSentenceWordCount);

            // Check for and do resync.
            DoSentenceResyncCheck(
                sentenceMapping,
                SentenceMappings,
                OriginalWordRuns,
                TranscribedWordRuns,
                TranscribedTextBlock,
                sentenceIndex,
                transcribedSentenceWordStartIndex,
                ref transcribedParentStartWordIndex);

            if (Statistics != null)
                Statistics.UpdateSums(sentenceMapping);

            if (!sentenceMapping.Ignored)
            {
                MarkTranscribedRunsUsed(sentenceMapping);
                GetRawTranscribedSentenceText(sentenceMapping);
            }

            return true;
        }

        // Finds best sentence candidate, copying over given sentence mapping
        // Return false if at end of transcribed runs.
        public bool FindBestSentenceCandidate(
            SentenceMapping sentenceMapping,
            List<SentenceMapping> sentenceMappings,
            List<MappingWordRun> originalWordRuns,
            List<MappingWordRun> transcribedWordRuns,
            TextBlock transcribedTextBlock,
            int sentenceIndex,
            ref int transcribedParentStartWordIndex,
            ref int transcribedSentenceWordCount)
        {
            int transcribedWordCount = transcribedWordRuns.Count();
            int transcribedWordCountRemainder = transcribedWordCount - transcribedParentStartWordIndex;
            SentenceMapping baseSentenceCandidate = CreateAndScoreCandidateSentence(
                sentenceMapping,
                transcribedWordRuns,
                transcribedTextBlock,
                transcribedParentStartWordIndex,
                transcribedSentenceWordCount);
            SentenceMapping sentenceCandidate = null;
            int transcribedSentenceCandidateWordCount = transcribedSentenceWordCount;
            int transcribedSentenceCandidateWordHighCount = transcribedWordCountRemainder;
            int transcribedSentenceCandidateWordLowCount = 1;
            double bestMainScore = baseSentenceCandidate.MainScore;
            double bestCombinedScore = baseSentenceCandidate.CombinedScore;
            int bestTranscribedSentenceWordCount = transcribedSentenceWordCount;
            SentenceMapping bestSentenceCandidate = baseSentenceCandidate;
            bool returnValue = true;

            DumpDebugWordMappingResultsLabel("############### Finding best sentence candidate:");

            DumpDebugWordMappingCandidateState(
                "Initial candidate",
                "",
                0,
                baseSentenceCandidate);

            if (bestMainScore != 1.0)
            {
                for (int loop = 0; loop < 2; loop++)
                {
                    int delta = (loop == 0 ? 1 : -1);
                    double lastMainScore = baseSentenceCandidate.MainScore;
                    double lastCombinedScore = baseSentenceCandidate.CombinedScore;
                    int worseCount = 0;

                    for (transcribedSentenceCandidateWordCount = transcribedSentenceWordCount + delta;
                        (transcribedSentenceCandidateWordCount >= transcribedSentenceCandidateWordLowCount) &&
                            (transcribedSentenceCandidateWordCount < transcribedSentenceCandidateWordHighCount);
                        transcribedSentenceCandidateWordCount += delta)
                    {
                        sentenceCandidate = CreateAndScoreCandidateSentence(
                            sentenceMapping,
                            transcribedWordRuns,
                            transcribedTextBlock,
                            transcribedParentStartWordIndex,
                            transcribedSentenceCandidateWordCount);

                        double combinedScore = sentenceCandidate.CombinedScore;
                        double mainScore = sentenceCandidate.MainScore;

                        if (mainScore == 1.0)
                        {
                            bestMainScore = mainScore;
                            bestCombinedScore = combinedScore;
                            bestTranscribedSentenceWordCount = transcribedSentenceCandidateWordCount;
                            bestSentenceCandidate = sentenceCandidate;

                            DumpDebugWordMappingCandidateState(
                                "Main score was 1.0",
                                "",
                                bestTranscribedSentenceWordCount,
                                sentenceCandidate);

                            DumpDebugWordMappingResultsLabel("Exiting inner loop, MainScore was 1.");

                            break;
                        }

                        if (combinedScore > bestCombinedScore)
                        {
                            bestMainScore = mainScore;
                            bestCombinedScore = combinedScore;
                            bestTranscribedSentenceWordCount = transcribedSentenceCandidateWordCount;
                            bestSentenceCandidate = sentenceCandidate;

                            DumpDebugWordMappingCandidateState(
                                "New best candidate",
                                "",
                                bestTranscribedSentenceWordCount,
                                sentenceCandidate);

                            worseCount = 0;
                        }
                        else if (combinedScore == bestCombinedScore)
                        {
                            DumpDebugWordMappingCandidateState(
                                "Equal to best candidate",
                                "",
                                bestTranscribedSentenceWordCount,
                                sentenceCandidate);

                            worseCount = 0;
                        }
                        else
                        {
                            DumpDebugWordMappingCandidateState(
                                "Not a better candidate",
                                "",
                                bestTranscribedSentenceWordCount,
                                sentenceCandidate);
                        }

                        if (combinedScore == 1.0)
                        {
                            DumpDebugWordMappingResultsLabel("Exiting inner loop, CombinedScore=" + combinedScore.ToString() +
                                ", MainScore=" + mainScore.ToString());
                            break;
                        }

                        if ((mainScore < lastMainScore) && (combinedScore < lastCombinedScore))
                        {
                            worseCount++;

                            if (worseCount == 3)
                            {
                                DumpDebugWordMappingResultsLabel("Exiting inner loop, both scores less than last." +
                                    " WorseCount=" + worseCount.ToString() +
                                    " CombinedScore=" + combinedScore.ToString() + ", MainScore=" + mainScore.ToString() +
                                    " LastCombinedScore=" + lastCombinedScore.ToString() + ", LastMainScore=" + lastMainScore.ToString());
                                break;
                            }
                            else
                            {
                                DumpDebugWordMappingResultsLabel("Both scores worse." +
                                    " WorseCount=" + worseCount.ToString() +
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

                // If enabled, walk sentence start to find best candidate.
                if (WalkSentenceStart)
                {
                    DumpDebugWordMappingCandidateState(
                        "Chosen candidate for end walking",
                        "",
                        bestTranscribedSentenceWordCount,
                        bestSentenceCandidate);

                    FindBestSentenceCandidateStartWalking(
                        bestSentenceCandidate,
                        sentenceMappings,
                        originalWordRuns,
                        transcribedWordRuns,
                        transcribedTextBlock,
                        sentenceIndex,
                        ref bestTranscribedSentenceWordCount);
                }

                // Copy best candidate over given sentence mapping.
                sentenceMapping.CopySentenceMappingShallow(bestSentenceCandidate);

                // Update reference arguments.
                transcribedParentStartWordIndex = bestSentenceCandidate.TranscribedSentence.ParentStopWordIndex;
                transcribedSentenceWordCount = bestTranscribedSentenceWordCount;

                DumpDebugWordMappingCandidateState(
                    "Chosen candidate",
                    "",
                    bestTranscribedSentenceWordCount,
                    sentenceMapping);
            }
            else
            {
                // Copy best candidate over given sentence mapping.
                sentenceMapping.CopySentenceMappingShallow(bestSentenceCandidate);

                // Update reference arguments.
                transcribedParentStartWordIndex = bestSentenceCandidate.TranscribedSentence.ParentStopWordIndex;
                transcribedSentenceWordCount = bestTranscribedSentenceWordCount;

                DumpDebugWordMappingResultsLabel("Exiting early because main score is 1.");
            }

            return returnValue;
        }

        // Finds best sentence candidate by walking the start, copying over given sentence mapping
        public bool FindBestSentenceCandidateStartWalking(
            SentenceMapping sentenceMapping,
            List<SentenceMapping> sentenceMappings,
            List<MappingWordRun> originalWordRuns,
            List<MappingWordRun> transcribedWordRuns,
            TextBlock transcribedTextBlock,
            int sentenceIndex,
            ref int transcribedSentenceWordCount)
        {
            SentenceMapping baseSentenceCandidate = sentenceMapping;
            SentenceMapping sentenceCandidate = null;
            int transcribedSentenceCandidateWordCount;
            double bestMainScore = baseSentenceCandidate.MainScore;
            double bestCombinedScore = baseSentenceCandidate.CombinedScore;
            int bestTranscribedSentenceWordCount;
            SentenceMapping bestSentenceCandidate = baseSentenceCandidate;
            int transcribedSentenceCandidateWordHighCount;
            int transcribedSentenceCandidateWordLowCount;
            int transcribedParentStartWordIndex;
            int transcribedParentStopWordIndex;
            bool returnValue = true;

            if (baseSentenceCandidate.TranscribedSentence == null)
                return false;

            transcribedSentenceWordCount = baseSentenceCandidate.TranscribedSentence.WordCount;
            bestTranscribedSentenceWordCount = transcribedSentenceWordCount;
            transcribedSentenceCandidateWordHighCount = transcribedSentenceWordCount + 5;
            transcribedSentenceCandidateWordLowCount = transcribedSentenceWordCount - 5;
            transcribedParentStopWordIndex = baseSentenceCandidate.TranscribedSentence.ParentStopWordIndex;

            if (transcribedSentenceCandidateWordLowCount < 1)
                transcribedSentenceCandidateWordLowCount = 1;

            if (transcribedParentStopWordIndex - transcribedSentenceCandidateWordHighCount < 0)
                transcribedSentenceCandidateWordHighCount = transcribedParentStopWordIndex;

            DumpDebugWordMappingResultsLabel("############### Finding best start walking sentence candidate:");

            if (bestMainScore != 1.0)
            {
                for (int loop = 0; loop < 2; loop++)
                {
                    int delta = (loop == 0 ? 1 : -1);
                    double lastMainScore = baseSentenceCandidate.MainScore;
                    double lastCombinedScore = baseSentenceCandidate.CombinedScore;
                    int worseCount = 0;

                    for (transcribedSentenceCandidateWordCount = transcribedSentenceWordCount + delta;
                        (transcribedSentenceCandidateWordCount >= transcribedSentenceCandidateWordLowCount) &&
                            (transcribedSentenceCandidateWordCount < transcribedSentenceCandidateWordHighCount);
                        transcribedSentenceCandidateWordCount += delta)
                    {
                        transcribedParentStartWordIndex = transcribedParentStopWordIndex - transcribedSentenceCandidateWordCount;

                        sentenceCandidate = CreateAndScoreCandidateSentence(
                            sentenceMapping,
                            transcribedWordRuns,
                            transcribedTextBlock,
                            transcribedParentStartWordIndex,
                            transcribedSentenceCandidateWordCount);

                        double combinedScore = sentenceCandidate.CombinedScore;
                        double mainScore = sentenceCandidate.MainScore;

                        if (mainScore == 1.0)
                        {
                            bestMainScore = mainScore;
                            bestCombinedScore = combinedScore;
                            bestTranscribedSentenceWordCount = transcribedSentenceCandidateWordCount;
                            bestSentenceCandidate = sentenceCandidate;

                            DumpDebugWordMappingCandidateState(
                                "Main score was 1.0 start walking",
                                "",
                                bestTranscribedSentenceWordCount,
                                sentenceCandidate);

                            DumpDebugWordMappingResultsLabel("Exiting start walking inner loop, MainScore was 1.");

                            break;
                        }

                        if (combinedScore > bestCombinedScore)
                        {
                            bestMainScore = mainScore;
                            bestCombinedScore = combinedScore;
                            bestTranscribedSentenceWordCount = transcribedSentenceCandidateWordCount;
                            bestSentenceCandidate = sentenceCandidate;

                            DumpDebugWordMappingCandidateState(
                                "New best start walking candidate",
                                "",
                                bestTranscribedSentenceWordCount,
                                sentenceCandidate);

                            worseCount = 0;
                        }
                        else if (combinedScore == bestCombinedScore)
                        {
                            DumpDebugWordMappingCandidateState(
                                "Equal to best candidate start walking",
                                "",
                                bestTranscribedSentenceWordCount,
                                sentenceCandidate);

                            worseCount = 0;
                        }
                        else
                        {
                            DumpDebugWordMappingCandidateState(
                                "Not a better candidate start walking",
                                "",
                                bestTranscribedSentenceWordCount,
                                sentenceCandidate);
                        }

                        if (combinedScore == 1.0)
                        {
                            DumpDebugWordMappingResultsLabel("Exiting start walking inner loop, CombinedScore=" + combinedScore.ToString() +
                                ", MainScore=" + mainScore.ToString());
                            break;
                        }

                        if ((mainScore < lastMainScore) && (combinedScore < lastCombinedScore))
                        {
                            worseCount++;

                            if (worseCount == 3)
                            {
                                DumpDebugWordMappingResultsLabel("Exiting start walking inner loop, both scores less than last." +
                                    " WorseCount=" + worseCount.ToString() +
                                    " CombinedScore=" + combinedScore.ToString() + ", MainScore=" + mainScore.ToString() +
                                    " LastCombinedScore=" + lastCombinedScore.ToString() + ", LastMainScore=" + lastMainScore.ToString());
                                break;
                            }
                            else
                            {
                                DumpDebugWordMappingResultsLabel("Both start walking scores worse." +
                                    " WorseCount=" + worseCount.ToString() +
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

                // Copy best candidate over given sentence mapping.
                sentenceMapping.CopySentenceMappingShallow(bestSentenceCandidate);
            }
            else
                DumpDebugWordMappingResultsLabel("Exiting start walking early because main score is 1.");

            return returnValue;
        }

        // Find next unused transcribed word run.
        // Returns false if at end.
        public bool FindNextUnusedWordRun(
            List<MappingWordRun> wordRuns,
            ref int index)
        {
            int count = wordRuns.Count();

            for (; index < count; index++)
            {
                if (!wordRuns[index].Used)
                    return true;
            }

            return false;
        }

        public enum ResyncReason
        {
            NoResync,
            MainScoreTooLow,
            UsedWords
        };

        // Check for need to do resync.
        public ResyncReason SentenceCandidateMeetsResyncRequirements(SentenceMapping sentenceMapping)
        {
            ResyncReason reason = ResyncReason.NoResync;

            if (sentenceMapping.MainScore < ResyncMainScoreThreshold)
                reason = ResyncReason.MainScoreTooLow;
            else if (!WalkSentenceStart)
            {
                // If first word has been consumed, trigger resync.
                MappingSentenceRun transcribedSentence = sentenceMapping.TranscribedSentence;

                if (transcribedSentence.WordCount != 0)
                {
                    MappingWordRun firstWordRun = transcribedSentence.GetWordRun(0);

                    if (firstWordRun != null)
                    {
                        if (firstWordRun.Used)
                            reason = ResyncReason.UsedWords;
                    }
                }
            }

            return reason;
        }

        // Check for resynce trigger and do resynce if triggered.
        // Returns true if did successful resync.
        // Returns false no resync triggered or resync was triggered but failed.
        public bool DoSentenceResyncCheck(
            SentenceMapping sentenceMapping,
            List<SentenceMapping> sentenceMappings,
            List<MappingWordRun> originalWordRuns,
            List<MappingWordRun> transcribedWordRuns,
            TextBlock transcribedTextBlock,
            int sentenceIndex,
            int transcribedParentStartWordIndexPrior,
            ref int transcribedParentStartWordIndex)
        {
            bool didResync = false;

            // Check for resync.
            ResyncReason resyncReason = SentenceCandidateMeetsResyncRequirements(sentenceMapping);

            if (resyncReason != ResyncReason.NoResync)
            {
                // Do actual resync.
                if (DoSentenceResync(
                        resyncReason,
                        sentenceMapping,
                        SentenceMappings,
                        OriginalWordRuns,
                        TranscribedWordRuns,
                        TranscribedTextBlock,
                        sentenceIndex,
                        transcribedParentStartWordIndexPrior,
                        ref transcribedParentStartWordIndex))
                    didResync = true;
            }

            return didResync;
        }

        // Do actual sentence resync.
        // Returns true if resync succeeded.
        // Regardless of whether resync succeeded, do sentence ignore check.
        public bool DoSentenceResync(
            ResyncReason reason,
            SentenceMapping sentenceMapping,
            List<SentenceMapping> sentenceMappings,
            List<MappingWordRun> originalWordRuns,
            List<MappingWordRun> transcribedWordRuns,
            TextBlock transcribedTextBlock,
            int sentenceIndex,
            int transcribedParentStartWordIndexPrior,
            ref int transcribedParentStartWordIndex)
        {
            MappingSentenceRun originalMasterSentence = OriginalSentenceRuns[sentenceMapping.OriginalSentenceIndex];
            MappingSentenceRun originalSentence = new MappingSentenceRun(originalMasterSentence);

            sentenceMapping.OriginalSentence = originalSentence;

            int transcribedSentenceWordCount = originalSentence.WordCount;
            int transcribedSentenceWordStartIndex;
            bool resyncSucceeded = false;

            sentenceMapping.RequiredResync = true;
            sentenceMapping.MainScorePriorToResync = sentenceMapping.MainScore;

            if (sentenceMapping.TranscribedSentence != null)
                sentenceMapping.BestCandidateSentencePriorToResync = sentenceMapping.TranscribedSentence.SentenceText;

            if (Statistics != null)
                Statistics.DumpResyncTraceStart(sentenceMapping);

            // Find best resync candidate.
            if (FindBestResyncSentenceCandidate(
                    reason,
                    sentenceMapping,
                    SentenceMappings,
                    OriginalWordRuns,
                    TranscribedWordRuns,
                    TranscribedTextBlock,
                    sentenceIndex,
                    out transcribedSentenceWordStartIndex,
                    out transcribedSentenceWordCount))
            {
                transcribedParentStartWordIndex = transcribedSentenceWordStartIndex;

                if (Statistics != null)
                    Statistics.DumpResyncTraceBestFound(sentenceMapping);

                // Find the best candiate at the resync position.
                FindBestSentenceCandidate(
                    sentenceMapping,        // Will have transcribed sentences set to best candidate afterwards.
                    SentenceMappings,
                    OriginalWordRuns,
                    TranscribedWordRuns,
                    TranscribedTextBlock,
                    sentenceIndex,
                    ref transcribedParentStartWordIndex,
                    ref transcribedSentenceWordCount);
            }
            else
            {
                if (Statistics != null)
                    Statistics.DumpResyncTraceFailed(sentenceMapping);
            }

            // Get the main score, possible resynced.
            double mainScore = sentenceMapping.MainScore;
            double combinedScore = sentenceMapping.CombinedScore;

            // Check for ignoring sentence.
            if (mainScore < IgnoreMainScoreThreshold)
            {
                sentenceMapping.Ignored = true;

                if (Statistics != null)
                    Statistics.DumpResyncIgnoringSentence(sentenceMapping);

                // Since we are ignoring it, clear out the transcribed sentence.
                sentenceMapping.TranscribedSentence = null;
                sentenceMapping.TranscribedExtraSentence = null;
                sentenceMapping.OriginalSentence.ResetMatches();

                if (sentenceMapping.OriginalExtraSentence != null)
                    sentenceMapping.OriginalExtraSentence.ResetMatches();

                // Reset transcribed start word index such that it goes back to the starting point.
                transcribedParentStartWordIndex = transcribedParentStartWordIndexPrior;
            }

            return resyncSucceeded;
        }

        public bool FindBestResyncSentenceCandidate(
            ResyncReason reason,
            SentenceMapping sentenceMapping,
            List<SentenceMapping> sentenceMappings,
            List<MappingWordRun> originalWordRuns,
            List<MappingWordRun> transcribedWordRuns,
            TextBlock transcribedTextBlock,
            int sentenceIndex,
            out int transcribedParentStartWordIndex,
            out int transcribedSentenceWordCount)
        {
            MappingSentenceRun originalSentence = sentenceMapping.OriginalSentence;
            MappingWordRun firstOriginalWordRun = originalSentence.GetWordRun(0);
            int originalSentenceWordStartIndex = originalSentence.ParentStartWordIndex;
            int originalSentenceWordCount = originalSentence.WordCount;
            int originalWordCount = originalWordRuns.Count();
            int transcribedWordCount = transcribedWordRuns.Count();
            int transcribedCandidateStartIndex;
            double priorMainScore = sentenceMapping.MainScore;

            transcribedParentStartWordIndex = -1;
            transcribedSentenceWordCount = originalSentenceWordCount;

            int transcribedCandidateStartLimit = transcribedWordCount - transcribedSentenceWordCount;
            double bestMainScore = -1.0;
            double bestFactoredScore = -1.0;
            int bestTranscribedStartIndex = -1;

            if (Statistics != null)
                PutConsoleMessage("Resyncing - " + Statistics.SentencePath);
            else
                PutConsoleMessage("Resyncing - sentence index " + sentenceIndex.ToString());

            // Find best transcribed candidate, string at the beginning of the transcribed word runs.
            for (transcribedCandidateStartIndex = 0;
                transcribedCandidateStartIndex < transcribedCandidateStartLimit;
                transcribedCandidateStartIndex++)
            {
                MappingSentenceRun originalSentenceCandidate = new MappingSentenceRun(originalSentence);

                firstOriginalWordRun = originalSentence.GetWordRun(0);
                originalSentenceWordCount = originalSentence.WordCount;

                MappingSentenceRun transcribedSentenceCandidate = CreateTranscribedSentence(
                    transcribedWordRuns,
                    transcribedTextBlock,
                    transcribedCandidateStartIndex,
                    transcribedSentenceWordCount);

                double mainScore = ScoreSentencePair(
                    originalSentenceCandidate,
                    transcribedSentenceCandidate);

                MappingWordRun firstTranscribedWordRun = transcribedSentenceCandidate.GetWordRun(0);
                double firstWordScore = CalculateWordMatchScore(
                    firstOriginalWordRun.WordText,
                    firstTranscribedWordRun.WordText,
                    firstOriginalWordRun.SentenceCharacterPosition,     // Will be 0
                    firstTranscribedWordRun.SentenceCharacterPosition,  // Will be 0
                    originalSentence.TextLengthNoSeparators,
                    transcribedSentenceCandidate.TextLengthNoSeparators,
                    firstOriginalWordRun.SentenceWordIndex,             // Will be 0
                    firstTranscribedWordRun.SentenceWordIndex,          // Will be 0
                    originalSentence.WordCount,
                    transcribedSentenceCandidate.WordCount,
                    null);                                              // Don't record scores.

                double firstWordAdjustedScore = ((mainScore * originalWordCount) + (firstWordScore * 3)) / (originalWordCount + 3);

                double wordPositionFactor = 1 - Math.Abs(
                    ((double)originalSentenceWordStartIndex / originalWordCount) -
                        ((double)transcribedCandidateStartIndex / transcribedWordCount));

                //wordPositionFactor = wordPositionFactor * wordPositionFactor;

                double mainFactoredScore = firstWordAdjustedScore * wordPositionFactor;

                bool newBest = false;

                if (mainFactoredScore > bestFactoredScore)
                {
                    bestMainScore = mainScore;
                    bestFactoredScore = mainFactoredScore;
                    bestTranscribedStartIndex = transcribedCandidateStartIndex;
                    newBest = true;
                }

                if (Statistics != null)
                    Statistics.DumpResyncTraceRow(
                        sentenceMapping,
                        transcribedCandidateStartIndex,
                        transcribedSentenceWordCount,
                        transcribedSentenceCandidate,
                        mainScore,
                        firstTranscribedWordRun,
                        firstWordScore,
                        firstWordAdjustedScore,
                        wordPositionFactor,
                        mainFactoredScore,
                        newBest,
                        bestMainScore,
                        bestFactoredScore,
                        bestTranscribedStartIndex);
            }

            if (reason == ResyncReason.MainScoreTooLow)
            {
                if (bestMainScore >= (2 * priorMainScore))
                    transcribedParentStartWordIndex = bestTranscribedStartIndex;
                else
                    return false;       // Accept only if score twice as big.
            }
            else
                transcribedParentStartWordIndex = bestTranscribedStartIndex;

            return true;
        }

        // Create and score transcribed sentence candidate.
        public SentenceMapping CreateAndScoreCandidateSentence(
            SentenceMapping sentenceMapping,
            List<MappingWordRun> transcribedWordRuns,
            TextBlock transcribedTextBlock,
            int transcribedParentStartWordIndex,
            int transcribedSentenceWordCount)
        {
            SentenceMapping candidateSentenceMapping = CreateCandidateSentence(
                sentenceMapping,
                transcribedWordRuns,
                transcribedTextBlock,
                transcribedParentStartWordIndex,
                transcribedSentenceWordCount);

            ScoreCandidateSentence(candidateSentenceMapping);

            return candidateSentenceMapping;
        }

        // Create transcribed sentence candidate.
        public SentenceMapping CreateCandidateSentence(
            SentenceMapping sentenceMapping,
            List<MappingWordRun> transcribedWordRuns,
            TextBlock transcribedTextBlock,
            int transcribedParentStartWordIndex,
            int transcribedSentenceWordCount)
        {
            SentenceMapping candidateSentenceMapping = new SentenceMapping(sentenceMapping);
            MappingSentenceRun transcribedSentenceCandidate = CreateTranscribedSentence(
                transcribedWordRuns,
                transcribedTextBlock,
                transcribedParentStartWordIndex,
                transcribedSentenceWordCount);

            candidateSentenceMapping.TranscribedSentence = transcribedSentenceCandidate;

            int startIndex = transcribedSentenceCandidate.ParentStartWordIndex;

            if (startIndex < TranscribedWordRuns.Count())
            {
                int stopIndex = transcribedSentenceCandidate.ParentStopWordIndex - 1;

                if (stopIndex < TranscribedWordRuns.Count())
                {
                    MappingWordRun startWordRun = TranscribedWordRuns[startIndex];
                    MappingWordRun stopWordRun = TranscribedWordRuns[stopIndex];
                    int textStartIndex = startWordRun.RawTextStartIndex;
                    int textStopIndex = stopWordRun.RawTextStopIndex;
                    transcribedSentenceCandidate.RawSentenceText = RawTranscribedTextBlock.Substring(textStartIndex, textStopIndex - textStartIndex);
                }
            }

            if (candidateSentenceMapping.OriginalExtraSentence != null)
            {
                int transcribedExtraParentStartWordIndex = transcribedSentenceCandidate.ParentStopWordIndex;

                MappingSentenceRun transcribedExtraSentenceCandidate = CreateTranscribedExtraSentence(
                    transcribedWordRuns,
                    transcribedTextBlock,
                    transcribedExtraParentStartWordIndex,
                    candidateSentenceMapping.OriginalExtraSentence);

                candidateSentenceMapping.TranscribedExtraSentence = transcribedExtraSentenceCandidate;
            }

            return candidateSentenceMapping;
        }

        // Create original extra sentence.
        public MappingSentenceRun CreateOriginalExtraSentence(
            List<SentenceMapping> sentenceMappings,
            List<MappingWordRun> originalWordRuns,
            List<TextBlock> originalTextBlocks,
            List<TextBlock> rawOriginalTextBlocks,
            int nextSentenceIndex)
        {
            SentenceMapping currentSentence = sentenceMappings[nextSentenceIndex - 1];
            SentenceMapping nextSentence = sentenceMappings[nextSentenceIndex];
            MappingSentenceRun currentOriginalSentence = currentSentence.OriginalSentence;
            MappingSentenceRun nextOriginalSentence = nextSentence.OriginalSentence;
            int startWordIndex = nextOriginalSentence.ParentStartWordIndex;
            int wordCount;

            switch (ExtraSentenceWordCountMode)
            {
                default:
                case ExtraSentenceWordCountModeCode.ConstantWordCount:
                    wordCount = ExtraSentenceLength;
                    break;
                case ExtraSentenceWordCountModeCode.OriginalMainCountHalf:
                    wordCount = currentOriginalSentence.WordCount / 2;
                    break;
                case ExtraSentenceWordCountModeCode.OriginalMainCountFull:
                    wordCount = currentOriginalSentence.WordCount;
                    break;
                case ExtraSentenceWordCountModeCode.OriginalMainCountDivisor:
                    if (ExtraSentenceDivisor != 0)
                        wordCount = currentOriginalSentence.WordCount / ExtraSentenceDivisor;
                    else
                        wordCount = currentOriginalSentence.WordCount;
                    break;
                case ExtraSentenceWordCountModeCode.OriginalNextCountFull:
                    wordCount = (nextOriginalSentence != null ? nextOriginalSentence.WordCount : 0);
                    break;
            }

            if (wordCount < ExtraSentenceLengthMinimum)
                wordCount = ExtraSentenceLengthMinimum;

            if ((ExtraSentenceLengthMaximum != 0) && (wordCount > ExtraSentenceLengthMaximum))
                wordCount = ExtraSentenceLengthMaximum;

            if (startWordIndex + wordCount > originalWordRuns.Count())
                wordCount = originalWordRuns.Count() - startWordIndex;

            int endWordIndex = startWordIndex + wordCount - 1;
            MappingWordRun endWordRun = originalWordRuns[endWordIndex];
            List<MappingWordRun> wordRuns = CopyWordRuns(originalWordRuns, startWordIndex, wordCount);
            string sentenceText = GetTextFromWordRunsNoAnnotations(wordRuns);
            string rawSentenceText = GetRawTextFromWordRunsNoAnnotations(wordRuns);
            int textLength = sentenceText.Length;
            MappingSentenceRun originalExtraSentenceRun = new MappingSentenceRun(
                0,
                textLength,
                sentenceText,
                rawSentenceText,
                wordRuns,
                startWordIndex);

            return originalExtraSentenceRun;
        }

        public string GetTextFromWordRuns(List<MappingWordRun> wordRuns)
        {
            int c = wordRuns.Count();
            int i;
            StringBuilder sb = new StringBuilder();
            for (i = 0; i < c; i++)
            {
                MappingWordRun wordRun = wordRuns[i];
                string word;
                if (wordRun != null)
                {
                    if (wordRun.Matched)
                        word = wordRun.WordText;
                    else
                        word = "-" + wordRun.WordText + "-";
                }
                else
                    word = "(null)";

                if (sb.Length != 0)
                    sb.Append(" ");

                sb.Append(word);
            }

            return sb.ToString();
        }

        public string GetTextFromWordRunsNoAnnotations(List<MappingWordRun> wordRuns)
        {
            int c = wordRuns.Count();
            int i;
            StringBuilder sb = new StringBuilder();
            for (i = 0; i < c; i++)
            {
                MappingWordRun wordRun = wordRuns[i];
                string word;
                if (wordRun != null)
                    word = wordRun.WordText;
                else
                    word = "(null)";

                if (sb.Length != 0)
                    sb.Append(" ");

                sb.Append(word);
            }

            return sb.ToString();
        }

        public string GetRawTextFromWordRunsNoAnnotations(List<MappingWordRun> wordRuns)
        {
            int c = wordRuns.Count();
            int i;
            StringBuilder sb = new StringBuilder();
            for (i = 0; i < c; i++)
            {
                MappingWordRun wordRun = wordRuns[i];
                string word;
                if (wordRun != null)
                    word = wordRun.RawWordText;
                else
                    word = "(null)";

                if (sb.Length != 0)
                    sb.Append(" ");

                sb.Append(word);
            }

            return sb.ToString();
        }

        public string GetTextReferencedFromWordRuns(
            TextBlock textBlock,
            List<MappingWordRun> wordRuns)
        {
            MappingWordRun startWordRun = wordRuns.First();
            MappingWordRun stopWordRun = wordRuns.Last();
            int textStartIndex = startWordRun.TextStartIndex;
            int textStopIndex = stopWordRun.TextStopIndex;
            string text = textBlock.Substring(textStartIndex, textStopIndex - textStartIndex);
            return text;
        }

        public string GetRawTextReferencedFromWordRuns(
            TextBlock textBlock,
            List<MappingWordRun> wordRuns)
        {
            MappingWordRun startWordRun = wordRuns.First();
            MappingWordRun stopWordRun = wordRuns.Last();
            int textStartIndex = startWordRun.RawTextStartIndex;
            int textStopIndex = stopWordRun.RawTextStopIndex;
            string text = textBlock.Substring(textStartIndex, textStopIndex - textStartIndex);
            return text;
        }

        // Create transcribed sentence candidate.
        public MappingSentenceRun CreateTranscribedSentence(
            List<MappingWordRun> transcribedWordRuns,
            TextBlock transcribedTextBlock,
            int transcribedParentStartWordIndex,
            int transcribedSentenceWordCount)
        {
            string transcribedSentenceText;
            List<MappingWordRun> transcribedSentenceWordRuns = CopyWordRuns(
                transcribedWordRuns,
                transcribedParentStartWordIndex,
                transcribedSentenceWordCount);

            if (transcribedSentenceWordCount > 0)
            {
                MappingWordRun transcribedStartWordRun = transcribedSentenceWordRuns.First();
                MappingWordRun transcribedLastWordRun = transcribedSentenceWordRuns.Last();
                int transcribedTextStartIndex = transcribedStartWordRun.TextStartIndex;
                int transcribedTextStopIndex = transcribedLastWordRun.TextStopIndex;
                int transcribedTextLength = transcribedTextStopIndex - transcribedTextStartIndex;

                transcribedSentenceText = transcribedTextBlock.Substring(
                    transcribedTextStartIndex,
                    transcribedTextLength);
            }
            else
                transcribedSentenceText = String.Empty;

            MappingSentenceRun transcribedSentence = new MappingSentenceRun(
                transcribedParentStartWordIndex,
                transcribedSentenceWordCount,
                transcribedSentenceText,
                String.Empty,
                transcribedSentenceWordRuns,
                transcribedParentStartWordIndex);

            return transcribedSentence;
        }

        // Create transcribed sentence candidate.
        public MappingSentenceRun CreateTranscribedExtraSentence(
            List<MappingWordRun> transcribedWordRuns,
            TextBlock transcribedTextBlock,
            int transcribedExtraParentStartWordIndex,
            MappingSentenceRun originalExtraSentence)
        {
            MappingSentenceRun transcribedExtraSentence = null;

            switch (ExtraSentenceCharacterCountMode)
            {
                case ExtraSentenceCharacterCountModeCode.MatchOriginalCharacterCount:
                    transcribedExtraSentence = CreateTranscribedExtraSentenceMatchOriginalCharacterCount(
                        transcribedWordRuns,
                        transcribedTextBlock,
                        transcribedExtraParentStartWordIndex,
                        originalExtraSentence);
                    break;
                case ExtraSentenceCharacterCountModeCode.MatchOriginalCharacterCountNoSplit:
                    transcribedExtraSentence = CreateTranscribedExtraSentenceMatchOriginalCharacterCountNoSplit(
                        transcribedWordRuns,
                        transcribedTextBlock,
                        transcribedExtraParentStartWordIndex,
                        originalExtraSentence);
                    break;
                case ExtraSentenceCharacterCountModeCode.MatchOriginalWordCount:
                    transcribedExtraSentence = CreateTranscribedExtraSentenceMatchOriginalWordCount(
                        transcribedWordRuns,
                        transcribedTextBlock,
                        transcribedExtraParentStartWordIndex,
                        originalExtraSentence);
                    break;
                default:
                    throw new Exception("CreateTranscribedExtraSentence: Missing code implementation.");
            }

            return transcribedExtraSentence;
        }

        // Create transcribed sentence candidate by matching number of character in original extra sentence.
        public MappingSentenceRun CreateTranscribedExtraSentenceMatchOriginalCharacterCount(
            List<MappingWordRun> transcribedWordRuns,
            TextBlock transcribedTextBlock,
            int transcribedExtraParentStartWordIndex,
            MappingSentenceRun originalExtraSentence)
        {
            int originalExtraSentenceCharacterCount = originalExtraSentence.TextLengthNoSeparators;
            int originalExtraSentenceWordCount = originalExtraSentence.WordCount;
            int transcribedExtraSentenceWordCount = 0;
            int transcribedExtraSentenceWordIndex = transcribedExtraParentStartWordIndex;
            int transcribedExtraSentenceWordIndexEnd = transcribedWordRuns.Count();
            int transcribedExtraSentenceCharacterCount = 0;
            StringBuilder transcribedExtraSentenceTextSB = new StringBuilder();
            List<MappingWordRun> transcribedSentenceWordRuns = new List<MappingWordRun>();
            int sentenceCharacterPosition = 0;
            int sentenceWordPosition = 0;

            for (; transcribedExtraSentenceWordIndex < transcribedExtraSentenceWordIndexEnd; transcribedExtraSentenceWordIndex++)
            {
                MappingWordRun transcribedRun = transcribedWordRuns[transcribedExtraSentenceWordIndex];
                int transcribedWordLength = transcribedRun.TextLength;
                int candidateCharacterCount = transcribedExtraSentenceCharacterCount + transcribedWordLength;

                if (candidateCharacterCount > originalExtraSentenceCharacterCount)
                {
                    int neededLength = originalExtraSentenceCharacterCount - transcribedExtraSentenceCharacterCount;

                    if (neededLength <= 0)
                        break;

                    transcribedRun = transcribedRun.CloneTruncatedWordRun(neededLength);
                }
                else
                    transcribedRun = new MappingWordRun(transcribedRun);

                transcribedRun.SentenceCharacterPosition = sentenceCharacterPosition;
                transcribedRun.SentenceWordIndex = sentenceWordPosition;

                transcribedSentenceWordRuns.Add(transcribedRun);

                // Add separator if not first. We don't care if it's a non-spaced language.
                if (transcribedExtraSentenceTextSB.Length != 0)
                    transcribedExtraSentenceTextSB.Append(" ");

                transcribedExtraSentenceTextSB.Append(transcribedRun.WordText);
                transcribedExtraSentenceCharacterCount += transcribedRun.TextLength;
                transcribedExtraSentenceWordCount++;
                sentenceCharacterPosition += transcribedRun.TextLength;
                sentenceWordPosition++;
            }

            MappingSentenceRun transcribedExtraSentence = new MappingSentenceRun(
                transcribedExtraParentStartWordIndex,
                transcribedExtraSentenceWordCount,
                transcribedExtraSentenceTextSB.ToString(),
                String.Empty,
                transcribedSentenceWordRuns,
                transcribedExtraParentStartWordIndex);

            return transcribedExtraSentence;
        }

        // Create transcribed sentence candidate by matching number of character in original extra sentence,
        // but without splitting the last word.
        public MappingSentenceRun CreateTranscribedExtraSentenceMatchOriginalCharacterCountNoSplit(
            List<MappingWordRun> transcribedWordRuns,
            TextBlock transcribedTextBlock,
            int transcribedExtraParentStartWordIndex,
            MappingSentenceRun originalExtraSentence)
        {
            int originalExtraSentenceCharacterCount = originalExtraSentence.TextLengthNoSeparators;
            int originalExtraSentenceWordCount = originalExtraSentence.WordCount;
            int transcribedExtraSentenceWordCount = 0;
            int transcribedExtraSentenceWordIndex = transcribedExtraParentStartWordIndex;
            int transcribedExtraSentenceWordIndexEnd = transcribedWordRuns.Count();
            int transcribedExtraSentenceCharacterCount = 0;
            StringBuilder transcribedExtraSentenceTextSB = new StringBuilder();
            List<MappingWordRun> transcribedSentenceWordRuns = new List<MappingWordRun>();
            int sentenceCharacterPosition = 0;
            int sentenceWordPosition = 0;

            for (; transcribedExtraSentenceWordIndex < transcribedExtraSentenceWordIndexEnd; transcribedExtraSentenceWordIndex++)
            {
                MappingWordRun transcribedRun = transcribedWordRuns[transcribedExtraSentenceWordIndex];
                int transcribedWordLength = transcribedRun.TextLength;
                int candidateCharacterCount = transcribedExtraSentenceCharacterCount + transcribedWordLength;

                transcribedRun = new MappingWordRun(transcribedRun);

                transcribedRun.SentenceCharacterPosition = sentenceCharacterPosition;
                transcribedRun.SentenceWordIndex = sentenceWordPosition;

                transcribedSentenceWordRuns.Add(transcribedRun);

                // Add separator if not first. We don't care if it's a non-spaced language.
                if (transcribedExtraSentenceTextSB.Length != 0)
                    transcribedExtraSentenceTextSB.Append(" ");

                transcribedExtraSentenceTextSB.Append(transcribedRun.WordText);
                transcribedExtraSentenceCharacterCount += transcribedRun.TextLength;
                transcribedExtraSentenceWordCount++;
                sentenceCharacterPosition += transcribedRun.TextLength;
                sentenceWordPosition++;

                if (candidateCharacterCount >= originalExtraSentenceCharacterCount)
                    break;
            }

            MappingSentenceRun transcribedExtraSentence = new MappingSentenceRun(
                transcribedExtraParentStartWordIndex,
                transcribedExtraSentenceWordCount,
                transcribedExtraSentenceTextSB.ToString(),
                String.Empty,
                transcribedSentenceWordRuns,
                transcribedExtraParentStartWordIndex);

            return transcribedExtraSentence;
        }

        // Create transcribed sentence candidate by matching number of character in original extra sentence,
        // but without splitting the last word.
        public MappingSentenceRun CreateTranscribedExtraSentenceMatchOriginalWordCount(
            List<MappingWordRun> transcribedWordRuns,
            TextBlock transcribedTextBlock,
            int transcribedExtraParentStartWordIndex,
            MappingSentenceRun originalExtraSentence)
        {
            int originalExtraSentenceWordCount = originalExtraSentence.WordCount;
            int transcribedExtraSentenceWordCount = 0;
            int transcribedExtraSentenceWordIndex = transcribedExtraParentStartWordIndex;
            int transcribedExtraSentenceWordIndexEnd = transcribedWordRuns.Count();
            StringBuilder transcribedExtraSentenceTextSB = new StringBuilder();
            List<MappingWordRun> transcribedSentenceWordRuns = new List<MappingWordRun>();
            int sentenceCharacterPosition = 0;
            int sentenceWordPosition = 0;

            for (; transcribedExtraSentenceWordIndex < transcribedExtraSentenceWordIndexEnd; transcribedExtraSentenceWordIndex++)
            {
                MappingWordRun transcribedRun = transcribedWordRuns[transcribedExtraSentenceWordIndex];

                transcribedRun = new MappingWordRun(transcribedRun);

                transcribedRun.SentenceCharacterPosition = sentenceCharacterPosition;
                transcribedRun.SentenceWordIndex = sentenceWordPosition;

                transcribedSentenceWordRuns.Add(transcribedRun);

                // Add separator if not first. We don't care if it's a non-spaced language.
                if (transcribedExtraSentenceTextSB.Length != 0)
                    transcribedExtraSentenceTextSB.Append(" ");

                transcribedExtraSentenceTextSB.Append(transcribedRun.WordText);
                transcribedExtraSentenceWordCount++;
                sentenceCharacterPosition += transcribedRun.TextLength;
                sentenceWordPosition++;

                if (transcribedExtraSentenceWordCount >= originalExtraSentenceWordCount)
                    break;
            }

            MappingSentenceRun transcribedExtraSentence = new MappingSentenceRun(
                transcribedExtraParentStartWordIndex,
                transcribedExtraSentenceWordCount,
                transcribedExtraSentenceTextSB.ToString(),
                String.Empty,
                transcribedSentenceWordRuns,
                transcribedExtraParentStartWordIndex);

            return transcribedExtraSentence;
        }

        public int CalculateTranscribedCandidateEndWordIndex(
            int originalParentStartWordIndex,
            int originalSentenceWordRunCount,
            int originalWordRunCount,
            int transcribedParentStartWordIndex,
            int transcribedWordRunCount)
        {
            int originalParentStopWordIndex = originalParentStartWordIndex + originalSentenceWordRunCount;
            int transcribedEndWordIndex;

            // If last sentence...
            if (originalParentStopWordIndex >= originalWordRunCount)
                transcribedEndWordIndex = transcribedWordRunCount;
            else
            {
                int originalWordRunRemainderCount = originalWordRunCount - originalParentStartWordIndex;
                int transcribedWordRunRemainderCount = transcribedWordRunCount - transcribedParentStartWordIndex;
                double wordCountRatio = (double)transcribedWordRunRemainderCount / originalWordRunRemainderCount;
                double roundingOffset = 0.5;

                if (wordCountRatio > 1.25)
                    wordCountRatio = 1.25;
                else if (wordCountRatio < .80)
                    wordCountRatio = .80;

                transcribedEndWordIndex =
                    (int)(transcribedParentStartWordIndex + ((originalSentenceWordRunCount * wordCountRatio) + roundingOffset));

                if (transcribedEndWordIndex > transcribedWordRunCount)
                    transcribedEndWordIndex = transcribedWordRunCount;
            }

            return transcribedEndWordIndex;
        }

        // Score candidate sentence, setting score in given sentence mapping.
        public void ScoreCandidateSentence(SentenceMapping sentenceMapping)
        {
            MappingSentenceRun originalSentence = sentenceMapping.OriginalSentence;
            MappingSentenceRun originalExtraSentence = sentenceMapping.OriginalExtraSentence;
            MappingSentenceRun transcribedSentence = sentenceMapping.TranscribedSentence;
            MappingSentenceRun transcribedExtraSentence = sentenceMapping.TranscribedExtraSentence;
            double mainScore;
            double extraScore = 0.0;
            double combinedMatchScore;
            int editDistanceRaw = 0;
            int editDistanceProcessed = 0;
            int editDistanceMatched = 0;
            // Get count before scoring, which may change the count.
            int wordsInOriginalSentence = originalSentence.WordCount;

            mainScore = ScoreSentencePair(
                originalSentence,
                transcribedSentence);

            if ((SentenceMatchMode == SentenceMatchModeCode.UseCombinedSentenceScore) &&
                (originalExtraSentence != null) &&
                (transcribedExtraSentence != null))
            {
                // Get count before scoring, which may change the count.
                int wordsInExtraOriginalSentence = originalExtraSentence.WordCount;

                extraScore = ScoreSentencePair(
                    originalExtraSentence,
                    transcribedExtraSentence);

                switch (WeightingMode)
                {
                    default:
                    case WeightingModeCode.WordCountWeighting:
                        combinedMatchScore =
                            ((mainScore * wordsInOriginalSentence * MainSentenceWeightingFactor)
                                + (extraScore * wordsInExtraOriginalSentence * ExtraSentenceWeightingFactor)) /
                            ((wordsInOriginalSentence * MainSentenceWeightingFactor)
                                + (wordsInExtraOriginalSentence * ExtraSentenceWeightingFactor));
                        break;
                    case WeightingModeCode.ConstantWeighting:
                        combinedMatchScore =
                            ((mainScore * MainSentenceWeightingFactor)
                                + (extraScore * ExtraSentenceWeightingFactor)) /
                            (MainSentenceWeightingFactor + ExtraSentenceWeightingFactor);
                        break;
                }
            }
            else
                combinedMatchScore = mainScore;

            if (originalSentence != null)
            {
                if (transcribedSentence != null)
                {
                    editDistanceRaw = TextUtilities.ComputeDistance(
                        TargetLanguageTool.StandardizeWord(originalSentence.RawSentenceText),
                        TargetLanguageTool.StandardizeWord(transcribedSentence.RawSentenceText));
                    editDistanceProcessed = TextUtilities.ComputeDistance(
                        TargetLanguageTool.StandardizeWord(originalSentence.SentenceText),
                        TargetLanguageTool.StandardizeWord(transcribedSentence.SentenceText));
                    editDistanceMatched = TextUtilities.ComputeDistance(
                        TargetLanguageTool.StandardizeWord(originalSentence.SentenceFromWords),
                        TargetLanguageTool.StandardizeWord(originalSentence.MatchedSentenceFromWordsRaw));
                }
                else
                {
                    editDistanceRaw = TextUtilities.ComputeDistance(
                        TargetLanguageTool.StandardizeWord(originalSentence.RawSentenceText),
                        String.Empty);
                    editDistanceProcessed = TextUtilities.ComputeDistance(
                        TargetLanguageTool.StandardizeWord(originalSentence.SentenceText),
                        String.Empty);
                    editDistanceMatched = TextUtilities.ComputeDistance(
                        TargetLanguageTool.StandardizeWord(originalSentence.SentenceFromWords),
                        String.Empty);
                }
            }

            sentenceMapping.MainScore = mainScore;
            sentenceMapping.ExtraScore = extraScore;
            sentenceMapping.CombinedScore = combinedMatchScore;
            sentenceMapping.EditDistanceRaw = editDistanceRaw;
            sentenceMapping.EditDistanceProcessed = editDistanceProcessed;
            sentenceMapping.EditDistanceMatched = editDistanceMatched;
        }

        public double ScoreSentencePair(
            MappingSentenceRun originalSentence,
            MappingSentenceRun transcribedSentence)
        {
            double sentenceScore;

            switch (SentenceMatchMode)
            {
                default:
                case SentenceMatchModeCode.UseCombinedSentenceScore:
                case SentenceMatchModeCode.UseMainScore:
                    {
                        // Calculate and record single word edit distance scores.
                        CalculateSingleWordEditDistanceScores(
                            originalSentence,
                            transcribedSentence);

                        // Do word combinations and calculate word match scores.
                        CalculateCombinedWordMatchScores(
                            originalSentence,
                            transcribedSentence);

                        // Record matched and unmatched counts and words for original sentence.
                        RecordMatchInfo(originalSentence);

                        // Record matched and unmatched counts and words for transcribed sentence.
                        RecordMatchInfo(transcribedSentence);

                        double sumOfOrignalWordMatchScores;
                        double sumOfTranscribedWordUnmatchScores;
                        int originalWordCount = originalSentence.WordCount; // Should be final count.

                        switch (MatchScoreMode)
                        {
                            default:
                            case MatchScoreModeCode.UseCharacterIndexOnly:
                                {
                                    // Get sums of word match scores in the original sentence.
                                    sumOfOrignalWordMatchScores = SumWordMatchScoresUsingCharacterIndex(originalSentence);

                                    // Get sums of word match scores in the transcribed sentence.
                                    sumOfTranscribedWordUnmatchScores = SumTranscribedWordUnmatchScoresUsingCharacterIndex(transcribedSentence);

                                    // Score calculation.
                                    sentenceScore =
                                        (sumOfOrignalWordMatchScores + 1) /
                                            (originalWordCount + 1 + (sumOfTranscribedWordUnmatchScores / TranscribedUnusedSumPenaltyDivisor));

                                    // Save used scores.
                                    SetWordScoresUsingCharacterIndex(originalSentence, transcribedSentence);
                                }
                                break;
                            case MatchScoreModeCode.UseWordIndexOnly:
                                {
                                    // Get sums of word match scores in the original sentence.
                                    sumOfOrignalWordMatchScores = SumWordMatchScoresUsingWordIndex(originalSentence);

                                    // Get sums of word match scores in the transcribed sentence.
                                    sumOfTranscribedWordUnmatchScores = SumTranscribedWordUnmatchScoresUsingWordIndex(transcribedSentence);

                                    // Score calculation.
                                    sentenceScore =
                                        (sumOfOrignalWordMatchScores + 1) /
                                            (originalWordCount + 1 + (sumOfTranscribedWordUnmatchScores / TranscribedUnusedSumPenaltyDivisor));

                                    // Save used scores.
                                    SetWordScoresUsingWordIndex(originalSentence, transcribedSentence);
                                }
                                break;
                            case MatchScoreModeCode.UseHighestMatchScore:
                                {
                                    // Get sums of word match scores in the original sentence.
                                    double sumOfOrignalWordMatchScoresUsingWordIndex = SumWordMatchScoresUsingWordIndex(originalSentence);

                                    // Get sums of word match scores in the transcribed sentence.
                                    double sumOfTranscribedWordUnmatchScoresUsingWordIndex = SumTranscribedWordUnmatchScoresUsingWordIndex(transcribedSentence);

                                    // Score calculation.
                                    double sentenceScoreUsingWordIndex =
                                        (sumOfOrignalWordMatchScoresUsingWordIndex + 1) /
                                            (originalWordCount + 1 + (sumOfTranscribedWordUnmatchScoresUsingWordIndex / TranscribedUnusedSumPenaltyDivisor));

                                    // Get sums of word match scores in the original sentence.
                                    double sumOfOrignalWordMatchScoresUsingCharacterIndex = SumWordMatchScoresUsingCharacterIndex(originalSentence);

                                    // Get sums of word match scores in the transcribed sentence.
                                    double sumOfTranscribedWordUnmatchScoresUsingCharacterIndex = SumTranscribedWordUnmatchScoresUsingCharacterIndex(transcribedSentence);

                                    // Score calculation.
                                    double sentenceScoreUsingCharacterIndex =
                                        (sumOfOrignalWordMatchScoresUsingCharacterIndex + 1) /
                                            (originalWordCount + 1 + (sumOfTranscribedWordUnmatchScoresUsingCharacterIndex / TranscribedUnusedSumPenaltyDivisor));

                                    if (sentenceScoreUsingCharacterIndex >= sentenceScoreUsingWordIndex)
                                    {
                                        sentenceScore = sentenceScoreUsingCharacterIndex;
                                        sumOfOrignalWordMatchScores = sumOfOrignalWordMatchScoresUsingCharacterIndex;
                                        sumOfTranscribedWordUnmatchScores = sumOfTranscribedWordUnmatchScoresUsingCharacterIndex;

                                        // Save used scores.
                                        SetWordScoresUsingCharacterIndex(originalSentence, transcribedSentence);
                                    }
                                    else
                                    {
                                        sentenceScore = sentenceScoreUsingWordIndex;
                                        sumOfOrignalWordMatchScores = sumOfOrignalWordMatchScoresUsingWordIndex;
                                        sumOfTranscribedWordUnmatchScores = sumOfTranscribedWordUnmatchScoresUsingWordIndex;

                                        // Save used scores.
                                        SetWordScoresUsingWordIndex(originalSentence, transcribedSentence);
                                    }
                                }
                                break;
                            case MatchScoreModeCode.UseWordAndCharacterIndexProduct:
                                {
                                    // Get sums of word match scores in the original sentence.
                                    sumOfOrignalWordMatchScores = SumWordMatchScoresUsingWordAndCharacterIndex(originalSentence);

                                    // Get sums of word match scores in the transcribed sentence.
                                    sumOfTranscribedWordUnmatchScores = SumTranscribedWordUnmatchScoresUsingWordAndCharacterIndex(transcribedSentence);

                                    // Score calculation.
                                    sentenceScore =
                                        (sumOfOrignalWordMatchScores + 1) /
                                            (originalWordCount + 1 + (sumOfTranscribedWordUnmatchScores / TranscribedUnusedSumPenaltyDivisor));

                                    // Save used scores.
                                    SetWordScoresUsingWordAndCharacterIndex(originalSentence, transcribedSentence);
                                }
                                break;
                        }

                        // Save sum in original sentence for reporting.
                        originalSentence.WordMatchScoreSum = sumOfOrignalWordMatchScores;

                        // Save sum in transcribed sentence for reporting.
                        transcribedSentence.WordMatchScoreSum = sumOfTranscribedWordUnmatchScores;
                    }
                    break;
                case SentenceMatchModeCode.UseSentenceDistance:
                    {
                        string originalProcessedSentence = originalSentence.SentenceText;
                        string transcribedProcessedSentence = transcribedSentence.SentenceText;
                        string originalNormalizedSentence = StandardizeWord(originalProcessedSentence);
                        string transcribedNormalizedSentence = StandardizeWord(transcribedProcessedSentence);
                        sentenceScore = CalculateEditDistanceScore(originalNormalizedSentence, transcribedNormalizedSentence);
                    }
                    break;
            }

            return sentenceScore;
        }

        // Calculate and record single word edit distance scores.
        public void CalculateSingleWordEditDistanceScores(
            MappingSentenceRun originalSentence,
            MappingSentenceRun transcribedSentence)
        {
            int originalWordCount = originalSentence.WordCount;
            int originalWordIndex;
            MappingWordRun originalWordRun = null;
            int transcribedWordCount = transcribedSentence.WordCount;
            int transcribedWordIndex;
            MappingWordRun transcribedWordRun = null;

            for (originalWordIndex = 0;
                originalWordIndex < originalWordCount;
                originalWordIndex++)
            {
                originalWordRun = originalSentence.GetWordRun(originalWordIndex);

                if (originalWordRun == null)
                    throw new Exception("AudioMapper.CalculateAndRecordSingleWordMatchScores: originalWordRun is null. Index: " +
                        originalWordIndex.ToString());

                string originalWord = originalWordRun.WordText;
                string transcribedWord;
                double bestEditDistanceScore = 0.0;
                int bestTranscribedWordIndex = -1;
                bool wasMatched = false;

                for (transcribedWordIndex = 0;
                    transcribedWordIndex < transcribedWordCount;
                    transcribedWordIndex++)
                {
                    transcribedWordRun = transcribedSentence.GetWordRun(transcribedWordIndex);

                    if (transcribedWordRun == null)
                        throw new Exception("AudioMapper.CalculateAndRecordSingleWordMatchScores: transcribedWordRun is null. Index: " +
                            transcribedWordIndex.ToString());

                    transcribedWord = transcribedWordRun.WordText;

                    double editDistanceScore = CalculateEditDistanceScore(
                        originalWordRun.WordText,
                        transcribedWordRun.WordText);

                    if (editDistanceScore > bestEditDistanceScore)
                    {
                        bestEditDistanceScore = editDistanceScore;
                        bestTranscribedWordIndex = transcribedWordIndex;
                        wasMatched = true;

                        // Exit early if we have a perfect match.
                        if (editDistanceScore == 1.0)
                            break;
                    }
                }

                if (wasMatched)
                {
                    transcribedWordRun = transcribedSentence.GetWordRun(bestTranscribedWordIndex);
                    transcribedWordRun.EditDistanceScore = bestEditDistanceScore;
                    originalWordRun.EditDistanceScore = bestEditDistanceScore;
                }
            }
        }

        // Do preferred word combinations and calculate and record word match scores.
        public void CalculateCombinedWordMatchScores(
            MappingSentenceRun originalSentence,
            MappingSentenceRun transcribedSentence)
        {
            int originalWordCount = originalSentence.WordCount;
            int originalWordIndex;

            // Loop over the original word runs.
            // Note that the original sentence my be changed to have combined words, so we can just increment the index.
            for (originalWordIndex = 0; originalWordIndex < originalSentence.WordCount; originalWordIndex++)
            {
                int transcribedWordIndex;
                int transcribedWordLength;
                int originalWordLength = 1;

                // Find the best match for the current original word position, possibly combined.
                if (!FindBestWordMatch(
                        originalSentence,
                        transcribedSentence,
                        originalWordIndex,
                        out originalWordLength,
                        out transcribedWordIndex,
                        out transcribedWordLength))
                {
                    // We didn't find a match for this original word.
                    continue;
                }

                // If the best original word length is not one, we combine the original word run
                // and set it in the original sentence destructively.
                if (originalWordLength != 1)
                {
                    originalSentence.CombineWordRuns(originalWordIndex, originalWordLength);

                    // Update the original sentence word count for the loop.
                    originalWordCount = originalSentence.WordCount;
                }

                // Get the word run, now possibly combined.
                MappingWordRun originalWordRun = originalSentence.GetWordRun(originalWordIndex);

                if (originalWordRun == null)
                    throw new Exception("AudioMapper.CalculateCombinedWordMatchScores: originalWordRun is null. Index: " +
                        originalWordIndex.ToString());

                // Copy and possibly combine the transcribed word run.
                // But we don't modify the transcribed sentence, as we need the word runs intact for other matches.
                MappingWordRun transcribedWordRun = new MappingWordRun(
                    transcribedSentence.WordRuns,
                    transcribedWordIndex,
                    transcribedWordLength);

                // Calculate the word score and set it in the original word run.
                double wordScore = CalculateWordMatchScore(
                    originalWordRun.WordText,
                    transcribedWordRun.WordText,
                    originalWordRun.SentenceCharacterPosition,
                    transcribedWordRun.SentenceCharacterPosition,
                    originalSentence.TextLengthNoSeparators,
                    transcribedSentence.TextLengthNoSeparators,
                    originalWordRun.SentenceWordIndex,
                    transcribedWordRun.SentenceWordIndex,
                    originalSentence.WordCount,
                    transcribedSentence.WordCount,
                    originalWordRun);

                // For now, any non-zero score is counted as a match.
                if (wordScore > 0)
                {
                    // Save the matching transcribed word run and its index in the original word run.
                    originalWordRun.MatchedWordRun = transcribedWordRun;
                    originalWordRun.MatchedWordIndex = transcribedWordIndex;

                    // Set the matched flag in the original word.
                    originalWordRun.Matched = true;

                    // Set the matched flag in the transcribed run.
                    transcribedWordRun.Matched = true;

                    // Set the matched flag and word score in the matched transcribed word runs.
                    {
                        MappingWordRun singleTranscribedWordRun = transcribedSentence.GetWordRun(transcribedWordIndex);

                        if (singleTranscribedWordRun != null)
                        {
                            // If the transcribed word was combined, only set the score in the first word.
                            // If the transcribed word was already scored, save only the highest score.
                            if (wordScore > singleTranscribedWordRun.WordMatchScoreUsingCharacterIndex)
                            {
                                singleTranscribedWordRun.WordMatchScoreUsingCharacterIndex = originalWordRun.WordMatchScoreUsingCharacterIndex;
                                singleTranscribedWordRun.WordMatchScoreUsingWordIndex = originalWordRun.WordMatchScoreUsingWordIndex;
                                singleTranscribedWordRun.WordPositionFactorUsingCharacterIndex = originalWordRun.WordPositionFactorUsingCharacterIndex;
                                singleTranscribedWordRun.WordPositionFactorUsingWordIndex = originalWordRun.WordPositionFactorUsingWordIndex;
                            }

                            singleTranscribedWordRun.Matched = true;
                        }

                        // Set matched flag in the transcribed non-first combined words.
                        // This is so we can report matched word counts.
                        for (int i = 1; i < transcribedWordLength; i++)
                        {
                            singleTranscribedWordRun = transcribedSentence.GetWordRun(transcribedWordIndex + i);

                            if (singleTranscribedWordRun != null)
                                singleTranscribedWordRun.Matched = true;
                        }
                    }
                }
            }
        }

        // Sum the word match scores in a sentence run using the character index version.
        public double SumWordMatchScoresUsingCharacterIndex(MappingSentenceRun sentenceRun)
        {
            int wordCount = sentenceRun.WordCount;
            int wordIndex;
            double wordScoreSum = 0.0;

            // Sum word scores.
            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                MappingWordRun wordRun = sentenceRun.GetWordRun(wordIndex);

                if ((wordRun != null) && wordRun.Matched)
                    wordScoreSum += wordRun.WordMatchScoreUsingCharacterIndex;
            }

            return wordScoreSum;
        }

        // Sum the word match scores in a sentence run using the word index version.
        public double SumWordMatchScoresUsingWordIndex(MappingSentenceRun sentenceRun)
        {
            int wordCount = sentenceRun.WordCount;
            int wordIndex;
            double wordScoreSum = 0.0;

            // Sum word scores.
            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                MappingWordRun wordRun = sentenceRun.GetWordRun(wordIndex);

                if ((wordRun != null) && wordRun.Matched)
                    wordScoreSum += wordRun.WordMatchScoreUsingWordIndex;
            }

            return wordScoreSum;
        }

        // Sum the word match scores in a sentence run using the character index version.
        public double SumWordMatchScoresUsingWordAndCharacterIndex(MappingSentenceRun sentenceRun)
        {
            int wordCount = sentenceRun.WordCount;
            int wordIndex;
            double wordScoreSum = 0.0;

            // Sum word scores.
            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                MappingWordRun wordRun = sentenceRun.GetWordRun(wordIndex);

                if ((wordRun != null) && wordRun.Matched)
                    wordScoreSum += wordRun.WordMatchScoreUsingWordAndCharacterIndex;
            }

            return wordScoreSum;
        }

        // Sum the word match scores in a sentence run using the character index version.
        public double SumTranscribedWordUnmatchScoresUsingCharacterIndex(MappingSentenceRun sentenceRun)
        {
            int wordCount = sentenceRun.WordCount;
            int wordIndex;
            double wordScoreSum = 0.0;

            // Sum word scores.
            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                MappingWordRun wordRun = sentenceRun.GetWordRun(wordIndex);

                if (wordRun != null)
                    wordScoreSum += 1.0 - wordRun.WordMatchScoreUsingCharacterIndex;
            }

            return wordScoreSum;
        }

        // Sum the word match scores in a sentence run using the word index version.
        public double SumTranscribedWordUnmatchScoresUsingWordIndex(MappingSentenceRun sentenceRun)
        {
            int wordCount = sentenceRun.WordCount;
            int wordIndex;
            double wordScoreSum = 0.0;

            // Sum word scores.
            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                MappingWordRun wordRun = sentenceRun.GetWordRun(wordIndex);

                if (wordRun != null)
                    wordScoreSum += 1.0 - wordRun.WordMatchScoreUsingWordIndex;
            }

            return wordScoreSum;
        }

        // Sum the word match scores in a sentence run using the word index version.
        public double SumTranscribedWordUnmatchScoresUsingWordAndCharacterIndex(MappingSentenceRun sentenceRun)
        {
            int wordCount = sentenceRun.WordCount;
            int wordIndex;
            double wordScoreSum = 0.0;

            // Sum word scores.
            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                MappingWordRun wordRun = sentenceRun.GetWordRun(wordIndex);

                if (wordRun != null)
                    wordScoreSum += 1.0 - wordRun.WordMatchScoreUsingWordAndCharacterIndex;
            }

            return wordScoreSum;
        }

        // Set the word scores and position factors of the words in the sentence runs from the character index version.
        public void SetWordScoresUsingCharacterIndex(
            MappingSentenceRun originalSentence,
            MappingSentenceRun transcribedSentence)
        {
            SetWordScoresUsingCharacterIndex(originalSentence);
            SetWordScoresUsingCharacterIndex(transcribedSentence);
        }

        // Set the word scores and position factors of the words in the sentence runs from the word index version.
        public void SetWordScoresUsingWordIndex(
            MappingSentenceRun originalSentence,
            MappingSentenceRun transcribedSentence)
        {
            SetWordScoresUsingWordIndex(originalSentence);
            SetWordScoresUsingWordIndex(transcribedSentence);
        }

        // Set the word scores and position factors of the words in the sentence runs from the word index version.
        public void SetWordScoresUsingWordAndCharacterIndex(
            MappingSentenceRun originalSentence,
            MappingSentenceRun transcribedSentence)
        {
            SetWordScoresUsingWordAndCharacterIndex(originalSentence);
            SetWordScoresUsingWordAndCharacterIndex(transcribedSentence);
        }

        // Set the word scores and position factors of the words in the sentence runs from the character index version.
        public void SetWordScoresUsingCharacterIndex(MappingSentenceRun sentenceRun)
        {
            int wordIndex;
            int wordCount = sentenceRun.WordCount;

            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                MappingWordRun wordRun = sentenceRun.GetWordRun(wordIndex);
                wordRun.WordPositionFactor = wordRun.WordPositionFactorUsingCharacterIndex;
                wordRun.WordMatchScore = wordRun.WordMatchScoreUsingCharacterIndex;
            }
        }

        // Set the word scores and position factors of the words in the sentence runs from the word index version.
        public void SetWordScoresUsingWordIndex(MappingSentenceRun sentenceRun)
        {
            int wordIndex;
            int wordCount = sentenceRun.WordCount;

            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                MappingWordRun wordRun = sentenceRun.GetWordRun(wordIndex);
                wordRun.WordPositionFactor = wordRun.WordPositionFactorUsingWordIndex;
                wordRun.WordMatchScore = wordRun.WordMatchScoreUsingWordIndex;
            }
        }

        // Set the word scores and position factors of the words in the sentence runs from the word index version.
        public void SetWordScoresUsingWordAndCharacterIndex(MappingSentenceRun sentenceRun)
        {
            int wordIndex;
            int wordCount = sentenceRun.WordCount;

            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                MappingWordRun wordRun = sentenceRun.GetWordRun(wordIndex);
                wordRun.WordPositionFactor = wordRun.WordPositionFactorUsingCharacterIndex * wordRun.WordPositionFactorUsingWordIndex;
                wordRun.WordMatchScore = wordRun.WordMatchScoreUsingWordAndCharacterIndex;
            }
        }

        // Find the best word match for one original word position, possibly combining
        // either or both the original and transcribed word runs.
        public bool FindBestWordMatch(
            MappingSentenceRun originalSentence,
            MappingSentenceRun transcribedSentence,
            int originalWordIndex,
            out int bestOriginalWordLength,
            out int bestTranscribedWordIndex,
            out int bestTranscribedWordLength)
        {
            int originalWordCount = originalSentence.WordCount;
            int originalWordLengthLimit = MaxWordGroupSize;
            int originalWordLength;
            double bestWordScore = 0.0;
            bool foundMatch = false;

            bestTranscribedWordIndex = -1;
            bestOriginalWordLength = 1;
            bestTranscribedWordLength = 1;

            // Make sure we don't exceed the end of the sentence.
            if (originalWordIndex + originalWordLengthLimit > originalWordCount)
                originalWordLengthLimit = originalWordCount - originalWordIndex;

            // Try multiple word lengths for the original.
            for (originalWordLength = 1; originalWordLength <= originalWordLengthLimit; originalWordLength++)
            {
                string originalWord = originalSentence.GetWordSpanNoSpace(originalWordIndex, originalWordLength);

                // Look for a best match with the current possibly combined original word span.
                if (FindBestTranscribedWordMatch(
                        originalSentence,
                        transcribedSentence,
                        originalWordIndex,
                        originalWordLength,
                        originalWord,
                        ref bestTranscribedWordIndex,
                        ref bestTranscribedWordLength,
                        ref bestWordScore))
                {
                    // We found a better match, so we save the word length for the original.
                    bestOriginalWordLength = originalWordLength;
                    foundMatch = true;
                }
            }

            return foundMatch;
        }

        // Find best possibly combined transcribed word match for an original possibly combined word run.
        // Returns true if found a better match.
        public bool FindBestTranscribedWordMatch(
            MappingSentenceRun originalSentence,
            MappingSentenceRun transcribedSentence,
            int originalWordIndex,
            int originalWordLength,
            string originalWord,
            ref int bestTranscribedWordIndex,
            ref int bestTranscribedWordLength,
            ref double bestWordScore)
        {
            MappingWordRun originalWordRun = originalSentence.GetWordRun(originalWordIndex);
            int originalSentenceCharacterIndex = originalWordRun.SentenceCharacterPosition;
            int originalSentenceCharacterLength = originalSentence.TextLengthNoSeparators;
            int transcribedWordIndex;
            int transcribedWordCount = transcribedSentence.WordCount;
            bool foundBest = false;

            for (transcribedWordIndex = 0;
                transcribedWordIndex < transcribedWordCount;
                transcribedWordIndex++)
            {
                MappingWordRun transcribedWordRun = transcribedSentence.GetWordRun(transcribedWordIndex);
                int transcribedSentenceCharacterIndex = transcribedWordRun.SentenceCharacterPosition;
                int transcribedSentenceCharacterLength = transcribedSentence.TextLengthNoSeparators;
                int transcribedWordLength;
                int transcribedWordLengthLimit = MaxWordGroupSize;
                string transcribedWord;

                if (transcribedWordIndex + transcribedWordLengthLimit > transcribedWordCount)
                    transcribedWordLengthLimit = transcribedWordCount - transcribedWordIndex;

                for (transcribedWordLength = 1; transcribedWordLength <= transcribedWordLengthLimit; transcribedWordLength++)
                {
                    // Don't try to match if both word lengths are the same and not 1.
                    if ((transcribedWordLength != 1) && (transcribedWordLength == originalWordLength))
                        continue;

                    transcribedWord = transcribedSentence.GetWordSpanNoSpace(transcribedWordIndex, transcribedWordLength);

                    double wordScore = CalculateWordMatchScoreUsingCharacterIndex(
                        originalWord,
                        transcribedWord,
                        originalSentenceCharacterIndex,
                        transcribedSentenceCharacterIndex,
                        originalSentenceCharacterLength,
                        transcribedSentenceCharacterLength);

                    if (wordScore > bestWordScore)
                    {
                        // Exit early if perfect match.
                        if (wordScore == 1.0)
                        {
                            bestTranscribedWordIndex = transcribedWordIndex;
                            bestTranscribedWordLength = transcribedWordLength;
                            bestWordScore = wordScore;
                            foundBest = true;
                            break;
                        }

                        if (IsCombinedBetterThanSingle(
                            originalSentence,
                            transcribedSentence,
                            originalWord,
                            transcribedWord,
                            originalWordIndex,
                            transcribedWordIndex,
                            originalWordLength,
                            transcribedWordLength))
                        {
                            bestTranscribedWordIndex = transcribedWordIndex;
                            bestTranscribedWordLength = transcribedWordLength;
                            bestWordScore = wordScore;
                            foundBest = true;
                        }
                    }
                }
            }

            return foundBest;
        }

        // Check to see if any of the additional components of a combined word have a better
        // single edit distance score than the combined word,
        public bool IsCombinedBetterThanSingle(
            MappingSentenceRun originalSentence,
            MappingSentenceRun transcribedSentence,
            string originalCombinedWord,
            string transcribedCombinedWord,
            int originalWordIndex,
            int transcribedWordIndex,
            int originalWordLength,
            int transcribedWordLength)
        {
            if ((originalWordLength == 1) && (transcribedWordLength == 1))
                return true;

            double editDistanceScore = CalculateEditDistanceScore(
                originalCombinedWord,
                transcribedCombinedWord);

            for (int i = 0; i < originalWordLength; i++)
            {
               MappingWordRun individualWordRun = originalSentence.GetWordRun(originalWordIndex + i);

                if (individualWordRun.EditDistanceScore > editDistanceScore)
                    return false;
            }

            for (int i = 0; i < transcribedWordLength; i++)
            {
                MappingWordRun individualWordRun = transcribedSentence.GetWordRun(transcribedWordIndex + i);

                if (individualWordRun.EditDistanceScore > editDistanceScore)
                    return false;
            }

            return true;
        }

        // Calculate edit distance score.
        public double CalculateEditDistanceScore(
            string originalWord,
            string transcribedWord)
        {
            if (String.IsNullOrEmpty(originalWord))
                throw new Exception("AudioMapper.CalculateEditDistanceScore: originalWord is empty.");

            if (String.IsNullOrEmpty(transcribedWord))
                throw new Exception("AudioMapper.CalculateEditDistanceScore: transcribedWord is empty.");

            int originalWordLength = originalWord.Length;
            double editDistance = TextUtilities.ComputeDistance(originalWord, transcribedWord);
            double editDistanceScore = ((double)originalWordLength + 1.0 - editDistance) / (originalWordLength + 1);

            if (editDistanceScore < 0.0)
                editDistanceScore = 0.0;

            return editDistanceScore;
        }

        // Calculate edit distance score using character index.
        public double CalculateWordMatchScore(
            string originalWord,
            string transcribedWord,
            int originalSentenceCharacterPosition,
            int transcribedSentenceCharacterPosition,
            int originalSentenceCharacterLength,
            int transcribedSentenceCharacterLength,
            int originalSentenceWordPosition,
            int transcribedSentenceWordPosition,
            int originalSentenceWordLength,
            int transcribedSentenceWordLength,
            MappingWordRun wordRun = null)  // For recording debugging information.
        {
            if (String.IsNullOrEmpty(originalWord))
                throw new Exception("AudioMapper.CalculateWordMatchScore: originalWord is empty.");

            if (String.IsNullOrEmpty(transcribedWord))
                throw new Exception("AudioMapper.CalculateWordMatchScore: transcribedWord is empty.");

            int originalWordLength = originalWord.Length;
            int editDistance = TextUtilities.ComputeDistance(originalWord, transcribedWord);

            double editDistanceFactor = ((double)originalWordLength + 1.0 - editDistance) / (originalWordLength + 1);

            if (editDistanceFactor < 0.0)
                editDistanceFactor = 0.0;

            double editDistanceFactorSquared = Math.Pow(editDistanceFactor, EditDistanceFactorExponent);

            double wordPositionFactorUsingCharacterIndex =
                1 - Math.Abs(
                    ((double)originalSentenceCharacterPosition / originalSentenceCharacterLength) -
                        ((double)transcribedSentenceCharacterPosition / transcribedSentenceCharacterLength));

            double wordPositionFactorUsingCharacterIndexSquared = Math.Pow(wordPositionFactorUsingCharacterIndex, PositionFactorExponent);

            double wordMatchScoreUsingCharacterIndex = editDistanceFactorSquared * wordPositionFactorUsingCharacterIndexSquared;

            if (wordMatchScoreUsingCharacterIndex < 0.0)
            {
                //throw new Exception("AudioMapper.CalculateWordMatchScore: wordMatchScoreUsingCharacterIndex lower than 0: " + originalWord);
                PutConsoleMessage("AudioMapper.CalculateWordMatchScore: wordMatchScoreUsingCharacterIndex lower than 0: " + originalWord);
                wordMatchScoreUsingCharacterIndex = 0;
            }

            double wordPositionFactorUsingWordIndex =
                1 - Math.Abs(
                    ((double)originalSentenceWordPosition / originalSentenceWordLength) -
                        ((double)transcribedSentenceWordPosition / transcribedSentenceWordLength));

            double wordPositionFactorUsingWordIndexSquared = Math.Pow(wordPositionFactorUsingWordIndex, PositionFactorExponent);

            double wordMatchScoreUsingWordIndex = editDistanceFactorSquared * wordPositionFactorUsingWordIndexSquared;

            if (wordMatchScoreUsingWordIndex < 0.0)
            {
                //throw new Exception("AudioMapper.CalculateWordMatchScore: wordMatchScoreUsingWordIndex lower than 0: " + originalWord);
                PutConsoleMessage("AudioMapper.CalculateWordMatchScore: wordMatchScoreUsingWordIndex lower than 0: " + originalWord);
                wordMatchScoreUsingWordIndex = 0;
            }

            double wordPositionFactorUsingWordAndCharacterIndexProduct = wordPositionFactorUsingWordIndex * wordPositionFactorUsingCharacterIndex;

            double wordMatchScoreUsingWordAndCharacterIndexProduct = editDistanceFactorSquared * wordPositionFactorUsingWordAndCharacterIndexProduct;

            if (wordMatchScoreUsingWordAndCharacterIndexProduct < 0.0)
            {
                //throw new Exception("AudioMapper.CalculateWordMatchScore:wordMatchScoreUsingWordAndCharacterIndexProduct lower than 0: " + originalWord);
                PutConsoleMessage("AudioMapper.CalculateWordMatchScore:wordMatchScoreUsingWordAndCharacterIndexProduct lower than 0: " + originalWord);
                wordMatchScoreUsingWordAndCharacterIndexProduct = 0;
            }

            if (wordRun != null)
            {
                wordRun.WordMatchScoreUsingCharacterIndex = wordMatchScoreUsingCharacterIndex;
                wordRun.WordMatchScoreUsingWordIndex = wordMatchScoreUsingWordIndex;
                wordRun.WordMatchScoreUsingWordAndCharacterIndex = wordMatchScoreUsingWordAndCharacterIndexProduct;
                wordRun.EditDistance = editDistance;
                wordRun.EditDistanceFactor = editDistanceFactor;
                wordRun.WordPositionFactorUsingCharacterIndex = wordPositionFactorUsingCharacterIndex;
                wordRun.WordPositionFactorUsingWordIndex = wordPositionFactorUsingWordIndex;
            }

            double wordMatchScore;

            switch (MatchScoreMode)
            {
                default:
                case MatchScoreModeCode.UseCharacterIndexOnly:
                    wordMatchScore = wordMatchScoreUsingCharacterIndex;
                    if (wordRun != null)
                    {
                        wordRun.WordMatchScore = wordMatchScoreUsingCharacterIndex;
                        wordRun.WordPositionFactor = wordPositionFactorUsingCharacterIndex;
                    }
                    break;
                case MatchScoreModeCode.UseWordIndexOnly:
                    wordMatchScore = wordMatchScoreUsingWordIndex;
                    if (wordRun != null)
                    {
                        wordRun.WordMatchScore = wordMatchScoreUsingWordIndex;
                        wordRun.WordPositionFactor = wordPositionFactorUsingWordIndex;
                    }
                    break;
                case MatchScoreModeCode.UseHighestMatchScore:
                    wordMatchScore = wordMatchScoreUsingCharacterIndex;
                    if (wordRun != null)
                    {
                        wordRun.WordMatchScore = wordMatchScoreUsingCharacterIndex;
                        wordRun.WordPositionFactor = wordPositionFactorUsingCharacterIndex;
                    }
                    break;
                case MatchScoreModeCode.UseWordAndCharacterIndexProduct:
                    wordMatchScore = wordMatchScoreUsingWordAndCharacterIndexProduct;
                    if (wordRun != null)
                    {
                        wordRun.WordMatchScore = wordMatchScoreUsingWordAndCharacterIndexProduct;
                        wordRun.WordPositionFactor = wordPositionFactorUsingWordAndCharacterIndexProduct;
                    }
                    break;
            }

            return wordMatchScore;
        }

        // Calculate edit distance score using character index.
        public double CalculateWordMatchScoreUsingCharacterIndex(
            string originalWord,
            string transcribedWord,
            int originalSentenceCharacterPosition,
            int transcribedSentenceCharacterPosition,
            int originalSentenceCharacterLength,
            int transcribedSentenceCharacterLength,
            MappingWordRun wordRun = null)  // For recording debugging information.
        {
            if (String.IsNullOrEmpty(originalWord))
                throw new Exception("AudioMapper.CalculateWordMatchScoreUsingCharacterIndex: originalWord is empty.");

            if (String.IsNullOrEmpty(transcribedWord))
                throw new Exception("AudioMapper.CalculateWordMatchScoreUsingCharacterIndex: transcribedWord is empty.");

            int originalWordLength = originalWord.Length;
            int editDistance = TextUtilities.ComputeDistance(originalWord, transcribedWord);

            double editDistanceFactor = ((double)originalWordLength + 1.0 - editDistance) / (originalWordLength + 1);

            if (editDistanceFactor < 0.0)
                editDistanceFactor = 0.0;

            double editDistanceFactorSquared = editDistanceFactor * editDistanceFactor;

            double wordPositionFactorUsingCharacterIndex =
                1 - Math.Abs(
                    ((double)originalSentenceCharacterPosition / originalSentenceCharacterLength) -
                        ((double)transcribedSentenceCharacterPosition / transcribedSentenceCharacterLength));

            double wordPositionFactorUsingCharacterIndexSquared = wordPositionFactorUsingCharacterIndex * wordPositionFactorUsingCharacterIndex;

            double wordMatchScoreUsingCharacterIndex = editDistanceFactorSquared * wordPositionFactorUsingCharacterIndexSquared;

            if (wordMatchScoreUsingCharacterIndex < 0.0)
                throw new Exception("AudioMapper.CalculateWordMatchScoreUsingCharacterIndex: Word score lower than 0: " + originalWord);

            if (wordRun != null)
            {
                wordRun.WordMatchScoreUsingCharacterIndex = wordMatchScoreUsingCharacterIndex;
                wordRun.EditDistance = editDistance;
                wordRun.EditDistanceFactor = editDistanceFactor;
                wordRun.WordPositionFactorUsingCharacterIndex = wordPositionFactorUsingCharacterIndex;
            }

            return wordMatchScoreUsingCharacterIndex;
        }

        // Calculate edit distance score using word index.
        public double CalculateWordMatchScoreUsingWordIndex(
            string originalWord,
            string transcribedWord,
            int originalSentenceWordPosition,
            int transcribedSentenceWordPosition,
            int originalSentenceWordLength,
            int transcribedSentenceWordLength,
            MappingWordRun wordRun = null)  // For recording debugging information.
        {
            if (String.IsNullOrEmpty(originalWord))
                throw new Exception("AudioMapper.CalculateWordMatchScoreUsingWordIndex: originalWord is empty.");

            if (String.IsNullOrEmpty(transcribedWord))
                throw new Exception("AudioMapper.CalculateWordMatchScoreUsingWordIndex: transcribedWord is empty.");

            int originalWordLength = originalWord.Length;
            int editDistance = TextUtilities.ComputeDistance(originalWord, transcribedWord);

            double editDistanceFactor = ((double)originalWordLength + 1.0 - editDistance) / (originalWordLength + 1);

            if (editDistanceFactor < 0.0)
                editDistanceFactor = 0.0;

            double editDistanceFactorSquared = editDistanceFactor * editDistanceFactor;

            double wordPositionFactorUsingWordIndex =
                1 - Math.Abs(
                    ((double)originalSentenceWordPosition / originalSentenceWordLength) -
                        ((double)transcribedSentenceWordPosition / transcribedSentenceWordLength));

            double wordPositionFactorUsingWordIndexSquared = wordPositionFactorUsingWordIndex * wordPositionFactorUsingWordIndex;

            double wordMatchScoreUsingWordIndex = editDistanceFactorSquared * wordPositionFactorUsingWordIndexSquared;

            if (wordMatchScoreUsingWordIndex < 0.0)
                throw new Exception("AudioMapper.CalculateWordMatchScoreUsingWordIndex: Word score lower than 0: " + originalWord);

            if (wordRun != null)
            {
                wordRun.WordMatchScoreUsingWordIndex = wordMatchScoreUsingWordIndex;
                wordRun.EditDistance = editDistance;
                wordRun.EditDistanceFactor = editDistanceFactor;
                wordRun.WordPositionFactorUsingWordIndex = wordPositionFactorUsingWordIndex;
            }

            return wordMatchScoreUsingWordIndex;
        }

        // Record matched and unmatched counts and words for sentence run.
        public void RecordMatchInfo(MappingSentenceRun sentenceRun)
        {
            int wordCount = sentenceRun.WordCount;
            int wordIndex;
            MappingWordRun wordRun;
            int matchedCount = 0;
            int unmatchedCount = 0;

            if (wordCount == 0)
                return;

            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                wordRun = sentenceRun.GetWordRun(wordIndex);

                if (wordRun == null)
                    continue;

                if (wordRun.Matched)
                    matchedCount++;
                else
                {
                    unmatchedCount++;

                    if (sentenceRun.UnmatchedWords == null)
                        sentenceRun.UnmatchedWords = new List<string>() { wordRun.WordText };
                    else
                        sentenceRun.UnmatchedWords.Add(wordRun.WordText);
                }
            }

            sentenceRun.MatchedCount = matchedCount;
            sentenceRun.UnmatchedCount = unmatchedCount;
        }

        // Set up sentence times.
        public void ConfigureSentenceTimes(
            WaveAudioBuffer audioWavData,
            List<SentenceMapping> sentenceMappings)
        {
            // Do the preliminary timing for the non-ignored sentences.
            SetupPreliminarySentenceTimes(sentenceMappings);

            // Do the main timings for the non-ignored sentences.
            SetupMainSentenceTimes(
                audioWavData,
                sentenceMappings);

            // Do the interpolated timings for the ignored sentences.
            SetupInterpolatedSentenceTimes(
                audioWavData,
                sentenceMappings);
        }


        // Set up the preliminary times in the non-ignored sentences.
        public void SetupPreliminarySentenceTimes(List<SentenceMapping> sentenceMappings)
        {
            int sentenceCount = SentenceMappings.Count();

            switch (SentenceMatchMode)
            {
                default:
                case SentenceMatchModeCode.UseCombinedSentenceScore:
                case SentenceMatchModeCode.UseMainScore:
                    // Set the times for the first and last words.
                    for (int sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
                    {
                        SentenceMapping sentenceMapping = sentenceMappings[sentenceIndex];

                        if ((DebugRowBreak != -1) && (sentenceIndex + 3 == DebugRowBreak))
                            PutConsoleMessage("SetupPreliminarySentenceTimes debug row breakpoint", DebugRowBreak.ToString());

                        if (!sentenceMapping.Ignored)
                            SetupPreliminarySentenceTimesOneSentence_Matched(
                                sentenceMappings,
                                sentenceIndex);
                    }
                    break;
                case SentenceMatchModeCode.UseSentenceDistance:
                    // Set the times for the first and last words.
                    for (int sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
                    {
                        SentenceMapping sentenceMapping = sentenceMappings[sentenceIndex];

                        if (!sentenceMapping.Ignored)
                            SetupPreliminarySentenceTimesOneSentence_EditDistance(
                                sentenceMappings,
                                sentenceIndex);
                    }
                    break;
            }

            // Reset the times for the previous sentence last and next sentence first words.
            for (int sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
            {
                SentenceMapping sentenceMapping = sentenceMappings[sentenceIndex];

                if (!sentenceMapping.Ignored)
                    FixupPreliminarySentenceTimesOneSentence(
                        sentenceMappings,
                        sentenceIndex);
            }
        }

        // Set up the preliminary times for one non-ignored sentence.
        public void SetupPreliminarySentenceTimesOneSentence_Matched(
            List<SentenceMapping> sentenceMappings,
            int sentenceIndex)
        {
            SentenceMapping sentenceMapping = sentenceMappings[sentenceIndex];
            MappingSentenceRun originalSentence = sentenceMapping.OriginalSentence;
            TimeSpan previousTranscribedWordStartTime = TimeSpan.Zero;
            TimeSpan previousTranscribedWordMiddleTime = TimeSpan.Zero;
            TimeSpan previousTranscribedWordStopTime = TimeSpan.Zero;
            TimeSpan firstTranscribedWordStartTime = TimeSpan.MaxValue;
            TimeSpan firstTranscribedWordMiddleTime = TimeSpan.MaxValue;
            TimeSpan firstTranscribedWordStopTime = TimeSpan.MaxValue;
            TimeSpan secondTranscribedWordMiddleTime = TimeSpan.Zero;
            TimeSpan lastTranscribedWordStartTime = TimeSpan.MinValue;
            TimeSpan lastTranscribedWordMiddleTime = TimeSpan.MinValue;
            TimeSpan lastTranscribedWordStopTime = TimeSpan.MinValue;
            TimeSpan nextTranscribedWordStartTime = TimeSpan.Zero;
            TimeSpan nextTranscribedWordMiddleTime = TimeSpan.Zero;
            TimeSpan nextTranscribedWordStopTime = TimeSpan.Zero;
            int firstTranscribedWordIndex = -1;
            int lastTranscribedWordIndex = -1;
            int previousTranscribedWordIndex = -1;
            int nextTranscribedWordIndex = -1;
            int wordRunCount = originalSentence.WordCount;
            int wordRunIndex;

            for (wordRunIndex = 0; wordRunIndex < wordRunCount; wordRunIndex++)
            {
                MappingWordRun originalWordRun = originalSentence.GetWordRun(wordRunIndex);
                MappingWordRun transcribedWordRun = originalWordRun.MatchedWordRun;

                if (originalWordRun.Matched && (transcribedWordRun != null))
                {
                    // Copy the word times from the matched transcribed word run.
                    originalWordRun.StartTime = transcribedWordRun.StartTime;
                    originalWordRun.StopTime = transcribedWordRun.StopTime;

                    if (originalWordRun.StartTime < firstTranscribedWordStartTime)
                    {
                        firstTranscribedWordStartTime = originalWordRun.StartTime;
                        firstTranscribedWordMiddleTime = originalWordRun.MiddleTime;
                        firstTranscribedWordStopTime = originalWordRun.StopTime;

                        MappingWordRun firstTranscribedWordRun = originalWordRun.MatchedWordRun;
                        firstTranscribedWordIndex = firstTranscribedWordRun.ParentWordIndex;
                        previousTranscribedWordIndex = firstTranscribedWordIndex - 1;

                        if (previousTranscribedWordIndex >= 0)
                        {
                            MappingWordRun previousTranscribedWordRun = TranscribedWordRuns[previousTranscribedWordIndex];
                            previousTranscribedWordStartTime = previousTranscribedWordRun.StartTime;
                            previousTranscribedWordMiddleTime = previousTranscribedWordRun.MiddleTime;
                            previousTranscribedWordStopTime = previousTranscribedWordRun.StopTime;
                        }
                    }

                    if (originalWordRun.StartTime > lastTranscribedWordStartTime)
                    {
                        lastTranscribedWordStartTime = originalWordRun.StartTime;
                        lastTranscribedWordMiddleTime = originalWordRun.MiddleTime;
                        lastTranscribedWordStopTime = originalWordRun.StopTime;

                        MappingWordRun lastTranscribedWordRun = originalWordRun.MatchedWordRun;
                        lastTranscribedWordIndex = lastTranscribedWordRun.ParentWordIndex;
                        nextTranscribedWordIndex = lastTranscribedWordIndex + 1;

                        if (nextTranscribedWordIndex < TranscribedWordRuns.Count())
                        {
                            MappingWordRun nextTranscribedWordRun = TranscribedWordRuns[nextTranscribedWordIndex];
                            nextTranscribedWordStartTime = nextTranscribedWordRun.StartTime;
                            nextTranscribedWordMiddleTime = nextTranscribedWordRun.MiddleTime;
                            nextTranscribedWordStopTime = nextTranscribedWordRun.StopTime;
                        }
                    }
                }
            }

            if (firstTranscribedWordStartTime != TimeSpan.MaxValue)
            {
                MappingWordRun secondTranscribedWordRun = originalSentence.SecondMatchedWordRun;

                if (secondTranscribedWordRun != null)
                    secondTranscribedWordMiddleTime = secondTranscribedWordRun.MiddleTime;

                MappingWordRun firstWordRun = originalSentence.FirstWordRun;
                MappingWordRun firstTranscribedWordRun = firstWordRun.MatchedWordRun;

                if (firstTranscribedWordRun != null)
                {
                    if (firstTranscribedWordStopTime < firstTranscribedWordRun.StopTime)
                        firstTranscribedWordStopTime = firstTranscribedWordRun.StopTime;
                }

                MappingWordRun lastWordRun = originalSentence.LastWordRun;
                MappingWordRun lastTranscribedWordRun = lastWordRun.MatchedWordRun;

                if (lastTranscribedWordRun != null)
                {
                    if (lastTranscribedWordStartTime < lastTranscribedWordRun.StartTime)
                        lastTranscribedWordStartTime = lastTranscribedWordRun.StartTime;
                }

                sentenceMapping.PreviousTranscribedWordStartTime = previousTranscribedWordStartTime;
                sentenceMapping.PreviousTranscribedWordMiddleTime = previousTranscribedWordMiddleTime;
                sentenceMapping.PreviousTranscribedWordStopTime = previousTranscribedWordStopTime;
                sentenceMapping.FirstTranscribedWordStartTime = firstTranscribedWordStartTime;
                sentenceMapping.FirstTranscribedWordMiddleTime = firstTranscribedWordMiddleTime;
                sentenceMapping.FirstTranscribedWordStopTime = firstTranscribedWordStopTime;
                sentenceMapping.SecondTranscribedWordMiddleTime = secondTranscribedWordMiddleTime;
                sentenceMapping.LastTranscribedWordStartTime = lastTranscribedWordStartTime;
                sentenceMapping.LastTranscribedWordMiddleTime = lastTranscribedWordMiddleTime;
                sentenceMapping.LastTranscribedWordStopTime = lastTranscribedWordStopTime;
                sentenceMapping.NextTranscribedWordStartTime = nextTranscribedWordStartTime;
                sentenceMapping.NextTranscribedWordMiddleTime = nextTranscribedWordMiddleTime;
                sentenceMapping.NextTranscribedWordStopTime = nextTranscribedWordStopTime;
            }
        }

        // Set up the preliminary times for one non-ignored sentence.
        public void SetupPreliminarySentenceTimesOneSentence_EditDistance(
            List<SentenceMapping> sentenceMappings,
            int sentenceIndex)
        {
            SentenceMapping sentenceMapping = sentenceMappings[sentenceIndex];
            MappingSentenceRun transcribedSentence = sentenceMapping.TranscribedSentence;
            TimeSpan previousTranscribedWordStartTime = TimeSpan.Zero;
            TimeSpan previousTranscribedWordMiddleTime = TimeSpan.Zero;
            TimeSpan previousTranscribedWordStopTime = TimeSpan.Zero;
            TimeSpan firstTranscribedWordStartTime = TimeSpan.MaxValue;
            TimeSpan firstTranscribedWordMiddleTime = TimeSpan.MaxValue;
            TimeSpan firstTranscribedWordStopTime = TimeSpan.MaxValue;
            TimeSpan secondTranscribedWordMiddleTime = TimeSpan.Zero;
            TimeSpan lastTranscribedWordStartTime = TimeSpan.MinValue;
            TimeSpan lastTranscribedWordMiddleTime = TimeSpan.MinValue;
            TimeSpan lastTranscribedWordStopTime = TimeSpan.MinValue;
            TimeSpan nextTranscribedWordStartTime = TimeSpan.Zero;
            TimeSpan nextTranscribedWordMiddleTime = TimeSpan.Zero;
            TimeSpan nextTranscribedWordStopTime = TimeSpan.Zero;
            MappingWordRun previousTranscribedWordRun = null;
            MappingWordRun firstTranscribedWordRun = transcribedSentence.FirstWordRun;
            MappingWordRun secondTranscribedWordRun = transcribedSentence.SecondWordRun;
            MappingWordRun lastTranscribedWordRun = transcribedSentence.LastWordRun;
            MappingWordRun nextTranscribedWordRun = null;
            int previousTranscribedWordIndex = -1;
            int firstTranscribedWordIndex = -1;
            int lastTranscribedWordIndex = -1;
            int nextTranscribedWordIndex = -1;

            if (firstTranscribedWordRun != null)
            {
                firstTranscribedWordStartTime = firstTranscribedWordRun.StartTime;
                firstTranscribedWordMiddleTime = firstTranscribedWordRun.MiddleTime;
                firstTranscribedWordStopTime = firstTranscribedWordRun.StopTime;
                firstTranscribedWordIndex = firstTranscribedWordRun.ParentWordIndex;
                previousTranscribedWordIndex = firstTranscribedWordIndex - 1;

                if (previousTranscribedWordIndex >= 0)
                {
                    previousTranscribedWordRun = TranscribedWordRuns[previousTranscribedWordIndex];
                    previousTranscribedWordStartTime = previousTranscribedWordRun.StartTime;
                    previousTranscribedWordMiddleTime = previousTranscribedWordRun.MiddleTime;
                    previousTranscribedWordStopTime = previousTranscribedWordRun.StopTime;
                }
            }

            if (secondTranscribedWordRun != null)
                secondTranscribedWordMiddleTime = secondTranscribedWordRun.MiddleTime;

            if (lastTranscribedWordRun != null)
            {
                lastTranscribedWordStartTime = lastTranscribedWordRun.StartTime;
                lastTranscribedWordMiddleTime = lastTranscribedWordRun.MiddleTime;
                lastTranscribedWordStopTime = lastTranscribedWordRun.StopTime;
                lastTranscribedWordIndex = lastTranscribedWordRun.ParentWordIndex;
                nextTranscribedWordIndex = lastTranscribedWordIndex + 1;

                if (nextTranscribedWordIndex < TranscribedWordRuns.Count())
                {
                    nextTranscribedWordRun = TranscribedWordRuns[nextTranscribedWordIndex];
                    nextTranscribedWordStartTime = nextTranscribedWordRun.StartTime;
                    nextTranscribedWordMiddleTime = nextTranscribedWordRun.MiddleTime;
                    nextTranscribedWordStopTime = nextTranscribedWordRun.StopTime;
                }
            }

            sentenceMapping.PreviousTranscribedWordStartTime = previousTranscribedWordStartTime;
            sentenceMapping.PreviousTranscribedWordMiddleTime = previousTranscribedWordMiddleTime;
            sentenceMapping.PreviousTranscribedWordStopTime = previousTranscribedWordStopTime;
            sentenceMapping.FirstTranscribedWordStartTime = firstTranscribedWordStartTime;
            sentenceMapping.FirstTranscribedWordMiddleTime = firstTranscribedWordMiddleTime;
            sentenceMapping.FirstTranscribedWordStopTime = firstTranscribedWordStopTime;
            sentenceMapping.SecondTranscribedWordMiddleTime = secondTranscribedWordMiddleTime;
            sentenceMapping.LastTranscribedWordStartTime = lastTranscribedWordStartTime;
            sentenceMapping.LastTranscribedWordMiddleTime = lastTranscribedWordMiddleTime;
            sentenceMapping.LastTranscribedWordStopTime = lastTranscribedWordStopTime;
            sentenceMapping.NextTranscribedWordStartTime = nextTranscribedWordStartTime;
            sentenceMapping.NextTranscribedWordMiddleTime = nextTranscribedWordMiddleTime;
            sentenceMapping.NextTranscribedWordStopTime = nextTranscribedWordStopTime;
        }

        // Set up the previous sentence last and next sentence first times.
        public void FixupPreliminarySentenceTimesOneSentence(
            List<SentenceMapping> sentenceMappings,
            int sentenceIndex)
        {
            SentenceMapping sentenceMapping = sentenceMappings[sentenceIndex];

            // All bets are off if we were resync'ed.
            if (sentenceMapping.RequiredResync)
                return;

            if (sentenceIndex > 0)
            {
                SentenceMapping previousSentenceMapping = sentenceMappings[sentenceIndex - 1];

                if (!previousSentenceMapping.RequiredResync)
                {
                    sentenceMapping.PreviousTranscribedWordStartTime = previousSentenceMapping.LastTranscribedWordStartTime;
                    sentenceMapping.PreviousTranscribedWordMiddleTime = previousSentenceMapping.LastTranscribedWordMiddleTime;
                    sentenceMapping.PreviousTranscribedWordStopTime = previousSentenceMapping.LastTranscribedWordStopTime;
                }
            }

            if (sentenceIndex < sentenceMappings.Count() - 1)
            {
                SentenceMapping nextSentenceMapping = sentenceMappings[sentenceIndex + 1];

                if (!nextSentenceMapping.RequiredResync)
                {
                    sentenceMapping.NextTranscribedWordStartTime = nextSentenceMapping.FirstTranscribedWordStartTime;
                    sentenceMapping.NextTranscribedWordMiddleTime = nextSentenceMapping.FirstTranscribedWordMiddleTime;
                    sentenceMapping.NextTranscribedWordStopTime = nextSentenceMapping.FirstTranscribedWordStopTime;
                }
            }
        }

        // Set up the main times in the non-ignored sentences.
        public void SetupMainSentenceTimes(
            WaveAudioBuffer audioWavData,
            List<SentenceMapping> sentenceMappings)
        {
            int sentenceCount = SentenceMappings.Count();

            for (int sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
            {
                SentenceMapping sentenceMapping = sentenceMappings[sentenceIndex];

                if (!sentenceMapping.Ignored)
                    // Do the preliminary timing for the indexed sentence.
                    SetupMainSentenceTimesOneSentence(
                        audioWavData,
                        sentenceMappings,
                        sentenceIndex);
            }
        }

        // Set up the main times for one non-ignored sentence.
        public void SetupMainSentenceTimesOneSentence(
            WaveAudioBuffer audioWavData,
            List<SentenceMapping> sentenceMappings,
            int sentenceIndex)
        {
            int sentenceCount = sentenceMappings.Count();
            SentenceMapping sentenceMappingBefore = (sentenceIndex > 0 ? sentenceMappings[sentenceIndex - 1] : null);
            SentenceMapping sentenceMappingCurrent = sentenceMappings[sentenceIndex];
            SentenceMapping sentenceMappingAfter = (sentenceIndex < sentenceCount - 1 ? sentenceMappings[sentenceIndex + 1] : null);

            if (Statistics != null)
                Statistics.SetupSentencePath(sentenceMappingCurrent.StudyItem, sentenceMappingCurrent.SentenceIndex);

            if ((DebugRowBreak != -1) && (sentenceIndex + 3 == DebugRowBreak))
                PutConsoleMessage("SetupMainSentenceTimesOneSentence debug row breakpoint", DebugRowBreak.ToString());

            if (sentenceMappingCurrent.StartTime == TimeSpan.Zero)
            {
                if ((sentenceMappingBefore != null) &&
                    !sentenceMappingBefore.RequiredResync &&
                    !sentenceMappingCurrent.RequiredResync)
                {
                    if (!ConfigureSentenceBreakAdjacentSentences(
                        audioWavData,
                        sentenceMappingBefore,
                        sentenceMappingCurrent))
                    {
                        if (Statistics != null)
                        {
                            PutConsoleMessage("Front silence break not found", Statistics.SentencePath);
                            sentenceMappingCurrent.Error = "Front silence break not found";
                        }
                    }
                }
                else
                    ConfigureSentenceStartTimeNonAdjacentSentence(
                        audioWavData,
                        sentenceMappingCurrent);
            }

            if (sentenceMappingCurrent.StopTime == TimeSpan.Zero)
            {
                if ((sentenceMappingAfter != null) &&
                    !sentenceMappingCurrent.RequiredResync &&
                    !sentenceMappingAfter.RequiredResync)
                {
                    if (!ConfigureSentenceBreakAdjacentSentences(
                        audioWavData,
                        sentenceMappingCurrent,
                        sentenceMappingAfter))
                    {
                        if (Statistics != null)
                        {
                            PutConsoleMessage("Back silence break not found", Statistics.SentencePath);
                            sentenceMappingCurrent.Error = "Back silence break not found";
                        }
                    }
                }
                else
                    ConfigureSentenceStopTimeNonAdjacentSentence(
                        audioWavData,
                        sentenceMappingCurrent);
            }
        }

        // Configure break times between the sentences (stop time of sentenceMappingBefore sentence,
        // start time of sentenceMappingAfter).
        public bool ConfigureSentenceBreakAdjacentSentences(
            WaveAudioBuffer audioWavData,
            SentenceMapping sentenceMappingBefore,
            SentenceMapping sentenceMappingAfter)
        {
            TimeSpan lowTimeLimit = sentenceMappingBefore.LastTranscribedWordStartTime;
            TimeSpan highTimeLimit = sentenceMappingAfter.SecondTranscribedWordMiddleTime;
            TimeSpan silenceStart;
            TimeSpan silenceStop;
            TimeSpan limitBefore;
            TimeSpan limitAfter;
            bool isEitherOneWord = false;

            if (lowTimeLimit < sentenceMappingBefore.SilenceStopFront)
                lowTimeLimit = sentenceMappingBefore.SilenceStopFront;

            if (sentenceMappingBefore.OriginalSentence.WordCount == 1)
            {
                limitBefore = lowTimeLimit;
                isEitherOneWord = true;
            }
            else
            {
                limitBefore = lowTimeLimit - SearchLimitDefault;

                if (limitBefore < sentenceMappingBefore.FirstTranscribedWordStopTime)
                    limitBefore = sentenceMappingBefore.FirstTranscribedWordStopTime;
            }

            if (sentenceMappingAfter.OriginalSentence.WordCount == 1)
            {
                highTimeLimit = sentenceMappingAfter.FirstTranscribedWordMiddleTime;
                isEitherOneWord = true;
            }

            if (highTimeLimit == TimeSpan.Zero)
            {
                if (sentenceMappingAfter.LastTranscribedWordStopTime != TimeSpan.Zero)
                    highTimeLimit = sentenceMappingAfter.LastTranscribedWordStopTime + SearchTimeIncrement;
                else
                    highTimeLimit = lowTimeLimit + SearchTimeIncrement + SearchTimeIncrement;
            }

            if (sentenceMappingAfter.OriginalSentence.WordCount == 1)
                limitAfter = highTimeLimit;
            else
            {
                limitAfter = highTimeLimit + SearchLimitDefault;

                if (limitAfter > sentenceMappingAfter.LastTranscribedWordStartTime)
                    limitAfter = sentenceMappingAfter.LastTranscribedWordStartTime;

                if (limitAfter == highTimeLimit)
                    limitAfter += SearchTimeIncrement;
            }

            for (;;)
            {
                if (GetBestSilence(
                    audioWavData,
                    lowTimeLimit,
                    highTimeLimit,
                    out silenceStart,
                    out silenceStop))
                {
                    if (!ObjectUtilities.TimeRunsOverlap(
                        silenceStart,
                        silenceStop,
                        sentenceMappingBefore.SilenceStartFront,
                        sentenceMappingBefore.SilenceStopFront))
                    {
                        TimeSpan silenceWidth = silenceStop - silenceStart;
                        TimeSpan leadTime = GetLeadTime(silenceWidth);
                        sentenceMappingBefore.StopTime = silenceStart + leadTime;
                        sentenceMappingBefore.SilenceStartBack = silenceStart;
                        sentenceMappingBefore.SilenceStopBack = silenceStop;
                        sentenceMappingAfter.StartTime = silenceStop - leadTime;
                        sentenceMappingAfter.SilenceStartFront = silenceStart;
                        sentenceMappingAfter.SilenceStopFront = silenceStop;
                    }
                    else
                    {
                        sentenceMappingBefore.StopTime = sentenceMappingBefore.NextTranscribedWordMiddleTime;
                        sentenceMappingBefore.SilenceStartBack = TimeSpan.Zero;
                        sentenceMappingBefore.SilenceStopBack = TimeSpan.Zero;
                        sentenceMappingAfter.StartTime = sentenceMappingAfter.FirstTranscribedWordStartTime;
                        sentenceMappingAfter.SilenceStartFront = TimeSpan.Zero;
                        sentenceMappingAfter.SilenceStopFront = TimeSpan.Zero;
                    }

                    if ((sentenceMappingBefore.StartTime != TimeSpan.Zero) && (sentenceMappingBefore.StartTime > sentenceMappingBefore.StopTime))
                    {
                        TimeSpan tmp = sentenceMappingBefore.StartTime;
                        sentenceMappingBefore.StartTime = sentenceMappingBefore.StopTime;
                        sentenceMappingBefore.StopTime = tmp;
                    }

                    break;
                }

                if (isEitherOneWord || ((lowTimeLimit == limitBefore) && (highTimeLimit == limitAfter)))
                {
                    if (GetMinimalSilence(
                        audioWavData,
                        sentenceMappingAfter.PreviousTranscribedWordStartTime,
                        sentenceMappingBefore.NextTranscribedWordStopTime,
                        out silenceStart,
                        out silenceStop))
                    {
                        TimeSpan midTime = new TimeSpan((silenceStart.Ticks + silenceStop.Ticks) / 2);
                        sentenceMappingBefore.StopTime = midTime;
                        sentenceMappingBefore.SilenceStartBack = silenceStart;
                        sentenceMappingBefore.SilenceStopBack = silenceStop;
                        sentenceMappingAfter.StartTime = midTime;
                        sentenceMappingAfter.SilenceStartFront = silenceStart;
                        sentenceMappingAfter.SilenceStopFront = silenceStop;
                    }
                    else
                    {
                        sentenceMappingBefore.StopTime = sentenceMappingBefore.NextTranscribedWordMiddleTime;
                        sentenceMappingBefore.SilenceStartBack = TimeSpan.Zero;
                        sentenceMappingBefore.SilenceStopBack = TimeSpan.Zero;
                        sentenceMappingAfter.StartTime = sentenceMappingAfter.PreviousTranscribedWordMiddleTime;
                        sentenceMappingAfter.SilenceStartFront = TimeSpan.Zero;
                        sentenceMappingAfter.SilenceStopFront = TimeSpan.Zero;
                    }

                    break;
                }

                lowTimeLimit -= SearchTimeIncrement;

                if (lowTimeLimit < limitBefore)
                    lowTimeLimit = limitBefore;

                highTimeLimit += SearchTimeIncrement;

                if (highTimeLimit > limitAfter)
                    highTimeLimit = limitAfter;
            }

            return true;
        }

        public void ConfigureSentenceStartTimeNonAdjacentSentence(
            WaveAudioBuffer audioWavData,
            SentenceMapping sentenceMapping)
        {
            TimeSpan lowTimeLimit;
            TimeSpan highTimeLimit;
            TimeSpan silenceStart;
            TimeSpan silenceStop;

            lowTimeLimit = sentenceMapping.PreviousTranscribedWordMiddleTime;

            if (lowTimeLimit < TimeSpan.Zero)
                lowTimeLimit = TimeSpan.Zero;

            if (sentenceMapping.SecondTranscribedWordMiddleTime != TimeSpan.Zero)
                highTimeLimit = sentenceMapping.SecondTranscribedWordMiddleTime;
            else
                highTimeLimit = sentenceMapping.FirstTranscribedWordMiddleTime;

            if (highTimeLimit > audioWavData.Duration)
                highTimeLimit = audioWavData.Duration;

            if (GetBestSilence(
                audioWavData,
                lowTimeLimit,
                highTimeLimit,
                out silenceStart,
                out silenceStop))
            {
                TimeSpan silenceWidth = silenceStop - silenceStart;
                TimeSpan leadTime = GetLeadTime(silenceWidth);
                sentenceMapping.StartTime = silenceStop - leadTime;
                sentenceMapping.SilenceStartFront = silenceStart;
                sentenceMapping.SilenceStopFront = silenceStop;
            }
            else
            {
                sentenceMapping.StartTime = sentenceMapping.FirstTranscribedWordStartTime - DefaultLeadTime;
                sentenceMapping.SilenceStartFront = TimeSpan.Zero;
                sentenceMapping.SilenceStopFront = TimeSpan.Zero;
            }
        }

        public void ConfigureSentenceStopTimeNonAdjacentSentence(
            WaveAudioBuffer audioWavData,
            SentenceMapping sentenceMapping)
        {
            TimeSpan lowTimeLimit;
            TimeSpan highTimeLimit;
            TimeSpan silenceStart;
            TimeSpan silenceStop;

            if (sentenceMapping.LastTranscribedWordStartTime == TimeSpan.Zero)
            {
                if (sentenceMapping.OriginalSentenceIndex >= OriginalSentenceRuns.Count() - 1)
                    lowTimeLimit = audioWavData.Duration - SearchTimeIncrement;
                else
                    return;
            }
            else
                lowTimeLimit = sentenceMapping.LastTranscribedWordStartTime;

            if (sentenceMapping.OriginalSentenceIndex >= OriginalSentenceRuns.Count() - 1)
                highTimeLimit = audioWavData.Duration;
            else if (sentenceMapping.NextTranscribedWordStopTime == audioWavData.Duration)
                highTimeLimit = audioWavData.Duration;
            else if (sentenceMapping.NextTranscribedWordStopTime == TimeSpan.Zero)
            {
                if (sentenceMapping.LastTranscribedWordStopTime == TimeSpan.Zero)
                        return;
                else
                    highTimeLimit = sentenceMapping.LastTranscribedWordStopTime + SearchLimitDefault;
            }
            else
                highTimeLimit = sentenceMapping.NextTranscribedWordStopTime + SearchTimeIncrement;

            if (highTimeLimit > audioWavData.Duration)
                highTimeLimit = audioWavData.Duration;

            if (GetBestSilence(
                audioWavData,
                lowTimeLimit,
                highTimeLimit,
                out silenceStart,
                out silenceStop))
            {
                TimeSpan silenceWidth = silenceStop - silenceStart;
                TimeSpan leadTime = GetLeadTime(silenceWidth);
                sentenceMapping.StopTime = silenceStart + leadTime;
                sentenceMapping.SilenceStartBack = silenceStart;
                sentenceMapping.SilenceStopBack = silenceStop;

                if ((sentenceMapping.StartTime != TimeSpan.Zero) && (sentenceMapping.StartTime > sentenceMapping.StopTime))
                {
                    TimeSpan tmp = sentenceMapping.StartTime;
                    sentenceMapping.StartTime = sentenceMapping.StopTime;
                    sentenceMapping.StopTime = tmp;
                }
            }
            // If last sentence...
            else if (sentenceMapping.OriginalSentenceIndex == OriginalSentenceRuns.Count() - 1)
            {
                sentenceMapping.StopTime = audioWavData.TimeLength;
                sentenceMapping.SilenceStartBack = TimeSpan.Zero;
                sentenceMapping.SilenceStartBack = TimeSpan.Zero;
            }
            else
            {
                sentenceMapping.StopTime = sentenceMapping.LastTranscribedWordStopTime + DefaultLeadTime;
                sentenceMapping.SilenceStartBack = TimeSpan.Zero;
                sentenceMapping.SilenceStartBack = TimeSpan.Zero;
            }
        }

        // Set up the interpolated sentence times for the ignored sentences.
        public void SetupInterpolatedSentenceTimes(
            WaveAudioBuffer audioWavData,
            List<SentenceMapping> sentenceMappings)
        {
            int sentenceCount = SentenceMappings.Count();

            for (int sentenceIndex = 0; sentenceIndex < sentenceCount; )
            {
                SentenceMapping sentenceMapping = sentenceMappings[sentenceIndex];

                if (sentenceMapping.Ignored)
                {
                    int sentenceEndIndex;

                    // Find next non-ignored sentence.
                    for (sentenceEndIndex = sentenceIndex + 1; sentenceEndIndex < sentenceCount; sentenceEndIndex++)
                    {
                        SentenceMapping sentenceMappingNext = sentenceMappings[sentenceEndIndex];

                        if (!sentenceMappingNext.Ignored)
                            break;
                    }

                    if ((DebugRowBreak != -1) && (sentenceIndex + 3 == DebugRowBreak))
                        PutConsoleMessage("SetupInterpolatedSentenceTimes debug row breakpoint", DebugRowBreak.ToString());

                    int ignoredSentenceCount = sentenceEndIndex - sentenceIndex;

                    if (ignoredSentenceCount == 1)
                        // Do the interpolated timings for the case of just one ignored sentence.
                        SetupInterpolatedSentenceTimesOneSentence(
                            audioWavData,
                            sentenceMappings,
                            sentenceIndex);
                    else
                        // Do the interpolated timings for the case of multiple ignored sentences.
                        SetupInterpolatedSentenceTimesMultipleSentences(
                            audioWavData,
                            sentenceMappings,
                            sentenceIndex,
                            ignoredSentenceCount);

                    sentenceIndex = sentenceEndIndex;
                }
                else
                    sentenceIndex++;
            }
        }

        // Set up the interpolated sentence times for one ignored sentence.
        public void SetupInterpolatedSentenceTimesOneSentence(
            WaveAudioBuffer audioWavData,
            List<SentenceMapping> sentenceMappings,
            int sentenceIndex)
        {
            int sentenceCount = sentenceMappings.Count();
            SentenceMapping sentenceMappingBefore = (sentenceIndex > 0 ? sentenceMappings[sentenceIndex - 1] : null);
            SentenceMapping sentenceMappingCurrent = sentenceMappings[sentenceIndex];
            SentenceMapping sentenceMappingAfter = (sentenceIndex < sentenceCount - 1 ? sentenceMappings[sentenceIndex + 1] : null);

            // Set start time of ignored sentence.
            if (sentenceMappingBefore != null)
            {
                if (sentenceMappingBefore.SilenceLengthBack != TimeSpan.Zero)
                {
                    TimeSpan leadTime = GetLeadTime(sentenceMappingBefore.SilenceLengthBack);
                    sentenceMappingCurrent.StartTime = sentenceMappingBefore.SilenceStopBack - leadTime;
                    sentenceMappingCurrent.SilenceStartFront = sentenceMappingBefore.SilenceStartBack;
                    sentenceMappingCurrent.SilenceStopFront = sentenceMappingBefore.SilenceStopBack;
                }
                else
                    sentenceMappingCurrent.StartTime = sentenceMappingBefore.StopTime;

                // Set stop time of ignored sentence.
                if (sentenceMappingAfter != null)
                {
                    if (!ReconfigureSentenceTimeUsingTooLongFirstWords(
                            audioWavData,
                            sentenceMappingCurrent,
                            sentenceMappingAfter))
                    {
                        if (sentenceMappingAfter.SilenceLengthFront != TimeSpan.Zero)
                        {
                            TimeSpan leadTime = GetLeadTime(sentenceMappingAfter.SilenceLengthFront);
                            sentenceMappingCurrent.StopTime = sentenceMappingAfter.SilenceStopFront - leadTime;
                            sentenceMappingCurrent.SilenceStartFront = sentenceMappingAfter.SilenceStartFront;
                            sentenceMappingCurrent.SilenceStopFront = sentenceMappingAfter.SilenceStopFront;
                        }
                        else
                            sentenceMappingCurrent.StopTime = sentenceMappingAfter.StartTime;
                    }
                }
                else
                    sentenceMappingCurrent.StopTime = audioWavData.Duration;
            }
            else
            {
                sentenceMappingCurrent.StartTime = TimeSpan.Zero;
                sentenceMappingCurrent.StopTime = TimeSpan.Zero;
            }
        }

        // Set up the interpolated sentence times for one ignored sentence.
        public void SetupInterpolatedSentenceTimesMultipleSentences(
            WaveAudioBuffer audioWavData,
            List<SentenceMapping> sentenceMappings,
            int sentenceIndex,
            int ignoredSentenceCount)
        {
            int sentenceCount = sentenceMappings.Count();
            SentenceMapping sentenceMappingBefore = (sentenceIndex > 0 ? sentenceMappings[sentenceIndex - 1] : null);
            SentenceMapping sentenceMappingCurrent = sentenceMappings[sentenceIndex];
            SentenceMapping sentenceMappingLast = sentenceMappings[sentenceIndex + ignoredSentenceCount - 1];
            SentenceMapping sentenceMappingAfter = (sentenceIndex + ignoredSentenceCount < sentenceCount ? sentenceMappings[sentenceIndex + ignoredSentenceCount] : null);

            if (Statistics != null)
                Statistics.SetupSentencePath(sentenceMappingCurrent.StudyItem, sentenceMappingCurrent.SentenceIndex);

            // Set start time of first ignored sentence.
            if (sentenceMappingBefore != null)
            {
                if (sentenceMappingBefore.SilenceLengthBack != TimeSpan.Zero)
                {
                    TimeSpan leadTime = GetLeadTime(sentenceMappingBefore.SilenceLengthBack);
                    sentenceMappingCurrent.StartTime = sentenceMappingBefore.SilenceStopBack - leadTime;
                    sentenceMappingCurrent.SilenceStartFront = sentenceMappingBefore.SilenceStartBack;
                    sentenceMappingCurrent.SilenceStopFront = sentenceMappingBefore.SilenceStopBack;
                }
                else
                    sentenceMappingCurrent.StartTime = sentenceMappingBefore.StopTime;
            }
            else
                sentenceMappingCurrent.StartTime = TimeSpan.Zero;

            // Set stop time of last ignored sentence.
            if (sentenceMappingAfter != null)
            {
                if (sentenceMappingAfter.SilenceLengthFront != TimeSpan.Zero)
                {
                    TimeSpan leadTime = GetLeadTime(sentenceMappingAfter.SilenceLengthFront);
                    sentenceMappingLast.StopTime = sentenceMappingAfter.SilenceStopFront - leadTime;
                    sentenceMappingLast.SilenceStartFront = sentenceMappingAfter.SilenceStartFront;
                    sentenceMappingLast.SilenceStopFront = sentenceMappingAfter.SilenceStopFront;
                }
                else
                    sentenceMappingLast.StopTime = sentenceMappingAfter.StartTime;
            }
            else
                sentenceMappingLast.StopTime = audioWavData.Duration;

            TimeSpan startTime = sentenceMappingCurrent.StartTime;
            TimeSpan stopTime = sentenceMappingLast.StopTime;
            TimeSpan totalIgnoredTime = stopTime - startTime;
            int totalWordCount = 0;
            int accumulatedWordCount = 0;

            // Get total word count for ignored sentences.
            for (int index = 0; index < ignoredSentenceCount - 1; index++)
            {
                SentenceMapping sentenceMapping = sentenceMappings[index];
                totalWordCount += sentenceMapping.OriginalSentence.WordCount;
            }

            // Set up interpolated best sentence times based on word counts..
            for (int index = 0; index < ignoredSentenceCount - 1; index++)
            {
                SentenceMapping sentenceMapping = sentenceMappings[index];
                TimeSpan sentenceStartTime = TimeSpan.FromSeconds(
                    (totalIgnoredTime.TotalSeconds * accumulatedWordCount) / totalWordCount);
                accumulatedWordCount += sentenceMapping.OriginalSentence.WordCount;
                TimeSpan sentenceStopTime = TimeSpan.FromSeconds(
                    (totalIgnoredTime.TotalSeconds * accumulatedWordCount) / totalWordCount);
                sentenceMapping.FirstTranscribedWordStartTime = sentenceMapping.FirstTranscribedWordStopTime = sentenceStartTime;
                sentenceMapping.LastTranscribedWordStartTime = sentenceMapping.LastTranscribedWordStopTime = sentenceStopTime;
            }

            accumulatedWordCount = 0;

            // Set up sentence times using interpolated best times and silence breaks.
            for (int index = 0; index < ignoredSentenceCount - 1; index++)
            {
                SentenceMapping sentenceMapping = sentenceMappings[index];
                SentenceMapping sentenceMappingNext = sentenceMappings[index + 1];
                ConfigureSentenceBreakAdjacentSentences(
                    audioWavData,
                    sentenceMapping,
                    sentenceMappingNext);
            }
        }

        // Look for and handle special case where transcribed text is missing,
        // but the time is included in the next sentence at or near the beginning.
        // If detected, try to find the sentence break silence.
        // Return true if handled.
        public bool ReconfigureSentenceTimeUsingTooLongFirstWords(
            WaveAudioBuffer audioWavData,
            SentenceMapping sentenceMappingBefore,
            SentenceMapping sentenceMapping)
        {
            TimeSpan lowTimeLimit;
            TimeSpan highTimeLimit;
            TimeSpan silenceStart;
            TimeSpan silenceStop;
            MappingSentenceRun originalSentence = sentenceMapping.OriginalSentence;
            int wordIndex;
            int wordCount = originalSentence.WordCount;
            MappingWordRun nextTranscribedWordRun = null;
            MappingWordRun lastTranscribedWordRun = null;

            // Starting at first word, skip any words less than a second long or not matched.
            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                MappingWordRun wordRun = originalSentence.GetWordRun(wordIndex);

                if (wordRun.MatchedWordRun == null)
                    continue;

                lastTranscribedWordRun = wordRun.MatchedWordRun;

                if (wordRun.MatchedWordRun.Duration.TotalSeconds >= 1.0)
                    break;
            }

            if (wordIndex >= wordCount / 2)
                return false;

            // Starting where we were, find last word with too much time.
            for (; wordIndex < wordCount; wordIndex++)
            {
                MappingWordRun wordRun = originalSentence.GetWordRun(wordIndex);

                if (wordRun.MatchedWordRun == null)
                    break;

                if (wordRun.MatchedWordRun.Duration.TotalSeconds < 1.0)
                    break;

                lastTranscribedWordRun = wordRun.MatchedWordRun;
            }

            if (lastTranscribedWordRun == null)
                return false;

            if (wordIndex >= wordCount / 2)
                return false;

            nextTranscribedWordRun = originalSentence.GetWordRun(wordIndex).MatchedWordRun;

            lowTimeLimit = lastTranscribedWordRun.StartTime;
            highTimeLimit = lastTranscribedWordRun.StopTime - SearchLimitDefault;

            TimeSpan silenceWidth = TimeSpan.Zero;
            bool found = false;

            if (GetBestSilence(
                audioWavData,
                lowTimeLimit,
                highTimeLimit,
                out silenceStart,
                out silenceStop))
            {
                silenceWidth = silenceStop - silenceStart;

                if (silenceWidth > MinimumLeadTime)
                    found = true;
                else
                {
                    highTimeLimit = lastTranscribedWordRun.StopTime;

                    if (GetBestSilence(
                            audioWavData,
                            lowTimeLimit,
                            highTimeLimit,
                            out silenceStart,
                            out silenceStop))
                    {
                        silenceWidth = silenceStop - silenceStart;

                        if (silenceWidth > MinimumLeadTime)
                            found = true;
                    }
                }
            }

            if (found)
            {
                TimeSpan leadTime = GetLeadTime(silenceWidth);
                sentenceMappingBefore.StopTime = silenceStart + leadTime;
                sentenceMappingBefore.SilenceStartBack = silenceStart;
                sentenceMappingBefore.SilenceStopBack = silenceStop;
                sentenceMappingBefore.Error = "Stop time readjusted skipping too-long words of next sentence.";
                sentenceMapping.StartTime = silenceStop - leadTime;
                sentenceMapping.SilenceStartFront = silenceStart;
                sentenceMapping.SilenceStopFront = silenceStop;
                sentenceMapping.Error = "Start time readjusted skipping too-long words.";
            }
            else
            {
                sentenceMappingBefore.StopTime = lastTranscribedWordRun.StopTime - DefaultLeadTime;
                sentenceMappingBefore.SilenceStartBack = TimeSpan.Zero;
                sentenceMappingBefore.SilenceStopBack = TimeSpan.Zero;
                sentenceMappingBefore.Error = "Start time readjusted skipping too-long words of next sentence, though silence not found.";
                sentenceMapping.StartTime = lastTranscribedWordRun.StopTime - DefaultLeadTime;
                sentenceMapping.SilenceStartFront = TimeSpan.Zero;
                sentenceMapping.SilenceStopFront = TimeSpan.Zero;
                sentenceMapping.Error = "Start time readjusted skipping too-long words, though silence not found.";
            }

            return true;
        }

        protected bool GetBestSilence(
            WaveAudioBuffer audioWavData,
            TimeSpan startTime,
            TimeSpan endTime,
            out TimeSpan silenceStart,
            out TimeSpan silenceStop)
        {
            int sampleIndexA;
            int sampleIndexB;
            int silenceStartIndex = 0;
            int silenceStopIndex = 0;
            int maxSampleIndex = audioWavData.SampleCount;
            TimeSpan startTimeA;
            TimeSpan startTimeB;
            int amplitude = AmplitudeThreshold;
            int silenceWidthInitial = audioWavData.GetSampleIndexFromTime(SilenceWidthThreshold);
            int silenceWidth = silenceWidthInitial;
            int widthDivisor = 1;
            bool found = false;

            for (widthDivisor = 1; widthDivisor <= 5; widthDivisor++)
            {
                startTimeA = startTime;
                startTimeB = endTime;

                sampleIndexA = audioWavData.GetSampleIndexFromTime(startTimeA);
                sampleIndexB = audioWavData.GetSampleIndexFromTime(startTimeB);

                if (sampleIndexB > maxSampleIndex)
                    sampleIndexB = maxSampleIndex;

                silenceWidth = silenceWidthInitial / widthDivisor;

                amplitude = AmplitudeThreshold;

                while (!(found = audioWavData.FindLongestSilence(
                    sampleIndexA,
                    sampleIndexB,
                    amplitude,
                    silenceWidth,
                    out silenceStartIndex,
                    out silenceStopIndex)))
                {
                    if (amplitude >= AmplitudeSilenceMax)
                        break;

                    amplitude += AmplitudeIncrement;
                }

                if (found)
                    break;
            }

            if (!found)
            {
                silenceStart = TimeSpan.Zero;
                silenceStop = TimeSpan.Zero;
                return false;
            }

            int silenceMidIndex = (silenceStartIndex + silenceStopIndex) / 2;

            if (audioWavData.FindSilenceRange(
                silenceMidIndex,
                amplitude,
                out silenceStartIndex,
                out silenceStopIndex))
            {
                silenceStart = audioWavData.GetSampleTime(silenceStartIndex);
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

        protected bool GetMinimalSilence(
            WaveAudioBuffer audioWavData,
            TimeSpan startTime,
            TimeSpan endTime,
            out TimeSpan silenceStart,
            out TimeSpan silenceStop)
        {
            int sampleIndexA;
            int sampleIndexB;
            int silenceStartIndex = 0;
            int silenceStopIndex = 0;
            int maxSampleIndex = audioWavData.SampleCount;
            int amplitude = AmplitudeThreshold;
            int silenceWidth = audioWavData.GetSampleIndexFromTime(SilenceWidthThresholdMinimal);
            bool found = false;

            sampleIndexA = audioWavData.GetSampleIndexFromTime(startTime);
            sampleIndexB = audioWavData.GetSampleIndexFromTime(endTime);

            if (sampleIndexB > maxSampleIndex)
                sampleIndexB = maxSampleIndex;

            while (!(found = audioWavData.FindLongestSilence(
                sampleIndexA,
                sampleIndexB,
                amplitude,
                silenceWidth,
                out silenceStartIndex,
                out silenceStopIndex)))
            {
                if (amplitude >= AmplitudeSilenceMax)
                    break;

                amplitude += AmplitudeIncrement;
            }

            if (!found)
            {
                silenceStart = TimeSpan.Zero;
                silenceStop = TimeSpan.Zero;
                return false;
            }

            int silenceMidIndex = (silenceStartIndex + silenceStopIndex) / 2;

            if (audioWavData.FindSilenceRange(
                silenceMidIndex,
                amplitude,
                out silenceStartIndex,
                out silenceStopIndex))
            {
                silenceStart = audioWavData.GetSampleTime(silenceStartIndex);
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

        protected TimeSpan GetLeadTime(TimeSpan silenceWidth)
        {
            if (silenceWidth < MinimumLeadTime)
                return new TimeSpan(silenceWidth.Ticks / 3);

            return DefaultLeadTime;
        }

        // Get raw audio data (for debugging).
        protected int[] GetAudioData(
            WaveAudioBuffer audioWavData,
            TimeSpan lowTimeLimit,
            TimeSpan highTimeLimit)
        {
            int sampleIndexA = audioWavData.GetSampleIndexFromTime(lowTimeLimit);
            int sampleIndexB = audioWavData.GetSampleIndexFromTime(highTimeLimit);
            int sampleCount = sampleIndexB - sampleIndexA;
            int[] audioData = new int[sampleCount];

            if (audioWavData.Open(MediaInterfaces.PortableFileMode.Open))
            {
                for (int index = 0; index < sampleCount; index++)
                    audioData[index] = audioWavData.GetMonoSample(sampleIndexA + index);

                audioWavData.Close();
            }

            return audioData;
        }

        // Take the timing info from the study and parsed sentence runs and set up
        // media runs in the study items.
        protected void ConfigureMediaRuns(
            List<MultiLanguageItem> studyItems,
            string mediaItemKey,
            string languageMediaItemKey,
            List<SentenceMapping> sentenceMappings)
        {
            int sentenceCount = SentenceMappings.Count();

            for (int sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
            {
                SentenceMapping sentenceMapping = sentenceMappings[sentenceIndex];
                TextRun sentenceRun = sentenceMapping.StudySentence;
                MediaRun mediaRun = sentenceRun.GetMediaRunWithReferenceKeys(mediaItemKey, languageMediaItemKey);

                if (sentenceMapping.Duration == TimeSpan.Zero)
                {
                    MultiLanguageItem studyItem = sentenceMapping.StudyItem;
                    string namePathString = studyItem.GetNameStringWithOrdinal() + "(" + sentenceMapping.SentenceIndex.ToString() + ")";

                    PutError("Sentence with no media time for " + TargetLanguageTool.LanguageName + ": " + namePathString);

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
                        sentenceMapping.StartTime,
                        sentenceMapping.Duration);
                    sentenceRun.AddMediaRun(mediaRun);
                }
                else
                {
                    mediaRun.Start = sentenceMapping.StartTime;
                    mediaRun.Length = sentenceMapping.Duration;
                }
            }
        }

        // Clone and deep copy a range of word runs.
        public List<MappingWordRun> CopyWordRuns(
            List<MappingWordRun> parentWordRuns,
            int parentWordStartIndex,
            int wordCount)
        {
            int parentWordCount = parentWordRuns.Count();
            List<MappingWordRun> wordRuns = new List<MappingWordRun>();
            int sentenceCharacterPosition = 0;
            int sentenceWordPosition = 0;

            for (int index = 0; index < wordCount; index++)
            {
                int parentWordIndex = parentWordStartIndex + index;

                if (parentWordIndex > parentWordCount)
                    break;

                MappingWordRun parentWordRun = parentWordRuns[parentWordIndex];
                MappingWordRun wordRun = new MappingWordRun(parentWordRun);
                wordRun.SentenceCharacterPosition = sentenceCharacterPosition;
                wordRun.SentenceWordIndex = sentenceWordPosition;
                wordRuns.Add(wordRun);
                sentenceCharacterPosition += wordRun.TextLength;
                sentenceWordPosition++;
            }

            return wordRuns;
        }

        // Replace one word run with one or more word runs.
        public void ReplaceWordRun(
            List<MappingWordRun> wordRuns,
            int startIndex,
            List<MappingWordRun> newWordRuns)
        {
            DeleteWordRunRange(
                wordRuns,
                startIndex,
                1);
            InsertWordRunRange(
                wordRuns,
                startIndex,
                newWordRuns);
        }

        // Replace a range of word runs with one word run constructed from the given word.
        public void ReplaceWordRunRange(
            List<MappingWordRun> wordRuns,
            int startIndex,
            int wordCount,
            string newWord)
        {
            MappingWordRun newWordRun = new MappingWordRun(
                wordRuns,
                startIndex,
                wordCount);
            ReplaceWordRunRange(wordRuns, startIndex, wordCount, newWordRun);
        }

        // Replace a range of word runs with one word run constructed from the given word.
        public void ReplaceWordRunRange(
            List<MappingWordRun> wordRuns,
            int startIndex,
            int wordCount,
            MappingWordRun newWordRun)
        {
            DeleteWordRunRange(
                wordRuns,
                startIndex,
                wordCount);
            InsertWordRun(
                wordRuns,
                startIndex,
                wordCount,
                newWordRun);
        }

        // Replace a range of word runs with another word run range.
        public void ReplaceWordRunRange(
            List<MappingWordRun> wordRuns,
            int startIndex,
            int wordCount,
            List<MappingWordRun> newWordRuns)
        {
            DeleteWordRunRange(
                wordRuns,
                startIndex,
                wordCount);
            InsertWordRunRange(
                wordRuns,
                startIndex,
                newWordRuns);
        }

        // Delete a range of word runs, including the text block.
        public void InsertWordRun(
            List<MappingWordRun> wordRuns,
            int startIndex,
            int wordCount,
            MappingWordRun newWordRun)
        {
            if (newWordRun == null)
                return;
            List<MappingWordRun> newWordRuns = new List<MappingWordRun>() { newWordRun };
            InsertWordRunRange(wordRuns, startIndex, newWordRuns);
        }

        // Delete a range of word runs, including the text block.
        public void InsertWordRunRange(
            List<MappingWordRun> wordRuns,
            int startIndex,
            List<MappingWordRun> newWordRuns)
        {
            if ((newWordRuns == null) || (newWordRuns.Count() == 0))
                return;

            int wordCount = newWordRuns.Count();

            if ((startIndex < 0) || (startIndex > wordRuns.Count()))
                return;

            string concatenatedText = ConcatenateWordRunText(newWordRuns, 0, wordCount);
            int newTextLength = concatenatedText.Length;
            MappingWordRun firstWordRun = newWordRuns.First();
            MappingWordRun lastWordRun = newWordRuns.Last();
            int sentenceCharacterPosition = firstWordRun.SentenceCharacterPosition;
            int newRunsTextLength = lastWordRun.SentenceTextStopIndex - firstWordRun.SentenceTextStartIndex;

            wordRuns.InsertRange(startIndex, newWordRuns);

            UpdateWordRunSentenceCharacterPosition(wordRuns, startIndex, sentenceCharacterPosition);
            UpdateWordRunSentenceTextIndex(wordRuns, startIndex + wordCount, newRunsTextLength);
            UpdateWordRunSentenceWordIndexes(wordRuns, startIndex + wordCount, wordCount);
        }

        // Delete a range of word runs, including the text block.
        public void DeleteWordRunRange(
            List<MappingWordRun> wordRuns,
            int startIndex,
            int wordCount)
        {
            if (wordCount <= 0)
                return;

            MappingWordRun firstWordRun = wordRuns[startIndex];
            MappingWordRun lastWordRun = wordRuns[startIndex + wordCount - 1];
            int sentenceCharacterPosition = firstWordRun.SentenceCharacterPosition;
            int sentenceTextStartIndex = firstWordRun.SentenceTextStartIndex;
            int deletedTextLength = lastWordRun.SentenceTextStopIndex - firstWordRun.SentenceTextStartIndex;

            wordRuns.RemoveRange(startIndex, wordCount);

            UpdateWordRunSentenceCharacterPosition(wordRuns, startIndex, sentenceCharacterPosition);
            UpdateWordRunSentenceTextIndex(wordRuns, startIndex, -deletedTextLength);
            UpdateWordRunSentenceWordIndexes(wordRuns, startIndex, -wordCount);
        }

        // Update sentence character position index of remainder word runs.
        public void UpdateWordRunSentenceCharacterPosition(
            List<MappingWordRun> wordRuns,
            int startIndex,
            int startSentenceCharacterPosition)
        {
            int wordCount = wordRuns.Count();
            int sentenceCharacterPosition = startSentenceCharacterPosition;

            for (int i = startIndex; i < wordCount; i++)
            {
                MappingWordRun wordRun = wordRuns[i];
                wordRun.SentenceCharacterPosition = sentenceCharacterPosition;
                sentenceCharacterPosition += wordRun.TextLength;
            }
        }

        // Update sentence character position index of remainder word runs.
        public void UpdateWordRunSentenceTextIndex(
            List<MappingWordRun> wordRuns,
            int startIndex,
            int delta)
        {
            int wordCount = wordRuns.Count();

            for (int i = startIndex; i < wordCount; i++)
            {
                MappingWordRun wordRun = wordRuns[i];
                wordRun.SentenceTextStartIndex += delta;
                wordRun.TextStartIndex += delta;
            }
        }

        // Update word run sentence word indexes.
        public void UpdateWordRunSentenceWordIndexes(
            List<MappingWordRun> wordRuns,
            int startIndex,
            int delta)
        {
            int wordCount = wordRuns.Count();

            for (int i = startIndex; i < wordCount; i++)
            {
                MappingWordRun wordRun = wordRuns[i];
                wordRun.SentenceWordIndex += delta;
                wordRun.ParentWordIndex += delta;
            }
        }

        // Mark the transcribed word runs used by a sentence mapping as used.
        public void MarkTranscribedRunsUsed(SentenceMapping sentenceMapping)
        {
            int endIndex = sentenceMapping.TranscribedSentence.ParentStopWordIndex;
            int index = sentenceMapping.TranscribedSentence.ParentStartWordIndex;

            for (; index < endIndex; index++)
                TranscribedWordRuns[index].Used = true;
        }

        // Get the raw transcribed sentence text.
        public void GetRawTranscribedSentenceText(SentenceMapping sentenceMapping)
        {
            MappingSentenceRun transcribedSentence = sentenceMapping.TranscribedSentence;

            if (transcribedSentence == null)
                return;

            int startIndex = transcribedSentence.ParentStartWordIndex;

            if (startIndex >= TranscribedWordRuns.Count())
                return;

            int stopIndex = transcribedSentence.ParentStopWordIndex - 1;

            if (stopIndex >= TranscribedWordRuns.Count())
                return;

            MappingWordRun startWordRun = TranscribedWordRuns[startIndex];
            MappingWordRun stopWordRun = TranscribedWordRuns[stopIndex];
            int textStartIndex = startWordRun.RawTextStartIndex;
            int textStopIndex = stopWordRun.RawTextStopIndex;
            string rawText = RawTranscribedTextBlock.Substring(textStartIndex, textStopIndex - textStartIndex);
            transcribedSentence.RawSentenceText = rawText;
        }

        // Compose a string by concatenating the words from the specified word runs.
        public string ConcatenateWordRunText(List<MappingWordRun> wordRuns,
            int startIndex,
            int wordCount)
        {
            if (wordCount == 1)
            {
                MappingWordRun wordRun = wordRuns[startIndex];
                return wordRun.WordText;
            }

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < wordCount; i++)
            {
                MappingWordRun wordRun = wordRuns[startIndex + i];

                if (i != 0)
                    sb.Append(" ");

                sb.Append(wordRun.WordText);
            }

            return sb.ToString();
        }

        // Copy audio mapper data.
        public void CopyAudioMapper(AudioMapper other)
        {
            TargetLanguageTool = other.TargetLanguageTool;
            SentenceMappings = other.CloneSentenceMappings();
            TranscribedWordRuns = other.CloneTranscribedWordRuns();
            Statistics = null;
        }

        // Clone sentence mappings list.
        public List<SentenceMapping> CloneSentenceMappings()
        {
            if (SentenceMappings == null)
                return null;

            List<SentenceMapping> sentenceMappings = new List<SentenceMapping>();

            foreach (SentenceMapping sentenceMapping in SentenceMappings)
                sentenceMappings.Add(new SentenceMapping(sentenceMapping));

            return sentenceMappings;
        }

        // Clone transcribed word runs list.
        public List<MappingWordRun> CloneTranscribedWordRuns()
        {
            if (TranscribedWordRuns == null)
                return null;

            List<MappingWordRun> wordRuns = new List<MappingWordRun>();

            foreach (MappingWordRun wordRun in TranscribedWordRuns)
                wordRuns.Add(new MappingWordRun(wordRun));

            return wordRuns;
        }

        // Clone statistics.
        public AudioMapperStatistics CloneStatistics()
        {
            if (Statistics == null)
                return null;

            return new Language.AudioMapperStatistics(Statistics);
        }

        public void PutExceptionError(Exception exc)
        {
            string message = exc.Message;

            if (exc.InnerException != null)
                message = message + ": " + exc.InnerException.Message;

            if (String.IsNullOrEmpty(Error))
                Error = message;
            else if (!Error.Contains(message))
                Error = Error + "\r\n" + message;
        }

        public void PutExceptionError(string message, Exception exc)
        {
            string fullMessage = message + ": " + exc.Message;

            if (exc.InnerException != null)
                fullMessage = fullMessage + ": " + exc.InnerException.Message;

            if (String.IsNullOrEmpty(Error))
                Error = fullMessage;
            else if (!Error.Contains(fullMessage))
                Error = Error + "\r\n" + fullMessage;
        }

        public void PutExceptionError(string message, string argument, Exception exc)
        {
            string fullMessage = message + ": " + argument + ": " + exc.Message;

            if (exc.InnerException != null)
                fullMessage = fullMessage + ": " + exc.InnerException.Message;

            if (String.IsNullOrEmpty(Error))
                Error = fullMessage;
            else if (!Error.Contains(fullMessage))
                Error = Error + "\r\n" + fullMessage;
        }

        public void PutError(string message)
        {
            string str = message;

            if (String.IsNullOrEmpty(Error))
                Error = str;
            else if (!Error.Contains(str))
                Error = Error + "\r\n" + str;
        }

        public void PutError(string message, string arg1)
        {
            string str = message + ": " + arg1;

            if (String.IsNullOrEmpty(Error))
                Error = str;
            else if (!Error.Contains(str))
                Error = Error + "\r\n" + str;
        }

        public void ClearError()
        {
            Error = String.Empty;
        }

        public void PutConsoleMessage(string message)
        {
            ApplicationData.Global.PutConsoleMessage(message);
        }

        public void PutConsoleMessage(string message, string arg1)
        {
            PutConsoleMessage(message + ": " + arg1);
        }

        // Dump candidate state.
        public void DumpDebugWordMappingCandidateState(
            string mainLabel,
            string extraMessage,
            int offset,
            SentenceMapping sentenceMapping)
        {
            if (Statistics != null)
                Statistics.DumpDebugWordMappingCandidateState(
                    mainLabel,
                    extraMessage,
                    offset,
                    sentenceMapping);
        }

        // Dump mapping results label.
        protected void DumpDebugWordMappingResultsLabel(string label)
        {
            if (Statistics != null)
                Statistics.DumpDebugWordMappingResultsLabel(label);
        }

        // Handle reporting.
        public void HandleReporting(List<SentenceMapping> sentenceMappings)
        {
            if (Statistics != null)
                Statistics.DumpMappingResults(sentenceMappings);
        }
    }
}
