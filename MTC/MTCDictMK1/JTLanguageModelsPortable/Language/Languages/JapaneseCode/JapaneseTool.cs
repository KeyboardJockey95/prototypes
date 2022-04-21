using System;
using System.Collections.Generic;
using System.IO;
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
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public partial class JapaneseToolCode : LanguageTool
    {
        public string AuxSep = " ";

        public static LanguageID JapaneseID = LanguageLookup.Japanese;
        public static LanguageID JapaneseKanaID = LanguageLookup.JapaneseKana;
        public static LanguageID JapaneseRomajiID = LanguageLookup.JapaneseRomaji;
        public static LanguageID EnglishID = LanguageLookup.English;
        public static List<string> EDictEntriesToBeAdded;
        public static List<string> EDictEntriesToBeUpdated;
        protected ConvertTransliterate CharacterConverter;
        protected ConvertTransliterate RomanizationConverter;

        public static List<LanguageID> LocalInflectionLanguageIDs = new List<LanguageID>()
            {
                JapaneseID,
                JapaneseKanaID,
                JapaneseRomajiID,
                EnglishID
            };

        public static List<LanguageID> JapaneseLanguageIDs = new List<LanguageID>()
            {
                JapaneseID,
                JapaneseKanaID,
                JapaneseRomajiID
            };

        public static ConvertRomaji RomajiConverter = new ConvertRomaji(
            JapaneseKanaID,
            '\0',
            null,
            false);

        public static ConvertRomaji KanaConverter = new ConvertRomaji(
            JapaneseID,
            '\0',
            null,
            false);

        public JapaneseToolCode() : base(JapaneseID)
        {
            _TargetLanguageIDs = JapaneseLanguageIDs;
            _HostLanguageIDs = new List<LanguageID>() { LanguageLookup.English };
            CharacterConverter = null;
            RomanizationConverter = null;
            CanInflectVerbs = true;
            CanInflectNouns = true;
            CanInflectAdjectives = true;
            CanDeinflectEndings = true;
        }

        public override IBaseObject Clone()
        {
            return new JapaneseToolCode();
        }

        public override List<LanguageID> InflectionLanguageIDs
        {
            get
            {
                return LocalInflectionLanguageIDs;
            }
        }

        public override List<Inflection> InflectAnyDictionaryFormAll(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            Sense sense;
            string definition;
            LexicalCategory category;
            List<Inflection> inflections = null;

            if (GetSenseCategoryDefinition(
                    dictionaryEntry,
                    EnglishID,
                    ref senseIndex,
                    ref synonymIndex,
                    out sense,
                    out category,
                    out definition))
            {
                StemSubstitutionCheck(ref dictionaryEntry, ref sense, ref senseIndex);
                switch (category)
                {
                    case LexicalCategory.Verb:
                        inflections = ConjugateVerbDictionaryFormAll(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex);
                        break;
                    case LexicalCategory.Adjective:
                        inflections = DeclineAdjectiveDictionaryFormAll(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex);
                        break;
                    case LexicalCategory.Noun:
                        if (IsNaAdjective(dictionaryEntry, senseIndex))
                            inflections = DeclineAdjectiveDictionaryFormAll(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                        else
                            inflections = ConjugateVerbDictionaryFormAll(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                        break;
                }
            }

            return inflections;
        }

        public override List<Inflection> InflectAnyDictionaryFormDefault(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            Sense sense;
            string definition;
            LexicalCategory category;
            List<Inflection> inflections = null;

            if (GetSenseCategoryDefinition(
                    dictionaryEntry,
                    EnglishID,
                    ref senseIndex,
                    ref synonymIndex,
                    out sense,
                    out category,
                    out definition))
            {
                StemSubstitutionCheck(ref dictionaryEntry, ref sense, ref senseIndex);
                switch (category)
                {
                    case LexicalCategory.Verb:
                        inflections = ConjugateVerbDictionaryFormDefault(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex);
                        break;
                    case LexicalCategory.Adjective:
                        inflections = DeclineAdjectiveDictionaryFormDefault(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex);
                        break;
                    case LexicalCategory.Noun:
                        if (IsNaAdjective(dictionaryEntry, senseIndex))
                            inflections = DeclineAdjectiveDictionaryFormDefault(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                        else
                            inflections = ConjugateVerbDictionaryFormDefault(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                        break;
                }
            }

            return inflections;
        }

        public override bool InflectAnyDictionaryFormDesignated(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            Designator designation,
            out Inflection inflection)
        {
            Sense sense;
            string definition;
            LexicalCategory category;
            bool returnValue = false;

            inflection = null;

            if (GetSenseCategoryDefinition(
                    dictionaryEntry,
                    LanguageLookup.English,
                    ref senseIndex,
                    ref synonymIndex,
                    out sense,
                    out category,
                    out definition))
            {
                StemSubstitutionCheck(ref dictionaryEntry, ref sense, ref senseIndex);

                switch (category)
                {
                    case LexicalCategory.Verb:
                        returnValue = ConjugateVerbDictionaryFormDesignated(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex,
                            designation,
                            out inflection);
                        break;
                    case LexicalCategory.Adjective:
                        returnValue = DeclineAdjectiveDictionaryFormDesignated(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex,
                            designation,
                            out inflection);
                        break;
                    case LexicalCategory.Noun:
                        returnValue = DeclineNounDictionaryFormDesignated(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex,
                            designation,
                            out inflection);
                        break;
                }
            }

            return returnValue;
        }

        public override DictionaryEntry GetDictionaryEntry(string dictionaryForm)
        {
            DictionaryEntry dictionaryEntry;

            lock (this)
            {
                if (DictionaryCacheFound.FindSingle(
                        MatchCode.Exact,
                        LanguageID,
                        dictionaryForm,
                        out dictionaryEntry))
                    return dictionaryEntry;
                else if (ConvertRomaji.IsAllKana(dictionaryForm))
                {
                    if (DictionaryCacheFound.FindSingle(
                            MatchCode.Exact,
                            JapaneseKanaID,
                            dictionaryForm,
                            out dictionaryEntry))
                        return dictionaryEntry;
                }
                else if (ConvertRomaji.IsAllRomaji(dictionaryForm))
                {
                    if (DictionaryCacheFound.FindSingle(
                            MatchCode.Exact,
                            JapaneseRomajiID,
                            dictionaryForm,
                            out dictionaryEntry))
                        return dictionaryEntry;
                }

                if (DictionaryCacheNotFound.HasSingle(
                            MatchCode.Exact,
                            LanguageID,
                            dictionaryForm))
                    return null;

                dictionaryEntry = DictionaryDatabase.Get(dictionaryForm, JapaneseID);

                if (dictionaryEntry == null)
                {
                    if (ConvertRomaji.IsAllKana(dictionaryForm))
                        dictionaryEntry = DictionaryDatabase.Get(dictionaryForm, JapaneseKanaID);
                    else if (ConvertRomaji.IsAllRomaji(dictionaryForm))
                        dictionaryEntry = DictionaryDatabase.Get(dictionaryForm, JapaneseRomajiID);
                }

                if (dictionaryEntry == null)
                {
                    DictionaryCacheNotFound.AddSingle(
                        MatchCode.Exact,
                        LanguageID,
                        dictionaryForm,
                        dictionaryEntry);

                    if (ConvertRomaji.IsAllKana(dictionaryForm))
                        DictionaryCacheNotFound.AddSingle(
                            MatchCode.Exact,
                            JapaneseKanaID,
                            dictionaryForm,
                            dictionaryEntry);
                    else if (ConvertRomaji.IsAllRomaji(dictionaryForm))
                        DictionaryCacheNotFound.AddSingle(
                            MatchCode.Exact,
                            JapaneseRomajiID,
                            dictionaryForm,
                            dictionaryEntry);
                }
                else
                    DictionaryCacheFound.AddSingle(
                        MatchCode.Exact,
                        dictionaryEntry.LanguageID,
                        dictionaryForm,
                        dictionaryEntry);

                return dictionaryEntry;
            }
        }

        public DictionaryEntry GetKanjiDictionaryEntry(string dictionaryForm)
        {
            return GetDictionaryLanguageEntry(dictionaryForm, JapaneseID);
        }

        public DictionaryEntry GetKanaDictionaryEntry(string dictionaryForm)
        {
            return GetDictionaryLanguageEntry(dictionaryForm, JapaneseKanaID);
        }

        public DictionaryEntry GetRomajiDictionaryEntry(string dictionaryForm)
        {
            return GetDictionaryLanguageEntry(dictionaryForm, JapaneseRomajiID);
        }

        public override DictionaryEntry GetStemDictionaryEntry(string dictionaryForm)
        {
            lock (this)
            {
                DictionaryEntry dictionaryEntry = GetDictionaryLanguageEntry(dictionaryForm, LanguageID);

                if (dictionaryEntry != null)
                {
                    if (dictionaryEntry.HasSenseWithCategory(LexicalCategory.Stem))
                        return dictionaryEntry;
                    if (ConvertRomaji.IsAllKana(dictionaryForm))
                        dictionaryEntry = GetDictionaryLanguageEntry(dictionaryForm, JapaneseKanaID);
                    else if (ConvertRomaji.IsAllRomaji(dictionaryForm))
                        dictionaryEntry = GetDictionaryLanguageEntry(dictionaryForm, JapaneseRomajiID);
                    if (dictionaryEntry != null)
                    {
                        if (dictionaryEntry.HasSenseWithCategory(LexicalCategory.Stem))
                            return dictionaryEntry;
                    }
                }

                return null;
            }
        }

        // Returns true if found either.
        public override bool GetStemAndNonStemDictionaryEntry(
            string dictionaryForm,
            out DictionaryEntry stemEntry,
            out DictionaryEntry nonStemEntry)
        {
            stemEntry = null;
            nonStemEntry = null;

            lock (this)
            {
                stemEntry = null;
                nonStemEntry = null;
                nonStemEntry = GetDictionaryLanguageEntry(dictionaryForm, LanguageID);
                if (nonStemEntry != null)
                {
                    if (nonStemEntry.HasSenseWithCategory(LexicalCategory.Stem))
                    {
                        stemEntry = nonStemEntry;
                        if (!nonStemEntry.HasSenseWithoutCategory(LexicalCategory.Stem))
                            nonStemEntry = null;
                    }
                }
                DictionaryEntry alternateEntry = null;
                if (ConvertRomaji.IsAllKana(dictionaryForm))
                {
                    switch (dictionaryForm)
                    {
                        /*
                        case "し":
                            alternateEntry = CreateStemDictionaryEntry(
                                JapaneseKanaID,
                                "為",
                                "し",
                                "shi",
                                "do",
                                "vs-i");
                            break;
                        */
                        default:
                            alternateEntry = GetDictionaryLanguageEntry(dictionaryForm, JapaneseKanaID);
                            break;
                    }
                }
                else if (ConvertRomaji.IsAllRomaji(dictionaryForm))
                {
                    switch (dictionaryForm)
                    {
                        /*
                        case "shi":
                            alternateEntry = CreateStemDictionaryEntry(
                                JapaneseRomajiID,
                                "為",
                                "し",
                                "shi",
                                "do",
                                "vs-i");
                            break;
                        */
                        default:
                            alternateEntry = GetDictionaryLanguageEntry(dictionaryForm, JapaneseRomajiID);
                            break;
                    }
                }
                if (alternateEntry != null)
                {
                    if (alternateEntry.HasSenseWithCategory(LexicalCategory.Stem))
                    {
                        if (stemEntry == null)
                            stemEntry = alternateEntry;
                        if (!alternateEntry.HasSenseWithoutCategory(LexicalCategory.Stem))
                        {
                            if (nonStemEntry == null)
                                nonStemEntry = alternateEntry;
                        }
                    }
                    else if (nonStemEntry == null)
                        nonStemEntry = alternateEntry;
                }
            }

            if ((stemEntry != null) || (nonStemEntry != null))
                return true;

            return false;
        }

        public DictionaryEntry CreateStemDictionaryEntry(
            LanguageID keyLanguageID,
            string kanji,
            string kana,
            string romaji,
            string hostText,
            string categoryString)
        {
            string key;
            string altText1;
            LanguageID altLanguageID1;
            string altText2;
            LanguageID altLanguageID2;
            if (keyLanguageID == JapaneseID)
            {
                key = kanji;
                altText1 = kana;
                altLanguageID1 = JapaneseKanaID;
                altText2 = romaji;
                altLanguageID2 = JapaneseRomajiID;
            }
            else if (keyLanguageID == JapaneseKanaID)
            {
                key = kana;
                altText1 = kanji;
                altLanguageID1 = JapaneseID;
                altText2 = romaji;
                altLanguageID2 = JapaneseRomajiID;
            }
            else
            {
                key = romaji;
                altText1 = kanji;
                altLanguageID1 = JapaneseID;
                altText2 = kana;
                altLanguageID2 = JapaneseKanaID;
            }
            DictionaryEntry dictionaryEntry = new DictionaryEntry(
                key,
                keyLanguageID,
                new List<LanguageString>()
                {
                    new LanguageString(0, altLanguageID1, altText1),
                    new LanguageString(0, altLanguageID2, altText2)
                },
                new List<Sense>()
                {
                    new Sense(
                        0,
                        LexicalCategory.Stem,
                        categoryString,
                        0,
                        new List<LanguageSynonyms>()
                        {
                            new LanguageSynonyms(JapaneseID, new List<string>(1) { kanji }),
                            new LanguageSynonyms(JapaneseKanaID, new List<string>(1) { kana }),
                            new LanguageSynonyms(JapaneseRomajiID, new List<string>(1) { romaji }),
                            new LanguageSynonyms(EnglishID, new List<string>(1) { hostText })
                        },
                        null)
                },
                null);

            return dictionaryEntry;
        }

        public override void DictionaryEntryAdded(DictionaryEntry dictionaryEntry)
        {
            EDictEntriesToBeAdded = CacheEDictEntry(EDictEntriesToBeAdded, dictionaryEntry);
        }

        public override void DictionaryEntryUpdated(DictionaryEntry dictionaryEntry, DictionaryEntry originalEntry)
        {
            EDictEntriesToBeUpdated = CacheEDictEntry(EDictEntriesToBeUpdated, dictionaryEntry);
        }

        private List<string> CacheEDictEntry(List<string> cachedEntries, DictionaryEntry dictionaryEntry)
        {
            if (cachedEntries == null)
                cachedEntries = new List<string>();
            for (int reading = 0; dictionaryEntry.HasSenseWithReading(reading); reading++)
            {
                string line = GetEdictLineFromDictionaryEntry(dictionaryEntry, reading);
                if (!cachedEntries.Contains(line))
                    cachedEntries.Add(line);
            }
            return cachedEntries;
        }

        public override void SaveDictionaryEntriesAddedAndUpdated()
        {
            SaveEDictEntriesAdded();
            SaveEDictEntriesUpdated();
        }

        private void SaveEDictEntriesAdded()
        {
            string dictionarySourceDirectory = GetDictionarySourceDirectoryFilePath(JapaneseID);
            string dictionarySourceAddedPath = MediaUtilities.ConcatenateFilePath(
                dictionarySourceDirectory,
                "edict_to_be_added_ja.u8");
            SaveEDictEntries(EDictEntriesToBeAdded, dictionarySourceAddedPath);
        }

        private void SaveEDictEntriesUpdated()
        {
            string dictionarySourceDirectory = GetDictionarySourceDirectoryFilePath(JapaneseID);
            string dictionarySourceAddedPath = MediaUtilities.ConcatenateFilePath(
                dictionarySourceDirectory,
                "edict_to_be_updated_ja.u8");
            SaveEDictEntries(EDictEntriesToBeUpdated, dictionarySourceAddedPath);
        }

        private void SaveEDictEntries(List<string> entriesNew, string filePath)
        {
            if ((entriesNew == null) || (entriesNew.Count() == 0))
                return;
            List<string> entriesOld = null;
            if (FileSingleton.Exists(filePath))
            {
                string[] entriesOldArray = FileSingleton.ReadAllLines(filePath, Encoding.UTF8);
                entriesOld = entriesOldArray.ToList();
            }
            else
                entriesOld = new List<string>();
            foreach (string entry in entriesNew)
            {
                if (!entriesOld.Contains(entry))
                    entriesOld.Add(entry);
            }
            if (entriesOld.Count() != 0)
            {
                try
                {
                    FileSingleton.DirectoryExistsCheck(filePath);
                    FileSingleton.WriteAllLines(filePath, entriesOld.ToArray(), Encoding.UTF8);
                }
                catch (Exception)
                {
                }
            }
            entriesNew.Clear();
        }

        public string GetEdictLineFromDictionaryEntry(DictionaryEntry dictionaryEntry, int reading)
        {
            StringBuilder sb = new StringBuilder();
            LanguageID languageID = dictionaryEntry.LanguageID;
            string languageCultureExtensionCode = languageID.LanguageCultureExtensionCode;
            string kanji;
            string kana;
            string romaji;

            GetJapaneseForms(dictionaryEntry, reading, out kanji, out kana, out romaji);

            sb.Append(kanji);
            sb.Append(" ");

            if (kanji != kana)
            {
                sb.Append("[");
                sb.Append(kana);
                sb.Append("] ");
            }

            sb.Append("/");

            foreach (Sense sense in dictionaryEntry.Senses)
            {
                if (sense.Reading != reading)
                    continue;

                LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(LanguageLookup.English);

                if (languageSynonyms == null)
                    continue;

                if (!String.IsNullOrEmpty(sense.CategoryString))
                {
                    sb.Append("(");
                    sb.Append(sense.CategoryString);
                    sb.Append(") ");
                }

                foreach (string synonym in languageSynonyms.Synonyms)
                {
                    sb.Append(synonym);
                    sb.Append("/");
                }
            }

            return sb.ToString();
        }

        public override string DecodeCategoryString(string categoryString)
        {
            if (String.IsNullOrEmpty(categoryString))
                return categoryString;

            if (categoryString.Contains(","))
            {
                string[] parts = categoryString.Split(LanguageLookup.Comma);
                int count = parts.Count();
                int index;
                string newCategoryString = String.Empty;

                for (index = 0; index < count; index++)
                {
                    string category = parts[index];

                    if (String.IsNullOrEmpty(category))
                        continue;

                    if (index != 0)
                        newCategoryString += ", ";

                    newCategoryString += DecodeCategorySegment(category);
                }

                return newCategoryString;
            }
            else
                return DecodeCategorySegment(categoryString);
        }

        public override string DecodeCategorySegment(string categorySegment)
        {
            string newCategoryString;

            if (FormatEDict.CategoryDescriptions.TryGetValue(categorySegment, out newCategoryString))
                return newCategoryString;

            return categorySegment;
        }

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

        protected bool GetJapaneseForms(
            DictionaryEntry dictionaryEntry,
            int reading,
            out string kanji,
            out string kana,
            out string romaji)
        {
            LanguageString alternate;
            bool returnValue = true;

            kanji = String.Empty;
            kana = String.Empty;
            romaji = String.Empty;

            if (dictionaryEntry.LanguageID == JapaneseID)
            {
                kanji = dictionaryEntry.KeyString;

                alternate = dictionaryEntry.GetAlternate(
                    JapaneseKanaID,
                    reading);

                if (alternate != null)
                    kana = alternate.Text;
                else
                {
                    kana = kanji;
                    returnValue = false;
                }

                alternate = dictionaryEntry.GetAlternate(
                    JapaneseRomajiID,
                    reading);

                if (alternate != null)
                    romaji = alternate.Text;
                else
                {
                    romaji = kana;
                    returnValue = false;
                }
            }
            else if (dictionaryEntry.LanguageID == JapaneseKanaID)
            {
                kana = dictionaryEntry.KeyString;

                alternate = dictionaryEntry.GetAlternate(
                    LanguageLookup.Japanese,
                    reading);

                if (alternate != null)
                    kanji = alternate.Text;
                else
                {
                    kanji = kana;
                    returnValue = false;
                }

                alternate = dictionaryEntry.GetAlternate(
                    JapaneseRomajiID,
                    reading);

                if (alternate != null)
                    romaji = alternate.Text;
                else
                {
                    romaji = kana;
                    returnValue = false;
                }
            }
            else if (dictionaryEntry.LanguageID == JapaneseRomajiID)
            {
                romaji = dictionaryEntry.KeyString;

                alternate = dictionaryEntry.GetAlternate(
                    JapaneseID,
                    reading);

                if (alternate != null)
                    kanji = alternate.Text;
                else
                {
                    kanji = romaji;
                    returnValue = false;
                }

                alternate = dictionaryEntry.GetAlternate(
                    JapaneseKanaID,
                    reading);

                if (alternate != null)
                    kana = alternate.Text;
                else
                {
                    kana = kanji;
                    returnValue = false;
                }
            }

            return returnValue;
        }

        protected MultiLanguageString GetJapaneseMultiLanguageStringFromDictionaryEntry(
            DictionaryEntry dictionaryEntry,
            int reading)
        {
            string kanji;
            string kana;
            string romaji;
            MultiLanguageString returnValue = new MultiLanguageString(null, JapaneseLanguageIDs);
            GetJapaneseForms(
                    dictionaryEntry,
                    reading,
                    out kanji,
                    out kana,
                    out romaji);
            SetJapaneseText(returnValue, kanji, kana, romaji);
            return returnValue;
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

        protected bool SetInflectedOutput(
            string stem,
            string ending,
            string englishInflected,
            Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            MultiLanguageString output = inflected.Output;
            MultiLanguageString prefix = inflected.Prefix;
            MultiLanguageString suffix = inflected.Suffix;
            string japaneseInflected = stem + ending;
            string kanaInflected = root.Text(JapaneseKanaID) + ending;
            string romajiInflected;
            string romajiEnding;
            bool returnValue = true;

            if (RomajiConverter.ToTable(out romajiEnding, ending))
                romajiInflected = root.Text(JapaneseRomajiID) + romajiEnding;
            else
            {
                romajiInflected = kanaInflected;
                inflected.AppendError("Error getting romaji ending.");
                returnValue = false;
            }

            output.SetText(JapaneseID, japaneseInflected);
            output.SetText(JapaneseKanaID, kanaInflected);
            output.SetText(JapaneseRomajiID, romajiInflected);
            output.SetText(EnglishID, englishInflected);

            if (stem.StartsWith("お"))
            {
                prefix.SetText(JapaneseID, "お");
                prefix.SetText(JapaneseKanaID, "お");
                prefix.SetText(JapaneseRomajiID, "o");
                RemoveFirstCharacter(root);
            }

            suffix.SetText(JapaneseID, ending);
            suffix.SetText(JapaneseKanaID, ending);
            suffix.SetText(JapaneseRomajiID, romajiEnding);

            return returnValue;
        }

        protected bool SetIrregularInflectedOutput(
            string stem,
            string kanaStem,
            string romajiStem,
            string ending,
            string englishInflected,
            Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            MultiLanguageString output = inflected.Output;
            MultiLanguageString prefix = inflected.Prefix;
            MultiLanguageString suffix = inflected.Suffix;
            string japaneseInflected = stem + ending;
            string kanaInflected = kanaStem + ending;
            string romajiInflected;
            string romajiEnding;
            bool returnValue = true;

            if (RomajiConverter.ToTable(out romajiEnding, ending))
            {
                //if ((stem.Length >= 1) && (stem[0] == 'し') && romajiRoot.StartsWith("su"))
                //    romajiRoot = "shi" + romajiRoot.Substring(2);
                romajiInflected = romajiStem + romajiEnding;
                if (!String.IsNullOrEmpty(AuxSep))
                    romajiInflected = romajiInflected.Replace("kudasai", AuxSep + "kudasai");
            }
            else
            {
                romajiInflected = kanaInflected;
                inflected.AppendError("Error getting romaji ending.");
                returnValue = false;
            }

            output.SetText(JapaneseID, japaneseInflected);
            output.SetText(JapaneseKanaID, kanaInflected);
            output.SetText(JapaneseRomajiID, romajiInflected);
            output.SetText(EnglishID, englishInflected);

            if (stem.StartsWith("お"))
            {
                prefix.SetText(JapaneseID, "お");
                prefix.SetText(JapaneseKanaID, "お");
                prefix.SetText(JapaneseRomajiID, "o");

                //if (stem == root.Text(JapaneseID))
                //    RemoveFirstCharacter(root);
            }

            suffix.SetText(JapaneseID, ending);
            suffix.SetText(JapaneseKanaID, ending);
            suffix.SetText(JapaneseRomajiID, romajiEnding);

            return returnValue;
        }

        protected bool SetFullInflectedOutput(
            Inflection inflected,
            string kanjiStem,
            string kanaStem,
            string romajiStem,
            string kanaEnding,
            string romajiEnding,
            string englishInflected)
        {
            MultiLanguageString root = inflected.Root;
            MultiLanguageString output = inflected.Output;
            MultiLanguageString prefix = inflected.Prefix;
            MultiLanguageString suffix = inflected.Suffix;
            string japaneseInflected = kanjiStem + kanaEnding;
            string kanaInflected = kanaStem + kanaEnding;
            string romajiInflected = romajiStem + romajiEnding;
            bool returnValue = true;

            if (!String.IsNullOrEmpty(AuxSep))
                romajiInflected = romajiInflected.Replace("kudasai", AuxSep + "kudasai");

            output.SetText(JapaneseID, japaneseInflected.Trim());
            output.SetText(JapaneseKanaID, kanaInflected.Trim());
            output.SetText(JapaneseRomajiID, romajiInflected.Trim());
            output.SetText(EnglishID, englishInflected.Trim());

            if (kanjiStem.StartsWith("お"))
            {
                kanjiStem = kanjiStem.Substring(1);
                kanaStem = kanaStem.Substring(1);
                romajiStem = romajiStem.Substring(1);
                prefix.SetText(JapaneseID, "お");
                prefix.SetText(JapaneseKanaID, "お");
                prefix.SetText(JapaneseRomajiID, "o");
            }

            root.SetText(JapaneseID, kanjiStem.Trim());
            root.SetText(JapaneseKanaID, kanaStem.Trim());
            root.SetText(JapaneseRomajiID, romajiStem.Trim());

            suffix.SetText(JapaneseID, kanaEnding.Trim());
            suffix.SetText(JapaneseKanaID, kanaEnding.Trim());
            suffix.SetText(JapaneseRomajiID, romajiEnding.Trim());

            return returnValue;
        }

        protected void SetText(MultiLanguageString mls, string kanji, string kana, string romaji, string english)
        {

            mls.SetText(JapaneseID, kanji);
            mls.SetText(JapaneseKanaID, kana);
            mls.SetText(JapaneseRomajiID, romaji);
            mls.SetText(EnglishID, english);
        }

        protected void SetJapaneseText(MultiLanguageString mls, string kanji, string kana, string romaji)
        {

            mls.SetText(JapaneseID, kanji);
            mls.SetText(JapaneseKanaID, kana);
            mls.SetText(JapaneseRomajiID, romaji);
        }

        protected bool RemoveTeEnding(MultiLanguageString output)
        {
            string inText;
            string outText;
            bool returnValue;
            inText = output.Text(JapaneseID);
            returnValue = RemoveTeEnding(inText, out outText);
            if (returnValue)
            {
                output.SetText(JapaneseID, outText);
                inText = output.Text(JapaneseKanaID);
                RemoveTeEnding(inText, out outText);
                output.SetText(JapaneseKanaID, outText);
                inText = output.Text(JapaneseRomajiID);
                RemoveTeEnding(inText, out outText);
                output.SetText(JapaneseRomajiID, outText);
            }
            return returnValue;
        }

        protected bool RemoveTeEnding(string input, out string output)
        {
            if (input.EndsWith("て"))
            {
                output = input.Substring(0, input.Length - 1);
                return true;
            }
            else if (input.EndsWith("te"))
            {
                output = input.Substring(0, input.Length - 2);
                return true;
            }
            output = input;
            return false;
        }

        protected void RomajiTeFixupCheck(MultiLanguageString output, string duplicatedChar)
        {
            string str = output.Text(JapaneseRomajiID);
            if (str.EndsWith("t"))
            {
                str = str.Substring(0, str.Length - 1) + duplicatedChar;
                output.SetText(JapaneseRomajiID, str);
            }
        }

        protected bool RemoveDeEnding(MultiLanguageString output)
        {
            string inText;
            string outText;
            bool returnValue;
            inText = output.Text(JapaneseID);
            returnValue = RemoveDeEnding(inText, out outText);
            if (returnValue)
            {
                output.SetText(JapaneseID, outText);
                inText = output.Text(JapaneseKanaID);
                RemoveDeEnding(inText, out outText);
                output.SetText(JapaneseKanaID, outText);
                inText = output.Text(JapaneseRomajiID);
                RemoveDeEnding(inText, out outText);
                output.SetText(JapaneseRomajiID, outText);
            }
            return returnValue;
        }

        protected bool RemoveDeEnding(string input, out string output)
        {
            if (input.EndsWith("で"))
            {
                output = input.Substring(0, input.Length - 1);
                return true;
            }
            else if (input.EndsWith("de"))
            {
                output = input.Substring(0, input.Length - 2);
                return true;
            }
            output = input;
            return false;
        }

        protected void ReplaceEndingSuToShi(ref string kanjiStem, ref string kanaStem, ref string romajiStem)
        {
            if (kanjiStem.EndsWith("す"))
            {
                kanjiStem = kanjiStem.Substring(0, kanjiStem.Length - 1) + "し";
                kanaStem = kanaStem.Substring(0, kanaStem.Length - 1) + "し";
                romajiStem = romajiStem.Substring(0, romajiStem.Length - 2) + "shi";
            }
        }

        protected void ReplaceEndingKuToKi(ref string kanjiStem, ref string kanaStem, ref string romajiStem)
        {
            if (kanjiStem.EndsWith("く"))
            {
                kanjiStem = kanjiStem.Substring(0, kanjiStem.Length - 1) + "き";
                kanaStem = kanaStem.Substring(0, kanaStem.Length - 1) + "き";
                romajiStem = romajiStem.Substring(0, romajiStem.Length - 2) + "ki";
            }
            else if (kanjiStem.EndsWith("来"))
            {
                kanjiStem = kanjiStem.Substring(0, kanjiStem.Length - 1) + "来";
                kanaStem = kanaStem.Substring(0, kanaStem.Length - 1) + "き";
                romajiStem = romajiStem.Substring(0, romajiStem.Length - 2) + "ki";
            }
        }

        protected void ReplaceEndingKuToKo(ref string kanjiStem, ref string kanaStem, ref string romajiStem)
        {
            if (kanjiStem.EndsWith("く"))
            {
                kanjiStem = kanjiStem.Substring(0, kanjiStem.Length - 1) + "こ";
                kanaStem = kanaStem.Substring(0, kanaStem.Length - 1) + "こ";
                romajiStem = romajiStem.Substring(0, romajiStem.Length - 2) + "ko";
            }
            else if (kanjiStem.EndsWith("来"))
            {
                kanjiStem = kanjiStem.Substring(0, kanjiStem.Length - 1) + "来";
                kanaStem = kanaStem.Substring(0, kanaStem.Length - 1) + "こ";
                romajiStem = romajiStem.Substring(0, romajiStem.Length - 2) + "ko";
            }
        }

        protected void FixupVerbSuru(Inflection inflected)
        {
            MultiLanguageString output = inflected.Output;
            MultiLanguageString root = inflected.Root;
            MultiLanguageString suffix = inflected.Suffix;
            string outputKanji = output.Text(JapaneseID);

            if (String.IsNullOrEmpty(outputKanji))
                return;

            string outputKana = output.Text(JapaneseKanaID);
            string outputRomaji = output.Text(JapaneseRomajiID);
            string rootKanji = root.Text(JapaneseID);
            string rootKana = root.Text(JapaneseKanaID);
            string rootRomaji = root.Text(JapaneseRomajiID);
            string suffixKanji = suffix.Text(JapaneseID);
            string suffixKana = suffix.Text(JapaneseKanaID);
            string suffixRomaji = suffix.Text(JapaneseRomajiID);
            char lastKanji = rootKana[rootKanji.Length - 1];
            char lastKana = rootKana[rootKana.Length - 1];
            int ofs;

            if (lastKana == 'す')
            {
                rootKanji = rootKanji.Substring(0, rootKanji.Length - 1);
                rootKana = rootKana.Substring(0, rootKana.Length - 1);

                ofs = outputKanji.IndexOf(rootKanji);
                suffixKanji = outputKanji.Substring(ofs + rootKanji.Length);

                ofs = outputKana.IndexOf(rootKana);
                suffixKana = outputKana.Substring(ofs + rootKana.Length);

                if (lastKana == 'す')
                    rootRomaji = rootRomaji.Substring(0, rootRomaji.Length - 2);
                else
                    rootRomaji = rootRomaji.Substring(0, rootRomaji.Length - 3);

                ofs = outputRomaji.IndexOf(rootRomaji);
                suffixRomaji = outputRomaji.Substring(ofs + rootRomaji.Length);

                root.SetText(JapaneseID, rootKanji);
                root.SetText(JapaneseKanaID, rootKana);
                root.SetText(JapaneseRomajiID, rootRomaji);
                suffix.SetText(JapaneseID, suffixKanji);
                suffix.SetText(JapaneseKanaID, suffixKana);
                suffix.SetText(JapaneseRomajiID, suffixRomaji);
            }
        }

        protected void FixupVerbKuru(Inflection inflected)
        {
            MultiLanguageString output = inflected.Output;
            MultiLanguageString root = inflected.Root;
            MultiLanguageString suffix = inflected.Suffix;
            string outputKanji = output.Text(JapaneseID);

            if (String.IsNullOrEmpty(outputKanji))
                return;

            string outputKana = output.Text(JapaneseKanaID);
            string outputRomaji = output.Text(JapaneseRomajiID);
            string rootKanji = root.Text(JapaneseID);
            string rootKana = root.Text(JapaneseKanaID);
            string rootRomaji = root.Text(JapaneseRomajiID);
            string suffixKanji = suffix.Text(JapaneseID);
            string suffixKana = suffix.Text(JapaneseKanaID);
            string suffixRomaji = suffix.Text(JapaneseRomajiID);
            char lastKanji = rootKana[rootKanji.Length - 1];
            char lastKana = rootKana[rootKana.Length - 1];
            int ofs;

            if ((lastKana == 'く') || (lastKana == 'き') || (lastKana == 'こ'))
            {
                rootKanji = rootKanji.Substring(0, rootKanji.Length - 1);
                rootKana = rootKana.Substring(0, rootKana.Length - 1);
                rootRomaji = rootRomaji.Substring(0, rootRomaji.Length - 2);

                ofs = outputKanji.IndexOf(rootKanji);
                suffixKanji = outputKanji.Substring(ofs + rootKanji.Length);

                ofs = outputKana.IndexOf(rootKana);
                suffixKana = outputKana.Substring(ofs + rootKana.Length);

                ofs = outputRomaji.IndexOf(rootRomaji);
                suffixRomaji = outputRomaji.Substring(ofs + rootRomaji.Length);

                root.SetText(JapaneseID, rootKanji);
                root.SetText(JapaneseKanaID, rootKana);
                root.SetText(JapaneseRomajiID, rootRomaji);
                suffix.SetText(JapaneseID, suffixKanji);
                suffix.SetText(JapaneseKanaID, suffixKana);
                suffix.SetText(JapaneseRomajiID, suffixRomaji);
            }
        }

        protected void AppendToOutput(
            MultiLanguageString output,
            string kanjiEnding,
            string kanaEnding,
            string romajiEnding)
        {
            output.SetText(JapaneseID, output.Text(JapaneseID) + kanjiEnding);
            output.SetText(JapaneseKanaID, output.Text(JapaneseKanaID) + kanaEnding);
            string romajiRoot = output.Text(JapaneseRomajiID);
            if (romajiRoot.EndsWith("t"))
                romajiRoot = romajiRoot.Substring(0, romajiRoot.Length - 1) + romajiEnding.Substring(0, 1);
            output.SetText(JapaneseRomajiID, romajiRoot + romajiEnding);
        }

        protected void AppendToOutput(
            MultiLanguageString output,
            MultiLanguageString suffix)
        {
            string kanjiEnding = suffix.Text(JapaneseID);
            string kanaEnding = suffix.Text(JapaneseKanaID);
            string romajiEnding = suffix.Text(JapaneseRomajiID);
            string englishEnding = suffix.Text(EnglishID);
            output.SetText(JapaneseID, output.Text(JapaneseID) + kanjiEnding);
            output.SetText(JapaneseKanaID, output.Text(JapaneseKanaID) + kanaEnding);
            string romajiRoot = output.Text(JapaneseRomajiID);
            if (romajiRoot.EndsWith("t"))
                romajiRoot = romajiRoot.Substring(0, romajiRoot.Length - 1) + romajiEnding.Substring(0, 1);
            output.SetText(JapaneseRomajiID, romajiRoot + romajiEnding);
            output.SetText(EnglishID, output.Text(EnglishID) + englishEnding);
        }

        protected bool ReplaceSuffix(
            MultiLanguageString mls,
            string kanjiIn,
            string kanaIn,
            string romajiIn,
            string kanjiOut,
            string kanaOut,
            string romajiOut)
        {
            string kanji = mls.Text(JapaneseID);
            string kana = mls.Text(JapaneseKanaID);
            string romaji = mls.Text(JapaneseRomajiID);
            bool returnValue = true;
            if (kanji.EndsWith(kanjiIn))
                mls.SetText(JapaneseID, kanji.Substring(0, kanjiIn.Length) + kanjiOut);
            else
                returnValue = false;
            if (kana.EndsWith(kanaIn))
                mls.SetText(JapaneseKanaID, kana.Substring(0, kanaIn.Length) + kanaOut);
            romaji = romaji.Replace(" ", "");
            if (romaji.EndsWith(romajiIn))
                mls.SetText(JapaneseRomajiID, romaji.Substring(0, romajiIn.Length) + romajiOut);
            return returnValue;
        }

        protected bool Replace(
            MultiLanguageString mls,
            string kanjiIn,
            string kanaIn,
            string romajiIn,
            string kanjiOut,
            string kanaOut,
            string romajiOut)
        {
            string kanji = mls.Text(JapaneseID);
            string kana = mls.Text(JapaneseKanaID);
            string romaji = mls.Text(JapaneseRomajiID);
            bool returnValue = true;
            if (kanji.Contains(kanjiIn))
                mls.SetText(JapaneseID, kanji.Replace(kanjiIn, kanjiOut));
            else
                returnValue = false;
            if (kana.Contains(kanaIn))
                mls.SetText(JapaneseKanaID, kana.Replace(kanaIn, kanaOut));
            romaji = romaji.Replace(" ", "");
            if (romaji.Contains(romajiIn))
                mls.SetText(JapaneseRomajiID, romaji.Replace(romajiIn, romajiOut));
            return returnValue;
        }

        protected void RemoveLastCharacter(MultiLanguageString output)
        {
            string inText;
            string outText;
            inText = output.Text(JapaneseID);
            if (inText.Length != 0)
                outText = inText.Substring(0, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseID, outText);
            inText = output.Text(JapaneseKanaID);
            if (inText.Length != 0)
                outText = inText.Substring(0, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseKanaID, outText);
            inText = output.Text(JapaneseRomajiID);
            if (inText.Length != 0)
                outText = inText.Substring(0, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseRomajiID, outText);
        }

        protected void RemoveFirstCharacter(MultiLanguageString output)
        {
            string inText;
            string outText;
            inText = output.Text(JapaneseID);
            if (inText.Length != 0)
                outText = inText.Substring(1, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseID, outText);
            inText = output.Text(JapaneseKanaID);
            if (inText.Length != 0)
                outText = inText.Substring(1, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseKanaID, outText);
            inText = output.Text(JapaneseRomajiID);
            if (inText.Length != 0)
                outText = inText.Substring(1, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseRomajiID, outText);
        }

        protected void RemoveOPrefix(MultiLanguageString output)
        {
            string inText;
            string outText;
            inText = output.Text(JapaneseID);
            if (!inText.StartsWith("お"))
                return;
            if (inText.Length != 0)
                outText = inText.Substring(1, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseID, outText);
            inText = output.Text(JapaneseKanaID);
            if (inText.Length != 0)
                outText = inText.Substring(1, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseKanaID, outText);
            inText = output.Text(JapaneseRomajiID);
            if (inText.Length != 0)
                outText = inText.Substring(1, inText.Length - 1);
            else
                outText = String.Empty;
            output.SetText(JapaneseRomajiID, outText);
        }

        protected void RemoveOPrefix(Inflection inflected)
        {
            RemoveOPrefix(inflected.Root);
            RemoveOPrefix(inflected.DictionaryForm);
            RemoveOPrefix(inflected.Output);
            RemoveOPrefix(inflected.Prefix);
        }

        protected void FixupSuffix(Inflection inflected)
        {
            MultiLanguageString output = inflected.Output;
            MultiLanguageString suffix = inflected.Suffix;
            MultiLanguageString prefix = inflected.Prefix;
            MultiLanguageString root = inflected.Root;

            foreach (LanguageID languageID in JapaneseLanguageIDs)
            {
                int offset = prefix.Text(languageID).Length + root.Text(languageID).Length;
                suffix.SetText(languageID, output.Text(languageID).Substring(offset));
            }
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
