using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public class ChineseTool : LanguageTool
    {
        public static LanguageID ChineseTraditionalID = LanguageLookup.ChineseTraditional;
        public static LanguageID ChineseSimplifiedID = LanguageLookup.ChineseSimplified;
        public static LanguageID ChinesePinyinID = LanguageLookup.ChinesePinyin;
        public static LanguageID EnglishID = LanguageLookup.English;
        public static List<LanguageID> ChineseLanguageIDs = new List<LanguageID>()
            {
                ChineseTraditionalID,
                ChineseSimplifiedID,
                ChinesePinyinID
            };
        protected ConvertTransliterate CharacterConverter;

        public ChineseTool() : base(LanguageLookup.ChineseTraditional)
        {
            _TargetLanguageIDs = ChineseLanguageIDs;
            _HostLanguageIDs = new List<LanguageID>() { LanguageLookup.English };
            CharacterConverter = null;
        }

        public override IBaseObject Clone()
        {
            return new ChineseTool();
        }

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

        public override string FixupTransliteration(
            string transliteration,
            LanguageID transliterationLanguageID,
            string nonTransliteration,
            LanguageID nonTransliterationLanguageID,
            bool isWord)
        {
            if (transliterationLanguageID == LanguageLookup.ChinesePinyin)
            {
                if (WordFixes == null)
                    InitializeWordFixes(null);

                transliteration = WordFixes.Convert(transliteration);

                if (isWord && (transliteration.Length > 0))
                {
                    // Remove spaces following unknown syllables.
                    transliteration = transliteration.Replace(" ", "");
                }
            }

            return transliteration;
        }

        public override bool ParseForeignWord(string text, int textIndex, int maxLength, out string word)
        {
            word = null;
            if (String.IsNullOrEmpty(text))
                return false;
            int textLength = text.Length;
            int index = textIndex;
            if ((maxLength <= 0) || (textLength <= 0) || (textIndex >= textLength))
                return false;
            char chr = text[index++];
            int maxIndex = textIndex + maxLength;
            if (maxIndex > textLength)
                maxIndex = textLength;
            if (IsPinyinChar(chr))
            {
                for (; index < maxIndex; index++)
                {
                    chr = text[index];
                    if (char.IsWhiteSpace(chr) || char.IsPunctuation(chr) ||
                        !IsPinyinChar(chr))
                    {
                        word = text.Substring(textIndex, index - textIndex);
                        return true;
                    }
                }
            }
            else if (index < maxIndex)
                index++;
            word = text.Substring(textIndex, index - textIndex);
            return true;
        }

        public List<char> ToneMarkedCharacters = new List<char>()
        {
            'ā',
            'á',
            'ǎ',
            'à',
            'ē',
            'é',
            'ě',
            'è',
            'ī',
            'í',
            'ǐ',
            'ì',
            'ō',
            'ó',
            'ǒ',
            'ò',
            'ū',
            'ú',
            'ǔ',
            'ù',
            'ǖ',
            'ǘ',
            'ǚ',
            'ǜ'
        };

        public bool IsPinyinChar(char chr)
        {
            if (((chr >= 'A') && (chr <= 'Z')) || ((chr >= 'a') && (chr <= 'z')))
                return true;

            if (ToneMarkedCharacters.Contains(char.ToLower(chr)))
                return true;

            return false;
        }

        public override string DisplayConversionCheck(
            string input,
            LanguageID languageID,
            string pattern)
        {
            if (!String.IsNullOrEmpty(pattern) &&
                !ConvertPinyinNumeric.IsNumeric(pattern) &&
                ConvertPinyinNumeric.IsNumeric(input))
            {
                string newInput = input;
                ConvertPinyinNumeric.ToToneMarks(out input, newInput);
            }

            return input;
        }

        public override string CharacterConvertLanguageText(string text, LanguageID languageID)
        {
            if ((languageID == ChineseSimplifiedID) || (languageID == ChineseTraditionalID))
            {
                if (CharacterConverter == null)
                    CharacterConverter = new ConvertTransliterate(
                        ChineseTraditionalID,
                        ChineseSimplifiedID,
                        '\0',
                        DictionaryDatabase,
                        true);

                if (languageID == ChineseSimplifiedID)
                    CharacterConverter.To(out text, text);
                else
                    CharacterConverter.From(out text, text);
            }

            return text;
        }
    }
}
