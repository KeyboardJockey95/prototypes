using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.Language
{
    public class MappingSentenceRun
    {
        // Sentence text after substitutions.
        public string SentenceText { get; set; }

        // Sentence text before substitutions.
        public string RawSentenceText { get; set; }

        // Sentence text start index in text block.
        public int TextStartIndex { get; set; }

        // Sentence text length.
        public int TextLength { get; set; }

        // Sentence text end index in text block (last character + 1).
        public int TextStopIndex
        {
            get
            {
                return TextStartIndex + TextLength;
            }
            set
            {
                TextLength = value - TextStartIndex;
            }
        }

        public int TextLengthNoSeparators
        {
            get
            {
                if (WordRuns == null)
                    return 0;

                int textLength = 0;

                foreach (MappingWordRun wordRun in WordRuns)
                    textLength += wordRun.TextLength;

                return textLength;
            }
        }

        // List of word runs.
        public List<MappingWordRun> WordRuns { get; set; }

        // Index of first word run in parent.
        public int ParentStartWordIndex { get; set; }

        // Word run count.
        public int WordCount
        {
            get
            {
                if (WordRuns != null)
                    return WordRuns.Count();
                return 0;
            }
        }

        // Index of word after last word run in parent.
        public int ParentStopWordIndex
        {
            get
            {
                return ParentStartWordIndex + WordCount;
            }
        }

        // Matched word count.
        public int MatchedCount { get; set; }

        // Unmatched word count.
        public int UnmatchedCount { get; set; }

        // Unmatched word count.
        public List<string> UnmatchedWords{ get; set; }

        // Sum of word match scores.
        public double WordMatchScoreSum { get; set; }

        // Main constructor.
        public MappingSentenceRun(
            int textStartIndex,
            int textLength,
            string sentenceText,
            string rawSentenceText,
            List<MappingWordRun> wordRuns,
            int parentStartWordIndex)
        {
            ClearMappingSentenceRun();
            TextStartIndex = textStartIndex;
            TextLength = textLength;
            SentenceText = sentenceText;
            RawSentenceText = rawSentenceText;
            WordRuns = wordRuns;
            ParentStartWordIndex = parentStartWordIndex;
        }

        public MappingSentenceRun(MappingSentenceRun other)
        {
            ClearMappingSentenceRun();
            CopyMappingSentenceRun(other);
        }

        public MappingSentenceRun()
        {
            ClearMappingSentenceRun();
        }

        protected void ClearMappingSentenceRun()
        {
            SentenceText = String.Empty;
            RawSentenceText = String.Empty;
            WordRuns = null;
            TextStartIndex = 0;
            TextLength = 0;
            ParentStartWordIndex = 0;
            MatchedCount = 0;
            UnmatchedCount = 0;
            UnmatchedWords = null;
            WordMatchScoreSum = 0.0;
        }

        protected void CopyMappingSentenceRun(MappingSentenceRun other)
        {
            SentenceText = other.SentenceText;
            RawSentenceText = other.RawSentenceText;
            WordRuns = other.CloneWordRuns();
            TextStartIndex = other.TextStartIndex;
            TextLength = other.TextLength;
            ParentStartWordIndex = other.ParentStartWordIndex;
            MatchedCount = other.MatchedCount;
            UnmatchedCount = other.UnmatchedCount;
            UnmatchedWords = (other.UnmatchedWords != null ? new List<string>(other.UnmatchedWords) : null);
            WordMatchScoreSum = other.WordMatchScoreSum;
        }

        public MappingWordRun GetWordRun(int sentenceWordIndex)
        {
            if (WordRuns == null)
                return null;

            if ((sentenceWordIndex < 0) || (sentenceWordIndex >= WordCount))
                //throw new Exception("MappingSentenceRun.GetWordRun: index out of bounds: " + sentenceWordIndex.ToString());
                return null;

            MappingWordRun wordRun = WordRuns[sentenceWordIndex];

            return wordRun;
        }

        public MappingWordRun FirstWordRun
        {
            get
            {
                if (WordRuns == null)
                    return null;

                if (WordCount > 0)
                    return WordRuns[0];

                return null;
            }
        }

        public MappingWordRun FirstMatchedWordRun
        {
            get
            {
                if (WordRuns == null)
                    return null;

                if (WordCount == 0)
                    return null;

                foreach (MappingWordRun wordRun in WordRuns)
                {
                    if (wordRun.Matched)
                        return wordRun;
                }

                return null;
            }
        }

        public MappingWordRun SecondWordRun
        {
            get
            {
                if (WordRuns == null)
                    return null;

                if (WordCount > 1)
                    return WordRuns[1];

                return null;
            }
        }

        public MappingWordRun SecondMatchedWordRun
        {
            get
            {
                if (WordRuns == null)
                    return null;

                if (WordCount == 0)
                    return null;

                bool first = true;

                foreach (MappingWordRun wordRun in WordRuns)
                {
                    if (wordRun.Matched)
                    {
                        if (!first)
                            return wordRun;

                        first = false;
                    }
                }

                return null;
            }
        }

        public MappingWordRun LastWordRun
        {
            get
            {
                if (WordRuns == null)
                    return null;

                if (WordCount > 0)
                    return WordRuns[WordCount - 1];

                return null;
            }
        }

        public MappingWordRun LastMatchedWordRun
        {
            get
            {
                if (WordRuns == null)
                    return null;

                if (WordCount == 0)
                    return null;

                int wordCount = WordCount;

                for (int wordIndex = wordCount - 1; wordIndex >= 0; wordIndex--)
                {
                    MappingWordRun wordRun = GetWordRun(wordIndex);

                    if (wordRun.Matched)
                        return wordRun;
                }

                return null;
            }
        }

        public TimeSpan FirstMatchedWordStartTime
        {
            get
            {
                MappingWordRun wordRun = FirstMatchedWordRun;

                if ((wordRun != null) && (wordRun.MatchedWordRun != null))
                    return wordRun.MatchedWordRun.StartTime;

                return TimeSpan.Zero;
            }
        }

        public TimeSpan FirstMatchedWordStopTime
        {
            get
            {
                MappingWordRun wordRun = FirstMatchedWordRun;

                if ((wordRun != null) && (wordRun.MatchedWordRun != null))
                    return wordRun.MatchedWordRun.StopTime;

                return TimeSpan.Zero;
            }
        }

        public TimeSpan LastMatchedWordStartTime
        {
            get
            {
                MappingWordRun wordRun = LastMatchedWordRun;

                if ((wordRun != null) && (wordRun.MatchedWordRun != null))
                    return wordRun.MatchedWordRun.StartTime;

                return TimeSpan.Zero;
            }
        }

        public TimeSpan LarstMatchedWordStopTime
        {
            get
            {
                MappingWordRun wordRun = LastMatchedWordRun;

                if ((wordRun != null) && (wordRun.MatchedWordRun != null))
                    return wordRun.MatchedWordRun.StopTime;

                return TimeSpan.Zero;
            }
        }

        public string GetWordSpanNoSpace(int sentenceWordIndex, int wordCount)
        {
            int wordOffset;
            string wordSpan = String.Empty;

            for (wordOffset = 0; wordOffset < wordCount; wordOffset++)
            {
                int wordIndex = sentenceWordIndex + wordOffset;

                if (wordIndex >= WordCount)
                    return wordSpan;

                MappingWordRun wordRun = GetWordRun(wordIndex);

                if (wordRun != null)
                    wordSpan += wordRun.WordText;
            }

            return wordSpan;
        }

        public void CombineWordRuns(
            int sentenceWordIndex,
            int wordCount)
        {
            if (wordCount <= 1)
                return;

            MappingWordRun wordRun = new MappingWordRun(
                WordRuns,
                sentenceWordIndex,
                wordCount);

            WordRuns.RemoveRange(sentenceWordIndex, wordCount);
            WordRuns.Insert(sentenceWordIndex, wordRun);

            for (sentenceWordIndex++; sentenceWordIndex < WordRuns.Count(); sentenceWordIndex++)
                WordRuns[sentenceWordIndex].SentenceWordIndex = sentenceWordIndex;
        }

        public List<MappingWordRun> CopyWordRunRange(
            int sentenceWordStartIndex,
            int wordCount)
        {
            if (WordRuns == null)
                return null;

            int parentWordCount = WordCount;
            List<MappingWordRun> wordRuns = new List<MappingWordRun>();

            for (int index = 0; index < wordCount; index++)
            {
                int parentWordIndex = sentenceWordStartIndex + index;

                if (parentWordIndex > parentWordCount)
                    break;

                MappingWordRun parentWordRun = WordRuns[parentWordIndex];
                MappingWordRun wordRun = new MappingWordRun(parentWordRun);
                wordRuns.Add(wordRun);
            }

            return wordRuns;
        }

        public string SentenceFromWords
        {
            get
            {
                int c = WordCount;
                int i;
                StringBuilder sb = new StringBuilder();
                for (i = 0; i < c; i++)
                {
                    MappingWordRun wordRun = GetWordRun(i);
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
        }

        public string MatchedSentenceFromWords
        {
            get
            {
                int c = WordCount;
                int i;
                StringBuilder sb = new StringBuilder();
                for (i = 0; i < c; i++)
                {
                    MappingWordRun wordRun = GetWordRun(i);
                    string word;
                    if (wordRun != null)
                    {
                        wordRun = wordRun.MatchedWordRun;

                        if (wordRun != null)
                            word = wordRun.WordText;
                        else
                            word = "*";
                    }
                    else
                        word = "(null)";

                    if (sb.Length != 0)
                        sb.Append(" ");

                    sb.Append(word);
                }
                return sb.ToString();
            }
        }

        public string MatchedSentenceFromWordsRaw
        {
            get
            {
                int c = WordCount;
                int i;
                StringBuilder sb = new StringBuilder();
                for (i = 0; i < c; i++)
                {
                    MappingWordRun wordRun = GetWordRun(i);
                    string word;
                    if (wordRun != null)
                    {
                        wordRun = wordRun.MatchedWordRun;

                        if (wordRun != null)
                            word = wordRun.WordText;
                        else
                            word = "";
                    }
                    else
                        word = "";

                    if (sb.Length != 0)
                        sb.Append(" ");

                    sb.Append(word);
                }
                return sb.ToString();
            }
        }

        public void ResetMatches()
        {
            int c = WordCount;
            int i;
            StringBuilder sb = new StringBuilder();
            for (i = 0; i < c; i++)
            {
                MappingWordRun wordRun = GetWordRun(i);
                if (wordRun != null)
                {
                    wordRun.Matched = false;
                    wordRun.MatchedWordIndex = -1;
                    wordRun.MatchedWordRun = null;
                }
            }
        }

        public List<MappingWordRun> CloneWordRuns()
        {
            List<MappingWordRun> wordRuns;

            if (WordRuns == null)
                wordRuns = null;
            else
            {
                wordRuns = new List<MappingWordRun>();

                foreach (MappingWordRun wordRun in WordRuns)
                    wordRuns.Add(new MappingWordRun(wordRun));
            }

            return wordRuns;
        }

        public static MappingSentenceRun CloneMappingSentenceRun(MappingSentenceRun other)
        {
            if (other == null)
                return null;

            return new MappingSentenceRun(other);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(SentenceText);
            sb.Append(",");
            sb.Append(RawSentenceText);
            sb.Append(",");
            sb.Append(TextStartIndex.ToString());
            sb.Append(",");
            sb.Append(TextLength.ToString());
            sb.Append(",");
            sb.Append(ParentStartWordIndex.ToString());
            sb.Append(",");
            sb.Append(WordCount.ToString());
            sb.Append(",");
            sb.Append(MatchedCount.ToString());
            sb.Append(",");
            sb.Append(UnmatchedCount.ToString());
            sb.Append(",");
            sb.Append(WordMatchScoreSum.ToString());

            return sb.ToString();
        }
    }
}
