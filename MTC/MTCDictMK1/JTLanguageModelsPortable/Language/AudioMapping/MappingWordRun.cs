using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public class MappingWordRun
    {
        // Possibly modified (substituted, combined) word text.
        public string WordText { get; set; }

        // Word text start index in text block.
        public int TextStartIndex { get; set; }

        // Word text length.
        public int TextLength
        {
            get
            {
                return (WordText != null ? WordText.Length : 0);
            }
        }

        // Word text end index in text block (last character + 1).
        public int TextStopIndex
        {
            get
            {
                return TextStartIndex + TextLength;
            }
        }

        // Word text start index in sentence.
        public int SentenceTextStartIndex { get; set; }

        // Word text start index relative to sentence start normalized to not include word separators.
        public int SentenceCharacterPosition { get; set; }

        // Word text end index in sentence (last character + 1).
        public int SentenceTextStopIndex
        {
            get
            {
                return SentenceTextStartIndex + TextLength;
            }
        }

        // Word count. (If combined, how many raw words.)
        public int WordCount { get; set; }

        // Word index in parent.
        public int ParentWordIndex { get; set; }

        // Word index in sentence run.
        public int SentenceWordIndex { get; set; }

        // Word start time.
        public TimeSpan StartTime { get; set; }

        // Word stop time.
        public TimeSpan StopTime { get; set; }

        // Word middle time.
        public TimeSpan MiddleTime
        {
            get
            {
                long ticks = (StopTime.Ticks + StartTime.Ticks) / 2;
                return new TimeSpan(ticks);
            }
        }

        // Word duration time.
        public TimeSpan Duration
        {
            get
            {
                return StopTime - StartTime;
            }
        }

        // Word match score calculated using character index.
        public double WordMatchScoreUsingCharacterIndex { get; set; }

        // Word match score calculated using word index.
        public double WordMatchScoreUsingWordIndex { get; set; }

        // Word match score calculated using word index.
        public double WordMatchScoreUsingWordAndCharacterIndex { get; set; }

        // Chosen word match score.
        public double WordMatchScore { get; set; }

        // Edit distance.
        public int EditDistance { get; set; }

        // Edit distance factor.
        public double EditDistanceFactor;

        // Word position factor using character index.
        public double WordPositionFactorUsingCharacterIndex;

        // Word position factor using word index.
        public double WordPositionFactorUsingWordIndex;

        // Chosen word position factor.
        public double WordPositionFactor;

        // Edit distance score.
        public double EditDistanceScore;

        // Matched word run.
        public MappingWordRun MatchedWordRun { get; set; }

        // Matched word index.
        public int MatchedWordIndex { get; set; }

        // Word was matched.
        public bool Matched { get; set; }

        // Set to true when finally consumed in a transcribed sentence run.
        public bool Used { get; set; }

        // Unmodified word text before substitutions.
        public string RawWordText { get; set; }

        // Raw word text start index before substitutions.
        public int RawTextStartIndex { get; set; }

        // Raw word text length before substitutions.
        public int RawTextLength
        {
            get
            {
                return (RawWordText != null ? RawWordText.Length : 0);
            }
        }

        // Word text end index in text block (last character + 1).
        public int RawTextStopIndex
        {
            get
            {
                return RawTextStartIndex + RawTextLength;
            }
        }

        // Constructor for transribed or test word run.
        public MappingWordRun(
            string wordText,
            string rawWordText,
            int textStartIndex,
            int rawTextStartIndex,
            int sentenceTextStartIndex,
            int sentenceCharacterPosition,
            int parentWordIndex,
            int sentenceWordIndex,
            TimeSpan startTime,
            TimeSpan stopTime)
        {
            ClearMappingWordRun();
            TextStartIndex = textStartIndex;
            RawTextStartIndex = rawTextStartIndex;
            SentenceTextStartIndex = sentenceTextStartIndex;
            SentenceCharacterPosition = sentenceCharacterPosition;
            WordText = wordText;
            RawWordText = rawWordText;
            ParentWordIndex = parentWordIndex;
            SentenceWordIndex = sentenceWordIndex;
            StartTime = startTime;
            StopTime = stopTime;
        }

        // Constructor for original word run.
        public MappingWordRun(
            string wordText,
            string rawWordText,
            int textStartIndex,
            int rawTextStartIndex,
            int sentenceTextStartIndex,
            int sentenceCharacterPosition,
            int parentWordIndex,
            int sentenceWordIndex)
        {
            ClearMappingWordRun();
            WordText = wordText;
            RawWordText = rawWordText;
            TextStartIndex = textStartIndex;
            RawTextStartIndex = rawTextStartIndex;
            SentenceTextStartIndex = sentenceTextStartIndex;
            SentenceCharacterPosition = sentenceCharacterPosition;
            ParentWordIndex = parentWordIndex;
            SentenceWordIndex = sentenceWordIndex;
        }

        // Constructor for combined word run.
        public MappingWordRun(
            List<MappingWordRun> wordRuns,
            int startIndex,
            int count)
        {
            ClearMappingWordRun();

            if ((count != 0) && (wordRuns != null) && (wordRuns.Count() != 0))
            {
                string wordText = String.Empty;
                string rawWordText = String.Empty;
                int endIndex = startIndex + count;

                if (endIndex > wordRuns.Count())
                    endIndex = wordRuns.Count();

                for (int index = startIndex; index < endIndex; index++)
                {
                    MappingWordRun wordRun = wordRuns[index];
                    wordText += wordRun.WordText;
                    rawWordText += wordRun.RawWordText;
                }

                MappingWordRun firstWordRun = wordRuns[startIndex];
                MappingWordRun lastWordRun = wordRuns[startIndex + count - 1];

                TextStartIndex = firstWordRun.TextStartIndex;
                SentenceTextStartIndex = firstWordRun.SentenceTextStartIndex;
                SentenceCharacterPosition = firstWordRun.SentenceCharacterPosition;
                WordText = wordText;
                RawWordText = rawWordText;
                WordCount = count;
                ParentWordIndex = firstWordRun.ParentWordIndex;
                SentenceWordIndex = firstWordRun.SentenceWordIndex;
                StartTime = firstWordRun.StartTime;
                StopTime = lastWordRun.StopTime;
            }
        }

        // Constructor for split word run.
        public MappingWordRun(
            MappingWordRun oldWordRun,
            string newWord,
            int expansionCharacterIndex,
            int expansionCharacterPosition,
            int expansionCharacterCount,
            int expansionWordIndex)
        {
            ClearMappingWordRun();
            TextStartIndex = oldWordRun.TextStartIndex + expansionCharacterIndex;
            SentenceTextStartIndex = oldWordRun.SentenceTextStartIndex + expansionCharacterIndex;
            SentenceCharacterPosition = oldWordRun.SentenceCharacterPosition + expansionCharacterPosition;
            WordText = newWord;
            RawWordText = oldWordRun.RawWordText;
            RawTextStartIndex = oldWordRun.RawTextStartIndex;
            WordCount = 1;
            ParentWordIndex = oldWordRun.ParentWordIndex + expansionWordIndex;
            SentenceWordIndex = oldWordRun.SentenceWordIndex + expansionWordIndex;
            double oldStartTime = oldWordRun.StartTime.TotalSeconds;
            double oldSeconds = oldWordRun.Duration.TotalSeconds;
            double newStartTime = oldStartTime + (expansionCharacterPosition * oldSeconds) / expansionCharacterCount;
            double newStopTime = oldStartTime + ((expansionCharacterPosition + TextLength) * oldSeconds) / expansionCharacterCount;
            StartTime = TimeSpan.FromSeconds(newStartTime);
            StopTime = TimeSpan.FromSeconds(newStopTime);
        }

        // Copy constructor.
        public MappingWordRun(MappingWordRun other)
        {
            ClearMappingWordRun();
            CopyMappingWordRun(other);
        }

        // Default constructor.
        public MappingWordRun()
        {
            ClearMappingWordRun();
        }

        // Clear mapping word run.
        protected void ClearMappingWordRun()
        {
            WordText = String.Empty;
            RawWordText = String.Empty;
            TextStartIndex = -1;
            SentenceTextStartIndex = -1;
            SentenceCharacterPosition = -1;
            WordCount = 1;
            ParentWordIndex = -1;
            SentenceWordIndex = -1;
            StartTime = TimeSpan.Zero;
            StopTime = TimeSpan.Zero;
            WordMatchScoreUsingCharacterIndex = 0.0;
            WordMatchScoreUsingWordIndex = 0.0;
            WordMatchScoreUsingWordAndCharacterIndex = 0.0;
            WordMatchScore = 0.0;
            EditDistance = 0;
            EditDistanceFactor = 0.0;
            WordPositionFactorUsingCharacterIndex = 0.0;
            WordPositionFactorUsingWordIndex = 0.0;
            WordPositionFactor = 0.0;
            EditDistanceScore = 0.0;
            MatchedWordRun = null;
            MatchedWordIndex = -1;
            Matched = false;
            Used = false;
        }

        // Copy mapping word run.
        protected void CopyMappingWordRun(MappingWordRun other)
        {
            WordText = other.WordText;
            RawWordText = other.RawWordText;
            TextStartIndex = other.TextStartIndex;
            SentenceTextStartIndex = other.SentenceTextStartIndex;
            SentenceCharacterPosition = other.SentenceCharacterPosition;
            WordCount = other.WordCount;
            ParentWordIndex = other.ParentWordIndex;
            SentenceWordIndex = other.SentenceWordIndex;
            StartTime = other.StartTime;
            StopTime = other.StopTime;
            EditDistance = other.EditDistance;
            EditDistanceFactor = other.EditDistanceFactor;
            EditDistanceScore = other.EditDistanceScore;
            WordPositionFactorUsingCharacterIndex = other.WordPositionFactorUsingCharacterIndex;
            WordPositionFactorUsingWordIndex = other.WordPositionFactorUsingWordIndex;
            WordPositionFactor = other.WordPositionFactor;
            WordMatchScoreUsingCharacterIndex = other.WordMatchScoreUsingCharacterIndex;
            WordMatchScoreUsingWordIndex = other.WordMatchScoreUsingWordIndex;
            WordMatchScoreUsingWordAndCharacterIndex = other.WordMatchScoreUsingWordAndCharacterIndex;
            WordMatchScore = other.WordMatchScore;
            MatchedWordRun = other.CloneMatchedWordRun();
            MatchedWordIndex = other.MatchedWordIndex;
            Matched = other.Matched;
            Used = other.Used;
        }

        // Deep clone this word run.
        public MappingWordRun CloneMatchedWordRun()
        {
            if (MatchedWordRun == null)
                return null;

            MappingWordRun wordRun = new MappingWordRun(MatchedWordRun);

            return wordRun;
        }

        // Create clone of this word run, but truncated to given character count.
        public MappingWordRun CloneTruncatedWordRun(int characterCount)
        {
            string truncatedWord = WordText.Substring(0, characterCount);
            MappingWordRun truncatedWordRun = new MappingWordRun(
                truncatedWord,
                RawWordText,
                TextStartIndex,
                RawTextStartIndex,
                SentenceTextStartIndex,
                SentenceCharacterPosition,
                ParentWordIndex,
                SentenceWordIndex,
                StartTime,
                StopTime);
            return truncatedWordRun;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(WordText);
            sb.Append(",");
            sb.Append(RawWordText);
            sb.Append(",");
            sb.Append(SentenceCharacterPosition.ToString());
            sb.Append(",");
            sb.Append(SentenceTextStartIndex.ToString());
            sb.Append(",");
            sb.Append(TextStartIndex.ToString());
            sb.Append(",");
            sb.Append(SentenceWordIndex.ToString());
            sb.Append(",");
            sb.Append(ParentWordIndex.ToString());
            sb.Append(",");
            sb.Append(StartTime.ToString());
            sb.Append(",");
            sb.Append(StopTime.ToString());
            sb.Append(",");
            sb.Append(WordMatchScoreUsingCharacterIndex.ToString());
            sb.Append(",");
            sb.Append(WordMatchScoreUsingWordIndex.ToString());
            sb.Append(",");
            sb.Append(WordMatchScoreUsingWordAndCharacterIndex.ToString());
            sb.Append(",");
            sb.Append(EditDistance.ToString());
            sb.Append(",");
            sb.Append(EditDistanceFactor.ToString());
            sb.Append(",");
            sb.Append(WordPositionFactorUsingCharacterIndex.ToString());
            sb.Append(",");
            sb.Append(WordPositionFactorUsingCharacterIndex.ToString());
            sb.Append(",");
            sb.Append(EditDistanceScore.ToString());

            if (MatchedWordRun != null)
            {
                sb.Append(",");
                sb.Append(MatchedWordRun.WordText);
            }

            sb.Append(",");
            sb.Append(MatchedWordIndex.ToString());
            sb.Append(",");
            sb.Append(Matched ? "matched" : "not matched");

            return sb.ToString();
        }
    }
}
