using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;

namespace JTLanguageModelsPortable.Language
{
    public class SentenceRun
    {
        public string ParentText;
        public string SentenceText;
        public int SentenceTextStartIndex;
        public int SentenceTextStopIndex;
        public int SentenceTextLength { get { return SentenceTextStopIndex - SentenceTextStartIndex; } }
        public int SentenceTextLengthNoSeparators;
        public List<WordRun> ParentWordRuns;
        public List<WordRun> SentenceWordRuns;
        public int ParentWordCount;
        public int StartWordIndexInParent;
        public int StopWordIndexInParent;
        public int SentenceWordCount { get { return StopWordIndexInParent - StartWordIndexInParent; } }
        public int FinalSentenceWordCount;
        public TimeSpan SentenceStartTime;
        public TimeSpan SentenceStopTime;
        public TimeSpan SentenceLengthTime { get { return SentenceStopTime - SentenceStartTime; } }
        public int StudyItemIndex;
        public MultiLanguageItem StudyItem;
        public int SentenceIndex;
        public TextRun StudySentence;
        public int ParentSentenceIndex;
        public double Score;
        public double CombinedScore;
        public double MainScorePriorToResync;
        public string BestParsedSentencePriorToResync;
        public int MatchedWordCount;
        public int UnmatchedWordCount;
        public List<string> UnmatchedWords;
        public TimeSpan SilenceStartFront;
        public TimeSpan SilenceStopFront;
        public TimeSpan SilenceLengthFront { get { return SilenceStopFront - SilenceStartFront; } }
        public TimeSpan SilenceStartBack;
        public TimeSpan SilenceStopBack;
        public TimeSpan SilenceLengthBack { get { return SilenceStopBack - SilenceStartBack; } }
        public SentenceRun ExtraSentenceRun;
        public bool RequiredReset;
        public bool Ignored;
        public string Error;

        // For a study time.
        public SentenceRun(
            string parentText,
            string sentenceText,
            int sentenceTextStartIndex,
            int sentenceTextStopIndex,
            List<WordRun> parentWordRuns,
            int startWordIndexInParent,
            int stopWordIndexInParent,
            TimeSpan sentenceStartTime,
            TimeSpan sentenceStopTime,
            int studyItemIndex,
            MultiLanguageItem studyItem,
            int sentenceIndex,
            TextRun studySentence,
            int parentSentenceIndex)
        {
            ParentText = parentText;
            SentenceText = sentenceText;
            SentenceTextStartIndex = sentenceTextStartIndex;
            SentenceTextStopIndex = sentenceTextStopIndex;
            SentenceTextLengthNoSeparators = 0;
            ParentWordRuns = parentWordRuns;
            ParentWordCount = parentWordRuns.Count();
            StartWordIndexInParent = startWordIndexInParent;
            StopWordIndexInParent = stopWordIndexInParent;
            SentenceWordRuns = null;
            SentenceStartTime = sentenceStartTime;
            SentenceStopTime = sentenceStopTime;
            StudyItemIndex = studyItemIndex;
            StudyItem = studyItem;
            SentenceIndex = sentenceIndex;
            StudySentence = studySentence;
            ParentSentenceIndex = parentSentenceIndex;
            Score = 0.0;
            CombinedScore = 0.0;
            MainScorePriorToResync = 0.0;
            BestParsedSentencePriorToResync = String.Empty;
            MatchedWordCount = 0;
            UnmatchedWordCount = 0;
            UnmatchedWords = null;
            SilenceStartFront = TimeSpan.Zero;
            SilenceStopFront = TimeSpan.Zero;
            SilenceStartBack = TimeSpan.Zero;
            SilenceStopBack = TimeSpan.Zero;
            ExtraSentenceRun = null;
            RequiredReset = false;
            Ignored = false;
            Error = null;

            // This converts this run to locally saved word runs.
            SetSentenceWordRunsFromParent();

            UpdateSentenceLengthNoSeparators();
        }

        // For parsed word runs.
        public SentenceRun(
            string parentText,
            List<WordRun> parentWordRuns,
            int startWordIndexInParent,
            int stopWordIndexInParent,
            int parsedSentenceIndex)
        {
            ParentText = parentText;
            if (startWordIndexInParent >= parentWordRuns.Count())
            {
                SentenceTextStartIndex = parentText.Length;
                SentenceTextStopIndex = parentText.Length;
            }
            else
            {
                SentenceTextStartIndex = parentWordRuns[startWordIndexInParent].StartIndex;

                if (stopWordIndexInParent > startWordIndexInParent)
                    SentenceTextStopIndex = parentWordRuns[stopWordIndexInParent - 1].StopIndex;
                else
                    SentenceTextStopIndex = SentenceTextStartIndex;
            }
            if (SentenceTextStopIndex > SentenceTextStartIndex)
                SentenceText = parentText.Substring(SentenceTextStartIndex, SentenceTextStopIndex - SentenceTextStartIndex);
            else
                SentenceText = string.Empty;
            ParentWordRuns = parentWordRuns;
            ParentWordCount = parentWordRuns.Count();
            StartWordIndexInParent = startWordIndexInParent;
            StopWordIndexInParent = stopWordIndexInParent;
            SentenceWordRuns = null;
            UpdateTimes();
            StudyItemIndex = -1;
            StudyItem = null;
            SentenceIndex = -1;
            StudySentence = null;
            ParentSentenceIndex = parsedSentenceIndex;
            Score = 0.0;
            CombinedScore = 0.0;
            MainScorePriorToResync = 0.0;
            BestParsedSentencePriorToResync = String.Empty;
            MatchedWordCount = 0;
            UnmatchedWordCount = 0;
            UnmatchedWords = null;
            SilenceStartFront = TimeSpan.Zero;
            SilenceStopFront = TimeSpan.Zero;
            SilenceStartBack = TimeSpan.Zero;
            SilenceStopBack = TimeSpan.Zero;
            ExtraSentenceRun = null;
            RequiredReset = false;
            Ignored = false;
            Error = null;

            // This converts this run to locally saved word runs.
            SetSentenceWordRunsFromParent();

            UpdateSentenceLengthNoSeparators();
        }

        // Copy constructor.
        public SentenceRun(SentenceRun other)
        {
            ParentText = other.ParentText;
            SentenceText = other.SentenceText;
            SentenceTextStartIndex = other.SentenceTextStartIndex;
            SentenceTextStopIndex = other.SentenceTextStopIndex;
            ParentWordRuns = other.ParentWordRuns;
            ParentWordCount = other.ParentWordCount;
            StartWordIndexInParent = other.StartWordIndexInParent;
            StopWordIndexInParent = other.StopWordIndexInParent;
            if (other.SentenceWordRuns != null)
                SentenceWordRuns = new List<WordRun>(other.SentenceWordRuns);
            else
                SentenceWordRuns = null;
            SentenceTextLengthNoSeparators = other.SentenceTextLengthNoSeparators;
            SentenceStartTime = other.SentenceStartTime;
            SentenceStopTime = other.SentenceStopTime;
            StudyItemIndex = other.StudyItemIndex;
            StudyItem = other.StudyItem;
            SentenceIndex = other.SentenceIndex;
            StudySentence = other.StudySentence;
            ParentSentenceIndex = other.ParentSentenceIndex;
            Score = other.Score;
            CombinedScore = other.CombinedScore;
            MainScorePriorToResync = other.MainScorePriorToResync;
            BestParsedSentencePriorToResync = other.BestParsedSentencePriorToResync;
            MatchedWordCount = other.MatchedWordCount;
            UnmatchedWordCount = other.UnmatchedWordCount;
            UnmatchedWords = null;
            SilenceStartFront = other.SilenceStartFront;
            SilenceStopFront = other.SilenceStopFront;
            SilenceStartBack = other.SilenceStartBack;
            SilenceStopBack = other.SilenceStopBack;
            ExtraSentenceRun = other.ExtraSentenceRun;
            RequiredReset = other.RequiredReset;
            Ignored = other.Ignored;
            Error = other.Error;
        }

        public override string ToString()
        {
            return
                "StudyItemIndex=" + StudyItemIndex.ToString()
                + " "
                + "SentenceIndex=" + SentenceIndex.ToString()
                + " "
                + "ParentSentenceIndex=" + ParentSentenceIndex.ToString()
                + " "
                + "StartWordIndexInParent=" + StartWordIndexInParent.ToString()
                + " "
                + "StopWordIndexInParent=" + StopWordIndexInParent.ToString()
                + " "
                + "Score=" + Score.ToString()
                + " "
                + "CombinedScore=" + CombinedScore.ToString()
                + " "
                + "MatchedWordCount=" + MatchedWordCount.ToString()
                + " "
                + "UnmatchedWordCount=" + UnmatchedWordCount.ToString()
                + " "
                + "UnmatchedWords=" + (UnmatchedWords != null ? ObjectUtilities.GetStringFromStringList(UnmatchedWords) : String.Empty)
                + " "
                + "SentenceStartTime=" + SentenceStartTime.ToString()
                + " "
                + "SentenceStopTime=" + SentenceStopTime.ToString()
                + " "
                + "SentenceText=" + SentenceText
                + " "
                + "SentenceTextLengthNoSeparators=" + SentenceTextLengthNoSeparators.ToString()
                + " "
                + (!String.IsNullOrEmpty(Error) ? Error : "");
        }

        public string Label
        {
            get
            {
                if (StudyItem != null)
                    return StudyItem.GetNamePathStringWithOrdinalInclusive(LanguageLookup.English, "/");
                else
                    return StudyItemIndex.ToString();
            }
        }

        // Update start word index.
        public void UpdateStartIndex(int newStartIndex)
        {
            StartWordIndexInParent = newStartIndex;

            if (StopWordIndexInParent > StartWordIndexInParent)
            {
                if (StopWordIndexInParent >= 0)
                    SentenceTextStartIndex = ParentWordRuns[StartWordIndexInParent].StartIndex;
                else
                    SentenceTextStartIndex = 0;
            }
            else
            {
                if (StartWordIndexInParent >= 0)
                    SentenceTextStopIndex = ParentWordRuns[StartWordIndexInParent].StartIndex;
                else
                    SentenceTextStopIndex = 0;
            }

            SentenceText = ParentText.Substring(SentenceTextStartIndex, SentenceTextStopIndex - SentenceTextStartIndex);

            if (SentenceWordRuns != null)
                SetSentenceWordRunsFromParent();

            UpdateSentenceLengthNoSeparators();
            UpdateTimes();
        }

        // Update stop word index.
        public void UpdateStopIndex(int newStopIndex)
        {
            StopWordIndexInParent = newStopIndex;

            if (StopWordIndexInParent > StartWordIndexInParent)
            {
                if (StopWordIndexInParent <= ParentWordRuns.Count())
                    SentenceTextStopIndex = ParentWordRuns[StopWordIndexInParent - 1].StopIndex;
                else
                    SentenceTextStopIndex = ParentText.Length;
            }
            else
            {
                if (StartWordIndexInParent < ParentWordRuns.Count())
                    SentenceTextStopIndex = ParentWordRuns[StartWordIndexInParent].StopIndex;
                else
                    SentenceTextStopIndex = ParentText.Length;
            }

            SentenceText = ParentText.Substring(SentenceTextStartIndex, SentenceTextStopIndex - SentenceTextStartIndex);

            if (SentenceWordRuns != null)
                SetSentenceWordRunsFromParent();

            UpdateSentenceLengthNoSeparators();
            UpdateTimes();
        }

        public void SetSentenceWordRunsFromParent()
        {
            if (SentenceWordRuns == null)
                SentenceWordRuns = new List<WordRun>();

            int count = SentenceWordCount;
            int localCount = SentenceWordRuns.Count();

            for (int i = 0; i < count; i++)
            {
                WordRun wordRun = ParentWordRuns[StartWordIndexInParent + i];

                if (i < localCount)
                {
                    if (SentenceWordRuns[i] != wordRun)
                        SentenceWordRuns[i] = wordRun;
                }
                else
                {
                    SentenceWordRuns.Add(wordRun);
                    localCount++;
                }
            }

            if (localCount > count)
            {
                for (int i = localCount - 1; i >= count; i--)
                    SentenceWordRuns.RemoveAt(i);
            }
        }

        public void UpdateSentenceLengthNoSeparators()
        {
            if (StopWordIndexInParent > StartWordIndexInParent)
            {
                int wordCount = SentenceWordCount;
                int length = 0;

                for (int i = 0; i < wordCount; i++)
                {
                    WordRun wordRun = GetWordRun(i);
                    wordRun.StartIndexNoSeparators = length;
                    length += wordRun.WordLength;
                    wordRun.StopIndexNoSeparators = length;
                }

                SentenceTextLengthNoSeparators = length;
            }
            else if (StopWordIndexInParent == StartWordIndexInParent)
            {
                if (StopWordIndexInParent < ParentWordRuns.Count())
                {
                    WordRun wordRun = GetWordRun(0);

                    if (wordRun != null)
                        SentenceTextLengthNoSeparators = wordRun.WordLength;
                    else
                        SentenceTextLengthNoSeparators = 0;
                }
                else
                    SentenceTextLengthNoSeparators = 0;
            }
            else
                SentenceTextLengthNoSeparators = 0;

            FinalSentenceWordCount = SentenceWordCount;
        }

        public void UpdateTimes()
        {
            if ((StartWordIndexInParent >= 0) && (StartWordIndexInParent < ParentWordRuns.Count()))
            {
                WordRun wordRun = GetWordRun(0);

                if (wordRun != null)
                    SentenceStartTime = wordRun.StartTime;
                else
                    SentenceStartTime = TimeSpan.Zero;
            }
            else if (ParentWordRuns.Count() != 0)
                SentenceStartTime = ParentWordRuns.Last().StopTime;
            else
                SentenceStartTime = TimeSpan.Zero;

            if ((StopWordIndexInParent > StartWordIndexInParent) && (StopWordIndexInParent <= ParentWordRuns.Count()))
                SentenceStopTime = ParentWordRuns[StopWordIndexInParent - 1].StopTime;
            else if (ParentWordRuns.Count() != 0)
                SentenceStopTime = ParentWordRuns.Last().StopTime;
            else
                SentenceStopTime = TimeSpan.Zero;
        }

        public void ClearWordFlags()
        {
            int sentenceWordIndex = 0;
            int sentenceWordCount = SentenceWordCount;

            for (sentenceWordIndex = 0; sentenceWordIndex < sentenceWordCount; sentenceWordIndex++)
            {
                WordRun wordRun = GetWordRun(sentenceWordIndex);

                if (wordRun != null)
                {
                    wordRun.ClearDataFields();
                    wordRun.WordIndex = sentenceWordIndex;
                }
            }

            UnmatchedWords = null;
        }

        public void SetUnmatched()
        {
            ClearWordFlags();

            Score = 0.0;
            CombinedScore = 0.0;
            RequiredReset = true;
            Ignored = true;

            UnmatchedWords = new List<string>(1) { "*" };
        }

        public WordRun GetWordRun(int indexInSentence)
        {
            if (ParentWordRuns == null)
                return null;

            if ((indexInSentence < 0) || (indexInSentence >= SentenceWordCount))
                //throw new Exception("GetWordRun: index out of bounds: " + indexInSentence.ToString());
                return null;

            if (SentenceWordRuns != null)
                return SentenceWordRuns[indexInSentence];

            WordRun wordRun = ParentWordRuns[StartWordIndexInParent + indexInSentence];

            return wordRun;
        }

        public void SetLocalWordRun(int indexInSentence, WordRun wordRun)
        {
#if true
            if (SentenceWordRuns == null)
                SetSentenceWordRunsFromParent();

            if ((indexInSentence >= 0) && (indexInSentence < SentenceWordCount))
                SentenceWordRuns[indexInSentence] = wordRun;
            else if (indexInSentence == SentenceWordCount)
                SentenceWordRuns.Add(wordRun);
            else
                throw new Exception("SetLocalWordRun: index out of range.");
#else
            if ((indexInSentence < 0) || (indexInSentence >= SentenceWordCount))
                return;

            ParentWordRuns[StartWordIndexInParent + indexInSentence] = wordRun;
#endif
        }

        public WordRun LastWordRun
        {
            get
            {
                if (SentenceWordCount != 0)
                    return GetWordRun(SentenceWordCount - 1);

                return null;
            }
        }

        public WordRun LastWordRunPossiblyCombined
        {
            get
            {
                if (SentenceWordCount != 0)
                {
                    int WordIndex;
                    WordRun wordRun = null;

                    for (WordIndex = SentenceWordCount - 1; WordIndex >= 0; WordIndex--)
                    {
                        wordRun = GetWordRun(WordIndex);

                        if (wordRun.WordSpanCount >= 1)
                            return wordRun;
                    }
                }

                return null;
            }
        }

        public void SetParentAndLocalWordRun(int indexInSentence, WordRun wordRun)
        {
            if ((indexInSentence < 0) || (indexInSentence >= SentenceWordCount))
                return;

            ParentWordRuns[StartWordIndexInParent + indexInSentence] = wordRun;

#if true
            if (SentenceWordRuns != null)
                SetLocalWordRun(indexInSentence, wordRun);
#endif
        }

        public string GetWordSpan(int indexInSentence, int wordCount)
        {
            int wordOffset;
            string wordSpan = String.Empty;

            for (wordOffset = 0; wordOffset < wordCount; wordOffset++)
            {
                int wordIndex = indexInSentence + wordOffset;

                if (wordIndex >= SentenceWordCount)
                    return wordSpan;

                WordRun wordRun = GetWordRun(wordIndex);

                if (wordRun != null)
                {
                    if (!String.IsNullOrEmpty(wordSpan))
                        wordSpan += " ";

                    wordSpan += wordRun.Word;
                }
            }

            return wordSpan;
        }

        public string GetWordSpanNoSpace(int indexInSentence, int wordCount)
        {
            int wordOffset;
            string wordSpan = String.Empty;

            for (wordOffset = 0; wordOffset < wordCount; wordOffset++)
            {
                int wordIndex = indexInSentence + wordOffset;

                if (wordIndex >= SentenceWordCount)
                    return wordSpan;

                WordRun wordRun = GetWordRun(wordIndex);

                if (wordRun != null)
                    wordSpan += wordRun.Word;
            }

            return wordSpan;
        }

        public string GetWord(int indexInSentence)
        {
            if ((indexInSentence < 0) || (indexInSentence >= SentenceWordCount))
                //throw new Exception("GetWordRun: index out of bounds: " + indexInSentence.ToString());
                return null;

            WordRun wordRun = GetWordRun(indexInSentence);

            if (wordRun != null)
                return wordRun.Word;

            return String.Empty;
        }

        public int GetMatchingWordSpanCount(
            int indexInSentence,
            string[] words)
        {
            int matchingCount = 0;

            for (int i = 0; i < words.Length; i++)
            {
                string word1 = GetWord(indexInSentence + i);
                string word2 = words[i];

                if (word1 != word2)
                    return matchingCount;

                matchingCount++;
            }

            return matchingCount;
        }

        public List<WordRun> GetWordRuns()
        {
            if (SentenceWordRuns != null)
                return new List<WordRun>(SentenceWordRuns);

            return ParentWordRuns.GetRange(StartWordIndexInParent, SentenceWordCount);
        }

        public string SentenceFromWords
        {
            get
            {
                int c = SentenceWordCount;
                int i;
                StringBuilder sb = new StringBuilder();
                for (i = 0; i < c; i++)
                {
                    WordRun wordRun = GetWordRun(i);
                    string word;
                    if (wordRun != null)
                    {
                        if (wordRun.WordSpanCount != 0)
                        {
                            if (String.IsNullOrEmpty(wordRun.MatchedWord))
                            {
                                if (wordRun.Matched)
                                    word = wordRun.Word;
                                else
                                    word = "-" + wordRun.Word + "-";
                            }
                            else
                            {
                                if (wordRun.Matched)
                                    word = wordRun.MatchedWord;
                                else
                                    word = "-" + wordRun.MatchedWord + "-";
                            }
                        }
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

        public string SentenceFromWordsNoSpaces
        {
            get
            {
                int c = SentenceWordCount;
                int i;
                StringBuilder sb = new StringBuilder();
                for (i = 0; i < c; i++)
                {
                    WordRun wordRun = GetWordRun(i);
                    string word;
                    if (wordRun != null)
                    {
                        if (wordRun.WordSpanCount != 0)
                        {
                            if (String.IsNullOrEmpty(wordRun.MatchedWord))
                            {
                                if (wordRun.Matched)
                                    word = wordRun.Word;
                                else
                                    word = "-" + wordRun.Word + "-";
                            }
                            else
                            {
                                if (wordRun.Matched)
                                    word = wordRun.MatchedWord;
                                else
                                    word = "-" + wordRun.MatchedWord + "-";
                            }
                        }
                        else
                            word = "*";
                    }
                    else
                        word = "(null)";

                    sb.Append(word);
                }
                return sb.ToString();
            }
        }

#if false
        // If used, these will need updating for SentenceWordRuns.

        public void SplitWord(
            int wordIndex,
            string newWord1,
            string separator,
            string newWord2,
            List<SentenceRun> sentenceRuns,
            ref string parsedText)
        {
            WordRun wordRun1 = GetWordRun(wordIndex);
            parsedText = parsedText.Remove(wordRun1.StartIndex, wordRun1.Length);
            parsedText = parsedText.Insert(wordRun1.StartIndex, newWord1 + separator + newWord2);
            int startIndex1 = wordRun1.StartIndex;
            int oldLength = wordRun1.Length;
            int stopIndex1 = startIndex1 + newWord1.Length;
            int startIndex2 = stopIndex1 + separator.Length;
            int stopIndex2 = startIndex2 + newWord2.Length;
            int newLength = newWord1.Length + separator.Length + newWord2.Length;
            int deltaLength = newLength - oldLength;
            TimeSpan startTime1 = wordRun1.StartTime;
            TimeSpan stopTime2 = wordRun1.StopTime;
            TimeSpan stopTime1 = new TimeSpan((startTime1.Ticks + stopTime2.Ticks) / 2);
            TimeSpan startTime2 = stopTime1;
            WordRun newWordRun = new WordRun(
                newWord2,
                startIndex2,
                stopIndex2,
                startTime2,
                stopTime2,
                wordIndex + 1);
            wordRun1.Word = newWord1;
            wordRun1.StopIndex = stopIndex1;
            wordRun1.StopTime = stopTime1;
            ParentWordRuns.Insert(StartWordIndexInParent + wordIndex + 1, newWordRun);
            StopWordIndexInParent++;
            SentenceTextStopIndex += deltaLength;
            ApplyTextOffset(wordIndex + 2, deltaLength);
            SentenceText = parsedText.Substring(SentenceTextStartIndex, SentenceTextStopIndex - SentenceTextStartIndex);
            foreach (SentenceRun parsedSentenceRun in sentenceRuns)
                parsedSentenceRun.ParentText = parsedText;
        }

        public void JoinWord(
            int wordIndex,
            string newWord,
            List<SentenceRun> sentenceRuns,
            ref string parsedText)
        {
            WordRun wordRun1 = GetWordRun(wordIndex);
            WordRun wordRun2 = GetWordRun(wordIndex + 1);
            if (wordRun2 == null)
                return;
            parsedText = parsedText.Remove(wordRun1.StartIndex, wordRun2.StopIndex - wordRun1.StartIndex);
            parsedText = parsedText.Insert(wordRun1.StartIndex, newWord);
            int oldLength = wordRun2.StopIndex - wordRun1.StartIndex;
            int newLength = newWord.Length;
            int deltaLength = newLength - oldLength;
            int startIndex = wordRun1.StartIndex;
            int stopIndex = startIndex + newLength;
            TimeSpan startTime = wordRun1.StartTime;
            TimeSpan stopTime = wordRun2.StopTime;
            wordRun1.Word = newWord;
            wordRun1.StopIndex = stopIndex;
            wordRun1.StopTime = stopTime;
            ParentWordRuns.RemoveAt(StartWordIndexInParent + wordIndex + 1);
            StopWordIndexInParent--;
            SentenceTextStopIndex += deltaLength;
            ApplyTextOffset(wordIndex + 1, deltaLength);
            SentenceText = parsedText.Substring(SentenceTextStartIndex, SentenceTextStopIndex - SentenceTextStartIndex);
            foreach (SentenceRun parsedSentenceRun in sentenceRuns)
                parsedSentenceRun.ParentText = parsedText;
        }

        public void ApplyTextOffset(
            int wordIndexStart,
            int deltaOffset)
        {
            int wordCount = ParentWordRuns.Count();

            for (int wordIndex = wordIndexStart + StartWordIndexInParent; wordIndex < wordCount; wordIndex++)
            {
                WordRun wordRun = ParentWordRuns[wordIndex];
                wordRun.StartIndex += deltaOffset;
                wordRun.StopIndex += deltaOffset;
                wordRun.WordIndex = wordIndex;
            }
        }
#endif

        public TimeSpan GetLastWordStartTime()
        {
            WordRun wordRun = GetWordRun(SentenceWordCount - 1);

            if (wordRun != null)
                return wordRun.StartTime;

            return TimeSpan.Zero;
        }

        public TimeSpan GetLastWordStopTime()
        {
            WordRun wordRun = GetWordRun(SentenceWordCount - 1);

            if (wordRun != null)
                return wordRun.StopTime;

            return TimeSpan.Zero;
        }

        public TimeSpan GetNextWordStartTime()
        {
            if ((ParentWordRuns != null) && (StopWordIndexInParent > StartWordIndexInParent) &&
                    (StopWordIndexInParent < ParentWordRuns.Count()))
                return ParentWordRuns[StopWordIndexInParent].StartTime;

            return TimeSpan.Zero;
        }

        public TimeSpan GetNextWordStopTime()
        {
            if ((ParentWordRuns != null) && (StopWordIndexInParent > StartWordIndexInParent) &&
                    (StopWordIndexInParent < ParentWordRuns.Count()))
                return ParentWordRuns[StopWordIndexInParent].StopTime;

            return TimeSpan.Zero;
        }

        public bool HasValidSentenceTimes()
        {
            if ((SentenceStartTime != SentenceStopTime) &&
                    (SentenceStartTime != TimeSpan.Zero) &&
                    (SentenceStopTime != TimeSpan.Zero) &&
                    (SentenceStartTime < SentenceStopTime))
                return true;

            return false;
        }

        public bool IsOverlapsTime(SentenceRun other)
        {
            if ((other.SentenceStartTime >= SentenceStartTime) && (other.SentenceStartTime <= SentenceStopTime))
                return true;

            if ((other.SentenceStopTime >= SentenceStartTime) && (other.SentenceStopTime <= SentenceStopTime))
                return true;

            if ((other.SentenceStartTime <= SentenceStartTime) && (other.SentenceStopTime >= SentenceStartTime))
                return true;

            if ((other.SentenceStartTime <= SentenceStopTime) && (other.SentenceStopTime >= SentenceStopTime))
                return true;

            return false;
        }

        public static bool GetStudySentencesAndWordRuns(
            List<MultiLanguageItem> studyItems,
            LanguageID languageID,
            string mediaItemKey,
            string languageMediaItemKey,
            int studyItemStartIndex,
            int sentenceStartIndex,
            out List<SentenceRun> studySentenceRuns,
            out List<WordRun> studyWordRuns)
        {
            studySentenceRuns = new List<SentenceRun>();
            studyWordRuns = WordRun.GetStudyRuns(
                studyItems,
                languageID,
                mediaItemKey,
                languageMediaItemKey,
                studyItemStartIndex,
                sentenceStartIndex);

            if (studyItems == null)
                return false;

            int studyWordIndex;
            int studyWordCount = studyWordRuns.Count();
            int studySentenceStartIndex = 0;
            int lastStudyItemIndex = -1;
            int lastSentenceIndex = -1;
            SentenceRun studySentenceRun = null;
            int parentSentenceIndex = 0;

            for (studyWordIndex = 0; studyWordIndex < studyWordCount; studyWordIndex++)
            {
                WordRun studyWordRun = studyWordRuns[studyWordIndex];

                if ((lastStudyItemIndex != -1) && 
                    ((studyWordRun.StudyItemIndex != lastStudyItemIndex) || (studyWordRun.SentenceIndex != lastSentenceIndex)))
                {
                    WordRun lastStudyWordRun = studyWordRuns[studyWordIndex - 1];
                    MultiLanguageItem studyItem = studyItems[lastStudyWordRun.StudyItemIndex];
                    LanguageItem languageItem = studyItem.LanguageItem(languageID);

                    if (languageItem == null)
                        throw new Exception("GetStudySentencesAndWordRuns: LanguageItem is null.");

                    TextRun sentenceRun = languageItem.GetSentenceRun(lastStudyWordRun.SentenceIndex);

                    if (sentenceRun == null)
                        throw new Exception("GetStudySentencesAndWordRuns: sentenceRun is null.");

                    studySentenceRun = new SentenceRun(
                        languageItem.Text,
                        languageItem.GetRunText(sentenceRun),
                        sentenceRun.Start,
                        sentenceRun.Stop,
                        studyWordRuns,
                        studySentenceStartIndex,
                        studyWordIndex,
                        TimeSpan.Zero,
                        TimeSpan.Zero,
                        lastStudyWordRun.StudyItemIndex,
                        studyItem,
                        lastStudyWordRun.SentenceIndex,
                        sentenceRun,
                        parentSentenceIndex);

                    studySentenceRuns.Add(studySentenceRun);
                    parentSentenceIndex++;
                    studySentenceStartIndex = studyWordIndex;
                }

                lastStudyItemIndex = studyWordRun.StudyItemIndex;
                lastSentenceIndex = studyWordRun.SentenceIndex;
            }

            if (studyWordIndex > 0)
            {
                WordRun lastStudyWordRun = studyWordRuns[studyWordIndex - 1];
                MultiLanguageItem studyItem = studyItems[lastStudyWordRun.StudyItemIndex];
                LanguageItem languageItem = studyItem.LanguageItem(languageID);

                if (languageItem == null)
                    throw new Exception("GetStudySentencesAndWordRuns: LanguageItem is null.");

                TextRun sentenceRun = languageItem.GetSentenceRun(lastStudyWordRun.SentenceIndex);

                if (sentenceRun == null)
                    throw new Exception("GetStudySentencesAndWordRuns: sentenceRun is null.");

                studySentenceRun = new SentenceRun(
                    languageItem.Text,
                    languageItem.GetRunText(sentenceRun),
                    sentenceRun.Start,
                    sentenceRun.Stop,
                    studyWordRuns,
                    studySentenceStartIndex,
                    studyWordIndex,
                    TimeSpan.Zero,
                    TimeSpan.Zero,
                    lastStudyWordRun.StudyItemIndex,
                    studyItem,
                    lastStudyWordRun.SentenceIndex,
                    sentenceRun,
                    parentSentenceIndex);

                studySentenceRuns.Add(studySentenceRun);
            }

            return true;
        }

        public static string GetCombinedText(List<SentenceRun> sentenceRuns)
        {
            StringBuilder sb = new StringBuilder();

            foreach (SentenceRun sentenceRun in sentenceRuns)
                sb.Append(sentenceRun.SentenceText);

            return sb.ToString();
        }
    }
}
