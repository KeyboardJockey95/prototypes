using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public class SemiRegular : BaseObjectKeyed
    {
        protected List<LiteralString> _Targets;
        protected HashSet<string> _TargetsHashSet;
        protected List<LiteralString> _DictionaryTargets;
        protected List<Classifier> _Conditions;
        protected List<SpecialAction> _Actions;
        protected bool _Post;
        protected bool _NoPost;
        protected string _Instance;

        public SemiRegular(
                LiteralString target,
                SpecialAction action,
                bool post,
                bool noPost,
                string instance) :
            base(target.GetIndexedString(0))
        {
            _Targets = new List<LiteralString>() { target };
            _TargetsHashSet = null;
            _DictionaryTargets = null;
            _Conditions = null;
            _Actions = new List<SpecialAction>() { action };
            _Post = post;
            _NoPost = noPost;
            _Instance = instance;
        }

        public SemiRegular(
                string label,
                List<LiteralString> targets,
                List<SpecialAction> actions,
                bool post,
                bool noPost,
                string instance) :
            base(label)
        {
            _Targets = targets;
            _TargetsHashSet = null;
            _DictionaryTargets = null;
            _Conditions = null;
            _Actions = actions;
            _Post = post;
            _NoPost = noPost;
            _Instance = instance;
        }

        public SemiRegular(
                string label,
                List<Classifier> conditions,
                List<SpecialAction> actions,
                bool post,
                bool noPost,
                string instance) :
            base(label)
        {
            _Targets = null;
            _TargetsHashSet = null;
            _DictionaryTargets = null;
            _Conditions = conditions;
            _Actions = actions;
            _Post = post;
            _NoPost = noPost;
            _Instance = instance;
        }

        public SemiRegular(
                string label,
                Classifier condition,
                List<SpecialAction> actions,
                bool post,
                bool noPost,
                string instance) :
            base(label)
        {
            _Targets = null;
            _TargetsHashSet = null;
            _DictionaryTargets = null;
            _Conditions = new List<Classifier>(1) { condition };
            _Actions = actions;
            _Post = post;
            _NoPost = noPost;
            _Instance = instance;
        }

        public SemiRegular(SemiRegular other) : base(other)
        {
            CopySemiRegular(other);
        }

        public SemiRegular(XElement element)
        {
            ClearSemiRegular();
            OnElement(element);
        }

        public SemiRegular()
        {
            ClearSemiRegular();
        }

        public void ClearSemiRegular()
        {
            _Targets = null;
            _TargetsHashSet = null;
            _DictionaryTargets = null;
            _Conditions = null;
            _Actions = null;
            _Post = false;
            _NoPost = false;
            _Instance = null;
        }

        public void CopySemiRegular(SemiRegular other)
        {
            _Targets = other.Targets;
            _TargetsHashSet = null;
            _DictionaryTargets = other.DictionaryTargets;
            _Conditions = other.Conditions;
            _Actions = other.Actions;
            _Post = other.Post;
            _NoPost = other.NoPost;
            _Instance = other.Instance;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrEmpty(Label))
                sb.Append("Label=" + Label);

            if (!String.IsNullOrEmpty(_Instance))
                sb.Append("Instance=" + _Instance);

            if ((Targets != null) && (Targets.Count() != 0))
            {
                if (sb.Length != 0)
                    sb.Append(" ");

                sb.Append("Targets=");
                int count = 0;

                foreach (LiteralString target in Targets)
                {
                    if (count != 0)
                        sb.Append(",");

                    sb.Append(target.StringListString);
                    count++;
                }
            }

            if ((DictionaryTargets != null) && (DictionaryTargets.Count() != 0))
            {
                if (sb.Length != 0)
                    sb.Append(" ");

                sb.Append("DictionaryTargets=");
                int count = 0;

                foreach (LiteralString dictionaryTarget in DictionaryTargets)
                {
                    if (count != 0)
                        sb.Append(",");

                    sb.Append(dictionaryTarget.StringListString);
                    count++;
                }
            }

            if ((Conditions != null) && (Conditions.Count() != 0))
            {
                if (sb.Length != 0)
                    sb.Append(" ");

                sb.Append("Conditions=");
                int count = 0;

                foreach (Classifier condition in Conditions)
                {
                    if (count != 0)
                        sb.Append(",");

                    sb.Append(condition.ToString());
                    count++;
                }
            }

            sb.Append("Post=" + Post.ToString());
            sb.Append("NoPost=" + NoPost.ToString());

            if ((Actions != null) && (Actions.Count() != 0))
            {
                if (sb.Length != 0)
                    sb.Append(" ");

                sb.AppendLine("  Actions=");
                int count = 0;

                foreach (SpecialAction actions in Actions)
                {
                    if (count != 0)
                        sb.Append(",");

                    sb.Append(actions.ToString());
                    count++;
                }
            }

            return sb.ToString();
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

        public string Instance
        {
            get
            {
                return _Instance;
            }
            set
            {
                _Instance = value;
            }
        }

        public bool MatchInstance(string instance)
        {
            if (String.IsNullOrEmpty(_Instance) || String.IsNullOrEmpty(instance))
                return true;

            if (instance == _Instance)
                return true;

            return false;
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

        public bool HasTargets()
        {
            if ((_Targets != null) && (_Targets.Count() != 0))
                return true;
            return false;
        }

        public int TargetCount()
        {
            if (_Targets == null)
                return 0;

            return _Targets.Count();
        }

        public bool MatchTarget(MultiLanguageString dictionaryForm)
        {
            if (_Targets == null)
                return false;

            List<LanguageString> languageStrings = dictionaryForm.LanguageStrings;

            if (languageStrings == null)
                return false;

            foreach (LanguageString languageString in languageStrings)
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

#if true
            if (_TargetsHashSet == null)
                PrimeTargetsHashSet();

            if (_TargetsHashSet.Contains(dictionaryForm))
                return true;
#else
            foreach (LiteralString target in _Targets)
            {
                if (target.Contains(dictionaryForm))
                    return true;
            }
#endif

            return false;
        }

        protected virtual void PrimeTargetsHashSet()
        {
            _TargetsHashSet = new HashSet<string>();

            foreach (LiteralString target in _Targets)
            {
                foreach (string str in target.Strings)
                    _TargetsHashSet.Add(str);
            }

        }

        public void AddTarget(LiteralString target)
        {
            if (_Targets == null)
                _Targets = new List<LiteralString>() { target };
            else if (!_Targets.Contains(target))
                _Targets.Add(target);
        }

        public void AddTargets(List<LiteralString> targets)
        {
            if (targets == null)
                return;

            foreach (LiteralString target in targets)
                AddTarget(target);
        }

        public List<LiteralString> DictionaryTargets
        {
            get
            {
                return _DictionaryTargets;
            }
            set
            {
                if (value != _DictionaryTargets)
                {
                    _DictionaryTargets = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasDictionaryTargets()
        {
            if ((_DictionaryTargets != null) && (_DictionaryTargets.Count() != 0))
                return true;
            return false;
        }

        public int DictionaryTargetCount()
        {
            if (_DictionaryTargets == null)
                return 0;

            return _DictionaryTargets.Count();
        }

        public bool MatchDictionaryTarget(MultiLanguageString dictionaryForm)
        {
            if (_DictionaryTargets == null)
                return false;

            if (dictionaryForm.LanguageStrings == null)
                return false;

            foreach (LanguageString languageString in dictionaryForm.LanguageStrings)
            {
                string text = languageString.Text;

                if (MatchDictionaryTarget(text))
                    return true;
            }

            return false;
        }

        public bool MatchDictionaryTarget(string dictionaryForm)
        {
            if (_DictionaryTargets == null)
                return false;

            foreach (LiteralString target in _DictionaryTargets)
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

        public bool HasConditions()
        {
            if ((_Conditions != null) && (_Conditions.Count() != 0))
                return true;
            return false;
        }

        public bool MatchCondition(
            MultiLanguageString dictionaryForm,
            LanguageTool tool)
        {
            if (_Conditions == null)
                return true;

            if (dictionaryForm.LanguageStrings == null)
                return false;

            foreach (LanguageString languageString in dictionaryForm.LanguageStrings)
            {
                string text = languageString.Text;

                if (MatchCondition(text, tool))
                    return true;
            }

            return false;
        }

        public bool MatchCondition(
            string dictionaryForm,
            LanguageTool tool)
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
                    case "EndsWithVowelAndLetter":
                        if (dictionaryForm.Length <= 1)
                            break;
                        if (tool.IsVowel(dictionaryForm[dictionaryForm.Length - 2]) && dictionaryForm.EndsWith(condition.Text))
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

        //public static string SemiRegularBreakkpoint = null;

        public bool MatchConditionPost(
            string dictionaryForm,
            string prefix,
            string stem,
            string suffix,
            string inflected,
            LanguageTool tool)
        {
            if (_Conditions == null)
                return false;

            int conditionCount = 0;
            int matchedConditions = 0;

            foreach (Classifier condition in _Conditions)
            {
                switch (condition.KeyString)
                {
                    case "AccentedStemVowel":
                        {
                            char accentedVowel;
                            int index;
                            //if ((SemiRegularBreakkpoint != null) && (dictionaryForm == SemiRegularBreakkpoint))
                            //    ApplicationData.Global.PutConsoleMessage("AccentedStemVowel: " + dictionaryForm);
                            if ((inflected != dictionaryForm) &&
                                tool.GetAccentedVowel(inflected, out accentedVowel, out index))
                            {
                                int prefixLength = prefix.Length;
                                if ((index >= prefixLength) && (index < prefixLength + stem.Length))
                                {
                                    if (condition.Text == accentedVowel.ToString())
                                        matchedConditions++;
                                }
                            }
                        }
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

        public List<SpecialAction> Actions
        {
            get
            {
                return _Actions;
            }
            set
            {
                if (value != _Actions)
                {
                    _Actions = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool MatchActions(SemiRegular other)
        {
            if (other == this)
                return true;

            if (_Actions == other.Actions)
                return true;

            if ((_Actions == null) || (other.Actions == null))
                return false;

            int actionCount = _Actions.Count();
            int actionIndex;

            if (actionCount != other.Actions.Count())
                return false;

            for (actionIndex = 0; actionIndex < actionCount; actionIndex++)
            {
                SpecialAction actionBase = _Actions[actionIndex];
                SpecialAction actionOther = other.Actions[actionIndex];

                if (actionBase.Compare(actionOther) != 0)
                    return false;
            }

            return true;
        }

        public bool ChangesSuffix()
        {
            if ((_Actions == null) || (_Actions.Count() == 0))
                return false;

            foreach (SpecialAction action in _Actions)
            {
                if (action.ChangesSuffix())
                    return true;
            }

            return false;
        }

        public bool Post
        {
            get
            {
                return _Post;
            }
            set
            {
                if (value != _Post)
                {
                    _Post = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool NoPost
        {
            get
            {
                return _NoPost;
            }
            set
            {
                if (value != _NoPost)
                {
                    _NoPost = value;
                    ModifiedFlag = true;
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if (!String.IsNullOrEmpty(Label))
                element.Add(new XAttribute("Label", Label));

            if (!String.IsNullOrEmpty(_Instance))
                element.Add(new XAttribute("Instance", _Instance));

            if (_Post)
                element.Add(new XAttribute("Post", "true"));

            if (_NoPost)
                element.Add(new XAttribute("NoPost", "true"));

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

            if ((_DictionaryTargets != null) && (_DictionaryTargets.Count() != 0))
            {
                if (_DictionaryTargets.First().Count() == 1)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (LiteralString dictionaryTarget in _DictionaryTargets)
                    {
                        if (sb.Length != 0)
                            sb.Append(",");

                        sb.Append(dictionaryTarget.GetIndexedString(0));
                    }

                    element.Add(new XElement("DictionaryTargets", sb.ToString()));
                }
                else
                {
                    foreach (LiteralString dictionaryTarget in _DictionaryTargets)
                        element.Add(new XElement("DictionaryTarget", dictionaryTarget.StringListString));
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

            if (_Actions != null)
            {
                foreach (SpecialAction action in _Actions)
                {
                    XElement childElement = action.GetElement("Action");
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
                case "Instance":
                    _Instance = attributeValue;
                    break;
                case "Post":
                    _Post = ObjectUtilities.GetBoolFromString(attributeValue, false);
                    break;
                case "NoPost":
                    _NoPost = ObjectUtilities.GetBoolFromString(attributeValue, false);
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
                case "Instance":
                    _Instance = childElement.Value.Trim();
                    break;
                case "Post":
                    _Post = ObjectUtilities.GetBoolFromString(childElement.Value.Trim(), false);
                    break;
                case "NoPost":
                    _NoPost = ObjectUtilities.GetBoolFromString(childElement.Value.Trim(), false);
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
                case "DictionaryTargets":
                    {
                        _DictionaryTargets = new List<LiteralString>();
                        List<string> dictionaryTargets = ObjectUtilities.GetStringListFromString(
                            childElement.Value.Trim());
                        foreach (string dictionaryTarget in dictionaryTargets)
                            _DictionaryTargets.Add(new LiteralString(dictionaryTarget));
                    }
                    break;
                case "DictionaryTarget":
                    {
                        LiteralString dictionaryTarget = new LiteralString(childElement.Value.Trim());
                        if (_DictionaryTargets != null)
                            _DictionaryTargets.Add(dictionaryTarget);
                        else
                            _DictionaryTargets = new List<LiteralString>() { dictionaryTarget };
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
                case "Action":
                    {
                        SpecialAction action = new SpecialAction(childElement);
                        if (_Actions != null)
                            _Actions.Add(action);
                        else
                            _Actions = new List<SpecialAction>() { action };
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
