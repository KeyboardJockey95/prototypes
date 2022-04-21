using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Converters
{
    // This class is for doing a transliteration conversion between languages.
    public class ConvertTransliterate : ConvertBase
    {
        public LanguageID FromLanguage { get; set; }
        public LanguageID ToLanguage { get; set; }
        public char SyllableSeparator { get; set; }
        public ConvertBase Converter { get; set; }
        public DictionaryRepository Dictionary { get; set; }
        public FormatQuickLookup QuickDictionaryTo { get; set; }
        public FormatQuickLookup QuickDictionaryFrom { get; set; }
        public LanguageTool Tool;
        // Keyed on language code, value is list of target and romanized string pairs.
        protected static Dictionary<string, string[]> _LazyTable = new Dictionary<string, string[]>();

        // For non-latin language to non-latin language.
        public ConvertTransliterate(
            LanguageID fromLanguageID,
            LanguageID toLanguageID,
            char syllableSeparator,
            DictionaryRepository dictionary,
            bool useQuickDictionary)
        {
            FromLanguage = fromLanguageID;
            ToLanguage = toLanguageID;
            SyllableSeparator = syllableSeparator;
            Dictionary = dictionary;
            if (useQuickDictionary)
            {
                QuickDictionaryTo = FormatQuickLookup.GetQuickDictionary(FromLanguage, ToLanguage);
                QuickDictionaryFrom = FormatQuickLookup.GetQuickDictionary(ToLanguage, FromLanguage);
            }
            Tool = ApplicationData.LanguageTools.Create(ToLanguage);
        }

        // For romanization.
        public ConvertTransliterate(
            bool isRomanization,
            LanguageID romanizationLanguageID,
            LanguageID nonRomanizationLanguageID,
            char syllableSeparator,
            DictionaryRepository dictionary,
            bool useQuickDictionary)
        {
            Converter = GetRomanizer(
                romanizationLanguageID,
                nonRomanizationLanguageID,
                syllableSeparator,
                dictionary,
                useQuickDictionary);
        }

        public ConvertTransliterate()
        {
            Converter = null;
        }

        public override bool To(out string output, string input)
        {
            output = "";

            if (Converter != null)
                return Converter.To(out output, input);
            else if (((Dictionary != null) || (QuickDictionaryTo != null)))
            {
                if (LanguageLookup.IsAlternatePhonetic(ToLanguage))
                    return ConvertAlternatePhonetic(FromLanguage, ToLanguage, SyllableSeparator, input, out output, Dictionary, QuickDictionaryTo, Tool);
                else
                    return ConvertCharacters(FromLanguage, ToLanguage, SyllableSeparator, input, out output, Dictionary, QuickDictionaryTo);
            }
            else
                return false;
        }

        public override bool From(out string output, string input)
        {
            output = "";

            if (Converter != null)
                return Converter.From(out output, input);
            else if (((Dictionary != null) || (QuickDictionaryFrom != null)))
            {
                if (LanguageLookup.IsAlternatePhonetic(FromLanguage))
                    return ConvertAlternatePhonetic(ToLanguage, FromLanguage, SyllableSeparator, input, out output, Dictionary, QuickDictionaryFrom, Tool);
                else
                    return ConvertCharacters(ToLanguage, FromLanguage, SyllableSeparator, input, out output, Dictionary, QuickDictionaryFrom);
            }
            else
                return false;
        }

        protected virtual ConvertBase GetRomanizer(
            LanguageID romanizationLanguageID,
            LanguageID nonRomanizationLaguageID,
            char syllableSeparator,
            DictionaryRepository dictionary,
            bool useQuickDictionary)
        {
            switch (romanizationLanguageID.LanguageCultureExtensionCode)
            {
                case "ja--rj":
                    if (nonRomanizationLaguageID == null)
                        nonRomanizationLaguageID = LanguageLookup.Japanese;
                    return new ConvertRomaji(
                        nonRomanizationLaguageID,
                        syllableSeparator,
                        dictionary,
                        useQuickDictionary);
                case "ko--rm":
                    return new ConvertHangul(
                        syllableSeparator,
                        dictionary,
                        useQuickDictionary);
                case "zh--pn":
                    if (nonRomanizationLaguageID == null)
                        nonRomanizationLaguageID = LanguageLookup.ChineseSimplified;
                    return new ConvertPinyin(
                        nonRomanizationLaguageID,
                        false,
                        dictionary,
                        useQuickDictionary);
                case "el--rm":
                    return new ConvertGreekRomanization(
                        syllableSeparator,
                        dictionary,
                        useQuickDictionary);
                case "ar--rm":
                case "fa--rm":
                case "ur--rm":
                    return new ConvertArabicRomanization(
                        syllableSeparator,
                        dictionary,
                        useQuickDictionary);
                default:
                    break;
            }

            if (nonRomanizationLaguageID == null)
            {
                nonRomanizationLaguageID = new LanguageID(romanizationLanguageID);
                nonRomanizationLaguageID.ExtensionCode = null;
            }

            string[] transliterationTable = null;

            lock (_LazyTable)
            {
                if (!_LazyTable.TryGetValue(romanizationLanguageID.LanguageCultureCode, out transliterationTable))
                {
                    string tableTildeUrl = MediaUtilities.ConcatenateUrlPath(
                        ApplicationData.ContentTildeUrl,
                        "Database/Romanization/Romanization_" +
                            nonRomanizationLaguageID.LanguageCultureCode +
                            ".txt");
                    string tableFilePath = ApplicationData.MapToFilePath(tableTildeUrl);

                    if (FileSingleton.Exists(tableFilePath))
                    {
                        try
                        {
                            string tableString = FileSingleton.ReadAllText(
                                tableFilePath, ApplicationData.Encoding);
                            transliterationTable = TextUtilities.GetStringListFromLinesString(tableString).ToArray();
                            if (transliterationTable.Count() < 2)
                                return null;
                            _LazyTable.Add(romanizationLanguageID.LanguageCultureCode, transliterationTable);
                            string[] tablePairs = transliterationTable.ToArray();
                            return new ConvertRomanization(
                                transliterationTable.ToArray(),
                                GetDictionaryFromTablePairs(tablePairs),
                                null,
                                null,
                                nonRomanizationLaguageID,
                                romanizationLanguageID,
                                syllableSeparator,
                                dictionary,
                                useQuickDictionary,
                                GetMaxInputLengthFromTablePairs(tablePairs),
                                GetMaxOutputLengthFromTablePairs(tablePairs));
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else
                {
                    string[] tablePairs = transliterationTable.ToArray();
                    return new ConvertRomanization(
                        transliterationTable.ToArray(),
                        GetDictionaryFromTablePairs(tablePairs),
                        null,
                        null,
                        nonRomanizationLaguageID,
                        romanizationLanguageID,
                        syllableSeparator,
                        dictionary,
                        useQuickDictionary,
                        GetMaxInputLengthFromTablePairs(tablePairs),
                        GetMaxOutputLengthFromTablePairs(tablePairs));
                }
            }

            return null;
        }

        protected virtual ConvertBase GetCharacterConverter(
            LanguageID targetLanguageID,
            LanguageID sourceLaguageID,
            char syllableSeparator,
            DictionaryRepository dictionary,
            bool useQuickDictionary)
        {
            return null;
        }

        public static bool ConvertCharacters(
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            char syllableSeparator,
            string inputString,
            out string outputString,
            DictionaryRepository dictionary,
            FormatQuickLookup quickDictionary)
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
                ConvertCharacterRun);
        }

        public static bool ConvertCharacterRun(
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
            LexItem lexItem = null;
            List<string> words = new List<string>(50);
            int index;
            int count = inputString.Length;
            int startIndex;
            string str;
            List<LanguageString> alternates;
            LanguageString alternate;
            int columnCount = count;
            int maxChars = 16;
            string characterStringOutput;
            bool returnValue = true;

            outputString = "";

            for (startIndex = 0; startIndex < count;)
            {
                char c = inputString[startIndex];

                if (LanguageLookup.SpaceAndPunctuationCharacters.Contains(c) || ((c >= ' ') && (c <= '~')))
                {
                    sb.Append(inputString[startIndex]);
                    startIndex++;
                    continue;
                }

                str = null;
                columnCount = count;

                if (columnCount > startIndex + maxChars)
                    columnCount = startIndex + maxChars;

                index = columnCount;

                bool found = false;

                if (quickDictionary != null)
                {
                    for (; index > startIndex; index--)
                    {
                        str = inputString.Substring(startIndex, index - startIndex);

                        if (quickDictionary.QuickDictionary.TryGetValue(str, out characterStringOutput))
                        {
                            sb.Append(characterStringOutput);
                            found = true;
                            break;
                        }
                    }
                }
                else
                {
                    lexItem = null;

                    if ((definition != null) && (tool != null) && definition.HasSenseWithStem())
                    {
                        str = inputString.Substring(startIndex, index - startIndex);

                        if (tool.LookupSuffix(str, out lexItem) && (lexItem.Text != null))
                        {
                            characterStringOutput = lexItem.Text.Text(LanguageLookup.JapaneseKana);
                            sb.Append(characterStringOutput);
                            str = lexItem.Value;
                            index = startIndex + str.Length;
                            found = true;
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
                                        characterStringOutput = alternate.Text;

                                        if (!String.IsNullOrEmpty(characterStringOutput))
                                        {
                                            sb.Append(characterStringOutput);
                                            found = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (!found)
                    sb.Append(str);

                if (!String.IsNullOrEmpty(str))
                    startIndex += str.Length;
                else
                    startIndex += 1;
            }

            outputString = sb.ToString().Trim();

            return returnValue;
        }

        /*
        public static bool ConvertCharacterRun(
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            char syllableSeparator,
            string inputString,
            out string outputString,
            DictionaryRepository dictionary,
            FormatQuickLookup quickDictionary,
            LanguageTool tool)
        {
            outputString = "";

            if (dictionary == null)
                return false;

            StringBuilder sb = new StringBuilder();
            DictionaryEntry definition;
            List<LanguageString> alternates;
            LanguageString alternate;
            int characterIndex;
            int characterCount = inputString.Length;
            string characterStringInput;
            string characterStringOutput;
            bool returnValue = false;

            if (quickDictionary != null)
            {
                for (characterIndex = 0; characterIndex < characterCount; characterIndex++)
                {
                    characterStringInput = inputString.Substring(characterIndex, 1);

                    if (quickDictionary.QuickDictionary.TryGetValue(characterStringInput, out characterStringOutput))
                        sb.Append(characterStringOutput);
                    else
                        sb.Append(characterStringInput);
                }
            }
            else
            {
                for (characterIndex = 0; characterIndex < characterCount; characterIndex++)
                {
                    if ((syllableSeparator != '\0') && (inputString[characterIndex] == syllableSeparator))
                        continue;

                    characterStringInput = inputString.Substring(characterIndex, 1);

                    if ((definition = dictionary.Get(characterStringInput, inputLanguage)) != null)
                    {
                        alternates = definition.Alternates;

                        if ((alternates != null) && (alternates.Count() != 0))
                        {
                            alternate = alternates.FirstOrDefault(x => x.LanguageID == outputLanguage);

                            if (alternate != null)
                            {
                                characterStringOutput = alternate.Text;

                                if (!String.IsNullOrEmpty(characterStringOutput))
                                {
                                    sb.Append(characterStringOutput);
                                    returnValue = true;
                                    continue;
                                }
                            }
                        }
                    }

                    // If we get here, we didn't find the character, so just use the input form.
                    sb.Append(characterStringInput);
                }
            }

            outputString = sb.ToString();

            return returnValue;
        }
        */

        public static bool ConvertAlternatePhonetic(
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            char syllableSeparator,
            string inputString,
            out string outputString,
            DictionaryRepository dictionary,
            FormatQuickLookup quickDictionary,
            LanguageTool tool)
        {
            return ConvertParagraph(
                inputLanguage,
                outputLanguage,
                syllableSeparator,
                inputString,
                out outputString,
                dictionary,
                quickDictionary,
                tool,
                ConvertAlternatePhoneticRun);
        }

        public static bool ConvertAlternatePhoneticRun(
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
            LexItem lexItem = null;
            List<string> words = new List<string>(50);
            int index;
            int count = inputString.Length;
            int startIndex;
            string str;
            List<LanguageString> alternates;
            LanguageString alternate;
            bool containsNewlines = false;
            int columnCount = count;
            int maxChars = 10;
            char c;
            //char[] whiteSpace = LanguageLookup.SpaceCharacters;
            char[] spaceAndPunctuation = LanguageLookup.SpaceAndPunctuationCharacters;
            string characterStringOutput;
            bool returnValue = true;

            if (inputString.Contains("\n") || inputString.Contains("\r"))
                containsNewlines = true;

            outputString = "";

            for (startIndex = 0; startIndex < count; )
            {
                c = inputString[startIndex];

                if (spaceAndPunctuation.Contains(c) || ((c >= ' ') && (c <= '~')))
                {
                    sb.Append(inputString[startIndex]);
                    startIndex++;
                    continue;
                }

                str = null;

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

                if (columnCount > startIndex + maxChars)
                    columnCount = startIndex + maxChars;

                index = columnCount;

                for (int i = startIndex; i < columnCount; i++)
                {
                    c = inputString[i];

                    if (((c >= ' ') && (c <= '~')) || spaceAndPunctuation.Contains(c))
                    {
                        index = i;
                        break;
                    }
                }

                bool found = false;

                if (quickDictionary != null)
                {
                    for (; index > startIndex; index--)
                    {
                        str = inputString.Substring(startIndex, index - startIndex);

                        if (quickDictionary.QuickDictionary.TryGetValue(str, out characterStringOutput))
                        {
                            sb.Append(characterStringOutput);
                            found = true;
                            break;
                        }
                    }
                }
                else
                {
                    lexItem = null;

                    if ((definition != null) && (tool != null) && definition.HasSenseWithStem())
                    {
                        str = inputString.Substring(startIndex, index - startIndex);

                        if (tool.LookupSuffix(str, out lexItem) && (lexItem.Text != null))
                        {
                            characterStringOutput = lexItem.Text.Text(LanguageLookup.JapaneseKana);
                            sb.Append(characterStringOutput);
                            str = lexItem.Value;
                            index = startIndex + str.Length;
                            found = true;
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
                                        characterStringOutput = alternate.Text;

                                        if (!String.IsNullOrEmpty(characterStringOutput))
                                        {
                                            sb.Append(characterStringOutput);
                                            found = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (!found)
                    sb.Append(str);

                if (!String.IsNullOrEmpty(str))
                    startIndex += str.Length;
                else
                    startIndex += 1;
            }

            outputString = sb.ToString().Trim();

            return returnValue;
        }

        public static void ConvertTranslate(
            LanguageID inputLanguage,
            string inputString,
            LanguageID outputLanguage,
            out string outputString,
            string separator,
            DictionaryRepository dictionary)
        {
            StringBuilder sb = new StringBuilder();
            DictionaryEntry definition;
            List<string> words = new List<string>(50);
            int index;
            int count = inputString.Length;
            int startIndex;
            string str;
            string segment;
            bool containsNewlines = false;
            int columnCount = count;
            int maxChars = 10;
            char c;
            //char[] whiteSpace = LanguageLookup.SpaceCharacters;
            char[] spaceAndPunctuation = LanguageLookup.SpaceAndPunctuationCharacters;

            if (inputString.Contains("\n") || inputString.Contains("\r"))
                containsNewlines = true;

            outputString = "";

            for (startIndex = 0; startIndex < count; )
            {
                c = inputString[startIndex];

                if (spaceAndPunctuation.Contains(c) || ((c >= ' ') && (c <= '~')))
                {
                    sb.Append(inputString[startIndex]);
                    startIndex++;
                    continue;
                }

                definition = null;
                str = null;
                segment = null;

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

                if (columnCount > startIndex + maxChars)
                    columnCount = startIndex + maxChars;

                index = columnCount;

                for (int i = startIndex; i < columnCount; i++)
                {
                    c = inputString[i];

                    if (((c >= ' ') && (c <= '~')) || spaceAndPunctuation.Contains(c))
                    {
                        index = i;
                        break;
                    }
                }

                for (; index > startIndex; index--)
                {
                    str = inputString.Substring(startIndex, index - startIndex);

                    if ((definition = dictionary.Get(str, inputLanguage)) != null)
                    {
                        segment = definition.Senses[0].LanguageSynonyms[0].GetSynonymIndexed(0);
                        //segment = definition.GetDefinition(outputLanguage);
                        break;
                    }
                }

                if (!String.IsNullOrEmpty(segment))
                {
                    if (sb.Length != 0)
                        sb.Append(separator);
                    sb.Append(segment);
                }
                else if (!String.IsNullOrEmpty(str))
                {
                    sb.Append(str);
                }

                if (!String.IsNullOrEmpty(str))
                    startIndex += str.Length;
                else
                    startIndex += 1;
            }

            outputString = sb.ToString().Trim();

            count = LanguageLookup.FatPunctuationCharacters.Count();
            string[] fatSpaced = LanguageLookup.FatPunctuationWithSpaceCharacters;
            string[] fat = LanguageLookup.FatPunctuationCharacters;
            string[] thinSpaced = LanguageLookup.ThinPunctuationWithSpaceCharacters;

            for (index = 0; index < count; index++)
            {
                outputString = outputString.Replace(fatSpaced[index], thinSpaced[index]);
                outputString = outputString.Replace(fat[index], thinSpaced[index]);
            }
        }

        public static string InputTransliterateCheck(string str, LanguageID languageID)
        {
            string returnValue = str;

            switch (languageID.LanguageCultureExtensionCode)
            {
                case "zh--pn":
                    ConvertPinyinNumeric.ToToneMarksCheck(out returnValue, str);
                    break;
                default:
                    break;
            }

            return returnValue;
        }
    }
}
