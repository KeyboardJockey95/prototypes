using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;

namespace JTLanguageModelsPortable.Dictionary
{
    public class DictionaryCache : BaseObjectKeyed
    {
        protected Dictionary<string, Dictionary<string, DictionaryEntry>> SingleEntryCache;
        protected MatchCode LastSingleMatchCode;
        protected LanguageID LastSingleLanguageID;
        protected string LastSinglePattern;
        protected DictionaryEntry LastSingleDictionaryEntry;
        protected Dictionary<string, Dictionary<string, List<DictionaryEntry>>> MultipleEntryCache;
        protected MatchCode LastMultipleMatchCode;
        protected LanguageID LastMultipleLanguageID;
        protected string LastMultiplePattern;
        protected List<DictionaryEntry> LastMultipleDictionaryEntries;

        public DictionaryCache(object key) : base(key)
        {
            ClearDictionaryCache();
        }

        public DictionaryCache()
        {
            ClearDictionaryCache();
        }

        public void ClearDictionaryCache()
        {
            SingleEntryCache = new Dictionary<string, Dictionary<string, DictionaryEntry>>(StringComparer.OrdinalIgnoreCase);
            LastSingleMatchCode = MatchCode.Any;
            LastSingleLanguageID = null;
            LastSinglePattern = null;
            LastSingleDictionaryEntry = null;
            MultipleEntryCache = new Dictionary<string, Dictionary<string, List<DictionaryEntry>>>(StringComparer.OrdinalIgnoreCase);
            LastMultipleMatchCode = MatchCode.Any;
            LastMultipleLanguageID = null;
            LastMultiplePattern = null;
            LastMultipleDictionaryEntries = null;
        }

        public bool HasSingle(
            MatchCode matchCode,
            LanguageID targetLanguageID,
            string pattern)
        {
            if ((matchCode == LastSingleMatchCode) &&
                    (targetLanguageID == LastSingleLanguageID) &&
                    (pattern == LastSinglePattern))
                return true;

            Dictionary<string, DictionaryEntry> subCache = GetSingleSubCache(matchCode, targetLanguageID);
            DictionaryEntry dictionaryEntry;

            if (subCache.TryGetValue(pattern, out dictionaryEntry))
            {
                LastSingleMatchCode = matchCode;
                LastSingleLanguageID = targetLanguageID;
                LastSinglePattern = pattern;
                LastSingleDictionaryEntry = dictionaryEntry;
                return true;
            }

            return false;
        }

        public bool HasSingles(
            MatchCode matchCode,
            List<LanguageID> targetLanguageIDs,
            string pattern,
            bool isAll)
        {
            int count = 0;

            foreach (LanguageID targetLanguageID in targetLanguageIDs)
            {
                if (HasSingle(matchCode, targetLanguageID, pattern))
                {
                    if (!isAll)
                        return true;
                    count++;
                }
            }

            if (count == targetLanguageIDs.Count())
                return true;

            return false;
        }

        public bool HasMultiple(
            MatchCode matchCode,
            LanguageID targetLanguageID,
            string pattern)
        {
            if ((matchCode == LastMultipleMatchCode) &&
                    (targetLanguageID == LastMultipleLanguageID) &&
                    (pattern == LastMultiplePattern))
                return true;

            Dictionary<string, List<DictionaryEntry>> subCache = GetMultipleSubCache(matchCode, targetLanguageID);
            List<DictionaryEntry> dictionaryEntries;

            if (subCache.TryGetValue(pattern, out dictionaryEntries))
            {
                LastMultipleMatchCode = matchCode;
                LastMultipleLanguageID = targetLanguageID;
                LastMultiplePattern = pattern;
                LastMultipleDictionaryEntries = dictionaryEntries;
                return true;
            }

            return false;
        }

        public bool HasMultiples(
            MatchCode matchCode,
            List<LanguageID> targetLanguageIDs,
            string pattern,
            bool isAll)
        {
            int count = 0;

            foreach (LanguageID targetLanguageID in targetLanguageIDs)
            {
                if (HasMultiple(matchCode, targetLanguageID, pattern))
                {
                    if (!isAll)
                        return true;
                    count++;
                }
            }

            if (count == targetLanguageIDs.Count())
                return true;

            return false;
        }

        public bool FindSingle(
            MatchCode matchCode,
            LanguageID targetLanguageID,
            string pattern,
            out DictionaryEntry dictionaryEntry)
        {
            if ((matchCode == LastSingleMatchCode) &&
                (targetLanguageID == LastSingleLanguageID) &&
                (pattern == LastSinglePattern))
            {
                dictionaryEntry = LastSingleDictionaryEntry;
                return true;
            }

            Dictionary<string, DictionaryEntry> subCache = GetSingleSubCache(matchCode, targetLanguageID);

            if (subCache.TryGetValue(pattern, out dictionaryEntry))
            {
                LastSingleMatchCode = matchCode;
                LastSingleLanguageID = targetLanguageID;
                LastSinglePattern = pattern;
                LastSingleDictionaryEntry = dictionaryEntry;
                return true;
            }

            return false;
        }

        public bool FindSingle(
            MatchCode matchCode,
            List<LanguageID> targetLanguageIDs,
            string pattern,
            out DictionaryEntry dictionaryEntry)
        {
            foreach (LanguageID targetLanguageID in targetLanguageIDs)
            {
                if (FindSingle(matchCode, targetLanguageID, pattern, out dictionaryEntry))
                    return true;
            }

            dictionaryEntry = null;

            return false;
        }

        public bool FindSingles(
            MatchCode matchCode,
            List<LanguageID> targetLanguageIDs,
            string pattern,
            out List<DictionaryEntry> dictionaryEntries)
        {
            bool returnValue = false;

            dictionaryEntries = null;

            foreach (LanguageID targetLanguageID in targetLanguageIDs)
            {
                DictionaryEntry dictionaryEntry;

                if (FindSingle(matchCode, targetLanguageID, pattern, out dictionaryEntry))
                {
                    if (dictionaryEntries == null)
                        dictionaryEntries = new List<DictionaryEntry>() { dictionaryEntry };
                    else
                        dictionaryEntries.Add(dictionaryEntry);

                    returnValue = true;
                }
            }

            return returnValue;
        }

        public bool FindMultiple(
            MatchCode matchCode,
            LanguageID targetLanguageID,
            string pattern,
            out List<DictionaryEntry> dictionaryEntries)
        {
            if ((matchCode == LastMultipleMatchCode) &&
                (targetLanguageID == LastMultipleLanguageID) &&
                (pattern == LastMultiplePattern))
            {
                dictionaryEntries = LastMultipleDictionaryEntries;
                return true;
            }

            Dictionary<string, List<DictionaryEntry>> subCache = GetMultipleSubCache(matchCode, targetLanguageID);

            if (subCache.TryGetValue(pattern, out dictionaryEntries))
            {
                LastMultipleMatchCode = matchCode;
                LastMultipleLanguageID = targetLanguageID;
                LastMultiplePattern = pattern;
                LastMultipleDictionaryEntries = dictionaryEntries;
                return true;
            }

            return false;
        }

        public bool FindMultiples(
            MatchCode matchCode,
            List<LanguageID> targetLanguageIDs,
            string pattern,
            out List<DictionaryEntry> dictionaryEntries)
        {
            bool returnValue = false;

            dictionaryEntries = null;

            foreach (LanguageID targetLanguageID in targetLanguageIDs)
            {
                List<DictionaryEntry> entries;

                if (FindMultiple(matchCode, targetLanguageID, pattern, out entries))
                {
                    if (dictionaryEntries == null)
                        dictionaryEntries = entries;
                    else
                        dictionaryEntries.AddRange(entries);

                    returnValue = true;
                }
            }

            return returnValue;
        }

        public bool AddSingle(
            MatchCode matchCode,
            LanguageID targetLanguageID,
            string pattern,
            DictionaryEntry dictionaryEntry)
        {
            Dictionary<string, DictionaryEntry> subCache = GetSingleSubCache(matchCode, targetLanguageID);

            DebugCheck(pattern);

            if (!String.IsNullOrEmpty(pattern))
            {
                subCache.Add(pattern, dictionaryEntry);
                return true;
            }

            return false;
        }

        public bool AddSingleCheck(
            MatchCode matchCode,
            LanguageID targetLanguageID,
            string pattern,
            DictionaryEntry dictionaryEntry)
        {
            Dictionary<string, DictionaryEntry> subCache = GetSingleSubCache(matchCode, targetLanguageID);

            if (!String.IsNullOrEmpty(pattern))
            {
                DictionaryEntry testDictionaryEntry;

                DebugCheck(pattern);

                if (!subCache.TryGetValue(pattern, out testDictionaryEntry))
                {
                    subCache.Add(pattern, dictionaryEntry);
                    return true;
                }
            }

            return false;
        }

        public bool AddSingles(
            MatchCode matchCode,
            List<DictionaryEntry> dictionaryEntries)
        {
            bool returnValue = true;

            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
            {
                LanguageID targetLanguageID = dictionaryEntry.LanguageID;
                string key = dictionaryEntry.KeyString;
                Dictionary<string, DictionaryEntry> subCache = GetSingleSubCache(matchCode, targetLanguageID);
                DictionaryEntry testDictionaryEntry;

                DebugCheck(key);

                if (!subCache.TryGetValue(key, out testDictionaryEntry))
                {
                    subCache.Add(key, dictionaryEntry);
                    return true;
                }
            }

            return returnValue;
        }

        public bool AddSingles(
            MatchCode matchCode,
            List<LanguageID> targetLanguageIDs,
            string pattern,
            DictionaryEntry dictionaryEntry)
        {
            bool returnValue = true;

            DebugCheck(pattern);

            foreach (LanguageID targetLanguageID in targetLanguageIDs)
            {
                Dictionary<string, DictionaryEntry> subCache = GetSingleSubCache(matchCode, targetLanguageID);
                DictionaryEntry entry;

                if (!subCache.TryGetValue(pattern, out entry))
                    subCache.Add(pattern, dictionaryEntry);
                else
                    subCache[pattern] = dictionaryEntry;
            }

            return returnValue;
        }

        public string DebugWordFound;

        protected void DebugCheck(string word)
        {
            //if (word == "であられる")
            //{
            //    DebugWordFound = word;
            //}
        }

        public bool AddNonExistentSinglesCheck(
            MatchCode matchCode,
            List<LanguageID> targetLanguageIDs,
            string pattern,
            List<DictionaryEntry> dictionaryEntries)
        {
            bool returnValue = true;

            foreach (LanguageID targetLanguageID in targetLanguageIDs)
            {
                Dictionary<string, DictionaryEntry> subCache = GetSingleSubCache(matchCode, targetLanguageID);
                DictionaryEntry dictionaryEntry = dictionaryEntries.FirstOrDefault(x => x.LanguageID == targetLanguageID);
                DictionaryEntry dictionaryEntryTest = dictionaryEntries.FirstOrDefault(x => x.LanguageID == targetLanguageID);

                if (!subCache.TryGetValue(pattern, out dictionaryEntryTest))
                    subCache.Add(pattern, dictionaryEntry);
                else
                    subCache[pattern] = dictionaryEntry;
            }

            return returnValue;
        }

        public bool AddMultiple(
            MatchCode matchCode,
            LanguageID targetLanguageID,
            string pattern,
            List<DictionaryEntry> dictionaryEntries)
        {
            Dictionary<string, List<DictionaryEntry>> subCache = GetMultipleSubCache(matchCode, targetLanguageID);

            if (!String.IsNullOrEmpty(pattern))
            {
                subCache.Add(pattern, dictionaryEntries);
                return true;
            }

            return false;
        }

        public bool AddMultipleCheck(
            MatchCode matchCode,
            LanguageID targetLanguageID,
            string pattern,
            List<DictionaryEntry> dictionaryEntries)
        {
            Dictionary<string, List<DictionaryEntry>> subCache = GetMultipleSubCache(matchCode, targetLanguageID);

            if (!String.IsNullOrEmpty(pattern))
            {
                List<DictionaryEntry> testDictionaryEntries;

                if (!subCache.TryGetValue(pattern, out testDictionaryEntries))
                {
                    subCache.Add(pattern, dictionaryEntries);
                    return true;
                }
            }

            return false;
        }

        public bool AddMultiples(
            MatchCode matchCode,
            List<DictionaryEntry> dictionaryEntries)
        {
            bool returnValue = true;

            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
            {
                LanguageID targetLanguageID = dictionaryEntry.LanguageID;
                string key = dictionaryEntry.KeyString;
                Dictionary<string, List<DictionaryEntry>> subCache = GetMultipleSubCache(matchCode, targetLanguageID);
                List<DictionaryEntry> entries;

                if (subCache.TryGetValue(key, out entries))
                    entries.Add(dictionaryEntry);
                else
                {
                    entries = new List<DictionaryEntry>() { dictionaryEntry };
                    subCache.Add(key, entries);
                }
            }

            return returnValue;
        }

        public bool AddMultiples(
            MatchCode matchCode,
            List<LanguageID> targetLanguageIDs,
            string pattern,
            List<DictionaryEntry> dictionaryEntries)
        {
            bool returnValue = true;

            foreach (LanguageID targetLanguageID in targetLanguageIDs)
            {
                Dictionary<string, List<DictionaryEntry>> subCache = GetMultipleSubCache(matchCode, targetLanguageID);
                List<DictionaryEntry> entries;

                if (!subCache.TryGetValue(pattern, out entries))
                    subCache.Add(pattern, dictionaryEntries);
                else
                    subCache[pattern] = dictionaryEntries;
            }

            return returnValue;
        }

        public bool AddEmptiesCheck(
            MatchCode matchCode,
            List<LanguageID> targetLanguageIDs,
            string pattern)
        {
            bool returnValue = true;

            foreach (LanguageID targetLanguageID in targetLanguageIDs)
            {
                Dictionary<string, List<DictionaryEntry>> subCache = GetMultipleSubCache(matchCode, targetLanguageID);
                List<DictionaryEntry> entries;

                if (!subCache.TryGetValue(pattern, out entries))
                    subCache.Add(pattern, null);
                else
                    subCache[pattern] = null;
            }

            return returnValue;
        }

        public bool RemoveSingle(
            MatchCode matchCode,
            LanguageID targetLanguageID,
            string pattern)
        {
            Dictionary<string, DictionaryEntry> subCache = GetSingleSubCache(matchCode, targetLanguageID);
            bool returnValue = subCache.Remove(pattern);

            if ((matchCode == LastSingleMatchCode) &&
                (targetLanguageID == LastSingleLanguageID) &&
                (pattern == LastSinglePattern))
            {
                LastSingleMatchCode = MatchCode.Any;
                LastSingleLanguageID = null;
                LastSinglePattern = null;
                LastSingleDictionaryEntry = null;
            }

            return returnValue;
        }

        public bool RemoveSingles(
            MatchCode matchCode,
            List<DictionaryEntry> dictionaryEntries)
        {
            bool returnValue = true;

            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
            {
                LanguageID targetLanguageID = dictionaryEntry.LanguageID;
                string key = dictionaryEntry.KeyString;

                if (!RemoveSingle(matchCode, targetLanguageID, key))
                    returnValue = false;
            }

            return returnValue;
        }

        public bool RemoveMultiple(
            MatchCode matchCode,
            LanguageID targetLanguageID,
            string pattern)
        {
            Dictionary<string, List<DictionaryEntry>> subCache = GetMultipleSubCache(matchCode, targetLanguageID);
            bool returnValue = subCache.Remove(pattern);

            if ((matchCode == LastMultipleMatchCode) &&
                (targetLanguageID == LastMultipleLanguageID) &&
                (pattern == LastMultiplePattern))
            {
                LastMultipleMatchCode = MatchCode.Any;
                LastMultipleLanguageID = null;
                LastMultiplePattern = null;
                LastMultipleDictionaryEntries = null;
            }

            return returnValue;
        }

        public bool RemoveMultiples(
            MatchCode matchCode,
            List<DictionaryEntry> dictionaryEntries)
        {
            bool returnValue = true;

            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
            {
                LanguageID targetLanguageID = dictionaryEntry.LanguageID;
                string key = dictionaryEntry.KeyString;

                if (!RemoveMultiple(matchCode, targetLanguageID, key))
                    returnValue = false;
            }

            return returnValue;
        }

        public void GetAllSingles(
            MatchCode matchCode,
            LanguageID targetLanguageID,
            ref List<DictionaryEntry> dictionaryEntries)
        {
            Dictionary<string, DictionaryEntry> subCache = GetSingleSubCache(matchCode, targetLanguageID);

            if (subCache.Count() == 0)
                return;

            if (dictionaryEntries == null)
                dictionaryEntries = new List<DictionaryEntry>();

            foreach (KeyValuePair<string, DictionaryEntry> kvp in subCache)
                dictionaryEntries.Add(kvp.Value);
        }

        public void GetAllSingles(
            MatchCode matchCode,
            List<LanguageID> targetLanguageIDs,
            ref List<DictionaryEntry> dictionaryEntries)
        {
            foreach (LanguageID targetLanguageID in targetLanguageIDs)
                GetAllSingles(matchCode, targetLanguageID, ref dictionaryEntries);
        }

        public void GetAllSinglesKeys(
            MatchCode matchCode,
            LanguageID targetLanguageID,
            ref List<string> dictionaryEntryKeys)
        {
            Dictionary<string, DictionaryEntry> subCache = GetSingleSubCache(matchCode, targetLanguageID);

            if (subCache.Count() == 0)
                return;

            if (dictionaryEntryKeys == null)
                dictionaryEntryKeys = new List<string>();

            foreach (KeyValuePair<string, DictionaryEntry> kvp in subCache)
                dictionaryEntryKeys.Add(kvp.Key);
        }

        public void GetAllSinglesKeys(
            MatchCode matchCode,
            List<LanguageID> targetLanguageIDs,
            ref List<string> dictionaryEntryKeys)
        {
            foreach (LanguageID targetLanguageID in targetLanguageIDs)
                GetAllSinglesKeys(matchCode, targetLanguageID, ref dictionaryEntryKeys);
        }

        protected Dictionary<string, DictionaryEntry> GetSingleSubCache(
            MatchCode matchCode,
            LanguageID targetLanguageID)
        {
            Dictionary<string, DictionaryEntry> subCache;
            string key = ComposeSubCacheKey(matchCode, targetLanguageID);
            if (SingleEntryCache.TryGetValue(key, out subCache))
                return subCache;
            subCache = new Dictionary<string, DictionaryEntry>(StringComparer.OrdinalIgnoreCase);
            SingleEntryCache.Add(key, subCache);
            return subCache;
        }

        protected Dictionary<string, List<DictionaryEntry>> GetMultipleSubCache(
            MatchCode matchCode,
            LanguageID targetLanguageID)
        {
            Dictionary<string, List<DictionaryEntry>> subCache;
            string key = ComposeSubCacheKey(matchCode, targetLanguageID);
            if (MultipleEntryCache.TryGetValue(key, out subCache))
                return subCache;
            subCache = new Dictionary<string, List<DictionaryEntry>>(StringComparer.OrdinalIgnoreCase);
            MultipleEntryCache.Add(key, subCache);
            return subCache;
        }

        protected string ComposeSubCacheKey(
            MatchCode matchCode,
            LanguageID targetLanguageID)
        {
            string key = matchCode.ToString() + "_" + targetLanguageID.LanguageCultureExtensionCode;
            return key;
        }
    }
}
