using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
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
    public partial class LanguageTool : BaseObjectLanguage
    {
        public virtual LexTable EndingsTable
        {
            get
            {
                InitializeConjugationTableCheck();
                return _EndingsTable;
            }
            set
            {
            }
        }

        public virtual void InitializeConjugationTableCheck()
        {
            if (CanDeinflect && (_EndingsTable == null))
                InitializeConjugationTable();
        }

        public virtual void InitializeConjugationTable()
        {
            if (!CanDeinflect)
                return;

            if (_EndingsTableCache != null)
            {
                if (_EndingsTableCache.TryGetValue(LanguageCode, out _EndingsTable))
                    return;
            }
            else
                _EndingsTableCache = new Dictionary<string, LexTable>(StringComparer.OrdinalIgnoreCase);

            lock (_EndingsTableCache)
            {
                if (UseFileBasedEndingsTable)
                {
                    string filePath = ApplicationData.LocalDataPath;

                    filePath = MediaUtilities.ConcatenateFilePath(filePath, "LexTables");
                    filePath = MediaUtilities.ConcatenateFilePath(filePath, LanguageName + "Table.xml");

                    if (FileSingleton.Exists(filePath))
                    {
                        List<List<Designator>> designatorLists = GetAllDesignationsLists();

                        byte[] data = FileSingleton.ReadAllBytes(filePath);

                        _EndingsTable = new LexTable(
                            TargetLanguageIDs,
                            designatorLists,
                            EndingsTableVersion);

                        _EndingsTable.BinaryData = data;

                        if (_EndingsTable.Version != EndingsTableVersion)
                        {
                            FileSingleton.Delete(filePath);
                            _EndingsTable = ComposeEndingsTable();
                            data = _EndingsTable.BinaryData;
                            FileSingleton.WriteAllBytes(filePath, data);
                        }
                    }
                    else
                    {
                        _EndingsTable = ComposeEndingsTable();
                        byte[] data = _EndingsTable.BinaryData;
                        FileSingleton.DirectoryExistsCheck(filePath);
                        FileSingleton.WriteAllBytes(filePath, data);
                    }
                }
                else
                    _EndingsTable = ComposeEndingsTable();

                if (_EndingsTableCache == null)
                    _EndingsTableCache = new Dictionary<string, LexTable>(StringComparer.OrdinalIgnoreCase);

                _EndingsTableCache.Add(LanguageCode, _EndingsTable);
            }
        }

        public virtual LexTable ComposeEndingsTable()
        {
            string source;
            int index = 0;
            int senseIndex = -1;
            int synonymIndex = -1;
            DictionaryEntry dictionaryEntry;
            List<Inflection> inflections;
            LexTable lexTable = new LexTable(
                TargetLanguageIDs,
                GetAllDesignationsLists(),
                EndingsTableVersion);

            if (EndingsTableSource == null)
                EndingsTableSource = GetEndingsTableSource();

            if (EndingsTableSource != null)
            {
                if (ApplicationData.IsMobileVersion &&
                        (ApplicationData.RemoteRepositories != null) &&
                        (DictionaryDatabase != ApplicationData.RemoteRepositories.Dictionary))
                    CheckGraphTableSource();

                int count = EndingsTableSource.Count();

                for (index = 0; (index < count) && ((source = EndingsTableSource[index]) != null); index++)
                {
                    dictionaryEntry = GetDictionaryEntry(source);

                    if (dictionaryEntry == null)
                    {
                        string message = "Error finding endings table source entry during conjugation graph load: " + source;
                        //throw new Exception(message);
                        continue;
                    }

                   // if (source == "ser")
                   //     ApplicationData.Global.PutConsoleMessage("ComposeEndingsTable: " + source);

                    senseIndex = -1;
                    synonymIndex = -1;

                    int senseCount = dictionaryEntry.SenseCount;

                    if (senseCount > 1)
                    {
                        int bestSenseIndex = -1;
                        int bestSynonymIndex = -1;
                        int bestLength = 1000;

                        for (senseIndex = 0; senseIndex < senseCount; senseIndex++)
                        {
                            Sense sense = dictionaryEntry.GetSenseIndexed(senseIndex);

                            if (sense == null)
                                continue;

                            LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(LanguageLookup.English);

                            if (languageSynonyms == null)
                                continue;

                            if (!languageSynonyms.HasProbableSynonyms())
                                continue;

                            int synonymCount = languageSynonyms.ProbableSynonymCount;

                            for (synonymIndex = 0; synonymIndex < synonymCount; synonymIndex++)
                            {
                                ProbableMeaning probableSynonym = languageSynonyms.GetProbableSynonymIndexed(synonymIndex);

                                if (probableSynonym == null)
                                    continue;

                                if (probableSynonym.Meaning.Length < bestLength)
                                {
                                    bestLength = probableSynonym.Meaning.Length;
                                    bestSenseIndex = senseIndex;
                                    bestSynonymIndex = synonymIndex;
                                }
                            }
                        }

                        senseIndex = bestSenseIndex;
                        synonymIndex = bestSynonymIndex;
                    }

                    inflections = InflectAnyDictionaryFormAll(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex);
                    AddInflectionEndingsToGraph(
                        lexTable,
                        inflections);
                }
            }

            return lexTable;
        }

        public virtual List<string> GetEndingsTableSource()
        {
            List<string> endingsTableSource = null;

            if (CanInflect("Verb"))
                LoadEndingsTableSourceFromInflectorTable("Verb", ref endingsTableSource);

            if (CanInflect("Adjective"))
                LoadEndingsTableSourceFromInflectorTable("Adjective", ref endingsTableSource);

            if (CanInflect("Noun"))
                LoadEndingsTableSourceFromInflectorTable("Noun", ref endingsTableSource);

            return endingsTableSource;
        }

        public virtual bool LoadEndingsTableSourceFromInflectorTable(
            string type,
            ref List<string> endingsTableSource)
        {
            bool returnValue = false;

            InflectorTable inflectorTable = InflectorTable(type);

            if ((inflectorTable != null) && (inflectorTable.EndingsSourcesCount() != 0))
            {
                if (endingsTableSource == null)
                    endingsTableSource = new List<string>();

                List<Classifier> endingsSources = inflectorTable.EndingsSources;

                foreach (Classifier endingsSource in endingsSources)
                    endingsTableSource.Add(endingsSource.Text);

                returnValue = true;
            }

            return returnValue;
        }

        public virtual bool CheckGraphTableSource()
        {
            List<string> sourceList = new List<string>();
            string source;
            int index = 0;
            DictionaryEntry dictionaryEntry;
            bool returnValue = false;

            if (EndingsTableSource == null)
                return returnValue;

            for (index = 0; (index < EndingsTableSource.Count()) && ((source = EndingsTableSource[index]) != null); index++)
            {
                dictionaryEntry = GetDictionaryEntry(source);

                if (dictionaryEntry == null)
                {
                    if (sourceList == null)
                        sourceList = new List<string>();

                    sourceList.Add(source);
                }
            }

            if ((sourceList != null) && (sourceList.Count() != 0))
            {
                List<DictionaryEntry> dictionaryEntries;
                List<string> dictionaryKeysNotFound;

                returnValue = NodeUtilities.GetServerVocabularyItems(
                    LanguageID,
                    TargetLanguageIDs,
                    HostLanguageIDs,
                    true,                       // bool translateMissingEntries
                    true,                       // bool synthesizeMissingAudio
                    sourceList,
                    out dictionaryEntries,
                    out dictionaryKeysNotFound);

                if (returnValue)
                    DictionaryCacheFound.AddSingles(Matchers.MatchCode.Exact, dictionaryEntries);
            }

            return returnValue;
        }

        public virtual void AddInflectionEndingsToGraph(
            LexTable lexTable,
            List<Inflection> inflections)
        {
            if ((lexTable == null) || (inflections == null))
                return;

            foreach (Inflection inflection in inflections)
            {
                if (inflection.HasPreWords(TargetLanguageIDs) || inflection.HasPostWords(TargetLanguageIDs))
                    continue;

                string category = inflection.CategoryString;
                Designator designation = inflection.Designation;
                MultiLanguageString endings = inflection.Suffix;
                List<string> endingsUsed = new List<string>();

                if (endings == null)
                    continue;

                foreach (LanguageID languageID in TargetLanguageIDs)
                {
                    string ending = endings.Text(languageID);

                    if (String.IsNullOrEmpty(ending) || endingsUsed.Contains(ending))
                        continue;

                    lexTable.Add(ending, endings, category, designation);
                    endingsUsed.Add(ending);
                }
            }
        }

        public virtual void AddInflectionOutputToGraph(
            LexTable lexTable,
            List<Inflection> inflections)
        {
            if ((lexTable == null) || (inflections == null))
                return;

            foreach (Inflection inflection in inflections)
            {
                string category = inflection.CategoryString;
                Designator designation = inflection.Designation;
                MultiLanguageString output = inflection.Output;
                List<string> outputUsed = new List<string>();

                if (output == null)
                    continue;

                foreach (LanguageID languageID in TargetLanguageIDs)
                {
                    string text = output.Text(languageID);

                    if (String.IsNullOrEmpty(text) || outputUsed.Contains(text))
                        continue;

                    lexTable.Add(text, output, category, designation);
                    outputUsed.Add(text);
                }
            }
        }

        public virtual void AddCompoundInflectionEndingToGraph(
            LexTable lexTable,
            List<Inflection> inflections,
            MultiLanguageString basis)
        {
            if ((lexTable == null) || (inflections == null))
                return;

            MultiLanguageString endings = new MultiLanguageString(null, TargetLanguageIDs);

            foreach (Inflection inflection in inflections)
            {
                string category = inflection.CategoryString;
                Designator designation = inflection.Designation;
                MultiLanguageString output = inflection.Output;

                if (output == null)
                    continue;

                foreach (LanguageID languageID in TargetLanguageIDs)
                {
                    string basisText = basis.Text(languageID);
                    string outputText = output.Text(languageID);
                    int ofs = outputText.IndexOf(basisText);
                    string endingText;

                    if (ofs != -1)
                        endingText = outputText.Substring(ofs);
                    else
                        endingText = inflection.Suffix.Text(languageID);

                    endings.SetText(languageID, endingText);
                }

                List<string> endingsUsed = new List<string>();

                foreach (LanguageID languageID in TargetLanguageIDs)
                {
                    string ending = endings.Text(languageID);

                    if (String.IsNullOrEmpty(ending) || endingsUsed.Contains(ending))
                        continue;

                    lexTable.Add(ending, endings, category, designation);
                    endingsUsed.Add(ending);
                }
            }
        }
    }
}
