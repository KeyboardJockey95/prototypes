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
    public class Inflector : Designator
    {
        protected string _Scope;
        protected string _TriggerLabel;
        protected List<LiteralString> _Pronouns;
        protected List<LiteralString> _PostPronouns;
        protected List<Modifier> _Modifiers;
        protected Inflector _PreInflector;
        protected Inflector _PostInflector;
        protected string _FilterName;
        protected List<SpecialAction> _PostActions;

        public Inflector(
                string label,
                List<Classifier> classifications,
                string scope,
                List<LiteralString> pronouns,
                List<Modifier> modifiers) :
            base(label, classifications)
        {
            _Scope = scope;
            _TriggerLabel = null;
            _Pronouns = pronouns;
            _Modifiers = modifiers;
            _PreInflector = null;
            _PostInflector = null;
            _FilterName = null;
            _PostActions = null;
        }

        public Inflector(
                string label,
                string[] classificationPairs,
                string scope,
                List<LiteralString> pronouns,
                List<Modifier> modifiers) :
            base(label, classificationPairs)
        {
            _Scope = scope;
            _TriggerLabel = null;
            _Pronouns = pronouns;
            _Modifiers = modifiers;
            _PreInflector = null;
            _PostInflector = null;
            _FilterName = null;
            _PostActions = null;
        }

        public Inflector(
                string label,
                List<string> classificationPairs,
                string scope,
                List<LiteralString> pronouns,
                List<Modifier> modifiers) :
            base(label, classificationPairs)
        {
            _Scope = scope;
            _TriggerLabel = null;
            _Pronouns = pronouns;
            _Modifiers = modifiers;
            _PreInflector = null;
            _PostInflector = null;
            _FilterName = null;
            _PostActions = null;
        }

        public Inflector(
                string label,
                Classifier singleClassification,
                string scope,
                List<LiteralString> pronouns,
                List<Modifier> modifiers) :
            base(label, singleClassification)
        {
            _Scope = scope;
            _TriggerLabel = null;
            _Pronouns = pronouns;
            _Modifiers = modifiers;
            _PreInflector = null;
            _PostInflector = null;
            _FilterName = null;
            _PostActions = null;
        }

        public Inflector(
                string label,
                Classifier doubleClassification1,
                Classifier doubleClassification2,
                string scope,
                List<LiteralString> pronouns,
                List<Modifier> modifiers) :
            base(label, doubleClassification1, doubleClassification2)
        {
            _Scope = scope;
            _TriggerLabel = null;
            _Pronouns = pronouns;
            _Modifiers = modifiers;
            _PreInflector = null;
            _PostInflector = null;
            _FilterName = null;
            _PostActions = null;
        }

        public Inflector(
                string label,
                Classifier tripleClassification1,
                Classifier tripleClassification2,
                Classifier tripleClassification3,
                string scope,
                List<LiteralString> pronouns,
                List<Modifier> modifiers) :
            base(label, tripleClassification1, tripleClassification2, tripleClassification3)
        {
            _Scope = scope;
            _TriggerLabel = null;
            _Pronouns = pronouns;
            _Modifiers = modifiers;
            _PreInflector = null;
            _PostInflector = null;
            _FilterName = null;
            _PostActions = null;
        }

        public Inflector(
                string keyLabel,
                string value,
                string scope,
                List<LiteralString> pronouns,
                List<Modifier> modifiers) :
            base(keyLabel, value)
        {
            _Scope = scope;
            _TriggerLabel = null;
            _Pronouns = pronouns;
            _Modifiers = modifiers;
            _PreInflector = null;
            _PostInflector = null;
            _FilterName = null;
            _PostActions = null;
        }

        // Merge match inflectors.
        public Inflector(
                Designator designator,
                List<Inflector> others)
            : base(designator)
        {
            ClearInflector();

            _Pronouns = new List<LiteralString>();

            foreach (Inflector other in others)
                ObjectUtilities.ListAddUniqueList<LiteralString>(_Pronouns, other.Pronouns);

            _PostPronouns = new List<LiteralString>();

            foreach (Inflector other in others)
                ObjectUtilities.ListAddUniqueList<LiteralString>(_PostPronouns, other.PostPronouns);

            Inflector inflector = others.First();

            if (inflector.Modifiers != null)
                _Modifiers = new List<Modifier>(inflector.Modifiers);
            else
                _Modifiers = null;

            _PreInflector = null;
            _PostInflector = null;
            _FilterName = null;
            _PostActions = null;
        }

        public Inflector(
                Inflector other,
                Designator designatorPrepend,
                Designator designatorAppend1,
                Designator designatorAppend2) :
            base(other)
        {
            CopyInflector(other);

            if (designatorPrepend != null)
                InsertClassifications(designatorPrepend);

            if (designatorAppend1 != null)
                AppendClassifications(designatorAppend1);

            if (designatorAppend2 != null)
                AppendClassifications(designatorAppend2);

            DefaultLabel();
            TouchAndClearModified();
        }

        public Inflector(Inflector other) :
            base(other)
        {
            CopyInflector(other);
        }

        public Inflector(XElement element)
        {
            OnElement(element);
            DefaultLabelCheck();
        }

        public Inflector()
        {
            ClearInflector();
        }

        public void ClearInflector()
        {
            _Scope = null;
            _TriggerLabel = null;
            _Pronouns = null;
            _PostPronouns = null;
            _Modifiers = null;
            _PreInflector = null;
            _PostInflector = null;
            _FilterName = null;
            _PostActions = null;
        }

        public void CopyInflector(Inflector other)
        {
            _Scope = other.Scope;
            _TriggerLabel = other.TriggerLabel;
            _Pronouns = other.ClonePronouns();
            _PostPronouns = other.ClonePostPronouns();
            _Modifiers = other.CloneModifiers();
            _PreInflector = ClonePreInflector();
            _PostInflector = ClonePostInflector();
            _FilterName = null;
            _PostActions = ClonePostActions();
        }

        public Designator Designator
        {
            get
            {
                return new Designator(this);
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

        public string TriggerLabel
        {
            get
            {
                return _TriggerLabel;
            }
            set
            {
                if (value != _TriggerLabel)
                {
                    _TriggerLabel = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasTrigger()
        {
            if (!String.IsNullOrEmpty(_TriggerLabel))
                return true;
            return false;
        }

        public List<Modifier> Modifiers
        {
            get
            {
                return _Modifiers;
            }
            set
            {
                if (value != _Modifiers)
                {
                    _Modifiers = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<Modifier> CloneModifiers()
        {
            if (_Modifiers == null)
                return null;

            List<Modifier> newModifiers = new List<Modifier>(_Modifiers.Count());

            foreach (Modifier modifier in _Modifiers)
                newModifiers.Add(new Modifier(modifier));

            return newModifiers;
        }

        public int ModifierCount()
        {
            if (_Modifiers == null)
                return 0;
            return _Modifiers.Count();
        }

        public bool HasModifier(string classType, string subClass)
        {
            Modifier modifier = null;

            if ((_Modifiers == null) || (_Modifiers.Count() == 0))
                return false;

            modifier = _Modifiers.FirstOrDefault(x => x.MatchClassAndSubClass(classType, subClass));

            if (modifier == null)
                return false;

            return true;
        }

        public bool HasModifier(string classType)
        {
            Modifier modifier = null;

            if ((_Modifiers == null) || (_Modifiers.Count() == 0))
                return false;

            modifier = _Modifiers.FirstOrDefault(x => x.MatchClass(classType));

            if (modifier == null)
                return false;

            return true;
        }

        public bool HasModifier(List<string> classType, List<string> subClass)
        {
            Modifier modifier = null;

            if ((_Modifiers == null) || (_Modifiers.Count() == 0))
                return false;

            modifier = _Modifiers.FirstOrDefault(x => x.MatchClassAndSubClass(classType, subClass));

            if (modifier == null)
                return false;

            return true;
        }

        public bool HasModifier(List<string> classType)
        {
            Modifier modifier = null;

            if ((_Modifiers == null) || (_Modifiers.Count() == 0))
                return false;

            modifier = _Modifiers.FirstOrDefault(x => x.MatchClass(classType));

            if (modifier == null)
                return false;

            return true;
        }

        public Modifier GetModifier(string classType, string subClass)
        {
            Modifier modifier = null;

            if ((_Modifiers == null) || (_Modifiers.Count() == 0))
                return modifier;

            modifier = _Modifiers.FirstOrDefault(x => x.MatchClassAndSubClass(classType, subClass));

            return modifier;
        }

        public Modifier GetModifier(string classType)
        {
            Modifier modifier = null;

            if ((_Modifiers == null) || (_Modifiers.Count() == 0))
                return modifier;

            modifier = _Modifiers.FirstOrDefault(x => x.MatchClass(classType));

            return modifier;
        }

        public Modifier GetModifier(List<string> classType, List<string> subClass)
        {
            Modifier modifier = null;

            if ((_Modifiers == null) || (_Modifiers.Count() == 0))
                return modifier;

            modifier = _Modifiers.FirstOrDefault(x => x.MatchClassAndSubClass(classType, subClass));

            return modifier;
        }

        public Modifier GetModifier(List<string> classType)
        {
            Modifier modifier = null;

            if ((_Modifiers == null) || (_Modifiers.Count() == 0))
                return modifier;

            modifier = _Modifiers.FirstOrDefault(x => x.MatchClass(classType));

            return modifier;
        }

        public Modifier GetModifierIndexed(int index)
        {
            if ((_Modifiers == null) || (index < 0) || (index >= _Modifiers.Count()))
                return null;

            Modifier modifier = _Modifiers[index];

            return modifier;
        }

        public void AppendModifier(Modifier modifier)
        {
            if (_Modifiers != null)
                _Modifiers.Add(modifier);
            else
                _Modifiers = new List<Modifier>() { modifier };
            ModifiedFlag = true;
        }

        public bool InsertModifier(int index, Modifier modifier)
        {
            if (_Modifiers != null)
            {
                if ((index >= 0) && (index <= _Modifiers.Count()))
                    _Modifiers.Insert(index, modifier);
                else
                    return false;
            }
            else if (index == 0)
                _Modifiers = new List<Modifier>() { modifier };
            else
                return false;

            ModifiedFlag = true;
            return true;
        }

        public bool DeleteModifier(Modifier modifier)
        {
            if ((modifier == null) || (_Modifiers == null))
                return false;

            bool returnValue = _Modifiers.Remove(modifier);

            if (returnValue)
                ModifiedFlag = true;

            return returnValue;
        }

        public bool DeleteModifierIndex(int index)
        {
            if ((_Modifiers == null) || (index < 0) || (index >= _Modifiers.Count()))
                return false;

            _Modifiers.RemoveAt(index);
            ModifiedFlag = true;

            return true;
        }

        public void SetDefaultCategory(LexicalCategory category)
        {
            if (_Modifiers == null)
                return;

            foreach (Modifier modifier in _Modifiers)
            {
                if (modifier.Category == LexicalCategory.Unknown)
                    modifier.Category = category;
            }
        }

        public List<LiteralString> Pronouns
        {
            get
            {
                return _Pronouns;
            }
            set
            {
                if (value != _Pronouns)
                {
                    _Pronouns = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<LiteralString> ClonePronouns()
        {
            if (_Pronouns == null)
                return null;

            List<LiteralString> newPronouns = new List<LiteralString>(_Pronouns.Count());

            foreach (LiteralString pronoun in _Pronouns)
                newPronouns.Add(new LiteralString(pronoun));

            return newPronouns;
        }

        public int PronounCount()
        {
            if (_Pronouns == null)
                return 0;

            return _Pronouns.Count();
        }

        public bool HasAnyPronouns()
        {
            if (_Pronouns == null)
                return false;

            return (_Pronouns.Count() != 0 ? true : false);
        }

        public bool HasPronoun(string pronoun)
        {
            if (String.IsNullOrEmpty(pronoun) || (_Pronouns == null))
                return false;

            if (_Pronouns.FirstOrDefault(x => x.Contains(pronoun)) != null)
                return false;

            return true;
        }

        public bool HasPronoun(LiteralString pronoun)
        {
            if (String.IsNullOrEmpty(pronoun) || (_Pronouns == null))
                return false;

            if (!_Pronouns.Contains(pronoun))
                return false;

            return true;
        }

        public LiteralString GetPronounIndexed(int index)
        {
            if ((_Pronouns == null) || (index < 0) || (index >= _Pronouns.Count()))
                return null;

            LiteralString pronoun = _Pronouns[index];

            return pronoun;
        }

        public void AppendPronoun(LiteralString pronoun)
        {
            if (_Pronouns != null)
                _Pronouns.Add(pronoun);
            else
                _Pronouns = new List<LiteralString>() { pronoun };
            ModifiedFlag = true;
        }

        public bool InsertPronoun(int index, LiteralString pronoun)
        {
            if (_Pronouns != null)
            {
                if ((index >= 0) && (index <= _Pronouns.Count()))
                    _Pronouns.Insert(index, pronoun);
                else
                    return false;
            }
            else if (index == 0)
                _Pronouns = new List<LiteralString>() { pronoun };
            else
                return false;

            ModifiedFlag = true;
            return true;
        }

        public bool DeletePronoun(LiteralString pronoun)
        {
            if ((pronoun == null) || (_Pronouns == null))
                return false;

            bool returnValue = _Pronouns.Remove(pronoun);

            if (returnValue)
                ModifiedFlag = true;

            return returnValue;
        }

        public bool DeletePronounIndexed(int index)
        {
            if ((_Pronouns == null) || (index < 0) || (index >= _Pronouns.Count()))
                return false;

            _Pronouns.RemoveAt(index);
            ModifiedFlag = true;

            return true;
        }

        public List<LiteralString> PostPronouns
        {
            get
            {
                return _PostPronouns;
            }
            set
            {
                if (value != _PostPronouns)
                {
                    _PostPronouns = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<LiteralString> ClonePostPronouns()
        {
            if (_PostPronouns == null)
                return null;

            List<LiteralString> newPostPronouns = new List<LiteralString>(_PostPronouns.Count());

            foreach (LiteralString pronoun in _PostPronouns)
                newPostPronouns.Add(new LiteralString(pronoun));

            return newPostPronouns;
        }

        public int PostPronounCount()
        {
            if (_PostPronouns == null)
                return 0;

            return _PostPronouns.Count();
        }

        public bool HasAnyPostPronouns()
        {
            if (_PostPronouns == null)
                return false;

            return (_PostPronouns.Count() != 0 ? true : false);
        }

        public bool HasPostPronoun(string pronoun)
        {
            if (String.IsNullOrEmpty(pronoun) || (_PostPronouns == null))
                return false;

            if (_PostPronouns.FirstOrDefault(x => x.Contains(pronoun)) != null)
                return false;

            return true;
        }

        public LiteralString GetPostPronounIndexed(int index)
        {
            if ((_PostPronouns == null) || (index < 0) || (index >= _PostPronouns.Count()))
                return null;

            LiteralString pronoun = _PostPronouns[index];

            return pronoun;
        }

        public void AppendPostPronoun(LiteralString pronoun)
        {
            if (_PostPronouns != null)
                _PostPronouns.Add(pronoun);
            else
                _PostPronouns = new List<LiteralString>() { pronoun };
            ModifiedFlag = true;
        }

        public bool InsertPostPronoun(int index, LiteralString pronoun)
        {
            if (_PostPronouns != null)
            {
                if ((index >= 0) && (index <= _PostPronouns.Count()))
                    _PostPronouns.Insert(index, pronoun);
                else
                    return false;
            }
            else if (index == 0)
                _PostPronouns = new List<LiteralString>() { pronoun };
            else
                return false;

            ModifiedFlag = true;
            return true;
        }

        public bool DeletePostPronoun(LiteralString pronoun)
        {
            if ((pronoun == null) || (_PostPronouns == null))
                return false;

            bool returnValue = _PostPronouns.Remove(pronoun);

            if (returnValue)
                ModifiedFlag = true;

            return returnValue;
        }

        public bool DeletePostPronounIndexed(int index)
        {
            if ((_PostPronouns == null) || (index < 0) || (index >= _PostPronouns.Count()))
                return false;

            _PostPronouns.RemoveAt(index);
            ModifiedFlag = true;

            return true;
        }

        public Inflector PreInflector
        {
            get
            {
                return _PreInflector;
            }
            set
            {
                if (_PreInflector != value)
                {
                    _PreInflector = value;
                    ModifiedFlag = true;
                }
            }
        }

        public Inflector ClonePreInflector()
        {
            if (_PreInflector == null)
                return null;

            return new Inflector(_PreInflector);
        }

        public void InsertPreInflector(Inflector preInflector)
        {
            preInflector.PreInflector = _PreInflector;
            _PreInflector = preInflector;
        }

        public void AppendPreInflector(Inflector preInflector)
        {
            if (_PreInflector == null)
            {
                _PreInflector = preInflector;
                return;
            }

            Inflector inflector = _PreInflector;

            while (inflector.PreInflector != null)
                inflector = inflector.PreInflector;

            inflector.PreInflector = preInflector;
        }

        public Inflector PostInflector
        {
            get
            {
                return _PostInflector;
            }
            set
            {
                if (_PostInflector != value)
                {
                    _PostInflector = value;
                    ModifiedFlag = true;
                }
            }
        }

        public Inflector ClonePostInflector()
        {
            if (_PostInflector == null)
                return null;

            return new Inflector(_PostInflector);
        }

        public void InsertPostInflector(Inflector postInflector)
        {
            postInflector.PostInflector = _PostInflector;
            _PostInflector = postInflector;
        }

        public void AppendPostInflector(Inflector postInflector)
        {
            if (_PostInflector == null)
            {
                _PostInflector = postInflector;
                return;
            }

            Inflector inflector = _PostInflector;

            while (inflector.PostInflector != null)
                inflector = inflector.PostInflector;

            inflector.PostInflector = postInflector;
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

        public List<SpecialAction> ClonePostActions()
        {
            if (_PostActions == null)
                return null;

            List<SpecialAction> newPostActions = new List<SpecialAction>(_PostActions.Count());

            foreach (SpecialAction action in _PostActions)
                newPostActions.Add(new SpecialAction(action));

            return newPostActions;
        }

        public int PostActionCount()
        {
            if (_PostActions == null)
                return 0;

            return _PostActions.Count();
        }

        public bool HasAnyPostActions()
        {
            if (_PostActions == null)
                return false;

            return (_PostActions.Count() != 0 ? true : false);
        }

        public SpecialAction GetPostActionIndexed(int index)
        {
            if ((_PostActions == null) || (index < 0) || (index >= _PostActions.Count()))
                return null;

            SpecialAction action = _PostActions[index];

            return action;
        }

        public void AppendPostAction(SpecialAction action)
        {
            if (_PostActions != null)
                _PostActions.Add(action);
            else
                _PostActions = new List<SpecialAction>() { action };
            ModifiedFlag = true;
        }

        public bool InsertPostAction(int index, SpecialAction action)
        {
            if (_PostActions != null)
            {
                if ((index >= 0) && (index <= _PostActions.Count()))
                    _PostActions.Insert(index, action);
                else
                    return false;
            }
            else if (index == 0)
                _PostActions = new List<SpecialAction>() { action };
            else
                return false;

            ModifiedFlag = true;
            return true;
        }

        public bool DeletePostAction(SpecialAction action)
        {
            if ((action == null) || (_PostActions == null))
                return false;

            bool returnValue = _PostActions.Remove(action);

            if (returnValue)
                ModifiedFlag = true;

            return returnValue;
        }

        public bool DeletePostActionIndexed(int index)
        {
            if ((_PostActions == null) || (index < 0) || (index >= _PostActions.Count()))
                return false;

            _PostActions.RemoveAt(index);
            ModifiedFlag = true;

            return true;
        }

        public bool Inflect(
            DictionaryEntry dictionaryEntry,
            int senseIndex,
            int synonymIndex,
            List<LanguageID> targetLanguageIDs,
            Designator designator,
            InflectorTable inflectorTable,
            LanguageTool languageTool,
            out Inflection inflection)
        {
            return languageTool.InflectTargetOrHostFiltered(
                dictionaryEntry,
                senseIndex,
                synonymIndex,
                this,
                designator,
                inflectorTable,
                null,
                out inflection);
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
            XElement element;

            if (name == "Stem")
            {
                element = new XElement(name);

                if (!String.IsNullOrEmpty(KeyString))
                    element.Add(new XAttribute("Name", KeyString));
            }
            else
            {
                element = base.GetElement(name);

                if (!String.IsNullOrEmpty(_Scope) && (_Scope != "All"))
                    element.Add(new XAttribute("Scope", _Scope));

                if (_Pronouns != null)
                {
                    foreach (LiteralString pronoun in _Pronouns)
                    {
                        XElement childElement = new XElement("Pronoun");
                        childElement.Add(new XAttribute("Value", pronoun.StringListString));
                        element.Add(childElement);
                    }
                }

                if (_PostPronouns != null)
                {
                    foreach (LiteralString postPronoun in _PostPronouns)
                    {
                        XElement childElement = new XElement("PostPronoun");
                        childElement.Add(new XAttribute("Value", postPronoun.StringListString));
                        element.Add(childElement);
                    }
                }
            }

            if (!String.IsNullOrEmpty(_TriggerLabel))
                element.Add(new XAttribute("TriggerLabel", _TriggerLabel));

            if (_Modifiers != null)
            {
                foreach (Modifier modifier in _Modifiers)
                {
                    XElement childElement = modifier.Xml;
                    element.Add(childElement);
                }
            }

            if (_PreInflector != null)
            {
                XElement preInflectorElement = _PreInflector.GetElement("PreInflector");
                element.Add(preInflectorElement);
            }

            if (_PostInflector != null)
            {
                XElement postInflectorElement = _PreInflector.GetElement("PostInflector");
                element.Add(postInflectorElement);
            }

            if (!String.IsNullOrEmpty(_FilterName))
                element.Add(new XElement("FilterName", _FilterName));

            if (_PostActions != null)
            {
                foreach (SpecialAction action in _PostActions)
                {
                    XElement childElement = action.GetElement("PostAction");
                    element.Add(childElement);
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
                case "Scope":
                    Scope = attributeValue;
                    break;
                case "TriggerLabel":
                    _TriggerLabel = attributeValue;
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
                case "Pronoun":
                    {
                        XAttribute valueAttribute = childElement.Attribute("Value");
                        if (valueAttribute != null)
                            AppendPronoun(new LiteralString(valueAttribute.Value.Trim()));
                        else
                            throw new Exception("Pronoun element missing Value attribute.");
                    }
                    break;
                case "PostPronoun":
                    {
                        XAttribute valueAttribute = childElement.Attribute("Value");
                        if (valueAttribute != null)
                            AppendPostPronoun(new LiteralString(valueAttribute.Value.Trim()));
                        else
                            throw new Exception("PostPronoun element missing Value attribute.");
                    }
                    break;
                case "Modifier":
                    {
                        Modifier modifier = new Modifier(childElement);
                        AppendModifier(modifier);
                    }
                    break;
                case "PreInflector":
                    {
                        Inflector inflector = new Inflector(childElement);
                        _PreInflector = inflector;
                    }
                    break;
                case "PostInflector":
                    {
                        Inflector inflector = new Inflector(childElement);
                        _PostInflector = inflector;
                    }
                    break;
                case "FilterName":
                    _FilterName = childElement.Value.Trim();
                    break;
                case "PostAction":
                    {
                        SpecialAction action = new SpecialAction(childElement);
                        AppendPostAction(action);
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
