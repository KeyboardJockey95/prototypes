using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.Converters
{
    public class ConvertRomanization : ConvertBase
    {
        public LanguageID NonRomanizationLanguageID { get; set; }
        public LanguageID RomanizationLanguageID { get; set; }
        public bool InsertWordSpace { get; set; }
        public char SyllableSeparator { get; set; }
        public DictionaryRepository Dictionary { get; set; }
        public FormatQuickLookup QuickDictionaryTo { get; set; }
        public FormatQuickLookup QuickDictionaryFrom { get; set; }
        public LanguageTool Tool;
        protected int TableSize;
        protected string[] TablePairs;      // Pairs of entries, i.e. { (non-latin 1), (latin 1), ... }.
        protected Dictionary<string, string> TableDictionary;
        protected int CharactersSize;
        protected char[] Characters;      // Single entries, i.e. { (non-latin 1), (non-latin 2), ... }.
        protected HashSet<char> CharacterSet;
        protected int MaxInputLength = 0;
        protected int MaxOutputLength = 0;
        protected bool ToTableIsAmbiguous = false;
        protected bool FromTableIsAmbiguous = false;

        public ConvertRomanization(
            string[] tablePairs,
            Dictionary<string, string> tableDictionary,
            char[] characters,
            HashSet<char> characterSet,
            LanguageID nonRomanizationLanguageID,
            LanguageID romanizationLanguageID,
            char syllableSeparator,
            DictionaryRepository dictionary,
            bool useQuickDictionary,
            int maxInputLength,
            int maxOutputLength)
        {
            NonRomanizationLanguageID = nonRomanizationLanguageID;
            RomanizationLanguageID = romanizationLanguageID;
            InsertWordSpace = false;
            SyllableSeparator = syllableSeparator;
            Dictionary = dictionary;
            if (useQuickDictionary)
            {
                QuickDictionaryTo = FormatQuickLookup.GetQuickDictionary(NonRomanizationLanguageID, RomanizationLanguageID);
                QuickDictionaryFrom = FormatQuickLookup.GetQuickDictionary(RomanizationLanguageID, NonRomanizationLanguageID);
            }
            Tool = null;
            TableSize = tablePairs.Count();
            TablePairs = tablePairs;
            TableDictionary = tableDictionary;
            if (characters != null)
                CharactersSize = characters.Count();
            Characters = characters;
            CharacterSet = characterSet;
            MaxInputLength = maxInputLength;
            MaxOutputLength = maxOutputLength;
        }

        public ConvertRomanization(LanguageID nonRomanizationLanguageID, LanguageID romanizationLanguageID,
            char syllableSeparator, DictionaryRepository dictionary, bool useQuickDictionary)
        {
            NonRomanizationLanguageID = nonRomanizationLanguageID;
            RomanizationLanguageID = romanizationLanguageID;
            InsertWordSpace = false;
            SyllableSeparator = syllableSeparator;
            Dictionary = dictionary;
            if (useQuickDictionary)
            {
                QuickDictionaryTo = FormatQuickLookup.GetQuickDictionary(NonRomanizationLanguageID, RomanizationLanguageID);
                QuickDictionaryFrom = FormatQuickLookup.GetQuickDictionary(RomanizationLanguageID, NonRomanizationLanguageID);
            }
            Tool = null;
            TableSize = 0;
            TablePairs = null;
            TableDictionary = null;
            CharactersSize = 0;
            Characters = null;
            CharacterSet = null;
            MaxInputLength = 0;
            MaxOutputLength = 0;
        }

        public ConvertRomanization()
        {
            NonRomanizationLanguageID = null;
            RomanizationLanguageID = null;
            InsertWordSpace = false;
            SyllableSeparator = '\0';
            Dictionary = null;
            QuickDictionaryTo = null;
            QuickDictionaryFrom = null;
            Tool = null;
            TableSize = 0;
            TablePairs = null;
            TableDictionary = null;
            CharactersSize = 0;
            Characters = null;
            CharacterSet = null;
            MaxInputLength = 0;
            MaxOutputLength = 0;
        }

        public override bool To(out string output, string input)
        {
            if (input == null)
            {
                output = "";
                return false;
            }

            if (((Dictionary != null) || (QuickDictionaryTo != null)) &&
                    (SyllableSeparator == '\0') &&
                    (ToTableIsAmbiguous || !IsAllNonRomanizedPhonetic(input)))
                return ConvertToRomanization(NonRomanizationLanguageID, RomanizationLanguageID, SyllableSeparator, input, out output,
                        Dictionary, QuickDictionaryTo);

            if (TableSize == 0)
            {
                output = "";
                return false;
            }

            bool returnValue = ToTable(out output, input);

            if (returnValue)
                output = PostTo(output);

            return returnValue;
        }

        public virtual string PostTo(string output)
        {
            return output;
        }

        public bool ToTable(out string output, string input)
        {
            int count = input.Length;
            int inputDelta = 1;
            int index;
            int length;
            StringBuilder sb = new StringBuilder();
            bool returnValue = (count > 0 ? false : true);

            for (index = 0; index < count; index += inputDelta)
            {
                if (SyllableSeparator != '\0')
                {
                    if (!char.IsWhiteSpace(input[index]) && (index > 0) && !char.IsWhiteSpace(input[index - 1]))
                        sb.Append(SyllableSeparator);
                }

                if (GetRomanizedSyllable(input, index, sb, out length, out inputDelta))
                    returnValue = true;
            }

            output = sb.ToString();

            return returnValue;
        }

        public override bool From(out string output, string input)
        {
            if (input == null)
            {
                output = "";
                return false;
            }

            if (((Dictionary != null) || (QuickDictionaryFrom != null)) &&
                    (SyllableSeparator == '\0') &&
                    (FromTableIsAmbiguous || !IsAllRomanizedPhonetic(input)))
                return ConvertToCharacters(RomanizationLanguageID, NonRomanizationLanguageID, SyllableSeparator, input, out output,
                        Dictionary, QuickDictionaryFrom);

            if (TableSize == 0)
            {
                output = "";
                return false;
            }

            return FromTable(out output, input);
        }

        public bool FromTable(out string output, string input)
        {

            int count = input.Length;
            int index;
            int inputLength;
            int outputLength;
            StringBuilder sb = new StringBuilder();
            bool returnValue = (count > 0 ? false : true);

            for (index = 0; index < count; )
            {
                if ((SyllableSeparator != '\0') && (input[index] == SyllableSeparator))
                {
                    index++;
                    continue;
                }

                if (GetTargetSyllable(input, index, out inputLength, sb, out outputLength))
                    returnValue = true;

                index += inputLength;
            }

            output = sb.ToString();

            return returnValue;
        }

        protected virtual bool GetRomanizedSyllable(string input, int offset, StringBuilder output, out int outputLength, out int inputDelta)
        {
            if (TableDictionary != null)
            {
                int length = input.Length - offset;
                string inputRun;
                if (MaxInputLength > 0)
                {
                    if (length > MaxInputLength)
                        length = MaxInputLength;
                }
                inputRun = input.Substring(offset, length);
                string outputRun;

                inputDelta = 0;

                if (length == 0)
                {
                    outputLength = 0;
                    return false;
                }

                while (length != 0)
                {
                    if (TableDictionary.TryGetValue(inputRun, out outputRun))
                    {
                        output.Append(outputRun);
                        outputLength = outputRun.Length;
                        inputDelta = length;
                        return true;
                    }
                    length--;
                    if (length != 0)
                        inputRun = inputRun.Substring(0, length);
                }

                inputDelta = 1;
                outputLength = 1;
                output.Append(inputRun[0]);

                return false;
            }

            int index;
            string targetSyllable = input.Substring(offset);
            string bestString = null;
            int bestLength = 0;

            inputDelta = 0;

            for (index = 0; index < TableSize; index += 2)
            {
                if (targetSyllable.StartsWith(TablePairs[index]))
                {
                    inputDelta = TablePairs[index].Length;
                    string romanization = TablePairs[index + 1];

                    if (inputDelta > bestLength)
                    {
                        bestString = romanization;
                        bestLength = inputDelta;
                    }
                }
            }

            if (bestLength > 0)
            {
                output.Append(bestString);
                outputLength = bestString.Length;
                return true;
            }

            inputDelta = 1;
            outputLength = 1;
            output.Append(targetSyllable[0]);

            return false;
        }


        protected virtual bool GetRomanizedSyllable(string input, out string output)
        {
            if (TableDictionary != null)
            {
                if (!String.IsNullOrEmpty(input))
                {
                    if (TableDictionary.TryGetValue(input, out output))
                        return true;
                }

                output = String.Empty;

                return false;
            }

            int index;

            for (index = 0; index < TableSize; index += 2)
            {
                if (input == TablePairs[index])
                {
                    output = TablePairs[index + 1];
                    return true;
                }
            }

            output = String.Empty;

            return false;
        }

        public virtual bool IsAllNonRomanizedPhonetic(string input)
        {
            if (String.IsNullOrEmpty(input))
                return false;

            if (CharacterSet != null)
            {
                foreach (char c in input)
                {
                    if (!CharacterSet.Contains(c))
                        return false;
                }

                return true;
            }

            return false;
        }

        public virtual bool IsAllRomanizedPhonetic(string input)
        {
            if (String.IsNullOrEmpty(input))
                return false;

            foreach (char c in input)
            {
                if (c > '~')
                    return false;
            }

            return true;
        }

        public virtual bool IsAllPhonetic(string input)
        {
            if (String.IsNullOrEmpty(input))
                return false;

            foreach (char c in input)
            {
                if (c > '~')
                    return false;
            }

            return true;
        }

        protected virtual bool GetTargetSyllable(string input, int offset, out int inputLength, StringBuilder output, out int outputLength)
        {
            int index;
            int maxLength = input.Length - offset;
            int bestLength = 0;
            string bestMatch = null;

            outputLength = 1;

            for (index = 0; index < TableSize; index += 2)
            {
                string currentRomanization = TablePairs[index + 1];
                int currentLength = currentRomanization.Length;

                if ((currentLength > maxLength) || (currentLength <= bestLength))
                    continue;

                if (MatchSubstring(input, offset, currentLength, currentRomanization))
                {
                    bestMatch = TablePairs[index];
                    bestLength = currentLength;
                }
            }

            if (bestLength > 0)
            {
                inputLength = bestLength;
                output.Append(bestMatch);
                return true;
            }

            output.Append(input[offset]);
            inputLength = 1;

            return false;
        }

        protected bool MatchSubstring(string input, int offset, int length, string pattern)
        {
            int index;

            for (index = 0; index < length; index++)
            {
                if (char.ToLower(input[offset + index]) != pattern[index])
                    return false;
            }

            return true;
        }

        public bool ConvertToRomanization(LanguageID inputLanguage, LanguageID outputLanguage, char syllableSeparator,
            string inputString, out string outputString, DictionaryRepository dictionary, FormatQuickLookup quickDictionary)
        {
            return ConvertParagraph(
                inputLanguage,
                outputLanguage,
                syllableSeparator,
                inputString,
                out outputString,
                dictionary,
                quickDictionary,
                Tool,
                ConvertToRomanizationRun);
        }

        public bool ConvertToRomanizationRun(
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            char syllableSeparator,
            string inputString,
            out string outputString,
            DictionaryRepository dictionary,
            FormatQuickLookup quickDictionary,
            LanguageTool tool,
            bool isWord)
        {
            StringBuilder sb = new StringBuilder();
            DictionaryEntry definition = null;
            LanguageString alternate;
            List<string> words = new List<string>(50);
            List<LanguageString> alternates;
            int index;
            int startIndex;
            string str;
            string romanizationText;
            string romanizationSyllable;
            bool containsNewlines = false;
            bool lastWasRomanization = false;
            bool gotEntry = false;
            bool gotSyllable = false;
            int maxChars = 10;
            char c;
            //char[] whiteSpace = LanguageLookup.SpaceCharacters;
            bool lastWasWhitespace = false;
            bool lastWasWidePunctuation = false;
            //bool hasZeroWidthSpaces = false;
            bool returnValue = true;

            if (inputString.Contains("\n") || inputString.Contains("\r"))
                containsNewlines = true;

            //if (inputString.IndexOf(LanguageLookup.ZeroWidthSpace) != -1)
            //    hasZeroWidthSpaces = true;
            //else if (inputString.IndexOf(LanguageLookup.ZeroWidthNoBreakSpace) != -1)
            //    hasZeroWidthSpaces = true;

            //if (inputString.IndexOf(LanguageLookup.ZeroWidthSpace) != -1)
            //    inputString = TextUtilities.NormalizeSpacesZero(inputString);

            inputString = TextUtilities.NormalizeSpacesZero(inputString);

            if (isWord)
            {
                if (IsAllNonRomanizedPhonetic(inputString))
                {
                    returnValue = ToTable(out outputString, inputString);

                    if (returnValue)
                        outputString = PostTo(outputString);

                    return returnValue;
                }
                else if ((tool != null) && tool.IsNumberString(inputString))
                {
                    outputString = tool.TranslateNumber(outputLanguage, inputString);
                    return true;
                }
            }

            int count = inputString.Length;
            int columnCount = count;

            outputString = "";

            for (startIndex = 0; startIndex < count; )
            {
                c = inputString[startIndex];

                if (LanguageLookup.SpaceAndPunctuationCharacters.Contains(c))
                {
                    string d;
                    if (c != LanguageLookup.ZeroWidthSpace)
                    {
                        d = LanguageLookup.ToRomanizedSpaceOrPunctuation(c);
                        if ((lastWasWhitespace || lastWasWidePunctuation) && (d.Length > 1))
                        {
                            if (d.StartsWith(" "))
                                d = d.Substring(1);
                            else
                                sb.Remove(sb.Length - 1, 1);
                        }
                        sb.Append(d);
                    }
                    else
                    {
                        if (!lastWasWhitespace)
                            d = LanguageLookup.ZeroWidthSpaceString + " ";
                        else
                            d = LanguageLookup.ZeroWidthSpaceString;
                        sb.Append(d);
                    }
                    startIndex++;
                    lastWasWhitespace = d.EndsWith(" ");
                    lastWasWidePunctuation = (d.Length > 1);
                    continue;
                }

                if ((c > ' ') && (c <= '~'))
                {
                    if (!lastWasWhitespace)
                        sb.Append(' ');

                    while ((startIndex < count) && (c > ' ') && (c <= '~'))
                    {
                        sb.Append(c);
                        startIndex++;
                        if (startIndex >= count)
                            break;
                        c = inputString[startIndex];
                    }

                    lastWasWhitespace = false;
                    lastWasWidePunctuation = false;
                    gotEntry = true;
                    continue;
                }

                str = null;
                romanizationText = null;
                romanizationSyllable = null;

                if (syllableSeparator != '\0')
                    columnCount = 1;
                else if (containsNewlines)
                {
                    columnCount = count;

                    for (index = startIndex; index < count; index++)
                    {
                        if ((inputString[index] == '\r') || (inputString[index] == '\n'))
                        {
                            columnCount = index;
                            break;
                        }
                    }
                }
                else
                    columnCount = count;

                if (columnCount > startIndex + maxChars)
                    columnCount = startIndex + maxChars;

                index = columnCount;

                for (int i = startIndex; i < columnCount; i++)
                {
                    c = inputString[i];

                    if (/*((c >= ' ') && (c <= '~')) ||*/ LanguageLookup.SpaceAndPunctuationCharacters.Contains(c))
                    {
                        index = i;
                        break;
                    }
                }

                for (; index > startIndex; index--)
                {
                    bool isInflection = false;

                    str = inputString.Substring(startIndex, index - startIndex);

                    if (Tool != null)
                    {
                        if (Tool.IsNumberString(str))
                        {
                            romanizationText = Tool.TranslateNumber(outputLanguage, str);
                            if (sb.Length != 0)
                                lastWasRomanization = true;
                            gotEntry = true;
                            returnValue = true;
                            break;
                        }
                        else if ((definition = Tool.LookupDictionaryEntry(
                            str,
                            Matchers.MatchCode.Exact,
                            inputLanguage,
                            null,
                            out isInflection)) != null)
                        {
                            str = definition.KeyString;
                            alternates = definition.Alternates;

                            if ((alternates != null) && (alternates.Count() != 0))
                            {
                                alternate = alternates.FirstOrDefault(x => x.LanguageID == outputLanguage);

                                if (alternate != null)
                                {
                                    romanizationText = alternate.Text;

                                    if (!String.IsNullOrEmpty(romanizationText))
                                    {
                                        if (sb.Length != 0)
                                            lastWasRomanization = true;
                                        gotEntry = true;
                                        returnValue = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (GetRomanizedSyllable(str, out romanizationSyllable))
                        {
                            gotSyllable = true;
                            returnValue = true;
                            break;
                        }
                    }
                    else
                    {
                        if ((dictionary != null) && (definition = dictionary.Get(str, inputLanguage)) != null)
                        {
                            alternates = definition.Alternates;

                            if ((alternates != null) && (alternates.Count() != 0))
                            {
                                alternate = alternates.FirstOrDefault(x => x.LanguageID == outputLanguage);

                                if (alternate != null)
                                {
                                    romanizationText = alternate.Text;

                                    if (!String.IsNullOrEmpty(romanizationText))
                                    {
                                        if (sb.Length != 0)
                                            lastWasRomanization = true;
                                        gotEntry = true;
                                        returnValue = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (GetRomanizedSyllable(str, out romanizationSyllable))
                        {
                            gotSyllable = true;
                            returnValue = true;
                            break;
                        }
                    }
                }

                if (!gotEntry && !gotSyllable)
                    lastWasRomanization = true;

                if (!isWord && /*!hasZeroWidthSpaces &&*/ !lastWasWhitespace)
                {
                    if (lastWasRomanization)
                    {
                        if (sb.Length != 0)
                        {
                            if (InsertWordSpace)
                                sb.Append(" ");
                            else if (syllableSeparator != '\0')
                                sb.Append(syllableSeparator);
                        }

                        lastWasRomanization = false;
                        gotEntry = false;
                        gotSyllable = false;
                    }
                }

                if (!String.IsNullOrEmpty(romanizationText) || !String.IsNullOrEmpty(romanizationSyllable))
                {
                    if (!String.IsNullOrEmpty(romanizationText))
                        sb.Append(romanizationText);
                    else if (!String.IsNullOrEmpty(romanizationSyllable))
                        sb.Append(romanizationSyllable);
                }
                else if (!String.IsNullOrEmpty(str))
                {
                    sb.Append(str);
                    returnValue = false;
                }

                romanizationText = null;
                romanizationSyllable = null;

                lastWasWhitespace = false;
                lastWasWidePunctuation = false;

                if (!String.IsNullOrEmpty(str))
                    startIndex += str.Length;
                else
                    startIndex += 1;
            }

            outputString = sb.ToString();

            if (InsertWordSpace)
            {
                /*
                count = LanguageLookup.FatPunctuationCharacters.Count();
                string[] fatSpaced = LanguageLookup.FatPunctuationWithSpaceCharacters;
                string[] fat = LanguageLookup.FatPunctuationCharacters;
                string[] thinSpaced = LanguageLookup.ThinPunctuationWithSpaceCharacters;

                for (index = 0; index < count; index++)
                {
                    outputString = outputString.Replace(fatSpaced[index], thinSpaced[index]);
                    outputString = outputString.Replace(fat[index], thinSpaced[index]);
                }
                */

                outputString = outputString.Trim();
            }

            outputString = PostTo(outputString);

            return returnValue;
        }

        public bool OldConvertToRomanizationRun(
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            char syllableSeparator,
            string inputString,
            out string outputString,
            DictionaryRepository dictionary,
            FormatQuickLookup quickDictionary,
            LanguageTool tool)
        {
            StringBuilder sb = new StringBuilder();
            DictionaryEntry definition = null;
            LanguageString alternate;
            LexItem lexItem = null;
            List<string> words = new List<string>(50);
            List<LanguageString> alternates;
            int index;
            int startIndex;
            string str;
            string romanizationText;
            string romanizationSyllable;
            bool containsNewlines = false;
            bool lastWasRomanization = false;
            bool gotEntry = false;
            bool gotSyllable = false;
            int maxChars = 10;
            char c;
            //char[] whiteSpace = LanguageLookup.SpaceCharacters;
            bool lastWasWhitespace = false;
            bool lastWasWidePunctuation = false;
            bool hasZeroWidthSpaces = false;
            bool returnValue = true;

            if (inputString.Contains("\n") || inputString.Contains("\r"))
                containsNewlines = true;

            if (inputString.IndexOf(LanguageLookup.ZeroWidthSpace) != -1)
                hasZeroWidthSpaces = true;
            else if (inputString.IndexOf(LanguageLookup.ZeroWidthNoBreakSpace) != -1)
                hasZeroWidthSpaces = true;

            if (inputString.IndexOf(LanguageLookup.ZeroWidthSpace) != -1)
                inputString = TextUtilities.NormalizeSpacesZero(inputString);

            int count = inputString.Length;
            int columnCount = count;

            outputString = "";

            for (startIndex = 0; startIndex < count;)
            {
                c = inputString[startIndex];

                if (LanguageLookup.SpaceAndPunctuationCharacters.Contains(c))
                {
                    string d;
                    if (c != LanguageLookup.ZeroWidthSpace)
                    {
                        d = LanguageLookup.ToRomanizedSpaceOrPunctuation(c);
                        if ((lastWasWhitespace || lastWasWidePunctuation) && (d.Length > 1))
                        {
                            if (d.StartsWith(" "))
                                d = d.Substring(1);
                            else
                                sb.Remove(sb.Length - 1, 1);
                        }
                        sb.Append(d);
                    }
                    else
                    {
                        if (!lastWasWhitespace)
                            d = LanguageLookup.ZeroWidthSpaceString + " ";
                        else
                            d = LanguageLookup.ZeroWidthSpaceString;
                        sb.Append(d);
                    }
                    startIndex++;
                    lastWasWhitespace = d.EndsWith(" ");
                    lastWasWidePunctuation = (d.Length > 1);
                    continue;
                }

                if ((c > ' ') && (c <= '~'))
                {
                    if (!lastWasWhitespace)
                        sb.Append(' ');

                    while ((startIndex < count) && (c > ' ') && (c <= '~'))
                    {
                        sb.Append(c);
                        startIndex++;
                        if (startIndex >= count)
                            break;
                        c = inputString[startIndex];
                    }

                    lastWasWhitespace = false;
                    lastWasWidePunctuation = false;
                    gotEntry = true;
                    continue;
                }

                str = null;
                romanizationText = null;
                romanizationSyllable = null;

                if (syllableSeparator != '\0')
                    columnCount = 1;
                else if (containsNewlines)
                {
                    columnCount = count;

                    for (index = startIndex; index < count; index++)
                    {
                        if ((inputString[index] == '\r') || (inputString[index] == '\n'))
                        {
                            columnCount = index;
                            break;
                        }
                    }
                }
                else
                    columnCount = count;

                if (columnCount > startIndex + maxChars)
                    columnCount = startIndex + maxChars;

                index = columnCount;

                for (int i = startIndex; i < columnCount; i++)
                {
                    c = inputString[i];

                    if (/*((c >= ' ') && (c <= '~')) ||*/ LanguageLookup.SpaceAndPunctuationCharacters.Contains(c))
                    {
                        index = i;
                        break;
                    }
                }

                if (quickDictionary != null)
                {
                    for (; index > startIndex; index--)
                    {
                        str = inputString.Substring(startIndex, index - startIndex);

                        if (quickDictionary.QuickDictionary.TryGetValue(str, out romanizationText))
                        {
                            if (gotEntry || gotSyllable)
                                lastWasRomanization = true;
                            gotEntry = true;
                            returnValue = true;
                            break;
                        }
                        else if (GetRomanizedSyllable(str, out romanizationSyllable))
                        {
                            gotSyllable = true;
                            returnValue = true;
                            break;
                        }
                    }
                }
                else
                {
                    lexItem = null;

                    if ((definition != null) && (Tool != null) && definition.HasSenseWithStem())
                    {
                        str = inputString.Substring(startIndex, index - startIndex);

                        if (Tool.LookupSuffix(str, out lexItem) && (lexItem.Text != null))
                        {
                            str = lexItem.Value;
                            index = startIndex + str.Length;
                            romanizationText = lexItem.Text.Text(LanguageLookup.JapaneseRomaji);
                        }
                        else
                            lexItem = null;

                        definition = null;
                    }

                    if (lexItem == null)
                    {
                        for (; index > startIndex; index--)
                        {
                            str = inputString.Substring(startIndex, index - startIndex);

                            if ((Tool != null) && Tool.IsNumberString(str))
                            {
                                romanizationText = Tool.TranslateNumber(outputLanguage, str);
                                if (sb.Length != 0)
                                    lastWasRomanization = true;
                                gotEntry = true;
                                returnValue = true;
                                break;
                            }
                            else if ((dictionary != null) && (definition = dictionary.Get(str, inputLanguage)) != null)
                            {
                                alternates = definition.Alternates;

                                if ((alternates != null) && (alternates.Count() != 0))
                                {
                                    alternate = alternates.FirstOrDefault(x => x.LanguageID == outputLanguage);

                                    if (alternate != null)
                                    {
                                        romanizationText = alternate.Text;

                                        if (!String.IsNullOrEmpty(romanizationText))
                                        {
                                            if (sb.Length != 0)
                                                lastWasRomanization = true;
                                            gotEntry = true;
                                            returnValue = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (GetRomanizedSyllable(str, out romanizationSyllable))
                            {
                                gotSyllable = true;
                                returnValue = true;
                                break;
                            }
                        }
                    }
                }

                if (!gotEntry && !gotSyllable)
                    lastWasRomanization = true;

                if (!hasZeroWidthSpaces && !lastWasWhitespace)
                {
                    if (lastWasRomanization)
                    {
                        if (sb.Length != 0)
                        {
                            if (InsertWordSpace)
                                sb.Append(" ");
                            else if (syllableSeparator != '\0')
                                sb.Append(syllableSeparator);
                        }

                        lastWasRomanization = false;
                        gotEntry = false;
                        gotSyllable = false;
                    }
                }

                if (!String.IsNullOrEmpty(romanizationText) || !String.IsNullOrEmpty(romanizationSyllable))
                {
                    if (!String.IsNullOrEmpty(romanizationText))
                        sb.Append(romanizationText);
                    else if (!String.IsNullOrEmpty(romanizationSyllable))
                        sb.Append(romanizationSyllable);
                }
                else if (!String.IsNullOrEmpty(str))
                {
                    sb.Append(str);
                    returnValue = false;
                }

                romanizationText = null;
                romanizationSyllable = null;

                lastWasWhitespace = false;
                lastWasWidePunctuation = false;

                if (!String.IsNullOrEmpty(str))
                    startIndex += str.Length;
                else
                    startIndex += 1;
            }

            outputString = sb.ToString();

            if (InsertWordSpace)
            {
                /*
                count = LanguageLookup.FatPunctuationCharacters.Count();
                string[] fatSpaced = LanguageLookup.FatPunctuationWithSpaceCharacters;
                string[] fat = LanguageLookup.FatPunctuationCharacters;
                string[] thinSpaced = LanguageLookup.ThinPunctuationWithSpaceCharacters;

                for (index = 0; index < count; index++)
                {
                    outputString = outputString.Replace(fatSpaced[index], thinSpaced[index]);
                    outputString = outputString.Replace(fat[index], thinSpaced[index]);
                }
                */

                outputString = outputString.Trim();
            }

            outputString = PostTo(outputString);

            return returnValue;
        }

        public bool ConvertToCharacters(LanguageID inputLanguage, LanguageID outputLanguage, char syllableSeparator,
            string inputString, out string outputString, DictionaryRepository dictionary, FormatQuickLookup quickDictionary)
        {
            return ConvertParagraph(
                inputLanguage,
                outputLanguage,
                syllableSeparator,
                inputString,
                out outputString,
                dictionary,
                quickDictionary,
                null,
                ConvertToCharactersRun);
        }

        public bool ConvertToCharactersRun(
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            char syllableSeparator,
            string inputString,
            out string outputString,
            DictionaryRepository dictionary,
            FormatQuickLookup quickDictionary,
            LanguageTool tool,
            bool isWord)
        {
            StringBuilder sb = new StringBuilder();
            DictionaryEntry definition = null;
            LanguageString alternate;
            List<string> words = new List<string>(50);
            List<LanguageString> alternates;
            int index;

            if (syllableSeparator != '\0')
                inputString = inputString.Replace(syllableSeparator.ToString(), "");

            int count = inputString.Length;
            int startIndex;
            string str;
            string charactersText;
            bool containsNewlines = false;
            int columnCount = count;
            char c;
            bool returnValue = false;

            if (inputString.Contains("\n") || inputString.Contains("\r"))
                containsNewlines = true;

            if (isWord)
            {
                if (IsAllNonRomanizedPhonetic(inputString))
                {
                    outputString = PostTo(inputString);
                    return returnValue;
                }
                else if ((tool != null) && tool.IsNumberString(inputString))
                {
                    outputString = tool.TranslateNumber(outputLanguage, inputString);
                    return true;
                }
            }

            outputString = "";

            for (startIndex = 0; startIndex < count; )
            {
                c = inputString[startIndex];

                if (LanguageLookup.SpaceAndPunctuationCharacters.Contains(c))
                {
                    sb.Append(c);
                    startIndex++;
                    continue;
                }

                str = null;
                charactersText = null;

                if (containsNewlines)
                {
                    columnCount = count;

                    for (index = startIndex; index < count; index++)
                    {
                        if ((inputString[index] == '\r') || (inputString[index] == '\n'))
                        {
                            columnCount = index;
                            break;
                        }
                    }
                }
                else
                    columnCount = count;

                index = columnCount;

                for (int i = startIndex; i < columnCount; i++)
                {
                    c = inputString[i];

                    if (LanguageLookup.SpaceAndPunctuationCharacters.Contains(c))
                    {
                        index = i;
                        break;
                    }
                }

                for (; index > startIndex; index--)
                {
                    bool isInflection = false;

                    str = inputString.Substring(startIndex, index - startIndex);

                    if (tool != null)
                    {
                        if (Tool.IsNumberString(str))
                        {
                            charactersText = Tool.TranslateNumber(outputLanguage, str);
                            returnValue = true;
                            break;
                        }
                        else if ((definition = Tool.LookupDictionaryEntry(
                            str,
                            Matchers.MatchCode.Exact,
                            inputLanguage,
                            null,
                            out isInflection)) != null)
                        {
                            alternates = definition.Alternates;

                            if ((alternates != null) && (alternates.Count() != 0))
                            {
                                alternate = alternates.FirstOrDefault(x => x.LanguageID == outputLanguage);

                                if (alternate != null)
                                {
                                    charactersText = alternate.Text;

                                    if (!String.IsNullOrEmpty(charactersText))
                                    {
                                        returnValue = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if ((definition = dictionary.Get(str, inputLanguage)) != null)
                        {
                            alternates = definition.Alternates;

                            if ((alternates != null) && (alternates.Count() != 0))
                            {
                                alternate = alternates.FirstOrDefault(x => x.LanguageID == outputLanguage);

                                if (alternate != null)
                                {
                                    charactersText = alternate.Text;

                                    if (!String.IsNullOrEmpty(charactersText))
                                    {
                                        returnValue = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(charactersText))
                    sb.Append(charactersText);
                else if (!String.IsNullOrEmpty(str))
                    sb.Append(str);

                if (!String.IsNullOrEmpty(str))
                    startIndex += str.Length;
                else
                    startIndex += 1;
            }

            outputString = sb.ToString();

            return returnValue;
        }

        public bool OldConvertToCharactersRun(
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            char syllableSeparator,
            string inputString,
            out string outputString,
            DictionaryRepository dictionary,
            FormatQuickLookup quickDictionary,
            LanguageTool tool)
        {
            StringBuilder sb = new StringBuilder();
            DictionaryEntry definition = null;
            LanguageString alternate;
            LexItem lexItem = null;
            List<string> words = new List<string>(50);
            List<LanguageString> alternates;
            int index;

            if (syllableSeparator != '\0')
                inputString = inputString.Replace(syllableSeparator.ToString(), "");

            int count = inputString.Length;
            int startIndex;
            string str;
            string charactersText;
            bool containsNewlines = false;
            int columnCount = count;
            char c;
            char[] whiteSpace = LanguageLookup.SpaceCharacters;
            bool returnValue = false;

            if (inputString.Contains("\n") || inputString.Contains("\r"))
                containsNewlines = true;

            outputString = "";

            for (startIndex = 0; startIndex < count;)
            {
                c = inputString[startIndex];

                if (whiteSpace.Contains(c))
                {
                    sb.Append(c);
                    startIndex++;
                    continue;
                }

                str = null;
                charactersText = null;

                if (containsNewlines)
                {
                    columnCount = count;

                    for (index = startIndex; index < count; index++)
                    {
                        if ((inputString[index] == '\r') || (inputString[index] == '\n'))
                        {
                            columnCount = index;
                            break;
                        }
                    }
                }
                else
                    columnCount = count;

                index = columnCount;

                for (int i = startIndex; i < columnCount; i++)
                {
                    c = inputString[i];

                    if (whiteSpace.Contains(c))
                    {
                        index = i;
                        break;
                    }
                }

                if (quickDictionary != null)
                {
                    for (; index > startIndex; index--)
                    {
                        str = inputString.Substring(startIndex, index - startIndex);

                        if (quickDictionary.QuickDictionary.TryGetValue(str, out charactersText))
                        {
                            returnValue = true;
                            break;
                        }
                    }
                }
                else
                {
                    lexItem = null;

                    if ((definition != null) && (Tool != null) && definition.HasSenseWithStem())
                    {
                        str = inputString.Substring(startIndex, index - startIndex);

                        if (Tool.LookupSuffix(str, out lexItem) && (lexItem.Text != null))
                        {
                            str = lexItem.Value;
                            index = startIndex + str.Length;
                            charactersText = lexItem.Text.Text(LanguageLookup.JapaneseKana);
                        }
                        else
                            lexItem = null;

                        definition = null;
                    }

                    if (lexItem == null)
                    {
                        for (; index > startIndex; index--)
                        {
                            str = inputString.Substring(startIndex, index - startIndex);

                            if ((definition = dictionary.Get(str, inputLanguage)) != null)
                            {
                                alternates = definition.Alternates;

                                if ((alternates != null) && (alternates.Count() != 0))
                                {
                                    alternate = alternates.FirstOrDefault(x => x.LanguageID == outputLanguage);

                                    if (alternate != null)
                                    {
                                        charactersText = alternate.Text;

                                        if (!String.IsNullOrEmpty(charactersText))
                                        {
                                            returnValue = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(charactersText))
                    sb.Append(charactersText);
                else if (!String.IsNullOrEmpty(str))
                    sb.Append(str);

                if (!String.IsNullOrEmpty(str))
                    startIndex += str.Length;
                else
                    startIndex += 1;
            }

            outputString = sb.ToString();

            return returnValue;
        }

        public bool ConvertCharacterToRomanization(char chr, out string romanized)
        {
            if (TableDictionary != null)
            {
                if (TableDictionary.TryGetValue(chr.ToString(), out romanized))
                    return true;
            }
            romanized = String.Empty;
            return false;
        }
    }
}
