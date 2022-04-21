using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    public class DesignatorTable : BaseObjectKeyed
    {
        public string Scope;    // "All", "Default", "Both"
        public List<Designator> Designators;

        public DesignatorTable(
            string name,
            string scope,
            List<Designator> designators) : base(name)
        {
            Scope = scope;
            Designators = designators;
        }

        public DesignatorTable(DesignatorTable other) : base(other)
        {
            Scope = other.Scope;

            if (other.Designators != null)
                Designators = new List<Designator>(other.Designators);
            else
                Designators = null;
        }

        public DesignatorTable(XElement element)
        {
            Designators = new List<Designator>();
            OnElement(element);
        }

        public DesignatorTable()
        {
            Scope = String.Empty;
            Designators = new List<Designator>();
        }

        public Designator GetDesignator(string label)
        {
            if (Designators == null)
                return null;

            Designator designator = Designators.FirstOrDefault(x => x.Label == label);

            return designator;
        }

        public Designator GetDesignatorIndexed(int index)
        {
            if (Designators == null)
                return null;

            if ((index < 0) || (index >= Designators.Count()))
                return null;

            Designator designator = Designators[index];

            return designator;
        }

        public void Add(Designator designator)
        {
            if (Designators == null)
                Designators = new List<Designator>();
            Designators.Add(designator);
        }

        public void Add(string keyLabel, string value)
        {
            Add(new Designator(keyLabel, value));
        }

        // Mood: Indicative, Subjunctive, Conditional, Imperative
        // Tense: Past, Present, Future
        // Aspect: Perfect, Imperfect (Imperfect1, Imperfect2)
        // Alternate: Alternate1, Alternate2, ...
        // Gender: Masculine, Feminine
        // Polarity: Positive, Negative
        // Person: First, Second, Third
        // Number: Singular, Plural
        // Special: Infinitive, Gerund, Present Participle, Past Participle
        public void Add(
            string mood,
            string tense,
            string aspect,
            string gender,
            string polarity,
            string person,
            string number,
            string special)
        {
            Designator designator = new Designator();
            string label = String.Empty;

            if (!String.IsNullOrEmpty(mood))
            {
                designator.AppendClassification("Mood", mood);

                if (!String.IsNullOrEmpty(label))
                    label += " ";

                label += mood;
            }

            if (!String.IsNullOrEmpty(tense))
            {
                designator.AppendClassification("Tense", tense);

                if (!String.IsNullOrEmpty(label))
                    label += " ";

                label += tense;
            }

            if (!String.IsNullOrEmpty(aspect))
            {
                designator.AppendClassification("Aspect", aspect);

                if (!String.IsNullOrEmpty(label))
                    label += " ";

                label += aspect;
            }

            if (!String.IsNullOrEmpty(polarity))
            {
                designator.AppendClassification("Polarity", polarity);

                if (!String.IsNullOrEmpty(label))
                    label += " ";

                label += polarity;
            }

            if (!String.IsNullOrEmpty(person))
            {
                designator.AppendClassification("Person", person);

                if (!String.IsNullOrEmpty(label))
                    label += " ";

                label += person;
            }

            if (!String.IsNullOrEmpty(gender))
            {
                designator.AppendClassification("Gender", gender);

                if (!String.IsNullOrEmpty(label))
                    label += " ";

                label += gender;
            }

            if (!String.IsNullOrEmpty(number))
            {
                designator.AppendClassification("Number", number);

                if (!String.IsNullOrEmpty(label))
                    label += " ";

                label += number;
            }

            if (!String.IsNullOrEmpty(special))
            {
                designator.AppendClassification("Special", special);

                if (!String.IsNullOrEmpty(label))
                    label += " ";

                label += special;
            }

            Add(designator);
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);
            element.Add(new XAttribute("Name", KeyString));
            if (!String.IsNullOrEmpty(Scope))
                element.Add(new XAttribute("Scope", Scope));
            if (Designators != null)
            {
                foreach (Designator designator in Designators)
                {
                    XElement designatorElement = designator.Xml;
                    element.Add(designatorElement);
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
                default:
                    return OnUnknownAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Designator":
                    Add(new Designator(childElement));
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
