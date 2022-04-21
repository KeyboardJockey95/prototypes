using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public enum InflectionOutputMode
    {
        MainWordPlusPrePostWords,
        FullNoPronouns,
        FullWithPronouns,
        FullNoMain,
        All
    }

    public partial class LanguageTool : BaseObjectLanguage
    {
        protected DictionaryRepository _DictionaryDatabase;
        protected DictionaryCache _DictionaryCacheFound;
        protected DictionaryCache _DictionaryCacheNotFound;
        protected DictionaryCache _LookupCacheFound;
        protected DictionaryCache _LookupCacheNotFound;

        protected DictionaryRepository _DictionaryStemsDatabase;
        protected DictionaryCache _DictionaryStemsCacheFound;
        protected DictionaryCache _DictionaryStemsCacheNotFound;
        protected DictionaryCache _LookupStemsCacheFound;
        protected DictionaryCache _LookupStemsCacheNotFound;

        protected DeinflectionRepository _DeinflectionDatabase;
        protected Dictionary<string, Deinflection> _DeinflectionCache;
        protected Dictionary<string, string[]> _ContainingPhrasesCache;

        // For one-language LanguageTools only.
        public virtual DictionaryEntry GetDictionaryEntry(string dictionaryForm)
        {
            DictionaryEntry dictionaryEntry;

            lock (this)
            {
                if (DictionaryCacheFound.FindSingle(
                        MatchCode.Exact,
                        LanguageID,
                        dictionaryForm,
                        out dictionaryEntry))
                    return dictionaryEntry;

                if (DictionaryCacheNotFound.HasSingle(
                        MatchCode.Exact,
                        LanguageID,
                        dictionaryForm))
                    return null;

                foreach (LanguageID languageID in TargetLanguageIDs)
                {
                    dictionaryEntry = GetDictionaryLanguageEntryNoCache(dictionaryForm, languageID);

                    if (dictionaryEntry != null)
                        break;
                }

                if (dictionaryEntry == null)
                    DictionaryCacheNotFound.AddSingle(
                        MatchCode.Exact,
                        LanguageID,
                        dictionaryForm,
                        dictionaryEntry);
                else
                    DictionaryCacheFound.AddSingle(
                        MatchCode.Exact,
                        LanguageID,
                        dictionaryForm,
                        dictionaryEntry);

                return dictionaryEntry;
            }
        }

        public virtual DictionaryEntry GetDictionaryLanguagesEntry(string dictionaryForm, List<LanguageID> languageIDs)
        {
            DictionaryEntry dictionaryEntry;

            foreach (LanguageID languageID in languageIDs)
            {
                dictionaryEntry = GetDictionaryLanguageEntry(dictionaryForm, languageID);

                if (dictionaryEntry != null)
                    return dictionaryEntry;
            }

            return null;
        }

        public virtual DictionaryEntry GetDictionaryLanguageEntry(string dictionaryForm, LanguageID languageID)
        {
            DictionaryEntry dictionaryEntry;

            lock (this)
            {
                if (DictionaryCacheFound.FindSingle(
                        MatchCode.Exact,
                        languageID,
                        dictionaryForm,
                        out dictionaryEntry))
                    return dictionaryEntry;

                if (DictionaryCacheNotFound.HasSingle(
                        MatchCode.Exact,
                        languageID,
                        dictionaryForm))
                    return null;

                dictionaryEntry = GetDictionaryLanguageEntryNoCache(dictionaryForm, languageID);

                if (dictionaryEntry == null)
                    DictionaryCacheNotFound.AddSingle(
                        MatchCode.Exact,
                        languageID,
                        dictionaryForm,
                        dictionaryEntry);
                else
                    DictionaryCacheFound.AddSingle(
                        MatchCode.Exact,
                        languageID,
                        dictionaryForm,
                        dictionaryEntry);

                return dictionaryEntry;
            }
        }

        public virtual DictionaryEntry GetDictionaryLanguageEntryNoCache(string dictionaryForm, LanguageID languageID)
        {
            string pattern = GetNormalizedText(dictionaryForm);
            DictionaryEntry dictionaryEntry = DictionaryDatabase.Get(pattern, languageID);
            return dictionaryEntry;
        }

        public virtual DictionaryEntry GetDictionaryLanguageEntryNoLock(string dictionaryForm, LanguageID languageID)
        {
            DictionaryEntry dictionaryEntry;

            if (DictionaryCacheFound.FindSingle(
                    MatchCode.Exact,
                    languageID,
                    dictionaryForm,
                    out dictionaryEntry))
                return dictionaryEntry;

            if (DictionaryCacheNotFound.HasSingle(
                    MatchCode.Exact,
                    languageID,
                    dictionaryForm))
                return null;

            dictionaryEntry = GetDictionaryLanguageEntryNoCache(dictionaryForm, languageID);

            if (dictionaryEntry == null)
                DictionaryCacheNotFound.AddSingle(
                    MatchCode.Exact,
                    languageID,
                    dictionaryForm,
                    dictionaryEntry);
            else
                DictionaryCacheFound.AddSingle(
                    MatchCode.Exact,
                    languageID,
                    dictionaryForm,
                    dictionaryEntry);

            return dictionaryEntry;
        }

        public virtual List<DictionaryEntry> GetDictionaryLanguageEntries(
            string dictionaryForm,
            List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return null;

            List<DictionaryEntry> dictionaryEntries = new List<DictionaryEntry>();

            lock (this)
            {
                foreach (LanguageID languageID in languageIDs)
                {
                    DictionaryEntry dictionaryEntry = GetDictionaryLanguageEntry(dictionaryForm, languageID);

                    if (dictionaryEntry != null)
                        dictionaryEntries.Add(dictionaryEntry);

                    dictionaryEntry = GetInflectionDictionaryLanguageEntry(dictionaryForm, languageID);

                    if (dictionaryEntry != null)
                        dictionaryEntries.Add(dictionaryEntry);
                }
            }

            return dictionaryEntries;
        }

        public virtual List<DictionaryEntry> GetDictionaryLanguageEntriesNoCache(
            string dictionaryForm,
            List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return null;

            List<DictionaryEntry> dictionaryEntries = new List<DictionaryEntry>();

            lock (this)
            {
                foreach (LanguageID languageID in languageIDs)
                {
                    DictionaryEntry dictionaryEntry = GetDictionaryLanguageEntryNoCache(dictionaryForm, languageID);

                    if (dictionaryEntry != null)
                        dictionaryEntries.Add(dictionaryEntry);
                }
            }

            return dictionaryEntries;
        }

        public virtual DictionaryEntry GetInflectionDictionaryLanguageEntry(string inflectedForm, LanguageID languageID)
        {
            inflectedForm = GetNormalizedText(inflectedForm);

            string preWords = null;
            string prefix = null;
            string baseInflectedForm = null;
            string suffix = null;
            string postWords = null;
            Designator overrideDesignator = null;
            List<Designator> extendedDesignators = null;
            Deinflection deinflection = GetDeinflectionCached(inflectedForm, languageID);
            DictionaryEntry dictionaryEntry = null;

            if (deinflection == null)
            {
                if (GetExtendedInflection(
                        inflectedForm,
                        languageID,
                        out preWords,
                        out prefix,
                        out baseInflectedForm,
                        out suffix,
                        out postWords,
                        out overrideDesignator,
                        out extendedDesignators,
                        out deinflection,
                        out dictionaryEntry))
                {
                    dictionaryEntry = GetExtendedDictionaryEntryFromDeinflection(
                        inflectedForm,
                        languageID,
                        preWords,
                        prefix,
                        baseInflectedForm,
                        suffix,
                        postWords,
                        overrideDesignator,
                        extendedDesignators,
                        deinflection);
                }
            }
            else
                dictionaryEntry = GetSimpleDictionaryEntryFromDeinflection(
                    inflectedForm,
                    languageID,
                    deinflection);

            return dictionaryEntry;
        }

        public virtual DictionaryEntry GetSimpleDictionaryEntryFromDeinflection(
            string baseInflectedForm,
            LanguageID languageID,
            Deinflection deinflection)
        {
            Inflection inflection = null;
            DictionaryEntry dictionaryEntry = null;

            if (deinflection == null)
                return null;

            if (deinflection.Instances == null)
                return null;

            try
            {
                foreach (DeinflectionInstance deinflectionInstance in deinflection.Instances)
                {
                    if (deinflectionInstance == null)
                        continue;

                    string dictionaryForm = deinflectionInstance.DictionaryForm;
                    LexicalCategory category = deinflectionInstance.Category;
                    string categoryString = deinflectionInstance.CategoryString;
                    string label = deinflectionInstance.InflectionLabel;
                    InflectorTable inflectorTable = InflectorTable(category.ToString());

                    if (inflectorTable == null)
                        continue;

                    Designator designator = inflectorTable.GetDesignator("All", label);

                    if ((designator == null) && label.Contains("Archaic"))
                        designator = inflectorTable.GetDesignator("Archaic", label);

                    if (designator == null)
                        continue;

                    DictionaryEntry dictionaryFormEntry = GetDictionaryLanguageEntryNoLock(dictionaryForm, languageID);

                    if (dictionaryFormEntry == null)
                        continue;

                    Sense sense = null;
                    int senseIndex = 0;
                    int senseCount = dictionaryFormEntry.SenseCount;
                    LanguageID hostLanguageID = null;

                    if (MultiTool != null)
                        hostLanguageID = MultiTool.HostLanguageIDs.First();

                    for (senseIndex = 0; senseIndex < senseCount; senseIndex++)
                    {
                        sense = dictionaryFormEntry.GetSenseIndexed(senseIndex);

                        if (sense == null)
                            continue;

                        if (sense.LanguageSynonyms == null)
                            continue;

                        if (sense.Category != category)
                            continue;

                        if (!String.IsNullOrEmpty(categoryString) && !String.IsNullOrEmpty(sense.CategoryString))
                        {
                            if (categoryString != sense.CategoryString)
                                continue;
                        }

                        if (sense.LanguageSynonyms == null)
                            continue;

                        foreach (LanguageSynonyms languageSynonymns in sense.LanguageSynonyms)
                        {
                            if ((hostLanguageID != null) && (languageSynonymns.LanguageID != hostLanguageID))
                                continue;

                            int synonymCount = languageSynonymns.SynonymCount;
                            int synonymIndex = 0;

                            for (synonymIndex = 0; synonymIndex < synonymCount; synonymIndex++)
                            {
                                string synonym = languageSynonymns.GetSynonymIndexed(synonymIndex);
                                bool gotInflection;

                                if (String.IsNullOrEmpty(synonym))
                                    continue;

                                int wordCount = TextUtilities.CountChars(synonym, ' ') + 1;
                                int wordCountLimit = 1;

                                if (category == LexicalCategory.Verb)
                                    wordCountLimit = 3;

                                if (wordCount > wordCountLimit)
                                    continue;

                                if (MultiTool != null)
                                    gotInflection = MultiTool.InflectAnyDictionaryFormDesignated(
                                        dictionaryFormEntry,
                                        senseIndex,
                                        synonymIndex,
                                        designator,
                                        out inflection);
                                else
                                    gotInflection = InflectAnyDictionaryFormDesignated(
                                        dictionaryFormEntry,
                                        ref senseIndex,
                                        ref synonymIndex,
                                        designator,
                                        out inflection);

                                if (!gotInflection)
                                    continue;

                                DictionaryEntry inflectionDictionaryEntry = CreateInflectionDictionaryEntry(
                                    baseInflectedForm,
                                    languageID,
                                    inflection,
                                    DefaultInflectionOutputMode);

                                if (inflectionDictionaryEntry == null)
                                    continue;

                                string keyLower = inflectionDictionaryEntry.KeyString.ToLower();
                                string inflectedLower = baseInflectedForm.ToLower();

                                if ((keyLower != inflectedLower) && !inflectedLower.Contains(keyLower))
                                    ApplicationData.Global.PutConsoleMessage("Dictionary keys not the same: " + baseInflectedForm + " vs. " +
                                        inflectionDictionaryEntry.KeyString + "\nThis usually means the Deinflection database is out of sync with the inflection table and needs regeneration.");

                                if (dictionaryEntry == null)
                                    dictionaryEntry = inflectionDictionaryEntry;
                                else
                                    dictionaryEntry.MergeEntry(inflectionDictionaryEntry);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return dictionaryEntry;
        }

        public virtual DictionaryEntry GetExtendedDictionaryEntryFromDeinflection(
            string inflectedForm,
            LanguageID languageID,
            string preWords,
            string prefix,
            string baseInflectedForm,
            string suffix,
            string postWords,
            Designator overrideDesignator,
            List<Designator> extendedDesignators,
            Deinflection deinflection)
        {
            Inflection inflection = null;
            DictionaryEntry dictionaryEntry = null;

            foreach (DeinflectionInstance deinflectionInstance in deinflection.Instances)
            {
                string dictionaryForm = deinflectionInstance.DictionaryForm;
                LexicalCategory category = deinflectionInstance.Category;
                string categoryString = deinflectionInstance.CategoryString;

                if (!IsExtendedInflectionValid(
                        extendedDesignators,
                        category,
                        categoryString))
                    continue;

                string label = deinflectionInstance.InflectionLabel;
                InflectorTable inflectorTable = InflectorTable(category.ToString());

                if (inflectorTable == null)
                    continue;

                Designator designator = inflectorTable.GetDesignator("All", label);

                if (designator == null)
                    continue;

                if (overrideDesignator != null)
                {
                    designator = new Designator(designator);
                    designator.ApplyOverride(overrideDesignator);
                }

                DictionaryEntry dictionaryFormEntry = GetDictionaryLanguageEntryNoLock(dictionaryForm, languageID);

                if (dictionaryFormEntry == null)
                    continue;

                Sense sense = null;
                Sense testSense = null;
                int senseIndex = 0;
                int senseCount = dictionaryFormEntry.SenseCount;

                for (senseIndex = 0; senseIndex < senseCount; senseIndex++)
                {
                    testSense = dictionaryFormEntry.GetSenseIndexed(senseIndex);

                    if (testSense.Category != category)
                        continue;

                    if (!String.IsNullOrEmpty(categoryString) && !String.IsNullOrEmpty(testSense.CategoryString))
                    {
                        if (categoryString != testSense.CategoryString)
                            continue;
                    }

                    sense = testSense;
                    break;
                }

                if (sense == null)
                    continue;

                if (sense.LanguageSynonyms == null)
                    continue;

                foreach (LanguageSynonyms languageSynonymns in sense.LanguageSynonyms)
                {
                    int synonymCount = languageSynonymns.SynonymCount;
                    int synonymIndex = 0;

                    for (synonymIndex = 0; synonymIndex < synonymCount; synonymIndex++)
                    {
                        string synonym = languageSynonymns.GetSynonymIndexed(synonymIndex);
                        bool gotInflection;

                        if (MultiTool != null)
                            gotInflection = MultiTool.InflectAnyDictionaryFormDesignated(
                                dictionaryFormEntry,
                                senseIndex,
                                synonymIndex,
                                designator,
                                out inflection);
                        else
                            gotInflection = InflectAnyDictionaryFormDesignated(
                                dictionaryFormEntry,
                                ref senseIndex,
                                ref synonymIndex,
                                designator,
                                out inflection);

                        if (!gotInflection)
                            continue;

                        FixupExtendedInflection(
                            inflection,
                            languageID,
                            preWords,
                            prefix,
                            baseInflectedForm,
                            suffix,
                            postWords,
                            extendedDesignators,
                            deinflection,
                            inflectedForm);

                        string mainWord = inflection.GetMainWord(languageID);

                        if (!TextUtilities.ContainsWholeWord(inflectedForm, mainWord))
                            continue;

                        foreach (Designator extendedDesignator in extendedDesignators)
                        {
                            Inflection newInflection = new Inflection(inflection);

                            newInflection.Designation.AppendClassifications(extendedDesignator);
                            newInflection.Designation.DefaultLabel();

                            if (MultiTool != null)
                                MultiTool.ExtendHostInflection(newInflection);

                            DictionaryEntry inflectionDictionaryEntry = CreateInflectionDictionaryEntry(
                                inflectedForm,
                                languageID,
                                newInflection,
                                DefaultInflectionOutputMode);

                            if (dictionaryEntry == null)
                                dictionaryEntry = inflectionDictionaryEntry;
                            else
                                dictionaryEntry.MergeEntry(inflectionDictionaryEntry);
                        }
                    }
                }
            }

            return dictionaryEntry;
        }

        public virtual bool IsExtendedInflectionValid(
            List<Designator> extendedDesignators,
            LexicalCategory category,
            string categoryString)
        {
            return true;
        }

        public virtual void FixupExtendedInflection(
            Inflection inflection,
            LanguageID languageID,
            string preWords,
            string prefix,
            string baseInflectedForm,
            string suffix,
            string postWords,
            List<Designator> extendedDesignators,
            Deinflection deinflection,
            string inflectedForm)
        {
            inflection.ExtendLanguage(
                languageID,
                preWords,
                prefix,
                suffix,
                postWords);
        }

        public virtual Deinflection GetDeinflectionCached(
            string inflectedForm,
            LanguageID languageID)
        {
            Deinflection deinflection;

            if (DeinflectionCache.TryGetValue(inflectedForm, out deinflection))
                return deinflection;

            deinflection = DeinflectionDatabase.Get(inflectedForm, languageID);

            DeinflectionCache.Add(inflectedForm, deinflection);

            return deinflection;
        }

        public virtual bool GetExtendedInflection(
            string inflectedForm,
            LanguageID languageID,
            out string preWords,
            out string prefix,
            out string baseInflectedForm,
            out string suffix,
            out string postWords,
            out Designator overrideDesignator,
            out List<Designator> extendedDesignators,
            out Deinflection deinflection,
            out DictionaryEntry dictionaryEntry)
        {
            bool returnValue = false;

            preWords = null;
            prefix = null;
            baseInflectedForm = null;
            suffix = null;
            postWords = null;
            overrideDesignator = null;
            extendedDesignators = null;
            deinflection = null;
            dictionaryEntry = null;

            string[] parts = inflectedForm.Split(LanguageLookup.Space);
            int foundIndex = 0;

            if (GetSupplementedInflection(
                inflectedForm,
                languageID,
                parts,
                out preWords,
                out prefix,
                out baseInflectedForm,
                out suffix,
                out postWords,
                out overrideDesignator,
                out extendedDesignators,
                out deinflection))
            {
                return true;
            }

            foreach (string part in parts)
            {
                if (GetExtendedInflectionBase(
                    part,
                    languageID,
                    out prefix,
                    out baseInflectedForm,
                    out suffix,
                    out extendedDesignators,
                    out deinflection))
                {
                    returnValue = true;
                    break;
                }

                foundIndex++;
            }

            if (returnValue && (parts.Length > 1))
            {
                int index = 0;

                foreach (string part in parts)
                {
                    if (index != foundIndex)
                    {
                        Designator tempDesignator;

                        ProcessOtherWordsInInflection(
                            part,
                            languageID,
                            index > foundIndex,
                            ref preWords,
                            ref postWords,
                            out tempDesignator);

                        if (tempDesignator != null)
                        {
                            if (overrideDesignator != null)
                                tempDesignator.AppendClassifications(tempDesignator);
                            else
                                overrideDesignator = tempDesignator;
                        }
                    }

                    index++;
                }
            }

            return returnValue;
        }

        public virtual bool GetSupplementedInflection(
            string inflectedForm,
            LanguageID languageID,
            string[] parts,
            out string preWords,
            out string prefix,
            out string baseInflectedForm,
            out string suffix,
            out string postWords,
            out Designator overrideDesignator,
            out List<Designator> extendedDesignators,
            out Deinflection deinflection)
        {
            preWords = null;
            prefix = null;
            baseInflectedForm = null;
            suffix = null;
            postWords = null;
            overrideDesignator = null;
            extendedDesignators = null;
            deinflection = null;
            return false;
        }

        public virtual bool GetExtendedInflectionBase(
            string word,
            LanguageID languageID,
            out string prefix,
            out string baseInflectedForm,
            out string suffix,
            out List<Designator> extendedDesignators,
            out Deinflection deinflection)
        {
            prefix = null;
            baseInflectedForm = null;
            suffix = null;
            extendedDesignators = null;
            deinflection = null;
            return false;
        }

        public virtual void ProcessOtherWordsInInflection(
            string word,
            LanguageID languageID,
            bool isPost,
            ref string preWords,
            ref string postWords,
            out Designator overideDesignator)
        {
            overideDesignator = null;
        }

        // For one-language LanguageTools only.
        public virtual DictionaryEntry GetStemDictionaryEntry(string dictionaryForm)
        {
            DictionaryEntry dictionaryEntry;

            lock (this)
            {
                if (DictionaryStemsCacheFound.FindSingle(
                        MatchCode.Exact,
                        LanguageID,
                        dictionaryForm,
                        out dictionaryEntry))
                    return dictionaryEntry;

                if (DictionaryStemsCacheNotFound.HasSingle(
                        MatchCode.Exact,
                        LanguageID,
                        dictionaryForm))
                    return null;

                foreach (LanguageID languageID in TargetLanguageIDs)
                {
                    dictionaryEntry = DictionaryStemsDatabase.Get(dictionaryForm, languageID);

                    if (dictionaryEntry != null)
                        break;
                }

                if (dictionaryEntry == null)
                    DictionaryStemsCacheNotFound.AddSingle(
                        MatchCode.Exact,
                        LanguageID,
                        dictionaryForm,
                        dictionaryEntry);
                else
                    DictionaryStemsCacheFound.AddSingle(
                        MatchCode.Exact,
                        LanguageID,
                        dictionaryForm,
                        dictionaryEntry);

                return dictionaryEntry;
            }
        }

        public virtual DictionaryEntry GetStemDictionaryLanguagesEntry(string dictionaryForm, List<LanguageID> languageIDs)
        {
            DictionaryEntry dictionaryEntry;

            foreach (LanguageID languageID in languageIDs)
            {
                dictionaryEntry = GetStemDictionaryLanguageEntry(dictionaryForm, languageID);

                if (dictionaryEntry != null)
                    return dictionaryEntry;
            }

            return null;
        }

        public virtual DictionaryEntry GetStemDictionaryLanguageEntry(string dictionaryForm, LanguageID languageID)
        {
            DictionaryEntry dictionaryEntry;

            lock (this)
            {
                if (DictionaryStemsCacheFound.FindSingle(
                        MatchCode.Exact,
                        languageID,
                        dictionaryForm,
                        out dictionaryEntry))
                    return dictionaryEntry;

                if (DictionaryStemsCacheNotFound.HasSingle(
                        MatchCode.Exact,
                        languageID,
                        dictionaryForm))
                    return null;

                dictionaryEntry = DictionaryStemsDatabase.Get(dictionaryForm, languageID);

                if (dictionaryEntry == null)
                    DictionaryStemsCacheNotFound.AddSingle(
                        MatchCode.Exact,
                        languageID,
                        dictionaryForm,
                        dictionaryEntry);
                else
                    DictionaryStemsCacheFound.AddSingle(
                        MatchCode.Exact,
                        languageID,
                        dictionaryForm,
                        dictionaryEntry);

                return dictionaryEntry;
            }
        }

        public virtual DictionaryEntry GetStemDictionaryLanguageEntryNoCache(string dictionaryForm, LanguageID languageID)
        {
            DictionaryEntry dictionaryEntry = DictionaryStemsDatabase.Get(dictionaryForm, languageID);
            return dictionaryEntry;
        }

        public virtual List<DictionaryEntry> GetStemDictionaryLanguageEntries(
            string dictionaryForm,
            List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return null;

            List<DictionaryEntry> dictionaryEntries = new List<DictionaryEntry>();

            lock (this)
            {
                foreach (LanguageID languageID in languageIDs)
                {
                    DictionaryEntry dictionaryEntry = GetStemDictionaryLanguageEntry(dictionaryForm, languageID);

                    if (dictionaryEntry != null)
                        dictionaryEntries.Add(dictionaryEntry);
                }
            }

            return dictionaryEntries;
        }

        public virtual List<DictionaryEntry> GetStemDictionaryLanguageEntriesNoCache(
            string dictionaryForm,
            List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return null;

            List<DictionaryEntry> dictionaryEntries = new List<DictionaryEntry>();

            lock (this)
            {
                foreach (LanguageID languageID in languageIDs)
                {
                    DictionaryEntry dictionaryEntry = GetStemDictionaryLanguageEntryNoCache(dictionaryForm, languageID);

                    if (dictionaryEntry != null)
                        dictionaryEntries.Add(dictionaryEntry);
                }
            }

            return dictionaryEntries;
        }

        // Returns true if found either.
        // For one-language LanguageTools only.
        public virtual bool GetStemAndNonStemDictionaryEntry(
            string pattern,
            out DictionaryEntry stemEntry,
            out DictionaryEntry nonStemEntry)
        {
            stemEntry = GetStemDictionaryEntry(pattern);

            if (stemEntry == null)
                nonStemEntry = GetDictionaryEntry(pattern);
            else
                nonStemEntry = null;

            return (stemEntry != null) || (nonStemEntry != null);
        }

        public virtual bool GetStemDictionaryFormEntry(
            DictionaryEntry stemEntry,
            List<DictionaryEntry> formAndStemEntries)
        {
            if (stemEntry.SenseCount == 0)
                return false;

            bool returnValue = false;

            foreach (Sense sense in stemEntry.Senses)
            {
                if ((sense.Category == LexicalCategory.Stem) || (sense.Category == LexicalCategory.IrregularStem))
                {
                    string dictionaryForm = sense.GetSynonymIndexed(stemEntry.LanguageID, 0);

                    if (String.IsNullOrEmpty(dictionaryForm))
                        continue;

                    if (formAndStemEntries.FirstOrDefault(x => x.MatchKey(dictionaryForm)) != null)
                    {
                        returnValue = true;
                        continue;
                    }

                    DictionaryEntry formEntry = GetDictionaryLanguageEntry(
                        dictionaryForm,
                        stemEntry.LanguageID);

                    if (formEntry != null)
                    {
                        formAndStemEntries.Add(formEntry);
                        returnValue = true;
                    }
                }
            }

            return returnValue;
        }

        public virtual List<DictionaryEntry> LookupFirstDictionaryEntries(
            string pattern,
            MatchCode matchType,
            List<LanguageID> languageIDs,
            int page,
            int pageSize,
            out LanguageID foundLanguageID)
        {
            List<DictionaryEntry> dictionaryEntries = null;

            foundLanguageID = null;

            foreach (LanguageID languageID in languageIDs)
            {
                if (!LookupDictionaryEntriesFilterCheck(pattern, languageID))
                    continue;

                dictionaryEntries = DictionaryDatabase.Lookup(
                    pattern,
                    matchType,
                    languageID,
                    page,
                    pageSize);

                if (dictionaryEntries == null)
                    continue;

                if (dictionaryEntries.Count() != 0)
                {
                    foundLanguageID = languageID;
                    return (dictionaryEntries);
                }
            }

            if (dictionaryEntries == null)
                dictionaryEntries = new List<DictionaryEntry>();

            return dictionaryEntries;
        }

        public virtual DictionaryEntry LookupFirstDictionaryEntry(
            string pattern,
            MatchCode matchType,
            List<LanguageID> languageIDs)
        {
            foreach (LanguageID languageID in languageIDs)
            {
                DictionaryEntry dictionaryEntry = LookupFirstDictionaryEntry(
                    pattern, matchType, languageID);

                if (dictionaryEntry != null)
                    return dictionaryEntry;
            }

            return null;
        }

        public virtual DictionaryEntry LookupFirstDictionaryEntry(
            string pattern,
            MatchCode matchType,
            LanguageID languageID)
        {
            if (!LookupDictionaryEntriesFilterCheck(pattern, languageID))
                return null;

            if (matchType == MatchCode.Exact)
                return GetDictionaryLanguageEntry(pattern, languageID);

            List<DictionaryEntry> dictionaryEntries = null;

            dictionaryEntries = DictionaryDatabase.Lookup(
                pattern,
                matchType,
                languageID,
                0,
                0);

            if (dictionaryEntries == null)
                return null;

            if (dictionaryEntries.Count() != 0)
                return dictionaryEntries.First();

            return null;
        }

        // Look word or words matching the pattern, including possibly conjugated.
        public virtual List<DictionaryEntry> LookupDictionaryEntries(
            string pattern,
            MatchCode matchType,
            List<LanguageID> languageIDs,
            int page,
            int pageSize)
        {
            List<DictionaryEntry> dictionaryEntries = null;
            List<DictionaryEntry> collectedDictionaryEntries = new List<DictionaryEntry>();

            switch (matchType)
            {
                case MatchCode.ParseBest:
                    collectedDictionaryEntries = LookupDictionaryEntriesParseBest(
                        pattern,
                        languageIDs);
                    break;
                case MatchCode.ParseAll:
                    collectedDictionaryEntries = LookupDictionaryEntriesParseAll(
                        pattern,
                        languageIDs);
                    break;
                case MatchCode.Exact:
                    collectedDictionaryEntries = LookupDictionaryEntriesExact(
                        pattern,
                        languageIDs);
                    break;
                default:
                    collectedDictionaryEntries = LookupDictionaryEntriesMatching(
                        pattern,
                        matchType,
                        languageIDs);
                    break;
            }

            if ((collectedDictionaryEntries != null) && (pageSize > 0) && (page > 0))
            {
                int start = (page - 1) * pageSize;
                int count = pageSize;

                if (start < collectedDictionaryEntries.Count())
                {
                    if (count > (collectedDictionaryEntries.Count() - start))
                        count = collectedDictionaryEntries.Count() - start;

                    dictionaryEntries = collectedDictionaryEntries.GetRange(start, count);
                }
            }
            else
                dictionaryEntries = collectedDictionaryEntries;

            return dictionaryEntries;
        }

        // Look up all the words in a string, including possibly conjugated.
        public virtual List<DictionaryEntry> LookupDictionaryEntriesParseBest(
            string pattern,
            List<LanguageID> languageIDs)
        {
            LanguageID languageID = languageIDs.First();
            List<DictionaryEntry> dictionaryEntries = null;
            List<TextGraphNode> nodes;

            if (Parse(pattern, languageID, out nodes))
            {
                dictionaryEntries = new List<DictionaryEntry>();

                foreach (TextGraphNode node in nodes)
                {
                    DictionaryEntry dictionaryEntry = node.Entry;

                    if (dictionaryEntry != null)
                        AddUniqueEntry(dictionaryEntry, dictionaryEntries);
                    else
                        AddNotFoundEntries(languageID, node.Text, dictionaryEntries);
                }
            }

            return dictionaryEntries;
        }

        // Look up all the words in a string, including possibly conjugated.
        public virtual List<DictionaryEntry> LookupDictionaryEntriesParseAll(
            string pattern,
            List<LanguageID> languageIDs)
        {
            LanguageID languageID = languageIDs.First();
            List<DictionaryEntry> dictionaryEntries = null;
            List<TextGraphNode> nodes;

            if (ParseAll(pattern, languageID, out nodes))
            {
                dictionaryEntries = new List<DictionaryEntry>();

                foreach (TextGraphNode node in nodes)
                {
                    DictionaryEntry dictionaryEntry = node.Entry;

                    if (dictionaryEntry != null)
                        AddUniqueEntry(dictionaryEntry, dictionaryEntries);
                    else
                        AddNotFoundEntries(languageID, node.Text, dictionaryEntries);
                }
            }

            return dictionaryEntries;
        }

        // Look up exact word, possibly conjugated.
        public virtual List<DictionaryEntry> LookupDictionaryEntriesExact(
            string pattern,
            List<LanguageID> languageIDs)
        {
            List<DictionaryEntry> bestDictionaryEntries = new List<DictionaryEntry>();

            if (IsNumberString(pattern))
            {
                bestDictionaryEntries.Add(CreateNumberDictionaryEntry(languageIDs.First(), pattern));
                return bestDictionaryEntries;
            }

            if (languageIDs == null)
                return null;

#if false
            // Not needed with new deinflection scheme.
            DictionaryEntry rootlessEntry = null;

            if (LookupRootlessEntryExact(pattern, out rootlessEntry))
            {
                bestDictionaryEntries.Add(rootlessEntry);
                return bestDictionaryEntries;
            }
#endif

            List<DictionaryEntry> rawEntries = GetDictionaryLanguageEntries(pattern, languageIDs);

            if (rawEntries != null)
                bestDictionaryEntries.AddRange(rawEntries);

#if false
            int fullLength = pattern.Length;
            string subPattern = pattern;
            int subLength = subPattern.Length;
            List<DictionaryEntry> inflectionEntries;

            while (subLength >= 1)
            {
                if (subLength != fullLength)
                {
                    rawEntries = GetStemDictionaryLanguageEntries(subPattern, languageIDs);

                    foreach (DictionaryEntry rawEntry in rawEntries)
                    {
                        if (rawEntry.HasSenseWithStem())
                        {
                            int endingMaxLength = fullLength - subLength;

                            if (endingMaxLength != 0)
                            {
                                string ending = pattern.Substring(subLength, endingMaxLength);

                                if (GetPossibleInflections(
                                        rawEntry,
                                        subPattern,
                                        ending,
                                        true,
                                        out inflectionEntries))
                                    bestDictionaryEntries.AddRange(inflectionEntries);
                                else if (subLength == fullLength)
                                    bestDictionaryEntries.Add(rawEntry);
                            }
                            else if (subLength == fullLength)
                                bestDictionaryEntries.Add(rawEntry);
                        }
                        else if (subLength == fullLength)
                            bestDictionaryEntries.Add(rawEntry);
                    }
                }

                if (subLength == 1)
                    break;

                subLength--;
                subPattern = subPattern.Substring(0, subLength);
            }
#endif

            if (bestDictionaryEntries.Count() == 0)
                LookupLanguageSpecificDictionaryEntries(pattern, languageIDs, bestDictionaryEntries);

            if (bestDictionaryEntries.Count() != 0)
                LookupCacheFound.AddSingles(MatchCode.Exact, bestDictionaryEntries);

            return bestDictionaryEntries;
        }

        public virtual bool LookupLanguageSpecificDictionaryEntries(
            string pattern,
            List<LanguageID> languageIDs,
            List<DictionaryEntry> bestDictionaryEntries)
        {
            return false;
        }

        public virtual bool LookupRootlessEntryExact(
            string pattern,
            out DictionaryEntry specialEntry)
        {
            specialEntry = null;
            return false;
        }

        // Look up all the words in a string, including possibly conjugated.
        public virtual List<DictionaryEntry> LookupDictionaryEntriesMatching(
            string pattern,
            MatchCode matchType,
            List<LanguageID> languageIDs)
        {
            List<DictionaryEntry> collectedDictionaryEntries;
            LanguageID languageID;

            collectedDictionaryEntries = LookupFirstDictionaryEntries(
                pattern,
                matchType,
                languageIDs,
                0,
                0,
                out languageID);

            LookupCacheFound.AddMultiples(matchType, collectedDictionaryEntries);

            return collectedDictionaryEntries;
        }

        // Lookup first possibly conjugated word that matches the pattern.
        // If matchType is Exact, the word must match the whole string.
        public virtual DictionaryEntry LookupDictionaryEntry(
            string pattern,
            MatchCode matchType,
            List<LanguageID> languageIDs,
            List<DictionaryEntry> formAndStemDictionaryEntries,
            out bool isInflection)
        {
            DictionaryEntry dictionaryEntry = null;
            DictionaryEntry stemEntry = null;
            DictionaryEntry nonStemEntry = null;
            DictionaryEntry bestDictionaryEntry = null;
            DictionaryEntry bestStemEntry = null;
            int bestLength = 0;
            bool bestIsInflection = false;
            string subPattern = pattern;
            string inflection = String.Empty;
            int patternLength = pattern.Length;
            string remainingText;

            if (IsNumberString(pattern))
            {
                dictionaryEntry = CreateNumberDictionaryEntry(languageIDs.First(), pattern);
                isInflection = false;
                return dictionaryEntry;
            }

            if (matchType != MatchCode.Exact)
            {
                List<DictionaryEntry> dictionaryEntries;

                if (LookupCacheFound.FindMultiples(matchType, languageIDs, pattern, out dictionaryEntries))
                {
                    dictionaryEntry = dictionaryEntries.FirstOrDefault();

                    if (dictionaryEntry != null)
                        isInflection = dictionaryEntry.HasSenseWithCategory(LexicalCategory.Inflection);
                    else
                        isInflection = false;

                    return dictionaryEntry;
                }

                if (DictionaryCacheNotFound.HasMultiples(matchType, languageIDs, pattern, true))
                {
                    isInflection = false;
                    return null;
                }

                dictionaryEntries = DictionaryDatabase.Lookup(
                    pattern,
                    matchType,
                    languageIDs,
                    0,
                    0);

                if ((dictionaryEntries == null) || (dictionaryEntries.Count() == 0))
                {
                    DictionaryCacheNotFound.AddMultiples(matchType, languageIDs, pattern, null);
                    isInflection = false;
                }
                else
                {
                    DictionaryCacheFound.AddMultiples(matchType, languageIDs, pattern, dictionaryEntries);

                    dictionaryEntry = dictionaryEntries.FirstOrDefault();

                    if (dictionaryEntry != null)
                        isInflection = dictionaryEntry.HasSenseWithCategory(LexicalCategory.Inflection);
                    else
                        isInflection = false;
                }

                return dictionaryEntry;
            }

            if (LookupCacheFound.FindSingle(matchType, languageIDs, pattern, out dictionaryEntry))
            {
                isInflection = dictionaryEntry.HasSenseWithCategory(LexicalCategory.Inflection);
                return dictionaryEntry;
            }

            if (DictionaryCacheFound.FindSingle(matchType, languageIDs, pattern, out dictionaryEntry))
            {
                isInflection = dictionaryEntry.HasSenseWithCategory(LexicalCategory.Inflection);
                return dictionaryEntry;
            }

            if (LookupCacheNotFound.HasSingles(matchType, languageIDs, pattern, true))
            {
                isInflection = false;
                return null;
            }

            //if (DictionaryCacheNotFound.HasSingles(matchType, languageIDs, pattern, true))
            //{
            //    isInflection = false;
            //    return null;
            //}

            while (subPattern.Length >= 1)
            {
                if (IsNumberString(subPattern))
                {
                    dictionaryEntry = CreateNumberDictionaryEntry(languageIDs.First(), subPattern);
                    bestDictionaryEntry = dictionaryEntry;
                    bestLength = subPattern.Length;
                    bestIsInflection = false;
                    break;
                }

                /*
                dictionaryEntry = LookupFirstDictionaryEntry(
                    subPattern,
                    matchType,
                    languageIDs);
                */
                bool foundStemOrNonStem = GetStemAndNonStemDictionaryEntry(subPattern, out stemEntry, out nonStemEntry);

                if (foundStemOrNonStem)
                {
                    if (nonStemEntry != null)
                    {
                        isInflection = nonStemEntry.HasSenseWithCategory(LexicalCategory.Inflection);

                        if (isInflection || (subPattern.Length == patternLength))
                        {
                            if (subPattern.Length > bestLength)
                            {
                                bestDictionaryEntry = nonStemEntry;
                                bestLength = subPattern.Length;
                                bestIsInflection = isInflection;
                                bestStemEntry = null;
                            }

                            if (subPattern.Length == 1)
                                break;

                            subPattern = subPattern.Substring(0, subPattern.Length - 1);
                            continue;
                        }
                    }

                    if (stemEntry != null)
                    {
                        remainingText = pattern.Substring(subPattern.Length);
                        dictionaryEntry = stemEntry;

                        if (HandleStem(
                                ref dictionaryEntry,
                                remainingText,
                                true,
                                null,
                                ref inflection,
                                out isInflection) &&
                            isInflection)
                        {
                            if (matchType == MatchCode.Exact)
                            {
                                if (inflection.Length == patternLength)
                                {
                                    if (bestLength == patternLength)
                                    {
                                        DictionaryEntry newEntry = new DictionaryEntry(dictionaryEntry);
                                        newEntry.MergeEntry(bestDictionaryEntry);
                                        bestDictionaryEntry = newEntry;
                                    }
                                    else
                                    {
                                        bestDictionaryEntry = dictionaryEntry;
                                        bestLength = inflection.Length;
                                        bestIsInflection = isInflection;
                                    }

                                    bestStemEntry = stemEntry;
                                }
                            }
                            else
                            {
                                if (inflection.Length > bestLength)
                                {
                                    bestDictionaryEntry = dictionaryEntry;
                                    bestLength = inflection.Length;
                                    bestIsInflection = isInflection;
                                    bestStemEntry = stemEntry;
                                }
                            }
                        }
                        else if (nonStemEntry != null)
                        {
                            if (matchType == MatchCode.Exact)
                            {
                                if (subPattern.Length == patternLength)
                                {
                                    bestDictionaryEntry = nonStemEntry;
                                    bestLength = subPattern.Length;
                                    bestIsInflection = false;
                                    bestStemEntry = null;
                                }
                            }
                            else
                            {
                                if (subPattern.Length > bestLength)
                                {
                                    bestDictionaryEntry = nonStemEntry;
                                    bestLength = subPattern.Length;
                                    bestIsInflection = false;
                                    bestStemEntry = null;
                                }
                            }
                        }
                    }
                    else if (nonStemEntry != null)
                    {
                        if (subPattern.Length > bestLength)
                        {
                            bestDictionaryEntry = nonStemEntry;
                            bestLength = subPattern.Length;
                            bestIsInflection = false;
                            bestStemEntry = null;
                        }
                    }
                }

                if (subPattern.Length == 1)
                    break;

                subPattern = subPattern.Substring(0, subPattern.Length - 1);
            }

            if (bestDictionaryEntry != null)
            {
                if (matchType == MatchCode.Exact)
                {
                    if (bestDictionaryEntry.KeyString.Length != pattern.Length)
                    {
                        if (TextUtilities.RemoveSpaces(bestDictionaryEntry.KeyString).ToLower() != TextUtilities.RemoveSpaces(pattern).ToLower())
                        {
                            isInflection = false;
                            LookupCacheNotFound.AddSingles(
                                matchType,
                                languageIDs,
                                pattern,
                                null);
                            return null;
                        }
                    }
                }

                isInflection = bestIsInflection;
                LookupCacheFound.AddSingleCheck(
                    matchType,
                    bestDictionaryEntry.LanguageID,
                    bestDictionaryEntry.KeyString,
                    bestDictionaryEntry);

                if ((formAndStemDictionaryEntries != null) && (bestStemEntry != null))
                {
                    if (formAndStemDictionaryEntries.FirstOrDefault(x => x.MatchKey(bestStemEntry.Key)) == null)
                    {
                        formAndStemDictionaryEntries.Add(bestStemEntry);
                        GetStemDictionaryFormEntry(bestStemEntry, formAndStemDictionaryEntries);
                    }
                }
            }
            else
            {
                LookupCacheNotFound.AddSingles(
                    matchType,
                    languageIDs,
                    pattern,
                    null);
                isInflection = false;
            }

            return bestDictionaryEntry;
        }

        public virtual DictionaryEntry LookupDictionaryEntry(
            string pattern,
            MatchCode matchType,
            LanguageID languageID,
            List<DictionaryEntry> formAndStemDictionaryEntries,
            out bool isInflection)
        {
            DictionaryEntry dictionaryEntry = null;
            DictionaryEntry bestDictionaryEntry = null;
            DictionaryEntry bestStemEntry = null;
            bool bestIsInflection = false;
            string subPattern = pattern;
            string inflection = String.Empty;
            int patternLength = pattern.Length;
            string remainingText;

            if (IsNumberString(pattern))
            {
                dictionaryEntry = CreateNumberDictionaryEntry(languageID, pattern);
                isInflection = false;
                return dictionaryEntry;
            }

            if (matchType != MatchCode.Exact)
            {
                List<DictionaryEntry> dictionaryEntries;

                if (DictionaryCacheFound.FindMultiple(matchType, languageID, pattern, out dictionaryEntries))
                {
                    dictionaryEntry = dictionaryEntries.FirstOrDefault();

                    if (dictionaryEntry != null)
                        isInflection = dictionaryEntry.HasSenseWithCategory(LexicalCategory.Inflection);
                    else
                        isInflection = false;

                    return dictionaryEntry;
                }

                if (DictionaryCacheNotFound.HasMultiple(matchType, languageID, pattern))
                {
                    isInflection = false;
                    return null;
                }

                dictionaryEntries = DictionaryDatabase.Lookup(
                    pattern,
                    matchType,
                    languageID,
                    0,
                    0);

                if ((dictionaryEntries == null) || (dictionaryEntries.Count() == 0))
                {
                    DictionaryCacheNotFound.AddMultiple(matchType, languageID, pattern, dictionaryEntries);
                    isInflection = false;
                }
                else
                {
                    DictionaryCacheFound.AddMultiple(matchType, languageID, pattern, dictionaryEntries);

                    dictionaryEntry = dictionaryEntries.FirstOrDefault();

                    if (dictionaryEntry != null)
                        isInflection = dictionaryEntry.HasSenseWithCategory(LexicalCategory.Inflection);
                    else
                        isInflection = false;
                }

                return dictionaryEntry;
            }

            if (LookupCacheFound.FindSingle(matchType, languageID, pattern, out dictionaryEntry))
            {
                isInflection = dictionaryEntry.HasSenseWithCategory(LexicalCategory.Inflection);
                return dictionaryEntry;
            }

            if (LookupCacheNotFound.HasSingle(matchType, languageID, pattern))
            {
                isInflection = false;
                return null;
            }

            while (subPattern.Length >= 1)
            {
                if (IsNumberString(subPattern))
                {
                    dictionaryEntry = CreateNumberDictionaryEntry(languageID, subPattern);
                    bestDictionaryEntry = dictionaryEntry;
                    bestStemEntry = null;
                    break;
                }

                dictionaryEntry = LookupFirstDictionaryEntry(
                    subPattern,
                    matchType,
                    languageID);

                if (dictionaryEntry != null)
                {
                    DictionaryEntry maybeStem = dictionaryEntry;

                    if (subPattern.Length != patternLength)
                    {
                        remainingText = pattern.Substring(subPattern.Length);
                        HandleStem(
                            ref dictionaryEntry,
                            remainingText,
                            true,
                            null,
                            ref inflection,
                            out isInflection);
                    }
                    else
                        isInflection = false;

                    if (bestDictionaryEntry == null)
                    {
                        bestDictionaryEntry = dictionaryEntry;
                        bestIsInflection = isInflection;

                        if (isInflection)
                            bestStemEntry = maybeStem;
                    }
                    else if (dictionaryEntry.KeyString.Length > bestDictionaryEntry.KeyString.Length)
                    {
                        bestDictionaryEntry = dictionaryEntry;
                        bestIsInflection = isInflection;

                        if (isInflection)
                            bestStemEntry = maybeStem;
                    }
                }

                if (subPattern.Length == 1)
                    break;

                subPattern = subPattern.Substring(0, subPattern.Length - 1);
            }

            isInflection = bestIsInflection;

            if (bestDictionaryEntry != null)
            {
                LookupCacheFound.AddSingleCheck(
                    matchType,
                    bestDictionaryEntry.LanguageID,
                    bestDictionaryEntry.KeyString,
                    bestDictionaryEntry);

                if ((formAndStemDictionaryEntries != null) && (bestStemEntry != null))
                {
                    if (formAndStemDictionaryEntries.FirstOrDefault(x => x.MatchKey(bestStemEntry.Key)) == null)
                    {
                        formAndStemDictionaryEntries.Add(bestStemEntry);
                        GetStemDictionaryFormEntry(bestStemEntry, formAndStemDictionaryEntries);
                    }
                }
            }
            else
                LookupCacheNotFound.AddSingleCheck(
                    matchType,
                    languageID,
                    pattern,
                    null);

            return bestDictionaryEntry;
        }

        public virtual DictionaryEntry LookupDictionaryFormEntry(Sense stemSense, out int dictionaryFormSenseIndex)
        {
            LanguageSynonyms languageSynonyms = stemSense.GetLanguageSynonyms(LanguageID);

            dictionaryFormSenseIndex = 0;

            if (languageSynonyms == null)
                return null;

            string dictionaryForm = languageSynonyms.GetSynonymIndexed(0);

            DictionaryEntry dictionaryFormEntry = GetDictionaryEntry(dictionaryForm);

            if (dictionaryFormEntry != null)
            {
                if (!String.IsNullOrEmpty(stemSense.CategoryString))
                {
                    foreach (Sense sense in dictionaryFormEntry.Senses)
                    {
                        if (sense.CategoryString == stemSense.CategoryString)
                            return dictionaryFormEntry;
                        else if (sense.CategoryString.StartsWith(stemSense.CategoryString) &&
                                (sense.CategoryString[stemSense.CategoryString.Length] == ','))
                            return dictionaryFormEntry;

                        dictionaryFormSenseIndex++;
                    }
                }
                else if (stemSense.Category != LexicalCategory.Unknown)
                {
                    foreach (Sense sense in dictionaryFormEntry.Senses)
                    {
                        if (sense.Category == stemSense.Category)
                            return dictionaryFormEntry;

                        dictionaryFormSenseIndex++;
                    }
                }
            }

            return null;
        }

        public void AddNotFoundEntries(LanguageID languageID, string text, List<DictionaryEntry> collectedDictionaryEntries)
        {
            if (languageID == null)
                languageID = TargetLanguageIDs.First();

            DictionaryEntry dictionaryEntry = new DictionaryEntry(text, languageID);

            if (TargetLanguageIDs != null)
            {
                foreach (LanguageID otherLanguageID in TargetLanguageIDs)
                {
                    if (otherLanguageID == languageID)
                        continue;
                    else if (!LanguageLookup.IsAlternateOfLanguageID(otherLanguageID, languageID))
                        continue;

                    LanguageString alternate = new LanguageString(0, otherLanguageID, "(not found)");
                    dictionaryEntry.AddAlternate(alternate);
                }

                if (HostLanguageIDs != null)
                {
                    foreach (LanguageID otherLanguageID in HostLanguageIDs)
                    {
                        ProbableMeaning probableSynonym = new ProbableMeaning(
                            "(not found)",
                            LexicalCategory.Unknown,
                            null,
                            float.NaN,
                            0,
                            0);
                        List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>() { probableSynonym };
                        LanguageSynonyms languageSynonyms = new LanguageSynonyms(otherLanguageID, probableSynonyms);
                        List<LanguageSynonyms> languageSynonymsList = new List<LanguageSynonyms>() { languageSynonyms };
                        Sense sense = new Sense(0, LexicalCategory.NotFound, null, 0, languageSynonymsList, null);
                        dictionaryEntry.AddSense(sense);
                    }
                }
            }
            else if (UserLanguageIDs != null)
            {
                foreach (LanguageID otherLanguageID in UserLanguageIDs)
                {
                    ProbableMeaning probableSynonym = new ProbableMeaning(
                        "(not found)",
                        LexicalCategory.Unknown,
                        null,
                        float.NaN,
                        0,
                        0);
                    List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>() { probableSynonym };
                    LanguageSynonyms languageSynonyms = new LanguageSynonyms(otherLanguageID, probableSynonyms);
                    List<LanguageSynonyms> languageSynonymsList = new List<LanguageSynonyms>() { languageSynonyms };
                    Sense sense = new Sense(0, LexicalCategory.NotFound, null, 0, languageSynonymsList, null);
                    dictionaryEntry.AddSense(sense);
                }
            }

            AddUniqueEntry(dictionaryEntry, collectedDictionaryEntries);
        }

        public static void AddUniqueEntries(List<DictionaryEntry> dictionaryEntries, List<DictionaryEntry> collectedDictionaryEntries)
        {
            if (dictionaryEntries == null)
                return;
            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
                AddUniqueEntry(dictionaryEntry, collectedDictionaryEntries);
        }

        public static void AddUniqueEntry(DictionaryEntry dictionaryEntry, List<DictionaryEntry> collectedDictionaryEntries)
        {
            if (dictionaryEntry == null)
                return;
            if (collectedDictionaryEntries == null)
                return;
            if (collectedDictionaryEntries.Count() == 0)
                collectedDictionaryEntries.Add(dictionaryEntry);
            else if (collectedDictionaryEntries.FirstOrDefault(x => DictionaryEntry.Compare(x, dictionaryEntry) == 0) == null)
                collectedDictionaryEntries.Add(dictionaryEntry);
        }

        public InflectionOutputMode DefaultInflectionOutputMode
        {
            get
            {
                return _DefaultInflectionOutputMode;
            }
            set
            {
                _DefaultInflectionOutputMode = value;
            }
        }

        public static string InflectionDictionarySourceName = "Inflection";

        private static int _InflectionDictionarySourceID = 0;
        public static int InflectionDictionarySourceID
        {
            get
            {
                if (_InflectionDictionarySourceID == 0)
                    _InflectionDictionarySourceID = ApplicationData.DictionarySourcesLazy.Add(InflectionDictionarySourceName);
                return _InflectionDictionarySourceID;
            }
        }

        private static List<int> _InflectionDictionarySourceIDList = null;
        public static List<int> InflectionDictionarySourceIDList
        {
            get
            {
                if (_InflectionDictionarySourceIDList == null)
                    _InflectionDictionarySourceIDList = new List<int>(1) { InflectionDictionarySourceID };

                return _InflectionDictionarySourceIDList;
            }
        }

        public virtual DictionaryEntry CreateInflectionDictionaryEntry(
            string inflectedForm,
            LanguageID languageID,
            Inflection inflection,
            InflectionOutputMode inflectionOutputMode)
        {
            string targetText;

            targetText = inflectedForm;

            DictionaryEntry inputEntry = inflection.Input;
            List<LanguageString> alternates = null;
            List<LanguageSynonyms> languageSynonymsList = new List<LanguageSynonyms>();
            MultiLanguageString dictionaryForm = inflection.DictionaryForm;
            DictionaryEntry dictionaryEntry = new DictionaryEntry(targetText, languageID);
            LexicalCategory category = LexicalCategory.Inflection;
            string categoryString = inflection.CategoryString /*+ "," + inflection.Designation.Label*/;
            bool isMultiple = inflection.GetOutput(languageID).Contains("/");

            foreach (LanguageID targetLanguageID in TargetLanguageIDs)
            {
                if (targetLanguageID != languageID)
                {
                    if (alternates == null)
                        alternates = new List<LanguageString>();

                    string alternateText;

                    if (inflection.GetOutput(languageID) == inflectedForm)
                        alternateText = inflection.GetOutput(targetLanguageID);
                    else if (inflection.GetPronounOutput(languageID) == inflectedForm)
                        alternateText = inflection.GetPronounOutput(targetLanguageID);
                    else if (inflection.GetMainWordPlusPrePostWords(languageID) == inflectedForm)
                        alternateText = inflection.GetMainWordPlusPrePostWords(targetLanguageID);
                    else if (inflection.GetMainWord(languageID) == inflectedForm)
                        alternateText = inflection.GetMainWord(targetLanguageID);
                    else
                    {
                        if (inflection.HasAnyContractedOutput())
                        {
                            MultiLanguageString mls = inflection.FindContractedOutput(inflectedForm, languageID);
                            if (mls != null)
                                alternateText = mls.Text(targetLanguageID);
                            else
                                alternateText = inflection.GetMainWordPlusPrePostWords(targetLanguageID);
                        }
                        else
                            alternateText = inflection.GetMainWordPlusPrePostWords(targetLanguageID);
                    }

                    if (isMultiple)
                    {
                        string[] alternateTexts = alternateText.Split(LanguageLookup.Slash, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string alternateStr in alternateTexts)
                            alternates.Add(
                                new LanguageString(
                                    0,
                                    targetLanguageID,
                                    alternateStr));
                    }
                    else
                        alternates.Add(
                            new LanguageString(
                                0,
                                targetLanguageID,
                                alternateText));
                }
            }

            if (alternates != null)
                dictionaryEntry.Alternates = alternates;

            List<LanguageID> hostLanguageIDs;
            List<int> hostSourceIDs = null;

            if (MultiTool != null)
            {
                hostLanguageIDs = MultiTool.HostLanguageIDs;

                if (hostLanguageIDs.Count() != 0)
                {
                    LanguageID hostLanguageID = hostLanguageIDs.First();
                    string hostDictionaryform = dictionaryForm.Text(hostLanguageID);
                    ProbableMeaning hostDictionaryFormMeaning;
                    int synonymIndex;
                    if (inputEntry.GetProbableSynonymContainingText(
                            hostLanguageID,
                            hostDictionaryform,
                            out synonymIndex,
                            out hostDictionaryFormMeaning))
                    {
                        if (hostDictionaryFormMeaning.HasSourceIDs())
                            hostSourceIDs = new List<int>(hostDictionaryFormMeaning.SourceIDs);
                        else
                            hostSourceIDs = new List<int>(1) { InflectionDictionarySourceID };
                    }
                    else
                        hostSourceIDs = new List<int>(1) { InflectionDictionarySourceID };
                }
                else
                    hostSourceIDs = new List<int>(1) { InflectionDictionarySourceID };
            }
            else
            {
                hostLanguageIDs = new List<LanguageID>();
                hostSourceIDs = new List<int>(1) { InflectionDictionarySourceID };
            }

            dictionaryEntry.SourceIDs = hostSourceIDs;

            foreach (LanguageID hostLanguageID in hostLanguageIDs)
            {
                List<string> hostTexts = inflection.GetMultipleUniqueOutputs(
                    hostLanguageID,
                    inflectionOutputMode);
                List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>();

                foreach (string hostText in hostTexts)
                {
                    if (isMultiple)
                    {
                        string[] texts = hostText.Split(LanguageLookup.Slash, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string text in texts)
                        {
                            ProbableMeaning probableSynonym = new ProbableMeaning(
                                text,
                                category,
                                categoryString,
                                float.NaN,
                                0,
                                hostSourceIDs);
                            probableSynonym.AddInflection(inflection);
                            probableSynonyms.Add(probableSynonym);
                        }
                    }
                    else
                    {
                        ProbableMeaning probableSynonym = new ProbableMeaning(
                            hostText,
                            category,
                            categoryString,
                            float.NaN,
                            0,
                            hostSourceIDs);
                        probableSynonym.AddInflection(inflection);
                        probableSynonyms.Add(probableSynonym);
                    }
                }

                LanguageSynonyms languageSynonyms = new LanguageSynonyms(hostLanguageID, probableSynonyms);
                languageSynonymsList.Add(languageSynonyms);
            }

            foreach (LanguageID tid in TargetLanguageIDs)
            {
                if (hostLanguageIDs.Contains(tid))
                    continue;

                ProbableMeaning probableSynonym = new ProbableMeaning(
                    dictionaryForm.Text(tid),
                    inflection.Category,
                    inflection.CategoryString + ",dictionary",
                    float.NaN,
                    0,
                    hostSourceIDs);
                List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>() { probableSynonym };
                LanguageSynonyms languageSynonyms = new LanguageSynonyms(tid, probableSynonyms);
                languageSynonymsList.Add(languageSynonyms);
            }

            Sense sense = new Sense(
                0,
                category,
                categoryString,
                0,
                languageSynonymsList,
                null);
            sense.AddInflection(inflection);
            dictionaryEntry.AddSense(sense);

            return dictionaryEntry;
        }

        public virtual bool GetSenseContainingText(
            DictionaryEntry dictionaryEntry,
            LanguageID languageID,
            string containingText,
            int senseIndex,
            int synonymIndex,
            out Sense sense,
            out LexicalCategory category,
            out string definition)
        {
            sense = null;
            category = LexicalCategory.Unknown;
            definition = String.Empty;

            if (dictionaryEntry == null)
                return false;

            sense = dictionaryEntry.GetSenseIndexed(senseIndex);

            if (sense == null)
            {
                if (dictionaryEntry.GetSenseContainingText(
                        languageID,
                        containingText,
                        out sense,
                        out senseIndex,
                        out synonymIndex,
                        out definition))
                {
                    category = sense.Category;
                    return true;
                }
            }
            else if (synonymIndex == -1)
            {
                category = sense.Category;

                if (sense.GetSynonymContainingText(languageID, containingText, out synonymIndex, out definition))
                    return true;
            }
            else
            {
                LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(languageID);

                category = sense.Category;

                if (languageSynonyms != null)
                {
                    if (languageSynonyms.GetSynonymContainingText(containingText, out synonymIndex, out definition))
                        return true;
                }
            }

            return false;
        }

        public virtual bool GetSenseCategoryDefinition(
            DictionaryEntry dictionaryEntry,
            LanguageID languageID,
            ref int senseIndex,
            ref int synonymIndex,
            out Sense sense,
            out LexicalCategory category,
            out string definition)
        {
            sense = null;
            category = LexicalCategory.Unknown;
            definition = String.Empty;

            if (dictionaryEntry == null)
                return false;

            if (senseIndex == -1)
            {
                senseIndex = 0;

                foreach (Sense s in dictionaryEntry.Senses)
                {
                    LexicalCategory cat = s.Category;

                    if (CanInflectCategory(cat))
                        sense = s;

                    if (sense != null)
                        break;

                    senseIndex++;
                }
            }
            else
                sense = dictionaryEntry.GetSenseIndexed(senseIndex);

            if ((senseIndex == -1) || (senseIndex == dictionaryEntry.SenseCount))
                senseIndex = 0;

            if (sense == null)
            {
                sense = dictionaryEntry.GetSenseWithLanguageID(languageID);

                if (sense == null)
                {
                    if (dictionaryEntry.LanguageID.LanguageCode != languageID.LanguageCode)
                        return false;

                    sense = dictionaryEntry.GetSenseIndexed(senseIndex);

                    if (sense == null)
                        return false;
                }
                else
                    senseIndex = dictionaryEntry.Senses.IndexOf(sense);
            }

            category = sense.Category;

            if (synonymIndex == -1)
                synonymIndex = 0;

            if (dictionaryEntry.LanguageID.LanguageCode == languageID.LanguageCode)
            {
                definition = dictionaryEntry.GetDefinition(languageID, false, false);

                if (!String.IsNullOrEmpty(definition))
                    return true;
            }
            else
            {
                LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(languageID);

                if (languageSynonyms != null)
                {
                    definition = languageSynonyms.GetSynonymIndexed(synonymIndex);

                    if (!String.IsNullOrEmpty(definition))
                        return true;
                }
            }

            return false;
        }

        public virtual bool InferCategoryFromWord(
            string word,
            out LexicalCategory category,
            out string categoryString)
        {
            category = LexicalCategory.Unknown;
            categoryString = null;
            return false;
        }

        public virtual string DecodeCategoryString(string categoryString)
        {
            return categoryString;
        }

        public virtual string DecodeCategorySegment(string categorySegment)
        {
            return categorySegment;
        }

        public virtual void StemSubstitutionCheck(ref DictionaryEntry dictionaryEntry, ref Sense sense, ref int senseIndex)
        {
            if (senseIndex < 0)
                senseIndex = 0;

            sense = dictionaryEntry.GetSenseIndexed(senseIndex);

            if ((sense != null) && ((sense.Category == LexicalCategory.Stem) || (sense.Category == LexicalCategory.IrregularStem)))
            {
                string nonStemKey = sense.GetDefinition(LanguageID, false, false);
                DictionaryEntry nonStemEntry = GetDictionaryEntry(nonStemKey);

                if (nonStemEntry != null)
                    dictionaryEntry = nonStemEntry;
            }
        }

        public virtual bool PopulateDeinflectionRepository()
        {
            List<DictionaryEntry> dictionaryEntries = ApplicationData.Repositories.Dictionary.GetAll(LanguageID);
            bool returnValue = true;

            if (dictionaryEntries == null)
                return false;

            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
            {
                if (dictionaryEntry.SenseCount == 0)
                    continue;

                HashSet<string> done = new HashSet<string>();
                string dictionaryForm = dictionaryEntry.KeyString;
                int senseIndex;
                int senseCount = dictionaryEntry.SenseCount;
                int synonymIndex = 0;
                Sense sense;
                string hash;

                for (senseIndex = 0; senseIndex < senseCount; senseIndex++)
                {
                    sense = dictionaryEntry.GetSenseIndexed(senseIndex);

                    if (!CanInflect(sense.Category.ToString()))
                        continue;

                    hash = sense.Category.ToString();

                    if (!String.IsNullOrEmpty(sense.CategoryString))
                        hash += "|" + sense.CategoryString;

                    if (!done.Add(hash))
                        continue;

                    if (!PopulateDeinflectionRepository(dictionaryEntry, senseIndex, synonymIndex))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public virtual bool PopulateDeinflectionRepository(DictionaryEntry dictionaryEntry, int senseIndex, int synonymIndex)
        {
            List<Inflection> inflections = InflectAnyDictionaryFormAll(dictionaryEntry, ref senseIndex, ref synonymIndex);

            if ((inflections == null) || (inflections.Count() == 0))
                return false;

            string dictionaryForm = dictionaryEntry.KeyString;
            Sense sense = dictionaryEntry.GetSenseIndexed(senseIndex);

            if (sense == null)
                return false;

            LexicalCategory category = sense.Category;
            string categoryString = sense.CategoryString;

            if (categoryString == null)
                categoryString = String.Empty;

            foreach (Inflection inflection in inflections)
            {
                string inflectionLabel = inflection.Label;
                int inflectionLabelID = ApplicationData.InflectionLabelsLazy.GetOrAdd(inflectionLabel);

                //if (inflectionLabel.Contains("Negative"))
                //    ApplicationData.Global.PutConsoleMessage("Label contains negative.");

                foreach (LanguageID languageID in TargetLanguageIDs)
                {
                    string mainInflectionWord = inflection.GetMainWordPlusPrePostWords(languageID);

                    if (String.IsNullOrEmpty(mainInflectionWord))
                        continue;

                    Deinflection deinflection = DeinflectionDatabase.Get(mainInflectionWord, languageID);
                    DeinflectionInstance deinflectionInstance = new DeinflectionInstance(
                        dictionaryForm,
                        category,
                        categoryString,
                        inflectionLabelID);

                    if (deinflection != null)
                    {
                        if (!deinflection.HasInstance(deinflectionInstance))
                        {
                            deinflection.AddInstance(deinflectionInstance);

                            if (!DeinflectionDatabase.Update(deinflection, languageID))
                                return false;
                        }
                    }
                    else
                    {
                        deinflection = new Deinflection(mainInflectionWord, deinflectionInstance);

                        if (!DeinflectionDatabase.Add(deinflection, languageID))
                            return false;
                    }
                }
            }

            return true;
        }

        public virtual int GetWordFrequency(
            string word,
            string type)    // "Lemma", "Inflection"
        {
            foreach (LanguageID languageID in TargetLanguageIDs)
            {
                if (!HasLanguageFrequencyTable(languageID))
                    continue;

                FrequencyTable table = GetFrequencyTable(languageID, type);
                int testFrequency = table.GetWordFrequency(word);

                if (testFrequency == 0)
                {
                    string normalized = GetNormalizedText(word);

                    if (normalized != word)
                        testFrequency = table.GetWordFrequency(normalized);

                    if (testFrequency == 0)
                    {
                        string frequencyNormalized = GetFrequencyNormalizedText(normalized);

                        if (frequencyNormalized != normalized)
                            testFrequency = table.GetWordFrequency(frequencyNormalized);
                    }
                }

                if (testFrequency != 0)
                    return testFrequency;
            }

            return 0;
        }

        public virtual int GetLowestPhraseWordFrequency(
            string wordOrPhrase,
            string type)    // "Lemma", "Inflection"
        {
            int bestFreq;

            if (!HasFrequencyTable())
                return 0;

            if (wordOrPhrase.Contains(" "))
            {
                string[] words = wordOrPhrase.Split(LanguageLookup.Space, StringSplitOptions.RemoveEmptyEntries);

                if (words.Length != 0)
                {
                    bestFreq = int.MaxValue;

                    foreach (string word in words)
                    {
                        int freq = GetWordFrequency(word, type);

                        if (freq == 0)
                            return 0;

                        if (freq < bestFreq)
                            bestFreq = freq;
                    }

                    if (bestFreq == int.MaxValue)
                        bestFreq = 0;
                }
                else
                    bestFreq = 0;
            }
            else
                bestFreq = GetWordFrequency(wordOrPhrase, type);

            return bestFreq;
        }

        public static FrequencyTable GetFrequencyTable(
            LanguageID languageID,
            string type)    // "Lemma", "Inflection"
        {
            string key = FrequencyTable.ComposeKey(languageID, type);
            FrequencyTable table = null;

            if (_FrequencyTableCache == null)
                _FrequencyTableCache = new Dictionary<string, FrequencyTable>();

            lock (_FrequencyTableCache)
            {
                if (_FrequencyTableCache.TryGetValue(key, out table))
                    return table;

                table = new FrequencyTable(languageID, type);

                _FrequencyTableCache.Add(key, table);
            }

            return table;
        }

        public bool HasLanguageFrequencyTable(LanguageID languageID)
        {
            string suffix = languageID.SymbolName;
            return HasCapability("HasFrequencyTable" + suffix);
        }

        public void SetHasLanguageFrequencyTable(LanguageID languageID, bool state)
        {
            string suffix = languageID.SymbolName;
            SetCapability("HasFrequencyTable" + suffix, state);
        }

        public bool HasFrequencyTable()
        {
            string suffix = LanguageID.SymbolName;
            return HasCapability("HasFrequencyTable" + suffix);
        }

        public void SetHasFrequencyTable(bool state)
        {
            string suffix = LanguageID.SymbolName;
            SetCapability("HasFrequencyTable" + suffix, state);
        }

        public virtual void ClearDictionaryCaches()
        {
            _DictionaryCacheFound = null;
            _DictionaryCacheNotFound = null;
            _LookupCacheFound = null;
            _LookupCacheNotFound = null;

            _DictionaryStemsCacheFound = null;
            _DictionaryStemsCacheNotFound = null;
            _LookupStemsCacheFound = null;
            _LookupStemsCacheNotFound = null;

            _DeinflectionCache = null;
            _ContainingPhrasesCache = null;
        }

        public virtual List<DictionaryEntry> GetCachedLookupEntriesFound()
        {
            List<DictionaryEntry> dictionaryEntriesFound = null;
            LookupCacheFound.GetAllSingles(MatchCode.Exact, TargetLanguageIDs, ref dictionaryEntriesFound);
            return dictionaryEntriesFound;
        }

        public virtual List<string> GetCachedLookupKeysNotFound()
        {
            List<string> dictionaryKeysNotFound = null;
            LookupCacheNotFound.GetAllSinglesKeys(MatchCode.Exact, TargetLanguageIDs, ref dictionaryKeysNotFound);
            return dictionaryKeysNotFound;
        }

        public virtual List<DictionaryEntry> GetCachedDictionaryEntriesFound()
        {
            List<DictionaryEntry> dictionaryEntriesFound = null;
            DictionaryCacheFound.GetAllSingles(MatchCode.Exact, TargetLanguageIDs, ref dictionaryEntriesFound);
            return dictionaryEntriesFound;
        }

        public virtual List<string> GetCachedDictionaryKeysNotFound()
        {
            List<string> dictionaryKeysNotFound = null;
            DictionaryCacheNotFound.GetAllSinglesKeys(MatchCode.Exact, TargetLanguageIDs, ref dictionaryKeysNotFound);
            return dictionaryKeysNotFound;
        }

        public virtual void DictionaryEntryAdded(DictionaryEntry dictionaryEntry)
        {
        }

        public virtual void DictionaryEntryUpdated(DictionaryEntry dictionaryEntry, DictionaryEntry originalEntry)
        {
        }

        public virtual void SaveDictionaryEntriesAddedAndUpdated()
        {
        }

        protected static string GetDictionarySourceDirectoryFilePath(LanguageID languageID)
        {
            string returnValue = MediaUtilities.ConcatenateFilePath(
                ApplicationData.ContentPath,
                "Dictionary");
            returnValue = MediaUtilities.ConcatenateFilePath(
                returnValue,
                "Source");
            returnValue = MediaUtilities.ConcatenateFilePath(
                returnValue,
                languageID.LanguageCode);
            return returnValue;
        }

        public virtual bool LookupDictionaryEntriesFilterCheck(string pattern, LanguageID languageID)
        {
            return true;
        }

        public virtual bool GetLongestLengths(
            LanguageID languageID,
            int longestDictionaryEntryLength,
            ref int longestPrefixLength,
            ref int longestSuffixLength,
            ref int longestInflectionLength)
        {
            return false;
        }

        public DictionaryRepository DictionaryDatabase
        {
            get
            {
                if (_DictionaryDatabase == null)
                    _DictionaryDatabase = ApplicationData.Repositories.Dictionary;
                return _DictionaryDatabase;
            }
            set
            {
                _DictionaryDatabase = value;
            }
        }

        public DeinflectionRepository DeinflectionDatabase
        {
            get
            {
                if (_DeinflectionDatabase == null)
                {
                    if (ApplicationData.IsMobileVersion && (ApplicationData.RemoteRepositories.Deinflections != null))
                        _DeinflectionDatabase = ApplicationData.RemoteRepositories.Deinflections;
                    else
                        _DeinflectionDatabase = ApplicationData.Repositories.Deinflections;
                }

                return _DeinflectionDatabase;
            }
            set
            {
                _DeinflectionDatabase = value;
            }
        }

        public DictionaryCache DictionaryCacheFound
        {
            get
            {
                if (_DictionaryCacheFound == null)
                    _DictionaryCacheFound = new DictionaryCache();
                return _DictionaryCacheFound;
            }
            set
            {
                _DictionaryCacheFound = value;
            }
        }

        public DictionaryCache DictionaryCacheNotFound
        {
            get
            {
                if (_DictionaryCacheNotFound == null)
                    _DictionaryCacheNotFound = new DictionaryCache();
                return _DictionaryCacheNotFound;
            }
            set
            {
                _DictionaryCacheNotFound = value;
            }
        }

        public DictionaryCache LookupCacheFound
        {
            get
            {
                if (_LookupCacheFound == null)
                    _LookupCacheFound = new DictionaryCache();
                return _LookupCacheFound;
            }
            set
            {
                _LookupCacheFound = value;
            }
        }

        public DictionaryCache LookupCacheNotFound
        {
            get
            {
                if (_LookupCacheNotFound == null)
                    _LookupCacheNotFound = new DictionaryCache();
                return _LookupCacheNotFound;
            }
            set
            {
                _LookupCacheNotFound = value;
            }
        }

        public DictionaryRepository DictionaryStemsDatabase
        {
            get
            {
                if (_DictionaryStemsDatabase == null)
                    _DictionaryStemsDatabase = ApplicationData.Repositories.DictionaryStems;
                return _DictionaryStemsDatabase;
            }
            set
            {
                _DictionaryStemsDatabase = value;
            }
        }

        public DictionaryCache DictionaryStemsCacheFound
        {
            get
            {
                if (_DictionaryStemsCacheFound == null)
                    _DictionaryStemsCacheFound = new DictionaryCache();
                return _DictionaryStemsCacheFound;
            }
            set
            {
                _DictionaryStemsCacheFound = value;
            }
        }

        public DictionaryCache DictionaryStemsCacheNotFound
        {
            get
            {
                if (_DictionaryStemsCacheNotFound == null)
                    _DictionaryStemsCacheNotFound = new DictionaryCache();
                return _DictionaryStemsCacheNotFound;
            }
            set
            {
                _DictionaryStemsCacheNotFound = value;
            }
        }

        public DictionaryCache LookupStemsCacheFound
        {
            get
            {
                if (_LookupStemsCacheFound == null)
                    _LookupStemsCacheFound = new DictionaryCache();
                return _LookupStemsCacheFound;
            }
            set
            {
                _LookupStemsCacheFound = value;
            }
        }

        public DictionaryCache LookupStemsCacheNotFound
        {
            get
            {
                if (_LookupStemsCacheNotFound == null)
                    _LookupStemsCacheNotFound = new DictionaryCache();
                return _LookupStemsCacheNotFound;
            }
            set
            {
                _LookupStemsCacheNotFound = value;
            }
        }

        public Dictionary<string, Deinflection> DeinflectionCache
        {
            get
            {
                if (_DeinflectionCache == null)
                    _DeinflectionCache = new Dictionary<string, Deinflection>();
                return _DeinflectionCache;
            }
            set
            {
                _DeinflectionCache = value;
            }
        }
    }
}
