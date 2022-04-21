using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Dictionary
{
    public class DictionaryEntry : BaseObjectLanguage
    {
        protected string _IPAReading;
        // Alternate key is a reading key integer. 0 is the default reading.
        protected List<LanguageString> _Alternates;
        protected int _Frequency;
        protected List<int> _SourceIDs;
        protected List<Sense> _Senses;
        protected List<MediaRun> _MediaRuns;

        public DictionaryEntry(
                string key,
                LanguageID languageID,
                List<LanguageString> alternates,
                int frequency,
                List<int> sourceIDs,
                List<Sense> senses,
                List<MediaRun> mediaRuns)
            : base(key, languageID)
        {
            ClearDictionaryEntry();
            _Alternates = alternates;
            _Frequency = frequency;
            _SourceIDs = sourceIDs;
            _Senses = senses;
            _MediaRuns = mediaRuns;
        }

        public DictionaryEntry(
                string key,
                LanguageID languageID,
                List<LanguageString> alternates,
                int frequency,
                int sourceID,
                List<Sense> senses,
                List<MediaRun> mediaRuns)
            : base(key, languageID)
        {
            ClearDictionaryEntry();
            _Alternates = alternates;
            _Frequency = frequency;
            if (sourceID != 0)
                _SourceIDs = new List<int>() { sourceID };
            else
                _SourceIDs = null;
            _Senses = senses;
            _MediaRuns = mediaRuns;
        }

        public DictionaryEntry(string key, LanguageID languageID)
            : base(key, languageID)
        {
            ClearDictionaryEntry();
        }

        public DictionaryEntry(DictionaryEntry other, string key)
            : base(other, key)
        {
            CopyDictionaryEntry(other);
        }

        public DictionaryEntry(DictionaryEntry other)
            : base(other)
        {
            CopyDictionaryEntry(other);
        }

        public DictionaryEntry(XElement element)
        {
            OnElement(element);
        }

        public DictionaryEntry()
        {
            ClearDictionaryEntry();
        }

        public override void Clear()
        {
            base.Clear();
            ClearDictionaryEntry();
        }

        public void ClearDictionaryEntry()
        {
            _IPAReading = null;
            _Alternates = null;
            _Frequency = 0;
            _SourceIDs = null;
            _Senses = null;
            _MediaRuns = null;
        }

        public void CopyDictionaryEntry(DictionaryEntry other)
        {
            _IPAReading = other._IPAReading;

            if (other.Alternates != null)
            {
                int count = other.Alternates.Count();

                _Alternates = new List<LanguageString>(count);

                foreach (LanguageString alternate in other.Alternates)
                    _Alternates.Add(new LanguageString(alternate));
            }
            else
                _Alternates = null;

            _Frequency = other.Frequency;

            if (other.SourceIDCount() != 0)
                _SourceIDs = new List<int>(other.SourceIDs);
            else
                _Senses = null;

            if (other.Senses != null)
            {
                int count = other.Senses.Count();

                _Senses = new List<Sense>(count);

                foreach (Sense sense in other.Senses)
                    _Senses.Add(new Sense(sense));
            }
            else
                _Senses = null;

            if (other.MediaRuns != null)
            {
                int count = other.MediaRuns.Count();

                _MediaRuns = new List<MediaRun>(count);

                foreach (MediaRun mediaRun in other.MediaRuns)
                    _MediaRuns.Add(new MediaRun(mediaRun));
            }
            else
                _MediaRuns = null;
        }

        public override IBaseObject Clone()
        {
            return new DictionaryEntry(this);
        }

        public override string ToString()
        {
            LanguageID languageID = FirstSenseLanguageID();
            return GetDefinition(languageID, true, true);
        }

        public string IPAReading
        {
            get
            {
                return _IPAReading;
            }
            set
            {
                if (value != _IPAReading)
                {
                    _IPAReading = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int Frequency
        {
            get
            {
                return _Frequency;
            }
            set
            {
                if (value != _Frequency)
                {
                    _Frequency = value;
                    ModifiedFlag = true;
                }
            }
        }

        public void MakeAlternate(DictionaryEntry other, LanguageID languageID)
        {
            LanguageString thisAlternate = other.GetAlternate(languageID);

            if (thisAlternate == null)
            {
                CopyDictionaryEntry(other);
                return;
            }

            Key = thisAlternate.Text;
            LanguageID = languageID;
            int alternateKey = AllocateAlternateKey(other.LanguageID, other.KeyString);

            _Alternates = new List<LanguageString>(1) { new LanguageString(alternateKey, other.LanguageID, other.KeyString) };

            if (other.Alternates != null)
            {
                foreach (LanguageString alternate in other.Alternates)
                {
                    if (alternate.LanguageID == languageID)
                        continue;

                    _Alternates.Add(new LanguageString(alternate.Key, alternate.LanguageID, alternate.Text));
                }
            }

            if (other.Senses != null)
            {
                int count = other.Senses.Count();

                _Senses = new List<Sense>(count);

                foreach (Sense sense in other.Senses)
                    _Senses.Add(new Sense(sense));
            }
            else
                _Senses = null;

            if (other.MediaRuns != null)
            {
                int count = other.MediaRuns.Count();

                _MediaRuns = new List<MediaRun>(count);

                foreach (MediaRun mediaRun in other.MediaRuns)
                    _MediaRuns.Add(new MediaRun(mediaRun));
            }
            else
                _MediaRuns = null;
        }

        public void MergeEntry(DictionaryEntry other)
        {
            if (!MatchKey(other.Key))
                return;

            if (LanguageID != other.LanguageID)
                return;

            if (other.Frequency > _Frequency)
                _Frequency = other.Frequency;

            MergeSourceIDs(other.SourceIDs);

            List<Sense> newSenses = new List<Sense>();

            if (other.Senses != null)
            {
                foreach (Sense sense in other.Senses)
                {
                    Sense newSense = new Sense(sense);
                    int reading = newSense.Reading;
                    int orgReading = -1;

                    if (other.Alternates != null)
                    {
                        List<LanguageString> oldAlternates = other.Alternates.Where(x => x.KeyInt == reading).ToList();
                        LanguageString oldAlternate = oldAlternates.FirstOrDefault();

                        foreach (LanguageString testAlternate in _Alternates)
                        {
                            if ((testAlternate.Text == oldAlternate.Text) && (testAlternate.LanguageID == oldAlternate.LanguageID))
                            {
                                List<LanguageString> orgAlternates = _Alternates.Where(x => x.KeyInt == testAlternate.KeyInt).ToList();
                                int orgCount = 0;

                                foreach (LanguageString anAlternate in oldAlternates)
                                {
                                    if (orgAlternates.FirstOrDefault(x => (x.KeyString == anAlternate.KeyString) && (x.LanguageID == anAlternate.LanguageID)) != null)
                                        orgCount++;
                                }

                                if (orgCount == oldAlternates.Count())
                                {
                                    orgReading = testAlternate.KeyInt;
                                    break;
                                }
                            }
                        }

                        if (orgReading == -1)
                        {
                            orgReading = AllocateAlternateKey();

                            foreach (LanguageString anAlternate in oldAlternates)
                            {
                                LanguageString newAlternate = new LanguageString(anAlternate);
                                newAlternate.Key = orgReading;
                                AddAlternate(newAlternate);
                            }
                        }

                        newSense.Reading = orgReading;
                    }

                    newSenses.Add(newSense);
                }
            }

            if (_Senses == null)
                _Senses = newSenses;
            else
            {
                foreach (Sense sense in newSenses)
                    MergeOrAddSense(sense);
            }

            if (other.MediaRuns != null)
            {
                int count = other.MediaRuns.Count();

                if (count != 0)
                {
                    if (_MediaRuns == null)
                    {
                        _MediaRuns = new List<MediaRun>(count);

                        foreach (MediaRun mediaRun in other.MediaRuns)
                            _MediaRuns.Add(new MediaRun(mediaRun));
                    }
                    else
                    {
                        foreach (MediaRun mediaRun in other.MediaRuns)
                        {
                            if (_MediaRuns.FirstOrDefault(x => MediaRun.Compare(x, mediaRun) == 0) == null)
                                _MediaRuns.Add(new MediaRun(mediaRun));
                        }
                    }
                }
            }
        }

        public void MergeAlternate(DictionaryEntry other, LanguageID languageID)
        {
            LanguageString thisAlternate = other.GetAlternate(languageID);

            if (thisAlternate == null)
            {
                CopyDictionaryEntry(other);
                return;
            }

            Key = thisAlternate.Text;
            LanguageID = languageID;

            MergeSourceIDs(other.SourceIDs);

            if (_Alternates == null)
                _Alternates = new List<LanguageString>(1)
                    {
                        new LanguageString(
                            other.AllocateAlternateKey(other.LanguageID, other.KeyString),
                            other.LanguageID,
                            other.KeyString)
                    };

            if (other.Alternates != null)
            {
                foreach (LanguageString alternate in other.Alternates)
                {
                    if (alternate.LanguageID == languageID)
                        continue;

                    if (_Alternates.FirstOrDefault(x => (x.LanguageID == alternate.LanguageID) && (x.Text == alternate.Text)) == null)
                        _Alternates.Add(new LanguageString(alternate.Key, alternate.LanguageID, alternate.Text));
                }
            }

            if (other.Senses != null)
            {
                int count = other.Senses.Count();

                if (_Senses == null)
                {
                    _Senses = new List<Sense>(count);

                    foreach (Sense sense in other.Senses)
                        _Senses.Add(new Sense(sense));
                }
                else
                {
                    foreach (Sense sense in other.Senses)
                        MergeOrAddSense(sense);
                }
            }

            if (other.MediaRuns != null)
            {
                int count = other.MediaRuns.Count();

                if (count != 0)
                {
                    if (_MediaRuns == null)
                    {
                        _MediaRuns = new List<MediaRun>(count);

                        foreach (MediaRun mediaRun in other.MediaRuns)
                            _MediaRuns.Add(new MediaRun(mediaRun));
                    }
                    else
                    {
                        foreach (MediaRun mediaRun in other.MediaRuns)
                        {
                            if (_MediaRuns.FirstOrDefault(x => MediaRun.Compare(x, mediaRun) == 0) == null)
                                _MediaRuns.Add(new MediaRun(mediaRun));
                        }
                    }
                }
            }
        }

        // Returns true if new sense added.
        public bool MergeOrAddSense(Sense other)
        {
            bool returnValue = true;

            if (other.LanguageSynonyms == null)
                return false;

            if (_Senses == null)
            {
                _Senses = new List<Sense>() { other };
                ModifiedFlag = true;
            }
            else
            {
                bool merged = false;

                foreach (Sense sense in _Senses)
                {
                    if (sense.Reading != other.Reading)
                        continue;

                    if ((other.Category != LexicalCategory.Unknown) || !String.IsNullOrEmpty(other.CategoryString))
                    {
                        if (((sense.Category != other.Category) || (sense.CategoryString != other.CategoryString))
                                && ((sense.Category != LexicalCategory.Unknown) && !String.IsNullOrEmpty(sense.CategoryString)))
                            continue;
                    }

                    if (sense.LanguageSynonyms == null)
                        continue;

                    if (sense.Match(other))
                        return false;

                    int lsi;
                    int lsc = (other.LanguageSynonyms != null ? other.LanguageSynonyms.Count() : 0);
                    bool mergeIt = false;

                    for (lsi = 0; lsi < lsc; lsi++)
                    {
                        LanguageSynonyms osym = other.LanguageSynonyms[lsi];
                        LanguageSynonyms lsym = sense.LanguageSynonyms.FirstOrDefault(x => x.LanguageID == osym.LanguageID);

                        if ((lsym != null) && lsym.HasProbableSynonyms())
                        {
                            int si;
                            int sc = osym.ProbableSynonymCount;

                            for (si = 0; si < sc; si++)
                            {
                                string os = osym.GetSynonymIndexed(si);

                                if (lsym.HasSynonym(os))
                                {
                                    mergeIt = true;
                                    break;
                                }
                            }

                            if (mergeIt)
                                break;
                        }
                    }

                    if (mergeIt)
                    {
                        merged = true;

                        sense.MergeInflections(other.Inflections);

                        if ((sense.Category == LexicalCategory.Unknown) || !String.IsNullOrEmpty(sense.CategoryString))
                        {
                            sense.Category = other.Category;
                            sense.CategoryString = other.CategoryString;
                        }

                        for (lsi = 0; lsi < lsc; lsi++)
                        {
                            LanguageSynonyms osym = other.LanguageSynonyms[lsi];
                            LanguageSynonyms lsym = sense.LanguageSynonyms.FirstOrDefault(x => x.LanguageID == osym.LanguageID);

                            if (lsym != null)
                            {
                                int si;
                                int sc = osym.ProbableSynonymCount;

                                for (si = 0; si < sc; si++)
                                {
                                    ProbableMeaning oProbableMeaning = osym.GetProbableSynonymIndexed(si);
                                    ProbableMeaning lProbableMeaning = lsym.FindProbableSynonym(oProbableMeaning.Meaning);

                                    if (lProbableMeaning == null)
                                        lsym.ProbableSynonyms.Add(new ProbableMeaning(oProbableMeaning));
                                    else
                                        lProbableMeaning.Merge(oProbableMeaning);

                                    ModifiedFlag = true;
                                }
                            }
                            else
                            {
                                sense.LanguageSynonyms.Add(new LanguageSynonyms(osym));
                                ModifiedFlag = true;
                            }
                        }
                    }
                }

                if (!merged)
                    _Senses.Add(other);
                else
                    returnValue = false;

                ModifiedFlag = true;
            }

            // Let importer do this later.
            //SortSensePriority();

            return returnValue;
        }

        // Returns true if new sense added.
        public bool AddUniqueSense(Sense other)
        {
            bool returnValue = true;

            if (other.LanguageSynonyms == null)
                return false;

            if (_Senses == null)
            {
                _Senses = new List<Sense>() { other };
                ModifiedFlag = true;
            }
            else
            {
                foreach (Sense sense in _Senses)
                {
                    if (sense.Reading != other.Reading)
                        continue;

                    LexicalCategory category = sense.Category;

                    if ((other.Category != LexicalCategory.Unknown) || !String.IsNullOrEmpty(other.CategoryString))
                    {
                        if (((category != other.Category) || (sense.CategoryString != other.CategoryString))
                                && ((category != LexicalCategory.Unknown) && !String.IsNullOrEmpty(sense.CategoryString)))
                            continue;
                    }

                    if (sense.LanguageSynonyms == null)
                        continue;

                    if (sense.Match(other))
                    {
                        // The synonyms should all be in the same order here, so we can
                        // merge the probabilities, frequencies, and source IDs.

                        int languageSynonymsCount = sense.LanguageSynonymsCount;
                        int languageSynonymsIndex;

                        for (languageSynonymsIndex = 0; languageSynonymsIndex < languageSynonymsCount; languageSynonymsIndex++)
                        {
                            LanguageSynonyms ls1 = sense.GetLanguageSynonymsIndexed(languageSynonymsIndex);
                            LanguageSynonyms ls2 = other.GetLanguageSynonymsIndexed(languageSynonymsIndex);

                            if ((ls1 == null) || (ls2 == null))
                                break;

                            int synonymCount = ls1.ProbableSynonymCount;
                            int synonymIndex;

                            for (synonymIndex = 0; synonymIndex < synonymCount; synonymIndex++)
                            {
                                ProbableMeaning ps1 = ls1.GetProbableSynonymIndexed(synonymIndex);
                                ProbableMeaning ps2 = ls2.GetProbableSynonymIndexed(synonymIndex);

                                if ((ps1 == null) || (ps2 == null))
                                    break;

                                ps1.Merge(ps2);
                            }
                        }

                        returnValue = false;
                    }
                    else if (sense.CanOverlay(other))   // All the other sense synonyms are present in this sense.
                    {
                        // The synonyms may not be in the same order here, but we can
                        // find the matching synonyms and merge the probabilities, frequencies, and source IDs.

                        int languageSynonymsCount = other.LanguageSynonymsCount;
                        int languageSynonymsIndex;

                        for (languageSynonymsIndex = 0; languageSynonymsIndex < languageSynonymsCount; languageSynonymsIndex++)
                        {
                            LanguageSynonyms ls2 = other.GetLanguageSynonymsIndexed(languageSynonymsIndex);

                            if (ls2 == null)
                                break;

                            foreach (LanguageSynonyms ls1 in sense.LanguageSynonyms)
                            {
                                if (ls1.CanOverlay(ls2))
                                {
                                    int synonymCount = ls2.ProbableSynonymCount;
                                    int synonymIndex;

                                    for (synonymIndex = 0; synonymIndex < synonymCount; synonymIndex++)
                                    {
                                        ProbableMeaning ps2 = ls2.GetProbableSynonymIndexed(synonymIndex);

                                        if (ps2 == null)
                                            break;

                                        ProbableMeaning ps1 = ls1.FindProbableSynonym(ps2.Meaning);

                                        if (ps1 == null)
                                            break;

                                        ps1.Merge(ps2);
                                    }
                                }
                            }

                        }

                        returnValue = false;
                    }
                }

                if (returnValue)
                {
                    _Senses.Add(other);
                    ModifiedFlag = true;
                }
            }

            // Let importer do this later.
            //SortSensePriority();

            return returnValue;
        }

        public void SortPriority()
        {
            SortSensePriority();
            SortAlternatePriority();
        }

        public void SortSensePriority()
        {
            if ((_Senses != null) && (_Senses.Count() >= 2))
            {
                if (_Senses.Count() == 2)
                {
                    if (_Senses[0].PriorityLevel < _Senses[1].PriorityLevel)
                    {
                        Sense tmp = _Senses[0];
                        _Senses[0] = _Senses[1];
                        _Senses[1] = tmp;
                        ModifiedFlag = true;
                    }
                }
                else
                {
                    int i;
                    int c = _Senses.Count() - 1;

                    for (; ; )
                    {
                        bool finished = true;

                        for (i = 0; i < c; i++)
                        {
                            if (_Senses[i].PriorityLevel < _Senses[i + 1].PriorityLevel)
                            {
                                Sense tmp = _Senses[i];
                                _Senses[i] = _Senses[i + 1];
                                _Senses[i + 1] = tmp;
                                ModifiedFlag = true;
                                finished = false;
                            }
                        }

                        if (finished)
                            break;
                    }
                }
            }
        }

        public void SortAlternatePriority()
        {
            if ((_Alternates != null) && (_Alternates.Count() >= 1) && (_Senses != null) && (_Senses.Count() != 0))
            {
                List<LanguageString> oldAlternates = new List<LanguageString>(_Alternates);
                List<LanguageString> newAlternates = new List<LanguageString>();
                Dictionary<int, int> readingMap = new Dictionary<int, int>();
                int newReading = 0;

                foreach (Sense sense in _Senses)
                {
                    int oldReading = sense.Reading;
                    int index;
                    int currentReading;

                    if (!readingMap.TryGetValue(oldReading, out currentReading))
                    {
                        currentReading = newReading++;
                        readingMap.Add(oldReading, currentReading);
                    }

                    for (index = 0; index < oldAlternates.Count(); )
                    {
                        LanguageString alternate = oldAlternates[index];

                        if (alternate.KeyInt == oldReading)
                        {
                            oldAlternates.RemoveAt(index);
                            newAlternates.Add(alternate);
                            alternate.Key = currentReading;
                        }
                        else
                            index++;
                    }

                    sense.Reading = currentReading;
                }

                if (oldAlternates.Count() != 0)
                {
                    foreach (LanguageString alternate in oldAlternates)
                    {
                        int oldReading = alternate.KeyInt;
                        int currentReading;

                        if (!readingMap.TryGetValue(oldReading, out currentReading))
                        {
                            currentReading = newReading++;
                            readingMap.Add(oldReading, currentReading);
                        }

                        alternate.Key = currentReading;
                    }

                    newAlternates.AddRange(oldAlternates);
                }

                _Alternates = newAlternates;
                ModifiedFlag = true;                
            }

            /*
            if ((_Alternates != null) && (_Alternates.Count() >= 2))
            {
                foreach (LanguageString ls in _Alternates)
                {
                    if (ls.KeyType != typeof(int))
                        ls.Key = 0;
                }

                if (_Alternates.Count() == 2)
                {
                    if (ObjectUtilities.CompareKeys(_Alternates[0], _Alternates[1]) < 0)
                    {
                        LanguageString tmp = _Alternates[0];
                        _Alternates[0] = _Alternates[1];
                        _Alternates[1] = tmp;
                        ModifiedFlag = true;
                    }
                }
                else
                {
                    int i;
                    int c = _Alternates.Count() - 1;

                    for (;;)
                    {
                        bool finished = true;

                        for (i = 0; i < c; i++)
                        {
                            if (ObjectUtilities.CompareKeys(_Alternates[i], _Alternates[i + 1]) < 0)
                            {
                                LanguageString tmp = _Alternates[i];
                                _Alternates[i] = _Alternates[i + 1];
                                _Alternates[i + 1] = tmp;
                                ModifiedFlag = true;
                                finished = false;
                            }
                        }

                        if (finished)
                            break;
                    }
                }
            }
            */
        }

        // Holds different representations of this entry's key, such as a phonetic spelling, romanization, or alternate language characters.
        public List<LanguageString> Alternates
        {
            get
            {
                return _Alternates;
            }
            set
            {
                if (value != _Alternates)
                {
                    ModifiedFlag = true;
                    _Alternates = value;
                }
            }
        }

        public LanguageString GetAlternate(LanguageID languageID)
        {
            if (_Alternates == null)
                return null;

            return _Alternates.FirstOrDefault(x => x.LanguageID == languageID);
        }

        public LanguageString GetAlternate(LanguageID languageID, int reading)
        {
            if (_Alternates == null)
                return null;

            return _Alternates.FirstOrDefault(x => (x.LanguageID == languageID) && (x.KeyInt == reading));
        }

        public void AddAlternate(LanguageString alternate)
        {
            if (_Alternates == null)
                _Alternates = new List<LanguageString>(1) { alternate };
            else
                _Alternates.Add(alternate);
        }

        public int AlternateCount
        {
            get
            {
                if (_Alternates == null)
                    return 0;
                else
                    return _Alternates.Count();
            }
        }

        public bool HasAlternateLanguage(LanguageID languageID)
        {
            if (_Alternates == null)
                return false;

            foreach (LanguageString alternate in _Alternates)
            {
                if (alternate.LanguageID == languageID)
                    return true;
            }

            return false;
        }

        public bool HasAlternates()
        {
            if (_Alternates == null)
                return false;
            else
                return (_Alternates.Count() != 0 ? true : false);
        }

        public int AllocateAlternateKey()
        {
            int alternateKey = 0;

            if ((_Alternates == null) || (_Alternates.Count() == 0))
                return alternateKey;

            foreach (LanguageString alternate in _Alternates)
            {
                if (alternate.KeyInt >= alternateKey)
                    alternateKey = alternate.KeyInt + 1;
            }

            return alternateKey;
        }

        public int AllocateAlternateKey(LanguageID languageID)
        {
            int alternateKey = 0;

            if ((_Alternates == null) || (_Alternates.Count() == 0))
                return alternateKey;

            foreach (LanguageString alternate in _Alternates)
            {
                if (alternate.LanguageID == languageID)
                    alternateKey++;
            }

            return alternateKey;
        }

        public int AllocateAlternateKey(LanguageID languageID, string text)
        {
            int alternateKey = 0;

            if ((_Alternates == null) || (_Alternates.Count() == 0))
                return alternateKey;

            foreach (LanguageString alternate in _Alternates)
            {
                if (alternate.LanguageID == languageID)
                {
                    if (text == alternate.Text)
                        return alternate.KeyInt;

                    alternateKey++;
                }
            }

            return alternateKey;
        }

        public int GetHighestAlternateKey()
        {
            int alternateKey = 0;

            if ((_Alternates == null) || (_Alternates.Count() == 0))
                return alternateKey;

            foreach (LanguageString alternate in _Alternates)
            {
                if (alternate.KeyInt > alternateKey)
                    alternateKey = alternate.KeyInt;
            }

            return alternateKey;
        }

        public List<int> SourceIDs
        {
            get
            {
                return _SourceIDs;
            }
            set
            {
                if (ObjectUtilities.CompareIntLists(value, _SourceIDs) != 0)
                {
                    _SourceIDs = value;
                    ModifiedFlag = true;
                }
            }
        }

        public bool HasSourceIDs()
        {
            if ((SourceIDs != null) && (SourceIDs.Count() != 0))
                return true;
            return false;
        }

        public bool HasSourceID(int sourceID)
        {
            if (_SourceIDs == null)
                return false;

            return _SourceIDs.Contains(sourceID);
        }

        public int GetSourceIDIndexed(int index)
        {
            if ((_SourceIDs == null) || (index < 0) || (index >= _SourceIDs.Count()))
                return 0;

            return _SourceIDs[index];
        }

        public void AddUniqueSourceID(int sourceID)
        {
            if (sourceID == 0)
                return;
            if (_SourceIDs == null)
            {
                _SourceIDs = new List<int>() { sourceID };
                ModifiedFlag = true;
            }
            else if (!_SourceIDs.Contains(sourceID))
            {
                _SourceIDs.Add(sourceID);
                ModifiedFlag = true;
            }
        }

        public void MergeSourceIDs(List<int> sourceIDs)
        {
            if ((sourceIDs == null) || (sourceIDs == _SourceIDs) || (sourceIDs.Count() == 0))
                return;
            if (_SourceIDs == null)
                _SourceIDs = new List<int>();
            foreach (int sourceID in sourceIDs)
            {
                if (!_SourceIDs.Contains(sourceID))
                {
                    _SourceIDs.Add(sourceID);
                    ModifiedFlag = true;
                }
            }
        }

        public void ClearSourceIDs()
        {
            if ((_SourceIDs != null) && (_SourceIDs.Count() != 0))
                ModifiedFlag = true;

            _SourceIDs = null;
        }

        public int SourceIDCount()
        {
            if (_SourceIDs == null)
                return 0;
            return _SourceIDs.Count();
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

        public string GetSourceNames()
        {
            return ApplicationData.DictionarySourcesLazy.GetByIDList(SourceIDs);
        }

        public string GetSourceNamesWithExclusion(List<int> excludeIDs)
        {
            return ApplicationData.DictionarySourcesLazy.GetByIDListWithExclusion(SourceIDs, excludeIDs);
        }

        // Contains the per-language definitions for this entry.
        public List<Sense> Senses
        {
            get
            {
                return _Senses;
            }
            set
            {
                if (value != _Senses)
                {
                    ModifiedFlag = true;
                    _Senses = value;
                }
            }
        }

        public int SenseCount
        {
            get
            {
                if (_Senses != null)
                    return _Senses.Count();
                return 0;
            }
        }

        public int GetSenseCount(List<LanguageDescriptor> languageDescriptors)
        {
            int count = 0;
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if (sense.HasAnyLanguage(languageDescriptors))
                        count++;
                }
            }
            return count;
        }

        public int GetSenseCount(List<LanguageID> languageIDs)
        {
            int count = 0;
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if (sense.HasAnyLanguage(languageIDs))
                        count++;
                }
            }
            return count;
        }

        public int GetSenseCount(LanguageID languageID)
        {
            int count = 0;
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if (sense.HasLanguage(languageID))
                        count++;
                }
            }
            return count;
        }

        public int GetCategorizedSenseCount(LanguageID languageID, LexicalCategory category)
        {
            int count = 0;
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if (sense.HasLanguage(languageID) && (sense.Category == category))
                        count++;
                }
            }
            return count;
        }

        public int GetRowCount(List<LanguageDescriptor> languageDescriptors, bool showExamples, bool showExampleTitle)
        {
            int count = 0;
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if (!sense.HasAnyLanguage(languageDescriptors))
                        continue;

                    count += sense.GetRowCount(languageDescriptors, showExamples, showExampleTitle);
                }
            }
            if (count == 0)
                count = 1;
            return count;
        }

        public Sense GetSenseIndexed(int index)
        {
            if (_Senses != null)
            {
                if ((index >= 0) && (index < _Senses.Count()))
                    return _Senses[index];
            }
            return null;
        }

        public Sense GetSenseWithLanguageID(LanguageID languageID)
        {
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if (sense.HasLanguage(languageID))
                        return sense;
                }
            }
            return null;
        }

        public List<Sense> GetSensesWithLanguageID(LanguageID languageID)
        {
            List<Sense> senses = new List<Sense>();

            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if (sense.HasLanguage(languageID))
                        senses.Add(sense);
                }
            }

            return senses;
        }

        public Sense GetSenseWithReading(int reading)
        {
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if (sense.Reading == reading)
                        return sense;
                }
            }
            return null;
        }

        // Get first sense with reading.
        public Sense GetSenseWithReading(
            int reading,
            List<LanguageID> alternateLanguageIDs)
        {
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if ((sense.Reading == reading) && sense.HasAnyLanguage(alternateLanguageIDs))
                        return sense;
                }
            }
            return null;
        }

        // Get all senses with reading
        public List<Sense> GetSensesWithReading(
            int reading,
            List<LanguageID> alternateLanguageIDs)
        {
            List<Sense> senses = new List<Sense>();

            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if ((sense.Reading == reading) && sense.HasAnyLanguage(alternateLanguageIDs))
                        senses.Add(sense);
                }
            }

            return senses;
        }

        public Sense GetCategorizedSense(LanguageID languageID, LexicalCategory category)
        {
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if (!sense.HasLanguage(languageID))
                        continue;

                    if (sense.Category != category)
                        continue;

                    return sense;
                }
            }
            return null;
        }

        public Sense GetCategorizedSense(LanguageID languageID, LexicalCategory category, string categorySubString)
        {
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if (!sense.HasLanguage(languageID))
                        continue;

                    if (sense.Category != category)
                        continue;

                    if (String.IsNullOrEmpty(sense.CategoryString) || !sense.CategoryString.Contains(categorySubString))
                        continue;

                    return sense;
                }
            }
            return null;
        }

        public Sense GetLanguageSenseWithoutCategory(LanguageID languageID, LexicalCategory category)
        {
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if (!sense.HasLanguage(languageID))
                        continue;

                    if (sense.Category == category)
                        continue;

                    return sense;
                }
            }
            return null;
        }

        public Sense GetSenseWithoutCategory(LexicalCategory category)
        {
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if (sense.Category == category)
                        continue;

                    return sense;
                }
            }
            return null;
        }

        public Sense GetSenseWithoutStem()
        {
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if ((sense.Category == LexicalCategory.Stem) || (sense.Category == LexicalCategory.IrregularStem))
                        continue;

                    return sense;
                }
            }
            return null;
        }

        public Sense FindSenseWithSynonym(string synonym, LanguageID languageID)
        {
            if (_Senses == null)
                return null;

            foreach (Sense sense in _Senses)
            {
                LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(languageID);

                if (languageSynonyms == null)
                    continue;

                ProbableMeaning probableMeaning = languageSynonyms.FindProbableSynonym(synonym);

                if (probableMeaning != null)
                    return sense;
            }

            return null;
        }

        public Sense FindSenseWithSynonymAndCategory(
            string synonym,
            LanguageID languageID,
            LexicalCategory category)
        {
            if (_Senses == null)
                return null;

            foreach (Sense sense in _Senses)
            {
                if (sense.Category != category)
                    continue;

                LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(languageID);

                if (languageSynonyms == null)
                    continue;

                ProbableMeaning probableMeaning = languageSynonyms.FindProbableSynonym(synonym);

                if (probableMeaning != null)
                    return sense;
            }

            return null;
        }

        public void AddSense(Sense sense)
        {
            if (_Senses == null)
                _Senses = new List<Sense>(1);

            _Senses.Add(sense);
        }

        public bool InsertSense(int index, Sense sense)
        {
            if (_Senses == null)
                _Senses = new List<Sense>(1) { sense };
            else if ((index >= 0) && (index <= _Senses.Count()))
                _Senses.Insert(index, sense);
            else
                return false;

            return true;
        }

        public void InsertSenseByFrequencyOrProbability(
            Sense sense,
            LanguageID languageID,
            int synonymIndex)
        {
            if (_Senses == null)
                _Senses = new List<Sense>(1) { sense };
            else
            {
                if (synonymIndex == -1)
                {
                    AddSense(sense);
                    return;
                }

                ProbableMeaning probableSynonym = sense.GetProbableSynonymIndexed(languageID, synonymIndex);

                if (probableSynonym != null)
                {
                    if ((probableSynonym.Frequency == 0) || (probableSynonym.Probability == 0.0f))
                    {
                        AddSense(sense);
                        return;
                    }
                }
                else
                {
                    AddSense(sense);
                    return;
                }

                for (int senseIndex = _Senses.Count() - 1; senseIndex >= 0; senseIndex--)
                {
                    Sense otherSense = _Senses[senseIndex];
                    LanguageSynonyms languageSynonyms = otherSense.GetLanguageSynonyms(languageID);

                    if ((languageSynonyms == null) || (languageSynonyms.ProbableSynonymCount == 0))
                        continue;

                    bool haveOne = false;

                    foreach (ProbableMeaning otherProbableSynonym in languageSynonyms.ProbableSynonyms)
                    {
                        if ((probableSynonym.Frequency < otherProbableSynonym.Frequency) &&
                                (probableSynonym.Probability < otherProbableSynonym.Probability))
                            haveOne = true;
                    }

                    if (haveOne)
                    {
                        InsertSense(senseIndex + 1, sense);
                        return;
                    }
                }

                AddSense(sense);
            }
        }

        public void InsertSenseByProbability(
            Sense sense,
            LanguageID languageID,
            int synonymIndex)
        {
            if (_Senses == null)
                _Senses = new List<Sense>(1) { sense };
            else
            {
                if (synonymIndex == -1)
                {
                    AddSense(sense);
                    return;
                }

                ProbableMeaning probableSynonym = sense.GetProbableSynonymIndexed(languageID, synonymIndex);

                if (probableSynonym != null)
                {
                    if (probableSynonym.Probability == 0.0f)
                    {
                        AddSense(sense);
                        return;
                    }
                }
                else
                {
                    AddSense(sense);
                    return;
                }

                for (int senseIndex = _Senses.Count() - 1; senseIndex >= 0; senseIndex--)
                {
                    Sense otherSense = _Senses[senseIndex];
                    LanguageSynonyms languageSynonyms = otherSense.GetLanguageSynonyms(languageID);

                    if ((languageSynonyms == null) || (languageSynonyms.ProbableSynonymCount == 0))
                        continue;

                    bool haveOne = false;

                    foreach (ProbableMeaning otherProbableSynonym in languageSynonyms.ProbableSynonyms)
                    {
                        if (probableSynonym.Probability < otherProbableSynonym.Probability)
                            haveOne = true;
                    }

                    if (haveOne)
                    {
                        InsertSense(senseIndex + 1, sense);
                        return;
                    }
                }

                AddSense(sense);
            }
        }

        public bool DeleteSenseIndexed(int index)
        {
            if ((index >= 0) && (index < SenseCount))
            {
                Senses.RemoveAt(index);
                return true;
            }

            return false;
        }

        public bool HasSenses()
        {
            return SenseCount != 0;
        }

        public LanguageID FirstSenseLanguageID()
        {
            Sense sense = GetSenseIndexed(0);

            if (sense == null)
                return null;

            LanguageSynonyms languageSynonyms = sense.GetLanguageSynonymsIndexed(0);

            if (languageSynonyms == null)
                return null;

            return languageSynonyms.LanguageID;
        }

        public bool HasSenseLanguage(LanguageID languageID)
        {
            if (_Senses == null)
                return false;

            foreach (Sense sense in _Senses)
            {
                if (sense.HasLanguage(languageID))
                    return true;
            }

            return false;
        }

        public bool HasAnySenseLanguage(List<LanguageID> languageIDs)
        {
            if (_Senses == null)
                return false;

            foreach (Sense sense in _Senses)
            {
                if (sense.HasAnyLanguage(languageIDs))
                    return true;
            }

            return false;
        }

        public bool HasAnySenseLanguage(List<LanguageDescriptor> languageDescriptors)
        {
            if (_Senses == null)
                return false;

            foreach (Sense sense in _Senses)
            {
                if (sense.HasAnyLanguage(languageDescriptors))
                    return true;
            }

            return false;
        }

        public bool HasSenseWithCategory(LexicalCategory category)
        {
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if (sense.Category == category)
                        return true;
                }
            }
            return false;
        }

        public bool HasSenseWithStem()
        {
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if ((sense.Category == LexicalCategory.Stem) || (sense.Category == LexicalCategory.IrregularStem))
                        return true;
                }
            }
            return false;
        }

        public bool HasSenseWithoutCategory(LexicalCategory category)
        {
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if (sense.Category != category)
                        return true;
                }
            }
            return false;
        }

        public bool HasSenseWithoutStem()
        {
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if ((sense.Category != LexicalCategory.Stem) && (sense.Category != LexicalCategory.IrregularStem))
                        return true;
                }
            }
            return false;
        }

        public bool HasSenseWithCategoryOnly(LexicalCategory category)
        {
            int withCount = 0;
            int withoutCount = 0;

            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if (sense.Category == category)
                        withCount++;
                    else
                        withoutCount++;
                }
            }
            if ((withCount != 0) && (withoutCount == 0))
                return true;
            return false;
        }

        public bool HasSenseWithStemOnly()
        {
            int withCount = 0;
            int withoutCount = 0;

            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if ((sense.Category == LexicalCategory.Stem) || (sense.Category == LexicalCategory.IrregularStem))
                        withCount++;
                    else
                        withoutCount++;
                }
            }
            if ((withCount != 0) && (withoutCount == 0))
                return true;
            return false;
        }

        public bool HasSenseWithCategorySubString(string categorySubString)
        {
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if (sense.CategoryString.Contains(categorySubString))
                    {
                        string[] parts = sense.CategoryString.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string part in parts)
                        {
                            if (part == categorySubString)
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool HasSenseWithReading(int reading)
        {
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if (sense.Reading == reading)
                        return true;
                }
            }
            return false;
        }

        public bool HasMeaning(string meaning, LanguageID languageID)
        {
            if (_Senses == null)
                return false;

            foreach (Sense sense in _Senses)
            {
                if (sense.HasMeaning(meaning, languageID))
                    return true;
            }

            return false;
        }

        public bool HasMeaning(string meaning, LanguageID languageID, LexicalCategory category, string extraCategory)
        {
            if (_Senses == null)
                return false;

            foreach (Sense sense in _Senses)
            {
                if (category != LexicalCategory.Unknown)
                {
                    if (sense.Category != category)
                        continue;

                    if (!String.IsNullOrEmpty(extraCategory))
                    {
                        if (sense.CategoryString != extraCategory)
                            continue;
                    }
                }

                if (sense.HasMeaning(meaning, languageID))
                    return true;
            }

            return false;
        }

        public int HighestSensePriority()
        {
            int p = -1000;

            if (_Senses == null)
                return p;

            foreach (Sense sense in _Senses)
            {
                if (sense.PriorityLevel > p)
                    p = sense.PriorityLevel;
            }

            return p;
        }

        public bool GetSenseContainingText(
            LanguageID languageID,
            string containingText,
            out Sense sense,
            out int senseIndex,
            out int synonymIndex,
            out string definition)
        {
            sense = null;
            senseIndex = -1;
            synonymIndex = -1;
            definition = String.Empty;

            if (SenseCount == 0)
                return false;

            int index = 0;

            foreach (Sense aSense in _Senses)
            {
                if (aSense.GetSynonymContainingText(languageID, containingText, out synonymIndex, out definition))
                {
                    sense = aSense;
                    senseIndex = index;
                    return true;
                }

                index++;
            }

            return false;
        }

        public bool GetSenseMatchingText(
            LanguageID languageID,
            string matchingText,
            out Sense sense,
            out int senseIndex,
            out int synonymIndex)
        {
            sense = null;
            senseIndex = -1;
            synonymIndex = -1;

            if (SenseCount == 0)
                return false;

            int index = 0;

            foreach (Sense aSense in _Senses)
            {
                if (aSense.GetSynonymMatchingText(languageID, matchingText, out synonymIndex))
                {
                    sense = aSense;
                    senseIndex = index;
                    return true;
                }

                index++;
            }

            return false;
        }

        public bool GetProbableSynonymContainingText(
            LanguageID languageID,
            string containingText,
            out int synonymIndex,
            out ProbableMeaning definition)
        {
            synonymIndex = -1;
            definition = null;

            if (SenseCount == 0)
                return false;

            foreach (Sense sense in _Senses)
            {
                if (sense.GetProbableSynonymContainingText(
                        languageID,
                        containingText,
                        out synonymIndex,
                        out definition))
                    return true;
            }

            return false;
        }

        public bool SortProbableSynonymsBySourceCount(LanguageID languageID)
        {
            if (SenseCount == 0)
                return false;

            int count = int.MaxValue;
            bool needsSort = false;
            bool returnValue = false;

            foreach (Sense sense in _Senses)
            {
                int thisCount = sense.MaxSourceIDCount(languageID);

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

            if (needsSort)
            {
                List<Sense> newList = new List<Sense>();

                foreach (Sense sense in _Senses)
                {
                    int c = newList.Count();
                    int i;
                    int maxCount = sense.MaxSourceIDCount(languageID);
                    bool inserted = false;

                    for (i = 0; i < c; i++)
                    {
                        Sense obj = newList[i];
                        int newCount = obj.MaxSourceIDCount(languageID);

                        if (maxCount > newCount)
                        {
                            newList.Insert(i, sense);
                            inserted = true;
                            break;
                        }
                    }

                    if (!inserted)
                        newList.Add(sense);
                }

                _Senses = newList;
                returnValue = true;
            }

            foreach (Sense sense in _Senses)
            {
                if (sense.SortProbableSynonymsBySourceCount(languageID))
                    returnValue = true;
            }

            return returnValue;
        }

        public bool DeleteProbableSynonym(
            LanguageID hostLanguageID,
            ProbableMeaning targetProbableSynonym)
        {
            int senseCount = SenseCount;
            int senseIndex;
            bool returnValue = false;

            for (senseIndex = senseCount - 1; senseIndex >= 0; senseIndex--)
            {
                Dictionary.Sense sense = GetSenseIndexed(senseIndex);
                LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(hostLanguageID);

                if (languageSynonyms == null)
                    continue;

                int probableSynonymIndex = languageSynonyms.GetProbableSynonymIndex(targetProbableSynonym);

                if (probableSynonymIndex != -1)
                {
                    languageSynonyms.DeleteSynonymIndexed(probableSynonymIndex);

                    if (languageSynonyms.ProbableSynonymCount == 0)
                    {
                        sense.RemoveLanguageSynonyms(languageSynonyms);

                        if (sense.LanguageSynonymsCount == 0)
                            DeleteSenseIndexed(senseIndex);
                    }

                    returnValue = true;
                }
            }

            return returnValue;
        }

        public bool FindAndReplaceProbableSynonym(
            ProbableMeaning probableSynonymOld,
            LanguageID hostLanguageID,
            ProbableMeaning probableSynonymNew)
        {
            int senseCount = SenseCount;
            int senseIndex;
            bool returnValue = false;

            for (senseIndex = senseCount - 1; senseIndex >= 0; senseIndex--)
            {
                Dictionary.Sense sense = GetSenseIndexed(senseIndex);
                LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(hostLanguageID);

                if (languageSynonyms == null)
                    continue;

                int probableSynonymIndex = languageSynonyms.GetProbableSynonymIndex(probableSynonymOld);

                if (probableSynonymIndex != -1)
                {
                    languageSynonyms.DeleteSynonymIndexed(probableSynonymIndex);
                    languageSynonyms.InsertProbableSynonym(probableSynonymIndex, probableSynonymNew);

                    returnValue = true;
                }
            }

            return returnValue;
        }

        public int MaxSourceIDCount(LanguageID languageID)
        {
            if (SenseCount == 0)
                return 0;

            int maxCount = 0;

            foreach (Sense sense in _Senses)
            {
                int thisCount = sense.MaxSourceIDCount(languageID);

                if (thisCount > maxCount)
                    maxCount = thisCount;
            }

            return maxCount;
        }

        public int MaxFrequency(LanguageID languageID)
        {
            if (SenseCount == 0)
                return 0;

            int maxCount = 0;

            foreach (Sense sense in _Senses)
            {
                int thisCount = sense.MaxFrequency(languageID);

                if (thisCount > maxCount)
                    maxCount = thisCount;
            }

            return maxCount;
        }

        public List<LexicalCategory> CollectUniqueCategories()
        {
            List<LexicalCategory> categories = new List<LexicalCategory>();

            if (SenseCount == 0)
                return categories;

            foreach (Sense sense in _Senses)
            {
                if (sense.Category == LexicalCategory.Unknown)
                    continue;

                if (!categories.Contains(sense.Category))
                    categories.Add(sense.Category);
            }

            return categories;
        }

        public List<string> CollectUniqueCategoryStrings()
        {
            List<string> categoryStrings = new List<string>();

            if (SenseCount == 0)
                return categoryStrings;

            foreach (Sense sense in _Senses)
            {
                if (String.IsNullOrEmpty(sense.CategoryString))
                    continue;

                if (!categoryStrings.Contains(sense.CategoryString))
                    categoryStrings.Add(sense.CategoryString);
            }

            return categoryStrings;
        }

        public bool HasAlternate(string text, LanguageID languageID)
        {
            if (AlternateCount == 0)
                return false;

            string normalizedText = TextUtilities.GetNormalizedString(text).ToLower();

            if (LanguageLookup.IsAlternatePhonetic(languageID))
                ConvertPinyinNumeric.ToNumeric(out normalizedText, normalizedText);

            foreach (LanguageString alternate in _Alternates)
            {
                if (alternate.LanguageID != languageID)
                    continue;

                if (alternate.TextLower == normalizedText.ToLower())
                    return true;
            }

            return false;
        }

        public string GetFirstAlternateText(LanguageID languageID)
        {
            if (_Alternates != null)
            {
                foreach (LanguageString alternate in _Alternates)
                {
                    if (alternate.LanguageID != languageID)
                        continue;

                    return alternate.Text;
                }
            }

            return String.Empty;
        }

        public string GetAlternateDefinition(LanguageID languageID)
        {
            string alternates = String.Empty;

            if (_Alternates != null)
            {
                foreach (LanguageString alternate in _Alternates)
                {
                    if (alternate.LanguageID != languageID)
                        continue;

                    string text = alternate.Text;

                    if (LanguageLookup.IsAlternatePhonetic(languageID))
                        ConvertPinyinNumeric.ToToneMarks(out text, text);

                    if (alternates.Length != 0)
                        alternates += " / " + text;
                    else
                        alternates = text;
                }
            }

            return alternates;
        }

        public string GetAlternateDefinitionMarkedUp(LanguageID languageID, List<LanguageDescription> languageDescriptions)
        {
            string alternates = String.Empty;

            if (_Alternates != null)
            {
                foreach (LanguageString alternate in _Alternates)
                {
                    if (alternate.LanguageID != languageID)
                        continue;

                    string text = alternate.Text;

                    if (LanguageLookup.IsAlternatePhonetic(languageID))
                        ConvertPinyinNumeric.ToToneMarks(out text, text);

                    text = LanguageUtilities.FormatDictionaryText(text, alternate.LanguageID, languageDescriptions);

                    if (alternates.Length != 0)
                        alternates += " / " + text;
                    else
                        alternates = text;
                }
            }

            return alternates;
        }

        public string GetAlternateDefinitions()
        {
            string alternates = String.Empty;

            if (_Alternates != null)
            {
                foreach (LanguageString alternate in _Alternates)
                {
                    string text = alternate.Text;

                    if (LanguageLookup.IsAlternatePhonetic(alternate.LanguageID))
                        ConvertPinyinNumeric.ToToneMarks(out text, text);

                    if (alternates.Length != 0)
                        alternates += " / " + text;
                    else
                        alternates = text;
                }
            }

            return alternates;
        }

        public string GetAlternateDefinitionsMarkedUp(List<LanguageDescription> languageDescriptions)
        {
            string alternates = String.Empty;

            if (_Alternates != null)
            {
                foreach (LanguageString alternate in _Alternates)
                {
                    string text = alternate.Text;

                    if (LanguageLookup.IsAlternatePhonetic(alternate.LanguageID))
                        ConvertPinyinNumeric.ToToneMarks(out text, text);

                    text = LanguageUtilities.FormatDictionaryText(text, alternate.LanguageID, languageDescriptions);

                    if (alternates.Length != 0)
                        alternates += " / " + text;
                    else
                        alternates = text;
                }
            }

            return alternates;
        }

        public string GetAlternateTranslation(LanguageID languageID)
        {
            string alternates = String.Empty;

            if (_Alternates != null)
            {
                foreach (LanguageString alternate in _Alternates)
                {
                    if (alternate.LanguageID != languageID)
                        continue;

                    string text = alternate.Text;

                    if (LanguageLookup.IsAlternatePhonetic(languageID))
                        ConvertPinyinNumeric.ToToneMarks(out text, text);

                    return text;
                }
            }

            return alternates;
        }

        public string GetAlternateTranslationWithReading(LanguageID languageID, int reading)
        {
            string alternates = String.Empty;

            if (_Alternates != null)
            {
                foreach (LanguageString alternate in _Alternates)
                {
                    if (alternate.LanguageID != languageID)
                        continue;

                    if (alternate.KeyInt != reading)
                        continue;

                    string text = alternate.Text;

                    if (LanguageLookup.IsAlternatePhonetic(languageID))
                        ConvertPinyinNumeric.ToToneMarks(out text, text);

                    return text;
                }
            }

            return alternates;
        }

        public string GetFullDefinition(int indentCount, int indentSize, bool showLexicalTag)
        {
            StringBuilder sb = new StringBuilder();

            string text = KeyString;

            if (LanguageLookup.IsAlternatePhonetic(LanguageID))
                ConvertPinyinNumeric.ToToneMarks(out text, text);

            sb.Append(TextUtilities.GetSpaces(indentCount * indentSize));
            sb.Append(text);

            if (AlternateCount != 0)
            {
                foreach (LanguageString alternate in _Alternates)
                    sb.Append(" (" + alternate.Text + "," + alternate.KeyString + ")");
            }

            sb.AppendLine(":");

            int senseCount = SenseCount;

            if (senseCount != 0)
            {
                int index = 1;

                foreach (Sense sense in _Senses)
                {
                    string infoString = String.Empty;

                    if (showLexicalTag)
                    {
                        if ((sense.Category != LexicalCategory.Unknown) || !String.IsNullOrEmpty(sense.CategoryString))
                        {
                            if (sense.Category != LexicalCategory.Unknown)
                                infoString = sense.Category.ToString();

                            if (!String.IsNullOrEmpty(sense.CategoryString))
                            {
                                if (!String.IsNullOrEmpty(infoString))
                                    infoString += ", ";

                                infoString += sense.CategoryString;
                            }

                            infoString = "Categories(" + infoString + ")";
                        }

                        if ((sense.Reading != -1) && (AlternateCount != 0))
                            infoString += " Reading(" + sense.Reading.ToString() + ")";

                        if (!String.IsNullOrEmpty(infoString))
                            infoString = " " + infoString;
                    }

                    sb.Append(TextUtilities.GetSpaces((indentCount + 1) * indentSize));
                    sb.AppendLine(
                        index.ToString()
                            + ":"
                            + infoString);

                    if (sense.LanguageSynonymsCount != 0)
                    {
                        foreach (LanguageSynonyms languageSynonyms in sense.LanguageSynonyms)
                        {
                            sb.Append(TextUtilities.GetSpaces((indentCount + 2) * indentSize));
                            sb.Append(
                                languageSynonyms.LanguageID.LanguageCultureExtensionCode
                                    + ": ");

                            if (languageSynonyms.ProbableSynonymCount != 0)
                            {
                                bool first = true;

                                foreach (ProbableMeaning probableSynonym in languageSynonyms.ProbableSynonyms)
                                {
                                    if (first)
                                        first = false;
                                    else
                                        sb.Append(", ");

                                    string synonym = probableSynonym.Meaning;

                                    if ((probableSynonym.Probability != 0.0f) && !float.IsNaN(probableSynonym.Probability))
                                        synonym += " P(" + probableSynonym.Probability.ToString() + ")";

                                    if (probableSynonym.Frequency != 0)
                                        synonym += " F(" + probableSynonym.Frequency.ToString() + ")";

                                    if (probableSynonym.HasSourceIDs())
                                        synonym += " S(" + probableSynonym.GetSourceNames() + ")";

                                    if (probableSynonym.Inflections != null)
                                        synonym += " " + probableSynonym.GetInflectionsDisplay();

                                    if (!String.IsNullOrEmpty(probableSynonym.CategoryString) && probableSynonym.CategoryString.Contains("dictionary"))
                                        synonym += " (dictionary form)";

                                    sb.Append(synonym);
                                }

                                sb.AppendLine(String.Empty);
                            }
                        }
                    }

                    int exampleCount = sense.ExampleCount;

                    if (exampleCount != 0)
                    {
                        sb.Append(TextUtilities.GetSpaces((indentCount + 1) * indentSize));
                        sb.Append("Examples:");

                        foreach (MultiLanguageString example in sense.Examples)
                        {
                            if (example.Count() != 0)
                            {
                                foreach (LanguageString languageString in example.LanguageStrings)
                                {
                                    sb.Append(TextUtilities.GetSpaces((indentCount + 2) * indentSize));
                                    sb.AppendLine(languageString.Text);
                                }
                            }
                        }
                    }

                    index++;
                }
            }

            return sb.ToString();
        }

        public string GetDefinition(LanguageID languageID, bool showLexicalTag, bool showSources)
        {
            if (languageID == LanguageID)
            {
                string text = KeyString;

                if (LanguageLookup.IsAlternatePhonetic(languageID))
                    ConvertPinyinNumeric.ToToneMarks(out text, text);

                if (showSources && HasSourceIDs())
                    text += " {" + GetSourceNames() + "}";

                return text;
            }

            string definition = GetAlternateDefinition(languageID);

            if (!String.IsNullOrEmpty(definition))
                return definition;

            int senseCount = (_Senses == null ? 0 : _Senses.Count());

            if (senseCount == 0)
                definition = String.Empty;
            else if (senseCount == 1)
                definition = _Senses[0].GetDefinition(languageID, showLexicalTag, showSources);
            else
            {
                StringBuilder sb = new StringBuilder();
                int index = 1;

                foreach (Sense sense in _Senses)
                {
                    definition = sense.GetDefinition(languageID, showLexicalTag, showSources);
                    if (!String.IsNullOrEmpty(definition))
                    {
                        if (index != 1)
                            sb.Append(LanguageLookup.GetLanguageSpaces(languageID, 2));
                        sb.Append(index);
                        sb.Append(LanguageLookup.GetLanguagePeriodAndSpace(languageID));
                        sb.Append(definition);
                        if (!LanguageLookup.EndsWithPeriod(definition))
                            sb.Append(LanguageLookup.GetLanguagePeriod(languageID) + "\n");
                        else
                            sb.Append("\n");
                        index++;
                    }
                }

                definition = sb.ToString();
            }

            return definition;
        }

        public string GetDefinitionMarkedUp(
            LanguageID languageID,
            List<LanguageDescription> languageDescriptions,
            bool showLexicalTag,
            bool showSources)
        {
            if (languageID == LanguageID)
            {
                string text = KeyString;

                if (LanguageLookup.IsAlternatePhonetic(languageID))
                    ConvertPinyinNumeric.ToToneMarks(out text, text);

                text = LanguageUtilities.FormatDictionaryText(text, languageID, languageDescriptions);

                return text;
            }

            string definition = GetAlternateDefinitionMarkedUp(languageID, languageDescriptions);

            if (!String.IsNullOrEmpty(definition))
                return definition;

            int senseCount = (_Senses == null ? 0 : _Senses.Count());

            if (senseCount == 0)
                definition = String.Empty;
            else if (senseCount == 1)
                definition = _Senses[0].GetDefinitionMarkedUp(languageID, languageDescriptions, showLexicalTag, showSources);
            else
            {
                StringBuilder sb = new StringBuilder();
                int index = 1;

                foreach (Sense sense in _Senses)
                {
                    definition = sense.GetDefinitionMarkedUp(languageID, languageDescriptions, showLexicalTag, showSources);
                    if (!String.IsNullOrEmpty(definition))
                    {
                        if (index != 1)
                            sb.Append(LanguageLookup.GetLanguageSpaces(languageID, 2));
                        sb.Append(index);
                        sb.Append(". ");
                        sb.Append(definition);
                        if (!LanguageLookup.EndsWithPeriod(definition))
                            sb.Append(LanguageLookup.GetLanguagePeriod(languageID) + "\n");
                        else
                            sb.Append("\n");
                        index++;
                    }
                }

                definition = sb.ToString();
            }

            return definition;
        }

        public string GetExportDefinition(LanguageID languageID, bool showLexicalTag, bool showSources)
        {
            if (languageID == LanguageID)
            {
                string text = KeyString;

                if (LanguageLookup.IsAlternatePhonetic(languageID))
                    ConvertPinyinNumeric.ToToneMarks(out text, text);

                return text;
            }

            string definition = GetAlternateDefinition(languageID);

            if (!String.IsNullOrEmpty(definition))
                return definition;

            int senseCount = (_Senses == null ? 0 : _Senses.Count());

            if (senseCount == 0)
                definition = String.Empty;
            else if (senseCount == 1)
                definition = _Senses[0].GetDefinition(languageID, showLexicalTag, showSources);
            else
            {
                StringBuilder sb = new StringBuilder();
                int index = 1;

                foreach (Sense sense in _Senses)
                {
                    definition = sense.GetDefinition(languageID, showLexicalTag, showSources);
                    if (!String.IsNullOrEmpty(definition))
                    {
                        if (index != 1)
                            sb.Append(LanguageLookup.GetLanguageSpaces(languageID, 1));
                        sb.Append(definition);
                        if (!LanguageLookup.EndsWithPeriod(definition))
                            sb.Append(LanguageLookup.GetLanguagePeriod(languageID));
                        index++;
                    }
                }

                definition = sb.ToString();
            }

            return definition;
        }

        public string GetCategorizedDefinition(LanguageID languageID, LexicalCategory category, bool showLexicalTag, bool showSources)
        {
            if (languageID == LanguageID)
            {
                string text = KeyString;

                if (LanguageLookup.IsAlternatePhonetic(languageID))
                    ConvertPinyinNumeric.ToToneMarks(out text, text);

                return text;
            }

            string definition = GetAlternateDefinition(languageID);

            if (!String.IsNullOrEmpty(definition))
                return definition;

            int senseCount = (_Senses == null ? 0 : _Senses.Count());

            if (senseCount == 0)
                definition = String.Empty;
            else if ((senseCount == 1) && (_Senses[0].Category == category))
                definition = _Senses[0].GetDefinition(languageID, showLexicalTag, showSources);
            else
            {
                StringBuilder sb = new StringBuilder();
                int index = 1;

                foreach (Sense sense in _Senses)
                {
                    if (!sense.HasLanguage(languageID))
                        continue;
                    definition = sense.GetDefinition(languageID, showLexicalTag, showSources);
                    if (!String.IsNullOrEmpty(definition))
                    {
                        if (index != 1)
                            sb.Append(LanguageLookup.GetLanguageSpaces(languageID, 2));
                        sb.Append(index);
                        sb.Append(LanguageLookup.GetLanguagePeriodAndSpace(languageID));
                        sb.Append(definition);
                        if (!LanguageLookup.EndsWithPeriod(definition))
                            sb.Append(LanguageLookup.GetLanguagePeriod(languageID) + "\n");
                        else
                            sb.Append("\n");
                        index++;
                    }
                }

                definition = sb.ToString();
            }

            return definition;
        }

        public bool ParseDefinition(
            LanguageID languageID,
            string str,
            char nextPatternChar,
            int startIndex,
            out int endIndex)
        {
            string value;
            bool returnValue = true;

            returnValue = TextUtilities.ParseString(
                str,
                nextPatternChar,
                startIndex,
                str.Length,
                out value,
                out endIndex);

            if (returnValue)
                returnValue = ParseDefinition(languageID, value);

            return returnValue;
        }

        public char[] SenseSeparators = { '.' };

        public bool ParseDefinition(
            LanguageID languageID,
            string value)
        {
            int alternateKey = 0;
            bool returnValue = true;

            if (languageID == LanguageID)
            {
                Key = value;
                return returnValue;
            }

            if (LanguageLookup.IsAlternateOfLanguageID(languageID, LanguageID))
            {
                LanguageString alternate = GetAlternate(languageID);
                if ((_Alternates != null) && (_Alternates.Count() != 0))
                    alternateKey = Alternates.First().KeyInt;
                if (alternate != null)
                    alternate.Text = value;
                else
                {
                    alternate = new LanguageString(alternateKey, languageID, value);
                    AddAlternate(alternate);
                }
                return returnValue;
            }

            string[] parts = value.Split(SenseSeparators, StringSplitOptions.None);
            int senseCount = parts.Length;
            int senseIndex;
            string senseString;
            Sense sense;

            if (_Senses == null)
                _Senses = new List<Sense>();

            for (senseIndex = 0; senseIndex < senseCount; senseIndex++)
            {
                senseString = parts[senseIndex].Trim();

                if (String.IsNullOrEmpty(senseString))
                    continue;

                sense = GetSenseIndexed(senseIndex);

                if (sense == null)
                {
                    sense = new Sense(
                        alternateKey,
                        LexicalCategory.Unknown,
                        null,
                        0,
                        null,
                        null);
                    AddSense(sense);
                }

                if (!sense.ParseDefinition(languageID, senseString))
                    returnValue = false;
            }

            return returnValue;
        }

        public string GetTranslation(LanguageID languageID)
        {
            if (languageID == LanguageID)
            {
                string text = KeyString;

                if (LanguageLookup.IsAlternatePhonetic(languageID))
                    ConvertPinyinNumeric.ToToneMarks(out text, text);

                return text;
            }

            string definition = GetAlternateTranslation(languageID);

            if (!String.IsNullOrEmpty(definition))
                return definition;

            int senseCount = (_Senses == null ? 0 : _Senses.Count());

            if (senseCount == 0)
                definition = String.Empty;
            else
            {
                foreach (Sense sense in _Senses)
                {
                    if (sense.HasLanguage(languageID))
                    {
                        definition = sense.GetTranslation(languageID);
                        break;
                    }
                }
            }

            return definition;
        }

        public string GetTranslationNonStem(LanguageID languageID)
        {
            if (languageID == LanguageID)
            {
                string text = KeyString;

                if (LanguageLookup.IsAlternatePhonetic(languageID))
                    ConvertPinyinNumeric.ToToneMarks(out text, text);

                return text;
            }

            Sense sense = GetSenseWithoutStem();
            int reading = 0;

            if (sense != null)
                reading = sense.Reading;

            string definition = GetAlternateTranslationWithReading(languageID, reading);

            if (!String.IsNullOrEmpty(definition))
                return definition;

            int senseCount = (_Senses == null ? 0 : _Senses.Count());
            bool isOnlyStem = HasSenseWithStemOnly();

            if (senseCount == 0)
                definition = String.Empty;
            else
            {
                foreach (Sense aSense in _Senses)
                {
                    if (((aSense.Category == LexicalCategory.Stem) || (aSense.Category == LexicalCategory.IrregularStem)) && !isOnlyStem)
                        continue;

                    if (aSense.HasLanguage(languageID))
                    {
                        definition = aSense.GetTranslation(languageID);
                        break;
                    }
                }
            }

            return definition;
        }

        public MultiLanguageString GetMultiLanguageString(
            int senseIndex,
            int synonymIndex,
            string stringKey,
            List<LanguageID> languageIDs)
        {
            MultiLanguageString multiLanguageString = new Object.MultiLanguageString(
                stringKey,
                languageIDs);

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageString languageString = multiLanguageString.LanguageString(languageID);
                string text = String.Empty;

                if (languageID == LanguageID)
                    text = KeyString;
                else if (senseIndex != -1)
                {
                    Sense sense = GetSenseIndexed(senseIndex);

                    if (sense != null)
                    {
                        int reading = sense.Reading;
                        LanguageString alternate = GetAlternate(languageID, reading);

                        if (alternate != null)
                            text = alternate.Text;
                        else
                        {
                            LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(languageID);

                            if (languageSynonyms != null)
                            {
                                if (synonymIndex != -1)
                                    text = languageSynonyms.GetSynonymIndexed(synonymIndex);
                                else
                                    text = languageSynonyms.GetDefinition(false);
                            }
                        }
                    }
                }
                else
                    text = GetDefinition(languageID, false, false);

                languageString.Text = text;
            }

            return multiLanguageString;
        }

        public bool GetMultiLanguageStringAndSense(
            int senseIndex,
            int synonymIndex,
            string stringKey,
            List<LanguageID> languageIDs,
            out ProbableMeaning outputHostMeaning,
            out Sense outputSense,
            out MultiLanguageString outMLS)
        {
            MultiLanguageString multiLanguageString = new Object.MultiLanguageString(
                stringKey,
                languageIDs);
            Sense sense = (senseIndex != -1 ? GetSenseIndexed(senseIndex) : null);
            ProbableMeaning hostMeaning = null;

            foreach (LanguageID languageID in languageIDs)
            {
                LanguageString languageString = multiLanguageString.LanguageString(languageID);
                string text = String.Empty;

                if (languageID == LanguageID)
                    text = KeyString;
                else if (sense != null)
                {
                    int reading = sense.Reading;
                    LanguageString alternate = GetAlternate(languageID, reading);

                    if (alternate != null)
                        text = alternate.Text;
                    else
                    {
                        LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(languageID);

                        if (languageSynonyms != null)
                        {
                            if (synonymIndex != -1)
                            {
                                hostMeaning = languageSynonyms.GetProbableSynonymIndexed(synonymIndex);
                                text = languageSynonyms.GetSynonymIndexed(synonymIndex);
                            }
                            else
                                text = languageSynonyms.GetDefinition(false);
                        }
                    }
                }
                else
                    text = GetDefinition(languageID, false, false);

                languageString.Text = text;
            }

            outputHostMeaning = hostMeaning;
            outputSense = sense;
            outMLS = multiLanguageString;

            return sense != null;
        }

        public void Retarget(
            MultiLanguageString input,
            MultiLanguageString output)
        {
            LanguageString lsInput = input.LanguageString(LanguageID);
            LanguageString lsOutput = output.LanguageString(LanguageID);

            if (lsInput != null)
            {
                if (KeyString == lsInput.Text)
                    Key = lsOutput.Text;
            }

            if (HasAlternates())
            {
                foreach (LanguageString alternateLS in Alternates)
                {
                    lsInput = input.LanguageString(alternateLS.LanguageID);
                    lsOutput = output.LanguageString(alternateLS.LanguageID);

                    if (alternateLS.Text == lsInput.Text)
                        alternateLS.Text = lsOutput.Text;
                }
            }

            if (SenseCount != 0)
            {
                foreach (Sense sense in Senses)
                    sense.Retarget(input, output);
            }
        }

        public void RetargetCategories(
            LexicalCategory outputCategory,
            string outputCategoryString)
        {
            if (SenseCount != 0)
            {
                foreach (Sense sense in Senses)
                {
                    sense.Category = outputCategory;
                    sense.CategoryString = outputCategoryString;
                }
            }
        }

        public List<string> GetExamples(LanguageID languageID)
        {
            List<String> examples = null;

            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                {
                    if ((sense.Examples != null) && (sense.Examples.Count() != 0))
                    {
                        foreach (MultiLanguageString example in sense.Examples)
                        {
                            string exampleText = example.Text(languageID);

                            if (!String.IsNullOrEmpty(exampleText))
                            {
                                if (examples == null)
                                    examples = new List<string>();

                                examples.Add(exampleText);
                            }
                        }
                    }
                }
            }

            return examples;
        }

        // Contains optional media runs for this entry.
        public List<MediaRun> MediaRuns
        {
            get
            {
                return _MediaRuns;
            }
            set
            {
                if (value != _MediaRuns)
                {
                    ModifiedFlag = true;
                    _MediaRuns = value;
                }
            }
        }

        public void GetDumpString(string prefix, StringBuilder sb)
        {
            sb.AppendLine("Key: " + KeyString);

            if (Alternates != null)
            {
                sb.AppendLine("Alternates:");

                foreach (LanguageString alternate in Alternates)
                {
                    sb.AppendLine(prefix + "    " + alternate.LanguageID.LanguageName(LanguageLookup.English) + ": " + alternate.Text);
                }
            }

            if (Senses != null)
            {
                sb.AppendLine("Senses:");

                foreach (Sense sense in Senses)
                    sense.GetDumpString(prefix + "   ", sb);
            }

            if (MediaRuns != null)
            {
                sb.AppendLine("MediaRuns:");

                foreach (MediaRun mediaRun in MediaRuns)
                    mediaRun.GetDumpString(prefix + "    ", sb);
            }
        }

        public override void CollectReferences(List<IBaseObjectKeyed> references, List<IBaseObjectKeyed> externalSavedChildren,
            List<IBaseObjectKeyed> externalNonSavedChildren, Dictionary<int, bool> intSelectFlags,
            Dictionary<string, bool> stringSelectFlags, Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles,
            VisitMedia visitFunction)
        {
            CollectMediaFiles(itemSelectFlags, languageSelectFlags, mediaFiles, visitFunction);
        }

        public void CollectMediaFiles(Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles, VisitMedia visitFunction)
        {
            bool useIt = true;

            if (itemSelectFlags != null)
            {
                if (!itemSelectFlags.TryGetValue(KeyString, out useIt))
                    useIt = true;
            }

            if (useIt)
            {
                LanguageID languageID = LanguageID;
                string entryKeyString = KeyString;

                CollectMediaFile(entryKeyString, languageID, languageSelectFlags, mediaFiles, visitFunction);

                if (SenseCount != 0)
                {
                    foreach (Sense sense in Senses)
                    {
                        foreach (KeyValuePair<string, bool> kvp in languageSelectFlags)
                        {
                            if (!kvp.Value)
                                continue;

                            languageID = LanguageLookup.GetLanguageIDNoAdd(kvp.Key);
                            LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(languageID);

                            if ((languageSynonyms != null) && languageSynonyms.HasProbableSynonyms())
                            {
                                int index;
                                int count = languageSynonyms.ProbableSynonymCount;
                                LanguageID synonymnLanguageID = languageSynonyms.LanguageID;

                                for (index = 0; index < count; index++)
                                {
                                    ProbableMeaning probableSynonym = languageSynonyms.GetProbableSynonymIndexed(index);
                                    string synonym = probableSynonym.Meaning;
                                    string synonymCanonical = TextUtilities.GetCanonicalText(synonym, synonymnLanguageID);

                                    CollectMediaFile(synonym, languageID, mediaFiles, visitFunction);

                                    if (!TextUtilities.IsEqualStringsIgnoreCase(synonymCanonical, synonym))
                                        CollectMediaFile(synonymCanonical, languageID, mediaFiles, visitFunction);
                                }
                            }

                            if (sense.HasExamples)
                            {
                                foreach (MultiLanguageString example in sense.Examples)
                                {
                                    if (example.HasText(languageID))
                                    {
                                        string exampleText = example.Text(languageID);
                                        string exampleTextCanonical = TextUtilities.GetCanonicalText(exampleText, languageID);

                                        CollectMediaFile(exampleText, languageID, mediaFiles, visitFunction);

                                        if (exampleTextCanonical != exampleText)
                                            CollectMediaFile(exampleTextCanonical, languageID, mediaFiles, visitFunction);
                                    }
                                }
                            }
                        }
                    }
                }

                if (AlternateCount != 0)
                {
                    foreach (LanguageString alternate in Alternates)
                    {
                        languageID = alternate.LanguageID;

                        bool collectIt = true;

                        if ((languageSelectFlags != null) &&
                                !languageSelectFlags.TryGetValue(
                                    languageID.LanguageCultureExtensionCode,
                                    out collectIt))
                            collectIt = false;

                        if (!collectIt)
                            continue;

                        string alternateText = alternate.Text;
                        string alternateDisplay = TextUtilities.GetDisplayText(
                            alternateText,
                            languageID,
                            ApplicationData.Repositories.Dictionary);
                        string alternateAlternateText;

                        CollectMediaFile(alternateText, languageID, mediaFiles, visitFunction);

                        if (!TextUtilities.IsEqualStringsIgnoreCase(alternateText, alternateDisplay))
                            alternateAlternateText = alternateDisplay;
                        else
                        {
                            string alternateCanonical = TextUtilities.GetCanonicalText(
                                alternateText,
                                languageID);
                            if (!TextUtilities.IsEqualStringsIgnoreCase(alternateText, alternateCanonical))
                                alternateAlternateText = alternateCanonical;
                            else
                                alternateAlternateText = alternateText;
                        }

                        if (!TextUtilities.IsEqualStringsIgnoreCase(alternateText, alternateAlternateText))
                            CollectMediaFile(alternateAlternateText, languageID, mediaFiles, visitFunction);
                    }
                }
            }
        }

        public void CollectMediaFile(string value, LanguageID languageID,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles, VisitMedia visitFunction)
        {
            bool collectIt = true;

            if ((languageSelectFlags != null) &&
                    !languageSelectFlags.TryGetValue(
                        languageID.LanguageCultureExtensionCode,
                        out collectIt))
                collectIt = false;

            if (collectIt)
                CollectMediaFile(value, languageID, mediaFiles, visitFunction);
        }

        public void CollectMediaFile(string value, LanguageID languageID,
            List<string> mediaFiles, VisitMedia visitFunction)
        {
            List<string> audioMimeTypes;
            List<string> imageMimeTypes;
            List<string> audioFilePaths;
            List<string> imageFilePaths;
            string filePath;
            bool audioExists = GetCheckMediaFilePathFromValue("Audio", languageID, value, out audioFilePaths, out audioMimeTypes);
            bool imageExists = GetCheckMediaFilePathFromValue("Pictures", languageID, value, out imageFilePaths, out imageMimeTypes);
            int count;
            int index;

            if (audioExists)
            {
                if (visitFunction != null)
                {
                    count = audioFilePaths.Count();
                    for (index = 0; index < count; index++)
                        visitFunction(mediaFiles, null, this, audioFilePaths[index], audioMimeTypes[index]);
                }
                else if (mediaFiles != null)
                {
                    count = audioFilePaths.Count();
                    for (index = 0; index < count; index++)
                    {
                        filePath = audioFilePaths[index];
                        if (!mediaFiles.Contains(filePath))
                            mediaFiles.Add(filePath);
                    }
                }
            }

            if (imageExists)
            {
                if (visitFunction != null)
                {
                    count = audioFilePaths.Count();
                    for (index = 0; index < count; index++)
                        visitFunction(mediaFiles, null, this, imageFilePaths[index], imageMimeTypes[index]);
                }
                else if (mediaFiles != null)
                {
                    count = imageFilePaths.Count();
                    for (index = 0; index < count; index++)
                    {
                        filePath = imageFilePaths[index];
                        if (!mediaFiles.Contains(filePath))
                            mediaFiles.Add(filePath);
                    }
                }
            }
        }

        public static string GetAudioTildeUrl(string dictionaryPath, LanguageID languageID)
        {
            dictionaryPath = dictionaryPath.Replace(ApplicationData.PlatformPathSeparator, "/");
            string returnValue = "";

            if (!dictionaryPath.StartsWith("~"))
                returnValue = GetMediaTildeUrl("Audio", languageID, dictionaryPath);
            else
                returnValue = dictionaryPath;

            if (!MediaUtilities.HasFileExtension(returnValue))
                returnValue = MediaUtilities.ChangeFileExtension(returnValue, ".mp3");

            return returnValue;
        }

        public static string GetAudioFilePath(string dictionaryPath, LanguageID languageID)
        {
            string returnValue = "";

            if (!dictionaryPath.StartsWith("~"))
                returnValue = GetMediaFilePath("Audio", languageID, dictionaryPath);
            else
            {
                dictionaryPath = dictionaryPath.Replace(ApplicationData.PlatformPathSeparator, "/");
                returnValue = ApplicationData.MapToFilePath(dictionaryPath);
            }

            if (!MediaUtilities.HasFileExtension(returnValue))
                returnValue = MediaUtilities.ChangeFileExtension(returnValue, ".mp3");

            return returnValue;
        }

        public static string GetPictureTildeUrl(string dictionaryPath, LanguageID languageID)
        {
            dictionaryPath = dictionaryPath.Replace('\\', '/');
            string returnValue = "";

            if (!dictionaryPath.StartsWith("~"))
                returnValue = GetMediaTildeUrl("Pictures", languageID, dictionaryPath);
            else
                returnValue = dictionaryPath;

            if (!MediaUtilities.HasFileExtension(returnValue))
                returnValue = MediaUtilities.ChangeFileExtension(returnValue, ".jpg");

            return returnValue;
        }

        public static string GetPictureFilePath(string dictionaryPath, LanguageID languageID)
        {
            string returnValue = "";

            if (!dictionaryPath.StartsWith("~"))
                returnValue = GetMediaFilePath("Pictures", languageID, dictionaryPath);
            else
            {
                dictionaryPath = dictionaryPath.Replace(ApplicationData.PlatformPathSeparator, "/");
                returnValue = ApplicationData.MapToFilePath(dictionaryPath);
            }

            if (!MediaUtilities.HasFileExtension(returnValue))
                returnValue = MediaUtilities.ChangeFileExtension(returnValue, ".jpg");

            return returnValue;
        }

        public static string GetMediaTildeUrl(string mediaType, LanguageID languageID, string fileName)
        {
            string returnValue = GetMediaTildeUrl(mediaType, languageID);
            returnValue = MediaUtilities.ConcatenateUrlPath(
                returnValue,
                fileName);
            return returnValue;
        }

        public static string GetMediaTildeUrl(string mediaType, LanguageID languageID)
        {
            string returnValue = MediaUtilities.ConcatenateUrlPath(
                ApplicationData.ContentTildeUrl,
                "Dictionary");
            returnValue = MediaUtilities.ConcatenateUrlPath(
                returnValue,
                mediaType);
            returnValue = MediaUtilities.ConcatenateUrlPath(
                returnValue,
                languageID.LanguageCultureExtensionCode);
            return returnValue;
        }

        public static string GetMediaDirectory(string mediaType, LanguageID languageID)
        {
            string returnValue = MediaUtilities.ConcatenateFilePath(
                ApplicationData.ContentPath,
                "Dictionary");
            returnValue = MediaUtilities.ConcatenateFilePath(
                returnValue,
                mediaType);
            returnValue = MediaUtilities.ConcatenateFilePath(
                returnValue,
                languageID.LanguageCultureExtensionCode);
            return returnValue;
        }

        public static string GetMediaFilePath(string mediaType, LanguageID languageID, string fileName)
        {
            string returnValue = GetMediaDirectory(mediaType, languageID);
            returnValue = MediaUtilities.ConcatenateFilePath(
                returnValue,
                fileName);
            return returnValue;
        }

        public static bool GetCheckMediaFilePathFromValue(
            string mediaType,
            LanguageID languageID,
            string value,
            out List<string> filePaths,
            out List<string> mimeTypes)
        {
            filePaths = null;
            mimeTypes = null;
            string fileFriendlyName = MediaUtilities.FileFriendlyName(
                value,
                 MediaUtilities.MaxMediaFileNameLength);
            string fileName;
            string filePath;
            string fileExt = String.Empty;
            string mimeType;
            string mediaDirectory = GetMediaDirectory(mediaType, languageID);
            bool exists = false;
            if (mediaType == "Audio")
            {
                AudioMultiReference audioReference = ApplicationData.Repositories.DictionaryMultiAudio.Get(value, languageID);
                if (audioReference != null)
                {
                    if (audioReference.AudioInstanceCount() != 0)
                    {
                        foreach (AudioInstance audioInstance in audioReference.AudioInstances)
                        {
                            filePath = GetAudioFilePath(audioInstance.FileName, languageID);
                            mimeType = audioInstance.MimeType;
                            if (!FileSingleton.Exists(filePath))
                                continue;
                            if (filePaths == null)
                                filePaths = new List<string>();
                            if (mimeTypes == null)
                                mimeTypes = new List<string>();
                            filePaths.Add(filePath);
                            mimeTypes.Add(mimeType);
                            exists = true;
                        }
                    }
                }
                else
                {
                    fileExt = ".mp3";
                    fileName = fileFriendlyName + fileExt;
                    filePath = MediaUtilities.ConcatenateFilePath(
                        mediaDirectory,
                        fileName);
                    mimeType = "audio/mpeg3";
                    if (FileSingleton.Exists(filePath))
                    {
                        if (filePaths == null)
                            filePaths = new List<string>();
                        if (mimeTypes == null)
                            mimeTypes = new List<string>();
                        filePaths.Add(filePath);
                        mimeTypes.Add(mimeType);
                        exists = true;
                    }
                }
            }
            else if (mediaType == "Pictures")
            {
                PictureReference pictureReference = ApplicationData.Repositories.DictionaryPictures.Get(value, languageID);
                if (pictureReference != null)
                {
                    filePath = GetPictureFilePath(pictureReference.PictureFilePath, languageID);
                    mimeType = pictureReference.PictureMimeType;
                }
                else
                {
                    fileExt = ".jpg";
                    fileName = fileFriendlyName + fileExt;
                    filePath = MediaUtilities.ConcatenateFilePath(
                        mediaDirectory,
                        fileName);
                    mimeType = "image/jpeg";
                    if (FileSingleton.Exists(filePath))
                    {
                        if (filePaths == null)
                            filePaths = new List<string>();
                        if (mimeTypes == null)
                            mimeTypes = new List<string>();
                        filePaths.Add(filePath);
                        mimeTypes.Add(mimeType);
                        exists = true;
                    }
                }
            }
            return exists;
        }

        public bool MediaCheck(List<LanguageID> languageIDs, ref string errorMessage)
        {
            bool returnValue = true;

            if (languageIDs.Contains(LanguageID))
                returnValue = CheckMediaFile(KeyString, LanguageID, ref errorMessage) && returnValue;

            if (SenseCount != 0)
            {
                foreach (Sense sense in Senses)
                {
                    foreach (LanguageID languageID in languageIDs)
                    {
                        LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(languageID);

                        if ((languageSynonyms != null) && languageSynonyms.HasProbableSynonyms())
                        {
                            int index;
                            int count = languageSynonyms.ProbableSynonymCount;
                            LanguageID synonymnLanguageID = languageSynonyms.LanguageID;

                            for (index = 0; index < count; index++)
                            {
                                string synonym = languageSynonyms.GetSynonymIndexed(index);
                                string synonymCanonical = TextUtilities.GetCanonicalText(synonym, synonymnLanguageID);

                                returnValue = CheckMediaFile(synonym, languageID, ref errorMessage) && returnValue;

                                if (!TextUtilities.IsEqualStringsIgnoreCase(synonymCanonical, synonym))
                                    returnValue = CheckMediaFile(synonymCanonical, languageID, ref errorMessage) && returnValue;
                            }
                        }

                        if (sense.HasExamples)
                        {
                            foreach (MultiLanguageString example in sense.Examples)
                            {
                                if (example.HasText(languageID))
                                {
                                    string exampleText = example.Text(languageID);
                                    string exampleTextCanonical = TextUtilities.GetCanonicalText(exampleText, languageID);

                                    returnValue = CheckMediaFile(exampleText, languageID, ref errorMessage) && returnValue;

                                    if (!!TextUtilities.IsEqualStringsIgnoreCase(exampleTextCanonical, exampleText))
                                        returnValue = CheckMediaFile(exampleTextCanonical, languageID, ref errorMessage) && returnValue;
                                }
                            }
                        }
                    }
                }
            }

            if (AlternateCount != 0)
            {
                foreach (LanguageString alternate in Alternates)
                {
                    LanguageID languageID = alternate.LanguageID;

                    if (!languageIDs.Contains(languageID))
                        continue;

                    string alternateText = alternate.Text;
                    string alternateDisplay = TextUtilities.GetDisplayText(
                        alternateText,
                        languageID,
                        ApplicationData.Repositories.Dictionary);
                    string alternateAlternateText;

                    returnValue = CheckMediaFile(alternateText, languageID, ref errorMessage) && returnValue;

                    if (!!TextUtilities.IsEqualStringsIgnoreCase(alternateText, alternateDisplay))
                        alternateAlternateText = alternateDisplay;
                    else
                    {
                        string alternateCanonical = TextUtilities.GetCanonicalText(
                            alternateText,
                            languageID);
                        if (!!TextUtilities.IsEqualStringsIgnoreCase(alternateText, alternateCanonical))
                            alternateAlternateText = alternateCanonical;
                        else
                            alternateAlternateText = alternateText;
                    }

                    if (!!TextUtilities.IsEqualStringsIgnoreCase(alternateText, alternateAlternateText))
                        returnValue = CheckMediaFile(alternateAlternateText, languageID, ref errorMessage) && returnValue;
                }
            }

            return returnValue;
        }

        public bool CheckMediaFile(string value, LanguageID languageID, ref string errorMessage)
        {
            bool returnValue = CheckMediaFile(value, languageID, "Audio", ref errorMessage);
            returnValue = CheckMediaFile(value, languageID, "Pictures", ref errorMessage) && returnValue;
            return returnValue;
        }

        public bool CheckMediaFile(string value, LanguageID languageID, string mediaType, ref string errorMessage)
        {
            string fileFriendlyName = MediaUtilities.FileFriendlyName(
                value,
                 MediaUtilities.MaxMediaFileNameLength);
            string filePath;
            string fileName;
            string fileExt = String.Empty;
            string mediaDirectory = GetMediaDirectory(mediaType, languageID);
            bool returnValue = false;

            if (mediaType == "Audio")
            {
                AudioMultiReference audioReference = ApplicationData.Repositories.DictionaryMultiAudio.Get(value, languageID);
                if (audioReference != null)
                {
                    if (audioReference.AudioInstanceCount() != 0)
                    {
                        foreach (AudioInstance audioInstance in audioReference.AudioInstances)
                        {
                            filePath = GetAudioFilePath(audioInstance.FileName, languageID);
                            if (FileSingleton.Exists(filePath))
                                returnValue = true;
                        }
                    }
                }
                else
                {
                    fileExt = ".mp3";
                    fileName = fileFriendlyName + fileExt;
                    filePath = MediaUtilities.ConcatenateFilePath(
                        mediaDirectory,
                        fileName);
                    if (FileSingleton.Exists(filePath))
                    {
                        AudioInstance audioInstance = new AudioInstance(
                            value,
                            ApplicationData.ApplicationName,
                            "audio/mpeg3",
                            fileName,
                            AudioInstance.UploadedSourceName,
                            null);
                        List<AudioInstance> audioInstances = new List<AudioInstance>(1) { audioInstance };
                            audioReference = new AudioMultiReference(
                                value,
                                languageID,
                                audioInstances);
                        try
                        {
                            if (ApplicationData.Repositories.DictionaryMultiAudio.Add(audioReference, languageID))
                                returnValue = true;
                            else
                            {
                                errorMessage += "Error adding audio reference: " + value;
                                returnValue = false;
                            }
                        }
                        catch (Exception exc)
                        {
                            errorMessage += "Exception while adding audio reference: " +
                                value +
                                "\n    " +
                                exc.Message;
                            if (exc.InnerException != null)
                                errorMessage += ": " + exc.InnerException.Message;
                            returnValue = false;
                        }
                    }
                }
            }
            else if (mediaType == "Pictures")
            {
                PictureReference pictureReference = ApplicationData.Repositories.DictionaryPictures.Get(value, languageID);
                if (pictureReference != null)
                    filePath = GetPictureFilePath(pictureReference.PictureFilePath, languageID);
                else
                {
                    fileExt = ".jpg";
                    fileName = fileFriendlyName + fileExt;
                    filePath = MediaUtilities.ConcatenateFilePath(
                        mediaDirectory,
                        fileName);
                }
                if (FileSingleton.Exists(filePath))
                    return true;
                if (FileSingleton.Exists(filePath))
                {
                    if (pictureReference == null)
                    {
                        pictureReference = new PictureReference(
                            value,
                            value,
                            ApplicationData.ApplicationName,
                            "picture/mpeg3",
                            fileFriendlyName);
                        try
                        {
                            if (!ApplicationData.Repositories.DictionaryPictures.Add(pictureReference, languageID))
                            {
                                errorMessage += "Error adding picture reference: " + value;
                                returnValue = false;
                            }
                        }
                        catch (Exception exc)
                        {
                            errorMessage += "Exception while adding picture reference: " +
                                value +
                                "\n    " +
                                exc.Message;
                            if (exc.InnerException != null)
                                errorMessage += ": " + exc.InnerException.Message;
                            returnValue = false;
                        }
                    }
                }
                else
                {
                    if (pictureReference != null)
                    {
                        try
                        {
                            if (!ApplicationData.Repositories.DictionaryPictures.DeleteKey(value, languageID))
                            {
                                errorMessage += "Error deleting picture reference: " + value;
                                returnValue = false;
                            }
                        }
                        catch (Exception exc)
                        {
                            errorMessage += "Exception while deleting picture reference: " +
                                value +
                                "\n    " +
                                exc.Message;
                            if (exc.InnerException != null)
                                errorMessage += ": " + exc.InnerException.Message;
                            returnValue = false;
                        }
                    }
                }
            }
            else
            {
                returnValue = false;
                errorMessage += "Unknown media type: " + mediaType + "\n";
            }

            return returnValue;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (!String.IsNullOrEmpty(_IPAReading))
                element.Add(new XElement("IPAReading", _IPAReading));
            if (_Frequency != 0)
                element.Add(new XElement("Frequency", _Frequency));
            if (_Alternates != null)
            {
                foreach (LanguageString alternate in _Alternates)
                    element.Add(alternate.GetElement("Alternate"));
            }
            if ((_SourceIDs != null) && (_SourceIDs.Count() != 0))
                element.Add(new XElement("SourceIDs", ObjectUtilities.GetStringFromIntList(_SourceIDs)));
            if (_Senses != null)
            {
                foreach (Sense sense in _Senses)
                    element.Add(sense.GetElement("Sense"));
            }
            if (_MediaRuns != null)
            {
                foreach (MediaRun mediaRun in _MediaRuns)
                    element.Add(mediaRun.GetElement("MediaRun"));
            }
            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "IPAReading":
                    _IPAReading = childElement.Value.Trim();
                    break;
                case "Frequency":
                    _Frequency = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), 0);
                    break;
                case "Alternate":
                    if (_Alternates == null)
                        _Alternates = new List<LanguageString>();
                    _Alternates.Add(new LanguageString(childElement));
                    break;
                case "SourceIDs":
                    _SourceIDs = ObjectUtilities.GetIntListFromString(childElement.Value.Trim());
                    break;
                case "Sense":
                    if (_Senses == null)
                        _Senses = new List<Sense>();
                    _Senses.Add(new Sense(childElement));
                    break;
                case "MediaRun":
                    if (_MediaRuns == null)
                        _MediaRuns = new List<MediaRun>();
                    _MediaRuns.Add(new MediaRun(childElement));
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public int CompareWhich(DictionaryEntry other, string which)
        {
            DictionaryEntry otherDictionaryEntry = other as DictionaryEntry;
            int diff, count;

            if (otherDictionaryEntry == null)
                return base.Compare(other);

            switch (which)
            {
                case "Key":
                    return ObjectUtilities.CompareKeys(this, other);
                case "LanguageID":
                    {
                        IBaseObjectLanguage otherString = other as IBaseObjectLanguage;
                        if (otherString == null)
                            return 1;
                        return LanguageID.Compare(LanguageID, otherString.LanguageID);
                    }
                case "LanguageCode":
                    {
                        IBaseObjectLanguage otherString = other as IBaseObjectLanguage;
                        if (otherString == null)
                            return 1;
                        return String.Compare(LanguageID.LanguageCultureExtensionCode, otherString.LanguageID.LanguageCultureExtensionCode);
                    }
                case "IPAReading":
                    return String.Compare(_IPAReading, otherDictionaryEntry.IPAReading);
                case "Frequency":
                    return _Frequency - otherDictionaryEntry.Frequency;
                case "Alternates":
                    if ((this.Alternates == null) && (otherDictionaryEntry.Alternates == null))
                        return 0;
                    if (this.Alternates == null)
                        return -1;
                    if (otherDictionaryEntry.Alternates == null)
                        return 1;
                    if (this.Alternates.Count() != otherDictionaryEntry.Alternates.Count())
                        return this.Alternates.Count() - otherDictionaryEntry.Alternates.Count();
                    count = this.Alternates.Count();
                    for (int i = 0; i < count; i++)
                    {
                        diff = LanguageString.Compare(this.Alternates[i], otherDictionaryEntry.Alternates[i]);
                        if (diff != 0)
                            return diff;
                    }
                    return 0;
                case "Senses":
                    if ((this.Senses == null) && (otherDictionaryEntry.Senses == null))
                        return 0;
                    if (this.Senses == null)
                        return -1;
                    if (otherDictionaryEntry.Senses == null)
                        return 1;
                    if (this.Senses.Count() != otherDictionaryEntry.Senses.Count())
                        return this.Senses.Count() - otherDictionaryEntry.Senses.Count();
                    count = this.Senses.Count();
                    for (int i = 0; i < count; i++)
                    {
                        diff = this.Senses[i].Compare(otherDictionaryEntry.Senses[i]);
                        if (diff != 0)
                            return diff;
                    }
                    return 0;
                case "MediaRuns":
                    if ((this.MediaRuns == null) && (otherDictionaryEntry.MediaRuns == null))
                        return 0;
                    if (this.MediaRuns == null)
                        return -1;
                    if (otherDictionaryEntry.MediaRuns == null)
                        return 1;
                    if (this.MediaRuns.Count() != otherDictionaryEntry.MediaRuns.Count())
                        return this.MediaRuns.Count() - otherDictionaryEntry.MediaRuns.Count();
                    count = this.MediaRuns.Count();
                    for (int i = 0; i < count; i++)
                    {
                        diff = MediaRun.Compare(this.MediaRuns[i], otherDictionaryEntry.MediaRuns[i]);
                        if (diff != 0)
                            return diff;
                    }
                    return 0;
                case null:
                case "":
                case "All":
                default:
                    {
                        diff = Compare(other);
                        if (diff != 0)
                            return diff;
                        return 0;
                    }
            }
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            DictionaryEntry otherDictionaryEntry = other as DictionaryEntry;
            int diff, count;

            if (otherDictionaryEntry == null)
                return base.Compare(other);

            if ((diff = String.Compare(_IPAReading, otherDictionaryEntry.IPAReading)) != 0)
                return diff;

            if ((this.Alternates != null) || (otherDictionaryEntry.Alternates != null))
            {
                if (this.Alternates == null)
                    return -1;
                if (otherDictionaryEntry.Alternates == null)
                    return 1;
                if (this.Alternates.Count() != otherDictionaryEntry.Alternates.Count())
                    return this.Alternates.Count() - otherDictionaryEntry.Alternates.Count();
                count = this.Alternates.Count();
                for (int i = 0; i < count; i++)
                {
                    diff = JTLanguageModelsPortable.Object.LanguageString.Compare(this.Alternates[i], otherDictionaryEntry.Alternates[i]);
                    if (diff != 0)
                        return diff;
                }
            }

            if ((this.Senses != null) || (otherDictionaryEntry.Senses != null))
            {
                if (this.Senses == null)
                    return -1;
                if (otherDictionaryEntry.Senses == null)
                    return 1;
                if (this.Senses.Count() != otherDictionaryEntry.Senses.Count())
                    return this.Senses.Count() - otherDictionaryEntry.Senses.Count();
                count = this.Senses.Count();
                for (int i = 0; i < count; i++)
                {
                    diff = this.Senses[i].Compare(otherDictionaryEntry.Senses[i]);
                    if (diff != 0)
                        return diff;
                }
            }

            if ((this.MediaRuns != null) || (otherDictionaryEntry.MediaRuns != null))
            {
                if (this.MediaRuns == null)
                    return -1;
                if (otherDictionaryEntry.MediaRuns == null)
                    return 1;
                if (this.MediaRuns.Count() != otherDictionaryEntry.MediaRuns.Count())
                    return this.MediaRuns.Count() - otherDictionaryEntry.MediaRuns.Count();
                count = this.MediaRuns.Count();
                for (int i = 0; i < count; i++)
                {
                    diff = MediaRun.Compare(this.MediaRuns[i], otherDictionaryEntry.MediaRuns[i]);
                    if (diff != 0)
                        return diff;
                }
            }

            diff = base.Compare(other);
            if (diff != 0)
                return diff;

            return 0;
        }

        public static int Compare(DictionaryEntry object1, DictionaryEntry object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareKeys(DictionaryEntry object1, DictionaryEntry object2)
        {
            return ObjectUtilities.CompareKeys(object1, object2);
        }

        public static Dictionary<LanguageID, List<DictionaryEntry>> SplitList(List<DictionaryEntry> dictionaryEntries)
        {
            Dictionary<LanguageID, List<DictionaryEntry>> dictionary = new Dictionary<LanguageID, List<DictionaryEntry>>();

            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
            {
                LanguageID languageID = dictionaryEntry.LanguageID;
                List<DictionaryEntry> languageEntries;

                if (dictionary.TryGetValue(languageID, out languageEntries))
                    languageEntries.Add(dictionaryEntry);
                else
                {
                    languageEntries = new List<DictionaryEntry>() { dictionaryEntry };
                    dictionary.Add(languageID, languageEntries);
                }
            }

            return dictionary;
        }

        // Merge dictionary entries if feasible. Replace in dictionaryEntries list.
        // Returns new list if any merged.
        public static List<DictionaryEntry> MergeDictionaryEntries(List<DictionaryEntry> dictionaryEntries)
        {
            if ((dictionaryEntries == null) || (dictionaryEntries.Count() < 2))
                return dictionaryEntries;

            List<DictionaryEntry> newDictionaryEntries = new List<DictionaryEntry>(dictionaryEntries);
            int entryCount = dictionaryEntries.Count();
            int entryIndex1;
            int entryIndex2;

            for (entryIndex1 = 0; entryIndex1 < entryCount; entryIndex1++)
            {
                DictionaryEntry oldEntry1 = newDictionaryEntries[entryIndex1];

                for (entryIndex2 = entryIndex1 + 1; entryIndex2 < entryCount; entryIndex2++)
                {
                    DictionaryEntry oldEntry2 = newDictionaryEntries[entryIndex2];

                    if (String.Compare(oldEntry2.KeyString, oldEntry1.KeyString, StringComparison.OrdinalIgnoreCase) != 0)
                        continue;

                    DictionaryEntry newEntry = new DictionaryEntry(oldEntry1);

                    newEntry.MergeEntry(oldEntry2);

                    newDictionaryEntries.RemoveAt(entryIndex2);
                    entryCount--;

                    newDictionaryEntries[entryIndex1] = newEntry;
                }
            }

            return newDictionaryEntries;
        }

        public static void Merge(
            DictionaryEntry de1,
            DictionaryEntry de2)
        {
            de1.MergeEntry(de2);
            de2.MergeEntry(de1);
        }
    }
}
