using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    public enum PunctuationFamily
    {
        Periods = 0,
        Colons = 1,
        Semicolons = 2,
        Dashes = 3,
        QuestionNarks = 4,
        Exclamations = 5
    }

    public class PunctuationCounter
    {
        // Constants
        public static char[] PunctuationChars = new char[] { '.', '。', '．', ':', '：', ';', '；', '-', '–', '－', '—', '?', '？', '!', '！' };
        public static string[] PunctuationAll = new string[] { ".", "。", "．", ": ", "：", ";", "；", "--", " – ", "－", "—", "?", "？", "!", "！" };
        public static string[] PunctuationPeriods = new string[] { ".", "。", "．" };
        public static string[] PunctuationColons = new string[] { ": ", "：" };
        public static string[] PunctuationSemicolons = new string[] { ";", "；" };
        public static string[] PunctuationDashes = new string[] { "--", " – ", "－", "—" };
        public static string[] PunctuationQuestionNarks = new string[] { "?", "？" };
        public static string[] PunctuationExclamations = new string[] { "!", "！" };
        public const int PunctuationFamilyCount = 6;
        public static string[][] PunctuationFamilies = new string[PunctuationFamilyCount][]
            {
                PunctuationPeriods,
                PunctuationColons,
                PunctuationSemicolons,
                PunctuationDashes,
                PunctuationQuestionNarks,
                PunctuationExclamations
            };
        public static string[] PunctuationFamilyNames =
            {
                "Periods",
                "Colons",
                "Semicolons",
                "Dashes",
                "QuestionNarks",
                "Exclamations",
            };
        public static PunctuationFamily[][] PunctuationCompatibility = new PunctuationFamily[PunctuationFamilyCount][]
            {
                new PunctuationFamily[] { PunctuationFamily.Periods, PunctuationFamily.Colons, PunctuationFamily.Semicolons, PunctuationFamily.Dashes },
                new PunctuationFamily[] { PunctuationFamily.Colons, PunctuationFamily.Periods, PunctuationFamily.Semicolons, PunctuationFamily.Dashes },
                new PunctuationFamily[] { PunctuationFamily.Semicolons, PunctuationFamily.Periods, PunctuationFamily.Colons, PunctuationFamily.Dashes },
                new PunctuationFamily[] { PunctuationFamily.Dashes, PunctuationFamily.Periods, PunctuationFamily.Colons, PunctuationFamily.Semicolons },
                new PunctuationFamily[] { PunctuationFamily.QuestionNarks },
                new PunctuationFamily[] { PunctuationFamily.Exclamations }
            };


        // Object data.
        public string Key;
        public int[] PunctuationFamilyCounts;
        public Dictionary<string, int> PunctuationCounts;
        public List<PunctuationFamily> PunctuationOrder;

        public PunctuationCounter(string key, string text, LanguageID languageID)
        {
            ClearPunctuationCounter();
            Key = key;
            CountPunctuation(text, languageID);
        }

        public PunctuationCounter(string key)
        {
            ClearPunctuationCounter();
            Key = key;
        }

        public PunctuationCounter(PunctuationCounter other)
        {
            CopyPunctuationCounter(other);
        }

        public PunctuationCounter()
        {
            ClearPunctuationCounter();
        }

        public void ClearPunctuationCounter()
        {
            Key = null;
            PunctuationFamilyCounts = new int[PunctuationFamilyCount];
            PunctuationCounts = new Dictionary<string, int>();
            PunctuationOrder = new List<PunctuationFamily>();
            ClearCounts();
        }

        public void CopyPunctuationCounter(PunctuationCounter other)
        {
            Key = other.Key;
            PunctuationFamilyCounts = new int[PunctuationFamilyCount];
            Array.Copy(other.PunctuationFamilyCounts, PunctuationFamilyCounts, PunctuationFamilyCount);
            PunctuationCounts = new Dictionary<string, int>(other.PunctuationCounts);
            PunctuationOrder = new List<PunctuationFamily>(other.PunctuationOrder);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Key + "(" + GetSumOfCurrentCounts().ToString() + "): ");
            sb.Append(CountString());
            sb.Append("; ");
            sb.Append(OrderString());

            return sb.ToString();
        }

        public string CountString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("PunctuationCounts = ");

            for (int i = 0; i < PunctuationFamilyCount; i++)
            {
                if (i != 0)
                    sb.Append(", ");

                sb.Append(PunctuationFamilyNames[i] + ": " + PunctuationFamilyCounts[i].ToString());
            }

            return sb.ToString();
        }

        public string OrderString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("PunctuationOrder = ");

            for (int i = 0; i < PunctuationOrder.Count(); i++)
            {
                if (i != 0)
                    sb.Append(", ");

                sb.Append(PunctuationFamilyNames[(int)PunctuationOrder[i]]);
            }

            return sb.ToString();
        }

        public void ClearCounts()
        {
            for (int i = 0; i < PunctuationFamilyCount; i++)
                PunctuationFamilyCounts[i] = 0;

            PunctuationCounts.Clear();

            foreach (string[] family in PunctuationFamilies)
            {
                foreach (string punct in family)
                    PunctuationCounts.Add(punct, 0);
            }
        }

        public void CountPunctuation(string text, LanguageID languageID)
        {
            if (String.IsNullOrEmpty(text))
                return;

            bool containsPeriod = (text.IndexOf('.') != -1);

            int textLength = text.Length;
            int textIndex;

            for (textIndex = 0; textIndex < textLength;)
            {
                char chr = text[textIndex];

                if (!PunctuationChars.Contains(chr))
                {
                    textIndex++;
                    continue;
                }

                if (chr == '.')
                {
                    int skipOffset;

                    if (LanguageTool.CheckForAbbreviationLanguage(text, languageID, textIndex, '.', out skipOffset))
                    {
                        textIndex += skipOffset;
                        continue;
                    }
                }

                bool found = false;

                for (int familyIndex = 0; familyIndex < PunctuationFamilyCount; familyIndex++)
                {
                    string[] familyStrings = PunctuationFamilies[familyIndex];

                    foreach (string punct in familyStrings)
                    {
                        int punctLength = punct.Length;

                        if (String.Compare(text, textIndex, punct, 0, punctLength) != 0)
                            continue;

                        PunctuationCounts[punct] += 1;
                        PunctuationFamilyCounts[familyIndex] += 1;
                        PunctuationOrder.Add((PunctuationFamily)familyIndex);

                        // Skip any sentence break characters that immediately follow.
                        for (; textIndex < textLength; textIndex++)
                        {
                            chr = text[textIndex];

                            if (!PunctuationChars.Contains(chr))
                                break;
                        }

                        textIndex += punctLength;
                        found = true;
                        break;
                    }

                    if (found)
                        break;
                }

                if (!found)
                    textIndex++;
            }
        }

        public static int CountTotalPunctuation(string text, LanguageID languageID)
        {
            int totalCount = 0;

            if (String.IsNullOrEmpty(text))
                return totalCount;

            bool containsPeriod = (text.IndexOf('.') != -1);

            foreach (string punct in PunctuationAll)
            {
                int count = TextUtilities.CountStrings(text, punct);

                // Handle abbreviations.
                if (containsPeriod && (punct == "."))
                {
                    int charCount = text.Length;
                    int charIndex = text.IndexOf('.');

                    for (; (charIndex != -1) && (charIndex < charCount); charIndex = text.IndexOf('.', charIndex + 1))
                    {
                        int skipOffset;

                        if (LanguageTool.CheckForAbbreviationLanguage(text, languageID, charIndex, '.', out skipOffset))
                        {
                            string abbrevFragment = text.Substring(charIndex, skipOffset);
                            int periodCount = TextUtilities.CountStrings(abbrevFragment, punct);
                            count -= periodCount;   // Remove abbreviation period count from count.
                        }
                    }
                }

                totalCount += count;
            }

            return totalCount;
        }

        public int GetSumOfCurrentCounts()
        {
            int count = 0;

            foreach (int c in PunctuationFamilyCounts)
                count += c;

            return count;
        }

        public string[] GetDelimiters()
        {
            List<string> delimiters = new List<string>();

            foreach (KeyValuePair<string, int> kvp in PunctuationCounts)
            {
                if (kvp.Value != 0)
                    delimiters.Add(kvp.Key);
            }

            return delimiters.ToArray();
        }

        // Additive.
        public static void GetSentencePunctuationStatistics(
            string sentence,
            string[] delimiters,
            LanguageID languageID,
            ref string unusedDelimiters,
            ref int unusedDelimiterCount,
            ref int usedDelimiters)
        {
            PunctuationCounter counter = new PunctuationCounter(null, sentence, languageID);
            List<string> unused = new List<string>();

            unusedDelimiters = String.Empty;

            foreach (string delimiter in PunctuationAll)
            {
                int delimiterCount = counter.PunctuationCounts[delimiter];

                if (delimiterCount != 0)
                    unused.Add(delimiter);

                unusedDelimiterCount += delimiterCount;
            }

            for (int i = 0; i < sentence.Length; i++)
            {
                foreach (string delimiter in unused)
                {
                    if (String.CompareOrdinal(sentence, i, delimiter, 0, delimiter.Length) == 0)
                        unusedDelimiters += delimiter;
                }
            }
        }

        // Additive.
        public static void GetTargetHostStatistics(
            string targetSentence,
            string hostSentence,
            string[] delimiters,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            bool isIgnoreLastpunctuation,
            ref string unusedTargetDelimiters,
            ref string unusedHostDelimiters,
            ref int unusedTargetDelimiterCount,
            ref int unusedHostDelimiterCount,
            ref int usedTargetDelimiters,
            ref int usedHostDelimiters,
            ref int delimiterDifference,
            ref int targetLength,
            ref int hostLength,
            ref double lengthRatio)
        {
            if (isIgnoreLastpunctuation)
            {
                GetSentencePunctuationStatistics(TextUtilities.RemovePunctuationFromStartAndEnd(targetSentence), delimiters, targetLanguageID, ref unusedTargetDelimiters, ref unusedTargetDelimiterCount, ref usedTargetDelimiters);
                GetSentencePunctuationStatistics(TextUtilities.RemovePunctuationFromStartAndEnd(hostSentence), delimiters, hostLanguageID, ref unusedHostDelimiters, ref unusedHostDelimiterCount, ref usedHostDelimiters);
            }
            else
            {
                GetSentencePunctuationStatistics(targetSentence, delimiters, targetLanguageID, ref unusedTargetDelimiters, ref unusedTargetDelimiterCount, ref usedTargetDelimiters);
                GetSentencePunctuationStatistics(hostSentence, delimiters, hostLanguageID, ref unusedHostDelimiters, ref unusedHostDelimiterCount, ref usedHostDelimiters);
            }

            delimiterDifference = unusedTargetDelimiterCount - unusedHostDelimiterCount;

            targetLength = targetSentence.Length;
            hostLength = hostSentence.Length;

            if (hostLength != 0)
                lengthRatio = (double)targetLength / (double)hostLength;
            else
                lengthRatio = 0.0;
        }

        // Get the delimiters used that either have the same total delimiter count, or that
        // have matching family counts over the counters.
        public static string[] GetUsedOrCommonDelimiters(List<PunctuationCounter> counters)
        {
            if (IsEqualTotalCount(counters))
                return GetUnionOfDelimiters(counters);  // Gets union of all delimiters used.
            else
                return GetCommonDelimiters(counters);   // Gets only those delimiters with matching counts.
        }

        // Get the union of delimiters used.
        public static string[] GetUnionOfDelimiters(List<PunctuationCounter> counters)
        {
            if ((counters == null) || (counters.Count() == 0))
                return new string[0];

            if (counters.Count() == 1)
                return counters[0].GetDelimiters();

            List<string> delimiters = new List<string>();

            foreach (PunctuationCounter counter in counters)
                ObjectUtilities.ListAddUniqueList(delimiters, counter.GetDelimiters().ToList());

            return delimiters.ToArray();
        }

        // Get the union of delimiters used.
        public static string[] GetUnionOfDelimiters(PunctuationCounter counter1, PunctuationCounter counter2)
        {

            List<string> delimiters = counter1.GetDelimiters().ToList();

            ObjectUtilities.ListAddUniqueList(delimiters, counter2.GetDelimiters().ToList());

            return delimiters.ToArray();
        }

        // Get the delimiters used that have matching family counts over the counters.
        public static string[] GetCommonDelimiters(List<PunctuationCounter> counters)
        {
            if ((counters == null) || (counters.Count() == 0))
                return new string[0];

            if (counters.Count() == 1)
                return counters[0].GetDelimiters();

            int counterCount = counters.Count();
            int counterIndex;
            List<string> delimiters = new List<string>();

            for (int familyIndex = 0; familyIndex < PunctuationFamilyCount; familyIndex++)
            {
                bool isMatch = false;

                int firstCount = counters[0].PunctuationFamilyCounts[familyIndex];

                if (firstCount != 0)
                {
                    isMatch = true;

                    for (counterIndex = 1; counterIndex < counterCount; counterIndex++)
                    {
                        PunctuationCounter counter = counters[counterIndex];

                        if (counter.PunctuationFamilyCounts[familyIndex] != firstCount)
                        {
                            isMatch = false;
                            break;
                        }
                    }
                }

                if (isMatch)
                {
                    string[] punctuationStrings = PunctuationFamilies[familyIndex];

                    foreach (string punct in punctuationStrings)
                    {
                        bool hasCount = false;

                        for (counterIndex = 0; counterIndex < counterCount; counterIndex++)
                        {
                            PunctuationCounter counter = counters[counterIndex];

                            if (counter.PunctuationCounts[punct] != 0)
                                hasCount = true;
                        }

                        if (hasCount)
                            delimiters.Add(punct);
                    }
                }
            }

            return delimiters.ToArray();
        }

        // Get the delimiters used that have matching family counts over the counters.
        public static string[] GetCommonDelimiters(PunctuationCounter counter1, PunctuationCounter counter2)
        {
            List<PunctuationCounter> counters = new List<PunctuationCounter>(2) { counter1, counter2 };
            return GetCommonDelimiters(counters);
        }

        // Get the delimiters used that have matching family counts over the counters.
        public static bool IsEqualTotalCount(List<PunctuationCounter> counters)
        {
            if ((counters == null) || (counters.Count() <= 1))
                return true;

            int counterCount = counters.Count();
            int counterIndex;
            int count = counters[0].GetSumOfCurrentCounts();

            for (counterIndex = 1; counterIndex < counterCount; counterIndex++)
            {
                if (counters[counterIndex].GetSumOfCurrentCounts() != count)
                    return false;
            }

            return true;
        }

        // Compare orders of delimiters.
        public static bool CompareDelimiterOrder(PunctuationCounter counter1, PunctuationCounter counter2)
        {
            int count1 = counter1.PunctuationOrder.Count();
            int count2 = counter2.PunctuationOrder.Count();
            bool returnValue = true;

            if (count1 != count2)
                returnValue = false;
            else
            {
                for (int i = 0; i < count1; i++)
                {
                    if (counter1.PunctuationOrder[i] != counter2.PunctuationOrder[i])
                    {
                        returnValue = false;
                        break;
                    }
                }
            }

            return returnValue;
        }

        // Compare compatible orders of delimiters.
        public static bool CompareCompatibleDelimiterOrder(PunctuationCounter counter1, PunctuationCounter counter2)
        {
            int count1 = counter1.PunctuationOrder.Count();
            int count2 = counter2.PunctuationOrder.Count();
            bool returnValue = true;

            if (count1 != count2)
                returnValue = false;
            else
            {
                for (int i = 0; i < count1; i++)
                {
                    if (counter1.PunctuationOrder[i] != counter2.PunctuationOrder[i])
                    {
                        PunctuationFamily family1 = counter1.PunctuationOrder[i];
                        PunctuationFamily family2 = counter2.PunctuationOrder[i];

                        if (!PunctuationCompatibility[(int)family1].Contains(family2))
                        {
                            returnValue = false;
                            break;
                        }
                    }
                }
            }

            return returnValue;
        }
    }
}
