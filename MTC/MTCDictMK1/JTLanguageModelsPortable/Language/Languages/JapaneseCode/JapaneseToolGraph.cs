using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Media;

namespace JTLanguageModelsPortable.Language
{
    public partial class JapaneseToolCode : JapaneseTool
    {
        public static int _JapaneseEndingsTableVersion = 1; // Increment this when the endings change.
        private static LexTable _JapaneseEndingsTable;
        private static bool useFileBasedEndingsTable = true;

        public override LexTable EndingsTable
        {
            get
            {
                InitializeConjugationTableCheck();
                return _JapaneseEndingsTable;
            }
            set
            {
                _JapaneseEndingsTable = value;
            }
        }

        public override void InitializeConjugationTableCheck()
        {
            if (_JapaneseEndingsTable == null)
                InitializeConjugationTable();
        }

        public override void InitializeConjugationTable()
        {
            if (useFileBasedEndingsTable)
            {
                string filePath = ApplicationData.DatabasePath;
                filePath = MediaUtilities.ConcatenateFilePath(filePath, "LexTables");
                filePath = MediaUtilities.ConcatenateFilePath(filePath, "JapaneseLexTable.xml");
                if (FileSingleton.Exists(filePath))
                {
                    byte[] data = FileSingleton.ReadAllBytes(filePath);
                    _JapaneseEndingsTable = new LexTable(
                        TargetLanguageIDs,
                        GetAllVerbDesignations(),
                        GetAllAdjectiveDesignations(),
                        _JapaneseEndingsTableVersion);
                    _JapaneseEndingsTable.BinaryData = data;
                    if (_JapaneseEndingsTable.Version != _JapaneseEndingsTableVersion)
                    {
                        FileSingleton.Delete(filePath);
                        _JapaneseEndingsTable = ComposeEndingsTable();
                        data = _JapaneseEndingsTable.BinaryData;
                        FileSingleton.WriteAllBytes(filePath, data);
                    }
                }
                else
                {
                    _JapaneseEndingsTable = ComposeEndingsTable();
                    byte[] data = _JapaneseEndingsTable.BinaryData;
                    FileSingleton.DirectoryExistsCheck(filePath);
                    FileSingleton.WriteAllBytes(filePath, data);
                }
            }
            else
                _JapaneseEndingsTable = ComposeEndingsTable();
        }

        public override bool HandleStem(
            ref DictionaryEntry dictionaryEntry,
            string remainingText,
            bool wholeWord,
            List<DictionaryEntry> formAndStemDictionaryEntries,
            ref string inflectionText,
            out bool isInflection)
        {
            DictionaryEntry dictionaryFormEntry;
            DictionaryEntry newDictionaryEntry = null;
            int dictionaryFormSenseIndex;
            int dictionaryFormSynonymIndex = 0;
            int senseIndex = 0;
            LexItem lexItem = null;
            bool returnValue = false;

            isInflection = false;

            foreach (Sense sense in dictionaryEntry.Senses)
            {
                if (!sense.HasLanguage(EnglishID))
                    continue;

                if ((sense.Category != LexicalCategory.Stem) && (sense.Category != LexicalCategory.IrregularStem))
                    continue;

                if (lexItem == null)
                {
                    if (wholeWord)
                        lexItem = EndingsTable.ParseExact(remainingText);
                    else
                        lexItem = EndingsTable.ParseLongest(remainingText);

                    if (lexItem == null)
                        continue;

                    inflectionText = dictionaryEntry.KeyString + lexItem.Value;

                    if (formAndStemDictionaryEntries != null)
                        formAndStemDictionaryEntries.Add(dictionaryEntry);
                }

                if (!String.IsNullOrEmpty(sense.CategoryString))
                {
                    string[] parts = sense.CategoryString.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string part in parts)
                    {
                        string cat = part.Trim();
                        Designator designation = lexItem.FindCategoryDesignation(cat);
                        Inflection inflection = null;

                        if (designation == null)
                            continue;

                        dictionaryFormEntry = null;

                        switch (cat)
                        {
                            case "v1":      // "Ichidan verb"
                            case "v2a-s":   // "Nidan verb with 'u' ending (archaic)"
                            case "v4h":     // "Yondan verb with `hu/fu' ending (archaic)"
                            case "v4r":     // "Yondan verb with `ru' ending (archaic)":
                            case "v5":      // "Godan verb (not completely classified)":
                            case "v5aru":   // "Godan verb - -aru special class":
                            case "v5b":     // "Godan verb with `bu' ending":
                            case "v5g":     // "Godan verb with `gu' ending":
                            case "v5k":     // "Godan verb with `ku' ending":
                            case "v5k-s":   // "Godan verb - Iku/Yuku special class":
                            case "v5m":     // "Godan verb with `mu' ending":
                            case "v5n":     // "Godan verb with `nu' ending":
                            case "v5r":     // "Godan verb with `ru' ending":
                            case "v5r-i":   // "Godan verb with `ru' ending (irregular verb)":
                            case "v5s":     // "Godan verb with `su' ending":
                            case "v5t":     // "Godan verb with `tsu' ending":
                            case "v5u":     // "Godan verb with `u' ending":
                            case "v5u-s":   // "Godan verb with `u' ending (special class)":
                            case "v5uru":   // "Godan verb - Uru old class verb (old form of Eru)":
                            case "v5z":     // "Godan verb with `zu' ending":
                            case "vz":      // "Ichidan verb - zuru verb (alternative form of -jiru verbs)":
                            case "vk":      // "Kuru verb - special class":
                            case "vn":      // "irregular nu verb":
                            case "vr":      // "irregular ru verb, plain form ends with -ri":
                            case "vs":      // "noun or participle which takes the aux. verb suru":
                            case "vs-c":    // "su verb - precursor to the modern suru":
                            case "vs-s":    // "suru verb - special class":
                            case "vs-i":    // "suru verb - irregular":
                                if ((dictionaryFormEntry = LookupDictionaryFormEntry(
                                    sense,
                                    out dictionaryFormSenseIndex)) != null)
                                {
                                    if (formAndStemDictionaryEntries != null)
                                        formAndStemDictionaryEntries.Add(dictionaryFormEntry);

                                    if (!ConjugateVerbDictionaryFormDesignated(
                                            dictionaryFormEntry,
                                            ref dictionaryFormSenseIndex,
                                            ref dictionaryFormSynonymIndex,
                                            designation,
                                            out inflection))
                                        inflection = null;
                                }
                                else
                                    inflection = null;
                                break;
                            case "vi":      // "intransitive verb":
                            case "vt":      // "transitive verb":
                                            // Should never get here.
                                break;
                            case "adj-i":
                            case "adj-na":
                                if ((dictionaryFormEntry = LookupDictionaryFormEntry(
                                    sense,
                                    out dictionaryFormSenseIndex)) != null)
                                {
                                    if (formAndStemDictionaryEntries != null)
                                        formAndStemDictionaryEntries.Add(dictionaryFormEntry);

                                    if (!DeclineAdjectiveDictionaryFormDesignated(
                                            dictionaryFormEntry,
                                            ref dictionaryFormSenseIndex,
                                            ref dictionaryFormSynonymIndex,
                                            designation,
                                            out inflection))
                                        inflection = null;
                                }
                                else
                                    inflection = null;
                                break;
                            default:
                                break;
                        }

                        if (inflection == null)
                            continue;

                        string output = inflection.GetOutput(dictionaryEntry.LanguageID);

                        if (output != inflectionText)
                        {
                            if (TextUtilities.RemoveSpaces(output) != TextUtilities.RemoveSpaces(inflectionText))
                                continue;
                        }

                        if (newDictionaryEntry != null)
                        {
                            DictionaryEntry additionalEntry = CreateInflectionDictionaryEntry(
                                inflectionText,
                                dictionaryEntry.LanguageID,
                                inflection,
                                DefaultInflectionOutputMode);
                            newDictionaryEntry.MergeEntry(additionalEntry);
                        }
                        else
                            newDictionaryEntry = CreateInflectionDictionaryEntry(
                                inflectionText,
                                dictionaryEntry.LanguageID,
                                inflection,
                                DefaultInflectionOutputMode);

                        isInflection = true;
                        returnValue = true;
                    }
                }

                senseIndex++;
            }

            if (isInflection)
                dictionaryEntry = newDictionaryEntry;

            return returnValue;
        }

        public override bool GetPossibleInflections(
            DictionaryEntry stemEntry,
            string stemText,
            string remainingText,
            bool isExactLengthOnly,
            out List<DictionaryEntry> inflectionEntries)
        {
            int stemLength = stemText.Length;
            int fullLength = stemLength + remainingText.Length;
            DictionaryEntry newDictionaryEntry = null;
            int senseIndex = 0;
            List<LexItem> lexItems = null;
            LanguageID stemLanguageID = stemEntry.LanguageID;
            string inflectionText = null;
            bool returnValue = false;

            inflectionEntries = null;

            foreach (Sense sense in stemEntry.Senses)
            {
                if ((sense.Category != LexicalCategory.Stem) && (sense.Category != LexicalCategory.IrregularStem))
                    continue;

                if (lexItems == null)
                {
                    if (!EndingsTable.Parse(remainingText, out lexItems))
                        return false;
                }

                if (!String.IsNullOrEmpty(sense.CategoryString))
                {
                    string[] parts = sense.CategoryString.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string part in parts)
                    {
                        string cat = part.Trim();

                        foreach (LexItem lexItem in lexItems)
                        {
                            if (isExactLengthOnly)
                            {
                                if (stemLength + lexItem.Text.Text(stemLanguageID).Length != fullLength)
                                    continue;
                            }

                            if (ProcessCategoryInflections(
                                    stemLanguageID,
                                    stemText,
                                    sense,
                                    cat,
                                    lexItem,
                                    null,
                                    ref newDictionaryEntry,
                                    out inflectionText))
                            {
                                if (inflectionEntries == null)
                                    inflectionEntries = new List<DictionaryEntry>();

                                DictionaryEntry existingEntry = inflectionEntries.FirstOrDefault(
                                    x => (x.KeyString == newDictionaryEntry.KeyString) && (x.LanguageID == newDictionaryEntry.LanguageID));

                                if (existingEntry != null)
                                    existingEntry.MergeEntry(newDictionaryEntry);
                                else
                                    inflectionEntries.Add(newDictionaryEntry);

                                returnValue = true;
                            }
                        }
                    }
                }

                senseIndex++;
            }

            return returnValue;
        }

        protected override bool ProcessCategoryInflections(
            LanguageID stemLanguageID,
            string stemText,
            Sense sense,
            string cat,
            LexItem lexItem,
            List<DictionaryEntry> formAndStemDictionaryEntries,
            ref DictionaryEntry newDictionaryEntry,
            out string inflectionText)
        {
            Designator designation = lexItem.FindCategoryDesignation(cat);
            Inflection inflection = null;
            DictionaryEntry dictionaryFormEntry;
            int dictionaryFormSenseIndex;
            int dictionaryFormSynonymIndex = 0;

            inflectionText = null;

            if (designation == null)
                return false;

            if (lexItem.Text == null)
                return false;

            inflectionText = stemText + lexItem.Text.Text(stemLanguageID);

            switch (cat)
            {
                case "v1":      // "Ichidan verb"
                case "v2a-s":   // "Nidan verb with 'u' ending (archaic)"
                case "v4h":     // "Yondan verb with `hu/fu' ending (archaic)"
                case "v4r":     // "Yondan verb with `ru' ending (archaic)":
                case "v5":      // "Godan verb (not completely classified)":
                case "v5aru":   // "Godan verb - -aru special class":
                case "v5b":     // "Godan verb with `bu' ending":
                case "v5g":     // "Godan verb with `gu' ending":
                case "v5k":     // "Godan verb with `ku' ending":
                case "v5k-s":   // "Godan verb - Iku/Yuku special class":
                case "v5m":     // "Godan verb with `mu' ending":
                case "v5n":     // "Godan verb with `nu' ending":
                case "v5r":     // "Godan verb with `ru' ending":
                case "v5r-i":   // "Godan verb with `ru' ending (irregular verb)":
                case "v5s":     // "Godan verb with `su' ending":
                case "v5t":     // "Godan verb with `tsu' ending":
                case "v5u":     // "Godan verb with `u' ending":
                case "v5u-s":   // "Godan verb with `u' ending (special class)":
                case "v5uru":   // "Godan verb - Uru old class verb (old form of Eru)":
                case "v5z":     // "Godan verb with `zu' ending":
                case "vz":      // "Ichidan verb - zuru verb (alternative form of -jiru verbs)":
                case "vk":      // "Kuru verb - special class":
                case "vn":      // "irregular nu verb":
                case "vr":      // "irregular ru verb, plain form ends with -ri":
                case "vs":      // "noun or participle which takes the aux. verb suru":
                case "vs-c":    // "su verb - precursor to the modern suru":
                case "vs-s":    // "suru verb - special class":
                case "vs-i":    // "suru verb - irregular":
                    if ((dictionaryFormEntry = LookupDictionaryFormEntry(
                        sense,
                        out dictionaryFormSenseIndex)) != null)
                    {
                        if (formAndStemDictionaryEntries != null)
                            formAndStemDictionaryEntries.Add(dictionaryFormEntry);

                        if (!ConjugateVerbDictionaryFormDesignated(
                                dictionaryFormEntry,
                                ref dictionaryFormSenseIndex,
                                ref dictionaryFormSynonymIndex,
                                designation,
                                out inflection))
                            inflection = null;
                    }
                    else
                        inflection = null;
                    break;
                case "vi":      // "intransitive verb":
                case "vt":      // "transitive verb":
                                // Should never get here.
                    break;
                case "adj-i":
                case "adj-na":
                    if ((dictionaryFormEntry = LookupDictionaryFormEntry(
                        sense,
                        out dictionaryFormSenseIndex)) != null)
                    {
                        if (formAndStemDictionaryEntries != null)
                            formAndStemDictionaryEntries.Add(dictionaryFormEntry);

                        if (!DeclineAdjectiveDictionaryFormDesignated(
                                dictionaryFormEntry,
                                ref dictionaryFormSenseIndex,
                                ref dictionaryFormSynonymIndex,
                                designation,
                                out inflection))
                            inflection = null;
                    }
                    else
                        inflection = null;
                    break;
                default:
                    break;
            }

            if (inflection == null)
                return false;

            if (inflection.GetOutput(stemLanguageID) != inflectionText)
            {
                if (stemLanguageID == JapaneseID)
                {
                    if (IsAllKana(inflectionText))
                    {
                        if (inflection.GetOutput(JapaneseKanaID) != inflectionText)
                            return false;
                    }
                    else if (IsAllRomaji(inflectionText))
                    {
                        if (inflection.GetOutput(JapaneseRomajiID) != inflectionText)
                            return false;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }

            if ((newDictionaryEntry != null) && (newDictionaryEntry.KeyString == inflectionText))
            {
                DictionaryEntry additionalEntry = CreateInflectionDictionaryEntry(
                    inflectionText,
                    stemLanguageID,
                    inflection,
                    DefaultInflectionOutputMode);
                newDictionaryEntry.MergeEntry(additionalEntry);
            }
            else
                newDictionaryEntry = CreateInflectionDictionaryEntry(
                    inflectionText,
                    stemLanguageID,
                    inflection,
                    DefaultInflectionOutputMode);

            return true;
        }

        protected string[] CompoundsToSeparate =
        {
            "ときに",
            "ことができます",
            "である",
            "には",
            "のです"
        };

        public override int CompareTextGraphNodeForWeight(
            TextGraphNode x,
            TextGraphNode y,
            TextGraph graph,
            LanguageID targetLanguageID)
        {
            if (x == y)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            DictionaryEntry xe = x.Entry;
            DictionaryEntry ye = y.Entry;

            if ((xe == null) || (ye == null))
            {
                if (xe == ye)
                    return 0;
                else if (xe == null)
                    return -1;
                else
                    return 1;
            }

            bool xIsNumber = xe.HasSenseWithCategory(LexicalCategory.Number);
            bool yIsNumber = ye.HasSenseWithCategory(LexicalCategory.Number);

            if (xIsNumber != yIsNumber)
            {
                if (yIsNumber)
                    return -1;
                return 1;
            }

            string xt = xe.KeyString;
            string yt = ye.KeyString;

            bool xSep = false;
            bool ySep = false;
            bool xNotSep = false;
            bool yNotSep = false;

            if (_WordFixes != null)
            {
                xSep = _WordFixes.IsCompoundToSeparate(xt);
                ySep = _WordFixes.IsCompoundToSeparate(yt);
                xNotSep = _WordFixes.IsCompoundToNotSeparate(xt);
                yNotSep = _WordFixes.IsCompoundToNotSeparate(yt);
            }

            if (xSep != ySep)
            {
                if (ySep)
                    return 1;
                return -1;
            }
            else if (xNotSep != yNotSep)
            {
                if (xNotSep)
                    return 1;
                return -1;
            }

            LanguageID languageIDX = xe.LanguageID;
            LanguageID languageIDY = ye.LanguageID;

            bool xIsStem = xe.HasSenseWithStemOnly();
            bool yIsStem = ye.HasSenseWithStemOnly();

            if (xIsStem != yIsStem)
            {
                if (yIsStem)
                    return 1;
                return -1;
            }

            int xNextSlotBestLength = GetNextSlotBestLength(x, graph);
            int yNextSlotBestLength = GetNextSlotBestLength(y, graph);

            int xCombinedLength = (xNextSlotBestLength >= 0 ? x.Length + xNextSlotBestLength : -1);
            int yCombinedLength = (yNextSlotBestLength >= 0 ? y.Length + yNextSlotBestLength : -1);

            if (xCombinedLength > yCombinedLength)
                return 1;

            if (xCombinedLength < yCombinedLength)
                return -1;

            bool xIsCompound = ConvertRomaji.IsCompound(x.Text);
            bool yIsCompound = ConvertRomaji.IsCompound(y.Text);

            if (xIsCompound != yIsCompound)
            {
                if (yIsCompound)
                    return 1;
                return -1;
            }

            bool xIsInflection = xe.HasSenseWithCategoryOnly(LexicalCategory.Inflection);
            bool yIsInflection = ye.HasSenseWithCategoryOnly(LexicalCategory.Inflection);

            if (targetLanguageID == JapaneseID)
            {
                bool xIsPhonetic = LanguageLookup.IsAlternatePhonetic(languageIDX);
                bool yIsPhonetic = LanguageLookup.IsAlternatePhonetic(languageIDY);

                if (xIsPhonetic || yIsPhonetic)
                {
                    bool xKanaUsually = xIsPhonetic && xe.HasSenseWithCategorySubString("uk");
                    bool yKanaUsually = yIsPhonetic && ye.HasSenseWithCategorySubString("uk");

                    if (xIsPhonetic && !xKanaUsually && !xIsInflection)
                    {
                        if (!yIsPhonetic || yKanaUsually || yIsInflection)
                            return -1;
                    }
                    else if (yIsPhonetic && !yKanaUsually && !yIsInflection)
                        return 1;
                }
            }

            if (x.Length > y.Length)
                return 1;

            if (x.Length < y.Length)
                return -1;

            if (xIsInflection != yIsInflection)
            {
                if (xIsInflection)
                    return 1;
                return -1;
            }

            return 0;
        }

        public override bool HasTextGraphNodeProblem(TextGraphNode node)
        {
            if (node == null)
                return false;

            if ((node.Entry == null) ||
                    ((node.Entry.LanguageID != LanguageID) && !node.Entry.HasSenseWithCategorySubString("uk")))
            {
                if ((WordFixes != null) && (WordFixes.CompoundsToNotSeparate != null))
                {
                    if (WordFixes.CompoundsToNotSeparate.Contains(node.Text))
                        return false;
                }

                return true;
            }

            return false;
        }

        public bool IsCompoundWord(string word)
        {
            return ConvertRomaji.IsCompound(word);
        }

        public override bool LookupDictionaryEntriesFilterCheck(string pattern, LanguageID languageID)
        {
            if (String.IsNullOrEmpty(pattern))
                return true;

            string first = pattern.Substring(0);

            switch (languageID.LanguageCultureExtensionCode)
            {
                case "ja":
                    break;
                case "ja--kn":
                    if (!ConvertRomaji.IsAllKana(first))
                        return false;
                    break;
                case "ja-rj":
                    if (!ConvertRomaji.IsAllRomaji(first))
                        return false;
                    break;
                default:
                    break;
            }

            return true;
        }

        public override DictionaryEntry LookupDictionaryFormEntry(Sense stemSense, out int dictionaryFormSenseIndex)
        {
            LanguageSynonyms languageSynonyms = stemSense.GetLanguageSynonyms(JapaneseID);

            dictionaryFormSenseIndex = 0;

            if (languageSynonyms == null)
                return null;

            string dictionaryForm = languageSynonyms.GetSynonymIndexed(0);

            DictionaryEntry dictionaryFormEntry = GetDictionaryEntry(dictionaryForm);

            if (dictionaryFormEntry != null)
            {
                foreach (Sense sense in dictionaryFormEntry.Senses)
                {
                    if (sense.CategoryString == stemSense.CategoryString)
                        return dictionaryFormEntry;

                    dictionaryFormSenseIndex++;
                }
            }

            return null;
        }

        public override DictionaryEntry CreateInflectionDictionaryEntry(
            string inflectedForm,
            LanguageID languageID,
            Inflection inflection,
            InflectionOutputMode inflectionOutputMode)
        {
            MultiLanguageString output = inflection.Output;

            switch (inflectionOutputMode)
            {
                case InflectionOutputMode.MainWordPlusPrePostWords:
                    output = inflection.MainWordPlusPrePostWords;
                    break;
                case InflectionOutputMode.FullNoPronouns:
                    output = inflection.Output;
                    break;
                case InflectionOutputMode.FullWithPronouns:
                    output = inflection.PronounOutput;
                    break;
                case InflectionOutputMode.FullNoMain:
                    output = inflection.MainWordPlusPrePostWords;
                    break;
                case InflectionOutputMode.All:
                    output = inflection.MainWordPlusPrePostWords;
                    break;
                default:
                    throw new Exception("CreateInflectionDictionaryEntry: Unsupported inflection output mode: " + inflectionOutputMode.ToString());
            }

            string targetText = output.Text(languageID);
            string kanji = output.Text(JapaneseID);
            string kana = output.Text(JapaneseKanaID);
            string romaji = output.Text(JapaneseRomajiID);
            string english = output.Text(EnglishID);
            List<LanguageString> alternates = new List<LanguageString>(2);
            MultiLanguageString dictionaryForm = inflection.DictionaryForm;
            LexicalCategory category = LexicalCategory.Inflection;
            string categoryString = inflection.CategoryString + "," + inflection.Designation.Label;

            if (!String.IsNullOrEmpty(inflection.CategoryString))
                categoryString = inflection.CategoryString + "," + inflection.Designation.Label;
            else
                categoryString = inflection.Designation.Label;

            switch (languageID.LanguageCultureExtensionCode)
            {
                case "ja":
                    alternates.Add(new Object.LanguageString(0, JapaneseKanaID, kana));
                    alternates.Add(new Object.LanguageString(0, JapaneseRomajiID, romaji));
                    break;
                case "ja--kn":
                    alternates.Add(new Object.LanguageString(0, JapaneseID, kanji));
                    alternates.Add(new Object.LanguageString(0, JapaneseRomajiID, romaji));
                    break;
                case "ja--rj":
                    alternates.Add(new Object.LanguageString(0, JapaneseID, kanji));
                    alternates.Add(new Object.LanguageString(0, JapaneseKanaID, kana));
                    break;
                default:
                    break;
            }

            List<LanguageSynonyms> languageSynonymsList = new List<LanguageSynonyms>();
            foreach (LanguageID tid in LanguageIDs)
            {
                ProbableMeaning probableSynonym = new ProbableMeaning(
                    dictionaryForm.Text(tid),
                    category,
                    categoryString,
                    float.NaN,
                    0,
                    InflectionDictionarySourceID);
                List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>() { probableSynonym };
                LanguageSynonyms languageSynonyms = new LanguageSynonyms(tid, probableSynonyms);
                languageSynonymsList.Add(languageSynonyms);
            }
            Sense sense = new Sense(
                0,
                category,
                categoryString,
                0,
                languageSynonymsList,
                null);
            sense.AddInflection(inflection);
            List<Sense> senses = new List<Sense>(1) { sense };

            DictionaryEntry dictionaryEntry = new DictionaryEntry(
                targetText,
                languageID,
                alternates,
                0,
                InflectionDictionarySourceID,
                senses,
                null);

            return dictionaryEntry;
        }

        public override bool LookupSuffix(string text, out LexItem lexItem)
        {
            lexItem = EndingsTable.ParseLongest(text);

            if (lexItem != null)
                return true;

            return false;
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
            if (IsKatakana(chr))
            {
                for (; index < maxIndex; index++)
                {
                    chr = text[index];
                    if (char.IsWhiteSpace(chr) || char.IsPunctuation(chr) || !IsKatakana(chr))
                    {
                        word = text.Substring(textIndex, index - textIndex);
                        return true;
                    }
                }
            }
            else if (IsRomaji(chr))
            {
                for (; index < maxIndex; index++)
                {
                    chr = text[index];
                    if (char.IsWhiteSpace(chr) || char.IsPunctuation(chr) || !IsRomaji(chr))
                    {
                        word = text.Substring(textIndex, index - textIndex);
                        return true;
                    }
                }
            }
            word = text.Substring(textIndex, index - textIndex);
            return true;
        }

        public static bool IsAllKana(string input)
        {
            return ConvertRomaji.IsAllKana(input);
        }

        public static bool IsAllRomaji(string input)
        {
            return ConvertRomaji.IsAllRomaji(input);
        }

        public static bool IsKana(char chr)
        {
            return ConvertRomaji.IsKana(chr);
        }

        public static bool IsKatakana(char chr)
        {
            return ConvertRomaji.IsKatakana(chr);
        }

        public static bool IsRomaji(char chr)
        {
            return ConvertRomaji.IsRomaji(chr);
        }

        public static bool IsCompound(string input)
        {
            return ConvertRomaji.IsCompound(input);
        }

        public static bool IsAllKanaOrRomaji(string input)
        {
            return ConvertRomaji.IsAllKanaOrRomaji(input);
        }

        public override bool GetLongestLengths(
            LanguageID languageID,
            int longestDictionaryEntryLength,
            ref int longestPrefixLength,
            ref int longestSuffixLength,
            ref int longestInflectionLength)
        {
            if (!JapaneseLanguageIDs.Contains(languageID))
                return false;

            longestPrefixLength = 1;
            longestSuffixLength = EndingsTable.GetLongest(languageID);
            longestInflectionLength = longestPrefixLength + longestDictionaryEntryLength + longestSuffixLength;

            return true;
        }

        public override LexTable ComposeEndingsTable()
        {
            string source;
            int index = 0;
            DictionaryEntry dictionaryEntry;
            List<Inflection> inflections;
            LexTable lexTable = new LexTable(
                TargetLanguageIDs,
                GetAllVerbDesignations(),
                GetAllAdjectiveDesignations(),
                _JapaneseEndingsTableVersion);

            if (ApplicationData.IsMobileVersion &&
                    (ApplicationData.RemoteRepositories != null) &&
                    (DictionaryDatabase != ApplicationData.RemoteRepositories.Dictionary))
                CheckGraphTableSource();

            for (index = 0; (source = SourceGodanIchidanVerbs[index]) != null; index ++)
            {
                int senseIndex = -1;
                int synonymIndex = -1;

                dictionaryEntry = GetDictionaryEntry(source);

                if (dictionaryEntry == null)
                {
                    string message = "Error finding verb dictionary entry during conjugation graph load: " + source;
                    //throw new Exception(message);
                    continue;
                }

                inflections = ConjugateVerbDictionaryFormAll(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex);
                AddInflectionEndingsToGraph(
                    lexTable,
                    inflections);
            }

            /* Already in SourceGodanIchidanVerbs table.
            for (index = 0; (source = SourceIrregularVerbs[index]) != null; index++)
            {
                dictionaryEntry = GetDictionaryEntry(source);

                if (dictionaryEntry == null)
                {
                    string message = "Error finding irregular verb dictionary entry during conjugation graph load: " + source;
                    //throw new Exception(message);
                    continue;
                }

                inflections = ConjugateVerbDictionaryFormAll(
                    dictionaryEntry,
                    senseIndex,
                    synonymIndex);
                AddInflectionOutputToGraph(
                    lexTable,
                    inflections);
            }
            */

            /* Not needed anymore becuase suffix endings are now the suru/kuru forms.
            for (index = 0; (source = SourceCompoundVerbs[index]) != null; index++)
            {
                dictionaryEntry = GetDictionaryEntry(source);

                if (dictionaryEntry == null)
                {
                    string message = "Error finding verb dictionary entry during conjugation graph load: " + source;
                    //throw new Exception(message);
                    continue;
                }

                inflections = ConjugateVerbDictionaryFormAll(
                    dictionaryEntry,
                    senseIndex,
                    synonymIndex);
                MultiLanguageString basis = GetJapaneseMultiLanguageStringFromDictionaryEntry(dictionaryEntry, 0);
                AddCompoundInflectionEndingToGraph(
                    lexTable,
                    inflections,
                    basis,
                    ref bestKanjiLength,
                    ref bestKanaLength,
                    ref bestRomajiLength);
            }
            */
            for (index = 0; (source = SourceIAdjectives[index]) != null; index++)
            {
                int senseIndex = -1;
                int synonymIndex = -1;

                dictionaryEntry = GetDictionaryEntry(source);

                if (dictionaryEntry == null)
                {
                    string message = "Error finding adjective dictionary entry during conjugation graph load: " + source;
                    //throw new Exception(message);
                    continue;
                }

                inflections = DeclineAdjectiveDictionaryFormAll(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex);
                AddInflectionEndingsToGraph(
                    lexTable,
                    inflections);
            }

            return lexTable;
        }

        public override bool CheckGraphTableSource()
        {
            List<string> sourceList = new List<string>();
            string source;
            int index = 0;
            DictionaryEntry dictionaryEntry;
            bool returnValue = false;

            for (index = 0; (source = SourceGodanIchidanVerbs[index]) != null; index++)
            {
                dictionaryEntry = GetDictionaryEntry(source);

                if (dictionaryEntry == null)
                {
                    if (sourceList == null)
                        sourceList = new List<string>();

                    sourceList.Add(source);
                }
            }

            for (index = 0; (source = SourceIrregularVerbs[index]) != null; index++)
            {
                dictionaryEntry = GetDictionaryEntry(source);

                if (dictionaryEntry == null)
                {
                    if (sourceList == null)
                        sourceList = new List<string>();

                    sourceList.Add(source);
                }
            }

            /* Not needed anymore becuase suffix endings are now the suru/kuru forms.
            for (index = 0; (source = SourceCompoundVerbs[index]) != null; index++)
            {
                dictionaryEntry = GetDictionaryEntry(source);

                if (dictionaryEntry == null)
                {
                    if (sourceList == null)
                        sourceList = new List<string>();

                    sourceList.Add(source);
                }
            }
            */
            for (index = 0; (source = SourceIAdjectives[index]) != null; index++)
            {
                dictionaryEntry = GetDictionaryEntry(source);

                if (dictionaryEntry == null)
                {
                    if (sourceList == null)
                        sourceList = new List<string>();

                    sourceList.Add(source);
                }
            }

            if ((sourceList != null) && (sourceList.Count() != 0))
            {
                List<DictionaryEntry> dictionaryEntries;
                List<string> dictionaryKeysNotFound;

                returnValue = NodeUtilities.GetServerVocabularyItems(
                    JapaneseID,
                    TargetLanguageIDs,
                    HostLanguageIDs,
                    true,                       // bool translateMissingEntries
                    true,                       // bool synthesizeMissingAudio
                    sourceList,
                    out dictionaryEntries,
                    out dictionaryKeysNotFound);

                if (returnValue)
                    DictionaryCacheFound.AddSingles(Matchers.MatchCode.Exact, dictionaryEntries);
            }

            return returnValue;
        }

        public bool GetRemoteToolDictionaryEntry(
            string entryKey,
            ref JapaneseToolCode remoteTool,
            out DictionaryEntry dictionaryEntry)
        {
            bool returnValue = false;

            dictionaryEntry = null;

            if (ApplicationData.IsMobileVersion &&
                (ApplicationData.RemoteRepositories != null) &&
                (DictionaryDatabase != ApplicationData.RemoteRepositories.Dictionary))
            {
                if (remoteTool == null)
                {
                    if (!ApplicationData.Global.IsConnectedToANetwork())
                    {
                        string message = ApplicationData.ApplicationName + " needs to download some dictionary entries, but the mobile device is not connected to a network.  Please connect to a network and try again.";
                        throw new Exception(message);
                    }

                    remoteTool = new JapaneseToolCode();
                    remoteTool.DictionaryDatabase = ApplicationData.RemoteRepositories.Dictionary;
                }

                dictionaryEntry = remoteTool.GetDictionaryEntry(entryKey);

                if (dictionaryEntry != null)
                {
                    try
                    {
                        if (DictionaryDatabase.Add(dictionaryEntry, dictionaryEntry.LanguageID))
                            returnValue = true;
                    }
                    catch (Exception)
                    {
                        string message = "Error adding dictionary entry: " + entryKey;
                        throw new Exception(message);
                    }
                }
            }

            return returnValue;
        }

        public override void AddInflectionEndingsToGraph(
            LexTable lexTable,
            List<Inflection> inflections)
        {
            if ((lexTable == null) || (inflections == null))
                return;

            string kanji;
            string kana;
            string romaji;

            foreach (Inflection inflection in inflections)
            {
                string category = inflection.CategoryString;
                Designator designation = inflection.Designation;
                MultiLanguageString endings = inflection.Suffix;

                if (endings == null)
                    continue;

                kanji = endings.Text(JapaneseID);
                kana = endings.Text(JapaneseKanaID);
                romaji = endings.Text(JapaneseRomajiID);

                if (kanji != kana)
                    lexTable.Add(kanji, endings, category, designation);

                lexTable.Add(kana, endings, category, designation);
                lexTable.Add(romaji, endings, category, designation);
            }
        }

        public override void AddInflectionOutputToGraph(
            LexTable lexTable,
            List<Inflection> inflections)
        {
            if ((lexTable == null) || (inflections == null))
                return;

            foreach (Inflection inflection in inflections)
            {
                string category = inflection.CategoryString;
                Designator designation = inflection.Designation;
                MultiLanguageString output = inflection.Output;

                if (output == null)
                    continue;

                if (output.Text(JapaneseID) != output.Text(JapaneseKanaID))
                    lexTable.Add(output.Text(JapaneseID), output, category, designation);

                lexTable.Add(output.Text(JapaneseKanaID), output, category, designation);
                lexTable.Add(output.Text(JapaneseRomajiID), output, category, designation);
            }
        }

        public override void AddCompoundInflectionEndingToGraph(
            LexTable lexTable,
            List<Inflection> inflections,
            MultiLanguageString basis)
        {
            if ((lexTable == null) || (inflections == null))
                return;

            MultiLanguageString endings = new MultiLanguageString(null, JapaneseLanguageIDs);

            foreach (Inflection inflection in inflections)
            {
                string category = inflection.CategoryString;
                Designator designation = inflection.Designation;
                MultiLanguageString output = inflection.Output;

                if (output == null)
                    continue;

                foreach (LanguageID languageID in JapaneseLanguageIDs)
                {
                    string basisText = basis.Text(languageID);
                    string outputText = output.Text(languageID);
                    int ofs = outputText.IndexOf(basisText);
                    string endingText;

                    if (ofs != -1)
                        endingText = outputText.Substring(ofs);
                    else
                        endingText = inflection.Suffix.Text(languageID);

                    endings.SetText(languageID, endingText);
                }

                if (endings.Text(JapaneseID) != endings.Text(JapaneseKanaID))
                    lexTable.Add(endings.Text(JapaneseID), endings, category, designation);

                lexTable.Add(endings.Text(JapaneseKanaID), endings, category, designation);
                lexTable.Add(endings.Text(JapaneseRomajiID), endings, category, designation);
            }
        }

        private static string[] SourceGodanIchidanVerbs =
        {
            "洗う",       // "洗う - あらう - arau - wash"
            "待つ",       // "待つ - まつ - matsu - wait"
            "取る",       // "取る - とる - toru - take"
            "書く",       // "書く - かく - kaku - write"
            "急ぐ",       // "急ぐ - いそぐ - isogu - hurry"
            "死ぬ",       // "死ぬ - しぬ - shinu - die"
            "喚ぶ",       // "喚ぶ - よぶ - yobu - call"
            "飲む",       // "飲む - のむ - nomu - drink"
            "話す",       // "話す - はなす - hanasu - talk"
            "見る",       // "見る - みる - miru - see"
            "食べる",     // "食べる - たべる - taberu - eat"
            "起きる",     // "起きる - おきる - okiru - get up
            "する",       // "する - する - suru - do"
            "来る",       // "来る - くる - kuru - come"
            "勉強",       // "勉強する - べんきょする - benkyosuru - study"
            "あたま来る",  // "あたま来る - あたまくる - atamakuru - get mad"
            "ある",       // "ある - ある - aru - exist (inanimate)"
            "いる",       // "いる - いる - iru - exist (animate)",
            "行く",       // "行く - いく - iku - go"
            "返る",       // "返る - かえる - kaeru - return"      // Godan
            "変える",     // "変える - かえる - kaeru - change"    // Ichdan
            "知る",       // "知る - しる - shiru - know"
            "言う",       // "言う - いう - iu - say"
            "上げる",     // "あげる - しる - ageru - give"
            "聞く",       // "聞く - きく - kiku - hear"
            "下さる",       // "下さる - くださる - kudasaru - kudasaru"
            null
        };

        private static string[] SourceIrregularVerbs =
        {
            "する",       // "する - する - suru - do"
            "来る",       // "来る - くる - kuru - come"
            null, null
        };

        private static string[] SourceCompoundVerbs =
        {
            "勉強",       // "勉強する - べんきょする - benkyosuru - study"
            "あたま来る",  // "あたま来る - あたまくる - atamakuru - get mad"
            null, null
        };

        private static string[] SourceIAdjectives =
        {
            "早い",       // "早い - はやい - hayai - fast/early"
            null
        };
    }
}
