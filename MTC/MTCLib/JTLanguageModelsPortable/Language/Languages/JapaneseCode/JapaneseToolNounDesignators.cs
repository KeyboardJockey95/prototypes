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
        private static string[] NounMoodArray =
        {
            "Indicative",
            "Continuative",
            "Prenomial",
            "Presumptive",
            "Provisional",
            "Conditional",
            "Alternative"
        };

        private static string[] NounMoodsWithPastArray =
        {
            "Indicative",
            "Presumptive"
        };

        private static List<Designator> _AllNounDesignations = null;

        public static List<Designator> AllNounDesignations
        {
            get
            {
                if (_AllNounDesignations != null)
                    return _AllNounDesignations;

                _AllNounDesignations = new List<Designator>();

                AddNounDesignations(_AllNounDesignations);

                return _AllNounDesignations;
            }
        }

        private static List<Designator> _DefaultNounDesignations = null;

        public static List<Designator> DefaultNounDesignations
        {
            get
            {
                if (_DefaultNounDesignations != null)
                    return _DefaultNounDesignations;

                _DefaultNounDesignations = new List<Designator>();

                AddNounDesignations(_DefaultNounDesignations);

                return _DefaultNounDesignations;
            }
        }

        private static void AddNounDesignations(List<Designator> designations)
        {
            AddNounDesignation(designations, "Indicative", "Present", "Plain", "Positive", null, null);
            AddNounDesignation(designations, "Indicative", "Present", "Plain", "Positive", "Arimasu", null);
            AddNounDesignation(designations, "Indicative", "Present", "Plain", "Negative", null, null);
            AddNounDesignation(designations, "Indicative", "Present", "Plain", "Negative", null, "ContractionDeWaToJa");
            AddNounDesignation(designations, "Indicative", "Present", "Polite", "Positive", null, null);
            AddNounDesignation(designations, "Indicative", "Present", "Polite", "Positive", "Arimasu", null);
            AddNounDesignation(designations, "Indicative", "Present", "Polite", "Negative", null, null);
            AddNounDesignation(designations, "Indicative", "Present", "Polite", "Negative", null, "ContractionDeWaToJa");
            AddNounDesignation(designations, "Indicative", "Present", "Polite", "Negative", "Nai", null);
            AddNounDesignation(designations, "Indicative", "Present", "Honorific", "Positive", null, null);
            AddNounDesignation(designations, "Indicative", "Present", "Honorific", "Negative", null, null);
            AddNounDesignation(designations, "Indicative", "Present", "Honorific", "Negative", null, "ContractionDeWaToJa");

            AddNounDesignation(designations, "Indicative", "Past", "Plain", "Positive", null, null);
            AddNounDesignation(designations, "Indicative", "Past", "Plain", "Positive", "Arimasu", null);
            AddNounDesignation(designations, "Indicative", "Past", "Plain", "Negative", null, null);
            AddNounDesignation(designations, "Indicative", "Past", "Plain", "Negative", null, "ContractionDeWaToJa");
            AddNounDesignation(designations, "Indicative", "Past", "Polite", "Positive", null, null);
            AddNounDesignation(designations, "Indicative", "Past", "Polite", "Positive", "Arimasu", null);
            AddNounDesignation(designations, "Indicative", "Past", "Polite", "Negative", "Arimasu", null);
            AddNounDesignation(designations, "Indicative", "Past", "Polite", "Negative", "Arimasu", "ContractionDeWaToJa");
            AddNounDesignation(designations, "Indicative", "Past", "Polite", "Negative", "Nak", "ContractionDeWaToJa");
            AddNounDesignation(designations, "Indicative", "Past", "Honorific", "Positive", null, null);
            AddNounDesignation(designations, "Indicative", "Past", "Honorific", "Negative", null, null);
            AddNounDesignation(designations, "Indicative", "Past", "Honorific", "Negative", null, "ContractionDeWaToJa");

            AddNounDesignation(designations, "Continuative", null, "Plain", "Positive", null, null);
            AddNounDesignation(designations, "Continuative", null, "Plain", "Positive", "Arimasu", null);
            AddNounDesignation(designations, "Continuative", null, "Plain", "Negative", null, null);
            AddNounDesignation(designations, "Continuative", null, "Plain", "Negative", null, "ContractionDeWaToJa");
            AddNounDesignation(designations, "Continuative", null, "Polite", "Positive", null, null);
            AddNounDesignation(designations, "Continuative", null, "Polite", "Positive", "Arimasu", null);
            AddNounDesignation(designations, "Continuative", null, "Polite", "Negative", null, null);
            AddNounDesignation(designations, "Continuative", null, "Polite", "Negative", null, "ContractionDeWaToJa");
            AddNounDesignation(designations, "Continuative", null, "Honorific", "Positive", null, null);
            AddNounDesignation(designations, "Continuative", null, "Honorific", "Negative", null, null);
            AddNounDesignation(designations, "Continuative", null, "Honorific", "Negative", null, "ContractionDeWaToJa");

            AddNounDesignation(designations, "Prenomial", null, "Plain", "Positive", "Arimasu", null);
            AddNounDesignation(designations, "Prenomial", null, "Plain", "Negative", null, null);
            AddNounDesignation(designations, "Prenomial", null, "Plain", "Negative", null, "ContractionDeWaToJa");

            AddNounDesignation(designations, "Presumptive", "Present", "Plain", "Positive", null, null);
            AddNounDesignation(designations, "Presumptive", "Present", "Plain", "Positive", "Arimasu", null);
            AddNounDesignation(designations, "Presumptive", "Present", "Plain", "Negative", "Nak", null);
            AddNounDesignation(designations, "Presumptive", "Present", "Plain", "Negative", "Nak", "ContractionDeWaToJa");
            AddNounDesignation(designations, "Presumptive", "Present", "Plain", "Negative", "Nai", null);
            AddNounDesignation(designations, "Presumptive", "Present", "Plain", "Negative", "Nai", "ContractionDeWaToJa");
            AddNounDesignation(designations, "Presumptive", "Present", "Polite", "Positive", null, null);
            AddNounDesignation(designations, "Presumptive", "Present", "Polite", "Positive", "Arimasu", null);
            AddNounDesignation(designations, "Presumptive", "Present", "Polite", "Negative", "Nai", null);
            AddNounDesignation(designations, "Presumptive", "Present", "Polite", "Negative", "Nai", "ContractionDeWaToJa");
            AddNounDesignation(designations, "Presumptive", "Present", "Polite", "Negative", "Arimasu", null);
            AddNounDesignation(designations, "Presumptive", "Present", "Polite", "Negative", "Arimasu", "ContractionDeWaToJa");
            AddNounDesignation(designations, "Presumptive", "Present", "Honorific", "Positive", null, null);
            AddNounDesignation(designations, "Presumptive", "Present", "Honorific", "Negative", null, null);
            AddNounDesignation(designations, "Presumptive", "Present", "Honorific", "Negative", null, "ContractionDeWaToJa");

            AddNounDesignation(designations, "Presumptive", "Past", "Plain", "Positive", null, null);
            AddNounDesignation(designations, "Presumptive", "Past", "Plain", "Positive", "Arimasu", null);
            AddNounDesignation(designations, "Presumptive", "Past", "Plain", "Negative", null, null);
            AddNounDesignation(designations, "Presumptive", "Past", "Plain", "Negative", null, "ContractionDeWaToJa");
            AddNounDesignation(designations, "Presumptive", "Past", "Plain", "Negative", "Desu", null);
            AddNounDesignation(designations, "Presumptive", "Past", "Plain", "Negative", "Desu", "ContractionDeWaToJa");
            AddNounDesignation(designations, "Presumptive", "Past", "Polite", "Positive", null, null);
            AddNounDesignation(designations, "Presumptive", "Past", "Polite", "Positive", "Desu", null);
            AddNounDesignation(designations, "Presumptive", "Past", "Polite", "Positive", "Arimasu", null);
            AddNounDesignation(designations, "Presumptive", "Past", "Polite", "Negative", "Nak", null);
            AddNounDesignation(designations, "Presumptive", "Past", "Polite", "Negative", "Nak", "ContractionDeWaToJa");
            AddNounDesignation(designations, "Presumptive", "Past", "Polite", "Negative", "Arimasu", null);
            AddNounDesignation(designations, "Presumptive", "Past", "Honorific", "Positive", null, null);
            AddNounDesignation(designations, "Presumptive", "Past", "Honorific", "Negative", null, null);
            AddNounDesignation(designations, "Presumptive", "Past", "Honorific", "Negative", null, "ContractionDeWaToJa");

            AddNounDesignation(designations, "Provisional", null, "Plain", "Positive", "Alternate1", null);
            AddNounDesignation(designations, "Provisional", null, "Plain", "Positive", "Alternate2", null);
            AddNounDesignation(designations, "Provisional", null, "Plain", "Negative", null, null);
            AddNounDesignation(designations, "Provisional", null, "Plain", "Negative", null, "ContractionDeWaToDe");
            AddNounDesignation(designations, "Provisional", null, "Plain", "Negative", null, "ContractionDeWaToJa");
            AddNounDesignation(designations, "Provisional", null, "Plain", "Negative", "Alternate1", "ContractionDeWaToJa");
            AddNounDesignation(designations, "Provisional", null, "Plain", "Negative", "Alternate2", "ContractionDeWaToJa");
            AddNounDesignation(designations, "Provisional", null, "Polite", "Positive", null, null);
            AddNounDesignation(designations, "Provisional", null, "Polite", "Negative", null, "ContractionDeWaToJa");

            AddNounDesignation(designations, "Conditional", null, "Plain", "Positive", null, null);
            AddNounDesignation(designations, "Conditional", null, "Plain", "Positive", "Arimasu", null);
            AddNounDesignation(designations, "Conditional", null, "Plain", "Negative", null, null);
            AddNounDesignation(designations, "Conditional", null, "Plain", "Negative", null, "ContractionDeWaToDe");
            AddNounDesignation(designations, "Conditional", null, "Plain", "Negative", null, "ContractionDeWaToJa");
            AddNounDesignation(designations, "Conditional", null, "Polite", "Positive", null, null);
            AddNounDesignation(designations, "Conditional", null, "Polite", "Positive", "Arimasu", null);
            AddNounDesignation(designations, "Conditional", null, "Polite", "Negative", null, null);
            AddNounDesignation(designations, "Conditional", null, "Polite", "Negative", null, "ContractionDeWaToDe");
            AddNounDesignation(designations, "Conditional", null, "Polite", "Negative", null, "ContractionDeWaToJa");
            AddNounDesignation(designations, "Conditional", null, "Honorific", "Positive", null, null);
            AddNounDesignation(designations, "Conditional", null, "Honorific", "Negative", null, null);
            AddNounDesignation(designations, "Conditional", null, "Honorific", "Negative", null, "ContractionDeWaToDe");
            AddNounDesignation(designations, "Conditional", null, "Honorific", "Negative", null, "ContractionDeWaToJa");

            AddNounDesignation(designations, "Alternative", null, "Plain", "Positive", null, null);
            AddNounDesignation(designations, "Alternative", null, "Plain", "Positive", "Arimasu", null);
            AddNounDesignation(designations, "Alternative", null, "Plain", "Negative", null, null);
            AddNounDesignation(designations, "Alternative", null, "Plain", "Negative", null, "ContractionDeWaToDe");
            AddNounDesignation(designations, "Alternative", null, "Plain", "Negative", null, "ContractionDeWaToJa");
            AddNounDesignation(designations, "Alternative", null, "Polite", "Positive", null, null);
            AddNounDesignation(designations, "Alternative", null, "Polite", "Positive", "Arimasu", null);
            AddNounDesignation(designations, "Alternative", null, "Honorific", "Positive", null, null);
        }

        private static void AddNounDesignation(
                List<Designator> designations,
                string mood,
                string tense,
                string politeness,
                string polarity,
                string alternate,
                string contraction)
        {
            string label = String.Empty;
            List<string> classifications = new List<string>();
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
            if (!String.IsNullOrEmpty(contraction))
            {
                label += " " + contraction;
                classifications.Add("Contraction");
                classifications.Add(contraction);
            }
            Designator designation = new Designator(
                label.Trim(),
                classifications);
            designations.Add(designation);
        }

        public override List<Designator> GetAllNounDesignations()
        {
            return AllNounDesignations;
        }

        public override List<Designator> GetDefaultNounDesignations()
        {
            return DefaultNounDesignations;
        }
    }
}
