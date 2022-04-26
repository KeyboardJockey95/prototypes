using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatEDict : FormatDictionary
    {
        // EDict header string.
        protected string Header { get; set; }
        protected ConvertRomaji KanaConverter;

        public static string EDictBreakpoint1 = null;
        public static string EDictBreakpoint2 = null;

        // Format data.
        private static string FormatDescription = "Format used for representing a Japanese dictionary.  See: http://www.edrdg.org/jmdict/edict_doc_old.html";

        public FormatEDict(
                string name,
                UserRecord userRecord,
                UserProfile userProfile,
                IMainRepository repositories,
                LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base(
                  name,
                  "FormatEDict",
                  FormatDescription,
                  String.Empty,
                  ".u8",
                  userRecord,
                  userProfile,
                  repositories,
                  languageUtilities,
                  nodeUtilities)
        {
            KanaConverter = null;
            IsAddReciprocals = true;
        }

        public FormatEDict(FormatEDict other)
            : base(other)
        {
            KanaConverter = null;
        }

        public FormatEDict()
            : base("EDict", "FormatEDict", FormatDescription,
                  String.Empty, ".u8", null, null, null, null, null)
        {
            KanaConverter = null;
            IsAddReciprocals = true;
        }

        public override Format Clone()
        {
            return new FormatEDict(this);
        }

        protected override void InitializeRepositories()
        {
            base.InitializeRepositories();

            if (KanaConverter == null)
                KanaConverter = new ConvertRomaji(LanguageLookup.JapaneseKana, '\0', null, false);
        }

        public override void Read(Stream stream)
        {
            try
            {
                PreRead(8);

                FileSize = (int)stream.Length;

                UpdateProgressElapsed("Reading stream ...");

                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;

                    State = StateCode.Reading;

                    // Read header.
                    Header = reader.ReadLine();

                    BytesRead += ApplicationData.Encoding.GetBytes(Header).Length + 1;

                    // Load dictionary with canonical entries.
                    while ((line = reader.ReadLine()) != null)
                        ReadLine(line);

                    WriteDictionary();
                    WriteDictionaryDisplayOutput();
                    SynthesizeMissingAudio();
                }
            }
            catch (Exception exc)
            {
                string msg = exc.Message;

                if (exc.InnerException != null)
                    msg += ": " + exc.InnerException.Message;

                PutError(msg);
            }
            finally
            {
                PostRead();
            }
        }

        protected override void DispatchLine(string line)
        {
            CheckForCancel();

            if (!String.IsNullOrEmpty(line))
            {
                line = line.Replace("　", " ");
                ReadEntry(line);
            }

            BytesRead += ApplicationData.Encoding.GetBytes(line).Length + 1;
        }

        protected override void ReadEntry(string line)
        {
            List<string> targetMeanings = new List<string>(3);
            List<string> hostMeanings = new List<string>(3);
            List<Sense> hostSenses = new List<Sense>();
            string japanese;
            string japaneseKana;
            string romaji;
            string collectedCategoryString = String.Empty;
            string senseCategoryStringPreIndex = String.Empty;
            string senseCategoryStringPostIndex = String.Empty;
            string senseCategoryString = String.Empty;
            LexicalCategory senseCategoryCode = LexicalCategory.Unknown;
            List<string> usedCategories = new List<string>();
            LexicalCategory primaryCategory = LexicalCategory.Unknown;
            string[] japaneseStrings = line.Split(new char[] { ' ' });
            DictionaryEntry dictionaryEntry;

            if ((EntryIndex % 1000) == 0)
                PutStatusMessage("Processing entry " + EntryIndex + ": " + line);

            if (japaneseStrings.Count() >= 1)
                japanese = japaneseStrings[0].Trim();
            else
                throw new ObjectException(Error = "FormatEDict: Fewer than expected Japanese fields: " + line);

            string[] kanaStrings = line.Split(new char[] { '[', ']' });
            string[] englishStrings = line.Split(new char[] { '/' });
            int priorityLevel = 0;
            int reading = 0;

            if (kanaStrings.Count() >= 2)
                japaneseKana = kanaStrings[1].Trim();
            else
                japaneseKana = japanese;

            switch (japanese)
            {
                case "は":
                    romaji = "wa";
                    break;
                case "を":
                    romaji = "o";
                    break;
                case "へ":
                    romaji = "e";
                    break;
                default:
                    if (!KanaConverter.To(out romaji, japaneseKana))
                        romaji = japaneseKana;
                    break;
            }

            if (!ConvertRomaji.IsAllRomaji(romaji))
                PutStatusMessage("Romaji contains kana: " + romaji + ": " + EntryIndex + ": " + line);

            targetMeanings.Add(japanese);
            targetMeanings.Add(japaneseKana);
            targetMeanings.Add(romaji);

            if (!String.IsNullOrEmpty(EDictBreakpoint1))
            {
                if (targetMeanings.Contains(EDictBreakpoint1))
                    ApplicationData.Global.PutConsoleMessage("EDictBreakpoint1: " + EDictBreakpoint1); 
            }

            int count = englishStrings.Count();
            int senseIndex = 0;

            for (int index = 1; index < count; index++)
            {
                string englishString = englishStrings[index].Trim();
                string categoryStrings = String.Empty;
                string filteredCategoryStrings = String.Empty;
                LexicalCategory categoryCode = LexicalCategory.Unknown;
                bool first = true;
                bool hadIndex = false;

                if ((englishString != null) && (englishString != ""))
                {
                    while (englishString.StartsWith("("))
                    {
                        int eofs = englishString.IndexOf(')');

                        if (eofs != -1)
                        {
                            categoryStrings = englishString.Substring(1, eofs - 1).Trim();
                            englishString = englishString.Substring(eofs + 1).Trim();

                            if ((categoryStrings.Length == 1) && (Char.IsDigit(categoryStrings, 0) || (categoryStrings == "P")))
                            {
                                if (categoryStrings == "P")
                                    priorityLevel = 1;
                                else
                                {
                                    try
                                    {
                                        senseIndex = Convert.ToInt32(categoryStrings) - 1;
                                        hadIndex = true;
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                            else
                            {
                                if (first)
                                {
                                    first = false;
                                    senseCategoryStringPostIndex = String.Empty;
                                    if (!hadIndex)
                                    {
                                        senseCategoryStringPreIndex = String.Empty;
                                        senseCategoryCode = LexicalCategory.Unknown;
                                    }
                                    senseCategoryString = String.Empty;
                                }

                                string[] categories = categoryStrings.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string cat in categories)
                                {
                                    LexicalCategory tmpCode;

                                    if (Categories.TryGetValue(cat, out tmpCode))
                                    {
                                        if (tmpCode != LexicalCategory.Unknown)
                                        {
                                            if (primaryCategory == LexicalCategory.Unknown)
                                                primaryCategory = tmpCode;
                                            if (senseCategoryCode == LexicalCategory.Unknown)
                                                senseCategoryCode = tmpCode;
                                        }

                                        if (categoryCode == LexicalCategory.Unknown)
                                            categoryCode = tmpCode;

                                        if ((categoryCode == LexicalCategory.Verb) && romaji.EndsWith("ō"))
                                        {
                                            romaji = romaji.Substring(0, romaji.Length - 1) + "ou";
                                            targetMeanings[2] = romaji;
                                        }

                                        if (!String.IsNullOrEmpty(filteredCategoryStrings))
                                            filteredCategoryStrings += ",";

                                        filteredCategoryStrings += cat;

                                        if (hadIndex)
                                        {
                                            if (!String.IsNullOrEmpty(senseCategoryStringPostIndex))
                                                senseCategoryStringPostIndex += ",";

                                            senseCategoryStringPostIndex += cat;
                                        }
                                        else
                                        {
                                            if (!String.IsNullOrEmpty(senseCategoryStringPreIndex))
                                                senseCategoryStringPreIndex += ",";

                                            senseCategoryStringPreIndex += cat;
                                        }

                                        if (!usedCategories.Contains(cat))
                                        {
                                            if (!String.IsNullOrEmpty(collectedCategoryString))
                                                collectedCategoryString += ",";

                                            collectedCategoryString += cat;
                                            usedCategories.Add(cat);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(englishString))
                {
                    Sense sense = null;

                    if (senseIndex < hostSenses.Count())
                    {
                        sense = hostSenses[senseIndex];
                        LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(HostLanguageIDs.First());
                        ProbableMeaning probableSynonym = new ProbableMeaning(
                            englishString,
                            sense.Category,
                            sense.CategoryString,
                            float.NaN,
                            0,
                            EDictDictionarySourceIDList);
                        languageSynonyms.AddProbableSynonym(probableSynonym);
                    }
                    else
                    {
                        senseCategoryString = senseCategoryStringPreIndex;
                        if (!String.IsNullOrEmpty(senseCategoryStringPostIndex))
                        {
                            if (!String.IsNullOrEmpty(senseCategoryStringPreIndex))
                                senseCategoryString += ",";
                            senseCategoryString += senseCategoryStringPostIndex;
                        }
                        sense = new Sense(
                            reading,
                            senseCategoryCode,
                            senseCategoryString,
                            0,
                            new List<LanguageSynonyms>()
                                {
                                    new LanguageSynonyms(
                                        HostLanguageIDs.First(),
                                        new List<ProbableMeaning>()
                                        {
                                            new ProbableMeaning(
                                                englishString,
                                                senseCategoryCode,
                                                senseCategoryString,
                                                float.NaN,
                                                0,
                                                EDictDictionarySourceIDList)
                                        })
                                },
                            new List<MultiLanguageString>());
                        hostSenses.Add(sense);
                    }
                }
            }

            if (priorityLevel != 0)
            {
                // Because we might not not the priority until the last item, we set it to all senses here.
                foreach (Sense es in hostSenses)
                    es.PriorityLevel = priorityLevel;

                    // For edict, later entries with (P) should have higher priority,
                    // but the sense order in the entry, i.e. (1), (2), (3) should be preserved.
                    // Therefore, to keep the FixUpEntry function from losing this order,
                    // we reverse the order here.

                    // Reverse it in FixUpEntry.
                //hostSenses.Reverse();
            }

            if (!String.IsNullOrEmpty(japanese))
                AddTargetEntry(
                    japanese,
                    LanguageLookup.Japanese,
                    targetMeanings,
                    EDictDictionarySourceIDList,
                    hostSenses,
                    out dictionaryEntry);

            if (!String.IsNullOrEmpty(japaneseKana))
                AddTargetEntry(
                    japaneseKana,
                    LanguageLookup.JapaneseKana,
                    targetMeanings,
                    EDictDictionarySourceIDList,
                    hostSenses,
                    out dictionaryEntry);

            if (!String.IsNullOrEmpty(romaji))
                AddTargetEntry(
                    romaji,
                    LanguageLookup.JapaneseRomaji,
                    targetMeanings,
                    EDictDictionarySourceIDList,
                    hostSenses,
                    out dictionaryEntry);

            if (IsAddReciprocals)
            {
                foreach (Sense englishSense in hostSenses)
                {
                    if (englishSense.LanguageSynonyms != null)
                    {
                        foreach (LanguageSynonyms languageSynonyms in englishSense.LanguageSynonyms)
                        {
                            if (languageSynonyms.LanguageID != LanguageLookup.English)
                                continue;

                            if (languageSynonyms.HasProbableSynonyms())
                            {
                                foreach (ProbableMeaning probableSynonym in languageSynonyms.ProbableSynonyms)
                                    AddHostEntry(
                                        probableSynonym.Meaning,
                                        LanguageLookup.English,
                                        EDictDictionarySourceIDList,
                                        targetMeanings,
                                        primaryCategory,
                                        collectedCategoryString,
                                        englishSense.PriorityLevel);
                            }
                        }
                    }
                }
            }

            EntryIndex++;
        }

        protected override void AddStemCheck(DictionaryEntry dictionaryEntry, Sense sense)
        {
            switch (sense.Category)
            {
                case LexicalCategory.Verb:
                case LexicalCategory.Adjective:
                    break;
                default:
                    return;
            }

            LanguageID targetLanguageID = dictionaryEntry.LanguageID;
            string kanji, kana, romaji;
            string kanjiStem, kanaStem, romajiStem;
            List<string> targetStems;
            string targetStem;
            char lastKana;
            Sense newSense;
            List<Sense> hostSenses;
            DictionaryEntry stemEntry;

            if (GetJapaneseForms(dictionaryEntry, sense.Reading, out kanji, out kana, out romaji))
            {
                kanjiStem = kanji.Substring(0, kanji.Length - 1);
                kanaStem = kana.Substring(0, kana.Length - 1);
                lastKana = kanji[kanji.Length - 1];

                switch (sense.Category)
                {
                    case LexicalCategory.Verb:
                        switch (lastKana)
                        {
                            case 'う':
                                romajiStem = romaji.Substring(0, romaji.Length - 1);
                                break;
                            case 'ぶ':
                            case 'く':
                            case 'む':
                            case 'ぬ':
                            case 'る':
                            case 'す':
                                romajiStem = romaji.Substring(0, romaji.Length - 2);
                                break;
                            case 'つ':
                                romajiStem = romaji.Substring(0, romaji.Length - 3);
                                break;
                            default:
                                return;
                        }
                        break;
                    case LexicalCategory.Adjective:
                        switch (lastKana)
                        {
                            case 'い':
                                romajiStem = romaji.Substring(0, romaji.Length - 1);
                                break;
                            case 'な':
                                romajiStem = romaji.Substring(0, romaji.Length - 2);
                                break;
                            default:
                                return;
                        }
                        break;
                    default:
                        return;
                }

                if (!String.IsNullOrEmpty(EDictBreakpoint2))
                {
                    if ((kanjiStem == EDictBreakpoint2) || (kanaStem == EDictBreakpoint2) || (romajiStem == EDictBreakpoint2))
                        ApplicationData.Global.PutConsoleMessage("EDictBreakpoint2: " + EDictBreakpoint2);
                }

                newSense = new Sense(sense);
                hostSenses = new List<Sense>() { newSense };

                switch (kanjiStem)
                {
                    case "為":
                    case "来":
                        newSense.Category = LexicalCategory.IrregularStem;
                        break;
                    default:
                        newSense.Category = LexicalCategory.Stem;
                        break;
                }

                newSense.AddLanguageSynonyms(
                    new LanguageSynonyms(
                        LanguageLookup.Japanese,
                        new List<ProbableMeaning>(1)
                        {
                            new ProbableMeaning(
                                kanji,
                                newSense.Category,
                                newSense.CategoryString,
                                float.NaN,
                                0,
                                EDictDictionarySourceIDList)
                        }));
                newSense.AddLanguageSynonyms(
                    new LanguageSynonyms(
                        LanguageLookup.JapaneseKana,
                        new List<ProbableMeaning>(1)
                        {
                            new ProbableMeaning(
                                kana,
                                newSense.Category,
                                newSense.CategoryString,
                                float.NaN,
                                0,
                                EDictDictionarySourceIDList)
                        }));
                newSense.AddLanguageSynonyms(
                    new LanguageSynonyms(
                        LanguageLookup.JapaneseRomaji,
                        new List<ProbableMeaning>(1)
                        {
                            new ProbableMeaning(
                                romaji,
                                newSense.Category,
                                newSense.CategoryString,
                                float.NaN,
                                0,
                                EDictDictionarySourceIDList)
                        }));

                switch (targetLanguageID.ExtensionCode)
                {
                    default:
                        targetStem = kanjiStem;
                        break;
                    case "kn":
                        targetStem = kanaStem;
                        break;
                    case "rj":
                        targetStem = romajiStem;
                        break;
                }

                targetStems = new List<string>(3)
                {
                    kanjiStem,
                    kanaStem,
                    romajiStem
                };

                AddTargetStemEntry(
                    targetStem,
                    targetLanguageID,
                    targetStems,
                    EDictDictionarySourceIDList,
                    hostSenses,
                    out stemEntry);

                switch (kanji)
                {
                    case "為る":
                        if (kana == "する")
                        {
                            AddSpecialStemTargetEntry(
                                dictionaryEntry,
                                sense,
                                targetLanguageID,
                                "為る",
                                "する",
                                "suru",
                                "し",
                                "し",
                                "shi");
                        }
                        break;
                    case "来る":
                        if (kana == "くる")
                        {
                            AddSpecialStemTargetEntry(
                                dictionaryEntry,
                                sense,
                                targetLanguageID,
                                "来る",
                                "くる",
                                "kuru",
                                "き",
                                "き",
                                "ki");
                            AddSpecialStemTargetEntry(
                                dictionaryEntry,
                                sense,
                                targetLanguageID,
                                "来る",
                                "くる",
                                "kuru",
                                "こ",
                                "こ",
                                "ko");
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        protected void AddSpecialStemTargetEntry(
            DictionaryEntry dictionaryEntry,
            Sense sense,
            LanguageID targetLanguageID,
            string kanji,
            string kana,
            string romaji,
            string kanjiStem,
            string kanaStem,
            string romajiStem)
        {
            List<string> targetStems;
            string targetStem;
            Sense newSense;
            List<Sense> hostSenses;
            DictionaryEntry stemEntry;

            newSense = new Sense(sense);
            hostSenses = new List<Sense>() { newSense };

            switch (kanjiStem)
            {
                case "為":
                case "来":
                    newSense.Category = LexicalCategory.IrregularStem;
                    break;
                default:
                    newSense.Category = LexicalCategory.Stem;
                    break;
            }

            newSense.AddLanguageSynonyms(
                new LanguageSynonyms(
                    LanguageLookup.Japanese,
                    new List<ProbableMeaning>(1)
                    {
                            new ProbableMeaning(
                                kanji,
                                newSense.Category,
                                newSense.CategoryString,
                                float.NaN,
                                0,
                                EDictDictionarySourceIDList)
                    }));
            newSense.AddLanguageSynonyms(
                new LanguageSynonyms(
                    LanguageLookup.JapaneseKana,
                    new List<ProbableMeaning>(1)
                    {
                            new ProbableMeaning(
                                kana,
                                newSense.Category,
                                newSense.CategoryString,
                                float.NaN,
                                0,
                                EDictDictionarySourceIDList)
                    }));
            newSense.AddLanguageSynonyms(
                new LanguageSynonyms(
                    LanguageLookup.JapaneseRomaji,
                    new List<ProbableMeaning>(1)
                    {
                            new ProbableMeaning(
                                romaji,
                                newSense.Category,
                                newSense.CategoryString,
                                float.NaN,
                                0,
                                EDictDictionarySourceIDList)
                    }));

            switch (targetLanguageID.ExtensionCode)
            {
                default:
                    targetStem = kanjiStem;
                    break;
                case "kn":
                    targetStem = kanaStem;
                    break;
                case "rj":
                    targetStem = romajiStem;
                    break;
            }

            targetStems = new List<string>(3)
            {
                kanjiStem,
                kanaStem,
                romajiStem
            };

            AddTargetStemEntry(
                targetStem,
                targetLanguageID,
                targetStems,
                EDictDictionarySourceIDList,
                hostSenses,
                out stemEntry);
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

            if (dictionaryEntry.LanguageID == LanguageLookup.Japanese)
            {
                kanji = dictionaryEntry.KeyString;

                alternate = dictionaryEntry.GetAlternate(
                    LanguageLookup.JapaneseKana,
                    reading);

                if (alternate != null)
                    kana = alternate.Text;
                else
                {
                    kana = kanji;
                    returnValue = false;
                }

                alternate = dictionaryEntry.GetAlternate(
                    LanguageLookup.JapaneseRomaji,
                    reading);

                if (alternate != null)
                    romaji = alternate.Text;
                else
                {
                    romaji = kana;
                    returnValue = false;
                }
            }
            else if (dictionaryEntry.LanguageID == LanguageLookup.JapaneseKana)
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
                    LanguageLookup.JapaneseRomaji,
                    reading);

                if (alternate != null)
                    romaji = alternate.Text;
                else
                {
                    romaji = kana;
                    returnValue = false;
                }
            }
            else if (dictionaryEntry.LanguageID == LanguageLookup.JapaneseRomaji)
            {
                romaji = dictionaryEntry.KeyString;

                alternate = dictionaryEntry.GetAlternate(
                    LanguageLookup.Japanese,
                    reading);

                if (alternate != null)
                    kanji = alternate.Text;
                else
                {
                    kanji = romaji;
                    returnValue = false;
                }

                alternate = dictionaryEntry.GetAlternate(
                    LanguageLookup.JapaneseKana,
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

        protected override void FixUpEntry(DictionaryEntry entry)
        {
            bool changed = false;

            if ((entry.LanguageID.LanguageCode == "ja") && (entry.SenseCount != 0))
            {
                int priorityLevel = 0;

                // We want later entries to have priority.
                entry.Senses.Reverse();

                foreach (Sense sense in entry.Senses)
                {
                    if (sense.PriorityLevel == 0)
                        continue;

                    priorityLevel++;

                    if (priorityLevel > sense.PriorityLevel)
                    {
                        sense.PriorityLevel = priorityLevel;
                        changed = true;
                    }
                    else if (sense.PriorityLevel > priorityLevel)
                    {
                        priorityLevel = sense.PriorityLevel;
                        changed = true;
                    }
                }

                // Handle some special cases.
                if ((entry.KeyString == "する") || (entry.KeyString == "suru"))
                {
                    foreach (Sense sense in entry.Senses)
                    {
                        if (sense.HasLanguage(LanguageLookup.English) &&
                            sense.CategoryString.StartsWith("vs-i") &&
                            sense.HasMeaning("to do", LanguageLookup.English))
                        {
                            sense.PriorityLevel = priorityLevel + 1;
                            changed = true;
                            break;
                        }
                    }
                }
                else if ((entry.KeyString == "できる") || (entry.KeyString == "dekiru"))
                {
                    Sense sense = entry.GetCategorizedSense(LanguageLookup.English, LexicalCategory.Verb, "v1");
                    if (sense != null)
                        sense.PriorityLevel = priorityLevel + 1;
                }
                else if ((entry.KeyString == "言う") || (entry.KeyString == "いう") || (entry.KeyString == "iu"))
                {
                    foreach (Sense sense in entry.Senses)
                    {
                        if (sense.HasLanguage(LanguageLookup.English) &&
                            sense.CategoryString.StartsWith("v5u") &&
                            sense.HasMeaning("to say", LanguageLookup.English))
                        {
                            sense.PriorityLevel = priorityLevel + 1;
                            changed = true;
                            break;
                        }
                    }
                }
                else if ((entry.KeyString == "いる") || (entry.KeyString == "iru"))
                {
                    foreach (Sense sense in entry.Senses)
                    {
                        if (sense.HasLanguage(LanguageLookup.English) &&
                            sense.CategoryString.StartsWith("v1") &&
                            sense.HasMeaning("to exist", LanguageLookup.English))
                        {
                            sense.PriorityLevel = priorityLevel + 1;
                            changed = true;
                            break;
                        }
                    }
                }
                else if ((entry.KeyString == "oru") || (entry.KeyString == "おる"))
                {
                    foreach (Sense sense in entry.Senses)
                    {
                        if (sense.HasLanguage(LanguageLookup.English) &&
                            sense.CategoryString.StartsWith("v5r,vi"))
                        {
                            sense.PriorityLevel = priorityLevel + 1;
                            changed = true;
                            break;
                        }
                    }
                }

                entry.SortPriority();
            }

            if (changed || entry.Modified)
                entry.TouchAndClearModified();
        }

        public static string EDictDictionarySourceName = "EDict";

        protected static int _EDictDictionarySourceID = 0;
        public static int EDictDictionarySourceID
        {
            get
            {
                if (_EDictDictionarySourceID == 0)
                    _EDictDictionarySourceID = ApplicationData.DictionarySourcesLazy.Add(EDictDictionarySourceName);

                return _EDictDictionarySourceID;
            }
        }

        protected static List<int> _EDictDictionarySourceIDList = null;
        public static List<int> EDictDictionarySourceIDList
        {
            get
            {
                if (_EDictDictionarySourceIDList == null)
                    _EDictDictionarySourceIDList = new List<int>(1) { EDictDictionarySourceID };

                return _EDictDictionarySourceIDList;
            }
        }

        public static new string TypeStringStatic { get { return "EDict"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }

        public static Dictionary<string, LexicalCategory> Categories = new Dictionary<string, LexicalCategory>()
        {
            {"MA", LexicalCategory.Unknown},
            {"X", LexicalCategory.Unknown},
            {"abbr", LexicalCategory.Unknown},
            {"adj-i", LexicalCategory.Adjective},
            {"adj-na", LexicalCategory.Adjective},
            {"adj-no", LexicalCategory.Adjective},
            {"adj-pn", LexicalCategory.Adjective},
            {"adj-t", LexicalCategory.Adjective},
            {"adj-f", LexicalCategory.Adjective},
            {"adj", LexicalCategory.Adjective},
            {"adv", LexicalCategory.Adverb},
            {"adv-to", LexicalCategory.Unknown},
            {"arch", LexicalCategory.Unknown},
            {"ateji", LexicalCategory.Unknown},
            {"aux", LexicalCategory.Unknown},
            {"aux-v", LexicalCategory.Unknown},
            {"aux-adj", LexicalCategory.Unknown},
            {"Buddh", LexicalCategory.Unknown},
            {"chem", LexicalCategory.Unknown},
            {"chn", LexicalCategory.Unknown},
            {"col", LexicalCategory.Unknown},
            {"comp", LexicalCategory.Unknown},
            {"conj", LexicalCategory.Unknown},
            {"ctr", LexicalCategory.Unknown},
            {"derog", LexicalCategory.Unknown},
            {"eK", LexicalCategory.Unknown},
            {"ek", LexicalCategory.Unknown},
            {"exp", LexicalCategory.Unknown},
            {"fam", LexicalCategory.Unknown},
            {"fem", LexicalCategory.Unknown},
            {"food", LexicalCategory.Unknown},
            {"geom", LexicalCategory.Unknown},
            {"gikun", LexicalCategory.Unknown},
            {"hon", LexicalCategory.Unknown},
            {"hum", LexicalCategory.Unknown},
            {"iK", LexicalCategory.Unknown},
            {"id", LexicalCategory.Unknown},
            {"ik", LexicalCategory.Unknown},
            {"int", LexicalCategory.Unknown},
            {"io", LexicalCategory.Unknown},
            {"iv", LexicalCategory.Unknown},
            {"ling", LexicalCategory.Unknown},
            {"m-sl", LexicalCategory.Unknown},
            {"male", LexicalCategory.Unknown},
            {"male-sl", LexicalCategory.Unknown},
            {"math", LexicalCategory.Unknown},
            {"mil", LexicalCategory.Unknown},
            {"n", LexicalCategory.Noun},
            {"n-adv", LexicalCategory.Noun},
            {"n-suf", LexicalCategory.Noun},
            {"n-pref", LexicalCategory.Noun},
            {"n-t", LexicalCategory.Noun},
            {"num", LexicalCategory.Number},
            {"oK", LexicalCategory.Unknown},
            {"obs", LexicalCategory.Unknown},
            {"obsc", LexicalCategory.Unknown},
            {"ok", LexicalCategory.Unknown},
            {"on-mim", LexicalCategory.Unknown},
            {"pn", LexicalCategory.Unknown},
            {"poet", LexicalCategory.Unknown},
            {"pol", LexicalCategory.Unknown},
            {"pref", LexicalCategory.Unknown},
            {"proverb", LexicalCategory.Unknown},
            {"prt", LexicalCategory.Unknown},
            {"physics", LexicalCategory.Unknown},
            {"rare", LexicalCategory.Unknown},
            {"sens", LexicalCategory.Unknown},
            {"sl", LexicalCategory.Unknown},
            {"suf", LexicalCategory.Unknown},
            {"uK", LexicalCategory.Unknown},
            {"uk", LexicalCategory.Unknown},
            {"v1", LexicalCategory.Verb},
            {"v2a-s", LexicalCategory.Verb},
            {"v4h", LexicalCategory.Verb},
            {"v4r", LexicalCategory.Verb},
            {"v5", LexicalCategory.Verb},
            {"v5aru", LexicalCategory.Verb},
            {"v5b", LexicalCategory.Verb},
            {"v5g", LexicalCategory.Verb},
            {"v5k", LexicalCategory.Verb},
            {"v5k-s", LexicalCategory.Verb},
            {"v5m", LexicalCategory.Verb},
            {"v5n", LexicalCategory.Verb},
            {"v5r", LexicalCategory.Verb},
            {"v5r-i", LexicalCategory.Verb},
            {"v5s", LexicalCategory.Verb},
            {"v5t", LexicalCategory.Verb},
            {"v5u", LexicalCategory.Verb},
            {"v5u-s", LexicalCategory.Verb},
            {"v5uru", LexicalCategory.Verb},
            {"v5z", LexicalCategory.Verb},
            {"vz", LexicalCategory.Verb},
            {"vi", LexicalCategory.Verb},
            {"vk", LexicalCategory.Verb},
            {"vn", LexicalCategory.Verb},
            {"vr", LexicalCategory.Verb},
            {"vs", LexicalCategory.Verb},
            {"vs-c", LexicalCategory.Verb},
            {"vs-s", LexicalCategory.Verb},
            {"vs-i", LexicalCategory.Verb},
            {"kyb", LexicalCategory.Unknown},
            {"osb", LexicalCategory.Unknown},
            {"ksb", LexicalCategory.Unknown},
            {"ktb", LexicalCategory.Unknown},
            {"tsb", LexicalCategory.Unknown},
            {"thb", LexicalCategory.Unknown},
            {"tsug", LexicalCategory.Unknown},
            {"kyu", LexicalCategory.Unknown},
            {"rkb", LexicalCategory.Unknown},
            {"nab", LexicalCategory.Unknown},
            {"vt", LexicalCategory.Unknown},
            {"vulg", LexicalCategory.Unknown}
        };

        public static Dictionary<string, string> CategoryDescriptions = new Dictionary<string, string>()
        {
            {"MA", "martial arts term"},
            {"X", "rude or X-rated term"},
            {"abbr", "abbreviation"},
            {"adj-i", "adjective"},
            {"adj-na", "adjectival nouns or quasi-adjectives"},
            {"adj-no", "nouns which may take the genitive case particle `no'"},
            {"adj-pn", "pre-noun adjectival"},
            {"adj-t", "`taru' adjective"},
            {"adj-f", "noun or verb acting prenominally"},
            {"adj", "former adjective classification (being removed)"},
            {"adv", "adverb"},
            {"adv-to", "adverb taking the `to' particle"},
            {"arch", "archaism"},
            {"ateji", "phonetic reading"},
            {"aux", "auxiliary"},
            {"aux-v", "auxiliary verb"},
            {"aux-adj", "auxiliary adjective"},
            {"Buddh", "Buddhist term"},
            {"chem", "chemistry term"},
            {"chn", "children's language"},
            {"col", "colloquialism"},
            {"comp", "computer terminology"},
            {"conj", "conjunction"},
            {"ctr", "counter"},
            {"derog", "derogatory"},
            {"eK", "exclusively kanji"},
            {"ek", "exclusively kana"},
            {"exp", "Expressions (phrases, clauses, etc.)"},
            {"fam", "familiar language"},
            {"fem", "female term or language"},
            {"food", "food term"},
            {"geom", "geometry term"},
            {"gikun", "gikun (meaning as reading) or jukujikun (special kanji reading)"},
            {"hon", "honorific or respectful language"},
            {"hum", "humble language"},
            {"iK", "word containing irregular kanji usage"},
            {"id", "idiomatic expression"},
            {"ik", "word containing irregular kana usage"},
            {"int", "interjection (kandoushi)"},
            {"io", "irregular okurigana usage"},
            {"iv", "irregular verb"},
            {"ling", "linguistics terminology"},
            {"m-sl", "manga slang"},
            {"male", "male term or language"},
            {"male-sl", "male slang"},
            {"math", "mathematics"},
            {"mil", "military"},
            {"n", "noun (common)"},
            {"n-adv", "adverbial noun"},
            {"n-suf", "noun, used as a suffix"},
            {"n-pref", "noun, used as a prefix"},
            {"n-t", "noun (temporal)"},
            {"num", "numeric"},
            {"oK", "word containing out-dated kanji"},
            {"obs", "obsolete term"},
            {"obsc", "obscure term"},
            {"ok", "out-dated or obsolete kana usage"},
            {"on-mim", "onomatopoeic or mimetic word"},
            {"pn", "pronoun"},
            {"poet", "poetical term"},
            {"pol", "polite (teineigo) language"},
            {"pref", "prefix"},
            {"proverb", "proverb"},
            {"prt", "particle"},
            {"physics", "physics terminology"},
            {"rare", "rare"},
            {"sens", "sensitive"},
            {"sl", "slang"},
            {"suf", "suffix"},
            {"uK", "word usually written using kanji alone"},
            {"uk", "word usually written using kana alone"},
            {"v1", "Ichidan (ru) verb"},
            {"v2a-s", "Nidan verb with 'u' ending (archaic)"},
            {"v4h", "Yondan verb with `hu/fu' ending (archaic)"},
            {"v4r", "Yondan verb with `ru' ending (archaic)"},
            {"v5", "Godan verb (not completely classified)"},
            {"v5aru", "Godan verb - -aru special class"},
            {"v5b", "Godan verb with `bu' ending"},
            {"v5g", "Godan verb with `gu' ending"},
            {"v5k", "Godan verb with `ku' ending"},
            {"v5k-s", "Godan verb - Iku/Yuku special class"},
            {"v5m", "Godan verb with `mu' ending"},
            {"v5n", "Godan verb with `nu' ending"},
            {"v5r", "Godan verb with `ru' ending"},
            {"v5r-i", "Godan verb with `ru' ending (irregular verb)"},
            {"v5s", "Godan verb with `su' ending"},
            {"v5t", "Godan verb with `tsu' ending"},
            {"v5u", "Godan verb with `u' ending"},
            {"v5u-s", "Godan verb with `u' ending (special class)"},
            {"v5uru", "Godan verb - Uru old class verb (old form of Eru)"},
            {"v5z", "Godan verb with `zu' ending"},
            {"vz", "Ichidan verb - zuru verb (alternative form of -jiru verbs)"},
            {"vi", "intransitive verb"},
            {"vk", "Kuru verb - special class"},
            {"vn", "irregular nu verb"},
            {"vr", "irregular ru verb, plain form ends with -ri"},
            {"vs", "noun or participle which takes the aux. verb suru"},
            {"vs-c", "su verb - precursor to the modern suru"},
            {"vs-s", "suru verb - special class"},
            {"vs-i", "suru verb - irregular"},
            {"kyb", "Kyoto-ben"},
            {"osb", "Osaka-ben"},
            {"ksb", "Kansai-ben"},
            {"ktb", "Kantou-ben"},
            {"tsb", "Tosa-ben"},
            {"thb", "Touhoku-ben"},
            {"tsug", "Tsugaru-ben"},
            {"kyu", "Kyuushuu-ben"},
            {"rkb", "Ryuukyuu-ben"},
            {"nab", "Nagano-ben"},
            {"vt", "transitive verb"},
            {"vulg", "vulgar expression or word"}
        };
    }
}
