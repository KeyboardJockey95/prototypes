using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Node
{
    public delegate List<DictionaryEntry> LookupDictionaryEntriesDelegate(
            string pattern,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs);

    public partial class NodeUtilities : ControllerUtilities
    {
        // This function caches remote dictionary entries found.
        // Be sure to flush the cache later on.
        public List<DictionaryEntry> LookupLocalOrRemoteDictionaryEntries(
            string pattern,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs)
        {
            LanguageID targetLanguageID = targetLanguageIDs.FirstOrDefault();
            LanguageTool tool = GetLanguageTool(targetLanguageID);

            if ((hostLanguageIDs != null) && (hostLanguageIDs.Count() != 0) && (targetLanguageID != null))
            {
                MultiLanguageTool multiTool = GetMultiLanguageTool(targetLanguageID, hostLanguageIDs);

                if ((multiTool != null) && (multiTool.LanguageToolCount() > 1))
                    tool.MultiTool = multiTool;
            }

            List<DictionaryEntry> dictionaryEntries = null;
            DictionaryEntry dictionaryEntry;
            bool isInflection = false;

            if (tool != null)
            {
#if false
                dictionaryEntries = tool.LookupDictionaryEntriesExact(
                    pattern,
                    targetLanguageIDs);
#else
                if (pattern == "いのろう")
                    ApplicationData.Global.PutConsoleMessage("いのろう");

                dictionaryEntry = tool.LookupDictionaryEntry(
                    pattern,
                    MatchCode.Exact,
                    targetLanguageIDs,
                    null,
                    out isInflection);

                if (dictionaryEntry != null)
                    dictionaryEntries = new List<DictionaryEntry>(1) { dictionaryEntry };
#endif
            }
            else
                dictionaryEntries = Repositories.Dictionary.Lookup(
                    pattern,
                    MatchCode.Exact,
                    targetLanguageIDs,
                    0,
                    0);

            if ((dictionaryEntries == null) || (dictionaryEntries.Count() == 0))
            {
                if (ApplicationData.IsMobileVersion && ApplicationData.Global.IsConnectedToANetwork())
                {
                    if (RemoteRepositories != null)
                    {
                        dictionaryEntry = GetCachedRemoteDictionaryEntry(pattern, targetLanguageIDs);

                        if (dictionaryEntry == null)
                        {
                            LanguageTool remoteTool = GetLanguageToolForRemote(targetLanguageID);

                            if (tool != null)
                            {
                                dictionaryEntry = remoteTool.LookupDictionaryEntry(
                                    pattern,
                                    MatchCode.Exact,
                                    targetLanguageIDs,
                                    null,
                                    out isInflection);

                                if (dictionaryEntry != null)
                                {
                                    dictionaryEntries = new List<DictionaryEntry>(1) { dictionaryEntry };
                                    AddCachedRemoteLanguageDictionaryEntry(dictionaryEntry, tool);
                                }
                            }
                            else
                            {
                                dictionaryEntries = RemoteRepositories.Dictionary.Lookup(
                                    pattern,
                                    JTLanguageModelsPortable.Matchers.MatchCode.Exact,
                                    targetLanguageIDs,
                                    0,
                                    0);
                                AddCachedRemoteLanguageDictionaryEntries(dictionaryEntries, tool);
                            }
                        }
                    }
                }
            }

            return dictionaryEntries;
        }

        public DictionaryEntry GetCachedRemoteDictionaryEntry(string key, List<LanguageID> languageIDs)
        {
            foreach (LanguageID languageID in languageIDs)
            {
                DictionaryEntry dictionaryEntry = GetCachedRemoteLanguageDictionaryEntry(key, languageID);

                if (dictionaryEntry != null)
                    return dictionaryEntry;
            }

            return null;
        }

        public DictionaryEntry GetCachedRemoteLanguageDictionaryEntry(string key, LanguageID languageID)
        {
            DictionaryEntry dictionaryEntry;

            if (RemoteDictionaryEntryCache.FindSingle(MatchCode.Exact, languageID, key, out dictionaryEntry))
                return dictionaryEntry;

            return null;
        }

        public void AddCachedRemoteLanguageDictionaryEntries(
            List<DictionaryEntry> dictionaryEntries,
            LanguageTool tool)
        {
            if (dictionaryEntries == null)
                return;

            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
                AddCachedRemoteLanguageDictionaryEntry(dictionaryEntry, tool);
        }

        public void AddCachedRemoteLanguageDictionaryEntry(
            DictionaryEntry dictionaryEntry,
            LanguageTool tool)
        {
            if (dictionaryEntry == null)
                return;

            if (tool != null)
                tool.DictionaryCacheFound.AddSingleCheck(
                    MatchCode.Exact,
                    dictionaryEntry.LanguageID,
                    dictionaryEntry.KeyString,
                    dictionaryEntry);

            RemoteDictionaryEntryCache.AddSingleCheck(
                    MatchCode.Exact,
                    dictionaryEntry.LanguageID,
                    dictionaryEntry.KeyString,
                    dictionaryEntry);
        }

        public bool FlushRemoteDictionaryCache()
        {
            if (_RemoteDictionaryEntryCache == null)
                return true;

            bool returnValue = true;

            List<DictionaryEntry> dictionaryEntries = null;

            RemoteDictionaryEntryCache.GetAllSingles(
                MatchCode.Exact,
                UserProfile.TargetLanguageIDs,
                ref dictionaryEntries);

            if (dictionaryEntries != null)
            {
                try
                {
                    if (!Repositories.Dictionary.AddList(dictionaryEntries))
                    {
                        PutError("Error saving cached remote dictionary entries");
                        returnValue = false;
                    }
                }
                catch (Exception exc)
                {
                    PutExceptionError("Exception saving cached remote dictionary entries", exc);
                    returnValue = false;
                }
            }

            RemoteDictionaryEntryCache.ClearDictionaryCache();

            return returnValue;
        }

        public bool CollectReciprocalDictionaryEntries(
            List<DictionaryEntry> dictionaryEntries,
            DictionaryRepository dictionaryRepository,
            List<DictionaryEntry> dictionaryEntriesToAdd,
            List<DictionaryEntry> dictionaryEntriesToUpdate,
            out string message)
        {
            string tempMessage;

            message = "";

            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
                if (!CollectReciprocalDictionaryEntries(
                        dictionaryEntry,
                        dictionaryRepository,
                        dictionaryEntriesToAdd,
                        dictionaryEntriesToUpdate,
                        out tempMessage))
                    message += tempMessage;

            return true;
        }

        public bool CollectReciprocalDictionaryEntries(
            DictionaryEntry dictionaryEntry,
            DictionaryRepository dictionaryRepository,
            List<DictionaryEntry> dictionaryEntriesToAdd,
            List<DictionaryEntry> dictionaryEntriesToUpdate,
            out string message)
        {
            string source, dest;

            message = "";

            if (!CollectAlternateDictionaryEntries(
                    dictionaryEntry,
                    dictionaryRepository,
                    dictionaryEntriesToAdd,
                    dictionaryEntriesToUpdate,
                    out message))
                return false;

            if (dictionaryEntry.Senses != null)
            {
                LanguageID entryLanguageID = dictionaryEntry.LanguageID;
                int senseIndex = 0;

                foreach (Sense sense in dictionaryEntry.Senses)
                {
                    if (sense.LanguageSynonyms != null)
                    {
                        foreach (LanguageSynonyms languageSynonyms in sense.LanguageSynonyms)
                        {
                            LanguageID languageID = languageSynonyms.LanguageID;

                            if (languageID == entryLanguageID)
                                continue;

                            if (languageSynonyms.HasProbableSynonyms())
                            {
                                int synonymIndex = 0;

                                foreach (ProbableMeaning probableSynonym in languageSynonyms.ProbableSynonyms)
                                {
                                    if (String.IsNullOrEmpty(probableSynonym.Meaning))
                                        continue;

                                    string synonym = probableSynonym.Meaning;

                                    DictionaryEntry reciprocalEntry = GetIntermediateEntry(
                                        synonym,
                                        languageID,
                                        dictionaryRepository,
                                        dictionaryEntriesToAdd,
                                        dictionaryEntriesToUpdate,
                                        out source);

                                    if (reciprocalEntry == null)
                                    {
                                        reciprocalEntry = new DictionaryEntry(synonym, languageID);
                                        LoadReciprocalDictionaryEntry(
                                            dictionaryEntry,
                                            reciprocalEntry,
                                            senseIndex,
                                            synonymIndex);
                                        dest = "Add";
                                    }
                                    else
                                    {
                                        LoadReciprocalDictionaryEntry(
                                            dictionaryEntry,
                                            reciprocalEntry,
                                            senseIndex,
                                            synonymIndex);
                                        dest = "Update";
                                    }

                                    ProcessIntermediateEntry(
                                        reciprocalEntry,
                                        dictionaryEntriesToAdd,
                                        dictionaryEntriesToUpdate,
                                        dest,
                                        source);
                                    FormatQuickLookup.UpdateDictionaryEntry(reciprocalEntry);
                                    synonymIndex++;
                                }
                            }
                        }
                    }

                    senseIndex++;
                }
            }

            return true;
        }

        public bool CollectAlternateDictionaryEntries(
            List<DictionaryEntry> dictionaryEntries,
            DictionaryRepository dictionaryRepository,
            List<DictionaryEntry> dictionaryEntriesToAdd,
            List<DictionaryEntry> dictionaryEntriesToUpdate,
            out string message)
        {
            message = "";
            string tempMessage;

            message = "";

            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
                if (!CollectReciprocalDictionaryEntries(
                        dictionaryEntry,
                        dictionaryRepository,
                        dictionaryEntriesToAdd,
                        dictionaryEntriesToUpdate,
                        out tempMessage))
                    message += tempMessage;

            return true;
        }

        public bool CollectAlternateDictionaryEntries(
            DictionaryEntry dictionaryEntry,
            DictionaryRepository dictionaryRepository,
            List<DictionaryEntry> dictionaryEntriesToAdd,
            List<DictionaryEntry> dictionaryEntriesToUpdate,
            out string message)
        {
            message = "";

            if (dictionaryEntry.Alternates != null)
            {
                foreach (LanguageString alternate in dictionaryEntry.Alternates)
                {
                    LanguageID languageID = alternate.LanguageID;
                    string source, dest;
                    DictionaryEntry alternateEntry = GetIntermediateEntry(
                        alternate.Text,
                        alternate.LanguageID,
                        dictionaryRepository,
                        dictionaryEntriesToAdd,
                        dictionaryEntriesToUpdate,
                        out source);

                    alternateEntry = dictionaryRepository.Get(alternate.Text, alternate.LanguageID);

                    if (alternateEntry == null)
                    {
                        alternateEntry = new DictionaryEntry();
                        alternateEntry.MakeAlternate(dictionaryEntry, languageID);
                        dest = "Add";
                    }
                    else
                    {
                        if (LanguageLookup.IsAlternatePhonetic(languageID))
                            alternateEntry.MergeAlternate(dictionaryEntry, languageID);
                        else
                            alternateEntry.MakeAlternate(dictionaryEntry, languageID);

                        dest = "Update";
                    }

                    ProcessIntermediateEntry(
                        alternateEntry,
                        dictionaryEntriesToAdd,
                        dictionaryEntriesToUpdate,
                        dest,
                        source);
                    FormatQuickLookup.UpdateDictionaryEntry(alternateEntry);
                }
            }

            return true;
        }

        protected DictionaryEntry GetIntermediateEntry(
            string key,
            LanguageID languageID,
            DictionaryRepository dictionaryRepository,
            List<DictionaryEntry> dictionaryEntriesToAdd,
            List<DictionaryEntry> dictionaryEntriesToUpdate,
            out string source)
        {
            DictionaryEntry dictionaryEntry;

            if ((dictionaryEntry = dictionaryEntriesToAdd.FirstOrDefault(
                    x => TextUtilities.IsEqualStringsIgnoreCase(x.KeyString, key)
                        && (x.LanguageID == languageID))) != null)
                source = "Add";
            else if ((dictionaryEntry = dictionaryEntriesToUpdate.FirstOrDefault(
                    x => TextUtilities.IsEqualStringsIgnoreCase(x.KeyString, key)
                        && (x.LanguageID == languageID))) != null)
                source = "Update";
            else if ((dictionaryEntry = dictionaryRepository.Get(key, languageID)) != null)
                source = "Exist";
            else
                source = "Missing";

            return dictionaryEntry;
        }

        protected void ProcessIntermediateEntry(
            DictionaryEntry dictionaryEntry,
            List<DictionaryEntry> dictionaryEntriesToAdd,
            List<DictionaryEntry> dictionaryEntriesToUpdate,
            string dest,
            string source)
        {
            switch (dest)
            {
                case "Add":
                    switch (source)
                    {
                        case "Add":
                        case "Update":
                        case "Exist":
                            break;
                        case "Missing":
                            dictionaryEntriesToAdd.Add(dictionaryEntry);
                            break;
                    }
                    break;
                case "Update":
                    switch (source)
                    {
                        case "Add":
                        case "Update":
                            break;
                        case "Exist":
                            dictionaryEntriesToAdd.Add(dictionaryEntry);
                            break;
                        case "Missing":
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        public bool AddOrEditReciprocalDictionaryEntries(
            DictionaryEntry dictionaryEntry,
            DictionaryRepository dictionaryRepository,
            out string message)
        {
            message = "";

            if (!UpdateAlternateDictionaryEntries(dictionaryEntry, dictionaryRepository, out message))
                return false;

            if (dictionaryEntry.Senses != null)
            {
                LanguageID entryLanguageID = dictionaryEntry.LanguageID;
                int senseIndex = 0;

                foreach (Sense sense in dictionaryEntry.Senses)
                {
                    if (sense.LanguageSynonyms != null)
                    {
                        foreach (LanguageSynonyms languageSynonyms in sense.LanguageSynonyms)
                        {
                            LanguageID languageID = languageSynonyms.LanguageID;

                            if (languageID == entryLanguageID)
                                continue;

                            if (languageSynonyms.HasProbableSynonyms())
                            {
                                int synonymIndex = 0;

                                foreach (ProbableMeaning probableSynonym in languageSynonyms.ProbableSynonyms)
                                {
                                    string synonym = probableSynonym.Meaning;

                                    if (String.IsNullOrEmpty(synonym))
                                        continue;

                                    DictionaryEntry reciprocalEntry = dictionaryRepository.Get(synonym, languageID);

                                    if (reciprocalEntry == null)
                                    {
                                        reciprocalEntry = new DictionaryEntry(synonym, languageID);

                                        LoadReciprocalDictionaryEntry(
                                            dictionaryEntry,
                                            reciprocalEntry,
                                            senseIndex,
                                            synonymIndex);

                                        if (dictionaryRepository.Add(reciprocalEntry, languageID))
                                        {
                                            FormatQuickLookup.UpdateDictionaryEntry(reciprocalEntry);
                                        }
                                        else
                                        {
                                            message = S("Error adding reciprocal dictionary entry") + ": " + reciprocalEntry.KeyString + "\n";
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        LoadReciprocalDictionaryEntry(
                                            dictionaryEntry,
                                            reciprocalEntry,
                                            senseIndex,
                                            synonymIndex);

                                        if (dictionaryRepository.Update(reciprocalEntry, languageID))
                                        {
                                            FormatQuickLookup.UpdateDictionaryEntry(reciprocalEntry);
                                        }
                                        else
                                        {
                                            message = S("Error updating reciprocal dictionary entry") + ": " + reciprocalEntry.KeyString + "\n";
                                            return false;
                                        }
                                    }

                                    synonymIndex++;
                                }
                            }
                        }
                    }

                    senseIndex++;
                }
            }

            return true;
        }

        public bool UpdateAlternateDictionaryEntries(
            DictionaryEntry dictionaryEntry,
            DictionaryRepository dictionaryRepository,
            out string message)
        {
            message = "";

            if (dictionaryEntry.Alternates != null)
            {
                foreach (LanguageString alternate in dictionaryEntry.Alternates)
                {
                    LanguageID languageID = alternate.LanguageID;
                    DictionaryEntry alternateEntry = dictionaryRepository.Get(alternate.Text, alternate.LanguageID);

                    if (alternateEntry == null)
                    {
                        alternateEntry = new DictionaryEntry();
                        alternateEntry.MakeAlternate(dictionaryEntry, languageID);

                        if (dictionaryRepository.Add(alternateEntry, languageID))
                        {
                            FormatQuickLookup.UpdateDictionaryEntry(alternateEntry);
                        }
                        else
                        {
                            message = S("Error adding alternate dictionary entry") + ": " + alternateEntry.KeyString + "\n";
                            return false;
                        }
                    }
                    else
                    {
                        if (LanguageLookup.IsAlternatePhonetic(languageID))
                            alternateEntry.MergeAlternate(dictionaryEntry, languageID);
                        else
                            alternateEntry.MakeAlternate(dictionaryEntry, languageID);

                        if (dictionaryRepository.Update(alternateEntry, languageID))
                        {
                            FormatQuickLookup.UpdateDictionaryEntry(alternateEntry);
                        }
                        else
                        {
                            message = S("Error updating alternate dictionary entry") + ": " + alternateEntry.KeyString + "\n";
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void LoadReciprocalDictionaryEntry(DictionaryEntry dictionaryEntry, DictionaryEntry reciprocalEntry, int senseIndex, int synonymIndex)
        {
            string entryText = dictionaryEntry.KeyString;
            LanguageID entryLanguageID = dictionaryEntry.LanguageID;
            LanguageID reciprocalLanguageID = reciprocalEntry.LanguageID;
            List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(reciprocalLanguageID);
            Sense sense = null;
            int reading = 0;
            LexicalCategory category = LexicalCategory.Unknown;
            string categoryString = null;

            if ((dictionaryEntry.Senses != null) && (senseIndex < dictionaryEntry.Senses.Count()))
            {
                sense = dictionaryEntry.Senses[senseIndex];
                category = sense.Category;
                categoryString = sense.CategoryString;
            }

            if (!reciprocalEntry.HasMeaning(entryText, entryLanguageID))
            {
                ProbableMeaning probableMeaning = new ProbableMeaning(
                    entryText,
                    category,
                    categoryString,
                    float.NaN,
                    0,
                    dictionaryEntry.SourceIDs);
                LanguageSynonyms newLanguageSynomyms = new LanguageSynonyms(
                    entryLanguageID, new List<ProbableMeaning> { probableMeaning });
                Sense newSense = new Sense(reading, LexicalCategory.Unknown, "", 0, new List<LanguageSynonyms>() { newLanguageSynomyms }, null);

                if (dictionaryEntry.Alternates != null)
                {
                    foreach (LanguageString alternate in dictionaryEntry.Alternates)
                    {
                        if (alternate.LanguageID != entryLanguageID)
                        {
                            probableMeaning = new ProbableMeaning(
                                alternate.Text,
                                category,
                                categoryString,
                                float.NaN,
                                0,
                                dictionaryEntry.SourceIDs);
                            newLanguageSynomyms = new LanguageSynonyms(
                                alternate.LanguageID, new List<ProbableMeaning> { probableMeaning });
                            newSense.AddLanguageSynonyms(newLanguageSynomyms);
                        }
                    }
                }

                if ((sense != null) && (sense.LanguageSynonyms != null))
                {
                    foreach (LanguageSynonyms languageSynonyms in sense.LanguageSynonyms)
                    {
                        LanguageID synonymLanguageID = languageSynonyms.LanguageID;

                        if ((synonymLanguageID == entryLanguageID) || (entryLanguageID == reciprocalLanguageID))
                            continue;

                        if ((alternateLanguageIDs != null) && alternateLanguageIDs.Contains(synonymLanguageID))
                            continue;

                        newLanguageSynomyms = new LanguageSynonyms(languageSynonyms);
                        newSense.AddLanguageSynonyms(newLanguageSynomyms);
                    }
                }

                reciprocalEntry.AddSense(newSense);
            }

            if ((alternateLanguageIDs != null) && (dictionaryEntry.Senses != null) && (senseIndex < dictionaryEntry.Senses.Count()))
            {
                sense = dictionaryEntry.Senses[senseIndex];

                foreach (LanguageID alternateLanguageID in alternateLanguageIDs)
                {
                    if (reciprocalEntry.GetAlternate(alternateLanguageID, sense.Reading) != null)
                        continue;

                    LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(alternateLanguageID);

                    if ((languageSynonyms != null) && languageSynonyms.HasProbableSynonyms())
                    {
                        string synonym = languageSynonyms.GetSynonymIndexed(synonymIndex);
                        reciprocalEntry.AddAlternate(new LanguageString(sense.Reading, alternateLanguageID, synonym));
                    }
                }
            }
        }

        // This function depends on the dictionary entry cache being cleared, usually because
        // this node utilities object was just created.  If this is not the case, clear the
        // cached tool's dictionary entry cache before calling this function.
        public bool GetVocabularyDictionaryEntries(
            LanguageID targetLanguageID,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            string userName,
            List<string> words,
            bool translateMissingItems,
            bool synthesizeMissingAudio,
            out List<DictionaryEntry> dictionaryEntriesFound,
            out List<string> dictionaryKeysNotFound,
            out string errorMessage)
        {
            dictionaryEntriesFound = null;
            dictionaryKeysNotFound = null;
            errorMessage = null;

            if (targetLanguageID == null)
            {
                errorMessage = "Target language ID is null.";
                return false;
            }

            if ((targetLanguageIDs == null) || (targetLanguageIDs.Count() == 0))
            {
                errorMessage = "Target language IDs list is empty.";
                return false;
            }

            if ((hostLanguageIDs == null) || (hostLanguageIDs.Count() == 0))
            {
                errorMessage = "Host language IDs list is empty.";
                return false;
            }

            if (words == null)
            {
                errorMessage = "Words list is null.";
                return false;
            }

            LanguageTool tool = GetLanguageTool(targetLanguageID);
            List<DictionaryEntry> translatedDictionaryEntries = null;
            int ordinal = 0;
            IEqualityComparer<string> comparerIgnoreCase =
                (IEqualityComparer<string>)ApplicationData.GetLanguageComparer<string>(targetLanguageID, true);

            if (tool != null)
            {
                List<DictionaryEntry> formsAndStemDictionaryEntries = new List<DictionaryEntry>();

                foreach (string word in words)
                {
                    bool isInflection;

                    ordinal++;

                    // Let the word and subwords be loaded into tool's caches.
                    DictionaryEntry dictionaryEntry = tool.LookupDictionaryEntry(
                        word,
                        MatchCode.Exact,
                        targetLanguageIDs,
                        formsAndStemDictionaryEntries,
                        out isInflection);

                    if ((dictionaryEntry == null) && translateMissingItems)
                    {
                        dictionaryEntry = GetTranslatedDictionaryEntry(
                            word, targetLanguageID, targetLanguageIDs, hostLanguageIDs);

                        if (dictionaryEntry != null)
                        {
                            if (translatedDictionaryEntries == null)
                                translatedDictionaryEntries = new List<DictionaryEntry>() { dictionaryEntry };
                            else
                                translatedDictionaryEntries.Add(dictionaryEntry);
                        }
                    }
                }

                dictionaryEntriesFound = tool.GetCachedLookupEntriesFound();

                foreach (DictionaryEntry formOrStemEntry in formsAndStemDictionaryEntries)
                {
                    if (dictionaryEntriesFound.FirstOrDefault(x => TextUtilities.IsEqualStringsIgnoreCase(x.KeyString, formOrStemEntry.KeyString)) == null)
                        dictionaryEntriesFound.Add(formOrStemEntry);
                }

                dictionaryKeysNotFound = tool.GetCachedLookupKeysNotFound();
            }
            else
            {
                dictionaryEntriesFound = new List<DictionaryEntry>();
                dictionaryKeysNotFound = new List<string>();

                foreach (string word in words)
                {
                    ordinal++;

                    List<DictionaryEntry> dictionaryEntries = Repositories.Dictionary.Lookup(
                        word, MatchCode.Exact, targetLanguageIDs, 0, 0);

                    if ((dictionaryEntries != null) && (dictionaryEntries.Count() != 0))
                    {
                        foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
                            dictionaryEntriesFound.Add(dictionaryEntry);
                    }
                    else if (translateMissingItems)
                    {
                        DictionaryEntry dictionaryEntry = GetTranslatedDictionaryEntry(
                            word, targetLanguageID, targetLanguageIDs, hostLanguageIDs);

                        if (dictionaryEntry != null)
                        {
                            if (translatedDictionaryEntries == null)
                                translatedDictionaryEntries = new List<DictionaryEntry>() { dictionaryEntry };
                            else
                                translatedDictionaryEntries.Add(dictionaryEntry);
                        }
                        else
                            dictionaryKeysNotFound.Add(word);
                    }
                    else
                        dictionaryKeysNotFound.Add(word);
                }
            }

            if (dictionaryEntriesFound != null)
            {
                if ((translatedDictionaryEntries != null) && (translatedDictionaryEntries.Count() != 0))
                {
                    foreach (DictionaryEntry dictionaryEntry in translatedDictionaryEntries)
                    {
                        string key = dictionaryEntry.KeyString;
                        if (dictionaryKeysNotFound != null)
                            dictionaryKeysNotFound.Remove(key);
                        dictionaryEntriesFound.Add(dictionaryEntry);
                    }
                }

                if (synthesizeMissingAudio)
                {
                    ordinal = 0;

                    foreach (DictionaryEntry entry in dictionaryEntriesFound)
                    {
                        string key = entry.KeyString;

                        ordinal++;

                        if (words.Contains(key, comparerIgnoreCase))
                            CheckDictionaryEntryAudioOne(entry);
                    }
                }
            }

            return true;
        }

        // This function depends on the dictionary entry cache being cleared, usually because
        // this node utilities object was just created.  If this is not the case, clear the
        // cached tool's dictionary entry cache before calling this function.
        public bool CheckAddTreeNewWordsToDictionary(
            BaseObjectNodeTree tree,
            LanguageID targetLanguageID,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            string userName,
            bool translateMissingItems,
            bool synthesizeMissingAudio)
        {
            List<string> words = new List<string>();
            bool returnValue = true;

            CollectDictionaryItemsFromTree(
                tree,
                "CheckAddTreeNewWordsToDictionary",
                true,                               // bool notFoundOnly
                targetLanguageID,
                false,                              // bool doInBackground
                words,
                null,
                null,
                synthesizeMissingAudio);            // bool synthesizeMissingAudio

            if (words.Count() != 0)
                returnValue = CheckAddNewWordsToDictionary(
                    words,
                    targetLanguageID,
                    targetLanguageIDs,
                    hostLanguageIDs,
                    userName,
                    translateMissingItems,
                    synthesizeMissingAudio);

            return returnValue;
        }

        // This function depends on the dictionary entry cache being cleared, usually because
        // this node utilities object was just created.  If this is not the case, clear the
        // cached tool's dictionary entry cache before calling this function.
        public bool CheckAddNodeNewWordsToDictionary(
            BaseObjectNode node,
            LanguageID targetLanguageID,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            string userName,
            bool translateMissingItems,
            bool synthesizeMissingAudio)
        {
            List<string> words = new List<string>();
            bool returnValue = true;

            CollectDictionaryItemsFromNode(
                node,
                true,                               // bool notFoundOnly
                targetLanguageID,
                false,                              // bool doInBackground
                words,
                null,
                null,
                synthesizeMissingAudio);            // bool synthesizeMissingAudio

            if (words.Count() != 0)
                returnValue = CheckAddNewWordsToDictionary(
                    words,
                    targetLanguageID,
                    targetLanguageIDs,
                    hostLanguageIDs,
                    userName,
                    translateMissingItems,
                    synthesizeMissingAudio);

            return returnValue;
        }

        // This function depends on the dictionary entry cache being cleared, usually because
        // this node utilities object was just created.  If this is not the case, clear the
        // cached tool's dictionary entry cache before calling this function.
        public bool CheckAddStudyListNewWordsToDictionary(
            ContentStudyList studyList,
            LanguageID targetLanguageID,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            string userName,
            bool translateMissingItems,
            bool synthesizeMissingAudio)
        {
            List<MultiLanguageItem> studyItems = studyList.StudyItemsRecurse;
            List<string> words = new List<string>();
            bool returnValue = true;

            switch (studyList.Content.ContentType)
            {
                case "Words":
                case "Characters":
                    foreach (MultiLanguageItem studyItem in studyItems)
                    {
                        LanguageItem languageItem = studyItem.LanguageItem(targetLanguageID);

                        if (languageItem == null)
                            continue;

                        string word = languageItem.Text;

                        if (!String.IsNullOrEmpty(word) && !words.Contains(word))
                            words.Add(word);
                    }
                    break;
                default:
                    foreach (MultiLanguageItem studyItem in studyItems)
                    {
                        LanguageItem languageItem = studyItem.LanguageItem(targetLanguageID);

                        if (languageItem == null)
                            continue;

                        if (!languageItem.HasText() || !languageItem.HasWordRuns())
                            continue;

                        foreach (TextRun wordRun in languageItem.WordRuns)
                        {
                            string word = languageItem.GetRunText(wordRun);

                            if (!String.IsNullOrEmpty(word) && !words.Contains(word))
                                words.Add(word);
                        }
                    }
                    break;
            }

            if (words.Count() != 0)
                returnValue = CheckAddNewWordsToDictionary(
                    words,
                    targetLanguageID,
                    targetLanguageIDs,
                    hostLanguageIDs,
                    userName,
                    translateMissingItems,
                    synthesizeMissingAudio);

            return returnValue;
        }

        public bool CheckAddNewWordsToDictionary(
            List<string> words,
            LanguageID targetLanguageID,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            string userName,
            bool translateMissingItems,
            bool synthesizeMissingAudio)
        {
            bool returnValue = true;

            if (targetLanguageID == null)
                return false;

            if ((targetLanguageIDs == null) || (targetLanguageIDs.Count() == 0))
                return false;

            if ((hostLanguageIDs == null) || (hostLanguageIDs.Count() == 0))
                return false;

            if (words == null)
                return false;

            List<DictionaryEntry> translatedDictionaryEntries = null;
            int ordinal = 0;

            LanguageTool tool = GetLanguageTool(targetLanguageID);

            if (tool != null)
            {
                foreach (string word in words)
                {
                    bool isInflection;

                    ordinal++;

                    if ((ordinal % 10) == 0)
                    {
                        if (synthesizeMissingAudio)
                            PutStatusMessageElapsed("Translating and generating audio for " + ordinal.ToString() + " of " + ProgressCount.ToString() + " dictionary entries.");
                        else
                            PutStatusMessageElapsed("Translating " + ordinal.ToString() + " of " + ProgressCount.ToString() + " dictionary entries.");
                    }

                    // Let the word and subwords be loaded into tool's caches.
                    DictionaryEntry dictionaryEntry = tool.LookupDictionaryEntry(
                        word,
                        MatchCode.Exact,
                        targetLanguageIDs,
                        null,
                        out isInflection);

                    if ((dictionaryEntry == null) && translateMissingItems)
                    {
                        dictionaryEntry = GetTranslatedDictionaryEntry(
                            word, targetLanguageID, targetLanguageIDs, hostLanguageIDs);

                        if (dictionaryEntry != null)
                        {
                            if (translatedDictionaryEntries == null)
                                translatedDictionaryEntries = new List<DictionaryEntry>() { dictionaryEntry };
                            else
                                translatedDictionaryEntries.Add(dictionaryEntry);
                        }
                    }

                    if (dictionaryEntry != null)
                    {
                        string key = dictionaryEntry.KeyString;

                        if (synthesizeMissingAudio && words.Contains(key))
                            CheckDictionaryEntryAudioOne(dictionaryEntry);
                    }
                }
            }
            else
            {
                foreach (string word in words)
                {
                    ordinal++;

                    if ((ordinal % 10) == 0)
                    {
                        if (synthesizeMissingAudio)
                            PutStatusMessageElapsed("Translating and generating audio for " + ordinal.ToString() + " of " + ProgressCount.ToString() + " dictionary entries.");
                        else
                            PutStatusMessageElapsed("Translating " + ordinal.ToString() + " of " + ProgressCount.ToString() + " dictionary entries.");
                    }

                    List<DictionaryEntry> dictionaryEntries = Repositories.Dictionary.Lookup(
                        word, MatchCode.Exact, targetLanguageIDs, 0, 0);

                    if ((dictionaryEntries == null) || (dictionaryEntries.Count() == 0))
                    {
                        DictionaryEntry dictionaryEntry = GetTranslatedDictionaryEntry(
                            word, targetLanguageID, targetLanguageIDs, hostLanguageIDs);

                        if (dictionaryEntry != null)
                        {
                            if (translatedDictionaryEntries == null)
                                translatedDictionaryEntries = new List<DictionaryEntry>() { dictionaryEntry };
                            else
                                translatedDictionaryEntries.Add(dictionaryEntry);

                            string key = dictionaryEntry.KeyString;

                            if (synthesizeMissingAudio && words.Contains(key))
                                CheckDictionaryEntryAudioOne(dictionaryEntry);
                        }
                    }
                    else
                    {
                        foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
                        {
                            string key = dictionaryEntry.KeyString;

                            if (synthesizeMissingAudio && words.Contains(key))
                                CheckDictionaryEntryAudioOne(dictionaryEntry);
                        }
                    }
                }
            }

            if ((translatedDictionaryEntries != null) && (translatedDictionaryEntries.Count() != 0))
            {
                try
                {
                    PutStatusMessageElapsed("Adding dictionary entries to database.");
                    returnValue = Repositories.Dictionary.AddList(translatedDictionaryEntries, targetLanguageID);
                }
                catch (Exception exc)
                {
                    PutExceptionError(
                        "Error saving translated dictionary entries for",
                        targetLanguageID.LanguageName(UILanguageID),
                        exc);
                    returnValue = false;
                }

                if (tool != null)
                {
                    foreach (DictionaryEntry dictionaryEntry in translatedDictionaryEntries)
                        tool.DictionaryEntryAdded(dictionaryEntry);

                    tool.SaveDictionaryEntriesAddedAndUpdated();
                }

                List<DictionaryEntry> dictionaryEntriesToAdd = new List<DictionaryEntry>();
                List<DictionaryEntry> dictionaryEntriesToUpdate = new List<DictionaryEntry>();
                string errorMessage;

                PutStatusMessageElapsed("Collecting reciprocal dictionary entries.");

                if (!CollectReciprocalDictionaryEntries(
                        translatedDictionaryEntries,
                        Repositories.Dictionary,
                        dictionaryEntriesToAdd,
                        dictionaryEntriesToUpdate,
                        out errorMessage))
                    PutError(errorMessage);

                PutStatusMessageElapsed("Adding reciprocal dictionary entries to dictionary.");
                returnValue = AddMixedDictionaryEntries(dictionaryEntriesToAdd) && returnValue;
                PutStatusMessageElapsed("Updating reciprocal dictionary entries in dictionary.");
                returnValue = UpdateMixedDictionaryEntries(dictionaryEntriesToUpdate) && returnValue;
            }

            return returnValue;
        }

        public bool AddMixedDictionaryEntries(List<DictionaryEntry> dictionaryEntries)
        {
            Dictionary<string, List<DictionaryEntry>> cache = SeparateDictionaryEntries(dictionaryEntries);

            if (cache == null)
                return true;

            bool returnValue = true;

            foreach (KeyValuePair<string, List<DictionaryEntry>> kvp in cache)
            {
                LanguageID languageID = LanguageLookup.GetLanguageIDNoAdd(kvp.Key);

                try
                {
                    returnValue = Repositories.Dictionary.AddList(kvp.Value, languageID);
                }
                catch (Exception exc)
                {
                    PutExceptionError(
                        "Error adding dictionary entries for",
                        languageID.LanguageName(UILanguageID),
                        exc);
                    returnValue = false;
                }
            }

            return returnValue;
        }

        public bool UpdateMixedDictionaryEntries(List<DictionaryEntry> dictionaryEntries)
        {
            Dictionary<string, List<DictionaryEntry>> cache = SeparateDictionaryEntries(dictionaryEntries);

            if (cache == null)
                return true;

            bool returnValue = true;

            foreach (KeyValuePair<string, List<DictionaryEntry>> kvp in cache)
            {
                LanguageID languageID = LanguageLookup.GetLanguageIDNoAdd(kvp.Key);

                try
                {
                    returnValue = Repositories.Dictionary.UpdateList(kvp.Value, languageID);
                }
                catch (Exception exc)
                {
                    PutExceptionError(
                        "Error updating dictionary entries for",
                        languageID.LanguageName(UILanguageID),
                        exc);
                    returnValue = false;
                }
            }

            return returnValue;
        }

        public Dictionary<string, List<DictionaryEntry>> SeparateDictionaryEntries(
            List<DictionaryEntry> dictionaryEntries)
        {
            if ((dictionaryEntries == null) || (dictionaryEntries.Count() == 0))
                return null;

            Dictionary<string, List<DictionaryEntry>> cache = new Dictionary<string, List<DictionaryEntry>>(StringComparer.OrdinalIgnoreCase);
            List<DictionaryEntry> currentList;

            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
            {
                if (cache.TryGetValue(dictionaryEntry.LanguageID.LanguageCultureExtensionCode, out currentList))
                    currentList.Add(dictionaryEntry);
                else
                {
                    currentList = new List<DictionaryEntry>() { dictionaryEntry };
                    cache.Add(dictionaryEntry.LanguageID.LanguageCultureExtensionCode, currentList);
                }
            }

            return cache;
        }

        public static string TranslateDictionarySourceName = "Google Translate";

        private static int _TranslateDictionarySourceID = 0;
        public static int TranslateDictionarySourceID
        {
            get
            {
                if (_TranslateDictionarySourceID == 0)
                    _TranslateDictionarySourceID = ApplicationData.DictionarySourcesLazy.Add(TranslateDictionarySourceName);
                return _TranslateDictionarySourceID;
            }
        }

        public DictionaryEntry GetTranslatedDictionaryEntry(
            string word,
            LanguageID targetLanguageID,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs)
        {
            DictionaryEntry dictionaryEntry = null;
            List<LanguageString> alternates = null;
            LanguageString alternate;
            List<Sense> senses;
            Sense sense;
            List<LanguageSynonyms> languageSynonymsList = new List<LanguageSynonyms>();
            LanguageSynonyms languageSynonyms;
            List<ProbableMeaning> probableSynonyms;
            ProbableMeaning probableSynonym;
            string alternateDefinition;
            string hostDefinition;
            LanguageTranslatorSource translatorSource;
            string errorMesage;

            foreach (LanguageID alternateLanguageID in targetLanguageIDs)
            {
                if (alternateLanguageID == targetLanguageID)
                    continue;

                if (Translator.TranslateString(
                        "ContentTranslate",
                        null,
                        null,
                        word,
                        targetLanguageID,
                        alternateLanguageID,
                        out alternateDefinition,
                        out translatorSource,
                        out errorMesage))
                {
                    if (LanguageLookup.IsAlternateOfLanguageID(alternateLanguageID, targetLanguageID))
                    {
                        // Hack to fix spaces in Romaji.
                        alternateDefinition = alternateDefinition.Replace(" ", "");
                        alternate = new LanguageString(0, alternateLanguageID, alternateDefinition);

                        if (alternates == null)
                            alternates = new List<LanguageString>() { alternate };
                        else
                            alternates.Add(alternate);
                    }
                    else
                    {
                        probableSynonym = new ProbableMeaning(
                            alternateDefinition,
                            LexicalCategory.Unknown,
                            null,
                            float.NaN,
                            0,
                            TranslateDictionarySourceID);
                        probableSynonyms = new List<ProbableMeaning>() { probableSynonym };
                        languageSynonyms = new LanguageSynonyms(alternateLanguageID, probableSynonyms);
                        languageSynonymsList.Add(languageSynonyms);
                    }
                }
            }

            foreach (LanguageID hostLanguageID in hostLanguageIDs)
            {
                if (hostLanguageID == targetLanguageID)
                    continue;

                if (Translator.TranslateString(
                        "ContentTranslate",
                        null,
                        null,
                        word,
                        targetLanguageID,
                        hostLanguageID,
                        out hostDefinition,
                        out translatorSource,
                        out errorMesage))
                {
                    if (LanguageLookup.IsAlternateOfLanguageID(hostLanguageID, targetLanguageID))
                    {
                        // Hack to fix spaces in Romaji.
                        hostDefinition = hostDefinition.Replace(" ", "");
                        alternate = new LanguageString(0, hostLanguageID, hostDefinition);

                        if (alternates == null)
                            alternates = new List<LanguageString>() { alternate };
                        else
                            alternates.Add(alternate);
                    }
                    else
                    {
                        probableSynonym = new ProbableMeaning(
                            hostDefinition,
                            LexicalCategory.Unknown,
                            null,
                            float.NaN,
                            0,
                            TranslateDictionarySourceID);
                        probableSynonyms = new List<ProbableMeaning>() { probableSynonym };
                        languageSynonyms = new LanguageSynonyms(hostLanguageID, probableSynonyms);
                        languageSynonymsList.Add(languageSynonyms);
                    }
                }
            }

            sense = new Sense(0, LexicalCategory.Unknown, null, 0, languageSynonymsList, null);
            senses = new List<Sense>() { sense };
            dictionaryEntry = new DictionaryEntry(
                word,
                targetLanguageID,
                alternates,
                0,
                TranslateDictionarySourceID,
                senses,
                null);

            return dictionaryEntry;
        }

        public bool AddStudyItemToDictionary(
            BaseObjectNode nodeOrTree,
            BaseObjectContent content,
            MultiLanguageItem studyItem,
            List<LanguageID> keyLanguageIDs,
            List<LanguageID> senseLanguageIDs,
            LexicalCategory category,
            string extraCategory,
            bool addExamplesFromExpansion,
            bool includeMedia,
            ref string errorMessage)
        {
            bool returnValue = true;

            foreach (LanguageID keyLanguageID in keyLanguageIDs)
            {
                if (!AddDictionaryEntry(
                        nodeOrTree,
                        content,
                        studyItem,
                        keyLanguageID,
                        senseLanguageIDs,
                        category,
                        extraCategory,
                        addExamplesFromExpansion,
                        includeMedia,
                        ref errorMessage))
                    returnValue = false;
            }

            return returnValue;
        }

        public bool AddDictionaryEntry(
            BaseObjectNode nodeOrTree,
            BaseObjectContent content,
            MultiLanguageItem studyItem,
            LanguageID keyLanguageID,
            List<LanguageID> senseLanguageIDs,
            LexicalCategory category,
            string extraCategory,
            bool addExamplesFromExpansion,
            bool includeMedia,
            ref string errorMessage)
        {
            string keyText = studyItem.Text(keyLanguageID);
            LanguageTool tool = GetLanguageTool(keyLanguageID);
            bool isInflection = false;
            DictionaryEntry originalDictionaryEntry;
            //if (tool != null)
            //    originalDictionaryEntry = tool.LookupDictionaryEntry(keyText, MatchCode.Exact, keyLanguageID, out isInflection);
            //else
                originalDictionaryEntry = Repositories.Dictionary.Get(keyText, keyLanguageID);
            DictionaryEntry dictionaryEntry;
            bool returnValue = true;

            if (originalDictionaryEntry != null)
                dictionaryEntry = originalDictionaryEntry;
            else
                dictionaryEntry = new DictionaryEntry(keyText, keyLanguageID);

            // Don't worry about returnValue for media copy.
            if (includeMedia)
                AddStudyItemMedia(
                    nodeOrTree,
                    content,
                    studyItem,
                    keyLanguageID,
                    keyText,
                    ref errorMessage);

            foreach (LanguageID senseLanguageID in senseLanguageIDs)
            {
                if (senseLanguageID == keyLanguageID)
                    continue;

                if (!studyItem.HasLanguageID(keyLanguageID))
                    continue;

                if (!AddSenseToDictionaryEntry(
                        nodeOrTree,
                        content,
                        studyItem,
                        dictionaryEntry,
                        keyLanguageID,
                        senseLanguageIDs,
                        0,
                        category,
                        extraCategory,
                        addExamplesFromExpansion,
                        includeMedia,
                        ref errorMessage))
                    returnValue = false;
            }

            if (returnValue && !isInflection)
            {
                if (originalDictionaryEntry != null)
                {
                    if (dictionaryEntry.Modified)
                    {
                        dictionaryEntry.TouchAndClearModified();

                        try
                        {
                            if (Repositories.Dictionary.Update(dictionaryEntry, keyLanguageID))
                                tool.DictionaryEntryUpdated(dictionaryEntry, originalDictionaryEntry);
                            else
                                errorMessage = S("Error updating existing dictionary entry for: ") + keyText + "\n";
                        }
                        catch (Exception exc)
                        {
                            errorMessage = S("Exception while updating existing dictionary entry for: ") + keyText + "\n";
                            errorMessage += exc.Message + "\n";
                            if (exc.InnerException != null)
                                errorMessage += exc.InnerException.Message + "\n";
                        }
                    }
                }
                else
                {
                    try
                    {
                        if (Repositories.Dictionary.Add(dictionaryEntry, keyLanguageID))
                            tool.DictionaryEntryAdded(dictionaryEntry);
                        else
                            errorMessage = S("Error adding dictionary entry for: ") + keyText + "\n";
                    }
                    catch (Exception exc)
                    {
                        errorMessage = S("Exception while adding new dictionary entry for: ") + keyText + "\n";
                        errorMessage += exc.Message + "\n";
                        if (exc.InnerException != null)
                            errorMessage += exc.InnerException.Message + "\n";
                    }
                }
            }

            return returnValue;
        }

        public static string StudyListDictionarySourceName = "Study List";

        private static int _StudyListDictionarySourceID = 0;
        public static int StudyListDictionarySourceID
        {
            get
            {
                if (_StudyListDictionarySourceID == 0)
                    _StudyListDictionarySourceID = ApplicationData.DictionarySourcesLazy.Add(StudyListDictionarySourceName);
                return _StudyListDictionarySourceID;
            }
        }

        private static char[] SenseSeparators = { ',', '/' };

        public bool AddSenseToDictionaryEntry(
            BaseObjectNode nodeOrTree,
            BaseObjectContent content,
            MultiLanguageItem studyItem,
            DictionaryEntry dictionaryEntry,
            LanguageID keyLanguageID,
            List<LanguageID> senseLanguageIDs,
            int reading,
            LexicalCategory category,
            string extraCategory,
            bool addExamplesFromExpansion,
            bool includeMedia,
            ref string errorMessage)
        {
            List<LanguageSynonyms> languageSynonyms = new List<LanguageSynonyms>();
            List<MultiLanguageString> examples = new List<MultiLanguageString>();
            bool returnValue = true;

            foreach (LanguageID senseLanguageID in senseLanguageIDs)
            {
                if (senseLanguageID == keyLanguageID)
                    continue;

                if (!studyItem.HasLanguageID(senseLanguageID))
                    continue;

                LanguageItem languageItem = studyItem.LanguageItem(senseLanguageID);

                if (languageItem == null)
                    continue;

                string senseText = TextUtilities.GetNormalizedString(languageItem.Text.Trim());
                string[] parts = senseText.Split(SenseSeparators, StringSplitOptions.RemoveEmptyEntries);
                List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>();
                string synonym;

                foreach (string part in parts)
                {
                    synonym = part.Trim();

                    if (LanguageLookup.IsAlternateOfLanguageID(senseLanguageID, keyLanguageID))
                    {
                        if (!dictionaryEntry.HasAlternate(synonym, senseLanguageID))
                        {
                            string alternateText;
                            if (LanguageLookup.IsAlternatePhonetic(senseLanguageID))
                                ConvertPinyinNumeric.ToNumeric(out alternateText, senseText);
                            else
                                alternateText = senseText;
                            reading = dictionaryEntry.AllocateAlternateKey(senseLanguageID);
                            LanguageString alternate = new LanguageString(reading, senseLanguageID, alternateText);
                            dictionaryEntry.AddAlternate(alternate);
                        }
                    }
                    else
                    {
                        if (!dictionaryEntry.HasMeaning(synonym, senseLanguageID, category, extraCategory))
                            probableSynonyms.Add(
                                new ProbableMeaning(
                                    synonym,
                                    LexicalCategory.Unknown,
                                    null,
                                    float.NaN,
                                    0,
                                    StudyListDictionarySourceID));
                    }

                    if (includeMedia && !AddStudyItemMedia(nodeOrTree, content, studyItem, senseLanguageID, synonym, ref errorMessage))
                        returnValue = false;
                }

                if (probableSynonyms.Count() == 0)
                    continue;
                else if (probableSynonyms.Count() > 1)
                {
                    if (includeMedia && !AddStudyItemMedia(nodeOrTree, content, studyItem, senseLanguageID, senseText, ref errorMessage))
                        returnValue = false;
                }

                LanguageSynonyms languageSynonym = new LanguageSynonyms(
                    senseLanguageID,
                    probableSynonyms);

                languageSynonyms.Add(languageSynonym);
            }

            if (addExamplesFromExpansion && studyItem.HasExpansionReference())
            {
                int exampleIndex = 0;

                foreach (MultiLanguageItemReference expansionReference in studyItem.ExpansionReferences)
                {
                    MultiLanguageItem exampleItem = expansionReference.Item;
                    string exampleKey = exampleIndex.ToString();

                    if (exampleItem == null)
                        continue;

                    MultiLanguageString example = new MultiLanguageString(exampleKey, new List<LanguageString>());

                    foreach (LanguageID senseLanguageID in senseLanguageIDs)
                    {
                        if (senseLanguageID == keyLanguageID)
                            continue;

                        if (!exampleItem.HasLanguageID(senseLanguageID))
                            continue;

                        string exampleText = exampleItem.Text(senseLanguageID);

                        example.Add(new LanguageString(exampleKey, senseLanguageID, exampleText));

                        if (includeMedia && !AddStudyItemMedia(nodeOrTree, content, exampleItem, senseLanguageID, exampleText, ref errorMessage))
                            returnValue = false;
                    }

                    if (example.Count() != 0)
                    {
                        examples.Add(example);
                        exampleIndex++;
                    }
                }
            }

            if (languageSynonyms.Count() != 0)
            {
                Sense sense = new Sense(
                    reading,
                    category,
                    extraCategory,
                    0,
                    languageSynonyms,
                    examples);
                dictionaryEntry.AddSense(sense);
            }

            return returnValue;
        }

        protected bool AddStudyItemMedia(
            BaseObjectNode nodeOrTree,
            BaseObjectContent content,
            MultiLanguageItem studyItem,
            LanguageID languageID,
            string text,
            ref string errorMessage)
        {
            if (nodeOrTree == null)
                return true;

            LanguageItem languageItem = studyItem.LanguageItem(languageID);
            string mediaPathUrl = studyItem.MediaTildeUrl;
            List<string> audioUrls = new List<string>();
            List<string> pictureUrls = new List<string>();
            bool returnValue = true;

            languageItem.CollectMediaUrls("Audio", mediaPathUrl, nodeOrTree, content, audioUrls, null);

            if (audioUrls.Count() != 0)
            {
                if (!AddAudioFile(languageID, text, audioUrls.First(), ref errorMessage))
                    returnValue = false;
            }

            languageItem.CollectMediaUrls("Picture", mediaPathUrl, nodeOrTree, content, pictureUrls, null);

            if (pictureUrls.Count() == 0)
            {
                languageItem.CollectMediaUrls("BigPicture", mediaPathUrl, nodeOrTree, content, pictureUrls, null);

                if (pictureUrls.Count() == 0)
                    languageItem.CollectMediaUrls("SmallPicture", mediaPathUrl, nodeOrTree, content, pictureUrls, null);
            }

            if (pictureUrls.Count() != 0)
            {
                if (!AddPictureFile(languageID, text, pictureUrls.First(), ref errorMessage))
                    returnValue = false;
            }

            return returnValue;
        }

        protected static int MaxMediaFileNameLength = 200;

        protected bool AddAudioFile(
            LanguageID languageID,
            string text,
            string sourceMediaUrl,
            ref string errorMessage)
        {
            AudioMultiReference audioReference = Repositories.DictionaryMultiAudio.Get(text, languageID);
            string fileFriendlyName = MediaUtilities.FileFriendlyName(text, MaxMediaFileNameLength);
            string fileName;
            string audioUrl = null;
            string audioFilePath = null;
            string mimeType = MediaUtilities.GetMimeTypeFromFileName(sourceMediaUrl);
            string sourceFilePath = ApplicationData.MapToFilePath(sourceMediaUrl);
            bool returnValue = true;

            if (audioReference == null) 
            {
                audioUrl = GetAudioTildeUrl(fileFriendlyName, mimeType, languageID);
                audioFilePath = ApplicationData.MapToFilePath(audioUrl);
                fileName = MediaUtilities.GetFileName(audioFilePath);

                if (FileSingleton.Exists(audioFilePath))
                {
                    AudioInstance audioInstance = new AudioInstance(
                        text,
                        ApplicationData.ApplicationName,
                        "audio/mpeg3",
                        fileName,
                        AudioInstance.UploadedSourceName,
                        null);
                    List<AudioInstance> audioInstances = new List<AudioInstance>(1) { audioInstance };
                    audioReference = new AudioMultiReference(
                        text,
                        languageID,
                        audioInstances);
                    try
                    {
                        if (!Repositories.DictionaryMultiAudio.Add(audioReference, languageID))
                        {
                            errorMessage += S("Error adding audio reference: ") + text + "\n";
                            returnValue = false;
                        }
                    }
                    catch (Exception exc)
                    {
                        errorMessage += S("Exception while adding audio reference: ") +
                            text +
                            "\n    " +
                            exc.Message + "\n";
                        if (exc.InnerException != null)
                            errorMessage += "        " + exc.InnerException.Message + "\n";
                        returnValue = false;
                    }

                    return returnValue;
                }
            }
            else
            {
                AudioInstance audioInstance = audioReference.GetAudioInstanceIndexed(0);
                if (audioInstance == null)
                {
                    audioUrl = GetAudioTildeUrl(fileFriendlyName, mimeType, languageID);
                    audioFilePath = ApplicationData.MapToFilePath(audioUrl);
                    if (FileSingleton.Exists(audioFilePath))
                    {
                        fileName = MediaUtilities.GetFileName(audioFilePath);
                        audioInstance = new AudioInstance(
                            text,
                            ApplicationData.ApplicationName,
                            "audio/mpeg3",
                            fileName,
                            AudioInstance.UploadedSourceName,
                            null);
                        audioReference.AddAudioInstance(audioInstance);
                        try
                        {
                            if (!Repositories.DictionaryMultiAudio.Update(audioReference, languageID))
                            {
                                errorMessage += S("Error updating audio reference: ") + text + "\n";
                                returnValue = false;
                            }
                        }
                        catch (Exception exc)
                        {
                            errorMessage += S("Exception while updating audio reference: ") +
                                text +
                                "\n    " +
                                exc.Message + "\n";
                            if (exc.InnerException != null)
                                errorMessage += "        " + exc.InnerException.Message + "\n";
                            returnValue = false;
                        }
                        return returnValue;
                    }
                }
                else
                {
                    audioUrl = GetAudioTildeUrl(audioInstance.FileName, audioInstance.MimeType, languageID);
                    audioFilePath = ApplicationData.MapToFilePath(audioUrl);

                    if (FileSingleton.Exists(audioFilePath))
                        return returnValue;
                }
            }

            if (sourceMediaUrl != audioUrl)
            {
                if (FileSingleton.Exists(sourceFilePath))
                {
                    try
                    {
                        string copyMessage;
                        if (!MediaConvertSingleton.LazyConvert(sourceFilePath, mimeType, false, out copyMessage))
                        {
                            errorMessage += copyMessage + "\n";
                            returnValue = false;
                        }
                        if (!MediaConvertSingleton.CopyCheck(sourceFilePath, mimeType, audioFilePath, out copyMessage))
                        {
                            errorMessage += copyMessage + "\n";
                            returnValue = false;
                        }
                    }
                    catch (Exception exc)
                    {
                        errorMessage += S("Exception while copying audio file for: ") +
                            text +
                            "\n    " +
                            exc.Message + "\n";
                        if (exc.InnerException != null)
                            errorMessage += "        " + exc.InnerException.Message + "\n";
                        returnValue = false;
                    }
                }
            }

            return returnValue;
        }

        protected bool AddPictureFile(
            LanguageID languageID,
            string text,
            string sourceMediaUrl,
            ref string errorMessage)
        {
            PictureReference pictureReference = Repositories.DictionaryPictures.Get(text, languageID);
            string fileFriendlyName = MediaUtilities.FileFriendlyName(text, MaxMediaFileNameLength);
            string pictureUrl = null;
            string pictureFilePath = null;
            string mimeType = MediaUtilities.GetMimeTypeFromFileName(sourceMediaUrl);
            string sourceFilePath = ApplicationData.MapToFilePath(sourceMediaUrl);
            bool returnValue = true;

            if (pictureReference == null)
            {
                pictureUrl = GetPictureTildeUrl(fileFriendlyName, mimeType, languageID);
                pictureFilePath = ApplicationData.MapToFilePath(pictureUrl);

                if (FileSingleton.Exists(pictureFilePath))
                {
                    pictureReference = new PictureReference(
                        text,
                        text,
                        ApplicationData.ApplicationName,
                        mimeType,
                        fileFriendlyName);
                    try
                    {
                        if (!Repositories.DictionaryPictures.Add(pictureReference, languageID))
                        {
                            errorMessage += S("Error adding picture reference: ") + text + "\n";
                            returnValue = false;
                        }
                    }
                    catch (Exception exc)
                    {
                        errorMessage += S("Exception while adding picture reference: ") +
                            text +
                            "\n    " +
                            exc.Message + "\n";
                        if (exc.InnerException != null)
                            errorMessage += "        " + exc.InnerException.Message + "\n";
                        returnValue = false;
                    }

                    return returnValue;
                }
            }
            else
            {
                pictureUrl = GetPictureTildeUrl(pictureReference.PictureFilePath, pictureReference.PictureMimeType, languageID);
                pictureFilePath = ApplicationData.MapToFilePath(pictureUrl);

                if (FileSingleton.Exists(pictureFilePath))
                    return returnValue;
            }

            if (sourceMediaUrl != pictureUrl)
            {
                if (FileSingleton.Exists(sourceFilePath))
                {
                    try
                    {
                        FileSingleton.DirectoryExistsCheck(pictureFilePath);
                        FileSingleton.Copy(sourceFilePath, pictureFilePath);
                    }
                    catch (Exception exc)
                    {
                        errorMessage += S("Exception while adding copying picture file for: ") +
                            text +
                            "\n    " +
                            exc.Message + "\n";
                        if (exc.InnerException != null)
                            errorMessage += "        " + exc.InnerException.Message + "\n";
                        returnValue = false;
                    }
                }
            }

            return returnValue;
        }

        public static string GetAudioDictionaryPath(LanguageID languageID)
        {
            string contentPath = ApplicationData.ContentPath;
            string dictionaryPath = MediaUtilities.ConcatenateFilePath(contentPath, "Dictionary");
            string audioPath = MediaUtilities.ConcatenateFilePath(dictionaryPath, "Audio");
            string returnValue = MediaUtilities.ConcatenateFilePath(audioPath, languageID.LanguageCultureExtensionCode);
            return returnValue;
        }

        public static string GetAudioTildeUrl(string dictionaryPath, string mimeType, LanguageID languageID)
        {
            dictionaryPath = dictionaryPath.Replace('\\', '/');
            string returnValue = "";

            if (!dictionaryPath.StartsWith("~"))
                returnValue += "~/Content/Dictionary/Audio/" + languageID.LanguageCultureExtensionCode + "/" + dictionaryPath;
            else
                returnValue = dictionaryPath;

            if (!MediaUtilities.HasFileExtension(returnValue))
            {
                string ext = MediaUtilities.GetFileExtensionFromMimeType(mimeType);
                returnValue = MediaUtilities.ChangeFileExtension(returnValue, ".mp3");
            }

            return returnValue;
        }

        public static string GetRemoteAudioTildeUrl(string dictionaryPath, string mimeType, LanguageID languageID)
        {
            dictionaryPath = dictionaryPath.Replace('\\', '/');
            string returnValue = "";

            if (!MediaUtilities.HasFileExtension(dictionaryPath))
            {
                string ext = MediaUtilities.GetFileExtensionFromMimeType(mimeType);
                dictionaryPath = MediaUtilities.ChangeFileExtension(dictionaryPath, ext);
            }

            if (!dictionaryPath.StartsWith("~"))
                returnValue = GetRemoteMediaTildeUrl("Audio", languageID, dictionaryPath);
            else
                returnValue = ApplicationData.GetRemoteMediaUrlFromTildeUrl(dictionaryPath);

            return returnValue;
        }

        public static string GetPictureTildeUrl(string dictionaryPath, string mimeType, LanguageID languageID)
        {
            dictionaryPath = dictionaryPath.Replace('\\', '/');
            string returnValue = "";

            if (!dictionaryPath.StartsWith("~"))
                returnValue += "~/Content/Dictionary/Pictures/" + languageID.LanguageCultureExtensionCode + "/" + dictionaryPath;
            else
                returnValue = dictionaryPath;

            if (!MediaUtilities.HasFileExtension(returnValue))
            {
                string ext = MediaUtilities.GetFileExtensionFromMimeType(mimeType);
                returnValue = MediaUtilities.ChangeFileExtension(returnValue, ext);
            }

            return returnValue;
        }

        public static string GetRemotePictureTildeUrl(string dictionaryPath, string mimeType, LanguageID languageID)
        {
            dictionaryPath = dictionaryPath.Replace('\\', '/');
            string returnValue = "";

            if (!MediaUtilities.HasFileExtension(dictionaryPath))
            {
                string ext = MediaUtilities.GetFileExtensionFromMimeType(mimeType);
                dictionaryPath = MediaUtilities.ChangeFileExtension(dictionaryPath, ext);
            }

            if (!dictionaryPath.StartsWith("~"))
                returnValue = GetRemoteMediaTildeUrl("Pictures", languageID, dictionaryPath);
            else
                returnValue = ApplicationData.GetRemoteMediaUrlFromTildeUrl(dictionaryPath);

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

        public static string GetRemoteMediaTildeUrl(string mediaType, LanguageID languageID, string fileName)
        {
            string returnValue = GetRemoteMediaTildeUrl(mediaType, languageID);
            returnValue = MediaUtilities.ConcatenateUrlPath(
                returnValue,
                fileName);
            return returnValue;
        }

        public static string GetRemoteMediaTildeUrl(string mediaType, LanguageID languageID)
        {
            string returnValue = MediaUtilities.ConcatenateUrlPath(
                ApplicationData.GetRemoteMediaUrlFromTildeUrl(ApplicationData.ContentTildeUrl),
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

        public bool GetMediaRecordKeys(
            List<DictionaryEntry> dictionaryEntries,
            Dictionary<int, bool> entrySelectFlags,
            List<LanguageDescriptor> displayLanguageDescriptors,
            List<LanguageString> audioRecordKeys,
            List<LanguageString> pictureRecordKeys,
            bool existingOnly,
            bool useRemoteMedia,
            bool isSynthesizeMissingAudio,
            bool showAudio,
            bool showPictures,
            bool showExamples)
        {
            int entryIndex = 0;
            bool returnValue = true;

            foreach (DictionaryEntry entry in dictionaryEntries)
            {
                LanguageID languageID = entry.LanguageID;
                string entryKeyString = entry.KeyString;
                //string entryTextString = TextUtilities.GetDisplayText(entry.KeyString, languageID, Repositories.Dictionary);
                bool entryHasAudio = false;
                bool entryHasPicture = false;
                string mainAudioUrl = String.Empty;
                string mainPictureUrl = String.Empty;
                string audioUrl;
                string pictureUrl;
                bool useIt = true;

                if ((entrySelectFlags != null) &&
                        entrySelectFlags.TryGetValue(entryIndex, out useIt) &&
                        !useIt)
                {
                    entryIndex++;
                    continue;
                }

                if (showAudio)
                    entryHasAudio = ProcessAudio(
                        entryKeyString,
                        languageID,
                        null,
                        languageID,
                        existingOnly,
                        null,
                        isSynthesizeMissingAudio,
                        useRemoteMedia,
                        audioRecordKeys,
                        out mainAudioUrl);

                if (showPictures)
                    entryHasPicture = ProcessPicture(
                        entryKeyString,
                        languageID,
                        null,
                        languageID,
                        existingOnly,
                        null,
                        useRemoteMedia,
                        pictureRecordKeys,
                        out mainPictureUrl);

                if (!entryHasAudio)
                    mainAudioUrl = null;

                if ((entry.Senses != null) && (entry.Senses.Count() != 0))
                {
                    foreach (Sense sense in entry.Senses)
                    {
                        foreach (LanguageDescriptor languageDescriptor in displayLanguageDescriptors)
                        {
                            languageID = languageDescriptor.LanguageID;
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

                                    if (TextUtilities.IsEqualStringsIgnoreCase(synonymCanonical, synonym))
                                        synonymCanonical = null;

                                    if (showAudio)
                                        ProcessAudio(
                                            synonym,
                                            synonymnLanguageID,
                                            synonymCanonical,
                                            synonymnLanguageID,
                                            existingOnly,
                                            null,
                                            isSynthesizeMissingAudio,
                                            useRemoteMedia,
                                            audioRecordKeys,
                                            out audioUrl);

                                    if (showPictures)
                                        ProcessPicture(
                                            synonym,
                                            synonymnLanguageID,
                                            synonymCanonical,
                                            synonymnLanguageID,
                                            existingOnly,
                                            null,
                                            useRemoteMedia,
                                            pictureRecordKeys,
                                            out pictureUrl);
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

                                        if (TextUtilities.IsEqualStringsIgnoreCase(exampleTextCanonical, exampleText))
                                            exampleTextCanonical = null;

                                        if (showAudio)
                                            ProcessAudio(
                                                exampleText,
                                                languageID,
                                                exampleTextCanonical,
                                                languageID,
                                                existingOnly,
                                                null,
                                                isSynthesizeMissingAudio,
                                                useRemoteMedia,
                                                audioRecordKeys,
                                                out audioUrl);

                                        ProcessPicture(
                                            exampleText,
                                            languageID,
                                            exampleTextCanonical,
                                            languageID,
                                            existingOnly,
                                            null,
                                            useRemoteMedia,
                                            pictureRecordKeys,
                                            out pictureUrl);
                                    }
                                }
                            }
                        }
                    }
                }

                if ((entry.Alternates != null) && (entry.Alternates.Count() != 0))
                {
                    foreach (LanguageString alternate in entry.Alternates)
                    {
                        languageID = alternate.LanguageID;
                        string alternateText = alternate.Text;
                        string alternateDisplay = TextUtilities.GetDisplayText(alternateText, languageID, Repositories.Dictionary);
                        string alternateAlternateText = null;
                        if (alternateText != alternateDisplay)
                            alternateAlternateText = alternateDisplay;
                        else
                        {
                            string alternateCanonical = TextUtilities.GetCanonicalText(
                                alternate.Text,
                                languageID);
                            if (alternateText != alternateCanonical)
                                alternateAlternateText = alternateCanonical;
                        }
                        if (showAudio)
                            ProcessAudio(
                                alternateText,
                                languageID,
                                alternateAlternateText,
                                languageID,
                                existingOnly,
                                mainAudioUrl,
                                isSynthesizeMissingAudio,
                                useRemoteMedia,
                                audioRecordKeys,
                                out audioUrl);
                        if (showPictures)
                            ProcessPicture(
                                alternateText,
                                languageID,
                                alternateAlternateText,
                                languageID,
                                existingOnly,
                                mainPictureUrl,
                                useRemoteMedia,
                                pictureRecordKeys,
                                out pictureUrl);

                        if (!String.IsNullOrEmpty(alternateAlternateText))
                        {
                            if (showAudio)
                                ProcessAudio(
                                    alternateAlternateText,
                                    languageID,
                                    alternateText,
                                    languageID,
                                    existingOnly,
                                    mainAudioUrl,
                                    isSynthesizeMissingAudio,
                                    useRemoteMedia,
                                    audioRecordKeys,
                                    out audioUrl);
                            if (showPictures)
                                ProcessPicture(
                                    alternateAlternateText,
                                    languageID,
                                    alternateText,
                                    languageID,
                                    existingOnly,
                                    mainPictureUrl,
                                    useRemoteMedia,
                                    pictureRecordKeys,
                                    out pictureUrl);
                        }
                    }
                }

                entryIndex++;
            }

            return returnValue;
        }

        // Returns true if audio file exists.
        public bool ProcessAudio(
            string normalKey,
            LanguageID languageID,
            string alternateKey,
            LanguageID alternateLanguageID,
            bool existingOnly,
            string mainAudioUrl,
            bool isSynthesizeMissingAudio,
            bool useRemoteMedia,
            List<LanguageString> audioRecordKeys,
            out string audioUrl)
        {
            string key = normalKey;
            string fileFriendlyName = MediaUtilities.FileFriendlyName(
                key,
                MediaUtilities.MaxMediaFileNameLength);
            bool entryHasAudio = false;
            AudioMultiRepository localRepository = Repositories.DictionaryMultiAudio;
            AudioMultiRepository remoteRepository = (RemoteRepositories != null ? RemoteRepositories.DictionaryMultiAudio : null);
            AudioMultiReference localAudioReference = null;
            AudioMultiReference remoteAudioReference = null;
            List<AudioInstance> audioInstances = null;
            AudioInstance audioInstance = null;
            string remoteAudioUrl;
            string audioFilePath = null;
            string alternateAudioUrl;
            string alternateAudioFilePath;
            string alternateFileFriendlyName;
            string mimeType = "audio/mpeg3";

            localAudioReference = localRepository.Get(key, languageID);

            audioUrl = null;

            if (localAudioReference == null)
            {
                audioUrl = GetAudioTildeUrl(fileFriendlyName, mimeType, languageID);
                audioFilePath = ApplicationData.MapToFilePath(audioUrl);

                if (!String.IsNullOrEmpty(audioFilePath) && FileSingleton.Exists(audioFilePath))
                {
                    entryHasAudio = true;

                    if (audioRecordKeys != null)
                        audioRecordKeys.Add(new LanguageString(key, languageID, audioUrl));

                    audioInstance = new AudioInstance(
                        key,
                        ApplicationData.ApplicationName,
                        mimeType,
                        fileFriendlyName + ".mp3",
                        AudioReference.SynthesizedSourceName,
                        null);
                    audioInstances = new List<AudioInstance>();
                    localAudioReference = new AudioMultiReference(
                        key,
                        languageID,
                        audioInstances);

                    try
                    {
                        if (!localRepository.Add(localAudioReference, languageID))
                            PutErrorArgument("Error adding audio reference: ", key);
                    }
                    catch (Exception exc)
                    {
                        PutErrorArgument("Exception while adding audio reference: ", key);
                        Error = Error + ("\n    " + exc.Message);

                        if (exc.InnerException != null)
                            Error = Error + ": " + exc.InnerException.Message;
                    }
                }
                else if (!String.IsNullOrEmpty(alternateKey) && (alternateKey != key))
                {
                    alternateFileFriendlyName = MediaUtilities.FileFriendlyName(
                        alternateKey,
                        MediaUtilities.MaxMediaFileNameLength);
                    alternateAudioUrl = GetAudioTildeUrl(alternateFileFriendlyName, mimeType, alternateLanguageID);
                    alternateAudioFilePath = ApplicationData.MapToFilePath(alternateAudioUrl);
                    audioUrl = alternateAudioUrl;
                    audioFilePath = alternateAudioFilePath;

                    if (FileSingleton.Exists(alternateAudioFilePath))
                    {
                        if (!existingOnly && (audioRecordKeys != null))
                            audioRecordKeys.Add(new LanguageString(key, languageID, alternateAudioUrl));

                        return true;
                    }
                }
            }

            if (useRemoteMedia && (localAudioReference == null) && (remoteRepository != null) && !entryHasAudio)
                remoteAudioReference = remoteRepository.Get(key, languageID);

            if (!entryHasAudio && (localAudioReference == null) && (remoteAudioReference != null))
            {
                remoteAudioUrl = GetRemoteAudioTildeUrl(fileFriendlyName, mimeType, languageID);
                audioUrl = GetAudioTildeUrl(fileFriendlyName, mimeType, languageID);
                audioFilePath = ApplicationData.MapToFilePath(audioUrl);

                if (ApplicationData.Global.GetRemoteMediaFile(remoteAudioUrl, audioFilePath, ref Error))
                {
                    if (audioRecordKeys != null)
                        audioRecordKeys.Add(new LanguageString(key, languageID, audioUrl));

                    entryHasAudio = true;

                    audioInstance = new AudioInstance(
                        key,
                        ApplicationData.ApplicationName,
                        mimeType,
                        fileFriendlyName + ".mp3",
                        AudioReference.SynthesizedSourceName,
                        null);
                    audioInstances = new List<AudioInstance>();
                    localAudioReference = new AudioMultiReference(
                        key,
                        languageID,
                        audioInstances);

                    try
                    {
                        if (!localRepository.Add(localAudioReference, languageID))
                            PutErrorArgument("Error adding audio reference: ", key);
                    }
                    catch (Exception exc)
                    {
                        PutErrorArgument("Exception while adding audio reference: ", key);
                        Error = Error + ("\n    " + exc.Message);

                        if (exc.InnerException != null)
                            Error = Error + ": " + exc.InnerException.Message;
                    }
                }
            }
            else if ((localAudioReference != null) && !entryHasAudio)
            {
                audioUrl = GetAudioTildeUrl(fileFriendlyName, mimeType, languageID);
                audioFilePath = ApplicationData.MapToFilePath(audioUrl);

                if (FileSingleton.Exists(audioFilePath))
                {
                    entryHasAudio = true;

                    if (audioRecordKeys != null)
                        audioRecordKeys.Add(new LanguageString(key, languageID, audioUrl));
                }
                else
                {
                    if (!String.IsNullOrEmpty(alternateKey) && (alternateKey != key))
                    {
                        alternateFileFriendlyName = MediaUtilities.FileFriendlyName(
                            alternateKey,
                            MediaUtilities.MaxMediaFileNameLength);
                        alternateAudioUrl = GetAudioTildeUrl(alternateFileFriendlyName, mimeType, alternateLanguageID);
                        alternateAudioFilePath = ApplicationData.MapToFilePath(alternateAudioUrl);
                        audioUrl = alternateAudioUrl;
                        audioFilePath = alternateAudioFilePath;

                        if (FileSingleton.Exists(alternateAudioFilePath))
                        {
                            if (!existingOnly && (audioRecordKeys != null))
                                audioRecordKeys.Add(new LanguageString(key, languageID, alternateAudioUrl));

                            return true;
                        }
                    }

                    if (!existingOnly && (audioRecordKeys != null))
                        audioRecordKeys.Add(new LanguageString(key, languageID, audioUrl));
                }
            }

            if (!entryHasAudio && existingOnly && !String.IsNullOrEmpty(mainAudioUrl))
            {
                if (audioRecordKeys != null)
                    audioRecordKeys.Add(new LanguageString(key, languageID, mainAudioUrl));

                entryHasAudio = true;
            }
            else if (!entryHasAudio && !existingOnly && !String.IsNullOrEmpty(audioUrl))
            {
                if (isSynthesizeMissingAudio && !String.IsNullOrEmpty(audioFilePath))
                {
                    entryHasAudio = AddSynthesizedVoiceDefault(
                        key,
                        audioFilePath,
                        languageID);

                    if (entryHasAudio &&
                        (localAudioReference == null) &&
                        (remoteAudioReference == null))
                    {
                        if (audioRecordKeys != null)
                            audioRecordKeys.Add(new LanguageString(key, languageID, audioUrl));

                        audioInstance = new AudioInstance(
                            key,
                            ApplicationData.ApplicationName,
                            mimeType,
                            fileFriendlyName + ".mp3",
                            AudioReference.SynthesizedSourceName,
                            null);
                        audioInstances = new List<AudioInstance>();
                        localAudioReference = new AudioMultiReference(
                            key,
                            languageID,
                            audioInstances);

                        try
                        {
                            if (!localRepository.Add(localAudioReference, languageID))
                                PutErrorArgument("Error adding audio reference: ", key);
                        }
                        catch (Exception exc)
                        {
                            PutErrorArgument("Exception while adding audio reference: ", key);
                            Error = Error + ("\n    " + exc.Message);

                            if (exc.InnerException != null)
                                Error = Error + ": " + exc.InnerException.Message;
                        }
                    }
                }
            }

            return entryHasAudio;
        }

        // Returns true if picture file exists.
        public bool ProcessPicture(
            string normalKey,
            LanguageID languageID,
            string alternateKey,
            LanguageID alternateLanguageID,
            bool existingOnly,
            string mainPictureUrl,
            bool useRemoteMedia,
            List<LanguageString> pictureRecordKeys,
            out string pictureUrl)
        {
            string key = normalKey;
            string fileFriendlyName = MediaUtilities.FileFriendlyName(
                key,
                 MediaUtilities.MaxMediaFileNameLength);
            bool entryHasPicture = false;
            PictureRepository localRepository = Repositories.DictionaryPictures;
            PictureRepository remoteRepository = (RemoteRepositories != null ? RemoteRepositories.DictionaryPictures : null);
            PictureReference localPictureReference = null;
            PictureReference remotePictureReference = null;
            string remotePictureUrl;
            string pictureFilePath;
            string alternatePictureUrl;
            string alternatePictureFilePath;
            string alternateFileFriendlyName;
            string mimeType = "image/jpeg";

            localPictureReference = localRepository.Get(key, languageID);

            pictureUrl = null;

            if (localPictureReference == null)
            {
                pictureUrl = GetPictureTildeUrl(fileFriendlyName, mimeType, languageID);
                pictureFilePath = ApplicationData.MapToFilePath(pictureUrl);

                if (FileSingleton.Exists(pictureFilePath))
                {
                    entryHasPicture = true;

                    if (pictureRecordKeys != null)
                        pictureRecordKeys.Add(new LanguageString(key, languageID, pictureUrl));

                    localPictureReference = new PictureReference(
                        key,
                        key,
                        ApplicationData.ApplicationName,
                        mimeType,
                        fileFriendlyName);

                    try
                    {
                        if (!localRepository.Add(localPictureReference, languageID))
                            PutErrorArgument("Error adding picture reference: ", key);
                    }
                    catch (Exception exc)
                    {
                        PutErrorArgument("Exception while adding picture reference: ", key);
                        Error = Error + ("\n    " + exc.Message);

                        if (exc.InnerException != null)
                            Error = Error + ": " + exc.InnerException.Message;
                    }
                }
                else if (!String.IsNullOrEmpty(alternateKey) && (alternateKey != key))
                {
                    alternateFileFriendlyName = MediaUtilities.FileFriendlyName(
                        alternateKey,
                        MediaUtilities.MaxMediaFileNameLength);
                    alternatePictureUrl = GetPictureTildeUrl(alternateFileFriendlyName, mimeType, alternateLanguageID);
                    alternatePictureFilePath = ApplicationData.MapToFilePath(alternatePictureUrl);
                    pictureUrl = alternatePictureUrl;
                    pictureFilePath = alternatePictureFilePath;

                    if (FileSingleton.Exists(alternatePictureFilePath))
                    {
                        entryHasPicture = true;

                        if (!existingOnly && (pictureRecordKeys != null))
                            pictureRecordKeys.Add(new LanguageString(key, languageID, alternatePictureFilePath));

                        localPictureReference = new PictureReference(
                            key,
                            key,
                            ApplicationData.ApplicationName,
                            mimeType,
                            alternateFileFriendlyName);

                        try
                        {
                            if (!localRepository.Add(localPictureReference, languageID))
                                PutErrorArgument("Error adding alternate picture reference: ", key);
                        }
                        catch (Exception exc)
                        {

                            Error = Error + S("Exception while adding picture reference: ") +
                                key +
                                "\n    " +
                                exc.Message;

                            if (exc.InnerException != null)
                                Error = Error + ": " + exc.InnerException.Message;
                        }
                    }
                }
            }

            if (!entryHasPicture && useRemoteMedia && (localPictureReference == null) && (RemoteRepositories != null))
                remotePictureReference = remoteRepository.Get(key, languageID);

            if ((localPictureReference == null) && (remotePictureReference != null) && !entryHasPicture)
            {
                remotePictureUrl = GetRemotePictureTildeUrl(fileFriendlyName, mimeType, languageID);
                pictureUrl = GetPictureTildeUrl(fileFriendlyName, mimeType, languageID);
                pictureFilePath = ApplicationData.MapToFilePath(pictureUrl);

                if (ApplicationData.Global.GetRemoteMediaFile(remotePictureUrl, pictureFilePath, ref Error))
                {
                    if (pictureRecordKeys != null)
                        pictureRecordKeys.Add(new LanguageString(key, languageID, pictureUrl));

                    entryHasPicture = true;

                    try
                    {
                        if (!localRepository.Add(remotePictureReference, languageID))
                            PutErrorArgument("Error adding picture reference: ", key);
                    }
                    catch (Exception exc)
                    {
                        Error = Error + S("Exception while adding picture reference: ") +
                            key +
                            "\n    " +
                            exc.Message;

                        if (exc.InnerException != null)

                            Error = Error + ": " + exc.InnerException.Message;
                    }
                }
            }
            else if (localPictureReference != null)
            {
                pictureUrl = GetPictureTildeUrl(fileFriendlyName, mimeType, languageID);
                pictureFilePath = ApplicationData.MapToFilePath(pictureUrl);

                if (FileSingleton.Exists(pictureFilePath))
                {
                    entryHasPicture = true;

                    if (pictureRecordKeys != null)
                        pictureRecordKeys.Add(new LanguageString(key, languageID, pictureUrl));
                }
                else
                {
                    if (!String.IsNullOrEmpty(alternateKey) && (alternateKey != key))
                    {
                        alternateFileFriendlyName = MediaUtilities.FileFriendlyName(
                            alternateKey,
                            MediaUtilities.MaxMediaFileNameLength);
                        alternatePictureUrl = GetPictureTildeUrl(alternateFileFriendlyName, mimeType, alternateLanguageID);
                        alternatePictureFilePath = ApplicationData.MapToFilePath(alternatePictureUrl);
                        pictureUrl = alternatePictureUrl;
                        pictureFilePath = alternatePictureFilePath;

                        if (FileSingleton.Exists(alternatePictureFilePath))
                        {
                            if (!existingOnly && (pictureRecordKeys != null))
                                pictureRecordKeys.Add(new LanguageString(key, languageID, alternatePictureUrl));
                            return true;
                        }
                    }

                    if (!existingOnly && (pictureRecordKeys != null))
                        pictureRecordKeys.Add(new LanguageString(key, languageID, pictureUrl));
                }
            }

            if (!entryHasPicture && !existingOnly && !String.IsNullOrEmpty(mainPictureUrl))
            {
                if (pictureRecordKeys != null)
                    pictureRecordKeys.Add(new LanguageString(key, languageID, mainPictureUrl));

                entryHasPicture = true;
            }

            return entryHasPicture;
        }

        public bool GetMediaForStudyItem(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs,
            bool isGetAudio,
            bool isGetPictures,
            bool useRemoteMedia,
            bool isLookupDictionaryAudio,
            bool isSynthesizeMissingAudio,
            bool isForceAudio,
            bool isLookupDictionaryPicture)
        {
            bool returnValue = true;

            if (isGetAudio && !GetAudioForStudyItem(
                    studyItem,
                    languageIDs,
                    useRemoteMedia,
                    isLookupDictionaryAudio,
                    isSynthesizeMissingAudio,
                    isForceAudio))
                returnValue = false;

            if (isGetPictures && isLookupDictionaryPicture && !GetPictureForStudyItem(
                    studyItem,
                    languageIDs,
                    useRemoteMedia))
                returnValue = false;

            return returnValue;
        }

        public bool GetAudioForStudyItem(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs,
            bool useRemoteMedia,
            bool isLookupDictionaryMedia,
            bool isSynthesizeMissingAudio,
            bool isForceAudio)
        {
            List<LanguageID> doneLanguageIDs = new List<LanguageID>();
            bool returnValue = true;

            foreach (LanguageID languageID in languageIDs)
            {
                if (doneLanguageIDs.Contains(languageID))
                    continue;

                doneLanguageIDs.Add(languageID);

                if (!studyItem.HasText(languageID))
                    continue;

                LanguageID baseLanguageID = languageID;
                LanguageID bestVoiceLanguageID = languageID;
                List<LanguageID> familyLanguageIDs = null;

                if (LanguageLookup.HasAnyAlternates(languageID))
                {
                    baseLanguageID = LanguageLookup.GetRootLanguageID(languageID);
                    bestVoiceLanguageID = LanguageLookup.GetBestVoiceLanguageID(baseLanguageID);

                    if (!studyItem.HasText(bestVoiceLanguageID))
                    {
                        bestVoiceLanguageID = baseLanguageID;

                        if (!studyItem.HasText(bestVoiceLanguageID))
                            bestVoiceLanguageID = languageID;
                    }

                    familyLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(baseLanguageID);

                    for (int fli = familyLanguageIDs.Count() - 1; fli >= 0; fli--)
                    {
                        LanguageID familyLanguageID = familyLanguageIDs[fli];

                        if (familyLanguageID == languageID)
                            familyLanguageIDs.Remove(familyLanguageID);
                        else if (!studyItem.HasText(familyLanguageID))
                            familyLanguageIDs.Remove(familyLanguageID);
                        else
                        {
                            if (!doneLanguageIDs.Contains(familyLanguageID))
                                doneLanguageIDs.Add(familyLanguageID);
                        }
                    }
                }

                if (!GetAudioForStudyItemLanguage(
                        studyItem,
                        languageID,
                        bestVoiceLanguageID,
                        familyLanguageIDs,
                        useRemoteMedia,
                        isLookupDictionaryMedia,
                        isSynthesizeMissingAudio,
                        isForceAudio))
                {
                    PutMessage("Get audio failed for", studyItem.Text(languageID));
                    returnValue = false;
                }
            }

            return returnValue;
        }

        public bool GetAudioForStudyItemLanguage(
            MultiLanguageItem studyItem,
            LanguageID itemLanguageID,
            LanguageID voiceLanguageID,
            List<LanguageID> familyLanguageIDs,
            bool useRemoteMedia,
            bool isLookupDictionaryMedia,
            bool isSynthesizeMissingAudio,
            bool isForceAudio)
        {
            LanguageItem languageItem = studyItem.LanguageItem(itemLanguageID);
            LanguageItem voiceLanguageItem = studyItem.LanguageItem(voiceLanguageID);
            bool returnValue = true;

            if (languageItem == null)
                return false;

            if (!languageItem.HasText())
                return true;

            if ((voiceLanguageItem == null) || !voiceLanguageItem.HasText())
            {
                voiceLanguageItem = languageItem;
                voiceLanguageID = itemLanguageID;
            }

            if (!languageItem.HasSentenceRuns())
            {
                languageItem.LoadSentenceRunsFromText();

                if (!languageItem.HasSentenceRuns())
                    return true;
            }

            if ((voiceLanguageItem != languageItem) && !voiceLanguageItem.HasSentenceRuns())
            {
                voiceLanguageItem.LoadSentenceRunsFromText();

                if (!voiceLanguageItem.HasSentenceRuns())
                    return true;
            }

            int sentenceCount = languageItem.SentenceRunCount();
            int sentenceIndex;

            for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
            {
                TextRun itemSentenceRun = languageItem.GetSentenceRun(sentenceIndex);
                TextRun voiceSentenceRun = voiceLanguageItem.GetSentenceRun(sentenceIndex);

                if (voiceSentenceRun == null)
                    return false;

                string text = voiceLanguageItem.GetRunText(voiceSentenceRun);
                string audioSourceUrl = null;
                string studyItemKey = studyItem.KeyString;
                string fileName = MediaUtilities.ComposeStudyItemFileName(
                    studyItemKey, sentenceIndex, itemLanguageID, "Audio", ".mp3");
                string urlDirectory = studyItem.MediaTildeUrl;
                string urlPath = MediaUtilities.ConcatenateUrlPath(urlDirectory, fileName);
                string filePath = ApplicationData.MapToFilePath(urlPath);
                bool isFileExists = FileSingleton.Exists(filePath);

                if (isForceAudio && isFileExists)
                {
                    try
                    {
                        FileSingleton.Delete(filePath);
                        isFileExists = false;
                    }
                    catch (Exception exc)
                    {
                        PutExceptionError("Error deleting pre-existing audio", exc);
                        return false;
                    }
                }

                if (isFileExists)
                    return true;

                MediaRun mediaRun = null;

                if (isLookupDictionaryMedia)
                {
                    string alternateKey = null;
                    LanguageID alternateLanguageID = null;

                    GetStudyItemAlternateKeyAndLanguage(
                        studyItem,
                        voiceLanguageID,
                        sentenceIndex,
                        out alternateKey,
                        out alternateLanguageID);

                    returnValue = ProcessAudio(text, voiceLanguageID, alternateKey, alternateLanguageID,
                        false, null, isSynthesizeMissingAudio, useRemoteMedia, null, out audioSourceUrl);

                    if (returnValue)
                    {
                        string audioSourceFilePath = ApplicationData.MapToFilePath(audioSourceUrl);

                        try
                        {
                            FileSingleton.DirectoryExistsCheck(filePath);
                            FileSingleton.Copy(audioSourceFilePath, filePath);
                        }
                        catch (Exception exc)
                        {
                            PutExceptionError("Error copying dictionary audio", exc);
                            return false;
                        }

                        mediaRun = itemSentenceRun.GetMediaRun("Audio");

                        if (mediaRun != null)
                        {
                            if (mediaRun.FileName != fileName)
                            {
                                itemSentenceRun.DeleteMediaRun(mediaRun);
                                mediaRun = new MediaRun("Audio", fileName, studyItem.Owner);
                                itemSentenceRun.AddMediaRun(mediaRun);
                            }
                        }
                        else
                        {
                            mediaRun = new MediaRun("Audio", fileName, studyItem.Owner);
                            itemSentenceRun.AddMediaRun(mediaRun);
                        }
                    }
                }
                else if (isSynthesizeMissingAudio)
                {
                    returnValue = AddSynthesizedVoiceDefault(
                        text,
                        filePath,
                        voiceLanguageID) && returnValue;

                    if (returnValue)
                    {
                        mediaRun = itemSentenceRun.GetMediaRun("Audio");

                        if (mediaRun != null)
                        {
                            if (mediaRun.FileName != fileName)
                            {
                                itemSentenceRun.DeleteMediaRun(mediaRun);
                                mediaRun = new MediaRun("Audio", fileName, studyItem.Owner);
                                itemSentenceRun.AddMediaRun(mediaRun);
                            }
                        }
                        else
                        {
                            mediaRun = new MediaRun("Audio", fileName, studyItem.Owner);
                            itemSentenceRun.AddMediaRun(mediaRun);
                        }
                    }
                }

                if ((mediaRun != null) && (familyLanguageIDs != null) && (familyLanguageIDs.Count() != 0))
                    returnValue = studyItem.PropogateMediaRun(familyLanguageIDs, sentenceIndex, mediaRun) && returnValue;
            }

            return returnValue;
        }

        public bool GetPictureForStudyItem(
            MultiLanguageItem studyItem,
            List<LanguageID> languageIDs,
            bool useRemoteMedia)
        {
            bool returnValue = true;

            foreach (LanguageID languageID in languageIDs)
            {
                if (!GetPictureForStudyItemLanguage(studyItem, languageID, useRemoteMedia))
                    returnValue = false;
            }

            return returnValue;
        }

        public bool GetPictureForStudyItemLanguage(
            MultiLanguageItem studyItem,
            LanguageID languageID,
            bool useRemoteMedia)
        {
            LanguageItem languageItem = studyItem.LanguageItem(languageID);

            if (languageItem == null)
                return false;

            if (!languageItem.HasText())
                return true;

            string text = languageItem.Text;
            string alternateKey = null;
            LanguageID alternateLanguageID = null;
            string pictureSourceUrl = null;
            bool returnValue;

            GetStudyItemAlternateKeyAndLanguage(studyItem, languageID, 0, out alternateKey, out alternateLanguageID);

            returnValue = ProcessPicture(text, languageID, alternateKey, alternateLanguageID,
                false, null, useRemoteMedia, null, out pictureSourceUrl);

            if (returnValue)
            {
                string studyItemKey = studyItem.KeyString;
                int sentenceRunIndex = 0;
                string fileName = MediaUtilities.ComposeStudyItemFileName(
                    studyItemKey, sentenceRunIndex, languageID, "Picture", ".jpg");
                string urlDirectory = studyItem.MediaTildeUrl;
                string urlPath = MediaUtilities.ConcatenateUrlPath(urlDirectory, fileName);
                string filePath = ApplicationData.MapToFilePath(urlPath);
                string pictureSourceFilePath = ApplicationData.MapToFilePath(pictureSourceUrl);

                try
                {
                    FileSingleton.Copy(pictureSourceFilePath, filePath);
                }
                catch (Exception exc)
                {
                    PutExceptionError("Error copying dictionary picture", exc);
                    return false;
                }

                MediaRun mediaRun = new MediaRun("Picture", fileName, studyItem.Owner);
                TextRun sentenceRun = languageItem.GetSentenceRun(0);

                if (sentenceRun == null)
                {
                    sentenceRun = new TextRun(languageItem, mediaRun);
                    languageItem.AddSentenceRun(sentenceRun);
                }
                else
                    sentenceRun.AddMediaRun(mediaRun);
            }

            return returnValue;
        }

        public bool GetStudyItemAlternateKeyAndLanguage(
            MultiLanguageItem studyItem,
            LanguageID languageID,
            int sentenceIndex,
            out string alternateKey,
            out LanguageID alternateLanguageID)
        {
            List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(languageID);

            alternateKey = null;
            alternateLanguageID = null;

            if (alternateLanguageIDs == null)
                return false;

            foreach (LanguageID altID in alternateLanguageIDs)
            {
                if (studyItem.HasText(altID))
                {
                    alternateKey = studyItem.RunText(altID, sentenceIndex);
                    alternateLanguageID = altID;
                    return true;
                }
            }

            return false;
        }

        public bool CheckDictionaryEntriesAndAudio(
            LanguageID targetLanguageID,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            string userName,
            List<string> dictionaryWordsList,
            List<string> audioKeys,
            out string errorMessage)
        {
            bool returnValue = true;

            errorMessage = null;

            if ((dictionaryWordsList != null) && (dictionaryWordsList.Count() != 0))
                returnValue = CheckDictionaryEntries(
                    targetLanguageID,
                    targetLanguageIDs,
                    hostLanguageIDs,
                    userName,
                    dictionaryWordsList) && returnValue;

            if ((audioKeys != null) && (audioKeys.Count() != 0))
                returnValue = CheckDictionaryAudio(
                    targetLanguageID,
                    audioKeys) && returnValue;

            return returnValue;
        }

        public bool CheckDictionaryEntries(
            LanguageID targetLanguageID,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            string userName,
            List<string> dictionaryWordsList)
        {
            List<LanguageID> languageIDs = new List<LanguageID>();
            List<DictionaryEntry> dictionaryEntries = new List<DictionaryEntry>();
            bool returnValue = true;

            if (targetLanguageIDs != null)
                languageIDs.AddRange(targetLanguageIDs);

            if (hostLanguageIDs != null)
                ObjectUtilities.ListAddUniqueList(languageIDs, hostLanguageIDs);

            foreach (string word in dictionaryWordsList)
            {
                DictionaryEntry dictionaryEntry;
                List<DictionaryEntry> dictionaryEntriesOne = Repositories.Dictionary.Lookup(
                    word,
                    MatchCode.Exact,
                    targetLanguageID,
                    0,
                    0);
                if ((dictionaryEntriesOne == null) || (dictionaryEntriesOne.Count() == 0))
                {
                    MultiLanguageItem mli = new MultiLanguageItem(0, targetLanguageIDs, hostLanguageIDs);
                    string errorMessage;
                    mli.SetText(targetLanguageID, word);
                    if (Translator.TranslateMultiLanguageItem(
                        mli,
                        languageIDs,
                        false,
                        false,
                        out errorMessage,
                        false))
                    {
                        dictionaryEntry = new DictionaryEntry(word, targetLanguageID);

                        foreach (LanguageID senseLanguageID in hostLanguageIDs)
                        {
                            if (!AddSenseToDictionaryEntry(
                                    null,
                                    null,
                                    mli,
                                    dictionaryEntry,
                                    targetLanguageID,
                                    hostLanguageIDs,
                                    0,
                                    LexicalCategory.Unknown,
                                    null,
                                    false,
                                    false,
                                    ref errorMessage))
                                returnValue = false;
                        }

                        dictionaryEntries.Add(dictionaryEntry);
                    }
                }
                else
                {
                    dictionaryEntry = dictionaryEntriesOne.First();
                    dictionaryEntries.Add(dictionaryEntry);
                }
            }

            try
            {
                returnValue = Repositories.Dictionary.AddList(dictionaryEntries, targetLanguageID);
            }
            catch (Exception exc)
            {
                PutExceptionError(
                    "Error saving dictionary entries for",
                    targetLanguageID.LanguageName(UILanguageID),
                    exc);
                returnValue = false;
            }

            return returnValue;
        }

        public bool CheckDictionaryEntriesAudio(
            List<DictionaryEntry> dictionaryEntries)
        {
            bool returnValue = true;

            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
            {
                if (!CheckDictionaryEntryAudioOne(dictionaryEntry))
                    returnValue = false;
            }

            return returnValue;
        }

        public bool CheckDictionaryEntryAudioOne(
            DictionaryEntry dictionaryEntry)
        {
            if (dictionaryEntry == null)
                return false;
            bool returnValue = true;
            string audioUrl;
            string audioKey = dictionaryEntry.KeyString;
            LanguageID alternateLanguageID = null;
            LanguageID targetLanguageID = dictionaryEntry.LanguageID;
            string alternateKey = null;
            if (!LanguageLookup.IsNonRomanizedAlternatePhonetic(targetLanguageID) && dictionaryEntry.HasAlternates())
            {
                foreach (LanguageString alternate in dictionaryEntry.Alternates)
                {
                    if (LanguageLookup.IsNonRomanizedAlternatePhonetic(alternate.LanguageID))
                    {
                        targetLanguageID = alternate.LanguageID;
                        audioKey = alternate.Text;
                        alternateKey = null;
                        alternateLanguageID = null;
                        break;
                    }
                    else if (String.IsNullOrEmpty(alternateKey))
                    {
                        alternateKey = alternate.Text;
                        alternateLanguageID = alternate.LanguageID;
                    }
                }
            }
            // Returns true if audio file exists.
            if (!ProcessAudio(
                    audioKey,           // string normal key
                    targetLanguageID,   // LanguageID languageID
                    alternateKey,       // string alternateKey
                    null,               // LanguageID alternateLanguageID
                    false,              // bool exisitingOnly
                    String.Empty,       // string mainAudioUrl
                    true,               // bool isSynthesizeMissingAudio
                    false,              // bool useRemoteMedia
                    null,               // List<LanguageString> audioRecordKeys
                    out audioUrl))      // out string audioUrl
                returnValue = false;
            return returnValue;
        }

        public bool CheckDictionaryAudio(
            LanguageID targetLanguageID,
            List<string> audioKeys)
        {
            bool returnValue = true;

            foreach (string audioKey in audioKeys)
            {
                if (!CheckDictionaryAudioOne(targetLanguageID, audioKey))
                    returnValue = false;
            }

            return returnValue;
        }

        public bool CheckDictionaryAudioOne(
            LanguageID targetLanguageID,
            string audioKey)
        {
            bool returnValue = true;
            string audioUrl;
            // Returns true if audio file exists.
            if (!ProcessAudio(
                    audioKey,           // string normal key
                    targetLanguageID,   // LanguageID languageID
                    String.Empty,       // string alternateKey
                    null,               // LanguageID alternateLanguageID
                    false,              // bool exisitingOnly
                    String.Empty,       // string mainAudioUrl
                    true,               // bool isSynthesizeMissingAudio
                    false,              // bool useRemoteMedia
                    null,               // List<LanguageString> audioRecordKeys
                    out audioUrl))      // out string audioUrl
                returnValue = false;
            return returnValue;
        }

        public void LoadToolStudyListInflections(
            ToolSession toolSession,
            ToolStudyList toolStudyList,
            LanguageID targetLanguageID)
        {
            List<ToolStudyItem> inflectionToolStudyItems = new List<ToolStudyItem>();
            List<ToolStudyItem> sourceToolStudyItems = toolStudyList.ToolStudyItems;
            if (sourceToolStudyItems == null)
                return;
            LanguageTool languageTool = GetLanguageTool(targetLanguageID);
            MultiLanguageTool multiLanguageTool = GetMultiLanguageTool(
                targetLanguageID,
                UserProfile.HostLanguageIDs);
            DictionaryEntry dictionaryEntry = null;
            int senseIndex = -1;
            int synonymIndex = -1;
            string categoryString = null;
            int studyItemOrdinal = sourceToolStudyItems.Count();

            foreach (ToolStudyItem toolStudyItem in sourceToolStudyItems)
            {
                MultiLanguageItem studyItem = toolStudyItem.StudyItem;
                LexicalCategory category = GetInflectableStudyItemCategory(
                    studyItem,
                    targetLanguageID,
                    languageTool,
                    out dictionaryEntry,
                    out senseIndex,
                    out synonymIndex,
                    out categoryString);

                toolStudyItem.Category = category;
                toolStudyItem.CategoryString = categoryString;

                if ((category == LexicalCategory.Unknown) || !languageTool.CanInflectCategory(category))
                    continue;

                List<Inflection> inflections = multiLanguageTool.InflectAnyDictionaryFormAll(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex);

                if (inflections == null)
                    continue;

                foreach (Inflection inflection in inflections)
                {
                    string key = "I" + studyItemOrdinal++;
                    MultiLanguageString output = inflection.PronounOutput;
                    MultiLanguageItem sourceStudyItem = toolStudyItem.StudyItem;
                    MultiLanguageItem inflectionStudyItem = new MultiLanguageItem(key, output);
                    inflectionStudyItem.Content = sourceStudyItem.Content;
                    ToolStudyItem inflectionToolStudyItem = new ToolStudyItem(key, toolStudyItem);
                    inflectionToolStudyItem.InflectionStudyItem = inflectionStudyItem;
                    inflectionToolStudyItem.StudyItem = inflectionStudyItem;
                    inflectionToolStudyItem.ContentStudyItemKey = sourceStudyItem.KeyString;
                    if (sourceStudyItem.StudyListKey != null)
                        inflectionToolStudyItem.ContentStudyListKey = (int)sourceStudyItem.StudyListKey;
                    inflectionToolStudyItem.Designation = inflection.Designation;
                    inflectionToolStudyItem.Category = inflection.Category;
                    inflectionToolStudyItem.CategoryString = inflection.CategoryString;
                    inflectionStudyItem.Modified = false;
                    inflectionToolStudyItem.Modified = false;
                    inflectionToolStudyItems.Add(inflectionToolStudyItem);
                }
            }

            toolStudyList.InflectionToolStudyItems = inflectionToolStudyItems;
        }

        public LexicalCategory GetInflectableStudyItemCategory(
            MultiLanguageItem studyItem,
            LanguageID targetLanguageID,
            LanguageTool languageTool,
            out DictionaryEntry dictionaryEntry,
            out int senseIndex,
            out int synonymIndex,
            out string categoryString)
        {
            LexicalCategory category = LexicalCategory.Unknown;
            LexicalCategory testCategory = LexicalCategory.Unknown;
            Annotation categoryAnnotation = studyItem.FindAnnotation("Category");

            dictionaryEntry = null;
            senseIndex = -1;
            synonymIndex = -1;
            categoryString = null;

            string word = studyItem.Text(targetLanguageID);

            if (String.IsNullOrEmpty(word))
                return category;

            List<DictionaryEntry> dictionaryEntries = languageTool.LookupDictionaryEntriesExact(
                word,
                languageTool.TargetLanguageIDs);

            if (categoryAnnotation != null)
                testCategory = Sense.GetLexicalCategoryFromString(categoryAnnotation.Value);

            if ((dictionaryEntries == null) || (dictionaryEntries.Count() == 0))
                return category;

            bool found = false;

            foreach (DictionaryEntry testDictionaryEntry in dictionaryEntries)
            {
                senseIndex = 0;

                foreach (Sense sense in testDictionaryEntry.Senses)
                {
                    if (sense.Category == LexicalCategory.Unknown)
                        continue;

                    if (sense.Category == LexicalCategory.Inflection)
                    {
                        Inflection inflection = sense.GetInflectionIndexed(0);

                        if (inflection != null)
                        {
                            category = inflection.Category;
                            categoryString = inflection.CategoryString;
                        }

                        if ((testCategory == LexicalCategory.Unknown) || (testCategory == category))
                        {
                            found = true;
                            break;
                        }
                    }
                    else if (languageTool.CanInflectCategory(sense.Category))
                    {
                        category = sense.Category;
                        categoryString = sense.CategoryString;

                        if ((testCategory == LexicalCategory.Unknown) || (testCategory == category))
                        {
                            found = true;
                            break;
                        }
                    }

                    senseIndex++;
                }

                if (found)
                {
                    dictionaryEntry = testDictionaryEntry;
                    break;
                }
            }

            if (!found)
            {
                dictionaryEntry = dictionaryEntries.First();
                senseIndex = 0;
                synonymIndex = 0;

                if (dictionaryEntry.SenseCount != 0)
                {
                    category = dictionaryEntry.GetSenseIndexed(0).Category;
                    categoryString = dictionaryEntry.GetSenseIndexed(0).CategoryString;
                }
            }

            /* Don't add Category annotation.
            if ((categoryAnnotation == null) && found && (category != LexicalCategory.Unknown))
            {
                string englishCategory = category.ToString();
                List<LanguageID> languageIDs = studyItem.LanguageIDs;
                MultiLanguageString categoryMLS = new MultiLanguageString(
                    null,
                    languageIDs);

                foreach (LanguageID languageID in languageIDs)
                {
                    BaseString translatedCategory = LanguageUtilities.TranslateString(
                        englishCategory,
                        englishCategory,
                        languageID,
                        Repositories.UIStrings);

                    if (translatedCategory != null)
                        categoryMLS.SetText(languageID, translatedCategory.Text);
                }

                categoryAnnotation = new Annotation(
                    "Category",
                    englishCategory,
                    categoryMLS);

                studyItem.AddAnnotation(categoryAnnotation);
            }
            */

            return category;
        }

        public void DumpDictionaryStatistics(
            LanguageID languageID,
            List<DictionaryEntry> entries)
        {
            int dummy;
            KeyValuePairStringIntComparer comparer = new KeyValuePairStringIntComparer();
            Dictionary<string, int> statsCategory = new Dictionary<string, int>();
            Dictionary<string, int> statsEDict = new Dictionary<string, int>();
            Dictionary<string, int> statsSenses = new Dictionary<string, int>();
            Dictionary<string, int> statsSynonyms = new Dictionary<string, int>();
            string languageName = languageID.LanguageName(LanguageLookup.English);
            int entryCount = entries.Count;
            int senseCount = 0;

            DumpString("Stats for " + languageName + ":");
            DumpString("    Entry count:                    " + entryCount.ToString());

            foreach (DictionaryEntry entry in entries)
            {
                senseCount += entry.SenseCount;

                foreach (Sense sense in entry.Senses)
                {
                    LexicalCategory category = sense.Category;
                    string categoryKey = category.ToString();
                    string categoryString = sense.CategoryString;

                    foreach (LanguageSynonyms languageSynomyns in sense.LanguageSynonyms)
                    {
                        LanguageID senseLanguageID = languageSynomyns.LanguageID;
                        string senseLanguageName = senseLanguageID.LanguageName(LanguageLookup.English);
                        if (statsSenses.TryGetValue(senseLanguageName, out dummy))
                            statsSenses[senseLanguageName]++;
                        else
                            statsSenses.Add(senseLanguageName, 1);
                        if (statsSynonyms.TryGetValue(senseLanguageName, out dummy))
                            statsSynonyms[senseLanguageName] += languageSynomyns.SynonymCount;
                        else
                            statsSynonyms.Add(senseLanguageName, languageSynomyns.SynonymCount);
                        if (senseLanguageID == LanguageLookup.English)
                        {
                            if (statsCategory.TryGetValue(categoryKey, out dummy))
                                statsCategory[categoryKey]++;
                            else
                                statsCategory.Add(categoryKey, 1);
                            if (!String.IsNullOrEmpty(categoryString))
                            {
                                string[] parts = categoryString.Split(LanguageLookup.Comma);
                                foreach (string pos in parts)
                                {
                                    if (statsEDict.TryGetValue(pos, out dummy))
                                        statsEDict[pos]++;
                                    else
                                        statsEDict.Add(pos, 1);
                                }
                            }
                        }
                    }
                }
            }

            LanguageDescription languageDescription = LanguageLookup.GetLanguageDescription(languageID);

            DumpString("    Longest entry key:              " + languageDescription.LongestDictionaryEntryLength.ToString());
            DumpString("    Longest prefix:                 " + languageDescription.LongestPrefixLength.ToString());
            DumpString("    Longest suffix:                 " + languageDescription.LongestSuffixLength.ToString());
            DumpString("    Longest inflection:             " + languageDescription.LongestInflectionLength.ToString());
            DumpString("    Entry sense count:              " + senseCount.ToString());

            foreach (KeyValuePair<string, int> senseKvp in statsSenses)
            {
                string senseLanguageName = senseKvp.Key;
                int languageSenseCount = senseKvp.Value;
                int synonymCount = statsSynonyms[senseLanguageName];
                string languageField = String.Format("{0,-28}", senseLanguageName + ":");
                DumpString("        " + languageField +
                    languageSenseCount.ToString() + " senses " +
                    synonymCount.ToString() + " synonyms");
                if (senseLanguageName == "English")
                {
                    DumpString("        Lexical categories:");
                    List<KeyValuePair<string, int>> categories = statsCategory.ToList();
                    categories.Sort(comparer);
                    foreach (KeyValuePair<string, int> kvpCategory in categories)
                    {
                        string categoryName = kvpCategory.Key;
                        int categoryCount = kvpCategory.Value;
                        string categoryField = String.Format("{0,-24}", categoryName + ":");
                        DumpString("            " + categoryField + categoryCount.ToString());
                    }
                    DumpString("        Category string keywords:");
                    categories = statsEDict.ToList();
                    categories.Sort(comparer);
                    foreach (KeyValuePair<string, int> kvpEDict in categories)
                    {
                        string keyWordName = kvpEDict.Key;
                        int keyWordCount = kvpEDict.Value;
                        string keyWordField = String.Format("{0,-24}", keyWordName + ":");
                        DumpString("            " + keyWordField + keyWordCount.ToString());
                    }
                }
            }
        }
    }
}
