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
    public class InflectorTable : BaseObjectLanguages
    {
        // Store documentation to be copied to generated inflector table.
        protected XElement _InflectorTableDocumentation;

        // Type of inflectable (i.e. "Verb", "Noun", "Adjective").
        protected string _Type;

        // For old endings graph (not used anymore).
        protected List<Classifier> _EndingsSources;
        protected int _EndingsVersion;

        // Designator tables.
        protected List<DesignatorTable> _DesignatorTables;
        protected List<DesignatorTable> _DesignatorsTemplateTables;

        // Word inflection class keys. (Language-specific.  i.e. "ar", "er", "ir")
        protected List<string> _ClassKeys;

        // Stem type (copied from InflectorDescription).
        protected string _StemType;

        // Subject pronoun descriptors (copied from InflectorDescription).
        protected List<TokenDescriptor> _SubjectPronouns;

        // Reflexive pronoun descriptors (copied from InflectorDescription).
        protected List<TokenDescriptor> _ReflexivePronouns;

        // Direct object pronoun descriptors (copied from InflectorDescription).
        protected List<TokenDescriptor> _DirectPronouns;

        // Indirect object pronoun descriptors (copied from InflectorDescription).
        protected List<TokenDescriptor> _IndirectPronouns;

        // Gender suffix descriptors (copied from InflectorDescription).
        protected List<TokenDescriptor> _GenderSuffixes;

        // Stem list (not used anymore).
        protected List<Inflector> _StemList;
        protected Dictionary<string, Inflector> _StemDictionary;                    // Not saved.

        // Inflector families (expanded from InflectorDescription object).
        protected List<InflectorFamily> _InflectorFamilyList;

        protected List<Inflector> _SimpleInflectorList;
        protected List<Inflector> _CompoundInflectorList;                           // Not saved. Generated from CompoundInflectors.
        protected List<Inflector> _ChildInflectorList;                              // Not saved. Generated from InflectorGroups.
        protected List<Inflector> _InflectorList;                                   // Not saved. Collects all inflectors.
        protected Dictionary<string, Inflector> _InflectorDictionary;               // Not saved.
        protected Dictionary<string, Inflector> _BestInflectorDictionary;           // Not saved.
        protected Dictionary<string, List<Inflector>> _BestInflectorListDictionary; // Not saved.
        protected List<InflectorTrigger> _InflectorTriggerList;
        protected List<CompoundInflector> _CompoundInflectors;

        // List of special inflectors.
        protected List<InflectorFamily> _SpecialInflectors;

        protected List<InflectorGroup> _InflectorGroups;
        protected List<IrregularTable> _IrregularTables;
        protected List<SemiRegular> _Irregulars;   // Not saved. Generated from IrregularTables.
        protected Dictionary<string, SemiRegular> _IrregularDictionary; // Not saved.
        protected List<SemiRegular> _SemiRegulars;

        // Automatic display row keys.
        protected List<string> _AutomaticRowKeys;

        // Automatic display column keys.
        protected List<string> _AutomaticColumnKeys;

        // Display major groups.
        protected List<InflectionsLayoutGroup> _MajorGroups;

        // Designation translations.
        protected List<Designator> _DesignationTranslations;

        // Map category strings to classes.
        protected Dictionary<string, Tuple<string, string>> _CategoryStringToClassMap;

        // Inflector filter list.
        protected List<InflectorFilter> _InflectorFilterList;

        // Inflector filter dictionary.
        protected Dictionary<string, InflectorFilter> _InflectorFilterDictionary;   // Not saved.

        // Helper entries (dictionary entries for helpers reduced to essentials).
        protected List<DictionaryEntry> _HelperEntries;

        public static List<string> Types = new List<string>()
        {
            "Noun",             // Abstract or concrete entity.
            "ProperNoun",       // Proper noun.
            "Pronoun",          // Substitute for a noun or noun phrase.
            "Determiner",       // Determiner (this, that, those, each, every, etc.).
            "Adjective",        // Qualifier of a noun.
            "Verb",             // Action or state of being.
            "Adverb",           // Qualifier of an adjective, verb, or other adverb.
            "Preposition",      // Establisher of relation and syntactic context.
            "Conjunction",      // Syntactic connector.
            "Interjection",     // Emotional greeting (or "exclamation").
            "Particle",         // Particle.
            "Article",          // Direct or indirect article.
            "MeasureWord",      // Measure word.
            "Number"            // Cardinal number.
        };

        public InflectorTable(
            string name,
            List<LanguageID> languageIDs,
            string type,
            List<DesignatorTable> designatorTables,
            List<DesignatorTable> designatorsTemplateTables,
            List<Inflector> simpleInflectors,
            List<CompoundInflector> compoundInflectors,
            List<InflectorGroup> inflectorGroups,
            List<IrregularTable> irregularTables,
            Dictionary<string, Tuple<string, string>> categoryStringToClassMap) :
                base(name, languageIDs, null, null)
        {
            ClearInflectorTable();
            _Type = type;
            _DesignatorTables = designatorTables;
            _DesignatorsTemplateTables = designatorsTemplateTables;
            _SimpleInflectorList = simpleInflectors;
            _CompoundInflectors = compoundInflectors;
            _InflectorGroups = inflectorGroups;
            _IrregularTables = irregularTables;
            _CategoryStringToClassMap = categoryStringToClassMap;
            LoadInflectorDictionary(simpleInflectors);
        }

        public InflectorTable(
            string name,
            List<LanguageID> languageIDs,
            InflectorDescription inflectorDescription) :
                base(name, languageIDs, null, null)
        {
            ClearInflectorTable();
            _Type = inflectorDescription.Type;
        }

        public InflectorTable(XElement element)
        {
            ClearInflectorTable();
            OnElement(element);
        }

        public InflectorTable(InflectorTable other) : base(other)
        {
            CopyInflectorTable(other);
        }

        public InflectorTable()
        {
            ClearInflectorTable();
        }

        public void ClearInflectorTable()
        {
            _InflectorTableDocumentation = null;
            _EndingsSources = new List<Classifier>();
            _EndingsVersion = 0;
            _Type = String.Empty;
            _DesignatorTables = new List<DesignatorTable>();
            _DesignatorsTemplateTables = new List<DesignatorTable>();
            _ClassKeys = null;
            _StemType = null;
            _SubjectPronouns = null;
            _ReflexivePronouns = null;
            _DirectPronouns = null;
            _IndirectPronouns = null;
            _GenderSuffixes = null;
            _StemList = new List<Inflector>();
            _StemDictionary = new Dictionary<string, Inflector>();
            _InflectorFamilyList = new List<InflectorFamily>();
            _SimpleInflectorList = new List<Inflector>();
            _CompoundInflectorList = new List<Inflector>();
            _ChildInflectorList = new List<Inflector>();
            _IrregularTables = new List<IrregularTable>();
            _Irregulars = new List<SemiRegular>();
            _IrregularDictionary = new Dictionary<string, SemiRegular>();
            _InflectorList = new List<Inflector>();
            _InflectorDictionary = new Dictionary<string, Inflector>();
            _BestInflectorDictionary = new Dictionary<string, Inflector>();
            _BestInflectorListDictionary = new Dictionary<string, List<Inflector>>();
            _InflectorTriggerList = new List<InflectorTrigger>();
            _CompoundInflectors = null;
            _SpecialInflectors = null;
            _InflectorFilterList = new List<InflectorFilter>();
            _InflectorFilterDictionary = new Dictionary<string, InflectorFilter>();
            _InflectorGroups = null;
            _SemiRegulars = null;
            _AutomaticRowKeys = null;
            _AutomaticColumnKeys = null;
            _MajorGroups = null;
            _DesignationTranslations = null;
            _CategoryStringToClassMap = null;
            _HelperEntries = null;
        }

        public void CopyInflectorTable(InflectorTable other)
        {
            _InflectorTableDocumentation = other.InflectorTableDocumentation;
            _EndingsSources = other.EndingsSources;
            _EndingsVersion = other.EndingsVersion;
            _Type = other.Type;
            _DesignatorTables = other.DesignatorTables;
            _DesignatorsTemplateTables = other.DesignatorsTemplateTables;
            _ClassKeys = other.ClassKeys;
            _StemType = other.StemType;
            _SubjectPronouns = other.SubjectPronouns;
            _ReflexivePronouns = other.ReflexivePronouns;
            _DirectPronouns = other.DirectPronouns;
            _IndirectPronouns = other.IndirectPronouns;
            _GenderSuffixes = other.GenderSuffixes;
            _StemList = other.Stems;
            _StemDictionary = new Dictionary<string, Inflector>();
            _InflectorFamilyList = other.InflectorFamilyList;
            _SimpleInflectorList = other.SimpleInflectorList;
            _CompoundInflectorList = other.CompoundInflectorList;
            _ChildInflectorList = other.ChildInflectorList;
            _IrregularTables = other.IrregularTables;
            _Irregulars = new List<SemiRegular>();
            _IrregularDictionary = new Dictionary<string, SemiRegular>();
            _InflectorList = other.Inflectors;
            _InflectorDictionary = new Dictionary<string, Inflector>();
            _BestInflectorDictionary = new Dictionary<string, Inflector>();
            _BestInflectorListDictionary = new Dictionary<string, List<Inflector>>();
            _InflectorTriggerList = new List<InflectorTrigger>();
            _CompoundInflectors = other.CompoundInflectors;
            _SpecialInflectors = other.SpecialInflectors;
            _InflectorFilterList = new List<InflectorFilter>();
            _InflectorFilterDictionary = new Dictionary<string, InflectorFilter>();
            _InflectorGroups = other.InflectorGroups;
            _SemiRegulars = null;
            _AutomaticRowKeys = (other.AutomaticRowKeys != null ? new List<string>(other.AutomaticRowKeys) : null);
            _AutomaticColumnKeys = (other.AutomaticColumnKeys != null ? new List<string>(other.AutomaticColumnKeys) : null);
            _MajorGroups = (other.MajorGroups != null ? new List<InflectionsLayoutGroup>(other.MajorGroups) : null);
            _DesignationTranslations = other.CloneDesignationTranslations();
            _CategoryStringToClassMap = other.CloneCategoryStringToClassMap();
            _HelperEntries = other.HelperEntries;
            LoadInflectorDictionary(_InflectorList);
            LoadInflectorFilterDictionary(_InflectorFilterList);
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
                    ModifiedFlag = true;
                }
            }
        }

        public LexicalCategory Category
        {
            get
            {
                LexicalCategory category;

                switch (_Type)
                {
                    case "Noun":
                        category = LexicalCategory.Noun;
                        break;
                    case "ProperNoun":
                        category = LexicalCategory.ProperNoun;
                        break;
                    case "Pronoun":
                        category = LexicalCategory.Pronoun;
                        break;
                    case "Determiner":
                        category = LexicalCategory.Determiner;
                        break;
                    case "Adjective":
                        category = LexicalCategory.Adjective;
                        break;
                    case "Verb":
                        category = LexicalCategory.Verb;
                        break;
                    case "Adverb":
                        category = LexicalCategory.Adverb;
                        break;
                    case "Preposition":
                        category = LexicalCategory.Preposition;
                        break;
                    case "Conjunction":
                        category = LexicalCategory.Conjunction;
                        break;
                    case "Interjection":
                        category = LexicalCategory.Interjection;
                        break;
                    case "Particle":
                        category = LexicalCategory.Particle;
                        break;
                    case "Article":
                        category = LexicalCategory.Article;
                        break;
                    case "MeasureWord":
                        category = LexicalCategory.MeasureWord;
                        break;
                    case "Number":
                        category = LexicalCategory.Number;
                        break;
                    default:
                        category = LexicalCategory.Unknown;
                        break;
                }

                return category;
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

        public TokenDescriptor FindIteratorTokenFuzzy(string iterate, Designator designator)
        {
            List<TokenDescriptor> iterators = GetIterateTokens(iterate);
            TokenDescriptor bestToken = null;
            int bestWeight = 0;

            foreach (TokenDescriptor token in iterators)
            {
                int weight = token.GetMatchWeight(designator);

                if (weight > bestWeight)
                {
                    bestToken = token;
                    bestWeight = weight;
                }
            }

            return bestToken;
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

        public List<DesignatorTable> DesignatorTables
        {
            get
            {
                return _DesignatorTables;
            }
            set
            {
                if (value != _DesignatorTables)
                {
                    _DesignatorTables = value;
                    ModifiedFlag = true;
                }
            }
        }

        public DesignatorTable GetDesignatorTable(string scope)
        {
            if (scope == "Both")
                scope = "All";

            if (_DesignatorTables == null)
                return null;

            DesignatorTable designatorTable = _DesignatorTables.FirstOrDefault(
                x => x.Scope == scope);

            return designatorTable;
        }

        public List<Designator> GetDesignators(string scope)
        {
            if (scope == "Both")
                scope = "All";

            if (_DesignatorTables == null)
                return null;

            DesignatorTable designatorTable = _DesignatorTables.FirstOrDefault(
                x => x.Scope == scope);
            List<Designator> designators = null;

            if (designatorTable != null)
                designators = designatorTable.Designators;

            return designators;
        }

        public Designator GetDesignator(string scope, string label)
        {
            if (scope == "Both")
                scope = "All";

            if (_DesignatorTables == null)
                return null;

            DesignatorTable designatorTable = _DesignatorTables.FirstOrDefault(
                x => x.Scope == scope);
            Designator designator = null;

            if (designatorTable != null)
                designator = designatorTable.GetDesignator(label);

            return designator;
        }

        public void AppendDesignator(Designator designator, string scope)
        {
            if (scope == "Both")
            {
                AppendDesignator(designator, "All");
                AppendDesignator(designator, "Default");
            }
            else
            {
                DesignatorTable designatorTable;

                if (_DesignatorTables == null)
                {
                    _DesignatorTables = new List<DesignatorTable>();
                    designatorTable = null;
                }
                else
                    designatorTable = DesignatorTables.FirstOrDefault(
                        x => x.Scope == scope);

                if (designatorTable == null)
                {
                    designatorTable = new DesignatorTable(
                        KeyString + scope,
                        scope,
                        null);
                    DesignatorTables.Add(designatorTable);
                }

                designatorTable.Add(designator);
            }
        }

        public void AppendDesignators(List<Designator> designators, string scope)
        {
            if (designators == null)
                return;

            if (scope == "Both")
            {
                AppendDesignators(designators, "All");
                AppendDesignators(designators, "Default");
            }
            else
            {
                foreach (Designator designator in designators)
                    AppendDesignator(designator, scope);
            }
        }

        public List<DesignatorTable> DesignatorsTemplateTables
        {
            get
            {
                return _DesignatorsTemplateTables;
            }
            set
            {
                if (value != _DesignatorsTemplateTables)
                {
                    _DesignatorsTemplateTables = value;
                    ModifiedFlag = true;
                }
            }
        }

        public DesignatorTable GetDesignatorsTemplateTable(string name)
        {
            if (_DesignatorsTemplateTables == null)
                return null;

            DesignatorTable designatorsTemplateTable = _DesignatorsTemplateTables.FirstOrDefault(
                x => x.Name == name);

            return designatorsTemplateTable;
        }

        public DesignatorTable GetDesignatorsTemplateTableIndexed(int index)
        {
            if (_DesignatorsTemplateTables == null)
                return null;

            if ((index < 0) || (index >= _DesignatorsTemplateTables.Count()))
                return null;

            DesignatorTable designatorsTemplateTable = _DesignatorsTemplateTables[index];

            return designatorsTemplateTable;
        }

        public void AppendDesignatorsTemplateTable(DesignatorTable designatorsTemplateTable)
        {
            if (_DesignatorsTemplateTables == null)
                _DesignatorsTemplateTables = new List<DesignatorTable>() { designatorsTemplateTable };
            else
                _DesignatorsTemplateTables.Add(designatorsTemplateTable);

            ModifiedFlag = true;
        }

        public int DesignatorsTemplateTableCount()
        {
            if (_DesignatorsTemplateTables == null)
                return 0;

            return _DesignatorsTemplateTables.Count();
        }

        public List<Inflector> Stems
        {
            get
            {
                return _StemList;
            }
            set
            {
                if (value != _StemList)
                {
                    _StemList = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int StemCount()
        {
            if (_StemList == null)
                return 0;

            return _StemList.Count();
        }

        public bool HasStem(string name)
        {
            Inflector stem = null;

            if (String.IsNullOrEmpty(name) || (_StemDictionary == null))
                return false;

            if (!_StemDictionary.TryGetValue(name, out stem))
                return false;

            return true;
        }

        public Inflector GetStem(string name)
        {
            Inflector stem = null;

            if (String.IsNullOrEmpty(name) || (_StemDictionary == null))
                return stem;

            _StemDictionary.TryGetValue(name, out stem);

            return stem;
        }

        public Inflector GetStemIndexed(int index)
        {
            if ((_StemList == null) || (index < 0) || (index >= _StemList.Count()))
                return null;

            Inflector stem = _StemList[index];

            return stem;
        }

        public void AppendStem(Inflector stem)
        {
            stem.SetDefaultCategory(Sense.GetLexicalCategoryFromString(Type));

            if (_StemList != null)
                _StemList.Add(stem);
            else
                _StemList = new List<Inflector>() { stem };

            AddStemToDictionary(stem);
        }

        public bool DeleteStem(Inflector stem)
        {
            if ((stem == null) || (_StemList == null))
                return false;

            RemoveStemFromDictionary(stem);
            bool returnValue = _StemList.Remove(stem);

            return returnValue;
        }

        public bool DeleteStemIndexed(int index)
        {
            if ((_StemList == null) || (index < 0) || (index >= _StemList.Count()))
                return false;

            RemoveStemFromDictionary(_StemList[index]);
            _StemList.RemoveAt(index);

            return true;
        }

        protected void LoadStemDictionary(List<Inflector> stems)
        {
            foreach (Inflector stem in stems)
                AddStemToDictionary(stem);
        }

        protected void AddStemToDictionary(Inflector stem)
        {
            if (stem != null)
            {
                try
                {
                    _StemDictionary.Add(stem.KeyString, stem);
                }
                catch (Exception)
                {
                    ApplicationData.Global.PutConsoleMessage("InflectorTable.AddStemToDictionary duplicate entry: " + stem.KeyString);
                }
            }
        }

        protected void RemoveStemFromDictionary(Inflector stem)
        {
            if (stem != null)
                _StemDictionary.Remove(stem.KeyString);
        }

        public List<InflectorFamily> InflectorFamilyList
        {
            get
            {
                return _InflectorFamilyList;
            }
            set
            {
                if (value != _InflectorFamilyList)
                {
                    _InflectorFamilyList = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int InflectorFamilyListCount()
        {
            if (_InflectorFamilyList == null)
                return 0;

            return _InflectorFamilyList.Count();
        }

        public bool HasInflectorFamilyList(string label)
        {
            InflectorFamily inflectorFamily = _InflectorFamilyList.FirstOrDefault(x => x.Label == label);
            if (inflectorFamily != null)
                return true;
            return false;
        }

        public InflectorFamily GetInflectorFamilyList(string label)
        {
            InflectorFamily inflectorFamily = _InflectorFamilyList.FirstOrDefault(x => x.Label == label);
            return inflectorFamily;
        }

        public InflectorFamily GetInflectorFamilyListIndexed(int index)
        {
            if ((_InflectorFamilyList == null) || (index < 0) || (index >= _InflectorFamilyList.Count()))
                return null;

            InflectorFamily inflectorFamily = _InflectorFamilyList[index];

            return inflectorFamily;
        }

        public InflectorFamily GetInflectorFamilyFromInflectorLabel(string label)
        {
            if (_InflectorFamilyList == null)
                return null;

            foreach (InflectorFamily inflectorFamily in _InflectorFamilyList)
            {
                if (inflectorFamily.HasInflector(label))
                    return inflectorFamily;
            }

            return null;
        }

        public void AppendInflectorFamilyList(InflectorFamily inflectorFamily)
        {
            if (inflectorFamily.Inflectors != null)
            {
                foreach (Inflector inflector in inflectorFamily.Inflectors)
                    AppendInflector(inflector);
            }

            if (_InflectorFamilyList != null)
                _InflectorFamilyList.Add(inflectorFamily);
            else
                _InflectorFamilyList = new List<InflectorFamily>() { inflectorFamily };
        }

        public bool InsertInflectorFamilyList(int index, InflectorFamily inflectorFamily)
        {
            if (inflectorFamily.Inflectors != null)
            {
                foreach (Inflector inflector in inflectorFamily.Inflectors)
                    AppendInflector(inflector);
            }

            if (_InflectorFamilyList != null)
            {
                if ((index >= 0) && (index <= _InflectorFamilyList.Count()))
                    _InflectorFamilyList.Insert(index, inflectorFamily);
                else
                    return false;
            }
            else if (index == 0)
                _InflectorFamilyList = new List<InflectorFamily>() { inflectorFamily };
            else
                return false;

            return true;
        }

        public bool DeleteInflectorFamilyList(InflectorFamily inflectorFamily)
        {
            if ((inflectorFamily == null) || (_InflectorFamilyList == null))
                return false;

            bool returnValue = _InflectorFamilyList.Remove(inflectorFamily);

            if (returnValue)
            {
                if (inflectorFamily.Inflectors != null)
                {
                    foreach (Inflector inflector in inflectorFamily.Inflectors)
                        DeleteInflector(inflector);
                }
            }

            return returnValue;
        }

        public bool DeleteInflectorFamilyListIndexed(int index)
        {
            if ((_InflectorFamilyList == null) || (index < 0) || (index >= _InflectorFamilyList.Count()))
                return false;

            InflectorFamily inflectorFamily = _InflectorFamilyList[index];

            _InflectorFamilyList.RemoveAt(index);

            if (inflectorFamily.Inflectors != null)
            {
                foreach (Inflector inflector in inflectorFamily.Inflectors)
                    DeleteInflector(inflector);
            }

            return true;
        }

        public List<Inflector> SimpleInflectorList
        {
            get
            {
                return _SimpleInflectorList;
            }
            set
            {
                if (value != _SimpleInflectorList)
                {
                    _SimpleInflectorList = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int SimpleInflectorListCount()
        {
            if (_SimpleInflectorList == null)
                return 0;

            return _SimpleInflectorList.Count();
        }

        public bool HasSimpleInflectorList(string label)
        {
            Inflector inflector = _SimpleInflectorList.FirstOrDefault(x => x.Label == label);
            if (inflector != null)
                return true;
            return false;
        }

        public Inflector GetSimpleInflectorList(string label)
        {
            Inflector inflector = _SimpleInflectorList.FirstOrDefault(x => x.Label == label);
            return inflector;
        }

        public Inflector GetSimpleInflectorListIndexed(int index)
        {
            if ((_SimpleInflectorList == null) || (index < 0) || (index >= _SimpleInflectorList.Count()))
                return null;

            Inflector inflector = _SimpleInflectorList[index];

            return inflector;
        }

        public void AppendSimpleInflectorList(Inflector inflector)
        {
            AppendInflector(inflector);

            if (_SimpleInflectorList != null)
                _SimpleInflectorList.Add(inflector);
            else
                _SimpleInflectorList = new List<Inflector>() { inflector };
        }

        public bool InsertSimpleInflectorList(int index, Inflector inflector)
        {
            AppendInflector(inflector);

            if (_SimpleInflectorList != null)
            {
                if ((index >= 0) && (index <= _SimpleInflectorList.Count()))
                    _SimpleInflectorList.Insert(index, inflector);
                else
                    return false;
            }
            else if (index == 0)
                _SimpleInflectorList = new List<Inflector>() { inflector };
            else
                return false;

            return true;
        }

        public bool DeleteSimpleInflectorList(Inflector inflector)
        {
            if ((inflector == null) || (_SimpleInflectorList == null))
                return false;

            bool returnValue = _SimpleInflectorList.Remove(inflector);

            if (returnValue)
                DeleteInflector(inflector);

            return returnValue;
        }

        public bool DeleteSimpleInflectorListIndexed(int index)
        {
            if ((_SimpleInflectorList == null) || (index < 0) || (index >= _SimpleInflectorList.Count()))
                return false;

            Inflector inflector = _SimpleInflectorList[index];

            _SimpleInflectorList.RemoveAt(index);

            return DeleteInflector(inflector);
        }

        public List<Inflector> CompoundInflectorList
        {
            get
            {
                return _CompoundInflectorList;
            }
            set
            {
                if (value != _CompoundInflectorList)
                {
                    _CompoundInflectorList = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int CompoundInflectorListCount()
        {
            if (_CompoundInflectorList == null)
                return 0;

            return _CompoundInflectorList.Count();
        }

        public bool HasCompoundInflectorList(string label)
        {
            Inflector inflector = _CompoundInflectorList.FirstOrDefault(x => x.Label == label);
            if (inflector != null)
                return true;
            return false;
        }

        public Inflector GetCompoundInflectorList(string label)
        {
            Inflector inflector = _CompoundInflectorList.FirstOrDefault(x => x.Label == label);
            return inflector;
        }

        public Inflector GetCompoundInflectorListIndexed(int index)
        {
            if ((_CompoundInflectorList == null) || (index < 0) || (index >= _CompoundInflectorList.Count()))
                return null;

            Inflector inflector = _CompoundInflectorList[index];

            return inflector;
        }

        public void AppendCompoundInflectorList(Inflector inflector)
        {
            AppendInflector(inflector);

            if (_CompoundInflectorList != null)
                _CompoundInflectorList.Add(inflector);
            else
                _CompoundInflectorList = new List<Inflector>() { inflector };
        }

        public bool InsertCompoundInflectorList(int index, Inflector inflector)
        {
            AppendInflector(inflector);

            if (_CompoundInflectorList != null)
            {
                if ((index >= 0) && (index <= _CompoundInflectorList.Count()))
                    _CompoundInflectorList.Insert(index, inflector);
                else
                    return false;
            }
            else if (index == 0)
                _CompoundInflectorList = new List<Inflector>() { inflector };
            else
                return false;

            return true;
        }

        public bool DeleteCompoundInflectorList(Inflector inflector)
        {
            if ((inflector == null) || (_CompoundInflectorList == null))
                return false;

            bool returnValue = _CompoundInflectorList.Remove(inflector);

            if (returnValue)
                DeleteInflector(inflector);

            return returnValue;
        }

        public bool DeleteCompoundInflectorListIndexed(int index)
        {
            if ((_CompoundInflectorList == null) || (index < 0) || (index >= _CompoundInflectorList.Count()))
                return false;

            Inflector inflector = _CompoundInflectorList[index];

            _CompoundInflectorList.RemoveAt(index);

            return DeleteInflector(inflector);
        }

        public List<Inflector> ChildInflectorList
        {
            get
            {
                return _ChildInflectorList;
            }
            set
            {
                if (value != _ChildInflectorList)
                {
                    _ChildInflectorList = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int ChildInflectorListCount()
        {
            if (_ChildInflectorList == null)
                return 0;

            return _ChildInflectorList.Count();
        }

        public bool HasChildInflectorList(string label)
        {
            Inflector inflector = _ChildInflectorList.FirstOrDefault(x => x.Label == label);
            if (inflector != null)
                return true;
            return false;
        }

        public Inflector GetChildInflectorList(string label)
        {
            Inflector inflector = _ChildInflectorList.FirstOrDefault(x => x.Label == label);
            return inflector;
        }

        public Inflector GetChildInflectorListIndexed(int index)
        {
            if ((_ChildInflectorList == null) || (index < 0) || (index >= _ChildInflectorList.Count()))
                return null;

            Inflector inflector = _ChildInflectorList[index];

            return inflector;
        }

        public void AppendChildInflectorList(Inflector inflector)
        {
            AppendInflector(inflector);

            if (_ChildInflectorList != null)
                _ChildInflectorList.Add(inflector);
            else
                _ChildInflectorList = new List<Inflector>() { inflector };
        }

        public void AppendChildInflectorList(List<Inflector> inflectors)
        {
            if (inflectors == null)
                return;

            AppendInflectors(inflectors);

            if (_ChildInflectorList != null)
                _ChildInflectorList.AddRange(inflectors);
            else
                _ChildInflectorList = new List<Inflector>(inflectors);
        }

        public bool InsertChildInflectorList(int index, Inflector inflector)
        {
            AppendInflector(inflector);

            if (_ChildInflectorList != null)
            {
                if ((index >= 0) && (index <= _ChildInflectorList.Count()))
                    _ChildInflectorList.Insert(index, inflector);
                else
                    return false;
            }
            else if (index == 0)
                _ChildInflectorList = new List<Inflector>() { inflector };
            else
                return false;

            return true;
        }

        public bool DeleteChildInflectorList(Inflector inflector)
        {
            if ((inflector == null) || (_ChildInflectorList == null))
                return false;

            bool returnValue = _ChildInflectorList.Remove(inflector);

            if (returnValue)
                DeleteInflector(inflector);

            return returnValue;
        }

        public bool DeleteChildInflectorListIndexed(int index)
        {
            if ((_ChildInflectorList == null) || (index < 0) || (index >= _ChildInflectorList.Count()))
                return false;

            Inflector inflector = _ChildInflectorList[index];

            _ChildInflectorList.RemoveAt(index);

            return DeleteInflector(inflector);
        }

        public List<IrregularTable> IrregularTables
        {
            get
            {
                return _IrregularTables;
            }
            set
            {
                if (value != _IrregularTables)
                {
                    _IrregularTables = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int IrregularTableCount()
        {
            if (_IrregularTables == null)
                return 0;

            return _IrregularTables.Count();
        }

        public bool HasIrregularTable(string label)
        {
            IrregularTable irregularTable = _IrregularTables.FirstOrDefault(x => x.Label == label);
            if (irregularTable != null)
                return true;
            return false;
        }

        public IrregularTable GetIrregularTable(string label)
        {
            IrregularTable irregularTable = _IrregularTables.FirstOrDefault(x => x.Label == label);
            return irregularTable;
        }

        public IrregularTable GetIrregularTableIndexed(int index)
        {
            if ((_IrregularTables == null) || (index < 0) || (index >= _IrregularTables.Count()))
                return null;

            IrregularTable irregularTable = _IrregularTables[index];

            return irregularTable;
        }

        public void AppendIrregularTable(IrregularTable irregularTable)
        {
            if (_IrregularTables != null)
                _IrregularTables.Add(irregularTable);
            else
                _IrregularTables = new List<IrregularTable>() { irregularTable };
        }

        public bool InsertIrregularTable(int index, IrregularTable irregularTable)
        {
            if (_IrregularTables != null)
            {
                if ((index >= 0) && (index <= _IrregularTables.Count()))
                    _IrregularTables.Insert(index, irregularTable);
                else
                    return false;
            }
            else if (index == 0)
                _IrregularTables = new List<IrregularTable>() { irregularTable };
            else
                return false;

            return true;
        }

        public bool DeleteIrregularTable(IrregularTable irregularTable)
        {
            if ((irregularTable == null) || (_IrregularTables == null))
                return false;

            bool returnValue = _IrregularTables.Remove(irregularTable);

            return returnValue;
        }

        public bool DeleteIrregularTableIndexed(int index)
        {
            if ((_IrregularTables == null) || (index < 0) || (index >= _IrregularTables.Count()))
                return false;

            _IrregularTables.RemoveAt(index);

            return true;
        }

        public bool ResolveIrregularsCheck(LanguageTool languageTool)
        {
            bool returnValue = true;

            if ((IrregularTableCount() != 0) && (IrregularsCount() == 0))
                returnValue = ResolveIrregulars(languageTool);

            return returnValue;
        }

        public bool ResolveIrregulars(LanguageTool languageTool)
        {
            bool returnValue = true;

            if (IrregularTableCount() != 0)
            {
                foreach (IrregularTable irregularTable in IrregularTables)
                {
                    List<SemiRegular> irregulars = irregularTable.CreateIrregulars();

                    foreach (SemiRegular irregular in irregulars)
                        AppendIrregular(irregular);
                }
            }

            return returnValue;
        }

        public List<SemiRegular> Irregulars
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
                    ModifiedFlag = true;
                }
            }
        }

        public int IrregularsCount()
        {
            if (_Irregulars == null)
                return 0;

            return _Irregulars.Count();
        }

        public bool HasIrregular(string label)
        {
            SemiRegular irregular = null;

            if (String.IsNullOrEmpty(label) || (_IrregularDictionary == null))
                return false;

            if (!_IrregularDictionary.TryGetValue(label, out irregular))
                return false;

            return true;
        }

        public SemiRegular GetIrregular(string label)
        {
            SemiRegular irregular = null;

            if (String.IsNullOrEmpty(label) || (_IrregularDictionary == null))
                return null;

            if (!_IrregularDictionary.TryGetValue(label, out irregular))
                return null;

            return irregular;
        }

        public SemiRegular GetIrregularIndexed(int index)
        {
            if ((_Irregulars == null) || (index < 0) || (index >= _Irregulars.Count()))
                return null;

            SemiRegular irregular = _Irregulars[index];

            return irregular;
        }

        public void AppendIrregular(SemiRegular irregular)
        {
            if (_Irregulars != null)
                _Irregulars.Add(irregular);
            else
                _Irregulars = new List<SemiRegular>() { irregular };

            AddIrregularToDictionary(irregular);
        }

        public bool InsertIrregular(int index, SemiRegular irregular)
        {
            if (_Irregulars != null)
            {
                if ((index >= 0) && (index <= _Irregulars.Count()))
                    _Irregulars.Insert(index, irregular);
                else
                    return false;
            }
            else if (index == 0)
                _Irregulars = new List<SemiRegular>() { irregular };
            else
                return false;

            AddIrregularToDictionary(irregular);

            return true;
        }

        public bool DeleteIrregular(SemiRegular irregular)
        {
            if ((irregular == null) || (_Irregulars == null))
                return false;

            bool returnValue = _Irregulars.Remove(irregular);

            RemoveIrregularFromDictionary(irregular);

            return returnValue;
        }

        public Dictionary<string, SemiRegular> IrregularDictionary
        {
            get
            {
                return _IrregularDictionary;
            }
        }

        public bool DeleteIrregularIndexed(int index)
        {
            if ((_Irregulars == null) || (index < 0) || (index >= _Irregulars.Count()))
                return false;

            SemiRegular irregular = _Irregulars[index];

            _Irregulars.RemoveAt(index);

            RemoveIrregularFromDictionary(irregular);

            return true;
        }

        protected void AddIrregularToDictionary(SemiRegular irregular)
        {
            if (irregular != null)
            {
                try
                {
                    _IrregularDictionary.Add(irregular.KeyString, irregular);
                }
                catch (Exception)
                {
                    ApplicationData.Global.PutConsoleMessage("InflectorTable.AddIrregularToDictionary duplicate entry: " + irregular.KeyString);
                }
            }
        }

        protected void RemoveIrregularFromDictionary(SemiRegular irregular)
        {
            if (irregular != null)
                _IrregularDictionary.Remove(irregular.KeyString);
        }

        public List<Inflector> Inflectors
        {
            get
            {
                return _InflectorList;
            }
            set
            {
                if (value != _InflectorList)
                {
                    _InflectorList = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int InflectorCount()
        {
            if (_InflectorList == null)
                return 0;

            return _InflectorList.Count();
        }

        public bool HasInflector(string label)
        {
            Inflector inflector = null;

            if (String.IsNullOrEmpty(label) || (_InflectorDictionary == null))
                return false;

            if (!_InflectorDictionary.TryGetValue(label, out inflector))
                return false;

            return true;
        }

        public Inflector GetInflector(string label)
        {
            Inflector inflector = null;

            if (String.IsNullOrEmpty(label) || (_InflectorDictionary == null))
                return inflector;

            _InflectorDictionary.TryGetValue(label, out inflector);

            return inflector;
        }

        public Inflector GetInflectorStartsWith(string label)
        {
            if (String.IsNullOrEmpty(label) || (_InflectorDictionary == null))
                return null;

            foreach (Inflector testInflector in _InflectorList)
            {
                if (testInflector.Label.StartsWith(label))
                    return testInflector;
            }

            return null;
        }

        public List<Inflector> GetInflectorsListSnapshot()
        {
            List<Inflector> inflectors;

            if (_InflectorList == null)
                inflectors = new List<Inflector>();
            else
                inflectors = new List<Inflector>(_InflectorList);

            return inflectors;
        }

        public List<Inflector> GetInflectorListWithWildCards(List<string> labelPatterns)
        {
            return GetInflectorListWithWildCardsStatic(labelPatterns, _InflectorList);
        }

        public static List<Inflector> GetInflectorListWithWildCardsStatic(
            List<string> labelPatterns,
            List<Inflector> sourceInflectors)
        {
            List<Inflector> inflectors = new List<Inflector>();

            if (labelPatterns == null)
                return inflectors;

            if (sourceInflectors == null)
                return inflectors;

            foreach (Inflector inflector in sourceInflectors)
            {
                foreach (string pattern in labelPatterns)
                {
                    if (TextUtilities.IsWildCardTextMatch(pattern, inflector.Label, false))
                        inflectors.Add(inflector);
                }
            }

            return inflectors;
        }

        public Inflector GetInflectorIndexed(int index)
        {
            if ((_InflectorList == null) || (index < 0) || (index >= _InflectorList.Count()))
                return null;

            Inflector inflector = _InflectorList[index];

            return inflector;
        }

        public void AppendInflector(Inflector inflector)
        {
            inflector.SetDefaultCategory(Sense.GetLexicalCategoryFromString(Type));

            if (_InflectorList != null)
                _InflectorList.Add(inflector);
            else
                _InflectorList = new List<Inflector>() { inflector };

            AddInflectorToDictionary(inflector);
        }

        public void AppendInflectors(List<Inflector> inflectors)
        {
            if (inflectors == null)
                return;

            foreach (Inflector inflector in inflectors)
                AppendInflector(inflector);
        }

        public void InsertAlternateInflector(Inflector inflector, Designator baseDesignator)
        {
            if (_InflectorList == null)
                _InflectorList = new List<Inflector>() { inflector };
            else
            {
                int count = _InflectorList.Count();
                int index;

                for (index = 0; index < count; index++)
                {
                    Inflector testInflector = _InflectorList[index];

                    if (testInflector.Label == baseDesignator.Label)
                        break;
                }

                if (index == count)
                    _InflectorList.Add(inflector);
                else
                {
                    for (index = index + 1; index < count; index++)
                    {
                        Inflector testInflector = _InflectorList[index];

                        if (!testInflector.Label.StartsWith(baseDesignator.Label))
                            break;
                    }

                    if (index == count)
                        _InflectorList.Add(inflector);
                    else
                        _InflectorList.Insert(index, inflector);
                }
            }

            AddInflectorToDictionary(inflector);
        }

        public bool DeleteInflector(Inflector inflector)
        {
            if ((inflector == null) || (_InflectorList == null))
                return false;

            RemoveInflectorFromDictionary(inflector);
            bool returnValue = _InflectorList.Remove(inflector);

            return returnValue;
        }

        public bool DeleteInflectorIndexed(int index)
        {
            if ((_InflectorList == null) || (index < 0) || (index >= _InflectorList.Count()))
                return false;

            RemoveInflectorFromDictionary(_InflectorList[index]);
            _InflectorList.RemoveAt(index);

            return true;
        }

        public Inflector GetBestInflector(Designator designator)
        {
            Inflector inflector = null;
            List<Inflector> bestInflectors = null;

            if (designator == null)
                return inflector;

            string key = designator.Label;

            if (_BestInflectorDictionary.TryGetValue(key, out inflector))
                return inflector;

            inflector = GetInflector(key);

            if (inflector != null)
            {
                _BestInflectorDictionary.Add(key, inflector);
                return inflector;
            }

            if (_InflectorList == null)
                return inflector;

            int bestWeight = 0;
            int weight = 0;

            foreach (Inflector testInflector in _InflectorList)
            {
                if (testInflector.Match(designator))
                    weight = 100;
                else
                    weight = testInflector.GetMatchWeight(designator);

                if (weight > bestWeight)
                {
                    if (bestInflectors == null)
                        bestInflectors = new List<Inflector>() { testInflector };
                    else
                    {
                        bestInflectors.Clear();
                        bestInflectors.Add(testInflector);
                    }

                    bestWeight = weight;
                }
                else if ((weight != 0) && (weight == bestWeight))
                {
                    if (bestInflectors == null)
                        bestInflectors = new List<Inflector>() { testInflector };
                    else
                        bestInflectors.Add(testInflector);
                }
            }

            if ((bestInflectors != null) && (bestInflectors.Count() != 0))
                inflector = new Inflector(designator, bestInflectors);

            _BestInflectorDictionary.Add(key, inflector);

            return inflector;
        }

        public List<Inflector> GetBestInflectors(Designator designator)
        {
            List<Inflector> bestInflectors = null;
            Inflector inflector = null;

            if (designator == null)
                return bestInflectors;

            string key = designator.Label;

            if (_BestInflectorListDictionary.TryGetValue(key, out bestInflectors))
                return bestInflectors;

            inflector = GetInflector(key);

            if (inflector != null)
            {
                bestInflectors = new List<Inflector>(1) { inflector };
                _BestInflectorListDictionary.Add(key, bestInflectors);
                return bestInflectors;
            }

            if (_InflectorList == null)
                return bestInflectors;

            int bestWeight = 0;
            int weight = 0;

            foreach (Inflector testInflector in _InflectorList)
            {
                if (testInflector.Match(designator))
                    weight = 100;
                else
                    weight = testInflector.GetMatchWeight(designator);

                if (weight != 0)
                {
                    if (weight > bestWeight)
                    {
                        if (bestInflectors == null)
                            bestInflectors = new List<Inflector>() { testInflector };
                        else
                        {
                            bestInflectors.Clear();
                            bestInflectors.Add(testInflector);
                        }

                        bestWeight = weight;
                    }
                    else if (weight == bestWeight)
                    {
                        if (bestInflectors == null)
                            bestInflectors = new List<Inflector>() { testInflector };
                        else
                            bestInflectors.Add(testInflector);
                    }
                }
            }

            _BestInflectorListDictionary.Add(key, bestInflectors);

            return bestInflectors;
        }

        public virtual bool Inflect(
            DictionaryEntry dictionaryEntry,
            int senseIndex,
            int synonymIndex,
            Designator designator,
            LanguageTool languageTool,
            out Inflection inflection)
        {
            Inflector inflector = GetInflector(designator.Label);

            inflection = null;

            if (inflector == null)
                return false;

            bool returnValue = languageTool.TableInflect(
                dictionaryEntry,
                ref senseIndex,
                ref synonymIndex,
                this,
                designator,
                out inflection);

            return returnValue;
        }

        protected void LoadInflectorDictionary(List<Inflector> inflectors)
        {
            foreach (Inflector inflector in inflectors)
                AddInflectorToDictionary(inflector);
        }

        protected void AddInflectorToDictionary(Inflector inflector)
        {
            //if (inflector.Label == "Indicative Present Negative Singular Second Archaic")
            //    ApplicationData.Global.PutConsoleMessage(inflector.Label);

            if (inflector != null)
            {
                try
                {
                    _InflectorDictionary.Add(inflector.KeyString, inflector);
                }
                catch (Exception)
                {
                    ApplicationData.Global.PutConsoleMessage("InflectorTable.AddInflectorToDictionary duplicate entry: " + inflector.KeyString);
                }
            }
        }

        protected void RemoveInflectorFromDictionary(Inflector inflector)
        {
            if (inflector != null)
                _InflectorDictionary.Remove(inflector.KeyString);
        }

        public List<InflectorTrigger> InflectorTriggerList
        {
            get
            {
                return _InflectorTriggerList;
            }
            set
            {
                if (value != _InflectorTriggerList)
                {
                    _InflectorTriggerList = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int InflectorTriggerCount()
        {
            if (_InflectorTriggerList == null)
                return 0;

            return _InflectorTriggerList.Count();
        }

        public bool HasInflectorTrigger(string label)
        {
            InflectorTrigger inflector = _InflectorTriggerList.FirstOrDefault(x => x.Label == label);
            if (inflector != null)
                return true;
            return false;
        }

        public InflectorTrigger GetInflectorTrigger(string label)
        {
            InflectorTrigger inflector = _InflectorTriggerList.FirstOrDefault(x => x.Label == label);
            return inflector;
        }

        public InflectorTrigger GetInflectorTriggerIndexed(int index)
        {
            if ((_InflectorTriggerList == null) || (index < 0) || (index >= _InflectorTriggerList.Count()))
                return null;

            InflectorTrigger inflector = _InflectorTriggerList[index];

            return inflector;
        }

        public void AppendInflectorTrigger(InflectorTrigger inflector)
        {
            if (_InflectorTriggerList != null)
                _InflectorTriggerList.Add(inflector);
            else
                _InflectorTriggerList = new List<InflectorTrigger>() { inflector };
        }

        public bool InsertInflectorTrigger(int index, InflectorTrigger inflector)
        {
            if (_InflectorTriggerList != null)
            {
                if ((index >= 0) && (index <= _InflectorTriggerList.Count()))
                    _InflectorTriggerList.Insert(index, inflector);
                else
                    return false;
            }
            else if (index == 0)
                _InflectorTriggerList = new List<InflectorTrigger>() { inflector };
            else
                return false;

            return true;
        }

        public bool DeleteInflectorTrigger(InflectorTrigger inflector)
        {
            if ((inflector == null) || (_InflectorTriggerList == null))
                return false;

            bool returnValue = _InflectorTriggerList.Remove(inflector);

            return returnValue;
        }

        public bool DeleteInflectorTriggerIndexed(int index)
        {
            if ((_InflectorTriggerList == null) || (index < 0) || (index >= _InflectorTriggerList.Count()))
                return false;

            InflectorTrigger inflector = _InflectorTriggerList[index];

            _InflectorTriggerList.RemoveAt(index);

            return true;
        }

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
                    CompoundInflectors = value;
                    ModifiedFlag = true;
                }
            }
        }

        public CompoundInflector GetCompoundInflectorIndexed(int index)
        {
            if (_CompoundInflectors == null)
                return null;

            if ((index < 0) || (index >= _CompoundInflectors.Count()))
                return null;

            CompoundInflector compoundInflector = _CompoundInflectors[index];

            return compoundInflector;
        }

        public CompoundInflector GetCompoundInflector(string label)
        {
            if (_CompoundInflectors == null)
                return null;

            CompoundInflector compoundInflector = _CompoundInflectors.FirstOrDefault(
                x => x.Label == label);

            return compoundInflector;
        }

        public void AppendCompoundInflector(CompoundInflector compoundInflector)
        {
            if (_CompoundInflectors == null)
                _CompoundInflectors = new List<CompoundInflector>() { compoundInflector };
            else
                _CompoundInflectors.Add(compoundInflector);
        }

        public void AppendCompoundInflectors(List<CompoundInflector> compoundInflectors)
        {
            if (_CompoundInflectors == null)
                _CompoundInflectors = compoundInflectors;
            else
                _CompoundInflectors.AddRange(compoundInflectors);
        }

        public int CompoundInflectorCount()
        {
            if (_CompoundInflectors != null)
                return _CompoundInflectors.Count();
            else
                return 0;
        }

        public bool ResolveCompoundInflectionsCheck(LanguageTool languageTool)
        {
            bool returnValue = true;

            if ((CompoundInflectorCount() != 0) && (CompoundInflectorListCount() == 0))
                returnValue = ResolveCompoundInflections(languageTool);

            return returnValue;
        }

        public bool ResolveCompoundInflections(LanguageTool languageTool)
        {
            bool returnValue = true;

            if (CompoundInflectorCount() != 0)
            {
                foreach (CompoundInflector compoundInflector in CompoundInflectors)
                {
                    if (!languageTool.ExpandCompoundInflector(
                            this,
                            null,
                            compoundInflector))
                        returnValue = false;
                }
            }

            return returnValue;
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

        public int SpecialInflectorCount()
        {
            if (_SpecialInflectors == null)
                return 0;

            return _SpecialInflectors.Count();
        }

        public bool ResolveSpecialInflectorsCheck(LanguageTool languageTool)
        {
            bool returnValue = true;

            if (SpecialInflectorCount() != 0)
                returnValue = ResolveSpecialInflectors(languageTool);

            return returnValue;
        }

        public bool ResolveSpecialInflectors(LanguageTool languageTool)
        {
            bool returnValue = true;

            if (SpecialInflectorCount() == 0)
                return true;

            int pass;

            for (pass = 1; ; pass++)
            {
                int count = 0;
                List<Inflector> sourceInflectors = GetInflectorsListSnapshot();

                foreach (InflectorFamily specialInflector in _SpecialInflectors)
                {
                    if (specialInflector.Pass != pass)
                        continue;

                    if (!languageTool.ExpandSpecialInflector(
                            this,
                            specialInflector,
                            sourceInflectors))
                        returnValue = false;

                    count++;
                }

                if (count == 0)
                    break;
            }

            return returnValue;
        }

        public List<InflectorGroup> InflectorGroups
        {
            get
            {
                return _InflectorGroups;
            }
            set
            {
                if (value != _InflectorGroups)
                {
                    InflectorGroups = value;
                    ModifiedFlag = true;
                }
            }
        }

        public InflectorGroup GetInflectorGroupIndexed(int index)
        {
            if (_InflectorGroups == null)
                return null;

            if ((index < 0) || (index >= _InflectorGroups.Count()))
                return null;

            InflectorGroup inflectorGroup = _InflectorGroups[index];

            return inflectorGroup;
        }

        public InflectorGroup GetInflectorGroup(string name)
        {
            if (_InflectorGroups == null)
                return null;

            InflectorGroup inflectorGroup = _InflectorGroups.FirstOrDefault(
                x => x.Name == name);

            return inflectorGroup;
        }

        public InflectorGroup GetInflectorGroupRecurse(string name)
        {
            if (_InflectorGroups == null)
                return null;

            foreach (InflectorGroup inflectorGroup in _InflectorGroups)
            {
                if (inflectorGroup.Name == name)
                    return inflectorGroup;

                InflectorGroup childInflectorGroup = inflectorGroup.GetInflectorGroupRecurse(name);

                if (childInflectorGroup != null)
                    return childInflectorGroup;
            }

            return null;
        }

        public void AppendInflectorGroup(InflectorGroup inflectorGroup)
        {
            if (inflectorGroup == null)
                return;

            if (String.IsNullOrEmpty(inflectorGroup.Type))
                inflectorGroup.Type = _Type;

            if (_InflectorGroups == null)
                _InflectorGroups = new List<InflectorGroup>() { inflectorGroup };
            else
                _InflectorGroups.Add(inflectorGroup);

            inflectorGroup.Modified = false;
        }

        public int InflectorGroupCount()
        {
            if (_InflectorGroups != null)
                return _InflectorGroups.Count();
            else
                return 0;
        }

        public bool ResolveInflectorGroupsCheck(LanguageTool languageTool)
        {
            bool returnValue = true;

            if ((InflectorGroupCount() != 0) && (ChildInflectorListCount() == 0))
                returnValue = ResolveInflectorGroups(languageTool);

            return returnValue;
        }

        public bool ResolveInflectorGroups(LanguageTool languageTool)
        {
            bool returnValue = true;

            if (InflectorGroupCount() != 0)
            {
                foreach (InflectorGroup inflectorGroup in _InflectorGroups)
                {
                    if (!inflectorGroup.ResolveInflectorGroup(languageTool, this, null))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public bool ResolveInflectorTableCheck(LanguageTool languageTool)
        {
            bool returnValue = ResolveIrregularsCheck(languageTool);

            returnValue = ResolveInflectorGroupsCheck(languageTool) && returnValue;

            returnValue = ResolveCompoundInflectionsCheck(languageTool) && returnValue;

            //returnValue = ResolveSpecialInflectorsCheck(languageTool) && returnValue;

            return returnValue;
        }

        public bool ResolveInflectorTable(LanguageTool languageTool)
        {
            bool returnValue = ResolveIrregulars(languageTool);

            returnValue = ResolveInflectorGroups(languageTool) && returnValue;

            returnValue = ResolveCompoundInflections(languageTool) && returnValue;

            //returnValue = ResolveSpecialInflectors(languageTool) && returnValue;

            return returnValue;
        }

        public List<SemiRegular> SemiRegulars
        {
            get
            {
                return _SemiRegulars;
            }
            set
            {
                if (value != _SemiRegulars)
                {
                    _SemiRegulars = value;
                    ModifiedFlag = true;
                }
            }
        }

        public SemiRegular GetSemiRegular(string label)
        {
            if (_SemiRegulars == null)
                return null;

            SemiRegular semiRegular = _SemiRegulars.FirstOrDefault(
                x => x.Label == label);

            return semiRegular;
        }

        public void AppendSemiRegular(SemiRegular semiRegular)
        {
            if (_SemiRegulars == null)
                _SemiRegulars = new List<SemiRegular>() { semiRegular };
            else
                _SemiRegulars.Add(semiRegular);
        }

        public SemiRegular FindSemiRegular(string dictionaryForm, LanguageTool tool)
        {
            if (_SemiRegulars == null)
                return null;

            SemiRegular returnValue = null;

            foreach (SemiRegular semiRegular in _SemiRegulars)
            {
                if (semiRegular.MatchCondition(dictionaryForm, tool))
                {
                    returnValue = semiRegular;
                    break;
                }
            }

            return returnValue;
        }

        public bool DeleteSemiRegular(SemiRegular semiRegular)
        {
            if (_SemiRegulars == null)
                return false;

            if (semiRegular == null)
                return false;

            return _SemiRegulars.Remove(semiRegular);
        }

        public int SemiRegularCount()
        {
            if (_SemiRegulars == null)
                return 0;

            return _SemiRegulars.Count();
        }

        public int SemiRegularOneTargetCount()
        {
            if (_SemiRegulars == null)
                return 0;

            int count = 0;

            foreach (SemiRegular semiRegular in _SemiRegulars)
            {
                if (semiRegular.TargetCount() == 1)
                    count++;
            }

            return count;
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

        public bool HasEndingsSourceWithValue(string label, string value)
        {
            if (String.IsNullOrEmpty(label) || String.IsNullOrEmpty(value) || (_EndingsSources == null))
                return false;

            if (_EndingsSources.FirstOrDefault(x => (x.KeyString == label) && (x.Text == value)) == null)
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

        public bool FindAndAppendEndingsSources()
        {
            bool returnValue = true;

            if (!FindAndAppendEndingsSources(_Irregulars))
                returnValue = false;

            if (!FindAndAppendEndingsSources(_SemiRegulars))
                returnValue = false;

            return returnValue;
        }

        public bool FindAndAppendEndingsSources(List<SemiRegular> semiRegulars)
        {
            if ((semiRegulars == null) || (semiRegulars.Count() == 0))
                return true;

            foreach (SemiRegular semiRegular in semiRegulars)
            {
                if (semiRegular.HasTargets())
                {
                    string target = semiRegular.Targets[0].GetIndexedString(0);

                    if (!HasEndingsSourceWithValue(Type, target))
                    {
                        Classifier endingSource = new Classifier(Type, target);
                        AppendEndingsSource(endingSource);
                    }
                }
            }

            return true;
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

        public InflectionsLayoutGroup GetMajorGroup(string label)
        {
            if (_MajorGroups == null)
                return null;

            InflectionsLayoutGroup majorGroup = _MajorGroups.FirstOrDefault(
                x => x.Label == label);

            return majorGroup;
        }

        public void AppendMajorGroup(InflectionsLayoutGroup majorGroup)
        {
            if (_MajorGroups == null)
                _MajorGroups = new List<InflectionsLayoutGroup>() { majorGroup };
            else
                _MajorGroups.Add(majorGroup);
        }

        public bool DeleteMajorGroup(InflectionsLayoutGroup majorGroup)
        {
            if (_MajorGroups == null)
                return false;

            if (majorGroup == null)
                return false;

            return _MajorGroups.Remove(majorGroup);
        }

        public List<InflectionsLayoutGroup> CloneMajorGroups()
        {
            if (_MajorGroups == null)
                return null;

            List<InflectionsLayoutGroup> majorGroups = new List<InflectionsLayoutGroup>(_MajorGroups.Count());

            foreach (InflectionsLayoutGroup majorGroup in _MajorGroups)
            {
                InflectionsLayoutGroup majorGroupCopy = new Language.InflectionsLayoutGroup(majorGroup);
                majorGroups.Add(majorGroupCopy);
            }

            return majorGroups;
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

        public bool GetDesignationTranslation(
            string fromLabel,
            out Designator fromDesignation,
            out Designator toDesignation)
        {
            fromDesignation = null;
            toDesignation = null;

            if (_DesignationTranslations == null)
                return false;

            int c = _DesignationTranslations.Count();
            int i;

            for (i = 0; i < c; i += 2)
            {
                if (_DesignationTranslations[i].Label == fromLabel)
                {
                    fromDesignation = _DesignationTranslations[i];
                    toDesignation = _DesignationTranslations[i + 1];
                    return true;
                }
            }

            return false;
        }

        public Designator GetDesignationTranslation(Designator fromDesignation)
        {
            Designator toDesignation = null;

            if (_DesignationTranslations == null)
                return toDesignation;

            int c = _DesignationTranslations.Count();
            int i;

            for (i = 1; i < c; i += 2)
            {
                if (fromDesignation.Match(_DesignationTranslations[i]))
                {
                    toDesignation = _DesignationTranslations[i + 1];
                    return toDesignation;
                }
            }

            return null;
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

        public bool DeleteDesignationTranslation(string label)
        {
            if (_DesignationTranslations == null)
                return false;

            Designator fromDesignation;

            int c = _DesignationTranslations.Count();
            int i;

            for (i = 1; i < c; i += 2)
            {
                fromDesignation = _DesignationTranslations[i];

                if (fromDesignation.Label == label)
                {
                    _DesignationTranslations.RemoveAt(i);
                    _DesignationTranslations.RemoveAt(i);
                    ModifiedFlag = true;
                    return true;
                }
            }

            return false;
        }

        public List<Designator> CloneDesignationTranslations()
        {
            if (_DesignationTranslations == null)
                return null;

            List<Designator> designationTranslations = new List<Designator>(_DesignationTranslations.Count());

            foreach (Designator designation in _DesignationTranslations)
            {
                Designator designationCopy = new Language.Designator(designation);
                designationTranslations.Add(designationCopy);
            }

            return designationTranslations;
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

        public bool HasCategoryStringToClassMap()
        {
            return (_CategoryStringToClassMap != null) && (_CategoryStringToClassMap.Count() != 0);
        }

        public bool GetClassesFromCategoryString(
            string categoryString,
            out string className,
            out string subClassName)
        {
            className = null;
            subClassName = null;

            if (_CategoryStringToClassMap == null)
                return false;

            string[] parts = categoryString.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);

            foreach (string categoryStringTerm in parts)
            {
                if (GetClassesFromCategoryStringTerm(categoryStringTerm, out className, out subClassName))
                    return true;
            }

            return false;
        }

        public bool GetClassesFromCategoryStringTerm(
            string categoryStringTerm,
            out string className,
            out string subClassName)
        {
            className = null;
            subClassName = null;

            if (_CategoryStringToClassMap == null)
                return false;

            Tuple<string, string> classes;

            if (_CategoryStringToClassMap.TryGetValue(categoryStringTerm, out classes))
            {
                className = classes.Item1;
                subClassName = classes.Item2;
                return true;
            }

            return false;
        }

        public void AppendCategoryStringToClassItem(
            string categoryClassTerm,
            string className,
            string subClassName)
        {
            if (_CategoryStringToClassMap == null)
                _CategoryStringToClassMap = new Dictionary<string, Tuple<string, string>>();

            _CategoryStringToClassMap.Add(
                categoryClassTerm,
                new Tuple<string, string>(className, subClassName));
        }

        public bool DeleteCategoryStringToClassItem(string categoryClassTerm)
        {
            if (_CategoryStringToClassMap == null)
                return false;

            return _CategoryStringToClassMap.Remove(categoryClassTerm);
        }

        public Dictionary<string, Tuple<string, string>> CloneCategoryStringToClassMap()
        {
            if (_CategoryStringToClassMap == null)
                return null;

            return new Dictionary<string, Tuple<string, string>>(_CategoryStringToClassMap);
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

        public int InflectorFilterCount()
        {
            if (_InflectorFilterList == null)
                return 0;

            return _InflectorFilterList.Count();
        }

        public bool HasInflectorFilter(string name)
        {
            InflectorFilter inflectorFilter = null;

            if (String.IsNullOrEmpty(name) || (_InflectorFilterDictionary == null))
                return false;

            if (!_InflectorFilterDictionary.TryGetValue(name, out inflectorFilter))
                return false;

            return true;
        }

        public InflectorFilter GetInflectorFilter(string name)
        {
            InflectorFilter inflectorFilter = null;

            if (String.IsNullOrEmpty(name) || (_InflectorFilterDictionary == null))
                return inflectorFilter;

            _InflectorFilterDictionary.TryGetValue(name, out inflectorFilter);

            return inflectorFilter;
        }

        public InflectorFilter GetInflectorFilterIndexed(int index)
        {
            if ((_InflectorFilterList == null) || (index < 0) || (index >= _InflectorFilterList.Count()))
                return null;

            InflectorFilter inflectorFilter = _InflectorFilterList[index];

            return inflectorFilter;
        }

        public void AppendInflectorFilter(InflectorFilter inflectorFilter)
        {
            if (_InflectorFilterList != null)
                _InflectorFilterList.Add(inflectorFilter);
            else
                _InflectorFilterList = new List<InflectorFilter>() { inflectorFilter };

            AddInflectorFilterToDictionary(inflectorFilter);
        }

        public void AppendInflectorFilters(List<InflectorFilter> inflectorFilters)
        {
            if (inflectorFilters == null)
                return;

            foreach (InflectorFilter inflectorFilter in inflectorFilters)
                AppendInflectorFilter(inflectorFilter);
        }

        public bool DeleteInflectorFilter(InflectorFilter inflectorFilter)
        {
            if ((inflectorFilter == null) || (_InflectorFilterList == null))
                return false;

            RemoveInflectorFilterFromDictionary(inflectorFilter);
            bool returnValue = _InflectorFilterList.Remove(inflectorFilter);

            return returnValue;
        }

        public bool DeleteInflectorFilterIndexed(int index)
        {
            if ((_InflectorFilterList == null) || (index < 0) || (index >= _InflectorFilterList.Count()))
                return false;

            RemoveInflectorFilterFromDictionary(_InflectorFilterList[index]);
            _InflectorFilterList.RemoveAt(index);

            return true;
        }

        protected void LoadInflectorFilterDictionary(List<InflectorFilter> inflectorFilters)
        {
            foreach (InflectorFilter inflectorFilter in inflectorFilters)
                AddInflectorFilterToDictionary(inflectorFilter);
        }

        protected void AddInflectorFilterToDictionary(InflectorFilter inflectorFilter)
        {
            if (inflectorFilter != null)
            {
                try
                {
                    if (_InflectorFilterDictionary == null)
                        _InflectorFilterDictionary = new Dictionary<string, InflectorFilter>();

                    _InflectorFilterDictionary.Add(inflectorFilter.KeyString, inflectorFilter);
                }
                catch (Exception)
                {
                    ApplicationData.Global.PutConsoleMessage("InflectorTable.AddInflectorFilterToDictionary duplicate entry: " + inflectorFilter.KeyString);
                }
            }
        }

        protected void RemoveInflectorFilterFromDictionary(InflectorFilter inflectorFilter)
        {
            if ((inflectorFilter != null) && (_InflectorFilterDictionary != null))
                _InflectorFilterDictionary.Remove(inflectorFilter.KeyString);
        }

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
                    ModifiedFlag = true;
                }
            }
        }

        public int HelperEntryCount()
        {
            if (_HelperEntries == null)
                return 0;

            return _HelperEntries.Count();
        }

        public bool HasHelperEntry(string name)
        {
            if (String.IsNullOrEmpty(name))
                return false;

            if (_HelperEntries == null)
                return false;

            if (_HelperEntries.FirstOrDefault(x => x.MatchKey(name)) != null)
                return true;

            return false;
        }

        public DictionaryEntry GetHelperEntry(string name)
        {
            if (String.IsNullOrEmpty(name) || (_HelperEntries == null))
                return null;

            DictionaryEntry helperEntry = _HelperEntries.FirstOrDefault(x => x.MatchKey(name));

            return helperEntry;
        }

        public DictionaryEntry GetHelperEntryIndexed(int index)
        {
            if ((_HelperEntries == null) || (index < 0) || (index >= _HelperEntries.Count()))
                return null;

            DictionaryEntry helperEntry = _HelperEntries[index];

            return helperEntry;
        }

        public void AppendHelperEntry(DictionaryEntry helperEntry)
        {
            if (_HelperEntries != null)
                _HelperEntries.Add(helperEntry);
            else
                _HelperEntries = new List<DictionaryEntry>() { helperEntry };
        }

        public void AppendHelperEntries(List<DictionaryEntry> helperEntrys)
        {
            if (helperEntrys == null)
                return;

            foreach (DictionaryEntry helperEntry in helperEntrys)
                AppendHelperEntry(helperEntry);
        }

        public bool DeleteHelperEntry(DictionaryEntry helperEntry)
        {
            if ((helperEntry == null) || (_HelperEntries == null))
                return false;

            bool returnValue = _HelperEntries.Remove(helperEntry);

            return returnValue;
        }

        public bool DeleteDictionaryEntryIndexed(int index)
        {
            if ((_HelperEntries == null) || (index < 0) || (index >= _HelperEntries.Count()))
                return false;

            _HelperEntries.RemoveAt(index);

            return true;
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            element.Add(new XAttribute("Name", KeyString));

            string targetLanguagesKey = TargetLanguagesKey;

            if (!String.IsNullOrEmpty(targetLanguagesKey))
                element.Add(new XAttribute("LanguageIDs", targetLanguagesKey));

            if (!String.IsNullOrEmpty(_Type))
                element.Add(new XAttribute("Type", _Type));

            if (_InflectorTableDocumentation != null)
                element.Add(_InflectorTableDocumentation);

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

            if ((_DesignatorsTemplateTables != null) && (_DesignatorsTemplateTables.Count() != 0))
            {
                foreach (DesignatorTable designatorsTemplateTable in _DesignatorsTemplateTables)
                {
                    XElement designatorsTemplateTableElement = designatorsTemplateTable.GetElement("DesignatorsTemplate");
                    element.Add(designatorsTemplateTableElement);
                }
            }

            if ((_ClassKeys != null) && (_ClassKeys.Count() != 0))
                element.Add(ObjectUtilities.GetElementFromStringList("ClassKeys", _ClassKeys));

            if (!String.IsNullOrEmpty(_StemType))
                element.Add(new XElement("StemType", _StemType));

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

            if (_StemList != null)
            {
                foreach (Inflector stemEntry in _StemList)
                {
                    XElement stemEntryElement = stemEntry.GetElement("Stem");
                    element.Add(stemEntryElement);
                }
            }

            if (_InflectorFamilyList != null)
            {
                foreach (InflectorFamily inflectorFamilyEntry in _InflectorFamilyList)
                {
                    XElement inflectorFamilyEntryElement = inflectorFamilyEntry.GetElement("InflectorFamily");
                    element.Add(inflectorFamilyEntryElement);
                }
            }

            if (_SimpleInflectorList != null)
            {
                foreach (Inflector inflectorEntry in _SimpleInflectorList)
                {
                    XElement inflectorEntryElement = inflectorEntry.GetElement("Inflector");
                    element.Add(inflectorEntryElement);
                }
            }

            if (_InflectorTriggerList != null)
            {
                foreach (InflectorTrigger inflectorTrigger in _InflectorTriggerList)
                {
                    XElement inflectorTriggerElement = inflectorTrigger.GetElement("Inflector");
                    element.Add(inflectorTriggerElement);
                }
            }

            if (_HelperEntries != null)
            {
                foreach (DictionaryEntry helperEntry in _HelperEntries)
                {
                    XElement helperEntryElement = helperEntry.GetElement("HelperEntry");
                    element.Add(helperEntryElement);
                }
            }

            if (_CompoundInflectors != null)
            {
                foreach (CompoundInflector compoundInflector in _CompoundInflectors)
                {
                    XElement compoundInflectorElement = compoundInflector.GetElement("CompoundInflector");
                    element.Add(compoundInflectorElement);
                }
            }

            if ((_SpecialInflectors != null) && (_SpecialInflectors.Count() != 0))
            {
                foreach (InflectorFamily specialInflector in _SpecialInflectors)
                    element.Add(specialInflector.GetElement("SpecialInflector"));
            }

            if (_InflectorFilterList != null)
            {
                foreach (InflectorFilter inflectorFilter in _InflectorFilterList)
                {
                    XElement stemEntryElement = inflectorFilter.GetElement("InflectorFilter");
                    element.Add(stemEntryElement);
                }
            }

            if (_InflectorGroups != null)
            {
                foreach (InflectorGroup inflectorGroup in _InflectorGroups)
                {
                    XElement inflectorGroupElement = inflectorGroup.GetElement("InflectorGroup");
                    element.Add(inflectorGroupElement);
                }
            }

            if (_IrregularTables != null)
            {
                foreach (IrregularTable irregularTable in _IrregularTables)
                {
                    XElement irregularTableElement = irregularTable.GetElement("IrregularTable");
                    element.Add(irregularTableElement);
                }
            }

            if (_SemiRegulars != null)
            {
                foreach (SemiRegular semiRegular in _SemiRegulars)
                {
                    XElement semiRegularElement = semiRegular.GetElement("SemiRegular");
                    element.Add(semiRegularElement);
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
                case "InflectorTableDocumentation":
                    _InflectorTableDocumentation = childElement;
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
                case "ClassKeys":
                    _ClassKeys = ObjectUtilities.GetStringListFromElement(childElement);
                    break;
                case "StemType":
                    _StemType = childElement.Value.Trim();
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
                case "DesignatorsTemplate":
                    {
                        DesignatorTable designatorsTemplateTable = new DesignatorTable(childElement);
                        if (_DesignatorsTemplateTables == null)
                            _DesignatorsTemplateTables = new List<DesignatorTable>() { designatorsTemplateTable };
                        else
                            _DesignatorsTemplateTables.Add(designatorsTemplateTable);
                    }
                    break;
                case "Stem":
                    {
                        Inflector stem = new Inflector(childElement);
                        string scope;
                        if (!String.IsNullOrEmpty(stem.Scope))
                            scope = stem.Scope;
                        else
                            scope = "Both";
                        AppendStem(stem);
                    }
                    break;
                case "InflectorFamily":
                    {
                        InflectorFamily inflectorFamily = new InflectorFamily(childElement);
                        if (inflectorFamily.Inflectors != null)
                        {
                            foreach (Inflector inflector in inflectorFamily.Inflectors)
                                AppendDesignator(inflector, "Both");
                        }
                        AppendInflectorFamilyList(inflectorFamily);
                    }
                    break;
                case "Inflector":
                    {
                        Inflector inflector = new Inflector(childElement);
                        string scope;
                        if (!String.IsNullOrEmpty(inflector.Scope))
                            scope = inflector.Scope;
                        else
                            scope = "Both";
                        AppendDesignator(inflector, scope);
                        AppendSimpleInflectorList(inflector);
                    }
                    break;
                case "InflectorTrigger":
                    {
                        InflectorTrigger inflectorTrigger = new InflectorTrigger(childElement);
                        AppendInflectorTrigger(inflectorTrigger);
                    }
                    break;
                case "HelperEntry":
                    {
                        DictionaryEntry helperEntry = new DictionaryEntry(childElement);
                        AppendHelperEntry(helperEntry);
                    }
                    break;
                case "CompoundInflector":
                    {
                        CompoundInflector compoundInflector = new CompoundInflector(childElement);
                        AppendCompoundInflector(compoundInflector);
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
                case "InflectorFilter":
                    {
                        InflectorFilter inflectorFilter = new InflectorFilter(childElement);
                        AppendInflectorFilter(inflectorFilter);
                    }
                    break;
                case "InflectorGroup":
                    {
                        InflectorGroup inflectorGroup = new InflectorGroup(childElement);
                        AppendInflectorGroup(inflectorGroup);
                    }
                    break;
                case "IrregularTable":
                    {
                        IrregularTable irregularTable = new IrregularTable(childElement);
                        AppendIrregularTable(irregularTable);
                    }
                    break;
                case "SemiRegular":
                    {
                        SemiRegular semiRegular = new SemiRegular(childElement);
                        AppendSemiRegular(semiRegular);
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
                    return false;
            }

            return true;
        }
    }
}
