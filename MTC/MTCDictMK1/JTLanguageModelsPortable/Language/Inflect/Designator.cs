using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    public class Designator : BaseObjectKeyed
    {
        public enum CombineCode
        {
            Union,
            Intersect
        }

        protected List<Classifier> _Classifications;
        public static List<string> ClassificationKeys = new List<string>()
            {
                "Person",
                "Number",
                "Gender",
                "Tense",
                "Aspect",
                "Mood",
                "Voice",
                "Case",
                "Possession",
                "Definiteness",
                "Politeness",
                "Causativity",
                "Clusivity",
                "Interrogativity",
                "Transitivity",
                "Valency",
                "Polarity",
                "Telicity",
                "Volition",
                "Mirativity",
                "Evidentiality",
                "Animacy",
                "Associativity",
                "Pluractionality",
                "Reciprocity",
                "Agreement",
                "Polypersonal Agreement",
                "Incorporation",
                "Noun Class",
                "Noun Classifier",
                "Verb Classifier",
                "Special",
                "Alternate",
                "Period",
                "Contraction",
                "Supplemental",
                "ReflexivePerson",
                "ReflexiveNumber",
                "ReflexiveGender",
                "ReflexivePoliteness",
                "DirectPerson",
                "DirectNumber",
                "DirectGender",
                "DirectPoliteness",
                "IndirectPerson",
                "IndirectNumber",
                "IndirectGender",
                "IndirectPoliteness"
            };
        public static List<string> DefaultValues = new List<string>()
            {
                "Person", "Any",
                "Number", "Any",
                "Gender", "Any",
                "Tense", "Any",
                "Aspect", "None",
                "Mood", "None",
                "Voice", "None",
                "Case", "Any",
                "Possession", "Any",
                "Definiteness", "Any",
                "Politeness", "Any",
                "Causativity", "Any",
                "Clusivity", "Any",
                "Interrogativity", "Any",
                "Transitivity", "Any",
                "Valency", "Any",
                "Polarity", "Positive",
                "Telicity", "Any",
                "Volition", "Any",
                "Mirativity", "Any",
                "Evidentiality", "Any",
                "Animacy", "Any",
                "Associativity", "Any",
                "Pluractionality", "Any",
                "Reciprocity", "Any",
                "Agreement", "Any",
                "Polypersonal Agreement", "Any",
                "Incorporation", "Any",
                "Noun Class", "Any",
                "Noun Classifier", "Any",
                "Verb Classifier", "Any",
                "Special", "None",
                "Alternate", "Any",
                "Period", "Any",
                "Contraction", "Any",
                "Supplemental", "Any",
                "ReflexivePerson", "Any",
                "ReflexiveNumber", "Any",
                "ReflexiveGender", "Any",
                "ReflexivePoliteness", "Any",
                "DirectPerson", "Any",
                "DirectNumber", "Any",
                "DirectGender", "Any",
                "DirectPoliteness", "Any",
                "IndirectPerson", "Any",
                "IndirectNumber", "Any",
                "IndirectGender", "Any",
                "IndirectPoliteness", "Any"
            };
        protected static Dictionary<string, string> _DefaultValueDictionary = null;
        public static Dictionary<string, string> DefaultValueDictionary
        {
            get
            {
                if (_DefaultValueDictionary == null)
                {
                    _DefaultValueDictionary = new Dictionary<string, string>();

                    int index;
                    int count = DefaultValues.Count();

                    for (index = 0; index < count; index += 2)
                        _DefaultValueDictionary.Add(DefaultValues[index], DefaultValues[index + 1]);
                }

                return _DefaultValueDictionary;
            }
        }

        public Designator(string label) :
            base(label)
        {
            _Classifications = null;
        }

        public Designator(
                string label,
                List<Classifier> classifications) :
            base(label)
        {
            _Classifications = classifications;
            DefaultLabelCheck();
        }

        public Designator(
                string label,
                string[] classificationPairs) :
            base(label)
        {
            if (classificationPairs != null)
            {
                _Classifications = new List<Classifier>();
                int count = classificationPairs.Count();
                int index;
                for (index = 0; index < count; index += 2)
                {
                    string type = classificationPairs[index];
                    string value = classificationPairs[index + 1];
                    _Classifications.Add(new Classifier(type, value));
                }
            }
            else
                _Classifications = null;

            DefaultLabelCheck();
        }

        public Designator(
                string label,
                List<string> classificationPairs) :
            base(label)
        {
            if (classificationPairs != null)
            {
                _Classifications = new List<Classifier>();
                int count = classificationPairs.Count();
                int index;
                for (index = 0; index < count; index += 2)
                {
                    string type = classificationPairs[index];
                    string value = classificationPairs[index + 1];
                    _Classifications.Add(new Classifier(type, value));
                }
            }
            else
                _Classifications = null;

            DefaultLabelCheck();
        }

        public Designator(
                string label,
                Classifier singleClassification) :
            base(label)
        {
            _Classifications = new List<Classifier>() { singleClassification };
            DefaultLabelCheck();
        }

        public Designator(
                string label,
                Classifier doubleClassification1,
                Classifier doubleClassification2) :
            base(label)
        {
            _Classifications = new List<Classifier>() { doubleClassification1, doubleClassification2 };
            DefaultLabelCheck();
        }

        public Designator(
                string label,
                Classifier tripleClassification1,
                Classifier tripleClassification2,
                Classifier tripleClassification3) :
            base(label)
        {
            _Classifications = new List<Classifier>() { tripleClassification1, tripleClassification2, tripleClassification3 };
            DefaultLabelCheck();
        }

        public Designator(
                string label,
                Classifier quadClassification1,
                Classifier quadClassification2,
                Classifier quadClassification3,
                Classifier quadClassification4) :
            base(label)
        {
            _Classifications = new List<Classifier>() { quadClassification1, quadClassification2, quadClassification3, quadClassification4 };
            DefaultLabelCheck();
        }

        public Designator(
                string keyLabel,
                string value) :
            base(keyLabel)
        {
            _Classifications = new List<Classifier>()
            {
                new Classifier(keyLabel, value)
            };
            Label = keyLabel;
        }

        // Create intersected designator.
        public Designator(List<Designator> others)
        {
            ClearDesignator();
            _Classifications = IntersectClassifiers(others);
            Label = ComposeLabel();
            TouchAndClearModified();
        }

        // Create intersected or unioned designator.
        public Designator(Designator designator1, Designator designator2, CombineCode combineCode)
        {
            ClearDesignator();

            switch (combineCode)
            {
                case CombineCode.Union:
                    _Classifications = UnionClassifiers(designator1, designator2);
                    break;
                case CombineCode.Intersect:
                    _Classifications = IntersectClassifiers(designator1, designator2);
                    break;
            }

            Label = ComposeLabel();
            TouchAndClearModified();
        }

        // Create intersected or unioned designator.
        public Designator(Designator designator1, Designator designator2, Designator designator3, CombineCode combineCode)
        {
            ClearDesignator();

            switch (combineCode)
            {
                case CombineCode.Union:
                    _Classifications = UnionClassifiers(designator1, designator2, designator3);
                    break;
                case CombineCode.Intersect:
                    _Classifications = IntersectClassifiers(designator1, designator2, designator3);
                    break;
            }

            Label = ComposeLabel();
            TouchAndClearModified();
        }

        public Designator(Designator other) :
            base(other)
        {
            CopyDesignator(other);
        }

        public Designator(XElement element)
        {
            OnElement(element);
            DefaultLabelCheck();
        }

        public Designator() 
        {
            ClearDesignator();
        }

        public void ClearDesignator()
        {
            _Classifications = null;
        }

        public void CopyDesignator(Designator other)
        {
            Label = other.Label;
            _Classifications = Classifier.CopyClassifiers(other.Classifications);
        }

        public override string ToString()
        {
            return Label;
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

        public List<Classifier> Classifications
        {
            get
            {
                return _Classifications;
            }
            set
            {
                if (value != _Classifications)
                {
                    _Classifications = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int ClassificationCount()
        {
            if (_Classifications == null)
                return 0;
            return _Classifications.Count();
        }

        public int ClassificationCount(string key)
        {
            if (_Classifications == null)
                return 0;
            return _Classifications.Count(x => x.KeyString == key);
        }

        public int UniqueClassificationKeyCount()
        {
            if (_Classifications == null)
                return 0;
            return GetKeys().Count();
        }

        public bool HasClassification(string name)
        {
            Classifier classification = null;

            if (String.IsNullOrEmpty(name) || (_Classifications == null))
                return false;

            classification = _Classifications.FirstOrDefault(x => x.KeyString == name);

            if (classification == null)
                return false;

            return true;
        }

        public bool HasClassificationWith(string name, string value)
        {
            Classifier classification = null;

            if (String.IsNullOrEmpty(name) || (_Classifications == null))
                return false;

            classification = _Classifications.FirstOrDefault(x => (x.KeyString == name) && (x.Text == value));

            if (classification == null)
                return false;

            return true;
        }

        public Classifier GetClassification(string name)
        {
            Classifier classification = null;

            if (String.IsNullOrEmpty(name) || (_Classifications == null))
                return null;

            classification = _Classifications.FirstOrDefault(x => x.KeyString == name);

            return classification;
        }

        public Classifier GetClassificationWithValue(string name, string value)
        {
            Classifier classification = null;

            if (String.IsNullOrEmpty(name) || (_Classifications == null))
                return null;

            classification = _Classifications.FirstOrDefault(x => (x.KeyString == name) && (x.Text == value));

            return classification;
        }

        public int GetClassificationIndexWithValue(string name, string value)
        {
            if (String.IsNullOrEmpty(name) || (_Classifications == null))
                return -1;

            int index = 0;

            foreach (Classifier classification in _Classifications)
            {
                if ((classification.KeyString == name) && (classification.Text == value))
                    return index;

                index++;
            }

            return -1;
        }

        public Classifier GetClassificationIndexed(int index)
        {
            if (_Classifications == null)
                return null;

            if ((index >= 0) && (index < _Classifications.Count()))
                return _Classifications[index];

            return null;
        }

        public Classifier GetClassificationIndexed(string name, int index)
        {
            if (String.IsNullOrEmpty(name) || (_Classifications == null))
                return null;

            int count = 0;

            foreach (Classifier classification in _Classifications)
            {
                if (classification.KeyString == name)
                {
                    if (count == index)
                        return classification;

                    count++;
                }
            }

            return null;
        }

        public string GetClassificationValue(string name)
        {
            string value = String.Empty;

            if (_Classifications == null)
                return value;

            foreach (Classifier classification in _Classifications)
            {
                if (classification.KeyString == name)
                {
                    if (!String.IsNullOrEmpty(value))
                        value += " " + classification.Text;
                    else
                        value += classification.Text;
                }
            }

            return value;

            /*
            Classifier classification = GetClassification(name);

            if (classification == null)
                return null;

            return classification.Text;
            */
        }

        public string GetClassificationValueIndexed(string name, int index)
        {
            Classifier classification = GetClassificationIndexed(name, index);

            if (classification == null)
                return null;

            return classification.Text;
        }

        public int GetClassificationIndex(string name)
        {
            if (String.IsNullOrEmpty(name) || (_Classifications == null))
                return -1;

            int index = 0;

            foreach (Classifier classification in _Classifications)
            {
                if (classification.KeyString == name)
                    return index;

                index++;
            }

            return -1;
        }

        public void AppendClassification(string name, string value)
        {
            Classifier item = new Classifier(name, value);

            if (_Classifications != null)
                _Classifications.Add(item);
            else
                _Classifications = new List<Classifier>() { item };
        }

        public void AppendUniqueClassification(string name, string value)
        {
            if (_Classifications != null)
            {
                foreach (Classifier classification in _Classifications)
                {
                    if ((classification.KeyString == name) && (classification.Text == value))
                        return;
                }
            }

            Classifier item = new Classifier(name, value);

            if (_Classifications != null)
                _Classifications.Add(item);
            else
                _Classifications = new List<Classifier>() { item };
        }

        public void AppendUniqueClassification(Classifier classifier)
        {
            if (classifier == null)
                return;

            if (_Classifications != null)
            {
                foreach (Classifier classification in _Classifications)
                {
                    if ((classification.KeyString == classifier.KeyString) && (classification.Text == classifier.Text))
                        return;
                }
            }

            if (_Classifications != null)
                _Classifications.Add(classifier);
            else
                _Classifications = new List<Classifier>() { classifier };
        }

        public void CopyAndAppendClassification(Classifier classification)
        {
            if (classification == null)
                return;

            Classifier item = new Classifier(classification);

            if (_Classifications != null)
                _Classifications.Add(item);
            else
                _Classifications = new List<Classifier>() { item };
        }

        public void CopyAndAppendUniqueClassification(Classifier classifier)
        {
            if (classifier == null)
                return;

            if (_Classifications != null)
            {
                foreach (Classifier classification in _Classifications)
                {
                    if ((classification.KeyString == classifier.KeyString) && (classification.Text == classifier.Text))
                        return;
                }
            }

            Classifier newClassification = new Classifier(classifier);

            if (_Classifications != null)
                _Classifications.Add(newClassification);
            else
                _Classifications = new List<Classifier>() { newClassification };
        }

        public void AppendClassifications(List<Classifier> classifications)
        {
            if (classifications == null)
                return;

            if (_Classifications == null)
                _Classifications = classifications;
            else
                _Classifications.AddRange(classifications);
        }

        public void AppendClassifications(Designator designator)
        {
            AppendClassifications(designator.CloneClassifications());
        }

        public void AppendUniqueClassifications(List<Classifier> classifications)
        {
            if (classifications == null)
                return;

            if (_Classifications == null)
                _Classifications = classifications;
            else
            {
                foreach (Classifier classifier in classifications)
                    AppendUniqueClassification(classifier);
            }
        }

        public void CopyAndAppendUniqueClassifications(Designator designator)
        {
            CopyAndAppendUniqueClassifications(designator.Classifications);
        }

        public void CopyAndAppendUniqueClassifications(List<Classifier> classifications)
        {
            if (classifications == null)
                return;

            if (_Classifications == null)
                _Classifications = CloneClassificationList(classifications);
            else
            {
                foreach (Classifier classifier in classifications)
                    CopyAndAppendUniqueClassification(classifier);
            }
        }

        public void InsertClassifications(List<Classifier> classifications)
        {
            if (classifications == null)
                return;

            if (_Classifications == null)
                _Classifications = classifications;
            else
                _Classifications.InsertRange(0, classifications);
        }

        public void InsertClassifications(Designator designator)
        {
            InsertClassifications(designator.CloneClassifications());
        }

        public void InsertFirstClassification(string name, string value)
        {
            Classifier item = new Classifier(name, value);

            if (_Classifications != null)
                _Classifications.Add(item);
            else
                _Classifications = new List<Classifier>() { item };
        }

        public bool DeleteFirstClassification(string name)
        {
            Classifier classification = null;

            if (String.IsNullOrEmpty(name) || (_Classifications == null))
                return false;

            classification = _Classifications.FirstOrDefault(x => x.KeyString == name);

            if (classification == null)
                return false;

            _Classifications.Remove(classification);

            return true;
        }

        public bool DeleteClassification(string name, string value)
        {
            Classifier classification = null;

            if (String.IsNullOrEmpty(name) || (_Classifications == null))
                return false;

            classification = _Classifications.FirstOrDefault(x => (x.KeyString == name) && (x.Text == value));

            if (classification == null)
                return false;

            _Classifications.Remove(classification);

            return true;
        }

        public List<Classifier> CloneClassifications()
        {
            if (_Classifications == null)
                return null;

            List<Classifier> classifications = new List<Classifier>();

            foreach (Classifier classifier in _Classifications)
                classifications.Add(new Classifier(classifier));

            return classifications;
        }

        public void ApplyOverride(Designator overrideDesignator)
        {
            int count = overrideDesignator.ClassificationCount();
            int index;

            for (index = 0; index < count; index++)
            {
                Classifier classifier = overrideDesignator.GetClassificationIndexed(index);

                int otherIndex = GetClassificationIndex(classifier.KeyString);

                if (otherIndex != -1)
                {
                    Classifications[otherIndex] = classifier;
                    DefaultLabel();
                }
            }
        }

        public List<string> GetKeys()
        {
            List<string> keys = new List<string>();

            if (ClassificationCount() != 0)
            {
                foreach (Classifier classifier in _Classifications)
                {
                    if (!keys.Contains(classifier.KeyString))
                        keys.Add(classifier.KeyString);
                }
            }

            return keys;
        }

        public void GetUniqueKeys(List<string> keys)
        {
            if (ClassificationCount() != 0)
            {
                foreach (Classifier classifier in _Classifications)
                {
                    if (!keys.Contains(classifier.KeyString))
                        keys.Add(classifier.KeyString);
                }
            }
        }

        public List<string> GetMergedKeys(Designator other)
        {
            List<string> keys = GetKeys();

            if (other != null)
                other.GetUniqueKeys(keys);

            return keys;
        }

        public string GetClassifierValue(string key)
        {
            if (String.IsNullOrEmpty(key) || (_Classifications == null))
                return null;

            string value = String.Empty;

            foreach (Classifier classification in _Classifications)
            {
                if (classification.KeyString == key)
                {
                    if (!String.IsNullOrEmpty(value))
                        value += " ";

                    value += classification.Text;
                }
            }

            return value;
        }

        public string GetClassifierValueOrDefault(string key)
        {
            if (String.IsNullOrEmpty(key) || (_Classifications == null))
                return null;

            string value = String.Empty;

            foreach (Classifier classification in _Classifications)
            {
                if ((string)classification.Key == key)
                {
                    if (!String.IsNullOrEmpty(value))
                        value += " ";

                    value += classification.Text;
                }
            }

            if (String.IsNullOrEmpty(value))
            {
                string defaultValue;

                if (DefaultValueDictionary.TryGetValue(key, out defaultValue))
                    return defaultValue;
            }

            return value;
        }

        public bool MatchIntersect(Designator other)
        {
            bool isHaveAnyMatch = (other.ClassificationCount() != 0);

            if (other.Label != Label)
            {
                foreach (Classifier classifier in other.Classifications)
                {
                    if (!HasClassificationWith(classifier.Name, classifier.Text))
                        return false;
                }
            }

            return isHaveAnyMatch;
        }

        public bool MatchOrAlternate(Designator other)
        {
            bool returnValue = true;

            if (other.Label != Label)
            {
                List<string> keys = GetMergedKeys(other);

                foreach (string key in keys)
                {
                    string value1 = GetClassificationValue(key);
                    string value2 = other.GetClassificationValue(key);

                    if ((value1 != value2) && (key != "Alternate") && (key != "Contraction"))
                    {
                        returnValue = false;
                        break;
                    }
                }
            }

            return returnValue;
        }

        public bool Match(Designator other)
        {
            bool returnValue = false;

            if (other.Label == Label)
                returnValue = true;
            else
            {
                List<string> keys = GetMergedKeys(other);
                int matchCount = 0;
                int keysCount = keys.Count();

                // If no intersection at all.
                //if (keysCount == (other.UniqueClassificationKeyCount() + UniqueClassificationKeyCount()))
                if (keysCount == (other.ClassificationCount() + ClassificationCount()))
                    return false;

                foreach (string key in keys)
                {
                    string value1 = GetClassifierValueOrDefault(key);
                    string value2 = other.GetClassifierValueOrDefault(key);

                    if (value1 == value2)
                        matchCount++;
                    else if ((value1 == "Any") || (value2 == "Any"))
                        matchCount++;
                    else if (value1 == "None")
                        return false;
                }

                returnValue = (matchCount == keys.Count() ? true : false);
            }

            return returnValue;
        }

        public int GetMatchWeight(Designator other)
        {
            int weight = 0;

            List<string> keys = GetMergedKeys(other);

            //if (keys.Count() == (UniqueClassificationKeyCount() + other.UniqueClassificationKeyCount()))
            if (keys.Count() == (ClassificationCount() + other.ClassificationCount()))
                return 0;

            if (other.Label == Label)
                weight = keys.Count();
            else
            {
                foreach (string key in keys)
                {
                    string value1 = GetClassifierValueOrDefault(key);
                    string value2 = other.GetClassifierValueOrDefault(key);

                    if (value1 == "None")
                    {
                        weight = 0;
                        break;
                    }

                    if (value2 == "None")
                        continue;

                    if (value1 == value2)
                        weight++;
                    else if ((value1 == "Any") || (value2 == "Any"))
                        weight++;
                    else if (EndsWithNumber(value1) && !EndsWithNumber(value2))
                    {
                        if (ValueWithoutNumber(value1) != value2)
                            return 0;
                    }
                    else if (EndsWithNumber(value2) && !EndsWithNumber(value1))
                    {
                        if (value1 != ValueWithoutNumber(value2))
                            return 0;
                    }
                    else
                        return 0;
                }
            }

            return weight;
        }

        // Create a designator from classifications where the keys start with the prefix.
        public Designator GetPrefixedDesignator(string prefix)
        {
            List<Classifier> classifications = new List<Classifier>();

            foreach (Classifier classifier in Classifications)
            {
                if (classifier.KeyString.StartsWith(prefix))
                    classifications.Add(new Classifier(classifier));
            }

            if (classifications.Count() == 0)
                return null;

            Designator designator = new Designator(null, classifications);

            return designator;
        }

        protected bool EndsWithNumber(string value)
        {
            if (String.IsNullOrEmpty(value))
                return false;
            if (char.IsDigit(value[value.Length - 1]))
                return true;

            return false;
        }

        protected string ValueWithoutNumber(string value)
        {
            return value.Substring(0, value.Length - 1);
        }

        public string ComposeLabel()
        {
            string label = String.Empty;

            if (_Classifications != null)
            {
                foreach (Classifier classification in _Classifications)
                {
                    if (classification.HasText())
                    {
                        if (!String.IsNullOrEmpty(label))
                            label += " ";

                        label += classification.Text;
                    }
                }
            }

            return label;
        }

        public void DefaultLabelCheck()
        {
            if (String.IsNullOrEmpty(Label))
            {
                Label = ComposeLabel();
                TouchAndClearModified();
            }
        }

        public void DefaultLabel()
        {
            Label = ComposeLabel();
            TouchAndClearModified();
        }

        public List<Classifier> UnionClassifiers(List<Designator> designators)
        {
            List<Classifier> classifiers = new List<Classifier>();

            foreach (Designator designator in designators)
            {
                foreach (Classifier classification in designator.Classifications)
                {
                    string key = classification.KeyString;
                    string text = classification.Text;
                    /*
                    Classifier otherClassification = classifiers.FirstOrDefault(x => x.KeyString == key);
                    if (otherClassification != null)
                    {
                        if (!otherClassification.Text.Contains(text))
                            otherClassification.Text = otherClassification.Text + "|" + text;
                    }
                    else
                    {
                        otherClassification = new Classifier(classification);
                        classifiers.Add(otherClassification);
                    }
                    */
                    Classifier otherClassification = classifiers.FirstOrDefault(x => (x.KeyString == key) && (x.Text == text));
                    if (otherClassification == null)
                    {
                        otherClassification = new Classifier(classification);
                        classifiers.Add(otherClassification);
                    }
                }
            }

            return classifiers;
        }

        public List<Classifier> UnionClassifiers(Designator designator1, Designator designator2)
        {
            List<Designator> designators  = new List<Designator>(2) { designator1, designator2 };
            return UnionClassifiers(designators);
        }

        public List<Classifier> UnionClassifiers(Designator designator1, Designator designator2, Designator designator3)
        {
            List<Designator> designators = new List<Designator>(2) { designator1, designator2, designator3 };
            return UnionClassifiers(designators);
        }

        public List<Classifier> IntersectClassifiers(List<Designator> designators)
        {
            List<Classifier> classifiers = new List<Classifier>();
            Dictionary<string, int> countDict = new Dictionary<string, int>();
            int designatorCount = designators.Count();

            foreach (Designator designator in designators)
            {
                foreach (Classifier classification in designator.Classifications)
                {
                    string key = classification.KeyString;
                    string text = classification.Text;
                    /*
                    Classifier otherClassification = classifiers.FirstOrDefault(x => x.KeyString == key);
                    if (otherClassification != null)
                    {
                        if (!otherClassification.Text.Contains(text))
                            otherClassification.Text = otherClassification.Text + "|" + text;
                    }
                    else
                    {
                        otherClassification = new Classifier(classification);
                        classifiers.Add(otherClassification);
                    }
                    */
                    Classifier otherClassification = classifiers.FirstOrDefault(x => (x.KeyString == key) && (x.Text == text));
                    if (otherClassification == null)
                    {
                        otherClassification = new Classifier(classification);
                        classifiers.Add(otherClassification);
                    }
                    int counter;
                    if (!countDict.TryGetValue(text, out counter))
                        countDict.Add(text, 1);
                    else
                        countDict[text] = counter + 1;
                }
            }

            int count = classifiers.Count();
            int index;

            for (index = count - 1; index >= 0; index--)
            {
                Classifier classification = classifiers[index];

                if (countDict[classification.Text] != designatorCount)
                    classifiers.RemoveAt(index);
            }

            return classifiers;
        }

        public List<Classifier> IntersectClassifiers(Designator designator1, Designator designator2)
        {
            List<Designator> designators = new List<Designator>(2) { designator1, designator2 };
            return IntersectClassifiers(designators);
        }

        public List<Classifier> IntersectClassifiers(Designator designator1, Designator designator2, Designator designator3)
        {
            List<Designator> designators = new List<Designator>(2) { designator1, designator2, designator3 };
            return IntersectClassifiers(designators);
        }

        // label is one or more classifier text values.  Skips classifications not in label.
        public void ReorderFromLabel(string label)
        {
            string[] parts = label.Split(LanguageLookup.Space);
            int count = parts.Count();
            int index;

            for (index = 0; index < count; index++)
            {
                string value = parts[index];
                Classifier classifier = null;

                int cc = ClassificationCount();
                int ci;

                for (ci = 0; ci < cc; ci++)
                {
                    classifier = _Classifications[ci];

                    if (classifier.Text == value)
                        break;
                }

                if (ci >= cc)
                    continue;

                if (ci != index)
                {
                    _Classifications.RemoveAt(ci);
                    _Classifications.Insert(index, classifier);
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if (Label != ComposeLabel())
            {
                if (!String.IsNullOrEmpty(Label))
                    element.Add(new XElement("Label", Label));
            }

            if (_Classifications != null)
            {
                foreach (Classifier classification in _Classifications)
                {
                    if (classification.HasText())
                    {
                        XElement childElement = classification.GetElement("Classifier");
                        element.Add(childElement);
                    }
                }
            }

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Label":
                    Label = childElement.Value.Trim();
                    break;
                case "Classifier":
                    {
                        Classifier classification = new Classifier(childElement);
                        if (_Classifications != null)
                            _Classifications.Add(classification);
                        else
                            _Classifications = new List<Classifier>() { classification };
                    }
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public static List<Classifier> CloneClassificationList(List<Classifier> classifications)
        {
            if (classifications == null)
                return null;

            List<Classifier> newClassifications = new List<Classifier>();

            foreach (Classifier classification in classifications)
                newClassifications.Add(new Classifier(classification));

            return newClassifications;
        }
    }
}
