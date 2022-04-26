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
        public override bool ConjugateVerbDictionaryFormDesignated(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            Designator designation,
            out Inflection inflection)
        {
            VerbClass verbClass = VerbClass.Unknown;
            string categoryString = String.Empty;
            JapaneseStems japaneseStems = new JapaneseStems();
            EnglishStems englishStems = new EnglishStems();
            MultiLanguageString output = new MultiLanguageString(null, LocalInflectionLanguageIDs);
            Special special = GetSpecial(designation);
            string error = String.Empty;
            bool returnValue = false;

            inflection = new Inflection(
                dictionaryEntry,
                designation,
                LocalInflectionLanguageIDs);

            if (GetVerbStems(
                    dictionaryEntry,
                    senseIndex,
                    synonymIndex,
                    out verbClass,
                    out categoryString,
                    japaneseStems,
                    englishStems,
                    inflection.Root,
                    inflection.DictionaryForm,
                    out error))
            {
                inflection.Category = LexicalCategory.Verb;
                inflection.CategoryString = categoryString;
                returnValue = ConjugateRecurse(
                    designation,
                    japaneseStems,
                    englishStems,
                    verbClass,
                    inflection);

                switch (verbClass)
                {
                    case VerbClass.Ichidan:
                    case VerbClass.Godan:
                    case VerbClass.Yondan:
                        break;
                    case VerbClass.Suru:
                        if (japaneseStems.DictionaryFormKana != "する")
                            FixupVerbSuru(inflection);
                        break;
                    case VerbClass.Kuru:
                        if (japaneseStems.DictionaryFormKana != "くる")
                            FixupVerbKuru(inflection);
                        break;
                    case VerbClass.Kureru:
                    case VerbClass.Aru:
                    case VerbClass.Iru:
                        break;
                    default:
                        break;
                }
            }

            FixupInflection(inflection);

            return returnValue;
        }

        protected bool ConjugateSpecialRecursive(Designator designation,
            VerbClass verbClass, EnglishStems englishStems,
            Inflection inflected, Inflection newInflected)
        {
            MultiLanguageString dictionaryForm = newInflected.DictionaryForm;
            MultiLanguageString output = inflected.Output;
            Designator newDesignation = new Designator(designation);
            string kanji = dictionaryForm.Text(JapaneseID);
            string kana = dictionaryForm.Text(JapaneseKanaID);
            string romaji = dictionaryForm.Text(JapaneseRomajiID);
            string stem = kanji.Substring(0, kanji.Length - 1);
            string kanaStem = kana.Substring(0, kana.Length - 1);
            string romajiStem = String.Empty;
            JapaneseStems newJapaneseStems = new JapaneseStems(kanji, kana, romaji, stem, kanaStem, romajiStem);
            EnglishStems newEnglishStems = new EnglishStems(englishStems);
            MultiLanguageString newRoot = newInflected.Root;
            string error = inflected.Error;
            bool returnValue = true;

            newDesignation.DeleteFirstClassification("Special");

            if (!newDesignation.HasClassification("Mood"))
                newDesignation.InsertFirstClassification("Mood", "Recursive");

            if (!newDesignation.HasClassification("Tense"))
                newDesignation.InsertFirstClassification("Tense", "Present");

            returnValue = GetJapaneseVerbStems(
                kanji,
                verbClass,
                newJapaneseStems,
                newRoot,
                ref error);

            inflected.Error = error;

            returnValue = ConjugateRecurse(
                newDesignation,
                newJapaneseStems,
                newEnglishStems,
                verbClass,
                newInflected) && returnValue;

            output.CopyText(newInflected.Output);
            inflected.Prefix.CopyText(newInflected.Prefix);
            FixupSuffix(inflected);

            return returnValue;
        }

        protected bool ConjugateRecurse(
            Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems,
            VerbClass verbClass,
            Inflection inflected)
        {
            Special special = GetSpecial(designation);
            bool returnValue = false;

            if (special == Special.Unknown)
            {
                Mood mood = GetMood(designation);
                Tense tense = GetTense(designation);

                switch (mood)
                {
                    case Mood.Indicative:
                        switch (tense)
                        {
                            case Tense.Present:
                                returnValue = ConjugatePresentIndicative(designation,
                                    japaneseStems, englishStems, verbClass, inflected);
                                break;
                            case Tense.Past:
                                returnValue = ConjugatePastIndicative(designation,
                                    japaneseStems, englishStems, verbClass, inflected);
                                break;
                            default:
                                inflected.Error = "Unexpected tense: " + tense.ToString();
                                break;
                        }
                        break;
                    case Mood.Recursive:
                        switch (tense)
                        {
                            case Tense.Present:
                                returnValue = ConjugatePresentRecursive(designation,
                                    japaneseStems, englishStems, verbClass, inflected);
                                break;
                            case Tense.Past:
                                returnValue = ConjugatePastRecursive(designation,
                                    japaneseStems, englishStems, verbClass, inflected);
                                break;
                            default:
                                inflected.Error = "Unexpected tense: " + tense.ToString();
                                break;
                        }
                        break;
                    case Mood.Volitional:
                        returnValue = ConjugateVolitional(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Mood.Presumptive:
                        switch (tense)
                        {
                            case Tense.Present:
                                returnValue = ConjugatePresumptive(designation,
                                    japaneseStems, englishStems, verbClass, inflected);
                                break;
                            case Tense.Past:
                                returnValue = ConjugatePastPresumptive(designation,
                                    japaneseStems, englishStems, verbClass, inflected);
                                break;
                            default:
                                inflected.Error = "Unexpected tense: " + tense.ToString();
                                break;
                        }
                        break;
                    case Mood.ContinuativeParticiple:
                        returnValue = ConjugateContinuativeParticiple(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Mood.ContinuativeInfinitive:
                        returnValue = ConjugateContinuativeInfinitive(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Mood.Progressive:
                        returnValue = ConjugateProgressive(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Mood.Unintentional:
                        returnValue = ConjugateUnintentional(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Mood.Probative:
                        returnValue = ConjugateProbative(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Mood.Imperative:
                        returnValue = ConjugateImperative(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Mood.Request:
                        returnValue = ConjugateRequest(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Mood.Provisional:
                        returnValue = ConjugateProvisional(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Mood.Conditional:
                        returnValue = ConjugateConditional(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Mood.Alternative:
                        returnValue = ConjugateAlternative(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    default:
                        inflected.Error = "Unexpected mood: " + mood.ToString();
                        break;
                }
            }
            else
            {
                switch (special)
                {
                    case Special.Dictionary:
                        returnValue = ConjugateDictionary(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Special.Stem:
                        returnValue = ConjugateStem(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Special.Participle:
                        returnValue = ConjugateParticiple(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Special.Perfective:
                        returnValue = ConjugatePerfective(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Special.Infinitive:
                        returnValue = ConjugateInfinitive(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Special.Potential:
                        returnValue = ConjugatePotential(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Special.Passive:
                        returnValue = ConjugatePassive(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Special.Causitive:
                        returnValue = ConjugateCausitive(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Special.Desire:
                        returnValue = ConjugateDesire(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Special.Honorific1:
                    case Special.Honorific2:
                        returnValue = ConjugateHonorific(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    case Special.Humble1:
                    case Special.Humble2:
                        returnValue = ConjugateHumble(designation,
                            japaneseStems, englishStems, verbClass, inflected);
                        break;
                    default:
                        inflected.Error = "Unexpected special: " + special.ToString();
                        break;
                }
            }

            return returnValue;
        }

        protected bool ConjugateRecurseSecondary(
            DictionaryEntry dictionaryEntry,
            Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems,
            VerbClass verbClass,
            Inflection inflected)
        {
            bool returnValue = ConjugateRecurse(
                designation,
                japaneseStems,
                englishStems,
                verbClass,
                inflected);

            RemoveOPrefix(inflected);

            return returnValue;
        }

        protected bool ConjugatePresentIndicative(
            Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems,
            VerbClass verbClass,
            Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string aStem = japaneseStems.AStem;
            string eStem = japaneseStems.EStem;
            string iStem = japaneseStems.IStem;
            string oStem = japaneseStems.OStem;
            string uStem = japaneseStems.UStem;
            string taStem = japaneseStems.TAStem;
            string teStem = japaneseStems.TEStem;
            string ending = string.Empty;
            string englishInflected = englishStems.Root;
            bool returnValue = true;

            switch (politeness)
            {
                case Politeness.Plain:
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                case VerbClass.Suru:
                                    break;
                                case VerbClass.Kuru:
                                    break;
                                case VerbClass.Kureru:
                                case VerbClass.Aru:
                                case VerbClass.Iru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            switch (GetContraction(designation))
                            {
                                case Contraction.ContractionRuNoToNNo:
                                    if (uStem == "る")
                                        ending = "んの";
                                    else
                                    {
                                        ending = uStem;
                                        returnValue = false;
                                    }
                                    break;
                                default:
                                    ending = uStem;
                                    break;
                            }
                            englishInflected = ProcessToken(englishInflected, "%n", null, englishInflected);
                            break;
                        case Polarity.Negative:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKo(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Aru:
                                    stem = stem.Substring(0, stem.Length - 1);
                                    kanaStem = kanaStem.Substring(0, kanaStem.Length - 1);
                                    romajiStem = romajiStem.Substring(0, romajiStem.Length - 1);
                                    aStem = aStem.Substring(0, aStem.Length - 1);
                                    RemoveLastCharacter(inflected.Root);
                                    break;
                                case VerbClass.Iru:
                                case VerbClass.Kureru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            switch (GetContraction(designation))
                            {
                                case Contraction.ContractionNaiToNee:
                                    ending = aStem + "ねえ";
                                    if ((verbClass == VerbClass.Aru) || (verbClass == VerbClass.Iru))
                                        returnValue = false;
                                    break;
                                case Contraction.ContractionNaiToN:
                                    ending = aStem + "ん";
                                    if ((verbClass == VerbClass.Aru) || (verbClass == VerbClass.Iru))
                                        returnValue = false;
                                    break;
                                case Contraction.ContractionRaNaiToNNai:
                                    if (uStem == "る")
                                        ending = "んない";
                                    else
                                    {
                                        ending = aStem + "ない";
                                        returnValue = false;
                                    }
                                    if ((verbClass == VerbClass.Aru) || (verbClass == VerbClass.Iru))
                                        returnValue = false;
                                    break;
                                default:
                                    ending = aStem + "ない";
                                    break;
                            }
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken(englishInflected, "%n", "not", "not " + englishInflected);
                            else
                                englishInflected = ProcessToken(englishInflected, "%n", "not", englishInflected + " not");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case Politeness.Polite:
                    switch (verbClass)
                    {
                        case VerbClass.Ichidan:
                        case VerbClass.Godan:
                        case VerbClass.Yondan:
                            break;
                        case VerbClass.Suru:
                            ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kuru:
                            ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kureru:
                            break;
                        case VerbClass.Aru:
                        case VerbClass.Iru:
                            break;
                        default:
                            inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                            returnValue = false;
                            break;
                    }
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            ending = iStem + "ます";
                            englishInflected = ProcessToken(englishInflected, "%n", null, englishInflected);
                            break;
                        case Polarity.Negative:
                            ending = iStem + "ません";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken(englishInflected, "%n", "not", "not " + englishInflected);
                            else
                                englishInflected = ProcessToken(englishInflected, "%n", "not", englishInflected + " not");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                default:
                    inflected.Error = "Unexpected politeness:" + politeness.ToString();
                    returnValue = false;
                    break;
            }

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            return returnValue;
        }

        protected bool ConjugatePastIndicative(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string aStem = japaneseStems.AStem;
            string eStem = japaneseStems.EStem;
            string iStem = japaneseStems.IStem;
            string oStem = japaneseStems.OStem;
            string uStem = japaneseStems.UStem;
            string taStem = japaneseStems.TAStem;
            string teStem = japaneseStems.TEStem;
            string ending = string.Empty;
            string englishInflected = englishStems.Preterite;
            bool returnValue = true;

            switch (politeness)
            {
                case Politeness.Plain:
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                case VerbClass.Iru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            ending = taStem;
                            englishInflected = ProcessToken(englishInflected, "%n", null, englishInflected);
                            break;
                        case Polarity.Negative:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKo(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                    stem = stem.Substring(0, stem.Length - 1);
                                    kanaStem = kanaStem.Substring(0, kanaStem.Length - 1);
                                    romajiStem = romajiStem.Substring(0, romajiStem.Length - 1);
                                    aStem = aStem.Substring(0, aStem.Length - 1);
                                    RemoveLastCharacter(root);
                                    break;
                                case VerbClass.Iru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            ending = aStem + "なかった";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken(englishInflected, "%n", "not", "did not " + englishStems.Root);
                            else
                                englishInflected = ProcessToken(englishInflected, "%n", "not", englishInflected + " not");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case Politeness.Polite:
                    switch (verbClass)
                    {
                        case VerbClass.Ichidan:
                        case VerbClass.Godan:
                        case VerbClass.Yondan:
                            break;
                        case VerbClass.Suru:
                            ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kuru:
                            ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kureru:
                            break;
                        case VerbClass.Aru:
                            break;
                        case VerbClass.Iru:
                            break;
                        default:
                            inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                            returnValue = false;
                            break;
                    }
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            ending = iStem + "ました";
                            englishInflected = ProcessToken(englishInflected, "%n", null, englishInflected);
                            break;
                        case Polarity.Negative:
                            ending = iStem + "ません" + AuxSep + "でした";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken(englishInflected, "%n", "not", "did not " + englishStems.Root);
                            else
                                englishInflected = ProcessToken(englishInflected, "%n", "not", englishInflected + " not");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                default:
                    inflected.Error = "Unexpected politeness:" + politeness.ToString();
                    returnValue = false;
                    break;
            }

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            return returnValue;
        }

        protected bool ConjugatePresentRecursive(
            Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems,
            VerbClass verbClass,
            Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string aStem = japaneseStems.AStem;
            string eStem = japaneseStems.EStem;
            string iStem = japaneseStems.IStem;
            string oStem = japaneseStems.OStem;
            string uStem = japaneseStems.UStem;
            string taStem = japaneseStems.TAStem;
            string teStem = japaneseStems.TEStem;
            string ending = string.Empty;
            string englishInflected = englishStems.Root;
            bool returnValue = true;

            switch (politeness)
            {
                case Politeness.Plain:
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                case VerbClass.Suru:
                                    break;
                                case VerbClass.Kuru:
                                    break;
                                case VerbClass.Kureru:
                                case VerbClass.Aru:
                                case VerbClass.Iru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            switch (GetContraction(designation))
                            {
                                case Contraction.ContractionRuNoToNNo:
                                    if (uStem == "る")
                                        ending = "んの";
                                    else
                                    {
                                        ending = uStem;
                                        returnValue = false;
                                    }
                                    break;
                                default:
                                    ending = uStem;
                                    break;
                            }
                            englishInflected = ProcessToken(englishInflected, "%n", null, englishInflected);
                            break;
                        case Polarity.Negative:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Aru:
                                    stem = stem.Substring(0, stem.Length - 1);
                                    kanaStem = kanaStem.Substring(0, kanaStem.Length - 1);
                                    romajiStem = romajiStem.Substring(0, romajiStem.Length - 1);
                                    aStem = aStem.Substring(0, aStem.Length - 1);
                                    RemoveLastCharacter(root);
                                    break;
                                case VerbClass.Iru:
                                case VerbClass.Kureru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            switch (GetContraction(designation))
                            {
                                case Contraction.ContractionNaiToNee:
                                    ending = aStem + "ねえ";
                                    break;
                                case Contraction.ContractionNaiToN:
                                    ending = aStem + "ん";
                                    break;
                                case Contraction.ContractionRaNaiToNNai:
                                    if (uStem == "る")
                                        ending = "んない";
                                    else
                                    {
                                        ending = aStem + "ない";
                                        returnValue = false;
                                    }
                                    break;
                                default:
                                    ending = aStem + "ない";
                                    break;
                            }
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken(englishInflected, "%n", "not", "not " + englishInflected);
                            else
                                englishInflected = ProcessToken(englishInflected, "%n", "not", englishInflected + " not");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case Politeness.Polite:
                    switch (verbClass)
                    {
                        case VerbClass.Ichidan:
                        case VerbClass.Godan:
                        case VerbClass.Yondan:
                            break;
                        case VerbClass.Suru:
                            ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kuru:
                            ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kureru:
                            break;
                        case VerbClass.Aru:
                        case VerbClass.Iru:
                            break;
                        default:
                            inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                            returnValue = false;
                            break;
                    }
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            ending = iStem + "ます";
                            englishInflected = ProcessToken(englishInflected, "%n", null, englishInflected);
                            break;
                        case Polarity.Negative:
                            ending = iStem + "ません";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken(englishInflected, "%n", "not", "not " + englishInflected);
                            else
                                englishInflected = ProcessToken(englishInflected, "%n", "not", englishInflected + " not");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                default:
                    inflected.Error = "Unexpected politeness:" + politeness.ToString();
                    returnValue = false;
                    break;
            }

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            return returnValue;
        }

        protected bool ConjugatePastRecursive(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string aStem = japaneseStems.AStem;
            string eStem = japaneseStems.EStem;
            string iStem = japaneseStems.IStem;
            string oStem = japaneseStems.OStem;
            string uStem = japaneseStems.UStem;
            string taStem = japaneseStems.TAStem;
            string teStem = japaneseStems.TEStem;
            string ending = string.Empty;
            string englishInflected = englishStems.Preterite;
            bool returnValue = true;

            switch (politeness)
            {
                case Politeness.Plain:
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                case VerbClass.Iru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            ending = taStem;
                            englishInflected = ProcessToken(englishInflected, "%n", null, englishInflected);
                            break;
                        case Polarity.Negative:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKo(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                    stem = stem.Substring(0, stem.Length - 1);
                                    kanaStem = kanaStem.Substring(0, kanaStem.Length - 1);
                                    romajiStem = romajiStem.Substring(0, romajiStem.Length - 1);
                                    aStem = aStem.Substring(0, aStem.Length - 1);
                                    RemoveLastCharacter(root);
                                    break;
                                case VerbClass.Iru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            ending = aStem + "なかった";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken(englishInflected, "%n", "not", "not " + englishStems.Root);
                            else
                                englishInflected = ProcessToken(englishInflected, "%n", "not", englishInflected + " not");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case Politeness.Polite:
                    switch (verbClass)
                    {
                        case VerbClass.Ichidan:
                        case VerbClass.Godan:
                        case VerbClass.Yondan:
                            break;
                        case VerbClass.Suru:
                            ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kuru:
                            ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kureru:
                            break;
                        case VerbClass.Aru:
                            break;
                        case VerbClass.Iru:
                            break;
                        default:
                            inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                            returnValue = false;
                            break;
                    }
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            ending = iStem + "ました";
                            englishInflected = ProcessToken(englishInflected, "%n", null, englishInflected);
                            break;
                        case Polarity.Negative:
                            ending = iStem + "ません" + AuxSep + "でした";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken(englishInflected, "%n", "not", "not " + englishStems.Root);
                            else
                                englishInflected = ProcessToken(englishInflected, "%n", "not", englishInflected + " not");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                default:
                    inflected.Error = "Unexpected politeness:" + politeness.ToString();
                    returnValue = false;
                    break;
            }

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            return returnValue;
        }

        protected bool ConjugateVolitional(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string aStem = japaneseStems.AStem;
            string eStem = japaneseStems.EStem;
            string iStem = japaneseStems.IStem;
            string oStem = japaneseStems.OStem;
            string uStem = japaneseStems.UStem;
            string taStem = japaneseStems.TAStem;
            string teStem = japaneseStems.TEStem;
            string ending = string.Empty;
            string englishInflected = englishStems.Root;
            bool returnValue = true;

            switch (politeness)
            {
                case Politeness.Plain:
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKo(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                    break;
                                case VerbClass.Iru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            ending = oStem;
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken("will " + englishInflected.Replace("/am/are/is", ""), "%n", null, "will " + englishInflected);
                            else
                                englishInflected = ProcessToken("will " + englishInflected.Replace("/am/are/is", ""), "%n", null, "will be");
                            break;
                        case Polarity.Negative:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                    ending = "まい";
                                    break;
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                    ending = uStem + "まい";
                                    break;
                                case VerbClass.Suru:
                                    ending = uStem + "まい";
                                    break;
                                case VerbClass.Kuru:
                                    ending = uStem + "まい";
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                    ending = uStem + "まい";
                                    break;
                                case VerbClass.Iru:
                                    ending = "まい";
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken("will " + englishInflected.Replace("/am/are/is", ""), "%n", "not", "will not " + englishInflected);
                            else
                                englishInflected = ProcessToken("will " + englishInflected.Replace("/am/are/is", ""), "%n", "not", "will not be");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case Politeness.Polite:
                    switch (verbClass)
                    {
                        case VerbClass.Ichidan:
                        case VerbClass.Godan:
                        case VerbClass.Yondan:
                            break;
                        case VerbClass.Suru:
                            ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kuru:
                            ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Aru:
                            break;
                        case VerbClass.Iru:
                            break;
                        default:
                            inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                            returnValue = false;
                            break;
                    }
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            ending = iStem + "ましょう";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken("will " + englishInflected.Replace("/am/are/is", ""), "%n", null, "will " + englishInflected);
                            else
                                englishInflected = ProcessToken("will " + englishInflected.Replace("/am/are/is", ""), "%n", null, "will be");
                            break;
                        case Polarity.Negative:
                            ending = iStem + "ますまい";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken("will " + englishInflected.Replace("/am/are/is", ""), "%n", "not", "will not " + englishInflected);
                            else
                                englishInflected = ProcessToken("will " + englishInflected.Replace("/am/are/is", ""), "%n", "not", "will not be");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                default:
                    inflected.Error = "Unexpected politeness:" + politeness.ToString();
                    returnValue = false;
                    break;
            }

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            return returnValue;
        }

        protected bool ConjugatePresumptive(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string aStem = japaneseStems.AStem;
            string eStem = japaneseStems.EStem;
            string iStem = japaneseStems.IStem;
            string oStem = japaneseStems.OStem;
            string uStem = japaneseStems.UStem;
            string taStem = japaneseStems.TAStem;
            string teStem = japaneseStems.TEStem;
            string ending = string.Empty;
            string englishInflected = englishStems.Root;
            bool returnValue = true;

            switch (politeness)
            {
                case Politeness.Plain:
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                case VerbClass.Suru:
                                    break;
                                case VerbClass.Kuru:
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                    break;
                                case VerbClass.Iru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            ending = uStem + "だろう";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken("will probably " + englishInflected.Replace("/am/are/is", ""), "%n", null, "will probably " + englishInflected);
                            else
                                englishInflected = ProcessToken("will probably " + englishInflected.Replace("/am/are/is", ""), "%n", null, "will probably be");
                            break;
                        case Polarity.Negative:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                case VerbClass.Iru:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKo(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                    stem = stem.Substring(0, stem.Length - 1);
                                    kanaStem = kanaStem.Substring(0, kanaStem.Length - 1);
                                    romajiStem = romajiStem.Substring(0, romajiStem.Length - 1);
                                    aStem = aStem.Substring(0, aStem.Length - 1);
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            ending = aStem + "ないだろう";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken("will probably not " + englishInflected.Replace("/am/are/is", ""), "%n", null, "will probably not " + englishInflected);
                            else
                                englishInflected = ProcessToken("will probably not " + englishInflected.Replace("/am/are/is", ""), "%n", null, "will probably not be");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case Politeness.Polite:
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                case VerbClass.Suru:
                                    break;
                                case VerbClass.Kuru:
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                    break;
                                case VerbClass.Iru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            ending = uStem + "でしょう";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken("will probably " + englishInflected.Replace("/am/are/is", ""), "%n", null, "will probably " + englishInflected);
                            else
                                englishInflected = ProcessToken("will probably " + englishInflected.Replace("/am/are/is", ""), "%n", null, "will probably be");
                            break;
                        case Polarity.Negative:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                case VerbClass.Iru:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKo(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Aru:
                                    stem = stem.Substring(0, stem.Length - 1);
                                    kanaStem = kanaStem.Substring(0, kanaStem.Length - 1);
                                    romajiStem = romajiStem.Substring(0, romajiStem.Length - 1);
                                    aStem = aStem.Substring(0, aStem.Length - 1);
                                    RemoveLastCharacter(root);
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            ending = aStem + "ないでしょう";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken("will probably not " + englishInflected.Replace("/am/are/is", ""), "%n", null, "will probably not " + englishInflected);
                            else
                                englishInflected = ProcessToken("will probably not " + englishInflected.Replace("/am/are/is", ""), "%n", null, "will probably not be");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                default:
                    inflected.Error = "Unexpected politeness:" + politeness.ToString();
                    returnValue = false;
                    break;
            }

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            return returnValue;
        }

        protected bool ConjugatePastPresumptive(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string aStem = japaneseStems.AStem;
            string eStem = japaneseStems.EStem;
            string iStem = japaneseStems.IStem;
            string oStem = japaneseStems.OStem;
            string uStem = japaneseStems.UStem;
            string taStem = japaneseStems.TAStem;
            string teStem = japaneseStems.TEStem;
            string ending = string.Empty;
            string englishInflected = englishStems.Root;
            bool returnValue = true;

            switch (politeness)
            {
                case Politeness.Plain:
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                case VerbClass.Iru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            if (GetContraction(designation) == Contraction.ContractionTadarouToTarou)
                                ending = taStem + "ろう";
                            else
                                ending = taStem + "だろう";
                            englishInflected = ProcessToken("probably " + englishStems.Preterite.Replace("/am/are/is", ""), "%n", null, "probably " + englishStems.Preterite);
                            break;
                        case Polarity.Negative:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                case VerbClass.Iru:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKo(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                    stem = stem.Substring(0, stem.Length - 1);
                                    kanaStem = kanaStem.Substring(0, kanaStem.Length - 1);
                                    romajiStem = romajiStem.Substring(0, romajiStem.Length - 1);
                                    aStem = aStem.Substring(0, aStem.Length - 1);
                                    RemoveLastCharacter(root);
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            ending = aStem + "なかっただろう";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken("probably did not " + englishStems.Root.Replace("be/am/are/is", ""), "%n", null, "probably did not " + englishStems.Root);
                            else
                                englishInflected = ProcessToken("probably was/were not " + englishInflected.Replace("be/am/are/is", ""), "%n", null, "probably was/were not");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case Politeness.Polite:
                    switch (verbClass)
                    {
                        case VerbClass.Ichidan:
                        case VerbClass.Godan:
                        case VerbClass.Yondan:
                            break;
                        case VerbClass.Suru:
                            ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kuru:
                            if (polarity == Polarity.Negative)
                                ReplaceEndingKuToKo(ref stem, ref kanaStem, ref romajiStem);
                            else
                                ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kureru:
                            break;
                        case VerbClass.Aru:
                        case VerbClass.Iru:
                            break;
                        default:
                            inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                            returnValue = false;
                            break;
                    }
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            ending = taStem + "でしょう";
                            englishInflected = ProcessToken("probably " + englishStems.Preterite.Replace("/am/are/is", ""), "%n", null, "probably " + englishStems.Preterite);
                            break;
                        case Polarity.Negative:
                            ending = aStem + "なかったでしょう";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken("probably did not " + englishStems.Root.Replace("be/am/are/is", ""), "%n", null, "probably did not " + englishStems.Root);
                            else
                                englishInflected = ProcessToken("probably was/were not " + englishInflected.Replace("be/am/are/is", ""), "%n", null, "probably was/were not");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                default:
                    inflected.Error = "Unexpected politeness:" + politeness.ToString();
                    returnValue = false;
                    break;
            }

            englishInflected = ProcessToken(englishInflected, "%n", null, englishInflected);

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            return returnValue;
        }

        protected bool ConjugateContinuativeParticiple(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string aStem = japaneseStems.AStem;
            string eStem = japaneseStems.EStem;
            string iStem = japaneseStems.IStem;
            string oStem = japaneseStems.OStem;
            string uStem = japaneseStems.UStem;
            string taStem = japaneseStems.TAStem;
            string teStem = japaneseStems.TEStem;
            string ending = string.Empty;
            string englishInflected = englishStems.Root;
            bool returnValue = true;

            switch (politeness)
            {
                case Politeness.Plain:
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                case VerbClass.Iru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            ending = teStem;
                            englishInflected = ProcessToken(englishStems.Root, "%n", null, englishStems.Root) + " and; " + ProcessToken(englishStems.PresentParticiple, "%n", null, englishStems.PresentParticiple) + " and";
                            break;
                        case Polarity.Negative:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                case VerbClass.Iru:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKo(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                    stem = stem.Substring(0, stem.Length - 1);
                                    kanaStem = kanaStem.Substring(0, kanaStem.Length - 1);
                                    romajiStem = romajiStem.Substring(0, romajiStem.Length - 1);
                                    aStem = aStem.Substring(0, aStem.Length - 1);
                                    RemoveLastCharacter(root);
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            Alternate alternate = GetAlternate(designation);
                            switch (alternate)
                            {
                                default:
                                case Alternate.Alternate1:
                                    ending = aStem + "ないで";
                                    break;
                                case Alternate.Alternate2:
                                    if (GetContraction(designation) == Contraction.ContractionNakuteWaToNakucha)
                                        ending = aStem + "なくちゃ";
                                    else
                                        ending = aStem + "なくて";
                                    break;
                            }
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken("did not " + englishInflected.Replace("/am/are/is", "") + " and; not " + englishStems.PresentParticiple.Replace("/am/are/is", "") + " and", "%n", null, "did not " + englishStems.Root + " and; not " + englishStems.PresentParticiple + " and");
                            else
                                englishInflected = ProcessToken("not " + englishInflected.Replace("/am/are/is", "") + " and; not " + englishStems.PresentParticiple.Replace("/am/are/is", "") + " and", "%n", null, "am/are/is not and; am/are/is not and");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case Politeness.Polite:
                    switch (verbClass)
                    {
                        case VerbClass.Ichidan:
                        case VerbClass.Godan:
                        case VerbClass.Yondan:
                            break;
                        case VerbClass.Suru:
                            ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kuru:
                            ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kureru:
                            break;
                        case VerbClass.Aru:
                        case VerbClass.Iru:
                            break;
                        default:
                            inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                            returnValue = false;
                            break;
                    }
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            ending = iStem + "まして";
                            englishInflected = ProcessToken(englishStems.Root, "%n", null, englishStems.Root) + " and; " + ProcessToken(englishStems.PresentParticiple, "%n", null, englishStems.PresentParticiple) + " and";
                            break;
                        case Polarity.Negative:
                            ending = iStem + "ませんで";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken("did not " + englishInflected.Replace("/am/are/is", "") + " and; not " + englishStems.PresentParticiple.Replace("/am/are/is", "") + " and", "%n", null, "did not " + englishStems.Root + " and; not " + englishStems.PresentParticiple + " and");
                            else
                                englishInflected = ProcessToken("not " + englishInflected.Replace("/am/are/is", "") + " and; not " + englishStems.PresentParticiple.Replace("/am/are/is", "") + " and", "%n", null, "am/are/is not and; am/are/is not and");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                default:
                    inflected.Error = "Unexpected politeness:" + politeness.ToString();
                    returnValue = false;
                    break;
            }

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            return returnValue;
        }

        protected bool ConjugateContinuativeInfinitive(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string aStem = japaneseStems.AStem;
            string eStem = japaneseStems.EStem;
            string iStem = japaneseStems.IStem;
            string oStem = japaneseStems.OStem;
            string uStem = japaneseStems.UStem;
            string taStem = japaneseStems.TAStem;
            string teStem = japaneseStems.TEStem;
            string ending = iStem;
            string englishInflected = englishStems.Root;
            bool returnValue = true;

            switch (politeness)
            {
                case Politeness.Plain:
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                case VerbClass.Iru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = englishStems.Root + " and; " + englishStems.PresentParticiple + " and";
                            else
                                englishInflected = "am/are/is and; am/are/is and";
                            returnValue = CreateInflected(
                                    stem,
                                    kanaStem,
                                    romajiStem,
                                    ending,
                                    verbClass,
                                    englishInflected,
                                    inflected) &&
                                returnValue;
                            break;
                        case Polarity.Negative:
                            inflected.Error = "Not used in negative mode.";
                            returnValue = false;
                            CreateInflected(
                                stem,
                                kanaStem,
                                romajiStem,
                                ending,
                                verbClass,
                                englishInflected,
                                inflected);
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case Politeness.Polite:
                    inflected.Error = "Not used in polite mode.";
                    returnValue = false;
                    CreateInflected(
                        stem,
                        kanaStem,
                        romajiStem,
                        ending,
                        verbClass,
                        englishInflected,
                        inflected);
                    break;
                default:
                    inflected.Error = "Unexpected politeness:" + politeness.ToString();
                    returnValue = false;
                    break;
            }

            return returnValue;
        }

        protected bool ConjugateProgressive(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            MultiLanguageString output = inflected.Output;
            MultiLanguageString suffix = inflected.Suffix;
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            Mood subMood = GetSubMood(designation);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string teStem = japaneseStems.TEStem;
            string ending = teStem;
            string englishInflected = englishStems.PresentParticiple;
            bool needAuxSep = false;
            bool returnValue = true;

            switch (politeness)
            {
                default:
                case Politeness.Plain:
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                case VerbClass.Iru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        case Polarity.Negative:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                case VerbClass.Iru:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case Politeness.Polite:
                    switch (verbClass)
                    {
                        case VerbClass.Ichidan:
                        case VerbClass.Godan:
                        case VerbClass.Yondan:
                            break;
                        case VerbClass.Suru:
                            ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kuru:
                            ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kureru:
                            break;
                        case VerbClass.Aru:
                        case VerbClass.Iru:
                            break;
                        default:
                            inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
            }

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            Designator newDesignation = new Designator(designation);
            VerbClass newVerbClass = VerbClass.Iru;
            string kanji;
            JapaneseStems newJapaneseStems;
            string error = inflected.Error;
            Contraction contraction = GetContraction(designation);

            switch (contraction)
            {
                case Contraction.ContractionTeIruToTeru:
                    if (RemoveTeEnding(output) && RemoveTeEnding(suffix))
                    {
                        kanji = "てる";
                        newJapaneseStems = new JapaneseStems(kanji, "てる", "teru", "て", "て", "te");
                    }
                    else if (RemoveDeEnding(output) && RemoveDeEnding(suffix))
                    {
                        kanji = "でる";
                        newJapaneseStems = new JapaneseStems(kanji, "でる", "deru", "で", "で", "de");
                        returnValue = false;
                    }
                    else
                    {
                        kanji = "いる";
                        newJapaneseStems = new JapaneseStems(kanji, "いる", "iru", "い", "い", "i");
                        needAuxSep = true;
                        returnValue = false;
                    }
                    break;
                case Contraction.ContractionDeIruToDeru:
                    if (RemoveDeEnding(output) && RemoveDeEnding(suffix))
                    {
                        kanji = "でる";
                        newJapaneseStems = new JapaneseStems(kanji, "でる", "deru", "で", "で", "de");
                    }
                    else if (RemoveTeEnding(output) && RemoveTeEnding(suffix))
                    {
                        kanji = "てる";
                        newJapaneseStems = new JapaneseStems(kanji, "てる", "teru", "て", "て", "te");
                        returnValue = false;
                    }
                    else
                    {
                        kanji = "いる";
                        newJapaneseStems = new JapaneseStems(kanji, "いる", "iru", "い", "い", "i");
                        needAuxSep = true;
                        returnValue = false;
                    }
                    break;
                case Contraction.ContractionTeIkuToTeku:
                    if (RemoveTeEnding(output) && RemoveTeEnding(suffix))
                    {
                        kanji = "てく";
                        newJapaneseStems = new JapaneseStems(kanji, "てく", "teku", "て", "て", "te");
                    }
                    else if (RemoveDeEnding(output) && RemoveDeEnding(suffix))
                    {
                        kanji = "でく";
                        newJapaneseStems = new JapaneseStems(kanji, "でく", "deku", "で", "で", "de");
                        returnValue = false;
                    }
                    else
                    {
                        kanji = "いる";
                        newJapaneseStems = new JapaneseStems(kanji, "いる", "iru", "い", "い", "i");
                        needAuxSep = true;
                        returnValue = false;
                    }
                    englishInflected = "going to " + englishStems.Root;
                    output.SetText(EnglishID, englishInflected);
                    break;
                case Contraction.ContractionDeIkuToDeku:
                    if (RemoveDeEnding(output) && RemoveDeEnding(suffix))
                    {
                        kanji = "でく";
                        newJapaneseStems = new JapaneseStems(kanji, "でく", "deku", "で", "で", "de");
                    }
                    else if (RemoveTeEnding(output) && RemoveTeEnding(suffix))
                    {
                        kanji = "てく";
                        newJapaneseStems = new JapaneseStems(kanji, "てく", "teku", "て", "て", "te");
                        returnValue = false;
                    }
                    else
                    {
                        kanji = "いる";
                        newJapaneseStems = new JapaneseStems(kanji, "いる", "iru", "い", "い", "i");
                        needAuxSep = true;
                        returnValue = false;
                    }
                    englishInflected = "going to " + englishStems.Root;
                    output.SetText(EnglishID, englishInflected);
                    break;
                case Contraction.ContractionTeOkuToToku:
                    if (RemoveTeEnding(output) && RemoveTeEnding(suffix))
                    {
                        kanji = "とく";
                        newJapaneseStems = new JapaneseStems(kanji, "とく", "toku", "と", "と", "to");
                    }
                    else if (RemoveDeEnding(output) && RemoveDeEnding(suffix))
                    {
                        kanji = "どく";
                        newJapaneseStems = new JapaneseStems(kanji, "どく", "doku", "ど", "ど", "do");
                        returnValue = false;
                    }
                    else
                    {
                        kanji = "いる";
                        newJapaneseStems = new JapaneseStems(kanji, "いる", "iru", "い", "い", "i");
                        needAuxSep = true;
                        returnValue = false;
                    }
                    englishInflected += " up";
                    output.SetText(EnglishID, englishInflected);
                    break;
                case Contraction.ContractionTeAgeruToTageru:
                    if (RemoveTeEnding(output) && RemoveTeEnding(suffix))
                    {
                        kanji = "たげる";
                        newJapaneseStems = new JapaneseStems(kanji, "たげる", "tageru", "たげ", "たげ", "tage");
                    }
                    else if (RemoveDeEnding(output) && RemoveDeEnding(suffix))
                    {
                        kanji = "だげる";
                        newJapaneseStems = new JapaneseStems(kanji, "だげる", "dageru", "だげ", "だげ", "dage");
                        returnValue = false;
                    }
                    else
                    {
                        kanji = "いる";
                        newJapaneseStems = new JapaneseStems(kanji, "いる", "iru", "い", "い", "i");
                        needAuxSep = true;
                        returnValue = false;
                    }
                    englishInflected += " for you";
                    output.SetText(EnglishID, englishInflected);
                    break;
                default:
                    kanji = "いる";
                    newJapaneseStems = new JapaneseStems(kanji, "いる", "iru", "い", "い", "i");
                    needAuxSep = true;
                    break;
            }

            newDesignation.DeleteClassification("Special", "Progressive");
            newDesignation.DeleteClassification("Mood", "Progressive");

            if (!newDesignation.HasClassification("Mood"))
                newDesignation.InsertFirstClassification("Mood", "Indicative");

            if (!newDesignation.HasClassification("Tense"))
                newDesignation.InsertFirstClassification("Tense", "Present");

            error = inflected.Error;

            Inflection newInflected = new Inflection(
                null,
                newDesignation,
                LocalInflectionLanguageIDs);

            MultiLanguageString newOutput = newInflected.Output;

            returnValue = GetJapaneseVerbStems(
                kanji,
                newVerbClass,
                newJapaneseStems,
                newInflected.Root,
                ref error) && returnValue;

            inflected.Error = error;

            Mood mood2 = GetMood(newDesignation);

            EnglishStems newEnglishStems = new EnglishStems(
                (mood2 == Mood.Indicative ? "be/am/are/is%n " : "%nbe/am/are/is ") + englishInflected,
                "was/were%n " + englishInflected,
                "%nbeing " + englishInflected,
                "%nbeen " + englishInflected);

            returnValue = ConjugateRecurse(
                newDesignation,
                newJapaneseStems,
                newEnglishStems,
                newVerbClass,
                newInflected) && returnValue;

            output.SetText(JapaneseID, output.Text(JapaneseID) + newOutput.Text(JapaneseID));
            output.SetText(JapaneseKanaID, output.Text(JapaneseKanaID) + newOutput.Text(JapaneseKanaID));
            output.SetText(JapaneseRomajiID, output.Text(JapaneseRomajiID) +
                (needAuxSep ? AuxSep : "") +
                newOutput.Text(JapaneseRomajiID));
            output.SetText(EnglishID, newOutput.Text(EnglishID) /*+ " " + output.Text(EnglishID)*/);

            suffix.SetText(JapaneseID, suffix.Text(JapaneseID) + newOutput.Text(JapaneseID));
            suffix.SetText(JapaneseKanaID, suffix.Text(JapaneseKanaID) + newOutput.Text(JapaneseKanaID));
            suffix.SetText(JapaneseRomajiID, suffix.Text(JapaneseRomajiID) + newOutput.Text(JapaneseRomajiID));

            return returnValue;
        }

        protected bool ConjugateUnintentional(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            MultiLanguageString output = inflected.Output;
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            Mood subMood = GetSubMood(designation);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string teStem = japaneseStems.TEStem;
            string ending = teStem;
            string englishInflected = englishStems.PresentParticiple;
            bool needAuxSep = true;
            bool returnValue = true;

            switch (politeness)
            {
                default:
                case Politeness.Plain:
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                case VerbClass.Iru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        case Polarity.Negative:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                case VerbClass.Iru:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case Politeness.Polite:
                    switch (verbClass)
                    {
                        case VerbClass.Ichidan:
                        case VerbClass.Godan:
                        case VerbClass.Yondan:
                            break;
                        case VerbClass.Suru:
                            ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kuru:
                            ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kureru:
                            break;
                        case VerbClass.Aru:
                        case VerbClass.Iru:
                            break;
                        default:
                            inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
            }

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            Designator newDesignation = new Designator(designation);
            VerbClass newVerbClass = VerbClass.Godan;
            string kanji;
            JapaneseStems newJapaneseStems;
            //EnglishStems newEnglishStems = new EnglishStems("be/am/are/is unintentionally", "was/were unintentionally", "being unintentionally", "been unintentionally");
            EnglishStems newEnglishStems = new EnglishStems(
                "be/am/are/is%n unintentionally " + englishInflected,
                "was/were%n unintentionally " + englishInflected,
                "being%n unintentionally " + englishInflected,
                "%nbeen unintentionally " + englishInflected);
            string error = inflected.Error;

            switch (GetContraction(designation))
            {
                case Contraction.ContractionTeShimauToChau:
                    if (RemoveTeEnding(output))
                    {
                        kanji = "ちゃう";
                        newJapaneseStems = new JapaneseStems(kanji, "ちゃう", "chau", "ちゃ", "ちゃ", "cha");
                        RomajiTeFixupCheck(output, "c");
                        needAuxSep = false;
                    }
                    else
                    {
                        kanji = "しまう";
                        newJapaneseStems = new JapaneseStems(kanji, "しまう", "shimau", "しま", "しま", "shima");
                        returnValue = false;
                    }
                    break;
                case Contraction.ContractionTeShimauToChimau:
                    if (RemoveTeEnding(output))
                    {
                        kanji = "ちまう";
                        newJapaneseStems = new JapaneseStems(kanji, "ちまう", "chimau", "ちま", "ちま", "chima");
                        RomajiTeFixupCheck(output, "c");
                        needAuxSep = false;
                    }
                    else if (RemoveDeEnding(output))
                    {
                        kanji = "じまう";
                        newJapaneseStems = new JapaneseStems(kanji, "じまう", "jimau", "じま", "じま", "jima");
                        needAuxSep = false;
                        returnValue = false;
                    }
                    else
                    {
                        kanji = "しまう";
                        newJapaneseStems = new JapaneseStems(kanji, "しまう", "shimau", "しま", "しま", "shima");
                        returnValue = false;
                    }
                    break;
                case Contraction.ContractionDeShimauToJimau1:
                    if (RemoveDeEnding(output))
                    {
                        kanji = "じまう";
                        newJapaneseStems = new JapaneseStems(kanji, "じまう", "jimau", "じま", "じま", "jima");
                        needAuxSep = false;
                    }
                    else if (RemoveTeEnding(output))
                    {
                        kanji = "ちまう";
                        newJapaneseStems = new JapaneseStems(kanji, "ちまう", "chimau", "ちま", "ちま", "chima");
                        needAuxSep = false;
                        returnValue = false;
                    }
                    else
                    {
                        kanji = "しまう";
                        newJapaneseStems = new JapaneseStems(kanji, "しまう", "shimau", "しま", "しま", "shima");
                        returnValue = false;
                    }
                    break;
                case Contraction.ContractionDeShimauToJimau2:
                    if (RemoveDeEnding(output))
                    {
                        kanji = "ぢまう";
                        newJapaneseStems = new JapaneseStems(kanji, "ぢまう", "jimau", "ぢま", "ぢま", "jima");
                        needAuxSep = false;
                    }
                    else if (RemoveTeEnding(output))
                    {
                        kanji = "ちまう";
                        newJapaneseStems = new JapaneseStems(kanji, "ちまう", "chimau", "ちま", "ちま", "chima");
                        needAuxSep = false;
                        returnValue = false;
                    }
                    else
                    {
                        kanji = "しまう";
                        newJapaneseStems = new JapaneseStems(kanji, "しまう", "shimau", "しま", "しま", "shima");
                        returnValue = false;
                    }
                    break;
                default:
                    kanji = "しまう";
                    newJapaneseStems = new JapaneseStems(kanji, "しまう", "shimau", "しま", "しま", "shima");
                    break;
            }

            newDesignation.DeleteClassification("Mood", "Unintentional");

            if (!newDesignation.HasClassification("Mood"))
                newDesignation.InsertFirstClassification("Mood", "Indicative");

            if (!newDesignation.HasClassification("Tense"))
                newDesignation.InsertFirstClassification("Tense", "Present");

            error = inflected.Error;

            Inflection newInflected = new Inflection(
                null,
                newDesignation,
                LocalInflectionLanguageIDs);

            MultiLanguageString newOutput = newInflected.Output;

            returnValue = GetJapaneseVerbStems(
                kanji,
                newVerbClass,
                newJapaneseStems,
                newInflected.Root,
                ref error) && returnValue;

            inflected.Error = error;

            returnValue = ConjugateRecurseSecondary(
                null,
                newDesignation,
                newJapaneseStems,
                newEnglishStems,
                newVerbClass,
                newInflected) && returnValue;

            output.SetText(JapaneseID, output.Text(JapaneseID) + newOutput.Text(JapaneseID));
            output.SetText(JapaneseKanaID, output.Text(JapaneseKanaID) + newOutput.Text(JapaneseKanaID));
            output.SetText(JapaneseRomajiID, output.Text(JapaneseRomajiID) +
                (needAuxSep ? AuxSep : "") +
                newOutput.Text(JapaneseRomajiID));
            output.SetText(EnglishID, newOutput.Text(EnglishID) /*+ " " + output.Text(EnglishID)*/);

            FixupSuffix(inflected);

            return returnValue;
        }

        protected bool ConjugateProbative(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            MultiLanguageString output = inflected.Output;
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            Mood subMood = GetSubMood(designation);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string teStem = japaneseStems.TEStem;
            string ending = teStem;
            string englishInflected = englishStems.Root;
            bool returnValue = true;

            switch (politeness)
            {
                default:
                case Politeness.Plain:
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                case VerbClass.Iru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        case Polarity.Negative:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                case VerbClass.Iru:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case Politeness.Polite:
                    switch (verbClass)
                    {
                        case VerbClass.Ichidan:
                        case VerbClass.Godan:
                        case VerbClass.Yondan:
                            break;
                        case VerbClass.Suru:
                            ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kuru:
                            ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kureru:
                            break;
                        case VerbClass.Aru:
                        case VerbClass.Iru:
                            break;
                        default:
                            inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
            }

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            Designator newDesignation = new Designator(designation);
            VerbClass newVerbClass = VerbClass.Godan;
            string kanji;
            JapaneseStems newJapaneseStems;
            EnglishStems newEnglishStems = new EnglishStems(
                "%ntry to " + englishInflected,
                "%ntried to " + englishInflected,
                "%ntrying to " + englishInflected,
                "have %ntried to " + englishInflected);
            string error = inflected.Error;

            kanji = "みる";
            newJapaneseStems = new JapaneseStems(kanji, "みる", "miru", "み", "み", "mi");

            newDesignation.DeleteClassification("Mood", "Probative");

            if (!newDesignation.HasClassification("Mood"))
                newDesignation.InsertFirstClassification("Mood", "Indicative");

            if (!newDesignation.HasClassification("Tense"))
                newDesignation.InsertFirstClassification("Tense", "Present");

            error = inflected.Error;

            Inflection newInflected = new Inflection(
                null,
                newDesignation,
                LocalInflectionLanguageIDs);

            MultiLanguageString newOutput = newInflected.Output;

            returnValue = GetJapaneseVerbStems(
                kanji,
                newVerbClass,
                newJapaneseStems,
                newInflected.Root,
                ref error) && returnValue;

            inflected.Error = error;

            returnValue = ConjugateRecurseSecondary(
                null,
                newDesignation,
                newJapaneseStems,
                newEnglishStems,
                newVerbClass,
                newInflected) && returnValue;

            output.SetText(JapaneseID, output.Text(JapaneseID) + newOutput.Text(JapaneseID));
            output.SetText(JapaneseKanaID, output.Text(JapaneseKanaID) + newOutput.Text(JapaneseKanaID));
            output.SetText(JapaneseRomajiID, output.Text(JapaneseRomajiID) + AuxSep + newOutput.Text(JapaneseRomajiID));
            output.SetText(EnglishID, newOutput.Text(EnglishID) /*+ " " + output.Text(EnglishID)*/);

            FixupSuffix(inflected);

            return returnValue;
        }

        protected bool ConjugateImperative(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string aStem = japaneseStems.AStem;
            string eStem = japaneseStems.EStem;
            string iStem = japaneseStems.IStem;
            string oStem = japaneseStems.OStem;
            string uStem = japaneseStems.UStem;
            string taStem = japaneseStems.TAStem;
            string teStem = japaneseStems.TEStem;
            string ending = string.Empty;
            string englishInflected = englishStems.Root;
            bool returnValue = true;

            switch (politeness)
            {
                case Politeness.Abrupt:
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Iru:
                                    Alternate alternate = GetAlternate(designation);
                                    switch (alternate)
                                    {
                                        default:
                                        case Alternate.Alternate1:
                                            ending = "ろ";
                                            break;
                                        case Alternate.Alternate2:
                                            ending = "よ";
                                            break;
                                    }
                                    break;
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                    ending = eStem;
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    ending = "ろ";
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKo(ref stem, ref kanaStem, ref romajiStem);
                                    ending = "い";
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                    ending = eStem;
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        case Polarity.Negative:
                            ending = uStem + "な";
                            englishInflected = "do not " + englishInflected;
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case Politeness.Plain:
                    switch (verbClass)
                    {
                        case VerbClass.Ichidan:
                        case VerbClass.Godan:
                        case VerbClass.Yondan:
                            break;
                        case VerbClass.Suru:
                            ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kuru:
                            ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kureru:
                        case VerbClass.Aru:
                        case VerbClass.Iru:
                            break;
                        default:
                            inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                            returnValue = false;
                            break;
                    }
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            ending = iStem + "なさい";
                            break;
                        case Polarity.Negative:
                            ending = iStem + "なさるな";
                            englishInflected = "do not " + englishInflected;
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case Politeness.Polite:
                    switch (verbClass)
                    {
                        case VerbClass.Ichidan:
                        case VerbClass.Godan:
                        case VerbClass.Yondan:
                            break;
                        case VerbClass.Suru:
                            ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kuru:
                            ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kureru:
                        case VerbClass.Aru:
                        case VerbClass.Iru:
                            break;
                        default:
                            inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                            returnValue = false;
                            break;
                    }
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            ending = iStem + "なさい";
                            englishInflected = "please " + englishInflected;
                            break;
                        case Polarity.Negative:
                            ending = iStem + "なさるな";
                            englishInflected = "please do not " + englishInflected;
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                default:
                    inflected.Error = "Unexpected politeness:" + politeness.ToString();
                    returnValue = false;
                    break;
            }

            englishInflected = ProcessToken(englishInflected, "%n", null, englishInflected).Replace("/am/are/is", "");

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            return returnValue;
        }

        protected bool ConjugateRequest(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string aStem = japaneseStems.AStem;
            string eStem = japaneseStems.EStem;
            string iStem = japaneseStems.IStem;
            string oStem = japaneseStems.OStem;
            string uStem = japaneseStems.UStem;
            string taStem = japaneseStems.TAStem;
            string teStem = japaneseStems.TEStem;
            string ending = string.Empty;
            string englishInflected = englishStems.Root;
            bool returnValue = true;

            switch (verbClass)
            {
                case VerbClass.Ichidan:
                case VerbClass.Godan:
                case VerbClass.Yondan:
                case VerbClass.Kureru:
                case VerbClass.Aru:
                case VerbClass.Iru:
                    switch (politeness)
                    {
                        case Politeness.Plain:
                        case Politeness.Polite:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    Alternate alternate = GetAlternate(designation);
                                    switch (alternate)
                                    {
                                        default:
                                        case Alternate.Alternate1:
                                            ending = teStem + "ください";
                                            break;
                                        case Alternate.Alternate2:
                                            stem = "お" + stem;
                                            kanaStem = "お" + kanaStem;
                                            romajiStem = "o" + romajiStem;
                                            ending = iStem + "ください";
                                            break;
                                    }
                                    englishInflected = "please " + englishInflected;
                                    break;
                                case Polarity.Negative:
                                    ending = aStem + "ないでください";
                                    englishInflected = "please do not " + englishInflected;
                                    break;
                                default:
                                    inflected.Error = "Unexpected polarity:" + polarity.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        case Politeness.Honorific:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    stem = "お" + stem;
                                    kanaStem = "お" + kanaStem;
                                    romajiStem = "o" + romajiStem;
                                    ending = iStem + "なさいませ";
                                    englishInflected = "please " + englishInflected;
                                    break;
                                case Polarity.Negative:
                                    stem = "お" + stem;
                                    kanaStem = "お" + kanaStem;
                                    romajiStem = "o" + romajiStem;
                                    ending = iStem + "なさいますな";
                                    englishInflected = "please do not " + englishInflected;
                                    break;
                                default:
                                    inflected.Error = "Unexpected polarity:" + polarity.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        default:
                            inflected.Error = "Unexpected politeness:" + politeness.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case VerbClass.Suru:
                    switch (politeness)
                    {
                        case Politeness.Plain:
                        case Politeness.Polite:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    Alternate alternate = GetAlternate(designation);
                                    switch (alternate)
                                    {
                                        default:
                                        case Alternate.Alternate1:
                                            ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                            ending = teStem + "ください";
                                            break;
                                        case Alternate.Alternate2:
                                            stem = stem.Substring(0, stem.Length - 1);
                                            kanaStem = kanaStem.Substring(0, kanaStem.Length - 1);
                                            romajiStem = romajiStem.Substring(0, romajiStem.Length - 1);
                                            ending = "なさってください";
                                            break;
                                    }
                                    englishInflected = "please " + englishInflected;
                                    break;
                                case Polarity.Negative:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    ending = stem + "ないでください";
                                    englishInflected = "please do not " + englishInflected;
                                    break;
                                default:
                                    inflected.Error = "Unexpected polarity:" + polarity.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        case Politeness.Honorific:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    stem = stem.Substring(0, stem.Length - 1);
                                    kanaStem = kanaStem.Substring(0, kanaStem.Length - 1);
                                    romajiStem = romajiStem.Substring(0, romajiStem.Length - 1);
                                    ending = "なさいませ";
                                    englishInflected = "please " + englishInflected;
                                    break;
                                case Polarity.Negative:
                                    ending = "なさいますな";
                                    englishInflected = "please do not " + englishInflected;
                                    break;
                                default:
                                    inflected.Error = "Unexpected polarity:" + polarity.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        default:
                            inflected.Error = "Unexpected politeness:" + politeness.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case VerbClass.Kuru:
                    switch (politeness)
                    {
                        case Politeness.Plain:
                        case Politeness.Polite:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                                    Alternate alternate = GetAlternate(designation);
                                    switch (alternate)
                                    {
                                        default:
                                        case Alternate.Alternate1:
                                            ending = teStem + "ください";
                                            break;
                                        case Alternate.Alternate2:
                                            stem = "おこし";
                                            kanaStem = "おこし";
                                            romajiStem = "okoshi";
                                            ending = "ください";
                                            break;
                                    }
                                    englishInflected = "please " + englishInflected;
                                    break;
                                case Polarity.Negative:
                                    ReplaceEndingKuToKo(ref stem, ref kanaStem, ref romajiStem);
                                    ending = "ないでください";
                                    englishInflected = "please do not " + englishInflected;
                                    break;
                                default:
                                    inflected.Error = "Unexpected polarity:" + polarity.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        case Politeness.Honorific:
                            stem = "おいで";
                            kanaStem = "おいで";
                            romajiStem = "oide";
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                    ending = "なさいませ";
                                    englishInflected = "please " + englishInflected;
                                    break;
                                case Polarity.Negative:
                                    ending = "なさいますな";
                                    englishInflected = "please do not " + englishInflected;
                                    break;
                                default:
                                    inflected.Error = "Unexpected polarity:" + polarity.ToString();
                                    returnValue = false;
                                    break;
                            }
                            break;
                        default:
                            inflected.Error = "Unexpected politeness:" + politeness.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                default:
                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                    returnValue = false;
                    break;
            }

            englishInflected = ProcessToken(englishInflected, "%n", null, englishInflected).Replace("/am/are/is", "");

            inflected.Output.SetText(JapaneseRomajiID, romajiStem);

            returnValue = CreateIrregularInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            return returnValue;
        }

        protected bool ConjugateProvisional(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string aStem = japaneseStems.AStem;
            string eStem = japaneseStems.EStem;
            string iStem = japaneseStems.IStem;
            string oStem = japaneseStems.OStem;
            string uStem = japaneseStems.UStem;
            string taStem = japaneseStems.TAStem;
            string teStem = japaneseStems.TEStem;
            string ending = string.Empty;
            string englishInflected = englishStems.Root;
            bool returnValue = true;

            switch (polarity)
            {
                case Polarity.Positive:
                    switch (verbClass)
                    {
                        case VerbClass.Ichidan:
                        case VerbClass.Godan:
                        case VerbClass.Yondan:
                        case VerbClass.Suru:
                            break;
                        case VerbClass.Kuru:
                            kanaStem = kanaStem.Substring(0, kanaStem.Length - 1) + "く";
                            break;
                        case VerbClass.Kureru:
                            break;
                        case VerbClass.Aru:
                        case VerbClass.Iru:
                            break;
                        default:
                            inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                            returnValue = false;
                            break;
                    }
                    ending = eStem + "ば";
                    englishInflected = "if " + englishInflected;
                    englishInflected = ProcessToken(englishInflected.Replace("be/am/are/is", ""), "%n", null, englishInflected);
                    break;
                case Polarity.Negative:
                    switch (verbClass)
                    {
                        case VerbClass.Ichidan:
                        case VerbClass.Godan:
                        case VerbClass.Yondan:
                        case VerbClass.Iru:
                            break;
                        case VerbClass.Suru:
                            ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kuru:
                            ReplaceEndingKuToKo(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kureru:
                            break;
                        case VerbClass.Aru:
                            stem = stem.Substring(0, stem.Length - 1);
                            kanaStem = kanaStem.Substring(0, kanaStem.Length - 1);
                            romajiStem = romajiStem.Substring(0, romajiStem.Length - 1);
                            aStem = aStem.Substring(0, aStem.Length - 1);
                            RemoveLastCharacter(root);
                            break;
                        default:
                            inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                            returnValue = false;
                            break;
                    }
                    if (GetContraction(designation) == Contraction.ContractionNakerebaToNakya)
                        ending = aStem + "なきゃ";
                    else
                        ending = aStem + "なければ";
                    if ((verbClass == VerbClass.Godan) && englishInflected.Contains("unintentional"))
                        englishInflected = ProcessToken("if " + englishInflected.Replace("be/am/are/is", ""), "%n", "not", "if not " + englishInflected);
                    else if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                        englishInflected = ProcessToken("if does " + englishInflected.Replace("be/am/are/is", ""), "%n", "not", "if does not " + englishInflected);
                    else
                        englishInflected = ProcessToken("if " + englishInflected.Replace("be/am/are/is", ""), "%n", "not", "if " + englishInflected + " not");
                    break;
                default:
                    inflected.Error = "Unexpected polarity:" + polarity.ToString();
                    returnValue = false;
                    break;
            }

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            return returnValue;
        }

        protected bool ConjugateConditional(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string aStem = japaneseStems.AStem;
            string eStem = japaneseStems.EStem;
            string iStem = japaneseStems.IStem;
            string oStem = japaneseStems.OStem;
            string uStem = japaneseStems.UStem;
            string taStem = japaneseStems.TAStem;
            string teStem = japaneseStems.TEStem;
            string ending = string.Empty;
            string englishInflected = englishStems.Root.Replace("be/am/are/is", "");
            bool returnValue = true;

            switch (politeness)
            {
                case Politeness.Plain:
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                case VerbClass.Iru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            ending = taStem + "ら";
                            if (englishInflected.Contains("try "))
                                englishInflected = ProcessToken("if were to " + englishInflected, "%n", null, "if were to " + englishInflected);
                            else
                                englishInflected = ProcessToken("if were to be " + englishInflected, "%n", null, "if were to " + englishInflected);
                            break;
                        case Polarity.Negative:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                case VerbClass.Iru:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKo(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                    stem = stem.Substring(0, stem.Length - 1);
                                    kanaStem = kanaStem.Substring(0, kanaStem.Length - 1);
                                    romajiStem = romajiStem.Substring(0, romajiStem.Length - 1);
                                    aStem = aStem.Substring(0, aStem.Length - 1);
                                    RemoveLastCharacter(root);
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            ending = aStem + "なかったら";
                            if (englishInflected.Contains("try "))
                                englishInflected = ProcessToken("if were to not " + englishInflected, "%n", "", "if were to not " + englishInflected);
                            else
                                englishInflected = ProcessToken("if were to not be " + englishInflected, "%n", "", "if were to not " + englishInflected);
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case Politeness.Polite:
                    switch (verbClass)
                    {
                        case VerbClass.Ichidan:
                        case VerbClass.Godan:
                        case VerbClass.Yondan:
                            break;
                        case VerbClass.Suru:
                            ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kuru:
                            ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kureru:
                            break;
                        case VerbClass.Aru:
                        case VerbClass.Iru:
                            break;
                        default:
                            inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                            returnValue = false;
                            break;
                    }
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            ending = iStem + "ましたら";
                            if (englishInflected.Contains("try "))
                                englishInflected = ProcessToken("if were to " + englishInflected, "%n", null, "if were to " + englishInflected);
                            else
                                englishInflected = ProcessToken("if were to be " + englishInflected, "%n", null, "if were to " + englishInflected);
                            break;
                        case Polarity.Negative:
                            ending = iStem + "ませんでしたら";
                            if (englishInflected.Contains("try "))
                                englishInflected = ProcessToken("if were to not " + englishInflected, "%n", "", "if were to not " + englishInflected);
                            else
                                englishInflected = ProcessToken("if were to not be " + englishInflected, "%n", "", "if were to not " + englishInflected);
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                default:
                    inflected.Error = "Unexpected politeness:" + politeness.ToString();
                    returnValue = false;
                    break;
            }

            englishInflected = ProcessToken(englishInflected, "%n", null, englishInflected);

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            return returnValue;
        }

        protected bool ConjugateAlternative(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            Politeness politeness = GetPoliteness(designation);
            Polarity polarity = GetPolarity(designation);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string aStem = japaneseStems.AStem;
            string eStem = japaneseStems.EStem;
            string iStem = japaneseStems.IStem;
            string oStem = japaneseStems.OStem;
            string uStem = japaneseStems.UStem;
            string taStem = japaneseStems.TAStem;
            string teStem = japaneseStems.TEStem;
            string ending = string.Empty;
            string englishInflected = englishStems.PresentParticiple;
            bool returnValue = true;

            switch (politeness)
            {
                case Politeness.Plain:
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                case VerbClass.Iru:
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            ending = taStem + "り";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken("things like " + englishInflected.Replace("being", "doing") + " and ...", "%n", null, "things like " + englishInflected.Replace("being", "doing") + " and ...");
                            else
                                englishInflected = ProcessToken("things like " + englishInflected.Replace("being being", "doing being") + " and ...", "%n", null, "things like " + englishInflected.Replace("being being", "doing being") + " and ...");
                            break;
                        case Polarity.Negative:
                            switch (verbClass)
                            {
                                case VerbClass.Ichidan:
                                case VerbClass.Godan:
                                case VerbClass.Yondan:
                                case VerbClass.Iru:
                                    break;
                                case VerbClass.Suru:
                                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kuru:
                                    ReplaceEndingKuToKo(ref stem, ref kanaStem, ref romajiStem);
                                    break;
                                case VerbClass.Kureru:
                                    break;
                                case VerbClass.Aru:
                                    stem = stem.Substring(0, stem.Length - 1);
                                    kanaStem = kanaStem.Substring(0, kanaStem.Length - 1);
                                    romajiStem = romajiStem.Substring(0, romajiStem.Length - 1);
                                    aStem = aStem.Substring(0, aStem.Length - 1);
                                    RemoveLastCharacter(root);
                                    break;
                                default:
                                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                                    returnValue = false;
                                    break;
                            }
                            ending = aStem + "なかったり";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken("things like not " + englishInflected.Replace("being", "doing") + " and ...", "%n", null, "things like not " + englishInflected.Replace("being", "doing") + " and ...");
                            else
                                englishInflected = ProcessToken("things like not " + englishInflected.Replace("being being", "doing being") + " and ...", "%n", null, "things like not " + englishInflected.Replace("being being", "doing being") + " and ...");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                case Politeness.Polite:
                    switch (verbClass)
                    {
                        case VerbClass.Ichidan:
                        case VerbClass.Godan:
                        case VerbClass.Yondan:
                            break;
                        case VerbClass.Suru:
                            ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kuru:
                            ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                            break;
                        case VerbClass.Kureru:
                            break;
                        case VerbClass.Aru:
                        case VerbClass.Iru:
                            break;
                        default:
                            inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                            returnValue = false;
                            break;
                    }
                    switch (polarity)
                    {
                        case Polarity.Positive:
                            ending = iStem + "ましたり";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken("things like " + englishInflected.Replace("being", "doing") + " and ...", "%n", null, "things like " + englishInflected.Replace("being", "doing") + " and ...");
                            else
                                englishInflected = ProcessToken("things like " + englishInflected.Replace("being being", "doing being") + " and ...", "%n", null, "things like " + englishInflected.Replace("being being", "doing being") + " and ...");
                            break;
                        case Polarity.Negative:
                            ending = iStem + "ませんでしたり";
                            if ((verbClass != VerbClass.Aru) && (verbClass != VerbClass.Iru))
                                englishInflected = ProcessToken("things like not " + englishInflected.Replace("being", "doing") + " and ...", "%n", null, "things like not " + englishInflected.Replace("being", "doing") + " and ...");
                            else
                                englishInflected = ProcessToken("things like not " + englishInflected.Replace("being being", "doing being") + " and ...", "%n", null, "things like not " + englishInflected.Replace("being being", "doing being") + " and ...");
                            break;
                        default:
                            inflected.Error = "Unexpected polarity:" + polarity.ToString();
                            returnValue = false;
                            break;
                    }
                    break;
                default:
                    inflected.Error = "Unexpected politeness:" + politeness.ToString();
                    returnValue = false;
                    break;
            }

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            return returnValue;
        }

        protected bool ConjugatePotential(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string eStem = japaneseStems.EStem;
            string ending = string.Empty;
            string englishInflected = englishStems.Root;
            VerbClass newVerbClass = VerbClass.Ichidan;
            bool returnValue = true;

            switch (verbClass)
            {
                case VerbClass.Ichidan:
                case VerbClass.Kureru:
                    ending = "られる";
                    break;
                case VerbClass.Iru:
                    ending = "られる";
                    englishStems = new EnglishStems("be", "be", "be", "be");
                    break;
                case VerbClass.Godan:
                case VerbClass.Yondan:
                    ending = eStem + "る";
                    break;
                case VerbClass.Suru:
                    stem = "できる";
                    kanaStem = "できる";
                    romajiStem = "dekiru";
                    SetJapaneseText(inflected.Root, "でき", "でき", "deki");
                    japaneseStems = new JapaneseStems(japaneseStems);
                    japaneseStems.DictionaryFormJapanese = "できる";
                    japaneseStems.DictionaryFormKana = "できる";
                    japaneseStems.DictionaryFormRomaji = "dekiru";
                    japaneseStems.StemJapanese = "でき";
                    japaneseStems.StemKana = "でき";
                    japaneseStems.StemRomaji = "deki";
                    break;
                case VerbClass.Kuru:
                    ReplaceEndingKuToKo(ref stem, ref kanaStem, ref romajiStem);
                    ending = "られる";
                    break;
                case VerbClass.Aru:
                    ending = eStem + "る";
                    englishStems = new EnglishStems("exist", "existed", "existing", "existed");
                    break;
                default:
                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                    returnValue = false;
                    break;
            }

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            if (returnValue)
            {
                Inflection newInflected = new Inflection(
                    String.Empty,
                    null,
                    designation,
                    new MultiLanguageString(String.Empty, LocalInflectionLanguageIDs),
                    new MultiLanguageString(String.Empty, LocalInflectionLanguageIDs),
                    new MultiLanguageString(inflected.Output),
                    new MultiLanguageString(String.Empty, LocalInflectionLanguageIDs),
                    new MultiLanguageString(String.Empty, LocalInflectionLanguageIDs),
                    new MultiLanguageString(String.Empty, LocalInflectionLanguageIDs),
                    LexicalCategory.Verb,
                    inflected.CategoryString,
                    true,
                    String.Empty);
                returnValue = ConjugateSpecialRecursive(
                    designation, newVerbClass,
                    englishStems, inflected, newInflected);
            }

            if (returnValue)
            {
                Mood mood = GetMood(designation);
                Mood subMood;
                Tense tense = GetTense(designation);
                Polarity polarity = GetPolarity(designation);
                Politeness politeness = GetPoliteness(designation);

                englishInflected = inflected.Output.Text(EnglishID);

                switch (mood)
                {
                    case Mood.Unknown:
                        Special special = GetSpecial2(designation);
                        switch (special)
                        {
                            case Special.Dictionary:
                                englishInflected = "can " + englishInflected;
                                break;
                            case Special.Participle:
                                englishInflected = "can be " + englishInflected;
                                break;
                            case Special.Perfective:
                                englishInflected = "able to have " + englishInflected;
                                break;
                            case Special.Infinitive:
                                englishInflected = "able " + englishInflected;
                                break;
                            case Special.Potential:
                            case Special.Passive:
                            case Special.Causitive:
                            case Special.Honorific1:
                            case Special.Honorific2:
                            case Special.Humble1:
                            case Special.Humble2:
                            default:
                                switch (tense)
                                {
                                    case Tense.Present:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "can " + englishStems.Root;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "can not " + englishStems.Root;
                                                break;
                                        }
                                        break;
                                    case Tense.Past:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "was/were able to " + englishStems.Root;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "was/were not able to " + englishStems.Root;
                                                break;
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                    case Mood.Indicative:
                    case Mood.Recursive:
                    default:
                        switch (tense)
                        {
                            case Tense.Present:
                            default:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "can " + englishStems.Root;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "can not " + englishStems.Root;
                                        break;
                                }
                                break;
                            case Tense.Past:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "was/were able to " + englishStems.Root;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "was/were not able to " + englishStems.Root;
                                        break;
                                }
                                break;
                        }
                        break;
                    case Mood.Volitional:
                        switch (polarity)
                        {
                            case Polarity.Positive:
                            default:
                                englishInflected = "will be able to " + englishStems.Root;
                                break;
                            case Polarity.Negative:
                                englishInflected = "will not be able to " + englishStems.Root;
                                break;
                        }
                        break;
                    case Mood.Presumptive:
                        switch (tense)
                        {
                            case Tense.Present:
                            default:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "will probably be able to " + englishStems.Root;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "will probably not be able to " + englishStems.Root;
                                        break;
                                }
                                break;
                            case Tense.Past:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "will probably be able to have " + englishStems.PastParticiple;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "will probably not be able to to have " + englishStems.PastParticiple;
                                        break;
                                }
                                break;
                        }
                        break;
                    case Mood.ContinuativeParticiple:
                    case Mood.ContinuativeInfinitive:
                        switch (polarity)
                        {
                            case Polarity.Positive:
                            default:
                                englishInflected = "able to " + englishStems.Root + " and";
                                break;
                            case Polarity.Negative:
                                englishInflected = "not be able to " + englishStems.Root + " and";
                                break;
                        }
                        break;
                    case Mood.Progressive:
                        subMood = GetSubMood(designation);
                        switch (subMood)
                        {
                            case Mood.Indicative:
                            case Mood.Recursive:
                            case Mood.Unknown:
                            default:
                                switch (tense)
                                {
                                    case Tense.Present:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "can be " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "can not be " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                    case Tense.Past:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "was/were able to be " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "was/were not able to be " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.Volitional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "will be able to be " + englishStems.PresentParticiple;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "will not be able to be " + englishStems.PresentParticiple;
                                        break;
                                }
                                break;
                            case Mood.Presumptive:
                                switch (tense)
                                {
                                    case Tense.Present:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "will probably be able to be " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "will probably not be able to be " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                    case Tense.Past:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "will probably be able to have been " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "will probably not be able to to have been " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.ContinuativeParticiple:
                            case Mood.ContinuativeInfinitive:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "able to be " + englishStems.PresentParticiple + " and";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "not be able to be " + englishStems.PresentParticiple + " and";
                                        break;
                                }
                                break;
                            case Mood.Imperative:
                                switch (politeness)
                                {
                                    case Politeness.Abrupt:
                                    case Politeness.Plain:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "be able to be " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "do not be able to be " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                    case Politeness.Polite:
                                    case Politeness.Honorific:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "please be able to be " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "please do not be able to be " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.Request:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "please be able to be " + englishStems.PresentParticiple;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "please do not be able to be " + englishStems.PresentParticiple;
                                        break;
                                }
                                break;
                            case Mood.Provisional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "if able to be " + englishStems.PresentParticiple;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "if not be able to be " + englishStems.PresentParticiple;
                                        break;
                                }
                                break;
                            case Mood.Conditional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "when able to be " + englishStems.PresentParticiple;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "when not be able to be " + englishStems.PresentParticiple;
                                        break;
                                }
                                break;
                            case Mood.Alternative:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "able to be " + englishStems.PresentParticiple + " and";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "not be able to be " + englishStems.PresentParticiple + " and";
                                        break;
                                }
                                break;
                        }
                        break;
                    case Mood.Unintentional:
                        subMood = GetSubMood(designation);
                        switch (subMood)
                        {
                            case Mood.Indicative:
                            case Mood.Recursive:
                            case Mood.Unknown:
                            default:
                                switch (tense)
                                {
                                    case Tense.Present:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "can be unintentionally " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "can not be unintentionally " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                    case Tense.Past:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "was/were able to be unintentionally " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "was/were not able to be unintentionally " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.Volitional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "will be able to be unintentionally " + englishStems.PresentParticiple;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "will not be able to be unintentionally " + englishStems.PresentParticiple;
                                        break;
                                }
                                break;
                            case Mood.Presumptive:
                                switch (tense)
                                {
                                    case Tense.Present:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "will probably be able to be unintentionally " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "will probably not be able to be unintentionally " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                    case Tense.Past:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "will probably be able to have been unintentionally " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "will probably not be able to to have been unintentionally " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.ContinuativeParticiple:
                            case Mood.ContinuativeInfinitive:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "able to be unintentionally " + englishStems.PresentParticiple + " and";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "not be able to be unintentionally " + englishStems.PresentParticiple + " and";
                                        break;
                                }
                                break;
                            case Mood.Imperative:
                                switch (politeness)
                                {
                                    case Politeness.Abrupt:
                                    case Politeness.Plain:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "be able to be unintentionally " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "do not be able to be unintentionally " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                    case Politeness.Polite:
                                    case Politeness.Honorific:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "please be able to be unintentionally " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "please do not be able to be unintentionally " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.Request:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "please be able to be unintentionally " + englishStems.PresentParticiple;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "please do not be able to be unintentionally " + englishStems.PresentParticiple;
                                        break;
                                }
                                break;
                            case Mood.Provisional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "if able to be unintentionally " + englishStems.PresentParticiple;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "if not be able to be unintentionally " + englishStems.PresentParticiple;
                                        break;
                                }
                                break;
                            case Mood.Conditional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "when able to be unintentionally " + englishStems.PresentParticiple;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "when not be able to be unintentionally " + englishStems.PresentParticiple;
                                        break;
                                }
                                break;
                            case Mood.Alternative:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "able to be unintentionally " + englishStems.PresentParticiple + " and";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "not be able to be unintentionally " + englishStems.PresentParticiple + " and";
                                        break;
                                }
                                break;
                        }
                        break;
                    case Mood.Probative:
                        subMood = GetSubMood(designation);
                        switch (subMood)
                        {
                            case Mood.Indicative:
                            case Mood.Recursive:
                            case Mood.Unknown:
                            default:
                                switch (tense)
                                {
                                    case Tense.Present:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "can try to " + englishStems.Root;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "can not try to " + englishStems.Root;
                                                break;
                                        }
                                        break;
                                    case Tense.Past:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "was/were able to try to " + englishStems.Root;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "was/were not able to try to " + englishStems.Root;
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.Volitional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "will be able to try to " + englishStems.Root;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "will not be able to try to " + englishStems.Root;
                                        break;
                                }
                                break;
                            case Mood.Presumptive:
                                switch (tense)
                                {
                                    case Tense.Present:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "will probably be able to try to " + englishStems.Root;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "will probably not be able to try to " + englishStems.Root;
                                                break;
                                        }
                                        break;
                                    case Tense.Past:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "will probably be able to have been unintentionally " + englishStems.Root;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "will probably not be able to to have been unintentionally " + englishStems.Root;
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.ContinuativeParticiple:
                            case Mood.ContinuativeInfinitive:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "able to try to " + englishStems.Root + " and";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "not be able to try to " + englishStems.Root + " and";
                                        break;
                                }
                                break;
                            case Mood.Imperative:
                                switch (politeness)
                                {
                                    case Politeness.Abrupt:
                                    case Politeness.Plain:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "be able to try to " + englishStems.Root;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "do not be able to try to " + englishStems.Root;
                                                break;
                                        }
                                        break;
                                    case Politeness.Polite:
                                    case Politeness.Honorific:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "please be able to try to " + englishStems.Root;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "please do not be able to try to " + englishStems.Root;
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.Request:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "please be able to try to " + englishStems.Root;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "please do not be able to try to " + englishStems.Root;
                                        break;
                                }
                                break;
                            case Mood.Provisional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "if able to try to " + englishStems.Root;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "if not be able to try to " + englishStems.Root;
                                        break;
                                }
                                break;
                            case Mood.Conditional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "when able to try to " + englishStems.Root;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "when not be able to try to " + englishStems.Root;
                                        break;
                                }
                                break;
                            case Mood.Alternative:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "able to try to " + englishStems.Root + " and";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "not be able to try to " + englishStems.Root + " and";
                                        break;
                                }
                                break;
                        }
                        break;
                    case Mood.Imperative:
                        switch (politeness)
                        {
                            case Politeness.Abrupt:
                            case Politeness.Plain:
                            default:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "be able to " + englishStems.Root;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "do not be able to " + englishStems.Root;
                                        break;
                                }
                                break;
                            case Politeness.Polite:
                            case Politeness.Honorific:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "please be able to " + englishStems.Root;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "please do not be able to " + englishStems.Root;
                                        break;
                                }
                                break;
                        }
                        break;
                    case Mood.Request:
                        switch (polarity)
                        {
                            case Polarity.Positive:
                            default:
                                englishInflected = "please be able to " + englishStems.Root;
                                break;
                            case Polarity.Negative:
                                englishInflected = "please do not be able to " + englishStems.Root;
                                break;
                        }
                        break;
                    case Mood.Provisional:
                        switch (polarity)
                        {
                            case Polarity.Positive:
                            default:
                                englishInflected = "if able to " + englishStems.Root;
                                break;
                            case Polarity.Negative:
                                englishInflected = "if not be able to " + englishStems.Root;
                                break;
                        }
                        break;
                    case Mood.Conditional:
                        switch (polarity)
                        {
                            case Polarity.Positive:
                            default:
                                englishInflected = "when able to " + englishStems.Root;
                                break;
                            case Polarity.Negative:
                                englishInflected = "when not be able to " + englishStems.Root;
                                break;
                        }
                        break;
                    case Mood.Alternative:
                        switch (polarity)
                        {
                            case Polarity.Positive:
                            default:
                                englishInflected = "able to " + englishStems.Root + " and";
                                break;
                            case Polarity.Negative:
                                englishInflected = "not be able to " + englishStems.Root + " and";
                                break;
                        }
                        break;
                }

                inflected.Output.SetText(EnglishID, englishInflected);
            }

            return returnValue;
        }

        protected bool ConjugatePassive(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string aStem = japaneseStems.AStem;
            string eStem = japaneseStems.EStem;
            string iStem = japaneseStems.IStem;
            string oStem = japaneseStems.OStem;
            string uStem = japaneseStems.UStem;
            string taStem = japaneseStems.TAStem;
            string teStem = japaneseStems.TEStem;
            string ending = string.Empty;
            string englishInflected = englishStems.Root;
            bool returnValue = true;

            switch (verbClass)
            {
                case VerbClass.Ichidan:
                case VerbClass.Kureru:
                    ending = "られる";
                    break;
                case VerbClass.Iru:
                    ending = "られる";
                    englishStems = new EnglishStems("be", "be", "be", "be");
                    break;
                case VerbClass.Godan:
                case VerbClass.Yondan:
                    ending = aStem + "れる";
                    break;
                case VerbClass.Suru:
                    stem = stem.Substring(0, stem.Length - 1) + "さ";
                    kanaStem = kanaStem.Substring(0, kanaStem.Length - 1) + "さ";
                    romajiStem = romajiStem.Substring(0, romajiStem.Length - 2) + "sa";
                    ending = "れる";
                    break;
                case VerbClass.Kuru:
                    ReplaceEndingKuToKo(ref stem, ref kanaStem, ref romajiStem);
                    ending = "られる";
                    break;
                case VerbClass.Aru:
                    ending = aStem + "れる";
                    englishStems = new EnglishStems("exist", "existed", "existing", "existed");
                    break;
                default:
                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                    returnValue = false;
                    break;
            }

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            if (returnValue)
            {
                Inflection newInflected = new Inflection(
                    String.Empty,
                    null,
                    designation,
                    new MultiLanguageString(String.Empty, LocalInflectionLanguageIDs),
                    new MultiLanguageString(String.Empty, LocalInflectionLanguageIDs),
                    new MultiLanguageString(inflected.Output),
                    new MultiLanguageString(String.Empty, LocalInflectionLanguageIDs),
                    new MultiLanguageString(String.Empty, LocalInflectionLanguageIDs),
                    new MultiLanguageString(String.Empty, LocalInflectionLanguageIDs),
                    LexicalCategory.Verb,
                    inflected.CategoryString,
                    true,
                    String.Empty);
                returnValue = ConjugateSpecialRecursive(
                    designation, VerbClass.Ichidan,
                    englishStems, inflected, newInflected);
            }

            if (returnValue)
            {
                Mood mood = GetMood(designation);
                Mood subMood;
                Tense tense = GetTense(designation);
                Polarity polarity = GetPolarity(designation);
                Politeness politeness = GetPoliteness(designation);

                englishInflected = inflected.Output.Text(EnglishID);

                switch (mood)
                {
                    case Mood.Unknown:
                        Special special = GetSpecial2(designation);
                        switch (special)
                        {
                            case Special.Dictionary:
                            case Special.Stem:
                                englishInflected = englishStems.PresentParticiple + " is done";
                                break;
                            case Special.Participle:
                                englishInflected += " is done";
                                break;
                            case Special.Perfective:
                                englishInflected = englishStems.PresentParticiple + " has been done";
                                break;
                            case Special.Infinitive:
                                englishInflected += " is done";
                                break;
                            case Special.Potential:
                            case Special.Passive:
                            case Special.Causitive:
                            case Special.Honorific1:
                            case Special.Honorific2:
                            case Special.Humble1:
                            case Special.Humble2:
                            default:
                                switch (tense)
                                {
                                    case Tense.Present:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = englishStems.PresentParticiple + " is done";
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = englishStems.PresentParticiple + " is not done";
                                                break;
                                        }
                                        break;
                                    case Tense.Past:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = englishStems.PresentParticiple + " was done";
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = englishStems.PresentParticiple + " was not done";
                                                break;
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                    case Mood.Indicative:
                    case Mood.Recursive:
                    default:
                        switch (tense)
                        {
                            case Tense.Present:
                            default:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = englishStems.PresentParticiple + " is done";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = englishStems.PresentParticiple + " is not done";
                                        break;
                                }
                                break;
                            case Tense.Past:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = englishStems.PresentParticiple + " was done";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = englishStems.PresentParticiple + " was not done";
                                        break;
                                }
                                break;
                        }
                        break;
                    case Mood.Volitional:
                        switch (polarity)
                        {
                            case Polarity.Positive:
                            default:
                                englishInflected = englishStems.PresentParticiple + " will be done";
                                break;
                            case Polarity.Negative:
                                englishInflected = englishStems.PresentParticiple + " will not be done";
                                break;
                        }
                        break;
                    case Mood.Presumptive:
                        switch (tense)
                        {
                            case Tense.Present:
                            default:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = englishStems.PresentParticiple + " will probably be done";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = englishStems.PresentParticiple + " will probably not be done";
                                        break;
                                }
                                break;
                            case Tense.Past:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = englishStems.PresentParticiple + " will probably have been done";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = englishStems.PresentParticiple + " will probably not have been done";
                                        break;
                                }
                                break;
                        }
                        break;
                    case Mood.ContinuativeParticiple:
                    case Mood.ContinuativeInfinitive:
                        switch (polarity)
                        {
                            case Polarity.Positive:
                            default:
                                englishInflected = englishStems.PresentParticiple + " is done and";
                                break;
                            case Polarity.Negative:
                                englishInflected = englishStems.PresentParticiple + " is not done and";
                                break;
                        }
                        break;
                    case Mood.Progressive:
                        subMood = GetSubMood(designation);
                        switch (subMood)
                        {
                            case Mood.Indicative:
                            case Mood.Unknown:
                            case Mood.Recursive:
                            default:
                                switch (tense)
                                {
                                    case Tense.Present:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = englishStems.PresentParticiple + " is being done";
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = englishStems.PresentParticiple + " is not being done";
                                                break;
                                        }
                                        break;
                                    case Tense.Past:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = englishStems.PresentParticiple + " was being done";
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = englishStems.PresentParticiple + " was not being done";
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.Volitional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = englishStems.PresentParticiple + " will be being done";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = englishStems.PresentParticiple + " will not be being done";
                                        break;
                                }
                                break;
                            case Mood.Presumptive:
                                switch (tense)
                                {
                                    case Tense.Present:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = englishStems.PresentParticiple + " will probably be being done";
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = englishStems.PresentParticiple + " will probably not be being done";
                                                break;
                                        }
                                        break;
                                    case Tense.Past:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = englishStems.PresentParticiple + " will probably have been being done";
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = englishStems.PresentParticiple + " will probably not have been being done";
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.ContinuativeParticiple:
                            case Mood.ContinuativeInfinitive:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = englishStems.PresentParticiple + " is being done and";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = englishStems.PresentParticiple + " is not being done and";
                                        break;
                                }
                                break;
                            case Mood.Imperative:
                                switch (politeness)
                                {
                                    case Politeness.Abrupt:
                                    case Politeness.Plain:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "be done being " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "do not be done being " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                    case Politeness.Polite:
                                    case Politeness.Honorific:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "please be done being " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "please do not be done being " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.Request:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "please be done being " + englishStems.PresentParticiple;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "please do not be done being " + englishStems.PresentParticiple;
                                        break;
                                }
                                break;
                            case Mood.Provisional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "if " + englishStems.PresentParticiple + " is being done";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "if " + englishStems.PresentParticiple + " is not being done";
                                        break;
                                }
                                break;
                            case Mood.Conditional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "when " + englishStems.PresentParticiple + " is being done";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "when " + englishStems.PresentParticiple + " is not being done";
                                        break;
                                }
                                break;
                            case Mood.Alternative:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "things like " + englishStems.PresentParticiple + " be bring done and";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "things like " + englishStems.PresentParticiple + " are not being done and";
                                        break;
                                }
                                break;
                        }
                        break;
                    case Mood.Unintentional:
                        subMood = GetSubMood(designation);
                        switch (subMood)
                        {
                            case Mood.Indicative:
                            case Mood.Unknown:
                            case Mood.Recursive:
                            default:
                                switch (tense)
                                {
                                    case Tense.Present:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = englishStems.PresentParticiple + " is being unintentionally done";
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = englishStems.PresentParticiple + " is not being unintentionally done";
                                                break;
                                        }
                                        break;
                                    case Tense.Past:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = englishStems.PresentParticiple + " was being unintentionally done";
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = englishStems.PresentParticiple + " was not being unintentionally done";
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.Volitional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = englishStems.PresentParticiple + " will be being unintentionally done";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = englishStems.PresentParticiple + " will not be being unintentionally done";
                                        break;
                                }
                                break;
                            case Mood.Presumptive:
                                switch (tense)
                                {
                                    case Tense.Present:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = englishStems.PresentParticiple + " will probably be being unintentionally done";
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = englishStems.PresentParticiple + " will probably not be being unintentionally done";
                                                break;
                                        }
                                        break;
                                    case Tense.Past:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = englishStems.PresentParticiple + " will probably have been being unintentionally done";
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = englishStems.PresentParticiple + " will probably not have been being unintentionally done";
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.ContinuativeParticiple:
                            case Mood.ContinuativeInfinitive:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = englishStems.PresentParticiple + " is being unintentionally done and";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = englishStems.PresentParticiple + " is not being unintentionally done and";
                                        break;
                                }
                                break;
                            case Mood.Imperative:
                                switch (politeness)
                                {
                                    case Politeness.Abrupt:
                                    case Politeness.Plain:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "be done being unintentionally " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "do not be done being unintentionally " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                    case Politeness.Polite:
                                    case Politeness.Honorific:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "please be done being unintentionally " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "please do not be done being unintentionally " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.Request:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "please be done being unintentionally " + englishStems.PresentParticiple;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "please do not be done being unintentionally " + englishStems.PresentParticiple;
                                        break;
                                }
                                break;
                            case Mood.Provisional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "if " + englishStems.PresentParticiple + " is being unintentionally done";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "if " + englishStems.PresentParticiple + " is not being unintentionally done";
                                        break;
                                }
                                break;
                            case Mood.Conditional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "when " + englishStems.PresentParticiple + " is being unintentionally done";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "when " + englishStems.PresentParticiple + " is not being unintentionally done";
                                        break;
                                }
                                break;
                            case Mood.Alternative:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "things like " + englishStems.PresentParticiple + " be being unintentionally done and";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "things like " + englishStems.PresentParticiple + " are not being unintentionally done and";
                                        break;
                                }
                                break;
                        }
                        break;
                    case Mood.Probative:
                        subMood = GetSubMood(designation);
                        switch (subMood)
                        {
                            case Mood.Indicative:
                            case Mood.Unknown:
                            case Mood.Recursive:
                            default:
                                switch (tense)
                                {
                                    case Tense.Present:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = englishStems.PresentParticiple + " is being tried";
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = englishStems.PresentParticiple + " is not being tried";
                                                break;
                                        }
                                        break;
                                    case Tense.Past:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = englishStems.PresentParticiple + " was being tried";
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = englishStems.PresentParticiple + " was not being tried";
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.Volitional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = englishStems.PresentParticiple + " will be being tried";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = englishStems.PresentParticiple + " will not be being tried";
                                        break;
                                }
                                break;
                            case Mood.Presumptive:
                                switch (tense)
                                {
                                    case Tense.Present:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = englishStems.PresentParticiple + " will probably be being tried";
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = englishStems.PresentParticiple + " will probably not be being tried";
                                                break;
                                        }
                                        break;
                                    case Tense.Past:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = englishStems.PresentParticiple + " will probably have been being tried";
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = englishStems.PresentParticiple + " will probably not have been being tried";
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.ContinuativeParticiple:
                            case Mood.ContinuativeInfinitive:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = englishStems.PresentParticiple + " is being tried and";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = englishStems.PresentParticiple + " is not being tried and";
                                        break;
                                }
                                break;
                            case Mood.Imperative:
                                switch (politeness)
                                {
                                    case Politeness.Abrupt:
                                    case Politeness.Plain:
                                    default:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "be trying to " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "do not be trying to " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                    case Politeness.Polite:
                                    case Politeness.Honorific:
                                        switch (polarity)
                                        {
                                            case Polarity.Positive:
                                            default:
                                                englishInflected = "please be trying to " + englishStems.PresentParticiple;
                                                break;
                                            case Polarity.Negative:
                                                englishInflected = "please do not be trying to " + englishStems.PresentParticiple;
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case Mood.Request:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "please be trying to " + englishStems.PresentParticiple;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "please do not be trying to " + englishStems.PresentParticiple;
                                        break;
                                }
                                break;
                            case Mood.Provisional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "if " + englishStems.PresentParticiple + " is being tried";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "if " + englishStems.PresentParticiple + " is not being tried";
                                        break;
                                }
                                break;
                            case Mood.Conditional:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "when " + englishStems.PresentParticiple + " is being tried";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "when " + englishStems.PresentParticiple + " is not being tried";
                                        break;
                                }
                                break;
                            case Mood.Alternative:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "things like " + englishStems.PresentParticiple + " be being tried and";
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "things like " + englishStems.PresentParticiple + " are not being tried and";
                                        break;
                                }
                                break;
                        }
                        break;
                    case Mood.Imperative:
                        switch (politeness)
                        {
                            case Politeness.Abrupt:
                            case Politeness.Plain:
                            default:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "be done " + englishStems.PresentParticiple;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "do not be done " + englishStems.PresentParticiple;
                                        break;
                                }
                                break;
                            case Politeness.Polite:
                            case Politeness.Honorific:
                                switch (polarity)
                                {
                                    case Polarity.Positive:
                                    default:
                                        englishInflected = "please be done " + englishStems.PresentParticiple;
                                        break;
                                    case Polarity.Negative:
                                        englishInflected = "please do not be done " + englishStems.PresentParticiple;
                                        break;
                                }
                                break;
                        }
                        break;
                    case Mood.Request:
                        switch (polarity)
                        {
                            case Polarity.Positive:
                            default:
                                englishInflected = "please be done " + englishStems.PresentParticiple;
                                break;
                            case Polarity.Negative:
                                englishInflected = "please do not be done " + englishStems.PresentParticiple;
                                break;
                        }
                        break;
                    case Mood.Provisional:
                        switch (polarity)
                        {
                            case Polarity.Positive:
                            default:
                                englishInflected = "if " + englishStems.PresentParticiple + " is done";
                                break;
                            case Polarity.Negative:
                                englishInflected = "if " + englishStems.PresentParticiple + " is not done";
                                break;
                        }
                        break;
                    case Mood.Conditional:
                        switch (polarity)
                        {
                            case Polarity.Positive:
                            default:
                                englishInflected = "when " + englishStems.PresentParticiple + " is done";
                                break;
                            case Polarity.Negative:
                                englishInflected = "when " + englishStems.PresentParticiple + " is not done";
                                break;
                        }
                        break;
                    case Mood.Alternative:
                        switch (polarity)
                        {
                            case Polarity.Positive:
                            default:
                                englishInflected = "things like " + englishStems.PresentParticiple + " be done and";
                                break;
                            case Polarity.Negative:
                                englishInflected = "things like " + englishStems.PresentParticiple + " are not done and";
                                break;
                        }
                        break;
                }

                inflected.Output.SetText(EnglishID, englishInflected);
            }

            return returnValue;
        }

        protected bool ConjugateCausitive(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string aStem = japaneseStems.AStem;
            string eStem = japaneseStems.EStem;
            string iStem = japaneseStems.IStem;
            string oStem = japaneseStems.OStem;
            string uStem = japaneseStems.UStem;
            string taStem = japaneseStems.TAStem;
            string teStem = japaneseStems.TEStem;
            string ending = string.Empty;
            string englishInflected = englishStems.Root;
            bool returnValue = true;

            switch (verbClass)
            {
                case VerbClass.Ichidan:
                case VerbClass.Kureru:
                case VerbClass.Iru:
                    ending = "させる";
                    break;
                case VerbClass.Godan:
                case VerbClass.Yondan:
                    ending = aStem + "せる";
                    break;
                case VerbClass.Suru:
                    stem = stem.Substring(0, stem.Length - 1) + "させる";
                    kanaStem = kanaStem.Substring(0, kanaStem.Length - 1) + "させる";
                    romajiStem = romajiStem.Substring(0, romajiStem.Length - 2) + "saseru";
                    break;
                case VerbClass.Kuru:
                    ReplaceEndingKuToKo(ref stem, ref kanaStem, ref romajiStem);
                    ending = "させる";
                    break;
                case VerbClass.Aru:
                    ending = aStem + "せる";
                    break;
                default:
                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                    returnValue = false;
                    break;
            }

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            if (returnValue)
            {
                Inflection newInflected = new Inflection(
                    String.Empty,
                    null,
                    designation,
                    new MultiLanguageString(String.Empty, LocalInflectionLanguageIDs),
                    new MultiLanguageString(String.Empty, LocalInflectionLanguageIDs),
                    new MultiLanguageString(inflected.Output),
                    new MultiLanguageString(String.Empty, LocalInflectionLanguageIDs),
                    new MultiLanguageString(String.Empty, LocalInflectionLanguageIDs),
                    new MultiLanguageString(String.Empty, LocalInflectionLanguageIDs),
                    LexicalCategory.Verb,
                    inflected.CategoryString,
                    true,
                    String.Empty);
                returnValue = ConjugateSpecialRecursive(
                    designation, VerbClass.Ichidan,
                    englishStems, inflected, newInflected);
            }

            if (returnValue)
            {
                Mood mood = GetMood(designation);
                Mood subMood;
                Tense tense = GetTense(designation);
                Polarity polarity = GetPolarity(designation);
                Politeness politeness = GetPoliteness(designation);
                bool isPassive = designation.HasClassificationWith("Special", "Passive");
                englishInflected = inflected.Output.Text(EnglishID);

                if (!isPassive)
                {
                    switch (mood)
                    {
                        case Mood.Unknown:
                            Special special = GetSpecial2(designation);
                            switch (special)
                            {
                                case Special.Dictionary:
                                case Special.Stem:
                                case Special.Participle:
                                    englishInflected = "make someone " + englishInflected;
                                    break;
                                case Special.Perfective:
                                    englishInflected = "make someone to have " + englishInflected;
                                    break;
                                case Special.Infinitive:
                                    englishInflected = "make someone " + englishInflected;
                                    break;
                                case Special.Potential:
                                case Special.Passive:
                                case Special.Causitive:
                                case Special.Honorific1:
                                case Special.Honorific2:
                                case Special.Humble1:
                                case Special.Humble2:
                                default:
                                    switch (tense)
                                    {
                                        case Tense.Present:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "makes/lets/will make/will let (someone) " + englishStems.Root;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "does/will not make/let (someone) " + englishStems.Root;
                                                    break;
                                            }
                                            break;
                                        case Tense.Past:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "did make/let (someone) " + englishStems.Root;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "did not make/let (someone) " + englishStems.Root;
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case Mood.Indicative:
                        case Mood.Recursive:
                        default:
                            switch (tense)
                            {
                                case Tense.Present:
                                default:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "makes/lets/will make/will let (someone) " + englishStems.Root;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "does/will not make/let (someone) " + englishStems.Root;
                                            break;
                                    }
                                    break;
                                case Tense.Past:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "did make/let (someone) " + englishStems.Root;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "did not make/let (someone) " + englishStems.Root;
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case Mood.Volitional:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                default:
                                    englishInflected = "will make/let (someone) " + englishStems.Root;
                                    break;
                                case Polarity.Negative:
                                    englishInflected = "will not make/let (someone) " + englishStems.Root;
                                    break;
                            }
                            break;
                        case Mood.Presumptive:
                            switch (tense)
                            {
                                case Tense.Present:
                                default:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "will probably make/let (someone) " + englishStems.Root;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "will probably not make/let (someone) " + englishStems.Root;
                                            break;
                                    }
                                    break;
                                case Tense.Past:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "will probably have made/let (someone) " + englishStems.PastParticiple;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "will probably not have made/let (someone) " + englishStems.PastParticiple;
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case Mood.ContinuativeParticiple:
                        case Mood.ContinuativeInfinitive:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                default:
                                    englishInflected = "makes/lets/will make/will let (someone) " + englishStems.Root + " and";
                                    break;
                                case Polarity.Negative:
                                    englishInflected = "does/will not make/let (someone) " + englishStems.Root + " and";
                                    break;
                            }
                            break;
                        case Mood.Progressive:
                            subMood = GetSubMood(designation);
                            switch (subMood)
                            {
                                case Mood.Indicative:
                                case Mood.Recursive:
                                case Mood.Unknown:
                                default:
                                    switch (tense)
                                    {
                                        case Tense.Present:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "makes/lets/will make/will let (someone) be " + englishStems.PresentParticiple;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "does/will not make/let (someone) be " + englishStems.PresentParticiple;
                                                    break;
                                            }
                                            break;
                                        case Tense.Past:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "did make/let (someone) be " + englishStems.PresentParticiple;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "did not make/let (someone) be " + englishStems.PresentParticiple;
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.Volitional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "will make/let (someone) be " + englishStems.PresentParticiple;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "will not make/let (someone) be " + englishStems.PresentParticiple;
                                            break;
                                    }
                                    break;
                                case Mood.Presumptive:
                                    switch (tense)
                                    {
                                        case Tense.Present:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "will probably make/let (someone) be " + englishStems.PresentParticiple;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "will probably not make/let (someone) be " + englishStems.PresentParticiple;
                                                    break;
                                            }
                                            break;
                                        case Tense.Past:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "will probably have made/let (someone) be " + englishStems.PresentParticiple;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "will probably not have made/let (someone) be " + englishStems.PresentParticiple;
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.ContinuativeParticiple:
                                case Mood.ContinuativeInfinitive:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "makes/lets/will make/will let (someone) be " + englishStems.PresentParticiple + " and";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "does/will not make/let (someone) be " + englishStems.PresentParticiple + " and";
                                            break;
                                    }
                                    break;
                                case Mood.Imperative:
                                    switch (politeness)
                                    {
                                        case Politeness.Abrupt:
                                        case Politeness.Plain:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "make/let (someone) be " + englishStems.PresentParticiple;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "do not make/let (someone) be " + englishStems.PresentParticiple;
                                                    break;
                                            }
                                            break;
                                        case Politeness.Polite:
                                        case Politeness.Honorific:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "please make/let (someone) be " + englishStems.PresentParticiple;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "please do not make/let (someone) be " + englishStems.PresentParticiple;
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.Request:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "please make/let (someone) be " + englishStems.PresentParticiple;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "please do not make/let (someone) be " + englishStems.PresentParticiple;
                                            break;
                                    }
                                    break;
                                case Mood.Provisional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "if X is " + englishStems.PresentParticiple;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "if X is not " + englishStems.PresentParticiple;
                                            break;
                                    }
                                    break;
                                case Mood.Conditional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "if X were to be/when X is " + englishStems.PresentParticiple;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "if X were not /when X is not " + englishStems.PresentParticiple;
                                            break;
                                    }
                                    break;
                                case Mood.Alternative:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "doing/things like " + englishStems.PresentParticiple + " and";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "not doing/not things like " + englishStems.PresentParticiple + " and";
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case Mood.Unintentional:
                            subMood = GetSubMood(designation);
                            switch (subMood)
                            {
                                case Mood.Indicative:
                                case Mood.Recursive:
                                case Mood.Unknown:
                                default:
                                    switch (tense)
                                    {
                                        case Tense.Present:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "makes/lets/will make/will let (someone) be unintentionally " + englishStems.PresentParticiple;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "does/will not make/let (someone) be unintentionally " + englishStems.PresentParticiple;
                                                    break;
                                            }
                                            break;
                                        case Tense.Past:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "did make/let (someone) be unintentionally " + englishStems.PresentParticiple;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "did not make/let (someone) be unintentionally " + englishStems.PresentParticiple;
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.Volitional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "will make/let (someone) be unintentionally " + englishStems.PresentParticiple;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "will not make/let (someone) be unintentionally " + englishStems.PresentParticiple;
                                            break;
                                    }
                                    break;
                                case Mood.Presumptive:
                                    switch (tense)
                                    {
                                        case Tense.Present:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "will probably make/let (someone) be unintentionally " + englishStems.PresentParticiple;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "will probably not make/let (someone) be unintentionally " + englishStems.PresentParticiple;
                                                    break;
                                            }
                                            break;
                                        case Tense.Past:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "will probably have made/let (someone) be unintentionally " + englishStems.PresentParticiple;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "will probably not have made/let (someone) be unintentionally " + englishStems.PresentParticiple;
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.ContinuativeParticiple:
                                case Mood.ContinuativeInfinitive:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "makes/lets/will make/will let (someone) be unintentionally " + englishStems.PresentParticiple + " and";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "does/will not make/let (someone) be unintentionally " + englishStems.PresentParticiple + " and";
                                            break;
                                    }
                                    break;
                                case Mood.Imperative:
                                    switch (politeness)
                                    {
                                        case Politeness.Abrupt:
                                        case Politeness.Plain:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "make/let (someone) be unintentionally " + englishStems.PresentParticiple;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "do not make/let (someone) be unintentionally " + englishStems.PresentParticiple;
                                                    break;
                                            }
                                            break;
                                        case Politeness.Polite:
                                        case Politeness.Honorific:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "please make/let (someone) be unintentionally " + englishStems.PresentParticiple;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "please do not make/let (someone) be unintentionally " + englishStems.PresentParticiple;
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.Request:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "please make/let (someone) be unintentionally " + englishStems.PresentParticiple;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "please do not make/let (someone) be unintentionally " + englishStems.PresentParticiple;
                                            break;
                                    }
                                    break;
                                case Mood.Provisional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "if X is unintentionally " + englishStems.PresentParticiple;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "if X is not unintentionally " + englishStems.PresentParticiple;
                                            break;
                                    }
                                    break;
                                case Mood.Conditional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "if X were to be/when X is unintentionally " + englishStems.PresentParticiple;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "if X were not /when X is not unintentionally " + englishStems.PresentParticiple;
                                            break;
                                    }
                                    break;
                                case Mood.Alternative:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "doing/things like unintentionally " + englishStems.PresentParticiple + " and";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "not doing/not things like unintentionally " + englishStems.PresentParticiple + " and";
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case Mood.Probative:
                            subMood = GetSubMood(designation);
                            switch (subMood)
                            {
                                case Mood.Indicative:
                                case Mood.Recursive:
                                case Mood.Unknown:
                                default:
                                    switch (tense)
                                    {
                                        case Tense.Present:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "makes/lets/will make/will let (someone) try to " + englishStems.Root;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "does/will not make/let (someone) try to " + englishStems.Root;
                                                    break;
                                            }
                                            break;
                                        case Tense.Past:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "did make/let (someone) try to " + englishStems.Root;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "did not make/let (someone) try to " + englishStems.Root;
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.Volitional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "will make/let (someone) try to " + englishStems.Root;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "will not make/let (someone) try to " + englishStems.Root;
                                            break;
                                    }
                                    break;
                                case Mood.Presumptive:
                                    switch (tense)
                                    {
                                        case Tense.Present:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "will probably make/let (someone) try to " + englishStems.Root;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "will probably not make/let (someone) try to " + englishStems.Root;
                                                    break;
                                            }
                                            break;
                                        case Tense.Past:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "will probably have made/let (someone) try to " + englishStems.Root;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "will probably not have made/let (someone) try to " + englishStems.Root;
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.ContinuativeParticiple:
                                case Mood.ContinuativeInfinitive:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "makes/lets/will make/will let (someone) try to " + englishStems.Root + " and";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "does/will not make/let (someone) try to " + englishStems.Root + " and";
                                            break;
                                    }
                                    break;
                                case Mood.Imperative:
                                    switch (politeness)
                                    {
                                        case Politeness.Abrupt:
                                        case Politeness.Plain:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "make/let (someone) try to " + englishStems.Root;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "do not make/let (someone) try to " + englishStems.Root;
                                                    break;
                                            }
                                            break;
                                        case Politeness.Polite:
                                        case Politeness.Honorific:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "please make/let (someone) try to " + englishStems.Root;
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "please do not make/let (someone) try to " + englishStems.Root;
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.Request:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "please make/let (someone) try to " + englishStems.Root;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "please do not make/let (someone) try to " + englishStems.Root;
                                            break;
                                    }
                                    break;
                                case Mood.Provisional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "if X makes/lets someone try to " + englishStems.Root;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "if X does not make/let someone try to " + englishStems.Root;
                                            break;
                                    }
                                    break;
                                case Mood.Conditional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "if X were to make/let someone try to//when X makes/lets someone try to " + englishStems.Root;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "if X were not to make/let someone try to//when X does not make/let someone try to " + englishStems.Root;
                                            break;
                                    }
                                    break;
                                case Mood.Alternative:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "trying to/things like to try to " + englishStems.Root + " and";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "not trying to/not things like to not try to " + englishStems.Root + " and";
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case Mood.Imperative:
                            switch (politeness)
                            {
                                case Politeness.Abrupt:
                                case Politeness.Plain:
                                default:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "make/let (someone) " + englishStems.Root;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "do not make/let (someone) " + englishStems.Root;
                                            break;
                                    }
                                    break;
                                case Politeness.Polite:
                                case Politeness.Honorific:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "please make/let (someone) " + englishStems.Root;
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "please do not make/let (someone) " + englishStems.Root;
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case Mood.Request:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                default:
                                    englishInflected = "please make/let (someone) " + englishStems.Root;
                                    break;
                                case Polarity.Negative:
                                    englishInflected = "please do not make/let (someone) " + englishStems.Root;
                                    break;
                            }
                            break;
                        case Mood.Provisional:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                default:
                                    englishInflected = "if X does " + englishStems.Root;
                                    break;
                                case Polarity.Negative:
                                    englishInflected = "if X does not " + englishStems.Root;
                                    break;
                            }
                            break;
                        case Mood.Conditional:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                default:
                                    englishInflected = "if X were to/when X does " + englishStems.Root;
                                    break;
                                case Polarity.Negative:
                                    englishInflected = "if X were not to/when X does not " + englishStems.Root;
                                    break;
                            }
                            break;
                        case Mood.Alternative:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                default:
                                    englishInflected = "doing/things like " + englishStems.PresentParticiple + " and";
                                    break;
                                case Polarity.Negative:
                                    englishInflected = "not doing/not things like " + englishStems.PresentParticiple + " and";
                                    break;
                            }
                            break;
                    }
                }
                else
                {
                    switch (mood)
                    {
                        case Mood.Unknown:
                            Special special = GetSpecial2(designation);
                            switch (special)
                            {
                                case Special.Dictionary:
                                case Special.Stem:
                                case Special.Participle:
                                    englishInflected = "make someone " + englishInflected;
                                    break;
                                case Special.Perfective:
                                    englishInflected = "make someone to have " + englishInflected;
                                    break;
                                case Special.Infinitive:
                                    englishInflected = "make someone " + englishInflected;
                                    break;
                                case Special.Potential:
                                case Special.Passive:
                                case Special.Causitive:
                                case Special.Honorific1:
                                case Special.Honorific2:
                                case Special.Humble1:
                                case Special.Humble2:
                                default:
                                    switch (tense)
                                    {
                                        case Tense.Present:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "makes/lets/will make/will let " + englishStems.PresentParticiple + " be done";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "does/will not make/let " + englishStems.PresentParticiple + " be done";
                                                    break;
                                            }
                                            break;
                                        case Tense.Past:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "did make/let " + englishStems.PresentParticiple + " be done";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "did not make/let " + englishStems.PresentParticiple + " be done";
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case Mood.Indicative:
                        case Mood.Recursive:
                        default:
                            switch (tense)
                            {
                                case Tense.Present:
                                default:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "makes/lets/will make/will let " + englishStems.PresentParticiple + " be done";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "does/will not make/let " + englishStems.PresentParticiple + " be done";
                                            break;
                                    }
                                    break;
                                case Tense.Past:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "did make/let " + englishStems.PresentParticiple + " be done";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "did not make/let " + englishStems.PresentParticiple + " be done";
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case Mood.Volitional:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                default:
                                    englishInflected = "will make/let " + englishStems.PresentParticiple + " be done";
                                    break;
                                case Polarity.Negative:
                                    englishInflected = "will not make/let " + englishStems.PresentParticiple + " be done";
                                    break;
                            }
                            break;
                        case Mood.Presumptive:
                            switch (tense)
                            {
                                case Tense.Present:
                                default:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "will probably make/let " + englishStems.PresentParticiple + " be done";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "will probably not make/let " + englishStems.PresentParticiple + " be done";
                                            break;
                                    }
                                    break;
                                case Tense.Past:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "will probably have made/let " + englishStems.PastParticiple + " be done";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "will probably not have made/let " + englishStems.PastParticiple + " be done";
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case Mood.ContinuativeParticiple:
                        case Mood.ContinuativeInfinitive:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                default:
                                    englishInflected = "makes/lets/will make/will let " + englishStems.PresentParticiple + " be done and";
                                    break;
                                case Polarity.Negative:
                                    englishInflected = "does/will not make/let " + englishStems.PresentParticiple + " be done and";
                                    break;
                            }
                            break;
                        case Mood.Progressive:
                            subMood = GetSubMood(designation);
                            switch (subMood)
                            {
                                case Mood.Indicative:
                                case Mood.Recursive:
                                case Mood.Unknown:
                                default:
                                    switch (tense)
                                    {
                                        case Tense.Present:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "makes/lets/will make/will let " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "does/will not make/let " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                            }
                                            break;
                                        case Tense.Past:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "did make/let " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "did not make/let " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.Volitional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "will make/let " + englishStems.PresentParticiple + " be being done";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "will not make/let " + englishStems.PresentParticiple + " be being done";
                                            break;
                                    }
                                    break;
                                case Mood.Presumptive:
                                    switch (tense)
                                    {
                                        case Tense.Present:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "will probably make/let " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "will probably not make/let " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                            }
                                            break;
                                        case Tense.Past:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "will probably have made/let " + englishStems.PastParticiple + " be being done";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "will probably not have made/let " + englishStems.PastParticiple + " be being done";
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.ContinuativeParticiple:
                                case Mood.ContinuativeInfinitive:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "makes/lets/will make/will let " + englishStems.PresentParticiple + " be being done and";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "does/will not make/let " + englishStems.PresentParticiple + " be being done and";
                                            break;
                                    }
                                    break;
                                case Mood.Imperative:
                                    switch (politeness)
                                    {
                                        case Politeness.Abrupt:
                                        case Politeness.Plain:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "make/let " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "do not make/let " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                            }
                                            break;
                                        case Politeness.Polite:
                                        case Politeness.Honorific:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "please make/let " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "please do not make/let " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.Request:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "please make/let " + englishStems.PresentParticiple + " be being done";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "please do not make/let " + englishStems.PresentParticiple + " be being done";
                                            break;
                                    }
                                    break;
                                case Mood.Provisional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "if makes/lets " + englishStems.PresentParticiple + " be being done";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "if does not let/make " + englishStems.PresentParticiple + " be being done";
                                            break;
                                    }
                                    break;
                                case Mood.Conditional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "if/when " + englishStems.PresentParticiple + " were/was made/let to be done";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "if/when " + englishStems.PresentParticiple + " were/was not made/let to be done";
                                            break;
                                    }
                                    break;
                                case Mood.Alternative:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "doing/things like " + englishStems.PresentParticiple + " be being done and";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "not doing/not things like " + englishStems.PresentParticiple + " be being done and";
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case Mood.Unintentional:
                            subMood = GetSubMood(designation);
                            switch (subMood)
                            {
                                case Mood.Indicative:
                                case Mood.Recursive:
                                case Mood.Unknown:
                                default:
                                    switch (tense)
                                    {
                                        case Tense.Present:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "makes/lets/will make/will let unintentionally " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "does/will not make/let unintentionally " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                            }
                                            break;
                                        case Tense.Past:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "did make/let unintentionally " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "did not make/let unintentionally " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.Volitional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "will make/let unintentionally " + englishStems.PresentParticiple + " be being done";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "will not make/let unintentionally " + englishStems.PresentParticiple + " be being done";
                                            break;
                                    }
                                    break;
                                case Mood.Presumptive:
                                    switch (tense)
                                    {
                                        case Tense.Present:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "will probably make/let unintentionally " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "will probably not make/let unintentionally " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                            }
                                            break;
                                        case Tense.Past:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "will probably have made/let unintentionally " + englishStems.PastParticiple + " be being done";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "will probably not have made/let unintentionally " + englishStems.PastParticiple + " be being done";
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.ContinuativeParticiple:
                                case Mood.ContinuativeInfinitive:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "makes/lets/will make/will let unintentionally " + englishStems.PresentParticiple + " be being done and";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "does/will not make/let unintentionally " + englishStems.PresentParticiple + " be being done and";
                                            break;
                                    }
                                    break;
                                case Mood.Imperative:
                                    switch (politeness)
                                    {
                                        case Politeness.Abrupt:
                                        case Politeness.Plain:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "make/let unintentionally " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "do not make/let unintentionally " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                            }
                                            break;
                                        case Politeness.Polite:
                                        case Politeness.Honorific:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "please make/let unintentionally " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "please do not make/let unintentionally " + englishStems.PresentParticiple + " be being done";
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.Request:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "please make/let unintentionally " + englishStems.PresentParticiple + " be being done";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "please do not make/let unintentionally " + englishStems.PresentParticiple + " be being done";
                                            break;
                                    }
                                    break;
                                case Mood.Provisional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "if unintentionally " + englishStems.PresentParticiple + " is being done";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "if unintentionally " + englishStems.PresentParticiple + " is not being done";
                                            break;
                                    }
                                    break;
                                case Mood.Conditional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "if/when unintentionally " + englishStems.PresentParticiple + " were/was being done";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "if/when unintentionally " + englishStems.PresentParticiple + " were/was not being done";
                                            break;
                                    }
                                    break;
                                case Mood.Alternative:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "doing/things like unintentionally " + englishStems.PresentParticiple + " be being done and";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "not doing/not things like unintentionally " + englishStems.PresentParticiple + " be being done and";
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case Mood.Probative:
                            subMood = GetSubMood(designation);
                            switch (subMood)
                            {
                                case Mood.Indicative:
                                case Mood.Recursive:
                                case Mood.Unknown:
                                default:
                                    switch (tense)
                                    {
                                        case Tense.Present:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "makes/lets/will make/will let " + englishStems.PresentParticiple + " be tried";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "does/will not make/let " + englishStems.PresentParticiple + " be tried";
                                                    break;
                                            }
                                            break;
                                        case Tense.Past:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "did make/let " + englishStems.PresentParticiple + " be tried";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "did not make/let " + englishStems.PresentParticiple + " be tried";
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.Volitional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "will make/let " + englishStems.PresentParticiple + " be tried";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "will not make/let " + englishStems.PresentParticiple + " be tried";
                                            break;
                                    }
                                    break;
                                case Mood.Presumptive:
                                    switch (tense)
                                    {
                                        case Tense.Present:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "will probably make/let " + englishStems.PresentParticiple + " be tried";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "will probably not make/let " + englishStems.PresentParticiple + " be tried";
                                                    break;
                                            }
                                            break;
                                        case Tense.Past:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "will probably have made/let " + englishStems.PresentParticiple + " be tried";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "will probably not have made/let " + englishStems.PresentParticiple + " be tried";
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.ContinuativeParticiple:
                                case Mood.ContinuativeInfinitive:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "makes/lets/will make/will let " + englishStems.PresentParticiple + " be tried and";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "does/will not make/let " + englishStems.PresentParticiple + " be tried and";
                                            break;
                                    }
                                    break;
                                case Mood.Imperative:
                                    switch (politeness)
                                    {
                                        case Politeness.Abrupt:
                                        case Politeness.Plain:
                                        default:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "make/let " + englishStems.PresentParticiple + " be tried";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "do not make/let " + englishStems.PresentParticiple + " be tried";
                                                    break;
                                            }
                                            break;
                                        case Politeness.Polite:
                                        case Politeness.Honorific:
                                            switch (polarity)
                                            {
                                                case Polarity.Positive:
                                                default:
                                                    englishInflected = "please make/let " + englishStems.PresentParticiple + " be tried";
                                                    break;
                                                case Polarity.Negative:
                                                    englishInflected = "please do not make/let " + englishStems.PresentParticiple + " be tried";
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case Mood.Request:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "please make/let " + englishStems.PresentParticiple + " be tried";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "please do not make/let " + englishStems.PresentParticiple + " be tried";
                                            break;
                                    }
                                    break;
                                case Mood.Provisional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "if making/letting someone " + englishStems.Root + " is tried";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "if making/letting someone " + englishStems.Root + " is not tried";
                                            break;
                                    }
                                    break;
                                case Mood.Conditional:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "if/when making/letting someone " + englishStems.PresentParticiple + " be tried";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "if/when making/letting someone " + englishStems.PresentParticiple + " be tried";
                                            break;
                                    }
                                    break;
                                case Mood.Alternative:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "things like making/letting someone " + englishStems.PresentParticiple + " be tried and";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "not things like making/letting someone " + englishStems.PresentParticiple + " be being done and";
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case Mood.Imperative:
                            switch (politeness)
                            {
                                case Politeness.Abrupt:
                                case Politeness.Plain:
                                default:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "make/let " + englishStems.PresentParticiple + " be done";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "do not make/let " + englishStems.PresentParticiple + " be done";
                                            break;
                                    }
                                    break;
                                case Politeness.Polite:
                                case Politeness.Honorific:
                                    switch (polarity)
                                    {
                                        case Polarity.Positive:
                                        default:
                                            englishInflected = "please make/let " + englishStems.PresentParticiple + " be done";
                                            break;
                                        case Polarity.Negative:
                                            englishInflected = "please do not make/let " + englishStems.PresentParticiple + " be done";
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case Mood.Request:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                default:
                                    englishInflected = "please make/let " + englishStems.PresentParticiple + " be done";
                                    break;
                                case Polarity.Negative:
                                    englishInflected = "please do not make/let " + englishStems.PresentParticiple + " be done";
                                    break;
                            }
                            break;
                        case Mood.Provisional:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                default:
                                    englishInflected = "if " + englishStems.PresentParticiple + " is done";
                                    break;
                                case Polarity.Negative:
                                    englishInflected = "if " + englishStems.PresentParticiple + " is not done";
                                    break;
                            }
                            break;
                        case Mood.Conditional:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                default:
                                    englishInflected = "if/when " + englishStems.PresentParticiple + " were/was done";
                                    break;
                                case Polarity.Negative:
                                    englishInflected = "if/when " + englishStems.PresentParticiple + " were/was not done";
                                    break;
                            }
                            break;
                        case Mood.Alternative:
                            switch (polarity)
                            {
                                case Polarity.Positive:
                                default:
                                    englishInflected = "doing/things like " + englishStems.PresentParticiple + " be done and";
                                    break;
                                case Polarity.Negative:
                                    englishInflected = "not doing/not things like " + englishStems.PresentParticiple + " be done and";
                                    break;
                            }
                            break;
                    }
                }

                inflected.Output.SetText(EnglishID, englishInflected);
            }

            return returnValue;
        }

        protected bool ConjugateDesire(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string kanjiStem = japaneseStems.StemJapanese;
            string iStem = japaneseStems.IStem;
            string englishRoot = "wanted";
            Inflection newInflected = new Inflection(inflected);
            bool returnValue = true;

            switch (verbClass)
            {
                case VerbClass.Ichidan:
                case VerbClass.Godan:
                case VerbClass.Kureru:
                case VerbClass.Iru:
                case VerbClass.Aru:
                case VerbClass.Yondan:
                    kanjiStem += iStem;
                    kanaStem += iStem;
                    romajiStem += ConvertKanaCharacterToRomaji(iStem);
                    break;
                case VerbClass.Suru:
                    ReplaceEndingSuToShi(ref kanjiStem, ref kanaStem, ref romajiStem);
                    break;
                case VerbClass.Kuru:
                    ReplaceEndingKuToKi(ref kanjiStem, ref kanaStem, ref romajiStem);
                    break;
                default:
                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                    returnValue = false;
                    break;
            }

            kanjiStem += "たい";
            kanaStem += "たい";
            romajiStem += "tai";

            returnValue = DeclineIAdjective(
                    kanjiStem,
                    kanaStem,
                    romajiStem,
                    designation,
                    englishRoot,
                    newInflected) &&
                returnValue;

            inflected.Output = newInflected.Output;
            inflected.PrependToOutput(EnglishID, englishStems.PresentParticiple + " ");

            if (inflected.Output.Text(JapaneseID) == inflected.Output.Text(JapaneseKanaID))
                inflected.Root.SetText(JapaneseID, inflected.Root.Text(JapaneseKanaID));

            FixupSuffix(inflected);

            return returnValue;
        }

        protected bool ConjugateHonorific(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            MultiLanguageString output = inflected.Output;
            MultiLanguageString suffix = inflected.Suffix;
            MultiLanguageString prefix = inflected.Prefix;
            Special special = GetSpecial(designation);
            string newVerbKanji = String.Empty;
            string newVerbKana = String.Empty;
            string newVerbRomaji = String.Empty;
            VerbClass newVerbClass = VerbClass.Unknown;
            string newCategoryString = inflected.CategoryString;
            bool replaced = true;
            int versionCount = 1;
            string error = null;
            bool returnValue = true;

            switch (japaneseStems.DictionaryFormJapanese)
            {
                case "する":
                    newVerbKanji = "なさる";
                    newVerbKana = "なさる";
                    newVerbRomaji = "nasaru";
                    newVerbClass = VerbClass.Godan;
                    newCategoryString = "v5r";
                    break;
                case "くる":
                case "来る":
                    newVerbKanji = "いらっしゃる";
                    newVerbKana = "いらっしゃる";
                    newVerbRomaji = "irassharu";
                    newVerbClass = VerbClass.Godan;
                    newCategoryString = "v5aru";
                    break;
                case "行く":
                    newVerbKanji = "いらっしゃる";
                    newVerbKana = "いらっしゃる";
                    newVerbRomaji = "irassharu";
                    newVerbClass = VerbClass.Godan;
                    newCategoryString = "v5aru";
                    break;
                case "いる":
                    newVerbKanji = "いらっしゃる";
                    newVerbKana = "いらっしゃる";
                    newVerbRomaji = "irassharu";
                    newVerbClass = VerbClass.Godan;
                    newCategoryString = "v5aru";
                    break;
                case "食べる":
                    newVerbKanji = "召し上がる";
                    newVerbKana = "めしあがる";
                    newVerbRomaji = "meshiagaru";
                    newVerbClass = VerbClass.Godan;
                    newCategoryString = "v5aru";
                    break;
                case "飲む":
                    newVerbKanji = "召し上がる";
                    newVerbKana = "めしあがる";
                    newVerbRomaji = "meshiagaru";
                    newVerbClass = VerbClass.Godan;
                    newCategoryString = "v5r";
                    break;
                case "言う":
                    newVerbKanji = "おっしゃる";
                    newVerbKana = "おっしゃる";
                    newVerbRomaji = "ossharu";
                    newVerbClass = VerbClass.Godan;
                    newCategoryString = "v5r";
                    break;
                case "見る":
                    versionCount = 2;
                    switch (special)
                    {
                        case Special.Honorific1:
                        default:
                            newVerbKanji = "ご覧になる";
                            newVerbKana = "ごらんいなる";
                            newVerbRomaji = "goran ni naru";
                            newVerbClass = VerbClass.Godan;
                            newCategoryString = "v5r";
                            break;
                        case Special.Honorific2:
                            // iku
                            // (to go)
                            newVerbKanji = "ご覧なさる";
                            newVerbKana = "ごらんなさる";
                            newVerbRomaji = "goran nasaru";
                            newVerbClass = VerbClass.Godan;
                            newCategoryString = "v5r";
                            // ukagau
                            break;
                    }
                    break;
                case "知る":
                    newVerbKanji = "ご存知でいらっしゃる";
                    newVerbKana = "ごぞんじでいらっしゃる";
                    newVerbRomaji = "gozonji de irassharu";
                    newVerbClass = VerbClass.Godan;
                    newCategoryString = "v5aru";
                    break;
                case "ある":
                    newVerbKanji = "ござる";
                    newVerbKana = "ござる";
                    newVerbRomaji = "gozaru";
                    newVerbClass = VerbClass.Yondan;
                    newCategoryString = "v4r";
                    break;
                default:
                    replaced = false;
                    versionCount = 2;
                    break;
            }

            Designator newDesignation = new Designator(designation);

            newDesignation.DeleteClassification("Special", "Honorific1");
            newDesignation.DeleteClassification("Special", "Honorific2");

            if (!newDesignation.HasClassification("Mood"))
                newDesignation.InsertFirstClassification("Mood", "Indicative");

            if (!newDesignation.HasClassification("Tense"))
                newDesignation.InsertFirstClassification("Tense", "Present");

            if (replaced)
            {
                JapaneseStems newJapaneseStems = new JapaneseStems();
                EnglishStems newEnglishStems = new EnglishStems();
                Inflection newInflected = new Inflection(null, newDesignation, LocalInflectionLanguageIDs);
                MultiLanguageString newRoot = newInflected.Root;
                MultiLanguageString newDictionaryForm = newInflected.DictionaryForm;

                error = inflected.Error;

                if (GetSubVerbStems(
                        newVerbKanji,
                        newVerbKana,
                        newVerbRomaji,
                        englishStems.Root,
                        newVerbClass,
                        newJapaneseStems,
                        newRoot,
                        newDictionaryForm,
                        out error))
                {
                    returnValue = ConjugateRecurse(
                        newDesignation,
                        newJapaneseStems,
                        englishStems,
                        newVerbClass,
                        newInflected);
                    prefix.Copy(newInflected.Prefix);
                    root.Copy(newInflected.Root);
                    suffix.Copy(newInflected.Suffix);
                    output.Copy(newInflected.Output);
                    inflected.CategoryString = newCategoryString;
                }
            }
            else
            {
                Politeness politeness = GetPoliteness(designation);
                Polarity polarity = GetPolarity(designation);
                string kanaStem = "お" + root.Text(JapaneseKanaID);
                string stem = "お" + japaneseStems.StemJapanese;
                string iStem = japaneseStems.IStem;
                string romajiStem = "o" + root.Text(JapaneseRomajiID);
                string ending = iStem;
                string englishInflected = englishStems.PresentParticiple;

                root.SetText(JapaneseID, stem);
                root.SetText(JapaneseKanaID, kanaStem);
                root.SetText(JapaneseRomajiID, romajiStem);

                returnValue = CreateInflected(
                        stem,
                        kanaStem,
                        romajiStem,
                        ending,
                        verbClass,
                        englishInflected,
                        inflected) &&
                    returnValue;

                string newKanji;
                string newRomaji;
                string newKanjiStem;
                string newRomajiStem;

                switch (special)
                {
                    case Special.Honorific1:
                    default:
                        newKanji = "になる";
                        newRomaji = "ninaru";
                        newKanjiStem = "にな";
                        newRomajiStem = "nina";
                        newVerbClass = VerbClass.Godan;
                        break;
                    case Special.Honorific2:
                        newKanji = "なさる";
                        newRomaji = "nasaru";
                        newKanjiStem = "なさ";
                        newRomajiStem = "nasa";
                        newVerbClass = VerbClass.Godan;
                        break;
                }

                Inflection newInflected = new Inflection(null, newDesignation, LocalInflectionLanguageIDs);
                MultiLanguageString newRoot = newInflected.Root;
                MultiLanguageString newDictionaryForm = newInflected.DictionaryForm;

                SetText(newRoot, newKanjiStem, newKanjiStem, newRomajiStem, englishStems.Root);
                SetText(newDictionaryForm, newKanji, newKanji, newRomaji, englishStems.Root);

                JapaneseStems newJapaneseStems = new JapaneseStems(newKanji, newKanji, newRomaji, newKanjiStem, newKanjiStem, newRomajiStem);

                error = inflected.Error;

                returnValue = GetJapaneseVerbStems(
                    newKanji,
                    newVerbClass,
                    newJapaneseStems,
                    newRoot,
                    ref error) && returnValue;

                returnValue = ConjugateRecurse(
                    newDesignation,
                    newJapaneseStems,
                    englishStems,
                    newVerbClass,
                    newInflected) && returnValue;

                MultiLanguageString newOutput = newInflected.Output;

                output.SetText(JapaneseID, output.Text(JapaneseID) + newOutput.Text(JapaneseID));
                output.SetText(JapaneseKanaID, output.Text(JapaneseKanaID) + newOutput.Text(JapaneseKanaID));
                output.SetText(JapaneseRomajiID, output.Text(JapaneseRomajiID) + newOutput.Text(JapaneseRomajiID));
                output.SetText(EnglishID, newOutput.Text(EnglishID));

                suffix.SetText(JapaneseID, suffix.Text(JapaneseID) + newOutput.Text(JapaneseID));
                suffix.SetText(JapaneseKanaID, suffix.Text(JapaneseKanaID) + newOutput.Text(JapaneseKanaID));
                suffix.SetText(JapaneseRomajiID, suffix.Text(JapaneseRomajiID) + newOutput.Text(JapaneseRomajiID));
            }

            if (returnValue && (special == Special.Honorific2) && (versionCount == 1))
            {
                returnValue = false;
                inflected.Error = "Only one honorific form.";
            }

            return returnValue;
        }

        protected bool ConjugateHumble(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            MultiLanguageString output = inflected.Output;
            MultiLanguageString suffix = inflected.Suffix;
            MultiLanguageString prefix = inflected.Prefix;
            Special special = GetSpecial(designation);
            string newVerbKanji = String.Empty;
            string newVerbKana = String.Empty;
            string newVerbRomaji = String.Empty;
            VerbClass newVerbClass = VerbClass.Unknown;
            string newCategoryString = inflected.CategoryString;
            bool replaced = true;
            int versionCount = 1;
            string error = null;
            bool returnValue = true;

            switch (japaneseStems.DictionaryFormJapanese)
            {
                case "する":
                    // suru
                    // (to do)
                    newVerbKanji = "いたす";
                    newVerbKana = "いたす";
                    newVerbRomaji = "itasu";
                    newVerbClass = VerbClass.Godan;
                    newCategoryString = "v5s";
                    break;
                case "くる":
                case "来る":
                    // kuru
                    // (to come)
                    newVerbKanji = "参る";
                    newVerbKana = "まいる";
                    newVerbRomaji = "mairu";
                    newVerbClass = VerbClass.Godan;
                    newCategoryString = "v5r";
                    break;
                case "行く":
                    versionCount = 2;
                    switch (special)
                    {
                        case Special.Humble1:
                        default:
                            // iku
                            // (to go)
                            newVerbKanji = "参る";
                            newVerbKana = "まいる";
                            newVerbRomaji = "mairu";
                            newVerbClass = VerbClass.Godan;
                            newCategoryString = "v5r";
                            break;
                        case Special.Humble2:
                            // iku
                            // (to go)
                            newVerbKanji = "伺う";
                            newVerbKana = "うかがう";
                            newVerbRomaji = "ukagau";
                            newVerbClass = VerbClass.Godan;
                            newCategoryString = "v5u";
                            break;
                    }
                    break;
                case "いる":
                    // iru
                    // (to be, to stay)
                    //newVerbKanji = "居る";
                    newVerbKanji = "おる";
                    newVerbKana = "おる";
                    newVerbRomaji = "oru";
                    newVerbClass = VerbClass.Godan;
                    newCategoryString = "v5r";
                    break;
                case "食べる":
                    // taberu
                    // (to eat)
                    newVerbKanji = "いただく";
                    newVerbKana = "いただく";
                    newVerbRomaji = "itadaku";
                    newVerbClass = VerbClass.Godan;
                    newCategoryString = "v5k";
                    break;
                case "飲む":
                    // nomu
                    // (to drink)
                    newVerbKanji = "いただく";
                    newVerbKana = "いただく";
                    newVerbRomaji = "itadaku";
                    newVerbClass = VerbClass.Godan;
                    newCategoryString = "v5k";
                    break;
                case "言う":
                    versionCount = 2;
                    switch (special)
                    {
                        case Special.Humble1:
                        default:
                            // iu
                            // (to say)
                            newVerbKanji = "申す";
                            newVerbKana = "もうす";
                            newVerbRomaji = "mousu";
                            newVerbClass = VerbClass.Godan;
                            newCategoryString = "v5s";
                            break;
                        case Special.Humble2:
                            // iu
                            // (to say)
                            newVerbKanji = "申し上げる";
                            newVerbKana = "もうしあげる";
                            newVerbRomaji = "moushiageru";
                            newVerbClass = VerbClass.Ichidan;
                            newCategoryString = "v1";
                            break;
                    }
                    break;
                case "上げる":
                    // ageru
                    // (to give)
                    newVerbKanji = "差し上げる";
                    newVerbKana = "さしあげる";
                    newVerbRomaji = "sashiageru";
                    newVerbClass = VerbClass.Ichidan;
                    newCategoryString = "v1";
                    break;
                case "見る":
                    versionCount = 2;
                    switch (special)
                    {
                        case Special.Humble1:
                        default:
                            // miru
                            // (to see)
                            //newVerbKanji = "拝見する";
                            newVerbKanji = "拝見";
                            newVerbKana = "はいけん";
                            newVerbRomaji = "haiken";
                            newVerbClass = VerbClass.Suru;
                            newCategoryString = "vs";
                            break;
                        case Special.Humble2:
                            // miru
                            // (to see)
                            newVerbKanji = "拝見いたす";
                            newVerbKana = "はいけんいたす";
                            newVerbRomaji = "haikenitasu";
                            newVerbClass = VerbClass.Godan;
                            newCategoryString = "v5s";
                            break;
                    }
                    break;
                case "知る":
                    // shiru
                    // (to know)
                    newVerbKanji = "存じる";
                    newVerbKana = "ぞんじる";
                    newVerbRomaji = "zonjiru";
                    newVerbClass = VerbClass.Ichidan;
                    newCategoryString = "v1";
                    break;
                case "聞く":
                    versionCount = 4;
                    switch (special)
                    {
                        case Special.Humble1:
                        default:
                            // kiku
                            // (to ask, to hear, to listen)
                            newVerbKanji = "伺う";
                            newVerbKana = "うかかう";
                            newVerbRomaji = "ukagau";
                            newVerbClass = VerbClass.Godan;
                            newCategoryString = "v5u";
                            break;
                        case Special.Humble2:
                            // kiku
                            // (to ask, to hear, to listen)
                            newVerbKanji = "承る";
                            newVerbKana = "うけたまわる";
                            newVerbRomaji = "uketamawaru";
                            newVerbClass = VerbClass.Godan;
                            newCategoryString = "v5r";
                            break;
                        case Special.Humble3:
                            // kiku
                            // (to ask, to hear, to listen)
                            //newVerbKanji = "拝聴する";
                            newVerbKanji = "拝聴";
                            newVerbKana = "はいちょう";
                            newVerbRomaji = "haichou";
                            newVerbClass = VerbClass.Suru;
                            newCategoryString = "vs";
                            break;
                        case Special.Humble4:
                            // kiku
                            // (to ask, to hear, to listen)
                            newVerbKanji = "拝聴いたす";
                            newVerbKana = "はいちょういたす";
                            newVerbRomaji = "haichouitasu";
                            newVerbClass = VerbClass.Godan;
                            newCategoryString = "v5s";
                            break;
                    }
                    break;
                case "ある":
                    // aru
                    // (to be, to exist)
                    newVerbKanji = "ござる";
                    newVerbKana = "ござる";
                    newVerbRomaji = "gozaru";
                    newVerbClass = VerbClass.Yondan;
                    newCategoryString = "v4r";
                    break;
                default:
                    replaced = false;
                    versionCount = 2;
                    break;
            }

            Designator newDesignation = new Designator(designation);

            newDesignation.DeleteClassification("Special", "Humble1");
            newDesignation.DeleteClassification("Special", "Humble2");

            if (!newDesignation.HasClassification("Mood"))
                newDesignation.InsertFirstClassification("Mood", "Indicative");

            if (!newDesignation.HasClassification("Tense"))
                newDesignation.InsertFirstClassification("Tense", "Present");

            if (replaced)
            {
                JapaneseStems newJapaneseStems = new JapaneseStems();
                Inflection newInflected = new Inflection(null, newDesignation, LocalInflectionLanguageIDs);
                MultiLanguageString newRoot = newInflected.Root;
                MultiLanguageString newDictionaryForm = newInflected.DictionaryForm;

                error = inflected.Error;

                if (GetSubVerbStems(
                        newVerbKanji,
                        newVerbKana,
                        newVerbRomaji,
                        englishStems.Root,
                        newVerbClass,
                        newJapaneseStems,
                        newRoot,
                        newDictionaryForm,
                        out error))
                {
                    returnValue = ConjugateRecurse(
                        newDesignation,
                        newJapaneseStems,
                        englishStems,
                        newVerbClass,
                        newInflected);
                    prefix.Copy(newInflected.Prefix);
                    root.Copy(newInflected.Root);
                    suffix.Copy(newInflected.Suffix);
                    output.Copy(newInflected.Output);
                    inflected.CategoryString = newCategoryString;
                }
            }
            else
            {
                Politeness politeness = GetPoliteness(designation);
                Polarity polarity = GetPolarity(designation);
                string kanaStem = "お" + root.Text(JapaneseKanaID);
                string stem = "お" + japaneseStems.StemJapanese;
                string iStem = japaneseStems.IStem;
                string romajiStem = "o" + root.Text(JapaneseRomajiID);
                string ending = iStem;
                string englishInflected = englishStems.PresentParticiple;

                root.SetText(JapaneseID, stem);
                root.SetText(JapaneseKanaID, kanaStem);
                root.SetText(JapaneseRomajiID, romajiStem);

                returnValue = CreateInflected(
                        stem,
                        kanaStem,
                        romajiStem,
                        ending,
                        verbClass,
                        englishInflected,
                        inflected) &&
                    returnValue;

                string newKanji;
                string newRomaji;
                string newKanjiStem;
                string newRomajiStem;

                switch (special)
                {
                    case Special.Humble1:
                    default:
                        newKanji = "する";
                        newRomaji = "suru";
                        newKanjiStem = "す";
                        newRomajiStem = "su";
                        newVerbClass = VerbClass.Suru;
                        break;
                    case Special.Humble2:
                        newKanji = "いたす";
                        newRomaji = "itasu";
                        newKanjiStem = "いた";
                        newRomajiStem = "ita";
                        newVerbClass = VerbClass.Godan;
                        break;
                }

                Inflection newInflected = new Inflection(null, newDesignation, LocalInflectionLanguageIDs);
                MultiLanguageString newRoot = newInflected.Root;
                MultiLanguageString newDictionaryForm = newInflected.DictionaryForm;

                SetText(newRoot, newKanjiStem, newKanjiStem, newRomajiStem, englishStems.Root);
                SetText(newDictionaryForm, newKanji, newKanji, newRomaji, englishStems.Root);

                JapaneseStems newJapaneseStems = new JapaneseStems(newKanji, newKanji, newRomaji, newKanjiStem, newKanjiStem, newRomajiStem);

                error = inflected.Error;

                returnValue = GetJapaneseVerbStems(
                    newKanji,
                    newVerbClass,
                    newJapaneseStems,
                    newRoot,
                    ref error) && returnValue;

                returnValue = ConjugateRecurse(
                    newDesignation,
                    newJapaneseStems,
                    englishStems,
                    newVerbClass,
                    newInflected) && returnValue;

                MultiLanguageString newOutput = newInflected.Output;

                output.SetText(JapaneseID, output.Text(JapaneseID) + newOutput.Text(JapaneseID));
                output.SetText(JapaneseKanaID, output.Text(JapaneseKanaID) + newOutput.Text(JapaneseKanaID));
                output.SetText(JapaneseRomajiID, output.Text(JapaneseRomajiID) + newOutput.Text(JapaneseRomajiID));
                output.SetText(EnglishID, newOutput.Text(EnglishID));

                suffix.SetText(JapaneseID, suffix.Text(JapaneseID) + newOutput.Text(JapaneseID));
                suffix.SetText(JapaneseKanaID, suffix.Text(JapaneseKanaID) + newOutput.Text(JapaneseKanaID));
                suffix.SetText(JapaneseRomajiID, suffix.Text(JapaneseRomajiID) + newOutput.Text(JapaneseRomajiID));
            }

            if (returnValue)
            {
                switch (special)
                {
                    case Special.Humble1:
                        break;
                    case Special.Humble2:
                        if (versionCount < 2)
                        {
                            returnValue = false;
                            error = "Only " + versionCount.ToString() + " humble form.";
                        }
                        break;
                    case Special.Humble3:
                        if (versionCount < 3)
                        {
                            returnValue = false;
                            error = "Only " + versionCount.ToString() + " humble form.";
                        }
                        break;
                    case Special.Humble4:
                        if (versionCount < 4)
                        {
                            returnValue = false;
                            error = "Only " + versionCount.ToString() + " humble form.";
                        }
                        break;
                }
            }

            return returnValue;
        }

        protected bool ConjugateDictionary(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string ending = japaneseStems.UStem;
            string englishInflected = englishStems.Root;
            bool returnValue = CreateInflected(
                japaneseStems.StemJapanese,
                kanaStem,
                romajiStem,
                ending,
                verbClass,
                englishInflected,
                inflected);
            return returnValue;
        }

        protected bool ConjugateStem(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            string kanjiStem = root.Text(JapaneseID);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string ending = String.Empty;
            string englishInflected = englishStems.Root;

            switch (verbClass)
            {
                case VerbClass.Ichidan:
                case VerbClass.Godan:
                case VerbClass.Yondan:
                    break;
                case VerbClass.Suru:
                    if (japaneseStems.DictionaryFormKana != "する")
                    {
                        char lastKanji = kanjiStem[kanjiStem.Length - 1];
                        char lastKana = kanaStem[kanaStem.Length - 1];
                        if ((lastKana == 'す') || (lastKana == 'し'))
                        {
                            kanjiStem = kanjiStem.Substring(0, kanjiStem.Length - 1);
                            kanaStem = kanaStem.Substring(0, kanaStem.Length - 1);
                            if (lastKana == 'す')
                                romajiStem = romajiStem.Substring(0, romajiStem.Length - 2);
                            else
                                romajiStem = romajiStem.Substring(0, romajiStem.Length - 3);
                            root.SetText(JapaneseID, kanjiStem);
                            root.SetText(JapaneseKanaID, kanaStem);
                            root.SetText(JapaneseRomajiID, romajiStem);
                        }
                    }
                    break;
                case VerbClass.Kuru:
                    if (japaneseStems.DictionaryFormKana != "くる")
                    {
                        char lastKanji = kanjiStem[kanjiStem.Length - 1];
                        char lastKana = kanaStem[kanaStem.Length - 1];
                        if ((lastKana == 'く') || (lastKana == 'き') || (lastKana == 'こ'))
                        {
                            kanjiStem = kanjiStem.Substring(0, kanjiStem.Length - 1);
                            kanaStem = kanaStem.Substring(0, kanaStem.Length - 1);
                            romajiStem = romajiStem.Substring(0, romajiStem.Length - 2);
                            root.SetText(JapaneseID, kanjiStem);
                            root.SetText(JapaneseKanaID, kanaStem);
                            root.SetText(JapaneseRomajiID, romajiStem);
                        }
                    }
                    break;
                case VerbClass.Kureru:
                case VerbClass.Aru:
                case VerbClass.Iru:
                    break;
                default:
                    break;
            }

            bool returnValue = CreateInflected(
                kanjiStem,
                kanaStem,
                romajiStem,
                ending,
                verbClass,
                englishInflected,
                inflected);

            return returnValue;
        }

        protected bool ConjugateParticiple(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            MultiLanguageString output = inflected.Output;
            MultiLanguageString suffix = inflected.Suffix;
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string ending = japaneseStems.TEStem;
            string englishInflected = englishStems.PresentParticiple;
            bool returnValue = true;

            switch (verbClass)
            {
                case VerbClass.Ichidan:
                    break;
                case VerbClass.Godan:
                case VerbClass.Yondan:
                    break;
                case VerbClass.Suru:
                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                    break;
                case VerbClass.Kuru:
                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                    break;
                case VerbClass.Kureru:
                    break;
                case VerbClass.Aru:
                case VerbClass.Iru:
                    break;
                default:
                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                    returnValue = false;
                    break;
            }

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            switch (GetContraction(designation))
            {
                case Contraction.ContractionTeWaToCha:
                case Contraction.ContractionDeWaToJa:
                    if (RemoveTeEnding(output) && RemoveTeEnding(suffix))
                    {
                        AppendToOutput(output, "ちゃ", "ちゃ", "cha");
                        AppendToOutput(suffix, "ちゃ", "ちゃ", "cha");
                    }
                    else if (RemoveDeEnding(output) && RemoveDeEnding(suffix))
                    {
                        AppendToOutput(output, "じゃ", "じゃ", "ja");
                        AppendToOutput(suffix, "じゃ", "じゃ", "ja");
                    }
                    break;
                case Contraction.ContractionTeWaShinaiToTeYaShinai:
                    if (RemoveTeEnding(output) && RemoveTeEnding(suffix))
                    {
                        AppendToOutput(output, "てはしない", "てはしない", "teyashinai");
                        AppendToOutput(suffix, "てはしない", "てはしない", "teyashinai");
                    }
                    else if (RemoveDeEnding(output) && RemoveDeEnding(suffix))
                    {
                        AppendToOutput(output, "ではしない", "ではしない", "deyashinai");
                        AppendToOutput(suffix, "ではしない", "ではしない", "deyashinai");
                        returnValue = false;
                    }
                    else
                        returnValue = false;
                    output.SetText(EnglishID, "not doing " + output.Text(EnglishID));
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        protected bool ConjugatePerfective(Designator designation,
            JapaneseStems japaneseStems,
            EnglishStems englishStems, VerbClass verbClass, Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string stem = japaneseStems.StemJapanese;
            string ending = japaneseStems.TAStem;
            string englishInflected = englishStems.PastParticiple;
            bool returnValue = true;

            switch (verbClass)
            {
                case VerbClass.Ichidan:
                    break;
                case VerbClass.Godan:
                case VerbClass.Yondan:
                    break;
                case VerbClass.Suru:
                    ReplaceEndingSuToShi(ref stem, ref kanaStem, ref romajiStem);
                    break;
                case VerbClass.Kuru:
                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                    break;
                case VerbClass.Kureru:
                    break;
                case VerbClass.Aru:
                case VerbClass.Iru:
                    break;
                default:
                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                    returnValue = false;
                    break;
            }

            returnValue = CreateInflected(
                    stem,
                    kanaStem,
                    romajiStem,
                    ending,
                    verbClass,
                    englishInflected,
                    inflected) &&
                returnValue;

            return returnValue;
        }

        protected bool ConjugateInfinitive(Designator designation,
            JapaneseStems japaneseStems, EnglishStems englishStems, VerbClass verbClass,
            Inflection inflected)
        {
            MultiLanguageString root = inflected.Root;
            string stem = root.Text(JapaneseID);
            string kanaStem = root.Text(JapaneseKanaID);
            string romajiStem = root.Text(JapaneseRomajiID);
            string ending = japaneseStems.IStem;
            string englishInflected = "to " + englishStems.Root;
            bool returnValue = true;

            switch (verbClass)
            {
                case VerbClass.Ichidan:
                    break;
                case VerbClass.Godan:
                case VerbClass.Yondan:
                    break;
                case VerbClass.Suru:
                    returnValue = false;
                    break;
                case VerbClass.Kuru:
                    ReplaceEndingKuToKi(ref stem, ref kanaStem, ref romajiStem);
                    break;
                case VerbClass.Kureru:
                    break;
                case VerbClass.Aru:
                case VerbClass.Iru:
                    break;
                default:
                    inflected.Error = "Unexpected verb class: " + verbClass.ToString();
                    returnValue = false;
                    break;
            }

            returnValue = CreateInflected(
                japaneseStems.StemJapanese,
                kanaStem,
                romajiStem,
                ending,
                verbClass,
                englishInflected,
                inflected) && returnValue;

            return returnValue;
        }

        protected bool CreateInflected(string stem, string kanaStem, string romajiStem, string ending, VerbClass verbClass,
            string englishInflected, Inflection inflected)
        {
            bool returnValue = false;

            switch (verbClass)
            {
                case VerbClass.Ichidan:
                case VerbClass.Godan:
                case VerbClass.Yondan:
                    returnValue = SetInflectedOutput(
                        stem,
                        ending,
                        englishInflected,
                        inflected);
                    break;
                case VerbClass.Suru:
                case VerbClass.Kuru:
                case VerbClass.Kureru:
                case VerbClass.Aru:
                case VerbClass.Iru:
                    returnValue = SetIrregularInflectedOutput(
                        stem,
                        kanaStem,
                        romajiStem,
                        ending,
                        englishInflected,
                        inflected);
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        protected bool CreateIrregularInflected(string stem, string kanaStem, string romajiStem, string ending, VerbClass verbClass,
            string englishInflected, Inflection inflected)
        {
            bool returnValue = SetIrregularInflectedOutput(
                stem,
                kanaStem,
                romajiStem,
                ending,
                englishInflected,
                inflected);

            return returnValue;
        }

        protected bool GetVerbStems(
            DictionaryEntry dictionaryEntry,
            int senseIndex,
            int synonymIndex,
            out VerbClass verbClass,
            out string categoryString,
            JapaneseStems stems,
            EnglishStems englishStems,
            MultiLanguageString root,
            MultiLanguageString dictionaryForm,
            out string error)
        {
            Sense sense;
            int reading = 0;
            string kanji = String.Empty;
            string kanjiStem = String.Empty;
            string kana = String.Empty;
            string kanaStem = String.Empty;
            string romaji = String.Empty;
            string romajiStem = String.Empty;
            string lastRootSyllableRomaji = String.Empty;
            bool returnValue = true;

            error = String.Empty;

            returnValue = GetSenseAndVerbClassFromDictionaryEntry(
                    dictionaryEntry,
                    senseIndex,
                    out sense,
                    out verbClass,
                    out reading,
                    out categoryString,
                    ref error);

            if (verbClass == VerbClass.Suru)
                GetSuruFormsAndStems(
                    dictionaryEntry,
                    sense,
                    synonymIndex,
                    reading,
                    categoryString,
                    out kanji,
                    out kanjiStem,
                    out kana,
                    out kanaStem,
                    out romaji,
                    out romajiStem);
            else
                GetJapaneseFormsAndStems(
                    dictionaryEntry,
                    sense,
                    synonymIndex,
                    reading,
                    out kanji,
                    out kanjiStem,
                    out kana,
                    out kanaStem,
                    out romaji,
                    out romajiStem);

            if (returnValue)
                GetEnglishStems(kanji, sense, synonymIndex, englishStems, root);
            else
                GetEnglishStems(kanji, dictionaryEntry.GetSenseIndexed(0), 0, englishStems, root);


            if ((verbClass == VerbClass.Unknown) && !String.IsNullOrEmpty(kana))
            {
                if (kana[kana.Length - 1] == 'る')
                    verbClass = VerbClass.Ichidan;
                else
                    verbClass = VerbClass.Godan;
            }

            stems.DictionaryFormJapanese = kanji;
            stems.DictionaryFormKana = kana;
            stems.DictionaryFormRomaji = romaji;

            stems.StemJapanese = (!String.IsNullOrEmpty(kanji) ? kanji.Substring(0, kanji.Length - 1) : String.Empty);
            stems.StemKana = kanaStem;
            stems.StemRomaji = romajiStem;

            dictionaryForm.SetText(JapaneseID, kanji);
            dictionaryForm.SetText(JapaneseKanaID, kana);
            dictionaryForm.SetText(JapaneseRomajiID, romaji);
            dictionaryForm.SetText(EnglishID, englishStems.Root);

            returnValue = GetJapaneseVerbStems(
                kanji,
                verbClass,
                stems,
                root,
                ref error);

            return returnValue;
        }

        protected bool GetSubVerbStems(
            string kanji,
            string kana,
            string romaji,
            string english,
            VerbClass verbClass,
            JapaneseStems stems,
            MultiLanguageString root,
            MultiLanguageString dictionaryForm,
            out string error)
        {
            bool returnValue = true;

            error = String.Empty;

            if ((verbClass == VerbClass.Suru) && !kana.EndsWith("する"))
            {
                kanji += "する";
                kana += "する";
                romaji += "suru";
            }

            string kanjiStem = (!String.IsNullOrEmpty(kanji) ? kanji.Substring(0, kanji.Length - 1) : String.Empty);
            string kanaStem = (!String.IsNullOrEmpty(kana) ? kana.Substring(0, kana.Length - 1) : String.Empty);
            string romajiStem = String.Empty;

            if (!String.IsNullOrEmpty(romaji) && (romaji.Length >= 2))
            {
                if (romaji[romaji.Length - 1] == 'u')
                {
                    switch (romaji[romaji.Length - 2])
                    {
                        case 'b':
                        case 'g':
                        case 'k':
                        case 'm':
                        case 'n':
                        case 'r':
                            romajiStem = romaji.Substring(0, romaji.Length - 2);
                            break;
                        case 's':
                            if ((romaji.Length > 2) && (romaji[romaji.Length - 3] == 't'))
                                romajiStem = romaji.Substring(0, romaji.Length - 3);
                            else
                                romajiStem = romaji.Substring(0, romaji.Length - 2);
                            break;
                        default:
                            romajiStem = romaji.Substring(0, romaji.Length - 1);
                            break;
                    }
                }
                else
                    romajiStem = romaji;
            }

            stems.DictionaryFormJapanese = kanji;
            stems.DictionaryFormKana = kana;
            stems.DictionaryFormRomaji = romaji;

            stems.StemJapanese = (!String.IsNullOrEmpty(kanji) ? kanji.Substring(0, kanji.Length - 1) : String.Empty);
            stems.StemKana = kanaStem;
            stems.StemRomaji = romajiStem;

            dictionaryForm.SetText(JapaneseID, kanji);
            dictionaryForm.SetText(JapaneseKanaID, kana);
            dictionaryForm.SetText(JapaneseRomajiID, romaji);
            dictionaryForm.SetText(EnglishID, english);

            returnValue = GetJapaneseVerbStems(
                kanji,
                verbClass,
                stems,
                root,
                ref error);

            return returnValue;
        }

        protected bool GetSenseAndVerbClassFromDictionaryEntry(
            DictionaryEntry dictionaryEntry,
            int senseIndex,
            out Sense sense,
            out VerbClass verbClass,
            out int reading,
            out string categoryString,
            ref string error)
        {
            if ((senseIndex >= 0) && (senseIndex < dictionaryEntry.SenseCount))
                sense = dictionaryEntry.GetSenseIndexed(senseIndex);
            else
            {
                sense = dictionaryEntry.GetCategorizedSense(EnglishID, LexicalCategory.Verb);
                if (sense == null)
                {
                    foreach (Sense aSense in dictionaryEntry.Senses)
                    {
                        if ((aSense.Category == LexicalCategory.Noun) && aSense.CategoryString.Contains("vs"))
                        {
                            sense = aSense;
                            break;
                        }
                    }
                }
            }
            return GetVerbClassFromSense(dictionaryEntry, sense, out verbClass, out reading, out categoryString, ref error);
        }

        protected bool GetVerbClassFromSense(
            DictionaryEntry dictionaryEntry,
            Sense sense,
            out VerbClass verbClass,
            out int reading,
            out string categoryString,
            ref string error)
        {
            if (sense == null)
            {
                verbClass = VerbClass.Unknown;
                reading = 0;
                categoryString = String.Empty;
                error = "Null sense in GetVerbClassFromSense.";
                return false;
            }

            reading = sense.Reading;
            categoryString = sense.CategoryString;

            string[] parts = categoryString.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);
            bool returnValue = true;

            verbClass = VerbClass.Unknown;

            foreach (string part in parts)
            {
                string cat = part.Trim();

                switch (cat)
                {
                    case "v1":      // "Ichidan verb"
                        verbClass = VerbClass.Ichidan;
                        categoryString = cat;
                        if ((dictionaryEntry.KeyString == "いる") || (dictionaryEntry.KeyString == "iru"))
                        {
                            if (sense.HasMeaning("to exist", EnglishID))
                                verbClass = VerbClass.Iru;
                        }
                        break;
                    case "v2a-s":   // "Nidan verb with 'u' ending (archaic)"
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "v4h":     // "Yondan verb with `hu/fu' ending (archaic)"
                        verbClass = VerbClass.Yondan;
                        categoryString = cat;
                        break;
                    case "v4r":     // "Yondan verb with `ru' ending (archaic)":
                        verbClass = VerbClass.Yondan;
                        categoryString = cat;
                        break;
                    case "v5":      // "Godan verb (not completely classified)":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "v5aru":   // "Godan verb - -aru special class":
                        verbClass = VerbClass.Yondan;
                        categoryString = cat;
                        break;
                    case "v5b":     // "Godan verb with `bu' ending":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "v5g":     // "Godan verb with `gu' ending":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "v5k":     // "Godan verb with `ku' ending":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "v5k-s":   // "Godan verb - Iku/Yuku special class":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "v5m":     // "Godan verb with `mu' ending":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "v5n":     // "Godan verb with `nu' ending":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "v5r":     // "Godan verb with `ru' ending":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "v5r-i":   // "Godan verb with `ru' ending (irregular verb)":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        if ((dictionaryEntry.KeyString == "ある") || (dictionaryEntry.KeyString == "aru"))
                        {
                            if (sense.HasMeaning("to exist", EnglishID))
                                categoryString = cat;
                            verbClass = VerbClass.Aru;
                        }
                        break;
                    case "v5s":     // "Godan verb with `su' ending":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "v5t":     // "Godan verb with `tsu' ending":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "v5u":     // "Godan verb with `u' ending":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "v5u-s":   // "Godan verb with `u' ending (special class)":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "v5uru":   // "Godan verb - Uru old class verb (old form of Eru)":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "v5z":     // "Godan verb with `zu' ending":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "vz":      // "Ichidan verb - zuru verb (alternative form of -jiru verbs)":
                        verbClass = VerbClass.Ichidan;
                        categoryString = cat;
                        break;
                    case "vi":      // "intransitive verb":
                        break;
                    case "vt":      // "transitive verb":
                        break;
                    case "vk":      // "Kuru verb - special class":
                        verbClass = VerbClass.Kuru;
                        categoryString = cat;
                        break;
                    case "vn":      // "irregular nu verb":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "vr":      // "irregular ru verb, plain form ends with -ri":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "vs":      // "noun or participle which takes the aux. verb suru":
                        verbClass = VerbClass.Suru;
                        categoryString = cat;
                        break;
                    case "vs-c":    // "su verb - precursor to the modern suru":
                        verbClass = VerbClass.Godan;
                        categoryString = cat;
                        break;
                    case "vs-s":    // "suru verb - special class":
                        verbClass = VerbClass.Suru;
                        categoryString = cat;
                        break;
                    case "vs-i":    // "suru verb - irregular":
                        verbClass = VerbClass.Suru;
                        categoryString = cat;
                        break;
                    default:
                        break;
                }
            }

            return returnValue;
        }

        protected void GetJapaneseFormsAndStems(
            DictionaryEntry dictionaryEntry,
            Sense sense,
            int synonymIndex,
            int reading,
            out string kanji,
            out string kanjiStem,
            out string kana,
            out string kanaStem,
            out string romaji,
            out string romajiStem)
        {
            LanguageString alternate;

            kanji = String.Empty;
            kanjiStem = String.Empty;
            kana = String.Empty;
            kanaStem = String.Empty;
            romaji = String.Empty;
            romajiStem = String.Empty;

            if (dictionaryEntry.LanguageID == JapaneseID)
            {
                kanji = dictionaryEntry.KeyString;
                kanjiStem = kanji.Substring(0, kanji.Length - 1);

                alternate = dictionaryEntry.GetAlternate(
                    JapaneseKanaID,
                    reading);

                if (alternate != null)
                {
                    kana = alternate.Text;
                    kanaStem = kana.Substring(0, kana.Length - 1);
                }
                else
                {
                    kana = kanji;
                    kanaStem = kanjiStem;
                }

                alternate = dictionaryEntry.GetAlternate(
                    JapaneseRomajiID,
                    reading);

                if (alternate != null)
                    romaji = alternate.Text;
                else
                    romaji = kana;
            }
            else if (dictionaryEntry.LanguageID == JapaneseKanaID)
            {
                kana = dictionaryEntry.KeyString;
                kanaStem = kana.Substring(0, kana.Length - 1);

                kanji = kana;
                kanjiStem = kanaStem;

                alternate = dictionaryEntry.GetAlternate(
                    JapaneseRomajiID,
                    reading);

                if (alternate != null)
                    romaji = alternate.Text;
                else
                    romaji = kana;
            }
            else if (dictionaryEntry.LanguageID == JapaneseRomajiID)
            {
                romaji = dictionaryEntry.KeyString;

                alternate = dictionaryEntry.GetAlternate(
                    JapaneseID,
                    reading);

                if (alternate != null)
                {
                    kanji = alternate.Text;
                    kanjiStem = kanji.Substring(0, kanji.Length - 1);
                }
                else
                    kanji = romaji;

                alternate = dictionaryEntry.GetAlternate(
                    JapaneseKanaID,
                    reading);

                if (alternate != null)
                {
                    kana = alternate.Text;
                    kanaStem = kana.Substring(0, kana.Length - 1);
                }
                else
                    kana = kanji;
            }
            else
            {
                LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(JapaneseID);

                if (languageSynonyms != null)
                {
                    kanji = languageSynonyms.GetSynonymIndexed(synonymIndex);
                    if (!String.IsNullOrEmpty(kanji))
                        kanjiStem = kanji.Substring(0, kanji.Length - 1);
                }

                languageSynonyms = sense.GetLanguageSynonyms(JapaneseKanaID);

                if (languageSynonyms != null)
                {
                    kana = languageSynonyms.GetSynonymIndexed(synonymIndex);
                    if (!String.IsNullOrEmpty(kanji))
                        kanaStem = kana.Substring(0, kana.Length - 1);
                }

                languageSynonyms = sense.GetLanguageSynonyms(JapaneseRomajiID);

                if (languageSynonyms != null)
                    romaji = languageSynonyms.GetSynonymIndexed(synonymIndex);
            }

            if (!String.IsNullOrEmpty(romaji) && (romaji.Length >= 2))
            {
                if (romaji[romaji.Length - 1] == 'u')
                {
                    switch (romaji[romaji.Length - 2])
                    {
                        case 'b':
                        case 'g':
                        case 'k':
                        case 'm':
                        case 'n':
                        case 'r':
                            romajiStem = romaji.Substring(0, romaji.Length - 2);
                            break;
                        case 's':
                            if ((romaji.Length > 2) && (romaji[romaji.Length - 3] == 't'))
                                romajiStem = romaji.Substring(0, romaji.Length - 3);
                            else
                                romajiStem = romaji.Substring(0, romaji.Length - 2);
                            break;
                        default:
                            romajiStem = romaji.Substring(0, romaji.Length - 1);
                            break;
                    }
                }
                else
                    romajiStem = romaji;
            }

            if (String.IsNullOrEmpty(kanaStem))
                kanaStem = (!String.IsNullOrEmpty(kanjiStem) ? kanjiStem : romajiStem);

            if (String.IsNullOrEmpty(kanjiStem))
                kanjiStem = (!String.IsNullOrEmpty(kanaStem) ? kanaStem : romajiStem);

            if ((sense != null) && !String.IsNullOrEmpty(sense.CategoryString) && sense.CategoryString.EndsWith("uk"))
            {
                kanji = kana;
                kanjiStem = kanaStem;
            }
        }

        protected void GetSuruFormsAndStems(
            DictionaryEntry dictionaryEntry,
            Sense sense,
            int synonymIndex,
            int reading,
            string categoryString,
            out string kanji,
            out string kanjiStem,
            out string kana,
            out string kanaStem,
            out string romaji,
            out string romajiStem)
        {
            kanji = String.Empty;
            kanjiStem = String.Empty;
            kana = String.Empty;
            kanaStem = String.Empty;
            romaji = String.Empty;
            romajiStem = String.Empty;

            if (dictionaryEntry.LanguageID == JapaneseID)
            {
                kanji = dictionaryEntry.KeyString;

                LanguageString alternateKana = dictionaryEntry.GetAlternate(
                    JapaneseKanaID,
                    reading);

                if (alternateKana != null)
                    kana = alternateKana.Text;
                else
                    kana = dictionaryEntry.KeyString;

                LanguageString alternateRomaji = dictionaryEntry.GetAlternate(
                    LanguageLookup.JapaneseRomaji,
                    reading);

                if (alternateRomaji != null)
                    romaji = alternateRomaji.Text;
                else
                    romaji = dictionaryEntry.KeyString;
            }
            else if (dictionaryEntry.LanguageID == JapaneseKanaID)
            {
                LanguageString alternateKanji = dictionaryEntry.GetAlternate(
                    JapaneseID,
                    reading);

                if (alternateKanji != null)
                    kanji = alternateKanji.Text;
                else
                    kanji = dictionaryEntry.KeyString;

                kana = dictionaryEntry.KeyString;

                LanguageString alternateRomaji = dictionaryEntry.GetAlternate(
                    LanguageLookup.JapaneseRomaji,
                    reading);

                if (alternateRomaji != null)
                    romaji = alternateRomaji.Text;
                else
                    romaji = dictionaryEntry.KeyString;
            }
            else if (dictionaryEntry.LanguageID == JapaneseRomajiID)
            {
                LanguageString alternateKanji = dictionaryEntry.GetAlternate(
                    JapaneseID,
                    reading);

                if (alternateKanji != null)
                    kanji = alternateKanji.Text;
                else
                    kanji = dictionaryEntry.KeyString;

                LanguageString alternateKana = dictionaryEntry.GetAlternate(
                    JapaneseKanaID,
                    reading);

                if (alternateKana != null)
                    kana = alternateKana.Text;
                else
                    kana = dictionaryEntry.KeyString;

                romaji = dictionaryEntry.KeyString;
            }
            else
            {
                LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(JapaneseID);

                if (languageSynonyms != null)
                    kanji = languageSynonyms.GetSynonymIndexed(synonymIndex);

                languageSynonyms = sense.GetLanguageSynonyms(JapaneseKanaID);

                if (languageSynonyms != null)
                    kana = languageSynonyms.GetSynonymIndexed(synonymIndex);

                languageSynonyms = sense.GetLanguageSynonyms(JapaneseRomajiID);

                if (languageSynonyms != null)
                    romaji = languageSynonyms.GetSynonymIndexed(synonymIndex);
            }

            if (!kana.EndsWith("する"))
            {
                kanji += "する";
                kana += "する";
                romaji += "suru";
            }

            kanjiStem = kanji.Substring(0, kanji.Length - 1);
            kanaStem = kana.Substring(0, kana.Length - 1);
            romajiStem = romaji.Substring(0, romaji.Length - 2);

            if (sense.CategoryString.EndsWith("uk"))
            {
                kanji = kana;
                kanjiStem = kanaStem;
            }
        }

        protected bool GetJapaneseVerbStems(
            string input,
            VerbClass verbClass,
            JapaneseStems stems,
            MultiLanguageString root,
            ref string error)
        {
            if (String.IsNullOrEmpty(input))
                return true;

            char lastChr = input[input.Length - 1];
            bool returnValue = true;

            stems.StemJapanese = input.Substring(0, input.Length - 1);
            stems.UStem = lastChr.ToString();

            switch (lastChr)
            {
                case 'ぶ':
                    stems.AStem = "ば";
                    stems.EStem = "べ";
                    stems.IStem = "び";
                    stems.OStem = "ぼ";
                    stems.TAStem = "んだ";
                    stems.TEStem = "んで";
                    stems.UStemRomaji = "bu";
                    break;
                case 'ぐ':
                    stems.AStem = "が";
                    stems.EStem = "げ";
                    stems.IStem = "ぎ";
                    stems.OStem = "ごう";
                    stems.TAStem = "いだ";
                    stems.TEStem = "いで";
                    stems.UStemRomaji = "gu";
                    break;
                case 'く':
                    stems.AStem = "か";
                    stems.EStem = "け";
                    stems.IStem = "き";
                    stems.OStem = "こう";
                    if (input.EndsWith("行く"))
                    {
                        stems.TAStem = "った";
                        stems.TEStem = "って";
                    }
                    else
                    {
                        stems.TAStem = "いた";
                        stems.TEStem = "いて";
                    }
                    stems.UStemRomaji = "ku";
                    break;
                case 'む':
                    stems.AStem = "ま";
                    stems.EStem = "め";
                    stems.IStem = "み";
                    stems.OStem = "もう";
                    stems.TAStem = "んだ";
                    stems.TEStem = "んで";
                    stems.UStemRomaji = "mu";
                    break;
                case 'ぬ':
                    stems.AStem = "な";
                    stems.EStem = "ね";
                    stems.IStem = "に";
                    stems.OStem = "のう";
                    stems.TAStem = "んだ";
                    stems.TEStem = "んで";
                    stems.UStemRomaji = "nu";
                    break;
                case 'る':
                    switch (verbClass)
                    {
                        case VerbClass.Ichidan:
                        case VerbClass.Suru:
                        case VerbClass.Kuru:
                        case VerbClass.Iru:
                            stems.AStem = String.Empty;
                            stems.EStem = "れ";
                            stems.IStem = String.Empty;
                            stems.OStem = "よう";
                            stems.TAStem = "た";
                            stems.TEStem = "て";
                            stems.UStemRomaji = "ru";
                            break;
                        case VerbClass.Godan:
                        case VerbClass.Aru:
                            stems.AStem = "ら";
                            stems.EStem = "れ";
                            stems.IStem = "り";
                            stems.OStem = "ろう";
                            stems.TAStem = "った";
                            stems.TEStem = "って";
                            stems.UStemRomaji = "ru";
                            break;
                        case VerbClass.Yondan:
                            stems.AStem = "ら";
                            stems.EStem = "え";
                            stems.IStem = "い";
                            stems.OStem = "ろう";
                            stems.TAStem = "った";
                            stems.TEStem = "って";
                            stems.UStemRomaji = "ru";
                            break;
                        default:
                            error = "Unexpected verb class: " + verbClass.ToString();
                            returnValue = false;
                            stems.AStem = String.Empty;
                            stems.EStem = "れ";
                            stems.IStem = String.Empty;
                            stems.OStem = "よう";
                            stems.TAStem = "っだ";
                            stems.TEStem = "って";
                            stems.UStemRomaji = "ru";
                            break;
                    }
                    break;
                case 'す':
                    stems.AStem = "さ";
                    stems.EStem = "せ";
                    stems.IStem = "し";
                    stems.OStem = "そう";
                    stems.TAStem = "した";
                    stems.TEStem = "して";
                    stems.UStemRomaji = "su";
                    break;
                case 'つ':
                    stems.AStem = "た";
                    stems.EStem = "て";
                    stems.IStem = "ち";
                    stems.OStem = "とう";
                    stems.TAStem = "った";
                    stems.TEStem = "って";
                    stems.UStemRomaji = "tsu";
                    break;
                case 'う':
                    stems.AStem = "わ";
                    stems.EStem = "え";
                    stems.IStem = "い";
                    stems.OStem = "おう";
                    stems.TAStem = "った";
                    stems.TEStem = "って";
                    stems.UStemRomaji = "u";
                    break;
                default:
                    stems.AStem = "あ";
                    stems.EStem = "え";
                    stems.IStem = "い";
                    stems.OStem = "お";
                    stems.UStem = String.Empty;
                    stems.TAStem = "だ";
                    stems.TEStem = "で";
                    stems.UStemRomaji = String.Empty;
                    verbClass = VerbClass.Unknown;
                    break;
            }

            root.SetText(
                JapaneseID,
                stems.StemJapanese);

            root.SetText(
                JapaneseKanaID,
                stems.StemKana);

            if (String.IsNullOrEmpty(stems.StemRomaji))
            {
                if (!String.IsNullOrEmpty(stems.DictionaryFormRomaji) && !String.IsNullOrEmpty(stems.UStemRomaji))
                    stems.StemRomaji = stems.DictionaryFormRomaji.Substring(0, stems.DictionaryFormRomaji.Length - stems.UStemRomaji.Length);
                else
                    stems.StemRomaji = String.Empty;
            }

            root.SetText(
                JapaneseRomajiID,
                stems.StemRomaji);

            return returnValue;
        }

        protected bool GetEnglishStems(
            string kanji,
            Sense sense,
            int synonymIndex,
            EnglishStems englishStems,
            MultiLanguageString root)
        {
            if (kanji == "する")
            {
                englishStems.Root = "do";
                englishStems.Preterite = "did";
                englishStems.PresentParticiple = "doing";
                englishStems.PastParticiple = "done";
            }
            else if ((kanji == "くる") || (kanji == "来る"))
            {
                englishStems.Root = "come";
                englishStems.Preterite = "came";
                englishStems.PresentParticiple = "coming";
                englishStems.PastParticiple = "come";
            }
            else if (kanji == "くれる")
            {
                englishStems.Root = "give";
                englishStems.Preterite = "gave";
                englishStems.PresentParticiple = "giving";
                englishStems.PastParticiple = "given";
            }
            else if (kanji == "ある")
            {
                englishStems.Root = "be/is/are";
                englishStems.Preterite = "was/were";
                englishStems.PresentParticiple = "being";
                englishStems.PastParticiple = "been";
            }
            else if (kanji == "いる")
            {
                englishStems.Root = "be/am/are/is";
                englishStems.Preterite = "was/were";
                englishStems.PresentParticiple = "being";
                englishStems.PastParticiple = "been";
            }
            else
            {
                if (sense != null)
                {
                    if (englishStems.GetStemsFromSense(sense, synonymIndex))
                    {
                        root.SetText(
                            EnglishID,
                            englishStems.Root);
                        return true;
                    }
                }

                englishStems.SetUnknown(kanji);

                root.SetText(
                    EnglishID,
                    String.Empty);

                return false;
            }

            return true;
        }
    }
}
