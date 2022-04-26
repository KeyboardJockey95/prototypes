using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    // Key is dictionary form of inflected word.
    public class DeinflectionInstance
    {
        public string DictionaryForm { get; set; }
        public LexicalCategory Category { get; set; }
        public string CategoryString { get; set; }
        public int InflectionLabelID { get; set; }

        public DeinflectionInstance(
            string dictionaryForm,
            LexicalCategory category,
            string categoryString,
            int inflectionLabelID)
        {
            DictionaryForm = dictionaryForm;
            Category = category;
            CategoryString = categoryString;
            InflectionLabelID = inflectionLabelID;
        }

        public DeinflectionInstance(DeinflectionInstance other)
        {
            DictionaryForm = other.DictionaryForm;
            Category = other.Category;
            CategoryString = other.CategoryString;
            InflectionLabelID = other.InflectionLabelID;
        }

        public DeinflectionInstance(XElement element)
        {
            DictionaryForm = element.Attribute("D").Value;
            Category = Sense.GetLexicalCategoryFromString(element.Attribute("C").Value);
            CategoryString = element.Attribute("S").Value;
            InflectionLabelID = ObjectUtilities.GetIntegerFromString(element.Attribute("I").Value, 0);
        }

        public DeinflectionInstance()
        {
            DictionaryForm = null;
            InflectionLabelID = 0;
        }

        public string InflectionLabel
        {
            get
            {
                if (InflectionLabelID > 0)
                    return ApplicationData.InflectionLabelsLazy.GetByID(InflectionLabelID);
                else
                    return null;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    InflectionLabelID = 0;
                else
                {
                    InflectionLabelID = ApplicationData.InflectionLabelsLazy.GetOrAdd(value);
                }
            }
        }

        public override string ToString()
        {
            string returnValue = DictionaryForm + "|" + Category.ToString() + "|" + CategoryString + "|" + InflectionLabel;
            return returnValue;
        }

        public XElement GetElement(string name)
        {
            XElement element = new XElement(name);
            element.Add(new XAttribute("D", DictionaryForm));
            element.Add(new XAttribute("C", Category.ToString()));
            element.Add(new XAttribute("S", CategoryString));
            element.Add(new XAttribute("I", InflectionLabelID));
            return element;
        }

        public int Compare(DeinflectionInstance other)
        {
            if (other == null)
                return 1;

            int diff = String.Compare(DictionaryForm, other.DictionaryForm);

            if (diff != 0)
                return diff;

            diff = (int)Category - (int)other.Category;

            if (diff != 0)
                return diff;

            diff = String.Compare(CategoryString, other.CategoryString);

            if (diff != 0)
                return diff;

            diff = InflectionLabelID - other.InflectionLabelID;

            return diff;
        }
    }
}
