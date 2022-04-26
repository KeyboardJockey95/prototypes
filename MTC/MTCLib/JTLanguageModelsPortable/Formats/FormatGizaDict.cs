using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatGizaDict : FormatDictionary
    {
        // Format arguments.
        public string SubFormat { get; set; }       // "Dictionary", "Alignment", "MergedAlignment"
        protected static string SubFormatPrompt = "Subformat";
        protected static string SubFormatHelp = "Enter the subformat.";
        public static List<string> SubFormats = new List<string>() { "Dictionary", "Alignment", "MergedAlignment" };

        public int StartLineNumber { get; set; }
        protected static string StartLineNumberPrompt = "Start line number";
        protected static string StartLineNumberHelp = "Enter the alignment file sentence number to start taking, or -1 for all.";

        public int EndLineNumber { get; set; }
        protected static string EndLineNumberPrompt = "End line number";
        protected static string EndLineNumberHelp = "Enter the alignment file sentence number after which to stop taking, or -1 for all.";

        //public bool IsAddReciprocals { get; set; }
        protected static string IsAddReciprocalsPrompt = "Add reciprocal dictionary entries";
        //protected static string IsAddReciprocalsHelp = "Set this to true to add reciprocal dictionary entries if creating a dictionary.";

        public int CountLimit { get; set; }
        protected static string CountLimitPrompt = "Meaning count limit";
        protected static string CountLimitHelp = "Enter the maximum number of meanings to allow for a dictionary entry.";

        public float ProbabilityThreshold { get; set; }
        protected static string ProbabilityThresholdPrompt = "Probability threshold";
        protected static string ProbabilityThresholdHelp = "Enter the lowest probability value to allow for a dictionary entry.";

        public float ProbabilityThresholdFactor { get; set; }
        protected static string ProbabilityThresholdFactorPrompt = "Probability threshold factor";
        protected static string ProbabilityThresholdFactorHelp = "Enter the relative probability threshold factor. Default 100."
            + " Dictionary entries where the probability is less than the highest found probability divided by this factor are filtered out.";

        //public bool DeleteBeforeImport { get; set; }
        protected static string DeleteBeforeImportPrompt = "Delete before import";
        //protected static string DeleteBeforeImportHelp = "Set this to true to delete prior content before import";

        //public bool IsSynthesizeMissingAudio { get; set; }
        protected static string IsSynthesizeMissingAudioPrompt = "Synthesize missing audio";
        //protected static string IsSynthesizeMissingAudioHelp = "Set this to true to synthesize missing audio";

        //protected string DisplayFilePath { get; set; }
        //protected static string DisplayFilePathPrompt = "Display file path";
        //protected string DisplayFilePathHelp = "Enter the file path for the display format alignment file output.";

        // Implementation data.
        public Dictionary<string, List<ProbableMeaning>> TargetGlossary;
        public Dictionary<string, List<ProbableMeaning>> HostGlossary;

        // Set to true to use dictonary from opposite Giza run.
        // Set to false to only use dictionary from the same run.
        public static bool IsUseCrossDictionary = true;

        private static string FormatDescription = "The GIZA++ word alignment tool dictionary output.  See: https://masatohagiwara.net/using-giza-to-obtain-word-alignment-between-bilingual-sentences.html";

        public FormatGizaDict(
                string name,
                UserRecord userRecord,
                UserProfile userProfile,
                IMainRepository repositories,
                LanguageUtilities languageUtilities)
            : base(
                name,
                "FormatGizaDict",
                FormatDescription,
                "text/plain",
                ".final",
                userRecord,
                userProfile,
                repositories,
                languageUtilities,
                null)
        {
            ClearFormatGizaDict();
        }

        public FormatGizaDict(FormatGizaDict other)
            : base(other)
        {
            CopyFormatGizaDict(other);
        }

        public FormatGizaDict()
            : base(
                "GizaDict",
                "FormatGizaDict",
                FormatDescription,
                "text/plain",
                "final",
                null,
                null,
                null,
                null,
                null)
        {
            ClearFormatGizaDict();
        }

        public void ClearFormatGizaDict()
        {
            SubFormat = "Alignment";
            StartLineNumber = -1;
            EndLineNumber = -1;
            IsAddReciprocals = false;
            CountLimit = 0;
            ProbabilityThreshold = 0.0f;
            ProbabilityThresholdFactor = 100.0f;
            DisplayFilePath = String.Empty;
            TargetGlossary = null;
            HostGlossary = null;
        }

        public void CopyFormatGizaDict(FormatGizaDict other)
        {
            SubFormat = other.SubFormat;
            StartLineNumber = other.StartLineNumber;
            EndLineNumber = other.EndLineNumber;
            IsAddReciprocals = other.IsAddReciprocals;
            CountLimit = other.CountLimit;
            ProbabilityThreshold = other.ProbabilityThreshold;
            ProbabilityThresholdFactor = other.ProbabilityThresholdFactor;
            DisplayFilePath = other.DisplayFilePath;
            TargetGlossary = null;
            HostGlossary = null;
        }

        public override Format Clone()
        {
            return new FormatGizaDict(this);
        }

        public override void LoadFromArguments()
        {
            SubFormat = GetStringListArgumentDefaulted("SubFormat", "stringlist", "rw", SubFormat,
                SubFormats, SubFormatPrompt, SubFormatHelp);

            StartLineNumber = GetIntegerArgumentDefaulted("StartLineNumber", "integer", "r", StartLineNumber,
                StartLineNumberPrompt, StartLineNumberHelp);

            EndLineNumber = GetIntegerArgumentDefaulted("EndLineNumber", "integer", "r", EndLineNumber,
                EndLineNumberPrompt, EndLineNumberHelp);

            IsAddReciprocals = GetFlagArgumentDefaulted("IsAddReciprocals", "flag", "r",
                IsAddReciprocals, IsAddReciprocalsPrompt, IsAddReciprocalsHelp, null, null);

            CountLimit = GetIntegerArgumentDefaulted("CountLimit", "integer", "r",
                CountLimit, CountLimitPrompt, CountLimitHelp);

            ProbabilityThreshold = GetFloatArgumentDefaulted("ProbabilityThreshold", "float", "r",
                ProbabilityThreshold, ProbabilityThresholdPrompt, ProbabilityThresholdHelp);

            ProbabilityThresholdFactor = GetFloatArgumentDefaulted("ProbabilityThresholdFactor", "float", "r",
                ProbabilityThresholdFactor, ProbabilityThresholdFactorPrompt, ProbabilityThresholdFactorHelp);

            DeleteBeforeImport = GetFlagArgumentDefaulted("DeleteBeforeImport", "flag", "r", DeleteBeforeImport,
                DeleteBeforeImportPrompt, DeleteBeforeImportHelp, null, null);

            IsSynthesizeMissingAudio = GetFlagArgumentDefaulted("IsSynthesizeMissingAudio", "flag", "rw", IsSynthesizeMissingAudio,
                IsSynthesizeMissingAudioPrompt, IsSynthesizeMissingAudioHelp, null, null);

            DisplayFilePath = GetArgumentDefaulted("DisplayFilePath", "string", "rw", DisplayFilePath, DisplayFilePathPrompt,
                DisplayFilePathHelp);
        }

        public override void SaveToArguments()
        {
            SetStringListArgument("SubFormat", "stringlist", "rw", SubFormat,
                SubFormats, SubFormatPrompt, SubFormatHelp);

            SetIntegerArgument("StartLineNumber", "integer", "r", StartLineNumber,
                StartLineNumberPrompt, StartLineNumberHelp);

            SetIntegerArgument("EndLineNumber", "integer", "r", EndLineNumber,
                EndLineNumberPrompt, EndLineNumberHelp);

            SetFlagArgument("IsAddReciprocals", "flag", "r",
                IsAddReciprocals, IsAddReciprocalsPrompt, IsAddReciprocalsHelp, null, null);

            SetIntegerArgument("CountLimit", "integer", "r", CountLimit,
                CountLimitPrompt, CountLimitHelp);

            SetFloatArgument(ProbabilityThresholdPrompt, "float", "r", ProbabilityThreshold,
                "Probability threshold", ProbabilityThresholdHelp);

            SetFloatArgument("ProbabilityThresholdFactor", "float", "r",
                ProbabilityThresholdFactor, ProbabilityThresholdFactorPrompt, ProbabilityThresholdFactorHelp);

            SetFlagArgument("DeleteBeforeImport", "flag", "r", DeleteBeforeImport, DeleteBeforeImportPrompt,
                DeleteBeforeImportHelp, null, null);

            SetFlagArgument("IsSynthesizeMissingAudio", "flag", "rw", IsSynthesizeMissingAudio,
                IsSynthesizeMissingAudioPrompt, IsSynthesizeMissingAudioHelp, null, null);

            SetArgument("DisplayFilePath", "string", "rw", DisplayFilePath, DisplayFilePathPrompt,
                DisplayFilePathHelp, null, null);
        }

        public override void Read(Stream stream)
        {
            try
            {
                PreRead(1);
                InitializeGlossary();
                FileSize = (int)stream.Length;

                UpdateProgressElapsed("Reading stream ...");

                using (StreamReader reader = new StreamReader(stream))
                {
                    State = StateCode.Reading;

                    if (SubFormat == "Dictionary")
                    {
                        string line;

                        while ((line = reader.ReadLine()) != null)
                            ReadLine(line);
                    }
                    else if (SubFormat == "Alignment")
                    {
                        string infoLine;
                        string textLine;
                        string alignmentLine;

                        while ((infoLine = reader.ReadLine()) != null)
                        {
                            if (!infoLine.StartsWith("#"))
                                throw new Exception("Info line didn't start with '#': " + infoLine);

                            textLine = reader.ReadLine().Trim();

                            if (String.IsNullOrEmpty(textLine))
                                throw new Exception("Empty text line.");

                            alignmentLine = reader.ReadLine();

                            if (String.IsNullOrEmpty(alignmentLine))
                                throw new Exception("Empty alignment line.");

                            ScanAlignment(infoLine, textLine, alignmentLine, TargetGlossary, HostGlossary);
                        }
                    }
                    else if (SubFormat == "MergedAlignment")
                    {
                        string targetInfoLine;
                        string targetTextLine;
                        string targetAlignmentLine;
                        string targetPrettyLine;
                        string hostInfoLine;
                        string hostTextLine;
                        string hostAlignmentLine;
                        string hostPrettyLine;
                        string emptyLine;

                        while ((targetInfoLine = reader.ReadLine()) != null)
                        {
                            if (!targetInfoLine.StartsWith("#"))
                                throw new Exception("Target info line didn't start with '#': " + targetInfoLine);

                            hostInfoLine = reader.ReadLine().Trim();

                            if (String.IsNullOrEmpty(hostInfoLine))
                                throw new Exception("Empty host info line.");

                            targetTextLine = reader.ReadLine().Trim();

                            if (String.IsNullOrEmpty(targetTextLine))
                                throw new Exception("Empty target text line.");

                            hostTextLine = reader.ReadLine().Trim();

                            if (String.IsNullOrEmpty(hostTextLine))
                                throw new Exception("Empty host text line.");

                            targetAlignmentLine = reader.ReadLine();

                            if (String.IsNullOrEmpty(targetAlignmentLine))
                                throw new Exception("Empty target alignment line.");

                            targetPrettyLine = reader.ReadLine();

                            if (String.IsNullOrEmpty(targetPrettyLine))
                                throw new Exception("Empty target pretty line.");

                            hostAlignmentLine = reader.ReadLine();

                            if (String.IsNullOrEmpty(hostAlignmentLine))
                                throw new Exception("Empty host alignment line.");

                            hostPrettyLine = reader.ReadLine();

                            if (String.IsNullOrEmpty(hostPrettyLine))
                                throw new Exception("Empty host pretty line.");

                            emptyLine = reader.ReadLine();

                            if (!String.IsNullOrEmpty(emptyLine))
                                throw new Exception("Expected empty line.");

                            ScanAlignment(targetInfoLine, targetTextLine, targetAlignmentLine, TargetGlossary, HostGlossary);
                            ScanAlignment(hostInfoLine, hostTextLine, hostAlignmentLine, HostGlossary, TargetGlossary);
                        }
                    }
                    else
                        throw new Exception("Unexpected subformat: " + SubFormat);

                    SaveGlossaries();
                    WriteDictionary();
                    WriteDictionaryDisplayOutput();
                    SynthesizeMissingAudio();
                }
            }
            catch (Exception exc)
            {
                string msg = exc.Message;

                if (exc.InnerException != null)
                    msg += ": " + exc.InnerException.Message;

                PutError(msg);
            }
            finally
            {
                PostRead();
            }
        }

        protected override void DispatchLine(string line)
        {
            line = line.Trim();

            if (line.Length != 0)
                ReadEntry(line);
        }

        protected override void ReadEntry(string line)
        {
            line = line.Trim();

            if (String.IsNullOrEmpty(line))
                return;

            string[] parts = line.Split(LanguageLookup.Space);

            if (parts.Length != 3)
                throw new Exception("Didn't get three parts in dictionary line: " + line);

            string hostWord;
            string targetWord;

            if (IsUseCrossDictionary)
            {
                hostWord = parts[1].Replace(LanguageLookup.NonBreakSpace, ' ');
                targetWord = parts[0].Replace(LanguageLookup.NonBreakSpace, ' ');
            }
            else
            {
                hostWord = parts[0].Replace(LanguageLookup.NonBreakSpace, ' ');
                targetWord = parts[1].Replace(LanguageLookup.NonBreakSpace, ' ');
            }

            if (String.IsNullOrEmpty(hostWord) || IsPunctuation(hostWord) || (hostWord == "NULL"))
                return;

            string probabilityString = parts[2];
            float probability = ObjectUtilities.GetFloatFromString(probabilityString, float.NaN);
            int frequency = 0;
            ProbableMeaning meaning;
            List<ProbableMeaning> meanings;

            //if (((targetWord == "y") && (hostWord == "and")) || ((targetWord == "and") && (hostWord == "y")))
            //    DumpString("y and here");

            if (TargetGlossary.TryGetValue(targetWord, out meanings))
            {
                meaning = meanings.FirstOrDefault(x => x.Meaning == hostWord);

                if (meaning != null)
                {
                    meaning.Probability = probability;
                    meaning.MergeSourceIDs(GizaDictionarySourceIDList);
                }
                else
                {
                    meaning = new ProbableMeaning(
                        hostWord,
                        LexicalCategory.Unknown,
                        null,
                        probability,
                        0,
                        GizaDictionarySourceIDList);
                    meanings.Add(meaning);
                }
            }
            else
                TargetGlossary.Add(
                    targetWord,
                    new List<ProbableMeaning>()
                    {
                        new ProbableMeaning(
                            hostWord,
                            LexicalCategory.Unknown,
                            null,
                            probability,
                            frequency,
                            GizaDictionarySourceIDList)
                    });

            if (IsAddReciprocals)
            {
                if (HostGlossary.TryGetValue(hostWord, out meanings))
                {
                    meaning = meanings.FirstOrDefault(x => x.Meaning == targetWord);

                    if (meaning != null)
                        meaning.MergeSourceIDs(GizaDictionarySourceIDList);
                    else
                    {
                        meaning = new ProbableMeaning(
                            targetWord,
                            LexicalCategory.Unknown,
                            null,
                            probability,
                            0,
                            GizaDictionarySourceIDList);
                        meanings.Add(meaning);
                    }
                }
                else
                    HostGlossary.Add(
                        hostWord,
                        new List<ProbableMeaning>()
                        {
                            new ProbableMeaning(
                                targetWord,
                                LexicalCategory.Unknown,
                                null,
                                probability,
                                0,
                                GizaDictionarySourceIDList)
                        });
            }
        }

        protected void ScanAlignment(
            string infoLine,
            string textLine,
            string alignmentLine,
            Dictionary<string, List<ProbableMeaning>> targetGlossary,
            Dictionary<string, List<ProbableMeaning>> hostGlossary)
        {
            int studyItemIndex = ScanStudyItemIndex(infoLine);

            if ((StartLineNumber > 0) && (studyItemIndex < (StartLineNumber - 1)))
                return;

            if ((EndLineNumber > 0) && (studyItemIndex > (EndLineNumber - 1)))
                return;

            string[] hostWords = textLine.Split(LanguageLookup.Space);
            string[] alignParts = alignmentLine.Split(LanguageLookup.Space);
            int alignCount = alignParts.Length;
            int alignIndex;
            StringBuilder sb = new StringBuilder();
            List<string> hostWordList = new List<string>();
            List<int> hostWordNumbers = new List<int>();

            for (alignIndex = 0; alignIndex < alignCount;)
            {
                string targetWord = alignParts[alignIndex++];

                if (alignIndex >= alignCount)
                    break;

                targetWord = targetWord.Replace(LanguageLookup.NonBreakSpace, ' ');

                bool isWord = (!String.IsNullOrEmpty(targetWord)
                    && !IsPunctuation(targetWord)
                    && (targetWord != "NULL"));

                if (alignParts[alignIndex++] != "({")
                    throw new Exception("Expected \"({\" token.");

                //if (targetWord == "prepararles")
                //    DumpString("prepararles");

                hostWordList.Clear();
                hostWordNumbers.Clear();

                for (; alignIndex < alignCount;)
                {
                    if (alignParts[alignIndex] == "})")
                    {
                        alignIndex++;
                        break;
                    }

                    string hostWordNumberString = alignParts[alignIndex++];
                    int hostWordNumber = int.Parse(hostWordNumberString);

                    if (isWord)
                    {
                        if (hostWordNumber > hostWords.Length)
                            throw new Exception("Host word number bigger than host word array count.");

                        string hostWord = hostWords[hostWordNumber - 1].Trim();

                        if (!String.IsNullOrEmpty(hostWord) && !IsPunctuation(hostWord))
                        {
                            hostWord = hostWord.Replace(LanguageLookup.NonBreakSpace, ' ');
                            hostWordList.Add(hostWord);
                            hostWordNumbers.Add(hostWordNumber);
                        }
                    }
                }

                if (isWord && (hostWordList.Count() != 0))
                {
                    int startIndex;
                    int endIndex = 0;

                    for (startIndex = 0; startIndex < hostWordList.Count(); startIndex = endIndex)
                    {
                        string hostWord = hostWordList[startIndex];

                        if (IsPunctuation(hostWord))
                            continue;

                        sb.Clear();
                        sb.Append(hostWord);

                        for (endIndex = startIndex + 1; endIndex < hostWordList.Count(); endIndex++)
                        {
                            if (hostWordNumbers[endIndex - 1] != hostWordNumbers[endIndex] - 1)
                                break;

                            if (hostWordList[endIndex - 1] == hostWordList[endIndex])
                                break;

                            hostWord = hostWordList[endIndex];

                            // FIXME: Fix this to handle non-space languages.
                            if (sb.Length != 0)
                                sb.Append(" ");

                            sb.Append(hostWord);
                        }

                        string hostWordOrPhrase = sb.ToString();
                        List<ProbableMeaning> meanings;
                        ProbableMeaning meaning;

                        if (hostWordOrPhrase == "prepare accomplish thing commandeth")
                            DumpString("prepare accomplish thing commandeth");

                        //if (((targetWord == "y") && (hostWordOrPhrase == "and")) || ((targetWord == "and") && (hostWordOrPhrase == "y")))
                        //    DumpString("y and here for alignment");

                        if (targetGlossary.TryGetValue(targetWord, out meanings))
                        {
                            meaning = meanings.FirstOrDefault(x => x.Meaning == hostWordOrPhrase);

                            if (meaning != null)
                            {
                                meaning.Frequency += 1;
                                meaning.MergeSourceIDs(GizaAlignmentSourceIDList);
                            }
                            else
                            {
                                meaning = new ProbableMeaning(
                                    hostWordOrPhrase,
                                    LexicalCategory.Unknown,
                                    null,
                                    float.NaN,
                                    1,
                                    GizaAlignmentSourceIDList);
                                meanings.Add(meaning);
                            }
                        }
                        else
                        {
                            meaning = new ProbableMeaning(
                                hostWordOrPhrase,
                                LexicalCategory.Unknown,
                                null,
                                float.NaN,
                                1,
                                GizaAlignmentSourceIDList);
                            meanings = new List<ProbableMeaning>() { meaning };
                            targetGlossary.Add(targetWord, meanings);
                        }

                        if (IsAddReciprocals)
                        {
                            if (hostGlossary.TryGetValue(hostWordOrPhrase, out meanings))
                            {
                                meaning = meanings.FirstOrDefault(x => x.Meaning == targetWord);

                                if (meaning != null)
                                {
                                    meaning.Frequency += 1;
                                    meaning.MergeSourceIDs(GizaAlignmentSourceIDList);
                                }
                                else
                                {
                                    meaning = new ProbableMeaning(
                                        targetWord,
                                        LexicalCategory.Unknown,
                                        null,
                                        float.NaN,
                                        1,
                                        GizaAlignmentSourceIDList);
                                    meanings.Add(meaning);
                                }
                            }
                            else
                            {
                                meaning = new ProbableMeaning(
                                    targetWord,
                                    LexicalCategory.Unknown,
                                    null,
                                    float.NaN,
                                    1,
                                    GizaAlignmentSourceIDList);
                                meanings = new List<ProbableMeaning>() { meaning };
                                hostGlossary.Add(hostWordOrPhrase, meanings);
                            }
                        }
                    }
                }
            }
        }

        protected int ScanStudyItemIndex(string infoLine)
        {
            string[] parts = infoLine.Split(LanguageLookup.ParenthesisCharacters);

            if (parts.Length != 3)
                throw new Exception("Info line format error: " + infoLine);

            int studyItemIndex = ObjectUtilities.GetIntegerFromString(parts[1], -1);

            return studyItemIndex;
        }

        protected List<int> ScanWordNumberToRunIndexMap(string[] hostParts)
        {
            int runIndex = 0;
            List<int> wordNumberToRunIndexMap = new List<int>();

            foreach (string part in hostParts)
            {
                if (!String.IsNullOrEmpty(part) && !IsPunctuation(part))
                    wordNumberToRunIndexMap.Add(runIndex++);
                else
                    wordNumberToRunIndexMap.Add(-1);
            }

            return wordNumberToRunIndexMap;
        }

        protected bool IsPunctuation(char c)
        {
            return char.IsPunctuation(c) || LanguageLookup.PunctuationCharacters.Contains(c);
        }

        protected bool IsPunctuation(string s)
        {
            foreach (char c in s)
            {
                if (!char.IsPunctuation(c) && !LanguageLookup.PunctuationCharacters.Contains(c))
                    return false;
            }

            return true;
        }

        protected bool IsWhiteSpace(char c)
        {
            return char.IsWhiteSpace(c) || LanguageLookup.SpaceCharacters.Contains(c);
        }

        protected void InitializeGlossary()
        {
            TargetGlossary = new Dictionary<string, List<ProbableMeaning>>(StringComparer.OrdinalIgnoreCase);
            HostGlossary = new Dictionary<string, List<ProbableMeaning>>(StringComparer.OrdinalIgnoreCase);
        }

        protected void SaveGlossaries()
        {
            foreach (KeyValuePair<string, List<ProbableMeaning>> kvp in TargetGlossary)
            {
                //if ((kvp.Key == "que") || (kvp.Key == "which"))
                //    DumpString("AddTargetMeanings here");

                AddTargetMeanings(kvp.Key, kvp.Value);
            }

            if (IsAddReciprocals || (SubFormat == "MergedAlignment"))
            {
                foreach (KeyValuePair<string, List<ProbableMeaning>> kvp in HostGlossary)
                    AddHostMeanings(kvp.Key, kvp.Value);
            }
        }

        protected void AddTargetMeanings(
            string targetWord,
            List<ProbableMeaning> hostMeanings)
        {
            List<string> targetMeanings = new List<string>() { targetWord };
            List<Sense> hostSenses = new List<Sense>();
            DictionaryEntry dictionaryEntry;

            SetFrequencyProbability(hostMeanings);

            FilterEntries(targetWord, hostMeanings);

            foreach (ProbableMeaning meaning in hostMeanings)
            {
                ProbableMeaning probableMeaning = new ProbableMeaning(meaning);
                List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>(1) { probableMeaning };
                LanguageSynonyms languageSynonyms = new LanguageSynonyms(
                    HostLanguageID,
                    probableSynonyms);
                List<LanguageSynonyms> languageSynonymsList = new List<LanguageSynonyms>(1) { languageSynonyms };
                Sense sense = new Sense(
                    0,
                    LexicalCategory.Unknown,
                    null,
                    0,
                    languageSynonymsList,
                    null);
                hostSenses.Add(sense);
            }

            List<int> sourceIDs = null;

            if ((hostMeanings != null) && (hostMeanings.Count() != 0))
                sourceIDs = hostMeanings[0].SourceIDs;

            AddTargetEntry(
                targetWord,
                TargetLanguageID,
                targetMeanings,
                sourceIDs,
                hostSenses,
                out dictionaryEntry);
        }

        protected void AddHostMeanings(
            string hostWord,
            List<ProbableMeaning> targetMeanings)
        {
            List<string> hostMeanings = new List<string>() { hostWord };
            List<Sense> targetSenses = new List<Sense>();
            DictionaryEntry dictionaryEntry;

            SetFrequencyProbability(targetMeanings);

            FilterEntries(hostWord, targetMeanings);

            foreach (ProbableMeaning meaning in targetMeanings)
            {
                ProbableMeaning probableMeaning = new ProbableMeaning(meaning);
                List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>(1) { probableMeaning };
                LanguageSynonyms languageSynonyms = new LanguageSynonyms(
                    TargetLanguageID,
                    probableSynonyms);
                List<LanguageSynonyms> languageSynonymsList = new List<LanguageSynonyms>(1) { languageSynonyms };
                Sense sense = new Sense(
                    0,
                    LexicalCategory.Unknown,
                    null,
                    0,
                    languageSynonymsList,
                    null);
                targetSenses.Add(sense);
            }

            List<int> sourceIDs = null;

            if ((targetMeanings != null) && (targetMeanings.Count() != 0))
                sourceIDs = targetMeanings[0].SourceIDs;

            AddHostEntry(
                hostWord,
                HostLanguageID,
                hostMeanings,
                sourceIDs,
                targetSenses,
                out dictionaryEntry);
        }

        public void SetFrequencyProbability(List<ProbableMeaning> meanings)
        {
            if (meanings == null)
                return;

            int frequencySum = 0;

            foreach (ProbableMeaning meaning in meanings)
                frequencySum += meaning.Frequency;

            if (frequencySum == 0)
                return;

            foreach (ProbableMeaning meaning in meanings)
            {
                if (meaning.Frequency != 0)
                    meaning.Probability = (float)meaning.Frequency / frequencySum;
            }
        }

        protected static ProbableMeaningComparer DefaultProbableMeaningComparer = new ProbableMeaningComparer();

        protected void FilterEntries(string word, List<ProbableMeaning> meanings)
        {
            int count = meanings.Count();
            int index;

            meanings.Sort(DefaultProbableMeaningComparer);

            float maxProbability = 0.0f;

            foreach (ProbableMeaning aMeaning in meanings)
            {
                if (!float.IsNaN(aMeaning.Probability))
                {
                    if (aMeaning.Probability > maxProbability)
                        maxProbability = aMeaning.Probability;

                    //if ((word == "da") && (aMeaning.Meaning == "give"))
                    //    DumpString("da/give");
                    //else if ((word == "give") && (aMeaning.Meaning == "da"))
                    //    DumpString("give/da");
                }
            }

            if (maxProbability != 0.0f)
            {
                float minProbability = maxProbability / ProbabilityThresholdFactor;

                if (minProbability != 0)
                {
                    for (index = count - 1; index != 0; index--)
                    {
                        ProbableMeaning meaning = meanings[index];

                        if (!float.IsNaN(meaning.Probability))
                        {
                            //if ((word == "da") && (meaning.Meaning == "give"))
                            //    DumpString("da/give");
                            //else if ((word == "give") && (meaning.Meaning == "da"))
                            //    DumpString("give/da");

                            if (meaning.Probability <= minProbability)
                                meanings.RemoveAt(index);
                        }
                    }

                    count = meanings.Count();
                }
            }

            if ((CountLimit != 0) && (count > CountLimit))
            {
                meanings.RemoveRange(CountLimit, count - CountLimit);
                count = meanings.Count();
            }

            if (ProbabilityThreshold > 0.0f)
            {
                for (index = count - 1; index != 0; index--)
                {
                    ProbableMeaning meaning = meanings[index];

                    //if ((word == "da") && (meaning.Meaning == "give"))
                    //    DumpString("da/give");
                    //else if ((word == "give") && (meaning.Meaning == "da"))
                    //    DumpString("give/da");

                    if (!float.IsNaN(meaning.Probability))
                    {
                        if (meaning.Probability < ProbabilityThreshold)
                            meanings.RemoveAt(index);
                    }
                }
            }
        }

        public static string GizaAlignmentSourceName = "Giza Alignment";
        public static string GizaDictionarySourceName = "Giza Dictionary";

        protected static int _GizaAlignmentSourceID = 0;
        public static int GizaAlignmentSourceID
        {
            get
            {
                if (_GizaAlignmentSourceID == 0)
                    _GizaAlignmentSourceID = ApplicationData.DictionarySourcesLazy.Add(GizaAlignmentSourceName);

                return _GizaAlignmentSourceID;
            }
        }

        protected static int _GizaDictionarySourceID = 0;
        public static int GizaDictionarySourceID
        {
            get
            {
                if (_GizaDictionarySourceID == 0)
                    _GizaDictionarySourceID = ApplicationData.DictionarySourcesLazy.Add(GizaDictionarySourceName);

                return _GizaDictionarySourceID;
            }
        }

        protected static List<int> _GizaAlignmentSourceIDList = null;
        public static List<int> GizaAlignmentSourceIDList
        {
            get
            {
                if (_GizaAlignmentSourceIDList == null)
                    _GizaAlignmentSourceIDList = new List<int>(1) { GizaAlignmentSourceID };

                return _GizaAlignmentSourceIDList;
            }
        }

        protected static List<int> _GizaDictionarySourceIDList = null;
        public static List<int> GizaDictionarySourceIDList
        {
            get
            {
                if (_GizaDictionarySourceIDList == null)
                    _GizaDictionarySourceIDList = new List<int>(1) { GizaDictionarySourceID };

                return _GizaDictionarySourceIDList;
            }
        }

        public static new string TypeStringStatic { get { return "GizaDict"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
