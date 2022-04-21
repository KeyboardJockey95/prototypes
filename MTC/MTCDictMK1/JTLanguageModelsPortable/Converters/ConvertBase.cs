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
    public class ConvertEntry
    {
        public string In { get; set; }
        public string Out { get; set; }

        public ConvertEntry(string input, string output)
        {
            In = input;
            Out = output;
        }

        public ConvertEntry()
        {
            In = null;
            Out = null;
        }
    }

    public class ConvertBase
    {
        public List<ConvertEntry> Table { get; set; }

        public static bool ConvertTo(out string output, string input, List<ConvertEntry> table)
        {
            output = input;

            foreach (ConvertEntry entry in table)
                output = output.Replace(entry.In, entry.Out);

            output = output.Replace("\x200b", "");
            return true;
        }

        public static bool ConvertFrom(out string output, string input, List<ConvertEntry> table)
        {
            output = input;

            foreach (ConvertEntry entry in table)
                output = output.Replace(entry.Out, entry.In);

            output = output.Replace("\x200b", "");
            return true;
        }

        public virtual bool To(out string output, string input)
        {
            return ConvertTo(out output, input, Table);
        }

        public virtual bool From(out string output, string input)
        {
            return ConvertFrom(out output, input, Table);
        }

        public ConvertBase(List<ConvertEntry> table)
        {
            Table = table;
        }

        public ConvertBase()
        {
            Table = null;
        }

        public static Dictionary<string, string> GetDictionaryFromTablePairs(string[] tablePairs)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            int index;
            int count = tablePairs.Length;

            for (index = 0; index < count; index += 2)
                dictionary.Add(tablePairs[index], tablePairs[index + 1]);

            return dictionary;
        }

        public static char[] GetCharactersFromTablePairs(string[] tablePairs)
        {
            List<char> list = new List<char>();

            int index;
            int count = tablePairs.Length;

            for (index = 0; index < count; index += 2)
                list.Add(tablePairs[index][0]);

            return list.ToArray();
        }

        public static HashSet<char> GetCharacterSetFromCharacters(char[] characters)
        {
            HashSet<char> characterSet = new HashSet<char>();

            int index;
            int count = characters.Length;

            for (index = 0; index < count; index++)
                characterSet.Add(characters[index]);

            return characterSet;
        }

        public static int GetMaxInputLengthFromTablePairs(string[] tablePairs)
        {
            int index;
            int count = tablePairs.Length;
            int maxLength = -1;

            for (index = 0; index < count; index += 2)
            {
                if (tablePairs[index].Length > maxLength)
                    maxLength = tablePairs[index].Length;
            }

            return maxLength;
        }

        public static int GetMaxOutputLengthFromTablePairs(string[] tablePairs)
        {
            int index;
            int count = tablePairs.Length;
            int maxLength = -1;

            for (index = 0; index < count; index += 2)
            {
                if (tablePairs[index + 1].Length > maxLength)
                    maxLength = tablePairs[index].Length;
            }

            return maxLength;
        }

        public delegate bool ConvertRunDelegate(
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            char syllableSeparator,
            string inputString,
            out string outputString,
            DictionaryRepository dictionary,
            FormatQuickLookup quickDictionary,
            LanguageTool tool,
            bool isWord);

        public static bool ConvertParagraph(
            LanguageID inputLanguage,
            LanguageID outputLanguage,
            char syllableSeparator,
            string inputString,
            out string outputString,
            DictionaryRepository dictionary,
            FormatQuickLookup quickDictionary,
            LanguageTool tool,
            ConvertRunDelegate convertRun)
        {
            outputString = "";

            if (String.IsNullOrEmpty(inputString))
                return true;

            if (dictionary == null)
                return false;

            bool hasZeroWidthSpaces = false;

            if (inputString.IndexOf(LanguageLookup.ZeroWidthSpace) != -1)
                hasZeroWidthSpaces = true;
            else if (inputString.IndexOf(LanguageLookup.ZeroWidthNoBreakSpace) != -1)
                hasZeroWidthSpaces = true;

            if (!hasZeroWidthSpaces)
                return convertRun(inputLanguage, outputLanguage, syllableSeparator,
                    inputString, out outputString, dictionary, quickDictionary, tool, false);

            StringBuilder sb = new StringBuilder();
            int index;
            int count = inputString.Length;
            int startIndex;
            string str, res;
            char c;
            bool isRomanized = LanguageLookup.IsRomanized(outputLanguage);
            bool returnValue = true;

            for (startIndex = 0; startIndex < count;)
            {
                for (index = startIndex; index < count; index++)
                {
                    c = inputString[index];

                    if (LanguageLookup.SpaceAndPunctuationCharacters.Contains(c))
                        break;
                }

                if (index != startIndex)
                {
                    str = inputString.Substring(startIndex, index - startIndex);

                    if (!convertRun(inputLanguage, outputLanguage, syllableSeparator,
                            str, out res, dictionary, quickDictionary, tool, true))
                        returnValue = false;

                    sb.Append(res);
                    startIndex = index;
                }

                if (index < count)
                {
                    c = inputString[index];

                    if (LanguageLookup.SpaceAndPunctuationCharacters.Contains(c))
                    {
                        startIndex = index + 1;

                        if (isRomanized)
                            sb.Append(LanguageLookup.ToRomanizedSpaceOrPunctuation(c));
                        else
                            sb.Append(c);
                    }
                }
            }

            outputString = sb.ToString().Trim();

            return returnValue;
        }
    }
}
