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
        public override bool DeclineNounDictionaryFormDesignated(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            Designator designation,
            out Inflection inflection)
        {
            bool returnValue = true;

            inflection = new Inflection(
                dictionaryEntry,
                designation,
                LocalInflectionLanguageIDs);

            if (dictionaryEntry == null)
            {
                inflection.Category = LexicalCategory.Verb;
                inflection.CategoryString = "desu";
                returnValue = DeclineCopula(
                    String.Empty,
                    String.Empty,
                    String.Empty,
                    String.Empty,
                    designation,
                    inflection);
                return returnValue;
            }

            if (designation == null)
            {
                inflection.Error = "Null designation in noun declension.";
                return false;
            }

            Sense sense;
            int reading = 0;
            string categoryString;
            string kanjiDictionaryForm;
            string kanaDictionaryForm;
            string romajiDictionaryForm;
            string englishRoot;

            if (dictionaryEntry == null)
            {
                inflection.Error = "Noun not found in dictionary in noun declension.";
                return false;
            }

            if ((senseIndex >= 0) && (senseIndex < dictionaryEntry.SenseCount))
                sense = dictionaryEntry.GetSenseIndexed(senseIndex);
            else
                sense = dictionaryEntry.GetCategorizedSense(EnglishID, LexicalCategory.Noun);

            if (sense == null)
            {
                inflection.Error = "Input is not a known noun in noun declension.";
                return false;
            }

            reading = sense.Reading;
            categoryString = sense.CategoryString;

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
                inflection.Error = "Dictionary doesn't have all transliterations for noun declension.";
                return false;
            }

            returnValue = DeclineCopula(
                kanjiDictionaryForm,
                kanaDictionaryForm,
                romajiDictionaryForm,
                englishRoot,
                designation,
                inflection);

            return returnValue;
        }

        protected bool DeclineCopula(
            string kanjiStem,
            string kanaStem,
            string romajiStem,
            string englishRoot,
            Designator designation,
            Inflection inflection)
        {
            Special special = GetSpecial(designation);
            Mood mood = GetMood(designation);
            Tense tense = GetTense(designation);
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            Alternate alternate = GetAlternate(designation);
            Contraction contraction = GetContraction(designation);
            bool returnValue = true;

            if (!String.IsNullOrEmpty(romajiStem))
                romajiStem += " ";

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
                                            switch (alternate)
                                            {
                                                case Alternate.Unknown:
                                                case Alternate.Desu:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "だ", "da", "is " + englishRoot);
                                                    break;
                                                case Alternate.Arimasu:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "である", "de aru", "is " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected alternate: " + alternate.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
                                            break;
                                        case Polarity.Negative:
                                            switch (contraction)
                                            {
                                                case Contraction.Unknown:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわない", "dewa nai", "is not " + englishRoot);
                                                    break;
                                                case Contraction.ContractionDeWaToJa:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃない", "ja nai", "is not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected contraction: " + contraction.ToString();
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
                                case Politeness.Polite:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                            switch (alternate)
                                            {
                                                case Alternate.Unknown:
                                                case Alternate.Desu:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "です", "desu", "is " + englishRoot);
                                                    break;
                                                case Alternate.Arimasu:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "であります", "de arimasu", "is " + englishRoot);
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
                                                    switch (contraction)
                                                    {
                                                        case Contraction.Unknown:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわありません", "dewa arimasen", "is not " + englishRoot);
                                                            break;
                                                        case Contraction.ContractionDeWaToJa:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃありません", "ja arimasen", "is not " + englishRoot);
                                                            break;
                                                        default:
                                                            inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                            returnValue = false;
                                                            break;
                                                    }
                                                    break;
                                                case Alternate.Nai:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃないです", "ja nai desu", "is not " + englishRoot);
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
                                case Politeness.Honorific:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でございます", "de gozaimasu", "is " + englishRoot);
                                            break;
                                        case Polarity.Negative:
                                            switch (contraction)
                                            {
                                                case Contraction.Unknown:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわございません", "dewa gozaimasen", "is not " + englishRoot);
                                                    break;
                                                case Contraction.ContractionDeWaToJa:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃございません", "ja gozaimasen", "is not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected contraction: " + contraction.ToString();
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
                                            switch (alternate)
                                            {
                                                case Alternate.Unknown:
                                                case Alternate.Desu:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "だった", "datta", "was " + englishRoot);
                                                    break;
                                                case Alternate.Arimasu:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "であった", "de atta", "was " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected alternate: " + alternate.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
                                            break;
                                        case Polarity.Negative:
                                            switch (contraction)
                                            {
                                                case Contraction.Unknown:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわなかった", "dewa nakatta", "was not " + englishRoot);
                                                    break;
                                                case Contraction.ContractionDeWaToJa:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃなかった", "ja nakatta", "was not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected contraction: " + contraction.ToString();
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
                                case Politeness.Polite:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                            switch (alternate)
                                            {
                                                case Alternate.Unknown:
                                                case Alternate.Desu:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でした", "deshita", "was " + englishRoot);
                                                    break;
                                                case Alternate.Arimasu:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でありました", "de arimashita", "was " + englishRoot);
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
                                                case Alternate.Arimasu:
                                                    switch (contraction)
                                                    {
                                                        case Contraction.Unknown:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわありませんでした", "dewa arimasen deshita", "was not " + englishRoot);
                                                            break;
                                                        case Contraction.ContractionDeWaToJa:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃありませんでした", "ja arimasen deshita", "was not " + englishRoot);
                                                            break;
                                                        default:
                                                            inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                            returnValue = false;
                                                            break;
                                                    }
                                                    break;
                                                case Alternate.Nak:
                                                    switch (contraction)
                                                    {
                                                        case Contraction.ContractionDeWaToJa:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃなかったです", "ja nakatta desu", "was not " + englishRoot);
                                                            break;
                                                        default:
                                                            inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                            returnValue = false;
                                                            break;
                                                    }
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
                                case Politeness.Honorific:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でございました", "de gozaimashita", englishRoot);
                                            break;
                                        case Polarity.Negative:
                                            switch (contraction)
                                            {
                                                case Contraction.Unknown:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわございませんでした", "dewa gozaimasen deshita", "not " + englishRoot);
                                                    break;
                                                case Contraction.ContractionDeWaToJa:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃございませんでした", "ja gozaimasen deshita", "not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected contraction: " + contraction.ToString();
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
                case Mood.Continuative:
                    switch (politeness)
                    {
                        case Politeness.Plain:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    switch (alternate)
                                    {
                                        case Alternate.Unknown:
                                        case Alternate.Desu:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "で", "de", englishRoot + " and");
                                            break;
                                        case Alternate.Arimasu:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "であり", "de ari", englishRoot + " and");
                                            break;
                                        default:
                                            inflection.Error = "Unexpected alternate: " + alternate.ToString();
                                            returnValue = false;
                                            break;
                                    }
                                    break;
                                case Polarity.Negative:
                                    switch (contraction)
                                    {
                                        case Contraction.Unknown:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわなくて", "dewa nakute", "not " + englishRoot + " and");
                                            break;
                                        case Contraction.ContractionDeWaToJa:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃなくて", "ja nakute", "not " + englishRoot + " and");
                                            break;
                                        default:
                                            inflection.Error = "Unexpected contraction: " + contraction.ToString();
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
                        case Politeness.Polite:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    switch (alternate)
                                    {
                                        case Alternate.Unknown:
                                        case Alternate.Desu:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でして", "deshite", englishRoot + " and");
                                            break;
                                        case Alternate.Arimasu:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でありまして", "de arimashite", englishRoot + " and");
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
                                            switch (contraction)
                                            {
                                                case Contraction.Unknown:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわありませんでして", "dewa arimasen deshite", "not " + englishRoot + " and");
                                                    break;
                                                case Contraction.ContractionDeWaToJa:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃありませんでして", "ja arimasen deshite", "not " + englishRoot + " and");
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
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
                        case Politeness.Honorific:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でございまして", "de gozaimashite", englishRoot + " and");
                                    break;
                                case Polarity.Negative:
                                    switch (contraction)
                                    {
                                        case Contraction.Unknown:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわございませんでして", "dewa gozaimasen deshite", "not " + englishRoot + " and");
                                            break;
                                        case Contraction.ContractionDeWaToJa:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃございませんでして", "ja gozaimasen deshite", "not " + englishRoot + " and");
                                            break;
                                        default:
                                            inflection.Error = "Unexpected contraction: " + contraction.ToString();
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
                case Mood.Prenomial:
                    if (!String.IsNullOrEmpty(kanjiStem))
                    {
                        switch (polarity)
                        {
                            case Polarity.Positive:
                                SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "な", "na", englishRoot + " (noun)");
                                break;
                            case Polarity.Negative:
                                switch (contraction)
                                {
                                    case Contraction.Unknown:
                                        SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわない", "dewa nai", englishRoot + " (noun)");
                                        break;
                                    case Contraction.ContractionDeWaToJa:
                                        SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃない", "ja nai", englishRoot + " (noun)");
                                        break;
                                    default:
                                        inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                        returnValue = false;
                                        break;
                                }
                                break;
                            default:
                                inflection.Error = "Unexpected polarity: " + polarity.ToString();
                                returnValue = false;
                                break;
                        }
                    }
                    else
                    {
                        inflection.Error = "Prenomial inflection not supported for the lone copula.";
                        returnValue = false;
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
                                            switch (alternate)
                                            {
                                                case Alternate.Unknown:
                                                case Alternate.Desu:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "だろう", "darō", "probably is " + englishRoot);
                                                    break;
                                                case Alternate.Arimasu:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "であろう", "de arō", "probably is " + englishRoot);
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
                                                case Alternate.Nak:
                                                    switch (contraction)
                                                    {
                                                        case Contraction.Unknown:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわなかろう", "dewa nakarō", "probably is not " + englishRoot);
                                                            break;
                                                        case Contraction.ContractionDeWaToJa:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃなかろう", "ja nakarō", "probably is not " + englishRoot);
                                                            break;
                                                        default:
                                                            inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                            returnValue = false;
                                                            break;
                                                    }
                                                    break;
                                                case Alternate.Nai:
                                                    switch (contraction)
                                                    {
                                                        case Contraction.Unknown:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわないだろう", "dewa nai darō", "probably is not " + englishRoot);
                                                            break;
                                                        case Contraction.ContractionDeWaToJa:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃないだろう", "ja nai darō", "probably is not " + englishRoot);
                                                            break;
                                                        default:
                                                            inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                            returnValue = false;
                                                            break;
                                                    }
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
                                case Politeness.Polite:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                            switch (alternate)
                                            {
                                                case Alternate.Unknown:
                                                case Alternate.Desu:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でしょう", "deshō", "probably is " + englishRoot);
                                                    break;
                                                case Alternate.Arimasu:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でありましょう", "de arimashō", "probably is " + englishRoot);
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
                                                case Alternate.Nai:
                                                    switch (contraction)
                                                    {
                                                        case Contraction.Unknown:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわないでしょう", "dewa nai deshō", "probably is not " + englishRoot);
                                                            break;
                                                        case Contraction.ContractionDeWaToJa:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃないでしょう", "ja nai deshō", "probably is not " + englishRoot);
                                                            break;
                                                        default:
                                                            inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                            returnValue = false;
                                                            break;
                                                    }
                                                    break;
                                                case Alternate.Arimasu:
                                                    switch (contraction)
                                                    {
                                                        case Contraction.Unknown:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわありませんでしょう", "dewa arimasen deshō", "probably is not " + englishRoot);
                                                            break;
                                                        case Contraction.ContractionDeWaToJa:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃありませんでしょう", "ja arimasen deshō", "probably is not " + englishRoot);
                                                            break;
                                                        default:
                                                            inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                            returnValue = false;
                                                            break;
                                                    }
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
                                case Politeness.Honorific:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でございましょう", "de gozaimashō", "probably is " + englishRoot);
                                            break;
                                        case Polarity.Negative:
                                            switch (contraction)
                                            {
                                                case Contraction.Unknown:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわございませんでしょう", "dewa gozaimasen deshō", "probably is not " + englishRoot);
                                                    break;
                                                case Contraction.ContractionDeWaToJa:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃございませんでしょう", "ja gozaimasen deshō", "probably is not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected contraction: " + contraction.ToString();
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
                                            switch (alternate)
                                            {
                                                case Alternate.Unknown:
                                                case Alternate.Desu:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "だったろう", "dattarō", "probably was " + englishRoot);
                                                    break;
                                                case Alternate.Arimasu:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "であったろう", "de attarō", "probably was " + englishRoot);
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
                                                    switch (contraction)
                                                    {
                                                        case Contraction.Unknown:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわなかったろう", "dewa nakattarō", "probably was not " + englishRoot);
                                                            break;
                                                        case Contraction.ContractionDeWaToJa:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃなかったろう", "ja nakattarō", "probably was not " + englishRoot);
                                                            break;
                                                        default:
                                                            inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                            returnValue = false;
                                                            break;
                                                    }
                                                    break;
                                                case Alternate.Desu:
                                                    switch (contraction)
                                                    {
                                                        case Contraction.Unknown:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわなかっただろう", "dewa nakatta darō", "probably was not " + englishRoot);
                                                            break;
                                                        case Contraction.ContractionDeWaToJa:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃなかっただろう", "ja nakatta darō", "probably was not " + englishRoot);
                                                            break;
                                                        default:
                                                            inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                            returnValue = false;
                                                            break;
                                                    }
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
                                case Politeness.Polite:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                            switch (alternate)
                                            {
                                                case Alternate.Unknown:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "だったでしょう", "datta deshō", "probably was " + englishRoot);
                                                    break;
                                                case Alternate.Desu:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でしたろう", "deshitarō", "probably was " + englishRoot);
                                                    break;
                                                case Alternate.Arimasu:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でありましたろう", "de arimashitarō", "probably was " + englishRoot);
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
                                                case Alternate.Nak:
                                                    switch (contraction)
                                                    {
                                                        case Contraction.Unknown:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわなかったでしょう", "dewa nakatta deshō", "probably was not " + englishRoot);
                                                            break;
                                                        case Contraction.ContractionDeWaToJa:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃなかったでしょう", "ja nakatta deshō", "probably was not " + englishRoot);
                                                            break;
                                                        default:
                                                            inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                            returnValue = false;
                                                            break;
                                                    }
                                                    break;
                                                case Alternate.Arimasu:
                                                    switch (contraction)
                                                    {
                                                        case Contraction.Unknown:
                                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわありませんでしたろう", "dewa arimasen deshitarō", "probably was not " + englishRoot);
                                                            break;
                                                        default:
                                                            inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                            returnValue = false;
                                                            break;
                                                    }
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
                                case Politeness.Honorific:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわございましたでしょう", "de gozaimashita deshō", "probably was " + englishRoot);
                                            break;
                                        case Polarity.Negative:
                                            switch (contraction)
                                            {
                                                case Contraction.Unknown:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわございませんでしたでしょう", "dewa gozaimasen deshita deshō", "probably was not " + englishRoot);
                                                    break;
                                                case Contraction.ContractionDeWaToJa:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃございませんでしたでしょう", "ja gozaimasen deshita deshō", "probably was not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected contraction: " + contraction.ToString();
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
                case Mood.Provisional:
                    switch (politeness)
                    {
                        case Politeness.Plain:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    switch (alternate)
                                    {
                                        case Alternate.Unknown:
                                        case Alternate.Alternate1:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "なら", "nara", "if " + englishRoot);
                                            break;
                                        case Alternate.Alternate2:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "ならば", "naraba", "if " + englishRoot);
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
                                            switch (contraction)
                                            {
                                                case Contraction.Unknown:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわなければ", "dewa nakereba", "if not " + englishRoot);
                                                    break;
                                                case Contraction.ContractionDeWaToDe:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でなければ", "de nakereba", "if not " + englishRoot);
                                                    break;
                                                case Contraction.ContractionDeWaToJa:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃなければ", "ja nakereba", "if not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
                                            break;
                                        case Alternate.Alternate1:
                                            switch (contraction)
                                            {
                                                case Contraction.ContractionDeWaToJa:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃなけりゃ", "ja nakerya", "if not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
                                            break;
                                        case Alternate.Alternate2:
                                            switch (contraction)
                                            {
                                                case Contraction.ContractionDeWaToJa:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃなけりゃあ", "ja nakeryā", "if not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
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
                        case Politeness.Polite:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "だと", "da to", "if " + englishRoot);
                                    break;
                                case Polarity.Negative:
                                    switch (contraction)
                                    {
                                        case Contraction.ContractionDeWaToJa:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃないと", "ja nai to", "if not " + englishRoot);
                                            break;
                                        default:
                                            inflection.Error = "Unexpected contraction: " + contraction.ToString();
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
                case Mood.Conditional:
                    switch (politeness)
                    {
                        case Politeness.Plain:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    switch (alternate)
                                    {
                                        case Alternate.Unknown:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "だったら", "dattara", "when " + englishRoot);
                                            break;
                                        case Alternate.Arimasu:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "であったら", "de attara", "when " + englishRoot);
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
                                            switch (contraction)
                                            {
                                                case Contraction.Unknown:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわなかったら", "dewa nakattara", "if not " + englishRoot);
                                                    break;
                                                case Contraction.ContractionDeWaToDe:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でなかったら", "de nakattara", "if not " + englishRoot);
                                                    break;
                                                case Contraction.ContractionDeWaToJa:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃなかったら", "ja nakattara", "if not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
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
                        case Politeness.Polite:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    switch (alternate)
                                    {
                                        case Alternate.Unknown:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でしたら", "deshitara", "when " + englishRoot);
                                            break;
                                        case Alternate.Arimasu:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でありましたら", "de arimashitara", "when " + englishRoot);
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
                                            switch (contraction)
                                            {
                                                case Contraction.Unknown:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわありませんでしたら", "dewa arimasen deshitara", "when not " + englishRoot);
                                                    break;
                                                case Contraction.ContractionDeWaToDe:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でありませんでしたら", "de arimasen deshitara", "when not " + englishRoot);
                                                    break;
                                                case Contraction.ContractionDeWaToJa:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃありませんでしたら", "ja arimasen deshitara", "when not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
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
                        case Politeness.Honorific:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でございましたら", "de gozaimashitara", "when " + englishRoot);
                                    break;
                                case Polarity.Negative:
                                    switch (alternate)
                                    {
                                        case Alternate.Unknown:
                                            switch (contraction)
                                            {
                                                case Contraction.Unknown:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわございませんでしたら", "dewa gozaimasen deshitara", "when not " + englishRoot);
                                                    break;
                                                case Contraction.ContractionDeWaToDe:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でございませんでしたら", "de gozaimasen deshitara", "when not " + englishRoot);
                                                    break;
                                                case Contraction.ContractionDeWaToJa:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃございませんでしたら", "ja gozaimasen deshitara", "when not " + englishRoot);
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
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
                case Mood.Alternative:
                    switch (politeness)
                    {
                        case Politeness.Plain:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    switch (alternate)
                                    {
                                        case Alternate.Unknown:
                                        case Alternate.Desu:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "だったり", "dattari", "like " + englishRoot + " and");
                                            break;
                                        case Alternate.Arimasu:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "であったり", "de attari", "like " + englishRoot + " and");
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
                                            switch (contraction)
                                            {
                                                case Contraction.Unknown:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でわなかったり", "dewa nakattari", "like " + englishRoot + " and");
                                                    break;
                                                case Contraction.ContractionDeWaToDe:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でなかったり", "de nakattari", "like " + englishRoot + " and");
                                                    break;
                                                case Contraction.ContractionDeWaToJa:
                                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "じゃなかったり", "ja nakattari", "like " + englishRoot + " and");
                                                    break;
                                                default:
                                                    inflection.Error = "Unexpected contraction: " + contraction.ToString();
                                                    returnValue = false;
                                                    break;
                                            }
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
                        case Politeness.Polite:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    switch (alternate)
                                    {
                                        case Alternate.Unknown:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でしたり", "deshitari", "like " + englishRoot + " and");
                                            break;
                                        case Alternate.Arimasu:
                                            SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でありましたり", "de arimashitari", "like " + englishRoot + " and");
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
                        case Politeness.Honorific:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    SetFullInflectedOutput(inflection, kanjiStem, kanaStem, romajiStem, "でございましたり", "de gozaimashitari", "like " + englishRoot + " and");
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

            return returnValue;
        }
    }
}
