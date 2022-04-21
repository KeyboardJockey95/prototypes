using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatDictionary : Format
    {
        // Infrastructure data.
        protected enum LanguageConfigurationCode
        {
            NoAlternates,
            SimpleRomanized,
            Character1Phonetic1Romanized1,
            Character2Romanized1
        };
        protected LanguageConfigurationCode LanguageConfiguration;
        protected bool IsAddReciprocals = false;
        protected bool DoStems = false;
        protected bool UseCache = true;
        protected List<Dictionary<string, DictionaryEntry>> CacheList = new List<Dictionary<string, DictionaryEntry>>();
        protected List<List<DictionaryEntry>> EntriesList = new List<List<DictionaryEntry>>();
        protected List<Dictionary<string, DictionaryEntry>> StemsCacheList = new List<Dictionary<string, DictionaryEntry>>();
        protected List<List<DictionaryEntry>> StemsEntriesList = new List<List<DictionaryEntry>>();
        protected int FileSize = 0;
        protected int BytesRead = 0;
        protected int EntryIndex = 0;
        protected int ErrorCount;
        protected enum StateCode
        {
            Idle,
            Reading,
            DisplayConversion,
            WritingDataBase,
            WritingXml,
            AddingAudio,
            Completed
        };
        protected StateCode State = StateCode.Idle;
        protected string OperationLanguageName;
        protected int OperationItemCount;
        protected int OperationItemIndex;

        // Current session information.
        protected DictionaryRepository DictionaryRepository;
        protected DictionaryRepository DictionaryStemsRepository;
        protected DictionaryRepository TemporaryDictionaryRepository;
        protected DictionaryRepository TemporaryDictionaryStemsRepository;
        protected int TargetCount;
        protected int HostCount;
        protected int UniqueLanguageCount;
        protected StreamWriter RawDisplayWriter;

        // Argument data.
        protected string DisplayFilePath { get; set; }
        protected static string DisplayFilePathPrompt = "Display file path";
        protected string DisplayFilePathHelp = "Enter the file path for the dictionary display output.";

        protected string RawDisplayFilePath { get; set; }
        protected static string RawDisplayFilePathPrompt = "Raw display file path";
        protected string RawDisplayFilePathHelp = "Enter the file path for displaying the raw input for debugging.";

        public FormatDictionary(
                string name,
                string type,
                string description,
                string mimeType,
                string defaultExtension,
                UserRecord userRecord,
                UserProfile userProfile,
                IMainRepository repositories,
                LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base(
                name,
                type,
                description,
                "Dictionary",
                "Dictionary",
                mimeType,
                defaultExtension,
                userRecord,
                userProfile,
                repositories,
                languageUtilities,
                nodeUtilities)
        {
            ClearFormatDictionary();
            SetupLanguages();
        }

        public FormatDictionary(FormatDictionary other)
            : base(other)
        {
            CopyFormatDictionary(other);
        }

        public void ClearFormatDictionary()
        {
            LanguageConfiguration = LanguageConfigurationCode.NoAlternates;
            UseCache = true;
            CacheList = new List<Dictionary<string, DictionaryEntry>>();
            EntriesList = new List<List<DictionaryEntry>>();
            StemsCacheList = new List<Dictionary<string, DictionaryEntry>>();
            StemsEntriesList = new List<List<DictionaryEntry>>();
            FileSize = 0;
            BytesRead = 0;
            EntryIndex = 0;
            ErrorCount = 0;
            State = StateCode.Idle;
            OperationLanguageName = String.Empty;
            OperationItemCount = 0;
            OperationItemIndex = 0;
            DisplayFilePath = null;
            RawDisplayFilePath = null;

            // Current session information.
            DictionaryRepository = null;
            DictionaryStemsRepository = null;
            TemporaryDictionaryRepository = null;
            TemporaryDictionaryStemsRepository = null;
            TargetCount = 0;
            HostCount = 0;
            UniqueLanguageCount = 0;
            RawDisplayWriter = null;

            Timer = new SoftwareTimer();
        }

        public void CopyFormatDictionary(FormatDictionary other)
        {
            LanguageConfiguration = other.LanguageConfiguration;
            UseCache = other.UseCache;
            CacheList = new List<Dictionary<string, DictionaryEntry>>();
            EntriesList = new List<List<DictionaryEntry>>();
            StemsCacheList = new List<Dictionary<string, DictionaryEntry>>();
            StemsEntriesList = new List<List<DictionaryEntry>>();
            FileSize = 0;
            BytesRead = 0;
            EntryIndex = 0;
            ErrorCount = 0;
            State = StateCode.Idle;
            OperationLanguageName = String.Empty;
            OperationItemCount = 0;
            OperationItemIndex = 0;
            DisplayFilePath = null;
            RawDisplayFilePath = null;

            // Current session information.
            DictionaryRepository = null;
            DictionaryStemsRepository = null;
            TemporaryDictionaryRepository = null;
            TemporaryDictionaryStemsRepository = null;
            TargetCount = other.TargetCount;
            HostCount = other.HostCount;
            UniqueLanguageCount = other.UniqueLanguageCount;
            RawDisplayWriter = null;
            Timer = new SoftwareTimer();
        }

        public override void SetupLanguages()
        {
            base.SetupLanguages();
            InitializeLanguageConfiguration();
        }

        protected void InitializeLanguageConfiguration()
        {
            if ((TargetLanguageIDs == null) || (TargetLanguageIDs.Count() == 0))
                return;

            LanguageID targetLanguageID = TargetLanguageIDs.First();

            if (targetLanguageID == LanguageLookup.Japanese)
                LanguageConfiguration = LanguageConfigurationCode.Character1Phonetic1Romanized1;
            else if ((targetLanguageID == LanguageLookup.ChineseSimplified) || (targetLanguageID == LanguageLookup.ChineseTraditional))
                LanguageConfiguration = LanguageConfigurationCode.Character2Romanized1;
            else if (LanguageLookup.HasAlternatePhonetic(targetLanguageID))
                LanguageConfiguration = LanguageConfigurationCode.SimpleRomanized;
            else
                LanguageConfiguration = LanguageConfigurationCode.NoAlternates;
        }

        protected virtual void InitializeRepositories()
        {
            if (Repositories == null)
                throw new Exception("Repositories not set in format.");

            DictionaryRepository = Repositories.Dictionary;
            DictionaryStemsRepository = Repositories.DictionaryStems;

            if (UseCache)
            {
                LoadCache();
                TemporaryDictionaryRepository = null;
                TemporaryDictionaryStemsRepository = null;
            }
            else
            {
                LanguageObjectStore temporaryObjectStore = new LanguageObjectStore();
                TemporaryDictionaryRepository = new DictionaryRepository(temporaryObjectStore);
                TemporaryDictionaryStemsRepository = new DictionaryRepository(temporaryObjectStore);
            }
        }

        protected void LoadCache()
        {
            List<LanguageID> languageIDs;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            // Load regular entries cache.
            LoadCacheHelper(
                "regular",
                DictionaryRepository,
                CacheList,
                EntriesList,
                languageIDs);

            // Load stem entries cache.
            LoadCacheHelper(
                "stem",
                DictionaryStemsRepository,
                StemsCacheList,
                StemsEntriesList,
                languageIDs);
        }

        protected void LoadCacheHelper(
            string label,
            DictionaryRepository distionaryRepository,
            List<Dictionary<string, DictionaryEntry>> cacheList,
            List<List<DictionaryEntry>> entriesList,
            List<LanguageID> languageIDs)
        {
            foreach (LanguageID languageID in languageIDs)
            {
                PutStatusMessage("Loading " + label + " cache for " + languageID.LanguageName(UILanguageID));

                Dictionary<string, DictionaryEntry> dictionary = new Dictionary<string, DictionaryEntry>(
                    (IEqualityComparer<string>)ApplicationData.GetLanguageComparer<string>(languageID, true));
                List<DictionaryEntry> list = distionaryRepository.GetAll(languageID);

                if (list == null)
                    throw new Exception("No " + label + " dictionary list for " + languageID.LanguageName(UILanguageID));

                int index = 0;

                if (DeleteBeforeImport && HostLanguageIDs.Contains(languageID))
                {
                    int count = list.Count();
                    List<DictionaryEntry> deleteList = new List<DictionaryEntry>();

                    for (index = count - 1; index >= 0; index--)
                    {
                        DictionaryEntry entry = list[index];
                        int senseCount = entry.SenseCount;

                        if (senseCount != 0)
                        {
                            for (int senseIndex = senseCount - 1; senseIndex >= 0; senseIndex--)
                            {
                                Sense sense = entry.GetSenseIndexed(senseIndex);
                                int languageSynonymCount = sense.LanguageSynonymsCount;

                                if (languageSynonymCount == 0)
                                {
                                    entry.Senses.RemoveAt(senseIndex);
                                    senseCount--;
                                }
                                else
                                {
                                    for (int languageSynonymIndex = languageSynonymCount - 1; languageSynonymIndex >= 0; languageSynonymIndex--)
                                    {
                                        LanguageSynonyms languageSynonyms = sense.GetLanguageSynonymsIndexed(languageSynonymIndex);

                                        if (TargetLanguageIDs.Contains(languageSynonyms.LanguageID))
                                        {
                                            sense.LanguageSynonyms.RemoveAt(languageSynonymIndex);
                                            languageSynonymCount--;
                                        }
                                    }

                                    if (languageSynonymCount == 0)
                                    {
                                        entry.Senses.RemoveAt(senseIndex);
                                        senseCount--;
                                    }
                                }
                            }
                        }

                        if (senseCount == 0)
                        {
                            deleteList.Add(entry);
                            list.RemoveAt(index);
                        }
                    }

                    if (deleteList.Count() != 0)
                    {
                        if (!distionaryRepository.DeleteList(deleteList, languageID))
                            throw new Exception("Error deleting " + deleteList.Count().ToString() + " " + label + " entries for " + languageID.LanguageName(UILanguageID));
                    }
                }

                foreach (DictionaryEntry entry in list)
                {
                    try
                    {
                        dictionary.Add(entry.KeyString, entry);
                    }
                    catch (Exception exc)
                    {
                        PutExceptionError("Loading " + label + " cache entry for " + languageID.LanguageName(UILanguageID) + " " + entry.KeyString, exc);
                    }

                    index++;
                }

                cacheList.Add(dictionary);
                entriesList.Add(list);
            }
        }

        protected virtual void InitializeRawDisplayOutput()
        {
            if (String.IsNullOrEmpty(RawDisplayFilePath))
                return;

            try
            {
                RawDisplayWriter = FileSingleton.CreateText(RawDisplayFilePath);
            }
            catch (Exception exc)
            {
                PutExceptionError("Exception creating raw display writer", exc);
            }
        }

        protected virtual void ResetRawDisplayOutput()
        {
            if (RawDisplayWriter == null)
                return;

            try
            {
                FileSingleton.Close(RawDisplayWriter);
            }
            catch (Exception exc)
            {
                PutExceptionError("Exception closing raw display writer", exc);
            }
        }

        public virtual void PutRawDisplayLine(string text)
        {
            if (RawDisplayWriter == null)
                return;

            RawDisplayWriter.WriteLine(text);
        }

        public virtual void PutRawDisplay(string text)
        {
            if (RawDisplayWriter == null)
                return;

            RawDisplayWriter.Write(text);
        }

        public override void DeleteFirst()
        {
            if (DictionaryRepository != null)
            {
                if (TargetLanguageIDs != null)
                {
                    foreach (LanguageID languageID in TargetLanguageIDs)
                    {
                        //DictionaryRepository.ObjectStore.GetObjectStore(languageID).CreateStoreCheck();
                        //DictionaryRepository.ObjectStore.GetObjectStore(languageID).DeleteStoreCheck();
                        //DictionaryRepository.ObjectStore.GetObjectStore(languageID).CreateStoreCheck();
                        DictionaryRepository.DeleteAll(languageID);
                    }
                }
            }
            if (DictionaryStemsRepository != null)
            {
                if (TargetLanguageIDs != null)
                {
                    foreach (LanguageID languageID in TargetLanguageIDs)
                    {
                        //DictionaryStemsRepository.ObjectStore.GetObjectStore(languageID).CreateStoreCheck();
                        //DictionaryStemsRepository.ObjectStore.GetObjectStore(languageID).DeleteStoreCheck();
                        //DictionaryStemsRepository.ObjectStore.GetObjectStore(languageID).CreateStoreCheck();
                        DictionaryStemsRepository.DeleteAll(languageID);
                    }
                }
            }
        }

        protected override void PreRead(int progressCount)
        {
            if (Timer != null)
                Timer.Start();

            ContinueProgress(ProgressCountBase + progressCount);

            DeleteMediaFiles = GetUserOptionFlag("DeleteMediaFiles", true);

            DictionaryRepository = Repositories.Dictionary;
            DictionaryStemsRepository = Repositories.DictionaryStems;

            if (DeleteBeforeImport)
            {
                UpdateProgressElapsed("Deleting prior entries ...");
                DeleteFirst();
            }
            else
                UpdateProgressElapsed("Skipping deletion of prior entries ...");

            TargetCount = TargetLanguageIDs.Count;
            HostCount = HostLanguageIDs.Count;
            UniqueLanguageCount = UniqueLanguageIDs.Count;

            UpdateProgressElapsed("Loading cache ...");

            InitializeRepositories();

            FileSize = 0;
            BytesRead = 0;
            EntryIndex = 0;
            ErrorCount = 0;
            State = StateCode.Idle;
            OperationLanguageName = String.Empty;
            OperationItemCount = 0;
            OperationItemIndex = 0;
            LineNumber = 0;

            InitializeRawDisplayOutput();
        }

        protected override void PostRead()
        {
            ResetRawDisplayOutput();
            UpdateProgressElapsed("Collecting lengths ...");
            CollectLengths();
            UpdateProgressElapsed(GetCompletedMessage());
            EndContinuedProgress();
        }

        protected virtual void ReadLine(string line)
        {
            CheckForCancel();

            if (!String.IsNullOrEmpty(line))
                DispatchLine(line);

            BytesRead += ApplicationData.Encoding.GetBytes(line).Length + 1;
        }

        protected virtual void DispatchLine(string line)
        {
            if (!String.IsNullOrEmpty(line))
                ReadEntry(line);
        }

        protected virtual void ReadDirective(string line)
        {
        }

        protected virtual void ReadEntry(string line)
        {
        }

        protected virtual void AddSimpleTargetEntryRecord(
            FormatDictionaryRecord record)
        {
            AddSimpleTargetEntry(
                record.TargetText,
                record.TargetLanguageID,
                record.IpaReading,
                record.SourceIDs,
                record.HostSynonyms,
                record.HostLanguageID,
                record.Category,
                record.CategoryString,
                record.Attributes,
                record.Priority,
                record.Examples,
                out record.Entry);
        }

        protected virtual void AddSimpleTargetEntry(
            string targetText,
            LanguageID targetLanguageID,
            string ipaReading,
            List<int> sourceIDs,
            List<string> hostSynonyms,
            LanguageID hostLanguageID,
            LexicalCategory category,
            string categoryString,
            List<LexicalAttribute> attributes,
            int priority,
            List<MultiLanguageString> examples,
            out DictionaryEntry dictionaryEntry)
        {
            dictionaryEntry = null;

            if (hostSynonyms == null)
                return;

            List<LanguageString> alternates = null;
            Sense newSense;
            List<Sense> newSenses = new List<Sense>();
            List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>();
            LanguageTool toolHost = GetLanguageTool(hostLanguageID);

            //if ((targetText == "decir") || (targetText == "DECIR"))
            //    ApplicationData.Global.PutConsoleMessage(targetText);

            dictionaryEntry = GetDefinition(targetText, targetLanguageID);

            foreach (string hostSynonym in hostSynonyms)
            {
                if (dictionaryEntry != null)
                {
                    ProbableMeaning oldProbableSynonym;
                    int synonymIndex;

                    if (dictionaryEntry.GetProbableSynonymContainingText(
                        hostLanguageID,
                        hostSynonym,
                        out synonymIndex,
                        out oldProbableSynonym))
                    {
                        oldProbableSynonym.MergeSourceIDs(sourceIDs);
                        continue;
                    }
                }

                ProbableMeaning probableSynonym = new ProbableMeaning(
                    hostSynonym,
                    category,
                    categoryString,
                    float.NaN,
                    0,
                    sourceIDs);
                probableSynonym.Attributes = attributes;
                if (toolHost != null)
                    toolHost.FixupProbableMeaningCategories(
                        probableSynonym,
                        hostLanguageID);
                probableSynonyms.Add(probableSynonym);
            }

            if (probableSynonyms.Count() == 0)
                return;

            LanguageSynonyms languageSynonyms = new LanguageSynonyms(
                hostLanguageID,
                probableSynonyms);
            List<MediaRun> mediaRuns = null;
            int reading = 0;

            newSense = new Sense(
                reading,
                category,
                categoryString,
                priority,
                new List<LanguageSynonyms>(1) { languageSynonyms },
                examples);

            newSenses.Add(newSense);

            //if (targetText == "y")
            //    PutStatusMessage("y");

            if (dictionaryEntry == null)
            {
                dictionaryEntry = new DictionaryEntry(
                    targetText,
                    targetLanguageID,
                    alternates,
                    0,
                    sourceIDs,
                    newSenses,
                    mediaRuns);

                dictionaryEntry.IPAReading = ipaReading;

                dictionaryEntry.SortPriority();

                AddDefinition(dictionaryEntry);

                if (DoStems)
                {
                    for (int i = 0; i < newSenses.Count(); i++)
                    {
                        Sense sense = newSenses[i];
                        AddStemCheck(dictionaryEntry, sense);
                    }
                }
            }
            else
            {
                dictionaryEntry.SortProbableSynonymsBySourceCount(hostLanguageID);

                if (dictionaryEntry.KeyString != targetText)
                {
                    if (TextUtilities.IsLowerString(targetText))
                        dictionaryEntry.Key = targetText;
                }

                if (!String.IsNullOrEmpty(ipaReading))
                {
                    if (String.IsNullOrEmpty(dictionaryEntry.IPAReading))
                        dictionaryEntry.IPAReading = ipaReading;
                }

                dictionaryEntry.MergeSourceIDs(sourceIDs);

                if (dictionaryEntry.Senses == null)
                    dictionaryEntry.Senses = new List<Sense>();

                if (DoStems)
                {
                    for (int i = 0; i < newSenses.Count(); i++)
                    {
                        Sense sense = newSenses[i];
                        if (dictionaryEntry.AddUniqueSense(sense))
                            AddStemCheck(dictionaryEntry, sense);
                    }
                }
                else
                {
                    for (int i = 0; i < newSenses.Count(); i++)
                    {
                        Sense sense = newSenses[i];
                        dictionaryEntry.AddUniqueSense(sense);
                    }
                }

                if (dictionaryEntry.Modified)
                {
                    dictionaryEntry.TouchAndClearModified();
                    UpdateDefinition(dictionaryEntry);
                }
            }
        }

        protected virtual void AddTargetEntry(
            string key,
            LanguageID languageID,
            List<string> targetMeanings,
            List<int> sourceIDs,
            List<Sense> hostSenses,
            out DictionaryEntry dictionaryEntry)
        {
            List<LanguageString> alternates = null;
            List<Sense> newSenses = new List<Sense>();
            List<MediaRun> mediaRuns = null;
            int reading = 0;
            bool modified = false;

            key = key.Trim();

            dictionaryEntry = GetDefinition(key, languageID);

            if (dictionaryEntry == null)
            {
                if (TargetCount > 1)
                    alternates = new List<LanguageString>();
            }
            else
            {
                alternates = dictionaryEntry.Alternates;

                if ((TargetCount > 1) && (alternates == null))
                    dictionaryEntry.Alternates = alternates = new List<LanguageString>();

                dictionaryEntry.MergeSourceIDs(sourceIDs);
            }

            AddAlternates(
                languageID,
                targetMeanings,
                dictionaryEntry,
                out alternates,
                ref reading,
                ref modified);

            foreach (Sense sense in hostSenses)
            {
                Sense newSense = new Sense(sense);
                newSense.Reading = reading;
                newSenses.Add(newSense);
            }

            if (dictionaryEntry == null)
            {
                dictionaryEntry = new DictionaryEntry(
                    key,
                    languageID,
                    alternates,
                    0,
                    sourceIDs,
                    newSenses,
                    mediaRuns);
                dictionaryEntry.SortPriority();
                AddDefinition(dictionaryEntry);

                if (DoStems)
                {
                    for (int i = 0; i < newSenses.Count(); i++)
                    {
                        Sense newSense = newSenses[i];
                        AddStemCheck(dictionaryEntry, newSense);
                    }
                }
            }
            else
            {
                if (dictionaryEntry.Senses == null)
                    dictionaryEntry.Senses = new List<Sense>();

                if (DoStems)
                {
                    foreach (Sense sense in newSenses)
                    {
                        if (dictionaryEntry.AddUniqueSense(sense))
                            AddStemCheck(dictionaryEntry, sense);
                    }
                }
                else
                {
                    foreach (Sense sense in newSenses)
                        dictionaryEntry.AddUniqueSense(sense);
                }

                if (modified || dictionaryEntry.Modified)
                {
                    dictionaryEntry.TouchAndClearModified();
                    UpdateDefinition(dictionaryEntry);
                }
            }
        }

        protected virtual void AddStemCheck(DictionaryEntry dictionaryEntry, Sense sense)
        {
        }

        protected virtual void AddSimpleTargetStemEntry(
            string targetStem,
            string targetDictionaryForm,
            List<int> sourceIDs,
            LexicalCategory category,
            string categoryString,
            LanguageID targetLanguageID,
            string hostDictionaryForm,
            LanguageID hostLanguageID,
            out DictionaryEntry dictionaryEntry)
        {
            List<LanguageString> alternates = null;
            Sense newSense;
            List<Sense> newSenses = new List<Sense>(1);
            ProbableMeaning targetProbableSynonym = new ProbableMeaning(
                targetDictionaryForm,
                category,
                categoryString,
                float.NaN,
                0,
                sourceIDs);
            ProbableMeaning hostProbableSynonym = new ProbableMeaning(
                hostDictionaryForm,
                category,
                categoryString,
                float.NaN,
                0,
                sourceIDs);
            LanguageSynonyms targetLanguageSynonyms = new LanguageSynonyms(
                targetLanguageID,
                new List<ProbableMeaning>(1) { targetProbableSynonym });
            LanguageSynonyms hostLanguageSynonyms = new LanguageSynonyms(
                hostLanguageID,
                new List<ProbableMeaning>(1) { hostProbableSynonym });
            List<LanguageSynonyms> languageSynonymsList =
                new List<LanguageSynonyms>(2) { targetLanguageSynonyms, hostLanguageSynonyms };
            List<MediaRun> mediaRuns = null;
            int reading = 0;
            int priority = 0;
            List<MultiLanguageString> examples = null;

            dictionaryEntry = GetStemDefinition(targetStem, targetLanguageID);

            newSense = new Sense(
                reading,
                category,
                categoryString,
                priority,
                languageSynonymsList,
                examples);

            newSenses.Add(newSense);

            if (dictionaryEntry == null)
            {
                dictionaryEntry = new DictionaryEntry(
                    targetStem,
                    targetLanguageID,
                    alternates,
                    0,
                    sourceIDs,
                    newSenses,
                    mediaRuns);

                AddStemDefinition(dictionaryEntry);
            }
            else
            {
                dictionaryEntry.MergeSourceIDs(sourceIDs);

                if (dictionaryEntry.Senses == null)
                    dictionaryEntry.Senses = new List<Sense>();

                foreach (Sense sense in newSenses)
                    dictionaryEntry.AddUniqueSense(sense);

                if (dictionaryEntry.Modified)
                {
                    dictionaryEntry.TouchAndClearModified();
                    UpdateStemDefinition(dictionaryEntry);
                }
            }
        }

        protected virtual void AddSimpleTargetStemEntry(
            string targetStem,
            string targetDictionaryForm,
            List<int> sourceIDs,
            LexicalCategory category,
            string categoryString,
            LanguageID targetLanguageID,
            List<string> hostDictionaryForms,
            LanguageID hostLanguageID,
            out DictionaryEntry dictionaryEntry)
        {
            List<LanguageString> alternates = null;
            Sense newSense;
            List<Sense> newSenses = new List<Sense>(1);
            ProbableMeaning targetProbableSynonym = new ProbableMeaning(
                targetDictionaryForm,
                category,
                categoryString,
                float.NaN,
                0,
                sourceIDs);
            LanguageSynonyms targetLanguageSynonyms = new LanguageSynonyms(
                targetLanguageID,
                new List<ProbableMeaning>(1) { targetProbableSynonym });
            List<ProbableMeaning> hostProbableSynonyms = new List<ProbableMeaning>();
            if (hostDictionaryForms != null)
            {
                foreach (string hostDictionaryForm in hostDictionaryForms)
                {
                    ProbableMeaning hostProbableSynonym = new ProbableMeaning(
                        hostDictionaryForm,
                        category,
                        categoryString,
                        float.NaN,
                        0,
                        sourceIDs);
                    hostProbableSynonyms.Add(hostProbableSynonym);
                }
            }
            LanguageSynonyms hostLanguageSynonyms = new LanguageSynonyms(
                hostLanguageID,
                hostProbableSynonyms);
            List<LanguageSynonyms> languageSynonymsList =
                new List<LanguageSynonyms>(2) { targetLanguageSynonyms, hostLanguageSynonyms };
            List<MediaRun> mediaRuns = null;
            int reading = 0;
            int priority = 0;
            List<MultiLanguageString> examples = null;

            dictionaryEntry = GetStemDefinition(targetStem, targetLanguageID);

            newSense = new Sense(
                reading,
                category,
                categoryString,
                priority,
                languageSynonymsList,
                examples);

            newSenses.Add(newSense);

            if (dictionaryEntry == null)
            {
                dictionaryEntry = new DictionaryEntry(
                    targetStem,
                    targetLanguageID,
                    alternates,
                    0,
                    sourceIDs,
                    newSenses,
                    mediaRuns);

                AddStemDefinition(dictionaryEntry);
            }
            else
            {
                dictionaryEntry.MergeSourceIDs(sourceIDs);

                if (dictionaryEntry.Senses == null)
                    dictionaryEntry.Senses = new List<Sense>();

                foreach (Sense sense in newSenses)
                    dictionaryEntry.AddUniqueSense(sense);

                if (dictionaryEntry.Modified)
                {
                    dictionaryEntry.TouchAndClearModified();
                    UpdateStemDefinition(dictionaryEntry);
                }
            }
        }

        protected virtual void AddTargetStemEntry(
            string key,
            LanguageID languageID,
            List<string> targetMeanings,
            List<int> sourceIDs,
            List<Sense> hostSenses,
            out DictionaryEntry dictionaryEntry)
        {
            List<LanguageString> alternates = null;
            List<Sense> newSenses = new List<Sense>();
            List<MediaRun> mediaRuns = null;
            int reading = 0;
            bool modified = false;

            key = key.Trim();

            dictionaryEntry = GetStemDefinition(key, languageID);

            if (dictionaryEntry == null)
            {
                if (TargetCount > 1)
                    alternates = new List<LanguageString>();
            }
            else
            {
                alternates = dictionaryEntry.Alternates;

                if ((TargetCount > 1) && (alternates == null))
                    dictionaryEntry.Alternates = alternates = new List<LanguageString>();

                dictionaryEntry.MergeSourceIDs(sourceIDs);
            }

            AddAlternates(
                languageID,
                targetMeanings,
                dictionaryEntry,
                out alternates,
                ref reading,
                ref modified);

            foreach (Sense sense in hostSenses)
            {
                Sense newSense = new Sense(sense);
                newSense.Reading = reading;
                newSenses.Add(newSense);
            }

            if (dictionaryEntry == null)
            {
                dictionaryEntry = new DictionaryEntry(
                    key,
                    languageID,
                    alternates,
                    0,
                    sourceIDs,
                    newSenses,
                    mediaRuns);
                dictionaryEntry.SortPriority();
                AddStemDefinition(dictionaryEntry);
            }
            else
            {
                if ((dictionaryEntry.Senses == null) || (dictionaryEntry.Senses.Count() == 0))
                    dictionaryEntry.Senses = newSenses;
                else
                {
                    foreach (Sense newSense in newSenses)
                        dictionaryEntry.MergeOrAddSense(newSense);
                }

                if (modified || dictionaryEntry.Modified)
                {
                    dictionaryEntry.SortPriority();
                    dictionaryEntry.TouchAndClearModified();
                    UpdateStemDefinition(dictionaryEntry);
                }
            }
        }

        protected virtual void AddAlternates(
            LanguageID languageID,
            List<string> targetMeanings,
            DictionaryEntry dictionaryEntry,
            out List<LanguageString> alternates,
            ref int reading,
            ref bool modified)
        {
            switch (LanguageConfiguration)
            {
                case LanguageConfigurationCode.NoAlternates:
                    alternates = null;
                    break;
                case LanguageConfigurationCode.SimpleRomanized:
                    AddAlternatesSimpleRomanized(
                        languageID,
                        targetMeanings,
                        dictionaryEntry,
                        out alternates,
                        ref reading,
                        ref modified);
                    break;
                case LanguageConfigurationCode.Character1Phonetic1Romanized1:
                    AddAlternatesCharacter1Phonetic1Romanized1(
                        languageID,
                        targetMeanings,
                        dictionaryEntry,
                        out alternates,
                        ref reading,
                        ref modified);
                    break;
                case LanguageConfigurationCode.Character2Romanized1:
                    AddAlternatesCharacter2Romanized1(
                        languageID,
                        targetMeanings,
                        dictionaryEntry,
                        out alternates,
                        ref reading,
                        ref modified);
                    break;
                default:
                    alternates = null;
                    PutError("Unsupported language configuration.");
                    break;
            }
        }

        protected virtual void AddAlternatesSimpleRomanized(
            LanguageID languageID,
            List<string> targetMeanings,
            DictionaryEntry dictionaryEntry,
            out List<LanguageString> alternates,
            ref int reading,
            ref bool modified)
        {
            if (TargetCount != 3)
                throw new Exception("Wrong target language count for Simple.");

            alternates = null;

            if (dictionaryEntry != null)
                alternates = dictionaryEntry.Alternates;
        }

        protected virtual void AddAlternatesCharacter1Phonetic1Romanized1(
            LanguageID languageID,
            List<string> targetMeanings,
            DictionaryEntry dictionaryEntry,
            out List<LanguageString> alternates,
            ref int reading,
            ref bool modified)
        {
            if (TargetCount != 3)
                throw new Exception("Wrong target language count for Character1Phonetic1Romanized1.");

            LanguageString alternate;
            LanguageID characterLanguageID = TargetLanguageIDs[0];
            LanguageID phoneticLanguageID = TargetLanguageIDs[1];
            LanguageID romanizedLanguageID = TargetLanguageIDs[2];
            string characterMeaning = targetMeanings[0];
            string phoneticMeaning = targetMeanings[1];
            string romanizedMeaning = targetMeanings[2];

            alternates = null;

            if (dictionaryEntry == null)
            {
                if (TargetCount > 1)
                    alternates = new List<LanguageString>();
            }
            else
            {
                alternates = dictionaryEntry.Alternates;

                if ((TargetCount > 1) && (alternates == null))
                    dictionaryEntry.Alternates = alternates = new List<LanguageString>();
            }

            if (languageID == characterLanguageID)
            {
                if ((alternate = alternates.FirstOrDefault(x => (x.LanguageID == phoneticLanguageID) && (x.Text == phoneticMeaning))) == null)
                {
                    if (dictionaryEntry != null)
                        reading = dictionaryEntry.AllocateAlternateKey(phoneticLanguageID);
                    alternates.Add(new LanguageString(reading, phoneticLanguageID, phoneticMeaning));
                    alternates.Add(new LanguageString(reading, romanizedLanguageID, romanizedMeaning));
                    modified = true;
                }
                else
                    reading = alternate.KeyInt;
                /*
                if ((alternate = alternates.FirstOrDefault(x => (x.LanguageID == romanizedLanguageID) && (x.Text == romanizedMeaning))) == null)
                {
                    if (dictionaryEntry != null)
                        reading = dictionaryEntry.AllocateAlternateKey(romanizedLanguageID);
                    alternates.Add(new LanguageString(reading, romanizedLanguageID, romanizedMeaning));
                    modified = true;
                }
                else
                    reading = alternate.KeyInt;
                */
            }
            else if (languageID == phoneticLanguageID)
            {
                if ((alternate = alternates.FirstOrDefault(x => (x.LanguageID == characterLanguageID) && (x.Text == characterMeaning))) == null)
                {
                    if (dictionaryEntry != null)
                        reading = dictionaryEntry.AllocateAlternateKey(characterLanguageID);
                    alternates.Add(new LanguageString(reading, characterLanguageID, characterMeaning));
                    alternates.Add(new LanguageString(reading, romanizedLanguageID, romanizedMeaning));
                    modified = true;
                }
                else
                    reading = alternate.KeyInt;
                /*
                if ((alternate = alternates.FirstOrDefault(x => (x.LanguageID == romanizedLanguageID) && (x.Text == romanizedMeaning))) == null)
                {
                    if (dictionaryEntry != null)
                        reading = dictionaryEntry.AllocateAlternateKey(romanizedLanguageID);
                    alternates.Add(new LanguageString(reading, characterLanguageID, characterMeaning));
                    alternates.Add(new LanguageString(reading, romanizedLanguageID, romanizedMeaning));
                    modified = true;
                }
                else
                    reading = alternate.KeyInt;
                */
            }
            else
            {
                if ((alternate = alternates.FirstOrDefault(x => (x.LanguageID == characterLanguageID) && (x.Text == characterMeaning))) == null)
                {
                    if (dictionaryEntry != null)
                        reading = dictionaryEntry.AllocateAlternateKey(characterLanguageID);
                    alternates.Add(new LanguageString(reading, characterLanguageID, characterMeaning));
                    alternates.Add(new LanguageString(reading, phoneticLanguageID, phoneticMeaning));
                    modified = true;
                }
                else
                    reading = alternate.KeyInt;
                /*
                if ((alternate = alternates.FirstOrDefault(x => (x.LanguageID == phoneticLanguageID) && (x.Text == phoneticMeaning))) == null)
                {
                    if (dictionaryEntry != null)
                        reading = dictionaryEntry.AllocateAlternateKey(phoneticLanguageID);
                    alternates.Add(new LanguageString(reading, characterLanguageID, characterMeaning));
                    alternates.Add(new LanguageString(reading, phoneticLanguageID, phoneticMeaning));
                    modified = true;
                }
                else
                    reading = alternate.KeyInt;
                */
            }
        }

        protected virtual void AddAlternatesCharacter2Romanized1(
            LanguageID languageID,
            List<string> targetMeanings,
            DictionaryEntry dictionaryEntry,
            out List<LanguageString> alternates,
            ref int reading,
            ref bool modified)
        {
            if (TargetCount != 3)
                throw new Exception("Wrong target language count for Character1Phonetic1Romanized1.");

            LanguageString alternate;
            LanguageID character1LanguageID = TargetLanguageIDs[0];
            LanguageID character2LanguageID = TargetLanguageIDs[1];
            LanguageID romanizedLanguageID = TargetLanguageIDs[2];
            string character1Meaning = targetMeanings[0];
            string character2Meaning = targetMeanings[1];
            string romanizedMeaning = targetMeanings[2];

            alternates = null;

            if (dictionaryEntry == null)
            {
                if (TargetCount > 1)
                    alternates = new List<LanguageString>();
            }
            else
            {
                alternates = dictionaryEntry.Alternates;

                if ((TargetCount > 1) && (alternates == null))
                    dictionaryEntry.Alternates = alternates = new List<LanguageString>();
            }

            if (languageID == character1LanguageID)
            {
                if ((alternate = alternates.FirstOrDefault(x => (x.LanguageID == romanizedLanguageID) && (x.Text == romanizedMeaning))) == null)
                {
                    if (dictionaryEntry != null)
                        reading = dictionaryEntry.AllocateAlternateKey(romanizedLanguageID);
                    alternates.Add(new LanguageString(reading, character2LanguageID, character2Meaning));
                    alternates.Add(new LanguageString(reading, romanizedLanguageID, romanizedMeaning));
                    modified = true;
                }
                else
                    reading = alternate.KeyInt;
            }
            else if (languageID == character2LanguageID)
            {
                if ((alternate = alternates.FirstOrDefault(x => (x.LanguageID == romanizedLanguageID) && (x.Text == romanizedMeaning))) == null)
                {
                    if (dictionaryEntry != null)
                        reading = dictionaryEntry.AllocateAlternateKey(romanizedLanguageID);
                    alternates.Add(new LanguageString(reading, character1LanguageID, character1Meaning));
                    alternates.Add(new LanguageString(reading, romanizedLanguageID, romanizedMeaning));
                    modified = true;
                }
                else
                    reading = alternate.KeyInt;
            }
            else
            {
                if ((alternate = alternates.FirstOrDefault(x => (x.LanguageID == character1LanguageID) && (x.Text == character1Meaning))) == null)
                {
                    if (!String.IsNullOrEmpty(character2Meaning))
                    {
                        alternates.Add(new LanguageString(0, character1LanguageID, character1Meaning));
                        modified = true;
                    }
                }
                else
                    reading = alternate.KeyInt;
                if ((alternate = alternates.FirstOrDefault(x => (x.LanguageID == character2LanguageID) && (x.Text == character2Meaning))) == null)
                {
                    if (!String.IsNullOrEmpty(character2Meaning))
                    {
                        alternates.Add(new LanguageString(0, character2LanguageID, character2Meaning));
                        modified = true;
                    }
                }
                else
                    reading = alternate.KeyInt;
            }
        }

        protected virtual void AddHostEntry(
            string hostMeaning,
            LanguageID languageID,
            List<int> sourceIDs,
            List<string> targetMeanings,
            LexicalCategory category,
            string categoryString,
            int priorityLevel)
        {
            DictionaryEntry dictionaryEntry;
            List<LanguageString> alternates = null;
            List<Sense> senses;
            Sense sense;
            List<LanguageSynonyms> languageSynonymsList;
            LanguageSynonyms languageSynonyms;
            List<MultiLanguageString> examples = null;
            List<MediaRun> mediaRuns = null;
            int reading = 0;

            hostMeaning = hostMeaning.Trim();

            dictionaryEntry = GetDefinition(hostMeaning, languageID);

            if (dictionaryEntry == null)
                senses = new List<Sense>();
            else
            {
                senses = dictionaryEntry.Senses;

                if (senses == null)
                    dictionaryEntry.Senses = senses = new List<Sense>();
                else
                {
                    for (int targetIndex = 0; targetIndex < TargetCount; targetIndex++)
                    {
                        LanguageID targetLanguageID = TargetLanguageIDs[targetIndex];
                        string targetMeaning = targetMeanings[targetIndex];

                        if (dictionaryEntry.HasMeaning(targetMeaning, targetLanguageID))
                            return;

                        targetIndex++;
                    }
                }

                reading = dictionaryEntry.GetHighestAlternateKey();
            }

            languageSynonymsList = new List<LanguageSynonyms>();

            for (int targetIndex = 0; targetIndex < TargetCount; targetIndex++)
            {
                LanguageID targetLanguageID = TargetLanguageIDs[targetIndex];
                string targetMeaning = targetMeanings[targetIndex];
                ProbableMeaning targetProbableMeaning = new ProbableMeaning(
                    targetMeaning,
                    category,
                    categoryString,
                    float.NaN,
                    0,
                    sourceIDs);
                List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>(1) { targetProbableMeaning };
                languageSynonyms = new LanguageSynonyms(targetLanguageID, probableSynonyms);
                languageSynonymsList.Add(languageSynonyms);
            }

            sense = new Sense(
                reading,
                category,
                categoryString,
                priorityLevel,
                languageSynonymsList,
                examples);
            senses.Add(sense);

            if (dictionaryEntry == null)
            {
                dictionaryEntry = new DictionaryEntry(
                    hostMeaning,
                    languageID,
                    alternates,
                    0,
                    sourceIDs,
                    senses,
                    mediaRuns);
                AddDefinition(dictionaryEntry);
            }
            else
            {
                dictionaryEntry.MergeSourceIDs(sourceIDs);

                reading = dictionaryEntry.GetHighestAlternateKey();
                sense.Reading = reading;
                dictionaryEntry.AddUniqueSense(sense);

                if (dictionaryEntry.Modified)
                    UpdateDefinition(dictionaryEntry);
            }
        }

        protected virtual void AddHostEntry(
            string key,
            LanguageID languageID,
            List<string> hostMeanings,
            List<int> sourceIDs,
            List<Sense> targetSenses,
            out DictionaryEntry dictionaryEntry)
        {
            List<LanguageString> alternates = null;
            List<Sense> newSenses = new List<Sense>();
            List<MediaRun> mediaRuns = null;
            int reading = 0;
            bool modified = false;

            key = key.Trim();

            dictionaryEntry = GetDefinition(key, languageID);

            if (dictionaryEntry == null)
            {
                if (TargetCount > 1)
                    alternates = new List<LanguageString>();
            }
            else
            {
                alternates = dictionaryEntry.Alternates;

                if ((TargetCount > 1) && (alternates == null))
                    dictionaryEntry.Alternates = alternates = new List<LanguageString>();
            }

            AddAlternates(
                languageID,
                hostMeanings,
                dictionaryEntry,
                out alternates,
                ref reading,
                ref modified);

            foreach (Sense sense in targetSenses)
            {
                Sense newSense = new Sense(sense);
                newSense.Reading = reading;
                newSenses.Add(newSense);
            }

            if (dictionaryEntry == null)
            {
                dictionaryEntry = new DictionaryEntry(
                    key,
                    languageID,
                    alternates,
                    0,
                    sourceIDs,
                    newSenses,
                    mediaRuns);
                dictionaryEntry.SortPriority();
                AddDefinition(dictionaryEntry);

                if (DoStems)
                {
                    foreach (Sense newSense in newSenses)
                        AddStemCheck(dictionaryEntry, newSense);
                }
            }
            else
            {
                dictionaryEntry.MergeSourceIDs(sourceIDs);

                if (dictionaryEntry.Senses == null)
                    dictionaryEntry.Senses = new List<Sense>();

                if (DoStems)
                {
                    foreach (Sense sense in newSenses)
                    {
                        if (dictionaryEntry.AddUniqueSense(sense))
                            AddStemCheck(dictionaryEntry, sense);
                    }
                }
                else
                {
                    foreach (Sense sense in newSenses)
                        dictionaryEntry.AddUniqueSense(sense);
                }

                if (modified || dictionaryEntry.Modified)
                {
                    dictionaryEntry.TouchAndClearModified();
                    UpdateDefinition(dictionaryEntry);
                }
            }
        }

        protected virtual void ConvertDictionaryToDisplay()
        {
            CheckForCancel();
            UpdateProgressElapsed("Convert to display form ...");

            List<LanguageID> languageIDs;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            foreach (LanguageID languageID in languageIDs)
                ConvertLanguageDictionaryToDisplay(languageID);
        }

        protected virtual void ConvertLanguageDictionaryToDisplay(LanguageID languageID)
        {
            int count = GetDefinitionCount(languageID);
            int index;
            DictionaryEntry entry;

            OperationLanguageName = languageID.LanguageName(UILanguageID);
            OperationItemCount = count;
            State = StateCode.DisplayConversion;

            for (index = 0; index < count; index++)
            {
                OperationItemIndex = index;
                entry = GetDefinitionIndexed(index, languageID);

                // Try again.
                if (entry == null)
                    entry = GetDefinitionIndexed(index, languageID);

                if (entry != null)
                {
                    if (ConvertDictionaryEntryToDisplay(entry))
                        UpdateDefinition(entry);
                }
                else
                    ErrorCount = ErrorCount + 1;
            }

            count = GetDefinitionCount(languageID);

            for (index = 0; index < count; index++)
            {
                OperationItemIndex = index;
                entry = GetStemDefinitionIndexed(index, languageID);

                // Try again.
                if (entry == null)
                    entry = GetStemDefinitionIndexed(index, languageID);

                if (entry != null)
                {
                    if (ConvertDictionaryEntryToDisplay(entry))
                        UpdateStemDefinition(entry);
                }
                else
                    ErrorCount = ErrorCount + 1;
            }
        }

        // Returns true if any changes.
        protected virtual bool ConvertDictionaryEntryToDisplay(DictionaryEntry entry)
        {
            return true;
        }

        protected virtual int GetDefinitionCount(LanguageID languageID)
        {
            int count;

            if (UseCache)
            {
                List<DictionaryEntry> list = GetEntriesList(languageID);
                count = list.Count();
            }
            else
                count = TemporaryDictionaryRepository.Count(languageID);

            return count;
        }

        protected virtual DictionaryEntry GetDefinitionIndexed(int index, LanguageID languageID)
        {
            DictionaryEntry dictionaryEntry;

            if (UseCache)
                dictionaryEntry = GetCachedEntryIndexed(index, languageID);
            else
                dictionaryEntry = TemporaryDictionaryRepository.GetIndexed(index, languageID);

            return dictionaryEntry;
        }

        protected virtual DictionaryEntry GetDefinition(string key, LanguageID languageID)
        {
            DictionaryEntry dictionaryEntry;

            if (UseCache)
                dictionaryEntry = GetCachedEntry(key, languageID);
            else
                dictionaryEntry = TemporaryDictionaryRepository.Get(key, languageID);

            return dictionaryEntry;
        }

        protected virtual void AddDefinition(DictionaryEntry dictionaryEntry)
        {
            LanguageID languageID = dictionaryEntry.LanguageID;

            if (UseCache)
            {
                Dictionary<string, DictionaryEntry> dictionary = GetCachedDictionary(languageID);
                List<DictionaryEntry> list = GetEntriesList(languageID);
                dictionary.Add(dictionaryEntry.KeyString, dictionaryEntry);
                list.Add(dictionaryEntry);
            }
            else
            {
                if (!TemporaryDictionaryRepository.Add(dictionaryEntry, languageID))
                    UpdateDefinition(dictionaryEntry);
            }

            if (ReadObjects == null)
                ReadObjects = new List<IBaseObject>() { dictionaryEntry };
            else
                ReadObjects.Add(dictionaryEntry);
        }

        protected virtual void UpdateDefinition(DictionaryEntry dictionaryEntry)
        {
            if (UseCache)
            {
                // No need to do anything, as we are operating directly on the entry.
            }
            else
            {
                TemporaryDictionaryRepository.Delete(dictionaryEntry, dictionaryEntry.LanguageID);

                if (!TemporaryDictionaryRepository.Add(dictionaryEntry, dictionaryEntry.LanguageID))
                    ErrorCount++;
            }
        }

        protected virtual bool WriteDictionary()
        {
            bool returnValue;

            CheckForCancel();
            UpdateProgressElapsed("Writing dictionary ...");

            if (UseCache)
            {
                returnValue = WriteDictionaryCached();
                returnValue = WriteStemsDictionaryCached() && returnValue;
            }
            else
            {
                returnValue = WriteDictionaryNotCached();
                returnValue = WriteStemsDictionaryNotCached() && returnValue;
            }

            return returnValue;
        }

        protected virtual bool WriteDictionaryCached()
        {
            bool returnValue = true;

            OperationItemCount = 0;
            OperationItemIndex = 0;
            State = StateCode.WritingDataBase;

            List<LanguageID> languageIDs;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            foreach (LanguageID languageID in languageIDs)
            {
                OperationLanguageName = languageID.LanguageName(UILanguageID);

                CheckForCancel();

                List<DictionaryEntry> entries = GetEntriesList(languageID);

                if (DictionaryRepository.Count(languageID) != 0)
                    DictionaryRepository.DeleteList(entries, languageID);

                OperationItemCount = entries.Count();
                OperationItemIndex = 0;

                PutStatusMessage("Fixing up cached dictionary: " + OperationLanguageName + " " + OperationItemCount.ToString() + " items");

                foreach (DictionaryEntry entry in entries)
                    FixUpEntry(entry);

                PutStatusMessage("Writing cached dictionary: " + OperationLanguageName + " " + OperationItemCount.ToString() + " items");

                try
                {
                    CheckForCancel();

                    if (!DictionaryRepository.AddList(entries, languageID))
                        returnValue = false;
                }
                catch (Exception)
                {
                    ErrorCount++;
                    returnValue = false;
                }

                OperationItemIndex = OperationItemCount;
            }

            return returnValue;
        }

        protected virtual bool WriteDictionaryNotCached()
        {
            bool returnValue = true;

            OperationItemCount = 0;
            OperationItemIndex = 0;
            State = StateCode.WritingDataBase;

            List<LanguageID> languageIDs;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            foreach (LanguageID languageID in languageIDs)
            {
                OperationLanguageName = languageID.LanguageName(UILanguageID);
                CheckForCancel();
                List<DictionaryEntry> entries = TemporaryDictionaryRepository.GetAll(languageID);

                OperationItemCount = entries.Count();
                OperationItemIndex = 0;

                PutStatusMessage("Writing non cached dictionary: " + OperationLanguageName + " " + OperationItemCount.ToString() + " items");

                foreach (DictionaryEntry entry in entries)
                {
                    try
                    {
                        CheckForCancel();

                        DictionaryEntry oldEntry = DictionaryRepository.Get(entry.Key, languageID);

                        if (oldEntry != null)
                        {
                            PutStatusMessage("Updating " + OperationLanguageName + " entry: " + entry.KeyString);

                            oldEntry.MergeEntry(entry);

                            FixUpEntry(entry);

                            if (!DictionaryRepository.Update(oldEntry, languageID))
                            {
                                ErrorCount++;
                                returnValue = false;
                            }
                        }
                        else
                        {
                            PutStatusMessage("Adding " + OperationLanguageName + " entry: " + entry.KeyString);

                            FixUpEntry(entry);

                            if (!DictionaryRepository.Add(entry, languageID))
                            {
                                ErrorCount++;
                                returnValue = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        ErrorCount++;
                        returnValue = false;
                    }

                    OperationItemIndex++;
                }
            }

            return returnValue;
        }

        protected virtual bool WriteDictionaryDisplayOutput()
        {
            bool returnValue = true;

            if (String.IsNullOrEmpty(DisplayFilePath))
                return true;

            CheckForCancel();
            UpdateProgressElapsed("Writing dictionary display files ...");

            OperationItemCount = 0;
            OperationItemIndex = 0;
            State = StateCode.WritingDataBase;

            Stream displayStream = OpenWriteStream(DisplayFilePath);

            if (displayStream == null)
                return false;

            using (StreamWriter writer = new StreamWriter(displayStream))
            {
                if (UseCache)
                {
                    returnValue = WriteDictionaryDisplayCached(writer);
                    returnValue = WriteStemsDictionaryDisplayCached(writer) && returnValue;
                }
                else
                {
                    returnValue = WriteDictionaryDisplayNotCached(writer);
                    returnValue = WriteStemsDictionaryDisplayNotCached(writer) && returnValue;
                }
            }

            OperationItemIndex = OperationItemCount;

            return returnValue;
        }

        protected virtual bool WriteDictionaryDisplayCached(StreamWriter writer)
        {
            List<LanguageID> languageIDs;
            bool returnValue = true;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            foreach (LanguageID languageID in languageIDs)
            {
                OperationLanguageName = languageID.LanguageName(UILanguageID);

                CheckForCancel();

                List<DictionaryEntry> entries = GetEntriesList(languageID);

                PutStatusMessage("Writing cached dictionary display file: " + OperationLanguageName + " " + OperationItemCount.ToString() + " items");

                try
                {
                    CheckForCancel();

                    writer.WriteLine("Dictionary entries for: " + OperationLanguageName + "\n");

                    foreach (DictionaryEntry entry in entries)
                    {
                        string entryString = entry.GetFullDefinition(0, 4, true);
                        writer.Write(entryString);
                    }

                    writer.WriteLine(String.Empty);
                }
                catch (Exception)
                {
                    ErrorCount++;
                    returnValue = false;
                }
            }

            return returnValue;
        }

        protected virtual bool WriteDictionaryDisplayNotCached(StreamWriter writer)
        {
            bool returnValue = true;

            List<LanguageID> languageIDs;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            foreach (LanguageID languageID in languageIDs)
            {
                OperationLanguageName = languageID.LanguageName(UILanguageID);

                CheckForCancel();

                List<DictionaryEntry> entries = DictionaryRepository.GetAll(languageID);

                if (entries == null)
                {
                    PutError("Error reading dictionary entries for display: " + languageID.LanguageName(UILanguageID));
                    ErrorCount++;
                    return false;
                }

                PutStatusMessage("Writing dictionary display file: " + OperationLanguageName + " " + OperationItemCount.ToString() + " items");

                try
                {
                    CheckForCancel();

                    writer.WriteLine("Dictionary entries for: " + OperationLanguageName + "\n");

                    foreach (DictionaryEntry entry in entries)
                    {
                        string entryString = entry.GetFullDefinition(0, 4, true);
                        writer.Write(entryString);
                    }

                    writer.WriteLine(String.Empty);
                }
                catch (Exception)
                {
                    ErrorCount++;
                    returnValue = false;
                }
            }

            return returnValue;
        }

        protected virtual List<DictionaryEntry> GetEntriesList(LanguageID languageID)
        {
            if (languageID == null)
                throw new Exception("Null language ID");

            List<LanguageID> languageIDs;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            int index = languageIDs.IndexOf(languageID);

            if (index == -1)
                throw new Exception("Language lookup failed: " + languageID.LanguageName(UILanguageID));

            List<DictionaryEntry> list = EntriesList[index];

            return list;
        }

        protected virtual Dictionary<string, DictionaryEntry> GetCachedDictionary(LanguageID languageID)
        {
            if (languageID == null)
                throw new Exception("Null language ID");

            List<LanguageID> languageIDs;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            int index = languageIDs.IndexOf(languageID);

            if (index == -1)
                throw new Exception("Language lookup failed: " + languageID.LanguageName(UILanguageID));

            Dictionary<string, DictionaryEntry> dictionary = CacheList[index];

            return dictionary;
        }

        protected virtual DictionaryEntry GetCachedEntry(string key, LanguageID languageID)
        {
            Dictionary<string, DictionaryEntry> dictionary = GetCachedDictionary(languageID);
            DictionaryEntry entry = null;

            if (dictionary.TryGetValue(key, out entry))
                return entry;

            return null;
        }

        protected virtual DictionaryEntry GetCachedEntryIndexed(int index, LanguageID languageID)
        {
            List<DictionaryEntry> list = GetEntriesList(languageID);
            DictionaryEntry entry = null;

            if ((index >= 0) && (index < list.Count()))
                entry = list[index];

            return entry;
        }

        protected virtual int GetStemDefinitionCount(LanguageID languageID)
        {
            int count;

            if (UseCache)
            {
                List<DictionaryEntry> list = GetStemsEntriesList(languageID);
                count = list.Count();
            }
            else
                count = TemporaryDictionaryStemsRepository.Count(languageID);

            return count;
        }

        protected virtual DictionaryEntry GetStemDefinitionIndexed(int index, LanguageID languageID)
        {
            DictionaryEntry dictionaryEntry;

            if (UseCache)
                dictionaryEntry = GetCachedStemEntryIndexed(index, languageID);
            else
                dictionaryEntry = TemporaryDictionaryStemsRepository.GetIndexed(index, languageID);

            return dictionaryEntry;
        }

        protected virtual DictionaryEntry GetStemDefinition(string key, LanguageID languageID)
        {
            DictionaryEntry dictionaryEntry;

            if (UseCache)
                dictionaryEntry = GetCachedStemEntry(key, languageID);
            else
                dictionaryEntry = TemporaryDictionaryStemsRepository.Get(key, languageID);

            return dictionaryEntry;
        }

        protected virtual void AddStemDefinition(DictionaryEntry dictionaryEntry)
        {
            LanguageID languageID = dictionaryEntry.LanguageID;

            if (UseCache)
            {
                Dictionary<string, DictionaryEntry> dictionary = GetCachedStemsDictionary(languageID);
                List<DictionaryEntry> list = GetStemsEntriesList(languageID);
                dictionary.Add(dictionaryEntry.KeyString, dictionaryEntry);
                list.Add(dictionaryEntry);
            }
            else
            {
                if (!TemporaryDictionaryStemsRepository.Add(dictionaryEntry, languageID))
                    UpdateStemDefinition(dictionaryEntry);
            }

            if (ReadObjects == null)
                ReadObjects = new List<IBaseObject>() { dictionaryEntry };
            else
                ReadObjects.Add(dictionaryEntry);
        }

        protected virtual void UpdateStemDefinition(DictionaryEntry dictionaryEntry)
        {
            if (UseCache)
            {
                // No need to do anything, as we are operating directly on the entry.
            }
            else
            {
                TemporaryDictionaryStemsRepository.Delete(dictionaryEntry, dictionaryEntry.LanguageID);

                if (!TemporaryDictionaryStemsRepository.Add(dictionaryEntry, dictionaryEntry.LanguageID))
                    ErrorCount++;
            }
        }

        protected virtual bool WriteStemsDictionary()
        {
            CheckForCancel();
            UpdateProgressElapsed("Writing stems dictionary ...");

            if (UseCache)
                return WriteStemsDictionaryCached();
            else
                return WriteStemsDictionaryNotCached();
        }

        protected virtual bool WriteStemsDictionaryCached()
        {
            bool returnValue = true;

            OperationItemCount = 0;
            OperationItemIndex = 0;
            State = StateCode.WritingDataBase;

            List<LanguageID> languageIDs;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            foreach (LanguageID languageID in languageIDs)
            {
                OperationLanguageName = languageID.LanguageName(UILanguageID);

                CheckForCancel();

                List<DictionaryEntry> entries = GetStemsEntriesList(languageID);

                if (DictionaryStemsRepository.Count(languageID) != 0)
                    DictionaryStemsRepository.DeleteList(entries, languageID);

                OperationItemCount = entries.Count();
                OperationItemIndex = 0;

                PutStatusMessage("Fixing up stems dictionary: " + OperationLanguageName + " " + OperationItemCount.ToString() + " items");

                foreach (DictionaryEntry entry in entries)
                    FixUpEntry(entry);

                PutStatusMessage("Writing stems dictionary: " + OperationLanguageName + " " + OperationItemCount.ToString() + " items");

                try
                {
                    CheckForCancel();

                    if (!DictionaryStemsRepository.AddList(entries, languageID))
                        returnValue = false;
                }
                catch (Exception)
                {
                    ErrorCount++;
                    returnValue = false;
                }

                OperationItemIndex = OperationItemCount;
            }

            return returnValue;
        }

        protected virtual bool WriteStemsDictionaryNotCached()
        {
            bool returnValue = true;

            OperationItemCount = 0;
            OperationItemIndex = 0;
            State = StateCode.WritingDataBase;

            List<LanguageID> languageIDs;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            foreach (LanguageID languageID in languageIDs)
            {
                OperationLanguageName = languageID.LanguageName(UILanguageID);
                CheckForCancel();
                List<DictionaryEntry> entries = TemporaryDictionaryStemsRepository.GetAll(languageID);

                OperationItemCount = entries.Count();
                OperationItemIndex = 0;

                PutStatusMessage("Writing stems dictionary: " + OperationLanguageName + " " + OperationItemCount.ToString() + " items");

                foreach (DictionaryEntry entry in entries)
                {
                    try
                    {
                        CheckForCancel();

                        DictionaryEntry oldEntry = DictionaryRepository.Get(entry.Key, languageID);

                        if (oldEntry != null)
                        {
                            PutStatusMessage("Updating stems " + OperationLanguageName + " entry: " + entry.KeyString);

                            oldEntry.MergeEntry(entry);

                            FixUpEntry(entry);

                            if (!DictionaryStemsRepository.Update(oldEntry, languageID))
                            {
                                ErrorCount++;
                                returnValue = false;
                            }
                        }
                        else
                        {
                            PutStatusMessage("Adding stems " + OperationLanguageName + " entry: " + entry.KeyString);

                            FixUpEntry(entry);

                            if (!DictionaryStemsRepository.Add(entry, languageID))
                            {
                                ErrorCount++;
                                returnValue = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        ErrorCount++;
                        returnValue = false;
                    }

                    OperationItemIndex++;
                }
            }

            return returnValue;
        }

        protected virtual bool WriteStemsDictionaryDisplayCached(StreamWriter writer)
        {
            List<LanguageID> languageIDs;
            bool returnValue = true;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            foreach (LanguageID languageID in languageIDs)
            {
                OperationLanguageName = languageID.LanguageName(UILanguageID);

                CheckForCancel();

                List<DictionaryEntry> entries = GetStemsEntriesList(languageID);

                PutStatusMessage("Writing dictionary stems display file: " + OperationLanguageName + " " + OperationItemCount.ToString() + " items");

                try
                {
                    CheckForCancel();

                    writer.WriteLine("Dictionary stem entries for: " + OperationLanguageName + "\n");

                    foreach (DictionaryEntry entry in entries)
                    {
                        string entryString = entry.GetFullDefinition(0, 4, true);
                        writer.Write(entryString);
                    }

                    writer.WriteLine(String.Empty);
                }
                catch (Exception)
                {
                    ErrorCount++;
                    returnValue = false;
                }
            }

            return returnValue;
        }

        protected virtual bool WriteStemsDictionaryDisplayNotCached(StreamWriter writer)
        {
            List<LanguageID> languageIDs;
            bool returnValue = true;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            foreach (LanguageID languageID in languageIDs)
            {
                OperationLanguageName = languageID.LanguageName(UILanguageID);

                CheckForCancel();

                List<DictionaryEntry> entries = DictionaryStemsRepository.GetAll(languageID);

                if (entries == null)
                {
                    PutError("Error reading dictionary stem entries for display: " + languageID.LanguageName(UILanguageID));
                    ErrorCount++;
                    return false;
                }

                PutStatusMessage("Writing dictionary stem display file: " + OperationLanguageName + " " + OperationItemCount.ToString() + " items");

                try
                {
                    CheckForCancel();

                    writer.WriteLine("Dictionary stem entries for: " + OperationLanguageName + "\n");

                    foreach (DictionaryEntry entry in entries)
                    {
                        string entryString = entry.GetFullDefinition(0, 4, true);
                        writer.Write(entryString);
                    }

                    writer.WriteLine(String.Empty);
                }
                catch (Exception)
                {
                    ErrorCount++;
                    returnValue = false;
                }
            }

            return returnValue;
        }

        protected virtual List<DictionaryEntry> GetStemsEntriesList(LanguageID languageID)
        {
            if (languageID == null)
                throw new Exception("Null language ID (stems)");

            List<LanguageID> languageIDs;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            int index = languageIDs.IndexOf(languageID);

            if (index == -1)
                throw new Exception("Language lookup failed (stems): " + languageID.LanguageName(UILanguageID));

            List<DictionaryEntry> list = StemsEntriesList[index];

            return list;
        }

        protected virtual Dictionary<string, DictionaryEntry> GetCachedStemsDictionary(LanguageID languageID)
        {
            if (languageID == null)
                throw new Exception("Null language ID (stems)");

            List<LanguageID> languageIDs;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            int index = languageIDs.IndexOf(languageID);

            if (index == -1)
                throw new Exception("Language lookup failed (stems): " + languageID.LanguageName(UILanguageID));

            Dictionary<string, DictionaryEntry> dictionary = StemsCacheList[index];

            return dictionary;
        }

        protected virtual DictionaryEntry GetCachedStemEntry(string key, LanguageID languageID)
        {
            Dictionary<string, DictionaryEntry> dictionary = GetCachedStemsDictionary(languageID);
            DictionaryEntry entry = null;

            if (dictionary.TryGetValue(key, out entry))
                return entry;

            return null;
        }

        protected virtual DictionaryEntry GetCachedStemEntryIndexed(int index, LanguageID languageID)
        {
            List<DictionaryEntry> list = GetStemsEntriesList(languageID);
            DictionaryEntry entry = null;

            if ((index >= 0) && (index < list.Count()))
                entry = list[index];

            return entry;
        }

        protected virtual void FixUpEntry(DictionaryEntry entry)
        {
            entry.SortPriority();
            entry.TouchAndClearModified();
        }

        protected virtual void SynthesizeMissingAudio()
        {
            if (IsSynthesizeMissingAudio)
            {
                UpdateProgressElapsed("Synthesizing missing audio ...");

                if (UseCache)
                    SynthesizeMissingAudioCached();
                else
                    SynthesizeMissingAudioNotCached();
            }
            else
            {
                UpdateProgressElapsed("Skipping audio synthesis check ...");
                UpdateProgressElapsed("Skipping add audio references ...");
                UpdateProgressElapsed("Skipping update audio references ...");
            }
        }

        protected virtual void SynthesizeMissingAudioCached()
        {
            ITextToSpeech speechEngine = TextToSpeechSingleton.GetTextToSpeech();

            State = StateCode.AddingAudio;

            string saveError = Error;
            Error = String.Empty;

            try
            {
                LanguageID targetLanguageID = TargetLanguageIDs.First();
                string voice = GetVoiceName(targetLanguageID);
                int speed = GetVoiceSpeed(targetLanguageID);

                OperationLanguageName = targetLanguageID.LanguageName(UILanguageID);
                OperationItemIndex = 0;

                PutStatusMessage("Synthesizing audio for: " + OperationLanguageName);

                List<DictionaryEntry> entries = GetEntriesList(targetLanguageID);

                if (entries != null)
                {
                    if (!NodeUtilities.AddSynthesizedVoiceToDictionaryEntries(
                        entries,
                        targetLanguageID,
                        voice,
                        speed,
                        IsForceAudio,
                        speechEngine,
                        ref OperationItemIndex))
                    {
                        PutStatusMessage(Error);
                        saveError = saveError + "\n" + Error;
                    }
                }

                foreach (LanguageID hostLanguageID in HostLanguageIDs)
                {
                    voice = GetVoiceName(hostLanguageID);
                    speed = GetVoiceSpeed(hostLanguageID);

                    OperationLanguageName = hostLanguageID.LanguageName(UILanguageID);
                    OperationItemIndex = 0;

                    PutStatusMessage("Synthesizing audio for: " + OperationLanguageName);

                    Error = String.Empty;

                    entries = GetEntriesList(hostLanguageID);

                    if (entries != null)
                    {
                        if (!NodeUtilities.AddSynthesizedVoiceToDictionaryEntries(
                            entries,
                            hostLanguageID,
                            voice,
                            speed,
                            IsForceAudio,
                            speechEngine,
                            ref OperationItemIndex))
                        {
                            PutStatusMessage(Error);
                            saveError = saveError + "\n" + Error;
                        }
                    }
                }
            }
            catch (Exception)
            {
                ErrorCount++;
            }
            finally
            {
                speechEngine.Reset();
                Error = saveError;
            }
        }

        protected virtual void SynthesizeMissingAudioNotCached()
        {
            ITextToSpeech speechEngine = TextToSpeechSingleton.GetTextToSpeech();

            State = StateCode.AddingAudio;

            string saveError = Error;
            Error = String.Empty;

            try
            {
                LanguageID targetLanguageID = TargetLanguageIDs.First();
                string voice = GetVoiceName(targetLanguageID);
                int speed = GetVoiceSpeed(targetLanguageID);

                OperationLanguageName = targetLanguageID.LanguageName(UILanguageID);
                OperationItemIndex = 0;

                PutStatusMessage("Synthesizing audio for: " + OperationLanguageName);

                List<DictionaryEntry> entries = TemporaryDictionaryRepository.GetAll(targetLanguageID);

                if (entries != null)
                {
                    if (!NodeUtilities.AddSynthesizedVoiceToDictionaryEntries(
                        entries,
                        targetLanguageID,
                        voice,
                        speed,
                        IsForceAudio,
                        speechEngine,
                        ref OperationItemIndex))
                    {
                        PutStatusMessage(Error);
                        saveError = saveError + "\n" + Error;
                    }
                }

                foreach (LanguageID hostLanguageID in HostLanguageIDs)
                {
                    voice = GetVoiceName(hostLanguageID);
                    speed = GetVoiceSpeed(hostLanguageID);

                    OperationLanguageName = hostLanguageID.LanguageName(UILanguageID);
                    OperationItemIndex = 0;

                    PutStatusMessage("Synthesizing audio for: " + OperationLanguageName);

                    Error = String.Empty;

                    entries = TemporaryDictionaryRepository.GetAll(hostLanguageID);

                    if (entries != null)
                    {
                        if (!NodeUtilities.AddSynthesizedVoiceToDictionaryEntries(
                            entries,
                            hostLanguageID,
                            voice,
                            speed,
                            IsForceAudio,
                            speechEngine,
                            ref OperationItemIndex))
                        {
                            PutStatusMessage(Error);
                            saveError = saveError + "\n" + Error;
                        }
                    }
                }
            }
            catch (Exception)
            {
                ErrorCount++;
            }
            finally
            {
                speechEngine.Reset();
                Error = saveError;
            }
        }

        protected virtual string GetVoiceName(LanguageID languageID)
        {
            return NodeUtilities.GetDefaultVoiceName(languageID);
        }

        protected virtual int GetVoiceSpeed(LanguageID languageID)
        {
            if (TargetLanguageIDs.Contains(languageID))
                return -2;

            return 0;
        }

        protected virtual void DumpTemporaryDictionary()
        {
            List<LanguageID> languageIDs;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            foreach (LanguageID languageID in languageIDs)
            {
                List<DictionaryEntry> entries;

                if (UseCache)
                    entries = GetEntriesList(languageID);
                else
                    entries = TemporaryDictionaryRepository.GetAll(languageID);

                foreach (DictionaryEntry entry in entries)
                {
                    StringBuilder sb = new StringBuilder();
                    entry.GetDumpString("    ", sb);
                    string text = sb.ToString();

                    DumpString(text);
                }
            }
        }

        public void BackupDictionaries(string sourcePath)
        {
            string directoryPath = MediaUtilities.GetFilePath(sourcePath);
            string sourceFileName = MediaUtilities.GetFileNameFromPath(sourcePath);
            string baseFileName = MediaUtilities.GetBaseFileName(sourceFileName);

            DumpString("Backing up dictionaries:");

            List<LanguageID> languageIDs;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            foreach (LanguageID languageID in languageIDs)
                BackupSingleDictionary(directoryPath, baseFileName, languageID);

            DumpString("Dictionary backup complete:");
        }

        public void BackupSingleDictionary(string directoryPath, string baseFileName, LanguageID languageID)
        {
            string fileName = baseFileName + "_" + languageID.SymbolName + ".xml";
            string filePath = MediaUtilities.ConcatenateFilePath(directoryPath, fileName);
            string languageName = languageID.LanguageName(LanguageLookup.English);
            XElement element = Repositories.Dictionary.ObjectStore.GetObjectStore(languageID).Xml;
            using (Stream stream = FileSingleton.OpenWrite(filePath))
            {
                try
                {
                    DumpString("Backing up " + languageName + " dictionary to " + filePath + ":");
                    element.Save(stream, SaveOptions.None);
                }
                catch (Exception exc)
                {
                    string message = "Exception: " + exc.Message;
                    if (exc.InnerException != null)
                        message += ": " + exc.InnerException.Message;
                    DumpString(message);
                }
                finally
                {
                    FileSingleton.Close(stream);
                }
            }
        }

        public virtual void CollectLengths()
        {
            List<LanguageID> languageIDs;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            foreach (LanguageID languageID in languageIDs)
            {
                List<DictionaryEntry> entries;
                LanguageDescription languageDescription = LanguageLookup.GetLanguageDescription(languageID);
                LanguageTool tool = ApplicationData.LanguageTools.Create(languageID);
                int bestLength = 0;
                int keyLength;

                if (languageDescription == null)
                    continue;

                if (UseCache)
                    entries = GetEntriesList(languageID);
                else
                    entries = TemporaryDictionaryRepository.GetAll(languageID);

                foreach (DictionaryEntry entry in entries)
                {
                    keyLength = entry.KeyString.Length;

                    if (keyLength > bestLength)
                        bestLength = keyLength;
                }

                if (bestLength > languageDescription.LongestDictionaryEntryLength)
                    languageDescription.LongestDictionaryEntryLength = bestLength;

                if (tool != null)
                {
                    int longestPrefix = 0;
                    int longestSuffix = 0;
                    int longestInflection = 0;

                    tool.GetLongestLengths(
                        languageID,
                        languageDescription.LongestDictionaryEntryLength,
                        ref longestPrefix,
                        ref longestSuffix,
                        ref longestInflection);

                    languageDescription.LongestPrefixLength = longestPrefix;
                    languageDescription.LongestSuffixLength = longestSuffix;
                    languageDescription.LongestInflectionLength = longestInflection;
                }

                LanguageLookup.UpdateLanguageDescription(languageDescription);
            }
        }

        public override void DumpStatistics()
        {
            List<LanguageID> languageIDs;

            if (IsAddReciprocals)
                languageIDs = UniqueLanguageIDs;
            else
                languageIDs = TargetLanguageIDs;

            foreach (LanguageID languageID in languageIDs)
            {
                List<DictionaryEntry> entries;

                if (UseCache)
                    entries = GetEntriesList(languageID);
                else
                    entries = TemporaryDictionaryRepository.GetAll(languageID);

                NodeUtilities.DumpDictionaryStatistics(languageID, entries);
            }
        }

        public override float GetProgress()
        {
            switch (State)
            {
                case StateCode.Idle:
                default:
                    return 0.0f;
                case StateCode.Reading:
                    if (BytesRead >= FileSize)
                        return 1.0f;
                    if (FileSize != 0)
                        return ((float)BytesRead / FileSize);
                    break;
                case StateCode.DisplayConversion:
                    break;
                case StateCode.WritingDataBase:
                    break;
                case StateCode.WritingXml:
                    break;
                case StateCode.AddingAudio:
                    break;
                case StateCode.Completed:
                    break;
            }

            return 1.0f;
        }

        public override string GetProgressMessage()
        {
            string message = "";

            switch (State)
            {
                case StateCode.Idle:
                default:
                    message = "Import beginning...";
                    break;
                case StateCode.Reading:
                    if (BytesRead >= FileSize)
                        message = "Read completed.  " + EntryIndex.ToString() + " entries read.  " + ItemCount.ToString() + " items created.";
                    else
                    {
                        int percent = 100;
                        if (FileSize != 0)
                            percent = (int)(((float)BytesRead / FileSize) * 100);
                        message = percent.ToString() + " percent of entries read.  " + ItemCount.ToString() + " items created.";
                    }
                    break;
                case StateCode.DisplayConversion:
                    message = OperationItemIndex.ToString() + " entries converted for language " + OperationLanguageName + ".";
                    break;
                case StateCode.WritingDataBase:
                    message = OperationItemIndex.ToString() + " entries of " + OperationItemCount + " written to database for language " + OperationLanguageName;
                    break;
                case StateCode.WritingXml:
                    message = "Writing XML data file.";
                    break;
                case StateCode.AddingAudio:
                    message = OperationItemIndex.ToString() + " entries of " + OperationItemCount + " synthesied audio for language " + OperationLanguageName;
                    break;
                case StateCode.Completed:
                    message = GetCompletedMessage();
                    break;
            }

            if (ErrorCount != 0)
                message += "  " + ErrorCount.ToString() + " errors so far.";

            if (Timer != null)
                message += "  Elapsed time is " + ElapsedTime.ToString();

            return message;
        }

        protected virtual string GetCompletedMessage()
        {
            string message;

            if (Timer != null)
            {
                Timer.Stop();
                OperationTime = Timer.GetTimeInSeconds();

                double totalTime = Timer.GetTimeInSeconds();
                double perTime = 0.0;

                if (ItemCount != 0)
                    perTime = totalTime / ItemCount;

                message =
                    "Import complete:" +
                    " Item count: " + ItemCount.ToString() +
                    " Total time: " + OperationTime.ToString() +
                    " Item average time: " + perTime.ToString() + " seconds";
            }
            else
                message = "Import complete.";

            return message;
        }

        public static string IsAddReciprocalsHelp = "Check this to add reciprical host entries.";

        public override void LoadFromArguments()
        {
            base.LoadFromArguments();

            IsAddReciprocals = GetFlagArgumentDefaulted("IsAddReciprocals", "flag", "r", IsAddReciprocals,
                "Add reciprocals", IsAddReciprocalsHelp, null, null);

            DeleteBeforeImport = GetFlagArgumentDefaulted("DeleteBeforeImport", "flag", "r", DeleteBeforeImport,
                "Delete before import", DeleteBeforeImportHelp, null, null);

            IsSynthesizeMissingAudio = GetFlagArgumentDefaulted("IsSynthesizeMissingAudio", "flag", "r", IsSynthesizeMissingAudio, "Synthesize missing audio",
                IsSynthesizeMissingAudioHelp, null, null);

            DisplayFilePath = GetArgumentDefaulted("DisplayFilePath", "string", "rw", DisplayFilePath, DisplayFilePathPrompt,
                DisplayFilePathHelp);

            RawDisplayFilePath = GetArgumentDefaulted("RawDisplayFilePath", "string", "rw", RawDisplayFilePath, RawDisplayFilePathPrompt,
                RawDisplayFilePathHelp);
        }

        public override void SaveToArguments()
        {
            base.SaveToArguments();

            SetFlagArgument("IsAddReciprocals", "flag", "r", IsAddReciprocals, "Add reciprocals",
                IsAddReciprocalsHelp, null, null);

            SetFlagArgument("DeleteBeforeImport", "flag", "r", DeleteBeforeImport, "Delete before import",
                DeleteBeforeImportHelp, null, null);

            SetFlagArgument("IsSynthesizeMissingAudio", "flag", "r", IsSynthesizeMissingAudio, "Synthesize missing audio",
                IsSynthesizeMissingAudioHelp, null, null);

            SetArgument("DisplayFilePath", "string", "rw", DisplayFilePath, DisplayFilePathPrompt,
                DisplayFilePathHelp, null, null);

            SetArgument("RawDisplayFilePath", "string", "rw", RawDisplayFilePath, RawDisplayFilePathPrompt,
                RawDisplayFilePathHelp, null, null);
        }

        public override void DumpArguments(string label)
        {
            base.DumpArguments(label);
        }

        // Check for supported capability.
        // importExport:  "Import" or "Export"
        // componentName: class name or GetComponentName value
        // capability: "Supported" for general support, "UseFlags" for component item select support, "ComponentFlags" for lesson component select support,
        //  "NodeFlags" for sub-object select support.
        public static new bool IsSupportedStatic(string importExport, string componentName, string capability)
        {
            if (importExport != "Import")
                return false;

            switch (componentName)
            {
                case "DictionaryEntry":
                    if (capability == "Support")
                        return true;
                    return false;
                default:
                    return false;
            }
        }

        public override bool IsSupportedVirtual(string importExport, string componentName, string capability)
        {
            return IsSupportedStatic(importExport, componentName, capability);
        }

        public static new string TypeStringStatic { get { return "Dictionary"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
