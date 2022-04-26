using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Matchers
{
    public partial class LanguageStringMatcher : Matcher
    {
        public Matcher KeyMatcher { get; set; }
        public Matcher LanguageIDMatcher { get; set; }
        public Matcher TextMatcher { get; set; }

        public LanguageStringMatcher(Matcher keyMatcher, Matcher languageIDMatcher, Matcher textMatcher,
                int page, int pageSize)
            : base(new List<Matcher> { keyMatcher, languageIDMatcher, textMatcher }, null, MatchCode.And, page, pageSize)
        {
            KeyMatcher = keyMatcher;
            LanguageIDMatcher = languageIDMatcher;
            TextMatcher = textMatcher;
        }

        public LanguageStringMatcher(LanguageStringMatcher other)
            : base(other)
        {
            CopyLanguageStringMatcher(other);
        }

        public LanguageStringMatcher(XElement element)
        {
            OnElement(element);
        }

        public LanguageStringMatcher()
        {
            ClearLanguageStringMatcher();
        }

        public override void Clear()
        {
            base.Clear();
            ClearLanguageStringMatcher();
        }

        public void ClearLanguageStringMatcher()
        {
            MatchType = MatchCode.And;
            KeyMatcher = null;
            LanguageIDMatcher = null;
            TextMatcher = null;
        }

        public virtual void CopyLanguageStringMatcher(LanguageStringMatcher other)
        {
            KeyMatcher = other.KeyMatcher;
            LanguageIDMatcher = other.LanguageIDMatcher;
            TextMatcher = other.TextMatcher;
        }

        public override bool Match(object obj)
        {
            if (obj == null)
                return false;

            bool keyMatch = true;
            bool languageIDMatch = true;
            bool textMatch = true;

            if (obj is MultiLanguageString)
            {
                MultiLanguageString mls = obj as MultiLanguageString;

                if ((mls == null) || (mls.Count() == 0))
                    return false;

                foreach (LanguageString languageStringObj in mls.LanguageStrings)
                {
                    if (KeyMatcher != null)
                        keyMatch = KeyMatcher.Match(languageStringObj.Key);

                    if (LanguageIDMatcher != null)
                        languageIDMatch = LanguageIDMatcher.Match(languageStringObj.LanguageID);

                    if (TextMatcher != null)
                        textMatch = TextMatcher.Match(languageStringObj.Text);

                    if (keyMatch && languageIDMatch && textMatch)
                        return true;
                }

                return false;
            }
            else if (obj is LanguageString)
            {
                LanguageString languageStringObj = obj as LanguageString;

                if (languageStringObj == null)
                    return false;

                if (KeyMatcher != null)
                    keyMatch = KeyMatcher.Match(languageStringObj.Key);

                if (LanguageIDMatcher != null)
                    languageIDMatch = LanguageIDMatcher.Match(languageStringObj.LanguageID);

                if (TextMatcher != null)
                    textMatch = TextMatcher.Match(languageStringObj.Text);

                return keyMatch && languageIDMatch && textMatch;
            }
            else
                return false;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            XElement matcherElement;
            if (KeyMatcher != null)
            {
                matcherElement = new XElement("KeyMatcher");
                matcherElement.Add(KeyMatcher.Xml);
                element.Add(matcherElement);
            }
            if (LanguageIDMatcher != null)
            {
                matcherElement = new XElement("LanguageIDMatcher");
                matcherElement.Add(LanguageIDMatcher.Xml);
                element.Add(matcherElement);
            }
            if (TextMatcher != null)
            {
                matcherElement = new XElement("TextMatcher");
                matcherElement.Add(TextMatcher.Xml);
                element.Add(matcherElement);
            }
            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "KeyMatcher":
                    KeyMatcher = (Matcher)ObjectUtilities.ResurrectBaseObject(childElement.Elements().First());
                    break;
                case "LanguageIDMatcher":
                    LanguageIDMatcher = (Matcher)ObjectUtilities.ResurrectBaseObject(childElement.Elements().First());
                    break;
                case "TextMatcher":
                    TextMatcher = (Matcher)ObjectUtilities.ResurrectBaseObject(childElement.Elements().First());
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
