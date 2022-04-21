using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;

namespace JTLanguageModelsPortable.Converters
{
    public class ConvertCanonical : ConvertBase
    {
        public LanguageID LanguageID { get; set; }
        public bool AddRegex { get; set; }
        public bool IsCharacterBased { get; set; }

        public ConvertCanonical(LanguageID languageID, bool addRegex)
        {
            LanguageID = languageID;
            AddRegex = addRegex;
            IsCharacterBased = LanguageLookup.IsCharacterBased(languageID);
        }

        public virtual bool Canonical(out MatchCode matchTypeOut, out string output, MatchCode matchTypeIn, string input)
        {
            bool returnValue = true;

            matchTypeOut = matchTypeIn;
            output = input;

            switch (LanguageID.LanguageCultureExtensionCode)
            {
                case "zh--pn":
                    returnValue = ConvertPinyinNumeric.Canonical(out output, input, AddRegex);
                    if (AddRegex && output.Contains("[1-5]"))
                    {
                        switch (matchTypeIn)
                        {
                            case MatchCode.Any:
                                break;
                            case MatchCode.Exact:
                                matchTypeOut = MatchCode.RegEx;
                                output = "^" + output + "$";
                                break;
                            case MatchCode.StartsWith:
                                matchTypeOut = MatchCode.RegEx;
                                output = "^" + output + ".*";
                                break;
                            case MatchCode.EndsWith:
                                matchTypeOut = MatchCode.RegEx;
                                output = ".*" + output + "$";
                                break;
                            case MatchCode.Contains:
                            case MatchCode.Fuzzy:
                                matchTypeOut = MatchCode.RegEx;
                                output = ".*" + output + ".*";
                                break;
                            case MatchCode.RegEx:
                            case MatchCode.Or:
                            case MatchCode.And:
                            case MatchCode.Xor:
                            case MatchCode.Not:
                            case MatchCode.Greater:
                            case MatchCode.GreaterOrEqual:
                            case MatchCode.Less:
                            case MatchCode.LessOrEqual:
                            case MatchCode.Between:
                            case MatchCode.Outside:
                            case MatchCode.ParseBest:
                            case MatchCode.ParseAll:
                            case MatchCode.CustomBase:
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        // Returns true if anything changed.
        public static bool NoPunctuation(out string output, string input)
        {
            StringBuilder sb = new StringBuilder();
            int index;
            int count = input.Length;
            char chr;
            bool returnValue = false;

            for (index = 0; index < count; index++)
            {
                chr = input[index];

                if (!Char.IsPunctuation(chr))
                {
                    sb.Append(chr);
                }
                else
                {
                    returnValue = true;

                    while (((index + 1) < count) && Char.IsWhiteSpace(input[index + 1]))
                        index++;
                }
            }

            output = sb.ToString();

            return returnValue;
        }

        public List<string> GetCanonicalWords(string str)
        {
            List<string> words = new List<string>();

            if (str == null)
                return words;

            if (IsCharacterBased)
            {
                string canonicalWord = str.ToLower();

                foreach (char chr in canonicalWord)
                {
                    if (!LanguageLookup.PunctuationCharacters.Contains(chr) && !char.IsWhiteSpace(chr))
                        words.Add(chr.ToString());
                }
            }
            else
            {
                string[] parts = str.Split(LanguageLookup.SpaceCharacters, StringSplitOptions.RemoveEmptyEntries);

                foreach (string word in parts)
                {
                    string canonicalWord = word.ToLower();
                    StringBuilder sb = new StringBuilder();

                    foreach (char chr in canonicalWord)
                    {
                        if (!LanguageLookup.PunctuationCharacters.Contains(chr))
                            sb.Append(chr);
                    }

                    canonicalWord = sb.ToString();
                    words.Add(canonicalWord);
                }
            }

            return words;
        }

        public List<int> GetWordStarts(string str)
        {
            List<int> wordStarts = new List<int>();

            if (str == null)
                return wordStarts;

            int charIndex = 0;
            int charCount = str.Length;

            if (IsCharacterBased)
            {
                string canonicalWord = str.ToLower();

                foreach (char chr in canonicalWord)
                {
                    if (!LanguageLookup.PunctuationCharacters.Contains(chr) && !char.IsWhiteSpace(chr))
                        wordStarts.Add(charIndex++);
                    else
                        charIndex++;
                }
            }
            else
            {
                char chr;

                // Skip leading space.
                if (charIndex < charCount)
                {
                    while (Char.IsWhiteSpace(chr = str[charIndex]))
                    {
                        charIndex++;

                        if (charIndex >= charCount)
                            break;
                    }
                }

                while (charIndex < charCount)
                {
                    wordStarts.Add(charIndex);

                    // Collect word.
                    if (charIndex < charCount)
                    {
                        while (!Char.IsWhiteSpace(chr = str[charIndex]))
                        {
                            charIndex++;

                            if (charIndex >= charCount)
                                break;
                        }
                    }

                    // Skip trailing space.
                    if (charIndex < charCount)
                    {
                        while (Char.IsWhiteSpace(chr = str[charIndex]))
                        {
                            charIndex++;

                            if (charIndex >= charCount)
                                break;
                        }
                    }
                }
            }

            return wordStarts;
        }

        public string FilterAsides(string str)
        {
            return TextUtilities.FilterAsides(str);
        }

        public string GetBlanks(int count)
        {
            string str = String.Empty;

            while (count-- > 0)
                str += "_";

            return str;
        }

        public bool SpreadWords(List<string> patternWords, List<string> inputWords,
            List<int> wordStarts, ref string output)
        {
            bool returnValue = false;

            if (patternWords.Count != inputWords.Count)
            {
                int maxWordCount = (inputWords.Count > patternWords.Count ? inputWords.Count : patternWords.Count);
                string patternWord, inputWord;

                for (int index = 0; index < maxWordCount; index++)
                {
                    if (index < patternWords.Count)
                        patternWord = patternWords[index];
                    else
                        patternWord = String.Empty;

                    if (index < inputWords.Count)
                        inputWord = inputWords[index];
                    else
                        inputWord = String.Empty;

                    if (inputWord != patternWord)
                    {
                        int j1, j2;

                        for (j1 = index + 1; j1 < patternWords.Count; j1++)
                        {
                            if (index < inputWords.Count)
                            {
                                if (patternWords[j1] == inputWords[index])
                                    break;
                            }
                        }

                        for (j2 = index + 1; j2 < inputWords.Count; j2++)
                        {
                            if (index < patternWords.Count)
                            {
                                if (patternWords[index] == inputWords[j2])
                                    break;
                            }
                        }

                        List<string> words = (j1 < j2 ? inputWords : patternWords);
                        List<string> other = (j1 < j2 ? patternWords : inputWords);
                        int j = (j1 < j2 ? j1 : j2);

                        if (j <= words.Count)
                        {
                            int jc = j - index;

                            if (index + jc < other.Count)
                            {
                                int k;

                                for (k = index; k < j; k++)
                                {
                                    int blanksLength = other[k].Length;
                                    string blanks = GetBlanks(blanksLength) + " ";
                                    words.Insert(k, blanks);

                                    if (j1 < j2)
                                    {
                                        int startIndex = wordStarts[k];
                                        output = output.Insert(startIndex, blanks);
                                        wordStarts.Insert(k, startIndex);

                                        for (int m = k + 1; m < wordStarts.Count; m++)
                                            wordStarts[m] = wordStarts[m] + blanksLength + 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            while (patternWords.Count > inputWords.Count)
            {
                string blanks = GetBlanks(patternWords[inputWords.Count].Length);
                inputWords.Add(blanks);
                output += " " + blanks;
            }

            while (inputWords.Count > patternWords.Count)
            {
                string blanks = GetBlanks(inputWords[patternWords.Count].Length);
                patternWords.Add(blanks);
            }

            return returnValue;
        }

        // Returns true if matched.  runs will have length of matched or non-matched text. matchFlags
        // will have true if matched for corresponding run.
        public bool MatchWithDiff(string pattern, string input, List<int> textRuns, List<bool> matchFlags,
            out string output)
        {
            switch (LanguageID.LanguageCultureExtensionCode)
            {
                case "zh--pn":
                    if (!ConvertPinyinNumeric.IsNumeric(pattern) && ConvertPinyinNumeric.IsNumeric(input))
                    {
                        string newInput = input;
                        ConvertPinyinNumeric.ToToneMarks(out input, newInput);
                    }
                    break;
                default:
                    break;
            }

            string patternFiltered = FilterAsides(pattern);
            List<string> patternWords;
            List<string> inputWords;
            if (pattern == patternFiltered)
            {
                patternWords = GetCanonicalWords(pattern);
                inputWords = GetCanonicalWords(FilterAsides(input));
            }
            else
            {
                patternWords = GetCanonicalWords(pattern);
                inputWords = GetCanonicalWords(input);
            }
            int patternCount = patternWords.Count;
            List<int> startIndexes = GetWordStarts(input);
            int inputCount = inputWords.Count;
            int wordIndex;
            string patternWord;
            string inputWord;
            bool wordMatched = true;
            List<bool> matchCharFlags = new List<bool>();
            int runSize = 0;
            char chr;
            bool returnValue = true;

            if (inputCount != patternCount)
            {
                SpreadWords(patternWords, inputWords, startIndexes, ref input);
                patternCount = patternWords.Count;
                inputCount = inputWords.Count;
            }

            int charIndex = 0;
            int charCount = (input != null ? input.Length : 0);

            // Skip leading space.
            if (charIndex < charCount)
            {
                while (Char.IsWhiteSpace(chr = input[charIndex]))
                {
                    matchCharFlags.Add(wordMatched);
                    charIndex++;

                    if (charIndex >= charCount)
                        break;
                }
            }

            for (wordIndex = 0; wordIndex < inputCount; wordIndex++)
            {
                if (wordIndex < patternCount)
                    patternWord = patternWords[wordIndex];
                else
                    patternWord = String.Empty;

                inputWord = inputWords[wordIndex];

                if (inputWord != patternWord)
                {
                    wordMatched = false;
                    returnValue = false;
                }
                else
                    wordMatched = true;

                // Collect word.
                if (charIndex < charCount)
                {
                    while (!Char.IsWhiteSpace(chr = input[charIndex]))
                    {
                        matchCharFlags.Add(wordMatched);
                        charIndex++;

                        if (charIndex >= charCount)
                            break;
                        else if (IsCharacterBased)
                            break;
                    }
                }

                // Skip trailing space.
                if (charIndex < charCount)
                {
                    while (Char.IsWhiteSpace(chr = input[charIndex]))
                    {
                        matchCharFlags.Add(wordMatched);
                        charIndex++;

                        if (charIndex >= charCount)
                            break;
                    }
                }
            }

            while (matchCharFlags.Count < charCount)
            {
                int index = matchCharFlags.Count;

                if ((index < pattern.Length) && (index < charCount) && (input[index] == pattern[index]))
                    matchCharFlags.Add(true);
                else
                    matchCharFlags.Add(false);
            }

            if (matchCharFlags.Count != 0)
            {
                wordMatched = matchCharFlags.First();

                for (charIndex = 0; charIndex < charCount; charIndex++)
                {
                    bool nextCharMatch = matchCharFlags[charIndex];

                    if (wordMatched != nextCharMatch)
                    {
                        textRuns.Add(runSize);
                        matchFlags.Add(wordMatched);
                        wordMatched = nextCharMatch;
                        runSize = 1;
                    }
                    else
                        runSize++;
                }

                if (runSize != 0)
                {
                    textRuns.Add(runSize);
                    matchFlags.Add(wordMatched);
                }
            }

            output = input;

            return returnValue;
        }

        // Returns true if matched.  runs will have length of matched or non-matched text. matchFlags
        // will have true if matched for corresponding run.
        public bool MatchWithDiff(
            MultiLanguageItem referenceStudyItem,
            MultiLanguageString textInputStrings,
            LanguageID languageID,
            List<LanguageDescriptor> languageDescriptors,
            LanguageUtilities languageUtilities,
            List<int> textRuns,
            List<int> matchCodes,
            out string output)
        {
            LanguageString languageString = textInputStrings.LanguageString(languageID);
            LanguageID inputLanguageID = languageID;
            string pattern = String.Empty;
            string input = String.Empty;
            if (languageString == null)
            {
                languageString = textInputStrings.LanguageStringFuzzy(languageID);
            }
            if (languageString != null)
            {
                input = TextUtilities.GetNormalizedString(languageString.Text);
                inputLanguageID = languageString.LanguageID;
            }
            LanguageItem languageItem = null;
            if (referenceStudyItem != null)
            {
                languageItem = referenceStudyItem.LanguageItem(inputLanguageID);
                if (languageItem == null)
                {
                    languageItem = referenceStudyItem.LanguageItemFuzzy(inputLanguageID);
                }
                if (languageItem != null)
                {
                    pattern = TextUtilities.GetNormalizedString(languageItem.Text);
                    pattern = TextUtilities.SubstitutionCheck(pattern, languageID, languageDescriptors);
                }
                else
                {
                    pattern = String.Format(
                        languageUtilities.TranslateUIString("(no text for {0})"),
                        languageID.LanguageName(languageUtilities.UILanguage));
                }
            }
            switch (LanguageID.LanguageCultureExtensionCode)
            {
                case "zh--pn":
                    if (!ConvertPinyinNumeric.IsNumeric(pattern) && ConvertPinyinNumeric.IsNumeric(input))
                    {
                        string newInput = input;
                        ConvertPinyinNumeric.ToToneMarks(out input, newInput);
                    }
                    break;
                default:
                    break;
            }

            string patternFiltered = FilterAsides(pattern);
            List<string> patternWords;
            List<string> inputWords;
            if (pattern == patternFiltered)
            {
                patternWords = GetCanonicalWords(pattern);
                inputWords = GetCanonicalWords(FilterAsides(input));
            }
            else
            {
                patternWords = GetCanonicalWords(pattern);
                inputWords = GetCanonicalWords(input);
            }
            int patternCount = patternWords.Count;
            List<int> startIndexes = GetWordStarts(input);
            int inputCount = inputWords.Count;
            int wordIndex;
            string patternWord;
            string inputWord;
            bool wordMatched = true;
            List<bool> matchCharFlags = new List<bool>();
            int runSize = 0;
            char chr;
            bool returnValue = true;

            if (inputCount != patternCount)
            {
                SpreadWords(patternWords, inputWords, startIndexes, ref input);
                patternCount = patternWords.Count;
                inputCount = inputWords.Count;
            }

            int charIndex = 0;
            int charCount = (input != null ? input.Length : 0);

            // Skip leading space.
            if (charIndex < charCount)
            {
                while (Char.IsWhiteSpace(chr = input[charIndex]))
                {
                    matchCharFlags.Add(wordMatched);
                    charIndex++;

                    if (charIndex >= charCount)
                        break;
                }
            }

            for (wordIndex = 0; wordIndex < inputCount; wordIndex++)
            {
                if (wordIndex < patternCount)
                    patternWord = patternWords[wordIndex];
                else
                    patternWord = String.Empty;

                inputWord = inputWords[wordIndex];

                if (inputWord != patternWord)
                {
                    wordMatched = false;
                    returnValue = false;
                }
                else
                    wordMatched = true;

                // Collect word.
                if (charIndex < charCount)
                {
                    while (!Char.IsWhiteSpace(chr = input[charIndex]))
                    {
                        matchCharFlags.Add(wordMatched);
                        charIndex++;

                        if (charIndex >= charCount)
                            break;
                        else if (IsCharacterBased)
                            break;
                    }
                }

                // Skip trailing space.
                if (charIndex < charCount)
                {
                    while (Char.IsWhiteSpace(chr = input[charIndex]))
                    {
                        matchCharFlags.Add(wordMatched);
                        charIndex++;

                        if (charIndex >= charCount)
                            break;
                    }
                }
            }

            while (matchCharFlags.Count < charCount)
            {
                int index = matchCharFlags.Count;

                if ((index < pattern.Length) && (index < charCount) && (input[index] == pattern[index]))
                    matchCharFlags.Add(true);
                else
                    matchCharFlags.Add(false);
            }

            if (matchCharFlags.Count != 0)
            {
                wordMatched = matchCharFlags.First();

                for (charIndex = 0; charIndex < charCount; charIndex++)
                {
                    bool nextCharMatch = matchCharFlags[charIndex];

                    if (wordMatched != nextCharMatch)
                    {
                        textRuns.Add(runSize);
                        matchCodes.Add(wordMatched ? 1 : 2);
                        wordMatched = nextCharMatch;
                        runSize = 1;
                    }
                    else
                        runSize++;
                }

                if (runSize != 0)
                {
                    textRuns.Add(runSize);
                    matchCodes.Add(wordMatched ? 1 : 2);
                }
            }

            output = input;

            return returnValue;
        }

        public override bool To(out string output, string input)
        {
            bool returnValue = true;

            switch (LanguageID.LanguageCultureExtensionCode)
            {
                case "zh--pn":
                    returnValue = ConvertPinyinNumeric.Canonical(out output, input, AddRegex);
                    break;
                default:
                    output = input;
                    break;
            }

            return returnValue;
        }

        public override bool From(out string output, string input)
        {
            output = input;
            return true;
        }
    }
}
