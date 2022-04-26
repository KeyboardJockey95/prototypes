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
    public class WordRun
    {
        public string Word;
        public List<string> CombinedWords;
        public string MatchedWord;
        public int StartIndex;
        public int StopIndex;
        public int Length { get { return StopIndex - StartIndex; } }
        public int WordLength;
        public int StartIndexNoSeparators;
        public int StopIndexNoSeparators;
        public TimeSpan StartTime;
        public TimeSpan StopTime;
        public TimeSpan MiddleTime { get { return new TimeSpan((StartTime.Ticks + StopTime.Ticks) / 2); } }
        public int StudyItemIndex;
        public int SentenceIndex;
        public int WordIndex;
        public int WordIndexOther;
        public WordRun OtherWordRun;
        public int WordSpanCount;    // Number of words in combined word span.
        public int SentenceWordCount;
        public int FinalSentenceWordCount;
        public bool Used;
        public bool Matched;
        public int EditDistance;
        public double EditDistanceFactor;
        public double WordPositionFactor;
        public double WordMatchScore;
        public double EditDistanceScore;
        public double SingleWordEditDistanceScore;

        public WordRun(
            string word,
            int startIndex,
            int stopIndex,
            int startIndexNoSeparators,
            int stopIndexNoSeparators,
            TimeSpan startTime,
            TimeSpan stopTime,
            int studyItemIndex,
            int sentenceIndex,
            int wordIndex,
            int wordIndexOther,
            bool used,
            bool matched)
        {
            Word = word;
            CombinedWords = null;
            MatchedWord = String.Empty;
            StartIndex = startIndex;
            StopIndex = stopIndex;
            WordLength = Word.Length;
            StartIndexNoSeparators = startIndexNoSeparators;
            StopIndexNoSeparators = stopIndexNoSeparators;
            StartTime = startTime;
            StopTime = stopTime;
            StudyItemIndex = studyItemIndex;
            SentenceIndex = sentenceIndex;
            WordIndex = wordIndex;
            WordIndexOther = wordIndexOther;
            OtherWordRun = null;
            WordSpanCount = 1;
            SentenceWordCount = 0;
            FinalSentenceWordCount = 0;
            Used = used;
            Matched = matched;
            EditDistance = 0;
            EditDistanceFactor = 0.0;
            WordPositionFactor = 0.0;
            WordMatchScore = 0.0;
            EditDistanceScore = 0.0;
            SingleWordEditDistanceScore = 0.0;
        }

        public WordRun(
            string word,
            int startIndex,
            int stopIndex,
            TimeSpan startTime,
            TimeSpan stopTime,
            int wordIndex)
        {
            Word = word;
            CombinedWords = null;
            MatchedWord = String.Empty;
            StartIndex = startIndex;
            StopIndex = stopIndex;
            WordLength = Word.Length;
            StartIndexNoSeparators = 0;
            StopIndexNoSeparators = 0;
            StartTime = startTime;
            StopTime = stopTime;
            StudyItemIndex = -1;
            SentenceIndex = -1;
            WordIndex = wordIndex;
            WordIndexOther = -1;
            OtherWordRun = null;
            WordSpanCount = 1;
            SentenceWordCount = 0;
            FinalSentenceWordCount = 0;
            Used = false;
            Matched = false;
            EditDistance = 0;
            EditDistanceFactor = 0.0;
            WordPositionFactor = 0.0;
            WordMatchScore = 0.0;
            EditDistanceScore = 0.0;
            SingleWordEditDistanceScore = 0.0;
        }

        public WordRun(
            WordRun studyRun,
            WordRun parsedRun)
        {
            Word = studyRun.Word;
            CombinedWords = null;
            StartIndex = studyRun.StartIndex;
            StopIndex = studyRun.StopIndex;
            WordLength = Word.Length;
            StartIndexNoSeparators = studyRun.StartIndexNoSeparators;
            StopIndexNoSeparators = studyRun.StopIndexNoSeparators;
            StartTime = parsedRun.StartTime;
            StopTime = parsedRun.StopTime;
            StudyItemIndex = studyRun.StudyItemIndex;
            SentenceIndex = studyRun.SentenceIndex;
            WordIndex = studyRun.WordIndex;
            WordIndexOther = parsedRun.WordIndex;
            OtherWordRun = null;
            WordSpanCount = studyRun.WordSpanCount;
            SentenceWordCount = 0;
            FinalSentenceWordCount = 0;
            Used = studyRun.Used && parsedRun.Used;
            Matched = studyRun.Matched && parsedRun.Matched;
            EditDistance = studyRun.EditDistance;
            EditDistanceFactor = studyRun.EditDistanceFactor;
            WordPositionFactor = studyRun.WordPositionFactor;
            WordMatchScore = studyRun.WordMatchScore;
            EditDistanceScore = studyRun.EditDistanceScore;
            SingleWordEditDistanceScore = studyRun.SingleWordEditDistanceScore;
        }

        public WordRun(WordRun other)
        {
            Word = other.Word;
            if (other.CombinedWords != null)
                CombinedWords = new List<string>(other.CombinedWords);
            else
                CombinedWords = null;
            MatchedWord = other.MatchedWord;
            StartIndex = other.StartIndex;
            StopIndex = other.StopIndex;
            WordLength = other.WordLength;
            StartIndexNoSeparators = other.StartIndexNoSeparators;
            StopIndexNoSeparators = other.StopIndexNoSeparators;
            StartTime = other.StartTime;
            StopTime = other.StopTime;
            StudyItemIndex = other.StudyItemIndex;
            SentenceIndex = other.SentenceIndex;
            WordIndex = other.WordIndex;
            WordIndexOther = other.WordIndex;
            OtherWordRun = other.OtherWordRun;
            WordSpanCount = other.WordSpanCount;
            SentenceWordCount = other.SentenceWordCount;
            FinalSentenceWordCount = other.FinalSentenceWordCount;
            Used = other.Used;
            Matched = other.Matched;
            EditDistance = other.EditDistance;
            EditDistanceFactor = other.EditDistanceFactor;
            WordPositionFactor = other.WordPositionFactor;
            WordMatchScore = other.WordMatchScore;
            EditDistanceScore = other.EditDistanceScore;
            SingleWordEditDistanceScore = other.SingleWordEditDistanceScore;
        }

        public void ClearDataFields()
        {
            if (CombinedWords != null)
                CombinedWords.Clear();
            OtherWordRun = null;
            WordIndexOther = -1;
            WordSpanCount = 1;
            Used = false;
            Matched = false;
            EditDistance = 0;
            EditDistanceFactor = 0.0;
            WordPositionFactor = 0.0;
            WordMatchScore = 0.0;
            EditDistanceScore = 0.0;
            SingleWordEditDistanceScore = 0.0;
        }

        public string GetCombinedWord(int wordCount, SentenceRun sentenceRun)
        {
            if (wordCount == 1)
                return Word;
            else if (wordCount > 1)
            {
                int index = wordCount - 1;

                if (CombinedWords == null)
                    CombinedWords = new List<string>();

                if (index >= CombinedWords.Count())
                {
                    for (int i = 0; i <= index; i++)
                    {
                        if (i >= CombinedWords.Count())
                        {
                            string combinedWord = sentenceRun.GetWordSpanNoSpace(WordIndex, i + 1);
                            CombinedWords.Add(combinedWord);
                        }
                    }
                }

                return CombinedWords[index];
            }

            return String.Empty;
        }

        public override string ToString()
        {
            return Word
                + " "
                + "MatchedWord=" + MatchedWord
                + " "
                + "Used=" + (Used ? "true" : "false")
                + " "
                + "Matched=" + (Matched ? "true" : "false")
                + " "
                + "WordIndex=" + WordIndex.ToString()
                + " "
                + "WordSpanCount=" + WordSpanCount.ToString()
                + " "
                + "WordMatchScore=" + WordMatchScore.ToString()
                + " "
                + "SentenceIndex=" + SentenceIndex.ToString()
                + " "
                + "StudyItemIndex=" + StudyItemIndex.ToString()
                + " "
                + "StartIndex=" + StartIndex.ToString()
                + " "
                + "StopIndex=" + StopIndex.ToString()
                + " "
                + "StartIndexNoSeparators=" + StartIndexNoSeparators.ToString()
                + " "
                + "StopIndexNoSeparators=" + StopIndexNoSeparators.ToString()
                + " "
                + "StartTime=" + StartTime.ToString()
                + " "
                + "StopTime=" + StopTime.ToString();
        }

        public static List<WordRun> GetStudyRuns(
            List<MultiLanguageItem> studyItems,
            LanguageID languageID,
            string mediaItemKey,
            string languageMediaItemKey,
            int studyItemStartIndex,
            int sentenceStartIndex)
        {
            List<WordRun> studyRuns = new List<WordRun>();

            if (studyItems == null)
                return studyRuns;

            int studyItemCount = studyItems.Count();
            int studyItemIndex = 0;
            int sentenceIndex;
            int startSentenceIndex = sentenceStartIndex;
            int wordIndex;
            int wordStartIndex;
            int wordStopIndex;
            TimeSpan wordStartTime;
            TimeSpan wordStopTime;
            string word;
            TextRun sentenceRun;
            MediaRun mediaRun;

            for (studyItemIndex = studyItemStartIndex; studyItemIndex < studyItemCount; studyItemIndex++)
            {
                MultiLanguageItem studyItem = studyItems[studyItemIndex];

                if (studyItem.IsNotMapped())
                    continue;

                LanguageItem languageItem = studyItem.LanguageItem(languageID);

                if (languageItem == null)
                    throw new Exception("LanguageItem is null.");

                if (!languageItem.HasText())
                    continue;

                if (languageItem.WordRuns == null)
                    throw new Exception("LanguageItem WordRuns is null.");

                int wordStartIndexNoSeparators = 0;
                int wordStopIndexNoSeparators = 0;

                wordIndex = 0;

                foreach (TextRun wordRun in languageItem.WordRuns)
                {
                    wordStartIndex = wordRun.Start;
                    wordStopIndex = wordRun.Stop;
                    wordStopIndexNoSeparators = wordStartIndexNoSeparators + wordRun.Length;
                    word = languageItem.GetRunText(wordRun).ToLower();

                    languageItem.GetWordSentenceRun(
                        wordStartIndex,
                        wordStopIndex,
                        out sentenceRun,
                        out sentenceIndex);

                    if (startSentenceIndex != 0)
                    {
                        if (sentenceIndex < startSentenceIndex)
                            continue;

                        startSentenceIndex = 0;
                    }

                    if (sentenceRun != null)
                        mediaRun = sentenceRun.GetMediaRunWithKeyAndReferenceKeys(
                            "Audio", mediaItemKey, languageMediaItemKey);
                    else
                        mediaRun = null;

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

                    WordRun studyRun = new WordRun(
                        word,
                        wordStartIndex,
                        wordStopIndex,
                        wordStartIndexNoSeparators,
                        wordStopIndexNoSeparators,
                        wordStartTime,
                        wordStopTime,
                        studyItemIndex,
                        sentenceIndex,
                        wordIndex,
                        -1,
                        false,
                        false);

                    studyRuns.Add(studyRun);

                    wordStartIndexNoSeparators = wordStopIndexNoSeparators;
                    wordIndex++;
                }
            }

            return studyRuns;
        }

        public static List<WordRun> GetParsedRuns(string parsedText, List<TextRun> parsedWordRuns)
        {
            int studyItemIndex = 0;
            int sentenceIndex = 0;
            int wordIndex = 0;
            int wordStartIndex;
            int wordStopIndex;
            int wordStartIndexNoSeparators = 0;
            int wordStopIndexNoSeparators = 0;
            TimeSpan wordStartTime;
            TimeSpan wordStopTime;
            string word;
            MediaRun mediaRun;
            List<WordRun> parsedRuns = new List<WordRun>();

            foreach (TextRun wordRun in parsedWordRuns)
            {
                wordStartIndex = wordRun.Start;
                wordStopIndex = wordRun.Stop;
                wordStopIndexNoSeparators = wordStartIndexNoSeparators + wordRun.Length;
                word = parsedText.Substring(wordStartIndex, wordStopIndex - wordStartIndex).ToLower();

                mediaRun = wordRun.MediaRuns.FirstOrDefault();

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

                WordRun studyRun = new WordRun(
                    word,
                    wordStartIndex,
                    wordStopIndex,
                    wordStartIndexNoSeparators,
                    wordStopIndexNoSeparators,
                    wordStartTime,
                    wordStopTime,
                    studyItemIndex,
                    sentenceIndex,
                    wordIndex,
                    -1,
                    false,
                    false);

                parsedRuns.Add(studyRun);

                wordStartIndexNoSeparators = wordStopIndexNoSeparators;
                wordIndex++;
            }

            return parsedRuns;
        }

        public static void ReplaceWordRuns(
            List<WordRun> wordRuns,
            int startIndex,
            int wordCount,
            string newWord)
        {
            WordRun startRun = wordRuns[startIndex];
            WordRun stopRun = wordRuns[startIndex + wordCount - 1];
            WordRun newRun = new WordRun(
                newWord,
                startRun.StartIndex,
                stopRun.StopIndex,
                startRun.StartIndexNoSeparators,
                stopRun.StopIndexNoSeparators,
                startRun.StartTime,
                stopRun.StopTime,
                startRun.StudyItemIndex,
                startRun.SentenceIndex,
                startRun.WordIndex,
                startRun.WordIndexOther,
                false,
                false);
            wordRuns.RemoveRange(startIndex, wordCount);
            wordRuns.Insert(startIndex, newRun);

            for (int i = startIndex + 1; i < wordRuns.Count(); i++)
            {
                WordRun wordRun = wordRuns[i];
                wordRun.WordIndex = i;
            }
        }

        public static void ReplaceWordRunSpan(
            List<WordRun> dstWordRuns,
            int dstStartIndex,
            int dstWordCount,
            List<WordRun> srcWordRuns,
            int srcStartIndex,
            int srcWordCount)
        {
            int srcWordIndex = srcStartIndex;
            int wordIndex;
            WordRun srcWordRun;
            string srcWord;
            WordRun dstWordRunFirst = dstWordRuns[dstStartIndex];
            WordRun dstWordRunLast = dstWordRuns[dstStartIndex + (dstWordCount - 1)];

            dstWordRuns.RemoveRange(dstStartIndex, dstWordCount);

            for (wordIndex = 0; wordIndex < srcWordCount; wordIndex++)
            {
                srcWordRun = srcWordRuns[srcWordIndex + wordIndex];
                srcWord = srcWordRun.Word;
                WordRun newRun = new WordRun(
                    srcWord,
                    dstWordRunFirst.StartIndex,
                    dstWordRunLast.StopIndex,
                    dstWordRunFirst.StartIndexNoSeparators,
                    dstWordRunLast.StopIndexNoSeparators,
                    dstWordRunFirst.StartTime,
                    dstWordRunLast.StopTime,
                    dstWordRunFirst.StudyItemIndex,
                    dstWordRunFirst.SentenceIndex,
                    dstWordRunFirst.WordIndex + wordIndex,
                    srcWordIndex,
                    false,
                    false);
                dstWordRuns.Insert(dstStartIndex + wordIndex, newRun);
            }

            for (int i = dstStartIndex + srcWordCount; i < dstWordRuns.Count(); i++)
            {
                WordRun wordRun = dstWordRuns[i];
                wordRun.WordIndex = i;
            }
        }

        public static int GetWordRunIndexWithStart(int startTextIndex, List<WordRun> wordRuns, ref int startIndex)
        {
            int index;
            int count = wordRuns.Count();

            for (index = startIndex; index < count; index++)
            {
                WordRun wordRun = wordRuns[index];

                if (wordRun.StartIndex == startTextIndex)
                {
                    startIndex = index + 1;
                    return index;
                }
            }

            startIndex = count;

            return -1;
        }
    }
}
