using System;
using System.Collections.Generic;
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

namespace JTLanguageModelsPortable.Language
{
    public partial class LanguageTool : BaseObjectLanguage
    {
        public virtual string[] VerbClassEndings
        {
            get
            {
                return null;
            }
        }

        public virtual string[] NonVerbClassEndings
        {
            get
            {
                return null;
            }
        }

        // Returns true if matched.  runs will have length of matched or non-matched text. matchFlags
        // will have true if matched for corresponding run.
        public bool MatchWithDiff(
            MultiLanguageItem referenceStudyItem,
            MultiLanguageString textInputStrings,
            LanguageID languageID,
            List<LanguageDescriptor> languageDescriptors,
            List<int> textRuns,
            List<int> matchCodes,
            out string output)
        {
            MultiLanguageItem filteredReferenceStudyItem = GetFilteredStudyItem(
                referenceStudyItem,
                languageDescriptors);
            LanguageItem filteredReferenceLanguageItem = filteredReferenceStudyItem.LanguageItem(languageID);
            List<TextRun> filteredWordRuns = filteredReferenceLanguageItem.WordRuns;
            List<TextGraphNode> filteredNodes;
            string textInput = textInputStrings.Text(languageID);
            string pattern = filteredReferenceLanguageItem.Text;
            bool returnValue = true;

            output = String.Empty;

            if (!ParseMatchingWordRuns(
                    filteredReferenceLanguageItem.Text,
                    languageID,
                    filteredWordRuns,
                    out filteredNodes))
                return false;

            TextGraphNode node = null;
            TextGraphNode matchedNode = null;
            int nodeCount = filteredNodes.Count();
            int nodeIndex = 0;
            int textIndex = 0;
            bool inAside = false;
            char asideEndChar = '\0';
            StringBuilder sb = new StringBuilder();
            string word;
            int wordLength;

            textInput = GetCanonicalText(
                textInput,
                languageID,
                languageDescriptors,
                pattern);
            int textLength = textInput.Length;
            List<byte> charCodes = new List<byte>(textLength);

            for (; textIndex < textLength;)
            {
                char chr = textInput[textIndex];

                if (inAside)
                {
                    if (chr == asideEndChar)
                        inAside = false;

                    textIndex++;
                    continue;
                }

                if (LanguageLookup.SpaceAndPunctuationCharacters.Contains(chr))
                {
                    if (LanguageLookup.MatchedAsideCharacters.Contains(chr))
                    {
                        asideEndChar = LanguageLookup.GetMatchedEndChar(chr);
                        inAside = true;
                        textIndex++;
                        continue;
                    }

                    sb.Append(chr);
                    charCodes.Add(0);
                    textIndex++;
                    continue;
                }

                if (nodeIndex < nodeCount)
                    node = filteredNodes[nodeIndex];
                else
                    node = null;

                int bestOfs = -1;
                bool found = MatchNodeRecurse(
                    nodeIndex,
                    nodeCount,
                    filteredNodes,
                    textInput,
                    textIndex,
                    out matchedNode,
                    out word,
                    ref bestOfs);

                if (found)
                {
                    wordLength = word.Length;
                    sb.Append(word);
                    textIndex += wordLength;
                    nodeIndex++;

                    for (int i = 0; i < wordLength; i++)
                        charCodes.Add(1);
                }
                else if (bestOfs < 0)
                {
                    int otherNodeIndex;
                    int otherBestOfs = -1;

                    for (otherNodeIndex = nodeIndex + 1; otherNodeIndex < nodeCount; otherNodeIndex++)
                    {
                        found = MatchNodeRecurse(
                            otherNodeIndex,
                            nodeCount,
                            filteredNodes,
                            textInput,
                            textIndex,
                            out matchedNode,
                            out word,
                            ref otherBestOfs);

                        if (found)
                        {
                            wordLength = word.Length;
                            int blankCount = 0;

                            if (node != null)
                                blankCount = matchedNode.Start - node.Start;

                            for (int i = 0; i < blankCount; i++)
                            {
                                sb.Append('_');
                                charCodes.Add(2);
                            }

                            nodeIndex = otherNodeIndex;
                            returnValue = false;
                            break;
                        }
                        else if (otherBestOfs != -1)
                        {
                            bestOfs = otherBestOfs;
                            nodeIndex = otherNodeIndex;
                            break;
                        }
                    }
                }

                if (found)
                    continue;

                if (bestOfs > 0)
                {
                    while (textIndex < bestOfs)
                    {
                        chr = textInput[textIndex];

                        if (inAside)
                        {
                            if (chr == asideEndChar)
                                inAside = false;

                            textIndex++;
                            continue;
                        }

                        if (LanguageLookup.SpaceAndPunctuationCharacters.Contains(chr))
                        {
                            if (LanguageLookup.MatchedAsideCharacters.Contains(chr))
                            {
                                asideEndChar = LanguageLookup.GetMatchedEndChar(chr);
                                inAside = true;
                                textIndex++;
                                continue;
                            }

                            sb.Append(chr);
                            charCodes.Add(0);
                            textIndex++;
                        }
                        else
                        {
                            sb.Append(chr);
                            charCodes.Add(2);
                            textIndex++;
                        }
                    }

                    returnValue = false;
                }
                else if (nodeIndex < (nodeCount - 1))
                {
                    sb.Append(chr);
                    charCodes.Add(2);
                    textIndex++;
                    returnValue = false;
                }
                else
                {
                    while (textIndex < textLength)
                    {
                        chr = textInput[textIndex];

                        if (inAside)
                        {
                            if (chr == asideEndChar)
                                inAside = false;

                            textIndex++;
                            continue;
                        }

                        sb.Append(chr);

                        if (LanguageLookup.SpaceAndPunctuationCharacters.Contains(chr))
                            charCodes.Add(0);
                        else
                            charCodes.Add(2);

                        textIndex++;
                    }
                    nodeIndex++;
                    returnValue = false;
                }
            }

            if (nodeIndex < nodeCount)
            {
                int patternIndex;
                int patternLength = pattern.Length;

                if (nodeIndex > 1)
                {
                    node = filteredNodes[nodeIndex - 1];
                    patternIndex = node.Stop;
                }
                else
                {
                    node = filteredNodes[nodeIndex];
                    patternIndex = node.Start;
                }

                if ((textLength > 0) && pattern[patternIndex] == textInput[textLength - 1])
                    patternIndex++;

                while (patternIndex < patternLength)
                {
                    char chr = pattern[patternIndex];

                    if (LanguageLookup.SpaceAndPunctuationCharacters.Contains(chr))
                    {
                        sb.Append(chr);
                        charCodes.Add(0);
                    }
                    else
                    {
                        sb.Append('_');
                        charCodes.Add(2);
                        returnValue = false;
                    }

                    patternIndex++;
                }
            }

            if (charCodes.Count() != 0)
            {
                int codeIndex;
                int codeCount = charCodes.Count();
                int lastCode = -1;
                int runLength = 0;

                for (codeIndex = 0; codeIndex < codeCount; codeIndex++, runLength++)
                {
                    byte code = charCodes[codeIndex];

                    if (code != lastCode)
                    {
                        if (runLength != 0)
                        {
                            textRuns.Add(runLength);
                            matchCodes.Add(lastCode);
                        }

                        runLength = 0;
                        lastCode = code;
                    }
                }

                textRuns.Add(runLength);
                matchCodes.Add(lastCode);
            }

            output = sb.ToString();

            return returnValue;
        }

        protected bool MatchNodeRecurse(
            int nodeIndex,
            int nodeCount,
            List<TextGraphNode> filteredNodes,
            string textInput,
            int textIndex,
            out TextGraphNode node,
            out string matchedWord,
            ref int bestOfs)
        {
            matchedWord = null;
            node = null;

            if (nodeIndex >= nodeCount)
                return false;

            node = filteredNodes[nodeIndex];
            DictionaryEntry entry = node.Entry;
            int wordLength;
            bool found = false;

            int ofs = IndexOfStringFuzzy(node.Text, textInput, textIndex, out wordLength);

            if (ofs == textIndex)
            {
                matchedWord = textInput.Substring(textIndex, wordLength);
                found = true;
            }
            else if (entry != null)
            {
                bestOfs = ofs = IndexOfStringFuzzy(entry.KeyString, textInput, textIndex, out wordLength);

                if (ofs == textIndex)
                {
                    matchedWord = textInput.Substring(textIndex, wordLength);
                    found = true;
                }
                else if (entry.HasAlternates())
                {
                    foreach (LanguageString alternate in entry.Alternates)
                    {
                        ofs = IndexOfStringFuzzy(alternate.Text, textInput, textIndex, out wordLength);

                        if (ofs == textIndex)
                        {
                            matchedWord = textInput.Substring(textIndex, wordLength);
                            found = true;
                            break;
                        }

                        if ((ofs > -1) && (ofs < bestOfs))
                            bestOfs = ofs;
                    }
                }
            }

            return found;
        }

        public virtual int IndexOfStringFuzzy(string pattern, string text, int textIndex, out int matchLength)
        {
            int patternLength = pattern.Length;
            int textLength = text.Length;
            int textEnd = textLength - patternLength;
            int returnValue = -1;

            if (textEnd < textLength)
                textEnd++;

            matchLength = 0;

            while (textIndex <= textEnd)
            {
                if (MatchStringFuzzy(pattern, text, textIndex, out matchLength))
                {
                    returnValue = textIndex;
                    break;
                }

                textIndex++;
            }

            return returnValue;
        }

        public virtual bool MatchStringStart(string pattern, string text)
        {
            if (text.Length < pattern.Length)
                return false;

            string textSegment = text.Substring(0, pattern.Length);

            if (MatchString(pattern, textSegment))
                return true;

            return false;
        }

        public virtual bool MatchString(string pattern, string text)
        {
            int matchLength;

            if (MatchStringFuzzy(pattern, text, 0, out matchLength))
            {
                if (matchLength == text.Length)
                    return true;
            }

            return false;
        }

        public virtual bool MatchStringFuzzy(string pattern, string text, int textIndex, out int matchLength)
        {
            int textStartIndex = textIndex;
            int patternIndex = 0;
            int patternLength = pattern.Length;
            int textLength = text.Length;
            bool returnValue = true;

            while (patternIndex < patternLength)
            {
                if (!MatchCharFuzzy(pattern, ref patternIndex, patternLength, text, ref textIndex, textLength))
                {
                    returnValue = false;
                    break;
                }
            }

            matchLength = textIndex - textStartIndex;

            return returnValue;
        }

        public virtual bool MatchCharFuzzy(
            string pattern,
            ref int patternIndex,
            int patternLength,
            string text,
            ref int textIndex,
            int textLength)
        {
            bool returnValue = false;

            if ((patternIndex < patternLength) && (textIndex < textLength))
            {
                char patternChar = char.ToLower(pattern[patternIndex]);
                char textChar = char.ToLower(text[textIndex]);

                if (textChar == patternChar)
                {
                    patternIndex++;
                    textIndex++;
                    returnValue = true;
                }
            }

            return returnValue;
        }

        public virtual bool MatchRomanizedString(
            string romanized,
            string targetText,
            out string matchedText)
        {
            matchedText = null;

            int romanizedIndex;
            int romanizedCount = romanized.Length;
            int targetIndex = 0;
            int targetCount = targetText.Length;
            string targetCharStr;
            List<string> romanizedStrings;

            for (romanizedIndex = 0; (romanizedIndex < romanizedCount) && (targetIndex < targetCount);)
            {
                targetCharStr = targetText.Substring(targetIndex, 1);
                romanizedStrings = GetCharacterRomanizedStrings(targetCharStr);
                bool matched = false;

                if (romanizedStrings == null)
                    return false;

                foreach (string romanizedString in romanizedStrings)
                {
                    if (MatchStringStart(romanizedString, romanized))
                    {
                        matched = true;
                        break;
                    }
                }

                if (matched)
                    targetIndex++;
                else
                    return false;
            }

            matchedText = targetText.Substring(0, targetIndex);

            return true;
        }

        public virtual bool MatchRomanizedStringAndGetAlternate(
            string romanizedWord,
            string targetText,
            out string matchedText,
            out string alternateText)
        {
            matchedText = String.Empty;
            alternateText = String.Empty;

            int romanizedIndex;
            int romanizedCount = romanizedWord.Length;
            int targetIndex = 0;
            int targetCount = targetText.Length;
            string targetCharStr;
            string romanizedPart;
            List<string> romanizedStrings = new List<string>();
            List<string> alternateStrings = new List<string>();

            for (romanizedIndex = 0; (romanizedIndex < romanizedCount) && (targetIndex < targetCount);)
            {
                romanizedPart = romanizedWord.Substring(romanizedIndex);
                targetCharStr = targetText.Substring(targetIndex, 1);

                romanizedStrings.Clear();
                alternateStrings.Clear();

                if (!GetCharacterRomanizedAndAlternateStrings(targetCharStr, romanizedStrings, alternateStrings))
                    return false;

                int index;
                int count = romanizedStrings.Count();
                bool matched = false;

                for (index = 0; index < count; index++)
                {
                    string romanizedString = romanizedStrings[index];

                    if (MatchStringStart(romanizedString, romanizedPart))
                    {
                        if (index < alternateStrings.Count())
                            alternateText += alternateStrings[index];

                        romanizedIndex += romanizedString.Length;
                        matched = true;
                        break;
                    }
                }

                if (matched)
                    targetIndex++;
                else
                    return false;
            }

            matchedText = targetText.Substring(0, targetIndex);

            return true;
        }

        public virtual MultiLanguageItem GetFilteredStudyItem(
            MultiLanguageItem multiLanguageItem,
            List<LanguageDescriptor> languageDescriptors)
        {
            MultiLanguageItem filteredMultiLanguageItem = new MultiLanguageItem();
            object key = multiLanguageItem.Key;

            filteredMultiLanguageItem.Key = key;

            int wordCount = multiLanguageItem.GetWordCount(TargetLanguageIDs);

            foreach (LanguageID languageID in TargetLanguageIDs)
            {
                LanguageItem languageItem = multiLanguageItem.LanguageItem(languageID);
                LanguageItem filteredLanguageItem;

                if (languageItem == null)
                {
                    filteredLanguageItem = new LanguageItem(key, languageID, String.Empty);
                    filteredMultiLanguageItem.Add(filteredLanguageItem);
                    continue;
                }

                filteredLanguageItem = GetFilteredLanguageItem(
                    languageItem,
                    languageDescriptors);

                filteredMultiLanguageItem.Add(filteredLanguageItem);
            }

            return filteredMultiLanguageItem;
        }

        public virtual LanguageItem GetFilteredLanguageItem(
            LanguageItem languageItem,
            List<LanguageDescriptor> languageDescriptors)
        {
            object key = languageItem.Key;
            LanguageID languageID = languageItem.LanguageID;
            int wordCount = languageItem.WordRunCount();
            int wordIndex;
            TextRun wordRun;
            TextRun filteredWordRun;
            int textIndex = 0;
            LanguageItem filteredLanguageItem = new LanguageItem(key, languageID, String.Empty);
            StringBuilder sb = new StringBuilder();
            bool inAside = false;
            char asideEndChar = '\0';

            if (wordCount == 0)
                wordCount = 1;

            filteredLanguageItem.WordRuns = new List<TextRun>(wordCount);

            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                wordRun = languageItem.GetWordRun(wordIndex);

                if (wordRun == null)
                    wordRun = new TextRun(0, languageItem.TextLength, null);

                if (textIndex < wordRun.Start)
                {
                    string betweenString = languageItem.Text.Substring(textIndex, wordRun.Start - textIndex);

                    betweenString = GetNormalizedText(betweenString);

                    int charIndex;
                    int charCount = betweenString.Length;

                    for (charIndex = 0; charIndex < charCount; charIndex++)
                    {
                        char chr = betweenString[charIndex];

                        if (inAside)
                        {
                            if (chr == asideEndChar)
                            {
                                inAside = false;
                                asideEndChar = '\0';
                            }
                        }
                        else
                        {
                            bool isAside = LanguageLookup.MatchedAsideCharacters.Contains(chr);

                            if (isAside)
                            {
                                asideEndChar = LanguageLookup.GetMatchedEndChar(chr);
                                inAside = true;
                            }
                            else
                                sb.Append(chr);
                        }
                    }

                    textIndex = wordRun.Start;
                }

                if (!inAside)
                {
                    string word = languageItem.Text.Substring(wordRun.Start, wordRun.Length);
                    string filteredWord = GetCanonicalText(word, languageID, languageDescriptors, null);
                    int start = sb.Length;
                    sb.Append(filteredWord);
                    filteredWordRun = new TextRun(start, filteredWord.Length, null);
                    filteredLanguageItem.WordRuns.Add(filteredWordRun);
                }

                textIndex += wordRun.Length;
            }

            if (textIndex < languageItem.TextLength)
            {
                string betweenString = languageItem.Text.Substring(textIndex, languageItem.TextLength - textIndex);

                betweenString = GetNormalizedText(betweenString);

                int charIndex;
                int charCount = betweenString.Length;

                for (charIndex = 0; charIndex < charCount; charIndex++)
                {
                    char chr = betweenString[charIndex];

                    if (inAside)
                    {
                        if (chr == asideEndChar)
                        {
                            inAside = false;
                            asideEndChar = '\0';
                        }
                    }
                    else
                    {
                        bool isAside = LanguageLookup.MatchedAsideCharacters.Contains(chr);

                        if (isAside)
                        {
                            asideEndChar = LanguageLookup.GetMatchedEndChar(chr);
                            inAside = true;
                        }
                        else
                            sb.Append(chr);
                    }
                }
            }

            filteredLanguageItem.Text = sb.ToString();

            return filteredLanguageItem;
        }

        public virtual bool IsNormallyCapitalized(string str)
        {
            return false;
        }

        public virtual string GetNormalizedText(
            string input)
        {
            string output = TextUtilities.GetNormalizedString(input);
            return output;
        }

        public virtual string GetFrequencyNormalizedText(
            string input)
        {
            return input;
        }

        public virtual string GetCanonicalText(
            string input,
            LanguageID languageID,
            List<LanguageDescriptor> languageDescriptors,
            string pattern)
        {
            string output = GetNormalizedText(input);
            output = TextUtilities.SubstitutionCheck(output, languageID, languageDescriptors);
            output = DisplayConversionCheck(output, languageID, pattern);
            return output;
        }

        public virtual string DisplayConversionCheck(
            string input,
            LanguageID languageID,
            string pattern)
        {
            return input;
        }

        public virtual bool IsNumberString(string str)
        {
            if (String.IsNullOrEmpty(str))
                return false;
            if (str.StartsWith("-"))
                str = str.Substring(1);
            if (String.IsNullOrEmpty(str))
                return false;

            bool first = true;
            char last = str[str.Length - 1];

            foreach (char c in str)
            {
                if ((c == '.') || (c == ','))
                    continue;

                if (((c == 'e') || (c == 'E')) && !first && (c != last))
                    continue;

                first = false;

                if ((c < '0') || (c > '9'))
                    return false;
            }
            return true;
        }

        public virtual bool IsExpandedNumberString(string str)
        {
            Dictionary<string, string> numberNameToDigitDictionary = NumberNameToDigitDictionary;

            if (numberNameToDigitDictionary == null)
                return false;   // Need language-specific implementation.

            string digits;

            if (!TextUtilities.ContainsOneOrMoreCharacters(str, LanguageLookup.DigitSeparatorCharacters))
            {
                if (numberNameToDigitDictionary.TryGetValue(str, out digits))
                    return true;
            }
            else
            {
                string[] parts = str.Split(LanguageLookup.DigitSeparatorCharacters);

                foreach (string part in parts)
                {
                    if (!numberNameToDigitDictionary.TryGetValue(str, out digits))
                        return false;
                }

                return true;
            }

            return false;
        }

        public virtual bool IsExpandedNumberStringConnector(string str)
        {
            if (String.IsNullOrEmpty(str))
                return false;

            string[] digitConnectorTable = DigitConnectorTable;

            if (digitConnectorTable == null)
                return false;   // Need language-specific implementation.

            bool returnValue = digitConnectorTable.Contains(str);

            return returnValue;
        }

        public virtual bool IsExpandedNumberStringConnector(string lastWord, string str)
        {
            if (String.IsNullOrEmpty(str))
                return false;

            string[] digitConnectorTable = DigitConnectorTable;

            if (digitConnectorTable == null)
                return false;   // Need language-specific implementation.

            bool returnValue = digitConnectorTable.Contains(str);

            if (!IsAllowWordBeforeDigitConnector(lastWord, str))
                return false;

            return returnValue;
        }

        public virtual bool IsAllowWordBeforeDigitConnector(string lastWord, string connector)
        {
            return true;    // Need language-specific implementation.
        }

        // Array of "(name)", "(digit)", ...
        // i.e. "zero", "0", "one", "1", ...
        public virtual string[] NumberNameTable
        {
            get
            {
                return null;
            }
        }

        // Array of "(digit)", "(name)", ...
        // i.e. "0", "zero", "1", "one", ...
        public virtual string[] NumberDigitTable
        {
            get
            {
                return null;
            }
        }

        // Array of non-punctuation strings that may separate number components.
        public virtual string[] DigitConnectorTable
        {
            get
            {
                return null;
            }
        }

        private Dictionary<string, string> _NumberDigitToNameDictionary;
        public virtual Dictionary<string, string> NumberDigitToNameDictionary
        {
            get
            {
                if (_NumberDigitToNameDictionary == null)
                {
                    string[] digitTable = NumberDigitTable;

                    if (digitTable == null)
                        return null;

                    _NumberDigitToNameDictionary = new Dictionary<string, string>();

                    for (int i = 0; i < NumberDigitTable.Length; i += 2)
                    {
                        if (!_NumberDigitToNameDictionary.ContainsKey(digitTable[i]))
                            _NumberDigitToNameDictionary.Add(digitTable[i], digitTable[i + 1]);
                    }
                }

                return _NumberDigitToNameDictionary;
            }
        }

        private Dictionary<string, string> _NumberNameToDigitDictionary;
        public virtual Dictionary<string, string> NumberNameToDigitDictionary
        {
            get
            {
                if (_NumberNameToDigitDictionary == null)
                {
                    string[] digitTable = NumberNameTable;

                    if (digitTable == null)
                        return null;

                    _NumberNameToDigitDictionary = new Dictionary<string, string>();

                    for (int i = 0; i < NumberNameTable.Length; i += 2)
                    {
                        if (!_NumberNameToDigitDictionary.ContainsKey(digitTable[i]))
                            _NumberNameToDigitDictionary.Add(digitTable[i], digitTable[i + 1]);
                    }
                }

                return _NumberNameToDigitDictionary;
            }
        }

        public virtual string GetDigitName(string digitString)
        {
            Dictionary<string, string> dictionary = NumberDigitToNameDictionary;

            if (dictionary == null)
                return null;

            string nameString;

            if (dictionary.TryGetValue(digitString, out nameString))
                return nameString;

            return digitString;
        }

        public virtual List<string[]> GetNumberExpansions(string numberString)
        {
            List<string[]> expansions = new List<string[]>();

            string[] standardExpansion = GetNumberExpansionStandard(numberString);

            if (standardExpansion != null)
                expansions.Add(standardExpansion);

            string[] digitsExpansion = GetNumberExpansionDigits(numberString);

            if (digitsExpansion != null)
            {
                if (standardExpansion != null)
                {
                    if (!digitsExpansion.SequenceEqual(standardExpansion))
                        expansions.Add(digitsExpansion);
                }
                else
                    expansions.Add(digitsExpansion);
            }

            return expansions;
        }

        public virtual string[] GetNumberExpansionDigits(string numberString)
        {
            return null;
        }

        public virtual string[] GetNumberExpansionStandard(string numberString)
        {
            if (String.IsNullOrEmpty(numberString))
                return null;

            Dictionary<string, string> dictionary = NumberDigitToNameDictionary;

            if (dictionary == null)
                return null;

            string nameString;

            if (dictionary.TryGetValue(numberString, out nameString))
                return nameString.Split(LanguageLookup.Space);

            string spelledOutNumber = ConvertWholeNumber(numberString);

            return spelledOutNumber.Split(LanguageLookup.Space);
        }

        protected virtual string ConvertWholeNumber(string Number)
        {
            return null;
        }

        public virtual string GetDigitsStringFromNumberExpansionList(List<string> expandedNumberStrings)
        {
            int value;

            if (GetDigitsFromNumberExpansionList(expandedNumberStrings, out value))
                return value.ToString();

            return String.Empty;
        }

        public virtual string GetDigitsStringFromNumberExpansionString(string expandedNumberString)
        {
            int value;

            if (String.IsNullOrEmpty(expandedNumberString))
                return String.Empty;

            string[] parts = expandedNumberString.Split(LanguageLookup.DigitSeparatorCharacters, StringSplitOptions.RemoveEmptyEntries);
            List<string> partsList = parts.ToList();

            if (GetDigitsFromNumberExpansionList(partsList, out value))
                return value.ToString();

            return String.Empty;
        }

        public virtual bool GetDigitsFromNumberExpansionList(List<string> expandedNumberStrings, out int value)
        {
            Dictionary<string, string> numberNameToDigitDictionary = NumberNameToDigitDictionary;

            value = 0;

            if (numberNameToDigitDictionary == null)
                return false;

            if ((expandedNumberStrings == null) || (expandedNumberStrings.Count() == 0))
                return false;

            int index;
            int count = expandedNumberStrings.Count();
            string digitString;
            int digitValue = 0;
            bool isAllOneDigits = true;

            for (index = 0; index < count; index++)
            {
                string expandedNumber = expandedNumberStrings[index];

                if (numberNameToDigitDictionary.TryGetValue(expandedNumber, out digitString))
                {
                    digitValue = ObjectUtilities.GetIntegerFromString(digitString, digitValue);

                    if (digitValue >= 10)
                    {
                        isAllOneDigits = false;
                        break;
                    }

                    value = (value * 10) + digitValue;
                }
            }

            if (isAllOneDigits)
                return true;

            value = 0;

            for (index = 0; index < count; index++)
            {
                string expandedNumber = expandedNumberStrings[index];

                digitValue = 0;

                if ((index > 0) && IsExpandedNumberStringConnector(expandedNumberStrings[index - 1], expandedNumber))
                    continue;

                if (numberNameToDigitDictionary.TryGetValue(expandedNumber, out digitString))
                {
                    digitValue = ObjectUtilities.GetIntegerFromString(digitString, digitValue);

                    //if (digitValue < 100)
                    {
                        int nextIndex = index + 1;
                        int nextCount = 0;

                        if (nextIndex < count)
                        {
                            string nextExpandedNumber = expandedNumberStrings[nextIndex];

                            if (IsExpandedNumberStringConnector(expandedNumberStrings[nextIndex - 1], nextExpandedNumber) && (++nextIndex < count))
                            {
                                nextExpandedNumber = expandedNumberStrings[nextIndex];
                                nextCount++;
                            }

                            if (numberNameToDigitDictionary.TryGetValue(nextExpandedNumber, out digitString))
                            {
                                int digitWeight = ObjectUtilities.GetIntegerFromString(digitString, 0);
                                switch (digitWeight)
                                {
                                    case 0:
                                        break;
                                    case 100:
                                    case 1000:
                                    case 10000:
                                    case 100000:
                                    case 1000000:
                                    case 10000000:
                                    case 100000000:
                                    case 1000000000:
#if true
                                        if (value >= 100)
                                        {
                                            digitValue += value;
                                            value = 0;
                                        }
#endif
                                        digitValue *= digitWeight;
                                        nextCount++;
                                        break;
                                    default:
                                        switch (digitValue)
                                        {
                                            case 1000:
                                                //value = 0;
                                                //digitValue *= value;
                                                digitValue += digitWeight;
                                                break;
                                            default:
                                                digitValue += digitWeight;
                                                break;
                                        }
                                        nextCount++;
                                        break;
                                }
                            }
                        }

                        index += nextCount;
                    }

                    value += digitValue;
                }
                else
                    return false;
            }

            return true;
        }

        public virtual string NormalizeNumberWord(string numberString)
        {
            // Need language implementation.
            return numberString;
        }

        public virtual bool ParseNumberString(
            string str,
            int startIndex,
            int length,
            out string numberString,
            ref int nextIndex)
        {
            int index = startIndex;
            int count = 0;
            int digitCount = 0;
            int exponentCount = 0;

            numberString = String.Empty;

            if (String.IsNullOrEmpty(str))
                return false;

            if (startIndex >= length)
                return false;

            if (str[index] == '-')
            {
                index++;
                count++;
            }

            if (index >= length)
                return false;

            for (; index < length; index++)
            {
                char c = str[index];

                switch (c)
                {
                    case '.':
                    case ',':
                        break;
                    case 'e':
                    case 'E':
                        if (exponentCount > 0)
                        {
                            numberString = str.Substring(startIndex, index - startIndex);
                            nextIndex = index;
                            return true;
                        }
                        exponentCount++;
                        break;
                    case '-':
                        if ((digitCount != 0) && (exponentCount == 0))
                        {
                            numberString = str.Substring(startIndex, index - startIndex);
                            nextIndex = index;
                            return true;
                        }
                        break;
                    default:
                        if (IsWhiteSpace(c))
                        {
                            if (digitCount != 0)
                            {
                                numberString = str.Substring(startIndex, index - startIndex);
                                nextIndex = index;
                                return true;
                            }
                            else
                                return false;
                        }
                        else if (IsNumberCharacter(c))
                            digitCount++;
                        else
                        {
                            if (digitCount != 0)
                            {
                                numberString = str.Substring(startIndex, index - startIndex);
                                nextIndex = index;
                                return true;
                            }
                            else
                                return false;
                        }
                        break;
                }
            }

            if (digitCount != 0)
            {
                numberString = str.Substring(startIndex, index - startIndex);
                nextIndex = index;
                return true;
            }

            return false;
        }

        public virtual bool IsNumberCharacter(char chr)
        {
            return Char.IsDigit(chr);
        }

        public virtual bool ParseWordString(
            string str,
            int startIndex,
            int length,
            out string wordString,
            ref int nextIndex)
        {
            int index = startIndex;
            int skipOffset = 0;

            wordString = String.Empty;

            if (String.IsNullOrEmpty(str))
                return false;

            if (startIndex >= length)
                return false;

            for (; index < length; index++)
            {
                char chr = str[index];

                if (chr == '.')
                {
                    if (CheckForAbbreviation(str, index, chr, out skipOffset))
                        index += skipOffset;
                    else
                        break;
                }
                else if ((chr == '\'') || (chr == '‘') || (chr == '’'))
                {
                    if (CheckForApostrophe(str, index, chr, out skipOffset))
                        index += skipOffset;
                    else
                        break;
                }
                else if (LanguageLookup.NonAlphanumericAndSpaceAndPunctuationCharacters.Contains(chr))
                    break;
            }

            if (index > 0)
            {
                wordString = str.Substring(startIndex, index - startIndex);
                nextIndex = startIndex + index;
                return true;
            }

            return false;
        }

        public virtual bool GetAccentedVowel(string outputText, out char accentedVowel, out int index)
        {
            bool returnValue = false;

            accentedVowel = '\0';
            index = -1;

            return returnValue;
        }

        public virtual int GetSyllableCount(string word)
        {
            return 0;
        }

        public virtual int GetLastNthVowelIndex(string word, int n)
        {
            return -1;
        }

        public virtual char GetLastNthVowel(string word, int n)
        {
            return '\0';
        }

        public virtual bool AccentLastVowel(ref string word)
        {
            bool returnValue = false;
            return returnValue;
        }

        public virtual bool DeaccentLastVowel(ref string word)
        {
            bool returnValue = false;
            return returnValue;
        }

        public virtual bool AccentNextToLastVowel(ref string word)
        {
            bool returnValue = false;
            return returnValue;
        }

        public virtual bool DeaccentNextToLastVowel(ref string word)
        {
            bool returnValue = false;
            return returnValue;
        }

        public static char[] GenericVowelCharacters =
        {
            'a',
            'e',
            'i',
            'o',
            'u',
            'y'
        };

        public virtual char[] VowelCharacters
        {
            get
            {
                return GenericVowelCharacters;
            }
        }

        public static char[] GenericAccentedCharacters =
        {
            'é',
            'ó',
            'à',
            'è',
            'ì',
            'ò',
            'ù',
            'â',
            'ê',
            'î',
            'ô',
            'û'
        };

        public virtual char[] AccentedCharacters
        {
            get
            {
                return GenericAccentedCharacters;
            }
        }

        public virtual bool StartsWithVowel(string word)
        {
            if (String.IsNullOrEmpty(word))
                return false;

            return IsVowel(word[0]);
        }

        public virtual bool IsVowel(char c)
        {
            return VowelCharacters.Contains(char.ToLower(c));
        }
    }
}
