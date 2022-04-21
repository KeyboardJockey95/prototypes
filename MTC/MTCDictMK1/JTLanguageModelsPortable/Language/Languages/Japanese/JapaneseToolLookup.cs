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

#if false
        public override DictionaryEntry GetStemDictionaryEntry(string dictionaryForm)
        {
            lock (this)
            {
                DictionaryEntry dictionaryEntry = GetDictionaryLanguageEntry(dictionaryForm, LanguageID);

                if (dictionaryEntry != null)
                {
                    if (dictionaryEntry.HasSenseWithStem())
                        return dictionaryEntry;
                    if (ConvertRomaji.IsAllKana(dictionaryForm))
                        dictionaryEntry = GetDictionaryLanguageEntry(dictionaryForm, JapaneseKanaID);
                    else if (ConvertRomaji.IsAllRomaji(dictionaryForm))
                        dictionaryEntry = GetDictionaryLanguageEntry(dictionaryForm, JapaneseRomajiID);
                    if (dictionaryEntry != null)
                    {
                        if (dictionaryEntry.HasSenseWithStem())
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
                    if (nonStemEntry.HasSenseWithStem())
                    {
                        stemEntry = nonStemEntry;
                        if (!nonStemEntry.HasSenseWithoutStem())
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
                    if (alternateEntry.HasSenseWithStem())
                    {
                        if (stemEntry == null)
                            stemEntry = alternateEntry;
                        if (!alternateEntry.HasSenseWithoutStem())
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
#endif

        public DictionaryEntry CreateStemDictionaryEntry(
            LanguageID keyLanguageID,
            string kanji,
            string kana,
            string romaji,
            List<int> sourceIDs,
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
            LexicalCategory category;
            switch (kanji[0])
            {
                case '為':
                case '来':
                    category = LexicalCategory.IrregularStem;
                    break;
                default:
                    category = LexicalCategory.Stem;
                    break;
            }
            DictionaryEntry dictionaryEntry = new DictionaryEntry(
                key,
                keyLanguageID,
                new List<LanguageString>()
                {
                    new LanguageString(0, altLanguageID1, altText1),
                    new LanguageString(0, altLanguageID2, altText2)
                },
                0,
                sourceIDs,
                new List<Sense>()
                {
                    new Sense(
                        0,
                        category,
                        categoryString,
                        0,
                        new List<LanguageSynonyms>()
                        {
                            new LanguageSynonyms(
                                JapaneseID,
                                new List<ProbableMeaning>(1)
                                {
                                    new ProbableMeaning(
                                        kanji,
                                        category,
                                        categoryString,
                                        float.NaN,
                                        0, 
                                        sourceIDs)
                                }),
                            new LanguageSynonyms(
                                JapaneseKanaID,
                                new List<ProbableMeaning>(1)
                                {
                                    new ProbableMeaning(
                                        kana,
                                        category,
                                        categoryString,
                                        float.NaN,
                                        0,
                                        sourceIDs)
                                }),
                            new LanguageSynonyms(
                                JapaneseRomajiID,
                                new List<ProbableMeaning>(1)
                                {
                                    new ProbableMeaning(
                                        romaji,
                                        category,
                                        categoryString,
                                        float.NaN,
                                        0,
                                        sourceIDs)
                                }),
                            new LanguageSynonyms(
                                EnglishID,
                                new List<ProbableMeaning>(1)
                                {
                                    new ProbableMeaning(
                                        hostText,
                                        category,
                                        categoryString,
                                        float.NaN,
                                        0,
                                        sourceIDs)
                                })
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

                if (languageSynonyms.HasProbableSynonyms())
                {
                    foreach (ProbableMeaning probableSynonym in languageSynonyms.ProbableSynonyms)
                    {
                        sb.Append(probableSynonym.Meaning);
                        sb.Append("/");
                    }
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
    }
}
