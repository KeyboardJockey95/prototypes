using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    public class InflectorFamily : Designator
    {
        // Either manually-specified inflectors, or generated inflectors.
        public List<Inflector> Inflectors;
        // Inflection source (see InflectionSources).
        public string InflectionSource;
        // Inflection source strings.
        public static string[] InflectionSources =
        {
            "Implicit",
            "Manual",
            "Compound",
            "External",
            "Inherited"
        };
        // Label of inherited external inflection.
        public string Inherit;
        // Name of iteration source (see IterateSources).
        public string Iterate;
        // Iteration source strings.
        public static string[] IterateSources =
        {
            "SubjectPronouns",
            "ReflexivePronouns",
            "DirectPronouns",
            "IndirectPronouns"
        };
        // Flag meaning that external doesn't have the iterations, such that the iteration is needed for the irregular generation.
        public bool IterateIrregulars;
        // Designators of iterator to exclude.
        public List<Designator> Exclude;
        // Describes inflection generation.
        public List<WordToken> Pattern;
        // A string that describes the format of the external inflection (see ExternalPatterns).
        public string ExternalPattern;
        // An alternate string that describes the format of the external inflection (see ExternalPatterns).
        public string AlternateExternalPattern;
        // External pattern tokens.
        public static string[] ExternalPatterns =
        {
            "%m",       // Main
            "%h",       // Helper
            "%p",       // Pronoun
            "%r",       // Reflexive Pronoun
            "%n",       // Polarizer
            "%o"        // Modal
        };
        // List of inflector labels or label patterns for expanded families.
        public List<string> ExpandLabels;
        // Which pass for special inflectors.
        public int Pass;

        public InflectorFamily(
                string label,
                List<Classifier> classifications,
                List<Inflector> inflectors,
                string inflectionSource,
                string inherit,
                string iterate,
                bool iterateIrregulars,
                List<Designator> exclude,
                List<WordToken> pattern,
                string externalPattern,
                string alternateExternalPattern,
                List<string> expandLabels,
                int pass) :
            base(label, classifications)
        {
            Inflectors = inflectors;
            InflectionSource = inflectionSource;
            Inherit = inherit;
            Iterate = iterate;
            IterateIrregulars = iterateIrregulars;
            Exclude = exclude;
            Pattern = pattern;
            ExternalPattern = externalPattern;
            AlternateExternalPattern = alternateExternalPattern;
            ExpandLabels = expandLabels;
            Pass = pass;
        }

        public InflectorFamily(XElement element)
        {
            OnElement(element);
            DefaultLabelCheck();
        }

        public InflectorFamily(InflectorFamily other) :
            base(other)
        {
            CopyInflectorFamily(other);
        }

        public InflectorFamily()
        {
            ClearInflectorFamily();
        }

        public void ClearInflectorFamily()
        {
            Inflectors = null;
            InflectionSource = null;
            Inherit = null;
            Iterate = null;
            IterateIrregulars = false;
            Exclude = null;
            Pattern = null;
            ExternalPattern = null;
            AlternateExternalPattern = null;
            ExpandLabels = null;
            Pass = 1;
        }

        public void CopyInflectorFamily(InflectorFamily other)
        {
            Inflectors = other.CloneInflectors();
            InflectionSource = other.InflectionSource;
            Inherit = other.Inherit;
            Iterate = other.Iterate;
            IterateIrregulars = other.IterateIrregulars;
            Exclude = CloneExclude();
            Pattern = other.ClonePattern();
            ExternalPattern = other.ExternalPattern;
            AlternateExternalPattern = other.AlternateExternalPattern;
            ExpandLabels = other.CloneExpandLabels();
            Pass = other.Pass;
        }

        public Designator Designator
        {
            get
            {
                return new Designator(this);
            }
        }

        public bool HasExternalPattern
        {
            get
            {
                return !String.IsNullOrEmpty(ExternalPattern);
            }
        }

        public bool IsInherit()
        {
            if (!String.IsNullOrEmpty(Inherit))
                return true;

            return false;
        }

        public bool IsIterate()
        {
            if (!String.IsNullOrEmpty(Iterate))
                return true;

            return false;
        }

        public bool HasInflector(string label)
        {
            if (Inflectors == null)
                return false;

            Inflector inflector = Inflectors.FirstOrDefault(x => x.Label == label);

            return inflector != null;
        }

        public Inflector GetInflector(string label)
        {
            if (Inflectors == null)
                return null;

            Inflector inflector = Inflectors.FirstOrDefault(x => x.Label == label);

            return inflector;
        }

        public void AppendInflector(Inflector inflector)
        {
            if (Inflectors == null)
                Inflectors = new List<Inflector>() { inflector };
            else
                Inflectors.Add(inflector);
        }

        public void InsertAlternateInflector(Inflector inflector, Designator baseDesignator)
        {
            if (Inflectors == null)
                Inflectors = new List<Inflector>() { inflector };
            else
            {
                int count = Inflectors.Count();
                int index;

                for (index = 0; index < count; index++)
                {
                    Inflector testInflector = Inflectors[index];

                    if (testInflector.Label == baseDesignator.Label)
                        break;
                }

                if (index == count)
                    Inflectors.Add(inflector);
                else
                {
                    for (index = index + 1; index < count; index++)
                    {
                        Inflector testInflector = Inflectors[index];

                        if (!testInflector.Label.StartsWith(baseDesignator.Label))
                            break;
                    }

                    if (index == count)
                        Inflectors.Add(inflector);
                    else
                        Inflectors.Insert(index, inflector);
                }
            }
        }

        public int InflectorCount()
        {
            if (Inflectors == null)
                return 0;

            return Inflectors.Count();
        }

        public List<Inflector> CloneInflectors()
        {
            if (Inflectors == null)
                return null;

            List<Inflector> inflectors = new List<Inflector>();

            foreach (Inflector inflector in Inflectors)
                inflectors.Add(new Inflector(inflector));

            return inflectors;
        }

        public bool HasAnyExclude()
        {
            if ((Exclude == null) || (Exclude.Count() == 0))
                return false;

            return true;
        }

        public bool HasExclude(Designator designator)
        {
            if ((Exclude == null) || (Exclude.Count() == 0))
                return false;

            if (Exclude.FirstOrDefault(x => x.Match(designator)) != null)
                return true;

            return false;
        }

        public Designator GetExclude(string label)
        {
            if (Exclude == null)
                return null;

            Designator designator = Exclude.FirstOrDefault(x => x.Label == label);

            return designator;
        }

        public void AppendExclude(Designator designator)
        {
            if (Exclude == null)
                Exclude = new List<Designator>() { designator };
            else
                Exclude.Add(designator);
        }

        public int ExcludeCount()
        {
            if (Exclude == null)
                return 0;

            return Exclude.Count();
        }

        public List<Designator> CloneExclude()
        {
            if (Exclude == null)
                return null;

            List<Designator> exclude = new List<Designator>();

            foreach (Designator designator in Exclude)
                exclude.Add(new Designator(designator));

            return exclude;
        }

        public bool HasAnyPronoun()
        {
            if (Pattern == null)
                return false;

            if ((Pattern.FirstOrDefault(x => x.Type == "Pronoun") != null) ||
                    (Pattern.FirstOrDefault(x => x.Type == "ImplicitPronoun") != null))
                return true;

            return false;
        }

        public bool HasAnyPolarizer()
        {
            if (Pattern == null)
                return false;

            if (Pattern.FirstOrDefault(x => x.Type == "Polarizer") != null)
                return true;

            return false;
        }

        public bool HasPattern()
        {
            if ((Pattern == null) || (Pattern.Count() == 0))
                return false;

            return true;
        }

        public List<WordToken> ClonePattern()
        {
            if (Pattern == null)
                return null;

            List<WordToken> pattern = new List<WordToken>();

            foreach (WordToken wordToken in Pattern)
                pattern.Add(new WordToken(wordToken));

            return pattern;
        }

        public List<string> CloneExpandLabels()
        {
            if (ExpandLabels == null)
                return null;

            return new List<string>(ExpandLabels);
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (Pass != 0)
                element.Add(new XAttribute("Pass", Pass.ToString()));

            if (Inflectors != null)
            {
                foreach (Inflector inflectorEntry in Inflectors)
                {
                    XElement inflectorEntryElement = inflectorEntry.GetElement("Inflector");
                    element.Add(inflectorEntryElement);
                }
            }

            if (!String.IsNullOrEmpty(InflectionSource))
                element.Add(new XElement("Source", InflectionSource));

            if (!String.IsNullOrEmpty(Inherit))
                element.Add(new XElement("Inherit", Inherit));

            if (!String.IsNullOrEmpty(Iterate))
                element.Add(new XElement("Iterate", Iterate));

            if (IterateIrregulars)
                element.Add(new XElement("IterateIrregulars", true));

            if (Exclude != null)
            {
                foreach (Designator designator in Exclude)
                {
                    XElement excludeElement = designator.GetElement("Exclude");
                    element.Add(excludeElement);
                }
            }

            if ((Pattern != null) && (Pattern.Count() != 0))
            {
                XElement patternElement = new XElement("Pattern");

                foreach (WordToken wordToken in Pattern)
                    patternElement.Add(wordToken.GetElement("Token"));

                element.Add(patternElement);
            }

            if (!String.IsNullOrEmpty(ExternalPattern))
                element.Add(new XElement("ExternalPattern", ExternalPattern));

            if (!String.IsNullOrEmpty(AlternateExternalPattern))
                element.Add(new XElement("AlternateExternalPattern", AlternateExternalPattern));

            if (ExpandLabels != null)
            {
                foreach (string expandLabel in ExpandLabels)
                    element.Add(new XElement("Expand", expandLabel));
            }

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Pass":
                    Pass = ObjectUtilities.GetIntegerFromString(attributeValue, 0);
                    break;
                default:
                    return base.OnAttribute(attribute);
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
                        AppendInflector(inflector);
                    }
                    break;
                case "Source":
                    InflectionSource = childElement.Value.Trim();
                    break;
                case "Inherit":
                    Inherit = childElement.Value.Trim();
                    break;
                case "Iterate":
                    Iterate = childElement.Value.Trim();
                    break;
                case "IterateIrregulars":
                    IterateIrregulars = (childElement.Value.Trim().ToLower() == "true");
                    break;
                case "Exclude":
                    {
                        Designator designator = new Designator(childElement);
                        AppendExclude(designator);
                    }
                    break;
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
                case "ExternalPattern":
                    ExternalPattern = childElement.Value.Trim();
                    break;
                case "AlternateExternalPattern":
                    AlternateExternalPattern = childElement.Value.Trim();
                    break;
                case "Expand":
                    if (ExpandLabels == null)
                        ExpandLabels = new List<string>();
                    ExpandLabels.Add(childElement.Value.Trim());
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
