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
        public enum VerbClass
        {
            Unknown,
            Ichidan,
            Godan,
            Yondan,
            Suru,
            Kuru,
            Kureru,
            Aru,
            Iru
        };

        public enum AdjectiveClass
        {
            Unknown,
            I,
            Na,
            No,
            Pn,
            T,
            F,
            Other
        };

        public enum Special
        {
            Unknown,
            Normal,
            Dictionary,
            Stem,
            Participle,
            Perfective,
            Infinitive,
            Potential,
            Passive,
            Causitive,
            Desire,
            Honorific1,
            Honorific2,
            Humble1,
            Humble2,
            Humble3,
            Humble4,
            Adverbial,
            Noun
        };

        public enum Tense
        {
            Unknown,
            Present,
            Past
        };

        public enum Mood
        {
            Unknown,
            Indicative,
            Recursive,
            Volitional,
            Prenomial,
            Presumptive,
            Probable,
            Continuative,
            ContinuativeParticiple,
            ContinuativeInfinitive,
            Progressive,
            Unintentional,
            Probative,
            Imperative,
            Request,
            Provisional,
            Conditional,
            Alternative
        };

        public enum Politeness
        {
            Unknown,
            Abrupt,
            Plain,
            Polite,
            Honorific
        };

        public enum Polarity
        {
            Unknown,
            Positive,
            Negative
        };

        public enum Alternate
        {
            Unknown,
            Alternate1,
            Alternate2,
            Alternate3,
            Alternate4,
            Desu,
            Arimasu,
            Gozaimasu,
            Nai,
            Nak
        };

        public enum Contraction
        {
            Unknown,
            ContractionTeShimauToChau,
            ContractionTeShimauToChimau,
            ContractionDeShimauToJimau1,
            ContractionDeShimauToJimau2,
            ContractionTeIruToTeru,
            ContractionDeIruToDeru,
            ContractionTeIkuToTeku,
            ContractionDeIkuToDeku,
            ContractionTeOkuToToku,
            ContractionTeWaToCha,
            ContractionDeWaToJa,
            ContractionDeWaToDe,
            ContractionTeWaShinaiToTeYaShinai,
            ContractionTeAgeruToTageru,
            ContractionRuNoToNNo,
            ContractionRaNaiToNNai,
            ContractionNakerebaToNakya,
            ContractionNakuteWaToNakucha,
            ContractionNaiToN,
            ContractionNaiToNee,
            ContractionDeWaNaiKaToJan,
            ContractionJaNaiKaToJan,
            ContractionTadarouToTarou
        };

        private static string[] TenseArray =
        {
            "Present",
            "Past"
        };

        private static string[] PolitenessArray =
        {
            "Plain",
            "Polite"
        };

        private static string[] PolarityArray =
        {
            "Positive",
            "Negative"
        };

        static string[] NoAlternates = { null };
        static string[] Alternates2 = { "Alternate1", "Alternate2" };

        protected Special GetSpecial(Designator designation)
        {
            Special special = GetSpecialIndexed(designation, 0);
            return special;
        }

        protected Special GetSpecial2(Designator designation)
        {
            Special special = GetSpecialIndexed(designation, 1);
            return special;
        }

        protected Special GetSpecialIndexed(Designator designation, int index)
        {
            string specialString = designation.GetClassificationValueIndexed("Special", index);

            if (String.IsNullOrEmpty(specialString))
                return Special.Unknown;

            Special special = Special.Unknown;

            switch (specialString)
            {
                case "Normal":
                    special = Special.Normal;
                    break;
                case "Dictionary":
                    special = Special.Dictionary;
                    break;
                case "Stem":
                    special = Special.Stem;
                    break;
                case "Participle":
                    special = Special.Participle;
                    break;
                case "Perfective":
                    special = Special.Perfective;
                    break;
                case "Infinitive":
                    special = Special.Infinitive;
                    break;
                case "Potential":
                    special = Special.Potential;
                    break;
                case "Passive":
                    special = Special.Passive;
                    break;
                case "Causitive":
                    special = Special.Causitive;
                    break;
                case "Desire":
                    special = Special.Desire;
                    break;
                case "Honorific1":
                    special = Special.Honorific1;
                    break;
                case "Honorific2":
                    special = Special.Honorific2;
                    break;
                case "Humble1":
                    special = Special.Humble1;
                    break;
                case "Humble2":
                    special = Special.Humble2;
                    break;
                case "Humble3":
                    special = Special.Humble3;
                    break;
                case "Humble4":
                    special = Special.Humble4;
                    break;
                case "Adverbial":
                    special = Special.Adverbial;
                    break;
                case "Noun":
                    special = Special.Noun;
                    break;
                default:
                    break;
            }

            return special;
        }

        protected Tense GetTense(Designator designation)
        {
            string tenseString = designation.GetClassificationValue("Tense");

            if (String.IsNullOrEmpty(tenseString))
                return Tense.Unknown;

            Tense tense = Tense.Unknown;

            switch (tenseString)
            {
                case "Present":
                    tense = Tense.Present;
                    break;
                case "Past":
                    tense = Tense.Past;
                    break;
                default:
                    break;
            }

            return tense;
        }

        protected Mood GetMood(Designator designation)
        {
            return GetMoodIndexed(designation, 0);
        }

        protected Mood GetSubMood(Designator designation)
        {
            return GetMoodIndexed(designation, 1);
        }

        protected Mood GetMoodIndexed(Designator designation, int index)
        {
            string moodString = designation.GetClassificationValueIndexed("Mood", index);

            if (String.IsNullOrEmpty(moodString))
                return Mood.Unknown;

            Mood mood = Mood.Unknown;

            switch (moodString)
            {
                case "Indicative":
                    mood = Mood.Indicative;
                    break;
                case "Recursive":
                    mood = Mood.Recursive;
                    break;
                case "Prenomial":
                    mood = Mood.Prenomial;
                    break;
                case "Volitional":
                    mood = Mood.Volitional;
                    break;
                case "Presumptive":
                    mood = Mood.Presumptive;
                    break;
                case "Probable":
                    mood = Mood.Probable;
                    break;
                case "Continuative":
                    mood = Mood.Continuative;
                    break;
                case "ContinuativeParticiple":
                    mood = Mood.ContinuativeParticiple;
                    break;
                case "ContinuativeInfinitive":
                    mood = Mood.ContinuativeInfinitive;
                    break;
                case "Progressive":
                    mood = Mood.Progressive;
                    break;
                case "Unintentional":
                    mood = Mood.Unintentional;
                    break;
                case "Probative":
                    mood = Mood.Probative;
                    break;
                case "Imperative":
                    mood = Mood.Imperative;
                    break;
                case "Request":
                    mood = Mood.Request;
                    break;
                case "Provisional":
                    mood = Mood.Provisional;
                    break;
                case "Conditional":
                    mood = Mood.Conditional;
                    break;
                case "Alternative":
                    mood = Mood.Alternative;
                    break;
                default:
                    break;
            }

            return mood;
        }

        protected Politeness GetPoliteness(Designator designation)
        {
            string politenessString = designation.GetClassificationValue("Politeness");

            if (String.IsNullOrEmpty(politenessString))
                return Politeness.Unknown;

            Politeness politeness = Politeness.Unknown;

            switch (politenessString)
            {
                case "Abrupt":
                    politeness = Politeness.Abrupt;
                    break;
                case "Plain":
                    politeness = Politeness.Plain;
                    break;
                case "Polite":
                    politeness = Politeness.Polite;
                    break;
                case "Honorific":
                    politeness = Politeness.Honorific;
                    break;
                default:
                    break;
            }

            return politeness;
        }

        protected Polarity GetPolarity(Designator designation)
        {
            string polarityString = designation.GetClassificationValue("Polarity");

            if (String.IsNullOrEmpty(polarityString))
                return Polarity.Unknown;

            Polarity polarity = Polarity.Unknown;

            switch (polarityString)
            {
                case "Positive":
                    polarity = Polarity.Positive;
                    break;
                case "Negative":
                    polarity = Polarity.Negative;
                    break;
                default:
                    break;
            }

            return polarity;
        }

        protected Alternate GetAlternate(Designator designation)
        {
            string alternateString = designation.GetClassificationValue("Alternate");

            if (String.IsNullOrEmpty(alternateString))
                return Alternate.Unknown;

            Alternate alternate = Alternate.Unknown;

            switch (alternateString)
            {
                case "Alternate1":
                    alternate = Alternate.Alternate1;
                    break;
                case "Alternate2":
                    alternate = Alternate.Alternate2;
                    break;
                case "Alternate3":
                    alternate = Alternate.Alternate3;
                    break;
                case "Alternate4":
                    alternate = Alternate.Alternate4;
                    break;
                case "Desu":
                    alternate = Alternate.Desu;
                    break;
                case "Arimasu":
                    alternate = Alternate.Arimasu;
                    break;
                case "Gozaimasu":
                    alternate = Alternate.Gozaimasu;
                    break;
                case "Nai":
                    alternate = Alternate.Nai;
                    break;
                case "Nak":
                    alternate = Alternate.Nak;
                    break;
                default:
                    break;
            }

            return alternate;
        }

        protected Contraction GetContraction(Designator designation)
        {
            string contractionString = designation.GetClassificationValue("Contraction");

            if (String.IsNullOrEmpty(contractionString))
                return Contraction.Unknown;

            Contraction contraction = Contraction.Unknown;

            switch (contractionString)
            {
                case "ContractionTeShimauToChau":
                    contraction = Contraction.ContractionTeShimauToChau;
                    break;
                case "ContractionTeShimauToChimau":
                    contraction = Contraction.ContractionTeShimauToChimau;
                    break;
                case "ContractionDeShimauToJimau1":
                    contraction = Contraction.ContractionDeShimauToJimau1;
                    break;
                case "ContractionDeShimauToJimau2":
                    contraction = Contraction.ContractionDeShimauToJimau2;
                    break;
                case "ContractionTeIruToTeru":
                    contraction = Contraction.ContractionTeIruToTeru;
                    break;
                case "ContractionDeIruToDeru":
                    contraction = Contraction.ContractionDeIruToDeru;
                    break;
                case "ContractionTeIkuToTeku":
                    contraction = Contraction.ContractionTeIkuToTeku;
                    break;
                case "ContractionDeIkuToDeku":
                    contraction = Contraction.ContractionDeIkuToDeku;
                    break;
                case "ContractionTeOkuToToku":
                    contraction = Contraction.ContractionTeOkuToToku;
                    break;
                case "ContractionTeWaToCha":
                    contraction = Contraction.ContractionTeWaToCha;
                    break;
                case "ContractionDeWaToJa":
                    contraction = Contraction.ContractionDeWaToJa;
                    break;
                case "ContractionDeWaToDe":
                    contraction = Contraction.ContractionDeWaToDe;
                    break;
                case "ContractionTeWaShinaiToTeYaShinai":
                    contraction = Contraction.ContractionTeWaShinaiToTeYaShinai;
                    break;
                case "ContractionTeAgeruToTageru":
                    contraction = Contraction.ContractionTeAgeruToTageru;
                    break;
                case "ContractionRuNoToNNo":
                    contraction = Contraction.ContractionRuNoToNNo;
                    break;
                case "ContractionRaNaiToNNai":
                    contraction = Contraction.ContractionRaNaiToNNai;
                    break;
                case "ContractionNakerebaToNakya":
                    contraction = Contraction.ContractionNakerebaToNakya;
                    break;
                case "ContractionNakuteWaToNakucha":
                    contraction = Contraction.ContractionNakuteWaToNakucha;
                    break;
                case "ContractionNaiToN":
                    contraction = Contraction.ContractionNaiToN;
                    break;
                case "ContractionNaiToNee":
                    contraction = Contraction.ContractionNaiToNee;
                    break;
                case "ContractionDeWaNaiKaToJan":
                    contraction = Contraction.ContractionDeWaNaiKaToJan;
                    break;
                case "ContractionJaNaiKaToJan":
                    contraction = Contraction.ContractionJaNaiKaToJan;
                    break;
                case "ContractionTadarouToTarou":
                    contraction = Contraction.ContractionTadarouToTarou;
                    break;
                default:
                    break;
            }

            return contraction;
        }

        protected AdjectiveClass GetAdjectiveClassFromCategoryString(string cat)
        {
            AdjectiveClass adjectiveClass = AdjectiveClass.Unknown;

            switch (cat)
            {
                case "adj-i":
                    adjectiveClass = AdjectiveClass.I;
                    break;
                case "adj-na":
                    adjectiveClass = AdjectiveClass.Na;
                    break;
                case "adj-no":
                    adjectiveClass = AdjectiveClass.No;
                    break;
                case "adj-pn":
                    adjectiveClass = AdjectiveClass.Pn;
                    break;
                case "adj-t":
                    adjectiveClass = AdjectiveClass.T;
                    break;
                case "adj-f":
                    adjectiveClass = AdjectiveClass.F;
                    break;
                case "adj":
                    adjectiveClass = AdjectiveClass.Other;
                    break;
                default:
                    break;
            }

            return adjectiveClass;
        }
    }
}
