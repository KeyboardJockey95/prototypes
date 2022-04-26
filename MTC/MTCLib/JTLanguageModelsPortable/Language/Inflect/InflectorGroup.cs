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
    public class InflectorGroup : Designator
    {
        protected string _Type;
        protected string _Scope;                            // "All", "Default", "Both"
        protected List<DesignatorTable> _DesignatorTables;
        protected List<DesignatorTable> _DesignatorsTemplateTables;
        protected List<Inflector> _SimpleInflectorList;
        protected List<CompoundInflector> _CompoundInflectors;
        protected List<InflectorGroup> _InflectorGroups;
        protected List<Inflector> _CompoundInflectorList;   // Not saved. Generated from CompoundInflectors.
        protected List<Inflector> _ChildInflectorList;      // Not saved. Generated from InflectorGroups.
        protected List<Inflector> _InflectorList;           // Not saved. Collects all inflectors.
        protected Dictionary<string, Inflector> _InflectorDictionary;   // Not saved.

        public InflectorGroup(
            string name,
            string type,
            string scope,
            List<Classifier> classifications,
            List<DesignatorTable> designatorTables,
            List<DesignatorTable> designatorsTemplateTables,
            List<Inflector> simpleInflectors,
            List<CompoundInflector> compoundInflectors,
            List<InflectorGroup> inflectorGroups) :  base(name, classifications)
        {
            ClearInflectorGroup();
            _Type = type;
            _Scope = scope;
            _DesignatorTables = designatorTables;
            _DesignatorsTemplateTables = designatorsTemplateTables;
            _SimpleInflectorList = simpleInflectors;
            _CompoundInflectors = compoundInflectors;
            _InflectorGroups = inflectorGroups;
            LoadInflectorDictionary(simpleInflectors);
        }

        public InflectorGroup(XElement element)
        {
            ClearInflectorGroup();
            OnElement(element);
            DefaultLabelCheck();
        }

        public InflectorGroup(InflectorGroup other) : base(other)
        {
            CopyInflectorGroup(other);
        }

        public InflectorGroup()
        {
            ClearInflectorGroup();
        }

        public void ClearInflectorGroup()
        {
            _Type = String.Empty;
            _Scope = String.Empty;
            _DesignatorTables = new List<DesignatorTable>();
            _DesignatorsTemplateTables = new List<DesignatorTable>();
            _SimpleInflectorList = new List<Inflector>();
            _CompoundInflectors = null;
            _InflectorGroups = null;
            _CompoundInflectorList = new List<Inflector>();
            _ChildInflectorList = new List<Inflector>();
            _InflectorList = new List<Inflector>();
            _InflectorDictionary = new Dictionary<string, Inflector>();
        }

        public void CopyInflectorGroup(InflectorGroup other)
        {
            _Type = other.Type;
            _Scope = other.Scope;
            _DesignatorTables = other.DesignatorTables;
            _DesignatorsTemplateTables = other.DesignatorsTemplateTables;
            _SimpleInflectorList = other.SimpleInflectorList;
            _CompoundInflectors = other.CompoundInflectors;
            _InflectorGroups = other.InflectorGroups;
            _CompoundInflectorList = other.CompoundInflectorList;
            _ChildInflectorList = other.ChildInflectorList;
            _InflectorList = other.Inflectors;
            _InflectorDictionary = new Dictionary<string, Inflector>();
            LoadInflectorDictionary(_InflectorList);
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

        public string Scope
        {
            get
            {
                return _Scope;
            }
            set
            {
                if (value != _Scope)
                {
                    _Scope = value;
                    ModifiedFlag = true;
                }
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

        public Inflector GetInflectorIndexed(int index)
        {
            if ((_InflectorList == null) || (index < 0) || (index >= _InflectorList.Count()))
                return null;

            Inflector inflector = _InflectorList[index];

            return inflector;
        }

        public void AppendInflector(Inflector inflector)
        {
            if (!String.IsNullOrEmpty(Type))
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

            inflector = GetInflector(designator.Label);

            if (inflector != null)
                return inflector;

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

            return inflector;
        }

        public List<Inflector> GetBestInflectors(Designator designator)
        {
            List<Inflector> bestInflectors = null;
            Inflector inflector = null;

            if (designator == null)
                return bestInflectors;

            inflector = GetInflector(designator.Label);

            if (inflector != null)
            {
                bestInflectors = new List<Inflector>(1) { inflector };
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

            return bestInflectors;
        }

        protected void LoadInflectorDictionary(List<Inflector> inflectors)
        {
            foreach (Inflector inflector in inflectors)
                AddInflectorToDictionary(inflector);
        }

        protected void AddInflectorToDictionary(Inflector inflector)
        {
            if (inflector != null)
            {
                try
                {
                    _InflectorDictionary.Add(inflector.KeyString, inflector);
                }
                catch (Exception)
                {
                    ApplicationData.Global.PutConsoleMessage("InflectorGroup.AddInflectorToDictionary duplicate entry: " + inflector.KeyString);
                }
            }
        }

        protected void RemoveInflectorFromDictionary(Inflector inflector)
        {
            if (inflector != null)
                _InflectorDictionary.Remove(inflector.KeyString);
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

        public int CompoundInflectorCount()
        {
            if (_CompoundInflectors != null)
                return _CompoundInflectors.Count();
            else
                return 0;
        }

        public bool ResolveCompoundInflectionsCheck(
            LanguageTool languageTool,
            InflectorTable inflectorTable)
        {
            bool returnValue = true;

            if ((CompoundInflectorCount() != 0) && (CompoundInflectorListCount() == 0))
                returnValue = ResolveCompoundInflections(languageTool, inflectorTable);

            return returnValue;
        }

        public bool ResolveCompoundInflections(
            LanguageTool languageTool,
            InflectorTable inflectorTable)
        {
            bool returnValue = true;

            if (CompoundInflectorCount() != 0)
            {
                foreach (CompoundInflector compoundInflector in CompoundInflectors)
                {
                    if (!languageTool.ExpandCompoundInflector(
                            inflectorTable,
                            this,
                            compoundInflector))
                        returnValue = false;
                }


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
        }

        public int InflectorGroupCount()
        {
            if (_InflectorGroups != null)
                return _InflectorGroups.Count();
            else
                return 0;
        }

        public bool ResolveInflectorGroupsCheck(
            LanguageTool languageTool,
            InflectorTable inflectorTable)
        {
            bool returnValue = true;

            if (InflectorGroupCount() != 0)
                returnValue = ResolveInflectorGroups(languageTool, inflectorTable);

            return returnValue;
        }

        public bool ResolveInflectorGroups(
            LanguageTool languageTool,
            InflectorTable inflectorTable)
        {
            bool returnValue = true;

            if (InflectorGroupCount() != 0)
            {
                foreach (InflectorGroup inflectorGroup in _InflectorGroups)
                {
                    if (!inflectorGroup.ResolveInflectorGroup(languageTool, inflectorTable, this))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public bool ResolveInflectorGroupCheck(
            LanguageTool languageTool,
            InflectorTable inflectorTable,
            InflectorGroup parentInflectorGroup)
        {
            bool returnValue = true;

            if (((CompoundInflectorCount() != 0) && (CompoundInflectorListCount() == 0)) ||
                ((InflectorGroupCount() != 0) && (ChildInflectorListCount() == 0)))
            {
                returnValue = ResolveInflectorGroupsCheck(languageTool, inflectorTable) && returnValue;

                returnValue = ResolveCompoundInflectionsCheck(languageTool, inflectorTable) && returnValue;

                CopyDesignatorsAndInflectorsToParent(inflectorTable, parentInflectorGroup);
            }

            return returnValue;
        }

        public bool ResolveInflectorGroup(
            LanguageTool languageTool,
            InflectorTable inflectorTable,
            InflectorGroup parentInflectorGroup)
        {
            bool returnValue = ResolveInflectorGroups(languageTool, inflectorTable);

            returnValue = ResolveCompoundInflections(languageTool, inflectorTable) && returnValue;

            CopyDesignatorsAndInflectorsToParent(inflectorTable, parentInflectorGroup);

            return returnValue;
        }

        public void CopyDesignatorsAndInflectorsToParent(
            InflectorTable inflectorTable,
            InflectorGroup parentInflectorGroup)
        {
            if (parentInflectorGroup != null)
            {
                if (_DesignatorTables != null)
                {
                    foreach (DesignatorTable designatorTable in _DesignatorTables)
                        parentInflectorGroup.AppendDesignators(
                            designatorTable.Designators,
                            designatorTable.Scope);
                }

                parentInflectorGroup.AppendInflectors(Inflectors);
            }
            else
            {
                if (_DesignatorTables != null)
                {
                    foreach (DesignatorTable designatorTable in _DesignatorTables)
                        inflectorTable.AppendDesignators(
                            designatorTable.Designators,
                            designatorTable.Scope);
                }

                inflectorTable.AppendChildInflectorList(Inflectors);
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            element.Add(new XAttribute("Name", KeyString));

            if (!String.IsNullOrEmpty(_Type))
                element.Add(new XAttribute("Type", _Type));

            if (!String.IsNullOrEmpty(Scope))
                element.Add(new XAttribute("Scope", Scope));

            if ((_DesignatorsTemplateTables != null) && (_DesignatorsTemplateTables.Count() != 0))
            {
                foreach (DesignatorTable designatorsTemplateTable in _DesignatorsTemplateTables)
                {
                    XElement designatorsTemplateTableElement = designatorsTemplateTable.GetElement("DesignatorsTemplate");
                    element.Add(designatorsTemplateTableElement);
                }
            }

            if (_SimpleInflectorList != null)
            {
                foreach (Inflector inflectorEntry in _SimpleInflectorList)
                {
                    XElement inflectorEntryElement = new XElement("Inflector");
                    element.Add(inflectorEntryElement);
                }
            }

            if (_CompoundInflectors != null)
            {
                foreach (CompoundInflector compoundInflector in _CompoundInflectors)
                {
                    XElement compoundInflectorElement = new XElement("CompoundInflector");
                    element.Add(compoundInflectorElement);
                }
            }

            if (_InflectorGroups != null)
            {
                foreach (InflectorGroup inflectorGroup in _InflectorGroups)
                {
                    XElement inflectorGroupElement = new XElement("InflectorGroup");
                    element.Add(inflectorGroupElement);
                }
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
                case "Type":
                    Type = attributeValue;
                    break;
                case "Scope":
                    Scope = attributeValue;
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
                case "CompoundInflector":
                    {
                        CompoundInflector compoundInflector = new CompoundInflector(childElement);
                        AppendCompoundInflector(compoundInflector);
                    }
                    break;
                case "InflectorGroup":
                    {
                        InflectorGroup inflectorGroup = new InflectorGroup(childElement);
                        AppendInflectorGroup(inflectorGroup);
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
                default:
                    return false;
            }

            return true;
        }
    }
}
