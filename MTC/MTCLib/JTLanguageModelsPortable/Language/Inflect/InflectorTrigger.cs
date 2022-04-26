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
    public class InflectorTrigger : BaseObjectKeyed
    {
        protected List<LiteralString> _Targets;
        protected List<Classifier> _Conditions;

        public InflectorTrigger(
                string label,
                List<LiteralString> targets,
                List<SpecialAction> actions,
                bool post,
                bool noPost) :
            base(label)
        {
            _Targets = targets;
            _Conditions = null;
        }

        public InflectorTrigger(
                string label,
                List<Classifier> conditions) :
            base(label)
        {
            _Targets = null;
            _Conditions = conditions;
        }

        public InflectorTrigger(InflectorTrigger other) : base(other)
        {
            CopyInflectorTrigger(other);
        }

        public InflectorTrigger(XElement element)
        {
            ClearInflectorTrigger();
            OnElement(element);
        }

        public InflectorTrigger()
        {
            ClearInflectorTrigger();
        }

        public void ClearInflectorTrigger()
        {
            _Targets = null;
            _Conditions = null;
        }

        public void CopyInflectorTrigger(InflectorTrigger other)
        {
            _Targets = other.CloneTargets();
            _Conditions = other.CloneConditions();
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

        public bool MatchTrigger(DictionaryEntry dictionaryEntry)
        {
            bool match = false;

            if (MatchTrigger(dictionaryEntry.KeyString))
                match = true;

            if (dictionaryEntry.HasAlternates())
            {
                foreach (LanguageString alternate in dictionaryEntry.Alternates)
                {
                    if (MatchTrigger(alternate.Text))
                        match = true;
                }
            }

            return match;
        }

        public bool MatchTrigger(string dictionaryForm)
        {
            bool match = false;

            if (HasTargets())
                match |= MatchTarget(dictionaryForm);

            if (HasConditions())
                match |= match && MatchCondition(dictionaryForm);

            return match;
        }

        public List<LiteralString> Targets
        {
            get
            {
                return _Targets;
            }
            set
            {
                if (value != _Targets)
                {
                    _Targets = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<LiteralString> CloneTargets()
        {
            if (_Targets == null)
                return null;

            List<LiteralString> newTargets = new List<LiteralString>(_Targets.Count());

            foreach (LiteralString target in _Targets)
                newTargets.Add(new LiteralString(target));

            return newTargets;
        }

        public bool HasTargets()
        {
            if ((_Targets != null) && (_Targets.Count() != 0))
                return true;
            return false;
        }

        public bool MatchTarget(MultiLanguageString dictionaryForm)
        {
            if (_Targets == null)
                return false;

            if (dictionaryForm.LanguageStrings == null)
                return false;

            foreach (LanguageString languageString in dictionaryForm.LanguageStrings)
            {
                string text = languageString.Text;

                if (MatchTarget(text))
                    return true;
            }

            return false;
        }

        public bool MatchTarget(string dictionaryForm)
        {
            if (_Targets == null)
                return false;

            foreach (LiteralString target in _Targets)
            {
                if (target.Contains(dictionaryForm))
                    return true;
            }

            return false;
        }

        public List<Classifier> Conditions
        {
            get
            {
                return _Conditions;
            }
            set
            {
                if (value != _Conditions)
                {
                    _Conditions = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<Classifier> CloneConditions()
        {
            if (_Conditions == null)
                return null;

            List<Classifier> newConditions = new List<Classifier>(_Conditions.Count());

            foreach (Classifier condition in _Conditions)
                newConditions.Add(new Classifier(condition));

            return newConditions;
        }

        public bool HasConditions()
        {
            if ((_Conditions != null) && (_Conditions.Count() != 0))
                return true;
            return false;
        }

        public bool MatchCondition(MultiLanguageString dictionaryForm)
        {
            if (_Conditions == null)
                return true;

            if (dictionaryForm.LanguageStrings == null)
                return false;

            foreach (LanguageString languageString in dictionaryForm.LanguageStrings)
            {
                string text = languageString.Text;

                if (MatchCondition(text))
                    return true;
            }

            return false;
        }

        public bool MatchCondition(string dictionaryForm)
        {
            if (_Conditions == null)
                return true;

            int conditionCount = 0;
            int matchedConditions = 0;

            foreach (Classifier condition in _Conditions)
            {
                switch (condition.KeyString)
                {
                    case "StartsWith":
                        if (dictionaryForm.StartsWith(condition.Text))
                            matchedConditions++;
                        break;
                    case "Matches":
                        if (dictionaryForm == condition.Text)
                            matchedConditions++;
                        break;
                    case "EndsWith":
                        if (dictionaryForm.EndsWith(condition.Text))
                            matchedConditions++;
                        break;
                    default:
                        throw new Exception("Unknown condition: " + condition.KeyString);
                }

                conditionCount++;
            }

            if (matchedConditions == conditionCount)
                return true;

            return false;
        }

        public int ConditionsLength
        {
            get
            {
                int length = 0;

                if (_Conditions != null)
                {
                    foreach (Classifier condition in _Conditions)
                        length += condition.TextLength;
                }

                return length;
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if (!String.IsNullOrEmpty(Label))
                element.Add(new XAttribute("Label", Label));

            if ((_Targets != null) && (_Targets.Count() != 0))
            {
                if (_Targets.First().Count() == 1)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (LiteralString target in _Targets)
                    {
                        if (sb.Length != 0)
                            sb.Append(",");

                        sb.Append(target.GetIndexedString(0));
                    }

                    element.Add(new XElement("Targets", sb.ToString()));
                }
                else
                {
                    foreach (LiteralString target in _Targets)
                        element.Add(new XElement("Target", target.StringListString));
                }
            }

            if (_Conditions != null)
            {
                foreach (Classifier condition in _Conditions)
                {
                    XElement childElement = condition.GetElement("Condition");
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
                case "Label":
                    Label = attributeValue;
                    break;
                default:
                    return true;
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
                case "Targets":
                    {
                        _Targets = new List<LiteralString>();
                        List<string> targets = ObjectUtilities.GetStringListFromString(
                            childElement.Value.Trim());
                        foreach (string target in targets)
                            _Targets.Add(new LiteralString(target));
                    }
                    break;
                case "Target":
                    {
                        LiteralString target = new LiteralString(childElement.Value.Trim());
                        if (_Targets != null)
                            _Targets.Add(target);
                        else
                            _Targets = new List<LiteralString>() { target };
                    }
                    break;
                case "Condition":
                    {
                        Classifier condition = new Classifier(childElement);
                        if (_Conditions != null)
                            _Conditions.Add(condition);
                        else
                            _Conditions = new List<Classifier>() { condition };
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
