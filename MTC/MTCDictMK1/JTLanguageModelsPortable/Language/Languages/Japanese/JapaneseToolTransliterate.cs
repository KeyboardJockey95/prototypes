using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Matchers;

namespace JTLanguageModelsPortable.Language
{
    public partial class JapaneseTool : LanguageTool
    {
        public override bool IsNumberString(string str)
        {
            if (String.IsNullOrEmpty(str))
                return false;

            if (str.StartsWith("-"))
                str = str.Substring(1);

            bool first = true;
            char last = str[str.Length - 1];

            foreach (char c in str)
            {
                if ((c == '.') || (c == ','))
                    continue;

                if (((c == 'e') || (c == 'E')) && !first && (c != last))
                    continue;

                first = false;

                if (!IsNumberCharacter(c))
                    return base.IsNumberString(str);
            }

            return true;
        }

        public override bool IsNumberCharacter(char chr)
        {
            switch (chr)
            {
                case '零':
                case '〇':
                case '一':
                case '二':
                case '三':
                case '四':
                case '五':
                case '六':
                case '七':
                case '八':
                case '九':
                case '十':
                case '百':
                case '千':
                case '万':
                case '億':
                case '兆':
                case '京':
                    return true;
                default:
                    return base.IsNumberCharacter(chr);
            }
        }

        public override string TranslateNumber(LanguageID languageID, string text)
        {
            if (languageID == EnglishID)
                return TranslateNumberToEnglish(text);
            else if (languageID == JapaneseRomajiID)
                return TranslateNumberToRomaji(text, false);
            else if (languageID == JapaneseKanaID)
                return TranslateNumberToKana(text);
            else
                return base.TranslateNumber(languageID, text);
        }

        public string TranslateNumberToEnglish(string text)
        {
            StringBuilder sb = new StringBuilder();
            long value1 = 0;
            long value10 = 0;
            long value100 = 0;
            long value1000 = 0;
            long value10000 = 0;
            long value100000000 = 0;
            long value1000000000000 = 0;
            long value10000000000000000 = 0;
            long valueInt = 0;

            if (String.IsNullOrEmpty(text))
                return text;

            foreach (char chr in text)
            {
                switch (chr)
                {
                    case '零':
                    case '〇':
                        break;
                    case '一':
                        value1 = 1;
                        break;
                    case '二':
                        value1 = 2;
                        break;
                    case '三':
                        value1 = 3;
                        break;
                    case '四':
                        value1 = 4;
                        break;
                    case '五':
                        value1 = 5;
                        break;
                    case '六':
                        value1 = 6;
                        break;
                    case '七':
                        value1 = 7;
                        break;
                    case '八':
                        value1 = 8;
                        break;
                    case '九':
                        value1 = 9;
                        break;
                    case '十':
                        if (value1 != 0)
                            value10 = 10 * value1;
                        else
                            value10 = 10;
                        value1 = 0;
                        break;
                    case '百':
                        if (value1 != 0)
                            value100 = 100 * value1;
                        else
                            value100 = 100;
                        value1 = 0;
                        break;
                    case '千':
                        if (value1 != 0)
                            value1000 = 1000 * value1;
                        else
                            value1000 = 1000;
                        value1 = 0;
                        break;
                    case '万':
                        valueInt = value1000 + value100 + value10 + value1;
                        if (valueInt != 0)
                            value10000 += 10000 * valueInt;
                        else
                            value10000 += 10000;
                        value1000 = value100 = value10 = value1 = 0;
                        break;
                    case '億':
                        valueInt = value10000 + value1000 + value100 + value10 + value1;
                        if (valueInt != 0)
                            value100000000 = 100000000 * valueInt;
                        else
                            value100000000 = 100000000;
                        value10000 = value1000 = value100 = value10 = value1 = 0;
                        break;
                    case '兆':
                        valueInt = value10000 + value1000 + value100 + value10 + value1;
                        if (valueInt != 0)
                            value1000000000000 = 1000000000000 * valueInt;
                        else
                            value1000000000000 = 1000000000000;
                        value10000 = value1000 = value100 = value10 = value1 = 0;
                        break;
                    case '京':
                        valueInt = value10000 + value1000 + value100 + value10 + value1;
                        if (valueInt != 0)
                            value10000000000000000 = 10000000000000000 * valueInt;
                        else
                            value10000000000000000 = 10000000000000000;
                        value10000 = value1000 = value100 = value10 = value1 = 0;
                        break;
                    default:
                        valueInt = value10000000000000000 + value1000000000000 + value100000000 +
                            value10000 + value1000 + value100 + value10 + value1;
                        if (valueInt != 0)
                            sb.Append(valueInt.ToString());
                        value10000000000000000 = value1000000000000 = value100000000 = value10000 =
                            value1000 = value100 = value10 = value1 = 0;
                        sb.Append(chr);
                        break;
                }
            }

            valueInt = value10000000000000000 + value1000000000000 + value100000000 +
                value10000 + value1000 + value100 + value10 + value1;

            if (valueInt != 0)
                sb.Append(valueInt.ToString());

            if ((valueInt == 0) && (sb.Length == 0))
                return "0";

            return sb.ToString();
        }


        // Punts on reading changes.
        public string TranslateNumberToRomaji(string text, bool insertSpaces)
        {
            StringBuilder sb = new StringBuilder();

            if (String.IsNullOrEmpty(text))
                return text;

            foreach (char chr in text)
            {
                switch (chr)
                {
                    case '零':
                    case '〇':
                        if (insertSpaces && (sb.Length != 0))
                            sb.Append(" ");
                        sb.Append("rei");
                        break;
                    case '一':
                        if (insertSpaces && (sb.Length != 0))
                            sb.Append(" ");
                        sb.Append("ichi");
                        break;
                    case '二':
                        if (insertSpaces && (sb.Length != 0))
                            sb.Append(" ");
                        sb.Append("ni");
                        break;
                    case '三':
                        if (insertSpaces && (sb.Length != 0))
                            sb.Append(" ");
                        sb.Append("san");
                        break;
                    case '四':
                        if (insertSpaces && (sb.Length != 0))
                            sb.Append(" ");
                        sb.Append("yon");
                        break;
                    case '五':
                        if (insertSpaces && (sb.Length != 0))
                            sb.Append(" ");
                        sb.Append("go");
                        break;
                    case '六':
                        if (insertSpaces && (sb.Length != 0))
                            sb.Append(" ");
                        sb.Append("roku");
                        break;
                    case '七':
                        if (insertSpaces && (sb.Length != 0))
                            sb.Append(" ");
                        sb.Append("nana");
                        break;
                    case '八':
                        if (insertSpaces && (sb.Length != 0))
                            sb.Append(" ");
                        sb.Append("hachi");
                        break;
                    case '九':
                        if (insertSpaces && (sb.Length != 0))
                            sb.Append(" ");
                        sb.Append("kyū");
                        break;
                    case '十':
                        if (insertSpaces && (sb.Length != 0))
                            sb.Append(" ");
                        sb.Append("jū");
                        break;
                    case '百':
                        if (insertSpaces && (sb.Length != 0))
                            sb.Append(" ");
                        sb.Append("hyaku");
                        break;
                    case '千':
                        if (insertSpaces && (sb.Length != 0))
                            sb.Append(" ");
                        sb.Append("sen");
                        break;
                    case '万':
                        if (insertSpaces && (sb.Length != 0))
                            sb.Append(" ");
                        sb.Append("man");
                        break;
                    case '億':
                        if (insertSpaces && (sb.Length != 0))
                            sb.Append(" ");
                        sb.Append("oku");
                        break;
                    case '兆':
                        if (insertSpaces && (sb.Length != 0))
                            sb.Append(" ");
                        sb.Append("chō");
                        break;
                    case '京':
                        if (insertSpaces && (sb.Length != 0))
                            sb.Append(" ");
                        sb.Append("kei");
                        break;
                    default:
                        sb.Append(chr);
                        break;
                }
            }

            return FixUpRomajiNumber(sb.ToString());
        }

        public override bool ConvertNumberToRomanized(string text, out string romanized)
        {
            romanized = TranslateNumberToRomaji(text, true);
            return !String.IsNullOrEmpty(romanized);
        }

        // Punts on reading changes.
        public string TranslateNumberToKana(string text)
        {
            StringBuilder sb = new StringBuilder();

            if (String.IsNullOrEmpty(text))
                return text;

            foreach (char chr in text)
            {
                switch (chr)
                {
                    case '零':
                    case '〇':
                        sb.Append("れい");
                        break;
                    case '一':
                        sb.Append("いち");
                        break;
                    case '二':
                        sb.Append("に");
                        break;
                    case '三':
                        sb.Append("さん");
                        break;
                    case '四':
                        sb.Append("よん");
                        break;
                    case '五':
                        sb.Append("ご");
                        break;
                    case '六':
                        sb.Append("ろく");
                        break;
                    case '七':
                        sb.Append("なな");
                        break;
                    case '八':
                        sb.Append("はち");
                        break;
                    case '九':
                        sb.Append("きゅ");
                        break;
                    case '十':
                        sb.Append("じゅう");
                        break;
                    case '百':
                        sb.Append("ひゃく");
                        break;
                    case '千':
                        sb.Append("せん");
                        break;
                    case '万':
                        sb.Append("まん");
                        break;
                    case '億':
                        sb.Append("おく");
                        break;
                    case '兆':
                        sb.Append("ちょう");
                        break;
                    case '京':
                        sb.Append("けい");
                        break;
                    default:
                        sb.Append(chr);
                        break;
                }
            }

            return FixUpKanaNumber(sb.ToString());
        }

        public static string[] RomajiNumberConversions =
        {
            "sanhyaku", "sanbyaku",
            "rokuhyaku", "roppyaku",
            "hachihyaku", "happyaku",
            "sansen", "sanzen",
            "hachichō", "hatchō",
            "jūchō", "jutchō",
            "rokukei", "rokkei",
            "hachikei", "hakkei",
            "jūkei", "jukkei",
            "hyakukei", "hyakkei"
        };

        public string FixUpRomajiNumber(string text)
        {
            string returnValue = TextUtilities.ReplaceSubStrings(text, RomajiNumberConversions);
            return returnValue;
        }

        public static string[] KanaNumberConversions =
        {
            "さんひゃく", "さんびゃく",
            "ろくひゃく", "ろっぴゃく",
            "はちひゃく", "はっぴゃく",
            "さんせん", "さんぜん",
            "はちちょう", "はっちょう",
            "じゅうちょう", "じゅっちょう",
            "ろくけい", "ろっけい",
            "はちけい", "はっけい",
            "じゅうけい", "じゅっけい",
            "ひゃくけい", "ひゃっけい"
        };

        public string FixUpKanaNumber(string text)
        {
            string returnValue = TextUtilities.ReplaceSubStrings(text, KanaNumberConversions);
            return returnValue;
        }

        public override string FixUpText(LanguageID languageID, string text)
        {
            string returnValue = text;

            if (String.IsNullOrEmpty(text))
                return text;

            if (languageID == JapaneseKanaID)
            {
                if (RomajiConverter.IsAllPhonetic(text))
                    return returnValue;

                StringBuilder sb = new StringBuilder();
                int length = text.Length;
                int index;

                for (index = 0; index < length; index++)
                {
                    char chr = text[index];

                    switch (chr)
                    {
                        case '零':
                        case '〇':
                            sb.Append("れい");
                            break;
                        case '一':
                            sb.Append("いち");
                            break;
                        case '二':
                            sb.Append("に");
                            break;
                        case '三':
                            sb.Append("さん");
                            break;
                        case '四':
                            sb.Append("よん");
                            break;
                        case '五':
                            sb.Append("ご");
                            break;
                        case '六':
                            sb.Append("ろく");
                            break;
                        case '七':
                            sb.Append("なな");
                            break;
                        case '八':
                            sb.Append("はち");
                            break;
                        case '九':
                            sb.Append("きゅ");
                            break;
                        case '十':
                            sb.Append("じゅう");
                            break;
                        case '百':
                            sb.Append("ひゃく");
                            break;
                        case '千':
                            sb.Append("せん");
                            break;
                        case '万':
                            sb.Append("まん");
                            break;
                        case '億':
                            sb.Append("おく");
                            break;
                        case '兆':
                            sb.Append("ちょう");
                            break;
                        case '京':
                            sb.Append("けい");
                            break;
                        default:
                            sb.Append(chr);
                            break;
                    }
                }

                returnValue = sb.ToString();

                if (!RomajiConverter.IsAllPhonetic(returnValue))
                {
                    RomajiConverter.ConvertToCharacters(
                        JapaneseID,
                        JapaneseKanaID,
                        '\0',
                        returnValue,
                        out returnValue,
                        DictionaryDatabase,
                        null);
                }

                returnValue = FixUpKanaNumber(returnValue);
            }

            return returnValue;
        }

        public static List<string> JapaneseRomajiEmbeddedFixupStrings = new List<string>()
        {
            " ha ",
            " wa ",
            " wo ",
            " o ",
            " he ",
            " e ",
            "\x200B",
            " "
        };

        public static List<string> JapaneseRomajiFullFixupStrings = new List<string>()
        {
            "ha",
            "wa",
            "wo",
            "o",
            "he",
            "e"
        };

        public static List<string> JapaneseCompoundsToSeparate = new List<string>()
        {
            "これは",
            "koreha",
            "それは",
            "soreha",
            "あれは",
            "areha"
        };

        public static List<string> JapaneseCompoundsToNotSeparate = new List<string>()
        {
            "わたしたち",
            "watashitachi"
        };

        public override void InitializeWordFixes(string wordFixesKey)
        {
            if (!String.IsNullOrEmpty(wordFixesKey))
            {
                string wordFixesFilePath = WordFixes.GetFilePath(wordFixesKey, null);
                WordFixes.CreateAndLoad(wordFixesFilePath, out _WordFixes);
            }
            else
                WordFixes = new WordFixes();

            InitializeDefaultWordFixes();
        }

        protected override void InitializeDefaultWordFixes()
        {
            WordFixes.AddEmbeddedConversions(JapaneseRomajiEmbeddedFixupStrings);
            WordFixes.AddFullConversions(JapaneseRomajiFullFixupStrings);
            WordFixes.AddCompoundsToSeparate(JapaneseCompoundsToSeparate);
            WordFixes.AddCompoundsToNotSeparate(JapaneseCompoundsToNotSeparate);
        }

        public override bool TransliterateMultiLanguageItem(
            MultiLanguageItem multiLanguageItem,
            bool force)
        {
            if (force)
                return base.TransliterateMultiLanguageItem(multiLanguageItem, force);

            LanguageItem languageItemKana = multiLanguageItem.LanguageItem(JapaneseKanaID);

            if ((languageItemKana == null) || !languageItemKana.HasText() || !languageItemKana.HasWordRuns())
            {
                if (languageItemKana == null)
                    return base.TransliterateMultiLanguageItem(multiLanguageItem, force);
                else
                {
                    if (!base.TransliterateLanguageItem(multiLanguageItem, languageItemKana, false))
                        return false;
                }
            }

            LanguageItem languageItemRomaji = multiLanguageItem.LanguageItem(JapaneseRomajiID);
            bool returnValue = true;

            if (languageItemRomaji == null)
            {
                languageItemRomaji = new LanguageItem(multiLanguageItem.Key, JapaneseRomajiID, String.Empty);
                multiLanguageItem.Add(languageItemRomaji);
            }
            else if (languageItemRomaji.HasText())
                return true;

            string sourceKana = languageItemKana.Text;
            List<TextRun> wordRunsKana = languageItemKana.WordRuns;
            List<TextRun> wordRunsRomaji = languageItemRomaji.WordRuns;

            if (wordRunsRomaji != null)
                wordRunsRomaji.Clear();
            else
                languageItemRomaji.WordRuns = wordRunsRomaji = new List<TextRun>();

            StringBuilder sb = new StringBuilder();
            int textIndex = 0;

            foreach (TextRun wordRunKana in wordRunsKana)
            {
                if (textIndex > wordRunKana.Start)
                    throw new Exception("Oops, parsed nodes in Transliterate are out of sync.");

                if (wordRunKana.Start > textIndex)
                {
                    string separatorsInput = sourceKana.Substring(textIndex, wordRunKana.Start - textIndex);
                    string separatorsOutput = TransliterateSeparatorsOrUnknown(
                        separatorsInput,
                        JapaneseKanaID,
                        JapaneseRomajiID,
                        sb,
                        true);
                    sb.Append(separatorsOutput);
                    textIndex += separatorsInput.Length;
                }
                else if (textIndex != 0)
                    sb.Append(" ");

                string wordKana = sourceKana.Substring(wordRunKana.Start, wordRunKana.Length);
                string wordRomaji;

                if (!RomajiConverter.ToTable(out wordRomaji, wordKana))
                {
                    if (!RomajiConverter.ConvertToRomanizationRun(
                            JapaneseKanaID,
                            JapaneseRomajiID,
                            ' ',
                            wordKana,
                            out wordRomaji,
                            DictionaryDatabase,
                            null,
                            this,
                            true))
                        wordRomaji = wordKana;
                }

                wordRomaji = FixupTransliteration(
                    wordRomaji,
                    JapaneseRomajiID,
                    wordKana,
                    JapaneseKanaID,
                    true);

                wordRomaji = FixUpText(JapaneseRomajiID, wordRomaji);

                TextRun wordRunRomaji = new TextRun(sb.Length, wordRomaji.Length, null);
                wordRunsRomaji.Add(wordRunRomaji);

                sb.Append(wordRomaji);
                textIndex += wordRunKana.Length;
            }

            if (textIndex < sourceKana.Length)
            {
                string separatorsInput = sourceKana.Substring(textIndex, sourceKana.Length - textIndex);
                string separatorsOutput = TransliterateSeparatorsOrUnknown(
                    separatorsInput,
                    JapaneseKanaID,
                    JapaneseRomajiID,
                    sb,
                    false);
                sb.Append(separatorsOutput);
            }

            languageItemRomaji.Text = sb.ToString();
            languageItemRomaji.ReDoSentenceRuns(languageItemKana);

            return returnValue;
        }

        public override string TransliterateSeparatorsOrUnknown(
            string input,
            LanguageID inputLanguageID,
            LanguageID outputLanguageID,
            StringBuilder sb,
            bool isNotEnd)
        {
            string output;

            if (LanguageLookup.IsRomanized(outputLanguageID))
            {
                if (LanguageLookup.IsSpaceOrPunctuation(input))
                    return TransliterateSeparators(input, inputLanguageID, outputLanguageID, sb, isNotEnd);
                else
                {
                    if (RomajiConverter.To(out output, input))
                    {
                        output = TransliterateSeparatorsExpandCheck(output, sb, isNotEnd);

                        output = FixupTransliteration(
                            output,
                            JapaneseRomajiID,
                            input,
                            JapaneseKanaID,
                            true);

                        output = FixUpText(JapaneseRomajiID, output);
                    }
                    else
                        return TransliterateSeparators(input, inputLanguageID, outputLanguageID, sb, isNotEnd);
                }
            }
            else
                output = input;

            return output;
        }

        public override string FixupTransliteration(
            string transliteration,
            LanguageID transliterationLanguageID,
            string nonTransliteration,
            LanguageID nonTransliterationLanguageID,
            bool isWord)
        {
            if ((transliterationLanguageID == LanguageLookup.JapaneseRomaji) || (transliterationLanguageID == LanguageLookup.JapaneseKana))
            {
                InitializeWordFixesCheck(null);
                transliteration = WordFixes.Convert(transliteration);
            }

            return transliteration;
        }

        public override string CharacterConvertLanguageText(string text, LanguageID languageID)
        {
            if (languageID == LanguageLookup.JapaneseKana)
            {
                if (!ConvertRomaji.IsAllKanaOrRomaji(text))
                {
                    if (CharacterConverter == null)
                        CharacterConverter = new ConvertTransliterate(
                            JapaneseID,
                            JapaneseKanaID,
                            '\0',
                            DictionaryDatabase,
                            true);

                    CharacterConverter.To(out text, text);
                }
            }

            return text;
        }

        public override bool IsRomanizedChar(char chr)
        {
            if (ConvertRomaji.IsRomaji(chr))
                return true;

            return false;
        }

        public override bool IsAlternateTransliterationChar(char chr)
        {
            if (ConvertRomaji.IsKana(chr))
                return true;

            return false;
        }

        public override string TransliterateUnknown(
            string input,
            LanguageID inputLanguageID,
            LanguageID outputLanguageID)
        {
            string output;

            if (outputLanguageID == JapaneseRomajiID)
                RomajiConverter.ToTable(out output, input);
            else
                output = input;

            return output;
        }

        protected string ConvertKanaCharacterToRomaji(string kanaChar)
        {
            string romaji;

            if (String.IsNullOrEmpty(kanaChar) || (kanaChar.Length != 1))
                return String.Empty;

            if (RomajiConverter.ConvertCharacterToRomanization(kanaChar[0], out romaji))
                return romaji;

            return String.Empty;
        }

        public override bool ConvertAlternateToRomanized(string text, out string romanized)
        {
            StringBuilder sb = new StringBuilder();

            if (String.IsNullOrEmpty(text))
            {
                romanized = String.Empty;
                return true;
            }

            bool returnValue = true;

            foreach (char c in text)
            {
                string rom = ConvertKanaCharacterToRomaji(c.ToString());

                if (String.IsNullOrEmpty(rom))
                {
                    sb.Append(c);
                    returnValue = false;
                }
                else
                    sb.Append(rom);
            }

            romanized = sb.ToString();

            return returnValue;
        }

        public override bool MatchCharFuzzy(
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
                else
                {
                    bool isPatternLong = false;
                    bool isPatternWo = false;
                    bool isPatternWa = false;
                    bool isPatternHa = false;
                    bool isPatternHe = false;

                    switch (patternChar)
                    {
                        case 'ā':
                        case 'ī':
                        case 'ē':
                        case 'ō':
                        case 'ū':
                            isPatternLong = true;
                            break;
                        case 'w':
                            if (patternIndex < patternLength - 1)
                            {
                                char nextChr = char.ToLower(pattern[patternIndex + 1]);
                                if (nextChr == 'o')
                                    isPatternWo = true;
                                else if (nextChr == 'a')
                                    isPatternWa = true;
                            }
                            break;
                        case 'h':
                            if (patternIndex < patternLength - 1)
                            {
                                char nextChr = char.ToLower(pattern[patternIndex + 1]);
                                if (nextChr == 'a')
                                    isPatternHa = true;
                                else if (nextChr == 'e')
                                    isPatternHe = true;
                            }
                            break;
                        default:
                            break;
                    }

                    bool isTextLong = false;
                    bool isTextWo = false;
                    bool isTextWa = false;
                    bool isTextHa = false;
                    bool isTextHe = false;

                    switch (textChar)
                    {
                        case 'ā':
                        case 'ī':
                        case 'ē':
                        case 'ō':
                        case 'ū':
                            isTextLong = true;
                            break;
                        case 'w':
                            if (textIndex < textLength - 1)
                            {
                                char nextChr = char.ToLower(text[textIndex + 1]);
                                if (nextChr == 'o')
                                    isTextWo = true;
                                else if (nextChr == 'a')
                                    isTextWa = true;
                            }
                            break;
                        case 'h':
                            if (textIndex < textLength - 1)
                            {
                                char nextChr = char.ToLower(text[textIndex + 1]);
                                if (nextChr == 'a')
                                    isTextHa = true;
                                else if (nextChr == 'e')
                                    isTextHe = true;
                            }
                            break;
                        default:
                            break;
                    }

                    if (isPatternLong)
                    {
                        if (!isTextLong)
                        {
                            if (textIndex < textLength - 1)
                            {
                                switch (patternChar)
                                {
                                    case 'ā':
                                        if ((char.ToLower(text[textIndex]) == 'a') &&
                                            (char.ToLower(text[textIndex + 1]) == 'a'))
                                        {
                                            patternIndex++;
                                            textIndex += 2;
                                            returnValue = true;
                                        }
                                        break;
                                    case 'ī':
                                        if ((char.ToLower(text[textIndex]) == 'i') &&
                                            (char.ToLower(text[textIndex + 1]) == 'i'))
                                        {
                                            patternIndex++;
                                            textIndex += 2;
                                            returnValue = true;
                                        }
                                        break;
                                    case 'ē':
                                        if ((char.ToLower(text[textIndex]) == 'e') &&
                                            (char.ToLower(text[textIndex + 1]) == 'i'))
                                        {
                                            patternIndex++;
                                            textIndex += 2;
                                            returnValue = true;
                                        }
                                        break;
                                    case 'ō':
                                        if ((char.ToLower(text[textIndex]) == 'o') &&
                                            ((char.ToLower(text[textIndex + 1]) == 'o') ||
                                                (char.ToLower(text[textIndex + 1]) == 'u')))
                                        {
                                            patternIndex++;
                                            textIndex += 2;
                                            returnValue = true;
                                        }
                                        break;
                                    case 'ū':
                                        if ((char.ToLower(text[textIndex]) == 'u') &&
                                            (char.ToLower(text[textIndex + 1]) == 'u'))
                                        {
                                            patternIndex++;
                                            textIndex += 2;
                                            returnValue = true;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    else if (isTextLong)
                    {
                        if (!isPatternLong)
                        {
                            if (patternIndex < patternLength - 1)
                            {
                                switch (textChar)
                                {
                                    case 'ā':
                                        if ((char.ToLower(pattern[patternIndex]) == 'a') &&
                                            (char.ToLower(pattern[patternIndex + 1]) == 'a'))
                                        {
                                            patternIndex += 2;
                                            textIndex++;
                                            returnValue = true;
                                        }
                                        break;
                                    case 'ī':
                                        if ((char.ToLower(pattern[patternIndex]) == 'i') &&
                                            (char.ToLower(pattern[patternIndex + 1]) == 'i'))
                                        {
                                            patternIndex += 2;
                                            textIndex++;
                                            returnValue = true;
                                        }
                                        break;
                                    case 'ē':
                                        if ((char.ToLower(pattern[patternIndex]) == 'e') &&
                                            (char.ToLower(pattern[patternIndex + 1]) == 'i'))
                                        {
                                            patternIndex += 2;
                                            textIndex++;
                                            returnValue = true;
                                        }
                                        break;
                                    case 'ō':
                                        if ((char.ToLower(pattern[patternIndex]) == 'o') &&
                                            ((char.ToLower(pattern[patternIndex + 1]) == 'o') ||
                                                (char.ToLower(pattern[patternIndex + 1]) == 'u')))
                                        {
                                            patternIndex += 2;
                                            textIndex++;
                                            returnValue = true;
                                        }
                                        break;
                                    case 'ū':
                                        if ((char.ToLower(pattern[patternIndex]) == 'u') &&
                                            (char.ToLower(pattern[patternIndex + 1]) == 'u'))
                                        {
                                            patternIndex += 2;
                                            textIndex++;
                                            returnValue = true;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    else if (isPatternHa)
                    {
                        if (isTextWa)
                        {
                            patternIndex += 2;
                            textIndex += 2;
                            returnValue = true;
                        }
                    }
                    else if (isPatternHe)
                    {
                        if (textChar == 'e')
                        {
                            patternIndex += 2;
                            textIndex += 1;
                            returnValue = true;
                        }
                    }
                    else if (isPatternWa)
                    {
                        if (isTextHa)
                        {
                            patternIndex += 2;
                            textIndex += 2;
                            returnValue = true;
                        }
                    }
                    else if (isPatternWo)
                    {
                        if (textChar == 'o')
                        {
                            patternIndex += 2;
                            textIndex += 1;
                            returnValue = true;
                        }
                    }
                    else if (isTextHa)
                    {
                        if (isPatternWa)
                        {
                            patternIndex += 2;
                            textIndex += 2;
                            returnValue = true;
                        }
                    }
                    else if (isTextHe)
                    {
                        if (patternChar == 'e')
                        {
                            patternIndex += 1;
                            textIndex += 2;
                            returnValue = true;
                        }
                    }
                    else if (isTextWa)
                    {
                        if (isPatternHa)
                        {
                            patternIndex += 2;
                            textIndex += 2;
                            returnValue = true;
                        }
                    }
                    else if (isTextWo)
                    {
                        if (patternChar == 'o')
                        {
                            patternIndex += 1;
                            textIndex += 2;
                            returnValue = true;
                        }
                    }
                }
            }

            return returnValue;
        }

        public string ExpandRomaji(string text)
        {
            int len = text.Length;
            bool haveLongVowels = false;

            for (int i = 0; (i < len) && !haveLongVowels; i++)
            {
                switch (text[i])
                {
                    case 'ā':
                    case 'ī':
                    case 'ē':
                    case 'ō':
                    case 'ū':
                        haveLongVowels = true;
                        break;
                }
            }

            if (!haveLongVowels)
                return text;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < len; i++)
            {
                char chr = text[i];

                switch (chr)
                {
                    case 'ā':
                        sb.Append("aa");
                        break;
                    case 'ī':
                        sb.Append("ii");
                        break;
                    case 'ē':
                        sb.Append("ei");
                        break;
                    case 'ō':
                        sb.Append("ou");
                        break;
                    case 'ū':
                        sb.Append("uu");
                        break;
                    default:
                        sb.Append(chr);
                        break;
                }
            }

            return sb.ToString();
        }
    }
}
