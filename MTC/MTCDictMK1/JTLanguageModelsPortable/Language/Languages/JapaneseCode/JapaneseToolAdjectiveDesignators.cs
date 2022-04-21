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
        private static string[] AdjectiveMoodArray =
        {
            "Indicative",
            "Presumptive",
            "ContinuativeParticiple",
            "Provisional",
            "Conditional",
            "Alternative"
        };

        private static string[] AdjectiveMoodsWithPastArray =
        {
            "Indicative",
            "Presumptive"
        };

        private static List<Designator> _AllAdjectiveDesignations = null;

        public static List<Designator> AllAdjectiveDesignations
        {
            get
            {
                if (_AllAdjectiveDesignations != null)
                    return _AllAdjectiveDesignations;

                _AllAdjectiveDesignations = new List<Designator>();

                AddAdjectiveDesignations(_AllAdjectiveDesignations);

                return _AllAdjectiveDesignations;
            }
        }

        private static List<Designator> _DefaultAdjectiveDesignations = null;

        public static List<Designator> DefaultAdjectiveDesignations
        {
            get
            {
                if (_DefaultAdjectiveDesignations != null)
                    return _DefaultAdjectiveDesignations;

                _DefaultAdjectiveDesignations = new List<Designator>();

                AddAdjectiveDesignations(_DefaultAdjectiveDesignations);

                return _DefaultAdjectiveDesignations;
            }
        }

        private static void AddAdjectiveDesignations(List<Designator> designations)
        {
            AddAdjectiveDesignation(designations, "Dictionary", null, null, null, null, null);
            AddAdjectiveDesignation(designations, "Stem", null, null, "Plain", null, null);
            AddAdjectiveDesignation(designations, "Stem", null, null, "Polite", null, null);
            AddAdjectiveDesignation(designations, "Adverbial", null, null, null, null, null);
            AddAdjectiveDesignation(designations, "Noun", null, null, null, null, null);
            AddSubAdjectiveDesignations(designations, null);
        }

        private static void AddSubAdjectiveDesignations(List<Designator> designations, string special)
        {
            AddAdjectiveDesignation(designations, special, "Indicative", "Present", "Plain", "Positive", null);
            AddAdjectiveDesignation(designations, special, "Indicative", "Present", "Plain", "Negative", null);
            AddAdjectiveDesignation(designations, special, "Indicative", "Present", "Polite", "Positive", "Desu");
            AddAdjectiveDesignation(designations, special, "Indicative", "Present", "Polite", "Positive", "Gozaimasu");
            AddAdjectiveDesignation(designations, special, "Indicative", "Present", "Polite", "Negative", "Desu");
            AddAdjectiveDesignation(designations, special, "Indicative", "Present", "Polite", "Negative", "Arimasu");
            AddAdjectiveDesignation(designations, special, "Indicative", "Present", "Polite", "Negative", "Gozaimasu");
            AddAdjectiveDesignation(designations, special, "Indicative", "Past", "Plain", "Positive", null);
            AddAdjectiveDesignation(designations, special, "Indicative", "Past", "Plain", "Negative", null);
            AddAdjectiveDesignation(designations, special, "Indicative", "Past", "Polite", "Positive", "Desu");
            AddAdjectiveDesignation(designations, special, "Indicative", "Past", "Polite", "Positive", "Arimasu");
            AddAdjectiveDesignation(designations, special, "Indicative", "Past", "Polite", "Positive", "Gozaimasu");
            AddAdjectiveDesignation(designations, special, "Indicative", "Past", "Polite", "Negative", "Desu");
            AddAdjectiveDesignation(designations, special, "Indicative", "Past", "Polite", "Negative", "Arimasu");
            AddAdjectiveDesignation(designations, special, "Indicative", "Past", "Polite", "Negative", "Gozaimasu");

            AddAdjectiveDesignation(designations, special, "Presumptive", "Present", "Plain", "Positive", null);
            AddAdjectiveDesignation(designations, special, "Presumptive", "Present", "Plain", "Negative", null);
            AddAdjectiveDesignation(designations, special, "Presumptive", "Present", "Polite", "Positive", "Arimasu");
            AddAdjectiveDesignation(designations, special, "Presumptive", "Present", "Polite", "Positive", "Gozaimasu");
            AddAdjectiveDesignation(designations, special, "Presumptive", "Present", "Polite", "Negative", "Arimasu");
            AddAdjectiveDesignation(designations, special, "Presumptive", "Present", "Polite", "Negative", "Gozaimasu");
            AddAdjectiveDesignation(designations, special, "Presumptive", "Past", "Plain", "Positive", null);
            AddAdjectiveDesignation(designations, special, "Presumptive", "Past", "Plain", "Negative", null);
            AddAdjectiveDesignation(designations, special, "Presumptive", "Past", "Polite", "Positive", null);
            AddAdjectiveDesignation(designations, special, "Presumptive", "Past", "Polite", "Negative", null);

            AddAdjectiveDesignation(designations, special, "Probable", "Present", "Plain", "Positive", null);
            AddAdjectiveDesignation(designations, special, "Probable", "Present", "Plain", "Negative", null);
            AddAdjectiveDesignation(designations, special, "Probable", "Present", "Polite", "Positive", null);
            AddAdjectiveDesignation(designations, special, "Probable", "Present", "Polite", "Negative", null);
            AddAdjectiveDesignation(designations, special, "Probable", "Past", "Plain", "Positive", null);
            AddAdjectiveDesignation(designations, special, "Probable", "Past", "Plain", "Negative", null);
            AddAdjectiveDesignation(designations, special, "Probable", "Past", "Polite", "Positive", null);
            AddAdjectiveDesignation(designations, special, "Probable", "Past", "Polite", "Negative", null);

            AddAdjectiveDesignation(designations, special, "Provisional", null, "Plain", "Positive", null);
            AddAdjectiveDesignation(designations, special, "Provisional", null, "Plain", "Negative", null);

            AddAdjectiveDesignation(designations, special, "ContinuativeParticiple", null, "Plain", "Positive", null);
            AddAdjectiveDesignation(designations, special, "ContinuativeParticiple", null, "Plain", "Negative", null);
            AddAdjectiveDesignation(designations, special, "ContinuativeParticiple", null, "Polite", "Positive", null);
            AddAdjectiveDesignation(designations, special, "ContinuativeParticiple", null, "Polite", "Negative", null);

            AddAdjectiveDesignation(designations, special, "Conditional", null, "Plain", "Positive", null);
            AddAdjectiveDesignation(designations, special, "Conditional", null, "Plain", "Negative", null);
            AddAdjectiveDesignation(designations, special, "Conditional", null, "Polite", "Positive", null);
            AddAdjectiveDesignation(designations, special, "Conditional", null, "Polite", "Negative", null);

            AddAdjectiveDesignation(designations, special, "Alternative", null, "Plain", "Positive", null);
            AddAdjectiveDesignation(designations, special, "Alternative", null, "Plain", "Negative", null);
            AddAdjectiveDesignation(designations, special, "Alternative", null, "Polite", "Positive", null);
            AddAdjectiveDesignation(designations, special, "Alternative", null, "Polite", "Negative", null);
        }

        private static void AddAdjectiveDesignation(
                List<Designator> designations,
                string special,
                string mood,
                string tense,
                string politeness,
                string polarity,
                string alternate)
        {
            string label = String.Empty;
            List<string> classifications = new List<string>();
            if (!String.IsNullOrEmpty(special))
            {
                label += " " + special;
                classifications.Add("Special");
                classifications.Add(special);
            }
            if (!String.IsNullOrEmpty(mood))
            {
                label += " " + mood;
                classifications.Add("Mood");
                classifications.Add(mood);
            }
            if (!String.IsNullOrEmpty(tense))
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
            if (!String.IsNullOrEmpty(polarity))
            {
                label += " " + polarity;
                classifications.Add("Polarity");
                classifications.Add(polarity);
            }
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
        }

        public override List<Designator> GetAllAdjectiveDesignations()
        {
            return AllAdjectiveDesignations;
        }

        public override List<Designator> GetDefaultAdjectiveDesignations()
        {
            return DefaultAdjectiveDesignations;
        }
    }
}
