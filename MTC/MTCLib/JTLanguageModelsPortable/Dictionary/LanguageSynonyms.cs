using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Dictionary
{
    public class LanguageSynonyms
    {
        protected LanguageID _LanguageID;
        protected List<ProbableMeaning> _ProbableSynonyms;
        protected bool _Modified;

        public LanguageSynonyms(LanguageID languageID, List<ProbableMeaning> probableSynonyms)
        {
            _LanguageID = languageID;

            _ProbableSynonyms = probableSynonyms;
            _Modified = false;
        }

        public LanguageSynonyms(LanguageSynonyms other)
        {
            Copy(other);
            _Modified = false;
        }

        public LanguageSynonyms(XElement element)
        {
            OnElement(element);
        }

        public LanguageSynonyms()
        {
            Clear();
        }

        public void Clear()
        {
            _LanguageID = null;
            _ProbableSynonyms = null;
            _Modified = false;
        }

        public void Copy(LanguageSynonyms other)
        {
            _LanguageID = other.LanguageID;

            if (other.HasProbableSynonyms())
                _ProbableSynonyms = new List<ProbableMeaning>(other.ProbableSynonyms);
            else
                _ProbableSynonyms = null;

            _Modified = true;
        }

        public override string ToString()
        {
            return (_LanguageID != null ? _LanguageID.LanguageCultureExtensionCode : "(null)") + ": " + GetDefinition(true);
        }

        public LanguageID LanguageID
        {
            get
            {
                return _LanguageID;
            }
            set
            {
                if (value != _LanguageID)
                {
                    _LanguageID = value;
                    _Modified = true;
                }
            }
        }

        public List<ProbableMeaning> ProbableSynonyms
        {
            get
            {
                return _ProbableSynonyms;
            }
            set
            {
                if (value != _ProbableSynonyms)
                {
                    _ProbableSynonyms = value;
                    _Modified = true;
                }
            }
        }

        public bool HasProbableSynonyms()
        {
            if ((_ProbableSynonyms != null) && (_ProbableSynonyms.Count() != 0))
                return true;
            return false;
        }

        public ProbableMeaning GetProbableSynonymIndexed(int index)
        {
            if ((_ProbableSynonyms != null) && (index >= 0) && (index < _ProbableSynonyms.Count()))
                return _ProbableSynonyms[index];

            return null;
        }

        public int GetProbableSynonymIndex(ProbableMeaning probableSynonym)
        {
            if (_ProbableSynonyms != null)
                return _ProbableSynonyms.IndexOf(probableSynonym);

            return -1;
        }

        public ProbableMeaning FindProbableSynonym(string synonym)
        {
            if ((_ProbableSynonyms != null) && (_ProbableSynonyms.Count() != 0))
                return _ProbableSynonyms.FirstOrDefault(x => x.MatchMeaning(synonym));

            return null;
        }

        public bool CanOverlay(LanguageSynonyms other)
        {
            if (!other.HasProbableSynonyms())
                return false;

            foreach (ProbableMeaning ps2 in other.ProbableSynonyms)
            {
                ProbableMeaning ps1 = FindProbableSynonym(ps2.Meaning);

                if (ps1 == null)
                    return false;

                if ((ps1.Category != LexicalCategory.Unknown) && (ps2.Category != LexicalCategory.Unknown))
                {
                    if (ps1.Category != ps2.Category)
                        return false;
                }
            }

            return true;
        }

        public bool AddProbableSynonym(ProbableMeaning probableSynonym)
        {
            if (_ProbableSynonyms == null)
            {
                _ProbableSynonyms = new List<ProbableMeaning>() { probableSynonym };
                _Modified = true;
                return true;
            }
            else if (!_ProbableSynonyms.Contains(probableSynonym))
            {
                foreach (ProbableMeaning meaning in _ProbableSynonyms)
                {
                    if (TextUtilities.IsEqualStringsIgnoreCase(meaning.Meaning, probableSynonym.Meaning))
                    {
                        if (meaning.Match(probableSynonym))
                            return true;
                        else
                        {
                            meaning.Merge(probableSynonym);
                            _Modified = true;
                            return true;
                        }
                    }
                }
                _ProbableSynonyms.Add(probableSynonym);
                _Modified = true;
                return true;
            }

            return false;
        }

        public bool InsertProbableSynonym(int index, ProbableMeaning probableSynonym)
        {
            if ((_ProbableSynonyms != null) && (index >= 0) && (index <= _ProbableSynonyms.Count()))
            {
                _ProbableSynonyms.Insert(index, probableSynonym);
                _Modified = true;
                return true;
            }

            return false;
        }

        public void DeleteAllProbableSynonyms()
        {
            if (_ProbableSynonyms != null)
            {
                _ProbableSynonyms.Clear();
                _Modified = true;
            }
        }

        public int ProbableSynonymCount
        {
            get
            {
                if (_ProbableSynonyms != null)
                    return _ProbableSynonyms.Count();
                return 0;
            }
        }

        public bool GetProbableSynonymContainingText(
            string containingText,
            out int synonymIndex,
            out ProbableMeaning definition)
        {
            int index = 0;

            synonymIndex = -1;
            definition = null;

            if (!HasProbableSynonyms())
                return false;

            foreach (ProbableMeaning probableSynonym in _ProbableSynonyms)
            {
                if (probableSynonym.Meaning.Contains(containingText))
                {
                    definition = probableSynonym;
                    synonymIndex = index;
                    return true;
                }

                index++;
            }

            return false;
        }

        public bool SortProbableSynonymsBySourceCount()
        {
            if (!HasProbableSynonyms() || (ProbableSynonymCount <= 1))
                return false;

            int count = int.MaxValue;
            bool needsSort = false;

            foreach (ProbableMeaning probableSynonym in _ProbableSynonyms)
            {
                int thisCount = probableSynonym.SourceIDCount();

                if (thisCount == count)
                    continue;

                if (thisCount < count)
                    count = thisCount;
                else
                {
                    needsSort = true;
                    break;
                }
            }

            if (!needsSort)
                return false;

            List<ProbableMeaning> newList = new List<ProbableMeaning>();

            foreach (ProbableMeaning probableSynonym in _ProbableSynonyms)
            {
                int c = newList.Count();
                int i;
                bool inserted = false;

                for (i = 0; i < c; i++)
                {
                    ProbableMeaning obj = newList[i];

                    if (probableSynonym.SourceIDCount() > obj.SourceIDCount())
                    {
                        newList.Insert(i, probableSynonym);
                        inserted = true;
                        break;
                    }
                }

                if (!inserted)
                    newList.Add(probableSynonym);
            }

            _ProbableSynonyms = newList;

            return true;
        }

        public int MaxSourceIDCount()
        {
            if (ProbableSynonymCount == 0)
                return 0;

            int maxCount = 0;

            foreach (ProbableMeaning probableSynonym in _ProbableSynonyms)
            {
                int thisCount = probableSynonym.SourceIDCount();

                if (thisCount > maxCount)
                    maxCount = thisCount;
            }

            return maxCount;
        }

        public int MaxFrequency()
        {
            if (ProbableSynonymCount == 0)
                return 0;

            int maxFrequency = 0;

            foreach (ProbableMeaning probableSynonym in _ProbableSynonyms)
            {
                int thisCount = probableSynonym.Frequency;

                if (thisCount > maxFrequency)
                    maxFrequency = thisCount;
            }

            return maxFrequency;
        }

        public List<string> Synonyms
        {
            get
            {
                if ((_ProbableSynonyms == null) || (_ProbableSynonyms.Count() == 0))
                    return null;
                List<string> synonyms = new List<string>();
                foreach (ProbableMeaning probableSynonym in _ProbableSynonyms)
                    synonyms.Add(probableSynonym.Meaning);
                return synonyms;
            }
        }

        public bool HasSynonyms()
        {
            if ((_ProbableSynonyms != null) && (_ProbableSynonyms.Count() != 0))
                return true;
            return false;
        }

        public bool HasSynonym(string synonym)
        {
            if ((_ProbableSynonyms == null) || (_ProbableSynonyms.Count() == 0))
                return false;

            foreach (ProbableMeaning probableSynonym in _ProbableSynonyms)
            {
                if (probableSynonym.MatchMeaningIgnoreCase(synonym))
                    return true;
            }

            return false;
        }

        public string GetSynonymIndexed(int index)
        {
            if ((_ProbableSynonyms != null) && (index >= 0) && (index < _ProbableSynonyms.Count()))
                return _ProbableSynonyms[index].Meaning;

            return String.Empty;
        }

        public bool AddSynonym(
            string synonym,
            LexicalCategory category,
            string categoryString,
            float probability,
            int frequency,
            int sourceID)
        {
            ProbableMeaning probableSynonym = new ProbableMeaning(
                synonym,
                category,
                categoryString,
                probability,
                frequency,
                sourceID);
            return AddProbableSynonym(probableSynonym);
        }

        public bool DeleteSynonymIndexed(int index)
        {
            if (_ProbableSynonyms == null)
                return false;

            if ((index < 0) || (index >= _ProbableSynonyms.Count()))
                return false;

            _ProbableSynonyms.RemoveAt(index);
            _Modified = true;

            return true;
        }

        public void DeleteAllSynonyms()
        {
            if (_ProbableSynonyms != null)
            {
                _ProbableSynonyms.Clear();
                _Modified = true;
            }
        }

        public int SynonymCount
        {
            get
            {
                if (_ProbableSynonyms != null)
                    return _ProbableSynonyms.Count();
                return 0;
            }
        }

        public bool HasMeaning(string meaning)
        {
            if (_ProbableSynonyms == null)
                return false;

            if (_ProbableSynonyms.FirstOrDefault(x => TextUtilities.IsEqualStringsIgnoreCase(x.Meaning, meaning)) != null)
                return true;

            return false;
        }

        public bool HasMeaningStart(string meaning)
        {
            if (_ProbableSynonyms == null)
                return false;

            foreach (ProbableMeaning probableSynonym in _ProbableSynonyms)
            {
                if (TextUtilities.StartsWithIgnoreCase(probableSynonym.Meaning, meaning))
                    return true;
            }

            return false;
        }

        public void Retarget(MultiLanguageString input, MultiLanguageString output, Sense sense)
        {
            LanguageString lsInput = input.LanguageString(LanguageID);
            LanguageString lsOutput = output.LanguageString(LanguageID);

            if (lsOutput != null)
            {
                if ((lsInput != null) && (_ProbableSynonyms != null) && (_ProbableSynonyms.Count() != 0))
                {
                    for (int i = 0; i < ProbableSynonymCount; i++)
                    {
                        ProbableMeaning probableSynonym = _ProbableSynonyms[i];

                        if (TextUtilities.IsEqualStringsIgnoreCase(probableSynonym.Meaning, lsInput.Text))
                            probableSynonym.Meaning = lsOutput.Text;
                    }
                }
                else
                {
                    _ProbableSynonyms.Clear();
                    AddSynonym(
                        lsOutput.Text,
                        sense.Category,
                        sense.CategoryString,
                        float.NaN,
                        0,
                        0);
                }
            }
        }

        public string GetDefinition(bool showSources)
        {
            int count = (_ProbableSynonyms == null ? 0 : _ProbableSynonyms.Count());
            string definition;
            bool isPhonetic = false;
            string synonym;
            string text;

            if (LanguageLookup.IsAlternatePhonetic(LanguageID))
                isPhonetic = true;

            if (count == 0)
                return String.Empty;
            else if (count == 1)
            {
                ProbableMeaning probableSynonym = GetProbableSynonymIndexed(0);

                definition = probableSynonym.Meaning;

                if (isPhonetic)
                    ConvertPinyinNumeric.ToToneMarks(out definition, definition);

                if (showSources && probableSynonym.HasSourceIDs())
                    definition += " {" + probableSynonym.SourceIDsString + "}";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                int index = 1;

                foreach (ProbableMeaning probableSynonym in _ProbableSynonyms)
                {
                    text = synonym = probableSynonym.Meaning;

                    if (index != 1)
                        sb.Append(" / ");

                    if (isPhonetic)
                        ConvertPinyinNumeric.ToToneMarks(out text, synonym);

                    sb.Append(text);

                    if (showSources && probableSynonym.HasSourceIDs())
                    {
                        sb.Append(" {");
                        sb.Append(probableSynonym.SourceIDsString);
                        sb.Append("}");
                    }

                    index++;
                }

                definition = sb.ToString();
            }

            return definition;
        }

        public string GetDefinitionMarkedUp(List<LanguageDescription> languageDescriptions, bool showSources)
        {
            int count = (_ProbableSynonyms == null ? 0 : _ProbableSynonyms.Count());
            string definition;
            bool isPhonetic = false;
            string synonym;
            string text;

            if (LanguageLookup.IsAlternatePhonetic(LanguageID))
                isPhonetic = true;

            if (count == 0)
                return String.Empty;
            else if (count == 1)
            {
                ProbableMeaning probableSynonym = GetProbableSynonymIndexed(0);

                definition = probableSynonym.Meaning;

                if (isPhonetic)
                    ConvertPinyinNumeric.ToToneMarks(out definition, definition);

                if (showSources && probableSynonym.HasSourceIDs())
                    definition += " {" + probableSynonym.SourceIDsString + "}";

                definition = LanguageUtilities.FormatDictionaryText(definition, LanguageID, languageDescriptions);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                int index = 1;

                foreach (ProbableMeaning probableSynonym in _ProbableSynonyms)
                {
                    text = synonym = probableSynonym.Meaning;

                    if (index != 1)
                        sb.Append(" / ");

                    if (isPhonetic)
                        ConvertPinyinNumeric.ToToneMarks(out text, synonym);

                    if (showSources && probableSynonym.HasSourceIDs())
                        text += " {" + probableSynonym.SourceIDsString + "}";

                    text = LanguageUtilities.FormatDictionaryText(text, LanguageID, languageDescriptions);
                    sb.Append(text);

                    index++;
                }

                definition = sb.ToString();
            }

            return definition;
        }

        public string GetTranslation()
        {
            int count = (_ProbableSynonyms == null ? 0 : _ProbableSynonyms.Count());
            string definition;
            bool isPhonetic = false;

            if (LanguageLookup.IsAlternatePhonetic(LanguageID))
                isPhonetic = true;

            if (count == 0)
                return String.Empty;
            else
            {
                definition = GetSynonymIndexed(0);

                if (isPhonetic)
                    ConvertPinyinNumeric.ToToneMarks(out definition, definition);
            }

            return definition;
        }

        public bool GetSynonymContainingText(string containingText, out int synonymIndex, out string definition)
        {
            int index = 0;

            synonymIndex = -1;
            definition = String.Empty;

            if (SynonymCount == 0)
                return false;
            
            foreach (ProbableMeaning probableSynonym in _ProbableSynonyms)
            {
                if (probableSynonym.Meaning.Contains(containingText))
                {
                    definition = probableSynonym.Meaning;
                    synonymIndex = index;
                    return true;
                }

                index++;
            }

            return false;
        }

        public bool GetSynonymMatchingText(string matchingText, out int synonymIndex)
        {
            int index = 0;

            synonymIndex = -1;

            if (SynonymCount == 0)
                return false;

            foreach (ProbableMeaning probableSynonym in _ProbableSynonyms)
            {
                if (probableSynonym.MatchMeaning(matchingText))
                {
                    synonymIndex = index;
                    return true;
                }

                index++;
            }

            return false;
        }

        public void GetDumpString(string prefix, StringBuilder sb)
        {
            sb.AppendLine(prefix + "LanguageID: " + LanguageID.LanguageName(LanguageLookup.English));

            if (_ProbableSynonyms != null)
            {
                sb.AppendLine(prefix + "Synonyms:");

                foreach (ProbableMeaning probableSynonym in _ProbableSynonyms)
                    sb.AppendLine(prefix + "    " + probableSynonym.Meaning);
            }
        }

        public bool Modified
        {
            get
            {
                return _Modified;
            }
            set
            {
                _Modified = value;
            }
        }

        public virtual XElement GetElement(string name)
        {
            XElement element = new XElement(name);
            if (_LanguageID != null)
                element.Add(new XAttribute("LanguageID", LanguageID));
            if (_ProbableSynonyms != null)
            {
                foreach (ProbableMeaning probableSynonym in _ProbableSynonyms)
                    element.Add(probableSynonym.GetElement("ProbableSynonym"));
            }
            return element;
        }

        public virtual bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "LanguageID":
                    _LanguageID = LanguageLookup.GetLanguageID(attributeValue);
                    break;
                default:
                    return false;
            }

            return true;
        }

        public virtual bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Synonym":     // Needed for inflecton tables
                    AddSynonym(
                        childElement.Value.Trim(),
                        LexicalCategory.Unknown,
                        null,
                        float.NaN,
                        0,
                        InflectionTableDictionarySourceID);
                    break;
                case "ProbableSynonym":
                    if (_ProbableSynonyms == null)
                        _ProbableSynonyms = new List<ProbableMeaning>();
                    _ProbableSynonyms.Add(new ProbableMeaning(childElement));
                    break;
                default:
                    return false;
            }

            return true;
        }

        public virtual void OnElement(XElement element)
        {
            Clear();

            foreach (XAttribute attribute in element.Attributes())
            {
                if (!OnAttribute(attribute))
                    throw new ObjectException("Unknown attribute " + attribute.Name + " in " + element.Name.ToString() + ".");
            }

            foreach (var childNode in element.Elements())
            {
                XElement childElement = childNode as XElement;

                if (childElement == null)
                    continue;

                if (!OnChildElement(childElement))
                    throw new ObjectException("Unknown child element " + childElement.Name + " in " + element.Name.ToString() + ".");
            }

            _Modified = false;
        }

        public bool Match(LanguageSynonyms other)
        {
            if (other.LanguageID != _LanguageID)
                return false;

            if ((other.ProbableSynonyms == null) && (_ProbableSynonyms == null))
                return true;

            if ((other.ProbableSynonyms == null) || (_ProbableSynonyms == null))
                return false;

            if (other.ProbableSynonymCount != _ProbableSynonyms.Count())
                return false;

            int count = _ProbableSynonyms.Count();

            for (int index = 0; index < ProbableSynonymCount; index++)
            {
                if (!_ProbableSynonyms[index].MatchMeaningIgnoreCase(other.ProbableSynonyms[index]))
                    return false;
            }

            return true;
        }

        public int Compare(LanguageSynonyms other)
        {
            int diff;

            diff = LanguageID.Compare(_LanguageID, other.LanguageID);

            if (diff != 0)
                return diff;

            if ((other.ProbableSynonyms == null) && (_ProbableSynonyms == null))
                return 0;

            if (other.ProbableSynonyms == null)
                return 1;

            if (_ProbableSynonyms == null)
                return -1;

            int count = _ProbableSynonyms.Count();
            int otherCount = other.ProbableSynonymCount;

            for (int index = 0; index < ProbableSynonymCount; index++)
            {
                if (index >= otherCount)
                    return 1;

                diff = _ProbableSynonyms[index].Compare(other.ProbableSynonyms[index]);

                if (diff != 0)
                    return diff;
            }

            if (otherCount > count)
                return 1;

            return 0;
        }

        public static string InflectionTableDictionarySourceName = "InflectionTable";

        protected static int _InflectionTableDictionarySourceID = 0;
        public static int InflectionTableDictionarySourceID
        {
            get
            {
                if (_InflectionTableDictionarySourceID == 0)
                    _InflectionTableDictionarySourceID = ApplicationData.DictionarySourcesLazy.Add(InflectionTableDictionarySourceName);

                return _InflectionTableDictionarySourceID;
            }
        }
    }
}
