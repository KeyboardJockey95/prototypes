using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Dictionary
{
    public enum LexicalCategory
    {
        Unknown,            // Unknown
        Multiple,           // Multiple categories.
        Noun,               // Abstract or concrete entity.
        ProperNoun,         // Proper noun.
        Pronoun,            // Substitute for a noun or noun phrase.
        Determiner,         // Determiner (this, that, those, each, every, etc.).
        Adjective,          // Qualifier of a noun.
        Verb,               // Action or state of being.
        Adverb,             // Qualifier of an adjective, verb, or other adverb.
        Preposition,        // Establisher of relation and syntactic context.
        Conjunction,        // Syntactic connector.
        Interjection,       // Emotional greeting (or "exclamation").
        Particle,           // Particle.
        Article,            // Direct or indirect article.
        MeasureWord,        // Measure word.
        Number,             // Cardinal number.
        Prefix,             // Prefix.
        Suffix,             // Suffix.
        Abbreviation,       // Abbreviation.
        Acronym,            // Acronym.
        Symbol,             // Symbol (i.e. TM).
        Phrase,             // Phrase.
        PrepositionalPhrase,// Prepositional phrase.
        Proverb,            // Proverb.
        Contraction,        // Contraction
        Idiom,              // Language idiom.
        Stem,               // Word stem.
        IrregularStem,      // Word stem for an irregular inflection.
        Inflection,         // Word inflection
        NotFound            // Place holder for entry not found.
    }

    public enum LexicalAttribute
    {
        None,
        Masculine,
        Feminine,
        Neuter,
        Singular,
        Plural,
        Dual,
        Uncountable
    }

    public class Sense
    {
        protected int _Reading;     // Key of corresponding alternate, 0 if none.
        protected LexicalCategory _Category;
        protected string _CategoryString;
        protected int _PriorityLevel;
        protected List<LanguageSynonyms> _LanguageSynonyms;
        protected List<MultiLanguageString> _Examples;
        protected List<Inflection> _Inflections;       // not saved.
        protected bool _Modified;

        public Sense(
            int reading,
            LexicalCategory category,
            string categoryString,
            int priorityLevel,
            List<LanguageSynonyms> languageSynonyms,
            List<MultiLanguageString> examples)
        {
            _Reading = reading;
            _Category = category;
            _CategoryString = categoryString;
            _PriorityLevel = priorityLevel;
            _LanguageSynonyms = languageSynonyms;
            _Examples = examples;
            _Modified = false;
        }

        public Sense(Sense other)
        {
            Copy(other);
            _Modified = false;
        }

        public Sense(XElement element)
        {
            OnElement(element);
        }

        public Sense()
        {
            Clear();
        }

        public void Clear()
        {
            _Reading = 0;
            _Category = LexicalCategory.Unknown;
            _CategoryString = string.Empty;
            _PriorityLevel = 0;
            _LanguageSynonyms = null;
            _Examples = null;
            _Inflections = null;
            _Modified = false;
        }

        public void Copy(Sense other)
        {
            _Reading = other.Reading;
            _Category = other.Category;
            _CategoryString = other.CategoryString;
            _PriorityLevel = other.PriorityLevel;

            if (other.LanguageSynonyms != null)
            {
                _LanguageSynonyms = new List<LanguageSynonyms>(other.LanguageSynonyms.Count());

                foreach (LanguageSynonyms languageSynonyms in other.LanguageSynonyms)
                    _LanguageSynonyms.Add(new LanguageSynonyms(languageSynonyms));
            }
            else
                _LanguageSynonyms = null;

            if (other.Examples != null)
            {
                _Examples = new List<MultiLanguageString>(other.Examples.Count());

                foreach (MultiLanguageString example in other.Examples)
                    _Examples.Add(new MultiLanguageString(example));
            }
            else
                _Examples = null;

            if (other.Inflections != null)
                _Inflections = new List<Inflection>(other.Inflections);
            else
                _Inflections = null;

            _Modified = true;
        }

        public override string ToString()
        {
            return GetDefinitions(true, true);
        }

        public int Reading
        {
            get
            {
                return _Reading;
            }
            set
            {
                if (value != _Reading)
                {
                    _Reading = value;
                    _Modified = true;
                }
            }
        }

        public LexicalCategory Category
        {
            get
            {
                return _Category;
            }
            set
            {
                if (value != _Category)
                {
                    _Category = value;
                    _Modified = true;
                }
            }
        }

        public string CategoryString
        {
            get
            {
                return _CategoryString;
            }
            set
            {
                if (value != _CategoryString)
                {
                    _CategoryString = value;
                    _Modified = true;
                }
            }
        }

        public int PriorityLevel
        {
            get
            {
                return _PriorityLevel;
            }
            set
            {
                if (value != _PriorityLevel)
                {
                    _PriorityLevel = value;
                    _Modified = true;
                }
            }
        }

        public List<LanguageSynonyms> LanguageSynonyms
        {
            get
            {
                return _LanguageSynonyms;
            }
            set
            {
                if (value != _LanguageSynonyms)
                {
                    _LanguageSynonyms = value;
                    _Modified = true;
                }
            }
        }

        public int LanguageSynonymsCount
        {
            get
            {
                if (_LanguageSynonyms != null)
                    return _LanguageSynonyms.Count();
                return 0;
            }
        }

        public LanguageSynonyms GetLanguageSynonyms(LanguageID languageID)
        {
            if (_LanguageSynonyms == null)
                return null;

            LanguageSynonyms languageSynonyms = _LanguageSynonyms.FirstOrDefault(x => x.LanguageID == languageID);

            return languageSynonyms;
        }

        public LanguageSynonyms GetLanguageSynonymsIndexed(int index)
        {
            if (_LanguageSynonyms == null)
                return null;

            if ((index < 0) || (index >= _LanguageSynonyms.Count()))
                return null;

            LanguageSynonyms languageSynonyms = _LanguageSynonyms[index];

            return languageSynonyms;
        }

        public LanguageSynonyms FindLanguageSynonymsWithSynonym(string synonym)
        {
            if (_LanguageSynonyms == null)
                return null;

            foreach (LanguageSynonyms languageSynonyms in _LanguageSynonyms)
            {
                ProbableMeaning probableMeaning = languageSynonyms.FindProbableSynonym(synonym);

                if (probableMeaning != null)
                    return languageSynonyms;
            }

            return null;
        }

        public void AddLanguageSynonyms(LanguageSynonyms languageSynonyms)
        {
            if (_LanguageSynonyms == null)
                _LanguageSynonyms = new List<LanguageSynonyms>(1);

            _LanguageSynonyms.Add(languageSynonyms);
        }

        public bool RemoveLanguageSynonyms(LanguageSynonyms languageSynonyms)
        {
            if (_LanguageSynonyms != null)
                return _LanguageSynonyms.Remove(languageSynonyms);

            return false;
        }

        public int GetRowCount(List<LanguageDescriptor> languageDescriptors, bool showExamples, bool showExampleTitle)
        {
            int count = GetLanguageSynonymsRowCount(languageDescriptors) +
                GetExampleRowCount(languageDescriptors, showExamples, showExampleTitle);

            return count;
        }

        public int GetLanguageSynonymsRowCount(List<LanguageDescriptor> languageDescriptors)
        {
            int count = 0;

            if (_LanguageSynonyms != null)
            {
                foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
                {
                    if (!languageDescriptor.Show || !languageDescriptor.Used)
                        continue;

                    LanguageSynonyms languageSynonyms = GetLanguageSynonyms(languageDescriptor.LanguageID);

                    if (languageSynonyms == null)
                        continue;

                    if (languageSynonyms.SynonymCount > count)
                        count = languageSynonyms.SynonymCount;
                }
            }

            return count;
        }

        public int GetExampleRowCount(List<LanguageDescriptor> languageDescriptors, bool showExamples, bool showExampleTitle)
        {
            int count = 0;

            if ((_Examples != null) && (_Examples.Count() != 0))
            {
                if (showExampleTitle)
                    count++;

                foreach (MultiLanguageString example in _Examples)
                {
                    if (example.HasText(languageDescriptors))
                        count++;
                }
            }

            return count;
        }

        public bool HasLanguage(LanguageID languageID)
        {
            if (_LanguageSynonyms != null)
            {
                foreach (LanguageSynonyms languageSynonyms in _LanguageSynonyms)
                {
                    if ((languageSynonyms.LanguageID == languageID) && 
                            (languageSynonyms.ProbableSynonymCount != 0))
                        return true;
                }
            }

            return false;
        }

        public bool HasAnyLanguage(List<LanguageID> languageIDs)
        {
            if (_LanguageSynonyms != null)
            {
                foreach (LanguageSynonyms languageSynonyms in _LanguageSynonyms)
                {
                    if (languageIDs.Contains(languageSynonyms.LanguageID) &&
                            (languageSynonyms.ProbableSynonymCount != 0))
                        return true;
                }
            }

            return false;
        }

        public bool HasAnyLanguage(List<LanguageDescriptor> languageDescriptors)
        {
            if (_LanguageSynonyms != null)
            {
                foreach (LanguageSynonyms languageSynonyms in _LanguageSynonyms)
                {
                    if ((languageDescriptors.FirstOrDefault(x => x.Show && x.Used && (x.LanguageID == languageSynonyms.LanguageID)) != null) &&
                            (languageSynonyms.ProbableSynonymCount != 0))
                        return true;
                }
            }

            return false;
        }

        public bool HasMeaning(string meaning, LanguageID languageID)
        {
            LanguageSynonyms languageSynonyms = GetLanguageSynonyms(languageID);

            if (languageSynonyms == null)
                return false;

            if (languageSynonyms.HasMeaning(meaning))
                return true;

            return false;
        }

        public bool HasMeaningStart(string meaning, LanguageID languageID)
        {
            LanguageSynonyms languageSynonyms = GetLanguageSynonyms(languageID);

            if (languageSynonyms == null)
                return false;

            if (languageSynonyms.HasMeaningStart(meaning))
                return true;

            return false;
        }

        public List<ProbableMeaning> GetProbableSynonyms(LanguageID languageID)
        {
            if (_LanguageSynonyms == null)
                return null;

            LanguageSynonyms languageSynonyms = _LanguageSynonyms.FirstOrDefault(x => x.LanguageID == languageID);

            if (languageSynonyms != null)
                return languageSynonyms.ProbableSynonyms;

            return new List<ProbableMeaning>();
        }

        public ProbableMeaning GetProbableSynonymIndexed(LanguageID languageID, int index)
        {
            if (_LanguageSynonyms == null)
                return null;

            LanguageSynonyms languageSynonyms = _LanguageSynonyms.FirstOrDefault(x => x.LanguageID == languageID);

            if (languageSynonyms != null)
                return languageSynonyms.GetProbableSynonymIndexed(index);

            return null;
        }

        public bool GetProbableSynonymContainingText(
            LanguageID languageID,
            string containingText,
            out int synonymIndex,
            out ProbableMeaning definition)
        {
            synonymIndex = -1;
            definition = null;

            if (LanguageSynonymsCount == 0)
                return false;

            LanguageSynonyms languageSynonyms = GetLanguageSynonyms(languageID);

            if (languageSynonyms != null)
                return languageSynonyms.GetProbableSynonymContainingText(
                    containingText,
                    out synonymIndex,
                    out definition);

            return false;
        }

        public bool SortProbableSynonymsBySourceCount(LanguageID languageID)
        {
            if (LanguageSynonymsCount == 0)
                return false;

            bool returnValue = false;

            foreach (LanguageSynonyms languageSynonyms in _LanguageSynonyms)
            {
                if (languageSynonyms.LanguageID != languageID)
                    continue;

                if (languageSynonyms.SortProbableSynonymsBySourceCount())
                    returnValue = true;
            }

            return returnValue;
        }

        public int MaxSourceIDCount(LanguageID languageID)
        {
            if (LanguageSynonymsCount == 0)
                return 0;

            int maxCount = 0;

            foreach (LanguageSynonyms languageSynonyms in _LanguageSynonyms)
            {
                if ((languageID != null) && (languageID != languageSynonyms.LanguageID))
                    continue;

                int thisCount = languageSynonyms.MaxSourceIDCount();

                if (thisCount > maxCount)
                    maxCount = thisCount;
            }

            return maxCount;
        }

        public int MaxFrequency(LanguageID languageID)
        {
            if (LanguageSynonymsCount == 0)
                return 0;

            int maxCount = 0;

            foreach (LanguageSynonyms languageSynonyms in _LanguageSynonyms)
            {
                if ((languageID != null) && (languageID != languageSynonyms.LanguageID))
                    continue;

                int thisCount = languageSynonyms.MaxFrequency();

                if (thisCount > maxCount)
                    maxCount = thisCount;
            }

            return maxCount;
        }

        public List<string> GetSynonyms(LanguageID languageID)
        {
            if (_LanguageSynonyms == null)
                return null;

            LanguageSynonyms languageSynonyms = _LanguageSynonyms.FirstOrDefault(x => x.LanguageID == languageID);

            if (languageSynonyms != null)
                return languageSynonyms.Synonyms;

            return new List<string>();
        }

        public string GetSynonymIndexed(LanguageID languageID, int index)
        {
            if (_LanguageSynonyms == null)
                return null;

            LanguageSynonyms languageSynonyms = _LanguageSynonyms.FirstOrDefault(x => x.LanguageID == languageID);

            if (languageSynonyms != null)
                return languageSynonyms.GetSynonymIndexed(index);

            return String.Empty;
        }

        public bool GetSynonymContainingText(
            LanguageID languageID,
            string containingText,
            out int synonymIndex,
            out string definition)
        {
            synonymIndex = -1;
            definition = String.Empty;

            if (LanguageSynonymsCount == 0)
                return false;

            LanguageSynonyms languageSynonyms = GetLanguageSynonyms(languageID);

            if (languageSynonyms != null)
                return languageSynonyms.GetSynonymContainingText(containingText, out synonymIndex, out definition);

            return false;
        }

        public bool GetSynonymMatchingText(
            LanguageID languageID,
            string matchingText,
            out int synonymIndex)
        {
            synonymIndex = -1;

            if (LanguageSynonymsCount == 0)
                return false;

            LanguageSynonyms languageSynonyms = GetLanguageSynonyms(languageID);

            if (languageSynonyms != null)
                return languageSynonyms.GetSynonymMatchingText(matchingText, out synonymIndex);

            return false;
        }

        public void Retarget(MultiLanguageString input, MultiLanguageString output)
        {
            if (LanguageSynonyms != null)
            {
                foreach (LanguageSynonyms languageSynonyms in LanguageSynonyms)
                    languageSynonyms.Retarget(input, output, this);
            }
        }

        public string GetDefinitions(bool showLexicalTag, bool showSources)
        {
            string definition = "";

            if (showLexicalTag)
                definition = GetLexicalCategoryTag(_LanguageSynonyms.First().LanguageID);

            if (_LanguageSynonyms != null)
            {
                foreach (LanguageSynonyms languageSynonyms in _LanguageSynonyms)
                {
                    if (!String.IsNullOrEmpty(definition))
                        definition += ";";

                    definition += languageSynonyms.LanguageID.LanguageCultureExtensionCode + ":" + languageSynonyms.GetDefinition(showSources);
                }
            }

            return definition;
        }

        public string GetDefinition(LanguageID languageID, bool showLexicalTag, bool showSources)
        {
            string definition = "";
            LanguageSynonyms languageSynonyms = GetLanguageSynonyms(languageID);

            if (languageSynonyms == null)
                return definition;

            if (showLexicalTag)
                definition = GetLexicalCategoryTag(languageID);

            definition += languageSynonyms.GetDefinition(showSources);

            return definition;
        }

        public string GetDefinitionMarkedUp(
            LanguageID languageID,
            List<LanguageDescription> languageDescriptions,
            bool showLexicalTag,
            bool showSources)
        {
            string definition = "";
            LanguageSynonyms languageSynonyms = GetLanguageSynonyms(languageID);

            if (languageSynonyms == null)
                return definition;

            if (showLexicalTag)
                definition = GetLexicalCategoryTag(languageID);

            definition += languageSynonyms.GetDefinitionMarkedUp(languageDescriptions, showSources);

            return definition;
        }

        public char[] SynonymSeparators = { '/' };
        public char[] LexicalCategorySeparators = { '(', ')' };
        public char[] SourceSeparators = { '{', '}' };

        public bool ParseDefinition(
            LanguageID languageID,
            string senseString)
        {
            LanguageSynonyms languageSynonyms;
            int synonymCount;
            int synonymIndex;
            string synonymString;
            bool returnValue = true;

            if (senseString.Contains("("))
            {
                string[] catParts = senseString.Split(LexicalCategorySeparators, StringSplitOptions.None);
                if (catParts.Length == 3)
                {
                    string catString = catParts[1].Trim();
                    senseString = catParts[2].Trim();

                    if (!String.IsNullOrEmpty(catString))
                    {
                        LexicalCategory lexicalCategory;
                        if (Sense.GetLexicalCategoryFromString(languageID, catString, out lexicalCategory))
                            Category = lexicalCategory;
                        else
                            CategoryString = catString;
                    }
                }
            }

            languageSynonyms = GetLanguageSynonyms(languageID);

            if (languageSynonyms == null)
            {
                languageSynonyms = new LanguageSynonyms(languageID, null);
                AddLanguageSynonyms(languageSynonyms);
            }
            else
                languageSynonyms.DeleteAllSynonyms();

            string[] rawSynonyms = senseString.Split(SynonymSeparators, StringSplitOptions.None);
            synonymCount = rawSynonyms.Length;

            for (synonymIndex = 0; synonymIndex < synonymCount; synonymIndex++)
            {
                List<int> sourceIDs = null;

                synonymString = rawSynonyms[synonymIndex].Trim();

                if (synonymString.Contains("{"))
                {
                    string[] sourceParts = senseString.Split(SourceSeparators, StringSplitOptions.None);
                    if (sourceParts.Length >= 2)
                    {
                        string sourceString = sourceParts[2].Trim();

                        senseString = sourceParts[1].Trim();

                        if (!String.IsNullOrEmpty(sourceString))
                        {
                            string[] sources = sourceString.Split(LanguageLookup.Comma);

                            foreach (string source in sources)
                            {
                                if (sourceIDs == null)
                                    sourceIDs = new List<int>();

                                int sourceID = ApplicationData.DictionarySourcesLazy.GetID(source.Trim());

                                if (sourceID != -1)
                                    sourceIDs.Add(sourceID);
                            }

                            if ((sourceIDs != null) && (sourceIDs.Count() == 0))
                                sourceIDs = null;
                        }
                    }
                }

                ProbableMeaning probableSynonym = new ProbableMeaning(
                    synonymString,
                    Category,
                    CategoryString,
                    float.NaN,
                    0,
                    sourceIDs);
                languageSynonyms.AddProbableSynonym(probableSynonym);
            }

            return returnValue;
        }

        public string GetTranslation(LanguageID languageID)
        {
            string definition = "";
            LanguageSynonyms languageSynonyms = GetLanguageSynonyms(languageID);

            if (languageSynonyms == null)
                return definition;

            definition = languageSynonyms.GetTranslation();

            return definition;
        }

        public List<MultiLanguageString> Examples
        {
            get
            {
                return _Examples;
            }
            set
            {
                if (value != _Examples)
                {
                    _Examples = value;
                    _Modified = true;
                }
            }
        }

        public MultiLanguageString GetExampleIndexed(int index)
        {
            MultiLanguageString example = null;

            if ((_Examples != null) && (_Examples.Count() != 0))
                example = _Examples[index];

            return example;
        }

        public int ExampleCount
        {
            get
            {
                if (_Examples != null)
                    return _Examples.Count();
                return 0;
            }
        }

        public int GetExampleCount(List<LanguageDescriptor> languageDescriptors)
        {
            int count = 0;
            if (_Examples != null)
            {
                foreach (MultiLanguageString example in _Examples)
                {
                    if (example.HasText(languageDescriptors))
                        count++;
                }
            }
            return count;
        }

        public List<string> GetExamples(LanguageID languageID)
        {
            List<String> examples = null;

            if ((_Examples != null) && (_Examples.Count() != 0))
            {
                foreach (MultiLanguageString example in _Examples)
                {
                    string exampleText = example.Text(languageID);

                    if (!String.IsNullOrEmpty(exampleText))
                    {
                        if (examples == null)
                            examples = new List<string>();

                        examples.Add(exampleText);
                    }
                }
            }

            return examples;
        }

        public bool HasExamples
        {
            get
            {
                if ((_Examples != null) && (_Examples.Count() != 0))
                    return true;
                return false;
            }
        }

        public List<Inflection> Inflections
        {
            get
            {
                return _Inflections;
            }
            set
            {
                _Inflections = value;
            }
        }

        public Inflection FindRegularInflection(
            string inflected,
            LanguageID languageID)
        {
            if (_Inflections == null)
                return null;

            Inflection inflection = _Inflections.FirstOrDefault(
                x => TextUtilities.IsEqualStringsIgnoreCase(x.GetRegularOutput(languageID), inflected));

            return inflection;
        }

        public Inflection FindRegularPronounInflection(
            string inflected,
            LanguageID languageID)
        {
            if (_Inflections == null)
                return null;

            Inflection inflection = _Inflections.FirstOrDefault(
                x => TextUtilities.IsEqualStringsIgnoreCase(x.GetRegularPronounOutput(languageID), inflected));

            return inflection;
        }

        public Inflection FindOutputInflection(
            string inflected,
            LanguageID languageID)
        {
            if (_Inflections == null)
                return null;

            Inflection inflection = _Inflections.FirstOrDefault(
                x => TextUtilities.IsEqualStringsIgnoreCase(x.GetOutput(languageID), inflected));

            return inflection;
        }

        public Inflection GetInflectionIndexed(int index)
        {
            if (_Inflections == null)
                return null;

            if ((index >= 0) && (index < _Inflections.Count()))
                return _Inflections[index];

            return null;
        }

        public void AddInflection(Inflection inflection)
        {
            if (_Inflections == null)
                _Inflections = new List<Inflection>();

            _Inflections.Add(inflection);
        }

        public void AddInflections(List<Inflection> inflections)
        {
            if (inflections == null)
                return;

            if (_Inflections == null)
                _Inflections = new List<Inflection>();

            _Inflections.AddRange(inflections);
        }

        public void MergeInflections(List<Inflection> inflections)
        {
            if (inflections == null)
                return;

            if (_Inflections == null)
            {
                _Inflections = new List<Inflection>(inflections);
                return;
            }

            if (_Inflections.Count() == 0)
                _Inflections.AddRange(inflections);
            else
            {
                foreach (Inflection inflection in inflections)
                {
                    if (_Inflections.FirstOrDefault(x => x.QuickCompare(inflection) == 0) == null)
                        _Inflections.Add(inflection);
                }
            }
        }

        public int InflectionCount()
        {
            if (_Inflections == null)
                return 0;

            return _Inflections.Count();
        }

        public bool HasInflections()
        {
            if ((_Inflections == null) || (_Inflections.Count() == 0))
                return false;

            return true;
        }

        public List<Inflection> CloneInflections()
        {
            List<Inflection> newInflections = null;

            if ((_Inflections != null) && (_Inflections.Count() != 0))
            {
                newInflections = new List<Inflection>();

                foreach (Inflection inflection in _Inflections)
                    newInflections.Add(new Inflection(inflection));
            }

            return newInflections;
        }

        public string GetInflectionsDisplay()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("I(");

            if ((_Inflections != null) && (_Inflections.Count() != 0))
            {
                bool first = true;

                foreach (Inflection inflection in _Inflections)
                {
                    if (first)
                        first = false;
                    else
                        sb.Append(", ");

                    sb.Append(inflection.Label);
                }
            }

            sb.Append(")");

            return sb.ToString();
        }

        public void GetDumpString(string prefix, StringBuilder sb)
        {
            sb.AppendLine(prefix + "Category: " + Category.ToString());
            sb.AppendLine(prefix + "CategoryString: " + CategoryString);

            if (LanguageSynonyms != null)
            {
                sb.AppendLine(prefix + "LanguageSynonyms:");

                foreach (LanguageSynonyms languageSynonyms in LanguageSynonyms)
                    languageSynonyms.GetDumpString(prefix + "    ", sb);
            }

            if (HasInflections())
                sb.AppendLine(GetInflectionsDisplay());

            if (Examples != null)
            {
                sb.AppendLine(prefix + "Examples:");

                foreach (MultiLanguageString example in Examples)
                {
                    if (example.LanguageStrings != null)
                    {
                        foreach (LanguageString languageString in example.LanguageStrings)
                            sb.AppendLine(prefix + "    " + languageString.LanguageID.LanguageName(LanguageLookup.English) + ": " + languageString.Text);
                    }
                }
            }
        }

        public bool Modified
        {
            get
            {
                return _Modified;
            }
            set
            {
                _Modified = value;
            }
        }

        public static LexicalCategory GetLexicalCategoryFromString(string lexicalCategoryString)
        {
            LexicalCategory code;

            switch (lexicalCategoryString)
            {
                case "Unknown":
                    code = LexicalCategory.Unknown;
                    break;
                case "Noun":
                    code = LexicalCategory.Noun;
                    break;
                case "ProperNoun":
                    code = LexicalCategory.ProperNoun;
                    break;
                case "Pronoun":
                    code = LexicalCategory.Pronoun;
                    break;
                case "Determiner":
                    code = LexicalCategory.Determiner;
                    break;
                case "Adjective":
                    code = LexicalCategory.Adjective;
                    break;
                case "Verb":
                    code = LexicalCategory.Verb;
                    break;
                case "Adverb":
                    code = LexicalCategory.Adverb;
                    break;
                case "Preposition":
                    code = LexicalCategory.Preposition;
                    break;
                case "Conjunction":
                    code = LexicalCategory.Conjunction;
                    break;
                case "Interjection":
                    code = LexicalCategory.Interjection;
                    break;
                case "Particle":
                    code = LexicalCategory.Particle;
                    break;
                case "Article":
                    code = LexicalCategory.Article;
                    break;
                case "MeasureWord":
                    code = LexicalCategory.MeasureWord;
                    break;
                case "Number":
                    code = LexicalCategory.Number;
                    break;
                case "Prefix":
                    code = LexicalCategory.Prefix;
                    break;
                case "Suffix":
                    code = LexicalCategory.Suffix;
                    break;
                case "Abbreviation":
                    code = LexicalCategory.Abbreviation;
                    break;
                case "Acronym":
                    code = LexicalCategory.Acronym;
                    break;
                case "Symbol":
                    code = LexicalCategory.Symbol;
                    break;
                case "Phrase":
                    code = LexicalCategory.Phrase;
                    break;
                case "PrepositionalPhrase":
                    code = LexicalCategory.PrepositionalPhrase;
                    break;
                case "Proverb":
                    code = LexicalCategory.Proverb;
                    break;
                case "Contraction":
                    code = LexicalCategory.Contraction;
                    break;
                case "Idiom":
                    code = LexicalCategory.Idiom;
                    break;
                case "Stem":
                    code = LexicalCategory.Stem;
                    break;
                case "IrregularStem":
                    code = LexicalCategory.IrregularStem;
                    break;
                case "Inflection":
                    code = LexicalCategory.Inflection;
                    break;
                case "NotFound":
                    code = LexicalCategory.NotFound;
                    break;
                default:
                    throw new ObjectException("Sense.GetLexicalCategoryFromString:  Unknown category:  " + lexicalCategoryString);
            }

            return code;
        }

        public static bool GetLexicalCategoryFromString(
            LanguageID languageID,
            string lexicalCategoryString,
            out LexicalCategory lexicalCategory)
        {
            foreach (MultiLanguageString mls in LexicalCategoryNames)
            {
                if (mls.Text(languageID).ToLower() == lexicalCategoryString.ToLower())
                {
                    lexicalCategory = (LexicalCategory)mls.Key;
                    return true;
                }
            }

            if (languageID != LanguageLookup.English)
            {
                foreach (MultiLanguageString mls in LexicalCategoryNames)
                {
                    if (mls.Text(LanguageLookup.English).ToLower() == lexicalCategoryString.ToLower())
                    {
                        lexicalCategory = (LexicalCategory)mls.Key;
                        return true;
                    }
                }
            }

            lexicalCategory = LexicalCategory.Unknown;

            return false;
        }

        public static bool GetStringFromLexicalCategory(
            LexicalCategory lexicalCategory,
            out string lexicalCategoryString)
        {
            lexicalCategoryString = String.Empty;

            foreach (MultiLanguageString mls in LexicalCategoryNames)
            {
                if (mls.KeyString == lexicalCategory.ToString())
                {
                    lexicalCategoryString = mls.Text(LanguageLookup.English);
                    return true;
                }
            }

            return false;
        }

        public static bool GetStringFromLexicalCategory(
            LanguageID languageID,
            LexicalCategory lexicalCategory,
            LanguageUtilities languageUtilities,
            out string lexicalCategoryString)
        {
            lexicalCategoryString = String.Empty;

            foreach (MultiLanguageString mls in LexicalCategoryNames)
            {
                if (mls.KeyString == lexicalCategory.ToString())
                {
                    lexicalCategoryString = mls.Text(languageID);

                    if (String.IsNullOrEmpty(lexicalCategoryString))
                    {
                        string english = mls.Text(LanguageLookup.English);
                        BaseString translatedString = languageUtilities.TranslateString(
                            english,
                            english,
                            languageID,
                            languageUtilities.UIStrings);

                        if (translatedString != null)
                        {
                            lexicalCategoryString = translatedString.Text;
                            LanguageString ls = new LanguageString(english, languageID, lexicalCategoryString);
                            mls.Add(ls);
                        }
                        else
                            return false;
                    }

                    return true;
                }
            }

            return false;
        }

        public static List<MultiLanguageString> LexicalCategoryNames = new List<MultiLanguageString>()
        {
            new MultiLanguageString(LexicalCategory.Unknown, new LanguageString(LexicalCategory.Unknown, LanguageLookup.English, "Unknown")),
            new MultiLanguageString(LexicalCategory.Multiple, new LanguageString(LexicalCategory.Multiple, LanguageLookup.English, "Multiple")),
            new MultiLanguageString(LexicalCategory.Noun, new LanguageString(LexicalCategory.Noun, LanguageLookup.English, "Noun")),
            new MultiLanguageString(LexicalCategory.ProperNoun, new LanguageString(LexicalCategory.ProperNoun, LanguageLookup.English, "ProperNoun")),
            new MultiLanguageString(LexicalCategory.Pronoun, new LanguageString(LexicalCategory.Pronoun, LanguageLookup.English, "Pronoun")),
            new MultiLanguageString(LexicalCategory.Determiner, new LanguageString(LexicalCategory.Determiner, LanguageLookup.English, "Determiner")),
            new MultiLanguageString(LexicalCategory.Adjective, new LanguageString(LexicalCategory.Adjective, LanguageLookup.English, "Adjective")),
            new MultiLanguageString(LexicalCategory.Verb, new LanguageString(LexicalCategory.Verb, LanguageLookup.English, "Verb")),
            new MultiLanguageString(LexicalCategory.Adverb, new LanguageString(LexicalCategory.Adverb, LanguageLookup.English, "Adverb")),
            new MultiLanguageString(LexicalCategory.Preposition, new LanguageString(LexicalCategory.Preposition, LanguageLookup.English, "Preposition")),
            new MultiLanguageString(LexicalCategory.Conjunction, new LanguageString(LexicalCategory.Conjunction, LanguageLookup.English, "Conjunction")),
            new MultiLanguageString(LexicalCategory.Interjection, new LanguageString(LexicalCategory.Interjection, LanguageLookup.English, "Interjection")),
            new MultiLanguageString(LexicalCategory.Particle, new LanguageString(LexicalCategory.Particle, LanguageLookup.English, "Particle")),
            new MultiLanguageString(LexicalCategory.Article, new LanguageString(LexicalCategory.Article, LanguageLookup.English, "Article")),
            new MultiLanguageString(LexicalCategory.MeasureWord, new LanguageString(LexicalCategory.MeasureWord, LanguageLookup.English, "MeasureWord")),
            new MultiLanguageString(LexicalCategory.Number, new LanguageString(LexicalCategory.Number, LanguageLookup.English, "Number")),
            new MultiLanguageString(LexicalCategory.Prefix, new LanguageString(LexicalCategory.Prefix, LanguageLookup.English, "Prefix")),
            new MultiLanguageString(LexicalCategory.Suffix, new LanguageString(LexicalCategory.Suffix, LanguageLookup.English, "Suffix")),
            new MultiLanguageString(LexicalCategory.Abbreviation, new LanguageString(LexicalCategory.Abbreviation, LanguageLookup.English, "Abbreviation")),
            new MultiLanguageString(LexicalCategory.Acronym, new LanguageString(LexicalCategory.Acronym, LanguageLookup.English, "Acronym")),
            new MultiLanguageString(LexicalCategory.Symbol, new LanguageString(LexicalCategory.Symbol, LanguageLookup.English, "Symbol")),
            new MultiLanguageString(LexicalCategory.Phrase, new LanguageString(LexicalCategory.Phrase, LanguageLookup.English, "Phrase")),
            new MultiLanguageString(LexicalCategory.PrepositionalPhrase, new LanguageString(LexicalCategory.PrepositionalPhrase, LanguageLookup.English, "PrepositionalPhrase")),
            new MultiLanguageString(LexicalCategory.Proverb, new LanguageString(LexicalCategory.Proverb, LanguageLookup.English, "Proverb")),
            new MultiLanguageString(LexicalCategory.Contraction, new LanguageString(LexicalCategory.Contraction, LanguageLookup.English, "Contraction")),
            new MultiLanguageString(LexicalCategory.Idiom, new LanguageString(LexicalCategory.Idiom, LanguageLookup.English, "Idiom")),
            new MultiLanguageString(LexicalCategory.Stem, new LanguageString(LexicalCategory.Stem, LanguageLookup.English, "Stem")),
            new MultiLanguageString(LexicalCategory.IrregularStem, new LanguageString(LexicalCategory.IrregularStem, LanguageLookup.English, "IrregularStem")),
            new MultiLanguageString(LexicalCategory.Inflection, new LanguageString(LexicalCategory.Inflection, LanguageLookup.English, "Inflection")),
            new MultiLanguageString(LexicalCategory.NotFound, new LanguageString(LexicalCategory.NotFound, LanguageLookup.English, "NotFound"))
        };

        public MultiLanguageString GetLexicalCategoryNameStrings()
        {
            MultiLanguageString multiLanguageString = LexicalCategoryNames.FirstOrDefault(x => (LexicalCategory)x.Key == _Category);
            return multiLanguageString;
        }

        public static MultiLanguageString GetLexicalCategoryNameStrings(LexicalCategory category)
        {
            MultiLanguageString multiLanguageString = LexicalCategoryNames.FirstOrDefault(x => (LexicalCategory)x.Key == category);
            return multiLanguageString;
        }

        public string GetLexicalCategoryName(LanguageID languageID)
        {
            MultiLanguageString multiLanguageString = GetLexicalCategoryNameStrings();
            if (multiLanguageString == null)
                return "";
            string text = multiLanguageString.Text(languageID);
            if (String.IsNullOrEmpty(text))
            {
                text = multiLanguageString.Text(LanguageLookup.English);
                if (ApplicationData.Translator != null)
                {
                    string translated;
                    LanguageTranslatorSource translatorSource;
                    string error;
                    if (ApplicationData.Translator.TranslateString(
                            "UITranslation",
                            "UIStrings",
                            text,
                            text,
                            LanguageLookup.English,
                            languageID,
                            out translated,
                            out translatorSource,
                            out error))
                        text = translated;
                }
            }
            return text;
        }

        public static string GetLexicalCategoryName(LexicalCategory category, LanguageID languageID)
        {
            MultiLanguageString multiLanguageString = GetLexicalCategoryNameStrings(category);
            string text = multiLanguageString.Text(languageID);
            if (String.IsNullOrEmpty(text))
            {
                text = multiLanguageString.Text(LanguageLookup.English);
                if (ApplicationData.Translator != null)
                {
                    string translated;
                    LanguageTranslatorSource translatorSource;
                    string error;
                    if (ApplicationData.Translator.TranslateString(
                            "UITranslation",
                            "UIStrings",
                            text,
                            text,
                            LanguageLookup.English,
                            languageID,
                            out translated,
                            out translatorSource,
                            out error))
                        text = translated;
                }
            }
            return text;
        }

        public string GetLexicalCategoryTag(LanguageID languageID)
        {
            if (!String.IsNullOrEmpty(_CategoryString))
                return "(" + _CategoryString + ") ";

            if (_Category == LexicalCategory.Unknown)
                return String.Empty;

            return "(" + GetLexicalCategoryName(LanguageLookup.English) + ") ";
        }

        public static List<string> GetLexicalCategoryNames(LanguageID languageID)
        {
            List<string> names = new List<string>(LexicalCategoryNames.Count());
            if (!LexicalCategoryNames[0].HasLanguageID(languageID))
                return null;
            foreach (MultiLanguageString multiLanguageString in LexicalCategoryNames)
            {
                string text = multiLanguageString.Text(languageID);
                if (String.IsNullOrEmpty(text))
                {
                    text = multiLanguageString.Text(LanguageLookup.English);
                    if (ApplicationData.Translator != null)
                    {
                        string translated;
                        LanguageTranslatorSource translatorSource;
                        string error;
                        if (ApplicationData.Translator.TranslateString(
                                "UITranslation",
                                "UIStrings",
                                text,
                                text,
                                LanguageLookup.English,
                                languageID,
                                out translated,
                                out translatorSource,
                                out error))
                            text = translated;
                    }
                }
                names.Add(text);
            }
            return names;
        }

        public static List<MultiLanguageString> LexicalCategoryAbbreviations = new List<MultiLanguageString>()
        {
            new MultiLanguageString(LexicalCategory.Unknown, new LanguageString(LexicalCategory.Unknown, LanguageLookup.English, "Un")),
            new MultiLanguageString(LexicalCategory.Multiple, new LanguageString(LexicalCategory.Multiple, LanguageLookup.English, "Mult")),
            new MultiLanguageString(LexicalCategory.Noun, new LanguageString(LexicalCategory.Noun, LanguageLookup.English, "N")),
            new MultiLanguageString(LexicalCategory.ProperNoun, new LanguageString(LexicalCategory.ProperNoun, LanguageLookup.English, "PN")),
            new MultiLanguageString(LexicalCategory.Pronoun, new LanguageString(LexicalCategory.Pronoun, LanguageLookup.English, "Pr")),
            new MultiLanguageString(LexicalCategory.Determiner, new LanguageString(LexicalCategory.Determiner, LanguageLookup.English, "Dt")),
            new MultiLanguageString(LexicalCategory.Adjective, new LanguageString(LexicalCategory.Adjective, LanguageLookup.English, "Adj")),
            new MultiLanguageString(LexicalCategory.Verb, new LanguageString(LexicalCategory.Verb, LanguageLookup.English, "V")),
            new MultiLanguageString(LexicalCategory.Adverb, new LanguageString(LexicalCategory.Adverb, LanguageLookup.English, "Adv")),
            new MultiLanguageString(LexicalCategory.Preposition, new LanguageString(LexicalCategory.Preposition, LanguageLookup.English, "Prep")),
            new MultiLanguageString(LexicalCategory.Conjunction, new LanguageString(LexicalCategory.Conjunction, LanguageLookup.English, "Con")),
            new MultiLanguageString(LexicalCategory.Interjection, new LanguageString(LexicalCategory.Interjection, LanguageLookup.English, "Int")),
            new MultiLanguageString(LexicalCategory.Particle, new LanguageString(LexicalCategory.Particle, LanguageLookup.English, "Part")),
            new MultiLanguageString(LexicalCategory.Article, new LanguageString(LexicalCategory.Article, LanguageLookup.English, "Art")),
            new MultiLanguageString(LexicalCategory.MeasureWord, new LanguageString(LexicalCategory.MeasureWord, LanguageLookup.English, "MW")),
            new MultiLanguageString(LexicalCategory.Number, new LanguageString(LexicalCategory.Number, LanguageLookup.English, "Num")),
            new MultiLanguageString(LexicalCategory.Prefix, new LanguageString(LexicalCategory.Prefix, LanguageLookup.English, "Pfx")),
            new MultiLanguageString(LexicalCategory.Suffix, new LanguageString(LexicalCategory.Suffix, LanguageLookup.English, "Sfx")),
            new MultiLanguageString(LexicalCategory.Abbreviation, new LanguageString(LexicalCategory.Abbreviation, LanguageLookup.English, "Abbr")),
            new MultiLanguageString(LexicalCategory.Acronym, new LanguageString(LexicalCategory.Acronym, LanguageLookup.English, "Acr")),
            new MultiLanguageString(LexicalCategory.Symbol, new LanguageString(LexicalCategory.Symbol, LanguageLookup.English, "Sym")),
            new MultiLanguageString(LexicalCategory.Phrase, new LanguageString(LexicalCategory.Phrase, LanguageLookup.English, "Phr")),
            new MultiLanguageString(LexicalCategory.PrepositionalPhrase, new LanguageString(LexicalCategory.PrepositionalPhrase, LanguageLookup.English, "PP")),
            new MultiLanguageString(LexicalCategory.Proverb, new LanguageString(LexicalCategory.Proverb, LanguageLookup.English, "Prov")),
            new MultiLanguageString(LexicalCategory.Contraction, new LanguageString(LexicalCategory.Contraction, LanguageLookup.English, "Con")),
            new MultiLanguageString(LexicalCategory.Idiom, new LanguageString(LexicalCategory.Idiom, LanguageLookup.English, "Idm")),
            new MultiLanguageString(LexicalCategory.Stem, new LanguageString(LexicalCategory.Stem, LanguageLookup.English, "Stm")),
            new MultiLanguageString(LexicalCategory.IrregularStem, new LanguageString(LexicalCategory.IrregularStem, LanguageLookup.English, "IrrStm")),
            new MultiLanguageString(LexicalCategory.Inflection, new LanguageString(LexicalCategory.Inflection, LanguageLookup.English, "Infl")),
            new MultiLanguageString(LexicalCategory.NotFound, new LanguageString(LexicalCategory.NotFound, LanguageLookup.English, "NF"))
        };

        public MultiLanguageString GetLexicalCategoryAbbreviationStrings()
        {
            MultiLanguageString multiLanguageString = LexicalCategoryAbbreviations.FirstOrDefault(x => (LexicalCategory)x.Key == _Category);
            return multiLanguageString;
        }

        public static MultiLanguageString GetLexicalCategoryAbbreviationStrings(LexicalCategory category)
        {
            MultiLanguageString multiLanguageString = LexicalCategoryAbbreviations.FirstOrDefault(x => (LexicalCategory)x.Key == category);
            return multiLanguageString;
        }

        public string GetLexicalCategoryAbbreviation(LanguageID languageID)
        {
            MultiLanguageString multiLanguageString = GetLexicalCategoryAbbreviationStrings();
            if (multiLanguageString == null)
                return "";
            string text = multiLanguageString.Text(languageID);
            if (String.IsNullOrEmpty(text))
            {
                text = multiLanguageString.Text(LanguageLookup.English);
                if (ApplicationData.Translator != null)
                {
                    string translated;
                    LanguageTranslatorSource translatorSource;
                    string error;
                    if (ApplicationData.Translator.TranslateString(
                            "UITranslation",
                            "UIStrings",
                            text,
                            text,
                            LanguageLookup.English,
                            languageID,
                            out translated,
                            out translatorSource,
                            out error))
                        text = translated;
                }
            }
            return text;
        }

        public static string GetLexicalCategoryAbbreviation(LexicalCategory category, LanguageID languageID)
        {
            MultiLanguageString multiLanguageString = GetLexicalCategoryAbbreviationStrings(category);
            string text = multiLanguageString.Text(languageID);
            if (String.IsNullOrEmpty(text))
            {
                text = multiLanguageString.Text(LanguageLookup.English);
                if (ApplicationData.Translator != null)
                {
                    string translated;
                    LanguageTranslatorSource translatorSource;
                    string error;
                    if (ApplicationData.Translator.TranslateString(
                            "UITranslation",
                            "UIStrings",
                            text,
                            text,
                            LanguageLookup.English,
                            languageID,
                            out translated,
                            out translatorSource,
                            out error))
                        text = translated;
                }
            }
            return text;
        }

        public string GetLexicalCategoryAbbreviationTag(LanguageID languageID)
        {
            if (!String.IsNullOrEmpty(_CategoryString))
                return "(" + _CategoryString + ") ";

            if (_Category == LexicalCategory.Unknown)
                return String.Empty;

            return "(" + GetLexicalCategoryAbbreviation(languageID) + ") ";
        }

        public static List<string> GetLexicalCategoryAbbreviations(LanguageID languageID)
        {
            List<string> names = new List<string>(LexicalCategoryAbbreviations.Count());
            if (!LexicalCategoryAbbreviations[0].HasLanguageID(languageID))
                return null;
            foreach (MultiLanguageString multiLanguageString in LexicalCategoryAbbreviations)
            {
                string text = multiLanguageString.Text(languageID);
                if (String.IsNullOrEmpty(text))
                {
                    text = multiLanguageString.Text(LanguageLookup.English);
                    if (ApplicationData.Translator != null)
                    {
                        string translated;
                        LanguageTranslatorSource translatorSource;
                        string error;
                        if (ApplicationData.Translator.TranslateString(
                                "UITranslation",
                                "UIStrings",
                                text,
                                text,
                                LanguageLookup.English,
                                languageID,
                                out translated,
                                out translatorSource,
                                out error))
                            text = translated;
                    }
                }
                names.Add(text);
            }
            return names;
        }

        public bool HasCategoryStringCode(string code)
        {
            if (!String.IsNullOrEmpty(_CategoryString))
            {
                string[] parts = _CategoryString.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);

                foreach (string part in parts)
                {
                    if (part == code)
                        return true;
                }
            }

            return false;
        }

        public virtual XElement GetElement(string name)
        {
            XElement element = new XElement(name);
            if (_Reading != 0)
                element.Add(new XAttribute("Reading", _Reading));
            element.Add(new XAttribute("Category", _Category.ToString()));
            if (!String.IsNullOrEmpty(_CategoryString))
                element.Add(new XAttribute("CategoryString", _CategoryString));
            if (_PriorityLevel != 0)
                element.Add(new XAttribute("PriorityLevel", _PriorityLevel));
            if (_LanguageSynonyms != null)
            {
                foreach (LanguageSynonyms synonyms in _LanguageSynonyms)
                    element.Add(synonyms.GetElement("LanguageSynonyms"));
            }
            if (_Examples != null)
            {
                foreach (MultiLanguageString example in _Examples)
                    element.Add(example.GetElement("Example"));
            }
            return element;
        }

        public virtual bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Reading":
                    try
                    {
                        _Reading = Convert.ToInt32(attributeValue);
                    }
                    catch (Exception)
                    {
                        _Reading = 0;
                    }
                    break;
                case "Category":
                    _Category = GetLexicalCategoryFromString(attributeValue);
                    break;
                case "CategoryString":
                    _CategoryString = attributeValue;
                    break;
                case "PriorityLevel":
                    try
                    {
                        _PriorityLevel = Convert.ToInt32(attributeValue);
                    }
                    catch (Exception)
                    {
                        _PriorityLevel = 0;
                    }
                    break;
                default:
                    return false;
            }

            return true;
        }

        public virtual bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "LanguageSynonyms":
                    if (_LanguageSynonyms == null)
                        _LanguageSynonyms = new List<LanguageSynonyms>();
                    _LanguageSynonyms.Add(new LanguageSynonyms(childElement));
                    break;
                case "Example":
                    if (_Examples == null)
                        _Examples = new List<MultiLanguageString>();
                    _Examples.Add(new MultiLanguageString(childElement));
                    break;
                default:
                    return false;
            }

            return true;
        }

        public virtual void OnElement(XElement element)
        {
            Clear();

            foreach (XAttribute attribute in element.Attributes())
            {
                if (!OnAttribute(attribute))
                    throw new ObjectException("Unknown attribute " + attribute.Name + " in " + element.Name.ToString() + ".");
            }

            foreach (var childNode in element.Elements())
            {
                XElement childElement = childNode as XElement;

                if (childElement == null)
                    continue;

                if (!OnChildElement(childElement))
                    throw new ObjectException("Unknown child element " + childElement.Name + " in " + element.Name.ToString() + ".");
            }

            _Modified = false;
        }

        public bool Match(Sense other)
        {
            if (other.Reading != _Reading)
                return false;

            LexicalCategory thisCategory = _Category;
            LexicalCategory otherCategory = other.Category;

            if (thisCategory == LexicalCategory.Unknown)
            {
                int languageSynonymsCount = LanguageSynonymsCount;

                for (int languageSynonymsIndex = 0; languageSynonymsIndex < languageSynonymsCount; languageSynonymsIndex++)
                {
                    LanguageSynonyms ls = GetLanguageSynonymsIndexed(languageSynonymsIndex);

                    if (ls == null)
                        break;

                    int synonymCount = ls.ProbableSynonymCount;
                    int synonymIndex;

                    for (synonymIndex = 0; synonymIndex < synonymCount; synonymIndex++)
                    {
                        ProbableMeaning ps = ls.GetProbableSynonymIndexed(synonymIndex);

                        if (ps == null)
                            break;

                        if (ps.Category != LexicalCategory.Unknown)
                        {
                            if (thisCategory == LexicalCategory.Unknown)
                                thisCategory = ps.Category;
                            else if (ps.Category != thisCategory)
                                return false;
                        }
                    }
                }
            }

            if (otherCategory == LexicalCategory.Unknown)
            {
                int languageSynonymsCount = LanguageSynonymsCount;

                for (int languageSynonymsIndex = 0; languageSynonymsIndex < languageSynonymsCount; languageSynonymsIndex++)
                {
                    LanguageSynonyms ls = GetLanguageSynonymsIndexed(languageSynonymsIndex);

                    if (ls == null)
                        break;

                    int synonymCount = ls.ProbableSynonymCount;
                    int synonymIndex;

                    for (synonymIndex = 0; synonymIndex < synonymCount; synonymIndex++)
                    {
                        ProbableMeaning ps = ls.GetProbableSynonymIndexed(synonymIndex);

                        if (ps == null)
                            break;

                        if (ps.Category != LexicalCategory.Unknown)
                        {
                            if (otherCategory == LexicalCategory.Unknown)
                                otherCategory = ps.Category;
                            else if (ps.Category != otherCategory)
                                return false;
                        }
                    }
                }
            }

            if ((otherCategory != LexicalCategory.Unknown) &&
                    (thisCategory != LexicalCategory.Unknown) &&
                    (otherCategory != thisCategory))
                return false;

            if (!String.IsNullOrEmpty(other.CategoryString) &&
                    !String.IsNullOrEmpty(_CategoryString) &&
                    (other.CategoryString != _CategoryString))
                return false;

            if (other.PriorityLevel != _PriorityLevel)
                return false;

            if ((other.LanguageSynonyms == null) && (_LanguageSynonyms == null))
                return true;

            if ((other.LanguageSynonyms == null) || (_LanguageSynonyms == null))
                return false;

            if (other.LanguageSynonyms.Count() != LanguageSynonyms.Count())
                return false;

            foreach (LanguageSynonyms languageSynonyms in other.LanguageSynonyms)
            {
                if (_LanguageSynonyms.FirstOrDefault(x => x.Match(languageSynonyms)) == null)
                    return false;
            }

            if ((other.Inflections == null) && (_Inflections == null))
                return true;

            if ((other.Inflections == null) || (_Inflections == null))
                return false;

            if (other.Inflections.Count() != Inflections.Count())
                return false;

            foreach (Inflection inflection in other.Inflections)
            {
                if (_Inflections.FirstOrDefault(x => x.QuickCompare(inflection) == 0) == null)
                    return false;
            }

            if ((other.Examples == null) && (_Examples == null))
                return true;

            if ((other.Examples == null) || (_Examples == null))
                return false;

            if (other.Examples.Count() != Examples.Count())
                return false;

            foreach (MultiLanguageString example in other.Examples)
            {
                if (_Examples.FirstOrDefault(x => MultiLanguageString.Compare(x, example) == 0) == null)
                    return false;
            }

            return true;
        }

        public bool CanOverlay(Sense other)
        {
            if (other.Reading != _Reading)
                return false;

            if ((other.Category != LexicalCategory.Unknown) &&
                    (_Category != LexicalCategory.Unknown) &&
                    (other.Category != _Category))
                return false;

            if (!String.IsNullOrEmpty(other.CategoryString) &&
                    !String.IsNullOrEmpty(_CategoryString) &&
                    (other.CategoryString != _CategoryString))
                return false;

            if (other.PriorityLevel != _PriorityLevel)
                return false;

            if ((other.LanguageSynonyms == null) && (_LanguageSynonyms == null))
                return true;

            if ((other.LanguageSynonyms == null) || (_LanguageSynonyms == null))
                return false;

            int matchCount = 0;

            foreach (LanguageSynonyms ls2 in other.LanguageSynonyms)
            {
                foreach (LanguageSynonyms ls1 in LanguageSynonyms)
                {
                    if (ls1.CanOverlay(ls2))
                    {
                        matchCount++;
                        break;
                    }
                }
            }

            if (matchCount == other.LanguageSynonymsCount)
                return true;

            return false;
        }

        public int Compare(Sense other)
        {
            int diff, count;

            if ((this.LanguageSynonyms == null) && (other.LanguageSynonyms == null))
                return 0;
            if (this.LanguageSynonyms == null)
                return -1;
            if (other.LanguageSynonyms == null)
                return 1;
            if (this.LanguageSynonyms.Count() != other.LanguageSynonyms.Count())
                return this.LanguageSynonyms.Count() - other.LanguageSynonyms.Count();
            count = this.LanguageSynonyms.Count();
            for (int i = 0; i < count; i++)
            {
                diff = this.LanguageSynonyms[i].Compare(other.LanguageSynonyms[i]);
                if (diff != 0)
                    return diff;
            }

            diff = ObjectUtilities.CompareInts(other.Reading, _Reading);

            if (diff != 0)
                return diff;

            if (other.Category > _Category)
                return 1;
            else if (other.Category < _Category)
                return -1;

            diff = ObjectUtilities.CompareStrings(other.CategoryString, _CategoryString);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareInts(other.PriorityLevel, _PriorityLevel);

            if (diff != 0)
                return diff;

            if ((this.Examples == null) && (other.Examples == null))
                return 0;
            if (this.Examples == null)
                return -1;
            if (other.Examples == null)
                return 1;
            if (this.Examples.Count() != other.Examples.Count())
                return this.Examples.Count() - other.Examples.Count();
            count = this.Examples.Count();
            for (int i = 0; i < count; i++)
            {
                diff = MultiLanguageString.Compare(this.Examples[i], other.Examples[i]);
                if (diff != 0)
                    return diff;
            }

            if ((this.Inflections == null) && (other.Inflections == null))
                return 0;
            if (this.Inflections == null)
                return -1;
            if (other.Inflections == null)
                return 1;
            if (this.Inflections.Count() != other.Inflections.Count())
                return this.Inflections.Count() - other.Inflections.Count();
            count = this.Inflections.Count();
            for (int i = 0; i < count; i++)
            {
                diff = this.Inflections[i].Compare(other.Inflections[i]);
                if (diff != 0)
                    return diff;
            }

            return 0;
        }

        public static int ComparePriority(Sense other1, Sense other2)
        {
            if ((other1 != null) && (other2 != null))
                return (other1.PriorityLevel - other2.PriorityLevel);
            else if (other1 != null)
                return 1;
            else if (other2 != null)
                return -1;
            else
                return 0;
        }
    }
}
