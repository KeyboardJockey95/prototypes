using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public class SpanishTool : LanguageTool
    {
        // Endings.
        public const string VerbEndingAr = "ar";
        public const string VerbEndingEr = "er";
        public const string VerbEndingIr = "ir";
        public const string VerbEndingArse = "arse";
        public const string VerbEndingErse = "erse";
        public const string VerbEndingIrse = "irse";
        public const string NounOrAdjectiveEndingO = "o";
        public const string NounOrAdjectiveEndingOs = "os";
        public const string NounOrAdjectiveEndingA = "a";
        public const string NounOrAdjectiveEndingAs = "as";
        public const string NounOrAdjectiveEndingCion = "ción";
        public const string NounOrAdjectiveEndingCiones = "ciónes";
        public const string NounOrAdjectiveEndingSion = "sión";
        public const string NounOrAdjectiveEndingSiones = "siónes";
        public const string NounOrAdjectiveEndingTad = "tad";
        public const string NounOrAdjectiveEndingTades = "tades";
        public const string NounOrAdjectiveEndingDad = "dad";
        public const string NounOrAdjectiveEndingDades = "dades";
        public const string NounOrAdjectiveEndingTud = "tud";
        public const string NounOrAdjectiveEndingTudes = "tudes";
        public const string NounOrAdjectiveEndingUmbre = "umbre";
        public const string NounOrAdjectiveEndingUmbres = "umbres";

        public string[] SpanishClassEndings = new string[]
        {
            VerbEndingAr,
            VerbEndingEr,
            VerbEndingIr,
            NounOrAdjectiveEndingO,
            NounOrAdjectiveEndingOs,
            NounOrAdjectiveEndingA,
            NounOrAdjectiveEndingAs,
            NounOrAdjectiveEndingCion,
            NounOrAdjectiveEndingCiones,
            NounOrAdjectiveEndingSion,
            NounOrAdjectiveEndingSiones,
            NounOrAdjectiveEndingTad,
            NounOrAdjectiveEndingTades,
            NounOrAdjectiveEndingDad,
            NounOrAdjectiveEndingDades,
            NounOrAdjectiveEndingTud,
            NounOrAdjectiveEndingTudes,
            NounOrAdjectiveEndingUmbre,
            NounOrAdjectiveEndingUmbres
        };

        public string[] SpanishVerbClassEndings =
        {
            VerbEndingAr,
            VerbEndingEr,
            VerbEndingIr
        };

        public override string[] VerbClassEndings
        {
            get
            {
                return SpanishVerbClassEndings;
            }
        }

        public string[] SpanishNonVerbClassEndings =
        {
            NounOrAdjectiveEndingO,
            NounOrAdjectiveEndingOs,
            NounOrAdjectiveEndingA,
            NounOrAdjectiveEndingAs,
            NounOrAdjectiveEndingCion,
            NounOrAdjectiveEndingCiones,
            NounOrAdjectiveEndingSion,
            NounOrAdjectiveEndingSiones,
            NounOrAdjectiveEndingTad,
            NounOrAdjectiveEndingTades,
            NounOrAdjectiveEndingDad,
            NounOrAdjectiveEndingDades,
            NounOrAdjectiveEndingTud,
            NounOrAdjectiveEndingTudes,
            NounOrAdjectiveEndingUmbre,
            NounOrAdjectiveEndingUmbres
        };

        public override string[] NonVerbClassEndings
        {
            get
            {
                return SpanishNonVerbClassEndings;
            }
        }

        public static List<string> EndingsTableSourceSpanish = new List<string>()
        {
            "amar",         // v
            "hablar",       // vi
            "encontrar",    // vir
            "practicar",    // vt, vit
            "apurar",       // vtr, vitr
            "abonar",       // vi, vp, vt
            "agradecer",    // v
            "amanecer",     // vi, vr
            "temer",        // vt
            "descomponer",  // vtr
            "atrever",      // vp
            "partir",       // v
            "advenir",      // vi
            "abolir",       // vt
            "revenir",      // vp
            null
        };

        public static string[] SpanishCommonNonInflectableWords =
        {
            "no",
            "el",
            "la",
            "las",
            "una",
            "un",
            "ninguno",
            "ninguna",
            "nada",
            "nunca",
            "los",
            "aquellos",
            "aquí",
            "allí",
            "y",
            "pero",
            "a",
            "en",
            "de",
            "por",
            "para",
            "porque",
            "como",
            "si",
            "que",
            "qué",
            "cuando",
            "dónde",
            "cómo",
            "yo",
            "tú",
            "usted",
            "él",
            "ella",
            "ellas",
            "ellos",
            "nosotros",
            "vosotros",
            "ellos",
            "ellas",
            "me",
            "te",
            "se",
            "le",
            "lo",
            "nos",
            "vos",
            "les",
            "mi",
            "ti",
            "tu",
            "su",
            "nuestro",
            "nuestra",
            "vuestro",
            "vuestra"
        };

        public override string[] CommonNonInflectableWords
        {
            get
            {
                return SpanishCommonNonInflectableWords;
            }
        }

        public SpanishTool() : base(LanguageLookup.Spanish)
        {
            SetCanInflect("Verb", true);
            SetCanInflect("Adjective", true);
            SetCanInflect("Noun", true);
            SetCanInflect("Unknown", false);
            CanDeinflect = true;
            UseFileBasedEndingsTable = false;
            _UsesImplicitPronouns = true;
            //EndingsTableSource = EndingsTableSourceSpanish;
        }

        public override IBaseObject Clone()
        {
            return new SpanishTool();
        }

        public override bool GetWordClassCategoryStringAndCodes(
            string word,
            LexicalCategory category,
            out string categoryString,
            out string classCode,
            out string subClassCode)
        {
            categoryString = String.Empty;
            classCode = String.Empty;
            subClassCode = String.Empty;

            if (String.IsNullOrEmpty(word))
                return false;

            string ending = null;
            bool returnValue = false;

            switch (category)
            {
                case LexicalCategory.Adjective:
                    ending = word.Substring(word.Length - 1);
                    if (SpanishNonVerbClassEndings.Contains(ending))
                    {
                        classCode = GetClassCode(word, ending);
                        subClassCode = GetSubCode(word, ending);
                        categoryString = "adj";
                        if (!String.IsNullOrEmpty(subClassCode))
                            categoryString += "," + subClassCode;
                        if (!String.IsNullOrEmpty(classCode))
                            categoryString += "," + classCode;
                        returnValue = true;
                    }
                    break;
                case LexicalCategory.Noun:
                    foreach (string suffix in SpanishNonVerbClassEndings)
                    {
                        if (word.EndsWith(suffix))
                        {
                            ending = suffix;
                            break;
                        }
                    }
                    if (ending != null)
                    {
                        classCode = GetClassCode(word, ending);
                        subClassCode = GetSubCode(word, ending);
                        categoryString = "n";
                        if (!String.IsNullOrEmpty(subClassCode))
                            categoryString += "," + subClassCode;
                        if (!String.IsNullOrEmpty(classCode))
                            categoryString += "," + classCode;
                        returnValue = true;
                    }
                    break;
                case LexicalCategory.Verb:
                    ending = word.Substring(word.Length - 2);
                    if (SpanishVerbClassEndings.Contains(ending))
                    {
                        classCode = ending;
                        categoryString = "v," + classCode;
                        returnValue = true;
                    }
                    else if (ending == "se")
                    {
                        ending = word.Substring(word.Length - 4, 2);
                        if (SpanishVerbClassEndings.Contains(ending))
                        {
                            classCode = ending;
                            categoryString = "v," + classCode + ",reflexive";
                            returnValue = true;
                        }
                    }
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        protected string GetClassCode(string word, string ending)
        {
            string subCode = String.Empty;

            switch (ending.ToLower())
            {
                case "o":
                    subCode = "o";
                    break;
                case "a":
                    subCode = "a";
                    break;
                case "os":
                    subCode = "o";
                    break;
                case "as":
                    subCode = "a";
                    break;
                case "ción":
                    subCode = "ción";
                    break;
                case "ciónes":
                    subCode = "ción";
                    break;
                case "tad":
                    subCode = "tad";
                    break;
                case "tades":
                    subCode = "tad";
                    break;
                case "dad":
                    subCode = "dad";
                    break;
                case "dades":
                    subCode = "dad";
                    break;
                case "tud":
                    subCode = "tud";
                    break;
                case "tudes":
                    subCode = "tud";
                    break;
                case "umbre":
                    subCode = "umbre";
                    break;
                case "umbres":
                    subCode = "umbre";
                    break;
                default:
                    break;
            }

            return subCode;
        }

        protected string GetSubCode(string word, string ending)
        {
            string subCode = String.Empty;

            switch (ending.ToLower())
            {
                case "o":
                    subCode = "m,s";
                    break;
                case "a":
                    subCode = "f,s";
                    break;
                case "os":
                    subCode = "m,p";
                    break;
                case "as":
                    subCode = "f,p";
                    break;
                case "ción":
                    subCode = "f,s";
                    break;
                case "ciónes":
                    subCode = "f,p";
                    break;
                case "tad":
                    subCode = "f,s";
                    break;
                case "tades":
                    subCode = "f,p";
                    break;
                case "dad":
                    subCode = "f,s";
                    break;
                case "dades":
                    subCode = "f,p";
                    break;
                case "tud":
                    subCode = "f,s";
                    break;
                case "tudes":
                    subCode = "f,p";
                    break;
                case "umbre":
                    subCode = "f,s";
                    break;
                case "umbres":
                    subCode = "f,p";
                    break;
                default:
                    break;
            }

            return subCode;
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

            if (languageID != LanguageID)
                return null;

            if (word.EndsWith("se"))
                word = word.Substring(0, word.Length - 2);

            string ending = null;
            string stem = null;

            foreach (string suffix in SpanishClassEndings)
            {
                if (word.EndsWith(suffix))
                {
                    ending = suffix;
                    stem = word.Substring(0, word.Length - suffix.Length);
                    break;
                }
            }

            switch (ending)
            {
                case "ar":
                    categoryString = ending;
                    classCode = "ar";
                    break;
                case "er":
                    categoryString = ending;
                    classCode = "er";
                    break;
                case "ir":
                    categoryString = ending;
                    classCode = "ir";
                    break;
                case "ír":
                    categoryString = "ir";
                    classCode = "ir";
                    break;
                case "o":
                    categoryString = "m,s";
                    classCode = "o";
                    break;
                case "a":
                    categoryString = "f,s";
                    classCode = "a";
                    break;
                case "os":
                    categoryString = "m,p";
                    classCode = "o";
                    break;
                case "as":
                    categoryString = "f,p";
                    classCode = "a";
                    break;
                case "ción":
                    categoryString = "f,s,ción";
                    classCode = "";
                    break;
                case "ciónes":
                    categoryString = "f,p,ción";
                    classCode = "";
                    break;
                case "sión":
                    categoryString = "f,s,sión";
                    classCode = "";
                    break;
                case "siónes":
                    categoryString = "f,p,sión";
                    classCode = "";
                    break;
                case "tad":
                    categoryString = "f,s,tad";
                    classCode = "";
                    break;
                case "tades":
                    categoryString = "f,p,tad";
                    classCode = "";
                    break;
                case "dad":
                    categoryString = "f,s,dad";
                    classCode = "";
                    break;
                case "dades":
                    categoryString = "f,p,dad";
                    classCode = "";
                    break;
                case "tud":
                    categoryString = "f,s,tud";
                    classCode = "";
                    break;
                case "tudes":
                    categoryString = "f,p,tud";
                    classCode = "";
                    break;
                case "umbre":
                    categoryString = "f,s,umbre";
                    classCode = "";
                    break;
                case "umbres":
                    categoryString = "f,p,umbre";
                    classCode = "";
                    break;
                default:
                    stem = null;
                    break;
            }

            return stem;
        }

        public override bool InferCategoryFromWord(
            string word,
            out LexicalCategory category,
            out string categoryString)
        {
            category = LexicalCategory.Unknown;
            categoryString = null;

            if (String.IsNullOrEmpty(word))
                return false;

            if (word.EndsWith("se"))
                word = word.Substring(0, word.Length - 2);

            foreach (string ending in SpanishVerbClassEndings)
            {
                if (word.EndsWith(ending))
                {
                    category = LexicalCategory.Verb;
                    categoryString = ending;
                    return true;
                }
            }

            foreach (string ending in SpanishNonVerbClassEndings)
            {
                if (word.EndsWith(ending))
                {
                    category = LexicalCategory.Noun;
                    categoryString = ending;
                    return true;
                }
            }

            return false;
        }

        public override LexicalCategory GetCategoryFromCategoryString(string categoryString)
        {
            if (String.IsNullOrEmpty(categoryString))
                return LexicalCategory.Unknown;
            string rawCategory;
            int ofs = categoryString.IndexOf(',');
            if (ofs == -1)
                rawCategory = categoryString;
            else
                rawCategory = categoryString.Substring(0, ofs);
            LexicalCategory category = GetCategoryFromRawCategoryEs(rawCategory, String.Empty);
            return category;
        }

        public static LexicalCategory GetCategoryFromRawCategoryEs(string rawCategory, string cValue)
        {
            LexicalCategory category;

            // Works for both THT and Lingoes dictionary.
            switch (rawCategory)
            {
                // Lingoes
                case "ar":
                case "er":
                case "ir":
                    category = LexicalCategory.Verb;
                    break;
                // default has to handle any other Lingoes case.

                // THT
                case "f":
                case "fp":
                case "m":
                case "mf":
                case "mfp":
                case "mp":
                case "n":
                case "p":
                case "vm":
                case "letter":
                    category = LexicalCategory.Noun;
                    break;
                case "prop":
                case "propf":
                case "propm":
                    category = LexicalCategory.ProperNoun;
                    break;
                case "pron":
                    category = LexicalCategory.Pronoun;
                    break;
                case "determiner":
                    category = LexicalCategory.Determiner;
                    break;
                case "adj":
                case "adjf":
                case "adjm":
                case "adjmf":
                    category = LexicalCategory.Adjective;
                    break;
                case "v":
                case "vi":
                case "vir":
                case "vit":
                case "vitr":
                case "vp":
                case "vr":
                case "vrr":
                case "vt":
                case "vtr":
                    category = LexicalCategory.Verb;
                    break;
                case "adv":
                case "advm":
                case "advf":
                case "advmf":
                    category = LexicalCategory.Adverb;
                    break;
                case "prep":
                    category = LexicalCategory.Preposition;
                    break;
                case "conj":
                    category = LexicalCategory.Conjunction;
                    break;
                case "interj":
                    category = LexicalCategory.Interjection;
                    break;
                case "particle":
                    category = LexicalCategory.Particle;
                    break;
                case "art":
                    category = LexicalCategory.Article;
                    break;
                case "meas":        // Not used.
                    category = LexicalCategory.MeasureWord;
                    break;
                case "num":
                case "numm":
                case "numf":
                case "cardinal num":
                    category = LexicalCategory.Number;
                    break;
                case "abbr":
                case "abbrm":
                case "abbrf":
                    category = LexicalCategory.Abbreviation;
                    break;
                case "acronym":
                    category = LexicalCategory.Acronym;
                    break;
                case "symbol":
                case "affix":
                    category = LexicalCategory.Symbol;
                    break;
                case "initialism":
                case "initialismm":
                case "initialismf":
                    if (TextUtilities.IsUpperString(cValue))
                        category = LexicalCategory.Acronym;
                    else
                        category = LexicalCategory.Phrase;
                    break;
                case "phrase":
                    category = LexicalCategory.Phrase;
                    break;
                case "prep phrase":
                    category = LexicalCategory.PrepositionalPhrase;
                    break;
                case "proverb":
                    category = LexicalCategory.Proverb;
                    break;
                case "contraction":
                    category = LexicalCategory.Contraction;
                    break;
                case "idiom":       // Not used.
                    category = LexicalCategory.Idiom;
                    break;
                case "stem":        // Not used.
                    category = LexicalCategory.Stem;
                    break;
                case "inflection":  // Not used.
                    category = LexicalCategory.Inflection;
                    break;
                case "prefix":
                    category = LexicalCategory.Prefix;
                    break;
                case "suffix":
                    category = LexicalCategory.Suffix;
                    break;
                case "not found":   // Not used.
                    category = LexicalCategory.NotFound;
                    break;

                default:
                    category = LexicalCategory.Unknown;
                    break;
            }

            return category;
        }

        public override bool FixupProbableMeaningCategories(
            ProbableMeaning probableMeaning,
            LanguageID languageID)
        {
            LexicalCategory category = probableMeaning.Category;
            string categoryString = probableMeaning.CategoryString;

            switch (probableMeaning.Category)
            {
                case LexicalCategory.Verb:
                case LexicalCategory.Noun:
                case LexicalCategory.Adjective:
                    InferCategoryFromWord(probableMeaning.Meaning, out category, out categoryString);
                    break;
                case LexicalCategory.Unknown:
                    InferCategoryFromWord(probableMeaning.Meaning, out category, out categoryString);
                    break;
            }

            probableMeaning.Category = category;
            probableMeaning.CategoryString = categoryString;

            return true;
        }

        public override bool GetClassesFromCategoryString(
            string categoryString,
            out string className,
            out string subClassName)
        {
            subClassName = null;

            foreach (string aClassName in SpanishClassEndings)
            {
                if (TextUtilities.ContainsWholeWord(categoryString, aClassName))
                {
                    className = aClassName;
                    return true;
                }
            }

            className = null;

            return false;
        }

        public override bool LookupRootlessEntryExact(
            string pattern,
            out DictionaryEntry specialEntry)
        {
            specialEntry = null;

            if (String.IsNullOrEmpty(pattern))
                return false;

            LexTable endingGraph = EndingsTable;

            if (endingGraph == null)
                return false;

            LexItem lexItem = endingGraph.ParseExact(pattern);

            if (lexItem != null)
            {
                DictionaryEntry rootEntry = GetDictionaryEntry("");

                if (rootEntry == null)
                    return false;

                Sense sense = rootEntry.FindSenseWithSynonym("to go", LanguageLookup.English);
                string inflectionText;

                if (ProcessCategoryInflections(
                    LanguageLookup.Spanish,
                    "",
                    sense,
                    sense.CategoryString,
                    lexItem,
                    null,
                    ref specialEntry,
                    out inflectionText))
                {
                    return true;
                }

            }

            return false;
        }

        public override bool GetSupplementedInflection(
            string inflectedForm,
            LanguageID languageID,
            string[] parts,
            out string preWords,
            out string prefix,
            out string baseInflectedForm,
            out string suffix,
            out string postWords,
            out Designator overrideDesignator,
            out List<Designator> extendedDesignators,
            out Deinflection deinflection)
        {
            InflectorTable inflectorTableVerb = InflectorTable("Verb");
            string word = parts[0];
            bool returnValue = false;

            preWords = null;
            prefix = null;
            baseInflectedForm = null;
            suffix = null;
            postWords = null;
            overrideDesignator = null;
            extendedDesignators = null;
            deinflection = null;

            foreach (TokenDescriptor token in inflectorTableVerb.SubjectPronouns)
            {
                if (word == token.TextFirst)
                {
                    if (parts.Length > 1)
                    {
                        word = parts[1];
                        break;
                    }
                }
            }

            foreach (TokenDescriptor token in inflectorTableVerb.ReflexivePronouns)
            {
                if (word == token.TextFirst)
                {
                    preWords = word;
                    baseInflectedForm = inflectedForm.Replace(word + " ", "");
                    deinflection = GetDeinflectionCached(baseInflectedForm, languageID);

                    if ((deinflection == null) || !deinflection.HasCategory(LexicalCategory.Verb))
                        deinflection = null;
                    else
                    {
                        extendedDesignators = new List<Designator>();
                        foreach (Designator designator in token.Designators)
                        {
                            extendedDesignators.Add(
                                new Designator(
                                    new Designator("Supplemental", "Reflexive"),
                                    designator,
                                    Designator.CombineCode.Union)
                            );
                        };

                        returnValue = true;
                    }

                    break;
                }
            }

            return returnValue;
        }

        public override bool GetExtendedInflectionBase(
            string word,
            LanguageID languageID,
            out string prefix,
            out string baseInflectedForm,
            out string suffix,
            out List<Designator> extendedDesignators,
            out Deinflection deinflection)
        {
            prefix = null;
            baseInflectedForm = null;
            suffix = null;
            extendedDesignators = null;
            deinflection = null;

            InflectorTable inflectorTableVerb = InflectorTable("Verb");
            List<List<Classifier>> classifiers = null;
            List<List<TokenDescriptor>> suffixes = null;
            string originalWord = word;
            int languageIndex = LanguageIDs.IndexOf(languageID);
            bool found = false;

            if (inflectorTableVerb != null)
            {
                bool done = false;
                int pass = 0;

                while (!done)
                {
                    foreach (TokenDescriptor token in inflectorTableVerb.ReflexivePronouns)
                    {
                        if ((word != token.TextFirst) && word.EndsWith(token.TextFirst))
                        {
                            if (suffixes == null)
                                suffixes = new List<List<TokenDescriptor>>() { new List<TokenDescriptor>() { token } };
                            else if (pass < suffixes.Count())
                                suffixes[0].Add(token);
                            else
                                suffixes.Insert(0, new List<TokenDescriptor>() { token });

                            if (classifiers == null)
                                classifiers = new List<List<Classifier>>() { new List<Classifier>() { new Classifier("Suffixed", "Reflexive") } };
                            else if (pass < classifiers.Count())
                                classifiers[0].Add(new Classifier("Suffixed", "Reflexive"));
                            else
                                classifiers.Insert(0, new List<Classifier>() { new Classifier("Suffixed", "Reflexive") });

                            found = true;
                        }
                    }

                    foreach (TokenDescriptor token in inflectorTableVerb.DirectPronouns)
                    {
                        if ((word != token.TextFirst) && word.EndsWith(token.TextFirst))
                        {
                            if (suffixes == null)
                                suffixes = new List<List<TokenDescriptor>>() { new List<TokenDescriptor>() { token } };
                            else if (pass < suffixes.Count())
                                suffixes[0].Add(token);
                            else
                                suffixes.Insert(0, new List<TokenDescriptor>() { token });

                            if (classifiers == null)
                                classifiers = new List<List<Classifier>>() { new List<Classifier>() { new Classifier("Suffixed", "Direct") } };
                            else if (pass < classifiers.Count())
                                classifiers[0].Add(new Classifier("Suffixed", "Direct"));
                            else
                                classifiers.Insert(0, new List<Classifier>() { new Classifier("Suffixed", "Direct") });

                            found = true;
                        }
                    }

                    foreach (TokenDescriptor token in inflectorTableVerb.IndirectPronouns)
                    {
                        if ((word != token.TextFirst) && word.EndsWith(token.TextFirst))
                        {
                            if (suffixes == null)
                                suffixes = new List<List<TokenDescriptor>>() { new List<TokenDescriptor>() { token } };
                            else if (pass < suffixes.Count())
                                suffixes[0].Add(token);
                            else
                                suffixes.Insert(0, new List<TokenDescriptor>() { token });

                            if (classifiers == null)
                                classifiers = new List<List<Classifier>>() { new List<Classifier>() { new Classifier("Suffixed", "Indirect") } };
                            else if (pass < classifiers.Count())
                                classifiers[0].Add(new Classifier("Suffixed", "Indirect"));
                            else
                                classifiers.Insert(0, new List<Classifier>() { new Classifier("Suffixed", "Indirect") });

                            found = true;
                        }
                    }

                    if (found)
                    {
                        List<TokenDescriptor> tokens = suffixes[0];
                        List<Classifier> classifierList = classifiers[0];
                        int i;
                        int c = tokens.Count();
                        int maxLength = 0;

                        for (i = 0; i < c; i++)
                        {
                            if (tokens[i].Text.GetIndexedString(languageIndex).Length > maxLength)
                                maxLength = tokens[i].Text.GetIndexedString(languageIndex).Length;
                        }

                        if (maxLength == word.Length)
                            return false;

                        for (i = c - 1; i >= 0; i--)
                        {
                            if (tokens[i].Text.GetIndexedString(languageIndex).Length < maxLength)
                            {
                                tokens.RemoveAt(i);
                                classifierList.RemoveAt(i);
                            }
                        }

                        word = word.Substring(0, word.Length - maxLength);

                        string testWord = word;

                        char lastChr = testWord[testWord.Length - 1];
                        int limit;

                        if (GetSyllableCount(testWord) > 1)
                            limit = 3;
                        else
                            limit = 2;

                        for (int mode = 0; mode < limit; mode++)
                        {
                            switch (mode)
                            {
                                case 0:
                                    break;
                                case 1:
                                    DeaccentLastVowel(ref testWord);
                                    break;
                                case 2:
                                    DeaccentNextToLastVowel(ref testWord);
                                    break;
                            }

                            deinflection = GetDeinflectionCached(testWord, languageID);

                            if ((deinflection == null) || !deinflection.HasCategory(LexicalCategory.Verb))
                            {
                                found = false;
                                deinflection = null;
                            }
                            else
                            {
                                word = testWord;
                                found = true;
                                done = true;
                                break;
                            }
                        }
                    }
                    else
                        done = true;

                    pass++;
                }

                if ((suffixes != null) && (suffixes.Count() != 0))
                {
                    List<Designator> designators = new List<Designator>();
                    int index = 0;

                    foreach (List<TokenDescriptor> tokens in suffixes)
                    {
                        List<Classifier> classifierList = classifiers[index];

                        TokenDescriptor token;
                        Classifier classifier;

                        if (tokens.Count() == 1)
                        {
                            token = tokens[0];
                            classifier = classifierList[0];
                        }
                        else
                        {
                            if (index == 0)
                            {
                                token = tokens[0];
                                classifier = classifierList[0];
                            }
                            else
                            {
                                token = tokens[1];
                                classifier = classifierList[1];
                            }
                        }

                        if (designators.Count() == 0)
                        {
                            foreach (Designator tokenDesignator in token.Designators)
                            {
                                Designator familyDesignator = new Designator(null, classifier);
                                familyDesignator.AppendClassifications(tokenDesignator);
                                familyDesignator.DefaultLabel();
                                designators.Add(familyDesignator);
                            }
                        }
                        else
                        {
                            List<Designator> newDesignators = new List<Designator>();

                            foreach (Designator designator in designators)
                            {
                                foreach (Designator tokenDesignator in token.Designators)
                                {
                                    Designator familyDesignator = new Designator(designator);
                                    familyDesignator.CopyAndAppendClassification(classifier);
                                    familyDesignator.AppendClassifications(tokenDesignator);
                                    familyDesignator.DefaultLabel();
                                    newDesignators.Add(familyDesignator);
                                }
                            }

                            designators = newDesignators;
                        }

                        index++;
                    }

                    extendedDesignators = designators;

                    suffix = originalWord.Substring(word.Length);
                }
            }

            baseInflectedForm = word;

            return found;
        }

        public override bool IsExtendedInflectionValid(
            List<Designator> extendedDesignators,
            LexicalCategory category,
            string categoryString)
        {
            foreach (Designator designator in extendedDesignators)
            {
                if (designator.HasClassification("Suffixed"))
                {
                    if (category == LexicalCategory.Verb)
                        return true;
                }
            }

            return false;
        }

        public override void FixupExtendedInflection(
            Inflection inflection,
            LanguageID languageID,
            string preWords,
            string prefix,
            string baseInflectedForm,
            string suffix,
            string postWords,
            List<Designator> extendedDesignators,
            Deinflection deinflection,
            string inflectedForm)
        {
            string mainWord = prefix + inflection.GetMainWord(languageID) + suffix;

            if ((extendedDesignators != null) &&
                (extendedDesignators.Count() != 0) &&
                !TextUtilities.ContainsWholeWord(inflectedForm, mainWord))
            {
                string oldSuffix = inflection.GetSuffix(languageID);

                if (String.IsNullOrEmpty(oldSuffix))
                    goto Fixup;

                int suffixCount = GetSyllableCount(suffix);
                int suffixSyllableCount = GetSyllableCount(oldSuffix);
                string oldRoot = inflection.GetRoot(languageID);
                int rootSyllableCount = GetSyllableCount(oldRoot);
                int syllableCount = suffixSyllableCount + rootSyllableCount;

                if (syllableCount <= 1)
                    goto Fixup;

                char lastChr = oldSuffix[oldSuffix.Length - 1];

                if (suffixCount == 1)
                {
                    if (IsVowelChar(lastChr) || (lastChr == 'n') || (lastChr == 's'))
                    {
                        if (suffixSyllableCount > 1)
                        {
                            if (IsStrongVowel(GetLastNthVowel(oldSuffix, 2)))
                            {
                                AccentNextToLastVowel(ref oldSuffix);
                                inflection.SetSuffix(languageID, oldSuffix);
                            }
                        }
                        else /*if (rootSyllableCount <= 1)*/
                        {
                            if (IsStrongVowel(GetLastNthVowel(oldRoot, 1)))
                            {
                                AccentLastVowel(ref oldRoot);
                                inflection.SetRoot(languageID, oldRoot);
                            }
                        }
                    }
                }
                else
                {
                    if (IsVowelChar(lastChr) || (lastChr == 'n') || (lastChr == 's'))
                    {
                        if (suffixSyllableCount > 1)
                        {
                            AccentNextToLastVowel(ref oldSuffix);
                            inflection.SetSuffix(languageID, oldSuffix);
                        }
                        else
                        {
                            AccentLastVowel(ref oldSuffix);
                            inflection.SetSuffix(languageID, oldSuffix);
                        }
                    }
                    else
                    {
                        AccentLastVowel(ref oldSuffix);
                        inflection.SetSuffix(languageID, oldSuffix);
                    }
                }
            }

            Fixup:
            inflection.ExtendLanguage(
                languageID,
                preWords,
                prefix,
                suffix,
                postWords);
        }

        // Handle adding things like reflexive, direct pronouns, indirect pronouns, and other suffixes and prefixes
        // not normally handle in inflecting.
        public override bool ExtendInflection(Inflection inflection)
        {
            Designator designator = inflection.Designation;
            int count = designator.ClassificationCount();
            int index;
            InflectorTable inflectorTable = null;
            TokenDescriptor token = null;
            Designator iteratorDesignator = null;
            bool changed = false;
            bool returnValue = true;

            for (index = 0; index < count; index++)
            {
                Classifier classifier = designator.GetClassificationIndexed(index);

                switch (classifier.KeyString)
                {
                    case "Suffixed":
                        inflectorTable = InflectorTable("Verb");
                        if (inflectorTable == null)
                            break;
                        iteratorDesignator = designator.GetPrefixedDesignator(classifier.Text);
                        token = inflectorTable.FindIteratorTokenFuzzy(classifier.Text + "Pronouns", iteratorDesignator);
                        if (token == null)
                            break;
                        inflection.AppendToSuffix(LanguageID, token.TextFirst);
                        changed = true;
                        break;
                    default:
                        break;
                }
            }

            if (changed)
                inflection.RegenerateLanguage(LanguageID);

            return returnValue;
        }

        public override void ProcessOtherWordsInInflection(
            string word,
            LanguageID languageID,
            bool isPost,
            ref string preWords,
            ref string postWords,
            out Designator overideDesignator)
        {
            if (((word == "no") || (word == "No")) && !isPost)
                overideDesignator = new Designator("Polarity", "Negative");
            else
                overideDesignator = null;
        }

        public override bool PreMatchConditionPost(
            string dictionaryForm,
            SemiRegular semiRegular)
        {
            /*
            if (semiRegular.KeyString == "ie")
            {
                List<string> syllableVowels = GetSyllableVowels(dictionaryForm);
                switch (syllableVowels.Count())
                {
                    case 2:
                        if (syllableVowels[0].Contains("e"))
                            return true;
                        break;
                    case 3:
                        if (syllableVowels[1].Contains("e"))
                            return true;
                        break;
                    case 4:
                        if (syllableVowels[2].Contains("e"))
                            return true;
                        break;
                    default:
                        break;
                }
            }
            else if (semiRegular.KeyString == "ue")
            {
                List<string> syllableVowels = GetSyllableVowels(dictionaryForm);
                switch (syllableVowels.Count())
                {
                    case 2:
                        if (syllableVowels[0].Contains("o"))
                            return true;
                        break;
                    case 3:
                        if (syllableVowels[1].Contains("o"))
                            return true;
                        break;
                    case 4:
                        if (syllableVowels[2].Contains("o"))
                            return true;
                        break;
                    default:
                        break;
                }
            }
            */

            return false;
        }

        public static string[] SuffixedClassifications =
        {
            "Reflexive",
            "Direct",
            "Indirect"
        };

        public static string[] BaseClassifications =
        {
            "Number", "Singular",
            "Number", "Plural",
            "Person", "First",
            "Person", "Second",
            "Person", "Third",
            "Gender", "Masculine",
            "Gender", "Feminine",
            "Politeness", "Informal",
            "Politeness", "Formal"
        };

        public override Designator GetExtendedDesignator(
            string type,
            string label,
            string scope,
            InflectorTable inflectorTable)
        {
            Designator designation = null;
            string key = null;
            int keyOfs = -1;
            string nonExtendedLabel = null;

            if (type != "Verb")
                return null;

            foreach (string item in SuffixedClassifications)
            {
                keyOfs = label.IndexOf(item);

                if (keyOfs != -1)
                {
                    key = item;
                    break;
                }
            }

            if (key == null)
                return null;

            nonExtendedLabel = label.Substring(0, keyOfs - 1);

            Designator nonExtendedDesignator = inflectorTable.GetDesignator(scope, nonExtendedLabel);

            if (nonExtendedDesignator == null)
                return null;

            List<Classifier> classifications = nonExtendedDesignator.CloneClassifications();

            classifications.Add(new Language.Classifier("Suffixed", key));

            int nextOfs = keyOfs + key.Length + 1;

            if (nextOfs <= label.Length)
            {
                string remainder = label.Substring(nextOfs);
                string[] parts = remainder.Split(LanguageLookup.Space, StringSplitOptions.None);

                foreach (string part in parts)
                {
                    string baseKey = null;

                    for (int i = 0; i < BaseClassifications.Length; i += 2)
                    {
                        if (BaseClassifications[i + 1] == part)
                        {
                            baseKey = BaseClassifications[i];
                            break;
                        }
                    }

                    if (baseKey != null)
                    {
                        string subKey = key + baseKey;
                        classifications.Add(new Classifier(subKey, part));
                    }
                    else
                    {
                        if (SuffixedClassifications.Contains(part))
                        {
                            key = part;
                            classifications.Add(new Language.Classifier("Suffixed", key));
                        }
                        else
                            return null;
                    }
                }

                designation = new Designator(label, classifications);
            }

            return designation;
        }

        public List<string> GetSyllableVowels(string str)
        {
            List<string> vowels = new List<string>();
            int syllableIndex = 0;

            foreach (char c in str)
            {
                if (IsTrueVowelChar(c))
                {
                    if (syllableIndex == vowels.Count())
                        vowels.Add(c.ToString());
                    else
                        vowels[syllableIndex] += c;
                }
                else if (syllableIndex < vowels.Count())
                    syllableIndex++;
            }

            return vowels;
        }

        public static char[] SpanishAccentedCharacters = { 'á', 'é', 'í', 'ó', 'ú' };

        public override char[] AccentedCharacters
        {
            get
            {
                return SpanishAccentedCharacters;
            }
        }

        public override bool GetAccentedVowel(string inflected, out char accentedVowel, out int index)
        {
            bool returnValue = false;

            accentedVowel = '\0';
            index = -1;

            if (String.IsNullOrEmpty(inflected))
                return false;

            int lastIndex = inflected.Length - 1;
            char lastChr = char.ToLower(inflected[lastIndex]);

            if ((index = TextUtilities.FindIndexOfFirstCharInArray(inflected, SpanishAccentedCharacters)) != -1)
            {
                accentedVowel = inflected[index];
                return true;
            }

            switch (lastChr)
            {
                case 'a':
                case 'á':
                case 'e':
                case 'é':
                case 'i':
                case 'í':
                case 'o':
                case 'ó':
                case 'u':
                case 'ú':
                case 'n':
                case 's':
                    {
                        int vc = 0;
                        for (int i = lastIndex; i >= 0; i--)
                        {
                            char chr = inflected[i];
                            if (IsVowelChar(chr))
                            {
                                char prevChr = (i > 0 ? inflected[i - 1] : '\0');
                                if (IsVowelChar(prevChr))
                                    continue;
                                if (vc == 1)
                                {
                                    index = i;
                                    accentedVowel = chr;
                                    returnValue = true;
                                    break;
                                }
                                vc++;
                            }
                        }
                    }
                    break;
                default:
                    {
                        for (int i = lastIndex; i >= 0; i--)
                        {
                            char chr = inflected[i];
                            if (IsVowelChar(chr))
                            {
                                char prevChr = (i > 0 ? inflected[i - 1] : '\0');
                                if (IsVowelChar(prevChr))
                                    continue;
                                index = i;
                                accentedVowel = chr;
                                returnValue = true;
                                break;
                            }
                        }
                    }
                    break;
            }

            return returnValue;
        }

        public static char[] SpanishStrongVowels =
        {
            'a',
            'e',
            'o',
            'á',
            'é',
            'í',
            'ó',
            'ú'
        };

        public static char[] SpanishWeakVowels =
        {
            'i',
            'u'
        };

        public bool IsStrongVowel(char c)
        {
            return SpanishStrongVowels.Contains(c);
        }

        public bool IsWeakVowel(char c)
        {
            return SpanishWeakVowels.Contains(c);
        }

        public override int GetSyllableCount(string word)
        {
            int count = 0;
            int index;
            int length = word.Length;

            for (index = 0; index < length;  index++)
            {
                char chr = word[index];

                if (IsTrueVowelChar(chr))
                {
                    if (index + 1 < length)
                    {
                        char nxt = word[index + 1];

                        if (IsTrueVowelChar(nxt))
                        {
                            if (IsWeakVowel(chr))
                            {
                                index++;
                                count++;
                            }
                            else
                            {
                                if (IsWeakVowel(nxt))
                                    index++;

                                count++;
                            }
                        }
                        else
                            count++;
                    }
                    else
                        count++;
                }
            }

            return count;
        }

        public override int GetLastNthVowelIndex(string word, int n)
        {
            int length = word.Length;
            int index;

            for (index = length - 1; index >= 0; index--)
            {
                char chr = word[index];

                if (IsTrueVowelChar(chr))
                {
                    if (n == 1)
                        return index;

                    n--;
                }
            }

            return -1;
        }

        public override char GetLastNthVowel(string word, int n)
        {
            int length = word.Length;
            int index;

            for (index = length - 1; index >= 0; index--)
            {
                char chr = word[index];

                if (IsTrueVowelChar(chr))
                {
                    if (n == 1)
                        return chr;

                    n--;
                }
            }

            return '\0';
        }

        public bool IsVowelChar(char chr)
        {
            bool returnValue;

            switch (char.ToLower(chr))
            {
                case 'a':
                case 'á':
                case 'e':
                case 'é':
                case 'i':
                case 'í':
                case 'o':
                case 'ó':
                case 'u':
                case 'ú':
                case 'n':
                case 's':
                    returnValue = true;
                    break;
                default:
                    returnValue = false;
                    break;
            }

            return returnValue;
        }

        public bool IsTrueVowelChar(char chr)
        {
            bool returnValue;

            switch (char.ToLower(chr))
            {
                case 'a':
                case 'á':
                case 'e':
                case 'é':
                case 'i':
                case 'í':
                case 'o':
                case 'ó':
                case 'u':
                case 'ú':
                    returnValue = true;
                    break;
                default:
                    returnValue = false;
                    break;
            }

            return returnValue;
        }

        public override bool AccentLastVowel(ref string word)
        {
            int length = word.Length;
            int index;
            bool returnValue = false;

            for (index = length - 1; index >= 0; index--)
            {
                if (IsTrueVowelChar(word[index]))
                {
                    word = 
                        word.Substring(0, index) +
                        ConvertCharToAccented(word[index]) +
                        word.Substring(index + 1);
                    returnValue = true;
                    break;
                }
            }

            return returnValue;
        }

        public override bool DeaccentLastVowel(ref string word)
        {
            int length = word.Length;
            int index;
            bool returnValue = false;

            for (index = length - 1; index >= 0; index--)
            {
                if (IsTrueVowelChar(word[index]))
                {
                    word =
                        word.Substring(0, index) +
                        ConvertCharToNonAccented(word[index]) +
                        word.Substring(index + 1);
                    returnValue = true;
                    break;
                }
            }

            return returnValue;
        }

        public override bool AccentNextToLastVowel(ref string word)
        {
            int length = word.Length;
            int index = GetLastNthVowelIndex(word, 2);
            bool returnValue = false;

            if (index != -1)
            {
                word =
                    word.Substring(0, index) +
                    ConvertCharToAccented(word[index]) +
                    word.Substring(index + 1);
                returnValue = true;
            }

            return returnValue;
        }

        public override bool DeaccentNextToLastVowel(ref string word)
        {
            int length = word.Length;
            int index = GetLastNthVowelIndex(word, 2);
            bool returnValue = false;

            if (index != -1)
            {
                word =
                    word.Substring(0, index) +
                    ConvertCharToNonAccented(word[index]) +
                    word.Substring(index + 1);
                returnValue = true;
            }

            return returnValue;
        }

        public string ConvertCharToAccented(char chr)
        {
            string returnValue;

            switch (char.ToLower(chr))
            {
                case 'a':
                    returnValue = "á";
                    break;
                case 'e':
                    returnValue = "é";
                    break;
                case 'i':
                    returnValue = "í";
                    break;
                case 'o':
                    returnValue = "ó";
                    break;
                case 'u':
                    returnValue = "ú";
                    break;
                default:
                    returnValue = chr.ToString();
                    break;
            }

            return returnValue;
        }

        public string ConvertCharToNonAccented(char chr)
        {
            string returnValue;

            switch (char.ToLower(chr))
            {
                case 'á':
                    returnValue = "a";
                    break;
                case 'é':
                    returnValue = "e";
                    break;
                case 'í':
                    returnValue = "i";
                    break;
                case 'ó':
                    returnValue = "o";
                    break;
                case 'ú':
                    returnValue = "u";
                    break;
                default:
                    returnValue = chr.ToString();
                    break;
            }

            return returnValue;
        }

        public static string[] SpanishNumberDigitTable =
        {
            "0", "cero",
            "1", "uno",
            "2", "dos",
            "3", "tres",
            "4", "cuatro",
            "5", "cinco",
            "6", "seis",
            "7", "siete",
            "8", "ocho",
            "9", "nueve",
            "10", "diez",
            "11", "once",
            "12", "doce",
            "13", "trece",
            "14", "catorce",
            "15", "quince",
            "16", "dieciséis",
            "17", "diecisiete",
            "18", "dieciocho",
            "19", "diecinueve",
            "20", "veinte",
            "21", "veintiuno",
            "22", "veintidós",
            "23", "veintitrés",
            "24", "veinticuatro",
            "25", "veinticinco",
            "26", "veintiséis",
            "27", "veintisiete",
            "28", "veintiocho",
            "29", "veintinueve",
            "30", "treinta",
            "31", "treinta y uno",
            "32", "treinta y dos",
            "33", "treinta y tres",
            "34", "treinta y cuatro",
            "35", "treinta y cinco",
            "36", "treinta y seis",
            "37", "treinta y siete",
            "38", "treinta y ocho",
            "39", "treinta y nueve",
            "40", "cuarenta",
            "41", "cuarenta y uno",
            "42", "cuarenta y dos",
            "43", "cuarenta y tres",
            "44", "cuarenta y cuatro",
            "45", "cuarenta y cinco",
            "46", "cuarenta y seis",
            "47", "cuarenta y siete",
            "48", "cuarenta y ocho",
            "49", "cuarenta y nueve",
            "50", "cincuenta",
            "51", "cincuenta y uno",
            "52", "cincuenta y dos",
            "53", "cincuenta y tres",
            "54", "cincuenta y cuatro",
            "55", "cincuenta y cinco",
            "56", "cincuenta y seis",
            "57", "cincuenta y siete",
            "58", "cincuenta y ocho",
            "59", "cincuenta y nueve",
            "60", "sesenta",
            "61", "sesenta y uno",
            "62", "sesenta y dos",
            "63", "sesenta y tres",
            "64", "sesenta y cuatro",
            "65", "sesenta y cinco",
            "66", "sesenta y seis",
            "67", "sesenta y siete",
            "68", "sesenta y ocho",
            "69", "sesenta y nueve",
            "70", "setenta",
            "71", "setenta y uno",
            "72", "setenta y dos",
            "73", "setenta y tres",
            "74", "setenta y cuatro",
            "75", "setenta y cinco",
            "76", "setenta y seis",
            "77", "setenta y siete",
            "78", "setenta y ocho",
            "79", "setenta y nueve",
            "80", "ochenta",
            "81", "ochenta y uno",
            "82", "ochenta y dos",
            "83", "ochenta y tres",
            "84", "ochenta y cuatro",
            "85", "ochenta y cinco",
            "86", "ochenta y seis",
            "87", "ochenta y siete",
            "88", "ochenta y ocho",
            "89", "ochenta y nueve",
            "90", "noventa",
            "91", "noventa y uno",
            "92", "noventa y dos",
            "93", "noventa y tres",
            "94", "noventa y cuatro",
            "95", "noventa y cinco",
            "96", "noventa y seis",
            "97", "noventa y siete",
            "98", "noventa y ocho",
            "99", "noventa y nueve",
            "100", "cien",
            "100", "ciento",
            "100", "cientos",
            "200", "doscientos",
            "300", "trescientos",
            "400", "cuatrocientos",
            "500", "quinientos",
            "600", "seiscientos",
            "700", "setecientos",
            "800", "ochocientos",
            "900", "novecientos",
            "1000", "mil",
            "1000000", "millón",
            "1000000", "millones",
            "1000000000", "mil millones"
        };

        public override string[] NumberDigitTable
        {
            get
            {
                return SpanishNumberDigitTable;
            }
        }

        public static string[] SpanishNumberNameTable =
        {
            "cero", "0",
            "uno", "1",
            "dos", "2",
            "tres", "3",
            "cuatro", "4",
            "cinco", "5",
            "seis", "6",
            "siete", "7",
            "ocho", "8",
            "nueve", "9",
            "diez", "10",
            "once", "11",
            "doce", "12",
            "trece", "13",
            "catorce", "14",
            "quince", "15",
            "dieciséis", "16",
            "diecisiete", "17",
            "dieciocho", "18",
            "diecinueve", "19",
            "veinte", "20",
            "veintiuno", "21",
            "veintidós", "22",
            "veintitrés", "23",
            "veinticuatro", "24",
            "veinticinco", "25",
            "veintiséis", "26",
            "veintisiete", "27",
            "veintiocho", "28",
            "veintinueve", "29",
            "treinta", "30",
            "treinta y uno", "31",
            "treinta y dos", "32",
            "treinta y tres", "33",
            "treinta y cuatro", "34",
            "treinta y cinco", "35",
            "treinta y seis", "36",
            "treinta y siete", "37",
            "treinta y ocho", "38",
            "treinta y nueve", "39",
            "cuarenta", "40",
            "cuarenta y uno", "41",
            "cuarenta y dos", "42",
            "cuarenta y tres", "43",
            "cuarenta y cuatro", "44",
            "cuarenta y cinco", "45",
            "cuarenta y seis", "46",
            "cuarenta y siete", "47",
            "cuarenta y ocho", "48",
            "cuarenta y nueve", "49",
            "cincuenta", "50",
            "cincuenta y uno", "51",
            "cincuenta y dos", "52",
            "cincuenta y tres", "53",
            "cincuenta y cuatro", "54",
            "cincuenta y cinco", "55",
            "cincuenta y seis", "56",
            "cincuenta y siete", "57",
            "cincuenta y ocho", "58",
            "cincuenta y nueve", "59",
            "sesenta", "60",
            "sesenta y uno", "61",
            "sesenta y dos", "62",
            "sesenta y tres", "63",
            "sesenta y cuatro", "64",
            "sesenta y cinco", "65",
            "sesenta y seis", "66",
            "sesenta y siete", "67",
            "sesenta y ocho", "68",
            "sesenta y nueve", "69",
            "setenta", "70",
            "setenta y uno", "71",
            "setenta y dos", "72",
            "setenta y tres", "73",
            "setenta y cuatro", "74",
            "setenta y cinco", "75",
            "setenta y seis", "76",
            "setenta y siete", "77",
            "setenta y ocho", "78",
            "setenta y nueve", "79",
            "ochenta", "80",
            "ochenta y uno", "81",
            "ochenta y dos", "82",
            "ochenta y tres", "83",
            "ochenta y cuatro", "84",
            "ochenta y cinco", "85",
            "ochenta y seis", "86",
            "ochenta y siete", "87",
            "ochenta y ocho", "88",
            "ochenta y nueve", "89",
            "noventa", "90",
            "noventa y uno", "91",
            "noventa y dos", "92",
            "noventa y tres", "93",
            "noventa y cuatro", "94",
            "noventa y cinco", "95",
            "noventa y seis", "96",
            "noventa y siete", "97",
            "noventa y ocho", "98",
            "noventa y nueve", "99",
            "cien", "100",
            "ciento", "100",
            "cienta", "100",
            "cientos", "100",
            "cientas", "100",
            "doscientos", "200",
            "doscientas", "200",
            "trescientos", "300",
            "trescientas", "300",
            "cuatrocientos", "400",
            "cuatrocientas", "400",
            "quinientos", "500",
            "quinientas", "500",
            "seiscientos", "600",
            "seiscientas", "600",
            "setecientos", "700",
            "setecientas", "700",
            "ochocientos", "800",
            "ochocientas", "800",
            "novecientos", "900",
            "novecientas", "900",
            "mil", "1000",
            "millón", "1000000",
            "millones", "1000000",
            "mil millón", "1000000000",
            "mil millones", "1000000000"
        };

        public override string[] NumberNameTable
        {
            get
            {
                return SpanishNumberNameTable;
            }
        }

        public static string[] SpanishNumberCombinationTable =
        {
            "veinte uno", "veintiuno",
            "veinte dos", "veintidós",
            "veinte tres", "veintitrés",
            "veinte cuatro", "veinticuatro",
            "veinte cinco", "veinticinco",
            "veinte seis", "veintiséis",
            "veinte siete", "veintisiete",
            "veinte ocho", "veintiocho",
            "veinte nueve", "veintinueve",
            "treinta uno", "treinta y uno",
            "treinta dos", "treinta y dos",
            "treinta tres", "treinta y tres",
            "treinta cuatro", "treinta y cuatro",
            "treinta cinco", "treinta y cinco",
            "treinta seis", "treinta y seis",
            "treinta siete", "treinta y siete",
            "treinta ocho", "treinta y ocho",
            "treinta nueve", "treinta y nueve",
            "cuarenta uno", "cuarenta y uno",
            "cuarenta dos", "cuarenta y dos",
            "cuarenta tres", "cuarenta y tres",
            "cuarenta cuatro", "cuarenta y cuatro",
            "cuarenta cinco", "cuarenta y cinco",
            "cuarenta seis", "cuarenta y seis",
            "cuarenta siete", "cuarenta y siete",
            "cuarenta ocho", "cuarenta y ocho",
            "cuarenta nueve", "cuarenta y nueve",
            "cincuenta uno", "cincuenta y uno",
            "cincuenta dos", "cincuenta y dos",
            "cincuenta tres", "cincuenta y tres",
            "cincuenta cuatro", "cincuenta y cuatro",
            "cincuenta cinco", "cincuenta y cinco",
            "cincuenta seis", "cincuenta y seis",
            "cincuenta siete", "cincuenta y siete",
            "cincuenta ocho", "cincuenta y ocho",
            "cincuenta nueve", "cincuenta y nueve",
            "sesenta uno", "sesenta y uno",
            "sesenta dos", "sesenta y dos",
            "sesenta tres", "sesenta y tres",
            "sesenta cuatro", "sesenta y cuatro",
            "sesenta cinco", "sesenta y cinco",
            "sesenta seis", "sesenta y seis",
            "sesenta siete", "sesenta y siete",
            "sesenta ocho", "sesenta y ocho",
            "sesenta nueve", "sesenta y nueve",
            "setenta uno", "setenta y uno",
            "setenta dos", "setenta y dos",
            "setenta tres", "setenta y tres",
            "setenta cuatro", "setenta y cuatro",
            "setenta cinco", "setenta y cinco",
            "setenta seis", "setenta y seis",
            "setenta siete", "setenta y siete",
            "setenta ocho", "setenta y ocho",
            "setenta nueve", "setenta y nueve",
            "ochenta uno", "ochenta y uno",
            "ochenta dos", "ochenta y dos",
            "ochenta tres", "ochenta y tres",
            "ochenta cuatro", "ochenta y cuatro",
            "ochenta cinco", "ochenta y cinco",
            "ochenta seis", "ochenta y seis",
            "ochenta siete", "ochenta y siete",
            "ochenta ocho", "ochenta y ocho",
            "ochenta nueve", "ochenta y nueve",
            "noventa uno", "noventa y uno",
            "noventa dos", "noventa y dos",
            "noventa tres", "noventa y tres",
            "noventa cuatro", "noventa y cuatro",
            "noventa cinco", "noventa y cinco",
            "noventa seis", "noventa y seis",
            "noventa siete", "noventa y siete",
            "noventa ocho", "noventa y ocho",
            "noventa nueve", "noventa y nueve",
            "uno cien", "cien",
            "uno ciento", "ciento",
            "uno cienta", "cienta",
            "uno cientos", "cientos",
            "uno cientas", "cientas",
            "dos cientos", "doscientos",
            "dos cientas", "doscientas",
            "tres cientos", "trescientos",
            "tres cientas", "trescientas",
            "cuatro cientos", "cuatrocientos",
            "cuatro cientas", "cuatrocientas",
            "cinco cientos", "quinientos",
            "cinco cientas", "quinientas",
            "seis cientos", "seiscientos",
            "seis cientas", "seiscientas",
            "siete cientos", "setecientos",
            "siete cientas", "setecientas",
            "ocho cientos", "ochocientos",
            "ocho cientas", "ochocientas",
            "nueve cientos", "novecientos",
            "nueve cientas", "novecientas",
            "uno mil", "mil",
            "uno millón", "millón",
            "uno millones", "millones",
            "uno mil millón", "mil millón",
            "uno mil millones", "mil millones"
        };

        private Dictionary<string, string> _SpanishNumberCombinationDictionary;
        public virtual Dictionary<string, string> SpanishNumberCombinationDictionary
        {
            get
            {
                if (_SpanishNumberCombinationDictionary == null)
                {
                    string[] digitTable = SpanishNumberCombinationTable;

                    if (digitTable == null)
                        return null;

                    _SpanishNumberCombinationDictionary = new Dictionary<string, string>();

                    for (int i = 0; i < SpanishNumberCombinationTable.Length; i += 2)
                    {
                        if (!_SpanishNumberCombinationDictionary.ContainsKey(digitTable[i]))
                            _SpanishNumberCombinationDictionary.Add(digitTable[i], digitTable[i + 1]);
                    }
                }

                return _SpanishNumberCombinationDictionary;
            }
        }

        public static string[] SpanishDigitConnectorTable =
        {
            "y"
        };

        public override string[] DigitConnectorTable
        {
            get
            {
                return SpanishDigitConnectorTable;
            }
        }

        public override bool IsAllowWordBeforeDigitConnector(string lastWord, string connector)
        {
            Dictionary<string, string> numberNameToDigitDictionary = NumberNameToDigitDictionary;

            if (String.IsNullOrEmpty(lastWord))
                return false;

            string numberString;

            if (numberNameToDigitDictionary.TryGetValue(lastWord, out numberString))
            {
                if (numberString.EndsWith("0"))
                {
                    int numberValue = ObjectUtilities.GetIntegerFromString(numberString, 0);

                    if ((numberValue >= 30) && (numberValue <= 90))
                        return true;
                }
            }

            return false;
        }

#if false
        public override string[] GetNumberExpansionStandard(string numberString)
        {
            if (String.IsNullOrEmpty(numberString))
                return null;

            Dictionary<string, string> dictionary = NumberDigitToNameDictionary;

            if (dictionary == null)
                return null;

            List<string> expansionList = new List<string>();
            string nameString;
            int startIndex;

            if (dictionary.TryGetValue(numberString, out nameString))
            {
                expansionList.InsertRange(0, nameString.Split(LanguageLookup.Space).ToList());
                return expansionList.ToArray();
            }

            startIndex = numberString.Length - 1;

            if (startIndex < 0)
                startIndex = 0;

            int baseValue = 1;

            for (; startIndex >= 0; startIndex--)
            {
                string digitString = numberString.Substring(startIndex, 1);

                if (digitString == "0")
                    continue;

                int subStartIndex;
                int subEndIndex = startIndex + 1;
                string subNumberString = String.Empty;

                for (subStartIndex = 0; subStartIndex < subEndIndex; subStartIndex++)
                {
                    string subNumberDigits = numberString.Substring(subStartIndex, subEndIndex - subStartIndex);

                    if ((subNumberDigits == "0") && (subStartIndex != (subEndIndex - 1)))
                        break;

                    if (dictionary.TryGetValue(subNumberDigits, out subNumberString))
                        break;
                }

                startIndex = subStartIndex;

                switch (baseValue)
                {
                    case 1:
                        expansionList.InsertRange(0, subNumberString.Split(LanguageLookup.Space).ToList());
                        break;
                    case 100:
                        if (subNumberString == "uno")
                        {
                            if (expansionList.Count() == 0)
                                expansionList.Insert(0, "cien");
                            else
                                expansionList.Insert(0, "ciento");
                        }
                        else
                        {
                            digitString += "00";

                            if (dictionary.TryGetValue(digitString, out nameString))
                                expansionList.Insert(0, nameString);
                        }
                        break;
                    default:
                        {
                            string baseValueString = baseValue.ToString();

                            if (dictionary.TryGetValue(baseValueString, out nameString))
                                expansionList.Insert(0, nameString);

                            expansionList.Insert(0, subNumberString);
                        }
                        break;
                }
            }

            return expansionList.ToArray();
        }
#endif

        // Derived from: https://www.c-sharpcorner.com/article/convert-numeric-value-into-words-currency-in-c-sharp/
        protected override string ConvertWholeNumber(string Number)
        {
            string word = "";

            try
            {
                bool beginsZero = false;//tests for 0XX
                bool isDone = false;//test if already translated
                double dblAmt = (Convert.ToDouble(Number));
                //if ((dblAmt > 0) && number.StartsWith("0"))
                if (dblAmt > 0)
                {//test for zero or digit zero in a nuemric
                    beginsZero = Number.StartsWith("0");

                    int numDigits = Number.Length;
                    int pos = 0;//store digit grouping
                    String placeSingle = "";
                    String placePlural = "";
                    switch (numDigits)
                    {
                        case 1://ones' range
                            word = ones(Number);
                            isDone = true;
                            break;
                        case 2://tens' range
                            word = tens(Number);
                            isDone = true;
                            break;
                        case 3://hundreds' range
                            pos = (numDigits % 3) + 1;
                            placeSingle = "ciento";
                            placePlural = "cientos";
                            break;
                        case 4://thousands' range
                        case 5:
                        case 6:
                            pos = (numDigits % 4) + 1;
                            placeSingle = placePlural = "mil";
                            // place = "millones";
                            break;
                        case 7://millions' range
                        case 8:
                        case 9:
                            pos = (numDigits % 7) + 1;
                            placeSingle = "millón";
                            placePlural = "millones";
                            break;
                        case 10://Billions's range
                        case 11:
                        case 12:
                            pos = (numDigits % 10) + 1;
                            placeSingle = "mil millón";
                            placePlural = "mil millones";
                            break;
                        //add extra case options for anything above Billion...
                        default:
                            isDone = true;
                            break;
                    }
                    if (!isDone)
                    {//if transalation is not done, continue...(Recursion comes in now!!)
                        if (Number.Substring(0, pos) != "0" && Number.Substring(pos) != "0")
                        {
                            try
                            {
                                string before = ConvertWholeNumber(Number.Substring(0, pos));
                                string after = ConvertWholeNumber(Number.Substring(pos));

                                if (String.IsNullOrEmpty(before) || (before == "uno"))
                                    word = placeSingle;
                                else
                                {
                                    word = before + " " + placePlural;

                                    string combinedWord;

                                    if (SpanishNumberCombinationDictionary.TryGetValue(word, out combinedWord))
                                        word = combinedWord;
                                }

                                word += " " + after;
                            }
                            catch { }
                        }
                        else
                        {
                            word = ConvertWholeNumber(Number.Substring(0, pos)) + ConvertWholeNumber(Number.Substring(pos));
                        }

                        //check for trailing zeros
                        //if (beginsZero) word = " and " + word.Trim();
                    }
                    //ignore digit grouping names
                    if (word.Trim().Equals(placeSingle))
                        word = "";
                }
            }
            catch { }
            return word.Trim();
        }

        private string tens(string Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = null;

            switch (_Number)
            {
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 30:
                case 40:
                case 50:
                case 60:
                case 70:
                case 80:
                case 90:
                    if (NumberDigitToNameDictionary.TryGetValue(Number, out name))
                        return name;
                    break;
                default:
                    break;
            }

            if (_Number > 0)
            {
                name = tens(Number.Substring(0, 1) + "0") + " " + ones(Number.Substring(1));
                string combinedWord;

                if (SpanishNumberCombinationDictionary.TryGetValue(name, out combinedWord))
                    name = combinedWord;
            }

            return name;
        }

        private string ones(string Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = "";
            switch (_Number)
            {

                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    if (NumberDigitToNameDictionary.TryGetValue(Number, out name))
                        return name;
                    break;
                default:
                    break;
            }
            return name;
        }
        // End derived code.

        public override string[] GetNumberExpansionDigits(string numberString)
        {
            if (String.IsNullOrEmpty(numberString))
                return null;

            List<string> expansionList = new List<string>();
            Dictionary<string, string> dictionary = NumberDigitToNameDictionary;

            foreach (char c in numberString)
            {
                string digitString = c.ToString();
                string nameString;

                if (dictionary.TryGetValue(digitString, out nameString))
                    expansionList.Add(nameString);
                else
                    return null;
            }

            return expansionList.ToArray();
        }

        public override string NormalizeNumberWord(string numberString)
        {
            return NormalizeNumberWordStatic(numberString);
        }

        public static string NormalizeNumberWordStatic(string numberString)
        {
            if (String.IsNullOrEmpty(numberString))
                return numberString;

            string numberStringLower = numberString.ToLower();

            numberStringLower = numberStringLower.Replace("seis", "séis");

            string normalizedNumberString;

            switch (numberStringLower)
            {
                case "cero":
                    normalizedNumberString = "0";
                    break;
                case "uno":
                    normalizedNumberString = "1";
                    break;
                case "dos":
                    normalizedNumberString = "2";
                    break;
                case "tres":
                    normalizedNumberString = "3";
                    break;
                case "cuatro":
                    normalizedNumberString = "4";
                    break;
                case "cinco":
                    normalizedNumberString = "5";
                    break;
                case "seis":
                case "séis":  //To account for above replacement.
                    normalizedNumberString = "6";
                    break;
                case "siete":
                    normalizedNumberString = "7";
                    break;
                case "ocho":
                    normalizedNumberString = "8";
                    break;
                case "nueve":
                    normalizedNumberString = "9";
                    break;
                case "diez":
                    normalizedNumberString = "10";
                    break;
                case "once":
                    normalizedNumberString = "11";
                    break;
                case "doce":
                    normalizedNumberString = "12";
                    break;
                case "trece":
                    normalizedNumberString = "13";
                    break;
                case "catorce":
                    normalizedNumberString = "14";
                    break;
                case "quince":
                    normalizedNumberString = "15";
                    break;
                case "dieciséis":
                    normalizedNumberString = "16";
                    break;
                case "diecisiete":
                    normalizedNumberString = "17";
                    break;
                case "dieciocho":
                    normalizedNumberString = "18";
                    break;
                case "diecinueve":
                    normalizedNumberString = "19";
                    break;
                case "veinte":
                    normalizedNumberString = "20";
                    break;
                case "veintiuno":
                    normalizedNumberString = "21";
                    break;
                case "veintidós":
                    normalizedNumberString = "22";
                    break;
                case "veintitrés":
                    normalizedNumberString = "23";
                    break;
                case "veinticuatro":
                    normalizedNumberString = "24";
                    break;
                case "veinticinco":
                    normalizedNumberString = "25";
                    break;
                case "veintiséis":
                    normalizedNumberString = "26";
                    break;
                case "veintisiete":
                    normalizedNumberString = "27";
                    break;
                case "veintiocho":
                    normalizedNumberString = "28";
                    break;
                case "veintinueve":
                    normalizedNumberString = "29";
                    break;
                case "treinta":
                    normalizedNumberString = "30";
                    break;
                case "treinta y uno":
                    normalizedNumberString = "31";
                    break;
                case "treinta y dos":
                    normalizedNumberString = "32";
                    break;
                case "treinta y tres":
                    normalizedNumberString = "33";
                    break;
                case "treinta y cuatro":
                    normalizedNumberString = "34";
                    break;
                case "treinta y cinco":
                    normalizedNumberString = "35";
                    break;
                case "treinta y seis":
                    normalizedNumberString = "36";
                    break;
                case "treinta y siete":
                    normalizedNumberString = "37";
                    break;
                case "treinta y ocho":
                    normalizedNumberString = "38";
                    break;
                case "treinta y nueve":
                    normalizedNumberString = "39";
                    break;
                case "cuarenta":
                    normalizedNumberString = "40";
                    break;
                case "cuarenta y uno":
                    normalizedNumberString = "41";
                    break;
                case "cuarenta y dos":
                    normalizedNumberString = "42";
                    break;
                case "cuarenta y tres":
                    normalizedNumberString = "43";
                    break;
                case "cuarenta y cuatro":
                    normalizedNumberString = "44";
                    break;
                case "cuarenta y cinco":
                    normalizedNumberString = "45";
                    break;
                case "cuarenta y seis":
                    normalizedNumberString = "46";
                    break;
                case "cuarenta y siete":
                    normalizedNumberString = "47";
                    break;
                case "cuarenta y ocho":
                    normalizedNumberString = "48";
                    break;
                case "cuarenta y nueve":
                    normalizedNumberString = "49";
                    break;
                case "cincuenta":
                    normalizedNumberString = "50";
                    break;
                case "cincuenta y uno":
                    normalizedNumberString = "51";
                    break;
                case "cincuenta y dos":
                    normalizedNumberString = "52";
                    break;
                case "cincuenta y tres":
                    normalizedNumberString = "53";
                    break;
                case "cincuenta y cuatro":
                    normalizedNumberString = "54";
                    break;
                case "cincuenta y cinco":
                    normalizedNumberString = "55";
                    break;
                case "cincuenta y seis":
                    normalizedNumberString = "56";
                    break;
                case "cincuenta y siete":
                    normalizedNumberString = "57";
                    break;
                case "cincuenta y ocho":
                    normalizedNumberString = "58";
                    break;
                case "cincuenta y nueve":
                    normalizedNumberString = "59";
                    break;
                case "sesenta":
                    normalizedNumberString = "60";
                    break;
                case "sesenta y uno":
                    normalizedNumberString = "61";
                    break;
                case "sesenta y dos":
                    normalizedNumberString = "62";
                    break;
                case "sesenta y tres":
                    normalizedNumberString = "63";
                    break;
                case "sesenta y cuatro":
                    normalizedNumberString = "64";
                    break;
                case "sesenta y cinco":
                    normalizedNumberString = "65";
                    break;
                case "sesenta y seis":
                    normalizedNumberString = "66";
                    break;
                case "sesenta y siete":
                    normalizedNumberString = "67";
                    break;
                case "sesenta y ocho":
                    normalizedNumberString = "68";
                    break;
                case "sesenta y nueve":
                    normalizedNumberString = "69";
                    break;
                case "setenta":
                    normalizedNumberString = "70";
                    break;
                case "setenta y uno":
                    normalizedNumberString = "71";
                    break;
                case "setenta y dos":
                    normalizedNumberString = "72";
                    break;
                case "setenta y tres":
                    normalizedNumberString = "73";
                    break;
                case "setenta y cuatro":
                    normalizedNumberString = "74";
                    break;
                case "setenta y cinco":
                    normalizedNumberString = "75";
                    break;
                case "setenta y seis":
                    normalizedNumberString = "76";
                    break;
                case "setenta y siete":
                    normalizedNumberString = "77";
                    break;
                case "setenta y ocho":
                    normalizedNumberString = "78";
                    break;
                case "setenta y nueve":
                    normalizedNumberString = "79";
                    break;
                case "ochenta":
                    normalizedNumberString = "80";
                    break;
                case "ochenta y uno":
                    normalizedNumberString = "81";
                    break;
                case "ochenta y dos":
                    normalizedNumberString = "82";
                    break;
                case "ochenta y tres":
                    normalizedNumberString = "83";
                    break;
                case "ochenta y cuatro":
                    normalizedNumberString = "84";
                    break;
                case "ochenta y cinco":
                    normalizedNumberString = "85";
                    break;
                case "ochenta y seis":
                    normalizedNumberString = "86";
                    break;
                case "ochenta y siete":
                    normalizedNumberString = "87";
                    break;
                case "ochenta y ocho":
                    normalizedNumberString = "88";
                    break;
                case "ochenta y nueve":
                    normalizedNumberString = "89";
                    break;
                case "noventa":
                    normalizedNumberString = "90";
                    break;
                case "noventa y uno":
                    normalizedNumberString = "91";
                    break;
                case "noventa y dos":
                    normalizedNumberString = "92";
                    break;
                case "noventa y tres":
                    normalizedNumberString = "93";
                    break;
                case "noventa y cuatro":
                    normalizedNumberString = "94";
                    break;
                case "noventa y cinco":
                    normalizedNumberString = "95";
                    break;
                case "noventa y seis":
                    normalizedNumberString = "96";
                    break;
                case "noventa y siete":
                    normalizedNumberString = "97";
                    break;
                case "noventa y ocho":
                    normalizedNumberString = "98";
                    break;
                case "noventa y nueve":
                    normalizedNumberString = "99";
                    break;
                case "cien":
                    normalizedNumberString = "100";
                    break;
                case "mil":
                    normalizedNumberString = "1000";
                    break;
                case "million":
                    normalizedNumberString = "1000000";
                    break;
                default:
                    normalizedNumberString = numberString;
                    break;
            }

            return normalizedNumberString;
        }

        // From https://spanishdictionary.cc/common-spanish-abbreviations
        public static Dictionary<string, string> SpanishAbbreviationDictionary = new Dictionary<string, string>()
        {
            { "a.C.", "antes de Cristo" },              // B.C.
            { "a. de C.", "antes de Cristo" },          // B.C.
            { "a.J.C.", "antes de Jesucristo" },        // B.C.
            { "a. de J.C.", "antes de Jesucristo" },    // B.C.
            { "a. m.", "antes del mediodía" },          // a.m before noon
            { "apdo.", "apartado" },                    // postal P.O Box
            { "aprox.", "aproximadamente" },            // approximately
            //{ "Av.", "avenida" },                       // Ave.avenue, in addresses
            { "Avda.", "avenida" },                     // Ave.avenue, in addresses
            { "Bs. As.", "Buenos Aires" },              // Buenos Aires
            //{ "c.c.", "centímetros cúbicos c.c." },     // cubic centimeters
            //{ "Cía", "compañía" },                      // Co. company
            //{ "cm", "centímetros" },                    // cm  centimeters
            { "c/u", "cada uno" },                      // apiece
            //{ "D.", "don" },                            // Sir
            // Mistaken for "da" from "dar".
            //{ "Da", "doña" },                           // Madam
            { "d.C.", "después de Cristo" },    		// A.D.
            { "d. de C.", "después de Cristo" },        // A.D.
            { "d.J.C.", "después de Jesucristo" },      // A.D.
            { "d. de J.C.", "después de Jesucristo" },  // A.D.
            { "dna.", "docena" },                       // dozen
            { "EE. UU.", "Estados Unidos de América" }, // U.S
            { "esq.", "esquina" },                      // street corner
            { "etc.", "etcétera" },                     // etc.
            { "F.C.", "ferrocarril R.R." },             // railroad
            { "FF. AA.", "fuerzas armadas" },           // armed forces
            { "Dr.", "doctor" },                        // Dr.
            { "Dra.", "doctora" },                      // Dr.
            // Mistaken for "e" as in "and".
            //{ "E", "este (punto cardinal)" },           // E   east
            { "Gob.", "gobierno" },                     // Gov.
            { "km/h", "kilómetros por hora" },          // kilometers per hour
            { "l", "litros" },                          // liters
            //{ "Lic.", "licenciado" },                   // attorney
            //{ "m", "metros" },                          // meters
            { "mm", "milímetros" },                     // millimeters
            // Someone's initials (Samuel H. Smith)
            //{ "h", "hora" },                            // hour
            { "Ing.", "ingeniero" },                    // engineer
            { "kg", "kilogramos" },                     // kg  kilograms
            { "pág.", "página" },                       // page
            { "N", "norte" },                           // N   north
            // Too easily mistaken for negative
            // { "no., núm.", "número" },                  // No. number
            // Mistaken for "O" as in "Oh".
            //{ "O", "oeste" },                           // W   west
            { "P.D.", "postdata" },                     // P.S.
            { "OEA", "Organización de Estados Americanos" },            // OAS Organization of American States
            //{ "ONU", "Organización de Naciones Unidas" },               // UN  United Nations
            { "OTAN", "La Organización del Tratado Atlántico Norte" },  // NATO    North Atlantic Treaty Organization
            { "p.ej.", "por ejemplo" },                 // e.g	for example
            { "p. m.", "post meridien" },               // p.m after noon
            { "Prof.", "profesor" },                    // Professor
            { "Profa.", "profesora" },                  // Professor
            { "q.e.p.d.", "que en paz descanse" },      // R.I.P   rest in peace
            // Too often mistaken.
            //{ "S", "sur S" },                           // south
            { "S.A.", "Sociedad Anónima" },             // Inc.
            { "S.L.", "Sociedad Limitada" },            // Ltd.
            { "Sr.", "señor" },                         // Mr.
            { "Sra.", "señora" },                       // Mrs., Ms.
            { "Srta.", "señorita" },                    // Miss, Ms.
            { "Ud.", "usted" },                         // You
            { "Vd.", "usted" },                         // You
            { "Uds.", "ustedes" },                      // You all
            { "Vds.", "ustedes" },                      // You all
            { "vol.", "volumen" },                      // vol.    volume
            { "W.C", "water closet" },                  // bathroom, toilet
        };

        public override Dictionary<string, string> AbbreviationDictionary
        {
            get
            {
                return SpanishAbbreviationDictionary;
            }
        }

        public override List<string> AutomaticRowKeys(string type)
        {
            InflectorTable inflectorTable = InflectorTable(type);
            List<string> rowKeys = null;

            if (inflectorTable != null)
                rowKeys = inflectorTable.AutomaticRowKeys;

            if ((rowKeys == null) || (rowKeys.Count() == 0))
            {
                rowKeys = new List<string>();

                switch (type)
                {
                    case "Verb":
                        rowKeys.Add("Number");
                        break;
                    case "Noun":
                        rowKeys.Add("Number");
                        break;
                    case "Adjective":
                        rowKeys.Add("Number");
                        break;
                    default:
                        break;
                }
            }

            return rowKeys;
        }

        public override List<string> AutomaticColumnKeys(string type)
        {
            InflectorTable inflectorTable = InflectorTable(type);
            List<string> columnKeys = null;

            if (inflectorTable != null)
                columnKeys = inflectorTable.AutomaticColumnKeys;

            if ((columnKeys == null) || (columnKeys.Count() == 0))
            {
                columnKeys = new List<string>();

                switch (type)
                {
                    case "Verb":
                        columnKeys.Add("Person");
                        columnKeys.Add("Politeness");
                        columnKeys.Add("Gender");
                        break;
                    case "Noun":
                        columnKeys.Add("Gender");
                        break;
                    case "Adjective":
                        columnKeys.Add("Gender");
                        break;
                    default:
                        break;
                }
            }

            return columnKeys;
        }

        // This is temporary, until I get a mechanism for reading a server directory.
        public override List<KeyValuePair<string, string>> GetInflectionsLayoutTypes(
            string category,
            LanguageUtilities languageUtilities)
        {
            List<KeyValuePair<string, string>> layoutTypes = new List<KeyValuePair<string, string>>();

            if (category == "Verb")
            {
                layoutTypes.Add(
                    new KeyValuePair<string, string>(
                        "Compact",
                        languageUtilities.TranslateUIString("Compact")));
                layoutTypes.Add(
                    new KeyValuePair<string, string>(
                        "Full",
                        languageUtilities.TranslateUIString("Full")));
                layoutTypes.Add(
                    new KeyValuePair<string, string>(
                        "Automated",
                        languageUtilities.TranslateUIString("Automated")));
            }

            return layoutTypes;
        }
    }
}
