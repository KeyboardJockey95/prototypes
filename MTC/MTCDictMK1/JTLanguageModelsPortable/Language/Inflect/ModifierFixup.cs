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
    public class ModifierFixup : BaseObject
    {
        public LexicalCategory Category;    // Verb, Adjective, Noun, etc.
        public string Class;                // i.e. verb class
        public string SubClass;             // i.e. verb sublcass
        public string Type;                 // See Modify
        public string Target;
        public LiteralString Input;
        public LiteralString Output;
        public LiteralString Separator;
        public string From;
        public string To;
        public LiteralString Qualifiers;
        public string AndCase;
        public string AndNotCase;
        public string OrCase;
        public string OrNotCase;

        public ModifierFixup(ModifierFixup other)
        {
            CopyModifierFixup(other);
        }

        public ModifierFixup(XElement element)
        {
            OnElement(element);
        }

        public ModifierFixup()
        {
            ClearModifierFixup();
        }

        public void ClearModifierFixup()
        {
            Category = LexicalCategory.Unknown;
            Class = String.Empty;
            SubClass = String.Empty;
            Type = null;
            Target = null;
            Input = null;
            Output = null;
            Separator = null;
            From = null;
            To = null;
            Qualifiers = null;
            AndCase = null;
            AndNotCase = null;
            OrCase = null;
            OrNotCase = null;
        }

        public void CopyModifierFixup(ModifierFixup other)
        {
            Category = other.Category;
            Class = other.Class;
            SubClass = other.SubClass;
            Type = other.Type;
            Target = other.Target;
            Input = other.Input;
            Output = other.Output;
            Separator = other.Separator;
            From = other.From;
            To = other.To;
            Qualifiers = other.Qualifiers;
            AndCase = other.AndCase;
            AndNotCase = other.AndNotCase;
            OrCase = other.OrCase;
            OrNotCase = other.OrNotCase;
        }

        public bool Modify(Modifier modifier)
        {
            bool returnValue = true;

            switch (Type)
            {
                case "Prepend":
                    returnValue = HandlePrepend(modifier);
                    break;
                case "Append":
                    returnValue = HandleAppend(modifier);
                    break;
                case "Replace":
                    returnValue = HandleReplace(modifier);
                    break;
                case "Move":
                    returnValue = HandleMove(modifier);
                    break;
                case "Remove":
                    returnValue = HandleRemove(modifier);
                    break;
                default:
                    throw new Exception("ModifierFixup.Modify: Unknown fixup type: " + Type);
            }

            return returnValue;
        }

        public bool HandlePrepend(Modifier modifier)
        {
            LiteralString ls = modifier.GetLiteralString(Target);
            if ((ls == null) || ls.IsEmpty())
                modifier.SetLiteralString(Target, Output);
            else
            {
                LiteralString newString = LiteralString.Concatenate(Output, Separator, ls);
                modifier.SetLiteralString(Target, newString);
            }
            return true;
        }

        public bool HandleAppend(Modifier modifier)
        {
            LiteralString ls = modifier.GetLiteralString(Target);
            if ((ls == null) || ls.IsEmpty())
                modifier.SetLiteralString(Target, Output);
            else
            {
                LiteralString newString = LiteralString.Concatenate(ls, Separator, Output);
                modifier.SetLiteralString(Target, newString);
            }
            return true;
        }

        public bool HandleReplace(Modifier modifier)
        {
            LiteralString ls = modifier.GetLiteralString(Target);
            if (ls == null)
                return false;
            bool returnValue = LiteralString.Replace(ls, Input, Output);
            return returnValue;
        }

        public bool HandleReplaceIn(Modifier modifier)
        {
            LiteralString ls = modifier.GetLiteralString(Target);
            if (ls == null)
                return false;
            bool returnValue = LiteralString.ReplaceIn(ls, Input, Output);
            return returnValue;
        }

        public bool HandleMove(Modifier modifier)
        {
            LiteralString ls = modifier.GetLiteralString(From);
            if (ls == null)
                return false;
            modifier.SetLiteralString(From, null);
            modifier.SetLiteralString(To, ls);
            bool returnValue = true;
            return returnValue;
        }

        public bool HandleRemove(Modifier modifier)
        {
            modifier.SetLiteralString(Target, null);
            return true;
        }

        public bool MatchCases(string caseLabel)
        {
            if (!MatchAndCase(caseLabel))
                return false;

            if (MatchAndNotCase(caseLabel))
                return false;

            if (!MatchOrCase(caseLabel))
                return false;

            if (MatchOrNotCase(caseLabel))
                return false;

            return true;
        }

        public bool MatchAndCase(string caseLabel)
        {
            if (String.IsNullOrEmpty(AndCase))
                return true;

            if (caseLabel.Contains(AndCase))
                return true;

            if (AndCase.Contains(","))
            {
                string[] parts = AndCase.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    if (!caseLabel.Contains(part.Trim()))
                        return false;
                }
                return true;
            }

            return false;
        }

        public bool MatchAndNotCase(string caseLabel)
        {
            if (String.IsNullOrEmpty(AndNotCase))
                return false;

            if (caseLabel.Contains(AndNotCase))
                return true;

            if (AndNotCase.Contains(","))
            {
                string[] parts = AndNotCase.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    if (!caseLabel.Contains(part.Trim()))
                        return false;
                }
                return true;
            }

            return false;
        }

        public bool MatchOrCase(string caseLabel)
        {
            if (String.IsNullOrEmpty(OrCase))
                return true;

            if (caseLabel.Contains(OrCase))
                return true;

            if (OrCase.Contains(","))
            {
                string[] parts = OrCase.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    if (caseLabel.Contains(part.Trim()))
                        return true;
                }
            }

            return false;
        }

        public bool MatchOrNotCase(string caseLabel)
        {
            if (String.IsNullOrEmpty(OrNotCase))
                return false;

            if (caseLabel.Contains(OrNotCase))
                return true;

            if (OrNotCase.Contains(","))
            {
                string[] parts = OrNotCase.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    if (caseLabel.Contains(part.Trim()))
                        return true;
                }
            }

            return false;
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if (Category != LexicalCategory.Unknown)
                element.Add(new XAttribute("Category", Category.ToString()));

            if (!String.IsNullOrEmpty(Class))
                element.Add(new XAttribute("Class", Class));

            if (!String.IsNullOrEmpty(SubClass))
                element.Add(new XAttribute("SubClass", SubClass));

            if (!String.IsNullOrEmpty(Type))
                element.Add(new XAttribute("Type", Type));

            if (!String.IsNullOrEmpty(Target))
                element.Add(new XAttribute("Target", Target));

            if (Input != null)
            {
                string literal = Input.StringListString;
                element.Add(new XElement("Input", literal));
            }

            if (Output != null)
            {
                string literal = Output.StringListString;
                element.Add(new XElement("Output", literal));
            }

            if (Separator != null)
            {
                string literal = Separator.StringListString;
                element.Add(new XElement("Separator", literal));
            }

            if (!String.IsNullOrEmpty(From))
                element.Add(new XAttribute("From", From));

            if (!String.IsNullOrEmpty(To))
                element.Add(new XAttribute("To", To));

            if (Qualifiers != null)
            {
                string literal = Qualifiers.StringListString;
                element.Add(new XElement("Qualifiers", literal));
            }

            if (!String.IsNullOrEmpty(AndCase))
                element.Add(new XAttribute("AndCase", AndCase));

            if (!String.IsNullOrEmpty(AndNotCase))
                element.Add(new XAttribute("AndNotCase", AndNotCase));

            if (!String.IsNullOrEmpty(OrCase))
                element.Add(new XAttribute("OrCase", OrCase));

            if (!String.IsNullOrEmpty(OrNotCase))
                element.Add(new XAttribute("OrNotCase", OrNotCase));

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
                    Class = attributeValue;
                    break;
                case "SubClass":
                    SubClass = attributeValue;
                    break;
                case "Type":
                    Type = attributeValue;
                    break;
                case "Target":
                    Target = attributeValue;
                    break;
                case "Input":
                    Input = new LiteralString(attributeValue);
                    break;
                case "Output":
                    Output = new LiteralString(attributeValue);
                    break;
                case "Separator":
                    Separator = new LiteralString(attributeValue);
                    break;
                case "From":
                    From = attributeValue;
                    break;
                case "To":
                    To = attributeValue;
                    break;
                case "Qualifiers":
                    Qualifiers = new LiteralString(attributeValue);
                    break;
                case "AndCase":
                    AndCase = attributeValue;
                    break;
                case "AndNotCase":
                    AndNotCase = attributeValue;
                    break;
                case "OrCase":
                    OrCase = attributeValue;
                    break;
                case "OrNotCase":
                    OrNotCase = attributeValue;
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
                case "Category":
                    Category = Sense.GetLexicalCategoryFromString(childElement.Value.Trim());
                    break;
                case "Class":
                    Class = childElement.Value.Trim();
                    break;
                case "SubClass":
                    SubClass = childElement.Value.Trim();
                    break;
                case "Type":
                    Type = childElement.Value.Trim();
                    break;
                case "Target":
                    Target = childElement.Value.Trim();
                    break;
                case "Input":
                    if (childElement.HasElements)
                    {
                        List<string> stringList = new List<string>();
                        foreach (XElement subElement in childElement.Elements())
                            stringList.Add(subElement.Value);
                        Input = new LiteralString(stringList);
                    }
                    else
                        Input = new LiteralString(childElement.Value.Trim());
                    break;
                case "Output":
                    if (childElement.HasElements)
                    {
                        List<string> stringList = new List<string>();
                        foreach (XElement subElement in childElement.Elements())
                            stringList.Add(subElement.Value);
                        Output = new LiteralString(stringList);
                    }
                    else
                        Output = new LiteralString(childElement.Value.Trim());
                    break;
                case "Separator":
                    if (childElement.HasElements)
                    {
                        List<string> stringList = new List<string>();
                        foreach (XElement subElement in childElement.Elements())
                            stringList.Add(subElement.Value);
                        Separator = new LiteralString(stringList);
                    }
                    else
                        Separator = new LiteralString(childElement.Value.Trim());
                    break;
                case "From":
                    From = childElement.Value.Trim();
                    break;
                case "To":
                    To = childElement.Value.Trim();
                    break;
                case "Qualifiers":
                    if (childElement.HasElements)
                    {
                        List<string> stringList = new List<string>();
                        foreach (XElement subElement in childElement.Elements())
                            stringList.Add(subElement.Value);
                        Qualifiers = new LiteralString(stringList);
                    }
                    else
                        Qualifiers = new LiteralString(childElement.Value.Trim());
                    break;
                case "AndCase":
                    AndCase = childElement.Value.Trim();
                    break;
                case "AndNotCase":
                    AndNotCase = childElement.Value.Trim();
                    break;
                case "OrCase":
                    OrCase = childElement.Value.Trim();
                    break;
                case "OrNotCase":
                    OrNotCase = childElement.Value.Trim();
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
