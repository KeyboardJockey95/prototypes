using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Repository
{
    public class DictionaryRepository : LanguageBaseRepository<DictionaryEntry>
    {
        public DictionaryRepository(ILanguageObjectStore objectStore) : base(objectStore) { }

        public DictionaryRepository(ILanguageObjectStore objectStore, XElement element) : base(objectStore, element) { }

        public DictionaryEntry GetEntry(string pattern, LanguageID languageID)
        {
            List<DictionaryEntry> entries = Lookup(pattern, MatchCode.Exact, languageID, 0, 0);
            DictionaryEntry returnValue = null;

            if (entries != null)
                returnValue = entries.FirstOrDefault();

            return returnValue;
        }

        public List<DictionaryEntry> Lookup(string pattern, MatchCode matchType, LanguageID languageID, int page, int pageSize)
        {
            ConvertCanonical canonical = new ConvertCanonical(languageID, true);
            string canonicalPattern;
            if (!canonical.Canonical(out matchType, out canonicalPattern, matchType, pattern))
                canonicalPattern = pattern;
            StringMatcher matcher = new StringMatcher(canonicalPattern, "Key", matchType, true, null, page, pageSize);
            List<DictionaryEntry> returnValue = Query(matcher, languageID);
            return returnValue;
        }

        // Note: Actual page count will be the union of individual language queries.
        public List<DictionaryEntry> Lookup(string pattern, MatchCode matchType, List<LanguageID> languageIDs, int page, int pageSize)
        {
            List<DictionaryEntry> returnValue = new List<DictionaryEntry>();
            List<DictionaryEntry> lookup;

            foreach (LanguageID languageID in languageIDs)
            {
                lookup = Lookup(pattern, matchType, languageID, page, pageSize);

                if ((lookup != null) && (lookup.Count() != 0))
                    returnValue.AddRange(lookup);
            }

            return returnValue;
        }

        public bool AddList(List<DictionaryEntry> dictionaryEntries)
        {
            Dictionary<LanguageID, List<DictionaryEntry>> dictionary = DictionaryEntry.SplitList(dictionaryEntries);
            bool returnValue = true;

            foreach (KeyValuePair<LanguageID, List<DictionaryEntry>> kvp in dictionary)
            {
                LanguageID languageID = kvp.Key;
                List<DictionaryEntry> languageEntries = kvp.Value;

                if (!AddList(languageEntries, languageID))
                    returnValue = false;
            }

            return returnValue;
        }

        public bool TranslateString(string fromString, LanguageID fromLanguageID, LanguageID toLanguageID, out string toString)
        {
            bool isCharacterBased = LanguageLookup.IsCharacterBased(fromLanguageID);
            bool isUseSpaces = LanguageLookup.IsUseSpacesToSeparateWords(fromLanguageID);
            bool gotSomething = false;

            if (!isUseSpaces)
            {
                foreach (string sc in LanguageLookup.SpaceStrings)
                    fromString = fromString.Replace(sc, "");
            }

            StringBuilder sb = new StringBuilder();
            DictionaryEntry definition;
            List<string> words = new List<string>(50);
            int index;
            int count = fromString.Length;
            int startIndex;
            string str;
            string translated;
            bool containsNewlines = false;
            int columnCount = count;
            int maxChars = (isCharacterBased ? 10 : 512);
            char c;
            char[] whiteSpace = LanguageLookup.SpaceCharacters;
            char[] punctuation = LanguageLookup.PunctuationCharacters;

            if (fromString.Contains("\n") || fromString.Contains("\r"))
                containsNewlines = true;

            toString = "";

            for (startIndex = 0; startIndex < count; )
            {
                c = fromString[startIndex];

                if (whiteSpace.Contains(c) || punctuation.Contains(c) || Char.IsDigit(c))
                {
                    sb.Append(fromString[startIndex]);
                    startIndex++;
                    continue;
                }

                definition = null;
                str = null;

                if (containsNewlines)
                {
                    columnCount = count;

                    for (index = startIndex; index < count; index++)
                    {
                        if ((fromString[index] == '\r') || (fromString[index] == '\n'))
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

                if (isUseSpaces)
                {
                    for (int i = startIndex; i < columnCount; i++)
                    {
                        c = fromString[i];

                        if (punctuation.Contains(c))
                        {
                            index = i;
                            break;
                        }
                    }

                    for (index--; index > startIndex; index--)
                    {
                        c = fromString[index];

                        if (!whiteSpace.Contains(c) && !punctuation.Contains(c))
                            continue;

                        str = fromString.Substring(startIndex, index - startIndex);

                        if ((definition = Get(str, fromLanguageID)) != null)
                        {
                            translated = definition.GetTranslation(toLanguageID);

                            if (!String.IsNullOrEmpty(translated))
                            {
                                sb.Append(translated);
                                gotSomething = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = startIndex; i < columnCount; i++)
                    {
                        c = fromString[i];

                        if (whiteSpace.Contains(c) || punctuation.Contains(c))
                        {
                            index = i;
                            break;
                        }
                    }

                    for (index--; index > startIndex; index--)
                    {
                        str = fromString.Substring(startIndex, index - startIndex);

                        if ((definition = Get(str, fromLanguageID)) != null)
                        {
                            translated = definition.GetTranslation(toLanguageID);

                            if (!String.IsNullOrEmpty(translated))
                            {
                                sb.Append(translated);
                                gotSomething = true;
                                break;
                            }
                        }
                    }
                }

                if (index == startIndex)
                {
                    if (String.IsNullOrEmpty(str))
                        str = fromString.Substring(startIndex, 1);

                    sb.Append(str);
                }

                if (!String.IsNullOrEmpty(str))
                    startIndex += str.Length;
                else
                    startIndex += 1;
            }

            toString = sb.ToString().Trim();

            return gotSomething && !String.IsNullOrEmpty(toString);
        }
    }
}
