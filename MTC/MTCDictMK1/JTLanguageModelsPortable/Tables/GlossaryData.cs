using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Tables
{
    public class GlossaryData
    {
        public LanguageID TargetLanguageID;
        public LanguageID HostLanguageID;
        public Dictionary<string, DictionaryEntry> EntryDictionary;
        public List<DictionaryEntry> EntryList;
        public DataTable PredictionTable;

        public GlossaryData(
            LanguageID targetLanguageID,
            LanguageID hostLanguageID)
        {
            TargetLanguageID = targetLanguageID;
            HostLanguageID = hostLanguageID;
            EntryDictionary = new Dictionary<string, DictionaryEntry>(StringComparer.OrdinalIgnoreCase);
            EntryList = new List<DictionaryEntry>();
            PredictionTable = null;
        }

        public GlossaryData(
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            Dictionary<string, DictionaryEntry> entryDictionary,
            List<DictionaryEntry> entryList,
            DataTable predictionTable)
        {
            TargetLanguageID = targetLanguageID;
            HostLanguageID = hostLanguageID;
            EntryDictionary = entryDictionary;
            EntryList = entryList;
            PredictionTable = predictionTable;
        }

        public GlossaryData()
        {
            TargetLanguageID = null;
            HostLanguageID = null;
            EntryDictionary = null;
            EntryList = null;
            PredictionTable = null;
        }

        public bool GetEntry(
            string key,
            out DictionaryEntry entry,
            out int glossaryIndex)
        {
            entry = null;
            glossaryIndex = -1;

            if (EntryDictionary.TryGetValue(key, out entry))
            {
                glossaryIndex = EntryList.IndexOf(entry);
                return true;
            }

            return false;
        }

        // Assumes already checked for existence.
        public bool AddEntry(
            DictionaryEntry entry,
            out int glossaryIndex)
        {
            glossaryIndex = EntryList.Count();
            EntryList.Add(entry);
            EntryDictionary.Add(entry.KeyString, entry);
            return true;
        }

        public static string GetKey(LanguageID targetLanguageID, LanguageID hostLanguageID)
        {
            string key = targetLanguageID.LanguageCultureExtensionCode + "_" + hostLanguageID.LanguageCultureExtensionCode;
            return key;
        }
    }
}
