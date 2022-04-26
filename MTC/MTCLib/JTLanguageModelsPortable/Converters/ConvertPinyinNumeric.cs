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
    public class ConvertPinyinNumeric : ConvertBase
    {
        // Move number from end-of-word to after-vowels.
        public static List<ConvertEntry> MoveNumberTable1 = new List<ConvertEntry>
        {
            new ConvertEntry("ng1","1ng"),
            new ConvertEntry("ng2","2ng"),
            new ConvertEntry("ng3","3ng"),
            new ConvertEntry("ng4","4ng"),
            new ConvertEntry("ng5","5ng"),
            new ConvertEntry("n1","1n"),
            new ConvertEntry("n2","2n"),
            new ConvertEntry("n3","3n"),
            new ConvertEntry("n4","4n"),
            new ConvertEntry("n5","5n"),
            new ConvertEntry("ar1","a1r"),
            new ConvertEntry("ar2","a2r"),
            new ConvertEntry("ar3","a3r"),
            new ConvertEntry("ar4","a4r"),
            new ConvertEntry("ar5","a5r"),
            new ConvertEntry("er1","e1r"),
            new ConvertEntry("er2","e2r"),
            new ConvertEntry("er3","e3r"),
            new ConvertEntry("er4","e4r"),
            new ConvertEntry("er5","e5r")
        };

        // Move from after-all-vowels to main syllable.
        public static List<ConvertEntry> MoveNumberTable2 = new List<ConvertEntry>
        {
            new ConvertEntry("ai1","a1i"),
            new ConvertEntry("ai2","a2i"),
            new ConvertEntry("ai3","a3i"),
            new ConvertEntry("ai4","a4i"),
            new ConvertEntry("ai5","a5i"),
            new ConvertEntry("ao1","a1o"),
            new ConvertEntry("ao2","a2o"),
            new ConvertEntry("ao3","a3o"),
            new ConvertEntry("ao4","a4o"),
            new ConvertEntry("ao5","a5o"),
            new ConvertEntry("ei1","e1i"),
            new ConvertEntry("ei2","e2i"),
            new ConvertEntry("ei3","e3i"),
            new ConvertEntry("ei4","e4i"),
            new ConvertEntry("ei5","e5i"),
            new ConvertEntry("ou1","o1u"),
            new ConvertEntry("ou2","o2u"),
            new ConvertEntry("ou3","o3u"),
            new ConvertEntry("ou4","o4u"),
            new ConvertEntry("ou5","o5u"),
        };

        // Replace letter-with-number with Unicode character.
        public static List<ConvertEntry> AddToneMarkTable = new List<ConvertEntry>
        {
            new ConvertEntry("a1", "ā"),
            new ConvertEntry("a2", "á"),
            new ConvertEntry("a3", "ǎ"),
            new ConvertEntry("a4", "à"),
            new ConvertEntry("a5", "a"),
            new ConvertEntry("e1", "ē"),
            new ConvertEntry("e2", "é"),
            new ConvertEntry("e3", "ě"),
            new ConvertEntry("e4", "è"),
            new ConvertEntry("e5", "e"),
            new ConvertEntry("i1", "ī"),
            new ConvertEntry("i2", "í"),
            new ConvertEntry("i3", "ǐ"),
            new ConvertEntry("i4", "ì"),
            new ConvertEntry("i5", "i"),
            new ConvertEntry("o1", "ō"),
            new ConvertEntry("o2", "ó"),
            new ConvertEntry("o3", "ǒ"),
            new ConvertEntry("o4", "ò"),
            new ConvertEntry("o5", "o"),
            new ConvertEntry("u1", "ū"),
            new ConvertEntry("u2", "ú"),
            new ConvertEntry("u3", "ǔ"),
            new ConvertEntry("u4", "ù"),
            new ConvertEntry("u5", "u"),
            new ConvertEntry("v1", "ǖ"),
            new ConvertEntry("v2", "ǘ"),
            new ConvertEntry("v3", "ǚ"),
            new ConvertEntry("v4", "ǜ"),
            new ConvertEntry("v5", "u:"),
            new ConvertEntry("u:1", "ǖ"),
            new ConvertEntry("u:2", "ǘ"),
            new ConvertEntry("u:3", "ǚ"),
            new ConvertEntry("u:4", "ǜ"),
            new ConvertEntry("u:5", "u:"),
            new ConvertEntry("u:", "ü"),
            /*
            new ConvertEntry(" 1r", "r"),
            new ConvertEntry(" 2r", "r"),
            new ConvertEntry(" 3r", "r"),
            new ConvertEntry(" 4r", "r"),
            new ConvertEntry(" 5r", "r"),
            new ConvertEntry("1r", "r"),
            new ConvertEntry("2r", "r"),
            new ConvertEntry("3r", "r"),
            new ConvertEntry("4r", "r"),
            new ConvertEntry("5r", "r")
            */
        };

        // Replace Unicode character with letter-with-number.
        public static List<ConvertEntry> AddNumericTable = new List<ConvertEntry>
        {
            new ConvertEntry("ar1", "ār"),
            new ConvertEntry("ar2", "ár"),
            new ConvertEntry("ar3", "ǎr"),
            new ConvertEntry("ar4", "àr"),
            new ConvertEntry("a1", "ā"),
            new ConvertEntry("a2", "á"),
            new ConvertEntry("a3", "ǎ"),
            new ConvertEntry("a4", "à"),
            new ConvertEntry("er1", "ēr"),
            new ConvertEntry("er2", "ér"),
            new ConvertEntry("er3", "ěr"),
            new ConvertEntry("er4", "èr"),
            new ConvertEntry("e1", "ē"),
            new ConvertEntry("e2", "é"),
            new ConvertEntry("e3", "ě"),
            new ConvertEntry("e4", "è"),
            new ConvertEntry("i1", "ī"),
            new ConvertEntry("i2", "í"),
            new ConvertEntry("i3", "ǐ"),
            new ConvertEntry("i4", "ì"),
            new ConvertEntry("o1", "ō"),
            new ConvertEntry("o2", "ó"),
            new ConvertEntry("o3", "ǒ"),
            new ConvertEntry("o4", "ò"),
            new ConvertEntry("u1", "ū"),
            new ConvertEntry("u2", "ú"),
            new ConvertEntry("u3", "ǔ"),
            new ConvertEntry("u4", "ù"),
            new ConvertEntry("v1", "ǖ"),
            new ConvertEntry("v2", "ǘ"),
            new ConvertEntry("v3", "ǚ"),
            new ConvertEntry("v4", "ǜ"),
            new ConvertEntry("u:1", "ǖ"),
            new ConvertEntry("u:2", "ǘ"),
            new ConvertEntry("u:3", "ǚ"),
            new ConvertEntry("u:4", "ǜ"),
            new ConvertEntry("u:", "ü"),
        };

        public static string[] Initials = new string[]
        {
            "b",
            "ch",
            "c",
            "d",
            "f",
            "g",
            "h",
            "j",
            "k",
            "l",
            "m",
            "n",
            "p",
            "q",
            "r",
            "sh",
            "s",
            "t",
            "w",
            "x",
            "y",
            "zh",
            "z"
        };

        public static string[] Finals = new string[]
        {
            "ai",
            "ang",
            "an",
            "ao",
            "ar",
            "au",
            "a",
            "ei",
            "eng",
            "en",
            "er",
            "e",
            "iang",
            "ian",
            "iao",
            "ia",
            "ien",
            "ie",
            "ing",
            "in",
            "iong",
            "iu",
            "i",
            "ong",
            "ou",
            "o",
            "uang",
            "uan",
            "uai",
            "ua",
            "uen",
            "ue",
            "ui",
            "un",
            "uo",
            "uan:",
            "un:",
            "u:",
            "u"
        };

        // Replace letter-with-number with Unicode character.
        public static List<ConvertEntry> MarkedTable = new List<ConvertEntry>
        {
            new ConvertEntry("ā", "a"),
            new ConvertEntry("á", "a"),
            new ConvertEntry("ǎ", "a"),
            new ConvertEntry("à", "a"),
            new ConvertEntry("ē", "e"),
            new ConvertEntry("é", "e"),
            new ConvertEntry("ě", "e"),
            new ConvertEntry("è", "e"),
            new ConvertEntry("ī", "i"),
            new ConvertEntry("í", "i"),
            new ConvertEntry("ǐ", "i"),
            new ConvertEntry("ì", "i"),
            new ConvertEntry("ō", "o"),
            new ConvertEntry("ó", "o"),
            new ConvertEntry("ǒ", "o"),
            new ConvertEntry("ò", "o"),
            new ConvertEntry("ū", "u"),
            new ConvertEntry("ú", "u"),
            new ConvertEntry("ǔ", "u"),
            new ConvertEntry("ù", "u"),
            new ConvertEntry("ǖ", "ü"),
            new ConvertEntry("ǘ", "ü"),
            new ConvertEntry("ǚ", "ü"),
            new ConvertEntry("ǜ", "ü")
        };

        public ConvertPinyinNumeric()
        {
        }

        public static bool ToToneMarksCheck(out string output, string input)
        {
            if (IsNumeric(input))
                return ToToneMarks(out output, input);
            else
                output = input;

            return false;
        }

        public static bool ToToneMarks(out string output, string input)
        {
            ConvertTo(out output, input.ToLower(), MoveNumberTable1);
            ConvertTo(out output, output, MoveNumberTable2);
            ConvertTo(out output, output, AddToneMarkTable);
            return true;
        }

        public static bool IsNumeric(string str)
        {
            int index;
            int count = str.Length;
            char c;
            char last = ' ';

            for (index = 0; index < count; index++)
            {
                c = str[index];

                if ((c >= '1') && (c <= '5'))
                {
                    if (Char.IsLetter(last))
                        return true;
                    else if (last == ':')
                        return true;
                }

                last = c;
            }

            return false;
        }

        public static bool IsMarked(char c)
        {
            foreach (ConvertEntry entry in MarkedTable)
            {
                if (entry.In[0] == c)
                    return true;
            }
            return false;
        }

        public static bool IsInitial(char c)
        {
            foreach (string s in Initials)
            {
                if (s[0] == c)
                    return true;
            }
            return false;
        }

        public static bool ToNumeric(out string output, string input)
        {
            string unmarked;

            output = "";

            ConvertTo(out unmarked, input.ToLower(), MarkedTable);

            int index;
            int count = unmarked.Length;

            for (index = 0; index < count; )
            {
                string sub = unmarked.Substring(index);
                string syllable = "";
                int initialSize = 0;
                int finalSize = 0;
                int syllableSize = 0;

                foreach (string initial in Initials)
                {
                    if (sub.StartsWith(initial))
                    {
                        initialSize = initial.Length;
                        syllableSize += initialSize;
                        syllable += initial;
                        sub = sub.Substring(initialSize);
                        break;
                    }
                }

                foreach (string final in Finals)
                {
                    if (sub.StartsWith(final))
                    {
                        finalSize = final.Length;

                        if ((final.Length > 1) && IsMarked(input[index + initialSize]) && IsMarked(input[index + initialSize + 1]))
                        {
                            finalSize = 1;
                            syllable += final.Substring(0, finalSize);
                        }
                        else if (final.EndsWith("r") && (sub.Length > finalSize) && !Char.IsWhiteSpace(sub[finalSize]) && !IsInitial(sub[finalSize]))
                        {
                            finalSize--;
                            syllable += final.Substring(0, finalSize);
                        }
                        else
                            syllable += final;

                        syllableSize += finalSize;
                        sub = sub.Substring(finalSize);
                        break;
                    }
                }

                if (syllableSize != 0)
                {
                    if ((sub.Length > 0) && (sub[0] >= '1') && (sub[0] <= '5'))
                    {
                        syllableSize++;
                        syllable = input.Substring(index, syllableSize);
                    }
                    else
                    {
                        string subInput = input.Substring(index, syllableSize);
                        syllable = subInput.ToLower();
                        ConvertFrom(out syllable, syllable, AddNumericTable);
                        ConvertFrom(out syllable, syllable, MoveNumberTable2);
                        ConvertFrom(out syllable, syllable, MoveNumberTable1);

                        int c = subInput.Length;
                        int i;
                        char[] carray = new char[syllable.Length];

                        for (i = 0; i < c; i++)
                        {
                            if (Char.IsUpper(subInput[i]))
                                carray[i] = Char.ToUpper(syllable[i]);
                            else
                                carray[i] = syllable[i];
                        }

                        if (syllable.Length > c)
                            carray[c] = syllable[c];

                        syllable = new string(carray);
                    }

                    output += syllable;
                    index += syllableSize;
                }
                else
                {
                    output += sub.Substring(0, 1);
                    index++;
                }
            }

            /*
            ConvertFrom(out output, input.ToLower(), AddNumericTable);
            ConvertFrom(out output, output, MoveNumberTable2);
            ConvertFrom(out output, output, MoveNumberTable1);
            */

            return true;
        }

        public override bool To(out string output, string input)
        {
            return ToToneMarks(out output, input);
        }

        public override bool From(out string output, string input)
        {
            return ToNumeric(out output, input);
        }

        static bool Match(string[] patterns, string str, int length, int inIndex, out int outIndex)
        {
            if (inIndex < length)
            {
                int patternLength;
                int subIndex;

                foreach (string pattern in patterns)
                {
                    patternLength = pattern.Length;

                    if (inIndex + patternLength > length)
                        continue;

                    for (subIndex = 0; subIndex < patternLength; subIndex++)
                    {
                        if (str[inIndex + subIndex] != pattern[subIndex])
                            break;
                    }

                    if (subIndex == patternLength)
                    {
                        outIndex = inIndex + patternLength;
                        return true;
                    }
                }
            }

            outIndex = inIndex;

            return false;
        }

        static bool MatchInitial(string str, int length, int inIndex, out int outIndex)
        {
            return Match(Initials, str, length, inIndex, out outIndex);
        }

        static bool MatchFinal(string str, int length, int inIndex, out int outIndex)
        {
            return Match(Finals, str, length, inIndex, out outIndex);
        }

        static bool MatchSyllable(string str, int length, int inIndex, out int outIndex)
        {
            int tmpIndex1;
            int tmpIndex2;
            int tmpIndex3;
            string lowerStr = str.ToLower();

            outIndex = inIndex;

            if (MatchInitial(lowerStr, length, inIndex, out tmpIndex1))
            {
                if (MatchFinal(lowerStr, length, tmpIndex1, out tmpIndex2))
                {
                    if (MatchToneNumber(lowerStr, length, tmpIndex2, out tmpIndex3))
                    {
                        outIndex = tmpIndex3;
                        return true;
                    }
                    else
                    {
                        outIndex = tmpIndex2;
                        return true;
                    }
                }
                else
                    return false;
            }
            else if (MatchFinal(lowerStr, length, inIndex, out tmpIndex1))
            {
                if (MatchToneNumber(lowerStr, length, tmpIndex1, out tmpIndex2))
                    outIndex = tmpIndex2;
                else
                    outIndex = tmpIndex1;

                return true;
            }

            return false;
        }

        static bool MatchToneNumber(string str, int length, int inIndex, out int outIndex)
        {
            if (inIndex < length)
            {
                char digit = str[inIndex];

                if ((digit >= '1') && (digit <= '5'))
                {
                    outIndex = inIndex + 1;
                    return true;
                }
            }

            outIndex = inIndex;

            return false;
        }

        static bool MatchSpace(string str, int length, int inIndex, out int outIndex)
        {
            bool returnValue = false;

            if (inIndex < length)
            {
                while (inIndex < length)
                {
                    switch (str[inIndex])
                    {
                        case ' ':
                        case '\t':
                        case '\n':
                        case '\r':
                            returnValue = true;
                            break;
                        default:
                            outIndex = inIndex;
                            return returnValue;
                    }

                    inIndex++;
                }
            }

            outIndex = inIndex;

            return returnValue;
        }

        static bool MatchNonSpace(string str, int length, int inIndex, out int outIndex)
        {
            bool returnValue = false;

            if (inIndex < length)
            {
                while (inIndex < length)
                {
                    switch (str[inIndex])
                    {
                        case ' ':
                        case '\t':
                        case '\n':
                        case '\r':
                            outIndex = inIndex;
                            return returnValue;
                        default:
                            returnValue = true;
                            break;
                    }

                    inIndex++;
                }
            }

            outIndex = inIndex;

            return returnValue;
        }

        public static bool CanonicalRun(out string output, string input, bool addRegex)
        {
            string intermediate;

            output = input;

            if (String.IsNullOrEmpty(input))
                return false;

            if (IsNumeric(input))
                intermediate = input;
            else if (!ToNumeric(out intermediate, input))
                return false;

            int length = intermediate.Length;
            int inIndex = 0;
            int outIndex = 0;
            StringBuilder sb = new StringBuilder(length * 2);
            string syllable;

            while (inIndex < length)
            {
                if (MatchSyllable(intermediate, length, inIndex, out outIndex))
                {
                    syllable = intermediate.Substring(inIndex, outIndex - inIndex);
                    sb.Append(syllable);

                    switch (syllable[syllable.Length - 1])
                    {
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                            break;
                        default:
                            if (addRegex)
                                sb.Append("[1-5]");
                            else
                                sb.Append("5");
                            break;
                    }

                    inIndex = outIndex;

                    while (inIndex < length)
                    {
                        if (Char.IsPunctuation(intermediate[inIndex]))
                        {
                            sb.Append(intermediate[inIndex]);
                            inIndex++;
                        }
                        else
                            break;
                    }

                    if (inIndex < length)
                    {
                        if (MatchSpace(intermediate, length, inIndex, out outIndex))
                        {
                            sb.Append(" ");
                            inIndex = outIndex;
                        }
                        else
                            sb.Append(" ");
                    }
                }
                else
                    return false;
            }

            output = sb.ToString();

            return true;
        }

        public static bool Canonical(out string output, string input, bool addRegex)
        {
            output = String.Empty;

            if (String.IsNullOrEmpty(input))
                return false;

            int length = input.Length;
            int inIndex = 0;
            int outIndex = 0;
            StringBuilder sb = new StringBuilder(length * 2);

            while (inIndex < length)
            {
                if (MatchNonSpace(input, length, inIndex, out outIndex))
                {
                    string run = input.Substring(inIndex, outIndex - inIndex);
                    string tmpOutput;

                    if (CanonicalRun(out tmpOutput, run, addRegex))
                        sb.Append(tmpOutput);
                    else
                        sb.Append(input.Substring(inIndex, outIndex - inIndex));

                    inIndex = outIndex;
                }
                else if (MatchSpace(input, length, inIndex, out outIndex))
                {
                    sb.Append(" ");
                    inIndex = outIndex;
                }
                else
                    inIndex++;
            }

            output = sb.ToString();

            return true;
        }

        // Convert canonical pinyin to display-friendly pinyin.  Returns true if any spaces removed.
        public static bool Display(out string output, string input, LanguageID languageID, bool toNumeric,
            DictionaryRepository dictionary, FormatQuickLookup quickDictionary /* pinyin to Chinese characters */)
        {
            output = input;

            if (String.IsNullOrEmpty(input))
                return false;

            if (!IsNumeric(input))
            {
                string tmp = input;
                if (!ToNumeric(out tmp, input))
                    return false;

                output = tmp;
            }

            List<string> runs = TextUtilities.ParseRuns(input);
            int endIndex = runs.Count();
            int startIndex;
            int runIndex;
            int runEndIndex;
            int index;
            int maxItemCount = 16;
            int itemCount;
            int spacesRemoved = 0;
            string run;
            StringBuilder sb;
            StringBuilder sbOut = new StringBuilder();
            char[] whiteSpace = LanguageLookup.SpaceCharacters;
            char[] nonAlphanumericCharacters = LanguageLookup.NonAlphanumericCharacters;
            List<string> displayRuns = new List<string>(endIndex);

            for (startIndex = 0; startIndex < endIndex; )
            {
                for (; startIndex < endIndex; startIndex++)
                {
                    run = runs[startIndex];

                    if ((whiteSpace.Contains(run[0]) || (nonAlphanumericCharacters.Contains(run[0]))))
                        sbOut.Append(run);
                    else
                        break;
                }

                for (runIndex = startIndex, runEndIndex = endIndex; runEndIndex > runIndex; runEndIndex--)
                {
                    sb = new StringBuilder();
                    itemCount = 0;

                    for (index = runIndex; index < runEndIndex; index++)
                    {
                        run = runs[index];

                        if (whiteSpace.Contains(run[0]))
                            continue;
                        else if (nonAlphanumericCharacters.Contains(run[0]))
                            break;

                        if (itemCount != 0)
                            sb.Append(" ");

                        sb.Append(run);
                        itemCount++;

                        if (itemCount == maxItemCount)
                        {
                            runEndIndex = index + 1;
                            break;
                        }
                    }

                    // If there were no embedded spaces...
                    if (itemCount <= 1)
                    {
                        if (index == startIndex)
                            startIndex = index + 1;
                        else
                            startIndex = index;

                        for (index = runIndex; index < runEndIndex; index++)
                            sbOut.Append(runs[index]);

                        break;
                    }

                    run = sb.ToString();

                    if (quickDictionary != null)
                    {
                        string value;

                        if (!quickDictionary.QuickDictionary.TryGetValue(run, out value))
                        {
                            runEndIndex--;

                            // Skip trailing space or non-alpha.
                            while (runEndIndex > runIndex)
                            {
                                run = runs[runEndIndex];

                                if (!whiteSpace.Contains(run[0]) && !nonAlphanumericCharacters.Contains(run[0]))
                                    break;

                                runEndIndex--;
                            }

                            continue;
                        }
                    }
                    else if (dictionary != null)
                    {
                        if (!dictionary.Contains(run, languageID))
                        {
                            runEndIndex--;

                            // Skip trailing space or non-alpha.
                            while (runEndIndex > runIndex)
                            {
                                run = runs[runEndIndex];

                                if (!whiteSpace.Contains(run[0]) && !nonAlphanumericCharacters.Contains(run[0]))
                                    break;

                                runEndIndex--;
                            }

                            continue;
                        }
                    }

                    for (index = runIndex; index < runEndIndex; index++)
                    {
                        run = runs[index];

                        if (whiteSpace.Contains(run[0]))
                            continue;

                        sbOut.Append(run);
                    }

                    spacesRemoved++;

                    if (startIndex < runEndIndex)
                        startIndex = runEndIndex;
                    else
                        startIndex += 1;

                    break;
                }
            }

            if (spacesRemoved != 0)
            {
                output = sbOut.ToString();

                int count = LanguageLookup.FatPunctuationCharacters.Count();
                string[] fat = LanguageLookup.FatPunctuationCharacters;
                string[] thinSpaced = LanguageLookup.ThinPunctuationWithSpaceCharacters;

                for (index = 0; index < count; index++)
                    output = output.Replace(fat[index], thinSpaced[index]);
            }

            if (!toNumeric)
            {
                string local;
                ToToneMarks(out local, output);
                output = local;
            }

            return (spacesRemoved == 0 ? false : true);
        }

        // Convert canonical pinyin to display-friendly pinyin.  Returns true if any spaces removed.
        public static bool Display(DictionaryEntry dictionaryEntry, bool toNumeric, DictionaryRepository dictionary,
            FormatQuickLookup quickDictionary)
        {
            LanguageID pinyinID = LanguageLookup.ChinesePinyin;
            string text;
            int index;
            int count;
            LanguageString languageString;
            string synonym;
            bool returnValue = false;

            if (dictionaryEntry == null)
                return false;

            if (dictionaryEntry.Alternates != null)
            {
                foreach (LanguageString alternate in dictionaryEntry.Alternates)
                {
                    if (alternate.LanguageID == pinyinID)
                    {
                        if (Display(out text, alternate.Text, pinyinID, toNumeric, dictionary, quickDictionary))
                        {
                            alternate.Text = text;
                            returnValue = true;
                        }
                    }
                }
            }

            if (dictionaryEntry.Senses != null)
            {
                foreach (Sense sense in dictionaryEntry.Senses)
                {
                    if (sense.LanguageSynonyms != null)
                    {
                        foreach (LanguageSynonyms languageSynonyms in sense.LanguageSynonyms)
                        {
                            if (languageSynonyms.LanguageID == pinyinID)
                            {
                                if (languageSynonyms.HasProbableSynonyms())
                                {
                                    count = languageSynonyms.ProbableSynonymCount;
                                    index = 0;

                                    for (index = 0; index < count; index++)
                                    {
                                        ProbableMeaning probableSynonym = languageSynonyms.GetProbableSynonymIndexed(index);

                                        synonym = probableSynonym.Meaning;

                                        if (Display(out text, synonym, pinyinID, toNumeric, dictionary, quickDictionary))
                                        {
                                            probableSynonym.Meaning = text;
                                            returnValue = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (sense.Examples != null)
                    {
                        foreach (MultiLanguageString example in sense.Examples)
                        {
                            languageString = example.LanguageString(pinyinID);

                            if (languageString != null)
                            {
                                if (Display(out text, languageString.Text, pinyinID, toNumeric, dictionary, quickDictionary))
                                {
                                    languageString.Text = text;
                                    returnValue = true;
                                }
                            }
                        }
                    }
                }
            }

            return returnValue;
        }
    }
}
