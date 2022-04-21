using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Dictionary
{
    public class ProbableMeaning
    {
        public string Meaning { get; set; }
        public string Note { get; set; }
        public LexicalCategory Category { get; set; }
        public string CategoryString { get; set; }
        public List<LexicalAttribute> Attributes { get; set; }
        public float Probability { get; set; }      // 1.0 (most probable) - 0.0 (least probable)
        public int Frequency { get; set; }
        public bool IsFiltered { get; set; }
        public List<int> SourceIDs { get; set; }
        protected List<Inflection> _Inflections;       // not saved.

        public ProbableMeaning(
            string meaning,
            string note,
            LexicalCategory category,
            string categoryString,
            List<LexicalAttribute> attributes,
            float probability,
            int frequency,
            List<int> sourceIDs,
            List<Inflection> inflections)
        {
            Meaning = meaning;
            Note = String.Empty;
            Category = category;
            CategoryString = categoryString;
            Attributes = attributes;
            Probability = probability;
            Frequency = frequency;
            IsFiltered = false;

            if (sourceIDs != null)
                SourceIDs = new List<int>(sourceIDs);
            else
                SourceIDs = new List<int>();

            Inflections = inflections;
        }

        public ProbableMeaning(
            string meaning,
            string note,
            LexicalCategory category,
            string categoryString,
            List<LexicalAttribute> attributes,
            float probability,
            int frequency,
            int sourceID,
            List<Inflection> inflections)
        {
            Meaning = meaning;
            Note = String.Empty;
            Category = category;
            CategoryString = categoryString;
            Attributes = null;
            Probability = probability;
            Frequency = frequency;
            IsFiltered = false;

            if (sourceID != 0)
                SourceIDs = new List<int>() { sourceID };
            else
                SourceIDs = new List<int>();

            Inflections = inflections;
        }

        public ProbableMeaning(
            string meaning,
            LexicalCategory category,
            string categoryString,
            float probability,
            int frequency,
            List<int> sourceIDs)
        {
            Meaning = meaning;
            Note = String.Empty;
            Category = category;
            CategoryString = categoryString;
            Attributes = null;
            Probability = probability;
            Frequency = frequency;
            IsFiltered = false;

            if (sourceIDs != null)
                SourceIDs = new List<int>(sourceIDs);
            else
                SourceIDs = new List<int>();

            Inflections = null;
        }

        public ProbableMeaning(
            string meaning,
            LexicalCategory category,
            string categoryString,
            float probability,
            int frequency,
            int sourceID)
        {
            Meaning = meaning;
            Note = String.Empty;
            Category = category;
            CategoryString = categoryString;
            Attributes = null;
            Probability = probability;
            Frequency = frequency;
            IsFiltered = false;

            if (sourceID != 0)
                SourceIDs = new List<int>() { sourceID };
            else
                SourceIDs = new List<int>();

            Inflections = null;
        }

        public ProbableMeaning(string meaning)
        {
            ClearProbableMeaning();
            Meaning = meaning;
        }

        public ProbableMeaning(ProbableMeaning other)
        {
            CopyProbableMeaning(other);
        }

        public ProbableMeaning(XElement element)
        {
            OnElement(element);
        }

        public ProbableMeaning()
        {
            ClearProbableMeaning();
        }

        public void ClearProbableMeaning()
        {
            Meaning = String.Empty;
            Note = String.Empty;
            Category = LexicalCategory.Unknown;
            CategoryString = String.Empty;
            Attributes = null;
            Probability = float.NaN;
            Frequency = 0;
            IsFiltered = false;
            SourceIDs = new List<int>();
            Inflections = null;
        }

        public void CopyProbableMeaning(ProbableMeaning other)
        {
            Meaning = other.Meaning;
            Note = other.Note;
            Category = other.Category;
            CategoryString = other.CategoryString;
            Attributes = null;
            Probability = other.Probability;
            Frequency = other.Frequency;
            IsFiltered = other.IsFiltered;
            SourceIDs = new List<int>(other.SourceIDs);
            Inflections = other.CloneInflections();
        }

        public bool IsPhrase()
        {
            if (String.IsNullOrEmpty(Meaning))
                return false;

            if (Meaning.Contains(" "))
                return true;

            return false;
        }

        public bool IsPhrase(int maxWords)
        {
            if (String.IsNullOrEmpty(Meaning))
                return false;

            int count = TextUtilities.CountChars(Meaning, ' ');

            if ((count == 0) || (count > (maxWords - 1)))
                return false;

            return true;
        }

        public bool MatchMeaning(string meaning)
        {
            return TextUtilities.IsEqualStringsIgnoreCase(meaning, Meaning);
        }

        public bool IsCompatible(ProbableMeaning other)
        {
            if (!TextUtilities.IsEqualStringsIgnoreCase(other.Meaning, Meaning))
                return false;

            if ((other.Category != Category) && (other.Category != LexicalCategory.Unknown) && (Category != LexicalCategory.Unknown))
                return false;

            if ((other.CategoryString != CategoryString) && !String.IsNullOrEmpty(other.CategoryString) && !String.IsNullOrEmpty(CategoryString))
                return false;

            return true;
        }

        public void Merge(ProbableMeaning other)
        {
            if (!TextUtilities.IsEqualStringsIgnoreCase(Meaning, other.Meaning))
                throw new Exception("In ProbableMeaning.Merge, the Meaning's don't match: "
                    + Meaning + " and " + other.Meaning);

            MergeProbability(other.Probability);
            MergeFrequency(other.Frequency);
            MergeIsFiltered(other.IsFiltered);
            MergeCategory(other.Category);
            MergeCategoryString(other.CategoryString);
            MergeAttributes(other.Attributes);
            MergeSourceIDs(other.SourceIDs);
            MergeInflections(other.Inflections);
        }

        public void MergeProbability(float probability)
        {
            if (!float.IsNaN(probability))
            {
                if (float.IsNaN(Probability) || (probability > Probability))
                    Probability = probability;
            }
        }

        public void MergeFrequency(int frequency)
        {
            if (frequency > Frequency)
                Frequency = frequency;
        }

        public void MergeIsFiltered(bool isFiltered)
        {
            IsFiltered = (IsFiltered && isFiltered);
        }

        public void MergeCategory(LexicalCategory category)
        {
            if (Category != category)
            {
                if (Category == LexicalCategory.Unknown)
                    Category = category;
                else if (category == LexicalCategory.Inflection)
                    Category = category;
                else if (category != LexicalCategory.Unknown)
                    throw new Exception("In ProbableMeaning.MergeCategory, the category's are not compatible: "
                        + Category.ToString() + " and " + category.ToString());
            }
        }

        public void MergeCategoryString(string categoryString)
        {
            if (CategoryString != categoryString)
            {
                if (String.IsNullOrEmpty(CategoryString))
                    CategoryString = categoryString;
                else if (!String.IsNullOrEmpty(categoryString))
                    CategoryString += "/" + Category;
                    //throw new Exception("In ProbableMeaning.MergeCategoryString, the CategoryString's are not compatible: "
                    //    + CategoryString + " and " + categoryString);
            }
        }

        public void MergeAttributes(List<LexicalAttribute> attributes)
        {
            if ((attributes == null) || (attributes.Count() == 0))
                return;

            if ((Attributes == null) || (Attributes.Count() == 0))
            {
                Attributes = new List<LexicalAttribute>(attributes);
                return;
            }

            List<LexicalAttribute> newAttributes = ObjectUtilities.ListConcatenateUnique(Attributes, attributes);

            foreach (LexicalAttribute attribute in newAttributes)
            {
                switch (attribute)
                {
                    case LexicalAttribute.None:
                        break;
                        /*
                    case LexicalAttribute.Masculine:
                        if (newAttributes.Contains(LexicalAttribute.Feminine) ||
                            newAttributes.Contains(LexicalAttribute.Neuter))
                        {
                            throw new Exception("In ProbableMeaning.MergeAttributes, the gender attributes are not compatible: "
                                + GetStringFromAttributes(Attributes) + " and " + GetStringFromAttributes(attributes));
                        }
                        break;
                    case LexicalAttribute.Feminine:
                        if (newAttributes.Contains(LexicalAttribute.Masculine) ||
                            newAttributes.Contains(LexicalAttribute.Neuter))
                        {
                            throw new Exception("In ProbableMeaning.MergeAttributes, the gender attributes are not compatible: "
                                + GetStringFromAttributes(Attributes) + " and " + GetStringFromAttributes(attributes));
                        }
                        break;
                    case LexicalAttribute.Neuter:
                        if (newAttributes.Contains(LexicalAttribute.Masculine) ||
                            newAttributes.Contains(LexicalAttribute.Feminine))
                        {
                            throw new Exception("In ProbableMeaning.MergeAttributes, the gender attributes are not compatible: "
                                + GetStringFromAttributes(Attributes) + " and " + GetStringFromAttributes(attributes));
                        }
                        break;
                        */
                    case LexicalAttribute.Singular:
                        if (newAttributes.Contains(LexicalAttribute.Plural) ||
                            newAttributes.Contains(LexicalAttribute.Dual) ||
                            newAttributes.Contains(LexicalAttribute.Uncountable))
                        {
                            throw new Exception("In ProbableMeaning.MergeAttributes, the number attributes are not compatible: "
                                + GetStringFromAttributes(Attributes) + " and " + GetStringFromAttributes(attributes));
                        }
                        break;
                    case LexicalAttribute.Plural:
                        if (newAttributes.Contains(LexicalAttribute.Singular) ||
                            newAttributes.Contains(LexicalAttribute.Dual) ||
                            newAttributes.Contains(LexicalAttribute.Uncountable))
                        {
                            throw new Exception("In ProbableMeaning.MergeAttributes, the number attributes are not compatible: "
                                + GetStringFromAttributes(Attributes) + " and " + GetStringFromAttributes(attributes));
                        }
                        break;
                    case LexicalAttribute.Dual:
                        if (newAttributes.Contains(LexicalAttribute.Singular) ||
                            newAttributes.Contains(LexicalAttribute.Plural) ||
                            newAttributes.Contains(LexicalAttribute.Uncountable))
                        {
                            throw new Exception("In ProbableMeaning.MergeAttributes, the number attributes are not compatible: "
                                + GetStringFromAttributes(Attributes) + " and " + GetStringFromAttributes(attributes));
                        }
                        break;
                    case LexicalAttribute.Uncountable:
                        if (newAttributes.Contains(LexicalAttribute.Singular) ||
                            newAttributes.Contains(LexicalAttribute.Plural) ||
                            newAttributes.Contains(LexicalAttribute.Dual))
                        {
                            throw new Exception("In ProbableMeaning.MergeAttributes, the number attributes are not compatible: "
                                + GetStringFromAttributes(Attributes) + " and " + GetStringFromAttributes(attributes));
                        }
                        break;
                }
            }
        }

        public void MergeSourceIDs(List<int> sourceIDs)
        {
            if ((sourceIDs == null) || (sourceIDs == SourceIDs) || (sourceIDs.Count() == 0))
                return;
            if (SourceIDs == null)
                SourceIDs = new List<int>();
            foreach (int sourceID in sourceIDs)
            {
                if (!SourceIDs.Contains(sourceID))
                    SourceIDs.Add(sourceID);
            }
        }

        public int SourceIDCount()
        {
            if (SourceIDs != null)
                return SourceIDs.Count();
            return 0;
        }

        public int SourceIDCountWithExclusion(List<int> excludeIDs)
        {
            if ((SourceIDs == null) || (SourceIDs.Count() == 0))
                return 0;

            if ((excludeIDs == null) || (excludeIDs.Count() == 0))
                return SourceIDCount();

            int count = 0;

            foreach (int sourceID in SourceIDs)
            {
                if (excludeIDs.Contains(sourceID))
                    continue;

                count++;
            }

            return count;
        }

        public bool HasSourceIDs()
        {
            if ((SourceIDs != null) && (SourceIDs.Count() != 0))
                return true;
            return false;
        }

        public bool HasSourceID(int id)
        {
            if ((SourceIDs != null) && (SourceIDs.Count() != 0))
                return SourceIDs.Contains(id);
            return false;
        }

        public string GetSourceNames()
        {
            return ApplicationData.DictionarySourcesLazy.GetByIDList(SourceIDs);
        }

        public string GetSourceNamesWithExclusion(List<int> excludeIDs)
        {
            return ApplicationData.DictionarySourcesLazy.GetByIDListWithExclusion(SourceIDs, excludeIDs);
        }

        public bool IsInflection()
        {
            if (Category == LexicalCategory.Inflection)
                return true;

            return false;
        }

        public List<Inflection> Inflections
        {
            get
            {
                return _Inflections;
            }
            set
            {
                _Inflections = value;
            }
        }

        public Inflection FindRegularInflection(
            string inflected,
            LanguageID languageID)
        {
            if (_Inflections == null)
                return null;

            Inflection inflection = _Inflections.FirstOrDefault(
                x => TextUtilities.IsEqualStringsIgnoreCase(x.GetRegularOutput(languageID), inflected));

            return inflection;
        }

        public Inflection FindRegularPronounInflection(
            string inflected,
            LanguageID languageID)
        {
            if (_Inflections == null)
                return null;

            Inflection inflection = _Inflections.FirstOrDefault(
                x => TextUtilities.IsEqualStringsIgnoreCase(x.GetRegularPronounOutput(languageID), inflected));

            return inflection;
        }

        public Inflection FindOutputInflection(
            string inflected,
            LanguageID languageID)
        {
            if (_Inflections == null)
                return null;

            Inflection inflection = _Inflections.FirstOrDefault(
                x => TextUtilities.IsEqualStringsIgnoreCase(x.GetOutput(languageID), inflected));

            return inflection;
        }

        public Inflection FindInflection(string label)
        {
            if (_Inflections == null)
                return null;

            Inflection inflection = _Inflections.FirstOrDefault(x => x.Label == label);

            return inflection;
        }

        public Inflection GetInflectionIndexed(int index)
        {
            if (_Inflections == null)
                return null;

            if ((index >= 0) && (index < _Inflections.Count()))
                return _Inflections[index];

            return null;
        }

        public void AddInflection(Inflection inflection)
        {
            if (_Inflections == null)
                _Inflections = new List<Inflection>();

            _Inflections.Add(inflection);
        }

        public void MergeInflections(List<Inflection> inflections)
        {
            if (inflections == null)
                return;

            if (_Inflections == null)
            {
                _Inflections = new List<Inflection>(inflections);
                return;
            }

            if (_Inflections.Count() == 0)
                _Inflections.AddRange(inflections);
            else
            {
                foreach (Inflection inflection in inflections)
                {
                    if (_Inflections.FirstOrDefault(x => x.QuickCompare(inflection) == 0) == null)
                        _Inflections.Add(inflection);
                }
            }
        }

        public int InflectionCount()
        {
            if (_Inflections == null)
                return 0;

            return _Inflections.Count();
        }

        public bool HasInflections()
        {
            if ((_Inflections == null) || (_Inflections.Count() == 0))
                return false;

            return true;
        }

        public List<Inflection> CloneInflections()
        {
            List<Inflection> newInflections = null;

            if ((_Inflections != null) && (_Inflections.Count() != 0))
            {
                newInflections = new List<Inflection>();

                foreach (Inflection inflection in _Inflections)
                    newInflections.Add(new Inflection(inflection));
            }

            return newInflections;
        }

        public string GetInflectionsDisplay()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("I(");

            if ((_Inflections != null) && (_Inflections.Count() != 0))
            {
                bool first = true;

                foreach (Inflection inflection in _Inflections)
                {
                    if (first)
                        first = false;
                    else
                        sb.Append(", ");

                    sb.Append(inflection.Label);
                }
            }

            sb.Append(")");

            return sb.ToString();
        }

        public static string GetStringFromAttributes(List<LexicalAttribute> attributes)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;

            if ((attributes == null) || (attributes.Count() == 0))
                return String.Empty;

            foreach (LexicalAttribute attribute in attributes)
            {
                if (first)
                    first = false;
                else
                    sb.Append(", ");

                sb.Append(attribute.ToString());
            }

            return sb.ToString();
        }

        public static List<LexicalAttribute> GetAttributesFromString(string value)
        {
            if (String.IsNullOrEmpty(value))
                return null;

            List<LexicalAttribute> attributes = new List<LexicalAttribute>();
            string[] parts = value.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
                attributes.Add(GetAttributeFromString(part.Trim()));

            return attributes;
        }

        public static LexicalAttribute GetAttributeFromString(string value)
        {
            if (String.IsNullOrEmpty(value))
                return LexicalAttribute.None;

            LexicalAttribute attribute;

            switch (value)
            {
                case "None":
                    attribute = LexicalAttribute.None;
                    break;
                case "Masculine":
                    attribute = LexicalAttribute.Masculine;
                    break;
                case "Feminine":
                    attribute = LexicalAttribute.Feminine;
                    break;
                case "Neuter":
                    attribute = LexicalAttribute.Neuter;
                    break;
                case "Singular":
                    attribute = LexicalAttribute.Singular;
                    break;
                case "Plural":
                    attribute = LexicalAttribute.Plural;
                    break;
                case "Dual":
                    attribute = LexicalAttribute.Dual;
                    break;
                case "Uncountable":
                    attribute = LexicalAttribute.Uncountable;
                    break;
                default:
                    throw new Exception("GetAttributeFromString: Unknown attribute: " + value);
            }

            return attribute;
        }

        public void GetDumpString(string prefix, StringBuilder sb)
        {
            sb.AppendLine(prefix + ToString());
        }

        public override string ToString()
        {
            return "Meaning: " + Meaning + ", "
                + Note + ", "
                + Category.ToString() + ", "
                + (CategoryString != null ? CategoryString : String.Empty) + ", "
                + GetStringFromAttributes(Attributes) + ", "
                + GetInflectionsDisplay() + ", "
                + Probability.ToString() + ", "
                + Frequency.ToString() + ", "
                + IsFiltered.ToString() + ", "
                + SourceIDsString;
        }

        public string SourceIDsString
        {
            get
            {
                if (SourceIDs.Count() == 0)
                    return String.Empty;
                else if (SourceIDs.Count() == 1)
                    return ApplicationData.DictionarySourcesLazy.GetByID(SourceIDs[0]);
                else
                {
                    string str = String.Empty;

                    foreach (int source in SourceIDs)
                    {
                        string name = ApplicationData.DictionarySourcesLazy.GetByID(source);

                        if (!String.IsNullOrEmpty(str))
                            str += ",";

                        str += name;
                    }

                    return str;
                }
            }
        }

        public virtual XElement GetElement(string name)
        {
            XElement element = new XElement(name);
            element.Add(new XElement("Meaning", Meaning));
            if (!String.IsNullOrEmpty(Note))
                element.Add(new XElement("Note", Note));
            element.Add(new XElement("Category", Category.ToString()));
            if (!String.IsNullOrEmpty(CategoryString))
                element.Add(new XElement("CategoryString", CategoryString));
            if ((Attributes != null) && (Attributes.Count() != 0))
                element.Add(new XElement("Attributes", GetStringFromAttributes(Attributes)));
            if (!float.IsNaN(Probability))
                element.Add(new XElement("Probability", Probability));
            if (Frequency != 0)
                element.Add(new XElement("Frequency", Frequency));
            if (IsFiltered)
                element.Add(new XElement("IsFiltered", IsFiltered));
            if ((SourceIDs != null) && (SourceIDs.Count() != 0))
                element.Add(new XElement("SourceIDs", ObjectUtilities.GetStringFromIntList(SourceIDs)));
            return element;
        }

        public bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Meaning":
                    Meaning = childElement.Value.Trim();
                    break;
                case "Note":
                    Note = childElement.Value.Trim();
                    break;
                case "Category":
                    Category = Sense.GetLexicalCategoryFromString(childElement.Value.Trim());
                    break;
                case "CategoryString":
                    CategoryString = childElement.Value.Trim();
                    break;
                case "Attributes":
                    Attributes = GetAttributesFromString(childElement.Value.Trim());
                    break;
                case "Probability":
                    Probability = ObjectUtilities.GetFloatFromString(childElement.Value.Trim(), float.NaN);
                    break;
                case "Frequency":
                    Frequency = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), 0);
                    break;
                case "IsFiltered":
                    IsFiltered = ObjectUtilities.GetBoolFromString(childElement.Value.Trim(), false);
                    break;
                case "SourceIDs":
                    SourceIDs = ObjectUtilities.GetIntListFromString(childElement.Value.Trim());
                    break;
                default:
                    return false;
            }

            return true;
        }

        public void OnElement(XElement element)
        {
            ClearProbableMeaning();

            foreach (var childNode in element.Elements())
            {
                XElement childElement = childNode as XElement;

                if (childElement == null)
                    continue;

                if (!OnChildElement(childElement))
                    throw new ObjectException("Unknown child element " + childElement.Name + " in " + element.Name.ToString() + ".");
            }
        }

        public bool MatchMeaningIgnoreCase(ProbableMeaning other)
        {
            return TextUtilities.IsEqualStringsIgnoreCase(other.Meaning, Meaning);
        }

        public bool MatchMeaningIgnoreCase(string meaning)
        {
            return TextUtilities.IsEqualStringsIgnoreCase(meaning, Meaning);
        }

        public bool Match(ProbableMeaning other)
        {
            if (!TextUtilities.IsEqualStringsIgnoreCase(other.Meaning, Meaning))
                return false;

            if (!TextUtilities.IsEqualStringsIgnoreCase(other.Note, Note))
                return false;

            if (Category != other.Category)
                return false;

            if (CategoryString != other.CategoryString)
                return false;

            if ((Attributes != null) && (other.Attributes != null))
            {
                if (ObjectUtilities.CompareIntLists(Attributes.Cast<int>().ToList(), other.Attributes.Cast<int>().ToList()) != 0)
                    return false;
            }

            if (Frequency != other.Frequency)
                return false;

            if (IsFiltered != other.IsFiltered)
                return false;

            if (Probability != other.Probability)
                return false;

            if (ObjectUtilities.CompareIntLists(SourceIDs, other.SourceIDs) != 0)
                return false;

            return true;
        }

        public int Compare(ProbableMeaning other)
        {
            int diff = String.Compare(Meaning, other.Meaning);
            if (diff != 0)
                return diff;
            diff = String.Compare(Note, other.Note);
            if (diff != 0)
                return diff;
            diff = (int)Category - (int)other.Category;
            if (diff != 0)
                return diff;
            if ((Attributes != null) && (other.Attributes != null))
            {
                diff = ObjectUtilities.CompareIntLists(Attributes.Cast<int>().ToList(), other.Attributes.Cast<int>().ToList());
                if (diff != 0)
                    return diff;
            }
            if (CategoryString != other.CategoryString)
            {
                if ((CategoryString != null) && (other.CategoryString != null))
                {
                    diff = String.Compare(CategoryString, other.CategoryString);
                    if (diff != 0)
                        return diff;
                }
                else if (String.IsNullOrEmpty(CategoryString) != String.IsNullOrEmpty(other.CategoryString))
                {
                    if (String.IsNullOrEmpty(CategoryString))
                        return -1;
                    else
                        return 1;
                }
            }
            if (Frequency == other.Frequency)
            {
                if (Probability == other.Probability)
                    return ObjectUtilities.CompareIntLists(SourceIDs, other.SourceIDs);
                else if (!float.IsNaN(Probability) && (Probability < other.Probability))
                    return 1;
                else
                    return -1;
            }
            else if (Frequency < other.Frequency)
                return 1;
            else
                return -1;
        }
    }
}
