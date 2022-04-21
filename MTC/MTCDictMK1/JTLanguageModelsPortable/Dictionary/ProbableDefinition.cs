using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Dictionary
{
    public class ProbableDefinition
    {
        public string TargetMeaning { get; set; }
        public int Frequency { get; set; }
        public List<int> SourceIDs { get; set; }
        public List<ProbableMeaning> HostMeanings { get; set; }

        public ProbableDefinition(
            string targetMeaning,
            int frequency,
            List<int> sourceIDs,
            List<ProbableMeaning> hostMeanings)
        {
            TargetMeaning = targetMeaning;
            Frequency = frequency;

            if (sourceIDs != null)
                SourceIDs = new List<int>(sourceIDs);
            else
                SourceIDs = new List<int>();

            HostMeanings = hostMeanings;
        }

        public ProbableDefinition(
            string targetMeaning,
            int frequency,
            List<int> sourceIDs,
            ProbableMeaning hostMeaning)
        {
            TargetMeaning = targetMeaning;
            Frequency = frequency;

            if (sourceIDs != null)
                SourceIDs = new List<int>(sourceIDs);
            else
                SourceIDs = new List<int>();

            if (hostMeaning != null)
                HostMeanings = new List<ProbableMeaning>() { hostMeaning };
            else
                HostMeanings = null;
        }

        public ProbableDefinition(
            string targetMeaning,
            int frequency,
            int sourceID,
            List<ProbableMeaning> hostMeanings)
        {
            TargetMeaning = targetMeaning;
            Frequency = frequency;

            if (sourceID != 0)
                SourceIDs = new List<int>() { sourceID };
            else
                SourceIDs = new List<int>();

            HostMeanings = hostMeanings;
        }

        public ProbableDefinition(
            string targetMeaning,
            int frequency,
            int sourceID,
            ProbableMeaning hostMeaning)
        {
            TargetMeaning = targetMeaning;
            Frequency = frequency;

            if (sourceID != 0)
                SourceIDs = new List<int>() { sourceID };
            else
                SourceIDs = new List<int>();

            if (hostMeaning != null)
                HostMeanings = new List<ProbableMeaning>() { hostMeaning };
            else
                HostMeanings = null;
        }

        public ProbableDefinition(
            DictionaryEntry dictionaryEntry,
            LanguageID hostLanguageID)
        {
            ClearProbableDefinition();
            if (dictionaryEntry != null)
                LoadFromDictionaryEntry(dictionaryEntry, hostLanguageID);
        }

        public ProbableDefinition(
            List<DictionaryEntry> dictionaryEntries,
            LanguageID hostLanguageID)
        {
            ClearProbableDefinition();
            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
                LoadFromDictionaryEntry(dictionaryEntry, hostLanguageID);
        }

        public ProbableDefinition(ProbableDefinition other)
        {
            CopyProbableDefinition(other);
        }

        public ProbableDefinition(XElement element)
        {
            OnElement(element);
        }

        public ProbableDefinition()
        {
            ClearProbableDefinition();
        }

        public void ClearProbableDefinition()
        {
            TargetMeaning = String.Empty;
            Frequency = 0;
            SourceIDs = new List<int>();
            HostMeanings = null;
        }

        public void CopyProbableDefinition(ProbableDefinition other)
        {
            TargetMeaning = other.TargetMeaning;
            Frequency = other.Frequency;

            if (other.HostMeanings != null)
            {
                HostMeanings = new List<ProbableMeaning>();

                foreach (ProbableMeaning hostMeaning in other.HostMeanings)
                    HostMeanings.Add(new ProbableMeaning(hostMeaning));
            }
            else
                HostMeanings = null;
        }

        public void Merge(ProbableDefinition other)
        {
            if (!TextUtilities.IsEqualStringsIgnoreCase(TargetMeaning, other.TargetMeaning))
                throw new Exception("In ProbableDefinition.Merge, the TargetMeaning's don't match: "
                    + TargetMeaning + " and " + other.TargetMeaning);

            MergeFrequency(other.Frequency);
            MergeSourceIDs(other.SourceIDs);
            MergeHostMeanings(other.HostMeanings);
        }

        public void MergeFrequency(int frequency)
        {
            if (frequency > Frequency)
                Frequency = frequency;
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

        public void MergeHostMeanings(List<ProbableMeaning> hostMeanings)
        {
            if ((hostMeanings == null) || (hostMeanings == HostMeanings) || (hostMeanings.Count() == 0))
                return;

            if (HostMeanings == null)
                HostMeanings = new List<ProbableMeaning>();

            foreach (ProbableMeaning hostMeaning in hostMeanings)
            {
                ProbableMeaning oldMeaning = FindHostMeaning(hostMeaning.Meaning);

                if (oldMeaning == null)
                    HostMeanings.Add(new ProbableMeaning(hostMeaning));
                else
                    oldMeaning.Merge(hostMeaning);
            }
        }

        public int TargetSourceIDCount()
        {
            if (SourceIDs != null)
                return SourceIDs.Count();
            return 0;
        }

        public bool HasSourceIDs()
        {
            if ((SourceIDs != null) && (SourceIDs.Count() != 0))
                return true;
            return false;
        }

        public string GetTargetSourceNames()
        {
            return ApplicationData.DictionarySourcesLazy.GetByIDList(SourceIDs);
        }

        public bool HasHostMeaning(string hostPhrase)
        {
            if (HostMeanings == null)
                return false;

            return HostMeanings.FirstOrDefault(x => TextUtilities.IsEqualStringsIgnoreCase(hostPhrase, x.Meaning)) != null;
        }

        public ProbableMeaning FindHostMeaning(string hostPhrase)
        {
            if (HostMeanings == null)
                return null;

            return HostMeanings.FirstOrDefault(x => TextUtilities.IsEqualStringsIgnoreCase(hostPhrase, x.Meaning));
        }

        public ProbableMeaning FindCompatibleHostMeaning(ProbableMeaning hostMeaning)
        {
            if (HostMeanings == null)
                return null;

            return HostMeanings.FirstOrDefault(x => x.IsCompatible(hostMeaning));
        }

        public ProbableMeaning GetHostMeaningIndexed(int index)
        {
            if ((index < 0) || (index > HostMeaningCount()))
                return null;

            return HostMeanings[index];
        }

        public int GetHostMeaningIndex(ProbableMeaning hostMeaning)
        {
            if (HostMeanings == null)
                return -1;

            return HostMeanings.IndexOf(hostMeaning);
        }

        public void AddHostMeaning(ProbableMeaning hostMeaning)
        {
            if (HostMeanings == null)
                HostMeanings = new List<ProbableMeaning>() { hostMeaning };
            else
                HostMeanings.Add(hostMeaning);
        }

        public void AddHostMeaningSorted(
            ProbableMeaning hostMeaning,
            ProbableMeaningComparer comparer)
        {
            if (HostMeanings == null)
                HostMeanings = new List<ProbableMeaning>() { hostMeaning };
            else
            {
                int count = HostMeanings.Count();
                int index;

                for (index = count - 1; index >= 0; index--)
                {
                    if (comparer.Compare(hostMeaning, HostMeanings[index]) >= 0)
                    {
                        HostMeanings.Insert(index + 1, hostMeaning);
                        return;
                    }
                }

                HostMeanings.Insert(0, hostMeaning);
            }
        }

        public int HostMeaningCount()
        {
            if (HostMeanings != null)
                return HostMeanings.Count();

            return 0;
        }

        public void SetFrequencyProbability()
        {
            if (HostMeanings == null)
                return;

            int frequencySum = 0;

            foreach (ProbableMeaning hostMeaning in HostMeanings)
                frequencySum += hostMeaning.Frequency;

            if (frequencySum == 0)
                return;

            foreach (ProbableMeaning hostMeaning in HostMeanings)
            {
                if (hostMeaning.Frequency != 0)
                    hostMeaning.Probability = (float)hostMeaning.Frequency / frequencySum;
            }
        }

        public void SortHostMeanings()
        {
            if (HostMeanings == null)
                return;
            if (HostMeanings.Count() <= 1)
                return;
            ProbableMeaningComparer comparer = new ProbableMeaningComparer();
            HostMeanings.Sort(comparer);
        }

        public void SortHostMeaningsByProbability()
        {
            if (HostMeanings == null)
                return;
            if (HostMeanings.Count() <= 1)
                return;
            ProbableMeaningComparer comparer = new ProbableMeaningComparer(ProbableMeaningComparer.DescendingProbabilitySortOrder);
            HostMeanings.Sort(comparer);
        }

        public void GetDumpString(string prefix, StringBuilder sb)
        {
            sb.AppendLine(prefix + ToString());
        }

        public override string ToString()
        {
            string str = "TargetMeaning: " + TargetMeaning + ", "
                + Frequency.ToString() + ", "
                + SourceIDsString + ", "
                + "HostMeanings: ";

            if ((HostMeanings != null) && (HostMeanings.Count() != 0))
            {
                str += "/";

                foreach (ProbableMeaning hostMeaning in HostMeanings)
                    str += hostMeaning.ToString() + "/";
            }

            return str;
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

        public void LoadFromDictionaryEntry(
            DictionaryEntry dictionaryEntry,
            LanguageID hostLanguageID)
        {
            TargetMeaning = dictionaryEntry.KeyString;
            Frequency = dictionaryEntry.Frequency;

            if (dictionaryEntry.SourceIDCount() != 0)
                MergeSourceIDs(dictionaryEntry.SourceIDs);

            if (dictionaryEntry.SenseCount == 0)
                return;

            foreach (Sense sense in dictionaryEntry.Senses)
            {
                if (sense.LanguageSynonymsCount == 0)
                    continue;

                foreach (LanguageSynonyms languageSynonyms in sense.LanguageSynonyms)
                {
                    if (languageSynonyms.LanguageID != hostLanguageID)
                        continue;

                    if (languageSynonyms.ProbableSynonymCount == 0)
                        continue;

                    foreach (ProbableMeaning probableSynonym in languageSynonyms.ProbableSynonyms)
                    {
                        if (String.IsNullOrEmpty(probableSynonym.Meaning))
                            continue;

                        ProbableMeaning oldProbableSynonym = FindCompatibleHostMeaning(probableSynonym);

                        if (oldProbableSynonym != null)
                            oldProbableSynonym.Merge(probableSynonym);
                        else
                            AddHostMeaning(new ProbableMeaning(probableSynonym));
                    }
                }
            }
        }

        public virtual XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            element.Add(new XElement("TargetMeaning", TargetMeaning));

            if (Frequency != 0)
                element.Add(new XElement("Frequency", Frequency));

            if ((SourceIDs != null) && (SourceIDs.Count() != 0))
                element.Add(new XElement("SourceIDs", ObjectUtilities.GetStringFromIntList(SourceIDs)));

            if (HostMeanings != null)
            {
                foreach (ProbableMeaning hostMeaning in HostMeanings)
                    element.Add(hostMeaning.GetElement("HostMeaning"));
            }

            return element;
        }

        public bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "TargetMeaning":
                    TargetMeaning = childElement.Value.Trim();
                    break;
                case "Frequency":
                    Frequency = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), 0);
                    break;
                case "SourceIDs":
                    SourceIDs = ObjectUtilities.GetIntListFromString(childElement.Value.Trim());
                    break;
                case "HostMeaning":
                    if (HostMeanings == null)
                        HostMeanings = new List<ProbableMeaning>();
                    HostMeanings.Add(new ProbableMeaning(childElement));
                    break;
                default:
                    return false;
            }

            return true;
        }

        public void OnElement(XElement element)
        {
            ClearProbableDefinition();

            foreach (var childNode in element.Elements())
            {
                XElement childElement = childNode as XElement;

                if (childElement == null)
                    continue;

                if (!OnChildElement(childElement))
                    throw new ObjectException("Unknown child element " + childElement.Name + " in " + element.Name.ToString() + ".");
            }
        }

        public bool Match(ProbableDefinition other)
        {
            if (other.TargetMeaning != TargetMeaning)
                return false;

            if (Frequency != other.Frequency)
                return false;

            if (ObjectUtilities.CompareIntLists(SourceIDs, other.SourceIDs) != 0)
                return false;

            if (other.HostMeaningCount() != HostMeaningCount())
                return false;

            if ((HostMeanings != null) && (other.HostMeanings != null))
            {
                int count = HostMeaningCount();
                int index;

                for (index = 0; index < count; index++)
                {
                    if (!HostMeanings[index].Match(other.HostMeanings[index]))
                        return false;
                }
            }

            return true;
        }

        public int Compare(ProbableDefinition other)
        {
            int diff = String.Compare(TargetMeaning, other.TargetMeaning);

            if (diff != 0)
                return diff;

            if (Frequency != other.Frequency)
            {
                if (Frequency < other.Frequency)
                    return 1;
                else
                    return -1;
            }

            diff = ObjectUtilities.CompareIntLists(SourceIDs, other.SourceIDs);

            if (diff != 0)
                return diff;

            int count1 = HostMeaningCount();
            int count2 = other.HostMeaningCount();
            int count = (count1 > count2 ? count1 : count2);
            int index;

            if ((HostMeanings != null) && (other.HostMeanings != null))
            {
                for (index = 0; index < count; index++)
                {
                    if (index >= count1)
                        return -1;
                    else if (index >= count2)
                        return 1;

                    diff = HostMeanings[index].Compare(other.HostMeanings[index]);

                    if (diff != 0)
                        return diff;
                }
            }

            return 0;
        }
    }
}
