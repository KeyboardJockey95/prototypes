using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public partial class JapaneseToolCode : JapaneseTool
    {
        public override List<Inflection> DeclineAdjectiveDictionaryFormAll(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Designator> designations;
            if (IsNaAdjective(dictionaryEntry, senseIndex))
                designations = GetAllNounDesignations();
            else if (IsIAdjective(dictionaryEntry, senseIndex))
                designations = GetDefaultAdjectiveDesignations();
            else
                designations = GetDefaultNounDesignations();
            List<Inflection> inflections = DeclineAdjectiveDictionaryFormSelected(
                dictionaryEntry,
                ref senseIndex,
                ref synonymIndex,
                designations);
            return inflections;
        }


        public override List<Inflection> DeclineAdjectiveDictionaryFormDefault(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Designator> designations;
            if (IsNaAdjective(dictionaryEntry, senseIndex))
                designations = GetDefaultNounDesignations();
            else if (IsIAdjective(dictionaryEntry, senseIndex))
                designations = GetDefaultAdjectiveDesignations();
            else
                designations = GetDefaultNounDesignations();
            List<Inflection> inflections = DeclineAdjectiveDictionaryFormSelected(
                dictionaryEntry,
                ref senseIndex,
                ref synonymIndex,
                designations);
            return inflections;
        }

        public bool IsNaAdjective(DictionaryEntry dictionaryEntry, int senseIndex)
        {
            Sense sense;
            
            if ((senseIndex >= 0) && (senseIndex < dictionaryEntry.SenseCount))
                sense = dictionaryEntry.GetSenseIndexed(senseIndex);
            else
                sense = dictionaryEntry.GetCategorizedSense(EnglishID, LexicalCategory.Adjective);

            if (sense != null)
            {
                string categoryString = sense.CategoryString;
                string[] parts = categoryString.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);

                foreach (string part in parts)
                {
                    string cat = part.Trim();

                    switch (cat)
                    {
                        case "adj-i":
                            return false;
                        case "adj-na":
                            return true;
                        case "adj-no":
                        case "adj-pn":
                        case "adj-t":
                        case "adj-f":
                        case "adj":
                            return false;
                    }
                }
            }

            return false;
        }

        public bool IsIAdjective(DictionaryEntry dictionaryEntry, int senseIndex)
        {
            Sense sense;

            if ((senseIndex >= 0) && (senseIndex < dictionaryEntry.SenseCount))
                sense = dictionaryEntry.GetSenseIndexed(senseIndex);
            else
                sense = dictionaryEntry.GetCategorizedSense(EnglishID, LexicalCategory.Adjective);

            if (sense != null)
            {
                string categoryString = sense.CategoryString;
                string[] parts = categoryString.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);

                foreach (string part in parts)
                {
                    string cat = part.Trim();

                    switch (cat)
                    {
                        case "adj-i":
                            return true;
                        case "adj-na":
                            return false;
                        case "adj-no":
                        case "adj-pn":
                        case "adj-t":
                        case "adj-f":
                        case "adj":
                            return false;
                    }
                }
            }

            return false;
        }

        public override bool DeclineAdjectiveDictionaryFormDesignated(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            Designator designation,
            out Inflection inflection)
        {
            inflection = new Inflection(
                dictionaryEntry,
                designation,
                LocalInflectionLanguageIDs);

            if (dictionaryEntry == null)
            {
                inflection.Error = "Null dictionary entry in adjective declension.";
                return false;
            }

            if (designation == null)
            {
                inflection.Error = "Null designation in adjective declension.";
                return false;
            }

            Sense sense;
            int reading = 0;
            string categoryString;
            AdjectiveClass adjectiveClass = AdjectiveClass.Unknown;
            string kanjiDictionaryForm;
            string kanaDictionaryForm;
            string romajiDictionaryForm;
            string englishRoot;
            bool returnValue = true;

            if (dictionaryEntry == null)
            {
                inflection.Error = "Adjective not found in dictionary in adjective declension.";
                return false;
            }

            if ((senseIndex >= 0) && (senseIndex < dictionaryEntry.SenseCount))
                sense = dictionaryEntry.GetSenseIndexed(senseIndex);
            else
                sense = dictionaryEntry.GetCategorizedSense(EnglishID, LexicalCategory.Adjective);

            if (sense == null)
            {
                inflection.Error = "Input is not a known adjective in adjective declension.";
                return false;
            }

            reading = sense.Reading;
            categoryString = sense.CategoryString;
            adjectiveClass = GetAdjectiveClassFromCategoryStringList(categoryString);

            LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(LanguageLookup.English);

            if (languageSynonyms != null)
                englishRoot = languageSynonyms.GetSynonymIndexed(0);
            else
                englishRoot = "(unknown)";

            if (!GetJapaneseForms(
                    dictionaryEntry,
                    reading,
                    out kanjiDictionaryForm,
                    out kanaDictionaryForm,
                    out romajiDictionaryForm))
            {
                inflection.Error = "Dictionary doesn't have all transliterations for adjective declension.";
                return false;
            }

            if (adjectiveClass == AdjectiveClass.I)
            {
                returnValue = DeclineIAdjective(
                    kanjiDictionaryForm,
                    kanaDictionaryForm,
                    romajiDictionaryForm,
                    designation,
                    englishRoot,
                    inflection);
            }
            else
            {
                kanjiDictionaryForm += "な";
                kanaDictionaryForm += "な";
                romajiDictionaryForm += "na";

                returnValue = DeclineNaAdjective(
                    kanjiDictionaryForm,
                    kanaDictionaryForm,
                    romajiDictionaryForm,
                    designation,
                    englishRoot,
                    inflection);
            }

            return returnValue;
        }

        protected bool DeclineIAdjective(
            string kanjiDictionaryForm,
            string kanaDictionaryForm,
            string romajiDictionaryForm,
            Designator designation,
            string englishRoot,
            Inflection inflection)
        {
            string kanjiStem;
            string kanaStem;
            string romajiStem;
            string kanjiGStem = String.Empty;
            string kanaGStem = String.Empty;
            string romajiGStem = String.Empty;
            Special special = GetSpecial(designation);
            Mood mood = GetMood(designation);
            Tense tense = GetTense(designation);
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            Alternate alternate = GetAlternate(designation);
            bool returnValue = true;

            inflection.Category = LexicalCategory.Adjective;
            inflection.CategoryString = "adj-i";

            if (!GetIAdjectiveStemsFromDictionaryForm(
                    kanjiDictionaryForm,
                    kanaDictionaryForm,
                    romajiDictionaryForm,
                    out kanjiStem,
                    out kanaStem,
                    out romajiStem))
            {
                inflection.Error = "Input is not a known i adjective in adjective declension.";
                return false;
            }

            if (!GetIAdjectiveGStemsFromStems(
                    kanjiStem,
                    kanaStem,
                    romajiStem,
                    out kanjiGStem,
                    out kanaGStem,
                    out romajiGStem))
            {
                inflection.Error = "Input is not a known i adjective in adjective declension.";
                return false;
            }

            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "い", "i", englishRoot);

            switch (special)
            {
                case Special.Dictionary:
                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "い", "i", englishRoot);
                    break;
                case Special.Stem:
                    if (politeness == Politeness.Polite)
                        SetFullInflectedOutput(inflection, kanjiGStem, kanaGStem, romajiGStem, "", "", englishRoot);
                    else
                        SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "", "", englishRoot);
                    break;
                case Special.Adverbial:
                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "く", "ku", englishRoot + "ly");
                    break;
                case Special.Noun:
                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "さ", "sa", englishRoot + "ness");
                    break;
                case Special.Desire:
                case Special.Unknown:
                    switch (mood)
                    {
                        case Mood.Indicative:
                            switch (tense)
                            {
                                case Tense.Present:
                                    switch (politeness)
                                    {
                                        case Politeness.Plain:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "い", "i", "is " + englishRoot);
                                                    break;
                                                case Polarity.Negative:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くない", "kunai", "is not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
                                            break;
                                        case Politeness.Polite:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                    switch (alternate)
                                                    {
                                                        case Alternate.Unknown:
                                                        case Alternate.Desu:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "いです", "idesu", "is " + englishRoot);
                                                            break;
                                                        case Alternate.Gozaimasu:
                                                            SetFullInflectedOutput(inflection, kanjiGStem, kanaGStem, romajiGStem, "ございます", "gozaimasu", "is " + englishRoot);
                                                            break;
                                                        default:
                                                            inflection.Error = "Unexpected alternate: " + alternate.ToString();
                                                            returnValue = false;
                                                            break;
                                                    }
                                                    break;
                                                case Polarity.Negative:
                                                    switch (alternate)
                                                    {
                                                        case Alternate.Unknown:
                                                        case Alternate.Desu:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くないです", "kunaidesu", "is not " + englishRoot);
                                                            break;
                                                        case Alternate.Arimasu:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くありません", "kuarimasen", "is not " + englishRoot);
                                                            break;
                                                        case Alternate.Gozaimasu:
                                                            SetFullInflectedOutput(inflection, kanjiGStem, kanaGStem, romajiGStem, "ございません", "gozaimasen", "is not " + englishRoot);
                                                            break;
                                                        default:
                                                            inflection.Error = "Unexpected alternate: " + alternate.ToString();
                                                            returnValue = false;
                                                            break;
                                                    }
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
                                            break;
                                        default:
                                            inflection.Error = "Unexpected politeness: " + politeness.ToString();
                                            returnValue = false;
                                            break;
                                    }
                                    break;
                                case Tense.Past:
                                    switch (politeness)
                                    {
                                        case Politeness.Plain:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "かった", "katta", "was " + englishRoot);
                                                    break;
                                                case Polarity.Negative:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くなかった", "kunakatta", "was not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
                                            break;
                                        case Politeness.Polite:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                    switch (alternate)
                                                    {
                                                        case Alternate.Unknown:
                                                        case Alternate.Desu:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "かったです", "kattadesu", "was " + englishRoot);
                                                            break;
                                                        case Alternate.Arimasu:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くありました", "kuarimashita", "was " + englishRoot);
                                                            break;
                                                        case Alternate.Gozaimasu:
                                                            SetFullInflectedOutput(inflection, kanjiGStem, kanaGStem, romajiGStem, "ございました", "gozaimashita", "was " + englishRoot);
                                                            break;
                                                        default:
                                                            inflection.Error = "Unexpected alternate: " + alternate.ToString();
                                                            returnValue = false;
                                                            break;
                                                    }
                                                    break;
                                                case Polarity.Negative:
                                                    switch (alternate)
                                                    {
                                                        case Alternate.Unknown:
                                                        case Alternate.Desu:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くなかったです", "kunattadesu", "was not " + englishRoot);
                                                            break;
                                                        case Alternate.Arimasu:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くありませんでした", "kuarimasendeshita", "was not " + englishRoot);
                                                            break;
                                                        case Alternate.Gozaimasu:
                                                            SetFullInflectedOutput(inflection, kanjiGStem, kanaGStem, romajiGStem, "ございませんでした", "gozaimasendeshita", "was not " + englishRoot);
                                                            break;
                                                        default:
                                                            inflection.Error = "Unexpected alternate: " + alternate.ToString();
                                                            returnValue = false;
                                                            break;
                                                    }
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
                                            break;
                                        default:
                                            inflection.Error = "Unexpected politeness: " + politeness.ToString();
                                            returnValue = false;
                                            break;
                                    }
                                    break;
                                default:
                                    inflection.Error = "Unexpected tense: " + tense.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        case Mood.Presumptive:
                            switch (tense)
                            {
                                case Tense.Present:
                                    switch (politeness)
                                    {
                                        case Politeness.Plain:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "かろう", "karō", "probably is " + englishRoot);
                                                    break;
                                                case Polarity.Negative:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くあるまい", "kuarumai", "probably is not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
                                            break;
                                        case Politeness.Polite:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                    switch (alternate)
                                                    {
                                                        case Alternate.Unknown:
                                                        case Alternate.Arimasu:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くありましょう", "kuarimashō", "probably is " + englishRoot);
                                                            break;
                                                        case Alternate.Gozaimasu:
                                                            SetFullInflectedOutput(inflection, kanjiGStem, kanaGStem, romajiGStem, "ございましょう", "gozaimashō", "probably is " + englishRoot);
                                                            break;
                                                        default:
                                                            inflection.Error = "Unexpected alternate: " + alternate.ToString();
                                                            returnValue = false;
                                                            break;
                                                    }
                                                    break;
                                                case Polarity.Negative:
                                                    switch (alternate)
                                                    {
                                                        case Alternate.Unknown:
                                                        case Alternate.Arimasu:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くありますまい", "kuarimasumai", "probably is not " + englishRoot);
                                                            break;
                                                        case Alternate.Gozaimasu:
                                                            SetFullInflectedOutput(inflection, kanjiGStem, kanaGStem, romajiGStem, "ございますまい", "gozaimasumai", "probably is not " + englishRoot);
                                                            break;
                                                        default:
                                                            inflection.Error = "Unexpected alternate: " + alternate.ToString();
                                                            returnValue = false;
                                                            break;
                                                    }
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
                                            break;
                                        default:
                                            inflection.Error = "Unexpected politeness: " + politeness.ToString();
                                            returnValue = false;
                                            break;
                                    }
                                    break;
                                case Tense.Past:
                                    switch (politeness)
                                    {
                                        case Politeness.Plain:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "かったろう", "kattarō", "was probably " + englishRoot);
                                                    break;
                                                case Polarity.Negative:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くなかったろ", "kunakattarō", "was probably not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
                                            break;
                                        case Politeness.Polite:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                    SetFullInflectedOutput(inflection, kanjiGStem, kanaGStem, romajiGStem, "ございましたでしょう", "gozaimashitadeshō", "was probably " + englishRoot);
                                                    break;
                                                case Polarity.Negative:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くありませんでしたろう", "kuarimasendeshitarō", "was probably not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
                                            break;
                                        default:
                                            inflection.Error = "Unexpected politeness: " + politeness.ToString();
                                            returnValue = false;
                                            break;
                                    }
                                    break;
                                default:
                                    inflection.Error = "Unexpected tense: " + tense.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        case Mood.Probable:
                            switch (tense)
                            {
                                case Tense.Present:
                                    switch (politeness)
                                    {
                                        case Politeness.Plain:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "だろう", "darō", "probably is " + englishRoot);
                                                    break;
                                                case Polarity.Negative:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くないだろう", "kunaidarō", "probably is not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
                                            break;
                                        case Politeness.Polite:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でしょう", "deshō", "probably is " + englishRoot);
                                                    break;
                                                case Polarity.Negative:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くないでしょう", "kunaideshō", "probably is not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
                                            break;
                                        default:
                                            inflection.Error = "Unexpected politeness: " + politeness.ToString();
                                            returnValue = false;
                                            break;
                                    }
                                    break;
                                case Tense.Past:
                                    switch (politeness)
                                    {
                                        case Politeness.Plain:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "かっただろう", "kattadarō", "was probably " + englishRoot);
                                                    break;
                                                case Polarity.Negative:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くなかっただろう", "kunakattadarō", "was probably not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
                                            break;
                                        case Politeness.Polite:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "かったでしょう", "kattadeshō", "was probably " + englishRoot);
                                                    break;
                                                case Polarity.Negative:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くなかったでしょう", "kunakattadeshō", "was probably not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
                                            break;
                                        default:
                                            inflection.Error = "Unexpected politeness: " + politeness.ToString();
                                            returnValue = false;
                                            break;
                                    }
                                    break;
                                default:
                                    inflection.Error = "Unexpected tense: " + tense.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        case Mood.Provisional:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "ければ", "kereba", "if " + englishRoot);
                                    break;
                                case Polarity.Negative:
                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くなければ", "kunakereba", "if not " + englishRoot);
                                    break;
                                default:
                                    inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        case Mood.ContinuativeParticiple:
                            switch (politeness)
                            {
                                case Politeness.Plain:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くて", "kute", englishRoot + " and");
                                            break;
                                        case Polarity.Negative:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くなくて", "kunakute", "not " + englishRoot + " and");
                                            break;
                                        default:
                                            inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                            returnValue = false;
                                            break;
                                    }
                                    break;
                                case Politeness.Polite:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くありまして", "kuarimashite", englishRoot + " and");
                                            break;
                                        case Polarity.Negative:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くありませんで", "kuarimasende", "not " + englishRoot + " and");
                                            break;
                                        default:
                                            inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                            returnValue = false;
                                            break;
                                    }
                                    break;
                                default:
                                    inflection.Error = "Unexpected politeness: " + politeness.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        case Mood.Conditional:
                            switch (politeness)
                            {
                                case Politeness.Plain:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "かったら", "kattara", "when " + englishRoot);
                                            break;
                                        case Polarity.Negative:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くなかったら", "kunakattara", "when not " + englishRoot);
                                            break;
                                        default:
                                            inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                            returnValue = false;
                                            break;
                                    }
                                    break;
                                case Politeness.Polite:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                            SetFullInflectedOutput(inflection, kanjiGStem, kanaGStem, romajiGStem, "ございましたら", "gozaimashitara", "when " + englishRoot);
                                            break;
                                        case Polarity.Negative:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くありませんでしたら", "kuarimasendeshitara", "when not " + englishRoot);
                                            break;
                                        default:
                                            inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                            returnValue = false;
                                            break;
                                    }
                                    break;
                                default:
                                    inflection.Error = "Unexpected politeness: " + politeness.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        case Mood.Alternative:
                            switch (politeness)
                            {
                                case Politeness.Plain:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "かったり", "kattari", "like " + englishRoot + " and");
                                            break;
                                        case Polarity.Negative:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くなかったり", "kunakattari", "like not " + englishRoot + " and");
                                            break;
                                        default:
                                            inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                            returnValue = false;
                                            break;
                                    }
                                    break;
                                case Politeness.Polite:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くございましたり", "kugozaimashitari", "like " + englishRoot + " and");
                                            break;
                                        case Polarity.Negative:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "くありませんでしたり", "kuarimasendeshitari", "like not " + englishRoot + " and");
                                            break;
                                        default:
                                            inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                            returnValue = false;
                                            break;
                                    }
                                    break;
                                default:
                                    inflection.Error = "Unexpected politeness: " + politeness.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        default:
                            inflection.Error = "Unexpected mood: " + mood.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                default:
                    inflection.Error = "Unexpected special: " + special.ToString();
                    returnValue = false;
                    break;
            }

            return returnValue;
        }

        protected bool DeclineNaAdjective(
            string kanjiDictionaryForm,
            string kanaDictionaryForm,
            string romajiDictionaryForm,
            Designator designation,
            string englishRoot,
            Inflection inflection)
        {
            string kanjiStem;
            string kanaStem;
            string romajiStem;
            string kanjiGStem = String.Empty;
            string kanaGStem = String.Empty;
            string romajiGStem = String.Empty;
            Special special = GetSpecial(designation);
            Mood mood = GetMood(designation);
            Tense tense = GetTense(designation);
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            Alternate alternate = GetAlternate(designation);
            bool returnValue = true;

            if (!GetNaAdjectiveStemsFromDictionaryForm(
                    kanjiDictionaryForm,
                    kanaDictionaryForm,
                    romajiDictionaryForm,
                    out kanjiStem,
                    out kanaStem,
                    out romajiStem))
            {
                inflection.Error = "Input is not a known i adjective in adjective declension.";
                return false;
            }

            returnValue = DeclineCopula(
                kanjiStem,
                kanaStem,
                romajiStem,
                englishRoot,
                designation,
                inflection);

            inflection.Category = LexicalCategory.Adjective;
            inflection.CategoryString = "adj-na";

            return returnValue;
        }

        protected AdjectiveClass GetAdjectiveClassFromCategoryStringList(string categoryString)
        {
            string[] parts = categoryString.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);
            AdjectiveClass adjectiveClass = AdjectiveClass.Unknown;

            foreach (string part in parts)
            {
                string cat = part.Trim();
                adjectiveClass = GetAdjectiveClassFromCategoryString(cat);
                if (adjectiveClass != AdjectiveClass.Unknown)
                    break;
            }

            return adjectiveClass;
        }

        public bool GetIAdjectiveStemsFromDictionaryForm(
            string kanjiDictionaryForm,
            string kanaDictionaryForm,
            string romajiDictionaryForm,
            out string kanjiStem,
            out string kanaStem,
            out string romajiStem)
        {
            bool returnValue = true;
            if ((kanjiDictionaryForm.Length >= 1) && (kanjiDictionaryForm[kanjiDictionaryForm.Length - 1] == 'い'))
                kanjiStem = kanjiDictionaryForm.Substring(0, kanjiDictionaryForm.Length - 1);
            else
            {
                kanjiStem = kanjiDictionaryForm;
                returnValue = false;
            }
            if ((kanaDictionaryForm.Length >= 1) && (kanaDictionaryForm[kanaDictionaryForm.Length - 1] == 'い'))
                kanaStem = kanaDictionaryForm.Substring(0, kanaDictionaryForm.Length - 1);
            else
            {
                kanaStem = kanaDictionaryForm;
                returnValue = false;
            }
            if ((romajiDictionaryForm.Length >= 1) && (romajiDictionaryForm[romajiDictionaryForm.Length - 1] == 'i'))
                romajiStem = romajiDictionaryForm.Substring(0, romajiDictionaryForm.Length - 1);
            else if ((romajiDictionaryForm.Length >= 1) && (romajiDictionaryForm[romajiDictionaryForm.Length - 1] == 'ī'))
                romajiStem = romajiDictionaryForm.Substring(0, romajiDictionaryForm.Length - 1) + "i";
            else if ((romajiDictionaryForm.Length >= 1) && (romajiDictionaryForm[romajiDictionaryForm.Length - 1] == 'ē'))
                romajiStem = romajiDictionaryForm.Substring(0, romajiDictionaryForm.Length - 1) + "e";
            else
            {
                romajiStem = romajiDictionaryForm;
                returnValue = false;
            }
            return returnValue;
        }

        public bool GetIAdjectiveGStemsFromStems(
            string kanjiStem,
            string kanaStem,
            string romajiStem,
            out string kanjiGStem,
            out string kanaGStem,
            out string romajiGStem)
        {
            kanjiGStem = kanjiStem;
            kanaGStem = kanaStem;
            romajiGStem = romajiStem;
            if (kanaStem.Length == 0)
                return false;
            char lastChar = kanaStem[kanaStem.Length - 1];
            bool returnValue = true;
            switch (lastChar)
            {
                case 'あ':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "あ", "a", "おう", "ō");
                    break;
                case 'か':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "か", "a", "こう", "ō");
                    break;
                case 'さ':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "さ", "a", "そう", "ō");
                    break;
                case 'た':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "た", "a", "とう", "ō");
                    break;
                case 'な':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "な", "a", "のう", "ō");
                    break;
                case 'は':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "は", "a", "ほう", "ō");
                    break;
                case 'ま':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "ま", "a", "もう", "ō");
                    break;
                case 'や':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "や", "a", "よう", "ō");
                    break;
                case 'ら':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "ら", "a", "ろう", "ō");
                    break;
                case 'わ':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "わ", "a", "をう", "ō");
                    break;
                case 'い':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "い", "i", "ゆう", "ū");
                    break;
                case 'き':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "き", "i", "きゅう", "yū");
                    break;
                case 'し':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "し", "i", "しゅう", "ū");
                    break;
                case 'ち':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "ち", "i", "ちゅう", "yū");
                    break;
                case 'に':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "に", "i", "にゅう", "yū");
                    break;
                case 'ひ':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "ひ", "i", "ひゅう", "yū");
                    break;
                case 'み':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "み", "i", "みゅう", "yū");
                    break;
                case 'り':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "り", "i", "りゅう", "yū");
                    break;
                case 'う':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "う", "u", "うう", "ū");
                    break;
                case 'く':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "く", "u", "くう", "ū");
                    break;
                case 'す':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "す", "u", "すう", "ū");
                    break;
                case 'つ':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "つ", "u", "つう", "ū");
                    break;
                case 'ぬ':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "ぬ", "u", "ぬう", "ū");
                    break;
                case 'ふ':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "ふ", "u", "ふう", "ū");
                    break;
                case 'む':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "む", "u", "むう", "ū");
                    break;
                case 'ゆ':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "ゆ", "u", "ゆう", "ū");
                    break;
                case 'る':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "る", "u", "るう", "ū");
                    break;
                case 'お':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "お", "o", "おう", "ō");
                    break;
                case 'こ':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "こ", "o", "こう", "ō");
                    break;
                case 'そ':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "そ", "o", "そう", "ō");
                    break;
                case 'と':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "と", "o", "とう", "ō");
                    break;
                case 'の':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "の", "o", "のう", "ō");
                    break;
                case 'ほ':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "ほ", "o", "ほう", "ō");
                    break;
                case 'も':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "も", "o", "もう", "ō");
                    break;
                case 'よ':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "よ", "o", "よう", "ō");
                    break;
                case 'ろ':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "ろ", "o", "ろう", "ō");
                    break;
                case 'を':
                    returnValue = ReplaceKanaRomajiEnding(ref kanaGStem, ref romajiGStem, "を", "o", "をう", "ō");
                    break;
                default:
                    returnValue = false;
                    break;
            }
            kanjiGStem = kanaGStem;
            return returnValue;
        }

        public bool GetNaAdjectiveStemsFromDictionaryForm(
            string kanjiDictionaryForm,
            string kanaDictionaryForm,
            string romajiDictionaryForm,
            out string kanjiStem,
            out string kanaStem,
            out string romajiStem)
        {
            bool returnValue = true;
            if ((kanjiDictionaryForm.Length >= 1) && (kanjiDictionaryForm[kanjiDictionaryForm.Length - 1] == 'な'))
                kanjiStem = kanjiDictionaryForm.Substring(0, kanjiDictionaryForm.Length - 1);
            else
            {
                kanjiStem = kanjiDictionaryForm;
                returnValue = false;
            }
            if ((kanaDictionaryForm.Length >= 1) && (kanaDictionaryForm[kanaDictionaryForm.Length - 1] == 'な'))
                kanaStem = kanaDictionaryForm.Substring(0, kanaDictionaryForm.Length - 1);
            else
            {
                kanaStem = kanaDictionaryForm;
                returnValue = false;
            }
            if (romajiDictionaryForm.EndsWith("na"))
                romajiStem = romajiDictionaryForm.Substring(0, romajiDictionaryForm.Length - 2);
            else
            {
                romajiStem = romajiDictionaryForm;
                returnValue = false;
            }
            return returnValue;
        }

        protected bool ReplaceKanaRomajiEnding(
            ref string kana,
            ref string romaji,
            string kanaOldEnding,
            string romajiOldEnding,
            string kanaNewEnding,
            string romajiNewEnding)
        {
            bool returnValue = true;
            if (kana.EndsWith(kanaOldEnding))
                kana = kana.Substring(0, kana.Length - kanaOldEnding.Length) + kanaNewEnding;
            else
                returnValue = false;
            if (romaji.EndsWith(romajiOldEnding))
                romaji = romaji.Substring(0, romaji.Length - romajiOldEnding.Length) + romajiNewEnding;
            else
                returnValue = false;
            return returnValue;
        }
    }
}
