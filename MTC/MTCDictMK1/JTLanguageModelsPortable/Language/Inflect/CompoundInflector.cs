using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;


namespace JTLanguageModelsPortable.Language
{
    public class CompoundInflector : BaseObjectKeyed
    {
        protected Designator _CompoundDesignatorPartial;
        protected string _Scope;
        protected string _TargetDesignatorLabel;
        protected List<Modifier> _TargetModifiers;
        protected string _Helper;
        protected int _HelperSenseIndex;
        protected int _HelperSynonymIndex;
        protected string _HelperDestination;
        protected List<int> _HelperLanguageIndexMap;
        protected List<ModifierFixup> _ModifierFixups;
        protected List<SpecialAction> _HelperActions;
        protected List<SpecialAction> _PostActions;
        protected List<string> _DesignatorsTemplateNames;
        protected List<List<Designator>> _DesignatorLists;
        protected List<Inflector> _ContractionInflectors;
        protected string _FilterName;

        public CompoundInflector(
                string label,
                Designator compoundDesignatorPartial,
                string scope,
                string targetDesignator,
                List<Modifier> targetModifiers,
                string helper,
                int helperSenseIndex,
                int helperSynonymIndex,
                string helperDestination,
                List<int> helperLanguageIndexMap,
                List<ModifierFixup> modifierFixups,
                List<SpecialAction> helperActions,
                List<SpecialAction> postActions,
                List<string> designatorsTemplateNames,
                List<List<Designator>> designatorLists,
                List<Inflector> contractionInflectors,
                string filterName) :
            base(label)
        {
            _CompoundDesignatorPartial = compoundDesignatorPartial;
            _Scope = scope;
            _TargetDesignatorLabel = targetDesignator;
            _TargetModifiers = targetModifiers;
            _Helper = helper;
            _HelperSenseIndex = helperSenseIndex;
            _HelperSynonymIndex = helperSynonymIndex;
            _HelperDestination = helperDestination;
            _HelperLanguageIndexMap = helperLanguageIndexMap;
            _ModifierFixups = modifierFixups;
            _HelperActions = helperActions;
            _PostActions = postActions;
            _DesignatorsTemplateNames = designatorsTemplateNames;
            _ContractionInflectors = contractionInflectors;
            _FilterName = filterName;
        }

        public CompoundInflector(XElement element)
        {
            ClearCompoundInflector();
            OnElement(element);
        }

        public CompoundInflector(CompoundInflector other) : base(other)
        {
            CopyCompoundInflector(other);
        }

        public CompoundInflector()
        {
            ClearCompoundInflector();
        }

        public void ClearCompoundInflector()
        {
            _CompoundDesignatorPartial = null;
            _Scope = "Both";
            _TargetDesignatorLabel = null;
            _TargetModifiers = null;
            _Helper = null;
            _HelperSenseIndex = -1;
            _HelperSynonymIndex = -1;
            _HelperDestination = null;
            _HelperLanguageIndexMap = null;
            _ModifierFixups = null;
            _HelperActions = null;
            _PostActions = null;
            _DesignatorsTemplateNames = null;
            _DesignatorLists = new List<List<Designator>>();
            _ContractionInflectors = null;
            _FilterName = null;
        }

        public void CopyCompoundInflector(CompoundInflector other)
        {
            _CompoundDesignatorPartial = other.CompoundDesignatorPartial;
            _Scope = other.Scope;
            _TargetDesignatorLabel = other.TargetDesignatorLabel;
            _TargetModifiers = other.CloneTargetModifiers();
            _Helper = other.Helper;
            _HelperSenseIndex = other.HelperSenseIndex;
            _HelperSynonymIndex = other.HelperSynonymIndex;
            _HelperDestination = other.HelperDestination;
            _HelperLanguageIndexMap = other.HelperLanguageIndexMap;
            _ModifierFixups = other.ModifierFixups;
            _HelperActions = other.HelperActions;
            _PostActions = other.PostActions;
            _DesignatorsTemplateNames = other.CloneDesignatorsTemplateNames();
            _DesignatorLists = other.CloneDesignatorLists();
            _ContractionInflectors = CloneContractionInflectors();
            _FilterName = other.FilterName;
        }

        public string Label
        {
            get
            {
                return KeyString;
            }
            set
            {
                Key = value;
            }
        }

        public Designator CompoundDesignatorPartial
        {
            get
            {
                return _CompoundDesignatorPartial;
            }
            set
            {
                if (value != _CompoundDesignatorPartial)
                {
                    _CompoundDesignatorPartial = value;
                    ModifiedFlag = true;
                }
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

        public string TargetDesignatorLabel
        {
            get
            {
                return _TargetDesignatorLabel;
            }
            set
            {
                if (value != _TargetDesignatorLabel)
                {
                    _TargetDesignatorLabel = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<Modifier> TargetModifiers
        {
            get
            {
                return _TargetModifiers;
            }
            set
            {
                if (value != _TargetModifiers)
                {
                    _TargetModifiers = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<Modifier> CloneTargetModifiers()
        {
            if (_TargetModifiers == null)
                return null;

            List<Modifier> targetModifiers = new List<Modifier>();

            foreach (Modifier targetModifier in _TargetModifiers)
            {
                Modifier newModifier = new Modifier(targetModifier);
                targetModifiers.Add(newModifier);
            }

            return targetModifiers;
        }

        public int TargetModifierCount()
        {
            if (_TargetModifiers == null)
                return 0;

            return _TargetModifiers.Count();
        }

        public string Helper
        {
            get
            {
                return _Helper;
            }
            set
            {
                if (value != _Helper)
                {
                    _Helper = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int HelperSenseIndex
        {
            get
            {
                return _HelperSenseIndex;
            }
            set
            {
                if (value != _HelperSenseIndex)
                {
                    _HelperSenseIndex = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int HelperSynonymIndex
        {
            get
            {
                return _HelperSynonymIndex;
            }
            set
            {
                if (value != _HelperSynonymIndex)
                {
                    _HelperSynonymIndex = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string HelperDestination
        {
            get
            {
                return _HelperDestination;
            }
            set
            {
                if (value != _HelperDestination)
                {
                    _HelperDestination = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<int> HelperLanguageIndexMap
        {
            get
            {
                return _HelperLanguageIndexMap;
            }
            set
            {
                if (value != _HelperLanguageIndexMap)
                {
                    _HelperLanguageIndexMap = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<ModifierFixup> ModifierFixups
        {
            get
            {
                return _ModifierFixups;
            }
            set
            {
                if (value != _ModifierFixups)
                {
                    _ModifierFixups = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<SpecialAction> HelperActions
        {
            get
            {
                return _HelperActions;
            }
            set
            {
                if (value != _HelperActions)
                {
                    _HelperActions = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<SpecialAction> PostActions
        {
            get
            {
                return _PostActions;
            }
            set
            {
                if (value != _PostActions)
                {
                    _PostActions = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int PostActionCount()
        {
            if (_PostActions == null)
                return 0;

            return _PostActions.Count();
        }

        public List<string> DesignatorsTemplateNames
        {
            get
            {
                return _DesignatorsTemplateNames;
            }
            set
            {
                if (_DesignatorsTemplateNames != value)
                {
                    _DesignatorsTemplateNames = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> CloneDesignatorsTemplateNames()
        {
            if (_DesignatorsTemplateNames == null)
                return null;

            return new List<string>(_DesignatorsTemplateNames);
        }

        public string GetDesignatorsTemplateNameIndexed(int index)
        {
            if (_DesignatorsTemplateNames == null)
                return null;

            if ((index < 0) || (index >= _DesignatorsTemplateNames.Count()))
                return null;

            return _DesignatorsTemplateNames[index];
        }

        public void AppendDesignatorsTemplateName(string designatorsTemplateName)
        {
            if (_DesignatorsTemplateNames == null)
                _DesignatorsTemplateNames = new List<string>() { designatorsTemplateName };
            else
                _DesignatorsTemplateNames.Add(designatorsTemplateName);

            ModifiedFlag = true;
        }

        public int DesignatorsTemplateNameCount()
        {
            if (_DesignatorsTemplateNames == null)
                return 0;

            return _DesignatorsTemplateNames.Count();
        }

        public List<List<Designator>> DesignatorLists
        {
            get
            {
                return _DesignatorLists;
            }
            set
            {
                if (value != _DesignatorLists)
                {
                    _DesignatorLists = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<List<Designator>> CloneDesignatorLists()
        {
            List<List<Designator>> designatorLists = new List<List<Designator>>();

            foreach (List<Designator> designatorList in _DesignatorLists)
            {
                List<Designator> newDesignatorList = new List<Designator>(designatorList);
                designatorLists.Add(newDesignatorList);
            }

            return designatorLists;
        }

        public List<Designator> GetDesignatorListIndexed(int index)
        {
            if ((index < 0) || (index >= _DesignatorLists.Count()))
                return null;

            return _DesignatorLists[index];
        }

        public List<Designator> CreateSimpleDesignators(InflectorTable inflectorTable)
        {
            return CreateDesignatorsRecurse(inflectorTable, null, 0);
        }

        public List<Designator> CreateSimpleDesignators(InflectorGroup inflectorGroup)
        {
            return CreateDesignatorsRecurse(inflectorGroup, null, 0);
        }

        public List<Designator> CreateCompoundDesignators(InflectorTable inflectorTable)
        {
            List<Designator> simpleDesignators = CreateSimpleDesignators(inflectorTable);
            List<Designator> compoundDesignators = new List<Designator>();

            foreach (Designator simpleDesignator in simpleDesignators)
            {
                Designator compoundDesignator = new Designator(CompoundDesignatorPartial);

                foreach (Classifier simpleClassification in simpleDesignator.Classifications)
                {
                    string key = simpleClassification.KeyString;
                    string value = simpleClassification.Text;
                    Classifier compoundClassification = compoundDesignator.GetClassificationWithValue(key, value);

                    if (compoundClassification == null)
                        compoundDesignator.AppendClassification(simpleClassification.KeyString, simpleClassification.Text);
                    //else if ((key == "Aspect") && (simpleClassification.Text != "Simple"))
                    //    compoundDesignator.AppendClassification(simpleClassification.KeyString, simpleClassification.Text);
                }

                compoundDesignator.DefaultLabel();
                compoundDesignators.Add(compoundDesignator);
            }

            return compoundDesignators;
        }

        public List<Designator> CreateCompoundDesignators(InflectorGroup inflectorGroup)
        {
            List<Designator> simpleDesignators = CreateSimpleDesignators(inflectorGroup);
            List<Designator> compoundDesignators = new List<Designator>();

            foreach (Designator simpleDesignator in simpleDesignators)
            {
                Designator compoundDesignator = new Designator(CompoundDesignatorPartial);

                foreach (Classifier simpleClassification in simpleDesignator.Classifications)
                {
                    string key = simpleClassification.KeyString;
                    string value = simpleClassification.Text;
                    Classifier compoundClassification = compoundDesignator.GetClassificationWithValue(key, value);

                    if (compoundClassification == null)
                        compoundDesignator.AppendClassification(simpleClassification.KeyString, simpleClassification.Text);
                    //else if ((key == "Aspect") && (simpleClassification.Text != "Simple"))
                    //    compoundDesignator.AppendClassification(simpleClassification.KeyString, simpleClassification.Text);
                }

                compoundDesignator.DefaultLabel();
                compoundDesignators.Add(compoundDesignator);
            }

            return compoundDesignators;
        }

        public List<Designator> CreateDesignatorsRecurse(
            InflectorTable inflectorTable,
            List<Designator> designators,
            int level)
        {
            List<Designator> levelDesignators = GetDesignatorListIndexed(level);

            if (levelDesignators == null)
            {
                string designatorsTemplateName = GetDesignatorsTemplateNameIndexed(level);

                if (!String.IsNullOrEmpty(designatorsTemplateName))
                {
                    DesignatorTable designatorsTemplateTable = inflectorTable.GetDesignatorsTemplateTable(designatorsTemplateName);

                    if (designatorsTemplateTable != null)
                        levelDesignators = designatorsTemplateTable.Designators;

                    if (levelDesignators == null)
                    {
                        InflectorGroup childInflectorGroup = inflectorTable.GetInflectorGroupRecurse(designatorsTemplateName);

                        if (childInflectorGroup != null)
                            levelDesignators = childInflectorGroup.GetDesignators("All");
                    }

                    if (levelDesignators == null)
                        throw new Exception("Can't find designator template table: " + designatorsTemplateName);
                }

                if (levelDesignators == null)
                    return designators;
            }

            if (designators != null)
            {
                List<Designator> newDesignators = new List<Designator>();

                foreach (Designator classDesignator in designators)
                {
                    foreach (Designator levelDesignator in levelDesignators)
                    {
                        Designator newDesignator = new Designator(classDesignator);
                        newDesignator.AppendClassifications(levelDesignator);
                        newDesignator.DefaultLabel();
                        newDesignators.Add(newDesignator);
                    }
                }

                return CreateDesignatorsRecurse(inflectorTable, newDesignators, level + 1);
            }
            else
                return CreateDesignatorsRecurse(inflectorTable, levelDesignators, level + 1);
        }

        public List<Designator> CreateDesignatorsRecurse(
            InflectorGroup inflectorGroup,
            List<Designator> designators,
            int level)
        {
            List<Designator> levelDesignators = GetDesignatorListIndexed(level);

            if (levelDesignators == null)
            {
                string designatorsTemplateName = GetDesignatorsTemplateNameIndexed(level);

                if (!String.IsNullOrEmpty(designatorsTemplateName))
                {
                    DesignatorTable designatorsTemplateTable = inflectorGroup.GetDesignatorsTemplateTable(designatorsTemplateName);

                    if (designatorsTemplateTable != null)
                        levelDesignators = designatorsTemplateTable.Designators;

                    if (levelDesignators == null)
                    {
                        InflectorGroup childInflectorGroup = inflectorGroup.GetInflectorGroupRecurse(designatorsTemplateName);

                        if (childInflectorGroup != null)
                            levelDesignators = childInflectorGroup.GetDesignators("All");
                    }

                    if (levelDesignators == null)
                        throw new Exception("Can't find designator template table: " + designatorsTemplateName);
                }

                if (levelDesignators == null)
                    return designators;
            }

            if (designators != null)
            {
                List<Designator> newDesignators = new List<Designator>();

                foreach (Designator classDesignator in designators)
                {
                    foreach (Designator levelDesignator in levelDesignators)
                    {
                        Designator newDesignator = new Designator(classDesignator);
                        newDesignator.AppendClassifications(levelDesignator);
                        newDesignator.DefaultLabel();
                        newDesignators.Add(newDesignator);
                    }
                }

                return CreateDesignatorsRecurse(inflectorGroup, newDesignators, level + 1);
            }
            else
                return CreateDesignatorsRecurse(inflectorGroup, levelDesignators, level + 1);
        }

        public List<Inflector> ContractionInflectors
        {
            get
            {
                return _ContractionInflectors;
            }
            set
            {
                if (_ContractionInflectors != value)
                {
                    _ContractionInflectors = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<Inflector> CloneContractionInflectors()
        {
            if (_ContractionInflectors == null)
                return null;

            List<Inflector> contractionInflectors = new List<Inflector>();

            foreach (Inflector inflector in _ContractionInflectors)
                contractionInflectors.Add(new Inflector(inflector));

            return contractionInflectors;
        }

        public int ContractionInflectorCount()
        {
            if (_ContractionInflectors == null)
                return 0;

            return _ContractionInflectors.Count();
        }

        public void DoHelperActions(Inflection helperInflection)
        {
            if ((_HelperActions == null) || (_HelperActions.Count() == 0))
                return;

            string caseLabel = helperInflection.Designation.Label;

            foreach (SpecialAction action in _HelperActions)
            {
                bool done = false;

                if (!action.MatchCases(caseLabel))
                    continue;

                switch (action.Type)
                {
                    case "ReplaceInOutputs":
                        {
                            int languageCount = helperInflection.Output.Count();
                            int languageIndex;
                            LanguageString ls;
                            string input;
                            string output;
                            for (languageIndex = 0; languageIndex < languageCount; languageIndex++)
                            {
                                input = action.Input.GetIndexedString(languageIndex);
                                output = action.Output.GetIndexedString(languageIndex);
                                ls = helperInflection.Output.LanguageString(languageIndex);
                                ls.Text = LanguageLookup.ReplaceWord(ls.Text, input, output, ls.LanguageID);
                                ls = helperInflection.PronounOutput.LanguageString(languageIndex);
                                ls.Text = LanguageLookup.ReplaceWord(ls.Text, input, output, ls.LanguageID);
                                ls = helperInflection.RegularOutput.LanguageString(languageIndex);
                                ls.Text = LanguageLookup.ReplaceWord(ls.Text, input, output, ls.LanguageID);
                                ls = helperInflection.RegularPronounOutput.LanguageString(languageIndex);
                                ls.Text = LanguageLookup.ReplaceWord(ls.Text, input, output, ls.LanguageID);
                            }
                        }
                        break;
                    case "AppendToOutputs":
                        {
                            int languageCount = helperInflection.Output.Count();
                            int languageIndex;
                            LanguageString ls;
                            string output;
                            for (languageIndex = 0; languageIndex < languageCount; languageIndex++)
                            {
                                output = action.Output.GetIndexedString(languageIndex);
                                ls = helperInflection.Output.LanguageString(languageIndex);
                                ls.Text = LanguageLookup.AppendWord(ls.Text, output, ls.LanguageID);
                                ls = helperInflection.PronounOutput.LanguageString(languageIndex);
                                ls.Text = LanguageLookup.AppendWord(ls.Text, output, ls.LanguageID);
                                ls = helperInflection.RegularOutput.LanguageString(languageIndex);
                                ls.Text = LanguageLookup.AppendWord(ls.Text, output, ls.LanguageID);
                                ls = helperInflection.RegularPronounOutput.LanguageString(languageIndex);
                                ls.Text = LanguageLookup.AppendWord(ls.Text, output, ls.LanguageID);
                            }
                        }
                        break;
                    default:
                        throw new Exception("Unsupported helper action type: " + action.Type);
                }

                if (done)
                    break;
            }
        }

        public string FilterName
        {
            get
            {
                return _FilterName;
            }
            set
            {
                if (value != _FilterName)
                {
                    _FilterName = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            element.Add(new XAttribute("Label", Label));

            if (_CompoundDesignatorPartial != null)
            {
                XElement compoundDesignatorPartialElement =
                    _CompoundDesignatorPartial.GetElement("CompoundDesignatorPartial");
                element.Add(compoundDesignatorPartialElement);
            }

            if ((_Scope != null) && (_Scope != "Both"))
                element.Add(new XAttribute("Scope", _Scope));

            element.Add(new XElement("TargetDesignatorLabel", _TargetDesignatorLabel));

            if (!String.IsNullOrEmpty(_Helper))
                element.Add(new XElement("Helper", _Helper));

            if (_HelperSenseIndex != -1)
                element.Add(new XElement("HelperSenseIndex", _HelperSenseIndex));

            if (_HelperSynonymIndex != -1)
                element.Add(new XElement("HelperSynonymIndex", _HelperSynonymIndex));

            if (!String.IsNullOrEmpty(_HelperDestination))
                element.Add(new XElement("HelperDestination", _HelperDestination));

            if ((_HelperLanguageIndexMap != null) && (_HelperLanguageIndexMap.Count() != 0))
                element.Add(
                    new XElement(
                        "HelperLanguageIndexMap",
                        ObjectUtilities.GetStringFromIntList(_HelperLanguageIndexMap)));

            if (_ModifierFixups != null)
            {
                foreach (ModifierFixup modifierFixup in _ModifierFixups)
                {
                    XElement modifierFixupElement = modifierFixup.GetElement("ModifierFixup");
                    element.Add(modifierFixupElement);
                }
            }

            if (_HelperActions != null)
            {
                foreach (SpecialAction helperAction in _HelperActions)
                {
                    XElement helperActionElement = helperAction.GetElement("HelperAction");
                    element.Add(helperActionElement);
                }
            }

            if (_PostActions != null)
            {
                foreach (SpecialAction helperAction in _PostActions)
                {
                    XElement helperActionElement = helperAction.GetElement("PostAction");
                    element.Add(helperActionElement);
                }
            }

            if ((_DesignatorsTemplateNames != null) && (_DesignatorsTemplateNames.Count() != 0))
            {
                element.Add(
                    new XElement(
                        "DesignatorsTemplateNames",
                        ObjectUtilities.GetStringFromStringList(_DesignatorsTemplateNames)));
            }

            foreach (List<Designator> designatorList in _DesignatorLists)
            {
                XElement designatorsElement = new XElement("Designators");

                foreach (Designator designator in designatorList)
                {
                    XElement designatorElement = designator.GetElement("Designator");
                    designatorsElement.Add(designatorElement);
                }

                element.Add(designatorsElement);
            }

            if (_TargetModifiers != null)
            {
                foreach (Modifier targetModifier in _TargetModifiers)
                {
                    XElement targetModifierElement = targetModifier.GetElement("TargetModifier");
                    element.Add(targetModifierElement);
                }
            }

            if (_ContractionInflectors != null)
            {
                foreach (Inflector inflector in _ContractionInflectors)
                {
                    XElement contractionInflectorElement = inflector.GetElement("ContractionInflector");
                    element.Add(contractionInflectorElement);
                }
            }

            if (!String.IsNullOrEmpty(_FilterName))
                element.Add(new XElement("FilterName", _FilterName));

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Label":
                    Label = attributeValue;
                    break;
                case "Scope":
                    _Scope = attributeValue;
                    break;
                case "TargetDesignatorLabel":
                    _TargetDesignatorLabel = attributeValue;
                    break;
                case "Helper":
                    _Helper = attributeValue;
                    break;
                case "HelperSenseIndex":
                    _HelperSenseIndex = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "HelperSynonymIndex":
                    _HelperSynonymIndex = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                case "HelperDestination":
                    _HelperDestination = attributeValue;
                    break;
                case "FilterName":
                    _FilterName = attributeValue;
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
                case "Label":
                    Label = childElement.Value.Trim();
                    break;
                case "CompoundDesignatorPartial":
                    _CompoundDesignatorPartial = new Designator(childElement);
                    break;
                case "Scope":
                    _Scope = childElement.Value.Trim();
                    break;
                case "TargetDesignatorLabel":
                    _TargetDesignatorLabel = childElement.Value.Trim();
                    break;
                case "Helper":
                    _Helper = childElement.Value.Trim();
                    break;
                case "HelperSenseIndex":
                    _HelperSenseIndex = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), 0);
                    break;
                case "HelperSynonymIndex":
                    _HelperSynonymIndex = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), 0);
                    break;
                case "HelperDestination":
                    _HelperDestination = childElement.Value.Trim();
                    break;
                case "HelperLanguageIndexMap":
                    _HelperLanguageIndexMap = ObjectUtilities.GetIntListFromString(childElement.Value.Trim());
                    break;
                case "ModifierFixup":
                    if (_ModifierFixups == null)
                        _ModifierFixups = new List<ModifierFixup> { new ModifierFixup(childElement) };
                    else
                        _ModifierFixups.Add(new ModifierFixup(childElement));
                    break;
                case "HelperAction":
                    if (_HelperActions == null)
                        _HelperActions = new List<SpecialAction> { new SpecialAction(childElement) };
                    else
                        _HelperActions.Add(new SpecialAction(childElement));
                    break;
                case "PostAction":
                    if (_PostActions == null)
                        _PostActions = new List<SpecialAction> { new SpecialAction(childElement) };
                    else
                        _PostActions.Add(new SpecialAction(childElement));
                    break;
                case "DesignatorsTemplateNames":
                    _DesignatorsTemplateNames = ObjectUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "Designators":
                    {
                        List<Designator> designatorList = new List<Designator>();
                        foreach (XElement subElement in childElement.Elements())
                        {
                            switch (subElement.Name.LocalName)
                            {
                                case "Designator":
                                    {
                                        Designator designator = new Designator(subElement);
                                        designatorList.Add(designator);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        _DesignatorLists.Add(designatorList);
                    }
                    break;
                case "TargetModifier":
                    if (_TargetModifiers == null)
                        _TargetModifiers = new List<Modifier> { new Modifier(childElement) };
                    else
                        _TargetModifiers.Add(new Modifier(childElement));
                    break;
                case "ContractionInflector":
                    if (_ContractionInflectors == null)
                        _ContractionInflectors = new List<Inflector> { new Inflector(childElement) };
                    else
                        _ContractionInflectors.Add(new Inflector(childElement));
                    break;
                case "FilterName":
                    _FilterName = childElement.Value.Trim();
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
