using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    // Used for inflector table generation.
    // Target language IDs mirror language tool's.
    // This file stored in: JTLanguageWeb/Content/LocalData/InflectorTables/(languageCode)/InflectorDescription_(type)_(languageCode).xml
    public class InflectorDescription : BaseObjectLanguages
    {
        // Store documentation for the XML description file storing this object,
        protected XElement _InflectorDescriptionDocumentation;

        // Store documentation to be copied to generated inflector table.
        protected XElement _InflectorTableDocumentation;

        // Word type (part of speech) ("Verb", "Noun", "Adjective")
        protected string _Type;
        public static string[] Types = { "Verb", "Noun", "Adjective" };

        // Word inflection class keys. (Language-specific.  i.e. "ar", "er", "ir")
        protected List<string> _ClassKeys;

        // Language used for external inflector and word examples.
        protected LanguageID _ExternalLanguageID;

        // Stem type.
        protected string _StemType;
        public static string[] StemTypes =
        {
            "DictionaryForm",       // The stem is the dictionary form.
            "RemoveClassKey"        // Remove class key from dictionary form and you have the stem.
        };

        // Regular inflection word examples.
        protected List<WordDescriptor> _Regulars;

        // Irregular inflection word examples.
        protected List<WordDescriptor> _Irregulars;

        // Endings version - copied to inflector table.
        protected int _EndingsVersion;

        // Source inflectables - copied to inflector table, or generated if not present.
        protected List<Classifier> _EndingsSources;

        // Subject pronoun descriptors.
        protected List<TokenDescriptor> _SubjectPronouns;

        // Reflexive pronoun descriptors.
        protected List<TokenDescriptor> _ReflexivePronouns;

        // Direct object pronoun descriptors.
        protected List<TokenDescriptor> _DirectPronouns;

        // Indirect object pronoun descriptors.
        protected List<TokenDescriptor> _IndirectPronouns;

        // Gender suffix descriptors.
        protected List<TokenDescriptor> _GenderSuffixes;

        // For handling case where external conjugator doesn't do all the pronouns when the conjugation is the same.
        protected Dictionary<string, List<string>> _ExternalPronounExpansions;

        // Compound patterns. These describe the layout of a verb phrase or sentence patterns.
        protected List<PhrasePattern> _Patterns;

        // List of inflector families, for ordering purposes and patterns.
        protected List<InflectorFamily> _InflectorFamilies;

        // List of compound inflectors.
        protected List<CompoundInflector> _CompoundInflectors;

        // List of special inflectors.
        protected List<InflectorFamily> _SpecialInflectors;

        // List of helper definitions.
        protected List<DictionaryEntry> _HelperEntries;

        // Dictionary of helper inflections.
        protected Dictionary<string, Dictionary<string, string>> _HelperInflections;

        // Map external conjugator designator IDs to external labels.
        protected Dictionary<string, string> _ExternalIDToExternalLabelDictionary;

        // Map external conjugator designator label to our designator labels.
        protected Dictionary<string, string> _ExternalLabelToLabelDictionary;

        // Map our designator labels to the external IDs.
        protected Dictionary<string, LiteralString> _LabelToExternalIDDictionary;

        // Map external conjugator designator IDs to our designator labels.
        protected Dictionary<string, string> _ExternalIDToLabelDictionary;

        // The following are just copied to the inflector table.

        // Inflector filter list - copied to inflector table.
        protected List<InflectorFilter> _InflectorFilterList;

        // Automatic display row keys - copied to inflector table.
        protected List<string> _AutomaticRowKeys;

        // Automatic display column keys - copied to inflector table.
        protected List<string> _AutomaticColumnKeys;

        // Display major groups - copied to inflector table.
        protected List<InflectionsLayoutGroup> _MajorGroups;

        // Designation translations - copied to inflector table.
        protected List<Designator> _DesignationTranslations;

        // Map category strings to classes - copied to inflector table.
        protected Dictionary<string, Tuple<string, string>> _CategoryStringToClassMap;

        public InflectorDescription(
            string key,
            string type,
            List<LanguageID> targetLanguageIDs,
            string owner) : base(key, targetLanguageIDs, null, owner)
        {
            ClearBaseObjectLanguages();
            Type = type;
        }

        public InflectorDescription(XElement element)
        {
            ClearBaseObjectLanguages();
            OnElement(element);
        }

        public InflectorDescription(InflectorDescription other) : base(other)
        {
            ClearBaseObjectLanguages();
            CopyInflectorDescription(other);
        }

        public InflectorDescription()
        {
            ClearBaseObjectLanguages();
        }

        public override void Clear()
        {
            base.Clear();
            ClearBaseObjectLanguages();
        }

        public void ClearInflectorDescription()
        {
            _InflectorDescriptionDocumentation = null;
            _InflectorTableDocumentation = null;
            _Type = null;
            _ClassKeys = null;
            _ExternalLanguageID = null;
            _StemType = null;
            _Regulars = null;
            _Irregulars = null;
            _EndingsVersion = 0;
            _EndingsSources = null;
            _SubjectPronouns = null;
            _ReflexivePronouns = null;
            _DirectPronouns = null;
            _IndirectPronouns = null;
            _GenderSuffixes = null;
            _ExternalPronounExpansions = null;
            _Patterns = null;
            _InflectorFamilies = null;
            _CompoundInflectors = null;
            _SpecialInflectors = null;
            _HelperEntries = null;
            _HelperInflections = null;
            _ExternalIDToExternalLabelDictionary = null;
            _ExternalLabelToLabelDictionary = null;
            _LabelToExternalIDDictionary = null;
            _ExternalIDToLabelDictionary = null;
            _InflectorFilterList = new List<InflectorFilter>();
            _AutomaticRowKeys = null;
            _AutomaticColumnKeys = null;
            _MajorGroups = null;
            _DesignationTranslations = null;
            _CategoryStringToClassMap = null;
        }

        public virtual void CopyInflectorDescription(InflectorDescription other)
        {
            _InflectorDescriptionDocumentation = other.InflectorDescriptionDocumentation;
            _InflectorTableDocumentation = other.InflectorTableDocumentation;
            _Type = other.Type;
            _ClassKeys = other.ClassKeys;
            _ExternalLanguageID = other.ExternalLanguageID;
            _StemType = other.StemType;
            _Regulars = other.Regulars;
            _Irregulars = other.Irregulars;
            _EndingsVersion = other.EndingsVersion;
            _EndingsSources = other.EndingsSources;
            _SubjectPronouns = other.SubjectPronouns;
            _ReflexivePronouns = other.ReflexivePronouns;
            _DirectPronouns = other.DirectPronouns;
            _IndirectPronouns = other.IndirectPronouns;
            _GenderSuffixes = other.GenderSuffixes;
            _ExternalPronounExpansions = other.ExternalPronounExpansions;
            _Patterns = other.Patterns;
            _InflectorFamilies = other.InflectorFamilies;
            _CompoundInflectors = other.CompoundInflectors;
            _SpecialInflectors = other.SpecialInflectors;
            _HelperEntries = other.HelperEntries;
            _HelperInflections = other.HelperInflections;
            _ExternalIDToExternalLabelDictionary = other.ExternalIDToExternalLabelDictionary;
            _ExternalLabelToLabelDictionary = other.ExternalLabelToLabelDictionary;
            _LabelToExternalIDDictionary = other.LabelToExternalIDDictionary;
            _ExternalIDToLabelDictionary = other.ExternalIDToLabelDictionary;
            _AutomaticRowKeys = (other.AutomaticRowKeys != null ? new List<string>(other.AutomaticRowKeys) : null);
            _AutomaticColumnKeys = (other.AutomaticColumnKeys != null ? new List<string>(other.AutomaticColumnKeys) : null);
            _MajorGroups = (other.MajorGroups != null ? new List<InflectionsLayoutGroup>(other.MajorGroups) : null);
            _DesignationTranslations = other.DesignationTranslations;
            _CategoryStringToClassMap = other.CategoryStringToClassMap;
        }

        // Store documentation for the XML description file storing this object,
        public XElement InflectorDescriptionDocumentation
        {
            get
            {
                return _InflectorDescriptionDocumentation;
            }
            set
            {
                if (value != _InflectorDescriptionDocumentation)
                {
                    _InflectorDescriptionDocumentation = value;
                    Modified = true;
                }
            }
        }

        // Store documentation to be copied to generated inflector table.
        public XElement InflectorTableDocumentation
        {
            get
            {
                return _InflectorTableDocumentation;
            }
            set
            {
                if (value != _InflectorTableDocumentation)
                {
                    _InflectorTableDocumentation = value;
                    Modified = true;
                }
            }
        }

        // Word type (part of speech) ("Verb", "Noun", "Adjective")
        public string Type
        {
            get
            {
                return _Type;
            }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    Modified = true;
                }
            }
        }

        // Word inflection class keys. (Language-specific.  i.e. "ar", "er", "ir")
        public List<string> ClassKeys
        {
            get
            {
                return _ClassKeys;
            }
            set
            {
                if (value != _ClassKeys)
                {
                    _ClassKeys = value;
                    Modified = true;
                }
            }
        }

        // Language used for external inflector and word examples.
        public LanguageID ExternalLanguageID
        {
            get
            {
                return _ExternalLanguageID;
            }
            set
            {
                if (value != _ExternalLanguageID)
                {
                    _ExternalLanguageID = value;
                    Modified = true;
                }
            }
        }

        // Stem type.
        public string StemType
        {
            get
            {
                return _StemType;
            }
            set
            {
                if (value != _StemType)
                {
                    _StemType = value;
                    Modified = true;
                }
            }
        }

        // Regular inflection word examples.
        public List<WordDescriptor> Regulars
        {
            get
            {
                return _Regulars;
            }
            set
            {
                if (value != _Regulars)
                {
                    _Regulars = value;
                    Modified = true;
                }
            }
        }

        // Irregular inflection word examples.
        public List<WordDescriptor> Irregulars
        {
            get
            {
                return _Irregulars;
            }
            set
            {
                if (value != _Irregulars)
                {
                    _Irregulars = value;
                    Modified = true;
                }
            }
        }

        public bool HasIrregular(string word)
        {
            if (_Irregulars == null)
                return false;

            if (_Irregulars.FirstOrDefault(x => x.Word == word) != null)
                return true;

            return false;
        }

        public int EndingsVersion
        {
            get
            {
                return _EndingsVersion;
            }
            set
            {
                _EndingsVersion = value;
            }
        }

        public List<Classifier> EndingsSources
        {
            get
            {
                return _EndingsSources;
            }
            set
            {
                if (value != _EndingsSources)
                {
                    _EndingsSources = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int EndingsSourcesCount()
        {
            if (_EndingsSources == null)
                return 0;

            return _EndingsSources.Count();
        }

        public bool HasEndingsSource(string label)
        {
            if (String.IsNullOrEmpty(label) || (_EndingsSources == null))
                return false;

            if (_EndingsSources.FirstOrDefault(x => x.KeyString == label) == null)
                return false;

            return true;
        }

        public List<Classifier> GetEndingsSources(string label)
        {
            List<Classifier> endingsSources = null;

            if (String.IsNullOrEmpty(label) || (_EndingsSources == null))
                return null;

            foreach (Classifier endingsSource in _EndingsSources)
            {
                if (endingsSource.KeyString == label)
                {
                    if (endingsSources == null)
                        endingsSources = new List<Classifier>() { endingsSource };
                    else
                        endingsSources.Add(endingsSource);
                }
            }

            return endingsSources;
        }

        public Classifier GetEndingsSourceIndexed(int index)
        {
            if ((_EndingsSources == null) || (index < 0) || (index >= _EndingsSources.Count()))
                return null;

            Classifier endingsSource = _EndingsSources[index];

            return endingsSource;
        }

        public void AppendEndingsSource(Classifier endingsSource)
        {
            if (_EndingsSources == null)
                _EndingsSources = new List<Classifier>() { endingsSource };
            else
                _EndingsSources.Add(endingsSource);
        }

        public bool InsertEndingsSource(int index, Classifier endingsSource)
        {
            if (_EndingsSources == null)
                _EndingsSources = new List<Classifier>() { endingsSource };
            else
                _EndingsSources.Insert(index, endingsSource);

            return true;
        }

        public bool DeleteEndingsSource(Classifier endingsSource)
        {
            if ((endingsSource == null) || (_EndingsSources == null))
                return false;

            bool returnValue = _EndingsSources.Remove(endingsSource);

            return returnValue;
        }

        public bool DeleteEndingsSourceIndex(int index)
        {
            if ((_EndingsSources == null) || (index < 0) || (index >= _EndingsSources.Count()))
                return false;

            _EndingsSources.RemoveAt(index);

            return true;
        }

        public List<TokenDescriptor> GetIterateTokens(string iterate)
        {
            List<TokenDescriptor> iterators;

            switch (iterate)
            {
                case "SubjectPronouns":
                    iterators = SubjectPronouns;
                    break;
                case "ReflexivePronouns":
                    iterators = ReflexivePronouns;
                    break;
                case "DirectPronouns":
                    iterators = DirectPronouns;
                    break;
                case "IndirectPronouns":
                    iterators = IndirectPronouns;
                    break;
                case "GenderSuffixes":
                    iterators = GenderSuffixes;
                    break;
                default:
                    throw new Exception("Unexpected iteration type: " + iterate);
            }

            return iterators;
        }

        // Subject pronoun descriptors.
        public List<TokenDescriptor> SubjectPronouns
        {
            get
            {
                return _SubjectPronouns;
            }
            set
            {
                if (value != _SubjectPronouns)
                {
                    _SubjectPronouns = value;
                    Modified = true;
                }
            }
        }

        public TokenDescriptor FindSubjectPronoun(string pronoun)
        {
            if (_SubjectPronouns == null)
                return null;

            return _SubjectPronouns.FirstOrDefault(x => x.Text.Contains(pronoun));
        }

        public Dictionary<string, TokenDescriptor> SubjectProunounsByDescriptorLabel
        {
            get
            {
                if (_SubjectPronouns == null)
                    return null;

                Dictionary<string, TokenDescriptor> dictionary = new Dictionary<string, TokenDescriptor>();

                foreach (TokenDescriptor pronounDescriptor in _SubjectPronouns)
                {
                    foreach (Designator pronounDesignator in pronounDescriptor.Designators)
                        dictionary.Add(pronounDesignator.Label, pronounDescriptor);
                }

                return dictionary;
            }
        }

        // Reflexive pronoun descriptors.
        public List<TokenDescriptor> ReflexivePronouns
        {
            get
            {
                return _ReflexivePronouns;
            }
            set
            {
                if (value != _ReflexivePronouns)
                {
                    _ReflexivePronouns = value;
                    Modified = true;
                }
            }
        }

        public TokenDescriptor FindReflexivePronoun(string pronoun)
        {
            if (_ReflexivePronouns == null)
                return null;

            return _ReflexivePronouns.FirstOrDefault(x => x.Text.Contains(pronoun));
        }

        public Dictionary<string, TokenDescriptor> ReflexiveProunounsByDescriptorLabel
        {
            get
            {
                if (_ReflexivePronouns == null)
                    return null;

                Dictionary<string, TokenDescriptor> dictionary = new Dictionary<string, TokenDescriptor>();

                foreach (TokenDescriptor pronounDescriptor in _ReflexivePronouns)
                {
                    foreach (Designator pronounDesignator in pronounDescriptor.Designators)
                        dictionary.Add(pronounDesignator.Label, pronounDescriptor);
                }

                return dictionary;
            }
        }

        // Direct object pronoun descriptors.
        public List<TokenDescriptor> DirectPronouns
        {
            get
            {
                return _DirectPronouns;
            }
            set
            {
                if (value != _DirectPronouns)
                {
                    _DirectPronouns = value;
                    Modified = true;
                }
            }
        }

        public TokenDescriptor FindDirectPronoun(string pronoun)
        {
            if (_DirectPronouns == null)
                return null;

            return _DirectPronouns.FirstOrDefault(x => x.Text.Contains(pronoun));
        }

        public Dictionary<string, TokenDescriptor> DirectProunounsByDescriptorLabel
        {
            get
            {
                if (_DirectPronouns == null)
                    return null;

                Dictionary<string, TokenDescriptor> dictionary = new Dictionary<string, TokenDescriptor>();

                foreach (TokenDescriptor pronounDescriptor in _DirectPronouns)
                {
                    foreach (Designator pronounDesignator in pronounDescriptor.Designators)
                        dictionary.Add(pronounDesignator.Label, pronounDescriptor);
                }

                return dictionary;
            }
        }

        // Indirect object pronoun descriptors.
        public List<TokenDescriptor> IndirectPronouns
        {
            get
            {
                return _IndirectPronouns;
            }
            set
            {
                if (value != _IndirectPronouns)
                {
                    _IndirectPronouns = value;
                    Modified = true;
                }
            }
        }

        public TokenDescriptor FindIndirectPronoun(string pronoun)
        {
            if (_IndirectPronouns == null)
                return null;

            return _IndirectPronouns.FirstOrDefault(x => x.Text.Contains(pronoun));
        }

        public Dictionary<string, TokenDescriptor> IndirectProunounsByDescriptorLabel
        {
            get
            {
                if (_IndirectPronouns == null)
                    return null;

                Dictionary<string, TokenDescriptor> dictionary = new Dictionary<string, TokenDescriptor>();

                foreach (TokenDescriptor pronounDescriptor in _IndirectPronouns)
                {
                    foreach (Designator pronounDesignator in pronounDescriptor.Designators)
                        dictionary.Add(pronounDesignator.Label, pronounDescriptor);
                }

                return dictionary;
            }
        }

        // Gender suffix descriptors.
        public List<TokenDescriptor> GenderSuffixes
        {
            get
            {
                return _GenderSuffixes;
            }
            set
            {
                if (value != _GenderSuffixes)
                {
                    _GenderSuffixes = value;
                    Modified = true;
                }
            }
        }

        public TokenDescriptor FindGenderSuffix(string suffix)
        {
            if (_GenderSuffixes == null)
                return null;

            return _GenderSuffixes.FirstOrDefault(x => x.Text.Contains(suffix));
        }

        public Dictionary<string, TokenDescriptor> GenderSuffixByDescriptorLabel
        {
            get
            {
                if (_GenderSuffixes == null)
                    return null;

                Dictionary<string, TokenDescriptor> dictionary = new Dictionary<string, TokenDescriptor>();

                foreach (TokenDescriptor suffixDescriptor in _GenderSuffixes)
                {
                    foreach (Designator suffixDesignator in suffixDescriptor.Designators)
                        dictionary.Add(suffixDesignator.Label, suffixDescriptor);
                }

                return dictionary;
            }
        }

        // For handling case where external conjugator doesn't do all the pronouns when the conjugation is the same.
        public Dictionary<string, List<string>> ExternalPronounExpansions
        {
            get
            {
                return _ExternalPronounExpansions;
            }
            set
            {
                if (value != _ExternalPronounExpansions)
                {
                    _ExternalPronounExpansions = value;
                    Modified = true;
                }
            }
        }

        public List<TokenDescriptor> GetExternalPronounExpansions(string pronounKey)
        {
            if (_ExternalPronounExpansions == null)
                return null;

            List<string> pronounKeys;

            if (_ExternalPronounExpansions.TryGetValue(pronounKey, out pronounKeys))
            {
                List<TokenDescriptor> pronounDescriptors = new List<TokenDescriptor>();

                foreach (string pronoun in pronounKeys)
                {
                    TokenDescriptor pronounDescriptor = FindSubjectPronoun(pronoun);

                    if (pronounDescriptor == null)
                        pronounDescriptor = FindReflexivePronoun(pronoun);

                    if (pronounDescriptor == null)
                        pronounDescriptor = FindDirectPronoun(pronoun);

                    if (pronounDescriptor == null)
                        pronounDescriptor = FindIndirectPronoun(pronoun);

                    if (pronounDescriptor == null)
                        throw new Exception("GetExternalPronounExpansions: Couldn't find pronoun: " + pronounKey);

                    pronounDescriptors.Add(pronounDescriptor);
                }

                return pronounDescriptors;
            }

            return null;
        }

        // Compound patterns. These describe the layout of a verb phrase or sentence patterns.
        public List<PhrasePattern> Patterns
        {
            get
            {
                return _Patterns;
            }
            set
            {
                if (value != _Patterns)
                {
                    _Patterns = value;
                    Modified = true;
                }
            }
        }

        // Map external conjugator designator IDs to external labels and describe inflection patterns.
        public List<InflectorFamily> InflectorFamilies
        {
            get
            {
                return _InflectorFamilies;
            }
            set
            {
                if (value != _InflectorFamilies)
                {
                    _InflectorFamilies = value;
                    Modified = true;
                }
            }
        }

        public int InflectorFamilyCount()
        {
            if (_InflectorFamilies == null)
                return 0;

            return _InflectorFamilies.Count();
        }

        public InflectorFamily GetInflectorFamilyIndexed(int index)
        {
            if ((_InflectorFamilies == null) || (index < 0) || (index >= _InflectorFamilies.Count()))
                return null;

            InflectorFamily inflectorFamily = _InflectorFamilies[index];

            return inflectorFamily;
        }

        // Describes how to create compound inflectors.
        public List<CompoundInflector> CompoundInflectors
        {
            get
            {
                return _CompoundInflectors;
            }
            set
            {
                if (value != _CompoundInflectors)
                {
                    _CompoundInflectors = value;
                    Modified = true;
                }
            }
        }

        // Special inflectors.
        public List<InflectorFamily> SpecialInflectors
        {
            get
            {
                return _SpecialInflectors;
            }
            set
            {
                if (value != _SpecialInflectors)
                {
                    _SpecialInflectors = value;
                    Modified = true;
                }
            }
        }

        // Gives more focused definitions of helper words.
        public List<DictionaryEntry> HelperEntries
        {
            get
            {
                return _HelperEntries;
            }
            set
            {
                if (value != _HelperEntries)
                {
                    _HelperEntries = value;
                    Modified = true;
                }
            }
        }

        // Gives more focused definitions of helper words.
        public Dictionary<string, Dictionary<string, string>> HelperInflections
        {
            get
            {
                return _HelperInflections;
            }
            set
            {
                if (value != _HelperInflections)
                {
                    _HelperInflections = value;
                    Modified = true;
                }
            }
        }

        public string GetHelperInflection(
            string helper,
            string label)
        {
            Dictionary<string, string> helperInflections;
            string inflection;

            if (_HelperInflections == null)
                return null;

            if (_HelperInflections.TryGetValue(helper, out helperInflections))
            {
                if (helperInflections.TryGetValue(label, out inflection))
                    return inflection;
            }

            return null;
        }

        public string GetHelperInflectionSimple(
            string helper,
            string label)
        {
            string raw = GetHelperInflection(helper, label);
            if (String.IsNullOrEmpty(raw))
                return null;
            string[] partsMultiple = raw.Split(LanguageLookup.Comma);
            string str = partsMultiple.Last();
            string[] partsPipe = str.Split(LanguageLookup.Bar);
            string inflection = partsPipe[0];
            return inflection;
        }

        public void SetHelperInflections(
            string helper,
            Dictionary<string, string> inflections)
        {
            Dictionary<string, string> existingInflections;

            if (_HelperInflections == null)
                _HelperInflections = new Dictionary<string, Dictionary<string, string>>();

            if (_HelperInflections.TryGetValue(helper, out existingInflections))
                _HelperInflections[helper] = inflections;
            else
                _HelperInflections.Add(helper, inflections);
        }

        // Map external conjugator designator IDs to external labels.
        public Dictionary<string, string> ExternalIDToExternalLabelDictionary
        {
            get
            {
                return _ExternalIDToExternalLabelDictionary;
            }
            set
            {
                if (value != _ExternalIDToExternalLabelDictionary)
                {
                    _ExternalIDToExternalLabelDictionary = value;
                    Modified = true;
                }
            }
        }

        // Map external conjugator designator label to our designator labels.
        public Dictionary<string, string> ExternalLabelToLabelDictionary
        {
            get
            {
                return _ExternalLabelToLabelDictionary;
            }
            set
            {
                if (value != _ExternalLabelToLabelDictionary)
                {
                    _ExternalLabelToLabelDictionary = value;
                    Modified = true;
                }
            }
        }

        // Map our designator labels to the external IDs.
        public Dictionary<string, LiteralString> LabelToExternalIDDictionary
        {
            get
            {
                return _LabelToExternalIDDictionary;
            }
            set
            {
                if (value != _LabelToExternalIDDictionary)
                {
                    _LabelToExternalIDDictionary = value;
                    Modified = true;
                }
            }
        }

        // Map external conjugator designator IDs to our designator labels.
        public Dictionary<string, string> ExternalIDToLabelDictionary
        {
            get
            {
                return _ExternalIDToLabelDictionary;
            }
            set
            {
                if (value != _ExternalIDToLabelDictionary)
                {
                    _ExternalIDToLabelDictionary = value;
                    Modified = true;
                }
            }
        }

        public List<InflectorFilter> InflectorFilterList
        {
            get
            {
                return _InflectorFilterList;
            }
            set
            {
                if (value != _InflectorFilterList)
                {
                    _InflectorFilterList = value;
                    ModifiedFlag = true;
                }
            }
        }

        public void AppendInflectorFilter(InflectorFilter inflectorFilter)
        {
            if (_InflectorFilterList != null)
                _InflectorFilterList.Add(inflectorFilter);
            else
                _InflectorFilterList = new List<InflectorFilter>() { inflectorFilter };
        }

        public List<string> AutomaticRowKeys
        {
            get
            {
                return _AutomaticRowKeys;
            }
            set
            {
                if (value != _AutomaticRowKeys)
                {
                    _AutomaticRowKeys = value;
                }
            }
        }

        public List<string> AutomaticColumnKeys
        {
            get
            {
                return _AutomaticColumnKeys;
            }
            set
            {
                if (value != _AutomaticColumnKeys)
                {
                    _AutomaticColumnKeys = value;
                }
            }
        }

        public List<InflectionsLayoutGroup> MajorGroups
        {
            get
            {
                return _MajorGroups;
            }
            set
            {
                if (value != _MajorGroups)
                {
                    _MajorGroups = value;
                }
            }
        }

        public void AppendMajorGroup(InflectionsLayoutGroup majorGroup)
        {
            if (_MajorGroups == null)
                _MajorGroups = new List<InflectionsLayoutGroup>() { majorGroup };
            else
                _MajorGroups.Add(majorGroup);
        }

        public List<Designator> DesignationTranslations
        {
            get
            {
                return _DesignationTranslations;
            }
            set
            {
                if (value != _DesignationTranslations)
                {
                    _DesignationTranslations = value;
                    ModifiedFlag = true;
                }
            }
        }

        public void AppendDesignationTranslation(Designator fromDesignation, Designator toDesignation)
        {
            if (_DesignationTranslations == null)
                _DesignationTranslations = new List<Designator>() { fromDesignation, toDesignation };
            else
            {
                _DesignationTranslations.Add(fromDesignation);
                _DesignationTranslations.Add(toDesignation);
            }
            ModifiedFlag = true;
        }

        public Dictionary<string, Tuple<string, string>> CategoryStringToClassMap
        {
            get
            {
                return _CategoryStringToClassMap;
            }
            set
            {
                if (value != _CategoryStringToClassMap)
                {
                    _CategoryStringToClassMap = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            element.Add(new XAttribute("Name", KeyString));

            string targetLanguagesKey = TargetLanguagesKey;

            if (!String.IsNullOrEmpty(targetLanguagesKey))
                element.Add(new XAttribute("LanguageIDs", targetLanguagesKey));

            if (!String.IsNullOrEmpty(_Type))
                element.Add(new XAttribute("Type", _Type));

            if (_InflectorDescriptionDocumentation != null)
                element.Add(_InflectorDescriptionDocumentation);

            if (_InflectorTableDocumentation != null)
                element.Add(_InflectorTableDocumentation);

            if ((_ClassKeys != null) && (_ClassKeys.Count() != 0))
                element.Add(ObjectUtilities.GetElementFromStringList("ClassKeys", _ClassKeys));

            if (_ExternalLanguageID != null)
                element.Add(_ExternalLanguageID.GetElement("ExternalLanguageID"));

            if (!String.IsNullOrEmpty(_StemType))
                element.Add(new XElement("StemType", _StemType));

            if ((_Regulars != null) && (_Regulars.Count() != 0))
            {
                XElement regularsElement = new XElement("Regulars");

                foreach (WordDescriptor word in _Regulars)
                    regularsElement.Add(word.GetElement("Regular"));

                element.Add(regularsElement);
            }

            if ((_Irregulars != null) && (_Irregulars.Count() != 0))
            {
                XElement irregularsElement = new XElement("Irregulars");

                foreach (WordDescriptor word in _Irregulars)
                    irregularsElement.Add(word.GetElement("Irregular"));

                element.Add(irregularsElement);
            }

            element.Add(new XElement("EndingsVersion", _EndingsVersion));

            if ((_EndingsSources != null) && (_EndingsSources.Count() != 0))
            {
                string type = null;
                List<string> sources = null;

                foreach (Classifier endingsSource in _EndingsSources)
                {
                    if (type != endingsSource.KeyString)
                    {
                        if ((sources != null) && (sources.Count() != 0))
                        {
                            XElement endingsSourceElement = new XElement("EndingsSourceGroup");
                            endingsSourceElement.Add(new XAttribute("Key", type));
                            endingsSourceElement.Value = ObjectUtilities.GetNewLinedIndentedStringsFromStringList(sources, 1, 2);
                            element.Add(endingsSourceElement);
                        }

                        type = endingsSource.KeyString;
                        sources = new List<string>();
                    }

                    sources.Add(endingsSource.Text);
                }

                if ((sources != null) && (sources.Count() != 0))
                {
                    XElement endingsSourceElement = new XElement("EndingsSourceGroup");
                    endingsSourceElement.Add(new XAttribute("Key", type));
                    endingsSourceElement.Value = ObjectUtilities.GetNewLinedIndentedStringsFromStringList(sources, 1, 2);
                    element.Add(endingsSourceElement);
                }
            }

            if ((_SubjectPronouns != null) && (_SubjectPronouns.Count() != 0))
            {
                XElement pronounsElement = new XElement("SubjectPronouns");

                foreach (TokenDescriptor pronoun in _SubjectPronouns)
                    pronounsElement.Add(pronoun.GetElement("SubjectPronoun"));

                element.Add(pronounsElement);
            }

            if ((_ReflexivePronouns != null) && (_ReflexivePronouns.Count() != 0))
            {
                XElement pronounsElement = new XElement("ReflexivePronouns");

                foreach (TokenDescriptor pronoun in _ReflexivePronouns)
                    pronounsElement.Add(pronoun.GetElement("ReflexivePronoun"));

                element.Add(pronounsElement);
            }

            if ((_DirectPronouns != null) && (_DirectPronouns.Count() != 0))
            {
                XElement pronounsElement = new XElement("DirectPronouns");

                foreach (TokenDescriptor pronoun in _DirectPronouns)
                    pronounsElement.Add(pronoun.GetElement("DirectPronoun"));

                element.Add(pronounsElement);
            }

            if ((_IndirectPronouns != null) && (_IndirectPronouns.Count() != 0))
            {
                XElement pronounsElement = new XElement("IndirectPronouns");

                foreach (TokenDescriptor pronoun in _IndirectPronouns)
                    pronounsElement.Add(pronoun.GetElement("IndirectPronoun"));

                element.Add(pronounsElement);
            }

            if ((_GenderSuffixes != null) && (_GenderSuffixes.Count() != 0))
            {
                XElement genderSuffixesElement = new XElement("GenderSuffixes");

                foreach (TokenDescriptor genderSuffix in _GenderSuffixes)
                    genderSuffixesElement.Add(genderSuffix.GetElement("GenderSuffix"));

                element.Add(genderSuffixesElement);
            }

            if ((_ExternalPronounExpansions != null) && (_ExternalPronounExpansions.Count() != 0))
            {
                XElement externalPronounExpnansionsElement = new XElement("ExternalPronounExpansions");

                foreach (KeyValuePair<string, List<string>> kvp in _ExternalPronounExpansions)
                {
                    XElement entry = new XElement("ExternalPronounExpansion", ObjectUtilities.GetStringFromStringList(kvp.Value));
                    entry.Add(new XAttribute("Key", kvp.Key));
                    externalPronounExpnansionsElement.Add(entry);
                }

                element.Add(externalPronounExpnansionsElement);
            }

            if ((_Patterns != null) && (_Patterns.Count() != 0))
            {
                XElement patternsElement = new XElement("Patterns");

                foreach (PhrasePattern pattern in _Patterns)
                    patternsElement.Add(pattern.GetElement("Pattern"));

                element.Add(patternsElement);
            }

            if ((_InflectorFamilies != null) && (_InflectorFamilies.Count() != 0))
            {
                XElement inflectorFamiliesElement = new XElement("InflectorFamilies");

                foreach (InflectorFamily inflectorFamily in _InflectorFamilies)
                    inflectorFamiliesElement.Add(inflectorFamily.GetElement("InflectorFamily"));

                element.Add(inflectorFamiliesElement);
            }

            if ((_CompoundInflectors != null) && (_CompoundInflectors.Count() != 0))
            {
                foreach (CompoundInflector compoundInflector in _CompoundInflectors)
                    element.Add(compoundInflector.GetElement("CompoundInflector"));
            }

            if ((_SpecialInflectors != null) && (_SpecialInflectors.Count() != 0))
            {
                foreach (InflectorFamily specialInflector in _SpecialInflectors)
                    element.Add(specialInflector.GetElement("SpecialInflector"));
            }

            if ((_HelperEntries != null) && (_HelperEntries.Count() != 0))
            {
                foreach (DictionaryEntry helperEntry in _HelperEntries)
                    element.Add(helperEntry.GetElement("HelperEntry"));
            }

            if (_HelperInflections != null)
            {
                foreach (KeyValuePair<string, Dictionary<string, string>> kvpInflections in _HelperInflections)
                {
                    XElement inflectionsElement = ObjectUtilities.GetElementFromDictionaryGroupedSimple<string, string>(
                        "HelperInflections", kvpInflections.Value);
                    inflectionsElement.Add(new XAttribute("Key", kvpInflections.Key));
                    element.Add(inflectionsElement);
                }
            }

            if (_ExternalIDToExternalLabelDictionary != null)
                element.Add(
                    ObjectUtilities.GetElementFromDictionaryGroupedSimple(
                        "ExternalIDToExternalLabelDictionary",
                        _ExternalIDToExternalLabelDictionary));

            if (_ExternalLabelToLabelDictionary != null)
                element.Add(
                    ObjectUtilities.GetElementFromDictionaryGroupedSimple(
                        "ExternalLabelToLabelDictionary",
                        _ExternalLabelToLabelDictionary));

            if (_LabelToExternalIDDictionary != null)
                element.Add(
                    ObjectUtilities.GetElementFromDictionaryGroupedSimple(
                        "LabelToExternalIDDictionary",
                        _LabelToExternalIDDictionary));

            if (_ExternalIDToLabelDictionary != null)
                element.Add(
                    ObjectUtilities.GetElementFromDictionaryGroupedSimple(
                        "ExternalIDToLabelDictionary",
                        _ExternalIDToLabelDictionary));

            if (_InflectorFilterList != null)
            {
                foreach (InflectorFilter inflectorFilter in _InflectorFilterList)
                {
                    XElement stemEntryElement = inflectorFilter.GetElement("InflectorFilter");
                    element.Add(stemEntryElement);
                }
            }

            if (_AutomaticRowKeys != null)
                element.Add(new XElement("AutomaticRowKeys", ObjectUtilities.GetStringFromStringList(_AutomaticRowKeys)));

            if (_AutomaticColumnKeys != null)
                element.Add(new XElement("AutomaticColumnKeys", ObjectUtilities.GetStringFromStringList(_AutomaticColumnKeys)));

            if (_MajorGroups != null)
            {
                foreach (InflectionsLayoutGroup majorGroup in _MajorGroups)
                {
                    XElement majorGroupElement = majorGroup.GetElement("Major");
                    element.Add(majorGroupElement);
                }
            }

            if (_DesignationTranslations != null)
            {
                int c = _DesignationTranslations.Count();
                int i;

                for (i = 0; i < c; i += 2)
                {
                    Designator fromDesignation = _DesignationTranslations[i];
                    Designator toDesignation = _DesignationTranslations[i + 1];
                    XElement designationTranslationsElement = new XElement("DesignationTranslations");
                    XElement fromElement = fromDesignation.GetElement("From");
                    XElement toElement = toDesignation.GetElement("To");
                    designationTranslationsElement.Add(fromElement);
                    designationTranslationsElement.Add(toElement);
                    element.Add(designationTranslationsElement);
                }
            }

            if (_CategoryStringToClassMap != null)
            {
                XElement mapElement = new XElement("CategoryStringToClassMap");

                foreach (KeyValuePair<string, Tuple<string, string>> kvp in _CategoryStringToClassMap)
                {
                    XElement itemElement = new XElement("ClassItem");
                    itemElement.Add(new XAttribute("Input", kvp.Key));
                    itemElement.Add(new XAttribute("Class", kvp.Value.Item1));
                    itemElement.Add(new XAttribute("SubClass", kvp.Value.Item2));
                    mapElement.Add(itemElement);
                }

                element.Add(mapElement);
            }

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Name":
                    Key = attributeValue;
                    break;
                case "LanguageIDs":
                    TargetLanguagesKey = attributeValue;
                    break;
                case "Type":
                    Type = attributeValue;
                    break;
                default:
                    return OnUnknownAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "InflectorDescriptionDocumentation":
                    _InflectorDescriptionDocumentation = childElement;
                    break;
                case "InflectorTableDocumentation":
                    _InflectorTableDocumentation = childElement;
                    break;
                case "ClassKeys":
                    _ClassKeys = ObjectUtilities.GetStringListFromElement(childElement);
                    break;
                case "ExternalLanguageID":
                    _ExternalLanguageID =ObjectUtilities.GetLanguageIDFromElement(childElement);
                    break;
                case "StemType":
                    _StemType = childElement.Value.Trim();
                    break;
                case "Regulars":
                    {
                        _Regulars = new List<WordDescriptor>();
                        foreach (XElement wordElement in childElement.Elements())
                        {
                            WordDescriptor word = new WordDescriptor(wordElement);
                            _Regulars.Add(word);
                        }
                    }
                    break;
                case "Irregulars":
                    {
                        _Irregulars = new List<WordDescriptor>();
                        foreach (XElement wordElement in childElement.Elements())
                        {
                            WordDescriptor word = new WordDescriptor(wordElement);
                            _Irregulars.Add(word);
                        }
                    }
                    break;
                case "EndingsVersion":
                    _EndingsVersion = ObjectUtilities.GetIntegerFromString(childElement.Value, 0);
                    break;
                case "EndingsSourceGroup":
                    {
                        XAttribute keyAttribute = childElement.Attribute("Key");
                        XAttribute versionAttribute = childElement.Attribute("Version");
                        if (keyAttribute != null)
                        {
                            string key = keyAttribute.Value;
                            string stringValue = childElement.Value;
                            List<string> stringList = ObjectUtilities.GetStringListFromString(stringValue);
                            foreach (string value in stringList)
                            {
                                Classifier endingsSource = new Classifier(key, value);
                                AppendEndingsSource(endingsSource);
                            }
                        }
                    }
                    break;
                case "EndingsSource":
                    {
                        Classifier endingsSource = new Classifier(childElement);
                        AppendEndingsSource(endingsSource);
                    }
                    break;
                case "SubjectPronouns":
                    {
                        _SubjectPronouns = new List<TokenDescriptor>();
                        foreach (XElement pronounElement in childElement.Elements())
                        {
                            TokenDescriptor pronoun = new TokenDescriptor(pronounElement);
                            _SubjectPronouns.Add(pronoun);
                        }
                    }
                    break;
                case "ReflexivePronouns":
                    {
                        _ReflexivePronouns = new List<TokenDescriptor>();
                        foreach (XElement pronounElement in childElement.Elements())
                        {
                            TokenDescriptor pronoun = new TokenDescriptor(pronounElement);
                            _ReflexivePronouns.Add(pronoun);
                        }
                    }
                    break;
                case "DirectPronouns":
                    {
                        _DirectPronouns = new List<TokenDescriptor>();
                        foreach (XElement pronounElement in childElement.Elements())
                        {
                            TokenDescriptor pronoun = new TokenDescriptor(pronounElement);
                            _DirectPronouns.Add(pronoun);
                        }
                    }
                    break;
                case "IndirectPronouns":
                    {
                        _IndirectPronouns = new List<TokenDescriptor>();
                        foreach (XElement pronounElement in childElement.Elements())
                        {
                            TokenDescriptor pronoun = new TokenDescriptor(pronounElement);
                            _IndirectPronouns.Add(pronoun);
                        }
                    }
                    break;
                case "GenderSuffixes":
                    {
                        _GenderSuffixes = new List<TokenDescriptor>();
                        foreach (XElement genderSuffixElement in childElement.Elements())
                        {
                            TokenDescriptor genderSuffix = new TokenDescriptor(genderSuffixElement);
                            _GenderSuffixes.Add(genderSuffix);
                        }
                    }
                    break;
                case "ExternalPronounExpansions":
                    {
                        ExternalPronounExpansions = new Dictionary<string, List<string>>();
                        foreach (XElement externalPronounExpansion in childElement.Elements())
                        {
                            string pronoun = externalPronounExpansion.Attribute("Key").Value.Trim();
                            List<string> pronounKeys = ObjectUtilities.GetStringListFromString(externalPronounExpansion.Value);
                            ExternalPronounExpansions.Add(pronoun, pronounKeys);
                        }
                    }
                    break;
                case "Patterns":
                    {
                        _Patterns = new List<PhrasePattern>();
                        foreach (XElement patternElement in childElement.Elements())
                        {
                            PhrasePattern pattern = new PhrasePattern(patternElement);
                            _Patterns.Add(pattern);
                        }
                    }
                    break;
                case "InflectorFamilies":
                    {
                        _InflectorFamilies = new List<InflectorFamily>();
                        foreach (XElement inflectorFamilyElement in childElement.Elements())
                        {
                            InflectorFamily inflectorFamily = new InflectorFamily(inflectorFamilyElement);
                            _InflectorFamilies.Add(inflectorFamily);
                        }
                    }
                    break;
                case "CompoundInflector":
                    {
                        if (_CompoundInflectors == null)
                            _CompoundInflectors = new List<CompoundInflector>();
                        CompoundInflector compoundInflector = new CompoundInflector(childElement);
                        _CompoundInflectors.Add(compoundInflector);
                    }
                    break;
                case "SpecialInflector":
                    {
                        if (_SpecialInflectors == null)
                            _SpecialInflectors = new List<InflectorFamily>();
                        InflectorFamily specialInflector = new InflectorFamily(childElement);
                        _SpecialInflectors.Add(specialInflector);
                    }
                    break;
                case "HelperEntry":
                    {
                        if (_HelperEntries == null)
                            _HelperEntries = new List<DictionaryEntry>();
                        DictionaryEntry helperEntry = new DictionaryEntry(childElement);
                        _HelperEntries.Add(helperEntry);
                    }
                    break;
                case "HelperInflections":
                    {
                        if (HelperInflections == null)
                            HelperInflections = new Dictionary<string, Dictionary<string, string>>();
                        string key = childElement.Attribute("Key").Value.Trim();
                        Dictionary<string, string> inflections = ObjectUtilities.GetDictionaryFromElementGroupedSimple<string, string>(
                            childElement, "string", "string");
                        HelperInflections.Add(key, inflections);
                    }
                    break;
                case "ExternalIDToExternalLabelDictionary":
                    _ExternalIDToExternalLabelDictionary =
                        ObjectUtilities.GetDictionaryFromElementGroupedSimple<string, string>(childElement, "string", "string");
                    break;
                case "ExternalLabelToLabelDictionary":
                    _ExternalLabelToLabelDictionary =
                        ObjectUtilities.GetDictionaryFromElementGroupedSimple<string, string>(childElement, "string", "string");
                    break;
                case "LabelToExternalIDDictionary":
                    _LabelToExternalIDDictionary =
                        ObjectUtilities.GetDictionaryFromElementGroupedSimple<string, LiteralString>(childElement, "string", "LiteralString");
                    break;
                case "ExternalIDToLabelDictionary":
                    _ExternalIDToLabelDictionary =
                        ObjectUtilities.GetDictionaryFromElementGroupedSimple<string, string>(childElement, "string", "string");
                    break;
                case "InflectorFilter":
                    {
                        InflectorFilter inflectorFilter = new InflectorFilter(childElement);
                        AppendInflectorFilter(inflectorFilter);
                    }
                    break;
                case "AutomaticRowKeys":
                    _AutomaticRowKeys = ObjectUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "AutomaticColumnKeys":
                    _AutomaticColumnKeys = ObjectUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "Major":
                    {
                        InflectionsLayoutGroup majorGroup = new InflectionsLayoutGroup(childElement);
                        AppendMajorGroup(majorGroup);
                    }
                    break;
                case "DesignationTranslations":
                    {
                        XElement fromElement = childElement.Element("From");
                        XElement toElement = childElement.Element("To");
                        Designator fromDesignation = new Designator(fromElement);
                        Designator toDesignation = new Designator(toElement);
                        AppendDesignationTranslation(fromDesignation, toDesignation);
                    }
                    break;
                case "CategoryStringToClassMap":
                    {
                        Dictionary<string, Tuple<string, string>> map = new Dictionary<string, Tuple<string, string>>();
                        foreach (XElement itemElement in childElement.Elements())
                        {
                            XAttribute inputAttribute = itemElement.Attribute("Input");
                            XAttribute classAttribute = itemElement.Attribute("Class");
                            XAttribute subClassAttribute = itemElement.Attribute("SubClass");
                            if ((inputAttribute != null) && (classAttribute != null) && (subClassAttribute != null))
                            {
                                string input = inputAttribute.Value;
                                string className = classAttribute.Value;
                                string subClassName = subClassAttribute.Value;
                                map.Add(input, new Tuple<string, string>(className, subClassName));
                            }
                            _CategoryStringToClassMap = map;
                        }
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
