using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    // Describes a phrase or sentence pattern.
    // Key is the designator label.
    public class PhrasePattern : BaseObjectKeyed
    {
        // The phrase pattern word tokens in order.
        public List<WordToken> Pattern;
        // Designates the inflection this pattern describes.
        public Designator Designation;

        public PhrasePattern(
            List<WordToken> pattern,
            Designator designation) : base(designation.Label)
        {
            Pattern = pattern;
            Designation = designation;
        }

        public PhrasePattern(XElement element)
        {
            OnElement(element);
        }

        public PhrasePattern(PhrasePattern other) : base(other)
        {
            CopyPhrasePattern(other);
        }

        public PhrasePattern()
        {
            ClearPhrasePattern();
        }

        public void CopyPhrasePattern(PhrasePattern other)
        {
            Pattern = other.ClonePattern();
            Designation = other.CloneDesignation();
        }

        public void ClearPhrasePattern()
        {
            Pattern = null;
            Designation = null;
        }

        public override void Clear()
        {
            ClearPhrasePattern();
        }

        public List<WordToken> ClonePattern()
        {
            if (Pattern == null)
                return null;

            List<WordToken> newPattern = new List<WordToken>();

            foreach (WordToken wordToken in Pattern)
                newPattern.Add(new WordToken(wordToken));

            return newPattern;
        }

        public Designator CloneDesignation()
        {
            if (Designation == null)
                return null;

            return new Designator(Designation);
        }

        public override string ToString()
        {
            return Xml.ToString();
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if ((Pattern != null) && (Pattern.Count() != 0))
            {
                XElement patternElement = new XElement("Pattern");

                foreach (WordToken wordToken in Pattern)
                    patternElement.Add(wordToken.GetElement("Token"));
            }

            if (Designation != null)
                element.Add(Designation.GetElement("Designation"));

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Pattern":
                    {
                        Pattern = new List<WordToken>();
                        foreach (XElement wordElement in childElement.Elements())
                        {
                            WordToken word = new WordToken(wordElement);
                            Pattern.Add(word);
                        }
                    }
                    break;
                case "Designation":
                    Designation = new Designator(childElement);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
