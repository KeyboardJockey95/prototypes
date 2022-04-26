using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.Converters
{
    public class ConvertPinyin : ConvertRomanization
    {
        public bool IsNumeric { get; set; }

        public ConvertPinyin(LanguageID chineseLanguageID, bool isNumeric, DictionaryRepository dictionary, bool useQuickDictionary)
            : base(chineseLanguageID, LanguageLookup.ChinesePinyin, '\0', dictionary, useQuickDictionary)
        {
            IsNumeric = isNumeric;
        }

        public override bool To(out string output, string input)
        {
            ConvertToPinyin(NonRomanizationLanguageID, input, IsNumeric, out output, Dictionary, QuickDictionaryTo);
            return true;
        }

        public override bool From(out string output, string input)
        {
            ConvertCharacters(RomanizationLanguageID, NonRomanizationLanguageID, input, out output, Dictionary, QuickDictionaryFrom);
            return true;
        }

        public static void ConvertToPinyin(LanguageID inputLanguage, string inputString, bool toNumeric, out string outputString,
            DictionaryRepository dictionary, FormatQuickLookup quickDictionary)
        {
            StringBuilder sb = new StringBuilder();
            DictionaryEntry definition;
            LanguageString alternate;
            List<string> words = new List<string>(50);
            List<LanguageString> alternates;
            LanguageID pinyinID = LanguageLookup.ChinesePinyin;
            int index;
            int count = inputString.Length;
            int startIndex;
            string str;
            string pinyinText;
            bool containsNewlines = false;
            bool lastWasPinyin = false;
            int columnCount = count;
            int maxChars = 10;
            char c;

            if (inputString.Contains("\n") || inputString.Contains("\r"))
                containsNewlines = true;

            outputString = "";

            for (startIndex = 0; startIndex < count; )
            {
                c = inputString[startIndex];

                if (LanguageLookup.SpaceCharacters.Contains(c) || ((c >= ' ') && (c <= '~')) || LanguageLookup.PunctuationCharacters.Contains(c))
                {
                    //if (LanguageLookup.MatchedFatStartCharacters.Contains(c))
                    //    sb.Append(" ");
                    sb.Append(inputString[startIndex]);
                    //if (LanguageLookup.MatchedFatEndCharacters.Contains(c))
                    //    sb.Append(" ");
                    startIndex++;
                    lastWasPinyin = false;
                    continue;
                }

                definition = null;
                str = null;
                pinyinText = null;

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

                    if (((c >= ' ') && (c <= '~')) || LanguageLookup.SpaceCharacters.Contains(c) || LanguageLookup.PunctuationCharacters.Contains(c))
                    {
                        index = i;
                        break;
                    }
                }

                if (quickDictionary != null)
                {
                    int saveIndex = index;

                    for (; index > startIndex; index--)
                    {
                        str = inputString.Substring(startIndex, index - startIndex);

                        if (quickDictionary.QuickDictionary.TryGetValue(str, out pinyinText))
                            break;
                    }
                }
                else
                {
                    for (; index > startIndex; index--)
                    {
                        str = inputString.Substring(startIndex, index - startIndex);

                        if ((definition = dictionary.Get(str, inputLanguage)) != null)
                        {
                            alternates = definition.Alternates;

                            if ((alternates != null) && (alternates.Count() != 0))
                            {
                                alternate = alternates.FirstOrDefault(x => x.LanguageID == pinyinID);

                                if (alternate != null)
                                {
                                    pinyinText = alternate.Text;

                                    if (!String.IsNullOrEmpty(pinyinText))
                                    {
                                        ConvertPinyinNumeric.Display(out pinyinText, pinyinText, pinyinID, toNumeric, dictionary, quickDictionary);

                                        // Done in ConvertPinyinNumeric.Display.
                                        //if (!toNumeric)
                                        //    ConvertPinyinNumeric.ToToneMarks(out pinyinText, pinyinText);

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(pinyinText))
                {
                    if (lastWasPinyin && (sb.Length != 0))
                        sb.Append(" ");

                    sb.Append(pinyinText);
                    lastWasPinyin = true;
                }
                else if (!String.IsNullOrEmpty(str))
                {
                    sb.Append(str);
                    lastWasPinyin = false;
                }

                if (!String.IsNullOrEmpty(str))
                    startIndex += str.Length;
                else
                    startIndex += 1;
            }

            outputString = sb.ToString().Trim();

            outputString = outputString.Replace(LanguageLookup.ZeroWidthSpaceString, LanguageLookup.ZeroWidthSpaceStringWithSpace);

            count = LanguageLookup.PunctuationCharacters.Count();

            for (index = 0; index < count; index++)
            {
                string punct = LanguageLookup.PunctuationCharacters[index].ToString();
                string punctWithSpace = LanguageLookup.ZeroWidthSpaceStringWithSpace + punct;
                string punctWithoutspace = LanguageLookup.ZeroWidthSpaceString + punct;
                outputString = outputString.Replace(
                    punctWithSpace,
                    punctWithoutspace);
            }

            count = LanguageLookup.FatPunctuationCharacters.Count();

            for (index = 0; index < count; index++)
            {
                outputString = outputString.Replace(
                    LanguageLookup.FatPunctuationWithSpaceCharacters[index],
                    LanguageLookup.ThinPunctuationWithSpaceCharacters[index]);
                outputString = outputString.Replace(
                    LanguageLookup.FatPunctuationCharacters[index],
                    LanguageLookup.ThinPunctuationWithSpaceCharacters[index]);
            }

            outputString = outputString.TrimEnd();
        }

        public static void ConvertCharacters(LanguageID inputLanguage, LanguageID outputLanguage,
            string inputString, out string outputString, DictionaryRepository dictionary, FormatQuickLookup quickDictionary)
        {
            StringBuilder sb = new StringBuilder();
            DictionaryEntry definition;
            List<LanguageString> alternates;
            LanguageString alternate;
            int characterIndex;
            int characterCount = inputString.Length;
            string characterStringInput;
            string characterStringOutput;

            outputString = "";

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
        }
    }
}
