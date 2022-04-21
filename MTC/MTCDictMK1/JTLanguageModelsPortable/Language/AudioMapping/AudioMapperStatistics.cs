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
using JTLanguageModelsPortable.Helpers;

namespace JTLanguageModelsPortable.Language
{
    public class AudioMapperStatistics : BaseObjectKeyed
    {
        // Current content path.
        public string ContentPath { get; set; }

        // Current sentence path.
        public string SentencePath { get; set; }

        // Current code label (i.e. "Original", "Transcribed").
        public string CodeLabel { get; set; }

        // Test name.
        public string TestName { get; set; }

        // How many sentences mapped.
        public int MappedSentenceCount { get; set; }

        // How many sentences with a main score of .75 or below.
        public int CombinedSentenceScorePoint75 { get; set; }

        // How many sentences with a main score of .25 or below.
        public int CombinedSentenceScorePoint25 { get; set; }

        // How many sentences match the first word.
        public int SentenceMatchFirstWordCount { get; set; }

        // How many sentences don't match the first word.
        public int SentenceNotMatchFirstWordCount { get; set; }

        // How many sentences match the last word.
        public int SentenceMatchLastWordCount { get; set; }

        // How many sentences don't match the last word.
        public int SentenceNotMatchLastWordCount { get; set; }

        // Sum of sentence main scores.
        public double SentenceMainScoreSum { get; set; }

        // Sum of sentence extra scores.
        public double SentenceExtraScoreSum { get; set; }

        // Sum of sentence combined scores.
        public double SentenceCombinedScoreSum { get; set; }

        // Sum of edit distances of original and transcribed raw sentences.
        public int SentenceEditDistanceRawSum { get; set; }

        // Sum of edit distances of original and transcribed processed sentences.
        public int SentenceEditDistanceProcessedSum { get; set; }

        // Sum of edit distances of original and transcribed matched word sentences.
        public int SentenceEditDistanceMatchedSum { get; set; }

        // Sum of first word scores.
        public double FirstWordScoreSum { get; set; }

        // Sum of last word scores.
        public double LastWordScoreSum { get; set; }

        // Sum of first and last word scores.
        public double FirstAndLastWordScoreSum { get; set; }

        // How many times a resync was triggered.
        public int RequiredResyncCount { get; set; }

        // How many times a sentence failed to be resyced and was ignored.
        public int IgnoredSentenceCount { get; set; }

        // Do main sentence report.
        public bool DumpSentenceMappingResults { get; set; }

        // String builder for main sentence report.
        public StringBuilder DumpSentenceMappingSB { get; set; }

        // Do main word report.
        public bool DumpWordMappingResults { get; set; }

        // String builder for main wprd report.
        public StringBuilder DumpWordMappingSB { get; set; }

        // Do word tracing report.
        public bool DumpWordTracingResults { get; set; }

        // String builder for word trace report.
        public StringBuilder DumpWordTracingSB { get; set; }

        // Do number normalization report.
        public bool DumpNumberTracingResults { get; set; }

        // String builder for number normalization report.
        public StringBuilder DumpNumberTracingSB { get; set; }

        // Do abbreviation normalization report.
        public bool DumpAbbreviationTracingResults { get; set; }

        // String builder for abbreviation normalization report.
        public StringBuilder DumpAbbreviationTracingSB { get; set; }

        // Do test map report.
        public bool DumpTestMapResults { get; set; }

        // String builder for test map report.
        public StringBuilder DumpTestMapSB { get; set; }

        // Speech-to-Text timer.
        public SoftwareTimer SpeechToTextTimer { get; set; }

        // Speech-to-Text time.
        public double SpeechToTextTime { get; set; }

        // Mapping timer.
        public SoftwareTimer MappingTimer { get; set; }

        // Mapping time.
        public double MappingTime { get; set; }

        // Command line.
        public string CommandLine { get; set; }

        // Parent statistics.  Info relayed here for a combined report.
        public AudioMapperStatistics Parent { get; set; }

        public static string TimeFormat = @"mm\:ss\.ff";

        public AudioMapperStatistics(
            string key,
            AudioMapperStatistics parent) : base(key)
        {
            ClearAudioMapperStatistics();
            Parent = parent;
        }

        public AudioMapperStatistics(AudioMapperStatistics other)
        {
            ClearAudioMapperStatistics();
            CopyAudioMapperStatistics(other);
        }

        public AudioMapperStatistics()
        {
            ClearAudioMapperStatistics();
        }

        protected void ClearAudioMapperStatistics()
        {
            ContentPath = String.Empty;
            SentencePath = String.Empty;
            CodeLabel = String.Empty;
            TestName = String.Empty;
            MappedSentenceCount = 0;
            CombinedSentenceScorePoint75 = 0;
            CombinedSentenceScorePoint25 = 0;
            SentenceMatchFirstWordCount = 0;
            SentenceNotMatchFirstWordCount = 0;
            SentenceMatchLastWordCount = 0;
            SentenceNotMatchLastWordCount = 0;
            SentenceMainScoreSum = 0.0;
            SentenceExtraScoreSum = 0.0;
            SentenceCombinedScoreSum = 0.0;
            SentenceEditDistanceRawSum = 0;
            SentenceEditDistanceProcessedSum = 0;
            SentenceEditDistanceMatchedSum = 0;
            FirstWordScoreSum = 0.0;
            LastWordScoreSum = 0.0;
            FirstAndLastWordScoreSum = 0.0;
            RequiredResyncCount = 0;
            IgnoredSentenceCount = 0;
            DumpSentenceMappingResults = false;
            DumpSentenceMappingSB = null;
            DumpWordMappingResults = false;
            DumpWordMappingSB = null;
            DumpWordTracingResults = false;
            DumpWordTracingSB = null;
            DumpNumberTracingResults = false;
            DumpNumberTracingSB = null;
            DumpAbbreviationTracingResults = false;
            DumpAbbreviationTracingSB = null;
            DumpTestMapResults = false;
            DumpTestMapSB = null;
            SpeechToTextTimer = null;
            MappingTimer = null;
            CommandLine = "";
            Parent = null;
        }

        protected void CopyAudioMapperStatistics(AudioMapperStatistics other)
        {
            ContentPath = other.ContentPath;
            SentencePath = other.SentencePath;
            CodeLabel = other.CodeLabel;
            TestName = other.TestName;
            MappedSentenceCount = other.MappedSentenceCount;
            CombinedSentenceScorePoint75 = other.CombinedSentenceScorePoint75;
            CombinedSentenceScorePoint25 = other.CombinedSentenceScorePoint25;
            SentenceMatchFirstWordCount = other.SentenceMatchFirstWordCount;
            SentenceNotMatchFirstWordCount = other.SentenceNotMatchFirstWordCount;
            SentenceMatchLastWordCount = other.SentenceMatchLastWordCount;
            SentenceNotMatchLastWordCount = other.SentenceNotMatchLastWordCount;
            SentenceMainScoreSum = other.SentenceMainScoreSum;
            SentenceExtraScoreSum = other.SentenceExtraScoreSum;
            SentenceCombinedScoreSum = other.SentenceCombinedScoreSum;
            SentenceEditDistanceRawSum = other.SentenceEditDistanceRawSum;
            SentenceEditDistanceProcessedSum = other.SentenceEditDistanceProcessedSum;
            SentenceEditDistanceMatchedSum = other.SentenceEditDistanceMatchedSum;
            FirstWordScoreSum = other.FirstWordScoreSum;
            LastWordScoreSum = other.LastWordScoreSum;
            FirstAndLastWordScoreSum = other.FirstAndLastWordScoreSum;
            RequiredResyncCount = other.RequiredResyncCount;
            IgnoredSentenceCount = other.IgnoredSentenceCount;
            DumpSentenceMappingResults = other.DumpSentenceMappingResults;
            DumpSentenceMappingSB = other.DumpSentenceMappingSB;
            DumpWordMappingResults = other.DumpWordMappingResults;
            DumpWordMappingSB = other.DumpWordMappingSB;
            DumpWordTracingResults = other.DumpWordTracingResults;
            DumpWordTracingSB = other.DumpWordTracingSB;
            DumpNumberTracingResults = other.DumpNumberTracingResults;
            DumpNumberTracingSB = other.DumpNumberTracingSB;
            DumpAbbreviationTracingResults = other.DumpAbbreviationTracingResults;
            DumpAbbreviationTracingSB = other.DumpAbbreviationTracingSB;
            DumpTestMapResults = other.DumpTestMapResults;
            DumpTestMapSB = other.DumpTestMapSB;
            SpeechToTextTimer = null;
            MappingTimer = null;
            CommandLine = other.CommandLine;
            Parent = other.Parent;
        }

        public void UpdateSums(SentenceMapping sentenceMapping)
        {
            // Get the main score, possible resynced.
            double mainScore = sentenceMapping.MainScore;
            double extraScore = sentenceMapping.ExtraScore;
            double combinedScore = sentenceMapping.CombinedScore;

            if (combinedScore < 0.75)
                CombinedSentenceScorePoint75++;

            if (combinedScore < 0.25)
                CombinedSentenceScorePoint25++;

            MappedSentenceCount++;

            if (sentenceMapping.IsFirstWordMatched())
                SentenceMatchFirstWordCount++;
            else
                SentenceNotMatchFirstWordCount++;

            if (sentenceMapping.IsLastWordMatched())
                SentenceMatchLastWordCount++;
            else
                SentenceNotMatchLastWordCount++;

            SentenceMainScoreSum += mainScore;
            SentenceExtraScoreSum += extraScore;
            SentenceCombinedScoreSum += combinedScore;
            SentenceEditDistanceRawSum += sentenceMapping.EditDistanceRaw;
            SentenceEditDistanceProcessedSum += sentenceMapping.EditDistanceProcessed;
            SentenceEditDistanceMatchedSum += sentenceMapping.EditDistanceMatched;
            FirstWordScoreSum += sentenceMapping.FirstWordScore();
            LastWordScoreSum += sentenceMapping.LastWordScore();
            FirstAndLastWordScoreSum += sentenceMapping.FirstWordScore() + sentenceMapping.LastWordScore();

            if (Parent != null)
                Parent.UpdateSums(sentenceMapping);
        }

        public void IncrementRequiredResyncCount()
        {
            RequiredResyncCount++;

            if (Parent != null)
                Parent.IncrementRequiredResyncCount();
        }

        public void IncrementIgnoredSentenceCount()
        {
            IgnoredSentenceCount++;

            if (Parent != null)
                Parent.IncrementIgnoredSentenceCount();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            FormatDump(sb);
            return sb.ToString();
        }

        public string DumpString(string prefix = "")
        {
            StringBuilder sb = new StringBuilder();
            FormatDump(sb, prefix);
            return sb.ToString();
        }

        public void PutDumpString(string prefix = "")
        {
            if (DumpSentenceMappingSB != null)
                FormatDump(DumpSentenceMappingSB, prefix);
            if (DumpWordMappingSB != null)
                FormatDump(DumpWordMappingSB, prefix);
        }

        public void FormatDump(StringBuilder sb, string prefix = "")
        {
            sb.AppendLine(prefix + "CommandLine = " + CommandLine);
            FormatAudioMapperSettings(sb, prefix);

            sb.AppendLine(prefix + "MappedSentenceCount = " + MappedSentenceCount.ToString());
            sb.AppendLine(prefix + "CombinedSentenceScorePoint25 = " + CombinedSentenceScorePoint25.ToString());
            sb.AppendLine(prefix + "CombinedSentenceScorePoint75 = " + CombinedSentenceScorePoint75.ToString());
            sb.AppendLine(prefix + "SentenceMatchFirstWordCount = " + SentenceMatchFirstWordCount.ToString());
            sb.AppendLine(prefix + "SentenceNotMatchFirstWordCount = " + SentenceNotMatchFirstWordCount.ToString());
            sb.AppendLine(prefix + "SentenceMatchLastWordCount = " + SentenceMatchLastWordCount.ToString());
            sb.AppendLine(prefix + "SentenceNotMatchLastWordCount = " + SentenceNotMatchLastWordCount.ToString());
            sb.AppendLine(prefix + "RequiredResyncCount = " + RequiredResyncCount.ToString());
            sb.AppendLine(prefix + "IgnoredSentenceCount = " + IgnoredSentenceCount.ToString());
            sb.AppendLine(prefix + "SentenceMainScoreSum = " + SentenceMainScoreSum.ToString());
            sb.AppendLine(prefix + "SentenceExtraScoreSum = " + SentenceExtraScoreSum.ToString());
            sb.AppendLine(prefix + "SentenceCombinedScoreSum = " + SentenceCombinedScoreSum.ToString());
            sb.AppendLine(prefix + "SentenceEditDistanceRawSum = " + SentenceEditDistanceRawSum.ToString());
            sb.AppendLine(prefix + "SentenceEditDistanceProcessedSum = " + SentenceEditDistanceProcessedSum.ToString());
            sb.AppendLine(prefix + "SentenceEditDistanceMatchedSum = " + SentenceEditDistanceMatchedSum.ToString());
            sb.AppendLine(prefix + "FirstWordScoreSum = " + FirstWordScoreSum.ToString());
            sb.AppendLine(prefix + "LastWordScoreSum = " + LastWordScoreSum.ToString());
            sb.AppendLine(prefix + "FirstAndLastWordScoreSum = " + FirstAndLastWordScoreSum.ToString());

            TimeSpan speechToTextTimeSpan = TimeSpan.FromSeconds(SpeechToTextTime);
            TimeSpan mappingTimeSpan = TimeSpan.FromSeconds(MappingTime);
            TimeSpan combinedTimeSpan = speechToTextTimeSpan + mappingTimeSpan;

            sb.AppendLine(prefix + "SpeechToText time = " + speechToTextTimeSpan.ToString());
            sb.AppendLine(prefix + "Mapping time = " + mappingTimeSpan.ToString());
            sb.AppendLine(prefix + "Combined time = " + combinedTimeSpan.ToString());
            sb.AppendLine(prefix + "When = " + DateTime.Now.ToString());
        }

        // Format AudioMapper settings string.
        public void FormatAudioMapperSettings(StringBuilder sb, string prefix = "")
        {
            sb.Append(prefix + "Settings: ");
            sb.Append("TestName=" + TestName);
            sb.Append(", MatchScoreMode=" + AudioMapper.MatchScoreMode.ToString());
            sb.Append(", ExtraSentenceWordCountMode=" + AudioMapper.ExtraSentenceWordCountMode.ToString());
            sb.Append(", ExtraSentenceCharacterCountMode=" + AudioMapper.ExtraSentenceCharacterCountMode.ToString());
            sb.Append(", SentenceMatchMode=" + AudioMapper.SentenceMatchMode.ToString());
            sb.Append(", WalkSentenceStart=" + AudioMapper.WalkSentenceStart.ToString());
            sb.Append(", WeightingMode=" + AudioMapper.WeightingMode.ToString());
            sb.Append(", ExtraSentenceLength=" + AudioMapper.ExtraSentenceLength.ToString());
            sb.Append(", ExtraSentenceLengthMinimum=" + AudioMapper.ExtraSentenceLengthMinimum.ToString());
            sb.Append(", ExtraSentenceLengthMaximum=" + AudioMapper.ExtraSentenceLengthMaximum.ToString());
            sb.Append(", ExtraSentenceDivisor=" + AudioMapper.ExtraSentenceDivisor.ToString());
            sb.Append(", MainSentenceWeightingFactor=" + AudioMapper.MainSentenceWeightingFactor.ToString());
            sb.Append(", ExtraSentenceWeightingFactor=" + AudioMapper.ExtraSentenceWeightingFactor.ToString());
            sb.Append(", TranscribedUnusedSumPenaltyDivisor=" + AudioMapper.TranscribedUnusedSumPenaltyDivisor.ToString());
            sb.Append(", ResyncMainScoreThreshold=" + AudioMapper.ResyncMainScoreThreshold.ToString());
            sb.Append(", IgnoreMainScoreThreshold=" + AudioMapper.IgnoreMainScoreThreshold.ToString());
            sb.Append(", MaxWordGroupSize=" + AudioMapper.MaxWordGroupSize.ToString());
            sb.Append(", EditDistanceFactorExponent=" + AudioMapper.EditDistanceFactorExponent.ToString());
            sb.Append(", PositionFactorExponent=" + AudioMapper.PositionFactorExponent.ToString());
            sb.Append(", AmplitudeThreshold=" + AudioMapper.AmplitudeThreshold.ToString());
            sb.Append(", AmplitudeIncrement=" + AudioMapper.AmplitudeIncrement.ToString());
            sb.Append(", AmplitudeSilenceMax=" + AudioMapper.AmplitudeSilenceMax.ToString());
            sb.Append(", SilenceWidthThreshold=" + AudioMapper.SilenceWidthThreshold.ToString());
            sb.Append(", SearchLimitDefault=" + AudioMapper.SearchLimitDefault.ToString());
            sb.Append(", SearchTimeIncrement=" + AudioMapper.SearchTimeIncrement.ToString());
            sb.Append(", DefaultLeadTime=" + AudioMapper.DefaultLeadTime.ToString());
            sb.AppendLine(", MinimumLeadTime=" + AudioMapper.MinimumLeadTime.ToString());
        }

        // Format AudioMapper settings string.
        public void FormatTestMapHeader(StringBuilder sb)
        {
            sb.Append("Test Name");
            sb.Append("\tMatch Score Mode");
            sb.Append("\tExtra Sentence Word Count Mode");
            sb.Append("\tExtra Sentence Character Count Mode");
            sb.Append("\tSentence Match Mode");
            sb.Append("\tWalk Sentence Start");
            sb.Append("\tWeighting Mode");
            sb.Append("\tExtra Sentence Length");
            sb.Append("\tExtra Sentence Length Minimum");
            sb.Append("\tExtra Sentence Length Maximum");
            sb.Append("\tExtra Sentence Divisor");
            sb.Append("\tMain Sentence Weighting Factor");
            sb.Append("\tExtra Sentence Weighting Factor");
            sb.Append("\tTranscribed Unused Sum Penalty Divisor");
            sb.Append("\tResync Main Score Threshold");
            sb.Append("\tIgnore Main Score Threshold");
            sb.Append("\tMax Word Group Size");
            sb.Append("\tEdit Distance Factor Exponent");
            sb.Append("\tPosition Factor Exponent");
            sb.Append("\tAmplitude Threshold");
            sb.Append("\tAmplitude Increment");
            sb.Append("\tAmplitude Silence Max");
            sb.Append("\tSilence Width Threshold");
            sb.Append("\tSearch Limit Default");
            sb.Append("\tSearch Time Increment");
            sb.Append("\tDefault Lead Time");
            sb.Append("\tMinimum Lead Time");
            sb.Append("\tMapped Sentence Count");
            sb.Append("\tCombined Sentence Score Point25");
            sb.Append("\tCombined Sentence Score Point75");
            sb.Append("\tSentence Match First Word Count");
            sb.Append("\tSentence Not Match First Word Count");
            sb.Append("\tSentence Match Last Word Count");
            sb.Append("\tSentence Not Match Last Word Count");
            sb.Append("\tRequired Resync Count");
            sb.Append("\tIgnored Sentence Count");
            sb.Append("\tSentence Main Score Sum");
            sb.Append("\tSentence Extra Score Sum");
            sb.Append("\tSentence Combined Score Sum");
            sb.Append("\tSentence Edit Distance Raw Sum");
            sb.Append("\tSentence Edit Distance Processed Sum");
            sb.Append("\tSentence Edit Distance Matched Sum");
            sb.Append("\tFirst Word Score Sum");
            sb.Append("\tLast Word Score Sum");
            sb.Append("\tFirst And Last Word Score Sum");
            sb.Append("\tSpeech To Text Time");
            sb.Append("\tMapping Time");
            sb.Append("\tCombined Time");
            sb.Append("\tWhen");
        }

        // Format AudioMapper settings string.
        public void FormatTestMapRow(StringBuilder sb)
        {
            sb.Append(TestName);
            sb.Append("\t" + AudioMapper.MatchScoreMode.ToString());
            sb.Append("\t" + AudioMapper.ExtraSentenceWordCountMode.ToString());
            sb.Append("\t" + AudioMapper.ExtraSentenceCharacterCountMode.ToString());
            sb.Append("\t" + AudioMapper.SentenceMatchMode.ToString());
            sb.Append("\t" + AudioMapper.WalkSentenceStart.ToString());
            sb.Append("\t" + AudioMapper.WeightingMode.ToString());
            sb.Append("\t" + AudioMapper.ExtraSentenceLength.ToString());
            sb.Append("\t" + AudioMapper.ExtraSentenceLengthMinimum.ToString());
            sb.Append("\t" + AudioMapper.ExtraSentenceLengthMaximum.ToString());
            sb.Append("\t" + AudioMapper.ExtraSentenceDivisor.ToString());
            sb.Append("\t" + AudioMapper.MainSentenceWeightingFactor.ToString());
            sb.Append("\t" + AudioMapper.ExtraSentenceWeightingFactor.ToString());
            sb.Append("\t" + AudioMapper.TranscribedUnusedSumPenaltyDivisor.ToString());
            sb.Append("\t" + AudioMapper.ResyncMainScoreThreshold.ToString());
            sb.Append("\t" + AudioMapper.IgnoreMainScoreThreshold.ToString());
            sb.Append("\t" + AudioMapper.MaxWordGroupSize.ToString());
            sb.Append("\t" + AudioMapper.EditDistanceFactorExponent.ToString());
            sb.Append("\t" + AudioMapper.PositionFactorExponent.ToString());
            sb.Append("\t" + AudioMapper.AmplitudeThreshold.ToString());
            sb.Append("\t" + AudioMapper.AmplitudeIncrement.ToString());
            sb.Append("\t" + AudioMapper.AmplitudeSilenceMax.ToString());
            sb.Append("\t" + AudioMapper.SilenceWidthThreshold.ToString(TimeFormat));
            sb.Append("\t" + AudioMapper.SearchLimitDefault.ToString(TimeFormat));
            sb.Append("\t" + AudioMapper.SearchTimeIncrement.ToString(TimeFormat));
            sb.Append("\t" + AudioMapper.DefaultLeadTime.ToString(TimeFormat));
            sb.Append("\t" + AudioMapper.MinimumLeadTime.ToString(TimeFormat));
            sb.Append("\t" + MappedSentenceCount.ToString());
            sb.Append("\t" + CombinedSentenceScorePoint25.ToString());
            sb.Append("\t" + CombinedSentenceScorePoint75.ToString());
            sb.Append("\t" + SentenceMatchFirstWordCount.ToString());
            sb.Append("\t" + SentenceNotMatchFirstWordCount.ToString());
            sb.Append("\t" + SentenceMatchLastWordCount.ToString());
            sb.Append("\t" + SentenceNotMatchLastWordCount.ToString());
            sb.Append("\t" + RequiredResyncCount.ToString());
            sb.Append("\t" + IgnoredSentenceCount.ToString());
            sb.Append("\t" + SentenceMainScoreSum.ToString());
            sb.Append("\t" + SentenceExtraScoreSum.ToString());
            sb.Append("\t" + SentenceCombinedScoreSum.ToString());
            sb.Append("\t" + SentenceEditDistanceRawSum.ToString());
            sb.Append("\t" + SentenceEditDistanceProcessedSum.ToString());
            sb.Append("\t" + SentenceEditDistanceMatchedSum.ToString());
            sb.Append("\t" + FirstWordScoreSum.ToString());
            sb.Append("\t" + LastWordScoreSum.ToString());
            sb.Append("\t" + FirstAndLastWordScoreSum.ToString());

            TimeSpan speechToTextTimeSpan = TimeSpan.FromSeconds(SpeechToTextTime);
            TimeSpan mappingTimeSpan = TimeSpan.FromSeconds(MappingTime);
            TimeSpan combinedTimeSpan = speechToTextTimeSpan + mappingTimeSpan;

            sb.Append("\t" + speechToTextTimeSpan.ToString(TimeFormat));
            sb.Append("\t" + mappingTimeSpan.ToString(TimeFormat));
            sb.Append("\t" + combinedTimeSpan.ToString(TimeFormat));
            sb.Append("\t" + DateTime.Now.ToString());
        }

        // Create and merge row for test map.
        public bool MergeTestMap(string filePath)
        {
            if (String.IsNullOrEmpty(TestName))
                return false;

            List<string> mapData = null;
            StringBuilder sb = new StringBuilder();

            FormatTestMapHeader(sb);

            string header = sb.ToString();

            sb.Clear();

            FormatTestMapRow(sb);

            string thisRow = sb.ToString();

            if (FileSingleton.Exists(filePath))
            {
                mapData = FileSingleton.ReadAllLines(filePath, ApplicationData.Encoding).ToList();

                if (mapData.Count() != 0)
                {
                    if (mapData[0] != header)
                        mapData.Clear();
                }
            }

            if ((mapData == null) || (mapData.Count() == 0))
            {
                mapData = new List<string>();
                mapData.Add(header);
                mapData.Add(thisRow);
            }
            else
            {
                int count = mapData.Count();
                int index;
                string key = TestName + "\t";
                bool found = false;

                for (index = 1; index < count; index++)
                {
                    string row = mapData[index];

                    if (row.StartsWith(key))
                    {
                        mapData[index] = thisRow;
                        found = true;
                        break;
                    }
                }

                if (!found)
                    mapData.Add(thisRow);
            }

            FileSingleton.WriteAllLines(
                filePath,
                mapData.ToArray(),
                ApplicationData.Encoding);

            return true;
        }

        // Start speech to text timer.
        public void StartSpeechToTextTimer()
        {
            if (SpeechToTextTimer == null)
                SpeechToTextTimer = new SoftwareTimer();

            SpeechToTextTimer.Start();

            if (Parent != null)
                Parent.StartSpeechToTextTimer();
        }

        // Stop speech to text timer.
        public void StopSpeechToTextTimer()
        {
            if (SpeechToTextTimer != null)
            {
                SpeechToTextTimer.Stop();
                SpeechToTextTime += SpeechToTextTimer.GetTimeInSeconds();
            }

            if (Parent != null)
                Parent.StopSpeechToTextTimer();
        }

        // Start speech to text timer.
        public void StartMappingTimer()
        {
            if (MappingTimer == null)
                MappingTimer = new SoftwareTimer();

            MappingTimer.Start();

            if (Parent != null)
                Parent.StartMappingTimer();
        }

        // Stop speech to text timer.
        public void StopMappingTimer()
        {
            if (MappingTimer != null)
            {
                MappingTimer.Stop();
                MappingTime += MappingTimer.GetTimeInSeconds();
            }

            if (Parent != null)
                Parent.StopMappingTimer();
        }

        // Set up reporting.
        public void SetUpReporting(
            bool doSentenceReporting,
            bool doWordReporting,
            bool doWordTracing,
            bool doNumberTracing,
            bool doAbbreviationTracing,
            bool doTestMapReporting)
        {
            DumpSentenceMappingResults = doSentenceReporting;

            if (doSentenceReporting)
                DumpSentenceMappingSB = new StringBuilder();

            DumpWordMappingResults = doSentenceReporting;

            if (doWordReporting)
                DumpWordMappingSB = new StringBuilder();

            DumpWordTracingResults = doWordTracing;

            if (doWordTracing)
            {
                DumpWordTracingSB = new StringBuilder();
                DumpWordMappingResultsHeading(DumpWordTracingSB);
            }

            DumpNumberTracingResults = doNumberTracing;

            if (doNumberTracing)
            {
                DumpNumberTracingSB = new StringBuilder();
                DumpNumberTracingSB.AppendLine("Number normalization report:");
            }

            DumpAbbreviationTracingResults = doAbbreviationTracing;

            if (doAbbreviationTracing)
            {
                DumpAbbreviationTracingSB = new StringBuilder();
                DumpAbbreviationTracingSB.AppendLine("Abbreviation normalization report:");
            }

            DumpTestMapResults = doTestMapReporting;

            if (doTestMapReporting)
            {
                DumpTestMapSB = new StringBuilder();
                DumpTestMapSB.AppendLine("Test map report:");
            }
        }

        // Set up reporting.
        public void WriteReports(
            string sentenceReportingFilePath,
            string wordReportingFilePath,
            string wordTracingFilePath,
            string numberTracingFilePath,
            string abbreviationTracingFilePath,
            string testMapFilePath)
        {
            if ((DumpSentenceMappingSB != null) && !String.IsNullOrEmpty(sentenceReportingFilePath))
                FileSingleton.WriteAllText(
                    sentenceReportingFilePath,
                    DumpSentenceMappingSB.ToString(),
                    ApplicationData.Encoding);

            if ((DumpWordMappingSB != null) && !String.IsNullOrEmpty(wordReportingFilePath))
                FileSingleton.WriteAllText(
                    wordReportingFilePath,
                    DumpWordMappingSB.ToString(),
                    ApplicationData.Encoding);

            if ((DumpWordTracingSB != null) && !String.IsNullOrEmpty(wordTracingFilePath))
                FileSingleton.WriteAllText(
                    wordTracingFilePath,
                    DumpWordTracingSB.ToString(),
                    ApplicationData.Encoding);

            if ((DumpNumberTracingSB != null) && !String.IsNullOrEmpty(numberTracingFilePath))
                FileSingleton.WriteAllText(
                    numberTracingFilePath,
                    DumpNumberTracingSB.ToString(),
                    ApplicationData.Encoding);

            if ((DumpAbbreviationTracingSB != null) && !String.IsNullOrEmpty(abbreviationTracingFilePath))
                FileSingleton.WriteAllText(
                    abbreviationTracingFilePath,
                    DumpAbbreviationTracingSB.ToString(),
                    ApplicationData.Encoding);

            if ((DumpTestMapSB != null) && !String.IsNullOrEmpty(testMapFilePath))
                MergeTestMap(testMapFilePath);
        }

        // Dump mapping results to internal string builder.
        public void DumpMappingResults(List<SentenceMapping> sentenceMappings)
        {
            if (DumpSentenceMappingSB != null)
                DumpSentenceMappingOutput(sentenceMappings, DumpSentenceMappingSB);

            if (DumpWordMappingSB != null)
                DumpWordMappingOutput(sentenceMappings, DumpWordMappingSB);

            if (Parent != null)
                Parent.DumpMappingResults(sentenceMappings);
        }

        // Dump sentence mapping results to given string builder.
        public void DumpSentenceMappingOutput(
            List<SentenceMapping> sentenceMappings,
            StringBuilder sb)
        {
            if (!DumpSentenceMappingResults)
                return;

            if ((sb != null) && (sb.Length == 0))
                DumpSentenceMappingResultsHeading(sb);

            int sentenceIndex;
            int sentenceCount = sentenceMappings.Count();

            for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
            {
                SentenceMapping sentenceMapping = sentenceMappings[sentenceIndex];

                DumpSentenceMappingResultsSentence(
                    sentenceMapping,
                    sentenceIndex,
                    sb,
                    false);
            }

            FormatDump(sb);
        }

        public void DumpSentenceMappingResultsHeading(StringBuilder sb)
        {
            DumpSentenceMappingResultsSentence(
                null,
                0,
                sb,
                true);
        }

        // Dump setnence mapping results sentence.
        public void DumpSentenceMappingResultsSentence(
            SentenceMapping sentenceMapping,
            int sentenceIndex,
            StringBuilder sb,
            bool isHeading)
        {
            string Path = String.Empty;
            string Verse = String.Empty;
            int SentenceNumber = 0;
            string OriginalSentenceText = String.Empty;
            string TranscribedSentenceText = String.Empty;
            string TranscribedMatchedWords = String.Empty;
            string OriginalSentenceTextPostProcessing = String.Empty;
            string TranscribedSentenceTextPostProcessing = String.Empty;
            int OriginalStartWordIndex = 0;
            int TranscribedStartWordIndex = 0;
            int OriginalWordCount = 0;
            int TranscribedWordCount = 0;
            int OriginalSentenceLengthNoSeparators = 0;
            int TranscribedSentenceLengthNoSeparators = 0;
            int MatchedWordCount = 0;
            int OriginalUnmatchedCount = 0;
            int TranscribedUnmatchedCount = 0;
            string OriginalUnmatchedWords = String.Empty;
            string TranscribedUnmatchedWords = String.Empty;
            string RequiredResync = String.Empty;
            string Ignored = String.Empty;
            string OriginalExtraText = String.Empty;
            string TranscribedExtraText = String.Empty;
            double MainScore = 0.0;
            double ExtraScore = 0.0;
            double CombinedScore = 0.0;
            double MainScorePriorToResync = 0.0;
            string TranscribedSentencePriorToResync = String.Empty;
            TimeSpan SentenceStart = TimeSpan.Zero;
            TimeSpan SentenceStop = TimeSpan.Zero;
            TimeSpan SentenceLength = TimeSpan.Zero;
            TimeSpan SilenceStartFront = TimeSpan.Zero;
            TimeSpan SilenceStopFront = TimeSpan.Zero;
            TimeSpan SilenceLengthFront = TimeSpan.Zero;
            TimeSpan SilenceStartBack = TimeSpan.Zero;
            TimeSpan SilenceStopBack = TimeSpan.Zero;
            TimeSpan SilenceLengthBack = TimeSpan.Zero;
            TimeSpan PreviousWordStartTime = TimeSpan.Zero;
            TimeSpan PreviousWordStopTime = TimeSpan.Zero;
            TimeSpan FirstWordStartTime = TimeSpan.Zero;
            TimeSpan FirstWordStopTime = TimeSpan.Zero;
            TimeSpan LastWordStartTime = TimeSpan.Zero;
            TimeSpan LastWordStopTime = TimeSpan.Zero;
            TimeSpan NextWordStartTime = TimeSpan.Zero;
            TimeSpan NextWordStopTime = TimeSpan.Zero;
            double MatchedTotalPercentage = 0.0;
            double FirstWordScore = 0.0;
            double LastWordScore = 0.0;
            int SentenceEditDistanceRaw = 0;
            int SentenceEditDistanceProcessed = 0;
            int SentenceEditDistanceMatched = 0;
            string errorMessage = String.Empty;

            if (sentenceMapping != null)
            {
                MappingSentenceRun originalSentence;
                MappingSentenceRun transcribedSentence;
                MappingSentenceRun originalExtraSentence;
                MappingSentenceRun transcribedExtraSentence;
                MultiLanguageItem studyItem;

                originalSentence = sentenceMapping.OriginalSentence;
                transcribedSentence = sentenceMapping.TranscribedSentence;
                originalExtraSentence = sentenceMapping.OriginalExtraSentence;
                transcribedExtraSentence = sentenceMapping.TranscribedExtraSentence;
                studyItem = sentenceMapping.StudyItem;
                Path = studyItem.Content.GetNamePathString(LanguageLookup.English, "/");
                Verse = studyItem.GetNameStringWithOrdinal();
                SentenceNumber = sentenceMapping.SentenceIndex + 1;
                OriginalSentenceText = originalSentence.RawSentenceText;
                TranscribedSentenceText = String.Empty;
                TranscribedMatchedWords = originalSentence.MatchedSentenceFromWords;
                OriginalSentenceTextPostProcessing = originalSentence.SentenceText;
                TranscribedSentenceTextPostProcessing = String.Empty;
                OriginalStartWordIndex = originalSentence.ParentStartWordIndex;
                TranscribedStartWordIndex = 0;
                OriginalWordCount = originalSentence.WordCount;
                TranscribedWordCount = 0;
                OriginalSentenceLengthNoSeparators = originalSentence.TextLengthNoSeparators;
                TranscribedSentenceLengthNoSeparators = 0;
                MatchedWordCount = originalSentence.MatchedCount;
                OriginalUnmatchedCount = originalSentence.UnmatchedCount;
                TranscribedUnmatchedCount = 0;
                OriginalUnmatchedWords =
                    (originalSentence.UnmatchedWords != null ?
                        TextUtilities.GetStringFromStringListDelimited(originalSentence.UnmatchedWords, ", ") :
                        String.Empty);
                TranscribedUnmatchedWords = String.Empty;
                RequiredResync = (sentenceMapping.RequiredResync ? "1" : "0");
                Ignored = (sentenceMapping.Ignored ? "1" : "0");
                OriginalExtraText = (originalExtraSentence != null ? originalExtraSentence.SentenceText : "");
                TranscribedExtraText = String.Empty;
                MainScore = sentenceMapping.MainScore;
                ExtraScore = sentenceMapping.ExtraScore;
                CombinedScore = sentenceMapping.CombinedScore;
                MainScorePriorToResync = sentenceMapping.MainScorePriorToResync;
                TranscribedSentencePriorToResync = sentenceMapping.BestCandidateSentencePriorToResync;
                SentenceStart = sentenceMapping.StartTime;
                SentenceStop = sentenceMapping.StopTime;
                SentenceLength = sentenceMapping.Duration;
                SilenceStartFront = sentenceMapping.SilenceStartFront;
                SilenceStopFront = sentenceMapping.SilenceStopFront;
                SilenceLengthFront = sentenceMapping.SilenceLengthFront;
                SilenceStartBack = sentenceMapping.SilenceStartBack;
                SilenceStopBack = sentenceMapping.SilenceStopBack;
                SilenceLengthBack = sentenceMapping.SilenceLengthBack;
                PreviousWordStartTime = sentenceMapping.PreviousTranscribedWordStartTime;
                PreviousWordStopTime = sentenceMapping.PreviousTranscribedWordStopTime;
                FirstWordStartTime = sentenceMapping.FirstTranscribedWordStartTime;
                FirstWordStopTime = sentenceMapping.FirstTranscribedWordStopTime;
                LastWordStartTime = sentenceMapping.LastTranscribedWordStartTime;
                LastWordStopTime = sentenceMapping.LastTranscribedWordStopTime;
                NextWordStartTime = sentenceMapping.NextTranscribedWordStartTime;
                NextWordStopTime = sentenceMapping.NextTranscribedWordStopTime;
                MatchedTotalPercentage = 0;

                if (transcribedSentence != null)
                {
                    TranscribedSentenceText = transcribedSentence.RawSentenceText;
                    TranscribedSentenceTextPostProcessing = transcribedSentence.SentenceText;
                    TranscribedStartWordIndex = transcribedSentence.ParentStartWordIndex;
                    TranscribedWordCount = transcribedSentence.WordCount;
                    TranscribedSentenceLengthNoSeparators = transcribedSentence.TextLengthNoSeparators;
                    TranscribedUnmatchedCount = transcribedSentence.UnmatchedCount;
                    TranscribedUnmatchedWords =
                        (transcribedSentence.UnmatchedWords != null ?
                            TextUtilities.GetStringFromStringListDelimited(transcribedSentence.UnmatchedWords, ", ") :
                            String.Empty);

                    SentenceEditDistanceRaw = TextUtilities.ComputeDistance(
                        originalSentence.RawSentenceText,
                        transcribedSentence.RawSentenceText);
                    SentenceEditDistanceProcessed = TextUtilities.ComputeDistance(
                        originalSentence.SentenceText,
                        transcribedSentence.SentenceText);
                    SentenceEditDistanceMatched = TextUtilities.ComputeDistance(
                        originalSentence.SentenceFromWords,
                        transcribedSentence.MatchedSentenceFromWords);
                }

                if (transcribedExtraSentence != null)
                    TranscribedExtraText = transcribedExtraSentence.SentenceText;

                if (OriginalWordCount != 0)
                    MatchedTotalPercentage = (double)MatchedWordCount / OriginalWordCount;

                FirstWordScore = sentenceMapping.FirstWordScore();
                LastWordScore = sentenceMapping.LastWordScore();
                SentenceEditDistanceRaw = sentenceMapping.EditDistanceRaw;
                SentenceEditDistanceProcessed = sentenceMapping.EditDistanceProcessed;
                SentenceEditDistanceMatched = sentenceMapping.EditDistanceMatched;
                errorMessage = sentenceMapping.Error;
            }

            if (isHeading)
            {
                if (!String.IsNullOrEmpty(TestName))
                    sb.AppendLine(TestName);
                else
                    FormatAudioMapperSettings(sb);
            }

            sb.Append(isHeading ? "Path" : Path);
            sb.Append("\t");
            sb.Append(isHeading ? "Verse" : Verse);
            sb.Append("\t");
            sb.Append(isHeading ? "Sentence #" : SentenceNumber.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original sentence text" : OriginalSentenceText);
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed sentence text" : TranscribedSentenceText);
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed matched words" : TranscribedMatchedWords);
            sb.Append("\t");
            sb.Append(isHeading ? "Original Start word index" : OriginalStartWordIndex.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed start word index" : TranscribedStartWordIndex.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original word count" : OriginalWordCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed word count" : TranscribedWordCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original final word count" : "");
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed final word count" : "");
            sb.Append("\t");
            sb.Append(isHeading ? "Original sentence length no separators" : OriginalSentenceLengthNoSeparators.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed sentence length no separators" : TranscribedSentenceLengthNoSeparators.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Matched word count" : MatchedWordCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original unmatched count" : OriginalUnmatchedCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed unmatched count" : TranscribedUnmatchedCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original unmatched words" : OriginalUnmatchedWords);
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed unmatched words" : TranscribedUnmatchedWords);
            sb.Append("\t");
            sb.Append(isHeading ? "Required resync" : RequiredResync);
            sb.Append("\t");
            sb.Append(isHeading ? "Ignored" : Ignored);
            sb.Append("\t");
            sb.Append(isHeading ? "Original extra text" : OriginalExtraText);
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed extra text" : TranscribedExtraText);
            sb.Append("\t");
            sb.Append(isHeading ? "Main score" : MainScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Extra score" : ExtraScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Combined score" : CombinedScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Main score prior to resync" : MainScorePriorToResync.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed sentence prior to resync" : TranscribedSentencePriorToResync);
            sb.Append("\t");
            sb.Append(isHeading ? "Matched/total percentage" : MatchedTotalPercentage.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Sentence start time" : SentenceStart.ToString(TimeFormat));
            sb.Append("\t");
            sb.Append(isHeading ? "Sentence stop time" : SentenceStop.ToString(TimeFormat));
            sb.Append("\t");
            sb.Append(isHeading ? "Sentence time length" : SentenceLength.ToString(TimeFormat));
            sb.Append("\t");
            sb.Append(isHeading ? "Silence start time front" : SilenceStartFront.ToString(TimeFormat));
            sb.Append("\t");
            sb.Append(isHeading ? "Silence stop time front" : SilenceStopFront.ToString(TimeFormat));
            sb.Append("\t");
            sb.Append(isHeading ? "Silence time length front" : SilenceLengthFront.ToString(TimeFormat));
            sb.Append("\t");
            sb.Append(isHeading ? "Silence start time back" : SilenceStartBack.ToString(TimeFormat));
            sb.Append("\t");
            sb.Append(isHeading ? "Silence stop time back" : SilenceStopBack.ToString(TimeFormat));
            sb.Append("\t");
            sb.Append(isHeading ? "Silence time length back" : SilenceLengthBack.ToString(TimeFormat));
            sb.Append("\t");
            sb.Append(isHeading ? "Previous word start time" : PreviousWordStartTime.ToString(TimeFormat));
            sb.Append("\t");
            sb.Append(isHeading ? "Previous word stop time" : PreviousWordStopTime.ToString(TimeFormat));
            sb.Append("\t");
            sb.Append(isHeading ? "First word start time" : FirstWordStartTime.ToString(TimeFormat));
            sb.Append("\t");
            sb.Append(isHeading ? "First word stop time" : FirstWordStopTime.ToString(TimeFormat));
            sb.Append("\t");
            sb.Append(isHeading ? "Last word start time" : LastWordStartTime.ToString(TimeFormat));
            sb.Append("\t");
            sb.Append(isHeading ? "Last word stop time" : LastWordStopTime.ToString(TimeFormat));
            sb.Append("\t");
            sb.Append(isHeading ? "Next word start time" : NextWordStartTime.ToString(TimeFormat));
            sb.Append("\t");
            sb.Append(isHeading ? "Next word stop time" : NextWordStopTime.ToString(TimeFormat));
            sb.Append("\t");
            sb.Append(isHeading ? "First word Score" : FirstWordScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Last word Score" : LastWordScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Edit Distance Raw" : SentenceEditDistanceRaw.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Edit Distance Processed" : SentenceEditDistanceProcessed.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Edit Distance Matched" : SentenceEditDistanceMatched.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original sentence text post processing" : OriginalSentenceTextPostProcessing);
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed sentence text post processing" : TranscribedSentenceTextPostProcessing);
            sb.Append("\t");
            sb.Append(isHeading ? "errorMessage" : errorMessage);
            sb.AppendLine("");
        }

        // Dump word mapping results to given string builder.
        public void DumpWordMappingOutput(
            List<SentenceMapping> sentenceMappings,
            StringBuilder sb)
        {
            if (!DumpWordMappingResults)
                return;

            if ((sb != null) && (sb.Length == 0))
                DumpWordMappingResultsHeading(sb);

            int sentenceIndex;
            int sentenceCount = sentenceMappings.Count();

            for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
            {
                SentenceMapping sentenceMapping = sentenceMappings[sentenceIndex];
                MappingSentenceRun originalSentenceRun = sentenceMapping.OriginalSentence;
                MappingSentenceRun transcribedSentenceRun = sentenceMapping.TranscribedSentence;
                MappingSentenceRun originalExtraSentenceRun = sentenceMapping.OriginalExtraSentence;
                MappingSentenceRun transcribedExtraSentenceRun = sentenceMapping.TranscribedExtraSentence;

                sb.AppendLine("Main original sentence:    " + originalSentenceRun.SentenceText);

                if (transcribedSentenceRun != null)
                    sb.AppendLine("Main transcribed sentence: " + transcribedSentenceRun.SentenceText);
                else
                    sb.AppendLine("Main transcribed sentence: (null)");

                DumpWordMappingResultsSentence(
                    sentenceMapping,
                    originalSentenceRun,
                    transcribedSentenceRun,
                    sb);

                if (originalExtraSentenceRun != null)
                    sb.AppendLine("Extra original sentence:    " + originalExtraSentenceRun.SentenceText);
                else
                    sb.AppendLine("Extra original sentence:    (null)");

                if (transcribedExtraSentenceRun != null)
                    sb.AppendLine("Extra transcribed sentence: " + transcribedExtraSentenceRun.SentenceText);
                else
                    sb.AppendLine("Extra transcribed sentence: (null)");

                if (originalExtraSentenceRun != null)
                    DumpWordMappingResultsSentence(
                        sentenceMapping,
                        originalExtraSentenceRun,
                        transcribedExtraSentenceRun,
                        sb);
            }

            FormatDump(sb);
        }

        // Dump candidate state.
        public void DumpDebugWordMappingCandidateState(
            string mainLabel,
            string extraMessage,
            int offset,
            SentenceMapping sentenceMapping)
        {
            MappingSentenceRun originalSentenceRun = sentenceMapping.OriginalSentence;
            MappingSentenceRun transcribedSentenceRunCandidate = sentenceMapping.TranscribedSentence;
            MappingSentenceRun originalExtraSentenceRun = sentenceMapping.OriginalExtraSentence;
            MappingSentenceRun transcribedExtraSentenceRunCandidate = sentenceMapping.TranscribedExtraSentence;

            DumpDebugWordMappingResultsLabel(mainLabel);
            DumpDebugWordMappingResultsLabel("OrigRaw(" +
                originalSentenceRun.WordCount.ToString() +
                "," +
                originalSentenceRun.TextLengthNoSeparators.ToString() +
                ")=" +
                originalSentenceRun.SentenceText);

            if (transcribedSentenceRunCandidate != null)
                DumpDebugWordMappingResultsLabel("CandRaw(" +
                    transcribedSentenceRunCandidate.WordCount.ToString() +
                    "," +
                    transcribedSentenceRunCandidate.TextLengthNoSeparators.ToString() +
                    ")=" +
                    transcribedSentenceRunCandidate.SentenceText);

            DumpDebugWordMappingResultsLabel("OrigWords=" + originalSentenceRun.SentenceFromWords);
            DumpDebugWordMappingResultsLabel("CandWords=" + originalSentenceRun.MatchedSentenceFromWords);

            DumpDebugWordMappingResultsSentence(
                sentenceMapping,
                originalSentenceRun,
                transcribedSentenceRunCandidate,
                "Offset = " + offset.ToString() +
                    ", CombinedScore = " + sentenceMapping.CombinedScore +
                    ", MainScore = " + sentenceMapping.MainScore +
                    ", ExtraScore = " + sentenceMapping.ExtraScore +
                    ", DistanceRaw = " + sentenceMapping.EditDistanceRaw +
                    ", DistanceProcessed = " + sentenceMapping.EditDistanceProcessed +
                    ", DistanceMatched = " + sentenceMapping.EditDistanceMatched,
                extraMessage);

            if ((originalExtraSentenceRun != null) && (transcribedExtraSentenceRunCandidate != null))
            {
                DumpDebugWordMappingResultsLabel("Next OrigRaw(" +
                    originalExtraSentenceRun.WordCount.ToString() +
                    "," +
                    originalExtraSentenceRun.TextLengthNoSeparators.ToString() +
                    ")=" +
                    originalExtraSentenceRun.SentenceText);
                DumpDebugWordMappingResultsLabel("Next CandRaw(" +
                    transcribedExtraSentenceRunCandidate.WordCount.ToString() +
                    "," +
                    transcribedExtraSentenceRunCandidate.TextLengthNoSeparators.ToString() +
                    ")=" +
                    transcribedExtraSentenceRunCandidate.SentenceText);
                DumpDebugWordMappingResultsLabel("Next OrigWord=" + originalExtraSentenceRun.SentenceFromWords);
                DumpDebugWordMappingResultsLabel("Next CandWord=" + transcribedExtraSentenceRunCandidate.SentenceFromWords);
                DumpDebugWordMappingResultsLabel("Next CandMatched=" + originalExtraSentenceRun.MatchedSentenceFromWords);

                DumpDebugWordMappingResultsSentence(
                    sentenceMapping,
                    originalExtraSentenceRun,
                    transcribedExtraSentenceRunCandidate,
                    "ExtraScore = " + sentenceMapping.ExtraScore,
                    "");
            }
            else
                DumpDebugWordMappingResultsLabel("(No next sentence candidate.)");
        }

        // Dump debug word mapping results for the given sentence.
        public void DumpDebugWordMappingResultsSentence(
            SentenceMapping sentenceMapping,
            MappingSentenceRun originalSentenceRun,
            MappingSentenceRun transcribedSentenceRun,
            string label,
            string extraMessage)
        {
            if (DumpWordTracingSB == null)
                return;

            if (!String.IsNullOrEmpty(label))
                DumpWordTracingSB.AppendLine(label);

            if (!String.IsNullOrEmpty(extraMessage))
                DumpWordTracingSB.AppendLine(extraMessage);

            DumpWordMappingResultsSentence(
                sentenceMapping,
                originalSentenceRun,
                transcribedSentenceRun,
                DumpWordTracingSB);
        }

        // Dump mapping results label.
        public void DumpDebugWordMappingResultsLabel(string label)
        {
            if (DumpWordTracingSB == null)
                return;

            if (!String.IsNullOrEmpty(label))
                DumpWordTracingSB.AppendLine(label);
        }

        public void DumpWordMappingResultsHeading(StringBuilder sb)
        {
            DumpWordMappingResultsWord(
                null,
                null,
                null,
                null,
                0,
                false,
                true,
                sb);
        }

        // Dump mapping results.
        public void DumpWordMappingResultsSentence(
            SentenceMapping sentenceMapping,
            MappingSentenceRun originalSentenceRun,
            MappingSentenceRun transcribedSentenceRun,
            StringBuilder sb)
        {
            int OriginalWordCount = originalSentenceRun.WordCount;

            List<MappingWordRun> wordRuns = originalSentenceRun.WordRuns;
            int wordIndex;

            for (wordIndex = 0; wordIndex < OriginalWordCount; wordIndex++)
            {
                MappingWordRun originalWordRun = wordRuns[wordIndex];

                DumpWordMappingResultsWord(
                    sentenceMapping,
                    originalSentenceRun,
                    transcribedSentenceRun,
                    originalWordRun,
                    wordIndex,
                    false,
                    false,
                    sb);
            }
        }

        // Dump mapping results.
        public void DumpWordMappingResultsWord(
            SentenceMapping sentenceMapping,
            MappingSentenceRun originalSentenceRun,
            MappingSentenceRun transcribedSentenceRun,
            MappingWordRun wordRun,
            int wordIndex,
            bool isTranscribed,
            bool isHeading,
            StringBuilder sb)
        {
            MultiLanguageItem studyItem = null;
            int sentenceIndex = 0;
            int transcribedWordIndex = 0;
            MappingWordRun transcribedWordRun = null;

            string Path = null;
            string Verse = null;
            int SentenceNumber = 0;
            string OriginalSentenceText = null;
            string TranscribedSentenceText = null;
            int OriginalWordCount = 0;
            int TranscribedWordCount = 0;
            int OriginalSentenceLengthNoSeparators = 0;
            int TranscribedSentenceLengthNoSeparators = 0;
            int OriginalMatchedWordCount = 0;
            int OriginalUnmatchedWordCount = 0;
            int TranscribedMatchedWordCount = 0;
            int TranscribedUnmatchedWordCount = 0;

            if (!isHeading)
            {
                studyItem = sentenceMapping.StudyItem;
                sentenceIndex = sentenceMapping.SentenceIndex;
                transcribedWordIndex = wordRun.MatchedWordIndex;
                transcribedWordRun = wordRun.MatchedWordRun;

                Path = studyItem.Content.GetNamePathString(LanguageLookup.English, "/");
                Verse = studyItem.GetNameStringWithOrdinal();
                SentenceNumber = sentenceIndex + 1;
                OriginalSentenceText = originalSentenceRun.SentenceText;
                TranscribedSentenceText = (transcribedSentenceRun != null ? transcribedSentenceRun.SentenceText : "");
                OriginalWordCount = originalSentenceRun.WordCount;
                TranscribedWordCount = (transcribedSentenceRun != null ? transcribedSentenceRun.WordCount : 0);
                OriginalSentenceLengthNoSeparators = originalSentenceRun.TextLengthNoSeparators;
                TranscribedSentenceLengthNoSeparators = (transcribedSentenceRun != null ? transcribedSentenceRun.TextLengthNoSeparators : 0);
                OriginalMatchedWordCount = originalSentenceRun.MatchedCount;
                OriginalUnmatchedWordCount = originalSentenceRun.UnmatchedCount;
                TranscribedMatchedWordCount = (transcribedSentenceRun != null ? transcribedSentenceRun.UnmatchedCount : 0);
                TranscribedUnmatchedWordCount = (transcribedSentenceRun != null ? transcribedSentenceRun.UnmatchedCount : 0);
            }

            if (isHeading)
            {
                if (!String.IsNullOrEmpty(TestName))
                    sb.AppendLine(TestName);
                else
                    FormatAudioMapperSettings(sb);
            }

            sb.Append(isHeading ? "Path" : Path);
            sb.Append("\t");
            sb.Append(isHeading ? "Verse" : Verse);
            sb.Append("\t");
            sb.Append(isHeading ? "Sentence #" : SentenceNumber.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original Word" : wordRun.WordText);
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed Word" : (transcribedWordRun != null ? transcribedWordRun.WordText : String.Empty));
            sb.Append("\t");
            sb.Append(isHeading ? "Word Index" : wordIndex.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Matched Word Index" : wordRun.MatchedWordIndex.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Matched" : (wordRun.Matched ? "1" : "0"));
            sb.Append("\t");
            sb.Append(isHeading ? "Word Match Score" : wordRun.WordMatchScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Word Position Factor" : wordRun.WordPositionFactor.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Edit Distance" : wordRun.EditDistance.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Edit Distance Factor" : wordRun.EditDistanceFactor.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Single Word Edit Distance Score" : wordRun.EditDistanceScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Word Span Count" : wordRun.WordCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original Character Index No Separators" : wordRun.SentenceCharacterPosition.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original Character Length" : wordRun.TextLength.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed Character Index No Separators" : (transcribedWordRun != null ? transcribedWordRun.SentenceCharacterPosition.ToString() : ""));
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed Character Length" : (transcribedWordRun != null ? transcribedWordRun.TextLength.ToString() : ""));
            sb.Append("\t");
            sb.Append(isHeading ? "Start Time" : (transcribedWordRun != null ? transcribedWordRun.StartTime.ToString(TimeFormat) : ""));
            sb.Append("\t");
            sb.Append(isHeading ? "Stop Time" : (transcribedWordRun != null ? transcribedWordRun.StopTime.ToString(TimeFormat) : ""));
            sb.Append("\t");
            sb.Append(isHeading ? "Original Word Count" : OriginalWordCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed Word Count" : TranscribedWordCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original Sentence Length No Separators" : OriginalSentenceLengthNoSeparators.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed Sentence Length No Separators" : TranscribedSentenceLengthNoSeparators.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original Matched Word Count" : OriginalMatchedWordCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Original Unmatched count" : OriginalUnmatchedWordCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed Matched Word Count" : TranscribedMatchedWordCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed Unmatched count" : TranscribedUnmatchedWordCount.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Sentence Main Score" : sentenceMapping.MainScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Sentence Extra Score" : sentenceMapping.ExtraScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Sentence Combined Score" : sentenceMapping.CombinedScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Required resync" : (sentenceMapping.RequiredResync ? "1" : "0"));
            sb.Append("\t");
            sb.Append(isHeading ? "Ignored" : (sentenceMapping.Ignored ? "1" : "0"));
            sb.AppendLine("");
        }

        // Dump start of resync info.
        public void DumpResyncTraceStart(SentenceMapping sentenceMapping)
        {
            MappingSentenceRun originalSentenceRun = sentenceMapping.OriginalSentence;
            MappingSentenceRun transcribedSentenceRun = sentenceMapping.TranscribedSentence;

            IncrementRequiredResyncCount();

            DumpDebugWordMappingResultsLabel(
                "############### Starting resync");
            DumpDebugWordMappingResultsLabel("OrigRaw(" +
                originalSentenceRun.WordCount.ToString() +
                "," +
                originalSentenceRun.TextLengthNoSeparators.ToString() +
                ")=" +
                originalSentenceRun.SentenceText);
            DumpDebugWordMappingResultsLabel("OrigWords=" + originalSentenceRun.SentenceFromWords);
            DumpDebugWordMappingResultsLabel("Resync start state:");
            DumpWordMappingResultsSentence(
                sentenceMapping,
                originalSentenceRun,
                transcribedSentenceRun,
                DumpWordTracingSB);
            DumpDebugWordMappingResultsLabel("Resync trace:");

            // Dump heading.
            DumpResyncTraceRow(
                sentenceMapping,
                -1,
                -1,
                null,
                0.0,
                null,
                0.0,
                0.0,
                0.0,
                0.0,
                false,
                0.0,
                0.0,
                0);
        }

        // Dump Resync trace row.
        public void DumpResyncTraceRow(
            SentenceMapping sentenceMapping,
            int transcribedCandidateStartIndex,
            int transcribedCandidateLength,
            MappingSentenceRun transcribedSentenceCandidate,
            double mainScore,
            MappingWordRun firstTranscribedWordRun,
            double firstWordScore,
            double firstWordAdjustedScore,
            double wordPositionFactor,
            double mainFactoredScore,
            bool newBest,
            double bestMainScore,
            double bestFactoredScore,
            int bestTranscribedStartIndex)
        {
            bool isHeading = (transcribedSentenceCandidate == null ? true : false);
            MultiLanguageItem studyItem = null;
            int sentenceIndex = 0;
            StringBuilder sb = DumpWordTracingSB;

            if (sb == null)
                return;

            string Path = null;
            string Verse = null;
            int SentenceNumber = 0;
            string FirstTranscribedWord = String.Empty;

            if (!isHeading)
            {
                studyItem = sentenceMapping.StudyItem;
                sentenceIndex = sentenceMapping.SentenceIndex;

                Path = studyItem.Content.GetNamePathString(LanguageLookup.English, "/");
                Verse = studyItem.GetNameStringWithOrdinal();
                SentenceNumber = sentenceIndex + 1;

                if (firstTranscribedWordRun != null)
                    FirstTranscribedWord = firstTranscribedWordRun.WordText;
            }

            sb.Append(isHeading ? "Path" : Path);
            sb.Append("\t");
            sb.Append(isHeading ? "Verse" : Verse);
            sb.Append("\t");
            sb.Append(isHeading ? "Sentence #" : SentenceNumber.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed Candidate Start Index" : transcribedCandidateStartIndex.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Transcribed Candidate Length" : transcribedCandidateLength.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Main Score" : mainScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "First Transcribed Word" : FirstTranscribedWord);
            sb.Append("\t");
            sb.Append(isHeading ? "First Word Score" : firstWordScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "First Word Adjusted Score" : firstWordAdjustedScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Word Position Factor" : wordPositionFactor.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Main Factored Score" : mainFactoredScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "New Best" : (newBest ? "1" : "0"));
            sb.Append("\t");
            sb.Append(isHeading ? "Best Main Score" : bestMainScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Best Factored Score" : bestFactoredScore.ToString());
            sb.Append("\t");
            sb.Append(isHeading ? "Best Transcribed Start Index" : bestTranscribedStartIndex.ToString());
            sb.AppendLine("");
        }

        // Dump Resync best found.
        public void DumpResyncTraceBestFound(SentenceMapping sentenceMapping)
        {
            DumpDebugWordMappingResultsLabel(
                "############### Resync best candidate found");
            DumpDebugWordMappingResultsLabel(
                "Now doing normal find best candidate length");
        }

        // Dump Resync failed.
        public void DumpResyncTraceFailed(SentenceMapping sentenceMapping)
        {
            DumpDebugWordMappingResultsLabel(
                "############### Resync failed");
        }

        // Dump Resync ignoring sentence.
        public void DumpResyncIgnoringSentence(SentenceMapping sentenceMapping)
        {
            DumpDebugWordMappingResultsLabel(
                "############### Ignore sentence triggered");
            DumpDebugWordMappingResultsLabel(
                "Main score: " + sentenceMapping.MainScore.ToString());

            IncrementIgnoredSentenceCount();

            DumpDebugWordMappingCandidateState(
                "Ignoring sentence",
                "",
                0,
                sentenceMapping);
        }

        // Dump number report to internal string builder.
        public void DumpNumberNormalization(string input, string output, int wordIndex)
        {
            if (DumpNumberTracingResults && (DumpNumberTracingSB != null))
                DumpNumberTracingSB.AppendLine(FormatWordLabel(wordIndex) + input + " -> " + output);

            if (Parent != null)
                Parent.DumpNumberNormalization(input, output, wordIndex);
        }

        // Dump number report to internal string builder.
        public void DumpAbbreviationNormalization(string input, string output, int wordIndex)
        {
            if (DumpAbbreviationTracingResults && (DumpAbbreviationTracingSB != null))
                DumpAbbreviationTracingSB.AppendLine(FormatWordLabel(wordIndex) + input + " -> " + output);

            if (Parent != null)
                Parent.DumpAbbreviationNormalization(input, output, wordIndex);

#if false
            if (
                    (input == "av") ||
                    (input == "s") ||
                    (input == "cia") ||
                    (input == "onu") ||
                    (input == "lic") ||
                    (input == "cm") ||
                    (input == "h") ||
                    (input == "d") ||
                    (input == "m") ||
                    (input == "CC"))
                ApplicationData.Global.PutConsoleMessage("DumpAbbreviationNormalization: input = " + input);
#endif
        }

        public string FormatSentenceLabel()
        {
            string path = String.Empty;

            if (!String.IsNullOrEmpty(CodeLabel))
                path += "(" + CodeLabel + ")";

            if (!String.IsNullOrEmpty(SentencePath))
            {
                if (!String.IsNullOrEmpty(path))
                    path += " ";

                path += SentencePath;
            }

            if (!String.IsNullOrEmpty(path))
                path += ": ";

            if ((path.Length % 24) != 0)
                path += TextUtilities.GetSpaces(24 - (path.Length % 24));

            return path;
        }

        public string FormatWordLabel(int wordIndex)
        {
            string path = String.Empty;

            if (!String.IsNullOrEmpty(CodeLabel))
                path += "(" + CodeLabel + ")";

            if (!String.IsNullOrEmpty(SentencePath))
            {
                if (!String.IsNullOrEmpty(path))
                    path += " ";

                path += SentencePath;
            }

            path += "[" + wordIndex.ToString() + "]: ";

            if ((path.Length % 24) != 0)
                path += TextUtilities.GetSpaces(24 - (path.Length % 24));

            return path;
        }

        public void SetupSentencePath(
            MultiLanguageItem studyItem,
            int sentenceIndex)
        {
            string path = String.Empty;

            if (!String.IsNullOrEmpty(ContentPath))
            {
                if (!String.IsNullOrEmpty(path))
                    path += "/";

                path += ContentPath;
            }

            if (studyItem != null)
            {
                string studyItemKey = studyItem.GetNameStringWithOrdinal();

                if (!String.IsNullOrEmpty(studyItemKey))
                {
                    if (!String.IsNullOrEmpty(path))
                        path += "/";

                    path += studyItemKey;
                }

                if (sentenceIndex >= 0)
                {
                    if (!String.IsNullOrEmpty(path))
                        path += ".";

                    path += (sentenceIndex + 1).ToString();
                }
            }

            SentencePath = path;

            if (Parent != null)
                Parent.SetupSentencePath(studyItem, sentenceIndex);
        }

        public void SetupContentPath(BaseObjectContent content)
        {
            if (content != null)
                ContentPath = content.GetNamePathString(LanguageLookup.English, "/");
            else
                ContentPath = String.Empty;

            if (Parent != null)
                Parent.SetupContentPath(content);
        }

        public void SetupCodeLabel(string codeLabel)
        {
            CodeLabel = codeLabel;

            if (Parent != null)
                Parent.SetupCodeLabel(codeLabel);
        }

        public void SetupTestName(string testName)
        {
            TestName = testName;

            if (Parent != null)
                Parent.SetupTestName(testName);
        }
    }
}
