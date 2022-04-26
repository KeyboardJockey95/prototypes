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
    public enum ModifierCode
    {
        None,
        StemChange          // a Function will be expected.
    }

    public class Modifier : BaseObject
    {
        public LexicalCategory Category;        // Verb, Adjective, Noun, etc.
        public List<string> Class;              // i.e. verb class
        public List<string> SubClass;           // i.e. verb sublcass
        public string TriggerFunction;          // Call to a generic or language-specific funciton used as a trigger. (Name(args))
        public string Stem;                     // Name of stem inflector, or null if none.
        public ModifierCode ModifierType;
        public LiteralString PrePronoun;
        public LiteralString PreWords;
        public LiteralString Prefix;
        public LiteralString Suffix;
        public LiteralString PostWords;
        public string Function;
        public List<SpecialAction> Actions;
        public LexicalCategory NewCategory;
        public string NewCategoryString;
        public string NewClass;
        public string NewSubClass;

        public Modifier(
            LexicalCategory category,
            List<string> classType,
            List<string> subClass,
            string stem,
            ModifierCode modifierType,
            LiteralString prePronoun,
            LiteralString preWords,
            LiteralString prefix,
            LiteralString suffix,
            LiteralString postWords,
            string function,
            List<SpecialAction> actions,
            LexicalCategory newCategory,
            string newCategoryString,
            string newClass,
            string newSubClass)
        {
            Category = category;
            Class = classType;
            SubClass = subClass;
            TriggerFunction = null;
            Stem = stem;
            ModifierType = modifierType;
            PrePronoun = prePronoun;
            PreWords = preWords;
            Prefix = prefix;
            Suffix = suffix;
            PostWords = postWords;
            Function = function;
            Actions = actions;
            NewCategory = newCategory;
            NewCategoryString = newCategoryString;
            NewClass = newClass;
            NewSubClass = newSubClass;
        }

        public Modifier(
            LexicalCategory category,
            List<string> classType,
            List<string> subClass,
            string stem,
            ModifierCode modifierType,
            string prePronoun,
            string preWords,
            string prefix,
            string suffix,
            string postWords,
            string function,
            List<SpecialAction> actions)
        {
            Category = category;
            Class = classType;
            SubClass = subClass;
            TriggerFunction = null;
            Stem = stem;
            ModifierType = modifierType;
            PrePronoun = (!String.IsNullOrEmpty(prePronoun) ? new LiteralString(prePronoun) : null);
            PreWords = (!String.IsNullOrEmpty(preWords) ? new LiteralString(preWords) : null);
            Prefix = (!String.IsNullOrEmpty(prefix) ? new LiteralString(prefix) : null);
            Suffix = (!String.IsNullOrEmpty(suffix) ? new LiteralString(suffix) : null);
            PostWords = (!String.IsNullOrEmpty(postWords) ? new LiteralString(postWords) : null);
            Function = function;
            Actions = actions;
            NewCategory = LexicalCategory.Unknown;
            NewCategoryString = null;
            NewClass = null;
            NewSubClass = null;
        }

        public Modifier(
            LexicalCategory category,
            List<string> classType,
            List<string> subClass,
            ModifierCode modifierType,
            LiteralString prePronoun,
            LiteralString preWords,
            LiteralString prefix,
            LiteralString suffix,
            LiteralString postWords)
        {
            Category = category;
            Class = classType;
            SubClass = subClass;
            TriggerFunction = null;
            Stem = null;
            ModifierType = modifierType;
            PrePronoun = prePronoun;
            PreWords = preWords;
            Prefix = prefix;
            Suffix = suffix;
            PostWords = postWords;
            Function = null;
            Actions = null;
            NewCategory = LexicalCategory.Unknown;
            NewCategoryString = null;
            NewClass = null;
            NewSubClass = null;
        }

        public Modifier(
            LexicalCategory category,
            List<string> classType,
            List<string> subClass,
            ModifierCode modifierType,
            string prePronoun,
            string preWords,
            string prefix,
            string suffix,
            string postWords)
        {
            Category = category;
            Class = classType;
            SubClass = subClass;
            TriggerFunction = null;
            Stem = null;
            ModifierType = modifierType;
            PrePronoun = (!String.IsNullOrEmpty(prePronoun) ? new LiteralString(prePronoun) : null);
            PreWords = (!String.IsNullOrEmpty(preWords) ? new LiteralString(preWords) : null);
            Prefix = (!String.IsNullOrEmpty(prefix) ? new LiteralString(prefix) : null);
            Suffix = (!String.IsNullOrEmpty(suffix) ? new LiteralString(suffix) : null);
            PostWords = (!String.IsNullOrEmpty(postWords) ? new LiteralString(postWords) : null);
            Function = null;
            Actions = null;
            NewCategory = LexicalCategory.Unknown;
            NewCategoryString = null;
            NewClass = null;
            NewSubClass = null;
        }

        public Modifier(
            LexicalCategory category,
            string classType,
            string suffix)
        {
            Category = category;
            Class = new List<string>() { classType };
            SubClass = null;
            TriggerFunction = null;
            Stem = null;
            ModifierType = ModifierCode.None;
            PrePronoun = null;
            PreWords = null;
            Prefix = null;
            Suffix = (!String.IsNullOrEmpty(suffix) ? new LiteralString(suffix) : null);
            PostWords = null;
            Function = null;
            Actions = null;
            NewCategory = LexicalCategory.Unknown;
            NewCategoryString = null;
            NewClass = null;
            NewSubClass = null;
        }

        public Modifier(
            LexicalCategory category,
            string classType,
            string preWords,
            string suffix)
        {
            Category = category;
            Class = new List<string>() { classType };
            SubClass = null;
            TriggerFunction = null;
            Stem = null;
            ModifierType = ModifierCode.None;
            PrePronoun = null;
            PreWords = (!String.IsNullOrEmpty(preWords) ? new LiteralString(preWords) : null);
            Prefix = null;
            Suffix = (!String.IsNullOrEmpty(suffix) ? new LiteralString(suffix) : null);
            PostWords = null;
            Function = null;
            Actions = null;
            NewCategory = LexicalCategory.Unknown;
            NewCategoryString = null;
            NewClass = null;
            NewSubClass = null;
        }

        public Modifier(
            LexicalCategory category,
            List<string> classType,
            List<string> subClass,
            ModifierCode modifierType,
            string function)
        {
            Category = category;
            Class = classType;
            SubClass = subClass;
            TriggerFunction = null;
            Stem = null;
            ModifierType = modifierType;
            PrePronoun = null;
            PreWords = null;
            Prefix = null;
            Suffix = null;
            PostWords = null;
            Function = function;
            Actions = null;
            NewCategory = LexicalCategory.Unknown;
            NewCategoryString = null;
            NewClass = null;
            NewSubClass = null;
        }

        public Modifier(
            LexicalCategory category,
            List<string> classType,
            List<string> subClass,
            ModifierCode modifierType)
        {
            Category = category;
            Class = classType;
            SubClass = subClass;
            TriggerFunction = null;
            Stem = null;
            ModifierType = modifierType;
            PrePronoun = null;
            PreWords = null;
            Prefix = null;
            Suffix = null;
            PostWords = null;
            Function = null;
            Actions = null;
            NewCategory = LexicalCategory.Unknown;
            NewCategoryString = null;
            NewClass = null;
            NewSubClass = null;
        }

        public Modifier(Modifier other)
        {
            CopyModifier(other);
        }

        public Modifier(XElement element)
        {
            OnElement(element);
        }

        public Modifier()
        {
            ClearModifier();
        }

        public void ClearModifier()
        {
            Category = LexicalCategory.Unknown;
            Class = null;
            SubClass = null;
            TriggerFunction = null;
            Stem = null;
            ModifierType = ModifierCode.None;
            PrePronoun = null;
            PreWords = null;
            Prefix = null;
            Suffix = null;
            PostWords = null;
            Function = null;
            Actions = null;
            NewCategory = LexicalCategory.Unknown;
            NewCategoryString = null;
            NewClass = null;
            NewSubClass = null;
        }

        public void CopyModifier(Modifier other)
        {
            Category = other.Category;
            if (other.Class != null)
                Class = new List<string>(other.Class);
            else
                Class = null;
            if (other.SubClass != null)
                SubClass = new List<string>(other.SubClass);
            else
                SubClass = null;
            TriggerFunction = other.TriggerFunction;
            Stem = other.Stem;
            ModifierType = other.ModifierType;
            PrePronoun = LiteralString.Clone(other.PrePronoun);
            PreWords = LiteralString.Clone(other.PreWords);
            Prefix = LiteralString.Clone(other.Prefix);
            Suffix = LiteralString.Clone(other.Suffix);
            PostWords = LiteralString.Clone(other.PostWords);
            Function = other.Function;
            Actions = other.CloneActions();
            NewCategory = other.NewCategory;
            NewCategoryString = other.NewCategoryString;
            NewClass = other.NewClass;
            NewSubClass = other.NewSubClass;
        }

        public bool HasAnyClass()
        {
            if ((Class != null) && (Class.Count() != 0))
                return true;

            return false;
        }

        public bool HasClass(string className)
        {
            if ((Class != null) && (Class.Count() != 0))
            {
                if (Class.Contains(className))
                    return true;
            }

            return false;
        }

        public string ClassDisplay()
        {
            if ((Class != null) && (Class.Count() != 0))
                return ObjectUtilities.GetStringFromStringList(Class);

            return String.Empty;
        }

        public bool HasAnySubClass()
        {
            if ((SubClass != null) && (SubClass.Count() != 0))
                return true;

            return false;
        }

        public bool HasSubClass(string SubclassName)
        {
            if ((SubClass != null) && (SubClass.Count() != 0))
            {
                if (SubClass.Contains(SubclassName))
                    return true;
            }

            return false;
        }

        public string SubClassDisplay()
        {
            if ((SubClass != null) && (SubClass.Count() != 0))
                return ObjectUtilities.GetStringFromStringList(SubClass);

            return String.Empty;
        }

        public bool HasAnyTriggerFunction()
        {
            return !String.IsNullOrEmpty(TriggerFunction);
        }

        public bool HasTriggerFunction(string triggerFunctionName)
        {
            return TextUtilities.ContainsWholeWord(TriggerFunction, triggerFunctionName);
        }

        public bool MatchClass(string className)
        {
            if ((Class != null) && (Class.Count() != 0))
            {
                if (String.IsNullOrEmpty(className))
                    return true;

                if (Class.Contains(className))
                    return true;

                return false;
            }

            return true;
        }

        public bool MatchClass(List<string> classNames)
        {
            if ((Class != null) && (Class.Count() != 0))
            {
                if ((classNames == null) || (classNames.Count() == 0))
                    return true;

                if (ObjectUtilities.CompareStringLists(classNames, Class) == 0)
                    return true;

                return false;
            }

            return true;
        }

        public bool MatchSubClass(string subClassName)
        {
            if ((SubClass != null) && (SubClass.Count() != 0))
            {
                if (String.IsNullOrEmpty(subClassName))
                    return true;

                if (SubClass.Contains(subClassName))
                    return true;

                return false;
            }

            return true;
        }

        public bool MatchSubClass(List<string> subClassNames)
        {
            if ((SubClass != null) && (SubClass.Count() != 0))
            {
                if ((subClassNames == null) || (subClassNames.Count() == 0))
                    return true;

                if (ObjectUtilities.CompareStringLists(subClassNames, SubClass) == 0)
                    return true;

                return false;
            }

            return true;
        }

        public bool MatchClassAndSubClass(string className, string subClassName)
        {
            if (!MatchClass(className))
                return false;

            if (!MatchSubClass(subClassName))
                return false;

            return true;
        }

        public bool MatchClassAndSubClass(List<string> classNames, List<string> subClassNames)
        {
            if (!MatchClass(classNames))
                return false;

            if (!MatchSubClass(subClassNames))
                return false;

            return true;
        }

        public LiteralString GetLiteralString(string target)
        {
            LiteralString ls;

            switch (target)
            {
                case "PrePronoun":
                    ls = PrePronoun;
                    break;
                case "PreWords":
                    ls = PreWords;
                    break;
                case "Prefix":
                    ls = Prefix;
                    break;
                case "Suffix":
                    ls = Suffix;
                    break;
                case "PostWords":
                    ls = PostWords;
                    break;
                default:
                    ls = null;
                    break;
            }

            return ls;
        }

        public void SetLiteralString(string target, LiteralString ls)
        {
            switch (target)
            {
                case "PrePronoun":
                    PrePronoun = ls;
                    break;
                case "PreWords":
                    PreWords = ls;
                    break;
                case "Prefix":
                    Prefix = ls;
                    break;
                case "Suffix":
                    Suffix = ls;
                    break;
                case "PostWords":
                    PostWords = ls;
                    break;
                default:
                    break;
            }
        }

        public List<SpecialAction> CloneActions()
        {
            if (Actions == null)
                return null;

            List<SpecialAction> actions = new List<SpecialAction>();

            foreach (SpecialAction action in Actions)
                actions.Add(new SpecialAction(action));

            return actions;
        }

        public void AppendAction(SpecialAction action)
        {
            if (Actions == null)
                Actions = new List<SpecialAction>() { action };
            else
                Actions.Add(action);
        }

        public bool Modify(
            MultiLanguageString dictionaryForm,
            MultiLanguageString stem,
            List<LanguageID> languageIDs,
            LanguageTool languageTool,
            Inflector inflector,
            InflectorTable inflectorTable,
            string instance,
            Inflection inflection)
        {
            return languageTool.InflectorModify(
                dictionaryForm,
                stem,
                languageIDs,
                inflector,
                this,
                null,
                null,
                inflectorTable,
                instance,
                inflection);
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if (Category != LexicalCategory.Unknown)
                element.Add(new XAttribute("Category", Category.ToString()));

            if (Class != null)
                element.Add(new XAttribute("Class", ObjectUtilities.GetStringFromStringList(Class)));

            if (SubClass != null)
                element.Add(new XAttribute("SubClass", ObjectUtilities.GetStringFromStringList(SubClass)));

            if (!String.IsNullOrEmpty(TriggerFunction))
                element.Add(new XAttribute("TriggerFunction", TriggerFunction));

            if (!String.IsNullOrEmpty(Stem))
                element.Add(new XAttribute("Stem", Stem));

            if (ModifierType != ModifierCode.None)
                element.Add(new XAttribute("ModifierType", ModifierType.ToString()));

            if (PrePronoun != null)
            {
                string literal = PrePronoun.StringListString;
                element.Add(new XAttribute("PrePronoun", literal));
            }

            if (PreWords != null)
            {
                string literal = PreWords.StringListString;
                element.Add(new XAttribute("PreWords", literal));
            }

            if (Prefix != null)
            {
                string literal = Prefix.StringListString;
                element.Add(new XAttribute("Prefix", literal));
            }

            if (Suffix != null)
            {
                string literal = Suffix.StringListString;
                element.Add(new XAttribute("Suffix", literal));
            }

            if (PostWords != null)
            {
                string literal = PostWords.StringListString;
                element.Add(new XAttribute("PostWords", literal));
            }

            if (!String.IsNullOrEmpty(Function))
                element.Add(new XAttribute("Function", Function));

            if (Actions != null)
            {
                foreach (SpecialAction action in Actions)
                {
                    XElement childElement = action.GetElement("Action");
                    element.Add(childElement);
                }
            }

            if (NewCategory != LexicalCategory.Unknown)
                element.Add(new XAttribute("NewCategory", NewCategory.ToString()));

            if (NewCategoryString != null)
                element.Add(new XAttribute("NewCategoryString", NewCategoryString));

            if (NewClass != null)
                element.Add(new XAttribute("NewClass", NewClass));

            if (NewSubClass != null)
                element.Add(new XAttribute("NewSubClass", NewSubClass));

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value;

            switch (attribute.Name.LocalName)
            {
                case "Category":
                    Category = Sense.GetLexicalCategoryFromString(attributeValue);
                    break;
                case "Class":
                    Class = ObjectUtilities.GetStringListFromString(attributeValue);
                    break;
                case "SubClass":
                    SubClass = ObjectUtilities.GetStringListFromString(attributeValue);
                    break;
                case "TriggerFunction":
                    TriggerFunction = attributeValue;
                    break;
                case "Stem":
                    Stem = attributeValue;
                    break;
                case "ModifierType":
                    ModifierType = GetModifierTypeFromString(attributeValue);
                    break;
                case "PrePronoun":
                    PrePronoun = new LiteralString(attributeValue);
                    break;
                case "PreWords":
                    PreWords = new LiteralString(attributeValue);
                    break;
                case "Prefix":
                    Prefix = new LiteralString(attributeValue);
                    break;
                case "Suffix":
                    Suffix = new LiteralString(attributeValue);
                    break;
                case "PostWords":
                    PostWords = new LiteralString(attributeValue);
                    break;
                case "Function":
                    Function = attributeValue;
                    break;
                case "NewCategory":
                    NewCategory = Sense.GetLexicalCategoryFromString(attributeValue);
                    break;
                case "NewCategoryString":
                    NewCategoryString = attributeValue;
                    break;
                case "NewClass":
                    NewClass = attributeValue;
                    break;
                case "NewSubClass":
                    NewSubClass = attributeValue;
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
                case "PrePronoun":
                    if (childElement.HasElements)
                    {
                        List<string> stringList = new List<string>();
                        foreach (XElement subElement in childElement.Elements())
                            stringList.Add(subElement.Value);
                        PrePronoun = new LiteralString(stringList);
                    }
                    else
                        PrePronoun = new LiteralString(childElement.Value.Trim());
                    break;
                case "PreWords":
                    if (childElement.HasElements)
                    {
                        List<string> stringList = new List<string>();
                        foreach (XElement subElement in childElement.Elements())
                            stringList.Add(subElement.Value);
                        PreWords = new LiteralString(stringList);
                    }
                    else
                        PreWords = new LiteralString(childElement.Value.Trim());
                    break;
                case "Prefix":
                    if (childElement.HasElements)
                    {
                        List<string> stringList = new List<string>();
                        foreach (XElement subElement in childElement.Elements())
                            stringList.Add(subElement.Value);
                        Prefix = new LiteralString(stringList);
                    }
                    else
                        Prefix = new LiteralString(childElement.Value.Trim());
                    break;
                case "Suffix":
                    if (childElement.HasElements)
                    {
                        List<string> stringList = new List<string>();
                        foreach (XElement subElement in childElement.Elements())
                            stringList.Add(subElement.Value);
                        Suffix = new LiteralString(stringList);
                    }
                    else
                        Suffix = new LiteralString(childElement.Value.Trim());
                    break;
                case "PostWords":
                    if (childElement.HasElements)
                    {
                        List<string> stringList = new List<string>();
                        foreach (XElement subElement in childElement.Elements())
                            stringList.Add(subElement.Value);
                        PostWords = new LiteralString(stringList);
                    }
                    else
                        PostWords = new LiteralString(childElement.Value.Trim());
                    break;
                case "Action":
                    {
                        SpecialAction action = new SpecialAction(childElement);
                        if (Actions != null)
                            Actions.Add(action);
                        else
                            Actions = new List<SpecialAction>() { action };
                    }
                    break;
                default:
                    return false;
            }

            return true;
        }

        public static ModifierCode GetModifierTypeFromString(string str)
        {
            ModifierCode modifierType;

            switch (str)
            {
                case "None":
                    modifierType = ModifierCode.None;
                    break;
                case "StemChange":
                    modifierType = ModifierCode.StemChange;
                    break;
                default:
                    throw new Exception("Unknown modifier type: " + str);
            }

            return modifierType;
        }
    }
}
