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
        public override List<LanguageID> InflectionLanguageIDs
        {
            get
            {
                return LocalInflectionLanguageIDs;
            }
        }

        public override bool FixupDictionaryFormAndCategories(
            MultiLanguageString dictionaryForm,
            MultiLanguageString stem,
            LexicalCategory expectedCategory,
            ref LexicalCategory category,
            ref string categoryString,
            ref string className,
            ref string subClassName)
        {
            if ((expectedCategory == LexicalCategory.Verb) && (category == LexicalCategory.Noun))
            {
                switch (className)
                {
                    case "suru":
                    case "":
                    case null:
                        category = LexicalCategory.Verb;
                        className = "suru";
                        subClassName = "ru";
                        if (dictionaryForm.HasText(JapaneseID))
                        {
                            dictionaryForm.LanguageString(JapaneseID).Text += "する";
                            stem.LanguageString(JapaneseID).Text += "す";
                        }
                        if (dictionaryForm.HasText(JapaneseKanaID))
                        {
                            dictionaryForm.LanguageString(JapaneseKanaID).Text += "する";
                            stem.LanguageString(JapaneseKanaID).Text += "す";
                        }
                        if (dictionaryForm.HasText(JapaneseRomajiID))
                        {
                            dictionaryForm.LanguageString(JapaneseRomajiID).Text += "suru";
                            stem.LanguageString(JapaneseRomajiID).Text += "su";
                        }
                        break;
                    default:
                        break;
                }
            }

            return true;
        }

        public override string GetStemAndClasses(
            string word,
            LanguageID languageID,
            out string categoryString,
            out string classCode,
            out string subClassCode)
        {
            categoryString = null;
            classCode = String.Empty;
            subClassCode = String.Empty;

            word = TextUtilities.FilterAsides(word).Trim();

            if (String.IsNullOrEmpty(word))
                return null;

            if (languageID == null)
                return null;

            if (languageID.LanguageCode != "ja")
                return null;

            string languageCode = languageID.LanguageCultureExtensionCode;
            string endingRomaji = String.Empty;
            char charBeforeEnding = '\0';
            string stem = null;

            switch (languageCode)
            {
                case "ja":
                case "ja--kn":
                    {
                        char endingChr = word[word.Length - 1];
                        RomajiConverter.ConvertCharacterToRomanization(endingChr, out endingRomaji);
                        if (!String.IsNullOrEmpty(endingRomaji))
                        {
                            if (endingRomaji == "ru")
                            {
                                if (word.Length >= 2)
                                {
                                    char before = word[word.Length - 2];
                                    string beforeRomaji;
                                    RomajiConverter.ConvertCharacterToRomanization(before, out beforeRomaji);
                                    if (!String.IsNullOrEmpty(beforeRomaji))
                                    {
                                        charBeforeEnding = beforeRomaji[beforeRomaji.Length - 1];
                                        if ((charBeforeEnding == 'i') || (charBeforeEnding == 'e'))
                                            endingRomaji = charBeforeEnding.ToString() + endingRomaji;
                                    }
                                }
                            }
                            stem = word.Substring(0, word.Length - 1);
                        }
                    }
                    break;
                case "ja--rj":
                    {
                        char endingChr = word[word.Length - 1];
                        if (endingChr == 'u')
                        {
                            if (word.Length >= 2)
                            {
                                char before = word[word.Length - 2];
                                switch (before)
                                {
                                    case 'b':
                                    case 'g':
                                    case 'k':
                                    case 'm':
                                    case 'n':
                                        endingRomaji = word.Substring(word.Length - 2);
                                        stem = word.Substring(0, word.Length - 2);
                                        break;
                                    case 'r':
                                        endingRomaji = word.Substring(word.Length - 2);
                                        before = word[word.Length - (endingRomaji.Length + 1)];
                                        if ((before == 'i') || (before == 'e'))
                                            endingRomaji = before.ToString() + endingRomaji;
                                        stem = word.Substring(0, word.Length - 2);
                                        break;
                                    case 's':
                                        if ((word.Length > 2) && (word[word.Length - 3] == 't'))
                                        {
                                            endingRomaji = word.Substring(word.Length - 3);
                                            stem = word.Substring(0, word.Length - 3);
                                        }
                                        else
                                        {
                                            endingRomaji = word.Substring(word.Length - 2);
                                            stem = word.Substring(0, word.Length - 2);
                                        }
                                        break;
                                    default:
                                        endingRomaji = "u";
                                        stem = word.Substring(0, word.Length - 1);
                                        break;
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            if (!String.IsNullOrEmpty(endingRomaji))
            {
                switch (endingRomaji)
                {
                    case "bu":
                        categoryString = "v5b";
                        classCode = "godan";
                        subClassCode = "bu";
                        break;
                    case "eru":
                        categoryString = "v1";
                        classCode = "ichidan";
                        subClassCode = "irueru";
                        break;
                    case "iru":
                        categoryString = "v1";
                        classCode = "ichidan";
                        subClassCode = "irueru";
                        break;
                    case "gu":
                        categoryString = "v5b";
                        classCode = "godan";
                        subClassCode = "gu";
                        break;
                    case "ku":
                        categoryString = "v5k";
                        classCode = "godan";
                        subClassCode = "ku";
                        break;
                    case "mu":
                        categoryString = "v5m";
                        classCode = "godan";
                        subClassCode = "mu";
                        break;
                    case "nu":
                        categoryString = "v5n";
                        classCode = "godan";
                        subClassCode = "nu";
                        break;
                    case "ru":
                        if (word.EndsWith("来る") || word.EndsWith("くる") || word.EndsWith("kuru"))
                        {
                            categoryString = "vk";
                            classCode = "kuru";
                            subClassCode = "ru";
                        }
                        else if (word.EndsWith("為る") || word.EndsWith("する") || word.EndsWith("suru"))
                        {
                            categoryString = "vs";
                            classCode = "suru";
                            subClassCode = "ru";
                        }
                        else
                        {
                            categoryString = "v5r";
                            classCode = "godan";
                            subClassCode = "ru";
                        }
                        break;
                    case "su":
                        categoryString = "v5s";
                        classCode = "godan";
                        subClassCode = "su";
                        break;
                    case "tsu":
                        categoryString = "v5t";
                        classCode = "godan";
                        subClassCode = "tsu";
                        break;
                    case "u":
                        categoryString = "v5u";
                        classCode = "godan";
                        subClassCode = "u";
                        break;
                    default:
                        break;
                }
            }

            return stem;
        }

        public override LexicalCategory GetCategoryFromCategoryString(string categoryString)
        {
            if (String.IsNullOrEmpty(categoryString))
                return LexicalCategory.Unknown;

            LexicalCategory category = LexicalCategory.Unknown;
            string[] parts = categoryString.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                string cat = part.Trim();

                switch (cat)
                {
                    case "MA":      // "martial arts term"
                    case "X":       // "rude or X-rated term"},
                    case "abbr":    // "abbreviation"},
                        break;
                    case "adj-i":   // "adjective"},
                    case "adj-na":  // "adjectival nouns or quasi-adjectives"},
                    case "adj-no":  // "nouns which may take the genitive case particle `no'"},
                    case "adj-pn":  // "pre-noun adjectival"},
                    case "adj-t":   // "`taru' adjective"},
                    case "adj-f":   // "noun or verb acting prenominally"},
                    case "adj":     // "former adjective classification (being removed)"},
                        category = LexicalCategory.Adjective;
                        break;
                    case "adv":     // "adverb"},
                    case "adv-to":  // "adverb taking the `to' particle"},
                        category = LexicalCategory.Adverb;
                        break;
                    case "arch":    // "archaism"},
                    case "ateji":   // "phonetic reading"},
                    case "aux":     // "auxiliary"},
                    case "aux-v":   // "auxiliary verb"},
                    case "aux-adj": // "auxiliary adjective"},
                    case "Buddh":   // "Buddhist term"},
                    case "chem":    // "chemistry term"},
                    case "chn":     // "children's language"},
                    case "col":     // "colloquialism"},
                    case "comp":    // "computer terminology"},
                        break;
                    case "conj":    // "conjunction"},
                        category = LexicalCategory.Conjunction;
                        break;
                    case "ctr":     // "counter"},
                        category = LexicalCategory.MeasureWord;
                        break;
                    case "derog":   // "derogatory"},
                    case "eK":      // "exclusively kanji"},
                    case "ek":      // "exclusively kana"},
                    case "exp":     // "Expressions (phrases:      // clauses:      // etc.)"},
                    case "fam":     // "familiar language"},
                    case "fem":     // "female term or language"},
                    case "food":    // "food term"},
                    case "geom":    // "geometry term"},
                    case "gikun":   // "gikun (meaning as reading) or jukujikun (special kanji reading)"},
                    case "hon":     // "honorific or respectful language"},
                    case "hum":     // "humble language"},
                    case "iK":      // "word containing irregular kanji usage"},
                    case "id":      // "idiomatic expression"},
                    case "ik":      // "word containing irregular kana usage"},
                    case "int":     // "interjection (kandoushi)"},
                    case "io":      // "irregular okurigana usage"},
                    case "iv":      // "irregular verb"},
                    case "ling":    // "linguistics terminology"},
                    case "m-sl":    // "manga slang"},
                    case "male":    // "male term or language"},
                    case "male-sl": // "male slang"},
                    case "math":    // "mathematics"},
                    case "mil":     // "military"},
                        break;
                    case "n":       // "noun (common)"},
                    case "n-adv":   // "adverbial noun"},
                    case "n-suf":   // "noun:      // used as a suffix"},
                    case "n-pref":  // "noun:      // used as a prefix"},
                    case "n-t":     // "noun (temporal)"},
                        category = LexicalCategory.Noun;
                        break;
                    case "num":     // "numeric"},
                        category = LexicalCategory.Number;
                        break;
                    case "oK":      // "word containing out-dated kanji"},
                    case "obs":     // "obsolete term"},
                    case "obsc":    // "obscure term"},
                    case "ok":      // "out-dated or obsolete kana usage"},
                    case "on-mim":  // "onomatopoeic or mimetic word"},
                    case "pn":      // "pronoun"},
                    case "poet":    // "poetical term"},
                    case "pol":     // "polite (teineigo) language"},
                    case "pref":    // "prefix"},
                    case "proverb": // "proverb"},
                    case "prt":     // "particle"},
                    case "physics": // "physics terminology"},
                    case "rare":    // "rare"},
                    case "sens":    // "sensitive"},
                    case "sl":      // "slang"},
                    case "suf":     // "suffix"},
                    case "uK":      // "word usually written using kanji alone"},
                    case "uk":      // "word usually written using kana alone"},
                        break;
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
                        category = LexicalCategory.Verb;
                        break;
                    case "vi":      // "intransitive verb":
                    case "vt":      // "transitive verb":
                        // Should never get here.
                        break;
                    case "kyb":      // "Kyoto-ben"},
                    case "osb":      // "Osaka-ben"},
                    case "ksb":      // "Kansai-ben"},
                    case "ktb":      // "Kantou-ben"},
                    case "tsb":      // "Tosa-ben"},
                    case "thb":      // "Touhoku-ben"},
                    case "tsug":     // "Tsugaru-ben"},
                    case "kyu":      // "Kyuushuu-ben"},
                    case "rkb":      // "Ryuukyuu-ben"},
                    case "nab":      // "Nagano-ben"},
                    case "vulg":     // "vulgar expression or word"}
                        break;
                    default:
                        break;
                }
            }

            return category;
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

        protected void SetJapaneseText(MultiLanguageString mls, string kanji, string kana, string romaji)
        {

            mls.SetText(JapaneseID, kanji);
            mls.SetText(JapaneseKanaID, kana);
            mls.SetText(JapaneseRomajiID, romaji);
        }
    }
}
