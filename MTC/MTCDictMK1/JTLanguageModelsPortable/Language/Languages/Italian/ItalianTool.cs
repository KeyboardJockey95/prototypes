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
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public class ItalianTool : LanguageTool
    {
        // Verb classes.
        public const string VerbEndingAre = "are";
        public const string VerbEndingEre = "ere";
        public const string VerbEndingIre = "ire";
        public const string NounOrAdjectiveEndingO = "o";
        public const string NounOrAdjectiveEndingA = "a";
        public const string NounOrAdjectiveEndingE = "e";
        public const string NounOrAdjectiveEndingI = "i";

        public string[] ItalianClassEndings = new string[]
        {
            VerbEndingAre,
            VerbEndingEre,
            VerbEndingIre,
            NounOrAdjectiveEndingO,
            NounOrAdjectiveEndingA,
            NounOrAdjectiveEndingE,
            NounOrAdjectiveEndingI
        };

        public string[] ItalianVerbClassEndings = new string[]
        {
            VerbEndingAre,
            VerbEndingEre,
            VerbEndingIre
        };

        public override string[] VerbClassEndings
        {
            get
            {
                return ItalianVerbClassEndings;
            }
        }

        public string[] ItalianNonVerbClassEndings = new string[]
        {
            NounOrAdjectiveEndingO,
            NounOrAdjectiveEndingA,
            NounOrAdjectiveEndingE,
            NounOrAdjectiveEndingI
        };

        public override string[] NonVerbClassEndings
        {
            get
            {
                return ItalianNonVerbClassEndings;
            }
        }

        public static char[] ItalianVowelCharacters =
        {
            'a',
            'e',
            'i',
            'o',
            'u',
            'é',
            'ó',
            'à',
            'è',
            'ì',
            'ò',
            'ù',
            'â',
            'ê',
            'î',
            'ô',
            'û',
            'y'
        };

        public override char[] VowelCharacters
        {
            get
            {
                return ItalianVowelCharacters;
            }
        }

        public static char[] ItalianAccentedCharacters =
        {
            'é',
            'ó',
            'à',
            'è',
            'ì',
            'ò',
            'ù',
            'â',
            'ê',
            'î',
            'ô',
            'û'
        };

        public override char[] AccentedCharacters
        {
            get
            {
                return ItalianAccentedCharacters;
            }
        }

        public ItalianTool() : base(LanguageLookup.Italian)
        {
            _UsesImplicitPronouns = true;
            SetCanInflect("Verb", true);
            SetCanInflect("Adjective", true);
            SetCanInflect("Noun", true);
            SetCanInflect("Unknown", true);
            CanDeinflect = true;
            UseFileBasedEndingsTable = false;
        }

        public override IBaseObject Clone()
        {
            return new ItalianTool();
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
            "Gender", "Feminine"
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

            List<Classifier> classifications = new List<Classifier>()
            {
                new Language.Classifier("Suffixed", key)
            };

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
                        return null;
                }

                designation = new Designator(label, classifications);
            }

            return designation;
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
                    if (ItalianNonVerbClassEndings.Contains(ending))
                    {
                        classCode = ending;
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
                    ending = word.Substring(word.Length - 1);
                    if (ItalianNonVerbClassEndings.Contains(ending))
                    {
                        classCode = ending;
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
                    ending = word.Substring(word.Length - 3);
                    if (ItalianVerbClassEndings.Contains(ending))
                    {
                        classCode = ending;
                        categoryString = "v," + classCode;
                        returnValue = true;
                    }
                    break;
                default:
                    break;
            }

            return returnValue;
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
                case "i":
                    subCode = "pl";
                    break;
                case "e":
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

            string stem = null;

            foreach (string ending in ItalianClassEndings)
            {
                if (word.EndsWith(ending))
                {
                    switch (ending)
                    {
                        case VerbEndingAre:
                            categoryString = "v," + VerbEndingAre;
                            classCode = VerbEndingAre;
                            stem = word.Substring(0, word.Length - 3);
                            break;
                        case VerbEndingEre:
                            categoryString = "v," + VerbEndingEre;
                            classCode = VerbEndingEre;
                            stem = word.Substring(0, word.Length - 3);
                            break;
                        case VerbEndingIre:
                            categoryString = "v," + VerbEndingIre;
                            classCode = VerbEndingIre;
                            stem = word.Substring(0, word.Length - 3);
                            break;
                        case NounOrAdjectiveEndingO:
                            categoryString = NounOrAdjectiveEndingO;
                            classCode = NounOrAdjectiveEndingO;
                            stem = word.Substring(0, word.Length - 1);
                            break;
                        case NounOrAdjectiveEndingA:
                            categoryString = NounOrAdjectiveEndingA;
                            classCode = NounOrAdjectiveEndingA;
                            stem = word.Substring(0, word.Length - 1);
                            break;
                        case NounOrAdjectiveEndingE:
                            categoryString = NounOrAdjectiveEndingE;
                            classCode = NounOrAdjectiveEndingE;
                            stem = word.Substring(0, word.Length - 1);
                            break;
                        case NounOrAdjectiveEndingI:
                            categoryString = NounOrAdjectiveEndingI;
                            classCode = NounOrAdjectiveEndingI;
                            stem = word.Substring(0, word.Length - 1);
                            break;
                        default:
                            stem = null;
                            break;
                    }

                    break;
                }
            }

            return stem;
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
            if (String.IsNullOrEmpty(categoryString))
                return InferCategoryFromWord(
                    dictionaryForm.Text(LanguageLookup.Italian),
                    out category,
                    out categoryString);

            return true;
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

        public override bool InferCategoryFromWord(
            string word,
            out LexicalCategory category,
            out string categoryString)
        {
            category = LexicalCategory.Unknown;
            categoryString = null;

            if (String.IsNullOrEmpty(word))
                return false;

            foreach (string ending in ItalianClassEndings)
            {
                if (word.EndsWith(ending))
                {
                    if (ending.Length == 3)
                    {
                        category = LexicalCategory.Verb;
                        categoryString = "v," + ending;
                        return true;
                    }
                    else if (ending.Length == 1)
                    {
                        category = LexicalCategory.Unknown;
                        categoryString = ending;
                        return true;
                    }
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
            LexicalCategory category = GetCategoryFromRawCategoryIt(rawCategory, String.Empty);
            return category;
        }

        public static LexicalCategory GetCategoryFromRawCategoryIt(string rawCategory, string cValue)
        {
            LexicalCategory category;

            switch (rawCategory)
            {
                case "n":
                    category = LexicalCategory.Noun;
                    break;
                case "pron":
                    category = LexicalCategory.Pronoun;
                    break;
                case "adj":
                    category = LexicalCategory.Adjective;
                    break;
                case "v":
                    category = LexicalCategory.Verb;
                    break;
                case "adv":
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
                case "pref":
                    category = LexicalCategory.Prefix;
                    break;
                default:
                    category = LexicalCategory.Unknown;
                    break;
            }

            return category;
        }

        public override bool GetClassesFromCategoryString(
            string categoryString,
            out string className,
            out string subClassName)
        {
            subClassName = String.Empty;

            foreach (string aClassName in ItalianClassEndings)
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

#if false
        public override bool LookupLanguageSpecificDictionaryEntries(
            string pattern,
            List<LanguageID> languageIDs,
            List<DictionaryEntry> bestDictionaryEntries)
        {
            string theEnding = null;
            bool found = false;

            foreach (string ending in ItalianNonVerbClassEndings)
            {
                if (pattern.EndsWith(ending))
                {
                    theEnding = ending;
                    found = true;
                    break;
                }
            }

            if (found)
            {
                string stem = pattern.Substring(0, pattern.Length - theEnding.Length);

                foreach (string ending in ItalianNonVerbClassEndings)
                {
                    if (ending == theEnding)
                        continue;

                    string testPattern = stem + ending;
                    DictionaryEntry dictionaryEntry = GetDictionaryEntry(testPattern);

                    if (dictionaryEntry != null)
                        bestDictionaryEntries.Add(dictionaryEntry);
                }
            }

            return bestDictionaryEntries.Count() != 0;
        }
#endif

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
                    LanguageLookup.Italian,
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
            int maxLength = 0;
            int diff = 0;
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
                        maxLength = 0;

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

                        if (lastChr == 'r')
                        {
                            testWord += 'e';
                            diff = -1;
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

                    suffix = originalWord.Substring(word.Length + diff);
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
            if (baseInflectedForm.EndsWith("re"))
                inflection.TruncateSuffixEnd(languageID, 1);

            inflection.ExtendLanguage(
                languageID,
                preWords,
                prefix,
                suffix,
                postWords);
        }

        public override void ProcessOtherWordsInInflection(
            string word,
            LanguageID languageID,
            bool isPost,
            ref string preWords,
            ref string postWords,
            out Designator overideDesignator)
        {
            if (((word == "non") || (word == "Non")) && !isPost)
                overideDesignator = new Designator("Polarity", "Negative");
            else
                overideDesignator = null;
        }

        // Handle trigger function call.
        public override bool ModifierTriggerFunctionCall(
            string triggerFunctionCall,
            string triggerFunctionName,
            List<string> triggerArguments,
            MultiLanguageString dictionaryForm)
        {
            switch (triggerFunctionName)
            {
                case "StartsWithZOrSPlusConsonant":
                    foreach (LanguageString ls in dictionaryForm.LanguageStrings)
                    {
                        string word = ls.Text;

                        if (String.IsNullOrEmpty(word))
                            continue;

                        if (char.ToLower(word[0]) == 'z')
                            return true;
                        else if (char.ToLower(word[0]) == 's')
                        {
                            if (word.Length >= 2)
                            {
                                if (!IsVowel(word[1]))
                                    return true;
                            }
                        }
                    }
                    break;
                case "StartsWithZOrSPlusConsonantOrXOrPnOrPsOrGn":
                    foreach (LanguageString ls in dictionaryForm.LanguageStrings)
                    {
                        string word = ls.Text;

                        if (String.IsNullOrEmpty(word))
                            continue;

                        if (char.ToLower(word[0]) == 'z')
                            return true;
                        else if (char.ToLower(word[0]) == 's')
                        {
                            if (word.Length >= 2)
                            {
                                if (!IsVowel(word[1]))
                                    return true;
                            }
                        }
                        else if (char.ToLower(word[0]) == 'x')
                            return true;
                        else if (char.ToLower(word[0]) == 'p')
                        {
                            if (word.Length >= 2)
                            {
                                if (char.ToLower(word[1]) == 'n')
                                    return true;
                                else if (char.ToLower(word[1]) == 's')
                                    return true;
                            }
                        }
                        else if (char.ToLower(word[0]) == 'g')
                        {
                            if (word.Length >= 2)
                            {
                                if (char.ToLower(word[1]) == 'n')
                                    return true;
                            }
                        }
                    }
                    break;
                default:
                    return base.ModifierTriggerFunctionCall(
                        triggerFunctionCall,
                        triggerFunctionName,
                        triggerArguments,
                        dictionaryForm);
            }

            return false;
        }

        public override bool PreMatchConditionPost(
            string dictionaryForm,
            SemiRegular semiRegular)
        {
            return false;
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

        public override bool GetAccentedVowel(string inflected, out char accentedVowel, out int index)
        {
            bool returnValue = false;

            accentedVowel = '\0';
            index = -1;

            if (String.IsNullOrEmpty(inflected))
                return false;

            int lastIndex = inflected.Length - 1;
            char lastChr = char.ToLower(inflected[lastIndex]);

            if ((index = TextUtilities.FindIndexOfFirstCharInArray(inflected, AccentedCharacters)) != -1)
            {
                accentedVowel = inflected[index];
                return true;
            }

            if (IsVowelCharOrS(lastChr))
            {
                int vc = 0;
                for (int i = lastIndex; i >= 0; i--)
                {
                    char chr = inflected[i];
                    if (IsVowelCharOrS(chr))
                    {
                        char prevChr = (i > 0 ? inflected[i - 1] : '\0');
                        if (IsVowelCharOrS(prevChr))
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
            else
            {
                for (int i = lastIndex; i >= 0; i--)
                {
                    char chr = inflected[i];
                    if (IsVowelCharOrS(chr))
                    {
                        char prevChr = (i > 0 ? inflected[i - 1] : '\0');
                        if (IsVowelCharOrS(prevChr))
                            continue;
                        index = i;
                        accentedVowel = chr;
                        returnValue = true;
                        break;
                    }
                }
            }

            return returnValue;
        }

        public override int GetSyllableCount(string word)
        {
            int count = 0;

            foreach (char chr in word)
            {
                if (IsTrueVowelChar(chr))
                    count++;
            }

            return count;
        }

        public bool IsVowelChar(char chr)
        {
            return ItalianVowelCharacters.Contains(chr);
        }

        public bool IsVowelCharOrS(char chr)
        {
            return (chr == 's') || ItalianVowelCharacters.Contains(chr);
        }

        public bool IsTrueVowelChar(char chr)
        {
            return ItalianVowelCharacters.Contains(chr);
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

        public string ConvertCharToAccented(char chr)
        {
            string returnValue;

            switch (char.ToLower(chr))
            {
                case 'a':
                    returnValue = "à";
                    break;
                case 'e':
                    //returnValue = "é";
                    returnValue = "è";
                    break;
                case 'i':
                    returnValue = "ì";
                    break;
                case 'o':
                    returnValue = "ò";
                    break;
                case 'u':
                    returnValue = "ù";
                    break;
                default:
                    returnValue = chr.ToString();
                    break;
            }

            return returnValue;
        }

        public static string[] ItalianNumberDigitTable =
        {
            "0", "zero",
            "1", "uno",
            "2", "due",
            "3", "tre",
            "4", "quattro",
            "5", "cinque",
            "6", "sei",
            "7", "sette",
            "8", "otto",
            "9", "nove",
            "10", "dieci",
            "11", "undici",
            "12", "dodici",
            "13", "tredici",
            "14", "quattordici",
            "15", "quindici",
            "16", "sedici",
            "17", "diciassette",
            "18", "diciotto",
            "19", "diciannove",
            "20", "venti",
            "21", "ventiuno",
            "22", "ventidue",
            "23", "ventitre",
            "24", "ventiquattro",
            "25", "venticinque",
            "26", "ventisei",
            "27", "ventisette",
            "28", "ventiotto",
            "29", "ventinove",
            "30", "trenta",
            "31", "trentauno",
            "32", "trentadue",
            "33", "trentatre",
            "34", "trentaquattro",
            "35", "trentacinque",
            "36", "trentasei",
            "37", "trentasette",
            "38", "trentaotto",
            "39", "trentanove",
            "40", "quaranta",
            "41", "quarantauno",
            "42", "quarantadue",
            "43", "quarantatre",
            "44", "quarantaquattro",
            "45", "quarantacinque",
            "46", "quarantasei",
            "47", "quarantasette",
            "48", "quarantaotto",
            "49", "quarantanove",
            "50", "cinquanta",
            "51", "cinquantauno",
            "52", "cinquantadue",
            "53", "cinquantatre",
            "54", "cinquantaquattro",
            "55", "cinquantacinque",
            "56", "cinquantasei",
            "57", "cinquantasette",
            "58", "cinquantaotto",
            "59", "cinquantanove",
            "60", "sessanta",
            "61", "sessantauno",
            "62", "sessantadue",
            "63", "sessantatre",
            "64", "sessantaquattro",
            "65", "sessantacinque",
            "66", "sessantasei",
            "67", "sessantasette",
            "68", "sessantaotto",
            "69", "sessantanove",
            "70", "settanta",
            "71", "settantauno",
            "72", "settantadue",
            "73", "settantatre",
            "74", "settantaquattro",
            "75", "settantacinque",
            "76", "settantasei",
            "77", "settantasette",
            "78", "settantaotto",
            "79", "settantanove",
            "80", "ottanta",
            "81", "ottantauno",
            "82", "ottantadue",
            "83", "ottantatre",
            "84", "ottantaquattro",
            "85", "ottantacinque",
            "86", "ottantasei",
            "87", "ottantasette",
            "88", "ottantaotto",
            "89", "ottantanove",
            "90", "novanta",
            "91", "novantauno",
            "92", "novantadue",
            "93", "novantatre",
            "94", "novantaquattro",
            "95", "novantacinque",
            "96", "novantasei",
            "97", "novantasette",
            "98", "novantaotto",
            "99", "novantanove",
            "100", "cento",
            "200", "duecento",
            "300", "trecento",
            "400", "quattrocento",
            "500", "cinquecento",
            "600", "seicento",
            "700", "setecento",
            "800", "ottocento",
            "900", "novecento",
            "1000", "mille",
            "1000", "mila",
            "1000000", "milione",
            "1000000", "milioni",
            "1000000", "un milione",
            "1000000000", "un miliardo",
            "1000000000", "miliardi"
        };

        public override string[] NumberDigitTable
        {
            get
            {
                return ItalianNumberDigitTable;
            }
        }

        public static string[] ItalianNumberNameTable =
        {
            "zero", "0",
            "uno", "1",
            "due", "2",
            "tre", "3",
            "quattro", "4",
            "cinque", "5",
            "sei", "6",
            "sette", "7",
            "otto", "8",
            "nove", "9",
            "dieci", "10",
            "undici", "11",
            "dodici", "12",
            "tredici", "13",
            "quattordici", "14",
            "quindici", "15",
            "sedici", "16",
            "diciassette", "17",
            "diciotto", "18",
            "diciannove", "19",
            "venti", "20",
            "ventiuno", "21",
            "ventidue", "22",
            "ventitre", "23",
            "ventiquattro", "24",
            "venticinque", "25",
            "ventisei", "26",
            "ventisette", "27",
            "ventiotto", "28",
            "ventinove", "29",
            "trenta", "30",
            "trentauno", "31",
            "trentadue", "32",
            "trentatre", "33",
            "trentaquattro", "34",
            "trentacinque", "35",
            "trentasei", "36",
            "trentasette", "37",
            "trentaotto", "38",
            "trentanove", "39",
            "quaranta", "40",
            "quarantauno", "41",
            "quarantadue", "42",
            "quarantatre", "43",
            "quarantaquattro", "44",
            "quarantacinque", "45",
            "quarantasei", "46",
            "quarantasette", "47",
            "quarantaotto", "48",
            "quarantanove", "49",
            "cinquanta", "50",
            "cinquantauno", "51",
            "cinquantadue", "52",
            "cinquantatre", "53",
            "cinquantaquattro", "54",
            "cinquantacinque", "55",
            "cinquantasei", "56",
            "cinquantasette", "57",
            "cinquantaotto", "58",
            "cinquantanove", "59",
            "sessanta", "60",
            "sessantauno", "61",
            "sessantadue", "62",
            "sessantatre", "63",
            "sessantaquattro", "64",
            "sessantacinque", "65",
            "sessantasei", "66",
            "sessantasette", "67",
            "sessantaotto", "68",
            "sessantanove", "69",
            "settanta", "70",
            "settantauno", "71",
            "settantadue", "72",
            "settantatre", "73",
            "settantaquattro", "74",
            "settantacinque", "75",
            "settantasei", "76",
            "settantasette", "77",
            "settantaotto", "78",
            "settantanove", "79",
            "ottanta", "80",
            "ottantauno", "81",
            "ottantadue", "82",
            "ottantatre", "83",
            "ottantaquattro", "84",
            "ottantacinque", "85",
            "ottantasei", "86",
            "ottantasette", "87",
            "ottantaotto", "88",
            "ottantanove", "89",
            "novanta", "90",
            "novantauno", "91",
            "novantadue", "92",
            "novantatre", "93",
            "novantaquattro", "94",
            "novantacinque", "95",
            "novantasei", "96",
            "novantasette", "97",
            "novantaotto", "98",
            "novantanove", "99",
            "cento", "100",
            "duecento", "200",
            "trecento", "300",
            "quattrocento", "400",
            "cinquecento", "500",
            "seicento", "600",
            "setecento", "700",
            "ottocento", "800",
            "novecento", "900",
            "mille", "1000",
            "mila", "1000",
            "un milione", "1000000",
            "milioni", "1000000",
            "un miliardo", "1000000000",
            "miliardi", "1000000000"
        };

        public override string[] NumberNameTable
        {
            get
            {
                return ItalianNumberNameTable;
            }
        }

        public static string[] ItalianNumberCombinationTable =
        {
            "venti uno", "ventiuno",
            "venti due", "ventidue",
            "venti tre", "ventitre",
            "venti quattro", "ventiquattro",
            "venti cinque", "venticinque",
            "venti sei", "ventisei",
            "venti sette", "ventisette",
            "venti otto", "ventiotto",
            "venti nove", "ventinove",
            "trenta uno", "trentauno",
            "trenta due", "trentadue",
            "trenta tre", "trentatre",
            "trenta quattro", "trentaquattro",
            "trenta cinque", "trentacinque",
            "trenta sei", "trentasei",
            "trenta sette", "trentasette",
            "trenta otto", "trentaotto",
            "trenta nove", "trentanove",
            "quaranta uno", "quarantauno",
            "quaranta due", "quarantadue",
            "quaranta tre", "quarantatre",
            "quaranta quattro", "quarantaquattro",
            "quaranta cinque", "quarantacinque",
            "quaranta sei", "quarantasei",
            "quaranta sette", "quarantasette",
            "quaranta otto", "quarantaotto",
            "quaranta nove", "quarantanove",
            "cinquanta uno", "cinquantauno",
            "cinquanta due", "cinquantadue",
            "cinquanta tre", "cinquantatre",
            "cinquanta quattro", "cinquantaquattro",
            "cinquanta cinque", "cinquantacinque",
            "cinquanta sei", "cinquantasei",
            "cinquanta sette", "cinquantasette",
            "cinquanta otto", "cinquantaotto",
            "cinquanta nove", "cinquantanove",
            "sessanta uno", "sessantauno",
            "sessanta due", "sessantadue",
            "sessanta tre", "sessantatre",
            "sessanta quattro", "sessantaquattro",
            "sessanta cinque", "sessantacinque",
            "sessanta sei", "sessantasei",
            "sessanta sette", "sessantasette",
            "sessanta otto", "sessantaotto",
            "sessanta nove", "sessantanove",
            "settanta uno", "settantauno",
            "settanta due", "settantadue",
            "settanta tre", "settantatre",
            "settanta quattro", "settantaquattro",
            "settanta cinque", "settantacinque",
            "settanta sei", "settantasei",
            "settanta sette", "settantasette",
            "settanta otto", "settantaotto",
            "settanta nove", "settantanove",
            "ottanta uno", "ottantauno",
            "ottanta due", "ottantadue",
            "ottanta tre", "ottantatre",
            "ottanta quattro", "ottantaquattro",
            "ottanta cinque", "ottantacinque",
            "ottanta sei", "ottantasei",
            "ottanta sette", "ottantasette",
            "ottanta otto", "ottantaotto",
            "ottanta nove", "ottantanove",
            "novanta uno", "novantauno",
            "novanta due", "novantadue",
            "novanta tre", "novantatre",
            "novanta quattro", "novantaquattro",
            "novanta cinque", "novantacinque",
            "novanta sei", "novantasei",
            "novanta sette", "novantasette",
            "novanta otto", "novantaotto",
            "novanta nove", "novantanove",
            "uno cento", "cento",
            "due cento", "duecento",
            "tre cento", "trecento",
            "quattro cento", "quattrocento",
            "cinque cento", "cinquecento",
            "sei cento", "seicento",
            "sette cento", "setecento",
            "otto cento", "ottocento",
            "nove cento", "novecento",
            "uno mille", "mille"
        };

        private Dictionary<string, string> _ItalianNumberCombinationDictionary;
        public virtual Dictionary<string, string> ItalianNumberCombinationDictionary
        {
            get
            {
                if (_ItalianNumberCombinationDictionary == null)
                {
                    string[] digitTable = ItalianNumberCombinationTable;

                    if (digitTable == null)
                        return null;

                    _ItalianNumberCombinationDictionary = new Dictionary<string, string>();

                    for (int i = 0; i < ItalianNumberCombinationTable.Length; i += 2)
                    {
                        if (!_ItalianNumberCombinationDictionary.ContainsKey(digitTable[i]))
                            _ItalianNumberCombinationDictionary.Add(digitTable[i], digitTable[i + 1]);
                    }
                }

                return _ItalianNumberCombinationDictionary;
            }
        }

        public static string[] ItalianDigitConnectorTable =
        {
            "y"
        };

        public override string[] DigitConnectorTable
        {
            get
            {
                return ItalianDigitConnectorTable;
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
                                expansionList.Insert(0, "cento");
                            else
                                expansionList.Insert(0, "cento");
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
                            placeSingle = "cento";
                            placePlural = "cento";
                            break;
                        case 4://thousands' range
                        case 5:
                        case 6:
                            pos = (numDigits % 4) + 1;
                            placeSingle = placePlural = "mille";
                            placePlural = "mila";
                            break;
                        case 7://millions' range
                        case 8:
                        case 9:
                            pos = (numDigits % 7) + 1;
                            placeSingle = "un milione";
                            placePlural = "milioni";
                            break;
                        case 10://Billions's range
                        case 11:
                        case 12:
                            pos = (numDigits % 10) + 1;
                            placeSingle = "un miliardo";
                            placePlural = "miliardi";
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

                                    if (ItalianNumberCombinationDictionary.TryGetValue(word, out combinedWord))
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

                if (ItalianNumberCombinationDictionary.TryGetValue(name, out combinedWord))
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

            string normalizedNumberString;

            switch (numberStringLower)
            {
                case "zero":
                    normalizedNumberString = "0";
                    break;
                case "uno":
                    normalizedNumberString = "1";
                    break;
                case "due":
                    normalizedNumberString = "2";
                    break;
                case "tre":
                    normalizedNumberString = "3";
                    break;
                case "quattro":
                    normalizedNumberString = "4";
                    break;
                case "cinque":
                    normalizedNumberString = "5";
                    break;
                case "sei":
                    normalizedNumberString = "6";
                    break;
                case "sette":
                    normalizedNumberString = "7";
                    break;
                case "otto":
                    normalizedNumberString = "8";
                    break;
                case "nove":
                    normalizedNumberString = "9";
                    break;
                case "dieci":
                    normalizedNumberString = "10";
                    break;
                case "undici":
                    normalizedNumberString = "11";
                    break;
                case "dodici":
                    normalizedNumberString = "12";
                    break;
                case "tredici":
                    normalizedNumberString = "13";
                    break;
                case "quattordici":
                    normalizedNumberString = "14";
                    break;
                case "quindici":
                    normalizedNumberString = "15";
                    break;
                case "sedici":
                    normalizedNumberString = "16";
                    break;
                case "diciassette":
                    normalizedNumberString = "17";
                    break;
                case "diciotto":
                    normalizedNumberString = "18";
                    break;
                case "diciannove":
                    normalizedNumberString = "19";
                    break;
                case "venti":
                    normalizedNumberString = "20";
                    break;
                case "ventiuno":
                    normalizedNumberString = "21";
                    break;
                case "ventidue":
                    normalizedNumberString = "22";
                    break;
                case "ventitre":
                    normalizedNumberString = "23";
                    break;
                case "ventiquattro":
                    normalizedNumberString = "24";
                    break;
                case "venticinque":
                    normalizedNumberString = "25";
                    break;
                case "ventisei":
                    normalizedNumberString = "26";
                    break;
                case "ventisette":
                    normalizedNumberString = "27";
                    break;
                case "ventiotto":
                    normalizedNumberString = "28";
                    break;
                case "ventinove":
                    normalizedNumberString = "29";
                    break;
                case "trenta":
                    normalizedNumberString = "30";
                    break;
                case "trentauno":
                    normalizedNumberString = "31";
                    break;
                case "trentadue":
                    normalizedNumberString = "32";
                    break;
                case "trentatre":
                    normalizedNumberString = "33";
                    break;
                case "trentaquattro":
                    normalizedNumberString = "34";
                    break;
                case "trentacinque":
                    normalizedNumberString = "35";
                    break;
                case "trentasei":
                    normalizedNumberString = "36";
                    break;
                case "trentasette":
                    normalizedNumberString = "37";
                    break;
                case "trentaotto":
                    normalizedNumberString = "38";
                    break;
                case "trentanove":
                    normalizedNumberString = "39";
                    break;
                case "quaranta":
                    normalizedNumberString = "40";
                    break;
                case "quarantauno":
                    normalizedNumberString = "41";
                    break;
                case "quarantadue":
                    normalizedNumberString = "42";
                    break;
                case "quarantatre":
                    normalizedNumberString = "43";
                    break;
                case "quarantaquattro":
                    normalizedNumberString = "44";
                    break;
                case "quarantacinque":
                    normalizedNumberString = "45";
                    break;
                case "quarantasei":
                    normalizedNumberString = "46";
                    break;
                case "quarantasette":
                    normalizedNumberString = "47";
                    break;
                case "quarantaotto":
                    normalizedNumberString = "48";
                    break;
                case "quarantanove":
                    normalizedNumberString = "49";
                    break;
                case "cinquanta":
                    normalizedNumberString = "50";
                    break;
                case "cinquantauno":
                    normalizedNumberString = "51";
                    break;
                case "cinquantadue":
                    normalizedNumberString = "52";
                    break;
                case "cinquantatre":
                    normalizedNumberString = "53";
                    break;
                case "cinquantaquattro":
                    normalizedNumberString = "54";
                    break;
                case "cinquantacinque":
                    normalizedNumberString = "55";
                    break;
                case "cinquantasei":
                    normalizedNumberString = "56";
                    break;
                case "cinquantasette":
                    normalizedNumberString = "57";
                    break;
                case "cinquantaotto":
                    normalizedNumberString = "58";
                    break;
                case "cinquantanove":
                    normalizedNumberString = "59";
                    break;
                case "sessanta":
                    normalizedNumberString = "60";
                    break;
                case "sessantauno":
                    normalizedNumberString = "61";
                    break;
                case "sessantadue":
                    normalizedNumberString = "62";
                    break;
                case "sessantatre":
                    normalizedNumberString = "63";
                    break;
                case "sessantaquattro":
                    normalizedNumberString = "64";
                    break;
                case "sessantacinque":
                    normalizedNumberString = "65";
                    break;
                case "sessantasei":
                    normalizedNumberString = "66";
                    break;
                case "sessantasette":
                    normalizedNumberString = "67";
                    break;
                case "sessantaotto":
                    normalizedNumberString = "68";
                    break;
                case "sessantanove":
                    normalizedNumberString = "69";
                    break;
                case "settanta":
                    normalizedNumberString = "70";
                    break;
                case "settantauno":
                    normalizedNumberString = "71";
                    break;
                case "settantadue":
                    normalizedNumberString = "72";
                    break;
                case "settantatre":
                    normalizedNumberString = "73";
                    break;
                case "settantaquattro":
                    normalizedNumberString = "74";
                    break;
                case "settantacinque":
                    normalizedNumberString = "75";
                    break;
                case "settantasei":
                    normalizedNumberString = "76";
                    break;
                case "settantasette":
                    normalizedNumberString = "77";
                    break;
                case "settantaotto":
                    normalizedNumberString = "78";
                    break;
                case "settantanove":
                    normalizedNumberString = "79";
                    break;
                case "ottanta":
                    normalizedNumberString = "80";
                    break;
                case "ottantauno":
                    normalizedNumberString = "81";
                    break;
                case "ottantadue":
                    normalizedNumberString = "82";
                    break;
                case "ottantatre":
                    normalizedNumberString = "83";
                    break;
                case "ottantaquattro":
                    normalizedNumberString = "84";
                    break;
                case "ottantacinque":
                    normalizedNumberString = "85";
                    break;
                case "ottantasei":
                    normalizedNumberString = "86";
                    break;
                case "ottantasette":
                    normalizedNumberString = "87";
                    break;
                case "ottantaotto":
                    normalizedNumberString = "88";
                    break;
                case "ottantanove":
                    normalizedNumberString = "89";
                    break;
                case "novanta":
                    normalizedNumberString = "90";
                    break;
                case "novantauno":
                    normalizedNumberString = "91";
                    break;
                case "novantadue":
                    normalizedNumberString = "92";
                    break;
                case "novantatre":
                    normalizedNumberString = "93";
                    break;
                case "novantaquattro":
                    normalizedNumberString = "94";
                    break;
                case "novantacinque":
                    normalizedNumberString = "95";
                    break;
                case "novantasei":
                    normalizedNumberString = "96";
                    break;
                case "novantasette":
                    normalizedNumberString = "97";
                    break;
                case "novantaotto":
                    normalizedNumberString = "98";
                    break;
                case "novantanove":
                    normalizedNumberString = "99";
                    break;
                case "cento":
                    normalizedNumberString = "100";
                    break;
                case "mille":
                case "mila":
                    normalizedNumberString = "1000";
                    break;
                case "un milione":
                    normalizedNumberString = "1000000";
                    break;
                default:
                    normalizedNumberString = numberString;
                    break;
            }

            return normalizedNumberString;
        }

        // From https://spanishdictionary.cc/common-spanish-abbreviations
        public static Dictionary<string, string> ItalianAbbreviationDictionary = new Dictionary<string, string>()
        {
            { "a.C.", "avanti Cristo" },                // B.C.
            { "d.C.", "dopo Cristo" },    		        // A.D.
            { "Dott.", "Dottore" },                     // Dr.
            { "Sig.", "Signor" },                       // Mr.
            { "Sig.ra", "Signora" },                    // Mrs., Ms.
            { "Sig.na", "Signorina" },                  // Miss, Ms.
            { "Sigg.", "Signori" },                     // Misters (plural)
        };

        public override Dictionary<string, string> AbbreviationDictionary
        {
            get
            {
                return ItalianAbbreviationDictionary;
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
