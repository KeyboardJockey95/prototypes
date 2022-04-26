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
        private static string[] VerbRecursiveArray =
        {
            "Potential",
            "Passive",
            "Causitive",
            "Desire",
            "Honorific1",
            "Honorific2",
            "Humble1",
            "Humble2",
            "Humble3",
            "Humble4"
        };

        private static string[] VerbMoodArray =
        {
            "Indicative",
            "Volitional",
            "Presumptive",
            "ContinuativeParticiple",
            "ContinuativeInfinitive",
            "Progressive",
            "Unintentional",
            "Probative",
            "Imperative",
            "Request",
            "Provisional",
            "Conditional",
            "Alternative"
        };

        private static string[] VerbMoodsWithPastArray =
        {
            "Indicative",
            "Presumptive",
            "Progressive",
            "Unintentional",
            "Probative"
        };

        private static string[] VerbCompoundMoods =
        {
            "Volitional",
            "Presumptive",
            "ContinuativeParticiple",
            "ContinuativeInfinitive",
            "Imperative",
            "Request",
            "Provisional",
            "Conditional",
            "Alternative"
        };

        private static string[] VerbMoodsForDefaultArray =
        {
            "Indicative"
        };

        static string[] VerbImperativePoliteness =
        {
            "Abrupt",
            "Plain"
        };
        static string[] VerbRequestPoliteness =
        {
            "Polite",
            "Honorific"
        };
        static string[] VerbProvisionalPoliteness =
        {
            null
        };

        private static List<Designator> _AllVerbDesignations = null;

        public static List<Designator> AllVerbDesignations
        {
            get
            {
                if (_AllVerbDesignations != null)
                    return _AllVerbDesignations;

                _AllVerbDesignations = new List<Designator>();

                AddVerbCoreSpecial(_AllVerbDesignations);

                AddVerbSpecialAndTense(_AllVerbDesignations, null, VerbMoodArray);

                foreach (string recursive in VerbRecursiveArray)
                {
                    if (recursive == "Desire")
                        AddSubAdjectiveDesignations(_AllVerbDesignations, "Desire");
                    else
                    {
                        AddVerbCoreSpecialRecursive(_AllVerbDesignations, recursive);
                        AddVerbSpecialAndTense(_AllVerbDesignations, recursive, VerbMoodArray);
                    }
                }

                return _AllVerbDesignations;
            }
        }

        private static List<Designator> _DefaultVerbDesignations = null;

        public static List<Designator> DefaultVerbDesignations
        {
            get
            {
                if (_DefaultVerbDesignations != null)
                    return _DefaultVerbDesignations;

                _DefaultVerbDesignations = new List<Designator>();

                AddVerbCoreSpecial(_DefaultVerbDesignations);

                AddVerbSpecialAndTense(_DefaultVerbDesignations, null, VerbMoodsForDefaultArray);

                foreach (string recursive in VerbRecursiveArray)
                {
                    AddVerbCoreSpecialRecursive(_DefaultVerbDesignations, recursive);
                    AddVerbSpecialAndTense(_DefaultVerbDesignations, recursive, VerbMoodsForDefaultArray);
                }

                return _DefaultVerbDesignations;
            }
        }

        private static void AddVerbCoreSpecial(List<Designator> designations)
        {
            // The dictionary form (no change).
            designations.Add(
                new Designator(
                    "Dictionary",
                    new Classifier("Special", "Dictionary")));

            // The verb stem - removing the '?u'.
            designations.Add(
                new Designator(
                    "Stem",
                    new Classifier("Special", "Stem")));

            // The infinitive form of the verb (I don't know how to derive this).
            designations.Add(
                new Designator(
                    "Infinitive",
                    new Classifier("Special", "Infinitive")));

            // The participle ("te") form.
            designations.Add(
                new Designator(
                    "Participle",
                    new Classifier("Special", "Participle"),
                    new Classifier("Tense", "Present")));

            // The participle "cha" and "ja" contraction.
            designations.Add(
                new Designator(
                    "Participle ContractionTeWaToCha",
                    new Classifier("Special", "Participle"),
                    new Classifier("Tense", "Present"),
                    new Classifier("Contraction", "ContractionTeWaToCha")));

            // The participle "TeYaShinai" contraction.
            /* Handle this in Contract and ExpandContraction, as the verb doesn't change.
            designations.Add(
                new Designator(
                    "Participle ContractionTeWaShinaiToTeYaShinai",
                    new Classifier("Special", "Participle"),
                    new Classifier("Tense", "Present"),
                    new Classifier("Contraction", "ContractionTeWaShinaiToTeYaShinai")));
            */

            // The perfective ("ta") form.
            designations.Add(
                new Designator(
                    "Perfective",
                    new Classifier("Special", "Perfective")));
        }

        private static void AddVerbCoreSpecialRecursive(List<Designator> designations, string special)
        {
            string specialPrfix = special + " ";

            // The dictionary form (no change).
            designations.Add(
                new Designator(
                    specialPrfix + "Dictionary",
                    new Classifier("Special", special),
                    new Classifier("Special", "Dictionary")));

            // The verb stem - removing the '?u'.
            designations.Add(
                new Designator(
                    specialPrfix + "Stem",
                    new Classifier("Special", special),
                    new Classifier("Special", "Stem")));

            // The infinitive form of the verb (I don't know how to derive this).
            designations.Add(
                new Designator(
                    specialPrfix + "Infinitive",
                    new Classifier("Special", special),
                    new Classifier("Special", "Infinitive")));

            // The participle ("te") form.
            designations.Add(
                new Designator(
                    specialPrfix + "Participle",
                    new Classifier("Special", special),
                    new Classifier("Special", "Participle"),
                    new Classifier("Tense", "Present")));

            // The participle "cha" and "ja" contraction.
            designations.Add(
                new Designator(
                    specialPrfix + "Participle ContractionTeWaToCha",
                    new Classifier("Special", special),
                    new Classifier("Special", "Participle"),
                    new Classifier("Tense", "Present"),
                    new Classifier("Contraction", "ContractionTeWaToCha")));

            // The participle "TeYaShinai" contraction.
            /* Handle this in Contract and ExpandContraction, as the verb doesn't change.
            designations.Add(
                new Designator(
                    specialPrfix + "Participle ContractionTeWaShinaiToTeYaShinai",
                    new Classifier("Special", special),
                    new Classifier("Special", "Participle"),
                    new Classifier("Tense", "Present"),
                    new Classifier("Contraction", "ContractionTeWaShinaiToTeYaShinai")));
            */

            // The perfective ("ta") form.
            designations.Add(
                new Designator(
                    specialPrfix + "Perfective",
                    new Classifier("Special", special),
                    new Classifier("Special", "Perfective")));
        }

        private static void AddVerbSpecialAndTense(List<Designator> designations, string special, string[] moods)
        {
            foreach (string mood in moods)
            {
                AddVerbMood(designations, special, null, mood, null, "Present");

                if (VerbMoodsWithPastArray.Contains(mood))
                    AddVerbMood(designations, special, null, mood, null, "Past");

                if ((mood == "Progressive") || (mood == "Unintentional") || (mood == "Probative"))
                {
                    foreach (string mood2 in VerbCompoundMoods)
                    {
                        AddVerbMood(designations, special, null, mood2, mood, "Present");

                        if (VerbMoodsWithPastArray.Contains(mood2))
                            AddVerbMood(designations, special, null, mood2, mood, "Past");
                    }
                }
            }

            if (special == "Causitive")
            {
                foreach (string mood in moods)
                {
                    AddVerbMood(designations, special, "Passive", mood, null, "Present");

                    if (VerbMoodsWithPastArray.Contains(mood))
                        AddVerbMood(designations, special, "Passive", mood, null, "Past");

                    if ((mood == "Progressive") || (mood == "Unintentional") || (mood == "Probative"))
                    {
                        foreach (string mood2 in VerbCompoundMoods)
                        {
                            AddVerbMood(designations, special, "Passive", mood2, mood, "Present");

                            if (VerbMoodsWithPastArray.Contains(mood2))
                                AddVerbMood(designations, special, "Passive", mood2, mood, "Past");
                        }
                    }
                }
            }
        }

        private static void AddVerbMood(
            List<Designator> designations,
            string special,
            string special2,
            string mood,
            string mood2,
            string tense)
        {
            string[] politenessArray ;
            switch (mood)
            {
                case "Imperative":
                    politenessArray = VerbImperativePoliteness;
                    break;
                case "Request":
                    politenessArray = VerbRequestPoliteness;
                    break;
                case "Provisional":
                    politenessArray = VerbProvisionalPoliteness;
                    break;
                default:
                    politenessArray = PolitenessArray;
                    break;
            }
            foreach (string politeness in politenessArray)
            {
                foreach (string polarity in PolarityArray)
                {
                    string[] alternates;
                    switch (mood)
                    {
                        case "ContinuativeParticiple":
                            if ((politeness == "Plain") && (polarity == "Negative"))
                                alternates = Alternates2;
                            else
                                alternates = NoAlternates;
                            break;
                        case "Request":
                            if ((politeness == "Polite") && (polarity == "Positive"))
                                alternates = Alternates2;
                            else
                                alternates = NoAlternates;
                            break;
                        default:
                            alternates = NoAlternates;
                            break;
                    }
                    foreach (string alternate in alternates)
                    {
                        if ((mood == "ContinuativeInfinitive") && ((politeness != "Plain") || (polarity != "Positive")))
                            continue;
                        string label = String.Empty;
                        List<string> classifications = new List<string>();
                        if (!String.IsNullOrEmpty(special))
                        {
                            label += " " + special;
                            classifications.Add("Special");
                            classifications.Add(special);
                        }
                        if (!String.IsNullOrEmpty(special2))
                        {
                            label += " " + special2;
                            classifications.Add("Special");
                            classifications.Add(special2);
                        }
                        if (!String.IsNullOrEmpty(mood2))
                        {
                            label += " " + mood2;
                            classifications.Add("Mood");
                            classifications.Add(mood2);
                        }
                        if ((mood != "Indicative") || String.IsNullOrEmpty(special))
                        {
                            label += " " + mood;
                            classifications.Add("Mood");
                            classifications.Add(mood);
                        }
                        if (VerbMoodsWithPastArray.Contains(mood))
                        {
                            label += " " + tense;
                            classifications.Add("Tense");
                            classifications.Add(tense);
                        }
                        if (!String.IsNullOrEmpty(politeness))
                        {
                            label += " " + politeness;
                            classifications.Add("Politeness");
                            classifications.Add(politeness);
                        }
                        label += " " + polarity;
                        classifications.Add("Polarity");
                        classifications.Add(polarity);
                        if (!String.IsNullOrEmpty(alternate))
                        {
                            label += " " + alternate;
                            classifications.Add("Alternate");
                            classifications.Add(alternate);
                        }
                        Designator designation = new Designator(
                            label.Trim(),
                            classifications);
                        designations.Add(designation);

                        // Handle some contractions.
                        switch (mood)
                        {
                            case "Indicative":
                                if ((tense == "Present") && (politeness == "Plain"))
                                {
                                    if (polarity == "Negative")
                                    {
                                        AddVerbContraction(designations, label, classifications, "ContractionNaiToN");
                                        AddVerbContraction(designations, label, classifications, "ContractionNaiToNee");
                                        AddVerbContraction(designations, label, classifications, "ContractionRaNaiToNNai");
                                    }
                                    else
                                        AddVerbContraction(designations, label, classifications, "ContractionRuNoToNNo");
                                }
                                break;
                            case "Presumptive":
                                if ((tense == "Past") && (politeness == "Plain") && (polarity == "Positive"))
                                    AddVerbContraction(designations, label, classifications, "ContractionTadarouToTarou");
                                break;
                            case "ContinuativeParticiple":
                                if ((tense != "Past") && (politeness == "Plain") && (polarity == "Negative") &&
                                        (alternate == "Alternate2"))
                                    AddVerbContraction(designations, label, classifications, "ContractionNakuteWaToNakucha");
                                break;
                            case "Progressive":
                                if ((politeness == "Plain") && (tense == "Present") && (polarity == "Positive"))
                                {
                                    AddVerbContraction(designations, label, classifications, "ContractionTeIruToTeru");
                                    AddVerbContraction(designations, label, classifications, "ContractionDeIruToDeru");
                                    AddVerbContraction(designations, label, classifications, "ContractionTeIkuToTeku");
                                    AddVerbContraction(designations, label, classifications, "ContractionDeIkuToDeku");
                                    AddVerbContraction(designations, label, classifications, "ContractionTeOkuToToku");
                                    AddVerbContraction(designations, label, classifications, "ContractionTeAgeruToTageru");
                                }
                                break;
                            case "Unintentional":
                                AddVerbContraction(designations, label, classifications, "ContractionTeShimauToChau");
                                AddVerbContraction(designations, label, classifications, "ContractionTeShimauToChimau");
                                AddVerbContraction(designations, label, classifications, "ContractionDeShimauToJimau1");
                                AddVerbContraction(designations, label, classifications, "ContractionDeShimauToJimau2");
                                break;
                            case "Provisional":
                                if (polarity == "Negative")
                                    AddVerbContraction(designations, label, classifications, "ContractionNakerebaToNakya");
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private static void AddVerbContraction(
            List<Designator> designations,
            string label,
            List<string> classifications,
            string contraction)
        {
            label += " " + contraction;
            classifications = new List<string>(classifications);
            classifications.Add("Contraction");
            classifications.Add(contraction);
            Designator designation = new Designator(
                label.Trim(),
                classifications);
            designations.Add(designation);
        }

        public override List<Designator> GetAllVerbDesignations()
        {
            return AllVerbDesignations;
        }

        public override List<Designator> GetDefaultVerbDesignations()
        {
            return DefaultVerbDesignations;
        }

        public override Designator GetCanonicalDesignation(
            Designator designation,
            LanguageTool toLanguageTool)
        {
            Designator canonicalDesignation = null;

            int progressiveIndex = designation.GetClassificationIndexWithValue("Mood", "Progressive");

            if (progressiveIndex != -1)
            {
                canonicalDesignation = new Designator(designation.Label, designation.CloneClassifications());
                canonicalDesignation.GetClassificationIndexed(progressiveIndex).Key = "Aspect";
            }

            return canonicalDesignation;
        }
    }
}
