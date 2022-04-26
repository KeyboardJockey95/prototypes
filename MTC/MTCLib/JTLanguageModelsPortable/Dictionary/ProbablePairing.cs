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
    public class ProbablePairing
    {
        public string TargetMeaning { get; set; }
        public string HostMeaning { get; set; }
        public float Probability { get; set; }
        public int Frequency { get; set; }      // 1.0 (most probable) - 0.0 (least probable)
        public List<int> SourceIDs { get; set; }

        public ProbablePairing(
            string targetMeaning,
            string hostMeaning,
            float probability,
            int frequency,
            List<int> sourceIDs)
        {
            TargetMeaning = targetMeaning;
            HostMeaning = hostMeaning;
            Probability = probability;
            Frequency = frequency;

            if (sourceIDs != null)
                SourceIDs = new List<int>(sourceIDs);
            else
                SourceIDs = new List<int>();
        }

        public ProbablePairing(
            string targetMeaning,
            string hostMeaning,
            float probability,
            int frequency,
            int sourceID)
        {
            TargetMeaning = targetMeaning;
            HostMeaning = hostMeaning;
            Probability = probability;
            Frequency = frequency;

            if (sourceID != 0)
                SourceIDs = new List<int>() { sourceID };
            else
                SourceIDs = new List<int>();
        }

        public ProbablePairing(ProbablePairing other)
        {
            Copy(other);
        }

        public ProbablePairing(XElement element)
        {
            OnElement(element);
        }

        public ProbablePairing()
        {
            Clear();
        }

        public void Clear()
        {
            TargetMeaning = String.Empty;
            HostMeaning = String.Empty;
            Probability = float.NaN;
            Frequency = 0;
            SourceIDs = new List<int>();
        }

        public void Copy(ProbablePairing other)
        {
            TargetMeaning = other.TargetMeaning;
            HostMeaning = other.HostMeaning;
            Probability = other.Probability;
            Frequency = other.Frequency;
            SourceIDs = new List<int>(other.SourceIDs);
        }

        public void Merge(ProbablePairing other)
        {
            if (other.Probability > Probability)
                Probability = other.Probability;

            if (other.Frequency > Frequency)
                Frequency = other.Frequency;

            MergeSourceIDs(other.SourceIDs);
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

        public bool HasSourceIDs()
        {
            if ((SourceIDs != null) && (SourceIDs.Count() != 0))
                return true;
            return false;
        }

        public string GetSourceNames()
        {
            return ApplicationData.DictionarySourcesLazy.GetByIDList(SourceIDs);
        }

        public void GetDumpString(string prefix, StringBuilder sb)
        {
            sb.AppendLine(prefix + ToString());
        }

        public override string ToString()
        {
            return "TargetMeaning: " + TargetMeaning + ", "
                + "HostMeaning: " + HostMeaning + ", "
                + Probability.ToString() + ", "
                + Frequency.ToString() + ", "
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
            element.Add(new XElement("TargetMeaning", TargetMeaning));
            element.Add(new XElement("HostMeaning", HostMeaning));
            if (!float.IsNaN(Probability))
                element.Add(new XElement("Probability", Probability));
            if (Frequency != 0)
                element.Add(new XElement("Frequency", Frequency));
            if ((SourceIDs != null) && (SourceIDs.Count() != 0))
                element.Add(new XElement("SourceIDs", ObjectUtilities.GetStringFromIntList(SourceIDs)));
            return element;
        }

        public bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "TargetMeaning":
                    TargetMeaning = childElement.Value.Trim();
                    break;
                case "HostMeaning":
                    HostMeaning = childElement.Value.Trim();
                    break;
                case "Probability":
                    Probability = ObjectUtilities.GetFloatFromString(childElement.Value.Trim(), float.NaN);
                    break;
                case "Frequency":
                    Frequency = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), 0);
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
            Clear();

            foreach (var childNode in element.Elements())
            {
                XElement childElement = childNode as XElement;

                if (childElement == null)
                    continue;

                if (!OnChildElement(childElement))
                    throw new ObjectException("Unknown child element " + childElement.Name + " in " + element.Name.ToString() + ".");
            }
        }

        public bool Match(ProbablePairing other)
        {
            if (other.TargetMeaning != TargetMeaning)
                return false;

            if (other.HostMeaning != HostMeaning)
                return false;

            if (Frequency != other.Frequency)
                return false;

            if (Probability != other.Probability)
                return false;

            if (ObjectUtilities.CompareIntLists(SourceIDs, other.SourceIDs) != 0)
                return false;

            return true;
        }

        public int Compare(ProbablePairing other)
        {
            int diff = String.Compare(TargetMeaning, other.TargetMeaning);
            if (diff != 0)
                return diff;
            diff = String.Compare(HostMeaning, other.HostMeaning);
            if (diff != 0)
                return diff;
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
